// Windows services without InstallUtil (S3a-7 of the .NET 10 move, docs/port-notes/S3a-7.md): ServicesHelper.Install and Uninstall run
// sc.exe create and sc.exe delete with the settings the HL7 service's ProjectInstaller gave through InstallUtil.exe. Safety: no test here
// creates, deletes, starts, stops or configures a real Windows service, none runs sc.exe, and none creates or deletes a real event log
// source. Every test replaces ServicesHelper's process start, registry read, service stop and event log source registration and removal
// (S3a-FU1) with fakes that only record the calls and answer with a canned result, and puts the real ones back after it; the tests compare
// the recorded calls exactly, so an unexpected call fails them.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ReleaseFlowTests {
	[TestClass]
	[DoNotParallelize]
	public class ServiceInstallTests {
		///<summary>The HL7 service's exe in the default install folder. Only a path: FileInfo does not touch the disk, and nothing runs it.</summary>
		private static readonly string _pathHL7Exe=@"C:\Program Files (x86)\Open PMS - Dental\OpenPmsDentalHL7\OpenPmsDentalHL7.exe";

		private Func<string,string,string,ServiceProcessResult> _funcRunProcessReal;
		private Func<string,string> _funcGetImagePathReal;
		private Func<string,bool> _funcStopServiceReal;
		private Action<string> _actionRegisterEventLogSourceReal;
		private Action<string> _actionRemoveEventLogSourceReal;
		///<summary>Every call of the fakes, in order: "run|file|arguments|working directory", "imagepath|name", "stop|name",
		///"eventlog-register|source", "eventlog-remove|source".</summary>
		private List<string> _listCalls;
		///<summary>What the fake process start answers.</summary>
		private ServiceProcessResult _serviceProcessResult;
		///<summary>What the fake registry read answers for any name (null: no such service).</summary>
		private string _imagePath;

		[TestInitialize]
		public void SetUp() {
			_funcRunProcessReal=ServicesHelper.FuncRunProcess;
			_funcGetImagePathReal=ServicesHelper.FuncGetImagePath;
			_funcStopServiceReal=ServicesHelper.FuncStopService;
			_actionRegisterEventLogSourceReal=ServicesHelper.ActionRegisterEventLogSource;
			_actionRemoveEventLogSourceReal=ServicesHelper.ActionRemoveEventLogSource;
			_listCalls=new List<string>();
			_serviceProcessResult=new ServiceProcessResult { StandardOutput="[SC] CreateService SUCCESS\r\n",ExitCode=0 };
			_imagePath=null;
			ServicesHelper.FuncRunProcess=(fileName,arguments,workingDirectory) => {
				_listCalls.Add("run|"+fileName+"|"+arguments+"|"+workingDirectory);
				return _serviceProcessResult;
			};
			ServicesHelper.FuncGetImagePath=serviceName => {
				_listCalls.Add("imagepath|"+serviceName);
				return _imagePath;
			};
			ServicesHelper.FuncStopService=serviceName => {
				_listCalls.Add("stop|"+serviceName);
				return true;
			};
			ServicesHelper.ActionRegisterEventLogSource=source => _listCalls.Add("eventlog-register|"+source);
			ServicesHelper.ActionRemoveEventLogSource=source => _listCalls.Add("eventlog-remove|"+source);
		}

		[TestCleanup]
		public void TearDown() {
			ServicesHelper.FuncRunProcess=_funcRunProcessReal;
			ServicesHelper.FuncGetImagePath=_funcGetImagePathReal;
			ServicesHelper.FuncStopService=_funcStopServiceReal;
			ServicesHelper.ActionRegisterEventLogSource=_actionRegisterEventLogSourceReal;
			ServicesHelper.ActionRemoveEventLogSource=_actionRemoveEventLogSourceReal;
		}

		private static string ScExe {
			get { return Path.Combine(Environment.SystemDirectory,"sc.exe"); }
		}

		///<summary>The settings OpenPmsDentalHL7/ProjectInstaller.cs (deleted in S3a-7) gave the service through InstallUtil, pinned:
		///ServiceProcessInstaller.Account=LocalSystem, ServiceInstaller.StartType=Automatic (DelayedAutoStart left false), the service name
		///ProgramNames.HL7ServiceName unless /ServiceName= named another, no DisplayName (Windows shows the service name) and no Description.
		///ServiceInstaller itself made it a Win32OwnProcess service with SERVICE_ERROR_NORMAL, and the ImagePath the exe's path in quotes.</summary>
		[TestMethod]
		public void ScCreate_HasTheProjectInstallersSettings() {
			Assert.AreEqual("OpenPmsDentalHL7",ProgramNames.HL7ServiceName,"The HL7 service's default name.");
			Assert.AreEqual("own",ServicesHelper.ScServiceType);
			Assert.AreEqual("auto",ServicesHelper.ScStartType,"Automatic, not delayed-auto (\"delayed-auto\") and not demand.");
			Assert.AreEqual("normal",ServicesHelper.ScErrorControl);
			Assert.AreEqual("LocalSystem",ServicesHelper.ScAccount);
			Assert.AreEqual("create \"OpenPmsDentalHL7\" binPath= \"\\\""+_pathHL7Exe+"\\\"\" type= own start= auto error= normal obj= LocalSystem"
				+" DisplayName= \"OpenPmsDentalHL7\"",
				ServicesHelper.GetScCreateArguments(ProgramNames.HL7ServiceName,_pathHL7Exe));
			//Another name (ServiceManager's name box, InstallUtil's /ServiceName=): the same settings under that name, display name included.
			Assert.AreEqual("create \"OpenPmsDentalHL7Clinic2\" binPath= \"\\\""+_pathHL7Exe+"\\\"\" type= own start= auto error= normal obj= LocalSystem"
				+" DisplayName= \"OpenPmsDentalHL7Clinic2\"",
				ServicesHelper.GetScCreateArguments("OpenPmsDentalHL7Clinic2",_pathHL7Exe));
		}

		///<summary>Install runs exactly one process: the system folder's sc.exe create (no sc description: the installer set none, no
		///InstallUtil), from the exe's folder, and hands sc.exe's output and exit code back. After the create it registers the event log source
		///named after the service, as InstallUtil's ServiceInstaller did (S3a-FU1); for the HL7 service's default name that is also
		///ProgramNames.EventLogSourceHL7, so one source.</summary>
		[TestMethod]
		public void Install_RunsScCreateOnly_ThenRegistersTheEventLogSource() {
			ServicesHelper.Install(ProgramNames.HL7ServiceName,new FileInfo(_pathHL7Exe),out string standardOutput,out int exitCode);
			CollectionAssert.AreEqual(new List<string> {
				"run|"+ScExe+"|"+ServicesHelper.GetScCreateArguments(ProgramNames.HL7ServiceName,_pathHL7Exe)+"|"+Path.GetDirectoryName(_pathHL7Exe),
				"eventlog-register|OpenPmsDentalHL7",
			},_listCalls,string.Join("; ",_listCalls));
			Assert.AreEqual(ProgramNames.HL7ServiceName,ProgramNames.EventLogSourceHL7);
			Assert.IsTrue(Path.IsPathRooted(ScExe) && ScExe.StartsWith(Environment.SystemDirectory,StringComparison.OrdinalIgnoreCase));
			Assert.AreEqual(ScExe,ServicesHelper.GetScExePath());
			Assert.AreEqual(0,exitCode);
			Assert.AreEqual("[SC] CreateService SUCCESS\r\n",standardOutput);
			Assert.AreEqual(0,ServicesHelper.GetScErrorCode(exitCode,standardOutput));
			Assert.AreEqual("",ServicesHelper.GetScErrorText(0));
		}

		///<summary>sc.exe create for a name already installed: exit code 1073 and its text, and the caller sees both.</summary>
		[TestMethod]
		public void Install_ServiceExists_Returns1073() {
			_serviceProcessResult=new ServiceProcessResult { StandardOutput="[SC] CreateService FAILED 1073:\r\n\r\nThe specified service already exists.\r\n",
				ExitCode=1073 };
			ServicesHelper.Install(ProgramNames.HL7ServiceName,new FileInfo(_pathHL7Exe),out string standardOutput,out int exitCode);
			Assert.AreEqual(1073,exitCode);
			Assert.AreEqual(ServicesHelper.ScErrorServiceExists,ServicesHelper.GetScErrorCode(exitCode,standardOutput));
			StringAssert.Contains(ServicesHelper.GetScErrorText(1073),"already installed");
			StringAssert.Contains(standardOutput,"FAILED 1073");
			Assert.AreEqual(1,_listCalls.Count,"No event log source is registered when nothing was installed: "+string.Join("; ",_listCalls));
		}

		///<summary>The HL7 exe under another name registers the source of that name (ServiceBase's start and stop entries) and the HL7 messages'
		///source; another exe only the source of its name (S3a-FU1).</summary>
		[TestMethod]
		public void Install_RegistersTheSourcesOfTheNameAndOfTheHL7Messages() {
			ServicesHelper.Install("OpenPmsDentalHL7Clinic2",new FileInfo(_pathHL7Exe),out string _,out int _);
			CollectionAssert.AreEqual(new List<string> {
				"run|"+ScExe+"|"+ServicesHelper.GetScCreateArguments("OpenPmsDentalHL7Clinic2",_pathHL7Exe)+"|"+Path.GetDirectoryName(_pathHL7Exe),
				"eventlog-register|OpenPmsDentalHL7Clinic2",
				"eventlog-register|OpenPmsDentalHL7",
			},_listCalls,string.Join("; ",_listCalls));
			string pathOtherExe=@"C:\Program Files (x86)\Open PMS - Dental\OpenDentalService\OpenDentalService.exe";
			_listCalls.Clear();
			ServicesHelper.Install("OpenPmsDentalService",new FileInfo(pathOtherExe),out string _,out int _);
			CollectionAssert.AreEqual(new List<string> {
				"run|"+ScExe+"|"+ServicesHelper.GetScCreateArguments("OpenPmsDentalService",pathOtherExe)+"|"+Path.GetDirectoryName(pathOtherExe),
				"eventlog-register|OpenPmsDentalService",
			},_listCalls,string.Join("; ",_listCalls));
			CollectionAssert.AreEqual(new List<string> { "OpenPmsDentalHL7" },
				ServicesHelper.GetEventLogSourcesToRegister("OpenPmsDentalHL7","openpmsdentalhl7.EXE"),"Case does not matter, and no source twice.");
			Assert.AreEqual("Application",ServicesHelper.EventLogNameApplication);
		}

		///<summary>sc.exe run without administrator rights: exit code 5, and the text says to run as administrator.</summary>
		[TestMethod]
		public void Install_AccessDenied_Returns5() {
			_serviceProcessResult=new ServiceProcessResult { StandardOutput="[SC] OpenSCManager FAILED 5:\r\n\r\nAccess is denied.\r\n",ExitCode=5 };
			ServicesHelper.Install(ProgramNames.HL7ServiceName,new FileInfo(_pathHL7Exe),out string standardOutput,out int exitCode);
			Assert.AreEqual(5,exitCode);
			Assert.AreEqual(ServicesHelper.ScErrorAccessDenied,ServicesHelper.GetScErrorCode(exitCode,standardOutput));
			StringAssert.Contains(ServicesHelper.GetScErrorText(5),"administrator");
			Assert.AreEqual(1,_listCalls.Count,"Not elevated: no event log source is registered: "+string.Join("; ",_listCalls));
		}

		///<summary>Uninstall reads the service's ImagePath, runs sc.exe delete, and only after a successful delete stops the service (Windows
		///removes it once it has stopped), in ServiceInstaller's order, then removes the event log source named after it (S3a-FU1). It reports
		///the sc.exe branch.</summary>
		[TestMethod]
		public void Uninstall_DeletesThenStops() {
			_imagePath=_pathHL7Exe;
			_serviceProcessResult=new ServiceProcessResult { StandardOutput="[SC] DeleteService SUCCESS\r\n",ExitCode=0 };
			ServicesHelper.Uninstall(ProgramNames.HL7ServiceName,out string standardOutput,out int exitCode,out bool isMySQLService);
			CollectionAssert.AreEqual(new List<string> {
				"imagepath|OpenPmsDentalHL7",
				"run|"+ScExe+"|delete \"OpenPmsDentalHL7\"|",
				"stop|OpenPmsDentalHL7",
				"eventlog-remove|OpenPmsDentalHL7",
			},_listCalls,string.Join("; ",_listCalls));
			Assert.AreEqual(0,exitCode);
			Assert.IsFalse(isMySQLService);
			Assert.AreEqual("delete \"OpenPmsDentalHL7\"",ServicesHelper.GetScDeleteArguments(ProgramNames.HL7ServiceName));
			//The overload without the branch does the same.
			_listCalls.Clear();
			ServicesHelper.Uninstall("OpenPmsDentalHL7Clinic2",out string _,out int _);
			CollectionAssert.AreEqual(new List<string> {
				"imagepath|OpenPmsDentalHL7Clinic2",
				"run|"+ScExe+"|delete \"OpenPmsDentalHL7Clinic2\"|",
				"stop|OpenPmsDentalHL7Clinic2",
				"eventlog-remove|OpenPmsDentalHL7Clinic2",
			},_listCalls,"Only the source of the name goes: another HL7 service may still write under OpenPmsDentalHL7. "+string.Join("; ",_listCalls));
		}

		///<summary>The stop after sc.exe delete waits 15 seconds, as long as InstallUtil's ServiceInstaller (10 seconds and a 5 second sleep),
		///not Stop's default 7 (S3a-FU1). The real seam is only read, never run: it would stop a real service.</summary>
		[TestMethod]
		public void Uninstall_StopWaitsAsLongAsServiceInstaller() {
			Assert.AreEqual(15,ServicesHelper.UninstallStopTimeoutSeconds);
			string source=File.ReadAllText(Path.Combine(TestPaths.RepoRoot,"CodeBase","Utilities","ServicesHelper.cs"));
			StringAssert.Contains(source,
				"FuncStopService=serviceName => Stop(GetServiceByServiceName(serviceName),timeoutSeconds:UninstallStopTimeoutSeconds);");
		}

		///<summary>ServiceManager reads Uninstall's result by the branch Uninstall reports (the registry's ImagePath), not by the path in its own
		///box (S3a-FU1).</summary>
		[TestMethod]
		public void ServiceManager_ReadsTheUninstallBranchFromUninstall() {
			string source=File.ReadAllText(Path.Combine(TestPaths.RepoRoot,"ServiceManager","FormServiceManage.cs"));
			int indexStart=source.IndexOf("private void butUninstall_Click(",StringComparison.Ordinal);
			int indexEnd=source.IndexOf("private void butStart_Click(",StringComparison.Ordinal);
			Assert.IsTrue(indexStart>0 && indexEnd>indexStart);
			string code=source.Substring(indexStart,indexEnd-indexStart);
			StringAssert.Contains(code,"ServicesHelper.Uninstall(textName.Text,out standardOutput,out exitCode,out isMySQLService);");
			StringAssert.Contains(code,"if(isMySQLService) {");
			Assert.AreEqual(1,code.Split(new[] { "IsMySQLService(textPathToExe.Text)" },StringSplitOptions.None).Length-1,
				"Only the path check before Uninstall reads the box.");
		}

		///<summary>No service with the name: sc.exe delete answers 1060, the service is not stopped, and the caller sees 1060.</summary>
		[TestMethod]
		public void Uninstall_ServiceDoesNotExist_Returns1060() {
			_imagePath=null;
			_serviceProcessResult=new ServiceProcessResult {
				StandardOutput="[SC] OpenService FAILED 1060:\r\n\r\nThe specified service does not exist as an installed service.\r\n",ExitCode=1060 };
			ServicesHelper.Uninstall("OpenPmsDentalNoSuchService",out string standardOutput,out int exitCode);
			CollectionAssert.AreEqual(new List<string> {
				"imagepath|OpenPmsDentalNoSuchService",
				"run|"+ScExe+"|delete \"OpenPmsDentalNoSuchService\"|",
			},_listCalls,string.Join("; ",_listCalls));
			Assert.AreEqual(1060,exitCode);
			Assert.AreEqual(ServicesHelper.ScErrorServiceDoesNotExist,ServicesHelper.GetScErrorCode(exitCode,standardOutput));
			StringAssert.Contains(ServicesHelper.GetScErrorText(1060),"No service");
		}

		///<summary>sc.exe delete without administrator rights: 5, and the service is left running (not stopped).</summary>
		[TestMethod]
		public void Uninstall_AccessDenied_DoesNotStop() {
			_imagePath=_pathHL7Exe;
			_serviceProcessResult=new ServiceProcessResult { StandardOutput="[SC] OpenService FAILED 5:\r\n\r\nAccess is denied.\r\n",ExitCode=5 };
			ServicesHelper.Uninstall(ProgramNames.HL7ServiceName,out string standardOutput,out int exitCode);
			Assert.AreEqual(5,exitCode);
			Assert.IsFalse(_listCalls.Contains("stop|OpenPmsDentalHL7"),string.Join("; ",_listCalls));
			Assert.IsFalse(_listCalls.Contains("eventlog-remove|OpenPmsDentalHL7"),"Not elevated: the source is left alone. "+string.Join("; ",_listCalls));
		}

		///<summary>A service already marked for deletion (1072, an earlier delete whose stop timed out): the caller sees 1072 and the service is
		///stopped, so that Windows removes it.</summary>
		[TestMethod]
		public void Uninstall_MarkedForDelete_StopsTheService() {
			_imagePath=_pathHL7Exe;
			_serviceProcessResult=new ServiceProcessResult {
				StandardOutput="[SC] DeleteService FAILED 1072:\r\n\r\nThe specified service has been marked for deletion.\r\n",ExitCode=1072 };
			ServicesHelper.Uninstall(ProgramNames.HL7ServiceName,out string standardOutput,out int exitCode);
			CollectionAssert.AreEqual(new List<string> {
				"imagepath|OpenPmsDentalHL7",
				"run|"+ScExe+"|delete \"OpenPmsDentalHL7\"|",
				"stop|OpenPmsDentalHL7",
				"eventlog-remove|OpenPmsDentalHL7",
			},_listCalls,string.Join("; ",_listCalls));
			Assert.AreEqual(ServicesHelper.ScErrorServiceMarkedForDelete,ServicesHelper.GetScErrorCode(exitCode,standardOutput));
		}

		///<summary>A MariaDB or MySQL service is removed as before: stopped, then mysqld.exe --remove from its bin folder, not sc.exe; its event
		///log source is left alone, and Uninstall reports the MySQL branch (S3a-FU1).</summary>
		[TestMethod]
		public void Uninstall_MySqlService_UsesMysqldRemove() {
			_imagePath=@"C:\Program Files\MariaDB 10.11\bin\mysqld.exe --defaults-file=C:\Program Files\MariaDB 10.11\data\my.ini MariaDB";
			ServicesHelper.Uninstall("MariaDB",out string standardOutput,out int exitCode,out bool isMySQLService);
			CollectionAssert.AreEqual(new List<string> {
				"imagepath|MariaDB",
				"stop|MariaDB",
				@"run|CMD.exe|/C mysqld.exe --remove MariaDB|C:\Program Files\MariaDB 10.11\bin",
			},_listCalls,string.Join("; ",_listCalls));
			Assert.IsTrue(isMySQLService);
			//No registry key: the sc.exe branch, whatever path a caller has.
			_imagePath=null;
			_serviceProcessResult=new ServiceProcessResult {
				StandardOutput="[SC] OpenService FAILED 1060:\r\n\r\nThe specified service does not exist as an installed service.\r\n",ExitCode=1060 };
			ServicesHelper.Uninstall("MariaDB",out string _,out int _,out bool isMySQLServiceGone);
			Assert.IsFalse(isMySQLServiceGone);
		}

		///<summary>The error of an sc.exe run: the exit code, else the number of a "FAILED nnnn:" line, else 0.</summary>
		[TestMethod]
		public void ScErrorCode_ReadsExitCodeThenOutput() {
			Assert.AreEqual(1072,ServicesHelper.GetScErrorCode(1072,""));
			Assert.AreEqual(1060,ServicesHelper.GetScErrorCode(0,"[SC] OpenService FAILED 1060:\r\n\r\nThe specified service does not exist."));
			Assert.AreEqual(0,ServicesHelper.GetScErrorCode(0,"[SC] DeleteService SUCCESS"));
			Assert.AreEqual(0,ServicesHelper.GetScErrorCode(0,null));
			StringAssert.Contains(ServicesHelper.GetScErrorText(1072),"marked for removal");
			StringAssert.Contains(ServicesHelper.GetScErrorText(1234),"1234");
		}

		///<summary>A name or path sc.exe would read wrongly is refused before any process starts.</summary>
		[TestMethod]
		public void ScArguments_RefuseQuotesAndBadNames() {
			foreach(string serviceName in new[] { "","  ",null,"Open\"PmsDental","OpenPmsDental/HL7",@"OpenPmsDental\HL7" }) {
				Assert.ThrowsExactly<ArgumentException>(() => ServicesHelper.GetScCreateArguments(serviceName,_pathHL7Exe),serviceName??"null");
				Assert.ThrowsExactly<ArgumentException>(() => ServicesHelper.GetScDeleteArguments(serviceName),serviceName??"null");
			}
			//A path ending in a backslash would turn the closing \" of binPath= into an escaped quote (S3a-FU1).
			foreach(string path in new[] { "","OpenPmsDentalHL7.exe",@"C:\Program Files\x"".exe",null,@"C:\Program Files (x86)\Open PMS - Dental\OpenPmsDentalHL7\" }) {
				Assert.ThrowsExactly<ArgumentException>(() => ServicesHelper.GetScCreateArguments(ProgramNames.HL7ServiceName,path),path??"null");
			}
			Assert.ThrowsExactly<ArgumentException>(() => ServicesHelper.Install("Open\"PmsDental",new FileInfo(_pathHL7Exe),out string _,out int _));
			Assert.AreEqual(0,_listCalls.Count,string.Join("; ",_listCalls));
		}

		///<summary>The safety rule of these tests: SetUp keeps the real seams so that TearDown can put them back, and the fakes are in place
		///while a test runs.</summary>
		[TestMethod]
		public void SetUp_KeepsTheRealSeams() {
			Assert.IsNotNull(_funcRunProcessReal);
			Assert.IsNotNull(_funcGetImagePathReal);
			Assert.IsNotNull(_funcStopServiceReal);
			Assert.AreNotSame(_funcRunProcessReal,ServicesHelper.FuncRunProcess);
			StringAssert.Contains(_funcRunProcessReal.Method.Name,"RunProcess");
			StringAssert.Contains(_funcGetImagePathReal.Method.Name,"GetImagePathFromRegistry");
			Assert.IsNotNull(_actionRegisterEventLogSourceReal);
			Assert.IsNotNull(_actionRemoveEventLogSourceReal);
			Assert.AreNotSame(_actionRegisterEventLogSourceReal,ServicesHelper.ActionRegisterEventLogSource);
			Assert.AreNotSame(_actionRemoveEventLogSourceReal,ServicesHelper.ActionRemoveEventLogSource);
			Assert.AreEqual("RegisterEventLogSource",_actionRegisterEventLogSourceReal.Method.Name);
			Assert.AreEqual("RemoveEventLogSource",_actionRemoveEventLogSourceReal.Method.Name);
		}
	}
}
