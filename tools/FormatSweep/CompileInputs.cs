// The compiler inputs of one project, as the build recorded them (Directory.Build.targets, ODRecordCompileInputs), and the compilation built
// from them. S3b-FMT, docs/port-notes/S3b-FMT.md.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace FormatSweep {
	public class CompileInputs {
		public string ProjectPath;
		public string InputsPath;
		public string AssemblyName;
		public string LangVersion;
		public List<string> ListDefines=new List<string>();
		public bool AllowUnsafe;
		public List<string> ListSources=new List<string>();
		public List<string> ListReferences=new List<string>();

		///<summary>The newest record under the project's obj\ folder (the WPF markup pass's temporary project excepted). Null if the project has not
		///been built since ODRecordCompileInputs came in.</summary>
		public static CompileInputs Find(string projectPath) {
			string folderObj=Path.Combine(Path.GetDirectoryName(projectPath),"obj");
			if(!Directory.Exists(folderObj)) {
				return null;
			}
			string name=Path.GetFileNameWithoutExtension(projectPath)+".compile-inputs.txt";
			FileInfo fileInfo=new DirectoryInfo(folderObj).EnumerateFiles(name,SearchOption.AllDirectories)
				.OrderByDescending(x => x.LastWriteTimeUtc)
				.FirstOrDefault();
			if(fileInfo==null) {
				return null;
			}
			return Read(fileInfo.FullName);
		}

		public static CompileInputs Read(string path) {
			CompileInputs compileInputs=new CompileInputs();
			compileInputs.InputsPath=path;
			foreach(string line in File.ReadAllLines(path)) {
				int index=line.IndexOf('=');
				if(index<0) {
					continue;
				}
				string key=line.Substring(0,index);
				string value=line.Substring(index+1);
				switch(key) {
					case "project":
						compileInputs.ProjectPath=value;
						break;
					case "assembly":
						compileInputs.AssemblyName=value;
						break;
					case "langversion":
						compileInputs.LangVersion=value;
						break;
					case "define":
						compileInputs.ListDefines.AddRange(value.Split(new[] { ';',',' },StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).Where(x => x!=""));
						break;
					case "unsafe":
						compileInputs.AllowUnsafe=value.Equals("true",StringComparison.OrdinalIgnoreCase);
						break;
					case "src":
						compileInputs.ListSources.Add(value);
						break;
					case "ref":
						compileInputs.ListReferences.Add(value);
						break;
				}
			}
			return compileInputs;
		}

		public CSharpParseOptions GetParseOptions() {
			LanguageVersion languageVersion=LanguageVersion.Default;
			if(!string.IsNullOrEmpty(LangVersion)) {
				LanguageVersionFacts.TryParse(LangVersion,out languageVersion);
			}
			return new CSharpParseOptions(languageVersion,DocumentationMode.None,SourceCodeKind.Regular,ListDefines);
		}

		///<summary>The compilation the compiler builds (without the analyzers and source generators: the scan needs only the types of the code
		///that is written by hand or by the designers).</summary>
		public CSharpCompilation CreateCompilation() {
			CSharpParseOptions parseOptions=GetParseOptions();
			List<SyntaxTree> listTrees=new List<SyntaxTree>();
			foreach(string path in ListSources.Distinct(StringComparer.OrdinalIgnoreCase)) {
				if(!File.Exists(path)) {
					continue;
				}
				SourceText sourceText;
				using(FileStream fileStream=File.OpenRead(path)) {
					sourceText=SourceText.From(fileStream,Encoding.UTF8,canBeEmbedded:false);
				}
				listTrees.Add(CSharpSyntaxTree.ParseText(sourceText,parseOptions,path));
			}
			List<MetadataReference> listReferences=ListReferences.Distinct(StringComparer.OrdinalIgnoreCase)
				.Where(x => File.Exists(x))
				.Select(x => (MetadataReference)MetadataReference.CreateFromFile(x))
				.ToList();
			CSharpCompilationOptions compilationOptions=new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
				allowUnsafe:AllowUnsafe,nullableContextOptions:NullableContextOptions.Disable);
			return CSharpCompilation.Create(AssemblyName,listTrees,listReferences,compilationOptions);
		}
	}
}
