// The source scans of the UI refresh's package UI-1 (docs/specs/ui-refresh-plan.md, sections 2.1, 3.2 and 9; docs/port-notes/UI-1.md):
// 1. Coverage: every file in OpenPmsDental\UI, CodeBase\UI, WpfControlsOD\UI and WpfControlsOD\Themes has exactly one row in
//    tests\UiRefreshTests\Data\coverage.tsv with the package that restyles it or the reason it keeps its look, and every row matches a file, so a
//    new control or a new shared dictionary cannot slip through.
// 2. Colour literals: no new Color.FromArgb, Color.FromRgb or hex colour in the shared controls (OpenPmsDental\UI,
//    OpenPmsDentalBusiness\UI\GridOD.cs, WpfControlsOD\UI\Controls) beyond the allow list tests\UiRefreshTests\Data\colour-literals.tsv, which
//    shrinks with each control package: a file with fewer literals than its rows fails too, so the list is cut when a package removes them
//    (OPMS_UI_COLOUR_LITERALS_WRITE=1 writes it again from the source).
// 3. UiTheme's GDI+ pens and brushes are cached per thread and shared: no using and no Dispose around them, anywhere in the repository (Pen and
//    SolidBrush are sealed, so a cached one cannot ignore Dispose).
// 4. UiTheme's WinForms files use only Windows Forms, System.Drawing and Microsoft.Win32, so RdlDesign can compile them in (section 3.6).
// It needs no build, database or network, and starts nothing.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NoHqTests {
	[TestClass]
	public class UiThemeScanTests {
		public const string VARIABLE_WRITE_COLOUR_LITERALS="OPMS_UI_COLOUR_LITERALS_WRITE";
		public const string PATH_COVERAGE=@"tests\UiRefreshTests\Data\coverage.tsv";
		public const string PATH_COLOUR_LITERALS=@"tests\UiRefreshTests\Data\colour-literals.tsv";
		///<summary>The folders whose every file needs a row in coverage.tsv (section 2.1). UI-PLAIN: CodeBase\UI too, where the shared looks moved.</summary>
		public static readonly string[] ArrayCoverageFolders=new string[] { @"OpenPmsDental\UI",@"CodeBase\UI",@"WpfControlsOD\UI",
			@"WpfControlsOD\Themes" };
		///<summary>Where new colour literals are not allowed (section 9): a folder or a file. GridScreenLook is the screen look GridOD and GridOld
		///draw with since UI-5, so it is scanned like GridOD. UI-PLAIN: the shared looks moved from OpenPmsDental\UI to CodeBase\UI stay
		///scanned.</summary>
		public static readonly string[] ArrayColourScopes=new string[] { @"OpenPmsDental\UI",@"OpenPmsDentalBusiness\UI\GridOD.cs",
			@"OpenPmsDentalBusiness\UI\GridScreenLook.cs",@"WpfControlsOD\UI\Controls",
			@"CodeBase\UI\UiInputFrame.cs",@"CodeBase\UI\UiLooks.cs",@"CodeBase\UI\UiRadioLook.cs",@"CodeBase\UI\UiScrollLook.cs",
			@"CodeBase\UI\UiTrackBarLook.cs",@"CodeBase\UI\UiUpDownLook.cs",@"CodeBase\UI\UiPlainWindow.cs",@"CodeBase\UI\UiTabLook.cs" };
		///<summary>The packages of section 7 a coverage row may name.</summary>
		public static readonly string[] ArrayPackages=new string[] { "UI-1","UI-2a","UI-2b","UI-3","UI-4a","UI-4b","UI-4c","UI-5","UI-6","UI-7a","UI-7b",
			"UI-8","UI-9a","UI-9b","UI-10","UI-11","UI-12","UI-13","UI-PLAIN","UI-PLAIN-2","UI-CAL" };
		///<summary>The reasons a coverage row may give instead of packages.</summary>
		public static readonly string[] ArrayReasons=new string[] { "keeps","no-drawing" };
		///<summary>UiTheme's partial files that RdlDesign may link: Windows Forms, System.Drawing and Microsoft.Win32 only. The WPF part is the other.
		///UiTheme.Renderer.cs is the token renderer (UI-6), which RdlDesign links (RdlDesign/ReportDesigner.csproj) with these and the icon renderer.</summary>
		public static readonly string[] ArrayUiThemeWinFormsFiles=new string[] { @"CodeBase\UI\UiTheme.cs",@"CodeBase\UI\UiTheme.Colors.cs",
			@"CodeBase\UI\UiTheme.Gdi.cs",@"CodeBase\UI\UiTheme.Metrics.cs",@"CodeBase\UI\UiTheme.Fonts.cs",@"CodeBase\UI\UiTheme.Renderer.cs" };
		public const string PATH_UITHEME_WPF=@"CodeBase\UI\UiTheme.Wpf.cs";
		private static readonly string[] _arrayUsingsWinForms=new string[] { "System","System.Collections.Generic","System.Drawing","System.Drawing.Drawing2D",
			"System.Windows.Forms","Microsoft.Win32" };

		#region Coverage
		[TestMethod]
		public void Coverage_EveryUiFileHasExactlyOneRow() {
			string pathRoot=RemovedAssembliesTests.GetRepositoryRoot();
			List<CoverageRow> listRows=ReadCoverage(File.ReadAllLines(Path.Combine(pathRoot,PATH_COVERAGE)));
			Assert.IsTrue(listRows.Count>=100,"Expected section 2.1's rows, found "+listRows.Count+".");
			List<string> listProblems=new List<string>();
			foreach(CoverageRow coverageRow in listRows) {
				listProblems.AddRange(GetProblemsOfPackage(coverageRow));
			}
			List<string> listFiles=new List<string>();
			foreach(string folder in ArrayCoverageFolders) {
				string pathFolder=Path.Combine(pathRoot,folder);
				Assert.IsTrue(Directory.Exists(pathFolder),"No folder "+folder+".");
				listFiles.AddRange(Directory.EnumerateFiles(pathFolder,"*",SearchOption.AllDirectories).Select(x => x.Substring(pathRoot.Length+1).Replace('\\','/')));
			}
			//UI-7b removed 72 of the IconLibrary folders' Gen_*.cs files (docs/port-notes/UI-7b.md): 235 files remain.
			Assert.IsTrue(listFiles.Count>=200,"Expected the shared control files, found "+listFiles.Count+".");
			listProblems.AddRange(GetCoverageProblems(listRows,listFiles));
			Assert.AreEqual(0,listProblems.Count,"coverage.tsv ("+PATH_COVERAGE+", section 2.1 of docs/specs/ui-refresh-plan.md) and the files disagree:\r\n"
				+string.Join("\r\n",listProblems));
		}

		[TestMethod]
		public void Coverage_Rules_MainNamesAndMatches() {
			Assert.AreEqual("OpenPmsDental/UI/Button",GetMainName("OpenPmsDental/UI/Button.cs"));
			Assert.AreEqual("OpenPmsDental/UI/Button",GetMainName("OpenPmsDental/UI/Button.designer.cs"));
			Assert.AreEqual("OpenPmsDental/UI/Button",GetMainName("OpenPmsDental/UI/Button.Designer.cs"));
			Assert.AreEqual("OpenPmsDental/UI/Button",GetMainName("OpenPmsDental/UI/Button.resx"));
			Assert.AreEqual("WpfControlsOD/UI/Controls/Grid",GetMainName("WpfControlsOD/UI/Controls/Grid.xaml"));
			Assert.AreEqual("WpfControlsOD/UI/Controls/Grid",GetMainName("WpfControlsOD/UI/Controls/Grid.xaml.cs"));
			Assert.AreEqual("OpenPmsDental/UI/Appt/ControlApptPanel.TimeBars",GetMainName("OpenPmsDental/UI/Appt/ControlApptPanel.TimeBars.cs"));
			List<CoverageRow> listRows=ReadCoverage(new string[] { "# a comment","files\tpackage\tnote","A/UI/Button\tUI-4a\tthe button",
				"A/UI/Appt/ControlApptPanel*\tkeeps\tcontent","A/UI/Tab/*\tUI-4b\ttabs","A/UI/Gone\tUI-6\tno such file" });
			Assert.AreEqual(4,listRows.Count);
			List<string> listProblems=GetCoverageProblems(listRows,new List<string> { "A/UI/Button.cs","A/UI/Button.resx","A/UI/Appt/ControlApptPanel.cs",
				"A/UI/Appt/ControlApptPanel.TimeBars.cs","A/UI/Tab/TabPage.cs","A/UI/New.cs","A/UI/Appt/ControlApptProvSlider.cs" });
			CollectionAssert.AreEquivalent(new[] { "A/UI/New.cs: no row","A/UI/Appt/ControlApptProvSlider.cs: no row","A/UI/Gone: the row matches no file" },listProblems,
				string.Join("; ",listProblems));
			//A file two rows match fails too.
			listRows.Add(new CoverageRow { Files="A/UI/But*",Package="UI-4a",Note="again",Line=9 });
			Assert.IsTrue(GetCoverageProblems(listRows,new List<string> { "A/UI/Button.cs" }).Any(x => x.StartsWith("A/UI/Button.cs: 2 rows")));
			//Packages: known ones or a reason.
			Assert.AreEqual(0,GetProblemsOfPackage(new CoverageRow { Files="x",Package="UI-2a,UI-3",Note="n" }).Count);
			Assert.AreEqual(0,GetProblemsOfPackage(new CoverageRow { Files="x",Package="no-drawing",Note="n" }).Count);
			Assert.AreEqual(1,GetProblemsOfPackage(new CoverageRow { Files="x",Package="UI-99",Note="n" }).Count);
			Assert.AreEqual(1,GetProblemsOfPackage(new CoverageRow { Files="x",Package="keeps,UI-4a",Note="n" }).Count);
			Assert.AreEqual(1,GetProblemsOfPackage(new CoverageRow { Files="x",Package="keeps",Note="" }).Count,"A reason needs its note.");
		}

		public class CoverageRow {
			public string Files="";
			public string Package="";
			public string Note="";
			public int Line;
		}

		public static List<CoverageRow> ReadCoverage(string[] arrayLines) {
			List<CoverageRow> listRows=new List<CoverageRow>();
			bool isHeaderSeen=false;
			for(int i=0;i<arrayLines.Length;i++) {
				string line=arrayLines[i];
				if(line.Trim()=="" || line.StartsWith("#")) {
					continue;
				}
				string[] arrayFields=line.Split('\t');
				if(!isHeaderSeen) {
					isHeaderSeen=true;
					Assert.AreEqual("files\tpackage\tnote",line,"coverage.tsv's header.");
					continue;
				}
				Assert.AreEqual(3,arrayFields.Length,"coverage.tsv line "+(i+1)+" needs files, package and note: "+line);
				listRows.Add(new CoverageRow { Files=arrayFields[0].Trim(),Package=arrayFields[1].Trim(),Note=arrayFields[2].Trim(),Line=i+1 });
			}
			return listRows;
		}

		///<summary>A file's path without its extensions: a designer, .resx, .xaml and .xaml.cs file goes with its main file.</summary>
		public static string GetMainName(string pathRelative) {
			string[] arrayEndings=new string[] { ".designer.cs",".xaml.cs",".cs",".resx",".xaml" };
			foreach(string ending in arrayEndings) {
				if(pathRelative.EndsWith(ending,StringComparison.OrdinalIgnoreCase)) {
					return pathRelative.Substring(0,pathRelative.Length-ending.Length);
				}
			}
			int indexDot=pathRelative.LastIndexOf('.');
			if(indexDot>pathRelative.LastIndexOf('/')) {
				return pathRelative.Substring(0,indexDot);
			}
			return pathRelative;
		}

		private static Regex GetRegexOfPattern(string pattern) {
			return new Regex("^"+Regex.Escape(pattern).Replace(@"\*",".*")+"$",RegexOptions.IgnoreCase);
		}

		public static List<string> GetCoverageProblems(List<CoverageRow> listRows,List<string> listFiles) {
			List<string> listProblems=new List<string>();
			List<Tuple<CoverageRow,Regex>> listPatterns=listRows.Select(x => Tuple.Create(x,GetRegexOfPattern(x.Files))).ToList();
			HashSet<CoverageRow> hashSetUsed=new HashSet<CoverageRow>();
			foreach(string file in listFiles.OrderBy(x => x,StringComparer.OrdinalIgnoreCase)) {
				string nameMain=GetMainName(file);
				List<CoverageRow> listMatches=listPatterns.Where(x => x.Item2.IsMatch(nameMain)).Select(x => x.Item1).ToList();
				if(listMatches.Count==0) {
					listProblems.Add(file+": no row");
					continue;
				}
				if(listMatches.Count>1) {
					listProblems.Add(file+": "+listMatches.Count+" rows ("+string.Join(", ",listMatches.Select(x => x.Files))+")");
				}
				foreach(CoverageRow coverageRow in listMatches) {
					hashSetUsed.Add(coverageRow);
				}
			}
			foreach(CoverageRow coverageRow in listRows.Where(x => !hashSetUsed.Contains(x))) {
				listProblems.Add(coverageRow.Files+": the row matches no file");
			}
			return listProblems;
		}

		public static List<string> GetProblemsOfPackage(CoverageRow coverageRow) {
			List<string> listProblems=new List<string>();
			string[] arrayParts=coverageRow.Package.Split(',').Select(x => x.Trim()).ToArray();
			if(arrayParts.Length==1 && ArrayReasons.Contains(arrayParts[0])) {
				if(coverageRow.Note=="") {
					listProblems.Add(coverageRow.Files+": "+arrayParts[0]+" needs a note that says why");
				}
				return listProblems;
			}
			foreach(string part in arrayParts) {
				if(!ArrayPackages.Contains(part)) {
					listProblems.Add(coverageRow.Files+": '"+part+"' is not a package of section 7 ("+string.Join(", ",ArrayPackages)+") and not alone as "
						+string.Join(" or ",ArrayReasons));
				}
			}
			return listProblems;
		}
		#endregion Coverage

		#region Colour literals
		[TestMethod]
		public void ColourLiterals_NoNewOnesInTheSharedControls() {
			string pathRoot=RemovedAssembliesTests.GetRepositoryRoot();
			SortedDictionary<string,int> dictFound=FindColourLiteralsInScopes(pathRoot);
			Assert.IsTrue(dictFound.Count>50,"Expected today's colour literals in the shared controls, found "+dictFound.Count+".");
			Assert.IsTrue(dictFound.Keys.Any(x => x.StartsWith("OpenPmsDentalBusiness/UI/GridOD.cs\t")),"The scan must read GridOD.cs.");
			Assert.IsTrue(dictFound.Keys.Any(x => x.StartsWith("WpfControlsOD/UI/Controls/") && x.Split('\t')[0].EndsWith(".xaml")),"The scan must read the XAML.");
			string pathList=Path.Combine(pathRoot,PATH_COLOUR_LITERALS);
			if(IsWriteRequested()) {
				WriteColourLiterals(pathList,dictFound);
			}
			Assert.IsTrue(File.Exists(pathList),"No allow list "+PATH_COLOUR_LITERALS+"; "+VARIABLE_WRITE_COLOUR_LITERALS+"=1 writes it.");
			SortedDictionary<string,int> dictAllowed=ReadColourLiterals(File.ReadAllLines(pathList));
			List<string> listProblems=CompareColourLiterals(dictFound,dictAllowed);
			Assert.AreEqual(0,listProblems.Count,"Colour literals in the shared controls (section 9 of docs/specs/ui-refresh-plan.md): use a UiTheme token for a new "
				+"colour; when a package removes literals, cut the allow list ("+VARIABLE_WRITE_COLOUR_LITERALS+"=1 writes "+PATH_COLOUR_LITERALS+" from the source):\r\n"
				+string.Join("\r\n",listProblems));
		}

		[TestMethod]
		public void ColourLiterals_Rules_FindFromArgbAndHex() {
			List<string> listCs=FindColourLiterals("X.cs",new string[] {
				"Color colorA=Color.FromArgb(65, 94, 154);//Color.FromArgb(1,2,3) in a comment",
				"/* Color.FromArgb(9,9,9) */ Brush b=new SolidBrush(System.Drawing.Color.FromArgb(",
				"  200,Color.FromArgb(1,2,3)));",
				"var c=ColorTranslator.FromHtml(\"#3C5A96\"); string s=\"#region\"; string t=\"not #FFF in a sentence\";",
				"media=Color.FromRgb(240,240,240);" });
			CollectionAssert.AreEquivalent(new[] { "Color.FromArgb(65,94,154)","Color.FromArgb(200,Color.FromArgb(1,2,3))","Color.FromArgb(1,2,3)",
				"#3C5A96","Color.FromRgb(240,240,240)" },listCs,string.Join("; ",listCs));
			List<string> listXaml=FindColourLiterals("X.xaml",new string[] {
				"<Grid Background=\"#FFFCFDFE\" > <!-- Background=\"#FF000000\" -->",
				"<SolidColorBrush x:Key=\"k\" Color=\"#c0c0c0\"/><Color>#FFF</Color> <TextBlock Text=\"#1\"/>" });
			CollectionAssert.AreEquivalent(new[] { "#FFFCFDFE","#C0C0C0","#FFF" },listXaml,string.Join("; ",listXaml));
			//The compare: more than allowed fails, fewer than allowed fails (cut the list), equal passes.
			SortedDictionary<string,int> dictAllowed=new SortedDictionary<string,int>(StringComparer.Ordinal) { { "a.cs\t#FFF",2 },{ "b.cs\t#000",1 } };
			Assert.AreEqual(0,CompareColourLiterals(new SortedDictionary<string,int>(dictAllowed,StringComparer.Ordinal),dictAllowed).Count);
			List<string> listProblems=CompareColourLiterals(new SortedDictionary<string,int>(StringComparer.Ordinal) { { "a.cs\t#FFF",3 },{ "c.cs\t#111",1 } },dictAllowed);
			Assert.AreEqual(3,listProblems.Count,string.Join("; ",listProblems));
			Assert.IsTrue(listProblems.Any(x => x.StartsWith("a.cs: 1 new #FFF")));
			Assert.IsTrue(listProblems.Any(x => x.StartsWith("c.cs: 1 new #111")));
			Assert.IsTrue(listProblems.Any(x => x.StartsWith("b.cs: #000 is gone")));
		}

		private static bool IsWriteRequested() {
			string value=Environment.GetEnvironmentVariable(VARIABLE_WRITE_COLOUR_LITERALS);
			return value!=null && value.Trim()=="1";
		}

		///<summary>Every colour literal in the scopes, as "path\tliteral" (path relative, forward slashes) to its count.</summary>
		public static SortedDictionary<string,int> FindColourLiteralsInScopes(string pathRoot) {
			SortedDictionary<string,int> dictFound=new SortedDictionary<string,int>(StringComparer.Ordinal);
			List<string> listPaths=new List<string>();
			foreach(string scope in ArrayColourScopes) {
				string pathScope=Path.Combine(pathRoot,scope);
				if(File.Exists(pathScope)) {
					listPaths.Add(pathScope);
					continue;
				}
				Assert.IsTrue(Directory.Exists(pathScope),"No scope "+scope+".");
				listPaths.AddRange(Directory.EnumerateFiles(pathScope,"*",SearchOption.AllDirectories)
					.Where(x => x.EndsWith(".cs",StringComparison.OrdinalIgnoreCase) || x.EndsWith(".xaml",StringComparison.OrdinalIgnoreCase)));
			}
			foreach(string path in listPaths) {
				string pathRelative=path.Substring(pathRoot.Length+1).Replace('\\','/');
				foreach(string literal in FindColourLiterals(path,File.ReadAllLines(path))) {
					string key=pathRelative+"\t"+literal;
					int count;
					dictFound.TryGetValue(key,out count);
					dictFound[key]=count+1;
				}
			}
			return dictFound;
		}

		private static readonly Regex _regexFromArgb=new Regex(@"\bColor\s*\.\s*From(?:Argb|Rgb|ScRgb)\s*\(");
		private static readonly Regex _regexHexInString=new Regex(@"""#(?<hex>[0-9A-Fa-f]{8}|[0-9A-Fa-f]{6}|[0-9A-Fa-f]{3})""");
		private static readonly Regex _regexHexInXaml=new Regex(@"(?<=[""'>\s])#(?<hex>[0-9A-Fa-f]{8}|[0-9A-Fa-f]{6}|[0-9A-Fa-f]{4}|[0-9A-Fa-f]{3})(?=[""'<\s])");
		private static readonly Regex _regexXamlComment=new Regex(@"<!--.*?-->",RegexOptions.Singleline);

		///<summary>The colour literals of one file: each Color.FromArgb, FromRgb and FromScRgb call with its arguments (whitespace removed; a call
		///inside another counts on its own as well), each hex colour string in C# ("#RGB", "#RRGGBB", "#AARRGGBB"), and each hex colour in XAML.
		///Comments are not code.</summary>
		public static List<string> FindColourLiterals(string path,string[] arrayLines) {
			List<string> listLiterals=new List<string>();
			if(path.EndsWith(".xaml",StringComparison.OrdinalIgnoreCase)) {
				string text=_regexXamlComment.Replace(string.Join("\n",arrayLines),"");
				foreach(Match match in _regexHexInXaml.Matches(text)) {
					listLiterals.Add("#"+match.Groups["hex"].Value.ToUpperInvariant());
				}
				return listLiterals;
			}
			string code=string.Join("\n",UiSourceText.RemoveComments(arrayLines,isKeepingStrings:true));
			foreach(Match match in _regexFromArgb.Matches(code)) {
				int indexOpen=match.Index+match.Length-1;
				int indexClose=UiSourceText.FindClosingParen(code,indexOpen);
				if(indexClose<0) {
					continue;
				}
				string call=code.Substring(match.Index,indexClose-match.Index+1);
				listLiterals.Add(Regex.Replace(call,@"\s+",""));
			}
			foreach(Match match in _regexHexInString.Matches(code)) {
				listLiterals.Add("#"+match.Groups["hex"].Value.ToUpperInvariant());
			}
			return listLiterals;
		}

		public static SortedDictionary<string,int> ReadColourLiterals(string[] arrayLines) {
			SortedDictionary<string,int> dictAllowed=new SortedDictionary<string,int>(StringComparer.Ordinal);
			bool isHeaderSeen=false;
			for(int i=0;i<arrayLines.Length;i++) {
				string line=arrayLines[i];
				if(line.StartsWith(ChromeColourScanTests.SECTION_MARK,StringComparison.Ordinal)) {
					break;//UI-11's section (ChromeColourScanTests) follows the table
				}
				if(line.Trim()=="" || line.StartsWith("#")) {
					continue;
				}
				if(!isHeaderSeen) {
					isHeaderSeen=true;
					Assert.AreEqual("file\tliteral\tcount",line,"colour-literals.tsv's header.");
					continue;
				}
				string[] arrayFields=line.Split('\t');
				Assert.AreEqual(3,arrayFields.Length,"colour-literals.tsv line "+(i+1)+": "+line);
				dictAllowed[arrayFields[0]+"\t"+arrayFields[1]]=int.Parse(arrayFields[2]);
			}
			return dictAllowed;
		}

		public static List<string> CompareColourLiterals(SortedDictionary<string,int> dictFound,SortedDictionary<string,int> dictAllowed) {
			List<string> listProblems=new List<string>();
			foreach(KeyValuePair<string,int> pair in dictFound) {
				int countAllowed;
				dictAllowed.TryGetValue(pair.Key,out countAllowed);
				if(pair.Value>countAllowed) {
					string[] arrayParts=pair.Key.Split('\t');
					listProblems.Add(arrayParts[0]+": "+(pair.Value-countAllowed)+" new "+arrayParts[1]);
				}
			}
			foreach(KeyValuePair<string,int> pair in dictAllowed) {
				int countFound;
				dictFound.TryGetValue(pair.Key,out countFound);
				if(countFound<pair.Value) {
					string[] arrayParts=pair.Key.Split('\t');
					listProblems.Add(arrayParts[0]+": "+arrayParts[1]+" is gone "+(pair.Value-countFound)+" time(s); cut the allow list");
				}
			}
			return listProblems;
		}

		///<summary>Writes the allow list from the source, keeping the line ends the file already has (LF for a new file), UTF-8 without a BOM. UI-11's
		///section (ChromeColourScanTests, written by tools/UiFit/colour_literals.py) stays as it is after the table.</summary>
		private static void WriteColourLiterals(string pathList,SortedDictionary<string,int> dictFound) {
			string lineEnd="\n";
			string section="";
			if(File.Exists(pathList)) {
				string textOld=File.ReadAllText(pathList);
				if(textOld.Contains("\r\n")) {
					lineEnd="\r\n";
				}
				int indexSection=textOld.IndexOf(ChromeColourScanTests.SECTION_MARK,StringComparison.Ordinal);
				if(indexSection>=0) {
					section=textOld.Substring(indexSection);
				}
			}
			StringBuilder stringBuilder=new StringBuilder();
			foreach(string line in new string[] {
				"# The colour literals the shared controls have today (docs/specs/ui-refresh-plan.md section 9; package UI-1): Color.FromArgb, FromRgb and",
				"# FromScRgb calls and hex colours in OpenPmsDental/UI, OpenPmsDentalBusiness/UI/GridOD.cs and WpfControlsOD/UI/Controls. NoHqTests'",
				"# UiThemeScanTests fails on a literal that is not here (use a UiTheme token) and on a row the source no longer has as often (cut the list):",
				"# each control package shrinks it. OPMS_UI_COLOUR_LITERALS_WRITE=1 writes it again from the source.",
				"# file: relative to the repository root, forward slashes. literal: the call without whitespace, or the hex colour in capitals. count: how often.",
				"file\tliteral\tcount" })
			{
				stringBuilder.Append(line).Append(lineEnd);
			}
			foreach(KeyValuePair<string,int> pair in dictFound) {
				stringBuilder.Append(pair.Key).Append('\t').Append(pair.Value).Append(lineEnd);
			}
			stringBuilder.Append(section);
			File.WriteAllText(pathList,stringBuilder.ToString(),new UTF8Encoding(false));
		}
		#endregion Colour literals

		#region Pens and brushes
		[TestMethod]
		public void UiThemePensAndBrushes_AreNeverDisposed() {
			string pathRoot=RemovedAssembliesTests.GetRepositoryRoot();
			List<string> listFindings=new List<string>();
			int countFiles=0;
			int countUsing=0;
			foreach(string path in Directory.EnumerateFiles(pathRoot,"*.cs",SearchOption.AllDirectories)) {
				string pathRelative=path.Substring(pathRoot.Length+1);
				if(IsSkippedFolder(pathRelative)) {
					continue;
				}
				countFiles++;
				string text=File.ReadAllText(path);
				if(!text.Contains("UiTheme")) {
					continue;
				}
				countUsing++;
				foreach(string finding in FindDisposedUiThemePens(text.Split('\n'))) {
					listFindings.Add(pathRelative+": "+finding);
				}
			}
			Assert.IsTrue(countFiles>5000,"Expected the repository's source files, found "+countFiles+".");
			Assert.IsTrue(countUsing>=4,"Expected UiTheme's own files at least, found "+countUsing+".");
			Assert.AreEqual(0,listFindings.Count,"UiTheme.GetPen and GetBrush hand out this thread's shared pen or brush: never wrap it in using and never dispose "
				+"it (section 3.2 of docs/specs/ui-refresh-plan.md); for a pen of your own, new Pen(UiTheme.GetColor(...)):\r\n"+string.Join("\r\n",listFindings));
		}

		[TestMethod]
		public void UiThemePenScan_Rules_FindUsingAndDispose() {
			string[] arrayBad=new string[] {
				"using(Pen pen=UiTheme.GetPen(EnumUiColor.Stroke)) { g.DrawLine(pen,0,0,1,1); }",
				"using (SolidBrush brush = CodeBase.UiTheme.GetBrush(EnumUiColor.Accent)) {}",
				"using(UiTheme.GetBrush(EnumUiColor.Accent)) {}",
				"using var pen=UiTheme.GetPen(EnumUiColor.Stroke,2);",
				"using Pen penFocus = UiTheme.GetPen(EnumUiColor.Accent, widthFocus);",
				"UiTheme.GetPen(EnumUiColor.Stroke).Dispose();",
				"Pen penOutline=UiTheme.GetPen(EnumUiColor.Stroke); g.DrawRectangle(penOutline,r); penOutline.Dispose();",
				"_brushTitle=UiTheme.GetBrush(EnumUiColor.Accent);\n\n_brushTitle?.Dispose();",
				"Brush brushRow=UiTheme.GetBrush(EnumUiColor.RowHover);\nusing(brushRow) {}" };
			foreach(string sample in arrayBad) {
				Assert.AreEqual(1,FindDisposedUiThemePens(sample.Split('\n')).Count,"Must be found: "+sample);
			}
			string[] arrayGood=new string[] {
				"g.DrawLine(UiTheme.GetPen(EnumUiColor.Stroke),0,0,1,1);",
				"Pen pen=UiTheme.GetPen(EnumUiColor.Stroke); g.DrawRectangle(pen,r);",
				"using(Pen pen=new Pen(UiTheme.GetColor(EnumUiColor.Stroke))) { }",
				"using(GraphicsPath path=UiTheme.GetRoundedRectanglePath(r,4)) { g.FillPath(UiTheme.GetBrush(EnumUiColor.Accent),path); }",
				"//using(Pen pen=UiTheme.GetPen(EnumUiColor.Stroke)) {} in a comment",
				"string s=\"UiTheme.GetPen(EnumUiColor.Stroke).Dispose()\";",
				"Pen penMine=new Pen(Color.Red); penMine.Dispose(); Pen pen2=UiTheme.GetPen(EnumUiColor.Stroke);" };
			foreach(string sample in arrayGood) {
				Assert.AreEqual(0,FindDisposedUiThemePens(sample.Split('\n')).Count,"Must not be found: "+sample);
			}
		}

		private const string GET=@"(?:CodeBase\s*\.\s*)?UiTheme\s*\.\s*Get(?:Pen|Brush)\b";
		private static readonly Regex _regexUsingStatement=new Regex(@"\busing\s*\(\s*(?:[\w\.<>]+\s+\w+\s*=\s*)?"+GET);
		private static readonly Regex _regexUsingDeclaration=new Regex(@"\busing\s+[\w\.<>]+\s+\w+\s*=\s*"+GET);
		private static readonly Regex _regexDisposeDirect=new Regex(GET+@"\s*\([^;{}]*?\)\s*\??\s*\.\s*Dispose\s*\(");
		private static readonly Regex _regexAssigned=new Regex(@"(?<name>\b[A-Za-z_]\w*)\s*=\s*"+GET);

		///<summary>The places in a file where a UiTheme pen or brush is wrapped in using or disposed: a using statement or declaration around
		///GetPen or GetBrush, a Dispose on its result, and a variable or field assigned from it that the same file disposes or wraps in using (a name
		///used for another pen as well is found too: give that one another name). Comments and strings are not code.</summary>
		public static List<string> FindDisposedUiThemePens(string[] arrayLines) {
			List<string> listFindings=new List<string>();
			string code=string.Join("\n",UiSourceText.RemoveComments(arrayLines,isKeepingStrings:false));
			foreach(Match match in _regexUsingStatement.Matches(code)) {
				listFindings.Add("line "+GetLine(code,match.Index)+": using around a UiTheme pen or brush");
			}
			foreach(Match match in _regexUsingDeclaration.Matches(code)) {
				listFindings.Add("line "+GetLine(code,match.Index)+": using declaration of a UiTheme pen or brush");
			}
			foreach(Match match in _regexDisposeDirect.Matches(code)) {
				listFindings.Add("line "+GetLine(code,match.Index)+": Dispose of a UiTheme pen or brush");
			}
			HashSet<string> hashSetNames=new HashSet<string>(_regexAssigned.Matches(code).Cast<Match>().Select(x => x.Groups["name"].Value));
			foreach(string name in hashSetNames) {
				Regex regexDispose=new Regex(@"(?<![\w\.])"+Regex.Escape(name)+@"\s*\??\s*\.\s*Dispose\s*\(|\busing\s*\(\s*"+Regex.Escape(name)+@"\s*\)");
				foreach(Match match in regexDispose.Matches(code)) {
					listFindings.Add("line "+GetLine(code,match.Index)+": "+name+" holds a UiTheme pen or brush and is disposed");
				}
			}
			return listFindings;
		}

		private static int GetLine(string code,int index) {
			int line=1;
			for(int i=0;i<index && i<code.Length;i++) {
				if(code[i]=='\n') {
					line++;
				}
			}
			return line;
		}

		///<summary>Build output, the third-party sources and other people's copies are not the repository's code.</summary>
		private static bool IsSkippedFolder(string pathRelative) {
			string[] arrayParts=pathRelative.Split('\\');
			for(int i=0;i<arrayParts.Length-1;i++) {
				string part=arrayParts[i];
				if(part.Equals("bin",StringComparison.OrdinalIgnoreCase) || part.Equals("obj",StringComparison.OrdinalIgnoreCase)
					|| part.Equals(".git",StringComparison.OrdinalIgnoreCase) || part.Equals(".claude",StringComparison.OrdinalIgnoreCase)
					|| part.Equals("packages",StringComparison.OrdinalIgnoreCase) || part.Equals("TestResults",StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}
			return arrayParts[0].Equals("third-party",StringComparison.OrdinalIgnoreCase);
		}
		#endregion Pens and brushes

		#region Linked files
		[TestMethod]
		public void UiThemeWinFormsFiles_UseOnlyWinFormsAndDrawing() {
			string pathRoot=RemovedAssembliesTests.GetRepositoryRoot();
			List<string> listFilesUiTheme=Directory.EnumerateFiles(Path.Combine(pathRoot,"CodeBase","UI"),"UiTheme*.cs")
				.Select(x => x.Substring(pathRoot.Length+1)).ToList();
			List<string> listKnown=ArrayUiThemeWinFormsFiles.Concat(new[] { PATH_UITHEME_WPF }).ToList();
			CollectionAssert.AreEquivalent(listKnown,listFilesUiTheme,"A new UiTheme partial file is either one RdlDesign may link (Windows Forms and "
				+"System.Drawing only: add it to ArrayUiThemeWinFormsFiles) or a WPF one.");
			Regex regexUsing=new Regex(@"^\s*using\s+(?<name>[\w\.]+)\s*;");
			Regex regexWpf=new Regex(@"\bSystem\s*\.\s*Windows\s*\.\s*(?!Forms\b)\w+");
			foreach(string file in ArrayUiThemeWinFormsFiles) {
				string[] arrayLines=File.ReadAllLines(Path.Combine(pathRoot,file));
				string code=string.Join("\n",UiSourceText.RemoveComments(arrayLines,isKeepingStrings:false));
				foreach(string line in code.Split('\n')) {
					Match match=regexUsing.Match(line);
					if(match.Success) {
						Assert.IsTrue(_arrayUsingsWinForms.Contains(match.Groups["name"].Value),file+" uses "+match.Groups["name"].Value
							+"; RdlDesign links this file, so it may use only "+string.Join(", ",_arrayUsingsWinForms)+".");
					}
				}
				Assert.IsFalse(regexWpf.IsMatch(code),file+" names WPF (System.Windows other than Forms): the WPF part belongs in "+PATH_UITHEME_WPF+".");
				Assert.IsTrue(code.Contains("namespace CodeBase"),file);
			}
			string codeWpf=File.ReadAllText(Path.Combine(pathRoot,PATH_UITHEME_WPF));
			Assert.IsTrue(codeWpf.Contains("using System.Windows.Media;"),"The WPF brushes are in "+PATH_UITHEME_WPF+".");
		}
		#endregion Linked files
	}

	///<summary>C# source text for the UI-1 scans: comments removed, strings kept or emptied (the quotes stay), per line.</summary>
	public static class UiSourceText {
		public static string[] RemoveComments(string[] arrayLines,bool isKeepingStrings) {
			string[] arrayResult=new string[arrayLines.Length];
			bool isInBlockComment=false;
			bool isInVerbatimString=false;
			for(int index=0;index<arrayLines.Length;index++) {
				string line=arrayLines[index].TrimEnd('\r');
				StringBuilder stringBuilder=new StringBuilder();
				int i=0;
				while(i<line.Length) {
					if(isInBlockComment) {
						int indexEnd=line.IndexOf("*/",i,StringComparison.Ordinal);
						if(indexEnd<0) {
							i=line.Length;
							continue;
						}
						isInBlockComment=false;
						i=indexEnd+2;
						continue;
					}
					if(isInVerbatimString) {
						if(line[i]=='"') {
							if(i+1<line.Length && line[i+1]=='"') {
								if(isKeepingStrings) {
									stringBuilder.Append("\"\"");
								}
								i+=2;
								continue;
							}
							isInVerbatimString=false;
							stringBuilder.Append('"');
						}
						else if(isKeepingStrings) {
							stringBuilder.Append(line[i]);
						}
						i++;
						continue;
					}
					char c=line[i];
					if(c=='/' && i+1<line.Length && line[i+1]=='/') {
						break;
					}
					if(c=='/' && i+1<line.Length && line[i+1]=='*') {
						isInBlockComment=true;
						i+=2;
						continue;
					}
					if(c=='@' && i+1<line.Length && line[i+1]=='"') {
						stringBuilder.Append("@\"");
						isInVerbatimString=true;
						i+=2;
						continue;
					}
					if((c=='$' && i+2<line.Length && line[i+1]=='@' && line[i+2]=='"') || (c=='@' && i+2<line.Length && line[i+1]=='$' && line[i+2]=='"')) {
						stringBuilder.Append("@\"");
						isInVerbatimString=true;
						i+=3;
						continue;
					}
					if(c=='"' || c=='\'') {
						stringBuilder.Append(c);
						i++;
						while(i<line.Length && line[i]!=c) {
							if(line[i]=='\\') {
								if(isKeepingStrings && i+1<line.Length) {
									stringBuilder.Append(line[i]).Append(line[i+1]);
								}
								i+=2;
								continue;
							}
							if(isKeepingStrings) {
								stringBuilder.Append(line[i]);
							}
							i++;
						}
						if(i<line.Length) {
							stringBuilder.Append(c);
						}
						i++;
						continue;
					}
					stringBuilder.Append(c);
					i++;
				}
				arrayResult[index]=stringBuilder.ToString();
			}
			return arrayResult;
		}

		///<summary>The index of the parenthesis that closes the one at indexOpen, or -1.</summary>
		public static int FindClosingParen(string code,int indexOpen) {
			int depth=0;
			for(int i=indexOpen;i<code.Length;i++) {
				if(code[i]=='(') {
					depth++;
				}
				else if(code[i]==')') {
					depth--;
					if(depth==0) {
						return i;
					}
				}
			}
			return -1;
		}
	}
}
