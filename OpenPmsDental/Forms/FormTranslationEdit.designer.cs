using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenPmsDental {
	public partial class FormTranslationEdit {
		private System.ComponentModel.IContainer components = null;

		///<summary></summary>
		protected override void Dispose( bool disposing ){
			if( disposing ){
				if(components != null){
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code

		private void InitializeComponent(){
			this.butSave = new OpenPmsDental.UI.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.textEnglish = new System.Windows.Forms.TextBox();
			this.textTranslation = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.textComments = new System.Windows.Forms.TextBox();
			this.textOtherTranslation = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// butSave
			// 
			this.butSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butSave.Location = new System.Drawing.Point(773, 587);
			this.butSave.Name = "butSave";
			this.butSave.Size = new System.Drawing.Size(75, 26);
			this.butSave.TabIndex = 3;
			this.butSave.Text = "&Save";
			this.butSave.Click += new System.EventHandler(this.butSave_Click);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(69, 8);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(52, 23);
			this.label1.TabIndex = 0;
			this.label1.Text = "English";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(49, 152);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(72, 23);
			this.label2.TabIndex = 0;
			this.label2.Text = "Translation";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textEnglish
			// 
			this.textEnglish.AcceptsReturn = true;
			this.textEnglish.Location = new System.Drawing.Point(127, 8);
			this.textEnglish.Multiline = true;
			this.textEnglish.Name = "textEnglish";
			this.textEnglish.ReadOnly = true;
			this.textEnglish.Size = new System.Drawing.Size(721, 130);
			this.textEnglish.TabIndex = 0;
			this.textEnglish.TabStop = false;
			// 
			// textTranslation
			// 
			this.textTranslation.AcceptsReturn = true;
			this.textTranslation.Location = new System.Drawing.Point(127, 152);
			this.textTranslation.Multiline = true;
			this.textTranslation.Name = "textTranslation";
			this.textTranslation.Size = new System.Drawing.Size(721, 130);
			this.textTranslation.TabIndex = 1;
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(49, 440);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(72, 23);
			this.label4.TabIndex = 0;
			this.label4.Text = "Comments";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textComments
			// 
			this.textComments.AcceptsReturn = true;
			this.textComments.Location = new System.Drawing.Point(127, 440);
			this.textComments.Multiline = true;
			this.textComments.Name = "textComments";
			this.textComments.Size = new System.Drawing.Size(721, 130);
			this.textComments.TabIndex = 2;
			// 
			// textOtherTranslation
			// 
			this.textOtherTranslation.Location = new System.Drawing.Point(127, 296);
			this.textOtherTranslation.Multiline = true;
			this.textOtherTranslation.Name = "textOtherTranslation";
			this.textOtherTranslation.ReadOnly = true;
			this.textOtherTranslation.Size = new System.Drawing.Size(721, 130);
			this.textOtherTranslation.TabIndex = 0;
			this.textOtherTranslation.TabStop = false;
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(17, 296);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(104, 23);
			this.label3.TabIndex = 0;
			this.label3.Text = "Other Translation";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// FormTranslationEdit
			// 
			this.AcceptButton = this.butSave;
			this.ClientSize = new System.Drawing.Size(856, 621);
			this.Controls.Add(this.butSave);
			this.Controls.Add(this.textOtherTranslation);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.textComments);
			this.Controls.Add(this.textTranslation);
			this.Controls.Add(this.textEnglish);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormTranslationEdit";
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "Translation Edit";
			this.Load += new System.EventHandler(this.FormTranslationEdit_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		private OpenPmsDental.UI.Button butSave;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox textEnglish;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox textComments;
		private System.Windows.Forms.TextBox textTranslation;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox textOtherTranslation;
	}
}
