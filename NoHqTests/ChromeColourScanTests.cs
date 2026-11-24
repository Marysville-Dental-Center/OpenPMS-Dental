// The source scan of the UI refresh's package UI-11 (docs/specs/ui-refresh-plan.md section 7, UI-11; docs/port-notes/UI-11.md): every literal
// BackColor and ForeColor value (a named colour, a system colour, Color.FromArgb with numbers, Color.FromName, Color.FromKnownColor,
// Color.Empty; in code every literal of the right-hand side, each branch of a "?:") that the designers and the code of the program's projects
// set must be listed with its reason in the UI-11 section of tests\UiRefreshTests\Data\colour-literals.tsv. A chrome
// literal (a window, panel, group box, tab page, label or check box in an old background: Windows' grey, white, 252,253,254) is not listed: the
// control takes its token from its parent and the looks (remove the designer line; in code use UiTheme). So a new chrome literal fails here, and
// so does a row the source no longer has as often (cut the list). tools\UiFit\colour_literals.py reads the same lines, decides each by its
// rules and writes the section; this test reads them on its own, with the same patterns.
// It needs no build, database or network, and starts nothing.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NoHqTests {
	[TestClass]
	public class ChromeColourScanTests {
		///<summary>The first line of the UI-11 section in colour-literals.tsv; the UI-1 table above it ends there.</summary>
		public const string SECTION_MARK="# UI-11: the chrome colour literals";
		public const string SECTION_HEADER="file\tcontrol\tproperty\tvalue\tcount\twhy";
		///<summary>The projects whose windows the scan reads (tools\UiFit\colour_literals.py's ROOTS).</summary>
		public static readonly string[] ArrayRoots=new string[] { "OpenPmsDental","OpenPmsDentalBusiness","CodeBase","SparksToothChart","WpfControlsOD",
			"RdlDesign","ODR","ImagingDeviceManager","CentralManager","DatabaseIntegrityCheck","DocumentationBuilder","MySQLManager",
			"OpenPmsDentalDbSettings","OpenPmsDentalGraph","OpenPmsDentalHL7","ServiceManager","UpdateFileCopier","xCrudGenerator" };
		///<summary>A literal value: a named or system colour, Color.FromArgb with numbers, Color.FromName("..."), Color.FromKnownColor(...). Not a
		///member of something else (x.Color.Red) and not a method call (Color.FromArgb(alpha,color)).</summary>
		private const string VALUE=@"(?<![\w.])(?:System\.Drawing\.)?(?:Color|SystemColors)\.(?:FromArgb\((?:[\s\d,()]|\(int\)|\(byte\))*\)"
			+@"|FromName\(""[^""]*""\)|FromKnownColor\([\w.]+\)|\w+\b(?!\s*\())";
		private static readonly Regex _regexDesigner=new Regex(@"^\s*this\.(?:(?<ctl>[\w.]+)\.)?(?<prop>BackColor|ForeColor) = (?<val>"+VALUE+@");\s*$");
		///<summary>The code's assignment; its right-hand side (RightHandSide) holds the literals, each branch of a "?:" its own.</summary>
		private static readonly Regex _regexCode=new Regex(@"(?<lhs>[\w.\[\]""]*?)\b(?<prop>BackColor|ForeColor)\s*=(?!=)\s*");
		private static readonly Regex _regexLiteral=new Regex(VALUE);
		private static readonly Regex _regexCast=new Regex(@"\(\(int\)\(\(\(byte\)\((\d+)\)\)\)\)");
		///<summary>The designer's control types that cannot show a transparent colour (no ControlStyles.SupportsTransparentBackColor): under a
		///transparent parent and without their own colour WinForms draws Windows' grey (UI.GroupBox white) in their place.</summary>
		public static readonly string[] ArrayTypesNotTransparent=new string[] { "OpenPmsDental.UI.CheckBox","OpenPmsDental.UI.GroupBox",
			"OpenPmsDental.UI.TabControl","System.Windows.Forms.TabControl" };
		private static readonly Regex _regexNew=new Regex(@"^\s*this\.(?<name>\w+) = new (?<type>[\w.]+)\(");
		private static readonly Regex _regexAdd=new Regex(@"^\s*this\.(?:(?<parent>[\w.]+)\.)?Controls\.Add\(this\.(?<child>\w+)\);");

		[TestMethod]
		public void ChromeColours_EveryLiteralIsListedWithItsReason() {
			string pathRoot=RemovedAssembliesTests.GetRepositoryRoot();
			SortedDictionary<string,int> dictFound=FindLiteralsInRoots(pathRoot);
			Assert.IsTrue(dictFound.Count>300,"Expected the windows' colour literals, found "+dictFound.Count+".");
			Assert.IsTrue(dictFound.Keys.Any(x => x.StartsWith("OpenPmsDental/User Controls/ContrPerio.cs\t")),"The scan must read the code.");
			Assert.IsTrue(dictFound.Keys.Any(x => x.StartsWith("RdlDesign/")),"The scan must read RdlDesign's designer code.");
			string pathList=Path.Combine(pathRoot,UiThemeScanTests.PATH_COLOUR_LITERALS);
			List<string> listProblems=new List<string>();
			SortedDictionary<string,int> dictAllowed=ReadSection(File.ReadAllLines(pathList),listProblems);
			listProblems.AddRange(UiThemeScanTests.CompareColourLiterals(dictFound,dictAllowed));
			Assert.AreEqual(0,listProblems.Count,"Colour literals in the windows (UI-11, docs/port-notes/UI-11.md): a chrome colour comes from its token "
				+"(remove the designer line so the control takes its parent's colour, or use UiTheme in code); a colour that stays (content, a user "
				+"setting, a status) needs its row with the reason; python tools/UiFit/colour_literals.py writes the section of "
				+UiThemeScanTests.PATH_COLOUR_LITERALS+":\r\n"+string.Join("\r\n",listProblems));
		}

		///<summary>UI-11 fix round: a control that cannot show a transparent colour (UI.CheckBox, UI.GroupBox, a tab control) takes no token from a
		///transparent parent: WinForms draws Windows' grey (UI.GroupBox white) in its place. So a designer that leaves such a control without its own
		///colour under a Color.Transparent container fails here: remove the container's Transparent line, so the control takes the token.</summary>
		[TestMethod]
		public void ChromeColours_NoOpaqueControlOnTransparentParent() {
			string pathRoot=RemovedAssembliesTests.GetRepositoryRoot();
			List<string> listProblems=new List<string>();
			int countFiles=0;
			foreach(string root in ArrayRoots) {
				string pathFolder=Path.Combine(pathRoot,root);
				if(!Directory.Exists(pathFolder)) {
					continue;
				}
				foreach(string path in Directory.EnumerateFiles(pathFolder,"*.cs",SearchOption.AllDirectories)) {
					string pathRelative=path.Substring(pathRoot.Length+1).Replace('\\','/');
					if(pathRelative.Split('/').Any(x => x.Equals("bin",StringComparison.OrdinalIgnoreCase) || x.Equals("obj",StringComparison.OrdinalIgnoreCase))) {
						continue;
					}
					string text=File.ReadAllText(path);
					if(!text.Contains("Color.Transparent") || !text.Contains("void InitializeComponent()")) {
						continue;
					}
					countFiles++;
					foreach(string found in FindOpaqueOnTransparent(text.Split('\n'))) {
						listProblems.Add(pathRelative+"\t"+found);
					}
				}
			}
			Assert.IsTrue(countFiles>10,"Expected designers with a transparent colour, found "+countFiles+".");
			Assert.AreEqual(0,listProblems.Count,"A control that cannot show a transparent colour sits without its own colour under a transparent "
				+"parent (file, control, type, parent): WinForms draws Windows' grey there. Remove the parent's Transparent line (UI-11, docs/port-notes/UI-11.md):\r\n"
				+string.Join("\r\n",listProblems));
		}

		[TestMethod]
		public void ChromeColours_Rules_FindDesignerAndCodeLiterals() {
			string[] arrayDesigner=new string[] {
				"\t\tprivate void InitializeComponent() {",
				"\t\t\tthis.groupBox1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(252)))), ((int)(((byte)(253)))), ((int)(((byte)(254)))));",
				"\t\t\tthis.label1.ForeColor = System.Drawing.Color.Red;\r",
				"\t\t\tthis.BackColor = System.Drawing.SystemColors.Control;",
				"\t\t\tthis.butColor.BackColor = colorFromDef;",
				"\t\t\t//this.label2.ForeColor = System.Drawing.Color.Blue;",
				"\t\t}",
				"\t\tprivate void Fill() {",
				"\t\t\tlabel.BackColor=Color.White; textA.ForeColor = SystemColors.GrayText;",
				"\t\t\tif(label.BackColor==Color.White) { }",
				"\t\t\tthis.BackColor=Color.Empty;",
				"\t\t\tnew Label { BackColor=Color.FromArgb(14,16,18),ForeColor=Color.White };",
				"\t\t}" };
			List<string> listFound=FindLiterals("A/FormX.Designer.cs",arrayDesigner);
			CollectionAssert.AreEqual(new[] {
				"groupBox1\tBackColor\tColor.FromArgb(252,253,254)",
				"label1\tForeColor\tColor.Red",
				".\tBackColor\tSystemColors.Control",
				"label\tBackColor\tColor.White",
				"textA\tForeColor\tSystemColors.GrayText",
				"this\tBackColor\tColor.Empty",
				"(initializer)\tBackColor\tColor.FromArgb(14,16,18)",
				"(initializer)\tForeColor\tColor.White" },listFound,string.Join("; ",listFound));
			//Outside InitializeComponent a designer-style line is code: its control is the expression without "this."
			List<string> listCode=FindLiterals("A/FormX.cs",new string[] { "\t\t\tthis.panel1.BackColor = System.Drawing.SystemColors.Window;" });
			CollectionAssert.AreEqual(new[] { "panel1\tBackColor\tSystemColors.Window" },listCode,string.Join("; ",listCode));
			//Every literal of the right-hand side, each branch of a "?:" its own row; Color.FromName and Color.FromKnownColor; not a colour that is
			//only an argument's part of a call (Color.FromArgb(alpha,...)'s name) or a member of something else.
			listCode=FindLiterals("A/FormX.cs",new string[] {
				"\t\t\tlabel.BackColor=isX?Color.White:SystemColors.Control;",
				"\t\t\tbutM.BackColor=Color.FromName(\"Control\");",
				"\t\t\tthis.ForeColor = this.Enabled? Color.Black: Color.Gray;",
				"\t\t\tpanel.BackColor=Color.FromKnownColor(KnownColor.Window);",
				"\t\t\tlabelInfo.ForeColor=isA ? Color.Firebrick : (isB ? Color.Blue : SystemColors.ControlText);",
				"\t\t\tpanel.BackColor=Color.FromArgb(alpha,colorDef); label.ForeColor=theme.Color.Red;",
				"\t\t\tnew Label { BackColor=isDark?Color.Black:Color.White,ForeColor=Color.Red };" });
			CollectionAssert.AreEqual(new[] {
				"label\tBackColor\tColor.White",
				"label\tBackColor\tSystemColors.Control",
				"butM\tBackColor\tColor.FromName(\"Control\")",
				"this\tForeColor\tColor.Black",
				"this\tForeColor\tColor.Gray",
				"panel\tBackColor\tColor.FromKnownColor(KnownColor.Window)",
				"labelInfo\tForeColor\tColor.Firebrick",
				"labelInfo\tForeColor\tColor.Blue",
				"labelInfo\tForeColor\tSystemColors.ControlText",
				"(initializer)\tBackColor\tColor.Black",
				"(initializer)\tBackColor\tColor.White",
				"(initializer)\tForeColor\tColor.Red" },listCode,string.Join("; ",listCode));
			//A UI.CheckBox without its own colour under a transparent tab page; a stock check box (it shows the transparent colour) and a UI.CheckBox
			//with its own colour are not.
			List<string> listOpaque=FindOpaqueOnTransparent(new string[] {
				"\t\t\tthis.tabMain = new OpenPmsDental.UI.TabPage();",
				"\t\t\tthis.checkA = new OpenPmsDental.UI.CheckBox();",
				"\t\t\tthis.checkB = new System.Windows.Forms.CheckBox();",
				"\t\t\tthis.checkC = new OpenPmsDental.UI.CheckBox();",
				"\t\t\tthis.panel1 = new System.Windows.Forms.Panel();",
				"\t\t\tthis.tabMain.BackColor = System.Drawing.Color.Transparent;",
				"\t\t\tthis.tabMain.Controls.Add(this.panel1);",
				"\t\t\tthis.panel1.Controls.Add(this.checkA);",
				"\t\t\tthis.tabMain.Controls.Add(this.checkB);",
				"\t\t\tthis.tabMain.Controls.Add(this.checkC);",
				"\t\t\tthis.checkC.BackColor = System.Drawing.SystemColors.Window;" });
			CollectionAssert.AreEqual(new[] { "checkA\tOpenPmsDental.UI.CheckBox\ttabMain" },listOpaque,string.Join("; ",listOpaque));
			//The section: a row without its reason is a problem.
			List<string> listProblems=new List<string>();
			SortedDictionary<string,int> dictAllowed=ReadSection(new string[] { "file\tliteral\tcount","a.cs\tColor.FromArgb(1,2,3)\t1",SECTION_MARK+" ...",
				"# a comment",SECTION_HEADER,"a.cs\t.\tBackColor\tColor.Red\t2\ta status","b.cs\tlabel1\tForeColor\tColor.Blue\t1\t" },listProblems);
			Assert.AreEqual(2,dictAllowed["a.cs\t.\tBackColor\tColor.Red"]);
			Assert.AreEqual(1,listProblems.Count,string.Join("; ",listProblems));
		}

		///<summary>Every literal under the roots, as "path\tcontrol\tproperty\tvalue" (path relative, forward slashes) to its count.</summary>
		public static SortedDictionary<string,int> FindLiteralsInRoots(string pathRoot) {
			SortedDictionary<string,int> dictFound=new SortedDictionary<string,int>(StringComparer.Ordinal);
			foreach(string root in ArrayRoots) {
				string pathFolder=Path.Combine(pathRoot,root);
				if(!Directory.Exists(pathFolder)) {
					continue;
				}
				foreach(string path in Directory.EnumerateFiles(pathFolder,"*.cs",SearchOption.AllDirectories)) {
					string pathRelative=path.Substring(pathRoot.Length+1).Replace('\\','/');
					string[] arrayParts=pathRelative.Split('/');
					if(arrayParts.Any(x => x.Equals("bin",StringComparison.OrdinalIgnoreCase) || x.Equals("obj",StringComparison.OrdinalIgnoreCase))) {
						continue;
					}
					string text=File.ReadAllText(path);
					if(!text.Contains("BackColor") && !text.Contains("ForeColor")) {
						continue;
					}
					foreach(string literal in FindLiterals(pathRelative,text.Split('\n'))) {
						string key=pathRelative+"\t"+literal;
						int count;
						dictFound.TryGetValue(key,out count);
						dictFound[key]=count+1;
					}
				}
			}
			return dictFound;
		}

		///<summary>The literals of one file as "control\tproperty\tvalue", in order. In InitializeComponent a designer line
		///"this.[control.]BackColor = value;" names its control ("." for the window); everywhere else each "x.BackColor=value" in code, its control the
		///expression before it without "this." ("(initializer)" in an object initializer). A // comment is not code.</summary>
		public static List<string> FindLiterals(string pathRelative,string[] arrayLines) {
			List<string> listLiterals=new List<string>();
			string text=string.Join("\n",arrayLines);
			bool isDesigner=pathRelative.EndsWith(".designer.cs",StringComparison.OrdinalIgnoreCase) || text.Contains("private void InitializeComponent()");
			int lineStart=-1;
			int lineEnd=-1;
			if(isDesigner) {
				GetInitializeComponentRange(arrayLines,out lineStart,out lineEnd);
			}
			for(int i=0;i<arrayLines.Length;i++) {
				string line=arrayLines[i];
				int indexComment=line.IndexOf("//",StringComparison.Ordinal);
				if(indexComment>=0) {
					line=line.Substring(0,indexComment);
				}
				if(lineStart>=0 && i>=lineStart && i<=lineEnd) {
					Match matchDesigner=_regexDesigner.Match(line);
					if(matchDesigner.Success) {
						string control=matchDesigner.Groups["ctl"].Success ? matchDesigner.Groups["ctl"].Value : ".";
						listLiterals.Add(control+"\t"+matchDesigner.Groups["prop"].Value+"\t"+Normalise(matchDesigner.Groups["val"].Value));
						continue;
					}
				}
				foreach(Match match in _regexCode.Matches(line)) {
					string lhs=match.Groups["lhs"].Value.TrimEnd('.');
					if(lhs.StartsWith("this.",StringComparison.Ordinal)) {
						lhs=lhs.Substring(5);
					}
					string control=lhs=="" ? "(initializer)" : lhs;
					foreach(Match matchLiteral in _regexLiteral.Matches(RightHandSide(line,match.Index+match.Length))) {
						listLiterals.Add(control+"\t"+match.Groups["prop"].Value+"\t"+Normalise(matchLiteral.Value));
					}
				}
			}
			return listLiterals;
		}

		///<summary>An assignment's right-hand side from start: up to the statement's ";", or a "," "}" ")" outside its own brackets (an object
		///initializer's next member, a call's end), or the line's end.</summary>
		public static string RightHandSide(string line,int start) {
			int depth=0;
			for(int i=start;i<line.Length;i++) {
				char c=line[i];
				if(c=='(' || c=='[' || c=='{') {
					depth++;
				}
				else if(c==')' || c==']' || c=='}') {
					if(depth==0) {
						return line.Substring(start,i-start);
					}
					depth--;
				}
				else if(c==';' || (c==',' && depth==0)) {
					return line.Substring(start,i-start);
				}
			}
			return line.Substring(start);
		}

		///<summary>The designer's controls of a type that cannot show a transparent colour (ArrayTypesNotTransparent), without a BackColor line of
		///their own, whose nearest ancestor with a BackColor line has Color.Transparent: "control\ttype\tancestor". WinForms draws Windows' grey (UI.GroupBox
		///white) there, not the parent's token.</summary>
		public static List<string> FindOpaqueOnTransparent(string[] arrayLines) {
			Dictionary<string,string> dictTypes=new Dictionary<string,string>();
			Dictionary<string,string> dictParents=new Dictionary<string,string>();
			Dictionary<string,string> dictBack=new Dictionary<string,string>();
			foreach(string lineRaw in arrayLines) {
				string line=lineRaw.TrimEnd('\r');
				Match matchNew=_regexNew.Match(line);
				if(matchNew.Success && !dictTypes.ContainsKey(matchNew.Groups["name"].Value)) {
					string type=matchNew.Groups["type"].Value;
					dictTypes[matchNew.Groups["name"].Value]=type.StartsWith("UI.",StringComparison.Ordinal) ? "OpenPmsDental."+type : type;
				}
				Match matchAdd=_regexAdd.Match(line);
				if(matchAdd.Success) {
					dictParents[matchAdd.Groups["child"].Value]=matchAdd.Groups["parent"].Success ? matchAdd.Groups["parent"].Value : ".";
				}
				Match matchDesigner=_regexDesigner.Match(line);
				if(matchDesigner.Success && matchDesigner.Groups["prop"].Value=="BackColor") {
					dictBack[matchDesigner.Groups["ctl"].Success ? matchDesigner.Groups["ctl"].Value : "."]=Normalise(matchDesigner.Groups["val"].Value);
				}
			}
			List<string> listFound=new List<string>();
			foreach(KeyValuePair<string,string> pair in dictParents) {
				string type;
				if(!dictTypes.TryGetValue(pair.Key,out type) || !ArrayTypesNotTransparent.Contains(type) || dictBack.ContainsKey(pair.Key)) {
					continue;
				}
				string parent=pair.Value;
				for(int depth=0;parent!=null && parent!="." && depth<50;depth++) {
					string back;
					if(dictBack.TryGetValue(parent,out back)) {
						if(back=="Color.Transparent") {
							listFound.Add(pair.Key+"\t"+type+"\t"+parent);
						}
						break;
					}
					dictParents.TryGetValue(parent,out parent);
				}
			}
			listFound.Sort(StringComparer.Ordinal);
			return listFound;
		}

		///<summary>The value as the list writes it: no namespace, no designer casts, no whitespace.</summary>
		public static string Normalise(string value) {
			string result=value.Replace("System.Drawing.","");
			result=_regexCast.Replace(result,"$1");
			return Regex.Replace(result,@"\s+","");
		}

		///<summary>The lines (0-based, inclusive) of the first InitializeComponent's body, by its braces; -1 when the file has none.</summary>
		private static void GetInitializeComponentRange(string[] arrayLines,out int lineStart,out int lineEnd) {
			lineStart=-1;
			lineEnd=-1;
			for(int i=0;i<arrayLines.Length;i++) {
				if(!arrayLines[i].Contains("void InitializeComponent()")) {
					continue;
				}
				lineStart=i;
				int depth=0;
				bool isOpened=false;
				for(int j=i;j<arrayLines.Length;j++) {
					depth+=arrayLines[j].Count(x => x=='{')-arrayLines[j].Count(x => x=='}');
					if(arrayLines[j].Contains("{")) {
						isOpened=true;
					}
					if(isOpened && depth<=0) {
						lineEnd=j;
						return;
					}
				}
				lineEnd=arrayLines.Length-1;
				return;
			}
		}

		///<summary>The UI-11 section's rows as "file\tcontrol\tproperty\tvalue" to their count. A row without its reason, a malformed row or a
		///missing section is a problem.</summary>
		public static SortedDictionary<string,int> ReadSection(string[] arrayLines,List<string> listProblems) {
			SortedDictionary<string,int> dictAllowed=new SortedDictionary<string,int>(StringComparer.Ordinal);
			int indexMark=Array.FindIndex(arrayLines,x => x.StartsWith(SECTION_MARK,StringComparison.Ordinal));
			if(indexMark<0) {
				listProblems.Add("colour-literals.tsv has no UI-11 section ("+SECTION_MARK+").");
				return dictAllowed;
			}
			bool isHeaderSeen=false;
			for(int i=indexMark;i<arrayLines.Length;i++) {
				string line=arrayLines[i].TrimEnd('\r');
				if(line.Trim()=="" || line.StartsWith("#",StringComparison.Ordinal)) {
					continue;
				}
				if(!isHeaderSeen) {
					isHeaderSeen=true;
					if(line!=SECTION_HEADER) {
						listProblems.Add("colour-literals.tsv's UI-11 header: "+line);
					}
					continue;
				}
				string[] arrayFields=line.Split('\t');
				int count;
				if(arrayFields.Length!=6 || !int.TryParse(arrayFields[4],out count)) {
					listProblems.Add("colour-literals.tsv line "+(i+1)+" is not file, control, property, value, count, why: "+line);
					continue;
				}
				if(arrayFields[5].Trim()=="") {
					listProblems.Add("colour-literals.tsv line "+(i+1)+" gives no reason: "+line);
				}
				dictAllowed[arrayFields[0]+"\t"+arrayFields[1]+"\t"+arrayFields[2]+"\t"+arrayFields[3]]=count;
			}
			return dictAllowed;
		}
	}
}
