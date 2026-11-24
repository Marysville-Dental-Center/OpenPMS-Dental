// Package S3a-M3a: the remaining forms' designer menus (Forms, Forms Reports, ReportingForRdl, CentralManager) are ContextMenuStrips (CentralManager's
// main menu a MenuStrip) with the items of the WinForms ContextMenus and MainMenu they replace. The expected lists were written from the designer
// files on main before the conversion (the WinForms menu, its MenuItems in the order of MenuItems.AddRange, their Text, Click handler, Visible,
// Enabled, Checked and submenus, and the menu's Popup handler), the way M2DesignerMenuTests does for the program modules. The grids that had a
// caller's WinForms ContextMenu (in the designer or in code) keep the grid's own items (DoAddGridItemsToContextMenuStrip).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace S3aM1Tests {
	[TestClass]
	public class M3DesignerMenuTests {
		///<summary>A menu of a designer file: its field, its items, and its Popup (now Opening) handler or null.</summary>
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

		///<summary>The designer files with their menus, from main before S3a-M3a. Paths from the repository root.</summary>
		public static readonly Dictionary<string,ExpectedMenu[]> DictExpected=new Dictionary<string,ExpectedMenu[]> {
			{ @"CentralManager\FormCentralManager.Designer.cs",new[] {
				new ExpectedMenu("mainMenu",null,"Logoff ->menuItemLogoff_Click","File {Change Password ->menuItemPassword_Click, User Settings ->menuItemUserSettings_Click (hidden)}","Setup {Connections ->menuConnSetup_Click, Display Fields ->menuItemDisplayFields_Click, Groups ->menuGroups_Click, Report Permissions ->menuItemReportSetup_Click, Security ->menuItemSecurity_Click}","Reports {Production and Income ->menuProdInc_Click}","Transfer {Patient ->menuTransferPatient_Click}","Help {Query Monitor ->menuItemQueryMonitor_Click, Payload Monitor ->menuItemPayloadMonitor_Click}"),
			} },
			{ @"OpenPmsDental\Forms Reports\FormRpDPPOvercharged.designer.cs",new[] {
				new ExpectedMenu("contextMenuGrid",null,"Go To Account ->menuItemGridGoToAccount_Click"),
			} },
			{ @"OpenPmsDental\Forms Reports\FormRpInsPendingSupplementals.designer.cs",new[] {
				new ExpectedMenu("contextMenuGrid",null,"Go to Account ->menuItemGoToAccount_Click"),
			} },
			{ @"OpenPmsDental\Forms Reports\FormRpProcNotBilledIns.designer.cs",new[] {
				new ExpectedMenu("contextMenuGrid",null,"Go To Account ->menuItemGridGoToAccount_Click"),
			} },
			{ @"OpenPmsDental\Forms Reports\FormRpProcOverpaid.designer.cs",new[] {
				new ExpectedMenu("contextMenuGrid",null,"Go To Account ->menuItemGridGoToAccount_Click"),
			} },
			{ @"OpenPmsDental\Forms Reports\FormRpProcedures.designer.cs",new[] {
				new ExpectedMenu("contextMenuGrid",null,"Go to Patient ->menuItemGoToPatient_Click"),
			} },
			{ @"OpenPmsDental\Forms Reports\FormRpTreatmentFinder.designer.cs",new[] {
				new ExpectedMenu("contextRightClick",null,"See Family ->menuItemFamily_Click","See Account ->menuItemAccount_Click"),
			} },
			{ @"OpenPmsDental\Forms\FormApptConflicts.Designer.cs",new[] {
				new ExpectedMenu("contextRightClick",null,"Select Patient ->menuItemSelectPatient_Click","Send to Pinboard ->menuItemPin_Click"),
			} },
			{ @"OpenPmsDental\Forms\FormApptEdit.designer.cs",new[] {
				new ExpectedMenu("contextMenuTimeArrived",null,"Now ->menuItemArrivedNow_Click"),
				new ExpectedMenu("contextMenuTimeSeated",null,"Now ->menuItemSeatedNow_Click"),
				new ExpectedMenu("contextMenuTimeDismissed",null,"Now ->menuItemDismissedNow_Click"),
			} },
			{ @"OpenPmsDental\Forms\FormClaimAttachPasteDXC.designer.cs",new[] {
				new ExpectedMenu("contextMenuImageGrid","contextMenuImageGrid_Popup","Referral Form ->menuItemReferralForm_Click","Diagnostic Report ->menuItemDiagnosticReport_Click","Explanation of Benefits ->menuItemExplanationOfBenefits_Click","Other Attachments ->menuItemOtherAttachments_Click","Periodontal Charts ->menuItemPeriodontalCharts_Click","X-Rays ->menuItemXRays_Click","Dental Models ->menuItemDentalModels_Click","Radiology Reports ->menuItemRadiologyReports_Click","Intra-Oral Photograph ->menuItemIntraOralPhotograph_Click","Narrative ->menuItemNarrative_Click"),
			} },
			{ @"OpenPmsDental\Forms\FormClaimEdit.designer.cs",new[] {
				new ExpectedMenu("contextMenuAttachments","contextMenuAttachments_Popup","Open ->menuItemOpen_Click","Rename ->menuItemRename_Click","Remove ->menuItemRemove_Click"),
				new ExpectedMenu("contextAdjust","contextAdjust_Popup","Add Adjustment ->menuItemAddAdj_Click (disabled)"),
			} },
			{ @"OpenPmsDental\Forms\FormClaimPayBatch.designer.cs",new[] {
				new ExpectedMenu("menuRightAttached",null,"Go to Account ->menuItemGotoAccount_Click"),
				new ExpectedMenu("menuRightOut",null,"Go to Account ->menuItemGotoOut_Click"),
			} },
			{ @"OpenPmsDental\Forms\FormClaimsSend.designer.cs",new[] {
				new ExpectedMenu("contextMenuStatus",null),
				new ExpectedMenu("contextMenuEclaims",null),
				new ExpectedMenu("contextMenuHistory",null,"Go To Account ->menuItemHistoryGoToAccount_Click"),
			} },
			{ @"OpenPmsDental\Forms\FormEmailEdit.designer.cs",new[] {
				new ExpectedMenu("menuAutographDropdown",null),
			} },
			{ @"OpenPmsDental\Forms\FormEmailTemplateEdit.designer.cs",new[] {
				new ExpectedMenu("contextMenuAttachments","contextMenuAttachments_Popup","Open ->menuItemOpen_Click","Rename ->menuItemRename_Click","Remove ->menuItemRemove_Click"),
			} },
			{ @"OpenPmsDental\Forms\FormLabCases.designer.cs",new[] {
				new ExpectedMenu("contextMenu1",null,"Go To Appointment ->menuItemGoTo_Click"),
			} },
			{ @"OpenPmsDental\Forms\FormPayment.designer.cs",new[] {
				new ExpectedMenu("contextMenuXcharge",null,"Settings ->menuXcharge_Click"),
				new ExpectedMenu("contextMenuPayConnect",null,"Settings ->menuPayConnect_Click"),
				new ExpectedMenu("contextMenuPaySimple",null,"Settings ->menuPaySimple_Click","Check ACH status ->menuPaySimpleCheckAch_Click (hidden)"),
			} },
			{ @"OpenPmsDental\Forms\FormRadOrderList.designer.cs",new[] {
				new ExpectedMenu("menuRightClick",null,"See Chart ->butGotoChart_Click","See Family ->butGotoFamily_Click"),
			} },
			{ @"OpenPmsDental\Forms\FormRecallList.designer.cs",new[] {
				new ExpectedMenu("menuRightClick","gridContextMenu_Popup"),
			} },
			{ @"OpenPmsDental\Forms\FormRecurringChargesHistory.designer.cs",new[] {
				new ExpectedMenu("contextMenu","contextMenu_Popup","Go To Account ->menuItemGoTo_Click","Open Payment ->menuItemOpenPayment_Click","View Error Message ->menuItemViewError_Click","Delete Pending Charge ->menuItemDeletePending_Click"),
			} },
			{ @"OpenPmsDental\Forms\FormScreenGroupEdit.designer.cs",new[] {
				new ExpectedMenu("patContextMenu",null,"Unknown ->patContextMenuItem_Click","Allowed ->patContextMenuItem_Click","NoPermission ->patContextMenuItem_Click","Refused ->patContextMenuItem_Click","Absent ->patContextMenuItem_Click","Behavior ->patContextMenuItem_Click","Other ->patContextMenuItem_Click"),
			} },
			{ @"OpenPmsDental\Forms\FormWiki.designer.cs",new[] {
				new ExpectedMenu("menuHomeDropDown",null,"Save As Home Page ->menuItemHomePageSave_Click"),
			} },
			{ @"OpenPmsDental\ReportingForRdl\FormReportCustom.designer.cs",new[] {
				new ExpectedMenu("menuScrollMode",null,"Continuous ->menuItemContinuous_Click","Continuous Facing ->menuItemContinuousFacing_Click","Facing ->menuItemFacing_Click","Single Page ->menuItemSinglePage_Click"),
			} },
			{ @"OpenPmsDental\ReportingForRdl\FormReportForRdl.designer.cs",new[] {
				new ExpectedMenu("menuScrollMode",null,"Continuous ->menuItemContinuous_Click","Continuous Facing ->menuItemContinuousFacing_Click","Facing ->menuItemFacing_Click","Single Page ->menuItemSinglePage_Click"),
			} },
		};

		///<summary>The grids that had a caller's WinForms ContextMenu on main, set in code (file, grid): they set ContextMenuStrip and
		///DoAddGridItemsToContextMenuStrip now. FormClaimAttachPasteDXC's gridMain set it in its designer (DesignerGrids test).</summary>
		public static readonly (string file,string grid)[] ArrayCodeGrids=new[] {
			(@"OpenPmsDental\Forms Reports\FormRpClaimNotSent.cs","gridMain"),(@"OpenPmsDental\Forms Reports\FormRpDPPOvercharged.cs","gridMain"),
			(@"OpenPmsDental\Forms Reports\FormRpInsPendingSupplementals.cs","gridMain"),(@"OpenPmsDental\Forms Reports\FormRpOutstandingIns.cs","gridMain"),
			(@"OpenPmsDental\Forms Reports\FormRpProcNotBilledIns.cs","gridMain"),(@"OpenPmsDental\Forms Reports\FormRpProcOverpaid.cs","gridMain"),
			(@"OpenPmsDental\Forms Reports\FormRpProcedures.cs","gridMain"),(@"OpenPmsDental\Forms Reports\FormRpTreatmentFinder.cs","gridMain"),
			(@"OpenPmsDental\Forms\FormApptConflicts.cs","gridConflicts"),(@"OpenPmsDental\Forms\FormClaimEdit.cs","gridProc"),
			(@"OpenPmsDental\Forms\FormClaimPayBatch.cs","gridAttached"),(@"OpenPmsDental\Forms\FormClaimPayBatch.cs","gridOut"),
			(@"OpenPmsDental\Forms\FormClaimsSend.cs","gridMain"),(@"OpenPmsDental\Forms\FormClaimsSend.cs","gridHistory"),
			(@"OpenPmsDental\Forms\FormEmailTemplateEdit.cs","gridAttachments"),(@"OpenPmsDental\Forms\FormEtrans835Edit.cs","gridClaimDetails"),
			(@"OpenPmsDental\Forms\FormInsVerificationList.cs","gridMain"),(@"OpenPmsDental\Forms\FormInsVerificationList.cs","gridPastDue"),
			(@"OpenPmsDental\Forms\FormInsVerificationList.cs","gridAssignStandard"),(@"OpenPmsDental\Forms\FormInsVerificationList.cs","gridAssignMedicaid"),
			(@"OpenPmsDental\Forms\FormLabCases.cs","gridMain"),(@"OpenPmsDental\Forms\FormPayPlanDynamic.cs","this.gridCharges"),
			(@"OpenPmsDental\Forms\FormProcCodes2.cs","gridMain"),(@"OpenPmsDental\Forms\FormQueryMonitor.cs","gridFeed"),
			(@"OpenPmsDental\Forms\FormRadOrderList.cs","gridMain"),(@"OpenPmsDental\Forms\FormRecallList.cs","gridRecalls"),
			(@"OpenPmsDental\Forms\FormRecallList.cs","gridReminders"),(@"OpenPmsDental\Forms\FormRecallList.cs","gridReactivations"),
			(@"OpenPmsDental\Forms\FormRecurringChargesHistory.cs","gridMain"),(@"OpenPmsDental\Forms\FormRpEraAutoProcessed.cs","gridEras"),
			(@"OpenPmsDental\Forms\FormRpEraAutoProcessed.cs","gridClaims"),(@"OpenPmsDental\Forms\FormScreenGroupEdit.cs","gridScreenPats"),
			(@"OpenPmsDental\Forms\FormTaskSearch.cs","gridTasks"),(@"OpenPmsDental\Forms\FormTreatPlanCurEdit.cs","grid"),
			(@"OpenPmsDental\Forms\FormUserQuery.cs","_gridResults"),
		};

		private static string ReadFile(string file) {
			return M2Support.ReadRepoFile(file.Split('\\'));
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
		public void DesignerMenus_AreToolStripMenus_WithTheItemsOfTheWinFormsMenus() {
			Assert.AreEqual(24,DictExpected.Count,"Every designer file with a WinForms menu on main (FormMedical's designer has none).");
			foreach(KeyValuePair<string,ExpectedMenu[]> keyValuePair in DictExpected) {
				string designer=ReadFile(keyValuePair.Key);
				Assert.IsFalse(Regex.IsMatch(designer,@"System\.Windows\.Forms\.(ContextMenu|MenuItem|MainMenu)\b(?!Strip)"),keyValuePair.Key+": no WinForms menu type.");
				Assert.IsFalse(Regex.IsMatch(designer,@"\.MenuItems\.|\.Popup ?\+=|\.Index = |this\.Menu = |\.ContextMenu = "),keyValuePair.Key+": no WinForms menu member is set.");
				List<string> listStrips=Regex.Matches(designer,@"this\.(\w+) = new System\.Windows\.Forms\.(?:ContextMenuStrip\(this\.components\)|MenuStrip\(\));")
					.Cast<Match>().Select(x => x.Groups[1].Value).ToList();
				Assert.IsTrue(designer.Contains("this.components = new System.ComponentModel.Container();"),keyValuePair.Key+": the components container.");
				foreach(ExpectedMenu expectedMenu in keyValuePair.Value) {
					string where=keyValuePair.Key+" "+expectedMenu.Name;
					Assert.IsTrue(listStrips.Contains(expectedMenu.Name),where+": a ContextMenuStrip in the components container, or a MenuStrip.");
					CollectionAssert.AreEqual(expectedMenu.ArrayItems,DescribeDesigner(designer,expectedMenu.Name,isTopLevel:true),
						where+": "+string.Join(" | ",DescribeDesigner(designer,expectedMenu.Name,isTopLevel:true)));
					List<string> listOpening=Regex.Matches(designer,@"this\."+Regex.Escape(expectedMenu.Name)
						+@"\.Opening ?\+= ?(?:new System\.ComponentModel\.CancelEventHandler\(this\.)?(\w+)\)?;").Cast<Match>().Select(x => x.Groups[1].Value).ToList();
					CollectionAssert.AreEqual(expectedMenu.Opening==null ? new List<string>() : new List<string> { expectedMenu.Opening },listOpening,where+": Opening.");
				}
			}
		}

		[TestMethod]
		public void DesignerGrids_WithACallersMenu_KeepTheGridItems() {
			string designer=ReadFile(@"OpenPmsDental\Forms\FormClaimAttachPasteDXC.designer.cs");
			StringAssert.Contains(designer,"this.gridMain.ContextMenuStrip = this.contextMenuImageGrid;");
			StringAssert.Contains(designer,"this.gridMain.DoAddGridItemsToContextMenuStrip = true;");
		}

		[TestMethod]
		public void CodeGrids_WithACallersMenu_KeepTheGridItems() {
			foreach((string file,string grid) in ArrayCodeGrids) {
				string code=ReadFile(file);
				Assert.IsFalse(Regex.IsMatch(code,Regex.Escape(grid)+@"\.ContextMenu\s*="),file+" "+grid+": no WinForms ContextMenu.");
				StringAssert.Contains(code,grid+".ContextMenuStrip=",file);
				StringAssert.Contains(code,grid+".DoAddGridItemsToContextMenuStrip=true;",file);
			}
			//The grids that set a null ContextMenu (the bridge then made an empty one with the grid's items) set a null ContextMenuStrip, so the grid
			//makes its own strip with the same items.
			StringAssert.Contains(ReadFile(@"OpenPmsDental\Forms\FormClaimPayBatch.cs"),"gridAttached.ContextMenuStrip=null;");
			StringAssert.Contains(ReadFile(@"OpenPmsDental\Forms\FormTreatPlanCurEdit.cs"),"gridAll.ContextMenuStrip=null;");
		}

		[TestMethod]
		public void ToolbarDropDowns_UseContextMenuStripDropDown() {
			foreach(string file in new[] { @"OpenPmsDental\Forms\FormCareCreditWeb.cs",@"OpenPmsDental\Forms\FormClaimsSend.cs",@"OpenPmsDental\Forms\FormEmailEdit.cs",
				@"OpenPmsDental\Forms\FormEmailMessageEdit.cs",@"OpenPmsDental\Forms\FormErx.cs",@"OpenPmsDental\Forms\FormWebView.cs",@"OpenPmsDental\Forms\FormWiki.cs",
				@"OpenPmsDental\Logic\ProgramL.cs" })
			{
				string code=ReadFile(file);
				Assert.IsFalse(Regex.IsMatch(Regex.Replace(code,@"//.*",""),@"\.DropDownMenu\b"),file+": no DropDownMenu (removed in S3a-M3).");
				Assert.IsTrue(code.Contains(".ContextMenuStripDropDown="),file);
			}
		}

		[TestMethod]
		public void CentralManager_MainMenuIsAMenuStrip_AboveTheControls() {
			string designer=ReadFile(@"CentralManager\FormCentralManager.Designer.cs");
			StringAssert.Contains(designer,"this.Controls.Add(this.mainMenu);");
			StringAssert.Contains(designer,"this.MainMenuStrip = this.mainMenu;");
			Assert.IsFalse(designer.Contains("this.Menu = "),"No WinForms MainMenu on the form.");
			Assert.IsFalse(ReadFile(@"CentralManager\FormCentralManager.cs").Contains("this.Menu=mainMenu;"));
			//The menu strip takes 24 pixels of the client area that the MainMenu took outside it: the controls stand below it. The exact positions are
			//UI-12 slice 12's layout grid, so the test checks that every top-level control is under the strip, not the numbers.
			StringAssert.Contains(designer,"this.ClientSize = new System.Drawing.Size(1834, 953);");
			StringAssert.Contains(designer,"this.mainMenu.Size = new System.Drawing.Size(1834, 24);");
			foreach(string control in new[] { "groupBox1","gridConns","gridPats","comboConnectionGroups","label1","labelVersion","butRefreshStatuses","groupBox2" }) {
				Match match=Regex.Match(designer,@"this\."+control+@"\.Location = new System\.Drawing\.Point\(\d+, (\d+)\);");
				Assert.IsTrue(match.Success,control+" has a location.");
				Assert.IsTrue(int.Parse(match.Groups[1].Value)>=24,control+" stands below the 24 px menu strip (y "+match.Groups[1].Value+").");
			}
		}
	}
}
