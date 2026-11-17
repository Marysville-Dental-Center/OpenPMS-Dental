// QuickBooks Desktop's 64-bit QBFC choice (N64-B6 of the native 64-bit integration survey, docs/port-notes/N64-B6.md): the newest of several
// late-bound ProgIDs that is registered for the process's bitness and activates (NativeFirstChooser.ActivateNewestComServer), against fake
// registrations and fake activations, for each registration case, each HRESULT and other failures; the move to the helper after a failed
// connection (ChooseHelperAfterConnectionFailure); each for a 32-bit and a 64-bit chooser and the bitness the test runs at (the lane runs this
// project as x86 and as x64). And the enum constants read by name from a type library (ComTypeLibEnums), on Windows' own stdole2.tlb loaded
// without registering it. No COM class is activated and nothing is written.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ComTypes=System.Runtime.InteropServices.ComTypes;

namespace NativeFirstTests {
	[TestClass]
	public class QbfcNative64ChooserTests {
		private const string NEWEST="Synthetic.QBFC18.SessionManager";
		private const string OLDER="Synthetic.QBFC17.SessionManager";
		private const string OLDEST="Synthetic.QBFC16.SessionManager";
		private static readonly string[] _arrayProgIds=new[] { NEWEST,OLDER,OLDEST };
		private const string CLSID="{8E2A4C61-7B3D-4F0E-A519-2C6D8B0F4E37}";//synthetic
		private const int E_FAIL=unchecked((int)0x80004005);
		private static readonly int[] _arrayHResultsNext=new[] { NativeFirstChooser.REGDB_E_CLASSNOTREG,NativeFirstChooser.CO_E_SERVER_EXEC_FAILURE,
			NativeFirstChooser.E_NOINTERFACE,NativeFirstChooser.TYPE_E_LIBNOTREGISTERED };

		private static NativeFirstChooser NewChooser(bool? is64BitProcess) {
			NativeFirstChooser nativeFirstChooser=new NativeFirstChooser("Synthetic QBFC 64-bit");
			nativeFirstChooser.ResetForTests(is64BitProcess);
			return nativeFirstChooser;
		}

		///<summary>A registration with the server keys given ("64 InprocServer32" and the like); none: no class id.</summary>
		private static ComServerRegistration Registration(string progId,params string[] arrayServers) {
			ComServerRegistration comServerRegistration=new ComServerRegistration { ProgId=progId };
			if(arrayServers.Length==0) {
				return comServerRegistration;
			}
			comServerRegistration.Clsid=CLSID;
			foreach(string server in arrayServers) {
				switch(server) {
					case "64 InprocServer32":
						comServerRegistration.HasInprocServer32View64=true;
						break;
					case "32 InprocServer32":
						comServerRegistration.HasInprocServer32View32=true;
						break;
					case "64 LocalServer32":
						comServerRegistration.HasLocalServer32View64=true;
						break;
					case "32 LocalServer32":
						comServerRegistration.HasLocalServer32View32=true;
						break;
					default:
						throw new ArgumentException(server);
				}
			}
			return comServerRegistration;
		}

		///<summary>Records the activations; each ProgID's object is its name, or the exception funcFail gives for it.</summary>
		private sealed class FakeActivations {
			public readonly List<string> ListActivations=new List<string>();
			public Func<string,Exception> FuncFail=progId => null;

			public object Activate(string progId) {
				ListActivations.Add(progId);
				Exception exception=FuncFail(progId);
				if(exception!=null) {
					throw exception;
				}
				return "object of "+progId;
			}
		}

		///<summary>The registration cases of the newest ProgID (the older ones are 64-bit DLLs), and whether a 32-bit and a 64-bit process
		///create it (else they go on to the next older).</summary>
		private static IEnumerable<Tuple<string,ComServerRegistration,bool,bool>> GetCases() {
			yield return Tuple.Create("InprocServer32, 64-bit view (Intuit's 64-bit QBFC)",Registration(NEWEST,"64 InprocServer32"),false,true);
			yield return Tuple.Create("InprocServer32, 32-bit view only",Registration(NEWEST,"32 InprocServer32"),true,false);
			yield return Tuple.Create("InprocServer32 in both views",Registration(NEWEST,"64 InprocServer32","32 InprocServer32"),true,true);
			yield return Tuple.Create("LocalServer32, 32-bit view",Registration(NEWEST,"32 LocalServer32"),true,true);
			yield return Tuple.Create("LocalServer32, 64-bit view",Registration(NEWEST,"64 LocalServer32"),true,true);
			yield return Tuple.Create("No class id (not installed)",Registration(NEWEST),false,false);
			yield return Tuple.Create("A class id without a server key",new ComServerRegistration { ProgId=NEWEST,Clsid=CLSID },false,false);
		}

		[TestMethod]
		public void Choice_EachRegistrationOfTheNewest_AtEachBitness() {
			foreach(Tuple<string,ComServerRegistration,bool,bool> tupleCase in GetCases()) {
				foreach(bool? is64BitProcess in new bool?[] { false,true,null }) {
					bool is64=is64BitProcess??Environment.Is64BitProcess;
					bool canCreate=is64 ? tupleCase.Item4 : tupleCase.Item3;
					string what=tupleCase.Item1+", "+(is64 ? "64" : "32")+"-bit process";
					NativeFirstChooser nativeFirstChooser=NewChooser(is64BitProcess);
					FakeActivations fakeActivations=new FakeActivations();
					List<string> listReads=new List<string>();
					Func<string,ComServerRegistration> funcRead=progId => {
						listReads.Add(progId);
						return progId==NEWEST ? tupleCase.Item2 : Registration(progId,is64 ? "64 InprocServer32" : "32 InprocServer32");
					};
					object obj=nativeFirstChooser.ActivateNewestComServer(_arrayProgIds,funcRead,fakeActivations.Activate,out string progIdActivated);
					string progIdExpected=canCreate ? NEWEST : OLDER;
					Assert.AreEqual("object of "+progIdExpected,obj,what);
					Assert.AreEqual(progIdExpected,progIdActivated,what);
					CollectionAssert.AreEqual(new[] { progIdExpected },fakeActivations.ListActivations,what+": a class that cannot be created here is not activated.");
					CollectionAssert.AreEqual(canCreate ? new[] { NEWEST } : new[] { NEWEST,OLDER },listReads,what);
					Assert.AreEqual(VendorPath.InProcess,nativeFirstChooser.Choice,what);
					StringAssert.Contains(nativeFirstChooser.LogLine,"in the program's process ("+(is64 ? "64" : "32")+"-bit process); "+progIdExpected
						+" activated in this process (",what);
					if(!canCreate) {
						StringAssert.Contains(nativeFirstChooser.LogLine,"; before it: "+tupleCase.Item2.Describe(),what);
					}
				}
			}
		}

		[TestMethod]
		public void Choice_EachHResult_MovesToTheNextOlder_AllOfThem_TheHelperIn64Bit() {
			foreach(int hResult in _arrayHResultsNext) {
				List<Func<Exception>> listExceptions=new List<Func<Exception>> {
					() => new COMException("Synthetic activation failure",hResult),
					() => new TargetInvocationException(new COMException("Synthetic activation failure",hResult)),
				};
				if(hResult==NativeFirstChooser.E_NOINTERFACE) {
					listExceptions.Add(() => new InvalidCastException("Synthetic: QueryInterface failed",hResult));
				}
				foreach(Func<Exception> funcException in listExceptions) {
					foreach(bool? is64BitProcess in new bool?[] { false,true,null }) {
						bool is64=is64BitProcess??Environment.Is64BitProcess;
						string what="0x"+hResult.ToString("X8")+" in a "+funcException().GetType().Name+", "+(is64 ? "64" : "32")+"-bit process";
						Func<string,ComServerRegistration> funcRead=progId => Registration(progId,"64 InprocServer32","32 InprocServer32");
						//The newest fails: the next older activates.
						NativeFirstChooser nativeFirstChooser=NewChooser(is64BitProcess);
						FakeActivations fakeActivations=new FakeActivations { FuncFail=progId => progId==NEWEST ? funcException() : null };
						object obj=nativeFirstChooser.ActivateNewestComServer(_arrayProgIds,funcRead,fakeActivations.Activate,out string progIdActivated);
						Assert.AreEqual("object of "+OLDER,obj,what);
						Assert.AreEqual(OLDER,progIdActivated,what);
						CollectionAssert.AreEqual(new[] { NEWEST,OLDER },fakeActivations.ListActivations,what);
						StringAssert.Contains(nativeFirstChooser.LogLine,"before it: "+NEWEST+": activating it here failed with 0x"+hResult.ToString("X8"),what);
						//All fail: the helper in a 64-bit process (logged with each one), nothing chosen in a 32-bit one.
						NativeFirstChooser nativeFirstChooserAll=NewChooser(is64BitProcess);
						FakeActivations fakeActivationsAll=new FakeActivations { FuncFail=progId => funcException() };
						Assert.IsNull(nativeFirstChooserAll.ActivateNewestComServer(_arrayProgIds,funcRead,fakeActivationsAll.Activate,out progIdActivated),what);
						Assert.AreEqual("",progIdActivated,what);
						CollectionAssert.AreEqual(_arrayProgIds,fakeActivationsAll.ListActivations,what);
						if(is64) {
							Assert.AreEqual(VendorPath.Helper32,nativeFirstChooserAll.Choice,what);
							StringAssert.StartsWith(nativeFirstChooserAll.LogLine,"Synthetic QBFC 64-bit: in the 32-bit helper program (64-bit process); none of "
								+string.Join(", ",_arrayProgIds)+" activated in this process: "+NEWEST+": activating it here failed with 0x",what);
							StringAssert.Contains(nativeFirstChooserAll.LogLine,OLDEST+": activating it here failed with 0x"+hResult.ToString("X8"),what);
							//Chosen: nothing is read or activated again.
							Assert.IsNull(nativeFirstChooserAll.ActivateNewestComServer(_arrayProgIds,progId => throw new AssertFailedException("Not read again."),
								progId => throw new AssertFailedException("Not activated again."),out progIdActivated),what);
						}
						else {
							Assert.IsNull(nativeFirstChooserAll.Choice,what+": a 32-bit process keeps its QBFC 10 path.");
							Assert.AreEqual("",nativeFirstChooserAll.LogLine,what);
						}
					}
				}
			}
		}

		[TestMethod]
		public void Choice_AnotherActivationFailure_IsThrown_NothingChosen_NoOlderTried() {
			foreach(Exception exception in new Exception[] {
				new COMException("Synthetic: unspecified failure",E_FAIL),
				new COMException("Synthetic: access denied",unchecked((int)0x80070005)),
				new InvalidOperationException("Synthetic"),
			}) {
				foreach(bool is64BitProcess in new[] { false,true }) {
					NativeFirstChooser nativeFirstChooser=NewChooser(is64BitProcess);
					FakeActivations fakeActivations=new FakeActivations { FuncFail=progId => exception };
					Exception exceptionThrown=Assert.Throws<Exception>(() => nativeFirstChooser.ActivateNewestComServer(_arrayProgIds,
						progId => Registration(progId,"64 InprocServer32","32 InprocServer32"),fakeActivations.Activate,out string progIdActivated));
					Assert.AreSame(exception,exceptionThrown);
					CollectionAssert.AreEqual(new[] { NEWEST },fakeActivations.ListActivations);
					Assert.IsNull(nativeFirstChooser.Choice);
				}
			}
		}

		[TestMethod]
		public void Choice_AnUnreadableRegistry_LeavesItToTheActivation() {
			foreach(bool is64BitProcess in new[] { false,true }) {
				NativeFirstChooser nativeFirstChooser=NewChooser(is64BitProcess);
				FakeActivations fakeActivations=new FakeActivations();
				object obj=nativeFirstChooser.ActivateNewestComServer(_arrayProgIds,progId => throw new UnauthorizedAccessException("Synthetic"),
					fakeActivations.Activate,out string progIdActivated);
				Assert.AreEqual("object of "+NEWEST,obj);
				Assert.AreEqual(NEWEST+" activated in this process",nativeFirstChooser.Reason,"No registration to describe.");
				NativeFirstChooser nativeFirstChooserNull=NewChooser(is64BitProcess);
				Assert.AreEqual("object of "+NEWEST,nativeFirstChooserNull.ActivateNewestComServer(_arrayProgIds,progId => null,fakeActivations.Activate,
					out progIdActivated));
			}
		}

		[TestMethod]
		public void Choice_TheHelperChosenBefore_NothingReadOrActivated() {
			NativeFirstChooser nativeFirstChooser=NewChooser(true);
			nativeFirstChooser.ForceChoiceForTests(VendorPath.Helper32);
			Assert.IsNull(nativeFirstChooser.ActivateNewestComServer(_arrayProgIds,progId => throw new AssertFailedException("Not read."),
				progId => throw new AssertFailedException("Not activated."),out string progIdActivated));
			Assert.AreEqual("",progIdActivated);
		}

		[TestMethod]
		public void ConnectionFailure_TheHelperIn64Bit_Once_NothingIn32Bit() {
			foreach(Exception exception in new Exception[] {
				new COMException("Synthetic: the company file could not be opened.",unchecked((int)0x80040408)),
				new COMException("Synthetic: unspecified failure",E_FAIL),
				new InvalidOperationException("Synthetic: no session manager"),
				new TargetInvocationException(new COMException("Synthetic: wrapped",NativeFirstChooser.REGDB_E_CLASSNOTREG)),
			}) {
				string what=exception.GetType().Name+": "+exception.Message;
				//64-bit, after this process was chosen: the helper, logged with the failure and the earlier choice; then kept, not logged again.
				NativeFirstChooser nativeFirstChooser64=NewChooser(true);
				nativeFirstChooser64.ActivateNewestComServer(_arrayProgIds,progId => Registration(progId,"64 InprocServer32"),new FakeActivations().Activate,
					out string progIdActivated);
				Assert.AreEqual(VendorPath.InProcess,nativeFirstChooser64.Choice);
				Assert.IsTrue(nativeFirstChooser64.ChooseHelperAfterConnectionFailure(exception),what);
				Assert.AreEqual(VendorPath.Helper32,nativeFirstChooser64.Choice,what);
				StringAssert.Contains(nativeFirstChooser64.LogLine,"in the 32-bit helper program (64-bit process); connecting failed in this process with ",what);
				StringAssert.Contains(nativeFirstChooser64.LogLine,", after this process had been chosen ("+NEWEST+" activated in this process",what);
				string logLine=nativeFirstChooser64.LogLine;
				Assert.IsTrue(nativeFirstChooser64.ChooseHelperAfterConnectionFailure(exception),what);
				Assert.AreEqual(logLine,nativeFirstChooser64.LogLine,what+": logged once.");
				//64-bit with nothing chosen before (an activation that threw): the helper too.
				NativeFirstChooser nativeFirstChooserNone=NewChooser(true);
				Assert.IsTrue(nativeFirstChooserNone.ChooseHelperAfterConnectionFailure(exception),what);
				Assert.AreEqual(VendorPath.Helper32,nativeFirstChooserNone.Choice,what);
				//32-bit: nothing changes, nothing is logged.
				NativeFirstChooser nativeFirstChooser32=NewChooser(false);
				Assert.IsFalse(nativeFirstChooser32.ChooseHelperAfterConnectionFailure(exception),what);
				Assert.IsNull(nativeFirstChooser32.Choice,what);
				Assert.AreEqual("",nativeFirstChooser32.LogLine,what);
				//At the bitness this test runs at.
				NativeFirstChooser nativeFirstChooserHere=NewChooser(null);
				Assert.AreEqual(Environment.Is64BitProcess,nativeFirstChooserHere.ChooseHelperAfterConnectionFailure(exception),what);
			}
		}

		[TestMethod]
		public void TheQbfcChooser_IsItsOwn_AndChoosesNothingBeforeItsFirstUse() {
			Assert.IsNull(NativeFirstChooser.QuickBooksDesktopNative64.Choice);
			Assert.AreEqual("QuickBooks Desktop 64-bit QBFC (QBSessionManager, late-bound)",NativeFirstChooser.QuickBooksDesktopNative64.VendorName);
			Assert.AreNotSame(NativeFirstChooser.QuickBooksDesktop,NativeFirstChooser.QuickBooksDesktopNative64,"QBFC 10's chooser is the 32-bit program's.");
			Assert.AreEqual("QuickBooks Desktop QBFC 10 (QBSessionManager)",NativeFirstChooser.QuickBooksDesktop.VendorName,"Unchanged.");
			Assert.AreEqual(Environment.Is64BitProcess,NativeFirstChooser.QuickBooksDesktopNative64.Is64BitProcess);
		}

		///<summary>stdole2.tlb of Windows' system folder (System32, or SysWOW64 for a 32-bit process), loaded without registering it.</summary>
		private static ComTypes.ITypeLib LoadStdOle2() {
			string path=Path.Combine(Environment.SystemDirectory,"stdole2.tlb");
			Assert.IsTrue(File.Exists(path),path);
			return ComTypeLibEnums.LoadWithoutRegistering(path);
		}

		[TestMethod]
		public void TypeLibEnums_ReadByName_FromAWindowsTypeLibrary() {
			ComTypes.ITypeLib typeLib=LoadStdOle2();
			//stdole's OLE_TRISTATE (Unchecked 0, Checked 1, Gray 2) and LoadPictureConstants (Default 0, Monochrome 1, VgaColor 2, Color 4).
			Dictionary<string,int> dictValues=ComTypeLibEnums.Read(typeLib,new[] { "Gray","Checked","Color","VgaColor" });
			Assert.AreEqual(2,dictValues["Gray"]);
			Assert.AreEqual(1,dictValues["Checked"]);
			Assert.AreEqual(4,dictValues["Color"]);
			Assert.AreEqual(2,dictValues["VgaColor"]);
			Assert.AreEqual(4,dictValues.Count);
		}

		[TestMethod]
		public void TypeLibEnums_AMissingNameOrNoComObject_Throws() {
			ComTypes.ITypeLib typeLib=LoadStdOle2();
			KeyNotFoundException keyNotFoundException=Assert.ThrowsExactly<KeyNotFoundException>(() => ComTypeLibEnums.Read(typeLib,new[] { "Gray",
				"rtAccountQueryRs" }));
			StringAssert.Contains(keyNotFoundException.Message,"rtAccountQueryRs");
			Assert.ThrowsExactly<ArgumentException>(() => ComTypeLibEnums.ReadFromObject(new object(),new[] { "Gray" }));
			Assert.ThrowsExactly<ArgumentException>(() => ComTypeLibEnums.ReadFromObject(null,new[] { "Gray" }));
		}
	}
}
