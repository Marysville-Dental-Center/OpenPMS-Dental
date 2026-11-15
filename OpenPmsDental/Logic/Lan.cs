using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using OpenPmsDentalBusiness;

namespace OpenPmsDental {
	///<summary>Lan is short for language.  Used to translate text to another language.</summary>
	public class Lan{

		//strings-----------------------------------------------
		///<summary>Converts a string to the current language.</summary>
		public static string g(string classType,string text) {
			string retVal=Lans.ConvertString(classType,text);
			return retVal;
		}

		///<summary>Converts a string to the current language.</summary>
		public static string g(object sender,string text) {
			string classType="All";
			if(sender!=null){
				classType=sender.GetType().Name;
			}
			string retVal=Lans.ConvertString(classType,text);
			return retVal;
		}

		///<summary>C is for control. Translates the text of all context menu strip items to another language. Takes any ToolStripDropDown: a
		///ContextMenuStrip, a ToolStripDropDownMenu or a ToolStripDropDown.</summary>
		public static void C(Control controlSender,params ToolStripDropDown[] paramsToolStripDropDowns) {
			string classType=(controlSender==null ? "All": controlSender.GetType().Name);
			C(classType,paramsToolStripDropDowns);
		}

		///<summary>C is for control. Translates the text of all context menu strip items to another language, with the same translation keys
		///(classType and the English text) as the WinForms menu items they replace. Separators and other items that are not ToolStripMenuItems
		///(a separator was a MenuItem "-" with nothing to translate) are left as they are.</summary>
		public static void C(string classType,params ToolStripDropDown[] paramsToolStripDropDowns) {
			for(int m=0;m<paramsToolStripDropDowns.Length;m++){
				for(int i=0;i<paramsToolStripDropDowns[m].Items.Count;i++){
					if(paramsToolStripDropDowns[m].Items[i] is ToolStripMenuItem toolStripMenuItem) {
						TranslateToolStripMenuItems(classType,toolStripMenuItem);
					}
				}
			}
		}

		///<summary>This is recursive</summary>
		public static void TranslateToolStripMenuItems(string classType,ToolStripMenuItem toolStripMenuItem) {
			//first translate any drop down items
			//Use ToolStripItem for DropDownItems since they can be of different Types. E.g. ToolStripMenuItem, ToolStripSeparator, etc.
			for(int i=0;i<toolStripMenuItem.DropDownItems.Count;i++){
				if(toolStripMenuItem.DropDownItems[i] is ToolStripMenuItem toolStripMenuItemSub) {
					TranslateToolStripMenuItems(classType,toolStripMenuItemSub);
				}
			}
			//then this tool strip menu item
			toolStripMenuItem.Text=Lans.ConvertString(classType,toolStripMenuItem.Text);
		}

		//controls-----------------------------------------------
		///<summary></summary>
		public static void C(string classType,params Control[] paramsControls) {
			C(classType,paramsControls,false);
		}

		///<summary></summary>
		public static void C(Control control,params Control[] paramsControls) {
			C(control,paramsControls,false);
		}

		///<summary></summary>
		public static void C(Control controlSender,Control[] controlArray,bool isRecursive) {
			string classType="All";
			if(controlSender!=null){
				classType=controlSender.GetType().Name;
			}
			C(classType,controlArray,isRecursive);
		}

		///<summary></summary>
		public static void C(string classType,Control[] controlArray,bool isRecursive) {
			for(int c=0;c<controlArray.Length;c++){
				if(controlArray[c].GetType()==typeof(UI.GridOD)) {
					TranslateGrid(controlArray[c]);
					continue;
				}
				if(controlArray[c] is TabControl tabControl) {
					C(classType,tabControl);
					continue;
				}
				if(controlArray[c] is UI.TabControl tabControlOD) {
					C(classType,tabControlOD);
					continue;
				}
				if(controlArray[c] is UI.MenuOD menuOD) {
					menuOD.TranslateMenuItems(classType);
					continue;
				}
				controlArray[c].Text=Lans.ConvertString(classType,controlArray[c].Text);
				if(isRecursive) {
					Cchildren(classType,controlArray[c]);
				}
			}
		}

		///<summary>This is recursive, but a little simpler than Fchildren.</summary>
		private static void Cchildren(string classType,Control controlParent) {
			for(int c=0;c<controlParent.Controls.Count;c++){
				if(controlParent.Controls[c].HasChildren) {
					Cchildren(classType,controlParent.Controls[c]);
				}
				controlParent.Controls[c].Text=Lans.ConvertString(classType,controlParent.Controls[c].Text);
			}
		}

		private static void TranslateGrid(Control control) {
			if(control.GetType()!=typeof(UI.GridOD)) {
				return;
			}
			UI.GridOD grid=((UI.GridOD)control);
			grid.Title=Lans.ConvertString(grid.TranslationName,grid.Title);
			for(int c=0;c<grid.Columns.Count;c++){
				grid.Columns[c].Heading=Lans.ConvertString(grid.TranslationName,grid.Columns[c].Heading);
			}
			TranslateGridWinFormsContextMenu(grid);
			if(grid.ContextMenuStrip!=null) {
				C(grid.TranslationName,grid.ContextMenuStrip);
			}
		}

		#region Bridge for S3a-M3b - WinForms ContextMenu of a grid
		//S3a-M3 removed Lan.C(Control or string,params Menu[]): no caller passes a WinForms ContextMenu or MainMenu any more. What is left translates
		//a WinForms ContextMenu that a caller set on a GridOD (GridOD's bridge, FormMedical's grids), with the same keys as before. Removed by
		//S3a-M3b with GridOD's bridge. NoHqTests' WinFormsMenuScanTests allows WinForms menus in this region only.
		///<summary>Translates the items of a WinForms ContextMenu that a caller set on the grid, if any.</summary>
		private static void TranslateGridWinFormsContextMenu(UI.GridOD grid) {
			if(grid.ContextMenu==null) {
				return;
			}
			for(int i=0;i<grid.ContextMenu.MenuItems.Count;i++){
				TranslateMenuItems(grid.TranslationName,grid.ContextMenu.MenuItems[i]);
			}
		}

		///<summary>This is recursive</summary>
		private static void TranslateMenuItems(string classType,MenuItem menuItem) {
			//first translate any child menuItems
			for(int i=0;i<menuItem.MenuItems.Count;i++){
				TranslateMenuItems(classType,menuItem.MenuItems[i]);
			}
			//then this menuitem
			menuItem.Text=Lans.ConvertString(classType,menuItem.Text);
		}
		#endregion Bridge for S3a-M3b - WinForms ContextMenu of a grid

		//tab conrol-----------------------------------------------------------------------------------
		///<summary></summary>
		public static void C(Control controlSender,params TabControl[] paramsTabControls) {
			string classType=(controlSender==null ? "All": controlSender.GetType().Name);
			C(classType,paramsTabControls);
		}

		///<summary></summary>
		public static void C(Control controlSender,params UI.TabControl[] paramsTabControls) {
			string classType=(controlSender==null ? "All": controlSender.GetType().Name);
			C(classType,paramsTabControls);
		}

		///<summary></summary>
		public static void C(string classType,params TabControl[] paramsTabControls) {
			for(int c=0;c<paramsTabControls.Length;c++){
				for(int p=0;p<paramsTabControls[c].TabPages.Count;p++){
					paramsTabControls[c].TabPages[p].Text=Lans.ConvertString(classType, paramsTabControls[c].TabPages[p].Text);
				}
			}
		}

		///<summary></summary>
		public static void C(string classType,params UI.TabControl[] paramsTabControls) {
			for(int c=0;c<paramsTabControls.Length;c++){
				for(int p=0;p<paramsTabControls[c].TabPages.Count;p++){
					paramsTabControls[c].TabPages[p].Text=Lans.ConvertString(classType, paramsTabControls[c].TabPages[p].Text);
				}
			}
		}

		//forms----------------------------------------------------------------------------------------
		///<summary>F is for Form. Translates the following controls on the entire form: title Text, labels, buttons, groupboxes, checkboxes, radiobuttons, ODGrid.  Can include a list of controls to exclude. Also puts all the correct controls into the All category (OK,Cancel,Close,Delete,etc).</summary>
		public static void F(Form formSender) {
			F(formSender,new Control[] { });
		}

		///<summary>F is for Form. Translates the following controls on the entire form: title Text, labels, buttons, groupboxes, checkboxes, radiobuttons, ODGrid.  Can include a list of controls to exclude. Also puts all the correct controls into the All category (OK,Cancel,Close,Delete,etc).</summary>
		public static void F(Form formSender,params Control[] paramsControlsExclusions) {
			if(CultureInfo.CurrentCulture.Name=="en-US") {
				return;
			}
			//if(CultureInfo.CurrentCulture.TextInfo.IsRightToLeft) {
			//	sender.RightToLeft=RightToLeft.Yes;//This started failing as we built each custom control without LTR support.
			//	sender.RightToLeftLayout=true;//This started failing when we switched to LayoutManager.
			//}
			//first translate the main title Text on the form:
			if(!Contains(paramsControlsExclusions,formSender)) {
				formSender.Text=Lans.ConvertString(formSender.GetType().Name,formSender.Text);
			}
			//then launch the recursive function for all child controls
			Fchildren(formSender,formSender,paramsControlsExclusions);
		}

		///<summary>Returns true if the contrToFind exists in the supplied contrArray. Used to check the exclusion list of F.</summary>
		private static bool Contains(Control[] controlArray,Control controlToFind) {
			for(int i=0;i<controlArray.Length;i++) {
				if(controlArray[i]==controlToFind) {
					return true;
				}
			}
			return false;
		}

		///<summary>Called from F and also recursively. Translates all children of the given control except those in the exclusions list.</summary>
		private static void Fchildren(Form formSender,Control controlParent,params Control[] paramsControls) {
			string senderTypeName=formSender.GetType().Name;
			for(int c=0;c<controlParent.Controls.Count;c++){
				Type typeContr=controlParent.Controls[c].GetType();
				//Any controls with children of their own.  First so that we always translate children.
				if(controlParent.Controls[c].HasChildren) {
					Fchildren(formSender,controlParent.Controls[c],paramsControls);
				}
				//Process contr
				if(paramsControls!=null && Contains(paramsControls,controlParent.Controls[c])) {
					continue;//Do not translate contr because it is excluded.
				}
				//Test to see if the control supports the .Text property.
				//Every control will have a .Text property present but some controls will purposefully throw a NotSupportedException (e.g. WebBrowser).
				try {
					controlParent.Controls[c].Text=controlParent.Controls[c].Text;
				}
				catch(Exception) {
					continue;//We cannot translate this control so move on.
				}
				if(typeContr==typeof(GroupBox)) {
					ShiftChildControls(controlParent.Controls[c]);
					TranslateControl(senderTypeName,controlParent.Controls[c]);
					continue;
				}
				if(typeContr==typeof(UI.GridOD)) {
					TranslateGrid(controlParent.Controls[c]);
					continue;
				}
				if(controlParent.Controls[c] is UI.MenuOD menuOD) {
					menuOD.TranslateMenuItems(senderTypeName);
					continue;
				}
				if(typeContr==typeof(Panel)) {
					ShiftChildControls(controlParent.Controls[c]);
					continue;
				}
				if(typeContr==typeof(TabControl)) {
					//Translate all tab pages on the tab control.
					C(senderTypeName,(TabControl)controlParent.Controls[c]);
					continue;
				}
				if(typeContr==typeof(UI.TabControl)) {
					//Translate all tab pages on the tab control.
					C(senderTypeName,(UI.TabControl)controlParent.Controls[c]);
					continue;
				}
				//Generically try to translate all orther controls not specifically mentioned above.
				TranslateControl(senderTypeName,controlParent.Controls[c]);
			}
		}

		private static void TranslateControl(string classType,Control control) {
			if(control.Text=="OK"
					|| control.Text=="&OK"
					|| control.Text=="Cancel"
					|| control.Text=="&Cancel"
					|| control.Text=="Close"
					|| control.Text=="&Close"
					|| control.Text=="Add"
					|| control.Text=="&Add"
					|| control.Text=="Delete"
					|| control.Text=="&Delete"
					|| control.Text=="Up"
					|| control.Text=="&Up"
					|| control.Text=="Down"
					|| control.Text=="&Down"
					|| control.Text=="Print"
					|| control.Text=="&Print") 
			{
				//Maintain the same translation text for the most common text throughout the entire program.
				control.Text=Lans.ConvertString("All",control.Text);
				return;
			}
			control.Text=Lans.ConvertString(classType,control.Text);
		}

		///<summary>Shift specific controls to the right for certain cultures (not USA).</summary>
		private static void ShiftChildControls(Control control) {
			if(!CultureInfo.CurrentCulture.TextInfo.IsRightToLeft) {
				return;
			}
			for(int c=0;c<control.Controls.Count;c++){
				control.Controls[c].Location=new Point(control.Width-control.Controls[c].Width-control.Controls[c].Left,control.Controls[c].Top);
			}
		}

	}
}