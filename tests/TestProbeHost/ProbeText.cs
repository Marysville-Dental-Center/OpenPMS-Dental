// The line format between a test and its child process (TestProbeHost): every field is UTF-8 text in base64, so a line holds only ASCII and no
// field can break a line; fields are separated by one space, and null is "~".
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestProbeHost {
	public static class ProbeText {
		///<summary>The child's first line once it has loaded the handler.</summary>
		public const string READY="ready";
		public const string REPLY_OK="ok";
		public const string REPLY_ERROR="error";
		private const string NULL_FIELD="~";

		///<summary>The fields as one line.</summary>
		public static string Join(IEnumerable<string> listFields) {
			return string.Join(" ",listFields.Select(x => EncodeField(x)));
		}

		///<summary>The fields of a line made by Join.</summary>
		public static string[] Split(string line) {
			return line.Split(' ').Select(x => DecodeField(x)).ToArray();
		}

		///<summary>A list as one field (for an argument or a result that is a list of strings).</summary>
		public static string JoinList(IEnumerable<string> listValues) {
			if(listValues==null) {
				return null;
			}
			List<string> list=listValues.ToList();
			//The count first, so an empty list and a list of one empty string differ.
			return Join(new[] { list.Count.ToString() }.Concat(list));
		}

		public static string[] SplitList(string text) {
			if(text==null) {
				return null;
			}
			string[] arrayFields=Split(text);
			int count=int.Parse(arrayFields[0]);
			if(arrayFields.Length!=count+1) {
				throw new FormatException("A list of "+count+" values has "+(arrayFields.Length-1)+".");
			}
			return arrayFields.Skip(1).ToArray();
		}

		///<summary>A reply line: ok and the result, or error and the exception's text.</summary>
		public static string FormatReply(bool isOk,string text) {
			return (isOk ? REPLY_OK : REPLY_ERROR)+" "+EncodeField(text);
		}

		private static string EncodeField(string value) {
			if(value==null) {
				return NULL_FIELD;
			}
			return Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
		}

		private static string DecodeField(string field) {
			if(field==NULL_FIELD) {
				return null;
			}
			return Encoding.UTF8.GetString(Convert.FromBase64String(field));
		}
	}
}
