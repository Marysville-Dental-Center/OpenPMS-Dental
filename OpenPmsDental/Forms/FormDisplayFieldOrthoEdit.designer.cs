using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenPmsDental {
	public partial class FormDisplayFieldOrthoEdit {
		/// <summary>Required designer variable.</summary>
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
			this.textDescription = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.textWidthMin = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.labelPickList = new System.Windows.Forms.Label();
			this.textPickList = new System.Windows.Forms.TextBox();
			this.butDown = new OpenPmsDental.UI.Button();
			this.butUp = new OpenPmsDental.UI.Button();
			this.textWidth = new OpenPmsDental.ValidNum();
			this.butSave = new OpenPmsDental.UI.Button();
			this.textDescriptionOverride = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.labelDefaultMode = new System.Windows.Forms.Label();
			this.groupBoxOD1 = new OpenPmsDental.UI.GroupBox();
			this.radioText = new System.Windows.Forms.RadioButton();
			this.radioPickList = new System.Windows.Forms.RadioButton();
			this.radioSignature = new System.Windows.Forms.RadioButton();
			this.radioProvider = new System.Windows.Forms.RadioButton();
			this.groupBoxOD1.SuspendLayout();
			this.SuspendLayout();
			// 
			// textDescription
			// 
			this.textDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textDescription.Location = new System.Drawing.Point(144, 35);
			this.textDescription.MaxLength = 255;
			this.textDescription.Name = "textDescription";
			this.textDescription.Size = new System.Drawing.Size(248, 23);
			this.textDescription.TabIndex = 5;
			this.textDescription.TextChanged += new System.EventHandler(this.textInternalName_TextChanged);
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(66, 35);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(72, 23);
			this.label2.TabIndex = 4;
			this.label2.Text = "Description";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(46, 89);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(92, 23);
			this.label3.TabIndex = 6;
			this.label3.Text = "Column Width";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textWidthMin
			// 
			this.textWidthMin.Location = new System.Drawing.Point(144, 62);
			this.textWidthMin.Name = "textWidthMin";
			this.textWidthMin.ReadOnly = true;
			this.textWidthMin.Size = new System.Drawing.Size(72, 23);
			this.textWidthMin.TabIndex = 9;
			this.textWidthMin.TabStop = false;
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(38, 62);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(100, 23);
			this.label4.TabIndex = 8;
			this.label4.Text = "Minimum Width";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(222, 65);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(175, 17);
			this.label5.TabIndex = 10;
			this.label5.Text = "(based on text above)";
			this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelPickList
			// 
			this.labelPickList.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.labelPickList.Location = new System.Drawing.Point(8, 251);
			this.labelPickList.Name = "labelPickList";
			this.labelPickList.Size = new System.Drawing.Size(130, 32);
			this.labelPickList.TabIndex = 89;
			this.labelPickList.Text = "Pick List\r\n(one entry per line)";
			this.labelPickList.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textPickList
			// 
			this.textPickList.AcceptsReturn = true;
			this.textPickList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textPickList.HideSelection = false;
			this.textPickList.Location = new System.Drawing.Point(144, 251);
			this.textPickList.Multiline = true;
			this.textPickList.Name = "textPickList";
			this.textPickList.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.textPickList.Size = new System.Drawing.Size(248, 250);
			this.textPickList.TabIndex = 20;
			// 
			// butDown
			// 
			this.butDown.AdjustImageLocation = new System.Drawing.Point(1, 0);
			this.butDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.butDown.Icon = OpenPmsDental.UI.EnumIcons.Down;
			this.butDown.Location = new System.Drawing.Point(432, 27);
			this.butDown.Name = "butDown";
			this.butDown.Size = new System.Drawing.Size(25, 24);
			this.butDown.TabIndex = 30;
			this.butDown.Click += new System.EventHandler(this.butDown_Click);
			// 
			// butUp
			// 
			this.butUp.AdjustImageLocation = new System.Drawing.Point(1, 0);
			this.butUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.butUp.Icon = OpenPmsDental.UI.EnumIcons.Up;
			this.butUp.Location = new System.Drawing.Point(401, 27);
			this.butUp.Name = "butUp";
			this.butUp.Size = new System.Drawing.Size(25, 24);
			this.butUp.TabIndex = 25;
			this.butUp.Click += new System.EventHandler(this.butUp_Click);
			// 
			// textWidth
			// 
			this.textWidth.Location = new System.Drawing.Point(144, 89);
			this.textWidth.MaxVal = 2000;
			this.textWidth.MinVal = 1;
			this.textWidth.Name = "textWidth";
			this.textWidth.Size = new System.Drawing.Size(72, 23);
			this.textWidth.TabIndex = 10;
			// 
			// butSave
			// 
			this.butSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butSave.Location = new System.Drawing.Point(427, 513);
			this.butSave.Name = "butSave";
			this.butSave.Size = new System.Drawing.Size(75, 26);
			this.butSave.TabIndex = 35;
			this.butSave.Text = "&Save";
			this.butSave.Click += new System.EventHandler(this.butSave_Click);
			// 
			// textDescriptionOverride
			// 
			this.textDescriptionOverride.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textDescriptionOverride.Location = new System.Drawing.Point(144, 8);
			this.textDescriptionOverride.MaxLength = 255;
			this.textDescriptionOverride.Name = "textDescriptionOverride";
			this.textDescriptionOverride.Size = new System.Drawing.Size(248, 23);
			this.textDescriptionOverride.TabIndex = 93;
			this.textDescriptionOverride.TextChanged += new System.EventHandler(this.textDisplayName_TextChanged);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(18, 8);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(120, 23);
			this.label1.TabIndex = 92;
			this.label1.Text = "Description Override";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label6
			// 
			this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label6.Location = new System.Drawing.Point(401, 8);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(101, 17);
			this.label6.TabIndex = 94;
			this.label6.Text = "(optional)";
			this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelDefaultMode
			// 
			this.labelDefaultMode.Location = new System.Drawing.Point(222, 92);
			this.labelDefaultMode.Name = "labelDefaultMode";
			this.labelDefaultMode.Size = new System.Drawing.Size(175, 17);
			this.labelDefaultMode.TabIndex = 95;
			this.labelDefaultMode.Text = "(default)";
			this.labelDefaultMode.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// groupBoxOD1
			// 
			this.groupBoxOD1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(249)))), ((int)(((byte)(249)))), ((int)(((byte)(249)))));
			this.groupBoxOD1.Controls.Add(this.radioProvider);
			this.groupBoxOD1.Controls.Add(this.radioSignature);
			this.groupBoxOD1.Controls.Add(this.radioPickList);
			this.groupBoxOD1.Controls.Add(this.radioText);
			this.groupBoxOD1.Location = new System.Drawing.Point(144, 120);
			this.groupBoxOD1.Name = "groupBoxOD1";
			this.groupBoxOD1.Size = new System.Drawing.Size(107, 123);
			this.groupBoxOD1.TabIndex = 96;
			this.groupBoxOD1.TabStop = false;
			this.groupBoxOD1.Text = "Type";
			// 
			// radioText
			// 
			this.radioText.AutoCheck = false;
			this.radioText.Location = new System.Drawing.Point(8, 23);
			this.radioText.Name = "radioText";
			this.radioText.Size = new System.Drawing.Size(46, 20);
			this.radioText.TabIndex = 0;
			this.radioText.TabStop = true;
			this.radioText.Text = "Text";
			this.radioText.UseVisualStyleBackColor = true;
			this.radioText.Click += new System.EventHandler(this.radioText_Click);
			// 
			// radioPickList
			// 
			this.radioPickList.AutoCheck = false;
			this.radioPickList.Location = new System.Drawing.Point(8, 47);
			this.radioPickList.Name = "radioPickList";
			this.radioPickList.Size = new System.Drawing.Size(68, 20);
			this.radioPickList.TabIndex = 1;
			this.radioPickList.TabStop = true;
			this.radioPickList.Text = "Pick List";
			this.radioPickList.UseVisualStyleBackColor = true;
			this.radioPickList.Click += new System.EventHandler(this.radioPickList_Click);
			// 
			// radioSignature
			// 
			this.radioSignature.AutoCheck = false;
			this.radioSignature.Location = new System.Drawing.Point(8, 71);
			this.radioSignature.Name = "radioSignature";
			this.radioSignature.Size = new System.Drawing.Size(75, 20);
			this.radioSignature.TabIndex = 2;
			this.radioSignature.TabStop = true;
			this.radioSignature.Text = "Signature";
			this.radioSignature.UseVisualStyleBackColor = true;
			this.radioSignature.Click += new System.EventHandler(this.radioSignature_Click);
			// 
			// radioProvider
			// 
			this.radioProvider.AutoCheck = false;
			this.radioProvider.Location = new System.Drawing.Point(8, 95);
			this.radioProvider.Name = "radioProvider";
			this.radioProvider.Size = new System.Drawing.Size(69, 20);
			this.radioProvider.TabIndex = 3;
			this.radioProvider.TabStop = true;
			this.radioProvider.Text = "Provider";
			this.radioProvider.UseVisualStyleBackColor = true;
			this.radioProvider.Click += new System.EventHandler(this.radioProvider_Click);
			// 
			// FormDisplayFieldOrthoEdit
			// 
			this.ClientSize = new System.Drawing.Size(510, 547);
			this.Controls.Add(this.groupBoxOD1);
			this.Controls.Add(this.butSave);
			this.Controls.Add(this.labelDefaultMode);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.textDescriptionOverride);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.butDown);
			this.Controls.Add(this.butUp);
			this.Controls.Add(this.labelPickList);
			this.Controls.Add(this.textPickList);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.textWidthMin);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.textWidth);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.textDescription);
			this.Controls.Add(this.label2);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormDisplayFieldOrthoEdit";
			this.ShowInTaskbar = false;
			this.Text = "Edit Ortho Display Field";
			this.Load += new System.EventHandler(this.FormDisplayFieldOrthoEdit_Load);
			this.groupBoxOD1.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion
		private OpenPmsDental.UI.Button butSave;
		private TextBox textDescription;
		private Label label2;
		private Label label3;
		private ValidNum textWidth;
		private TextBox textWidthMin;
		private Label label4;
		private Label label5;
		private Label labelPickList;
		private TextBox textPickList;
		private UI.Button butDown;
		private UI.Button butUp;
		private TextBox textDescriptionOverride;
		private Label label1;
		private Label label6;
		private Label labelDefaultMode;
		private OpenPmsDental.UI.GroupBox groupBoxOD1;
		private RadioButton radioProvider;
		private RadioButton radioSignature;
		private RadioButton radioPickList;
		private RadioButton radioText;
	}
}
