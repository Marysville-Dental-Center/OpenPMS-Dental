// File > Graphics, rebuilt in TC-8 (the TC-8 row's "Setup > Graphics"), through the program's own window: it shows the stored choice as "3D chart"
// or "Simple 2D chart" and the "Draw in software" switch, its preview starts what the choices give, and Save writes the save rule to this
// computer's computerpref row in the scratch database. The window is made and its Load and Save handlers are called, but it is not shown;
// for gallery G4 (OPMS_CHART_GALLERY) one test draws it off the desktop into a picture.
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
		private ComputerPref _computerPrefBefore;

		[TestInitialize]
		public void TestInitialize() {
			HostDb.EnsureOrSkip();
			HostDb.SetCulture();
			_computerPrefBefore=ComputerPrefs.LocalComputer.Copy();
		}

		[TestCleanup]
		public void TestCleanup() {
			if(_computerPrefBefore==null || ComputerPrefs.IsLocalComputerNull()) {
				return;
			}
			ComputerPref computerPrefOld=ComputerPrefs.LocalComputer.Copy();
			ComputerPrefs.LocalComputer.GraphicsSimple=_computerPrefBefore.GraphicsSimple;
			ComputerPrefs.LocalComputer.DirectXFormat=_computerPrefBefore.DirectXFormat;
			ComputerPrefs.Update(ComputerPrefs.LocalComputer,computerPrefOld);
		}

		///<summary>Stores the local computer's choice in the database, as the program last saved it.</summary>
		private static void StoreLocalComputer(DrawingMode drawingMode,string directXFormat) {
			ComputerPref computerPrefOld=ComputerPrefs.LocalComputer.Copy();
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

		///<summary>The stored row of this computer, read from the database: "GraphicsSimple DirectXFormat".</summary>
		private static string ReadStoredRow() {
			return DataCore.GetScalar("SELECT CONCAT(GraphicsSimple,' ',DirectXFormat) FROM computerpref WHERE ComputerPrefNum="
				+POut.Long(ComputerPrefs.LocalComputer.ComputerPrefNum));
		}

		[TestMethod]
		public void Load_ShowsTheStoredChoice_AndThePreviewStartsIt() {
			object[][] arrayCases=new object[][] {
				new object[] { DrawingMode.DirectX,"",true,false,ToothChartEngine.Wpf3D,"3D chart, live view." },
				new object[] { DrawingMode.OpenGL,"",true,false,ToothChartEngine.Wpf3D,"3D chart, live view." },
				new object[] { DrawingMode.DirectX,"0;Hardware;64;D16;R5G6B5;FourSamples",true,false,ToothChartEngine.Wpf3D,"3D chart, live view." },
				new object[] { DrawingMode.DirectX,ToothChartWrapper.DirectXFormatWpfSoftware,true,true,ToothChartEngine.Wpf3D,
					"3D chart, drawn in software: the smoothed picture, the same as printouts and sheets." },
				new object[] { DrawingMode.Simple2D,"",false,false,ToothChartEngine.Simple2D,"Simple 2D chart." },
				new object[] { DrawingMode.Simple2D,ToothChartWrapper.DirectXFormatWpfSoftware,false,true,ToothChartEngine.Simple2D,"Simple 2D chart." },
			};
			foreach(object[] arrayCase in arrayCases) {
				StoreLocalComputer((DrawingMode)arrayCase[0],(string)arrayCase[1]);
				string what="stored "+arrayCase[0]+" \""+arrayCase[1]+"\"";
				StaRunner.Run(() => {
					using(FormGraphics formGraphics=new FormGraphics()) {
						Invoke(formGraphics,"FormGraphics_Load");
						Assert.AreEqual((bool)arrayCase[2],GetField<RadioButton>(formGraphics,"radio3DChart").Checked,what+": 3D chart");
						Assert.AreEqual(!(bool)arrayCase[2],GetField<RadioButton>(formGraphics,"radioSimpleChart").Checked,what+": Simple 2D chart");
						OpenPmsDental.UI.CheckBox checkDrawInSoftware=GetField<OpenPmsDental.UI.CheckBox>(formGraphics,"checkDrawInSoftware");
						Assert.AreEqual((bool)arrayCase[3],checkDrawInSoftware.Checked,what+": Draw in software");
						Assert.AreEqual((bool)arrayCase[2],checkDrawInSoftware.Enabled,what+": the check box is the 3D chart's only");
						ToothChartWrapper wrapper=GetField<ToothChartWrapper>(formGraphics,"toothChartWrapper");
						Assert.AreEqual((ToothChartEngine)arrayCase[4],wrapper.Engine,what+": the preview's engine");
						Assert.AreEqual((string)arrayCase[5],GetField<Label>(formGraphics,"labelPreviewStatus").Text,what+": the status line");
						using(Bitmap bitmap=wrapper.GetBitmap()) {
							Assert.AreEqual(new Size(410,307),bitmap.Size);
							Assert.IsFalse(ChartImageCompare.DescribeBitmap(bitmap).IsBlank,what+": the preview's sample is drawn");
						}
					}
				});
				Assert.AreEqual((int)(DrawingMode)arrayCase[0]+" "+arrayCase[1],ReadStoredRow(),what+": Load changes nothing.");
			}
		}

		[TestMethod]
		public void Save_WritesTheSaveRule_ToThisComputersRow() {
			//(stored mode, stored format, "3D chart", "Draw in software") -> the stored row after Save
			object[][] arrayCases=new object[][] {
				new object[] { DrawingMode.Simple2D,"",true,false,"0 " },
				new object[] { DrawingMode.DirectX,"",true,false,"0 " },
				new object[] { DrawingMode.OpenGL,"",true,false,"2 " },
				new object[] { DrawingMode.OpenGL,"",false,false,"1 " },
				new object[] { DrawingMode.DirectX,"0;Hardware;64;D16;R5G6B5;FourSamples",true,false,"0 0;Hardware;64;D16;R5G6B5;FourSamples" },
				new object[] { DrawingMode.DirectX,"0;Hardware;64;D16;R5G6B5;FourSamples",true,true,"0 WpfSoftware" },
				new object[] { DrawingMode.OpenGL,ToothChartWrapper.DirectXFormatWpfSoftware,true,false,"2 " },
				new object[] { DrawingMode.Simple2D,"",true,true,"0 WpfSoftware" },
			};
			foreach(object[] arrayCase in arrayCases) {
				StoreLocalComputer((DrawingMode)arrayCase[0],(string)arrayCase[1]);
				string what="stored "+arrayCase[0]+" \""+arrayCase[1]+"\", 3D chart "+arrayCase[2]+", Draw in software "+arrayCase[3];
				DialogResult dialogResult=DialogResult.None;
				StaRunner.Run(() => {
					using(FormGraphics formGraphics=new FormGraphics()) {
						Invoke(formGraphics,"FormGraphics_Load");
						RadioButton radio3DChart=GetField<RadioButton>(formGraphics,"radio3DChart");
						RadioButton radioSimpleChart=GetField<RadioButton>(formGraphics,"radioSimpleChart");
						OpenPmsDental.UI.CheckBox checkDrawInSoftware=GetField<OpenPmsDental.UI.CheckBox>(formGraphics,"checkDrawInSoftware");
						if((bool)arrayCase[2]) {
							radio3DChart.Checked=true;
							Invoke(formGraphics,"radio3DChart_Click");
						}
						else {
							radioSimpleChart.Checked=true;
							Invoke(formGraphics,"radioSimpleChart_Click");
						}
						checkDrawInSoftware.Checked=(bool)arrayCase[3];
						Invoke(formGraphics,"checkDrawInSoftware_Click");
						Invoke(formGraphics,"butSave_Click");
						dialogResult=formGraphics.DialogResult;
					}
				});
				Assert.AreEqual(DialogResult.OK,dialogResult,what);
				Assert.AreEqual((string)arrayCase[4],ReadStoredRow(),what);
				//What was saved starts the chart the window showed (the Chart module's InitializeLocalData reads the row through the relay).
				StaRunner.Run(() => {
					using(ToothChartWrapper wrapper=new ToothChartWrapper()) {
						wrapper.Size=new Size(410,307);
						ToothChartRelay.ApplyComputerSettings(wrapper);
						Assert.AreEqual((bool)arrayCase[2] ? ToothChartEngine.Wpf3D : ToothChartEngine.Simple2D,wrapper.Engine,what);
						Assert.AreEqual((bool)arrayCase[3],wrapper.WpfDrawInSoftware,what);
					}
				});
			}
		}

		[TestMethod]
		public void Gallery_DrawsTheWindowOffTheDesktop() {
			//Gallery G4 (TC-8's part): File > Graphics drawn into a picture (gallery folder, OPMS_CHART_GALLERY_GRAPHICS_NAME or "graphics-after"), the
			//window placed off the desktop as the image tests place their forms. Its 3D preview is a WPF live view or the image host, which a window's
			//picture (DrawToBitmap) leaves out, so the preview's own picture is drawn over its place.
			string folder=HostPictures.GalleryFolder;
			if(folder==null) {
				Assert.Inconclusive(HostPictures.EnvGallery+" is not set; this draws the window for the gallery only.");
			}
			string name=Environment.GetEnvironmentVariable("OPMS_CHART_GALLERY_GRAPHICS_NAME");
			if(string.IsNullOrWhiteSpace(name)) {
				name="graphics-after";
			}
			foreach(bool isDrawnInSoftware in new[] { false,true }) {
				StoreLocalComputer(DrawingMode.DirectX,isDrawnInSoftware ? ToothChartWrapper.DirectXFormatWpfSoftware : "");
				string fileName=name+(isDrawnInSoftware ? "-software" : "")+".png";
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
							if(wrapper==null || wrapper.LiveViewWpf!=null
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
								Bitmap bitmapPreview=wrapper.LiveViewWpf!=null ? wrapper.LiveViewWpf.GetBitmapForTests() : wrapper.ImageHostWpf?.GetBitmapShown();
								if(bitmapPreview!=null) {
									Point pointScreen=wrapper.PointToScreen(Point.Empty);
									using(Graphics g=Graphics.FromImage(bitmap)) {
										g.DrawImageUnscaled(bitmapPreview,pointScreen.X-formGraphics.Left,pointScreen.Y-formGraphics.Top);
									}
									bitmapPreview.Dispose();
								}
							}
							PngWriter.Save(bitmap,Path.Combine(folder,fileName));
						}
						formGraphics.Close();
					}
				});
				if(name.Contains("before")) {
					break;//the window before TC-8 has no software switch
				}
			}
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
