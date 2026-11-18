// The UI-9a tests (docs/specs/ui-refresh-plan.md section 7's UI-9a row, docs/port-notes/UI-9a.md): the bitmap pictures of the converted slices
// (icons/picture-slices.tsv) turned into icons by tools/Convert-PictureUses.ps1 from icons/picture-map.tsv. The map has a row for every picture
// use the tool finds in a converted slice and the slice has nothing left to convert; every designer GetObject and Properties.Resources picture
// still has its resource; each recorded conversion (icons/picture-converted.tsv) is in its designer line and, in the built window, the control
// shows the icon (UI.Button.Icon) or a bitmap of the old picture's size (IconLibrary.GetBitmap), so nothing moves. The gallery G-UI3 slice is
// opt-in: OPMS_UI9A_PICTURES names the folder for each converted window's picture and each converted control's picture, off screen (UI-0b's
// builder); OPMS_UI9A_RECORD may name another record (the tool's -Check -Record output, for the before run on the commit before the conversion).
// No database, no window shown.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental;
using OpenPmsDental.UI;
using UiRefreshTests.Scan;
using UiRefreshTests.TextFit;

namespace UiRefreshTests {
	[TestClass]
	public class PictureMapTests {
		public const string VARIABLE_PICTURES="OPMS_UI9A_PICTURES";
		public const string VARIABLE_RECORD="OPMS_UI9A_RECORD";

		///<summary>A row of icons/picture-converted.tsv.</summary>
		public class ConvertedRow {
			public string Slice;
			public string File;
			public string Control;
			public string Property;
			public string Key;
			public string Target;
			public string How;
			public int Width;
			public int Height;

			public string WindowName {
				get { return Path.GetFileName(File).Split('.')[0]; }
			}

			public override string ToString() {
				return File+" "+Control+" "+Key+" -> "+Target+" ("+How+" "+Width+"x"+Height+")";
			}
		}

		#region Helpers
		private static List<string[]> ReadTsv(string path) {
			List<string[]> listRows=new List<string[]>();
			foreach(string line in File.ReadAllLines(path)) {
				if(line.Trim()=="" || line.StartsWith("#")) {
					continue;
				}
				listRows.Add(line.Split('\t'));
			}
			return listRows;
		}

		public static List<ConvertedRow> ReadRecord(string path) {
			List<ConvertedRow> listRows=new List<ConvertedRow>();
			if(!File.Exists(path)) {
				return listRows;
			}
			foreach(string[] arrayFields in ReadTsv(path)) {
				if(arrayFields[0]=="slice") {
					continue;
				}
				listRows.Add(new ConvertedRow { Slice=arrayFields[0],File=arrayFields[1],Control=arrayFields[2],Property=arrayFields[3],Key=arrayFields[4],
					Target=arrayFields[5],How=arrayFields[6],Width=int.Parse(arrayFields[7]),Height=int.Parse(arrayFields[8]) });
			}
			return listRows;
		}

		private static List<string> GetSlices() {
			return ReadTsv(Path.Combine(RepoPaths.Root(),"icons","picture-slices.tsv")).Select(x => x[0]).ToList();
		}

		///<summary>Runs tools/Convert-PictureUses.ps1 with the arguments in Windows PowerShell; returns its exit code and output.</summary>
		private static int RunTool(string arguments,out string output) {
			string pathScript=Path.Combine(RepoPaths.Root(),"tools","Convert-PictureUses.ps1");
			ProcessStartInfo processStartInfo=new ProcessStartInfo("powershell.exe",
				"-NoProfile -ExecutionPolicy Bypass -File \""+pathScript+"\" -Root \""+RepoPaths.Root()+"\" "+arguments);
			processStartInfo.UseShellExecute=false;
			processStartInfo.RedirectStandardOutput=true;
			processStartInfo.RedirectStandardError=true;
			processStartInfo.CreateNoWindow=true;
			using(Process process=Process.Start(processStartInfo)) {
				System.Threading.Tasks.Task<string> taskError=process.StandardError.ReadToEndAsync();
				output=process.StandardOutput.ReadToEnd();
				process.WaitForExit();
				output+=taskError.Result;
				return process.ExitCode;
			}
		}

		private static object GetField(object obj,string name) {
			for(Type type=obj.GetType();type!=null;type=type.BaseType) {
				FieldInfo fieldInfo=type.GetField(name,BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public|BindingFlags.DeclaredOnly);
				if(fieldInfo!=null) {
					return fieldInfo.GetValue(obj);
				}
			}
			return null;
		}

		///<summary>The resx entry names of a .resx file (outside its header comment).</summary>
		private static HashSet<string> GetResxNames(string path) {
			string text=Regex.Replace(File.ReadAllText(path),"<!--[\\s\\S]*?-->","");
			return new HashSet<string>(Regex.Matches(text,"<data name=\"(?<n>[^\"]+)\"").Cast<Match>().Select(x => x.Groups["n"].Value));
		}

		private static int CountInk(Bitmap bitmap) {
			int count=0;
			for(int y=0;y<bitmap.Height;y++) {
				for(int x=0;x<bitmap.Width;x++) {
					if(bitmap.GetPixel(x,y).A>=64) {
						count++;
					}
				}
			}
			return count;
		}
		#endregion Helpers

		[TestMethod]
		public void Map_HasARowForEveryPictureUse_AndEveryConvertedSliceIsDone() {
			List<string> listSlices=GetSlices();
			Assert.IsTrue(listSlices.Count>0,"icons/picture-slices.tsv lists no slice.");
			string folder=Path.Combine(Path.GetTempPath(),"UiRefreshTests-ui9a-"+Guid.NewGuid().ToString("N"));
			Directory.CreateDirectory(folder);
			try {
				foreach(string slice in listSlices) {
					string pathInventory=Path.Combine(folder,slice+".tsv");
					string output;
					int exitCode=RunTool("-Slice "+slice+" -Check -Inventory \""+pathInventory+"\"",out output);
					Assert.IsTrue(File.Exists(pathInventory),slice+": the tool wrote no inventory.\r\n"+output);
					List<string[]> listUses=ReadTsv(pathInventory).Skip(1).ToList();
					Assert.IsTrue(listUses.Count>0,slice+": the tool found no picture use.");
					List<string> listUnmapped=listUses.Where(x => x[2]=="").Select(x => x[1]+" ("+x[3]+":"+x[4]+")").ToList();
					Assert.AreEqual(0,listUnmapped.Count,slice+": picture uses with no row in icons/picture-map.tsv:\r\n"+string.Join("\r\n",listUnmapped));
					//Nothing left to convert: every use is a keep, a UI-9b ImageList or a recorded conversion.
					List<string> listLeft=listUses.Where(x => (x[0]=="Resources" || x[0]=="Resx") && x[2]!="keep" && x[2]!="UI-9b")
						.Select(x => x[1]+" -> "+x[2]+" ("+x[3]+":"+x[4]+")").ToList();
					Assert.AreEqual(0,listLeft.Count,slice+": pictures still to convert (run the tool with -Apply):\r\n"+string.Join("\r\n",listLeft));
					Assert.AreEqual(0,exitCode,slice+": the tool's -Check reports changes or unmapped uses:\r\n"+output);
					Assert.IsTrue(listUses.Any(x => x[0]=="Converted"),slice+": no recorded conversion in icons/picture-converted.tsv.");
				}
			}
			finally {
				Directory.Delete(folder,true);
			}
		}

		[TestMethod]
		public void Record_EveryConversionIsInItsDesignerLine_WithAKnownIcon() {
			List<ConvertedRow> listRows=ReadRecord(Path.Combine(RepoPaths.Root(),"icons","picture-converted.tsv"));
			Assert.IsTrue(listRows.Count>0,"icons/picture-converted.tsv is empty.");
			List<string> listSlices=GetSlices();
			foreach(ConvertedRow row in listRows) {
				Assert.IsTrue(listSlices.Contains(row.Slice),row+": the slice is not in icons/picture-slices.tsv.");
				string text=File.ReadAllText(Path.Combine(RepoPaths.Root(),row.File.Replace('/','\\')));
				Match matchTarget=Regex.Match(row.Target,@"^EnumIcons\.(?<m>\w+)$");
				Assert.IsTrue(matchTarget.Success,row+": the record's target is no EnumIcons member.");
				string member=matchTarget.Groups["m"].Value;
				Assert.IsTrue(Enum.IsDefined(typeof(EnumIcons),member),row+": EnumIcons has no "+member+".");
				Assert.IsTrue(IconLibrary.GetDesigns((EnumIcons)Enum.Parse(typeof(EnumIcons),member)).Length>0,row+": no design.");
				string line;
				if(row.How=="Icon") {
					Assert.AreEqual("Image",row.Property,row.ToString());
					Assert.AreEqual(22,row.Width,row+": an Icon replaces a 22 px picture (UI.Button's slot).");
					Assert.AreEqual(22,row.Height,row.ToString());
					line="this."+row.Control+".Icon = OpenPmsDental.UI.EnumIcons."+member+";";
				}
				else {
					Assert.AreEqual("GetBitmap",row.How,row.ToString());
					line="this."+row.Control+"."+row.Property+" = OpenPmsDental.UI.IconLibrary.GetBitmap(OpenPmsDental.UI.EnumIcons."+member+","+row.Width+","+row.Height+");";
				}
				Assert.IsTrue(text.Contains(line) || text.Contains(line.Substring("this.".Length)),row+": the file has no line "+line);//a code line (FormPayment.cs, FormQueryParser.cs) may omit this.
				Assert.IsFalse(Regex.IsMatch(text,@"\bthis\."+Regex.Escape(row.Control)+@"\.(Image|BackgroundImage)\s*=\s*(\(\(System\.Drawing\.Image\)|global::OpenPmsDental\.Properties)"),
					row+": the control still has a picture line.");
			}
		}

		[TestMethod]
		public void Designers_EveryPictureTheyReadIsStillThere() {
			string root=RepoPaths.Root();
			string folderProgram=Path.Combine(root,"OpenPmsDental");
			HashSet<string> hashSetResources=GetResxNames(Path.Combine(folderProgram,"Properties","Resources.resx"));
			Regex regexResources=new Regex(@"(?<![\w.])(?:global::)?(?:OpenPmsDental\.)?Properties\.Resources\.(?<n>\w+)");
			Regex regexGetObject=new Regex("resources\\.GetObject\\(\"(?<n>[^\"]+)\"\\)");
			List<string> listMissing=new List<string>();
			int countGetObject=0;
			foreach(string path in Directory.GetFiles(folderProgram,"*.cs",SearchOption.AllDirectories)) {
				if(path.Contains("\\bin\\") || path.Contains("\\obj\\")) {
					continue;
				}
				string text=File.ReadAllText(path);
				foreach(Match match in regexResources.Matches(text)) {
					string name=match.Groups["n"].Value;
					if(name=="ResourceManager" || name=="Culture") {
						continue;
					}
					if(!hashSetResources.Contains(name)) {
						listMissing.Add(path.Substring(root.Length)+": Properties.Resources."+name);
					}
				}
				MatchCollection matchCollection=regexGetObject.Matches(text);
				if(matchCollection.Count==0) {
					continue;
				}
				string pathResx=Path.Combine(Path.GetDirectoryName(path),Path.GetFileName(path).Split('.')[0]+".resx");
				HashSet<string> hashSetResx=File.Exists(pathResx) ? GetResxNames(pathResx) : new HashSet<string>();
				foreach(Match match in matchCollection) {
					countGetObject++;
					if(!hashSetResx.Contains(match.Groups["n"].Value)) {
						listMissing.Add(path.Substring(root.Length)+": resources.GetObject(\""+match.Groups["n"].Value+"\") has no entry in "+Path.GetFileName(pathResx));
					}
				}
			}
			Assert.IsTrue(countGetObject>0,"No designer reads a .resx entry: the scan found nothing.");
			Assert.AreEqual(0,listMissing.Count,"Pictures the code reads that are gone:\r\n"+string.Join("\r\n",listMissing));
		}

		[TestMethod]
		public void GetBitmap_HasThePicturesSize_TheIconCentredInItsRoleColour() {
			foreach(Size size in new[] { new Size(22,22),new Size(16,16),new Size(17,16),new Size(13,15),new Size(20,23),new Size(18,16) }) {
				using(Bitmap bitmap=IconLibrary.GetBitmap(EnumIcons.Print,size.Width,size.Height)) {
					Assert.AreEqual(size,bitmap.Size);
					Assert.AreEqual(96f,bitmap.HorizontalResolution,0.01f);
					Assert.AreEqual(PixelFormat.Format32bppArgb,bitmap.PixelFormat);
					Assert.IsTrue(CountInk(bitmap)>size.Width,"Print draws nothing at "+size);
					Color colorRole=IconLibrary.GetColor(EnumIcons.Print);
					for(int y=0;y<bitmap.Height;y++) {
						for(int x=0;x<bitmap.Width;x++) {
							Color color=bitmap.GetPixel(x,y);
							if(color.A<128) {
								continue;//an antialiased edge: its colour is rounded through the alpha
							}
							Assert.IsTrue(Math.Abs(color.R-colorRole.R)<=4 && Math.Abs(color.G-colorRole.G)<=4 && Math.Abs(color.B-colorRole.B)<=4,
								"Print at "+size+": a pixel of "+color+", not the role colour "+colorRole+".");
						}
					}
				}
			}
			//Centred in the largest square: a 20 x 23 bitmap draws the 20 px design from row 1.
			using(Bitmap bitmap=IconLibrary.GetBitmap(EnumIcons.Pin,20,23)) {
				for(int x=0;x<20;x++) {
					Assert.AreEqual(0,bitmap.GetPixel(x,0).A,"Row 0 is above the centred 20 px slot.");
					Assert.AreEqual(0,bitmap.GetPixel(x,22).A,"Row 22 is below the centred 20 px slot.");
				}
			}
			using(Bitmap bitmap=IconLibrary.GetBitmap(EnumIcons.None,16,16)) {
				Assert.AreEqual(0,CountInk(bitmap),"None draws nothing.");
			}
			using(Bitmap bitmap=IconLibrary.GetBitmap(DentalIcon.Tooth,20,20)) {
				Assert.IsTrue(CountInk(bitmap)>20,"A dental icon draws.");
			}
		}

		///<summary>Windows the off-screen builder cannot make because their constructors need objects it cannot give (an X835 claim); a
		///NullReferenceException from any other window still fails the test.</summary>
		private static readonly string[] _arrayWindowsNeedingArguments=new[] { "FormEtrans835ClaimEdit","FormEtrans835ProcEdit","FormMountDefEdit","FormSheetDefEdit" };

		[TestMethod]
		public void BuiltWindows_ShowTheIconOrABitmapOfThePicturesSize() {
			List<ConvertedRow> listRows=ReadRecord(Path.Combine(RepoPaths.Root(),"icons","picture-converted.tsv"));
			Assert.IsTrue(listRows.Count>0,"icons/picture-converted.tsv is empty.");
			List<string> listProblems=new List<string>();
			int countBuilt=0;
			StaRunner.Run(() => {
				NoDatabaseGuard.Install();
				DefaultFontOverride.Set(TextFitFontOption.Today,1f);
				List<ScanType> listScanTypes=WindowBuilder.GetScanTypes();
				foreach(IGrouping<string,ConvertedRow> grouping in listRows.GroupBy(x => x.WindowName)) {
					ScanType scanType=listScanTypes.SingleOrDefault(x => x.Assembly=="OpenPmsDental" && x.Type.Name==grouping.Key);
					if(scanType==null) {
						listProblems.Add(grouping.Key+": not a window of the scan.");
						continue;
					}
					BuiltWindow builtWindow;
					try {
						builtWindow=WindowBuilder.Build(scanType);
					}
					catch(Exception exception) {
						if(NoDatabaseGuard.IsDatabaseException(exception) || exception.Message.EndsWith("is an invalid pref name.")) {
							continue;//needs a database (or the preference cache it fills: FormRecallList, FormSchedule read EasyNoClinics): the designer lines are checked by Record_EveryConversionIsInItsDesignerLine
						}
						if(exception is NullReferenceException && _arrayWindowsNeedingArguments.Contains(grouping.Key)) {
							continue;//a window whose constructor arguments the builder cannot give (an X835 claim): the designer lines are checked by Record_EveryConversionIsInItsDesignerLine
						}
						listProblems.Add(grouping.Key+": "+exception.GetType().Name+" "+exception.Message);
						continue;
					}
					using(builtWindow) {
						countBuilt++;
						foreach(ConvertedRow row in grouping) {
							Control control=GetField(builtWindow.Root,row.Control) as Control;
							if(control==null) {
								listProblems.Add(row+": no control.");
								continue;
							}
							EnumIcons icon=(EnumIcons)Enum.Parse(typeof(EnumIcons),row.Target.Substring("EnumIcons.".Length));
							Image image=row.Property=="BackgroundImage" ? control.BackgroundImage : (control as System.Windows.Forms.ButtonBase)?.Image;
							if(row.How=="Icon") {
								OpenPmsDental.UI.Button button=control as OpenPmsDental.UI.Button;
								if(button==null || button.Icon!=icon || button.Image!=null) {
									listProblems.Add(row+": not a UI.Button showing the icon.");
								}
								continue;
							}
							if(image==null || image.Width!=row.Width || image.Height!=row.Height) {
								listProblems.Add(row+": the picture is "+(image==null ? "missing" : image.Width+"x"+image.Height)+".");
								continue;
							}
							if(CountInk((Bitmap)image)==0) {
								listProblems.Add(row+": the bitmap is empty.");
							}
						}
					}
				}
			});
			Assert.AreEqual(0,listProblems.Count,string.Join("\r\n",listProblems));
			Assert.IsTrue(countBuilt>=listRows.Select(x => x.WindowName).Distinct().Count()/2,"Most converted windows build without a database: "+countBuilt);
		}

		///<summary>The G-UI3 slice's pictures (opt-in): each converted window off screen (windows\<window>.png) and each converted control
		///(controls\<window>.<control>.png), from the record (or OPMS_UI9A_RECORD). The same test at the commit before the conversion, with the
		///tool's planned record, makes the before set; tools/New-UiGallery.ps1 pairs them by name.</summary>
		[TestMethod]
		public void Gallery_ConvertedWindowsAndControls() {
			string folder=Environment.GetEnvironmentVariable(VARIABLE_PICTURES);
			if(string.IsNullOrWhiteSpace(folder)) {
				Assert.Inconclusive("Opt-in: "+VARIABLE_PICTURES+" names the folder for the G-UI3 pictures of UI-9a.");
			}
			string pathRecord=Environment.GetEnvironmentVariable(VARIABLE_RECORD);
			if(string.IsNullOrWhiteSpace(pathRecord)) {
				pathRecord=Path.Combine(RepoPaths.Root(),"icons","picture-converted.tsv");
			}
			List<ConvertedRow> listRows=ReadRecord(pathRecord);
			Assert.IsTrue(listRows.Count>0,pathRecord+" is empty.");
			List<string> listNotes=new List<string>();
			int countPictures=0;
			StaRunner.Run(() => {
				NoDatabaseGuard.Install();
				DefaultFontOverride.Set(TextFitFontOption.Today,1f);
				List<ScanType> listScanTypes=WindowBuilder.GetScanTypes();
				foreach(IGrouping<string,ConvertedRow> grouping in listRows.GroupBy(x => x.WindowName).OrderBy(x => x.Key)) {
					ScanType scanType=listScanTypes.SingleOrDefault(x => x.Assembly=="OpenPmsDental" && x.Type.Name==grouping.Key);
					if(scanType==null) {
						listNotes.Add(grouping.Key+"\tnot a window of the scan");
						continue;
					}
					try {
						using(BuiltWindow builtWindow=WindowBuilder.Build(scanType)) {
							Control controlWindow=builtWindow.Root;
							if(builtWindow.Form is FormODBase formODBase && formODBase.PanelClient!=null && builtWindow.Root==builtWindow.Form) {
								controlWindow=formODBase.PanelClient;//the content; the frame is UI-3's and is drawn by OnLoad
							}
							using(Bitmap bitmap=DesignSheetWindows.Capture(controlWindow,Rectangle.Empty)) {
								Save(bitmap,Path.Combine(folder,"windows",grouping.Key+".png"));
								countPictures++;
							}
							foreach(ConvertedRow row in grouping) {
								Control control=GetField(builtWindow.Root,row.Control) as Control;
								if(control==null) {
									listNotes.Add(grouping.Key+"."+row.Control+"\tno control");
									continue;
								}
								using(Bitmap bitmap=DesignSheetWindows.Capture(control,Rectangle.Empty)) {
									Save(bitmap,Path.Combine(folder,"controls",grouping.Key+"."+row.Control+".png"));
									countPictures++;
								}
							}
						}
					}
					catch(Exception exception) {
						listNotes.Add(grouping.Key+"\tnot pictured: "+exception.GetType().Name+" "+exception.Message.Replace("\r"," ").Replace("\n"," "));
					}
				}
			});
			File.WriteAllLines(Path.Combine(folder,"ui9a-notes.tsv"),listNotes);
			Assert.IsTrue(countPictures>0,"No picture written.");
		}

		private static void Save(Bitmap bitmap,string path) {
			Directory.CreateDirectory(Path.GetDirectoryName(path));
			bitmap.Save(path,ImageFormat.Png);
		}
	}
}
