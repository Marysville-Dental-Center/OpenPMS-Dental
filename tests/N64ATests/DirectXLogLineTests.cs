// The D3DX log line (N64-A, item A4 of docs/specs/native64-survey.md, row 12) is gone with the DirectX chart (TC-9a, docs/port-notes/TC-9a.md):
// it named the folder and bitness of the d3dx9_43.dll the first chart start loaded, and since TC-9a nothing loads that file. These tests pin that
// no D3DX is loaded: a chart that starts and draws (the WPF chart, the Simple 2D chart and a stored DirectX or OpenGL choice) leaves no D3DX
// module in the process and no SharpDX or Tao assembly in the test domain, and the chart library imports no DirectX or OpenGL function. They
// run in the x86 runner and in the lane's x64 test build alike.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Cecil;
using OpenPmsDentalBusiness;
using SparksToothChart;

namespace N64ATests {
	[TestClass]
	public class DirectXLogLineTests {
		[TestMethod]
		public void ChartsThatStartAndDraw_LoadNoD3dx() {
			foreach(DrawingMode drawingMode in new[] { DrawingMode.DirectX,DrawingMode.OpenGL,DrawingMode.Simple2D }) {
				using(ToothChartWrapper wrapper=new ToothChartWrapper()) {
					wrapper.Size=new Size(410,307);
					wrapper.DrawMode=drawingMode;//a stored DirectX or OpenGL choice starts the WPF chart (TC-8)
					using(Bitmap bitmap=wrapper.GetBitmap()) {
						Assert.AreEqual(new Size(410,307),bitmap.Size,drawingMode.ToString());
					}
				}
			}
			List<string> listModules=new List<string>();
			using(Process process=Process.GetCurrentProcess()) {
				foreach(ProcessModule processModule in process.Modules) {
					listModules.Add(processModule.ModuleName);
				}
			}
			Console.WriteLine(listModules.Count+" modules in this "+(Environment.Is64BitProcess ? "64-bit" : "32-bit")+" process after three charts drew.");
			List<string> listD3dx=listModules.Where(x => x.StartsWith("d3dx",StringComparison.OrdinalIgnoreCase)).ToList();
			Assert.AreEqual(0,listD3dx.Count,"No D3DX is loaded: "+string.Join(", ",listD3dx));
			List<string> listAssemblies=AppDomain.CurrentDomain.GetAssemblies().Select(x => x.GetName().Name)
				.Where(x => x.StartsWith("SharpDX",StringComparison.OrdinalIgnoreCase) || x.StartsWith("Tao.",StringComparison.OrdinalIgnoreCase)).ToList();
			Assert.AreEqual(0,listAssemblies.Count,"No SharpDX or Tao assembly is loaded: "+string.Join(", ",listAssemblies));
		}

		///<summary>The chart library's native imports (P/Invoke module references), read with Mono.Cecil without loading anything: no D3DX, Direct3D
		///or OpenGL file, and no SharpDX or Tao assembly reference. The DirectX runtime check that loaded d3dx9_43.dll (LoadLibrary) went with it.</summary>
		[TestMethod]
		public void ChartLibrary_ImportsNoDirectXOrOpenGL() {
			string path=typeof(ToothChartWrapper).Assembly.Location;
			using(MemoryStream memoryStream=new MemoryStream(File.ReadAllBytes(path)))
			using(ModuleDefinition moduleDefinition=ModuleDefinition.ReadModule(memoryStream)) {
				List<string> listImports=moduleDefinition.ModuleReferences.Select(x => x.Name).ToList();
				Console.WriteLine("SparksToothChart imports: "+string.Join(", ",listImports));
				Assert.IsFalse(listImports.Any(x => Regex.IsMatch(x,@"^(d3dx|d3d9|opengl32|glu32)",RegexOptions.IgnoreCase)),string.Join(", ",listImports));
				List<string> listReferences=moduleDefinition.AssemblyReferences.Select(x => x.Name).ToList();
				Assert.IsFalse(listReferences.Any(x => x.StartsWith("SharpDX") || x.StartsWith("Tao.")),string.Join(", ",listReferences));
			}
			Assert.IsNull(typeof(ToothChartWrapper).GetField("D3DX_DLL"),"The D3DX file name went with the DirectX chart.");
			Assert.IsNull(typeof(ToothChartWrapper).GetMethod("GetDirectXRuntimeLogLine"),"The D3DX log line went with the DirectX chart.");
		}

		[TestMethod]
		public void Source_TheWrapperWritesNoD3dxLine() {
			string wrapper=TestSupport.ReadRepoFile(@"SparksToothChart\ToothChartWrapper.cs");
			string code=string.Join("\n",wrapper.Replace("\r\n","\n").Split('\n').Where(x => !x.TrimStart().StartsWith("//")));
			StringAssert.DoesNotMatch(code,new Regex(@"d3dx9|D3DX_DLL|CanLoadLibrary|IsDirectXRuntimeAvailable|DescribeLoadedD3dx|LoadLibrary"),
				"The wrapper loads no D3DX and writes no D3DX line.");
		}
	}
}
