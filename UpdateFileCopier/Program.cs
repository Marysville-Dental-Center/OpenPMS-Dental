using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace UpdateFileCopier {
	static class Program {
		/// <summary>The main entry point for the application.  Takes the following arguments: sourcedir processid destdir doKillServices</summary>
		[STAThread]
		static void Main(string[] arguments) {
			//Before anything creates a window (S3b-4 of docs/specs/net10-plan.md): the .NET Framework's default font, which the forms were laid out
			//with (.NET's is Segoe UI 9; SetDefaultFont throws once a window exists), then per-monitor DPI awareness, the program's mode (the
			//.NET Framework build of the program set it in its app.config, which .NET does not read).
			Application.SetDefaultFont(new System.Drawing.Font("Microsoft Sans Serif",8.25f));
			Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
			//Then the Windows code pages for Encoding.GetEncoding (.NET has only the Unicode encodings, ASCII and Latin-1 built in). This
			//program does not call ODInitialize.Initialize, which registers them for the others (S3b-2).
			System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			if(arguments.Length==9) {
				Application.Run(new FormMain(arguments[0],arguments[1],arguments[2],Convert.ToBoolean(arguments[3]),Convert.ToBoolean(arguments[4]),arguments[5],arguments[6],arguments[7],arguments[8]));
			}
			else if(arguments.Length==5) {
				Application.Run(new FormMain(arguments[0],arguments[1],arguments[2],Convert.ToBoolean(arguments[3]),Convert.ToBoolean(arguments[4])));
			}
			else if(arguments.Length==4) {
				Application.Run(new FormMain(arguments[0],arguments[1],arguments[2],Convert.ToBoolean(arguments[3]),true));
			}
			else if(arguments.Length==3) {
				Application.Run(new FormMain(arguments[0],arguments[1],arguments[2],true,true));
			}
			else if(arguments.Length==2) {
				Application.Run(new FormMain(arguments[0],arguments[1],CodeBase.ProgramNames.DefaultInstallFolder,true,true));
			}
			else {//just for rare debugging situations
				Application.Run(new FormMain(@"C:\OpenDentImages\UpdateFiles","0",CodeBase.ProgramNames.DefaultInstallFolder,true,true));
			}
		}
	}
}
