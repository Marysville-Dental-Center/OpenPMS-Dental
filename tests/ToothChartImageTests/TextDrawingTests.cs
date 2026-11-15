// Package TC-5 (docs/specs/wpf-tooth-chart-plan.md, the TC-5 row): what the WPF chart's scene builder makes of free text, paint type Text other
// than "W" (O-22) and TextBelow (O-21), and the text mover (CursorTool.MoveText) of the shared mouse, the image host and the live view. No
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

		#region Paint text and TextBelow
		[TestMethod]
		public void PaintText_OtherThanW_IsCentredOnTheCrown_WStaysTheWatchMark() {
			StaRunner.Run(() => {
				foreach(ChartSize chartSize in new[] { ChartScenes.SizeModule,ChartScenes.SizeBigWindow }) {
					using ToothChartWrapper wrapper=CreateReset(chartSize.Size,out ToothChartRelay relay);
					relay.SetPrimary("20");
					relay.SetMissing("3");
					relay.MoveTooth("14",0,0,0,2,0,0);
					relay.SetText("3",Color.FromArgb(255,200,0,0),"Fx");
					relay.SetText("14",Color.Blue,"M");
					relay.SetText("30",Color.Green,"ABC");
					relay.SetText("19",Color.Red,"W");
					relay.SetText("K",Color.Purple,"P");
					relay.SetText("L",Color.Purple,"Q");
					relay.SetText("8",Color.Purple,"");
					ToothChartData tcData=wrapper.TcData;
					ToothScene scene=ToothSceneBuilder.Build(tcData);
					string name=chartSize.FolderName;
					Assert.AreEqual(2f,tcData.ListToothGraphics["14"].ShiftM);
					CheckCentredOnCrown(scene,tcData,"3",GetShiftedX(tcData,"3"),name);
					CheckCentredOnCrown(scene,tcData,"14",tcData.GetTransX("14")-2,name);//a left tooth: mesial is toward the midline
					CheckCentredOnCrown(scene,tcData,"30",GetShiftedX(tcData,"30"),name);
					CheckCentredOnCrown(scene,tcData,"K",GetShiftedX(tcData,"K"),name);
					ToothSceneItem2D item3=GetItem(scene,ToothSceneBuilder.SourcePaintText+"3");
					Assert.AreEqual(700,item3.Font.Weight,"Bold.");
					Assert.AreEqual((int)Math.Round(13*tcData.PixelScaleRatio),item3.Font.Height);
					Assert.AreEqual(Color.FromArgb(255,200,0,0).ToArgb(),item3.Color.ToArgb());
					Assert.IsTrue(item3.Origin.Y+item3.Font.Height<scene.Size.Height/2,"Maxillary: above the centre line.");
					Assert.IsTrue(GetItem(scene,ToothSceneBuilder.SourcePaintText+"30").Origin.Y>scene.Size.Height/2,"Mandibular: below it.");
					Assert.IsNull(GetItem(scene,ToothSceneBuilder.SourcePaintText+"19"),"\"W\" is the watch mark.");
					Assert.IsTrue(tcData.ListToothGraphics["19"].Watch);
					Assert.IsNotNull(scene.Items2D.FirstOrDefault(x => x.Source=="watch of 19"));
					Assert.IsNull(GetItem(scene,ToothSceneBuilder.SourcePaintText+"L"),"A primary tooth that is not shown has no text.");
					Assert.IsNull(GetItem(scene,ToothSceneBuilder.SourcePaintText+"8"),"Empty text draws nothing.");
					//After the watches, before the drawings and the numbers.
					int indexWatch=scene.Items2D.ToList().FindLastIndex(x => x.Source.StartsWith("watch of "));
					int indexNumber=scene.Items2D.ToList().FindIndex(x => x.Source.StartsWith("number "));
					Assert.IsTrue(scene.Items2D.IndexOf(item3)>indexWatch && scene.Items2D.IndexOf(item3)<indexNumber);
				}
			});
		}

		///<summary>The tooth's x in mm with its mesial shift (SetPrimary shifts some teeth to make room), as the watch marks place it.</summary>
		private static float GetShiftedX(ToothChartData tcData,string toothId) {
			ToothGraphic toothGraphic=tcData.ListToothGraphics[toothId];
			return tcData.GetTransX(toothId)+(ToothGraphic.IsRight(toothId) ? toothGraphic.ShiftM : -toothGraphic.ShiftM);
		}

		private static void CheckCentredOnCrown(ToothScene scene,ToothChartData tcData,string toothId,float xMm,string name) {
			ToothSceneItem2D item=GetItem(scene,ToothSceneBuilder.SourcePaintText+toothId);
			Assert.IsNotNull(item,"No paint text on "+toothId+" ("+name+").");
			float yMm=tcData.GetTransYfacial(toothId)+(ToothGraphic.IsMaxillary(toothId) ? -1 : 1)*ToothSceneBuilder.PaintTextCrownOffsetMm;
			scene.ProjectDirectX(ChartMatrix.Identity,xMm,yMm,0,out float xScreen,out float yScreen);
			Assert.AreEqual(xScreen,item.Origin.X+item.WidthPixels/2f,1.01,"Centred across on "+toothId+" ("+name+").");
			Assert.AreEqual(yScreen,item.Origin.Y+item.Font.Height/2f,1.01,"Centred on the crown of "+toothId+" ("+name+").");
		}

		[TestMethod]
		public void TextBelow_IsInTheBandAtTheEdge_CutToFitTheTooth_MovedInUnderAWatch() {
			StaRunner.Run(() => {
				foreach(ChartSize chartSize in new[] { ChartScenes.SizeModule,ChartScenes.SizePrintout,ChartScenes.SizeSheetWide }) {
					using ToothChartWrapper wrapper=CreateReset(chartSize.Size,out ToothChartRelay relay);
					relay.SetPrimary("4");
					relay.SetTextBelow("14",Color.Blue,"IMP");
					relay.SetTextBelow("19",Color.Green,"IMP");
					relay.SetTextBelow("3",Color.Red,"IMP");
					relay.SetText("3",Color.Red,"W");
					relay.SetTextBelow("30",Color.Red,"IMP");
					relay.SetText("30",Color.Red,"W");
					relay.SetTextBelow("25",Color.Red,"Implant planned");
					relay.SetTextBelow("A",Color.Red,"Watch");
					relay.SetTextBelow("S",Color.Red,"Watch");
					relay.SetTextBelow("1",Color.Red,"Bone graft");
					ToothChartData tcData=wrapper.TcData;
					ToothScene scene=ToothSceneBuilder.Build(tcData);
					string name=chartSize.FolderName;
					ChartFont fontBelow=ToothSceneBuilder.GetFontTextBelow(tcData.PixelScaleRatio);
					ChartFont fontWatch=ToothSceneBuilder.GetFontWatch(tcData.PixelScaleRatio);
					Assert.AreEqual(0,GetItem(scene,ToothSceneBuilder.SourceTextBelow+"14").Origin.Y,"Maxillary: the top edge ("+name+").");
					Assert.AreEqual(scene.Size.Height-fontBelow.Height,GetItem(scene,ToothSceneBuilder.SourceTextBelow+"19").Origin.Y,"Mandibular: the bottom edge.");
					Assert.AreEqual(fontWatch.Height,GetItem(scene,ToothSceneBuilder.SourceTextBelow+"3").Origin.Y,"Under the watch mark's line.");
					Assert.AreEqual(scene.Size.Height-fontBelow.Height-fontWatch.Height,GetItem(scene,ToothSceneBuilder.SourceTextBelow+"30").Origin.Y,
						"Above the watch mark's line.");
					float pixelsPerMm=scene.Size.Width/scene.WidthProjection;
					foreach(ToothSceneItem2D item in scene.Items2D.Where(x => x.Source.StartsWith(ToothSceneBuilder.SourceTextBelow))) {
						string toothId=item.Source.Substring(ToothSceneBuilder.SourceTextBelow.Length);
						string toothIdPerm=Tooth.IsPrimary(toothId) ? Tooth.PriToPerm(toothId) : toothId;
						int widthMax=(int)Math.Floor(ToothGraphic.GetWidth(toothIdPerm)*pixelsPerMm);
						Assert.IsTrue(item.WidthPixels<=widthMax || item.Text.Length==1,item.Source+" \""+item.Text+"\" is "+item.WidthPixels+" wide, the tooth "+widthMax+".");
						Assert.IsTrue(item.Origin.X>=0 && item.Origin.X+item.WidthPixels<=scene.Size.Width,item.Source+" stays inside the picture.");
						Assert.AreEqual(fontBelow.Height,item.Font.Height);
						Assert.AreEqual(400,item.Font.Weight);
						if(toothId!="1") {
							scene.ProjectDirectX(ChartMatrix.Identity,GetShiftedX(tcData,toothId),0,0,out float xScreen,out float yScreen);
							Assert.AreEqual(xScreen,item.Origin.X+item.WidthPixels/2f,1.01,"Centred on "+toothId+" ("+name+").");
						}
					}
					ToothSceneItem2D item25=GetItem(scene,ToothSceneBuilder.SourceTextBelow+"25");
					Assert.IsTrue(item25.Text.Length<"Implant planned".Length && "Implant planned".StartsWith(item25.Text),"Cut: \""+item25.Text+"\".");
					Assert.AreEqual("IMP",GetItem(scene,ToothSceneBuilder.SourceTextBelow+"14").Text);
					Assert.IsNotNull(GetItem(scene,ToothSceneBuilder.SourceTextBelow+"A"),"A shown primary tooth has its text.");
					Assert.IsNull(GetItem(scene,ToothSceneBuilder.SourceTextBelow+"S"),"A primary tooth that is not shown has none.");
				}
			});
		}

		[TestMethod]
		public void TextBelowFitted_DropsCharactersFromTheEnd_KeepsTheFirst() {
			ChartFont font=ToothSceneBuilder.GetFontTextBelow(1f);
			int widthIm=ChartFontMeasure.MeasureText(font,"IM").Width;
			Assert.AreEqual("IM",ToothSceneBuilder.GetTextBelowFitted(font,"IMPLANT",widthIm));
			Assert.AreEqual("I",ToothSceneBuilder.GetTextBelowFitted(font,"IMPLANT",1));
			Assert.AreEqual("IMPLANT",ToothSceneBuilder.GetTextBelowFitted(font,"IMPLANT",1000));
			Assert.AreEqual("",ToothSceneBuilder.GetTextBelowFitted(font,"",10));
			Assert.AreEqual("",ToothSceneBuilder.GetTextBelowFitted(font,null,10));
		}

		[TestMethod]
		public void Relay_PassesTheTextPaintTypes_ToTheWrapper() {
			StaRunner.Run(() => {
				using ToothChartWrapper wrapper=CreateReset(ChartScenes.SizeModule.Size,out ToothChartRelay relay);
				relay.SetText("5",Color.Red,"Rx");
				relay.SetTextBelow("12",Color.Blue,"IMP");
				relay.AddText("note",new PointF(5,6),Color.Green,9);
				Assert.AreEqual("Rx",wrapper.TcData.ListToothGraphics["5"].PaintText);
				Assert.AreEqual(Color.Red.ToArgb(),wrapper.TcData.ListToothGraphics["5"].colorPaintText.ToArgb());
				Assert.IsFalse(wrapper.TcData.ListToothGraphics["5"].Watch);
				Assert.AreEqual("IMP",wrapper.TcData.ListToothGraphics["12"].TextBelow);
				Assert.AreEqual(Color.Blue.ToArgb(),wrapper.TcData.ListToothGraphics["12"].colorTextBelow.ToArgb());
				Assert.AreEqual(1,wrapper.TcData.ListFreeTexts.Count);
				Assert.AreEqual(9,wrapper.TcData.ListFreeTexts[0].ToothInitialNum);
				relay.SetText("6",Color.Red,"W");
				Assert.IsTrue(wrapper.TcData.ListToothGraphics["6"].Watch);
				Assert.AreEqual("",wrapper.TcData.ListToothGraphics["6"].PaintText);
				relay.SetText("99",Color.Red,"X");//not a tooth: ignored, as every Set method ignores one
				relay.ResetTeeth();
				Assert.AreEqual("",wrapper.TcData.ListToothGraphics["5"].PaintText,"ResetTeeth clears the texts.");
				Assert.AreEqual("",wrapper.TcData.ListToothGraphics["12"].TextBelow);
			});
		}
		#endregion Paint text and TextBelow

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
	}
}
