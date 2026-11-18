// The "Microsoft Sans Serif" scan and the literal test of the plain forms' control fonts (package UI-2a-4, docs/specs/ui-refresh-plan.md
// sections 5.1, 5.2 and 9, docs/port-notes/UI-2a.md). The scan reads the source of the folders in FontLiteralScan.ArrayFolders (each UI-2a-4
// slice adds its folders there) with comments skipped, and finds every line that names "Microsoft Sans Serif" or FontFamily.GenericSansSerif:
// in code outside InitializeComponent everywhere, and in the designer code (InitializeComponent) of the plain forms, of the FormODBase windows
// whose constructor does not call InitializeLayoutManager and of the UserControls no program window hosts. The designer code of the windows and
// UserControls under LayoutManager is exempt (the mapping of 5.1 changes those fonts at run time); "under LayoutManager" is the tier 1 scan's
// own list (WindowBuilder.GetScanTypes, section 5.4). A site that stays (print, document or content, 5.2) is a row of
// Data\font-literal-allow.tsv; any other is a screen font that goes through its UiTheme token. The control and form fonts of the in-scope
// designer code (this.x.Font, this.Font) are allowed by the scan until UI-2b and checked by the literal test instead: their family and size
// follow FontBody by the rule of 3.4, and a default font (FontBody's family and size, Regular) is removed so the control inherits. MS Chart
// element fonts are not control fonts and are not in the literal test. No database, no window shown.
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
		///Each later slice (the modules, UI and business folders of UI-9a-6, then the other projects) adds its folders here and its rows to the
		///allow list.</summary>
		public static readonly string[] ArrayFolders=new string[] {
			@"OpenPmsDental\Forms",
			@"OpenPmsDental\Forms Reports",
		};
		public const string PATH_ALLOW=@"tests\UiRefreshTests\Data\font-literal-allow.tsv";
		public const string HEADER_ALLOW="file\tcode\tcount\tsort\treason";
		///<summary>The sorts a row of the allow list may have (section 5.2): a printout, a document (on screen or saved, attached or exported), or
		///office content (sheet, eForm and dashboard-widget defaults, chart element fonts).</summary>
		public static readonly string[] ArraySorts=new string[] { "print","document","content" };
		private static readonly Regex _regexFontName=new Regex(@"Microsoft Sans Serif|\bGenericSansSerif\b");
		///<summary>A control's or the form's Font set to a literal font, as the designer writes it.</summary>
		private static readonly Regex _regexControlFont=new Regex(
			@"\b(?<target>this(?:\.\w+)*?)\.Font\s*=\s*new\s+(?:System\.Drawing\.)?Font\s*\(\s*""(?<family>[^""]+)""\s*,\s*(?<size>[0-9]+(?:\.[0-9]+)?)[Ff]?(?<rest>[^;]*);");
		private static readonly Regex _regexInitializeComponent=new Regex(@"\bvoid\s+InitializeComponent\s*\(");

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
				foreach(Match match in _regexControlFont.Matches(body)) {
					FontLiteral fontLiteral=new FontLiteral();
					fontLiteral.File=file;
					fontLiteral.Line=GetLine(textNoComments,range.Item1+match.Index);
					fontLiteral.ClassFullName=range.Item3;
					fontLiteral.Target=match.Groups["target"].Value;
					fontLiteral.Family=match.Groups["family"].Value;
					fontLiteral.Size=float.Parse(match.Groups["size"].Value,CultureInfo.InvariantCulture);
					fontLiteral.Style=ParseStyle(match.Groups["rest"].Value);
					fontFileScan.ListLiterals.Add(fontLiteral);
				}
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

		///<summary>True for a site the scan allows without a row: a literal control or form font in in-scope designer code while the tokens hold
		///today's font (until UI-2b); the literal test checks it.</summary>
		public static bool IsAllowedAsControlFont(FontSite fontSite) {
			return fontSite.IsDesignerCode && fontSite.FontLiteral!=null && UiTheme.IsFontMappingIdentity;
		}

		///<summary>The rule of 3.4 for a literal control or form font of a plain form or a window without InitializeLayoutManager (5.1). Null when
		///it follows the rule, else why not. Other families (Arial, Courier New, Wingdings) stay and are fine.</summary>
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
			if(isAtBodySize && fontLiteral.Style==FontStyle.Regular) {
				return "it is the default font (FontBody's family and size, Regular): remove it so the control inherits (5.1)";
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
					listErrors.Add("A row's sort must be print, document or content: "+fontAllowRow.File+" "+fontAllowRow.Code);
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
		}

		[TestMethod]
		public void ScanFile_SkipsComments_ExemptsLayoutManagerDesignerCode_AndFindsTheRest() {
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
