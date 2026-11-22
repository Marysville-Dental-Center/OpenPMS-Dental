using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;

using System.Windows.Forms;
using CodeBase;


namespace OpenPmsDental{
///<summary></summary>
	public class TableTimeBar : OpenPmsDental.ContrTable{
		private System.ComponentModel.IContainer components = null;
		///<summary>UI-13: a 5-minute cell's height at 96 dpi; the cells and the slider's steps take it at the window's scale (GetCellHeight).</summary>
		private const int ROW_HEIGHT_96=14;
		///<summary>UI-13: the column's width at 96 dpi.</summary>
		private const int COL_WIDTH_96=13;
		///<summary>UI-13: true while the bar lays itself out again after its window's scale changed (OnSizeChanged).</summary>
		private bool _isScaling;
		///<summary>UI-13: true once the constructor has set the cells up (a size change before that is the designer's).</summary>
		private bool _isConstructed;

		///<summary></summary>
		public TableTimeBar(){
			InitializeComponent();// This call is required by the Windows Form Designer.
			MaxRows=40;
			MaxCols=1;
			ShowScroll=false;
			FieldsArePresent=false;
			HeadingIsPresent=false;
			InstantClassesPar();
			SetRowHeight(0,39,ROW_HEIGHT_96);
			ColWidth[0]=COL_WIDTH_96;
			ColAlign[0]=HorizontalAlignment.Center;
			SetGridColor(Color.LightGray);
			SetModernFrame();//UI-13: one frame around the column (PaintModern)
			/*TopBorder[0,6]=Color.Black;
			TopBorder[0,12]=Color.Black;
			TopBorder[0,18]=Color.Black;
			TopBorder[0,24]=Color.Black;
			TopBorder[0,30]=Color.Black;
			TopBorder[0,36]=Color.Black;*/
			LayoutTables();
			_isConstructed=true;
		}

		///<summary></summary>
		protected override void Dispose( bool disposing ){
			if( disposing ){
				if (components != null){
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Designer generated code

		private void InitializeComponent(){
			this.SuspendLayout();
			// 
			// TableTimeBar
			// 
			this.Name = "TableTimeBar";
			this.Load += new System.EventHandler(this.TableTimeBar_Load);
			this.ResumeLayout(false);

		}
		#endregion

		private void TableTimeBar_Load(object sender, System.EventArgs e) {
			LayoutTables();
		}

		///<summary>UI-13: the cells at the window's scale (dpi and zoom): 14 by 13 px at 96 dpi, scaled through the LayoutManager the window passes
		///in (LayoutManagerForms.PassLayoutManager), so the column, its cells and its letters grow with the window as its other controls do.</summary>
		protected override void SetCellSizesForScale(){
			if(MaxRows<1 || RowHeight==null || RowHeight.Length<MaxRows){
				return;
			}
			SetRowHeight(0,MaxRows-1,Math.Max(1,LayoutManager.Scale(ROW_HEIGHT_96)));
			ColWidth[0]=Math.Max(1,LayoutManager.Scale(COL_WIDTH_96));
		}

		///<summary>UI-13: when the window lays the bar out at a new scale (a move to a monitor of another dpi, a zoom change), its cells follow.
		///</summary>
		protected override void OnSizeChanged(EventArgs e){
			base.OnSizeChanged(e);
			if(!_isConstructed || _isScaling || MaxRows<1 || RowHeight==null || RowHeight.Length<1 || RowHeight[0]==LayoutManager.Scale(ROW_HEIGHT_96)){
				return;
			}
			_isScaling=true;
			try{
				LayoutTables(true);
			}
			finally{
				_isScaling=false;
			}
		}

		///<summary>UI-13: a 5-minute cell's height in pixels at the window's scale (14 at 96 dpi): the slider's step.</summary>
		public int GetCellHeight(){
			if(RowHeight==null || RowHeight.Length<1 || RowHeight[0]<1){
				return ROW_HEIGHT_96;
			}
			return RowHeight[0];
		}

		///<summary>UI-13: where the windows put the slider's handle under a pattern of countCells cells, in the parent's coordinates: 2 px in from
		///the bar's left and 1 px under the last cell at 96 dpi, scaled.</summary>
		public Point GetSliderLocation(int countCells){
			return new Point(Location.X+LayoutManager.Scale(2),Location.Y+countCells*GetCellHeight()+LayoutManager.Scale(1));
		}

		///<summary>UI-13: the number of cells the slider's handle at ySlider (its top, in the parent's coordinates) stands for: the windows' mouse
		///math, with the cell's height at the window's scale.</summary>
		public int GetSliderStep(int ySlider){
			return (int)(Math.Round((Decimal)(ySlider-Location.Y)/GetCellHeight()));
		}

		///<summary>UI-13: the scale the bar is drawn at: its cells' height against 96 dpi's 14 px.</summary>
		private float GetScaleDrawn(){
			return Math.Max(1f,GetCellHeight()/(float)ROW_HEIGHT_96);
		}

		///<summary>UI-13 (docs/port-notes/UI-13.md, the owner's G-UI3-ui12p row "Time pattern on the left looks odd"): the time pattern drawn as one
		///framed column of 5-minute cells in the input frame's look (SurfaceRaised, a 1 px StrokeStrong frame with 2 px corners, Gridline between
		///the cells); a provider cell (X) filled Accent with its letter in TextOnAccent, an assistant cell (/) AccentSubtle with its letter in Text,
		///a white line between two filled cells so the pattern reads as steps. Every cell where ContrTable puts it: the clicks and the slider's
		///14 px steps are unchanged.</summary>
		protected override bool IsModernLook{
			get{
				return true;
			}
		}

		protected override void PaintModern(Graphics g){
			//Every size at the scale the cells are laid out at (SetCellSizesForScale): the frame's line and corners, the lines between cells, the letters.
			float scale=GetScaleDrawn();
			int widthLine=UiTheme.GetLineWidth(scale);
			Rectangle rectangleFrame=new Rectangle(0,0,Width,Height);
			Color colorParent=Parent==null ? SystemColors.Control : Parent.BackColor;
			if(colorParent.A<255){
				colorParent=SystemColors.Control;
			}
			using(SolidBrush brushParent=new SolidBrush(colorParent)){
				g.FillRectangle(brushParent,rectangleFrame);
			}
			UiControlPaint.Fill(g,EnumUiColor.SurfaceRaised,rectangleFrame,UiTheme.CORNER_CHECKBOX,scale);
			System.Drawing.Drawing2D.GraphicsState graphicsState=g.Save();
			using(System.Drawing.Drawing2D.GraphicsPath graphicsPath=UiTheme.GetRoundedRectanglePath(Rectangle.Inflate(rectangleFrame,-widthLine,-widthLine),
				UiTheme.GetCornerRadius(UiTheme.CORNER_CHECKBOX,scale)))
			{
				g.SetClip(graphicsPath,System.Drawing.Drawing2D.CombineMode.Intersect);
				for(int i=0;i<MaxRows;i++){
					Rectangle rectangleCell=GetCellRectangleLaidOut(0,i);
					rectangleCell=new Rectangle(widthLine,rectangleCell.Y,Width-2*widthLine,rectangleCell.Height);
					string text=Cell[0,i]??"";
					bool isFilled=text!="";
					bool isFilledBefore=i>0 && !string.IsNullOrEmpty(Cell[0,i-1]);
					if(isFilled){
						g.FillRectangle(UiTheme.GetBrush(text=="X" ? EnumUiColor.Accent : EnumUiColor.AccentSubtle),rectangleCell);
					}
					if(i>0){
						//The line over the cell: white between two filled cells, Gridline otherwise.
						EnumUiColor uiColorLine=isFilled && isFilledBefore ? EnumUiColor.SurfaceRaised : EnumUiColor.Gridline;
						g.FillRectangle(UiTheme.GetBrush(uiColorLine),new Rectangle(rectangleCell.X,rectangleCell.Y,rectangleCell.Width,widthLine));
					}
				}
				//The letters after every fill (a letter a little taller than its cell is not covered by the next cell's fill), in the grid font at the
				//cells' scale in pixels, so they grow with the cells at every dpi and zoom.
				using(Font font=new Font(UiTheme.FontGrid.FontFamily,UiTheme.FontGrid.SizeInPoints*96f/72f*scale,UiTheme.FontGrid.Style,GraphicsUnit.Pixel))
				using(StringFormat stringFormat=new StringFormat())
				{
					stringFormat.Alignment=StringAlignment.Center;
					stringFormat.LineAlignment=StringAlignment.Center;
					for(int i=0;i<MaxRows;i++){
						string text=Cell[0,i]??"";
						if(text==""){
							continue;
						}
						Rectangle rectangleCell=GetCellRectangleLaidOut(0,i);
						rectangleCell=new Rectangle(widthLine,rectangleCell.Y,Width-2*widthLine,rectangleCell.Height);
						//GDI+ text: it follows the move each panel's paint applies to the picture (GDI text drew some letters where the panel was not).
						g.DrawString(text,font,UiTheme.GetBrush(text=="X" ? EnumUiColor.TextOnAccent : EnumUiColor.Text),rectangleCell,stringFormat);
					}
				}
			}
			g.Restore(graphicsState);
			UiControlPaint.Outline(g,EnumUiColor.StrokeStrong,rectangleFrame,widthLine,UiTheme.CORNER_CHECKBOX,scale);
		}

		private void butSlider_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e) {
		
		}

		private void butSlider_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e) {
		
		}

		private void butSlider_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e) {
		
		}

		

	}
}

