namespace DocumentationBuilder {
	partial class Form1 {
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
			this.label1 = new System.Windows.Forms.Label();
			this.butBuild = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.textVersion = new System.Windows.Forms.TextBox();
			this.textConnStr = new System.Windows.Forms.TextBox();
			this.textPrevVersion = new System.Windows.Forms.TextBox();
			this.labelPrevVersion = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.butBuildUnitTest = new System.Windows.Forms.Button();
			this.label7 = new System.Windows.Forms.Label();
			this.labelPrefDocumentation = new System.Windows.Forms.Label();
			this.buttonPrefDocumentation = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(8, 8);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(558, 47);
			this.label1.TabIndex = 0;
			this.label1.Text = resources.GetString("label1.Text");
			// 
			// butBuild
			// 
			this.butBuild.Location = new System.Drawing.Point(212, 256);
			this.butBuild.Name = "butBuild";
			this.butBuild.Size = new System.Drawing.Size(151, 28);
			this.butBuild.TabIndex = 1;
			this.butBuild.Text = "Build OD Documentation";
			this.butBuild.UseVisualStyleBackColor = true;
			this.butBuild.Click += new System.EventHandler(this.butBuild_Click);
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(8, 57);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(558, 32);
			this.label2.TabIndex = 2;
			this.label2.Text = "Step 1: Build the release of OpenPmsDental and UnitTests. This generates OpenPmsDentalBus" +
    "iness.xml etc. which contain all the xml Summaries that we will then pull from. " +
    "";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(8, 91);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(558, 45);
			this.label3.TabIndex = 3;
			this.label3.Text = resources.GetString("label3.Text");
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(8, 201);
			this.label4.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(558, 18);
			this.label4.TabIndex = 4;
			this.label4.Text = "Step 5: Click one of the Build buttons below.";
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(56, 147);
			this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(152, 23);
			this.label6.TabIndex = 6;
			this.label6.Text = "Step 3: Specify the version:";
			// 
			// textVersion
			// 
			this.textVersion.Location = new System.Drawing.Point(214, 147);
			this.textVersion.Name = "textVersion";
			this.textVersion.Size = new System.Drawing.Size(60, 23);
			this.textVersion.TabIndex = 7;
			// 
			// textConnStr
			// 
			this.textConnStr.Location = new System.Drawing.Point(11, 120);
			this.textConnStr.Name = "textConnStr";
			this.textConnStr.ReadOnly = true;
			this.textConnStr.Size = new System.Drawing.Size(541, 20);
			this.textConnStr.TabIndex = 8;
			// 
			// textPrevVersion
			// 
			this.textPrevVersion.Location = new System.Drawing.Point(214, 174);
			this.textPrevVersion.Name = "textPrevVersion";
			this.textPrevVersion.Size = new System.Drawing.Size(60, 23);
			this.textPrevVersion.TabIndex = 12;
			// 
			// labelPrevVersion
			// 
			this.labelPrevVersion.Location = new System.Drawing.Point(8, 174);
			this.labelPrevVersion.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.labelPrevVersion.Name = "labelPrevVersion";
			this.labelPrevVersion.Size = new System.Drawing.Size(200, 23);
			this.labelPrevVersion.TabIndex = 11;
			this.labelPrevVersion.Text = "Step 4: Specify the previous version:";
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(8, 288);
			this.label5.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(558, 33);
			this.label5.TabIndex = 14;
			this.label5.Text = "The output file is DocumentationBuilder/UnitTestsDocumentation.xml.\r\nApproximate " +
    "time to complete is 5 seconds on a fast computer";
			// 
			// butBuildUnitTest
			// 
			this.butBuildUnitTest.Location = new System.Drawing.Point(196, 323);
			this.butBuildUnitTest.Name = "butBuildUnitTest";
			this.butBuildUnitTest.Size = new System.Drawing.Size(182, 28);
			this.butBuildUnitTest.TabIndex = 13;
			this.butBuildUnitTest.Text = "Build Unit Test Documentation";
			this.butBuildUnitTest.UseVisualStyleBackColor = true;
			this.butBuildUnitTest.Click += new System.EventHandler(this.butBuildUnitTest_Click);
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(8, 221);
			this.label7.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(558, 33);
			this.label7.TabIndex = 15;
			this.label7.Text = "The output file is DocumentationBuilder/OpenDentalDocumentation.xml, which will b" +
    "e automatically launched when done.  Approximate time to complete is 20 seconds " +
    "on a fast computer.";
			// 
			// labelPrefDocumentation
			// 
			this.labelPrefDocumentation.Location = new System.Drawing.Point(8, 355);
			this.labelPrefDocumentation.Name = "labelPrefDocumentation";
			this.labelPrefDocumentation.Size = new System.Drawing.Size(558, 45);
			this.labelPrefDocumentation.TabIndex = 17;
			this.labelPrefDocumentation.Text = "Approximate time to complete is 3 seconds on a fast computer\r\nThen, manually copy" +
    " Pref(version).html to unversioned\\ODHelpFaqSite\\ODHelpFaqSite\\PreferenceTable\\";
			this.labelPrefDocumentation.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// buttonPrefDocumentation
			// 
			this.buttonPrefDocumentation.Location = new System.Drawing.Point(207, 402);
			this.buttonPrefDocumentation.Name = "buttonPrefDocumentation";
			this.buttonPrefDocumentation.Size = new System.Drawing.Size(161, 28);
			this.buttonPrefDocumentation.TabIndex = 16;
			this.buttonPrefDocumentation.Text = "Build Pref Documentation";
			this.buttonPrefDocumentation.UseVisualStyleBackColor = true;
			this.buttonPrefDocumentation.Click += new System.EventHandler(this.buttonPrefDocumentation_Click);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.ClientSize = new System.Drawing.Size(612, 508);
			this.Controls.Add(this.labelPrefDocumentation);
			this.Controls.Add(this.buttonPrefDocumentation);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.butBuildUnitTest);
			this.Controls.Add(this.textPrevVersion);
			this.Controls.Add(this.labelPrevVersion);
			this.Controls.Add(this.textConnStr);
			this.Controls.Add(this.textVersion);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.butBuild);
			this.Controls.Add(this.label1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "Form1";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Documentation Builder";
			this.Load += new System.EventHandler(this.Form1_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button butBuild;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.TextBox textVersion;
		private System.Windows.Forms.TextBox textConnStr;
		private System.Windows.Forms.TextBox textPrevVersion;
		private System.Windows.Forms.Label labelPrevVersion;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Button butBuildUnitTest;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label labelPrefDocumentation;
		private System.Windows.Forms.Button buttonPrefDocumentation;
	}
}

