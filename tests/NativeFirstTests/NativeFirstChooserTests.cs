// The native-first chooser (S3a-14n of the .NET 10 move, docs/port-notes/S3a-14.md, section "S3a-14n: native first"; net10-plan decision 23):
// one choice per vendor piece and process, from a managed DLL's headers, a native DLL's header, or one COM activation; logged; the 32-bit
// helper only for a 64-bit process that cannot run the piece. Each rule is checked for both bitnesses (ResetForTests) and for the bitness
// the test runs at, which the lane runs as x86 and as x64. The COM activations are fakes, apart from one class id that is registered nowhere;
// no vendor DLL is loaded and no vendor class is activated.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NativeFirstTests {
	[TestClass]
	public class NativeFirstChooserTests {
		private const int E_FAIL=unchecked((int)0x80004005);
		///<summary>A class id registered nowhere (a random GUID, made for this test).</summary>
		private static readonly Guid _guidNotRegistered=new Guid("7c1e04d2-93b5-4f0a-a8e6-2d5b91f3c047");

		///<summary>A chooser of its own for one test, deciding for is64BitProcess (null: the process's bitness).</summary>
		private static NativeFirstChooser NewChooser(bool? is64BitProcess) {
			NativeFirstChooser nativeFirstChooser=new NativeFirstChooser("Synthetic vendor piece");
			nativeFirstChooser.ResetForTests(is64BitProcess);
			return nativeFirstChooser;
		}

		private static VendorPath Expected(bool canRunInProcess,bool is64BitProcess) {
			return canRunInProcess || !is64BitProcess ? VendorPath.InProcess : VendorPath.Helper32;
		}

		[TestMethod]
		public void Decide_TheHelperOnlyForA64BitProcessThatCannotRunThePiece() {
			Assert.AreEqual(VendorPath.InProcess,NativeFirstChooser.Decide(true,false));
			Assert.AreEqual(VendorPath.InProcess,NativeFirstChooser.Decide(true,true));
			Assert.AreEqual(VendorPath.InProcess,NativeFirstChooser.Decide(false,false),"A 32-bit process: its helper is 32-bit too.");
			Assert.AreEqual(VendorPath.Helper32,NativeFirstChooser.Decide(false,true));
		}

		[TestMethod]
		public void TheChoosersOfTheFourVendorPieces_ChooseNothingBeforeTheirFirstUse() {
			foreach(NativeFirstChooser nativeFirstChooser in new[] { NativeFirstChooser.Topaz,NativeFirstChooser.SchickCdr,NativeFirstChooser.Guru,
				NativeFirstChooser.QuickBooksDesktop })
			{
				Assert.IsFalse(string.IsNullOrWhiteSpace(nativeFirstChooser.VendorName));
				Assert.IsNull(nativeFirstChooser.Choice,nativeFirstChooser.VendorName+" chose nothing in this test process.");
				Assert.AreEqual(Environment.Is64BitProcess,nativeFirstChooser.Is64BitProcess);
			}
		}

		[TestMethod]
		public void ManagedAssembly_EachKindAtEachBitness() {
			Dictionary<string,byte[]> dictFiles=new Dictionary<string,byte[]> {
				{ "AnyCpu.dll",SyntheticPe.AnyCpuDll() },
				{ "X86Only.dll",SyntheticPe.X86OnlyDll() },
				{ "AnyCpu32BitPreferred.dll",SyntheticPe.AnyCpu32BitPreferredDll() },
				{ "X64.dll",SyntheticPe.X64ManagedDll() },
				{ "MixedModeX86.dll",SyntheticPe.MixedModeX86Dll() },
			};
			Dictionary<string,bool[]> dictCanRun=new Dictionary<string,bool[]> {//{ 32-bit, 64-bit }
				{ "AnyCpu.dll",new[] { true,true } },
				{ "X86Only.dll",new[] { true,false } },
				//.NET loads "any CPU, 32-bit preferred" into an x86 process only (S3b-4); the .NET Framework into either (PeImageInfoTests).
				{ "AnyCpu32BitPreferred.dll",new[] { true,false } },
				{ "X64.dll",new[] { false,true } },
				{ "MixedModeX86.dll",new[] { true,false } },
			};
			using(SyntheticPe.TestFolder testFolder=SyntheticPe.NewFolder()) {
				foreach(KeyValuePair<string,byte[]> pair in dictFiles) {
					string path=testFolder.Write(pair.Key,pair.Value);
					foreach(bool? is64BitProcess in new bool?[] { false,true,null }) {
						bool is64=is64BitProcess??Environment.Is64BitProcess;
						bool canRun=dictCanRun[pair.Key][is64 ? 1 : 0];
						NativeFirstChooser nativeFirstChooser=NewChooser(is64BitProcess);
						VendorPath vendorPath=nativeFirstChooser.ChooseForManagedAssembly(path);
						string what=pair.Key+" in a "+(is64 ? "64" : "32")+"-bit process";
						Assert.AreEqual(Expected(canRun,is64),vendorPath,what);
						Assert.AreEqual(vendorPath,nativeFirstChooser.Choice,what);
						StringAssert.Contains(nativeFirstChooser.Reason,path,what);
						StringAssert.StartsWith(nativeFirstChooser.LogLine,"Synthetic vendor piece: "
							+(vendorPath==VendorPath.InProcess ? "in the program's process" : "in the 32-bit helper program")
							+" ("+(is64 ? "64" : "32")+"-bit process); ",what);
						if(!canRun && !is64) {
							StringAssert.Contains(nativeFirstChooser.Reason,"the 32-bit helper could not run it either",what);
						}
					}
				}
			}
		}

		[TestMethod]
		public void ManagedAssembly_TheChoiceIsMadeOncePerProcess() {
			using(SyntheticPe.TestFolder testFolder=SyntheticPe.NewFolder()) {
				string pathX86=testFolder.Write("X86Only.dll",SyntheticPe.X86OnlyDll());
				string pathAnyCpu=testFolder.Write("AnyCpu.dll",SyntheticPe.AnyCpuDll());
				NativeFirstChooser nativeFirstChooser=NewChooser(true);
				Assert.AreEqual(VendorPath.Helper32,nativeFirstChooser.ChooseForManagedAssembly(pathX86));
				string logLine=nativeFirstChooser.LogLine;
				//A later call, even for another file, returns the first choice and logs nothing new.
				Assert.AreEqual(VendorPath.Helper32,nativeFirstChooser.ChooseForManagedAssembly(pathAnyCpu));
				Assert.AreEqual(VendorPath.Helper32,nativeFirstChooser.ChooseForNativeDll("Anything.dll",PeImageInfo.IMAGE_FILE_MACHINE_AMD64,
					new[] { testFolder.Path }));
				Assert.AreEqual(logLine,nativeFirstChooser.LogLine);
				//A new process (ResetForTests) chooses again.
				nativeFirstChooser.ResetForTests(true);
				Assert.IsNull(nativeFirstChooser.Choice);
				Assert.AreEqual("",nativeFirstChooser.LogLine);
				Assert.AreEqual(VendorPath.InProcess,nativeFirstChooser.ChooseForManagedAssembly(pathAnyCpu));
			}
		}

		[TestMethod]
		public void ManagedAssembly_AFileThatCannotBeClassified_StaysInTheProcess_WhereLoadingItSaysWhy() {
			using(SyntheticPe.TestFolder testFolder=SyntheticPe.NewFolder()) {
				string pathMissing=Path.Combine(testFolder.Path,"Missing.dll");
				string pathText=testFolder.Write("Text.dll",System.Text.Encoding.ASCII.GetBytes("not a PE file"));
				string pathNative=testFolder.Write("Native.dll",SyntheticPe.NativeX86Dll());
				foreach(string path in new[] { pathMissing,pathText,pathNative }) {
					foreach(bool is64BitProcess in new[] { false,true }) {
						NativeFirstChooser nativeFirstChooser=NewChooser(is64BitProcess);
						Assert.AreEqual(VendorPath.InProcess,nativeFirstChooser.ChooseForManagedAssembly(path),path);
						StringAssert.Contains(nativeFirstChooser.Reason,"the header could not be classified",path);
					}
				}
			}
		}

		[TestMethod]
		public void NativeDll_EachMachineAtEachBitness_FoundInTheFirstFolderThatHasIt() {
			using(SyntheticPe.TestFolder testFolderFirst=SyntheticPe.NewFolder())
			using(SyntheticPe.TestFolder testFolderSecond=SyntheticPe.NewFolder()) {
				Dictionary<string,bool[]> dictCanRun=new Dictionary<string,bool[]> {//{ 32-bit, 64-bit }
					{ "x86",new[] { true,false } },
					{ "x64",new[] { false,true } },
					{ "arm64",new[] { false,false } },
				};
				Dictionary<string,byte[]> dictFiles=new Dictionary<string,byte[]> {
					{ "x86",SyntheticPe.NativeX86Dll() },
					{ "x64",SyntheticPe.NativeX64Dll() },
					{ "arm64",SyntheticPe.NativeArm64Dll() },
				};
				foreach(string machine in dictFiles.Keys) {
					string fileName="Synthetic"+machine+".dll";
					testFolderSecond.Write(fileName,dictFiles[machine]);
					//The first folder has the same name with the other machine: the first folder wins, as the loader's search order does.
					testFolderFirst.Write(fileName,machine=="x64" ? SyntheticPe.NativeX86Dll() : SyntheticPe.NativeX64Dll());
					foreach(bool? is64BitProcess in new bool?[] { false,true,null }) {
						bool is64=is64BitProcess??Environment.Is64BitProcess;
						NativeFirstChooser nativeFirstChooser=NewChooser(is64BitProcess);
						VendorPath vendorPath=nativeFirstChooser.ChooseForNativeDll(fileName,PeImageInfo.IMAGE_FILE_MACHINE_I386,
							new[] { Path.Combine(testFolderSecond.Path,"none"),testFolderSecond.Path });
						Assert.AreEqual(Expected(dictCanRun[machine][is64 ? 1 : 0],is64),vendorPath,machine+(is64 ? " 64" : " 32"));
						StringAssert.Contains(nativeFirstChooser.Reason,Path.Combine(testFolderSecond.Path,fileName));
						NativeFirstChooser nativeFirstChooserFirst=NewChooser(is64BitProcess);
						nativeFirstChooserFirst.ChooseForNativeDll(fileName,PeImageInfo.IMAGE_FILE_MACHINE_I386,
							new[] { testFolderFirst.Path,testFolderSecond.Path });
						StringAssert.Contains(nativeFirstChooserFirst.Reason,Path.Combine(testFolderFirst.Path,fileName));
					}
				}
			}
		}

		[TestMethod]
		public void NativeDll_NotFound_CountsAsTheVendorsKnownBuild() {
			using(SyntheticPe.TestFolder testFolder=SyntheticPe.NewFolder()) {
				foreach(bool? is64BitProcess in new bool?[] { false,true,null }) {
					bool is64=is64BitProcess??Environment.Is64BitProcess;
					NativeFirstChooser nativeFirstChooser=NewChooser(is64BitProcess);
					VendorPath vendorPath=nativeFirstChooser.ChooseForNativeDll("MedVisorInterface.dll",PeImageInfo.IMAGE_FILE_MACHINE_I386,
						new[] { testFolder.Path });
					Assert.AreEqual(is64 ? VendorPath.Helper32 : VendorPath.InProcess,vendorPath);
					StringAssert.Contains(nativeFirstChooser.Reason,"MedVisorInterface.dll was not found; the vendor's known build is a native x86 image");
					NativeFirstChooser nativeFirstChooserX64=NewChooser(is64BitProcess);
					Assert.AreEqual(VendorPath.InProcess,nativeFirstChooserX64.ChooseForNativeDll("Missing.dll",PeImageInfo.IMAGE_FILE_MACHINE_AMD64,
						new[] { testFolder.Path }),"A known x64 build: the program's process in a 64-bit one, and a 32-bit one keeps its own as well.");
				}
			}
		}

		[TestMethod]
		public void NativeDll_TheLoadersFolders_StartWithTheProgramsFolderAndTheSystemFolder() {
			List<string> listFolders=NativeFirstChooser.GetNativeDllSearchFolders();
			Assert.AreEqual(AppDomain.CurrentDomain.BaseDirectory,listFolders[0]);
			Assert.AreEqual(Environment.SystemDirectory,listFolders[1]);
			Assert.AreEqual(Environment.GetFolderPath(Environment.SpecialFolder.Windows),listFolders[2]);
			Assert.AreEqual(Environment.CurrentDirectory,listFolders[3]);
			//kernel32.dll is found in the system folder, and it is of this process's machine type.
			string path=NativeFirstChooser.FindNativeDll("kernel32.dll");
			Assert.AreEqual(Path.Combine(Environment.SystemDirectory,"kernel32.dll"),path);
			NativeFirstChooser nativeFirstChooser=NewChooser(null);
			Assert.AreEqual(VendorPath.InProcess,nativeFirstChooser.ChooseForNativeDll("kernel32.dll",PeImageInfo.IMAGE_FILE_MACHINE_I386));
			Assert.AreEqual("",NativeFirstChooser.FindNativeDll("NoSuchSyntheticVendorLibrary_7c1e04d2.dll"));
		}

		[TestMethod]
		public void Com_ActivatedHere_ChoosesTheProcess_AndKeepsActivatingHere() {
			foreach(bool? is64BitProcess in new bool?[] { false,true,null }) {
				NativeFirstChooser nativeFirstChooser=NewChooser(is64BitProcess);
				int countActivations=0;
				object obj=nativeFirstChooser.ActivateComObject(() => { countActivations++; return new object(); });
				Assert.IsNotNull(obj);
				Assert.AreEqual(VendorPath.InProcess,nativeFirstChooser.Choice);
				Assert.AreEqual("its class activated in this process",nativeFirstChooser.Reason);
				Assert.IsNotNull(nativeFirstChooser.ActivateComObject(() => { countActivations++; return new object(); }));
				Assert.AreEqual(2,countActivations,"Each call activates, as the vendor code's new did.");
				//Once this process is chosen, a later failure is the vendor's, thrown as before (no switch to the helper).
				COMException comException=Assert.ThrowsExactly<COMException>(() => nativeFirstChooser.ActivateComObject<object>(
					() => throw new COMException("Synthetic: class not registered",NativeFirstChooser.REGDB_E_CLASSNOTREG)));
				Assert.AreEqual(NativeFirstChooser.REGDB_E_CLASSNOTREG,comException.HResult);
				Assert.AreEqual(VendorPath.InProcess,nativeFirstChooser.Choice);
			}
		}

		[TestMethod]
		public void Com_NotActivatableHere_EachError_HelperIn64Bit_ThrownIn32Bit() {
			Func<Exception>[] arrayFailures=new Func<Exception>[] {
				() => new COMException("Synthetic: class not registered",NativeFirstChooser.REGDB_E_CLASSNOTREG),
				() => new COMException("Synthetic: server execution failed",NativeFirstChooser.CO_E_SERVER_EXEC_FAILURE),
				() => new TargetInvocationException(new COMException("Synthetic: class not registered",NativeFirstChooser.REGDB_E_CLASSNOTREG)),
			};
			foreach(Func<Exception> funcFailure in arrayFailures) {
				Assert.IsTrue(NativeFirstChooser.IsNotActivatableHere(funcFailure()));
				//64-bit: the helper, nothing thrown, and no further activation is tried in this process.
				NativeFirstChooser nativeFirstChooser64=NewChooser(true);
				int countActivations=0;
				object obj=nativeFirstChooser64.ActivateComObject<object>(() => { countActivations++; throw funcFailure(); });
				Assert.IsNull(obj);
				Assert.AreEqual(VendorPath.Helper32,nativeFirstChooser64.Choice);
				StringAssert.Contains(nativeFirstChooser64.Reason,"activating it here failed with 0x8");
				StringAssert.Contains(nativeFirstChooser64.LogLine,"in the 32-bit helper program (64-bit process)");
				Assert.IsNull(nativeFirstChooser64.ActivateComObject(() => { countActivations++; return new object(); }));
				Assert.AreEqual(1,countActivations,"Tried once.");
				//32-bit: the process, and the failure is thrown as the vendor code's was.
				NativeFirstChooser nativeFirstChooser32=NewChooser(false);
				Exception exception=funcFailure();
				Exception exceptionThrown=null;
				try {
					nativeFirstChooser32.ActivateComObject<object>(() => throw exception);
				}
				catch(Exception ex) {
					exceptionThrown=ex;
				}
				Assert.AreSame(exception,exceptionThrown,"The vendor's own failure, unchanged.");
				Assert.AreEqual(VendorPath.InProcess,nativeFirstChooser32.Choice);
				StringAssert.Contains(nativeFirstChooser32.Reason,"the 32-bit helper could not activate it either");
				Assert.IsNotNull(nativeFirstChooser32.ActivateComObject(() => new object()),"The next call activates here again.");
			}
		}

		[TestMethod]
		public void Com_AnyOtherFailure_IsThrown_AndChoosesNothing() {
			foreach(bool is64BitProcess in new[] { false,true }) {
				foreach(Exception exception in new Exception[] { new COMException("Synthetic: unspecified",E_FAIL),new InvalidOperationException("Synthetic"),
					new TargetInvocationException(new COMException("Synthetic: unspecified",E_FAIL)) })
				{
					Assert.IsFalse(NativeFirstChooser.IsNotActivatableHere(exception));
					NativeFirstChooser nativeFirstChooser=NewChooser(is64BitProcess);
					Exception exceptionThrown=null;
					try {
						nativeFirstChooser.ActivateComObject<object>(() => throw exception);
					}
					catch(Exception ex) {
						exceptionThrown=ex;
					}
					Assert.AreSame(exception,exceptionThrown);
					Assert.IsNull(nativeFirstChooser.Choice,"The next call tries again, as the vendor code did.");
					Assert.IsNotNull(nativeFirstChooser.ActivateComObject(() => new object()));
					Assert.AreEqual(VendorPath.InProcess,nativeFirstChooser.Choice);
				}
			}
		}

		[TestMethod]
		public void Com_AClassRegisteredNowhere_FailsWithTheErrorTheChooserCatches() {
			//A real activation (Windows' own answer, no vendor class): REGDB_E_CLASSNOTREG, at the bitness the test runs at.
			NativeFirstChooser nativeFirstChooser=NewChooser(null);
			Func<object> funcActivate=() => Activator.CreateInstance(Type.GetTypeFromCLSID(_guidNotRegistered));
			if(Environment.Is64BitProcess) {
				Assert.IsNull(nativeFirstChooser.ActivateComObject(funcActivate));
				Assert.AreEqual(VendorPath.Helper32,nativeFirstChooser.Choice);
				StringAssert.Contains(nativeFirstChooser.Reason,"0x80040154");
				return;
			}
			COMException comException=Assert.ThrowsExactly<COMException>(() => nativeFirstChooser.ActivateComObject(funcActivate));
			Assert.AreEqual(NativeFirstChooser.REGDB_E_CLASSNOTREG,comException.HResult);
			Assert.AreEqual(VendorPath.InProcess,nativeFirstChooser.Choice);
		}

		[TestMethod]
		public void TheFourVendorPieces_AtTheBitnessThisTestRunsAt() {
			//The rule as each vendor piece meets it (S3a-14 section 2): a 32-bit program runs all four in its own process, as before S3a-13 and
			//S3a-14; a 64-bit one runs an AnyCPU SigPlusNET and CDR's out-of-process server itself, and an x86-only SigPlusNET, Guru (x86 only) and
			//QBFC 10 (a 32-bit in-process server, so not registered for a 64-bit caller) in the helper.
			bool is64=Environment.Is64BitProcess;
			using(SyntheticPe.TestFolder testFolder=SyntheticPe.NewFolder()) {
				string pathAnyCpu=testFolder.Write("SigPlusNET.dll",SyntheticPe.AnyCpuDll());
				Assert.AreEqual(VendorPath.InProcess,NewChooser(null).ChooseForManagedAssembly(pathAnyCpu),"Topaz, current SigPlusNET (AnyCPU)");
				string pathX86=testFolder.Write("SigPlusNETx86.dll",SyntheticPe.X86OnlyDll());
				Assert.AreEqual(is64 ? VendorPath.Helper32 : VendorPath.InProcess,NewChooser(null).ChooseForManagedAssembly(pathX86),
					"Topaz, an x86-only SigPlusNET");
				testFolder.Write("MedVisorInterface.dll",SyntheticPe.NativeX86Dll());
				Assert.AreEqual(is64 ? VendorPath.Helper32 : VendorPath.InProcess,NewChooser(null).ChooseForNativeDll("MedVisorInterface.dll",
					PeImageInfo.IMAGE_FILE_MACHINE_I386,new[] { testFolder.Path }),"Guru");
				NativeFirstChooser nativeFirstChooserCdr=NewChooser(null);
				Assert.IsNotNull(nativeFirstChooserCdr.ActivateComObject(() => new object()),"Schick CDR: an out-of-process server activates in either bitness.");
				Assert.AreEqual(VendorPath.InProcess,nativeFirstChooserCdr.Choice);
				NativeFirstChooser nativeFirstChooserQbfc=NewChooser(null);
				Func<object> funcQbfc=() => {
					if(Environment.Is64BitProcess) {
						throw new COMException("Synthetic: a 32-bit in-process server is not registered for a 64-bit caller",NativeFirstChooser.REGDB_E_CLASSNOTREG);
					}
					return new object();
				};
				object qbfc=nativeFirstChooserQbfc.ActivateComObject(funcQbfc);
				Assert.AreEqual(is64 ? VendorPath.Helper32 : VendorPath.InProcess,nativeFirstChooserQbfc.Choice,"QuickBooks Desktop, QBFC 10");
				Assert.AreEqual(is64,qbfc==null);
			}
		}
	}
}
