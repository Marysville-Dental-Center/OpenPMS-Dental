// The other menu parts of package S3a-M1, built in memory and never shown: PopupHelper's links as ToolStripMenuItems (the same texts and order
// as the MenuItems of the bridge), the toolbar button's drop-down members, ODR's ComboBoxMulti list on a ContextMenuStrip, and the sheet combo
// box texts that SheetFields escapes for a menu.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ODR;
using OpenPmsDental.UI;
using OpenPmsDentalBusiness;
using OpenPmsDentalBusiness.UI;

namespace S3aM1Tests {
	[TestClass]
	public class MenuPartsTests {
		[TestInitialize]
		public void TestInitialize() {
			S3aM1Db.SetCultureUs();
		}

		#region PopupHelper
		[TestMethod]
		public void PopupHelper_ToolStripLinks_SameTextsAndOrderAsTheMenuItemLinks() {
			string text="Notes: www.example.test and https://portal.example.test/a, also (www.b.example.test). patnum:42 tasknum:7 PatNum:5";
			foreach(bool isRightClickLinks in new[] { false,true }) {
				List<ToolStripMenuItem> listToolStripMenuItems=PopupHelper.GetToolStripMenuItemLinks(text,isRightClickLinks);
				List<MenuItem> listMenuItems=PopupHelper.GetContextMenuItemLinks(text,isRightClickLinks);
				CollectionAssert.AreEqual(listMenuItems.Select(x => x.Text).ToList(),listToolStripMenuItems.Select(x => x.Text).ToList());
				Assert.IsTrue(listToolStripMenuItems.All(x => x.Enabled && x.Available && x.DropDownItems.Count==0));
				List<string> listTexts=listToolStripMenuItems.Select(x => x.Text).ToList();
				CollectionAssert.AreEqual(listTexts.OrderBy(x => x).ToList(),listTexts,"Sorted, as before.");
				Assert.IsTrue(listTexts.Contains("Web - www.example.test"));
				Assert.IsTrue(listTexts.Contains("Web - https://portal.example.t..."),"Cut after 24 characters: "+string.Join("; ",listTexts));
				Assert.AreEqual(isRightClickLinks,listTexts.Contains("PatNum - 42"));
				Assert.AreEqual(isRightClickLinks,listTexts.Contains("PatNum - 5"));
				Assert.AreEqual(isRightClickLinks,listTexts.Contains("TaskNum - 7"));
			}
			Assert.AreEqual(0,PopupHelper.GetToolStripMenuItemLinks("no links here",true).Count);
		}
		#endregion PopupHelper

		#region ODToolBarButton
		[TestMethod]
		public void ToolBarButton_DropDown_StripIsTheMember_MenuBridgeIsGone() {
			Assert.AreEqual(typeof(ContextMenuStrip),typeof(ODToolBarButton).GetField(nameof(ODToolBarButton.ContextMenuStripDropDown)).FieldType);
			Assert.IsNull(typeof(ODToolBarButton).GetField("DropDownMenu"),"S3a-M3 removed the WinForms bridge: every caller sets ContextMenuStripDropDown.");
			using(ContextMenuStrip contextMenuStrip=new ContextMenuStrip())
			using(ToolBarOD toolBarOD=new ToolBarOD()) {
				contextMenuStrip.Items.Add(new ToolStripMenuItem("Send"));
				contextMenuStrip.Items.Add(new ToolStripMenuItem("Direct Messaging"));
				ODToolBarButton odToolBarButton=new ODToolBarButton("Send",null);
				odToolBarButton.Style=ODToolBarButtonStyle.DropDownButton;
				odToolBarButton.ContextMenuStripDropDown=contextMenuStrip;
				toolBarOD.Buttons.Add(odToolBarButton);
				Assert.AreSame(contextMenuStrip,toolBarOD.Buttons[0].ContextMenuStripDropDown);
			}
		}
		#endregion ODToolBarButton

		#region ComboBoxMulti
		private static ContextMenuStrip MakeComboMenu(ComboBoxMulti comboBoxMulti) {
			ContextMenuStrip contextMenuStrip=(ContextMenuStrip)Priv.Call(comboBoxMulti,"MakeMenu");
			Priv.Set(comboBoxMulti,"cMenu",contextMenuStrip);
			return contextMenuStrip;
		}

		[TestMethod]
		public void ComboBoxMulti_List_HasOneOwnerPaintedItemPerEntry() {
			using(ComboBoxMulti comboBoxMulti=new ComboBoxMulti()) {
				comboBoxMulti.Width=120;
				comboBoxMulti.Items.AddRange(new[] { "Active","Inactive","A & B" });
				ContextMenuStrip contextMenuStrip=MakeComboMenu(comboBoxMulti);
				Assert.IsFalse(contextMenuStrip.ShowImageMargin);
				Assert.IsFalse(contextMenuStrip.ShowCheckMargin);
				CollectionAssert.AreEqual(new[] { "Active","Inactive","A & B" },contextMenuStrip.Items.Cast<ToolStripItem>().Select(x => x.Text).ToArray());
				foreach(ToolStripItem toolStripItem in contextMenuStrip.Items) {
					Assert.IsInstanceOfType(toolStripItem,typeof(ToolStripMenuItem));
					Assert.IsFalse(toolStripItem.AutoSize);
					Assert.AreEqual(new Size(120-18,14),toolStripItem.Size,"The size MeasureItem gave the owner-drawn MenuItem.");
				}
			}
		}

		[TestMethod]
		public void ComboBoxMulti_Click_TogglesTheEntry_FillsTheText_AndKeepsTheListOpen() {
			using(ComboBoxMulti comboBoxMulti=new ComboBoxMulti()) {
				comboBoxMulti.Items.AddRange(new[] { "Active","Inactive","Archived" });
				ContextMenuStrip contextMenuStrip=MakeComboMenu(comboBoxMulti);
				TextBox textMain=(TextBox)Priv.Get(comboBoxMulti,"textMain");
				contextMenuStrip.Items[2].PerformClick();
				contextMenuStrip.Items[0].PerformClick();
				CollectionAssert.AreEqual(new ArrayList { 2,0 },comboBoxMulti.SelectedIndices);
				Assert.AreEqual("Archived OR Active",textMain.Text);
				contextMenuStrip.Items[2].PerformClick();
				CollectionAssert.AreEqual(new ArrayList { 0 },comboBoxMulti.SelectedIndices);
				Assert.AreEqual("Active",textMain.Text);
				//The list stays open after a click (the ContextMenu showed itself again); any other close goes through.
				ToolStripDropDownClosingEventArgs closingEventArgs=new ToolStripDropDownClosingEventArgs(ToolStripDropDownCloseReason.ItemClicked);
				Priv.Call(comboBoxMulti,"cMenu_Closing",contextMenuStrip,closingEventArgs);
				Assert.IsTrue(closingEventArgs.Cancel);
				foreach(ToolStripDropDownCloseReason closeReason in new[] { ToolStripDropDownCloseReason.AppClicked,ToolStripDropDownCloseReason.AppFocusChange,
					ToolStripDropDownCloseReason.Keyboard,ToolStripDropDownCloseReason.CloseCalled })
				{
					closingEventArgs=new ToolStripDropDownClosingEventArgs(closeReason);
					Priv.Call(comboBoxMulti,"cMenu_Closing",contextMenuStrip,closingEventArgs);
					Assert.IsFalse(closingEventArgs.Cancel,closeReason.ToString());
				}
			}
		}

		[TestMethod]
		public void ComboBoxMulti_Paint_SelectedEntryOnHighlight_OthersOnWindow() {
			using(ComboBoxMulti comboBoxMulti=new ComboBoxMulti()) {
				comboBoxMulti.Items.AddRange(new[] { "Active","Inactive" });
				comboBoxMulti.SetSelected(1,true);
				ContextMenuStrip contextMenuStrip=MakeComboMenu(comboBoxMulti);
				for(int i=0;i<2;i++) {
					ToolStripItem toolStripItem=contextMenuStrip.Items[i];
					using(Bitmap bitmap=new Bitmap(toolStripItem.Width,toolStripItem.Height))
					using(Graphics g=Graphics.FromImage(bitmap)) {
						g.Clear(Color.Magenta);
						Priv.Call(comboBoxMulti,"MenuItem_Paint",toolStripItem,new PaintEventArgs(g,new Rectangle(Point.Empty,toolStripItem.Size)));
						Color colorExpected=(i==1 ? SystemColors.Highlight : SystemColors.Window);
						Color colorCorner=bitmap.GetPixel(toolStripItem.Width-1,toolStripItem.Height-1);//text never reaches the corner
						Assert.AreEqual(colorExpected.ToArgb(),colorCorner.ToArgb(),"Entry "+i);
					}
				}
			}
		}
		#endregion ComboBoxMulti

		#region SheetFields combo texts
		[TestMethod]
		public void SheetCombo_EscapedTexts_AreMenuItemsNotSeparators_AndRoundTrip() {
			SheetField sheetField=new SheetField();
			sheetField.FieldValue="B;A&B|-|C";
			List<string> listItems=SheetFields.GetComboMenuItems(sheetField);
			CollectionAssert.AreEqual(new List<string> { "A&&B","&-","C" },listItems);
			using(ContextMenuStrip contextMenuStrip=new ContextMenuStrip()) {
				//Whether a converted form adds them as ToolStripMenuItems or with Items.Add(text), none is a separator.
				foreach(string item in listItems) {
					contextMenuStrip.Items.Add(new ToolStripMenuItem(item));
					contextMenuStrip.Items.Add(item);
				}
				Assert.IsFalse(contextMenuStrip.Items.OfType<ToolStripSeparator>().Any());
				//Unescaped, "-" would have been one (the reason SheetFields escapes it).
				Assert.IsInstanceOfType(contextMenuStrip.Items.Add("-"),typeof(ToolStripSeparator));
			}
			Dictionary<string,string> dictExpected=new Dictionary<string,string> { { "A&&B","A&B" },{ "&-","-" },{ "C","C" } };
			foreach(string item in listItems) {
				ToolStripMenuItem toolStripMenuItem=new ToolStripMenuItem(item);
				sheetField.FieldValue="B;A&B|-|C";
				SheetFields.SetComboFieldValue(sheetField,toolStripMenuItem.Text);
				Assert.AreEqual(dictExpected[item]+";A&B|-|C",sheetField.FieldValue);
			}
		}
		#endregion SheetFields combo texts
	}
}
