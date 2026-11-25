// Tests of package TC-6 (docs/specs/wpf-tooth-chart-plan.md, the TC-6 row and section 10's parity rows ORTHO-1, ORTHO-2, RR-1 and SM-1;
// docs/port-notes/TC-6.md): ortho mode's facial-only layout and its hit-testing, the brackets on the facial surfaces following the teeth, the
// wires and elastics, the retained roots and the space maintainers' bars, the Helix box's winding, and the wrapper's state.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Media.Media3D;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental;
using OpenPmsDentalBusiness;
using SparksToothChart;
using SparksToothChart.Wpf3D;

namespace ToothChartImageTests {
	[TestClass]
	public class WpfOrthoTests {
		///<summary>A 2D wrapper of the size with the scene applied through the relay (its TcData is the chart's data in every engine), and its scene.</summary>
		private static ToothScene BuildScene(string sceneName,ChartSize chartSize,out ToothChartData tcData) {
			ToothChartData tcDataOut=null;
			ToothScene toothScene=StaRunner.Run(() => {
				using(ToothChartWrapper wrapper=new ToothChartWrapper()) {
					wrapper.Size=chartSize.Size;
					ChartScenes.Prepare(WpfCharts.CreateRelay(wrapper),ChartScenes.Get(sceneName),chartSize.ColorSet);
					tcDataOut=wrapper.TcData.Copy();
					return ToothSceneBuilder.Build(wrapper.TcData);
				}
			});
			tcData=tcDataOut;
			return toothScene;
		}

		///<summary>The image point of the centre of the facial surface (F or B) of a tooth's facial view, as the chart draws it.</summary>
		private static PointF GetFacialCentre(ToothScene scene,ToothChartData tcData,string toothId) {
			ToothGraphic toothGraphic=tcData.ListToothGraphics[toothId];
			ToothGroup group=toothGraphic.GetGroup(ToothGraphic.IsAnterior(toothId)?ToothGroupType.F:ToothGroupType.B) ?? toothGraphic.GetGroup(ToothGroupType.Enamel);
			PointF? point=ParityProbes.GetCentre(scene,toothGraphic,toothGraphic.Groups.IndexOf(group),ToothSceneBuilder.GetFacialWorld(tcData,toothGraphic),true);
			Assert.IsNotNull(point,toothId);
			return point.Value;
		}

		[TestMethod]
		public void OrthoMode_DrawsOnlyTheFacialRows_MovedTowardTheCentreLine_TheNumbersStay() {
			ToothScene sceneNormal=BuildScene("empty-palmer",ChartScenes.SizeModule,out ToothChartData tcDataNormal);
			ToothScene sceneOrtho=BuildScene("ortho-mode-hardware-off",ChartScenes.SizeModule,out ToothChartData tcDataOrtho);
			Assert.IsFalse(tcDataNormal.IsOrthoMode);
			Assert.IsTrue(tcDataOrtho.IsOrthoMode);
			Assert.IsTrue(sceneNormal.Items3D.Any(x => x.Source.EndsWith(" occlusal")));
			Assert.IsFalse(sceneOrtho.Items3D.Any(x => x.Source.Contains("occlusal")),"Ortho mode draws no occlusal row (ORTHO-1).");
			List<ToothSceneItem3D> listFacialNormal=sceneNormal.Items3D.Where(x => x.Source.EndsWith(" facial")).ToList();
			List<ToothSceneItem3D> listFacialOrtho=sceneOrtho.Items3D.Where(x => x.Source.EndsWith(" facial")).ToList();
			Assert.AreEqual(listFacialNormal.Count,listFacialOrtho.Count,"The same facial views.");
			Assert.IsTrue(listFacialOrtho.Count>300);
			for(int i=0;i<listFacialNormal.Count;i++) {
				ToothSceneItem3D itemNormal=listFacialNormal[i];
				ToothSceneItem3D itemOrtho=listFacialOrtho[i];
				Assert.AreEqual(itemNormal.ToothId,itemOrtho.ToothId);
				Assert.AreEqual(itemNormal.GroupIndex,itemOrtho.GroupIndex);
				float shift=ToothGraphic.IsMaxillary(itemNormal.ToothId) ? -ToothSceneBuilder.OrthoFacialShiftMm : ToothSceneBuilder.OrthoFacialShiftMm;
				Assert.AreEqual(itemNormal.Transform.M41,itemOrtho.Transform.M41,itemOrtho.ToString());
				Assert.AreEqual(itemNormal.Transform.M42+shift,itemOrtho.Transform.M42,0.0001,itemOrtho.ToString());
				Assert.AreEqual(itemNormal.Transform.M11,itemOrtho.Transform.M11,"The teeth keep their size and turn (Capture13: no enlargement).");
			}
			//The numbers and the centre line stay where they are: only the rows move.
			List<ToothSceneItem2D> listNumbersNormal=sceneNormal.Items2D.Where(x => x.Source.StartsWith("number ") || x.Source==ToothSceneBuilder.SourceCentreLine).ToList();
			List<ToothSceneItem2D> listNumbersOrtho=sceneOrtho.Items2D.Where(x => x.Source.StartsWith("number ") || x.Source==ToothSceneBuilder.SourceCentreLine).ToList();
			Assert.AreEqual(33,listNumbersOrtho.Count,"32 Palmer numbers and the centre line.");
			Assert.AreEqual(string.Join("\r\n",listNumbersNormal.Select(x => x.ToString())),string.Join("\r\n",listNumbersOrtho.Select(x => x.ToString())));
			//The rows end near the centre line: the upper incisors' facial centres within 12 to 22 mm of it (normal chart: about 26 mm).
			PointF pointOrtho=GetFacialCentre(sceneOrtho,tcDataOrtho,"8");
			PointF pointNormal=GetFacialCentre(sceneNormal,tcDataNormal,"8");
			Assert.AreEqual(ToothSceneBuilder.OrthoFacialShiftMm*tcDataOrtho.ScaleMmToPix,pointOrtho.Y-pointNormal.Y,0.5,"Moved down by the shift.");
			Assert.AreEqual(pointNormal.X,pointOrtho.X,0.01);
		}

		[TestMethod]
		public void OrthoMode_HitTesting_FollowsTheLayout_EveryCrownSelectsItsTooth() {
			StaRunner.Run(() => {
				foreach(ChartSize chartSize in new[] { ChartScenes.SizeModule,ChartScenes.SizePrintout,ChartScenes.SizeBigWindow }) {
					foreach(string sceneName in new[] { "ortho-mode-hardware-off","ortho-mode-moved-teeth" }) {
						using ToothChartWrapper wrapper=new ToothChartWrapper();
						wrapper.Size=chartSize.Size;
						ChartScenes.Prepare(WpfCharts.CreateRelay(wrapper),ChartScenes.Get(sceneName),chartSize.ColorSet);
						ToothChartData tcData=wrapper.TcData;
						ToothScene toothScene=ToothSceneBuilder.Build(tcData);
						for(int i=1;i<=32;i++) {
							string toothId=i.ToString();
							PointF pointF=GetFacialCentre(toothScene,tcData,toothId);
							Point point=new Point((int)Math.Floor(pointF.X),(int)Math.Floor(pointF.Y));
							Assert.AreEqual(toothId,tcData.GetToothAtPoint(point),sceneName+" "+chartSize.FolderName+" at "+point);
							tcData.SelectedTeeth.Clear();
							ToothChartMouse toothChartMouse=new ToothChartMouse();
							toothChartMouse.MouseDown(tcData,point,chartSize.Size);
							toothChartMouse.MouseUp(tcData,point,chartSize.Size);
							CollectionAssert.AreEqual(new[] { toothId },tcData.SelectedTeeth,"A click on the crown of "+toothId+" ("+sceneName+" "+chartSize.FolderName+")");
						}
					}
				}
			});
		}

		[TestMethod]
		public void Brackets_SitOnTheFacialSurface_AndFollowMovedAndMirroredTeeth() {
			foreach(string sceneName in new[] { "ortho-hardware-normal-layout","ortho-hardware-moved-teeth","ortho-mode-moved-teeth" }) {
				ToothScene toothScene=BuildScene(sceneName,ChartScenes.SizeModule,out ToothChartData tcData);
				for(int i=1;i<=32;i++) {
					string toothId=i.ToString();
					ToothGraphic toothGraphic=tcData.ListToothGraphics[toothId];
					List<ToothSceneItem3D> listItems=toothScene.Items3D.Where(x => x.Source.StartsWith(ToothSceneBuilder.SourceBracket+toothId)
						&& (x.Source.Length==(ToothSceneBuilder.SourceBracket+toothId).Length || x.Source[(ToothSceneBuilder.SourceBracket+toothId).Length]==' ')).ToList();
					if(!toothGraphic.HasBracket) {
						Assert.AreEqual(0,listItems.Count,sceneName+" "+toothId);
						continue;
					}
					Assert.AreEqual(5,listItems.Count,sceneName+" "+toothId+": the base and four tie wings.");
					Assert.IsTrue(listItems.All(x => x.ToothId==ToothSceneShapes.Box && x.GroupIndex==0 && !x.IsReversed && !x.IsFlat && x.Color.A==255));
					Assert.AreEqual(Color.Silver.ToArgb(),listItems[0].Color.ToArgb(),"The base in the hardware's colour.");
					Assert.IsTrue(listItems.Skip(1).All(x => x.Color.R<listItems[0].Color.R),"Darker tie wings.");
					//The base's transform is the unit box scaled, then the bracket's world: the tooth's facial world with its movements.
					ChartMatrix world=ToothSceneBuilder.GetBracketWorld(tcData,toothGraphic);
					ChartMatrix expected=ChartMatrix.Scaling(ToothSceneBuilder.BracketWidthMm,ToothSceneBuilder.BracketHeightMm,
						ToothSceneBuilder.BracketBaseBackMm+ToothSceneBuilder.BracketBaseFrontMm)
						*ChartMatrix.Translation(0,0,(ToothSceneBuilder.BracketBaseFrontMm-ToothSceneBuilder.BracketBaseBackMm)/2f)*world;
					Assert.IsTrue(expected.IsEqualTo(listItems[0].Transform),sceneName+" "+toothId+": "+listItems[0].Transform);
					//On the crown: the bracket's middle lies inside the crown's projected outline, and its face in front of every crown point under it.
					CheckOnCrown(toothScene,tcData,toothGraphic,sceneName);
				}
			}
		}

		private static void CheckOnCrown(ToothScene toothScene,ToothChartData tcData,ToothGraphic toothGraphic,string sceneName) {
			ChartMatrix worldFacial=ToothSceneBuilder.GetFacialWorld(tcData,toothGraphic);
			ChartMatrix worldBracket=ToothSceneBuilder.GetBracketWorld(tcData,toothGraphic);
			worldBracket.TransformCoordinate(0,0,ToothSceneBuilder.BracketBaseFrontMm,out float xFront,out float yFront,out float zFront);
			ToothSceneBuilder.BracketPlacement placement=ToothSceneBuilder.GetBracketPlacement(toothGraphic);
			float zFrontModel=placement.FaceZ+ToothSceneBuilder.BracketBaseFrontMm;
			float xMin=float.MaxValue,xMax=float.MinValue,yMin=float.MaxValue,yMax=float.MinValue,zUnder=float.MinValue;
			for(int g=0;g<toothGraphic.Groups.Count;g++) {
				ToothGroupType groupType=toothGraphic.Groups[g].GroupType;
				if(groupType==ToothGroupType.Cementum || groupType==ToothGroupType.Buildup || groupType==ToothGroupType.None) {
					continue;
				}
				foreach(int index in toothGraphic.Groups[g].GetTriangleIndices()) {
					Vertex3f vertex=toothGraphic.VertexNormals[index].Vertex;
					worldFacial.TransformCoordinate(vertex.X,vertex.Y,vertex.Z,out float x,out float y,out float z);
					xMin=Math.Min(xMin,x);
					xMax=Math.Max(xMax,x);
					yMin=Math.Min(yMin,y);
					yMax=Math.Max(yMax,y);
					//Under the bracket, in the model's own frame, where the bracket is placed (the world transform is the same for both).
					if(Math.Abs(vertex.X)<ToothSceneBuilder.BracketWidthMm/2f && Math.Abs(vertex.Y-placement.CenterY)<ToothSceneBuilder.BracketHeightMm/2f) {
						zUnder=Math.Max(zUnder,vertex.Z);
					}
				}
			}
			string name=sceneName+" "+toothGraphic.ToothID;
			Assert.IsTrue(xFront>xMin && xFront<xMax && yFront>yMin && yFront<yMax,name+": the bracket's middle is on the crown.");
			Assert.AreNotEqual(float.MinValue,zUnder,name);
			Assert.IsTrue(zFrontModel>zUnder,name+": the bracket's face ("+zFrontModel+") is in front of the crown under it ("+zUnder+").");
			Assert.IsTrue(zFrontModel-zUnder<=ToothSceneBuilder.BracketBaseFrontMm+0.0001f,name+": and on it, not floating.");
			//About 4 mm from the incisal or occlusal edge (the crown's lowest point on the maxilla, its highest on the mandible), within the tilts of
			//the moved teeth.
			float fromEdge=ToothGraphic.IsMaxillary(toothGraphic.ToothID) ? yFront-yMin : yMax-yFront;
			Assert.IsTrue(fromEdge>2.5f && fromEdge<5.5f,name+": "+fromEdge+" mm from the edge.");
		}

		[TestMethod]
		public void WiresAndElastics_AreThinLines_FromTheBrackets_InTheirOrder_UnderTheWatchesAndNumbers() {
			ToothScene toothScene=BuildScene("ortho-hardware-moved-teeth",ChartScenes.SizeModule,out ToothChartData tcData);
			List<ToothSceneItem2D> listItems=toothScene.Items2D.ToList();
			List<ToothSceneItem2D> listLines=listItems.Where(x => x.Source.StartsWith(ToothSceneBuilder.SourceWire) || x.Source.StartsWith(ToothSceneBuilder.SourceWireInBracket)
				|| x.Source.StartsWith(ToothSceneBuilder.SourceElastic)).ToList();
			//The painter's order: wire 2-15 (in 2, 2-3, in 3, ..., in 15), wire 18-31, then the two elastics.
			Assert.AreEqual(tcData.ListOrthoWires.Count,listLines.Count);
			Assert.AreEqual(14+13+14+13+2,listLines.Count);
			Assert.AreEqual(ToothSceneBuilder.SourceWireInBracket+"2",listLines[0].Source);
			Assert.AreEqual(ToothSceneBuilder.SourceWire+"2-3",listLines[1].Source);
			Assert.AreEqual(ToothSceneBuilder.SourceElastic+"6-30",listLines[listLines.Count-2].Source);
			Assert.AreEqual(ToothSceneBuilder.SourceElastic+"11-19",listLines[listLines.Count-1].Source);
			int indexFirstLine=listItems.IndexOf(listLines[0]);
			int indexLastLine=listItems.IndexOf(listLines[listLines.Count-1]);
			Assert.IsTrue(listItems.Take(indexFirstLine).All(x => !x.Source.StartsWith("number ") && x.Source!=ToothSceneBuilder.SourceCentreLine),"After the teeth.");
			Assert.IsTrue(listItems.FindIndex(x => x.Source.StartsWith("number "))>indexLastLine,"Under the numbers.");
			foreach(ToothSceneItem2D item in listLines) {
				Assert.AreEqual(ChartItem2DKind.Polygons,item.Kind);
				Assert.AreEqual(1,item.Polygons.Count,item.ToString());
				Assert.AreEqual(255,item.Color.A);
				//Thin: the quad's short side is the line's width, about one pixel at the module's size (0.32 mm, 3.15 pixels a mm).
				PointF[] arrayPoints=item.Polygons[0];
				double sideA=Distance(arrayPoints[0],arrayPoints[1]);
				double sideB=Distance(arrayPoints[1],arrayPoints[2]);
				double width=Math.Min(sideA,sideB);
				Assert.IsTrue(width>0.5 && width<=2.01,item.Source+": "+width+" pixels wide (ORTHO-2: about 1 to 2 pixels).");
			}
			Assert.AreEqual(ChartScenes.OrthoSilver.ToArgb(),listLines[0].Color.ToArgb());
			Assert.AreEqual(ChartScenes.OrthoElasticYellow.ToArgb(),listLines[listLines.Count-1].Color.ToArgb());
			//An elastic runs from the middle of one bracket to the middle of the other (here 11, shifted, to 19); a wire in a bracket across its slot.
			PointF pointStart=Project(toothScene,ToothSceneBuilder.GetBracketPoint(tcData,tcData.ListToothGraphics["11"],0));
			PointF pointEnd=Project(toothScene,ToothSceneBuilder.GetBracketPoint(tcData,tcData.ListToothGraphics["19"],0));
			CheckLineEnds(listLines[listLines.Count-1],pointStart,pointEnd);
			ToothGraphic toothGraphic6=tcData.ListToothGraphics["6"];
			CheckLineEnds(listLines.Single(x => x.Source==ToothSceneBuilder.SourceWireInBracket+"6"),
				Project(toothScene,ToothSceneBuilder.GetBracketPoint(tcData,toothGraphic6,-ToothSceneBuilder.BracketWidthMm/2f)),
				Project(toothScene,ToothSceneBuilder.GetBracketPoint(tcData,toothGraphic6,ToothSceneBuilder.BracketWidthMm/2f)));
			//Between brackets, from the end of one slot to the facing end of the next: 9 (mirrored) to 10 (tipped).
			CheckLineEnds(listLines.Single(x => x.Source==ToothSceneBuilder.SourceWire+"9-10"),
				Project(toothScene,ToothSceneBuilder.GetBracketPoint(tcData,tcData.ListToothGraphics["9"],ToothSceneBuilder.BracketWidthMm/2f)),
				Project(toothScene,ToothSceneBuilder.GetBracketPoint(tcData,tcData.ListToothGraphics["10"],-ToothSceneBuilder.BracketWidthMm/2f)));
			//On the mandible the numbers run right to left: 18 is right of 19, so the wire leaves 18's left end.
			CheckLineEnds(listLines.Single(x => x.Source==ToothSceneBuilder.SourceWire+"18-19"),
				Project(toothScene,ToothSceneBuilder.GetBracketPoint(tcData,tcData.ListToothGraphics["18"],-ToothSceneBuilder.BracketWidthMm/2f)),
				Project(toothScene,ToothSceneBuilder.GetBracketPoint(tcData,tcData.ListToothGraphics["19"],ToothSceneBuilder.BracketWidthMm/2f)));
		}

		private static PointF Project(ToothScene toothScene,Vertex3f vertex) {
			return toothScene.ProjectToImage(ChartMatrix.Identity,vertex.X,vertex.Y,vertex.Z);
		}

		private static double Distance(PointF p1,PointF p2) {
			return Math.Sqrt((p1.X-p2.X)*(p1.X-p2.X)+(p1.Y-p2.Y)*(p1.Y-p2.Y));
		}

		///<summary>The line's quad has its two short sides' middles at the two ends (within a pixel: a horizontal line is snapped to whole pixels).</summary>
		private static void CheckLineEnds(ToothSceneItem2D item,PointF pointStart,PointF pointEnd) {
			PointF[] arrayPoints=item.Polygons[0];
			PointF middleA=new PointF((arrayPoints[0].X+arrayPoints[3].X)/2f,(arrayPoints[0].Y+arrayPoints[3].Y)/2f);
			PointF middleB=new PointF((arrayPoints[1].X+arrayPoints[2].X)/2f,(arrayPoints[1].Y+arrayPoints[2].Y)/2f);
			if(Distance(arrayPoints[0],arrayPoints[1])<Distance(arrayPoints[1],arrayPoints[2])) {
				middleA=new PointF((arrayPoints[0].X+arrayPoints[1].X)/2f,(arrayPoints[0].Y+arrayPoints[1].Y)/2f);
				middleB=new PointF((arrayPoints[2].X+arrayPoints[3].X)/2f,(arrayPoints[2].Y+arrayPoints[3].Y)/2f);
			}
			bool isSame=Distance(middleA,pointStart)<1.01 && Distance(middleB,pointEnd)<1.01;
			bool isSwapped=Distance(middleA,pointEnd)<1.01 && Distance(middleB,pointStart)<1.01;
			Assert.IsTrue(isSame || isSwapped,item.Source+": ends "+middleA+" and "+middleB+", expected "+pointStart+" and "+pointEnd);
		}

		[TestMethod]
		public void RetainedRoot_DrawsOnlyTheRoot_InBothViews_OnlyOnAToothThatIsShown() {
			ToothScene toothScene=BuildScene("retained-root-extraction-completed",ChartScenes.SizeModule,out ToothChartData tcData);
			//29 (TP) keeps its root: only cementum groups, in the TP colour, lit (not flat), in the facial and the occlusal view.
			List<ToothSceneItem3D> listItems29=toothScene.Items3D.Where(x => x.ToothId=="29").ToList();
			Assert.IsTrue(listItems29.Count>=2,"Both views: "+listItems29.Count);
			Assert.IsTrue(listItems29.All(x => x.GroupType==ToothGroupType.Cementum && x.Color.ToArgb()==ChartScenes.TP.Dark.ToArgb() && !x.IsFlat
				&& x.Source.EndsWith(ToothSceneBuilder.SourceRetainedRootSuffix)));
			Assert.IsTrue(listItems29.Any(x => x.Source.StartsWith("29 facial")) && listItems29.Any(x => x.Source.StartsWith("29 occlusal")));
			Assert.AreEqual(ToothSceneBuilder.SpecularCementum,listItems29[0].Specular);
			//9 and 20 are missing (their extraction is complete): no root and no tooth; 20 has the X of the completed-extraction graphic.
			Assert.IsFalse(toothScene.Items3D.Concat(toothScene.Items3DUnder).Any(x => x.ToothId=="9" || x.ToothId=="20"));
			Assert.IsTrue(toothScene.Items2D.Any(x => x.Source=="big X of 20"));
			Assert.IsFalse(toothScene.Items2D.Any(x => x.Source=="big X of 9"),"A completed extraction with the graphic off draws nothing.");
			//A planned extraction keeps the root, with the X over it.
			ToothScene scenePlanned=BuildScene("retained-root-extraction-planned",ChartScenes.SizeModule,out ToothChartData tcDataPlanned);
			Assert.IsTrue(scenePlanned.Items3D.Where(x => x.ToothId=="9").All(x => x.GroupType==ToothGroupType.Cementum));
			Assert.IsTrue(scenePlanned.Items3D.Any(x => x.ToothId=="9"));
			Assert.IsTrue(scenePlanned.Items2D.Any(x => x.Source=="big X of 9"));
			//An unerupted tooth's root is translucent, beneath the opaque teeth, as the rest of an unerupted tooth (LOOK-2).
			ToothScene sceneUnerupted=StaRunner.Run(() => {
				using(ToothChartWrapper wrapper=new ToothChartWrapper()) {
					wrapper.Size=ChartScenes.SizeModule.Size;
					ToothChartRelay relay=WpfCharts.CreateRelay(wrapper);
					ChartScenes.Prepare(relay,ChartScenes.Get("empty-universal"),ChartColorSet.Module);
					relay.MoveTooth("1",0,0,0,0,-12,0);
					relay.SetRetainedRoot("1",ChartScenes.TP.Dark);
					return ToothSceneBuilder.Build(wrapper.TcData);
				}
			});
			Assert.IsFalse(sceneUnerupted.Items3D.Any(x => x.ToothId=="1"));
			Assert.IsTrue(sceneUnerupted.Items3DUnder.Where(x => x.ToothId=="1").All(x => x.GroupType==ToothGroupType.Cementum
				&& x.Color.A==ToothSceneBuilder.UneruptedAlpha));
			Assert.IsTrue(sceneUnerupted.Items3DUnder.Any(x => x.ToothId=="1"));
		}

		[TestMethod]
		public void SpaceMaintainer_IsAFlatBarAcrossEmptySpacesOnly_AtCrownHeight() {
			ToothScene toothScene=BuildScene("space-maintainer-missing-tooth",ChartScenes.SizeModule,out ToothChartData tcData);
			List<ToothSceneItem3D> listBars=GetBars(toothScene);
			Assert.AreEqual(2,listBars.Count,string.Join("\r\n",listBars));
			ToothSceneItem3D bar9=listBars.Single(x => x.Source==ToothSceneBuilder.SourceSpaceMaintainer+"9");
			Assert.IsTrue(bar9.IsFlat && bar9.ToothId==ToothSceneShapes.Box && bar9.Color.ToArgb()==ChartScenes.TP.Dark.ToArgb());
			//Across 9's space (8.5 mm), its middle 24 mm above the centre line (29 - 5), 1.5 mm high, at the depth of the teeth's middles.
			Assert.AreEqual(ToothGraphic.GetWidth("9"),bar9.Transform.M11,0.0001);
			Assert.AreEqual(ToothSceneBuilder.SpaceMaintainerHeightMm,bar9.Transform.M22,0.0001);
			Assert.AreEqual(tcData.GetTransX("9"),bar9.Transform.M41,0.0001);
			Assert.AreEqual(24f,bar9.Transform.M42,0.0001);
			Assert.AreEqual(0f,bar9.Transform.M43,0.0001);
			ToothSceneItem3D bar29=listBars.Single(x => x.Source==ToothSceneBuilder.SourceSpaceMaintainer+"29");
			Assert.AreEqual(-24f,bar29.Transform.M42,0.0001);
			Assert.AreEqual(ChartScenes.C.Dark.ToArgb(),bar29.Color.ToArgb());
			//Present teeth get no bar (the older scenes): single teeth, a quadrant, every status.
			foreach(string sceneName in new[] { "space-maintainer-tooth","space-maintainer-quadrant","space-maintainer-statuses" }) {
				Assert.AreEqual(0,GetBars(BuildScene(sceneName,ChartScenes.SizeModule,out _)).Count,sceneName);
			}
			//Two missing teeth in a range: one bar across both spaces.
			listBars=GetBars(BuildScene("space-maintainer-range-two-missing",ChartScenes.SizeModule,out ToothChartData tcDataRange));
			Assert.AreEqual(2,listBars.Count);
			ToothSceneItem3D bar45=listBars.Single(x => x.Source==ToothSceneBuilder.SourceSpaceMaintainer+"4,5");
			Assert.AreEqual(ToothGraphic.GetWidth("4")+ToothGraphic.GetWidth("5"),bar45.Transform.M11,0.0001);
			Assert.AreEqual((tcDataRange.GetTransX("4")+tcDataRange.GetTransX("5"))/2f,bar45.Transform.M41,0.0001);
			Assert.IsTrue(listBars.Any(x => x.Source==ToothSceneBuilder.SourceSpaceMaintainer+"12,13"));
			//A quadrant: only its missing teeth.
			listBars=GetBars(BuildScene("space-maintainer-quadrant-missing",ChartScenes.SizeModule,out _));
			CollectionAssert.AreEquivalent(new[] { ToothSceneBuilder.SourceSpaceMaintainer+"12,13",ToothSceneBuilder.SourceSpaceMaintainer+"28,29" },
				listBars.Select(x => x.Source).ToArray());
			//A lost primary molar over an unerupted premolar: the space is empty.
			listBars=GetBars(BuildScene("space-maintainer-primary",ChartScenes.SizeModule,out _));
			CollectionAssert.AreEquivalent(new[] { ToothSceneBuilder.SourceSpaceMaintainer+"20",ToothSceneBuilder.SourceSpaceMaintainer+"29" },
				listBars.Select(x => x.Source).ToArray());
			//Never over a present tooth: a pontic fills 9, 25 and 26 are there; 20's bar is there, behind the moved 19.
			listBars=GetBars(BuildScene("space-maintainer-not-over-teeth",ChartScenes.SizeModule,out _));
			CollectionAssert.AreEqual(new[] { ToothSceneBuilder.SourceSpaceMaintainer+"20" },listBars.Select(x => x.Source).ToArray());
			//In ortho mode the bar is on the moved row.
			ToothScene sceneOrtho=BuildScene("ortho-mode-other-work",ChartScenes.SizeModule,out _);
			Assert.AreEqual(24f-ToothSceneBuilder.OrthoFacialShiftMm,GetBars(sceneOrtho).Single().Transform.M42,0.0001);
		}

		private static List<ToothSceneItem3D> GetBars(ToothScene toothScene) {
			return toothScene.Items3D.Where(x => x.Source.StartsWith(ToothSceneBuilder.SourceSpaceMaintainer)).ToList();
		}

		[TestMethod]
		public void HelixBox_IsAUnitBox_WhoseFrontFacesFaceOut() {
			StaRunner.Run(() => {
				ToothMeshCache toothMeshCache=new ToothMeshCache();
				MeshGeometry3D mesh=toothMeshCache.GetMesh(ToothSceneShapes.Box,0,false);
				MeshGeometry3D meshReversed=toothMeshCache.GetMesh(ToothSceneShapes.Box,0,true);
				Assert.IsNotNull(mesh);
				Assert.IsTrue(mesh.IsFrozen && meshReversed.IsFrozen);
				Assert.IsNull(toothMeshCache.GetMesh(ToothSceneShapes.Box,1,false));
				Assert.AreEqual(24,mesh.Positions.Count,"Six faces of four vertices (MeshBuilder.AddBox).");
				Assert.AreEqual(mesh.Positions.Count,mesh.Normals.Count,"Every vertex has a normal.");
				Assert.AreEqual(36,mesh.TriangleIndices.Count);
				foreach(Point3D point in mesh.Positions) {
					Assert.AreEqual(0.5,Math.Abs(point.X),1e-6);
					Assert.AreEqual(0.5,Math.Abs(point.Y),1e-6);
					Assert.AreEqual(0.5,Math.Abs(point.Z),1e-6);
				}
				for(int i=0;i+2<mesh.TriangleIndices.Count;i+=3) {
					Point3D a=mesh.Positions[mesh.TriangleIndices[i]];
					Point3D b=mesh.Positions[mesh.TriangleIndices[i+1]];
					Point3D c=mesh.Positions[mesh.TriangleIndices[i+2]];
					Vector3D normal=Vector3D.CrossProduct(b-a,c-a);
					Vector3D outward=new Vector3D((a.X+b.X+c.X)/3,(a.Y+b.Y+c.Y)/3,(a.Z+b.Z+c.Z)/3);
					Assert.IsTrue(Vector3D.DotProduct(normal,outward)>0,"Triangle "+(i/3)+" runs counter-clockwise seen from outside.");
					Assert.IsTrue(Vector3D.DotProduct(mesh.Normals[mesh.TriangleIndices[i]],outward)>0,"Its vertex normal points out.");
					Assert.AreEqual(mesh.TriangleIndices[i],meshReversed.TriangleIndices[i]);
					Assert.AreEqual(mesh.TriangleIndices[i+1],meshReversed.TriangleIndices[i+2]);
					Assert.AreEqual(-mesh.Normals[mesh.TriangleIndices[i]],meshReversed.Normals[meshReversed.TriangleIndices[i]]);
				}
				Assert.AreEqual(1,toothMeshCache.CountMeshSets);
			});
		}

		[TestMethod]
		public void SpaceMaintainerBar_IsDrawnInItsColourExactly_AndTheBracketIsLit() {
			WpfSession.RequireRendering();//REL-0.1-FIX: inconclusive where WPF draws nothing (a disconnected session)
			StaRunner.Run(() => {
				using ToothChartWrapper wrapper=WpfCharts.CreateWrapper(ChartScenes.SizeModule.Size);
				ToothChartRelay relay=WpfCharts.CreateRelay(wrapper);
				using Bitmap bitmap=WpfCharts.Draw(relay,ChartScenes.Get("space-maintainer-missing-tooth"),ChartColorSet.Module);
				Assert.AreEqual(ToothChartEngine.Wpf3D,wrapper.Engine);
				ToothScene toothScene=ToothSceneBuilder.Build(wrapper.TcData);
				PointF pointBar=toothScene.ProjectToImage(ChartMatrix.Identity,wrapper.TcData.GetTransX("9"),24f,0);
				Color color=bitmap.GetPixel((int)pointBar.X,(int)pointBar.Y);
				Color colorTp=ChartScenes.TP.Dark;
				Assert.IsTrue(Math.Abs(color.R-colorTp.R)<=2 && Math.Abs(color.G-colorTp.G)<=2 && Math.Abs(color.B-colorTp.B)<=2,
					"The bar's middle is the TP colour itself (unlit, as Open Dental's 198,0,0): "+color);
				using Bitmap bitmapBracket=WpfCharts.Draw(relay,ChartScenes.Get("ortho-hardware-normal-layout"),ChartColorSet.Module);
				ToothScene sceneBracket=ToothSceneBuilder.Build(wrapper.TcData);
				//The base between the left and right wings, below the slot: lit silver, about 0.93 of 192 (ortho9: B4B3B3).
				ChartMatrix worldBracket=ToothSceneBuilder.GetBracketWorld(wrapper.TcData,wrapper.TcData.ListToothGraphics["3"]);
				PointF pointBase=sceneBracket.ProjectToImage(worldBracket,0,-0.7f,ToothSceneBuilder.BracketBaseFrontMm);
				Color colorBase=bitmapBracket.GetPixel((int)pointBase.X,(int)pointBase.Y);
				Assert.IsTrue(colorBase.R>170 && colorBase.R<187 && Math.Abs(colorBase.R-colorBase.B)<=3,"The bracket's lit, matte base: "+colorBase);
				//A tie wing: 0.7 of the colour, lit (ortho9: 124).
				PointF pointWing=sceneBracket.ProjectToImage(worldBracket,0.775f,-0.7f,ToothSceneBuilder.BracketBaseFrontMm+ToothSceneBuilder.BracketWingDepthMm);
				Color colorWing=bitmapBracket.GetPixel((int)pointWing.X,(int)pointWing.Y);
				Assert.IsTrue(colorWing.R>110 && colorWing.R<140,"A tie wing: "+colorWing);
			});
		}

		[TestMethod]
		public void Wrapper_ResetTeethClearsTheHardware_KeepsOrthoMode_AndCopyIsDeep() {
			StaRunner.Run(() => {
				using ToothChartWrapper wrapper=new ToothChartWrapper();
				wrapper.Size=ChartScenes.SizeModule.Size;
				ToothChartRelay relay=WpfCharts.CreateRelay(wrapper);
				relay.ResetTeeth();
				relay.SetOrthoMode(true);
				Assert.IsTrue(wrapper.IsOrthoMode);
				relay.SetBracket("8",Color.Silver);
				relay.AddOrthoWireInBracket("8",Color.Silver);
				relay.AddOrthoWireBetweenBrackets("8","9",Color.Silver);
				relay.AddOrthoElastic("6","27",Color.Yellow);
				relay.AddOrthoElastic("6","XX",Color.Yellow);//not a tooth: ignored
				relay.SetRetainedRoot("9",Color.Red);
				relay.SetSpaceMaintainer("K",Color.Blue);
				ToothChartData tcData=wrapper.TcData;
				Assert.AreEqual(3,tcData.ListOrthoWires.Count);
				Assert.AreEqual(ToothChartOrthoWireKind.Elastic,tcData.ListOrthoWires[2].Kind);
				Assert.IsTrue(tcData.ListToothGraphics["8"].HasBracket && tcData.ListToothGraphics["9"].IsRetainedRoot && tcData.ListToothGraphics["K"].IsSpaceMaintainer);
				ToothChartData tcDataCopy=tcData.Copy();
				tcData.ListOrthoWires[0].ToothIDstart="7";
				Assert.AreEqual("8",tcDataCopy.ListOrthoWires[0].ToothIDstart,"Copy copies the wires.");
				Assert.IsTrue(tcDataCopy.IsOrthoMode);
				relay.ResetTeeth();
				Assert.AreEqual(0,tcData.ListOrthoWires.Count);
				Assert.IsFalse(tcData.ListToothGraphics["8"].HasBracket || tcData.ListToothGraphics["9"].IsRetainedRoot || tcData.ListToothGraphics["K"].IsSpaceMaintainer);
				Assert.IsTrue(wrapper.IsOrthoMode,"The callers set ortho mode before each refill; ResetTeeth keeps it.");
				relay.SetOrthoMode(false);
				Assert.IsFalse(wrapper.IsOrthoMode);
			});
		}
	}
}
