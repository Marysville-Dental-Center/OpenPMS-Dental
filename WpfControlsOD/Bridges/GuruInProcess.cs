// Guru's MedVisorInterface.dll in the program's own process, the native-first path (S3a-14n of the .NET 10 move, docs/port-notes/S3a-14.md,
// section "S3a-14n: native first"): Guru.cs's MVStart, MVSendPatient, ToCPlusPlusStructure and DestroyCPlusPlusStruct of before S3a-14, moved
// here unchanged (only ToCPlusPlusStructure's commented-out virtual table lines are left out). NativeFirstChooser.Guru calls them here when the MedVisorInterface.dll the loader would find is of the program's machine type
// (no 64-bit one is known, so after stage 3b the 32-bit helper's Guru module, Guru.cs's other path, runs them).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Runtime.InteropServices;
using CodeBase;
using OpenPmsDentalHelper32.Guru;

namespace OpenPmsDental.Bridges {
	///<summary>MedVisorInterface.dll in the program's process (Guru.cs's path before S3a-14).</summary>
	internal static class GuruInProcess {
		///<summary>Guru's DLL, as the DllImports below name it.</summary>
		public const string DLL_FILE_NAME="MedVisorInterface.dll";

		///<summary>True when NativeFirstChooser.Guru chose this process: the MedVisorInterface.dll the loader finds first is of this process's
		///machine type (in a 32-bit program always, as before S3a-14). When none is found, Guru's known build (x86) is assumed, so a missing DLL is
		///reported as before ("Could not find MedVisorInterface.dll") by whichever path would have loaded it.</summary>
		public static bool IsChosen() {
			return NativeFirstChooser.Guru.ChooseForNativeDll(DLL_FILE_NAME,PeImageInfo.IMAGE_FILE_MACHINE_I386)==VendorPath.InProcess;
		}

		[DllImport("MedVisorInterface.dll")]
		public static extern int MVStart();

		//[DllImport("MedVisorInterface.dll")]
		//private static extern bool MVGetRunningState();

		[DllImport("MedVisorInterface.dll")]
		private static extern int MVSendPatient(IntPtr mvPatient);
		public static int MVSendPatient(MVPatient mvPatient) {
			IntPtr patStruct=ToCPlusPlusStructure(mvPatient);
			int retVal=MVSendPatient(patStruct);
			DestroyCPlusPlusStruct(patStruct);
			return retVal;
		}

		///<summary>CRITICAL FUNCTION: Creates a converted copy of this object into an unmanaged c++ structure (including the virutal function table) which is compatible with MedVisorInterface.dll module and returns the address to the newly created structure. The newly created structure will point to the same image data as the managed/local structure does. Finally, the structure which is returned (pointed to) must be freed using DesctroyCPlusPlusStructure(), or else there will be a memory leak. This function was designed to work on 32-bit and 64-bit operating systems.</summary>
		internal static IntPtr ToCPlusPlusStructure(MVPatient mvPatient) {
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
	}
}
