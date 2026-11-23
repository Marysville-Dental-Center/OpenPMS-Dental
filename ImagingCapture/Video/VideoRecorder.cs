// This file is part of the open source dental practice management software derived from Open Dental.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Threading;
using ImagingCapture.Uvc;

namespace ImagingCapture.Video {
	///<summary>What a recording made.</summary>
	public sealed class VideoRecordingResult {
		public long FramesWritten { get; set; }
		///<summary>Frames the encoder could not keep up with (left out; the video plays on).</summary>
		public long FramesDropped { get; set; }
		public TimeSpan Duration { get; set; }
		///<summary>"" or why the file is incomplete or missing.</summary>
		public string Error { get; set; }="";
		public bool IsLimitReached { get; set; }
		///<summary>True when the recording thread had not finished the file when Stop gave up waiting: the file may still be open and incomplete.
		///</summary>
		public bool IsTimedOut { get; set; }
		///<summary>A copy of the first frame written, for the picture of the video; null when none was written.</summary>
		public UvcFrame FrameFirst { get; set; }
	}

	///<summary>Records the camera's preview frames to a video file (IMG-P4c; design section 4). The camera's streaming thread offers each frame
	///(OfferFrame copies it into a pooled buffer and returns at once; when the encoder falls behind, frames are dropped, never the stream
	///held up); a thread of its own writes them with an IVideoWriter, made at the first frame when the frame size is known. Timestamps come
	///from the frames' stream times, with paused time taken out. Stops accepting frames at the limit (2 minutes by default, decision C5);
	///the window then calls Stop.</summary>
	public sealed class VideoRecorder:IDisposable {
		public const int LimitSecondsDefault=120;
		///<summary>The warning before the limit (design: "warns at 1:45").</summary>
		public const int WarnBeforeLimitSeconds=15;
		private const int QueueCapacity=6;

		private readonly Func<int,int,IVideoWriter> _funcCreateWriter;
		private readonly long _frameDuration100ns;
		private readonly int _limitSeconds;
		private readonly object _lock=new object();
		private readonly Queue<Item> _queue=new Queue<Item>();
		private readonly Stack<byte[]> _stackBuffers=new Stack<byte[]>();
		private readonly AutoResetEvent _autoResetEvent=new AutoResetEvent(false);
		private Thread _thread;
		private IVideoWriter _writer;
		private bool _isRecording;
		private bool _isStopRequested;
		private bool _isPaused;
		private bool _isResumePending;
		private bool _isLimitReached;
		private double _sampleTimeStart=double.NaN;
		private double _sampleTimeLast=double.NaN;
		///<summary>Seconds taken out of the timeline: pauses.</summary>
		private double _secondsPaused;
		private long _timestampLast100ns=-1;
		private long _framesWritten;
		private long _framesDropped;
		private string _error="";
		private UvcFrame _frameFirst;
		private bool _isTimedOut;
		private int _widthFirst;
		private int _heightFirst;

		private sealed class Item {
			public byte[] Pixels;
			public int Width;
			public int Height;
			public int Stride;
			public long Timestamp100ns;
		}

		///<summary>funcCreateWriter(width,height) opens the file on the recording thread. framesPerSecond: the camera's rate (30 when unknown).
		///</summary>
		public VideoRecorder(Func<int,int,IVideoWriter> funcCreateWriter,double framesPerSecond,int limitSeconds=LimitSecondsDefault) {
			_funcCreateWriter=funcCreateWriter??throw new ArgumentNullException(nameof(funcCreateWriter));
			double fps=framesPerSecond>0 && framesPerSecond<=240 ? framesPerSecond : 30;
			_frameDuration100ns=(long)Math.Round(10000000d/fps);
			_limitSeconds=limitSeconds>0 ? limitSeconds : LimitSecondsDefault;
		}

		public bool IsRecording { get { lock(_lock) { return _isRecording; } } }
		public bool IsPaused { get { lock(_lock) { return _isPaused; } } }
		public bool IsLimitReached { get { lock(_lock) { return _isLimitReached; } } }
		///<summary>True when the writer failed (no encoder, disk full): nothing more is recorded; the window stops and says why.</summary>
		public bool IsFailed { get { lock(_lock) { return _error!=""; } } }
		public int LimitSeconds => _limitSeconds;

		///<summary>The recorded time so far (pauses left out).</summary>
		public TimeSpan Elapsed {
			get {
				lock(_lock) {
					return _timestampLast100ns<0 ? TimeSpan.Zero : TimeSpan.FromTicks(_timestampLast100ns+_frameDuration100ns);
				}
			}
		}

		///<summary>True from WarnBeforeLimitSeconds before the limit.</summary>
		public bool IsNearLimit => Elapsed.TotalSeconds>=_limitSeconds-WarnBeforeLimitSeconds;

		public void Start() {
			lock(_lock) {
				if(_isRecording || _thread!=null) {
					throw new InvalidOperationException("Already recording.");
				}
				_isRecording=true;
			}
			_thread=new Thread(RunWriter) { IsBackground=true,Name="Camera video recording" };//MTA: the sink writer is free-threaded
			_thread.Start();
		}

		public void Pause() {
			lock(_lock) {
				if(_isRecording && !_isPaused) {
					_isPaused=true;
				}
			}
		}

		public void Resume() {
			lock(_lock) {
				if(_isRecording && _isPaused) {
					_isPaused=false;
					_isResumePending=true;//the next frame's gap since the last one is taken out
				}
			}
		}

		///<summary>The camera's streaming thread: copies the frame for the recording thread, or drops it when that thread is behind. Never blocks
		///and never throws.</summary>
		public void OfferFrame(UvcFrame frame) {
			if(frame==null) {
				return;
			}
			try {
				Item item;
				lock(_lock) {
					if(!_isRecording || _isStopRequested || _isPaused || _isLimitReached || _error!="") {
						return;
					}
					double sampleTime=frame.SampleTime;
					if(double.IsNaN(_sampleTimeStart)) {
						_sampleTimeStart=sampleTime;
						_sampleTimeLast=sampleTime;
					}
					if(_isResumePending) {
						_isResumePending=false;
						double gap=sampleTime-_sampleTimeLast-_frameDuration100ns/1e7;
						if(gap>0) {
							_secondsPaused+=gap;
						}
					}
					long timestamp=(long)Math.Round((sampleTime-_sampleTimeStart-_secondsPaused)*1e7);
					if(timestamp<=_timestampLast100ns) {
						timestamp=_timestampLast100ns+_frameDuration100ns;//a stream without a clock, or times out of order
					}
					if(timestamp>=_limitSeconds*10000000L) {
						_isLimitReached=true;
						return;
					}
					_sampleTimeLast=sampleTime;
					if(_queue.Count>=QueueCapacity) {
						_framesDropped++;
						return;
					}
					int length=frame.Stride*frame.Height;
					byte[] pixels=_stackBuffers.Count>0 && _stackBuffers.Peek().Length==length ? _stackBuffers.Pop() : new byte[length];
					item=new Item { Pixels=pixels,Width=frame.Width,Height=frame.Height,Stride=frame.Stride,Timestamp100ns=timestamp };
					_timestampLast100ns=timestamp;
					if(_frameFirst==null) {
						_frameFirst=frame.Clone();
					}
				}
				Buffer.BlockCopy(frame.Pixels,0,item.Pixels,0,item.Pixels.Length);
				lock(_lock) {
					_queue.Enqueue(item);
				}
				_autoResetEvent.Set();
			}
			catch(Exception ex) {
				lock(_lock) {
					_error=_error!="" ? _error : "A frame could not be recorded: "+ex.Message;
				}
			}
		}

		///<summary>Stops: the frames waiting are written, the file is completed. Safe to call more than once.</summary>
		public VideoRecordingResult Stop() {
			Thread thread;
			lock(_lock) {
				_isStopRequested=true;
				thread=_thread;
			}
			_autoResetEvent.Set();
			bool isWaited;
			lock(_lock) {
				isWaited=_isTimedOut;
			}
			if(thread!=null && thread.IsAlive && !isWaited && !thread.Join(TimeSpan.FromSeconds(20))) {
				lock(_lock) {
					_isTimedOut=true;
					_error=_error!="" ? _error : "The recording did not finish in time.";
				}
			}
			lock(_lock) {
				_isRecording=false;
				return new VideoRecordingResult {
					FramesWritten=_framesWritten,
					FramesDropped=_framesDropped,
					Duration=_timestampLast100ns<0 ? TimeSpan.Zero : TimeSpan.FromTicks(_timestampLast100ns+_frameDuration100ns),
					Error=_error,
					IsLimitReached=_isLimitReached,
					IsTimedOut=_isTimedOut,
					FrameFirst=_frameFirst,
				};
			}
		}

		private void RunWriter() {
			try {
				while(true) {
					Item item=null;
					bool isStop;
					lock(_lock) {
						if(_queue.Count>0) {
							item=_queue.Dequeue();
						}
						isStop=_isStopRequested;
					}
					if(item==null) {
						if(isStop) {
							break;
						}
						_autoResetEvent.WaitOne(500);
						continue;
					}
					Write(item);
				}
				if(_writer!=null) {
					_writer.Finish();
				}
			}
			catch(Exception ex) {
				lock(_lock) {
					_error=_error!="" ? _error : ex.Message;
				}
			}
			finally {
				try {
					_writer?.Dispose();
				}
				catch(Exception exDispose) {
					lock(_lock) {
						_error=_error!="" ? _error : exDispose.Message;
					}
				}
				_writer=null;
			}
		}

		private void Write(Item item) {
			bool isFailed;
			lock(_lock) {
				isFailed=_error!="";
			}
			if(!isFailed) {
				if(_writer==null) {
					_writer=_funcCreateWriter(item.Width,item.Height);
					_widthFirst=item.Width;
					_heightFirst=item.Height;
				}
				else if(item.Width!=_widthFirst || item.Height!=_heightFirst) {
					throw new InvalidOperationException("The picture size changed while recording; the video ends here.");
				}
				_writer.WriteFrame(item.Pixels,item.Stride,item.Timestamp100ns,_frameDuration100ns);
				lock(_lock) {
					_framesWritten++;
				}
			}
			lock(_lock) {
				if(_stackBuffers.Count<QueueCapacity+1) {
					_stackBuffers.Push(item.Pixels);
				}
			}
		}

		public void Dispose() {
			Stop();//waits only when the thread is still running and was not waited for yet
			if(_thread==null || !_thread.IsAlive) {
				_autoResetEvent.Dispose();//else the late thread still uses it; the GC takes it
			}
		}
	}
}
