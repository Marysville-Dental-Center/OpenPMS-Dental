// The parity probes of the WPF chart's graphical perio chart against today's DirectX pictures (package TC-7, docs/specs/wpf-tooth-chart-plan.md
// section 7): colours at the teeth and the perio marks, the place of the numbers, the legend's words and the mobility numbers, and the centre line.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental;
using SparksToothChart;
using SparksToothChart.Wpf3D;

namespace ToothChartImageTests {
	///<summary>Every perio scene that TC-0 recorded in DirectX (649x696), probed in the WPF picture and in the DirectX picture. The WPF pictures are the
	///committed baselines (ChartScenes_MatchTheWpfBaselines checks them against fresh renders); with OPMS_PARITY_FRESH=1 the scenes are drawn again
	///instead. Each test writes its results to %TEMP%\ToothChartImageTests (parity-perio*.txt).</summary>
	[TestClass]
	public class PerioParityTests {
		[TestMethod]
		public void PerioParity_TeethAndMarks_MatchTheDirectXRecordings() {
			List<PerioProbeOutcome> listOutcomes=new List<PerioProbeOutcome>();
			List<string> listMissing=new List<string>();
			ForEachRecordedPerioPicture((scene,chartSize,wrapper,arrayDirectX,arrayWpf) => {
				listOutcomes.AddRange(PerioParityProbes.Probe(scene.Name,chartSize.Size,PerioParityProbes.GetPoints(wrapper.TcData),arrayDirectX,arrayWpf,
					wrapper.TcData.ColorBackground));
			},listMissing);
			List<PerioProbeOutcome> listFailed=listOutcomes.Where(x => x.Failure!=null && x.Allowance==null).ToList();
			List<PerioProbeOutcome> listAllowed=listOutcomes.Where(x => x.Failure!=null && x.Allowance!=null).ToList();
			StringBuilder sb=new StringBuilder();
			sb.AppendLine("Perio parity probes of the WPF chart against the DirectX recordings: "+listOutcomes.Count+" probes, "
				+listOutcomes.Count(x => x.Failure==null)+" pass, "+listAllowed.Count+" differ and are allowed, "+listFailed.Count+" fail.");
			foreach(var group in listOutcomes.GroupBy(x => x.Probe.Kind)) {
				sb.AppendLine("  "+group.Key+": "+group.Count()+" probes, "+group.Count(x => x.Failure==null)+" pass, "+group.Count(x => x.Failure!=null && x.Allowance!=null)
					+" allowed, "+group.Count(x => x.Failure!=null && x.Allowance==null)+" fail");
			}
			foreach(PerioProbeOutcome outcome in listAllowed) {
				sb.AppendLine("Allowed: "+outcome);
			}
			foreach(PerioProbeOutcome outcome in listFailed.Take(400)) {
				sb.AppendLine("Failed: "+outcome);
			}
			WpfCharts.WriteEvidence("parity-perio.txt",sb.ToString());
			Assert.AreEqual(0,listMissing.Count,"Pictures missing:\r\n"+string.Join("\r\n",listMissing));
			foreach(string kind in new[] { "tooth","implant","probing","bleeding","suppuration","furcation","line","legend" }) {
				//The fewest are the implants: 19 and 30 of perio-missing-and-implants, each in its arch's two rows.
				Assert.IsTrue(listOutcomes.Count(x => x.Probe.Kind==kind)>=4,"Probes of "+kind+": "+listOutcomes.Count(x => x.Probe.Kind==kind));
			}
			Assert.IsTrue(listOutcomes.Count>2000,"Probes: "+listOutcomes.Count);
			List<PerioAllowance> listUnused=PerioParityProbes.Allowances.Where(x => !listAllowed.Any(y => y.Allowance==x)).ToList();
			Assert.AreEqual(0,listUnused.Count,"Allowances that cover nothing (remove them): "+string.Join("; ",listUnused));
			Assert.AreEqual(0,listFailed.Count,listFailed.Count+" probes differ from DirectX:\r\n"+string.Join("\r\n",listFailed.Take(60)));
		}

		[TestMethod]
		public void PerioParity_NumbersLegendAndMobility_SitWhereDirectXDrawsThem_WithinOnePixel() {
			List<string> listOutcomes=new List<string>();
			List<string> listFailed=new List<string>();
			Dictionary<string,int> dictCountByKind=new Dictionary<string,int>();
			List<string> listMissing=new List<string>();
			ForEachRecordedPerioPicture((scene,chartSize,wrapper,arrayDirectX,arrayWpf) => {
				ToothScene toothScene=ToothSceneBuilder.Build(wrapper.TcData);
				foreach(ToothSceneItem2D item in toothScene.Items2D.Where(x => x.Kind==ChartItem2DKind.Text)) {
					string kind;
					Func<Color,bool> funcIsInk;
					if(item.Source.StartsWith("number ")) {
						kind="number";
					}
					else if(item.Source.StartsWith(ToothSceneBuilder.SourceLegend)) {
						kind="legend";
					}
					else if(item.Source.StartsWith(ToothSceneBuilder.SourceMobility)) {
						kind="mobility";
					}
					else {
						continue;
					}
					Color colorText=Color.FromArgb(255,item.Color);
					Color colorBack=toothScene.ColorBackground;
					if(kind=="mobility") {
						if(!ParityProbes.IsColoured(colorText)) {
							continue;//a black mobility number is drawn over the tooth, whose shading is dark too: only a coloured one is found by its hue
						}
						float hueText=colorText.GetHue();
						funcIsInk=color => color.GetSaturation()>=0.3 && color.GetBrightness()>0.08 && color.GetBrightness()<0.92
							&& ParityProbes.GetHueDifference(color.GetHue(),hueText)<=30;
					}
					else {
						funcIsInk=color => ParityProbeTests.GetProjection(color,colorBack,colorText)>=0.4 && ParityProbeTests.GetDistanceFromLine(color,colorBack,colorText)<=48;
					}
					Rectangle rectangleSearch=new Rectangle(item.Origin.X-1,item.Origin.Y,item.WidthPixels+2,item.Font.Height);
					Rectangle? rectangleDirectX=ParityProbeTests.GetInkBounds(arrayDirectX,chartSize.Size,rectangleSearch,funcIsInk);
					if(rectangleDirectX==null) {
						continue;
					}
					dictCountByKind[kind]=(dictCountByKind.TryGetValue(kind,out int count)?count:0)+1;
					Rectangle? rectangleWpf=ParityProbeTests.GetInkBounds(arrayWpf,chartSize.Size,rectangleSearch,funcIsInk);
					string text=scene.Name+" "+item.Source+" \""+item.Text+"\": DirectX "+Describe(rectangleDirectX.Value)+", WPF "
						+(rectangleWpf==null?"no ink":Describe(rectangleWpf.Value));
					listOutcomes.Add(text);
					if(rectangleWpf==null) {
						listFailed.Add(text);
						continue;
					}
					Rectangle d=rectangleDirectX.Value;
					Rectangle w=rectangleWpf.Value;
					int worst=new[] { w.Left-d.Left,w.Top-d.Top,w.Right-d.Right,w.Bottom-d.Bottom }.Max(x => Math.Abs(x));
					if(worst>ParityProbeTests.MaxTextEdgePixels) {
						listFailed.Add(text+" (an edge "+worst+" pixels off)");
					}
				}
			},listMissing);
			WpfCharts.WriteEvidence("parity-perio-text.txt","Perio text boxes against DirectX: "+listOutcomes.Count+" ("+string.Join(", ",dictCountByKind.Select(x => x.Value+" "+x.Key))
				+"), "+listFailed.Count+" with an edge of their ink more than "+ParityProbeTests.MaxTextEdgePixels+" pixel from DirectX's.\r\n"
				+string.Join("\r\n",listFailed.Select(x => "Failed: "+x)));
			Assert.AreEqual(0,listMissing.Count,"Pictures missing:\r\n"+string.Join("\r\n",listMissing));
			Assert.IsTrue(dictCountByKind.TryGetValue("number",out int countNumbers) && countNumbers>=300,"Numbers: "+countNumbers);
			Assert.IsTrue(dictCountByKind.TryGetValue("legend",out int countLegend) && countLegend>=80,"Legend words: "+countLegend);
			Assert.IsTrue(dictCountByKind.TryGetValue("mobility",out int countMobility) && countMobility>=10,"Red mobility numbers: "+countMobility);
			Assert.AreEqual(0,listFailed.Count,listFailed.Count+" text boxes are off:\r\n"+string.Join("\r\n",listFailed.Take(60)));
		}

		[TestMethod]
		public void PerioParity_CentreLineAndLegendBars_FillThePixelsDirectXFills() {
			//The centre line, the legend's three line samples and its two probing bars are axis-aligned rectangles, snapped to the pixels DirectX fills
			//(ToothSceneItem2D.SnapToDirectXPixels): their rows (and the bars' columns) must be DirectX's exactly.
			List<string> listFailures=new List<string>();
			int countChecks=0;
			List<string> listMissing=new List<string>();
			ForEachRecordedPerioPicture((scene,chartSize,wrapper,arrayDirectX,arrayWpf) => {
				ToothScene toothScene=ToothSceneBuilder.Build(wrapper.TcData);
				foreach(ToothSceneItem2D item in toothScene.Items2D.Where(x => x.Kind==ChartItem2DKind.Polygons && (x.Source==ToothSceneBuilder.SourcePerioCentreLine
					|| x.Source==ToothSceneBuilder.SourceLegend+"gingival margin" || x.Source==ToothSceneBuilder.SourceLegend+"CAL" || x.Source==ToothSceneBuilder.SourceLegend+"MGJ"
					|| x.Source==ToothSceneBuilder.SourceLegend+"probing" || x.Source==ToothSceneBuilder.SourceLegend+"probing red")))
				{
					PointF[] arrayPoints=item.Polygons.Single();
					Rectangle rectangle=Rectangle.FromLTRB((int)arrayPoints.Min(x => x.X),(int)arrayPoints.Min(x => x.Y),(int)arrayPoints.Max(x => x.X),
						(int)arrayPoints.Max(x => x.Y));
					bool isBar=item.Source.StartsWith(ToothSceneBuilder.SourceLegend+"probing");
					int xMiddle=(rectangle.Left+rectangle.Right)/2;
					int yMiddle=(rectangle.Top+rectangle.Bottom-1)/2;
					foreach(int x in isBar?new[] { xMiddle }:new[] { rectangle.Left+3,xMiddle,rectangle.Right-4 }) {
						countChecks++;
						string rowsDirectX=GetRun(arrayDirectX,chartSize.Size,item.Color,x,yMiddle,false);
						string rowsWpf=GetRun(arrayWpf,chartSize.Size,item.Color,x,yMiddle,false);
						if(rowsDirectX!=rowsWpf) {
							listFailures.Add(scene.Name+" "+item.Source+" at column "+x+": DirectX fills rows "+rowsDirectX+", WPF "+rowsWpf);
						}
					}
					if(isBar) {
						countChecks++;
						string columnsDirectX=GetRun(arrayDirectX,chartSize.Size,item.Color,xMiddle,yMiddle,true);
						string columnsWpf=GetRun(arrayWpf,chartSize.Size,item.Color,xMiddle,yMiddle,true);
						if(columnsDirectX!=columnsWpf) {
							listFailures.Add(scene.Name+" "+item.Source+" at row "+yMiddle+": DirectX fills columns "+columnsDirectX+", WPF "+columnsWpf);
						}
					}
				}
			},listMissing);
			WpfCharts.WriteEvidence("parity-perio-crisp.txt","Perio centre line and legend bars against DirectX: "+countChecks+" checks, "+listFailures.Count+" differ.\r\n"
				+string.Join("\r\n",listFailures));
			Assert.AreEqual(0,listMissing.Count,"Pictures missing:\r\n"+string.Join("\r\n",listMissing));
			Assert.IsTrue(countChecks>=100,"Checks: "+countChecks);
			Assert.AreEqual(0,listFailures.Count,listFailures.Count+" differ:\r\n"+string.Join("\r\n",listFailures.Take(40)));
		}

		///<summary>The run of pixels of exactly the colour through (x,y), down the column (or along the row, isHorizontal), as "first-last", or "none".</summary>
		private static string GetRun(byte[] arrayPixelsArgb,Size size,Color color,int x,int y,bool isHorizontal) {
			Func<int,int,bool> funcIs=(xPixel,yPixel) => {
				if(xPixel<0 || yPixel<0 || xPixel>=size.Width || yPixel>=size.Height) {
					return false;
				}
				int index=(yPixel*size.Width+xPixel)*4;
				return arrayPixelsArgb[index+2]==color.R && arrayPixelsArgb[index+1]==color.G && arrayPixelsArgb[index]==color.B;
			};
			if(!funcIs(x,y)) {
				return "none";
			}
			int first=isHorizontal?x:y;
			int last=first;
			while(isHorizontal?funcIs(first-1,y):funcIs(x,first-1)) {
				first--;
			}
			while(isHorizontal?funcIs(last+1,y):funcIs(x,last+1)) {
				last++;
			}
			return first+"-"+last;
		}

		private static string Describe(Rectangle rectangle) {
			return rectangle.Left+","+rectangle.Top+" to "+rectangle.Right+","+rectangle.Bottom;
		}

		///<summary>Runs action for every perio scene that TC-0 recorded in DirectX, on an STA thread, with the scene prepared on a new Wpf3D wrapper, the
		///DirectX picture's pixels and the WPF picture's pixels (the committed baseline, or a fresh render with OPMS_PARITY_FRESH=1).</summary>
		public static void ForEachRecordedPerioPicture(Action<ChartScene,ChartSize,ToothChartWrapper,byte[],byte[]> action,List<string> listMissing) {
			bool isFresh=Environment.GetEnvironmentVariable(ParityProbeTests.EnvFresh)=="1";
			StaRunner.Run(() => {
				WpfCharts.ForEachPerioScene((scene,chartSize,wrapper,relay) => {
					if(!scene.HasLegacyRecording) {
						return;
					}
					string name=chartSize.FolderName+"/"+scene.Name;
					string pathDirectX=Path.Combine(TestPaths.BaselinesLegacy,"DirectX",chartSize.FolderName,scene.Name+".png");
					if(!File.Exists(pathDirectX)) {
						listMissing.Add(name+": no DirectX recording");
						return;
					}
					byte[] arrayWpf;
					if(isFresh) {
						using Bitmap bitmapWpf=WpfCharts.Draw(relay,scene,chartSize.ColorSet);
						arrayWpf=ChartImageCompare.GetPixelsArgb(bitmapWpf);
					}
					else {
						string pathWpf=WpfCharts.GetPicturePath(WpfCharts.BaselinesWpf,chartSize,scene);
						if(!File.Exists(pathWpf)) {
							listMissing.Add(name+": no WPF baseline");
							return;
						}
						ChartScenes.Prepare(relay,scene,chartSize.ColorSet);
						using Bitmap bitmapWpf=WpfImageTests.LoadBitmap(pathWpf);
						arrayWpf=ChartImageCompare.GetPixelsArgb(bitmapWpf);
					}
					using Bitmap bitmapDirectX=WpfImageTests.LoadBitmap(pathDirectX);
					action(scene,chartSize,wrapper,ChartImageCompare.GetPixelsArgb(bitmapDirectX),arrayWpf);
				});
			});
		}
	}
}
