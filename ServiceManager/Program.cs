using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace ServiceManager {
	static class Program {
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args) {
			//First: the Windows code pages for Encoding.GetEncoding (.NET has only the Unicode encodings, ASCII and Latin-1 built in). This
			//program does not call ODInitialize.Initialize, which registers them for the others (S3b-3 of the .NET 10 plan).
			System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			if(args!=null && args.Length > 0 && args[0].GetType()==typeof(string)) {
				Application.Run(new FormServiceManage(args[0],true,false));
			}
			else {
				Application.Run(new FormMain());
			}
		}
	}
}
