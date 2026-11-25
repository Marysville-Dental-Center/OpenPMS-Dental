// This file is part of the open source dental practice management software derived from Open Dental.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ImagingDeviceManager.Config;

namespace ImagingDeviceManager.Capture {
	///<summary>The sensor driver's own settings, kept in capture.ini as `[Device.N] Source.&lt;Section&gt;.&lt;Key&gt;` and sent to the source before
	///each enable through its settings capability (`[Device.N] SettingsCap`, IMG-SRCSET; twain-contract 1.4 "host settings"). They replace a
	///hand edit of the driver's own INI file on each computer. This program knows no driver's settings: the driver describes them (its
	///settings-description capability, `[Device.N] SettingsSchemaCap`), the session saves that description beside capture.ini each time it
	///opens the source (GetSchemaFileName), and Capture settings builds its rows from it. An empty value is not sent, so the driver's own value
	///applies.</summary>
	public static class TwainSourceSettings {
		///<summary>The prefix of the capture.ini keys: `Source.Sensor.XRaySource`.</summary>
		public const string KeyPrefix="Source.";

		///<summary>True for a `Source.*` key name (any case).</summary>
		public static bool IsSourceKey(string key) {
			return key!=null && key.Trim().StartsWith(KeyPrefix,StringComparison.OrdinalIgnoreCase) && key.Trim().Length>KeyPrefix.Length;
		}

		///<summary>The `Source.*` keys of section that have a value, in ordinal order ignoring case (the order they are sent and hashed in).</summary>
		public static List<string> GetSetKeys(LocalCaptureConfig config,string section) {
			if(config==null) {
				return new List<string>();
			}
			return config.GetKeyNames(section).Where(x => IsSourceKey(x) && config.GetString(section,x,"").Trim()!="")
				.OrderBy(x => x,StringComparer.OrdinalIgnoreCase).ToList();
		}

		///<summary>The most UTF-8 bytes the settings text may have: one TWTY_STR1024 without its terminating NUL.</summary>
		public const int TextBytesMax=1025;

		///<summary>The `Section.Key=value;...` text for the settings capability from the device's section: every `Source.*` key with a value, in
		///GetSetKeys order. "" when none is set.</summary>
		public static string Build(LocalCaptureConfig config,string section,List<string> listWarnings) {
			string error;
			return Build(config,section,listWarnings,out error);
		}

		///<summary>Build, with error: "" or why a set value cannot be sent (one with ';' or '=', which the text cannot carry; it is left out, with
		///a warning). The session does not enable with such an error: the office's setting would not be applied.</summary>
		public static string Build(LocalCaptureConfig config,string section,List<string> listWarnings,out string error) {
			error="";
			List<string> listPairs=new List<string>();
			foreach(string key in GetSetKeys(config,section)) {
				string value=config.GetString(section,key,"").Trim();
				if(value.IndexOf(';')>=0 || value.IndexOf('=')>=0) {
					listWarnings?.Add("["+section+"] "+key+"="+value+" has ';' or '=', which the driver's settings cannot carry; it is not sent.");
					error+=(error=="" ? "" : " ")+key.Trim()+" has ';' or '=', which the driver's settings cannot carry.";
					continue;
				}
				listPairs.Add(key.Trim().Substring(KeyPrefix.Length)+"="+value);
			}
			return string.Join(";",listPairs);
		}

		///<summary>"source-settings-12.json": the driver's description of its settings for device 12, beside capture.ini.</summary>
		public static string GetSchemaFileName(long imagingDeviceNum) {
			return "source-settings-"+imagingDeviceNum.ToString(System.Globalization.CultureInfo.InvariantCulture)+".json";
		}

		///<summary>Writes json as the description file in folderImaging, only when it differs from what is there. Returns "" or the error.
		///Never throws.</summary>
		public static string SaveSchema(string folderImaging,long imagingDeviceNum,string json) {
			try {
				if(string.IsNullOrWhiteSpace(folderImaging) || imagingDeviceNum<=0 || string.IsNullOrEmpty(json)) {
					return "nothing to save";
				}
				string path=Path.Combine(folderImaging,GetSchemaFileName(imagingDeviceNum));
				if(File.Exists(path) && File.ReadAllText(path,Encoding.UTF8)==json) {
					return "";
				}
				Directory.CreateDirectory(folderImaging);
				string pathTemp=path+".tmp";
				File.WriteAllText(pathTemp,json,new UTF8Encoding(false));
				File.Copy(pathTemp,path,true);
				File.Delete(pathTemp);
				return "";
			}
			catch(Exception ex) {
				return ex.Message;
			}
		}

		///<summary>The saved description for the device in folderImaging, or null when there is none or it cannot be read.</summary>
		public static string ReadSchema(string folderImaging,long imagingDeviceNum) {
			try {
				if(string.IsNullOrWhiteSpace(folderImaging) || imagingDeviceNum<=0) {
					return null;
				}
				string path=Path.Combine(folderImaging,GetSchemaFileName(imagingDeviceNum));
				return File.Exists(path) ? File.ReadAllText(path,Encoding.UTF8) : null;
			}
			catch {
				return null;
			}
		}
	}
}
