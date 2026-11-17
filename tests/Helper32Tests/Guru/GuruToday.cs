// The Guru code of WpfControlsOD/Bridges/Guru.cs before S3a-14 of the .NET 10 move that builds MedVisorInterface.dll's patient structure:
// MVPatient, Tidy and ToCPlusPlusStructure, kept here to compare the helper's structure with byte for byte (GuruModuleTests). The text is the
// old one; only the class is renamed (MVPatientToday) and the vfptr comment block is left out.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Runtime.InteropServices;

namespace Helper32Tests.Guru {
	internal static class GuruToday {
		///<summary>This class is defined with a structural layout, so that it can be properly marshaled to a C-code structure.</summary>
		[StructLayout(LayoutKind.Sequential,Pack=8)]
		public class MVPatientToday {
			public char[] LastName=new char[65];
			public char[] MiddleName=new char[65];
			public char[] FirstName=new char[65];
			public char[] Prefix=new char[65];
			public char[] Suffix=new char[65];
			public char[] Sex=new char[2];
			public char[] BirthDate=new char[9];
			public char[] ID=new char[65];
			public char[] Directory=new char[260];
		}

		public static IntPtr ToCPlusPlusStructure(MVPatientToday mvPatient) {
			int size=sizeof(char)*(65+65+65+65+65+2+9+65+260);//The size of the unmanaged structure.
			IntPtr retVal=Marshal.AllocHGlobal(size);//The location where the unmanaged structure will rest.
			unsafe {//unmanaged code section.
				byte* copyLoc=(byte*)retVal.ToPointer();
				//LastName
				size=65;
				Marshal.Copy(mvPatient.LastName,0,(IntPtr)copyLoc,size);
				copyLoc+=size;
				//MiddleName
				size=65;
				Marshal.Copy(mvPatient.MiddleName,0,(IntPtr)copyLoc,size);
				copyLoc+=size;
				//FirstName
				size=65;
				Marshal.Copy(mvPatient.FirstName,0,(IntPtr)copyLoc,size);
				copyLoc+=size;
				//Prefix
				size=65;
				Marshal.Copy(mvPatient.Prefix,0,(IntPtr)copyLoc,size);
				copyLoc+=size;
				//Suffix
				size=65;
				Marshal.Copy(mvPatient.Suffix,0,(IntPtr)copyLoc,size);
				copyLoc+=size;
				//Sex
				size=2;
				Marshal.Copy(mvPatient.Sex,0,(IntPtr)copyLoc,size);
				copyLoc+=size;
				//BirthDate
				size=9;
				Marshal.Copy(mvPatient.BirthDate,0,(IntPtr)copyLoc,size);
				copyLoc+=size;
				//ID
				size=65;
				Marshal.Copy(mvPatient.ID,0,(IntPtr)copyLoc,size);
				copyLoc+=size;
				//Directory
				size=260;
				Marshal.Copy(mvPatient.Directory,0,(IntPtr)copyLoc,size);
				copyLoc+=size;
			}
			return retVal;
		}

		///<summary>Destroys a c++ structure created by ToCPlusPlusStructure().</summary>
		public static void DestroyCPlusPlusStruct(IntPtr cPlusPlusStruct) {
			Marshal.FreeHGlobal(cPlusPlusStruct);//Free the structure itself.
		}

		///<summary>Takes the input and modifies the length to the specified length. Appends a null, and returns the result as a character array.</summary>
		public static char[] Tidy(string input,int length) {
			string retVal=input;
			if(input.Length > length) {
				retVal=input.Substring(0,length);
			}
			if(input.Length < length) {
				retVal=input.PadRight(length,'\0');
			}
			retVal+='\0';
			return retVal.ToCharArray();
		}
	}
}
