// A copy of the helper under a name of its own, so the processes a test starts can be told from any other (S3a-13, docs/port-notes/S3a-13.md,
// section 8), as ProbeProcess does for TestProbeHost.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;

namespace Helper32Tests.Support {
	///<summary>OpenPmsDentalHelper32-&lt;12 hex&gt;.exe and its .exe.config in this test folder (the helper's application base is the folder, so a
	///test module of this assembly loads there). Dispose waits for the copy's processes to end (it ends any left after 15 seconds: processes of
	///this copy's name, which only this test started) and deletes both files.</summary>
	public sealed class HelperCopy:IDisposable {
		public const string HELPER_FILE_NAME="OpenPmsDentalHelper32.exe";
		public const string COPY_PREFIX="OpenPmsDentalHelper32-";
		private static readonly Regex _regexCopyName=new Regex(@"^OpenPmsDentalHelper32-[0-9a-f]{12}\.exe(\.config)?$",RegexOptions.IgnoreCase);

		public string PathExe { get; private set; }

		///<summary>The copy's file name, as a process list shows it.</summary>
		public string FileName {
			get {
				return Path.GetFileName(PathExe);
			}
		}

		public static string Folder {
			get {
				return Path.GetDirectoryName(typeof(HelperCopy).Assembly.Location);
			}
		}

		///<summary>The extra arguments that load FakeModule as the helper's module "Test".</summary>
		public static string ArgsFakeModule {
			get {
				return "--test-module Helper32Tests "+typeof(FakeModule).FullName;
			}
		}

		public static HelperCopy Create() {
			string pathSource=Path.Combine(Folder,HELPER_FILE_NAME);
			if(!File.Exists(pathSource)) {
				throw new FileNotFoundException("The helper is not built next to the tests: "+pathSource);
			}
			DeleteStaleCopies();
			byte[] arrayRandom=new byte[6];
			using(RandomNumberGenerator randomNumberGenerator=RandomNumberGenerator.Create()) {
				randomNumberGenerator.GetBytes(arrayRandom);
			}
			string pathExe=Path.Combine(Folder,COPY_PREFIX+string.Concat(arrayRandom.Select(x => x.ToString("x2")))+".exe");
			File.Copy(pathSource,pathExe);
			File.SetCreationTimeUtc(pathExe,DateTime.UtcNow);
			if(File.Exists(pathSource+".config")) {
				File.Copy(pathSource+".config",pathExe+".config");
				File.SetCreationTimeUtc(pathExe+".config",DateTime.UtcNow);
			}
			HelperCopy helperCopy=new HelperCopy();
			helperCopy.PathExe=pathExe;
			return helperCopy;
		}

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
			DeleteQuietly(PathExe+".config");
		}

		///<summary>Copies a test process that died before its Dispose left in the folder: only files of exactly the copies' names, made before this
		///process started (a copy whose helper still runs is locked and stays).</summary>
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
