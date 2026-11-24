// Group U's own pieces (package UI-0c of docs/specs/ui-refresh-plan.md), checked without the opt-in, without starting the program, without a
// database and without showing a window: windows.tsv against the program's source (every window type exists, every menu path is the main
// menu's, every module handler, button and grid opens the window the row names, every guard table exists), the walker on a test form that is
// built and never shown (UI-0b's shared measurer and bounds record, the font option, the date and clock masks, the files it writes), and U0's
// comparison with synthetic pictures and approved.tsv rows (UI-0a's masking and hashing rule).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UiGalleryTools;
using UiRefreshTests.TextFit;
using WindowChecksTests.InProc;

namespace WindowChecksTests {
	[TestClass]
	public class UiHarnessTests {
		#region Helpers
		private static string NewTempFolder() {
			string folder=Path.Combine(Path.GetTempPath(),"ui0c-selftest-"+Guid.NewGuid().ToString("N").Substring(0,12));
			Directory.CreateDirectory(folder);
			return folder;
		}

		private static void DeleteQuietly(string folder) {
			try {
				Directory.Delete(folder,true);
			}
			catch(Exception) {
				//A file still open by an antivirus scan: left in the temp folder.
			}
		}

		///<summary>Runs the action on an STA thread with the en-US culture (Windows Forms is built there, as the harness's window thread is STA).</summary>
		private static void RunSta(Action action) {
			Exception exception=null;
			Thread thread=new Thread(() => {
				try {
					Thread.CurrentThread.CurrentCulture=new CultureInfo("en-US");
					action();
				}
				catch(Exception ex) {
					exception=ex;
				}
			});
			thread.SetApartmentState(ApartmentState.STA);
			thread.Start();
			if(!thread.Join(TimeSpan.FromMinutes(2))) {
				Assert.Fail("The STA thread did not end within 2 minutes");
			}
			if(exception!=null) {
				System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(exception).Throw();
			}
		}

		private static string RepoFile(params string[] arrayParts) {
			return Path.Combine(new[] { WinChkEnv.RepoRoot }.Concat(arrayParts).ToArray());
		}

		private static string _textMenus;
		private static Dictionary<string,List<string>> _dictClassFiles;

		///<summary>The source files of OpenPmsDental that declare each class (partial classes in several files), by class name.</summary>
		private static Dictionary<string,List<string>> ClassFiles {
			get {
				if(_dictClassFiles==null) {
					_dictClassFiles=new Dictionary<string,List<string>>();
					foreach(string path in Directory.GetFiles(RepoFile("OpenPmsDental"),"*.cs",SearchOption.AllDirectories)) {
						if(path.Contains(Path.DirectorySeparatorChar+"obj"+Path.DirectorySeparatorChar) || path.Contains(Path.DirectorySeparatorChar+"bin"+Path.DirectorySeparatorChar)) {
							continue;
						}
						foreach(Match match in Regex.Matches(File.ReadAllText(path),@"\bclass\s+(\w+)")) {
							if(!_dictClassFiles.TryGetValue(match.Groups[1].Value,out List<string> listFiles)) {
								listFiles=new List<string>();
								_dictClassFiles[match.Groups[1].Value]=listFiles;
							}
							if(!listFiles.Contains(path)) {
								listFiles.Add(path);
							}
						}
					}
				}
				return _dictClassFiles;
			}
		}

		///<summary>The source of every file that declares the class.</summary>
		private static string ClassSource(string className) {
			Assert.IsTrue(ClassFiles.TryGetValue(className,out List<string> listFiles),"No source declares class "+className);
			return string.Join("\n",listFiles.Select(x => File.ReadAllText(x)));
		}

		///<summary>The body of the method (from its opening brace to the matching closing one), skipping strings, characters and comments while
		///matching braces; null when the source has no such method.</summary>
		public static string MethodBody(string source,string methodName) {
			Match match=Regex.Match(source,@"[\w>\]]\s+"+Regex.Escape(methodName)+@"\s*\([^;{]*?\)\s*\{");
			if(!match.Success) {
				return null;
			}
			int indexOpen=match.Index+match.Length-1;
			int depth=0;
			int i=indexOpen;
			while(i<source.Length) {
				char c=source[i];
				if(c=='/' && i+1<source.Length && source[i+1]=='/') {
					i=source.IndexOf('\n',i);
					if(i<0) {
						break;
					}
					continue;
				}
				if(c=='/' && i+1<source.Length && source[i+1]=='*') {
					i=source.IndexOf("*/",i+2,StringComparison.Ordinal);
					if(i<0) {
						break;
					}
					i+=2;
					continue;
				}
				if(c=='"' || c=='\'') {
					bool isVerbatim=c=='"' && i>0 && (source[i-1]=='@' || (source[i-1]=='$' && i>1 && source[i-2]=='@'));
					i++;
					while(i<source.Length) {
						if(!isVerbatim && source[i]=='\\') {
							i+=2;
							continue;
						}
						if(source[i]==c) {
							if(isVerbatim && i+1<source.Length && source[i+1]=='"') {
								i+=2;
								continue;
							}
							break;
						}
						i++;
					}
					i++;
					continue;
				}
				if(c=='{') {
					depth++;
				}
				else if(c=='}') {
					depth--;
					if(depth==0) {
						return source.Substring(indexOpen,i-indexOpen+1);
					}
				}
				i++;
			}
			return null;
		}

		///<summary>True when the method's body creates the window (new Window(), or a method of the same source that it calls does (two calls
		///deep: a handler that calls LaunchTaskWindow, say).</summary>
		public static bool CreatesWindow(string source,string methodName,string window,int depth=2) {
			string body=MethodBody(source,methodName);
			if(body==null) {
				return false;
			}
			if(Regex.IsMatch(body,@"\bnew\s+"+Regex.Escape(window)+@"\s*\(")) {
				return true;
			}
			if(depth==0) {
				return false;
			}
			foreach(string called in Regex.Matches(body,@"\b([A-Z]\w+)\s*\(").Cast<Match>().Select(x => x.Groups[1].Value).Distinct()) {
				if(called!=methodName && CreatesWindow(source,called,window,depth-1)) {
					return true;
				}
			}
			return false;
		}

		private static string Clean(string text) {
			return text.Replace("&","").Trim();
		}

		private static string TextMenus {
			get {
				if(_textMenus==null) {
					_textMenus=File.ReadAllText(RepoFile("OpenPmsDental","Main Modules","FormOpenDentalMenus.cs"));
				}
				return _textMenus;
			}
		}

		///<summary>The main menu's items as (label, handler or "", label of the menu it is in), from FormOpenDentalMenus.cs: each Layout method's
		///items belong to the menu item passed to it, whose label comes from its new MenuItemOD("label").</summary>
		private static List<string[]> GetMainMenuItems() {
			string text=TextMenus;
			Dictionary<string,string> dictVariableLabels=new Dictionary<string,string>();
			foreach(Match match in Regex.Matches(text,@"(\w+)\s*=\s*new MenuItemOD\(""([^""]+)""")) {
				dictVariableLabels[match.Groups[1].Value]=Clean(match.Groups[2].Value);
			}
			//The menu each Layout method fills: the label of the variable it is called with.
			Dictionary<string,string> dictMethodParents=new Dictionary<string,string>();
			foreach(Match match in Regex.Matches(text,@"\b(Layout\w+)\((\w+)\);")) {
				if(dictVariableLabels.TryGetValue(match.Groups[2].Value,out string label)) {
					dictMethodParents[match.Groups[1].Value]=label;
				}
			}
			List<string[]> listItems=new List<string[]>();
			foreach(Match matchMethod in Regex.Matches(text,@"private void (Layout\w+)\(MenuItemOD \w+\)")) {
				string body=MethodBody(text,matchMethod.Groups[1].Value);
				dictMethodParents.TryGetValue(matchMethod.Groups[1].Value,out string parent);
				foreach(Match match in Regex.Matches(body,@"\.Add\(""([^""]+)"",(\w+)\)|new MenuItemOD\(""([^""]+)""(?:,(\w+))?\)")) {
					string label=Clean(match.Groups[1].Success ? match.Groups[1].Value : match.Groups[3].Value);
					string handler=match.Groups[2].Success ? match.Groups[2].Value : (match.Groups[4].Success ? match.Groups[4].Value : "");
					listItems.Add(new[] { label,handler,parent??"" });
				}
			}
			//The top-level menus (LayoutMenu adds them to menuMain).
			foreach(Match match in Regex.Matches(MethodBody(text,"LayoutMenu"),@"new MenuItemOD\(""([^""]+)""")) {
				listItems.Add(new[] { Clean(match.Groups[1].Value),"","" });
			}
			return listItems;
		}

		private static Assembly AssemblyProgram {
			get {
				return typeof(OpenPmsDental.FormOpenDental).Assembly;
			}
		}

		private static Type FindWindowType(string name) {
			foreach(Assembly assembly in new[] { AssemblyProgram,Assembly.Load("WpfControlsOD") }) {
				Type[] arrayTypes;
				try {
					arrayTypes=assembly.GetTypes();
				}
				catch(ReflectionTypeLoadException ex) {
					arrayTypes=ex.Types.Where(x => x!=null).ToArray();
				}
				Type type=arrayTypes.FirstOrDefault(x => x.Name==name);
				if(type!=null) {
					return type;
				}
			}
			return null;
		}

		private static Bitmap NewPicture(int width,int height,Color color) {
			Bitmap bitmap=new Bitmap(width,height,PixelFormat.Format32bppArgb);
			using(Graphics g=Graphics.FromImage(bitmap)) {
				g.Clear(color);
				g.FillRectangle(Brushes.Navy,width/2,height/2,3,3);
			}
			return bitmap;
		}
		#endregion Helpers

		#region windows.tsv
		///<summary>windows.tsv parses; ids and pictures are unique; it holds about 50 openable common windows, the windows the dump cannot fill
		///with a reason, the main window in each module at 1366x768 with and without a patient and at 1920x1080, and the dashboard.</summary>
		[TestMethod]
		public void WindowsTsv_Parses_WithTheWindowsThePlanAsksFor() {
			List<UiWindowRow> listRows=UiWindows.Read();
			Assert.AreEqual(listRows.Count,listRows.Select(x => x.Id).Distinct().Count(),"ids are unique");
			Assert.AreEqual(listRows.Count,listRows.Select(x => x.Picture.ToLowerInvariant()).Distinct().Count(),"pictures are unique");
			List<UiWindowRow> listWindows=listRows.Where(x => Regex.IsMatch(x.Id,@"^U\d+$")).ToList();
			int countOpenable=listWindows.Count(x => x.Opened!=UiWindowRow.OPENED_NONE);
			Assert.IsTrue(countOpenable>=50,"about 50 common windows: "+countOpenable);
			Assert.AreEqual(listWindows.Count,listWindows.Select(x => x.Window).Distinct().Count(),"each window once");
			foreach(UiWindowRow row in listWindows.Where(x => x.Opened==UiWindowRow.OPENED_NONE)) {
				StringAssert.Contains(row.Target,"dump","row "+row.Id+" says why the dump cannot fill it");
			}
			Assert.IsTrue(listWindows.Any(x => x.Opened==UiWindowRow.OPENED_NONE),"the windows the dump cannot fill are listed");
			//The plan's themes (section 7's UI-0c row), each with at least one window.
			foreach(string theme in new[] { "claims","recall","confirmation","schedules","pay plans","billing","procedure codes","definitions","security","tasks",
				"medical","Rx","labs" })
			{
				Assert.IsTrue(listWindows.Any(x => x.Opened!=UiWindowRow.OPENED_NONE && x.Note.StartsWith(theme)),"a window for "+theme);
			}
			foreach(string size in new[] { "1366x768","1920x1080" }) {
				foreach(string module in UiWindows.ArrayModules) {
					Assert.IsTrue(listRows.Any(x => x.Opened==UiWindowRow.OPENED_MAIN && x.Target==size && x.Module==module && x.HasPatient),size+" "+module+" with a patient");
				}
			}
			foreach(string module in UiWindows.ArrayModules) {
				Assert.IsTrue(listRows.Any(x => x.Opened==UiWindowRow.OPENED_MAIN && x.Module==module && !x.HasPatient),module+" without a patient");
			}
			Assert.IsTrue(listRows.Any(x => x.Opened==UiWindowRow.OPENED_DASHBOARD && x.HasPatient && x.ListGuardTables.Contains("sheetdef")),"the dashboard");
			foreach(UiWindowRow row in listRows.Where(x => x.Opened==UiWindowRow.OPENED_MAIN || x.Opened==UiWindowRow.OPENED_DASHBOARD)) {
				Assert.AreNotEqual(Size.Empty,row.MainSize,"row "+row.Id+" names a size");
				Assert.AreEqual("FormOpenDental",row.Window);
			}
			//The test side picks the rows by prefix; the methods' names say the ranges.
			Assert.IsTrue(listWindows.Last().Id=="U"+listWindows.Count,"U1 to U"+listWindows.Count+" without gaps");
			StringAssert.Contains(typeof(UTests).GetMethods().Select(x => x.Name).First(x => x.StartsWith("U1to")),"U"+listWindows.Count+"_",
				"UTests' method name says the last window");
		}

		///<summary>A malformed row is refused with its line number.</summary>
		[TestMethod]
		public void WindowsTsv_RefusesMalformedRows() {
			string ok="U1\tU1-x.png\tFormX\tno\t-\tmenu\tSetup > X\t-\tnote";
			Assert.AreEqual(1,UiWindows.Parse("# comment\n"+UiWindows.HEADER+"\n"+ok+"\n","w.tsv").Count);
			foreach(string bad in new[] {
				"U1\tU1-x.png\tFormX\tno\t-\tmenu\tSetup > X\t-",//8 fields
				"X1\tX1-x.png\tFormX\tno\t-\tmenu\tSetup > X\t-\tnote",//id
				"U1\tU2-x.png\tFormX\tno\t-\tmenu\tSetup > X\t-\tnote",//picture of another id
				"U1\tU1-x.png\tControlX\tno\t-\tmenu\tSetup > X\t-\tnote",//not a window type
				"U1\tU1-x.png\tFormX\tmaybe\t-\tmenu\tSetup > X\t-\tnote",//patient
				"U1\tU1-x.png\tFormX\tno\tReports\tmenu\tSetup > X\t-\tnote",//module
				"U1\tU1-x.png\tFormX\tno\t-\tclick\tSetup > X\t-\tnote",//opened
				"U1\tU1-x.png\tFormX\tno\t-\tmenu\t\t-\tnote",//target
				"U1\tU1-x.png\tFormX\tno\t-\tmenu\tSetup > X\tPatient; DROP\tnote",//guard
			}) {
				FormatException formatException=Assert.ThrowsExactly<FormatException>(() => UiWindows.Parse(ok+"\n"+bad,"w.tsv"),bad);
				StringAssert.Contains(formatException.Message,"w.tsv line 2: ");
			}
		}

		///<summary>Every window type is a Form of the program or a Frm of WpfControlsOD.</summary>
		[TestMethod]
		public void WindowsTsv_EveryWindowTypeExists() {
			List<string> listMissing=new List<string>();
			foreach(UiWindowRow row in UiWindows.Read()) {
				Type type=FindWindowType(row.Window);
				if(type==null) {
					listMissing.Add(row.Id+" "+row.Window+": no such type");
					continue;
				}
				bool isWindow=typeof(Form).IsAssignableFrom(type) || typeof(System.Windows.FrameworkElement).IsAssignableFrom(type);
				if(!isWindow) {
					listMissing.Add(row.Id+" "+row.Window+": not a Form or a WPF element");
				}
			}
			Assert.AreEqual(0,listMissing.Count,string.Join("; ",listMissing));
		}

		///<summary>Every menu row's path is the main menu's (each item in the menu before it, by FormOpenDentalMenus.cs), and its handler creates
		///the window.</summary>
		[TestMethod]
		public void WindowsTsv_MenuRows_AreTheMainMenusItems_AndOpenTheirWindow() {
			List<string[]> listItems=GetMainMenuItems();
			Assert.IsTrue(listItems.Any(x => x[0]=="Schedules" && x[1]=="menuItemSched_Click" && x[2]=="Setup"),"the menu parser finds Setup > Schedules");
			string sourceMain=ClassSource("FormOpenDental");
			Assert.IsTrue(CreatesWindow(sourceMain,"menuItemSched_Click","FormSchedule"));
			Assert.IsFalse(CreatesWindow(sourceMain,"menuItemSched_Click","FormDefinitions"),"another window is not taken for the handler's");
			Assert.IsNull(MethodBody(sourceMain,"menuItemNoSuchHandler_Click"));
			List<string> listProblems=new List<string>();
			foreach(UiWindowRow row in UiWindows.Read().Where(x => x.Opened==UiWindowRow.OPENED_MENU)) {
				string[] arrayPath=row.MenuPath;
				for(int i=0;i<arrayPath.Length;i++) {
					string parent=i==0 ? "" : arrayPath[i-1];
					if(!listItems.Any(x => x[0]==arrayPath[i] && x[2]==parent)) {
						listProblems.Add(row.Id+": no item '"+arrayPath[i]+"' in "+(parent=="" ? "the menu bar" : "'"+parent+"'"));
					}
				}
				string[] arrayLeaf=listItems.FirstOrDefault(x => x[0]==arrayPath.Last() && x[2]==(arrayPath.Length>1 ? arrayPath[arrayPath.Length-2] : "") && x[1]!="");
				if(arrayLeaf==null) {
					listProblems.Add(row.Id+": '"+row.Target+"' has no handler");
					continue;
				}
				if(!CreatesWindow(sourceMain,arrayLeaf[1],row.Window)) {
					listProblems.Add(row.Id+": "+arrayLeaf[1]+" does not create "+row.Window);
				}
			}
			Assert.AreEqual(0,listProblems.Count,string.Join("; ",listProblems));
		}

		///<summary>Every module row: the main window's field of the module exists, and its handler, its button's Click handler or its grid's
		///CellDoubleClick handler creates the window; the Account row's handler reads the column; a script row's script exists.</summary>
		[TestMethod]
		public void WindowsTsv_ModuleRows_OpenTheirWindow() {
			List<string> listProblems=new List<string>();
			foreach(UiWindowRow row in UiWindows.Read()) {
				switch(row.Opened) {
					case UiWindowRow.OPENED_HANDLER:
					case UiWindowRow.OPENED_BUTTON:
					case UiWindowRow.OPENED_GRID: {
						FieldInfo fieldModule=typeof(OpenPmsDental.FormOpenDental).GetField(row.ModuleField,BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public);
						if(fieldModule==null) {
							listProblems.Add(row.Id+": the main window has no field "+row.ModuleField);
							continue;
						}
						Type typeModule=fieldModule.FieldType;
						string source=ClassSource(typeModule.Name);
						string handler=row.Member;
						if(row.Opened==UiWindowRow.OPENED_HANDLER) {
							try {
								InProcChecks.FindMethod(typeModule,row.Member);
							}
							catch(MissingMethodException) {
								listProblems.Add(row.Id+": "+typeModule.Name+" has no handler "+row.Member+" without arguments or with (sender, e)");
								continue;
							}
						}
						else {
							FieldInfo fieldControl=typeModule.GetField(row.Member,BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public);
							if(fieldControl==null || !typeof(Control).IsAssignableFrom(fieldControl.FieldType)) {
								listProblems.Add(row.Id+": "+typeModule.Name+" has no control "+row.Member);
								continue;
							}
							string eventName=row.Opened==UiWindowRow.OPENED_BUTTON ? "Click" : "CellDoubleClick";
							Match match=Regex.Match(source,@"this\."+Regex.Escape(row.Member)+@"\."+eventName+@"\s*\+=\s*new [\w.]+\(this\.(\w+)\)");
							if(!match.Success) {
								listProblems.Add(row.Id+": "+row.Member+"."+eventName+" has no handler in "+typeModule.Name);
								continue;
							}
							handler=match.Groups[1].Value;
						}
						if(!CreatesWindow(source,handler,row.Window)) {
							listProblems.Add(row.Id+": "+typeModule.Name+"."+handler+" does not create "+row.Window);
						}
						if(row.Opened==UiWindowRow.OPENED_GRID && !Regex.IsMatch(row.GridMatch,"^(tag|text):.+")) {
							listProblems.Add(row.Id+": the grid row is matched by tag: or text:");
						}
						break;
					}
					case UiWindowRow.OPENED_ACCOUNTROW: {
						string source=ClassSource("ControlAccount");
						string body=MethodBody(source,"gridAccount_CellDoubleClick");
						if(!CreatesWindow(source,"gridAccount_CellDoubleClick",row.Window) || !body.Contains("[\""+row.Target+"\"]")) {
							listProblems.Add(row.Id+": the Account grid's double-click does not open "+row.Window+" for "+row.Target);
						}
						break;
					}
					case UiWindowRow.OPENED_SCRIPT: {
						MethodInfo methodInfo=typeof(InProcChecks).GetMethod("ScriptU"+row.Target,BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic);
						if(methodInfo==null || methodInfo.ReturnType!=typeof(Ui.Opened)
							|| !methodInfo.GetParameters().Select(x => x.ParameterType).SequenceEqual(new[] { typeof(Ui),typeof(CheckResult) }))
						{
							listProblems.Add(row.Id+": no script Ui.Opened ScriptU"+row.Target+"(Ui,CheckResult)");
						}
						break;
					}
				}
			}
			Assert.AreEqual(0,listProblems.Count,string.Join("; ",listProblems));
		}

		///<summary>Every guard table is a table of the program's schema (db/init/new-database.sql), so the counts never fail on a typo.</summary>
		[TestMethod]
		public void WindowsTsv_GuardTables_AreTablesOfTheSchema() {
			string schema=File.ReadAllText(RepoFile("db","init","new-database.sql"));
			HashSet<string> setTables=new HashSet<string>(Regex.Matches(schema,@"CREATE TABLE `(\w+)`").Cast<Match>().Select(x => x.Groups[1].Value));
			Assert.IsTrue(setTables.Contains("patient") && setTables.Count>300,"tables read: "+setTables.Count);
			List<string> listMissing=UiWindows.Read().SelectMany(x => x.ListGuardTables.Select(y => x.Id+" "+y)).Where(x => !setTables.Contains(x.Split(' ')[1])).ToList();
			Assert.AreEqual(0,listMissing.Count,string.Join("; ",listMissing));
		}

		///<summary>UI-0c fix 1: what group U answers on the way to a window and on closing is still what the program asks, so a changed text fails
		///here and not in a run at the owner's desktop: each expected question is in the source of its window (or a class the window holds) with
		///its button's kind, each window shown first is created in the row window's Load, and each discard question Ui.Close answers OK is asked
		///with OK and Cancel.</summary>
		[TestMethod]
		public void Fix1_ExpectedQuestionsAndWindowsOnTheWay_AreThePrograms() {
			List<string> listProblems=new List<string>();
			HashSet<string> setWindows=new HashSet<string>(UiWindows.Read().Select(x => x.Window));
			Dictionary<string,string> dictQuestionClasses=new Dictionary<string,string> { { "FormTasks","UserControlTasks" } };
			foreach(KeyValuePair<string,(string Question,string Button,string Why)> pair in InProcChecks._dictUQuestions) {
				if(!setWindows.Contains(pair.Key)) {
					listProblems.Add(pair.Key+" is no row's window");
				}
				string source=ClassSource(dictQuestionClasses.TryGetValue(pair.Key,out string className) ? className : pair.Key);
				string kind=pair.Value.Button=="Yes" || pair.Value.Button=="No" ? "YesNo" : "OKCancel";
				if(!Regex.IsMatch(source,@"MsgBoxButtons\."+kind+@",\s*"""+Regex.Escape(pair.Value.Question))) {
					listProblems.Add(pair.Key+": no "+kind+" question '"+pair.Value.Question+"' in "+(className??pair.Key));
				}
			}
			foreach(KeyValuePair<string,(string Window,string Message,string Button,string Why)> pair in InProcChecks._dictUOnTheWay) {
				if(!setWindows.Contains(pair.Key)) {
					listProblems.Add(pair.Key+" is no row's window");
				}
				string body=MethodBody(ClassSource(pair.Key),pair.Key+"_Load")??"";
				if(!body.Contains("new "+pair.Value.Window+"(") || !Regex.IsMatch(ClassSource(pair.Value.Window),@"\bClose\(\);")) {
					listProblems.Add(pair.Key+"_Load does not show "+pair.Value.Window+", or "+pair.Value.Window+" never closes itself");
				}
			}
			//U2's message: FormClaimReports_Shown shows the import's error, and the import names a missing report folder with this text.
			string bodyShown=MethodBody(ClassSource("FormClaimReports"),"FormClaimReports_Shown")??"";
			string sourceClearinghouses=File.ReadAllText(RepoFile("OpenPmsDentalBusiness","Data Interface","Clearinghouses.cs"));
			string messageU2=InProcChecks._dictUOnTheWay["FormClaimsSend"].Message;
			if(!bodyShown.Contains("MessageBox.Show(errorMessage)")
				|| !sourceClearinghouses.Contains("\""+messageU2.TrimEnd(':')+"\")+\": \"+clearinghouseClin.ResponsePath"))
			{
				listProblems.Add("FormClaimReports_Shown no longer shows the import's '"+messageU2+"' message");
			}
			foreach(string question in Ui.ArrayDiscardQuestions) {
				if(!Regex.IsMatch(ClassSource("FormClaimEdit"),@"MsgBoxButtons\.OKCancel,\s*"""+Regex.Escape(question))) {
					listProblems.Add("FormClaimEdit asks no OKCancel '"+question+"'");
				}
			}
			Assert.IsTrue(InProcChecks._dictUQuestions.ContainsKey("FormTasks") && InProcChecks._dictUOnTheWay.ContainsKey("FormClaimsSend"),
				"the U31 and U2 entries");
			Assert.AreEqual(0,listProblems.Count,string.Join("; ",listProblems));
		}
		#endregion windows.tsv

		#region The walker
		///<summary>The walker on a test form built and never shown: it records every control's bounds in UI-0b's format, measures the visible
		///controls with text (a label on a tab page that is not selected too, a hidden one not), finds an overflow with its free space, suggests
		///masks for today's date and the clock, and writes files the gallery builder and the mask reader accept. With the font option it swaps the
		///fonts and finds the label that fits today and not in Segoe UI 9.</summary>
		[TestMethod]
		public void Walker_MeasuresATestForm_RecordsBounds_SuggestsMasks_AndMeasuresTheFontOption() {
			string folder=NewTempFolder();
			try {
				RunSta(() => {
					DateTime dateTimeNow=new DateTime(2026,11,17,14,5,0);
					using Form form=new Form();
					form.FormBorderStyle=FormBorderStyle.None;
					form.ShowInTaskbar=false;
					form.Font=new Font("Microsoft Sans Serif",8.25f);
					form.ClientSize=new Size(500,400);
					Label labelFits=new Label { Name="labelFits",Text="OK",AutoSize=false,Location=new Point(10,10),Size=new Size(100,20) };
					Label labelCut=new Label { Name="labelCut",Text="A label whose text is far too long for its box",AutoSize=false,Location=new Point(10,40),
						Size=new Size(60,14) };
					Label labelHidden=new Label { Name="labelHidden",Text="A hidden label whose text is far too long",AutoSize=false,Location=new Point(10,70),
						Size=new Size(40,14),Visible=false };
					Label labelToday=new Label { Name="labelToday",Text="Date: "+dateTimeNow.ToString("d",new CultureInfo("en-US")),AutoSize=false,
						Location=new Point(10,200),Size=new Size(150,20) };
					Label labelClock=new Label { Name="labelClock",Text=dateTimeNow.AddMinutes(-3).ToString("T",new CultureInfo("en-US")),AutoSize=false,
						Location=new Point(200,200),Size=new Size(120,20) };
					string textTight="Patient Information";
					Size sizeTight=TextRenderer.MeasureText(textTight,form.Font);
					Label labelTight=new Label { Name="labelTight",Text=textTight,AutoSize=false,Location=new Point(10,240),
						Size=new Size(sizeTight.Width+4,sizeTight.Height+2) };
					System.Windows.Forms.TabControl tabControl=new System.Windows.Forms.TabControl { Name="tabControl",Location=new Point(250,10),Size=new Size(200,150) };
					TabPage tabPage1=new TabPage("One") { Name="tabPage1" };
					TabPage tabPage2=new TabPage("Two") { Name="tabPage2" };
					tabPage2.Controls.Add(new Label { Name="labelOnPage2",Text="Text on the second page that is too long",AutoSize=false,Location=new Point(5,5),
						Size=new Size(50,14) });
					tabControl.TabPages.Add(tabPage1);
					tabControl.TabPages.Add(tabPage2);
					form.Controls.AddRange(new Control[] { labelFits,labelCut,labelHidden,labelToday,labelClock,labelTight,tabControl });
					UiWalkResult result=UiWalker.Walk(form,"T1-test","T1-test.png",Rectangle.Empty,dateTimeNow);
					Assert.IsFalse(form.Visible,"The test form is never shown");
					//The bounds record: every control and the form itself, in UI-0b's format.
					Assert.AreEqual(1+UiWalker.AllControls(form).Count(),result.ListBounds.Count);
					Assert.AreEqual(".",result.ListBounds[0].Path);
					Assert.AreEqual(new Rectangle(10,40,60,14),result.ListBounds.Single(x => x.Path=="labelCut").Bounds);
					//The measured controls.
					List<string> listMeasured=result.ListMeasures.Select(x => x.Path).ToList();
					CollectionAssert.Contains(listMeasured,"labelFits");
					CollectionAssert.Contains(listMeasured,"tabControl/tabPage2/labelOnPage2","a label on the page that is not selected is measured");
					CollectionAssert.DoesNotContain(listMeasured,"labelHidden","a hidden label is not measured");
					Assert.IsFalse(result.ListMeasures.Single(x => x.Path=="labelFits").Result.IsOverflow);
					Assert.IsFalse(result.ListMeasures.Single(x => x.Path=="labelTight").Result.IsOverflow,"the tight label fits in today's font");
					UiWalkMeasure measureCut=result.ListMeasures.Single(x => x.Path=="labelCut");
					Assert.IsTrue(measureCut.Result.IsOverflow);
					Assert.AreEqual(TextFitMeasurer.GetFreeSpace(labelCut),measureCut.PointFree);
					Assert.AreEqual(250-70,measureCut.PointFree.X,"free up to the tab control beside it");
					//The masks: today's date and the clock, at their places in the picture.
					Assert.AreEqual(2,result.ListMasks.Count,string.Join("; ",result.ListMasks.Select(x => x.ToMaskLine())));
					UiMaskSuggestion maskDate=result.ListMasks.Single(x => x.What.Contains("labelToday"));
					Assert.AreEqual(new Rectangle(10,200,150,20),maskDate.Rectangle);
					StringAssert.StartsWith(maskDate.What,"date: ");
					StringAssert.StartsWith(result.ListMasks.Single(x => x.What.Contains("labelClock")).What,"clock: ");
					//The font option: the swap's fonts, the new overflow, the bounds compared.
					UiWalker.WalkOption(form,result,TextFitFontOption.Segoe9,new HashSet<string>());
					Assert.AreEqual("Segoe UI",labelTight.Font.FontFamily.Name);
					Assert.AreEqual("segoe9",result.FontOption);
					List<string> listNew=result.ListNewOverflows.Select(x => x.Path).ToList();
					CollectionAssert.Contains(listNew,"labelTight","fits today, not in Segoe UI 9");
					CollectionAssert.DoesNotContain(listNew,"labelCut","already overflowing today: not new");
					StringAssert.Contains(result.Summary,"in segoe9: ");
					//The files.
					UiWalker.Write(result,folder);
					UiWalker.Write(result,folder);
				});
				List<TextFitBoundsRow> listRead;
				using(StreamReader streamReader=new StreamReader(Path.Combine(folder,"bounds-record.tsv"))) {
					listRead=TextFitBoundsRecord.Read(streamReader);
				}
				Assert.AreEqual(2*11,listRead.Count,"two writes of 11 rows, one header");
				Assert.AreEqual(11,UiApprovalCheck.ReadBoundsRecord(Path.Combine(folder,"bounds-record.tsv")).Count,"the last record of the window is kept");
				string textMasks=File.ReadAllText(Path.Combine(folder,"masks-suggested.tsv"));
				List<UiMaskRow> listMasks=UiPictureMask.ParseMasks(textMasks,"masks-suggested.tsv");
				Assert.AreEqual(4,listMasks.Count,"the suggestions are rows tools\\ui-gallery-masks.tsv accepts");
				Assert.AreEqual(new Rectangle(10,200,150,20),UiPictureMask.GetRectangle(listMasks.First(x => x.What.Contains("labelToday")),500,400));
				string[] arrayClipNotes=File.ReadAllLines(Path.Combine(folder,"clip-notes.tsv"));
				Assert.AreEqual(UiWalker.HEADER_CLIP_NOTES,arrayClipNotes[0]);
				Assert.IsTrue(arrayClipNotes.Any(x => x.StartsWith("T1-test.png\tlabelCut\ttoday's font, already overflowing")),string.Join(" | ",arrayClipNotes));
				Assert.IsTrue(arrayClipNotes.Any(x => x.StartsWith("T1-test.png\tlabelTight\tsegoe9, new")),string.Join(" | ",arrayClipNotes));
				string[] arrayTextFit=File.ReadAllLines(Path.Combine(folder,"text-fit.tsv"));
				Assert.AreEqual(UiWalker.HEADER_TEXT_FIT,arrayTextFit[0]);
				Assert.AreEqual(1,arrayTextFit.Count(x => x==UiWalker.HEADER_TEXT_FIT),"one header");
				Assert.IsTrue(File.Exists(Path.Combine(folder,"bounds-record-segoe9.tsv")) && File.Exists(Path.Combine(folder,"bounds-segoe9.tsv")));
				byte[] arrayBytes=File.ReadAllBytes(Path.Combine(folder,"masks-suggested.tsv"));
				Assert.AreNotEqual(0xEF,arrayBytes[0],"no BOM");
			}
			finally {
				DeleteQuietly(folder);
			}
		}

		///<summary>Today's date in the forms a window shows it, and a time near now, are found; other dates and times are not.</summary>
		[TestMethod]
		public void Walker_FindsTodaysDateAndTheClock_Only() {
			RunSta(() => {
				DateTime dateTimeNow=new DateTime(2026,11,17,9,30,0);
				Assert.AreEqual("date",UiWalker.GetDateOrClock("11/17/2026",dateTimeNow));
				Assert.AreEqual("date",UiWalker.GetDateOrClock("From 2026-11-17 to 2026-12-17",dateTimeNow));
				Assert.AreEqual("date",UiWalker.GetDateOrClock("Tuesday, November 17, 2026",dateTimeNow));
				Assert.AreEqual("clock",UiWalker.GetDateOrClock("9:41:07 AM",dateTimeNow));
				Assert.AreEqual("clock",UiWalker.GetDateOrClock("Server Time 09:12",dateTimeNow));
				Assert.AreEqual("",UiWalker.GetDateOrClock("11/09/2026",dateTimeNow),"the synthetic appointments' day is not today");
				Assert.AreEqual("",UiWalker.GetDateOrClock("1:30 PM",dateTimeNow),"four hours off");
				Assert.AreEqual("",UiWalker.GetDateOrClock("Claim #2, $100.00",dateTimeNow));
				Assert.AreEqual("",UiWalker.GetDateOrClock("",dateTimeNow));
				using DateTimePicker dateTimePicker=new DateTimePicker { Value=dateTimeNow };
				Assert.AreEqual("date",UiWalker.GetDateOrClock(dateTimePicker,dateTimeNow));
				dateTimePicker.Value=dateTimeNow.AddDays(-3);
				Assert.AreEqual("",UiWalker.GetDateOrClock(dateTimePicker,dateTimeNow));
			});
			Assert.IsNull(UiWalker.GetFontOption(""));
			Assert.AreEqual(TextFitFontOption.Segoe825,UiWalker.GetFontOption("Segoe825"));
			Assert.ThrowsExactly<ArgumentException>(() => UiWalker.GetFontOption("segoe10"));
		}
		#endregion The walker

		#region U0
		///<summary>U0 hashes each picture with the gallery's rule and compares it with approved.tsv: a picture whose masked pixels changed is still
		///approved, one that changed elsewhere is "changed since approval" (for the owner, PARTLY), one never approved is "not approved yet", one
		///the results name but the folder lacks is "missing". It writes U0.tsv and approved.tsv rows for the pictures not approved.</summary>
		[TestMethod]
		public void U0_ComparesSyntheticPicturesWithApprovedRows() {
			string folder=NewTempFolder();
			try {
				List<UiMaskRow> listMasks=UiPictureMask.ParseMasks("picture\tsize\tanchor\tx\ty\twidth\theight\twhat\n"
					+"U1-a.png\t*\ttop-left\t0\t0\t4\t4\tclock: a test clock\n","masks.tsv");
				string sha1;
				string sha2Old;
				using(Bitmap bitmap=NewPicture(20,10,Color.White)) {
					sha1=UiPictureMask.GetMaskedPixelSha256(bitmap,"U1-a.png",listMasks);
					bitmap.SetPixel(1,1,Color.Red);//inside the mask: still the approved picture
					bitmap.Save(Path.Combine(folder,"U1-a.png"),ImageFormat.Png);
				}
				using(Bitmap bitmap=NewPicture(20,10,Color.White)) {
					sha2Old=UiPictureMask.GetMaskedPixelSha256(bitmap,"U2-b.png",listMasks);
					bitmap.SetPixel(15,5,Color.Red);//outside any mask: changed
					bitmap.Save(Path.Combine(folder,"U2-b.png"),ImageFormat.Png);
				}
				using(Bitmap bitmap=NewPicture(30,10,Color.Gray)) {
					bitmap.Save(Path.Combine(folder,"U3-c.png"),ImageFormat.Png);
				}
				List<UiApprovedRow> listApproved=UiPictureMask.ParseApproved(UiPictureMask.ApprovedHeader+"\n"
					+"U1-a.png\t20x10\t"+sha1+"\tG-UI3\tabc1234\t2026-11-20\n"
					+"U2-b.png\t20x10\t"+sha2Old+"\tG-UI3\tabc1234\t2026-11-20\n","approved.tsv");
				List<string> listPictures=new List<string> { "U1-a.png","U2-b.png","U3-c.png","U4-gone.png" };
				List<UiPictureState> listStates=UiApprovalCheck.Classify(folder,listPictures,listMasks,listApproved);
				CollectionAssert.AreEqual(new[] { UiApprovalCheck.STATE_APPROVED,UiApprovalCheck.STATE_CHANGED,UiApprovalCheck.STATE_NOT_APPROVED,
					UiApprovalCheck.STATE_MISSING },listStates.Select(x => x.State).ToArray());
				Assert.AreEqual(sha1,listStates[0].Sha256,"the masked pixel changed nothing");
				Assert.AreEqual(30,listStates[2].Width);
				string folderOut=Path.Combine(folder,"out");
				CheckResult checkResult=UiApprovalCheck.Check(listStates,listApproved.Count,folderOut,"def5678","2026-11-21");
				Assert.AreEqual("U0",checkResult.Id);
				Assert.AreEqual(CheckStatus.Partly,checkResult.Status,checkResult.Summary);
				Assert.AreEqual(1,checkResult.ListForPerson.Count);
				StringAssert.Contains(checkResult.ListForPerson[0],"U2-b.png");
				Assert.IsFalse(checkResult.ListForPerson[0].Contains("U3-c.png"),"not approved yet is not for the owner here");
				Assert.IsTrue(checkResult.ListNotes.Any(x => x.Contains("U3-c.png")));
				Assert.IsTrue(checkResult.ListNotes.Any(x => x.Contains("U4-gone.png")));
				string[] arrayU0=File.ReadAllLines(Path.Combine(folderOut,"U0.tsv"));
				Assert.AreEqual(5,arrayU0.Length);
				StringAssert.StartsWith(arrayU0[2],"U2-b.png\t20x10\t");
				StringAssert.Contains(arrayU0[2],"\t"+UiApprovalCheck.STATE_CHANGED+"\t20x10\t"+sha2Old+"\tG-UI3\tabc1234");
				List<UiApprovedRow> listRows=UiPictureMask.ReadApproved(Path.Combine(folderOut,"U0-approved-rows.tsv"));
				CollectionAssert.AreEqual(new[] { "U2-b.png","U3-c.png" },listRows.Select(x => x.Picture).ToArray(),"rows for the pictures not approved");
				Assert.AreEqual(listStates[1].Sha256,listRows[0].Sha256);
				Assert.AreEqual("def5678",listRows[0].Commit);
				Assert.AreEqual("2026-11-21",listRows[0].Date);
				Assert.IsTrue(UiPictureMask.IsApproved(listRows,"U3-c.png",30,10,listStates[2].Sha256),"once appended, the picture counts as approved");
				//An empty approved.tsv: every picture not approved yet, said so, and no failure.
				List<UiPictureState> listStatesEmpty=UiApprovalCheck.Classify(folder,listPictures.Take(3),listMasks,new List<UiApprovedRow>());
				Assert.IsTrue(listStatesEmpty.All(x => x.State==UiApprovalCheck.STATE_NOT_APPROVED));
				CheckResult checkResultEmpty=UiApprovalCheck.Check(listStatesEmpty,0,null,"def5678","2026-11-21");
				Assert.AreEqual(CheckStatus.Passed,checkResultEmpty.Status,checkResultEmpty.Summary);
				CollectionAssert.Contains(checkResultEmpty.ListNotes,UiApprovalCheck.TEXT_EMPTY);
				StringAssert.Contains(checkResultEmpty.Summary,"3 "+UiApprovalCheck.STATE_NOT_APPROVED);
			}
			finally {
				DeleteQuietly(folder);
			}
		}

		///<summary>U0 takes the pictures of this commit's results rows, each once.</summary>
		[TestMethod]
		public void U0_TakesThePicturesOfThisCommitsResults() {
			List<string[]> listRows=new List<string[]> {
				new[] { "t","U1","PASSED","x","x","U1-a.png","abc" },
				new[] { "t","U2","PASSED","x","x","U2-b.png, U2-c.png","abc" },
				new[] { "t","A1","PASSED","x","x","A1-main.png, U1-a.png","abc" },
				new[] { "t","B1","PASSED","x","x","B1-appts-day.png","old1234" },
				new[] { "t","Z","PASSED","x","x","","abc" },
			};
			CollectionAssert.AreEqual(new[] { "U1-a.png","U2-b.png","U2-c.png","A1-main.png" },UiApprovalCheck.GetPictures(listRows,"abc"));
		}

		///<summary>The bounds record of a run against the before set: windows both hold, a moved control found, an added window left out.</summary>
		[TestMethod]
		public void U0_ComparesTheBoundsRecordWithTheBeforeSet() {
			TextFitBoundsRow NewRow(string window,string path,int x) {
				return new TextFitBoundsRow { Window=window,Path=path,Type=path=="." ? "OpenPmsDental.FormX" : "System.Windows.Forms.Label",Bounds=new Rectangle(x,5,50,14),
					ClientSize=new Size(50,14),IsVisible=true,Anchor=AnchorStyles.Top|AnchorStyles.Left };
			}
			List<TextFitBoundsRow> listBefore=new List<TextFitBoundsRow> { NewRow("U1-x",".",0),NewRow("U1-x","label1",10),NewRow("U1-x","label2",80) };
			List<TextFitBoundsRow> listAfter=new List<TextFitBoundsRow> { NewRow("U1-x",".",0),NewRow("U1-x","label1",10),NewRow("U1-x","label2",82),
				NewRow("U2-new",".",0),NewRow("U2-new","label1",10) };
			List<TextFitBoundsDifference> listDifferences=UiApprovalCheck.CompareBounds(listBefore,listAfter);
			Assert.AreEqual(1,listDifferences.Count,string.Join("; ",listDifferences));
			Assert.AreEqual("bounds",listDifferences[0].Kind);
			Assert.AreEqual("label2",listDifferences[0].Row.Path);
		}

		///<summary>The repository's mask file and approved.tsv parse, and the mask file covers group U's Appointments pictures.</summary>
		[TestMethod]
		public void U0_RepositoryFiles_Parse_AndMaskGroupUsAppointmentsPictures() {
			List<UiMaskRow> listMasks=UiPictureMask.ReadMasks(UiApprovalCheck.PathMasks);
			UiPictureMask.ReadApproved(UiApprovalCheck.PathApproved);
			foreach(UiWindowRow row in UiWindows.Read().Where(x => x.Opened==UiWindowRow.OPENED_MAIN && x.Module=="Appointments")) {
				Size size=row.MainSize;
				List<UiMaskRow> listFor=UiPictureMask.GetMasksFor(listMasks,row.Picture,size.Width,size.Height);
				Assert.IsTrue(listFor.Any(x => x.What.StartsWith("date: ")),row.Picture+" has its date mask");
			}
		}
		#endregion U0

		#region The reference set
		///<summary>UI-11-final (docs/port-notes/UI-11-final.md section 3): baseline.tsv, the reference pictures of the final build (every window at
		///100%, the densest tenth at 150%), in approved.tsv's format: every row parses, one picture once, all from one commit and one set, the
		///pictures under windows-100/ and windows-150/. approved.tsv stays the owner's approvals.</summary>
		[TestMethod]
		public void Baseline_OneCommit_EveryPictureOnce() {
			string path=Path.Combine(WinChkEnv.RepoRoot,"tests","WindowChecksTests","UiApproved","baseline.tsv");
			List<UiApprovedRow> listRows=UiPictureMask.ParseApproved(File.ReadAllText(path),"baseline.tsv");
			Assert.IsTrue(listRows.Count>=1000,"The reference pictures: "+listRows.Count);
			Assert.AreEqual(listRows.Count,listRows.Select(x => x.Picture.ToLowerInvariant()).Distinct().Count(),"a picture twice");
			Assert.AreEqual(1,listRows.Select(x => x.Commit).Distinct().Count(),"one commit");
			StringAssert.Matches(listRows[0].Commit,new Regex("^[0-9a-f]{7,40}$"));
			Assert.AreEqual(1,listRows.Select(x => x.Gallery).Distinct().Count(),"one set");
			Assert.IsTrue(listRows.All(x => x.Picture.StartsWith("windows-100/") || x.Picture.StartsWith("windows-150/")),"windows-100/ or windows-150/");
			Assert.IsTrue(listRows.Count(x => x.Picture.StartsWith("windows-150/"))>=100,"the densest tenth at 150%");
		}
		#endregion The reference set
	}
}
