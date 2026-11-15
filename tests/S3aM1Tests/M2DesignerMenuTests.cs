// Package S3a-M2: the designer menus of the program modules are ContextMenuStrips with the items of the WinForms ContextMenus they replace.
// The expected lists were written from the designer files on main before the conversion (the WinForms ContextMenu, its MenuItems in the order
// of MenuItems.AddRange, their Text, Click handler, Visible and submenus, and the menu's Popup handler). The test reads each converted designer
// file and describes its ContextMenuStrips the same way, so a window that cannot be built without a database or a screen (the main window,
// the Chart module) is checked too. The modules that can be built in memory are also checked live (M2ModuleMenuTests).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace S3aM1Tests {
	[TestClass]
	public class M2DesignerMenuTests {
		///<summary>A menu of a designer file: its field, its items as M2Support.Describe writes them (without shortcuts, which no designer menu
		///has), and its Popup (now Opening) handler or null.</summary>
		public class ExpectedMenu {
			public string Name;
			public string[] ArrayItems;
			public string Opening;

			public ExpectedMenu(string name,string opening,params string[] arrayItems) {
				Name=name;
				Opening=opening;
				ArrayItems=arrayItems;
			}
		}

		///<summary>The designer files with their menus, from main before S3a-M2.</summary>
		public static readonly Dictionary<string,ExpectedMenu[]> DictExpected=new Dictionary<string,ExpectedMenu[]> {
			{ @"Main Modules\FormOpenDental.Designer.cs",new[] {
				new ExpectedMenu("menuSplitter",null,"Dock to Bottom ->menuItemDockBottom_Click","Dock to Right ->menuItemDockRight_Click"),
				new ExpectedMenu("menuPatient","menuPatient_Popup"),
				new ExpectedMenu("menuLabel","menuLabel_Popup"),
				new ExpectedMenu("menuEmail","menuEmail_Popup"),
				new ExpectedMenu("menuLetter","menuLetter_Popup"),
				new ExpectedMenu("menuTask","menuTask_Popup","New for [User] ->menuItemTaskNewForUser_Click","Reminders ->menuItemTaskReminders_Click"),
			} },
			{ @"Main Modules\ControlAccount.Designer.cs",new[] {
				new ExpectedMenu("contextMenuIns",null,"Primary ->menuInsPri_Click","Secondary ->menuInsSec_Click","Medical ->menuInsMedical_Click",
					"Other ->menuInsOther_Click"),
				new ExpectedMenu("contextMenuStatement",null,"Walkout ->menuItemStatementWalkout_Click","Email ->menuItemStatementEmail_Click",
					"Receipt ->menuItemReceipt_Click","Invoice ->menuItemInvoice_Click","Limited ->menuItemLimited_Click",
					"Limited (Custom) ->menuItemLimitedCustom_Click","More Options ->menuItemStatementMore_Click"),
				new ExpectedMenu("contextMenuRepeat",null,"Standard Monthly ->MenuItemRepeatStand_Click","Email Monthly ->MenuItemRepeatEmail_Click",
					"Canada Monthly ->menuItemRepeatCanada_Click"),
				new ExpectedMenu("contextMenuQuickProcs",null),
				new ExpectedMenu("contextMenuPayment","contextMenuPayment_Popup","Allocate Unearned ->menuItemAllocateUnearned_Click",
					"Income Transfer ->menuItemIncomeTransfer_Click","Get CareCredit Self-Service Link ->menuItemSendCareCreditCSS_Click"),
				new ExpectedMenu("contextMenuPayPlan",null,"Payment Plan ->MenuItemDynamicPayPlan_Click","Insurance Payment Plan ->menuItemInsPayPlan_Click"),
				new ExpectedMenu("contextMenuAdjust",null,"Apply Sales Tax ->menuItemSalesTax_Click","Add Multiple ->menuItemAddMultAdj_Click"),
				new ExpectedMenu("contextMenuAcctGrid","contextMenuAcctGrid_Popup","Add Adjustment ->menuItemAddAdj_Click","Refund ->menuItemAddRefund_Click",
					"Edit Charge ->menuItemEditPayPlanCharge_Click","Delete Charge ->menuItemDeletePayPlanCharge_Click (hidden)",
					"Snip Attachment for DentalXChange ->menuItemSnipAttachment_Click","Select Attachment for DentalXChange ->menuItemSelectImage_Click",
					"Paste Attachment(s) for DentalXChange ->menuItemPasteAttachment_Click",
					"View Sent Attachments for DentalXChange ->menuItemAttachmentHistory_Click"),
			} },
			{ @"Main Modules\ControlChart.designer.cs",new[] {
				new ExpectedMenu("menuProgRight","menuProgRight_Popup","Delete ->menuItemDelete_Click","Set Complete ->menuItemSetComplete_Click",
					"Set Existing Current  ->menuItemSetEC_Click","Set Existing Other ->menuItemSetEO_Click","Edit All ->menuItemEditSelected_Click",
					"Group Note ->menuItemGroupSelected_Click","Print Progress Notes ... ->menuItemPrintProg_Click",
					"Print Day for Hospital ->menuItemPrintDay_Click","Detach Lab Fee ->menuItemLabFeeDetach_Click","Attach Lab Fee ->menuItemLabFee_Click",
					"Print Routing Slip ->menuItemPrintRouteSlip_Click","Group for Multiple Visits ->menuItemGroupMultiVisit_Click",
					"View Multivisit Group(s) ->menuItemViewMultiVisitGroup_Click"),
				new ExpectedMenu("menuConsent","menuConsent_Popup"),
				new ExpectedMenu("menuToothChart","menuToothChart_Popup","Show Big ->menuItemChartBig_Click","Save to Images ->menuItemChartSave_Click"),
				new ExpectedMenu("menuErx",null,"Refresh ->menuItemErxRefresh_Click","Pending Prescriptions ->menuItemDoseSpotPendingPescr_Click",
					"Refill Requests ->menuItemDoseSpotRefillReqs_Click","Transaction Errors ->menuItemDoseSpotTransactionErrors_Click"),
				new ExpectedMenu("_contextMenuRxManage",null,"Rx Manage ->menuItemRxManage_Click"),
				new ExpectedMenu("menuOrthoChart","menuOrthoChart_Popup"),
			} },
			{ @"Main Modules\ControlAppt.Designer.cs",new[] {
				new ExpectedMenu("menuReminderEdit",null,"Done (affects all users) ->menuItemReminderDone_Click","Go To ->menuItemReminderGoto_Click"),
				new ExpectedMenu("menuRecall",null),
			} },
			{ @"Main Modules\ControlFamily.designer.cs",new[] {
				new ExpectedMenu("contextMenuInsurance",null,"Plans for Family ->menuPlansForFam_Click"),
				new ExpectedMenu("contextMenuDiscount",null,"Drop Discount Plan ->menuItemRemoveDiscount_Click"),
			} },
			{ @"Main Modules\ControlTreat.designer.cs",new[] {
				new ExpectedMenu("contextMenuPreAuthGrid","contextMenuPreAuthGrid_Popup","Snip Attachment for DentalXChange ->menuItemSnipAttachment_Click",
					"Select Attachment for DentalXChange ->menuItemSelectImage_Click","Paste Attachment(s) for DentalXChange ->menuItemPasteAttachment_Click",
					"View Sent Attachments for DentalXChange ->menuItemAttachmentHistory_Click"),
				new ExpectedMenu("menuConsent","menuConsent_Popup"),
			} },
			{ @"Main Modules\ControlImages.Designer.cs",new[] {
				new ExpectedMenu("menuForms",null),
				new ExpectedMenu("menuMounts",null),
				new ExpectedMenu("menuTree",null,"Print ->menuTree_Click","Delete ->menuTree_Click","Info ->menuTree_Click","Find Tasks ->menuTree_Click"),
			} },
			{ @"User Controls\ControlImageDisplay.Designer.cs",new[] {
				new ExpectedMenu("menuPanelMain",null,"Copy ->menuPanelMain_Click","Copy without drawings ->menuPanelMain_Click","Paste ->menuPanelMain_Click",
					"Flip Horizontally ->menuPanelMain_Click","Rotate Left ->menuPanelMain_Click","Rotate Right ->menuPanelMain_Click",
					"Rotate 180 ->menuPanelMain_Click","Delete ->menuPanelMain_Click","Info ->menuPanelMain_Click","Find Tasks ->menuPanelMain_Click"),
			} },
			{ @"User Controls\UserControlTasks.Designer.cs",new[] {
				new ExpectedMenu("menuEdit","menuEdit_Popup","Done (affects all users) ->menuItemDone_Click","-","Edit Properties ->menuItemEdit_Click",
					"Set Priority","-","Cut ->menuItemCut_Click","Copy ->menuItemCopy_Click","Paste ->menuItemPaste_Click","Delete ->menuItemDelete_Click","-",
					"Subscribe ->menuItemSubscribe_Click","Unsubscribe ->menuItemUnsubscribe_Click","-","Send to Me ->menuItemSendToMe_Click",
					"Send to Me && Go To ->menuItemSendAndGoto_Click","Go To ->menuItemGoto_Click","Mark as Read ->menuItemMarkRead_Click",
					"Archive ->menuArchive_Click (hidden)","Unarchive ->menuUnarchive_Click (hidden)","Navigate to Attachment"),
				new ExpectedMenu("menuTask",null,"Reminder ->menuItemTaskReminder_Click"),
				new ExpectedMenu("menuFilter",null),
			} },
			{ @"User Controls\EmailPreviewControl.Designer.cs",new[] {
				new ExpectedMenu("contextMenuAttachments","contextMenuAttachments_Popup","Open ->menuItemOpen_Click","Rename ->menuItemRename_Click",
					"Remove ->menuItemRemove_Click"),
			} },
			{ @"User Controls\UserControlSecurityUserGroup.Designer.cs",new[] {
				new ExpectedMenu("contextMenuUsers",null),//"Copy User" is added in its Load (M2ModuleMenuTests)
			} },
		};

		///<summary>The grids of the designer files that had a caller's WinForms ContextMenu, and so the grid's items: now a ContextMenuStrip with
		///DoAddGridItemsToContextMenuStrip.</summary>
		private static readonly (string file,string grid,string menu)[] _arrayGridMenus=new[] {
			(@"Main Modules\ControlAccount.Designer.cs","gridAccount","contextMenuAcctGrid"),
			(@"Main Modules\ControlChart.designer.cs","gridProg","menuProgRight"),
			(@"Main Modules\ControlTreat.designer.cs","gridPreAuth","contextMenuPreAuthGrid"),
		};

		private static string ReadDesigner(string file) {
			return M2Support.ReadRepoFile(new[] { "OpenPmsDental" }.Concat(file.Split('\\')).ToArray());
		}

		///<summary>The items of a menu or submenu of the designer text, described as M2Support.Describe describes a live item.</summary>
		private static List<string> DescribeDesigner(string designer,string name,bool isTopLevel) {
			string collection=isTopLevel ? "Items" : "DropDownItems";
			Match match=Regex.Match(designer,@"this\."+Regex.Escape(name)+@"\."+collection+@"\.AddRange\(new System\.Windows\.Forms\.ToolStripItem\[\] \{(?<items>[^}]*)\}\);");
			if(!match.Success) {
				return new List<string>();
			}
			List<string> listDescriptions=new List<string>();
			foreach(string item in Regex.Matches(match.Groups["items"].Value,@"this\.(\w+)").Cast<Match>().Select(x => x.Groups[1].Value)) {
				if(Regex.IsMatch(designer,@"this\."+Regex.Escape(item)+@" = new System\.Windows\.Forms\.ToolStripSeparator\(\);")) {
					listDescriptions.Add("-");
					continue;
				}
				Assert.IsTrue(Regex.IsMatch(designer,@"this\."+Regex.Escape(item)+@" = new System\.Windows\.Forms\.ToolStripMenuItem\(\);"),item);
				Match matchText=Regex.Match(designer,@"this\."+Regex.Escape(item)+@"\.Text = ""(?<text>(?:[^""\\]|\\.)*)"";");
				string text=matchText.Success ? Regex.Unescape(matchText.Groups["text"].Value) : "";
				List<string> listHandlers=Regex.Matches(designer,@"this\."+Regex.Escape(item)+@"\.Click \+= new System\.EventHandler\(this\.(\w+)\);")
					.Cast<Match>().Select(x => x.Groups[1].Value).ToList();
				if(listHandlers.Count>0) {
					text+=" ->"+string.Join("+",listHandlers);
				}
				if(designer.Contains("this."+item+".Visible = false;")) {
					text+=" (hidden)";
				}
				if(designer.Contains("this."+item+".Enabled = false;")) {
					text+=" (disabled)";
				}
				if(designer.Contains("this."+item+".Checked = true;")) {
					text+=" (checked)";
				}
				List<string> listSub=DescribeDesigner(designer,item,isTopLevel:false);
				if(listSub.Count>0) {
					text+=" {"+string.Join(", ",listSub)+"}";
				}
				listDescriptions.Add(text);
			}
			return listDescriptions;
		}

		[TestMethod]
		public void DesignerMenus_AreContextMenuStrips_WithTheItemsOfTheWinFormsMenus() {
			foreach(KeyValuePair<string,ExpectedMenu[]> keyValuePair in DictExpected) {
				string designer=ReadDesigner(keyValuePair.Key);
				Assert.IsFalse(Regex.IsMatch(designer,@"System\.Windows\.Forms\.(ContextMenu|MenuItem|MainMenu)\(\)"),keyValuePair.Key+": no WinForms menu is made.");
				Assert.IsFalse(Regex.IsMatch(designer,@"\.MenuItems\.|\.Popup \+=|\.Index = "),keyValuePair.Key+": no WinForms menu member is set.");
				List<string> listStrips=Regex.Matches(designer,@"this\.(\w+) = new System\.Windows\.Forms\.ContextMenuStrip\(this\.components\);")
					.Cast<Match>().Select(x => x.Groups[1].Value).ToList();
				foreach(ExpectedMenu expectedMenu in keyValuePair.Value) {
					string where=keyValuePair.Key+" "+expectedMenu.Name;
					Assert.IsTrue(listStrips.Contains(expectedMenu.Name),where+": a ContextMenuStrip in the components container.");
					CollectionAssert.AreEqual(expectedMenu.ArrayItems,DescribeDesigner(designer,expectedMenu.Name,isTopLevel:true),
						where+": "+string.Join(" | ",DescribeDesigner(designer,expectedMenu.Name,isTopLevel:true)));
					List<string> listOpening=Regex.Matches(designer,@"this\."+Regex.Escape(expectedMenu.Name)+@"\.Opening \+= new System\.ComponentModel\.CancelEventHandler\(this\.(\w+)\);")
						.Cast<Match>().Select(x => x.Groups[1].Value).ToList();
					CollectionAssert.AreEqual(expectedMenu.Opening==null ? new List<string>() : new List<string> { expectedMenu.Opening },listOpening,where+": Opening.");
				}
			}
		}

		[TestMethod]
		public void DesignerGrids_WithACallersMenu_KeepTheGridItems() {
			foreach((string file,string grid,string menu) in _arrayGridMenus) {
				string designer=ReadDesigner(file);
				StringAssert.Contains(designer,"this."+grid+".ContextMenuStrip = this."+menu+";",file);
				StringAssert.Contains(designer,"this."+grid+".DoAddGridItemsToContextMenuStrip = true;",file);
				Assert.IsFalse(designer.Contains("this."+grid+".ContextMenu = "),file);
			}
		}

		[TestMethod]
		public void OwnedFiles_UseNoWinFormsMenu() {
			//The files of the S3a-M2 row (and PatientL, whose patient-menu helpers only the main window calls): no WinForms ContextMenu, MenuItem,
			//MainMenu or Menu, no Popup, no MenuItems, and no toolbar DropDownMenu (the bridge that S3a-M3's callers still use).
			string[] arrayFiles=new[] {
				@"Main Modules\FormOpenDental.cs",@"Main Modules\FormOpenDental.Designer.cs",
				@"Main Modules\ControlAccount.cs",@"Main Modules\ControlAccount.Designer.cs",@"Main Modules\ControlAccount.SalesTax.cs",
				@"Main Modules\ControlAccount.ServiceDateView.cs",@"Main Modules\ControlAccount.StatementOptions.cs",
				@"Main Modules\ControlChart.cs",@"Main Modules\ControlChart.designer.cs",@"Main Modules\ControlChart.ChartLetters.cs",
				@"Main Modules\ControlAppt.cs",@"Main Modules\ControlAppt.Designer.cs",
				@"Main Modules\ControlFamily.cs",@"Main Modules\ControlFamily.designer.cs",
				@"Main Modules\ControlTreat.cs",@"Main Modules\ControlTreat.designer.cs",
				@"Main Modules\ControlImages.cs",@"Main Modules\ControlImages.Designer.cs",
				@"User Controls\ControlImageDisplay.cs",@"User Controls\ControlImageDisplay.Designer.cs",
				@"User Controls\UserControlTasks.cs",@"User Controls\UserControlTasks.Designer.cs",
				@"User Controls\EmailPreviewControl.cs",@"User Controls\EmailPreviewControl.Designer.cs",
				@"User Controls\ODtextBox.cs",
				@"User Controls\UserControlSecurityUserGroup.cs",@"User Controls\UserControlSecurityUserGroup.Designer.cs",
				@"Logic\PatientL.cs",
			};
			Regex regexForbidden=new Regex(@"(?<!WpfControls\.UI\.)\b(ContextMenu|MenuItem|MainMenu|Menu)\b|\.MenuItems\b|\.Popup\s*[+-]?=|\.DropDownMenu\b|GetContextMenu\(|\bShortcut\.|this\.Menu\b");
			foreach(string file in arrayFiles) {
				string[] arrayLines=M2Support.ReadRepoFile(new[] { "OpenPmsDental" }.Concat(file.Split('\\')).ToArray()).Split('\n');
				for(int i=0;i<arrayLines.Length;i++) {
					if(Regex.IsMatch(arrayLines[i],@"^\s*#(end)?region\b")) {
						continue;//region names such as "Menu - File"
					}
					string code=Regex.Replace(arrayLines[i],@"//.*$","");//comments may name the old types
					code=Regex.Replace(code,@"""(?:[^""\\]|\\.)*""","\"\"");//and so may strings ("MainMenu" is a translation key)
					Match match=regexForbidden.Match(code);
					Assert.IsFalse(match.Success,file+" line "+(i+1)+": "+arrayLines[i].Trim());
				}
			}
		}
	}
}
