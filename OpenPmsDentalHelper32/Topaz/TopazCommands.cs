// The Topaz module's commands and the values its calls carry (S3a-14 of the .NET 10 move, docs/port-notes/S3a-14.md, section 5). Compiled into
// the helper and linked into OpenPmsDentalBusiness (UI\TopazWrapper.cs, UI\TopazSignatureView.cs), so the program and the helper name them
// alike and cross the control's arguments and results alike.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Globalization;
using System.Text;
using CodeBase;

namespace OpenPmsDentalHelper32.Topaz {
	internal static class TopazCommands {
		public const string MODULE="Topaz";
		///<summary>Topaz's .NET signature control assembly, as Topaz names it.</summary>
		public const string ASSEMBLY_FILE_NAME="SigPlusNET.dll";
		///<summary>The signature control in that assembly.</summary>
		public const string CONTROL_TYPE_NAME="Topaz.SigPlusNET";
		///<summary>Args: the path of the SigPlusNET.dll the program found. Results: "" when the control type loaded, else why not (the path and the
		///reason, as TopazWrapper said it before S3a-14).</summary>
		public const string LOAD="Load";
		///<summary>Args: the path of SigPlusNET.dll. Results: the new control's id. One control per signature box of the program.</summary>
		public const string CREATE="Create";
		///<summary>Args: the control's id. Results: none. Disposes of the control.</summary>
		public const string RELEASE="Release";
		///<summary>Args: the control's id, the public method's name, then a type and a value (EncodeValue) for each argument. Results: the type and
		///the value of what the method returned ("v" for void). The control's own public methods, called by name as TopazWrapper did.</summary>
		public const string CALL="Call";
		///<summary>Args: the control's id. Results: "1" when the control's private Sig field and the field type's private SetAutoKeyData method
		///could be read (TopazWrapper.FillSignatureEncodings stops without them), else "0".</summary>
		public const string CAN_SET_AUTO_KEY_BYTES="CanSetAutoKeyBytes";
		///<summary>Args: the control's id. Data: the key data's bytes in one encoding. Results: none. Calls Sig.SetAutoKeyData(byte[]).</summary>
		public const string SET_AUTO_KEY_BYTES="SetAutoKeyBytes";
		///<summary>Args: the control's id, the width and the height in pixels. Data back: a PNG of the control drawn at that size, the picture the
		///program shows in place of the control (Control.DrawToBitmap, as SignatureBoxWrapper.GetSigImage drew the control before).</summary>
		public const string PICTURE="Picture";
		///<summary>Args: the control's id, the program's window (the handle's value, invariant), the width and height of the control, the window's
		///title and the texts of its OK, Clear and Cancel buttons. Shows the helper's signing window with the control over the program's window
		///and waits until it closes. Results: "1" for OK, "0" for Cancel (the pad is cleared). Data back: the picture, as PICTURE.</summary>
		public const string SIGN="Sign";
		///<summary>The value types of CALL.</summary>
		public const string TYPE_VOID="v";
		public const string TYPE_NULL="z";
		public const string TYPE_INT="i";
		public const string TYPE_LONG="l";
		public const string TYPE_SHORT="h";
		public const string TYPE_BOOL="b";
		public const string TYPE_DOUBLE="d";
		public const string TYPE_STRING="s";
		///<summary>A string that is not well-formed UTF-16 (a lone surrogate: note text cut in the middle of an emoji), as its UTF-16 code units in
		///hex, four digits each. The protocol's UTF-8 cannot carry such a string (S3a-14f), and Topaz's control got it as it was when it ran in the
		///program's process, so the helper's control gets it unchanged too: the same key data, the same signature check.</summary>
		public const string TYPE_STRING_UTF16="u";
		///<summary>Any other type, as its ToString (the program never reads one).</summary>
		public const string TYPE_OTHER="o";

		///<summary>value's type and its text (invariant; a double round-trips).</summary>
		public static string[] EncodeValue(object value) {
			switch(value) {
				case null:
					return new[] { TYPE_NULL,"" };
				case int intValue:
					return new[] { TYPE_INT,intValue.ToString(CultureInfo.InvariantCulture) };
				case long longValue:
					return new[] { TYPE_LONG,longValue.ToString(CultureInfo.InvariantCulture) };
				case short shortValue:
					return new[] { TYPE_SHORT,shortValue.ToString(CultureInfo.InvariantCulture) };
				case bool boolValue:
					return new[] { TYPE_BOOL,boolValue ? "1" : "0" };
				case double doubleValue:
					return new[] { TYPE_DOUBLE,doubleValue.ToStringNetFx("R",CultureInfo.InvariantCulture) };
				case string stringValue:
					if(HasLoneSurrogate(stringValue)) {
						return new[] { TYPE_STRING_UTF16,StringToUtf16Hex(stringValue) };
					}
					return new[] { TYPE_STRING,stringValue };
				default:
					if(value.GetType().IsEnum) {
						return new[] { TYPE_LONG,Convert.ToInt64(value,CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture) };
					}
					return new[] { TYPE_OTHER,Convert.ToString(value,CultureInfo.InvariantCulture) };
			}
		}

		///<summary>The value EncodeValue wrote; null for void and null.</summary>
		public static object DecodeValue(string type,string text) {
			switch(type) {
				case TYPE_VOID:
				case TYPE_NULL:
					return null;
				case TYPE_INT:
					return int.Parse(text,NumberStyles.AllowLeadingSign,CultureInfo.InvariantCulture);
				case TYPE_LONG:
					return long.Parse(text,NumberStyles.AllowLeadingSign,CultureInfo.InvariantCulture);
				case TYPE_SHORT:
					return short.Parse(text,NumberStyles.AllowLeadingSign,CultureInfo.InvariantCulture);
				case TYPE_BOOL:
					return text=="1";
				case TYPE_DOUBLE:
					return double.Parse(text,NumberStyles.Float,CultureInfo.InvariantCulture);
				case TYPE_STRING:
				case TYPE_OTHER:
					return text;
				case TYPE_STRING_UTF16:
					return StringFromUtf16Hex(text);
				default:
					throw new ArgumentException("No value type "+type+".");
			}
		}

		///<summary>True when value has a high surrogate not followed by a low one, or a low surrogate not after a high one.</summary>
		public static bool HasLoneSurrogate(string value) {
			for(int i=0;i<value.Length;i++) {
				if(char.IsHighSurrogate(value[i])) {
					if(i+1<value.Length && char.IsLowSurrogate(value[i+1])) {
						i++;//a pair
						continue;
					}
					return true;
				}
				if(char.IsLowSurrogate(value[i])) {
					return true;
				}
			}
			return false;
		}

		private static string StringToUtf16Hex(string value) {
			StringBuilder stringBuilder=new StringBuilder(value.Length*4);
			foreach(char c in value) {
				stringBuilder.Append(((int)c).ToString("X4",CultureInfo.InvariantCulture));
			}
			return stringBuilder.ToString();
		}

		private static string StringFromUtf16Hex(string text) {
			if(text.Length%4!=0) {
				throw new ArgumentException("A "+TYPE_STRING_UTF16+" value has four hex digits per character.");
			}
			char[] arrayChars=new char[text.Length/4];
			for(int i=0;i<arrayChars.Length;i++) {
				arrayChars[i]=(char)int.Parse(text.Substring(4*i,4),NumberStyles.AllowHexSpecifier,CultureInfo.InvariantCulture);
			}
			return new string(arrayChars);
		}

		public static string IntToText(int value) {
			return value.ToString(CultureInfo.InvariantCulture);
		}

		public static int IntFromText(string text) {
			return int.Parse(text,NumberStyles.AllowLeadingSign,CultureInfo.InvariantCulture);
		}

		public static string HandleToText(IntPtr handle) {
			return handle.ToInt64().ToString(CultureInfo.InvariantCulture);
		}

		public static IntPtr HandleFromText(string text) {
			return new IntPtr(long.Parse(text,NumberStyles.AllowLeadingSign,CultureInfo.InvariantCulture));
		}
	}

	///<summary>Thrown by the helper for a control id it does not know: the program's control lived in a helper that has ended since. The program
	///then creates its control again (TopazSignatureView), which it recognizes by this type's full name.</summary>
	internal sealed class TopazControlGoneException:Exception {
		public TopazControlGoneException(string message) : base(message) {
		}
	}
}
