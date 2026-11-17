// The 32-bit helper's Guru module (S3a-14 of the .NET 10 move, docs/port-notes/S3a-14.md, section 4): Guru's native MedVisorInterface.dll,
// loaded and called here instead of in the program's process. MVStart and MVSendPatient are called as WpfControlsOD\Bridges\Guru.cs called them
// before S3a-14, and the structure MVSendPatient reads is built by the same ToCPlusPlusStructure, byte for byte.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace OpenPmsDentalHelper32.Guru {
	///<summary>MedVisorInterface.dll is loaded by Windows on the first MVStart (a DllImport loads its library on the first call, never when the
	///module is built), from the helper's folder, the system folders or the PATH; without it the call throws DllNotFoundException, which the
	///program shows as before ("Could not find MedVisorInterface.dll. Verify that Guru is installed.").</summary>
	internal sealed class GuruModule:IHelper32Module {
		private readonly Func<int> _funcMVStart;
		private readonly Func<IntPtr,int> _funcMVSendPatient;

		[DllImport("MedVisorInterface.dll")]
		private static extern int MVStart();

		//[DllImport("MedVisorInterface.dll")]
		//private static extern bool MVGetRunningState();

		[DllImport("MedVisorInterface.dll")]
		private static extern int MVSendPatient(IntPtr mvPatient);

		///<summary>The module the helper runs: MedVisorInterface.dll's own functions.</summary>
		public GuruModule() : this(() => MVStart(),x => MVSendPatient(x)) {
		}

		///<summary>Tests: fakes of MVStart and MVSendPatient (the fake reads the structure before it is freed).</summary>
		internal GuruModule(Func<int> funcMVStart,Func<IntPtr,int> funcMVSendPatient) {
			_funcMVStart=funcMVStart;
			_funcMVSendPatient=funcMVSendPatient;
		}

		public string Name {
			get {
				return GuruCommands.MODULE;
			}
		}

		public Helper32Reply Handle(Helper32Request request) {
			switch(request.Command) {
				case GuruCommands.MV_START:
					return Helper32Reply.Ok(new[] { _funcMVStart().ToString(CultureInfo.InvariantCulture) },null);
				case GuruCommands.MV_SEND_PATIENT:
					MVPatient mvPatient=GuruCommands.DecodePatient(request.Args,request.Data);
					return Helper32Reply.Ok(new[] { SendPatient(mvPatient).ToString(CultureInfo.InvariantCulture) },null);
				default:
					throw new ArgumentException("The Guru module has no command "+request.Command+".");
			}
		}

		private int SendPatient(MVPatient mvPatient) {
			IntPtr patStruct=ToCPlusPlusStructure(mvPatient);
			int retVal=_funcMVSendPatient(patStruct);
			DestroyCPlusPlusStruct(patStruct);
			return retVal;
		}

		///<summary>CRITICAL FUNCTION: Creates a converted copy of this object into an unmanaged c++ structure (including the virutal function table) which is compatible with MedVisorInterface.dll module and returns the address to the newly created structure. The newly created structure will point to the same image data as the managed/local structure does. Finally, the structure which is returned (pointed to) must be freed using DesctroyCPlusPlusStructure(), or else there will be a memory leak. This function was designed to work on 32-bit and 64-bit operating systems.
		///Moved from Guru.cs unchanged (S3a-14). Each Marshal.Copy copies size chars (2*size bytes) and the pointer moves on size bytes, so each
		///field's second half lies under the next field's start; the first 921 of the 1320 bytes are written, the rest is not, as before.</summary>
		internal static IntPtr ToCPlusPlusStructure(MVPatient mvPatient) {
			int size=sizeof(char)*(65+65+65+65+65+2+9+65+260);//The size of the unmanaged structure.
			IntPtr retVal=Marshal.AllocHGlobal(size);//The location where the unmanaged structure will rest.
			unsafe {//unmanaged code section.
				byte* copyLoc=(byte*)retVal.ToPointer();

				////__vfptr (virtual function table pointer).
				////The layout of the virtual function table as expected by MedVisorInterface.dll was discovered by running the
				////sen4 example project (provided by Suni) in debug mode and viewing one of the CImageData object at runtime.
				//IntPtr vfptr=Marshal.AllocHGlobal(3*Marshal.SizeOf(typeof(IntPtr)));
				//byte* vtp=(byte*)vfptr.ToPointer();
				////The pointer to ~CImageData() (will not be called by any native function calls to SuniUSB.dll,
				////so no need to define it). Always set to zero just to fill the space.
				//Marshal.Copy(new IntPtr[] { IntPtr.Zero },0,(IntPtr)vtp,1);
				//vtp+=Marshal.SizeOf(typeof(IntPtr));
				////PutPixel() pointer
				//IntPtr putPixelPtr=Marshal.GetFunctionPointerForDelegate(ppd);
				//Marshal.Copy(new IntPtr[] { putPixelPtr },0,(IntPtr)vtp,1);
				//vtp+=Marshal.SizeOf(putPixelPtr);
				////GetPixel() pointer
				//IntPtr getPixelPtr=Marshal.GetFunctionPointerForDelegate(gpd);
				//Marshal.Copy(new IntPtr[] { getPixelPtr },0,(IntPtr)vtp,1);
				//vtp+=Marshal.SizeOf(getPixelPtr);
				////End of virtual function table definition.
				////Virtual table
				//Marshal.Copy(new IntPtr[] { vfptr },0,(IntPtr)copyLoc,1);
				//copyLoc+=Marshal.SizeOf(vfptr);

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
		internal static void DestroyCPlusPlusStruct(IntPtr cPlusPlusStruct) {
			Marshal.FreeHGlobal(cPlusPlusStruct);//Free the structure itself.
		}
	}
}
