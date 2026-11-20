// The UI-9a tests (docs/specs/ui-refresh-plan.md section 7's UI-9a row, docs/port-notes/UI-9a.md): the bitmap pictures of the converted slices
// (icons/picture-slices.tsv) turned into icons by tools/Convert-PictureUses.ps1 from icons/picture-map.tsv. The map has a row for every picture
// use the tool finds in a converted slice and the slice has nothing left to convert; every designer GetObject and Properties.Resources picture
// still has its resource; each recorded conversion (icons/picture-converted.tsv) is in its designer line and, in the built window, the control
// shows the icon (UI.Button.Icon) or a bitmap of the old picture's size (IconLibrary.GetBitmap), so nothing moves. The gallery G-UI3 slice is
// opt-in: OPMS_UI9A_PICTURES names the folder for each converted window's picture and each converted control's picture, off screen (UI-0b's
// builder); OPMS_UI9A_RECORD may name another record (the tool's -Check -Record output, for the before run on the commit before the conversion),
// OPMS_UI9A_SLICE limits the set to the slices that start with its text. UI-9a-6 adds the shared projects (OpenPmsDentalGraph and
// OpenPmsDentalBusiness, converted by hand to CodeBase's UiIcons.GetBitmap, recorded with how "UiIcons") and the 11 WPF buttons whose
// BitmapFileName pictures became icons (pictured when the session renders WPF). No database, no window shown.
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
		///<summary>Optional for the gallery: only the record rows whose slice starts with this text (UI-9a-6), so a slice's set holds its own windows.</summary>
		public const string VARIABLE_SLICE="OPMS_UI9A_SLICE";

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

		///<summary>The assembly of a recorded file: its project folder (OpenPmsDental, OpenPmsDentalGraph, OpenPmsDentalBusiness, CentralManager, CodeBase, ODR; RdlDesign's assembly is RdlDesigner).</summary>
		private static string GetAssembly(ConvertedRow row) {
			string project=row.File.Split('/')[0];
			return project=="RdlDesign" ? "RdlDesigner" : project;//the report designer's assembly is named RdlDesigner
		}

		///<summary>The picture of a converted control: its BackgroundImage, or its public Image property (a button, label, PictureBox or ODPictureBox).</summary>
		private static Image GetImage(Control control,string property) {
			if(property=="BackgroundImage") {
				return control.BackgroundImage;
			}
			PropertyInfo propertyInfo=control.GetType().GetProperty("Image",BindingFlags.Instance|BindingFlags.Public,null,typeof(Image),Type.EmptyTypes,null);
			return propertyInfo?.GetValue(control) as Image;
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

		///<summary>The UI-9a-7 slices of the projects that have no picture on main (the slice confirms it: ServiceManager, MySQLManager, xCrudGenerator,
		///OpenPmsDentalDbSettings, OpenPmsDentalHL7, DatabaseIntegrityCheck, DocumentationBuilder, UpdateFileCopier, OpenPmsDentalHelper32).</summary>
		private static readonly string[] _arraySlicesWithoutPictures=new[] { "UI-9a-7c","UI-9a-7d","UI-9a-7e","UI-9a-7f","UI-9a-7g","UI-9a-7h","UI-9a-7i","UI-9a-7j","UI-9a-7k" };

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
					if(_arraySlicesWithoutPictures.Contains(slice)) {
						Assert.AreEqual(0,listUses.Count,slice+": a project that had no picture on main has one now; give it map rows.");
						continue;
					}
					Assert.IsTrue(listUses.Count>0,slice+": the tool found no picture use.");
					List<string> listUnmapped=listUses.Where(x => x[2]=="").Select(x => x[1]+" ("+x[3]+":"+x[4]+")").ToList();
					Assert.AreEqual(0,listUnmapped.Count,slice+": picture uses with no row in icons/picture-map.tsv:\r\n"+string.Join("\r\n",listUnmapped));
					//Nothing left to convert: every use is a keep, a UI-9b ImageList or a recorded conversion.
					List<string> listLeft=listUses.Where(x => (x[0]=="Resources" || x[0]=="Resx") && x[2]!="keep" && x[2]!="UI-9b")
						.Select(x => x[1]+" -> "+x[2]+" ("+x[3]+":"+x[4]+")").ToList();
					Assert.AreEqual(0,listLeft.Count,slice+": pictures still to convert (run the tool with -Apply):\r\n"+string.Join("\r\n",listLeft));
					Assert.AreEqual(0,exitCode,slice+": the tool's -Check reports changes or unmapped uses:\r\n"+output);
					//A slice converts something, or (UI-9a-6's folders of ImageLists and kept pictures) everything it has is a keep or an ImageList.
					Assert.IsTrue(listUses.Any(x => x[0]=="Converted") || listUses.All(x => x[2]=="keep" || x[2]=="UI-9b"),
						slice+": no recorded conversion in icons/picture-converted.tsv.");
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
				else if(row.How=="UiIcons") {
					//OpenPmsDentalGraph and OpenPmsDentalBusiness cannot reach OpenPmsDental.UI.IconLibrary: CodeBase's renderer by enum and member name.
					Assert.IsFalse(row.File.StartsWith("OpenPmsDental/"),row+": the program's own files use IconLibrary.GetBitmap.");
					line="this."+row.Control+"."+row.Property+" = global::CodeBase.UiIcons.GetBitmap(\"EnumIcons\",\""+member+"\","+row.Width+","+row.Height+");";
				}
				else {
					Assert.AreEqual("GetBitmap",row.How,row.ToString());
					line="this."+row.Control+"."+row.Property+" = OpenPmsDental.UI.IconLibrary.GetBitmap(OpenPmsDental.UI.EnumIcons."+member+","+row.Width+","+row.Height+");";
				}
				Assert.IsTrue(text.Contains(line) || text.Contains(line.Substring("this.".Length)),row+": the file has no line "+line);//a code line (FormPayment.cs, FormQueryParser.cs) may omit this.
				Assert.IsFalse(Regex.IsMatch(text,@"\bthis\."+Regex.Escape(row.Control)+@"\.(Image|BackgroundImage)\s*=\s*(\(\(System\.Drawing\.Image\)|global::OpenPmsDental\w*\.Properties)"),
					row+": the control still has a picture line.");
			}
		}

		[TestMethod]
		public void Designers_EveryPictureTheyReadIsStillThere() {
			string root=RepoPaths.Root();
			List<string> listMissing=new List<string>();
			int countGetObject=0;
			//The program, and since UI-9a-6 the shared projects whose pictures the slice converted or kept, each with its own Resources (the
			//namespace its code names them by). Their old conversion code (ConvertDatabases) names pictures in comments, so comments are left out there.
			countGetObject+=CheckPicturesOfProject(root,"OpenPmsDental","OpenPmsDental",false,listMissing);
			countGetObject+=CheckPicturesOfProject(root,"OpenPmsDentalBusiness","OpenPmsDentalBusiness",true,listMissing);
			countGetObject+=CheckPicturesOfProject(root,"OpenPmsDentalGraph","OpenPmsDentalGraph",true,listMissing);
			countGetObject+=CheckPicturesOfProject(root,"SparksToothChart","SparksToothChart",true,listMissing);
			countGetObject+=CheckPicturesOfProject(root,"WpfControlsOD","WpfControls",true,listMissing);
			Assert.IsTrue(countGetObject>0,"No designer reads a .resx entry: the scan found nothing.");
			Assert.AreEqual(0,listMissing.Count,"Pictures the code reads that are gone:\r\n"+string.Join("\r\n",listMissing));
		}

		///<summary>Adds to the list each Properties.Resources name the project's code reads that its Properties/Resources.resx lacks, and each designer
		///resources.GetObject entry its window's .resx lacks. Returns the number of GetObject reads.</summary>
		private static int CheckPicturesOfProject(string root,string project,string namespaceProject,bool isCommentsLeftOut,List<string> listMissing) {
			string folderProject=Path.Combine(root,project);
			HashSet<string> hashSetResources=GetResxNames(Path.Combine(folderProject,"Properties","Resources.resx"));
			Regex regexResources=new Regex(@"(?<![\w.])(?:global::)?(?:"+Regex.Escape(namespaceProject)+@"\.)?Properties\.Resources\.(?<n>\w+)");
			Regex regexGetObject=new Regex("resources\\.GetObject\\(\"(?<n>[^\"]+)\"\\)");
			int countGetObject=0;
			foreach(string path in Directory.GetFiles(folderProject,"*.cs",SearchOption.AllDirectories)) {
				if(path.Contains("\\bin\\") || path.Contains("\\obj\\")) {
					continue;
				}
				string text=File.ReadAllText(path);
				if(isCommentsLeftOut) {
					text=Regex.Replace(text,@"/\*[\s\S]*?\*/|//[^\r\n]*","");
				}
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
			return countGetObject;
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
			//CodeBase's renderer by name (UI-9a-6: OpenPmsDentalGraph and OpenPmsDentalBusiness): the same bitmap as IconLibrary's for the same member.
			foreach(Size size in new[] { new Size(16,16),new Size(18,18),new Size(10,10),new Size(13,15) }) {
				using(Bitmap bitmap=UiIcons.GetBitmap("EnumIcons","Preview",size.Width,size.Height))
				using(Bitmap bitmapProgram=IconLibrary.GetBitmap(EnumIcons.Preview,size.Width,size.Height)) {
					Assert.AreEqual(size,bitmap.Size);
					Assert.AreEqual(96f,bitmap.HorizontalResolution,0.01f);
					Assert.AreEqual(PixelFormat.Format32bppArgb,bitmap.PixelFormat);
					Assert.IsTrue(CountInk(bitmap)>0,"Preview draws nothing at "+size);
					for(int y=0;y<size.Height;y++) {
						for(int x=0;x<size.Width;x++) {
							Assert.AreEqual(bitmapProgram.GetPixel(x,y).ToArgb(),bitmap.GetPixel(x,y).ToArgb(),"UiIcons.GetBitmap and IconLibrary.GetBitmap differ at "+x+","+y+" ("+size+").");
						}
					}
				}
			}
			using(Bitmap bitmap=UiIcons.GetBitmap("EnumIcons","NoSuchMember",16,16)) {
				Assert.AreEqual(0,CountInk(bitmap),"A name with no manifest row draws nothing.");
			}
		}

		///<summary>RdlDesign references no CodeBase: it compiles in CodeBase's icon table and GDI+ renderer as linked files (ReportDesigner.csproj).
		///That linked copy draws each toolbar icon of UI-9a-7 at today's picture's pixel size, the same pixels as CodeBase's own renderer, and the
		///designer's hidden buttons carry those bitmaps.</summary>
		[TestMethod]
		public void RdlDesign_LinkedRendererDrawsItsToolbarIcons() {
			string root=RepoPaths.Root();
			string textProject=File.ReadAllText(Path.Combine(root,"RdlDesign","ReportDesigner.csproj"));
			foreach(string file in new[] { "UiIcons.cs","UiIcons.Gdi.cs","IconData.g.cs" }) {
				Assert.IsTrue(textProject.Contains("<Compile Include=\"..\\CodeBase\\UI\\Icons\\"+file+"\""),"ReportDesigner.csproj does not link "+file+".");
			}
			List<ConvertedRow> listRows=ReadRecord(Path.Combine(root,"icons","picture-converted.tsv")).Where(x => x.File.StartsWith("RdlDesign/")).ToList();
			Assert.AreEqual(21,listRows.Count,"The 21 toolbar pictures of the report designer (RdlDesigner.cs).");
			Assert.IsTrue(listRows.All(x => x.How=="UiIcons"),"RdlDesign takes its icons from the linked renderer (UiIcons).");
			Assembly assembly=WindowBuilder.LoadPlainExe("RdlDesign","RdlDesigner");
			Type typeIcons=assembly.GetType("CodeBase.UiIcons");
			Assert.IsNotNull(typeIcons,"RdlDesigner.dll has no CodeBase.UiIcons: the renderer is not linked.");
			MethodInfo methodInfo=typeIcons.GetMethod("GetBitmap",BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic,null,
				new[] { typeof(string),typeof(string),typeof(int),typeof(int) },null);
			Assert.IsNotNull(methodInfo,"The linked renderer has no GetBitmap(enum name, member, width, height).");
			foreach(ConvertedRow row in listRows) {
				string member=row.Target.Substring("EnumIcons.".Length);
				using(Bitmap bitmapLinked=(Bitmap)methodInfo.Invoke(null,new object[] { "EnumIcons",member,row.Width,row.Height }))
				using(Bitmap bitmapCodeBase=UiIcons.GetBitmap("EnumIcons",member,row.Width,row.Height)) {
					Assert.AreEqual(new Size(row.Width,row.Height),bitmapLinked.Size,row.ToString());
					Assert.IsTrue(CountInk(bitmapLinked)>0,row+": the linked renderer draws nothing.");
					for(int y=0;y<row.Height;y++) {
						for(int x=0;x<row.Width;x++) {
							Assert.AreEqual(bitmapCodeBase.GetPixel(x,y).ToArgb(),bitmapLinked.GetPixel(x,y).ToArgb(),row+": the linked renderer differs from CodeBase's at "+x+","+y+".");
						}
					}
				}
			}
		}

		///<summary>Windows the off-screen builder cannot make because their constructors need objects it cannot give (an X835 claim); a
		///NullReferenceException from any other window still fails the test.</summary>
		private static readonly string[] _arrayWindowsNeedingArguments=new[] { "FormEtrans835ClaimEdit","FormEtrans835ProcEdit","FormMountDefEdit","FormSheetDefEdit" };
		///<summary>Controls that add child controls in code while they are built, which LayoutManager allows only in their host (the Security window):
		///the builder cannot make them on their own. Their designer lines are checked by Record_EveryConversionIsInItsDesignerLine.</summary>
		private static readonly string[] _arrayWindowsAddingControlsInCode=new[] { "UserControlSecurityUserGroup" };

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
					ScanType scanType=listScanTypes.SingleOrDefault(x => x.Assembly==GetAssembly(grouping.First()) && x.Type.Name==grouping.Key);
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
						if(exception is ApplicationException && exception.Message.StartsWith("Controls cannot be added programmatically.")
							&& _arrayWindowsAddingControlsInCode.Contains(grouping.Key))
						{
							continue;//LayoutManager's rule for controls added in code, outside the host window: the designer lines are checked by Record_EveryConversionIsInItsDesignerLine
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
							Image image=GetImage(control,row.Property);
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
			string slicePrefix=Environment.GetEnvironmentVariable(VARIABLE_SLICE);
			if(!string.IsNullOrWhiteSpace(slicePrefix)) {
				listRows=listRows.Where(x => x.Slice.StartsWith(slicePrefix.Trim(),StringComparison.Ordinal)).ToList();
			}
			Assert.IsTrue(listRows.Count>0,pathRecord+" is empty"+(string.IsNullOrWhiteSpace(slicePrefix) ? "." : " for the slices "+slicePrefix+"."));
			List<string> listNotes=new List<string>();
			int countPictures=0;
			StaRunner.Run(() => {
				NoDatabaseGuard.Install();
				DefaultFontOverride.Set(TextFitFontOption.Today,1f);
				List<ScanType> listScanTypes=WindowBuilder.GetScanTypes();
				foreach(IGrouping<string,ConvertedRow> grouping in listRows.GroupBy(x => x.WindowName).OrderBy(x => x.Key)) {
					ScanType scanType=listScanTypes.SingleOrDefault(x => x.Assembly==GetAssembly(grouping.First()) && x.Type.Name==grouping.Key);
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
								if(control.Width<=0 || control.Height<=0) {
									//Laid out to nothing until it is needed (the signature box's buttons): its picture alone, on the control colour.
									Image image=GetImage(control,row.Property);
									if(image==null) {
										listNotes.Add(grouping.Key+"."+row.Control+"\tnot pictured: "+control.Width+" x "+control.Height+" in the built window, no picture");
										continue;
									}
									using(Bitmap bitmap=new Bitmap(image.Width,image.Height,PixelFormat.Format24bppRgb))
									using(Graphics g=Graphics.FromImage(bitmap)) {
										g.Clear(SystemColors.Control);
										g.DrawImage(image,new Rectangle(0,0,image.Width,image.Height));
										Save(bitmap,Path.Combine(folder,"controls",grouping.Key+"."+row.Control+".png"));
										countPictures++;
									}
									listNotes.Add(grouping.Key+"."+row.Control+"\t"+control.Width+" x "+control.Height+" in the built window: its picture alone");
									continue;
								}
								try {
									using(Bitmap bitmap=DesignSheetWindows.Capture(control,Rectangle.Empty)) {
										Save(bitmap,Path.Combine(folder,"controls",grouping.Key+"."+row.Control+".png"));
										countPictures++;
									}
								}
								catch(Exception exception) {
									listNotes.Add(grouping.Key+"."+row.Control+"\tnot pictured: "+exception.GetType().Name+" "+exception.Message.Replace("\r"," ").Replace("\n"," "));
								}
							}
						}
					}
					catch(Exception exception) {
						listNotes.Add(grouping.Key+"\tnot pictured: "+exception.GetType().Name+" "+exception.Message.Replace("\r"," ").Replace("\n"," "));
					}
				}
			});
			//The WPF buttons of UI-9a-6 (their Frms are not built off screen): each button as its XAML sets it, today's picture (BitmapFileName) or the
			//icon, when this session renders WPF (a locked or disconnected session draws nothing: the port note's steps for the lead).
			if(string.IsNullOrWhiteSpace(slicePrefix) || "UI-9a-6".StartsWith(slicePrefix.Trim(),StringComparison.Ordinal)
				|| slicePrefix.Trim().StartsWith("UI-9a-6",StringComparison.Ordinal))
			{
				StaRunner.Run(() => {
					if(!ControlPictures.RendersWpf()) {
						listNotes.Add("wpf\tnot pictured: this session does not render WPF (locked or disconnected)");
						return;
					}
					_=System.IO.Packaging.PackUriHelper.UriSchemePack;//registers pack:// for BitmapFileName
					foreach(string[] arrayButton in ArrayWpfButtons) {
						string name=Path.GetFileNameWithoutExtension(arrayButton[0])+"."+arrayButton[1];
						try {
							WpfControls.UI.Button button=MakeWpfButton(arrayButton[0],arrayButton[1],out double width,out double height);
							using(Bitmap bitmap=ControlPictures.RenderWpf(button,width,height,1f)) {
								Save(bitmap,Path.Combine(folder,"wpf",name+".png"));
								countPictures++;
							}
						}
						catch(Exception exception) {
							listNotes.Add(name+"\tnot pictured: "+exception.GetType().Name+" "+exception.Message.Replace("\r"," ").Replace("\n"," "));
						}
					}
				});
			}
			File.WriteAllLines(Path.Combine(folder,"ui9a-notes.tsv"),listNotes);
			Assert.IsTrue(countPictures>0,"No picture written.");
		}

		///<summary>The WPF button of a Frm as its XAML sets it (Width, Height, Text, Icon or BitmapFileName, ImageAlign), on its own.</summary>
		private static WpfControls.UI.Button MakeWpfButton(string fileXaml,string nameButton,out double width,out double height) {
			string element=GetWpfButtonElement(fileXaml,nameButton);
			Assert.IsNotNull(element,fileXaml+" has no ui:Button "+nameButton+".");
			WpfControls.UI.Button button=new WpfControls.UI.Button();
			width=double.Parse(GetXamlAttribute(element,"Width"),System.Globalization.CultureInfo.InvariantCulture);
			height=double.Parse(GetXamlAttribute(element,"Height"),System.Globalization.CultureInfo.InvariantCulture);
			button.Width=width;
			button.Height=height;
			button.Text=GetXamlAttribute(element,"Text")??"";
			string icon=GetXamlAttribute(element,"Icon");
			if(icon!=null) {
				button.Icon=(WpfControls.UI.EnumIcons)Enum.Parse(typeof(WpfControls.UI.EnumIcons),icon);
			}
			string bitmapFileName=GetXamlAttribute(element,"BitmapFileName");
			if(bitmapFileName!=null) {
				button.BitmapFileName=bitmapFileName;
			}
			return button;
		}

		///<summary>The start tag of the ui:Button with the x:Name in the XAML file (repository-relative), or null.</summary>
		private static string GetWpfButtonElement(string fileXaml,string nameButton) {
			string text=File.ReadAllText(Path.Combine(RepoPaths.Root(),fileXaml.Replace('/','\\')));
			Match match=Regex.Match(text,"<ui:Button\\b[^>]*\\bx:Name=\""+Regex.Escape(nameButton)+"\"[^>]*>");
			return match.Success ? match.Value : null;
		}

		private static string GetXamlAttribute(string element,string attribute) {
			Match match=Regex.Match(element,"\\s"+Regex.Escape(attribute)+"=\"(?<v>[^\"]*)\"");
			return match.Success ? match.Groups["v"].Value : null;
		}

		///<summary>The 11 WPF buttons whose BitmapFileName pictures UI-9a-6 turned into icons: XAML file, button, WPF EnumIcons member.</summary>
		public static readonly string[][] ArrayWpfButtons=new string[][] {
			new[] { "WpfControlsOD/Frms/FrmFriendlyException.xaml","butPrintOD","Print" },
			new[] { "WpfControlsOD/Frms/FrmMountLayoutEdit.xaml","butUp","Up" },
			new[] { "WpfControlsOD/Frms/FrmMountLayoutEdit.xaml","butDown","Down" },
			new[] { "WpfControlsOD/Frms/FrmQuickPaste.xaml","butEditNote","Edit" },
			new[] { "WpfControlsOD/Frms/FrmQuickPaste.xaml","butUpNote","Up" },
			new[] { "WpfControlsOD/Frms/FrmQuickPaste.xaml","butDownNote","Down" },
			new[] { "WpfControlsOD/Frms/FrmQuickPaste.xaml","butUpCat","Up" },
			new[] { "WpfControlsOD/Frms/FrmQuickPaste.xaml","butDownCat","Down" },
			new[] { "WpfControlsOD/Frms/FrmReferralsPatient.xaml","butUp","Up" },
			new[] { "WpfControlsOD/Frms/FrmReferralsPatient.xaml","butDown","Down" },
			new[] { "WpfControlsOD/Frms/FrmZipSelect.xaml","butEdit","Edit" },
		};

		[TestMethod]
		public void Wpf_ConvertedButtonsShowTheirIcons_NoBitmapFileNameLeft() {
			string root=RepoPaths.Root();
			HashSet<string> hashSetWpf=new HashSet<string>(Enum.GetNames(typeof(WpfControls.UI.EnumIcons)));
			foreach(string[] arrayButton in ArrayWpfButtons) {
				string element=GetWpfButtonElement(arrayButton[0],arrayButton[1]);
				Assert.IsNotNull(element,arrayButton[0]+" has no ui:Button "+arrayButton[1]+".");
				Assert.AreEqual(arrayButton[2],GetXamlAttribute(element,"Icon"),arrayButton[0]+" "+arrayButton[1]+": the icon.");
				Assert.IsNull(GetXamlAttribute(element,"BitmapFileName"),arrayButton[0]+" "+arrayButton[1]+" still has a BitmapFileName.");
				Assert.IsTrue(hashSetWpf.Contains(arrayButton[2]),"The WPF EnumIcons has no "+arrayButton[2]+".");
				Assert.IsTrue(WpfControls.UI.IconLibrary.GetDesigns((WpfControls.UI.EnumIcons)Enum.Parse(typeof(WpfControls.UI.EnumIcons),arrayButton[2])).Length>0,
					arrayButton[2]+": no design.");
			}
			//No XAML of the program sets a picture by file any more (the BitmapFileName property stays, for a picture that is not an icon).
			List<string> listLeft=new List<string>();
			foreach(string project in new[] { "WpfControlsOD","OpenPmsDental" }) {
				foreach(string path in Directory.GetFiles(Path.Combine(root,project),"*.xaml",SearchOption.AllDirectories)) {
					if(path.Contains("\\bin\\") || path.Contains("\\obj\\")) {
						continue;
					}
					foreach(Match match in Regex.Matches(File.ReadAllText(path),"\\bBitmapFileName=\"[^\"]+\"")) {
						listLeft.Add(path.Substring(root.Length)+": "+match.Value);
					}
				}
			}
			Assert.AreEqual(0,listLeft.Count,"BitmapFileName pictures left:\r\n"+string.Join("\r\n",listLeft));
			//WpfConverter writes the date range picker's week buttons with icons the WPF enum has.
			string textConverter=File.ReadAllText(Path.Combine(root,"OpenPmsDental","UIManagement","WpfConverter.cs"));
			Assert.IsFalse(textConverter.Contains("BitmapFileName=\\\""),"WpfConverter still writes a BitmapFileName.");
			MatchCollection matchCollection=Regex.Matches(textConverter,"Icon=\\\\\"(?<m>\\w+)\\\\\"");
			Assert.IsTrue(matchCollection.Count>=2,"WpfConverter writes the week buttons' icons.");
			foreach(Match match in matchCollection) {
				Assert.IsTrue(hashSetWpf.Contains(match.Groups["m"].Value),"WpfConverter writes Icon "+match.Groups["m"].Value+", which the WPF EnumIcons does not have.");
			}
		}

		private static void Save(Bitmap bitmap,string path) {
			Directory.CreateDirectory(Path.GetDirectoryName(path));
			bitmap.Save(path,ImageFormat.Png);
		}
	}
}
