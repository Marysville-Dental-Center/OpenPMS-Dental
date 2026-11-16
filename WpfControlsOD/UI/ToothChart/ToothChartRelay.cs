using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CodeBase;
using OpenPmsDental.UI;
using OpenPmsDentalBusiness;
using SparksToothChart;

namespace OpenPmsDental{
	///<summary>Relays tooth chart commands to SparksToothChart.ToothChartWrapper, the only tooth chart. Open Dental's closed DirectX 11 chart
	///(Sparks3D.dll, under Open Dental's own licence) was removed on 2026-11-01; the WPF chart takes its place, chosen by Open Dental's own DirectX 11
	///settings (TC-8b, GetEngineForComputer). The wrapper draws with the WPF chart or the Simple 2D chart, Open Dental's one older technology left,
	///and falls back to Simple 2D on its own when the WPF chart cannot start. The features only Sparks3D drew are drawn by the WPF chart: free text and
	///paint text other than "W" since TC-5 (TextBelow is shown by the Chart module, outside the tooth picture), ortho mode, orthodontic hardware,
	///retained roots and space maintainers since TC-6; the Simple 2D chart ignores those last four, as Open Dental's older charts do. Since TC-2 the
	///callers' painting code is ToothChartPainter, which draws on the relay as its IToothChartPaintTarget, and the callers' chart settings are the
	///relay's settings helpers.</summary>
	public class ToothChartRelay:IToothChartPaintTarget{
		private ToothChartWrapper _toothChartWrapper;

		///<summary>hasHwnd is kept for callers; the wrapper always needs a window handle.</summary>
		public ToothChartRelay(bool hasHwnd=true){
		}

		#region Properties
		public int Bottom{
			get{
				return _toothChartWrapper.Bottom;
			}
		}

		public Color ColorBackgroundMain{
			set{
				_toothChartWrapper.ColorBackground=value;
			}
		}

		public Color ColorDrawing{
			set{
				_toothChartWrapper.ColorDrawing=value;
			}
		}

		public Color ColorText{
			set{
				_toothChartWrapper.ColorText=value;
			}
		}

		public Color ColorTextHighlightBack{
			set{
				_toothChartWrapper.ColorBackHighlight=value;
			}
		}

		public Color ColorTextHighlightFore{
			set{
				_toothChartWrapper.ColorTextHighlight=value;
			}
		}

		public CursorTool CursorTool{
			get{
				return _toothChartWrapper.CursorTool;
			}
			set{
				_toothChartWrapper.CursorTool=value;
			}
		}

		public bool Enabled{
			set{
				_toothChartWrapper.Enabled=value;
			}
		}

		public int Height{
			get{
				return _toothChartWrapper.Height;
			}
		}

		///<summary>Sets the wrapper's perio mode to the value given. Turning it on is only allowed in the WPF chart (since TC-7; the wrapper throws
		///otherwise).</summary>
		public bool IsPerioMode{
			set{
				_toothChartWrapper.PerioMode=value;
			}
		}

		public List<string> SelectedTeeth{
			get{
				return _toothChartWrapper.SelectedTeeth;
			}
		}

		public int Width{
			get{
				return _toothChartWrapper.Width;
			}
		}
		#endregion Properties

		#region Methods
		public void AddDrawingSegment(ToothInitial drawingSegment){
			_toothChartWrapper.AddDrawingSegment(drawingSegment);
		}

		///<summary>A wire from one bracket to the next: a thin line in the WPF chart (TC-6, ORTHO-2); the other engines ignore it.</summary>
		public void AddOrthoWireBetweenBrackets(string toothIDstart,string toothIDend,Color color){
			_toothChartWrapper.AddOrthoWireBetweenBrackets(toothIDstart,toothIDend,color);
		}

		///<summary>A wire across one bracket: a thin line in the WPF chart (TC-6, ORTHO-2); the other engines ignore it.</summary>
		public void AddOrthoWireInBracket(string toothID,Color color){
			_toothChartWrapper.AddOrthoWireInBracket(toothID,color);
		}

		///<summary>An elastic between two brackets: a thin line in the WPF chart (TC-6, ORTHO-2); the other engines ignore it.</summary>
		public void AddOrthoElastic(string toothIDstart,string toothIDend,Color color){
			_toothChartWrapper.AddOrthoElastic(toothIDstart,toothIDend,color);
		}

		public void AddPerioMeasure(int intTooth,PerioSequenceType sequenceType,int mb,int b,int db,int ml,int l, int dl){
			_toothChartWrapper.AddPerioMeasure(intTooth,sequenceType,mb,b,db,ml,l,dl);
		}

		///<summary>A free-text label (a toothinitial row of type Text) at location, the top left of its text in the chart's 410x307 drawing space. Not
		///to be confused with SetText. The WPF chart and the 2D chart draw it (TC-5); with CursorTool.MoveText the WPF chart moves it and raises
		///TextMoved.</summary>
		public void AddText(string text,PointF location,Color color,long toothInitialNum){
			_toothChartWrapper.AddText(text,location,color,toothInitialNum);
		}

		///<summary>toothChartWrapper.SuspendLayout</summary>
		public void BeginUpdate(){
			_toothChartWrapper.SuspendLayout();//badly named
		}

		public void DisposeControl(){
			_toothChartWrapper.Dispose();
		}

		///<summary>toothChartWrapper.ResumeLayout</summary>
		public void EndUpdate(){
			_toothChartWrapper.ResumeLayout();//badly named
		}

		public Bitmap GetBitmap(){
			return _toothChartWrapper.GetBitmap();
		}

		public void MoveTooth(string toothID,float rotate,float tipM,float tipB,float shiftM,float shiftO,float shiftB){
			_toothChartWrapper.MoveTooth(toothID,rotate,tipM,tipB,shiftM,shiftO,shiftB);
		}

		public void ResetTeeth(){
			_toothChartWrapper.ResetTeeth();
		}

		public void SetBigX(string toothID,Color color){
			_toothChartWrapper.SetBigX(toothID,color);
		}

		///<summary>A bracket on the tooth's facial surface, drawn by the WPF chart (TC-6, ORTHO-2); the other engines ignore it.</summary>
		public void SetBracket(string toothID,Color color){
			_toothChartWrapper.SetBracket(toothID,color);
		}

		public void SetBU(string toothID,Color color){
			_toothChartWrapper.SetBU(toothID,color);
		}

		public void SetCrown(string toothID,Color color){
			_toothChartWrapper.SetCrown(toothID,color);
		}

		public void SetHidden(string toothID){
			_toothChartWrapper.SetHidden(toothID);
		}

		public void SetImplant(string toothID,Color color){
			_toothChartWrapper.SetImplant(toothID,color);
		}

		public void SetMissing(string toothID){
			_toothChartWrapper.SetMissing(toothID);
		}

		public void SetMobility(string toothID,string mobility,Color color){
			_toothChartWrapper.SetMobility(toothID,mobility,color);
		}

		///<summary>Ortho mode: the WPF chart draws only the facial rows, moved toward the centre line (TC-6, ORTHO-1); the other engines ignore it. The
		///callers switch Universal numbering to Palmer themselves.</summary>
		public void SetOrthoMode(bool isOrthoMode){
			_toothChartWrapper.SetOrthoMode(isOrthoMode);
		}

		public void SetPerioColors(Color colorBleeding,Color colorSuppuration,Color colorProbing,Color colorProbingRed,Color colorGM,Color colorCAL,
			Color colorMGJ,Color colorFurcations,Color colorFurcationsRed,int redLimitProbing,int redLimitFurcations){
			_toothChartWrapper.ColorBleeding=colorBleeding;
			_toothChartWrapper.ColorSuppuration=colorSuppuration;
			_toothChartWrapper.ColorProbing=colorProbing;
			_toothChartWrapper.ColorProbingRed=colorProbingRed;
			_toothChartWrapper.ColorGingivalMargin=colorGM;
			_toothChartWrapper.ColorCAL=colorCAL;
			_toothChartWrapper.ColorMGJ=colorMGJ;
			_toothChartWrapper.ColorFurcations=colorFurcations;
			_toothChartWrapper.ColorFurcationsRed=colorFurcationsRed;
			_toothChartWrapper.RedLimitProbing=redLimitProbing;
			_toothChartWrapper.RedLimitFurcations=redLimitFurcations;
		}

		public void SetPontic(string toothID,Color color){
			_toothChartWrapper.SetPontic(toothID,color);
		}

		public void SetPrimary(string toothID){
			_toothChartWrapper.SetPrimary(toothID);
		}

		public void SetRCT(string toothID,Color color){
			_toothChartWrapper.SetRCT(toothID,color);
		}

		///<summary>Paint type RetainedRoot: the root only, in the colour, drawn by the WPF chart on a tooth that is shown (TC-6, RR-1); the other engines
		///ignore it.</summary>
		public void SetRetainedRoot(string toothID,Color color){
			_toothChartWrapper.SetRetainedRoot(toothID,color);
		}

		public void SetSealant(string toothID,Color color){
			_toothChartWrapper.SetSealant(toothID,color);
		}

		public void SetSelected(string toothID,bool setValue){
			_toothChartWrapper.SetSelected(toothID,setValue);
		}

		///<summary>Paint type SpaceMaintainer, one tooth of the procedure: a solid bar across the tooth's space when no tooth is drawn there, drawn by the
		///WPF chart (TC-6, SM-1); the other engines ignore it.</summary>
		public void SetSpaceMaintainer(string toothID,Color color){
			_toothChartWrapper.SetSpaceMaintainer(toothID,color);
		}

		public void SetSurfaceColors(string toothID,string surfaces,Color color){
			_toothChartWrapper.SetSurfaceColors(toothID,surfaces,color);
		}

		///<summary>Paint type Text: "W" is the watch mark; any other text is drawn beyond the root tip, in the watch mark's band (O-22, TC-5).</summary>
		public void SetText(string toothID,Color color,string text){
			if(text=="W"){
				_toothChartWrapper.SetWatch(toothID,color);
				return;
			}
			_toothChartWrapper.SetText(toothID,color,text);
		}

		///<summary>Also passes the wrapper's TextMoved and TextColorChanged on as this relay's.</summary>
		public void SetToothChartWrapper(ToothChartWrapper toothChartWrapper){
			if(_toothChartWrapper!=null) {
				_toothChartWrapper.TextMoved-=ToothChartWrapper_TextMoved;
				_toothChartWrapper.TextColorChanged-=ToothChartWrapper_TextColorChanged;
			}
			_toothChartWrapper=toothChartWrapper;
			if(_toothChartWrapper!=null) {
				_toothChartWrapper.TextMoved+=ToothChartWrapper_TextMoved;
				_toothChartWrapper.TextColorChanged+=ToothChartWrapper_TextColorChanged;
			}
		}

		public void SetToothNumberingNomenclature(ToothNumberingNomenclature toothNumberingNomenclature){
			_toothChartWrapper.SetToothNumberingNomenclature(toothNumberingNomenclature);
		}

		public void SetVeneer(string toothID,Color color){
			_toothChartWrapper.SetVeneer(toothID,color);
		}
		#endregion Methods

		#region Colors
		private static List<UserOdPref> _listUserOdPrefsToothChartColor;
		
		/// <summary> Checks to see if preferences are up to date. If they are, no database query is made. However if a change occurs for a user, the list needs to update itself.
		internal static List<UserOdPref> GetListUserOdPrefs() {
			if(_listUserOdPrefsToothChartColor==null) {
				RefreshToothColorsPrefs();
			}
			return _listUserOdPrefsToothChartColor;
		}

		///<summary>Checks ProvNums only if the logged in user has 'ToothChartUsesDiffColorByProv' user pref on (ToothChartPainter.DoesToothColorPrefApply).</summary>
		public bool DoesToothColorPrefApply(List<long> listCurProvNums,long procProvNum) {
			return ToothChartPainter.DoesToothColorPrefApply(listCurProvNums,procProvNum);
		}

		///<summary>The ProvNums whose completed work keeps its own colour (ToothChartPainter.GetPertinentProvNumsForToothColorPref).</summary>
		public List<long> GetPertinentProvNumsForToothColorPref(Userod user,Patient patCur,List<Appointment> listAppts) {
			return ToothChartPainter.GetPertinentProvNumsForToothColorPref(user,patCur,listAppts);
		}

		///<summary>Sets colors for the teeth in the graphical tooth chart (ToothChartPainter.GetToothColors).</summary>
		public bool GetToothColors(ProcedureCode curProc,ProcStat procStatus,bool doUseEcEoForCProcs,out Color cDark,out Color cLight) {
			return ToothChartPainter.GetToothColors(curProc,procStatus,doUseEcEoForCProcs,out cDark,out cLight);
		}

		///<summary>Will update list of users with preference if a change is made while OD is running</summary>
		public static void RefreshToothColorsPrefs() {
			_listUserOdPrefsToothChartColor=UserOdPrefs.GetByFkeyType(UserOdFkeyType.ToothChartUsesDiffColorByProv);
		}

		///<summary>Returns true if the currently logged in user has ColorChartUsesDiffByProv preference (ToothChartPainter.HasToothColorUserPref).</summary>
		public bool HasToothColorUserPref(){
			return ToothChartPainter.HasToothColorUserPref();
		}

		#endregion Colors

		#region Settings helpers (TC-2, TC-8, TC-8b)
		///<summary>The status line's word when the 3D tooth chart can start on this computer (File > Graphics, TC-8b, NEW-1), as Open Dental's
		///"DirectX 11 status on this computer" says it.</summary>
		public const string Chart3DStatusAvailable="Available";
		///<summary>The start of the status line when the 3D tooth chart cannot start on this computer: "Fell back: " and the reason (TC-8b, NEW-1).</summary>
		public const string Chart3DStatusFellBackPrefix="Fell back: ";
		///<summary>The graphical perio chart's refusal on a computer that draws the Simple 2D chart (TC-8b, ENG-5): Open Dental's own sentence for the
		///same case ("In the Graphics setup window, you must first select DirectX.", its 24.3 FormPerio, public in its source), with the 3D tooth
		///chart in DirectX's place and the menu named. The exact wording of Open Dental 26.2 for a computer not on DirectX 11 is not public
		///(od-unknown, docs/port-notes/TC-8b.md).</summary>
		public const string PerioNeeds3DChartMessage="In the Graphics setup window (File > Graphics), you must first select the 3D tooth chart.";

		///<summary>Open Dental's global "DirectX 11 Tooth Chart on all computers" setting, the fork's "3D Tooth Chart on all computers" (TC-8b, ENG-2):
		///the preference DirectX11ToothChartUseIfAvail, "Use if available" (true, the default, also when the row or the cache is missing, as Open
		///Dental's 24.3 start-up read it) or "Do not use" (false). No schema change: the row is Open Dental's own.</summary>
		public static bool IsChart3DOnAllComputers() {
			return PrefC.GetBoolSilent(PrefName.DirectX11ToothChartUseIfAvail,true);
		}

		///<summary>Saves the global setting (IsChart3DOnAllComputers) and returns true when the stored value changed; the caller then signals the
		///preference cache (DataValid.SetInvalid(InvalidType.Prefs)), as Open Dental's Graphics window did.</summary>
		public static bool SetChart3DOnAllComputers(bool isChart3DOnAllComputers) {
			return Prefs.UpdateBool(PrefName.DirectX11ToothChartUseIfAvail,isChart3DOnAllComputers);
		}

		///<summary>Open Dental's per-computer "DirectX 11 Tooth Chart" setting, the fork's "3D Tooth Chart" (TC-8b, ENG-2): computerpref.GraphicsUseDirectX11,
		///Unknown "Use the global setting" (a new computer's 0), Yes "Use if available", No "Do not use". Unknown for a null computer.</summary>
		public static YN GetChart3DThisComputer(ComputerPref computerPref) {
			if(computerPref==null) {
				return YN.Unknown;
			}
			return computerPref.GraphicsUseDirectX11;
		}

		///<summary>True when a computer uses the 3D tooth chart (TC-8b, ENG-2), as Open Dental 24.3 decided it at start-up for its DirectX 11 chart: the
		///computer's own "Use if available" or "Do not use" wins, and "Use the global setting" (or any other stored number) follows the global setting.</summary>
		public static bool IsChart3DUsed(bool isChart3DOnAllComputers,YN chart3DThisComputer) {
			if(chart3DThisComputer==YN.Yes) {
				return true;
			}
			if(chart3DThisComputer==YN.No) {
				return false;
			}
			return isChart3DOnAllComputers;
		}

		///<summary>The engine a computer draws with (TC-8b, ENG-2): the WPF chart when it uses the 3D tooth chart (IsChart3DUsed), which falls back to
		///Simple 2D on its own, logged once, when it cannot start. Otherwise the older technology, which Open Dental chooses with computerpref.GraphicsSimple
		///("only Do not use lets GraphicsSimple apply"): of Open Dental's three (DirectX 9, Simple 2D, OpenGL) only Simple 2D is left since TC-9a, so every
		///stored value draws Simple 2D; a stored 0 or 2 is kept for a return to Open Dental (decision 22), which draws its DirectX 9 or OpenGL chart there.
		///A computer stored as Simple 2D (1) gets the 3D chart unless its setting is "Do not use" (revises O-30).</summary>
		public static ToothChartEngine GetEngineForComputer(bool isChart3DOnAllComputers,ComputerPref computerPref) {
			if(IsChart3DUsed(isChart3DOnAllComputers,GetChart3DThisComputer(computerPref))) {
				return ToothChartEngine.Wpf3D;
			}
			return ToothChartEngine.Simple2D;
		}

		///<summary>GetEngineForComputer with the office's global setting (IsChart3DOnAllComputers).</summary>
		public static ToothChartEngine GetEngineForComputer(ComputerPref computerPref) {
			return GetEngineForComputer(IsChart3DOnAllComputers(),computerPref);
		}

		///<summary>True when the computer's WPF chart shows the smoothed picture on screen (the renderer's software image, the same as every printout):
		///since TC-8b (ENG-3, revising O-34 (b1)) every computer's default, an empty computerpref.DirectXFormat included. Only the per-computer option
		///"Draw in software" unticked, stored as ToothChartWrapper.DirectXFormatWpfLiveView, shows the quick live view. TC-8's "WpfSoftware" and a DirectX
		///device format an older build saved are the default. True for a null computer.</summary>
		public static bool IsDrawnInSoftware(ComputerPref computerPref) {
			return computerPref==null || !ToothChartWrapper.IsWpfLiveViewFormat(computerPref.DirectXFormat);
		}

		///<summary>Applies the local computer's chart settings (ApplyComputerPref with ComputerPrefs.LocalComputer) to a wrapper and starts its engine.
		///One helper for the copies each caller had before TC-2 (ControlChart twice, ControlTreat, SheetPrinting, FormSheetDefEdit and
		///FormToothChartingBig). The copies differed a little before TC-8 (the hardware flag, the OpenGL pixel format and the DirectX device format,
		///set and saved in different ways); since TC-8 every caller applies the same two settings and nothing is saved (no ComputerPrefs.Update when an
		///image is made, or ever here), so TC-9a removed the parameter that named each caller's old way (ToothChartSettingsUse).</summary>
		public static void ApplyComputerSettings(ToothChartWrapper toothChartWrapper) {
			ApplyComputerPref(toothChartWrapper,ComputerPrefs.LocalComputer);
		}

		///<summary>Applies one computer's chart settings to a wrapper: first its "Draw in software" switch (IsDrawnInSoftware), so a computer drawn in
		///software never makes a live view first (TC-3's fix round 3), then the engine the computer draws with (GetEngineForComputer, TC-8b): the WPF
		///chart, which falls back to Simple 2D with one log line when it cannot start, or the 2D chart. Reads computerPref and the global setting only;
		///saves nothing.</summary>
		public static void ApplyComputerPref(ToothChartWrapper toothChartWrapper,ComputerPref computerPref) {
			toothChartWrapper.WpfDrawInSoftware=IsDrawnInSoftware(computerPref);
			toothChartWrapper.Engine=GetEngineForComputer(computerPref);
		}

		///<summary>The gate of the graphical perio chart (FormPerio's Graphical button), moved from FormPerio in TC-2. Since TC-8b (ENG-5, revising O-31)
		///it opens only on a computer that uses the 3D tooth chart (GetEngineForComputer), as Open Dental opens it only on its 3D charts (its Graphics
		///page: none on Simple 2D); on a computer that draws Simple 2D it refuses with PerioNeeds3DChartMessage, which names File > Graphics. A 3D chart
		///that cannot start falls back to Simple 2D when FormPerioGraphical starts it, and the perio mode then says so (ApplyGraphicalPerioMode).</summary>
		public static bool CanOpenGraphicalPerio(out string message) {
			return CanOpenGraphicalPerio(IsChart3DOnAllComputers(),ComputerPrefs.LocalComputer,out message);
		}

		///<summary>CanOpenGraphicalPerio for a given global setting and computer.</summary>
		public static bool CanOpenGraphicalPerio(bool isChart3DOnAllComputers,ComputerPref computerPref,out string message) {
			if(GetEngineForComputer(isChart3DOnAllComputers,computerPref)!=ToothChartEngine.Wpf3D) {
				message=PerioNeeds3DChartMessage;
				return false;
			}
			message="";
			return true;
		}

		///<summary>Starts the graphical perio chart's engine (FormPerioGraphical), moved from FormPerioGraphical in TC-2: the WPF chart, which draws perio
		///since TC-7, with the computer's "Draw in software" switch set first, as ApplyComputerPref sets it. CanOpenGraphicalPerio lets only a computer
		///that uses the 3D tooth chart get here (TC-8b). The chart also gets the office's Recession option (TC-7), which the WPF chart's legend shows. If
		///the WPF chart cannot start, the wrapper falls back to Simple 2D, logged once, and the perio mode that FormPerioGraphical sets next says the 3D
		///chart could not start.</summary>
		public static void ApplyGraphicalPerioMode(ToothChartWrapper toothChartWrapper) {
			toothChartWrapper.PerioUseRecessionTerm=PerioMeasures.GetUseRecessionTerm();
			toothChartWrapper.WpfDrawInSoftware=IsDrawnInSoftware(ComputerPrefs.LocalComputer);
			toothChartWrapper.Engine=ToothChartEngine.Wpf3D;
		}

		///<summary>File > Graphics' save rule (TC-8b, on TC-8's) on computerPref, which the caller then saves with ComputerPrefs.Update; the global
		///setting is saved by the caller (SetChart3DOnAllComputers). Only the columns a choice changes are written, and the stored values stay Open
		///Dental's, so a return to Open Dental (decision 22) reads the same:
		///- The per-computer setting writes GraphicsUseDirectX11 (Unknown, Yes or No) when it changed.
		///- GraphicsSimple follows TC-8's rule for the chart the computer draws, written only when the window's choices change that chart (the global
		///  setting before and after, the computer's setting before and after): a change to Simple 2D writes 1, so Open Dental, whose older technology
		///  then applies, draws Simple 2D too; a change to the 3D chart writes 0 only over a stored 1, and a stored 0 or 2 is kept.
		///- "Draw in software" (ticked, the default since ENG-3) clears the live view's value; unticked it writes ToothChartWrapper.DirectXFormatWpfLiveView.
		///  Any other stored value (TC-8's "WpfSoftware", a DirectX device format an older build saved) is kept while the box stays ticked.</summary>
		public static void SetGraphicsChoice(ComputerPref computerPref,bool isChart3DOnAllComputersBefore,bool isChart3DOnAllComputers,
			YN chart3DThisComputer,bool isDrawnInSoftware)
		{
			bool is3DBefore=IsChart3DUsed(isChart3DOnAllComputersBefore,GetChart3DThisComputer(computerPref));
			bool is3DAfter=IsChart3DUsed(isChart3DOnAllComputers,chart3DThisComputer);
			if(computerPref.GraphicsUseDirectX11!=chart3DThisComputer) {
				computerPref.GraphicsUseDirectX11=chart3DThisComputer;
			}
			if(is3DBefore && !is3DAfter) {
				computerPref.GraphicsSimple=DrawingMode.Simple2D;
			}
			else if(!is3DBefore && is3DAfter && computerPref.GraphicsSimple==DrawingMode.Simple2D) {
				computerPref.GraphicsSimple=DrawingMode.DirectX;
			}
			if(!isDrawnInSoftware) {
				computerPref.DirectXFormat=ToothChartWrapper.DirectXFormatWpfLiveView;
			}
			else if(ToothChartWrapper.IsWpfLiveViewFormat(computerPref.DirectXFormat)) {
				computerPref.DirectXFormat="";
			}
		}

		///<summary>File > Graphics' status line for this computer (TC-8b, NEW-1): Chart3DStatusAvailable when the WPF chart can start in this process,
		///otherwise Chart3DStatusFellBackPrefix and the reason it falls back to Simple 2D (ToothChartWrapper.CanStartWpf3D). In English; the window
		///translates its parts.</summary>
		public static string GetChart3DStatus() {
			if(ToothChartWrapper.CanStartWpf3D(out string reason)) {
				return Chart3DStatusAvailable;
			}
			return Chart3DStatusFellBackPrefix+reason;
		}

		///<summary>File > Graphics' live preview (TC-8): starts the chart the window's choices would give on this computer, the "Draw in software"
		///switch first, then the engine (the WPF chart when the choices use the 3D tooth chart, the 2D chart otherwise), and paints a synthetic sample on
		///it (PaintGraphicsPreview). So a PC whose driver draws no WPF 3D (dotnet/wpf#7076; in the program every WPF window is drawn in software, O-34)
		///shows at once what the choices change. Saves nothing.</summary>
		public static void ApplyGraphicsPreview(ToothChartWrapper toothChartWrapper,bool is3DChart,bool isDrawnInSoftware) {
			toothChartWrapper.WpfDrawInSoftware=isDrawnInSoftware;
			toothChartWrapper.Engine=is3DChart ? ToothChartEngine.Wpf3D : ToothChartEngine.Simple2D;
			ToothChartRelay toothChartRelay=new ToothChartRelay();
			toothChartRelay.SetToothChartWrapper(toothChartWrapper);
			PaintGraphicsPreview(toothChartRelay,Defs.GetDefsForCategory(DefCat.ChartGraphicColors));
		}

		///<summary>The preview's status line in File > Graphics, in English (the window translates it): the chart the preview's wrapper started for the
		///window's choices. A 3D chart that could not start fell back to the 2D chart, logged once, as it does in every other place.</summary>
		public static string GetGraphicsPreviewStatus(ToothChartWrapper toothChartWrapper,bool is3DChart) {
			if(!is3DChart) {
				return "Simple 2D chart.";
			}
			if(toothChartWrapper.Engine!=ToothChartEngine.Wpf3D) {
				return "The 3D chart could not start on this computer, so the Simple 2D chart is shown (the ToothChart log says why).";
			}
			if(toothChartWrapper.WpfDrawInSoftware) {
				return "3D chart, drawn in software: the smoothed picture, the same as printouts and sheets.";
			}
			return "3D chart, quick live view.";
		}
		///<summary>The preview's sample in File > Graphics: no patient, only made-up work in the office's Chart module colours (listDefsChartColors,
		///DefCat.ChartGraphicColors: 10 and 11 the background and text, 0 and 5 planned work, 1 and 6 completed work): a completed MOD filling on 3, a
		///completed crown on 14, a planned root canal with a build-up on 19, a planned extraction on 1 and a missing 30, in the office's numbering.</summary>
		public static void PaintGraphicsPreview(ToothChartRelay toothChartRelay,List<Def> listDefsChartColors) {
			toothChartRelay.BeginUpdate();
			toothChartRelay.SetToothNumberingNomenclature((ToothNumberingNomenclature)PrefC.GetInt(PrefName.UseInternationalToothNumbers));
			toothChartRelay.ColorBackgroundMain=listDefsChartColors[10].ItemColor;
			toothChartRelay.ColorText=listDefsChartColors[11].ItemColor;
			toothChartRelay.ColorTextHighlightFore=listDefsChartColors[12].ItemColor;
			toothChartRelay.ColorTextHighlightBack=listDefsChartColors[13].ItemColor;
			toothChartRelay.ResetTeeth();
			toothChartRelay.SetOrthoMode(false);
			toothChartRelay.SetSurfaceColors("3","MOD",listDefsChartColors[1].ItemColor);
			toothChartRelay.SetCrown("14",listDefsChartColors[6].ItemColor);
			toothChartRelay.SetRCT("19",listDefsChartColors[0].ItemColor);
			toothChartRelay.SetBU("19",listDefsChartColors[0].ItemColor);
			toothChartRelay.SetBigX("1",listDefsChartColors[0].ItemColor);
			toothChartRelay.SetMissing("30");
			toothChartRelay.EndUpdate();
		}
		#endregion Settings helpers (TC-2, TC-8, TC-8b)

		#region Free text (TC-2)
		///<summary>The wrapper's TextMoved, passed on: a free-text label (ToothInitialType.Text) was dragged with CursorTool.MoveText to a point in the
		///chart's 410×307 space. The WPF chart raises it (TC-5); the Chart module saves the move (MoveTextLabel).</summary>
		public event ToothChartTextMovedEventHandler TextMoved;

		private void ToothChartWrapper_TextMoved(object sender,ToothChartTextMovedEventArgs e) {
			TextMoved?.Invoke(this,e);
		}

		///<summary>The wrapper's TextColorChanged, passed on: the Draw tab's Change Color (CursorTool.ColorChanger) was pressed on a free-text label
		///(ToothInitialType.Text), which now has the pen colour (TC-5; Open Dental's manual: "change the color of an existing drawing or text"). The
		///WPF chart raises it; the Chart module saves the colour.</summary>
		public event ToothChartTextColorChangedEventHandler TextColorChanged;

		private void ToothChartWrapper_TextColorChanged(object sender,ToothChartTextColorChangedEventArgs e) {
			TextColorChanged?.Invoke(this,e);
		}

		///<summary>Moves a free-text label to location, a point in the chart's 410×307 space: clamped to 0..410 and 0..307 and rounded to whole
		///numbers, then written with ToothInitial.SetText, the label's text unchanged. Whole numbers matter: SetText writes the point in the
		///workstation's number format, and GetTextPoint reads only digits and a dot, so a decimal comma (de-DE) or a minus sign would move the
		///label to 0,0. The caller saves the label.</summary>
		public static void MoveTextLabel(ToothInitial toothInitial,PointF location) {
			float x=RoundToWhole(location.X,410);
			float y=RoundToWhole(location.Y,307);
			string text="";
			int indexSemicolon=(toothInitial.DrawText==null) ? -1 : toothInitial.DrawText.IndexOf(';');
			if(indexSemicolon>=0) {
				text=toothInitial.DrawText.Substring(indexSemicolon+1);//everything after the point, a semicolon in the text included
			}
			toothInitial.SetText(new PointF(x,y),text);
		}

		///<summary>value clamped to 0..max and rounded to a whole number (halves away from zero); 0 for a value that is not a number.</summary>
		private static float RoundToWhole(float value,int max) {
			if(float.IsNaN(value)) {
				return 0;
			}
			double clamped=Math.Min(Math.Max(value,0),max);
			return (float)Math.Round(clamped,MidpointRounding.AwayFromZero);
		}
		#endregion Free text (TC-2)

		#region The dashboard's picture made in the background (TC-3b)
		///<summary>The picture GetBitmap would return now, made in the background so the caller does not wait for it: the WPF chart's picture for the
		///Patient Dashboard (ControlChart.FillToothChart, TC-3b). Null when the chart cannot make it that way (Simple 2D, a chart
		///with no window handle, a call off its thread); the caller then calls GetBitmap, as before. See ToothChartWrapper.RequestBitmapInBackground.</summary>
		public ToothChartBitmapRequest RequestBitmapInBackground(){
			return _toothChartWrapper.RequestBitmapInBackground();
		}
		#endregion The dashboard's picture made in the background (TC-3b)

		#region Graphical perio (TC-7)
		///<summary>The office's Perio Setup option "Use the term Recession" (Open Dental 24.4) for the graphical perio chart: the WPF chart's legend then
		///names the gingival margin line "Recession", unless the exam has hyperplasia. ApplyGraphicalPerioMode sets it from the preference.</summary>
		public bool IsPerioRecessionTerm{
			get{
				return _toothChartWrapper.PerioUseRecessionTerm;
			}
			set{
				_toothChartWrapper.PerioUseRecessionTerm=value;
			}
		}
		#endregion Graphical perio (TC-7)
	}

	///<summary>The chart settings FormOpenDental compares after a refresh, to redraw the Chart module only when one of them changed
	///(NeedsRedraw("ChartModule")). Moved from FormOpenDental (TC-2); FormOpenDental keeps its task-list cache. It holds the engine the computer
	///draws with (GetEngineForComputer, TC-8b: the global 3D setting, this computer's and GraphicsSimple) and the "Draw in software" switch, in place
	///of the DirectX and OpenGL fields (the hardware flag, the pixel format and the DirectX format), which no chart reads any more: a change that
	///leaves the engine as it was (0 to 2, or the global setting under a computer's own choice) needs no redraw.</summary>
	public class ToothChartSettingsSnapshot {
		private Dictionary<string,object> _dictionaryChartPrefsCache=new Dictionary<string,object>();

		///<summary>Takes the settings as they are now, replacing the last snapshot.</summary>
		public void Take() {
			_dictionaryChartPrefsCache.Clear();
			//Chart Drawing Prefs
			_dictionaryChartPrefsCache.Add(PrefName.UseInternationalToothNumbers.ToString(),PrefC.GetInt(PrefName.UseInternationalToothNumbers));
			_dictionaryChartPrefsCache.Add("ToothChartEngine",ToothChartRelay.GetEngineForComputer(ComputerPrefs.LocalComputer));
			_dictionaryChartPrefsCache.Add("WpfDrawInSoftware",ToothChartRelay.IsDrawnInSoftware(ComputerPrefs.LocalComputer));
			_dictionaryChartPrefsCache.Add(PrefName.ShowFeatureEhr.ToString(),PrefC.GetBool(PrefName.ShowFeatureEhr));
			_dictionaryChartPrefsCache.Add(PrefName.OrthoShowInChart.ToString(),PrefC.GetBool(PrefName.OrthoShowInChart));
			//The extraction graphic (TC-5): a change on the Chart preferences page redraws the Chart module.
			_dictionaryChartPrefsCache.Add(ToothChartExtractionGraphic.PREF_NAME,ToothChartExtractionGraphic.IsOn());
		}

		///<summary>True when nothing was taken yet or a setting differs from the last snapshot. Throws when a setting cannot be read or compared (a
		///snapshot cut short by an error); FormOpenDental then redraws.</summary>
		public bool HasChanged() {
			return _dictionaryChartPrefsCache.Count==0
				|| PrefC.GetInt(PrefName.UseInternationalToothNumbers)!=(int)_dictionaryChartPrefsCache["UseInternationalToothNumbers"]
				|| ToothChartRelay.GetEngineForComputer(ComputerPrefs.LocalComputer)!=(ToothChartEngine)_dictionaryChartPrefsCache["ToothChartEngine"]
				|| ToothChartRelay.IsDrawnInSoftware(ComputerPrefs.LocalComputer)!=(bool)_dictionaryChartPrefsCache["WpfDrawInSoftware"]
				|| PrefC.GetBool(PrefName.ShowFeatureEhr)!=(bool)_dictionaryChartPrefsCache["ShowFeatureEhr"]
				|| PrefC.GetBool(PrefName.OrthoShowInChart)!=(bool)_dictionaryChartPrefsCache["OrthoShowInChart"]
				|| ToothChartExtractionGraphic.IsOn()!=(bool)_dictionaryChartPrefsCache[ToothChartExtractionGraphic.PREF_NAME];
		}
	}

	///<summary>What ToothChartPainter draws on: the relay, which passes each call to the chart, or a test's recording target. Only the calls the
	///painter makes.</summary>
	public interface IToothChartPaintTarget {
		void AddDrawingSegment(ToothInitial drawingSegment);
		void AddOrthoElastic(string toothIDstart,string toothIDend,Color color);
		void AddOrthoWireBetweenBrackets(string toothIDstart,string toothIDend,Color color);
		void AddOrthoWireInBracket(string toothID,Color color);
		void AddText(string text,PointF location,Color color,long toothInitialNum);
		void MoveTooth(string toothID,float rotate,float tipM,float tipB,float shiftM,float shiftO,float shiftB);
		void SetBigX(string toothID,Color color);
		void SetBracket(string toothID,Color color);
		void SetBU(string toothID,Color color);
		void SetCrown(string toothID,Color color);
		void SetHidden(string toothID);
		void SetImplant(string toothID,Color color);
		void SetMissing(string toothID);
		void SetPontic(string toothID,Color color);
		void SetPrimary(string toothID);
		void SetRCT(string toothID,Color color);
		void SetRetainedRoot(string toothID,Color color);
		void SetSealant(string toothID,Color color);
		void SetSpaceMaintainer(string toothID,Color color);
		void SetSurfaceColors(string toothID,string surfaces,Color color);
		void SetText(string toothID,Color color,string text);
		void SetVeneer(string toothID,Color color);
	}

	///<summary>One procedure as ToothChartPainter.PaintProcedures draws it, from a progress-notes row (the Chart module and the big chart) or a
	///Procedure (the printout, sheets and the dashboard). The procedure code is looked up as those callers did: by the row's code, or by the
	///procedure's CodeNum.</summary>
	public class ToothChartPaintProc {
		public bool IsHidden;
		public ProcedureCode ProcedureCode;
		public ProcStat ProcStatus;
		public long ProvNum;
		public string ToothNum;
		public string Surf;
		public string ToothRange;

		///<summary>Progress-notes rows with raw database values: ProcStatus, ProcCode, ToothNum, HideGraphics, Surf, ToothRange and ProvNum.</summary>
		public static List<ToothChartPaintProc> FromDataRows(List<DataRow> listDataRows) {
			List<ToothChartPaintProc> listProcs=new List<ToothChartPaintProc>();
			for(int i=0;i<listDataRows.Count;i++) {
				listProcs.Add(new ToothChartPaintProc {
					IsHidden=listDataRows[i]["HideGraphics"].ToString()=="1",
					ProcedureCode=ProcedureCodes.GetProcCode(listDataRows[i]["ProcCode"].ToString()),
					ProcStatus=(ProcStat)PIn.Long(listDataRows[i]["ProcStatus"].ToString()),
					ProvNum=PIn.Long(listDataRows[i]["ProvNum"].ToString()),
					ToothNum=listDataRows[i]["ToothNum"].ToString(),
					Surf=listDataRows[i]["Surf"].ToString(),
					ToothRange=listDataRows[i]["ToothRange"].ToString(),
				});
			}
			return listProcs;
		}

		public static List<ToothChartPaintProc> FromProcedures(List<Procedure> listProcedures) {
			List<ToothChartPaintProc> listProcs=new List<ToothChartPaintProc>();
			for(int i=0;i<listProcedures.Count;i++) {
				listProcs.Add(new ToothChartPaintProc {
					IsHidden=listProcedures[i].HideGraphics,
					ProcedureCode=ProcedureCodes.GetProcCode(listProcedures[i].CodeNum),
					ProcStatus=listProcedures[i].ProcStatus,
					ProvNum=listProcedures[i].ProvNum,
					ToothNum=listProcedures[i].ToothNum,
					Surf=listProcedures[i].Surf,
					ToothRange=listProcedures[i].ToothRange,
				});
			}
			return listProcs;
		}
	}

	///<summary>How the callers' painting differs (O-25), kept as options of the one painter so that each caller paints as it did before TC-2.
	///The For methods give each caller's set.</summary>
	public class ToothChartPaintOptions {
		///<summary>Skips procedures whose code paints nothing or treats the whole mouth. Only the Chart module skipped Mouth procedures.</summary>
		public bool DoSkipMouth;
		///<summary>The Treatment Plan printout's own colours (no TPi exception for a code's graphic colour, no per-provider colours, and status D or
		///a status it does not know is not drawn). Otherwise ToothChartPainter.GetToothColors, which paints such a status white.</summary>
		public bool DoUseTreatPlanColors;
		///<summary>Looks up whose completed work keeps its colour (GetPertinentProvNumsForToothColorPref) before painting. Every caller but the
		///printout did.</summary>
		public bool DoLookUpProviderColors;
		///<summary>Lets the per-provider colours apply (the user's ToothChartUsesDiffColorByProv): not while the Chart module prints, and on sheets
		///only in the Patient Dashboard.</summary>
		public bool IsProviderColorAllowed;
		///<summary>The patient and their appointments, for the per-provider colours.</summary>
		public Patient Patient;
		public List<Appointment> ListAppointments;

		///<summary>The Chart module (ControlChart.DrawProcGraphics): Mouth procedures skipped; the per-provider colours, except when printing.</summary>
		public static ToothChartPaintOptions ForChartModule(Patient patient,List<Appointment> listAppointments,bool isPrinting) {
			return new ToothChartPaintOptions { DoSkipMouth=true,DoLookUpProviderColors=true,IsProviderColorAllowed=!isPrinting,Patient=patient,
				ListAppointments=listAppointments };
		}

		///<summary>The big chart (FormToothChartingBig): the per-provider colours always.</summary>
		public static ToothChartPaintOptions ForBigChart(Patient patient,List<Appointment> listAppointments) {
			return new ToothChartPaintOptions { DoLookUpProviderColors=true,IsProviderColorAllowed=true,Patient=patient,ListAppointments=listAppointments };
		}

		///<summary>Sheets and the Patient Dashboard (SheetPrinting.GetToothChartHelper): the per-provider colours only in the dashboard.</summary>
		public static ToothChartPaintOptions ForSheet(bool isInPatientDashboard,Patient patCur,List<Appointment> listAppts) {
			return new ToothChartPaintOptions { DoLookUpProviderColors=true,IsProviderColorAllowed=isInPatientDashboard,Patient=patCur,
				ListAppointments=listAppts };
		}

		///<summary>The Treatment Plan printout (ControlTreat.PrepImageForPrinting): its own colours.</summary>
		public static ToothChartPaintOptions ForTreatPlanPrintout() {
			return new ToothChartPaintOptions { DoUseTreatPlanColors=true };
		}
	}

	///<summary>The painting code of the tooth chart's callers (TC-2): the procedures, the tooth initials and the orthodontic hardware, drawn on an
	///IToothChartPaintTarget. Moved from ControlChart, ControlTreat, SheetPrinting and FormToothChartingBig, so that the callers hold no chart code
	///and tests can log what each caller paints (tests/ToothChartImageTests, CallLogTests).</summary>
	public static class ToothChartPainter {
		#region Chart module (ControlChart)
		///<summary>ControlChart.FillToothChart: the progress-notes rows the chart draws for the slider date. listProcNumsTreatPlans is null unless
		///"Treatment Plans" is checked; then only completed work, conditions, referrals and the selected plans' procedures are drawn. Otherwise the
		///rows are those of listDataRowsProcsOrig (the procedures the progress notes show). Rows the slider date has not reached go to
		///listDataRowsProcsSkipped (ShouldRowShowGraphical).</summary>
		public static List<DataRow> GetChartModuleRows(DataTable table,List<long> listProcNumsTreatPlans,List<DataRow> listDataRowsProcsOrig,DateTime dateLimit,
			out List<DataRow> listDataRowsProcsSkipped)
		{
			List<DataRow> listDataRowsProcsForGraphical;
			if(listProcNumsTreatPlans!=null) {
				//filter list of DataRows to only include completed work and work for the selected treatment plans
				listDataRowsProcsForGraphical=new List<DataRow>();
				listDataRowsProcsSkipped=new List<DataRow>();
				for(int i=0;i<table.Rows.Count;i++) {
					//If proc status is anything except TP and TPi
					if(new[] { ProcStat.C,ProcStat.Cn,ProcStat.EC,ProcStat.EO,ProcStat.R }.Contains((ProcStat)PIn.Long(table.Rows[i]["ProcStatus"].ToString()))
						|| listProcNumsTreatPlans.Contains(PIn.Long(table.Rows[i]["ProcNum"].ToString())))
					{
						if(!ShouldRowShowGraphical(table.Rows[i],dateLimit,listDataRowsProcsSkipped)) {
							continue;
						}
						listDataRowsProcsForGraphical.Add(table.Rows[i]);
					}
				}
				return listDataRowsProcsForGraphical;
			}
			//put list back to the original list of DataRows
			List<DataRow> listDataRowsSkipped=new List<DataRow>();
			listDataRowsProcsSkipped=listDataRowsSkipped;//the lambda below cannot use the out parameter
			List<string> listProcNumStrsOrig=listDataRowsProcsOrig.Select(x => x["ProcNum"].ToString()).ToList();
			listDataRowsProcsForGraphical=table.Select().Where(x => listProcNumStrsOrig.Contains(x["ProcNum"].ToString())
				&& ShouldRowShowGraphical(x,dateLimit,listDataRowsSkipped)).ToList();
			return listDataRowsProcsForGraphical;
		}

		///<summary>ControlChart.DrawOrthoHardware: the hardware of the exam shown, hidden pieces only when "Show Hidden" is checked.</summary>
		public static List<OrthoHardware> GetChartModuleOrthoHardwares(List<OrthoHardware> listOrthoHardwares,DateTime dateExam,bool isShowHidden) {
			return listOrthoHardwares.FindAll(x=>x.DateExam==dateExam && (!x.IsHidden || isShowHidden));
		}

		///<summary>ControlChart.FillToothChart: a copy of the tooth initials for the slider date. A missing tooth whose extraction the slider has not reached yet (skipped, or completed after the slider date and drawn as planned) is not missing.</summary>
		public static List<ToothInitial> GetChartModuleToothInitials(List<ToothInitial> listToothInitials,List<DataRow> listDataRowsProcsSkipped,
			List<DataRow> listDataRowsProcsForGraphical,DateTime dateLimit)
		{
			List<ToothInitial> listToothInitialsCopy=listToothInitials.Select(x => x.Copy()).ToList();
			for(int i=0;i<listDataRowsProcsSkipped.Count();i++) {
				ProcStat procStat=(ProcStat)PIn.Long(listDataRowsProcsSkipped[i]["ProcStatus"].ToString());
				if(!procStat.In(ProcStat.C,ProcStat.EO,ProcStat.EC)) {
					continue;
				}
				ProcedureCode procedureCode=ProcedureCodes.GetProcCode(listDataRowsProcsSkipped[i]["ProcCode"].ToString());
				if(procedureCode.PaintType!=ToothPaintingType.Extraction) {
					continue;
				}
				listToothInitialsCopy.RemoveAll(x => x.InitialType==ToothInitialType.Missing && x.ToothNum==listDataRowsProcsSkipped[i]["ToothNum"].ToString());
			}
			//Also remove any extractions for TP procs that were set to TP by the ShouldRowShowGraphical
			for(int i=0;i<listDataRowsProcsForGraphical.Count();i++) {
				DateTime dateTP;
				DateTime dateComplete;
				ProcStat procStat=(ProcStat)PIn.Int(listDataRowsProcsForGraphical[i]["ProcStatus"].ToString());
				if(!procStat.In(ProcStat.TP)) {
					continue;
				}
				if(!DateTime.TryParse(listDataRowsProcsForGraphical[i]["DateTP"].ToString(),out dateTP)) {
					continue;
				}
				if(!DateTime.TryParse(listDataRowsProcsForGraphical[i]["DateEntryC"].ToString(),out dateComplete)) {
					continue;
				}
				ProcedureCode procedureCode=ProcedureCodes.GetProcCode(listDataRowsProcsForGraphical[i]["ProcCode"].ToString());
				if(procedureCode.PaintType==ToothPaintingType.Extraction 
						&& dateLimit<dateComplete && dateLimit>=dateTP) 
				{//Procedure is C and the slider date is after or equal to the TP date, but before the completion date
					listToothInitialsCopy.RemoveAll(x => x.InitialType==ToothInitialType.Missing && x.ToothNum==listDataRowsProcsForGraphical[i]["ToothNum"].ToString());//Pretend the row is TP for the tooth chart
				}
			}
			return listToothInitialsCopy;
		}

		///<summary>Returns true if rowCur represents a valid procedure that should be shown on the tooth chart based on the current track bar date. Returns false if rowCur does not have a valid DateTP, DateEntryC, and ProcDate set. Will also return false if rowCur should not be drawn on the tooth chart (based on the track bar's currently selected date / status of rowCur). If the row needs to be skipped, it gets added to listDataRowsProcsSkipped. Moved from ControlChart.</summary>
		public static bool ShouldRowShowGraphical(DataRow dataRow,DateTime dateLimit,List<DataRow> listDataRowsProcsSkipped) {
			DateTime dateTP;
			DateTime dateComplete;
			DateTime dateScheduled;
			ProcStat procStat=(ProcStat)PIn.Int(dataRow["ProcStatus"].ToString());
			if(!DateTime.TryParse(dataRow["DateTP"].ToString(),out dateTP)) {
				return false;
			}
			if(!DateTime.TryParse(dataRow["DateEntryC"].ToString(),out dateComplete)) {
				return false;
			}
			if(!DateTime.TryParse(dataRow["aptDateTime"].ToString(),out dateScheduled)) {
				return false;
			}
			if(dateLimit<dateTP) {//slider date is before the TP date
				listDataRowsProcsSkipped.Add(dataRow);
				return false;//Skip the proc
			}
			if(!procStat.In(ProcStat.C,ProcStat.TP) && dateLimit<dateComplete) {//slider date is before the completion date and the procedure is not C or TP
				listDataRowsProcsSkipped.Add(dataRow);
				return false;//Skip the proc
			}
			if(procStat==ProcStat.C && dateLimit<dateComplete && dateLimit>=dateTP) {//Procedure is C and the slider date is after or equal to the TP date, but before the completion date
				dataRow["ProcStatus"]=POut.Int((int)ProcStat.TP);//Pretend the row is TP for the tooth chart
			}
			return true;
		}
		#endregion Chart module (ControlChart)

		#region The painter
		///<summary>The primary teeth of the tooth initials. They come first, so a primary tooth can be set missing afterwards; the Chart module also
		///restores its selection after them.</summary>
		public static void PaintPrimaryTeeth(IToothChartPaintTarget target,List<ToothInitial> listToothInitials) {
			//first, primary.  That way, you can still set a primary tooth missing afterwards.
			for(int i=0;i<listToothInitials.Count;i++) {
				if(listToothInitials[i].InitialType==ToothInitialType.Primary) {
					target.SetPrimary(listToothInitials[i].ToothNum);
				}
			}
		}

		///<summary>The other tooth initials: missing and hidden teeth, movements, drawings and free text.</summary>
		public static void PaintToothInitials(IToothChartPaintTarget target,List<ToothInitial> listToothInitials) {
			for(int i=0;i<listToothInitials.Count;i++) {
				switch(listToothInitials[i].InitialType) {
					case ToothInitialType.Missing:
						target.SetMissing(listToothInitials[i].ToothNum);
						break;
					case ToothInitialType.Hidden:
						target.SetHidden(listToothInitials[i].ToothNum);
						break;
					case ToothInitialType.Rotate:
						target.MoveTooth(listToothInitials[i].ToothNum,listToothInitials[i].Movement,0,0,0,0,0);
						break;
					case ToothInitialType.TipM:
						target.MoveTooth(listToothInitials[i].ToothNum,0,listToothInitials[i].Movement,0,0,0,0);
						break;
					case ToothInitialType.TipB:
						target.MoveTooth(listToothInitials[i].ToothNum,0,0,listToothInitials[i].Movement,0,0,0);
						break;
					case ToothInitialType.ShiftM:
						target.MoveTooth(listToothInitials[i].ToothNum,0,0,0,listToothInitials[i].Movement,0,0);
						break;
					case ToothInitialType.ShiftO:
						target.MoveTooth(listToothInitials[i].ToothNum,0,0,0,0,listToothInitials[i].Movement,0);
						break;
					case ToothInitialType.ShiftB:
						target.MoveTooth(listToothInitials[i].ToothNum,0,0,0,0,0,listToothInitials[i].Movement);
						break;
					case ToothInitialType.Drawing:
						target.AddDrawingSegment(listToothInitials[i].Copy());
						break;
					case ToothInitialType.Text:
						target.AddText(listToothInitials[i].GetTextString(), listToothInitials[i].GetTextPoint(), listToothInitials[i].ColorDraw, listToothInitials[i].ToothInitialNum);
						break;
				}
			}
		}

		///<summary>The procedures of every caller (O-25: one painter, each caller's own output kept as options): hidden procedures are skipped, and so
		///are completed or existing extractions (the missing teeth show them) unless the office shows completed extractions with an X (O-20,
		///ToothChartExtractionGraphic, TC-5, on by default as in Open Dental 26.2): then they draw the big X in their status's colour over the missing
		///tooth, in the list's order like any other procedure, whatever else is charted on that tooth (a completed implant or pontic included: the
		///setting is a plain switch, as Open Dental's). Each procedure is painted by its code's paint type in the colour of its status. TextBelow
		///(18) paints nothing on the tooth: the Chart module shows it in its one-line strip under the date slider (GetTextBelowStripText).
		///options says how each caller differs.</summary>
		public static void PaintProcedures(IToothChartPaintTarget target,List<ToothChartPaintProc> listProcs,List<ToothInitial> listToothInitials,
			ToothChartPaintOptions options)
		{
			string[] stringArrayTeeth;
			bool isCompletedExtractionShown=ToothChartExtractionGraphic.IsOn();
			List<long> listProvNums=new List<long>();
			if(options.DoLookUpProviderColors) {
				listProvNums=GetPertinentProvNumsForToothColorPref(Security.CurUser,options.Patient,options.ListAppointments);
			}
			List<Def> listDefsTreatPlan=null;
			if(options.DoUseTreatPlanColors) {
				listDefsTreatPlan=Defs.GetDefsForCategory(DefCat.ChartGraphicColors,true);
			}
			for(int i=0;i<listProcs.Count;i++) {
				ToothChartPaintProc proc=listProcs[i];
				if(proc.IsHidden) {
					continue;
				}
				ProcedureCode procedureCode=proc.ProcedureCode;
				if(options.DoSkipMouth && (procedureCode.PaintType==ToothPaintingType.None || procedureCode.TreatArea==TreatmentArea.Mouth)) {
					continue;
				}
				if(procedureCode.PaintType==ToothPaintingType.Extraction && IsCompletedStatus(proc.ProcStatus) && !isCompletedExtractionShown) {
					continue;//prevents the red X. Missing teeth already handled. With the extraction graphic on, the X is drawn over the missing tooth.
				}
				Color cDark;
				Color cLight;
				if(options.DoUseTreatPlanColors) {
					if(!GetTreatPlanColors(procedureCode,proc.ProcStatus,listDefsTreatPlan,out cDark,out cLight)) {
						continue;//Don't draw.
					}
				}
				else {
					bool isProviderColor=options.IsProviderColorAllowed && DoesToothColorPrefApply(listProvNums,proc.ProvNum);
					//A status GetToothColors does not know (D, from an invalidated locked procedure) is painted white, as before.
					GetToothColors(procedureCode,proc.ProcStatus,isProviderColor,out cDark,out cLight);
				}
				switch(procedureCode.PaintType) {
					case ToothPaintingType.BridgeDark:
						if(ToothInitials.ToothIsMissingOrHidden(listToothInitials,proc.ToothNum)){
							target.SetPontic(proc.ToothNum,cDark);
						}
						else{
							target.SetCrown(proc.ToothNum,cDark);
						}
						break;
					case ToothPaintingType.BridgeLight:
						if(ToothInitials.ToothIsMissingOrHidden(listToothInitials,proc.ToothNum)) {
							target.SetPontic(proc.ToothNum,cLight);
						}
						else {
							target.SetCrown(proc.ToothNum,cLight);
						}
						break;
					case ToothPaintingType.CrownDark:
						target.SetCrown(proc.ToothNum,cDark);
						break;
					case ToothPaintingType.CrownLight:
						target.SetCrown(proc.ToothNum,cLight);
						break;
					case ToothPaintingType.DentureDark:
						if(proc.Surf=="U") {
							stringArrayTeeth=new string[14];
							for(int t=0;t<14;t++) {
								stringArrayTeeth[t]=(t+2).ToString();
							}
						}
						else if(proc.Surf=="L") {
							stringArrayTeeth=new string[14];
							for(int t=0;t<14;t++) {
								stringArrayTeeth[t]=(t+18).ToString();
							}
						}
						else {
							stringArrayTeeth=proc.ToothRange.Split(new char[] {','});
						}
						for(int t=0;t<stringArrayTeeth.Length;t++) {
							if(ToothInitials.ToothIsMissingOrHidden(listToothInitials,stringArrayTeeth[t])) {
								target.SetPontic(stringArrayTeeth[t],cDark);
								continue;
							}
							target.SetCrown(stringArrayTeeth[t],cDark);
						}
						break;
					case ToothPaintingType.DentureLight:
						if(proc.Surf=="U") {
							stringArrayTeeth=new string[14];
							for(int t=0;t<14;t++) {
								stringArrayTeeth[t]=(t+2).ToString();
							}
						}
						else if(proc.Surf=="L") {
							stringArrayTeeth=new string[14];
							for(int t=0;t<14;t++) {
								stringArrayTeeth[t]=(t+18).ToString();
							}
						}
						else {
							stringArrayTeeth=proc.ToothRange.Split(new char[] { ',' });
						}
						for(int t=0;t<stringArrayTeeth.Length;t++) {
							if(ToothInitials.ToothIsMissingOrHidden(listToothInitials,stringArrayTeeth[t])) {
								target.SetPontic(stringArrayTeeth[t],cLight);
								continue;
							}
							target.SetCrown(stringArrayTeeth[t],cLight);
						}
						break;
					case ToothPaintingType.Extraction:
						target.SetBigX(proc.ToothNum,cDark);
						break;
					case ToothPaintingType.FillingDark:
						target.SetSurfaceColors(proc.ToothNum,proc.Surf,cDark);
						break;
					case ToothPaintingType.FillingLight:
						target.SetSurfaceColors(proc.ToothNum,proc.Surf,cLight);
						break;
					case ToothPaintingType.Implant:
						target.SetImplant(proc.ToothNum,cDark);
						break;
					case ToothPaintingType.PostBU:
						target.SetBU(proc.ToothNum,cDark);
						break;
					case ToothPaintingType.RCT:
						target.SetRCT(proc.ToothNum,cDark);
						break;
					case ToothPaintingType.RetainedRoot:
						target.SetRetainedRoot(proc.ToothNum,cDark);
						break;
					case ToothPaintingType.Sealant:
						target.SetSealant(proc.ToothNum,cDark);
						break;
					case ToothPaintingType.SpaceMaintainer:
						if(procedureCode.TreatArea==TreatmentArea.Tooth && proc.ToothNum!=""){
							target.SetSpaceMaintainer(proc.ToothNum,cDark);
							break;
						}
						if(procedureCode.TreatArea==TreatmentArea.ToothRange && proc.ToothRange!="") {
							stringArrayTeeth=proc.ToothRange.Split(',');
							for(int t=0;t<stringArrayTeeth.Length;t++) {
								target.SetSpaceMaintainer(stringArrayTeeth[t],cDark);
							}
							break;
						}
						if(procedureCode.AreaAlsoToothRange==true && proc.ToothRange!="") {
							stringArrayTeeth=proc.ToothRange.Split(',');
							for(int t=0;t<stringArrayTeeth.Length;t++) {
								target.SetSpaceMaintainer(stringArrayTeeth[t],cDark);
							}
							break;
						}
						if(procedureCode.TreatArea==TreatmentArea.Quad){
							stringArrayTeeth=new string[0];
							if(proc.Surf=="UR") {
								stringArrayTeeth=new string[] {"4","5","6","7","8"};
							}
							if(proc.Surf=="UL") {
								stringArrayTeeth=new string[] {"9","10","11","12","13"};
							}
							if(proc.Surf=="LL") {
								stringArrayTeeth=new string[] {"20","21","22","23","24"};
							}
							if(proc.Surf=="LR") {
								stringArrayTeeth=new string[] {"25","26","27","28","29"};
							}							
							for(int t=0;t<stringArrayTeeth.Length;t++) {//could still be length 0
								target.SetSpaceMaintainer(stringArrayTeeth[t],cDark);
							}
						}
						break;
					case ToothPaintingType.Text:
						target.SetText(proc.ToothNum,cDark,procedureCode.PaintText);//"W" is the watch mark; other text is drawn on the tooth (TC-5)
						break;
					case ToothPaintingType.Veneer:
						target.SetVeneer(proc.ToothNum,cLight);
						break;
				}
			}
		}

		///<summary>True for the statuses of work that is done: completed (C), existing current provider (EC) and existing other provider (EO).</summary>
		private static bool IsCompletedStatus(ProcStat procStat) {
			return procStat==ProcStat.C || procStat==ProcStat.EC || procStat==ProcStat.EO;
		}

		///<summary>The separator between the texts of several TextBelow procedures in the Chart module's strip (TC-5). Open Dental's public
		///screenshot shows one item only, so how it joins several is not known; this is the fork's choice.</summary>
		public const string TextBelowStripSeparator="; ";

		///<summary>The text of the Chart module's TextBelow strip (paint type TextBelow, 18, Open Dental 26.2: "the procedure's custom text displays
		///below the odontogram", in a one-line box under the date slider, not in the tooth picture). listDataRows is the procedure list the Chart
		///module paints (GetChartModuleRows: after the date slider, the chart view's status filters and the selected treatment plans), so the
		///strip follows them. It is read before the painter's skips, so a TextBelow procedure of any treatment area counts: tooth, arch, quadrant
		///and mouth alike (the painter leaves out Mouth procedures in the Chart module). A procedure with Hide Graphics is left out, as the painter
		///leaves it out. The codes' paint texts, in ProcDate order (rows of the same date in the list's order), joined by TextBelowStripSeparator;
		///an empty paint text adds nothing. "" when there is none, and the Chart module then hides the strip.</summary>
		public static string GetTextBelowStripText(List<DataRow> listDataRows) {
			List<Tuple<DateTime,int,string>> listTexts=new List<Tuple<DateTime,int,string>>();
			if(listDataRows==null) {
				return "";
			}
			for(int i=0;i<listDataRows.Count;i++) {
				DataRow dataRow=listDataRows[i];
				if(dataRow["HideGraphics"].ToString()=="1") {
					continue;
				}
				ProcedureCode procedureCode=ProcedureCodes.GetProcCode(dataRow["ProcCode"].ToString());
				if(procedureCode==null || procedureCode.PaintType!=ToothPaintingType.TextBelow || string.IsNullOrEmpty(procedureCode.PaintText)) {
					continue;
				}
				DateTime dateProc;
				object objectDate=dataRow["ProcDate"];
				if(objectDate is DateTime) {
					dateProc=(DateTime)objectDate;
				}
				else {
					dateProc=PIn.Date(objectDate.ToString());
				}
				listTexts.Add(Tuple.Create(dateProc.Date,i,procedureCode.PaintText));
			}
			return string.Join(TextBelowStripSeparator,listTexts.OrderBy(x => x.Item1).ThenBy(x => x.Item2).Select(x => x.Item3));
		}

		///<summary>The Treatment Plan printout's own colours (ControlTreat before TC-2): the code's graphic colour whatever the status (no
		///exception for TPi), otherwise the chart colour of the status, and no per-provider colours. False for status D and for a status it does
		///not know, which the printout does not draw.</summary>
		private static bool GetTreatPlanColors(ProcedureCode procedureCode,ProcStat procStat,List<Def> listDefs,out Color cDark,out Color cLight) {
			cDark=Color.White;
			cLight=Color.White;
			if(procedureCode.GraphicColor!=Color.FromArgb(0)) {
				cDark=procedureCode.GraphicColor;
				cLight=procedureCode.GraphicColor;
				return true;
			}
			switch(procStat) {
				case ProcStat.C:
					cDark=listDefs[1].ItemColor;
					cLight=listDefs[6].ItemColor;
					return true;
				case ProcStat.TP:
					cDark=listDefs[0].ItemColor;
					cLight=listDefs[5].ItemColor;
					return true;
				case ProcStat.EC:
					cDark=listDefs[2].ItemColor;
					cLight=listDefs[7].ItemColor;
					return true;
				case ProcStat.EO:
					cDark=listDefs[3].ItemColor;
					cLight=listDefs[8].ItemColor;
					return true;
				case ProcStat.R:
					cDark=listDefs[4].ItemColor;
					cLight=listDefs[9].ItemColor;
					return true;
				case ProcStat.Cn:
					cDark=listDefs[16].ItemColor;
					cLight=listDefs[17].ItemColor;
					return true;
				case ProcStat.D:  //Don't think this can ever happen, but skip anyway.
				default:
					return false;//Don't draw.
			}
		}

		///<summary>Orthodontic hardware: brackets, then the wires and elastics between and within them, in the colours of their specs.</summary>
		public static void PaintOrthoHardware(IToothChartPaintTarget target,List<OrthoHardware> listOrthoHardwares) {
			List<OrthoHardwareSpec> listOrthoHardwareSpecs=OrthoHardwareSpecs.GetDeepCopy();
			List<OrthoHardwares.OrthoWire> listOrthoWires=new List<OrthoHardwares.OrthoWire>();//also used for elastics
			for(int i=0;i<listOrthoHardwares.Count;i++){
				OrthoHardwareSpec orthoHardwareSpec=listOrthoHardwareSpecs.Find(x=>x.OrthoHardwareSpecNum==listOrthoHardwares[i].OrthoHardwareSpecNum);
				if(listOrthoHardwares[i].OrthoHardwareType==EnumOrthoHardwareType.Bracket){
					target.SetBracket(listOrthoHardwares[i].ToothRange,orthoHardwareSpec.ItemColor);
				}
				if(listOrthoHardwares[i].OrthoHardwareType==EnumOrthoHardwareType.Wire){
					listOrthoWires.AddRange(OrthoHardwares.GetWires(listOrthoHardwares[i].ToothRange,orthoHardwareSpec.ItemColor));
				}
				if(listOrthoHardwares[i].OrthoHardwareType==EnumOrthoHardwareType.Elastic){
					listOrthoWires.AddRange(OrthoHardwares.GetElastics(listOrthoHardwares[i].ToothRange,orthoHardwareSpec.ItemColor));
				}
			}
			for(int i=0;i<listOrthoWires.Count;i++){
				if(listOrthoWires[i].OrthoWireType==OrthoHardwares.EnumOrthoWireType.BetweenBrackets){
					target.AddOrthoWireBetweenBrackets(listOrthoWires[i].ToothIDstart,listOrthoWires[i].ToothIDend,listOrthoWires[i].ColorDraw);
				}
				if(listOrthoWires[i].OrthoWireType==OrthoHardwares.EnumOrthoWireType.InBracket){
					target.AddOrthoWireInBracket(listOrthoWires[i].ToothIDstart,listOrthoWires[i].ColorDraw);
				}
				if(listOrthoWires[i].OrthoWireType==OrthoHardwares.EnumOrthoWireType.Elastic){
					target.AddOrthoElastic(listOrthoWires[i].ToothIDstart,listOrthoWires[i].ToothIDend,listOrthoWires[i].ColorDraw);
				}
			}
		}
		#endregion The painter

		#region Colours (moved from ToothChartRelay, whose methods forward here)
		///<summary>Checks ProvNums only if the logged in user has 'ToothChartUsesDiffColorByProv' user pref on</summary>
		public static bool DoesToothColorPrefApply(List<long> listCurProvNums,long procProvNum) {
			//This first check is just being extra careful.
			return HasToothColorUserPref() && !listCurProvNums.Contains(procProvNum);
		}

		///<summary>
		///Returns ProvNums related to the passed in entities in a specific order. Returns the first of the following conditions: 
		///	1. Returns an empty list if the user currently logged in does not have ToothChartUsesDiffColorByProv user pref. 
		///	2. Returns the ProvNum of the user currently logged in if one is set. 
		///	3. Returns all primary providers for any appointment for the patient passed in that is scheduled for today. 
		///	4. Returns the primary provider if none of the previous conditions were met. Can return an empty list.
		///	</summary>
		public static List<long> GetPertinentProvNumsForToothColorPref(Userod user,Patient patCur,List<Appointment> listAppts) {
			List<long> listCurProvNums=new List<long>();
			if(!HasToothColorUserPref()) {//We will leave this method if the current user doesn't have the pref
				return new List<long>();
			}
			//First, checks if current user is a provider
			if(user?.ProvNum!=0) {
				return new List<long>() { Security.CurUser.ProvNum };
			}
			if(patCur==null) {
				return new List<long>();
			}
			//Second, grabs the primary provider on today's appointment for the current patient. This list has already been filtered but just in case
			List<long> listApptProvNums=listAppts?.Where(x=>x.AptDateTime.Date==DateTime.Today && x.PatNum==patCur.PatNum)
				.Select(x=>x.ProvNum).ToList();
			if(!listApptProvNums.IsNullOrEmpty()) {
				return listApptProvNums;
			}
			//Finally, return the patient's primary provider. 
			return new List<long>() { patCur.PriProv };
		}

		///<summary>Sets colors for the teeth in the graphical tooth chart.  Pass doUseEcEoForCProcs=true if C procs should be colored like EO/EC.</summary>
		public static bool GetToothColors(ProcedureCode curProc,ProcStat procStatus,bool doUseEcEoForCProcs,out Color cDark,out Color cLight) {
			cDark=Color.White;
			cLight=Color.White;
			List<Def> listDefsToothColors=Defs.GetDefsForCategory(DefCat.ChartGraphicColors,true);
			if(curProc.GraphicColor!=Color.FromArgb(0) && procStatus!=ProcStat.TPi) {
				cDark=curProc.GraphicColor;
				cLight=curProc.GraphicColor;
				return true;
			}
			switch(procStatus) {
				case ProcStat.C:					
					if(doUseEcEoForCProcs) {//Whoever is currently viewing the ToothChart wants other Provider's C procs to show as EC
						cDark=listDefsToothColors[3].ItemColor;//same values as ProcStat.EO
						cLight=listDefsToothColors[8].ItemColor;
					}
					else {
						cDark=listDefsToothColors[1].ItemColor;
						cLight=listDefsToothColors[6].ItemColor;
					}
					return true;
				case ProcStat.TP:
					cDark=listDefsToothColors[0].ItemColor;
					cLight=listDefsToothColors[5].ItemColor;
					return true;
				case ProcStat.EC:
					cDark=listDefsToothColors[2].ItemColor;
					cLight=listDefsToothColors[7].ItemColor;
					return true;
				case ProcStat.EO:
					cDark=listDefsToothColors[3].ItemColor;
					cLight=listDefsToothColors[8].ItemColor;
					return true;
				case ProcStat.R:
					cDark=listDefsToothColors[4].ItemColor;
					cLight=listDefsToothColors[9].ItemColor;
					return true;
				case ProcStat.Cn:
					cDark=listDefsToothColors[16].ItemColor;
					cLight=listDefsToothColors[17].ItemColor;
					return true;
				case ProcStat.D://Can happen with invalidated locked procs.
				default:
					cDark=Color.White;
					cLight=Color.White;
					return false;//Don't draw.
			}
		}

		///<summary>Returns true if the currently logged in user has ColorChartUsesDiffByProv preference</summary>
		public static bool HasToothColorUserPref(){
			return ToothChartRelay.GetListUserOdPrefs().Select(x => x.UserNum).Contains(Security.CurUser.UserNum);
		}
		#endregion Colours (moved from ToothChartRelay, whose methods forward here)
	}
}
