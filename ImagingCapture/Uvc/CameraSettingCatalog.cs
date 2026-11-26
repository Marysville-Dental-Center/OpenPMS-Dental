// This file is part of the open source dental practice management software derived from Open Dental.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

namespace ImagingCapture.Uvc {
	///<summary>How a camera setting's value reads (CameraSettingCatalog).</summary>
	public enum CameraSettingKind {
		///<summary>A whole number within the camera's range, with CameraSettingUnit.</summary>
		Number,
		///<summary>0 = Off, 1 = On.</summary>
		OnOff,
		///<summary>One of CameraSettingDef.ArrayChoices; the value is the index.</summary>
		Choice,
	}

	///<summary>The unit of a Number setting, from the DirectShow definitions of the two control interfaces.</summary>
	public enum CameraSettingUnit {
		///<summary>A plain number in the camera's own scale.</summary>
		None,
		///<summary>Degrees: Hue, Pan, Tilt, Roll.</summary>
		Degrees,
		///<summary>Kelvin: White balance.</summary>
		Kelvin,
		///<summary>Exposure: the driver's value is log base 2 of seconds, so -6 is 1/64 s.</summary>
		Log2Seconds,
		///<summary>Iris: the driver's value is the f-stop times 10, so 28 is f/2.8.</summary>
		FStopTimes10,
	}

	///<summary>One camera setting the driver can offer: its readable name and how its value reads.</summary>
	public sealed class CameraSettingDef {
		public CameraSettingDef(UvcControlInterface controlInterface,int property,string displayName,CameraSettingKind kind,CameraSettingUnit unit,
			bool isOnOffWhenBinary,string help,params string[] arrayChoices)
		{
			Interface=controlInterface;
			Property=property;
			DisplayName=displayName;
			Kind=kind;
			Unit=unit;
			IsOnOffWhenBinary=isOnOffWhenBinary;
			Help=help??"";
			ArrayChoices=new ReadOnlyCollection<string>(arrayChoices??new string[0]);
		}

		public UvcControlInterface Interface { get; }
		public int Property { get; }
		///<summary>The id in capture.ini ([Camera.Settings] keys, [Level.N] Setting): the Windows SDK name, for example "Gain".</summary>
		public string Id {
			get {
				return UvcControls.GetPropertyName(Interface,Property);
			}
		}
		///<summary>"White balance", "Iris (aperture)".</summary>
		public string DisplayName { get; }
		public CameraSettingKind Kind { get; }
		public CameraSettingUnit Unit { get; }
		///<summary>A Number setting that reads Off/On when the camera's range is 0 to 1 (Backlight compensation).</summary>
		public bool IsOnOffWhenBinary { get; }
		public string Help { get; }
		///<summary>For a Choice: the text of each value, by value.</summary>
		public IReadOnlyList<string> ArrayChoices { get; }

		public override string ToString() {
			return DisplayName;
		}
	}

	///<summary>Every camera setting the standard driver pages show (Video Proc Amp and Camera Control), with readable names and values: no
	///property numbers and no hex. Format and TryParse turn a driver value into text and back, for the screen and for capture.ini. Nothing
	///here is specific to any camera; the range always comes from the camera.</summary>
	public static class CameraSettingCatalog {
		public const string TextAuto="Auto";
		public const string TextOff="Off";
		public const string TextOn="On";
		private static readonly ReadOnlyCollection<CameraSettingDef> _listAll=new ReadOnlyCollection<CameraSettingDef>(new List<CameraSettingDef> {
			Def(UvcControlInterface.VideoProcAmp,0,"Brightness",CameraSettingKind.Number,CameraSettingUnit.None,"How light the picture is."),
			Def(UvcControlInterface.VideoProcAmp,1,"Contrast",CameraSettingKind.Number,CameraSettingUnit.None,"The difference between light and dark."),
			Def(UvcControlInterface.VideoProcAmp,2,"Hue",CameraSettingKind.Number,CameraSettingUnit.Degrees,"Shifts all colors around the color wheel."),
			Def(UvcControlInterface.VideoProcAmp,3,"Saturation",CameraSettingKind.Number,CameraSettingUnit.None,"How strong the colors are."),
			Def(UvcControlInterface.VideoProcAmp,4,"Sharpness",CameraSettingKind.Number,CameraSettingUnit.None,"Edge sharpening."),
			Def(UvcControlInterface.VideoProcAmp,5,"Gamma",CameraSettingKind.Number,CameraSettingUnit.None,"The brightness of the mid tones."),
			Def(UvcControlInterface.VideoProcAmp,7,"White balance",CameraSettingKind.Number,CameraSettingUnit.Kelvin,"The color temperature the camera treats as white."),
			Def(UvcControlInterface.VideoProcAmp,8,"Backlight compensation",CameraSettingKind.Number,CameraSettingUnit.None,"Brightens a subject in front of a bright background.",
				true),
			Def(UvcControlInterface.VideoProcAmp,9,"Gain",CameraSettingKind.Number,CameraSettingUnit.None,
				"Signal amplification. Some cameras use it for something else, such as the brightness of a built-in light."),
			Def(UvcControlInterface.VideoProcAmp,6,"Color",CameraSettingKind.OnOff,CameraSettingUnit.None,"Off gives a black and white picture."),
			Def(UvcControlInterface.VideoProcAmp,UvcControls.VideoProcAmpPowerLineFrequency,"Power line frequency",CameraSettingKind.Choice,CameraSettingUnit.None,
				"Avoids flicker under lights on the mains: 60 Hz in North America, 50 Hz in most other places.",false,TextOff,"50 Hz","60 Hz",TextAuto),
			Def(UvcControlInterface.CameraControl,4,"Exposure",CameraSettingKind.Number,CameraSettingUnit.Log2Seconds,"How long each frame collects light."),
			Def(UvcControlInterface.CameraControl,UvcControls.CameraControlLowLightCompensation,"Low light compensation",CameraSettingKind.OnOff,CameraSettingUnit.None,
				"Lets the camera lower the frame rate to get a longer exposure in poor light."),
			Def(UvcControlInterface.CameraControl,5,"Iris (aperture)",CameraSettingKind.Number,CameraSettingUnit.FStopTimes10,"The lens opening, as an f-stop."),
			Def(UvcControlInterface.CameraControl,6,"Focus",CameraSettingKind.Number,CameraSettingUnit.None,"The focus distance, in the camera's own scale."),
			Def(UvcControlInterface.CameraControl,3,"Zoom",CameraSettingKind.Number,CameraSettingUnit.None,"Optical zoom, in the camera's own scale."),
			Def(UvcControlInterface.CameraControl,0,"Pan",CameraSettingKind.Number,CameraSettingUnit.Degrees,"Turns the view left or right."),
			Def(UvcControlInterface.CameraControl,1,"Tilt",CameraSettingKind.Number,CameraSettingUnit.Degrees,"Turns the view up or down."),
			Def(UvcControlInterface.CameraControl,2,"Roll",CameraSettingKind.Number,CameraSettingUnit.Degrees,
				"Turns the picture around its center. Some cameras report their buttons here instead."),
		});

		private static CameraSettingDef Def(UvcControlInterface controlInterface,int property,string displayName,CameraSettingKind kind,CameraSettingUnit unit,
			string help,bool isOnOffWhenBinary=false,params string[] arrayChoices)
		{
			return new CameraSettingDef(controlInterface,property,displayName,kind,unit,isOnOffWhenBinary,help,arrayChoices);
		}

		///<summary>Every setting, in the order the settings window lists them: the picture (Video Proc Amp), then the lens (Camera Control).</summary>
		public static IReadOnlyList<CameraSettingDef> All {
			get {
				return _listAll;
			}
		}

		public static CameraSettingDef Get(UvcControlInterface controlInterface,int property) {
			return _listAll.FirstOrDefault(x => x.Interface==controlInterface && x.Property==property);
		}

		///<summary>The setting named by its id ("Gain", "WhiteBalance") or its display name ("White balance"), case and spaces ignored.</summary>
		public static bool TryGet(string text,out CameraSettingDef cameraSettingDef) {
			string textKey=Squeeze(text);
			cameraSettingDef=_listAll.FirstOrDefault(x => Squeeze(x.Id)==textKey || Squeeze(x.DisplayName)==textKey);
			return cameraSettingDef!=null && textKey!="";
		}

		///<summary>The kind for this camera: a Number setting with IsOnOffWhenBinary reads Off/On when the range is 0 to 1.</summary>
		public static CameraSettingKind GetKind(CameraSettingDef cameraSettingDef,UvcControlRange controlRange) {
			if(cameraSettingDef.Kind==CameraSettingKind.Number && cameraSettingDef.IsOnOffWhenBinary && controlRange.IsOk
				&& controlRange.Min==0 && controlRange.Max==1)
			{
				return CameraSettingKind.OnOff;
			}
			return cameraSettingDef.Kind;
		}

		///<summary>The value as people read it: "Auto", "Off", "60 Hz", "1/64 s", "f/2.8", "4600 K", "15 degrees", "128". forFile: the text for
		///capture.ini (degrees without the degree sign, so the file stays plain ASCII).</summary>
		public static string Format(CameraSettingDef cameraSettingDef,UvcControlRange controlRange,int value,bool isAuto,bool forFile=false) {
			if(isAuto) {
				return TextAuto;
			}
			switch(GetKind(cameraSettingDef,controlRange)) {
				case CameraSettingKind.OnOff:
					return value==0 ? TextOff : TextOn;
				case CameraSettingKind.Choice:
					if(value>=0 && value<cameraSettingDef.ArrayChoices.Count) {
						return cameraSettingDef.ArrayChoices[value];
					}
					return value.ToString(CultureInfo.InvariantCulture);
			}
			switch(cameraSettingDef.Unit) {
				case CameraSettingUnit.Degrees:
					return value.ToString(CultureInfo.InvariantCulture)+(forFile ? "" : "°");
				case CameraSettingUnit.Kelvin:
					return value.ToString(CultureInfo.InvariantCulture)+" K";
				case CameraSettingUnit.Log2Seconds:
					return FormatSeconds(value);
				case CameraSettingUnit.FStopTimes10:
					return "f/"+(value/10.0).ToString("0.#",CultureInfo.InvariantCulture);
			}
			return value.ToString(CultureInfo.InvariantCulture);
		}

		///<summary>"1/64 s" for -6, "1 s" for 0, "4 s" for 2.</summary>
		public static string FormatSeconds(int log2Seconds) {
			if(log2Seconds<0) {
				return "1/"+Pow2(-log2Seconds).ToString(CultureInfo.InvariantCulture)+" s";
			}
			return Pow2(log2Seconds).ToString(CultureInfo.InvariantCulture)+" s";
		}

		private static long Pow2(int exponent) {
			return 1L<<Math.Min(exponent,40);
		}

		///<summary>The range as people read it: "0 to 255", "1/2048 s to 1 s", "Off or On", "Off, 50 Hz or 60 Hz", plus ", or Auto" when the
		///camera can set it itself.</summary>
		public static string FormatRange(CameraSettingDef cameraSettingDef,UvcControlRange controlRange) {
			if(!controlRange.IsOk) {
				return "";
			}
			string text;
			CameraSettingKind kind=GetKind(cameraSettingDef,controlRange);
			if(kind==CameraSettingKind.Number) {
				text=Format(cameraSettingDef,controlRange,controlRange.Min,false)+" to "+Format(cameraSettingDef,controlRange,controlRange.Max,false);
				if(controlRange.Step>1) {
					text+=", in steps of "+controlRange.Step.ToString(CultureInfo.InvariantCulture);
				}
			}
			else {
				List<string> listChoices=GetChoices(cameraSettingDef,controlRange).Where(x => x!=TextAuto || kind==CameraSettingKind.Choice).ToList();
				text=listChoices.Count<=1 ? string.Join("",listChoices)
					: string.Join(", ",listChoices.Take(listChoices.Count-1))+" or "+listChoices.Last();
			}
			if(controlRange.CanAuto && kind==CameraSettingKind.Number) {
				text+=", or Auto";
			}
			return text;
		}

		///<summary>The choices of an On/Off or Choice setting within the camera's range, then "Auto" when the camera can set it itself. Empty
		///for a Number without Auto; only "Auto" for a Number with it (the number is typed).</summary>
		public static List<string> GetChoices(CameraSettingDef cameraSettingDef,UvcControlRange controlRange) {
			List<string> listChoices=new List<string>();
			CameraSettingKind kind=GetKind(cameraSettingDef,controlRange);
			int min=controlRange.IsOk ? controlRange.Min : 0;
			int max=controlRange.IsOk ? controlRange.Max : int.MaxValue;
			if(kind==CameraSettingKind.OnOff) {
				listChoices.Add(TextOff);
				listChoices.Add(TextOn);
			}
			else if(kind==CameraSettingKind.Choice) {
				for(int i=0;i<cameraSettingDef.ArrayChoices.Count;i++) {
					if(i>=min && i<=max) {
						listChoices.Add(cameraSettingDef.ArrayChoices[i]);
					}
				}
			}
			if(controlRange.IsOk && controlRange.CanAuto && !listChoices.Contains(TextAuto)) {
				listChoices.Add(TextAuto);
			}
			return listChoices;
		}

		///<summary>Reads a value as people write it (what Format gives, case and spaces ignored; a number may leave out its unit; "On" also
		///reads "1" and "Off" "0"; exposure also reads "0.5 s" and is rounded to the nearest power of 2). Checks it against controlRange when
		///that is known: out of range, or Auto on a camera that cannot, is false with error.</summary>
		public static bool TryParse(CameraSettingDef cameraSettingDef,UvcControlRange controlRange,string text,out int value,out bool isAuto,out string error) {
			value=0;
			isAuto=false;
			error="";
			string textTrimmed=(text??"").Trim();
			string textKey=Squeeze(textTrimmed);
			if(textKey=="") {
				error="No value.";
				return false;
			}
			CameraSettingKind kind=GetKind(cameraSettingDef,controlRange);
			if(textKey==Squeeze(TextAuto) && !(kind==CameraSettingKind.Choice && cameraSettingDef.ArrayChoices.Any(x => Squeeze(x)==textKey))) {
				if(controlRange.IsOk && !controlRange.CanAuto) {
					error=cameraSettingDef.DisplayName+" cannot be set to Auto on this camera.";
					return false;
				}
				isAuto=true;
				value=controlRange.IsOk ? controlRange.Default : 0;
				return true;
			}
			bool isParsed;
			switch(kind) {
				case CameraSettingKind.OnOff:
					isParsed=TryParseOnOff(textKey,out value);
					break;
				case CameraSettingKind.Choice:
					value=cameraSettingDef.ArrayChoices.ToList().FindIndex(x => Squeeze(x)==textKey);
					isParsed=value>=0 || int.TryParse(textKey,NumberStyles.Integer,CultureInfo.InvariantCulture,out value);
					break;
				default:
					isParsed=TryParseNumber(cameraSettingDef.Unit,textKey,out value);
					break;
			}
			if(!isParsed) {
				error="\""+textTrimmed+"\" is not a value of "+cameraSettingDef.DisplayName+(controlRange.IsOk ? " ("+FormatRange(cameraSettingDef,controlRange)+")." : ".");
				return false;
			}
			if(controlRange.IsOk && (value<controlRange.Min || value>controlRange.Max)) {
				error=Format(cameraSettingDef,controlRange,value,false)+" is outside what this camera allows for "+cameraSettingDef.DisplayName
					+" ("+FormatRange(cameraSettingDef,controlRange)+").";
				return false;
			}
			return true;
		}

		private static bool TryParseOnOff(string textKey,out int value) {
			value=0;
			if(textKey==Squeeze(TextOn) || textKey=="1") {
				value=1;
				return true;
			}
			return textKey==Squeeze(TextOff) || textKey=="0";
		}

		private static bool TryParseNumber(CameraSettingUnit unit,string textKey,out int value) {
			value=0;
			switch(unit) {
				case CameraSettingUnit.Degrees:
					textKey=TrimEnd(TrimEnd(TrimEnd(textKey,"°"),"degrees"),"deg");
					break;
				case CameraSettingUnit.Kelvin:
					textKey=TrimEnd(textKey,"k");
					break;
				case CameraSettingUnit.Log2Seconds:
					return TryParseSeconds(textKey,out value);
				case CameraSettingUnit.FStopTimes10:
					textKey=textKey.StartsWith("f/") ? textKey.Substring(2) : textKey;
					double fStop;
					if(!double.TryParse(textKey,NumberStyles.Float,CultureInfo.InvariantCulture,out fStop) || fStop<=0 || fStop>1000) {
						return false;
					}
					value=(int)Math.Round(fStop*10);
					return true;
			}
			return int.TryParse(textKey,NumberStyles.Integer,CultureInfo.InvariantCulture,out value);
		}

		///<summary>"1/64s", "1/64", "0.5s", "2s" (already squeezed) to log base 2 of seconds, rounded.</summary>
		private static bool TryParseSeconds(string textKey,out int value) {
			value=0;
			textKey=TrimEnd(TrimEnd(textKey,"seconds"),"s");
			double seconds;
			int idxSlash=textKey.IndexOf('/');
			if(idxSlash>=0) {
				double numerator;
				double denominator;
				if(!double.TryParse(textKey.Substring(0,idxSlash),NumberStyles.Float,CultureInfo.InvariantCulture,out numerator)
					|| !double.TryParse(textKey.Substring(idxSlash+1),NumberStyles.Float,CultureInfo.InvariantCulture,out denominator) || denominator<=0)
				{
					return false;
				}
				seconds=numerator/denominator;
			}
			else if(!double.TryParse(textKey,NumberStyles.Float,CultureInfo.InvariantCulture,out seconds)) {
				return false;
			}
			if(seconds<=0 || double.IsInfinity(seconds) || double.IsNaN(seconds)) {
				return false;
			}
			value=(int)Math.Round(Math.Log(seconds,2));
			return value>=-40 && value<=40;
		}

		private static string TrimEnd(string text,string suffix) {
			return text.EndsWith(suffix,StringComparison.Ordinal) ? text.Substring(0,text.Length-suffix.Length) : text;
		}

		///<summary>Lower case without spaces, for comparing names and values.</summary>
		private static string Squeeze(string text) {
			return new string((text??"").Where(x => !char.IsWhiteSpace(x)).ToArray()).ToLowerInvariant();
		}
	}
}
