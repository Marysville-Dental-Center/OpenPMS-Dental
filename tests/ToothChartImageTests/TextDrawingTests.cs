// Package TC-5 (docs/specs/wpf-tooth-chart-plan.md, the TC-5 row): what the WPF chart's scene builder makes of free text, paint type Text other
// than "W" (O-22, in the watch mark's band since the parity round; TextBelow is not in the picture), the text mover (CursorTool.MoveText) and Change
// Color on a label (CursorTool.ColorChanger) of the shared mouse, the image host and the live view. No
// database: the scenes are set through the relay as the program's callers set them.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental;
using OpenPmsDentalBusiness;
using SparksToothChart;
using SparksToothChart.Wpf3D;

namespace ToothChartImageTests {
	[TestClass]
	public class TextDrawingTests {
		///<summary>A Simple 2D wrapper of the size, reset as a refill resets it: its TcData is the chart's data in every engine, and the scene builder
		///needs nothing else.</summary>
		private static ToothChartWrapper CreateReset(Size size,out ToothChartRelay relay) {
			ToothChartWrapper wrapper=new ToothChartWrapper();
			wrapper.Size=size;
			relay=WpfCharts.CreateRelay(wrapper);
			relay.ResetTeeth();
			return wrapper;
		}

		private static ToothSceneItem2D GetItem(ToothScene scene,string source) {
			return scene.Items2D.FirstOrDefault(x => x.Source==source);
		}

		#region Free text
		[TestMethod]
		public void FreeText_IsDrawnLast_AtItsPointScaledAsTheDrawings_OpaqueInItsColour() {
			StaRunner.Run(() => {
				foreach(ChartSize chartSize in new[] { ChartScenes.SizeModule,ChartScenes.SizePrintout,ChartScenes.SizeBigWindow,ChartScenes.SizeSheetTall }) {
					using ToothChartWrapper wrapper=CreateReset(chartSize.Size,out ToothChartRelay relay);
					relay.AddText("Synthetic label",new PointF(100,50),Color.FromArgb(128,10,20,30),7);
					relay.AddDrawingSegment(new ToothInitial { InitialType=ToothInitialType.Drawing,DrawingSegment="100,50;120,50",ColorDraw=Color.Black });
					ToothChartData tcData=wrapper.TcData;
					ToothScene scene=ToothSceneBuilder.Build(tcData);
					ToothSceneItem2D item=scene.Items2D.Last();
					Assert.AreEqual(ToothSceneBuilder.SourceFreeText+"7",item.Source,"The label is drawn last, over the numbers and drawings ("+chartSize.FolderName+").");
					Assert.AreEqual(ChartItem2DKind.Text,item.Kind);
					Assert.AreEqual("Synthetic label",item.Text);
					Assert.AreEqual(Color.FromArgb(255,10,20,30).ToArgb(),item.Color.ToArgb(),"Opaque, as the drawings of the same colour.");
					Assert.AreEqual(ToothSceneBuilder.GetFontFreeText(tcData.PixelScaleRatio).Height,item.Font.Height);
					Assert.AreEqual((int)Math.Round(14*tcData.PixelScaleRatio),item.Font.Height,"Arial with a cell of 14 pixels times the ratio.");
					//The point goes where the drawings put the same point of the 410x307 space.
					float xExpected=tcData.RectTarget.X+100*tcData.PixelScaleRatio;
					float yExpected=tcData.RectTarget.Y+50*tcData.PixelScaleRatio;
					Assert.IsTrue(Math.Abs(item.Origin.X-xExpected)<=1.5 && Math.Abs(item.Origin.Y-yExpected)<=1.5,
						"Label at "+item.Origin+", the drawing space's 100,50 at "+xExpected+","+yExpected+" ("+chartSize.FolderName+").");
					ToothSceneItem2D itemDrawing=GetItem(scene,"drawing 0");
					Assert.IsNotNull(itemDrawing);
					PointF pointDrawingStart=itemDrawing.Polygons[0].OrderBy(x => x.X).First();
					Assert.IsTrue(Math.Abs(pointDrawingStart.X-item.Origin.X)<=2.5,"The drawing from 100,50 starts where the label does.");
					Assert.IsTrue(scene.Items2D.IndexOf(item)>scene.Items2D.ToList().FindLastIndex(x => x.Source.StartsWith("number ")));
					Assert.AreEqual(ChartFontMeasure.MeasureText(item.Font,item.Text).Width,item.WidthPixels);
				}
			});
		}

		[TestMethod]
		public void FreeText_PastAnEdge_IsDrawnInsideThePicture_AddedAgainReplaces_ResetClears() {
			StaRunner.Run(() => {
				using ToothChartWrapper wrapper=CreateReset(ChartScenes.SizeModule.Size,out ToothChartRelay relay);
				relay.AddText("corner",new PointF(409,306),Color.Black,1);
				relay.AddText("left",new PointF(0,0),Color.Black,2);
				relay.AddText("",new PointF(10,10),Color.Black,3);
				ToothScene scene=ToothSceneBuilder.Build(wrapper.TcData);
				ToothSceneItem2D itemCorner=GetItem(scene,ToothSceneBuilder.SourceFreeText+"1");
				Assert.AreEqual(410-itemCorner.WidthPixels,itemCorner.Origin.X,"Moved left until its right edge is the picture's.");
				Assert.AreEqual(307-itemCorner.Font.Height,itemCorner.Origin.Y,"Moved up until its bottom edge is the picture's.");
				Assert.AreEqual(new Point(0,0),GetItem(scene,ToothSceneBuilder.SourceFreeText+"2").Origin);
				Assert.IsNull(GetItem(scene,ToothSceneBuilder.SourceFreeText+"3"),"A label with no text draws nothing.");
				List<FreeTextBox> listBoxes=ToothSceneBuilder.GetFreeTextBoxes(wrapper.TcData);
				Assert.AreEqual(2,listBoxes.Count);
				FreeTextBox boxCorner=listBoxes[0];
				Assert.AreEqual(new Rectangle(itemCorner.Origin,new Size(itemCorner.WidthPixels,itemCorner.Font.Height)),boxCorner.Bounds);
				float ratio=wrapper.TcData.PixelScaleRatio;
				Assert.AreEqual(409-(409*ratio+wrapper.TcData.RectTarget.X-boxCorner.Bounds.X)/ratio,boxCorner.LocationShown.X,1.01f,
					"Where the label is seen, in the drawing space.");
				//The same row added again (a refill without a reset) replaces the label.
				relay.AddText("moved",new PointF(50,60),Color.Red,1);
				Assert.AreEqual(3,wrapper.TcData.ListFreeTexts.Count);
				Assert.AreEqual("moved",wrapper.TcData.ListFreeTexts[0].Text);
				Assert.AreEqual(new PointF(50,60),wrapper.TcData.ListFreeTexts[0].Location);
				//A copy of the data (prints) holds its own labels.
				ToothChartData tcDataCopy=wrapper.TcData.Copy();
				tcDataCopy.ListFreeTexts[0].Location=new PointF(1,1);
				Assert.AreEqual(new PointF(50,60),wrapper.TcData.ListFreeTexts[0].Location);
				relay.ResetTeeth();
				Assert.AreEqual(0,wrapper.TcData.ListFreeTexts.Count,"ResetTeeth clears the labels, as it clears the drawings.");
			});
		}
		#endregion Free text

		#region Paint text
		///<summary>The top of the watch mark's line, as AddWatches places the "W": 153 pixels times the ratio above the centre over a maxillary tooth,
		///138 below it under a mandibular one, in the projection's pixels, on a whole pixel.</summary>
		private static int GetWatchBandTop(ToothScene scene,ToothChartData tcData,bool isMaxillary) {
			float toMm=1f/tcData.ScaleMmToPix;
			float yMm=isMaxillary ? tcData.PixelScaleRatio*(153f*toMm) : tcData.PixelScaleRatio*(-138f*toMm);
			scene.ProjectDirectX(ChartMatrix.Identity,0,yMm,-6f,out float xScreen,out float yScreen);
			return (int)Math.Floor(yScreen);
		}

		[TestMethod]
		public void PaintText_OtherThanW_IsInTheWatchBand_NearerTheToothThanAW_WStaysTheWatchMark() {
			//The parity round of TC-5 (PT-2): as Open Dental draws it ("Fracture" above maxillary 9, "UCF" under mandibular 24 and 25 with the "W"
			//under it), not on the crown and not in bold.
			StaRunner.Run(() => {
				foreach(ChartSize chartSize in new[] { ChartScenes.SizeModule,ChartScenes.SizeBigWindow,ChartScenes.SizeSheetTall }) {
					using ToothChartWrapper wrapper=CreateReset(chartSize.Size,out ToothChartRelay relay);
					relay.SetPrimary("20");
					relay.SetMissing("3");
					relay.MoveTooth("14",0,0,0,2,0,0);
					relay.SetText("3",Color.FromArgb(255,200,0,0),"Fx");
					relay.SetText("14",Color.Blue,"M");
					relay.SetText("30",Color.Green,"ABC");
					relay.SetText("19",Color.Red,"W");
					relay.SetText("9",Color.DarkBlue,"UCF");
					relay.SetText("9",Color.DarkBlue,"W");
					relay.SetText("24",Color.DarkRed,"UCF");
					relay.SetText("24",Color.DarkRed,"W");
					relay.SetText("K",Color.Purple,"P");
					relay.SetText("L",Color.Purple,"Q");
					relay.SetText("8",Color.Purple,"");
					ToothChartData tcData=wrapper.TcData;
					ToothScene scene=ToothSceneBuilder.Build(tcData);
					string name=chartSize.FolderName;
					ChartFont fontWatch=ToothSceneBuilder.GetFontWatch(tcData.PixelScaleRatio);
					int yTop=GetWatchBandTop(scene,tcData,true);
					int yBottom=GetWatchBandTop(scene,tcData,false);
					Assert.AreEqual(2f,tcData.ListToothGraphics["14"].ShiftM);
					//Without a "W": on the watch mark's own line, above a maxillary root tip and below a mandibular one.
					CheckPaintText(scene,"3",GetShiftedX(tcData,"3"),yTop,name);
					CheckPaintText(scene,"14",tcData.GetTransX("14")-2,yTop,name);//a left tooth: mesial is toward the midline
					CheckPaintText(scene,"30",GetShiftedX(tcData,"30"),yBottom,name);
					CheckPaintText(scene,"K",GetShiftedX(tcData,"K"),yBottom,name);
					//With a "W": the text one line nearer the tooth, the "W" at the edge, on both arches.
					CheckPaintText(scene,"9",GetShiftedX(tcData,"9"),yTop+fontWatch.Height,name);
					CheckPaintText(scene,"24",GetShiftedX(tcData,"24"),yBottom-fontWatch.Height,name);
					Assert.AreEqual(yTop,GetItem(scene,"watch of 9").Origin.Y,"The W of 9 keeps the top edge ("+name+").");
					Assert.AreEqual(yBottom,GetItem(scene,"watch of 24").Origin.Y,"The W of 24 keeps the bottom edge ("+name+").");
					ToothSceneItem2D item3=GetItem(scene,ToothSceneBuilder.SourcePaintText+"3");
					Assert.AreEqual(400,item3.Font.Weight,"Regular weight, as Open Dental's.");
					Assert.AreEqual(fontWatch.Height,item3.Font.Height,"The watch mark's size.");
					Assert.AreEqual(Color.FromArgb(255,200,0,0).ToArgb(),item3.Color.ToArgb(),"The procedure's colour.");
					Assert.IsTrue(item3.Origin.Y+item3.Font.Height<=scene.Size.Height/4,"Maxillary: beyond the root tip, at the top ("+name+").");
					Assert.IsTrue(GetItem(scene,ToothSceneBuilder.SourcePaintText+"30").Origin.Y>=scene.Size.Height*3/4,"Mandibular: at the bottom ("+name+").");
					Assert.IsNull(GetItem(scene,ToothSceneBuilder.SourcePaintText+"19"),"\"W\" is the watch mark.");
					Assert.IsTrue(tcData.ListToothGraphics["19"].Watch);
					Assert.IsNotNull(GetItem(scene,"watch of 19"));
					Assert.IsNull(GetItem(scene,ToothSceneBuilder.SourcePaintText+"L"),"A primary tooth that is not shown has no text.");
					Assert.IsNull(GetItem(scene,ToothSceneBuilder.SourcePaintText+"8"),"Empty text draws nothing.");
					//After the watches, before the drawings and the numbers.
					int indexWatch=scene.Items2D.ToList().FindLastIndex(x => x.Source.StartsWith("watch of "));
					int indexNumber=scene.Items2D.ToList().FindIndex(x => x.Source.StartsWith("number "));
					Assert.IsTrue(scene.Items2D.IndexOf(item3)>indexWatch && scene.Items2D.IndexOf(item3)<indexNumber);
					Assert.IsFalse(scene.Items2D.Any(x => x.Source.StartsWith("text below ")),"TextBelow is not in the picture.");
				}
			});
		}

		///<summary>The tooth's x in mm with its mesial shift (SetPrimary shifts some teeth to make room), as the watch marks place it.</summary>
		private static float GetShiftedX(ToothChartData tcData,string toothId) {
			ToothGraphic toothGraphic=tcData.ListToothGraphics[toothId];
			return tcData.GetTransX(toothId)+(ToothGraphic.IsRight(toothId) ? toothGraphic.ShiftM : -toothGraphic.ShiftM);
		}

		private static void CheckPaintText(ToothScene scene,string toothId,float xMm,int yExpected,string name) {
			ToothSceneItem2D item=GetItem(scene,ToothSceneBuilder.SourcePaintText+toothId);
			Assert.IsNotNull(item,"No paint text on "+toothId+" ("+name+").");
			scene.ProjectDirectX(ChartMatrix.Identity,xMm,0,0,out float xScreen,out float yScreen);
			Assert.AreEqual(xScreen,item.Origin.X+item.WidthPixels/2f,1.01,"Centred on "+toothId+" at the watch mark's x ("+name+").");
			Assert.AreEqual(yExpected,item.Origin.Y,"The band line of "+toothId+" ("+name+").");
			Assert.AreEqual(ChartFontMeasure.MeasureText(item.Font,item.Text).Width,item.WidthPixels);
		}

		[TestMethod]
		public void PaintText_The2DChartDrawsItInItsWatchRows() {
			//The 2D chart's rows of its watch marks (RenderToothWatch: y 0 at the top, Height-Font.Size-8 at the bottom): text-other's paint text is
			//drawn there and nowhere else. Its pixels are pinned by Simple2DImageTests.
			StaRunner.Run(() => {
				Size size=ChartScenes.SizeModule.Size;
				using ToothChartWrapper wrapperEmpty=new ToothChartWrapper();
				wrapperEmpty.Size=size;
				using Bitmap bitmapEmpty=WpfCharts.Draw(WpfCharts.CreateRelay(wrapperEmpty),ChartScenes.Get("empty-universal"),ChartColorSet.Module);
				using ToothChartWrapper wrapper=new ToothChartWrapper();
				wrapper.Size=size;
				using Bitmap bitmap=WpfCharts.Draw(WpfCharts.CreateRelay(wrapper),ChartScenes.Get("text-other"),ChartColorSet.Module);
				Assert.AreEqual(DrawingMode.Simple2D,wrapper.DrawMode);
				int heightRow=wrapper.Font.Height+2;
				int yBottomRow=(int)(size.Height-wrapper.Font.Size-8f)-2;
				int countTop=0;
				int countBottom=0;
				for(int y=0;y<size.Height;y++) {
					for(int x=0;x<size.Width;x++) {
						if(bitmapEmpty.GetPixel(x,y).ToArgb()==bitmap.GetPixel(x,y).ToArgb()) {
							continue;
						}
						Assert.IsTrue(y<heightRow || y>=yBottomRow,"A changed pixel at "+x+","+y+" is outside the watch rows.");
						if(y<heightRow) {
							countTop++;
						}
						else {
							countBottom++;
						}
					}
				}
				Assert.IsTrue(countTop>10 && countBottom>10,"Text in both rows: "+countTop+" and "+countBottom+" pixels.");
			});
		}

		[TestMethod]
		public void TextSizes_The2DChartsFontIsFixed_TheWpfChartsScales() {
			//Review of TC-5, item 2 (kept as it is, documented in TC-5.md 1.1 and 1.2): the 2D chart draws its TC-5 text in the control's fixed 8.25 pt
			//font at every size, as its tooth numbers and "W"; the WPF chart scales its fonts by the pixel scale ratio. The measured sizes go to
			//%TEMP%\ToothChartImageTests\tc5-text-sizes.txt for the port note.
			List<string> listLines=new List<string>();
			List<int> listHeights2D=new List<int>();
			List<int> listHeightsWpf=new List<int>();
			StaRunner.Run(() => {
				foreach(ChartSize chartSize in new[] { ChartScenes.SizeModule,ChartScenes.SizeBigWindow }) {
					using ToothChartWrapper wrapper=CreateReset(chartSize.Size,out ToothChartRelay relay);
					float ratio=wrapper.TcData.PixelScaleRatio;
					using Bitmap bitmap=new Bitmap(10,10);
					using Graphics g=Graphics.FromImage(bitmap);
					g.TextRenderingHint=System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;//as ToothChart2D.OnPaint draws
					using StringFormat format=ToothChart2D.CreateTextFormat();
					Font font2D=wrapper.Font;
					ChartFont fontPaint=ToothSceneBuilder.GetFontPaintText(ratio);
					ChartFont fontFree=ToothSceneBuilder.GetFontFreeText(ratio);
					listHeights2D.Add(font2D.Height);
					listHeightsWpf.Add(fontPaint.Height);
					listLines.Add(chartSize.FolderName+" (ratio "+ratio.ToString("0.###",System.Globalization.CultureInfo.InvariantCulture)+"): 2D "
						+font2D.Name+" "+font2D.SizeInPoints.ToString(System.Globalization.CultureInfo.InvariantCulture)+" pt, cell "+font2D.Height
						+" px, \"UCF\" "+Math.Round(g.MeasureString("UCF",font2D,PointF.Empty,format).Width,1).ToString(System.Globalization.CultureInfo.InvariantCulture)
						+" px, \"Upper right\" "+Math.Round(g.MeasureString("Upper right",font2D,PointF.Empty,format).Width,1)
							.ToString(System.Globalization.CultureInfo.InvariantCulture)+" px; "
						+"WPF paint text cell "+fontPaint.Height+" px, \"UCF\" "+ChartFontMeasure.MeasureText(fontPaint,"UCF").Width+" px; "
						+"WPF free text cell "+fontFree.Height+" px, \"Upper right\" "+ChartFontMeasure.MeasureText(fontFree,"Upper right").Width+" px.");
				}
			});
			WpfCharts.WriteEvidence("tc5-text-sizes.txt",string.Join("\r\n",listLines));
			Assert.AreEqual(listHeights2D[0],listHeights2D[1],"The 2D chart's text does not scale.");
			Assert.IsTrue(listHeightsWpf[1]>listHeightsWpf[0],"The WPF chart's text scales with the chart.");
		}

		[TestMethod]
		public void Relay_PassesTheTextPaintType_ToTheWrapper() {
			StaRunner.Run(() => {
				using ToothChartWrapper wrapper=CreateReset(ChartScenes.SizeModule.Size,out ToothChartRelay relay);
				relay.SetText("5",Color.Red,"Rx");
				relay.AddText("note",new PointF(5,6),Color.Green,9);
				Assert.AreEqual("Rx",wrapper.TcData.ListToothGraphics["5"].PaintText);
				Assert.AreEqual(Color.Red.ToArgb(),wrapper.TcData.ListToothGraphics["5"].colorPaintText.ToArgb());
				Assert.IsFalse(wrapper.TcData.ListToothGraphics["5"].Watch);
				Assert.AreEqual(1,wrapper.TcData.ListFreeTexts.Count);
				Assert.AreEqual(9,wrapper.TcData.ListFreeTexts[0].ToothInitialNum);
				relay.SetText("6",Color.Red,"W");
				Assert.IsTrue(wrapper.TcData.ListToothGraphics["6"].Watch);
				Assert.AreEqual("",wrapper.TcData.ListToothGraphics["6"].PaintText);
				relay.SetText("99",Color.Red,"X");//not a tooth: ignored, as every Set method ignores one
				relay.ResetTeeth();
				Assert.AreEqual("",wrapper.TcData.ListToothGraphics["5"].PaintText,"ResetTeeth clears the texts.");
			});
		}
		#endregion Paint text

		#region The text mover
		private class MoveLog {
			public List<ToothChartTextMovedEventArgs> ListMoved=new List<ToothChartTextMovedEventArgs>();
			public int CountChartChanged;
		}

		private static ToothChartMouse CreateMouse(MoveLog moveLog) {
			ToothChartMouse toothChartMouse=new ToothChartMouse();
			toothChartMouse.ActionChartChanged=() => moveLog.CountChartChanged++;
			toothChartMouse.TextMoved+=(sender,e) => moveLog.ListMoved.Add(e);
			return toothChartMouse;
		}

		private static Point GetCentre(Rectangle rectangle) {
			return new Point(rectangle.X+rectangle.Width/2,rectangle.Y+rectangle.Height/2);
		}

		[TestMethod]
		public void MoveText_DragsTheLabelUnderTheMouse_AndRaisesTextMovedOnMouseUp() {
			StaRunner.Run(() => {
				foreach(Size size in new[] { new Size(410,307),new Size(615,461),new Size(926,858) }) {
					using ToothChartWrapper wrapper=CreateReset(size,out ToothChartRelay relay);
					relay.AddText("under",new PointF(100,50),Color.Black,41);
					relay.AddText("drag me",new PointF(100,50),Color.Black,42);//on top of 41
					ToothChartData tcData=wrapper.TcData;
					tcData.CursorTool=CursorTool.MoveText;
					MoveLog moveLog=new MoveLog();
					ToothChartMouse toothChartMouse=CreateMouse(moveLog);
					FreeTextBox box=ToothSceneBuilder.GetFreeTextBoxes(tcData)[1];
					Point pointDown=GetCentre(box.Bounds);
					toothChartMouse.MouseDown(tcData,pointDown,size);
					Assert.AreSame(tcData.ListFreeTexts[1],toothChartMouse.FreeTextMoving,"The top-most label under the mouse is picked up.");
					Point pointMove=new Point(pointDown.X+30,pointDown.Y+20);
					toothChartMouse.MouseMove(tcData,pointMove,size);
					Assert.AreEqual(1,moveLog.CountChartChanged,"A move draws the chart again.");
					Assert.AreEqual(0,moveLog.ListMoved.Count,"Nothing is saved before mouse up.");
					Rectangle boundsMoved=ToothSceneBuilder.GetFreeTextBoxes(tcData)[1].Bounds;
					Assert.IsTrue(Math.Abs(boundsMoved.X-box.Bounds.X-30)<=1 && Math.Abs(boundsMoved.Y-box.Bounds.Y-20)<=1,
						"The label follows the mouse: "+box.Bounds+" to "+boundsMoved+" ("+size+").");
					Point pointUp=new Point(pointDown.X+40,pointDown.Y+25);
					toothChartMouse.MouseUp(tcData,pointUp,size);
					Assert.AreEqual(1,moveLog.ListMoved.Count);
					Assert.AreEqual(42,moveLog.ListMoved[0].ToothInitialNum);
					float ratio=tcData.PixelScaleRatio;
					Assert.AreEqual(100+40/ratio,moveLog.ListMoved[0].Location.X,0.001,"The new top left in the 410x307 space ("+size+").");
					Assert.AreEqual(50+25/ratio,moveLog.ListMoved[0].Location.Y,0.001);
					Assert.AreEqual(new PointF(100,50),tcData.ListFreeTexts[0].Location,"The label under it stays.");
					Assert.IsNull(toothChartMouse.FreeTextMoving);
					//A press that is not on a label does nothing, and neither does a later move or release.
					toothChartMouse.MouseDown(tcData,new Point(2,size.Height/2),size);
					toothChartMouse.MouseMove(tcData,new Point(50,size.Height/2),size);
					toothChartMouse.MouseUp(tcData,new Point(50,size.Height/2),size);
					Assert.AreEqual(1,moveLog.ListMoved.Count);
					Assert.AreEqual(0,tcData.SelectedTeeth.Count,"The text mover selects no tooth.");
					//A click on a label without a move raises nothing and keeps its point.
					PointF pointBefore=tcData.ListFreeTexts[1].Location;
					Point pointClick=GetCentre(ToothSceneBuilder.GetFreeTextBoxes(tcData)[1].Bounds);
					toothChartMouse.MouseDown(tcData,pointClick,size);
					toothChartMouse.MouseUp(tcData,pointClick,size);
					Assert.AreEqual(1,moveLog.ListMoved.Count);
					Assert.AreEqual(pointBefore,tcData.ListFreeTexts[1].Location);
				}
			});
		}

		[TestMethod]
		public void MoveText_ALabelDrawnInsideTheEdge_IsDraggedFromWhereItIsSeen() {
			StaRunner.Run(() => {
				Size size=ChartScenes.SizeModule.Size;
				using ToothChartWrapper wrapper=CreateReset(size,out ToothChartRelay relay);
				relay.AddText("corner",new PointF(409,306),Color.Black,5);
				ToothChartData tcData=wrapper.TcData;
				tcData.CursorTool=CursorTool.MoveText;
				MoveLog moveLog=new MoveLog();
				ToothChartMouse toothChartMouse=CreateMouse(moveLog);
				FreeTextBox box=ToothSceneBuilder.GetFreeTextBoxes(tcData)[0];
				Point pointDown=GetCentre(box.Bounds);
				//A click keeps the stored point, past the edge as it is.
				toothChartMouse.MouseDown(tcData,pointDown,size);
				toothChartMouse.MouseUp(tcData,pointDown,size);
				Assert.AreEqual(new PointF(409,306),tcData.ListFreeTexts[0].Location);
				Assert.AreEqual(0,moveLog.ListMoved.Count);
				toothChartMouse.MouseDown(tcData,pointDown,size);
				toothChartMouse.MouseMove(tcData,new Point(pointDown.X-50,pointDown.Y-40),size);
				Rectangle boundsMoved=ToothSceneBuilder.GetFreeTextBoxes(tcData)[0].Bounds;
				Assert.IsTrue(Math.Abs(boundsMoved.X-(box.Bounds.X-50))<=1 && Math.Abs(boundsMoved.Y-(box.Bounds.Y-40))<=1,
					"The label moves at once with the mouse, from where it was seen: "+box.Bounds+" to "+boundsMoved);
				toothChartMouse.MouseUp(tcData,new Point(pointDown.X-50,pointDown.Y-40),size);
				Assert.AreEqual(1,moveLog.ListMoved.Count);
				Assert.AreEqual(box.LocationShown.X-50/tcData.PixelScaleRatio,moveLog.ListMoved[0].Location.X,0.001);
				Assert.AreEqual(box.LocationShown.Y-40/tcData.PixelScaleRatio,moveLog.ListMoved[0].Location.Y,0.001);
			});
		}

		[TestMethod]
		public void MoveText_APressWithNoMoveAndNoUp_LeavesTheLabelsPoint() {
			//A press that never reaches MouseUp (the capture lost, a dialog, Alt-Tab) must not leave the label at the point it is drawn at inside the
			//edge: the chart's data keeps the stored point (review of TC-5, item 4).
			StaRunner.Run(() => {
				Size size=ChartScenes.SizeModule.Size;
				using ToothChartWrapper wrapper=CreateReset(size,out ToothChartRelay relay);
				relay.AddText("corner",new PointF(409,306),Color.Black,5);
				ToothChartData tcData=wrapper.TcData;
				tcData.CursorTool=CursorTool.MoveText;
				MoveLog moveLog=new MoveLog();
				ToothChartMouse toothChartMouse=CreateMouse(moveLog);
				FreeTextBox box=ToothSceneBuilder.GetFreeTextBoxes(tcData)[0];
				Assert.AreNotEqual(new PointF(409,306),box.LocationShown,"The label is drawn inside the edge.");
				toothChartMouse.MouseDown(tcData,GetCentre(box.Bounds),size);
				Assert.AreSame(tcData.ListFreeTexts[0],toothChartMouse.FreeTextMoving);
				Assert.AreEqual(new PointF(409,306),tcData.ListFreeTexts[0].Location,"No move and no up: the stored point.");
				Assert.AreEqual(0,moveLog.ListMoved.Count);
			});
		}

		[TestMethod]
		public void MoveText_OtherToolsLeaveTheLabels() {
			StaRunner.Run(() => {
				Size size=ChartScenes.SizeModule.Size;
				using ToothChartWrapper wrapper=CreateReset(size,out ToothChartRelay relay);
				relay.AddText("label",new PointF(100,50),Color.Black,5);
				ToothChartData tcData=wrapper.TcData;
				Point point=GetCentre(ToothSceneBuilder.GetFreeTextBoxes(tcData)[0].Bounds);
				foreach(CursorTool cursorTool in new[] { CursorTool.Pointer,CursorTool.Pen,CursorTool.Eraser,CursorTool.ColorChanger }) {
					tcData.CursorTool=cursorTool;
					MoveLog moveLog=new MoveLog();
					ToothChartMouse toothChartMouse=CreateMouse(moveLog);
					toothChartMouse.SegmentDrawn+=(sender,e) => { };
					toothChartMouse.MouseDown(tcData,point,size);
					toothChartMouse.MouseMove(tcData,new Point(point.X+30,point.Y+10),size);
					toothChartMouse.MouseUp(tcData,new Point(point.X+30,point.Y+10),size);
					Assert.AreEqual(0,moveLog.ListMoved.Count,cursorTool.ToString());
					Assert.AreEqual(new PointF(100,50),tcData.ListFreeTexts[0].Location,cursorTool.ToString());
					tcData.PointList.Clear();
					tcData.SelectedTeeth.Clear();
				}
			});
		}

		[TestMethod]
		public void ImageHost_MovesALabel_AndTheWrapperAndRelayRaiseTextMoved() {
			StaRunner.Run(() => {
				using ToothChartWrapper wrapper=WpfCharts.CreateWrapper(ChartScenes.SizeModule.Size);
				ToothChartRelay relay=WpfCharts.CreateRelay(wrapper);
				ChartScenes.Prepare(relay,ChartScenes.Get("text-label-moved"),ChartColorSet.Module);
				relay.CursorTool=CursorTool.MoveText;
				List<ToothChartTextMovedEventArgs> listWrapper=new List<ToothChartTextMovedEventArgs>();
				List<ToothChartTextMovedEventArgs> listRelay=new List<ToothChartTextMovedEventArgs>();
				wrapper.TextMoved+=(sender,e) => listWrapper.Add(e);
				relay.TextMoved+=(sender,e) => listRelay.Add(e);
				ToothChartImageHost toothChartImageHost=wrapper.ImageHostWpf;
				toothChartImageHost.Size=wrapper.Size;
				using(Bitmap bitmap=relay.GetBitmap()) {
					Assert.IsFalse(wrapper.IsWpfSceneDirty);
				}
				Point point=GetCentre(ToothSceneBuilder.GetFreeTextBoxes(wrapper.TcData)[0].Bounds);
				ToothChartMouseTests.RaiseMouse(toothChartImageHost,"OnMouseDown",point);
				ToothChartMouseTests.RaiseMouse(toothChartImageHost,"OnMouseMove",new Point(point.X-20,point.Y+7));
				Assert.AreEqual(new PointF(230,187),wrapper.TcData.ListFreeTexts[0].Location,"The label follows the mouse at once.");
				ToothChartMouseTests.RaiseMouse(toothChartImageHost,"OnMouseUp",new Point(point.X-20,point.Y+7));
				Assert.AreEqual(1,listWrapper.Count);
				Assert.AreEqual(1,listRelay.Count);
				Assert.AreEqual(11,listRelay[0].ToothInitialNum);
				Assert.AreEqual(250-20f,listRelay[0].Location.X,0.001);
				Assert.AreEqual(180+7f,listRelay[0].Location.Y,0.001);
				using(Bitmap bitmap=relay.GetBitmap()) {
					ToothSceneItem2D item=WpfCharts.BuildScene(wrapper).Items2D.Last();
					Assert.AreEqual(ToothSceneBuilder.SourceFreeText+"11",item.Source);
					Assert.AreEqual(new Point(230,187),item.Origin,"Drawn where it was dropped (the module size is the drawing space's own: one pixel each).");
				}
			});
		}

		[TestMethod]
		public void LiveView_MovesALabel_AndTheRelayRaisesTextMoved() {
			if(!Environment.UserInteractive) {
				Assert.Inconclusive("Needs an interactive desktop session: the wrapper must be shown.");
			}
			StaRunner.Run(() => {
				using Form form=WpfCharts.CreateOffscreenForm(ChartScenes.SizeModule.Size);
				ToothChartWrapper wrapper=WpfCharts.CreateWrapper(ChartScenes.SizeModule.Size);
				form.Controls.Add(wrapper);
				ToothChartRelay relay=WpfCharts.CreateRelay(wrapper);
				form.Show();
				ToothChartWpfView toothChartWpfView=WpfCharts.WaitForLiveView(wrapper);
				Assert.IsNotNull(toothChartWpfView,"The live view did not start on a shown wrapper.");
				ChartScenes.Prepare(relay,ChartScenes.Get("text-label-moved"),ChartColorSet.Module);
				relay.CursorTool=CursorTool.MoveText;
				List<ToothChartTextMovedEventArgs> listRelay=new List<ToothChartTextMovedEventArgs>();
				relay.TextMoved+=(sender,e) => listRelay.Add(e);
				Assert.IsTrue(WpfRendererTests.PumpUntil(() => toothChartWpfView.SceneShown!=null && !wrapper.IsWpfSceneDirty,20000));
				Point point=GetCentre(ToothSceneBuilder.GetFreeTextBoxes(wrapper.TcData)[0].Bounds);
				toothChartWpfView.MouseDownForTests(point);
				toothChartWpfView.MouseMoveForTests(new Point(point.X+15,point.Y-9));
				Assert.IsTrue(WpfRendererTests.PumpUntil(() => !wrapper.IsWpfSceneDirty && toothChartWpfView.SceneShown.Items2D.Last().Origin.X
					==ToothSceneBuilder.GetFreeTextBoxes(wrapper.TcData)[0].Bounds.X,20000),"The live view shows the label where the mouse took it.");
				toothChartWpfView.MouseUpForTests(new Point(point.X+15,point.Y-9));
				Assert.AreEqual(1,listRelay.Count);
				Assert.AreEqual(11,listRelay[0].ToothInitialNum);
				Assert.AreEqual(250+15f,listRelay[0].Location.X,0.001);
				Assert.AreEqual(180-9f,listRelay[0].Location.Y,0.001);
				wrapper.Dispose();
				form.Close();
			});
		}
		#endregion The text mover

		#region Change Color on free text
		[TestMethod]
		public void ColorChanger_RecoloursTheTopMostLabel_WhenNoDrawingIsNear_AndRaisesTextColorChanged() {
			//Open Dental's manual: Change Color is "to change the color of an existing drawing or text" (the parity round of TC-5).
			StaRunner.Run(() => {
				foreach(Size size in new[] { new Size(410,307),new Size(615,461),new Size(926,858) }) {
					using ToothChartWrapper wrapper=CreateReset(size,out ToothChartRelay relay);
					relay.AddText("under",new PointF(100,50),Color.Black,41);
					relay.AddText("recolour me",new PointF(100,50),Color.Black,42);//on top of 41
					relay.AddText("far",new PointF(300,250),Color.Black,43);
					ToothChartData tcData=wrapper.TcData;
					tcData.CursorTool=CursorTool.ColorChanger;
					tcData.ColorDrawing=Color.Green;
					MoveLog moveLog=new MoveLog();
					ToothChartMouse toothChartMouse=CreateMouse(moveLog);
					List<ToothChartTextColorChangedEventArgs> listChanged=new List<ToothChartTextColorChangedEventArgs>();
					List<string> listSegments=new List<string>();
					toothChartMouse.TextColorChanged+=(sender,e) => listChanged.Add(e);
					toothChartMouse.SegmentDrawn+=(sender,e) => listSegments.Add(e.DrawingSegement);
					Point point=GetCentre(ToothSceneBuilder.GetFreeTextBoxes(tcData)[1].Bounds);
					toothChartMouse.MouseDown(tcData,point,size);
					toothChartMouse.MouseUp(tcData,point,size);
					Assert.AreEqual(1,listChanged.Count,size.ToString());
					Assert.AreEqual(42,listChanged[0].ToothInitialNum,"The top-most label under the mouse.");
					Assert.AreEqual(Color.Green.ToArgb(),listChanged[0].ColorText.ToArgb(),"The pen colour.");
					Assert.AreEqual(Color.Green.ToArgb(),tcData.ListFreeTexts[1].ColorText.ToArgb(),"Drawn in the new colour at once.");
					Assert.AreEqual(Color.Black.ToArgb(),tcData.ListFreeTexts[0].ColorText.ToArgb(),"The label under it keeps its colour.");
					Assert.AreEqual(Color.Black.ToArgb(),tcData.ListFreeTexts[2].ColorText.ToArgb());
					Assert.AreEqual(1,moveLog.CountChartChanged,"The chart is drawn again.");
					Assert.AreEqual(0,moveLog.ListMoved.Count);
					Assert.AreEqual(new PointF(100,50),tcData.ListFreeTexts[1].Location,"Change Color does not move the label.");
					Assert.AreEqual(0,listSegments.Count);
					//A press on no label and no drawing does nothing.
					toothChartMouse.MouseDown(tcData,new Point(2,size.Height/2),size);
					toothChartMouse.MouseUp(tcData,new Point(2,size.Height/2),size);
					Assert.AreEqual(1,listChanged.Count);
				}
			});
		}

		[TestMethod]
		public void ColorChanger_ADrawingNearThePress_IsRecolouredFirst_TheLabelKeepsItsColour() {
			//The drawings are looked for first, as DirectX's colour changer does; only a press with no drawing near reaches the labels.
			StaRunner.Run(() => {
				Size size=ChartScenes.SizeModule.Size;
				using ToothChartWrapper wrapper=CreateReset(size,out ToothChartRelay relay);
				relay.AddText("label",new PointF(100,50),Color.Black,5);
				ToothChartData tcData=wrapper.TcData;
				Point point=GetCentre(ToothSceneBuilder.GetFreeTextBoxes(tcData)[0].Bounds);
				PointF pointDrawing=tcData.GetPointMouseScaled(point.X,point.Y,size);
				string segment=pointDrawing.X.ToString(System.Globalization.CultureInfo.CurrentCulture)+","+pointDrawing.Y.ToString(System.Globalization.CultureInfo.CurrentCulture)
					+";"+(pointDrawing.X+1).ToString(System.Globalization.CultureInfo.CurrentCulture)+","+pointDrawing.Y.ToString(System.Globalization.CultureInfo.CurrentCulture);
				relay.AddDrawingSegment(new ToothInitial { InitialType=ToothInitialType.Drawing,DrawingSegment=segment,ColorDraw=Color.Black });
				tcData.CursorTool=CursorTool.ColorChanger;
				tcData.ColorDrawing=Color.Red;
				MoveLog moveLog=new MoveLog();
				ToothChartMouse toothChartMouse=CreateMouse(moveLog);
				List<ToothChartTextColorChangedEventArgs> listChanged=new List<ToothChartTextColorChangedEventArgs>();
				List<string> listSegments=new List<string>();
				toothChartMouse.TextColorChanged+=(sender,e) => listChanged.Add(e);
				toothChartMouse.SegmentDrawn+=(sender,e) => listSegments.Add(e.DrawingSegement);
				toothChartMouse.MouseDown(tcData,point,size);
				toothChartMouse.MouseUp(tcData,point,size);
				Assert.AreEqual(1,listSegments.Count,"The drawing is recoloured.");
				Assert.AreEqual(Color.Red.ToArgb(),tcData.DrawingSegmentList[0].ColorDraw.ToArgb());
				Assert.AreEqual(0,listChanged.Count,"The label is not.");
				Assert.AreEqual(Color.Black.ToArgb(),tcData.ListFreeTexts[0].ColorText.ToArgb());
			});
		}

		[TestMethod]
		public void ImageHost_RecoloursALabel_AndTheWrapperAndRelayRaiseTextColorChanged() {
			StaRunner.Run(() => {
				using ToothChartWrapper wrapper=WpfCharts.CreateWrapper(ChartScenes.SizeModule.Size);
				ToothChartRelay relay=WpfCharts.CreateRelay(wrapper);
				ChartScenes.Prepare(relay,ChartScenes.Get("text-label-moved"),ChartColorSet.Module);
				relay.CursorTool=CursorTool.ColorChanger;
				relay.ColorDrawing=Color.DarkOrange;
				List<ToothChartTextColorChangedEventArgs> listWrapper=new List<ToothChartTextColorChangedEventArgs>();
				List<ToothChartTextColorChangedEventArgs> listRelay=new List<ToothChartTextColorChangedEventArgs>();
				wrapper.TextColorChanged+=(sender,e) => listWrapper.Add(e);
				relay.TextColorChanged+=(sender,e) => listRelay.Add(e);
				ToothChartImageHost toothChartImageHost=wrapper.ImageHostWpf;
				toothChartImageHost.Size=wrapper.Size;
				using(Bitmap bitmap=relay.GetBitmap()) {
					Assert.IsFalse(wrapper.IsWpfSceneDirty);
				}
				Point point=GetCentre(ToothSceneBuilder.GetFreeTextBoxes(wrapper.TcData)[0].Bounds);
				ToothChartMouseTests.RaiseMouse(toothChartImageHost,"OnMouseDown",point);
				ToothChartMouseTests.RaiseMouse(toothChartImageHost,"OnMouseUp",point);
				Assert.AreEqual(1,listWrapper.Count);
				Assert.AreEqual(1,listRelay.Count);
				Assert.AreEqual(11,listRelay[0].ToothInitialNum);
				Assert.AreEqual(Color.DarkOrange.ToArgb(),listRelay[0].ColorText.ToArgb());
				using(Bitmap bitmap=relay.GetBitmap()) {
					ToothSceneItem2D item=WpfCharts.BuildScene(wrapper).Items2D.Last();
					Assert.AreEqual(ToothSceneBuilder.SourceFreeText+"11",item.Source);
					Assert.AreEqual(Color.DarkOrange.ToArgb(),item.Color.ToArgb(),"Drawn in the new colour.");
				}
			});
		}

		[TestMethod]
		public void LiveView_RecoloursALabel_AndTheRelayRaisesTextColorChanged() {
			if(!Environment.UserInteractive) {
				Assert.Inconclusive("Needs an interactive desktop session: the wrapper must be shown.");
			}
			StaRunner.Run(() => {
				using Form form=WpfCharts.CreateOffscreenForm(ChartScenes.SizeModule.Size);
				ToothChartWrapper wrapper=WpfCharts.CreateWrapper(ChartScenes.SizeModule.Size);
				form.Controls.Add(wrapper);
				ToothChartRelay relay=WpfCharts.CreateRelay(wrapper);
				form.Show();
				ToothChartWpfView toothChartWpfView=WpfCharts.WaitForLiveView(wrapper);
				Assert.IsNotNull(toothChartWpfView,"The live view did not start on a shown wrapper.");
				ChartScenes.Prepare(relay,ChartScenes.Get("text-label-moved"),ChartColorSet.Module);
				relay.CursorTool=CursorTool.ColorChanger;
				relay.ColorDrawing=Color.DarkOrange;
				List<ToothChartTextColorChangedEventArgs> listRelay=new List<ToothChartTextColorChangedEventArgs>();
				relay.TextColorChanged+=(sender,e) => listRelay.Add(e);
				Assert.IsTrue(WpfRendererTests.PumpUntil(() => toothChartWpfView.SceneShown!=null && !wrapper.IsWpfSceneDirty,20000));
				Point point=GetCentre(ToothSceneBuilder.GetFreeTextBoxes(wrapper.TcData)[0].Bounds);
				toothChartWpfView.MouseDownForTests(point);
				toothChartWpfView.MouseUpForTests(point);
				Assert.AreEqual(1,listRelay.Count);
				Assert.AreEqual(11,listRelay[0].ToothInitialNum);
				Assert.AreEqual(Color.DarkOrange.ToArgb(),listRelay[0].ColorText.ToArgb());
				Assert.IsTrue(WpfRendererTests.PumpUntil(() => !wrapper.IsWpfSceneDirty && toothChartWpfView.SceneShown.Items2D.Last().Color.ToArgb()
					==Color.DarkOrange.ToArgb(),20000),"The live view shows the label in its new colour.");
				wrapper.Dispose();
				form.Close();
			});
		}
		#endregion Change Color on free text
	}
}
