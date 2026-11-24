
namespace OpenPmsDental {
	partial class UserControlApptGeneral {
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
			this.groupBoxProduction = new OpenPmsDental.UI.GroupBox();
			this.checkApptModuleAdjInProd = new OpenPmsDental.UI.CheckBox();
			this.checkApptModuleProductionUsesOps = new OpenPmsDental.UI.CheckBox();
			this.groupBoxOD2 = new OpenPmsDental.UI.GroupBox();
			this.labelApptProvChangePrompt = new System.Windows.Forms.Label();
			this.comboApptProvChangePrompt = new OpenPmsDental.UI.ComboBox();
			this.textApptWithoutProcsDefaultLength = new OpenPmsDental.ValidNum();
			this.checkAppointmentTimeIsLocked = new OpenPmsDental.UI.CheckBox();
			this.checkApptsRequireProc = new OpenPmsDental.UI.CheckBox();
			this.checkApptAllowFutureComplete = new OpenPmsDental.UI.CheckBox();
			this.checkUnscheduledListNoRecalls = new OpenPmsDental.UI.CheckBox();
			this.checkApptsAllowOverlap = new OpenPmsDental.UI.CheckBox();
			this.checkApptMirrorSecondary = new OpenPmsDental.UI.CheckBox();
			this.labelApptWithoutProcsDefaultLength = new System.Windows.Forms.Label();
			this.checkPreventChangesToComplAppts = new OpenPmsDental.UI.CheckBox();
			this.checkApptAllowEmptyComplete = new OpenPmsDental.UI.CheckBox();
			this.groupBoxCalendarBehavior = new OpenPmsDental.UI.GroupBox();
			this.textApptAutoRefreshRange = new OpenPmsDental.ValidNum();
			this.comboSearchBehavior = new OpenPmsDental.UI.ComboBox();
			this.labelApptAutoRefreshRange = new System.Windows.Forms.Label();
			this.label13 = new System.Windows.Forms.Label();
			this.checkApptTimeReset = new OpenPmsDental.UI.CheckBox();
			this.groupBox2 = new OpenPmsDental.UI.GroupBox();
			this.comboBrokenApptProc = new OpenPmsDental.UI.ComboBox();
			this.comboBrokenApptAdjType = new OpenPmsDental.UI.ComboBox();
			this.label37 = new System.Windows.Forms.Label();
			this.checkBrokenApptCommLog = new OpenPmsDental.UI.CheckBox();
			this.checkBrokenApptRequiredOnMove = new OpenPmsDental.UI.CheckBox();
			this.checkBrokenApptAdjustment = new OpenPmsDental.UI.CheckBox();
			this.label7 = new System.Windows.Forms.Label();
			this.groupBoxScheduling = new OpenPmsDental.UI.GroupBox();
			this.checkAppointmentMakeRecallSearchesOpenings = new OpenPmsDental.UI.CheckBox();
			this.labelScheduleProvUnassigned = new System.Windows.Forms.Label();
			this.comboScheduleProvUnassigned = new OpenPmsDental.UI.ComboBox();
			this.groupBoxScheduling.SuspendLayout();
			this.groupBoxProduction.SuspendLayout();
			this.groupBoxOD2.SuspendLayout();
			this.groupBoxCalendarBehavior.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBoxProduction
			// 
			this.groupBoxProduction.Controls.Add(this.checkApptModuleAdjInProd);
			this.groupBoxProduction.Controls.Add(this.checkApptModuleProductionUsesOps);
			this.groupBoxProduction.Location = new System.Drawing.Point(8, 581);
			this.groupBoxProduction.Name = "groupBoxProduction";
			this.groupBoxProduction.Size = new System.Drawing.Size(478, 75);
			this.groupBoxProduction.TabIndex = 327;
			this.groupBoxProduction.Text = "Production";
			// 
			// checkApptModuleAdjInProd
			// 
			this.checkApptModuleAdjInProd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkApptModuleAdjInProd.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkApptModuleAdjInProd.Location = new System.Drawing.Point(8, 23);
			this.checkApptModuleAdjInProd.Name = "checkApptModuleAdjInProd";
			this.checkApptModuleAdjInProd.Size = new System.Drawing.Size(243, 20);
			this.checkApptModuleAdjInProd.TabIndex = 307;
			this.checkApptModuleAdjInProd.Text = "Add daily adjustments to net production";
			// 
			// checkApptModuleProductionUsesOps
			// 
			this.checkApptModuleProductionUsesOps.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkApptModuleProductionUsesOps.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkApptModuleProductionUsesOps.Location = new System.Drawing.Point(8, 47);
			this.checkApptModuleProductionUsesOps.Name = "checkApptModuleProductionUsesOps";
			this.checkApptModuleProductionUsesOps.Size = new System.Drawing.Size(302, 20);
			this.checkApptModuleProductionUsesOps.TabIndex = 309;
			this.checkApptModuleProductionUsesOps.Text = "Appointments Module production uses operatories";
			// 
			// groupBoxOD2
			// 
			this.groupBoxOD2.Controls.Add(this.labelApptProvChangePrompt);
			this.groupBoxOD2.Controls.Add(this.comboApptProvChangePrompt);
			this.groupBoxOD2.Controls.Add(this.textApptWithoutProcsDefaultLength);
			this.groupBoxOD2.Controls.Add(this.checkAppointmentTimeIsLocked);
			this.groupBoxOD2.Controls.Add(this.checkApptsRequireProc);
			this.groupBoxOD2.Controls.Add(this.checkApptAllowFutureComplete);
			this.groupBoxOD2.Controls.Add(this.checkUnscheduledListNoRecalls);
			this.groupBoxOD2.Controls.Add(this.checkApptsAllowOverlap);
			this.groupBoxOD2.Controls.Add(this.labelApptWithoutProcsDefaultLength);
			this.groupBoxOD2.Controls.Add(this.checkPreventChangesToComplAppts);
			this.groupBoxOD2.Controls.Add(this.checkApptAllowEmptyComplete);
			this.groupBoxOD2.Controls.Add(this.checkApptMirrorSecondary);
			this.groupBoxOD2.Location = new System.Drawing.Point(8, 291);
			this.groupBoxOD2.Name = "groupBoxOD2";
			this.groupBoxOD2.Size = new System.Drawing.Size(478, 282);
			this.groupBoxOD2.TabIndex = 325;
			this.groupBoxOD2.Text = "Appointment Behavior";
			// 
			// labelApptProvChangePrompt
			// 
			this.labelApptProvChangePrompt.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.labelApptProvChangePrompt.Location = new System.Drawing.Point(24, 227);
			this.labelApptProvChangePrompt.Name = "labelApptProvChangePrompt";
			this.labelApptProvChangePrompt.Size = new System.Drawing.Size(218, 23);
			this.labelApptProvChangePrompt.TabIndex = 329;
			this.labelApptProvChangePrompt.Text = "Appointment provider change behavior";
			this.labelApptProvChangePrompt.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// comboApptProvChangePrompt
			// 
			this.comboApptProvChangePrompt.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.comboApptProvChangePrompt.Location = new System.Drawing.Point(248, 227);
			this.comboApptProvChangePrompt.Name = "comboApptProvChangePrompt";
			this.comboApptProvChangePrompt.Size = new System.Drawing.Size(222, 23);
			this.comboApptProvChangePrompt.TabIndex = 328;
			// 
			// textApptWithoutProcsDefaultLength
			// 
			this.textApptWithoutProcsDefaultLength.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.textApptWithoutProcsDefaultLength.Location = new System.Drawing.Point(248, 99);
			this.textApptWithoutProcsDefaultLength.MaxVal = 600;
			this.textApptWithoutProcsDefaultLength.MinVal = 5;
			this.textApptWithoutProcsDefaultLength.Name = "textApptWithoutProcsDefaultLength";
			this.textApptWithoutProcsDefaultLength.ShowZero = false;
			this.textApptWithoutProcsDefaultLength.Size = new System.Drawing.Size(40, 20);
			this.textApptWithoutProcsDefaultLength.TabIndex = 314;
			// 
			// checkAppointmentTimeIsLocked
			// 
			this.checkAppointmentTimeIsLocked.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkAppointmentTimeIsLocked.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkAppointmentTimeIsLocked.Location = new System.Drawing.Point(8, 23);
			this.checkAppointmentTimeIsLocked.Name = "checkAppointmentTimeIsLocked";
			this.checkAppointmentTimeIsLocked.Size = new System.Drawing.Size(220, 20);
			this.checkAppointmentTimeIsLocked.TabIndex = 297;
			this.checkAppointmentTimeIsLocked.Text = "Appointment time locked by default";
			this.checkAppointmentTimeIsLocked.MouseUp += new System.Windows.Forms.MouseEventHandler(this.checkAppointmentTimeIsLocked_MouseUp);
			// 
			// checkApptsRequireProc
			// 
			this.checkApptsRequireProc.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkApptsRequireProc.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkApptsRequireProc.Location = new System.Drawing.Point(8, 47);
			this.checkApptsRequireProc.Name = "checkApptsRequireProc";
			this.checkApptsRequireProc.Size = new System.Drawing.Size(207, 20);
			this.checkApptsRequireProc.TabIndex = 310;
			this.checkApptsRequireProc.Text = "Appointments require procedures";
			this.checkApptsRequireProc.CheckedChanged += new System.EventHandler(this.checkApptsRequireProcs_CheckedChanged);
			this.checkApptsRequireProc.Click += new System.EventHandler(this.checkApptsRequireProc_Click);
			// 
			// checkApptAllowFutureComplete
			// 
			this.checkApptAllowFutureComplete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkApptAllowFutureComplete.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkApptAllowFutureComplete.Location = new System.Drawing.Point(8, 71);
			this.checkApptAllowFutureComplete.Name = "checkApptAllowFutureComplete";
			this.checkApptAllowFutureComplete.Size = new System.Drawing.Size(263, 20);
			this.checkApptAllowFutureComplete.TabIndex = 311;
			this.checkApptAllowFutureComplete.Text = "Allow setting future appointments complete";
			// 
			// checkUnscheduledListNoRecalls
			// 
			this.checkUnscheduledListNoRecalls.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkUnscheduledListNoRecalls.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkUnscheduledListNoRecalls.Location = new System.Drawing.Point(8, 203);
			this.checkUnscheduledListNoRecalls.Name = "checkUnscheduledListNoRecalls";
			this.checkUnscheduledListNoRecalls.Size = new System.Drawing.Size(338, 20);
			this.checkUnscheduledListNoRecalls.TabIndex = 318;
			this.checkUnscheduledListNoRecalls.Text = "Do not allow recall appointments on the Unscheduled List";
			// 
			// checkApptsAllowOverlap
			// 
			this.checkApptsAllowOverlap.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkApptsAllowOverlap.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkApptsAllowOverlap.Location = new System.Drawing.Point(8, 179);
			this.checkApptsAllowOverlap.Name = "checkApptsAllowOverlap";
			this.checkApptsAllowOverlap.Size = new System.Drawing.Size(176, 20);
			this.checkApptsAllowOverlap.TabIndex = 322;
			this.checkApptsAllowOverlap.Text = "Appointments allow overlap";
			//
			// checkApptMirrorSecondary
			//
			this.checkApptMirrorSecondary.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkApptMirrorSecondary.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkApptMirrorSecondary.Location = new System.Drawing.Point(8, 254);
			this.checkApptMirrorSecondary.Name = "checkApptMirrorSecondary";
			this.checkApptMirrorSecondary.Size = new System.Drawing.Size(382, 20);
			this.checkApptMirrorSecondary.TabIndex = 330;
			this.checkApptMirrorSecondary.Text = "New appointments mirror into the secondary provider's operatory";
			// 
			// labelApptWithoutProcsDefaultLength
			// 
			this.labelApptWithoutProcsDefaultLength.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.labelApptWithoutProcsDefaultLength.Location = new System.Drawing.Point(24, 95);
			this.labelApptWithoutProcsDefaultLength.Name = "labelApptWithoutProcsDefaultLength";
			this.labelApptWithoutProcsDefaultLength.Size = new System.Drawing.Size(218, 32);
			this.labelApptWithoutProcsDefaultLength.TabIndex = 313;
			this.labelApptWithoutProcsDefaultLength.Text = "Appointment without procedures default length in minutes";
			this.labelApptWithoutProcsDefaultLength.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// checkPreventChangesToComplAppts
			// 
			this.checkPreventChangesToComplAppts.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkPreventChangesToComplAppts.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkPreventChangesToComplAppts.Location = new System.Drawing.Point(8, 155);
			this.checkPreventChangesToComplAppts.Name = "checkPreventChangesToComplAppts";
			this.checkPreventChangesToComplAppts.Size = new System.Drawing.Size(420, 20);
			this.checkPreventChangesToComplAppts.TabIndex = 321;
			this.checkPreventChangesToComplAppts.Text = "Prevent changes to completed appointments with completed procedures";
			// 
			// checkApptAllowEmptyComplete
			// 
			this.checkApptAllowEmptyComplete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkApptAllowEmptyComplete.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkApptAllowEmptyComplete.Location = new System.Drawing.Point(8, 131);
			this.checkApptAllowEmptyComplete.Name = "checkApptAllowEmptyComplete";
			this.checkApptAllowEmptyComplete.Size = new System.Drawing.Size(336, 20);
			this.checkApptAllowEmptyComplete.TabIndex = 312;
			this.checkApptAllowEmptyComplete.Text = "Allow setting appointments without procedures complete";
			// 
			// groupBoxCalendarBehavior
			// 
			this.groupBoxCalendarBehavior.Controls.Add(this.textApptAutoRefreshRange);
			this.groupBoxCalendarBehavior.Controls.Add(this.comboSearchBehavior);
			this.groupBoxCalendarBehavior.Controls.Add(this.labelApptAutoRefreshRange);
			this.groupBoxCalendarBehavior.Controls.Add(this.label13);
			this.groupBoxCalendarBehavior.Controls.Add(this.checkApptTimeReset);
			this.groupBoxCalendarBehavior.Location = new System.Drawing.Point(8, 169);
			this.groupBoxCalendarBehavior.Name = "groupBoxCalendarBehavior";
			this.groupBoxCalendarBehavior.Size = new System.Drawing.Size(478, 114);
			this.groupBoxCalendarBehavior.TabIndex = 324;
			this.groupBoxCalendarBehavior.Text = "Calendar Behavior";
			// 
			// textApptAutoRefreshRange
			// 
			this.textApptAutoRefreshRange.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.textApptAutoRefreshRange.Location = new System.Drawing.Point(248, 54);
			this.textApptAutoRefreshRange.MaxVal = 600;
			this.textApptAutoRefreshRange.MinVal = -1;
			this.textApptAutoRefreshRange.Name = "textApptAutoRefreshRange";
			this.textApptAutoRefreshRange.ShowZero = false;
			this.textApptAutoRefreshRange.Size = new System.Drawing.Size(72, 23);
			this.textApptAutoRefreshRange.TabIndex = 320;
			// 
			// comboSearchBehavior
			// 
			this.comboSearchBehavior.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.comboSearchBehavior.Location = new System.Drawing.Point(248, 23);
			this.comboSearchBehavior.Name = "comboSearchBehavior";
			this.comboSearchBehavior.Size = new System.Drawing.Size(222, 23);
			this.comboSearchBehavior.TabIndex = 298;
			// 
			// labelApptAutoRefreshRange
			// 
			this.labelApptAutoRefreshRange.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.labelApptAutoRefreshRange.Location = new System.Drawing.Point(24, 50);
			this.labelApptAutoRefreshRange.Name = "labelApptAutoRefreshRange";
			this.labelApptAutoRefreshRange.Size = new System.Drawing.Size(218, 32);
			this.labelApptAutoRefreshRange.TabIndex = 319;
			this.labelApptAutoRefreshRange.Text = "Number of days out to automatically refresh Appointments Module";
			this.labelApptAutoRefreshRange.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label13
			// 
			this.label13.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label13.Location = new System.Drawing.Point(24, 23);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(218, 23);
			this.label13.TabIndex = 299;
			this.label13.Text = "Search Behavior";
			this.label13.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// checkApptTimeReset
			// 
			this.checkApptTimeReset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkApptTimeReset.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkApptTimeReset.Location = new System.Drawing.Point(8, 86);
			this.checkApptTimeReset.Name = "checkApptTimeReset";
			this.checkApptTimeReset.Size = new System.Drawing.Size(236, 20);
			this.checkApptTimeReset.TabIndex = 306;
			this.checkApptTimeReset.Text = "Reset calendar to today on Clinic select";
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.comboBrokenApptProc);
			this.groupBox2.Controls.Add(this.comboBrokenApptAdjType);
			this.groupBox2.Controls.Add(this.label37);
			this.groupBox2.Controls.Add(this.checkBrokenApptCommLog);
			this.groupBox2.Controls.Add(this.checkBrokenApptRequiredOnMove);
			this.groupBox2.Controls.Add(this.checkBrokenApptAdjustment);
			this.groupBox2.Controls.Add(this.label7);
			this.groupBox2.Location = new System.Drawing.Point(8, 8);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(478, 153);
			this.groupBox2.TabIndex = 305;
			this.groupBox2.Text = "Broken Appointment Automation";
			// 
			// comboBrokenApptProc
			// 
			this.comboBrokenApptProc.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.comboBrokenApptProc.Location = new System.Drawing.Point(248, 23);
			this.comboBrokenApptProc.Name = "comboBrokenApptProc";
			this.comboBrokenApptProc.Size = new System.Drawing.Size(222, 23);
			this.comboBrokenApptProc.TabIndex = 234;
			// 
			// comboBrokenApptAdjType
			// 
			this.comboBrokenApptAdjType.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.comboBrokenApptAdjType.Location = new System.Drawing.Point(248, 98);
			this.comboBrokenApptAdjType.Name = "comboBrokenApptAdjType";
			this.comboBrokenApptAdjType.Size = new System.Drawing.Size(222, 23);
			this.comboBrokenApptAdjType.TabIndex = 70;
			// 
			// label37
			// 
			this.label37.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label37.Location = new System.Drawing.Point(24, 23);
			this.label37.Name = "label37";
			this.label37.Size = new System.Drawing.Size(218, 23);
			this.label37.TabIndex = 235;
			this.label37.Text = "Broken appointment procedure type";
			this.label37.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// checkBrokenApptCommLog
			// 
			this.checkBrokenApptCommLog.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkBrokenApptCommLog.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkBrokenApptCommLog.Location = new System.Drawing.Point(8, 50);
			this.checkBrokenApptCommLog.Name = "checkBrokenApptCommLog";
			this.checkBrokenApptCommLog.Size = new System.Drawing.Size(223, 20);
			this.checkBrokenApptCommLog.TabIndex = 61;
			this.checkBrokenApptCommLog.Text = "Make broken appointment commlog";
			// 
			// checkBrokenApptRequiredOnMove
			// 
			this.checkBrokenApptRequiredOnMove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkBrokenApptRequiredOnMove.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkBrokenApptRequiredOnMove.Location = new System.Drawing.Point(8, 125);
			this.checkBrokenApptRequiredOnMove.Name = "checkBrokenApptRequiredOnMove";
			this.checkBrokenApptRequiredOnMove.Size = new System.Drawing.Size(383, 20);
			this.checkBrokenApptRequiredOnMove.TabIndex = 323;
			this.checkBrokenApptRequiredOnMove.Text = "Force users to break scheduled appointments before rescheduling";
			// 
			// checkBrokenApptAdjustment
			// 
			this.checkBrokenApptAdjustment.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkBrokenApptAdjustment.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkBrokenApptAdjustment.Location = new System.Drawing.Point(8, 74);
			this.checkBrokenApptAdjustment.Name = "checkBrokenApptAdjustment";
			this.checkBrokenApptAdjustment.Size = new System.Drawing.Size(233, 20);
			this.checkBrokenApptAdjustment.TabIndex = 217;
			this.checkBrokenApptAdjustment.Text = "Make broken appointment adjustment";
			// 
			// label7
			// 
			this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label7.Location = new System.Drawing.Point(24, 98);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(218, 23);
			this.label7.TabIndex = 71;
			this.label7.Text = "Broken appt default adj type";
			this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			//
			// groupBoxScheduling
			//
			this.groupBoxScheduling.Controls.Add(this.checkAppointmentMakeRecallSearchesOpenings);
			this.groupBoxScheduling.Controls.Add(this.labelScheduleProvUnassigned);
			this.groupBoxScheduling.Controls.Add(this.comboScheduleProvUnassigned);
			this.groupBoxScheduling.Location = new System.Drawing.Point(8, 664);
			this.groupBoxScheduling.Name = "groupBoxScheduling";
			this.groupBoxScheduling.Size = new System.Drawing.Size(478, 87);
			this.groupBoxScheduling.TabIndex = 331;
			this.groupBoxScheduling.Text = "Recall and Schedules";
			//
			// checkAppointmentMakeRecallSearchesOpenings
			//
			this.checkAppointmentMakeRecallSearchesOpenings.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkAppointmentMakeRecallSearchesOpenings.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkAppointmentMakeRecallSearchesOpenings.Location = new System.Drawing.Point(8, 23);
			this.checkAppointmentMakeRecallSearchesOpenings.Name = "checkAppointmentMakeRecallSearchesOpenings";
			this.checkAppointmentMakeRecallSearchesOpenings.Size = new System.Drawing.Size(287, 20);
			this.checkAppointmentMakeRecallSearchesOpenings.TabIndex = 332;
			this.checkAppointmentMakeRecallSearchesOpenings.Text = "Make Recall automatically searches for openings";
			//
			// labelScheduleProvUnassigned
			//
			this.labelScheduleProvUnassigned.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.labelScheduleProvUnassigned.Location = new System.Drawing.Point(24, 47);
			this.labelScheduleProvUnassigned.Name = "labelScheduleProvUnassigned";
			this.labelScheduleProvUnassigned.Size = new System.Drawing.Size(218, 32);
			this.labelScheduleProvUnassigned.TabIndex = 334;
			this.labelScheduleProvUnassigned.Text = "Default provider for unassigned operatories";
			this.labelScheduleProvUnassigned.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			//
			// comboScheduleProvUnassigned
			//
			this.comboScheduleProvUnassigned.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.comboScheduleProvUnassigned.Location = new System.Drawing.Point(248, 51);
			this.comboScheduleProvUnassigned.Name = "comboScheduleProvUnassigned";
			this.comboScheduleProvUnassigned.Size = new System.Drawing.Size(222, 23);
			this.comboScheduleProvUnassigned.TabIndex = 333;
			//
			// UserControlApptGeneral
			//
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.groupBoxScheduling);
			this.Controls.Add(this.groupBoxProduction);
			this.Controls.Add(this.groupBoxOD2);
			this.Controls.Add(this.groupBoxCalendarBehavior);
			this.Controls.Add(this.groupBox2);
			this.Name = "UserControlApptGeneral";
			this.Size = new System.Drawing.Size(494, 759);
			this.groupBoxScheduling.ResumeLayout(false);
			this.groupBoxProduction.ResumeLayout(false);
			this.groupBoxOD2.ResumeLayout(false);
			this.groupBoxOD2.PerformLayout();
			this.groupBoxCalendarBehavior.ResumeLayout(false);
			this.groupBoxCalendarBehavior.PerformLayout();
			this.groupBox2.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private OpenPmsDental.UI.CheckBox checkBrokenApptRequiredOnMove;
		private OpenPmsDental.UI.CheckBox checkApptsAllowOverlap;
		private OpenPmsDental.UI.CheckBox checkApptMirrorSecondary;
		private OpenPmsDental.UI.CheckBox checkPreventChangesToComplAppts;
		private ValidNum textApptAutoRefreshRange;
		private System.Windows.Forms.Label labelApptAutoRefreshRange;
		private OpenPmsDental.UI.CheckBox checkUnscheduledListNoRecalls;
		private ValidNum textApptWithoutProcsDefaultLength;
		private System.Windows.Forms.Label labelApptWithoutProcsDefaultLength;
		private OpenPmsDental.UI.CheckBox checkApptAllowEmptyComplete;
		private OpenPmsDental.UI.CheckBox checkApptAllowFutureComplete;
		private OpenPmsDental.UI.CheckBox checkApptsRequireProc;
		private OpenPmsDental.UI.CheckBox checkApptModuleProductionUsesOps;
		private OpenPmsDental.UI.CheckBox checkApptModuleAdjInProd;
		private OpenPmsDental.UI.CheckBox checkApptTimeReset;
		private UI.GroupBox groupBox2;
		private System.Windows.Forms.Label label37;
		private UI.ComboBox comboBrokenApptProc;
		private UI.CheckBox checkBrokenApptCommLog;
		private UI.CheckBox checkBrokenApptAdjustment;
		private UI.ComboBox comboBrokenApptAdjType;
		private System.Windows.Forms.Label label7;
		private OpenPmsDental.UI.CheckBox checkAppointmentTimeIsLocked;
		private UI.ComboBox comboSearchBehavior;
		private System.Windows.Forms.Label label13;
		private UI.GroupBox groupBoxCalendarBehavior;
		private UI.GroupBox groupBoxOD2;
		private UI.GroupBox groupBoxProduction;
		private System.Windows.Forms.Label labelApptProvChangePrompt;
		private UI.ComboBox comboApptProvChangePrompt;
		private UI.GroupBox groupBoxScheduling;
		private OpenPmsDental.UI.CheckBox checkAppointmentMakeRecallSearchesOpenings;
		private System.Windows.Forms.Label labelScheduleProvUnassigned;
		private UI.ComboBox comboScheduleProvUnassigned;
	}
}
