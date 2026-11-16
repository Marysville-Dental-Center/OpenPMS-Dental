// The Middle Tier server's entry point (package S3b-6 of the .NET 10 move, docs/port-notes/S3b-6.md): a Windows service by default
// (registered with sc.exe, docs/port-notes/S3b-6.md), an IIS in-process application as an option (web.config), or a console program.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;

namespace OpenPmsDentalServer {
	public static class Program {
		///<summary>Runs the server until the service, IIS or Ctrl+C stops it. Exit code 1 when its addresses are not HTTPS only or a setting is
		///wrong (logged, and written to standard error).</summary>
		public static int Main(string[] args) {
			//The Windows code pages first, before anything reads ANSI text (S3b-2; DtoProcessor's ODInitialize.Initialize registers them again).
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
			WebApplication webApplication;
			try {
				webApplication=ServerHost.Build(args,null,null);
			}
			catch(InvalidOperationException ex) {
				Console.Error.WriteLine(ex.Message);
				return 1;
			}
			List<string> listProblems=ServerHost.GetHttpsProblems(webApplication.Configuration,ServerHost.IsHostedByIis());
			if(listProblems.Count>0) {
				foreach(string problem in listProblems) {
					webApplication.Logger.LogCritical(problem);
					Console.Error.WriteLine(problem);
				}
				return 1;
			}
			ServerHost.Configure(webApplication);
			webApplication.Run();
			return 0;
		}
	}
}
