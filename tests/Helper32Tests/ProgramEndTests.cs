// No helper is left after the program's process ends, and each program process gets its own helper (S3a-13 of the .NET 10 move,
// docs/port-notes/S3a-13.md, section 8). The program processes are TestProbeHost children that start their helpers through Helper32Client.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Helper32Tests.Support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestProbeHost;

namespace Helper32Tests {
	[TestClass]
	public class ProgramEndTests:HelperTestBase {
		private static ProbeProcess StartProgram() {
			string pathConfig=typeof(ProgramProbe).Assembly.Location+".config";
			string configText=File.Exists(pathConfig) ? File.ReadAllText(pathConfig) : null;
			return ProbeProcess.Start(HelperCopy.Folder,configText,typeof(ProgramProbe),TimeSpan.FromMinutes(2));
		}

		private static bool WaitForEnd(int processId,string exeFile,TimeSpan timeSpan) {
			DateTime dateTimeDeadline=DateTime.UtcNow+timeSpan;
			while(DateTime.UtcNow<dateTimeDeadline) {
				if(!ProcessList.GetAll().Any(x => x.ProcessId==processId && string.Equals(x.ExeFile,exeFile,StringComparison.OrdinalIgnoreCase))) {
					return true;
				}
				Thread.Sleep(100);
			}
			return false;
		}

		[TestMethod]
		public void ProgramProcessEndsAbruptly_ItsHelperEndsToo() {
			HelperCopy helperCopy=NewCopy();
			using(ProbeProcess probeProcess=StartProgram()) {
				int processIdProgram=int.Parse(probeProcess.Call("Pid"),CultureInfo.InvariantCulture);
				int processIdHelper=int.Parse(probeProcess.Call("Start",helperCopy.PathExe),CultureInfo.InvariantCulture);
				Assert.AreEqual(processIdProgram,ProcessList.GetParentId(processIdHelper),"The program process started its helper.");
				Assert.AreEqual("1",probeProcess.Call("Count"));
				probeProcess.Call("Die");
				Assert.IsTrue(WaitForEnd(processIdHelper,helperCopy.FileName,TimeSpan.FromSeconds(20)),"The helper ended with its program process.");
			}
			Assert.AreEqual(0,helperCopy.GetProcesses().Count);
		}

		///<summary>The helper is inside a vendor call when the program process ends: the pipe is not read then, so the job object (or the helper's
		///own watch on the program's process) ends it.</summary>
		[TestMethod]
		public void ProgramProcessEndsDuringACall_ItsHelperEndsToo() {
			HelperCopy helperCopy=NewCopy();
			using(ProbeProcess probeProcess=StartProgram()) {
				int processIdHelper=int.Parse(probeProcess.Call("Start",helperCopy.PathExe),CultureInfo.InvariantCulture);
				probeProcess.Call("SleepInHelper");
				probeProcess.Call("Die");
				Assert.IsTrue(WaitForEnd(processIdHelper,helperCopy.FileName,TimeSpan.FromSeconds(20)),"The busy helper ended with its program process.");
			}
			Assert.AreEqual(0,helperCopy.GetProcesses().Count);
		}

		[TestMethod]
		public void ProgramProcessEndsNormally_ItsHelperEndsToo() {
			HelperCopy helperCopy=NewCopy();
			int processIdHelper;
			using(ProbeProcess probeProcess=StartProgram()) {
				processIdHelper=int.Parse(probeProcess.Call("Start",helperCopy.PathExe),CultureInfo.InvariantCulture);
			}//closes the program process's input: it returns from Main and ends
			Assert.IsTrue(WaitForEnd(processIdHelper,helperCopy.FileName,TimeSpan.FromSeconds(20)),"The helper ended with its program process.");
		}

		[TestMethod]
		public void TwoProgramProcesses_EachGetsItsOwnHelper() {
			HelperCopy helperCopy=NewCopy();
			using(ProbeProcess probeProcessA=StartProgram())
			using(ProbeProcess probeProcessB=StartProgram()) {
				int processIdProgramA=int.Parse(probeProcessA.Call("Pid"),CultureInfo.InvariantCulture);
				int processIdProgramB=int.Parse(probeProcessB.Call("Pid"),CultureInfo.InvariantCulture);
				int processIdHelperA=int.Parse(probeProcessA.Call("Start",helperCopy.PathExe),CultureInfo.InvariantCulture);
				int processIdHelperB=int.Parse(probeProcessB.Call("Start",helperCopy.PathExe),CultureInfo.InvariantCulture);
				Assert.AreNotEqual(processIdHelperA,processIdHelperB);
				Assert.AreEqual(processIdProgramA,ProcessList.GetParentId(processIdHelperA));
				Assert.AreEqual(processIdProgramB,ProcessList.GetParentId(processIdHelperB));
				Assert.AreEqual("1",probeProcessA.Call("Count"));
				Assert.AreEqual("2",probeProcessA.Call("Count"));
				Assert.AreEqual("1",probeProcessB.Call("Count"),"B's helper has its own state.");
				//One program ending leaves the other's helper running.
				probeProcessA.Call("Die");
				Assert.IsTrue(WaitForEnd(processIdHelperA,helperCopy.FileName,TimeSpan.FromSeconds(20)));
				Assert.AreEqual("2",probeProcessB.Call("Count"));
				Assert.AreEqual(1,helperCopy.GetProcesses().Count);
			}
		}
	}
}
