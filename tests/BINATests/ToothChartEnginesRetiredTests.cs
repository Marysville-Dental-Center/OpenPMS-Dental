// The old tooth chart engines stay retired (package TC-9a, docs/port-notes/TC-9a.md; the TC-9a row of docs/specs/wpf-tooth-chart-plan.md). The
// DirectX 9 engine (through SharpDX, which needed d3dx9_43.dll from Microsoft's legacy DirectX End-User Runtime) and the OpenGL engine (through
// Tao) are deleted with their fallback (DirectX, then OpenGL, then Simple 2D), the DirectX runtime check and the DirectX format helpers. The
// chart draws with the WPF chart or the Simple 2D chart only: a stored DirectX (0) or OpenGL (2) choice starts the WPF chart, which falls back to
// Simple 2D with one log line, and a stored Simple 2D (1) stays 2D (O-30). DrawingMode keeps its three stored values. Replaces
// ToothChartFallbackTests (BIN-A, TC-8), whose tests of the old engines' fallback went with that code; its tests of what stays are here.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalBusiness;
using OpenPmsDentalBusiness.SheetFramework;
using SparksToothChart;

namespace BINATests {
	[TestClass]
	public class ToothChartEnginesRetiredTests {
		///<summary>The files TC-9a deleted.</summary>
		private static readonly string[] _arrayFilesDeleted={ @"SparksToothChart\ToothChartDirectX.cs",@"SparksToothChart\ToothChartDirectX.Designer.cs",
			@"SparksToothChart\ToothChartDirectX.resx",@"SparksToothChart\ToothChartOpenGL.cs",@"SparksToothChart\ToothChartOpenGL.Designer.cs",
			@"SparksToothChart\ToothChartOpenGL.resx",@"CodeBase\Controls\OpenGLWinFormsControl.cs" };

		///<summary>The members of the wrapper TC-9a deleted: the old engines' properties, the DirectX runtime check and its log line, the old engines'
		///fallback, the DirectX format helpers and the SharpDX exception test.</summary>
		private static readonly string[] _arrayMembersDeleted={ "D3DX_DLL","IsDirectXRuntimeAvailable","CanLoadLibrary","GetDirectXRuntimeLogLine",
			"DescribeLoadedD3dx","GetFallbackDrawMode","StartFirstWorkingMode","TryStartDrawMode","DisposeDirectX","UseHardware","AutoFinish",
			"PreferredPixelFormatNumber","DeviceFormat","GetPreferredDirectXFormat","TestDirectXFormat","IsGraphicsDeviceException","LoadLibrary",
			"GetModuleFileName","FreeLibrary" };

		///<summary>The source without its comment lines (a line whose text starts with //, the XML comments included), so the notes that say what was
		///retired do not count.</summary>
		private static string GetCode(string source) {
			return string.Join("\n",source.Replace("\r\n","\n").Split('\n').Where(x => !x.TrimStart().StartsWith("//")));
		}

		[TestMethod]
		public void OldEngineFiles_AreGone() {
			string root=RepoFiles.Root();
			foreach(string relative in _arrayFilesDeleted) {
				Assert.IsFalse(File.Exists(Path.Combine(root,relative)),relative+" was deleted in TC-9a.");
			}
			Assert.AreEqual(0,RepoFiles.Files("ToothChartDirectX*","ToothChartOpenGL*","OpenGLWinFormsControl*").Count,
				"No copy of the old engines' files is left in the repository.");
		}

		[TestMethod]
		public void ChartAssembly_HasNoOldEngine_AndReferencesNoSharpDXOrTao() {
			Assembly assemblyChart=typeof(ToothChartWrapper).Assembly;
			List<string> listTypes=assemblyChart.GetTypes().Select(x => x.FullName).ToList();
			Assert.IsFalse(listTypes.Any(x => x.Contains("ToothChartDirectX") || x.Contains("ToothChartOpenGL")),string.Join(", ",
				listTypes.Where(x => x.Contains("DirectX") || x.Contains("OpenGL"))));
			List<string> listReferences=assemblyChart.GetReferencedAssemblies().Select(x => x.Name).ToList();
			Assert.IsFalse(listReferences.Any(x => x.StartsWith("SharpDX") || x.StartsWith("Tao.")),"SparksToothChart references "+string.Join(", ",listReferences));
			Assert.IsNull(typeof(CodeBase.ODFileUtils).Assembly.GetType("CodeBase.OpenGLWinFormsControl"),"CodeBase's OpenGL control was deleted.");
			//The chart project names no SharpDX or Tao reference; the program's own (OpenPmsDental, CodeBase, UnitTests) went in TC-9b, with the
			//five DLLs (BINBTests, SharpDXTaoRemovedTests).
			string project=RepoFiles.Read(@"SparksToothChart\SparksToothChart.csproj");
			StringAssert.DoesNotMatch(project,new Regex(@"<Reference\s+Include=""(SharpDX|Tao)"),"SparksToothChart.csproj");
			StringAssert.DoesNotMatch(project,new Regex(@"ToothChartDirectX|ToothChartOpenGL"),"SparksToothChart.csproj");
		}

		[TestMethod]
		public void Wrapper_HasNoOldEngineMember_OrCode() {
			BindingFlags bindingFlags=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static|BindingFlags.Instance|BindingFlags.DeclaredOnly;
			List<string> listMembers=typeof(ToothChartWrapper).GetMembers(bindingFlags).Select(x => x.Name).ToList();
			foreach(string member in _arrayMembersDeleted) {
				Assert.IsFalse(listMembers.Contains(member),"ToothChartWrapper."+member+" was deleted in TC-9a.");
			}
			string code=GetCode(RepoFiles.Read(@"SparksToothChart\ToothChartWrapper.cs"));
			StringAssert.DoesNotMatch(code,new Regex(@"\bToothChartDirectX\b|\bToothChartOpenGL\b|\bSharpDX\b|\bTao\.|d3dx9|LoadLibrary|DrawingMode\.OpenGL"),
				"The wrapper has no code of the old engines.");
			foreach(string relative in Directory.GetFiles(Path.Combine(RepoFiles.Root(),"SparksToothChart"),"*.cs",SearchOption.AllDirectories)) {
				if(relative.Contains(@"\bin\") || relative.Contains(@"\obj\")) {
					continue;
				}
				StringAssert.DoesNotMatch(GetCode(File.ReadAllText(relative)),new Regex(@"\busing\s+(SharpDX|Tao)\b|\bSharpDX\.\w|\bTao\.(OpenGl|Platform)\b"),relative);
			}
			//The relay: no start-up DirectX search and no ToothChartSettingsUse (TC-8's note: every caller applies the same settings).
			string relay=GetCode(RepoFiles.Read(@"WpfControlsOD\UI\ToothChart\ToothChartRelay.cs"));
			StringAssert.DoesNotMatch(relay,new Regex(@"ChooseStartupDirectXFormat|ToothChartSettingsUse|GetPreferredDirectXFormat|TestDirectXFormat"),"The relay.");
			StringAssert.Contains(relay,"public static void ApplyComputerSettings(ToothChartWrapper toothChartWrapper) {");
			Assert.IsFalse(RepoFiles.Read(@"OpenPmsDental\Main Modules\FormOpenDental.cs").Contains("ChooseStartupDirectXFormat"),
				"FormOpenDental's start-up search call went with the search.");
		}

		[TestMethod]
		public void Engines_AreTheWpfChartAndSimple2D_AndDrawingModeKeepsItsThreeStoredValues() {
			CollectionAssert.AreEqual(new[] { "Simple2D","Wpf3D" },Enum.GetNames(typeof(ToothChartEngine)));
			Assert.AreEqual(1,(int)ToothChartEngine.Simple2D,"Simple2D keeps the value of DrawingMode.Simple2D.");
			Assert.AreEqual(3,(int)ToothChartEngine.Wpf3D);
			//The stored computerpref.GraphicsSimple: 0, 1 and 2 as before, so a return to Open Dental reads them as it did (decision 22).
			CollectionAssert.AreEqual(new[] { "DirectX","Simple2D","OpenGL" },Enum.GetNames(typeof(DrawingMode)));
			Assert.AreEqual(0,(int)DrawingMode.DirectX);
			Assert.AreEqual(1,(int)DrawingMode.Simple2D);
			Assert.AreEqual(2,(int)DrawingMode.OpenGL);
			//The fallback kinds: none for the old engines any more.
			Assert.IsFalse(Enum.GetNames(typeof(ToothChartFallbackKind)).Contains("DrawMode"));
		}

		[TestMethod]
		public void Wrapper_FallsBackWithoutADialog() {
			string wrapper=RepoFiles.Read(@"SparksToothChart\ToothChartWrapper.cs");
			StringAssert.DoesNotMatch(wrapper,new Regex(@"MessageBox\.|MsgBox\."),"A fallback never shows a dialog.");
			//TC-8: a stored DirectX or OpenGL request starts the WPF chart, which falls back to Simple 2D with one log line (Wpf3DStart).
			StringAssert.Contains(wrapper,"if(value!=DrawingMode.Simple2D) {");
			StringAssert.Contains(wrapper,"Engine=ToothChartEngine.Wpf3D;//TC-8: the stored DirectX and OpenGL choices start the WPF chart");
			StringAssert.Contains(wrapper,"LogFallbackOnce(ToothChartFallbackKind.Wpf3DStart,\"The \"+ToothChartEngine.Wpf3D+\" tooth chart could not start (\"");
			//Setup > Graphics tests no DirectX format (TC-8 rebuilt it).
			Assert.IsFalse(RepoFiles.Read(@"OpenPmsDental\Forms\FormGraphics.cs").Contains("TestDirectXFormat"));
		}

		[TestMethod]
		public void StoredDirectXAndOpenGLRequests_StartTheWpfChart_AndSimple2DStays_O30() {
			//The wrapper's DrawMode is the stored choice. 0 (DirectX) and 2 (OpenGL) start the WPF chart, and 1 stays the 2D chart. On this test's own
			//thread: the wrapper makes no WPF object on a caller's thread, and its picture comes from the chart's render thread. Moved from
			//ToothChartFallbackTests (TC-8).
			foreach(DrawingMode drawingMode in new[] { DrawingMode.DirectX,DrawingMode.OpenGL,DrawingMode.Simple2D }) {
				using(ToothChartWrapper wrapper=new ToothChartWrapper()) {
					wrapper.Size=new Size(410,307);
					int countBefore=ToothChartWrapper.GetCountFallbackMessages();
					wrapper.DrawMode=drawingMode;
					Assert.AreEqual(drawingMode==DrawingMode.Simple2D ? ToothChartEngine.Simple2D : ToothChartEngine.Wpf3D,wrapper.Engine,drawingMode.ToString());
					Assert.AreEqual(drawingMode==DrawingMode.Simple2D ? DrawingMode.Simple2D : DrawingMode.DirectX,wrapper.DrawMode,
						drawingMode+": the WPF chart reads as the stored DirectX it replaces (it has no stored form).");
					using(Bitmap bitmap=wrapper.GetBitmap()) {
						Assert.AreEqual(new Size(410,307),bitmap.Size);
					}
					List<string> listMessages=ToothChartWrapper.GetFallbackMessagesSince(countBefore);
					Assert.AreEqual(0,listMessages.Count,drawingMode+": "+string.Join("; ",listMessages));
				}
			}
			//Engine starts only the two engines left: Simple2D, and Wpf3D.
			using(ToothChartWrapper wrapper=new ToothChartWrapper()) {
				wrapper.Size=new Size(410,307);
				wrapper.Engine=ToothChartEngine.Wpf3D;
				Assert.AreEqual(ToothChartEngine.Wpf3D,wrapper.Engine);
				wrapper.Engine=ToothChartEngine.Simple2D;
				Assert.AreEqual(ToothChartEngine.Simple2D,wrapper.Engine);
				Assert.AreEqual(DrawingMode.Simple2D,wrapper.DrawMode);
			}
		}

		[TestMethod]
		public void BusinessLayerChartImage_IsAPlainImageOfTheRightSize() {
			//The business layer's tooth chart image was drawn by Sparks3D. It is now a plain image that says so, at the size callers expect. Moved from
			//ToothChartFallbackTests (BIN-A).
			using Image image=ToothChartHelper.GetImageNotAvailable(500,370,Color.White,Color.Black);
			Assert.AreEqual(new Size(500,370),image.Size);
			Assert.AreEqual(Color.White.ToArgb(),((Bitmap)image).GetPixel(0,0).ToArgb());
			using Image imagePerio=ToothChartHelper.GetToothChartImageProcess(null,new List<Procedure>(),new List<ToothInitial>(),
				OpenPmsDentalBusiness.ToothNumberingNomenclature.Universal,new List<PerioMeasure>(),new Dictionary<PrefName,Color>(),new List<Def>(),
				new List<Procedure>(),new List<ProcedureCode>(),new List<ProcedureCode>(),false,false,true,500,370,false,false);
			Assert.AreEqual(new Size(649,696),imagePerio.Size,"The perio image keeps the graphical perio chart's size.");
		}
	}
}
