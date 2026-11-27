// Package S3a-M2: the program modules built in memory (never shown, on an STA thread), with their menus checked live: each designer menu is
// a ContextMenuStrip with the items, texts, order, separators, hidden states and click handlers of the WinForms ContextMenu it replaces (the
// lists of M2DesignerMenuTests), each grid that had a caller's ContextMenu shows that strip with the grid's own items
// (DoAddGridItemsToContextMenuStrip), and the menus built in code (ODtextBox's, the patient menu of PatientL, the Family module's grid menu)
// have their items, shortcuts and Opening behaviour. The main window and the Chart and Images modules need a database or a screen to be
// built; their designer menus are checked from the designer text (M2DesignerMenuTests) and their other menus by the launch checks.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental;
using OpenPmsDental.UI;
using OpenPmsDentalBusiness;

namespace S3aM1Tests {
	[TestClass]
	public class M2ModuleMenuTests {
		#region Helpers
		///<summary>Compares each designer menu of the file with the expected list of M2DesignerMenuTests, on the live control.</summary>
		private static void AssertDesignerMenus(Control control,string designerFile,params string[] arrayOpeningInCode) {
			foreach(M2DesignerMenuTests.ExpectedMenu expectedMenu in M2DesignerMenuTests.DictExpected[designerFile]) {
				object menu=Priv.Get(control,expectedMenu.Name);
				Assert.IsInstanceOfType(menu,typeof(ContextMenuStrip),expectedMenu.Name);
				ContextMenuStrip contextMenuStrip=(ContextMenuStrip)menu;
				CollectionAssert.AreEqual(expectedMenu.ArrayItems,M2Support.Describe(contextMenuStrip),
					expectedMenu.Name+": "+string.Join(" | ",M2Support.Describe(contextMenuStrip)));
				//The menu's own Opening, besides the grid's (ContextMenuStrip_Opening), which runs before or after it in the order the designer
				//subscribes them, as the grid's Popup did on the WinForms ContextMenu.
				List<string> listOpening=M2Support.GetOpeningHandlerNames(contextMenuStrip);
				CollectionAssert.AreEqual(expectedMenu.Opening==null ? new List<string>() : new List<string> { expectedMenu.Opening },
					listOpening.Where(x => x!="ContextMenuStrip_Opening" && !arrayOpeningInCode.Contains(x)).ToList(),
					expectedMenu.Name+": its Opening ("+string.Join(", ",listOpening)+").");
				foreach(string opening in arrayOpeningInCode) {
					Assert.IsTrue(listOpening.Contains(opening),expectedMenu.Name+": the Opening added in code, "+opening+".");
				}
			}
		}

		///<summary>The grid shows the caller's strip and adds the grid's own items to it, as it did to the caller's WinForms ContextMenu.</summary>
		private static void AssertGridMenu(Control control,string gridName,string menuName) {
			GridOD gridOD=(GridOD)Priv.Get(control,gridName);
			Assert.AreSame(Priv.Get(control,menuName),gridOD.ContextMenuStrip,gridName);
			Assert.IsTrue(gridOD.DoAddGridItemsToContextMenuStrip,gridName);
#pragma warning disable WFDEV006 //the .NET Framework menu types, kept by .NET 10 for binary compatibility only: checked for absence (S3b-5)
			Assert.IsNull(gridOD.ContextMenu,gridName+": no WinForms ContextMenu.");
#pragma warning restore WFDEV006
			Assert.IsTrue(M2Support.GetOpeningHandlerNames(gridOD.ContextMenuStrip).Contains("ContextMenuStrip_Opening"),gridName+": the grid's Opening.");
		}

		private static void HandlerForTests(object sender,EventArgs e) {
		}
		#endregion Helpers

		#region Modules
		[TestMethod]
		public void ControlAccount_MenusAndGrids() {
			M2Support.RunSta(() => {
				using(ControlAccount controlAccount=new ControlAccount()) {
					AssertDesignerMenus(controlAccount,@"Main Modules\ControlAccount.Designer.cs");
					AssertGridMenu(controlAccount,"gridAccount","contextMenuAcctGrid");
					AssertGridMenu(controlAccount,"gridServiceDateView","contextMenuAcctGrid");//the same strip, as the ContextMenu was
					//The Statement Options window lists the entries that are Available: a ToolStripItem's Visible is false while its menu is not shown.
					ContextMenuStrip contextMenuStatement=(ContextMenuStrip)Priv.Get(controlAccount,"contextMenuStatement");
					Assert.IsTrue(contextMenuStatement.Items.OfType<ToolStripMenuItem>().All(x => !x.Visible && x.Available && x.Tag is StatementChoice));
				}
			});
		}

		[TestMethod]
		public void ControlAppt_MenusAndGrids() {
			M2Support.RunSta(() => {
				using(ControlAppt controlAppt=new ControlAppt()) {
					AssertDesignerMenus(controlAppt,@"Main Modules\ControlAppt.Designer.cs");
					AssertGridMenu(controlAppt,"gridReminders","menuReminderEdit");
				}
			});
		}

		[TestMethod]
		public void ControlTreat_MenusAndGrids() {
			M2Support.RunSta(() => {
				using(ControlTreat controlTreat=new ControlTreat()) {
					AssertDesignerMenus(controlTreat,@"Main Modules\ControlTreat.designer.cs");
					AssertGridMenu(controlTreat,"gridPreAuth","contextMenuPreAuthGrid");
				}
			});
		}

		[TestMethod]
		public void ControlFamily_Menus_AndThePatientGridMenuIsReplacedWhenTheModuleIsLeft() {
			M2Support.RunSta(() => {
				using(ControlFamily controlFamily=new ControlFamily()) {
					AssertDesignerMenus(controlFamily,@"Main Modules\ControlFamily.designer.cs");
					GridOD gridPat=(GridOD)Priv.Get(controlFamily,"gridPat");
					controlFamily.ModuleUnselected();
					ContextMenuStrip contextMenuStripFirst=gridPat.ContextMenuStrip;
					Assert.IsNotNull(contextMenuStripFirst);
					Assert.AreEqual(0,contextMenuStripFirst.Items.Count);
					Assert.IsTrue(gridPat.DoAddGridItemsToContextMenuStrip,"The grid adds Copy Cell Text, Copy Rows and the links, as to the ContextMenu.");
					contextMenuStripFirst.Items.Add(new ToolStripMenuItem("View SS#") { Name="ViewSS#",Visible=false });
					controlFamily.ModuleUnselected();
					Assert.AreNotSame(contextMenuStripFirst,gridPat.ContextMenuStrip,"A new menu: the items the module added are gone.");
					Assert.AreEqual(0,gridPat.ContextMenuStrip.Items.Count);
					Assert.IsTrue(contextMenuStripFirst.IsDisposed,"The old menu is disposed (at once here: the module has no window handle).");
				}
			});
		}

		[TestMethod]
		public void UserControlTasks_MenusAndGrid() {
			M2Support.RunSta(() => {
				using(UserControlTasks userControlTasks=new UserControlTasks()) {
					AssertDesignerMenus(userControlTasks,@"User Controls\UserControlTasks.Designer.cs");
					AssertGridMenu(userControlTasks,"gridMain","menuEdit");
					//Set Priority and Navigate to Attachment are submenus filled when the menu opens.
					ContextMenuStrip menuEdit=(ContextMenuStrip)Priv.Get(userControlTasks,"menuEdit");
					ToolStripMenuItem menuItemPriority=(ToolStripMenuItem)Priv.Get(userControlTasks,"menuItemPriority");
					Assert.AreSame(menuEdit,menuItemPriority.Owner);
					menuItemPriority.DropDownItems.Add("High").Click+=HandlerForTests;
					Assert.AreEqual("Set Priority {High ->HandlerForTests}",M2Support.Describe(menuItemPriority));
				}
			});
		}

		[TestMethod]
		public void EmailPreviewControl_MenuAndGrid() {
			M2Support.RunSta(() => {
				using(EmailPreviewControl emailPreviewControl=new EmailPreviewControl()) {
					AssertDesignerMenus(emailPreviewControl,@"User Controls\EmailPreviewControl.Designer.cs");
					AssertGridMenu(emailPreviewControl,"gridAttachments","contextMenuAttachments");
				}
			});
		}

		[TestMethod]
		public void ControlImageDisplay_PanelMenu() {
			M2Support.RunSta(() => {
				using(ControlImageDisplay controlImageDisplay=new ControlImageDisplay()) {
					//IMG-P2: the constructor adds Teeth after Info in code (the designer file is unchanged). Checked here, then taken out so the
					//rest of the menu is compared with the designer as before.
					ContextMenuStrip contextMenuStrip=(ContextMenuStrip)Priv.Get(controlImageDisplay,"menuPanelMain");
					ToolStripMenuItem menuItemTeeth=(ToolStripMenuItem)Priv.Get(controlImageDisplay,"_menuItemTeeth");
					Assert.AreEqual(contextMenuStrip.Items.IndexOf((ToolStripItem)Priv.Get(controlImageDisplay,"menuItemInfo"))+1,
						contextMenuStrip.Items.IndexOf(menuItemTeeth),"Teeth follows Info.");
					Assert.AreEqual("Teeth...",menuItemTeeth.Text);
					//FB-1: Retake X-ray... is added in code after Teeth, the same way.
					ToolStripMenuItem menuItemRetake=(ToolStripMenuItem)Priv.Get(controlImageDisplay,"_menuItemRetake");
					Assert.AreEqual(contextMenuStrip.Items.IndexOf(menuItemTeeth)+1,contextMenuStrip.Items.IndexOf(menuItemRetake),"Retake follows Teeth.");
					Assert.AreEqual("Retake X-ray...",menuItemRetake.Text);
					contextMenuStrip.Items.Remove(menuItemTeeth);
					contextMenuStrip.Items.Remove(menuItemRetake);
					AssertDesignerMenus(controlImageDisplay,@"User Controls\ControlImageDisplay.Designer.cs","menuPanelMain_OpeningRetake");
					Control panelMain=(Control)Priv.Get(controlImageDisplay,"panelMain");
					Assert.AreSame(Priv.Get(controlImageDisplay,"menuPanelMain"),panelMain.ContextMenuStrip);
#pragma warning disable WFDEV006 //the .NET Framework menu types, kept by .NET 10 for binary compatibility only: checked for absence (S3b-5)
					Assert.IsNull(panelMain.ContextMenu);
#pragma warning restore WFDEV006
				}
			});
		}
		#endregion Modules

		#region ODtextBox
		[TestMethod]
		public void ODtextBox_Menu_SameItemsShortcutsAndIndexes() {
			M2Support.RunSta(() => {
				using(ODtextBox odTextBox=new ODtextBox()) {
					ContextMenuStrip contextMenuStrip=(ContextMenuStrip)Priv.Get(odTextBox,"contextMenu");
					Assert.AreSame(contextMenuStrip,odTextBox.ContextMenuStrip);
#pragma warning disable WFDEV006 //the .NET Framework menu types, kept by .NET 10 for binary compatibility only: checked for absence (S3b-5)
					Assert.IsNull(odTextBox.ContextMenu);
#pragma warning restore WFDEV006
					//The indexes that contextMenu_Popup and menuItem_Click use (0-4 suggestions, 13 cut, 14 copy, 16 paste plain text, 17 edit auto note).
					List<string> listExpected=new List<string> {
						" ->menuItem_Click"," ->menuItem_Click"," ->menuItem_Click"," ->menuItem_Click"," ->menuItem_Click","-",
						"Add to Dictionary ->menuItem_Click","Disable Spell Check ->menuItem_Click","-",
						"Insert Date ->menuItem_Click [Ctrl+D]","Insert Quick Paste Note ->menuItem_Click [Ctrl+Q]","Insert Auto Note ->menuItem_Click","-",
						"Cut ->menuItem_Click [Ctrl+X]","Copy ->menuItem_Click [Ctrl+C]","Paste ->menuItem_Click [Ctrl+V]","Paste Plain Text ->menuItem_Click",
						"Edit Auto Note ->menuItem_Click",
					};
					CollectionAssert.AreEqual(listExpected,M2Support.Describe(contextMenuStrip),string.Join(" | ",M2Support.Describe(contextMenuStrip)));
					Assert.AreEqual("contextMenu_Popup",M2Support.GetOpeningHandlerNames(contextMenuStrip).Single());
				}
			});
		}

		[TestMethod]
		public void ODtextBox_Opening_SetsTheItemsAndAddsTheLinks() {
			M2Support.RunSta(() => {
				using(ODtextBox odTextBox=new ODtextBox()) {
					odTextBox.SpellCheckIsEnabled=false;
					odTextBox.HasAutoNotes=false;
					odTextBox.Text="See www.example.test for the note.";
					ContextMenuStrip contextMenuStrip=(ContextMenuStrip)Priv.Get(odTextBox,"contextMenu");
					for(int i=0;i<2;i++) {
						Assert.IsFalse(M2Support.Open(contextMenuStrip,odTextBox),"Opening "+i+" is not cancelled.");
						List<string> listDescriptions=M2Support.Describe(contextMenuStrip);
						CollectionAssert.AreEqual(new List<string> {
							" ->menuItem_Click (hidden)"," ->menuItem_Click (hidden)"," ->menuItem_Click (hidden)"," ->menuItem_Click (hidden)",
							" ->menuItem_Click (hidden)","-","Add to Dictionary ->menuItem_Click (hidden)","Disable Spell Check ->menuItem_Click (hidden)","-",
							"Insert Date ->menuItem_Click [Ctrl+D]","Insert Quick Paste Note ->menuItem_Click [Ctrl+Q]",
							"Insert Auto Note ->menuItem_Click (hidden) (disabled)","-",
							"Cut ->menuItem_Click [Ctrl+X] (disabled)","Copy ->menuItem_Click [Ctrl+C] (disabled)","Paste ->menuItem_Click [Ctrl+V]",
							"Paste Plain Text ->menuItem_Click (hidden) (disabled)","Edit Auto Note ->menuItem_Click (hidden) (disabled)","-",
						},listDescriptions.Take(19).ToList(),"Opening "+i+": "+string.Join(" | ",listDescriptions));
						//The separators of a strip are ToolStripSeparators; the hidden ones at 5 and 8 stay hidden (the separator items, not "-" texts).
						Assert.IsFalse(contextMenuStrip.Items[5].Available);
						Assert.IsFalse(contextMenuStrip.Items[8].Available);
						Assert.IsInstanceOfType(contextMenuStrip.Items[18],typeof(ToolStripSeparator));
						//The link of the text, once: the links of the previous opening are removed first.
						Assert.AreEqual(1,listDescriptions.Count(x => x.StartsWith("Web - www.example.test")),string.Join(" | ",listDescriptions));
						Assert.AreEqual(20,listDescriptions.Count);
					}
					//A selection enables Cut and Copy.
					odTextBox.SelectionStart=0;
					odTextBox.SelectionLength=3;
					M2Support.Open(contextMenuStrip,odTextBox);
					Assert.IsTrue(contextMenuStrip.Items[13].Enabled && contextMenuStrip.Items[14].Enabled);
				}
			});
		}
		#endregion ODtextBox

		#region PatientL
		[TestMethod]
		public void PatientL_PatientMenu_OnAContextMenuStrip() {
			M2Support.RunSta(() => {
				Patient patient1=new Patient { PatNum=1001,LName="Alpha",FName="Ann",Preferred="",MiddleI="" };
				Patient patient2=new Patient { PatNum=1002,LName="Beta",FName="Bob",Preferred="",MiddleI="" };
				Patient patient3=new Patient { PatNum=1003,LName="Beta",FName="Cy",Preferred="",MiddleI="" };
				using(Control control=new Control())
				using(ContextMenuStrip contextMenuStrip=new ContextMenuStrip()) {
					PatientL.RemoveAllFromMenu(contextMenuStrip);
					//No patient and no history: nothing, and the menu does not open (FormOpenDental.menuPatient_Popup leaves it cancelled).
					PatientL.AddFamilyToMenu(contextMenuStrip,HandlerForTests,0,null);
					Assert.AreEqual(0,contextMenuStrip.Items.Count);
					Assert.IsTrue(M2Support.Open(contextMenuStrip,control));
					Assert.IsTrue(PatientL.AddPatsToMenu(contextMenuStrip,HandlerForTests,patient1));
					Assert.IsFalse(PatientL.AddPatsToMenu(contextMenuStrip,HandlerForTests,patient1),"The same patient again.");
					Assert.IsTrue(PatientL.AddPatsToMenu(contextMenuStrip,HandlerForTests,patient2));
					Family family=new Family(new List<Patient> { patient2,patient3 });
					PatientL.AddFamilyToMenu(contextMenuStrip,HandlerForTests,patient2.PatNum,family);
					CollectionAssert.AreEqual(new List<string> { "Beta, Bob ->HandlerForTests","Alpha, Ann ->HandlerForTests","-","FAMILY",
						"Beta, Bob ->HandlerForTests","Beta, Cy ->HandlerForTests" },M2Support.Describe(contextMenuStrip),
						string.Join(" | ",M2Support.Describe(contextMenuStrip)));
					Assert.AreEqual(patient1.PatNum,PatientL.ButtonSelect(contextMenuStrip,contextMenuStrip.Items[1],family));
					Assert.AreEqual(patient2.PatNum,PatientL.ButtonSelect(contextMenuStrip,contextMenuStrip.Items[4],family));
					Assert.AreEqual(patient3.PatNum,PatientL.ButtonSelect(contextMenuStrip,contextMenuStrip.Items[5],family));
					Assert.IsFalse(M2Support.Open(contextMenuStrip,control));
					PatientL.RemoveAllFromMenu(contextMenuStrip);
					Assert.AreEqual(0,contextMenuStrip.Items.Count);
				}
			});
		}
		#endregion PatientL
	}
}
