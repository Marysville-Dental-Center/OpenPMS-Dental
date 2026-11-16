using CodeBase;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;
using OpenPmsDentalBusiness;//Only ReleaseManifest, linked from OpenPmsDentalBusiness/Misc/ForkRelease/ReleaseManifest.cs.

namespace UpdateFileCopier {
	public partial class FormMain:Form {
		private string _sourceDirectory;
		private string _destDirectory;
		private string _serverName;
		private string _databaseName;
		private string _mySqlUser;
		private string _mySqlPassHash;
		///<summary>Set to true if any files in the installation directory are currently in use.</summary>
		private bool _hasFilesInUse=false;
		///<summary>Set to true if any files in the UpdateFiles folder failed to copy over.</summary>
		private bool _hasCopyFailed=false;
		///<summary>Set to true if the source folder is not exactly the release its manifest describes (product name, file list, sizes, SHA-256).
		///Nothing is copied then.</summary>
		private bool _hasVerifyFailed=false;
		///<summary>Set to true when, after the copy, a runtime file of the installation folder does not match the runtime part the release names
		///(S3b-7). The release manifest is then not written, so the workstation never claims runtime files of two versions; the setup carries the
		///full runtime.</summary>
		private bool _hasRuntimeMismatch=false;
		///<summary>The release manifest of the source folder, read by VerifyRelease. Null until then.</summary>
		private ReleaseManifest _releaseManifest;
		///<summary>If anything goes wrong with the file copying, this string should be used to hold more details about the error to show later.</summary>
		private string _error="";
		///<summary>This indicates whether the file copier should kill all services before copying files.</summary>
		private bool _doKillServices=true;
		///<summary>Indicates whether the copied files will be opened after copying is complete.</summary>
		private bool _doLaunchOpenDental=true;
		///<summary>Directory names of special folders that need to have their files preserved.
		///These names should be treated as if they are being appended to the end of _sourceDirectory and _destDirectory.
		///Any sub folders should be explicitly listed within this list because the copy method used is not recursive.
		///E.g. "\\Parent", "\\Parent\\Child", and "\\Gramps\\Dad\\Bro" are all valid directory names.</summary>
		private readonly List<string> _listSpecialDirs=new List<string>() {
			"\\OpenDentalReplicationService",
			//The 32-bit helper program with its own x86 runtime (OpenPmsDentalBusiness.ForkReleases.ListSpecialDirs; S3b-7).
			"\\Helper32",
		};

		///<summary>Temporary directory that will be used to copy any files present in the destination folder before attempting to copy.</summary>
		private string TempPathDest {
			get {
				return ODFileUtils.CombinePaths(Path.GetTempPath(),ProgramNames.TempFolderName,ProgramNames.UpdateFileCopierTempFolderName,"local");
			}
		}
		///<summary>Temporary directory that will be used to copy all files in the destination folder.</summary>
		private string TempPathSource {
			get {
				return ODFileUtils.CombinePaths(Path.GetTempPath(),ProgramNames.TempFolderName,ProgramNames.UpdateFileCopierTempFolderName,"source");
			}
		}

		public FormMain(string sourceDirectory,string openDentProcessId,string destDirectory,bool doKillServices,bool doLaunchOpenDental,
			string serverName=null,string databaseName=null,string mySqlUser=null,string mySqlPassHash=null)
		{
			InitializeComponent();
			_sourceDirectory=sourceDirectory;
			_destDirectory=destDirectory;
			_doKillServices=doKillServices;
			_doLaunchOpenDental=doLaunchOpenDental;
			_serverName=serverName;
			_databaseName=databaseName;
			_mySqlUser=mySqlUser;
			_mySqlPassHash=mySqlPassHash;
		}

		private void FormMain_Load(object sender,EventArgs e) {

		}

		private void FormMain_Shown(object sender,EventArgs e) {
			StartFileCopierThread();
		}

		///<summary>Kills the program (ProgramNames.ProcessName) and the other programs in the installation directory and then starts a file copier thread.</summary>
		private void StartFileCopierThread() {
			Cursor=Cursors.WaitCursor;
			if(_doKillServices) {//Generally kill except when dealing with DynamicMode
				//Kill every instance of the program (ProgramNames.ProcessName, matched by its whole name).
				//An exe under another name (an older install that used a different name) is not closed.
				KillProcess(ProgramNames.ProcessName);
				//Kill known applications that are present within the installation directory so that the resources are freed if currently in use.
				//A customer complained on the forums about CentralManager explicitly.  DatabaseIntegrityCheck and ServiceManager are just in case.
				KillProcess("CentralManager");
				KillProcess("DatabaseIntegrityCheck");
				KillProcess("ServiceManager");
				//Every other process that runs from the installation folder (S3b-7): the programs there share one runtime (the x64 one beside the
				//exes, the helper's in Helper32\), so the connection settings tool, the helper and the HL7 service hold runtime files too.
				KillProcessesInFolder(_destDirectory);
				//Create a separate thread to copy over the files from the UpdateFiles folder share.
				//In putting this logic in a thread, the user can close the window and not disrupt the copying process.
			}
			ODThread odThread=new ODThread(OnThreadStart);
			odThread.AddExceptionHandler((e) => {
				ShowCriticalError(e.Message);
			});
			odThread.AddExitHandler(OnThreadComplete);
			odThread.Start(true);
		}

		private void ShowCriticalError(string error) {
			if(this.InvokeRequired) {
				this.Invoke(() => ShowCriticalError(error));
				return;
			}
			SetLabelText("Critical Error: "+error
				+"\r\n\r\nPlease address the error above, make sure all instances of Open PMS - Dental are closed, then click Retry.");
			butRetry.Visible=true;
		}

		///<summary></summary>
		private static void KillProcess(string name) {
			Process[] processes=Process.GetProcessesByName(name);
			for(int i=0;i<processes.Length;i++) {
				try {
					processes[i].Kill();//CloseMainWindow and Close were ineffective if a dialog was open.
				}
				catch {
					//Kill() could fail if the process is closed between the time that the process list is read and the time that the Kill() function is called.
					//Since each Kill() call could take a few seconds, this exception could easily be caused by user interaction. In this case, the instance
					//is already closed so we don't need to take any further action.
				}
			}
		}

		///<summary>Stops every process whose program file is in the folder or one of its sub folders (Helper32\), except this one. A process whose
		///path cannot be read (another user's, a protected one) is left alone; the files-in-use check then names the file it holds.</summary>
		private static void KillProcessesInFolder(string folder) {
			if(string.IsNullOrWhiteSpace(folder)) {
				return;
			}
			string folderFull=Path.GetFullPath(folder).TrimEnd('\\')+"\\";
			int processIdThis=Environment.ProcessId;
			foreach(Process process in Process.GetProcesses()) {
				try {
					if(process.Id==processIdThis) {
						continue;
					}
					string path=GetProcessPath(process);
					if(path!="" && Path.GetFullPath(path).StartsWith(folderFull,StringComparison.OrdinalIgnoreCase)) {
						process.Kill();
					}
				}
				catch {
					//The process ended meanwhile, or it may not be stopped by this user. The files-in-use check reports a file it still holds.
				}
				finally {
					process.Dispose();
				}
			}
		}

		///<summary>The full path of the process's program file, or "" when it cannot be read.</summary>
		private static string GetProcessPath(Process process) {
			try {
				return process.MainModule?.FileName??"";
			}
			catch {
				return "";
			}
		}

		///<summary>Attempts to open files that will be updated in the destination dir to check if any are currently in use.
		///Attempts to create temporary directories that will house the installation files temporarily.
		///Attempts to copy all files in the destination directory to a temp directory so that files can be reverted in case of a catastrophic failure.
		///Attempts to copy all files in the source directory to a local temp directory.
		///Attempts to move all files from the source temp directory into the destination directory.</summary>
		private void OnThreadStart(ODThread odThread) {
			SetLabelText("Preparing to Copy Files...");
			//Delay the thread for 300 milliseconds to make sure the above processes have really exited.
			Thread.Sleep(300);
			//Any file exclusions happened when the release was published (OpenPmsDentalBusiness.ForkReleases.ShouldPublishFile). Ex: FreeDentalConfig.xml
			_hasFilesInUse=false;
			_hasCopyFailed=false;
			_hasVerifyFailed=false;
			_hasRuntimeMismatch=false;
			//Only a fork release whose manifest names our product and whose every file matches its SHA-256 is ever copied.
			if(!VerifyRelease(_sourceDirectory)) {
				_hasVerifyFailed=true;
				return;
			}
			List<FileInfoHelper> listSourceFiles=GetFilesFromDir(_sourceDirectory);
			List<FileInfoHelper> listDestFiles=GetFilesFromDir(_destDirectory);
			//Recreate the temp directories that will be used in the copying process
			if(!CreateTempDirectories()) {
				_hasCopyFailed=true;
				return;
			}
			//Check if any of the corresponding files in the installation directory are in use.
			if(HasFilesInUse(listSourceFiles,listDestFiles)) {
				_hasFilesInUse=true;
				return;
			}
			if(!CopyFilesToDir(listDestFiles,TempPathDest) || !CopyFilesToDir(listSourceFiles,TempPathSource)) {
				_hasCopyFailed=true;
				return;
			}
			//Check the local copy again: it is what gets moved into the installation folder.
			if(!VerifyRelease(TempPathSource)) {
				_hasVerifyFailed=true;
				return;
			}
			if(!MoveFilesToDestDir(listSourceFiles)) {
				RevertDestFiles(listDestFiles);//Revert the files in the destination directory back to the way they were before trying to update them.
				_hasCopyFailed=true;
				return;
			}
			CleanUp();
		}

		///<summary></summary>
		private void OnThreadComplete(ODThread odThread) {
			LaunchOpenDental();
		}

		///<summary>Checks the folder against its release manifest (OpenPmsDentalBusiness.ReleaseManifest, compiled into this program). Returns false and sets
		///_error if the manifest is missing, unreadable or for another product, or if any file is missing, extra, or has the wrong size or SHA-256.
		///</summary>
		private bool VerifyRelease(string folder) {
			SetLabelText("Verifying the release...");
			string pathManifest=ODFileUtils.CombinePaths(folder,ReleaseManifest.FILE_NAME);
			try {
				if(!File.Exists(pathManifest)) {
					_error="The update folder has no release manifest ("+ReleaseManifest.FILE_NAME+"). Nothing was copied.";
					return false;
				}
				ReleaseManifest releaseManifest=ReleaseManifest.Parse(File.ReadAllText(pathManifest));
				//The runtime part comes along only when the pull found a new runtime key (S3b-7). Then the folder holds its manifest, which must name the
				//runtime part the release needs, and every runtime file must belong to that one key: never a mix of two runtime versions.
				string pathRuntimeManifest=ODFileUtils.CombinePaths(folder,ReleaseRuntimePart.MANIFEST_FILE_NAME);
				bool hasRuntimePart=File.Exists(pathRuntimeManifest);
				if(hasRuntimePart) {
					ReleaseRuntimePart releaseRuntimePart=ReleaseRuntimePart.ParseManifest(File.ReadAllText(pathRuntimeManifest));
					if(!releaseRuntimePart.IsSameAs(releaseManifest.RuntimePart) || releaseRuntimePart.ArchiveSha256!=releaseManifest.RuntimePart.ArchiveSha256) {
						_error="The runtime part in the update folder is not the one the release needs. Nothing was copied.";
						return false;
					}
				}
				List<string> listProblems=releaseManifest.VerifyFolder(folder,hasRuntimePart);
				if(listProblems.Count>0) {
					_error="The update files do not match their release manifest. Nothing was copied.\r\n"+string.Join("\r\n",listProblems.Take(10));
					return false;
				}
				_releaseManifest=releaseManifest;
			}
			catch(Exception ex) {
				_error="The release manifest could not be checked: "+ex.Message+" Nothing was copied.";
				return false;
			}
			return true;
		}

		///<summary>Tries to recreate the two temporary directories that will be used in the copying process.
		///Returns false if deleting or creating the directories fails.</summary>
		private bool CreateTempDirectories() {
			try {
				//Recreate the current temp dir (used to copy files currently present in the destination directory).
				if(Directory.Exists(TempPathDest)) {
					Directory.Delete(TempPathDest,true);
				}
				Directory.CreateDirectory(TempPathDest);
				//Recreate the source temp dir (files from the source directory).
				if(Directory.Exists(TempPathSource)) {
					Directory.Delete(TempPathSource,true);
				}
				Directory.CreateDirectory(TempPathSource);
			}
			catch {
				//SetLabelText("Temporary directory recreate failure.");
				return false;
			}
			return true;
		}

		///<summary>Clean up method for after copying has completed.  Deletes temporary directories.</summary>
		private void CleanUp() {
			SetLabelText("Cleaning up...");
			try {
				if(Directory.Exists(TempPathDest)) {
					Directory.Delete(TempPathDest,true);
				}
				if(Directory.Exists(TempPathSource)) {
					Directory.Delete(TempPathSource,true);
				}
			}
			catch {
				//Can't delete the temporary folders that we created.  Oh well.
			}
		}

		///<summary>Returns all fils found in the directory passed in.  Also returns files within special folders if present.
		///E.g. will explicitly look for a folder called "Sparks3D" and return each file within if found.</summary>
		private List<FileInfoHelper> GetFilesFromDir(string directory) {
			DirectoryInfo dirInfo=new DirectoryInfo(directory);
			List<FileInfoHelper> listFileInfos=dirInfo.GetFiles().Select(x => new FileInfoHelper(x)).ToList();
			//This is where we will add code whenever there are specific folders that need to be copied to all workstations.
			//We don't want to make a generic method that uses recursion to copy all files and folders because there are some that shouldn't be distributed.
			foreach(string specialDir in _listSpecialDirs) {
				DirectoryInfo specialDirInfo=new DirectoryInfo(directory+specialDir);
				if(specialDirInfo.Exists) {
					listFileInfos.AddRange(specialDirInfo.GetFiles().Select(x => new FileInfoHelper(x,specialDir)).ToList());
				}
			}
			return listFileInfos;
		}

		///<summary>Verifies that all files passed in are not currently in use in the destination directory.
		///Returns true if there was a file with the same name in the list of files that will be copied still in use.</summary>
		private bool HasFilesInUse(List<FileInfoHelper> listSourceFiles,List<FileInfoHelper> listDestFiles) {
			//For every file passed in, make sure the corresponding file in the destination directory is not in use.
			//The names of our dlls are fairly unique so it should be fine to look for file name collisions (do not worry about directory info).
			List<string> listPresentFileNames=listSourceFiles.Select(x => x.FileInfoCur.Name).ToList();
			//Check if a file is in use if it is present within the destination directory and will need to be updated.
			foreach(FileInfoHelper destFile in listDestFiles.Where(x => listPresentFileNames.Contains(x.FileInfoCur.Name))) {
				//Try to open the file which will successfully tell us if the file is currently in use.
				try {
					using(FileStream fStream=destFile.FileInfoCur.Open(FileMode.Open,FileAccess.ReadWrite)) {
						//The quickest, easiest way to tell if a file is already in use is to simply try to open it with Write permissions.
						//If the file is in use and we try to open it for writing, then an exception will occur.  We also need Read permissions to copy.
						//If successful, the stream will close right away and move on to the next file.
						//This does not combat thread race conditions.  That is an impossible issue to combat (no such thing as an accurate "file in use" check).
					}
				}
				catch(System.Security.SecurityException se) {
					_error="Security Error when accessing "+destFile.FileInfoCur.Name+": "+se.Message;
					return true;
				}
				catch(ArgumentException ae) {
					_error="Argument Error when accessing "+destFile.FileInfoCur.Name+": "+ae.Message;
					return true;
				}
				catch(FileNotFoundException fnfe) {
					_error="File Not Found Error when accessing "+destFile.FileInfoCur.Name+": "+fnfe.Message;
					return true;
				}
				catch(UnauthorizedAccessException uae) {
					_error="Unauthorized Access Error when accessing "+destFile.FileInfoCur.Name+": "+uae.Message;
					return true;
				}
				catch(DirectoryNotFoundException dnfe) {
					_error="Directory Not Found Error when accessing "+destFile.FileInfoCur.Name+": "+dnfe.Message;
					return true;
				}
				catch(IOException ioe) {
					_error="IO Error when accessing "+destFile.FileInfoCur.Name+": "+ioe.Message;
					return true;
				}
				//The file is unavailable because it is still be written to or used by another thread.
				catch(Exception ex) {
					_error="Generic Error when accessing "+destFile.FileInfoCur.Name+": "+ex.Message;
					return true;
				}
			}
			return false;
		}

		///<summary>Copies files to the specified directory.  Returns false if something goes wrong with any file.</summary>
		private bool CopyFilesToDir(List<FileInfoHelper> listDestFiles,string dirPath) {
			foreach(FileInfoHelper destFile in listDestFiles) {
				SetLabelText("Backing up: "+destFile.FileInfoCur.Name);
				string destDir=dirPath+destFile.SubDir;
				string sourceFileName=destFile.FileInfoCur.FullName;
				string destFileName=ODFileUtils.CombinePaths(destDir,destFile.FileInfoCur.Name);
				try {
					if(!Directory.Exists(destDir)) {
						Directory.CreateDirectory(destDir);
					}
					File.Copy(sourceFileName,destFileName,true);
				}
				catch {
					//SetLabelText("Copying files from destination to temp failed.");
					return false;
				}
			}
			return true;//Files backed up successfully.
		}

		///<summary>Moves all files in the temporary directory to the destination directory.
		///Returns false if there was a problem moving the files from the temporary directory to the destination directory.</summary>
		private bool MoveFilesToDestDir(List<FileInfoHelper> listSourceFiles) {
			string sourceManifest="";
			foreach(FileInfoHelper sourceFile in listSourceFiles) {
				if(sourceFile.SubDir=="" && sourceFile.FileInfoCur.Name==ReleaseManifest.FILE_NAME) {
					sourceManifest=ODFileUtils.CombinePaths(TempPathSource+sourceFile.SubDir,sourceFile.FileInfoCur.Name);
					continue;
				}
				if(sourceFile.SubDir=="" && sourceFile.FileInfoCur.Name==ReleaseRuntimePart.MANIFEST_FILE_NAME) {
					continue;//The runtime part's manifest stays in the update folder; the release manifest lists its files (RuntimeFile lines).
				}
				SetLabelText("Verifying: "+sourceFile.FileInfoCur.Name);
				string destDir=_destDirectory+sourceFile.SubDir;
				string source=ODFileUtils.CombinePaths(TempPathSource+sourceFile.SubDir,sourceFile.FileInfoCur.Name);
				string dest=ODFileUtils.CombinePaths(destDir,sourceFile.FileInfoCur.Name);
				try {
					if(!Directory.Exists(destDir)) {
						Directory.CreateDirectory(destDir);
					}
					File.Copy(source,dest,true);
				}
				catch {
					//SetLabelText("Copying files from temp to destination failed.");
					return false;
				}
			}
			//Every runtime file of the installation folder must be the one the release's runtime part lists, whether this run copied the runtime part or
			//the folder kept its own (the same key). Otherwise no release manifest is written: the folder would claim a release whose runtime files are
			//of another version (S3b-7). Nothing is reverted; the setup carries the full runtime.
			if(_releaseManifest?.RuntimePart!=null) {
				SetLabelText("Checking the runtime files...");
				List<string> listRuntimeProblems=_releaseManifest.RuntimePart.VerifyFiles(_destDirectory);
				if(listRuntimeProblems.Count>0) {
					_hasRuntimeMismatch=true;
					_error="The runtime files in "+_destDirectory+" do not all match the runtime of the release (.NET "+_releaseManifest.RuntimePart.Version
						+"), so the release was not recorded as installed.\r\n"+string.Join("\r\n",listRuntimeProblems.Take(10));
					return true;//The files were copied; LaunchOpenDental reports the mismatch instead of starting the program.
				}
			}
			//Save the release manifest for last, so the installation folder only claims a release once all of its files are in place.
			if(sourceManifest!="") {
				try {
					File.Copy(sourceManifest,Path.Combine(_destDirectory,ReleaseManifest.FILE_NAME),true);
				}
				catch {
					//SetLabelText("Copying Manifest.txt from temp to destination failed.");
					return false;
				}
			}
			return true;//Files copied successfully.
		}

		private void RevertDestFiles(List<FileInfoHelper> listDestFiles) {
			foreach(FileInfoHelper destFile in listDestFiles) {
				SetLabelText("Reverting: "+destFile.FileInfoCur.Name);
				string destDir=_destDirectory+destFile.SubDir;
				string source=ODFileUtils.CombinePaths(TempPathDest+destFile.SubDir,destFile.FileInfoCur.Name);
				string dest=ODFileUtils.CombinePaths(destDir,destFile.FileInfoCur.Name);
				try {
					if(!Directory.Exists(destDir)) {
						Directory.CreateDirectory(destDir);
					}
					File.Copy(source,dest,true);
				}
				catch {
					//Not sure what to do here.  Maybe someone started using a file in the middle of a copy?
				}
			}
		}

		///<summary></summary>
		private void SetLabelText(string text, bool isLongText=false) {
			if(this.InvokeRequired) {
				this.Invoke((Action)delegate() { SetLabelText(text); });
				return;
			}
			if(isLongText) {
				labelFile.MaximumSize = new Size(labelFile.Width,0);
				labelFile.AutoSize=true;
				labelFile.Text=text;
				labelFile.AutoSize=false;
				return;
			}
			labelFile.Text=text;
		}

		///<summary>Checks and displays messages for any errors that occurred during the file copying.
		///If no errors, attempts to launch a new instance of Open Dental with the freshly copied binaries.</summary>
		private void LaunchOpenDental() {
			if(this.InvokeRequired) {
				this.Invoke((Action)delegate() { LaunchOpenDental(); });
				return;
			}
			Cursor=Cursors.Default;
			if(_hasFilesInUse) {
				string error="There are files still in use.  Please make sure all instances of Open PMS - Dental are closed then click Retry.";
				if(!string.IsNullOrEmpty(_error)) {
					error=_error+"\r\n\r\nPlease address the error above, make sure all instances of Open PMS - Dental are closed, then click Retry.";
				}
				SetLabelText(error,isLongText:true);
				butRetry.Visible=true;
			}
			else if(_hasVerifyFailed) {
				//A failed check is not retried: the files will not change. The workstation keeps its current program.
				SetLabelText(_error+"\r\n\r\nPublish the release again from the update server, then start the program.",isLongText:true);
			}
			else if(_hasRuntimeMismatch) {
				//Not retried: the runtime files of the folder are not the release's. Only the setup puts the whole runtime back.
				SetLabelText(_error+"\r\n\r\nRun the setup of this release on this workstation as an administrator: it carries the full runtime.",
					isLongText:true);
			}
			else if(_hasCopyFailed) {
				SetLabelText("Some files failed to copy.  Verify network access and permissions are correct, then click Retry.");
				butRetry.Visible=true;
			}
			else {//Everything copied correctly, try and launch Open Dental.
				SetLabelText("Done");
				Thread.Sleep(300);//Give SetLabelText some time to finish to make sure Application.Exit doesn't get called too soon.
				//The program exe name comes from ProgramNames, the one place a rename changes it.
				try {
					if(_doLaunchOpenDental) {
						string destFileName=ODFileUtils.CombinePaths(_destDirectory,ProgramNames.ExeFileName);
						//Arguments: servername, database name, sql username, sql password hash
						Process proc=new Process();
						proc.StartInfo.UseShellExecute=true;//Through the shell, which asks for consent if the program needs elevation (the .NET default is false).
						if(!string.IsNullOrWhiteSpace(_serverName)) {
							proc.StartInfo.Arguments=" ServerName="+_serverName
								+" DatabaseName="+_databaseName
								+" MySqlUser="+_mySqlUser;
							//The program passes a password hash only for a server on this computer. For a server over the network the restarted program
							//uses the password stored on this computer (docs/specs/db-connection-security.md), so none is passed on.
							if(!string.IsNullOrEmpty(_mySqlPassHash)) {
								proc.StartInfo.Arguments+=" MySqlPassHash="+_mySqlPassHash;
							}
						}
						proc.StartInfo.FileName=destFileName;
						proc.Start();
					}
				}
				catch(Exception ex) {
					ex.DoNothing();
				}
				Application.Exit();//To close the form.
			}
		}

		private void butRetry_Click(object sender,EventArgs e) {
			butRetry.Visible=false;
			StartFileCopierThread();
		}

		///<summary>A helper class that keeps track of the sub directory that this file belongs to within source and dest directories.</summary>
		private class FileInfoHelper {
			///<summary>The FileInfo for the original file.  The directory information on this file is not to be used.</summary>
			public FileInfo FileInfoCur;
			///<summary>The sub directory where this file should be copied to relative to _sourceDirectory or _destDirectory.
			///Will be blank if this file has no sub directory and is destined for the root directory.</summary>
			public string SubDir;

			public FileInfoHelper(FileInfo fileInfo,string subDir="") {
				FileInfoCur=fileInfo;
				SubDir=subDir;
			}
		}
	}	
}
