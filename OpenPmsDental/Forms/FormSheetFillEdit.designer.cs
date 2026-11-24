namespace OpenPmsDental{
	partial class FormSheetFillEdit {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			for(int i=0;i<panelMain.Controls.Count;i++){
				if(panelMain.Controls[i] is System.Windows.Forms.PictureBox pictureBox){
					pictureBox.Image?.Dispose();
				}
			}
			if(disposing) {
				components?.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.components = new System.ComponentModel.Container();
			this.textNote = new System.Windows.Forms.TextBox();
			this.labelNote = new System.Windows.Forms.Label();
			this.labelDateTime = new System.Windows.Forms.Label();
			this.panelScroll = new OpenPmsDental.PanelScrollManual();
			this.panelMain = new OpenPmsDental.UI.ControlDoubleBuffered();
			this.checkErase = new OpenPmsDental.UI.CheckBox();
			this.textDescription = new System.Windows.Forms.TextBox();
			this.labelDescription = new System.Windows.Forms.Label();
			this.labelShowInTerminal = new System.Windows.Forms.Label();
			this.textDateTime = new System.Windows.Forms.TextBox();
			this.butUnlock = new OpenPmsDental.UI.Button();
			this.textShowInTerminal = new OpenPmsDental.ValidNum();
			this.butDelete = new OpenPmsDental.UI.Button();
			this.butPrintOrEmail = new OpenPmsDental.UI.Button();
			this.butOK = new OpenPmsDental.UI.Button();
			this.butChangePat = new OpenPmsDental.UI.Button();
			this.butRestore = new OpenPmsDental.UI.Button();
			this.butSave = new OpenPmsDental.UI.Button();
			this.butPrint = new OpenPmsDental.UI.Button();
			this.butEmail = new OpenPmsDental.UI.Button();
			this.butToKiosk = new OpenPmsDental.UI.Button();
			this.butPDF = new OpenPmsDental.UI.Button();
			this.butAddField = new OpenPmsDental.UI.Button();
			this.timerTextChanged = new System.Windows.Forms.Timer(this.components);
			this.butReduceFontSize = new OpenPmsDental.UI.Button();
			this.butFontAbout = new OpenPmsDental.UI.Button();
			this.panelScroll.SuspendLayout();
			this.SuspendLayout();
			// 
			// textNote
			// 
			this.textNote.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.textNote.Location = new System.Drawing.Point(162, 609);
			this.textNote.Multiline = true;
			this.textNote.Name = "textNote";
			this.textNote.Size = new System.Drawing.Size(146, 95);
			this.textNote.TabIndex = 6;
			this.textNote.TabStop = false;
			// 
			// labelNote
			// 
			this.labelNote.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.labelNote.Location = new System.Drawing.Point(162, 591);
			this.labelNote.Name = "labelNote";
			this.labelNote.Size = new System.Drawing.Size(146, 16);
			this.labelNote.TabIndex = 7;
			this.labelNote.Text = "Internal Note";
			this.labelNote.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// labelDateTime
			// 
			this.labelDateTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.labelDateTime.Location = new System.Drawing.Point(8, 591);
			this.labelDateTime.Name = "labelDateTime";
			this.labelDateTime.Size = new System.Drawing.Size(146, 16);
			this.labelDateTime.TabIndex = 76;
			this.labelDateTime.Text = "Date time";
			this.labelDateTime.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// panelScroll
			// 
			this.panelScroll.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panelScroll.AutoScroll = true;
			this.panelScroll.BackColor = System.Drawing.Color.WhiteSmoke;
			this.panelScroll.Controls.Add(this.panelMain);
			this.panelScroll.Location = new System.Drawing.Point(8, 8);
			this.panelScroll.Name = "panelScroll";
			this.panelScroll.Size = new System.Drawing.Size(871, 571);
			this.panelScroll.TabIndex = 1;
			this.panelScroll.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panelScroll_MouseUp);
			// 
			// panelMain
			// 
			this.panelMain.BackColor = System.Drawing.SystemColors.Window;
			this.panelMain.Location = new System.Drawing.Point(0, 0);
			this.panelMain.Name = "panelMain";
			this.panelMain.Size = new System.Drawing.Size(560, 494);
			this.panelMain.TabIndex = 0;
			this.panelMain.Paint += new System.Windows.Forms.PaintEventHandler(this.panelMain_Paint);
			this.panelMain.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.panelMain_MouseDoubleClick);
			this.panelMain.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelMain_MouseDown);
			this.panelMain.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panelMain_MouseMove);
			this.panelMain.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panelMain_MouseUp);
			this.panelMain.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.PanelMain_PreviewKeyDown);
			// 
			// checkErase
			// 
			this.checkErase.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.checkErase.Location = new System.Drawing.Point(320, 627);
			this.checkErase.Name = "checkErase";
			this.checkErase.Size = new System.Drawing.Size(81, 18);
			this.checkErase.TabIndex = 81;
			this.checkErase.TabStop = false;
			this.checkErase.Text = "Eraser Tool";
			this.checkErase.Click += new System.EventHandler(this.checkErase_Click);
			// 
			// textDescription
			// 
			this.textDescription.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.textDescription.Location = new System.Drawing.Point(8, 654);
			this.textDescription.Name = "textDescription";
			this.textDescription.Size = new System.Drawing.Size(146, 23);
			this.textDescription.TabIndex = 84;
			this.textDescription.TabStop = false;
			// 
			// labelDescription
			// 
			this.labelDescription.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.labelDescription.Location = new System.Drawing.Point(8, 636);
			this.labelDescription.Name = "labelDescription";
			this.labelDescription.Size = new System.Drawing.Size(146, 16);
			this.labelDescription.TabIndex = 85;
			this.labelDescription.Text = "Description";
			this.labelDescription.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// labelShowInTerminal
			// 
			this.labelShowInTerminal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.labelShowInTerminal.Location = new System.Drawing.Point(-1, 681);
			this.labelShowInTerminal.Name = "labelShowInTerminal";
			this.labelShowInTerminal.Size = new System.Drawing.Size(120, 23);
			this.labelShowInTerminal.TabIndex = 86;
			this.labelShowInTerminal.Text = "Show Order In Kiosk";
			this.labelShowInTerminal.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textDateTime
			// 
			this.textDateTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.textDateTime.AutoSize = false;
			this.textDateTime.Location = new System.Drawing.Point(8, 609);
			this.textDateTime.Name = "textDateTime";
			this.textDateTime.Size = new System.Drawing.Size(146, 23);
			this.textDateTime.TabIndex = 88;
			this.textDateTime.TabStop = false;
			// 
			// butUnlock
			// 
			this.butUnlock.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butUnlock.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butUnlock.Location = new System.Drawing.Point(320, 652);
			this.butUnlock.Name = "butUnlock";
			this.butUnlock.Size = new System.Drawing.Size(81, 24);
			this.butUnlock.TabIndex = 89;
			this.butUnlock.TabStop = false;
			this.butUnlock.Text = "Unlock";
			this.butUnlock.Visible = false;
			this.butUnlock.Click += new System.EventHandler(this.butUnlock_Click);
			// 
			// textShowInTerminal
			// 
			this.textShowInTerminal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.textShowInTerminal.Location = new System.Drawing.Point(125, 681);
			this.textShowInTerminal.MaxVal = 127;
			this.textShowInTerminal.MinVal = 1;
			this.textShowInTerminal.Name = "textShowInTerminal";
			this.textShowInTerminal.ShowZero = false;
			this.textShowInTerminal.Size = new System.Drawing.Size(29, 23);
			this.textShowInTerminal.TabIndex = 87;
			this.textShowInTerminal.TabStop = false;
			// 
			// butDelete
			// 
			this.butDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butDelete.Icon = OpenPmsDental.UI.EnumIcons.DeleteX;
			this.butDelete.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butDelete.Location = new System.Drawing.Point(798, 624);
			this.butDelete.Name = "butDelete";
			this.butDelete.Size = new System.Drawing.Size(81, 24);
			this.butDelete.TabIndex = 79;
			this.butDelete.TabStop = false;
			this.butDelete.Text = "Delete";
			this.butDelete.Click += new System.EventHandler(this.butDelete_Click);
			// 
			// butPrintOrEmail
			// 
			this.butPrintOrEmail.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butPrintOrEmail.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butPrintOrEmail.Location = new System.Drawing.Point(320, 680);
			this.butPrintOrEmail.Name = "butPrintOrEmail";
			this.butPrintOrEmail.Size = new System.Drawing.Size(81, 24);
			this.butPrintOrEmail.TabIndex = 80;
			this.butPrintOrEmail.TabStop = false;
			this.butPrintOrEmail.Text = "Print/Email";
			this.butPrintOrEmail.Click += new System.EventHandler(this.butPrintOrEmail_Click);
			// 
			// butOK
			// 
			this.butOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butOK.Location = new System.Drawing.Point(798, 680);
			this.butOK.Name = "butOK";
			this.butOK.Size = new System.Drawing.Size(81, 24);
			this.butOK.TabIndex = 3;
			this.butOK.TabStop = false;
			this.butOK.Text = "OK";
			this.butOK.Click += new System.EventHandler(this.butOK_Click);
			// 
			// butChangePat
			// 
			this.butChangePat.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butChangePat.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butChangePat.Location = new System.Drawing.Point(498, 624);
			this.butChangePat.Name = "butChangePat";
			this.butChangePat.Size = new System.Drawing.Size(81, 24);
			this.butChangePat.TabIndex = 90;
			this.butChangePat.TabStop = false;
			this.butChangePat.Text = "Change Pat";
			this.butChangePat.UseVisualStyleBackColor = true;
			this.butChangePat.Click += new System.EventHandler(this.butChangePat_Click);
			// 
			// butRestore
			// 
			this.butRestore.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butRestore.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butRestore.Location = new System.Drawing.Point(498, 652);
			this.butRestore.Name = "butRestore";
			this.butRestore.Size = new System.Drawing.Size(81, 24);
			this.butRestore.TabIndex = 91;
			this.butRestore.TabStop = false;
			this.butRestore.Text = "UnDelete";
			this.butRestore.UseVisualStyleBackColor = true;
			this.butRestore.Visible = false;
			this.butRestore.Click += new System.EventHandler(this.butRestore_Click);
			// 
			// butSave
			// 
			this.butSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butSave.Location = new System.Drawing.Point(798, 652);
			this.butSave.Name = "butSave";
			this.butSave.Size = new System.Drawing.Size(81, 24);
			this.butSave.TabIndex = 92;
			this.butSave.TabStop = false;
			this.butSave.Text = "Save";
			this.butSave.Visible = false;
			this.butSave.Click += new System.EventHandler(this.butSave_Click);
			// 
			// butPrint
			// 
			this.butPrint.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butPrint.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butPrint.Location = new System.Drawing.Point(409, 680);
			this.butPrint.Name = "butPrint";
			this.butPrint.Size = new System.Drawing.Size(81, 24);
			this.butPrint.TabIndex = 93;
			this.butPrint.TabStop = false;
			this.butPrint.Text = "Print";
			this.butPrint.UseVisualStyleBackColor = true;
			this.butPrint.Click += new System.EventHandler(this.butPrint_Click);
			// 
			// butEmail
			// 
			this.butEmail.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butEmail.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butEmail.Location = new System.Drawing.Point(498, 680);
			this.butEmail.Name = "butEmail";
			this.butEmail.Size = new System.Drawing.Size(81, 24);
			this.butEmail.TabIndex = 94;
			this.butEmail.TabStop = false;
			this.butEmail.Text = "Email";
			this.butEmail.UseVisualStyleBackColor = true;
			this.butEmail.Click += new System.EventHandler(this.butEmail_Click);
			// 
			// butToKiosk
			// 
			this.butToKiosk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butToKiosk.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
			this.butToKiosk.Image = OpenPmsDental.UI.IconLibrary.GetBitmap(OpenPmsDental.UI.EnumIcons.Up,22,22);
			this.butToKiosk.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butToKiosk.Location = new System.Drawing.Point(498, 596);
			this.butToKiosk.Name = "butToKiosk";
			this.butToKiosk.Size = new System.Drawing.Size(81, 24);
			this.butToKiosk.TabIndex = 95;
			this.butToKiosk.TabStop = false;
			this.butToKiosk.Text = "To Kiosk";
			this.butToKiosk.UseVisualStyleBackColor = true;
			this.butToKiosk.Click += new System.EventHandler(this.butToKiosk_Click);
			// 
			// butPDF
			// 
			this.butPDF.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butPDF.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butPDF.Location = new System.Drawing.Point(409, 652);
			this.butPDF.Name = "butPDF";
			this.butPDF.Size = new System.Drawing.Size(81, 24);
			this.butPDF.TabIndex = 83;
			this.butPDF.TabStop = false;
			this.butPDF.Text = "Create PDF";
			this.butPDF.Click += new System.EventHandler(this.butPDF_Click);
			// 
			// butAddField
			// 
			this.butAddField.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butAddField.Icon = OpenPmsDental.UI.EnumIcons.Add;
			this.butAddField.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butAddField.Location = new System.Drawing.Point(587, 624);
			this.butAddField.Name = "butAddField";
			this.butAddField.Size = new System.Drawing.Size(81, 24);
			this.butAddField.TabIndex = 97;
			this.butAddField.TabStop = false;
			this.butAddField.Text = "Add Field";
			this.butAddField.UseVisualStyleBackColor = true;
			this.butAddField.Click += new System.EventHandler(this.butAddField_Click);
			// 
			// timerTextChanged
			// 
			this.timerTextChanged.Interval = 3000;
			this.timerTextChanged.Tick += new System.EventHandler(this.timerTextChanged_Tick);
			// 
			// butReduceFontSize
			// 
			this.butReduceFontSize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butReduceFontSize.Location = new System.Drawing.Point(587, 652);
			this.butReduceFontSize.Name = "butReduceFontSize";
			this.butReduceFontSize.Size = new System.Drawing.Size(98, 24);
			this.butReduceFontSize.TabIndex = 108;
			this.butReduceFontSize.TabStop = false;
			this.butReduceFontSize.Text = "Reduce Font Size";
			this.butReduceFontSize.UseVisualStyleBackColor = true;
			this.butReduceFontSize.Click += new System.EventHandler(this.butReduceFontSize_Click);
			// 
			// butFontAbout
			// 
			this.butFontAbout.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butFontAbout.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.butFontAbout.Location = new System.Drawing.Point(691, 652);
			this.butFontAbout.Name = "butFontAbout";
			this.butFontAbout.Size = new System.Drawing.Size(24, 24);
			this.butFontAbout.TabIndex = 109;
			this.butFontAbout.TabStop = false;
			this.butFontAbout.Text = "?";
			this.butFontAbout.UseVisualStyleBackColor = true;
			this.butFontAbout.Click += new System.EventHandler(this.butFontAbout_Click);
			// 
			// FormSheetFillEdit
			// 
			this.ClientSize = new System.Drawing.Size(887, 712);
			this.Controls.Add(this.butFontAbout);
			this.Controls.Add(this.butReduceFontSize);
			this.Controls.Add(this.butAddField);
			this.Controls.Add(this.butOK);
			this.Controls.Add(this.butToKiosk);
			this.Controls.Add(this.butEmail);
			this.Controls.Add(this.butPrint);
			this.Controls.Add(this.butSave);
			this.Controls.Add(this.butRestore);
			this.Controls.Add(this.butChangePat);
			this.Controls.Add(this.butUnlock);
			this.Controls.Add(this.textDateTime);
			this.Controls.Add(this.textShowInTerminal);
			this.Controls.Add(this.labelShowInTerminal);
			this.Controls.Add(this.textDescription);
			this.Controls.Add(this.labelDescription);
			this.Controls.Add(this.butPDF);
			this.Controls.Add(this.butDelete);
			this.Controls.Add(this.checkErase);
			this.Controls.Add(this.panelScroll);
			this.Controls.Add(this.butPrintOrEmail);
			this.Controls.Add(this.labelDateTime);
			this.Controls.Add(this.textNote);
			this.Controls.Add(this.labelNote);
			this.Name = "FormSheetFillEdit";
			this.Text = "Fill Sheet";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormSheetFillEdit_FormClosing);
			this.Load += new System.EventHandler(this.FormSheetFillEdit_Load);
			this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.FormSheetFillEdit_MouseUp);
			this.panelScroll.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private OpenPmsDental.UI.Button butOK;
		private System.Windows.Forms.TextBox textNote;
		private System.Windows.Forms.Label labelNote;
		private System.Windows.Forms.Label labelDateTime;
		private OpenPmsDental.PanelScrollManual panelScroll;
		private OpenPmsDental.UI.ControlDoubleBuffered panelMain;
		private OpenPmsDental.UI.Button butDelete;
		private OpenPmsDental.UI.Button butPrintOrEmail;
		private OpenPmsDental.UI.CheckBox checkErase;
		private System.Windows.Forms.TextBox textDescription;
		private System.Windows.Forms.Label labelDescription;
		private System.Windows.Forms.Label labelShowInTerminal;
		private ValidNum textShowInTerminal;
		private System.Windows.Forms.TextBox textDateTime;
		private UI.Button butUnlock;
		private UI.Button butChangePat;
		private UI.Button butRestore;
		private UI.Button butSave;
		private UI.Button butPrint;
		private UI.Button butEmail;
		private UI.Button butToKiosk;
		private UI.Button butPDF;
		private UI.Button butAddField;
		private System.Windows.Forms.Timer timerTextChanged;
		private UI.Button butReduceFontSize;
		private UI.Button butFontAbout;
	}
}