using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenPmsDental {
	public partial class FormProcTools {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormProcTools));
			this.checkAutocodes = new OpenPmsDental.UI.CheckBox();
			this.checkTcodes = new OpenPmsDental.UI.CheckBox();
			this.checkNcodes = new OpenPmsDental.UI.CheckBox();
			this.checkCodeImport = new OpenPmsDental.UI.CheckBox();
			this.checkCodeGroups = new OpenPmsDental.UI.CheckBox();
			this.label5 = new System.Windows.Forms.Label();
			this.checkProcButtons = new OpenPmsDental.UI.CheckBox();
			this.checkApptProcsQuickAdd = new OpenPmsDental.UI.CheckBox();
			this.checkRecallTypes = new OpenPmsDental.UI.CheckBox();
			this.butRun = new OpenPmsDental.UI.Button();
			this.labelLineOne = new System.Windows.Forms.Label();
			this.labelLineTwo = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// checkAutocodes
			// 
			this.checkAutocodes.Location = new System.Drawing.Point(8, 183);
			this.checkAutocodes.Name = "checkAutocodes";
			this.checkAutocodes.Size = new System.Drawing.Size(682, 34);
			this.checkAutocodes.TabIndex = 43;
			this.checkAutocodes.Text = "Auto Codes - Deletes all current auto codes and then adds the default auto codes." +
    " Procedure codes must have already been entered or they cannot be added as an au" +
    "to code.";
			// 
			// checkTcodes
			// 
			this.checkTcodes.Location = new System.Drawing.Point(8, 59);
			this.checkTcodes.Name = "checkTcodes";
			this.checkTcodes.Size = new System.Drawing.Size(682, 34);
			this.checkTcodes.TabIndex = 44;
			this.checkTcodes.Text = "T Codes - Remove temp codes, codes that start with \"T\", which were only needed fo" +
    "r the trial version. If a T code has already been used, then this moves it to th" +
    "e obsolete category.";
			// 
			// checkNcodes
			// 
			this.checkNcodes.Location = new System.Drawing.Point(8, 97);
			this.checkNcodes.Name = "checkNcodes";
			this.checkNcodes.Size = new System.Drawing.Size(682, 20);
			this.checkNcodes.TabIndex = 46;
			this.checkNcodes.Text = "N Codes - Add any missing no-fee codes.";
			// 
			// checkCodeImport
			// 
			this.checkCodeImport.Location = new System.Drawing.Point(8, 121);
			this.checkCodeImport.Name = "checkCodeImport";
			this.checkCodeImport.Size = new System.Drawing.Size(682, 34);
			this.checkCodeImport.TabIndex = 55;
			this.checkCodeImport.Text = "D Codes - Import CDT codes from a licensed code file that you supply. Adds missing codes, updates descriptions, fil" +
    "ls in missing abbreviations and moves codes the file marks as deleted to the Obsolete category.";
			// 
			// checkCodeGroups
			// 
			this.checkCodeGroups.Location = new System.Drawing.Point(8, 221);
			this.checkCodeGroups.Name = "checkCodeGroups";
			this.checkCodeGroups.Size = new System.Drawing.Size(682, 34);
			this.checkCodeGroups.TabIndex = 56;
			this.checkCodeGroups.Text = "Code Groups - Deletes all code groups and recreates the defaults for the code system you choose. Benefits that us" +
    "e a code group are moved to the matching new group.";
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(8, 8);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(682, 47);
			this.label5.TabIndex = 48;
			this.label5.Text = resources.GetString("label5.Text");
			// 
			// checkProcButtons
			// 
			this.checkProcButtons.Location = new System.Drawing.Point(8, 259);
			this.checkProcButtons.Name = "checkProcButtons";
			this.checkProcButtons.Size = new System.Drawing.Size(682, 34);
			this.checkProcButtons.TabIndex = 49;
			this.checkProcButtons.Text = resources.GetString("checkProcButtons.Text");
			// 
			// checkApptProcsQuickAdd
			// 
			this.checkApptProcsQuickAdd.Location = new System.Drawing.Point(8, 297);
			this.checkApptProcsQuickAdd.Name = "checkApptProcsQuickAdd";
			this.checkApptProcsQuickAdd.Size = new System.Drawing.Size(682, 34);
			this.checkApptProcsQuickAdd.TabIndex = 51;
			this.checkApptProcsQuickAdd.Text = "Appt Procs Quick Add - This is the list of procedures that you pick from within t" +
    "he Edit Appointment window. This resets the list to default.";
			// 
			// checkRecallTypes
			// 
			this.checkRecallTypes.Location = new System.Drawing.Point(8, 335);
			this.checkRecallTypes.Name = "checkRecallTypes";
			this.checkRecallTypes.Size = new System.Drawing.Size(682, 20);
			this.checkRecallTypes.TabIndex = 52;
			this.checkRecallTypes.Text = "Recall Types - Resets the recall types and triggers to default. Replaces any T co" +
    "des with D codes.";
			// 
			// butRun
			// 
			this.butRun.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butRun.Location = new System.Drawing.Point(608, 426);
			this.butRun.Name = "butRun";
			this.butRun.Size = new System.Drawing.Size(82, 26);
			this.butRun.TabIndex = 50;
			this.butRun.Text = "Run Now";
			this.butRun.Click += new System.EventHandler(this.butRun_Click);
			// 
			// labelLineOne
			// 
			this.labelLineOne.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.labelLineOne.Location = new System.Drawing.Point(8, 161);
			this.labelLineOne.Name = "labelLineOne";
			this.labelLineOne.Size = new System.Drawing.Size(682, 2);
			this.labelLineOne.TabIndex = 53;
			// 
			// labelLineTwo
			// 
			this.labelLineTwo.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.labelLineTwo.Location = new System.Drawing.Point(8, 175);
			this.labelLineTwo.Name = "labelLineTwo";
			this.labelLineTwo.Size = new System.Drawing.Size(682, 2);
			this.labelLineTwo.TabIndex = 54;
			// 
			// FormProcTools
			// 
			this.ClientSize = new System.Drawing.Size(698, 460);
			this.Controls.Add(this.checkCodeGroups);
			this.Controls.Add(this.checkCodeImport);
			this.Controls.Add(this.labelLineTwo);
			this.Controls.Add(this.labelLineOne);
			this.Controls.Add(this.checkRecallTypes);
			this.Controls.Add(this.checkApptProcsQuickAdd);
			this.Controls.Add(this.butRun);
			this.Controls.Add(this.checkProcButtons);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.checkNcodes);
			this.Controls.Add(this.checkTcodes);
			this.Controls.Add(this.checkAutocodes);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormProcTools";
			this.ShowInTaskbar = false;
			this.Text = "Procedure Code Tools";
			this.Load += new System.EventHandler(this.FormProcTools_Load);
			this.ResumeLayout(false);

		}
		#endregion
		private OpenPmsDental.UI.CheckBox checkAutocodes;
		private OpenPmsDental.UI.CheckBox checkTcodes;
		private OpenPmsDental.UI.CheckBox checkNcodes;
		private Label label5;
		private OpenPmsDental.UI.CheckBox checkProcButtons;
		private OpenPmsDental.UI.Button butRun;
		private OpenPmsDental.UI.CheckBox checkApptProcsQuickAdd;
		private OpenPmsDental.UI.CheckBox checkRecallTypes;
		private Label labelLineOne;
		private Label labelLineTwo;
		private OpenPmsDental.UI.CheckBox checkCodeImport;
		private OpenPmsDental.UI.CheckBox checkCodeGroups;
	}
}
