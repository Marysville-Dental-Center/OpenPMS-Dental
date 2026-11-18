using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Data;
using System.Windows.Forms;
using CodeBase;

namespace OpenPmsDental.UI
{
	/// <summary>Better and simpler than the MS picturebox.  Always resizes the image to fit in the box.  Never crops or stretches.</summary>
	public class ODPictureBox : System.Windows.Forms.Control
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private Image image;
		private string textNullImage;
		private bool _hasBorder;

		///<summary></summary>
		public ODPictureBox()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();
			DoubleBuffered=true;
			HasBorder=true;//Required because we specified a default value of true for the designer.
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing ){
			if( disposing ){
				image?.Dispose();
				components?.Dispose();
			}
			base.Dispose( disposing );
		}

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
		}
		#endregion

		///<summary></summary>
		[Category("OD"),Description("The image displayed in the PictureBox.")]
		[DefaultValue(null)]
		public Image Image{
			set{
				//Return early if same image, otherwise when we dispose of 'image', 'value' will also be disposed of as well.
				if(image==value) {
					return;
				}
				image?.Dispose();
				image=value;
				Invalidate();
			}
			get{
				return image;
			}
		}

		///<summary></summary>
		[Category("OD"),Description("The text that will display if the image is null.")]
		public string TextNullImage{
			set{
				textNullImage=value;
				Invalidate();
			}
			get{
				return textNullImage;
			}
		}

		///<summary></summary>
		[Category("OD")]
		[Description("Determines whether to draw the border of the picturebox.")]
		[DefaultValue(true)]
		public bool HasBorder {
			set {
				_hasBorder=value;
				Invalidate();
			}
			get {
				return _hasBorder;
			}
		}

		///<summary>UI-4c-2: true when the picture box is office content (docs/specs/ui-refresh-plan.md, section 5.1), so it keeps today's look: the Gray
		///border, the Gray hint and 1 px padding. That is when it, or a control it sits in, is a content type or a panel marked with
		///LayoutManagerForms.MarkContent (the patient dashboard, a print preview). DashPatPicture overrides it to true: the dashboard's picture widget is
		///content wherever it sits.</summary>
		protected virtual bool IsContentLook {
			get {
				return LayoutManagerForms.IsContent(this,Parent);
			}
		}

		///<summary></summary>
		protected override void OnPaint(PaintEventArgs e) {
			//base.OnPaint (e);
			Graphics g=e.Graphics;
			g.InterpolationMode=InterpolationMode.High;
			bool isContentLook=IsContentLook;
			if(HasBorder) {
				if(isContentLook) {
					g.DrawRectangle(Pens.Gray,0,0,Width-1,Height-1);//office content keeps today's border
				}
				else {
					//UI-4c-2 (section 3.6 of the UI refresh plan): the container border, 1 px Stroke, square (it was Gray). The picture inside is the office's.
					float scale=DeviceDpi/96f;
					UiControlPaint.Outline(g,EnumUiColor.Stroke,new Rectangle(0,0,Width,Height),UiTheme.GetLineWidth(scale),0,scale);
				}
			}
			float heightImage=-1;
			float widthImage=-1;
			if(image!=null) {
				ODException.SwallowAnyException(() => {
					heightImage=(float)image.Height;
					widthImage=(float)image.Width;
				});
			}
			if(image==null || heightImage<=0 || widthImage<=0){
				StringFormat stringFormat=new StringFormat();
				stringFormat.Alignment=StringAlignment.Center;
				stringFormat.LineAlignment=StringAlignment.Center;
				if(isContentLook) {
					using(SolidBrush solidBrush=new SolidBrush(Color.Gray)) {//office content keeps today's hint
						g.DrawString(textNullImage,this.Font,solidBrush,new RectangleF(0,0,Width,Height),stringFormat);
					}
				}
				else {
					//The hint when there is no picture: TextSecondary (it was Gray).
					g.DrawString(textNullImage,this.Font,UiTheme.GetBrush(EnumUiColor.TextSecondary),new RectangleF(0,0,Width,Height),stringFormat);
				}
			}
			else{
				float ratio;
				//amount of space around the outside edge of the image in pixels. 1px shows border, 2px shows border and a little whitespace.
				int padding=0;
				if(HasBorder) {
					//the border's width: 1 px, 2 px at 200%; office content keeps today's 1 px
					padding=isContentLook ? 1 : UiTheme.GetLineWidth(DeviceDpi/96f);
				}
				//Debug.WriteLine("Hratio:"+(float)image.Height/(float)Height+"Wratio:"+(float)image.Width/(float)Width);
				if(heightImage/(float)Height > widthImage/(float)Width){//Image is proportionally taller
					ratio=(float)Height/heightImage;
					g.DrawImage(image,new RectangleF(Width/2-(widthImage*ratio)/2+padding,padding,widthImage*ratio-(2*padding),Height-(2*padding)));
				}
				else{//image proportionally wider
					ratio=(float)Width/widthImage;
					g.DrawImage(image,new RectangleF(padding,(float)Height/2-(heightImage*ratio)/2+padding,Width-(2*padding),heightImage*ratio-(2*padding)));
				}
			}
		}

		///<summary></summary>
		protected override void OnResize(EventArgs e) {
			base.OnResize (e);
			Invalidate();
		}





	}


}
