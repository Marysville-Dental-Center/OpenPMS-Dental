// Tests for the tooth chart's draw mode fallback (package BIN-A, docs/port-notes/BIN-A.md). SparksToothChart is the only chart now. Its DirectX
// mode needs d3dx9_43.dll from Microsoft's legacy DirectX End-User Runtime, which a clean Windows lacks, so a mode that cannot start falls back to
// the next one (DirectX, OpenGL, Simple 2D) without an error dialog and is logged once. The fallback choice is tested through
// ToothChartWrapper.StartFirstWorkingMode with fake starters, so no window or graphics device is needed.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalBusiness;
using OpenPmsDentalBusiness.SheetFramework;
using SparksToothChart;

namespace BINATests {
	[TestClass]
	public class ToothChartFallbackTests {
		///<summary>Runs StartFirstWorkingMode with a starter that fails for the modes in arrayFailing (throwing for DirectX, as a missing D3DX does,
		///and returning false otherwise). Returns the started mode; listTried and listLog get what happened.</summary>
		private static DrawingMode Run(DrawingMode drawingModeRequested,bool isDirectXRuntimeAvailable,DrawingMode[] arrayFailing,
			List<DrawingMode> listTried,List<string> listLog)
		{
			return ToothChartWrapper.StartFirstWorkingMode(drawingModeRequested,isDirectXRuntimeAvailable,drawingMode => {
				listTried.Add(drawingMode);
				if(Array.IndexOf(arrayFailing,drawingMode)<0) {
					return true;
				}
				if(drawingMode==DrawingMode.DirectX) {
					throw new DllNotFoundException("Unable to load DLL 'd3dx9_43.dll'");
				}
				return false;
			},listLog.Add);
		}

		[TestMethod]
		public void FallbackOrder_IsDirectXThenOpenGLThenSimple2D() {
			Assert.AreEqual(DrawingMode.OpenGL,ToothChartWrapper.GetFallbackDrawMode(DrawingMode.DirectX));
			Assert.AreEqual(DrawingMode.Simple2D,ToothChartWrapper.GetFallbackDrawMode(DrawingMode.OpenGL));
			Assert.AreEqual(DrawingMode.Simple2D,ToothChartWrapper.GetFallbackDrawMode(DrawingMode.Simple2D),"Simple 2D is the last resort.");
		}

		[TestMethod]
		public void MissingDirectXRuntime_SkipsDirectXAndStartsOpenGL() {
			List<DrawingMode> listTried=new List<DrawingMode>();
			List<string> listLog=new List<string>();
			DrawingMode drawingMode=Run(DrawingMode.DirectX,false,new DrawingMode[0],listTried,listLog);
			Assert.AreEqual(DrawingMode.OpenGL,drawingMode);
			CollectionAssert.AreEqual(new[] { DrawingMode.OpenGL },listTried,"DirectX is not even tried without d3dx9_43.dll.");
			Assert.AreEqual(1,listLog.Count);
			StringAssert.Contains(listLog[0],ToothChartWrapper.D3DX_DLL);
			Assert.AreEqual("d3dx9_43.dll",ToothChartWrapper.D3DX_DLL);
		}

		[TestMethod]
		public void MissingDirectXRuntimeAndNoOpenGL_EndsInSimple2D() {
			List<DrawingMode> listTried=new List<DrawingMode>();
			List<string> listLog=new List<string>();
			DrawingMode drawingMode=Run(DrawingMode.DirectX,false,new[] { DrawingMode.OpenGL },listTried,listLog);
			Assert.AreEqual(DrawingMode.Simple2D,drawingMode);
			CollectionAssert.AreEqual(new[] { DrawingMode.OpenGL,DrawingMode.Simple2D },listTried);
			Assert.AreEqual(2,listLog.Count);
		}

		[TestMethod]
		public void DirectXThatThrows_FallsBackToOpenGL() {
			List<DrawingMode> listTried=new List<DrawingMode>();
			List<string> listLog=new List<string>();
			DrawingMode drawingMode=Run(DrawingMode.DirectX,true,new[] { DrawingMode.DirectX },listTried,listLog);
			Assert.AreEqual(DrawingMode.OpenGL,drawingMode);
			CollectionAssert.AreEqual(new[] { DrawingMode.DirectX,DrawingMode.OpenGL },listTried);
			Assert.AreEqual(1,listLog.Count);
			StringAssert.Contains(listLog[0],"DllNotFoundException","The log says why DirectX did not start.");
			StringAssert.Contains(listLog[0],"OpenGL");
		}

		[TestMethod]
		public void DirectXAndOpenGLFailing_EndsInSimple2D() {
			List<DrawingMode> listTried=new List<DrawingMode>();
			List<string> listLog=new List<string>();
			DrawingMode drawingMode=Run(DrawingMode.DirectX,true,new[] { DrawingMode.DirectX,DrawingMode.OpenGL },listTried,listLog);
			Assert.AreEqual(DrawingMode.Simple2D,drawingMode);
			CollectionAssert.AreEqual(new[] { DrawingMode.DirectX,DrawingMode.OpenGL,DrawingMode.Simple2D },listTried);
			Assert.AreEqual(2,listLog.Count);
		}

		[TestMethod]
		public void WorkingModes_StartAsRequested_WithoutLogging() {
			foreach(DrawingMode drawingModeRequested in new[] { DrawingMode.DirectX,DrawingMode.OpenGL,DrawingMode.Simple2D }) {
				List<DrawingMode> listTried=new List<DrawingMode>();
				List<string> listLog=new List<string>();
				Assert.AreEqual(drawingModeRequested,Run(drawingModeRequested,true,new DrawingMode[0],listTried,listLog));
				CollectionAssert.AreEqual(new[] { drawingModeRequested },listTried);
				Assert.AreEqual(0,listLog.Count);
			}
		}

		[TestMethod]
		public void OpenGLRequested_IsNotAffectedByAMissingDirectXRuntime() {
			List<DrawingMode> listTried=new List<DrawingMode>();
			List<string> listLog=new List<string>();
			Assert.AreEqual(DrawingMode.OpenGL,Run(DrawingMode.OpenGL,false,new DrawingMode[0],listTried,listLog));
			Assert.AreEqual(0,listLog.Count);
		}

		[TestMethod]
		public void Simple2DFailure_IsNotSwallowed() {
			//Simple 2D is the last resort, so its failure surfaces as before instead of leaving a chart with no control.
			try {
				ToothChartWrapper.StartFirstWorkingMode(DrawingMode.Simple2D,true,drawingMode => throw new InvalidOperationException("2D failed"),null);
				Assert.Fail("Expected the Simple 2D failure to surface.");
			}
			catch(InvalidOperationException ex) {
				Assert.AreEqual("2D failed",ex.Message);
			}
		}

		[TestMethod]
		public void DirectXRuntimeCheck_FindsTheInstalledFile() {
			bool isAvailable=ToothChartWrapper.IsDirectXRuntimeAvailable();
			Assert.AreEqual(isAvailable,ToothChartWrapper.IsDirectXRuntimeAvailable(),"Checked once per process.");
			//A 32-bit test process loads 32-bit system files from the SysWOW64 folder (System32 on 32-bit Windows), a 64-bit one (the lane's x64
			//test build, S3a-15) from System32.
			string folderSystem=Environment.Is64BitProcess?Environment.SystemDirectory:Environment.GetFolderPath(Environment.SpecialFolder.SystemX86);
			string pathSystem=Path.Combine(folderSystem,ToothChartWrapper.D3DX_DLL);
			if(File.Exists(pathSystem)) {
				Assert.IsTrue(isAvailable,pathSystem+" is installed, so DirectX can start.");
			}
		}

		[TestMethod]
		public void Wrapper_UsesTheFallbackWithoutADialog() {
			string wrapper=RepoFiles.Read(@"SparksToothChart\ToothChartWrapper.cs");
			//The old engines' fallback stays for Engine (the legacy recorder and the tests) until TC-9a deletes them.
			StringAssert.Contains(wrapper,"drawMode=StartFirstWorkingMode(value,IsDirectXRuntimeAvailable(),TryStartDrawMode,LogFallbackOnce);");
			StringAssert.DoesNotMatch(wrapper,new Regex(@"MessageBox\.|MsgBox\."),"A fallback never shows a dialog.");
			//Callers that test DirectX must treat any other engine as failure, because the chart may fall back to OpenGL instead of 2D. The format test
			//and search moved from FormGraphics to the wrapper in TC-2; since TC-8 they start the DirectX engine itself (Engine), because DrawMode's
			//DirectX starts the WPF chart, and only the legacy recorder and the tests call them.
			StringAssert.Contains(wrapper,"toothChartWrapperTest.Engine=ToothChartEngine.DirectX;");
			StringAssert.Contains(wrapper,"if(toothChartWrapperTest.Engine!=ToothChartEngine.DirectX) {");
			StringAssert.Contains(wrapper,"if(!ToothChartWrapper.IsDirectXRuntimeAvailable()) {");
			//TC-8: a stored DirectX or OpenGL request starts the WPF chart, which falls back to Simple 2D with one log line (Wpf3DStart).
			StringAssert.Contains(wrapper,"if(value!=DrawingMode.Simple2D) {");
			StringAssert.Contains(wrapper,"Engine=ToothChartEngine.Wpf3D;//TC-8: the stored DirectX and OpenGL choices start the WPF chart");
			StringAssert.Contains(wrapper,"LogFallbackOnce(ToothChartFallbackKind.Wpf3DStart,\"The \"+ToothChartEngine.Wpf3D+\" tooth chart could not start (\"");
			//Setup > Graphics tests no DirectX format (TC-8 rebuilt it), and the start-up search is a no-op: no computer is moved to OpenGL any more.
			Assert.IsFalse(RepoFiles.Read(@"OpenPmsDental\Forms\FormGraphics.cs").Contains("TestDirectXFormat"));
			string relay=RepoFiles.Read(@"WpfControlsOD\UI\ToothChart\ToothChartRelay.cs");
			Assert.IsFalse(relay.Contains("GetFallbackDrawMode("),"The relay moves no computer to the next old engine.");
			Assert.IsFalse(relay.Contains("GetPreferredDirectXFormat("),"The relay searches no DirectX format.");
			StringAssert.Contains(RepoFiles.Read(@"OpenPmsDental\Main Modules\FormOpenDental.cs"),"if(ToothChartRelay.ChooseStartupDirectXFormat(this)) {");
		}

		[TestMethod]
		public void StoredDirectXAndOpenGLRequests_StartTheWpfChart_AndSimple2DStays_O30() {
			//TC-8: the wrapper's DrawMode is the stored choice. 0 (DirectX) and 2 (OpenGL) start the WPF chart, so no computer reaches the old engines or
			//their fallback any more, and 1 stays the 2D chart. On this test's own thread: the wrapper makes no WPF object on a caller's thread, and
			//its picture comes from the chart's render thread.
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
		}

		[TestMethod]
		public void BusinessLayerChartImage_IsAPlainImageOfTheRightSize() {
			//The business layer's tooth chart image was drawn by Sparks3D. It is now a plain image that says so, at the size callers expect.
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
