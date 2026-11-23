namespace OpenPmsDental{
	partial class FormAdvertisingPostcardsSend {
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
			if(disposing) {
				pictureProof.Image?.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.menuMain = new OpenPmsDental.UI.MenuOD();
			this.labelSandbox = new System.Windows.Forms.Label();
			this.labelClinic = new System.Windows.Forms.Label();
			this.comboClinic = new OpenPmsDental.UI.ComboBox();
			this.labelRecipients = new System.Windows.Forms.Label();
			this.textRecipients = new System.Windows.Forms.TextBox();
			this.butSelectPatients = new OpenPmsDental.UI.Button();
			this.checkHousehold = new OpenPmsDental.UI.CheckBox();
			this.labelRecipientNotes = new System.Windows.Forms.Label();
			this.labelDesign = new System.Windows.Forms.Label();
			this.comboDesign = new OpenPmsDental.UI.ComboBox();
			this.butRefreshDesigns = new OpenPmsDental.UI.Button();
			this.butEditDesign = new OpenPmsDental.UI.Button();
			this.butNewDesign = new OpenPmsDental.UI.Button();
			this.labelDesignWarning = new System.Windows.Forms.Label();
			this.labelMailClass = new System.Windows.Forms.Label();
			this.comboMailClass = new OpenPmsDental.UI.ComboBox();
			this.labelMailDate = new System.Windows.Forms.Label();
			this.textMailDate = new OpenPmsDental.ValidDate();
			this.labelReturnAddress = new System.Windows.Forms.Label();
			this.labelReturnAddressValue = new System.Windows.Forms.Label();
			this.butCheckAddresses = new OpenPmsDental.UI.Button();
			this.butEstimate = new OpenPmsDental.UI.Button();
			this.butSend = new OpenPmsDental.UI.Button();
			this.labelEstimate = new System.Windows.Forms.Label();
			this.labelBaaBlocked = new System.Windows.Forms.Label();
			this.pictureProof = new System.Windows.Forms.PictureBox();
			this.labelProof = new System.Windows.Forms.Label();
			this.gridOrders = new OpenPmsDental.UI.GridOD();
			this.checkAllOrders = new OpenPmsDental.UI.CheckBox();
			this.butRefreshOrders = new OpenPmsDental.UI.Button();
			this.butCancelOrder = new OpenPmsDental.UI.Button();
			this.labelCutoff = new System.Windows.Forms.Label();
			this.butOpenPortal = new OpenPmsDental.UI.Button();
			this.butClose = new OpenPmsDental.UI.Button();
			((System.ComponentModel.ISupportInitialize)(this.pictureProof)).BeginInit();
			this.SuspendLayout();
			//
			// menuMain
			//
			this.menuMain.Location = new System.Drawing.Point(8, 8);
			this.menuMain.Name = "menuMain";
			this.menuMain.Size = new System.Drawing.Size(820, 24);
			this.menuMain.TabIndex = 0;
			this.menuMain.Text = "Menu";
			//
			// labelSandbox
			//
			this.labelSandbox.ForeColor = System.Drawing.Color.Red;
			this.labelSandbox.Location = new System.Drawing.Point(8, 36);
			this.labelSandbox.Name = "labelSandbox";
			this.labelSandbox.Size = new System.Drawing.Size(804, 18);
			this.labelSandbox.TabIndex = 1;
			this.labelSandbox.Text = "Sandbox: nothing will be printed or mailed.";
			//
			// labelClinic
			//
			this.labelClinic.Location = new System.Drawing.Point(8, 58);
			this.labelClinic.Name = "labelClinic";
			this.labelClinic.Size = new System.Drawing.Size(106, 23);
			this.labelClinic.TabIndex = 2;
			this.labelClinic.Text = "Clinic";
			this.labelClinic.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			//
			// comboClinic
			//
			this.comboClinic.Location = new System.Drawing.Point(120, 58);
			this.comboClinic.Name = "comboClinic";
			this.comboClinic.Size = new System.Drawing.Size(200, 23);
			this.comboClinic.TabIndex = 3;
			this.comboClinic.SelectionChangeCommitted += new System.EventHandler(this.comboClinic_SelectionChangeCommitted);
			//
			// labelRecipients
			//
			this.labelRecipients.Location = new System.Drawing.Point(8, 85);
			this.labelRecipients.Name = "labelRecipients";
			this.labelRecipients.Size = new System.Drawing.Size(106, 23);
			this.labelRecipients.TabIndex = 4;
			this.labelRecipients.Text = "Recipients";
			this.labelRecipients.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			//
			// textRecipients
			//
			this.textRecipients.Location = new System.Drawing.Point(120, 85);
			this.textRecipients.Name = "textRecipients";
			this.textRecipients.ReadOnly = true;
			this.textRecipients.Size = new System.Drawing.Size(80, 23);
			this.textRecipients.TabIndex = 5;
			//
			// butSelectPatients
			//
			this.butSelectPatients.Location = new System.Drawing.Point(206, 85);
			this.butSelectPatients.Name = "butSelectPatients";
			this.butSelectPatients.Size = new System.Drawing.Size(110, 23);
			this.butSelectPatients.TabIndex = 6;
			this.butSelectPatients.Text = "&Select Patients";
			this.butSelectPatients.Click += new System.EventHandler(this.butSelectPatients_Click);
			//
			// checkHousehold
			//
			this.checkHousehold.Location = new System.Drawing.Point(120, 113);
			this.checkHousehold.Name = "checkHousehold";
			this.checkHousehold.Size = new System.Drawing.Size(309, 20);
			this.checkHousehold.TabIndex = 7;
			this.checkHousehold.Text = "One card per household, addressed to the guarantor";
			this.checkHousehold.Click += new System.EventHandler(this.checkHousehold_Click);
			//
			// labelRecipientNotes
			//
			this.labelRecipientNotes.Location = new System.Drawing.Point(120, 139);
			this.labelRecipientNotes.Name = "labelRecipientNotes";
			this.labelRecipientNotes.Size = new System.Drawing.Size(350, 30);
			this.labelRecipientNotes.TabIndex = 8;
			//
			// labelDesign
			//
			this.labelDesign.Location = new System.Drawing.Point(8, 173);
			this.labelDesign.Name = "labelDesign";
			this.labelDesign.Size = new System.Drawing.Size(106, 23);
			this.labelDesign.TabIndex = 9;
			this.labelDesign.Text = "Design";
			this.labelDesign.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			//
			// comboDesign
			//
			this.comboDesign.Location = new System.Drawing.Point(120, 173);
			this.comboDesign.Name = "comboDesign";
			this.comboDesign.Size = new System.Drawing.Size(272, 23);
			this.comboDesign.TabIndex = 10;
			this.comboDesign.SelectionChangeCommitted += new System.EventHandler(this.comboDesign_SelectionChangeCommitted);
			//
			// butRefreshDesigns
			//
			this.butRefreshDesigns.Location = new System.Drawing.Point(398, 173);
			this.butRefreshDesigns.Name = "butRefreshDesigns";
			this.butRefreshDesigns.Size = new System.Drawing.Size(75, 23);
			this.butRefreshDesigns.TabIndex = 11;
			this.butRefreshDesigns.Text = "Refresh";
			this.butRefreshDesigns.Click += new System.EventHandler(this.butRefreshDesigns_Click);
			//
			// butEditDesign
			//
			this.butEditDesign.Location = new System.Drawing.Point(120, 200);
			this.butEditDesign.Name = "butEditDesign";
			this.butEditDesign.Size = new System.Drawing.Size(100, 24);
			this.butEditDesign.TabIndex = 12;
			this.butEditDesign.Text = "Edit design";
			this.butEditDesign.Click += new System.EventHandler(this.butEditDesign_Click);
			//
			// butNewDesign
			//
			this.butNewDesign.Location = new System.Drawing.Point(226, 200);
			this.butNewDesign.Name = "butNewDesign";
			this.butNewDesign.Size = new System.Drawing.Size(100, 24);
			this.butNewDesign.TabIndex = 13;
			this.butNewDesign.Text = "New design";
			this.butNewDesign.Click += new System.EventHandler(this.butNewDesign_Click);
			//
			// labelDesignWarning
			//
			this.labelDesignWarning.ForeColor = System.Drawing.Color.Firebrick;
			this.labelDesignWarning.Location = new System.Drawing.Point(120, 228);
			this.labelDesignWarning.Name = "labelDesignWarning";
			this.labelDesignWarning.Size = new System.Drawing.Size(350, 30);
			this.labelDesignWarning.TabIndex = 14;
			//
			// labelMailClass
			//
			this.labelMailClass.Location = new System.Drawing.Point(8, 262);
			this.labelMailClass.Name = "labelMailClass";
			this.labelMailClass.Size = new System.Drawing.Size(106, 23);
			this.labelMailClass.TabIndex = 15;
			this.labelMailClass.Text = "Mail class";
			this.labelMailClass.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			//
			// comboMailClass
			//
			this.comboMailClass.Location = new System.Drawing.Point(120, 262);
			this.comboMailClass.Name = "comboMailClass";
			this.comboMailClass.Size = new System.Drawing.Size(152, 23);
			this.comboMailClass.TabIndex = 16;
			this.comboMailClass.SelectionChangeCommitted += new System.EventHandler(this.comboMailClass_SelectionChangeCommitted);
			//
			// labelMailDate
			//
			this.labelMailDate.Location = new System.Drawing.Point(8, 289);
			this.labelMailDate.Name = "labelMailDate";
			this.labelMailDate.Size = new System.Drawing.Size(106, 23);
			this.labelMailDate.TabIndex = 17;
			this.labelMailDate.Text = "Mail date";
			this.labelMailDate.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			//
			// textMailDate
			//
			this.textMailDate.Location = new System.Drawing.Point(120, 289);
			this.textMailDate.Name = "textMailDate";
			this.textMailDate.Size = new System.Drawing.Size(100, 23);
			this.textMailDate.TabIndex = 18;
			this.textMailDate.TextChanged += new System.EventHandler(this.textMailDate_TextChanged);
			//
			// labelReturnAddress
			//
			this.labelReturnAddress.Location = new System.Drawing.Point(8, 316);
			this.labelReturnAddress.Name = "labelReturnAddress";
			this.labelReturnAddress.Size = new System.Drawing.Size(106, 23);
			this.labelReturnAddress.TabIndex = 19;
			this.labelReturnAddress.Text = "Return address";
			this.labelReturnAddress.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			//
			// labelReturnAddressValue
			//
			this.labelReturnAddressValue.Location = new System.Drawing.Point(120, 319);
			this.labelReturnAddressValue.Name = "labelReturnAddressValue";
			this.labelReturnAddressValue.Size = new System.Drawing.Size(350, 62);
			this.labelReturnAddressValue.TabIndex = 20;
			//
			// butCheckAddresses
			//
			this.butCheckAddresses.Location = new System.Drawing.Point(120, 385);
			this.butCheckAddresses.Name = "butCheckAddresses";
			this.butCheckAddresses.Size = new System.Drawing.Size(110, 24);
			this.butCheckAddresses.TabIndex = 21;
			this.butCheckAddresses.Text = "Check addresses";
			this.butCheckAddresses.Click += new System.EventHandler(this.butCheckAddresses_Click);
			//
			// butEstimate
			//
			this.butEstimate.Location = new System.Drawing.Point(236, 385);
			this.butEstimate.Name = "butEstimate";
			this.butEstimate.Size = new System.Drawing.Size(90, 24);
			this.butEstimate.TabIndex = 22;
			this.butEstimate.Text = "&Estimate";
			this.butEstimate.Click += new System.EventHandler(this.butEstimate_Click);
			//
			// butSend
			//
			this.butSend.Location = new System.Drawing.Point(332, 385);
			this.butSend.Name = "butSend";
			this.butSend.Size = new System.Drawing.Size(110, 24);
			this.butSend.TabIndex = 23;
			this.butSend.Text = "Place &order...";
			this.butSend.Click += new System.EventHandler(this.butSend_Click);
			//
			// labelEstimate
			//
			this.labelEstimate.Location = new System.Drawing.Point(120, 413);
			this.labelEstimate.Name = "labelEstimate";
			this.labelEstimate.Size = new System.Drawing.Size(350, 62);
			this.labelEstimate.TabIndex = 24;
			//
			// labelBaaBlocked
			//
			this.labelBaaBlocked.ForeColor = System.Drawing.Color.Firebrick;
			this.labelBaaBlocked.Location = new System.Drawing.Point(8, 483);
			this.labelBaaBlocked.Name = "labelBaaBlocked";
			this.labelBaaBlocked.Size = new System.Drawing.Size(552, 32);
			this.labelBaaBlocked.TabIndex = 25;
			this.labelBaaBlocked.Text = "Check addresses, Estimate and Place order are blocked: Setup does not record a signed business associate agreement with the vendor.";
			//
			// pictureProof
			//
			this.pictureProof.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.pictureProof.Location = new System.Drawing.Point(490, 78);
			this.pictureProof.Name = "pictureProof";
			this.pictureProof.Size = new System.Drawing.Size(322, 220);
			this.pictureProof.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.pictureProof.TabIndex = 27;
			this.pictureProof.TabStop = false;
			//
			// labelProof
			//
			this.labelProof.Location = new System.Drawing.Point(490, 58);
			this.labelProof.Name = "labelProof";
			this.labelProof.Size = new System.Drawing.Size(322, 18);
			this.labelProof.TabIndex = 26;
			this.labelProof.Text = "Proof (front)";
			//
			// gridOrders
			//
			this.gridOrders.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
			this.gridOrders.Location = new System.Drawing.Point(8, 519);
			this.gridOrders.Name = "gridOrders";
			this.gridOrders.Size = new System.Drawing.Size(804, 139);
			this.gridOrders.TabIndex = 28;
			this.gridOrders.Title = "Recent Orders";
			this.gridOrders.TranslationName = "TablePostcardOrders";
			this.gridOrders.CellClick += new OpenPmsDental.UI.ODGridClickEventHandler(this.gridOrders_CellClick);
			//
			// checkAllOrders
			//
			this.checkAllOrders.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkAllOrders.Location = new System.Drawing.Point(568, 495);
			this.checkAllOrders.Name = "checkAllOrders";
			this.checkAllOrders.Size = new System.Drawing.Size(244, 20);
			this.checkAllOrders.TabIndex = 29;
			this.checkAllOrders.Text = "Show every order on the vendor account";
			this.checkAllOrders.Click += new System.EventHandler(this.checkAllOrders_Click);
			//
			// butRefreshOrders
			//
			this.butRefreshOrders.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butRefreshOrders.Location = new System.Drawing.Point(8, 666);
			this.butRefreshOrders.Name = "butRefreshOrders";
			this.butRefreshOrders.Size = new System.Drawing.Size(80, 24);
			this.butRefreshOrders.TabIndex = 30;
			this.butRefreshOrders.Text = "&Refresh";
			this.butRefreshOrders.Click += new System.EventHandler(this.butRefreshOrders_Click);
			//
			// butCancelOrder
			//
			this.butCancelOrder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butCancelOrder.Location = new System.Drawing.Point(94, 666);
			this.butCancelOrder.Name = "butCancelOrder";
			this.butCancelOrder.Size = new System.Drawing.Size(95, 24);
			this.butCancelOrder.TabIndex = 31;
			this.butCancelOrder.Text = "Cancel order";
			this.butCancelOrder.Click += new System.EventHandler(this.butCancelOrder_Click);
			//
			// labelCutoff
			//
			this.labelCutoff.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.labelCutoff.Location = new System.Drawing.Point(195, 663);
			this.labelCutoff.Name = "labelCutoff";
			this.labelCutoff.Size = new System.Drawing.Size(398, 30);
			this.labelCutoff.TabIndex = 32;
			this.labelCutoff.Text = "Pending orders can be canceled until 11:30 PM Eastern on the day they were placed.";
			//
			// butOpenPortal
			//
			this.butOpenPortal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butOpenPortal.Location = new System.Drawing.Point(599, 666);
			this.butOpenPortal.Name = "butOpenPortal";
			this.butOpenPortal.Size = new System.Drawing.Size(130, 24);
			this.butOpenPortal.TabIndex = 33;
			this.butOpenPortal.Text = "Open vendor portal";
			this.butOpenPortal.Click += new System.EventHandler(this.butOpenPortal_Click);
			//
			// butClose
			//
			this.butClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butClose.Location = new System.Drawing.Point(737, 666);
			this.butClose.Name = "butClose";
			this.butClose.Size = new System.Drawing.Size(75, 24);
			this.butClose.TabIndex = 34;
			this.butClose.Text = "&Close";
			this.butClose.Click += new System.EventHandler(this.butClose_Click);
			//
			// FormAdvertisingPostcardsSend
			//
			this.ClientSize = new System.Drawing.Size(820, 698);
			this.Controls.Add(this.butClose);
			this.Controls.Add(this.butOpenPortal);
			this.Controls.Add(this.labelCutoff);
			this.Controls.Add(this.butCancelOrder);
			this.Controls.Add(this.butRefreshOrders);
			this.Controls.Add(this.checkAllOrders);
			this.Controls.Add(this.gridOrders);
			this.Controls.Add(this.labelProof);
			this.Controls.Add(this.pictureProof);
			this.Controls.Add(this.labelBaaBlocked);
			this.Controls.Add(this.labelEstimate);
			this.Controls.Add(this.butSend);
			this.Controls.Add(this.butEstimate);
			this.Controls.Add(this.butCheckAddresses);
			this.Controls.Add(this.labelReturnAddressValue);
			this.Controls.Add(this.labelReturnAddress);
			this.Controls.Add(this.textMailDate);
			this.Controls.Add(this.labelMailDate);
			this.Controls.Add(this.comboMailClass);
			this.Controls.Add(this.labelMailClass);
			this.Controls.Add(this.labelDesignWarning);
			this.Controls.Add(this.butNewDesign);
			this.Controls.Add(this.butEditDesign);
			this.Controls.Add(this.butRefreshDesigns);
			this.Controls.Add(this.comboDesign);
			this.Controls.Add(this.labelDesign);
			this.Controls.Add(this.labelRecipientNotes);
			this.Controls.Add(this.checkHousehold);
			this.Controls.Add(this.butSelectPatients);
			this.Controls.Add(this.textRecipients);
			this.Controls.Add(this.labelRecipients);
			this.Controls.Add(this.comboClinic);
			this.Controls.Add(this.labelClinic);
			this.Controls.Add(this.labelSandbox);
			this.Controls.Add(this.menuMain);
			this.Name = "FormAdvertisingPostcardsSend";
			this.Text = "Postcards";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormAdvertisingPostcardsSend_FormClosing);
			this.Load += new System.EventHandler(this.FormAdvertisingPostcardsSend_Load);
			((System.ComponentModel.ISupportInitialize)(this.pictureProof)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private OpenPmsDental.UI.MenuOD menuMain;
		private System.Windows.Forms.Label labelSandbox;
		private System.Windows.Forms.Label labelClinic;
		private OpenPmsDental.UI.ComboBox comboClinic;
		private System.Windows.Forms.Label labelRecipients;
		private System.Windows.Forms.TextBox textRecipients;
		private OpenPmsDental.UI.Button butSelectPatients;
		private OpenPmsDental.UI.CheckBox checkHousehold;
		private System.Windows.Forms.Label labelRecipientNotes;
		private System.Windows.Forms.Label labelDesign;
		private OpenPmsDental.UI.ComboBox comboDesign;
		private OpenPmsDental.UI.Button butRefreshDesigns;
		private OpenPmsDental.UI.Button butEditDesign;
		private OpenPmsDental.UI.Button butNewDesign;
		private System.Windows.Forms.Label labelDesignWarning;
		private System.Windows.Forms.Label labelMailClass;
		private OpenPmsDental.UI.ComboBox comboMailClass;
		private System.Windows.Forms.Label labelMailDate;
		private OpenPmsDental.ValidDate textMailDate;
		private System.Windows.Forms.Label labelReturnAddress;
		private System.Windows.Forms.Label labelReturnAddressValue;
		private OpenPmsDental.UI.Button butCheckAddresses;
		private OpenPmsDental.UI.Button butEstimate;
		private OpenPmsDental.UI.Button butSend;
		private System.Windows.Forms.Label labelEstimate;
		private System.Windows.Forms.Label labelBaaBlocked;
		private System.Windows.Forms.PictureBox pictureProof;
		private System.Windows.Forms.Label labelProof;
		private OpenPmsDental.UI.GridOD gridOrders;
		private OpenPmsDental.UI.CheckBox checkAllOrders;
		private OpenPmsDental.UI.Button butRefreshOrders;
		private OpenPmsDental.UI.Button butCancelOrder;
		private System.Windows.Forms.Label labelCutoff;
		private OpenPmsDental.UI.Button butOpenPortal;
		private OpenPmsDental.UI.Button butClose;
	}
}
