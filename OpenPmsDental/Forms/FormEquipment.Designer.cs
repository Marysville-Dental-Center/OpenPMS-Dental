namespace OpenPmsDental{
	partial class FormEquipment {
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
			this.label1 = new System.Windows.Forms.Label();
			this.groupBox1 = new OpenPmsDental.UI.GroupBox();
			this.butRefresh = new OpenPmsDental.UI.Button();
			this.textDateStart = new OpenPmsDental.ValidDate();
			this.textDateEnd = new OpenPmsDental.ValidDate();
			this.radioPurchased = new System.Windows.Forms.RadioButton();
			this.radioSold = new System.Windows.Forms.RadioButton();
			this.radioAll = new System.Windows.Forms.RadioButton();
			this.textSnDesc = new System.Windows.Forms.TextBox();
			this.label7 = new System.Windows.Forms.Label();
			this.butPrint = new OpenPmsDental.UI.Button();
			this.butAdd = new OpenPmsDental.UI.Button();
			this.gridMain = new OpenPmsDental.UI.GridOD();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(12, 8);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(170, 39);
			this.label1.TabIndex = 7;
			this.label1.Text = "This list tracks equipment for payment of property taxes.";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.butRefresh);
			this.groupBox1.Controls.Add(this.textDateStart);
			this.groupBox1.Controls.Add(this.textDateEnd);
			this.groupBox1.Location = new System.Drawing.Point(426, 8);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(257, 54);
			this.groupBox1.TabIndex = 23;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Date Range";
			// 
			// butRefresh
			// 
			this.butRefresh.Location = new System.Drawing.Point(171, 23);
			this.butRefresh.Name = "butRefresh";
			this.butRefresh.Size = new System.Drawing.Size(78, 23);
			this.butRefresh.TabIndex = 23;
			this.butRefresh.Text = "Refresh";
			this.butRefresh.Click += new System.EventHandler(this.butRefresh_Click);
			// 
			// textDateStart
			// 
			this.textDateStart.Location = new System.Drawing.Point(8, 23);
			this.textDateStart.Name = "textDateStart";
			this.textDateStart.Size = new System.Drawing.Size(74, 23);
			this.textDateStart.TabIndex = 21;
			// 
			// textDateEnd
			// 
			this.textDateEnd.Location = new System.Drawing.Point(88, 23);
			this.textDateEnd.Name = "textDateEnd";
			this.textDateEnd.Size = new System.Drawing.Size(77, 20);
			this.textDateEnd.TabIndex = 22;
			// 
			// radioPurchased
			// 
			this.radioPurchased.Location = new System.Drawing.Point(697, 8);
			this.radioPurchased.Name = "radioPurchased";
			this.radioPurchased.Size = new System.Drawing.Size(80, 18);
			this.radioPurchased.TabIndex = 23;
			this.radioPurchased.Text = "Purchased";
			this.radioPurchased.UseVisualStyleBackColor = true;
			this.radioPurchased.Click += new System.EventHandler(this.radioPurchased_Click);
			// 
			// radioSold
			// 
			this.radioSold.Location = new System.Drawing.Point(697, 30);
			this.radioSold.Name = "radioSold";
			this.radioSold.Size = new System.Drawing.Size(48, 18);
			this.radioSold.TabIndex = 24;
			this.radioSold.Text = "Sold";
			this.radioSold.UseVisualStyleBackColor = true;
			this.radioSold.Click += new System.EventHandler(this.radioSold_Click);
			// 
			// radioAll
			// 
			this.radioAll.Checked = true;
			this.radioAll.Location = new System.Drawing.Point(697, 52);
			this.radioAll.Name = "radioAll";
			this.radioAll.Size = new System.Drawing.Size(39, 18);
			this.radioAll.TabIndex = 25;
			this.radioAll.TabStop = true;
			this.radioAll.Text = "All";
			this.radioAll.UseVisualStyleBackColor = true;
			this.radioAll.Click += new System.EventHandler(this.radioAll_Click);
			// 
			// textSnDesc
			// 
			this.textSnDesc.Location = new System.Drawing.Point(288, 24);
			this.textSnDesc.Name = "textSnDesc";
			this.textSnDesc.Size = new System.Drawing.Size(132, 23);
			this.textSnDesc.TabIndex = 39;
			this.textSnDesc.TextChanged += new System.EventHandler(this.textSn_TextChanged);
			// 
			// label7
			// 
			this.label7.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.label7.Location = new System.Drawing.Point(186, 24);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(96, 23);
			this.label7.TabIndex = 38;
			this.label7.Text = "SN/Descipt/Loc";
			this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// butPrint
			// 
			this.butPrint.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butPrint.Icon = OpenPmsDental.UI.EnumIcons.Print;
			this.butPrint.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butPrint.Location = new System.Drawing.Point(93, 554);
			this.butPrint.Name = "butPrint";
			this.butPrint.Size = new System.Drawing.Size(75, 24);
			this.butPrint.TabIndex = 8;
			this.butPrint.Text = "Print";
			this.butPrint.Click += new System.EventHandler(this.butPrint_Click);
			// 
			// butAdd
			// 
			this.butAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butAdd.Icon = OpenPmsDental.UI.EnumIcons.Add;
			this.butAdd.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butAdd.Location = new System.Drawing.Point(12, 554);
			this.butAdd.Name = "butAdd";
			this.butAdd.Size = new System.Drawing.Size(75, 24);
			this.butAdd.TabIndex = 6;
			this.butAdd.Text = "Add";
			this.butAdd.Click += new System.EventHandler(this.butAdd_Click);
			// 
			// gridMain
			// 
			this.gridMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.gridMain.Location = new System.Drawing.Point(12, 78);
			this.gridMain.Name = "gridMain";
			this.gridMain.Size = new System.Drawing.Size(950, 464);
			this.gridMain.TabIndex = 5;
			this.gridMain.Title = "Equipment";
			this.gridMain.TranslationName = "TableEquipment";
			this.gridMain.CellDoubleClick += new OpenPmsDental.UI.ODGridClickEventHandler(this.gridMain_CellDoubleClick);
			// 
			// FormEquipment
			// 
			this.ClientSize = new System.Drawing.Size(974, 586);
			this.Controls.Add(this.textSnDesc);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.radioAll);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.radioSold);
			this.Controls.Add(this.radioPurchased);
			this.Controls.Add(this.butPrint);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.butAdd);
			this.Controls.Add(this.gridMain);
			this.Name = "FormEquipment";
			this.Text = "Equipment";
			this.Load += new System.EventHandler(this.FormEquipment_Load);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private OpenPmsDental.UI.GridOD gridMain;
		private OpenPmsDental.UI.Button butAdd;
		private System.Windows.Forms.Label label1;
		private OpenPmsDental.UI.Button butPrint;
		private ValidDate textDateEnd;
		private ValidDate textDateStart;
		private OpenPmsDental.UI.GroupBox groupBox1;
		private System.Windows.Forms.RadioButton radioPurchased;
		private System.Windows.Forms.RadioButton radioSold;
		private System.Windows.Forms.RadioButton radioAll;
		private OpenPmsDental.UI.Button butRefresh;
		private System.Windows.Forms.TextBox textSnDesc;
		private System.Windows.Forms.Label label7;
	}
}