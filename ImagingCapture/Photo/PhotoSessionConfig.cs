// This file is part of the open source dental practice management software derived from Open Dental.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Globalization;
using ImagingDeviceManager.Config;

namespace ImagingCapture.Photo {
	///<summary>The camera session's settings of this computer (IMG-P4e; owner imaging design, camera and video, section 5 "Camera setup"), in
	///capture.ini beside the camera's own ([Camera], CameraConfig): [Photo] FreezeFirst, ClickSound and IdleMinutes, and [Video] LimitSeconds.
	///Missing or unreadable values take the design's defaults: save at once (decision C2), the click on, 10 idle minutes, a 2-minute video
	///(decision C5).</summary>
	public sealed class PhotoSessionConfig {
		public const string SectionPhoto="Photo";
		public const string SectionVideo="Video";
		public const string KeyFreezeFirst="FreezeFirst";
		public const string KeyClickSound="ClickSound";
		public const string KeyIdleMinutes="IdleMinutes";
		public const string KeyLimitSeconds="LimitSeconds";
		public const int MinutesIdleDefault=10;
		public const int MinutesIdleMin=1;
		public const int MinutesIdleMax=120;
		public const int LimitSecondsDefault=120;
		public const int LimitSecondsMin=10;
		public const int LimitSecondsMax=600;

		///<summary>The first press of Capture freezes the picture and the second saves it (decision C2's setting); false saves at once.</summary>
		public bool IsFreezeFirst { get; set; }
		///<summary>A soft click on every photo.</summary>
		public bool IsClickOn { get; set; }=true;
		///<summary>Minutes without a photo or a key after which the camera is released.</summary>
		public int MinutesIdle { get; set; }=MinutesIdleDefault;
		///<summary>The longest video, in seconds.</summary>
		public int VideoLimitSeconds { get; set; }=LimitSecondsDefault;

		public static PhotoSessionConfig Read(LocalCaptureConfig localCaptureConfig) {
			PhotoSessionConfig config=new PhotoSessionConfig();
			if(localCaptureConfig==null) {
				return config;
			}
			config.IsFreezeFirst=localCaptureConfig.GetBool(SectionPhoto,KeyFreezeFirst,false);
			config.IsClickOn=localCaptureConfig.GetBool(SectionPhoto,KeyClickSound,true);
			config.MinutesIdle=Clamp(localCaptureConfig.GetInt(SectionPhoto,KeyIdleMinutes,MinutesIdleDefault),MinutesIdleMin,MinutesIdleMax);
			config.VideoLimitSeconds=Clamp(localCaptureConfig.GetInt(SectionVideo,KeyLimitSeconds,LimitSecondsDefault),LimitSecondsMin,LimitSecondsMax);
			return config;
		}

		///<summary>Writes the values (clamped) and saves capture.ini. False with error when it could not be saved.</summary>
		public bool Write(LocalCaptureConfig localCaptureConfig,out string error) {
			error="";
			if(localCaptureConfig==null) {
				error="The capture settings file is not known.";
				return false;
			}
			bool isSet=localCaptureConfig.Set(SectionPhoto,KeyFreezeFirst,IsFreezeFirst ? "1" : "0")
				&& localCaptureConfig.Set(SectionPhoto,KeyClickSound,IsClickOn ? "1" : "0")
				&& localCaptureConfig.Set(SectionPhoto,KeyIdleMinutes,Clamp(MinutesIdle,MinutesIdleMin,MinutesIdleMax).ToString(CultureInfo.InvariantCulture))
				&& localCaptureConfig.Set(SectionVideo,KeyLimitSeconds,Clamp(VideoLimitSeconds,LimitSecondsMin,LimitSecondsMax).ToString(CultureInfo.InvariantCulture));
			if(!isSet || !localCaptureConfig.Save()) {
				error=localCaptureConfig.LastError!="" ? localCaptureConfig.LastError : "The camera settings could not be saved.";
				return false;
			}
			return true;
		}

		public PhotoSessionConfig Copy() {
			return (PhotoSessionConfig)MemberwiseClone();
		}

		///<summary>What changed, for the Setup log: "Freeze first: off to on; Video limit: 120 to 90 s". "" when nothing changed.</summary>
		public static string DescribeChanges(PhotoSessionConfig configOld,PhotoSessionConfig configNew) {
			List<string> listChanges=new List<string>();
			if(configOld.IsFreezeFirst!=configNew.IsFreezeFirst) {
				listChanges.Add("Freeze first: "+OnOff(configOld.IsFreezeFirst)+" to "+OnOff(configNew.IsFreezeFirst));
			}
			if(configOld.IsClickOn!=configNew.IsClickOn) {
				listChanges.Add("Click on capture: "+OnOff(configOld.IsClickOn)+" to "+OnOff(configNew.IsClickOn));
			}
			if(configOld.MinutesIdle!=configNew.MinutesIdle) {
				listChanges.Add("Release the camera after: "+configOld.MinutesIdle+" to "+configNew.MinutesIdle+" minutes");
			}
			if(configOld.VideoLimitSeconds!=configNew.VideoLimitSeconds) {
				listChanges.Add("Video limit: "+configOld.VideoLimitSeconds+" to "+configNew.VideoLimitSeconds+" s");
			}
			return string.Join("; ",listChanges);
		}

		private static string OnOff(bool isOn) {
			return isOn ? "on" : "off";
		}

		public static int Clamp(int value,int min,int max) {
			return Math.Max(min,Math.Min(max,value));
		}
	}
}
