// One Middle Tier server of the host tests (package S3b-6 of the .NET 10 move, docs/port-notes/S3b-6.md): OpenPmsDentalServer's own web
// application (ServerHost.Build and Configure, the endpoint the Windows service runs) on the in-process test server, in an assembly load
// context of its own. The server's copies of the business layer, CodeBase and DataConnectionBase have their own static state (the database
// connection, the caches, the ServerMT role, the thread-static request state), as each ASMX application had in its own AppDomain in IIS, so
// two servers on two databases and the test's client share none of it. The client reaches the server through the test server's handler
// (OpenDentalServerHttp.HttpMessageHandlerOverride, RoutingHandler).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Xml;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenPmsDentalServer;

namespace MiddleTierHostTests {
	///<summary>A started server: the handler that reaches it, its folder and the database it reads from its config file.</summary>
	public sealed class IsolatedServer:IDisposable {
		///<summary>The name of the server's load context, read inside it: proof that the server's business layer is not the client's.</summary>
		public readonly string LoadContextName;
		public readonly HttpMessageHandler Handler;
		public readonly string ContentRoot;
		public readonly string PathBase;
		private readonly IDisposable _disposableStop;

		private IsolatedServer(HttpMessageHandler handler,IDisposable disposableStop,string loadContextName,string contentRoot,string pathBase) {
			Handler=handler;
			_disposableStop=disposableStop;
			LoadContextName=loadContextName;
			ContentRoot=contentRoot;
			PathBase=pathBase;
		}

		///<summary>Writes the server's config file into a new folder (its content root), named after the application as the ASMX host required for
		///several applications in one folder ("/S3b6A" reads S3b6AConfig.xml), loads the server into a new load context and starts it. Throws when it
		///cannot start.</summary>
		public static IsolatedServer Start(string name,string pathBase,string database,long? maxRequestBodySize) {
			string contentRoot=Path.Combine(Path.GetTempPath(),"s3b6_ht_"+Guid.NewGuid().ToString("N").Substring(0,12)+"_"+name);
			Directory.CreateDirectory(contentRoot);
			WriteConfigFile(Path.Combine(contentRoot,pathBase.Trim('/')+"Config.xml"),database,pathBase);
			ServerLoadContext serverLoadContext=new ServerLoadContext(name,Path.GetDirectoryName(typeof(IsolatedServer).Assembly.Location));
			Assembly assemblyTests=serverLoadContext.LoadFromAssemblyPath(typeof(IsolatedServer).Assembly.Location);
			MethodInfo methodInfoStart=assemblyTests.GetType(typeof(ServerBridge).FullName,true).GetMethod(nameof(ServerBridge.Start));
			object[] arrayResults;
			try {
				arrayResults=(object[])methodInfoStart.Invoke(null,new object[] { contentRoot,pathBase,maxRequestBodySize?.ToString() });
			}
			catch(TargetInvocationException ex) {
				throw new InvalidOperationException("The server "+name+" did not start: "+(ex.InnerException??ex),ex.InnerException??ex);
			}
			return new IsolatedServer((HttpMessageHandler)arrayResults[0],(IDisposable)arrayResults[1],(string)arrayResults[2],contentRoot,pathBase);
		}

		///<summary>The database settings the server reads on its first request (Userods.LoadDatabaseInfoFromFile): the local MariaDB, the scratch
		///database, root with a blank password, and the application name the request will have.</summary>
		private static void WriteConfigFile(string path,string database,string applicationName) {
			XmlWriterSettings xmlWriterSettings=new XmlWriterSettings { Indent=true,Encoding=new UTF8Encoding(false) };
			using(XmlWriter xmlWriter=XmlWriter.Create(path,xmlWriterSettings)) {
				xmlWriter.WriteStartElement("ConnectionSettings");
				xmlWriter.WriteStartElement("DatabaseConnection");
				xmlWriter.WriteElementString("ComputerName",HostDb.Server);
				xmlWriter.WriteElementString("Database",database);
				xmlWriter.WriteElementString("User",HostDb.User);
				xmlWriter.WriteElementString("Password",HostDb.Password);
				xmlWriter.WriteElementString("UserLow","");
				xmlWriter.WriteElementString("PasswordLow","");
				xmlWriter.WriteElementString("DatabaseType","MySql");
				xmlWriter.WriteElementString("ApplicationName",applicationName);
				xmlWriter.WriteElementString("VerboseLogging","false");
				xmlWriter.WriteEndElement();
				xmlWriter.WriteEndElement();
			}
		}

		///<summary>Stops the server and deletes its folder. The load context stays (it is not collectible) until the test process ends.</summary>
		public void Dispose() {
			try {
				_disposableStop.Dispose();
			}
			catch(Exception) {
				//Stopping a server that failed is not what the tests check.
			}
			try {
				Directory.Delete(ContentRoot,true);
			}
			catch(Exception) {
				//A file the server still held; Windows cleans the temporary folder.
			}
		}
	}

	///<summary>Loads the server and the libraries with static state from the test's folder into this context, so each server has its own copy;
	///everything else (the shared frameworks, ASP.NET Core, the packages) comes from the default context and is shared, so the test server's handler
	///is the HttpMessageHandler type the client knows.</summary>
	public class ServerLoadContext:AssemblyLoadContext {
		///<summary>The assemblies each server has its own copy of: the server, the business layer with its generated serializers, the libraries
		///below it with static state, and this test assembly (for ServerBridge, which must bind to the server's copies).</summary>
		public static readonly string[] ArrayIsolated=new[] { "OpenPmsDentalServer","OpenPmsDentalBusiness","OpenPmsDentalBusiness.XmlSerializers",
			"CodeBase","DataConnectionBase","ODCrypt","OpenPmsDentalStorage","PasswordVaultWrapper","MiddleTierHostTests" };
		private readonly string _folder;

		public ServerLoadContext(string name,string folder) : base("MiddleTierServer-"+name,false) {
			_folder=folder;
		}

		protected override Assembly Load(AssemblyName assemblyName) {
			if(!ArrayIsolated.Contains(assemblyName.Name,StringComparer.OrdinalIgnoreCase)) {
				return null;//the default context's
			}
			string path=Path.Combine(_folder,assemblyName.Name+".dll");
			if(!File.Exists(path)) {
				return null;
			}
			return LoadFromAssemblyPath(path);
		}
	}

	///<summary>Runs inside a server's load context (IsolatedServer.Start calls it by reflection on that context's copy of this assembly), so every
	///type here is the server's copy.</summary>
	public static class ServerBridge {
		///<summary>Builds and starts the server as Program.Main does, on the test server instead of Kestrel (no HTTPS address check: the test
		///server's requests are HTTPS by their address), with the path base, the body limit (null for the default) and the version check of a
		///Release build (DtoProcessor.DoCheckVersionInDebug: the databases have this build's ProgramVersion). Returns the handler, the object that
		///stops the server, and the name of the business layer's load context.</summary>
		public static object[] Start(string contentRoot,string pathBase,string maxRequestBodySize) {
			OpenPmsDentalBusiness.DtoProcessor.DoCheckVersionInDebug=true;
			Dictionary<string,string> dictSettings=new Dictionary<string,string>();
			dictSettings[ServerSettings.SECTION+":PathBase"]=pathBase;
			if(maxRequestBodySize!=null) {
				dictSettings[ServerSettings.SECTION+":MaxRequestBodySize"]=maxRequestBodySize;
			}
			WebApplication webApplication=ServerHost.Build(new string[0],contentRoot,webApplicationBuilder => {
				webApplicationBuilder.WebHost.UseTestServer();
				webApplicationBuilder.Configuration.AddInMemoryCollection(dictSettings);
				//No console or Windows event log output from the test servers.
				webApplicationBuilder.Logging.ClearProviders();
			});
			ServerHost.Configure(webApplication);
			webApplication.Start();
			TestServer testServer=webApplication.GetTestServer();
			testServer.BaseAddress=new Uri("https://localhost/");
			string loadContextName=AssemblyLoadContext.GetLoadContext(typeof(OpenPmsDentalBusiness.DtoProcessor).Assembly).Name;
			return new object[] { testServer.CreateHandler(),new WebApplicationStopper(webApplication),loadContextName };
		}

		private class WebApplicationStopper:IDisposable {
			private readonly WebApplication _webApplication;

			public WebApplicationStopper(WebApplication webApplication) {
				_webApplication=webApplication;
			}

			public void Dispose() {
				_webApplication.StopAsync().GetAwaiter().GetResult();
				((IDisposable)_webApplication).Dispose();
			}
		}
	}
}
