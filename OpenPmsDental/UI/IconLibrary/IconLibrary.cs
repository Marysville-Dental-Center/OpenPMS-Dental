using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using CodeBase;

namespace OpenPmsDental.UI{
	//The icons of the toolbars, buttons and module bar. Since package UI-7b of the UI refresh (docs/specs/ui-refresh-plan.md, sections 3.5, 4.2
	//and 4.3; docs/port-notes/UI-7b.md) every member but three draws a Fluent UI System Icon from the one icon table in CodeBase
	//(CodeBase/UI/Icons, made by tools/New-IconData.ps1 from icons/manifest.json, where each member has its row keyed by its name). The WPF
	//IconLibrary (WpfControlsOD/UI/IconLibrary) draws the same rows. Chart32G, Chart32W and ChartMed32 keep their pictures (Gen_*.cs, through
	//IconSelector) until UI-8 makes the dental icons.
	//To add an icon: add the member here (and to the WPF enum when WPF draws it), add its manifest row (icons/README.md), run the generator.
	public enum EnumIcons{
		None,
		Account32,
		Acquire,
		Add,
		Appt32,
		ArrowLeft,
		ArrowRight,
		BreakAptX,
		Chart32,
		Chart32G,
		Chart32W,
		ChartMed32,
		CommLog,
		Complete,
		DeleteX,
		Email,
		Family32,
		ImageSelectorDoc,
		ImageSelectorFile,
		ImageSelectorFolder,
		ImageSelectorFolderWeb,
		ImageSelectorMount,
		ImageSelectorPhoto,
		ImageSelectorXray,
		Imaging32,
		Manage32,
		PatAdd,
		PatDelete,
		Patient,
		PatMoveFam,
		PatSelect,
		PatSetGuarantor,
		Probe,
		Recall,
		Text,
		TreatPlan32,
		TreatPlanMed32,
		Video,
		WebMail
	}

	///<summary></summary>
	public partial class IconLibrary{
		///<summary>This can be set to true if a customer computer is crashing. It forces use of GDI+ and no Direct2D (NoD2D.txt). The Fluent icons are
		///then filled with GDI+ (CodeBase.UiIcons), the same shapes antialiased.</summary>
		public static bool OnlyGDI;
		///<summary>The manifest's enum name of these icons' rows.</summary>
		public const string MANIFEST_ENUM="EnumIcons";
		///<summary>The suffix of the row of an icon's Filled style ("on" states: the selected module, section 4.3): Appt32Filled.</summary>
		public const string FILLED_SUFFIX="Filled";
		///<summary>The designs of each member by rising size, Regular and Filled, indexed by the enum's value; filled on first use.</summary>
		private static IconDesign[][] _arrayDesignsRegular;
		private static IconDesign[][] _arrayDesignsFilled;
		private static readonly object _lockDesigns=new object();

		#region Designs
		///<summary>Whether the icon still draws its picture (Gen_*.cs) instead of a Fluent icon: Chart32G, Chart32W and ChartMed32, until UI-8.</summary>
		public static bool IsPicture(EnumIcons icon){
			switch(icon){
				case EnumIcons.Chart32G:
				case EnumIcons.Chart32W:
				case EnumIcons.ChartMed32:
					return true;
				default:
					return false;
			}
		}

		///<summary>The icon's designs in the icon table by rising size, Regular or Filled. The Filled style falls back to Regular when the icon has no
		///Filled row. Empty for None and the pictures.</summary>
		public static IconDesign[] GetDesigns(EnumIcons icon,bool isFilled=false){
			if(_arrayDesignsRegular==null){
				lock(_lockDesigns){
					if(_arrayDesignsRegular==null){
						Array arrayValues=Enum.GetValues(typeof(EnumIcons));
						int count=0;
						foreach(object value in arrayValues){
							count=Math.Max(count,(int)value+1);
						}
						IconDesign[][] arrayRegular=new IconDesign[count][];
						IconDesign[][] arrayFilled=new IconDesign[count][];
						foreach(object value in arrayValues){
							EnumIcons iconEach=(EnumIcons)value;
							if(iconEach==EnumIcons.None || IsPicture(iconEach)){
								arrayRegular[(int)value]=new IconDesign[0];
								arrayFilled[(int)value]=new IconDesign[0];
								continue;
							}
							arrayRegular[(int)value]=UiIcons.GetDesigns(MANIFEST_ENUM,iconEach.ToString());
							IconDesign[] arrayDesignsFilled=UiIcons.GetDesigns(MANIFEST_ENUM,iconEach.ToString()+FILLED_SUFFIX);
							if(arrayDesignsFilled.Length==0){
								arrayDesignsFilled=arrayRegular[(int)value];
							}
							arrayFilled[(int)value]=arrayDesignsFilled;
						}
						_arrayDesignsFilled=arrayFilled;
						_arrayDesignsRegular=arrayRegular;
					}
				}
			}
			IconDesign[][] arrayDesigns=isFilled ? _arrayDesignsFilled : _arrayDesignsRegular;
			int index=(int)icon;
			if(index<0 || index>=arrayDesigns.Length){
				return new IconDesign[0];
			}
			return arrayDesigns[index];
		}

		///<summary>The design drawn in a square slot of sizeSlot device pixels (section 3.5: the largest that fits, never stretched up). Null for None and
		///the pictures.</summary>
		public static IconDesign GetDesign(EnumIcons icon,int sizeSlot,bool isFilled=false){
			return UiIcons.GetDesign(GetDesigns(icon,isFilled),sizeSlot);
		}

		///<summary>The icon's default colour: its manifest role as a colour token (UQ-4: Text, with Danger, Success and Accent for a few).</summary>
		public static Color GetColor(EnumIcons icon){
			IconDesign[] arrayDesigns=GetDesigns(icon);
			if(arrayDesigns.Length==0){
				return UiTheme.GetColor(EnumUiColor.Text);
			}
			return UiIcons.GetRoleColor(arrayDesigns[0]);
		}
		#endregion Designs

		#region Drawing
		public static void Draw(Direct2d d,EnumIcons icon,Rectangle rectangle){
			if(icon==EnumIcons.None){
				return;
			}
			if(!IsPicture(icon)){
				d.FillIcon(GetDesign(icon,Math.Min(rectangle.Width,rectangle.Height)),rectangle,GetColor(icon));
				return;
			}
			using Bitmap bitmap=GetPictureBitmap(icon);
			if(bitmap==null){
				return;
			}
			d.DrawBitmapImmediate(bitmap,rectangle);
		}

		///<summary>A WPF image of the icon at its size (Width: 22 or 32 px at 96 DPI) for the WPF toolbars and buttons that the WinForms helpers fill
		///(UIManagement). Null when there is nothing to draw.</summary>
		public static System.Windows.Media.Imaging.BitmapImage DrawWpf(EnumIcons icon){
			if(icon==EnumIcons.None){
				return null;
			}
			byte[] byteArray;
			if(IsPicture(icon)){
				string base64=IconSelector.GetBase64(icon);
				if(base64==""){
					return null;
				}
				byteArray=Convert.FromBase64String(base64);
			}
			else{
				int width=Width(icon);
				using Bitmap bitmap=UiIcons.GetBitmap(GetDesign(icon,width),width,GetColor(icon));
				using MemoryStream memoryStreamPng=new MemoryStream();
				bitmap.Save(memoryStreamPng,ImageFormat.Png);
				byteArray=memoryStreamPng.ToArray();
			}
			System.Windows.Media.Imaging.BitmapImage bitmapImage=new System.Windows.Media.Imaging.BitmapImage();
			bitmapImage.BeginInit();
			bitmapImage.StreamSource=new MemoryStream(byteArray);
			bitmapImage.EndInit();
			return bitmapImage;
		}

		///<summary>Draws the icon in its default colour (GetColor). A Fluent icon is filled with Direct2D into a bitmap of the rectangle over the
		///gradient (the bitmap target has no transparency, so the button's gradient is drawn under it to match), then that bitmap onto g; with OnlyGDI,
		///in design mode, or when Direct2D cannot start (which sets OnlyGDI from then on), it is filled with GDI+ on whatever g already shows. The
		///pictures (IsPicture) draw their bitmaps as before.</summary>
		public static void Draw(Graphics g,EnumIcons icon,Rectangle rectangle,Color colorGradientTop,Color colorGradientBottom,bool designMode=false){
			if(icon==EnumIcons.None){
				return;
			}
			if(!IsPicture(icon)){
				DrawFluent(g,GetDesign(icon,Math.Min(rectangle.Width,rectangle.Height)),rectangle,GetColor(icon),colorGradientTop,colorGradientBottom,designMode);
				return;
			}
			Bitmap bitmap=GetPictureBitmap(icon);
			if(bitmap==null){
				return;
			}
			if(icon!=EnumIcons.Chart32G && icon!=EnumIcons.Chart32W){
				g.DrawImage(bitmap,rectangle);
				bitmap.Dispose();
				return;
			}
			//Everything below this point is just for the chart.
			//Chart bitmap needs high quality. Downside is white halo, so draw it slighly smaller, then cover the halo
			GraphicsState graphicsState=g.Save();
			g.InterpolationMode=InterpolationMode.HighQualityBicubic;
			g.DrawImage(bitmap,rectangle);
			bitmap.Dispose();
			if(icon==EnumIcons.Chart32G){
				//this is the one with the white edge
				using(Pen pen=new Pen(Color.FromArgb(240,240,240))){
					g.DrawRectangle(pen,rectangle.X+0.75f,rectangle.Y+0.75f,rectangle.Width-1.25f,rectangle.Height-1.25f);
				}
			}
			g.Restore(graphicsState);
		}

		///<summary>Draws the icon in a colour of the caller's, Regular or Filled (section 3.6: the selected module's icon is Filled in Accent; UI-6), over
		///colorBack, the background the control has under the icon (Direct2D's bitmap has no transparency). As Draw: Direct2D, or GDI+ with OnlyGDI.
		///The pictures draw their bitmaps.</summary>
		public static void DrawColored(Graphics g,EnumIcons icon,Rectangle rectangle,Color color,Color colorBack,bool isFilled=false){
			if(icon==EnumIcons.None){
				return;
			}
			if(IsPicture(icon)){
				Draw(g,icon,rectangle,colorBack,colorBack);
				return;
			}
			DrawFluent(g,GetDesign(icon,Math.Min(rectangle.Width,rectangle.Height),isFilled),rectangle,color,colorBack,colorBack,designMode:false);
		}

		///<summary>Draws as disabled: a Fluent icon filled in TextDisabled (section 4.2, item 6), not greyed with ControlPaint; the pictures greyed as
		///before.</summary>
		public static void DrawDisabled(Graphics g,EnumIcons icon,Rectangle rectangle){
			if(icon==EnumIcons.None){
				return;
			}
			if(!IsPicture(icon)){
				UiIcons.Draw(g,GetDesign(icon,Math.Min(rectangle.Width,rectangle.Height)),rectangle,UiTheme.GetColor(EnumUiColor.TextDisabled));
				return;
			}
			Bitmap bitmap=GetPictureBitmap(icon);
			if(bitmap==null){
				return;
			}
			Bitmap bitmapDisabled=new Bitmap(bitmap.Width,bitmap.Height);
			Graphics gfx=Graphics.FromImage(bitmapDisabled);
			ControlPaint.DrawImageDisabled(gfx,bitmap,0,0,ColorOD.Control);
			g.DrawImage(bitmapDisabled,rectangle);
			gfx.Dispose();
			bitmapDisabled.Dispose();
			bitmap.Dispose();
		}

		///<summary>Fills a design in the rectangle: Direct2D into a bitmap over the gradient, then onto g pixel for pixel; GDI+ with OnlyGDI, in design
		///mode, or when Direct2D cannot start.</summary>
		private static void DrawFluent(Graphics g,IconDesign iconDesign,Rectangle rectangle,Color color,Color colorGradientTop,Color colorGradientBottom,bool designMode){
			if(iconDesign==null || rectangle.Width<=0 || rectangle.Height<=0){
				return;
			}
			if(designMode || OnlyGDI){
				UiIcons.Draw(g,iconDesign,rectangle,color);
				return;
			}
			using Bitmap bitmap=new Bitmap(rectangle.Width,rectangle.Height);
			Direct2d d=null;
			try{
				d=Direct2d.FromBitmap(bitmap);
			}
			catch{
				OnlyGDI=true;//from now on
				d?.Dispose();
				UiIcons.Draw(g,iconDesign,rectangle,color);
				return;
			}
			try{
				d.BeginDraw();
				d.CreateGradientBrush(0,colorGradientTop,colorGradientBottom,0,0,0,rectangle.Height);
				d.FillRectangleGradient(0,0,0,rectangle.Width,rectangle.Height);
				d.FillIcon(iconDesign,new Rectangle(0,0,rectangle.Width,rectangle.Height),color);
				d.EndDraw();
			}
			finally{
				d.Dispose();
			}
			GraphicsState graphicsState=g.Save();
			g.InterpolationMode=InterpolationMode.NearestNeighbor;
			g.PixelOffsetMode=PixelOffsetMode.Half;
			g.DrawImage(bitmap,rectangle,0,0,rectangle.Width,rectangle.Height,GraphicsUnit.Pixel);
			g.Restore(graphicsState);
		}

		///<summary>A new bitmap of a picture icon's Gen_*.cs image, or null when it has none. The caller disposes it.</summary>
		private static Bitmap GetPictureBitmap(EnumIcons icon){
			string base64=IconSelector.GetBase64(icon);
			if(base64==""){
				return null;
			}
			byte[] byteArray=Convert.FromBase64String(base64);
			using MemoryStream memoryStream=new MemoryStream(byteArray);
			using Bitmap bitmapRead=(Bitmap)Image.FromStream(memoryStream);
			return new Bitmap(bitmapRead);
		}
		#endregion Drawing

		///<summary>32 or 22</summary>
		public static int Width(EnumIcons icon){
			switch(icon){
				case EnumIcons.Account32:
				case EnumIcons.Appt32:
				case EnumIcons.Family32:
				case EnumIcons.Imaging32:
				case EnumIcons.Manage32:
				case EnumIcons.TreatPlan32:
					return 32;
				default:
					return 22;
			}
		}

		///<summary>Whether the icon is drawn as vectors: every icon but the pictures (IsPicture) since UI-7b.</summary>
		public static bool IsVector(EnumIcons icon){
			return icon!=EnumIcons.None && !IsPicture(icon);
		}

	}


}


//https://github.com/jingwood/d2dlib/issues/2
