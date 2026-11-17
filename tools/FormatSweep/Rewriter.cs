// The mechanical rewrite (S3b-FMT, docs/port-notes/S3b-FMT.md): applies the sites' edits to each file, keeps the file's bytes everywhere
// else (encoding, BOM, line ends), and adds "using CodeBase;" where an edit needs it. A file that is not UTF-8 (Windows-1252) is read and
// written as Latin-1, byte for byte.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FormatSweep {
	public static class Rewriter {
		///<summary>Rewrites the files of the sites that have edits. Returns one line per file (or problem).</summary>
		public static List<string> Apply(IEnumerable<Site> sites) {
			List<string> listLines=new List<string>();
			foreach(IGrouping<string,Site> group in sites.Where(x => x.ListEdits.Count>0).GroupBy(x => x.FilePath,StringComparer.OrdinalIgnoreCase)) {
				string path=group.Key;
				byte[] arrayBytes=File.ReadAllBytes(path);
				bool hasBom=arrayBytes.Length>=3 && arrayBytes[0]==0xEF && arrayBytes[1]==0xBB && arrayBytes[2]==0xBF;
				Encoding encoding=new UTF8Encoding(hasBom,true);
				string text;
				try {
					text=encoding.GetString(arrayBytes,hasBom ? 3 : 0,arrayBytes.Length-(hasBom ? 3 : 0));
				}
				catch(DecoderFallbackException) {
					//Not UTF-8 (a Windows-1252 file of the original code): read byte for byte as Latin-1, which writes every byte back as it was; the
					//edits are ASCII. The scan read the file as UTF-8 with a replacement character for each bad byte, so the positions agree only if
					//no byte sequence was read as one character.
					encoding=Encoding.Latin1;
					hasBom=false;
					text=encoding.GetString(arrayBytes);
					if(new UTF8Encoding(false,false).GetString(arrayBytes).Length!=text.Length) {
						listLines.Add("SKIPPED (neither UTF-8 nor one byte a character): "+path);
						continue;
					}
				}
				string textNew=RewriteText(text,group.ToList(),path,out int countEdits);
				if(textNew==text) {
					continue;
				}
				byte[] arrayBytesNew=encoding.GetBytes(textNew);
				using(FileStream fileStream=new FileStream(path,FileMode.Create,FileAccess.Write)) {
					if(hasBom) {
						fileStream.Write(new byte[] { 0xEF,0xBB,0xBF },0,3);
					}
					fileStream.Write(arrayBytesNew,0,arrayBytesNew.Length);
				}
				listLines.Add(path+": "+countEdits+" edits");
			}
			return listLines;
		}

		///<summary>The text with the sites' edits applied (each edit once), and "using CodeBase;" added where an edit needs it.</summary>
		public static string RewriteText(string text,List<Site> listSites,string path,out int countEdits) {
			//Every edit once (the same place found in two projects or two parses gives the same edits).
			List<Edit> listEdits=new List<Edit>();
			HashSet<string> hashEdits=new HashSet<string>();
			foreach(Edit edit in listSites.SelectMany(x => x.ListEdits)) {
				if(hashEdits.Add(edit.Start+"|"+edit.Length+"|"+edit.Text)) {
					listEdits.Add(edit);
				}
			}
			//Replacements must not overlap; insertions at one position keep the order they were made in (a wrap's ")" before an outer one's).
			List<Edit> listSorted=listEdits.Select((x,i) => new { Edit=x,Index=i }).OrderBy(x => x.Edit.Start).ThenBy(x => x.Index).Select(x => x.Edit).ToList();
			for(int i=1;i<listSorted.Count;i++) {
				Edit editPrev=listSorted[i-1];
				if(listSorted[i].Start<editPrev.Start+editPrev.Length) {
					throw new InvalidOperationException("Overlapping edits in "+path+" at "+listSorted[i].Start);
				}
			}
			StringBuilder stringBuilder=new StringBuilder(text);
			//From the end, so earlier positions stay valid; at one position in reverse order of making, so the insertions end up in the order
			//they were made.
			foreach(Edit edit in listSorted.Select((x,i) => new { Edit=x,Order=i }).OrderByDescending(x => x.Edit.Start).ThenByDescending(x => x.Order).Select(x => x.Edit)) {
				stringBuilder.Remove(edit.Start,edit.Length);
				stringBuilder.Insert(edit.Start,edit.Text);
			}
			string textNew=stringBuilder.ToString();
			if(listSites.Any(x => x.NeedsUsingCodeBase)) {
				textNew=AddUsingCodeBase(textNew);
			}
			countEdits=listEdits.Count;
			return textNew;
		}

		///<summary>Adds "using CodeBase;" after the last using directive at the top of the file, unless the file has it or its code is in the
		///CodeBase namespace (or one inside it).</summary>
		public static string AddUsingCodeBase(string text) {
			CompilationUnitSyntax compilationUnit=(CompilationUnitSyntax)CSharpSyntaxTree.ParseText(text).GetRoot();
			if(compilationUnit.Usings.Any(x => x.Name!=null && x.Name.ToString()=="CodeBase" && x.Alias==null && x.StaticKeyword.IsKind(SyntaxKind.None))) {
				return text;
			}
			foreach(BaseNamespaceDeclarationSyntax namespaceDeclaration in compilationUnit.DescendantNodes().OfType<BaseNamespaceDeclarationSyntax>()) {
				string name=namespaceDeclaration.Name.ToString();
				if(name=="CodeBase" || name.StartsWith("CodeBase.")) {
					return text;
				}
				if(namespaceDeclaration.Usings.Any(x => x.Name!=null && x.Name.ToString()=="CodeBase" && x.Alias==null)) {
					return text;
				}
			}
			string newLine=text.Contains("\r\n") ? "\r\n" : "\n";
			if(compilationUnit.Usings.Count>0) {
				UsingDirectiveSyntax usingLast=compilationUnit.Usings.Last();
				int end=usingLast.FullSpan.End;//after its line end
				//Where the using's own line ends (its trailing trivia holds the line end).
				return text.Insert(end,"using CodeBase;"+newLine);
			}
			//No usings: before the first member (after the file's leading comments).
			SyntaxNode syntaxNodeFirst=(SyntaxNode)compilationUnit.Externs.FirstOrDefault() ?? compilationUnit.AttributeLists.FirstOrDefault() ?? (SyntaxNode)compilationUnit.Members.FirstOrDefault();
			if(syntaxNodeFirst==null) {
				return text;
			}
			return text.Insert(syntaxNodeFirst.SpanStart,"using CodeBase;"+newLine);
		}
	}
}
