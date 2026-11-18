// The real windows of gallery G-UI1 part 1 (package UI-1, docs/specs/ui-refresh-plan.md sections 7 and 8, docs/port-notes/UI-1.md):
// FormPatientEdit, FormProcEdit, the Account module (ControlAccount), the Account module's toolbar with its grid, and one WPF Frm
// (FrmProviderPick), so the owner judges the font on real windows. Each WinForms window is built off screen with UI-0b's builder (no
// constructor, no database, never shown) and drawn with the test-side font swap in today's font, the chosen options (Segoe UI 9, 17 px grid
// rows) and the other options (Segoe UI 8.25, 16 px rows). They keep today's colours and controls: the colours are judged on the sheet. The
// toolbar and the grid get synthetic content (the Account module's button captions; made-up rows), since the builder does not run the module's
// own code. The WPF Frm already uses Segoe UI 11.5 (= 9 pt) and is drawn once, with RenderTargetBitmap, without a window.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CodeBase;
using OpenPmsDental;
using OpenPmsDental.UI;
using UiRefreshTests.Scan;
using UiRefreshTests.TextFit;

namespace UiRefreshTests {
	///<summary>A font option of the real windows: the body font (UQ-1) and the grid's cell font, which sets the row height (UQ-2).</summary>
	public class SheetWindowOption {
		public string Folder;
		public TextFitFontOption FontOption;
		public string GridFamily;
		public float GridSize;

		public bool IsToday {
			get { return FontOption.IsToday; }
		}
	}

	public static class DesignSheetWindows {
		public static readonly SheetWindowOption OptionToday=new SheetWindowOption { Folder="",FontOption=TextFitFontOption.Today,GridFamily="",GridSize=8.5f };
		public static readonly SheetWindowOption OptionChosen=new SheetWindowOption { Folder="windows-9pt-17px",FontOption=TextFitFontOption.Segoe9,
			GridFamily=UiTheme.FONT_FAMILY,GridSize=UiTheme.FONT_SIZE_BODY };
		public static readonly SheetWindowOption OptionOther=new SheetWindowOption { Folder="windows-8.25pt-16px",FontOption=TextFitFontOption.Segoe825,
			GridFamily=UiTheme.FONT_FAMILY,GridSize=UiTheme.FONT_SIZE_BODY_COMPACT };
		public const string FOLDER_WPF="windows-wpf";

		///<summary>The pictures of one window in one option: the picture's file name to the bitmap.</summary>
		private class Shot {
			public string Name;
			public Bitmap Bitmap;
		}

		///<summary>Draws every window in today's font into before\{option folder} and in each option into after\{option folder}, and the Frm into
		///after\windows-wpf. Returns the files written. Runs on the caller's STA thread; nothing is shown.</summary>
		public static List<string> Render(string folderBefore,string folderAfter,List<string> listNotes) {
			List<string> listFiles=new List<string>();
			NoDatabaseGuard.Install();
			if(!IsGdiTextSmoothed()) {
				listNotes.Add("This session draws Windows' own (GDI) text without smoothing (a disconnected or remote session), so the real windows' text "
					+"looks heavier and more jagged than on a desktop with ClearType. Their sizes and line breaks are right; for the final look of the font, "
					+"run this test again in a connected session.");
			}
			foreach(SheetWindowOption option in new[] { OptionChosen,OptionOther }) {
				foreach(Shot shot in RenderAll(OptionToday,listNotes)) {
					listFiles.Add(SaveShot(shot,Path.Combine(folderBefore,option.Folder)));
				}
				foreach(Shot shot in RenderAll(option,listNotes)) {
					listFiles.Add(SaveShot(shot,Path.Combine(folderAfter,option.Folder)));
				}
			}
			DefaultFontOverride.Set(TextFitFontOption.Today,1f);
			return listFiles;
		}

		private static string SaveShot(Shot shot,string folder) {
			Directory.CreateDirectory(folder);
			string path=Path.Combine(folder,shot.Name+".png");
			shot.Bitmap.Save(path,ImageFormat.Png);
			shot.Bitmap.Dispose();
			return path;
		}

		private static List<Shot> RenderAll(SheetWindowOption option,List<string> listNotes) {
			List<Shot> listShots=new List<Shot>();
			DefaultFontOverride.Set(option.FontOption,1f);
			listShots.Add(RenderForm(typeof(FormPatientEdit),option));
			listShots.Add(RenderForm(typeof(FormProcEdit),option));
			listShots.AddRange(RenderAccount(option,listNotes));
			return listShots;
		}

		private static Shot RenderForm(Type type,SheetWindowOption option) {
			ScanType scanType=new ScanType { Type=type,Kind=BuildKind.LayoutManagerWindow,Assembly=type.Assembly.GetName().Name,WindowKey=type.FullName };
			using(BuiltWindow builtWindow=WindowBuilder.Build(scanType)) {
				if(builtWindow.LayoutManager==null) {
					throw new Exception(type.Name+" was built without its LayoutManagerForms.");
				}
				ApplyOption(builtWindow.Form,option);
				//The window's content: PanelClient, which LayoutManager puts every designer control in. The frame and title bar are UI-3's and are
				//drawn by FormODBase.OnLoad's handlers, which do not run here.
				FormODBase formODBase=(FormODBase)builtWindow.Form;
				return new Shot { Name=type.Name,Bitmap=Capture(formODBase.PanelClient,Rectangle.Empty) };
			}
		}

		///<summary>The Account module as the builder makes it, its toolbar filled with the module's captions and its account grid with made-up rows:
		///the whole module, and its toolbar with the grid.</summary>
		private static List<Shot> RenderAccount(SheetWindowOption option,List<string> listNotes) {
			ScanType scanType=new ScanType { Type=typeof(ControlAccount),Kind=BuildKind.UserControlAdd,Assembly="OpenPmsDental",WindowKey=typeof(ControlAccount).FullName };
			using(BuiltWindow builtWindow=WindowBuilder.Build(scanType)) {
				ToolBarOD toolBarOD=(ToolBarOD)GetField(builtWindow.Root,"ToolBarMain");
				GridOD gridAccount=(GridOD)GetField(builtWindow.Root,"gridAccount");
				FillToolBar(toolBarOD);
				ApplyOption(builtWindow.Form,option);
				CreateHandles(builtWindow.Form);
				FillGrid(gridAccount);
				listNotes.Add("ControlAccount's account grid in "+(option.IsToday ? "today's font" : option.Folder)+": rows "+GetFirstRowHeight(gridAccount)
					+" px (GridOD with WrapText on measures the text; without it, the font's height plus 1)");
				List<Shot> listShots=new List<Shot>();
				listShots.Add(new Shot { Name="ControlAccount",Bitmap=Capture(builtWindow.Root,Rectangle.Empty) });
				//The toolbar and the grid: from the top of the module to the grid's bottom, the module's width.
				Rectangle rectangleGrid=GetBoundsIn(gridAccount,builtWindow.Root);
				Rectangle rectangleCrop=Rectangle.FromLTRB(0,0,builtWindow.Root.Width,Math.Min(builtWindow.Root.Height,rectangleGrid.Bottom+2));
				if(rectangleCrop.Height<20) {
					throw new Exception("The account grid is not inside the module: "+rectangleGrid);
				}
				listShots.Add(new Shot { Name="ControlAccount-toolbar-and-grid",Bitmap=Capture(builtWindow.Root,rectangleCrop) });
				return listShots;
			}
		}

		///<summary>The Account module's toolbar buttons (ControlAccount.LayoutToolBar's captions, pictures and drop-downs), without its code.</summary>
		private static void FillToolBar(ToolBarOD toolBarOD) {
			toolBarOD.Buttons.Clear();
			ODToolBarButton button=new ODToolBarButton("Payment",1,"","Payment");
			button.Style=ODToolBarButtonStyle.DropDownButton;
			toolBarOD.Buttons.Add(button);
			button=new ODToolBarButton("Adjustment",2,"","Adjustment");
			button.Style=ODToolBarButtonStyle.DropDownButton;
			toolBarOD.Buttons.Add(button);
			button=new ODToolBarButton("New Claim",3,"","Insurance");
			button.Style=ODToolBarButtonStyle.DropDownButton;
			toolBarOD.Buttons.Add(button);
			toolBarOD.Buttons.Add(new ODToolBarButton(ODToolBarButtonStyle.Separator));
			button=new ODToolBarButton("Payment Plan",-1,"","PayPlan");
			button.Style=ODToolBarButtonStyle.DropDownButton;
			toolBarOD.Buttons.Add(button);
			toolBarOD.Buttons.Add(new ODToolBarButton("Installment Plan",-1,"","InstallPlan"));
			toolBarOD.Buttons.Add(new ODToolBarButton(ODToolBarButtonStyle.Separator));
			button=new ODToolBarButton("Quick Procs",-1,"","QuickProcs");
			button.Style=ODToolBarButtonStyle.DropDownButton;
			toolBarOD.Buttons.Add(button);
			toolBarOD.Buttons.Add(new ODToolBarButton("Repeating Charge",-1,"","RepeatCharge"));
			toolBarOD.Buttons.Add(new ODToolBarButton(ODToolBarButtonStyle.Separator));
			button=new ODToolBarButton("Statement",4,"","Statement");
			button.Style=ODToolBarButtonStyle.DropDownButton;
			toolBarOD.Buttons.Add(button);
			toolBarOD.Invalidate();
		}

		///<summary>Made-up account rows: dates, codes, descriptions and amounts of a synthetic patient.</summary>
		private static void FillGrid(GridOD gridOD) {
			gridOD.BeginUpdate();
			gridOD.Columns.Clear();
			gridOD.Columns.Add(new GridColumn("Date",76));
			gridOD.Columns.Add(new GridColumn("Patient",100));
			gridOD.Columns.Add(new GridColumn("Code",50));
			gridOD.Columns.Add(new GridColumn("Tth",30));
			gridOD.Columns.Add(new GridColumn("Description",222));
			gridOD.Columns.Add(new GridColumn("Charges",64,HorizontalAlignment.Right));
			gridOD.Columns.Add(new GridColumn("Credits",64,HorizontalAlignment.Right));
			gridOD.Columns.Add(new GridColumn("Balance",64,HorizontalAlignment.Right));
			gridOD.ListGridRows.Clear();
			string[][] arrayRows=new string[][] {
				new string[] { "09/08/2026","Alex Sample","D0150","","Comprehensive oral evaluation","95.00","","95.00" },
				new string[] { "09/08/2026","Alex Sample","D0210","","Intraoral, complete series","150.00","","245.00" },
				new string[] { "09/08/2026","Alex Sample","D1110","","Prophylaxis, adult","98.00","","343.00" },
				new string[] { "09/08/2026","Alex Sample","","","Insurance estimate","","210.00","133.00" },
				new string[] { "09/22/2026","Alex Sample","D2392","14","Resin composite, two surfaces, posterior","185.00","","318.00" },
				new string[] { "09/22/2026","Alex Sample","","","Patient payment, card","","100.00","218.00" },
				new string[] { "10/14/2026","Sam Sample","D0120","","Periodic oral evaluation","45.00","","263.00" },
				new string[] { "10/14/2026","Sam Sample","D1120","","Prophylaxis, child","72.00","","335.00" },
				new string[] { "10/14/2026","Sam Sample","D1206","","Fluoride varnish","38.00","","373.00" },
				new string[] { "11/02/2026","Alex Sample","","","Insurance payment","","148.00","225.00" },
				new string[] { "11/09/2026","Alex Sample","D0220","3","Periapical, first film","28.00","","253.00" },
				new string[] { "11/09/2026","Alex Sample","D3330","3","Endodontic therapy, molar","1,050.00","","1,303.00" } };
			foreach(string[] arrayCells in arrayRows) {
				gridOD.ListGridRows.Add(new GridRow(arrayCells));
			}
			gridOD.EndUpdate();
		}

		///<summary>Today's font: nothing to do. An option: UI-0b's font swap (the rule of section 3.4, the 0.96 factor), then each grid's cell, header
		///and title fonts (GridOD draws with its own, section 5.1), so its rows follow the option's font (UQ-2).</summary>
		private static void ApplyOption(Form form,SheetWindowOption option) {
			if(option.IsToday) {
				return;
			}
			TextFitFontSwap.Swap(form,option.FontOption);
			foreach(GridOD gridOD in GetAll<GridOD>(form)) {
				SetGridFonts(gridOD,option.GridFamily,option.GridSize);
			}
		}

		///<summary>Sets GridOD's screen fonts as UI-2a and UI-2b will (FontGrid with its styles, FontTitle), and computes the rows again.</summary>
		public static void SetGridFonts(GridOD gridOD,string family,float size) {
			SetField(gridOD,"_fontCell",new Font(family,size));
			SetField(gridOD,"_fontCellBold",new Font(family,size,FontStyle.Bold));
			SetField(gridOD,"_fontUnderline",new Font(family,size,FontStyle.Underline));
			SetField(gridOD,"_fontUnderlineBold",new Font(family,size,FontStyle.Underline|FontStyle.Bold));
			SetField(gridOD,"_fontHeader",new Font(family,size,FontStyle.Bold));
			SetField(gridOD,"_fontTitle",new Font(UiTheme.FONT_FAMILY_SEMIBOLD,UiTheme.FONT_SIZE_TITLE));
			gridOD.BeginUpdate();
			gridOD.EndUpdate();
		}

		///<summary>The row height GridOD gives its first row now (its HeightMain), or 0 without rows.</summary>
		public static int GetFirstRowHeight(GridOD gridOD) {
			if(gridOD.ListGridRows.Count==0) {
				return 0;
			}
			return gridOD.ListGridRows[0].State.HeightMain;
		}

		///<summary>The control's bounds in an ancestor's client coordinates, from the parents' locations (no window handle needed).</summary>
		public static Rectangle GetBoundsIn(Control control,Control ancestor) {
			Rectangle rectangle=control.Bounds;
			for(Control parent=control.Parent;parent!=null && parent!=ancestor;parent=parent.Parent) {
				rectangle.Offset(parent.Left,parent.Top);
			}
			return rectangle;
		}

		private static void SetField(object obj,string name,object value) {
			FieldInfo fieldInfo=obj.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic);
			if(fieldInfo==null) {
				throw new Exception(obj.GetType().Name+" has no field "+name+".");
			}
			(fieldInfo.GetValue(obj) as IDisposable)?.Dispose();
			fieldInfo.SetValue(obj,value);
		}

		private static object GetField(object obj,string name) {
			FieldInfo fieldInfo=obj.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public);
			if(fieldInfo==null) {
				throw new Exception(obj.GetType().Name+" has no field "+name+".");
			}
			return fieldInfo.GetValue(obj);
		}

		public static List<T> GetAll<T>(Control control) where T : Control {
			List<T> list=new List<T>();
			foreach(Control child in control.Controls) {
				if(child is T t) {
					list.Add(t);
				}
				list.AddRange(GetAll<T>(child));
			}
			return list;
		}

		///<summary>Creates the window handles from the top down: hidden windows (the form is never shown, so nothing appears on the desktop), which
		///WM_PRINT can draw. Load does not run: that needs the form to be made visible.</summary>
		public static void CreateHandles(Control control) {
			IntPtr handle=control.Handle;
			foreach(Control child in control.Controls) {
				CreateHandles(child);
			}
		}

		///<summary>Draws the control and everything in it off screen, all of it or the crop, into a 24-bit picture. Each control draws itself
		///(WM_PRINT, as DrawToBitmap does) and its visible children are drawn over it, back to front, clipped to its client area: Windows prints
		///the children of a window only while the window is visible, and these windows are never shown.</summary>
		public static Bitmap Capture(Control control,Rectangle rectangleCrop) {
			CreateHandles(control);
			using(Bitmap bitmapAll=new Bitmap(control.Width,control.Height,PixelFormat.Format24bppRgb)) {
				using(Graphics g=Graphics.FromImage(bitmapAll)) {
					g.Clear(SystemColors.Control);
					DrawTree(g,control,Point.Empty);
				}
				if(rectangleCrop.IsEmpty) {
					return bitmapAll.Clone(new Rectangle(0,0,bitmapAll.Width,bitmapAll.Height),PixelFormat.Format24bppRgb);
				}
				rectangleCrop.Intersect(new Rectangle(0,0,bitmapAll.Width,bitmapAll.Height));
				return bitmapAll.Clone(rectangleCrop,PixelFormat.Format24bppRgb);
			}
		}

		internal static void DrawTree(Graphics g,Control control,Point pointOffset) {
			if(control.Width<=0 || control.Height<=0) {
				return;
			}
			using(Bitmap bitmap=new Bitmap(control.Width,control.Height,PixelFormat.Format24bppRgb)) {
				control.DrawToBitmap(bitmap,new Rectangle(0,0,control.Width,control.Height));
				g.DrawImage(bitmap,new Rectangle(pointOffset,control.Size));
			}
			Point pointClient=GetClientOrigin(control);
			if(control is TextBoxBase textBoxBase && !(control is RichTextBox)) {
				DrawEdit(g,textBoxBase,new Rectangle(pointOffset,control.Size),
					new Rectangle(pointOffset.X+pointClient.X,pointOffset.Y+pointClient.Y,control.ClientSize.Width,control.ClientSize.Height));
			}
			System.Drawing.Drawing2D.GraphicsState graphicsState=g.Save();
			g.IntersectClip(new Rectangle(pointOffset.X+pointClient.X,pointOffset.Y+pointClient.Y,control.ClientSize.Width,control.ClientSize.Height));
			//Controls[0] is on top: draw from the last to the first.
			for(int i=control.Controls.Count-1;i>=0;i--) {
				Control child=control.Controls[i];
				if(!TextFitBoundsRecord.IsDesignedVisible(child)) {
					continue;
				}
				if(control is System.Windows.Forms.TabControl tabControl && child!=tabControl.SelectedTab) {
					continue;
				}
				DrawTree(g,child,new Point(pointOffset.X+pointClient.X+child.Left,pointOffset.Y+pointClient.Y+child.Top));
			}
			g.Restore(graphicsState);
		}

		///<summary>The grey of the Windows 10 and 11 text box border.</summary>
		private static readonly Color _colorEditBorder=Color.FromArgb(122,122,122);

		///<summary>A text box as Windows paints it: its border, its background and its text from the left. The Windows edit control of a window that
		///was never shown prints nothing usable (WM_PRINT leaves it black on .NET 10), so it is painted here, in the box's own font, which is what
		///the font options change. The same in today's picture and the options', so the comparison is fair.</summary>
		private static void DrawEdit(Graphics g,TextBoxBase textBoxBase,Rectangle rectangleWindow,Rectangle rectangleClient) {
			Color colorBack=textBoxBase.Enabled ? textBoxBase.BackColor : SystemColors.Control;
			using(SolidBrush brush=new SolidBrush(colorBack)) {
				g.FillRectangle(brush,rectangleWindow);
			}
			if(textBoxBase.BorderStyle!=BorderStyle.None) {
				using(Pen pen=new Pen(textBoxBase.BorderStyle==BorderStyle.FixedSingle ? SystemColors.WindowFrame : _colorEditBorder)) {
					g.DrawRectangle(pen,rectangleWindow.X,rectangleWindow.Y,rectangleWindow.Width-1,rectangleWindow.Height-1);
				}
			}
			string text=textBoxBase.Text;
			if(textBoxBase is TextBox textBox && (textBox.PasswordChar!='\0' || textBox.UseSystemPasswordChar)) {
				text=new string('*',text.Length);
			}
			if(string.IsNullOrEmpty(text)) {
				return;
			}
			TextFormatFlags textFormatFlags=TextFormatFlags.TextBoxControl|TextFormatFlags.NoPrefix|TextFormatFlags.Left
				|(textBoxBase.Multiline ? TextFormatFlags.WordBreak : TextFormatFlags.SingleLine);
			Rectangle rectangleText=new Rectangle(rectangleClient.X+1,rectangleClient.Y+1,rectangleClient.Width-2,rectangleClient.Height-1);
			TextRenderer.DrawText(g,text,textBoxBase.Font,rectangleText,textBoxBase.Enabled ? textBoxBase.ForeColor : SystemColors.GrayText,colorBack,textFormatFlags);
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct RECT {
			public int Left;
			public int Top;
			public int Right;
			public int Bottom;
		}

		[DllImport("user32.dll")]
		private static extern bool GetWindowRect(IntPtr hWnd,out RECT rect);

		///<summary>Where the control's client area starts inside its window (its border and title before it), from the window's handle.</summary>
		private static Point GetClientOrigin(Control control) {
			RECT rect;
			if(!GetWindowRect(control.Handle,out rect)) {
				return Point.Empty;
			}
			Point pointClient=control.PointToScreen(Point.Empty);
			return new Point(pointClient.X-rect.Left,pointClient.Y-rect.Top);
		}

		///<summary>True when GDI text (TextRenderer, what stock controls draw with) comes out smoothed in this session: dark text on white then has
		///more than two colours.</summary>
		public static bool IsGdiTextSmoothed() {
			using(Bitmap bitmap=new Bitmap(80,30,PixelFormat.Format24bppRgb))
			using(Graphics g=Graphics.FromImage(bitmap))
			using(Font font=new Font(UiTheme.FONT_FAMILY,UiTheme.FONT_SIZE_BODY)) {
				g.Clear(Color.White);
				TextRenderer.DrawText(g,"Agw",font,new Point(4,4),Color.Black);
				HashSet<int> hashSet=new HashSet<int>();
				for(int y=0;y<bitmap.Height;y++) {
					for(int x=0;x<bitmap.Width;x++) {
						hashSet.Add(bitmap.GetPixel(x,y).ToArgb());
					}
				}
				return hashSet.Count>2;
			}
		}

		///<summary>The number of opaque pixels darker than mid grey (text, outlines), sampled every second pixel. A transparent picture (WPF that
		///drew nothing) has none.</summary>
		public static int CountDarkPixels(Bitmap bitmap) {
			int count=0;
			for(int y=0;y<bitmap.Height;y+=2) {
				for(int x=0;x<bitmap.Width;x+=2) {
					Color color=bitmap.GetPixel(x,y);
					if(color.A>=128 && color.R+color.G+color.B<3*128) {
						count++;
					}
				}
			}
			return count;
		}

		///<summary>The number of distinct colours in a picture, up to max (a blank picture has 1).</summary>
		public static int CountColors(Bitmap bitmap,int max) {
			HashSet<int> hashSet=new HashSet<int>();
			for(int y=0;y<bitmap.Height;y+=3) {
				for(int x=0;x<bitmap.Width;x+=3) {
					hashSet.Add(bitmap.GetPixel(x,y).ToArgb());
					if(hashSet.Count>=max) {
						return hashSet.Count;
					}
				}
			}
			return hashSet.Count;
		}

		#region WPF
		///<summary>Draws FrmProviderPick without a window (Measure, Arrange, RenderTargetBitmap) into after\windows-wpf. WPF's software renderer draws
		///into the bitmap; nothing is shown. Returns the file, or null with the reason in listNotes when this session cannot render WPF.</summary>
		public static string RenderFrm(string folderAfter,List<string> listNotes) {
			FrmProviderPick frmProviderPick=new FrmProviderPick();
			double width=frmProviderPick.Width;
			double height=frmProviderPick.Height;
			//A hidden HwndSource (WS_POPUP without WS_VISIBLE: never shown) gives the Frm a presentation source, so WPF raises Loaded and the
			//controls build their content. The Frm's own Load (FrmODBase.Load, which reads the database) is raised by FormFrame and does not run.
			System.Windows.Interop.HwndSourceParameters hwndSourceParameters=new System.Windows.Interop.HwndSourceParameters("DesignSheetFrm",
				(int)Math.Ceiling(width),(int)Math.Ceiling(height));
			hwndSourceParameters.WindowStyle=unchecked((int)0x80000000);
			System.Windows.Interop.HwndSource hwndSource=new System.Windows.Interop.HwndSource(hwndSourceParameters);
			try {
				hwndSource.RootVisual=frmProviderPick;
				frmProviderPick.Measure(new System.Windows.Size(width,height));
				frmProviderPick.Arrange(new System.Windows.Rect(0,0,width,height));
				frmProviderPick.UpdateLayout();
				//Let the dispatcher run the Loaded handlers and the layout they cause.
				System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(new Action(() => { }),System.Windows.Threading.DispatcherPriority.ContextIdle);
				frmProviderPick.UpdateLayout();
				return RenderVisual(frmProviderPick,width,height,folderAfter,listNotes);
			}
			finally {
				hwndSource.RootVisual=null;
				hwndSource.Dispose();
			}
		}

		private static string RenderVisual(System.Windows.Media.Visual visual,double width,double height,string folderAfter,List<string> listNotes) {
			FrmProviderPick frmProviderPick=(FrmProviderPick)visual;
			System.Windows.Media.Imaging.RenderTargetBitmap renderTargetBitmap=new System.Windows.Media.Imaging.RenderTargetBitmap((int)Math.Ceiling(width),
				(int)Math.Ceiling(height),96,96,System.Windows.Media.PixelFormats.Pbgra32);
			//A white background under the Frm, as its FormFrame gives it.
			System.Windows.Media.DrawingVisual drawingVisual=new System.Windows.Media.DrawingVisual();
			using(System.Windows.Media.DrawingContext drawingContext=drawingVisual.RenderOpen()) {
				drawingContext.DrawRectangle(System.Windows.Media.Brushes.White,null,new System.Windows.Rect(0,0,width,height));
			}
			renderTargetBitmap.Render(drawingVisual);
			renderTargetBitmap.Render(frmProviderPick);
			string folder=Path.Combine(folderAfter,FOLDER_WPF);
			Directory.CreateDirectory(folder);
			string path=Path.Combine(folder,"FrmProviderPick.png");
			System.Windows.Media.Imaging.PngBitmapEncoder pngBitmapEncoder=new System.Windows.Media.Imaging.PngBitmapEncoder();
			pngBitmapEncoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(renderTargetBitmap));
			using(FileStream fileStream=File.Create(path)) {
				pngBitmapEncoder.Save(fileStream);
			}
			bool isBlank;
			using(Bitmap bitmap=new Bitmap(path)) {
				isBlank=CountDarkPixels(bitmap)<200;
			}
			if(isBlank) {
				//WPF draws nothing in a disconnected or locked session (section 8 of the plan), not even into a RenderTargetBitmap.
				File.Delete(path);
				listNotes.Add("FrmProviderPick came out blank: WPF draws nothing in this session (disconnected or locked), not even off screen. The WPF "
					+"window waits for the next connected session (run this test again there).");
				return null;
			}
			return path;
		}
		#endregion WPF
	}
}
