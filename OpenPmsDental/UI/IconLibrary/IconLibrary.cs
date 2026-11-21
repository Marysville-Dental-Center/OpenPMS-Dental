using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using CodeBase;

namespace OpenPmsDental.UI{
	//The icons of the toolbars, buttons and module bar. Since package UI-7b of the UI refresh (docs/specs/ui-refresh-plan.md, sections 3.5, 4.2
	//and 4.3; docs/port-notes/UI-7b.md) every member but three draws a Fluent UI System Icon from the one icon table in CodeBase
	//(CodeBase/UI/Icons, made by tools/New-IconData.ps1 from icons/manifest.json, where each member has its row keyed by its name). The WPF
	//IconLibrary (WpfControlsOD/UI/IconLibrary) draws the same rows. Since UI-8 (section 4.4, docs/port-notes/UI-8.md) the module bar's chart
	//icons draw the custom dental icons of the same table instead (GetDentalIcon): Chart32G the Tooth, Chart32W the ToothFilled and ChartMed32
	//the ChartMedical; their pictures (Gen_*.cs, IconSelector) are gone.
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
		WebMail,
		//UI-9a-1 (Forms A-C, docs/port-notes/UI-9a.md): the bitmap pictures' purposes (section 4.3), each with its manifest row. Later UI-9a
		//slices append their own block.
		Down,
		Edit,
		Export,
		Label,
		Note,
		Pin,
		Preview,
		Print,
		Send,
		Sync,
		Up,
		//UI-9a-3 (Forms M-Q, docs/port-notes/UI-9a.md): purposes of the pictures of Forms M-Q, each with its manifest row.
		ChevronDown,
		ChevronUp,
		CloseDoor,
		Microphone,
		ZoomIn,
		ZoomOut,
		//UI-9a-4 (Forms R-S, docs/port-notes/UI-9a.md): the purpose of the recording picture of FormRecordAudio, with its manifest row.
		Record,
		//UI-9a-5 (Forms T-Z and Forms Reports, docs/port-notes/UI-9a.md): the copy and paste pictures of FormTaskEdit and FormUserQuery; the manifest
		//rows and the WPF members exist (Imaging toolbar).
		Copy,
		Paste,
		//The Excel export and query view pictures of FormQuery, with their manifest rows.
		ExportExcel,
		QueryView,
		//UI-9a-6 (Main Modules, User Controls and the shared projects, docs/port-notes/UI-9a.md): the pictures of the appointment and Manage modules, with their manifest rows.
		PinOff,
		Calculator,
		Backup,
		Deposit,
		Insurance,
		TaskList,
		//The pictures of the dashboard (OpenPmsDentalGraph) and of the signature box (OpenPmsDentalBusiness), with their manifest rows.
		Move,
		TableDeleteRow,
		TableDeleteColumn,
		TableInsertRow,
		TableInsertColumn,
		ChartAdd,
		Pen,
		Signature,
		//UI-9a-7 (the other projects, docs/port-notes/UI-9a.md): the toolbar of the report designer (RdlDesign), each with its manifest row.
		Table,
		Line,
		Image,
		Rectangle,
		Subreport,
		ListBullets,
		ChartBar,
		TextBox,
		Matrix,
		Save,
		Open,
		Cut,
		NewDocument,
		Undo,
		Pdf,
		Xml,
		Html,
		Mht,
		//UI-9b-1 (the main window and the module toolbars, docs/port-notes/UI-9b.md): the purposes of the module toolbars' ImageList pictures, each
		//with its manifest row; PerioChart draws UI-8's custom perio chart icon (GetDentalIcon), as Chart32G draws the tooth.
		Payment,
		Adjustment,
		ApptLists,
		Call,
		Unscheduled,
		Rx,
		Anesthesia,
		PerioChart,
		//UI-9b-2 (the imaging module and its tree, docs/port-notes/UI-9b.md): the purposes of ControlImages' two ImageLists. The WinForms enum adds the 11
		//imaging members the WPF enum already has (every WinForms member is also a WPF member), then the two new ones, each with its manifest row.
		ArrowsAll,
		Crop,
		Flip,
		Hand,
		Import,
		Info,
		RotateL,
		RotateR,
		ScanMulti,
		ScanPhoto,
		ScanXray,
		Scan,
		UsbStick,
		//UI-9b-3 (the other trees: the purposes of the task lists, the setup wizard, the security tree, the auto notes and the email inbox (docs/port-notes/UI-9b.md, section 4), each with its manifest row.)
		CheckBoxEmpty,
		CheckBoxChecked,
		CheckBoxPartial,
		TaskNew,
		TaskListNew,
		TaskListAdd,
		ChevronLeft,
		ChevronRight,
		ChevronDoubleRight,
		Reply,
		ReplyAll,
		Forward,
		Search,
		Inbox,
		InfoSelected
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
		///<summary>The custom dental icon (UI-8, section 4.4) an icon draws instead of an EnumIcons row: the module bar's chart icons, Chart32G the Tooth
		///(at rest), Chart32W the ToothFilled (the selected module) and ChartMed32 the ChartMedical (the medical layout); since UI-9b-1 the Chart
		///module's Perio Chart button, PerioChart the PerioChart. DentalIcon.None for every other icon. Their Filled style is the dental icon's Filled twin.</summary>
		public static DentalIcon GetDentalIcon(EnumIcons icon){
			switch(icon){
				case EnumIcons.Chart32G:
					return DentalIcon.Tooth;
				case EnumIcons.Chart32W:
					return DentalIcon.ToothFilled;
				case EnumIcons.ChartMed32:
					return DentalIcon.ChartMedical;
				case EnumIcons.PerioChart:
					return DentalIcon.PerioChart;
				default:
					return DentalIcon.None;
			}
		}

		///<summary>The icon's designs in the icon table by rising size, Regular or Filled. The Filled style falls back to Regular when the icon has no
		///Filled row. Empty for None.</summary>
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
							if(iconEach==EnumIcons.None){
								arrayRegular[(int)value]=new IconDesign[0];
								arrayFilled[(int)value]=new IconDesign[0];
								continue;
							}
							DentalIcon dentalIcon=GetDentalIcon(iconEach);
							if(dentalIcon!=DentalIcon.None){
								arrayRegular[(int)value]=UiIcons.GetDesigns(dentalIcon);
								arrayFilled[(int)value]=UiIcons.GetDesigns(UiIcons.GetFilled(dentalIcon));
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

		///<summary>The design drawn in a square slot of sizeSlot device pixels (section 3.5: the largest that fits, never stretched up). Null for
		///None.</summary>
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
			d.FillIcon(GetDesign(icon,Math.Min(rectangle.Width,rectangle.Height)),rectangle,GetColor(icon));
		}

		///<summary>A WPF image of the icon at its size (Width: 22 or 32 px at 96 DPI) for the WPF toolbars and buttons that the WinForms helpers fill
		///(UIManagement). Null when there is nothing to draw.</summary>
		public static System.Windows.Media.Imaging.BitmapImage DrawWpf(EnumIcons icon){
			if(icon==EnumIcons.None){
				return null;
			}
			int width=Width(icon);
			byte[] byteArray;
			using(Bitmap bitmap=UiIcons.GetBitmap(GetDesign(icon,width),width,GetColor(icon)))
			using(MemoryStream memoryStreamPng=new MemoryStream()){
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
		///in design mode, or when Direct2D cannot start (which sets OnlyGDI from then on), it is filled with GDI+ on whatever g already shows.</summary>
		public static void Draw(Graphics g,EnumIcons icon,Rectangle rectangle,Color colorGradientTop,Color colorGradientBottom,bool designMode=false){
			if(icon==EnumIcons.None){
				return;
			}
			DrawFluent(g,GetDesign(icon,Math.Min(rectangle.Width,rectangle.Height)),rectangle,GetColor(icon),colorGradientTop,colorGradientBottom,designMode);
		}

		///<summary>Draws the icon in a colour of the caller's, Regular or Filled (section 3.6: the selected module's icon is Filled in Accent; UI-6), over
		///colorBack, the background the control has under the icon (Direct2D's bitmap has no transparency). As Draw: Direct2D, or GDI+ with OnlyGDI.</summary>
		public static void DrawColored(Graphics g,EnumIcons icon,Rectangle rectangle,Color color,Color colorBack,bool isFilled=false){
			if(icon==EnumIcons.None){
				return;
			}
			DrawFluent(g,GetDesign(icon,Math.Min(rectangle.Width,rectangle.Height),isFilled),rectangle,color,colorBack,colorBack,designMode:false);
		}

		///<summary>Draws as disabled: the icon filled in TextDisabled (section 4.2, item 6), not greyed with ControlPaint.</summary>
		public static void DrawDisabled(Graphics g,EnumIcons icon,Rectangle rectangle){
			if(icon==EnumIcons.None){
				return;
			}
			UiIcons.Draw(g,GetDesign(icon,Math.Min(rectangle.Width,rectangle.Height)),rectangle,UiTheme.GetColor(EnumUiColor.TextDisabled));
		}

		///<summary>A new 32-bit ARGB bitmap of width x height pixels with the icon in its role colour (GetColor) on transparent, centred in the
		///largest square that fits (section 3.5: the largest design that fits, never stretched up). For a control that shows a picture where a
		///bitmap used to be (UI-9a, docs/port-notes/UI-9a.md): the picture's 96 DPI size, so nothing moves; the control scales it as it scaled the
		///bitmap. Filled with GDI+, so it works in design mode, with OnlyGDI and before any window exists. The caller (the control) owns it.</summary>
		public static Bitmap GetBitmap(EnumIcons icon,int width,int height){
			return GetBitmap(GetDesign(icon,Math.Min(width,height)),width,height,GetColor(icon));
		}

		///<summary>The icon factory of the toolbars and lists (UI-9b, docs/port-notes/UI-9b.md; section 4.2, item 5): a new 32-bit ImageList of
		///sizePixels square holding one image per icon, in the order given, each under its member's name as the key (Images.IndexOfKey("Print")),
		///each the icon's design for the size (section 3.5: the largest that fits, centred, never stretched up) in its role colour on transparent
		///(GetBitmap). Pass the real pixel size at the current DPI (ToolBarOD.SetImageListIcons passes its scaled 22 px slot) and build a new list
		///when it changes. A list may take more images after it is made (program links, ProgramL.LoadToolBar; plug-ins), by key or by index. The
		///caller owns the list. CodeBase's UiIcons.GetImageList makes it: the one place the program makes an ImageList in code.</summary>
		public static System.Windows.Forms.ImageList GetImageList(EnumIcons[] arrayIcons,int sizePixels){
			sizePixels=Math.Max(sizePixels,1);
			string[] arrayKeys=new string[arrayIcons.Length];
			Bitmap[] arrayBitmaps=new Bitmap[arrayIcons.Length];
			for(int i=0;i<arrayIcons.Length;i++){
				arrayKeys[i]=arrayIcons[i].ToString();
				arrayBitmaps[i]=GetBitmap(arrayIcons[i],sizePixels,sizePixels);
			}
			return UiIcons.GetImageList(sizePixels,arrayKeys,arrayBitmaps);
		}

		///<summary>The list of a tree or list view that must follow the DPI (UI-9b-2, docs/port-notes/UI-9b.md, section 1.3): imageList when it is
		///already sizePixels square and holds the icons, else a new GetImageList and the old one disposed. Call it where the control is shown or its
		///scale may have changed, with the real pixel size (LayoutManager.Scale(16)); keep the index order of arrayIcons so every ImageIndex and
		///ImageKey keeps its picture's purpose. The caller owns the result.</summary>
		public static System.Windows.Forms.ImageList GetImageListForSize(System.Windows.Forms.ImageList imageList,EnumIcons[] arrayIcons,int sizePixels){
			sizePixels=Math.Max(sizePixels,1);
			if(imageList!=null && imageList.ImageSize.Width==sizePixels && imageList.ImageSize.Height==sizePixels && imageList.Images.Count>=arrayIcons.Length){
				return imageList;
			}
			imageList?.Dispose();
			return GetImageList(arrayIcons,sizePixels);
		}

		///<summary>GetBitmap for a custom dental icon (UI-8), in its role colour.</summary>
		public static Bitmap GetBitmap(DentalIcon dentalIcon,int width,int height){
			IconDesign iconDesign=UiIcons.GetDesign(dentalIcon,Math.Min(width,height));
			Color color=UiTheme.GetColor(EnumUiColor.Text);
			if(iconDesign!=null){
				color=UiIcons.GetRoleColor(iconDesign);
			}
			return GetBitmap(iconDesign,width,height,color);
		}

		private static Bitmap GetBitmap(IconDesign iconDesign,int width,int height,Color color){
			Bitmap bitmap=new Bitmap(Math.Max(width,1),Math.Max(height,1),PixelFormat.Format32bppArgb);
			bitmap.SetResolution(96,96);//as the pictures: drawn at their pixel size
			if(iconDesign==null){
				return bitmap;
			}
			int sizeSlot=Math.Min(width,height);
			using(Graphics g=Graphics.FromImage(bitmap)){
				UiIcons.Draw(g,iconDesign,new Rectangle((width-sizeSlot)/2,(height-sizeSlot)/2,sizeSlot,sizeSlot),color);
			}
			return bitmap;
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

		///<summary>Whether the icon is drawn as vectors: every icon since UI-8 (the Fluent icons since UI-7b, the chart icons' dental icons since
		///UI-8).</summary>
		public static bool IsVector(EnumIcons icon){
			return icon!=EnumIcons.None;
		}

	}


}


//https://github.com/jingwood/d2dlib/issues/2
