// UI-12 slice 14 (docs/port-notes/UI-12.md, "Slice 14"): the gallery states ("<window>#<state>", FitGallery.FillSampleState) of the windows
// whose code shows or lays out their parts at run time: the task list's date tabs, the sheet check box field's field types, the update
// window's panels, the user query's report and a dragged split, the progress window's bars, the Family module's super family and clones
// pane. Each state sets what the window's own code sets (its Visible flags, its tab, its split) and calls the window's own layout code where
// it has one; harness only, no database.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace UiRefreshTests {
	public static class Slice14States {
		private const BindingFlags FLAGS=BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public;

		///<summary>The controls FormSheetFieldCheckBox.listFields_SelectedIndexChanged hides before it shows a field type's.</summary>
		private static readonly string[] _arraySheetCheckHidden=new string[] { "labelMiscInstructions","labelReportableName","textReportableName",
			"groupRadio","groupRadioMisc","labelUiLabelMobileCheckBoxNonMisc","textUiLabelMobileCheckBoxNonMisc","labelRequired","checkRequired",
			"labelMedical","listMedical","radioYes","radioNo","labelYesNo","butAddAllergy","butAddProblem","labelMobileCheckOverride",
			"textMobileCheckOverride","labelMobileMedicalNameOverride","textMobileMedicalNameOverride" };

		///<summary>The states of the slice's windows; null for a window or state of another slice.</summary>
		public static Control FillState(Control root,string window,string state) {
			switch(window+"#"+state) {
				case "OpenPmsDental.FormTasks#date":
				case "OpenPmsDental.UserControlTasks#date":
					//The By Date tab: the calendar under the tabs, the tree and grid under it (LayoutTreeAndGrid).
					Control userControlTasks=window.EndsWith("FormTasks") ? Find(root,"userControlTasks1") : root;
					SelectTab(userControlTasks,"tabControl","tabDate");
					userControlTasks.GetType().GetMethod("LayoutTreeAndGrid",FLAGS).Invoke(userControlTasks,null);
					return root;
				case "OpenPmsDental.FormSheetFieldCheckBox#radio":
					//A check box of a radio group (not misc): the group's items list, its mobile captions, Required (a mobile sheet).
					return ShowSheetCheck(root,"groupRadio","labelRequired","checkRequired");
				case "OpenPmsDental.FormSheetFieldCheckBox#misc":
					return ShowSheetCheck(root,"labelMiscInstructions","labelReportableName","textReportableName","groupRadioMisc","labelRequired","checkRequired");
				case "OpenPmsDental.FormSheetFieldCheckBox#allergy":
					Find(root,"labelMedical").Text="Allergies";//as the handler sets it
					return ShowSheetCheck(root,"labelRequired","checkRequired","labelMedical","listMedical","radioYes","radioNo","labelYesNo","butAddAllergy",
						"labelMobileCheckOverride","textMobileCheckOverride","labelMobileMedicalNameOverride","textMobileMedicalNameOverride");
				case "OpenPmsDental.FormSheetFieldCheckBox#problem":
					Find(root,"butAddProblem").Location=Find(root,"butAddAllergy").Location;//as the handler places it
					Find(root,"labelMedical").Text="Problems";
					return ShowSheetCheck(root,"labelRequired","checkRequired","labelMedical","listMedical","radioYes","radioNo","labelYesNo","butAddProblem",
						"labelMobileCheckOverride","textMobileCheckOverride","labelMobileMedicalNameOverride","textMobileMedicalNameOverride");
				case "OpenPmsDental.FormSheetFieldCheckBox#single":
					//A check box with no radio values (addressAndHmPhoneIsSameEntireFamily) on a mobile sheet: its mobile caption only, on Required's
					//row as the handler places it (fix round: Required is hidden for it).
					ShowSheetCheck(root,"labelUiLabelMobileCheckBoxNonMisc","textUiLabelMobileCheckBoxNonMisc");
					int topRequired=Find(root,"checkRequired").Top;
					foreach(string name in new[] { "labelUiLabelMobileCheckBoxNonMisc","textUiLabelMobileCheckBoxNonMisc" }) {
						Control control=Find(root,name);
						control.Location=new Point(control.Left,topRequired);
					}
					return root;
				case "OpenPmsDental.FormUpdate#load":
					//What the fork's Load shows: the message, the window a padding under it (FormUpdate_Load since slice 14).
					Control textMessage=Find(root,"textConnectionMessage");
					textMessage.Text="Checking for updates with Open Dental is not available in this build. Install new versions manually.\r\n"
						+"After a new version is installed on one computer, the other computers update themselves from this database.";
					Form formUpdate=root.FindForm();
					int heightLessUpdate=root.ClientSize.Height-textMessage.Bottom-CodeBase.UiGrid.PAD;
					int heightRoot=root.Height;
					formUpdate.Height-=heightLessUpdate;
					if(root!=formUpdate && root.Height==heightRoot) {
						root.Height-=heightLessUpdate;//the client panel, when it does not follow the window off screen
					}
					return root;
				case "OpenPmsDental.FormUpdate#classic":
					//The panels the fork's Load hides: the classic check for updates.
					Find(root,"panelClassic").Visible=true;
					return root;
				case "OpenPmsDental.FormUpdate#versions":
					//The version groups and the second Check button, which no code of the fork shows.
					foreach(string name in new string[] { "groupBuild","groupStable","groupBeta","groupAlpha","butCheck2" }) {
						Find(root,name).Visible=true;
					}
					return root;
				case "OpenPmsDental.FormQuery#report":
					//IsReport: the print preview over the window, the zoom panel, no Print Preview button (FormQuery_Load).
					Find(root,"printPreviewControl2").Visible=true;
					Find(root,"splitContainer").Visible=false;
					Find(root,"butPrintPreview").Visible=false;
					Find(root,"panelZoom").Visible=true;
					return root;
				case "OpenPmsDental.FormQuery#split":
					//The split dragged down: the query pane twice as tall as Load's 140.
					((OpenPmsDental.UI.SplitContainer)Find(root,"splitContainer")).SplitterDistance=320;
					return root;
				case "CodeBase.FormProgressExtended#bars":
					//Two progress bars, as UpdateProgress adds them (AddNewProgressBar): a block bar and a marquee.
					//The list its field initializer makes (the builder runs no initializers), then the bars.
					root.GetType().GetField("_listProgressBars",FLAGS).SetValue(root,new List<CodeBase.ODProgressBar>());
					MethodInfo methodInfoAdd=root.GetType().GetMethod("AddNewProgressBar",FLAGS);
					methodInfoAdd.Invoke(root,new object[] { "Tables","Checking the tables","40%",40,100,"",CodeBase.ProgBarStyle.Blocks,0,false,false,false });
					methodInfoAdd.Invoke(root,new object[] { "Files","Copying the files","",0,100,"",CodeBase.ProgBarStyle.Marquee,30,false,false,true });
					root.PerformLayout();
					return root;
				case "CodeBase.FormProgressStatus#history":
					//The constructor with hasHistory: the window 60 px wider and 120 px taller, the history box in place of the message.
					Form formStatus=root.FindForm() ?? (Form)root;
					formStatus.Height+=120;
					formStatus.Width+=60;
					Find(root,"labelMsg").Visible=false;
					Find(root,"textHistoryMsg").Visible=true;
					return root;
				case "ServiceManager.FormWebConfigSettings#service":
					//A service other than the replication service (the constructor): the one-way check box and the master group hidden, the window
					//shorter by them, as the constructor does since slice 14 (main: Height=375).
					Form formSettings=root.FindForm() ?? (Form)root;
					Control groupMaster=Find(root,"groupReplicationMaster");
					int heightLess=groupMaster.Bottom-Find(root,"textPEM").Bottom;
					formSettings.Height-=heightLess;
					Find(root,"checkIsOneWayReplication").Visible=false;
					groupMaster.Visible=false;
					return root;
				case "OpenPmsDental.ControlFamily#superclones":
					//A patient with a super family and clones: the pane beside the insurance grid (RefreshModuleScreen).
					Control splitContainerSuperClones=Find(root,"splitContainerSuperClones");
					splitContainerSuperClones.Visible=true;
					MethodInfo methodInfoLayout=root.GetType().GetMethod("LayoutSuperClones",FLAGS);
					if(methodInfoLayout!=null) {
						methodInfoLayout.Invoke(root,new object[] { true });
					}
					else {//main's RefreshModuleScreen: the insurance grid 2 px right of the pane, to the module's edge
						Control gridIns=Find(root,"gridIns");
						gridIns.Bounds=new Rectangle(splitContainerSuperClones.Right+2,gridIns.Top,root.Width-splitContainerSuperClones.Right-2,gridIns.Height);
					}
					return root;
			}
			return null;
		}

		private static Control ShowSheetCheck(Control root,params string[] arrayShown) {
			foreach(string name in _arraySheetCheckHidden) {
				Find(root,name).Visible=arrayShown.Contains(name);
			}
			return root;
		}

		private static void SelectTab(Control root,string nameTabControl,string namePage) {
			OpenPmsDental.UI.TabControl tabControl=(OpenPmsDental.UI.TabControl)Find(root,nameTabControl);
			int index=tabControl.TabPages.IndexOf((OpenPmsDental.UI.TabPage)Find(tabControl,namePage));
			typeof(OpenPmsDental.UI.TabControl).GetField("_selectedIndex",FLAGS).SetValue(tabControl,index);
			tabControl.LayoutTabs();
			tabControl.Invalidate();
		}

		private static Control Find(Control root,string name) {
			Control control=FindIn(root,name);
			if(control!=null) {
				return control;
			}
			if(root.GetType().GetField(name,FLAGS)?.GetValue(root) is Control controlField) {
				return controlField;
			}
			throw new ArgumentException("No control "+name+" in "+root.Name+".");
		}

		private static Control FindIn(Control root,string name) {
			foreach(Control child in root.Controls) {
				if(child.Name==name) {
					return child;
				}
				Control found=FindIn(child,name);
				if(found!=null) {
					return found;
				}
			}
			return null;
		}
	}
}
