using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using OpenPmsDentalBusiness;

namespace SparksToothChart.Wpf3D {
	///<summary>Builds the immutable scene of the WPF chart from ToothChartData: a pure function of the data, run on the caller's thread, that
	///makes no WPF object (docs/specs/wpf-tooth-chart-plan.md, section 5). It follows the DirectX chart's DrawScene step by step:
	///- 3D layer: the tooth groups, implants and perio teeth that DirectX draws with depth testing on, each with DirectX's world matrix (without the
	///  handedness flip), its colour (opaque; in the chart, unerupted teeth half-transparent as Open Dental's DirectX 11 chart draws them, in the
	///  scene's Items3DUnder, beneath the opaque teeth: TC-3c, LOOK-2), the specular value of DirectX's GetGroupMaterial, and the reversed winding
	///  when the transform mirrors (5.2).
	///- 2D layer: everything DirectX draws with depth testing off, and its D3DX text, in DirectX's order: per tooth the big X, the root canal lines
	///  and the build-up/post silhouette (facial view), and the sealant "S" (occlusal view); then the watches, the drawings, the centre line and the
	///  numbers (5.1). Points are projected with DirectX's own projection (ToothScene.ProjectToImage), the mapping the camera uses.
	///The perio chart (TC-7) follows DrawScenePerio: each row's teeth with the perio light, then that row's decorations in the 2D layer, then the
	///centre line, the numbers and the legend.</summary>
	public static class ToothSceneBuilder {
		///<summary>The specular values of DirectX's GetGroupMaterial: cementum 0.1, near-white enamel colours 0.4, every other colour 1.0.</summary>
		public const float SpecularCementum=0.1f;
		public const float SpecularEnamel=0.4f;
		public const float SpecularNormal=1f;
		///<summary>TC-3c (LOOK-2): the alpha of an unerupted tooth in the chart, 161 of 255 (0.63). Open Dental's DirectX 11 chart draws unerupted teeth
		///half-transparent over the background; its 2023 mixed-dentition screenshot
		///(https://i0.wp.com/opendental.blog/wp-content/uploads/2023/07/image-4.png?ssl=1) gives 0.63: the enamel's red and green are equal (250,250,240),
		///so over the background 233,254,254 a translucent pixel's green minus red is (1-alpha)*21, which averages 7.8 over 18090 pixels of the
		///unerupted crowns (0.60 to 0.69 by region). od-unknown-refined: a Set Mixed Dentition screenshot of Open Dental 26.2 settles it. The same
		///screenshot shows them beneath the opaque teeth: the primary teeth's roots are crisp over the pale permanent crowns (ToothScene.Items3DUnder).</summary>
		public const int UneruptedAlpha=161;

		///<summary>The scene of the chart as tcData holds it, at tcData's control size (ToothChartData.SizeControl, which the wrapper sets on every
		///resize). An empty size gives an empty scene.</summary>
		public static ToothScene Build(ToothChartData tcData) {
			SceneBuilding building=CreateBuilding(tcData);
			if(building==null) {
				Size size=tcData.SizeControl;
				return new ToothScene(new Size(Math.Max(size.Width,0),Math.Max(size.Height,0)),tcData.ColorBackground,tcData.PerioMode,
					tcData.SizeOriginalProjection.Width,tcData.SizeOriginalProjection.Height,new List<ToothSceneItem3D>(),new List<ToothSceneItem2D>());
			}
			if(tcData.PerioMode) {
				AddPerioScene(building);
			}
			else {
				AddChartScene(building);
			}
			return new ToothScene(building.Scene.Size,tcData.ColorBackground,tcData.PerioMode,building.Scene.WidthProjection,building.Scene.HeightProjection,
				building.ListItems3D,building.ListItems2D,building.ListItems3DUnder);
		}

		///<summary>An empty building of the scene at tcData's control size and projection, or null for an empty size.</summary>
		private static SceneBuilding CreateBuilding(ToothChartData tcData) {
			Size size=tcData.SizeControl;
			if(size.Width<1 || size.Height<1 || tcData.ScaleMmToPix<=0 || float.IsNaN(tcData.ScaleMmToPix) || float.IsInfinity(tcData.ScaleMmToPix)) {
				return null;
			}
			//DirectX's Render: the projection keeps 130 x 97.34 mm and widens for a wide or tall control.
			float heightProj=tcData.SizeOriginalProjection.Height;
			float widthProj=tcData.SizeOriginalProjection.Width;
			if(tcData.IsWide) {
				widthProj=heightProj*size.Width/size.Height;
			}
			else {
				heightProj=widthProj*size.Height/size.Width;
			}
			return new SceneBuilding(tcData,size,widthProj,heightProj);
		}

		#region Chart scene
		///<summary>DirectX's DrawScene: for each tooth the facial view, then the occlusal view; then the watches, the drawings, the centre line and the
		///numbers. TC-5 adds what DirectX does not draw: the paint text in the watch band after the watches, and the free-text
		///labels last, over everything. TC-3c (LOOK-2): the half-transparent unerupted teeth go into the scene's Items3DUnder, a 3D layer beneath
		///the opaque teeth: they blend over the background, and every opaque tooth covers them whatever their depth, as Open Dental's 2023
		///mixed-dentition screenshot shows (the primary teeth's roots crisp over the pale permanent crowns). TC-6 adds what only Open Dental's
		///DirectX 11 chart drew: ortho mode's facial-only layout (no occlusal views, ORTHO-1), the retained roots and the brackets in each facial
		///view (RR-1, ORTHO-2), the space maintainers' bars after the teeth (SM-1), and the wires and elastics as thin lines in the 2D layer after
		///the teeth's own 2D items, under the watches and numbers (ORTHO-2).</summary>
		private static void AddChartScene(SceneBuilding building) {
			ToothChartData tcData=building.TcData;
			for(int t=0;t<tcData.ListToothGraphics.Count;t++) {
				if(tcData.ListToothGraphics[t].ToothID=="implant") {//this is not an actual tooth.
					continue;
				}
				AddFacialView(building,tcData.ListToothGraphics[t]);
				if(!tcData.IsOrthoMode) {
					AddOcclusalView(building,tcData.ListToothGraphics[t]);
				}
			}
			AddSpaceMaintainers(building);
			AddOrthoWires(building);
			AddWatches(building);
			AddPaintTexts(building);
			AddDrawingSegments(building);
			AddNumbersAndLines(building);
			AddFreeTexts(building);
		}

		///<summary>DirectX's DrawFacialView: the tooth, then (depth off) the big X, the root canal lines and the build-up/post, then the implant. TC-6:
		///a retained root on a tooth that is shown draws only its root (RR-1), and a bracket goes on a crown that is drawn (ORTHO-2), after the
		///implant; in ortho mode the row is moved toward the centre line (GetTransYfacial, ORTHO-1).</summary>
		private static void AddFacialView(SceneBuilding building,ToothGraphic toothGraphic) {
			ToothChartData tcData=building.TcData;
			string toothId=toothGraphic.ToothID;
			ChartMatrix world=GetFacialWorld(tcData,toothGraphic);
			if(toothGraphic.Visible || (toothGraphic.IsCrown && toothGraphic.IsImplant) || toothGraphic.IsPontic) {
				if(IsRetainedRootShown(toothGraphic)) {
					AddRetainedRoot(building,toothGraphic,world,toothId+" facial");
				}
				else {
					AddTooth(building,toothGraphic,world,toothId+" facial");
				}
			}
			if(toothGraphic.DrawBigX) {//Extraction
				string source="big X of "+toothId;
				if(ToothGraphic.IsMaxillary(toothId)) {
					AddLineList(building,world,toothGraphic.colorX,0.71f,source,new Vertex3f(-2f,12f,0f),new Vertex3f(2f,-6f,0f));
					AddLineList(building,world,toothGraphic.colorX,0.71f,source,new Vertex3f(2f,12f,0f),new Vertex3f(-2f,-6f,0f));
				}
				else {
					AddLineList(building,world,toothGraphic.colorX,0.71f,source,new Vertex3f(-2f,6f,0f),new Vertex3f(2f,-12f,0f));
					AddLineList(building,world,toothGraphic.colorX,0.71f,source,new Vertex3f(2f,6f,0f),new Vertex3f(-2f,-12f,0f));
				}
			}
			if(toothGraphic.Visible && toothGraphic.IsRCT) {//Root Canal
				foreach(LineSimple line in toothGraphic.GetRctLines()) {
					AddExtendedLineStrip(building,world,0.08f,false,toothGraphic.colorRCT,0.8f,"root canal of "+toothId,line.Vertices.ToArray());
				}
			}
			ToothGroup groupBU=toothGraphic.GetGroup(ToothGroupType.Buildup);//during debugging, not all teeth have a BU group yet.
			if(toothGraphic.Visible && groupBU!=null && groupBU.Visible) {//BU or Post
				AddSilhouette(building,toothGraphic,toothGraphic.Groups.IndexOf(groupBU),world,groupBU.PaintColor,"build-up of "+toothId);
			}
			if(toothGraphic.IsImplant) {
				AddImplant(building,toothGraphic,world,"implant of "+toothId);
			}
			if(toothGraphic.HasBracket && IsCrownDrawnFacial(toothGraphic)) {
				AddBracket(building,toothGraphic,world);
			}
		}

		///<summary>DirectX's DrawImplant: the implant model in the implant colour with the normal specular value, upside down for a maxillary tooth.</summary>
		private static void AddImplant(SceneBuilding building,ToothGraphic toothGraphic,ChartMatrix world,string source) {
			ToothChartData tcData=building.TcData;
			if(ToothGraphic.IsMaxillary(toothGraphic.ToothID)) {
				//flip the implant upside down
				world=ChartMatrix.RotationZ((float)Math.PI)*world;
			}
			ToothGraphic implantGraphic=tcData.ListToothGraphics["implant"];
			if(implantGraphic==null) {
				return;
			}
			for(int g=0;g<implantGraphic.Groups.Count;g++) {
				ToothGroup group=implantGraphic.Groups[g];
				if(!group.Visible || group.GroupType==ToothGroupType.Buildup || group.Faces.Count==0) {
					continue;
				}
				building.ListItems3D.Add(new ToothSceneItem3D("implant",g,group.GroupType,GetImplantColor(toothGraphic),SpecularNormal,world,source));
			}
		}

		///<summary>DirectX's DrawOcclusalView: the tooth seen from occlusal, unless the primary tooth over it shows, then (depth on) the sealant "S".</summary>
		private static void AddOcclusalView(SceneBuilding building,ToothGraphic toothGraphic) {
			ToothChartData tcData=building.TcData;
			string toothId=toothGraphic.ToothID;
			ChartMatrix toothTrans=ChartMatrix.Translation(tcData.GetTransX(toothId),tcData.GetTransYocclusal(toothId),0);
			ChartMatrix toothRot=GetOcclusalRotation(toothId);
			ChartMatrix world=GetUserRotationAndTranslation(toothGraphic)*toothRot*toothTrans;
			if(!Tooth.IsPrimary(toothId)//if perm tooth
				&& Tooth.IsValidDB(Tooth.PermToPri(toothId))
				&& tcData.ListToothGraphics[Tooth.PermToPri(toothId)].Visible)//and the primary tooth is visible
			{
				//do not paint
			}
			else if(toothGraphic.Visible//might not be visible if an implant
				|| (toothGraphic.IsCrown && toothGraphic.IsImplant))//a crown on an implant will paint
			//pontics won't paint, because tooth is invisible
			{
				if(IsRetainedRootShown(toothGraphic)) {
					AddRetainedRoot(building,toothGraphic,world,toothId+" occlusal");//TC-6 (RR-1): the root seen from occlusal, a small crescent
				}
				else {
					AddTooth(building,toothGraphic,world,toothId+" occlusal");
				}
			}
			if(toothGraphic.Visible && toothGraphic.IsSealant && !IsRetainedRootShown(toothGraphic)) {//draw sealant (a retained root has no crown)
				float toMm=1f/tcData.ScaleMmToPix;
				ChartMatrix worldSealant=GetUserTranslation(toothGraphic)*toothRot*toothTrans;
				ChartFont font=building.FontSealant;
				if(ToothGraphic.IsMaxillary(toothId)) {
					AddText(building,"S",worldSealant,tcData.PixelScaleRatio*(-6f*toMm),tcData.PixelScaleRatio*(-100f*toMm),-6f,toothGraphic.colorSealant,font,
						"sealant of "+toothId);
				}
				else {
					AddText(building,"S",worldSealant,tcData.PixelScaleRatio*(-6f*toMm),tcData.PixelScaleRatio*(22f*toMm),-6f,toothGraphic.colorSealant,font,
						"sealant of "+toothId);
				}
			}
		}

		///<summary>The view rotation of the occlusal surface (DrawOcclusalView): -110 degrees about X for the maxilla, 110 for mandibular anterior
		///teeth and 120 for mandibular posterior teeth.</summary>
		public static ChartMatrix GetOcclusalRotation(string toothId) {
			ChartMatrix toothRot=ChartMatrix.Identity;
			if(ToothGraphic.IsMaxillary(toothId)) {
				toothRot=toothRot*ChartMatrix.RotationX((float)((-110f*Math.PI)/180f));//rotate angle about line from origin to x,y,z
			}
			else {//mandibular
				if(ToothGraphic.IsAnterior(toothId)) {
					toothRot=toothRot*ChartMatrix.RotationX((float)((110f*Math.PI)/180f));
				}
				else {
					toothRot=toothRot*ChartMatrix.RotationX((float)((120f*Math.PI)/180f));
				}
			}
			return toothRot;
		}

		///<summary>DirectX's DrawTooth with GetGroupMaterial: every visible group except the build-up and the unnamed groups, in its paint colour.
		///In the chart an unerupted tooth (ShiftO below -10) is half-transparent (GetGroupColor) and goes into ListItems3DUnder, beneath the opaque
		///teeth; the perio chart keeps DirectX's opaque half RGB (GetGroupColorPerio).</summary>
		private static void AddTooth(SceneBuilding building,ToothGraphic toothGraphic,ChartMatrix world,string source) {
			for(int g=0;g<toothGraphic.Groups.Count;g++) {
				ToothGroup group=toothGraphic.Groups[g];
				if(!group.Visible || group.Faces.Count==0 || group.GroupType==ToothGroupType.Buildup || group.GroupType==ToothGroupType.None) {
					continue;
				}
				Color color=building.TcData.PerioMode ? GetGroupColorPerio(toothGraphic,group) : GetGroupColor(toothGraphic,group);
				ToothSceneItem3D item=new ToothSceneItem3D(toothGraphic.ToothID,g,group.GroupType,color,GetGroupSpecular(group),world,source);
				if(color.A<255) {
					building.ListItems3DUnder.Add(item);
				}
				else {
					building.ListItems3D.Add(item);
				}
			}
		}

		///<summary>The colour of a tooth group in the chart: its paint colour, opaque. TC-3c (LOOK-2): an unerupted tooth (ShiftO below -10) keeps its
		///RGB at alpha UneruptedAlpha, as Open Dental's DirectX 11 chart draws it, pale and translucent over the background, where the DirectX 9 chart
		///drew it opaque at half RGB (GetGroupColorPerio).</summary>
		public static Color GetGroupColor(ToothGraphic toothGraphic,ToothGroup group) {
			Color colorPaint=group.PaintColor;
			if(toothGraphic.ShiftO<-10) {//if unerupted
				return Color.FromArgb(UneruptedAlpha,colorPaint.R,colorPaint.G,colorPaint.B);
			}
			return Color.FromArgb(255,colorPaint);
		}

		///<summary>The colour of a tooth group in the perio chart, DirectX's rule: its paint colour, at half RGB when the tooth is unerupted (ShiftO
		///below -10). DirectX halves the alpha too but draws with blending off, so the colour is opaque. The graphical perio chart moves no tooth
		///(FormPerioGraphical), and how Open Dental 26.2 draws it is not public (PERIO-1), so it stays as TC-7 built it.</summary>
		public static Color GetGroupColorPerio(ToothGraphic toothGraphic,ToothGroup group) {
			Color colorPaint=group.PaintColor;
			if(toothGraphic.ShiftO<-10) {//if unerupted
				return Color.FromArgb(255,colorPaint.R/2,colorPaint.G/2,colorPaint.B/2);
			}
			return Color.FromArgb(255,colorPaint);
		}

		///<summary>The colour of a tooth's implant: its implant colour, opaque, as DirectX draws it with blending off.</summary>
		public static Color GetImplantColor(ToothGraphic toothGraphic) {
			return Color.FromArgb(255,toothGraphic.colorImplant);
		}

		///<summary>DirectX's GetGroupMaterial: cementum 0.1; a near-white paint colour (R over 245, G over 245, B over 235; the enamel colour is
		///250,250,240) 0.4, because DirectX washes out the specular on enamel; every other colour 1.0.</summary>
		public static float GetGroupSpecular(ToothGroup group) {
			if(group.GroupType==ToothGroupType.Cementum) {
				return SpecularCementum;
			}
			if(group.PaintColor.R>245 && group.PaintColor.G>245 && group.PaintColor.B>235) {
				return SpecularEnamel;
			}
			return SpecularNormal;
		}
		#endregion Chart scene

		#region Tooth movement
		///<summary>DirectX's ToothTranslationMatrix: ShiftM, ShiftO and ShiftB with the signs of the tooth's quadrant.</summary>
		public static ChartMatrix GetUserTranslation(ToothGraphic toothGraphic) {
			string toothId=toothGraphic.ToothID;
			if(ToothGraphic.IsRight(toothId)) {
				if(ToothGraphic.IsMaxillary(toothId)) {//UR
					return ChartMatrix.Translation(toothGraphic.ShiftM,-toothGraphic.ShiftO,toothGraphic.ShiftB);
				}
				return ChartMatrix.Translation(toothGraphic.ShiftM,toothGraphic.ShiftO,toothGraphic.ShiftB);//LR
			}
			if(ToothGraphic.IsMaxillary(toothId)) {//UL
				return ChartMatrix.Translation(-toothGraphic.ShiftM,-toothGraphic.ShiftO,toothGraphic.ShiftB);
			}
			return ChartMatrix.Translation(-toothGraphic.ShiftM,toothGraphic.ShiftO,toothGraphic.ShiftB);//LL
		}

		///<summary>DirectX's ToothRotationMatrix: Rotate (about Y) first, then TipB (about X), then TipM (about Z), with the signs of the tooth's
		///quadrant.</summary>
		public static ChartMatrix GetUserRotation(ToothGraphic toothGraphic) {
			string toothId=toothGraphic.ToothID;
			ChartMatrix rotM;
			ChartMatrix rotB;
			ChartMatrix rot;
			if(ToothGraphic.IsRight(toothId)) {
				if(ToothGraphic.IsMaxillary(toothId)) {//UR
					rotM=ChartMatrix.RotationZ(((float)(toothGraphic.TipM*Math.PI)/180));
					rotB=ChartMatrix.RotationX(((float)(-toothGraphic.TipB*Math.PI)/180));
					rot=ChartMatrix.RotationY(((float)(toothGraphic.Rotate*Math.PI)/180));
				}
				else {//LR
					rotM=ChartMatrix.RotationZ(((float)(-toothGraphic.TipM*Math.PI)/180));
					rotB=ChartMatrix.RotationX(((float)(toothGraphic.TipB*Math.PI)/180));
					rot=ChartMatrix.RotationY(((float)(-toothGraphic.Rotate*Math.PI)/180));
				}
			}
			else {
				if(ToothGraphic.IsMaxillary(toothId)) {//UL
					rotM=ChartMatrix.RotationZ(((float)(-toothGraphic.TipM*Math.PI)/180));
					rotB=ChartMatrix.RotationX(((float)(-toothGraphic.TipB*Math.PI)/180));
					rot=ChartMatrix.RotationY(((float)(toothGraphic.Rotate*Math.PI)/180));
				}
				else {//LL
					rotM=ChartMatrix.RotationZ(((float)(toothGraphic.TipM*Math.PI)/180));
					rotB=ChartMatrix.RotationX(((float)(toothGraphic.TipB*Math.PI)/180));
					rot=ChartMatrix.RotationY(((float)(-toothGraphic.Rotate*Math.PI)/180));
				}
			}
			return rot*rotB*rotM;
		}

		///<summary>DirectX's ToothRotationAndTranslationMatrix: the user's rotations, then the user's translation.</summary>
		public static ChartMatrix GetUserRotationAndTranslation(ToothGraphic toothGraphic) {
			return GetUserRotation(toothGraphic)*GetUserTranslation(toothGraphic);
		}
		#endregion Tooth movement

		#region Perio scene
		///<summary>The four rows of the graphical perio chart, from the top: maxillary facial, maxillary lingual, mandibular lingual, mandibular
		///facial.</summary>
		public enum PerioRow {
			MaxillaryFacial,
			MaxillaryLingual,
			MandibularLingual,
			MandibularFacial,
		}

		///<summary>DirectX's DrawScenePerio: baseY moves the chart as near to the top of the control as possible, so a taller control gives the
		///legend room at the bottom.</summary>
		public static float GetPerioBaseY(ToothChartData tcData) {
			//The distance from y=0 to the tip of the highest root in the upper-most maxillary row.
			const float maxYmm=63f;
			return tcData.SizeControl.Height/(2f*tcData.ScaleMmToPix)-maxYmm;
		}

		///<summary>A perio row's orientation: DirectX's row matrix (DrawScenePerio) without the handedness flip and 400 mm shift of its defOrient.
		///The two lingual rows add a Z flip, which shows the teeth from behind; their transforms mirror, so their teeth use the reversed winding.</summary>
		public static ChartMatrix GetPerioRowOrientation(PerioRow perioRow,float baseY) {
			ChartMatrix defOrient=ChartMatrix.Translation(0,baseY,0);
			switch(perioRow) {
				case PerioRow.MaxillaryFacial:
					return ChartMatrix.Translation(0,45f,0)*defOrient;
				case PerioRow.MaxillaryLingual:
					return ChartMatrix.Scaling(1f,1f,-1f)*ChartMatrix.Translation(0,12f,0)*defOrient;
				case PerioRow.MandibularLingual:
					return ChartMatrix.Scaling(1f,1f,-1f)*ChartMatrix.Translation(0,-12f,0)*defOrient;
				default:
					return ChartMatrix.Translation(0,-45f,0)*defOrient;
			}
		}

		///<summary>DirectX's DrawScenePerio: the four rows, each its teeth and implants (3D layer) and then its decorations (2D layer, DrawPerioRow), then
		///the centre line and the numbers (DrawNumbersAndLinesPerio) and the legend (DrawPerioLegend), all in DirectX's order (TC-7).</summary>
		private static void AddPerioScene(SceneBuilding building) {
			ToothChartData tcData=building.TcData;
			float baseY=GetPerioBaseY(tcData);
			List<ToothGraphic> listMaxillary=new List<ToothGraphic>();
			List<ToothGraphic> listMandible=new List<ToothGraphic>();
			for(int t=0;t<tcData.ListToothGraphics.Count;t++) {
				if(tcData.ListToothGraphics[t].ToothID=="implant") {
					continue;
				}
				if(ToothGraphic.IsMaxillary(tcData.ListToothGraphics[t].ToothID)) {
					listMaxillary.Add(tcData.ListToothGraphics[t]);
				}
				else {
					listMandible.Add(tcData.ListToothGraphics[t]);
				}
			}
			AddPerioRow(building,listMaxillary,GetPerioRowOrientation(PerioRow.MaxillaryFacial,baseY),"maxillary facial row");
			AddPerioRowDecorations(building,listMaxillary,GetPerioRowOrientation(PerioRow.MaxillaryFacial,baseY),true,false,"maxillary facial row");
			AddPerioRow(building,listMaxillary,GetPerioRowOrientation(PerioRow.MaxillaryLingual,baseY),"maxillary lingual row");
			AddPerioRowDecorations(building,listMaxillary,GetPerioRowOrientation(PerioRow.MaxillaryLingual,baseY),true,true,"maxillary lingual row");
			AddPerioRow(building,listMandible,GetPerioRowOrientation(PerioRow.MandibularLingual,baseY),"mandibular lingual row");
			AddPerioRowDecorations(building,listMandible,GetPerioRowOrientation(PerioRow.MandibularLingual,baseY),false,true,"mandibular lingual row");
			AddPerioRow(building,listMandible,GetPerioRowOrientation(PerioRow.MandibularFacial,baseY),"mandibular facial row");
			AddPerioRowDecorations(building,listMandible,GetPerioRowOrientation(PerioRow.MandibularFacial,baseY),false,false,"mandibular facial row");
			AddNumbersAndLinesPerio(building,baseY);
			AddPerioLegend(building,-60f,baseY-63f);
		}

		private static void AddPerioRow(SceneBuilding building,List<ToothGraphic> listToothGraphics,ChartMatrix orientation,string rowName) {
			ToothChartData tcData=building.TcData;
			for(int t=0;t<listToothGraphics.Count;t++) {
				ToothGraphic toothGraphic=listToothGraphics[t];
				ChartMatrix world=ChartMatrix.Translation(tcData.GetTransX(toothGraphic.ToothID),0,0)*orientation;
				if(toothGraphic.Visible || (toothGraphic.IsCrown && toothGraphic.IsImplant) || toothGraphic.IsPontic) {
					AddTooth(building,toothGraphic,world,toothGraphic.ToothID+" "+rowName);
				}
				if(toothGraphic.IsImplant) {
					AddImplant(building,toothGraphic,world,"implant of "+toothGraphic.ToothID+" "+rowName);
				}
			}
		}

		///<summary>The start of the Source of the perio items, for tests and messages. None starts with "number ", "sealant " or "watch ", the sources
		///the chart's DirectX parity tests probe, except the perio numbers, which are the chart's numbers.</summary>
		public const string SourcePerioLine="perio mm line ";
		public const string SourceMobility="mobility of ";
		public const string SourceFurcation="furcation ";
		public const string SourceProbing="probing ";
		public const string SourceBleeding="bleeding ";
		public const string SourceSuppuration="suppuration ";
		public const string SourceGingivalMargin="gingival margin line ";
		public const string SourceCAL="CAL line ";
		public const string SourceMGJ="MGJ line ";
		public const string SourcePerioCentreLine="perio centre line";
		///<summary>The Source of the chart's centre line (AddNumbersAndLines).</summary>
		public const string SourceCentreLine="centre line";
		public const string SourceLegend="legend ";
		///<summary>The legend's names, in English as DirectX prints them.</summary>
		public const string LegendGingivalMargin="Gingival Margin";
		public const string LegendRecession="Recession";

		///<summary>DirectX's DrawPerioRow after the row's teeth, with depth testing off: the millimetre lines (the 0 line in the text colour, grey lines at
		///3, 6 and 9 mm toward the roots); then per tooth the mobility number (facial rows only), the furcation triangles, the probing bars and the bleeding
		///and suppuration droplets of the row's three sites; then the gingival margin, CAL and MGJ lines. Every item in the row's orientation.</summary>
		private static void AddPerioRowDecorations(SceneBuilding building,List<ToothGraphic> listToothGraphics,ChartMatrix orientation,bool maxillary,
			bool lingual,string rowName)
		{
			ToothChartData tcData=building.TcData;
			float sign=maxillary?1:-1;
			const float leftX=-65f;
			const float rightX=65f;
			const int mmMax=9;
			//Draw the horizontal line at 0
			AddExtendedLineStrip(building,orientation,0,false,tcData.ColorText,0.3f,SourcePerioLine+"0 of the "+rowName,new Vertex3f(leftX,0,0),
				new Vertex3f(rightX,0,0));
			//Draw the other horizontal lines
			for(int mm=3;mm<=mmMax;mm+=3) {
				AddExtendedLineStrip(building,orientation,0,false,Color.Gray,0.16f,SourcePerioLine+mm+" of the "+rowName,new Vertex3f(leftX,sign*mm,0),
					new Vertex3f(rightX,sign*mm,0));
			}
			PerioSurf[] arraySurfs=lingual
				? new PerioSurf[] { PerioSurf.DL,PerioSurf.L,PerioSurf.ML }
				: new PerioSurf[] { PerioSurf.DB,PerioSurf.B,PerioSurf.MB };
			for(int t=0;t<listToothGraphics.Count;t++) {
				ToothGraphic toothGraphic=listToothGraphics[t];
				string toothId=toothGraphic.ToothID;
				if(!lingual) {
					//Draw mobility numbers, centred on the tooth by the numbers' measure.
					SizeF sizeMobility=MeasureStringMm(building,toothGraphic.Mobility);
					ChartMatrix worldMobility=ChartMatrix.Translation(tcData.GetTransX(toothId)-sizeMobility.Width/2f,0,0)*orientation;
					AddText(building,toothGraphic.Mobility,worldMobility,0,maxillary?-2.5f:5.5f,0,toothGraphic.colorMobility,building.FontNumbers,
						SourceMobility+toothId);
				}
				ChartMatrix world=ChartMatrix.Translation(tcData.GetTransX(toothId),0,0)*orientation;
				int intTooth=ToothGraphic.IdToInt(toothId);
				foreach(PerioSurf perioSurf in arraySurfs) {
					AddFurcation(building,world,intTooth,perioSurf,maxillary,toothId);
				}
				foreach(PerioSurf perioSurf in arraySurfs) {
					AddProbingBar(building,world,intTooth,perioSurf,toothId);
				}
				foreach(PerioSurf perioSurf in arraySurfs) {
					AddDroplet(building,world,intTooth,perioSurf,true,maxillary,toothId);
				}
				foreach(PerioSurf perioSurf in arraySurfs) {
					AddDroplet(building,world,intTooth,perioSurf,false,maxillary,toothId);
				}
			}
			AddHorizontalLines(building,orientation,PerioSequenceType.GingMargin,maxillary,lingual,tcData.ColorGingivalMargin,SourceGingivalMargin+"of the "+rowName);
			AddHorizontalLines(building,orientation,PerioSequenceType.CAL,maxillary,lingual,tcData.ColorCAL,SourceCAL+"of the "+rowName);
			AddHorizontalLines(building,orientation,PerioSequenceType.MGJ,maxillary,lingual,tcData.ColorMGJ,SourceMGJ+"of the "+rowName);
		}

		///<summary>DirectX's GM, CAL and MGJ lines of a row: each line of GetHorizontalLines as a strip 0.4 mm wide, every joint and end lengthened by 0.04
		///mm.</summary>
		private static void AddHorizontalLines(SceneBuilding building,ChartMatrix orientation,PerioSequenceType perioSequenceType,bool maxillary,bool lingual,
			Color color,string source)
		{
			List<LineSimple> listLines=building.TcData.GetHorizontalLines(perioSequenceType,maxillary,!lingual);
			for(int i=0;i<listLines.Count;i++) {
				AddExtendedLineStrip(building,orientation,0.04f,true,color,0.4f,source,listLines[i].Vertices.ToArray());
			}
		}

		///<summary>DirectX's DrawFurcationTriangle for a site: a grade 1 to 3 at the site's furcation position, pointing toward the crown (its tip at the
		///position, its base toward the root: up on the maxilla).</summary>
		private static void AddFurcation(SceneBuilding building,ChartMatrix world,int intTooth,PerioSurf perioSurf,bool maxillary,string toothId) {
			int furcationValue=building.TcData.GetFurcationValue(intTooth,perioSurf);
			if(furcationValue<1 || furcationValue>3) {
				return;
			}
			PointF pointSite=building.TcData.GetFurcationPos(intTooth,perioSurf);
			AddFurcationTriangle(building,world,pointSite.X,pointSite.Y,maxillary,furcationValue,SourceFurcation+furcationValue+" of "+toothId+" "+perioSurf);
		}

		///<summary>DirectX's GetFurcationColor: the furcation colour, red from the red limit on.</summary>
		public static Color GetFurcationColor(ToothChartData tcData,int furcationValue) {
			if(furcationValue>=tcData.RedLimitFurcations) {
				return tcData.ColorFurcationsRed;
			}
			return tcData.ColorFurcations;
		}

		///<summary>DirectX's DrawFurcationTriangle: an equilateral triangle with 2 mm sides and its tip at (tipX,tipY), its base above the tip when pointUp.
		///Grade 1 is its two sides from the tip (an open "V"), grade 2 its outline, grade 3 its outline and the triangle filled; lines 0.38 mm wide,
		///lengthened by 0.1 mm.</summary>
		private static void AddFurcationTriangle(SceneBuilding building,ChartMatrix world,float tipX,float tipY,bool pointUp,int furcationValue,string source) {
			const float triSideLenMM=2f;
			float sign=pointUp?1:-1;
			Color color=GetFurcationColor(building.TcData,furcationValue);
			//We form an equilateral triangle.
			Vertex3f p0=new Vertex3f(tipX+triSideLenMM/2f,tipY+sign*((float)(triSideLenMM*Math.Sqrt(3)/2f)),0);
			Vertex3f p1=new Vertex3f(tipX,tipY,0);
			Vertex3f p2=new Vertex3f(tipX-triSideLenMM/2f,tipY+sign*((float)(triSideLenMM*Math.Sqrt(3)/2f)),0);
			if(furcationValue==1) {
				AddExtendedLineStrip(building,world,0.1f,false,color,0.38f,source,p0,p1,p2);
			}
			else if(furcationValue==2) {
				AddExtendedLineStrip(building,world,0.1f,true,color,0.38f,source,p0,p1,p2,p0);
			}
			else if(furcationValue==3) {
				AddExtendedLineStrip(building,world,0.1f,true,color,0.38f,source,p0,p1,p2,p0);
				AddPolygonFan(building,world,color,source+" (filled)",p0,p1,p2);
			}
		}

		///<summary>DirectX's DrawProbingBar: the bar of GetProbingLine, 0.6 mm wide, in the probing colour or the red one.</summary>
		private static void AddProbingBar(SceneBuilding building,ChartMatrix world,int intTooth,PerioSurf perioSurf,string toothId) {
			const float barWidthMM=0.6f;
			Color colorBar;
			LineSimple lineBar=building.TcData.GetProbingLine(intTooth,perioSurf,out colorBar);
			if(lineBar==null) {
				return;
			}
			RectangleF rectBar=new RectangleF(lineBar.Vertices[0].X-barWidthMM/2f,lineBar.Vertices[0].Y,barWidthMM,lineBar.Vertices[1].Y-lineBar.Vertices[0].Y);
			AddRectangle(building,world,rectBar,colorBar,SourceProbing+toothId+" "+perioSurf);
		}

		///<summary>DirectX's DrawDroplet for a site: the bleeding or suppuration droplet at its place from GetBleedingOrSuppuration (none at 0,0), flipped in
		///Y on the mandible.</summary>
		private static void AddDroplet(SceneBuilding building,ChartMatrix world,int intTooth,PerioSurf perioSurf,bool isBleeding,bool maxillary,string toothId) {
			ToothChartData tcData=building.TcData;
			PointF pointDroplet=tcData.GetBleedingOrSuppuration(intTooth,perioSurf,isBleeding);
			if(pointDroplet.X==0 && pointDroplet.Y==0) {
				return;//No droplet to draw at this site.
			}
			ChartMatrix worldDroplet=ChartMatrix.Translation(pointDroplet.X,pointDroplet.Y,0)*world;
			if(!maxillary) {
				//When the droplet is for a mandibular tooth, flip the droplet about the x-axis (negate y values).
				worldDroplet=ChartMatrix.Scaling(1f,-1f,1f)*worldDroplet;
			}
			Color colorDroplet=isBleeding?tcData.ColorBleeding:tcData.ColorSuppuration;
			AddDropletShape(building,worldDroplet,0,0,colorDroplet,(isBleeding?SourceBleeding:SourceSuppuration)+toothId+" "+perioSurf);
		}

		///<summary>DirectX's DrawDroplet(x,y,color): the pentagon of GetDropletVertices moved to (x,y), drawn as DirectX's triangle fan.</summary>
		private static void AddDropletShape(SceneBuilding building,ChartMatrix world,float x,float y,Color color,string source) {
			List<Vertex3f> listPoints=new List<Vertex3f>();
			foreach(Vertex3f vertex in building.TcData.GetDropletVertices()) {
				listPoints.Add(new Vertex3f(x+vertex.X,y+vertex.Y,vertex.Z));
			}
			AddPolygonFan(building,world,color,source,listPoints.ToArray());
		}

		///<summary>DirectX's DrawColoredPolygon: a triangle fan from the first point, one 2D item of its triangles (DirectX fills each with no culling, so
		///their union, which the Nonzero rule gives).</summary>
		private static void AddPolygonFan(SceneBuilding building,ChartMatrix world,Color color,string source,params Vertex3f[] arrayPoints) {
			if(arrayPoints.Length<3) {
				return;
			}
			PointF[] arrayProjected=ProjectPolygon(building,world,arrayPoints);
			List<PointF[]> listPolygons=new List<PointF[]>();
			for(int i=1;i+1<arrayProjected.Length;i++) {
				listPolygons.Add(new PointF[] { arrayProjected[0],arrayProjected[i],arrayProjected[i+1] });
			}
			building.ListItems2D.Add(ToothSceneItem2D.CreatePolygons(color,listPolygons,source));
		}

		///<summary>DirectX's DrawNumbersAndLinesPerio: the black centre line, 0.5 mm, drawn in the world matrix the last row left set (the mandibular
		///facial row's, so its y of 45 lands on baseY, between the arches), then the numbers 1-32, never highlighted, 30 mm above baseY on the maxilla and
		///29 mm below it on the mandible.</summary>
		private static void AddNumbersAndLinesPerio(SceneBuilding building,float baseY) {
			AddLineList(building,GetPerioRowOrientation(PerioRow.MandibularFacial,baseY),Color.Black,0.5f,SourcePerioCentreLine,new Vertex3f(-65f,45f,0),
				new Vertex3f(65f,45f,0));
			for(int i=1;i<=32;i++) {
				string toothId=Tooth.FromOrdinal(i);
				float yOffset=ToothGraphic.IsMaxillary(toothId)?30:-29;
				AddNumber(building,toothId,false,baseY+yOffset);
			}
		}

		///<summary>The legend's name of the gingival margin line (Open Dental 24.4): "Recession" when the office's Perio Setup option is on and the exam
		///has no hyperplasia (a gingival margin stored over 100), as the Perio Chart's row label (PerioMeasures.GetGingMarginTerm); otherwise "Gingival
		///Margin", DirectX's text.</summary>
		public static string GetLegendGingivalMarginText(ToothChartData tcData) {
			if(!tcData.PerioUseRecessionTerm) {
				return LegendGingivalMargin;
			}
			bool hasHyperplasia=false;
			foreach(PerioMeasure perioMeasure in tcData.ListPerioMeasure) {
				if(perioMeasure.SequenceType!=PerioSequenceType.GingMargin) {
					continue;
				}
				foreach(int value in new int[] { perioMeasure.MBvalue,perioMeasure.Bvalue,perioMeasure.DBvalue,perioMeasure.MLvalue,perioMeasure.Lvalue,
					perioMeasure.DLvalue })
				{
					if(PerioMeasures.IsHyperplasia(value)) {
						hasHyperplasia=true;
					}
				}
			}
			if(PerioMeasures.GetGingMarginTerm(true,hasHyperplasia)==PerioMeasures.TermRecession) {
				return LegendRecession;
			}
			return LegendGingivalMargin;
		}

		///<summary>DirectX's DrawPerioLegend at (xLeft,yTop) in mm: three columns of black text with their marks drawn to scale in the perio colours:
		///bleeding and suppuration droplets and the two probing bars; the gingival margin (or recession), CAL and MGJ lines, 2 pixels high; the three
		///furcation grades pointing down, in the colour their grade has under the red limit.</summary>
		private static void AddPerioLegend(SceneBuilding building,float xLeft,float yTop) {
			ToothChartData tcData=building.TcData;
			float ySpace=4f;
			Color textColor=Color.Black;
			ChartFont font=building.FontNumbers;
			ChartMatrix world=ChartMatrix.Translation(xLeft,yTop,0);
			AddText(building,"Bleeding",world,0,0,0,textColor,font,SourceLegend+"text Bleeding");
			AddDropletShape(building,world,18f,-1.5f,tcData.ColorBleeding,SourceLegend+"bleeding");
			AddText(building,"Suppuration",world,0,-ySpace,0,textColor,font,SourceLegend+"text Suppuration");
			AddDropletShape(building,world,18f,-ySpace-1.5f,tcData.ColorSuppuration,SourceLegend+"suppuration");
			AddText(building,"Probing",world,0,-2*ySpace,0,textColor,font,SourceLegend+"text Probing");
			AddRectangle(building,world,new RectangleF(18f,-2*ySpace-3f,0.6f,3f),tcData.ColorProbing,SourceLegend+"probing");
			AddRectangle(building,world,new RectangleF(19f,-2*ySpace-3f,0.6f,3f),tcData.ColorProbingRed,SourceLegend+"probing red");
			world=world*ChartMatrix.Translation(35f,0,0);
			string textGingivalMargin=GetLegendGingivalMarginText(tcData);
			AddText(building,textGingivalMargin,world,0,0,0,textColor,font,SourceLegend+"text "+textGingivalMargin);
			AddRectangle(building,world,new RectangleF(35f,-1.5f,15f,2f/tcData.ScaleMmToPix),tcData.ColorGingivalMargin,SourceLegend+"gingival margin");
			AddText(building,"Clinical Attachment Level",world,0,-ySpace,0,textColor,font,SourceLegend+"text Clinical Attachment Level");
			AddRectangle(building,world,new RectangleF(35f,-ySpace-1.5f,15f,2f/tcData.ScaleMmToPix),tcData.ColorCAL,SourceLegend+"CAL");
			AddText(building,"Mucogingival Junction",world,0,-2*ySpace,0,textColor,font,SourceLegend+"text Mucogingival Junction");
			AddRectangle(building,world,new RectangleF(35f,-2*ySpace-1.5f,15f,2f/tcData.ScaleMmToPix),tcData.ColorMGJ,SourceLegend+"MGJ");
			world=world*ChartMatrix.Translation(65f,0,0);
			AddText(building,"Furcation 1",world,0,0,0,textColor,font,SourceLegend+"text Furcation 1");
			AddFurcationTriangle(building,world,17f,-0.5f,false,1,SourceLegend+"furcation 1");
			AddText(building,"Furcation 2",world,0,-ySpace,0,textColor,font,SourceLegend+"text Furcation 2");
			AddFurcationTriangle(building,world,17f,-ySpace-0.5f,false,2,SourceLegend+"furcation 2");
			AddText(building,"Furcation 3",world,0,-2*ySpace,0,textColor,font,SourceLegend+"text Furcation 3");
			AddFurcationTriangle(building,world,17f,-2*ySpace-0.5f,false,3,SourceLegend+"furcation 3");
		}
		#endregion Perio scene

		#region 2D layer
		///<summary>DirectX's DrawWatches and RenderToothWatch: a "W" for each watched tooth, in the procedure's colour, at DirectX's place. A permanent
		///tooth's watch shows even when the tooth is missing; a primary tooth's only when it is visible, and it takes its permanent tooth's place.
		///TC-3c (PT-1): plain text, regular weight, without the bigger white "W" that DirectX 9 drew first as an outline, as Open Dental's DirectX 11
		///chart draws it (https://i0.wp.com/opendental.blog/wp-content/uploads/2024/08/Capture17.png?ssl=1 and
		///https://i0.wp.com/opendental.blog/wp-content/uploads/2026/10/image-6.png?ssl=1).</summary>
		private static void AddWatches(SceneBuilding building) {
			ToothChartData tcData=building.TcData;
			//A Hashtable keyed by the permanent tooth, as DirectX keeps it: its order is the order of the watches.
			Hashtable hashtableWatchTeeth=new Hashtable(tcData.ListToothGraphics.Count);
			for(int t=0;t<tcData.ListToothGraphics.Count;t++) {//loop through each adult tooth
				ToothGraphic toothGraphic=tcData.ListToothGraphics[t];
				if(toothGraphic.ToothID=="implant" || !toothGraphic.Watch || Tooth.IsPrimary(toothGraphic.ToothID)) {
					continue;
				}
				hashtableWatchTeeth[toothGraphic.ToothID]=toothGraphic;
			}
			for(int t=0;t<tcData.ListToothGraphics.Count;t++) {//loop through each primary tooth
				ToothGraphic toothGraphic=tcData.ListToothGraphics[t];
				if(toothGraphic.ToothID=="implant" || !toothGraphic.Watch || !Tooth.IsPrimary(toothGraphic.ToothID) || !toothGraphic.Visible) {
					continue;
				}
				hashtableWatchTeeth[Tooth.PriToPerm(toothGraphic.ToothID)]=toothGraphic;
			}
			foreach(DictionaryEntry dictionaryEntry in hashtableWatchTeeth) {
				ToothGraphic toothGraphic=(ToothGraphic)dictionaryEntry.Value;
				ChartMatrix toothTrans;
				if(ToothGraphic.IsRight(toothGraphic.ToothID)) {
					toothTrans=ChartMatrix.Translation(tcData.GetTransX(toothGraphic.ToothID)+toothGraphic.ShiftM,0,0);
				}
				else {
					toothTrans=ChartMatrix.Translation(tcData.GetTransX(toothGraphic.ToothID)-toothGraphic.ShiftM,0,0);
				}
				float toMm=1f/tcData.ScaleMmToPix;
				string source="watch of "+toothGraphic.ToothID;
				if(ToothGraphic.IsMaxillary(toothGraphic.ToothID)) {
					AddText(building,"W",toothTrans,tcData.PixelScaleRatio*(-6f*toMm),tcData.PixelScaleRatio*(153f*toMm)-GetOrthoShift(tcData),-6f,
						toothGraphic.colorWatch,building.FontWatch,source);
				}
				else {
					AddText(building,"W",toothTrans,tcData.PixelScaleRatio*(-6f*toMm),tcData.PixelScaleRatio*(-138f*toMm)+GetOrthoShift(tcData),-6f,
						toothGraphic.colorWatch,building.FontWatch,source);
				}
			}
		}

		///<summary>DirectX's DrawDrawingSegments: every saved drawing in its colour, then the points of the drawing in progress in the pen colour.</summary>
		private static void AddDrawingSegments(SceneBuilding building) {
			ToothChartData tcData=building.TcData;
			for(int s=0;s<tcData.DrawingSegmentList.Count;s++) {
				string[] arrayPointStr=tcData.DrawingSegmentList[s].DrawingSegment.Split(';');
				List<Vertex3f> listPoints=new List<Vertex3f>();
				for(int p=0;p<arrayPointStr.Length;p++) {
					string[] arrayXy=arrayPointStr[p].Split(',');
					float x;
					float y;
					if(IsValidCoordinate(arrayXy,out x,out y)) {
						PointF pointMm=tcData.PointDrawingPixToMm(new PointF(x,y));
						listPoints.Add(new Vertex3f(pointMm.X,pointMm.Y,0f));
					}
				}
				AddExtendedLineStrip(building,ChartMatrix.Identity,0.08f,true,tcData.DrawingSegmentList[s].ColorDraw,0.71f,"drawing "+s,listPoints.ToArray());
			}
			List<Vertex3f> listCurrent=new List<Vertex3f>();
			for(int i=0;i<tcData.PointList.Count;i++) {
				PointF pointMm=tcData.PointPixToMm(tcData.PointList[i]);
				listCurrent.Add(new Vertex3f(pointMm.X,pointMm.Y,0));
			}
			AddExtendedLineStrip(building,ChartMatrix.Identity,0.08f,true,tcData.ColorDrawing,0.71f,"drawing in progress",listCurrent.ToArray());
		}

		///<summary>DirectX's IsValidCoordinate: "x,y" with both parsed by float.TryParse in the current culture.</summary>
		private static bool IsValidCoordinate(string[] arrayCoordinate,out float x,out float y) {
			x=0;
			y=0;
			return arrayCoordinate.Length==2 && float.TryParse(arrayCoordinate[0],out x) && float.TryParse(arrayCoordinate[1],out y);
		}

		///<summary>DirectX's DrawNumbersAndLines: the centre line, then the number of each tooth 1-32 and A-T in order, selected ones on their
		///highlight rectangle. TC-3c (LOOK-4): the centre line is in the chart's text colour (ColorText: the "Text" chart graphic colour in the module
		///and the big chart, "Text on TPs" on printouts and sheets), the colour of the numbers, as Open Dental's DirectX 11 chart draws it dark like its
		///numbers (https://i0.wp.com/odtechblog.wordpress.com/wp-content/uploads/2026/08/image-6.png?ssl=1 and
		///https://i0.wp.com/opendental.blog/wp-content/uploads/2024/08/Capture17.png?ssl=1), where DirectX 9 drew it white. Its place and pixels
		///stay DirectX's.</summary>
		private static void AddNumbersAndLines(SceneBuilding building) {
			ToothChartData tcData=building.TcData;
			AddLineList(building,ChartMatrix.Identity,Color.FromArgb(255,tcData.ColorText),0.32f,SourceCentreLine,new Vertex3f(-65f,0,0),new Vertex3f(65f,0,0));
			for(int i=1;i<=52;i++) {
				string toothId=Tooth.FromOrdinal(i);
				AddNumber(building,toothId,tcData.SelectedTeeth.Contains(toothId),0);
			}
		}

		///<summary>DirectX's DrawNumber: nothing for a hidden number or a primary tooth whose letter is not shown; otherwise the highlight rectangle
		///when selected, then the number, a tenth of the box's width in from its left.</summary>
		private static void AddNumber(SceneBuilding building,string toothId,bool isSelected,float offsetY) {
			ToothChartData tcData=building.TcData;
			if(!Tooth.IsValidDB(toothId)) {
				return;
			}
			if(tcData.ListToothGraphics[toothId].HideNumber) {//if this is a "hidden" number
				return;//skip
			}
			//primary, but not set to show primary letters
			if(Tooth.IsPrimary(toothId) && !tcData.ListToothGraphics[Tooth.PriToPerm(toothId)].ShowPrimaryLetter) {
				return;
			}
			string displayNum=Tooth.DisplayGraphic(toothId,tcData.ToothNumberingNomenclature);
			SizeF labelSize=MeasureStringMm(building,displayNum);
			RectangleF recMm=tcData.GetNumberRecMm(toothId,labelSize);
			recMm.Y+=offsetY;
			Color foreColor=tcData.ColorText;
			if(isSelected) {
				foreColor=tcData.ColorTextHighlight;
				//Draw the background rectangle only if the text is selected.
				AddRectangle(building,ChartMatrix.Identity,recMm,tcData.ColorBackHighlight,"highlight of "+toothId);
			}
			//Offsets the text by 10% of the rectangle width to ensure that there is at least on pixel of space on the left for the background rectangle.
			AddText(building,displayNum,ChartMatrix.Identity,recMm.X+recMm.Width*0.1f,recMm.Y+recMm.Height,0,foreColor,building.FontNumbers,"number "+toothId);
		}

		///<summary>DirectX's MeasureStringMm: the text's size in the numbers' font, the width times 1.25 and the height times 0.8 (D3DX adds more
		///vertical and less horizontal room than GDI+), in mm.</summary>
		public static SizeF MeasureStringMm(SceneBuilding building,string text) {
			if(string.IsNullOrEmpty(text)) {
				return new SizeF(0,0);
			}
			Size sizeText=ChartFontMeasure.MeasureText(building.FontNumbers,text);
			float width=sizeText.Width;
			float height=sizeText.Height;
			return new SizeF((width*1.25f)/building.TcData.ScaleMmToPix,(height*0.8f)/building.TcData.ScaleMmToPix);
		}

		///<summary>DirectX's PrintString: the D3DX text's top left goes to the ceiling of x and the floor of y of the point's projection.</summary>
		private static void AddText(SceneBuilding building,string text,ChartMatrix world,float x,float y,float z,Color color,ChartFont font,string source) {
			if(string.IsNullOrEmpty(text)) {
				return;
			}
			float xScreen;
			float yScreen;
			building.Scene.ProjectDirectX(world,x,y,z,out xScreen,out yScreen);
			Point origin=new Point((int)Math.Ceiling(xScreen),(int)Math.Floor(yScreen));
			int widthPixels=ChartFontMeasure.MeasureText(font,text).Width;
			building.ListItems2D.Add(ToothSceneItem2D.CreateText(text,color,font,origin,widthPixels,source));
		}

		///<summary>DirectX's DrawColoredRectangle: the rectangle in mm (origin at the lower left) as one polygon.</summary>
		private static void AddRectangle(SceneBuilding building,ChartMatrix world,RectangleF rect,Color color,string source) {
			Vertex3f[] arrayVerts=new Vertex3f[] {
				new Vertex3f(rect.Left,rect.Bottom,0),
				new Vertex3f(rect.Left,rect.Top,0),
				new Vertex3f(rect.Right,rect.Top,0),
				new Vertex3f(rect.Right,rect.Bottom,0),
			};
			List<PointF[]> listPolygons=new List<PointF[]>();
			listPolygons.Add(ProjectPolygon(building,world,arrayVerts));
			building.ListItems2D.Add(ToothSceneItem2D.CreatePolygons(color,listPolygons,source));
		}

		///<summary>DirectX's DrawExtendedLineStrip: the strip broken into two-point lines, each line lengthened by extendDist at the joints (and at the
		///ends when extendEndPoints), then drawn as DrawLineList draws them.</summary>
		private static void AddExtendedLineStrip(SceneBuilding building,ChartMatrix world,float extendDist,bool extendEndPoints,Color color,float lineWidthMm,
			string source,params Vertex3f[] arrayPoints)
		{
			if(arrayPoints.Length<2) {
				return;//Can happen with drawings somehow.
			}
			List<Vertex3f> listLines=new List<Vertex3f>();
			for(int p=0;p<arrayPoints.Length-1;p++) {
				Vertex3f p1=arrayPoints[p];
				Vertex3f p2=arrayPoints[p+1];
				bool p1IsEndPoint=(p==0);
				bool p2IsEndPoint=(p==arrayPoints.Length-2);
				float dx=p2.X-p1.X;
				float dy=p2.Y-p1.Y;
				float dz=p2.Z-p1.Z;
				float length=(float)Math.Sqrt((dx*dx)+(dy*dy)+(dz*dz));
				if(length==0) {
					continue;
				}
				//SharpDX's Vector3.Normalize: multiply by 1/length.
				float inverse=1f/length;
				dx*=inverse;
				dy*=inverse;
				dz*=inverse;
				float x1=p1.X;
				float y1=p1.Y;
				float z1=p1.Z;
				float x2=p2.X;
				float y2=p2.Y;
				float z2=p2.Z;
				//Do not extend the endpoints for the ends of the line strips unless extendEndPoints=true.
				if(!p1IsEndPoint || extendEndPoints) {
					x1=x1-extendDist*dx;
					y1=y1-extendDist*dy;
					z1=z1-extendDist*dz;
				}
				if(!p2IsEndPoint || extendEndPoints) {
					x2=x2+extendDist*dx;
					y2=y2+extendDist*dy;
					z2=z2+extendDist*dz;
				}
				listLines.Add(new Vertex3f(x1,y1,z1));
				listLines.Add(new Vertex3f(x2,y2,z2));
			}
			AddLineList(building,world,color,lineWidthMm,source,listLines.ToArray());
		}

		///<summary>DirectX's DrawLineList: each pair of points becomes a rectangle lineWidthMm wide (two triangles in DirectX), in the plane of the
		///points, transformed by world and projected. One 2D item for the list.</summary>
		private static void AddLineList(SceneBuilding building,ChartMatrix world,Color color,float lineWidthMm,string source,params Vertex3f[] arrayPoints) {
			List<PointF[]> listPolygons=new List<PointF[]>();
			for(int i=0;i<arrayPoints.Length-1;i+=2) {
				Vertex3f p1=arrayPoints[i];
				Vertex3f p2=arrayPoints[i+1];
				float vx=p2.X-p1.X;
				float vy=p2.Y-p1.Y;
				float vz=p2.Z-p1.Z;
				if((float)Math.Sqrt((vx*vx)+(vy*vy)+(vz*vz))==0) {
					continue;
				}
				//Our "up" vector. Z component does not matter because of orthographic projection.
				float ux=-vy;
				float uy=vx;
				float lengthUp=(float)Math.Sqrt((ux*ux)+(uy*uy));
				if(lengthUp==0) {
					continue;//a line straight along Z has no width on screen
				}
				//SharpDX's Normalize (times 1/length), then times half the width, as DirectX computes it.
				float inverse=1f/lengthUp;
				ux=ux*inverse*(lineWidthMm/2);
				uy=uy*inverse*(lineWidthMm/2);
				Vertex3f[] arrayQuad=new Vertex3f[] {
					new Vertex3f(p1.X+ux,p1.Y+uy,p1.Z),
					new Vertex3f(p2.X+ux,p2.Y+uy,p2.Z),
					new Vertex3f(p2.X-ux,p2.Y-uy,p2.Z),
					new Vertex3f(p1.X-ux,p1.Y-uy,p1.Z),
				};
				listPolygons.Add(ProjectPolygon(building,world,arrayQuad));
			}
			if(listPolygons.Count==0) {
				return;
			}
			building.ListItems2D.Add(ToothSceneItem2D.CreatePolygons(color,listPolygons,source));
		}

		///<summary>The silhouette of a 3D group as DirectX shows a group drawn with depth testing off and only an emissive colour: the union of its
		///projected triangles, each turned the same way and all filled with the Nonzero rule. Flat colour, no depth, no culling.</summary>
		private static void AddSilhouette(SceneBuilding building,ToothGraphic toothGraphic,int indexGroup,ChartMatrix world,Color color,string source) {
			if(indexGroup<0) {
				return;
			}
			List<int> listTriangles=toothGraphic.Groups[indexGroup].GetTriangleIndices();
			if(listTriangles.Count<3) {
				return;
			}
			PointF[] arrayProjected=new PointF[toothGraphic.VertexNormals.Count];
			bool[] arrayIsProjected=new bool[toothGraphic.VertexNormals.Count];
			List<PointF[]> listPolygons=new List<PointF[]>();
			for(int i=0;i+2<listTriangles.Count;i+=3) {
				PointF[] arrayTriangle=new PointF[3];
				for(int v=0;v<3;v++) {
					int index=listTriangles[i+v];
					if(!arrayIsProjected[index]) {
						Vertex3f vertex=toothGraphic.VertexNormals[index].Vertex;
						arrayProjected[index]=building.Scene.ProjectToImage(world,vertex.X,vertex.Y,vertex.Z);
						arrayIsProjected[index]=true;
					}
					arrayTriangle[v]=arrayProjected[index];
				}
				listPolygons.Add(arrayTriangle);
			}
			building.ListItems2D.Add(ToothSceneItem2D.CreatePolygons(color,listPolygons,source));
		}

		private static PointF[] ProjectPolygon(SceneBuilding building,ChartMatrix world,Vertex3f[] arrayVerts) {
			PointF[] arrayPoints=new PointF[arrayVerts.Length];
			for(int i=0;i<arrayVerts.Length;i++) {
				arrayPoints[i]=building.Scene.ProjectToImage(world,arrayVerts[i].X,arrayVerts[i].Y,arrayVerts[i].Z);
			}
			return arrayPoints;
		}
		#endregion 2D layer

		#region Text of TC-5: paint text and free text
		///<summary>The start of the Source of each kind of TC-5 text item, for tests and messages. None starts with "number ", "sealant " or "watch ",
		///the sources the DirectX parity tests probe.</summary>
		public const string SourcePaintText="paint text of ";
		public const string SourceFreeText="free text ";

		///<summary>Paint type Text with text other than "W" (O-22, as Open Dental draws it: "Fracture" above maxillary 9 in its 2021 tip, "UCF" under
		///mandibular 24 and 25 in its 26.2 screenshot): each tooth's text in the watch mark's band beyond the root tip, above a maxillary tooth and
		///below a mandibular one, centred on the tooth with its mesial shift (the watch mark's x), in the procedure's colour (opaque) and the paint
		///text font (the watch mark's font, regular weight). When the tooth's place also has a watch mark (GetWatchPositions), the text goes one line
		///nearer the tooth and the "W" stays at the edge, as "UCF" over "W" in the 26.2 screenshot (seen on the mandible; the maxilla mirrors it). A
		///permanent tooth's text shows even when the tooth is missing, as its watch mark does; a primary tooth's only when the primary tooth is
		///shown. The text stays inside the picture across.</summary>
		private static void AddPaintTexts(SceneBuilding building) {
			ToothChartData tcData=building.TcData;
			List<string> listWatchPositions=GetWatchPositions(tcData);
			Size size=building.Scene.Size;
			ChartFont font=building.FontPaintText;
			float toMm=1f/tcData.ScaleMmToPix;
			for(int t=0;t<tcData.ListToothGraphics.Count;t++) {
				ToothGraphic toothGraphic=tcData.ListToothGraphics[t];
				if(toothGraphic.ToothID=="implant" || string.IsNullOrEmpty(toothGraphic.PaintText)) {
					continue;
				}
				string toothId=toothGraphic.ToothID;
				bool isPrimary=Tooth.IsPrimary(toothId);
				if(isPrimary && !toothGraphic.Visible) {
					continue;
				}
				string toothIdPerm=isPrimary ? Tooth.PriToPerm(toothId) : toothId;
				bool isMaxillary=ToothGraphic.IsMaxillary(toothId);
				//The watch mark's line (AddWatches): its top at 153 pixels times the ratio above the centre over a maxillary tooth, 138 below it under a
				//mandibular one.
				float yMm=isMaxillary ? tcData.PixelScaleRatio*(153f*toMm)-GetOrthoShift(tcData) : tcData.PixelScaleRatio*(-138f*toMm)+GetOrthoShift(tcData);
				float xScreen;
				float yScreen;
				building.Scene.ProjectDirectX(ChartMatrix.Identity,GetShiftedTransX(tcData,toothGraphic),yMm,-6f,out xScreen,out yScreen);
				int widthPixels=ChartFontMeasure.MeasureText(font,toothGraphic.PaintText).Width;
				int x=(int)Math.Ceiling(xScreen-widthPixels/2f);
				x=Math.Max(0,Math.Min(x,size.Width-widthPixels));
				int y=(int)Math.Floor(yScreen);
				if(listWatchPositions.Contains(toothIdPerm)) {
					y+=isMaxillary ? building.FontWatch.Height : -building.FontWatch.Height;//one line nearer the tooth; the "W" keeps the edge
				}
				building.ListItems2D.Add(ToothSceneItem2D.CreateText(toothGraphic.PaintText,Color.FromArgb(255,toothGraphic.colorPaintText),font,
					new Point(x,y),widthPixels,SourcePaintText+toothId));
			}
		}
		///<summary>The permanent tooth positions where a watch mark is drawn, by the rules of AddWatches: a permanent tooth's watch always, a primary
		///tooth's when the primary tooth is shown (in its permanent tooth's place).</summary>
		private static List<string> GetWatchPositions(ToothChartData tcData) {
			List<string> listPositions=new List<string>();
			for(int t=0;t<tcData.ListToothGraphics.Count;t++) {
				ToothGraphic toothGraphic=tcData.ListToothGraphics[t];
				if(toothGraphic.ToothID=="implant" || !toothGraphic.Watch) {
					continue;
				}
				if(!Tooth.IsPrimary(toothGraphic.ToothID)) {
					listPositions.Add(toothGraphic.ToothID);
				}
				else if(toothGraphic.Visible) {
					listPositions.Add(Tooth.PriToPerm(toothGraphic.ToothID));
				}
			}
			return listPositions;
		}

		///<summary>The tooth's x in mm with its mesial shift, as the watch marks place it (DrawWatches).</summary>
		private static float GetShiftedTransX(ToothChartData tcData,ToothGraphic toothGraphic) {
			if(ToothGraphic.IsRight(toothGraphic.ToothID)) {
				return tcData.GetTransX(toothGraphic.ToothID)+toothGraphic.ShiftM;
			}
			return tcData.GetTransX(toothGraphic.ToothID)-toothGraphic.ShiftM;
		}

		///<summary>The free-text labels, last, over everything: each at its place (FreeTextBox), in its colour (opaque, as the drawings of the same
		///colour) and the free-text font.</summary>
		private static void AddFreeTexts(SceneBuilding building) {
			foreach(FreeTextBox freeTextBox in GetFreeTextBoxes(building)) {
				building.ListItems2D.Add(ToothSceneItem2D.CreateText(freeTextBox.FreeText.Text,Color.FromArgb(255,freeTextBox.FreeText.ColorText),
					building.FontFreeText,freeTextBox.Bounds.Location,freeTextBox.Bounds.Width,SourceFreeText+freeTextBox.FreeText.ToothInitialNum));
			}
		}

		///<summary>Where each free-text label of tcData is drawn, in the order they are drawn (the last is on top), at tcData's control size: the boxes
		///the mouse picks a label up by (ToothChartMouse, CursorTool.MoveText). Empty for an empty size or the perio chart, which draws no labels.</summary>
		public static List<FreeTextBox> GetFreeTextBoxes(ToothChartData tcData) {
			if(tcData.PerioMode) {
				return new List<FreeTextBox>();
			}
			SceneBuilding building=CreateBuilding(tcData);
			if(building==null) {
				return new List<FreeTextBox>();
			}
			return GetFreeTextBoxes(building);
		}

		///<summary>Each label's text cell: its top left is the label's point in the 410x307 drawing space, mapped to the picture as the drawings are
		///(PointDrawingPixToMm and DirectX's projection) and put on the nearest whole pixel; its size is the text's
		///width and the font's cell height. A label that would run past an edge is moved back inside (a label wider or taller than the picture sits at
		///its left or top), so it can always be seen and picked up. Labels with no text are left out.</summary>
		private static List<FreeTextBox> GetFreeTextBoxes(SceneBuilding building) {
			ToothChartData tcData=building.TcData;
			Size size=building.Scene.Size;
			ChartFont font=building.FontFreeText;
			List<FreeTextBox> listBoxes=new List<FreeTextBox>();
			for(int i=0;i<tcData.ListFreeTexts.Count;i++) {
				ToothChartFreeText freeText=tcData.ListFreeTexts[i];
				if(string.IsNullOrEmpty(freeText.Text)) {
					continue;
				}
				PointF pointMm=tcData.PointDrawingPixToMm(freeText.Location);
				float xScreen;
				float yScreen;
				building.Scene.ProjectDirectX(ChartMatrix.Identity,pointMm.X,pointMm.Y,0,out xScreen,out yScreen);
				if(float.IsNaN(xScreen) || float.IsNaN(yScreen) || float.IsInfinity(xScreen) || float.IsInfinity(yScreen)) {
					xScreen=0;
					yScreen=0;
				}
				int widthPixels=ChartFontMeasure.MeasureText(font,freeText.Text).Width;
				//The nearest whole pixel: no DirectX text to match here, and a whole point of the drawing space lands on its pixel whatever the float's last bit.
				int xPoint=(int)Math.Round(Math.Max(Math.Min(xScreen,int.MaxValue/2f),int.MinValue/2f),MidpointRounding.AwayFromZero);
				int yPoint=(int)Math.Round(Math.Max(Math.Min(yScreen,int.MaxValue/2f),int.MinValue/2f),MidpointRounding.AwayFromZero);
				int x=Math.Max(0,Math.Min(xPoint,size.Width-widthPixels));
				int y=Math.Max(0,Math.Min(yPoint,size.Height-font.Height));
				FreeTextBox freeTextBox=new FreeTextBox();
				freeTextBox.FreeText=freeText;
				freeTextBox.Bounds=new Rectangle(x,y,widthPixels,font.Height);
				//The label's point in the drawing space where it is seen: its own point, or, when it was moved back inside, that point moved as much.
				float ratio=tcData.PixelScaleRatio>0 ? tcData.PixelScaleRatio : 1f;
				freeTextBox.LocationShown=new PointF(freeText.Location.X+(x-xPoint)/ratio,freeText.Location.Y+(y-yPoint)/ratio);
				listBoxes.Add(freeTextBox);
			}
			return listBoxes;
		}
		#endregion Text of TC-5: paint text and free text

		#region Orthodontics, retained roots and space maintainers (TC-6)
		///<summary>ORTHO-1: how far ortho mode moves each facial row toward the centre line, in mm. Measured in Open Dental's ortho-mode screenshots
		///against its normal chart (Capture17, 2024): the teeth keep their size there (a central incisor about 70 pixels long, the 16 teeth as wide as
		///in the normal chart), and the crowns' edges are 46 pixels nearer the centre line than in the normal chart in Capture13 (2024) and 46.5 in
		///ortho9 (2022, on the molars) at 410 pixels across, 3.154 pixels a mm: 14.6 mm. The numbers between the rows stay where they are.</summary>
		public const float OrthoFacialShiftMm=14.6f;
		///<summary>ORTHO-2: the bracket's base, a flat face 2.8 mm wide and 2.2 mm high (about 9 by 7 pixels at the module's size, as Open Dental's
		///brackets in ortho9), its middle 4 mm from the crown's incisal or occlusal edge toward the root (measured: 13 pixels in ortho9 and
		///Capture13).</summary>
		public const float BracketWidthMm=2.8f;
		public const float BracketHeightMm=2.2f;
		public const float BracketFromEdgeMm=4f;
		///<summary>The base's depth: from 0.4 mm inside the crown's facial surface to 0.3 mm in front of it.</summary>
		public const float BracketBaseBackMm=0.4f;
		public const float BracketBaseFrontMm=0.3f;
		///<summary>The four tie wings on the base, darker, 0.4 mm in front of it: 0.65 by 0.8 mm each, 0.3 mm in from the base's sides, leaving the
		///slot (0.6 mm high, across the middle) and a gap between the left and right wings, as ortho9's brackets show (a 9 by 7 pixel square with
		///two dark 2-pixel wings above and below a 2-pixel slot).</summary>
		public const float BracketWingWidthMm=0.65f;
		public const float BracketWingHeightMm=0.8f;
		public const float BracketWingInsetMm=0.3f;
		public const float BracketWingDepthMm=0.4f;
		public const float BracketSlotHeightMm=0.6f;
		///<summary>The wings' colour: the bracket's colour times this (ortho9: wings about 124 against a base of 180).</summary>
		public const float BracketWingShade=0.7f;
		///<summary>The width of a wire or an elastic, in mm: one pixel at the module's size (410 pixels across 130 mm), as ortho9's wires and
		///elastics (ORTHO-2: thin lines of about 1 to 2 pixels).</summary>
		public const float OrthoLineWidthMm=0.32f;
		///<summary>SM-1: the bar's height and depth in mm (space-main-chart.png: about 4.7 pixels high at 407 pixels across), and how far its middle is
		///from the facial row's line toward the crowns (its middle 24.1 mm above the centre line there, the row's line at 29 mm).</summary>
		public const float SpaceMaintainerHeightMm=1.5f;
		public const float SpaceMaintainerDepthMm=1f;
		public const float SpaceMaintainerFromRowMm=5f;
		public const string SourceBracket="bracket of ";
		public const string SourceWireInBracket="wire in bracket of ";
		public const string SourceWire="wire ";
		public const string SourceElastic="elastic ";
		public const string SourceSpaceMaintainer="space maintainer ";
		public const string SourceRetainedRootSuffix=" retained root";
		private static readonly object _lockBracketPlacements=new object();
		private static readonly Dictionary<string,BracketPlacement> _dictBracketPlacements=new Dictionary<string,BracketPlacement>();

		///<summary>Where a tooth model's bracket sits, in the model's own mm: the middle of the base face (x 0), and the z of the crown's facial
		///surface under the bracket.</summary>
		public struct BracketPlacement {
			public float CenterY;
			public float FaceZ;
		}

		///<summary>ORTHO-1: how far the facial rows move toward the centre line: OrthoFacialShiftMm in ortho mode, else 0.</summary>
		public static float GetOrthoShift(ToothChartData tcData) {
			return tcData.IsOrthoMode ? OrthoFacialShiftMm : 0f;
		}

		///<summary>The y of a tooth's facial view in mm: ToothChartData.GetTransYfacial (DirectX's place), moved toward the centre line by
		///OrthoFacialShiftMm in ortho mode (ORTHO-1). ToothChartData's own method stays DirectX's: the OpenGL chart reads it.</summary>
		public static float GetTransYfacial(ToothChartData tcData,string toothId) {
			float y=tcData.GetTransYfacial(toothId);
			if(ToothGraphic.IsMaxillary(toothId)) {
				return y-GetOrthoShift(tcData);
			}
			return y+GetOrthoShift(tcData);
		}

		///<summary>DirectX's world matrix of a tooth's facial view: the user's rotations and translation, then the tooth's place (in ortho mode the
		///moved row).</summary>
		public static ChartMatrix GetFacialWorld(ToothChartData tcData,ToothGraphic toothGraphic) {
			string toothId=toothGraphic.ToothID;
			ChartMatrix toothTrans=ChartMatrix.Translation(tcData.GetTransX(toothId),GetTransYfacial(tcData,toothId),0);
			return GetUserRotationAndTranslation(toothGraphic)*toothTrans;
		}

		///<summary>RR-1: a retained root is drawn only on a tooth that is shown. A completed extraction marks the tooth missing (Open Dental:
		///"once the root's extraction is complete, the root graphic is gone"), and so hides it.</summary>
		public static bool IsRetainedRootShown(ToothGraphic toothGraphic) {
			return toothGraphic.IsRetainedRoot && toothGraphic.Visible;
		}

		///<summary>True when the facial view draws the tooth's crown: a tooth that is shown, a crown on an implant or a pontic, but not a retained root,
		///which has no crown.</summary>
		public static bool IsCrownDrawnFacial(ToothGraphic toothGraphic) {
			return (toothGraphic.Visible || (toothGraphic.IsCrown && toothGraphic.IsImplant) || toothGraphic.IsPontic) && !IsRetainedRootShown(toothGraphic);
		}

		///<summary>RR-1, as Open Dental's DirectX 11 chart draws a retained root (https://odtechblog.files.wordpress.com/2021/01/retained-chart.png):
		///only the tooth's root (its cementum groups), in the procedure's colour, lit like the teeth. In the facial view that is the root; in the
		///occlusal view, where the crown no longer covers it, the near side of the root seen from occlusal: a small crescent. An unerupted tooth's root
		///is translucent like the rest of an unerupted tooth (GetGroupColor's rule) and goes beneath the opaque teeth.</summary>
		private static void AddRetainedRoot(SceneBuilding building,ToothGraphic toothGraphic,ChartMatrix world,string source) {
			for(int g=0;g<toothGraphic.Groups.Count;g++) {
				ToothGroup group=toothGraphic.Groups[g];
				if(!group.Visible || group.Faces.Count==0 || group.GroupType!=ToothGroupType.Cementum) {
					continue;
				}
				Color color=Color.FromArgb(255,toothGraphic.colorRetainedRoot);
				if(toothGraphic.ShiftO<-10) {//if unerupted
					color=Color.FromArgb(UneruptedAlpha,color.R,color.G,color.B);
				}
				ToothSceneItem3D item=new ToothSceneItem3D(toothGraphic.ToothID,g,group.GroupType,color,GetGroupSpecular(group),world,source+SourceRetainedRootSuffix);
				if(color.A<255) {
					building.ListItems3DUnder.Add(item);
				}
				else {
					building.ListItems3D.Add(item);
				}
			}
		}

		///<summary>Where the model of toothGraphic's tooth takes a bracket (cached per tooth ID: the models never change). The crown is every group but
		///the root, the build-up and the unnamed ones. Its edge is the crown's lowest point on a maxillary tooth (the crowns point down) and its
		///highest on a mandibular one; the bracket's middle is BracketFromEdgeMm from there toward the root, and its face sits on the crown's most
		///facial point (largest z) under the bracket.</summary>
		public static BracketPlacement GetBracketPlacement(ToothGraphic toothGraphic) {
			string toothId=toothGraphic.ToothID;
			lock(_lockBracketPlacements) {
				BracketPlacement bracketPlacement;
				if(_dictBracketPlacements.TryGetValue(toothId,out bracketPlacement)) {
					return bracketPlacement;
				}
				HashSet<int> hashSetCrown=new HashSet<int>();
				for(int g=0;g<toothGraphic.Groups.Count;g++) {
					ToothGroupType groupType=toothGraphic.Groups[g].GroupType;
					if(groupType==ToothGroupType.Cementum || groupType==ToothGroupType.Buildup || groupType==ToothGroupType.None
						|| groupType==ToothGroupType.Canals || groupType==ToothGroupType.Implant)
					{
						continue;
					}
					foreach(int index in toothGraphic.Groups[g].GetTriangleIndices()) {
						hashSetCrown.Add(index);
					}
				}
				bool isMaxillary=ToothGraphic.IsMaxillary(toothId);
				float edgeY=isMaxillary ? float.MaxValue : float.MinValue;
				float maxZ=float.MinValue;
				foreach(int index in hashSetCrown) {
					Vertex3f vertex=toothGraphic.VertexNormals[index].Vertex;
					edgeY=isMaxillary ? Math.Min(edgeY,vertex.Y) : Math.Max(edgeY,vertex.Y);
					maxZ=Math.Max(maxZ,vertex.Z);
				}
				if(hashSetCrown.Count==0) {
					edgeY=0;
					maxZ=0;
				}
				float centerY=isMaxillary ? edgeY+BracketFromEdgeMm : edgeY-BracketFromEdgeMm;
				float faceZ=float.MinValue;
				foreach(int index in hashSetCrown) {
					Vertex3f vertex=toothGraphic.VertexNormals[index].Vertex;
					if(Math.Abs(vertex.X)<=BracketWidthMm/2f && Math.Abs(vertex.Y-centerY)<=BracketHeightMm/2f) {
						faceZ=Math.Max(faceZ,vertex.Z);
					}
				}
				if(faceZ==float.MinValue) {
					faceZ=maxZ;//no crown point under the bracket: in front of the whole crown
				}
				bracketPlacement=new BracketPlacement();
				bracketPlacement.CenterY=centerY;
				bracketPlacement.FaceZ=faceZ;
				_dictBracketPlacements[toothId]=bracketPlacement;
				return bracketPlacement;
			}
		}

		///<summary>The world matrix of a tooth's bracket: its place on the model (GetBracketPlacement), then the tooth's facial world, so the bracket
		///follows the tooth's movements and rotations.</summary>
		public static ChartMatrix GetBracketWorld(ToothChartData tcData,ToothGraphic toothGraphic) {
			BracketPlacement bracketPlacement=GetBracketPlacement(toothGraphic);
			return ChartMatrix.Translation(0,bracketPlacement.CenterY,bracketPlacement.FaceZ)*GetFacialWorld(tcData,toothGraphic);
		}

		///<summary>ORTHO-2, as Open Dental draws a bracket (ortho9 and Capture13): a flat face with a slot. Five boxes of ToothSceneShapes.Box in the 3D
		///layer, lit like the teeth: the base in the hardware's colour, and four darker tie wings in front of it, two above and two below the
		///slot.</summary>
		private static void AddBracket(SceneBuilding building,ToothGraphic toothGraphic,ChartMatrix worldFacial) {
			BracketPlacement bracketPlacement=GetBracketPlacement(toothGraphic);
			ChartMatrix world=ChartMatrix.Translation(0,bracketPlacement.CenterY,bracketPlacement.FaceZ)*worldFacial;
			string source=SourceBracket+toothGraphic.ToothID;
			Color colorBase=Color.FromArgb(255,toothGraphic.colorBracket);
			Color colorWing=Color.FromArgb(255,(int)Math.Round(colorBase.R*BracketWingShade),(int)Math.Round(colorBase.G*BracketWingShade),
				(int)Math.Round(colorBase.B*BracketWingShade));
			float baseDepth=BracketBaseBackMm+BracketBaseFrontMm;
			AddBox(building,world,0,0,(BracketBaseFrontMm-BracketBaseBackMm)/2f,BracketWidthMm,BracketHeightMm,baseDepth,colorBase,source);
			float wingX=BracketWidthMm/2f-BracketWingInsetMm-BracketWingWidthMm/2f;
			float wingY=BracketSlotHeightMm/2f+BracketWingHeightMm/2f;
			float wingZ=BracketBaseFrontMm+BracketWingDepthMm/2f;
			foreach(float signY in new float[] { 1f,-1f }) {
				foreach(float signX in new float[] { -1f,1f }) {
					AddBox(building,world,signX*wingX,signY*wingY,wingZ,BracketWingWidthMm,BracketWingHeightMm,BracketWingDepthMm,colorWing,source+" wing");
				}
			}
		}

		///<summary>One box of ToothSceneShapes.Box in the 3D layer: the unit box scaled to the size, moved to the centre, then by world. Lit like the
		///teeth, or unlit in its colour exactly when isFlat.</summary>
		private static void AddBox(SceneBuilding building,ChartMatrix world,float x,float y,float z,float width,float height,float depth,Color color,
			string source,bool isFlat=false)
		{
			ChartMatrix transform=ChartMatrix.Scaling(width,height,depth)*ChartMatrix.Translation(x,y,z)*world;
			building.ListItems3D.Add(new ToothSceneItem3D(ToothSceneShapes.Box,0,ToothGroupType.None,Color.FromArgb(255,color),SpecularNormal,transform,source,
				isFlat));
		}

		///<summary>A point of a tooth's bracket in scene mm: x along the slot (0 the middle, half the width its ends), at the front of the wings.</summary>
		public static Vertex3f GetBracketPoint(ToothChartData tcData,ToothGraphic toothGraphic,float x) {
			ChartMatrix world=GetBracketWorld(tcData,toothGraphic);
			float xOut;
			float yOut;
			float zOut;
			world.TransformCoordinate(x,0,BracketBaseFrontMm+BracketWingDepthMm,out xOut,out yOut,out zOut);
			return new Vertex3f(xOut,yOut,zOut);
		}

		///<summary>ORTHO-2, as Open Dental draws them (ortho9: thin grey wires through the bracket slots, thin yellow elastics from bracket to
		///bracket): every wire and elastic as a line OrthoLineWidthMm wide in the 2D layer, in its colour, in the order the painter gave them. A wire
		///in a bracket runs across the slot; a wire between brackets from the end of one slot to the facing end of the next; an elastic from the
		///middle of one bracket to the middle of the other. The points follow each tooth's movements; a missing tooth's place is used as it is.</summary>
		private static void AddOrthoWires(SceneBuilding building) {
			ToothChartData tcData=building.TcData;
			foreach(ToothChartOrthoWire orthoWire in tcData.ListOrthoWires) {
				ToothGraphic toothGraphicStart=tcData.ListToothGraphics[orthoWire.ToothIDstart];
				if(toothGraphicStart==null) {
					continue;
				}
				if(orthoWire.Kind==ToothChartOrthoWireKind.InBracket) {
					AddLineList(building,ChartMatrix.Identity,orthoWire.ColorDraw,OrthoLineWidthMm,SourceWireInBracket+orthoWire.ToothIDstart,
						GetBracketPoint(tcData,toothGraphicStart,-BracketWidthMm/2f),GetBracketPoint(tcData,toothGraphicStart,BracketWidthMm/2f));
					continue;
				}
				ToothGraphic toothGraphicEnd=tcData.ListToothGraphics[orthoWire.ToothIDend];
				if(toothGraphicEnd==null) {
					continue;
				}
				Vertex3f pointStart=GetBracketPoint(tcData,toothGraphicStart,0);
				Vertex3f pointEnd=GetBracketPoint(tcData,toothGraphicEnd,0);
				if(orthoWire.Kind==ToothChartOrthoWireKind.Elastic) {
					AddLineList(building,ChartMatrix.Identity,orthoWire.ColorDraw,OrthoLineWidthMm,SourceElastic+orthoWire.ToothIDstart+"-"+orthoWire.ToothIDend,
						pointStart,pointEnd);
					continue;
				}
				//Between brackets: from the slot's end that faces the other bracket.
				float sign=pointStart.X<=pointEnd.X ? 1f : -1f;
				AddLineList(building,ChartMatrix.Identity,orthoWire.ColorDraw,OrthoLineWidthMm,SourceWire+orthoWire.ToothIDstart+"-"+orthoWire.ToothIDend,
					GetBracketPoint(tcData,toothGraphicStart,sign*BracketWidthMm/2f),GetBracketPoint(tcData,toothGraphicEnd,-sign*BracketWidthMm/2f));
			}
		}

		///<summary>SM-1: true when a tooth's crown is drawn in its space at crown height: a shown tooth that has erupted (a retained root counts: the
		///tooth is there), a pontic, or a crown on an implant. A missing or hidden tooth, and an unerupted one (its crown is moved into the root's
		///place), leave the space empty.</summary>
		public static bool IsToothInSpace(ToothGraphic toothGraphic) {
			if(toothGraphic==null) {
				return false;
			}
			return (toothGraphic.Visible && toothGraphic.ShiftO>=-10) || toothGraphic.IsPontic || (toothGraphic.IsCrown && toothGraphic.IsImplant);
		}

		///<summary>SM-1, as Open Dental's DirectX 11 chart draws a space maintainer (https://odtechblog.files.wordpress.com/2021/01/space-main-chart.png;
		///"designed to not ever draw in the same space where a tooth already exists", https://www.opendentalsoft.com:8085/forum/viewtopic.php?t=7522):
		///for every permanent tooth's space that has a space maintainer (on the permanent tooth or on its primary tooth) and no tooth in it
		///(IsToothInSpace for both), a solid bar of ToothSceneShapes.Box in the procedure's colour, unlit (the screenshot's bar is the TP colour
		///198,0,0 itself, flat), across the space, at about crown height on the
		///facial view (SpaceMaintainerFromRowMm from the facial row's line toward the crowns; in ortho mode on the moved row). Neighbouring spaces of
		///the same colour make one bar. The bar is 3D, behind the teeth's facial surfaces, so a neighbour moved into the space covers it.</summary>
		private static void AddSpaceMaintainers(SceneBuilding building) {
			ToothChartData tcData=building.TcData;
			foreach(bool isMaxillary in new bool[] { true,false }) {
				int start=isMaxillary ? 1 : 17;
				float yBar=isMaxillary ? GetTransYfacial(tcData,"3")-SpaceMaintainerFromRowMm : GetTransYfacial(tcData,"30")+SpaceMaintainerFromRowMm;
				float left=0;
				float right=0;
				Color colorRun=Color.Empty;
				string sourceRun=null;
				for(int i=start;i<start+16;i++) {
					string permId=i.ToString();
					ToothGraphic toothGraphicPerm=tcData.ListToothGraphics[permId];
					string priId=Tooth.PermToPri(permId);
					ToothGraphic toothGraphicPri=priId=="" ? null : tcData.ListToothGraphics[priId];
					bool isBar=false;
					Color color=Color.Empty;
					if(toothGraphicPerm!=null && toothGraphicPerm.IsSpaceMaintainer) {
						isBar=true;
						color=toothGraphicPerm.colorSpaceMaintainer;
					}
					else if(toothGraphicPri!=null && toothGraphicPri.IsSpaceMaintainer) {
						isBar=true;
						color=toothGraphicPri.colorSpaceMaintainer;
					}
					if(isBar && (IsToothInSpace(toothGraphicPerm) || IsToothInSpace(toothGraphicPri))) {
						isBar=false;
					}
					float x=tcData.GetTransX(permId);
					float halfWidth=ToothGraphic.GetWidth(permId)/2f;
					float leftSpace=Math.Min(x-halfWidth,x+halfWidth);
					float rightSpace=Math.Max(x-halfWidth,x+halfWidth);
					color=Color.FromArgb(255,color);
					if(sourceRun!=null && (!isBar || color.ToArgb()!=colorRun.ToArgb())) {
						AddBox(building,ChartMatrix.Identity,(left+right)/2f,yBar,0,right-left,SpaceMaintainerHeightMm,SpaceMaintainerDepthMm,colorRun,sourceRun,true);
						sourceRun=null;
					}
					if(!isBar) {
						continue;
					}
					if(sourceRun==null) {
						left=leftSpace;
						right=rightSpace;
						colorRun=color;
						sourceRun=SourceSpaceMaintainer+permId;
						continue;
					}
					left=Math.Min(left,leftSpace);
					right=Math.Max(right,rightSpace);
					sourceRun+=","+permId;
				}
				if(sourceRun!=null) {
					AddBox(building,ChartMatrix.Identity,(left+right)/2f,yBar,0,right-left,SpaceMaintainerHeightMm,SpaceMaintainerDepthMm,colorRun,sourceRun,true);
				}
			}
		}
		#endregion Orthodontics, retained roots and space maintainers (TC-6)

		#region Fonts
		///<summary>DirectX's PrepareForDirectX: the numbers' font, Arial with a cell height of 14 and an average width of 5 pixels at the original
		///size, both times the square root of the pixel scale ratio.</summary>
		public static ChartFont GetFontNumbers(float pixelScaleRatio) {
			return ChartFontMeasure.GetFont("Arial",(int)Math.Round((float)(14*Math.Sqrt(pixelScaleRatio))),(int)Math.Round((float)(5*Math.Sqrt(pixelScaleRatio))),400);
		}

		///<summary>The sealant's "S": Arial 25/9 times the ratio.</summary>
		public static ChartFont GetFontSealant(float pixelScaleRatio) {
			return ChartFontMeasure.GetFont("Arial",(int)Math.Round(25*pixelScaleRatio),(int)Math.Round(9*pixelScaleRatio),400);
		}

		///<summary>The watch's "W": Arial 15/6 times the ratio.</summary>
		public static ChartFont GetFontWatch(float pixelScaleRatio) {
			return ChartFontMeasure.GetFont("Arial",(int)Math.Round(15*pixelScaleRatio),(int)Math.Round(6*pixelScaleRatio),400);
		}

		///<summary>TC-5's free-text labels: Arial with a cell height of 14 times the ratio and its own width, normal weight. DirectX drew no free text,
		///so there is nothing to match; the size reads like the tooth numbers'.</summary>
		public static ChartFont GetFontFreeText(float pixelScaleRatio) {
			return ChartFontMeasure.GetFont("Arial",Math.Max(1,(int)Math.Round(14*pixelScaleRatio)),0,400);
		}

		///<summary>TC-5's paint text (paint type Text other than "W"), in the watch mark's band: the watch mark's own font (GetFontWatch: Arial 15/6 times
		///the ratio, regular weight), as Open Dental draws "UCF" beside its "W".</summary>
		public static ChartFont GetFontPaintText(float pixelScaleRatio) {
			return GetFontWatch(pixelScaleRatio);
		}
		#endregion Fonts

		///<summary>The state of one Build: the data, an empty scene of the right size and projection for the projections, the fonts and the item
		///lists in the order they are added.</summary>
		public class SceneBuilding {
			public readonly ToothChartData TcData;
			///<summary>The scene's size and projection, with no items: ProjectDirectX and ProjectToImage need only those.</summary>
			public readonly ToothScene Scene;
			public readonly List<ToothSceneItem3D> ListItems3D=new List<ToothSceneItem3D>();
			public readonly List<ToothSceneItem2D> ListItems2D=new List<ToothSceneItem2D>();
			///<summary>TC-3c: the chart's half-transparent 3D items (unerupted teeth), the scene's Items3DUnder, drawn beneath the opaque ones.</summary>
			public readonly List<ToothSceneItem3D> ListItems3DUnder=new List<ToothSceneItem3D>();
			private ChartFont _fontNumbers;
			private ChartFont _fontSealant;
			private ChartFont _fontWatch;
			private ChartFont _fontFreeText;
			private ChartFont _fontPaintText;

			public SceneBuilding(ToothChartData tcData,Size size,float widthProj,float heightProj) {
				TcData=tcData;
				Scene=new ToothScene(size,tcData.ColorBackground,tcData.PerioMode,widthProj,heightProj,new List<ToothSceneItem3D>(),new List<ToothSceneItem2D>());
			}

			public ChartFont FontNumbers {
				get {
					if(_fontNumbers==null) {
						_fontNumbers=GetFontNumbers(TcData.PixelScaleRatio);
					}
					return _fontNumbers;
				}
			}

			public ChartFont FontSealant {
				get {
					if(_fontSealant==null) {
						_fontSealant=GetFontSealant(TcData.PixelScaleRatio);
					}
					return _fontSealant;
				}
			}

			public ChartFont FontWatch {
				get {
					if(_fontWatch==null) {
						_fontWatch=GetFontWatch(TcData.PixelScaleRatio);
					}
					return _fontWatch;
				}
			}

			public ChartFont FontFreeText {
				get {
					if(_fontFreeText==null) {
						_fontFreeText=GetFontFreeText(TcData.PixelScaleRatio);
					}
					return _fontFreeText;
				}
			}

			public ChartFont FontPaintText {
				get {
					if(_fontPaintText==null) {
						_fontPaintText=GetFontPaintText(TcData.PixelScaleRatio);
					}
					return _fontPaintText;
				}
			}
		}
	}

	///<summary>Where one free-text label is drawn (ToothSceneBuilder.GetFreeTextBoxes, TC-5).</summary>
	public sealed class FreeTextBox {
		///<summary>The label, as the chart's data holds it (not a copy): the mouse moves it in place while it is dragged.</summary>
		public ToothChartFreeText FreeText;
		///<summary>The text cell in pixels of the picture: the label's top left and its text's width and the font's cell height.</summary>
		public Rectangle Bounds;
		///<summary>The label's point in the 410x307 drawing space where it is seen: its own point, or the point it was moved back inside the picture
		///to. A drag starts from here, so the label stays under the mouse.</summary>
		public PointF LocationShown;
	}

	///<summary>GDI fonts and text widths for the 2D layer. The DirectX chart's D3DX fonts are GDI fonts (D3DXCreateFont passes its height, width
	///and weight to CreateFont), so GDI gives their exact metrics and widths, with no WPF object, on any thread. Fonts and widths are cached; a lock
	///keeps the one memory DC to one thread at a time.</summary>
	public static class ChartFontMeasure {
		private static readonly object _lock=new object();
		private static IntPtr _hdc=IntPtr.Zero;
		private static Dictionary<string,FontEntry> _dictFonts=new Dictionary<string,FontEntry>();
		private static Dictionary<string,Size> _dictSizes=new Dictionary<string,Size>();
		private const int MaxFonts=64;
		private const int MaxSizes=4096;

		private class FontEntry {
			public IntPtr HFont;
			public ChartFont ChartFont;
		}

		///<summary>The font as D3DX makes it: CreateFont(height, width, 0, 0, weight, no italic, ANSI_CHARSET, OUT_DEVICE_PRECIS, CLIP_DEFAULT_PRECIS,
		///CLEARTYPE_QUALITY, DEFAULT_PITCH, face), with GDI's ascent and em height.</summary>
		public static ChartFont GetFont(string faceName,int height,int width,int weight) {
			lock(_lock) {
				return GetFontEntry(faceName,height,width,weight).ChartFont;
			}
		}

		///<summary>The size of one line of text in the font, as GDI's GetTextExtentPoint32 gives it: the advance width and the cell height.</summary>
		public static Size MeasureText(ChartFont chartFont,string text) {
			if(string.IsNullOrEmpty(text)) {
				return new Size(0,chartFont.Height);
			}
			string key=GetKey(chartFont.FaceName,chartFont.Height,chartFont.Width,chartFont.Weight)+"\n"+text;
			lock(_lock) {
				Size size;
				if(_dictSizes.TryGetValue(key,out size)) {
					return size;
				}
				FontEntry fontEntry=GetFontEntry(chartFont.FaceName,chartFont.Height,chartFont.Width,chartFont.Weight);
				IntPtr hdc=GetDC();
				IntPtr hFontOld=SelectObject(hdc,fontEntry.HFont);
				try {
					SIZE sizeNative;
					if(!GetTextExtentPoint32(hdc,text,text.Length,out sizeNative)) {
						throw new InvalidOperationException("GDI could not measure the text \""+text+"\" in "+chartFont+".");
					}
					size=new Size(sizeNative.cx,sizeNative.cy);
				}
				finally {
					SelectObject(hdc,hFontOld);
				}
				if(_dictSizes.Count>=MaxSizes) {
					_dictSizes.Clear();
				}
				_dictSizes[key]=size;
				return size;
			}
		}

		private static string GetKey(string faceName,int height,int width,int weight) {
			return faceName+"|"+height+"|"+width+"|"+weight;
		}

		private static FontEntry GetFontEntry(string faceName,int height,int width,int weight) {
			string key=GetKey(faceName,height,width,weight);
			FontEntry fontEntry;
			if(_dictFonts.TryGetValue(key,out fontEntry)) {
				return fontEntry;
			}
			if(_dictFonts.Count>=MaxFonts) {
				foreach(FontEntry fontEntryOld in _dictFonts.Values) {
					DeleteObject(fontEntryOld.HFont);
				}
				_dictFonts.Clear();
				_dictSizes.Clear();
			}
			const int ANSI_CHARSET=0;
			const int OUT_DEVICE_PRECIS=5;
			const int CLIP_DEFAULT_PRECIS=0;
			const int CLEARTYPE_QUALITY=5;
			const int DEFAULT_PITCH=0;
			IntPtr hFont=CreateFont(height,width,0,0,weight,0,0,0,ANSI_CHARSET,OUT_DEVICE_PRECIS,CLIP_DEFAULT_PRECIS,CLEARTYPE_QUALITY,DEFAULT_PITCH,faceName);
			if(hFont==IntPtr.Zero) {
				throw new InvalidOperationException("GDI could not make the font "+key+".");
			}
			IntPtr hdc=GetDC();
			IntPtr hFontOld=SelectObject(hdc,hFont);
			TEXTMETRIC textMetric;
			try {
				if(!GetTextMetrics(hdc,out textMetric)) {
					throw new InvalidOperationException("GDI gave no metrics for the font "+key+".");
				}
			}
			finally {
				SelectObject(hdc,hFontOld);
			}
			fontEntry=new FontEntry();
			fontEntry.HFont=hFont;
			fontEntry.ChartFont=new ChartFont(faceName,height,width,weight,textMetric.tmAscent,textMetric.tmHeight-textMetric.tmInternalLeading);
			_dictFonts[key]=fontEntry;
			return fontEntry;
		}

		private static IntPtr GetDC() {
			if(_hdc==IntPtr.Zero) {
				_hdc=CreateCompatibleDC(IntPtr.Zero);
				if(_hdc==IntPtr.Zero) {
					throw new InvalidOperationException("GDI could not make a memory device context.");
				}
			}
			return _hdc;
		}

		#region Native
		[StructLayout(LayoutKind.Sequential)]
		private struct SIZE {
			public int cx;
			public int cy;
		}

		[StructLayout(LayoutKind.Sequential,CharSet=CharSet.Unicode)]
		private struct TEXTMETRIC {
			public int tmHeight;
			public int tmAscent;
			public int tmDescent;
			public int tmInternalLeading;
			public int tmExternalLeading;
			public int tmAveCharWidth;
			public int tmMaxCharWidth;
			public int tmWeight;
			public int tmOverhang;
			public int tmDigitizedAspectX;
			public int tmDigitizedAspectY;
			public char tmFirstChar;
			public char tmLastChar;
			public char tmDefaultChar;
			public char tmBreakChar;
			public byte tmItalic;
			public byte tmUnderlined;
			public byte tmStruckOut;
			public byte tmPitchAndFamily;
			public byte tmCharSet;
		}

		[DllImport("gdi32.dll",CharSet=CharSet.Unicode,EntryPoint="CreateFontW")]
		private static extern IntPtr CreateFont(int nHeight,int nWidth,int nEscapement,int nOrientation,int fnWeight,uint fdwItalic,uint fdwUnderline,
			uint fdwStrikeOut,uint fdwCharSet,uint fdwOutputPrecision,uint fdwClipPrecision,uint fdwQuality,uint fdwPitchAndFamily,string lpszFace);

		[DllImport("gdi32.dll")]
		private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

		[DllImport("gdi32.dll")]
		private static extern IntPtr SelectObject(IntPtr hdc,IntPtr hgdiobj);

		[DllImport("gdi32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool DeleteObject(IntPtr hObject);

		[DllImport("gdi32.dll",CharSet=CharSet.Unicode,EntryPoint="GetTextMetricsW")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool GetTextMetrics(IntPtr hdc,out TEXTMETRIC lptm);

		[DllImport("gdi32.dll",CharSet=CharSet.Unicode,EntryPoint="GetTextExtentPoint32W")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool GetTextExtentPoint32(IntPtr hdc,string lpString,int c,out SIZE lpSize);
		#endregion Native
	}
}
