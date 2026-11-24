// UI-OVF's toolbar tests (docs/port-notes/UI-OVF.md; docs/specs/ui-refresh-plan.md section 7): the buttons that do not fit go into a » overflow
// button at the toolbar's right end, whose drop-down has each of them with the same text, picture, enabled and checked state, click and sub-menu.
// ToolBarOD (the program's toolbars), the WPF ToolBar (the Imaging module) and the stock tool strips (UiToolStripRenderer draws their own overflow
// button in the same look). ToolBarOD is reachable from the keyboard (arrows, Home, End, Enter, Space, Down) and tells screen readers about its
// buttons and the overflow button. Built off screen: no window is shown (the drop-downs are built, not opened), no database.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental.UI;
using UiRefreshTests.Scan;

namespace UiRefreshTests.Toolbars {
	[TestClass]
	public class ToolBarOverflowTests {
		///<summary>A toolbar whose protected key handling the tests can call.</summary>
		private class ToolBarODKeys:ToolBarOD {
			///<summary>As after a Tab into the toolbar: Windows shows keyboard cues (the toolbar takes its keys only then).</summary>
			protected override bool ShowFocusCues=>true;

			public bool IsInputKeyTest(Keys keys) {
				return IsInputKey(keys);
			}

			public void PressKey(Keys keys) {
				OnKeyDown(new KeyEventArgs(keys));
			}
		}

		///<summary>The toolbar's own layout, as its OnPaint runs it.</summary>
		public static void LayOut(ToolBarOD toolBarOD) {
			MethodInfo methodInfo=typeof(ToolBarOD).GetMethod("ComputeButtonsizes",BindingFlags.Instance|BindingFlags.NonPublic);
			using(Bitmap bitmap=new Bitmap(1,1))
			using(Graphics g=Graphics.FromImage(bitmap)) {
				methodInfo.Invoke(toolBarOD,new object[] { g });
			}
		}

		///<summary>A toolbar with a button of each kind: Alpha (icon), a separator, Bravo (toggle, pushed), Charlie (disabled), a separator, Delta
		///(drop-down with a menu of two items), Echo (drop-down whose arrow raises ButtonDropDownClick), an icon-only button with a tooltip, Golf (a
		///label) and "Hotel & Co".</summary>
		private static ToolBarODKeys BuildToolBar(ContextMenuStrip contextMenuStripDelta) {
			ToolBarODKeys toolBarOD=new ToolBarODKeys();
			toolBarOD.Size=new Size(2000,ToolBarModel.HEIGHT_TOOLBAR);
			toolBarOD.Buttons.Add(new ODToolBarButton("Alpha",EnumIcons.Add,"Alpha tip","Alpha"));
			toolBarOD.Buttons.Add(new ODToolBarButton(ODToolBarButtonStyle.Separator));
			ODToolBarButton toolBarButtonBravo=new ODToolBarButton("Bravo",-1,"","Bravo");
			toolBarButtonBravo.Style=ODToolBarButtonStyle.ToggleButton;
			toolBarButtonBravo.IsTogglePushed=true;
			toolBarOD.Buttons.Add(toolBarButtonBravo);
			ODToolBarButton toolBarButtonCharlie=new ODToolBarButton("Charlie",EnumIcons.DeleteX,"","Charlie");
			toolBarButtonCharlie.Enabled=false;
			toolBarOD.Buttons.Add(toolBarButtonCharlie);
			toolBarOD.Buttons.Add(new ODToolBarButton(ODToolBarButtonStyle.Separator));
			ODToolBarButton toolBarButtonDelta=new ODToolBarButton("Delta",EnumIcons.Print,"","Delta");
			toolBarButtonDelta.Style=ODToolBarButtonStyle.DropDownButton;
			toolBarButtonDelta.ContextMenuStripDropDown=contextMenuStripDelta;
			toolBarOD.Buttons.Add(toolBarButtonDelta);
			ODToolBarButton toolBarButtonEcho=new ODToolBarButton("Echo",-1,"","Echo");
			toolBarButtonEcho.Style=ODToolBarButtonStyle.DropDownButton;
			toolBarOD.Buttons.Add(toolBarButtonEcho);
			toolBarOD.Buttons.Add(new ODToolBarButton("",EnumIcons.Print,"Foxtrot tip","Foxtrot"));
			ODToolBarButton toolBarButtonGolf=new ODToolBarButton("Golf",-1,"","Golf");
			toolBarButtonGolf.Style=ODToolBarButtonStyle.Label;
			toolBarOD.Buttons.Add(toolBarButtonGolf);
			toolBarOD.Buttons.Add(new ODToolBarButton("Hotel & Co",-1,"","Hotel"));
			return toolBarOD;
		}

		///<summary>Sets the toolbar's width so the first button and the overflow button fit, nothing more, and lays it out.</summary>
		private static void NarrowToFirstButton(ToolBarOD toolBarOD) {
			LayOut(toolBarOD);
			int widthOverflow=toolBarOD.LayoutManager.Scale(UiToolStripRenderer.WIDTH_OVERFLOW_BUTTON96);
			toolBarOD.Width=toolBarOD.Buttons[0].Bounds.Right+widthOverflow+1;
			LayOut(toolBarOD);
		}

		private static void ToolBarOD_Fits_NoOverflowButton_Body() {
			NoDatabaseGuard.Install();
			using(ContextMenuStrip contextMenuStrip=new ContextMenuStrip())
			using(ToolBarOD toolBarOD=BuildToolBar(contextMenuStrip)) {
				LayOut(toolBarOD);
				Assert.IsFalse(toolBarOD.IsOverflowing);
				Assert.AreEqual(Rectangle.Empty,toolBarOD.BoundsOverflow);
				Assert.AreEqual(0,toolBarOD.GetButtonsOverflowed().Count);
				Assert.AreEqual(0,toolBarOD.GetOverflowMenu().Items.Count);
			}
		}

		private static void ToolBarOD_OnlySeparatorsPastTheEdge_NoOverflowButton_Body() {
			NoDatabaseGuard.Install();
			using(ToolBarOD toolBarOD=new ToolBarOD()) {
				toolBarOD.Size=new Size(2000,ToolBarModel.HEIGHT_TOOLBAR);
				toolBarOD.Buttons.Add(new ODToolBarButton("Alpha",-1,"","Alpha"));
				toolBarOD.Buttons.Add(new ODToolBarButton(ODToolBarButtonStyle.Separator));
				LayOut(toolBarOD);
				toolBarOD.Width=toolBarOD.Buttons[0].Bounds.Right+1;
				LayOut(toolBarOD);
				Assert.IsFalse(toolBarOD.IsOverflowing,"a separator alone is not worth an overflow button");
			}
		}

		private static void ToolBarOD_Overflow_TheButtonsThatDoNotFitAreInTheDropDown_Body() {
			NoDatabaseGuard.Install();
			foreach(float scale in new[] { 1f,1.5f }) {
				using(ContextMenuStrip contextMenuStrip=new ContextMenuStrip())
				using(ToolBarODKeys toolBarOD=BuildToolBar(contextMenuStrip)) {
					contextMenuStrip.Items.Add("Delta one");
					contextMenuStrip.Items.Add("Delta two");
					toolBarOD.LayoutManager.SetScaleMS(scale);
					NarrowToFirstButton(toolBarOD);
					string textScale=" at "+(int)(scale*100)+"%";
					Assert.IsTrue(toolBarOD.IsOverflowing,textScale);
					int widthOverflow=(int)Math.Round(UiToolStripRenderer.WIDTH_OVERFLOW_BUTTON96*scale);
					Assert.AreEqual(new Rectangle(toolBarOD.Width-widthOverflow,0,widthOverflow,toolBarOD.Height-1),toolBarOD.BoundsOverflow,"the right end"+textScale);
					//Alpha stays; everything after it (the separator first) is in the drop-down, in order.
					List<ODToolBarButton> listOverflowed=toolBarOD.GetButtonsOverflowed();
					CollectionAssert.AreEqual(Enumerable.Range(1,toolBarOD.Buttons.Count-1).Select(x => toolBarOD.Buttons[x]).ToList(),listOverflowed,textScale);
					Assert.IsFalse(toolBarOD.IsOverflowed(toolBarOD.Buttons[0]));
					Assert.IsTrue(toolBarOD.Buttons[0].Bounds.Right<=toolBarOD.BoundsOverflow.Left,"the button left on the bar is not under the overflow button"+textScale);
					//The drop-down: no leading separator; the items in the buttons' order with their states.
					ContextMenuStrip contextMenuOverflow=toolBarOD.GetOverflowMenu();
					List<string> listTexts=contextMenuOverflow.Items.Cast<ToolStripItem>().Select(x => x is ToolStripSeparator ? "|" : x.Text).ToList();
					//Delta twice: its main part, then its menu (its arrow); Echo once, as nothing handles ButtonDropDownClick.
					CollectionAssert.AreEqual(new[] { "Bravo","Charlie","|","Delta","Delta","Echo","Foxtrot tip","Golf","Hotel && Co" },listTexts,string.Join(",",listTexts)+textScale);
					ToolStripMenuItem itemBravo=(ToolStripMenuItem)contextMenuOverflow.Items[0];
					Assert.IsTrue(itemBravo.Checked,"a pushed toggle is checked");
					Assert.IsFalse(((ToolStripMenuItem)contextMenuOverflow.Items[1]).Enabled,"disabled stays disabled");
					Assert.IsNotNull(contextMenuOverflow.Items[1].Image,"Charlie's icon");
					Assert.AreEqual((int)Math.Round(16*scale),contextMenuOverflow.Items[1].Image.Width,"the icon at the menu's size"+textScale);
					ToolStripMenuItem itemDelta=(ToolStripMenuItem)contextMenuOverflow.Items[3];
					Assert.IsFalse(itemDelta.HasDropDownItems,"Delta's main part has no sub-menu: Enter and a click run it");
					Assert.IsNotNull(itemDelta.Image,"Delta's icon on its main part");
					ToolStripMenuItem itemDeltaMenu=(ToolStripMenuItem)contextMenuOverflow.Items[4];
					Assert.AreSame(contextMenuStrip,itemDeltaMenu.DropDown,"Delta's own menu is the second item's sub-menu");
					Assert.IsTrue(itemDeltaMenu.HasDropDownItems);
					ToolStripMenuItem itemEcho=(ToolStripMenuItem)contextMenuOverflow.Items[5];
					Assert.AreEqual(0,itemEcho.DropDownItems.Count,"no arrow item while nothing handles ButtonDropDownClick");
					Assert.IsNotNull(contextMenuOverflow.Items[6].Image,"the icon-only button's icon");
					Assert.IsFalse(contextMenuOverflow.Items[7].Enabled,"a label is not clickable");
				}
			}
		}

		private static void ToolBarOD_Overflow_AnItemDoesWhatTheButtonDoes_Body() {
			NoDatabaseGuard.Install();
			using(ContextMenuStrip contextMenuStrip=new ContextMenuStrip())
			using(ToolBarODKeys toolBarOD=BuildToolBar(contextMenuStrip)) {
				contextMenuStrip.Items.Add("Delta one");
				List<string> listClicked=new List<string>();
				toolBarOD.ButtonClick+=(sender,e) => listClicked.Add("click "+e.Button.Tag);
				toolBarOD.ButtonDropDownClick+=(sender,e) => listClicked.Add("arrow "+e.Button.Tag);
				NarrowToFirstButton(toolBarOD);
				ContextMenuStrip contextMenuOverflow=toolBarOD.GetOverflowMenu();
				ToolStripMenuItem itemBravo=(ToolStripMenuItem)contextMenuOverflow.Items[0];
				itemBravo.PerformClick();
				Assert.IsFalse(toolBarOD.Buttons[2].IsTogglePushed,"the toggle flips, as a click on it does");
				contextMenuOverflow.Items[1].PerformClick();//Charlie, disabled
				contextMenuOverflow.Items[3].PerformClick();//Delta's main part
				contextMenuOverflow.Items[4].PerformClick();//Delta's menu: opens its sub-menu, never the main part
				List<string> listTexts=contextMenuOverflow.Items.Cast<ToolStripItem>().Select(x => x is ToolStripSeparator ? "|" : x.Text).ToList();
				CollectionAssert.AreEqual(new[] { "Bravo","Charlie","|","Delta","Delta","Echo","Echo...","Foxtrot tip","Golf","Hotel && Co" },listTexts,
					string.Join(",",listTexts)+": Echo's arrow item, as ButtonDropDownClick is handled");
				Assert.IsFalse(((ToolStripMenuItem)contextMenuOverflow.Items[6]).HasDropDownItems,"a plain item, not a sub-menu");
				contextMenuOverflow.Items[5].PerformClick();//Echo's main part
				contextMenuOverflow.Items[6].PerformClick();//Echo's arrow
				contextMenuOverflow.Items[9].PerformClick();//Hotel
				CollectionAssert.AreEqual(new[] { "click Bravo","click Delta","click Echo","arrow Echo","click Hotel" },listClicked,string.Join(", ",listClicked));
				//Building the drop-down again (or disposing the toolbar) gives Delta its menu back; the menu is the button's, never disposed here.
				toolBarOD.GetOverflowMenu();
				toolBarOD.Dispose();
				Assert.IsFalse(contextMenuStrip.IsDisposed,"the drop-down button's own menu survives the toolbar's drop-down");
				Assert.IsNull(contextMenuStrip.OwnerItem,"given back to its button");
			}
		}

		///<summary>Sends the key to the item as its drop-down does to the selected item (ToolStripItem.ProcessDialogKey). Returns whether it was taken.</summary>
		private static bool ProcessItemKey(ToolStripItem toolStripItem,Keys keys) {
			MethodInfo methodInfo=typeof(ToolStripItem).GetMethod("ProcessDialogKey",BindingFlags.Instance|BindingFlags.NonPublic,null,new[] { typeof(Keys) },null);
			return (bool)methodInfo.Invoke(toolStripItem,new object[] { keys });
		}

		///<summary>Raises the menu's Opening, as showing it does (the program's handlers, then the toolbar's). Returns whether it was cancelled.</summary>
		private static bool RaiseOpening(ContextMenuStrip contextMenuStrip) {
			System.ComponentModel.CancelEventArgs cancelEventArgs=new System.ComponentModel.CancelEventArgs();
			MethodInfo methodInfo=typeof(ToolStripDropDown).GetMethod("OnOpening",BindingFlags.Instance|BindingFlags.NonPublic,null,new[] { typeof(System.ComponentModel.CancelEventArgs) },null);
			methodInfo.Invoke(contextMenuStrip,new object[] { cancelEventArgs });
			return cancelEventArgs.Cancel;
		}

		private static void ToolBarOD_Overflow_DropDownButton_EnterRunsTheMainPart_TheMenuIsItsOwnItem_Body() {
			NoDatabaseGuard.Install();
			using(ContextMenuStrip contextMenuStrip=new ContextMenuStrip())
			using(ToolBarODKeys toolBarOD=BuildToolBar(contextMenuStrip)) {
				contextMenuStrip.Items.Add("Delta one");
				List<string> listClicked=new List<string>();
				toolBarOD.ButtonClick+=(sender,e) => listClicked.Add("click "+e.Button.Tag);
				toolBarOD.ButtonDropDownClick+=(sender,e) => listClicked.Add("arrow "+e.Button.Tag);
				NarrowToFirstButton(toolBarOD);
				ContextMenuStrip contextMenuOverflow=toolBarOD.GetOverflowMenu();
				//Enter on the selected item, as the drop-down sends it: Delta's main part clicks (an item with a sub-menu would open it instead).
				ToolStripMenuItem itemDelta=(ToolStripMenuItem)contextMenuOverflow.Items[3];
				Assert.AreEqual("Delta",itemDelta.Text);
				Assert.IsTrue(ProcessItemKey(itemDelta,Keys.Enter));
				CollectionAssert.AreEqual(new[] { "click Delta" },listClicked,string.Join(", ",listClicked));
				//The next item is the menu: Enter, Space and Right open it (it has sub-items), its own click runs nothing.
				ToolStripMenuItem itemDeltaMenu=(ToolStripMenuItem)contextMenuOverflow.Items[4];
				Assert.IsTrue(itemDeltaMenu.HasDropDownItems);
				Assert.AreSame(contextMenuStrip,itemDeltaMenu.DropDown);
				itemDeltaMenu.PerformClick();
				Assert.AreEqual(1,listClicked.Count,"the menu's item does not run the main part");
				//Echo's arrow opens a window: its own plain item, Enter runs it.
				ToolStripMenuItem itemEchoArrow=(ToolStripMenuItem)contextMenuOverflow.Items[6];
				Assert.AreEqual("Echo...",itemEchoArrow.Text);
				Assert.IsTrue(ProcessItemKey(itemEchoArrow,Keys.Enter));
				Assert.IsTrue(ProcessItemKey(contextMenuOverflow.Items[5],Keys.Enter));
				CollectionAssert.AreEqual(new[] { "click Delta","arrow Echo","click Echo" },listClicked,string.Join(", ",listClicked));
			}
		}

		private static void ToolBarOD_Overflow_AMenuFilledOnOpening_HasASubMenuAndIsFilled_Body() {
			NoDatabaseGuard.Install();
			using(ContextMenuStrip contextMenuStrip=new ContextMenuStrip())
			using(ToolBarODKeys toolBarOD=BuildToolBar(contextMenuStrip)) {
				//As FormOpenDental's menuEmail: empty until its Opening handler clears and fills it.
				bool isEmptyNow=false;
				contextMenuStrip.Opening+=(sender,e) => {
					contextMenuStrip.Items.Clear();
					if(!isEmptyNow) {
						contextMenuStrip.Items.Add("Delta filled on opening");
						contextMenuStrip.Items.Add("Delta filled too");
					}
				};
				NarrowToFirstButton(toolBarOD);
				ContextMenuStrip contextMenuOverflow=toolBarOD.GetOverflowMenu();
				ToolStripMenuItem itemDeltaMenu=(ToolStripMenuItem)contextMenuOverflow.Items[4];
				Assert.IsTrue(itemDeltaMenu.HasDropDownItems,"an empty menu still has a sub-menu (a placeholder), so hover, Right and Enter open it");
				Assert.IsFalse(RaiseOpening(contextMenuStrip),"it opens");
				CollectionAssert.AreEqual(new[] { "Delta filled on opening","Delta filled too" },contextMenuStrip.Items.Cast<ToolStripItem>().Select(x => x.Text).ToList(),
					"the program's handler filled it; the placeholder is gone");
				//Emptied again: the placeholder comes back with the next drop-down; a menu still empty after Opening does not open.
				contextMenuStrip.Items.Clear();
				isEmptyNow=true;
				contextMenuOverflow=toolBarOD.GetOverflowMenu();
				Assert.IsTrue(((ToolStripMenuItem)contextMenuOverflow.Items[4]).HasDropDownItems);
				Assert.IsTrue(RaiseOpening(contextMenuStrip),"nothing to show: cancelled, as the button's own empty menu shows nothing");
				Assert.AreEqual(0,contextMenuStrip.Items.Count);
				//Given back: no placeholder left in the button's menu, and the toolbar's Opening handler is gone.
				contextMenuOverflow=toolBarOD.GetOverflowMenu();
				toolBarOD.Dispose();
				Assert.AreEqual(0,contextMenuStrip.Items.Count,"the placeholder is taken out when the menu is given back");
				Assert.IsNull(contextMenuStrip.OwnerItem);
				Assert.IsFalse(RaiseOpening(contextMenuStrip),"the toolbar's handler no longer runs (it would cancel an empty menu)");
			}
		}

		private static void ToolBarOD_Keyboard_ArrowsReachTheOverflowButton_EnterAndSpaceClick_Body() {
			NoDatabaseGuard.Install();
			using(ContextMenuStrip contextMenuStrip=new ContextMenuStrip())
			using(ToolBarODKeys toolBarOD=BuildToolBar(contextMenuStrip)) {
				List<string> listClicked=new List<string>();
				toolBarOD.ButtonClick+=(sender,e) => listClicked.Add(e.Button.Tag.ToString());
				FieldInfo fieldInfoFocused=typeof(ToolBarOD).GetField("_toolBarButtonFocused",BindingFlags.Instance|BindingFlags.NonPublic);
				//Wide: the arrows go over the enabled buttons (not Charlie, the separators or the label Golf).
				LayOut(toolBarOD);
				foreach(Keys keys in new[] { Keys.Left,Keys.Right,Keys.Home,Keys.End,Keys.Enter,Keys.Space,Keys.Down }) {
					Assert.IsTrue(toolBarOD.IsInputKeyTest(keys),keys+" is the toolbar's own key");
				}
				Assert.IsFalse(toolBarOD.IsInputKeyTest(Keys.Tab),"Tab leaves the toolbar");
				List<string> listVisited=new List<string>();
				toolBarOD.PressKey(Keys.Home);
				for(int i=0;i<7;i++) {
					listVisited.Add(((ODToolBarButton)fieldInfoFocused.GetValue(toolBarOD)).Tag.ToString());
					toolBarOD.PressKey(Keys.Right);
				}
				CollectionAssert.AreEqual(new[] { "Alpha","Bravo","Delta","Echo","Foxtrot","Hotel","Alpha" },listVisited,string.Join(",",listVisited));
				toolBarOD.PressKey(Keys.Home);
				toolBarOD.PressKey(Keys.Enter);//Alpha
				toolBarOD.PressKey(Keys.Right);
				toolBarOD.PressKey(Keys.Space);//Bravo, a toggle
				Assert.IsFalse(toolBarOD.Buttons[2].IsTogglePushed);
				CollectionAssert.AreEqual(new[] { "Alpha","Bravo" },listClicked);
				//Narrow: End goes to the overflow button, the last stop.
				NarrowToFirstButton(toolBarOD);
				toolBarOD.PressKey(Keys.End);
				Assert.AreEqual(ToolBarOD.TAG_OVERFLOW,((ODToolBarButton)fieldInfoFocused.GetValue(toolBarOD)).Tag);
				toolBarOD.PressKey(Keys.Right);
				Assert.AreEqual("Alpha",((ODToolBarButton)fieldInfoFocused.GetValue(toolBarOD)).Tag,"wraps round");
			}
		}

		private static void ToolBarOD_Accessibility_ButtonsAndTheOverflowButtonAnnounceThemselves_Body() {
			NoDatabaseGuard.Install();
			using(ContextMenuStrip contextMenuStrip=new ContextMenuStrip())
			using(ToolBarODKeys toolBarOD=BuildToolBar(contextMenuStrip)) {
				NarrowToFirstButton(toolBarOD);
				AccessibleObject accessibleObject=toolBarOD.AccessibilityObject;
				Assert.AreEqual(AccessibleRole.ToolBar,accessibleObject.Role);
				Assert.AreEqual(2,accessibleObject.GetChildCount(),"Alpha and the overflow button");
				AccessibleObject accessibleObjectAlpha=accessibleObject.GetChild(0);
				Assert.AreEqual("Alpha",accessibleObjectAlpha.Name);
				Assert.AreEqual(AccessibleRole.PushButton,accessibleObjectAlpha.Role);
				AccessibleObject accessibleObjectOverflow=accessibleObject.GetChild(1);
				Assert.AreEqual("More buttons",accessibleObjectOverflow.Name);
				Assert.AreEqual(AccessibleRole.ButtonMenu,accessibleObjectOverflow.Role);
				Assert.IsTrue(accessibleObjectOverflow.State.HasFlag(AccessibleStates.HasPopup));
				Assert.AreEqual("Open",accessibleObjectOverflow.DefaultAction);
				Assert.AreEqual(toolBarOD.RectangleToScreen(toolBarOD.BoundsOverflow),accessibleObjectOverflow.Bounds);
				//Wide: every button but the separators, each with its kind and state.
				toolBarOD.Width=2000;
				LayOut(toolBarOD);
				Assert.AreEqual(8,accessibleObject.GetChildCount());
				Assert.AreEqual(AccessibleRole.CheckButton,accessibleObject.GetChild(1).Role);
				Assert.IsTrue(accessibleObject.GetChild(1).State.HasFlag(AccessibleStates.Checked),"Bravo is pushed");
				Assert.IsTrue(accessibleObject.GetChild(2).State.HasFlag(AccessibleStates.Unavailable),"Charlie is disabled");
				Assert.AreEqual(AccessibleRole.ButtonDropDown,accessibleObject.GetChild(3).Role);
				Assert.AreEqual("Foxtrot tip",accessibleObject.GetChild(5).Name,"an icon-only button is named by its tooltip");
				Assert.AreEqual(AccessibleRole.StaticText,accessibleObject.GetChild(6).Role,"the label");
			}
		}

		private static void ToolBarOD_Overflow_DrawsTheGlyphAndNotTheOverflowedButtons_Body() {
			NoDatabaseGuard.Install();
			using(ContextMenuStrip contextMenuStrip=new ContextMenuStrip())
			using(ToolBarODKeys toolBarOD=BuildToolBar(contextMenuStrip)) {
				NarrowToFirstButton(toolBarOD);
				toolBarOD.Width+=10;//room for the separator after Alpha, not for Bravo: the separator is left out too, so a gap shows
				LayOut(toolBarOD);
				Assert.AreEqual(1,toolBarOD.Buttons.IndexOf(toolBarOD.GetButtonsOverflowed()[0]),"the separator goes with the buttons after it");
				using(Bitmap bitmap=ToolBarPictureTests.Draw(toolBarOD)) {
					Rectangle rectangleOverflow=toolBarOD.BoundsOverflow;
					Assert.IsTrue(CountDark(bitmap,rectangleOverflow)>=6,"the » glyph");
					//Between Alpha and the overflow button: the toolbar's background only (the overflowed buttons are not drawn there).
					Rectangle rectangleGap=new Rectangle(toolBarOD.Buttons[0].Bounds.Right,2,rectangleOverflow.Left-toolBarOD.Buttons[0].Bounds.Right,toolBarOD.Height-4);
					Assert.AreEqual(0,CountDark(bitmap,rectangleGap),"nothing drawn past the last button");
				}
			}
		}

		private static void ToolStrip_OwnOverflowButton_InTheLook_Body() {
			NoDatabaseGuard.Install();
			UiTheme.InitWinForms();
			using(ToolStrip toolStrip=new ToolStrip()) {
				toolStrip.Renderer=new UiToolStripRenderer();
				toolStrip.GripStyle=ToolStripGripStyle.Hidden;
				toolStrip.Size=new Size(160,ToolBarModel.HEIGHT_TOOLBAR);
				for(int i=0;i<10;i++) {
					toolStrip.Items.Add(new ToolStripButton("Button "+i));
				}
				toolStrip.CreateControl();
				toolStrip.PerformLayout();
				Assert.IsTrue(toolStrip.Items[9].IsOnOverflow,"the last button is on the strip's overflow");
				Rectangle rectangleOverflow=toolStrip.OverflowButton.Bounds;
				Assert.IsTrue(rectangleOverflow.Width>0);
				using(Bitmap bitmap=new Bitmap(toolStrip.Width,toolStrip.Height)) {
					toolStrip.DrawToBitmap(bitmap,new Rectangle(0,0,toolStrip.Width,toolStrip.Height));
					Assert.IsTrue(CountDark(bitmap,rectangleOverflow)>=6,"the » glyph");
					//At rest no shape and no gradient (the professional renderer's): the button's edges are one flat colour, top to bottom.
					Color colorTop=bitmap.GetPixel(rectangleOverflow.X+1,rectangleOverflow.Y+2);
					Color colorMiddle=bitmap.GetPixel(rectangleOverflow.X+1,rectangleOverflow.Y+rectangleOverflow.Height/2);
					Color colorBottom=bitmap.GetPixel(rectangleOverflow.Right-2,rectangleOverflow.Bottom-4);
					Assert.IsTrue(IsNear(colorTop,colorMiddle,3) && IsNear(colorTop,colorBottom,3),"flat at rest: "+colorTop+", "+colorMiddle+", "+colorBottom);
				}
			}
		}

		[TestMethod]
		public void WpfToolBar_Overflow_TheButtonsThatDoNotFitAreInTheDropDown() {
			IconRendererTests.RunSta(() => {
				NoDatabaseGuard.Install();
				List<string> listClicked=new List<string>();
				WpfControls.UI.ToolBar toolBar=new WpfControls.UI.ToolBar();
				toolBar.Add("Alpha",(sender,e) => listClicked.Add("Alpha"),WpfControls.UI.EnumIcons.Add);
				toolBar.AddSeparator();
				toolBar.Add("Bravo",(sender,e) => listClicked.Add("Bravo"),toolBarButtonStyle:WpfControls.UI.ToolBarButtonStyle.ToggleButton);
				WpfControls.UI.ToolBarButton toolBarButtonCharlie=new WpfControls.UI.ToolBarButton("Charlie",(sender,e) => listClicked.Add("Charlie"));
				toolBarButtonCharlie.IsEnabled=false;
				toolBar.Add(toolBarButtonCharlie);
				WpfControls.UI.ContextMenu contextMenuDelta=new WpfControls.UI.ContextMenu();
				contextMenuDelta.Add(new WpfControls.UI.MenuItem("Delta one",(sender,e) => listClicked.Add("Delta one")));
				contextMenuDelta.Opened+=(sender,e) => {
					if(contextMenuDelta.Items.Count==1) {
						contextMenuDelta.Add(new WpfControls.UI.MenuItem("Delta filled on opening",(sender2,e2) => listClicked.Add("Delta filled")));
					}
				};
				toolBar.Add("Delta",(sender,e) => listClicked.Add("Delta"),WpfControls.UI.EnumIcons.Print,WpfControls.UI.ToolBarButtonStyle.DropDownButton,
					contextMenuDropDown:contextMenuDelta);
				toolBar.Add("Scan:",null);
				toolBar.Add("",(sender,e) => listClicked.Add("Foxtrot"),WpfControls.UI.EnumIcons.Print,toolTipText:"Foxtrot_tip");
				//Wide: everything fits, no overflow button.
				toolBar.Measure(new System.Windows.Size(3000,ToolBarModel.HEIGHT_TOOLBAR));
				toolBar.Arrange(new System.Windows.Rect(0,0,3000,ToolBarModel.HEIGHT_TOOLBAR));
				Assert.IsFalse(toolBar.IsOverflowing);
				Assert.AreEqual(System.Windows.Visibility.Collapsed,toolBar.ToolBarButtonOverflow.Visibility);
				//Narrow: Alpha and the overflow button.
				double widthAlpha=((System.Windows.UIElement)((System.Windows.Controls.Panel)toolBar.FindName("stackPanel")).Children[0]).DesiredSize.Width;
				double width=widthAlpha+UiToolStripRenderer.WIDTH_OVERFLOW_BUTTON96+1;
				toolBar.Measure(new System.Windows.Size(width,ToolBarModel.HEIGHT_TOOLBAR));
				toolBar.Arrange(new System.Windows.Rect(0,0,width,ToolBarModel.HEIGHT_TOOLBAR));
				toolBar.UpdateLayout();
				Assert.IsTrue(toolBar.IsOverflowing);
				Assert.AreEqual(System.Windows.Visibility.Visible,toolBar.ToolBarButtonOverflow.Visibility);
				Assert.AreEqual(6,toolBar.GetItemsOverflowed().Count,"the separator and the five after it");
				//An overflowed button has no room on the bar: WPF clips it to its empty layout slot (not drawn, not hit).
				Assert.AreEqual(0,System.Windows.Controls.Primitives.LayoutInformation.GetLayoutSlot(toolBarButtonCharlie).Width,"an overflowed button is not shown on the bar");
				WpfControls.UI.ContextMenu contextMenu=toolBar.GetOverflowMenu();
				List<WpfControls.UI.MenuItem> listItems=contextMenu.GetMenuItems();
				CollectionAssert.AreEqual(new[] { "Bravo","Charlie","Delta","Scan:","Foxtrot__tip" },listItems.Select(x => x.Header.ToString()).ToList());
				Assert.IsFalse(listItems[1].IsEnabled,"disabled stays disabled");
				Assert.IsFalse(listItems[3].IsEnabled,"a label is not clickable");
				Assert.IsNotNull(listItems[4].Icon,"the icon-only button's icon");
				Assert.IsNotNull(listItems[2].Icon,"Delta's icon");
				//Delta: the main part, a separator, then its menu's items (filled on opening too).
				List<string> listSub=listItems[2].Items.Cast<object>().Select(x => x is System.Windows.Controls.Separator ? "|" : ((System.Windows.Controls.MenuItem)x).Header.ToString()).ToList();
				CollectionAssert.AreEqual(new[] { "Delta","|","Delta one","Delta filled on opening" },listSub,string.Join(",",listSub));
				listItems[0].RaiseEvent(new System.Windows.RoutedEventArgs(System.Windows.Controls.MenuItem.ClickEvent));
				listItems[4].RaiseEvent(new System.Windows.RoutedEventArgs(System.Windows.Controls.MenuItem.ClickEvent));
				((System.Windows.Controls.MenuItem)listItems[2].Items[0]).RaiseEvent(new System.Windows.RoutedEventArgs(System.Windows.Controls.MenuItem.ClickEvent));
				((System.Windows.Controls.MenuItem)listItems[2].Items[3]).RaiseEvent(new System.Windows.RoutedEventArgs(System.Windows.Controls.MenuItem.ClickEvent));
				CollectionAssert.AreEqual(new[] { "Bravo","Foxtrot","Delta","Delta filled" },listClicked,string.Join(", ",listClicked));
				Assert.IsTrue(toolBar.GetOverflowMenu().GetMenuItems()[0].IsChecked,"Bravo is pushed now");
			});
		}

		[TestMethod]
		public void WpfToolBarButton_Keyboard_FocusableEnterClicks() {
			IconRendererTests.RunSta(() => {
				NoDatabaseGuard.Install();
				int countClicks=0;
				WpfControls.UI.ToolBarButton toolBarButton=new WpfControls.UI.ToolBarButton("Alpha",(sender,e) => countClicks++);
				Assert.IsTrue(toolBarButton.Focusable);
				Assert.IsNotNull(toolBarButton.FocusVisualStyle,"the shared focus ring");
				Assert.IsTrue(WpfFocusTests.RaiseKeyDown(toolBarButton,System.Windows.Input.Key.Enter));
				Assert.IsTrue(WpfFocusTests.RaiseKeyDown(toolBarButton,System.Windows.Input.Key.Space));
				Assert.AreEqual(2,countClicks);
				WpfControls.UI.ToolBarButton toolBarButtonLabel=new WpfControls.UI.ToolBarButton("Scan:",null);
				Assert.IsFalse(WpfFocusTests.RaiseKeyDown(toolBarButtonLabel,System.Windows.Input.Key.Enter),"a label does not take Enter");
			});
		}

		[TestMethod]
		public void ToolBarOD_Fits_NoOverflowButton() {
			RunWinForms(ToolBarOD_Fits_NoOverflowButton_Body);
		}

		[TestMethod]
		public void ToolBarOD_OnlySeparatorsPastTheEdge_NoOverflowButton() {
			RunWinForms(ToolBarOD_OnlySeparatorsPastTheEdge_NoOverflowButton_Body);
		}

		[TestMethod]
		public void ToolBarOD_Overflow_TheButtonsThatDoNotFitAreInTheDropDown() {
			RunWinForms(ToolBarOD_Overflow_TheButtonsThatDoNotFitAreInTheDropDown_Body);
		}

		[TestMethod]
		public void ToolBarOD_Overflow_AnItemDoesWhatTheButtonDoes() {
			RunWinForms(ToolBarOD_Overflow_AnItemDoesWhatTheButtonDoes_Body);
		}

		[TestMethod]
		public void ToolBarOD_Overflow_DropDownButton_EnterRunsTheMainPart_TheMenuIsItsOwnItem() {
			RunWinForms(ToolBarOD_Overflow_DropDownButton_EnterRunsTheMainPart_TheMenuIsItsOwnItem_Body);
		}

		[TestMethod]
		public void ToolBarOD_Overflow_AMenuFilledOnOpening_HasASubMenuAndIsFilled() {
			RunWinForms(ToolBarOD_Overflow_AMenuFilledOnOpening_HasASubMenuAndIsFilled_Body);
		}

		[TestMethod]
		public void ToolBarOD_Keyboard_ArrowsReachTheOverflowButton_EnterAndSpaceClick() {
			RunWinForms(ToolBarOD_Keyboard_ArrowsReachTheOverflowButton_EnterAndSpaceClick_Body);
		}

		[TestMethod]
		public void ToolBarOD_Accessibility_ButtonsAndTheOverflowButtonAnnounceThemselves() {
			RunWinForms(ToolBarOD_Accessibility_ButtonsAndTheOverflowButtonAnnounceThemselves_Body);
		}

		[TestMethod]
		public void ToolBarOD_Overflow_DrawsTheGlyphAndNotTheOverflowedButtons() {
			RunWinForms(ToolBarOD_Overflow_DrawsTheGlyphAndNotTheOverflowedButtons_Body);
		}

		[TestMethod]
		public void ToolStrip_OwnOverflowButton_InTheLook() {
			RunWinForms(ToolStrip_OwnOverflowButton_InTheLook_Body);
		}

		///<summary>Runs a WinForms test on an STA thread, as the program's UI thread: no database, the token renderer of the thread's menus.</summary>
		private static void RunWinForms(Action action) {
			IconRendererTests.RunSta(() => {
				NoDatabaseGuard.Install();
				UiTheme.InitWinForms();
				action();
			});
		}

		///<summary>The number of pixels in the rectangle darker than mid grey (the glyphs' antialiased strokes, in Text, have such pixels; the
		///toolbar's backgrounds and shapes are light).</summary>
		public static int CountDark(Bitmap bitmap,Rectangle rectangle) {
			int count=0;
			Rectangle rectangleIn=Rectangle.Intersect(rectangle,new Rectangle(0,0,bitmap.Width,bitmap.Height));
			for(int x=rectangleIn.Left;x<rectangleIn.Right;x++) {
				for(int y=rectangleIn.Top;y<rectangleIn.Bottom;y++) {
					Color color=bitmap.GetPixel(x,y);
					if(color.R+color.G+color.B<3*128) {
						count++;
					}
				}
			}
			return count;
		}

		///<summary>The number of pixels in the rectangle within 60 (per channel sum) of the colour: antialiased glyph pixels count too.</summary>
		public static int CountNear(Bitmap bitmap,Rectangle rectangle,Color color) {
			int count=0;
			Rectangle rectangleIn=Rectangle.Intersect(rectangle,new Rectangle(0,0,bitmap.Width,bitmap.Height));
			for(int x=rectangleIn.Left;x<rectangleIn.Right;x++) {
				for(int y=rectangleIn.Top;y<rectangleIn.Bottom;y++) {
					if(IsNear(bitmap.GetPixel(x,y),color,60)) {
						count++;
					}
				}
			}
			return count;
		}

		public static bool IsNear(Color color1,Color color2,int tolerance) {
			return Math.Abs(color1.R-color2.R)+Math.Abs(color1.G-color2.G)+Math.Abs(color1.B-color2.B)<=tolerance;
		}
	}
}
