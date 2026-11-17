// The repository's root and source files, for the tests that read source (S3a-13).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.IO;

namespace Helper32Tests.Support {
	internal static class TestPaths {
		///<summary>The repository root: the first folder above the test assembly that holds LICENSE and NOTICE.</summary>
		public static string RepoRoot {
			get {
				DirectoryInfo directoryInfo=new DirectoryInfo(Path.GetDirectoryName(typeof(TestPaths).Assembly.Location));
				while(directoryInfo!=null) {
					if(File.Exists(Path.Combine(directoryInfo.FullName,"LICENSE")) && File.Exists(Path.Combine(directoryInfo.FullName,"NOTICE"))) {
						return directoryInfo.FullName;
					}
					directoryInfo=directoryInfo.Parent;
				}
				throw new InvalidOperationException("LICENSE and NOTICE not found above "+typeof(TestPaths).Assembly.Location);
			}
		}

		public static string ReadSource(params string[] arrayParts) {
			return File.ReadAllText(Path.Combine(RepoRoot,Path.Combine(arrayParts)));
		}
	}
}
