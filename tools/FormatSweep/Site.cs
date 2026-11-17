// One place FormatSweep found, and the text edits that route it through the helpers. S3b-FMT, docs/port-notes/S3b-FMT.md.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System.Collections.Generic;

namespace FormatSweep {
	///<summary>The kinds of place the scan finds. Each is guarded (the guard test fails on one that is not routed and not on the allow list)
	///unless it says "reported".</summary>
	public static class Categories {
		///<summary>double/float/double?/float? .ToString(), .ToString(format), .ToString(provider), .ToString(format,provider): renamed to
		///ToStringNetFx.</summary>
		public const string TO_STRING="ToString";
		///<summary>Convert.ToString(double|float[,provider]): becomes value.ToStringNetFx([provider]).</summary>
		public const string CONVERT_TO_STRING="Convert.ToString";
		///<summary>A double or float in a string concatenation (+ or +=): value.ToStringNetFx().</summary>
		public const string CONCAT="Concat";
		///<summary>A double or float in an interpolated string's hole: value.AsNetFx() (the hole's alignment and format stay).</summary>
		public const string INTERPOLATION="Interpolation";
		///<summary>A double or float handed as an object to a composite format or text writer of the base library (string.Format,
		///StringBuilder.AppendFormat, TextWriter.Write(format,..), Console, Debug, Trace, string.Concat(object..)): value.AsNetFx().</summary>
		public const string COMPOSITE="Composite";
		///<summary>A double or float handed to a number overload that writes text: StringBuilder.Append/Insert, TextWriter.Write/WriteLine,
		///Console.Write/WriteLine, Debug/Trace.Write(Line)(object): value.ToStringNetFx().</summary>
		public const string APPEND="Append";
		///<summary>DateTime/DateTimeOffset.AddDays .. AddMilliseconds with an argument that is not a whole number: AddXNetFx.</summary>
		public const string DATETIME_ADD="DateTime.AddX";
		///<summary>TimeSpan.FromDays .. FromMilliseconds(double) with an argument that is not a whole number: ODTimeSpan.FromX (S3b-5).</summary>
		public const string TIMESPAN_FROM="TimeSpan.FromX";
		///<summary>string.Join or string.Concat of a sequence of doubles or floats (each one's ToString()): rewritten by hand.</summary>
		public const string JOIN="Join";
		///<summary>Double.ToString or Convert.ToString(double) as a method group (a delegate): rewritten by hand.</summary>
		public const string METHOD_GROUP="MethodGroup";
		///<summary>Reported: a double or float boxed to object (or an interface) and handed to anything else (a data row's cell, a list box, a
		///method of the program that takes an object). Not guarded: most never become text; the port note lists the ones that do.</summary>
		public const string BOXED="Boxed";

		public static bool IsGuarded(string category) {
			return category!=BOXED;
		}
	}

	public class Edit {
		public int Start;
		public int Length;
		public string Text;

		public Edit(int start,int length,string text) {
			Start=start;
			Length=length;
			Text=text;
		}
	}

	public class Site {
		public string Project;
		///<summary>Full path of the source file.</summary>
		public string FilePath;
		///<summary>Path relative to the repository root.</summary>
		public string RelativePath;
		public int Line;
		public int Column;
		public string Category;
		///<summary>The code of the place, on one line.</summary>
		public string Code;
		///<summary>The constant format, if the place has one (for the truth table's list of formats).</summary>
		public string Format;
		///<summary>What the callee is, for the boxed places.</summary>
		public string Target;
		///<summary>The edits that route the place through the helpers; empty when it is rewritten by hand.</summary>
		public List<Edit> ListEdits=new List<Edit>();
		///<summary>True when the edits call an extension method or class of CodeBase, so the file needs "using CodeBase;".</summary>
		public bool NeedsUsingCodeBase;

		public string Key {
			get { return RelativePath+"|"+Category+"|"+Code; }
		}

		public override string ToString() {
			return Category+"\t"+Project+"\t"+RelativePath+"\t"+Line+"\t"+Code+(Format==null ? "" : "\tformat="+Format)+(Target==null ? "" : "\ttarget="+Target);
		}
	}
}
