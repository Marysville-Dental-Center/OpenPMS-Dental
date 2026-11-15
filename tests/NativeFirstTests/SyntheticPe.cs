// Synthetic PE files for the native-first tests (S3a-14n): the smallest image whose headers PeImageInfo reads, with a chosen machine type,
// optional header (PE32 or PE32+) and, for a .NET assembly, a CLI header with chosen flags. No code: nothing can or does load these files.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.IO;
using CodeBase;

namespace NativeFirstTests {
	internal static class SyntheticPe {
		private const int OFFSET_PE=0x80;
		private const int SECTION_RVA=0x2000;
		private const int SECTION_RAW=0x400;
		private const int SECTION_SIZE=0x200;

		///<summary>An IL-only assembly for any CPU (PE32, x86 machine, ILONLY), as Topaz's current SigPlusNET is.</summary>
		public static byte[] AnyCpuDll() {
			return Build(PeImageInfo.IMAGE_FILE_MACHINE_I386,false,PeImageInfo.COMIMAGE_FLAGS_ILONLY);
		}

		///<summary>An IL-only assembly that requires 32-bit (ILONLY and 32BITREQUIRED), as an x86 build of a managed DLL is.</summary>
		public static byte[] X86OnlyDll() {
			return Build(PeImageInfo.IMAGE_FILE_MACHINE_I386,false,PeImageInfo.COMIMAGE_FLAGS_ILONLY|PeImageInfo.COMIMAGE_FLAGS_32BITREQUIRED);
		}

		///<summary>An IL-only assembly for any CPU, 32-bit preferred (ILONLY, 32BITREQUIRED and 32BITPREFERRED).</summary>
		public static byte[] AnyCpu32BitPreferredDll() {
			return Build(PeImageInfo.IMAGE_FILE_MACHINE_I386,false,
				PeImageInfo.COMIMAGE_FLAGS_ILONLY|PeImageInfo.COMIMAGE_FLAGS_32BITREQUIRED|PeImageInfo.COMIMAGE_FLAGS_32BITPREFERRED);
		}

		///<summary>An IL-only assembly built for x64 (PE32+, AMD64 machine).</summary>
		public static byte[] X64ManagedDll() {
			return Build(PeImageInfo.IMAGE_FILE_MACHINE_AMD64,true,PeImageInfo.COMIMAGE_FLAGS_ILONLY);
		}

		///<summary>A mixed-mode (C++/CLI) assembly for x86: a CLI header without ILONLY.</summary>
		public static byte[] MixedModeX86Dll() {
			return Build(PeImageInfo.IMAGE_FILE_MACHINE_I386,false,0);
		}

		///<summary>A native DLL for x86 (no CLI header), as Guru's MedVisorInterface.dll is.</summary>
		public static byte[] NativeX86Dll() {
			return Build(PeImageInfo.IMAGE_FILE_MACHINE_I386,false,null);
		}

		///<summary>A native DLL for x64 (PE32+).</summary>
		public static byte[] NativeX64Dll() {
			return Build(PeImageInfo.IMAGE_FILE_MACHINE_AMD64,true,null);
		}

		///<summary>A native DLL for ARM64 (PE32+), which neither an x86 nor an x64 process loads.</summary>
		public static byte[] NativeArm64Dll() {
			return Build(PeImageInfo.IMAGE_FILE_MACHINE_ARM64,true,null);
		}

		///<summary>The image: a DOS header whose e_lfanew points at 0x80, "PE\0\0", the COFF header (one section), the optional header with 16 data
		///directories (the CLI header's, index 14, set when corFlags is given), the section table (one .text section at RVA 0x2000, raw at 0x400)
		///and, in that section, the CLI header (cb 72, runtime 2.5) with corFlags.</summary>
		public static byte[] Build(ushort machine,bool isPe32Plus,uint? corFlags) {
			byte[] bytes=new byte[SECTION_RAW+SECTION_SIZE];
			using(MemoryStream stream=new MemoryStream(bytes))
			using(BinaryWriter writer=new BinaryWriter(stream)) {
				writer.Write((ushort)0x5A4D);//"MZ"
				stream.Position=0x3C;
				writer.Write(OFFSET_PE);
				stream.Position=OFFSET_PE;
				writer.Write(0x00004550u);//"PE\0\0"
				ushort sizeOptionalHeader=(ushort)(isPe32Plus ? 240 : 224);
				writer.Write(machine);
				writer.Write((ushort)1);//NumberOfSections
				writer.Write(0u);//TimeDateStamp
				writer.Write(0u);//PointerToSymbolTable
				writer.Write(0u);//NumberOfSymbols
				writer.Write(sizeOptionalHeader);
				writer.Write((ushort)0x2102);//Characteristics: executable, 32-bit machine word, DLL
				long offsetOptional=stream.Position;
				writer.Write((ushort)(isPe32Plus ? 0x020B : 0x010B));
				stream.Position=offsetOptional+(isPe32Plus ? 108 : 92);
				writer.Write(16u);//NumberOfRvaAndSizes
				long offsetDirectories=stream.Position;
				if(corFlags!=null) {
					stream.Position=offsetDirectories+14*8;
					writer.Write((uint)SECTION_RVA);
					writer.Write(72u);
				}
				stream.Position=offsetOptional+sizeOptionalHeader;
				writer.Write(new byte[] { (byte)'.',(byte)'t',(byte)'e',(byte)'x',(byte)'t',0,0,0 });
				writer.Write((uint)SECTION_SIZE);//VirtualSize
				writer.Write((uint)SECTION_RVA);//VirtualAddress
				writer.Write((uint)SECTION_SIZE);//SizeOfRawData
				writer.Write((uint)SECTION_RAW);//PointerToRawData
				if(corFlags!=null) {
					stream.Position=SECTION_RAW;
					writer.Write(72u);//cb
					writer.Write((ushort)2);//MajorRuntimeVersion
					writer.Write((ushort)5);//MinorRuntimeVersion
					writer.Write(0u);//MetaData RVA
					writer.Write(0u);//MetaData size
					writer.Write(corFlags.Value);
				}
			}
			return bytes;
		}

		///<summary>A new empty folder of the test's own under the temporary folder (TestFolder.Dispose deletes it).</summary>
		public static TestFolder NewFolder() {
			return new TestFolder();
		}

		internal sealed class TestFolder:IDisposable {
			public readonly string Path=System.IO.Path.Combine(System.IO.Path.GetTempPath(),"NativeFirstTests_"+Guid.NewGuid().ToString("N"));

			public TestFolder() {
				Directory.CreateDirectory(Path);
			}

			///<summary>Writes bytes as fileName in this folder and returns its path.</summary>
			public string Write(string fileName,byte[] bytes) {
				string path=System.IO.Path.Combine(Path,fileName);
				File.WriteAllBytes(path,bytes);
				return path;
			}

			public void Dispose() {
				if(Directory.Exists(Path)) {
					Directory.Delete(Path,true);
				}
			}
		}
	}
}
