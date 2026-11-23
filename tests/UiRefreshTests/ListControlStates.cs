// The real controls of package UI-4b-1 (combo boxes, pickers and lists; docs/port-notes/UI-4b.md) put in each state off screen (ListStateControls),
// and the design sheet's drawing of each state at a control's own bounds (ListStateDrawings: SheetPainter's shapes with the light tokens, as
// DesignSheetControls draws the combo box and list item families, without the text, with the Fluent chevron of UI-7a in place of the sheet's
// stand-in). Used by ListControlStateTests and by the G-UI1 part 2b state strips (ListControlGallery). No window is shown.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CodeBase;

namespace UiRefreshTests {
	///<summary>The WinForms combo box with its focus line forced (a hidden window cannot take focus) and hover through its own mouse handler.</summary>
	public class StateComboBox : OpenPmsDental.UI.ComboBox {
		public bool IsFocusForced;

		protected override bool IsFocusRingShown {
			get { return IsFocusForced; }
		}

		public void Hover() {
			OnMouseMove(new MouseEventArgs(MouseButtons.None,0,5,5,0));
		}
	}

	///<summary>The WinForms clinic picker with its focus line forced and hover through its own mouse handler (right of the label).</summary>
	public class StateClinicPicker : OpenPmsDental.UI.ComboBoxClinicPicker {
		public bool IsFocusForced;

		protected override bool IsFocusRingShown {
			get { return IsFocusForced; }
		}

		public void Hover() {
			OnMouseMove(new MouseEventArgs(MouseButtons.None,0,Width-5,5,0));
		}
	}

	///<summary>The WinForms list box with focus forced: the selection bar's focus (IsFocusForced) and the focused row's outline (IsFocusRingForced).</summary>
	public class StateListBox : OpenPmsDental.UI.ListBox {
		public bool IsFocusForced;
		public bool IsFocusRingForced;

		protected override bool HasFocusForSelection {
			get { return IsFocusForced; }
		}

		protected override bool IsFocusRingShown {
			get { return IsFocusRingForced; }
		}

		public void HoverItem(int index,int heightLine) {
			OnMouseMove(new MouseEventArgs(MouseButtons.None,0,10,3+index*heightLine+heightLine/2,0));
		}
	}

	///<summary>The WinForms drop-down list of the combo boxes, painted into a bitmap by its own Paint handler (it is never shown).</summary>
	public class StateFormComboPicker : OpenPmsDental.UI.FormComboPicker {
		public bool IsFocusForced;

		protected override bool HasFocusForSelection {
			get { return IsFocusForced; }
		}
	}

	///<summary>The states of a list item in the strips and tests: rest, hover, focused (the focused row's outline), disabled, selected with and without
	///focus.</summary>
	public static class ListStates {
		public static readonly SheetState[] ArrayComboStates=new SheetState[] { SheetState.Normal,SheetState.Hover,SheetState.Focused,SheetState.Disabled };
		public static readonly SheetState[] ArrayListStates=new SheetState[] { SheetState.Normal,SheetState.Hover,SheetState.Focused,SheetState.Disabled,
			SheetState.Selected,SheetState.SelectedNoFocus };
		///<summary>The states a drop-down list shows (it has focus while it is open; the no-focus bar is shown too).</summary>
		public static readonly SheetState[] ArrayDropDownStates=new SheetState[] { SheetState.Normal,SheetState.Hover,SheetState.Selected,SheetState.SelectedNoFocus };
		///<summary>The states a WPF list can be put in off screen (hover in a tree is the mouse's own: shown at a connected desktop only).</summary>
		public static readonly SheetState[] ArrayWpfListStates=new SheetState[] { SheetState.Normal,SheetState.Hover,SheetState.Disabled,SheetState.Selected,
			SheetState.SelectedNoFocus };
	}

	///<summary>The row positions a list picture was drawn with, in device pixels from the control's top left corner.</summary>
	public class ListPicture : IDisposable {
		public Bitmap Bitmap;
		public List<Rectangle> ListItems=new List<Rectangle>();

		public void Dispose() {
			Bitmap?.Dispose();
		}
	}

	public static class ListStateControls {
		public static readonly string[] ArrayItemTexts=new string[] { "Recall","Exam","Hygiene" };

		#region Reflection
		public static void SetPrivate(object target,string field,object value) {
			FieldInfo fieldInfo=FindField(target.GetType(),field);
			fieldInfo.SetValue(target,value);
		}

		public static T GetPrivate<T>(object target,string field) {
			return (T)FindField(target.GetType(),field).GetValue(target);
		}

		public static void CallPrivate(object target,string method,params object[] arrayArgs) {
			Type type=target.GetType();
			while(type!=null) {
				MethodInfo methodInfo=type.GetMethods(BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public|BindingFlags.DeclaredOnly)
					.FirstOrDefault(x => x.Name==method && x.GetParameters().Length==arrayArgs.Length);
				if(methodInfo!=null) {
					methodInfo.Invoke(target,arrayArgs);
					return;
				}
				type=type.BaseType;
			}
			throw new MissingMethodException(target.GetType().Name,method);
		}

		private static FieldInfo FindField(Type type,string field) {
			while(type!=null) {
				FieldInfo fieldInfo=type.GetField(field,BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public|BindingFlags.DeclaredOnly);
				if(fieldInfo!=null) {
					return fieldInfo;
				}
				type=type.BaseType;
			}
			throw new MissingFieldException(field);
		}
		#endregion Reflection

		#region WinForms
		///<summary>A hidden form (never shown) with a panel in the Surface colour, the parent of the control under test.</summary>
		private static Form MakeHost(out System.Windows.Forms.Panel panel) {
			Form form=new Form();
			form.ShowInTaskbar=false;
			form.Size=new Size(700,500);
			panel=new System.Windows.Forms.Panel();
			panel.BackColor=SheetPainter.Light(EnumUiColor.Surface);
			panel.Bounds=new Rectangle(0,0,680,460);
			form.Controls.Add(panel);
			return form;
		}

		private static Bitmap Draw(Control control) {
			Bitmap bitmap=new Bitmap(control.Width,control.Height,PixelFormat.Format32bppArgb);
			control.DrawToBitmap(bitmap,new Rectangle(0,0,control.Width,control.Height));
			return bitmap;
		}

		private static Font ScaleFont(Font font,float scale) {
			return new Font(font.FontFamily,font.Size*scale,font.Style);
		}

		///<summary>UI.ComboBox (121 x 21 at 96 DPI) with one item, selected, of the text, in the state.</summary>
		public static Bitmap RenderWinFormsComboBox(SheetState sheetState,string text,float scale) {
			System.Windows.Forms.Panel panel;
			using(Form form=MakeHost(out panel)) {
				StateComboBox comboBox=new StateComboBox();
				comboBox.LayoutManager.SetScaleMS(scale);
				comboBox.Font=ScaleFont(comboBox.Font,scale);
				comboBox.Bounds=new Rectangle(10,10,UiTheme.ScaleI(121,scale),UiTheme.ScaleI(21,scale));
				comboBox.Items.Add(text);
				comboBox.SelectedIndex=0;
				panel.Controls.Add(comboBox);
				IntPtr handle=comboBox.Handle;
				switch(sheetState) {
					case SheetState.Hover:
						comboBox.Hover();
						break;
					case SheetState.Focused:
						comboBox.IsFocusForced=true;
						break;
					case SheetState.Disabled:
						comboBox.Enabled=false;
						break;
				}
				return Draw(comboBox);
			}
		}

		///<summary>ComboBoxClinicPicker (200 x 21 at 96 DPI, its label shown, test mode: no database) in the state. label: the label's text ("" for the
		///chrome: the label area of "Clinic", left empty). Returns the box's rectangle (right of the label area).</summary>
		public static Bitmap RenderWinFormsClinicPicker(SheetState sheetState,string label,float scale,out Rectangle rectangleBox) {
			System.Windows.Forms.Panel panel;
			using(Form form=MakeHost(out panel)) {
				StateClinicPicker clinicPicker=new StateClinicPicker();
				clinicPicker.IsTestModeNoDb=true;
				clinicPicker.LayoutManager.SetScaleMS(scale);
				clinicPicker.Font=ScaleFont(clinicPicker.Font,scale);
				clinicPicker.Bounds=new Rectangle(10,10,UiTheme.ScaleI(200,scale),UiTheme.ScaleI(21,scale));
				panel.Controls.Add(clinicPicker);
				IntPtr handle=clinicPicker.Handle;
				//UI-12 slice 1: the label area is sized from the label's text (WidthLabelArea), so the chrome is drawn with the label of the text picture
				//and its label area cleared: both pictures have the box at one x.
				GetPrivate<System.Windows.Forms.Label>(clinicPicker,"labelFake").Text=label=="" ? "Clinic" : label;
				switch(sheetState) {
					case SheetState.Hover:
						clinicPicker.Hover();
						break;
					case SheetState.Focused:
						clinicPicker.IsFocusForced=true;
						break;
					case SheetState.Disabled:
						clinicPicker.Enabled=false;
						break;
				}
				int xBox=clinicPicker.WidthLabelArea;
				rectangleBox=new Rectangle(xBox,0,clinicPicker.Width-xBox,clinicPicker.Height);
				Bitmap bitmap=Draw(clinicPicker);
				if(label=="") {
					using(Graphics g=Graphics.FromImage(bitmap))
					using(SolidBrush solidBrush=new SolidBrush(bitmap.GetPixel(0,0))) {
						g.FillRectangle(solidBrush,0,0,xBox,bitmap.Height);
					}
				}
				return bitmap;
			}
		}

		///<summary>UI.ListBox (120 x 60 at 96 DPI) with the three items (texts: ArrayItemTexts, or "" each for the chrome) in the state: hover and the
		///selection on the first item; Focused is the focused row's outline (focus and keyboard cues, nothing selected).</summary>
		public static ListPicture RenderWinFormsListBox(SheetState sheetState,bool withText,float scale) {
			System.Windows.Forms.Panel panel;
			using(Form form=MakeHost(out panel)) {
				StateListBox listBox=new StateListBox();
				listBox.LayoutManager.SetScaleMS(scale);
				listBox.Font=ScaleFont(listBox.Font,scale);
				listBox.Bounds=new Rectangle(10,10,UiTheme.ScaleI(120,scale),UiTheme.ScaleI(60,scale));
				foreach(string text in ArrayItemTexts) {
					listBox.Items.Add(withText ? text : "");
				}
				panel.Controls.Add(listBox);
				IntPtr handle=listBox.Handle;
				ListPicture listPicture=new ListPicture();
				//The row height comes from the font in the first paint.
				using(Bitmap bitmapFirst=Draw(listBox)) {
				}
				int heightLine=(int)GetPrivate<float>(listBox,"_heightLine");
				switch(sheetState) {
					case SheetState.Hover:
						listBox.HoverItem(0,heightLine);
						break;
					case SheetState.Focused:
						listBox.IsFocusForced=true;
						listBox.IsFocusRingForced=true;
						break;
					case SheetState.Disabled:
						listBox.Enabled=false;
						break;
					case SheetState.Selected:
						listBox.SetSelected(0);
						listBox.IsFocusForced=true;
						break;
					case SheetState.SelectedNoFocus:
						listBox.SetSelected(0);
						break;
				}
				listPicture.Bitmap=Draw(listBox);
				for(int i=0;i<ArrayItemTexts.Length;i++) {
					listPicture.ListItems.Add(new Rectangle(2,i*heightLine+2,listBox.Width-4,heightLine));
				}
				return listPicture;
			}
		}

		///<summary>The combo boxes' drop-down (FormComboPicker) with its "combobox" on top (HeightCombo 21 at 96 DPI) and the three items, painted by its
		///own Paint handler into a bitmap 121 px wide at 96 DPI. Hover and selection on the first item.</summary>
		public static ListPicture RenderWinFormsDropDown(SheetState sheetState,bool withText,float scale) {
			using(StateFormComboPicker formComboPicker=new StateFormComboPicker()) {
				formComboPicker.LayoutManager.SetScaleMS(scale);
				formComboPicker.Font=ScaleFont(formComboPicker.Font,scale);
				//FormComboPicker_Load takes the row height from the font the combo box passes in (here already at the scale).
				int heightLine=formComboPicker.Font.Height;
				formComboPicker.ListStrings=ArrayItemTexts.Select(x => withText ? x : "").ToList();
				formComboPicker.ListAbbrevs=formComboPicker.ListStrings;
				formComboPicker.HeightCombo=UiTheme.ScaleI(21,scale);
				SetPrivate(formComboPicker,"_heightLineItem",heightLine);
				SetPrivate(formComboPicker,"_isOriginalLocation",true);
				int width=UiTheme.ScaleI(121,scale);
				int height=formComboPicker.ListStrings.Count*heightLine+4+formComboPicker.HeightCombo;
				formComboPicker.Size=new Size(width,height);
				switch(sheetState) {
					case SheetState.Hover:
						SetPrivate(formComboPicker,"_hoverIndex",0);
						break;
					case SheetState.Selected:
						formComboPicker.SelectedIndex=0;
						formComboPicker.IsFocusForced=true;
						break;
					case SheetState.SelectedNoFocus:
						formComboPicker.SelectedIndex=0;
						break;
				}
				ListPicture listPicture=new ListPicture();
				listPicture.Bitmap=new Bitmap(width,height,PixelFormat.Format32bppArgb);
				using(Graphics g=Graphics.FromImage(listPicture.Bitmap))
				using(PaintEventArgs paintEventArgs=new PaintEventArgs(g,new Rectangle(0,0,width,height))) {
					CallPrivate(formComboPicker,"FormComboPicker_Paint",formComboPicker,paintEventArgs);
				}
				for(int i=0;i<formComboPicker.ListStrings.Count;i++) {
					listPicture.ListItems.Add(new Rectangle(1,i*heightLine+2+formComboPicker.HeightCombo,width-3,heightLine));
				}
				return listPicture;
			}
		}
		#endregion WinForms

		#region WPF
		///<summary>The control with the combo box's focus visual style (the Accent line inside the bottom edge) laid over it at its bounds, as WPF's
		///adorner draws it while the control has keyboard focus.</summary>
		private static System.Windows.FrameworkElement WithComboFocusVisual(System.Windows.FrameworkElement frameworkElement,double width,double height) {
			System.Windows.Controls.Grid grid=new System.Windows.Controls.Grid();
			grid.Width=width;
			grid.Height=height;
			grid.Children.Add(frameworkElement);
			System.Windows.Controls.Control controlFocus=new System.Windows.Controls.Control();
			controlFocus.Style=(System.Windows.Style)frameworkElement.FindResource(WpfControls.UI.UiLookLists.KeyFocusVisualStyleCombo);
			controlFocus.Width=width;
			controlFocus.Height=height;
			grid.Children.Add(controlFocus);
			return grid;
		}

		public static Bitmap RenderWpfComboBox(SheetState sheetState,string text,float scale) {
			WpfControls.UI.ComboBox comboBox=new WpfControls.UI.ComboBox();
			comboBox.Width=121;
			comboBox.Height=21;
			comboBox.Items.Add(text);
			comboBox.SelectedIndex=0;
			if(sheetState==SheetState.Disabled) {
				comboBox.IsEnabled=false;
			}
			if(sheetState==SheetState.Hover) {
				SetPrivate(comboBox,"_isHover",true);
				CallPrivate(comboBox,"SetColors");
			}
			System.Windows.FrameworkElement frameworkElement=comboBox;
			if(sheetState==SheetState.Focused) {
				frameworkElement=WithComboFocusVisual(comboBox,121,21);
			}
			return ControlPictures.RenderWpf(frameworkElement,121,21,scale);
		}

		///<summary>ComboClinic (200 x 21) in the state; label: its text ("" for the chrome). Returns the combo box's rectangle in device pixels.</summary>
		public static Bitmap RenderWpfComboClinic(SheetState sheetState,string label,float scale,out Rectangle rectangleBox) {
			WpfControls.UI.ComboClinic comboClinic=new WpfControls.UI.ComboClinic();
			comboClinic.Width=200;
			comboClinic.Height=21;
			GetPrivate<System.Windows.Controls.TextBlock>(comboClinic,"textBlock").Text=label;
			WpfControls.UI.ComboBox comboBox=GetPrivate<WpfControls.UI.ComboBox>(comboClinic,"comboBox");
			if(sheetState==SheetState.Disabled) {
				comboClinic.IsEnabled=false;
			}
			if(sheetState==SheetState.Hover) {
				SetPrivate(comboBox,"_isHover",true);
				CallPrivate(comboBox,"SetColors");
			}
			comboClinic.Measure(new System.Windows.Size(200,21));
			comboClinic.Arrange(new System.Windows.Rect(0,0,200,21));
			comboClinic.UpdateLayout();
			System.Windows.Point point=comboBox.TranslatePoint(new System.Windows.Point(0,0),comboClinic);
			rectangleBox=UiTheme.SnapRectangle(new RectangleF((float)point.X*scale,(float)point.Y*scale,(float)comboBox.ActualWidth*scale,(float)comboBox.ActualHeight*scale));
			return ControlPictures.RenderWpf(comboClinic,200,21,scale);
		}

		///<summary>The WPF ListBox (120 x 60) with the three items (or "" each), hover and selection on the first; the selection's focus is the keyboard
		///focus inside the list, forced through the field the list reads it from.</summary>
		public static ListPicture RenderWpfListBox(SheetState sheetState,bool withText,float scale) {
			WpfControls.UI.ListBox listBox=new WpfControls.UI.ListBox();
			listBox.Width=120;
			listBox.Height=60;
			foreach(string text in ArrayItemTexts) {
				listBox.Items.Add(withText ? text : "");
			}
			if(sheetState==SheetState.Disabled) {
				listBox.IsEnabled=false;
			}
			if(sheetState==SheetState.Hover) {
				SetPrivate(listBox,"_hoverIndex",0);
			}
			if(sheetState==SheetState.Selected || sheetState==SheetState.SelectedNoFocus) {
				listBox.SelectedIndex=0;
			}
			SetPrivate(listBox,"_isFocusForced",sheetState==SheetState.Selected);
			CallPrivate(listBox,"SetColors");
			return RenderWpfList(listBox,GetPrivate<System.Windows.Controls.StackPanel>(listBox,"stackPanel"),120,60,scale);
		}

		///<summary>The WPF ListView (240 x 140) with two thumbnail items (no picture), hover and selection on the first.</summary>
		public static ListPicture RenderWpfListView(SheetState sheetState,bool withText,float scale) {
			WpfControls.UI.ListView listView=new WpfControls.UI.ListView();
			listView.Width=240;
			listView.Height=140;
			listView.AddItem(new WpfControls.UI.ListViewItem { Text=withText ? "Bitewings" : "" });
			listView.AddItem(new WpfControls.UI.ListViewItem { Text=withText ? "Panoramic" : "" });
			if(sheetState==SheetState.Hover) {
				SetPrivate(listView,"_hoverIndex",0);
			}
			if(sheetState==SheetState.Selected || sheetState==SheetState.SelectedNoFocus) {
				SetPrivate(listView,"_selectedIndex",0);
			}
			if(sheetState==SheetState.Disabled) {
				listView.IsEnabled=false;
			}
			SetPrivate(listView,"_isFocusForced",sheetState==SheetState.Selected);
			CallPrivate(listView,"SetColors");
			return RenderWpfList(listView,GetPrivate<System.Windows.Controls.WrapPanel>(listView,"wrapPanel"),240,140,scale);
		}

		///<summary>The WPF TreeView (160 x 60) with three items, the first selected for Selected and SelectedNoFocus; the tree's focus is
		///Selector.IsSelectionActive, which WPF sets on the items while the tree has keyboard focus (set here through its key).</summary>
		public static ListPicture RenderWpfTreeView(SheetState sheetState,bool withText,float scale) {
			WpfControls.UI.TreeView treeView=new WpfControls.UI.TreeView();
			treeView.Width=160;
			treeView.Height=60;
			System.Windows.Controls.TreeView treeViewInner=GetPrivate<System.Windows.Controls.TreeView>(treeView,"treeView");
			List<WpfControls.UI.TreeViewItem> listItems=new List<WpfControls.UI.TreeViewItem>();
			foreach(string text in ArrayItemTexts) {
				WpfControls.UI.TreeViewItem treeViewItem=new WpfControls.UI.TreeViewItem();
				treeViewItem.Text=withText ? text : "";
				treeViewInner.Items.Add(treeViewItem);
				listItems.Add(treeViewItem);
			}
			if(sheetState==SheetState.Disabled) {
				treeView.IsEnabled=false;
			}
			if(sheetState==SheetState.Selected || sheetState==SheetState.SelectedNoFocus) {
				listItems[0].IsSelected=true;
			}
			//Laid out first, so the items have their template (the theme style of Themes/Generic.xaml).
			treeView.Measure(new System.Windows.Size(160,60));
			treeView.Arrange(new System.Windows.Rect(0,0,160,60));
			treeView.UpdateLayout();
			if(sheetState==SheetState.Selected) {
				FieldInfo fieldInfoKey=typeof(System.Windows.Controls.Primitives.Selector).GetField("IsSelectionActivePropertyKey",BindingFlags.Static|BindingFlags.NonPublic);
				listItems[0].SetValue((System.Windows.DependencyPropertyKey)fieldInfoKey.GetValue(null),true);
			}
			if(sheetState==SheetState.Hover) {
				//IsMouseOver cannot be set: the hover look is the template's, the same RowHover as the lists, set on the row here as the trigger does.
				System.Windows.Controls.Border border=(System.Windows.Controls.Border)listItems[0].Template.FindName("Bd",listItems[0]);
				border.Background=WpfControls.UI.UiLook.Brush(EnumUiColor.RowHover);
			}
			treeView.UpdateLayout();
			ListPicture listPicture=new ListPicture();
			listPicture.Bitmap=ControlPictures.RenderWpf(treeView,160,60,scale);
			foreach(WpfControls.UI.TreeViewItem treeViewItem in listItems) {
				System.Windows.FrameworkElement frameworkElementRow=(System.Windows.FrameworkElement)treeViewItem.Template.FindName("Bd",treeViewItem);
				listPicture.ListItems.Add(GetRectangle(frameworkElementRow,treeView,scale));
			}
			return listPicture;
		}

		///<summary>The WPF drop-down of the combo boxes (WindowComboPicker, 121 wide) with the three items, its content rendered off screen (the window is
		///never shown); hover and selection on the first, the focus forced as for the list box.</summary>
		public static ListPicture RenderWpfDropDown(SheetState sheetState,bool withText,float scale) {
			WpfControls.UI.WindowComboPicker windowComboPicker=new WpfControls.UI.WindowComboPicker();
			windowComboPicker.ListStrings=ArrayItemTexts.Select(x => withText ? x : "").ToList();
			windowComboPicker.ListAbbrevs=windowComboPicker.ListStrings;
			CallPrivate(windowComboPicker,"AddItems");
			if(sheetState==SheetState.Hover) {
				SetPrivate(windowComboPicker,"_hoverIndex",0);
			}
			if(sheetState==SheetState.Selected || sheetState==SheetState.SelectedNoFocus) {
				windowComboPicker.SelectedIndex=0;
			}
			SetPrivate(windowComboPicker,"_isFocusForced",sheetState==SheetState.Selected);
			CallPrivate(windowComboPicker,"SetColors");
			System.Windows.FrameworkElement frameworkElementContent=(System.Windows.FrameworkElement)windowComboPicker.Content;
			windowComboPicker.Content=null;
			System.Windows.Controls.StackPanel stackPanel=GetPrivate<System.Windows.Controls.StackPanel>(windowComboPicker,"stackPanel");
			ListPicture listPicture=RenderWpfList(frameworkElementContent,stackPanel,121,60,scale);
			windowComboPicker.Close();
			return listPicture;
		}

		///<summary>Lays the list out, renders it and records its rows (the children of the panel) in device pixels.</summary>
		private static ListPicture RenderWpfList(System.Windows.FrameworkElement frameworkElement,System.Windows.Controls.Panel panelItems,double width,double height,float scale) {
			frameworkElement.Width=width;
			frameworkElement.Height=height;
			frameworkElement.Measure(new System.Windows.Size(width,height));
			frameworkElement.Arrange(new System.Windows.Rect(0,0,width,height));
			frameworkElement.UpdateLayout();
			ListPicture listPicture=new ListPicture();
			listPicture.Bitmap=ControlPictures.RenderWpf(frameworkElement,width,height,scale);
			foreach(System.Windows.FrameworkElement frameworkElementRow in panelItems.Children) {
				listPicture.ListItems.Add(GetRectangle(frameworkElementRow,frameworkElement,scale));
			}
			return listPicture;
		}

		private static Rectangle GetRectangle(System.Windows.FrameworkElement frameworkElement,System.Windows.Media.Visual visualAncestor,float scale) {
			System.Windows.Point point=frameworkElement.TranslatePoint(new System.Windows.Point(0,0),(System.Windows.UIElement)visualAncestor);
			return UiTheme.SnapRectangle(new RectangleF((float)point.X*scale,(float)point.Y*scale,(float)frameworkElement.ActualWidth*scale,
				(float)frameworkElement.ActualHeight*scale));
		}
		#endregion WPF
	}

	///<summary>The design sheet's drawing of a state at a control's own bounds: DesignSheetControls.DrawComboBox and DrawListOrGridRow's shapes and
	///tokens, without the text, with the Fluent chevron of UI-7a.</summary>
	public static class ListStateDrawings {
		private static Bitmap Start(Size size,Color colorBack,float scale,out Graphics g,out SheetPainter p) {
			Bitmap bitmap=new Bitmap(size.Width,size.Height,PixelFormat.Format32bppArgb);
			g=Graphics.FromImage(bitmap);
			g.Clear(colorBack);
			p=new SheetPainter(g,scale,SheetPainter.Light);
			return bitmap;
		}

		public static Color ComboTextColor(SheetState sheetState) {
			return SheetPainter.Light(sheetState==SheetState.Disabled ? EnumUiColor.TextDisabled : EnumUiColor.Text);
		}

		///<summary>DesignSheetControls.DrawComboBox at the box's rectangle in a picture of the size on colorBack (empty: Surface): SurfaceRaised with 4 px
		///corners, the border (StrokeStrongHover on hover, Stroke disabled, StrokeStrong otherwise), the chevron 6 px from the right, the focus line.</summary>
		public static Bitmap ComboBox(Size size,Rectangle rectangleBox,SheetState sheetState,float scale,Color colorBack=default(Color)) {
			Graphics g;
			SheetPainter p;
			Bitmap bitmap=Start(size,colorBack.IsEmpty ? SheetPainter.Light(EnumUiColor.Surface) : colorBack,scale,out g,out p);
			using(g)
			using(p) {
				DrawComboBox(p,rectangleBox,sheetState);
			}
			return bitmap;
		}

		public static void DrawComboBox(SheetPainter p,Rectangle rectangle,SheetState sheetState) {
			bool isDisabled=sheetState==SheetState.Disabled;
			p.Fill(EnumUiColor.SurfaceRaised,rectangle,UiTheme.CORNER_CONTROL);
			EnumUiColor uiColorBorder=sheetState==SheetState.Hover ? EnumUiColor.StrokeStrongHover : (isDisabled ? EnumUiColor.Stroke : EnumUiColor.StrokeStrong);
			p.Outline(uiColorBorder,rectangle,p.LineWidth,UiTheme.CORNER_CONTROL);
			int sizeChevron=p.S(UiTheme.SIZE_CHEVRON);
			Rectangle rectangleChevron=new Rectangle(rectangle.Right-p.S(6)-sizeChevron,rectangle.Y+(rectangle.Height-sizeChevron)/2,sizeChevron,sizeChevron);
			UiIcons.Draw(p.G,UiGlyph.ChevronDown,rectangleChevron,p.C(isDisabled ? EnumUiColor.TextDisabled : EnumUiColor.Text));
			if(sheetState==SheetState.Focused) {
				//Section 3.6: a 2 px Accent line inside the bottom edge.
				int width=p.FocusWidth;
				int inset=p.S(UiTheme.CORNER_CONTROL)/2;
				p.Line(EnumUiColor.Accent,rectangle.X+inset,rectangle.Bottom-width,rectangle.Right-inset,rectangle.Bottom-width,width);
			}
		}

		public static Color ItemTextColor(SheetState sheetState) {
			if(sheetState==SheetState.Disabled) {
				return SheetPainter.Light(EnumUiColor.TextDisabled);
			}
			return SheetPainter.Light(sheetState==SheetState.Selected || sheetState==SheetState.SelectedNoFocus ? EnumUiColor.TextSelected : EnumUiColor.Text);
		}

		///<summary>A list in the state (DesignSheetControls.DrawListOrGridRow's list item at each row): SurfaceRaised; the first row RowHover on hover,
		///AccentSubtle with the 2 px bar (Accent with focus, SelectionBarNoFocus without) when selected, the 1 px Accent outline when focused; the
		///square border (StrokeStrong, Stroke disabled) at rectangleBorder (empty: none). colorBack: the picture's background (the control's own).</summary>
		public static Bitmap List(Size size,List<Rectangle> listItems,Rectangle rectangleBorder,SheetState sheetState,float scale,Color colorBack) {
			Graphics g;
			SheetPainter p;
			Bitmap bitmap=Start(size,colorBack,scale,out g,out p);
			using(g)
			using(p) {
				for(int i=0;i<listItems.Count;i++) {
					DrawItem(p,listItems[i],i==0 ? sheetState : SheetState.Normal);
				}
				if(!rectangleBorder.IsEmpty) {
					p.Outline(sheetState==SheetState.Disabled ? EnumUiColor.Stroke : EnumUiColor.StrokeStrong,rectangleBorder,p.LineWidth,0);
				}
			}
			return bitmap;
		}

		public static void DrawItem(SheetPainter p,Rectangle rectangleRow,SheetState sheetState) {
			EnumUiColor uiColorFill=EnumUiColor.SurfaceRaised;
			if(sheetState==SheetState.Hover) {
				uiColorFill=EnumUiColor.RowHover;
			}
			if(sheetState==SheetState.Selected || sheetState==SheetState.SelectedNoFocus) {
				uiColorFill=EnumUiColor.AccentSubtle;
			}
			p.Fill(uiColorFill,rectangleRow,0);
			if(sheetState==SheetState.Selected || sheetState==SheetState.SelectedNoFocus) {
				//Section 3.2: the 2 px bar on the left edge, Accent with focus, SelectionBarNoFocus without.
				p.Fill(sheetState==SheetState.Selected ? EnumUiColor.Accent : EnumUiColor.SelectionBarNoFocus,new Rectangle(rectangleRow.X,rectangleRow.Y,p.FocusWidth,
					rectangleRow.Height),0);
			}
			if(sheetState==SheetState.Focused) {
				//Section 3.6: the focused row gets a 1 px solid Accent outline inside the row.
				p.Outline(EnumUiColor.Accent,rectangleRow,p.LineWidth,0);
			}
		}

		///<summary>The selection bar's columns in a row: the row's first FocusWidth pixels.</summary>
		public static Rectangle SelectionBar(Rectangle rectangleRow,float scale) {
			return new Rectangle(rectangleRow.X,rectangleRow.Y,UiTheme.GetFocusWidth(scale),rectangleRow.Height);
		}
	}
}
