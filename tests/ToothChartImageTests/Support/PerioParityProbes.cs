// The parity probes of the WPF chart's graphical perio chart against today's DirectX pictures (package TC-7, docs/specs/wpf-tooth-chart-plan.md
// section 7): the teeth of the four rows, the probing bars, the bleeding and suppuration droplets, the furcation marks, the GM, CAL and MGJ lines
// and the legend's marks, by colour; the numbers, the legend's words and the mobility numbers by the place of their ink.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using OpenPmsDentalBusiness;
using SparksToothChart;
using SparksToothChart.Wpf3D;

namespace ToothChartImageTests {
	///<summary>One colour probe of a perio picture.</summary>
	public class PerioProbePoint {
		///<summary>"tooth", "implant", "probing", "bleeding", "suppuration", "furcation", "line" or "legend".</summary>
		public string Kind;
		///<summary>What is probed, for messages: for example "3 maxillary facial row" or "probing 3 MB".</summary>
		public string Name;
		///<summary>In image coordinates with pixel centres on halves (ToothScene.ProjectToImage).</summary>
		public PointF Point;
		///<summary>1: the mean of the 3x3 pixels (a tooth's surface). 0: the one pixel (a mark or a line).</summary>
		public int SampleRadius;
		///<summary>False for a missing tooth: both pictures must agree on tooth versus background there.</summary>
		public bool IsToothDrawn=true;
		///<summary>For a line: its colour. The probe counts only where DirectX shows the line on top there (a later line or mark may cover it in both
		///pictures, and a 2-pixel line drawn without smoothing may miss the probe's pixel).</summary>
		public Color? ColorLine;
	}

	///<summary>One probe of a perio picture against the DirectX recording.</summary>
	public class PerioProbeOutcome {
		public string SceneName;
		public PerioProbePoint Probe;
		public Color ColorDirectX;
		public Color ColorWpf;
		public string Failure;
		public PerioAllowance Allowance;

		public override string ToString() {
			return SceneName+" "+Probe.Kind+" "+Probe.Name+" at "+Probe.Point.X.ToString("0.0",CultureInfo.InvariantCulture)+","
				+Probe.Point.Y.ToString("0.0",CultureInfo.InvariantCulture)+": DirectX "+ParityProbes.Describe(ColorDirectX)+", WPF "+ParityProbes.Describe(ColorWpf)
				+(Failure==null?"":" ("+Failure+")")+(Allowance==null?"":" [allowed: "+Allowance.Reason+"]");
		}
	}

	///<summary>A reviewed exception of the perio probes, with its reason. SceneName may be "*"; the probe's Name must start with NameStart.</summary>
	public class PerioAllowance {
		public string SceneName;
		public string Kind;
		public string NameStart;
		public string Reason;

		public PerioAllowance(string sceneName,string kind,string nameStart,string reason) {
			SceneName=sceneName;
			Kind=kind;
			NameStart=nameStart;
			Reason=reason;
		}

		public bool Covers(PerioProbeOutcome outcome) {
			return (SceneName=="*" || SceneName==outcome.SceneName) && Kind==outcome.Probe.Kind && outcome.Probe.Name.StartsWith(NameStart,StringComparison.Ordinal);
		}

		public override string ToString() {
			return SceneName+" "+Kind+" "+NameStart+": "+Reason;
		}
	}

	///<summary>The perio chart's parity probes. Every point is computed as the chart draws the item: the row's orientation (ToothSceneBuilder, DirectX's
	///matrices), the tooth's place and the perio geometry of ToothChartData, projected with the chart's own projection. A sample passes by section 7's
	///rules (ParityProbes.GetFailure): lightness within 20 points and, for a coloured sample, the hue within 12 degrees; at a missing tooth both pictures
	///agree on tooth versus background.</summary>
	public static class PerioParityProbes {
		///<summary>A line probe counts where DirectX's pixel is within this distance (RGB) of the line's colour.</summary>
		public const double MaxLineColourDistance=60;

		///<summary>The reviewed exceptions. Empty while every probe agrees.</summary>
		public static readonly List<PerioAllowance> Allowances=new List<PerioAllowance>();

		///<summary>The rows as ToothSceneBuilder.AddPerioScene draws them, with whether each is maxillary and lingual.</summary>
		public static readonly ToothSceneBuilder.PerioRow[] ArrayRows={ ToothSceneBuilder.PerioRow.MaxillaryFacial,ToothSceneBuilder.PerioRow.MaxillaryLingual,
			ToothSceneBuilder.PerioRow.MandibularLingual,ToothSceneBuilder.PerioRow.MandibularFacial };

		public static bool IsMaxillary(ToothSceneBuilder.PerioRow perioRow) {
			return perioRow==ToothSceneBuilder.PerioRow.MaxillaryFacial || perioRow==ToothSceneBuilder.PerioRow.MaxillaryLingual;
		}

		public static bool IsLingual(ToothSceneBuilder.PerioRow perioRow) {
			return perioRow==ToothSceneBuilder.PerioRow.MaxillaryLingual || perioRow==ToothSceneBuilder.PerioRow.MandibularLingual;
		}

		///<summary>The probe points of the perio chart as tcData holds it, at its control size.</summary>
		public static List<PerioProbePoint> GetPoints(ToothChartData tcData) {
			ToothScene scene=ToothSceneBuilder.Build(tcData);
			float baseY=ToothSceneBuilder.GetPerioBaseY(tcData);
			List<PerioProbePoint> listPoints=new List<PerioProbePoint>();
			foreach(ToothSceneBuilder.PerioRow perioRow in ArrayRows) {
				bool isMaxillary=IsMaxillary(perioRow);
				bool isLingual=IsLingual(perioRow);
				string rowName=GetRowName(perioRow);
				ChartMatrix orientation=ToothSceneBuilder.GetPerioRowOrientation(perioRow,baseY);
				for(int i=1;i<=32;i++) {
					string toothId=i.ToString();
					if(ToothGraphic.IsMaxillary(toothId)!=isMaxillary) {
						continue;
					}
					ToothGraphic toothGraphic=tcData.ListToothGraphics[toothId];
					ChartMatrix world=ChartMatrix.Translation(tcData.GetTransX(toothId),0,0)*orientation;
					AddToothPoint(listPoints,scene,toothGraphic,world,isLingual,toothId+" "+rowName);
					if(toothGraphic.IsImplant) {
						AddImplantPoint(listPoints,scene,tcData,toothGraphic,world,"implant of "+toothId+" "+rowName);
					}
					PerioSurf[] arraySurfs=isLingual
						? new PerioSurf[] { PerioSurf.DL,PerioSurf.L,PerioSurf.ML }
						: new PerioSurf[] { PerioSurf.DB,PerioSurf.B,PerioSurf.MB };
					foreach(PerioSurf perioSurf in arraySurfs) {
						AddSitePoints(listPoints,scene,tcData,world,i,perioSurf,isMaxillary,toothId);
					}
				}
				AddLinePoints(listPoints,scene,tcData,orientation,PerioSequenceType.GingMargin,isMaxillary,isLingual,tcData.ColorGingivalMargin,"gingival margin "+rowName);
				AddLinePoints(listPoints,scene,tcData,orientation,PerioSequenceType.CAL,isMaxillary,isLingual,tcData.ColorCAL,"CAL "+rowName);
				AddLinePoints(listPoints,scene,tcData,orientation,PerioSequenceType.MGJ,isMaxillary,isLingual,tcData.ColorMGJ,"MGJ "+rowName);
			}
			AddLegendPoints(listPoints,scene,tcData,baseY);
			return listPoints;
		}

		public static string GetRowName(ToothSceneBuilder.PerioRow perioRow) {
			switch(perioRow) {
				case ToothSceneBuilder.PerioRow.MaxillaryFacial:
					return "maxillary facial row";
				case ToothSceneBuilder.PerioRow.MaxillaryLingual:
					return "maxillary lingual row";
				case ToothSceneBuilder.PerioRow.MandibularLingual:
					return "mandibular lingual row";
				default:
					return "mandibular facial row";
			}
		}

		///<summary>The centre of the surface the row shows: the facial (B, or F on an anterior tooth) in a facial row, the lingual (L) in a lingual row,
		///whose mirrored transform shows the tooth from behind; the enamel when the model has no such group.</summary>
		private static void AddToothPoint(List<PerioProbePoint> listPoints,ToothScene scene,ToothGraphic toothGraphic,ChartMatrix world,bool isLingual,string name) {
			ToothGroupType groupType=isLingual?ToothGroupType.L:(ToothGraphic.IsAnterior(toothGraphic.ToothID)?ToothGroupType.F:ToothGroupType.B);
			ToothGroup group=toothGraphic.GetGroup(groupType) ?? toothGraphic.GetGroup(ToothGroupType.Enamel);
			if(group==null) {
				return;
			}
			PointF? pointCentre=GetVisibleCentre(scene,toothGraphic,toothGraphic.Groups.IndexOf(group),world);
			if(pointCentre==null) {
				return;
			}
			PerioProbePoint probe=Add(listPoints,"tooth",name,pointCentre.Value,1);
			probe.IsToothDrawn=toothGraphic.Visible || (toothGraphic.IsCrown && toothGraphic.IsImplant) || toothGraphic.IsPontic;
		}

		///<summary>The implant's body, upside down on the maxilla (DrawImplant).</summary>
		private static void AddImplantPoint(List<PerioProbePoint> listPoints,ToothScene scene,ToothChartData tcData,ToothGraphic toothGraphic,ChartMatrix world,
			string name)
		{
			ToothGraphic implantGraphic=tcData.ListToothGraphics["implant"];
			if(implantGraphic==null) {
				return;
			}
			if(ToothGraphic.IsMaxillary(toothGraphic.ToothID)) {
				world=ChartMatrix.RotationZ((float)Math.PI)*world;
			}
			for(int g=0;g<implantGraphic.Groups.Count;g++) {
				if(!implantGraphic.Groups[g].Visible || implantGraphic.Groups[g].GroupType==ToothGroupType.Buildup || implantGraphic.Groups[g].Faces.Count==0) {
					continue;
				}
				PointF? pointCentre=GetVisibleCentre(scene,implantGraphic,g,world);
				if(pointCentre!=null) {
					Add(listPoints,"implant",name,pointCentre.Value,1);
					return;
				}
			}
		}

		///<summary>A site's probing bar (its middle, when it is at least 1 mm long), its bleeding and suppuration droplets (their middles) and its
		///furcation mark (grade 3: the middle of the filled triangle; grades 2 and 3: the middle of its base).</summary>
		private static void AddSitePoints(List<PerioProbePoint> listPoints,ToothScene scene,ToothChartData tcData,ChartMatrix world,int intTooth,PerioSurf perioSurf,
			bool isMaxillary,string toothId)
		{
			string site=toothId+" "+perioSurf;
			Color colorBar;
			LineSimple lineBar=tcData.GetProbingLine(intTooth,perioSurf,out colorBar);
			if(lineBar!=null && Math.Abs(lineBar.Vertices[1].Y-lineBar.Vertices[0].Y)>=1f) {
				Add(listPoints,"probing",site,scene.ProjectToImage(world,lineBar.Vertices[0].X,(lineBar.Vertices[0].Y+lineBar.Vertices[1].Y)/2f,0),0);
			}
			foreach(bool isBleeding in new[] { true,false }) {
				PointF pointDroplet=tcData.GetBleedingOrSuppuration(intTooth,perioSurf,isBleeding);
				if(pointDroplet.X==0 && pointDroplet.Y==0) {
					continue;
				}
				//The droplet's middle: 0.27 mm above its centre point, below it on the mandible (the droplet is flipped there).
				float yMiddle=pointDroplet.Y+(isMaxillary?0.27f:-0.27f);
				Add(listPoints,isBleeding?"bleeding":"suppuration",site,scene.ProjectToImage(world,pointDroplet.X,yMiddle,0),0);
			}
			int furcationValue=tcData.GetFurcationValue(intTooth,perioSurf);
			if(furcationValue>=2 && furcationValue<=3) {
				PointF pointTip=tcData.GetFurcationPos(intTooth,perioSurf);
				float sign=isMaxillary?1:-1;
				float height=(float)(2f*Math.Sqrt(3)/2f);
				Add(listPoints,"furcation",site+" base",scene.ProjectToImage(world,pointTip.X,pointTip.Y+sign*height,0),0);
				if(furcationValue==3) {
					Add(listPoints,"furcation",site+" fill",scene.ProjectToImage(world,pointTip.X,pointTip.Y+sign*height*2f/3f,0),0);
				}
			}
		}

		///<summary>The middle of every segment of the row's GM, CAL or MGJ lines.</summary>
		private static void AddLinePoints(List<PerioProbePoint> listPoints,ToothScene scene,ToothChartData tcData,ChartMatrix orientation,
			PerioSequenceType perioSequenceType,bool isMaxillary,bool isLingual,Color color,string name)
		{
			List<LineSimple> listLines=tcData.GetHorizontalLines(perioSequenceType,isMaxillary,!isLingual);
			for(int l=0;l<listLines.Count;l++) {
				for(int v=0;v+1<listLines[l].Vertices.Count;v++) {
					Vertex3f vertex1=listLines[l].Vertices[v];
					Vertex3f vertex2=listLines[l].Vertices[v+1];
					PerioProbePoint probe=Add(listPoints,"line",name+" "+l+"."+v,scene.ProjectToImage(orientation,(vertex1.X+vertex2.X)/2f,(vertex1.Y+vertex2.Y)/2f,0),0);
					probe.ColorLine=color;
				}
			}
		}

		///<summary>The legend's marks (DrawPerioLegend at -60 mm and baseY-63 mm): the two droplets, the two probing bars, the three line samples and the
		///filled furcation 3.</summary>
		private static void AddLegendPoints(List<PerioProbePoint> listPoints,ToothScene scene,ToothChartData tcData,float baseY) {
			ChartMatrix world=ChartMatrix.Translation(-60f,baseY-63f,0);
			Add(listPoints,"legend","bleeding",scene.ProjectToImage(world,18f,-1.5f+0.27f,0),0);
			Add(listPoints,"legend","suppuration",scene.ProjectToImage(world,18f,-4f-1.5f+0.27f,0),0);
			Add(listPoints,"legend","probing",scene.ProjectToImage(world,18.3f,-8f-1.5f,0),0);
			Add(listPoints,"legend","probing red",scene.ProjectToImage(world,19.3f,-8f-1.5f,0),0);
			ChartMatrix worldLines=ChartMatrix.Translation(35f,0,0)*world;
			float yLineMiddle=-1.5f+(1f/tcData.ScaleMmToPix);
			Add(listPoints,"legend","gingival margin",scene.ProjectToImage(worldLines,42.5f,yLineMiddle,0),0);
			Add(listPoints,"legend","CAL",scene.ProjectToImage(worldLines,42.5f,yLineMiddle-4f,0),0);
			Add(listPoints,"legend","MGJ",scene.ProjectToImage(worldLines,42.5f,yLineMiddle-8f,0),0);
			ChartMatrix worldFurcations=ChartMatrix.Translation(65f,0,0)*worldLines;
			float height=(float)(2f*Math.Sqrt(3)/2f);
			Add(listPoints,"legend","furcation 3",scene.ProjectToImage(worldFurcations,17f,-8.5f-height*2f/3f,0),0);
		}

		private static PerioProbePoint Add(List<PerioProbePoint> listPoints,string kind,string name,PointF point,int sampleRadius) {
			PerioProbePoint probe=new PerioProbePoint();
			probe.Kind=kind;
			probe.Name=name;
			probe.Point=point;
			probe.SampleRadius=sampleRadius;
			listPoints.Add(probe);
			return probe;
		}

		///<summary>The area-weighted centre, in the image, of the group's triangles that face the camera after the world transform. A mirrored transform
		///(the lingual rows) reverses the winding, so there a triangle faces the camera when its transformed geometric normal points away from it, as the
		///WPF chart's reversed index list draws it (plan 5.2). Null when none does.</summary>
		public static PointF? GetVisibleCentre(ToothScene scene,ToothGraphic toothGraphic,int indexGroup,ChartMatrix world) {
			int[] arrayTriangles=ToothMeshWinding.GetWoundTriangles(toothGraphic,indexGroup);
			double sign=world.Determinant3x3<0?-1:1;
			double sumArea=0;
			double sumX=0;
			double sumY=0;
			for(int i=0;i+2<arrayTriangles.Length;i+=3) {
				float[] arrayX=new float[3];
				float[] arrayY=new float[3];
				float[] arrayZ=new float[3];
				PointF[] arrayProjected=new PointF[3];
				for(int v=0;v<3;v++) {
					Vertex3f vertex=toothGraphic.VertexNormals[arrayTriangles[i+v]].Vertex;
					world.TransformCoordinate(vertex.X,vertex.Y,vertex.Z,out arrayX[v],out arrayY[v],out arrayZ[v]);
					arrayProjected[v]=scene.ProjectToImage(world,vertex.X,vertex.Y,vertex.Z);
				}
				double nz=((double)arrayX[1]-arrayX[0])*((double)arrayY[2]-arrayY[0])-((double)arrayY[1]-arrayY[0])*((double)arrayX[2]-arrayX[0]);
				if(nz*sign<=0) {
					continue;
				}
				double area=Math.Abs(((double)arrayProjected[1].X-arrayProjected[0].X)*((double)arrayProjected[2].Y-arrayProjected[0].Y)
					-((double)arrayProjected[1].Y-arrayProjected[0].Y)*((double)arrayProjected[2].X-arrayProjected[0].X))/2;
				sumArea+=area;
				sumX+=area*(arrayProjected[0].X+arrayProjected[1].X+arrayProjected[2].X)/3;
				sumY+=area*(arrayProjected[0].Y+arrayProjected[1].Y+arrayProjected[2].Y)/3;
			}
			if(sumArea<=0) {
				return null;
			}
			return new PointF((float)(sumX/sumArea),(float)(sumY/sumArea));
		}

		///<summary>Probes both pictures at every point inside them. A line probe whose DirectX pixel is not the line's colour is left out (the line is
		///covered there, or the unsmoothed line misses the pixel). A mark or a line (SampleRadius 0) is sampled in the WPF picture at the pixel of the
		///3x3 around the point nearest to DirectX's colour: the WPF chart smooths the sloped lines and the triangles that DirectX draws without
		///smoothing, so an edge drawn later (a GM, CAL or MGJ line over a probing bar) may tint the one pixel by a part of a pixel in one picture
		///and not in the other. A mark of another colour, or more than a pixel off, still fails.</summary>
		public static List<PerioProbeOutcome> Probe(string sceneName,Size size,List<PerioProbePoint> listPoints,byte[] arrayDirectX,byte[] arrayWpf,
			Color colorBackground)
		{
			List<PerioProbeOutcome> listOutcomes=new List<PerioProbeOutcome>();
			foreach(PerioProbePoint probe in listPoints) {
				if(probe.Point.X<0 || probe.Point.Y<0 || probe.Point.X>=size.Width || probe.Point.Y>=size.Height) {
					continue;
				}
				PerioProbeOutcome outcome=new PerioProbeOutcome();
				outcome.SceneName=sceneName;
				outcome.Probe=probe;
				outcome.ColorDirectX=ParityProbes.Sample(arrayDirectX,size,probe.Point,probe.SampleRadius);
				outcome.ColorWpf=probe.SampleRadius==0
					? SampleNearest(arrayWpf,size,probe.Point,outcome.ColorDirectX)
					: ParityProbes.Sample(arrayWpf,size,probe.Point,probe.SampleRadius);
				if(probe.ColorLine!=null && GetDistance(outcome.ColorDirectX,probe.ColorLine.Value)>MaxLineColourDistance) {
					continue;
				}
				outcome.Failure=ParityProbes.GetFailure(outcome.ColorDirectX,outcome.ColorWpf,colorBackground,probe.IsToothDrawn);
				if(outcome.Failure!=null) {
					outcome.Allowance=Allowances.FirstOrDefault(x => x.Covers(outcome));
				}
				listOutcomes.Add(outcome);
			}
			return listOutcomes;
		}

		///<summary>The pixel of the 3x3 around the point (inside the picture) whose colour is nearest to colorTarget.</summary>
		public static Color SampleNearest(byte[] arrayPixelsArgb,Size size,PointF point,Color colorTarget) {
			int xCentre=(int)Math.Floor(point.X);
			int yCentre=(int)Math.Floor(point.Y);
			Color colorBest=Color.Empty;
			double distanceBest=double.MaxValue;
			for(int y=yCentre-1;y<=yCentre+1;y++) {
				for(int x=xCentre-1;x<=xCentre+1;x++) {
					if(x<0 || y<0 || x>=size.Width || y>=size.Height) {
						continue;
					}
					int index=(y*size.Width+x)*4;
					Color color=Color.FromArgb(255,arrayPixelsArgb[index+2],arrayPixelsArgb[index+1],arrayPixelsArgb[index]);
					double distance=GetDistance(color,colorTarget);
					if(distance<distanceBest) {
						distanceBest=distance;
						colorBest=color;
					}
				}
			}
			return colorBest;
		}

		public static double GetDistance(Color color1,Color color2) {
			double r=color1.R-color2.R;
			double g=color1.G-color2.G;
			double b=color1.B-color2.B;
			return Math.Sqrt(r*r+g*g+b*b);
		}
	}
}
