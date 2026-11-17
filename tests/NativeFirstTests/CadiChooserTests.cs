// Mediadent / CADI's choice (N64-A3 of the native 64-bit integration survey, docs/port-notes/N64-A3.md): the ProgID's registration read in
// both registry views (ComServerRegistration, against a fake registry), the choice from it for each registry case (NativeFirstChooser
// .ChooseForComServer), and the move to the 32-bit helper for each HRESULT that says this process cannot reach the server
// (ChooseHelperAfterFailure), each for a 32-bit and a 64-bit chooser and for the bitness the test runs at (the lane runs this project as x86
// and as x64). Two real registry reads, read only: a ProgID registered nowhere, and the Windows Script Runtime's Scripting.FileSystemObject.
// No class is activated and nothing is written.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NativeFirstTests {
	[TestClass]
	public class CadiChooserTests {
		private const string PROG_ID="Mediadent.Application";
		private const string CLSID="{6F0C8E8A-2B1D-4C5E-9A3F-7D21B5E40C19}";//synthetic
		private const int E_FAIL=unchecked((int)0x80004005);
		private static readonly int[] _arrayHResultsHelper=new[] { NativeFirstChooser.REGDB_E_CLASSNOTREG,NativeFirstChooser.CO_E_SERVER_EXEC_FAILURE,
			NativeFirstChooser.E_NOINTERFACE,NativeFirstChooser.TYPE_E_LIBNOTREGISTERED };

		///<summary>A registry of HKEY_CLASSES_ROOT keys per view; a key's default value is null when it has none. Records each read.</summary>
		private sealed class FakeRegistry:IComRegistryReader {
			public readonly Dictionary<string,string> DictView64=new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);
			public readonly Dictionary<string,string> DictView32=new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);
			public readonly List<string> ListReads=new List<string>();

			public FakeRegistry Add(bool isView64,string subKey,string value=null) {
				(isView64 ? DictView64 : DictView32)[subKey]=value;
				return this;
			}

			public string GetDefaultValue(bool isView64,string subKey) {
				ListReads.Add((isView64 ? "64 " : "32 ")+subKey);
				(isView64 ? DictView64 : DictView32).TryGetValue(subKey,out string value);
				return value;
			}

			public bool HasKey(bool isView64,string subKey) {
				ListReads.Add((isView64 ? "64 " : "32 ")+subKey+" (key)");
				return (isView64 ? DictView64 : DictView32).ContainsKey(subKey);
			}
		}

		///<summary>The ProgID with its class id in that view and the server keys given (each "64 LocalServer32" and the like).</summary>
		private static FakeRegistry Registry(bool isProgIdInView64,bool isProgIdInView32,params string[] arrayServers) {
			FakeRegistry fakeRegistry=new FakeRegistry();
			if(isProgIdInView64) {
				fakeRegistry.Add(true,PROG_ID+@"\CLSID",CLSID);
			}
			if(isProgIdInView32) {
				fakeRegistry.Add(false,PROG_ID+@"\CLSID",CLSID);
			}
			foreach(string server in arrayServers) {
				string[] arrayParts=server.Split(' ');
				fakeRegistry.Add(arrayParts[0]=="64",@"CLSID\"+CLSID+@"\"+arrayParts[1],@"C:\Program Files (x86)\Synthetic\server.exe");
			}
			return fakeRegistry;
		}

		private static NativeFirstChooser NewChooser(bool? is64BitProcess) {
			NativeFirstChooser nativeFirstChooser=new NativeFirstChooser("Synthetic CADI");
			nativeFirstChooser.ResetForTests(is64BitProcess);
			return nativeFirstChooser;
		}

		///<summary>The registry cases: the registration, and whether a 32-bit and a 64-bit process can create the class.</summary>
		private static IEnumerable<Tuple<string,FakeRegistry,bool,bool>> GetCases() {
			//LocalServer32: a server program of its own, which COM starts for a caller of either bitness (also from the 32-bit view only).
			yield return Tuple.Create("LocalServer32, 32-bit view (the expected CADI)",Registry(true,true,"32 LocalServer32"),true,true);
			yield return Tuple.Create("LocalServer32, ProgID in the 32-bit view only",Registry(false,true,"32 LocalServer32"),true,true);
			yield return Tuple.Create("LocalServer32, 64-bit view",Registry(true,false,"64 LocalServer32"),true,true);
			yield return Tuple.Create("LocalServer32 in both views",Registry(true,true,"64 LocalServer32","32 LocalServer32"),true,true);
			yield return Tuple.Create("LocalServer32 and a 32-bit InprocServer32",Registry(true,true,"32 LocalServer32","32 InprocServer32"),true,true);
			//InprocServer32: a DLL, which only a process of its view's bitness loads.
			yield return Tuple.Create("InprocServer32, 32-bit view only",Registry(true,true,"32 InprocServer32"),true,false);
			yield return Tuple.Create("InprocServer32, 64-bit view only",Registry(true,true,"64 InprocServer32"),false,true);
			yield return Tuple.Create("InprocServer32 in both views",Registry(true,true,"64 InprocServer32","32 InprocServer32"),true,true);
		}

		[TestMethod]
		public void Registration_ReadsTheClassIdAndTheServerKeysOfBothViews() {
			FakeRegistry fakeRegistry=Registry(false,true,"32 LocalServer32");
			ComServerRegistration comServerRegistration=ComServerRegistration.Read(PROG_ID,fakeRegistry);
			Assert.AreEqual(CLSID,comServerRegistration.Clsid);
			Assert.IsTrue(comServerRegistration.IsRegistered);
			Assert.IsTrue(comServerRegistration.HasLocalServer32View32);
			Assert.IsFalse(comServerRegistration.HasLocalServer32View64);
			Assert.IsFalse(comServerRegistration.HasInprocServer32View32 || comServerRegistration.HasInprocServer32View64);
			CollectionAssert.AreEqual(new[] {
				@"64 Mediadent.Application\CLSID",
				@"64 Mediadent.Application\CurVer",
				@"32 Mediadent.Application\CLSID",
				@"64 CLSID\"+CLSID+@"\LocalServer32 (key)",
				@"32 CLSID\"+CLSID+@"\LocalServer32 (key)",
				@"64 CLSID\"+CLSID+@"\InprocServer32 (key)",
				@"32 CLSID\"+CLSID+@"\InprocServer32 (key)",
			},fakeRegistry.ListReads);
			Assert.AreEqual("Mediadent.Application ("+CLSID+") has LocalServer32 in the 32-bit registry view only",comServerRegistration.Describe());
		}

		[TestMethod]
		public void Registration_FollowsCurVer_AndNormalizesTheClassId() {
			FakeRegistry fakeRegistry=new FakeRegistry()
				.Add(true,PROG_ID+@"\CurVer","Mediadent.Application.8")
				.Add(true,@"Mediadent.Application.8\CLSID"," "+CLSID.ToLowerInvariant().Trim('{','}')+" ")
				.Add(true,@"CLSID\"+CLSID+@"\LocalServer32");
			ComServerRegistration comServerRegistration=ComServerRegistration.Read(PROG_ID,fakeRegistry);
			Assert.AreEqual(CLSID,comServerRegistration.Clsid);
			Assert.IsTrue(comServerRegistration.HasLocalServer32View64);
		}

		[TestMethod]
		public void Registration_None_IsNotRegistered() {
			foreach(FakeRegistry fakeRegistry in new[] {
				new FakeRegistry(),//CADI not installed
				Registry(true,true),//a class id without a server key
				new FakeRegistry().Add(true,PROG_ID+@"\CLSID","not a class id").Add(false,PROG_ID+@"\CurVer",@"..\Elsewhere"),
			}) {
				ComServerRegistration comServerRegistration=ComServerRegistration.Read(PROG_ID,fakeRegistry);
				Assert.IsFalse(comServerRegistration.IsRegistered);
				Assert.IsFalse(comServerRegistration.CanCreateFrom(true) || comServerRegistration.CanCreateFrom(false));
			}
			Assert.AreEqual("Mediadent.Application has no class id in either registry view",ComServerRegistration.Read(PROG_ID,new FakeRegistry()).Describe());
			Assert.AreEqual("Mediadent.Application ("+CLSID+") has no server key in either registry view",ComServerRegistration.Read(PROG_ID,Registry(true,true)).Describe());
		}

		[TestMethod]
		public void Choice_EachRegistryCase_AtEachBitness() {
			foreach(Tuple<string,FakeRegistry,bool,bool> tupleCase in GetCases()) {
				foreach(bool? is64BitProcess in new bool?[] { false,true,null }) {
					bool is64=is64BitProcess??Environment.Is64BitProcess;
					bool canCreate=is64 ? tupleCase.Item4 : tupleCase.Item3;
					NativeFirstChooser nativeFirstChooser=NewChooser(is64BitProcess);
					VendorPath vendorPath=nativeFirstChooser.ChooseForComServer(() => ComServerRegistration.Read(PROG_ID,tupleCase.Item2));
					VendorPath vendorPathExpected=canCreate || !is64 ? VendorPath.InProcess : VendorPath.Helper32;
					string what=tupleCase.Item1+", "+(is64 ? "64" : "32")+"-bit process";
					Assert.AreEqual(vendorPathExpected,vendorPath,what);
					Assert.AreEqual(vendorPath,nativeFirstChooser.Choice,what+": the choice is fixed.");
					StringAssert.Contains(nativeFirstChooser.LogLine,"("+(is64 ? "64" : "32")+"-bit process)",what);
					StringAssert.Contains(nativeFirstChooser.LogLine,"Mediadent.Application ("+CLSID+") has ",what);
					if(!canCreate) {
						StringAssert.Contains(nativeFirstChooser.LogLine,is64 ? "in the 32-bit helper program" : "the 32-bit helper could not run it either",what);
					}
				}
			}
		}

		[TestMethod]
		public void Choice_TheRegistryIsReadOnce_AndTheChoiceKept() {
			NativeFirstChooser nativeFirstChooser=NewChooser(true);
			int countReads=0;
			Func<ComServerRegistration> funcRead=() => {
				countReads++;
				return ComServerRegistration.Read(PROG_ID,Registry(true,true,"32 InprocServer32"));
			};
			Assert.AreEqual(VendorPath.Helper32,nativeFirstChooser.ChooseForComServer(funcRead));
			string logLine=nativeFirstChooser.LogLine;
			Assert.AreEqual(VendorPath.Helper32,nativeFirstChooser.ChooseForComServer(funcRead));
			Assert.AreEqual(1,countReads);
			Assert.AreEqual(logLine,nativeFirstChooser.LogLine);
		}

		[TestMethod]
		public void Choice_NotRegisteredOrUnreadable_StaysInTheProcess_WithoutFixingTheChoice() {
			//Creating the class there says CADI is not installed, as before; the next call reads the registry again (CADI may be installed since).
			foreach(bool is64BitProcess in new[] { false,true }) {
				NativeFirstChooser nativeFirstChooser=NewChooser(is64BitProcess);
				Assert.AreEqual(VendorPath.InProcess,nativeFirstChooser.ChooseForComServer(() => ComServerRegistration.Read(PROG_ID,new FakeRegistry())));
				Assert.AreEqual(VendorPath.InProcess,nativeFirstChooser.ChooseForComServer(() => throw new UnauthorizedAccessException("Synthetic")));
				Assert.AreEqual(VendorPath.InProcess,nativeFirstChooser.ChooseForComServer(() => null));
				Assert.IsNull(nativeFirstChooser.Choice);
				Assert.AreEqual("",nativeFirstChooser.LogLine);
				//Installed since: the next call chooses.
				Assert.AreEqual(is64BitProcess ? VendorPath.Helper32 : VendorPath.InProcess,
					nativeFirstChooser.ChooseForComServer(() => ComServerRegistration.Read(PROG_ID,Registry(true,true,"32 InprocServer32"))));
				Assert.IsNotNull(nativeFirstChooser.Choice);
			}
		}

		[TestMethod]
		public void Failure_EachHResult_TheHelperIn64Bit_NothingIn32Bit() {
			foreach(int hResult in _arrayHResultsHelper) {
				List<Exception> listExceptions=new List<Exception> {
					new COMException("Synthetic: unreachable",hResult),
					new TargetInvocationException(new COMException("Synthetic: unreachable",hResult)),
				};
				if(hResult==NativeFirstChooser.E_NOINTERFACE) {
					listExceptions.Add(new InvalidCastException("Synthetic: QueryInterface failed",hResult));//a failed QueryInterface of the RCW
				}
				foreach(Exception exception in listExceptions) {
					string what="0x"+hResult.ToString("X8")+" in a "+exception.GetType().Name;
					Assert.IsTrue(NativeFirstChooser.IsComServerUnreachableHere(exception),what);
					//64-bit, after the registry chose this process: the helper, logged with the failure and the earlier choice.
					NativeFirstChooser nativeFirstChooser64=NewChooser(true);
					Assert.AreEqual(VendorPath.InProcess,nativeFirstChooser64.ChooseForComServer(() => ComServerRegistration.Read(PROG_ID,
						Registry(true,true,"32 LocalServer32"))));
					Assert.IsTrue(nativeFirstChooser64.ChooseHelperAfterFailure(exception),what);
					Assert.AreEqual(VendorPath.Helper32,nativeFirstChooser64.Choice,what);
					StringAssert.Contains(nativeFirstChooser64.LogLine,"in the 32-bit helper program (64-bit process); it failed in this process with 0x"
						+hResult.ToString("X8"),what);
					StringAssert.Contains(nativeFirstChooser64.LogLine,"after this process had been chosen (Mediadent.Application",what);
					Assert.IsTrue(nativeFirstChooser64.ChooseHelperAfterFailure(exception),what+": the helper stays chosen.");
					Assert.AreEqual(VendorPath.Helper32,nativeFirstChooser64.ChooseForComServer(() => throw new AssertFailedException("Not read again.")));
					//64-bit with nothing chosen before (the class registered nowhere): the helper too.
					NativeFirstChooser nativeFirstChooserNone=NewChooser(true);
					Assert.IsTrue(nativeFirstChooserNone.ChooseHelperAfterFailure(exception),what);
					Assert.AreEqual(VendorPath.Helper32,nativeFirstChooserNone.Choice);
					//32-bit: the helper could reach nothing this process cannot; nothing changes and nothing is logged.
					NativeFirstChooser nativeFirstChooser32=NewChooser(false);
					Assert.AreEqual(VendorPath.InProcess,nativeFirstChooser32.ChooseForComServer(() => ComServerRegistration.Read(PROG_ID,
						Registry(true,true,"32 LocalServer32"))));
					string logLine32=nativeFirstChooser32.LogLine;
					Assert.IsFalse(nativeFirstChooser32.ChooseHelperAfterFailure(exception),what);
					Assert.AreEqual(VendorPath.InProcess,nativeFirstChooser32.Choice);
					Assert.AreEqual(logLine32,nativeFirstChooser32.LogLine);
					//At the bitness this test runs at.
					NativeFirstChooser nativeFirstChooserHere=NewChooser(null);
					Assert.AreEqual(Environment.Is64BitProcess,nativeFirstChooserHere.ChooseHelperAfterFailure(exception),what);
					Assert.AreEqual(Environment.Is64BitProcess ? (VendorPath?)VendorPath.Helper32 : null,nativeFirstChooserHere.Choice);
				}
			}
		}

		[TestMethod]
		public void Failure_AnyOtherError_ChangesNothing() {
			foreach(Exception exception in new Exception[] {
				new COMException("Synthetic CADI failure",E_FAIL),
				new COMException("Synthetic: access denied",unchecked((int)0x80070005)),
				new ArgumentNullException("type"),//Type.GetTypeFromProgID gave null: CADI is not installed
				new InvalidCastException("Synthetic: another cast",unchecked((int)0x80131500)),
				new InvalidOperationException("Synthetic"),
				new TargetInvocationException(new InvalidOperationException("Synthetic")),
			}) {
				Assert.IsFalse(NativeFirstChooser.IsComServerUnreachableHere(exception),exception.GetType().Name);
				foreach(bool is64BitProcess in new[] { false,true }) {
					NativeFirstChooser nativeFirstChooser=NewChooser(is64BitProcess);
					Assert.IsFalse(nativeFirstChooser.ChooseHelperAfterFailure(exception));
					Assert.IsNull(nativeFirstChooser.Choice);
				}
			}
		}

		[TestMethod]
		public void TheCadiChooser_ChoosesNothingBeforeItsFirstUse() {
			Assert.IsNull(NativeFirstChooser.Cadi.Choice);
			StringAssert.Contains(NativeFirstChooser.Cadi.VendorName,"Mediadent.Application");
			Assert.AreEqual(Environment.Is64BitProcess,NativeFirstChooser.Cadi.Is64BitProcess);
		}

		[TestMethod]
		public void WindowsRegistry_AProgIdRegisteredNowhere_IsNotRegistered() {
			//Read only. A ProgID made for this test (a random GUID in its name).
			ComServerRegistration comServerRegistration=ComServerRegistration.Read("OpenPmsTests.NotRegistered.b7d6e9c2a14f4e0d93a85c1f2e7b6d40");
			Assert.AreEqual("",comServerRegistration.Clsid);
			Assert.IsFalse(comServerRegistration.IsRegistered);
			NativeFirstChooser nativeFirstChooser=NewChooser(null);
			Assert.AreEqual(VendorPath.InProcess,nativeFirstChooser.ChooseForComServer(() => comServerRegistration));
			Assert.IsNull(nativeFirstChooser.Choice);
		}

		[TestMethod]
		public void WindowsRegistry_TheScriptRuntime_HasAnInprocServerForThisBitness() {
			//Read only. Windows registers scrrun.dll's Scripting.FileSystemObject as an InprocServer32 in each view it has (both on 64-bit Windows).
			ComServerRegistration comServerRegistration=ComServerRegistration.Read("Scripting.FileSystemObject");
			Assert.AreEqual("{0D43FE01-F093-11CF-8940-00A0C9054228}",comServerRegistration.Clsid);
			Assert.IsTrue(comServerRegistration.HasInprocServer(Environment.Is64BitProcess),comServerRegistration.Describe());
			if(Environment.Is64BitOperatingSystem) {
				Assert.IsTrue(comServerRegistration.HasInprocServer32View64 && comServerRegistration.HasInprocServer32View32,comServerRegistration.Describe());
			}
			Assert.IsFalse(comServerRegistration.HasLocalServer,comServerRegistration.Describe());
			NativeFirstChooser nativeFirstChooser=NewChooser(null);
			Assert.AreEqual(VendorPath.InProcess,nativeFirstChooser.ChooseForComServer(() => comServerRegistration));
		}
	}
}
