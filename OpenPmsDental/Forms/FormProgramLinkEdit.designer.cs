using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenPmsDental {
	public partial class FormProgramLinkEdit {
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormProgramLinkEdit));
			this.butSave = new OpenPmsDental.UI.Button();
			this.checkEnabled = new OpenPmsDental.UI.CheckBox();
			this.butDelete = new OpenPmsDental.UI.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.textProgName = new System.Windows.Forms.TextBox();
			this.textProgDesc = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.textPath = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.textCommandLine = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.listToolBars = new OpenPmsDental.UI.ListBox();
			this.label6 = new System.Windows.Forms.Label();
			this.textButtonText = new System.Windows.Forms.TextBox();
			this.label7 = new System.Windows.Forms.Label();
			this.textNote = new System.Windows.Forms.TextBox();
			this.label8 = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.gridMain = new OpenPmsDental.UI.GridOD();
			this.textPluginDllName = new System.Windows.Forms.TextBox();
			this.label5 = new System.Windows.Forms.Label();
			this.textOverride = new System.Windows.Forms.TextBox();
			this.labelOverride = new System.Windows.Forms.Label();
			this.butClear = new OpenPmsDental.UI.Button();
			this.butImport = new OpenPmsDental.UI.Button();
			this.label10 = new System.Windows.Forms.Label();
			this.pictureBox = new System.Windows.Forms.PictureBox();
			this.butOutputFile = new OpenPmsDental.UI.Button();
			this.butClinicLink = new OpenPmsDental.UI.Button();
			this.groupboxPLbuttons = new OpenPmsDental.UI.GroupBox();
			this.labelClinicStateWarning = new System.Windows.Forms.Label();
			this.labelDisableForClinic = new System.Windows.Forms.Label();
			this.label11 = new System.Windows.Forms.Label();
			this.butPdmpCertificate = new OpenPmsDental.UI.Button();
			this.butPdmpCheckSetup = new OpenPmsDental.UI.Button();
			this.comboClinicPdmp = new OpenPmsDental.UI.ComboBoxClinicPicker();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
			this.groupboxPLbuttons.SuspendLayout();
			this.SuspendLayout();
			// 
			// butSave
			// 
			this.butSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butSave.Location = new System.Drawing.Point(694, 576);
			this.butSave.Name = "butSave";
			this.butSave.Size = new System.Drawing.Size(75, 26);
			this.butSave.TabIndex = 1;
			this.butSave.Text = "&Save";
			this.butSave.Click += new System.EventHandler(this.butSave_Click);
			// 
			// checkEnabled
			// 
			this.checkEnabled.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkEnabled.Location = new System.Drawing.Point(359, 62);
			this.checkEnabled.Name = "checkEnabled";
			this.checkEnabled.Size = new System.Drawing.Size(66, 20);
			this.checkEnabled.TabIndex = 41;
			this.checkEnabled.Text = "Enabled";
			// 
			// butDelete
			// 
			this.butDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butDelete.Icon = OpenPmsDental.UI.EnumIcons.DeleteX;
			this.butDelete.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butDelete.Location = new System.Drawing.Point(8, 576);
			this.butDelete.Name = "butDelete";
			this.butDelete.Size = new System.Drawing.Size(75, 26);
			this.butDelete.TabIndex = 43;
			this.butDelete.Text = "&Delete";
			this.butDelete.Click += new System.EventHandler(this.butDelete_Click);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(265, 8);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(88, 23);
			this.label1.TabIndex = 44;
			this.label1.Text = "Internal Name";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textProgName
			// 
			this.textProgName.AutoSize = false;
			this.textProgName.Location = new System.Drawing.Point(359, 8);
			this.textProgName.Name = "textProgName";
			this.textProgName.ReadOnly = true;
			this.textProgName.Size = new System.Drawing.Size(276, 23);
			this.textProgName.TabIndex = 45;
			// 
			// textProgDesc
			// 
			this.textProgDesc.AutoSize = false;
			this.textProgDesc.Location = new System.Drawing.Point(359, 35);
			this.textProgDesc.Name = "textProgDesc";
			this.textProgDesc.Size = new System.Drawing.Size(276, 23);
			this.textProgDesc.TabIndex = 47;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(281, 35);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(72, 23);
			this.label2.TabIndex = 46;
			this.label2.Text = "Description";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textPath
			// 
			this.textPath.AutoSize = false;
			this.textPath.Location = new System.Drawing.Point(359, 86);
			this.textPath.Name = "textPath";
			this.textPath.Size = new System.Drawing.Size(410, 23);
			this.textPath.TabIndex = 49;
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(241, 86);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(112, 23);
			this.label3.TabIndex = 48;
			this.label3.Text = "Path of file to open";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textCommandLine
			// 
			this.textCommandLine.AutoSize = false;
			this.textCommandLine.Location = new System.Drawing.Point(359, 144);
			this.textCommandLine.Name = "textCommandLine";
			this.textCommandLine.Size = new System.Drawing.Size(410, 23);
			this.textCommandLine.TabIndex = 52;
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(112, 140);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(241, 32);
			this.label4.TabIndex = 51;
			this.label4.Text = "Optional command line arguments.  Leave this blank for most bridges.";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// listToolBars
			// 
			this.listToolBars.Location = new System.Drawing.Point(19, 73);
			this.listToolBars.Name = "listToolBars";
			this.listToolBars.SelectionMode = OpenPmsDental.UI.SelectionMode.MultiExtended;
			this.listToolBars.Size = new System.Drawing.Size(156, 121);
			this.listToolBars.TabIndex = 53;
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(19, 53);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(173, 18);
			this.label6.TabIndex = 56;
			this.label6.Text = "Add a button to these toolbars";
			this.label6.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// textButtonText
			// 
			this.textButtonText.AutoSize = false;
			this.textButtonText.Location = new System.Drawing.Point(359, 242);
			this.textButtonText.Name = "textButtonText";
			this.textButtonText.Size = new System.Drawing.Size(196, 23);
			this.textButtonText.TabIndex = 58;
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(253, 176);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(88, 18);
			this.label7.TabIndex = 57;
			this.label7.Text = "Text on button";
			this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textNote
			// 
			this.textNote.Location = new System.Drawing.Point(359, 460);
			this.textNote.MaxLength = 4000;
			this.textNote.Multiline = true;
			this.textNote.Name = "textNote";
			this.textNote.Size = new System.Drawing.Size(323, 80);
			this.textNote.TabIndex = 59;
			// 
			// label8
			// 
			this.label8.Location = new System.Drawing.Point(541, 433);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(44, 23);
			this.label8.TabIndex = 60;
			this.label8.Text = "Notes";
			this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label9
			// 
			this.label9.Location = new System.Drawing.Point(347, 176);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(410, 62);
			this.label9.TabIndex = 61;
			this.label9.Text = resources.GetString("label9.Text");
			// 
			// gridMain
			// 
			this.gridMain.Location = new System.Drawing.Point(359, 296);
			this.gridMain.Name = "gridMain";
			this.gridMain.Size = new System.Drawing.Size(410, 133);
			this.gridMain.TabIndex = 62;
			this.gridMain.Title = "Additional Properties";
			this.gridMain.TranslationName = "TableProperties";
			this.gridMain.CellDoubleClick += new OpenPmsDental.UI.ODGridClickEventHandler(this.gridMain_CellDoubleClick);
			// 
			// textPluginDllName
			// 
			this.textPluginDllName.AutoSize = false;
			this.textPluginDllName.Location = new System.Drawing.Point(359, 269);
			this.textPluginDllName.Name = "textPluginDllName";
			this.textPluginDllName.Size = new System.Drawing.Size(196, 23);
			this.textPluginDllName.TabIndex = 64;
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(253, 242);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(100, 23);
			this.label5.TabIndex = 63;
			this.label5.Text = "Plug-in dll name";
			this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textOverride
			// 
			this.textOverride.AutoSize = false;
			this.textOverride.Location = new System.Drawing.Point(359, 113);
			this.textOverride.Name = "textOverride";
			this.textOverride.Size = new System.Drawing.Size(410, 23);
			this.textOverride.TabIndex = 66;
			// 
			// labelOverride
			// 
			this.labelOverride.Location = new System.Drawing.Point(137, 113);
			this.labelOverride.Name = "labelOverride";
			this.labelOverride.Size = new System.Drawing.Size(216, 23);
			this.labelOverride.TabIndex = 65;
			this.labelOverride.Text = "Local path override.  Usually left blank.";
			this.labelOverride.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// butClear
			// 
			this.butClear.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butClear.Location = new System.Drawing.Point(19, 225);
			this.butClear.Name = "butClear";
			this.butClear.Size = new System.Drawing.Size(75, 26);
			this.butClear.TabIndex = 70;
			this.butClear.Text = "Clear";
			this.butClear.Click += new System.EventHandler(this.butClear_Click);
			// 
			// butImport
			// 
			this.butImport.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butImport.Location = new System.Drawing.Point(100, 225);
			this.butImport.Name = "butImport";
			this.butImport.Size = new System.Drawing.Size(75, 26);
			this.butImport.TabIndex = 69;
			this.butImport.Text = "Import";
			this.butImport.Click += new System.EventHandler(this.butImport_Click);
			// 
			// label10
			// 
			this.label10.Location = new System.Drawing.Point(23, 198);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(124, 23);
			this.label10.TabIndex = 68;
			this.label10.Text = "Button Image (22x22)";
			this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// pictureBox
			// 
			this.pictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.pictureBox.Location = new System.Drawing.Point(153, 198);
			this.pictureBox.Name = "pictureBox";
			this.pictureBox.Size = new System.Drawing.Size(22, 22);
			this.pictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
			this.pictureBox.TabIndex = 72;
			this.pictureBox.TabStop = false;
			// 
			// butOutputFile
			// 
			this.butOutputFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butOutputFile.Location = new System.Drawing.Point(331, 576);
			this.butOutputFile.Name = "butOutputFile";
			this.butOutputFile.Size = new System.Drawing.Size(75, 26);
			this.butOutputFile.TabIndex = 73;
			this.butOutputFile.Text = "Output File";
			this.butOutputFile.Click += new System.EventHandler(this.butOutputFile_Click);
			// 
			// butClinicLink
			// 
			this.butClinicLink.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butClinicLink.Location = new System.Drawing.Point(19, 23);
			this.butClinicLink.Name = "butClinicLink";
			this.butClinicLink.Size = new System.Drawing.Size(127, 26);
			this.butClinicLink.TabIndex = 76;
			this.butClinicLink.Text = "Hide Button for Clinics";
			this.butClinicLink.Click += new System.EventHandler(this.butClinicLink_Click);
			// 
			// groupboxPLbuttons
			// 
			this.groupboxPLbuttons.Controls.Add(this.labelClinicStateWarning);
			this.groupboxPLbuttons.Controls.Add(this.listToolBars);
			this.groupboxPLbuttons.Controls.Add(this.label6);
			this.groupboxPLbuttons.Controls.Add(this.butClear);
			this.groupboxPLbuttons.Controls.Add(this.butClinicLink);
			this.groupboxPLbuttons.Controls.Add(this.pictureBox);
			this.groupboxPLbuttons.Controls.Add(this.butImport);
			this.groupboxPLbuttons.Controls.Add(this.label10);
			this.groupboxPLbuttons.Location = new System.Drawing.Point(15, 176);
			this.groupboxPLbuttons.Name = "groupboxPLbuttons";
			this.groupboxPLbuttons.Size = new System.Drawing.Size(200, 296);
			this.groupboxPLbuttons.TabIndex = 0;
			this.groupboxPLbuttons.Text = "Button Settings";
			// 
			// labelClinicStateWarning
			// 
			this.labelClinicStateWarning.ForeColor = System.Drawing.Color.DarkRed;
			this.labelClinicStateWarning.Location = new System.Drawing.Point(19, 255);
			this.labelClinicStateWarning.Name = "labelClinicStateWarning";
			this.labelClinicStateWarning.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.labelClinicStateWarning.Size = new System.Drawing.Size(159, 33);
			this.labelClinicStateWarning.TabIndex = 76;
			this.labelClinicStateWarning.Text = "Program Link button is not visible for some clinics.";
			this.labelClinicStateWarning.Visible = false;
			// 
			// labelDisableForClinic
			// 
			this.labelDisableForClinic.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.labelDisableForClinic.ForeColor = System.Drawing.Color.DarkRed;
			this.labelDisableForClinic.Location = new System.Drawing.Point(15, 544);
			this.labelDisableForClinic.Name = "labelDisableForClinic";
			this.labelDisableForClinic.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.labelDisableForClinic.Size = new System.Drawing.Size(751, 20);
			this.labelDisableForClinic.TabIndex = 77;
			this.labelDisableForClinic.Text = "User is Clinic restricted, some functions of this window are disabled. To enable," +
    " contact an administrator.";
			this.labelDisableForClinic.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.labelDisableForClinic.Visible = false;
			// 
			// label11
			// 
			this.label11.Location = new System.Drawing.Point(561, 271);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(149, 18);
			this.label11.TabIndex = 78;
			this.label11.Text = "Example: Something.dll";
			this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// butPdmpCertificate
			// 
			this.butPdmpCertificate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butPdmpCertificate.Location = new System.Drawing.Point(419, 576);
			this.butPdmpCertificate.Name = "butPdmpCertificate";
			this.butPdmpCertificate.Size = new System.Drawing.Size(120, 26);
			this.butPdmpCertificate.TabIndex = 79;
			this.butPdmpCertificate.Text = "Client certificate...";
			this.butPdmpCertificate.Visible = false;
			this.butPdmpCertificate.Click += new System.EventHandler(this.butPdmpCertificate_Click);
			// 
			// butPdmpCheckSetup
			// 
			this.butPdmpCheckSetup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butPdmpCheckSetup.Location = new System.Drawing.Point(545, 576);
			this.butPdmpCheckSetup.Name = "butPdmpCheckSetup";
			this.butPdmpCheckSetup.Size = new System.Drawing.Size(90, 26);
			this.butPdmpCheckSetup.TabIndex = 80;
			this.butPdmpCheckSetup.Text = "Check setup";
			this.butPdmpCheckSetup.Visible = false;
			this.butPdmpCheckSetup.Click += new System.EventHandler(this.butPdmpCheckSetup_Click);
			// 
			// comboClinicPdmp
			// 
			this.comboClinicPdmp.HqDescription = "Default";
			this.comboClinicPdmp.IncludeUnassigned = true;
			this.comboClinicPdmp.Location = new System.Drawing.Point(548, 433);
			this.comboClinicPdmp.Name = "comboClinicPdmp";
			this.comboClinicPdmp.Size = new System.Drawing.Size(199, 23);
			this.comboClinicPdmp.TabIndex = 81;
			this.comboClinicPdmp.Visible = false;
			this.comboClinicPdmp.SelectionChangeCommitted += new System.EventHandler(this.comboClinicPdmp_SelectionChangeCommitted);
			// 
			// FormProgramLinkEdit
			// 
			this.ClientSize = new System.Drawing.Size(777, 610);
			this.Controls.Add(this.comboClinicPdmp);
			this.Controls.Add(this.butPdmpCheckSetup);
			this.Controls.Add(this.butPdmpCertificate);
			this.Controls.Add(this.label11);
			this.Controls.Add(this.labelDisableForClinic);
			this.Controls.Add(this.butOutputFile);
			this.Controls.Add(this.textOverride);
			this.Controls.Add(this.labelOverride);
			this.Controls.Add(this.textPluginDllName);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.gridMain);
			this.Controls.Add(this.label9);
			this.Controls.Add(this.label8);
			this.Controls.Add(this.textNote);
			this.Controls.Add(this.textButtonText);
			this.Controls.Add(this.textCommandLine);
			this.Controls.Add(this.textPath);
			this.Controls.Add(this.textProgDesc);
			this.Controls.Add(this.textProgName);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.butDelete);
			this.Controls.Add(this.checkEnabled);
			this.Controls.Add(this.butSave);
			this.Controls.Add(this.groupboxPLbuttons);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormProgramLinkEdit";
			this.ShowInTaskbar = false;
			this.Text = "Program Link";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.FormProgramLinkEdit_Closing);
			this.Load += new System.EventHandler(this.FormProgramLinkEdit_Load);
			this.Shown += new System.EventHandler(this.FormProgramLinkEdit_Shown);
			((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
			this.groupboxPLbuttons.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion
		private OpenPmsDental.UI.Button butSave;
		private OpenPmsDental.UI.CheckBox checkEnabled;
		private OpenPmsDental.UI.Button butDelete;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox textProgName;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox textProgDesc;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.TextBox textPath;
		private System.Windows.Forms.TextBox textCommandLine;
		private OpenPmsDental.UI.ListBox listToolBars;
		private System.Windows.Forms.TextBox textButtonText;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.TextBox textNote;
		private Label label9;// Required designer variable.
		private OpenPmsDental.UI.GridOD gridMain;
		private TextBox textPluginDllName;
		private Label label5;
		private TextBox textOverride;
		private Label labelOverride;
		private UI.Button butClear;
		private UI.Button butImport;
		private Label label10;
		private PictureBox pictureBox;
		private UI.Button butOutputFile;
		private UI.Button butClinicLink;
		private OpenPmsDental.UI.GroupBox groupboxPLbuttons;
		private Label labelClinicStateWarning;
		private Label labelDisableForClinic;
		private Label label11;
		private UI.Button butPdmpCertificate;
		private UI.Button butPdmpCheckSetup;
		private OpenPmsDental.UI.ComboBoxClinicPicker comboClinicPdmp;
	}
}
