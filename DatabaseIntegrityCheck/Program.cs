using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace DatabaseIntegrityCheck {
	static class Program {
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() {
			//Before anything creates a window (S3b-4 of docs/specs/net10-plan.md): the body font (UiTheme.FontBody, Segoe UI 9 since UI-2b,
			//section 5.1 of docs/specs/ui-refresh-plan.md; SetDefaultFont throws once a window exists), then per-monitor DPI awareness, the program's mode (the
			//.NET Framework build of the program set it in its app.config, which .NET does not read).
			Application.SetDefaultFont(new System.Drawing.Font("Segoe UI",9f));
			Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
			//The UI refresh's token renderer (UI-6, docs/specs/ui-refresh-plan.md section 3.6) for every stock menu and tool strip of this thread.
			CodeBase.UiTheme.InitWinForms();
			//The Windows code pages for Encoding.GetEncoding (.NET has only the Unicode encodings, ASCII and Latin-1 built in), before anything reads
			//ANSI text. This program does not call ODInitialize.Initialize, which registers them for the others (S3b-4).
			System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new FormDatabaseCheck());
		}
	}
}