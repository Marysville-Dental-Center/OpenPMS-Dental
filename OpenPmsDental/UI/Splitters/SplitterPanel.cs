using CodeBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenPmsDental.UI {
	///<summary>Analogous to the MS SplitterPanel.</summary>
	[Designer(typeof(Design.SplitterPanelDesigner))]
	public class SplitterPanel:Panel {
		
		public SplitterPanel() { 
			DoubleBuffered=true;
		}

		///<summary>Not meaningful for this control, so we ignore it.</summary>
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new DockStyle Dock{
			get;
			set;
		}

		///<summary>Not meaningful for this control, so we ignore it.</summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new Point Location{
			get=>base.Location;
			set=>value=base.Location;
		}

		///<summary>The panel's border on its outer sides (section 3.6: containers 1 px Stroke, scaled by the line rule of 3.3), in the split container's
		///GetColorBorderLine: Stroke, or the ColorBorder a window set.</summary>
		protected override void OnPaint(PaintEventArgs e){
			base.OnPaint(e);
			if(Parent is null){
				return;
			}
			SplitContainer splitContainer=Parent as SplitContainer;
			if(splitContainer is null){
				return;
			}
			Graphics g = e.Graphics;
			int w=UiTheme.GetLineWidth(splitContainer.LayoutManager.ScaleMy());
			using SolidBrush solidBrush=new SolidBrush(splitContainer.GetColorBorderLine());
			bool isLeft;
			bool isTop;
			bool isRight;
			bool isBottom;
			if(splitContainer.Orientation==Orientation.Vertical){
				isTop=true;
				isBottom=true;
				isLeft=this==splitContainer.Panel1 || splitContainer.Panel1Collapsed;//Left panel, or the right one alone
				isRight=this==splitContainer.Panel2 || splitContainer.Panel2Collapsed;
			}
			else{//horizontal
				isLeft=true;
				isRight=true;
				isTop=this==splitContainer.Panel1 || splitContainer.Panel1Collapsed;//Top panel, or the bottom one alone
				isBottom=this==splitContainer.Panel2 || splitContainer.Panel2Collapsed;
			}
			if(this!=splitContainer.Panel1 && this!=splitContainer.Panel2){
				return;
			}
			if(isTop){
				g.FillRectangle(solidBrush,0,0,Width,w);
			}
			if(isBottom){
				g.FillRectangle(solidBrush,0,Height-w,Width,w);
			}
			if(isLeft){
				g.FillRectangle(solidBrush,0,0,w,Height);
			}
			if(isRight){
				g.FillRectangle(solidBrush,Width-w,0,w,Height);
			}
		}


	}
}
