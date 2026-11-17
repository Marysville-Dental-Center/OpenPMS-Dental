// A copy of the helper under a name of its own, so the processes a test starts can be told from any other (S3a-13, docs/port-notes/S3a-13.md,
// section 8), as ProbeProcess does for TestProbeHost.
// Since S3b-5 of the .NET 10 move the helper is the program's own: OpenPmsDental\bin\<configuration>\Helper32\, where the program's build puts
// the x86 helper with its own self-contained x86 runtime (S3b-4). A copy of its apphost alone cannot start anywhere else (S3b-0 item 7), so this
// process makes one helper folder of its own beside the test (hard links of every file of Helper32\, a copy where a link cannot be made), and
// each HelperCopy is a copy of the apphost there under its own name, which starts the folder's OpenPmsDentalHelper32.dll on the folder's runtime.
// The test assembly that holds the helper's test modules (FakeModule, the stand-ins of BINBTests and VendorDirectTests) is x64 like every
// test assembly, which a 32-bit process cannot load: the folder gets an any-CPU copy of it (Mono.Cecil), so the helper loads the same code.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using Mono.Cecil;

namespace Helper32Tests.Support {
	///<summary>OpenPmsDentalHelper32-&lt;12 hex&gt;.exe in this process's helper folder (Folder), the helper's application base, where a test
	///module of the test assembly loads. Dispose waits for the copy's processes to end (it ends any left after 15 seconds: processes of this
	///copy's name, which only this test started) and deletes the copy.</summary>
	public sealed class HelperCopy:IDisposable {
		public const string HELPER_FILE_NAME="OpenPmsDentalHelper32.exe";
		public const string COPY_PREFIX="OpenPmsDentalHelper32-";
		///<summary>The folder under the test's own folder that holds the helper folders of the test processes.</summary>
		private const string FOLDER_COPIES="helper32-copies";
		private static readonly Regex _regexCopyName=new Regex(@"^OpenPmsDentalHelper32-[0-9a-f]{12}\.exe$",RegexOptions.IgnoreCase);
		private static readonly object _lockFolder=new object();
		private static string _folder;

		public string PathExe { get; private set; }

		///<summary>The copy's file name, as a process list shows it.</summary>
		public string FileName {
			get {
				return Path.GetFileName(PathExe);
			}
		}

		///<summary>The folder of the test assembly.</summary>
		public static string FolderTests {
			get {
				return Path.GetDirectoryName(typeof(HelperCopy).Assembly.Location);
			}
		}

		///<summary>The helper's folder of this test process: made at the first use from the program output's Helper32\ folder, with the test
		///assembly (any CPU) and TopazStandIn.dll beside the helper; deleted when the process ends.</summary>
		public static string Folder {
			get {
				lock(_lockFolder) {
					if(_folder==null) {
						_folder=MakeFolder();
					}
					return _folder;
				}
			}
		}

		///<summary>The program output's Helper32\ folder of the test's configuration (bin\Debug or bin\Release, as the test's own folder).</summary>
		public static string FolderProgramHelper {
			get {
				string configuration=new DirectoryInfo(FolderTests).Name;
				string folderRoot=FolderTests;
				while(folderRoot!=null && !File.Exists(Path.Combine(folderRoot,"OpenDental.sln"))) {
					folderRoot=Path.GetDirectoryName(folderRoot);
				}
				if(folderRoot==null) {
					throw new DirectoryNotFoundException("The repository root (OpenDental.sln) is not above "+FolderTests+".");
				}
				return Path.Combine(folderRoot,"OpenPmsDental","bin",configuration,"Helper32");
			}
		}

		///<summary>The extra arguments that load FakeModule as the helper's module "Test".</summary>
		public static string ArgsFakeModule {
			get {
				return "--test-module "+typeof(HelperCopy).Assembly.GetName().Name+" "+typeof(FakeModule).FullName;
			}
		}

		public static HelperCopy Create() {
			string folder=Folder;
			string pathSource=Path.Combine(folder,HELPER_FILE_NAME);
			DeleteStaleCopies();
			byte[] arrayRandom=new byte[6];
			using(RandomNumberGenerator randomNumberGenerator=RandomNumberGenerator.Create()) {
				randomNumberGenerator.GetBytes(arrayRandom);
			}
			string pathExe=Path.Combine(folder,COPY_PREFIX+string.Concat(arrayRandom.Select(x => x.ToString("x2")))+".exe");
			File.Copy(pathSource,pathExe);
			File.SetCreationTimeUtc(pathExe,DateTime.UtcNow);
			HelperCopy helperCopy=new HelperCopy();
			helperCopy.PathExe=pathExe;
			return helperCopy;
		}

		///<summary>Makes this process's helper folder: helper32-copies\&lt;process id&gt;-&lt;hex&gt;\ beside the test assembly. Every file of the
		///program output's Helper32\ folder as a hard link (the same volume, so no 100 MB copy per test process), else as a copy; the test
		///assembly written for any CPU; TopazStandIn.dll when the test has it. The folders of test processes that have ended are deleted first.
		///</summary>
		private static string MakeFolder() {
			string folderSource=FolderProgramHelper;
			if(!File.Exists(Path.Combine(folderSource,HELPER_FILE_NAME))) {
				throw new FileNotFoundException("The helper is not built into the program's output (OpenPmsDental's CopyHelper32): "
					+Path.Combine(folderSource,HELPER_FILE_NAME)+". Build OpenPmsDental (x64) first.");
			}
			string folderCopies=Path.Combine(FolderTests,FOLDER_COPIES);
			DeleteFoldersOfEndedProcesses(folderCopies);
			int processId;
			using(Process processThis=Process.GetCurrentProcess()) {
				processId=processThis.Id;
			}
			string folder=Path.Combine(folderCopies,processId+"-"+Guid.NewGuid().ToString("N").Substring(0,8));
			Directory.CreateDirectory(folder);
			foreach(string pathFile in Directory.GetFiles(folderSource)) {
				string pathTarget=Path.Combine(folder,Path.GetFileName(pathFile));
				if(!CreateHardLink(pathTarget,pathFile,IntPtr.Zero)) {
					File.Copy(pathFile,pathTarget);
				}
			}
			//The test assembly, whose test modules the helper loads by name from its folder (TestModule), for any CPU.
			string pathTests=typeof(HelperCopy).Assembly.Location;
			DefaultAssemblyResolver defaultAssemblyResolver=new DefaultAssemblyResolver();
			defaultAssemblyResolver.AddSearchDirectory(FolderTests);
			using(ModuleDefinition moduleDefinition=ModuleDefinition.ReadModule(pathTests,
				new ReaderParameters { InMemory=true,AssemblyResolver=defaultAssemblyResolver }))
			{
				moduleDefinition.Architecture=TargetArchitecture.I386;
				moduleDefinition.Attributes=ModuleAttributes.ILOnly;
				moduleDefinition.Write(Path.Combine(folder,Path.GetFileName(pathTests)));
			}
			defaultAssemblyResolver.Dispose();
			string pathStandIn=Path.Combine(FolderTests,"TopazStandIn.dll");
			if(File.Exists(pathStandIn)) {
				File.Copy(pathStandIn,Path.Combine(folder,"TopazStandIn.dll"));
			}
			AppDomain.CurrentDomain.ProcessExit+=(sender,e) => DeleteFolderQuietly(folder);
			return folder;
		}

		///<summary>The helper folders of test processes that are no longer running (by the process id in the folder's name and the folder's
		///creation time, so a new process with a reused id keeps nothing of an old one's).</summary>
		private static void DeleteFoldersOfEndedProcesses(string folderCopies) {
			if(!Directory.Exists(folderCopies)) {
				return;
			}
			foreach(string folder in Directory.GetDirectories(folderCopies)) {
				string name=Path.GetFileName(folder);
				int processId;
				if(!int.TryParse(name.Split('-')[0],out processId)) {
					continue;
				}
				bool isRunning=false;
				try {
					using(Process process=Process.GetProcessById(processId)) {
						isRunning=!process.HasExited && process.StartTime.ToUniversalTime()<=Directory.GetCreationTimeUtc(folder);
					}
				}
				catch(Exception) {
					//No such process.
				}
				if(!isRunning) {
					DeleteFolderQuietly(folder);
				}
			}
		}

		private static void DeleteFolderQuietly(string folder) {
			try {
				Directory.Delete(folder,true);
			}
			catch(Exception) {
				//A helper that still runs holds its files; the next test process deletes the folder.
			}
		}

		[DllImport("kernel32.dll",CharSet=CharSet.Unicode,SetLastError=true)]
		private static extern bool CreateHardLink(string lpFileName,string lpExistingFileName,IntPtr lpSecurityAttributes);

		///<summary>The running processes of this copy.</summary>
		public List<ProcessEntry> GetProcesses() {
			return ProcessList.GetNamed(FileName);
		}

		public void Dispose() {
			DateTime dateTimeDeadline=DateTime.UtcNow.AddSeconds(15);
			while(GetProcesses().Count>0 && DateTime.UtcNow<dateTimeDeadline) {
				Thread.Sleep(100);
			}
			foreach(ProcessEntry entry in GetProcesses()) {
				try {
					using(Process process=Process.GetProcessById(entry.ProcessId)) {
						process.Kill();//a process of this copy, which only this test started
						process.WaitForExit(10000);
					}
				}
				catch(Exception) {
					//ended meanwhile
				}
			}
			DeleteQuietly(PathExe);
		}

		///<summary>Copies a test process that died before its Dispose left in this process's helper folder: only files of exactly the copies'
		///names, made before this process started (a copy whose helper still runs is locked and stays). The folders of ended test processes go
		///whole (MakeFolder).</summary>
		public static void DeleteStaleCopies() {
			DateTime dateTimeStart=Process.GetCurrentProcess().StartTime.ToUniversalTime();
			foreach(string path in Directory.GetFiles(Folder,COPY_PREFIX+"*")) {
				if(_regexCopyName.IsMatch(Path.GetFileName(path)) && File.GetCreationTimeUtc(path)<dateTimeStart) {
					DeleteQuietly(path);
				}
			}
		}

		private static void DeleteQuietly(string path) {
			for(int i=0;i<20;i++) {
				try {
					if(File.Exists(path)) {
						File.Delete(path);
					}
					return;
				}
				catch(IOException) {
					Thread.Sleep(250);//the process that ran it is still going away
				}
				catch(UnauthorizedAccessException) {
					Thread.Sleep(250);
				}
			}
		}
	}
}
