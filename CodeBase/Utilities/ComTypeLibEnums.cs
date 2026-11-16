// The values of named enum constants, read from a COM server's own type library (N64-B6 of the native 64-bit integration survey,
// docs/port-notes/N64-B6.md). A late-bound caller (QuickBooks Desktop's 64-bit QBFC, called through IDispatch without an interop) must pass and
// compare the server's enum values, which can differ between the server's versions (QBFC's ENResponseType numbers its response types in
// order, and each QBFC version adds some), so they are looked up by name in the type library the object itself reports
// (IDispatch.GetTypeInfo, ITypeInfo.GetContainingTypeLib), never taken from another version's interop. Read only: nothing is registered,
// created or changed.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ComTypes=System.Runtime.InteropServices.ComTypes;

namespace CodeBase {
	///<summary>Reads enum constants by name from a type library (ComTypes.ITypeLib).</summary>
	public static class ComTypeLibEnums {
		///<summary>IDispatch, for its GetTypeInfo only (the other methods are declared so the vtable order is right, and never called).</summary>
		[ComImport]
		[Guid("00020400-0000-0000-C000-000000000046")]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		private interface IDispatchTypeInfo {
			[PreserveSig]
			int GetTypeInfoCount(out uint countTypeInfo);
			[PreserveSig]
			int GetTypeInfo(uint indexTypeInfo,int lcid,out ComTypes.ITypeInfo typeInfo);
		}

		///<summary>The values of arrayNames, each an enum constant of the type library that comObject's own type information belongs to (its
		///IDispatch.GetTypeInfo, then ITypeInfo.GetContainingTypeLib). Throws when the object has no type information, or a name is not an enum
		///constant there.</summary>
		public static Dictionary<string,int> ReadFromObject(object comObject,IList<string> arrayNames) {
			if(comObject==null || !Marshal.IsComObject(comObject)) {
				throw new ArgumentException("Not a COM object: its type library cannot be read.");
			}
			IDispatchTypeInfo dispatch=(IDispatchTypeInfo)comObject;
			int hResult=dispatch.GetTypeInfo(0,0,out ComTypes.ITypeInfo typeInfo);
			if(hResult!=0 || typeInfo==null) {
				throw new COMException("The COM object gave no type information.",hResult==0 ? unchecked((int)0x80004005) : hResult);
			}
			typeInfo.GetContainingTypeLib(out ComTypes.ITypeLib typeLib,out int indexTypeInfo);
			if(typeLib==null) {
				throw new COMException("The COM object's type information has no type library.",unchecked((int)0x80004005));
			}
			return Read(typeLib,arrayNames);
		}

		///<summary>The values of arrayNames, each the constant of that name in any enum of typeLib (MIDL makes enum constant names unique in a
		///library). Throws when a name is not found there, or its value is not an integer.</summary>
		public static Dictionary<string,int> Read(ComTypes.ITypeLib typeLib,IList<string> arrayNames) {
			Dictionary<string,int> dictValues=new Dictionary<string,int>(StringComparer.Ordinal);
			HashSet<string> hashSetWanted=new HashSet<string>(arrayNames,StringComparer.Ordinal);
			int countTypes=typeLib.GetTypeInfoCount();
			for(int i=0;i<countTypes && dictValues.Count<hashSetWanted.Count;i++) {
				typeLib.GetTypeInfoType(i,out ComTypes.TYPEKIND typeKind);
				if(typeKind!=ComTypes.TYPEKIND.TKIND_ENUM) {
					continue;
				}
				typeLib.GetTypeInfo(i,out ComTypes.ITypeInfo typeInfo);
				ReadEnum(typeInfo,hashSetWanted,dictValues);
			}
			foreach(string name in arrayNames) {
				if(!dictValues.ContainsKey(name)) {
					throw new KeyNotFoundException("The type library has no enum constant "+name+".");
				}
			}
			return dictValues;
		}

		private static void ReadEnum(ComTypes.ITypeInfo typeInfo,HashSet<string> hashSetWanted,Dictionary<string,int> dictValues) {
			typeInfo.GetTypeAttr(out IntPtr pTypeAttr);
			try {
				ComTypes.TYPEATTR typeAttr=(ComTypes.TYPEATTR)Marshal.PtrToStructure(pTypeAttr,typeof(ComTypes.TYPEATTR));
				for(int j=0;j<typeAttr.cVars;j++) {
					typeInfo.GetVarDesc(j,out IntPtr pVarDesc);
					try {
						ComTypes.VARDESC varDesc=(ComTypes.VARDESC)Marshal.PtrToStructure(pVarDesc,typeof(ComTypes.VARDESC));
						if(varDesc.varkind!=ComTypes.VARKIND.VAR_CONST) {
							continue;
						}
						string[] arrayNames=new string[1];
						typeInfo.GetNames(varDesc.memid,arrayNames,1,out int countNames);
						if(countNames<1 || !hashSetWanted.Contains(arrayNames[0]) || dictValues.ContainsKey(arrayNames[0])) {
							continue;
						}
						object value=Marshal.GetObjectForNativeVariant(varDesc.desc.lpvarValue);
						dictValues[arrayNames[0]]=Convert.ToInt32(value,System.Globalization.CultureInfo.InvariantCulture);
					}
					finally {
						typeInfo.ReleaseVarDesc(pVarDesc);
					}
				}
			}
			finally {
				typeInfo.ReleaseTypeAttr(pTypeAttr);
			}
		}

		[DllImport("oleaut32.dll",CharSet=CharSet.Unicode,PreserveSig=false)]
		private static extern void LoadTypeLibEx(string fileName,int regKind,out ComTypes.ITypeLib typeLib);

		///<summary>Loads the type library of the file at path without registering it (REGKIND_NONE), for reading only. For tests (a Windows type
		///library such as stdole2.tlb).</summary>
		public static ComTypes.ITypeLib LoadWithoutRegistering(string path) {
			const int REGKIND_NONE=2;
			LoadTypeLibEx(path,REGKIND_NONE,out ComTypes.ITypeLib typeLib);
			return typeLib;
		}
	}
}
