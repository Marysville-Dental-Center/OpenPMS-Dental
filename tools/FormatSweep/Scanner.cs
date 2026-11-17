// The scan of the shipped projects (S3b-FMT, docs/port-notes/S3b-FMT.md): each project's compilation from the inputs its last build
// recorded, every syntax tree of the repository's own source (not obj\ or bin\, not third-party\), and the code under "#if DEBUG"'s other
// branch too (a second parse without DEBUG). Used by FormatSweep.exe and by the guard test (tests\FormatParityTests).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace FormatSweep {
	public class ScanResult {
		public List<Site> ListSites=new List<Site>();
		///<summary>Projects of the list whose compile inputs were not found (not built since ODRecordCompileInputs came in).</summary>
		public List<string> ListProjectsMissing=new List<string>();
		///<summary>Per project: source files scanned, and the compiler errors of the compilation (no analyzers or generators run, so a few may
		///be expected; a type the scan needs is resolved all the same).</summary>
		public List<string> ListProjectNotes=new List<string>();
	}

	public static class Scanner {
		///<summary>The project files of shipped-projects.txt (relative to the root).</summary>
		public static List<string> ReadProjectList(string path) {
			return File.ReadAllLines(path).Select(x => x.Trim()).Where(x => x!="" && !x.StartsWith("#")).ToList();
		}

		public static ScanResult Scan(string root,IEnumerable<string> listProjectsRelative,Action<string> log) {
			ScanResult scanResult=new ScanResult();
			HashSet<string> hashKeys=new HashSet<string>();
			foreach(string projectRelative in listProjectsRelative) {
				string projectPath=Path.Combine(root,projectRelative);
				CompileInputs compileInputs=CompileInputs.Find(projectPath);
				if(compileInputs==null) {
					scanResult.ListProjectsMissing.Add(projectRelative);
					continue;
				}
				DateTime dateTimeStart=DateTime.Now;
				CSharpCompilation compilation=compileInputs.CreateCompilation();
				int countErrors=compilation.GetDiagnostics().Count(x => x.Severity==DiagnosticSeverity.Error);
				int countFiles=0;
				string name=Path.GetFileNameWithoutExtension(projectRelative);
				List<SyntaxTree> listTreesOwn=compilation.SyntaxTrees.Where(x => IsOwnSource(root,x.FilePath)).ToList();
				foreach(SyntaxTree syntaxTree in listTreesOwn) {
					countFiles++;
					AddSites(scanResult,hashKeys,name,root,compilation.GetSemanticModel(syntaxTree));
				}
				//The other branch of "#if DEBUG": the same files parsed without DEBUG, in a compilation with those trees replaced.
				CSharpParseOptions parseOptionsRelease=compileInputs.GetParseOptions()
					.WithPreprocessorSymbols(compileInputs.ListDefines.Where(x => x!="DEBUG"));
				List<SyntaxTree> listTreesDebug=listTreesOwn.Where(x => x.GetText().ToString().Contains("DEBUG")).ToList();
				if(listTreesDebug.Count>0) {
					CSharpCompilation compilationRelease=compilation;
					List<SyntaxTree> listTreesRelease=new List<SyntaxTree>();
					foreach(SyntaxTree syntaxTree in listTreesDebug) {
						SyntaxTree syntaxTreeRelease=CSharpSyntaxTree.ParseText(syntaxTree.GetText(),parseOptionsRelease,syntaxTree.FilePath);
						compilationRelease=compilationRelease.ReplaceSyntaxTree(syntaxTree,syntaxTreeRelease);
						listTreesRelease.Add(syntaxTreeRelease);
					}
					foreach(SyntaxTree syntaxTree in listTreesRelease) {
						AddSites(scanResult,hashKeys,name,root,compilationRelease.GetSemanticModel(syntaxTree));
					}
				}
				string note=name+": "+countFiles+" files, "+countErrors+" compiler errors without analyzers, "+(int)(DateTime.Now-dateTimeStart).TotalSeconds+" s";
				scanResult.ListProjectNotes.Add(note);
				log?.Invoke(note);
			}
			return scanResult;
		}

		///<summary>A site is kept once (a file compiled into two projects, or found again in the release parse) by its file, place and kind.</summary>
		private static void AddSites(ScanResult scanResult,HashSet<string> hashKeys,string project,string root,SemanticModel semanticModel) {
			SiteFinder siteFinder=new SiteFinder(project,root,semanticModel);
			siteFinder.Run();
			foreach(Site site in siteFinder.ListSites) {
				string key=site.RelativePath+"|"+site.Line+"|"+site.Column+"|"+site.Category+"|"+site.Code;
				if(hashKeys.Add(key)) {
					scanResult.ListSites.Add(site);
				}
			}
		}

		///<summary>True for the repository's own source: under the root, not in an obj\ or bin\ folder, not under third-party\.</summary>
		public static bool IsOwnSource(string root,string path) {
			if(string.IsNullOrEmpty(path)) {
				return false;
			}
			string relative=SiteFinder.GetRelativePath(root,path);
			if(relative.StartsWith("..") || Path.IsPathRooted(relative)) {
				return false;
			}
			string[] arrayParts=relative.Split('\\');
			if(arrayParts.Any(x => x.Equals("obj",StringComparison.OrdinalIgnoreCase) || x.Equals("bin",StringComparison.OrdinalIgnoreCase))) {
				return false;
			}
			return !arrayParts[0].Equals("third-party",StringComparison.OrdinalIgnoreCase);
		}
	}
}
