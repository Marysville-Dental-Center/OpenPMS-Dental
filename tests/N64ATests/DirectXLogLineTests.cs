// The D3DX log line (N64-A, item A4 of docs/specs/native64-survey.md, row 12): at the first chart start, ToothChartWrapper writes one
// ToothChart log line naming the folder and bitness of the d3dx9_43.dll this process loaded. In the x86 run that is the 32-bit file, in the
// lane's x64 test build the 64-bit one. The check only loads and frees the library, as IsDirectXRuntimeAvailable always did; no chart and no
// window is made.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.IO;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SparksToothChart;

namespace N64ATests {
	[TestClass]
	public class DirectXLogLineTests {
		[TestMethod]
		public void LoadedD3dx_LineNamesItsFolderAndBitness() {
			bool isAvailable=ToothChartWrapper.IsDirectXRuntimeAvailable();
			string line=ToothChartWrapper.GetDirectXRuntimeLogLine();
			Console.WriteLine(line);
			//The record of this run, next to the test assembly (the lane's runners print no output of a passing test).
			File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"N64aD3dxLine."+(Environment.Is64BitProcess ? "x64" : "x86")+".txt"),
				isAvailable ? line : "(not available)");
			if(!isAvailable) {
				Assert.AreEqual("",line,"No line when the file could not be loaded (the chart's fallback line says so).");
				Assert.Inconclusive(ToothChartWrapper.D3DX_DLL+" is not installed for this bitness here; the DirectX chart falls back to OpenGL.");
			}
			string bitness=Environment.Is64BitProcess ? "64-bit" : "32-bit";
			StringAssert.StartsWith(line,"DirectX: "+ToothChartWrapper.D3DX_DLL+" loaded from ");
			StringAssert.EndsWith(line,", a native "+bitness+(Environment.Is64BitProcess ? " (x64) PE32+" : " (x86) PE32")+" file, in this "+bitness
				+" process.");
			string folder=line.Substring(("DirectX: "+ToothChartWrapper.D3DX_DLL+" loaded from ").Length);
			folder=folder.Substring(0,folder.IndexOf(", a native ",StringComparison.Ordinal));
			int indexNote=folder.IndexOf(" (the loader reports ",StringComparison.Ordinal);
			if(indexNote>0) {
				folder=folder.Substring(0,indexNote);
			}
			Assert.IsTrue(File.Exists(Path.Combine(folder,ToothChartWrapper.D3DX_DLL)),folder);
			if(!Environment.Is64BitProcess && Environment.Is64BitOperatingSystem) {
				Assert.IsFalse(string.Equals(folder.TrimEnd('\\'),Environment.SystemDirectory.TrimEnd('\\'),StringComparison.OrdinalIgnoreCase),
					"A 32-bit process names the 32-bit system folder, not System32: "+line);
			}
			Assert.AreEqual(line,ToothChartWrapper.GetDirectXRuntimeLogLine(),"One line per process.");
		}

		///<summary>The line for any loaded file: its folder (in a 32-bit process on 64-bit Windows, System32 is named as the 32-bit system folder
		///that WOW64 redirects it to), its PE bitness and the process's; and the line when the path could not be read.</summary>
		[TestMethod]
		public void DescribeLoadedD3dx_AnyFileAndNoPath() {
			string path=Path.Combine(Environment.SystemDirectory,"kernel32.dll");
			string folder=Environment.SystemDirectory;
			if(!Environment.Is64BitProcess && Environment.Is64BitOperatingSystem) {
				folder=Environment.GetFolderPath(Environment.SpecialFolder.SystemX86)+" (the loader reports "+Environment.SystemDirectory+", which WOW64 redirects)";
			}
			string line=ToothChartWrapper.DescribeLoadedD3dx(path,Environment.Is64BitProcess);
			Assert.AreEqual("DirectX: kernel32.dll loaded from "+folder+", "+PortableExecutableFile.Read(path).DescribeBitness()
				+", in this "+(Environment.Is64BitProcess ? "64-bit" : "32-bit")+" process.",line);
			string pathOther=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows),"explorer.exe");
			StringAssert.StartsWith(ToothChartWrapper.DescribeLoadedD3dx(pathOther,false),"DirectX: explorer.exe loaded from "
				+Path.GetDirectoryName(pathOther)+", ","Another folder is named as it is.");
			Assert.AreEqual("DirectX: d3dx9_43.dll loaded (its path could not be read) in this 64-bit process.",ToothChartWrapper.DescribeLoadedD3dx("",true));
		}

		[TestMethod]
		public void Source_TheLineIsWrittenOnceToTheToothChartLog() {
			string wrapper=TestSupport.ReadRepoFile(@"SparksToothChart\ToothChartWrapper.cs");
			StringAssert.Contains(wrapper,"_isDirectXRuntimeAvailable=CanLoadLibrary(D3DX_DLL,out string pathLoaded);");
			StringAssert.Contains(wrapper,"_directXRuntimeLogLine=DescribeLoadedD3dx(pathLoaded,Environment.Is64BitProcess);");
			StringAssert.Contains(wrapper,"CodeBase.Logger.WriteLine(_directXRuntimeLogLine,\"ToothChart\");");
		}
	}
}
