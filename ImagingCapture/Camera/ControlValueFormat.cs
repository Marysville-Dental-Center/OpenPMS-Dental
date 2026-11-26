// This file is part of the open source dental practice management software derived from Open Dental.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace ImagingCapture.Camera {
	///<summary>Text forms of control values and masks, for capture.ini and the live test page. Control values are 32-bit signed
	///(the LONG of IAMCameraControl::Get and IAMVideoProcAmp::Get); hex and binary show their two's-complement bits.</summary>
	public static class ControlValueFormat {
		///<summary>"0x10", "0x0", "0xFFFFFFFF": as few hex digits as the value needs. The form capture.ini masks are written in.</summary>
		public static string ToHex(int value) {
			return "0x"+unchecked((uint)value).ToString("X",CultureInfo.InvariantCulture);
		}

		///<summary>"0x0010" for values 0 to 0xFFFF, otherwise all eight digits ("0x00012345", "0xFFFFFFFF"), so a column of values
		///lines up with ToBinary.</summary>
		public static string ToHexPadded(int value) {
			uint bits=unchecked((uint)value);
			return "0x"+bits.ToString(bits<=0xFFFF ? "X4" : "X8",CultureInfo.InvariantCulture);
		}

		///<summary>"0000 0000 0001 0000": 16 bits for values 0 to 0xFFFF, otherwise 32, most significant first, in groups of four.</summary>
		public static string ToBinary(int value) {
			uint bits=unchecked((uint)value);
			int countBits=bits<=0xFFFF ? 16 : 32;
			StringBuilder stringBuilder=new StringBuilder(countBits+countBits/4);
			for(int i=countBits-1;i>=0;i--) {
				stringBuilder.Append(((bits>>i) & 1)==1 ? '1' : '0');
				if(i>0 && i%4==0) {
					stringBuilder.Append(' ');
				}
			}
			return stringBuilder.ToString();
		}

		///<summary>The bit numbers of a mask as capture.ini's Bits holds them: "3", "0,2". "" for 0.</summary>
		public static string ToBitsText(int mask) {
			return string.Join(",",GetBits(mask).Select(x => x.ToString(CultureInfo.InvariantCulture)));
		}

		///<summary>The bit numbers set in value, lowest first (0 to 31).</summary>
		public static List<int> GetBits(int value) {
			List<int> listBits=new List<int>();
			uint bits=unchecked((uint)value);
			for(int i=0;i<32;i++) {
				if(((bits>>i) & 1)==1) {
					listBits.Add(i);
				}
			}
			return listBits;
		}

		///<summary>The bits that are on, for people: "0 1 2 4 5", or "none".</summary>
		public static string ToBitsOn(int value) {
			List<int> listBits=GetBits(value);
			return listBits.Count==0 ? "none" : string.Join(" ",listBits.Select(x => x.ToString(CultureInfo.InvariantCulture)));
		}

		///<summary>Reads Bits: bit numbers 0 to 31, separated by commas or spaces. False when empty or when a part is not a bit number.</summary>
		public static bool TryParseBits(string text,out int mask) {
			mask=0;
			string[] arrayParts=(text??"").Split(new[] { ',',' ' },StringSplitOptions.RemoveEmptyEntries);
			if(arrayParts.Length==0) {
				return false;
			}
			foreach(string part in arrayParts) {
				int bit;
				if(!int.TryParse(part,NumberStyles.None,CultureInfo.InvariantCulture,out bit) || bit>31) {
					mask=0;
					return false;
				}
				mask|=unchecked((int)(1u<<bit));
			}
			return true;
		}

		///<summary>"-5", "200": invariant decimal.</summary>
		public static string ToDecimal(int value) {
			return value.ToString(CultureInfo.InvariantCulture);
		}

		///<summary>Parses a mask or value as capture.ini holds it: hex with a 0x prefix ("0x10", "0xFFFFFFFF"; up to eight digits, bit
		///31 allowed) or decimal ("16", "-1", or up to 4294967295). Surrounding whitespace is ignored. False for anything else.</summary>
		public static bool TryParse(string text,out int value) {
			value=0;
			if(text==null) {
				return false;
			}
			string trimmed=text.Trim();
			if(trimmed.StartsWith("0x",StringComparison.OrdinalIgnoreCase)) {
				string digits=trimmed.Substring(2);
				uint bits;
				if(digits.Length==0 || digits.Length>8 || !uint.TryParse(digits,NumberStyles.AllowHexSpecifier,CultureInfo.InvariantCulture,out bits)) {
					return false;
				}
				value=unchecked((int)bits);
				return true;
			}
			if(int.TryParse(trimmed,NumberStyles.AllowLeadingSign,CultureInfo.InvariantCulture,out value)) {
				return true;
			}
			uint unsignedValue;
			if(uint.TryParse(trimmed,NumberStyles.None,CultureInfo.InvariantCulture,out unsignedValue)) {
				value=unchecked((int)unsignedValue);
				return true;
			}
			value=0;
			return false;
		}
	}
}
