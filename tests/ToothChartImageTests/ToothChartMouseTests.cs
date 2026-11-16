// Mouse tests of the WPF chart (package TC-3, docs/specs/wpf-tooth-chart-plan.md section 7 item 7): the shared ToothChartMouse with synthesised
// input gives the selection, the drag fill-in and the SegmentDrawn strings that ToothChartData's own functions give for the same points, and the
// DirectX chart's answers to the same input, recorded before TC-9a retired that chart (Baselines\legacy-numbers\directx-mouse.txt).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental;
using OpenPmsDentalBusiness;
using SparksToothChart;
using SparksToothChart.Wpf3D;

namespace ToothChartImageTests {
	[TestClass]
	public class ToothChartMouseTests {
		///<summary>What a mouse raised and asked for.</summary>
		private class MouseLog {
			public List<string> ListSegments=new List<string>();
			public int CountSelectionsChanged;
			public int CountChartChanged;
			public int CountStrokeChanged;
		}

		private static ToothChartMouse CreateMouse(MouseLog mouseLog) {
			ToothChartMouse toothChartMouse=new ToothChartMouse();
			toothChartMouse.ActionChartChanged=() => mouseLog.CountChartChanged++;
			toothChartMouse.ActionPenStrokeChanged=() => mouseLog.CountStrokeChanged++;
			toothChartMouse.SegmentDrawn+=(sender,e) => mouseLog.ListSegments.Add(e.DrawingSegement);
			toothChartMouse.ToothSelectionsChanged+=sender => mouseLog.CountSelectionsChanged++;
			return toothChartMouse;
		}

		///<summary>A 2D wrapper of the size with the scene applied through the relay: its TcData is the chart's data in every engine.</summary>
		private static ToothChartWrapper CreatePrepared(string sceneName,ChartSize chartSize) {
			ToothChartWrapper wrapper=new ToothChartWrapper();
			wrapper.Size=chartSize.Size;
			ChartScenes.Prepare(WpfCharts.CreateRelay(wrapper),ChartScenes.Get(sceneName),chartSize.ColorSet);
			return wrapper;
		}

		[TestMethod]
		public void Pointer_Click_TogglesTheToothGetToothAtPointGives_AndCommitsOnMouseUp() {
			StaRunner.Run(() => {
				foreach(ChartSize chartSize in new[] { ChartScenes.SizeModule,ChartScenes.SizePrintout,ChartScenes.SizeSheetTall }) {
					using ToothChartWrapper wrapper=CreatePrepared("select-primary-letter",chartSize);
					ToothChartData tcData=wrapper.TcData;
					int countPoints=0;
					for(int y=0;y<chartSize.Size.Height;y+=7) {
						for(int x=0;x<chartSize.Size.Width;x+=9) {
							tcData.SelectedTeeth.Clear();
							MouseLog mouseLog=new MouseLog();
							ToothChartMouse toothChartMouse=CreateMouse(mouseLog);
							Point point=new Point(x,y);
							string toothExpected=tcData.GetToothAtPoint(point);
							toothChartMouse.MouseDown(tcData,point,chartSize.Size);
							CollectionAssert.AreEqual(new[] { toothExpected },tcData.SelectedTeeth,"Down at "+point+" ("+chartSize.FolderName+")");
							Assert.AreEqual(1,mouseLog.CountChartChanged,"The chart is drawn again on mouse down.");
							Assert.AreEqual(0,mouseLog.CountSelectionsChanged,"Not committed before mouse up.");
							toothChartMouse.MouseUp(tcData,point,chartSize.Size);
							Assert.AreEqual(1,mouseLog.CountSelectionsChanged,"Committed on mouse up.");
							//A second click on the same tooth takes it away again, and that is a change too.
							toothChartMouse.MouseDown(tcData,point,chartSize.Size);
							toothChartMouse.MouseUp(tcData,point,chartSize.Size);
							Assert.AreEqual(0,tcData.SelectedTeeth.Count);
							Assert.AreEqual(2,mouseLog.CountSelectionsChanged);
							countPoints++;
						}
					}
					Assert.IsTrue(countPoints>1000,"Points: "+countPoints);
				}
			});
		}

		[TestMethod]
		public void Pointer_HoverAndAnUnchangedMouseUp_ChangeAndCommitNothing() {
			StaRunner.Run(() => {
				using ToothChartWrapper wrapper=CreatePrepared("empty-universal",ChartScenes.SizeModule);
				ToothChartData tcData=wrapper.TcData;
				MouseLog mouseLog=new MouseLog();
				ToothChartMouse toothChartMouse=CreateMouse(mouseLog);
				//No visible hover, as today: moving over the teeth with no button changes nothing and asks for no image.
				foreach(string toothId in new[] { "3","8","14","19","30" }) {
					toothChartMouse.MouseMove(tcData,GetNumberCentre(tcData,toothId),ChartScenes.SizeModule.Size);
				}
				Assert.AreEqual(0,tcData.SelectedTeeth.Count);
				Assert.AreEqual(0,mouseLog.CountChartChanged);
				//A mouse up whose selection is the one the last mouse down saw commits nothing.
				toothChartMouse.MouseUp(tcData,GetNumberCentre(tcData,"8"),ChartScenes.SizeModule.Size);
				Assert.AreEqual(0,mouseLog.CountSelectionsChanged);
				Assert.IsFalse(toothChartMouse.IsMouseDown);
			});
		}

		[TestMethod]
		public void Pointer_Drag_FillsInTheSkippedTeeth_AsGetAffectedTeethGives() {
			StaRunner.Run(() => {
				//Across the maxillary numbers from 3 to 12 in steps that skip teeth: every tooth from 3 to 12 ends up selected.
				using(ToothChartWrapper wrapper=CreatePrepared("empty-universal",ChartScenes.SizeModule)) {
					ToothChartData tcData=wrapper.TcData;
					List<Point> listPoints=GetDragPoints(GetNumberCentre(tcData,"3"),GetNumberCentre(tcData,"12"),5);
					MouseLog mouseLog=new MouseLog();
					List<string> listExpected=ReplayDirectXDrag(tcData,listPoints);
					DriveDrag(CreateMouse(mouseLog),tcData,listPoints,ChartScenes.SizeModule.Size);
					CollectionAssert.AreEquivalent(listExpected,tcData.SelectedTeeth,"As DirectX's MouseMove with ToothChartData's functions.");
					CollectionAssert.AreEquivalent(Enumerable.Range(3,10).Select(x => x.ToString()).ToList(),tcData.SelectedTeeth,"3 to 12, the skipped ones filled in.");
					Assert.AreEqual(1,mouseLog.CountSelectionsChanged);
				}
				//The primary band of a mixed dentition: a drag over the letters takes the primary teeth.
				using(ToothChartWrapper wrapper=CreatePrepared("primary-dentition",ChartScenes.SizeModule)) {
					ToothChartData tcData=wrapper.TcData;
					List<Point> listPoints=GetDragPoints(GetNumberCentre(tcData,"A"),GetNumberCentre(tcData,"J"),4);
					List<string> listExpected=ReplayDirectXDrag(tcData,listPoints);
					DriveDrag(CreateMouse(new MouseLog()),tcData,listPoints,ChartScenes.SizeModule.Size);
					CollectionAssert.AreEquivalent(listExpected,tcData.SelectedTeeth);
					CollectionAssert.AreEquivalent(new[] { "A","B","C","D","E","F","G","H","I","J" },tcData.SelectedTeeth,"A to J.");
				}
			});
		}

		[TestMethod]
		public void Pen_Stroke_GivesTheSegmentDrawnStringOfGetPointMouseScaled() {
			StaRunner.Run(() => {
				CultureInfo cultureOld=Thread.CurrentThread.CurrentCulture;
				try {
					Thread.CurrentThread.CurrentCulture=new CultureInfo("en-US");
					foreach(ChartSize chartSize in new[] { ChartScenes.SizeModule,ChartScenes.SizePrintout }) {
						using ToothChartWrapper wrapper=CreatePrepared("empty-universal",chartSize);
						ToothChartData tcData=wrapper.TcData;
						tcData.CursorTool=CursorTool.Pen;
						MouseLog mouseLog=new MouseLog();
						ToothChartMouse toothChartMouse=CreateMouse(mouseLog);
						Point[] arrayPoints={ new Point(40,60),new Point(77,61),new Point(123,58),new Point(250,185) };
						toothChartMouse.MouseMove(tcData,new Point(1,1),chartSize.Size);
						Assert.AreEqual(0,tcData.PointList.Count,"No point without the button down.");
						toothChartMouse.MouseDown(tcData,arrayPoints[0],chartSize.Size);
						for(int i=1;i<arrayPoints.Length;i++) {
							toothChartMouse.MouseMove(tcData,arrayPoints[i],chartSize.Size);
						}
						Assert.AreEqual(arrayPoints.Length,tcData.PointList.Count,"The stroke in progress.");
						Assert.AreEqual(0,mouseLog.ListSegments.Count,"Nothing is raised before mouse up.");
						toothChartMouse.MouseUp(tcData,arrayPoints[arrayPoints.Length-1],chartSize.Size);
						string expected=string.Join(";",arrayPoints.Select(x => tcData.GetPointMouseScaled(x.X,x.Y,chartSize.Size))
							.Select(x => x.X+","+x.Y));
						CollectionAssert.AreEqual(new[] { expected },mouseLog.ListSegments);
						Assert.AreEqual(0,tcData.PointList.Count,"The stroke is cleared on mouse up.");
						Assert.AreEqual(arrayPoints.Length+1,mouseLog.CountStrokeChanged,"The stroke is drawn again for each point and once when it ends.");
						if(chartSize==ChartScenes.SizeModule) {
							Assert.AreEqual("40,60;77,61;123,58;250,185",expected,"At 410x307 the points are the drawing space's own.");
						}
						else {
							Assert.AreEqual("32.8,49.8;63.1,50.6;100.9,48.1;205,153.5",expected,"At 500x370 they are scaled into 410x307, to tenths.");
						}
					}
				}
				finally {
					Thread.CurrentThread.CurrentCulture=cultureOld;
				}
			});
		}

		[TestMethod]
		public void Eraser_RemovesTheFirstDrawingWithinEightOfTheCursorPlusOffset_OnlyWhileTheButtonIsDown() {
			StaRunner.Run(() => {
				using ToothChartWrapper wrapper=CreatePrepared("drawing-segments",ChartScenes.SizeModule);
				ToothChartData tcData=wrapper.TcData;
				tcData.CursorTool=CursorTool.Eraser;
				List<string> listSegmentsBefore=tcData.DrawingSegmentList.Select(x => x.DrawingSegment).ToList();
				Assert.AreEqual(3,listSegmentsBefore.Count);
				MouseLog mouseLog=new MouseLog();
				ToothChartMouse toothChartMouse=CreateMouse(mouseLog);
				//The red zig-zag's point 90,220: the eraser's centre is the cursor plus 8.49, so the cursor at 81.51,211.51 hits it dead on.
				Point pointNear=new Point(82,212);//the centre at 90.49,220.49, within 8 of 90,220
				Point pointFar=new Point(82+8,212+8);//the centre about 11 from 90,220 and far from every other point
				toothChartMouse.MouseMove(tcData,pointNear,ChartScenes.SizeModule.Size);
				Assert.AreEqual(3,tcData.DrawingSegmentList.Count,"No erasing without the button down.");
				toothChartMouse.MouseDown(tcData,pointFar,ChartScenes.SizeModule.Size);
				Assert.AreEqual(3,tcData.DrawingSegmentList.Count,"Mouse down alone erases nothing, as in DirectX.");
				toothChartMouse.MouseMove(tcData,pointFar,ChartScenes.SizeModule.Size);
				Assert.AreEqual(3,tcData.DrawingSegmentList.Count,"Outside the radius.");
				Assert.AreEqual(-1,ToothChartMouse.FindSegmentNear(tcData,tcData.GetPointMouseScaled(pointFar.X+ToothChartMouse.EraserOffset,
					pointFar.Y+ToothChartMouse.EraserOffset,ChartScenes.SizeModule.Size),ToothChartMouse.EraserRadius));
				toothChartMouse.MouseMove(tcData,pointNear,ChartScenes.SizeModule.Size);
				CollectionAssert.AreEqual(new[] { listSegmentsBefore[1] },mouseLog.ListSegments,"SegmentDrawn with the erased drawing's string.");
				CollectionAssert.AreEqual(new[] { listSegmentsBefore[0],listSegmentsBefore[2] },tcData.DrawingSegmentList.Select(x => x.DrawingSegment).ToList());
				Assert.AreEqual(1,mouseLog.CountChartChanged);
				toothChartMouse.MouseUp(tcData,pointNear,ChartScenes.SizeModule.Size);
				Assert.AreEqual(1,mouseLog.ListSegments.Count,"Mouse up raises nothing for the eraser.");
			});
		}

		[TestMethod]
		public void ColorChanger_RecoloursTheFirstDrawingWithinTwo_AndRaisesItsOldString() {
			StaRunner.Run(() => {
				using ToothChartWrapper wrapper=CreatePrepared("drawing-segments",ChartScenes.SizeModule);
				ToothChartData tcData=wrapper.TcData;
				tcData.CursorTool=CursorTool.ColorChanger;
				tcData.ColorDrawing=Color.Orange;
				MouseLog mouseLog=new MouseLog();
				ToothChartMouse toothChartMouse=CreateMouse(mouseLog);
				string segmentBlue=tcData.DrawingSegmentList[2].DrawingSegment;
				toothChartMouse.MouseDown(tcData,new Point(205,163),ChartScenes.SizeModule.Size);//3 from 205,160: outside the radius of 2
				toothChartMouse.MouseUp(tcData,new Point(205,163),ChartScenes.SizeModule.Size);
				Assert.AreEqual(0,mouseLog.ListSegments.Count);
				Assert.AreEqual(Color.Blue.ToArgb(),tcData.DrawingSegmentList[2].ColorDraw.ToArgb());
				toothChartMouse.MouseDown(tcData,new Point(205,161),ChartScenes.SizeModule.Size);//1 from 205,160
				CollectionAssert.AreEqual(new[] { segmentBlue },mouseLog.ListSegments);
				Assert.AreEqual(Color.Orange.ToArgb(),tcData.DrawingSegmentList[2].ColorDraw.ToArgb());
				Assert.AreEqual(1,mouseLog.CountChartChanged);
			});
		}

		[TestMethod]
		public void Cursors_ComeFromTheEmbeddedFiles_ForTheWrapperAndItsSubclasses() {
			StaRunner.Run(() => {
				foreach(CursorTool cursorTool in new[] { CursorTool.Pen,CursorTool.Eraser,CursorTool.ColorChanger }) {
					string resourceName=ToothChartMouse.GetCursorResourceName(cursorTool);
					Assert.IsTrue(typeof(ToothChartWrapper).Assembly.GetManifestResourceNames().Contains(resourceName),resourceName);
					using(System.IO.Stream stream=ToothChartMouse.GetCursorStream(cursorTool)) {
						Assert.IsNotNull(stream);
					}
					Cursor cursor=ToothChartMouse.GetCursor(cursorTool);
					Assert.AreNotSame(Cursors.Default,cursor,cursorTool.ToString());
				}
				Assert.IsNull(ToothChartMouse.GetCursorResourceName(CursorTool.Pointer));
				Assert.AreSame(Cursors.Default,ToothChartMouse.GetCursor(CursorTool.Pointer));
				//A subclass of the wrapper (here the tests' own) gets the cursors too: they are loaded from the chart's assembly, not the runtime type's.
				using(TestWrapper wrapper=new TestWrapper()) {
					wrapper.CursorTool=CursorTool.Pen;
					Assert.AreNotSame(Cursors.Default,wrapper.Cursor);
					wrapper.CursorTool=CursorTool.Pointer;
					Assert.AreSame(Cursors.Default,wrapper.Cursor);
				}
			});
		}

		[TestMethod]
		public void ImageHost_DrivesTheSharedMouse_AndTheWrapperRaisesItsEvents() {
			StaRunner.Run(() => {
				using ToothChartWrapper wrapper=WpfCharts.CreateWrapper(ChartScenes.SizeModule.Size);
				ToothChartRelay relay=WpfCharts.CreateRelay(wrapper);
				ChartScenes.Prepare(relay,ChartScenes.Get("drawing-segments"),ChartColorSet.Module);
				int countSelections=0;
				List<string> listSegments=new List<string>();
				wrapper.ToothSelectionsChanged+=sender => countSelections++;
				wrapper.SegmentDrawn+=(sender,e) => listSegments.Add(e.DrawingSegement);
				ToothChartImageHost toothChartImageHost=wrapper.ImageHostWpf;
				toothChartImageHost.Size=wrapper.Size;
				Point point=GetNumberCentre(wrapper.TcData,"14");
				RaiseMouse(toothChartImageHost,"OnMouseDown",point);
				RaiseMouse(toothChartImageHost,"OnMouseUp",point);
				CollectionAssert.AreEqual(new[] { "14" },wrapper.SelectedTeeth);
				Assert.AreEqual(1,countSelections);
				Assert.IsTrue(wrapper.IsWpfSceneDirty,"The selection asks for a new scene.");
				relay.CursorTool=CursorTool.Pen;
				RaiseMouse(toothChartImageHost,"OnMouseDown",new Point(10,10));
				RaiseMouse(toothChartImageHost,"OnMouseMove",new Point(20,12));
				RaiseMouse(toothChartImageHost,"OnMouseUp",new Point(20,12));
				CollectionAssert.AreEqual(new[] { ToothChartMouse.GetDrawingSegment(wrapper.TcData,new List<PointF> { new PointF(10,10),new PointF(20,12) },
					ChartScenes.SizeModule.Size) },listSegments);
			});
		}

		///<summary>TC-9a: the switch with which DirectX_GivesTheSameSelectionsAndStrings_ForTheSameInput wrote the DirectX chart's answers, while the
		///DirectX engine was still in the build (RecordDirectXMouse).</summary>
		public const string EnvRecordDirectXMouse="OPMS_RECORD_DIRECTX_MOUSE";
		public const string FileDirectXMouse="directx-mouse.txt";

		///<summary>The DirectX chart's answers to the input below, recorded before TC-9a retired that chart (Baselines\legacy-numbers\directx-mouse.txt):
		///the shared class gives the same selections after each input, the same SegmentDrawn strings in order, the same commits of the selection and
		///the same drawings, recoloured and erased alike, at the module size (where GetPointMouseScaled changes nothing), the printout size and 150%
		///of the module (where the SegmentDrawn strings are scaled into 410x307 and rounded to tenths) and the big chart window.</summary>
		[TestMethod]
		public void DirectX_GivesTheSameSelectionsAndStrings_ForTheSameInput() {
			Dictionary<string,List<string[]>> dictRecorded=ReadRecordedDirectXMouse();
			StaRunner.Run(() => {
				CultureInfo cultureOld=Thread.CurrentThread.CurrentCulture;
				try {
					Thread.CurrentThread.CurrentCulture=new CultureInfo("en-US");
					foreach(ChartSize chartSize in new[] { ChartScenes.SizeModule,ChartScenes.SizePrintout,new ChartSize(615,461,ChartColorSet.Module),
						ChartScenes.SizeBigWindow })
					{
						string sizeName=chartSize.Size.Width+"x"+chartSize.Size.Height;
						Assert.IsTrue(dictRecorded.ContainsKey(sizeName),"No DirectX answers recorded at "+sizeName);
						CompareWithDirectX(chartSize,dictRecorded[sizeName]);
					}
					Assert.AreEqual(4,dictRecorded.Count,"The four sizes, and no other: "+string.Join(", ",dictRecorded.Keys));
				}
				finally {
					Thread.CurrentThread.CurrentCulture=cultureOld;
				}
			});
		}

		///<summary>Recorded Baselines\legacy-numbers\directx-mouse.txt with OPMS_RECORD_DIRECTX_MOUSE=1 on the build PC: the input of CompareWithDirectX
		///raised on a DirectX chart of each size on a shown form (the chart's own OnMouseDown, OnMouseMove and OnMouseUp), and the DirectX chart's
		///selections, SegmentDrawn strings, commits and drawings, while the same run checked the shared class against the live chart. The recording
		///is kept (O-32); it cannot be made again.</summary>
		[TestMethod]
		[Ignore("The DirectX engine was retired in TC-9a: its answers to the mouse input cannot be recorded again. The recording it made "
			+"(Baselines\\legacy-numbers\\directx-mouse.txt) is kept, and DirectX_GivesTheSameSelectionsAndStrings_ForTheSameInput compares with it.")]
		public void RecordDirectXMouse() {
			Assert.Inconclusive("Retired in TC-9a.");
		}

		///<summary>The recording of the DirectX chart's answers, by size ("410x307"): each line's fields after the size (kind, then the kind's fields).
		///</summary>
		private static Dictionary<string,List<string[]>> ReadRecordedDirectXMouse() {
			string path=Path.Combine(TestPaths.BaselinesLegacyNumbers,FileDirectXMouse);
			Assert.IsTrue(File.Exists(path),"The recording of the DirectX chart's answers is missing: "+path);
			Dictionary<string,List<string[]>> dictRecorded=new Dictionary<string,List<string[]>>();
			foreach(string line in File.ReadAllLines(path,Encoding.UTF8)) {
				if(line.StartsWith("#") || line.Trim()=="") {
					continue;
				}
				string[] arrayFields=line.Split('\t');
				if(!dictRecorded.ContainsKey(arrayFields[0])) {
					dictRecorded[arrayFields[0]]=new List<string[]>();
				}
				dictRecorded[arrayFields[0]].Add(arrayFields.Skip(1).ToArray());
			}
			return dictRecorded;
		}

		///<summary>The same input as the DirectX chart of the size got when it was recorded, to the shared class on a copy of its data: hovers, clicks,
		///a drag, a pen stroke, the colour changer and the eraser, at points scaled from the module's (the drawings of "drawing-segments" are in the
		///410x307 space). listRecorded holds the DirectX chart's answers at this size.</summary>
		private static void CompareWithDirectX(ChartSize chartSize,List<string[]> listRecorded) {
			Size size=chartSize.Size;
			string sizeName=size.Width+"x"+size.Height;
			using ToothChartWrapper wrapperShared=CreatePrepared("drawing-segments",chartSize);
			MouseLog mouseLog=new MouseLog();
			ToothChartMouse toothChartMouse=CreateMouse(mouseLog);
			ToothChartData tcDataShared=wrapperShared.TcData;
			List<Tuple<CursorTool,string,Point>> listInput=new List<Tuple<CursorTool,string,Point>>();
			Point point3=GetNumberCentre(tcDataShared,"3");
			Point point12=GetNumberCentre(tcDataShared,"12");
			Point point30=GetNumberCentre(tcDataShared,"30");
			listInput.Add(Tuple.Create(CursorTool.Pointer,"move",point30));
			listInput.Add(Tuple.Create(CursorTool.Pointer,"down",point30));
			listInput.Add(Tuple.Create(CursorTool.Pointer,"up",point30));
			listInput.Add(Tuple.Create(CursorTool.Pointer,"move",point3));
			listInput.Add(Tuple.Create(CursorTool.Pointer,"down",point3));
			foreach(Point point in GetDragPoints(point3,point12,5).Skip(1)) {
				listInput.Add(Tuple.Create(CursorTool.Pointer,"move",point));
			}
			listInput.Add(Tuple.Create(CursorTool.Pointer,"up",point12));
			listInput.Add(Tuple.Create(CursorTool.Pen,"down",ScaleFromModule(new PointF(30,280),size)));
			listInput.Add(Tuple.Create(CursorTool.Pen,"move",ScaleFromModule(new PointF(45,284),size)));
			listInput.Add(Tuple.Create(CursorTool.Pen,"move",ScaleFromModule(new PointF(63,290),size)));
			listInput.Add(Tuple.Create(CursorTool.Pen,"up",ScaleFromModule(new PointF(63,290),size)));
			//Within 2 of the blue drawing's point 205,160 once scaled back (GetPointMouseScaled).
			listInput.Add(Tuple.Create(CursorTool.ColorChanger,"down",ScaleFromModule(new PointF(205,161),size)));
			listInput.Add(Tuple.Create(CursorTool.ColorChanger,"up",ScaleFromModule(new PointF(205,161),size)));
			//The eraser's centre is the cursor plus 8.49 screen pixels, scaled: the cursor goes where that centre lands near the red zig-zag's 90,220.
			Point pointEraser=new Point((int)Math.Round(90f*size.Width/410f-ToothChartMouse.EraserOffset),
				(int)Math.Round(220f*size.Height/307f-ToothChartMouse.EraserOffset));
			listInput.Add(Tuple.Create(CursorTool.Eraser,"down",pointEraser));
			listInput.Add(Tuple.Create(CursorTool.Eraser,"move",pointEraser));
			listInput.Add(Tuple.Create(CursorTool.Eraser,"up",pointEraser));
			List<string[]> listRecordedInput=listRecorded.Where(x => x[0]=="input").ToList();
			Assert.AreEqual(listInput.Count,listRecordedInput.Count,sizeName+": the recorded input is this input.");
			for(int i=0;i<listInput.Count;i++) {
				Tuple<CursorTool,string,Point> input=listInput[i];
				string[] arrayRecorded=listRecordedInput[i];
				//The recorded input: its index, tool, kind and point, and the teeth DirectX had selected after it.
				Assert.AreEqual(i+"|"+input.Item1+"|"+input.Item2+"|"+input.Item3.X+","+input.Item3.Y,string.Join("|",arrayRecorded.Skip(1).Take(4)),
					sizeName+": the recorded input "+i+" is this input.");
				tcDataShared.CursorTool=input.Item1;
				tcDataShared.ColorDrawing=Color.Orange;
				if(input.Item2=="down") {
					toothChartMouse.MouseDown(tcDataShared,input.Item3,size);
				}
				else if(input.Item2=="up") {
					toothChartMouse.MouseUp(tcDataShared,input.Item3,size);
				}
				else {
					toothChartMouse.MouseMove(tcDataShared,input.Item3,size);
				}
				List<string> listSelectedDirectX=arrayRecorded[5]=="" ? new List<string>() : arrayRecorded[5].Split(',').ToList();
				CollectionAssert.AreEquivalent(listSelectedDirectX,tcDataShared.SelectedTeeth,sizeName+": "+input.Item1+" "+input.Item2+" at "+input.Item3);
			}
			List<string> listSegmentsDirectX=listRecorded.Where(x => x[0]=="segment").Select(x => x[2]).ToList();
			int countSelectionsDirectX=int.Parse(listRecorded.Single(x => x[0]=="selections")[1]);
			List<string> listDrawingsDirectX=listRecorded.Where(x => x[0]=="drawing").Select(x => x[2]).ToList();
			CollectionAssert.AreEqual(listSegmentsDirectX,mouseLog.ListSegments,sizeName+": the same SegmentDrawn strings, in order.");
			Assert.AreEqual(countSelectionsDirectX,mouseLog.CountSelectionsChanged,sizeName+": the same commits of the selection.");
			CollectionAssert.AreEqual(listDrawingsDirectX,tcDataShared.DrawingSegmentList.Select(x => x.DrawingSegment+"|"+x.ColorDraw.ToArgb()).ToList(),
				sizeName+": the same drawings, recoloured and erased alike.");
			Assert.IsTrue(listSegmentsDirectX.Count==3,sizeName+": the pen stroke, the recoloured drawing and the erased one: "
				+string.Join(" / ",listSegmentsDirectX));
			Assert.IsTrue(countSelectionsDirectX==2 && tcDataShared.SelectedTeeth.Count==11,sizeName+": 30, then 3 to 12: "+string.Join(",",tcDataShared.SelectedTeeth));
			if(size!=ChartScenes.SizeModule.Size) {
				Assert.IsTrue(listSegmentsDirectX[0].Contains("."),sizeName+": the stroke's points are scaled into 410x307 and rounded to tenths: "+listSegmentsDirectX[0]);
			}
		}

		///<summary>A point of the module's 410x307 chart at the same place of a chart of the size, rounded to whole pixels.</summary>
		private static Point ScaleFromModule(PointF pointModule,Size size) {
			return new Point((int)Math.Round(pointModule.X*size.Width/410f),(int)Math.Round(pointModule.Y*size.Height/307f));
		}

		#region Helpers
		///<summary>The centre of a tooth's number box, in the chart's pixels, as the chart computes the box (DirectX's measure rule).</summary>
		public static Point GetNumberCentre(ToothChartData tcData,string toothId) {
			ToothScene toothScene=ToothSceneBuilder.Build(tcData);
			ToothSceneItem2D item=toothScene.Items2D.First(x => x.Source=="number "+toothId);
			return new Point(item.Origin.X+item.WidthPixels/2,item.Origin.Y+item.Font.Height/2);
		}

		///<summary>Points from start to end in countSteps equal steps, the start and the end included: wide steps, so a drag skips teeth.</summary>
		public static List<Point> GetDragPoints(Point pointStart,Point pointEnd,int countSteps) {
			List<Point> listPoints=new List<Point>();
			for(int i=0;i<=countSteps;i++) {
				listPoints.Add(new Point(pointStart.X+(pointEnd.X-pointStart.X)*i/countSteps,pointStart.Y+(pointEnd.Y-pointStart.Y)*i/countSteps));
			}
			return listPoints;
		}

		///<summary>DirectX's pointer logic for a drag over the points (a hover at the first, then down there, moves to the others), written out with
		///ToothChartData's own functions on a copy of the selection: the selection DirectX ends with.</summary>
		private static List<string> ReplayDirectXDrag(ToothChartData tcData,List<Point> listPoints) {
			List<string> listSelected=new List<string>(tcData.SelectedTeeth);
			string hotToothOld=tcData.GetToothAtPoint(listPoints[0]);//the hover before the press
			string toothClicked=tcData.GetToothAtPoint(listPoints[0]);
			Toggle(listSelected,toothClicked);
			foreach(Point point in listPoints.Skip(1)) {
				string hotTooth=tcData.GetToothAtPoint(point);
				if(hotTooth==hotToothOld) {
					continue;
				}
				foreach(string toothId in tcData.GetAffectedTeeth(hotToothOld,hotTooth,tcData.PointPixToMm(point).Y)) {
					Toggle(listSelected,toothId);
				}
				hotToothOld=hotTooth;
			}
			return listSelected;
		}

		private static void Toggle(List<string> listSelected,string toothId) {
			if(listSelected.Contains(toothId)) {
				listSelected.Remove(toothId);
			}
			else {
				listSelected.Add(toothId);
			}
		}

		private static void DriveDrag(ToothChartMouse toothChartMouse,ToothChartData tcData,List<Point> listPoints,Size size) {
			toothChartMouse.MouseMove(tcData,listPoints[0],size);//the hover before the press
			toothChartMouse.MouseDown(tcData,listPoints[0],size);
			foreach(Point point in listPoints.Skip(1)) {
				toothChartMouse.MouseMove(tcData,point,size);
			}
			toothChartMouse.MouseUp(tcData,listPoints[listPoints.Count-1],size);
		}

		///<summary>Raises a control's protected OnMouseDown, OnMouseMove or OnMouseUp with the left button at the point: synthesised input that runs
		///the control's own handlers.</summary>
		public static void RaiseMouse(Control control,string methodName,Point point) {
			MethodInfo methodInfo=typeof(Control).GetMethod(methodName,BindingFlags.Instance|BindingFlags.NonPublic,null,new[] { typeof(MouseEventArgs) },null);
			MouseButtons mouseButtons=methodName=="OnMouseMove"?MouseButtons.None:MouseButtons.Left;
			methodInfo.Invoke(control,new object[] { new MouseEventArgs(mouseButtons,methodName=="OnMouseMove"?0:1,point.X,point.Y,0) });
		}
		#endregion Helpers
	}
}
