// What the tier 1 scan reads from the source (docs/specs/ui-refresh-plan.md section 5.4, docs/port-notes/UI-0b.md): which classes have a
// constructor that calls InitializeLayoutManager (a FormODBase type gets it in the scan only then; the five allocator windows of section 5.1
// never call it), and which types the program's own source creates with "new", so a UserControl of another assembly counts as hosted under
// LayoutManager when a program window creates it. Comments and string literals are skipped; a constructor's body is found by its braces.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace UiRefreshTests.Scan {
	public class SourceIndex {
		///<summary>Full class name (namespace.Class) to true when one of its constructors calls InitializeLayoutManager. A class without a
		///constructor in the source is not in it.</summary>
		public Dictionary<string,bool> DictCallsInitializeLayoutManager=new Dictionary<string,bool>();
		///<summary>Simple type names that the program's source (OpenPmsDental) creates with "new".</summary>
		public HashSet<string> SetNewInProgram=new HashSet<string>();
		///<summary>The folders the index reads, relative to the repository root.</summary>
		public static readonly string[] ArrayFolders=new string[] { "OpenPmsDental","CentralManager","CodeBase","DatabaseIntegrityCheck",
			"DocumentationBuilder","MySQLManager","ODR","OpenPmsDentalBusiness","OpenPmsDentalDbSettings","OpenPmsDentalGraph","OpenPmsDentalHL7",
			"RdlDesign","RdlViewer","ServiceManager","SparksToothChart","UpdateFileCopier","WpfControlsOD","xCrudGenerator" };
		private static SourceIndex _sourceIndex;
		private static readonly object _lock=new object();

		///<summary>The index of the repository, read once per process.</summary>
		public static SourceIndex Get() {
			lock(_lock) {
				if(_sourceIndex==null) {
					_sourceIndex=Read(RepoPaths.Root());
				}
				return _sourceIndex;
			}
		}

		public static SourceIndex Read(string root) {
			SourceIndex sourceIndex=new SourceIndex();
			foreach(string folder in ArrayFolders) {
				string path=Path.Combine(root,folder);
				if(!Directory.Exists(path)) {
					continue;
				}
				foreach(string file in Directory.EnumerateFiles(path,"*.cs",SearchOption.AllDirectories)) {
					if(IsBuildFolder(file)) {
						continue;
					}
					string text=StripCommentsAndStrings(File.ReadAllText(file));
					sourceIndex.AddFile(text,folder=="OpenPmsDental");
				}
			}
			return sourceIndex;
		}

		private static bool IsBuildFolder(string file) {
			string normalized=file.Replace('\\','/');
			return normalized.Contains("/bin/") || normalized.Contains("/obj/");
		}

		///<summary>True, false, or null when the class has no constructor in the source.</summary>
		public bool? CallsInitializeLayoutManager(string fullName) {
			bool value;
			if(DictCallsInitializeLayoutManager.TryGetValue(fullName,out value)) {
				return value;
			}
			return null;
		}

		///<summary>Indexes one file whose comments and strings are already removed.</summary>
		public void AddFile(string text,bool isProgram) {
			Match matchNamespace=Regex.Match(text,@"\bnamespace\s+([\w.]+)");
			string nameSpace=matchNamespace.Success ? matchNamespace.Groups[1].Value : "";
			HashSet<string> setClasses=new HashSet<string>();
			foreach(Match match in Regex.Matches(text,@"\bclass\s+(\w+)")) {
				setClasses.Add(match.Groups[1].Value);
			}
			//Methods of this file that call InitializeLayoutManager: a constructor may call it through one of them (FormCCDPrint's Init).
			List<string> listHelpers=new List<string>();
			foreach(Match match in Regex.Matches(text,@"\bvoid\s+(\w+)\s*\(")) {
				string bodyMethod=GetBody(text,match.Index+match.Length-1);
				if(match.Groups[1].Value!="InitializeLayoutManager" && bodyMethod!=null && Regex.IsMatch(bodyMethod,@"\bInitializeLayoutManager\s*\(")) {
					listHelpers.Add(match.Groups[1].Value);
				}
			}
			foreach(Match match in Regex.Matches(text,@"\b(?:public|private|protected|internal)\s+(\w+)\s*\(")) {
				string name=match.Groups[1].Value;
				if(!setClasses.Contains(name)) {
					continue;
				}
				string body=GetBody(text,match.Index+match.Length-1);
				if(body==null) {
					continue;
				}
				string fullName=nameSpace=="" ? name : nameSpace+"."+name;
				bool calls=Regex.IsMatch(body,@"\bInitializeLayoutManager\s*\(")
					|| listHelpers.Any(x => Regex.IsMatch(body,@"\b"+x+@"\s*\("));
				bool valueOld;
				if(DictCallsInitializeLayoutManager.TryGetValue(fullName,out valueOld)) {
					calls|=valueOld;
				}
				DictCallsInitializeLayoutManager[fullName]=calls;
			}
			if(isProgram) {
				foreach(Match match in Regex.Matches(text,@"\bnew\s+([\w.]+)\s*\(")) {
					string typeName=match.Groups[1].Value;
					int index=typeName.LastIndexOf('.');
					SetNewInProgram.Add(typeName.Substring(index+1));
				}
			}
		}

		///<summary>The body of the constructor whose parameter list opens at indexParen: from the first '{' after the list (past ": this(...)" or
		///": base(...)") to its matching '}'. Null when it is a declaration without a body.</summary>
		public static string GetBody(string text,int indexParen) {
			int start;
			int end;
			if(!GetBodyBounds(text,indexParen,out start,out end)) {
				return null;
			}
			return text.Substring(start,end-start+1);
		}

		///<summary>GetBody's start ('{') and end ('}') indexes; false when there is no body.</summary>
		public static bool GetBodyBounds(string text,int indexParen,out int start,out int end) {
			start=-1;
			end=-1;
			int depth=0;
			int i=indexParen;
			for(;i<text.Length;i++) {
				if(text[i]=='(') {
					depth++;
				}
				else if(text[i]==')') {
					depth--;
					if(depth==0) {
						break;
					}
				}
			}
			for(i++;i<text.Length;i++) {
				if(text[i]=='{') {
					break;
				}
				if(text[i]==';') {
					return false;
				}
			}
			if(i>=text.Length) {
				return false;
			}
			int indexStart=i;
			depth=0;
			for(;i<text.Length;i++) {
				if(text[i]=='{') {
					depth++;
				}
				else if(text[i]=='}') {
					depth--;
					if(depth==0) {
						start=indexStart;
						end=i;
						return true;
					}
				}
			}
			return false;
		}

		///<summary>The text with // and /* */ comments and string and character literals replaced by spaces, so braces and names inside them do
		///not count. Verbatim (@"") and interpolated strings are treated as plain strings; braces inside an interpolation hole are skipped with
		///the string.</summary>
		public static string StripCommentsAndStrings(string text) {
			StringBuilder stringBuilder=new StringBuilder(text.Length);
			int i=0;
			while(i<text.Length) {
				char c=text[i];
				char next=i+1<text.Length ? text[i+1] : '\0';
				if(c=='/' && next=='/') {
					while(i<text.Length && text[i]!='\n') {
						stringBuilder.Append(' ');
						i++;
					}
					continue;
				}
				if(c=='/' && next=='*') {
					int end=text.IndexOf("*/",i+2,StringComparison.Ordinal);
					end=end<0 ? text.Length : end+2;
					AppendBlank(stringBuilder,text,i,end);
					i=end;
					continue;
				}
				if(c=='@' && next=='"' || c=='$' && next=='@' || c=='@' && next=='$') {
					int startQuote=text.IndexOf('"',i);
					int end=startQuote+1;
					while(end<text.Length) {
						if(text[end]=='"') {
							if(end+1<text.Length && text[end+1]=='"') {
								end+=2;
								continue;
							}
							break;
						}
						end++;
					}
					end=Math.Min(text.Length,end+1);
					AppendBlank(stringBuilder,text,i,end);
					i=end;
					continue;
				}
				if(c=='"' || c=='\'') {
					int end=i+1;
					while(end<text.Length && text[end]!=c && text[end]!='\n') {
						if(text[end]=='\\') {
							end++;
						}
						end++;
					}
					end=Math.Min(text.Length,end+1);
					AppendBlank(stringBuilder,text,i,end);
					i=end;
					continue;
				}
				stringBuilder.Append(c);
				i++;
			}
			return stringBuilder.ToString();
		}

		private static void AppendBlank(StringBuilder stringBuilder,string text,int start,int end) {
			for(int j=start;j<end;j++) {
				stringBuilder.Append(text[j]=='\n' ? '\n' : ' ');
			}
		}
	}

	///<summary>Paths in the repository the tests read.</summary>
	public static class RepoPaths {
		private static string _root;

		///<summary>The repository root: the first folder above the test's output folder that holds OpenDental.sln.</summary>
		public static string Root() {
			if(_root!=null) {
				return _root;
			}
			for(DirectoryInfo directoryInfo=new DirectoryInfo(AppContext.BaseDirectory);directoryInfo!=null;directoryInfo=directoryInfo.Parent) {
				if(File.Exists(Path.Combine(directoryInfo.FullName,"OpenDental.sln"))) {
					_root=directoryInfo.FullName;
					return _root;
				}
			}
			throw new DirectoryNotFoundException("Repository root (OpenDental.sln) not found above "+AppContext.BaseDirectory);
		}

		public static string DataFile(string name) {
			return Path.Combine(Root(),"tests","UiRefreshTests","Data",name);
		}
	}
}
