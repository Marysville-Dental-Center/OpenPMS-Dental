// The source scan of UI-BRAND (docs/port-notes/UI-BRAND.md): the program is "Open PMS - Dental" ("Open PMS" for short) and no longer calls
// itself OD or Open Dental. Every string literal of the shipped sources (C# literals, .resx strings, XAML attribute values and texts, and the
// lines of the setup script and its pages, and the help texts of OpenPmsDental\Resources\PrefInfos.xml, which the program embeds and shows
// as the Preferences window's info pop-ups, each link of them read on its own) that names OD, Open Dental or OpenDental as a word must be listed with its reason in
// NoHqTests\Data\brand-kept.tsv: the GPL attribution, Open Dental's own programs and services, origin notes for adopted databases, database,
// file and preference names, vendor and protocol fields, identifiers and developer-only texts. So a new text in which the program names itself
// OD or Open Dental fails here, and so does a kept row the source no longer has (cut the list) and a kept row without a reason.
// The designers' [Category("OD")] attributes (the Visual Studio property grid's category of the program's own controls) are left out by rule.
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
	public class BrandTests {
		///<summary>OD as a word (case-sensitive), or Open Dental with or without a space or hyphen (any case).</summary>
		private static readonly Regex _regexBrand=new Regex(@"(?<![A-Za-z0-9_])OD(?![A-Za-z0-9_])|(?i:open[ \-]?dental)");
		///<summary>The text before a literal "OD" that makes it a designer category attribute.</summary>
		private static readonly Regex _regexCategoryBefore=new Regex(@"[\[,]\s*Category\s*\(\s*$");
		///<summary>A resx string: its name and value. Entries with a type or mimetype attribute (images, other objects) are not strings.</summary>
		private static readonly Regex _regexResxData=new Regex(@"<data\s+name=""([^""]*)""([^>]*)>\s*<value>(.*?)</value>",RegexOptions.Singleline);
		private static readonly Regex _regexXamlAttribute=new Regex(@"(\w[\w.:]*)=""([^""]*)""");
		private static readonly Regex _regexXamlText=new Regex(@">([^<>]+)<");
		///<summary>A help text of PrefInfos.xml: the text of a ControlText or Details element (its HTML written with XML escapes).</summary>
		private static readonly Regex _regexPrefInfoText=new Regex(@"<(ControlText|Details)>([^<]*)</\1>");
		///<summary>A link in a help text, up to the quote, space, tag or escape that ends it ("https:/" too, as one of the texts writes it).</summary>
		private static readonly Regex _regexUrl=new Regex(@"https?:/+[^\s'""<>&]+");
		///<summary>Text files outside the scanned extensions that the program embeds and shows: the preferences' help texts (Properties.Resources.PrefInfos,
		///shown by FormPreferences in FormWebBrowserPrefs).</summary>
		private static readonly string[] _arrayEmbeddedTextFiles=new string[] { "OpenPmsDental/Resources/PrefInfos.xml" };
		///<summary>Top-level folders that hold no shipped program: tests, developer tools, documentation, data, third-party and build input.</summary>
		private static readonly string[] _arrayFoldersSkipped=new string[] { "tests","UnitTests","UnitTestsCore","xCrudGenerator","DocumentationBuilder",
			"tools","third-party","site","docs","db","Required dlls","packages","icons","branding","NoHqTests","PluginExample" };
		///<summary>A kept row whose text is shorter than the whole literal must be at least this long, so that a short text never keeps every
		///literal that contains it.</summary>
		private const int MIN_PART_LENGTH=20;

		///<summary>One string literal of a source file that names the brand.</summary>
		public class BrandHit {
			public string File;
			public int Line;
			public string Text;
			public override string ToString() {
				return File+":"+Line+": "+Text;
			}
		}

		///<summary>One row of brand-kept.tsv.</summary>
		public class KeptRow {
			public string File;
			public string Text;
			public string Reason;
			public int LineInTsv;
		}

		[TestMethod]
		public void EveryLiteralThatNamesOdOrOpenDental_IsKeptWithAReason() {
			string pathRoot=RemovedAssembliesTests.GetRepositoryRoot();
			List<BrandHit> listHits=FindBrandHits(pathRoot);
			Assert.IsTrue(listHits.Count>150,"Only "+listHits.Count+" literals found; the scan does not see the sources.");
			List<KeptRow> listRows=ReadKeptRows(pathRoot,new List<string>());
			List<string> listNotKept=listHits.Where(x => !listRows.Any(y => IsMatch(y,x))).Select(x => x.ToString()).ToList();
			Assert.AreEqual(0,listNotKept.Count,"Texts that name the program OD or Open Dental. Say Open PMS (or Open PMS - Dental), or, when the text "
				+"is not the program naming itself, list it with its reason in NoHqTests\\Data\\brand-kept.tsv:\r\n"+string.Join("\r\n",listNotKept));
		}

		[TestMethod]
		public void EveryKeptRow_IsStillInTheSource() {
			string pathRoot=RemovedAssembliesTests.GetRepositoryRoot();
			List<BrandHit> listHits=FindBrandHits(pathRoot);
			List<KeptRow> listRows=ReadKeptRows(pathRoot,new List<string>());
			List<string> listStale=listRows.Where(x => !listHits.Any(y => IsMatch(x,y)))
				.Select(x => "brand-kept.tsv line "+x.LineInTsv+": "+x.File+"\t"+x.Text).ToList();
			Assert.AreEqual(0,listStale.Count,"Kept rows that no literal of the source has any more (remove them):\r\n"+string.Join("\r\n",listStale));
		}

		[TestMethod]
		public void EveryKeptRow_HasAReason_AndIsListedOnce() {
			string pathRoot=RemovedAssembliesTests.GetRepositoryRoot();
			List<string> listProblems=new List<string>();
			List<KeptRow> listRows=ReadKeptRows(pathRoot,listProblems);
			Assert.IsTrue(listRows.Count>100,"Only "+listRows.Count+" kept rows read.");
			for(int i=0;i<listRows.Count;i++) {
				KeptRow row=listRows[i];
				if(string.IsNullOrWhiteSpace(row.Reason)) {
					listProblems.Add("brand-kept.tsv line "+row.LineInTsv+" gives no reason: "+row.File+"\t"+row.Text);
				}
				if(!_regexBrand.IsMatch(row.Text)) {
					listProblems.Add("brand-kept.tsv line "+row.LineInTsv+" does not name OD or Open Dental: "+row.Text);
				}
				if(listRows.Take(i).Any(x => x.File==row.File && x.Text==row.Text)) {
					listProblems.Add("brand-kept.tsv line "+row.LineInTsv+" is listed twice: "+row.File+"\t"+row.Text);
				}
			}
			Assert.AreEqual(0,listProblems.Count,string.Join("\r\n",listProblems));
		}

		///<summary>The C# lexer sees every kind of literal and nothing in comments, character literals or preprocessor lines, and leaves the
		///designers' category attributes out.</summary>
		[TestMethod]
		public void Lexer_FindsLiteralsOfEveryKind_AndSkipsCommentsAndCategories() {
			string source=string.Join("\n",new string[] {
				"#region Don't read this \"OD\" here",
				"//label.Text=\"Open Dental\";",
				"/* \"Close Other OD Programs\" */",
				"[Category(\"OD\"),Description(\"A grid.\")]",
				"char c='\"'; string a=\"one OD\";",
				"string b=@\"two \"\"quoted\"\" Open Dental\";",
				"string d=$\"three {(x?\"OD\":\"y\")} OpenDental {{x}}\";",
				"string e=$@\"four {name} open-dental\";",
				"#endregion",
			});
			List<KeyValuePair<int,string>> listLiterals=LexCs(source);
			List<string> listBrand=listLiterals.Select(x => x.Value).Where(x => _regexBrand.IsMatch(x)).ToList();
			CollectionAssert.AreEqual(new List<string> {
				"one OD",
				"two \"quoted\" Open Dental",
				"three {\"OD\"\"y\"} OpenDental {x}",
				"four {} open-dental",
			},listBrand,string.Join(" | ",listBrand));
			Assert.AreEqual(5,GetLine(GetLineStarts(source),listLiterals.Single(x => x.Value=="one OD").Key));
		}

		///<summary>The PrefInfos reader reads the ControlText and Details texts only, each link on its own and the text without its links, and the
		///scan reads PrefInfos.xml, which the program embeds as a string resource and shows in the Preferences window.</summary>
		[TestMethod]
		public void PrefInfosLexer_ReadsHelpTextsAndLinksApart() {
			string source=string.Join("\n",new string[] {
				"<PrefInf>",
				"  <PrefName>OpenDentalPref</PrefName>",
				"  <ControlText>Open Dental option</ControlText>",
				"  <Details>See &lt;a href='https://www.opendental.com/manual/a.html' target='_blank'&gt;A&lt;/a&gt; and https:/opendental.com/b.html</Details>",
				"  <Details />",
				"</PrefInf>",
			});
			List<KeyValuePair<int,string>> listLiterals=LexPrefInfos(source);
			CollectionAssert.AreEqual(new List<string> {
				"Open Dental option",
				"https://www.opendental.com/manual/a.html",
				"https:/opendental.com/b.html",
				"See &lt;a href='' target='_blank'&gt;A&lt;/a&gt; and ",
			},listLiterals.Select(x => x.Value).ToList(),string.Join(" | ",listLiterals.Select(x => x.Value)));
			Assert.AreEqual(4,GetLine(GetLineStarts(source),listLiterals.Single(x => x.Value=="https:/opendental.com/b.html").Key));
			string pathRoot=RemovedAssembliesTests.GetRepositoryRoot();
			string resx=File.ReadAllText(Path.Combine(pathRoot,"OpenPmsDental","Properties","Resources.resx"));
			Assert.IsTrue(resx.Contains(@"<value>..\Resources\PrefInfos.xml;System.String"),"PrefInfos.xml is no longer an embedded string resource.");
			Assert.IsTrue(FindBrandHits(pathRoot).Any(x => x.File=="OpenPmsDental/Resources/PrefInfos.xml"),"The scan does not read PrefInfos.xml.");
		}

		///<summary>A row keeps a literal of its file when its text is the whole literal, or a part of it of at least MIN_PART_LENGTH characters.</summary>
		private static bool IsMatch(KeptRow row,BrandHit hit) {
			if(row.File!=hit.File) {
				return false;
			}
			if(row.Text==hit.Text) {
				return true;
			}
			return row.Text.Length>=MIN_PART_LENGTH && hit.Text.Contains(row.Text);
		}

		///<summary>The rows of NoHqTests\Data\brand-kept.tsv: file, text, reason. Lines that start with # and the header are skipped. Lines with
		///another number of fields are added to listProblems.</summary>
		public static List<KeptRow> ReadKeptRows(string pathRoot,List<string> listProblems) {
			string path=Path.Combine(pathRoot,"NoHqTests","Data","brand-kept.tsv");
			Assert.IsTrue(File.Exists(path),path+" is missing.");
			string[] arrayLines=File.ReadAllLines(path,Encoding.UTF8);
			List<KeptRow> listRows=new List<KeptRow>();
			bool hasHeader=false;
			for(int i=0;i<arrayLines.Length;i++) {
				string line=arrayLines[i];
				if(line=="" || line.StartsWith("#")) {
					continue;
				}
				if(!hasHeader) {
					hasHeader=true;
					if(line!="file\ttext\treason") {
						listProblems.Add("brand-kept.tsv's header is not file, text, reason: "+line);
					}
					continue;
				}
				string[] arrayFields=line.Split('\t');
				if(arrayFields.Length!=3) {
					listProblems.Add("brand-kept.tsv line "+(i+1)+" is not file, text, reason: "+line);
					continue;
				}
				listRows.Add(new KeptRow { File=arrayFields[0],Text=arrayFields[1],Reason=arrayFields[2],LineInTsv=i+1 });
			}
			return listRows;
		}

		///<summary>Every literal of the shipped sources that names the brand, with its repository-relative path ('/' separators) and line.</summary>
		public static List<BrandHit> FindBrandHits(string pathRoot) {
			List<BrandHit> listHits=new List<BrandHit>();
			foreach(string path in GetSourceFiles(pathRoot)) {
				string pathRelative=path.Substring(pathRoot.Length).TrimStart('\\','/').Replace('\\','/');
				string text=ReadText(path);
				string extension=Path.GetExtension(path).ToLowerInvariant();
				List<KeyValuePair<int,string>> listLiterals;
				if(extension==".cs") {
					listLiterals=LexCs(text);
				}
				else if(extension==".resx") {
					listLiterals=LexResx(text);
				}
				else if(extension==".xaml") {
					listLiterals=LexXaml(text);
				}
				else if(_arrayEmbeddedTextFiles.Contains(pathRelative)) {
					listLiterals=LexPrefInfos(text);
				}
				else {
					string[] arrayLines=text.Split('\n');
					listLiterals=new List<KeyValuePair<int,string>>();
					int index=0;
					for(int i=0;i<arrayLines.Length;i++) {
						listLiterals.Add(new KeyValuePair<int,string>(index,arrayLines[i]));
						index+=arrayLines[i].Length+1;
					}
				}
				int[] arrayLineStarts=GetLineStarts(text);
				for(int i=0;i<listLiterals.Count;i++) {
					if(_regexBrand.IsMatch(listLiterals[i].Value)) {
						listHits.Add(new BrandHit { File=pathRelative,Line=GetLine(arrayLineStarts,listLiterals[i].Key),Text=listLiterals[i].Value });
					}
				}
			}
			return listHits;
		}

		///<summary>The .cs, .resx and .xaml files of the shipped projects (and the files at the root), and the setup script and pages of
		///installer. Skips the folders in _arrayFoldersSkipped, build output (bin, obj) and folders whose names start with a dot.</summary>
		private static List<string> GetSourceFiles(string pathRoot) {
			List<string> listPaths=new List<string>();
			foreach(string path in Directory.GetFiles(pathRoot)) {
				if(IsScannedFile(path,false)) {
					listPaths.Add(path);
				}
			}
			foreach(string pathFolder in Directory.GetDirectories(pathRoot)) {
				string name=Path.GetFileName(pathFolder);
				if(name.StartsWith(".") || _arrayFoldersSkipped.Contains(name,StringComparer.OrdinalIgnoreCase)) {
					continue;
				}
				AddFiles(pathFolder,name.Equals("installer",StringComparison.OrdinalIgnoreCase),listPaths);
			}
			foreach(string pathEmbedded in _arrayEmbeddedTextFiles) {
				string path=Path.Combine(pathRoot,pathEmbedded.Replace('/',Path.DirectorySeparatorChar));
				Assert.IsTrue(File.Exists(path),path+" is missing: the scan reads it.");
				listPaths.Add(path);
			}
			listPaths.Sort(StringComparer.Ordinal);
			return listPaths;
		}

		private static void AddFiles(string pathFolder,bool isInstaller,List<string> listPaths) {
			foreach(string path in Directory.GetFiles(pathFolder)) {
				if(IsScannedFile(path,isInstaller)) {
					listPaths.Add(path);
				}
			}
			foreach(string pathSub in Directory.GetDirectories(pathFolder)) {
				string name=Path.GetFileName(pathSub);
				if(name.StartsWith(".") || name.Equals("bin",StringComparison.OrdinalIgnoreCase) || name.Equals("obj",StringComparison.OrdinalIgnoreCase)) {
					continue;
				}
				AddFiles(pathSub,isInstaller,listPaths);
			}
		}

		private static bool IsScannedFile(string path,bool isInstaller) {
			string extension=Path.GetExtension(path).ToLowerInvariant();
			if(extension==".cs" || extension==".resx" || extension==".xaml") {
				return true;
			}
			return isInstaller && (extension==".iss" || extension==".txt");
		}

		///<summary>The file's text with \n line ends: UTF-8 (a BOM is dropped), or Windows-1252 when the bytes are not UTF-8.</summary>
		private static string ReadText(string path) {
			byte[] arrayBytes=File.ReadAllBytes(path);
			int offset=0;
			if(arrayBytes.Length>=3 && arrayBytes[0]==0xEF && arrayBytes[1]==0xBB && arrayBytes[2]==0xBF) {
				offset=3;
			}
			string text;
			try {
				text=new UTF8Encoding(false,true).GetString(arrayBytes,offset,arrayBytes.Length-offset);
			}
			catch(DecoderFallbackException) {
				Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
				text=Encoding.GetEncoding(1252).GetString(arrayBytes);
			}
			return text.Replace("\r\n","\n");
		}

		///<summary>The string literals of C# source, each with the index where it starts, as the source writes them: escapes are not decoded, a
		///verbatim literal's "" is one quote, an interpolated literal's {{ and }} are one brace, and of its expressions only the braces and the
		///literals inside them are kept (so {name} is {}, and {(x?"a":"b")} is {"a""b"}). Comments, character
		///literals and preprocessor lines are skipped, and so is the literal "OD" of a [Category("OD")] attribute.</summary>
		public static List<KeyValuePair<int,string>> LexCs(string text) {
			List<KeyValuePair<int,string>> listLiterals=new List<KeyValuePair<int,string>>();
			int n=text.Length;
			int i=0;
			while(i<n) {
				char c=text[i];
				if(c=='#') {
					int start=i==0 ? 0 : text.LastIndexOf('\n',i-1)+1;
					if(text.Substring(start,i-start).Trim()=="") {
						i=SkipToLineEnd(text,i);
						continue;
					}
				}
				if(c=='/' && i+1<n && text[i+1]=='/') {
					i=SkipToLineEnd(text,i);
					continue;
				}
				if(c=='/' && i+1<n && text[i+1]=='*') {
					int end=text.IndexOf("*/",i+2,StringComparison.Ordinal);
					i=end<0 ? n : end+2;
					continue;
				}
				if(c=='\'') {
					int j=i+1;
					while(j<n && text[j]!='\'' && text[j]!='\n') {
						j+=text[j]=='\\' ? 2 : 1;
					}
					i=j+1;
					continue;
				}
				if(c=='"' && string.CompareOrdinal(text,i,"\"\"\"",0,3)==0) {//A raw string literal.
					int end=text.IndexOf("\"\"\"",i+3,StringComparison.Ordinal);
					if(end<0) {
						end=n;
					}
					listLiterals.Add(new KeyValuePair<int,string>(i,text.Substring(i+3,end-i-3)));
					i=end+3;
					continue;
				}
				bool isPrefixed=(c=='@' || c=='$') && i+1<n
					&& (text[i+1]=='"' || ((text[i+1]=='@' || text[i+1]=='$') && i+2<n && text[i+2]=='"'));
				if(c=='"' || isPrefixed) {
					int k=i;
					string prefix="";
					while(text[k]!='"') {
						prefix+=text[k];
						k++;
					}
					string literal;
					int next=ReadString(text,k,prefix.Contains("@"),prefix.Contains("$"),out literal);
					int startBefore=Math.Max(0,i-40);
					if(!(literal=="OD" && _regexCategoryBefore.IsMatch(text.Substring(startBefore,i-startBefore)))) {
						listLiterals.Add(new KeyValuePair<int,string>(i,literal));
					}
					i=next;
					continue;
				}
				i++;
			}
			return listLiterals;
		}

		private static int SkipToLineEnd(string text,int i) {
			int end=text.IndexOf('\n',i);
			return end<0 ? text.Length : end;
		}

		///<summary>The index where each line starts.</summary>
		private static int[] GetLineStarts(string text) {
			List<int> listStarts=new List<int> { 0 };
			for(int i=0;i<text.Length;i++) {
				if(text[i]=='\n') {
					listStarts.Add(i+1);
				}
			}
			return listStarts.ToArray();
		}

		///<summary>The 1-based line of the character at index.</summary>
		private static int GetLine(int[] arrayLineStarts,int index) {
			int found=Array.BinarySearch(arrayLineStarts,index);
			return found>=0 ? found+1 : ~found;
		}

		///<summary>Reads the literal whose opening quote is at i. Returns the index after its closing quote.</summary>
		private static int ReadString(string text,int i,bool isVerbatim,bool isInterpolated,out string literal) {
			int n=text.Length;
			StringBuilder stringBuilder=new StringBuilder();
			i++;
			while(i<n) {
				char c=text[i];
				if(isVerbatim) {
					if(c=='"') {
						if(i+1<n && text[i+1]=='"') {
							stringBuilder.Append('"');
							i+=2;
							continue;
						}
						literal=stringBuilder.ToString();
						return i+1;
					}
				}
				else {
					if(c=='\\') {
						stringBuilder.Append(text,i,Math.Min(2,n-i));
						i+=2;
						continue;
					}
					if(c=='"' || c=='\n') {
						literal=stringBuilder.ToString();
						return i+1;
					}
				}
				if(isInterpolated && c=='{') {
					if(i+1<n && text[i+1]=='{') {
						stringBuilder.Append('{');
						i+=2;
						continue;
					}
					//An expression: kept as written up to its matching brace, with the literals inside it.
					int depth=1;
					i++;
					stringBuilder.Append('{');
					while(i<n && depth>0) {
						char d=text[i];
						if(d=='{') {
							depth++;
							i++;
							continue;
						}
						if(d=='}') {
							depth--;
							i++;
							if(depth==0) {
								stringBuilder.Append('}');
							}
							continue;
						}
						if(d=='"') {
							string inner;
							i=ReadString(text,i,false,false,out inner);
							stringBuilder.Append('"').Append(inner).Append('"');
							continue;
						}
						if(d=='\'') {
							int j=i+1;
							while(j<n && text[j]!='\'') {
								j+=text[j]=='\\' ? 2 : 1;
							}
							i=j+1;
							continue;
						}
						if(d=='@' && i+1<n && text[i+1]=='"') {
							string inner;
							i=ReadString(text,i+1,true,false,out inner);
							continue;
						}
						i++;
					}
					continue;
				}
				if(isInterpolated && c=='}' && i+1<n && text[i+1]=='}') {
					stringBuilder.Append('}');
					i+=2;
					continue;
				}
				stringBuilder.Append(c);
				i++;
			}
			literal=stringBuilder.ToString();
			return i;
		}

		///<summary>The string entries of a .resx as "name=value" with the index where each starts, the value as the file writes it (XML escapes not decoded).</summary>
		public static List<KeyValuePair<int,string>> LexResx(string text) {
			List<KeyValuePair<int,string>> listLiterals=new List<KeyValuePair<int,string>>();
			foreach(Match match in _regexResxData.Matches(text)) {
				string attributes=match.Groups[2].Value;
				if(attributes.Contains("type=") || attributes.Contains("mimetype=")) {
					continue;
				}
				listLiterals.Add(new KeyValuePair<int,string>(match.Index,match.Groups[1].Value+"="+match.Groups[3].Value));
			}
			return listLiterals;
		}

		///<summary>The help texts of PrefInfos.xml (ControlText and Details), as the file writes them (XML escapes not decoded), with the index where
		///each starts. Each link is a literal of its own, and the text is read without its links, so that a kept link never keeps the words around it.</summary>
		public static List<KeyValuePair<int,string>> LexPrefInfos(string text) {
			List<KeyValuePair<int,string>> listLiterals=new List<KeyValuePair<int,string>>();
			foreach(Match match in _regexPrefInfoText.Matches(text)) {
				Group group=match.Groups[2];
				foreach(Match matchUrl in _regexUrl.Matches(group.Value)) {
					listLiterals.Add(new KeyValuePair<int,string>(group.Index+matchUrl.Index,matchUrl.Value));
				}
				listLiterals.Add(new KeyValuePair<int,string>(group.Index,_regexUrl.Replace(group.Value,"")));
			}
			return listLiterals;
		}

		///<summary>The attribute values of a .xaml as "name=value", and its element texts, with the index where each starts.</summary>
		public static List<KeyValuePair<int,string>> LexXaml(string text) {
			List<KeyValuePair<int,string>> listLiterals=new List<KeyValuePair<int,string>>();
			foreach(Match match in _regexXamlAttribute.Matches(text)) {
				listLiterals.Add(new KeyValuePair<int,string>(match.Index,match.Groups[1].Value+"="+match.Groups[2].Value));
			}
			foreach(Match match in _regexXamlText.Matches(text)) {
				string value=match.Groups[1].Value.Trim();
				if(value!="") {
					listLiterals.Add(new KeyValuePair<int,string>(match.Index,value));
				}
			}
			return listLiterals;
		}
	}
}
