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

		///<summary></summary>
		public TableTimeBar(){
			InitializeComponent();// This call is required by the Windows Form Designer.
			MaxRows=40;
			MaxCols=1;
			ShowScroll=false;
			FieldsArePresent=false;
			HeadingIsPresent=false;
			InstantClassesPar();
			SetRowHeight(0,39,14);
			ColWidth[0]=13;
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
			Rectangle rectangleFrame=new Rectangle(0,0,Width,Height);
			Color colorParent=Parent==null ? SystemColors.Control : Parent.BackColor;
			if(colorParent.A<255){
				colorParent=SystemColors.Control;
			}
			using(SolidBrush brushParent=new SolidBrush(colorParent)){
				g.FillRectangle(brushParent,rectangleFrame);
			}
			UiControlPaint.Fill(g,EnumUiColor.SurfaceRaised,rectangleFrame,UiTheme.CORNER_CHECKBOX,1f);
			System.Drawing.Drawing2D.GraphicsState graphicsState=g.Save();
			using(System.Drawing.Drawing2D.GraphicsPath graphicsPath=UiTheme.GetRoundedRectanglePath(Rectangle.Inflate(rectangleFrame,-1,-1),UiTheme.CORNER_CHECKBOX)){
				g.SetClip(graphicsPath,System.Drawing.Drawing2D.CombineMode.Intersect);
				for(int i=0;i<MaxRows;i++){
					Rectangle rectangleCell=GetCellRectangleLaidOut(0,i);
					rectangleCell=new Rectangle(1,rectangleCell.Y,Width-2,rectangleCell.Height);
					string text=Cell[0,i]??"";
					bool isFilled=text!="";
					bool isFilledBefore=i>0 && !string.IsNullOrEmpty(Cell[0,i-1]);
					if(isFilled){
						g.FillRectangle(UiTheme.GetBrush(text=="X" ? EnumUiColor.Accent : EnumUiColor.AccentSubtle),rectangleCell);
					}
					if(i>0){
						//The line over the cell: white between two filled cells, Gridline otherwise.
						EnumUiColor uiColorLine=isFilled && isFilledBefore ? EnumUiColor.SurfaceRaised : EnumUiColor.Gridline;
						g.FillRectangle(UiTheme.GetBrush(uiColorLine),new Rectangle(rectangleCell.X,rectangleCell.Y,rectangleCell.Width,1));
					}
				}
				//The letters after every fill (a letter a little taller than its 14 px cell is not covered by the next cell's fill).
				for(int i=0;i<MaxRows;i++){
					string text=Cell[0,i]??"";
					if(text==""){
						continue;
					}
					Rectangle rectangleCell=GetCellRectangleLaidOut(0,i);
					rectangleCell=new Rectangle(1,rectangleCell.Y,Width-2,rectangleCell.Height);
					//GDI+ text: it follows the move each panel's paint applies to the picture (GDI text drew some letters where the panel was not).
					using(StringFormat stringFormat=new StringFormat()){
						stringFormat.Alignment=StringAlignment.Center;
						stringFormat.LineAlignment=StringAlignment.Center;
						g.DrawString(text,UiTheme.FontGrid,UiTheme.GetBrush(text=="X" ? EnumUiColor.TextOnAccent : EnumUiColor.Text),rectangleCell,stringFormat);
					}
				}
			}
			g.Restore(graphicsState);
			UiControlPaint.Outline(g,EnumUiColor.StrokeStrong,rectangleFrame,1,UiTheme.CORNER_CHECKBOX,1f);
		}

		private void butSlider_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e) {
		
		}

		private void butSlider_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e) {
		
		}

		private void butSlider_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e) {
		
		}

		

	}
}

