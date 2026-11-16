using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MySQLManager {
	static class Program {
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() {
			//Before anything creates a window (S3b-4 of docs/specs/net10-plan.md): the .NET Framework's default font, which the forms were laid out
			//with (.NET's is Segoe UI 9; SetDefaultFont throws once a window exists), then per-monitor DPI awareness, the program's mode (the
			//.NET Framework build of the program set it in its app.config, which .NET does not read).
			Application.SetDefaultFont(new System.Drawing.Font("Microsoft Sans Serif",8.25f));
			Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
			//The Windows code pages for Encoding.GetEncoding (.NET has only the Unicode encodings, ASCII and Latin-1 built in), before anything reads
			//ANSI text. This program does not call ODInitialize.Initialize, which registers them for the others (S3b-4).
			System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new FormMySQLUserManager());
		}
	}
}
