using System;
using System.Threading;

namespace CodeBase {
	///<summary>The cancellation of the action that a thread runs for a progress window, held per thread (net10-plan, S3a-1b). The progress window
	///(FrmProgressAuto) sets its token on its worker thread with Use; ODThread.RunParallel passes it on to its pool threads. Thread.Abort is gone
	///(it throws PlatformNotSupportedException on .NET 5 and later), so after a cancel an action stops where it checks: the database layer
	///(DataConnection) and the Middle Tier client (RemotingClient) call ThrowIfCancellationRequested before every command or request, so no
	///database write starts on such a thread after the user cancelled; loops that write files or call a vendor check it before each write.
	///A thread that never had a token set (the main thread, the Middle Tier server's threads, every ODThread other than RunParallel's) has
	///CancellationToken.None and is never stopped by this class.</summary>
	public static class ODCancellation {
		///<summary>The token of the action this thread runs, or CancellationToken.None.</summary>
		[ThreadStatic]
		private static CancellationToken _cancellationToken;
		///<summary>How many RunUncancellable calls this thread is inside.</summary>
		[ThreadStatic]
		private static int _countUncancellable;
		///<summary>The Logger sub-directory of the lines written by Log.</summary>
		public const string LOG_SUB_DIRECTORY="Cancellation";

		///<summary>Raised with every line written by Log, after it went to the log file. For tests and diagnostics; never raised for the UI.</summary>
		public static event Action<string> LineLogged;

		///<summary>The token of the action this thread runs, or CancellationToken.None. Inside RunUncancellable still the same token; use
		///IsCancellationRequested or ThrowIfCancellationRequested to check it.</summary>
		public static CancellationToken Current {
			get {
				return _cancellationToken;
			}
		}

		///<summary>True while this thread runs a unit of work passed to RunUncancellable.</summary>
		public static bool IsInUncancellableUnit {
			get {
				return _countUncancellable>0;
			}
		}

		///<summary>True if the user cancelled the action this thread runs and the thread is not inside RunUncancellable.</summary>
		public static bool IsCancellationRequested {
			get {
				return _countUncancellable==0 && _cancellationToken.IsCancellationRequested;
			}
		}

		///<summary>Throws OperationCanceledException if the user cancelled the action this thread runs, unless the thread is inside
		///RunUncancellable. Call it before each write step (a row, a batch, a file, a vendor call) that must not start after a cancel.</summary>
		public static void ThrowIfCancellationRequested() {
			if(IsCancellationRequested) {
				throw new OperationCanceledException(_cancellationToken);
			}
		}

		///<summary>Sets the token for this thread until the returned object is disposed, which puts back the token the thread had before.</summary>
		public static IDisposable Use(CancellationToken cancellationToken) {
			CancellationTokenScope cancellationTokenScope=new CancellationTokenScope(_cancellationToken,_countUncancellable);
			_cancellationToken=cancellationToken;
			_countUncancellable=0;
			return cancellationTokenScope;
		}

		///<summary>Runs action as one unit that a cancel does not split: inside it ThrowIfCancellationRequested does nothing, so the database layer
		///and progress events let the unit finish, also after the user cancelled. For a write and what belongs to it (its security log entry, the
		///claim that records what a vendor was sent), when the stop is to come before or after the unit, never in its middle. The next check after
		///the unit stops the action.</summary>
		public static void RunUncancellable(Action action) {
			_countUncancellable++;
			try {
				action();
			}
			finally {
				_countUncancellable--;
			}
		}

		///<summary>The token for the threads that this thread starts to share its work (ODThread.RunParallel): CancellationToken.None inside
		///RunUncancellable, so that a unit's own pool runs to its end too.</summary>
		public static CancellationToken GetTokenForChildThreads() {
			if(_countUncancellable>0) {
				return CancellationToken.None;
			}
			return _cancellationToken;
		}

		///<summary>Writes one line to the log (Logger, sub-directory LOG_SUB_DIRECTORY) and raises LineLogged. Never throws and shows nothing: for
		///what happens to an action after its window has gone (an exception after a cancel, a worker still running, a thread after a timed-out
		///Join).</summary>
		public static void Log(string line) {
			try {
				Logger.WriteLine(line,LOG_SUB_DIRECTORY);
			}
			catch(Exception ex) {
				ex.DoNothing();//The log folder may not be writable. Logging never stops the program.
			}
			try {
				LineLogged?.Invoke(line);
			}
			catch(Exception ex) {
				ex.DoNothing();
			}
		}

		///<summary>Puts back the token a thread had before Use.</summary>
		private class CancellationTokenScope:IDisposable {
			private readonly CancellationToken _cancellationTokenBefore;
			private readonly int _countUncancellableBefore;
			private bool _isDisposed;

			public CancellationTokenScope(CancellationToken cancellationTokenBefore,int countUncancellableBefore) {
				_cancellationTokenBefore=cancellationTokenBefore;
				_countUncancellableBefore=countUncancellableBefore;
			}

			public void Dispose() {
				if(_isDisposed) {
					return;
				}
				_isDisposed=true;
				_cancellationToken=_cancellationTokenBefore;
				_countUncancellable=_countUncancellableBefore;
			}
		}
	}
}
