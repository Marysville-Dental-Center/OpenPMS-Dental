using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CodeBase;
using OpenPmsDental.UI;
using OpenPmsDentalBusiness;

namespace OpenPmsDental {
	///<summary>The tooth picker of the Imaging module (imaging design P2, owner 2026-11-22): changes the teeth of one image. Click a tooth to toggle it, or
	///drag along a row; the quadrant and anterior buttons toggle a group; the text box takes typed teeth ("3-5") in the office's numbering. Permanent
	///and primary rows show together, so mixed dentition works. For an image in a mount, the slot's own teeth are underlined, and a check box also
	///changes the slot's teeth in this mount. The window only returns the result (ToothNumbers, IsSlotChanged); the caller saves and logs.
	///Its controls are made in code (BuildControls), so the UI rollout's designer data needs no rows for it.</summary>
	public class FormToothPicker:FormODBase {
		#region Fields - Public
		///<summary>The teeth chosen, in the database's format: American numbers separated by commas, permanent before primary. Set on OK.</summary>
		public string ToothNumbers="";
		///<summary>True on OK when the check box asked to change the slot's teeth in this mount too.</summary>
		public bool IsSlotChanged;
		///<summary>Where to open: the window goes beside this screen point, kept on its screen. Placed after FormODBase's Load has sized the
		///window for the monitor's dpi. Null keeps the location set by the caller.</summary>
		public Point? PointAnchor;
		#endregion Fields - Public

		#region Fields - Private
		private const int CELL_GAP=2;
		private const int CELL_HEIGHT=28;
		private const int CELL_WIDTH=30;
		///<summary>Space between the upper and the lower arch.</summary>
		private const int ARCH_GAP=8;
		private UI.Button butCancel;
		private UI.Button butOK;
		private ControlDoubleBuffered chart;
		private UI.CheckBox checkSlot;
		private HashSet<string> _hashSetSelected=new HashSet<string>();
		///<summary>The selection when the mouse went down, for a drag along a row.</summary>
		private HashSet<string> _hashSetAtMouseDown;
		private HashSet<string> _hashSetSlot=new HashSet<string>();
		///<summary>Set while the text box is being changed in code, so its TextChanged does not count as typing.</summary>
		private bool _isFillingText;
		private bool _isTextTyped;
		private Label labelHint;
		private Label labelTeeth;
		private List<ToothCell> _listCells=new List<ToothCell>();
		private ToothCell _toothCellHover;
		private ToothCell _toothCellMouseDown;
		private bool _isDragSelecting;
		private ToothNumberingNomenclature _toothNumberingNomenclature;
		private System.Windows.Forms.TextBox textTeeth;
		#endregion Fields - Private

		#region Constructor
		///<summary>toothNumbers and toothNumbersSlot are in the database's format. toothNumbersSlot is null for an image that is not in a mount.</summary>
		public FormToothPicker(string toothNumbers,string toothNumbersSlot,ToothNumberingNomenclature toothNumberingNomenclature) {
			_toothNumberingNomenclature=toothNumberingNomenclature;
			_hashSetSelected=ImageTeeth.SplitTeeth(toothNumbers);
			BuildControls(isMountItem:toothNumbersSlot!=null);
			if(toothNumbersSlot!=null){
				_hashSetSlot=ImageTeeth.SplitTeeth(toothNumbersSlot);
			}
			BuildCells();
			InitializeLayoutManager();
			Lan.F(this);
			FillText();
		}
		#endregion Constructor

		#region Methods - Event Handlers
		protected override void OnLoad(EventArgs e) {
			base.OnLoad(e);//sizes the window for the monitor's dpi
			if(PointAnchor==null){
				return;
			}
			Point pointAnchor=PointAnchor.Value;
			Rectangle rectangleArea=System.Windows.Forms.Screen.FromPoint(pointAnchor).WorkingArea;
			int x=Math.Max(rectangleArea.Left,Math.Min(pointAnchor.X+16,rectangleArea.Right-Width));
			int y=Math.Max(rectangleArea.Top,Math.Min(pointAnchor.Y-Height/2,rectangleArea.Bottom-Height));
			Location=new Point(x,y);
		}

		private void butCancel_Click(object sender,EventArgs e) {
			DialogResult=DialogResult.Cancel;
		}

		private void butOK_Click(object sender,EventArgs e) {
			if(_isTextTyped && !ParseText()){
				return;
			}
			ToothNumbers=ImageTeeth.JoinTeeth(_hashSetSelected);
			IsSlotChanged=checkSlot!=null && checkSlot.Checked;
			DialogResult=DialogResult.OK;
		}

		private void butQuick_Click(object sender,EventArgs e) {
			if(_isTextTyped && !ParseText()){
				return;
			}
			string[] arrayTeeth=(string[])((Control)sender).Tag;
			if(arrayTeeth.Length==0){//Clear
				_hashSetSelected.Clear();
			}
			else if(arrayTeeth.All(x => _hashSetSelected.Contains(x))){
				_hashSetSelected.ExceptWith(arrayTeeth);
			}
			else{
				_hashSetSelected.UnionWith(arrayTeeth);
			}
			FillText();
			chart.Invalidate();
		}

		private void chart_MouseDown(object sender,MouseEventArgs e) {
			if(e.Button!=MouseButtons.Left){
				return;
			}
			ToothCell toothCell=HitTest(e.Location);
			if(toothCell==null){
				return;
			}
			if(_isTextTyped && !ParseText()){
				return;
			}
			_toothCellMouseDown=toothCell;
			_hashSetAtMouseDown=new HashSet<string>(_hashSetSelected);
			_isDragSelecting=true;
			ApplyDrag(toothCell);
		}

		private void chart_MouseLeave(object sender,EventArgs e) {
			_toothCellHover=null;
			chart.Invalidate();
		}

		private void chart_MouseMove(object sender,MouseEventArgs e) {
			ToothCell toothCell=HitTest(e.Location);
			if(toothCell!=_toothCellHover){
				_toothCellHover=toothCell;
				chart.Invalidate();
			}
			if(_isDragSelecting && toothCell!=null && toothCell.Row==_toothCellMouseDown.Row){
				ApplyDrag(toothCell);
			}
		}

		private void chart_MouseUp(object sender,MouseEventArgs e) {
			_isDragSelecting=false;
		}

		private void chart_Paint(object sender,PaintEventArgs e) {
			Graphics g=e.Graphics;
			g.Clear(UiTheme.Surface);
			float scale=LayoutManager.ScaleMy();
			//A font takes only the program's zoom: Windows adds the dpi part (LayoutManagerForms' rule, as FormMountDefEdit's labels).
			using Font font=new Font(UiTheme.FontBody.FontFamily,LayoutManager.ScaleFontODZoom(UiTheme.FontBody.SizeInPoints*0.9f));
			using SolidBrush solidBrushCell=new SolidBrush(UiTheme.SurfaceRaised);
			using SolidBrush solidBrushHover=new SolidBrush(UiTheme.SubtleHover);
			using SolidBrush solidBrushSelected=new SolidBrush(UiTheme.Accent);
			using Pen penCell=new Pen(UiTheme.StrokeStrong);
			using Pen penSlot=new Pen(UiTheme.Warning,Math.Max(2f,3f*scale));
			for(int i=0;i<_listCells.Count;i++){
				ToothCell toothCell=_listCells[i];
				Rectangle rectangle=ScaleRectangle(toothCell.Rectangle,scale);
				bool isSelected=_hashSetSelected.Contains(toothCell.ToothNum);
				Brush brush=isSelected ? solidBrushSelected : (toothCell==_toothCellHover ? solidBrushHover : solidBrushCell);
				g.FillRectangle(brush,rectangle);
				g.DrawRectangle(penCell,rectangle.X,rectangle.Y,rectangle.Width-1,rectangle.Height-1);
				if(_hashSetSlot.Contains(toothCell.ToothNum)){
					//the slot's own teeth: a line along the bottom of the cell
					float y=rectangle.Bottom-penSlot.Width/2-1;
					g.DrawLine(penSlot,rectangle.Left+2,y,rectangle.Right-3,y);
				}
				Color colorText=isSelected ? UiTheme.TextOnAccent : UiTheme.Text;
				TextRenderer.DrawText(g,Tooth.Display(toothCell.ToothNum,_toothNumberingNomenclature),font,rectangle,colorText,
					TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter|TextFormatFlags.NoPadding);
			}
		}

		private void textTeeth_Leave(object sender,EventArgs e) {
			if(_isTextTyped){
				ParseText();
			}
		}

		private void textTeeth_TextChanged(object sender,EventArgs e) {
			if(!_isFillingText){
				_isTextTyped=true;
			}
		}
		#endregion Methods - Event Handlers

		#region Methods - Private
		///<summary>Sets every cell of the drag's row between the mouse-down cell and toothCell to the opposite of what the mouse-down cell was.</summary>
		private void ApplyDrag(ToothCell toothCell){
			bool isSelecting=!_hashSetAtMouseDown.Contains(_toothCellMouseDown.ToothNum);
			_hashSetSelected=new HashSet<string>(_hashSetAtMouseDown);
			int colFrom=Math.Min(_toothCellMouseDown.Col,toothCell.Col);
			int colTo=Math.Max(_toothCellMouseDown.Col,toothCell.Col);
			foreach(ToothCell toothCellRow in _listCells.Where(x => x.Row==toothCell.Row && x.Col>=colFrom && x.Col<=colTo)){
				if(isSelecting){
					_hashSetSelected.Add(toothCellRow.ToothNum);
				}
				else{
					_hashSetSelected.Remove(toothCellRow.ToothNum);
				}
			}
			FillText();
			chart.Invalidate();
		}

		///<summary>Makes the window's controls on the UI-12 grid: 8 px padding, 23 px fields at a 27 px pitch, labels 6 px before their field, the
		///dialog buttons bottom right 8 px apart.</summary>
		private void BuildControls(bool isMountItem){
			SuspendLayout();
			int widthChart=16*CELL_WIDTH+15*CELL_GAP;//510
			int heightChart=4*CELL_HEIGHT+3*CELL_GAP+ARCH_GAP;//126
			int xRight=8+widthChart;//518, the content's right edge
			labelHint=new Label();
			labelHint.Location=new Point(8,8);
			labelHint.Size=new Size(widthChart,20);
			labelHint.Name="labelHint";
			labelHint.Text="Click a tooth or drag along a row. The middle rows are the primary teeth.";
			labelHint.TextAlign=ContentAlignment.MiddleLeft;
			labelHint.ForeColor=UiTheme.TextSecondary;
			Controls.Add(labelHint);
			chart=new ControlDoubleBuffered();
			chart.Location=new Point(8,32);
			chart.Size=new Size(widthChart,heightChart);
			chart.Name="chart";
			chart.TabStop=false;
			chart.Paint+=chart_Paint;
			chart.MouseDown+=chart_MouseDown;
			chart.MouseMove+=chart_MouseMove;
			chart.MouseUp+=chart_MouseUp;
			chart.MouseLeave+=chart_MouseLeave;
			Controls.Add(chart);
			//Quick buttons: the permanent teeth of each quadrant and each arch's six anterior teeth, then Clear.
			string[][] arrayGroups=new string[][] {
				Range(1,8),Range(6,11),Range(9,16),Range(17,24),Range(22,27),Range(25,32),new string[0] };
			string[] arrayTexts=new string[] { "UR","UA","UL","LL","LA","LR","Clear" };
			int yQuick=32+heightChart+8;//166
			int widthQuick=(widthChart-6*6)/7;//67
			for(int i=0;i<arrayGroups.Length;i++){
				UI.Button button=new UI.Button();
				button.Location=new Point(8+i*(widthQuick+6),yQuick);
				button.Size=new Size(i==arrayGroups.Length-1 ? xRight-(8+i*(widthQuick+6)) : widthQuick,23);
				button.Name="butQuick"+arrayTexts[i];
				button.Text=arrayTexts[i];
				button.Tag=arrayGroups[i];
				button.TabIndex=1+i;
				button.Click+=butQuick_Click;
				Controls.Add(button);
			}
			int yTeeth=yQuick+27;//193
			labelTeeth=new Label();
			labelTeeth.Location=new Point(8,yTeeth);
			labelTeeth.Size=new Size(44,23);
			labelTeeth.Name="labelTeeth";
			labelTeeth.Text="Teeth";
			labelTeeth.TextAlign=ContentAlignment.MiddleRight;
			Controls.Add(labelTeeth);
			textTeeth=new System.Windows.Forms.TextBox();
			textTeeth.Location=new Point(58,yTeeth);
			textTeeth.Size=new Size(xRight-58,23);
			textTeeth.Name="textTeeth";
			textTeeth.TabIndex=10;
			textTeeth.TextChanged+=textTeeth_TextChanged;
			textTeeth.Leave+=textTeeth_Leave;
			Controls.Add(textTeeth);
			int yButtons=yTeeth+27+8;//228
			if(isMountItem){
				checkSlot=new UI.CheckBox();
				checkSlot.Location=new Point(58,yTeeth+27);//on the field column
				checkSlot.Size=new Size(xRight-58,23);
				checkSlot.Name="checkSlot";
				checkSlot.Text="Also change this slot's teeth in this mount (the underlined teeth)";
				checkSlot.CheckAlign=ContentAlignment.MiddleLeft;
				checkSlot.TabIndex=11;
				Controls.Add(checkSlot);
				yButtons+=27;
			}
			butCancel=new UI.Button();
			butCancel.Location=new Point(xRight-75,yButtons);
			butCancel.Size=new Size(75,26);
			butCancel.Name="butCancel";
			butCancel.Text="Cancel";
			butCancel.TabIndex=21;
			butCancel.Click+=butCancel_Click;
			Controls.Add(butCancel);
			butOK=new UI.Button();
			butOK.Location=new Point(xRight-75-8-75,yButtons);
			butOK.Size=new Size(75,26);
			butOK.Name="butOK";
			butOK.Text="OK";
			butOK.TabIndex=20;
			butOK.Click+=butOK_Click;
			Controls.Add(butOK);
			ClientSize=new Size(xRight+8,yButtons+26+8);
			Name="FormToothPicker";
			ShowIcon=false;
			ShowInTaskbar=false;
			MinimizeBox=false;
			MaximizeBox=false;
			StartPosition=FormStartPosition.Manual;
			Text="Teeth";
			ResumeLayout(false);
		}

		///<summary>Rows from the top: permanent upper (1-16), primary upper (A-J), primary lower (T-K), permanent lower (32-17), each from the
		///patient's right to left as a chart shows them. The primary rows sit under the permanent rows' teeth 4 to 13.</summary>
		private void BuildCells(){
			_listCells.Clear();
			for(int col=0;col<16;col++){
				AddCell(0,col,(col+1).ToString());
				AddCell(3,col,(32-col).ToString());
			}
			for(int i=0;i<10;i++){
				AddCell(1,3+i,((char)('A'+i)).ToString());
				AddCell(2,3+i,((char)('T'-i)).ToString());
			}
		}

		private void AddCell(int row,int col,string toothNum){
			int y=row*(CELL_HEIGHT+CELL_GAP)+(row>=2 ? ARCH_GAP : 0);
			_listCells.Add(new ToothCell { Row=row,Col=col,ToothNum=toothNum,Rectangle=new Rectangle(col*(CELL_WIDTH+CELL_GAP),y,CELL_WIDTH,CELL_HEIGHT) });
		}

		private void FillText(){
			_isFillingText=true;
			textTeeth.Text=Tooth.DisplayRange(ImageTeeth.JoinTeeth(_hashSetSelected),_toothNumberingNomenclature);
			_isFillingText=false;
			_isTextTyped=false;
			labelHint.ForeColor=UiTheme.TextSecondary;
			labelHint.Text=Lan.g(this,"Click a tooth or drag along a row. The middle rows are the primary teeth.");
		}

		private ToothCell HitTest(Point point){
			float scale=LayoutManager.ScaleMy();
			return _listCells.FirstOrDefault(x => ScaleRectangle(x.Rectangle,scale).Contains(point));
		}

		///<summary>Reads the typed teeth. False, with the reason in the hint line, when they cannot be read.</summary>
		private bool ParseText(){
			string toothNumbers;
			try{
				toothNumbers=Tooth.ParseRange(textTeeth.Text);
			}
			catch(Exception ex){
				labelHint.ForeColor=UiTheme.Danger;
				labelHint.Text=ex.Message;
				return false;
			}
			_hashSetSelected=ImageTeeth.SplitTeeth(toothNumbers);
			FillText();
			chart.Invalidate();
			return true;
		}

		protected override bool ProcessCmdKey(ref Message msg,Keys keyData) {
			if(keyData==Keys.Escape){
				DialogResult=DialogResult.Cancel;
				return true;
			}
			//Enter is OK (reading typed teeth first), except on a focused button other than OK, which Enter clicks as usual.
			if(keyData==Keys.Enter && !(ActiveControl is System.Windows.Forms.Button && ActiveControl!=butOK)){
				butOK_Click(this,EventArgs.Empty);
				return true;
			}
			return base.ProcessCmdKey(ref msg,keyData);
		}

		private static string[] Range(int from,int to){
			return Enumerable.Range(from,to-from+1).Select(x => x.ToString()).ToArray();
		}

		private static Rectangle ScaleRectangle(Rectangle rectangle,float scale){
			return new Rectangle((int)Math.Round(rectangle.X*scale),(int)Math.Round(rectangle.Y*scale),(int)Math.Round(rectangle.Width*scale),
				(int)Math.Round(rectangle.Height*scale));
		}

		#endregion Methods - Private

		#region Class - ToothCell
		private class ToothCell {
			public int Col;
			public Rectangle Rectangle;
			public int Row;
			public string ToothNum;
		}
		#endregion Class - ToothCell
	}
}
