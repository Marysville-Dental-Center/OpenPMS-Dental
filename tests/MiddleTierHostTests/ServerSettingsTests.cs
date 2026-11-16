// The Middle Tier server's settings and limits (package S3b-6 of the .NET 10 move, docs/port-notes/S3b-6.md), without a database: the
// defaults (/OpenDentalServer, 1 GiB), the limits Kestrel and IIS get, the HTTPS-only check of the addresses, the IIS web.config and the
// appsettings.json that ship with the server.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalServer;

namespace MiddleTierHostTests {
	[TestClass]
	public class ServerSettingsTests {
		[TestMethod]
		public void Defaults_AreTheAsmxPathAndOneGiB() {
			ServerSettings serverSettings=ServerSettings.Read(Configuration(new Dictionary<string,string>()));
			Assert.AreEqual("/OpenDentalServer",serverSettings.PathBase.Value);
			Assert.AreEqual(1073741824L,serverSettings.MaxRequestBodySize);
			Assert.AreEqual(1073741824L,ServerSettings.DEFAULT_MAX_REQUEST_BODY_SIZE);
		}

		[TestMethod]
		public void Settings_AreRead_AndAWrongOneIsRefused() {
			ServerSettings serverSettings=ServerSettings.Read(Configuration(new Dictionary<string,string> {
				{ "MiddleTier:PathBase","/Clinic2/" },{ "MiddleTier:MaxRequestBodySize","2048" } }));
			Assert.AreEqual("/Clinic2",serverSettings.PathBase.Value);
			Assert.AreEqual(2048L,serverSettings.MaxRequestBodySize);
			Assert.IsFalse(ServerSettings.Read(Configuration(new Dictionary<string,string> { { "MiddleTier:PathBase","/" } })).PathBase.HasValue,
				"/ is the site root.");
			Assert.ThrowsExactly<InvalidOperationException>(() => ServerSettings.Read(Configuration(new Dictionary<string,string> {
				{ "MiddleTier:PathBase","Clinic2" } })));
			Assert.ThrowsExactly<InvalidOperationException>(() => ServerSettings.Read(Configuration(new Dictionary<string,string> {
				{ "MiddleTier:MaxRequestBodySize","0" } })));
			Assert.ThrowsExactly<InvalidOperationException>(() => ServerSettings.Read(Configuration(new Dictionary<string,string> {
				{ "MiddleTier:MaxRequestBodySize","1GB" } })));
		}

		///<summary>The application ServerHost builds gives Kestrel and IIS the server's limit (1 GiB by default), as the old Web.config gave IIS and
		///ASP.NET. It is built, not started.</summary>
		[TestMethod]
		public void KestrelAndIis_GetTheBodyLimit() {
			string contentRoot=Path.Combine(Path.GetTempPath(),"s3b6_ht_settings_"+Guid.NewGuid().ToString("N").Substring(0,12));
			Directory.CreateDirectory(contentRoot);
			try {
				foreach(long? limit in new long?[] { null,4096 }) {
					WebApplication webApplication=ServerHost.Build(limit==null ? new string[0] : new[] { "--MiddleTier:MaxRequestBodySize="+limit },
						contentRoot,null);
					try {
						long expected=limit??1073741824L;
						Assert.AreEqual(expected,webApplication.Services.GetRequiredService<IOptions<KestrelServerOptions>>().Value.Limits.MaxRequestBodySize);
						Assert.AreEqual(expected,webApplication.Services.GetRequiredService<IOptions<IISServerOptions>>().Value.MaxRequestBodySize);
						Assert.AreEqual(expected,webApplication.Services.GetRequiredService<ServerSettings>().MaxRequestBodySize);
						Assert.AreEqual(contentRoot.TrimEnd('\\'),webApplication.Environment.ContentRootPath.TrimEnd('\\'),"The content root the caller gave.");
					}
					finally {
						((IDisposable)webApplication).Dispose();
					}
				}
			}
			finally {
				Directory.Delete(contentRoot,true);
			}
		}

		[TestMethod]
		public void HttpsProblems_PlainHttpOrNoAddress_AreRefused() {
			Assert.AreEqual(0,ServerHost.GetHttpsProblems(Configuration(new Dictionary<string,string> {
				{ "Kestrel:Endpoints:Https:Url","https://*:443" } }),false).Count);
			Assert.AreEqual(0,ServerHost.GetHttpsProblems(Configuration(new Dictionary<string,string> { { "urls","https://127.0.0.1:7443" } }),false).Count);
			Assert.AreEqual(0,ServerHost.GetHttpsProblems(Configuration(new Dictionary<string,string> { { "https_ports","443" } }),false).Count);
			List<string> listProblems=ServerHost.GetHttpsProblems(Configuration(new Dictionary<string,string> {
				{ "Kestrel:Endpoints:Https:Url","https://*:443" },{ "Kestrel:Endpoints:Http:Url","http://*:80" } }),false);
			Assert.AreEqual(1,listProblems.Count,string.Join("; ",listProblems));
			StringAssert.Contains(listProblems[0],"http://*:80");
			Assert.AreEqual(1,ServerHost.GetHttpsProblems(Configuration(new Dictionary<string,string> { { "urls","http://127.0.0.1:5000" } }),false).Count);
			Assert.AreEqual(1,ServerHost.GetHttpsProblems(Configuration(new Dictionary<string,string> {
				{ "urls","https://127.0.0.1:7443" },{ "http_ports","8080" } }),false).Count);
			listProblems=ServerHost.GetHttpsProblems(Configuration(new Dictionary<string,string>()),false);
			Assert.AreEqual(1,listProblems.Count);
			StringAssert.Contains(listProblems[0],"No HTTPS address");
			//In IIS the site's bindings decide, and the endpoint refuses plain HTTP itself.
			Assert.AreEqual(0,ServerHost.GetHttpsProblems(Configuration(new Dictionary<string,string>()),true).Count);
			Assert.IsFalse(ServerHost.IsHostedByIis());
		}

		///<summary>What ships with the server: appsettings.json with one HTTPS endpoint and a certificate from the machine's store, the default path
		///base and limit; web.config for IIS in-process with the same limit.</summary>
		[TestMethod]
		public void ShippedFiles_AppSettingsAndWebConfig() {
			string folderServer=Path.GetDirectoryName(HostDb.FindRepoFile(Path.Combine("OpenPmsDentalServer","OpenPmsDentalServer.csproj")));
			IConfigurationRoot configurationRoot=new ConfigurationBuilder().AddJsonFile(Path.Combine(folderServer,"appsettings.json")).Build();
			Assert.AreEqual(0,ServerHost.GetHttpsProblems(configurationRoot,false).Count);
			Assert.AreEqual("https://*:443",configurationRoot["Kestrel:Endpoints:Https:Url"]);
			Assert.AreEqual("LocalMachine",configurationRoot["Kestrel:Endpoints:Https:Certificate:Location"]);
			ServerSettings serverSettings=ServerSettings.Read(configurationRoot);
			Assert.AreEqual("/OpenDentalServer",serverSettings.PathBase.Value);
			Assert.AreEqual(1073741824L,serverSettings.MaxRequestBodySize);
			XDocument xDocument=XDocument.Load(Path.Combine(folderServer,"web.config"));
			XElement xElementAspNetCore=xDocument.Descendants("aspNetCore").Single();
			Assert.AreEqual(@".\OpenPmsDentalServer.exe",(string)xElementAspNetCore.Attribute("processPath"));
			Assert.AreEqual("inprocess",(string)xElementAspNetCore.Attribute("hostingModel"));
			Assert.AreEqual("AspNetCoreModuleV2",(string)xDocument.Descendants("handlers").Single().Element("add").Attribute("modules"));
			Assert.AreEqual("1073741824",(string)xDocument.Descendants("requestLimits").Single().Attribute("maxAllowedContentLength"));
		}

		private static IConfiguration Configuration(Dictionary<string,string> dictValues) {
			return new ConfigurationBuilder().AddInMemoryCollection(dictValues).Build();
		}
	}
}
