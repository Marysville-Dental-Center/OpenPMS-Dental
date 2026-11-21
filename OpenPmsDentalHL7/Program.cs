using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Microsoft.Win32;
using CodeBase;

namespace OpenPmsDentalHL7 {
	static class Program {
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main(string[] args) {
			//Before anything creates a window (S3b-4 of docs/specs/net10-plan.md): the body font (UiTheme.FontBody, Segoe UI 9 since UI-2b,
			//section 5.1 of docs/specs/ui-refresh-plan.md; SetDefaultFont throws once a window exists), then per-monitor DPI awareness, the program's mode (the
			//.NET Framework build of the program set it in its app.config, which .NET does not read).
			Application.SetDefaultFont(new System.Drawing.Font("Segoe UI",9f));
			Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
			//The UI refresh's token renderer (UI-6, docs/specs/ui-refresh-plan.md section 3.6) for every stock menu and tool strip of this thread.
			CodeBase.UiTheme.InitWinForms();
			//Then the Windows code pages for Encoding.GetEncoding (.NET has only the Unicode encodings, ASCII and Latin-1 built in). This
			//program does not call ODInitialize.Initialize, which registers them for the others (S3b-3 of the .NET 10 plan).
			System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
			if(ODBuild.IsDebug()) {
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				Application.Run(new FormDebug(ProgramNames.HL7ServiceName));
			}
			else {
				EventLog.WriteEntry(ProgramNames.EventLogSourceHL7Main, DateTime.Now.ToLongTimeString() +" - Service main method starting...");
				ServiceHL7 serviceHL7=new ServiceHL7();
				serviceHL7.ServiceName=ProgramNames.HL7ServiceFallbackName;//default when no installed service matches this folder (not the installer's name)
				//Get the executing assembly location directory (location of this OpenPmsDentalHL7.exe)
				string executingDir=Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
				//Get all installed services
				List<ServiceController> serviceControllersOD=new List<ServiceController>();
				ServiceController[] serviceControllersAll=ServiceController.GetServices();
				//Get all installed services whose names start with ProgramNames.ServicePrefix, or with the prefix of builds before the rename, so
				//a service installed then (OpenDentHL7) still finds its own name
				for(int i=0;i<serviceControllersAll.Length;i++) {
					if(ProgramNames.HasServicePrefix(serviceControllersAll[i].ServiceName)) {
						serviceControllersOD.Add(serviceControllersAll[i]);
					}
				}
				string pathToODHL7exe;
				//Get the service that is installed from the same directory as the current directory
				for(int i=0;i<serviceControllersOD.Count;i++) {
					RegistryKey hklm=Registry.LocalMachine;
					hklm=hklm.OpenSubKey(@"System\CurrentControlSet\Services\"+serviceControllersOD[i].ServiceName);
					pathToODHL7exe=hklm.GetValue("ImagePath").ToString();
					pathToODHL7exe=pathToODHL7exe.Replace("\"","");
					pathToODHL7exe=Path.GetDirectoryName(pathToODHL7exe);
					if(pathToODHL7exe==executingDir) {
						//Set the name of the service to run as the name of the service installed from this directory
						serviceHL7.ServiceName=serviceControllersOD[i].ServiceName;
						break;
					}
				}
				ServiceBase.Run(serviceHL7);
				EventLog.WriteEntry(ProgramNames.EventLogSourceHL7Main,DateTime.Now.ToLongTimeString() +" - Service main method exiting...");
			}
		}
	}
}
