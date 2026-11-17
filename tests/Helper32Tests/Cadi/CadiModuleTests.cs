// Mediadent / CADI (N64-A3 of the native 64-bit integration survey, docs/port-notes/N64-A3.md): for each scenario the program's in-process path
// (WpfControlsOD/Bridges/CADIInProcess.cs, linked, driven as CADI.cs drives it) and the helper's Cadi module (linked, with the patient through
// the command's encoding) make the calls today's code made (CadiToday, CADI.cs before N64-A3), in the same order with the same values and
// types, and show the same messages; the main scenarios are pinned call by call. The in-process path's choice (NativeFirstChooser.Cadi) is
// checked for each registry case and each HRESULT that leads to the helper, for a 32-bit and a 64-bit chooser. No COM class is created and
// CADI is never started: CADI's object is a fake (FakeCadi). The windows are the hidden ones the code makes; none is shown, and each is
// destroyed at the end of its test.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental.Bridges;
using OpenPmsDentalHelper32;
using OpenPmsDentalHelper32.Cadi;

namespace Helper32Tests.Cadi {
	[TestClass]
	public class CadiModuleTests {
		private const int E_FAIL=unchecked((int)0x80004005);
		private static readonly DateTime TDATE_START=new DateTime(1899,12,30);
		private static readonly string _imageFolder=@"\\cadi.example.test\images";
		private readonly List<FakeCadi> _listFakes=new List<FakeCadi>();
		private static CADINativeWindow _windowInProcess;

		///<summary>The four failures that say this process cannot reach CADI's server.</summary>
		private static readonly int[] _arrayHResultsHelper=new[] { NativeFirstChooser.REGDB_E_CLASSNOTREG,NativeFirstChooser.CO_E_SERVER_EXEC_FAILURE,
			NativeFirstChooser.E_NOINTERFACE,NativeFirstChooser.TYPE_E_LIBNOTREGISTERED };

		private static readonly TodayPatient _patientOne=new TodayPatient { PatNum=1001,ChartNumber="CH-1001",LName="Synthetic",FName="Patient One",
			Birthdate=new DateTime(1980,5,17),Age=46,IsFemale=true,PatOrChartNum="0",ImageFolder=_imageFolder };
		private static readonly TodayPatient _patientTwo=new TodayPatient { PatNum=2002,ChartNumber="CH-2002",LName="Test\u00e9",FName="Zweite",
			Birthdate=new DateTime(1955,1,2),Age=71,IsFemale=false,PatOrChartNum="1",ImageFolder=_imageFolder };

		///<summary>One step of a scenario: CADI.SendData with a patient (or none), or CADI exiting.</summary>
		private sealed class Step {
			public bool IsCadiExit;
			public TodayPatient Patient;
		}

		private static Step Send(TodayPatient todayPatient) {
			return new Step { Patient=todayPatient };
		}

		private static readonly Step _stepSendNoPatient=new Step();
		private static readonly Step _stepCadiExits=new Step { IsCadiExit=true };

		private FakeCadi NewFake(Action<FakeCadi> script) {
			FakeCadi fakeCadi=new FakeCadi();
			script?.Invoke(fakeCadi);
			_listFakes.Add(fakeCadi);
			return fakeCadi;
		}

		[TestCleanup]
		public void DestroyTheWindows() {
			foreach(FakeCadi fakeCadi in _listFakes) {
				fakeCadi.DestroyWindows();
			}
			_windowInProcess=null;
			CadiToday.Reset();
		}

		///<summary>CADI.cs's GetCadiPatient (the values CADINativeWindow.SendData sent before N64-A3), for the in-process path and the module; checked
		///against CADI.cs's text in CadiCs_WorksOutThePatientsValuesAsToday.</summary>
		private static CadiPatient ToCadiPatient(TodayPatient pat) {
			if(pat==null) {
				return null;
			}
			string id="";
			if(pat.PatOrChartNum=="0") {
				id=pat.PatNum.ToString();
			}
			else {
				id=pat.ChartNumber;
			}
			CadiPatient cadiPatient=new CadiPatient();
			cadiPatient.Name=pat.LName+", "+pat.FName;
			cadiPatient.DateOfBirth=pat.Birthdate.Subtract(TDATE_START).TotalDays;
			cadiPatient.Age=pat.Age;
			cadiPatient.Sex=pat.IsFemale ? "F" : "M";
			string imagePath=pat.ImageFolder;
			cadiPatient.PicLibPath=CodeBase.ODFileUtils.CombinePaths(imagePath,id);
			return cadiPatient;
		}

		///<summary>A registration with these server keys.</summary>
		private static ComServerRegistration Registration(bool hasLocal64,bool hasLocal32,bool hasInproc64,bool hasInproc32) {
			ComServerRegistration comServerRegistration=new ComServerRegistration();
			comServerRegistration.ProgId=CADIInProcess.PROGRAM_ID;
			if(hasLocal64 || hasLocal32 || hasInproc64 || hasInproc32) {
				comServerRegistration.Clsid="{6F0C8E8A-2B1D-4C5E-9A3F-7D21B5E40C19}";//synthetic
			}
			comServerRegistration.HasLocalServer32View64=hasLocal64;
			comServerRegistration.HasLocalServer32View32=hasLocal32;
			comServerRegistration.HasInprocServer32View64=hasInproc64;
			comServerRegistration.HasInprocServer32View32=hasInproc32;
			return comServerRegistration;
		}

		///<summary>CADI as a 32-bit server program, registered in the 32-bit view (what the survey expects).</summary>
		private static ComServerRegistration RegistrationLocalServer32Bit() {
			return Registration(false,true,false,false);
		}

		///<summary>Runs the steps through today's code, the in-process path (a chooser for is64BitProcess, CADI registered as registration) and the
		///module, each with a fake CADI the same script drives, and checks that all three made the same calls and showed the same messages.
		///Returns today's calls.</summary>
		private List<string> RunAll(Action<FakeCadi> script,bool is64BitProcess,ComServerRegistration comServerRegistration,params Step[] arraySteps) {
			FakeCadi fakeToday=NewFake(script);
			CadiToday.Reset();
			CadiToday.FuncCreateComObject=fakeToday.Create;
			CadiToday.ActionReleaseComObject=fakeToday.Release;
			CadiToday.ActionShowMessage=x => fakeToday.ListCalls.Add("shown: "+x);
			foreach(Step step in arraySteps) {
				if(step.IsCadiExit) {
					fakeToday.CloseCadi();
					continue;
				}
				CadiToday.SendData(step.Patient);
			}
			FakeCadi fakeInProcess=NewFake(script);
			RunInProcess(fakeInProcess,is64BitProcess,comServerRegistration,() => {
				foreach(Step step in arraySteps) {
					if(step.IsCadiExit) {
						fakeInProcess.CloseCadi();
						continue;
					}
					SendInProcess(fakeInProcess,ToCadiPatient(step.Patient));
				}
			});
			AssertSameCalls(fakeToday,fakeInProcess,"in-process path");
			FakeCadi fakeModule=NewFake(script);
			CadiModule cadiModule=new CadiModule(fakeModule.Create,fakeModule.Release);
			foreach(Step step in arraySteps) {
				if(step.IsCadiExit) {
					fakeModule.CloseCadi();
					continue;
				}
				SendToModule(fakeModule,cadiModule,ToCadiPatient(step.Patient));
			}
			AssertSameCalls(fakeToday,fakeModule,"module");
			return fakeToday.ListCalls;
		}

		private static void AssertSameCalls(FakeCadi fakeToday,FakeCadi fakeOther,string nameOther) {
			CollectionAssert.AreEqual(fakeToday.ListCalls,fakeOther.ListCalls,"The calls differ from today's.\ntoday:\n  "
				+string.Join("\n  ",fakeToday.ListCalls)+"\n"+nameOther+":\n  "+string.Join("\n  ",fakeOther.ListCalls));
		}

		///<summary>A ProgID only the 32-bit registry view knows gives a null type: a 64-bit chooser turns it into REGDB_E_CLASSNOTREG, which is
		///retried in the helper; a 32-bit one keeps today's ArgumentNullException (CADI.cs's message). No COM class is created.</summary>
		[TestMethod]
		public void CreateComObject_NullType_64BitIsClassNotRegistered_32BitAsBefore() {
			try {
				NativeFirstChooser.Cadi.ResetForTests(true);
				COMException comException=Assert.ThrowsExactly<COMException>(() => CADIInProcess.CreateComObject(null));
				Assert.AreEqual(unchecked((int)0x80040154),comException.HResult);
				Assert.IsTrue(CADIInProcess.IsRetriedInHelper(comException,false),"Nothing reached CADI yet: retried in the helper.");
				NativeFirstChooser.Cadi.ResetForTests(false);
				Assert.ThrowsExactly<ArgumentNullException>(() => CADIInProcess.CreateComObject(null));
			}
			finally {
				NativeFirstChooser.Cadi.ResetForTests();
			}
		}

		///<summary>Runs action with fakeCadi as CADI's object of the in-process path, CADI registered as comServerRegistration and a chooser of a
		///new process (deciding for is64BitProcess), and puts everything back afterwards.</summary>
		private static void RunInProcess(FakeCadi fakeCadi,bool is64BitProcess,ComServerRegistration comServerRegistration,Action action) {
			Func<object> funcCreateOld=CADIInProcess.FuncCreateComObject;
			Action<object> actionReleaseOld=CADIInProcess.ActionReleaseComObject;
			Func<ComServerRegistration> funcReadOld=CADIInProcess.FuncReadRegistration;
			NativeFirstChooser.Cadi.ResetForTests(is64BitProcess);
			_windowInProcess=null;
			CADIInProcess.FuncCreateComObject=fakeCadi.Create;
			CADIInProcess.ActionReleaseComObject=fakeCadi.Release;
			CADIInProcess.FuncReadRegistration=() => {
				fakeCadi.ListCalls.Add("read the registration");
				return comServerRegistration;
			};
			try {
				action();
			}
			finally {
				CADIInProcess.FuncCreateComObject=funcCreateOld;
				CADIInProcess.ActionReleaseComObject=actionReleaseOld;
				CADIInProcess.FuncReadRegistration=funcReadOld;
				_windowInProcess=null;
				NativeFirstChooser.Cadi.ResetForTests();
			}
		}

		///<summary>CADI.SendData's in-process steps (CADI.cs since N64-A3): the choice, the window kept until it closes, its SendData; a false
		///SendData or a helper choice goes to the helper (recorded here as "to the helper"); a thrown message is shown.</summary>
		private static void SendInProcess(FakeCadi fakeCadi,CadiPatient cadiPatient) {
			try {
				int countCalls=fakeCadi.ListCalls.Count;
				bool isChosen=CADIInProcess.IsChosen();
				//The registry is read once per process (the first call) and not again once the choice is made: left out of the comparison.
				fakeCadi.ListCalls.RemoveAll(x => x=="read the registration");
				if(isChosen) {
					if(_windowInProcess==null) { //Only release this reference once the CADIWindow instance has closed.
						_windowInProcess=new CADINativeWindow(new Action(() => _windowInProcess=null ));
					}
					if(_windowInProcess.SendData(cadiPatient)) {
						return;
					}
				}
				fakeCadi.ListCalls.Add("to the helper");
			}
			catch(Exception e) {
				fakeCadi.ListCalls.Add("shown: "+e.Message);
			}
		}

		///<summary>The helper's side of CADI.cs's SendDataToHelper: the request with the encoded patient; what the module throws is what the
		///program shows (Helper32Failure.Remote carries its message).</summary>
		private static void SendToModule(FakeCadi fakeCadi,CadiModule cadiModule,CadiPatient cadiPatient) {
			try {
				Helper32Reply reply=cadiModule.Handle(Request(CadiCommands.SEND_DATA,CadiCommands.EncodePatient(cadiPatient)));
				Assert.IsTrue(reply.IsOk);
				Assert.AreEqual(0,reply.Results.Length);
			}
			catch(Exception e) when(!(e is AssertFailedException)) {
				fakeCadi.ListCalls.Add("shown: "+e.Message);
			}
		}

		private static Helper32Request Request(string command,params string[] args) {
			return new Helper32Request { Module=CadiCommands.MODULE,Command=command,Args=args };
		}

		private static string PicLib(TodayPatient todayPatient,string id) {
			return "OleLoadPicLib(\""+CodeBase.ODFileUtils.CombinePaths(todayPatient.ImageFolder,id)+"\":String)";
		}

		private static readonly string[] _arrayOpen=new[] {
			"create Mediadent.Application",
			"OleClientWindowHandle(hidden window:Int32,1:Int32,16:UInt32,0:Int32,0:Int32)",
		};

		///<summary>Each scenario that does not involve the four HRESULTs behaves as today for a 32-bit and a 64-bit chooser.</summary>
		private static readonly bool[] _arrayBitness=new[] { false,true };

		[TestMethod]
		public void SendData_PatientByPatNum_OpensCadiAndSendsThePatient_AsToday() {
			foreach(bool is64BitProcess in _arrayBitness) {
				List<string> listCalls=RunAll(null,is64BitProcess,RegistrationLocalServer32Bit(),Send(_patientOne));
				CollectionAssert.AreEqual(_arrayOpen.Concat(new[] {
					"OleBringToFrontApp()",
					"OleBeginTransaction()",
					"OleSetPatientName(\"Synthetic, Patient One\":String)",
					"OleSetPatientDateOfBirth(29358:Double)",
					"OleSetPatientBiologicalAge(46:Int32)",
					"OleSetPatientSex(\"F\":String)",
					PicLib(_patientOne,"1001"),
					"OleEndTransaction()",
				}).ToList(),listCalls);
			}
		}

		[TestMethod]
		public void SendData_PatientByChartNumber_AsToday() {
			foreach(bool is64BitProcess in _arrayBitness) {
				List<string> listCalls=RunAll(null,is64BitProcess,RegistrationLocalServer32Bit(),Send(_patientTwo));
				CollectionAssert.AreEqual(_arrayOpen.Concat(new[] {
					"OleBringToFrontApp()",
					"OleBeginTransaction()",
					"OleSetPatientName(\"Test\u00e9, Zweite\":String)",
					"OleSetPatientDateOfBirth(20091:Double)",
					"OleSetPatientBiologicalAge(71:Int32)",
					"OleSetPatientSex(\"M\":String)",
					PicLib(_patientTwo,"CH-2002"),
					"OleEndTransaction()",
				}).ToList(),listCalls);
			}
		}

		[TestMethod]
		public void SendData_NoPatient_OnlyOpensCadi_ThenAPatientUsesTheSameObject_AsToday() {
			foreach(bool is64BitProcess in _arrayBitness) {
				List<string> listCalls=RunAll(null,is64BitProcess,RegistrationLocalServer32Bit(),_stepSendNoPatient,Send(_patientOne));
				CollectionAssert.AreEqual(_arrayOpen,listCalls.Take(2).ToList());
				Assert.AreEqual("OleBringToFrontApp()",listCalls[2]);
				Assert.AreEqual(1,listCalls.Count(x => x=="create Mediadent.Application"),"One object for both calls.");
			}
		}

		[TestMethod]
		public void SendData_TwoPatients_KeepTheObjectAndTheWindow_AsToday() {
			foreach(bool is64BitProcess in _arrayBitness) {
				List<string> listCalls=RunAll(null,is64BitProcess,RegistrationLocalServer32Bit(),Send(_patientOne),Send(_patientTwo));
				Assert.AreEqual(1,listCalls.Count(x => x=="create Mediadent.Application"));
				Assert.AreEqual(1,listCalls.Count(x => x.StartsWith("OleClientWindowHandle(")));
				Assert.AreEqual(2,listCalls.Count(x => x=="OleEndTransaction()"));
				Assert.IsFalse(listCalls.Any(x => x.StartsWith("Marshal.ReleaseComObject")),"The object lives between the calls.");
			}
		}

		[TestMethod]
		public void SendData_CadiExitsBetweenTwoPatients_ReleasesTheObject_AndTheNextCallOpensCadiAgain_AsToday() {
			foreach(bool is64BitProcess in _arrayBitness) {
				List<string> listCalls=RunAll(null,is64BitProcess,RegistrationLocalServer32Bit(),Send(_patientOne),_stepCadiExits,Send(_patientTwo));
				int indexExit=listCalls.IndexOf("CADI exits: WM_CLOSE");
				Assert.AreEqual("Marshal.ReleaseComObject(cadi)",listCalls[indexExit+1],"The exit notice releases CADI's object.");
				CollectionAssert.AreEqual(_arrayOpen,listCalls.Skip(indexExit+2).Take(2).ToList(),"A new window and object for the next patient.");
				Assert.AreEqual(2,listCalls.Count(x => x=="create Mediadent.Application"));
			}
		}

		[TestMethod]
		public void SendData_CadiRefusesTheWindow_ShowsTodaysMessage_AndTheNextCallStartsOver_AsToday() {
			foreach(bool is64BitProcess in _arrayBitness) {
				List<string> listCalls=RunAll(x => x.ResultClientWindowHandle=14,is64BitProcess,RegistrationLocalServer32Bit(),Send(_patientOne),Send(_patientOne));
				CollectionAssert.AreEqual(new[] {
					"create Mediadent.Application",
					"OleClientWindowHandle(hidden window:Int32,1:Int32,16:UInt32,0:Int32,0:Int32)",
					"Marshal.ReleaseComObject(cadi)",
					"shown: Unable to communicate with CADI. Verify it is installed and try again.",
					"create Mediadent.Application",
					"OleClientWindowHandle(hidden window:Int32,1:Int32,16:UInt32,0:Int32,0:Int32)",
					"Marshal.ReleaseComObject(cadi)",
					"shown: Unable to communicate with CADI. Verify it is installed and try again.",
				},listCalls);
			}
		}

		[TestMethod]
		public void SendData_ACallFailsWithAnotherComError_ShowsTodaysMessage_WithoutTouchingTheObject_AsToday() {
			foreach(bool is64BitProcess in _arrayBitness) {
				List<string> listCalls=RunAll(x => x.DictThrows["OleSetPatientName"]=new COMException("Synthetic CADI failure",E_FAIL),is64BitProcess,
					RegistrationLocalServer32Bit(),Send(_patientOne),Send(_patientTwo));
				int indexFirst=listCalls.IndexOf("OleSetPatientName(\"Synthetic, Patient One\":String)");
				Assert.AreEqual("shown: Unable to access CADI. Verify it is installed and try again.",listCalls[indexFirst+1],
					"A COMException: the object is dropped without a release, as today.");
				CollectionAssert.AreEqual(_arrayOpen,listCalls.Skip(indexFirst+2).Take(2).ToList(),"The next call starts over.");
				Assert.IsFalse(listCalls.Any(x => x.StartsWith("Marshal.ReleaseComObject")));
			}
		}

		[TestMethod]
		public void SendData_ACallFailsOtherwise_ShowsItsMessage_AndReleasesTheObject_AsToday() {
			foreach(bool is64BitProcess in _arrayBitness) {
				List<string> listCalls=RunAll(x => x.DictThrows["OleBringToFrontApp"]=new InvalidOperationException("Synthetic: not now"),is64BitProcess,
					RegistrationLocalServer32Bit(),Send(_patientOne));
				CollectionAssert.AreEqual(_arrayOpen.Concat(new[] {
					"OleBringToFrontApp()",
					"Marshal.ReleaseComObject(cadi)",
					"shown: Synthetic: not now",
				}).ToList(),listCalls);
			}
		}

		[TestMethod]
		public void SendData_CadiNotInstalled_ShowsTodaysMessage_AsToday() {
			//Type.GetTypeFromProgID gives null and Activator.CreateInstance(null) throws ArgumentNullException: today's "Unable to open CADI".
			//The registry has no class id for the ProgID: the in-process path stays chosen without fixing the choice.
			foreach(bool is64BitProcess in _arrayBitness) {
				List<string> listCalls=RunAll(x => x.ExceptionCreate=new ArgumentNullException("type"),is64BitProcess,Registration(false,false,false,false),
					Send(_patientOne),_stepSendNoPatient);
				CollectionAssert.AreEqual(new[] {
					"create Mediadent.Application",
					"shown: Unable to open CADI. Verify it is installed and try again.",
					"create Mediadent.Application",
					"shown: Unable to open CADI. Verify it is installed and try again.",
				},listCalls);
			}
		}

		[TestMethod]
		public void SendData_The4HResults_In32Bit_ShowTodaysMessages_AsToday() {
			//A 32-bit process: its helper is 32-bit too and could reach nothing it cannot, so each failure shows as before.
			foreach(int hResult in _arrayHResultsHelper) {
				List<string> listCalls=RunAll(x => x.ExceptionCreate=new COMException("Synthetic: unreachable",hResult),false,RegistrationLocalServer32Bit(),
					Send(_patientOne));
				CollectionAssert.AreEqual(new[] { "create Mediadent.Application","shown: Unable to access CADI. Verify it is installed and try again." },
					listCalls,"0x"+hResult.ToString("X8"));
				listCalls=RunAll(x => x.DictThrows["OleClientWindowHandle"]=new COMException("Synthetic: unreachable",hResult),false,
					RegistrationLocalServer32Bit(),Send(_patientOne));
				CollectionAssert.AreEqual(_arrayOpen.Concat(new[] { "shown: Unable to access CADI. Verify it is installed and try again." }).ToList(),
					listCalls,"0x"+hResult.ToString("X8"));
				Assert.IsNull(NativeFirstChooser.Cadi.Choice,"Put back after the run.");
			}
			//The chooser stays on this process.
			FakeCadi fakeCadi=NewFake(x => x.ExceptionCreate=new COMException("Synthetic: unreachable",NativeFirstChooser.REGDB_E_CLASSNOTREG));
			RunInProcess(fakeCadi,false,RegistrationLocalServer32Bit(),() => {
				SendInProcess(fakeCadi,ToCadiPatient(_patientOne));
				Assert.AreEqual(VendorPath.InProcess,NativeFirstChooser.Cadi.Choice);
				Assert.IsTrue(CADIInProcess.IsChosen());
			});
		}

		[TestMethod]
		public void InProcess64_OnlyA32BitDllServer_GoesToTheHelper_WithNothingCreated() {
			FakeCadi fakeCadi=NewFake(null);
			RunInProcess(fakeCadi,true,Registration(false,false,false,true),() => {
				SendInProcess(fakeCadi,ToCadiPatient(_patientOne));
				SendInProcess(fakeCadi,ToCadiPatient(_patientTwo));
				CollectionAssert.AreEqual(new[] { "to the helper","to the helper" },fakeCadi.ListCalls);
				Assert.AreEqual(0,fakeCadi.CountCreated);
				Assert.AreEqual(VendorPath.Helper32,NativeFirstChooser.Cadi.Choice);
				StringAssert.Contains(NativeFirstChooser.Cadi.LogLine,"in the 32-bit helper program (64-bit process)");
				StringAssert.Contains(NativeFirstChooser.Cadi.LogLine,"InprocServer32 in the 32-bit registry view only");
			});
			//The same registration in a 32-bit process: in process (the 32-bit DLL server loads there).
			fakeCadi=NewFake(null);
			RunInProcess(fakeCadi,false,Registration(false,false,false,true),() => {
				SendInProcess(fakeCadi,ToCadiPatient(_patientOne));
				Assert.AreEqual(VendorPath.InProcess,NativeFirstChooser.Cadi.Choice);
				Assert.AreEqual("OleEndTransaction()",fakeCadi.ListCalls.Last());
			});
		}

		[TestMethod]
		public void InProcess64_TheActivationFails_EachHResult_RetriedInTheHelperNow_AndTheHelperFromThenOn() {
			foreach(ComServerRegistration comServerRegistration in new[] { RegistrationLocalServer32Bit(),Registration(false,false,false,false) }) {
				foreach(int hResult in _arrayHResultsHelper) {
					FakeCadi fakeCadi=NewFake(x => x.ExceptionCreate=new COMException("Synthetic: unreachable",hResult));
					RunInProcess(fakeCadi,true,comServerRegistration,() => {
						SendInProcess(fakeCadi,ToCadiPatient(_patientOne));
						SendInProcess(fakeCadi,ToCadiPatient(_patientTwo));
						CollectionAssert.AreEqual(new[] { "create Mediadent.Application","to the helper","to the helper" },fakeCadi.ListCalls,
							"0x"+hResult.ToString("X8")+": no message, the helper repeats the call; the next one goes to the helper without a new activation.");
						Assert.IsNull(_windowInProcess,"The window is closed.");
						Assert.AreEqual(VendorPath.Helper32,NativeFirstChooser.Cadi.Choice);
						StringAssert.Contains(NativeFirstChooser.Cadi.LogLine,"0x"+hResult.ToString("X8"));
						StringAssert.Contains(NativeFirstChooser.Cadi.LogLine,"in the 32-bit helper program (64-bit process)");
					});
				}
			}
			//Wrapped in a TargetInvocationException (a late-bound creation can wrap it), the same.
			FakeCadi fakeCadiWrapped=NewFake(x => x.ExceptionCreate=new System.Reflection.TargetInvocationException(
				new COMException("Synthetic: unreachable",NativeFirstChooser.CO_E_SERVER_EXEC_FAILURE)));
			RunInProcess(fakeCadiWrapped,true,RegistrationLocalServer32Bit(),() => {
				SendInProcess(fakeCadiWrapped,ToCadiPatient(_patientOne));
				CollectionAssert.AreEqual(new[] { "create Mediadent.Application","to the helper" },fakeCadiWrapped.ListCalls);
			});
		}

		[TestMethod]
		public void InProcess64_TheFirstCallFails_NoInterfaceOrNoTypeLibrary_RetriedInTheHelperNow_TheObjectReleased() {
			List<Exception> listExceptions=_arrayHResultsHelper.Select(x => (Exception)new COMException("Synthetic: unreachable",x)).ToList();
			listExceptions.Add(new InvalidCastException("Synthetic: QueryInterface failed",NativeFirstChooser.E_NOINTERFACE));
			foreach(Exception exception in listExceptions) {
				FakeCadi fakeCadi=NewFake(x => x.DictThrows["OleClientWindowHandle"]=exception);
				RunInProcess(fakeCadi,true,RegistrationLocalServer32Bit(),() => {
					SendInProcess(fakeCadi,ToCadiPatient(_patientOne));
					CollectionAssert.AreEqual(_arrayOpen.Concat(new[] { "Marshal.ReleaseComObject(cadi)","to the helper" }).ToList(),fakeCadi.ListCalls,
						exception.GetType().Name+" 0x"+exception.HResult.ToString("X8"));
					Assert.AreEqual(VendorPath.Helper32,NativeFirstChooser.Cadi.Choice);
					StringAssert.Contains(NativeFirstChooser.Cadi.LogLine,"after this process had been chosen");
				});
			}
		}

		[TestMethod]
		public void InProcess64_ALaterCallFails_ShowsTodaysMessage_AndTheHelperFromTheNextClickOn() {
			foreach(int hResult in _arrayHResultsHelper) {
				FakeCadi fakeCadi=NewFake(x => x.DictThrows["OleSetPatientName"]=new COMException("Synthetic: unreachable",hResult));
				RunInProcess(fakeCadi,true,RegistrationLocalServer32Bit(),() => {
					SendInProcess(fakeCadi,ToCadiPatient(_patientOne));
					SendInProcess(fakeCadi,ToCadiPatient(_patientTwo));
					CollectionAssert.AreEqual(_arrayOpen.Concat(new[] {
						"OleBringToFrontApp()",
						"OleBeginTransaction()",
						"OleSetPatientName(\"Synthetic, Patient One\":String)",
						//Calls reached CADI: no second try now, today's message; the next click goes to the helper.
						"shown: Unable to access CADI. Verify it is installed and try again.",
						"to the helper",
					}).ToList(),fakeCadi.ListCalls,"0x"+hResult.ToString("X8"));
					Assert.AreEqual(VendorPath.Helper32,NativeFirstChooser.Cadi.Choice);
				});
			}
		}

		[TestMethod]
		public void InProcess64_TheObjectOfAnEarlierClickFails_ShowsTodaysMessage_AndTheHelperFromTheNextClickOn() {
			FakeCadi fakeCadi=NewFake(null);
			RunInProcess(fakeCadi,true,RegistrationLocalServer32Bit(),() => {
				SendInProcess(fakeCadi,ToCadiPatient(_patientOne));
				fakeCadi.DictThrows["OleBringToFrontApp"]=new COMException("Synthetic: unreachable",NativeFirstChooser.CO_E_SERVER_EXEC_FAILURE);
				fakeCadi.ListCalls.Clear();
				SendInProcess(fakeCadi,ToCadiPatient(_patientTwo));
				SendInProcess(fakeCadi,ToCadiPatient(_patientTwo));
				CollectionAssert.AreEqual(new[] {
					"OleBringToFrontApp()",
					"shown: Unable to access CADI. Verify it is installed and try again.",
					"to the helper",
				},fakeCadi.ListCalls);
			});
		}

		[TestMethod]
		public void InProcess64_AnotherFailure_StaysInTheProcess_AsToday() {
			FakeCadi fakeCadi=NewFake(x => x.ExceptionCreate=new COMException("Synthetic CADI failure",E_FAIL));
			RunInProcess(fakeCadi,true,RegistrationLocalServer32Bit(),() => {
				SendInProcess(fakeCadi,ToCadiPatient(_patientOne));
				Assert.AreEqual("shown: Unable to access CADI. Verify it is installed and try again.",fakeCadi.ListCalls.Last());
				Assert.AreEqual(VendorPath.InProcess,NativeFirstChooser.Cadi.Choice);
				StringAssert.Contains(NativeFirstChooser.Cadi.LogLine,"in the program's process (64-bit process)");
				StringAssert.Contains(NativeFirstChooser.Cadi.LogLine,"LocalServer32 in the 32-bit registry view only");
			});
		}

		[TestMethod]
		public void InProcess_TheChoiceIsMadeOnce_TheRegistryReadOnce() {
			FakeCadi fakeCadi=NewFake(null);
			int countReads=0;
			RunInProcess(fakeCadi,true,RegistrationLocalServer32Bit(),() => {
				Func<ComServerRegistration> funcRead=CADIInProcess.FuncReadRegistration;
				CADIInProcess.FuncReadRegistration=() => { countReads++; return funcRead(); };
				for(int i=0;i<3;i++) {
					Assert.IsTrue(CADIInProcess.IsChosen());
				}
				Assert.AreEqual(1,countReads);
			});
		}

		[TestMethod]
		public void Module_TheWrongArgumentsOrCommand_ReachNoCadiCall() {
			FakeCadi fakeCadi=NewFake(null);
			CadiModule cadiModule=new CadiModule(fakeCadi.Create,fakeCadi.Release);
			Assert.ThrowsExactly<ArgumentException>(() => cadiModule.Handle(Request(CadiCommands.SEND_DATA,"Synthetic, Patient One","1")));
			Assert.ThrowsExactly<ArgumentException>(() => cadiModule.Handle(Request(CadiCommands.SEND_DATA,"Synthetic, Patient One","x","46","F","p")));
			Assert.ThrowsExactly<ArgumentException>(() => cadiModule.Handle(Request(CadiCommands.SEND_DATA,"Synthetic, Patient One","1","x","F","p")));
			Assert.ThrowsExactly<ArgumentException>(() => cadiModule.Handle(Request("NoSuchCommand")));
			Assert.AreEqual(0,fakeCadi.CountCreated);
			Assert.AreEqual(IntPtr.Zero,cadiModule.WindowHandle);
		}

		[TestMethod]
		public void Module_TheConstructorCreatesNothing() {
			//The helper builds every module at its start (Helper32Modules.CreateModules): no window and no CADI object before the first command.
			CadiModule cadiModule=new CadiModule();
			Assert.AreEqual(CadiCommands.MODULE,cadiModule.Name);
			Assert.AreEqual(IntPtr.Zero,cadiModule.WindowHandle);
		}

		[TestMethod]
		public void Module_KeepsItsWindowUntilCadiExits() {
			FakeCadi fakeCadi=NewFake(null);
			CadiModule cadiModule=new CadiModule(fakeCadi.Create,fakeCadi.Release);
			SendToModule(fakeCadi,cadiModule,ToCadiPatient(_patientOne));
			IntPtr handle=cadiModule.WindowHandle;
			Assert.AreNotEqual(IntPtr.Zero,handle);
			Assert.AreEqual(handle,fakeCadi.ListHandles.Single(),"CADI was given the module's window.");
			SendToModule(fakeCadi,cadiModule,ToCadiPatient(_patientTwo));
			Assert.AreEqual(handle,cadiModule.WindowHandle);
			fakeCadi.CloseCadi();
			Assert.AreEqual(IntPtr.Zero,cadiModule.WindowHandle,"The exit notice released the window.");
		}

		[TestMethod]
		public void Commands_ThePatientCrossesUnchanged() {
			foreach(double dateOfBirth in new[] { 29358d,0d,-1.5d,0.1+0.2,36525.000000001,double.MaxValue,double.Epsilon }) {
				CadiPatient cadiPatient=new CadiPatient { Name="Test\u00e9, Zweite \u4e2d",DateOfBirth=dateOfBirth,Age=-1,Sex="F",PicLibPath=null };
				CadiPatient cadiPatientBack=CadiCommands.DecodePatient(CadiCommands.EncodePatient(cadiPatient));
				Assert.AreEqual(cadiPatient.Name,cadiPatientBack.Name);
				Assert.AreEqual(BitConverter.DoubleToInt64Bits(dateOfBirth),BitConverter.DoubleToInt64Bits(cadiPatientBack.DateOfBirth),
					dateOfBirth.ToString("R",CultureInfo.InvariantCulture));
				Assert.AreEqual(-1,cadiPatientBack.Age);
				Assert.AreEqual("F",cadiPatientBack.Sex);
				Assert.IsNull(cadiPatientBack.PicLibPath);
			}
			Assert.AreEqual(0,CadiCommands.EncodePatient(null).Length);
			Assert.IsNull(CadiCommands.DecodePatient(new string[0]));
		}

		[TestMethod]
		public void CadiCs_WorksOutThePatientsValuesAsToday_AndSendsThemInProcessFirst() {
			string text=Support.TestPaths.ReadSource("WpfControlsOD","Bridges","CADI.cs").Replace("\r\n","\n");
			//GetCadiPatient: main's expressions (ToCadiPatient above repeats them for the tests).
			foreach(string line in new[] {
				"if(ProgramProperties.GetPropVal(ProgramCur.ProgramNum,ProgramProperties.PropertyDescs.PatOrChartNum)==\"0\") {\n\t\t\t\tid=pat.PatNum.ToString();",
				"else {\n\t\t\t\tid=pat.ChartNumber;\n\t\t\t}",
				"cadiPatient.Name=pat.LName+\", \"+pat.FName;",
				"cadiPatient.DateOfBirth=pat.Birthdate.Subtract(TDATE_START).TotalDays;",
				"cadiPatient.Age=pat.Age;",
				"cadiPatient.Sex=pat.Gender==PatientGender.Female ? \"F\" : \"M\";",
				"string imagePath=ProgramProperties.GetPropVal(ProgramCur.ProgramNum,ProgramProperties.PropertyDescs.ImageFolder);",
				"cadiPatient.PicLibPath=CodeBase.ODFileUtils.CombinePaths(imagePath,id);",
				"private static readonly DateTime TDATE_START=new DateTime(1899,12,30);",
			}) {
				StringAssert.Contains(text,line);
			}
			//SendData: the patient only when there is one; the in-process path first, the helper after it.
			string[] arrayInOrder=new[] {
				"if(pat!=null) {\n\t\t\t\t\tcadiPatient=GetCadiPatient(ProgramCur,pat);\n\t\t\t\t}",
				"if(CADIInProcess.IsChosen()) {",
				"if(CADIWindow==null) {",
				"CADIWindow=new CADINativeWindow(new Action(() => CADIWindow=null ));",
				"if(CADIWindow.SendData(cadiPatient)) {\n\t\t\t\t\t\treturn;\n\t\t\t\t\t}",
				"SendDataToHelper(cadiPatient);",
				"MessageBox.Show(e.Message);",
				"SecurityLogs.MakeLogEntry(EnumPermType.ChartModule,ODMethodsT.Coalesce(pat).PatNum,e.Message);",
				"Helper32Client.Default.CallPumping(CadiCommands.MODULE,CadiCommands.SEND_DATA,CadiCommands.EncodePatient(cadiPatient),null,_timeSpanCall,",
			};
			int index=0;
			foreach(string part in arrayInOrder) {
				int indexPart=text.IndexOf(part,index,StringComparison.Ordinal);
				Assert.IsTrue(indexPart>=0,"CADI.cs has, in this order: "+part);
				index=indexPart+part.Length;
			}
		}
	}
}
