// Tests of packages TC-8 and TC-8b (docs/specs/wpf-tooth-chart-plan.md, the TC-8 row and section 10's parity rows; docs/port-notes/TC-8.md and
// TC-8b.md): the engine choice moves to the WPF chart, chosen since TC-8b by Open Dental's own 3D settings. The mapping of every combination of
// the global setting, this computer's setting and GraphicsSimple (ENG-2), File > Graphics' save rule, the smoothed picture as the on-screen
// default (ENG-3), the status line (NEW-1), the graphical perio gate (ENG-5), the "Draw in software" switch set before the engine, Wpf3D's lack of
// a stored form, the wrapper's DrawMode request, the dashboard's MTA path, and the live view's fallback when WPF's render thread fails on the UI
// thread. No database: the settings are passed as ComputerPref objects (ToothChartRelay.ApplyComputerPref);
// the host tests (tests/ToothChartHostTests) check the same through the program on a scratch database.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental;
using OpenPmsDentalBusiness;
using SparksToothChart;
using SparksToothChart.Wpf3D;

namespace ToothChartImageTests {
	[TestClass]
	public class EngineChoiceTests {
		private static ComputerPref NewComputerPref(DrawingMode drawingMode,string directXFormat,YN chart3DThisComputer=YN.Unknown) {
			return new ComputerPref { ComputerName="SYNTHETIC-PC",GraphicsSimple=drawingMode,DirectXFormat=directXFormat,GraphicsUseHardware=true,
				PreferredPixelFormatNum=7,GraphicsDoubleBuffering=true,GraphicsUseDirectX11=chart3DThisComputer,ComputerOS=PlatformOD.Win32NT };
		}

		#region The mapping (TC-8b, ENG-2)
		///<summary>The YN values of computerpref.GraphicsUseDirectX11 the tests try: Unknown, Yes, No, and a number Open Dental never stores (read as
		///"Use the global setting").</summary>
		private static readonly YN[] _arrayChart3DThisComputer=new[] { YN.Unknown,YN.Yes,YN.No,(YN)7 };

		[TestMethod]
		public void Mapping_EveryCombinationOfTheGlobalSetting_ThisComputersSetting_AndGraphicsSimple() {
			//ENG-2: the 3D tooth chart (the WPF chart) when this computer's setting is "Use if available", or "Use the global setting" with the global
			//setting on; otherwise Open Dental's older technology (GraphicsSimple), of which only Simple 2D is left, whatever is stored. O-30 revised: a
			//computer stored as Simple 2D gets the 3D chart unless its setting is "Do not use".
			int countCases=0;
			foreach(bool isChart3DOnAllComputers in new[] { true,false }) {
				foreach(YN chart3DThisComputer in _arrayChart3DThisComputer) {
					foreach(DrawingMode drawingMode in new[] { DrawingMode.DirectX,DrawingMode.Simple2D,DrawingMode.OpenGL,(DrawingMode)5 }) {
						ComputerPref computerPref=NewComputerPref(drawingMode,"",chart3DThisComputer);
						bool isExpected3D=chart3DThisComputer==YN.Yes || (chart3DThisComputer!=YN.No && isChart3DOnAllComputers);
						string what="global "+isChart3DOnAllComputers+", this computer "+chart3DThisComputer+", GraphicsSimple "+drawingMode;
						Assert.AreEqual(isExpected3D,ToothChartRelay.IsChart3DUsed(isChart3DOnAllComputers,chart3DThisComputer),what);
						Assert.AreEqual(isExpected3D ? ToothChartEngine.Wpf3D : ToothChartEngine.Simple2D,
							ToothChartRelay.GetEngineForComputer(isChart3DOnAllComputers,computerPref),what);
						Assert.AreEqual(chart3DThisComputer,ToothChartRelay.GetChart3DThisComputer(computerPref),what);
						countCases++;
					}
				}
			}
			Assert.AreEqual(32,countCases);
			Assert.AreEqual(YN.Unknown,ToothChartRelay.GetChart3DThisComputer(null),"A null computer uses the global setting.");
			Assert.AreEqual(ToothChartEngine.Wpf3D,ToothChartRelay.GetEngineForComputer(true,null));
			Assert.AreEqual(ToothChartEngine.Simple2D,ToothChartRelay.GetEngineForComputer(false,null));
			//The global setting is Open Dental's preference, "Use if available" when the row or the cache is missing (default 1), as Open Dental 24.3 read it.
			Assert.IsTrue(ToothChartRelay.IsChart3DOnAllComputers(),"No database here, or the new database's 1: Use if available.");
			string relay=SourceText.ReadCode(@"WpfControlsOD\UI\ToothChart\ToothChartRelay.cs");
			StringAssert.Contains(SourceText.GetMethod(relay,"public static bool IsChart3DOnAllComputers() {"),
				"return PrefC.GetBoolSilent(PrefName.DirectX11ToothChartUseIfAvail,true);");
			StringAssert.Contains(SourceText.GetMethod(relay,"public static bool SetChart3DOnAllComputers(bool isChart3DOnAllComputers) {"),
				"return Prefs.UpdateBool(PrefName.DirectX11ToothChartUseIfAvail,isChart3DOnAllComputers);");
			//The stored values stay 0, 1 and 2: DrawingMode gets no new value, Wpf3D is the in-memory engine only, and YN keeps Open Dental's three.
			CollectionAssert.AreEqual(new[] { 0,1,2 },Enum.GetValues(typeof(DrawingMode)).Cast<int>().ToArray());
			Assert.IsFalse(Enum.GetNames(typeof(DrawingMode)).Contains("Wpf3D"),"Wpf3D has no stored form.");
			CollectionAssert.AreEqual(new[] { 0,1,2 },Enum.GetValues(typeof(YN)).Cast<int>().ToArray());
		}

		[TestMethod]
		public void SoftwareSwitch_TheSmoothedPictureIsTheDefault_OnlyTheLiveViewValueIsTheLiveView() {
			//ENG-3 (revises O-34 (b1)): the smoothed picture is every computer's default on screen; "Draw in software" unticked (WpfLiveView) is the quick
			//live view.
			Assert.IsTrue(ToothChartRelay.IsDrawnInSoftware(null),"A null computer: the default.");
			Assert.IsTrue(ToothChartRelay.IsDrawnInSoftware(NewComputerPref(DrawingMode.DirectX,"")),"Empty is the default, the smoothed picture.");
			Assert.IsTrue(ToothChartRelay.IsDrawnInSoftware(NewComputerPref(DrawingMode.DirectX,"0;Hardware;64;D16;R5G6B5;FourSamples")),
				"A DirectX device format an older build saved is the default.");
			Assert.IsTrue(ToothChartRelay.IsDrawnInSoftware(NewComputerPref(DrawingMode.DirectX,ToothChartWrapper.DirectXFormatWpfSoftware)),
				"TC-8's value means the same as the default.");
			Assert.IsFalse(ToothChartRelay.IsDrawnInSoftware(NewComputerPref(DrawingMode.DirectX,ToothChartWrapper.DirectXFormatWpfLiveView)));
			Assert.IsFalse(ToothChartRelay.IsDrawnInSoftware(NewComputerPref(DrawingMode.OpenGL," WpfLiveView ")));
			Assert.IsTrue(ToothChartRelay.IsDrawnInSoftware(NewComputerPref(DrawingMode.DirectX,"wpfliveview")),"The stored value is exact.");
			Assert.AreEqual("WpfLiveView",ToothChartWrapper.DirectXFormatWpfLiveView);
			Assert.IsFalse(ToothChartWrapper.DirectXFormatWpfLiveView.Contains(";"),"No ';', so a DirectX build reads no device format from it.");
		}

		[TestMethod]
		public void OnScreenDefault_IsTheSmoothedPicture_ForAComputerWithNothingStored() {
			//ENG-3: a new computer's row (GraphicsSimple 0, GraphicsUseDirectX11 0, DirectXFormat empty) gets the WPF chart drawn in software, the
			//renderer's smoothed picture, on screen.
			StaRunner.Run(() => {
				using(ToothChartWrapper wrapper=WpfCharts.CreateWrapper(ChartScenes.SizeModule.Size)) {
					ToothChartRelay.ApplyComputerPref(wrapper,new ComputerPref { ComputerName="SYNTHETIC-PC" });
					Assert.AreEqual(ToothChartEngine.Wpf3D,wrapper.Engine);
					Assert.IsTrue(wrapper.WpfDrawInSoftware,"The smoothed picture is the on-screen default.");
				}
			});
		}

		[TestMethod]
		public void ApplyComputerPref_StartsTheEngineTheComputerDrawsWith_WithTheSwitchFirst() {
			//No database in this process, or the new database's global "Use if available": the computer's own setting decides.
			StaRunner.Run(() => {
				foreach(YN chart3DThisComputer in new[] { YN.Unknown,YN.Yes,YN.No }) {
					foreach(DrawingMode drawingMode in new[] { DrawingMode.DirectX,DrawingMode.Simple2D,DrawingMode.OpenGL }) {
						foreach(string directXFormat in new[] { "","0;Hardware;64;D16;R5G6B5;FourSamples",ToothChartWrapper.DirectXFormatWpfSoftware,
							ToothChartWrapper.DirectXFormatWpfLiveView })
						{
							using(ToothChartWrapper wrapper=WpfCharts.CreateWrapper(ChartScenes.SizeModule.Size)) {
								wrapper.Engine=ToothChartEngine.Simple2D;
								int countBefore=ToothChartWrapper.GetCountFallbackMessages();
								ComputerPref computerPref=NewComputerPref(drawingMode,directXFormat,chart3DThisComputer);
								ComputerPref computerPrefBefore=computerPref.Copy();
								ToothChartRelay.ApplyComputerPref(wrapper,computerPref);
								string what=chart3DThisComputer+", "+drawingMode+" with \""+directXFormat+"\"";
								bool is3D=chart3DThisComputer!=YN.No;
								Assert.AreEqual(is3D ? ToothChartEngine.Wpf3D : ToothChartEngine.Simple2D,wrapper.Engine,what);
								Assert.AreEqual(directXFormat!=ToothChartWrapper.DirectXFormatWpfLiveView,wrapper.WpfDrawInSoftware,what);
								Assert.AreEqual(is3D ? DrawingMode.DirectX : DrawingMode.Simple2D,wrapper.DrawMode,
									what+": Wpf3D reads as DirectX, so a caller that saves DrawMode keeps the 3D chart.");
								Assert.AreEqual(0,ToothChartWrapper.GetFallbackMessagesSince(countBefore).Count,what+": nothing fell back.");
								AssertFieldsEqual(computerPrefBefore,computerPref,what+": the settings are read, never changed.");
							}
						}
					}
				}
			});
			//The order in the source: the switch before the engine (TC-3's fix round 3), so a computer drawn in software never makes a live view first.
			string method=SourceText.GetMethod(SourceText.ReadCode(@"WpfControlsOD\UI\ToothChart\ToothChartRelay.cs"),
				"public static void ApplyComputerPref(ToothChartWrapper toothChartWrapper,ComputerPref computerPref) {");
			SourceText.AssertInOrder(method,"ApplyComputerPref","toothChartWrapper.WpfDrawInSoftware=IsDrawnInSoftware(computerPref);",
				"toothChartWrapper.Engine=GetEngineForComputer(computerPref);");
			StringAssert.DoesNotMatch(method,new System.Text.RegularExpressions.Regex(@"ComputerPrefs\.Update|DeviceFormat|UseHardware|PreferredPixelFormat"));
			string settings=SourceText.GetMethod(SourceText.ReadCode(@"WpfControlsOD\UI\ToothChart\ToothChartRelay.cs"),
				"public static void ApplyComputerSettings(ToothChartWrapper toothChartWrapper) {");//TC-9a: the wrapper only (ToothChartSettingsUse went)
			StringAssert.Contains(settings,"ApplyComputerPref(toothChartWrapper,ComputerPrefs.LocalComputer);");
			StringAssert.DoesNotMatch(settings,new System.Text.RegularExpressions.Regex(@"ComputerPrefs\.Update"),"No ComputerPrefs.Update when an image is made.");
		}

		[TestMethod]
		public void ShownWrapper_OfAComputerDrawnInSoftware_NeverMakesALiveView() {
			WpfLiveViewTests.RequireDesktop();
			StaRunner.Run(() => {
				int countViewsMade=0;
				ToothChartWpfView.ActionStepForTests=(toothChartWpfView,step) => {
					if(step==ToothChartWpfView.StepCreate) {
						countViewsMade++;
					}
				};
				try {
					using Form form=WpfCharts.CreateOffscreenForm(ChartScenes.SizeModule.Size);
					ToothChartWrapper wrapper=WpfCharts.CreateWrapper(ChartScenes.SizeModule.Size);
					form.Controls.Add(wrapper);
					form.Show();
					Application.DoEvents();
					//A shown wrapper that runs the WPF chart's quick live view (the per-computer option since TC-8b), then gets a computer with the default,
					//the smoothed picture drawn in software.
					ToothChartRelay.ApplyComputerPref(wrapper,NewComputerPref(DrawingMode.DirectX,ToothChartWrapper.DirectXFormatWpfLiveView));
					WpfLiveViewTests.WaitForLiveView(wrapper);
					int countViewsBefore=countViewsMade;
					ToothChartRelay.ApplyComputerPref(wrapper,NewComputerPref(DrawingMode.OpenGL,""));
					ToothChartRelay relay=WpfCharts.CreateRelay(wrapper);
					ChartScenes.Prepare(relay,ChartScenes.Get("restored-adult"),ChartColorSet.Module);
					ToothChartImageHost toothChartImageHost=wrapper.ImageHostWpf;
					Assert.IsTrue(WpfRendererTests.PumpUntil(() => toothChartImageHost.CountImagesReceived>=1 && !toothChartImageHost.IsRenderInFlight,20000),
						"The image host shows the scene.");
					Assert.AreEqual(countViewsBefore,countViewsMade,"The engine restarted drawn in software: no live view was made first.");
					Assert.IsNull(wrapper.LiveViewWpf);
					Assert.IsTrue(toothChartImageHost.Visible);
					wrapper.Dispose();
					form.Close();
				}
				finally {
					ToothChartWpfView.ActionStepForTests=null;
				}
			});
		}

		[TestMethod]
		public void DrawModeRequests_OfDirectXAndOpenGL_StartWpf3D_AndSimple2DStays() {
			StaRunner.Run(() => {
				foreach(DrawingMode drawingMode in new[] { DrawingMode.DirectX,DrawingMode.OpenGL,DrawingMode.Simple2D }) {
					using(ToothChartWrapper wrapper=new ToothChartWrapper()) {
						wrapper.Size=ChartScenes.SizeModule.Size;
						int countBefore=ToothChartWrapper.GetCountFallbackMessages();
						wrapper.DrawMode=drawingMode;
						Assert.AreEqual(drawingMode==DrawingMode.Simple2D ? ToothChartEngine.Simple2D : ToothChartEngine.Wpf3D,wrapper.Engine,drawingMode.ToString());
						Assert.AreEqual(0,ToothChartWrapper.GetFallbackMessagesSince(countBefore).Count,drawingMode+": nothing fell back, and no DirectX was tried.");
						using(Bitmap bitmap=wrapper.GetBitmap()) {
							Assert.AreEqual(ChartScenes.SizeModule.Size,bitmap.Size);
						}
					}
				}
			});
		}
		[TestMethod]
		public void StoredChoice_WhoseWpfStartFails_FallsBackToSimple2D_WithOneLogLine() {
			//In a fresh process, because a start failure and a logged line are kept for the whole process (SecondProcess, "stored-start-fails").
			SecondProcessResult result=SecondProcess.Run(120000,"stored-start-fails");
			Console.WriteLine(result.Output);
			Assert.AreEqual(0,result.ExitCode,result.Output);
			StringAssert.Contains(result.Output,"engine stored 0: Simple2D");
			StringAssert.Contains(result.Output,"engine requested 2: Simple2D");
			StringAssert.Contains(result.Output,"bitmap: 410x307, blank False");
			StringAssert.Contains(result.Output,"fallback messages: 2","Each wrapper fell back once; no DirectX or OpenGL engine was tried.");
			StringAssert.Contains(result.Output,"log lines: 1","One line was written.");
			StringAssert.Contains(result.Output,"message: The Wpf3D tooth chart could not start (InvalidOperationException: ");
			StringAssert.Contains(result.Output,"). Using Simple2D.");
			Assert.IsFalse(result.Output.Contains("DirectX tooth chart"),"No DirectX fallback line.");
			Assert.IsFalse(result.Output.Contains("OpenGL tooth chart"),"No OpenGL fallback line.");
			//NEW-1 (TC-8b): File > Graphics' status on this computer says why, and reading it writes no log line.
			StringAssert.Contains(result.Output,"status: Fell back: The WPF tooth chart's renderer could not start: A forced start failure.");
			StringAssert.Contains(result.Output,"log lines after the status: 1");
		}
		#endregion The mapping (TC-8b, ENG-2)

		#region The status line (TC-8b, NEW-1)
		[TestMethod]
		public void Status_IsAvailable_WhenTheWpfChartStarts_AndFellBackWithTheReason_WhenItCannot() {
			Assert.AreEqual("Available",ToothChartRelay.Chart3DStatusAvailable);
			Assert.AreEqual("Fell back: ",ToothChartRelay.Chart3DStatusFellBackPrefix);
			Assert.AreEqual("Available",ToothChartRelay.GetChart3DStatus(),"The WPF chart starts in this process.");
			Assert.IsTrue(ToothChartWrapper.CanStartWpf3D(out string reason));
			Assert.AreEqual("",reason);
			//A start failure kept by the renderer (the test hook stands in for a computer where WPF cannot draw; the second-process test above checks the
			//real path), then cleared again: the status says why, and the chart starts again afterwards.
			int countBefore=ToothChartWrapper.GetCountFallbackMessages();
			SparksToothChart.Wpf3D.ToothChartRenderer.SetStartFailureForTests(new InvalidOperationException("A synthetic driver failure."));
			try {
				Assert.AreEqual("Fell back: The WPF tooth chart's renderer could not start: A synthetic driver failure.",ToothChartRelay.GetChart3DStatus());
				Assert.IsFalse(ToothChartWrapper.CanStartWpf3D(out reason));
				Assert.AreEqual("The WPF tooth chart's renderer could not start: A synthetic driver failure.",reason);
			}
			finally {
				SparksToothChart.Wpf3D.ToothChartRenderer.SetStartFailureForTests(null);
			}
			Assert.AreEqual(0,ToothChartWrapper.GetFallbackMessagesSince(countBefore).Count,"Reading the status logs nothing.");
			Assert.AreEqual("Available",ToothChartRelay.GetChart3DStatus(),"Cleared: the renderer starts again.");
		}
		#endregion The status line (TC-8b, NEW-1)

		#region The graphical perio gate (TC-8b, ENG-5)
		[TestMethod]
		public void PerioGate_OpensOnlyOnAComputerThatUsesThe3DChart_AndNamesFileGraphicsOtherwise() {
			//ENG-5 (revises O-31): Open Dental opens the graphical perio chart only on its 3D charts; a computer that draws Simple 2D is refused with
			//Open Dental's 24.3 sentence, the 3D tooth chart in DirectX's place and the menu named.
			Assert.AreEqual("In the Graphics setup window (File > Graphics), you must first select the 3D tooth chart.",ToothChartRelay.PerioNeeds3DChartMessage);
			foreach(bool isChart3DOnAllComputers in new[] { true,false }) {
				foreach(YN chart3DThisComputer in _arrayChart3DThisComputer) {
					foreach(DrawingMode drawingMode in new[] { DrawingMode.DirectX,DrawingMode.Simple2D,DrawingMode.OpenGL }) {
						ComputerPref computerPref=NewComputerPref(drawingMode,"",chart3DThisComputer);
						bool isOpen=ToothChartRelay.CanOpenGraphicalPerio(isChart3DOnAllComputers,computerPref,out string message);
						string what="global "+isChart3DOnAllComputers+", this computer "+chart3DThisComputer+", GraphicsSimple "+drawingMode;
						bool is3D=ToothChartRelay.GetEngineForComputer(isChart3DOnAllComputers,computerPref)==ToothChartEngine.Wpf3D;
						Assert.AreEqual(is3D,isOpen,what);
						Assert.AreEqual(is3D ? "" : ToothChartRelay.PerioNeeds3DChartMessage,message,what);
					}
				}
			}
			string relay=SourceText.ReadCode(@"WpfControlsOD\UI\ToothChart\ToothChartRelay.cs");
			StringAssert.Contains(SourceText.GetMethod(relay,"public static bool CanOpenGraphicalPerio(out string message) {"),
				"return CanOpenGraphicalPerio(IsChart3DOnAllComputers(),ComputerPrefs.LocalComputer,out message);");
		}
		#endregion The graphical perio gate (TC-8b, ENG-5)

		#region The save rule
		[TestMethod]
		public void SaveRule_WritesOnlyWhatAChoiceChanges_AndGraphicsSimpleStaysReadableByOpenDental() {
			//(global before, global after, this computer before, this computer after, GraphicsSimple before) -> GraphicsSimple after
			object[][] arrayCases=new object[][] {
				//The 3D chart before and after: GraphicsSimple is kept, whatever it is (Open Dental does not read it there).
				new object[] { true,true,YN.Unknown,YN.Unknown,DrawingMode.Simple2D,DrawingMode.Simple2D },
				new object[] { true,true,YN.Unknown,YN.Yes,DrawingMode.Simple2D,DrawingMode.Simple2D },
				new object[] { true,true,YN.Unknown,YN.Unknown,DrawingMode.OpenGL,DrawingMode.OpenGL },
				new object[] { true,false,YN.Yes,YN.Yes,DrawingMode.OpenGL,DrawingMode.OpenGL },
				//Simple 2D before and after: 1 over a stored 0 or 2, so Open Dental, whose older technology applies, draws the Simple 2D the window
				//shows checked, not its DirectX 9 or OpenGL chart (as Open Dental's own Save always writes GraphicsSimple from that group).
				new object[] { false,false,YN.No,YN.Unknown,DrawingMode.DirectX,DrawingMode.Simple2D },
				new object[] { false,false,YN.Unknown,YN.Unknown,DrawingMode.DirectX,DrawingMode.Simple2D },
				new object[] { true,true,YN.No,YN.No,DrawingMode.OpenGL,DrawingMode.Simple2D },
				new object[] { false,false,YN.No,YN.No,DrawingMode.Simple2D,DrawingMode.Simple2D },
				//To Simple 2D: 1, so Open Dental, whose older technology then applies, draws Simple 2D too.
				new object[] { true,true,YN.Unknown,YN.No,DrawingMode.DirectX,DrawingMode.Simple2D },
				new object[] { true,true,YN.Yes,YN.No,DrawingMode.OpenGL,DrawingMode.Simple2D },
				new object[] { true,false,YN.Unknown,YN.Unknown,DrawingMode.DirectX,DrawingMode.Simple2D },
				new object[] { true,true,YN.Unknown,YN.No,DrawingMode.Simple2D,DrawingMode.Simple2D },
				//To the 3D chart: 0 only over a stored 1; a stored 0 or 2 is kept (TC-8's rule).
				new object[] { true,true,YN.No,YN.Unknown,DrawingMode.Simple2D,DrawingMode.DirectX },
				new object[] { false,false,YN.No,YN.Yes,DrawingMode.Simple2D,DrawingMode.DirectX },
				new object[] { false,true,YN.Unknown,YN.Unknown,DrawingMode.Simple2D,DrawingMode.DirectX },
				new object[] { true,true,YN.No,YN.Yes,DrawingMode.OpenGL,DrawingMode.OpenGL },
				new object[] { false,true,YN.Unknown,YN.Unknown,DrawingMode.DirectX,DrawingMode.DirectX },
			};
			foreach(object[] arrayCase in arrayCases) {
				YN chart3DThisComputerBefore=(YN)arrayCase[2];
				YN chart3DThisComputerAfter=(YN)arrayCase[3];
				ComputerPref computerPref=NewComputerPref((DrawingMode)arrayCase[4],"",chart3DThisComputerBefore);
				ComputerPref computerPrefBefore=computerPref.Copy();
				ToothChartRelay.SetGraphicsChoice(computerPref,(bool)arrayCase[0],(bool)arrayCase[1],chart3DThisComputerAfter,true);
				string what="global "+arrayCase[0]+" -> "+arrayCase[1]+", this computer "+chart3DThisComputerBefore+" -> "+chart3DThisComputerAfter
					+", GraphicsSimple "+arrayCase[4];
				Assert.AreEqual((DrawingMode)arrayCase[5],computerPref.GraphicsSimple,what);
				Assert.AreEqual(chart3DThisComputerAfter,computerPref.GraphicsUseDirectX11,what);
				computerPrefBefore.GraphicsSimple=computerPref.GraphicsSimple;
				computerPrefBefore.GraphicsUseDirectX11=computerPref.GraphicsUseDirectX11;
				AssertFieldsEqual(computerPrefBefore,computerPref,what+": no other column is written.");
				//Open Dental reads the saved row as this program does: its DirectX 11 chart where this one draws the WPF chart, and its older
				//technology's Simple 2D where this one draws Simple 2D (every save that leaves the computer on Simple 2D stores 1, so no saved row
				//leaves Open Dental a stored 0 or 2, its DirectX 9 or OpenGL chart, which this program retired).
				bool isDrawn3D=ToothChartRelay.GetEngineForComputer((bool)arrayCase[1],computerPref)==ToothChartEngine.Wpf3D;
				bool isOpenDental3DChart=ToothChartRelay.IsChart3DUsed((bool)arrayCase[1],computerPref.GraphicsUseDirectX11);
				Assert.AreEqual(isDrawn3D,isOpenDental3DChart,what);
				if(!isDrawn3D) {
					Assert.AreEqual(DrawingMode.Simple2D,computerPref.GraphicsSimple,what+": on Simple 2D after the save, Open Dental draws Simple 2D too.");
				}
			}
		}

		[TestMethod]
		public void SaveRule_DrawInSoftware_WritesOrClearsOnlyTheLiveViewValue() {
			string formatDirectX="0;Hardware;64;D16;R5G6B5;FourSamples";
			//(stored DirectXFormat, "Draw in software") -> DirectXFormat after the save
			string[][] arrayCases=new string[][] {
				new[] { "","on","" },
				new[] { formatDirectX,"on",formatDirectX },
				new[] { ToothChartWrapper.DirectXFormatWpfSoftware,"on",ToothChartWrapper.DirectXFormatWpfSoftware },
				new[] { ToothChartWrapper.DirectXFormatWpfLiveView,"on","" },
				new[] { " WpfLiveView ","on","" },
				new[] { "","off",ToothChartWrapper.DirectXFormatWpfLiveView },
				new[] { formatDirectX,"off",ToothChartWrapper.DirectXFormatWpfLiveView },
				new[] { ToothChartWrapper.DirectXFormatWpfSoftware,"off",ToothChartWrapper.DirectXFormatWpfLiveView },
				new[] { ToothChartWrapper.DirectXFormatWpfLiveView,"off",ToothChartWrapper.DirectXFormatWpfLiveView },
			};
			foreach(YN chart3DThisComputer in new[] { YN.Unknown,YN.No }) {
				foreach(string[] arrayCase in arrayCases) {
					ComputerPref computerPref=NewComputerPref(DrawingMode.DirectX,arrayCase[0],chart3DThisComputer);
					ToothChartRelay.SetGraphicsChoice(computerPref,true,true,chart3DThisComputer,arrayCase[1]=="on");
					string what="\""+arrayCase[0]+"\", "+arrayCase[1]+", this computer "+chart3DThisComputer;
					Assert.AreEqual(arrayCase[2],computerPref.DirectXFormat,what);
					//What was saved reads back as the window's choice.
					Assert.AreEqual(arrayCase[1]=="on",ToothChartRelay.IsDrawnInSoftware(computerPref),what);
				}
			}
		}

		[TestMethod]
		public void SaveRule_SavedThenApplied_GivesTheWindowsChoice() {
			StaRunner.Run(() => {
				foreach(DrawingMode drawingModeStored in new[] { DrawingMode.DirectX,DrawingMode.Simple2D,DrawingMode.OpenGL }) {
					foreach(YN chart3DThisComputerStored in new[] { YN.Unknown,YN.Yes,YN.No }) {
						foreach(YN chart3DThisComputer in new[] { YN.Unknown,YN.Yes,YN.No }) {
							foreach(bool isDrawnInSoftware in new[] { true,false }) {
								ComputerPref computerPref=NewComputerPref(drawingModeStored,"",chart3DThisComputerStored);
								//The global setting of this process ("Use if available") before and after, as File > Graphics saves it.
								ToothChartRelay.SetGraphicsChoice(computerPref,true,true,chart3DThisComputer,isDrawnInSoftware);
								using(ToothChartWrapper wrapper=WpfCharts.CreateWrapper(ChartScenes.SizeModule.Size)) {
									ToothChartRelay.ApplyComputerPref(wrapper,computerPref);
									string what="stored "+drawingModeStored+" "+chart3DThisComputerStored+", chosen "+chart3DThisComputer+", software "+isDrawnInSoftware;
									Assert.AreEqual(chart3DThisComputer!=YN.No ? ToothChartEngine.Wpf3D : ToothChartEngine.Simple2D,wrapper.Engine,what);
									Assert.AreEqual(isDrawnInSoftware,wrapper.WpfDrawInSoftware,what);
								}
							}
						}
					}
				}
			});
		}

		///<summary>Every public field of the two ComputerPref objects is equal.</summary>
		private static void AssertFieldsEqual(ComputerPref computerPrefExpected,ComputerPref computerPrefActual,string what) {
			foreach(FieldInfo fieldInfo in typeof(ComputerPref).GetFields(BindingFlags.Public|BindingFlags.Instance)) {
				Assert.AreEqual(fieldInfo.GetValue(computerPrefExpected),fieldInfo.GetValue(computerPrefActual),what+" ("+fieldInfo.Name+")");
			}
		}
		#endregion The save rule

		#region The dashboard's MTA worker
		[TestMethod]
		public void MtaWorker_GetsTheWpfChartFromTheStoredMode_AndNothingIsLogged() {
			WpfSession.RequireRendering();//REL-0.1-FIX: inconclusive where WPF draws nothing (a disconnected session)
			//The Patient Dashboard makes its wrapper on an MTA ODThread worker (UserControlDashboard, DashToothChart, SheetPrinting.GetToothChartHelper).
			//Since TC-1 the wrapper makes no WPF object there; since TC-8 the stored 0 or 2 asks for the WPF chart there too.
			foreach(DrawingMode drawingMode in new[] { DrawingMode.DirectX,DrawingMode.OpenGL }) {
				Exception exception=null;
				string result="";
				Thread thread=new Thread(() => {
					try {
						int countBefore=ToothChartWrapper.GetCountFallbackMessages();
						using(Form form=new Form())
						using(ToothChartWrapper wrapper=new ToothChartWrapper()) {
							wrapper.Size=new Size(410,307);
							ToothChartRelay.ApplyComputerPref(wrapper,NewComputerPref(drawingMode,""));
							form.Controls.Add(wrapper);
							ToothChartRelay relay=WpfCharts.CreateRelay(wrapper);
							ChartScenes.Prepare(relay,ChartScenes.Get("restored-adult"),ChartColorSet.Module);
							using(Bitmap bitmap=relay.GetBitmap()) {
								result=wrapper.Engine+" "+bitmap.Width+"x"+bitmap.Height+" blank "+ChartImageCompare.DescribeBitmap(bitmap).IsBlank
									+" logged "+ToothChartWrapper.GetFallbackMessagesSince(countBefore).Count;
							}
						}
					}
					catch(Exception ex) {
						exception=ex;
					}
				});
				thread.SetApartmentState(ApartmentState.MTA);
				thread.Start();
				Assert.IsTrue(thread.Join(120000),"The MTA worker finished.");
				Assert.IsNull(exception,exception?.ToString());
				Assert.AreEqual("Wpf3D 410x307 blank False logged 0",result,drawingMode.ToString());
			}
		}
		#endregion The dashboard's MTA worker

		#region The live view's render-thread failure on the UI thread
		[TestMethod]
		public void RenderThreadFailure_IsRecognisedByItsHResultOrItsThrower() {
			Assert.AreEqual(unchecked((int)0x88980406),ToothChartWrapper.HRESULT_UCEERR_RENDERTHREADFAILURE);
			Assert.IsTrue(ToothChartWrapper.IsWpfRenderThreadFailure(new COMException("UCEERR_RENDERTHREADFAILURE",ToothChartWrapper.HRESULT_UCEERR_RENDERTHREADFAILURE)));
			Assert.IsTrue(ToothChartWrapper.IsWpfRenderThreadFailure(new InvalidOperationException("Wrapped.",
				new COMException("UCEERR_RENDERTHREADFAILURE",ToothChartWrapper.HRESULT_UCEERR_RENDERTHREADFAILURE))),"Inside another exception.");
			Assert.IsFalse(ToothChartWrapper.IsWpfRenderThreadFailure(null));
			Assert.IsFalse(ToothChartWrapper.IsWpfRenderThreadFailure(new InvalidOperationException("Something else.")));
			Assert.IsFalse(ToothChartWrapper.IsWpfRenderThreadFailure(new COMException("Another HRESULT.",unchecked((int)0x88980403))));
			//The exception MediaContext.NotifyPartitionIsZombie throws (InvalidOperationException, "An unspecified error occurred on the render thread")
			//carries no HRESULT of its own; its thrower names it. WPF's own method is internal, so it is called by reflection with the failure code WPF
			//passes it, which throws without touching a render thread.
			MethodInfo methodInfoZombie=typeof(System.Windows.Media.Visual).Assembly.GetType("System.Windows.Media.MediaContext")
				?.GetMethod("NotifyPartitionIsZombie",BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static);
			if(methodInfoZombie==null) {
				Assert.Inconclusive("MediaContext.NotifyPartitionIsZombie is not in this WPF; the HRESULT checks above ran.");
			}
			Exception exceptionZombie=null;
			string whyNot="";
			StaRunner.Run(() => {
				object mediaContext=null;
				try {
					if(!methodInfoZombie.IsStatic) {
						mediaContext=typeof(System.Windows.Media.Visual).Assembly.GetType("System.Windows.Media.MediaContext")
							.GetMethod("From",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,new object[] { System.Windows.Threading.Dispatcher.CurrentDispatcher });
					}
				}
				catch(Exception ex) {
					whyNot="MediaContext.From failed: "+ex.GetBaseException().Message;
					return;
				}
				try {
					methodInfoZombie.Invoke(mediaContext,new object[] { unchecked((int)0x88980406) });
				}
				catch(TargetInvocationException ex) {
					exceptionZombie=ex.InnerException;
				}
			});
			if(whyNot!="") {
				Assert.Inconclusive(whyNot+"; the HRESULT checks above ran.");
			}
			Assert.IsNotNull(exceptionZombie,"NotifyPartitionIsZombie threw.");
			Assert.IsTrue(ToothChartWrapper.IsWpfRenderThreadFailure(exceptionZombie),exceptionZombie.GetType().Name+": "+exceptionZombie.Message);
		}

		[TestMethod]
		public void LiveView_WhoseRenderThreadFailsOnTheUiThread_FallsBackToTheImageHost() {
			WpfLiveViewTests.RequireDesktop();
			StaRunner.Run(() => {
				using Form form=WpfCharts.CreateOffscreenForm(ChartScenes.SizeModule.Size);
				ToothChartWrapper wrapper=WpfCharts.CreateWrapper(ChartScenes.SizeModule.Size);
				form.Controls.Add(wrapper);
				ToothChartRelay relay=WpfCharts.CreateRelay(wrapper);
				ChartScenes.Prepare(relay,ChartScenes.Get("restored-adult"),ChartColorSet.Module);
				form.Show();
				ToothChartWpfView toothChartWpfView=WpfLiveViewTests.WaitForLiveView(wrapper);
				ToothChartImageHost toothChartImageHost=wrapper.ImageHostWpf;
				int countImages=toothChartImageHost.CountImagesReceived;
				int countBefore=ToothChartWrapper.GetCountFallbackMessages();
				//Another exception on the same Dispatcher is not the chart's: it is left unhandled for the program's handlers (here the test's, added after
				//the wrapper's, which sees it unhandled and handles it so the test goes on).
				List<string> listSeenByLaterHandler=new List<string>();
				System.Windows.Threading.DispatcherUnhandledExceptionEventHandler handlerLater=(sender,e) => {
					listSeenByLaterHandler.Add(e.Exception.Message+" handled before: "+e.Handled);
					e.Handled=true;
				};
				System.Windows.Threading.Dispatcher dispatcher=System.Windows.Threading.Dispatcher.CurrentDispatcher;
				dispatcher.UnhandledException+=handlerLater;
				try {
					dispatcher.BeginInvoke(new Action(() => throw new InvalidOperationException("Not a render failure.")));
					Assert.IsTrue(WpfRendererTests.PumpUntil(() => listSeenByLaterHandler.Count==1,20000));
					Assert.AreEqual("Not a render failure. handled before: False",listSeenByLaterHandler[0]);
					Assert.AreSame(toothChartWpfView,wrapper.LiveViewWpf,"The live view stays.");
					//The failure WPF throws on the UI thread when its render thread fails.
					dispatcher.BeginInvoke(new Action(() => throw new COMException("UCEERR_RENDERTHREADFAILURE",ToothChartWrapper.HRESULT_UCEERR_RENDERTHREADFAILURE)));
					Assert.IsTrue(WpfRendererTests.PumpUntil(() => wrapper.LiveViewWpf==null,20000),"The live view is released.");
					Assert.IsTrue(WpfRendererTests.PumpUntil(() => toothChartImageHost.CountImagesReceived>countImages && !toothChartImageHost.IsRenderInFlight,20000),
						"The image host shows the current data.");
					Assert.AreEqual(2,listSeenByLaterHandler.Count);
					StringAssert.EndsWith(listSeenByLaterHandler[1],"handled before: True","The chart handled the render failure.");
				}
				finally {
					dispatcher.UnhandledException-=handlerLater;
				}
				Assert.IsTrue(toothChartWpfView.IsDisposed,"The failed view is released.");
				Assert.IsTrue(toothChartImageHost.Visible);
				Assert.AreSame(toothChartImageHost,wrapper.Controls.Cast<Control>().Single());
				List<string> listMessages=ToothChartWrapper.GetFallbackMessagesSince(countBefore);
				Assert.AreEqual(1,listMessages.Count,string.Join("; ",listMessages));
				StringAssert.Contains(listMessages[0],"live view could not draw on screen (COMException: UCEERR_RENDERTHREADFAILURE). Showing its software render on screen.");
				using(Bitmap bitmap=relay.GetBitmap())
				using(Bitmap bitmapShown=toothChartImageHost.GetBitmapShown()) {
					Assert.IsTrue(ChartImageCompare.GetPixelsArgb(bitmap).SequenceEqual(ChartImageCompare.GetPixelsArgb(bitmapShown)),"The image host shows the scene.");
				}
				//The wrapper stays on the image host: no live view is made again until the engine starts again.
				int countImagesAfter=toothChartImageHost.CountImagesReceived;
				relay.SetSelected("8",true);
				Assert.IsTrue(WpfRendererTests.PumpUntil(() => toothChartImageHost.CountImagesReceived>countImagesAfter && !toothChartImageHost.IsRenderInFlight,20000));
				Assert.IsNull(wrapper.LiveViewWpf,"No new live view.");
				wrapper.Dispose();
				form.Close();
			});
		}
		#endregion The live view's render-thread failure on the UI thread
	}
}
