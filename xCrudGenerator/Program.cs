using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CodeBase;

namespace xCrudGenerator {
	static class Program {
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args) {
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
			string database=GetCommandLineValue("database=",args);
			string server=GetCommandLineValue("server=",args);
			string user=GetCommandLineValue("user=",args);
			string password=GetCommandLineValue("password=",args);
			bool doUpdateDb=GetCommandLineValue("doupdatedb=",args).In("true","1");
			if(string.IsNullOrEmpty(server)) {
				server="localhost";
			}
			if(string.IsNullOrEmpty(user)) {
				user="root";
			}
			if(doUpdateDb && !string.IsNullOrEmpty(database)) {
				CrudGenHelper.UpdateDatabase(database,server,user,password);
				return;
			}
			string crudTypes=GetCommandLineValue("crud=",args);
			if(!string.IsNullOrEmpty(crudTypes)) {
				Environment.Exit(Form1.GenerateCrudFromCommandLine(crudTypes));
				return;
			}
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new Form1());
		}

		///<summary>Returns the remainer of the command line argument that is after the cmdLineKey. If not founds, returns an empty string.</summary>
		private static string GetCommandLineValue(string cmdLineKey,string[] args) {
			foreach(string clArg in args) {
				if(clArg.ToLower().StartsWith(cmdLineKey) && clArg.Length > cmdLineKey.Length) {
					return clArg.Substring(cmdLineKey.Length).Trim('"');
				}
			}
			return "";
		}
	}
}
