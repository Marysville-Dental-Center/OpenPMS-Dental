// File > Graphics, rebuilt in TC-8 and given Open Dental's layout in TC-8b (the plan's parity rows ENG-1, ENG-2, ENG-3, ENG-5 and NEW-1), through
// the program's own window: it shows the global "3D Tooth Chart on all computers" setting, this computer's "Use the global setting / Use if
// available / Do not use", the status on this computer, the older technology (Simple 2D) and "Draw in software" as stored; its preview starts what
// the choices give; and Save writes only what a choice changes to this computer's computerpref row and the preference in the scratch database. The
// window is made and its Load and Save handlers are called, but it is not shown; for the gallery (OPMS_CHART_GALLERY) one test draws it off the
// desktop into pictures, one per state.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using DataConnectionBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental;
using OpenPmsDentalBusiness;
using SparksToothChart;
using ToothChartImageTests;

namespace ToothChartHostTests {
	[TestClass]
	public class GraphicsWindowTests {
		private const string FORMAT_DIRECTX="0;Hardware;64;D16;R5G6B5;FourSamples";
		private const string STATUS_3D_SOFTWARE="3D chart, drawn in software: the smoothed picture, the same as printouts and sheets.";
		private const string STATUS_3D_LIVE="3D chart, quick live view.";
		private const string STATUS_2D="Simple 2D chart.";
		private ComputerPref _computerPrefBefore;

		[TestInitialize]
		public void TestInitialize() {
			HostDb.EnsureOrSkip();
			HostDb.SetCulture();
			_computerPrefBefore=ComputerPrefs.LocalComputer.Copy();
			ToothChartRelay.SetChart3DOnAllComputers(true);
		}

		[TestCleanup]
		public void TestCleanup() {
			SparksToothChart.Wpf3D.ToothChartRenderer.SetStartFailureForTests(null);
			if(_computerPrefBefore==null || ComputerPrefs.IsLocalComputerNull()) {
				return;
			}
			ComputerPref computerPrefOld=ComputerPrefs.LocalComputer.Copy();
			ComputerPrefs.LocalComputer.GraphicsSimple=_computerPrefBefore.GraphicsSimple;
			ComputerPrefs.LocalComputer.DirectXFormat=_computerPrefBefore.DirectXFormat;
			ComputerPrefs.LocalComputer.GraphicsUseDirectX11=_computerPrefBefore.GraphicsUseDirectX11;
			ComputerPrefs.Update(ComputerPrefs.LocalComputer,computerPrefOld);
			ToothChartRelay.SetChart3DOnAllComputers(true);//the new database's value
			Prefs.UpdateBool(PrefName.PracticeIsMedicalOnly,false);
		}

		///<summary>Stores the local computer's settings in the database, as the program last saved them, and the office's global 3D setting.</summary>
		private static void StoreLocalComputer(bool isChart3DOnAllComputers,YN chart3DThisComputer,DrawingMode drawingMode,string directXFormat) {
			ToothChartRelay.SetChart3DOnAllComputers(isChart3DOnAllComputers);
			ComputerPref computerPrefOld=ComputerPrefs.LocalComputer.Copy();
			ComputerPrefs.LocalComputer.GraphicsUseDirectX11=chart3DThisComputer;
			ComputerPrefs.LocalComputer.GraphicsSimple=drawingMode;
			ComputerPrefs.LocalComputer.DirectXFormat=directXFormat;
			ComputerPrefs.Update(ComputerPrefs.LocalComputer,computerPrefOld);
		}

		private static T GetField<T>(FormGraphics formGraphics,string name) {
			return (T)typeof(FormGraphics).GetField(name,BindingFlags.NonPublic|BindingFlags.Instance).GetValue(formGraphics);
		}

		private static void Invoke(FormGraphics formGraphics,string name) {
			typeof(FormGraphics).GetMethod(name,BindingFlags.NonPublic|BindingFlags.Instance).Invoke(formGraphics,new object[] { formGraphics,EventArgs.Empty });
		}

		///<summary>The stored row of this computer and the global setting, read from the database: "GraphicsSimple GraphicsUseDirectX11 DirectXFormat"
		///and, after a '|', the preference's value.</summary>
		private static string ReadStoredRow() {
			return DataCore.GetScalar("SELECT CONCAT(GraphicsSimple,' ',GraphicsUseDirectX11,' ',DirectXFormat) FROM computerpref WHERE ComputerPrefNum="
				+POut.Long(ComputerPrefs.LocalComputer.ComputerPrefNum))
				+"|"+DataCore.GetScalar("SELECT ValueString FROM preference WHERE PrefName='"+POut.String(nameof(PrefName.DirectX11ToothChartUseIfAvail))+"'");
		}

		[TestMethod]
		public void Load_ShowsTheStoredChoices_TheStatus_AndThePreviewStartsThem() {
			//(global, this computer, GraphicsSimple, DirectXFormat) -> 3D chart used, "Draw in software", the preview's status line
			object[][] arrayCases=new object[][] {
				new object[] { true,YN.Unknown,DrawingMode.DirectX,"",true,true,STATUS_3D_SOFTWARE },
				new object[] { true,YN.Unknown,DrawingMode.Simple2D,"",true,true,STATUS_3D_SOFTWARE },//ENG-2: a stored 1 gets the 3D chart
				new object[] { true,YN.Yes,DrawingMode.OpenGL,ToothChartWrapper.DirectXFormatWpfLiveView,true,false,STATUS_3D_LIVE },
				new object[] { true,YN.Unknown,DrawingMode.DirectX,ToothChartWrapper.DirectXFormatWpfSoftware,true,true,STATUS_3D_SOFTWARE },
				new object[] { true,YN.No,DrawingMode.DirectX,"",false,true,STATUS_2D },
				new object[] { false,YN.Unknown,DrawingMode.DirectX,"",false,true,STATUS_2D },
				new object[] { false,YN.Yes,DrawingMode.Simple2D,FORMAT_DIRECTX,true,true,STATUS_3D_SOFTWARE },
				new object[] { false,YN.No,DrawingMode.Simple2D,ToothChartWrapper.DirectXFormatWpfLiveView,false,false,STATUS_2D },
			};
			foreach(object[] arrayCase in arrayCases) {
				bool isChart3DOnAllComputers=(bool)arrayCase[0];
				YN chart3DThisComputer=(YN)arrayCase[1];
				StoreLocalComputer(isChart3DOnAllComputers,chart3DThisComputer,(DrawingMode)arrayCase[2],(string)arrayCase[3]);
				string rowBefore=ReadStoredRow();
				string what="global "+isChart3DOnAllComputers+", this computer "+chart3DThisComputer+", stored "+arrayCase[2]+" \""+arrayCase[3]+"\"";
				bool is3D=(bool)arrayCase[4];
				StaRunner.Run(() => {
					using(FormGraphics formGraphics=new FormGraphics()) {
						Invoke(formGraphics,"FormGraphics_Load");
						Assert.AreEqual(isChart3DOnAllComputers,GetField<RadioButton>(formGraphics,"radioChart3DAllUse").Checked,what+": global Use if available");
						Assert.AreEqual(!isChart3DOnAllComputers,GetField<RadioButton>(formGraphics,"radioChart3DAllDontUse").Checked,what+": global Do not use");
						Assert.AreEqual(chart3DThisComputer==YN.Unknown,GetField<RadioButton>(formGraphics,"radioChart3DThisGlobal").Checked,what+": Use the global setting");
						Assert.AreEqual(chart3DThisComputer==YN.Yes,GetField<RadioButton>(formGraphics,"radioChart3DThisUse").Checked,what+": Use if available");
						Assert.AreEqual(chart3DThisComputer==YN.No,GetField<RadioButton>(formGraphics,"radioChart3DThisDontUse").Checked,what+": Do not use");
						Assert.IsTrue(GetField<RadioButton>(formGraphics,"radioSimpleChart").Checked,what+": Simple 2D, the one older technology left");
						Assert.IsTrue(GetField<RadioButton>(formGraphics,"radioStatusAvailable").Checked,what+": the status on this computer");
						Assert.IsFalse(GetField<RadioButton>(formGraphics,"radioStatusFellBack").Checked,what+": the status on this computer");
						OpenPmsDental.UI.CheckBox checkDrawInSoftware=GetField<OpenPmsDental.UI.CheckBox>(formGraphics,"checkDrawInSoftware");
						Assert.AreEqual((bool)arrayCase[5],checkDrawInSoftware.Checked,what+": Draw in software");
						Assert.AreEqual(is3D,checkDrawInSoftware.Enabled,what+": the check box is the 3D chart's only");
						ToothChartWrapper wrapper=GetField<ToothChartWrapper>(formGraphics,"toothChartWrapper");
						Assert.AreEqual(is3D ? ToothChartEngine.Wpf3D : ToothChartEngine.Simple2D,wrapper.Engine,what+": the preview's engine");
						Assert.AreEqual((string)arrayCase[6],GetField<Label>(formGraphics,"labelPreviewStatus").Text,what+": the preview's status line");
						using(Bitmap bitmap=wrapper.GetBitmap()) {
							Assert.AreEqual(new Size(410,307),bitmap.Size);
							Assert.IsFalse(ChartImageCompare.DescribeBitmap(bitmap).IsBlank,what+": the preview's sample is drawn");
						}
					}
				});
				Assert.AreEqual(rowBefore,ReadStoredRow(),what+": Load changes nothing.");
			}
		}

		[TestMethod]
		public void Load_OnAComputerWhose3DChartFallsBack_ShowsFellBackWithTheReason() {
			//NEW-1: the renderer's kept start failure (the test hook stands in for a computer where WPF cannot draw) gives "Fell back: " and the reason in
			//the status group, and the preview falls back to Simple 2D, as every chart on that computer does.
			StoreLocalComputer(true,YN.Unknown,DrawingMode.DirectX,"");
			SparksToothChart.Wpf3D.ToothChartRenderer.SetStartFailureForTests(new InvalidOperationException("A synthetic driver failure."));
			try {
				StaRunner.Run(() => {
					using(FormGraphics formGraphics=new FormGraphics()) {
						Invoke(formGraphics,"FormGraphics_Load");
						Assert.IsFalse(GetField<RadioButton>(formGraphics,"radioStatusAvailable").Checked);
						RadioButton radioStatusFellBack=GetField<RadioButton>(formGraphics,"radioStatusFellBack");
						Assert.IsTrue(radioStatusFellBack.Checked);
						Assert.AreEqual("Fell back: The WPF tooth chart's renderer could not start: A synthetic driver failure.",radioStatusFellBack.Text);
						Assert.AreEqual(ToothChartEngine.Simple2D,GetField<ToothChartWrapper>(formGraphics,"toothChartWrapper").Engine);
						Assert.AreEqual("The 3D chart could not start on this computer, so the Simple 2D chart is shown (the ToothChart log says why).",
							GetField<Label>(formGraphics,"labelPreviewStatus").Text);
					}
				});
			}
			finally {
				SparksToothChart.Wpf3D.ToothChartRenderer.SetStartFailureForTests(null);
			}
		}

		[TestMethod]
		public void Load_InAMedicalPractice_UsesOpenDentalsMedicalWording() {
			//Open Dental's own Load words its older choice and its options so in a medical practice or clinic; the 3D groups keep their words.
			StoreLocalComputer(true,YN.Unknown,DrawingMode.DirectX,"");
			Prefs.UpdateBool(PrefName.PracticeIsMedicalOnly,true);
			StaRunner.Run(() => {
				using(FormGraphics formGraphics=new FormGraphics()) {
					Invoke(formGraphics,"FormGraphics_Load");
					Assert.AreEqual("Use Simple Graphics",GetField<RadioButton>(formGraphics,"radioSimpleChart").Text);
					Assert.AreEqual("Options For 3D Graphics",GetField<OpenPmsDental.UI.GroupBox>(formGraphics,"groupOptions").Text);
					Assert.AreEqual("Most users will never need to change any of these options.  These are only used when the 3D graphics are not working properly.",
						GetField<Label>(formGraphics,"labelOptions").Text);
					Assert.AreEqual("3D Tooth Chart on all computers",GetField<OpenPmsDental.UI.GroupBox>(formGraphics,"groupChart3DAll").Text);
				}
			});
		}

		[TestMethod]
		public void Save_WritesOnlyWhatAChoiceChanges_ToThisComputersRowAndThePreference() {
			//(stored GraphicsSimple, stored this computer, stored DirectXFormat, global before, chosen global, chosen this computer, "Draw in software")
			// -> the stored row after Save ("GraphicsSimple GraphicsUseDirectX11 DirectXFormat|global")
			object[][] arrayCases=new object[][] {
				new object[] { DrawingMode.Simple2D,YN.Unknown,"",true,true,YN.Unknown,true,"1 0 |1" },//nothing changed: nothing written (ENG-2: 3D both)
				new object[] { DrawingMode.DirectX,YN.Unknown,"",true,true,YN.No,true,"1 2 |1" },//to Simple 2D: 1
				new object[] { DrawingMode.Simple2D,YN.No,"",true,true,YN.Unknown,true,"0 0 |1" },//to the 3D chart over a stored 1: 0
				new object[] { DrawingMode.OpenGL,YN.No,"",true,true,YN.Yes,true,"2 1 |1" },//to the 3D chart: a stored 2 is kept
				new object[] { DrawingMode.DirectX,YN.Unknown,"",true,false,YN.Unknown,true,"1 0 |0" },//the global Do not use: this computer to Simple 2D
				new object[] { DrawingMode.Simple2D,YN.Unknown,"",false,true,YN.Unknown,true,"0 0 |1" },//the global Use if available: to the 3D chart
				new object[] { DrawingMode.DirectX,YN.Yes,"",true,false,YN.Yes,true,"0 1 |0" },//the global changes, this computer's chart does not
				new object[] { DrawingMode.DirectX,YN.Unknown,"",true,true,YN.Unknown,false,"0 0 WpfLiveView|1" },//the quick live view
				new object[] { DrawingMode.DirectX,YN.Unknown,ToothChartWrapper.DirectXFormatWpfLiveView,true,true,YN.Unknown,true,"0 0 |1" },//the default again
				new object[] { DrawingMode.DirectX,YN.Unknown,FORMAT_DIRECTX,true,true,YN.Unknown,true,"0 0 "+FORMAT_DIRECTX+"|1" },//kept for a return
				new object[] { DrawingMode.DirectX,YN.Unknown,ToothChartWrapper.DirectXFormatWpfSoftware,true,true,YN.Unknown,true,"0 0 WpfSoftware|1" },
			};
			foreach(object[] arrayCase in arrayCases) {
				StoreLocalComputer((bool)arrayCase[3],(YN)arrayCase[1],(DrawingMode)arrayCase[0],(string)arrayCase[2]);
				bool isChart3DOnAllComputers=(bool)arrayCase[4];
				YN chart3DThisComputer=(YN)arrayCase[5];
				bool isDrawnInSoftware=(bool)arrayCase[6];
				string what="stored "+arrayCase[0]+" "+arrayCase[1]+" \""+arrayCase[2]+"\", global "+arrayCase[3]+" -> "+isChart3DOnAllComputers
					+", this computer "+chart3DThisComputer+", Draw in software "+isDrawnInSoftware;
				long countLogsBefore=PIn.Long(DataCore.GetScalar("SELECT COUNT(*) FROM securitylog WHERE LogText LIKE '3D Tooth Chart on all computers set to%'"));
				DialogResult dialogResult=DialogResult.None;
				StaRunner.Run(() => {
					using(FormGraphics formGraphics=new FormGraphics()) {
						Invoke(formGraphics,"FormGraphics_Load");
						RadioButton radioGlobal=GetField<RadioButton>(formGraphics,isChart3DOnAllComputers ? "radioChart3DAllUse" : "radioChart3DAllDontUse");
						radioGlobal.Checked=true;
						Invoke(formGraphics,"radioChart3D_Click");
						string nameThis=chart3DThisComputer==YN.Yes ? "radioChart3DThisUse" : (chart3DThisComputer==YN.No ? "radioChart3DThisDontUse" : "radioChart3DThisGlobal");
						GetField<RadioButton>(formGraphics,nameThis).Checked=true;
						Invoke(formGraphics,"radioChart3D_Click");
						GetField<OpenPmsDental.UI.CheckBox>(formGraphics,"checkDrawInSoftware").Checked=isDrawnInSoftware;
						Invoke(formGraphics,"checkDrawInSoftware_Click");
						Invoke(formGraphics,"butSave_Click");
						dialogResult=formGraphics.DialogResult;
					}
				});
				Assert.AreEqual(DialogResult.OK,dialogResult,what);
				Assert.AreEqual((string)arrayCase[7],ReadStoredRow(),what);
				long countLogsAfter=PIn.Long(DataCore.GetScalar("SELECT COUNT(*) FROM securitylog WHERE LogText LIKE '3D Tooth Chart on all computers set to%'"));
				Assert.AreEqual((bool)arrayCase[3]!=isChart3DOnAllComputers ? 1 : 0,countLogsAfter-countLogsBefore,what+": a security log entry for a global change");
				//What was saved starts the chart the window showed (the Chart module's InitializeLocalData reads it through the relay).
				bool is3D=ToothChartRelay.IsChart3DUsed(isChart3DOnAllComputers,chart3DThisComputer);
				StaRunner.Run(() => {
					using(ToothChartWrapper wrapper=new ToothChartWrapper()) {
						wrapper.Size=new Size(410,307);
						ToothChartRelay.ApplyComputerSettings(wrapper);
						Assert.AreEqual(is3D ? ToothChartEngine.Wpf3D : ToothChartEngine.Simple2D,wrapper.Engine,what);
						Assert.AreEqual(isDrawnInSoftware,wrapper.WpfDrawInSoftware,what);
					}
				});
			}
		}

		///<summary>One state of the window for the gallery: what is stored before it opens, and the file name.</summary>
		private class GalleryState {
			public string Name;
			public bool IsChart3DOnAllComputers=true;
			public YN Chart3DThisComputer=YN.Unknown;
			public string DirectXFormat="";
			public bool IsFellBack;
			public bool IsMedical;
		}

		[TestMethod]
		public void Gallery_DrawsTheWindowOffTheDesktop_InEachState() {
			//The gallery of TC-8b: File > Graphics drawn into a picture per state (gallery folder, the name prefix OPMS_CHART_GALLERY_GRAPHICS_NAME or
			//"graphics-tc8b"), the window placed off the desktop as the image tests place their forms. Its 3D preview is a WPF live view or the image host,
			//which a window's picture (DrawToBitmap) leaves out, so the preview's own picture is drawn over its place.
			string folder=HostPictures.GalleryFolder;
			if(folder==null) {
				Assert.Inconclusive(HostPictures.EnvGallery+" is not set; this draws the window for the gallery only.");
			}
			string name=Environment.GetEnvironmentVariable("OPMS_CHART_GALLERY_GRAPHICS_NAME");
			if(string.IsNullOrWhiteSpace(name)) {
				name="graphics-tc8b";
			}
			List<GalleryState> listStates=new List<GalleryState> {
				new GalleryState { Name="default" },
				new GalleryState { Name="global-do-not-use",IsChart3DOnAllComputers=false },
				new GalleryState { Name="this-use-if-available",IsChart3DOnAllComputers=false,Chart3DThisComputer=YN.Yes },
				new GalleryState { Name="this-do-not-use",Chart3DThisComputer=YN.No },
				new GalleryState { Name="quick-live-view",DirectXFormat=ToothChartWrapper.DirectXFormatWpfLiveView },
				new GalleryState { Name="fell-back",IsFellBack=true },
				new GalleryState { Name="medical",IsMedical=true },
			};
			foreach(GalleryState galleryState in listStates) {
				StoreLocalComputer(galleryState.IsChart3DOnAllComputers,galleryState.Chart3DThisComputer,DrawingMode.DirectX,galleryState.DirectXFormat);
				Prefs.UpdateBool(PrefName.PracticeIsMedicalOnly,galleryState.IsMedical);
				SparksToothChart.Wpf3D.ToothChartRenderer.SetStartFailureForTests(galleryState.IsFellBack
					? new InvalidOperationException("A synthetic driver failure (gallery).") : null);
				try {
					DrawWindow(Path.Combine(folder,name+"-"+galleryState.Name+".png"));
				}
				finally {
					SparksToothChart.Wpf3D.ToothChartRenderer.SetStartFailureForTests(null);
					Prefs.UpdateBool(PrefName.PracticeIsMedicalOnly,false);
				}
			}
		}

		///<summary>Shows File > Graphics off the desktop, waits for its preview, and saves the window's picture with the preview drawn over its place.</summary>
		private static void DrawWindow(string path) {
			StaRunner.Run(() => {
				using(Form formGraphics=(Form)Activator.CreateInstance(typeof(FormGraphics))) {
					formGraphics.StartPosition=FormStartPosition.Manual;
					formGraphics.Location=new Point(-32000,-32000);
					formGraphics.ShowInTaskbar=false;
					formGraphics.Show();
					FieldInfo fieldInfoWrapper=typeof(FormGraphics).GetField("toothChartWrapper",BindingFlags.NonPublic|BindingFlags.Instance);
					ToothChartWrapper wrapper=(ToothChartWrapper)fieldInfoWrapper?.GetValue(formGraphics);
					System.Diagnostics.Stopwatch stopwatch=System.Diagnostics.Stopwatch.StartNew();
					while(stopwatch.ElapsedMilliseconds<20000) {
						Application.DoEvents();
						if(wrapper==null || wrapper.Engine==ToothChartEngine.Simple2D || wrapper.LiveViewWpf!=null
							|| (wrapper.ImageHostWpf!=null && wrapper.ImageHostWpf.CountImagesReceived>0 && !wrapper.ImageHostWpf.IsRenderInFlight))
						{
							break;
						}
						System.Threading.Thread.Sleep(10);
					}
					for(int i=0;i<20;i++) {
						Application.DoEvents();
						System.Threading.Thread.Sleep(10);
					}
					//The computer's name in the title is this PC's: the picture gets a placeholder instead.
					formGraphics.Text=formGraphics.Text.Replace(ComputerPrefs.LocalComputer.ComputerName,"SYNTHETIC-PC");
					Application.DoEvents();
					using(Bitmap bitmap=new Bitmap(formGraphics.Width,formGraphics.Height)) {
						formGraphics.DrawToBitmap(bitmap,new Rectangle(0,0,bitmap.Width,bitmap.Height));
						//A window's picture leaves out child controls that do not print themselves with their parent, so each control is drawn over
						//its place, parents first; the chart is drawn below.
						Point pointClient=formGraphics.PointToScreen(Point.Empty);
						for(int i=formGraphics.Controls.Count-1;i>=0;i--) {
							Control control=formGraphics.Controls[i];
							DrawTree(control,bitmap,new Point(pointClient.X-formGraphics.Left+control.Left,pointClient.Y-formGraphics.Top+control.Top));
						}
						if(wrapper!=null) {
							Bitmap bitmapPreview;
							if(wrapper.Engine==ToothChartEngine.Simple2D) {
								bitmapPreview=wrapper.GetBitmap();//the 2D chart's own picture, what it shows
							}
							else {
								bitmapPreview=wrapper.LiveViewWpf!=null ? wrapper.LiveViewWpf.GetBitmapForTests() : wrapper.ImageHostWpf?.GetBitmapShown();
							}
							if(bitmapPreview!=null) {
								Point pointScreen=wrapper.PointToScreen(Point.Empty);
								using(Graphics g=Graphics.FromImage(bitmap)) {
									g.DrawImageUnscaled(bitmapPreview,pointScreen.X-formGraphics.Left,pointScreen.Y-formGraphics.Top);
								}
								bitmapPreview.Dispose();
							}
						}
						PngWriter.Save(bitmap,path);
					}
					formGraphics.Close();
				}
			});
		}

		///<summary>Draws a control (DrawToBitmap) at pointOffset in bitmapTarget, then its children over it, back to front. A tooth chart and an
		///ElementHost are left out: the caller draws the chart's own picture.</summary>
		private static void DrawTree(Control control,Bitmap bitmapTarget,Point pointOffset) {
			if(!control.Visible || control.Width<=0 || control.Height<=0 || control is ToothChartWrapper
				|| control is System.Windows.Forms.Integration.ElementHost)
			{
				return;
			}
			using(Bitmap bitmap=new Bitmap(control.Width,control.Height)) {
				control.DrawToBitmap(bitmap,new Rectangle(0,0,bitmap.Width,bitmap.Height));
				using(Graphics g=Graphics.FromImage(bitmapTarget)) {
					g.DrawImageUnscaled(bitmap,pointOffset);
				}
			}
			for(int i=control.Controls.Count-1;i>=0;i--) {
				Control controlChild=control.Controls[i];
				DrawTree(controlChild,bitmapTarget,new Point(pointOffset.X+controlChild.Left,pointOffset.Y+controlChild.Top));
			}
		}
	}
}
