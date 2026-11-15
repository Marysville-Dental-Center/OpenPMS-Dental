// Scene-builder and wrapper tests of the WPF chart's graphical perio chart (package TC-7, docs/specs/wpf-tooth-chart-plan.md, the TC-7 row): DirectX's
// order of each row's decorations, the perio colours and red limits, the droplet flip, the centre line and numbers, the legend and its Recession term
// (Open Dental 24.4), perio mode on the WPF chart, and the relay's perio helpers.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental;
using OpenPmsDentalBusiness;
using SparksToothChart;
using SparksToothChart.Wpf3D;

namespace ToothChartImageTests {
	[TestClass]
	public class PerioSceneTests {
		private static readonly string[] _arrayRowNames={ "maxillary facial row","maxillary lingual row","mandibular lingual row","mandibular facial row" };

		///<summary>The scene of a perio scene, prepared through the relay on a new Wpf3D wrapper of the perio size (no render).</summary>
		private static ToothScene BuildPerioScene(string sceneName) {
			return StaRunner.Run(() => {
				using(ToothChartWrapper wrapper=WpfCharts.CreateWrapper(ChartScenes.SizePerio.Size)) {
					ChartScenes.Prepare(WpfCharts.CreateRelay(wrapper),ChartScenes.Get(sceneName),ChartColorSet.Perio);
					return ToothSceneBuilder.Build(wrapper.TcData);
				}
			});
		}

		///<summary>A perio chart's data made without a wrapper (WpfSceneTests.CreateData) in perio mode at the perio size, with the scenes' colours.</summary>
		private static ToothChartData CreatePerioData() {
			ToothChartData tcData=WpfSceneTests.CreateData(ChartScenes.SizePerio.Size);
			tcData.PerioMode=true;
			tcData.ColorBackground=Color.White;
			tcData.ColorText=Color.Black;
			tcData.ColorBleeding=ChartScenes.PerioBleeding;
			tcData.ColorSuppuration=ChartScenes.PerioSuppuration;
			tcData.ColorProbing=ChartScenes.PerioProbing;
			tcData.ColorProbingRed=ChartScenes.PerioProbingRed;
			tcData.ColorGingivalMargin=ChartScenes.PerioGM;
			tcData.ColorCAL=ChartScenes.PerioCAL;
			tcData.ColorMGJ=ChartScenes.PerioMGJ;
			tcData.ColorFurcations=ChartScenes.PerioFurcations;
			tcData.ColorFurcationsRed=ChartScenes.PerioFurcationsRed;
			tcData.RedLimitProbing=4;
			tcData.RedLimitFurcations=2;
			return tcData;
		}

		private static void AddMeasure(ToothChartData tcData,int intTooth,PerioSequenceType perioSequenceType,int mb,int b,int db,int ml,int l,int dl) {
			PerioMeasure perioMeasure=new PerioMeasure();
			perioMeasure.IntTooth=intTooth;
			perioMeasure.SequenceType=perioSequenceType;
			perioMeasure.MBvalue=mb;
			perioMeasure.Bvalue=b;
			perioMeasure.DBvalue=db;
			perioMeasure.MLvalue=ml;
			perioMeasure.Lvalue=l;
			perioMeasure.DLvalue=dl;
			tcData.ListPerioMeasure.Add(perioMeasure);
		}

		private static int IndexOf(ToothScene scene,Func<ToothSceneItem2D,bool> func,string what) {
			for(int i=0;i<scene.Items2D.Count;i++) {
				if(func(scene.Items2D[i])) {
					return i;
				}
			}
			Assert.Fail("No 2D item: "+what+"\r\n"+scene.Describe());
			return -1;
		}

		private static int LastIndexOf(ToothScene scene,Func<ToothSceneItem2D,bool> func,string what) {
			for(int i=scene.Items2D.Count-1;i>=0;i--) {
				if(func(scene.Items2D[i])) {
					return i;
				}
			}
			Assert.Fail("No 2D item: "+what+"\r\n"+scene.Describe());
			return -1;
		}

		[TestMethod]
		public void PerioScene_EachRow_DrawsItsDecorationsInDirectXsOrder() {
			ToothScene scene=BuildPerioScene("perio-full-exam");
			Assert.IsTrue(scene.IsPerio);
			Assert.AreEqual(0.6f,scene.LightAmbient,"The perio light: ambient 0.6.");
			Assert.AreEqual(0.4f,scene.LightDiffuse,"The perio light: diffuse 0.4.");
			int indexCentre=IndexOf(scene,x => x.Source==ToothSceneBuilder.SourcePerioCentreLine,"centre line");
			for(int r=0;r<_arrayRowNames.Length;r++) {
				string rowName=_arrayRowNames[r];
				bool isLingual=rowName.Contains("lingual");
				int indexZeroLine=IndexOf(scene,x => x.Source==ToothSceneBuilder.SourcePerioLine+"0 of the "+rowName,"0 line of the "+rowName);
				int indexNine=IndexOf(scene,x => x.Source==ToothSceneBuilder.SourcePerioLine+"9 of the "+rowName,"9 mm line of the "+rowName);
				Assert.IsTrue(indexNine==indexZeroLine+3,"The 0 line, then 3, 6 and 9 mm.");
				int indexRowEnd=r+1<_arrayRowNames.Length
					?IndexOf(scene,x => x.Source==ToothSceneBuilder.SourcePerioLine+"0 of the "+_arrayRowNames[r+1],"the next row")
					:indexCentre;
				Assert.IsTrue(indexRowEnd>indexNine,"Each row's decorations come before the next row's.");
				List<string> listSitesOfRow=isLingual?new List<string> { " DL"," L"," ML" }:new List<string> { " DB"," B"," MB" };
				//The row's marks (mobility only in a facial row, the others at the row's sites), then its lines: GM, then CAL, then MGJ.
				string[] arrayLineSources={ ToothSceneBuilder.SourceGingivalMargin+"of the "+rowName,ToothSceneBuilder.SourceCAL+"of the "+rowName,
					ToothSceneBuilder.SourceMGJ+"of the "+rowName };
				int stage=-1;//-1: marks; 0, 1, 2: the GM, CAL and MGJ lines
				int countMarks=0;
				for(int i=indexNine+1;i<indexRowEnd;i++) {
					ToothSceneItem2D item=scene.Items2D[i];
					int indexLine=Array.IndexOf(arrayLineSources,item.Source);
					if(indexLine>=0) {
						Assert.IsTrue(indexLine>=stage,"GM, then CAL, then MGJ, after the marks: "+item);
						stage=indexLine;
						continue;
					}
					Assert.AreEqual(-1,stage,"No mark after the lines: "+item);
					countMarks++;
					if(item.Source.StartsWith(ToothSceneBuilder.SourceMobility)) {
						Assert.IsFalse(isLingual,"Mobility only in the facial rows: "+item);
						continue;
					}
					Assert.IsTrue(listSitesOfRow.Any(x => item.Source.Contains(x)),"A mark of the "+rowName+" at one of its sites: "+item);
				}
				Assert.IsTrue(countMarks>20,"The "+rowName+" has marks: "+countMarks);
				Assert.IsTrue(stage>=0,"The "+rowName+" has lines.");
			}
			//Then the centre line, the numbers 1-32 and the legend.
			List<ToothSceneItem2D> listNumbers=scene.Items2D.Where(x => x.Source.StartsWith("number ")).ToList();
			CollectionAssert.AreEqual(Enumerable.Range(1,32).Select(x => "number "+x).ToList(),listNumbers.Select(x => x.Source).ToList(),"Numbers 1-32, no primary letters.");
			Assert.IsFalse(scene.Items2D.Any(x => x.Source.StartsWith("highlight of ")),"The perio chart highlights no number.");
			Assert.IsTrue(scene.Items2D.IndexOf(listNumbers[0])>indexCentre);
			int indexLegend=IndexOf(scene,x => x.Source.StartsWith(ToothSceneBuilder.SourceLegend),"legend");
			Assert.IsTrue(indexLegend>scene.Items2D.IndexOf(listNumbers.Last()),"The legend last.");
			Assert.IsTrue(scene.Items2D.Skip(indexLegend).All(x => x.Source.StartsWith(ToothSceneBuilder.SourceLegend)));
			CollectionAssert.AreEqual(new[] { "Bleeding","Suppuration","Probing","Gingival Margin","Clinical Attachment Level","Mucogingival Junction","Furcation 1",
				"Furcation 2","Furcation 3" },scene.Items2D.Skip(indexLegend).Where(x => x.Kind==ChartItem2DKind.Text).Select(x => x.Text).ToArray());
			//Nothing of the normal chart.
			Assert.IsFalse(scene.Items2D.Any(x => x.Source=="centre line" || x.Source.StartsWith("watch ") || x.Source.StartsWith("sealant ")));
			foreach(ToothSceneItem2D item in scene.Items2D.Where(x => x.Kind==ChartItem2DKind.Polygons)) {
				Assert.AreEqual(255,item.Color.A,item.ToString());
			}
		}

		[TestMethod]
		public void PerioScene_OneTooth_MarksInDirectXsOrder_WithTheirColoursAndRedLimits() {
			ToothChartData tcData=CreatePerioData();
			AddMeasure(tcData,3,PerioSequenceType.Furcation,1,2,3,-1,-1,-1);//MB 1, B 2, DB 3
			AddMeasure(tcData,3,PerioSequenceType.Probing,3,4,5,2,-1,0);//MB 3, B 4, DB 5, ML 2, L none, DL 0
			AddMeasure(tcData,3,PerioSequenceType.GingMargin,0,0,0,0,0,0);
			AddMeasure(tcData,3,PerioSequenceType.BleedSupPlaqCalc,(int)BleedingFlags.Blood,(int)(BleedingFlags.Blood|BleedingFlags.Suppuration),
				(int)BleedingFlags.Suppuration,(int)BleedingFlags.Blood,0,0);
			tcData.ListToothGraphics["3"].Mobility="2";
			tcData.ListToothGraphics["3"].colorMobility=Color.Red;
			ToothScene scene=ToothSceneBuilder.Build(tcData);
			List<ToothSceneItem2D> listOf3=scene.Items2D.Where(x => x.Source.EndsWith(" of 3") || x.Source.Contains(" of 3 ") || x.Source.StartsWith("probing 3 ")
				|| x.Source.StartsWith("bleeding 3 ") || x.Source.StartsWith("suppuration 3 ")).ToList();
			Assert.AreEqual(string.Join(" | ",new[] {
				"mobility of 3",
				"furcation 3 of 3 DB","furcation 3 of 3 DB (filled)","furcation 2 of 3 B","furcation 1 of 3 MB",
				"probing 3 DB","probing 3 B","probing 3 MB",
				"bleeding 3 B","bleeding 3 MB",
				"suppuration 3 DB","suppuration 3 B",
				"probing 3 ML",
				"bleeding 3 ML",
			}),string.Join(" | ",listOf3.Select(x => x.Source)),"DirectX's order: mobility, then per site of the row furcations, probing, bleeding, suppuration; the "
				+"lingual row's marks come in its own row.");
			Dictionary<string,ToothSceneItem2D> dict=listOf3.GroupBy(x => x.Source).ToDictionary(x => x.Key,x => x.First());
			Assert.AreEqual(Color.Red.ToArgb(),dict["mobility of 3"].Color.ToArgb(),"Mobility in its own colour.");
			Assert.AreEqual("2",dict["mobility of 3"].Text);
			Assert.AreEqual(ChartScenes.PerioFurcations.ToArgb(),dict["furcation 1 of 3 MB"].Color.ToArgb(),"Grade 1 below the red limit 2.");
			Assert.AreEqual(ChartScenes.PerioFurcationsRed.ToArgb(),dict["furcation 2 of 3 B"].Color.ToArgb(),"Grade 2 at the red limit.");
			Assert.AreEqual(ChartScenes.PerioFurcationsRed.ToArgb(),dict["furcation 3 of 3 DB (filled)"].Color.ToArgb());
			Assert.AreEqual(2,dict["furcation 1 of 3 MB"].Polygons.Count,"Grade 1: the two sides from the tip.");
			Assert.AreEqual(3,dict["furcation 2 of 3 B"].Polygons.Count,"Grade 2: the outline.");
			Assert.AreEqual(3,dict["furcation 3 of 3 DB"].Polygons.Count,"Grade 3: the outline...");
			Assert.AreEqual(1,dict["furcation 3 of 3 DB (filled)"].Polygons.Count,"...and the filled triangle.");
			Assert.AreEqual(ChartScenes.PerioProbing.ToArgb(),dict["probing 3 MB"].Color.ToArgb(),"3 mm below the red limit 4.");
			Assert.AreEqual(ChartScenes.PerioProbingRed.ToArgb(),dict["probing 3 B"].Color.ToArgb(),"4 mm at the red limit.");
			Assert.AreEqual(ChartScenes.PerioProbingRed.ToArgb(),dict["probing 3 DB"].Color.ToArgb(),"5 mm.");
			Assert.AreEqual(ChartScenes.PerioBleeding.ToArgb(),dict["bleeding 3 B"].Color.ToArgb());
			Assert.AreEqual(ChartScenes.PerioSuppuration.ToArgb(),dict["suppuration 3 B"].Color.ToArgb());
			Assert.IsFalse(scene.Items2D.Any(x => x.Source=="probing 3 L" || x.Source=="probing 3 DL"),"No bar for -1 or 0.");
			//The probing bar: 0.6 mm wide, 3 mm long from the gingival margin toward the root (up on the maxilla).
			RectangleF rectangleBar=GetBounds(dict["probing 3 MB"]);
			Assert.AreEqual(0.6f*tcData.ScaleMmToPix,rectangleBar.Width,1.01f,"0.6 mm wide, on DirectX's pixels.");
			Assert.AreEqual(3f*tcData.ScaleMmToPix,rectangleBar.Height,1.01f,"3 mm long.");
		}

		[TestMethod]
		public void PerioScene_Droplets_AreFlippedOnTheMandible() {
			ToothChartData tcData=CreatePerioData();
			AddMeasure(tcData,3,PerioSequenceType.BleedSupPlaqCalc,0,(int)BleedingFlags.Blood,0,0,0,0);
			AddMeasure(tcData,30,PerioSequenceType.BleedSupPlaqCalc,0,(int)BleedingFlags.Blood,0,0,0,0);
			ToothScene scene=ToothSceneBuilder.Build(tcData);
			foreach(string toothNum in new[] { "3","30" }) {
				PointF[] arrayPoints=scene.Items2D.Single(x => x.Source=="bleeding "+toothNum+" B").Polygons.SelectMany(x => x).ToArray();
				float yTop=arrayPoints.Min(x => x.Y);
				float yBottom=arrayPoints.Max(x => x.Y);
				//The droplet's point (0,0.89) is its tip: up on the maxilla, down on the mandible. Its widest part lies nearer the round end.
				float xMiddle=arrayPoints.Average(x => x.X);
				float widthNearTop=GetWidthAt(arrayPoints,yTop+(yBottom-yTop)*0.2f);
				float widthNearBottom=GetWidthAt(arrayPoints,yTop+(yBottom-yTop)*0.8f);
				if(toothNum=="3") {
					Assert.IsTrue(widthNearTop<widthNearBottom,"Maxilla: the tip up. "+widthNearTop+" "+widthNearBottom);
				}
				else {
					Assert.IsTrue(widthNearTop>widthNearBottom,"Mandible: flipped, the tip down. "+widthNearTop+" "+widthNearBottom);
				}
				Assert.IsTrue(xMiddle>0);
			}
		}

		[TestMethod]
		public void PerioScene_CentreLineAndNumbers_SitAtBaseY() {
			ToothChartData tcData=CreatePerioData();
			ToothScene scene=ToothSceneBuilder.Build(tcData);
			float baseY=ToothSceneBuilder.GetPerioBaseY(tcData);
			ToothSceneItem2D itemCentre=scene.Items2D.Single(x => x.Source==ToothSceneBuilder.SourcePerioCentreLine);
			Assert.AreEqual(Color.Black.ToArgb(),itemCentre.Color.ToArgb(),"Black, whatever the text colour.");
			RectangleF rectangle=GetBounds(itemCentre);
			PointF pointBaseY=scene.ProjectToImage(ChartMatrix.Identity,0,baseY,0);
			Assert.AreEqual(pointBaseY.Y,rectangle.Top+rectangle.Height/2f,1f,"The line is drawn at y=45 in the mandibular facial row's world, so at baseY.");
			Assert.AreEqual(0.5f*tcData.ScaleMmToPix,rectangle.Height,1f,"0.5 mm.");
			Assert.AreEqual(0,rectangle.Left,1f);
			Assert.AreEqual(scene.Size.Width,rectangle.Right,1f,"Across the chart.");
			//The numbers: the maxilla's above the centre line, the mandible's below it, in the text colour.
			ToothSceneItem2D item8=scene.Items2D.Single(x => x.Source=="number 8");
			ToothSceneItem2D item25=scene.Items2D.Single(x => x.Source=="number 25");
			Assert.IsTrue(item8.Origin.Y<rectangle.Top && item25.Origin.Y>rectangle.Bottom);
			Assert.AreEqual(Color.Black.ToArgb(),item8.Color.ToArgb());
		}

		[TestMethod]
		public void PerioScene_MobilityIsDrawnOnlyInTheFacialRows_CentredOnTheTooth() {
			ToothChartData tcData=CreatePerioData();
			tcData.ListToothGraphics["8"].Mobility="1";
			tcData.ListToothGraphics["8"].colorMobility=Color.Black;
			tcData.ListToothGraphics["25"].Mobility="3";
			tcData.ListToothGraphics["25"].colorMobility=Color.Red;
			ToothScene scene=ToothSceneBuilder.Build(tcData);
			Assert.AreEqual(1,scene.Items2D.Count(x => x.Source=="mobility of 8"),"Once: the maxillary facial row only.");
			Assert.AreEqual(1,scene.Items2D.Count(x => x.Source=="mobility of 25"),"Once: the mandibular facial row only.");
			float baseY=ToothSceneBuilder.GetPerioBaseY(tcData);
			ToothSceneItem2D item8=scene.Items2D.Single(x => x.Source=="mobility of 8");
			PointF pointRow=scene.ProjectToImage(ToothSceneBuilder.GetPerioRowOrientation(ToothSceneBuilder.PerioRow.MaxillaryFacial,baseY),tcData.GetTransX("8"),-2.5f,0);
			Assert.AreEqual(pointRow.Y,item8.Origin.Y+0.5f,1f,"2.5 mm below the maxillary facial row's line, on the crown.");
			Assert.AreEqual(pointRow.X,item8.Origin.X+item8.WidthPixels/2f,2f,"Centred by the numbers' measure (DirectX's width x 1.25).");
			Assert.AreEqual(Color.Red.ToArgb(),scene.Items2D.Single(x => x.Source=="mobility of 25").Color.ToArgb());
			Assert.IsFalse(scene.Items2D.Any(x => x.Source=="mobility of 9"),"No mobility, no item.");
		}

		[TestMethod]
		public void PerioLegend_NamesTheGingivalMarginLine_AsTheOfficeChose() {
			ToothChartData tcData=CreatePerioData();
			AddMeasure(tcData,3,PerioSequenceType.GingMargin,1,2,0,0,-1,1);
			Assert.AreEqual("Gingival Margin",ToothSceneBuilder.GetLegendGingivalMarginText(tcData),"The option off (the default): DirectX's text.");
			tcData.PerioUseRecessionTerm=true;
			Assert.AreEqual("Recession",ToothSceneBuilder.GetLegendGingivalMarginText(tcData),"The option on (Open Dental 24.4).");
			ToothScene scene=ToothSceneBuilder.Build(tcData);
			ToothSceneItem2D itemText=scene.Items2D.Single(x => x.Source==ToothSceneBuilder.SourceLegend+"text Recession");
			Assert.AreEqual("Recession",itemText.Text);
			Assert.IsFalse(scene.Items2D.Any(x => x.Kind==ChartItem2DKind.Text && x.Text=="Gingival Margin"));
			//The line keeps its colour; only the name changes.
			Assert.AreEqual(ChartScenes.PerioGM.ToArgb(),scene.Items2D.Single(x => x.Source==ToothSceneBuilder.SourceLegend+"gingival margin").Color.ToArgb());
			//Hyperplasia on any site of the exam (a gingival margin stored over 100) brings the old name back, as the Perio Chart's row label.
			AddMeasure(tcData,30,PerioSequenceType.GingMargin,0,0,0,0,102,0);
			Assert.AreEqual("Gingival Margin",ToothSceneBuilder.GetLegendGingivalMarginText(tcData));
			//An MGJ or probing value over 100 is not hyperplasia.
			tcData.ListPerioMeasure.RemoveAt(tcData.ListPerioMeasure.Count-1);
			AddMeasure(tcData,30,PerioSequenceType.MGJ,0,0,0,0,102,0);
			Assert.AreEqual("Recession",ToothSceneBuilder.GetLegendGingivalMarginText(tcData));
			//The same rule as the Perio Chart's label.
			Assert.AreEqual(PerioMeasures.TermRecession,PerioMeasures.GetGingMarginTerm(true,false));
			Assert.AreEqual(PerioMeasures.TermGingMarg,PerioMeasures.GetGingMarginTerm(true,true));
		}

		[TestMethod]
		public void PerioScenes_RecessionScenes_ShowTheirTerms() {
			Assert.IsTrue(BuildPerioScene("perio-recession-term").Items2D.Any(x => x.Kind==ChartItem2DKind.Text && x.Text=="Recession"));
			Assert.IsTrue(BuildPerioScene("perio-recession-term-hyperplasia").Items2D.Any(x => x.Kind==ChartItem2DKind.Text && x.Text=="Gingival Margin"));
			Assert.IsTrue(BuildPerioScene("perio-full-exam").Items2D.Any(x => x.Kind==ChartItem2DKind.Text && x.Text=="Gingival Margin"));
		}

		[TestMethod]
		public void PerioScene_LegendFollowsTheColoursAndTheFurcationRedLimit() {
			ToothScene scene=BuildPerioScene("perio-custom-colours");
			Dictionary<string,Color> dict=scene.Items2D.Where(x => x.Source.StartsWith(ToothSceneBuilder.SourceLegend) && x.Kind==ChartItem2DKind.Polygons)
				.GroupBy(x => x.Source).ToDictionary(x => x.Key.Substring(ToothSceneBuilder.SourceLegend.Length),x => x.First().Color);
			Assert.AreEqual(Color.DarkRed.ToArgb(),dict["bleeding"].ToArgb());
			Assert.AreEqual(Color.Gold.ToArgb(),dict["suppuration"].ToArgb());
			Assert.AreEqual(Color.Teal.ToArgb(),dict["probing"].ToArgb());
			Assert.AreEqual(Color.Magenta.ToArgb(),dict["probing red"].ToArgb());
			Assert.AreEqual(Color.SaddleBrown.ToArgb(),dict["gingival margin"].ToArgb());
			Assert.AreEqual(Color.Green.ToArgb(),dict["CAL"].ToArgb());
			Assert.AreEqual(Color.Navy.ToArgb(),dict["MGJ"].ToArgb());
			Assert.AreEqual(Color.Blue.ToArgb(),dict["furcation 1"].ToArgb(),"Grade 1 under the red limit 2: the furcation colour.");
			Assert.AreEqual(Color.Crimson.ToArgb(),dict["furcation 2"].ToArgb(),"From the red limit: the red one.");
			Assert.AreEqual(Color.Crimson.ToArgb(),dict["furcation 3"].ToArgb());
			Assert.IsTrue(scene.Items2D.Where(x => x.Source.StartsWith(ToothSceneBuilder.SourceLegend+"text ")).All(x => x.Color.ToArgb()==Color.Black.ToArgb()),
				"The legend's words are black.");
		}

		[TestMethod]
		public void PerioScene_LingualRowsAndTheirImplants_UseTheReversedWinding() {
			ToothScene scene=BuildPerioScene("perio-implants-both-arches");
			foreach(ToothSceneItem3D item in scene.Items3D) {
				bool isLingual=item.Source.Contains("lingual row");
				Assert.AreEqual(isLingual,item.IsReversed,item.ToString());
			}
			foreach(string toothNum in new[] { "3","14","19" }) {
				Assert.AreEqual(2,scene.Items3D.Where(x => x.Source.StartsWith("implant of "+toothNum+" ")).Select(x => x.Source).Distinct().Count(),
					"The implant of "+toothNum+" in its arch's two rows, the facial and the lingual.");
			}
			Assert.IsFalse(scene.Items3D.Any(x => x.Source.StartsWith("implant of 30 ")),"A skipped tooth with no implant draws none.");
		}

		[TestMethod]
		public void Wrapper_PerioMode_IsAllowedOnTheWpfChart_AndRefusedOnSimple2D() {
			StaRunner.Run(() => {
				using(ToothChartWrapper wrapper=WpfCharts.CreateWrapper(ChartScenes.SizePerio.Size)) {
					wrapper.PerioMode=true;
					Assert.IsTrue(wrapper.PerioMode);
					using(Bitmap bitmap=wrapper.GetBitmap()) {
						Assert.AreEqual(ChartScenes.SizePerio.Size,bitmap.Size);
					}
					Assert.AreEqual(ToothChartEngine.Wpf3D,wrapper.Engine);
					wrapper.PerioMode=false;
					wrapper.PerioUseRecessionTerm=true;
					Assert.IsTrue(wrapper.TcData.PerioUseRecessionTerm);
					wrapper.ResetTeeth();
					Assert.IsTrue(wrapper.PerioUseRecessionTerm,"An office setting: ResetTeeth keeps it, as it keeps the perio colours.");
				}
				using(ToothChartWrapper wrapper=new ToothChartWrapper()) {
					wrapper.Size=ChartScenes.SizePerio.Size;
					wrapper.DrawMode=DrawingMode.Simple2D;
					Exception ex=Expect.Throws<Exception>(() => wrapper.PerioMode=true);
					//Since TC-8 the graphical perio chart asks for the WPF chart on every computer (O-31): a 2D wrapper here is one whose WPF chart
					//could not start, and the message says so instead of naming DirectX.
					StringAssert.Contains(ex.Message,"The graphical perio chart needs the 3D tooth chart, which could not start on this computer.");
				}
			});
		}

		[TestMethod]
		public void Relay_ApplyGraphicalPerioMode_PassesTheRecessionOption() {
			//The source: the option is read from the preference, then (TC-8, O-31) the computer's "Draw in software" switch is set and the WPF chart
			//starts, on every computer: no DirectX engine and no developer switch any more.
			string code=SourceText.ReadCode(@"WpfControlsOD\UI\ToothChart\ToothChartRelay.cs");
			string method=SourceText.GetMethod(code,"public static void ApplyGraphicalPerioMode(");
			SourceText.AssertInOrder(method,"ApplyGraphicalPerioMode","toothChartWrapper.PerioUseRecessionTerm=PerioMeasures.GetUseRecessionTerm();",
				"toothChartWrapper.WpfDrawInSoftware=IsDrawnInSoftware(ComputerPrefs.LocalComputer);","toothChartWrapper.Engine=ToothChartEngine.Wpf3D;");
			StringAssert.DoesNotMatch(method,new System.Text.RegularExpressions.Regex(@"DrawMode|DirectX|IsWpfDeveloperSwitchOn"));
			//At run time with no database: the option reads off, and a wrapper that had it on gets it off.
			StaRunner.Run(() => {
				using(ToothChartWrapper wrapper=new ToothChartWrapper()) {
					wrapper.Size=ChartScenes.SizePerio.Size;
					wrapper.DrawMode=DrawingMode.Simple2D;
					wrapper.PerioUseRecessionTerm=true;
					ToothChartRelay relay=WpfCharts.CreateRelay(wrapper);
					Assert.IsTrue(relay.IsPerioRecessionTerm);
					relay.IsPerioRecessionTerm=false;
					Assert.IsFalse(wrapper.PerioUseRecessionTerm);
				}
			});
		}

		private static RectangleF GetBounds(ToothSceneItem2D item) {
			PointF[] arrayPoints=item.Polygons.SelectMany(x => x).ToArray();
			float left=arrayPoints.Min(x => x.X);
			float top=arrayPoints.Min(x => x.Y);
			return new RectangleF(left,top,arrayPoints.Max(x => x.X)-left,arrayPoints.Max(x => x.Y)-top);
		}

		///<summary>The width of the polygons' points within a quarter pixel of the row y (enough for a pentagon's edges drawn as a fan).</summary>
		private static float GetWidthAt(PointF[] arrayPoints,float y) {
			//The pentagon's outline: intersect each edge of the convex hull of the points with the row.
			List<PointF> listHull=GetHull(arrayPoints);
			List<float> listX=new List<float>();
			for(int i=0;i<listHull.Count;i++) {
				PointF p1=listHull[i];
				PointF p2=listHull[(i+1)%listHull.Count];
				if((p1.Y-y)*(p2.Y-y)>0 || p1.Y==p2.Y) {
					continue;
				}
				listX.Add(p1.X+(y-p1.Y)*(p2.X-p1.X)/(p2.Y-p1.Y));
			}
			return listX.Count<2?0:listX.Max()-listX.Min();
		}

		private static List<PointF> GetHull(PointF[] arrayPoints) {
			List<PointF> listSorted=arrayPoints.Distinct().OrderBy(p => p.X).ThenBy(p => p.Y).ToList();
			if(listSorted.Count<3) {
				return listSorted;
			}
			Func<PointF,PointF,PointF,double> cross=(o,a,b) => ((double)a.X-o.X)*((double)b.Y-o.Y)-((double)a.Y-o.Y)*((double)b.X-o.X);
			List<PointF> listLower=new List<PointF>();
			foreach(PointF p in listSorted) {
				while(listLower.Count>=2 && cross(listLower[listLower.Count-2],listLower[listLower.Count-1],p)<=0) {
					listLower.RemoveAt(listLower.Count-1);
				}
				listLower.Add(p);
			}
			List<PointF> listUpper=new List<PointF>();
			for(int i=listSorted.Count-1;i>=0;i--) {
				PointF p=listSorted[i];
				while(listUpper.Count>=2 && cross(listUpper[listUpper.Count-2],listUpper[listUpper.Count-1],p)<=0) {
					listUpper.RemoveAt(listUpper.Count-1);
				}
				listUpper.Add(p);
			}
			listLower.RemoveAt(listLower.Count-1);
			listUpper.RemoveAt(listUpper.Count-1);
			listLower.AddRange(listUpper);
			return listLower;
		}
	}
}
