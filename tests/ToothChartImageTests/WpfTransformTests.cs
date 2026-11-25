// Transform and layer tests of the WPF chart (package TC-1, docs/specs/wpf-tooth-chart-plan.md sections 5.1 to 5.3): sample vertices through WPF's
// camera chain land within 0.001 mm of the DirectX chain's screen points, which were computed with SharpDX.Mathematics and recorded before TC-9a
// retired the DirectX chart (Baselines\legacy-numbers\directx-transforms.txt), and a marker vertex lands on the same pixel in the 3D layer and in
// the 2D layer.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Media.Media3D;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental;
using OpenPmsDentalBusiness;
using SparksToothChart;
using SparksToothChart.Wpf3D;

namespace ToothChartImageTests {
	[TestClass]
	public class WpfTransformTests {
		///<summary>The scenes of the transform test: the normal chart with movements in all four quadrants and implants, at the module size and at a
		///wide size, and the perio chart with implants. Each with its data, which outlives its wrapper (disposing a wrapper leaves its data alone).</summary>
		private static List<KeyValuePair<ToothChartData,ToothScene>> BuildTransformScenes() {
			List<KeyValuePair<ToothChartData,ToothScene>> listScenes=new List<KeyValuePair<ToothChartData,ToothScene>>();
			foreach(Size size in new[] { ChartScenes.SizeModule.Size,ChartScenes.SizeSheetWide.Size,ChartScenes.SizeSheetTall.Size }) {
				using(ToothChartWrapper wrapper=new ToothChartWrapper()) {
					wrapper.Size=size;
					WpfMeshTests.ApplyMovementsAndImplants(WpfCharts.CreateRelay(wrapper));
					listScenes.Add(new KeyValuePair<ToothChartData,ToothScene>(wrapper.TcData,ToothSceneBuilder.Build(wrapper.TcData)));
				}
			}
			using(ToothChartWrapper wrapperPerio=new ToothChartWrapper()) {
				wrapperPerio.Size=ChartScenes.SizePerio.Size;
				ToothChartRelay relayPerio=WpfCharts.CreateRelay(wrapperPerio);
				relayPerio.SetMissing("3");
				relayPerio.SetImplant("3",Color.Gainsboro);
				relayPerio.SetMissing("30");
				relayPerio.SetImplant("30",Color.Gainsboro);
				wrapperPerio.TcData.PerioMode=true;
				listScenes.Add(new KeyValuePair<ToothChartData,ToothScene>(wrapperPerio.TcData,ToothSceneBuilder.Build(wrapperPerio.TcData)));
			}
			return listScenes;
		}

		[TestMethod]
		public void SampleVertices_LandWithinAThousandthOfAMm_InWpfAndInDirectX() {
			//The DirectX side is the recording of the DirectX chain (SharpDX.Matrix and Vector3.Project, as ToothChartDirectX computed it), made before
			//TC-9a retired the DirectX chart and the SharpDX.Mathematics reference (RecordDirectXTransforms): every sample vertex's screen point.
			Dictionary<string,PointF> dictRecorded=ReadRecordedDirectXPoints();
			StaRunner.Run(() => {
				int countPoints=0;
				double maxDifferenceMm=0;
				string worst="";
				HashSet<string> hashSetKeys=new HashSet<string>();
				int indexScene=0;
				foreach(KeyValuePair<ToothChartData,ToothScene> pair in BuildTransformScenes()) {
					ToothChartData tcData=pair.Key;
					ToothScene toothScene=pair.Value;
					Viewport3DVisual viewport3DVisual=new Viewport3DVisual();
					viewport3DVisual.Camera=ToothChartRenderer.CreateCamera(toothScene);
					viewport3DVisual.Viewport=new System.Windows.Rect(0,0,toothScene.Size.Width,toothScene.Size.Height);
					foreach(ToothSceneItem3D item in toothScene.Items3D) {
						ToothGraphic toothGraphicModel=tcData.ListToothGraphics[item.ToothId];
						ModelVisual3D modelVisual3D=new ModelVisual3D();
						modelVisual3D.Transform=new MatrixTransform3D(ToothChartRenderer.ToMatrix3D(item.Transform));
						viewport3DVisual.Children.Add(modelVisual3D);
						GeneralTransform3DTo2D generalTransform3DTo2D=modelVisual3D.TransformToAncestor(viewport3DVisual);
						//Sample vertices: every 37th of the model.
						for(int v=item.GroupIndex%37;v<toothGraphicModel.VertexNormals.Count;v+=37) {
							Vertex3f vertex=toothGraphicModel.VertexNormals[v].Vertex;
							string key=GetTransformKey(indexScene,toothScene,item,v);
							Assert.IsTrue(dictRecorded.TryGetValue(key,out PointF pointDirectX),"No recorded DirectX point for "+key.Replace("\t"," "));
							Assert.IsTrue(hashSetKeys.Add(key),"A sample vertex twice: "+key.Replace("\t"," "));
							Assert.IsTrue(generalTransform3DTo2D.TryTransform(new Point3D(vertex.X,vertex.Y,vertex.Z),out System.Windows.Point pointWpf));
							//WPF's pixel centres are on halves, DirectX 9's on whole numbers (ToothScene.CameraCenterX).
							double dxPixels=(pointWpf.X-0.5)-pointDirectX.X;
							double dyPixels=(pointWpf.Y-0.5)-pointDirectX.Y;
							double differenceMm=Math.Sqrt(dxPixels*dxPixels+dyPixels*dyPixels)/tcData.ScaleMmToPix;
							if(differenceMm>maxDifferenceMm) {
								maxDifferenceMm=differenceMm;
								worst=item.Source+" "+item.ToothId+" vertex "+v+" at "+toothScene.Size.Width+"x"+toothScene.Size.Height+": WPF "+pointWpf
									+", DirectX "+pointDirectX.X+","+pointDirectX.Y;
							}
							countPoints++;
						}
						viewport3DVisual.Children.Remove(modelVisual3D);
					}
					indexScene++;
				}
				Console.WriteLine(countPoints+" vertices compared with the recorded DirectX chain; the largest difference is "+maxDifferenceMm.ToString("0.000000")
					+" mm ("+worst+").");
				Assert.IsTrue(countPoints>5000,"Compared "+countPoints);
				Assert.AreEqual(dictRecorded.Count,countPoints,"Every recorded sample vertex is compared, and no other: the scenes are the recorded ones.");
				Assert.IsTrue(maxDifferenceMm<0.001,"WPF and DirectX differ by "+maxDifferenceMm+" mm: "+worst);
			});
		}

		///<summary>The recording of the DirectX chain (Baselines\legacy-numbers\directx-transforms.txt), made by RecordDirectXTransforms before TC-9a:
		///the key of each sample vertex (GetTransformKey) and its DirectX screen point.</summary>
		public static Dictionary<string,PointF> ReadRecordedDirectXPoints() {
			string path=Path.Combine(TestPaths.BaselinesLegacyNumbers,FileDirectXTransforms);
			Assert.IsTrue(File.Exists(path),"The recording of the DirectX chain is missing: "+path);
			Dictionary<string,PointF> dictRecorded=new Dictionary<string,PointF>();
			foreach(string line in File.ReadAllLines(path)) {
				if(line.StartsWith("#") || line.Trim()=="") {
					continue;
				}
				string[] arrayFields=line.Split('\t');
				Assert.AreEqual(7,arrayFields.Length,"A recorded line: "+line);
				string key=string.Join("\t",arrayFields.Take(5));
				dictRecorded.Add(key,new PointF(float.Parse(arrayFields[5],CultureInfo.InvariantCulture),float.Parse(arrayFields[6],CultureInfo.InvariantCulture)));
			}
			return dictRecorded;
		}

		///<summary>TC-9a: the switch with which RecordDirectXTransforms wrote the recording, while SharpDX.Mathematics was still referenced.</summary>
		public const string EnvRecordDirectXTransforms="OPMS_RECORD_DIRECTX_TRANSFORMS";
		public const string FileDirectXTransforms="directx-transforms.txt";

		///<summary>The key of a sample vertex in the recording: the scene's index and size, the item's source and group, and the vertex.</summary>
		public static string GetTransformKey(int indexScene,ToothScene toothScene,ToothSceneItem3D item,int vertex) {
			return indexScene+"\t"+toothScene.Size.Width+"x"+toothScene.Size.Height+"\t"+item.Source+"\t"+item.GroupIndex+"\t"+vertex;
		}

		///<summary>Recorded Baselines\legacy-numbers\directx-transforms.txt (24,254 sample vertices) with OPMS_RECORD_DIRECTX_TRANSFORMS=1 on the build
		///PC, from the DirectX chain computed with SharpDX.Mathematics (copies of ToothChartDirectX's matrix code: Render, DrawScene, DrawFacialView,
		///DrawOcclusalView, DrawImplant, DrawScenePerio, DrawPerioRow, ToothTranslationMatrix, ToothRotationMatrix). The recording is kept (O-32);
		///it cannot be made again.</summary>
		[TestMethod]
		[Ignore("The DirectX engine was retired in TC-9a, with this project's SharpDX.Mathematics reference: the DirectX chain cannot be computed again. "
			+"The recording it made (Baselines\\legacy-numbers\\directx-transforms.txt) is kept, and SampleVertices_... compares with it.")]
		public void RecordDirectXTransforms() {
			Assert.Inconclusive("Retired in TC-9a.");
		}

		[TestMethod]
		public void MarkerVertex_LandsOnTheSamePixel_InThe3DLayerAndThe2DLayer() {
			WpfSession.RequireRendering();//REL-0.1-FIX: inconclusive where WPF draws nothing (a disconnected session)
			StaRunner.Run(() => {
				using ToothChartWrapper wrapper=new ToothChartWrapper();
				wrapper.Size=ChartScenes.SizeModule.Size;
				ToothChartRelay relay=WpfCharts.CreateRelay(wrapper);
				WpfMeshTests.ApplyMovementsAndImplants(relay);
				ToothScene toothScene=ToothSceneBuilder.Build(wrapper.TcData);
				Viewport3DVisual viewport3DVisual=new Viewport3DVisual();
				viewport3DVisual.Camera=ToothChartRenderer.CreateCamera(toothScene);
				viewport3DVisual.Viewport=new System.Windows.Rect(0,0,toothScene.Size.Width,toothScene.Size.Height);
				int countPoints=0;
				foreach(ToothSceneItem3D item in toothScene.Items3D.Where((x,i) => i%5==0)) {
					ToothGraphic toothGraphicModel=wrapper.TcData.ListToothGraphics[item.ToothId];
					ModelVisual3D modelVisual3D=new ModelVisual3D();
					modelVisual3D.Transform=new MatrixTransform3D(ToothChartRenderer.ToMatrix3D(item.Transform));
					viewport3DVisual.Children.Add(modelVisual3D);
					GeneralTransform3DTo2D generalTransform3DTo2D=modelVisual3D.TransformToAncestor(viewport3DVisual);
					for(int v=0;v<toothGraphicModel.VertexNormals.Count;v+=11) {
						Vertex3f vertex=toothGraphicModel.VertexNormals[v].Vertex;
						System.Windows.Point point3D=generalTransform3DTo2D.Transform(new Point3D(vertex.X,vertex.Y,vertex.Z));
						PointF point2D=toothScene.ProjectToImage(item.Transform,vertex.X,vertex.Y,vertex.Z);
						Assert.AreEqual(point3D.X,point2D.X,0.002,item.Source+" vertex "+v);
						Assert.AreEqual(point3D.Y,point2D.Y,0.002,item.Source+" vertex "+v);
						countPoints++;
					}
					viewport3DVisual.Children.Remove(modelVisual3D);
				}
				Assert.IsTrue(countPoints>1000,"Compared "+countPoints);
				//And in rendered pixels: tooth 8's facial view in the 3D layer alone, and its projected triangles as one 2D silhouette alone, both white
				//on black, cover the same box to the pixel.
				List<ToothSceneItem3D> listItems8=toothScene.Items3D.Where(x => x.Source=="8 facial").Select(x => new ToothSceneItem3D(x.ToothId,
					x.GroupIndex,x.GroupType,Color.White,x.Specular,x.Transform,x.Source)).ToList();
				Assert.IsTrue(listItems8.Count>3);
				List<PointF[]> listTriangles=new List<PointF[]>();
				foreach(ToothSceneItem3D item in listItems8) {
					ToothGraphic toothGraphicModel=wrapper.TcData.ListToothGraphics[item.ToothId];
					int[] arrayTriangles=ToothMeshWinding.GetWoundTriangles(toothGraphicModel,item.GroupIndex);
					for(int i=0;i+2<arrayTriangles.Length;i+=3) {
						listTriangles.Add(new[] { arrayTriangles[i],arrayTriangles[i+1],arrayTriangles[i+2] }.Select(x => {
							Vertex3f vertex=toothGraphicModel.VertexNormals[x].Vertex;
							return toothScene.ProjectToImage(item.Transform,vertex.X,vertex.Y,vertex.Z);
						}).ToArray());
					}
				}
				ToothScene toothScene3D=new ToothScene(toothScene.Size,Color.Black,false,toothScene.WidthProjection,toothScene.HeightProjection,listItems8,
					new List<ToothSceneItem2D>());
				ToothScene toothScene2D=new ToothScene(toothScene.Size,Color.Black,false,toothScene.WidthProjection,toothScene.HeightProjection,
					new List<ToothSceneItem3D>(),new List<ToothSceneItem2D> { ToothSceneItem2D.CreatePolygons(Color.White,listTriangles,"silhouette of 8") });
				Rectangle rectangle3D=GetLitBounds(ToothChartRenderer.Instance.Render(toothScene3D,null));
				Rectangle rectangle2D=GetLitBounds(ToothChartRenderer.Instance.Render(toothScene2D,null));
				Console.WriteLine("Tooth 8 facial: 3D layer "+rectangle3D+", 2D layer "+rectangle2D);
				Assert.IsTrue(rectangle3D.Width>10 && rectangle3D.Height>20,"The tooth was drawn: "+rectangle3D);
				Assert.IsTrue(Math.Abs(rectangle3D.Left-rectangle2D.Left)<=1 && Math.Abs(rectangle3D.Right-rectangle2D.Right)<=1
					&& Math.Abs(rectangle3D.Top-rectangle2D.Top)<=1 && Math.Abs(rectangle3D.Bottom-rectangle2D.Bottom)<=1,
					"The layers are aligned: 3D "+rectangle3D+", 2D "+rectangle2D);
			});
		}

		///<summary>The box of the pixels brighter than a fifth of white.</summary>
		private static Rectangle GetLitBounds(ToothChartRenderResult result) {
			int left=int.MaxValue;
			int top=int.MaxValue;
			int right=-1;
			int bottom=-1;
			for(int y=0;y<result.Size.Height;y++) {
				for(int x=0;x<result.Size.Width;x++) {
					int i=(y*result.Size.Width+x)*4;
					int max=Math.Max(result.PixelsBgra[i],Math.Max(result.PixelsBgra[i+1],result.PixelsBgra[i+2]));
					if(max<=51) {
						continue;
					}
					left=Math.Min(left,x);
					top=Math.Min(top,y);
					right=Math.Max(right,x);
					bottom=Math.Max(bottom,y);
				}
			}
			if(right<0) {
				return Rectangle.Empty;
			}
			return Rectangle.FromLTRB(left,top,right+1,bottom+1);
		}
	}
}
