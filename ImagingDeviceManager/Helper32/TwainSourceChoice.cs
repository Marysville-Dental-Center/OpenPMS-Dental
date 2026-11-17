// This file is part of the open source dental practice management software derived from Open Dental.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;

namespace ImagingDeviceManager.Helper32 {
	///<summary>Which TWAIN data source manager a source was chosen from (S3b-8, decision 23: native 64-bit first).</summary>
	public enum TwainDsmKind {
		///<summary>Not recorded: a choice stored before S3b-8, or by hand. The program looks in its own DSM first, then in the 32-bit one.</summary>
		Unspecified,
		///<summary>The DSM of the program's own process: the 64-bit TWAINDSM.dll of System32 in the x64 program.</summary>
		Native,
		///<summary>The 32-bit TWAINDSM.dll of SysWOW64, through the 32-bit helper: a source with only a 32-bit driver.</summary>
		Helper32,
	}

	///<summary>A stored TWAIN source choice (ImagingDevice.TwainName) and which DSM it came from. A source of the program's own DSM is stored by
	///its ProductName alone, as before; a source that only the 32-bit DSM lists is stored as its ProductName followed by Marker32, so that the
	///device list shows it and the choice records where it came from. Parse reads both, and a name without the marker (every choice stored
	///before S3b-8) reads as Unspecified, which still works: the session looks for it natively first, then in the 32-bit DSM (TwainSourcePath).
	///A ProductName is at most 32 characters (TW_STR32), and the marker is never part of one the program wrote; a source that really has a name
	///ending with the marker is still found, because the whole text is looked up natively before the marker is read (TwainSourcePath).</summary>
	public sealed class TwainSourceChoice {
		///<summary>What a 32-bit-only choice ends with.</summary>
		public const string Marker32=" (32-bit TWAIN)";

		///<summary>The source's ProductName, trimmed, without the marker.</summary>
		public string ProductName { get; }
		public TwainDsmKind Dsm { get; }
		///<summary>The text the choice was read from, trimmed.</summary>
		public string StoredText { get; }

		public TwainSourceChoice(string productName,TwainDsmKind dsm) {
			ProductName=(productName??"").Trim();
			Dsm=dsm;
			StoredText=Format(ProductName,dsm);
		}

		private TwainSourceChoice(string productName,TwainDsmKind dsm,string storedText) {
			ProductName=productName;
			Dsm=dsm;
			StoredText=storedText;
		}

		///<summary>The text to store: the ProductName, followed by Marker32 for a 32-bit-only source.</summary>
		public static string Format(string productName,TwainDsmKind dsm) {
			string name=(productName??"").Trim();
			if(dsm==TwainDsmKind.Helper32 && name!="") {
				return name+Marker32;
			}
			return name;
		}

		///<summary>The choice stored as storedText. Null or blank gives an empty ProductName.</summary>
		public static TwainSourceChoice Parse(string storedText) {
			string text=(storedText??"").Trim();
			if(text.Length>Marker32.Length && text.EndsWith(Marker32,StringComparison.OrdinalIgnoreCase)) {
				string name=text.Substring(0,text.Length-Marker32.Length).Trim();
				if(name!="") {
					return new TwainSourceChoice(name,TwainDsmKind.Helper32,text);
				}
			}
			return new TwainSourceChoice(text,TwainDsmKind.Unspecified,text);
		}

		///<summary>The ProductName of storedText, without the marker: what a source gate ([Twain] AllowedSources) and MSG_OPENDS compare.</summary>
		public static string GetProductName(string storedText) {
			return Parse(storedText).ProductName;
		}
	}
}
