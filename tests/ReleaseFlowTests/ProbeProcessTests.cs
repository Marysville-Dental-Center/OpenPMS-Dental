// The child process of the tests that need one (tests\TestProbeHost, S3a-8 and S3a-8b of the .NET 10 move; docs/port-notes/S3a-8b.md): the
// copies of the host that a test process left behind when it died before Dispose are deleted on the next start, and only those; a started
// child answers and its copy is gone after Dispose; a run-once child is a console program's Main (its exit code and output, the process ends
// only when its foreground threads have, a timeout ends it). In scratch folders under the temp folder, with synthetic files; the only process
// started is the host itself, with this assembly's handlers below.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestProbeHost;

namespace ReleaseFlowTests {
	[TestClass]
	public class ProbeProcessTests {
		private static void DeleteQuietly(string folder) {
			try {
				Directory.Delete(folder,true);
			}
			catch(Exception) {
				//Left in the temp folder.
			}
		}

		///<summary>When this test process started, in UTC.</summary>
		private static DateTime ProcessStartUtc() {
			using(Process process=Process.GetCurrentProcess()) {
				return process.StartTime.ToUniversalTime();
			}
		}

		///<summary>A file with the given creation time (a synthetic copy: its bytes do not matter to the clean-up).</summary>
		private static string WriteFile(string folder,string fileName,DateTime dateCreatedUtc) {
			string path=Path.Combine(folder,fileName);
			File.WriteAllText(path,"synthetic "+fileName);
			File.SetCreationTimeUtc(path,dateCreatedUtc);
			return path;
		}

		private static List<string> ListCopies(string folder) {
			return Directory.GetFiles(folder).Select(x => Path.GetFileName(x)).Where(x => ProbeProcess.IsCopyName(x)).OrderBy(x => x).ToList();
		}

		[TestMethod]
		public void IsCopyName_OnlyTheNamesCopyHostMakes() {
			Assert.IsTrue(ProbeProcess.IsCopyName("TestProbeHost-0123456789ab.exe"));
			Assert.IsTrue(ProbeProcess.IsCopyName("TestProbeHost-0123456789ab.exe.config"));
			Assert.IsFalse(ProbeProcess.IsCopyName("TestProbeHost.exe"),"The host itself, next to the test assembly.");
			Assert.IsFalse(ProbeProcess.IsCopyName("TestProbeHost.exe.config"));
			Assert.IsFalse(ProbeProcess.IsCopyName("TestProbeHost-0123456789AB.exe"),"Guid.ToString(\"N\") is lower case.");
			Assert.IsFalse(ProbeProcess.IsCopyName("TestProbeHost-0123456789a.exe"),"11 digits.");
			Assert.IsFalse(ProbeProcess.IsCopyName("TestProbeHost-0123456789abc.exe"),"13 digits.");
			Assert.IsFalse(ProbeProcess.IsCopyName("TestProbeHost-0123456789ab.dll"));
			Assert.IsFalse(ProbeProcess.IsCopyName("ToothChartSecondProcess-01234567.exe"),"Another program's name.");
			Assert.IsFalse(ProbeProcess.IsCopyName("TestProbeHost-0123456789ab.exe.bak"));
		}

		///<summary>Only the host's own copies (and their configs) made before this process started are deleted: a copy made since (a child of this
		///process, or of another test process that started later) stays, a copy whose child still runs (here: the file is held open, as a running
		///exe is) stays with its config, and no other file is touched, even with an old date.</summary>
		[TestMethod]
		public void DeleteStaleCopies_OnlyTheHostsOwnCopiesFromBeforeThisProcess() {
			string folder=TestPaths.NewTempFolder("probeprocess");
			try {
				DateTime dateOld=ProcessStartUtc().AddHours(-1);
				DateTime dateNew=DateTime.UtcNow.AddSeconds(1);
				string pathStale=WriteFile(folder,"TestProbeHost-00000000000a.exe",dateOld);
				string pathStaleConfig=WriteFile(folder,"TestProbeHost-00000000000a.exe.config",dateOld);
				string pathOrphanConfig=WriteFile(folder,"TestProbeHost-00000000000b.exe.config",dateOld);
				string pathHeld=WriteFile(folder,"TestProbeHost-00000000000c.exe",dateOld);
				string pathHeldConfig=WriteFile(folder,"TestProbeHost-00000000000c.exe.config",dateOld);
				string pathFresh=WriteFile(folder,"TestProbeHost-00000000000d.exe",dateNew);
				string pathFreshConfig=WriteFile(folder,"TestProbeHost-00000000000d.exe.config",dateNew);
				List<string> listOthers=new List<string> {
					WriteFile(folder,"TestProbeHost.exe",dateOld),
					WriteFile(folder,"TestProbeHost.exe.config",dateOld),
					WriteFile(folder,"TestProbeHost-0123456789AB.exe",dateOld),
					WriteFile(folder,"TestProbeHost-00000000000e.dll",dateOld),
					WriteFile(folder,"ToothChartSecondProcess-01234567.exe",dateOld),
					WriteFile(folder,"Other.exe",dateOld),
				};
				List<string> listDeleted;
				using(new FileStream(pathHeld,FileMode.Open,FileAccess.Read,FileShare.None)) {
					listDeleted=ProbeProcess.DeleteStaleCopies(folder);
				}
				CollectionAssert.AreEquivalent(new[] { pathStale,pathStaleConfig,pathOrphanConfig },listDeleted,string.Join("; ",listDeleted));
				Assert.IsFalse(File.Exists(pathStale));
				Assert.IsFalse(File.Exists(pathStaleConfig));
				Assert.IsFalse(File.Exists(pathOrphanConfig));
				Assert.IsTrue(File.Exists(pathHeld),"A copy whose child still runs cannot be deleted.");
				Assert.IsTrue(File.Exists(pathHeldConfig),"Its config stays with it.");
				Assert.IsTrue(File.Exists(pathFresh),"A copy made after this process started may be a running child's.");
				Assert.IsTrue(File.Exists(pathFreshConfig));
				foreach(string path in listOthers) {
					Assert.IsTrue(File.Exists(path),"Not a copy's name: "+path);
				}
				//Once the held copy's child has ended, the next clean-up takes it and its config.
				CollectionAssert.AreEquivalent(new[] { pathHeld,pathHeldConfig },ProbeProcess.DeleteStaleCopies(folder));
				Assert.AreEqual(0,ProbeProcess.DeleteStaleCopies(Path.Combine(folder,"missing")).Count,"A folder that is not there.");
			}
			finally {
				DeleteQuietly(folder);
			}
		}

		///<summary>Start cleans the folder first, then its own child answers; Dispose ends it and deletes its copy and config, so no copy is left.
		///</summary>
		[TestMethod]
		public void Start_DeletesTheStaleCopiesFirst_AndDisposeLeavesNoCopy() {
			string folder=TestPaths.NewTempFolder("probeprocess");
			try {
				DateTime dateOld=ProcessStartUtc().AddHours(-1);
				WriteFile(folder,"TestProbeHost-00000000000a.exe",dateOld);
				WriteFile(folder,"TestProbeHost-00000000000a.exe.config",dateOld);
				using(ProbeProcess probeProcess=ProbeProcess.Start(folder,"<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<configuration />\r\n",
					typeof(ProbeEchoHandler),TimeSpan.FromMinutes(2)))
				{
					List<string> listCopies=ListCopies(folder);
					Assert.AreEqual(2,listCopies.Count,"Only the running child's copy and its config: "+string.Join(", ",listCopies));
					Assert.IsFalse(listCopies.Contains("TestProbeHost-00000000000a.exe"));
					string reply=probeProcess.Call(ProbeEchoHandler.COMMAND_ECHO,"synthetic text","\u00e4 second line\r\n");
					Assert.AreEqual("synthetic text|\u00e4 second line\r\n|"+Path.GetFullPath(folder).TrimEnd('\\'),reply);
					Assert.AreEqual(0,ProbeProcess.DeleteStaleCopies(folder).Count,"The running child's copy is newer than this process.");
					Assert.AreEqual(2,ListCopies(folder).Count);
				}
				Assert.AreEqual(0,ListCopies(folder).Count,"Dispose deletes the copy and its config.");
			}
			finally {
				DeleteQuietly(folder);
			}
		}

		///<summary>RunOnce: the entry method is the Main of a process of its own, from the folder, with the config given: its exit code and output
		///come back, a foreground thread it leaves keeps the process until the thread ends (a background one does not), an exception out of it ends
		///the process with the host's exit code for it, a hang is ended at the timeout, and no copy is left.</summary>
		[TestMethod]
		public void RunOnce_IsTheProcesssMain() {
			string folder=TestPaths.NewTempFolder("probeprocess");
			try {
				ProbeRunResult probeRunResult=ProbeProcess.RunOnce(folder,null,typeof(ProbeOnceEntry),new[] { "exit",
					"synthetic argument with spaces" },TimeSpan.FromMinutes(2));
				Assert.IsFalse(probeRunResult.IsTimedOut,probeRunResult.Output);
				Assert.AreEqual(7,probeRunResult.ExitCode,probeRunResult.Output);
				StringAssert.Contains(probeRunResult.Output,"args: synthetic argument with spaces\r\n");
				StringAssert.Contains(probeRunResult.Output,"base: "+Path.GetFullPath(folder).TrimEnd('\\')+"\r\n");
				StringAssert.Contains(probeRunResult.Output,"stderr: to standard error\r\n");
				StringAssert.Contains(probeRunResult.Output,"apartment: MTA\r\n");
				probeRunResult=ProbeProcess.RunOnce(folder,null,typeof(ProbeOnceEntry),new[] { "foreground" },TimeSpan.FromMinutes(2));
				Assert.AreEqual(0,probeRunResult.ExitCode,probeRunResult.Output);
				StringAssert.Contains(probeRunResult.Output,"foreground thread ended\r\n","The process waited for its foreground thread.");
				Assert.IsTrue(probeRunResult.Elapsed.TotalMilliseconds>=1500,probeRunResult.Elapsed.ToString());
				probeRunResult=ProbeProcess.RunOnce(folder,null,typeof(ProbeOnceEntry),new[] { "background" },TimeSpan.FromMinutes(2));
				Assert.IsFalse(probeRunResult.IsTimedOut,"A background thread does not keep the process: "+probeRunResult.Output);
				Assert.AreEqual(0,probeRunResult.ExitCode,probeRunResult.Output);
				probeRunResult=ProbeProcess.RunOnce(folder,null,typeof(ProbeOnceEntry),new[] { "throw" },TimeSpan.FromMinutes(2));
				Assert.IsFalse(probeRunResult.IsTimedOut);
				Assert.AreEqual(92,probeRunResult.ExitCode,"Run threw: the host's own exit code. "+probeRunResult.Output);
				StringAssert.Contains(probeRunResult.Output,"stderr: Run threw: System.InvalidOperationException: A synthetic failure out of Run.");
				probeRunResult=ProbeProcess.RunOnce(folder,null,typeof(ProbeOnceEntry),new[] { "hang" },TimeSpan.FromSeconds(3));
				Assert.IsTrue(probeRunResult.IsTimedOut,probeRunResult.Output);
				Assert.AreEqual(-1,probeRunResult.ExitCode);
				probeRunResult=ProbeProcess.RunOnce(folder,null,typeof(ProbeEchoHandler),new string[0],TimeSpan.FromMinutes(2));
				Assert.AreEqual(91,probeRunResult.ExitCode,"No Run method: the host's own exit code. "+probeRunResult.Output);
				StringAssert.Contains(probeRunResult.Output,"declares no public static int Run(string[])");
				Assert.AreEqual(0,ListCopies(folder).Count,"Each run deletes its copy.");
			}
			finally {
				DeleteQuietly(folder);
			}
		}
	}

	///<summary>The handler of Start_DeletesTheStaleCopiesFirst_AndDisposeLeavesNoCopy, in the child process. No MSTest type here.</summary>
	public static class ProbeEchoHandler {
		public const string COMMAND_ECHO="Echo";

		public static string Handle(string command,string[] arrayArgs) {
			if(command==COMMAND_ECHO) {
				return string.Join("|",arrayArgs)+"|"+Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory).TrimEnd('\\');
			}
			throw new ArgumentException("Unknown command "+command);
		}
	}

	///<summary>The entry method of RunOnce_IsTheProcesssMain, in the child process. No MSTest type here.</summary>
	public static class ProbeOnceEntry {
		public static int Run(string[] args) {
			switch(args[0]) {
				case "exit":
					Console.WriteLine("args: "+args[1]);
					Console.WriteLine("base: "+Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory).TrimEnd('\\'));
					Console.WriteLine("apartment: "+Thread.CurrentThread.GetApartmentState());
					Console.Error.WriteLine("to standard error");
					return 7;
				case "foreground":
					new Thread(() => {
						Thread.Sleep(2000);
						Console.WriteLine("foreground thread ended");
					}).Start();
					return 0;
				case "background":
					new Thread(() => Thread.Sleep(Timeout.Infinite)) { IsBackground=true }.Start();
					return 0;
				case "throw":
					throw new InvalidOperationException("A synthetic failure out of Run.");
				case "hang":
					Thread.Sleep(Timeout.Infinite);
					return 0;
				default:
					return 2;
			}
		}
	}
}
