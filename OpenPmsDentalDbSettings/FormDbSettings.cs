// The window of the connection settings tool.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Drawing;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;
using CodeBase;
using DataConnectionBase;

namespace OpenPmsDentalDbSettings {
	///<summary>Server, database, user, the office CA certificate, and the client certificate or the database password; Test connection, Save,
	///Cancel (docs/specs/db-connection-security.md section 4.1). Filled from FreeDentalConfig.xml and then from the switches the tool was started
	///with (the installer passes its page's values).</summary>
	public class FormDbSettings:Form {
		public const string TITLE="Open PMS - Dental connection settings";
		private readonly string _configPath;
		private readonly string _caCopyPath;
		private readonly DbSettingsRequest _dbSettingsRequestStart;
		private DbConnectionConfig _dbConnectionConfigCurrent=new DbConnectionConfig();
		private string _pfxPathPending;
		private string _pfxPasswordPending;
		private TextBox _textServer;
		private TextBox _textDatabase;
		private TextBox _textUser;
		private TextBox _textCaFile;
		private Label _labelCaCurrent;
		private RadioButton _radioCertificate;
		private RadioButton _radioPassword;
		private Label _labelCertificateCurrent;
		private Label _labelPfxPending;
		private Button _butImportPfx;
		private TextBox _textPassword;
		private Label _labelStatus;

		///<summary>Set when the settings were saved; null when the window closed without saving.</summary>
		public DbSettingsResult Result { get; private set; }

		public FormDbSettings(string configPath,string caCopyPath,DbSettingsRequest dbSettingsRequestStart) {
			_configPath=configPath;
			_caCopyPath=caCopyPath;
			_dbSettingsRequestStart=dbSettingsRequestStart??new DbSettingsRequest();
			BuildControls();
			UiPlainWindow.Attach(this);//UI-PLAIN: the look of the program's windows (docs/port-notes/UI-PLAIN.md)
			Load+=FormDbSettings_Load;
			Shown+=FormDbSettings_Shown;
		}

		private void BuildControls() {
			Text=TITLE;
			FormBorderStyle=FormBorderStyle.FixedDialog;
			MaximizeBox=false;
			StartPosition=FormStartPosition.CenterScreen;
			AutoScaleDimensions=new SizeF(96F,96F);
			AutoScaleMode=AutoScaleMode.Dpi;
			ClientSize=new Size(560,470);
			try {
				Icon=Icon.ExtractAssociatedIcon(Application.ExecutablePath);
			}
			catch(Exception ex) {
				ex.DoNothing();
			}
			//UI-12 slice 14: on the layout grid (UiGrid): label + field rows one pitch apart from the padding, the labels right-aligned a label gap
			//before one field column, the CA's current file under its row, the credential group's choices with what each choice sets under its
			//text (the stock radio button's 22 px text lead), the notes, the bar on the bottom padding a section gap under the status.
			_textServer=AddRow("&Server:",UiGrid.PAD);
			_textDatabase=AddRow("&Database:",UiGrid.PAD+UiGrid.PITCH);
			_textUser=AddRow("&User:",UiGrid.PAD+2*UiGrid.PITCH);
			_textServer.Leave+=(sender,e) => ShowRootWarning();
			_textUser.Leave+=(sender,e) => ShowRootWarning();
			int yCa=UiGrid.PAD+3*UiGrid.PITCH;
			Controls.Add(new Label() { Text="Office &CA certificate:",Location=new Point(UiGrid.PAD,yCa),Size=new Size(WIDTH_LABEL,UiGrid.FIELD_HEIGHT),
				TextAlign=ContentAlignment.MiddleRight });
			Button butBrowseCa=new Button() { Text="Browse...",Location=new Point(ClientSize.Width-UiGrid.PAD-76,yCa),Size=new Size(76,UiGrid.FIELD_HEIGHT) };
			_textCaFile=new TextBox() { Location=new Point(X_FIELD,yCa),Size=new Size(butBrowseCa.Left-UiGrid.ITEM_GAP-X_FIELD,UiGrid.FIELD_HEIGHT) };
			Controls.Add(_textCaFile);
			butBrowseCa.Click+=butBrowseCa_Click;
			Controls.Add(butBrowseCa);
			_labelCaCurrent=new Label() { Location=new Point(X_FIELD,_textCaFile.Bottom+UiGrid.ROW_GAP),Size=new Size(ClientSize.Width-UiGrid.PAD-X_FIELD,34),
				ForeColor=SystemColors.GrayText };
			Controls.Add(_labelCaCurrent);
			int widthContent=ClientSize.Width-2*UiGrid.PAD;
			GroupBox groupCredential=new GroupBox() { Text="Client credential",Location=new Point(UiGrid.PAD,_labelCaCurrent.Bottom+UiGrid.SIBLING_GAP),
				Size=new Size(widthContent,100) };
			int widthGroupContent=widthContent-2*UiGrid.PAD;
			int xUnder=UiGrid.PAD+22;//under a choice's text
			int yGroup=18+UiGrid.TITLE_GAP;//under the title band
			_radioCertificate=new RadioButton() { Text="Client &certificate from the computer's certificate store (recommended)",Location=new Point(UiGrid.PAD,yGroup),
				Size=new Size(widthGroupContent,UiGrid.CHECK_HEIGHT) };
			_labelCertificateCurrent=new Label() { Location=new Point(xUnder,_radioCertificate.Bottom+UiGrid.ROW_GAP),Size=new Size(widthGroupContent-22,18),
				ForeColor=SystemColors.GrayText };
			_butImportPfx=new Button() { Text="&Import .pfx...",Location=new Point(xUnder,_labelCertificateCurrent.Bottom+UiGrid.ROW_GAP),
				Size=new Size(110,UiGrid.FIELD_HEIGHT) };
			_butImportPfx.Click+=butImportPfx_Click;
			_labelPfxPending=new Label() { Location=new Point(_butImportPfx.Right+UiGrid.ITEM_GAP,_butImportPfx.Top),
				Size=new Size(UiGrid.PAD+widthGroupContent-_butImportPfx.Right-UiGrid.ITEM_GAP,UiGrid.FIELD_HEIGHT),TextAlign=ContentAlignment.MiddleLeft };
			_radioPassword=new RadioButton() { Text="Database &password, stored on this computer (DPAPI, machine scope)",
				Location=new Point(UiGrid.PAD,_butImportPfx.Bottom+UiGrid.SECTION_GAP),Size=new Size(widthGroupContent,UiGrid.CHECK_HEIGHT) };
			_textPassword=new TextBox() { Location=new Point(xUnder,_radioPassword.Bottom+UiGrid.ROW_GAP),Size=new Size(250,UiGrid.FIELD_HEIGHT),
				UseSystemPasswordChar=true };
			Label labelPasswordHint=new Label() { Text="Blank keeps the stored password.",Location=new Point(_textPassword.Right+UiGrid.ITEM_GAP,_textPassword.Top),
				Size=new Size(UiGrid.PAD+widthGroupContent-_textPassword.Right-UiGrid.ITEM_GAP,UiGrid.FIELD_HEIGHT),ForeColor=SystemColors.GrayText,
				TextAlign=ContentAlignment.MiddleLeft };
			groupCredential.Height=_textPassword.Bottom+UiGrid.PAD;
			_radioCertificate.CheckedChanged+=(sender,e) => SetCredentialEnabled();
			_radioPassword.CheckedChanged+=(sender,e) => SetCredentialEnabled();
			groupCredential.Controls.AddRange(new Control[] { _radioCertificate,_labelCertificateCurrent,_butImportPfx,_labelPfxPending,_radioPassword,
				_textPassword,labelPasswordHint });
			Controls.Add(groupCredential);
			Label labelNote=new Label() { Text="A server on this computer (localhost, 127.0.0.1, ::1) needs no CA or certificate. Any other server is reached "
				+"only encrypted, and its certificate must come from the office CA and name the server.",
				Location=new Point(UiGrid.PAD,groupCredential.Bottom+UiGrid.SIBLING_GAP),Size=new Size(widthContent,34) };
			Controls.Add(labelNote);
			int yBar=ClientSize.Height-UiGrid.PAD-26;
			_labelStatus=new Label() { Location=new Point(UiGrid.PAD,labelNote.Bottom+UiGrid.ROW_GAP),
				Size=new Size(widthContent,yBar-UiGrid.SECTION_GAP-labelNote.Bottom-UiGrid.ROW_GAP) };
			Controls.Add(_labelStatus);
			Button butTest=new Button() { Text="&Test connection",Location=new Point(UiGrid.PAD,yBar),Size=new Size(120,26) };
			butTest.Click+=butTest_Click;
			Button butCancel=new Button() { Text="Cancel",Location=new Point(ClientSize.Width-UiGrid.PAD-75,yBar),Size=new Size(75,26),DialogResult=DialogResult.Cancel };
			Button butSave=new Button() { Text="Save",Location=new Point(butCancel.Left-UiGrid.DIALOG_GAP-75,yBar),Size=new Size(75,26) };
			butSave.Click+=butSave_Click;
			Controls.AddRange(new Control[] { butTest,butSave,butCancel });
			CancelButton=butCancel;
		}

		///<summary>UI-12 slice 14: the label column's width (Office CA certificate: on one line) and the field column after it.</summary>
		private const int WIDTH_LABEL=132;
		private const int X_FIELD=UiGrid.PAD+WIDTH_LABEL+UiGrid.LABEL_GAP;

		private TextBox AddRow(string caption,int y) {
			Controls.Add(new Label() { Text=caption,Location=new Point(UiGrid.PAD,y),Size=new Size(WIDTH_LABEL,UiGrid.FIELD_HEIGHT),TextAlign=ContentAlignment.MiddleRight });
			TextBox textBox=new TextBox() { Location=new Point(X_FIELD,y),Size=new Size(ClientSize.Width-UiGrid.PAD-X_FIELD,UiGrid.FIELD_HEIGHT) };
			Controls.Add(textBox);
			return textBox;
		}

		private void FormDbSettings_Load(object sender,EventArgs e) {
			try {
				_dbConnectionConfigCurrent=DbSettingsRunner.ReadCurrent(_configPath);
			}
			catch(DbSettingsException ex) {
				MessageBox.Show(ex.Message+"\r\n\r\nSaving writes a new connection into it.",TITLE);
			}
			_textServer.Text=_dbSettingsRequestStart.Server??_dbConnectionConfigCurrent.ServerName;
			_textDatabase.Text=_dbSettingsRequestStart.Database??_dbConnectionConfigCurrent.Database;
			_textUser.Text=_dbSettingsRequestStart.User??_dbConnectionConfigCurrent.User;
			_textCaFile.Text=_dbSettingsRequestStart.CaFile??"";
			_labelCaCurrent.Text=_dbConnectionConfigCurrent.SslCa=="" ? "Now: none." : "Now: "+_dbConnectionConfigCurrent.SslCa
				+(File.Exists(_dbConnectionConfigCurrent.SslCa) ? "" : " (the file does not exist)")+". A file chosen here is copied there.";
			if(_dbSettingsRequestStart.PfxFile!=null) {
				_pfxPathPending=_dbSettingsRequestStart.PfxFile;
				_pfxPasswordPending=_dbSettingsRequestStart.PfxPassword;
				_labelPfxPending.Text="To import when you save: "+Path.GetFileName(_pfxPathPending);
			}
			if(_dbSettingsRequestStart.DbPassword!=null) {
				_textPassword.Text=_dbSettingsRequestStart.DbPassword;
			}
			ShowCurrentCertificate();
			bool isCertificate=DbSettingsRunner.ResolveAuth(_dbSettingsRequestStart,_dbConnectionConfigCurrent)==DbSettingsAuth.Certificate;
			_radioCertificate.Checked=isCertificate;
			_radioPassword.Checked=!isCertificate;
			SetCredentialEnabled();
			ShowRootWarning();
		}

		///<summary>Shows the warning about the user root for a server over the network in the status line (DbSettingsRunner.GetRootUserWarning),
		///or clears it when it no longer applies. Only a warning: Save still saves.</summary>
		private void ShowRootWarning() {
			string warning=DbSettingsRunner.GetRootUserWarning(_textServer.Text.Trim(),_textUser.Text.Trim());
			if(warning!="") {
				_labelStatus.Text=warning;
				_labelStatus.ForeColor=Color.DarkRed;
				return;
			}
			if(_labelStatus.ForeColor==Color.DarkRed) {
				_labelStatus.Text="";
				_labelStatus.ForeColor=SystemColors.ControlText;
			}
		}

		private void FormDbSettings_Shown(object sender,EventArgs e) {
			//Setup's page passes the .pfx with /pfx= and never its password: ask for it now, not only when Save fails.
			EnsurePfxPassword();
		}

		///<summary>Asks for the password of the .pfx that is to be imported when it has none yet (a .pfx from /pfx= without a secrets file, as
		///setup passes it), and checks it with the file without storing anything (ClientCertificateStore.LoadPfxForCheck). True when there is no
		///such .pfx or its password is now known; false when the user cancels or the check fails, and the .pfx then stays waiting for its
		///password.</summary>
		private bool EnsurePfxPassword() {
			if(_pfxPathPending==null || _pfxPasswordPending!=null) {
				return true;
			}
			string fileName=Path.GetFileName(_pfxPathPending);
			string password;
			try {
				password=FormSecretPrompt.Ask("The password of the client certificate file "+fileName+":");
			}
			catch(DbSettingsException) {
				_labelPfxPending.Text="To import when you save: "+fileName+" (its password is still needed)";
				return false;
			}
			try {
				using(X509Certificate2 x509Certificate2=ClientCertificateStore.LoadPfxForCheck(_pfxPathPending,password)) {
					_labelPfxPending.Text="To import when you save: "+ClientCertificateStore.Describe(x509Certificate2);
				}
			}
			catch(DbSettingsException ex) {
				_labelPfxPending.Text="To import when you save: "+fileName+" (its password is still needed)";
				MessageBox.Show(ex.Message,TITLE,MessageBoxButtons.OK,MessageBoxIcon.Warning);
				return false;
			}
			_pfxPasswordPending=password;
			return true;
		}

		private void ShowCurrentCertificate() {
			string thumbprint=_dbSettingsRequestStart.Thumbprint??_dbConnectionConfigCurrent.ClientCertThumbprint;
			if(string.IsNullOrEmpty(thumbprint)) {
				_labelCertificateCurrent.Text="Now: none.";
				return;
			}
			X509Certificate2 x509Certificate2=null;
			try {
				x509Certificate2=ClientCertificateStore.FindInStore(thumbprint);
			}
			catch(Exception ex) {
				ex.DoNothing();
			}
			_labelCertificateCurrent.Text=x509Certificate2==null ? "Now: "+thumbprint+" (not in the computer's store)."
				: "Now: "+ClientCertificateStore.Describe(x509Certificate2)+".";
		}

		private void SetCredentialEnabled() {
			_butImportPfx.Enabled=_radioCertificate.Checked;
			_textPassword.Enabled=_radioPassword.Checked;
		}

		private void butBrowseCa_Click(object sender,EventArgs e) {
			using(OpenFileDialog openFileDialog=new OpenFileDialog()) {
				openFileDialog.Title="The office CA certificate";
				openFileDialog.Filter="Certificates (*.pem;*.crt;*.cer)|*.pem;*.crt;*.cer|All files (*.*)|*.*";
				if(openFileDialog.ShowDialog(this)!=DialogResult.OK) {
					return;
				}
				try {
					_labelStatus.ForeColor=SystemColors.ControlText;
					_labelStatus.Text="CA: "+OfficeCaFile.Describe(OfficeCaFile.Load(openFileDialog.FileName));
					_textCaFile.Text=openFileDialog.FileName;
				}
				catch(DbSettingsException ex) {
					MessageBox.Show(ex.Message,TITLE);
				}
			}
		}

		private void butImportPfx_Click(object sender,EventArgs e) {
			using(OpenFileDialog openFileDialog=new OpenFileDialog()) {
				openFileDialog.Title="The client certificate";
				openFileDialog.Filter="Client certificate (*.pfx;*.p12)|*.pfx;*.p12|All files (*.*)|*.*";
				if(openFileDialog.ShowDialog(this)!=DialogResult.OK) {
					return;
				}
				string password;
				try {
					password=FormSecretPrompt.Ask("The password of "+Path.GetFileName(openFileDialog.FileName)+":");
				}
				catch(DbSettingsException) {
					return;
				}
				try {
					using(X509Certificate2 x509Certificate2=ClientCertificateStore.LoadPfxForCheck(openFileDialog.FileName,password)) {
						_labelPfxPending.Text="To import when you save: "+ClientCertificateStore.Describe(x509Certificate2);
					}
					_pfxPathPending=openFileDialog.FileName;
					_pfxPasswordPending=password;
				}
				catch(DbSettingsException ex) {
					MessageBox.Show(ex.Message,TITLE);
				}
			}
		}

		private DbSettingsRequest RequestFromScreen() {
			DbSettingsRequest dbSettingsRequest=new DbSettingsRequest();
			dbSettingsRequest.IsWindow=true;
			dbSettingsRequest.Server=_textServer.Text.Trim();
			dbSettingsRequest.Database=_textDatabase.Text.Trim();
			dbSettingsRequest.User=_textUser.Text.Trim();
			dbSettingsRequest.CaFile=_textCaFile.Text.Trim()=="" ? null : _textCaFile.Text.Trim();
			if(_radioCertificate.Checked) {
				dbSettingsRequest.Auth=DbSettingsAuth.Certificate;
				dbSettingsRequest.PfxFile=_pfxPathPending;
				dbSettingsRequest.PfxPassword=_pfxPasswordPending;
				if(_pfxPathPending==null) {
					dbSettingsRequest.Thumbprint=_dbSettingsRequestStart.Thumbprint;
				}
			}
			else {
				dbSettingsRequest.Auth=DbSettingsAuth.Password;
				dbSettingsRequest.DbPassword=_textPassword.Text=="" ? null : _textPassword.Text;
			}
			return dbSettingsRequest;
		}

		private void butTest_Click(object sender,EventArgs e) {
			DbSettingsRequest dbSettingsRequest=RequestFromScreen();
			if(dbSettingsRequest.PfxFile!=null) {
				MessageBox.Show("The client certificate is imported when you save. Save first, then test.",TITLE);
				return;
			}
			string sslCa=dbSettingsRequest.CaFile??_dbConnectionConfigCurrent.SslCa;
			string thumbprint="";
			string password="";
			if(dbSettingsRequest.Auth==DbSettingsAuth.Certificate) {
				thumbprint=DbConnectionConfig.NormalizeThumbprint(dbSettingsRequest.Thumbprint??_dbConnectionConfigCurrent.ClientCertThumbprint);
			}
			else if(dbSettingsRequest.DbPassword!=null) {
				password=dbSettingsRequest.DbPassword;
			}
			else if(dbSettingsRequest.User==_dbConnectionConfigCurrent.User) {
				try {
					password=_dbConnectionConfigCurrent.GetPassword(dbSettingsRequest.Server);
				}
				catch(DbConnectionRefusedException ex) {
					MessageBox.Show(ex.Message,TITLE);
					return;
				}
			}
			Cursor=Cursors.WaitCursor;
			DbSettingsResult dbSettingsResult=DbSettingsRunner.TestConnection(dbSettingsRequest.Server,dbSettingsRequest.Database,dbSettingsRequest.User,
				password,sslCa,thumbprint);
			Cursor=Cursors.Default;
			_labelStatus.ForeColor=SystemColors.ControlText;
			_labelStatus.Text=dbSettingsResult.Message;
			MessageBox.Show(dbSettingsResult.Message,TITLE,MessageBoxButtons.OK,
				dbSettingsResult.ExitCode==DbSettingsRunner.EXIT_OK ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
		}

		private void butSave_Click(object sender,EventArgs e) {
			string warningRoot=DbSettingsRunner.GetRootUserWarning(_textServer.Text.Trim(),_textUser.Text.Trim());
			if(warningRoot!="" && MessageBox.Show(warningRoot+"\r\n\r\nSave with the user root anyway?",TITLE,MessageBoxButtons.YesNo,
				MessageBoxIcon.Warning)!=DialogResult.Yes)
			{
				_textUser.Focus();
				return;//A warning only: the administrator may change the user, or save anyway.
			}
			if(_radioCertificate.Checked && !EnsurePfxPassword()) {
				return;//The .pfx still waits for its password; nothing is saved.
			}
			Cursor=Cursors.WaitCursor;
			DbSettingsResult dbSettingsResult=DbSettingsRunner.Apply(RequestFromScreen(),_configPath,_caCopyPath);
			Cursor=Cursors.Default;
			_labelStatus.ForeColor=SystemColors.ControlText;
			_labelStatus.Text=dbSettingsResult.Message;
			if(dbSettingsResult.ExitCode!=DbSettingsRunner.EXIT_OK) {
				MessageBox.Show(dbSettingsResult.Message,TITLE,MessageBoxButtons.OK,MessageBoxIcon.Warning);
				return;
			}
			Result=dbSettingsResult;
			_pfxPasswordPending=null;
			if(MessageBox.Show(dbSettingsResult.Message+"\r\n\r\nTest the connection now?",TITLE,MessageBoxButtons.YesNo)==DialogResult.Yes) {
				Cursor=Cursors.WaitCursor;
				DbSettingsResult dbSettingsResultTest=DbSettingsRunner.TestSaved(_configPath);
				Cursor=Cursors.Default;
				MessageBox.Show(dbSettingsResultTest.Message,TITLE,MessageBoxButtons.OK,
					dbSettingsResultTest.ExitCode==DbSettingsRunner.EXIT_OK ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
			}
			DialogResult=DialogResult.OK;
			Close();
		}
	}
}
