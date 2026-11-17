// Tests for the owner's launch-check scripts of stage 3b (LAUNCH-3b, docs/launch-checks-3b.md): tools/Test-MiddleTierService.ps1 (the Middle
// Tier as a Windows service), tools/Test-CleanInstall.ps1 and tools/Test-CleanInstall-InSandbox.ps1 (the clean install in Windows Sandbox).
// Only their plain-code parts run here: each script is dot-sourced by a driver script and its functions are called (the .wsb XML, the results
// reader, the settings and certificate files written into a temp folder), -PlanOnly runs, and each script runs where it refuses before any
// change (unelevated, outside the sandbox). Nothing is elevated, no service is installed, Windows Sandbox is never started, no certificate store
// is opened and no database is used. Plus the checks the repository makes of the owner's scripts: ASCII only, no secret, its hosts.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalBusiness;

namespace ReleaseFlowTests {
	[TestClass]
	public class LaunchCheckScriptTests {
		private const string SCRIPT_SERVICE="Test-MiddleTierService.ps1";
		private const string SCRIPT_CLEAN="Test-CleanInstall.ps1";
		private const string SCRIPT_SANDBOX="Test-CleanInstall-InSandbox.ps1";
		///<summary>The scripts the owner runs, never an agent: the installer test of S3b-7 and the two of LAUNCH-3b with the in-sandbox part.</summary>
		private static readonly string[] _arrayOwnerScripts=new[] { "Test-Installer.ps1",SCRIPT_SERVICE,SCRIPT_CLEAN,SCRIPT_SANDBOX };

		#region Helpers
		private class ScriptResult {
			public int ExitCode;
			public string Output;
			public List<string> Lines;
		}

		private static string ScriptPath(string name) {
			return Path.Combine(TestPaths.RepoRoot,"tools",name);
		}

		private static string Quote(string arg) {
			return "\""+arg+"\"";
		}

		///<summary>Runs a script with the 64-bit Windows PowerShell (the test host is x64), arguments as given.</summary>
		private static ScriptResult RunScript(string script,params string[] arrayArgs) {
			ProcessStartInfo processStartInfo=new ProcessStartInfo("powershell.exe");
			processStartInfo.Arguments="-NoProfile -NonInteractive -ExecutionPolicy Bypass -File "+Quote(script)+" "+string.Join(" ",arrayArgs);
			processStartInfo.UseShellExecute=false;
			processStartInfo.RedirectStandardOutput=true;
			processStartInfo.RedirectStandardError=true;
			processStartInfo.CreateNoWindow=true;
			using(Process process=Process.Start(processStartInfo)) {
				Task<string> taskOut=process.StandardOutput.ReadToEndAsync();
				Task<string> taskErr=process.StandardError.ReadToEndAsync();
				if(!process.WaitForExit(120000)) {
					process.Kill();
					Assert.Fail(Path.GetFileName(script)+" did not finish in 2 minutes.");
				}
				process.WaitForExit();
				ScriptResult scriptResult=new ScriptResult();
				scriptResult.ExitCode=process.ExitCode;
				scriptResult.Output=taskOut.Result+taskErr.Result;
				scriptResult.Lines=scriptResult.Output.Split(new[] { "\r\n","\n" },StringSplitOptions.RemoveEmptyEntries).Select(x => x.TrimEnd()).ToList();
				return scriptResult;
			}
		}

		///<summary>Writes the driver text (ASCII) into the folder and runs it; the driver dot-sources a script and calls its functions.</summary>
		private static ScriptResult RunDriver(string folder,string driverText) {
			string driver=Path.Combine(folder,"driver-"+Guid.NewGuid().ToString("N").Substring(0,8)+".ps1");
			File.WriteAllText(driver,driverText,Encoding.ASCII);
			try {
				ScriptResult scriptResult=RunScript(driver);
				Assert.AreEqual(0,scriptResult.ExitCode,scriptResult.Output);
				return scriptResult;
			}
			finally {
				File.Delete(driver);
			}
		}

		///<summary>The value of the driver's output line "KEY=value".</summary>
		private static string Value(ScriptResult scriptResult,string key) {
			string line=scriptResult.Lines.FirstOrDefault(x => x.StartsWith(key+"="));
			Assert.IsNotNull(line,"No line "+key+"= in: "+scriptResult.Output);
			return line.Substring(key.Length+1);
		}

		private static void DeleteFolder(string folder) {
			try {
				Directory.Delete(folder,true);
			}
			catch(IOException) { }
			catch(UnauthorizedAccessException) { }
		}

		private static bool IsElevated() {
			return new System.Security.Principal.WindowsPrincipal(System.Security.Principal.WindowsIdentity.GetCurrent())
				.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
		}

		///<summary>The script's top-level statements as PowerShell's parser sees them: kind, first line number and first line of text.</summary>
		private static List<string[]> TopLevelStatements(string folder,string script) {
			ScriptResult scriptResult=RunDriver(folder,"$ErrorActionPreference='Stop'\r\n$tokens=$null\r\n$errors=$null\r\n"
				+"$ast=[System.Management.Automation.Language.Parser]::ParseFile('"+script+"',[ref]$tokens,[ref]$errors)\r\n"
				+"\"ERRORS=$($errors.Count)\"\r\n"
				+"foreach($s in $ast.EndBlock.Statements){ \"STMT|$($s.GetType().Name)|$($s.Extent.StartLineNumber)|$(($s.Extent.Text -split \"`r?`n\")[0])\" }\r\n");
			Assert.AreEqual("0",Value(scriptResult,"ERRORS"),Path.GetFileName(script)+" parses without errors: "+scriptResult.Output);
			return scriptResult.Lines.Where(x => x.StartsWith("STMT|")).Select(x => x.Split(new[] { '|' },4).Skip(1).ToArray()).ToList();
		}

		///<summary>The words of a statement that change something on the PC.</summary>
		private static readonly string[] _arrayChanges=new[] { "New-Item","Remove-Item","Copy-Item","Start-Process","sc.exe","Invoke-Sc","CREATE DATABASE","DROP DATABASE",
			"WriteAll","AppendAll","Set-Item","Add-Content","Out-File","reg.exe","shutdown","X509Store","Stop-Process" };

		///<summary>Before the statement that holds gate, the script only defines functions, assigns, reads and refuses: no statement outside a
		///function changes anything, and the only top-level if before it is the dot-source guard or one that reads.</summary>
		private static void AssertNothingChangesBefore(string folder,string name,string gate) {
			string script=ScriptPath(name);
			string text=File.ReadAllText(script);
			List<string[]> listStatements=TopLevelStatements(folder,script);
			int indexGate=listStatements.FindIndex(x => x[2].Contains(gate));
			Assert.IsTrue(indexGate>0,name+": the gate '"+gate+"' is a top-level statement.");
			Assert.IsTrue(listStatements.Take(indexGate).Any(x => x[0]=="IfStatementAst" && x[2].StartsWith("if($MyInvocation.InvocationName -eq '.')")),
				name+": the dot-source guard comes before the gate, so a test that dot-sources the script runs nothing of it.");
			string[] arrayLines=text.Replace("\r\n","\n").Split('\n');
			for(int i=0;i<indexGate;i++) {
				if(listStatements[i][0]=="FunctionDefinitionAst") {
					continue;
				}
				int lineStart=int.Parse(listStatements[i][1]);
				int lineEnd=int.Parse(listStatements[i+1][1]);
				string statement=string.Join("\n",arrayLines.Skip(lineStart-1).Take(lineEnd-lineStart));
				foreach(string change in _arrayChanges) {
					Assert.IsFalse(statement.Contains(change),name+": nothing before the gate changes anything ("+change+" at line "+lineStart+")");
				}
			}
		}
		#endregion Helpers

		#region The owner's scripts: ASCII, no secret, the command in the header
		///<summary>Every script the owner runs is ASCII (Windows PowerShell 5.1 reads a file without a byte order mark in the ANSI code page) and
		///holds no secret: no certificate thumbprint, no password or key value, no address but the loopback one and the documentation range, and
		///no host but example.test names and localhost.</summary>
		[TestMethod]
		public void OwnerScripts_AreAscii_AndHoldNoSecret() {
			foreach(string name in _arrayOwnerScripts) {
				byte[] arrayBytes=File.ReadAllBytes(ScriptPath(name));
				Assert.IsFalse(arrayBytes.Any(x => x>=0x80),name+": ASCII only.");
				string text=Encoding.ASCII.GetString(arrayBytes);
				Assert.IsFalse(Regex.IsMatch(text,@"\b[0-9A-Fa-f]{40}\b"),name+": no certificate thumbprint.");
				Assert.IsFalse(Regex.IsMatch(text,@"(?i)(password|pwd|secret|apikey|api_key)\s*[=:]\s*['""][^'""\s$]+['""]"),name+": no password or key value.");
				Assert.IsFalse(Regex.IsMatch(text,@"(?i)--password"),name+": no password on a database client's command line.");
				Assert.IsFalse(Regex.IsMatch(text,@"(?i)BEGIN (RSA |EC )?PRIVATE KEY"),name+": no key.");
				//An address: four numbers not inside a longer dotted run (an OID) and not a version ([version]'2.5.1.0').
				foreach(Match match in Regex.Matches(text,@"(?<![\d.])\d{1,3}(\.\d{1,3}){3}(?![\d.])")) {
					if(text.Substring(Math.Max(0,match.Index-20),Math.Min(20,match.Index)).IndexOf("version",StringComparison.OrdinalIgnoreCase)>=0) {
						continue;
					}
					Assert.IsTrue(match.Value=="127.0.0.1" || match.Value.StartsWith("192.0.2."),name+": no address but 127.0.0.1 and 192.0.2.x ("+match.Value+").");
				}
				//A host name in lower case (the .NET names, System.Net and Microsoft.NET, are not).
				foreach(Match match in Regex.Matches(text,@"\b[a-z0-9-]+(\.[a-z0-9-]+)*\.(com|net|org|io|ms|cloud|local|lan|corp)\b")) {
					Assert.Fail(name+": no host name but example.test ones and localhost ("+match.Value+").");
				}
				if(name==SCRIPT_SANDBOX) {
					StringAssert.Contains(text,"Do not run it yourself");
					StringAssert.Contains(text,"It refuses to run anywhere else.");
				}
				else {
					StringAssert.Contains(text,"The owner runs it, never an agent",name);
				}
			}
		}

		///<summary>The header of each of the two scripts gives the owner's one line on a line of its own, and docs/launch-checks-3b.md gives the same
		///line.</summary>
		[TestMethod]
		public void LaunchScripts_HeaderGivesTheOwnersCommand_AndTheChecklistTheSame() {
			string checklist=TestPaths.ReadSource("docs","launch-checks-3b.md");
			Dictionary<string,string> dictCommands=new Dictionary<string,string> {
				{ SCRIPT_SERVICE,@"powershell -NoProfile -ExecutionPolicy Bypass -File ""<repo>\tools\Test-MiddleTierService.ps1""" },
				{ SCRIPT_CLEAN,@"powershell -NoProfile -ExecutionPolicy Bypass -File ""<repo>\tools\Test-CleanInstall.ps1"" -Setup ""<dryrun>\OpenPmsDental-<version>-Setup.exe"" -Staging ""<dryrun>\staging""" },
			};
			foreach(KeyValuePair<string,string> pair in dictCommands) {
				string script=File.ReadAllText(ScriptPath(pair.Key));
				string header=script.Substring(0,script.IndexOf("#>",StringComparison.Ordinal)).Replace("\r\n","\n");
				StringAssert.Contains(header,"\n\n  "+pair.Value+"\n\n",pair.Key+": the one line, on a line of its own.");
				StringAssert.Contains(header,"WHAT IT CHANGES ON THIS PC",pair.Key);
				StringAssert.Contains(header,@"docs\launch-checks-3b.md",pair.Key);
				StringAssert.Contains(checklist,pair.Value,"docs/launch-checks-3b.md gives "+pair.Key+"'s line.");
			}
			StringAssert.Contains(File.ReadAllText(ScriptPath(SCRIPT_SERVICE)),"\"Run as administrator\"");
			StringAssert.Contains(File.ReadAllText(ScriptPath(SCRIPT_CLEAN)),"Enable-WindowsOptionalFeature -Online -FeatureName Containers-DisposableClientVM -All");
			StringAssert.Contains(checklist,"Enable-WindowsOptionalFeature -Online -FeatureName Containers-DisposableClientVM -All");
		}
		#endregion

		#region tools/Test-MiddleTierService.ps1
		///<summary>Before the elevation check the script only defines functions and values; unelevated it ends with exit code 2 and changes nothing.
		///Run only unelevated: an elevated test runner would let the script go on, so the run is then left out (the script is never run elevated
		///here).</summary>
		[TestMethod]
		public void MiddleTierService_RefusesUnelevated_AndChangesNothing() {
			string folder=TestPaths.NewTempFolder("launch3b_svc_refuse");
			try {
				AssertNothingChangesBefore(folder,SCRIPT_SERVICE,"$isElevated=$principal.IsInRole(");
				string text=File.ReadAllText(ScriptPath(SCRIPT_SERVICE));
				//The first change comes after every refusal and after -PlanOnly's exit.
				int indexFirstChange=text.IndexOf("New-Item -ItemType Directory -Force -Path $OutDir",StringComparison.Ordinal);
				Assert.IsTrue(indexFirstChange>0);
				foreach(string stop in new[] { "exit 2","if($PlanOnly){","Write-Host 'Exit code 3'","A service named $ServiceName exists already","The server is not built",
					"is not the self-contained x64 build","The host tests are not built","No MariaDB client","does not answer as $DbUser","The database $Database exists already" })
				{
					int index=text.IndexOf(stop,StringComparison.Ordinal);
					Assert.IsTrue(index>0 && index<indexFirstChange,"Before the first change: "+stop);
				}
				if(IsElevated()) {
					Assert.Inconclusive("The test runner is elevated: the script is not run here (it would go on to install the service).");
				}
				string outDir=Path.Combine(folder,"out");
				ScriptResult scriptResult=RunScript(ScriptPath(SCRIPT_SERVICE),"-ServerFolder",Quote(Path.Combine(folder,"none")),"-OutDir",Quote(outDir));
				Assert.AreEqual(2,scriptResult.ExitCode,scriptResult.Output);
				StringAssert.Contains(scriptResult.Output,"needs an elevated PowerShell");
				StringAssert.Contains(scriptResult.Output,"Nothing was changed.");
				CollectionAssert.Contains(scriptResult.Lines,"Exit code 2");
				Assert.IsFalse(Directory.Exists(outDir),"No log folder.");
				Assert.AreEqual(0,Directory.GetFileSystemEntries(folder).Length,"Nothing written.");
			}
			finally {
				DeleteFolder(folder);
			}
		}

		///<summary>-PlanOnly prints every step with the names a run would use, says what a real run would refuse, and makes nothing: no log folder, no
		///temp folder.</summary>
		[TestMethod]
		public void MiddleTierService_PlanOnly_PrintsEveryStep_AndChangesNothing() {
			string folder=TestPaths.NewTempFolder("launch3b_svc_plan");
			try {
				string outDir=Path.Combine(folder,"out");
				foreach(bool withClientSession in new[] { false,true }) {
					List<string> listArgs=new List<string> { "-PlanOnly","-ServerFolder",Quote(Path.Combine(folder,"none")),"-OutDir",Quote(outDir),"-Port","7443" };
					if(withClientSession) {
						listArgs.Add("-ClientSession");
					}
					ScriptResult scriptResult=RunScript(ScriptPath(SCRIPT_SERVICE),listArgs.ToArray());
					Assert.AreEqual(0,scriptResult.ExitCode,scriptResult.Output);
					List<string> listPlan=scriptResult.Lines.Where(x => x.StartsWith("PLAN ")).ToList();
					Assert.AreEqual(withClientSession ? 12 : 10,listPlan.Count,scriptResult.Output);
					string plan=string.Join("\n",listPlan);
					StringAssert.Contains(plan,"sc.exe create OpenPmsDentalServer-Test binPath= ");
					StringAssert.Contains(plan,"https://127.0.0.1:7443/OpenDentalServer/ServiceMain.asmx answers 405 with Allow: POST");
					StringAssert.Contains(plan,"POST with Content-Length 1073741825 and no body: 413");
					StringAssert.Contains(plan,"-Include MiddleTierHostTests with MIDDLETIER_TEST_URL, MIDDLETIER_TEST_DB=mtsvc_");
					StringAssert.Contains(plan,"(no certificate store)");
					StringAssert.Contains(plan,"sc.exe stop and sc.exe delete OpenPmsDentalServer-Test");
					Assert.AreEqual(withClientSession,plan.Contains(@"Cert:\LocalMachine\Root"),"The Trusted Root step only with -ClientSession.");
					Match matchDatabase=Regex.Match(scriptResult.Output,@"Database: (mtsvc_\d{12}_\d{4}) on 127\.0\.0\.1:3306");
					Assert.IsTrue(matchDatabase.Success,scriptResult.Output);
					StringAssert.Contains(scriptResult.Output,"A real run would refuse: The server is not built");
					CollectionAssert.Contains(scriptResult.Lines,"Nothing was changed.");
					CollectionAssert.Contains(scriptResult.Lines,"Exit code 0");
					Match matchWork=Regex.Match(plan,@"into the new temp folder (\S+)");
					Assert.IsTrue(matchWork.Success);
					Assert.IsFalse(Directory.Exists(matchWork.Groups[1].Value),"No temp folder: "+matchWork.Groups[1].Value);
					Assert.IsFalse(Directory.Exists(outDir),"No log folder.");
				}
			}
			finally {
				DeleteFolder(folder);
			}
		}

		///<summary>The plain parts, dot-sourced: the database name, the PFX file (made in memory, written to the folder given, no store), the
		///server's appsettings.json and OpenDentalServerConfig.xml, the address, the raw requests and the reading of the test run.</summary>
		[TestMethod]
		public void MiddleTierService_PlainParts_WriteTheFilesTheServerReads() {
			string folder=TestPaths.NewTempFolder("launch3b_svc_parts");
			try {
				File.WriteAllText(Path.Combine(folder,"run-pass.txt"),"\uFEFFRUN mode=Targeted root=x started=2026-11-17T10:00:00 assemblies=2 vstest=x\r\n"
					+"MiddleTierHostTests exit=0 total=19 passed=16 failed=0 skipped=3 seconds=30\r\nNoHqTests exit=0 total=115 passed=115 failed=0 skipped=0 seconds=50\r\n"
					+"DONE ok=True finished=2026-11-17T10:01:00\r\n");
				File.WriteAllText(Path.Combine(folder,"run-fail.txt"),"MiddleTierHostTests exit=1 total=19 passed=15 failed=1 skipped=3 seconds=30\r\n"
					+"NoHqTests exit=0 total=115 passed=115 failed=0 skipped=0 seconds=50\r\nDONE ok=False finished=x\r\n");
				File.WriteAllText(Path.Combine(folder,"run-nodone.txt"),"MiddleTierHostTests exit=0 total=19 passed=16 failed=0 skipped=3 seconds=30\r\n");
				File.WriteAllText(Path.Combine(folder,"run-few.txt"),"MiddleTierHostTests exit=0 total=19 passed=4 failed=0 skipped=15 seconds=30\r\n"
					+"NoHqTests exit=0 total=115 passed=115 failed=0 skipped=0 seconds=50\r\nDONE ok=True finished=x\r\n");
				string pfxPathInSettings=@"C:\Temp\a & b\mtsvc-test.pfx";
				ScriptResult scriptResult=RunDriver(folder,"$ErrorActionPreference='Stop'\r\n"
					+". '"+ScriptPath(SCRIPT_SERVICE)+"'\r\n"
					+"$out='"+folder+"'\r\n"
					+"\"DB=$(New-DatabaseName (Get-Date))\"\r\n"
					+"foreach($n in @('mtsvc_261117000000_1234','opendental_dev','s3b6_gate','mtsvc_A','mtsvc_a;drop',('mtsvc_'+('a'*60)))){ \"NAME $n=$(Test-DatabaseName $n)\" }\r\n"
					+"$pw=New-PfxPassword\r\n"
					+"\"PW=$pw\"\r\n"
					+"$c=New-TestCertificatePfx (Join-Path $out 'test.pfx') $pw\r\n"
					+"\"THUMB=$($c.Thumbprint)\"\r\n"
					+"\"CER=$([Convert]::ToBase64String($c.Cer))\"\r\n"
					+"[IO.File]::WriteAllText((Join-Path $out 'appsettings.json'),(New-AppSettingsJson 7443 '"+pfxPathInSettings+"' $pw))\r\n"
					+"[IO.File]::WriteAllText((Join-Path $out 'config.xml'),(New-ServerConfigXml 'mtsvc_261117000000_1234'))\r\n"
					+"\"URL=$(Get-ServiceUrl 7443)\"\r\n"
					+"[IO.File]::WriteAllText((Join-Path $out 'get.txt'),(New-RawRequest 'GET' 7443 0))\r\n"
					+"[IO.File]::WriteAllText((Join-Path $out 'post.txt'),(New-RawRequest 'POST' 7443 1073741825))\r\n"
					+"\"STATUS1=$(Get-StatusCode \"HTTP/1.1 405 Method Not Allowed`r`nAllow: POST`r`n`r`n\")\"\r\n"
					+"\"STATUS2=$(Get-StatusCode 'garbage')\"\r\n"
					+"foreach($case in 'pass','fail','nodone','few'){ $r=Test-HostTestRun @(Get-Content (Join-Path $out \"run-$case.txt\")); \"RUN_$case=$($r.Done)|$($r.HostOk)|$($r.NoHqOk)\" }\r\n");
				//The database name: mtsvc_, the time, a number; only such names are created and dropped.
				Assert.IsTrue(Regex.IsMatch(Value(scriptResult,"DB"),@"^mtsvc_\d{12}_\d{4}$"),Value(scriptResult,"DB"));
				CollectionAssert.IsSubsetOf(new[] { "NAME mtsvc_261117000000_1234=True","NAME opendental_dev=False","NAME s3b6_gate=False","NAME mtsvc_A=False",
					"NAME mtsvc_a;drop=False","NAME mtsvc_"+new string('a',60)+"=False" },scriptResult.Lines);
				//The certificate: a PFX file in the folder given, with its key, for 127.0.0.1 and localhost, server authentication, not a CA, two days.
				string password=Value(scriptResult,"PW");
				Assert.AreEqual(32,password.Length,"24 random bytes in base64.");
				string thumbprint=Value(scriptResult,"THUMB");
				Assert.IsTrue(Regex.IsMatch(thumbprint,"^[0-9A-F]{40}$"),thumbprint);
				using(X509Certificate2 certificate=X509CertificateLoader.LoadPkcs12FromFile(Path.Combine(folder,"test.pfx"),password,X509KeyStorageFlags.EphemeralKeySet)) {
					Assert.AreEqual(thumbprint,certificate.Thumbprint);
					Assert.IsTrue(certificate.HasPrivateKey);
					Assert.AreEqual("CN=OpenPmsDentalServer-Test launch check",certificate.Subject);
					X509SubjectAlternativeNameExtension san=certificate.Extensions.OfType<X509SubjectAlternativeNameExtension>().Single();
					CollectionAssert.AreEqual(new[] { "127.0.0.1" },san.EnumerateIPAddresses().Select(x => x.ToString()).ToArray());
					CollectionAssert.AreEqual(new[] { "localhost" },san.EnumerateDnsNames().ToArray());
					X509EnhancedKeyUsageExtension eku=certificate.Extensions.OfType<X509EnhancedKeyUsageExtension>().Single();
					CollectionAssert.AreEqual(new[] { "1.3.6.1.5.5.7.3.1" },eku.EnhancedKeyUsages.Cast<System.Security.Cryptography.Oid>().Select(x => x.Value).ToArray());
					Assert.IsFalse(certificate.Extensions.OfType<X509BasicConstraintsExtension>().Single().CertificateAuthority);
					Assert.IsTrue(certificate.NotAfter-certificate.NotBefore<=TimeSpan.FromDays(3));
					Assert.IsTrue(certificate.NotAfter>DateTime.Now.AddDays(1));
					CollectionAssert.AreEqual(certificate.RawData,Convert.FromBase64String(Value(scriptResult,"CER")),"The public part for -ClientSession.");
				}
				//No certificate store in the function that makes it.
				string script=File.ReadAllText(ScriptPath(SCRIPT_SERVICE));
				string function=Regex.Match(script,@"(?s)function New-TestCertificatePfx.*?\n}\r?\n").Value;
				Assert.IsTrue(function.Length>100);
				foreach(string store in new[] { "X509Store","Cert:\\","New-SelfSignedCertificate","Import-PfxCertificate","certutil","PersistKeySet" }) {
					Assert.IsFalse(function.Contains(store),"New-TestCertificatePfx uses no store: "+store);
				}
				//appsettings.json: one endpoint on 127.0.0.1 with the PFX file (no store), the Middle Tier settings as shipped.
				using(JsonDocument jsonDocument=JsonDocument.Parse(File.ReadAllText(Path.Combine(folder,"appsettings.json"))))
				using(JsonDocument jsonDocumentShipped=JsonDocument.Parse(TestPaths.ReadSource("OpenPmsDentalServer","appsettings.json")))
				{
					JsonElement https=jsonDocument.RootElement.GetProperty("Kestrel").GetProperty("Endpoints").GetProperty("Https");
					Assert.AreEqual(1,jsonDocument.RootElement.GetProperty("Kestrel").GetProperty("Endpoints").EnumerateObject().Count(),"One endpoint.");
					Assert.AreEqual("https://127.0.0.1:7443",https.GetProperty("Url").GetString());
					JsonElement certificateSettings=https.GetProperty("Certificate");
					CollectionAssert.AreEquivalent(new[] { "Path","Password" },certificateSettings.EnumerateObject().Select(x => x.Name).ToArray(),"A file, no store.");
					Assert.AreEqual(pfxPathInSettings,certificateSettings.GetProperty("Path").GetString());
					Assert.AreEqual(password,certificateSettings.GetProperty("Password").GetString());
					JsonElement middleTier=jsonDocument.RootElement.GetProperty("MiddleTier");
					JsonElement middleTierShipped=jsonDocumentShipped.RootElement.GetProperty("MiddleTier");
					Assert.AreEqual(middleTierShipped.GetProperty("PathBase").GetString(),middleTier.GetProperty("PathBase").GetString());
					Assert.AreEqual(middleTierShipped.GetProperty("MaxRequestBodySize").GetInt64(),middleTier.GetProperty("MaxRequestBodySize").GetInt64());
					Assert.AreEqual(1073741824L,middleTier.GetProperty("MaxRequestBodySize").GetInt64(),"The limit the 413 request is one byte above.");
				}
				//OpenDentalServerConfig.xml: the sample's elements, the scratch database on 127.0.0.1 as root with a blank password.
				XmlDocument xmlDocument=new XmlDocument();
				xmlDocument.Load(Path.Combine(folder,"config.xml"));
				XmlNode connection=xmlDocument.SelectSingleNode("/ConnectionSettings/DatabaseConnection");
				Assert.AreEqual("127.0.0.1",connection["ComputerName"].InnerText);
				Assert.AreEqual("mtsvc_261117000000_1234",connection["Database"].InnerText);
				Assert.AreEqual("root",connection["User"].InnerText);
				Assert.AreEqual("",connection["Password"].InnerText);
				Assert.AreEqual("MySql",connection["DatabaseType"].InnerText);
				Assert.AreEqual("/OpenDentalServer",connection["ApplicationName"].InnerText);
				XmlDocument xmlDocumentSample=new XmlDocument();
				xmlDocumentSample.LoadXml(TestPaths.ReadSource("OpenPmsDentalServer","OpenDentalServerConfig.xml"));
				foreach(XmlNode xmlNode in connection.ChildNodes) {
					Assert.IsNotNull(xmlDocumentSample.SelectSingleNode("/ConnectionSettings/DatabaseConnection/"+xmlNode.Name),"The sample has "+xmlNode.Name);
				}
				//The address, the two requests and their answers.
				Assert.AreEqual("https://127.0.0.1:7443/OpenDentalServer/ServiceMain.asmx",Value(scriptResult,"URL"));
				Assert.AreEqual("GET /OpenDentalServer/ServiceMain.asmx HTTP/1.1\r\nHost: 127.0.0.1:7443\r\nConnection: close\r\n\r\n",File.ReadAllText(Path.Combine(folder,"get.txt")));
				Assert.AreEqual("POST /OpenDentalServer/ServiceMain.asmx HTTP/1.1\r\nHost: 127.0.0.1:7443\r\nContent-Type: text/plain; charset=utf-8\r\n"
					+"Content-Length: 1073741825\r\nConnection: close\r\n\r\n",File.ReadAllText(Path.Combine(folder,"post.txt")));
				Assert.AreEqual("405",Value(scriptResult,"STATUS1"));
				Assert.AreEqual("0",Value(scriptResult,"STATUS2"));
				//The test run: done, the host tests without failure and at least 16 passed, NoHqTests without failure.
				Assert.AreEqual("True|True|True",Value(scriptResult,"RUN_pass"));
				Assert.AreEqual("True|False|True",Value(scriptResult,"RUN_fail"));
				Assert.AreEqual("False|True|False",Value(scriptResult,"RUN_nodone"));
				Assert.AreEqual("True|False|True",Value(scriptResult,"RUN_few"));
			}
			finally {
				DeleteFolder(folder);
			}
		}

		///<summary>The script talks to no database server but 127.0.0.1:3306 (no option file decides it), touches only its own service and database,
		///and undoes every change in its finally block, also after a failure.</summary>
		[TestMethod]
		public void MiddleTierService_TouchesOnlyItsOwn_AndUndoesEverythingInFinally() {
			string script=File.ReadAllText(ScriptPath(SCRIPT_SERVICE));
			Assert.IsTrue(Regex.IsMatch(script,@"(?m)^\$DbHost='127\.0\.0\.1'\r?$"),"The one database host.");
			StringAssert.Contains(script,"$DbPort=3306");
			StringAssert.Contains(script,"& $script:DbClient '--no-defaults' \"--host=$DbHost\" \"--port=$DbPort\" '--protocol=TCP' \"--user=$DbUser\"");
			Assert.AreEqual(1,Regex.Matches(script,@"& \$script:DbClient").Count,"One place runs the database client.");
			Assert.AreEqual("OpenPmsDentalServer-Test",Regex.Match(script,@"(?m)^\$ServiceName='([^']+)'").Groups[1].Value);
			string[] arrayLines=script.Replace("\r\n","\n").Split('\n');
			List<string> listSc=arrayLines.Where(x => x.Contains("Invoke-Sc ") && !x.TrimStart().StartsWith("function ")).ToList();
			Assert.AreEqual(4,listSc.Count,"create, start, stop and delete: "+string.Join(" | ",listSc));
			foreach(string line in listSc) {
				StringAssert.Contains(line,"$ServiceName","sc.exe only for the test service: "+line);
			}
			List<string> listDatabase=arrayLines.Where(x => x.Contains("Invoke-Sql") && (x.Contains("CREATE DATABASE") || x.Contains("DROP DATABASE"))).ToList();
			Assert.AreEqual(2,listDatabase.Count,string.Join(" | ",listDatabase));
			foreach(string line in listDatabase) {
				StringAssert.Contains(line,"``$Database``","Only the run's database: "+line);
			}
			Assert.AreEqual("mtsvc_",Regex.Match(script,@"(?m)^\$DbPrefix='([^']+)'").Groups[1].Value);
			//MiddleTierHostTests create and drop that prefix too (MIDDLETIER_TEST_DB).
			StringAssert.Contains(TestPaths.ReadSource("tests","MiddleTierHostTests","HostDb.cs"),"public const string PREFIX_SERVICE_CHECK=\"mtsvc_\";");
			string blockFinally=script.Substring(script.LastIndexOf("\nfinally {",StringComparison.Ordinal));
			foreach(string undo in new[] { "Invoke-Sc \"stop $ServiceName\"","Invoke-Sc \"delete $ServiceName\"","Remove-Item -LiteralPath $WorkFolder -Recurse -Force",
				"DROP DATABASE IF EXISTS ``$Database``","if(-not $eventSourceBefore -and (Test-Path $EventLogSourceKey))","if($rootAdded){","$store.Remove($found)",
				"Remove-Item -Path \"Env:\\$name\"","if($databaseReserved){","if($serviceCreated){" })
			{
				StringAssert.Contains(blockFinally,undo,"The finally block undoes: "+undo);
			}
			StringAssert.Contains(script,"Write-Host (\"{0} checks passed, {1} failed. Logs: {2}\" -f $script:Passes,$script:Failures.Count,$OutDir)");
			//The 64-bit view and elevation are refused before anything, as Test-Installer refuses them.
			StringAssert.Contains(script,"if(-not [Environment]::Is64BitProcess){");
			//The tests run in external mode with the S3b-6 variables, and the database is kept for the client session and dropped by the script.
			foreach(string variable in new[] { "$env:MIDDLETIER_TEST_URL=","$env:MIDDLETIER_TEST_DB=$Database","$env:MIDDLETIER_TEST_CERT_THUMBPRINT=$thumbprint","$env:S3B6_KEEP_DB='1'" }) {
				StringAssert.Contains(script,variable);
			}
			string hostTestRun=TestPaths.ReadSource("tests","MiddleTierHostTests","HostTestRun.cs");
			foreach(string variable in new[] { "MIDDLETIER_TEST_URL","MIDDLETIER_TEST_DB","MIDDLETIER_TEST_CERT_THUMBPRINT","S3B6_KEEP_DB" }) {
				StringAssert.Contains(hostTestRun,"=\""+variable+"\";","MiddleTierHostTests reads "+variable);
			}
		}
		#endregion tools/Test-MiddleTierService.ps1

		#region tools/Test-CleanInstall.ps1 and tools/Test-CleanInstall-InSandbox.ps1
		///<summary>The in-sandbox script refuses on any computer but the sandbox, before its first step, changing nothing: as this test's user, and as
		///the copy the host script puts beside host.txt with this computer's name (the copy in the host's temp folder never runs on the host).
		///</summary>
		[TestMethod]
		public void InSandbox_RefusesOutsideTheSandbox_AndChangesNothing() {
			Assert.AreNotEqual("WDAGUtilityAccount",Environment.UserName,"Tests never run as the sandbox's user.");
			string folder=TestPaths.NewTempFolder("launch3b_sandbox_refuse");
			try {
				AssertNothingChangesBefore(folder,SCRIPT_SANDBOX,"if(-not $isSandbox){");
				string folderIn=Path.Combine(folder,"in");
				Directory.CreateDirectory(folderIn);
				File.Copy(ScriptPath(SCRIPT_SANDBOX),Path.Combine(folderIn,SCRIPT_SANDBOX));
				File.WriteAllText(Path.Combine(folderIn,"host.txt"),Environment.MachineName,Encoding.ASCII);
				string results=Path.Combine(folder,"results");
				foreach(string script in new[] { ScriptPath(SCRIPT_SANDBOX),Path.Combine(folderIn,SCRIPT_SANDBOX) }) {
					ScriptResult scriptResult=RunScript(script,"-SetupFile",Quote(Path.Combine(folder,"none","OpenPmsDental-0.0-Setup.exe")),"-StagingFolder",
						Quote(Path.Combine(folder,"none","staging")),"-ResultsFolder",Quote(results));
					Assert.AreEqual(3,scriptResult.ExitCode,scriptResult.Output);
					StringAssert.Contains(scriptResult.Output,"runs only inside Windows Sandbox");
					StringAssert.Contains(scriptResult.Output,"Nothing was changed.");
					Assert.IsFalse(Directory.Exists(results),"No results folder.");
				}
				CollectionAssert.AreEquivalent(new[] { SCRIPT_SANDBOX,"host.txt" },Directory.GetFiles(folderIn).Select(x => Path.GetFileName(x)).ToArray());
				CollectionAssert.AreEquivalent(new[] { "in" },Directory.GetFileSystemEntries(folder).Select(x => Path.GetFileName(x)).ToArray());
			}
			finally {
				DeleteFolder(folder);
			}
		}

		///<summary>The host script, too, only defines functions and values before its 64-bit check; -PlanOnly prints its six steps and the .wsb it
		///would write (well-formed), says whether Windows Sandbox is on, and makes nothing.</summary>
		[TestMethod]
		public void CleanInstall_PlanOnly_PrintsTheStepsAndTheWsb_AndChangesNothing() {
			string folder=TestPaths.NewTempFolder("launch3b_clean_plan");
			try {
				AssertNothingChangesBefore(folder,SCRIPT_CLEAN,"if(-not [Environment]::Is64BitProcess){");
				string outDir=Path.Combine(folder,"results");
				ScriptResult scriptResult=RunScript(ScriptPath(SCRIPT_CLEAN),"-PlanOnly","-Setup",Quote(Path.Combine(folder,"dryrun","OpenPmsDental-0.0-Setup.exe")),"-Staging",
					Quote(Path.Combine(folder,"dryrun","staging")),"-OutDir",Quote(outDir));
				Assert.AreEqual(0,scriptResult.ExitCode,scriptResult.Output);
				List<string> listPlan=scriptResult.Lines.Where(x => x.StartsWith("PLAN ")).ToList();
				Assert.AreEqual(6,listPlan.Count,scriptResult.Output);
				Assert.IsTrue(scriptResult.Lines.Any(x => Regex.IsMatch(x,@"^Windows Sandbox: (on|off)")),scriptResult.Output);
				StringAssert.Contains(scriptResult.Output,"/VERYSILENT /SUPPRESSMSGBOXES /PREREQS=download /DBSERVER=localhost");
				StringAssert.Contains(scriptResult.Output,"A real run would refuse: No setup found");
				CollectionAssert.Contains(scriptResult.Lines,"Nothing was changed.");
				CollectionAssert.Contains(scriptResult.Lines,"Exit code 0");
				int indexStart=scriptResult.Lines.IndexOf("The .wsb file it would write:");
				int indexEnd=scriptResult.Lines.FindIndex(indexStart,x => x=="</Configuration>");
				Assert.IsTrue(indexStart>0 && indexEnd>indexStart,scriptResult.Output);
				XmlDocument xmlDocument=new XmlDocument();
				xmlDocument.LoadXml(string.Join("\n",scriptResult.Lines.Skip(indexStart+1).Take(indexEnd-indexStart)));
				Assert.AreEqual(4,xmlDocument.SelectNodes("/Configuration/MappedFolders/MappedFolder").Count);
				Match matchWork=Regex.Match(string.Join("\n",listPlan),@"make (\S+) with in\\");
				Assert.IsTrue(matchWork.Success);
				Assert.IsFalse(Directory.Exists(matchWork.Groups[1].Value),"No work folder: "+matchWork.Groups[1].Value);
				CollectionAssert.AreEqual(new string[0],Directory.GetFileSystemEntries(folder),"Nothing written.");
			}
			finally {
				DeleteFolder(folder);
			}
		}

		///<summary>The .wsb file: the setup's folder, the staging folder and the script folder mapped read-only, the results folder writable, the
		///host's paths kept exactly (XML escaping), networking on for the Visual C++ download, no GPU, audio, camera or printer, and the logon
		///command that runs the in-sandbox script from its read-only folder with the folders it maps, whose parameters that script has.</summary>
		[TestMethod]
		public void CleanInstall_Wsb_MapsReadOnly_AndRunsTheInSandboxScript() {
			string folder=TestPaths.NewTempFolder("launch3b_clean_wsb");
			try {
				string setupFolder=@"C:\Dry run & co\out";
				ScriptResult scriptResult=RunDriver(folder,"$ErrorActionPreference='Stop'\r\n"
					+". '"+ScriptPath(SCRIPT_CLEAN)+"'\r\n"
					+"[IO.File]::WriteAllText('"+Path.Combine(folder,"a.wsb")+"',(New-SandboxConfig '"+setupFolder+"' '"+setupFolder+@"\staging' 'C:\w\in' 'C:\w\results' 'OpenPmsDental-0.0-Setup.exe' $false))"+"\r\n"
					+"[IO.File]::WriteAllText('"+Path.Combine(folder,"b.wsb")+"',(New-SandboxConfig 'C:\\s' 'C:\\s\\staging' 'C:\\w\\in' 'C:\\w\\results' 'OpenPmsDental-0.0-Setup.exe' $true))\r\n"
					+"\"OK=1\"\r\n");
				Assert.AreEqual("1",Value(scriptResult,"OK"));
				XmlDocument xmlDocument=new XmlDocument();
				xmlDocument.Load(Path.Combine(folder,"a.wsb"));
				XmlNodeList xmlNodeList=xmlDocument.SelectNodes("/Configuration/MappedFolders/MappedFolder");
				List<string> listMappings=xmlNodeList.Cast<XmlNode>().Select(x => x["HostFolder"].InnerText+" > "+x["SandboxFolder"].InnerText+" "+x["ReadOnly"].InnerText).ToList();
				CollectionAssert.AreEqual(new[] { setupFolder+@" > C:\launch\setup true",setupFolder+@"\staging > C:\launch\staging true",@"C:\w\in > C:\launch\in true",
					@"C:\w\results > C:\launch\results false" },listMappings);
				Assert.AreEqual("Enable",xmlDocument.SelectSingleNode("/Configuration/Networking").InnerText,"Setup downloads the x64 Visual C++ runtime.");
				foreach(string device in new[] { "VGpu","AudioInput","VideoInput","PrinterRedirection" }) {
					Assert.AreEqual("Disable",xmlDocument.SelectSingleNode("/Configuration/"+device).InnerText,device);
				}
				string command=xmlDocument.SelectSingleNode("/Configuration/LogonCommand/Command").InnerText;
				Assert.AreEqual(@"powershell.exe -NoProfile -ExecutionPolicy Bypass -File C:\launch\in\Test-CleanInstall-InSandbox.ps1 -SetupFile ""C:\launch\setup\OpenPmsDental-0.0-Setup.exe"" "
					+@"-StagingFolder C:\launch\staging -ResultsFolder C:\launch\results",command);
				XmlDocument xmlDocumentOpen=new XmlDocument();
				xmlDocumentOpen.Load(Path.Combine(folder,"b.wsb"));
				Assert.IsTrue(xmlDocumentOpen.SelectSingleNode("/Configuration/LogonCommand/Command").InnerText.EndsWith(" -NoShutdown"),"-KeepSandboxOpen.");
				//The in-sandbox script takes those parameters and runs from that folder.
				string sandbox=File.ReadAllText(ScriptPath(SCRIPT_SANDBOX));
				string blockParam=Regex.Match(sandbox,@"(?s)\nparam\((.*?)\n\)").Groups[1].Value;
				foreach(string parameter in new[] { "[string]$SetupFile","[string]$StagingFolder","[string]$ResultsFolder","[switch]$NoShutdown" }) {
					StringAssert.Contains(blockParam,parameter);
				}
				StringAssert.Contains(sandbox,@"$SandboxScriptFolder='C:\launch\in'");
				StringAssert.Contains(File.ReadAllText(ScriptPath(SCRIPT_CLEAN)),@"$SandboxInFolder='C:\launch\in'");
			}
			finally {
				DeleteFolder(folder);
			}
		}

		///<summary>What the in-sandbox script writes, the host script reads: ok, FAIL and note lines, the DONE line (a file without it is a run that
		///did not finish), a missing file as nothing; a character outside ASCII is written as '?'. And the install folder's comparison.</summary>
		[TestMethod]
		public void CleanInstall_ResultsFile_RoundTrip_AndTheInstallFolderComparison() {
			string folder=TestPaths.NewTempFolder("launch3b_clean_results");
			try {
				string results=Path.Combine(folder,"results.txt");
				string unfinished=Path.Combine(folder,"unfinished.txt");
				ScriptResult scriptResult=RunDriver(folder,"$ErrorActionPreference='Stop'\r\n"
					+". '"+ScriptPath(SCRIPT_SANDBOX)+"'\r\n"
					+"Write-ResultLine '"+results+"' 'RUN started=x'\r\n"
					+"Write-ResultLine '"+results+"' 'ok    first check'\r\n"
					+"Write-ResultLine '"+results+"' ('FAIL  second check '+[char]0xE9)\r\n"
					+"Write-ResultLine '"+results+"' 'note  a note'\r\n"
					+"Write-ResultLine '"+results+"' 'ok    third check'\r\n"
					+"Write-ResultLine '"+results+"' 'DONE passed=2 failed=1'\r\n"
					+"Write-ResultLine '"+unfinished+"' 'ok    only one'\r\n"
					+"$installed=@{ 'a.dll'='1'; 'b.dll'='X'; 'extra.txt'='3'; 'FreeDentalConfig.xml'='4'; 'uninstall\\unins000.dat'='5'; 'uninstall\\unins000.exe'='6' }\r\n"
					+"$staging=@{ 'a.dll'='1'; 'b.dll'='2'; 'c.dll'='3' }\r\n"
					+"$cmp=Compare-InstallFolder $installed $staging\r\n"
					+"\"CMP=$($cmp.Missing -join ',')|$($cmp.Changed -join ',')|$($cmp.Extra -join ',')|$($cmp.SetupOwn.Count)\"\r\n"
					+"\"ARGS=$((Get-SetupArguments 'C:\\launch\\results\\setup.log') -join ' ')\"\r\n"
					+". '"+ScriptPath(SCRIPT_CLEAN)+"'\r\n"
					+"foreach($p in @('"+results+"','"+unfinished+"','"+Path.Combine(folder,"none.txt")+"')){ $r=Read-SandboxResults $p; "
						+"\"READ=$($r.Passes.Count)|$($r.Failures.Count)|$($r.Notes.Count)|$($r.Done)|$(Test-ResultsDone $p)|$($r.Failures -join ',')\" }\r\n");
				List<string> listRead=scriptResult.Lines.Where(x => x.StartsWith("READ=")).ToList();
				CollectionAssert.AreEqual(new[] { "READ=2|1|1|True|True|second check ?","READ=1|0|0|False|False|","READ=0|0|0|False|False|" },listRead);
				Assert.IsFalse(File.ReadAllBytes(results).Any(x => x>=0x80),"The results file is ASCII.");
				Assert.AreEqual("c.dll|b.dll|extra.txt|3",Value(scriptResult,"CMP"));
				string arguments=Value(scriptResult,"ARGS");
				Assert.AreEqual(@"/VERYSILENT /SUPPRESSMSGBOXES /NORESTART /PREREQS=download /DBSERVER=localhost /DBNAME=opms_sandbox /DBUSER=opms_user "
					+@"/LOG=""C:\launch\results\setup.log""",arguments);
				//A database server on the sandbox's own computer: a silent setup runs no connection settings tool for it.
				Assert.IsTrue(DataConnectionBase.DbConnectionSecurity.IsLoopbackServer("localhost"));
				//The setup knows /PREREQS=download.
				string iss=TestPaths.ReadSource("installer","OpenPmsDental.iss");
				StringAssert.Contains(iss,"{param:PREREQS|ask}");
				StringAssert.Contains(iss,";   /PREREQS=ask|download|skip");
			}
			finally {
				DeleteFolder(folder);
			}
		}

		///<summary>The in-sandbox script checks what the launch check needs: no .NET runtime before and after the install (dotnet.exe and the shared
		///framework folders), the x64 Visual C++ runtime installed, the uninstall entry of the setup's AppId in the 64-bit view, the installed
		///connection settings tool on the install folder's coreclr.dll and hostfxr.dll, the 64-bit TWAINDSM.dll, the silent uninstall, the DONE line
		///and the shutdown that discards the sandbox.</summary>
		[TestMethod]
		public void InSandbox_ChecksTheCleanInstall() {
			string sandbox=File.ReadAllText(ScriptPath(SCRIPT_SANDBOX));
			string iss=TestPaths.ReadSource("installer","OpenPmsDental.iss");
			string appId=Regex.Match(iss,@"(?m)^AppId=\{(\{[^}]+\})").Groups[1].Value;
			Assert.IsTrue(appId.Length>30,appId);
			StringAssert.Contains(sandbox,"$AppId='"+appId+"_is1'");
			StringAssert.Contains(sandbox,@"$UninstallKey=""HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\$AppId""");
			StringAssert.Contains(sandbox,"$DbSettingsExe='"+ProgramNames.DbSettingsExeFileName+"'");
			foreach(string text in new[] { @"'dotnet\dotnet.exe'",@"'dotnet\shared\Microsoft.NETCore.App'",@"'dotnet\shared\Microsoft.WindowsDesktop.App'",
				@"'HKLM:\SOFTWARE\Microsoft\VisualStudio\14.0\VC\Runtimes\x64'","-ArgumentList '/check'","(Split-Path $_ -Leaf) -eq 'coreclr.dll'",
				"(Split-Path $_ -Leaf) -eq 'hostfxr.dll'","($machine -eq 0x8664)",@"'uninstall\unins000.exe'","'/VERYSILENT','/SUPPRESSMSGBOXES','/NORESTART'",
				"Write-ResultLine $ResultsPath (\"DONE passed={0} failed={1} finished={2}\"","& shutdown.exe /s /t 5" })
			{
				StringAssert.Contains(sandbox,text);
			}
			//The DONE line and the shutdown come in the finally block, after a failure too.
			string blockFinally=sandbox.Substring(sandbox.LastIndexOf("\nfinally {",StringComparison.Ordinal));
			StringAssert.Contains(blockFinally,"DONE passed=");
			StringAssert.Contains(blockFinally,"if(-not $NoShutdown){ & shutdown.exe");
			//The host script waits with a time limit and prints the summary line of Test-Installer.
			string clean=File.ReadAllText(ScriptPath(SCRIPT_CLEAN));
			StringAssert.Contains(clean,"[int]$TimeoutMinutes=30");
			StringAssert.Contains(clean,"while(-not (Test-ResultsDone $resultsPath) -and (Get-Date) -lt $deadline)");
			StringAssert.Contains(clean,"Write-Host (\"{0} checks passed, {1} failed. Logs: {2}\" -f $script:Passes,$script:Failures.Count,$OutDir)");
			StringAssert.Contains(clean,@"$SandboxExe=Join-Path ([Environment]::SystemDirectory) 'WindowsSandbox.exe'");
			//It starts nothing but Windows Sandbox and runs nothing but Build-Release's list.
			CollectionAssert.AreEquivalent(new[] { "$SandboxExe" },Regex.Matches(clean,@"Start-Process -FilePath (\S+)").Cast<Match>().Select(x => x.Groups[1].Value).ToArray());
			string code=clean.Substring(clean.IndexOf("#>",StringComparison.Ordinal));
			Assert.IsFalse(Regex.IsMatch(code,@"(?m)^\s*Enable-WindowsOptionalFeature"),"The script never enables the feature itself: it prints the command.");
			StringAssert.Contains(code,"Write-Host \"  $EnableCommand\"");
		}
		#endregion
	}
}
