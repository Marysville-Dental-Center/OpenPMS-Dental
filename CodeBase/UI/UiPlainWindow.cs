// UI-PLAIN (docs/port-notes/UI-PLAIN.md; the owner, 2026-11-22: "Overall make sure you modernize all element types"): the plain windows (a
// System.Windows.Forms.Form that is not a FormODBase: the plain exes' windows, the Central Manager, the MySQL Manager, the database tools, the
// CRUD generator, the HL7 debug window, the service manager, the update file copier, the graph's dashboard dialogs, the CodeBase progress
// windows and message box) in the look FormODBase gives the program's windows: the window background token (UiTheme.Surface, FormODBase's
// BackColor) and every shared look FormODBase attaches to its controls (UiInputFrame.AttachTree: the inputs' frame, the radio buttons, the
// up-down boxes, and UiLooks: scroll bars, sliders, stock buttons, check boxes, group boxes, progress bars, borders, tree and list views and
// the rest), on the controls the window has and on those its code adds later. The fonts are the exes' default font (Segoe UI 9, the body
// token, set in each Main) and the menus the token renderer (UiTheme.InitWinForms, in each Main). Each plain window calls Attach once, in its
// constructor after InitializeComponent. Only pictures change: no size, place, text, event, value or behaviour.
// UI-PLAIN-2 (docs/port-notes/UI-PLAIN.md section 8): Attach also gives the window the program's title bar and frame (UiPlainFrame), which
// adds the title band to the window and moves its content below it when the window loads.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace CodeBase {
	///<summary>Gives a plain window (a Form that is not a FormODBase) the look of the program's windows: the background token and the shared looks
	///of its controls, now and when its code adds controls.</summary>
	public static class UiPlainWindow {
		///<summary>The controls whose added children get the look (ControlAdded is handled once per control).</summary>
		private static readonly ConditionalWeakTable<Control,object> _tableWatched=new ConditionalWeakTable<Control,object>();
		///<summary>The windows Attach was called for.</summary>
		private static readonly ConditionalWeakTable<Form,object> _tableWindows=new ConditionalWeakTable<Form,object>();
		///<summary>The window background of the old look the plain windows' designers name (ColorOD.Background before UI-3: 252,253,254).</summary>
		private static readonly Color _colorBackgroundOld=Color.FromArgb(252,253,254);

		///<summary>The one call of a plain window's constructor, right after InitializeComponent: the window's background in the token, every
		///control in its look, and the controls its code adds later too. Calling it again changes nothing. Does nothing while the gallery harness
		///draws the windows without the looks (UiLooks.IsOffForGallery).</summary>
		public static void Attach(Form form) {
			if(form==null || UiLooks.IsOffForGallery || _tableWindows.TryGetValue(form,out _)) {
				return;
			}
			_tableWindows.Add(form,null);
			SetBackColor(form);
			AttachTree(form);
			Watch(form);
			//UI-PLAIN-2: the program's title bar and frame (UiPlainFrame), from the window's handle on; its content moves below the band at Load.
			UiPlainFrame.Attach(form);
		}

		///<summary>True when Attach was called for the window.</summary>
		public static bool IsAttached(Form form) {
			return form!=null && _tableWindows.TryGetValue(form,out _);
		}

		///<summary>True for a background the window background token replaces: the classic window grey (SystemColors.Control, the plain windows'
		///default) and the old look's window background (252,253,254).</summary>
		public static bool IsOldBackground(Color color) {
			if(color.IsEmpty || color.A<255) {
				return false;
			}
			return color==SystemColors.Control || color.ToArgb()==SystemColors.Control.ToArgb() || color.ToArgb()==_colorBackgroundOld.ToArgb();
		}

		///<summary>True for a window's own background the token replaces: an old background (IsOldBackground) or the light classic grey a window
		///chose (SystemColors.ControlLight: the CodeBase progress windows).</summary>
		public static bool IsOldWindowBackground(Color color) {
			return IsOldBackground(color) || (color.A==255 && color.ToArgb()==SystemColors.ControlLight.ToArgb());
		}

		///<summary>True for a control that shows the window's background (the window, a panel or split container and its panels, a user control, a
		///group box, a label, a check box or radio button), whose old background becomes the token. Not a stock tab page: the stock tab control
		///keeps Windows' themed tabs and pages (UI-13's inventory). Inputs, lists, buttons and the rest keep their colours (their looks use them).
		///</summary>
		public static bool IsBackground(Control control) {
			if(control is TabPage && control.GetType()==typeof(TabPage)) {
				return false;
			}
			return control is Form || control is UserControl || control is Panel || control is SplitContainer || control is GroupBox
				|| control is Label || control is CheckBox || control is RadioButton;
		}

		///<summary>The control and its children: an old background (IsOldBackground; on the window IsOldWindowBackground) on a background control
		///(IsBackground) becomes the window background token. A child that takes its parent's colour reads the new one and stays as it is.</summary>
		private static void SetBackColor(Control control) {
			bool isOld=control is Form ? IsOldWindowBackground(control.BackColor) : IsOldBackground(control.BackColor);
			if(IsBackground(control) && isOld) {
				control.BackColor=UiTheme.Surface;
			}
			foreach(Control child in control.Controls) {
				SetBackColor(child);
			}
		}

		///<summary>Every shared look on every control under root (UiInputFrame.AttachTree, as FormODBase attaches them).</summary>
		private static void AttachTree(Control root) {
			UiInputFrame.AttachTree(root);
		}

		///<summary>Handles ControlAdded on the control and every control under it, once each: an added control and its children get the background
		///and the looks, and are watched in turn (the progress window's bars, the dashboard's panels and cells are added by code).</summary>
		private static void Watch(Control control) {
			if(!_tableWatched.TryGetValue(control,out _)) {
				_tableWatched.Add(control,null);
				control.ControlAdded+=Control_ControlAdded;
			}
			foreach(Control child in control.Controls) {
				Watch(child);
			}
		}

		private static void Control_ControlAdded(object sender,ControlEventArgs e) {
			Control control=e.Control;
			if(control==null || UiLooks.IsOffForGallery) {
				return;
			}
			try {
				SetBackColor(control);
				UiInputFrame.AttachOne(control);
				AttachTree(control);
				Watch(control);
			}
			catch(Exception) {
				//The look is only a picture: the control works as Windows draws it.
			}
		}
	}
}
