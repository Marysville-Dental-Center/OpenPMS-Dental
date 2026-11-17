// The facts of a DLL or EXE file that tell whether this process can load it, read from the file's bytes without loading it (N64-A of the
// .NET 10 move, docs/port-notes/N64-A.md; the native 64-bit integration survey, docs/specs/native64-survey.md, items A2 and A4): the PE header's
// machine type and PE32 or PE32+ (Microsoft PE/COFF specification), and for a .NET assembly its CLI header flags (ECMA-335 II.25.3.3), the
// metadata version string (II.24.2.1), its TargetFrameworkAttribute and the core library it references (the Assembly, AssemblyRef, TypeRef,
// MemberRef and CustomAttribute tables, II.22). The .NET Framework has no System.Reflection.Metadata, and the program ships no Mono.Cecil, so
// this small reader does it; tests/N64ATests checks it against Mono.Cecil on every assembly of its output folder.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace CodeBase {
	///<summary>What a PE file (DLL or EXE) is, read from its headers and, for a .NET assembly, its metadata tables. Nothing is loaded. Read
	///returns an object for every file it can open: IsPe is false for a file that is not a PE image, IsManaged false for a native one.</summary>
	public sealed class PortableExecutableFile {
		public const ushort IMAGE_FILE_MACHINE_I386=0x014C;
		public const ushort IMAGE_FILE_MACHINE_AMD64=0x8664;
		public const ushort IMAGE_FILE_MACHINE_ARM64=0xAA64;
		public const uint COMIMAGE_FLAGS_ILONLY=0x00000001;
		public const uint COMIMAGE_FLAGS_32BITREQUIRED=0x00000002;
		public const uint COMIMAGE_FLAGS_32BITPREFERRED=0x00020000;
		private const ushort MAGIC_PE32=0x010B;
		private const ushort MAGIC_PE32_PLUS=0x020B;
		private const int INDEX_DIRECTORY_CLI_HEADER=14;
		private const uint METADATA_SIGNATURE=0x424A5342;//"BSJB"
		//The metadata tables this reader needs (ECMA-335 II.22).
		private const int TABLE_TYPE_REF=0x01;
		private const int TABLE_MEMBER_REF=0x0A;
		private const int TABLE_CUSTOM_ATTRIBUTE=0x0C;
		private const int TABLE_ASSEMBLY=0x20;
		private const int TABLE_ASSEMBLY_REF=0x23;
		private const int TABLE_COUNT=0x2D;//Module (0x00) to GenericParamConstraint (0x2C); the portable PDB tables are never in an assembly.

		///<summary>The file this was read from, or "" when read from bytes.</summary>
		public string Path="";
		///<summary>True when the file has a DOS header, a PE signature and a PE32 or PE32+ optional header.</summary>
		public bool IsPe;
		///<summary>The COFF header's machine type (IMAGE_FILE_MACHINE_...). 0 when not a PE file.</summary>
		public ushort Machine;
		///<summary>True for a PE32+ (64-bit) optional header.</summary>
		public bool IsPe32Plus;
		///<summary>True when the file has a CLI header: a .NET assembly (or module).</summary>
		public bool IsManaged;
		///<summary>The CLI header's flags (COMIMAGE_FLAGS_...). 0 for a native file.</summary>
		public uint CorFlags;
		///<summary>The metadata root's version string, for example "v4.0.30319" (every .NET Framework 4 and .NET assembly) or "v2.0.50727". ""
		///when not read.</summary>
		public string MetadataVersion="";
		///<summary>The value of the assembly's System.Runtime.Versioning.TargetFrameworkAttribute, for example ".NETFramework,Version=v4.8",
		///".NETStandard,Version=v2.0" or ".NETCoreApp,Version=v10.0". "" when the assembly has none (compilers before .NET Framework 4 wrote none)
		///or the tables could not be read.</summary>
		public string TargetFramework="";
		///<summary>The core library the assembly references: "mscorlib" (.NET Framework), "netstandard" (.NET Standard 2.0 and later),
		///"System.Runtime" (.NET Core, .NET and .NET Standard 1.x) or "System.Private.CoreLib". "" when it references none of them.</summary>
		public string CoreLibraryName="";
		///<summary>The version of the referenced CoreLibraryName, or null.</summary>
		public Version CoreLibraryVersion;
		///<summary>Why the metadata tables were not read, or "" when they were (or the file is not managed). For the log.</summary>
		public string MetadataError="";

		///<summary>IL only (no native code): the flag that lets an assembly be "any CPU".</summary>
		public bool IsIlOnly {
			get {
				return (CorFlags & COMIMAGE_FLAGS_ILONLY)!=0;
			}
		}

		///<summary>The 32BITREQUIRED flag (corflags /32BITREQ, platform x86), with or without 32BITPREFERRED.</summary>
		public bool Is32BitRequired {
			get {
				return (CorFlags & COMIMAGE_FLAGS_32BITREQUIRED)!=0;
			}
		}

		///<summary>The 32BITPREFERRED flag (platform "anycpu32bitpreferred", which compilers allow for an EXE only).</summary>
		public bool Is32BitPreferred {
			get {
				return (CorFlags & COMIMAGE_FLAGS_32BITPREFERRED)!=0;
			}
		}

		///<summary>A managed IL-only PE32 x86 image without 32BITREQUIRED, or with both 32BITREQUIRED and 32BITPREFERRED ("any CPU, 32-bit
		///preferred"): an "any CPU" assembly, which loads in a 32-bit and in a 64-bit process. The lane's x64 test run of tests/N64ATests showed
		///that a 64-bit .NET Framework process loads a DLL with both flags (N64-A, docs/port-notes/N64-A.md).</summary>
		public bool IsAnyCpu {
			get {
				return IsManaged && IsIlOnly && !IsPe32Plus && Machine==IMAGE_FILE_MACHINE_I386 && (!Is32BitRequired || Is32BitPreferred);
			}
		}

		///<summary>The machine type of this process: x86, x64 or ARM64 (RuntimeInformation.ProcessArchitecture). 0 for another.</summary>
		public static ushort GetProcessMachine() {
			switch(RuntimeInformation.ProcessArchitecture) {
				case Architecture.X86:
					return IMAGE_FILE_MACHINE_I386;
				case Architecture.X64:
					return IMAGE_FILE_MACHINE_AMD64;
				case Architecture.Arm64:
					return IMAGE_FILE_MACHINE_ARM64;
				default:
					return 0;
			}
		}

		///<summary>True when a process of machine type machineProcess (IMAGE_FILE_MACHINE_...) can map this image. For an assembly, the rule of
		///the runtime's binder: an any-CPU assembly (IsAnyCpu) loads in every process, every other one (32BITREQUIRED without 32BITPREFERRED;
		///mixed mode; PE32+) only in a process of its machine type. A native DLL only in a process of its machine type.</summary>
		public bool CanLoadInProcess(ushort machineProcess) {
			if(!IsPe) {
				return false;
			}
			if(IsAnyCpu) {
				return true;
			}
			return Machine==machineProcess;
		}

		///<summary>The machine type as the log names it: "x86", "x64", "ARM64" or "machine 0x...".</summary>
		public static string GetMachineName(ushort machine) {
			switch(machine) {
				case IMAGE_FILE_MACHINE_I386:
					return "x86";
				case IMAGE_FILE_MACHINE_AMD64:
					return "x64";
				case IMAGE_FILE_MACHINE_ARM64:
					return "ARM64";
				default:
					return "machine 0x"+machine.ToString("X4");
			}
		}

		///<summary>For the log and the messages: "a .NET assembly for any CPU", "a 32-bit only (x86) .NET assembly", "a 64-bit (x64) .NET
		///assembly", "a mixed-mode x86 .NET assembly", "a native 64-bit (x64) DLL" or "not a program file (no PE header)".</summary>
		public string DescribeBitness() {
			if(!IsPe) {
				return "not a program file (no PE header)";
			}
			string machine=GetMachineName(Machine);
			string bitness=(Machine==IMAGE_FILE_MACHINE_I386) ? "32-bit ("+machine+")" : (IsPe32Plus ? "64-bit ("+machine+")" : machine);
			if(!IsManaged) {
				return "a native "+bitness+" "+(IsPe32Plus ? "PE32+" : "PE32")+" file";
			}
			if(IsAnyCpu) {
				return Is32BitRequired ? "a .NET assembly for any CPU, 32-bit preferred" : "a .NET assembly for any CPU";
			}
			if(!IsIlOnly) {
				return "a mixed-mode "+bitness+" .NET assembly";
			}
			if(Machine==IMAGE_FILE_MACHINE_I386) {
				return "a 32-bit only (x86) .NET assembly";
			}
			return "a "+bitness+" .NET assembly";
		}

		///<summary>Reads the file at path (shared for reading, writing and deleting, so a file in use can be read).</summary>
		public static PortableExecutableFile Read(string path) {
			byte[] bytes;
			using(FileStream fileStream=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.ReadWrite|FileShare.Delete)) {
				bytes=new byte[fileStream.Length];
				int offset=0;
				while(offset<bytes.Length) {
					int count=fileStream.Read(bytes,offset,bytes.Length-offset);
					if(count<=0) {
						break;
					}
					offset+=count;
				}
			}
			PortableExecutableFile portableExecutableFile=Read(bytes);
			portableExecutableFile.Path=path;
			return portableExecutableFile;
		}

		///<summary>Reads the image in bytes: the DOS header's e_lfanew, the "PE\0\0" signature, the COFF header, the optional header's magic and
		///its CLI header directory, the section table to find RVAs in the file, the CLI header, the metadata root and its #~ stream. Never throws
		///for a malformed file: what could not be read stays at its default and, for the metadata, MetadataError says why.</summary>
		public static PortableExecutableFile Read(byte[] bytes) {
			PortableExecutableFile portableExecutableFile=new PortableExecutableFile();
			try {
				portableExecutableFile.ReadHeaders(bytes);
			}
			catch(Exception ex) {//a truncated or malformed file: an index out of range
				if(portableExecutableFile.IsManaged) {
					portableExecutableFile.MetadataError="The metadata could not be read ("+ex.GetType().Name+").";
				}
				else {
					portableExecutableFile.IsPe=false;
				}
			}
			return portableExecutableFile;
		}

		private void ReadHeaders(byte[] bytes) {
			if(bytes.Length<0x40 || ReadUInt16(bytes,0)!=0x5A4D) {//"MZ"
				return;
			}
			long offsetPe=ReadUInt32(bytes,0x3C);
			if(offsetPe+24>bytes.Length || ReadUInt32(bytes,(int)offsetPe)!=0x00004550) {//"PE\0\0"
				return;
			}
			int offsetCoff=(int)offsetPe+4;
			ushort machine=ReadUInt16(bytes,offsetCoff);
			ushort countSections=ReadUInt16(bytes,offsetCoff+2);
			ushort sizeOptionalHeader=ReadUInt16(bytes,offsetCoff+16);
			int offsetOptional=offsetCoff+20;
			if(sizeOptionalHeader<2 || offsetOptional+sizeOptionalHeader>bytes.Length) {
				return;
			}
			ushort magic=ReadUInt16(bytes,offsetOptional);
			if(magic!=MAGIC_PE32 && magic!=MAGIC_PE32_PLUS) {
				return;
			}
			IsPe=true;
			Machine=machine;
			IsPe32Plus=(magic==MAGIC_PE32_PLUS);
			int offsetCountDirectories=IsPe32Plus ? 108 : 92;
			int offsetDirectories=offsetCountDirectories+4;
			if(sizeOptionalHeader<offsetDirectories+(INDEX_DIRECTORY_CLI_HEADER+1)*8) {
				return;
			}
			uint countDirectories=ReadUInt32(bytes,offsetOptional+offsetCountDirectories);
			if(countDirectories<=INDEX_DIRECTORY_CLI_HEADER) {
				return;
			}
			uint rvaCli=ReadUInt32(bytes,offsetOptional+offsetDirectories+INDEX_DIRECTORY_CLI_HEADER*8);
			uint sizeCli=ReadUInt32(bytes,offsetOptional+offsetDirectories+INDEX_DIRECTORY_CLI_HEADER*8+4);
			if(rvaCli==0 || sizeCli<24) {
				return;
			}
			List<uint[]> listSections=new List<uint[]>();//virtual address, virtual size, raw pointer, raw size
			int offsetSections=offsetOptional+sizeOptionalHeader;
			for(int i=0;i<countSections;i++) {
				int offsetSection=offsetSections+i*40;
				if(offsetSection+40>bytes.Length) {
					break;
				}
				listSections.Add(new uint[] { ReadUInt32(bytes,offsetSection+12),ReadUInt32(bytes,offsetSection+8),ReadUInt32(bytes,offsetSection+20),
					ReadUInt32(bytes,offsetSection+16) });
			}
			int offsetCliHeader=RvaToOffset(listSections,rvaCli,bytes.Length);
			if(offsetCliHeader<0 || offsetCliHeader+24>bytes.Length) {
				return;
			}
			IsManaged=true;
			CorFlags=ReadUInt32(bytes,offsetCliHeader+16);
			uint rvaMetadata=ReadUInt32(bytes,offsetCliHeader+8);
			int offsetMetadata=RvaToOffset(listSections,rvaMetadata,bytes.Length);
			if(offsetMetadata<0) {
				MetadataError="The CLI header's metadata directory is outside the file.";
				return;
			}
			ReadMetadata(bytes,offsetMetadata);
		}

		///<summary>The file offset of rva, or -1 when no section holds it.</summary>
		private static int RvaToOffset(List<uint[]> listSections,uint rva,int lengthFile) {
			foreach(uint[] section in listSections) {
				uint size=Math.Max(section[1],section[3]);
				if(rva<section[0] || rva>=(long)section[0]+size) {
					continue;
				}
				long offset=(long)rva-section[0]+section[2];
				if(offset<0 || offset>=lengthFile) {
					return -1;
				}
				return (int)offset;
			}
			return -1;
		}

		///<summary>The metadata root (II.24.2.1), its stream headers (II.24.2.2) and the #~ stream (II.24.2.6).</summary>
		private void ReadMetadata(byte[] bytes,int offsetRoot) {
			if(ReadUInt32(bytes,offsetRoot)!=METADATA_SIGNATURE) {
				MetadataError="The metadata root has no BSJB signature.";
				return;
			}
			int lengthVersion=(int)ReadUInt32(bytes,offsetRoot+12);
			if(lengthVersion<0 || lengthVersion>255) {
				MetadataError="The metadata version string is "+lengthVersion+" bytes long.";
				return;
			}
			MetadataVersion=Encoding.UTF8.GetString(bytes,offsetRoot+16,lengthVersion).TrimEnd('\0');
			int offset=offsetRoot+16+lengthVersion;
			offset+=2;//Flags
			int countStreams=ReadUInt16(bytes,offset);
			offset+=2;
			int offsetTables=-1;
			int offsetStrings=-1;
			int sizeStrings=0;
			int offsetBlob=-1;
			int sizeBlob=0;
			for(int i=0;i<countStreams;i++) {
				int offsetStream=(int)ReadUInt32(bytes,offset);
				int sizeStream=(int)ReadUInt32(bytes,offset+4);
				offset+=8;
				int offsetNameEnd=offset;
				while(bytes[offsetNameEnd]!=0) {
					offsetNameEnd++;
				}
				string name=Encoding.ASCII.GetString(bytes,offset,offsetNameEnd-offset);
				offset+=((offsetNameEnd-offset)/4+1)*4;//the name, its terminator, padded to 4 bytes
				switch(name) {
					case "#~":
					case "#-":
						offsetTables=offsetRoot+offsetStream;
						break;
					case "#Strings":
						offsetStrings=offsetRoot+offsetStream;
						sizeStrings=sizeStream;
						break;
					case "#Blob":
						offsetBlob=offsetRoot+offsetStream;
						sizeBlob=sizeStream;
						break;
				}
			}
			if(offsetTables<0 || offsetStrings<0) {
				MetadataError="The metadata has no #~ or #Strings stream.";
				return;
			}
			MetadataTables metadataTables=new MetadataTables(bytes,offsetTables,offsetStrings,sizeStrings,offsetBlob,sizeBlob);
			if(metadataTables.Error!="") {
				MetadataError=metadataTables.Error;
				return;
			}
			ReadCoreLibrary(metadataTables);
			ReadTargetFramework(metadataTables);
		}

		///<summary>CoreLibraryName and CoreLibraryVersion from the AssemblyRef table (II.22.5): MajorVersion, MinorVersion, BuildNumber,
		///RevisionNumber, Flags, PublicKeyOrToken, Name, Culture, HashValue.</summary>
		private void ReadCoreLibrary(MetadataTables metadataTables) {
			string[] arrayCoreNames=new[] { "System.Private.CoreLib","System.Runtime","netstandard","mscorlib" };
			int rankFound=int.MaxValue;
			for(int row=1;row<=metadataTables.GetRowCount(TABLE_ASSEMBLY_REF);row++) {
				int offsetRow=metadataTables.GetRowOffset(TABLE_ASSEMBLY_REF,row);
				Version version=new Version(ReadUInt16(metadataTables.Bytes,offsetRow),ReadUInt16(metadataTables.Bytes,offsetRow+2),
					ReadUInt16(metadataTables.Bytes,offsetRow+4),ReadUInt16(metadataTables.Bytes,offsetRow+6));
				int offsetName=offsetRow+12+metadataTables.SizeBlobIndex;
				string name=metadataTables.ReadString(metadataTables.ReadIndex(offsetName,metadataTables.SizeStringIndex));
				int rank=Array.IndexOf(arrayCoreNames,name);
				if(rank>=0 && rank<rankFound) {//an assembly that references several (a facade) is named by the newest
					rankFound=rank;
					CoreLibraryName=name;
					CoreLibraryVersion=version;
				}
			}
		}

		///<summary>TargetFramework: the CustomAttribute rows (II.22.10) whose parent is the Assembly row and whose constructor is a MemberRef
		///(II.22.25) of the TypeRef (II.22.38) System.Runtime.Versioning.TargetFrameworkAttribute; the value blob is the prolog 0x0001 and one
		///SerString (II.23.3).</summary>
		private void ReadTargetFramework(MetadataTables metadataTables) {
			int countCustomAttributes=metadataTables.GetRowCount(TABLE_CUSTOM_ATTRIBUTE);
			if(countCustomAttributes==0 || metadataTables.GetRowCount(TABLE_ASSEMBLY)==0) {
				return;
			}
			int sizeHasCustomAttribute=metadataTables.GetCodedIndexSize(5,MetadataTables.ArrayHasCustomAttribute);
			int sizeCustomAttributeType=metadataTables.GetCodedIndexSize(3,MetadataTables.ArrayCustomAttributeType);
			int sizeMemberRefParent=metadataTables.GetCodedIndexSize(3,MetadataTables.ArrayMemberRefParent);
			int sizeResolutionScope=metadataTables.GetCodedIndexSize(2,MetadataTables.ArrayResolutionScope);
			const int TAG_HAS_CUSTOM_ATTRIBUTE_ASSEMBLY=14;
			const int TAG_CUSTOM_ATTRIBUTE_TYPE_MEMBER_REF=3;
			const int TAG_MEMBER_REF_PARENT_TYPE_REF=1;
			for(int row=1;row<=countCustomAttributes;row++) {
				int offsetRow=metadataTables.GetRowOffset(TABLE_CUSTOM_ATTRIBUTE,row);
				uint parent=metadataTables.ReadIndex(offsetRow,sizeHasCustomAttribute);
				if((parent & 0x1F)!=TAG_HAS_CUSTOM_ATTRIBUTE_ASSEMBLY) {
					continue;
				}
				uint type=metadataTables.ReadIndex(offsetRow+sizeHasCustomAttribute,sizeCustomAttributeType);
				if((type & 0x07)!=TAG_CUSTOM_ATTRIBUTE_TYPE_MEMBER_REF) {
					continue;
				}
				int rowMemberRef=(int)(type>>3);
				if(rowMemberRef<1 || rowMemberRef>metadataTables.GetRowCount(TABLE_MEMBER_REF)) {
					continue;
				}
				uint classMemberRef=metadataTables.ReadIndex(metadataTables.GetRowOffset(TABLE_MEMBER_REF,rowMemberRef),sizeMemberRefParent);
				if((classMemberRef & 0x07)!=TAG_MEMBER_REF_PARENT_TYPE_REF) {
					continue;
				}
				int rowTypeRef=(int)(classMemberRef>>3);
				if(rowTypeRef<1 || rowTypeRef>metadataTables.GetRowCount(TABLE_TYPE_REF)) {
					continue;
				}
				int offsetTypeRef=metadataTables.GetRowOffset(TABLE_TYPE_REF,rowTypeRef)+sizeResolutionScope;
				string typeName=metadataTables.ReadString(metadataTables.ReadIndex(offsetTypeRef,metadataTables.SizeStringIndex));
				string typeNamespace=metadataTables.ReadString(metadataTables.ReadIndex(offsetTypeRef+metadataTables.SizeStringIndex,
					metadataTables.SizeStringIndex));
				if(typeName!="TargetFrameworkAttribute" || typeNamespace!="System.Runtime.Versioning") {
					continue;
				}
				uint indexValue=metadataTables.ReadIndex(offsetRow+sizeHasCustomAttribute+sizeCustomAttributeType,metadataTables.SizeBlobIndex);
				byte[] blob=metadataTables.ReadBlob(indexValue);
				if(blob==null || blob.Length<3 || blob[0]!=0x01 || blob[1]!=0x00 || blob[2]==0xFF) {//no prolog, or a null string
					continue;
				}
				int offsetString=2;
				int lengthString=ReadCompressedUInt(blob,ref offsetString);
				if(lengthString<0 || offsetString+lengthString>blob.Length) {
					continue;
				}
				TargetFramework=Encoding.UTF8.GetString(blob,offsetString,lengthString);
				return;
			}
		}

		///<summary>An unsigned compressed integer (II.23.2) at offset, which moves past it; -1 for a malformed one.</summary>
		private static int ReadCompressedUInt(byte[] bytes,ref int offset) {
			byte first=bytes[offset];
			if((first & 0x80)==0) {
				offset+=1;
				return first;
			}
			if((first & 0xC0)==0x80) {
				int value=((first & 0x3F)<<8) | bytes[offset+1];
				offset+=2;
				return value;
			}
			if((first & 0xE0)==0xC0) {
				int value=((first & 0x1F)<<24) | (bytes[offset+1]<<16) | (bytes[offset+2]<<8) | bytes[offset+3];
				offset+=4;
				return value;
			}
			return -1;
		}

		private static ushort ReadUInt16(byte[] bytes,int offset) {
			return (ushort)(bytes[offset] | (bytes[offset+1]<<8));
		}

		private static uint ReadUInt32(byte[] bytes,int offset) {
			return (uint)(bytes[offset] | (bytes[offset+1]<<8) | (bytes[offset+2]<<16) | (bytes[offset+3]<<24));
		}

		///<summary>The #~ stream's table header (II.24.2.6) and the row sizes of the 45 tables an assembly can have (II.22), so any row of any
		///table can be found.</summary>
		private sealed class MetadataTables {
			//The tables of each coded index (II.24.2.6), in tag order; -1 for an unused tag.
			internal static readonly int[] ArrayTypeDefOrRef=new[] { 0x02,0x01,0x1B };
			internal static readonly int[] ArrayHasConstant=new[] { 0x04,0x08,0x17 };
			internal static readonly int[] ArrayHasCustomAttribute=new[] { 0x06,0x04,0x01,0x02,0x08,0x09,0x0A,0x00,0x0E,0x17,0x14,0x11,0x1A,0x1B,0x20,
				0x23,0x26,0x27,0x28,0x2A,0x2C,0x2B };
			internal static readonly int[] ArrayHasFieldMarshal=new[] { 0x04,0x08 };
			internal static readonly int[] ArrayHasDeclSecurity=new[] { 0x02,0x06,0x20 };
			internal static readonly int[] ArrayMemberRefParent=new[] { 0x02,0x01,0x1A,0x06,0x1B };
			internal static readonly int[] ArrayHasSemantics=new[] { 0x14,0x17 };
			internal static readonly int[] ArrayMethodDefOrRef=new[] { 0x06,0x0A };
			internal static readonly int[] ArrayMemberForwarded=new[] { 0x04,0x06 };
			internal static readonly int[] ArrayImplementation=new[] { 0x26,0x23,0x27 };
			internal static readonly int[] ArrayCustomAttributeType=new[] { -1,-1,0x06,0x0A,-1 };
			internal static readonly int[] ArrayResolutionScope=new[] { 0x00,0x1A,0x23,0x01 };
			internal static readonly int[] ArrayTypeOrMethodDef=new[] { 0x02,0x06 };

			internal readonly byte[] Bytes;
			internal string Error="";
			internal int SizeStringIndex;
			internal int SizeGuidIndex;
			internal int SizeBlobIndex;
			private readonly int _offsetStrings;
			private readonly int _sizeStrings;
			private readonly int _offsetBlob;
			private readonly int _sizeBlob;
			private readonly int[] _arrayRowCounts=new int[64];
			private readonly int[] _arrayRowSizes=new int[TABLE_COUNT];
			private readonly int[] _arrayTableOffsets=new int[TABLE_COUNT];

			internal MetadataTables(byte[] bytes,int offsetTables,int offsetStrings,int sizeStrings,int offsetBlob,int sizeBlob) {
				Bytes=bytes;
				_offsetStrings=offsetStrings;
				_sizeStrings=sizeStrings;
				_offsetBlob=offsetBlob;
				_sizeBlob=sizeBlob;
				byte heapSizes=bytes[offsetTables+6];
				SizeStringIndex=(heapSizes & 0x01)!=0 ? 4 : 2;
				SizeGuidIndex=(heapSizes & 0x02)!=0 ? 4 : 2;
				SizeBlobIndex=(heapSizes & 0x04)!=0 ? 4 : 2;
				ulong valid=(ulong)ReadUInt32(bytes,offsetTables+8) | ((ulong)ReadUInt32(bytes,offsetTables+12)<<32);
				int offset=offsetTables+24;
				for(int table=0;table<64;table++) {
					if((valid & (1UL<<table))==0) {
						continue;
					}
					if(table>=TABLE_COUNT) {
						Error="The metadata has table 0x"+table.ToString("X2")+", which an assembly does not have.";
						return;
					}
					_arrayRowCounts[table]=(int)ReadUInt32(bytes,offset);
					offset+=4;
				}
				if((heapSizes & 0x40)!=0) {//extra data after the row counts (an edit-and-continue delta)
					offset+=4;
				}
				for(int table=0;table<TABLE_COUNT;table++) {
					_arrayRowSizes[table]=GetRowSize(table);
				}
				for(int table=0;table<TABLE_COUNT;table++) {
					_arrayTableOffsets[table]=offset;
					offset+=_arrayRowCounts[table]*_arrayRowSizes[table];
				}
				if(offset>bytes.Length) {
					Error="The metadata tables run past the end of the file.";
				}
			}

			internal int GetRowCount(int table) {
				return _arrayRowCounts[table];
			}

			///<summary>The file offset of row (1-based) of table.</summary>
			internal int GetRowOffset(int table,int row) {
				return _arrayTableOffsets[table]+(row-1)*_arrayRowSizes[table];
			}

			internal uint ReadIndex(int offset,int size) {
				return size==2 ? ReadUInt16(Bytes,offset) : ReadUInt32(Bytes,offset);
			}

			///<summary>The string at index of the #Strings heap (UTF-8, null-terminated).</summary>
			internal string ReadString(uint index) {
				if(index>=_sizeStrings) {
					return "";
				}
				int offsetStart=_offsetStrings+(int)index;
				int offsetEnd=offsetStart;
				while(offsetEnd<Bytes.Length && offsetEnd<_offsetStrings+_sizeStrings && Bytes[offsetEnd]!=0) {
					offsetEnd++;
				}
				return Encoding.UTF8.GetString(Bytes,offsetStart,offsetEnd-offsetStart);
			}

			///<summary>The blob at index of the #Blob heap, or null.</summary>
			internal byte[] ReadBlob(uint index) {
				if(_offsetBlob<0 || index>=_sizeBlob) {
					return null;
				}
				int offset=_offsetBlob+(int)index;
				int length=ReadCompressedUInt(Bytes,ref offset);
				if(length<0 || offset+length>Bytes.Length) {
					return null;
				}
				byte[] blob=new byte[length];
				Array.Copy(Bytes,offset,blob,0,length);
				return blob;
			}

			///<summary>2 bytes when every table of the coded index has fewer rows than 2^(16-bitsTag), otherwise 4 (II.24.2.6).</summary>
			internal int GetCodedIndexSize(int bitsTag,int[] arrayTables) {
				int rowsMax=0;
				foreach(int table in arrayTables) {
					if(table>=0) {
						rowsMax=Math.Max(rowsMax,_arrayRowCounts[table]);
					}
				}
				return rowsMax<(1<<(16-bitsTag)) ? 2 : 4;
			}

			private int GetTableIndexSize(int table) {
				return _arrayRowCounts[table]<0x10000 ? 2 : 4;
			}

			///<summary>The size of one row of table, from its columns (II.22).</summary>
			private int GetRowSize(int table) {
				int sizeString=SizeStringIndex;
				int sizeGuid=SizeGuidIndex;
				int sizeBlob=SizeBlobIndex;
				switch(table) {
					case 0x00://Module: Generation, Name, Mvid, EncId, EncBaseId
						return 2+sizeString+3*sizeGuid;
					case 0x01://TypeRef: ResolutionScope, TypeName, TypeNamespace
						return GetCodedIndexSize(2,ArrayResolutionScope)+2*sizeString;
					case 0x02://TypeDef: Flags, TypeName, TypeNamespace, Extends, FieldList, MethodList
						return 4+2*sizeString+GetCodedIndexSize(2,ArrayTypeDefOrRef)+GetTableIndexSize(0x04)+GetTableIndexSize(0x06);
					case 0x03://FieldPtr
						return GetTableIndexSize(0x04);
					case 0x04://Field: Flags, Name, Signature
						return 2+sizeString+sizeBlob;
					case 0x05://MethodPtr
						return GetTableIndexSize(0x06);
					case 0x06://MethodDef: RVA, ImplFlags, Flags, Name, Signature, ParamList
						return 4+2+2+sizeString+sizeBlob+GetTableIndexSize(0x08);
					case 0x07://ParamPtr
						return GetTableIndexSize(0x08);
					case 0x08://Param: Flags, Sequence, Name
						return 2+2+sizeString;
					case 0x09://InterfaceImpl: Class, Interface
						return GetTableIndexSize(0x02)+GetCodedIndexSize(2,ArrayTypeDefOrRef);
					case 0x0A://MemberRef: Class, Name, Signature
						return GetCodedIndexSize(3,ArrayMemberRefParent)+sizeString+sizeBlob;
					case 0x0B://Constant: Type and padding, Parent, Value
						return 2+GetCodedIndexSize(2,ArrayHasConstant)+sizeBlob;
					case 0x0C://CustomAttribute: Parent, Type, Value
						return GetCodedIndexSize(5,ArrayHasCustomAttribute)+GetCodedIndexSize(3,ArrayCustomAttributeType)+sizeBlob;
					case 0x0D://FieldMarshal: Parent, NativeType
						return GetCodedIndexSize(1,ArrayHasFieldMarshal)+sizeBlob;
					case 0x0E://DeclSecurity: Action, Parent, PermissionSet
						return 2+GetCodedIndexSize(2,ArrayHasDeclSecurity)+sizeBlob;
					case 0x0F://ClassLayout: PackingSize, ClassSize, Parent
						return 2+4+GetTableIndexSize(0x02);
					case 0x10://FieldLayout: Offset, Field
						return 4+GetTableIndexSize(0x04);
					case 0x11://StandAloneSig: Signature
						return sizeBlob;
					case 0x12://EventMap: Parent, EventList
						return GetTableIndexSize(0x02)+GetTableIndexSize(0x14);
					case 0x13://EventPtr
						return GetTableIndexSize(0x14);
					case 0x14://Event: EventFlags, Name, EventType
						return 2+sizeString+GetCodedIndexSize(2,ArrayTypeDefOrRef);
					case 0x15://PropertyMap: Parent, PropertyList
						return GetTableIndexSize(0x02)+GetTableIndexSize(0x17);
					case 0x16://PropertyPtr
						return GetTableIndexSize(0x17);
					case 0x17://Property: Flags, Name, Type
						return 2+sizeString+sizeBlob;
					case 0x18://MethodSemantics: Semantics, Method, Association
						return 2+GetTableIndexSize(0x06)+GetCodedIndexSize(1,ArrayHasSemantics);
					case 0x19://MethodImpl: Class, MethodBody, MethodDeclaration
						return GetTableIndexSize(0x02)+2*GetCodedIndexSize(1,ArrayMethodDefOrRef);
					case 0x1A://ModuleRef: Name
						return sizeString;
					case 0x1B://TypeSpec: Signature
						return sizeBlob;
					case 0x1C://ImplMap: MappingFlags, MemberForwarded, ImportName, ImportScope
						return 2+GetCodedIndexSize(1,ArrayMemberForwarded)+sizeString+GetTableIndexSize(0x1A);
					case 0x1D://FieldRVA: RVA, Field
						return 4+GetTableIndexSize(0x04);
					case 0x1E://EncLog: Token, FuncCode
						return 8;
					case 0x1F://EncMap: Token
						return 4;
					case 0x20://Assembly: HashAlgId, Major, Minor, Build, Revision, Flags, PublicKey, Name, Culture
						return 4+8+4+sizeBlob+2*sizeString;
					case 0x21://AssemblyProcessor
						return 4;
					case 0x22://AssemblyOS
						return 12;
					case 0x23://AssemblyRef: Major, Minor, Build, Revision, Flags, PublicKeyOrToken, Name, Culture, HashValue
						return 8+4+sizeBlob+2*sizeString+sizeBlob;
					case 0x24://AssemblyRefProcessor: Processor, AssemblyRef
						return 4+GetTableIndexSize(0x23);
					case 0x25://AssemblyRefOS: OSPlatformId, OSMajorVersion, OSMinorVersion, AssemblyRef
						return 12+GetTableIndexSize(0x23);
					case 0x26://File: Flags, Name, HashValue
						return 4+sizeString+sizeBlob;
					case 0x27://ExportedType: Flags, TypeDefId, TypeName, TypeNamespace, Implementation
						return 4+4+2*sizeString+GetCodedIndexSize(2,ArrayImplementation);
					case 0x28://ManifestResource: Offset, Flags, Name, Implementation
						return 4+4+sizeString+GetCodedIndexSize(2,ArrayImplementation);
					case 0x29://NestedClass: NestedClass, EnclosingClass
						return 2*GetTableIndexSize(0x02);
					case 0x2A://GenericParam: Number, Flags, Owner, Name
						return 2+2+GetCodedIndexSize(1,ArrayTypeOrMethodDef)+sizeString;
					case 0x2B://MethodSpec: Method, Instantiation
						return GetCodedIndexSize(1,ArrayMethodDefOrRef)+sizeBlob;
					case 0x2C://GenericParamConstraint: Owner, Constraint
						return GetTableIndexSize(0x2A)+GetCodedIndexSize(2,ArrayTypeDefOrRef);
					default:
						return 0;
				}
			}
		}
	}
}
