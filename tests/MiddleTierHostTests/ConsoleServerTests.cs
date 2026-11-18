// The built Middle Tier server as a process of its own (docs/port-notes/S3b-6.md, section 9): OpenPmsDentalServer.exe from the build output,
// with the settings files tools/Test-MiddleTierService.ps1 writes for the Windows service check (its own functions write them: the PFX file,
// appsettings.json, OpenDentalServerConfig.xml), started as a console program without a window from System32 (a service's working
// directory), on Kestrel over HTTPS on 127.0.0.1, against database A. The in-process servers build their own config files, so only this test
// reads the script's; it is the service check without the service, which needs no elevation. In process only: against a running server
// (MIDDLETIER_TEST_URL) it is inconclusive. The server is not built: inconclusive, a failure with OPMS_REQUIRE_BUILD_OUTPUT=1.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalBusiness;
using OpenPmsDentalBusiness.WebServices;
using ProjectFileTests;

namespace MiddleTierHostTests {
	[TestClass]
	public class ConsoleServerTests {
		private const string SCRIPT="Test-MiddleTierService.ps1";
		private const string SERVER_EXE="OpenPmsDentalServer.exe";

		[TestInitialize]
		public void SetupTest() {
			HostTestRun.Begin();
		}

		[TestCleanup]
		public void TearDownTest() {
			RemotingClient.HasAutomaticConnectionLostRetry=false;
			RemotingClient.HasMiddleTierConnectionFailed=false;
			RemotingClient.HasLoginFailed=false;
		}

		///<summary>The client logs in through the server and gets its answers; an ODException comes back as the server's information line, and
		///anything else is in the server's log with its stack (DtoProcessor.ActionLogException).</summary>
		[TestMethod]
		public void BuiltServer_WithTheServiceCheckSettings_FromSystem32_AnswersTheClient() {
			HostTestRun.RequireInProcess("starts the built server itself, on database A");
			string script=HostDb.FindRepoFile(Path.Combine("tools",SCRIPT));
			string repoRoot=Path.GetDirectoryName(Path.GetDirectoryName(script));
			//The server built in this test's own configuration (its bin\Debug for this bin\Debug).
			string configuration=Path.GetFileName(AppContext.BaseDirectory.TrimEnd('\\','/'));
			string serverExe=Path.Combine(repoRoot,"OpenPmsDentalServer","bin",configuration,SERVER_EXE);
			BuildOutput.RequireFile(serverExe,"The server is not built: no "+serverExe+". Build OpenPmsDentalServer (x64) through the lane.");
			string folder=Path.Combine(Path.GetTempPath(),"s3b6_ht_console_"+Guid.NewGuid().ToString("N").Substring(0,12));
			Directory.CreateDirectory(folder);
			Process process=null;
			StringBuilder stringBuilderOutput=new StringBuilder();
			HttpMessageHandlerScope httpMessageHandlerScope=null;
			try {
				int port=GetFreePort();
				string[] arrayValues=WriteSettings(script,folder,port,HostTestRun.TargetA.Database);
				string thumbprint=arrayValues[0];
				string url=arrayValues[1];
				Assert.AreEqual("https://127.0.0.1:"+port+"/OpenDentalServer/ServiceMain.asmx",url);
				process=StartServer(serverExe,folder,stringBuilderOutput);
				WaitUntilListening(process,port,stringBuilderOutput);
				//The client trusts the test certificate only (as the service check's run does, MIDDLETIER_TEST_CERT_THUMBPRINT).
				httpMessageHandlerScope=new HttpMessageHandlerScope(new RoutingHandler(thumbprint));
				HostTestRun.UseTarget(new HostTarget("the built server",url,HostTestRun.TargetA.Database),HostTestRun.UserA);
				Userod userod=null;
				try {
					userod=Userods.CheckUserAndPassword(HostTestRun.UserA.UserName,HostTestRun.UserA.Password,false);
				}
				catch(Exception ex) {
					Assert.Fail("The log-in through the built server failed: "+ex.Message+"\r\nThe server's output:\r\n"+GetText(stringBuilderOutput));
				}
				Assert.AreEqual(HostTestRun.UserA.UserName,userod.UserName);
				Assert.AreEqual("Processed: <&>",WebServiceTests.GetString("<&>"));
				Assert.AreEqual(HostTestRun.TargetA.Database,MiscData.GetCurrentDatabase(),"The server reads the database the script's config file names.");
				ODException odException=Assert.ThrowsExactly<ODException>(() => WebServiceTests.GetODException());
				Assert.AreEqual("New password cannot be the same as the current password.",odException.Message);
				WaitForOutput(stringBuilderOutput,"New password cannot be the same as the current password.");
				Assert.IsFalse(GetText(stringBuilderOutput).Contains("fail:"),"No error in the server's log yet: "+GetText(stringBuilderOutput));
				//A request that is not a DTO: the client gets the DtoException, the server's log the error with its stack.
				string result=new OpenDentalServerHttp(url).ProcessRequest("not a DTO");
				StringAssert.Contains(result,"DtoException");
				WaitForOutput(stringBuilderOutput,"The Middle Tier server answered");
				string output=GetText(stringBuilderOutput);
				StringAssert.Contains(output,"fail: "+typeof(DtoProcessor).FullName,output);
				StringAssert.Contains(output,"   at OpenPmsDentalBusiness.",output);
			}
			finally {
				httpMessageHandlerScope?.Dispose();
				if(process!=null) {
					try {
						if(!process.HasExited) {
							process.Kill();
						}
						process.WaitForExit(30000);
					}
					catch(InvalidOperationException) {
						//It stopped on its own.
					}
					process.Dispose();
				}
				DeleteFolder(folder);
			}
		}

		///<summary>The service check's own functions (dot-sourced) write the PFX file, appsettings.json and OpenDentalServerConfig.xml into the
		///folder, as the script writes them into its copy of the server folder. Returns the certificate's thumbprint and the server's address.
		///</summary>
		private static string[] WriteSettings(string script,string folder,int port,string database) {
			string driver=Path.Combine(folder,"driver.ps1");
			File.WriteAllText(driver,"$ErrorActionPreference='Stop'\r\n"
				+". '"+script.Replace("'","''")+"'\r\n"
				+"$out='"+folder.Replace("'","''")+"'\r\n"
				+"$pfx=Join-Path $out 'mtsvc-test.pfx'\r\n"
				+"$pw=New-PfxPassword\r\n"
				+"$c=New-TestCertificatePfx $pfx $pw\r\n"
				+"$utf8=New-Object System.Text.UTF8Encoding($false)\r\n"
				+"[IO.File]::WriteAllText((Join-Path $out 'appsettings.json'),(New-AppSettingsJson "+port+" $pfx $pw),$utf8)\r\n"
				+"[IO.File]::WriteAllText((Join-Path $out 'OpenDentalServerConfig.xml'),(New-ServerConfigXml '"+database+"'),$utf8)\r\n"
				+"\"THUMB=$($c.Thumbprint)\"\r\n"
				+"\"URL=$(Get-ServiceUrl "+port+")\"\r\n",Encoding.ASCII);
			ProcessStartInfo processStartInfo=new ProcessStartInfo(Path.Combine(Environment.SystemDirectory,"WindowsPowerShell","v1.0","powershell.exe"));
			processStartInfo.ArgumentList.Add("-NoProfile");
			processStartInfo.ArgumentList.Add("-NonInteractive");
			processStartInfo.ArgumentList.Add("-ExecutionPolicy");
			processStartInfo.ArgumentList.Add("Bypass");
			processStartInfo.ArgumentList.Add("-File");
			processStartInfo.ArgumentList.Add(driver);
			processStartInfo.UseShellExecute=false;
			processStartInfo.CreateNoWindow=true;
			processStartInfo.RedirectStandardOutput=true;
			processStartInfo.RedirectStandardError=true;
			using(Process process=Process.Start(processStartInfo)) {
				Task<string> taskOut=process.StandardOutput.ReadToEndAsync();
				Task<string> taskErr=process.StandardError.ReadToEndAsync();
				if(!process.WaitForExit(120000)) {
					process.Kill();
					Assert.Fail(SCRIPT+"'s functions did not finish in 2 minutes.");
				}
				process.WaitForExit();
				string output=taskOut.Result+taskErr.Result;
				Assert.AreEqual(0,process.ExitCode,output);
				string[] arrayLines=output.Split(new[] { "\r\n","\n" },StringSplitOptions.RemoveEmptyEntries);
				string thumbprint=arrayLines.FirstOrDefault(x => x.StartsWith("THUMB="))?.Substring(6);
				string url=arrayLines.FirstOrDefault(x => x.StartsWith("URL="))?.Substring(4);
				Assert.IsNotNull(thumbprint,output);
				Assert.IsNotNull(url,output);
				File.Delete(driver);
				return new[] { thumbprint,url };
			}
		}

		///<summary>The server as a console program without a window: from System32, as the service control manager starts a service, with the
		///folder of the settings as its content root (ServerSettingsTests: without the argument it is the exe's folder). Its event log output is
		///off, so the test writes nothing to the Application log; its console output goes to stringBuilderOutput.</summary>
		private static Process StartServer(string serverExe,string folder,StringBuilder stringBuilderOutput) {
			ProcessStartInfo processStartInfo=new ProcessStartInfo(serverExe);
			processStartInfo.ArgumentList.Add("--contentRoot");
			processStartInfo.ArgumentList.Add(folder);
			processStartInfo.WorkingDirectory=Environment.SystemDirectory;
			processStartInfo.UseShellExecute=false;
			processStartInfo.CreateNoWindow=true;
			processStartInfo.RedirectStandardOutput=true;
			processStartInfo.RedirectStandardError=true;
			processStartInfo.Environment["Logging__EventLog__LogLevel__Default"]="None";
			//Addresses from the environment would be added to the settings' one (ServerHost.GetHttpsProblems).
			processStartInfo.Environment.Remove("ASPNETCORE_URLS");
			processStartInfo.Environment.Remove("ASPNETCORE_HTTP_PORTS");
			processStartInfo.Environment.Remove("ASPNETCORE_HTTPS_PORTS");
			Process process=new Process();
			process.StartInfo=processStartInfo;
			DataReceivedEventHandler dataReceivedEventHandler=(sender,e) => {
				if(e.Data!=null) {
					lock(stringBuilderOutput) {
						stringBuilderOutput.AppendLine(e.Data);
					}
				}
			};
			process.OutputDataReceived+=dataReceivedEventHandler;
			process.ErrorDataReceived+=dataReceivedEventHandler;
			process.Start();
			process.BeginOutputReadLine();
			process.BeginErrorReadLine();
			return process;
		}

		private static void WaitUntilListening(Process process,int port,StringBuilder stringBuilderOutput) {
			DateTime dateTimeEnd=DateTime.UtcNow.AddSeconds(60);
			while(DateTime.UtcNow<dateTimeEnd) {
				if(process.HasExited) {
					process.WaitForExit();
					Assert.Fail("The built server stopped at its start (exit code "+process.ExitCode+"):\r\n"+GetText(stringBuilderOutput));
				}
				using(TcpClient tcpClient=new TcpClient()) {
					try {
						tcpClient.Connect("127.0.0.1",port);
						return;
					}
					catch(SocketException) {
						//Not listening yet.
					}
				}
				Thread.Sleep(250);
			}
			Assert.Fail("The built server does not listen on 127.0.0.1:"+port+" after 60 seconds:\r\n"+GetText(stringBuilderOutput));
		}

		///<summary>Waits up to 20 seconds for the text in the server's output (its console log writes on a thread of its own).</summary>
		private static void WaitForOutput(StringBuilder stringBuilderOutput,string text) {
			DateTime dateTimeEnd=DateTime.UtcNow.AddSeconds(20);
			while(!GetText(stringBuilderOutput).Contains(text)) {
				if(DateTime.UtcNow>dateTimeEnd) {
					Assert.Fail("Not in the server's output after 20 seconds: "+text+"\r\n"+GetText(stringBuilderOutput));
				}
				Thread.Sleep(100);
			}
		}

		private static string GetText(StringBuilder stringBuilderOutput) {
			lock(stringBuilderOutput) {
				return stringBuilderOutput.ToString();
			}
		}

		private static int GetFreePort() {
			TcpListener tcpListener=new TcpListener(System.Net.IPAddress.Loopback,0);
			tcpListener.Start();
			int port=((System.Net.IPEndPoint)tcpListener.LocalEndpoint).Port;
			tcpListener.Stop();
			return port;
		}

		private static void DeleteFolder(string folder) {
			for(int i=0;i<10;i++) {
				try {
					Directory.Delete(folder,true);
					return;
				}
				catch(IOException) {
				}
				catch(UnauthorizedAccessException) {
				}
				Thread.Sleep(500);
			}
		}

		///<summary>The client's handler for the test, and the one before it again afterwards.</summary>
		private sealed class HttpMessageHandlerScope:IDisposable {
			private readonly HttpMessageHandler _httpMessageHandlerBefore;

			public HttpMessageHandlerScope(HttpMessageHandler httpMessageHandler) {
				_httpMessageHandlerBefore=OpenDentalServerHttp.HttpMessageHandlerOverride;
				OpenDentalServerHttp.HttpMessageHandlerOverride=httpMessageHandler;
			}

			public void Dispose() {
				OpenDentalServerHttp.HttpMessageHandlerOverride=_httpMessageHandlerBefore;
			}
		}
	}
}
