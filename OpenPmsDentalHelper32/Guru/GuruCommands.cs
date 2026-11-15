// The Guru module's commands and the patient it sends (S3a-14 of the .NET 10 move, docs/port-notes/S3a-14.md, section 4). Compiled into the
// helper and linked into WpfControlsOD (Bridges\Guru.cs), so the program and the helper name them alike and cross the patient alike.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;

namespace OpenPmsDentalHelper32.Guru {
	internal static class GuruCommands {
		public const string MODULE="Guru";
		///<summary>Args: none. Results: MedVisorInterface.MVStart's return value (invariant). A missing MedVisorInterface.dll comes back as the
		///helper's System.DllNotFoundException.</summary>
		public const string MV_START="MVStart";
		///<summary>Args: the length of each of the patient's nine character arrays (MVPatient's field order). Data: their characters, UTF-16
		///little-endian, one after another (EncodePatient). Results: MedVisorInterface.MVSendPatient's return value (invariant). The helper builds
		///the structure that MedVisorInterface.dll reads from them (ToCPlusPlusStructure).</summary>
		public const string MV_SEND_PATIENT="MVSendPatient";

		///<summary>The patient's arrays as MV_SEND_PATIENT's arguments and data. Every char crosses as its two bytes, so a name the program cut
		///inside a surrogate pair (Tidy) reaches MedVisorInterface.dll as it did, which a text argument (UTF-8) would not keep.</summary>
		public static string[] EncodePatient(MVPatient mvPatient,out byte[] data) {
			char[][] arrayFields=mvPatient.GetFields();
			string[] arrayLengths=new string[arrayFields.Length];
			List<byte> listBytes=new List<byte>();
			for(int i=0;i<arrayFields.Length;i++) {
				arrayLengths[i]=arrayFields[i].Length.ToString(CultureInfo.InvariantCulture);
				foreach(char c in arrayFields[i]) {
					listBytes.Add((byte)(c & 0xFF));
					listBytes.Add((byte)(c>>8));
				}
			}
			data=listBytes.ToArray();
			return arrayLengths;
		}

		///<summary>The patient EncodePatient wrote. Throws for arguments that are not nine lengths that fit the data.</summary>
		public static MVPatient DecodePatient(string[] args,byte[] data) {
			if(args==null || args.Length!=MVPatient.FIELD_COUNT || data==null) {
				throw new ArgumentException(MV_SEND_PATIENT+" takes the nine lengths of the patient's fields and their characters.");
			}
			char[][] arrayFields=new char[MVPatient.FIELD_COUNT][];
			int index=0;
			for(int i=0;i<MVPatient.FIELD_COUNT;i++) {
				int length=int.Parse(args[i],NumberStyles.None,CultureInfo.InvariantCulture);
				if(length>(data.Length-index)/2) {
					throw new ArgumentException("The patient's field "+i.ToString(CultureInfo.InvariantCulture)+" is longer than the data sent.");
				}
				arrayFields[i]=new char[length];
				for(int j=0;j<length;j++) {
					arrayFields[i][j]=(char)(data[index] | (data[index+1]<<8));
					index+=2;
				}
			}
			if(index!=data.Length) {
				throw new ArgumentException("The patient's data is longer than its fields.");
			}
			MVPatient mvPatient=new MVPatient();
			mvPatient.SetFields(arrayFields);
			return mvPatient;
		}
	}

	///<summary>This class is defined with a structural layout, so that it can be properly marshaled to a C-code structure. The class of
	///Guru.cs before S3a-14, moved here so the program fills it and the helper builds MedVisorInterface.dll's structure from it.</summary>
	[StructLayout(LayoutKind.Sequential,Pack=8)]
	internal class MVPatient {
		public const int FIELD_COUNT=9;
		public char[] LastName=new char[65];
		public char[] MiddleName=new char[65];
		public char[] FirstName=new char[65];
		public char[] Prefix=new char[65];
		public char[] Suffix=new char[65];
		public char[] Sex=new char[2];
		public char[] BirthDate=new char[9];
		public char[] ID=new char[65];
		public char[] Directory=new char[260];

		///<summary>The nine fields in their order.</summary>
		public char[][] GetFields() {
			return new[] { LastName,MiddleName,FirstName,Prefix,Suffix,Sex,BirthDate,ID,Directory };
		}

		public void SetFields(char[][] arrayFields) {
			LastName=arrayFields[0];
			MiddleName=arrayFields[1];
			FirstName=arrayFields[2];
			Prefix=arrayFields[3];
			Suffix=arrayFields[4];
			Sex=arrayFields[5];
			BirthDate=arrayFields[6];
			ID=arrayFields[7];
			Directory=arrayFields[8];
		}
	}
}
