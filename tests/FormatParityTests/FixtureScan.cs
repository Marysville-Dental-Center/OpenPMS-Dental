// The scan and the rewrite of tools\FormatSweep on a fixture compilation of a few lines (S3b-FMT, docs/port-notes/S3b-FMT.md): the
// framework's assemblies and CodeBase as references.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CodeBase;
using FormatSweep;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace FormatParityTests {
	internal static class FixtureScan {
		private const string PATH_FIXTURE=@"C:\fixture\Fixture.cs";

		private static CSharpCompilation Compile(string source) {
			SyntaxTree syntaxTree=CSharpSyntaxTree.ParseText(source,new CSharpParseOptions(LanguageVersion.CSharp8),PATH_FIXTURE);
			string trusted=(string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES");
			List<MetadataReference> listReferences=trusted.Split(Path.PathSeparator)
				.Where(x => x.EndsWith(".dll",StringComparison.OrdinalIgnoreCase))
				.Select(x => (MetadataReference)MetadataReference.CreateFromFile(x))
				.ToList();
			if(!trusted.Contains(typeof(ODNumberText).Assembly.Location)) {
				listReferences.Add(MetadataReference.CreateFromFile(typeof(ODNumberText).Assembly.Location));
			}
			return CSharpCompilation.Create("Fixture",new[] { syntaxTree },listReferences,
				new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
		}

		public static List<Site> Run(string source) {
			CSharpCompilation compilation=Compile(source);
			SiteFinder siteFinder=new SiteFinder("Fixture",@"C:\fixture",compilation.GetSemanticModel(compilation.SyntaxTrees[0]));
			siteFinder.Run();
			return siteFinder.ListSites;
		}

		public static string Rewrite(string source) {
			List<Site> listSites=Run(source).Where(x => x.ListEdits.Count>0).ToList();
			return Rewriter.RewriteText(source,listSites,PATH_FIXTURE,out int countEdits);
		}

		public static int CountErrors(string source) {
			return Compile(source).GetDiagnostics().Count(x => x.Severity==DiagnosticSeverity.Error);
		}
	}
}
