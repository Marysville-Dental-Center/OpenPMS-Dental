namespace OpenPmsDental{
	partial class FormTimePick {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if(disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.dateTimePicker = new System.Windows.Forms.DateTimePicker();
			this.radioPM = new System.Windows.Forms.RadioButton();
			this.radioAM = new System.Windows.Forms.RadioButton();
			this.label1 = new System.Windows.Forms.Label();
			this.comboMinute = new OpenPmsDental.UI.ComboBox();
			this.comboHour = new OpenPmsDental.UI.ComboBox();
			this.groupDate = new OpenPmsDental.UI.GroupBox();
			this.groupTime = new OpenPmsDental.UI.GroupBox();
			this.butOK = new OpenPmsDental.UI.Button();
			this.groupDate.SuspendLayout();
			this.groupTime.SuspendLayout();
			this.SuspendLayout();
			// 
			// dateTimePicker
			// 
			this.dateTimePicker.Location = new System.Drawing.Point(8, 23);
			this.dateTimePicker.Name = "dateTimePicker";
			this.dateTimePicker.Size = new System.Drawing.Size(221, 23);
			this.dateTimePicker.TabIndex = 65;
			// 
			// radioPM
			// 
			this.radioPM.Location = new System.Drawing.Point(144, 47);
			this.radioPM.Name = "radioPM";
			this.radioPM.Size = new System.Drawing.Size(43, 18);
			this.radioPM.TabIndex = 63;
			this.radioPM.Text = "PM";
			this.radioPM.UseVisualStyleBackColor = true;
			// 
			// radioAM
			// 
			this.radioAM.Checked = true;
			this.radioAM.Location = new System.Drawing.Point(144, 25);
			this.radioAM.Name = "radioAM";
			this.radioAM.Size = new System.Drawing.Size(44, 18);
			this.radioAM.TabIndex = 62;
			this.radioAM.TabStop = true;
			this.radioAM.Text = "AM";
			this.radioAM.UseVisualStyleBackColor = true;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(66, 23);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(12, 23);
			this.label1.TabIndex = 61;
			this.label1.Text = ":";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// comboMinute
			// 
			this.comboMinute.Location = new System.Drawing.Point(80, 23);
			this.comboMinute.Name = "comboMinute";
			this.comboMinute.Size = new System.Drawing.Size(56, 23);
			this.comboMinute.TabIndex = 60;
			// 
			// comboHour
			// 
			this.comboHour.Location = new System.Drawing.Point(8, 23);
			this.comboHour.Name = "comboHour";
			this.comboHour.Size = new System.Drawing.Size(56, 23);
			this.comboHour.TabIndex = 59;
			// 
			// groupDate
			// 
			this.groupDate.Controls.Add(this.dateTimePicker);
			this.groupDate.Location = new System.Drawing.Point(8, 8);
			this.groupDate.Name = "groupDate";
			this.groupDate.Size = new System.Drawing.Size(237, 54);
			this.groupDate.TabIndex = 66;
			this.groupDate.TabStop = false;
			this.groupDate.Text = "Pick Date";
			// 
			// groupTime
			// 
			this.groupTime.Controls.Add(this.comboHour);
			this.groupTime.Controls.Add(this.radioPM);
			this.groupTime.Controls.Add(this.comboMinute);
			this.groupTime.Controls.Add(this.radioAM);
			this.groupTime.Controls.Add(this.label1);
			this.groupTime.Location = new System.Drawing.Point(8, 70);
			this.groupTime.Name = "groupTime";
			this.groupTime.Size = new System.Drawing.Size(237, 73);
			this.groupTime.TabIndex = 67;
			this.groupTime.TabStop = false;
			this.groupTime.Text = "Pick Time";
			// 
			// butOK
			// 
			this.butOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butOK.Location = new System.Drawing.Point(170, 155);
			this.butOK.Name = "butOK";
			this.butOK.Size = new System.Drawing.Size(75, 24);
			this.butOK.TabIndex = 3;
			this.butOK.Text = "&OK";
			this.butOK.Click += new System.EventHandler(this.butOK_Click);
			// 
			// FormTimePick
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = new System.Drawing.Size(253, 187);
			this.Controls.Add(this.groupTime);
			this.Controls.Add(this.groupDate);
			this.Controls.Add(this.butOK);
			this.Name = "FormTimePick";
			this.Text = "Pick Time";
			this.Load += new System.EventHandler(this.FormTimePick_Load);
			this.groupDate.ResumeLayout(false);
			this.groupTime.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private OpenPmsDental.UI.Button butOK;
		private OpenPmsDental.UI.ComboBox comboHour;
		private OpenPmsDental.UI.ComboBox comboMinute;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.RadioButton radioAM;
		private System.Windows.Forms.RadioButton radioPM;
		private System.Windows.Forms.DateTimePicker dateTimePicker;
		private OpenPmsDental.UI.GroupBox groupDate;
		private OpenPmsDental.UI.GroupBox groupTime;
	}
}