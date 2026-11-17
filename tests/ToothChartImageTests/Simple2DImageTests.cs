// Image tests of the Simple 2D chart's drawing of package TC-5 (docs/specs/wpf-tooth-chart-plan.md, the TC-5 row and O-24): free text, paint text
// other than "W" (in the watch mark's band) and the completed-extraction graphic, drawn with GDI+ by ToothChart2D, against Baselines\simple2d-net48
// with the tolerances of section 7 (TextBelow is not in the picture since the parity round: the Chart module's strip shows it). Only the scenes with TC-5's drawing are gated here: the 2D chart's other pictures are TC-0's recording
// (Baselines\legacy\Simple2D), which TC-5 does not change.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental;
using OpenPmsDentalBusiness;
using SparksToothChart;

namespace ToothChartImageTests {
	///<summary>Record the baselines (after a deliberate change of the 2D chart's TC-5 drawing, in a commit that says why): set OPMS_RECORD_2D_CHART=1
	///for a run of this assembly. Each PNG gets a .txt with the scene, its size and colours, and its pixel SHA-256.</summary>
	[TestClass]
	public class Simple2DImageTests {
		public const string EnvRecord="OPMS_RECORD_2D_CHART";

		///<summary>The scenes whose 2D picture has TC-5's drawing: the older scenes that the 2D chart now draws more of, and TC-5's own scenes.</summary>
		public static readonly string[] ArrayScenes={ "text-other","text-other-eo-r","text-label-long","text-labels-over-teeth",
			"text-labels-at-edges","text-label-moved","extraction-completed-graphic","extraction-completed-graphic-primary","text-below-with-watch",
			"text-below-cut-to-fit","text-other-moved-and-missing","text-label-past-edge","text-all-kinds",
			"extraction-completed-graphic-under-implant","extraction-completed-graphic-under-pontic","text-other-with-watch" };

		///<summary>The folder of the 2D chart's TC-5 baselines on .NET Framework 4.8 x86: tests\ToothChartImageTests\Baselines\simple2d-net48.</summary>
		public static string Baselines2D {
			get {
				return Path.Combine(TestPaths.ProjectFolder,"Baselines","simple2d-net48");
			}
		}

		///<summary>The folder a recording writes to and a comparison starts from: Baselines2D, or in a 64-bit process that records,
		///Baselines\simple2d-net48-x64 (TC-10, docs/port-notes/TC-10.md), so an x64 recording never overwrites the x86 baselines.
		///S3b-5W: on .NET x64 always Baselines\simple2d-net10-x64, the full set, for a recording and a comparison alike.</summary>
		public static string Baselines2DForThisProcess(bool isRecording) {
			if(WpfCharts.IsNet10X64) {
				return WpfCharts.Baselines2DNet10X64;
			}
			if(isRecording && Environment.Is64BitProcess) {
				return Path.Combine(TestPaths.ProjectFolder,"Baselines","simple2d-net48-x64");
			}
			return Baselines2D;
		}

		[TestMethod]
		public void Tc5Scenes_MatchTheSimple2DBaselines() {
			bool isRecording=Environment.GetEnvironmentVariable(EnvRecord)=="1";
			string folderFailures=Path.Combine(Path.GetTempPath(),"ToothChartImageTests","simple2d-failures");
			if(!isRecording && Directory.Exists(folderFailures)) {
				Directory.Delete(folderFailures,true);
			}
			List<string> listFailures=new List<string>();
			int countCompared=0;
			int countWritten=0;
			StaRunner.Run(() => {
				foreach(string sceneName in ArrayScenes) {
					ChartScene scene=ChartScenes.Get(sceneName);
					foreach(ChartSize chartSize in ChartScenes.GetSizes(scene)) {
						string name=chartSize.FolderName+"/"+scene.Name;
						//TC-10: a 64-bit process records into Baselines\simple2d-net48-x64 (keep only the pictures beyond tolerance there, README.md) and
						//compares with the x64 picture where that set has one.
						string pathPng=Path.Combine(Baselines2DForThisProcess(isRecording),chartSize.FolderName,scene.Name+".png");
						using ToothChartWrapper wrapper=new ToothChartWrapper();
						wrapper.Size=chartSize.Size;
						ToothChartRelay relay=WpfCharts.CreateRelay(wrapper);
						using Bitmap bitmap=WpfCharts.Draw(relay,scene,chartSize.ColorSet);
						Assert.AreEqual(DrawingMode.Simple2D,wrapper.DrawMode,name);
						if(isRecording) {
							Directory.CreateDirectory(Path.GetDirectoryName(pathPng));
							PngWriter.Save(bitmap,pathPng);
							BitmapFacts bitmapFacts=ChartImageCompare.DescribeBitmap(bitmap);
							File.WriteAllLines(Path.ChangeExtension(pathPng,".txt"),new[] {
								"Scene: "+scene.Name,
								"Group: "+scene.Group,
								"Description: "+scene.Description,
								"Size: "+chartSize.FolderName,
								"Colours: "+chartSize.ColorSet,
								"Engine: Simple2D (ToothChart2D, GDI+)",
								"Bitmap: "+bitmapFacts.Size.Width+"x"+bitmapFacts.Size.Height+" "+bitmapFacts.PixelFormat,
								"Pixels SHA-256: "+bitmapFacts.PixelsSha256,
								"Framework: "+WpfImageTests.GetFramework()+", "+WpfImageTests.GetBitness(),
							},new UTF8Encoding(false));
							countWritten++;
							continue;
						}
						if(!File.Exists(pathPng)) {
							listFailures.Add(name+": no baseline (record with "+EnvRecord+"=1).");
							continue;
						}
						using Bitmap bitmapBaseline=WpfImageTests.LoadBitmap(WpfCharts.ForThisBitness(pathPng));
						ChartImageCompareResult result=ChartImageCompare.Compare(bitmapBaseline,bitmap);
						countCompared++;
						if(!result.IsPass) {
							string pathDiff=ChartImageCompare.WriteFailureImages(Path.Combine(folderFailures,chartSize.FolderName),scene.Name,bitmapBaseline,bitmap);
							listFailures.Add(name+": "+result+" Images: "+pathDiff);
						}
					}
				}
			});
			if(isRecording) {
				Console.WriteLine(countWritten+" Simple 2D baselines written to "+Baselines2DForThisProcess(true)+".");
				return;
			}
			Assert.AreEqual(0,listFailures.Count,listFailures.Count+" scenes differ from the Simple 2D baselines:\r\n"+string.Join("\r\n",listFailures));
			Assert.IsTrue(countCompared>=ArrayScenes.Length*2,"Compared "+countCompared);
		}

		[TestMethod]
		public void Tc5Scenes_DifferFromTheOldRecording_WhereThe2DChartDrawsMore() {
			//The older scenes in the list were recorded by TC-0, when the 2D chart drew none of this: their 2D picture must have changed.
			StaRunner.Run(() => {
				foreach(string sceneName in ArrayScenes.Where(x => ChartScenes.Get(x).HasLegacyRecording)) {
					ChartScene scene=ChartScenes.Get(sceneName);
					ChartSize chartSize=ChartScenes.SizeModule;
					string pathLegacy=Path.Combine(TestPaths.BaselinesLegacy,"Simple2D",chartSize.FolderName,scene.Name+".png");
					Assert.IsTrue(File.Exists(pathLegacy),pathLegacy);
					using ToothChartWrapper wrapper=new ToothChartWrapper();
					wrapper.Size=chartSize.Size;
					using Bitmap bitmap=WpfCharts.Draw(WpfCharts.CreateRelay(wrapper),scene,chartSize.ColorSet);
					using Bitmap bitmapLegacy=WpfImageTests.LoadBitmap(pathLegacy);
					Assert.IsTrue(ChartImageCompare.Compare(bitmapLegacy,bitmap).CountDifferent>20,sceneName+": the 2D chart should draw TC-5's text now.");
				}
			});
		}
	}
}
