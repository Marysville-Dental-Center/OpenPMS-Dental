// The source scan of the UI refresh's package UI-11 (docs/specs/ui-refresh-plan.md section 7, UI-11; docs/port-notes/UI-11.md): every literal
// BackColor and ForeColor value (a named colour, a system colour, Color.FromArgb with numbers, Color.Empty) that the designers and the code of
// the program's projects set must be listed with its reason in the UI-11 section of tests\UiRefreshTests\Data\colour-literals.tsv. A chrome
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
		private const string VALUE=@"(?:System\.Drawing\.)?(?:Color|SystemColors)\.(?:FromArgb\((?:[\s\d,()]|\(int\)|\(byte\))*\)|\w+)";
		private static readonly Regex _regexDesigner=new Regex(@"^\s*this\.(?:(?<ctl>[\w.]+)\.)?(?<prop>BackColor|ForeColor) = (?<val>"+VALUE+@");\s*$");
		private static readonly Regex _regexCode=new Regex(@"(?<lhs>[\w.\[\]""]*?)\b(?<prop>BackColor|ForeColor)\s*=(?!=)\s*(?<val>"+VALUE+@")(?=\s*[;,})])");
		private static readonly Regex _regexCast=new Regex(@"\(\(int\)\(\(\(byte\)\((\d+)\)\)\)\)");

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
					listLiterals.Add(control+"\t"+match.Groups["prop"].Value+"\t"+Normalise(match.Groups["val"].Value));
				}
			}
			return listLiterals;
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
