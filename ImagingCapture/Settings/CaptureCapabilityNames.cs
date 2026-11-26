// This file is part of the open source dental practice management software derived from Open Dental.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ImagingDeviceManager.Config;

namespace ImagingCapture.Settings {
	///<summary>FB-1: readable names for the custom capability numbers in Capture settings (rows such as SettingsCap=0x8005 were
	///unreadable). The names are not in the code: device-specific numbers come from the TWAIN source's published contract, so each computer's
	///capture.ini lists them, one per line, name=number:
	///  [CapabilityNames]
	///  MYCAP_SETTINGS=0x8005
	///Capture settings shows the name next to the number and accepts the name when typed. capture.ini still stores the hex number, so the
	///gated settings hash and every reader of the file are unchanged. Names are never filled in as a default.</summary>
	public class CaptureCapabilityNames {
		public const string SectionCapabilityNames="CapabilityNames";
		private readonly List<(int Number,string Name)> _listCaps=new List<(int,string)>();

		///<summary>No names: every value shows as written.</summary>
		public CaptureCapabilityNames() {
		}

		///<summary>The names of [CapabilityNames]. A line whose value is no capability number (hex, 0x0001-0xFFFF) or whose name is not a
		///plain word is skipped; for a number listed twice the first name wins.</summary>
		public static CaptureCapabilityNames Read(LocalCaptureConfig config) {
			CaptureCapabilityNames capabilityNames=new CaptureCapabilityNames();
			if(config==null) {
				return capabilityNames;
			}
			foreach(string key in config.GetKeyNames(SectionCapabilityNames)) {
				string name=(key??"").Trim();
				int number;
				if(name=="" || !name.All(x => char.IsLetterOrDigit(x) || x=='_') || name.All(char.IsDigit)
					|| !TryParseHex(config.GetString(SectionCapabilityNames,key),out number) || number<=0 || number>ushort.MaxValue)
				{
					continue;
				}
				capabilityNames.Add(number,name);
			}
			return capabilityNames;
		}

		///<summary>Adds a name (tests, and Read).</summary>
		public void Add(int number,string name) {
			if(_listCaps.Any(x => x.Number==number || string.Equals(x.Name,name,StringComparison.OrdinalIgnoreCase))) {
				return;
			}
			_listCaps.Add((number,name));
		}

		public int Count => _listCaps.Count;

		///<summary>The name of a capability number, or "" when it has none.</summary>
		public string GetName(int number) {
			return _listCaps.FirstOrDefault(x => x.Number==number).Name??"";
		}

		///<summary>For the grid: the name of a named number, "NAME" (no hex on screen once it has a name); the text as written otherwise, since
		///an unnamed number has nothing else to show (empty stays empty).</summary>
		public string ToDisplay(string text) {
			return ToEditText(text);
		}

		///<summary>For the value box: the name for a named number, the text as written otherwise.</summary>
		public string ToEditText(string text) {
			int number;
			if(!TryParseHex(text,out number)) {
				return text??"";
			}
			string name=GetName(number);
			return name=="" ? text : name;
		}

		///<summary>What is stored for what was typed: a known name (any case, with or without the "(0x....)" the grid shows) becomes its hex
		///number; anything else is stored as typed and checked by the validator.</summary>
		public string ToStored(string text) {
			string trimmed=(text??"").Trim();
			int idxParen=trimmed.IndexOf('(');
			string namePart=idxParen>0 ? trimmed.Substring(0,idxParen).Trim() : trimmed;
			var cap=_listCaps.FirstOrDefault(x => string.Equals(x.Name,namePart,StringComparison.OrdinalIgnoreCase));
			if(cap.Name!=null) {
				return ToHex(cap.Number);
			}
			return text??"";
		}

		///<summary>The help line: the names that can be typed, or how to add them.</summary>
		public string GetHelp() {
			if(_listCaps.Count==0) {
				return "To see names instead of numbers, list them in capture.ini under ["+SectionCapabilityNames+"], one per line: NAME=0x8001.";
			}
			return "Type the name or the number. Names from capture.ini ["+SectionCapabilityNames+"]: "
				+string.Join(", ",_listCaps.OrderBy(x => x.Number).Select(x => x.Name))+".";
		}

		private static string ToHex(int number) {
			return "0x"+number.ToString("X4",CultureInfo.InvariantCulture);
		}

		///<summary>The same rule as LocalCaptureConfig.TryParseHex.</summary>
		private static bool TryParseHex(string text,out int value) {
			value=0;
			if(text==null) {
				return false;
			}
			string digits=text.Trim();
			if(digits.StartsWith("0x",StringComparison.OrdinalIgnoreCase)) {
				digits=digits.Substring(2);
			}
			uint parsed;
			if(digits.Length==0 || !uint.TryParse(digits,NumberStyles.AllowHexSpecifier,CultureInfo.InvariantCulture,out parsed) || parsed>int.MaxValue) {
				return false;
			}
			value=(int)parsed;
			return true;
		}
	}
}
