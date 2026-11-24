// UI-OVF's WPF focus tests (docs/port-notes/UI-OVF.md; docs/specs/ui-refresh-plan.md section 3.6, "Focus, for every focusable self-drawn
// control"): the WPF Button, CheckBox, RadioButton and toolbar buttons are tab stops like the WinForms ones, with the shared focus visual style
// (WPF draws it only when the focus came from the keyboard). Enter and Space press a focused button and the key is handled, so a Frm's own Enter
// (its bubbling KeyDown) does not run as well; Space toggles a check box; Space and the arrow keys check radio buttons. A Frm that sets no tab
// order of its own tabs in reading order (UiTabOrder). The Frms whose PreviewKeyDown takes Enter (before any focused control) are checked. A real
// Tab walk runs in a window shown off screen (inconclusive when the session gives it no keyboard focus). No database.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UiRefreshTests.Scan;
using UiRefreshTests.Toolbars;

namespace UiRefreshTests {
	[TestClass]
	public class WpfFocusTests {
		///<summary>A presentation source for key events raised in a test (a KeyEventArgs needs one; nothing is shown).</summary>
		private class PresentationSourceTest:PresentationSource {
			private System.Windows.Media.Visual _visualRoot;

			public override System.Windows.Media.Visual RootVisual {
				get {
					return _visualRoot;
				}
				set {
					_visualRoot=value;
				}
			}

			public override bool IsDisposed=>false;

			protected override System.Windows.Media.CompositionTarget GetCompositionTargetCore() {
				return null;
			}
		}

		///<summary>Raises KeyDown for the key on the element (no modifier keys down), as the keyboard does on the focused element. Returns whether
		///it was handled.</summary>
		public static bool RaiseKeyDown(UIElement uIElement,Key key) {
			KeyEventArgs keyEventArgs=new KeyEventArgs(Keyboard.PrimaryDevice,new PresentationSourceTest(),0,key);
			keyEventArgs.RoutedEvent=Keyboard.KeyDownEvent;
			uIElement.RaiseEvent(keyEventArgs);
			return keyEventArgs.Handled;
		}

		[TestMethod]
		public void Controls_AreTabStops_WithTheSharedFocusVisual() {
			IconRendererTests.RunSta(() => {
				NoDatabaseGuard.Install();
				foreach(FrameworkElement frameworkElement in new FrameworkElement[] { new WpfControls.UI.Button(),new WpfControls.UI.CheckBox(),
					new WpfControls.UI.RadioButton(),new WpfControls.UI.ToolBarButton() })
				{
					string name=frameworkElement.GetType().Name;
					Assert.IsTrue(frameworkElement.Focusable,name+" takes the focus");
					if(!(frameworkElement is WpfControls.UI.ToolBarButton)) {
						Assert.IsTrue(KeyboardNavigation.GetIsTabStop(frameworkElement),name+" is a tab stop");
					}
					Assert.IsNotNull(frameworkElement.FocusVisualStyle,name+": the shared focus visual style (shown only with keyboard cues)");
				}
				WpfControls.UI.Button buttonPrimary=new WpfControls.UI.Button();
				buttonPrimary.IsPrimary=true;
				Assert.AreSame(buttonPrimary.FindResource(WpfControls.UI.UiLook.KeyFocusVisualStyleOnAccent),buttonPrimary.FocusVisualStyle,"the ring on the accent");
				//The toolbar is one tab stop; the arrow keys move between its buttons.
				WpfControls.UI.ToolBar toolBar=new WpfControls.UI.ToolBar();
				Assert.AreEqual(KeyboardNavigationMode.Once,KeyboardNavigation.GetTabNavigation(toolBar));
			});
		}

		[TestMethod]
		public void Button_EnterAndSpaceClick_EscIsLeftToTheWindow() {
			IconRendererTests.RunSta(() => {
				NoDatabaseGuard.Install();
				int countClicks=0;
				WpfControls.UI.Button button=new WpfControls.UI.Button();
				button.Click+=(sender,e) => countClicks++;
				Assert.IsTrue(RaiseKeyDown(button,Key.Enter),"Enter is the focused button's: the Frm's own Enter does not run too");
				Assert.IsTrue(RaiseKeyDown(button,Key.Space));
				Assert.AreEqual(2,countClicks);
				Assert.IsFalse(RaiseKeyDown(button,Key.Escape),"Esc closes the window as before (FrmODBase)");
				Assert.IsFalse(RaiseKeyDown(button,Key.A));
				button.IsEnabled=false;
				Assert.IsFalse(RaiseKeyDown(button,Key.Enter),"a disabled button does nothing");
				Assert.AreEqual(2,countClicks);
			});
		}

		[TestMethod]
		public void CheckBox_SpaceToggles_EnterIsLeftToTheWindow() {
			IconRendererTests.RunSta(() => {
				NoDatabaseGuard.Install();
				int countClicks=0;
				WpfControls.UI.CheckBox checkBox=new WpfControls.UI.CheckBox();
				checkBox.Click+=(sender,e) => countClicks++;
				Assert.IsTrue(RaiseKeyDown(checkBox,Key.Space));
				Assert.AreEqual(true,checkBox.Checked);
				Assert.IsTrue(RaiseKeyDown(checkBox,Key.Space));
				Assert.AreEqual(false,checkBox.Checked);
				Assert.IsFalse(RaiseKeyDown(checkBox,Key.Enter),"Enter stays the window's (its default button)");
				Assert.AreEqual(2,countClicks);
			});
		}

		[TestMethod]
		public void RadioButton_SpaceChecks_ArrowsMoveInTheGroup() {
			IconRendererTests.RunSta(() => {
				NoDatabaseGuard.Install();
				System.Windows.Controls.Grid grid=new System.Windows.Controls.Grid();
				List<WpfControls.UI.RadioButton> listRadioButtons=new List<WpfControls.UI.RadioButton>();
				for(int i=0;i<3;i++) {
					WpfControls.UI.RadioButton radioButton=new WpfControls.UI.RadioButton();
					grid.Children.Add(radioButton);
					listRadioButtons.Add(radioButton);
				}
				Assert.IsTrue(RaiseKeyDown(listRadioButtons[1],Key.Space));
				Assert.IsTrue(listRadioButtons[1].Checked);
				Assert.IsTrue(RaiseKeyDown(listRadioButtons[1],Key.Down));
				CollectionAssert.AreEqual(new[] { false,false,true },listRadioButtons.Select(x => x.Checked).ToList(),"Down checks the next one");
				Assert.IsTrue(RaiseKeyDown(listRadioButtons[2],Key.Right));
				CollectionAssert.AreEqual(new[] { true,false,false },listRadioButtons.Select(x => x.Checked).ToList(),"and wraps round");
				Assert.IsTrue(RaiseKeyDown(listRadioButtons[0],Key.Up));
				CollectionAssert.AreEqual(new[] { false,false,true },listRadioButtons.Select(x => x.Checked).ToList(),"Up goes back");
			});
		}

		[TestMethod]
		public void TabOrder_ReadingOrder_RowsTopToBottomLeftToRight() {
			List<Rect> listRects=new List<Rect> {
				new Rect(200,10,80,20),//row 1, right
				new Rect(10,12,80,20),//row 1, left (2 DIP lower: the same row)
				new Rect(300,45,80,20),//row 2, right
				new Rect(10,40,80,20),//row 2, left
				new Rect(10,200,80,24),//the bottom
			};
			CollectionAssert.AreEqual(new[] { 1,0,3,2,4 },WpfControls.UI.UiTabOrder.GetReadingOrder(listRects));
		}

		///<summary>A window's content in scrambled XAML order: Save (bottom right), Delete (bottom left), a check box (middle), the description text
		///box (top), a group box (right of the check box) with two radio buttons, right then left.</summary>
		public static System.Windows.Controls.Grid BuildScrambledContent() {
			System.Windows.Controls.Grid grid=new System.Windows.Controls.Grid();
			grid.Width=400;
			grid.Height=300;
			grid.Children.Add(NewAt(new WpfControls.UI.Button() { Name="butSave",Text="Save" },HorizontalAlignment.Right,VerticalAlignment.Bottom,new Thickness(0,0,20,15),75,24));
			grid.Children.Add(NewAt(new WpfControls.UI.Button() { Name="butDelete",Text="Delete" },HorizontalAlignment.Left,VerticalAlignment.Bottom,new Thickness(20,0,0,15),75,24));
			grid.Children.Add(NewAt(new WpfControls.UI.CheckBox() { Name="checkInactive",Text="Inactive" },HorizontalAlignment.Left,VerticalAlignment.Top,new Thickness(20,80,0,0),120,20));
			grid.Children.Add(NewAt(new WpfControls.UI.TextBox() { Name="textDescription" },HorizontalAlignment.Left,VerticalAlignment.Top,new Thickness(20,20,0,0),200,20));
			WpfControls.UI.GroupBox groupBox=new WpfControls.UI.GroupBox() { Name="groupType",Text="Type" };
			grid.Children.Add(NewAt(groupBox,HorizontalAlignment.Left,VerticalAlignment.Top,new Thickness(200,70,0,0),180,80));
			groupBox.Items.Add(NewAt(new WpfControls.UI.RadioButton() { Name="radioRight",Text="Right" },HorizontalAlignment.Left,VerticalAlignment.Top,new Thickness(90,25,0,0),70,20));
			groupBox.Items.Add(NewAt(new WpfControls.UI.RadioButton() { Name="radioLeft",Text="Left" },HorizontalAlignment.Left,VerticalAlignment.Top,new Thickness(10,25,0,0),70,20));
			return grid;
		}

		private static FrameworkElement NewAt(FrameworkElement frameworkElement,HorizontalAlignment horizontalAlignment,VerticalAlignment verticalAlignment,
			Thickness thickness,double width,double height)
		{
			frameworkElement.HorizontalAlignment=horizontalAlignment;
			frameworkElement.VerticalAlignment=verticalAlignment;
			frameworkElement.Margin=thickness;
			frameworkElement.Width=width;
			frameworkElement.Height=height;
			return frameworkElement;
		}

		///<summary>The name of the element or of its nearest named ancestor (a text box's inner tab stop is named by the text box).</summary>
		public static string GetName(DependencyObject dependencyObject) {
			while(dependencyObject!=null) {
				if(dependencyObject is FrameworkElement frameworkElement && !string.IsNullOrEmpty(frameworkElement.Name)
					&& frameworkElement.GetType().Namespace=="WpfControls.UI")
				{
					return frameworkElement.Name;
				}
				dependencyObject=System.Windows.Media.VisualTreeHelper.GetParent(dependencyObject) ?? LogicalTreeHelper.GetParent(dependencyObject);
			}
			return null;
		}

		[TestMethod]
		public void TabOrder_Apply_ReadingOrderAndNestedGroups_AnOrderOfTheFrmsOwnWins() {
			IconRendererTests.RunSta(() => {
				NoDatabaseGuard.Install();
				System.Windows.Controls.Grid grid=BuildScrambledContent();
				grid.Measure(new Size(400,300));
				grid.Arrange(new Rect(0,0,400,300));
				grid.UpdateLayout();
				List<UIElement> listOrdered=WpfControls.UI.UiTabOrder.Apply(grid);
				List<string> listNames=listOrdered.Select(x => GetName(x)).ToList();
				CollectionAssert.AreEqual(new[] { "textDescription","groupType","radioLeft","radioRight","checkInactive","butDelete","butSave" },listNames,
					string.Join(",",listNames));
				//Inside the group the order is its own (1, 2); in the window the group is second.
				Assert.AreEqual(2,KeyboardNavigation.GetTabIndex(listOrdered[1]));
				Assert.AreEqual(1,KeyboardNavigation.GetTabIndex(listOrdered[2]));
				Assert.AreEqual(4,KeyboardNavigation.GetTabIndex(listOrdered.Single(x => GetName(x)=="butDelete")));
				//A Frm that sets a TabIndexOD keeps its own order: nothing is set.
				System.Windows.Controls.Grid gridOwn=BuildScrambledContent();
				((WpfControls.UI.TextBox)gridOwn.Children[3]).TabIndexOD=1;
				gridOwn.Measure(new Size(400,300));
				gridOwn.Arrange(new Rect(0,0,400,300));
				Assert.AreEqual(0,WpfControls.UI.UiTabOrder.Apply(gridOwn).Count);
				Assert.AreEqual(int.MaxValue,KeyboardNavigation.GetTabIndex((UIElement)gridOwn.Children[0]),"Save keeps the default");
			});
		}

		[TestMethod]
		public void TabOrder_RealTabWalk_InAShownWindow() {
			List<string> listNames=null;
			bool isFocused=false;
			IconRendererTests.RunSta(() => {
				NoDatabaseGuard.Install();
				Window window=new Window();
				window.WindowStyle=WindowStyle.None;
				window.ShowInTaskbar=false;
				window.Left=-20000;
				window.Top=-20000;
				window.Width=400;
				window.Height=300;
				System.Windows.Controls.Grid grid=BuildScrambledContent();
				window.Content=grid;
				try {
					window.Show();
					window.Activate();
					grid.UpdateLayout();
					List<UIElement> listOrdered=WpfControls.UI.UiTabOrder.Apply(grid);
					UIElement uIElementFirst=listOrdered[0];
					uIElementFirst.Focus();
					isFocused=uIElementFirst.IsKeyboardFocused;
					if(!isFocused) {
						return;
					}
					listNames=new List<string>();
					for(int i=0;i<7;i++) {
						listNames.Add(GetName((DependencyObject)Keyboard.FocusedElement));
						((UIElement)Keyboard.FocusedElement).MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
					}
				}
				finally {
					window.Close();
				}
			});
			if(!isFocused) {
				Assert.Inconclusive("The session gave the off-screen window no keyboard focus; the order itself is checked in TabOrder_Apply.");
			}
			CollectionAssert.AreEqual(new[] { "textDescription","radioLeft","radioRight","checkInactive","butDelete","butSave","textDescription" },listNames,
				string.Join(",",listNames));
		}

		///<summary>The Frms whose PreviewKeyDown (which runs before a focused control's KeyDown) looks at Enter, and why each is right now that a
		///focused button takes Enter itself.</summary>
		public static readonly Dictionary<string,string> DictPreviewEnterReviewed=new Dictionary<string,string> {
			{ "FrmApptProvPrompt.xaml.cs","leaves Enter to a focused button (Keyboard.FocusedElement is a Button)" },
			{ "FrmCaptureSession.xaml.cs","takes Enter only while typing tooth numbers, and handles it" },
		};

		[TestMethod]
		public void Frms_PreviewKeyDownEnter_DoesNotRunBesideAFocusedButton() {
			string root=RepoPaths.Root();
			List<string> listFiles=new List<string>();
			foreach(string folder in new[] { "WpfControlsOD","ImagingCapture","OpenPmsDental" }) {
				listFiles.AddRange(Directory.GetFiles(Path.Combine(root,folder),"Frm*.xaml.cs",SearchOption.AllDirectories)
					.Where(x => !x.Contains(@"\obj\") && !x.Contains(@"\bin\")));
			}
			Assert.IsTrue(listFiles.Count>150,"the Frms are found: "+listFiles.Count);
			List<string> listFound=new List<string>();
			foreach(string file in listFiles) {
				string text=File.ReadAllText(file);
				foreach(Match match in Regex.Matches(text,@"PreviewKeyDown\s*\+=\s*(\w+)\s*;")) {
					string body;
					try {
						body=ToolBarSources.GetMethodBody(text,match.Groups[1].Value);
					}
					catch(Exception) {
						continue;//a handler of another class
					}
					if(body.Contains("Key.Enter") || body.Contains("Key.Return")) {
						listFound.Add(Path.GetFileName(file));
					}
				}
			}
			CollectionAssert.AreEquivalent(DictPreviewEnterReviewed.Keys.ToList(),listFound.Distinct().ToList(),string.Join(", ",listFound));
			string textPrompt=File.ReadAllText(listFiles.Single(x => Path.GetFileName(x)=="FrmApptProvPrompt.xaml.cs"));
			Assert.IsTrue(textPrompt.Contains("Keyboard.FocusedElement is WpfControls.UI.Button"));
		}
	}
}
