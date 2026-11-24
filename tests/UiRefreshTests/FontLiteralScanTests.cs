// The "Microsoft Sans Serif" scan and the literal test of the plain forms' control fonts (package UI-2a-4, docs/specs/ui-refresh-plan.md
// sections 5.1, 5.2 and 9, docs/port-notes/UI-2a.md). The scan reads the source of the folders in FontLiteralScan.ArrayFolders (each UI-2a-4
// slice adds its folders there) with comments skipped, and finds every line that names "Microsoft Sans Serif" or FontFamily.GenericSansSerif:
// in code outside InitializeComponent everywhere, and in the designer code (InitializeComponent) of the plain forms, of the FormODBase windows
// whose constructor does not call InitializeLayoutManager and of the UserControls no program window hosts. The designer code of the windows and
// UserControls under LayoutManager is exempt (the mapping of 5.1 changes those fonts at run time); "under LayoutManager" is the tier 1 scan's
// own list (WindowBuilder.GetScanTypes, section 5.4). A site that stays (print, document or content, 5.2) is a row of
// Data\font-literal-allow.tsv; any other is a screen font that goes through its UiTheme token. The control and form fonts of the in-scope
// designer code (this.x.Font, this.Font) are allowed by the scan until UI-2b and checked by the literal test instead: their family and size
// follow FontBody by the rule of 3.4, and a default font (FontBody's family and size, Regular) is removed so the control inherits, where its
// parent has the default font (UI-2a-5: the parent's font comes from the designer code's Controls.Add lines). MS Chart element fonts are not
// control fonts and are not in the literal test. Data\font-literal-sites.tsv is the generated per-site report (UI-2a-5). No database, no
// window shown.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UiRefreshTests.Scan;

namespace UiRefreshTests {
	///<summary>One line of the scan: a line that names Microsoft Sans Serif or GenericSansSerif.</summary>
	public class FontSite {
		///<summary>Relative to the repository root, with backslashes.</summary>
		public string File="";
		///<summary>1-based.</summary>
		public int Line;
		///<summary>The line as it reads, its indent and trailing white space trimmed.</summary>
		public string Code="";
		///<summary>True inside an InitializeComponent of a class whose designer code is in the scan's scope.</summary>
		public bool IsDesignerCode;
		///<summary>Designer code only: the line sets a control's or the form's Font to a literal font (checked by the literal test).</summary>
		public FontLiteral FontLiteral;

		public override string ToString() {
			return File+":"+Line.ToString(CultureInfo.InvariantCulture)+" "+Code;
		}
	}

	///<summary>A literal font that the designer code of an in-scope class sets on a control or on the form: this.x.Font=new Font("family",size,style).</summary>
	public class FontLiteral {
		public string File="";
		public int Line;
		///<summary>The class whose InitializeComponent it is in (namespace.Class).</summary>
		public string ClassFullName="";
		///<summary>"this.label1" or "this" (the form's own font).</summary>
		public string Target="";
		public string Family="";
		public float Size;
		public FontStyle Style;
		///<summary>The parent the designer code adds the target to (this.panel1.Controls.Add(this.label1)): "this.panel1", "this" for the form or
		///UserControl itself, "" for the form's own font.</summary>
		public string ParentTarget="";
		///<summary>The font the target would inherit without its literal: the nearest literal up its parent chain in the same designer code (the
		///parent's own, or one the parent inherits), or null when the parent has the default font (the exe's default, or FormODBase's FontBody).
		///Plan 5.1: a default font is removed only where the parent has the default font.</summary>
		public FontLiteral FontParent;

		///<summary>True for FontBody's family and size in Regular, the font a control inherits from a parent with the default font.</summary>
		public bool IsBodyRegular() {
			Font fontBody=UiTheme.FontBody;
			return string.Equals(Family,fontBody.Name,StringComparison.OrdinalIgnoreCase) && Math.Abs(Size-fontBody.SizeInPoints)<0.01f
				&& Style==FontStyle.Regular;
		}

		public override string ToString() {
			return File+":"+Line.ToString(CultureInfo.InvariantCulture)+" "+Target+".Font "+Family+" "+Size.ToString(CultureInfo.InvariantCulture)+" "+Style;
		}
	}

	///<summary>What the scan finds in one file.</summary>
	public class FontFileScan {
		public List<FontSite> ListSites=new List<FontSite>();
		public List<FontLiteral> ListLiterals=new List<FontLiteral>();
		///<summary>The classes with an InitializeComponent in the file (namespace.Class) and whether their designer code is in scope.</summary>
		public List<Tuple<string,bool>> ListDesignerClasses=new List<Tuple<string,bool>>();
	}

	///<summary>One row of Data\font-literal-allow.tsv.</summary>
	public class FontAllowRow {
		public string File="";
		public string Code="";
		public int Count;
		public string Sort="";
		public string Reason="";
	}

	public static class FontLiteralScan {
		///<summary>The folders of the scan, relative to the repository root. UI-2a-4 slice 1: OpenPmsDental\Forms and OpenPmsDental\Forms Reports.
		///UI-2a-5: the other OpenPmsDental folders of UI-9a-6 (Main Modules, User Controls, UI, UIManagement, Logic, Bridges, ReportingForRdl,
		///ReportingComplex, SheetFramework, SmartCards, Misc, Tables Old Grids), OpenPmsDentalBusiness, OpenPmsDentalGraph and SparksToothChart.
		///UI-2a-6, the last slice: the other projects of UI-9a-7 (CentralManager, RdlDesign, ServiceManager, MySQLManager, xCrudGenerator, OpenPmsDentalDbSettings,
		///OpenPmsDentalHL7, DatabaseIntegrityCheck, DocumentationBuilder, UpdateFileCopier, OpenPmsDentalHelper32, CodeBase, ODR and WpfControlsOD).
		///Each later slice (the other projects) adds its folders here and its rows to the allow list.</summary>
		public static readonly string[] ArrayFolders=new string[] {
			@"OpenPmsDental\Forms",
			@"OpenPmsDental\Forms Reports",
			@"OpenPmsDental\Main Modules",
			@"OpenPmsDental\User Controls",
			@"OpenPmsDental\UI",
			@"OpenPmsDental\UIManagement",
			@"OpenPmsDental\Logic",
			@"OpenPmsDental\Bridges",
			@"OpenPmsDental\ReportingForRdl",
			@"OpenPmsDental\ReportingComplex",
			@"OpenPmsDental\SheetFramework",
			@"OpenPmsDental\SmartCards",
			@"OpenPmsDental\Misc",
			@"OpenPmsDental\Tables Old Grids",
			@"OpenPmsDentalBusiness",
			@"OpenPmsDentalGraph",
			@"SparksToothChart",
			@"CentralManager",
			@"RdlDesign",
			@"ServiceManager",
			@"MySQLManager",
			@"xCrudGenerator",
			@"OpenPmsDentalDbSettings",
			@"OpenPmsDentalHL7",
			@"DatabaseIntegrityCheck",
			@"DocumentationBuilder",
			@"UpdateFileCopier",
			@"OpenPmsDentalHelper32",
			@"CodeBase",
			@"ODR",
			@"WpfControlsOD",
		};
		public const string PATH_ALLOW=@"tests\UiRefreshTests\Data\font-literal-allow.tsv";
		public const string HEADER_ALLOW="file\tcode\tcount\tsort\treason";
		///<summary>The sorts a row of the allow list may have (section 5.2): a printout, a document (on screen or saved, attached or exported),
		///office content (sheet, eForm and dashboard-widget defaults, chart element fonts, the content pins of the tooth chart, the perio chart and
		///the dashboard), or an exe's default font literal (the first statement of its Main, 5.1, which NoHqTests' DefaultFontTests checks against
		///the token; UI-2a-5), or a token or default-font definition itself (UiTheme's pinned family and sizes, the default of the WPF drawing Font;
		///UI-2a-6: UI-2b changes it together with the tokens).</summary>
		public static readonly string[] ArraySorts=new string[] { "print","document","content","exe","token" };
		///<summary>The per-site report beside the allow list (UI-2a-5): every site of the scan with its sort and why, written by
		///Sites_ReportIsInStep when VARIABLE_SITES_WRITE is 1.</summary>
		public const string PATH_SITES=@"tests\UiRefreshTests\Data\font-literal-sites.tsv";
		public const string HEADER_SITES="site\tsort\treason";
		public const string VARIABLE_SITES_WRITE="OPMS_UI_FONT_SITES_WRITE";
		///<summary>The report's sort of a literal control or form font of in-scope designer code: no allow-list row, the literal test checks it.</summary>
		public const string SORT_LITERAL="literal";
		private static readonly Regex _regexFontName=new Regex(@"Microsoft Sans Serif|\bGenericSansSerif\b");
		///<summary>A control's or the form's Font set to a literal font, as the designer writes it.</summary>
		private static readonly Regex _regexControlFont=new Regex(
			@"\b(?<target>this(?:\.\w+)*?)\.Font\s*=\s*new\s+(?:System\.Drawing\.)?Font\s*\(\s*""(?<family>[^""]+)""\s*,\s*(?<size>[0-9]+(?:\.[0-9]+)?)[Ff]?(?<rest>[^;]*);");
		private static readonly Regex _regexInitializeComponent=new Regex(@"\bvoid\s+InitializeComponent\s*\(");
		///<summary>A control added to its parent: this.panel1.Controls.Add(this.label1), also with a TableLayoutPanel's column and row.</summary>
		private static readonly Regex _regexControlsAdd=new Regex(@"\b(?<parent>this(?:\.\w+)*?)\.Controls\.Add\(\s*(?<child>this\.\w+)\s*[,)]");
		///<summary>Controls added to their parent at once: this.panel1.Controls.AddRange(new System.Windows.Forms.Control[] { this.a, this.b }).</summary>
		private static readonly Regex _regexControlsAddRange=new Regex(
			@"\b(?<parent>this(?:\.\w+)*?)\.Controls\.AddRange\(\s*new\s+[\w.]+\s*\[\s*\]\s*\{(?<list>[^}]*)\}");

		#region The scan
		///<summary>The .cs files of the scan's folders, relative to the root with backslashes, build folders skipped, in ordinal order.</summary>
		public static List<string> GetFiles(string root) {
			List<string> listFiles=new List<string>();
			foreach(string folder in ArrayFolders) {
				string path=Path.Combine(root,folder);
				Assert.IsTrue(Directory.Exists(path),"A folder of the scan is missing: "+folder);
				foreach(string file in Directory.EnumerateFiles(path,"*.cs",SearchOption.AllDirectories)) {
					string relative=Path.GetRelativePath(root,file).Replace('/','\\');
					if(relative.Contains(@"\bin\") || relative.Contains(@"\obj\")) {
						continue;
					}
					listFiles.Add(relative);
				}
			}
			return listFiles.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x => x,StringComparer.Ordinal).ToList();
		}

		///<summary>Scans one file's text. funcIsDesignerInScope gets a class's full name (namespace.Class) and says whether its designer code is
		///in the scope (a plain form, a window without InitializeLayoutManager, a UserControl no program window hosts).</summary>
		public static FontFileScan ScanFile(string file,string text,Func<string,bool> funcIsDesignerInScope) {
			FontFileScan fontFileScan=new FontFileScan();
			string textNoComments=StripComments(text);
			string textStripped=SourceIndex.StripCommentsAndStrings(text);
			//The InitializeComponent bodies, with their class and whether their designer code is in scope.
			List<Tuple<int,int,string,bool>> listRanges=new List<Tuple<int,int,string,bool>>();
			foreach(Match match in _regexInitializeComponent.Matches(textStripped)) {
				int start;
				int end;
				if(!SourceIndex.GetBodyBounds(textStripped,match.Index+match.Length-1,out start,out end)) {
					continue;
				}
				string before=textStripped.Substring(0,match.Index);
				Match matchClass=Regex.Matches(before,@"\bclass\s+(\w+)").Cast<Match>().LastOrDefault();
				Match matchNamespace=Regex.Matches(before,@"\bnamespace\s+([\w.]+)").Cast<Match>().LastOrDefault();
				string nameClass=matchClass==null ? "" : matchClass.Groups[1].Value;
				string fullName=matchNamespace==null ? nameClass : matchNamespace.Groups[1].Value+"."+nameClass;
				bool isInScope=funcIsDesignerInScope(fullName);
				listRanges.Add(Tuple.Create(start,end,fullName,isInScope));
				fontFileScan.ListDesignerClasses.Add(Tuple.Create(fullName,isInScope));
			}
			//The literal control and form fonts of the in-scope designer code.
			foreach(Tuple<int,int,string,bool> range in listRanges.Where(x => x.Item4)) {
				string body=textNoComments.Substring(range.Item1,range.Item2-range.Item1+1);
				List<FontLiteral> listLiteralsRange=new List<FontLiteral>();
				foreach(Match match in _regexControlFont.Matches(body)) {
					FontLiteral fontLiteral=new FontLiteral();
					fontLiteral.File=file;
					fontLiteral.Line=GetLine(textNoComments,range.Item1+match.Index);
					fontLiteral.ClassFullName=range.Item3;
					fontLiteral.Target=match.Groups["target"].Value;
					fontLiteral.Family=match.Groups["family"].Value;
					fontLiteral.Size=float.Parse(match.Groups["size"].Value,CultureInfo.InvariantCulture);
					fontLiteral.Style=ParseStyle(match.Groups["rest"].Value);
					listLiteralsRange.Add(fontLiteral);
				}
				SetFontParents(listLiteralsRange,GetParents(body));
				fontFileScan.ListLiterals.AddRange(listLiteralsRange);
			}
			//The sites, line by line.
			string[] arrayLinesNoComments=textNoComments.Split('\n');
			string[] arrayLines=text.Split('\n');
			int offset=0;
			for(int i=0;i<arrayLinesNoComments.Length;i++) {
				int offsetLine=offset;
				offset+=arrayLinesNoComments[i].Length+1;
				if(!_regexFontName.IsMatch(arrayLinesNoComments[i])) {
					continue;
				}
				Tuple<int,int,string,bool> range=listRanges.FirstOrDefault(x => x.Item1<=offsetLine && offsetLine<=x.Item2);
				if(range!=null && !range.Item4) {
					continue;//Designer code under LayoutManager: exempt, the mapping changes it at run time (5.1).
				}
				FontSite fontSite=new FontSite();
				fontSite.File=file;
				fontSite.Line=i+1;
				fontSite.Code=arrayLines[i].Trim();
				fontSite.IsDesignerCode=range!=null;
				if(range!=null) {
					fontSite.FontLiteral=fontFileScan.ListLiterals.FirstOrDefault(x => x.Line==i+1);
				}
				fontFileScan.ListSites.Add(fontSite);
			}
			return fontFileScan;
		}

		///<summary>Each control's parent in one InitializeComponent body, from its Controls.Add and Controls.AddRange lines: "this.label1" to
		///"this.panel1" or "this".</summary>
		public static Dictionary<string,string> GetParents(string body) {
			Dictionary<string,string> dictParents=new Dictionary<string,string>(StringComparer.Ordinal);
			foreach(Match match in _regexControlsAdd.Matches(body)) {
				dictParents[match.Groups["child"].Value]=match.Groups["parent"].Value;
			}
			foreach(Match match in _regexControlsAddRange.Matches(body)) {
				foreach(Match matchChild in Regex.Matches(match.Groups["list"].Value,@"\bthis\.\w+")) {
					dictParents[matchChild.Value]=match.Groups["parent"].Value;
				}
			}
			return dictParents;
		}

		///<summary>Sets each literal's ParentTarget and FontParent (plan 5.1). A control's parent is the one it is added to; a container's panel
		///(this.splitContainer1.Panel1) takes its container's font; a control the designer code adds to nothing it names sits in the form (this).
		///The form's own font and a UserControl's own font have the default font as their parent's.</summary>
		public static void SetFontParents(List<FontLiteral> listLiterals,Dictionary<string,string> dictParents) {
			Dictionary<string,FontLiteral> dictLiterals=new Dictionary<string,FontLiteral>(StringComparer.Ordinal);
			foreach(FontLiteral fontLiteral in listLiterals) {
				dictLiterals[fontLiteral.Target]=fontLiteral;//The last one the designer sets wins, as at run time.
			}
			foreach(FontLiteral fontLiteral in listLiterals) {
				if(fontLiteral.Target=="this") {
					fontLiteral.ParentTarget="";
					fontLiteral.FontParent=null;
					continue;
				}
				fontLiteral.ParentTarget=GetParent(fontLiteral.Target,dictParents);
				fontLiteral.FontParent=null;
				string target=fontLiteral.ParentTarget;
				for(int i=0;i<100 && target!="";i++) {//A guard against a cycle in malformed designer code.
					FontLiteral fontLiteralUp;
					if(dictLiterals.TryGetValue(target,out fontLiteralUp)) {
						fontLiteral.FontParent=fontLiteralUp;
						break;
					}
					target=target=="this" ? "" : GetParent(target,dictParents);
				}
			}
		}

		///<summary>The parent of a target: the one it is added to; for a container's panel (this.splitContainer1.Panel1, three parts or more) the
		///container; else the form (this).</summary>
		private static string GetParent(string target,Dictionary<string,string> dictParents) {
			string parent;
			if(dictParents.TryGetValue(target,out parent)) {
				return parent;
			}
			int indexLastDot=target.LastIndexOf('.');
			if(target.Count(x => x=='.')>=2) {
				return target.Substring(0,indexLastDot);
			}
			return "this";
		}

		///<summary>True for a site the scan allows without a row: a literal control or form font in in-scope designer code while the tokens hold
		///today's font (until UI-2b); the literal test checks it.</summary>
		public static bool IsAllowedAsControlFont(FontSite fontSite) {
			return fontSite.IsDesignerCode && fontSite.FontLiteral!=null && UiTheme.IsFontMappingIdentity;
		}

		///<summary>The rule of 3.4 for a literal control or form font of a plain form or a window without InitializeLayoutManager (5.1). Null when
		///it follows the rule, else why not. Other families (Arial, Courier New, Wingdings) stay and are fine. A default font (FontBody's family and
		///size, Regular) must be removed only where the parent has the default font (fontLiteral.FontParent null, or itself FontBody's family and
		///size in Regular): under a parent with another font (a Bold form font, for example) the literal is what keeps the control Regular, and it
		///stays.</summary>
		public static string CheckLiteral(FontLiteral fontLiteral) {
			Font fontBody=UiTheme.FontBody;
			bool isTodaysFamily=string.Equals(fontLiteral.Family,UiTheme.FONT_FAMILY_TODAY,StringComparison.OrdinalIgnoreCase);
			bool isBodyFamily=string.Equals(fontLiteral.Family,fontBody.Name,StringComparison.OrdinalIgnoreCase);
			if(!isTodaysFamily && !isBodyFamily) {
				return null;
			}
			if(!isBodyFamily) {
				return "its family is "+fontLiteral.Family+", FontBody's is "+fontBody.Name+": UI-2b changes the literal together with the token";
			}
			bool isAtBodySize=Math.Abs(fontLiteral.Size-fontBody.SizeInPoints)<0.01f;
			if(!UiTheme.IsFontMappingIdentity && UiTheme.IsBodySize(fontLiteral.Size) && !isAtBodySize) {
				return "a body size ("+fontLiteral.Size.ToString(CultureInfo.InvariantCulture)+" pt) becomes FontBody's size ("
					+fontBody.SizeInPoints.ToString(CultureInfo.InvariantCulture)+" pt) by the rule of 3.4";
			}
			bool isParentDefault=fontLiteral.FontParent==null || fontLiteral.FontParent.IsBodyRegular();
			if(isAtBodySize && fontLiteral.Style==FontStyle.Regular && isParentDefault) {
				return "it is the default font (FontBody's family and size, Regular) and its parent has the default font: remove it so the control"
					+" inherits (5.1)";
			}
			return null;
		}

		///<summary>Bold, Italic, Underline and Strikeout named in the rest of a designer font's arguments.</summary>
		public static FontStyle ParseStyle(string rest) {
			FontStyle fontStyle=FontStyle.Regular;
			if(Regex.IsMatch(rest,@"\bFontStyle\.Bold\b")) {
				fontStyle|=FontStyle.Bold;
			}
			if(Regex.IsMatch(rest,@"\bFontStyle\.Italic\b")) {
				fontStyle|=FontStyle.Italic;
			}
			if(Regex.IsMatch(rest,@"\bFontStyle\.Underline\b")) {
				fontStyle|=FontStyle.Underline;
			}
			if(Regex.IsMatch(rest,@"\bFontStyle\.Strikeout\b")) {
				fontStyle|=FontStyle.Strikeout;
			}
			return fontStyle;
		}

		private static int GetLine(string text,int index) {
			int line=1;
			for(int i=0;i<index && i<text.Length;i++) {
				if(text[i]=='\n') {
					line++;
				}
			}
			return line;
		}

		///<summary>The text with // and /* */ comments replaced by spaces (newlines kept, so offsets and lines stay); string and character
		///literals are kept, so a font name inside a string is found and a "//" inside a string is not a comment.</summary>
		public static string StripComments(string text) {
			StringBuilder stringBuilder=new StringBuilder(text.Length);
			int i=0;
			while(i<text.Length) {
				char c=text[i];
				char next=i+1<text.Length ? text[i+1] : '\0';
				if(c=='/' && next=='/') {
					while(i<text.Length && text[i]!='\n') {
						stringBuilder.Append(text[i]=='\r' ? '\r' : ' ');
						i++;
					}
					continue;
				}
				if(c=='/' && next=='*') {
					int end=text.IndexOf("*/",i+2,StringComparison.Ordinal);
					end=end<0 ? text.Length : end+2;
					for(int j=i;j<end;j++) {
						stringBuilder.Append(text[j]=='\n' || text[j]=='\r' ? text[j] : ' ');
					}
					i=end;
					continue;
				}
				int endLiteral=-1;
				if(c=='@' && next=='"' || c=='$' && next=='@' || c=='@' && next=='$') {
					int startQuote=text.IndexOf('"',i);
					endLiteral=startQuote+1;
					while(endLiteral<text.Length) {
						if(text[endLiteral]=='"') {
							if(endLiteral+1<text.Length && text[endLiteral+1]=='"') {
								endLiteral+=2;
								continue;
							}
							break;
						}
						endLiteral++;
					}
					endLiteral=Math.Min(text.Length,endLiteral+1);
				}
				else if(c=='"' || c=='\'') {
					endLiteral=i+1;
					while(endLiteral<text.Length && text[endLiteral]!=c && text[endLiteral]!='\n') {
						if(text[endLiteral]=='\\') {
							endLiteral++;
						}
						endLiteral++;
					}
					endLiteral=Math.Min(text.Length,endLiteral+1);
				}
				if(endLiteral>i) {
					stringBuilder.Append(text,i,endLiteral-i);
					i=endLiteral;
					continue;
				}
				stringBuilder.Append(c);
				i++;
			}
			return stringBuilder.ToString();
		}
		#endregion The scan

		#region The allow list
		public static List<FontAllowRow> ReadAllowList(string root) {
			List<FontAllowRow> listRows=new List<FontAllowRow>();
			bool isHeaderRead=false;
			foreach(string lineRaw in File.ReadAllLines(Path.Combine(root,PATH_ALLOW),Encoding.UTF8)) {
				string line=lineRaw.TrimEnd('\r');
				if(line.StartsWith("#",StringComparison.Ordinal) || line.Trim()=="") {
					continue;
				}
				if(!isHeaderRead) {
					Assert.AreEqual(HEADER_ALLOW,line,"font-literal-allow.tsv's header.");
					isHeaderRead=true;
					continue;
				}
				string[] arrayFields=line.Split('\t');
				Assert.AreEqual(5,arrayFields.Length,"A row of font-literal-allow.tsv needs five fields: "+line);
				FontAllowRow fontAllowRow=new FontAllowRow();
				fontAllowRow.File=arrayFields[0];
				fontAllowRow.Code=arrayFields[1];
				fontAllowRow.Count=int.Parse(arrayFields[2],CultureInfo.InvariantCulture);
				fontAllowRow.Sort=arrayFields[3];
				fontAllowRow.Reason=arrayFields[4];
				listRows.Add(fontAllowRow);
			}
			Assert.IsTrue(isHeaderRead,"font-literal-allow.tsv has no header.");
			return listRows;
		}

		///<summary>The per-site report's rows: site (file:line), sort and why, in file and line order. A literal control or form font of in-scope
		///designer code is SORT_LITERAL; a site the allow list does not have is "unlisted" (the scan fails on it).</summary>
		public static List<string> GetSiteRows(List<FontFileScan> listFileScans,List<FontAllowRow> listRows) {
			Dictionary<string,FontAllowRow> dictRows=listRows.GroupBy(x => x.File+"\t"+x.Code).ToDictionary(x => x.Key,x => x.First());
			List<string> listSiteRows=new List<string>();
			foreach(FontSite fontSite in listFileScans.SelectMany(x => x.ListSites).OrderBy(x => x.File,StringComparer.Ordinal).ThenBy(x => x.Line)) {
				string sort;
				string reason;
				FontAllowRow fontAllowRow;
				if(IsAllowedAsControlFont(fontSite)) {
					sort=SORT_LITERAL;
					reason=DescribeLiteral(fontSite.FontLiteral);
				}
				else if(dictRows.TryGetValue(fontSite.File+"\t"+fontSite.Code,out fontAllowRow)) {
					sort=fontAllowRow.Sort;
					reason=fontAllowRow.Reason;
				}
				else {
					sort="unlisted";
					reason="not on the allow list";
				}
				listSiteRows.Add(fontSite.File+":"+fontSite.Line.ToString(CultureInfo.InvariantCulture)+"\t"+sort+"\t"+reason);
			}
			return listSiteRows;
		}

		///<summary>Why a literal control or form font stays: the designer code, the parent's font, and what UI-2b does.</summary>
		public static string DescribeLiteral(FontLiteral fontLiteral) {
			string parent;
			if(fontLiteral.Target=="this") {
				parent="the form's or UserControl's own font";
			}
			else if(fontLiteral.FontParent==null) {
				parent="its parent ("+fontLiteral.ParentTarget+") has the default font";
			}
			else {
				parent="its parent ("+fontLiteral.ParentTarget+") has "+fontLiteral.FontParent.Family+" "
					+fontLiteral.FontParent.Size.ToString(CultureInfo.InvariantCulture)+" "+fontLiteral.FontParent.Style
					+(fontLiteral.FontParent.Target==fontLiteral.ParentTarget ? "" : " from "+fontLiteral.FontParent.Target);
			}
			return "designer code in scope: "+fontLiteral.Target+".Font "+fontLiteral.Family+" "+fontLiteral.Size.ToString(CultureInfo.InvariantCulture)
				+" "+fontLiteral.Style+"; "+parent+"; stays a literal, the literal test checks it and UI-2b changes it (5.1)";
		}
		#endregion The allow list

		#region The scope
		///<summary>The tier 1 scan's list (section 5.4) as a lookup: a class's full name to whether its designer code is in the scope. A class the
		///list does not know (not a Form or UserControl) is in scope, so nothing is skipped by mistake.</summary>
		public static Func<string,bool> GetScopeFromScanTypes(List<ScanType> listScanTypes) {
			Dictionary<string,bool> dictUnderLayoutManager=new Dictionary<string,bool>(StringComparer.Ordinal);
			foreach(ScanType scanType in listScanTypes) {
				string fullName=scanType.Type.FullName.Replace('+','.');
				bool isUnder;
				if(dictUnderLayoutManager.TryGetValue(fullName,out isUnder)) {
					dictUnderLayoutManager[fullName]=isUnder && scanType.IsUnderLayoutManager;
					continue;
				}
				dictUnderLayoutManager[fullName]=scanType.IsUnderLayoutManager;
			}
			return fullName => {
				bool isUnder;
				return !dictUnderLayoutManager.TryGetValue(fullName,out isUnder) || !isUnder;
			};
		}
		#endregion The scope
	}

	[TestClass]
	public class FontLiteralScanTests {
		[TestCleanup]
		public void TestCleanup() {
			UiTheme.SetFontForTest(null);
		}

		///<summary>Every file of the scan's folders, scanned with the tier 1 scan's scope.</summary>
		private static List<FontFileScan> ScanTheFolders() {
			List<FontFileScan> listFileScans=null;
			StaRunner.Run(() => {
				NoDatabaseGuard.Install();
				Func<string,bool> funcIsDesignerInScope=FontLiteralScan.GetScopeFromScanTypes(WindowBuilder.GetScanTypes());
				string root=RepoPaths.Root();
				listFileScans=FontLiteralScan.GetFiles(root)
					.Select(x => FontLiteralScan.ScanFile(x,File.ReadAllText(Path.Combine(root,x)),funcIsDesignerInScope))
					.ToList();
			});
			return listFileScans;
		}

		[TestMethod]
		public void Scan_NoMicrosoftSansSerifOutsideTheAllowList() {
			List<FontFileScan> listFileScans=ScanTheFolders();
			List<FontSite> listSites=listFileScans.SelectMany(x => x.ListSites).ToList();
			Assert.IsTrue(listSites.Count>0,"The scan found no site at all: does it read the folders?");
			List<FontAllowRow> listRows=FontLiteralScan.ReadAllowList(RepoPaths.Root());
			List<string> listErrors=new List<string>();
			foreach(FontAllowRow fontAllowRow in listRows) {
				if(!FontLiteralScan.ArraySorts.Contains(fontAllowRow.Sort)) {
					listErrors.Add("A row's sort must be print, document, content, exe or token: "+fontAllowRow.File+" "+fontAllowRow.Code);
				}
				if(fontAllowRow.Reason.Trim()=="") {
					listErrors.Add("A row needs its reason: "+fontAllowRow.File+" "+fontAllowRow.Code);
				}
				if(!FontLiteralScan.ArrayFolders.Any(x => fontAllowRow.File.StartsWith(x+@"\",StringComparison.OrdinalIgnoreCase))) {
					listErrors.Add("A row's file is outside the scan's folders: "+fontAllowRow.File);
				}
			}
			foreach(IGrouping<string,FontAllowRow> group in listRows.GroupBy(x => x.File+"\t"+x.Code)) {
				if(group.Count()>1) {
					listErrors.Add("A row is listed twice: "+group.Key);
				}
			}
			List<FontSite> listListed=listSites.Where(x => !FontLiteralScan.IsAllowedAsControlFont(x)).ToList();
			Dictionary<string,int> dictCounts=listListed.GroupBy(x => x.File+"\t"+x.Code).ToDictionary(x => x.Key,x => x.Count());
			Dictionary<string,int> dictAllowed=listRows.GroupBy(x => x.File+"\t"+x.Code).ToDictionary(x => x.Key,x => x.First().Count);
			foreach(FontSite fontSite in listListed) {
				if(!dictAllowed.ContainsKey(fontSite.File+"\t"+fontSite.Code)) {
					listErrors.Add("Not on the allow list (a screen font goes through its UiTheme token; print, document and content are listed): "+fontSite);
				}
			}
			foreach(KeyValuePair<string,int> pair in dictAllowed) {
				int count;
				dictCounts.TryGetValue(pair.Key,out count);
				if(count!=pair.Value) {
					listErrors.Add("The source has this line "+count+" times, the allow list says "+pair.Value+" (cut or correct the row): "+pair.Key.Replace("\t"," "));
				}
			}
			Console.WriteLine("Sites: "+listSites.Count+" in "+listSites.Select(x => x.File).Distinct().Count()+" files; allowed control fonts: "
				+listSites.Count(FontLiteralScan.IsAllowedAsControlFont)+"; rows: "+listRows.Count+".");
			Assert.AreEqual(0,listErrors.Count,string.Join(Environment.NewLine,listErrors));
		}

		[TestMethod]
		public void Literals_ControlAndFormFontsOfPlainForms_FollowTheToken() {
			List<FontFileScan> listFileScans=ScanTheFolders();
			List<FontLiteral> listLiterals=listFileScans.SelectMany(x => x.ListLiterals).ToList();
			List<string> listErrors=new List<string>();
			foreach(FontLiteral fontLiteral in listLiterals) {
				string error=FontLiteralScan.CheckLiteral(fontLiteral);
				if(error!=null) {
					listErrors.Add(fontLiteral+": "+error);
				}
			}
			List<string> listInScope=listFileScans.SelectMany(x => x.ListDesignerClasses).Where(x => x.Item2).Select(x => x.Item1).Distinct().ToList();
			Console.WriteLine("Designer code in scope: "+string.Join(", ",listInScope)+"; literal control and form fonts: "+listLiterals.Count+".");
			Assert.AreEqual(0,listErrors.Count,string.Join(Environment.NewLine,listErrors));
		}

		[TestMethod]
		public void Scope_IsTheTierOneScansList() {
			List<FontFileScan> listFileScans=ScanTheFolders();
			Dictionary<string,bool> dictClasses=new Dictionary<string,bool>();
			foreach(Tuple<string,bool> tuple in listFileScans.SelectMany(x => x.ListDesignerClasses)) {
				dictClasses[tuple.Item1]=tuple.Item2;
			}
			Assert.IsTrue(dictClasses.Count>700,"Classes with designer code in the scan's folders: "+dictClasses.Count);
			Assert.IsTrue(dictClasses["OpenPmsDental.FormSplash"],"FormSplash is a plain Form: its designer code is in the scope.");
			Assert.IsFalse(dictClasses["OpenPmsDental.FormDisplayFieldEdit"],"A LayoutManager window's designer code is exempt.");
			Assert.IsFalse(dictClasses["OpenPmsDental.FormSheetFillEdit"],"A LayoutManager window's designer code is exempt.");
			//UI-2a-5: the five allocator windows without InitializeLayoutManager (5.1) were in scope; since UI-12F (the grid's final consistency pass)
			//they call it, so their designer code is exempt like every LayoutManager window's. A module and a graph UserControl are found.
			foreach(string name in new string[] { "FormProgressViewer","FormInstallAllocator_Provider","FormReport_GuarantorAllocationCheck",
				"FormReport_ProviderIncomeReport","FormWarnToCloseComputers" })
			{
				Assert.IsFalse(dictClasses["OpenPmsDental.Reporting.Allocators.MyAllocator1."+name],name+" calls InitializeLayoutManager: exempt.");
			}
			Assert.IsFalse(dictClasses["OpenPmsDental.ControlAccount"],"A module is hosted through LayoutManager: exempt.");
			Assert.IsTrue(dictClasses["OpenPmsDentalGraph.FormDashboardEditTab"],"The graph's forms are plain forms: in scope.");
			Assert.IsTrue(dictClasses["OpenPmsDentalGraph.GraphIncome"],"The graph's UserControls are hosted by plain forms only (5.1): in scope.");
		}

		[TestMethod]
		public void Sites_ReportIsInStep() {
			List<FontFileScan> listFileScans=ScanTheFolders();
			string root=RepoPaths.Root();
			List<string> listSiteRows=FontLiteralScan.GetSiteRows(listFileScans,FontLiteralScan.ReadAllowList(root));
			string pathSites=Path.Combine(root,FontLiteralScan.PATH_SITES);
			if(Environment.GetEnvironmentVariable(FontLiteralScan.VARIABLE_SITES_WRITE)=="1") {
				string text="# Every Microsoft Sans Serif and GenericSansSerif site of the font scan's folders (docs/specs/ui-refresh-plan.md sections 5.1, 5.2\n"
					+"# and 9; docs/port-notes/UI-2a.md), generated by FontLiteralScanTests.Sites_ReportIsInStep with "+FontLiteralScan.VARIABLE_SITES_WRITE+"=1.\n"
					+"# sort: print, document, content, exe or token (a row of font-literal-allow.tsv, its reason), or literal (a control or form font of in-scope\n"
					+"# designer code, which the literal test checks). The screen sites that take a token no longer name the family and are not here.\n"
					+"# The test compares the sites per file and sort, not the line numbers, so an edit above a site needs no new report.\n"
					+FontLiteralScan.HEADER_SITES+"\n"+string.Join("",listSiteRows.Select(x => x+"\n"));
				File.WriteAllText(pathSites,text,new UTF8Encoding(false));
			}
			Assert.IsTrue(File.Exists(pathSites),pathSites+" is missing ("+FontLiteralScan.VARIABLE_SITES_WRITE+"=1 writes it).");
			List<string> listOld=File.ReadAllLines(pathSites,Encoding.UTF8).Select(x => x.TrimEnd('\r'))
				.Where(x => x!="" && !x.StartsWith("#",StringComparison.Ordinal) && x!=FontLiteralScan.HEADER_SITES).ToList();
			Func<string,string> funcFileAndSort=row => {
				string[] arrayFields=row.Split('\t');
				string site=arrayFields[0];
				int indexColon=site.LastIndexOf(':');
				return (indexColon<0 ? site : site.Substring(0,indexColon))+"\t"+(arrayFields.Length>1 ? arrayFields[1] : "");
			};
			List<string> listNew=listSiteRows.Select(funcFileAndSort).OrderBy(x => x,StringComparer.Ordinal).ToList();
			List<string> listFile=listOld.Select(funcFileAndSort).OrderBy(x => x,StringComparer.Ordinal).ToList();
			CollectionAssert.AreEqual(listNew,listFile,"font-literal-sites.tsv is out of step with the scan (per file and sort); write it again with "
				+FontLiteralScan.VARIABLE_SITES_WRITE+"=1.");
			Assert.IsFalse(listSiteRows.Any(x => x.Split('\t')[1]=="unlisted"),"A site is not on the allow list (see Scan_NoMicrosoftSansSerifOutsideTheAllowList).");
		}

		[TestMethod]
		public void ScanFile_SkipsComments_ExemptsLayoutManagerDesignerCode_AndFindsTheRest() {
			//The rule with today's tokens first (the program held them until UI-2b; the program's own are UI-2b's since the flip).
			UiTheme.SetFontForTest(UiFontOption.Today);
			string text=string.Join("\r\n",new string[] {
				"namespace OpenPmsDental {",
				"	partial class FormUnder {",
				"		private void InitializeComponent() {",
				"			this.label1.Font = new System.Drawing.Font(\"Microsoft Sans Serif\", 8.25F, System.Drawing.FontStyle.Bold);",
				"		}",
				"	}",
				"	partial class FormPlain {",
				"		private void InitializeComponent() {",
				"			this.label2.Font = new System.Drawing.Font(\"Microsoft Sans Serif\", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));",
				"			this.Font = new System.Drawing.Font(\"Microsoft Sans Serif\", 8.25F);",
				"			this.label3.Font = new System.Drawing.Font(\"Arial\", 8.25F);",
				"			chartArea1.AxisX.LabelStyle.Font = new System.Drawing.Font(\"Microsoft Sans Serif\", 11.25F);",
				"		}",
				"		//new Font(\"Microsoft Sans Serif\",8.25f) in a comment",
				"		/* FontFamily.GenericSansSerif in a block comment */",
				"		private string _url=\"http://example.test/a\";//GenericSansSerif after a string with //",
				"		private Font _font=new Font(FontFamily.GenericSansSerif,8.5f);",
				"	}",
				"}",
			});
			FontFileScan fontFileScan=FontLiteralScan.ScanFile(@"X\Y.cs",text,x => x!="OpenPmsDental.FormUnder");
			CollectionAssert.AreEqual(new int[] { 9,10,12,17 },fontFileScan.ListSites.Select(x => x.Line).ToArray(),
				string.Join("; ",fontFileScan.ListSites));
			Assert.IsTrue(fontFileScan.ListSites[0].IsDesignerCode);
			Assert.IsNotNull(fontFileScan.ListSites[0].FontLiteral);
			Assert.IsNotNull(fontFileScan.ListSites[1].FontLiteral,"The form's own font.");
			Assert.IsNull(fontFileScan.ListSites[2].FontLiteral,"A chart element font is not a control font.");
			Assert.IsFalse(fontFileScan.ListSites[3].IsDesignerCode);
			Assert.AreEqual("private Font _font=new Font(FontFamily.GenericSansSerif,8.5f);",fontFileScan.ListSites[3].Code);
			Assert.AreEqual(3,fontFileScan.ListLiterals.Count,"label2, the form and label3 (Arial).");
			FontLiteral fontLiteral=fontFileScan.ListLiterals[0];
			Assert.AreEqual("this.label2",fontLiteral.Target);
			Assert.AreEqual("OpenPmsDental.FormPlain",fontLiteral.ClassFullName);
			Assert.AreEqual(8.25f,fontLiteral.Size);
			Assert.AreEqual(FontStyle.Bold,fontLiteral.Style);
			Assert.AreEqual("this",fontFileScan.ListLiterals[1].Target);
			CollectionAssert.AreEqual(new string[] { "OpenPmsDental.FormUnder","OpenPmsDental.FormPlain" },
				fontFileScan.ListDesignerClasses.Select(x => x.Item1).ToArray());
			//Until UI-2b the scan leaves the literal control fonts to the literal test; a chart element font needs a row.
			Assert.IsTrue(FontLiteralScan.IsAllowedAsControlFont(fontFileScan.ListSites[0]));
			Assert.IsFalse(FontLiteralScan.IsAllowedAsControlFont(fontFileScan.ListSites[2]));
			Assert.IsFalse(FontLiteralScan.IsAllowedAsControlFont(fontFileScan.ListSites[3]));
			UiTheme.SetFontForTest(UiFontOption.UI2b);
			Assert.IsFalse(FontLiteralScan.IsAllowedAsControlFont(fontFileScan.ListSites[0]),"After the flip a Microsoft Sans Serif literal needs a row.");
		}

		[TestMethod]
		public void CheckLiteral_TheRuleOf34() {
			//The rule with today's tokens first (the program held them until UI-2b; the program's own are UI-2b's since the flip).
			UiTheme.SetFontForTest(UiFontOption.Today);
			Assert.IsNull(FontLiteralScan.CheckLiteral(Literal("Microsoft Sans Serif",8.25f,FontStyle.Bold)));
			Assert.IsNull(FontLiteralScan.CheckLiteral(Literal("Microsoft Sans Serif",14.25f,FontStyle.Regular)));
			Assert.IsNull(FontLiteralScan.CheckLiteral(Literal("Microsoft Sans Serif",8f,FontStyle.Regular)),"8 pt is not the default font.");
			Assert.IsNull(FontLiteralScan.CheckLiteral(Literal("Arial",8.25f,FontStyle.Regular)),"Other families stay (3.4).");
			StringAssert.Contains(FontLiteralScan.CheckLiteral(Literal("Microsoft Sans Serif",8.25f,FontStyle.Regular)),"default font");
			UiTheme.SetFontForTest(UiFontOption.UI2b);
			StringAssert.Contains(FontLiteralScan.CheckLiteral(Literal("Microsoft Sans Serif",8.25f,FontStyle.Bold)),"UI-2b changes the literal");
			Assert.IsNull(FontLiteralScan.CheckLiteral(Literal("Segoe UI",9f,FontStyle.Bold)));
			Assert.IsNull(FontLiteralScan.CheckLiteral(Literal("Segoe UI",14.25f,FontStyle.Regular)));
			StringAssert.Contains(FontLiteralScan.CheckLiteral(Literal("Segoe UI",8.25f,FontStyle.Bold)),"body size");
			StringAssert.Contains(FontLiteralScan.CheckLiteral(Literal("Segoe UI",9f,FontStyle.Regular)),"default font");
			Assert.IsNull(FontLiteralScan.CheckLiteral(Literal("Wingdings",9f,FontStyle.Regular)));
		}

		///<summary>UI-2a-5, the review fix of UI-2a-4: a default font is removed only where the parent has the default font (5.1). Under a Bold form
		///font (FormReport_ProviderIncomeReport sets one) a Regular 8.25 literal keeps the control Regular and stays.</summary>
		[TestMethod]
		public void CheckLiteral_ComparesWithTheParentsFont() {
			//The rule with today's tokens first (the program held them until UI-2b; the program's own are UI-2b's since the flip).
			UiTheme.SetFontForTest(UiFontOption.Today);
			FontLiteral fontLiteralBoldForm=Literal("Microsoft Sans Serif",8.25f,FontStyle.Bold);
			fontLiteralBoldForm.Target="this";
			FontLiteral fontLiteral=Literal("Microsoft Sans Serif",8.25f,FontStyle.Regular);
			fontLiteral.FontParent=fontLiteralBoldForm;
			Assert.IsNull(FontLiteralScan.CheckLiteral(fontLiteral),"Under a Bold parent the Regular literal stays.");
			fontLiteral.FontParent=Literal("Microsoft Sans Serif",8.25f,FontStyle.Regular);
			StringAssert.Contains(FontLiteralScan.CheckLiteral(fontLiteral),"default font","A parent whose literal is the default font has the default font.");
			fontLiteral.FontParent=Literal("Arial",12f,FontStyle.Bold);
			Assert.IsNull(FontLiteralScan.CheckLiteral(fontLiteral),"Under an Arial parent the literal stays.");
			fontLiteral.FontParent=null;
			StringAssert.Contains(FontLiteralScan.CheckLiteral(fontLiteral),"default font");
		}

		[TestMethod]
		public void ScanFile_FindsEachLiteralsParentFont() {
			//The rule with today's tokens first (the program held them until UI-2b; the program's own are UI-2b's since the flip).
			UiTheme.SetFontForTest(UiFontOption.Today);
			string text=string.Join("\r\n",new string[] {
				"namespace OpenPmsDental {",
				"	partial class FormPlain {",
				"		private void InitializeComponent() {",
				"			this.panel1.Controls.Add(this.label1);",
				"			this.panel1.Font = new System.Drawing.Font(\"Microsoft Sans Serif\", 8.25F, System.Drawing.FontStyle.Bold);",
				"			this.label1.Font = new System.Drawing.Font(\"Microsoft Sans Serif\", 8.25F);",
				"			this.splitContainer1.Panel1.Controls.Add(this.label2);",
				"			this.splitContainer1.Panel2.Controls.AddRange(new System.Windows.Forms.Control[] {",
				"			this.label3,",
				"			this.label4});",
				"			this.label2.Font = new System.Drawing.Font(\"Microsoft Sans Serif\", 8.25F);",
				"			this.label3.Font = new System.Drawing.Font(\"Microsoft Sans Serif\", 8.25F);",
				"			this.tableLayoutPanel1.Controls.Add(this.label5, 0, 1);",
				"			this.label5.Font = new System.Drawing.Font(\"Microsoft Sans Serif\", 8.25F);",
				"			this.label6.Font = new System.Drawing.Font(\"Microsoft Sans Serif\", 8.25F);",
				"			this.Controls.Add(this.panel1);",
				"			this.Controls.Add(this.splitContainer1);",
				"			this.Font = new System.Drawing.Font(\"Arial\", 12F, System.Drawing.FontStyle.Bold);",
				"		}",
				"	}",
				"}",
			});
			FontFileScan fontFileScan=FontLiteralScan.ScanFile(@"X\Y.cs",text,x => true);
			Dictionary<string,FontLiteral> dictLiterals=fontFileScan.ListLiterals.ToDictionary(x => x.Target);
			Assert.AreEqual("this.panel1",dictLiterals["this.label1"].ParentTarget);
			Assert.AreSame(dictLiterals["this.panel1"],dictLiterals["this.label1"].FontParent,"label1 inherits panel1's Bold font.");
			Assert.IsNull(FontLiteralScan.CheckLiteral(dictLiterals["this.label1"]),"So its Regular literal stays.");
			Assert.AreEqual("this.splitContainer1.Panel1",dictLiterals["this.label2"].ParentTarget);
			Assert.AreSame(dictLiterals["this"],dictLiterals["this.label2"].FontParent,"A container's panel takes the container's font, here the form's.");
			Assert.AreEqual("this.splitContainer1.Panel2",dictLiterals["this.label3"].ParentTarget,"Controls.AddRange.");
			Assert.AreEqual("this.tableLayoutPanel1",dictLiterals["this.label5"].ParentTarget,"Controls.Add with a column and a row.");
			Assert.AreEqual("this",dictLiterals["this.label6"].ParentTarget,"Added to nothing the designer code names: the form.");
			Assert.AreSame(dictLiterals["this"],dictLiterals["this.label6"].FontParent);
			Assert.IsNull(dictLiterals["this"].FontParent,"The form's own font has the default font as its parent's.");
			//Without the form's Arial font, label2 sits under the default font and its literal must go.
			FontFileScan fontFileScanNoFormFont=FontLiteralScan.ScanFile(@"X\Y.cs",text.Replace("this.Font = new System.Drawing.Font(\"Arial\", 12F, System.Drawing.FontStyle.Bold);",""),x => true);
			FontLiteral fontLiteralLabel2=fontFileScanNoFormFont.ListLiterals.Single(x => x.Target=="this.label2");
			Assert.IsNull(fontLiteralLabel2.FontParent);
			StringAssert.Contains(FontLiteralScan.CheckLiteral(fontLiteralLabel2),"remove it");
			StringAssert.Contains(FontLiteralScan.DescribeLiteral(dictLiterals["this.label1"]),"its parent (this.panel1) has Microsoft Sans Serif 8.25 Bold");
		}

		[TestMethod]
		public void AllowList_EachRowHasASortAndAReason() {
			List<FontAllowRow> listRows=FontLiteralScan.ReadAllowList(RepoPaths.Root());
			Assert.IsTrue(listRows.Count>0);
			foreach(FontAllowRow fontAllowRow in listRows) {
				Assert.IsTrue(FontLiteralScan.ArraySorts.Contains(fontAllowRow.Sort),fontAllowRow.File+" "+fontAllowRow.Code);
				Assert.IsTrue(fontAllowRow.Count>0,fontAllowRow.File+" "+fontAllowRow.Code);
			}
		}

		private static FontLiteral Literal(string family,float size,FontStyle fontStyle) {
			return new FontLiteral { File=@"X\Y.Designer.cs",Line=1,Target="this.label1",Family=family,Size=size,Style=fontStyle };
		}
	}
}
