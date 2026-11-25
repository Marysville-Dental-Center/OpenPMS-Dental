// This file is part of the open source dental practice management software derived from Open Dental.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ImagingCapture.Metadata;
using ImagingDeviceManager.Capture;
using ImagingDeviceManager.Config;

namespace ImagingCapture.Settings {
	///<summary>One setting the driver describes (source-settings-schema/1).</summary>
	public sealed class SourceSettingsSchemaEntry {
		///<summary>"Sensor.XRayMa": the driver's Section.Key, sent as is.</summary>
		public string Key { get; set; }
		///<summary>choice, integer, number or text.</summary>
		public string Type { get; set; }
		public List<string> ListValues { get; set; }=new List<string>();
		public double? Min { get; set; }
		public double? Max { get; set; }
		///<summary>What the driver uses when nothing is set ("" when it then uses what the sensor has).</summary>
		public string Default { get; set; }="";
		///<summary>It changes the delivered pixels.</summary>
		public bool ChangesImage { get; set; }
		public string Help { get; set; }="";
	}

	///<summary>The sensor driver's description of its settings (twain-contract 1.4, IMG-SRCSET), which the session saves beside capture.ini at each
	///open and Capture settings builds its Source.* rows from. It is untrusted input: its keys become capture.ini key names and its texts appear
	///on screen, so Parse takes it only within strict limits (size, known types, a safe character set, length limits) and refuses the whole
	///description otherwise, with the reason. A refused description gives no rows.</summary>
	public sealed class SourceSettingsSchema {
		public const string SchemaName="source-settings-schema/1";
		public const int JsonLengthMax=64*1024;
		public const int SettingsMax=100;
		public const int ValuesMax=32;
		public const int ValueLengthMax=32;
		public const int HelpLengthMax=400;
		///<summary>Section.Key: letters and digits, each part starting with a letter, at most 32 characters each.</summary>
		private static readonly Regex _regexKey=new Regex(@"^[A-Za-z][A-Za-z0-9]{0,31}\.[A-Za-z][A-Za-z0-9]{0,31}$",RegexOptions.CultureInvariant);
		///<summary>A choice or default value: letters, digits, space and . , _ + - only.</summary>
		private static readonly Regex _regexValue=new Regex(@"^[A-Za-z0-9 .,_+\-]*$",RegexOptions.CultureInvariant);
		private static readonly string[] _arrayTypes={ "choice","integer","number","text" };

		public string Source { get; private set; }="";
		public string Version { get; private set; }="";
		public List<SourceSettingsSchemaEntry> ListEntries { get; }=new List<SourceSettingsSchemaEntry>();

		///<summary>The description in json, or null with error (one sentence) when it is not a valid source-settings-schema/1 document within the
		///limits. Never throws.</summary>
		public static SourceSettingsSchema Parse(string json,out string error) {
			error="";
			try {
				if(string.IsNullOrEmpty(json)) {
					error="The driver's settings description is empty.";
					return null;
				}
				if(json.Length>JsonLengthMax) {
					error="The driver's settings description is longer than "+JsonLengthMax+" characters.";
					return null;
				}
				JsonValue root=JsonReader.Parse(json);
				if(root==null || root.Kind!=JsonKind.Object) {
					error="The driver's settings description is not a JSON object.";
					return null;
				}
				if(GetString(root,"schema")!=SchemaName) {
					error="The driver's settings description is not "+SchemaName+".";
					return null;
				}
				SourceSettingsSchema schema=new SourceSettingsSchema { Source=Limit(GetString(root,"source")),Version=Limit(GetString(root,"version")) };
				JsonValue jsonSettings=root.Get("settings");
				if(jsonSettings==null || jsonSettings.Kind!=JsonKind.Array || jsonSettings.ListItems.Count==0 || jsonSettings.ListItems.Count>SettingsMax) {
					error="The driver's settings description has no list of 1 to "+SettingsMax+" settings.";
					return null;
				}
				HashSet<string> hashSetKeys=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
				for(int i=0;i<jsonSettings.ListItems.Count;i++) {
					string errorEntry;
					SourceSettingsSchemaEntry entry=ParseEntry(jsonSettings.ListItems[i],out errorEntry);
					if(entry==null) {
						error="Setting "+(i+1).ToString(CultureInfo.InvariantCulture)+" of the driver's settings description: "+errorEntry;
						return null;
					}
					if(!hashSetKeys.Add(entry.Key)) {
						error="The driver's settings description lists "+entry.Key+" twice.";
						return null;
					}
					schema.ListEntries.Add(entry);
				}
				return schema;
			}
			catch(Exception ex) {
				error="The driver's settings description could not be read: "+ex.Message;
				return null;
			}
		}

		///<summary>The description saved for the device beside capture.ini (folderImaging; null: the folder of config's primary file), or null
		///with error ("" when there is none: no SettingsSchemaCap, or the session has not opened the source yet).</summary>
		public static SourceSettingsSchema LoadForDevice(LocalCaptureConfig config,long imagingDeviceNum,out string error) {
			error="";
			string folderImaging=config?.PathPrimary==null ? null : Path.GetDirectoryName(config.PathPrimary);
			string json=TwainSourceSettings.ReadSchema(folderImaging,imagingDeviceNum);
			if(json==null) {
				return null;
			}
			return Parse(json,out error);
		}

		private static SourceSettingsSchemaEntry ParseEntry(JsonValue jsonEntry,out string error) {
			error="";
			if(jsonEntry==null || jsonEntry.Kind!=JsonKind.Object) {
				error="not an object.";
				return null;
			}
			SourceSettingsSchemaEntry entry=new SourceSettingsSchemaEntry();
			entry.Key=GetString(jsonEntry,"key")??"";
			if(!_regexKey.IsMatch(entry.Key)) {
				error="its key is not Section.Key with letters and digits only (at most 32 each).";
				return null;
			}
			entry.Type=GetString(jsonEntry,"type")??"";
			if(!_arrayTypes.Contains(entry.Type)) {
				error=entry.Key+" has a type this program does not know (choice, integer, number or text).";
				return null;
			}
			if(entry.Type=="choice") {
				JsonValue jsonValues=jsonEntry.Get("values");
				if(jsonValues==null || jsonValues.Kind!=JsonKind.Array || jsonValues.ListItems.Count==0 || jsonValues.ListItems.Count>ValuesMax) {
					error=entry.Key+" is a choice without 1 to "+ValuesMax+" values.";
					return null;
				}
				foreach(JsonValue jsonValue in jsonValues.ListItems) {
					string value=jsonValue.Kind==JsonKind.String ? jsonValue.Text : null;
					if(!IsSafeValue(value) || value=="") {
						error=entry.Key+" has a value that is empty, longer than "+ValueLengthMax+" characters or has other characters than letters, digits, "
							+"space and . , _ + -.";
						return null;
					}
					entry.ListValues.Add(value);
				}
			}
			if(entry.Type=="integer" || entry.Type=="number") {
				entry.Min=GetNumber(jsonEntry,"min");
				entry.Max=GetNumber(jsonEntry,"max");
				if(entry.Min!=null && entry.Max!=null && entry.Min>entry.Max) {
					error=entry.Key+" has a min above its max.";
					return null;
				}
			}
			entry.Default=GetString(jsonEntry,"default")??"";
			if(!IsSafeValue(entry.Default)) {
				error=entry.Key+" has a default with other characters than letters, digits, space and . , _ + -, or longer than "+ValueLengthMax+".";
				return null;
			}
			JsonValue jsonChanges=jsonEntry.Get("changesImage");
			entry.ChangesImage=jsonChanges!=null && jsonChanges.Kind==JsonKind.Bool && jsonChanges.Bool;
			entry.Help=GetString(jsonEntry,"help")??"";
			if(entry.Help.Length>HelpLengthMax || entry.Help.Any(x => char.IsControl(x))) {
				error=entry.Key+" has a help text longer than "+HelpLengthMax+" characters or with control characters.";
				return null;
			}
			return entry;
		}

		private static bool IsSafeValue(string value) {
			return value!=null && value.Length<=ValueLengthMax && _regexValue.IsMatch(value);
		}

		private static string GetString(JsonValue jsonObject,string name) {
			JsonValue jsonValue=jsonObject.Get(name);
			return jsonValue!=null && jsonValue.Kind==JsonKind.String ? jsonValue.Text : null;
		}

		private static double? GetNumber(JsonValue jsonObject,string name) {
			JsonValue jsonValue=jsonObject.Get(name);
			double value;
			return jsonValue!=null && jsonValue.TryGetDouble(out value) ? value : (double?)null;
		}

		///<summary>Source and version, for display: at most 64 characters without control characters.</summary>
		private static string Limit(string text) {
			string textClean=new string((text??"").Where(x => !char.IsControl(x)).ToArray());
			return textClean.Length>64 ? textClean.Substring(0,64) : textClean;
		}
	}
}
