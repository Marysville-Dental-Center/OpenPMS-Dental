// This file is part of the open source dental practice management software derived from Open Dental.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CodeBase;
using ImagingDeviceManager.Config;
using OpenPmsDentalBusiness;

namespace ImagingCapture.Photo {
	///<summary>Where a camera session's videos are written while they record, and what happens to the file after (IMG-P4c; design section 4).
	///A recording streams to %ProgramData%\OpenPMS-Dental\Imaging\Video\recording on this computer, with a sidecar written when it starts
	///(PhotoQuarantine's format: the patient, the session's category and teeth). On Stop it is imported into the patient's images and the
	///local files go. When the import fails, or the program stopped while recording (found at the next session's start), the files move to
	///the unsaved photos folder, where Camera, Unsaved photos files them (a fragmented MP4 is playable up to where it stopped).</summary>
	public static class VideoFiles {
		public const string SubfolderRecording=@"Video\recording";
		///<summary>Recording needs this much free space on the disk of the recording folder (design section 6: "Disk nearly full").</summary>
		public const long BytesFreeMinimum=2L*1024*1024*1024;

		public static string GetRecordingFolder() {
			string pathConfig=LocalCaptureConfig.DefaultPath;
			return pathConfig==null ? null : Path.Combine(Path.GetDirectoryName(pathConfig),SubfolderRecording);
		}

		///<summary>The bytes free on the disk of folder, or -1 when unknown.</summary>
		public static long GetBytesFree(string folder) {
			try {
				string root=Path.GetPathRoot(folder);
				return new DriveInfo(root).AvailableFreeSpace;
			}
			catch(Exception ex) {
				ex.DoNothing();
				return -1;
			}
		}

		///<summary>True when recording may start: the free space is known to be at least BytesFreeMinimum, or cannot be read (then recording is
		///allowed and stops on a write error).</summary>
		public static bool IsEnoughSpace(long bytesFree) {
			return bytesFree<0 || bytesFree>=BytesFreeMinimum;
		}

		///<summary>Makes the recording folder (protected as the Imaging folder) and the path of a new recording, and writes its sidecar. Returns
		///the .mp4 path, or null with error.</summary>
		public static string BeginRecordingFile(PhotoUnsaved photoUnsaved,out string error) {
			error="";
			try {
				string errorFolder;
				if(!LocalCaptureConfig.EnsureImagingFolder(SubfolderRecording,out errorFolder)) {
					error="The video folder is not usable. "+errorFolder;
					return null;
				}
				return BeginRecordingFileInFolder(GetRecordingFolder(),photoUnsaved,out error);
			}
			catch(Exception ex) {
				error=ex.Message;
				return null;
			}
		}

		///<summary>BeginRecordingFile in folder (tests use a folder of their own).</summary>
		public static string BeginRecordingFileInFolder(string folder,PhotoUnsaved photoUnsaved,out string error) {
			error="";
			try {
				Directory.CreateDirectory(folder);
				string baseName=photoUnsaved.DateTimeTaken.ToString("yyyyMMdd-HHmmss-fff",CultureInfo.InvariantCulture)+"_P"
					+photoUnsaved.PatNum.ToString(CultureInfo.InvariantCulture)+"_video";
				string path=Path.Combine(folder,baseName+".mp4");
				for(int i=2;File.Exists(path);i++) {
					path=Path.Combine(folder,baseName+"_"+i.ToString(CultureInfo.InvariantCulture)+".mp4");
				}
				photoUnsaved.PathImage=path;
				File.WriteAllText(photoUnsaved.PathSidecar,PhotoQuarantine.BuildSidecar(photoUnsaved),Encoding.UTF8);
				return path;
			}
			catch(Exception ex) {
				error=ex.Message;
				return null;
			}
		}

		///<summary>Removes a recording's files after its import (or when nothing was recorded). Never throws; false when a file stays.</summary>
		public static bool Delete(string pathVideo) {
			try {
				if(File.Exists(pathVideo)) {
					File.Delete(pathVideo);
				}
				string pathSidecar=Path.ChangeExtension(pathVideo,".txt");
				if(File.Exists(pathSidecar)) {
					File.Delete(pathSidecar);
				}
				return true;
			}
			catch(Exception ex) {
				ex.DoNothing();
				return false;
			}
		}

		///<summary>Moves a recording and its sidecar to the unsaved photos folder (folderUnsaved: null for the default), with the reason in the
		///sidecar. Returns the new path, or null with error (the files then stay where they are).</summary>
		public static string MoveToUnsaved(string pathVideo,string reason,out string error,string folderUnsaved=null,TimeSpan? duration=null) {
			error="";
			try {
				if(folderUnsaved==null) {
					string errorFolder;
					if(!LocalCaptureConfig.EnsureImagingFolder(PhotoQuarantine.Subfolder,out errorFolder)) {
						error="The folder for unsaved photos is not usable. "+errorFolder;
						return null;
					}
					folderUnsaved=PhotoQuarantine.GetFolder();
				}
				Directory.CreateDirectory(folderUnsaved);
				string pathSidecar=Path.ChangeExtension(pathVideo,".txt");
				PhotoUnsaved photoUnsaved=File.Exists(pathSidecar) ? PhotoQuarantine.ParseSidecar(File.ReadAllText(pathSidecar),pathVideo) : null;
				if(photoUnsaved==null) {
					photoUnsaved=ParseFileName(pathVideo);//the sidecar is lost: the patient and time from the file's name
				}
				if(photoUnsaved==null) {
					error="The video's patient is not known (no sidecar): it stays in "+Path.GetDirectoryName(pathVideo)+".";
					return null;
				}
				string pathNew=Path.Combine(folderUnsaved,Path.GetFileName(pathVideo));
				for(int i=2;File.Exists(pathNew);i++) {
					pathNew=Path.Combine(folderUnsaved,Path.GetFileNameWithoutExtension(pathVideo)+"_"+i.ToString(CultureInfo.InvariantCulture)+".mp4");
				}
				photoUnsaved.PathImage=pathNew;
				photoUnsaved.Reason=reason??"";
				if(duration!=null) {
					photoUnsaved.DurationSeconds=(int)Math.Round(duration.Value.TotalSeconds);
				}
				//the sidecar first, so the video is never there without one
				File.WriteAllText(photoUnsaved.PathSidecar,PhotoQuarantine.BuildSidecar(photoUnsaved),Encoding.UTF8);
				try {
					File.Move(pathVideo,pathNew);
				}
				catch {
					File.Delete(photoUnsaved.PathSidecar);
					throw;
				}
				if(File.Exists(pathSidecar)) {
					File.Delete(pathSidecar);
				}
				return pathNew;
			}
			catch(Exception ex) {
				error=ex.Message;
				return null;
			}
		}

		///<summary>The patient and time of a recording from its file name (yyyyMMdd-HHmmss-fff_P{PatNum}_video), or null.</summary>
		public static PhotoUnsaved ParseFileName(string pathVideo) {
			string name=Path.GetFileNameWithoutExtension(pathVideo)??"";
			System.Text.RegularExpressions.Match match=System.Text.RegularExpressions.Regex.Match(name,@"^(\d{8}-\d{6}-\d{3})_P(\d+)_video");
			if(!match.Success) {
				return null;
			}
			long patNum=long.Parse(match.Groups[2].Value,CultureInfo.InvariantCulture);
			DateTime dateTimeTaken;
			DateTime.TryParseExact(match.Groups[1].Value,"yyyyMMdd-HHmmss-fff",CultureInfo.InvariantCulture,DateTimeStyles.None,out dateTimeTaken);
			return patNum>0 ? new PhotoUnsaved { PatNum=patNum,PathImage=pathVideo,Slot=PhotoSessionModel.VideoLabel,DateTimeTaken=dateTimeTaken } : null;
		}

		///<summary>Recordings left in the recording folder (folder: null for the default) by a program that stopped while recording: those not
		///in use move to the unsaved photos folder. Returns how many moved. Never throws.</summary>
		public static int RecoverStale(string folder=null,string folderUnsaved=null) {
			int count=0;
			try {
				folder=folder??GetRecordingFolder();
				if(folder==null || !Directory.Exists(folder)) {
					return 0;
				}
				foreach(string pathVideo in Directory.GetFiles(folder,"*.mp4")) {
					if(IsInUse(pathVideo)) {
						continue;//another session on this computer is recording it
					}
					string error;
					if(MoveToUnsaved(pathVideo,"The program stopped while this video was recording.",out error,folderUnsaved)!=null) {
						count++;
					}
				}
				//a sidecar without its video: the program stopped before the first frame (an hour old, so never a recording just starting)
				foreach(string pathSidecar in Directory.GetFiles(folder,"*.txt")) {
					if(!File.Exists(Path.ChangeExtension(pathSidecar,".mp4")) && File.GetLastWriteTime(pathSidecar)<DateTime.Now.AddHours(-1)) {
						try {
							File.Delete(pathSidecar);
						}
						catch(Exception exDelete) {
							exDelete.DoNothing();
						}
					}
				}
			}
			catch(Exception ex) {
				ex.DoNothing();
			}
			return count;
		}

		private static bool IsInUse(string path) {
			try {
				using(FileStream fileStream=new FileStream(path,FileMode.Open,FileAccess.ReadWrite,FileShare.None)) {
					return false;
				}
			}
			catch(IOException) {
				return true;
			}
			catch(UnauthorizedAccessException) {
				return true;//another Windows user's recording on this computer
			}
		}

		///<summary>"0:42" for a duration.</summary>
		public static string FormatDuration(TimeSpan timeSpan) {
			int seconds=(int)Math.Round(timeSpan.TotalSeconds);
			return (seconds/60).ToString(CultureInfo.InvariantCulture)+":"+(seconds%60).ToString("00",CultureInfo.InvariantCulture);
		}

		///<summary>Imports a finished recording into the patient's images: the document described "Video 0:42", with the teeth and the time it
		///started. Returns the document, or null with error (the caller then keeps the files as unsaved).</summary>
		public static Document Import(IPhotoStore store,string pathVideo,long patNum,long categoryDefNum,string toothNumbers,DateTime dateTimeStarted,
			TimeSpan duration,out string error)
		{
			error="";
			Document document;
			try {
				document=store.ImportVideo(pathVideo,categoryDefNum,patNum);
			}
			catch(Exception ex) {
				error="The video could not be saved: "+ex.Message;
				return null;
			}
			Document documentOld=document.Copy();
			document.Description=duration>TimeSpan.Zero ? "Video "+FormatDuration(duration) : "Video";
			document.ToothNumbers=toothNumbers??"";
			if(dateTimeStarted.Year>1880) {
				document.DateCreated=dateTimeStarted;
			}
			try {
				store.UpdateDocument(document,documentOld);
			}
			catch(Exception ex) {
				error="The video was saved, but its description, teeth and time could not be set: "+ex.Message;
			}
			return document;
		}
	}
}
