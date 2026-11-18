// The Middle Tier's ASP.NET Core host (package S3b-6 of the .NET 10 move, docs/port-notes/S3b-6.md): builds the web application, its limits
// and its one endpoint. Program.Main runs it; the host tests (tests/MiddleTierHostTests) build the same application on the test server.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenPmsDentalBusiness;

namespace OpenPmsDentalServer {
	///<summary>The parts of the server that Program.Main and the host tests share.</summary>
	public static class ServerHost {
		///<summary>The Windows service's name (ServiceName for its event log and its start and stop), also the name sc.exe registers it under in
		///docs/port-notes/S3b-6.md.</summary>
		public const string SERVICE_NAME="OpenPmsDentalServer";
		///<summary>The environment variable the ASP.NET Core Module of IIS sets for the application it starts (in-process or out-of-process).</summary>
		private const string IIS_PHYSICAL_PATH="ASPNETCORE_IIS_PHYSICAL_PATH";

		///<summary>The web application, not started. contentRoot is the folder of the database settings (OpenDentalServerConfig.xml): null for the
		///exe's own folder (AppContext.BaseDirectory, the folder the service or IIS runs it from), unless args name one (--contentRoot).
		///actionConfigureBuilder, when not null, changes the builder before the settings are read (the host tests: the test server and their
		///settings). Throws InvalidOperationException for a setting the server cannot use (ServerSettings.Read).</summary>
		public static WebApplication Build(string[] args,string contentRoot,Action<WebApplicationBuilder> actionConfigureBuilder) {
			//The content root is a host setting, fixed once the builder exists, so it goes in with the arguments (C# 8 has no init-only setters
			//for WebApplicationOptions). The last --contentRoot wins: the caller's, then the command line's, then the exe's folder.
			List<string> listArgs=new List<string>();
			if(contentRoot==null && !(args??new string[0]).Any(x => x.TrimStart('-','/').StartsWith("contentRoot",StringComparison.OrdinalIgnoreCase))) {
				listArgs.Add("--contentRoot");
				listArgs.Add(AppContext.BaseDirectory);
			}
			listArgs.AddRange(args??new string[0]);
			if(contentRoot!=null) {
				listArgs.Add("--contentRoot");
				listArgs.Add(contentRoot);
			}
			WebApplicationBuilder webApplicationBuilder=WebApplication.CreateBuilder(listArgs.ToArray());
			//A Windows service when the service control manager started it (the default way to run it); a console program or an IIS application
			//otherwise, where this does nothing. Its event log takes the server's log.
			webApplicationBuilder.Services.AddWindowsService(x => x.ServiceName=SERVICE_NAME);
			actionConfigureBuilder?.Invoke(webApplicationBuilder);
			ServerSettings serverSettings=ServerSettings.Read(webApplicationBuilder.Configuration);
			webApplicationBuilder.Services.AddSingleton(serverSettings);
			//1 GiB (ServerSettings) in Kestrel and in IIS in-process; web.config gives IIS's request filtering the same limit. Neither has an
			//execution timeout for a request (Web.config's executionTimeout of 3600 seconds has no counterpart): the client waits up to an hour.
			webApplicationBuilder.WebHost.ConfigureKestrel(x => {
				x.Limits.MaxRequestBodySize=serverSettings.MaxRequestBodySize;
				x.AddServerHeader=false;
			});
			webApplicationBuilder.Services.Configure<IISServerOptions>(x => x.MaxRequestBodySize=serverSettings.MaxRequestBodySize);
			return webApplicationBuilder.Build();
		}

		///<summary>The endpoint for every request, and the ServerMT role for the whole process: every thread of a Middle Tier server is the
		///server's, the threads the business layer starts included (each request also sets it for its own thread, MiddleTierEndpoint).</summary>
		public static MiddleTierEndpoint Configure(WebApplication webApplication) {
			RemotingClient.MiddleTierRole=MiddleTierRole.ServerMT;
			//The exceptions the client gets as a DtoException (type and message only) go to the server's log with their stack: an ODException (a
			//wrong password, a version mismatch, a rule of the business layer) as information, anything else as an error, which a Windows service
			//also writes to the Application event log (its default level there is Warning).
			ILogger logger=webApplication.Services.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(DtoProcessor).FullName);
			DtoProcessor.ActionLogException=(methodName,exception) => {
				if(exception is CodeBase.ODException) {
					logger.LogInformation("{Method}: {Type}: {Message}",methodName,exception.GetType().Name,exception.Message);
					return;
				}
				logger.LogError(exception,"The Middle Tier server answered {Method} with an error.",methodName);
			};
			MiddleTierEndpoint middleTierEndpoint=new MiddleTierEndpoint(webApplication.Services.GetRequiredService<ServerSettings>(),
				webApplication.Environment.ContentRootPath);
			webApplication.Run(middleTierEndpoint.HandleAsync);
			return middleTierEndpoint;
		}

		///<summary>True when the ASP.NET Core Module of IIS started this process: IIS's bindings decide the addresses, and Kestrel's are not used.
		///</summary>
		public static bool IsHostedByIis() {
			return !string.IsNullOrEmpty(Environment.GetEnvironmentVariable(IIS_PHYSICAL_PATH));
		}

		///<summary>Why the configured addresses are not HTTPS only, one line each; empty when they are. Kestrel's addresses come from its
		///Endpoints section (appsettings.json), urls (--urls, ASPNETCORE_URLS) and http_ports/https_ports (ASPNETCORE_HTTP_PORTS,
		///ASPNETCORE_HTTPS_PORTS); with none of them Kestrel would listen on http://localhost:5000. In IIS (isIis) the site's bindings decide and the
		///endpoint refuses a plain HTTP request itself.</summary>
		public static List<string> GetHttpsProblems(IConfiguration configuration,bool isIis) {
			List<string> listProblems=new List<string>();
			if(isIis) {
				return listProblems;
			}
			List<string> listUrls=new List<string>();
			foreach(IConfigurationSection section in configuration.GetSection("Kestrel:Endpoints").GetChildren()) {
				string url=section["Url"];
				if(string.IsNullOrWhiteSpace(url)) {
					listProblems.Add("Kestrel:Endpoints:"+section.Key+" has no Url.");
					continue;
				}
				listUrls.Add(url.Trim());
			}
			string urls=configuration["urls"];
			if(!string.IsNullOrWhiteSpace(urls)) {
				listUrls.AddRange(urls.Split(new[] { ';' },StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).Where(x => x!=""));
			}
			if(!string.IsNullOrWhiteSpace(configuration["http_ports"])) {
				listProblems.Add("http_ports (ASPNETCORE_HTTP_PORTS) asks for plain HTTP ports: "+configuration["http_ports"]+".");
			}
			bool hasHttpsPorts=!string.IsNullOrWhiteSpace(configuration["https_ports"]);
			foreach(string url in listUrls) {
				if(!url.StartsWith("https://",StringComparison.OrdinalIgnoreCase)) {
					listProblems.Add("Not an HTTPS address: "+url+". The Middle Tier server listens on HTTPS only.");
				}
			}
			if(listUrls.Count==0 && !hasHttpsPorts) {
				listProblems.Add("No HTTPS address is configured: set Kestrel:Endpoints:Https:Url and its certificate in appsettings.json.");
			}
			return listProblems;
		}
	}
}
