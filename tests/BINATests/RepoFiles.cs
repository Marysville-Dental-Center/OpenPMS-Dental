// Repository helpers for the BIN-A tests: the root, file listing without build output, and the referenced assemblies of a built file.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Cecil;

namespace BINATests {
	internal static class RepoFiles {
		///<summary>The repository root: the first folder above the test assembly with LICENSE and NOTICE (not found by the solution's name, which the
		///rename may change; package REN-PROOF).</summary>
		public static string Root() {
			DirectoryInfo directoryInfo=new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
			while(directoryInfo!=null) {
				if(File.Exists(Path.Combine(directoryInfo.FullName,"LICENSE")) && File.Exists(Path.Combine(directoryInfo.FullName,"NOTICE"))) {
					return directoryInfo.FullName;
				}
				directoryInfo=directoryInfo.Parent;
			}
			Assert.Fail("Repository root not found above "+AppDomain.CurrentDomain.BaseDirectory);
			return null;
		}

		public static string Read(string relativePath) {
			return File.ReadAllText(Path.Combine(Root(),relativePath));
		}

		///<summary>True for build output, packages, git data, worktrees and this test project (whose own source names the removed binaries).</summary>
		private static bool IsSkipped(string relativePath) {
			string relative="\\"+relativePath.TrimStart('\\')+"\\";
			return relative.Contains(@"\bin\") || relative.Contains(@"\obj\") || relative.StartsWith(@"\packages\",StringComparison.OrdinalIgnoreCase)
				|| relative.StartsWith(@"\.claude\") || relative.StartsWith(@"\.git\") || relative.StartsWith(@"\tests\BINATests\",StringComparison.OrdinalIgnoreCase);
		}

		///<summary>Every file under the root that matches one of the patterns, relative to the root, without the skipped folders.</summary>
		public static List<string> Files(params string[] arraySearchPatterns) {
			string root=Root();
			List<string> listFiles=new List<string>();
			foreach(string searchPattern in arraySearchPatterns) {
				foreach(string file in Directory.EnumerateFiles(root,searchPattern,SearchOption.AllDirectories)) {
					string relative=file.Substring(root.Length).TrimStart('\\');
					if(!IsSkipped(relative)) {
						listFiles.Add(relative);
					}
				}
			}
			return listFiles;
		}

		///<summary>The names of the assemblies a built file references, read with Mono.Cecil from the metadata of a copy of its bytes, so the file
		///stays unlocked and nothing is loaded (S3a-8 of the .NET 10 move: .NET has no reflection-only load).</summary>
		public static List<string> ReferencedAssemblyNames(string path) {
			using(MemoryStream memoryStream=new MemoryStream(File.ReadAllBytes(path)))
			using(ModuleDefinition moduleDefinition=ModuleDefinition.ReadModule(memoryStream)) {
				return moduleDefinition.AssemblyReferences.Select(x => x.Name).ToList();
			}
		}
	}
}
