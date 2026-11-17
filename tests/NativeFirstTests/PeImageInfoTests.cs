// The PE and CLI header reader of the native-first chooser (S3a-14n of the .NET 10 move, docs/port-notes/S3a-14.md, section "S3a-14n: native
// first"), against synthetic files of each kind and against real files this process has loaded (CodeBase.dll, this test assembly, the
// system's kernel32.dll), which must read as runnable at the bitness the test runs at (the lane runs this assembly as x86 and as x64).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.IO;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NativeFirstTests {
	[TestClass]
	public class PeImageInfoTests {
		private static PeImageInfo Read(byte[] bytes) {
			using(MemoryStream stream=new MemoryStream(bytes)) {
				return PeImageInfo.Read(stream);
			}
		}

		[TestMethod]
		public void AnyCpuDll_IsManagedIlOnly_RunsAtEitherBitness() {
			PeImageInfo peImageInfo=Read(SyntheticPe.AnyCpuDll());
			Assert.AreEqual(PeImageKind.Managed,peImageInfo.Kind);
			Assert.AreEqual(PeImageInfo.IMAGE_FILE_MACHINE_I386,peImageInfo.Machine);
			Assert.IsFalse(peImageInfo.IsPe32Plus);
			Assert.IsTrue(peImageInfo.IsIlOnly);
			Assert.IsFalse(peImageInfo.Is32BitRequired);
			Assert.IsTrue(peImageInfo.CanRunInProcess(false));
			Assert.IsTrue(peImageInfo.CanRunInProcess(true));
			Assert.AreEqual("a managed IL-only assembly for any CPU",peImageInfo.Describe());
		}

		[TestMethod]
		public void X86OnlyDll_Requires32Bit_RunsOnlyInA32BitProcess() {
			PeImageInfo peImageInfo=Read(SyntheticPe.X86OnlyDll());
			Assert.AreEqual(PeImageKind.Managed,peImageInfo.Kind);
			Assert.IsTrue(peImageInfo.IsIlOnly);
			Assert.IsTrue(peImageInfo.Is32BitRequired);
			Assert.IsTrue(peImageInfo.CanRunInProcess(false));
			Assert.IsFalse(peImageInfo.CanRunInProcess(true));
			Assert.AreEqual("a managed IL-only assembly that requires 32-bit (x86)",peImageInfo.Describe());
		}

		[TestMethod]
		public void AnyCpu32BitPreferredDll_RunsAtEitherBitness() {
			PeImageInfo peImageInfo=Read(SyntheticPe.AnyCpu32BitPreferredDll());
			Assert.AreEqual(PeImageKind.Managed,peImageInfo.Kind);
			Assert.IsFalse(peImageInfo.Is32BitRequired,"32BITREQUIRED with 32BITPREFERRED is any CPU.");
			Assert.IsTrue(peImageInfo.CanRunInProcess(false));
			Assert.IsTrue(peImageInfo.CanRunInProcess(true));
		}

		[TestMethod]
		public void X64ManagedDll_RunsOnlyInA64BitProcess() {
			PeImageInfo peImageInfo=Read(SyntheticPe.X64ManagedDll());
			Assert.AreEqual(PeImageKind.Managed,peImageInfo.Kind);
			Assert.AreEqual(PeImageInfo.IMAGE_FILE_MACHINE_AMD64,peImageInfo.Machine);
			Assert.IsTrue(peImageInfo.IsPe32Plus);
			Assert.IsFalse(peImageInfo.CanRunInProcess(false));
			Assert.IsTrue(peImageInfo.CanRunInProcess(true));
			Assert.AreEqual("a managed IL-only x64 assembly",peImageInfo.Describe());
		}

		[TestMethod]
		public void MixedModeX86Dll_RunsOnlyInA32BitProcess() {
			PeImageInfo peImageInfo=Read(SyntheticPe.MixedModeX86Dll());
			Assert.AreEqual(PeImageKind.Managed,peImageInfo.Kind);
			Assert.IsFalse(peImageInfo.IsIlOnly);
			Assert.IsTrue(peImageInfo.CanRunInProcess(false));
			Assert.IsFalse(peImageInfo.CanRunInProcess(true));
			Assert.AreEqual("a managed mixed-mode x86 assembly",peImageInfo.Describe());
		}

		[TestMethod]
		public void NativeDlls_RunOnlyWhereTheirMachineIsTheProcesss() {
			PeImageInfo peImageInfoX86=Read(SyntheticPe.NativeX86Dll());
			Assert.AreEqual(PeImageKind.Native,peImageInfoX86.Kind);
			Assert.AreEqual(0u,peImageInfoX86.CorFlags);
			Assert.IsTrue(peImageInfoX86.CanRunInProcess(false));
			Assert.IsFalse(peImageInfoX86.CanRunInProcess(true));
			Assert.AreEqual("a native x86 image",peImageInfoX86.Describe());
			PeImageInfo peImageInfoX64=Read(SyntheticPe.NativeX64Dll());
			Assert.AreEqual(PeImageKind.Native,peImageInfoX64.Kind);
			Assert.IsTrue(peImageInfoX64.IsPe32Plus);
			Assert.IsFalse(peImageInfoX64.CanRunInProcess(false));
			Assert.IsTrue(peImageInfoX64.CanRunInProcess(true));
			PeImageInfo peImageInfoArm64=Read(SyntheticPe.NativeArm64Dll());
			Assert.AreEqual(PeImageKind.Native,peImageInfoArm64.Kind);
			Assert.IsFalse(peImageInfoArm64.CanRunInProcess(false));
			Assert.IsFalse(peImageInfoArm64.CanRunInProcess(true));
			Assert.AreEqual("a native ARM64 image",peImageInfoArm64.Describe());
		}

		[TestMethod]
		public void NotAPeFile_ReadsAsNotPe_AndRunsNowhere() {
			byte[] bytesMzOnly=SyntheticPe.NativeX86Dll();
			bytesMzOnly[0x80]=(byte)'X';//the PE signature broken
			byte[] bytesBadMagic=SyntheticPe.NativeX86Dll();
			bytesBadMagic[0x80+24]=0x07;//the optional header's magic
			byte[] bytesFarOffset=SyntheticPe.NativeX86Dll();
			bytesFarOffset[0x3C+2]=0x10;//e_lfanew beyond the file
			foreach(byte[] bytes in new[] { new byte[0],new byte[10],new byte[0x200],System.Text.Encoding.ASCII.GetBytes(new string('x',0x200)),
				bytesMzOnly,bytesBadMagic,bytesFarOffset })
			{
				PeImageInfo peImageInfo=Read(bytes);
				Assert.AreEqual(PeImageKind.NotPe,peImageInfo.Kind);
				Assert.IsFalse(peImageInfo.CanRunInProcess(false));
				Assert.IsFalse(peImageInfo.CanRunInProcess(true));
				Assert.AreEqual("not a PE image",peImageInfo.Describe());
			}
		}

		[TestMethod]
		public void ACliDirectoryOutsideEverySection_ReadsAsNative() {
			byte[] bytes=SyntheticPe.AnyCpuDll();
			//The section's virtual address moved away from the CLI directory's RVA (0x2000): the header is not in the file, so no CLI flags.
			bytes[0x80+24+224+12+1]=0x40;
			PeImageInfo peImageInfo=Read(bytes);
			Assert.AreEqual(PeImageKind.Native,peImageInfo.Kind);
		}

		[TestMethod]
		public void ReadFromAFile_IsTheSameAsFromItsBytes() {
			using(SyntheticPe.TestFolder testFolder=SyntheticPe.NewFolder()) {
				string path=testFolder.Write("Synthetic.dll",SyntheticPe.X86OnlyDll());
				PeImageInfo peImageInfo=PeImageInfo.Read(path);
				Assert.AreEqual(PeImageKind.Managed,peImageInfo.Kind);
				Assert.IsTrue(peImageInfo.Is32BitRequired);
				Assert.ThrowsExactly<FileNotFoundException>(() => PeImageInfo.Read(Path.Combine(testFolder.Path,"Missing.dll")));
			}
		}

		[TestMethod]
		public void TheFilesThisProcessLoaded_ReadAsRunnableAtItsBitness() {
			bool is64BitProcess=Environment.Is64BitProcess;
			Assert.AreEqual(is64BitProcess ? 8 : 4,IntPtr.Size,"The test runs at the bitness it reports.");
			foreach(string path in new[] { typeof(NativeFirstChooser).Assembly.Location,typeof(PeImageInfoTests).Assembly.Location }) {
				PeImageInfo peImageInfo=PeImageInfo.Read(path);
				Assert.AreEqual(PeImageKind.Managed,peImageInfo.Kind,path);
				Assert.IsTrue(peImageInfo.CanRunInProcess(is64BitProcess),path+" is loaded here, so it reads as runnable: "+peImageInfo.Describe());
			}
			//This process's kernel32.dll: the system folder of a 32-bit process on 64-bit Windows is SysWOW64, which file access is redirected to,
			//as the loader looks there.
			string pathKernel32=Path.Combine(Environment.SystemDirectory,"kernel32.dll");
			PeImageInfo peImageInfoKernel32=PeImageInfo.Read(pathKernel32);
			Assert.AreEqual(PeImageKind.Native,peImageInfoKernel32.Kind);
			Assert.AreEqual(is64BitProcess ? PeImageInfo.IMAGE_FILE_MACHINE_AMD64 : PeImageInfo.IMAGE_FILE_MACHINE_I386,peImageInfoKernel32.Machine);
			Assert.IsTrue(peImageInfoKernel32.CanRunInProcess(is64BitProcess));
			Assert.IsFalse(peImageInfoKernel32.CanRunInProcess(!is64BitProcess));
		}
	}
}
