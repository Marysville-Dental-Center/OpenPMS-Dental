// The helper's protocol with a fake module (S3a-13 of the .NET 10 move, docs/port-notes/S3a-13.md, section 8): start, call, reply, error, a
// crash during a call, a timeout, a missing helper, the pipe's name and security, and one helper per client. Each test starts copies of the
// helper from this folder and ends them; HelperTestBase checks that none is left.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using CodeBase;
using Helper32Tests.Support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalHelper32;

namespace Helper32Tests {
	[TestClass]
	public class HelperProcessTests:HelperTestBase {
		private static readonly TimeSpan _timeSpanCall=TimeSpan.FromSeconds(60);

		private Helper32Client NewClient(HelperCopy helperCopy) {
			return new Helper32Client(helperCopy.PathExe,HelperCopy.ArgsFakeModule);
		}

		[TestMethod]
		public void Call_StartsTheHelperOnce_AndReturnsTheModulesReply() {
			HelperCopy helperCopy=NewCopy();
			using(Helper32Client client=NewClient(helperCopy)) {
				Assert.AreEqual(0,client.ProcessId,"Nothing starts before the first call.");
				Assert.AreEqual(0,helperCopy.GetProcesses().Count);
				byte[] arrayData=Encoding.UTF8.GetBytes("bytes \u00e9\u4e2d");
				Helper32Result result=client.Call(FakeModule.MODULE,"Echo",new[] { "one","",null,"\u00e9 \u4e2d \r\n|" },arrayData,_timeSpanCall);
				CollectionAssert.AreEqual(new[] { "one","",null,"\u00e9 \u4e2d \r\n|" },result.Results,"Strings cross as they are, null and empty kept.");
				CollectionAssert.AreEqual(arrayData,result.Data);
				int processId=client.ProcessId;
				Assert.AreNotEqual(0,processId);
				Assert.AreEqual(1,helperCopy.GetProcesses().Count);
				Assert.AreEqual(processId,helperCopy.GetProcesses()[0].ProcessId);
				Assert.AreEqual(Process.GetCurrentProcess().Id,ProcessList.GetParentId(processId),"The client's process started the helper.");
				//The module's state lives as long as the helper: the second call goes to the same process.
				Assert.AreEqual("1",client.Call(FakeModule.MODULE,"Count",_timeSpanCall)[0]);
				Assert.AreEqual("2",client.Call(FakeModule.MODULE,"Count",_timeSpanCall)[0]);
				Assert.AreEqual(processId,client.ProcessId);
				string[] arrayPing=client.Call(Helper32Protocol.MODULE_HOST,Helper32Protocol.COMMAND_PING,_timeSpanCall);
				Assert.AreEqual(Helper32Protocol.VERSION.ToString(),arrayPing[0]);
				Assert.AreEqual(processId.ToString(),arrayPing[1]);
				Assert.AreEqual("0",arrayPing[2],"The helper is a 32-bit process.");
				//The module runs on the helper's one main thread, a single-threaded apartment, for every call.
				string[] arrayApartment=client.Call(FakeModule.MODULE,"Apartment",_timeSpanCall);
				Assert.AreEqual("STA",arrayApartment[0]);
				CollectionAssert.AreEqual(arrayApartment,client.Call(FakeModule.MODULE,"Apartment",_timeSpanCall));
			}
			Assert.AreEqual(0,helperCopy.GetProcesses().Count,"Dispose ends the helper (it closes the pipe, and the helper ends by itself).");
		}

		[TestMethod]
		public void Call_ModuleThrows_IsARemoteErrorWithItsMessage_AndTheHelperKeepsRunning() {
			HelperCopy helperCopy=NewCopy();
			using(Helper32Client client=NewClient(helperCopy)) {
				client.Call(FakeModule.MODULE,"Count",_timeSpanCall);
				int processId=client.ProcessId;
				Helper32Exception exception=Assert.ThrowsExactly<Helper32Exception>(() => client.Call(FakeModule.MODULE,"Throw",_timeSpanCall,
					"Synthetic vendor failure \u00e9"));
				Assert.AreEqual(Helper32Failure.Remote,exception.Failure);
				Assert.AreEqual("Synthetic vendor failure \u00e9",exception.Message,"The program shows the vendor code's message as before.");
				Assert.AreEqual(typeof(InvalidOperationException).FullName,exception.RemoteTypeName);
				//An unknown module or command is an error of the same kind.
				Assert.AreEqual(Helper32Failure.Remote,Assert.ThrowsExactly<Helper32Exception>(() => client.Call("NoSuchModule","X",_timeSpanCall)).Failure);
				Assert.AreEqual(Helper32Failure.Remote,Assert.ThrowsExactly<Helper32Exception>(() => client.Call(Helper32Protocol.MODULE_HOST,"X",
					_timeSpanCall)).Failure);
				Assert.AreEqual("2",client.Call(FakeModule.MODULE,"Count",_timeSpanCall)[0],"The same helper answers after the errors.");
				Assert.AreEqual(processId,client.ProcessId);
			}
		}

		[TestMethod]
		public void Call_AStringTheProtocolCannotEncode_IsAProtocolError_AndNothingIsSent() {
			//S3a-14f: the strict UTF-8 throws EncoderFallbackException for a lone surrogate (note text cut in the middle of an emoji), which Call
			//let through before. The message is built in memory first, so nothing reaches the helper, which keeps answering in order.
			HelperCopy helperCopy=NewCopy();
			using(Helper32Client client=NewClient(helperCopy)) {
				Assert.AreEqual("1",client.Call(FakeModule.MODULE,"Count",_timeSpanCall)[0]);
				int processId=client.ProcessId;
				foreach(string value in new[] { "note \uD83D","\uDE00 note" }) {
					Helper32Exception exception=Assert.ThrowsExactly<Helper32Exception>(() => client.Call(FakeModule.MODULE,"Echo",new[] { "ok",value },
						null,_timeSpanCall));
					Assert.AreEqual(Helper32Failure.Protocol,exception.Failure);
					StringAssert.StartsWith(exception.Message,"A text of Test Echo cannot be sent to the 32-bit helper program: ");
					StringAssert.EndsWith(exception.Message," Nothing was sent.");
					Assert.IsInstanceOfType(exception.InnerException,typeof(EncoderFallbackException));
				}
				Assert.ThrowsExactly<Helper32Exception>(() => client.CallPumping(FakeModule.MODULE,"Echo",new[] { "\uD83D" },null,_timeSpanCall,
					isReactivating:false),"The pumping call too (Schick's and the signing window's).");
				Assert.AreEqual("2",client.Call(FakeModule.MODULE,"Count",_timeSpanCall)[0],"The same helper, in step: none of those requests reached it.");
				Assert.AreEqual(processId,client.ProcessId);
				//A whole emoji is well-formed and crosses as before.
				CollectionAssert.AreEqual(new[] { "emoji 😀" },client.Call(FakeModule.MODULE,"Echo",_timeSpanCall,"emoji 😀"));
				//A reply the helper cannot encode: the helper answers with the reason instead of ending.
				Helper32Exception exceptionReply=Assert.ThrowsExactly<Helper32Exception>(() => client.Call(FakeModule.MODULE,"LoneSurrogate",_timeSpanCall));
				Assert.AreEqual(Helper32Failure.Remote,exceptionReply.Failure);
				Assert.AreEqual(typeof(EncoderFallbackException).FullName,exceptionReply.RemoteTypeName);
				Assert.AreEqual("3",client.Call(FakeModule.MODULE,"Count",_timeSpanCall)[0]);
				Assert.AreEqual(processId,client.ProcessId);
			}
		}

		[TestMethod]
		public void Call_HelperEndsDuringTheCall_IsReportedOnce_AndTheNextCallStartsANewHelper() {
			HelperCopy helperCopy=NewCopy();
			using(Helper32Client client=NewClient(helperCopy)) {
				client.Call(FakeModule.MODULE,"Count",_timeSpanCall);
				int processIdFirst=client.ProcessId;
				Helper32Exception exception=Assert.ThrowsExactly<Helper32Exception>(() => client.Call(FakeModule.MODULE,"Exit",_timeSpanCall,"77"));
				Assert.AreEqual(Helper32Failure.Crashed,exception.Failure);
				StringAssert.Contains(exception.Message,"ended during Test Exit (exit code 77)");
				StringAssert.Contains(exception.Message,"started again on the next call");
				Assert.AreEqual(0,client.ProcessId);
				//The next call starts a new helper (the module's state starts again) and is answered.
				Assert.AreEqual("1",client.Call(FakeModule.MODULE,"Count",_timeSpanCall)[0]);
				int processIdSecond=client.ProcessId;
				Assert.AreNotEqual(processIdFirst,processIdSecond);
				//An abrupt end (a crash of vendor code) is reported the same way.
				exception=Assert.ThrowsExactly<Helper32Exception>(() => client.Call(FakeModule.MODULE,"Kill",_timeSpanCall));
				Assert.AreEqual(Helper32Failure.Crashed,exception.Failure);
				Assert.AreEqual("1",client.Call(FakeModule.MODULE,"Count",_timeSpanCall)[0]);
				Assert.AreEqual(1,helperCopy.GetProcesses().Count,"Only the newest helper runs.");
			}
		}

		[TestMethod]
		public void Call_HelperEndedBetweenCalls_IsStartedAgainWithoutAnError() {
			HelperCopy helperCopy=NewCopy();
			using(Helper32Client client=NewClient(helperCopy)) {
				client.Call(FakeModule.MODULE,"Count",_timeSpanCall);
				using(Process process=Process.GetProcessById(client.ProcessId)) {
					process.Kill();//the helper this test started
					process.WaitForExit(10000);
				}
				Assert.AreEqual("1",client.Call(FakeModule.MODULE,"Count",_timeSpanCall)[0]);
			}
		}

		[TestMethod]
		public void Call_NotAnsweredInTime_TimesOut_EndsTheHelper_AndTheNextCallStartsANewOne() {
			HelperCopy helperCopy=NewCopy();
			using(Helper32Client client=NewClient(helperCopy)) {
				client.Call(FakeModule.MODULE,"Count",_timeSpanCall);
				int processIdFirst=client.ProcessId;
				Stopwatch stopwatch=Stopwatch.StartNew();
				Helper32Exception exception=Assert.ThrowsExactly<Helper32Exception>(() => client.Call(FakeModule.MODULE,"Sleep",TimeSpan.FromSeconds(2),
					"120000"));
				stopwatch.Stop();
				Assert.AreEqual(Helper32Failure.TimedOut,exception.Failure);
				StringAssert.Contains(exception.Message,"did not answer Test Sleep within 2 seconds");
				Assert.IsTrue(stopwatch.Elapsed<TimeSpan.FromSeconds(30),"The timeout ends the wait: "+stopwatch.Elapsed);
				Assert.IsFalse(ProcessList.GetAll().Any(x => x.ProcessId==processIdFirst && x.ExeFile==helperCopy.FileName),"The helper that did not "
					+"answer was ended.");
				Assert.AreEqual("1",client.Call(FakeModule.MODULE,"Count",_timeSpanCall)[0]);
				Assert.AreNotEqual(processIdFirst,client.ProcessId);
			}
		}

		[TestMethod]
		public void Call_HelperMissing_GivesAClearMessage_AndStartsNothing() {
			string pathMissing=Path.Combine(HelperCopy.Folder,"OpenPmsDentalHelper32-000000000000.exe");
			Assert.IsFalse(File.Exists(pathMissing));
			using(Helper32Client client=new Helper32Client(pathMissing)) {
				Helper32Exception exception=Assert.ThrowsExactly<Helper32Exception>(() => client.Call(FakeModule.MODULE,"Count",_timeSpanCall));
				Assert.AreEqual(Helper32Failure.Missing,exception.Failure);
				StringAssert.Contains(exception.Message,"OpenPmsDentalHelper32.exe was not found at "+pathMissing);
				StringAssert.Contains(exception.Message,"Reinstall");
				Assert.AreEqual(0,client.ProcessId);
				//Every call says so again (nothing is cached), and none throws anything else.
				Assert.AreEqual(Helper32Failure.Missing,Assert.ThrowsExactly<Helper32Exception>(() => client.Call(FakeModule.MODULE,"Count",_timeSpanCall)).Failure);
			}
		}

		[TestMethod]
		public void Call_AfterDispose_Throws() {
			HelperCopy helperCopy=NewCopy();
			Helper32Client client=NewClient(helperCopy);
			client.Call(FakeModule.MODULE,"Count",_timeSpanCall);
			client.Dispose();
			client.Dispose();
			Assert.ThrowsExactly<ObjectDisposedException>(() => client.Call(FakeModule.MODULE,"Count",_timeSpanCall));
		}

		[TestMethod]
		public void TwoClients_GetTwoHelpers_WithPipesOfTheirOwn() {
			HelperCopy helperCopy=NewCopy();
			using(Helper32Client clientA=NewClient(helperCopy))
			using(Helper32Client clientB=NewClient(helperCopy)) {
				Assert.AreEqual("1",clientA.Call(FakeModule.MODULE,"Count",_timeSpanCall)[0]);
				Assert.AreEqual("1",clientB.Call(FakeModule.MODULE,"Count",_timeSpanCall)[0]);
				Assert.AreEqual("2",clientA.Call(FakeModule.MODULE,"Count",_timeSpanCall)[0]);
				Assert.AreNotEqual(clientA.ProcessId,clientB.ProcessId);
				Assert.AreNotEqual(clientA.PipeName,clientB.PipeName);
				Assert.AreEqual(2,helperCopy.GetProcesses().Count);
			}
		}

		[TestMethod]
		public void Pipe_IsNamedAtRandom_ForTheProgramsProcess() {
			HelperCopy helperCopy=NewCopy();
			using(Helper32Client client=NewClient(helperCopy)) {
				client.Call(FakeModule.MODULE,"Count",_timeSpanCall);
				string pipeName=client.PipeName;
				Assert.IsTrue(Regex.IsMatch(pipeName,"^OpenPmsDentalHelper32-"+Process.GetCurrentProcess().Id+"-[0-9a-f]{32}$"),pipeName);
				Assert.IsTrue(Helper32Protocol.IsPipeName(pipeName));
				//The next helper gets a new name.
				using(Process process=Process.GetProcessById(client.ProcessId)) {
					process.Kill();
					process.WaitForExit(10000);
				}
				client.Call(FakeModule.MODULE,"Count",_timeSpanCall);
				Assert.AreNotEqual(pipeName,client.PipeName);
			}
		}

		///<summary>The pipe, read by the helper from its own end: owned by the current user, with a protected DACL (no inherited entries) whose only
		///entry allows the current user to read and write. No Everyone, no other user or group, no network or anonymous entry.</summary>
		[TestMethod]
		public void Pipe_OnlyTheCurrentUserMayOpenIt() {
			HelperCopy helperCopy=NewCopy();
			using(Helper32Client client=NewClient(helperCopy)) {
				string sddl=client.Call(Helper32Protocol.MODULE_HOST,Helper32Protocol.COMMAND_PIPE_SECURITY,_timeSpanCall)[0];
				string sid=WindowsIdentity.GetCurrent().User.Value;
				Match match=Regex.Match(sddl,@"^O:(?<owner>[^D]+)D:(?<flags>[A-Z]*)(?<aces>(\([^)]*\))*)$");
				Assert.IsTrue(match.Success,sddl);
				Assert.AreEqual(sid,match.Groups["owner"].Value,sddl);
				StringAssert.Contains(match.Groups["flags"].Value,"P","A protected DACL: nothing inherited. "+sddl);
				string[] arrayAces=Regex.Matches(match.Groups["aces"].Value,@"\(([^)]*)\)").Cast<Match>().Select(x => x.Groups[1].Value).ToArray();
				Assert.AreEqual(1,arrayAces.Length,sddl);
				string[] arrayParts=arrayAces[0].Split(';');
				Assert.AreEqual("A",arrayParts[0],"An allow entry. "+sddl);
				Assert.AreEqual(sid,arrayParts[5],"For the current user only. "+sddl);
				foreach(string other in new[] { "WD","AN","NU","BU","AU","IU","S-1-1-0" }) {
					Assert.IsFalse(arrayAces.Any(x => x.EndsWith(";"+other)),other+" in "+sddl);
				}
			}
		}

		///<summary>The pipe's create call, in the client's source: the local machine's namespace, one instance (created first or not at all),
		///remote clients rejected, not inherited.</summary>
		[TestMethod]
		public void Pipe_IsCreatedLocal_FirstInstance_RejectingRemoteClients() {
			string text=File.ReadAllText(Path.Combine(TestPaths.RepoRoot,"CodeBase","Utilities","Helper32Client.cs"));
			StringAssert.Contains(text,@"NativeMethods.CreateNamedPipe(@""\\.\pipe\""+pipeName,");
			StringAssert.Contains(text,"NativeMethods.PIPE_ACCESS_DUPLEX|NativeMethods.FILE_FLAG_FIRST_PIPE_INSTANCE|NativeMethods.FILE_FLAG_OVERLAPPED,");
			StringAssert.Contains(text,"NativeMethods.PIPE_TYPE_BYTE|NativeMethods.PIPE_READMODE_BYTE|NativeMethods.PIPE_WAIT|NativeMethods.PIPE_REJECT_REMOTE_CLIENTS,");
			StringAssert.Contains(text,"1,65536,65536,0,ref securityAttributes);");
			StringAssert.Contains(text,"securityAttributes.bInheritHandle=0;");
			StringAssert.Contains(text,"public const uint PIPE_REJECT_REMOTE_CLIENTS=0x00000008;");
			StringAssert.Contains(text,"public const uint FILE_FLAG_FIRST_PIPE_INSTANCE=0x00080000;");
			//The helper connects to the local machine only, and only to a pipe of the program's form.
			string textHelper=File.ReadAllText(Path.Combine(TestPaths.RepoRoot,"OpenPmsDentalHelper32","Program.cs"));
			StringAssert.Contains(textHelper,"new NamedPipeClientStream(\".\",helperArguments.PipeName,");
			StringAssert.Contains(textHelper,"TokenImpersonationLevel.Identification");
		}

		///<summary>Started any other way than by the program, the helper ends at once with its usage exit code and shows no window.</summary>
		[TestMethod]
		public void Helper_WithoutTheProgramsArguments_EndsAtOnce() {
			HelperCopy helperCopy=NewCopy();
			string pipeName=Helper32Protocol.NewPipeName(Process.GetCurrentProcess().Id);
			foreach(string arguments in new[] { "","--pipe "+pipeName,"--parent "+Process.GetCurrentProcess().Id,"--pipe x --parent "+Process.GetCurrentProcess().Id,
				"--pipe "+Helper32Protocol.NewPipeName(1)+" --parent "+Process.GetCurrentProcess().Id,"--pipe "+pipeName+" --parent "+Process.GetCurrentProcess().Id
				+" --unknown" })
			{
				Assert.AreEqual(Helper32Protocol.EXIT_USAGE,RunHelper(helperCopy,arguments),"Arguments: "+arguments);
			}
			//The program's arguments, but no program made that pipe.
			Assert.AreEqual(Helper32Protocol.EXIT_CONNECT,RunHelper(helperCopy,"--pipe "+pipeName+" --parent "+Process.GetCurrentProcess().Id,
				TimeSpan.FromSeconds(60)),"No pipe of that name: the helper gives up after its connect timeout.");
		}

		private static int RunHelper(HelperCopy helperCopy,string arguments,TimeSpan? timeSpanWait=null) {
			ProcessStartInfo processStartInfo=new ProcessStartInfo(helperCopy.PathExe,arguments);
			processStartInfo.UseShellExecute=false;
			processStartInfo.CreateNoWindow=true;
			using(Process process=Process.Start(processStartInfo)) {
				if(!process.WaitForExit(2000)) {
					//Still waiting for its pipe: it shows no window meanwhile.
					process.Refresh();
					try {
						Assert.AreEqual(IntPtr.Zero,process.MainWindowHandle,"No window.");
					}
					catch(InvalidOperationException) {
						//it ended just now
					}
				}
				Assert.IsTrue(process.WaitForExit((int)(timeSpanWait??TimeSpan.FromSeconds(20)).TotalMilliseconds),"The helper ended.");
				return process.ExitCode;
			}
		}

		///<summary>The helper is a Windows program without a console (no console window opens when it starts) and an x86 file.</summary>
		[TestMethod]
		public void HelperExe_IsAWindowsProgramWithoutAConsole_ForX86() {
			byte[] arrayBytes=File.ReadAllBytes(Path.Combine(HelperCopy.Folder,HelperCopy.HELPER_FILE_NAME));
			int offsetPe=BitConverter.ToInt32(arrayBytes,0x3C);
			Assert.AreEqual(0x4550,BitConverter.ToInt32(arrayBytes,offsetPe),"PE signature");
			Assert.AreEqual(0x14C,BitConverter.ToUInt16(arrayBytes,offsetPe+4),"IMAGE_FILE_MACHINE_I386");
			int offsetOptional=offsetPe+24;
			Assert.AreEqual(2,BitConverter.ToUInt16(arrayBytes,offsetOptional+68),"IMAGE_SUBSYSTEM_WINDOWS_GUI: no console window.");
		}

		[TestMethod]
		public void TestModule_LoadsOnlyATestAssemblyOfTheHelpersFolder() {
			HelperCopy helperCopy=NewCopy();
			foreach(string argumentsModule in new[] { "--test-module CodeBase CodeBase.Helper32Client","--test-module ..\\Helper32Tests X",
				"--test-module Helper32Tests "+typeof(FakeModule).FullName+" "+Helper32Protocol.MODULE_HOST,"--test-module NoSuchTests X" })
			{
				using(Helper32Client client=new Helper32Client(helperCopy.PathExe,argumentsModule)) {
					Helper32Exception exception=Assert.ThrowsExactly<Helper32Exception>(() => client.Call(FakeModule.MODULE,"Count",_timeSpanCall));
					Assert.AreEqual(Helper32Failure.StartFailed,exception.Failure,argumentsModule);
					StringAssert.Contains(exception.Message,"exit code "+Helper32Protocol.EXIT_USAGE,argumentsModule);
				}
			}
		}
	}
}
