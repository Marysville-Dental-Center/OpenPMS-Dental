
namespace OpenPmsDental {
	partial class UserControlApptAppearance {
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.groupBoxOD1 = new OpenPmsDental.UI.GroupBox();
			this.labelApptSchedEnforceSpecialty = new System.Windows.Forms.Label();
			this.checkApptExclamation = new OpenPmsDental.UI.CheckBox();
			this.comboApptSchedEnforceSpecialty = new OpenPmsDental.UI.ComboBox();
			this.checkApptModuleDefaultToWeek = new OpenPmsDental.UI.CheckBox();
			this.checkApptSecondaryProviderConsiderOpOnly = new OpenPmsDental.UI.CheckBox();
			this.checkApptRefreshEveryMinute = new OpenPmsDental.UI.CheckBox();
			this.comboDelay = new OpenPmsDental.UI.ComboBox();
			this.apptClickDelay = new System.Windows.Forms.Label();
			this.label45 = new System.Windows.Forms.Label();
			this.comboWeekViewStartDay = new OpenPmsDental.UI.ComboBox();
			this.groupBoxBlockouts = new OpenPmsDental.UI.GroupBox();
			this.checkReplaceBlockouts = new OpenPmsDental.UI.CheckBox();
			this.checkSolidBlockouts = new OpenPmsDental.UI.CheckBox();
			this.groupBoxApptBubble = new OpenPmsDental.UI.GroupBox();
			this.textApptBubNoteLength = new System.Windows.Forms.TextBox();
			this.label21 = new System.Windows.Forms.Label();
			this.checkApptBubbleDelay = new OpenPmsDental.UI.CheckBox();
			this.checkAppointmentBubblesDisabled = new OpenPmsDental.UI.CheckBox();
			this.groupBoxAppearance = new OpenPmsDental.UI.GroupBox();
			this.label58 = new System.Windows.Forms.Label();
			this.label46 = new System.Windows.Forms.Label();
			this.textApptProvbarWidth = new OpenPmsDental.ValidNum();
			this.label54 = new System.Windows.Forms.Label();
			this.textApptFontSize = new System.Windows.Forms.TextBox();
			this.butApptLineColor = new System.Windows.Forms.Button();
			this.label25 = new System.Windows.Forms.Label();
			this.groupBoxWaitingRoom = new OpenPmsDental.UI.GroupBox();
			this.label3 = new System.Windows.Forms.Label();
			this.comboTimeArrived = new OpenPmsDental.UI.ComboBox();
			this.comboTimeSeated = new OpenPmsDental.UI.ComboBox();
			this.label5 = new System.Windows.Forms.Label();
			this.comboTimeDismissed = new OpenPmsDental.UI.ComboBox();
			this.label6 = new System.Windows.Forms.Label();
			this.textWaitRoomWarn = new System.Windows.Forms.TextBox();
			this.label22 = new System.Windows.Forms.Label();
			this.checkWaitingRoomFilterByView = new OpenPmsDental.UI.CheckBox();
			this.butColor = new System.Windows.Forms.Button();
			this.label23 = new System.Windows.Forms.Label();
			this.groupBoxTimeBars = new OpenPmsDental.UI.GroupBox();
			this.checkProvTimeOnOpBackground = new OpenPmsDental.UI.CheckBox();
			this.checkTimeBarSecondaryColor = new OpenPmsDental.UI.CheckBox();
			this.groupBoxTimeBars.SuspendLayout();
			this.groupBoxOD1.SuspendLayout();
			this.groupBoxBlockouts.SuspendLayout();
			this.groupBoxApptBubble.SuspendLayout();
			this.groupBoxAppearance.SuspendLayout();
			this.groupBoxWaitingRoom.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBoxOD1
			// 
			this.groupBoxOD1.Controls.Add(this.comboApptSchedEnforceSpecialty);
			this.groupBoxOD1.Controls.Add(this.comboDelay);
			this.groupBoxOD1.Controls.Add(this.comboWeekViewStartDay);
			this.groupBoxOD1.Controls.Add(this.labelApptSchedEnforceSpecialty);
			this.groupBoxOD1.Controls.Add(this.checkApptExclamation);
			this.groupBoxOD1.Controls.Add(this.checkApptModuleDefaultToWeek);
			this.groupBoxOD1.Controls.Add(this.checkApptSecondaryProviderConsiderOpOnly);
			this.groupBoxOD1.Controls.Add(this.checkApptRefreshEveryMinute);
			this.groupBoxOD1.Controls.Add(this.apptClickDelay);
			this.groupBoxOD1.Controls.Add(this.label45);
			this.groupBoxOD1.Location = new System.Drawing.Point(8, 541);
			this.groupBoxOD1.Name = "groupBoxOD1";
			this.groupBoxOD1.Size = new System.Drawing.Size(478, 204);
			this.groupBoxOD1.TabIndex = 330;
			this.groupBoxOD1.Text = "Functionality";
			// 
			// labelApptSchedEnforceSpecialty
			// 
			this.labelApptSchedEnforceSpecialty.Location = new System.Drawing.Point(24, 47);
			this.labelApptSchedEnforceSpecialty.Name = "labelApptSchedEnforceSpecialty";
			this.labelApptSchedEnforceSpecialty.Size = new System.Drawing.Size(218, 23);
			this.labelApptSchedEnforceSpecialty.TabIndex = 333;
			this.labelApptSchedEnforceSpecialty.Text = "Enforce clinic specialties";
			this.labelApptSchedEnforceSpecialty.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// checkApptExclamation
			// 
			this.checkApptExclamation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkApptExclamation.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkApptExclamation.Location = new System.Drawing.Point(8, 74);
			this.checkApptExclamation.Name = "checkApptExclamation";
			this.checkApptExclamation.Size = new System.Drawing.Size(456, 20);
			this.checkApptExclamation.TabIndex = 222;
			this.checkApptExclamation.Text = "Show ! on appts for ins not sent, if added to Appt View (might cause slowdown)";
			// 
			// comboApptSchedEnforceSpecialty
			// 
			this.comboApptSchedEnforceSpecialty.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.comboApptSchedEnforceSpecialty.Location = new System.Drawing.Point(248, 47);
			this.comboApptSchedEnforceSpecialty.Name = "comboApptSchedEnforceSpecialty";
			this.comboApptSchedEnforceSpecialty.Size = new System.Drawing.Size(222, 23);
			this.comboApptSchedEnforceSpecialty.TabIndex = 332;
			// 
			// checkApptModuleDefaultToWeek
			// 
			this.checkApptModuleDefaultToWeek.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkApptModuleDefaultToWeek.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkApptModuleDefaultToWeek.Location = new System.Drawing.Point(8, 98);
			this.checkApptModuleDefaultToWeek.Name = "checkApptModuleDefaultToWeek";
			this.checkApptModuleDefaultToWeek.Size = new System.Drawing.Size(266, 20);
			this.checkApptModuleDefaultToWeek.TabIndex = 220;
			this.checkApptModuleDefaultToWeek.Text = "Appointments Module defaults to week view";
			// 
			// checkApptSecondaryProviderConsiderOpOnly
			// 
			this.checkApptSecondaryProviderConsiderOpOnly.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkApptSecondaryProviderConsiderOpOnly.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkApptSecondaryProviderConsiderOpOnly.Location = new System.Drawing.Point(8, 23);
			this.checkApptSecondaryProviderConsiderOpOnly.Name = "checkApptSecondaryProviderConsiderOpOnly";
			this.checkApptSecondaryProviderConsiderOpOnly.Size = new System.Drawing.Size(297, 20);
			this.checkApptSecondaryProviderConsiderOpOnly.TabIndex = 331;
			this.checkApptSecondaryProviderConsiderOpOnly.Text = "Force op\'s hygiene provider as secondary provider";
			this.checkApptSecondaryProviderConsiderOpOnly.Click += new System.EventHandler(this.checkApptSecondaryProviderConsiderOpOnly_Click);
			// 
			// checkApptRefreshEveryMinute
			// 
			this.checkApptRefreshEveryMinute.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkApptRefreshEveryMinute.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkApptRefreshEveryMinute.Location = new System.Drawing.Point(8, 149);
			this.checkApptRefreshEveryMinute.Name = "checkApptRefreshEveryMinute";
			this.checkApptRefreshEveryMinute.Size = new System.Drawing.Size(160, 20);
			this.checkApptRefreshEveryMinute.TabIndex = 235;
			this.checkApptRefreshEveryMinute.Text = "Refresh every 60 seconds";
			// 
			// comboDelay
			// 
			this.comboDelay.AllowDrop = true;
			this.comboDelay.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.comboDelay.Location = new System.Drawing.Point(248, 122);
			this.comboDelay.Name = "comboDelay";
			this.comboDelay.Size = new System.Drawing.Size(222, 23);
			this.comboDelay.TabIndex = 232;
			// 
			// apptClickDelay
			// 
			this.apptClickDelay.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.apptClickDelay.Location = new System.Drawing.Point(24, 122);
			this.apptClickDelay.Name = "apptClickDelay";
			this.apptClickDelay.Size = new System.Drawing.Size(218, 23);
			this.apptClickDelay.TabIndex = 233;
			this.apptClickDelay.Text = "Click delay after opening Appt Edit";
			this.apptClickDelay.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label45
			// 
			this.label45.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label45.Location = new System.Drawing.Point(24, 173);
			this.label45.Name = "label45";
			this.label45.Size = new System.Drawing.Size(218, 23);
			this.label45.TabIndex = 290;
			this.label45.Text = "Week View start day";
			this.label45.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// comboWeekViewStartDay
			// 
			this.comboWeekViewStartDay.AllowDrop = true;
			this.comboWeekViewStartDay.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.comboWeekViewStartDay.Location = new System.Drawing.Point(248, 173);
			this.comboWeekViewStartDay.Name = "comboWeekViewStartDay";
			this.comboWeekViewStartDay.Size = new System.Drawing.Size(222, 23);
			this.comboWeekViewStartDay.TabIndex = 289;
			// 
			// groupBoxBlockouts
			// 
			this.groupBoxBlockouts.Controls.Add(this.checkReplaceBlockouts);
			this.groupBoxBlockouts.Controls.Add(this.checkSolidBlockouts);
			this.groupBoxBlockouts.Location = new System.Drawing.Point(8, 264);
			this.groupBoxBlockouts.Name = "groupBoxBlockouts";
			this.groupBoxBlockouts.Size = new System.Drawing.Size(478, 75);
			this.groupBoxBlockouts.TabIndex = 0;
			this.groupBoxBlockouts.Text = "Blockouts";
			// 
			// checkReplaceBlockouts
			// 
			this.checkReplaceBlockouts.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkReplaceBlockouts.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkReplaceBlockouts.Location = new System.Drawing.Point(8, 47);
			this.checkReplaceBlockouts.Name = "checkReplaceBlockouts";
			this.checkReplaceBlockouts.Size = new System.Drawing.Size(458, 20);
			this.checkReplaceBlockouts.TabIndex = 318;
			this.checkReplaceBlockouts.Text = "Allow \'Block appointment scheduling\' blockouts to replace conflicting blockouts";
			// 
			// checkSolidBlockouts
			// 
			this.checkSolidBlockouts.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkSolidBlockouts.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkSolidBlockouts.Location = new System.Drawing.Point(8, 23);
			this.checkSolidBlockouts.Name = "checkSolidBlockouts";
			this.checkSolidBlockouts.Size = new System.Drawing.Size(397, 20);
			this.checkSolidBlockouts.TabIndex = 220;
			this.checkSolidBlockouts.Text = "Use solid blockouts instead of outlines on the Appointments Module";
			// 
			// groupBoxApptBubble
			// 
			this.groupBoxApptBubble.Controls.Add(this.textApptBubNoteLength);
			this.groupBoxApptBubble.Controls.Add(this.label21);
			this.groupBoxApptBubble.Controls.Add(this.checkApptBubbleDelay);
			this.groupBoxApptBubble.Controls.Add(this.checkAppointmentBubblesDisabled);
			this.groupBoxApptBubble.Location = new System.Drawing.Point(8, 154);
			this.groupBoxApptBubble.Name = "groupBoxApptBubble";
			this.groupBoxApptBubble.Size = new System.Drawing.Size(478, 102);
			this.groupBoxApptBubble.TabIndex = 329;
			this.groupBoxApptBubble.Text = "Appointment Bubble";
			// 
			// textApptBubNoteLength
			// 
			this.textApptBubNoteLength.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.textApptBubNoteLength.AutoSize = false;
			this.textApptBubNoteLength.Location = new System.Drawing.Point(248, 23);
			this.textApptBubNoteLength.Name = "textApptBubNoteLength";
			this.textApptBubNoteLength.Size = new System.Drawing.Size(84, 23);
			this.textApptBubNoteLength.TabIndex = 304;
			// 
			// label21
			// 
			this.label21.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label21.Location = new System.Drawing.Point(24, 23);
			this.label21.Name = "label21";
			this.label21.Size = new System.Drawing.Size(218, 23);
			this.label21.TabIndex = 303;
			this.label21.Text = "Appointment bubble max note length";
			this.label21.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// checkApptBubbleDelay
			// 
			this.checkApptBubbleDelay.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkApptBubbleDelay.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkApptBubbleDelay.Location = new System.Drawing.Point(8, 74);
			this.checkApptBubbleDelay.Name = "checkApptBubbleDelay";
			this.checkApptBubbleDelay.Size = new System.Drawing.Size(208, 20);
			this.checkApptBubbleDelay.TabIndex = 221;
			this.checkApptBubbleDelay.Text = "Appointment bubble popup delay";
			// 
			// checkAppointmentBubblesDisabled
			// 
			this.checkAppointmentBubblesDisabled.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkAppointmentBubblesDisabled.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkAppointmentBubblesDisabled.Location = new System.Drawing.Point(8, 50);
			this.checkAppointmentBubblesDisabled.Name = "checkAppointmentBubblesDisabled";
			this.checkAppointmentBubblesDisabled.Size = new System.Drawing.Size(399, 20);
			this.checkAppointmentBubblesDisabled.TabIndex = 234;
			this.checkAppointmentBubblesDisabled.Text = "Default appointment bubble to \'disabled\' for new appointment views";
			// 
			// groupBoxAppearance
			// 
			this.groupBoxAppearance.Controls.Add(this.textApptProvbarWidth);
			this.groupBoxAppearance.Controls.Add(this.textApptFontSize);
			this.groupBoxAppearance.Controls.Add(this.butApptLineColor);
			this.groupBoxAppearance.Controls.Add(this.label58);
			this.groupBoxAppearance.Controls.Add(this.label46);
			this.groupBoxAppearance.Controls.Add(this.label54);
			this.groupBoxAppearance.Controls.Add(this.label25);
			this.groupBoxAppearance.Location = new System.Drawing.Point(8, 8);
			this.groupBoxAppearance.Name = "groupBoxAppearance";
			this.groupBoxAppearance.Size = new System.Drawing.Size(478, 138);
			this.groupBoxAppearance.TabIndex = 328;
			this.groupBoxAppearance.Text = "Appearance";
			// 
			// label58
			// 
			this.label58.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label58.Location = new System.Drawing.Point(24, 77);
			this.label58.Name = "label58";
			this.label58.Size = new System.Drawing.Size(218, 32);
			this.label58.TabIndex = 286;
			this.label58.Text = "Width of provider time bar on left of each appointment";
			this.label58.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label46
			// 
			this.label46.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label46.ForeColor = System.Drawing.Color.DarkRed;
			this.label46.Location = new System.Drawing.Point(198, 113);
			this.label46.Name = "label46";
			this.label46.Size = new System.Drawing.Size(242, 17);
			this.label46.TabIndex = 291;
			this.label46.Text = "Also, see Appt View Edit for more settings";
			this.label46.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textApptProvbarWidth
			// 
			this.textApptProvbarWidth.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.textApptProvbarWidth.AutoSize = false;
			this.textApptProvbarWidth.Location = new System.Drawing.Point(248, 81);
			this.textApptProvbarWidth.MaxVal = 20;
			this.textApptProvbarWidth.Name = "textApptProvbarWidth";
			this.textApptProvbarWidth.Size = new System.Drawing.Size(48, 23);
			this.textApptProvbarWidth.TabIndex = 288;
			// 
			// label54
			// 
			this.label54.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label54.Location = new System.Drawing.Point(24, 50);
			this.label54.Name = "label54";
			this.label54.Size = new System.Drawing.Size(218, 23);
			this.label54.TabIndex = 251;
			this.label54.Text = "Appointment font size";
			this.label54.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// textApptFontSize
			// 
			this.textApptFontSize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.textApptFontSize.AutoSize = false;
			this.textApptFontSize.Location = new System.Drawing.Point(248, 50);
			this.textApptFontSize.Name = "textApptFontSize";
			this.textApptFontSize.Size = new System.Drawing.Size(48, 23);
			this.textApptFontSize.TabIndex = 285;
			// 
			// butApptLineColor
			// 
			this.butApptLineColor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.butApptLineColor.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.butApptLineColor.Location = new System.Drawing.Point(248, 23);
			this.butApptLineColor.Name = "butApptLineColor";
			this.butApptLineColor.Size = new System.Drawing.Size(24, 23);
			this.butApptLineColor.TabIndex = 226;
			this.butApptLineColor.Click += new System.EventHandler(this.butApptLineColor_Click);
			// 
			// label25
			// 
			this.label25.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label25.Location = new System.Drawing.Point(24, 23);
			this.label25.Name = "label25";
			this.label25.Size = new System.Drawing.Size(218, 23);
			this.label25.TabIndex = 224;
			this.label25.Text = "Appointment time line color";
			this.label25.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// groupBoxWaitingRoom
			// 
			this.groupBoxWaitingRoom.Controls.Add(this.comboTimeArrived);
			this.groupBoxWaitingRoom.Controls.Add(this.comboTimeSeated);
			this.groupBoxWaitingRoom.Controls.Add(this.comboTimeDismissed);
			this.groupBoxWaitingRoom.Controls.Add(this.textWaitRoomWarn);
			this.groupBoxWaitingRoom.Controls.Add(this.butColor);
			this.groupBoxWaitingRoom.Controls.Add(this.label3);
			this.groupBoxWaitingRoom.Controls.Add(this.label5);
			this.groupBoxWaitingRoom.Controls.Add(this.label6);
			this.groupBoxWaitingRoom.Controls.Add(this.label22);
			this.groupBoxWaitingRoom.Controls.Add(this.checkWaitingRoomFilterByView);
			this.groupBoxWaitingRoom.Controls.Add(this.label23);
			this.groupBoxWaitingRoom.Location = new System.Drawing.Point(8, 347);
			this.groupBoxWaitingRoom.Name = "groupBoxWaitingRoom";
			this.groupBoxWaitingRoom.Size = new System.Drawing.Size(478, 186);
			this.groupBoxWaitingRoom.TabIndex = 327;
			this.groupBoxWaitingRoom.Text = "Waiting Room";
			// 
			// label3
			// 
			this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label3.Location = new System.Drawing.Point(24, 23);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(218, 23);
			this.label3.TabIndex = 292;
			this.label3.Text = "Time Arrived trigger";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// comboTimeArrived
			// 
			this.comboTimeArrived.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.comboTimeArrived.Location = new System.Drawing.Point(248, 23);
			this.comboTimeArrived.Name = "comboTimeArrived";
			this.comboTimeArrived.Size = new System.Drawing.Size(222, 23);
			this.comboTimeArrived.TabIndex = 291;
			// 
			// comboTimeSeated
			// 
			this.comboTimeSeated.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.comboTimeSeated.Location = new System.Drawing.Point(248, 50);
			this.comboTimeSeated.Name = "comboTimeSeated";
			this.comboTimeSeated.Size = new System.Drawing.Size(222, 23);
			this.comboTimeSeated.TabIndex = 293;
			// 
			// label5
			// 
			this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label5.Location = new System.Drawing.Point(24, 50);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(218, 23);
			this.label5.TabIndex = 294;
			this.label5.Text = "Time Seated (in op) trigger";
			this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// comboTimeDismissed
			// 
			this.comboTimeDismissed.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.comboTimeDismissed.Location = new System.Drawing.Point(248, 77);
			this.comboTimeDismissed.Name = "comboTimeDismissed";
			this.comboTimeDismissed.Size = new System.Drawing.Size(222, 23);
			this.comboTimeDismissed.TabIndex = 295;
			// 
			// label6
			// 
			this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label6.Location = new System.Drawing.Point(24, 77);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(218, 23);
			this.label6.TabIndex = 296;
			this.label6.Text = "Time Dismissed trigger";
			this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// textWaitRoomWarn
			// 
			this.textWaitRoomWarn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.textWaitRoomWarn.AutoSize = false;
			this.textWaitRoomWarn.Location = new System.Drawing.Point(248, 128);
			this.textWaitRoomWarn.Name = "textWaitRoomWarn";
			this.textWaitRoomWarn.Size = new System.Drawing.Size(84, 23);
			this.textWaitRoomWarn.TabIndex = 304;
			// 
			// label22
			// 
			this.label22.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label22.Location = new System.Drawing.Point(24, 128);
			this.label22.Name = "label22";
			this.label22.Size = new System.Drawing.Size(218, 23);
			this.label22.TabIndex = 303;
			this.label22.Text = "Waiting room alert time";
			this.label22.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// checkWaitingRoomFilterByView
			// 
			this.checkWaitingRoomFilterByView.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkWaitingRoomFilterByView.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkWaitingRoomFilterByView.Location = new System.Drawing.Point(8, 104);
			this.checkWaitingRoomFilterByView.Name = "checkWaitingRoomFilterByView";
			this.checkWaitingRoomFilterByView.Size = new System.Drawing.Size(371, 20);
			this.checkWaitingRoomFilterByView.TabIndex = 300;
			this.checkWaitingRoomFilterByView.Text = "Filter the waiting room based on the selected appointment view";
			// 
			// butColor
			// 
			this.butColor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.butColor.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.butColor.Location = new System.Drawing.Point(248, 155);
			this.butColor.Name = "butColor";
			this.butColor.Size = new System.Drawing.Size(24, 23);
			this.butColor.TabIndex = 225;
			this.butColor.Click += new System.EventHandler(this.butColor_Click);
			// 
			// label23
			// 
			this.label23.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label23.Location = new System.Drawing.Point(24, 155);
			this.label23.Name = "label23";
			this.label23.Size = new System.Drawing.Size(218, 23);
			this.label23.TabIndex = 223;
			this.label23.Text = "Waiting room alert color";
			this.label23.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			//
			// groupBoxTimeBars
			//
			this.groupBoxTimeBars.Controls.Add(this.checkProvTimeOnOpBackground);
			this.groupBoxTimeBars.Controls.Add(this.checkTimeBarSecondaryColor);
			this.groupBoxTimeBars.Location = new System.Drawing.Point(8, 753);
			this.groupBoxTimeBars.Name = "groupBoxTimeBars";
			this.groupBoxTimeBars.Size = new System.Drawing.Size(478, 75);
			this.groupBoxTimeBars.TabIndex = 340;
			this.groupBoxTimeBars.Text = "Time Bars";
			//
			// checkProvTimeOnOpBackground
			//
			this.checkProvTimeOnOpBackground.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkProvTimeOnOpBackground.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkProvTimeOnOpBackground.Location = new System.Drawing.Point(8, 23);
			this.checkProvTimeOnOpBackground.Name = "checkProvTimeOnOpBackground";
			this.checkProvTimeOnOpBackground.Size = new System.Drawing.Size(416, 20);
			this.checkProvTimeOnOpBackground.TabIndex = 341;
			this.checkProvTimeOnOpBackground.Text = "Show provider time on the operatory background in the provider\'s color";
			//
			// checkTimeBarSecondaryColor
			//
			this.checkTimeBarSecondaryColor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkTimeBarSecondaryColor.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkTimeBarSecondaryColor.Location = new System.Drawing.Point(8, 47);
			this.checkTimeBarSecondaryColor.Name = "checkTimeBarSecondaryColor";
			this.checkTimeBarSecondaryColor.Size = new System.Drawing.Size(413, 20);
			this.checkTimeBarSecondaryColor.TabIndex = 342;
			this.checkTimeBarSecondaryColor.Text = "No provider time set: show the secondary provider color in the time bar";
			//
			// UserControlApptAppearance
			//
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.Color.White;
			this.Controls.Add(this.groupBoxTimeBars);
			this.Controls.Add(this.groupBoxOD1);
			this.Controls.Add(this.groupBoxBlockouts);
			this.Controls.Add(this.groupBoxApptBubble);
			this.Controls.Add(this.groupBoxAppearance);
			this.Controls.Add(this.groupBoxWaitingRoom);
			this.Name = "UserControlApptAppearance";
			this.Size = new System.Drawing.Size(494, 836);
			this.groupBoxOD1.ResumeLayout(false);
			this.groupBoxBlockouts.ResumeLayout(false);
			this.groupBoxApptBubble.ResumeLayout(false);
			this.groupBoxApptBubble.PerformLayout();
			this.groupBoxAppearance.ResumeLayout(false);
			this.groupBoxAppearance.PerformLayout();
			this.groupBoxWaitingRoom.ResumeLayout(false);
			this.groupBoxWaitingRoom.PerformLayout();
			this.groupBoxTimeBars.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion
		private System.Windows.Forms.Label label46;
		private System.Windows.Forms.Label label45;
		private UI.ComboBox comboWeekViewStartDay;
		private OpenPmsDental.UI.CheckBox checkAppointmentBubblesDisabled;
		private ValidNum textApptProvbarWidth;
		private OpenPmsDental.UI.CheckBox checkSolidBlockouts;
		private System.Windows.Forms.Label label58;
		private OpenPmsDental.UI.CheckBox checkApptExclamation;
		private System.Windows.Forms.TextBox textApptFontSize;
		private System.Windows.Forms.Button butApptLineColor;
		private OpenPmsDental.UI.CheckBox checkApptBubbleDelay;
		private System.Windows.Forms.Button butColor;
		private System.Windows.Forms.Label label23;
		private UI.ComboBox comboDelay;
		private System.Windows.Forms.Label label25;
		private OpenPmsDental.UI.CheckBox checkApptModuleDefaultToWeek;
		private OpenPmsDental.UI.CheckBox checkApptRefreshEveryMinute;
		private System.Windows.Forms.Label apptClickDelay;
		private System.Windows.Forms.Label label54;
		private UI.GroupBox groupBoxWaitingRoom;
		private System.Windows.Forms.Label label3;
		private UI.ComboBox comboTimeArrived;
		private UI.ComboBox comboTimeSeated;
		private System.Windows.Forms.Label label5;
		private UI.ComboBox comboTimeDismissed;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.TextBox textWaitRoomWarn;
		private System.Windows.Forms.Label label22;
		private OpenPmsDental.UI.CheckBox checkWaitingRoomFilterByView;
		private UI.GroupBox groupBoxAppearance;
		private UI.GroupBox groupBoxApptBubble;
		private UI.GroupBox groupBoxBlockouts;
		private System.Windows.Forms.TextBox textApptBubNoteLength;
		private System.Windows.Forms.Label label21;
		private OpenPmsDental.UI.CheckBox checkReplaceBlockouts;
		private UI.GroupBox groupBoxOD1;
		private System.Windows.Forms.Label labelApptSchedEnforceSpecialty;
		private UI.ComboBox comboApptSchedEnforceSpecialty;
		private OpenPmsDental.UI.CheckBox checkApptSecondaryProviderConsiderOpOnly;
		private UI.GroupBox groupBoxTimeBars;
		private OpenPmsDental.UI.CheckBox checkProvTimeOnOpBackground;
		private OpenPmsDental.UI.CheckBox checkTimeBarSecondaryColor;
	}
}
