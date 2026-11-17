// The Mediadent / CADI module's command and its arguments (N64-A3 of the native 64-bit integration survey, docs/port-notes/N64-A3.md).
// Compiled into the helper and linked into WpfControlsOD (Bridges\CADI.cs, Bridges\CADIInProcess.cs), so the program and the helper name the
// command alike and send CADI the same values.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Globalization;
using CodeBase;

namespace OpenPmsDentalHelper32.Cadi {
	internal static class CadiCommands {
		public const string MODULE="Cadi";
		///<summary>Args: none, to open CADI without a patient (CADI.SendData with no patient), or the patient's five values (CadiPatient, in its
		///order, EncodePatient). Results: none. Opens CADI, registers the module's window for CADI's exit notice, and sends the patient, with the
		///calls CADI.cs made before N64-A3.</summary>
		public const string SEND_DATA="SendData";

		///<summary>The arguments of SEND_DATA for cadiPatient (null: none).</summary>
		public static string[] EncodePatient(CadiPatient cadiPatient) {
			if(cadiPatient==null) {
				return new string[0];
			}
			return new[] {
				cadiPatient.Name,
				//G17 gives back the same double (TDATE days), so CADI gets the value the program computed.
				cadiPatient.DateOfBirth.ToStringNetFx("G17",CultureInfo.InvariantCulture),
				cadiPatient.Age.ToString(CultureInfo.InvariantCulture),
				cadiPatient.Sex,
				cadiPatient.PicLibPath,
			};
		}

		///<summary>The patient of SEND_DATA's arguments, or null for none. Throws ArgumentException for any other count or a value that does not
		///parse.</summary>
		public static CadiPatient DecodePatient(string[] args) {
			if(args==null || args.Length==0) {
				return null;
			}
			if(args.Length!=5) {
				throw new ArgumentException(SEND_DATA+" takes no argument or the patient's name, birth date, age, sex and image folder.");
			}
			CadiPatient cadiPatient=new CadiPatient();
			cadiPatient.Name=args[0];
			if(!double.TryParse(args[1],NumberStyles.Float,CultureInfo.InvariantCulture,out cadiPatient.DateOfBirth)) {
				throw new ArgumentException(SEND_DATA+": the birth date is not a number.");
			}
			if(!int.TryParse(args[2],NumberStyles.Integer,CultureInfo.InvariantCulture,out cadiPatient.Age)) {
				throw new ArgumentException(SEND_DATA+": the age is not a number.");
			}
			cadiPatient.Sex=args[3];
			cadiPatient.PicLibPath=args[4];
			return cadiPatient;
		}
	}

	///<summary>The values CADI.cs sends CADI for one patient, with the types of its calls before N64-A3 (each crosses IDispatch as that type).
	///</summary>
	internal sealed class CadiPatient {
		///<summary>OleSetPatientName: "LName, FName".</summary>
		public string Name;
		///<summary>OleSetPatientDateOfBirth: the birth date as a COM TDATE, the days since 1899-12-30 (a double).</summary>
		public double DateOfBirth;
		///<summary>OleSetPatientBiologicalAge: the patient's age (an int).</summary>
		public int Age;
		///<summary>OleSetPatientSex: "F" or "M".</summary>
		public string Sex;
		///<summary>OleLoadPicLib: the image folder combined with the PatNum or ChartNumber.</summary>
		public string PicLibPath;
	}
}
