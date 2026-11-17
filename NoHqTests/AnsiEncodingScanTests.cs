// S3a-3 (net10 plan): no shipped code uses Encoding.Default, which is the ANSI code page on .NET Framework but UTF-8 on .NET. Code that means the
// Windows ANSI page names it (CodeBase.ODEncodings.GetAnsiEncoding). A source scan of the C# projects of the solution filter, except the
// tests, the tools and the third-party sources; it needs no build, database or network.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NoHqTests {
	[TestClass]
	public class AnsiEncodingScanTests {
		[TestMethod]
		public void ShippedSource_HasNoEncodingDefault() {
			string pathRoot=RemovedAssembliesTests.GetRepositoryRoot();
			List<string> listFolders=GetShippedProjectFolders(pathRoot);
			Assert.IsTrue(listFolders.Any(x => Path.GetFileName(x)=="OpenPmsDentalBusiness"),"The scan must include the business layer.");
			Assert.IsTrue(listFolders.Count>=15,"Expected the shipped projects of the solution filter, found "+listFolders.Count+".");
			Regex regex=new Regex(@"\bEncoding\s*\.\s*Default\b");
			List<string> listOffenders=new List<string>();
			int countFiles=0;
			foreach(string folder in listFolders) {
				foreach(string path in Directory.EnumerateFiles(folder,"*.cs",SearchOption.AllDirectories)) {
					if(IsBuildFolder(folder,path)) {
						continue;
					}
					countFiles++;
					string[] arrayLines=File.ReadAllLines(path);
					for(int i=0;i<arrayLines.Length;i++) {
						if(regex.IsMatch(RemoveLineComment(arrayLines[i]))) {
							listOffenders.Add(path.Substring(pathRoot.Length+1)+":"+(i+1));
						}
					}
				}
			}
			Assert.IsTrue(countFiles>1000,"Expected the shipped source files, found "+countFiles+".");
			Assert.AreEqual(0,listOffenders.Count,"Encoding.Default is UTF-8 on .NET; use ODEncodings.GetAnsiEncoding() for the ANSI page: "
				+string.Join(", ",listOffenders));
		}

		[TestMethod]
		public void RemoveLineComment_KeepsCodeAndDropsComments() {
			Assert.AreEqual("x=Encoding.Default;",RemoveLineComment("x=Encoding.Default;//note"));
			Assert.AreEqual("\t\t",RemoveLineComment("\t\t///<summary>Encoding.Default is UTF-8 on .NET.</summary>"));
			Assert.AreEqual("s=\"http:",RemoveLineComment("s=\"http://example.test\";"),"Crude: the rest of a line after // in a string is not scanned.");
		}

		///<summary>The folders of the C# projects in the solution filter that ship or build shipped files: not tests\, tools\, third-party\, the
		///legacy UnitTests and UnitTestsCore, or this project.</summary>
		private static List<string> GetShippedProjectFolders(string pathRoot) {
			string pathSlnf=Directory.GetFiles(pathRoot,"*.slnf").Single();
			List<string> listFolders=new List<string>();
			foreach(Match match in Regex.Matches(File.ReadAllText(pathSlnf),"\"([^\"]+\\.csproj)\"")) {
				string pathProject=match.Groups[1].Value.Replace("\\\\","\\");
				string firstPart=pathProject.Split('\\')[0];
				if(new[] { "tests","tools","third-party","UnitTests","UnitTestsCore","NoHqTests" }.Contains(firstPart,StringComparer.OrdinalIgnoreCase)) {
					continue;
				}
				listFolders.Add(Path.GetDirectoryName(Path.Combine(pathRoot,pathProject)));
			}
			return listFolders.Distinct().ToList();
		}

		private static bool IsBuildFolder(string folder,string path) {
			string[] arrayParts=path.Substring(folder.Length+1).Split(Path.DirectorySeparatorChar);
			return arrayParts.Any(x => x.Equals("bin",StringComparison.OrdinalIgnoreCase) || x.Equals("obj",StringComparison.OrdinalIgnoreCase));
		}

		private static string RemoveLineComment(string line) {
			int index=line.IndexOf("//",StringComparison.Ordinal);
			if(index<0) {
				return line;
			}
			return line.Substring(0,index);
		}
	}
}
