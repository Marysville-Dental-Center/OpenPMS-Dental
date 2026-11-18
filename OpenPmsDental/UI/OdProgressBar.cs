using CodeBase;
using OpenPmsDentalBusiness;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenPmsDental.UI {
	public class OdProgressBar:ProgressBar {
		private int _targetValue=0;
		private Color _targetColor=Color.Red;
		private Font _displayFont=DefaultFont;
		///<summary>Set by the window's LayoutManager: the scale of the corners and lines (section 3.3 of the UI refresh plan).</summary>
		public LayoutManagerForms LayoutManager=new LayoutManagerForms();

		public OdProgressBar() {
			this.SetStyle(ControlStyles.UserPaint,true);
		}

		[Category("Appearance"),Description("Sets a value for the target line")]
		public int TargetValue {
			get {
				return _targetValue;
			}
			set {
				_targetValue=value;
			}
		}

		[Category("Appearance"),Description("Sets a color for the target line")]
		public Color TargetColor {
			get {
				return _targetColor;
			}
			set {
				_targetColor=value;
			}
		}

		protected override void OnPaintBackground(PaintEventArgs pevent) {
			//Limits flickering
		}

		///<summary>Section 3.6 of the UI refresh plan, "Dialogs": the SurfaceRaised track with 4 px corners, the Accent fill (TextDisabled when
		///disabled) and the 1 px Stroke outline, as the design sheet's progress bar (UiDialogPaint.DrawProgress); then the target line in TargetColor
		///(the caller's colour) as before. No gradient: BackColor and ForeColor no longer colour the bar.</summary>
		protected override void OnPaint(PaintEventArgs e) {
			using(Bitmap bitmap=new Bitmap(Math.Max(1,Width),Math.Max(1,Height))) {
				using(Graphics g=Graphics.FromImage(bitmap)) {
					DrawBar(g);
				}
				e.Graphics.DrawImage(bitmap,0,0);
			}
		}

		///<summary>Draws the bar in its client area: the parent's background behind the corners, the track, the fill and the target line.</summary>
		public void DrawBar(Graphics g) {
			g.Clear(Parent?.BackColor ?? UiTheme.Surface);
			Rectangle rectangleTrack=new Rectangle(0,0,Width,Height);
			UiDialogPaint.DrawProgress(g,rectangleTrack,GetFraction(),Enabled,LayoutManager.ScaleMy());
			if(_targetValue>Minimum && _targetValue<=Maximum) {
				//Using a rectangle for a line because using a line was causing issues
				int targetValueLocation=(int)(this.Width*(((double)_targetValue-(double)Minimum)/((double)Maximum-(double)Minimum)));
				Rectangle targetLine=new Rectangle(targetValueLocation,0,4,this.Height);
				targetLine.Inflate(new Size(-1,-1));//Reduce target line color size
				using(SolidBrush brushLine=new SolidBrush(_targetColor)) {
					g.FillRectangle(brushLine,targetValueLocation,1,targetLine.Width,targetLine.Height);
				}
			}
		}

		///<summary>The share of the bar the fill covers: Value between Minimum and Maximum (0 when they are equal).</summary>
		public double GetFraction() {
			if(Maximum<=Minimum) {
				return 0;
			}
			return ((double)Value-(double)Minimum)/((double)Maximum-(double)Minimum);
		}

		protected override void OnEnabledChanged(EventArgs e) {
			base.OnEnabledChanged(e);
			Invalidate();
		}
	}
}
