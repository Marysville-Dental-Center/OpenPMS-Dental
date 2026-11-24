namespace OpenPmsDental {
    partial class FormCalendar {
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
			this.butOK = new OpenPmsDental.UI.Button();
			this.monthCalendar = new OpenPmsDental.UI.MonthCalendarOD();
			this.SuspendLayout();
			// 
			// butOK
			// 
			this.butOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butOK.Location = new System.Drawing.Point(188,192);
			this.butOK.Name = "butOK";
			this.butOK.Size = new System.Drawing.Size(75,24);
			this.butOK.TabIndex = 3;
			this.butOK.Text = "&OK";
			this.butOK.Click += new System.EventHandler(this.butOK_Click);
			// 
			// monthCalendar
			// 
			this.monthCalendar.AllowKeyboard = true;
			this.monthCalendar.Location = new System.Drawing.Point(18,8);
			this.monthCalendar.MaxSelectionCount = 1;
			this.monthCalendar.Name = "monthCalendar";
			this.monthCalendar.Size = new System.Drawing.Size(227, 162);
			this.monthCalendar.TabIndex = 4;
			this.monthCalendar.DateChanged += new System.EventHandler(this.monthCalendar_DateChanged);
			// 
			// FormCalendar
			// 
			this.ClientSize = new System.Drawing.Size(271,224);
			this.Controls.Add(this.monthCalendar);
			this.Controls.Add(this.butOK);
			this.Name = "FormCalendar";
			this.Text = "Choose Date";
			this.Load += new System.EventHandler(this.FormDatePicker_Load);
			this.ResumeLayout(false);

        }

        #endregion

        private OpenPmsDental.UI.Button butOK;
        private OpenPmsDental.UI.MonthCalendarOD monthCalendar;
    }
}