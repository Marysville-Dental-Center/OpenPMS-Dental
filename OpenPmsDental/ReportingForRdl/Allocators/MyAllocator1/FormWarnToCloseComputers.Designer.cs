namespace OpenPmsDental.Reporting.Allocators.MyAllocator1
{
	partial class FormWarnToCloseComputers
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormWarnToCloseComputers));
			this.lblMessage = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.butYes = new OpenPmsDental.UI.Button();
			this.butNoCancel = new OpenPmsDental.UI.Button();
			this.panel1 = new System.Windows.Forms.Panel();
			this.label2 = new System.Windows.Forms.Label();
			this.progressRingWait = new CodeBase.UiProgressRing();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// lblMessage
			// 
			this.lblMessage.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.lblMessage.Font = new System.Drawing.Font("Segoe UI", 13.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblMessage.ForeColor = System.Drawing.Color.Maroon;
			this.lblMessage.Location = new System.Drawing.Point(8, 8);
			this.lblMessage.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblMessage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.lblMessage.Name = "lblMessage";
			this.lblMessage.Size = new System.Drawing.Size(767, 161);
			this.lblMessage.TabIndex = 0;
			this.lblMessage.Text = resources.GetString("lblMessage.Text");
			// 
			// label1
			// 
			this.label1.Font = new System.Drawing.Font("Segoe UI", 13.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.ForeColor = System.Drawing.Color.Maroon;
			this.label1.Location = new System.Drawing.Point(260, 255);
			this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(320, 34);
			this.label1.TabIndex = 1;
			this.label1.Text = "Is this the only computer running?";
			// 
			// butYes
			// 
			this.butYes.Location = new System.Drawing.Point(586, 255);
			this.butYes.Margin = new System.Windows.Forms.Padding(4);
			this.butYes.Name = "butYes";
			this.butYes.Size = new System.Drawing.Size(64, 34);
			this.butYes.TabIndex = 2;
			this.butYes.Text = "Yes";
			this.butYes.UseVisualStyleBackColor = true;
			this.butYes.Click += new System.EventHandler(this.butYes_Click);
			// 
			// butNoCancel
			// 
			this.butNoCancel.Location = new System.Drawing.Point(658, 255);
			this.butNoCancel.Margin = new System.Windows.Forms.Padding(4);
			this.butNoCancel.Name = "butNoCancel";
			this.butNoCancel.Size = new System.Drawing.Size(117, 34);
			this.butNoCancel.TabIndex = 3;
			this.butNoCancel.Text = "No/Cancel";
			this.butNoCancel.UseVisualStyleBackColor = true;
			this.butNoCancel.Click += new System.EventHandler(this.butNoCancel_Click);
			// 
			// panel1
			// 
			this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.panel1.Controls.Add(this.label2);
			this.panel1.Controls.Add(this.progressRingWait);
			this.panel1.Location = new System.Drawing.Point(8, 177);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(767, 66);
			this.panel1.TabIndex = 4;
			// 
			// label2
			// 
			this.label2.Font = new System.Drawing.Font("Segoe UI", 13.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label2.ForeColor = System.Drawing.Color.Maroon;
			this.label2.Location = new System.Drawing.Point(64, 16);
			this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(368, 34);
			this.label2.TabIndex = 5;
			this.label2.Text = "Close Other OD Programs Now!!";
			// 
			// progressRingWait
			// 
			this.progressRingWait.Location = new System.Drawing.Point(8, 8);
			this.progressRingWait.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
			this.progressRingWait.Name = "progressRingWait";
			this.progressRingWait.Size = new System.Drawing.Size(50, 50);
			this.progressRingWait.TabIndex = 5;
			this.progressRingWait.TabStop = false;
			// 
			// FormWarnToCloseComputers
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = new System.Drawing.Size(783, 297);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.butNoCancel);
			this.Controls.Add(this.butYes);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.lblMessage);
			this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ForeColor = System.Drawing.Color.Maroon;
			this.Margin = new System.Windows.Forms.Padding(4);
			this.Name = "FormWarnToCloseComputers";
			this.Text = "Warning";
			this.panel1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label lblMessage;
		private System.Windows.Forms.Label label1;
		private OpenPmsDental.UI.Button butYes;
		private OpenPmsDental.UI.Button butNoCancel;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Label label2;
		private CodeBase.UiProgressRing progressRingWait;
	}
}