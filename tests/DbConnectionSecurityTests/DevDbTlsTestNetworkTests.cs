// DB-TLS-ENV (docs/port-notes/DB-TLS.md, section 11): tools/Enable-LocalDbTls.ps1 -WithTestNetwork, the environment of S2-2's database TLS check,
// without changing anything. The certificate set with the test network's names and the two test client .pfx files, made in a temporary folder
// (-TestCertificatesFolder) and read back without a store (EphemeralKeySet); the refusal of an address outside 192.0.2.0/24 before anything
// else; the checks of an address in use and of this computer's network on synthetic facts; the record in my.ini on synthetic text; the enable
// plan and -Revert's plan, which undoes each of its steps; a real dry run; and the adapter's SetupAPI source compiled, never called. Nothing here
// elevates, creates an adapter, changes a network setting, the firewall, the MariaDB service or a certificate store.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DbConnectionSecurityTests {
	[TestClass]
	public class DevDbTlsTestNetworkTests {
		///<summary>Every change of the test network, as its tag in the enable run's plan; -Revert's plan names each one.</summary>
		private static readonly string[] _arrayTags={ "adapter","address","bindings","bind-address","client-certificates","firewall","key-access",
			"server-certificate" };
		private string _folder;

		[TestInitialize]
		public void SetUp() {
			_folder=TestHelpers.NewTempFolder();
		}

		[TestCleanup]
		public void CleanUp() {
			TestHelpers.DeleteFolder(_folder);
		}

		private string RunDriver(string[] arrayFunctions,string[] arrayBody) {
			return DevDbTlsScriptTests.RunDriverIn(_folder,arrayFunctions,arrayBody);
		}

		///<summary>This computer's IPv4 addresses, read without changing anything.</summary>
		private static List<string> GetLocalAddresses() {
			return NetworkInterface.GetAllNetworkInterfaces()
				.SelectMany(x => x.GetIPProperties().UnicastAddresses)
				.Where(x => x.Address.AddressFamily==AddressFamily.InterNetwork)
				.Select(x => x.Address.ToString())
				.ToList();
		}

		///<summary>The tags ("[adapter]") of plan lines.</summary>
		private static List<string> GetTags(string text) {
			return System.Text.RegularExpressions.Regex.Matches(text,@"\[([a-z-]+)\]").Cast<System.Text.RegularExpressions.Match>()
				.Select(x => x.Groups[1].Value).Where(x => _arrayTags.Contains(x)).Distinct().OrderBy(x => x).ToList();
		}

		[TestMethod]
		public void TestNetwork_TestCertificates_TheServerNamesTheAddressAndTheHost_TwoClientPfxFilesOfTheSameCa() {
			string folder=Path.Combine(_folder,"certs");
			int exitCode=DevDbTlsScriptTests.RunPowerShell(DevDbTlsScriptTests.GetScriptPath(),"-TestCertificatesFolder \""+folder+"\" -WithTestNetwork 192.0.2.25",
				out string output);
			Assert.AreEqual(0,exitCode,output);
			CollectionAssert.AreEquivalent(new[] { "ca.pem","server-cert.pem","server-key.pem","test-client-capi.pfx","test-client-cng.pfx",
				"test-client-pfx-password.txt" },Directory.GetFiles(folder).Select(x => Path.GetFileName(x)).ToArray(),"No file with the CA's private key.");
			X509Certificate2 x509Certificate2Ca=new X509Certificate2(DevDbTlsScriptTests.ReadPem(Path.Combine(folder,"ca.pem"),"CERTIFICATE"));
			X509Certificate2 x509Certificate2Server=new X509Certificate2(DevDbTlsScriptTests.ReadPem(Path.Combine(folder,"server-cert.pem"),"CERTIFICATE"));
			//VerifyFull to the test address needs it in the server certificate; the host name is there too (the owner's decision).
			CollectionAssert.AreEqual(new[] { "DNS:localhost","IP:127.0.0.1","DNS:"+Dns.GetHostName(),"IP:192.0.2.25" },
				DevDbTlsScriptTests.GetAlternativeNames(DevDbTlsScriptTests.GetExtension(x509Certificate2Server,"2.5.29.17")));
			DevDbTlsScriptTests.AssertChainsTo(x509Certificate2Server,x509Certificate2Ca);
			string password=File.ReadAllText(Path.Combine(folder,"test-client-pfx-password.txt"));
			Assert.IsTrue(System.Text.RegularExpressions.Regex.IsMatch(password,"^[0-9a-f]{32}$"),"A random password of 32 hex digits.");
			List<string> listThumbprints=new List<string>();
			foreach(string[] arrayCase in new[] { new[] { "test-client-capi.pfx","opms-dbtls-test-client-capi" },new[] { "test-client-cng.pfx","opms-dbtls-test-client-cng" } }) {
				byte[] arrayPfx=File.ReadAllBytes(Path.Combine(folder,arrayCase[0]));
				X509Certificate2Collection x509Certificate2Collection=new X509Certificate2Collection();
				//EphemeralKeySet: the key is read into memory only, nothing goes to a key store.
				x509Certificate2Collection.Import(arrayPfx,password,X509KeyStorageFlags.EphemeralKeySet);
				Assert.AreEqual(1,x509Certificate2Collection.Count,arrayCase[0]+": only the client certificate, no CA, so the import adds nothing to another store.");
				X509Certificate2 x509Certificate2=x509Certificate2Collection[0];
				Assert.IsTrue(x509Certificate2.HasPrivateKey,arrayCase[0]);
				StringAssert.StartsWith(x509Certificate2.Subject,"CN="+arrayCase[1]+",");
				Assert.AreEqual(x509Certificate2Ca.Subject,x509Certificate2.Issuer,arrayCase[0]);
				Assert.IsFalse(((X509BasicConstraintsExtension)DevDbTlsScriptTests.GetExtension(x509Certificate2,"2.5.29.19")).CertificateAuthority);
				Assert.AreEqual("1.3.6.1.5.5.7.3.2",((X509EnhancedKeyUsageExtension)DevDbTlsScriptTests.GetExtension(x509Certificate2,"2.5.29.37"))
					.EnhancedKeyUsages.Cast<Oid>().Single().Value,arrayCase[0]+": client authentication only.");
				Assert.IsTrue(x509Certificate2.NotAfter>DateTime.Now.AddYears(4) && x509Certificate2.NotAfter<=x509Certificate2Ca.NotAfter,arrayCase[0]);
				DevDbTlsScriptTests.AssertChainsTo(x509Certificate2,x509Certificate2Ca);
				using(RSA rsaPrivate=x509Certificate2.GetRSAPrivateKey())
				using(RSA rsaPublic=x509Certificate2.GetRSAPublicKey())
				{
					Assert.IsTrue(rsaPublic.KeySize>=2048,arrayCase[0]);
					byte[] arrayData=Encoding.ASCII.GetBytes("DB-TLS-ENV");
					byte[] arraySignature=rsaPrivate.SignData(arrayData,HashAlgorithmName.SHA256,RSASignaturePadding.Pkcs1);
					Assert.IsTrue(rsaPublic.VerifyData(arrayData,arraySignature,HashAlgorithmName.SHA256,RSASignaturePadding.Pkcs1),arrayCase[0]+": the key belongs to it.");
				}
				listThumbprints.Add(x509Certificate2.Thumbprint);
				X509Certificate2Collection x509Certificate2CollectionWrong=new X509Certificate2Collection();
				TestHelpers.AssertThrows<CryptographicException>(() => x509Certificate2CollectionWrong.Import(arrayPfx,"wrong",X509KeyStorageFlags.EphemeralKeySet));
			}
			Assert.AreNotEqual(listThumbprints[0],listThumbprints[1],"Two certificates, each with its own key.");
			Assert.IsFalse(File.ReadAllText(Path.Combine(folder,"ca.pem")).Contains("PRIVATE KEY"),"The CA's key is discarded, as without the switch.");
		}

		[TestMethod]
		public void TestNetwork_AnAddressOutside192_0_2_0_24_IsRefused_BeforeAnythingElse() {
			string script=DevDbTlsScriptTests.GetScriptPath();
			foreach(string address in new[] { "10.0.0.5","192.0.3.1","192.0.2.0","192.0.2.255","192.0.2.256","192.0.2.025","192.0.2","198.51.100.25","localhost","::1" }) {
				Assert.AreEqual(1,DevDbTlsScriptTests.RunPowerShell(script,"-DryRun -WithTestNetwork "+address,out string output),address+": "+output);
				StringAssert.Contains(output,"REFUSED: -WithTestNetwork "+address+" is not a host address of 192.0.2.0/24",address);
				Assert.IsFalse(output.Contains("Service:"),address+": refused before the service is even looked at.");
				Assert.IsFalse(output.Contains("Plan:"),address);
			}
			string folder=Path.Combine(_folder,"certs");
			Assert.AreEqual(1,DevDbTlsScriptTests.RunPowerShell(script,"-TestCertificatesFolder \""+folder+"\" -WithTestNetwork 10.0.0.5",out string outputFolder),outputFolder);
			StringAssert.Contains(outputFolder,"is not a host address of 192.0.2.0/24");
			Assert.IsFalse(Directory.Exists(folder),"Nothing written.");
			//The later modes take the test network from the record in my.ini; the switch goes with the enable run only.
			foreach(string mode in new[] { "-Revert","-RequireSecureTransport","-DryRun -Revert" }) {
				Assert.AreEqual(1,DevDbTlsScriptTests.RunPowerShell(script,mode+" -WithTestNetwork 192.0.2.25",out string outputMode),mode+": "+outputMode);
				StringAssert.Contains(outputMode,"REFUSED: -WithTestNetwork goes with the enable run (or -TestCertificatesFolder) only",mode);
			}
		}

		[TestMethod]
		public void TestNetwork_AnAddressInUse_OrAComputerItCannotBeMadeOn_IsRefused() {
			//The script's checks on synthetic addresses, routes and facts of this computer's network (Get-NetworkFacts reads the real ones).
			string output=RunDriver(new[] { "ConvertTo-AddressNumber","Get-TestAddressFormProblem","Get-TestAddressUseProblem","Get-FirewallBlockRanges",
				"Get-TestNetworkProblem","Test-FirewallOn" },new[] {
				"function Addr([string]$Ip, [string]$Alias) { [pscustomobject]@{ IPAddress = $Ip; InterfaceAlias = $Alias } }",
				"function Route([string]$Prefix, [string]$Alias) { [pscustomobject]@{ DestinationPrefix = $Prefix; InterfaceAlias = $Alias } }",
				"$addresses = @((Addr '127.0.0.1' 'Loopback'), (Addr '198.51.100.7' 'Ethernet'))",
				"$routes = @((Route '0.0.0.0/0' 'Ethernet'), (Route '198.51.100.0/24' 'Ethernet'), (Route '255.255.255.255/32' 'Ethernet'), (Route '224.0.0.0/4' 'Ethernet'), (Route '127.0.0.0/8' 'Loopback'))",
				"function U([string]$Case, $Addr, $Rt) { $p = Get-TestAddressUseProblem '192.0.2.25' $Addr $Rt; if ($null -eq $p) { $p = 'OK' }; 'USE ' + $Case + ': ' + $p }",
				"U 'free' $addresses $routes",
				"U 'same' ($addresses + @(Addr '192.0.2.25' 'Wi-Fi')) $routes",
				"U 'other' ($addresses + @(Addr '192.0.2.99' 'vEthernet')) $routes",
				"U 'net24' $addresses ($routes + @(Route '192.0.2.0/24' 'VPN'))",
				"U 'net16' $addresses ($routes + @(Route '192.0.0.0/16' 'VPN'))",
				"U 'host32' $addresses ($routes + @(Route '192.0.2.200/32' 'VPN'))",
				"U 'neighbour' $addresses ($routes + @(Route '192.0.3.0/24' 'VPN'))",
				"foreach ($a in @('192.0.2.1', '192.0.2.25', '192.0.2.254', ' 192.0.2.7 ')) { 'FORM ' + $a.Trim() + ': ' + ($null -eq (Get-TestAddressFormProblem $a)) }",
				"function F([bool]$FwOn, $Open = @(), [bool]$Netloop = $true, [bool]$NameTaken = $false, $Rules = @()) {",
				"  [pscustomobject]@{ Addresses = $addresses; Routes = $routes; FirewallProfiles = @([pscustomobject]@{ Name = 'Domain'; Enabled = $FwOn }, [pscustomobject]@{ Name = 'Public'; Enabled = $FwOn });",
				"    OpenInterfaces = $Open; Loopbacks = @(); AdapterNameTaken = $NameTaken; RulesTaken = $Rules; HasNetloop = $Netloop; HostName = 'testhost' }",
				"}",
				"function N([string]$Case, $Facts, [string]$Version = '10.11.19-MariaDB') { $p = Get-TestNetworkProblem '192.0.2.25' $Facts $Version; if ($null -eq $p) { $p = 'OK' }; 'NET ' + $Case + ': ' + $p }",
				"$fwd = @([pscustomobject]@{ InterfaceAlias = 'vEthernet (Default Switch)'; Forwarding = 'Enabled'; WeakHostReceive = 'Disabled' })",
				"N 'ok' (F $true)",
				"N 'onforwarding' (F $true $fwd)",
				"N 'offclosed' (F $false)",
				"N 'offforwarding' (F $false $fwd)",
				"N 'old' (F $true) '10.6.18-MariaDB'",
				"N 'newer' (F $true) '11.4.2-MariaDB'",
				"N 'nonetloop' (F $true @() $false)",
				"N 'name' (F $true @() $true $true)",
				"N 'rules' (F $true @() $true $false @('OpenPmsDental-DbTlsTest-Allow3306'))",
				"N 'inuse' ([pscustomobject]@{ Addresses = ($addresses + @(Addr '192.0.2.25' 'Wi-Fi')); Routes = $routes; FirewallProfiles = @(); OpenInterfaces = @(); Loopbacks = @(); AdapterNameTaken = $false; RulesTaken = @(); HasNetloop = $true; HostName = 'h' })",
				"'FWON ' + (Test-FirewallOn (F $true)) + ' ' + (Test-FirewallOn (F $false))",
				"'RANGES ' + ((Get-FirewallBlockRanges '192.0.2.1') -join ',') + ' ' + ((Get-FirewallBlockRanges '192.0.2.25') -join ',') + ' ' + ((Get-FirewallBlockRanges '192.0.2.254') -join ',')",
			});
			StringAssert.Contains(output,"USE free: OK\r\n");
			StringAssert.Contains(output,"USE same: -WithTestNetwork 192.0.2.25 is in use: this computer has that address on Wi-Fi.");
			StringAssert.Contains(output,"USE other: -WithTestNetwork 192.0.2.25: this computer already has 192.0.2.99 of 192.0.2.0/24 on vEthernet");
			StringAssert.Contains(output,"USE net24: -WithTestNetwork 192.0.2.25: the route 192.0.2.0/24 on VPN leads into 192.0.2.0/24");
			StringAssert.Contains(output,"USE net16: -WithTestNetwork 192.0.2.25: the route 192.0.0.0/16 on VPN leads into 192.0.2.0/24");
			StringAssert.Contains(output,"USE host32: -WithTestNetwork 192.0.2.25: the route 192.0.2.200/32 on VPN leads into 192.0.2.0/24");
			StringAssert.Contains(output,"USE neighbour: OK\r\n","192.0.3.0/24 is another range.");
			foreach(string address in new[] { "192.0.2.1","192.0.2.25","192.0.2.254","192.0.2.7" }) {
				StringAssert.Contains(output,"FORM "+address+": True\r\n");
			}
			StringAssert.Contains(output,"NET ok: OK\r\n");
			StringAssert.Contains(output,"NET onforwarding: OK\r\n","With the firewall on, its block rule keeps other computers out even through a forwarding interface.");
			StringAssert.Contains(output,"NET offclosed: OK\r\n","No firewall rule is needed when no interface forwards to the address.");
			StringAssert.Contains(output,"NET offforwarding: The Windows firewall is off in the profile Domain, Public, and vEthernet (Default Switch) forwards IPv4");
			StringAssert.Contains(output,"NET old: The server is 10.6.18-MariaDB. A bind-address with two addresses needs MariaDB 10.11 or later.");
			StringAssert.Contains(output,"NET newer: OK\r\n");
			StringAssert.Contains(output,"NET nonetloop: Windows' own loopback adapter driver (netloop.inf) is not in");
			StringAssert.Contains(output,"NET name: A network adapter named 'OPMS DB TLS test' exists already");
			StringAssert.Contains(output,"NET rules: The firewall rule OpenPmsDental-DbTlsTest-Allow3306 exists already");
			StringAssert.Contains(output,"NET inuse: -WithTestNetwork 192.0.2.25 is in use");
			StringAssert.Contains(output,"FWON True False");
			StringAssert.Contains(output,"RANGES 0.0.0.0-192.0.2.0,192.0.2.2-255.255.255.255 0.0.0.0-192.0.2.24,192.0.2.26-255.255.255.255 "
				+"0.0.0.0-192.0.2.253,192.0.2.255-255.255.255.255","Every remote address but the test address itself.");
		}

		[TestMethod]
		public void TestNetwork_TheBlockRecordsWhatTheRunMade_ForRevertAndRequireSecureTransport_AndRefusesARecordNoRunWrote() {
			string output=RunDriver(new[] { "Add-ToMysqldSection","Get-BlockLines","Get-CreatedParents","Get-RecordedTestNetwork","Get-TestNetworkRecordProblem",
				"Get-TestAddressFormProblem","Get-ServerOptions","Get-ExpectedBindAddress","Get-BindText","Remove-Block","Test-HasBlock","Get-RecordedFolders" },new[] {
				"$net = [pscustomobject]@{ Address = '192.0.2.25'; AdapterId = 'ROOT\\NET\\0003'; FirewallRules = @($FirewallAllowName, $FirewallBlockName); CapiThumbprint = ('A' * 40); CngThumbprint = ('B' * 40) }",
				"function Paths($Net) { [pscustomobject]@{ DevFolder = 'C:\\u\\OpenPmsDental\\DevDbTls'; ClientFolder = ''; CreatedParents = @(); Ca = 'C:\\x\\ca.pem'; ServerCert = 'C:\\x\\s.pem'; ServerKey = 'C:\\x\\k.pem'; TestNetwork = $Net } }",
				"$ini = \"[mysqld]`nport=3306`nbind-address=127.0.0.1`n`n[client]`nport=3306`n\"",
				"$new = Add-ToMysqldSection $ini (Get-BlockLines (Paths $net) $false) \"`n\"",
				"[System.IO.File]::WriteAllText((Join-Path $Folder 'new.ini'), $new)",
				"$r = Get-RecordedTestNetwork $new",
				"'RECORD ' + $r.Address + '|' + $r.AdapterId + '|' + (@($r.FirewallRules) -join ',') + '|' + $r.CapiThumbprint + '|' + $r.CngThumbprint",
				"'RECORDOK ' + ($null -eq (Get-TestNetworkRecordProblem $r))",
				"'BIND ' + (Get-ServerOptions $new)['bind_address'] + ' ' + (Get-ExpectedBindAddress $new) + ' ' + (Get-ExpectedBindAddress $ini) + ' ' + (Get-BindText '127.0.0.1, 192.0.2.25')",
				"'FOLDERS ' + (Get-RecordedFolders $new).DevFolder",
				"'REMOVED ' + ((Remove-Block $new) -eq $ini) + ' ' + ($null -eq (Get-RecordedTestNetwork (Remove-Block $new)))",
				"$require = Add-ToMysqldSection (Remove-Block $new) (Get-BlockLines (Paths $r) $true) \"`n\"",
				"$r2 = Get-RecordedTestNetwork $require",
				"'REQUIRE ' + ($r2.AdapterId -eq $r.AdapterId) + ' ' + ($r2.CngThumbprint -eq $r.CngThumbprint) + ' ' + (@($r2.FirewallRules).Count) + ' ' + (Get-ServerOptions $require)['require_secure_transport'] + ' ' + (Get-ServerOptions $require)['bind_address']",
				"'NONET ' + ($null -eq (Get-RecordedTestNetwork (Add-ToMysqldSection $ini (Get-BlockLines (Paths $null) $false) \"`n\"))) + ' ' + ((Add-ToMysqldSection $ini (Get-BlockLines (Paths $null) $false) \"`n\").Contains('bind-address=127.0.0.1,'))",
				"$off = [pscustomobject]@{ Address = '192.0.2.25'; AdapterId = 'ROOT\\NET\\0004'; FirewallRules = @(); CapiThumbprint = ('C' * 40); CngThumbprint = ('D' * 40) }",
				"$rOff = Get-RecordedTestNetwork (Add-ToMysqldSection $ini (Get-BlockLines (Paths $off) $false) \"`n\")",
				"'OFF ' + @($rOff.FirewallRules).Count + ' ' + ($null -eq (Get-TestNetworkRecordProblem $rOff))",
				"function P([string]$Case, [hashtable]$Changes) {",
				"  $c = [pscustomobject]@{ Address = $r.Address; AdapterId = $r.AdapterId; FirewallRules = @($r.FirewallRules); CapiThumbprint = $r.CapiThumbprint; CngThumbprint = $r.CngThumbprint }",
				"  foreach ($k in $Changes.Keys) { $c.$k = $Changes[$k] }",
				"  $p = Get-TestNetworkRecordProblem $c; if ($null -eq $p) { $p = 'OK' }; 'PROBLEM ' + $Case + ': ' + $p",
				"}",
				"P 'address' @{ Address = '10.0.0.1' }",
				"P 'adapter' @{ AdapterId = 'PCI\\VEN_8086&DEV_15B8\\3&11583659&0&FE' }",
				"P 'adapterdots' @{ AdapterId = 'ROOT\\NET\\0001\\..' }",
				"P 'rule' @{ FirewallRules = @('Some other rule') }",
				"P 'thumb' @{ CngThumbprint = 'xyz' }",
			});
			string textNew=File.ReadAllText(Path.Combine(_folder,"new.ini"));
			StringAssert.Contains(textNew,"\n# test-address=192.0.2.25\n# test-adapter=ROOT\\NET\\0003\n# test-firewall-rule=OpenPmsDental-DbTlsTest-Allow3306\n"
				+"# test-firewall-rule=OpenPmsDental-DbTlsTest-Block3306\n# test-client-capi="+new string('A',40)+"\n# test-client-cng="+new string('B',40)+"\nssl_ca=",
				"The record as comment lines, after the folders' record.");
			StringAssert.Contains(textNew,"tls_version=TLSv1.2,TLSv1.3\nbind-address=127.0.0.1,192.0.2.25\n# END Open PMS - Dental local test TLS\n");
			StringAssert.Contains(output,"RECORD 192.0.2.25|ROOT\\NET\\0003|OpenPmsDental-DbTlsTest-Allow3306,OpenPmsDental-DbTlsTest-Block3306|"+new string('A',40)+"|"+new string('B',40)+"\r\n");
			StringAssert.Contains(output,"RECORDOK True");
			StringAssert.Contains(output,"BIND 127.0.0.1,192.0.2.25 127.0.0.1,192.0.2.25 127.0.0.1 127.0.0.1,192.0.2.25\r\n",
				"The block's bind-address follows my.ini's and wins; the later modes expect it exactly when the block records the address.");
			StringAssert.Contains(output,@"FOLDERS C:\u\OpenPmsDental\DevDbTls");
			StringAssert.Contains(output,"REMOVED True True","Removing the block gives my.ini back as it was.");
			StringAssert.Contains(output,"REQUIRE True True 2 ON 127.0.0.1,192.0.2.25\r\n","-RequireSecureTransport's block keeps the record and the address.");
			StringAssert.Contains(output,"NONET True False","Without -WithTestNetwork: no record, no second address.");
			StringAssert.Contains(output,"OFF 0 True","The firewall was off: no rule recorded.");
			StringAssert.Contains(output,"PROBLEM address: my.ini records the test address 10.0.0.1, which is not a host address of 192.0.2.0/24.");
			StringAssert.Contains(output,@"PROBLEM adapter: my.ini records the test adapter 'PCI\VEN_8086&DEV_15B8\3&11583659&0&FE', which is not a device this script creates");
			StringAssert.Contains(output,@"PROBLEM adapterdots: my.ini records the test adapter 'ROOT\NET\0001\..'");
			StringAssert.Contains(output,"PROBLEM rule: my.ini records the firewall rule 'Some other rule', which is not one of this script's");
			StringAssert.Contains(output,"PROBLEM thumb: my.ini records the test client certificate 'xyz', which is not a thumbprint");
			//The source: the later modes read the record (Get-Paths), and every mode's checks expect the recorded bind-address.
			string script=TestHelpers.ReadRepoFile("tools","Enable-LocalDbTls.ps1");
			StringAssert.Contains(script,"$testNetwork = Get-RecordedTestNetwork $script:IniFile.Text");
			StringAssert.Contains(script,"$expectedBind = Get-ExpectedBindAddress $script:IniFile.Text");
		}

		[TestMethod]
		public void TestNetwork_RevertsPlan_UndoesEveryStepOfTheEnablePlan_AndRemovesOnlyWhatTheScriptMade() {
			string output=RunDriver(new[] { "Get-TestNetworkEnablePlan","Get-TestNetworkRevertPlan","Get-TestNetworkStateProblem","Get-FirewallBlockRanges","Test-FirewallOn" },new[] {
				"$r = [pscustomobject]@{ Address = '192.0.2.25'; AdapterId = 'ROOT\\NET\\0003'; FirewallRules = @($FirewallAllowName, $FirewallBlockName); CapiThumbprint = ('A' * 40); CngThumbprint = ('B' * 40) }",
				"function S([bool]$Present, [string[]]$Hw = @('*MSLOOP'), [string]$Group = $FirewallGroup, [string]$CapiSubject = '', [bool]$Issued = $true, [bool]$HasCa = $true) {",
				"  if ($CapiSubject -eq '') { $CapiSubject = 'CN=' + $TestClientCapiName + ', O=Open PMS - Dental local test' }",
				"  [pscustomobject]@{",
				"    Adapter = [pscustomobject]@{ Exists = $Present; HardwareIds = $Hw; Addresses = @('192.0.2.25') }",
				"    Rules = @($r.FirewallRules | ForEach-Object { [pscustomobject]@{ Name = $_; Exists = $Present; Group = $Group } })",
				"    Certificates = @([pscustomobject]@{ Kind = 'CAPI'; Thumbprint = $r.CapiThumbprint; Name = $TestClientCapiName; Exists = $Present; Subject = $CapiSubject; IssuedByCa = $Issued },",
				"      [pscustomobject]@{ Kind = 'CNG'; Thumbprint = $r.CngThumbprint; Name = $TestClientCngName; Exists = $Present; Subject = ('CN=' + $TestClientCngName + ', O=Open PMS - Dental local test'); IssuedByCa = $Issued })",
				"    HasCa = $HasCa",
				"  }",
				"}",
				"function Q([string]$Case, $State) { $p = Get-TestNetworkStateProblem $r $State; if ($null -eq $p) { $p = 'OK' }; 'STATE ' + $Case + ': ' + $p }",
				"Q 'present' (S $true)",
				"Q 'gone' (S $false @())",
				"Q 'notloop' (S $true @('PCI\\VEN_8086&DEV_15B8'))",
				"Q 'group' (S $true @('*MSLOOP') 'Another group')",
				"Q 'subject' (S $true @('*MSLOOP') $FirewallGroup 'CN=someone else, O=x')",
				"Q 'issuer' (S $true @('*MSLOOP') $FirewallGroup '' $false)",
				"Q 'noca' (S $true @('*MSLOOP') $FirewallGroup '' $false $false)",
				"$on = [pscustomobject]@{ FirewallProfiles = @([pscustomobject]@{ Name = 'Domain'; Enabled = $true }); Loopbacks = @('ROOT\\NET\\0000') }",
				"$offFacts = [pscustomobject]@{ FirewallProfiles = @([pscustomobject]@{ Name = 'Domain'; Enabled = $false }); Loopbacks = @() }",
				"[System.IO.File]::WriteAllLines((Join-Path $Folder 'enable.txt'), [string[]](Get-TestNetworkEnablePlan '192.0.2.25' 'testhost' $on 'C:\\x\\opms-dev-tls'))",
				"[System.IO.File]::WriteAllLines((Join-Path $Folder 'enable-off.txt'), [string[]](Get-TestNetworkEnablePlan '192.0.2.25' 'testhost' $offFacts 'C:\\x\\opms-dev-tls'))",
				"[System.IO.File]::WriteAllLines((Join-Path $Folder 'revert.txt'), [string[]](Get-TestNetworkRevertPlan $r (S $true)))",
				"[System.IO.File]::WriteAllLines((Join-Path $Folder 'revert-gone.txt'), [string[]](Get-TestNetworkRevertPlan $r (S $false @())))",
				"$rOff = [pscustomobject]@{ Address = '192.0.2.25'; AdapterId = 'ROOT\\NET\\0003'; FirewallRules = @(); CapiThumbprint = ('A' * 40); CngThumbprint = ('B' * 40) }",
				"[System.IO.File]::WriteAllLines((Join-Path $Folder 'revert-off.txt'), [string[]](Get-TestNetworkRevertPlan $rOff (S $true)))",
			});
			StringAssert.Contains(output,"STATE present: OK\r\n");
			StringAssert.Contains(output,"STATE gone: OK\r\n","What is gone already is not refused: -Revert can run again after a step failed.");
			StringAssert.Contains(output,@"STATE notloop: The recorded test adapter ROOT\NET\0003 is not a loopback adapter");
			StringAssert.Contains(output,"STATE group: The firewall rule OpenPmsDental-DbTlsTest-Allow3306 is in the group 'Another group'");
			StringAssert.Contains(output,"STATE subject: The recorded certificate "+new string('A',40)+" in LocalMachine\\My is CN=someone else, O=x, not CN=opms-dbtls-test-client-capi");
			StringAssert.Contains(output,"STATE issuer: The recorded certificate "+new string('A',40)+" in LocalMachine\\My was not issued by the test CA");
			StringAssert.Contains(output,"STATE noca: The recorded certificate "+new string('A',40)+" cannot be checked against the test CA");
			string enable=File.ReadAllText(Path.Combine(_folder,"enable.txt"));
			string revert=File.ReadAllText(Path.Combine(_folder,"revert.txt"));
			CollectionAssert.AreEqual(_arrayTags.OrderBy(x => x).ToList(),GetTags(enable),"The enable plan names every step: "+enable);
			CollectionAssert.AreEqual(GetTags(enable),GetTags(revert),"-Revert's plan undoes every step of the enable plan: "+revert);
			CollectionAssert.AreEqual(GetTags(File.ReadAllText(Path.Combine(_folder,"enable-off.txt"))),
				GetTags(File.ReadAllText(Path.Combine(_folder,"revert-off.txt"))),"Also with the firewall off.");
			CollectionAssert.AreEqual(GetTags(enable),GetTags(File.ReadAllText(Path.Combine(_folder,"revert-gone.txt"))),"Also when everything is gone already.");
			//The enable plan: the adapter of Windows' own driver, the address and nothing else, only IPv4 bound, the two rules, the two providers.
			StringAssert.Contains(enable,"Microsoft KM-TEST Loopback Adapter (hardware id *MSLOOP, Windows' own netloop.inf from the driver");
			StringAssert.Contains(enable,"no download, no devcon");
			StringAssert.Contains(enable,@"Loopback adapters that exist already are not touched: ROOT\NET\0000.");
			StringAssert.Contains(enable,"Give it 192.0.2.25/32 and nothing else: no gateway, no DNS server");
			StringAssert.Contains(enable,"Switch off every binding of the adapter but Internet Protocol Version 4 (ms_tcpip): File and Printer Sharing");
			StringAssert.Contains(enable,"OpenPmsDental-DbTlsTest-Allow3306 allows it from 192.0.2.25 itself (this computer), OpenPmsDental-DbTlsTest-Block3306 blocks");
			StringAssert.Contains(enable,"(0.0.0.0-192.0.2.24, 192.0.2.26-255.255.255.255)");
			StringAssert.Contains(enable,"'Microsoft Enhanced RSA and AES Cryptographic Provider'");
			StringAssert.Contains(enable,"'Microsoft Software Key Storage Provider' (CNG)");
			StringAssert.Contains(enable,"The .pfx files are deleted after the import.");
			StringAssert.Contains(enable,@"Give BUILTIN\Users read access to both key files");
			StringAssert.Contains(enable,"also names 192.0.2.25 and this computer's host name testhost.");
			StringAssert.Contains(File.ReadAllText(Path.Combine(_folder,"enable-off.txt")),"The Windows firewall is off in every profile: no rule is added");
			//-Revert's plan names exactly what the record names.
			StringAssert.Contains(revert,"Remove the firewall rules OpenPmsDental-DbTlsTest-Allow3306 and OpenPmsDental-DbTlsTest-Block3306");
			StringAssert.Contains(revert,"("+new string('A',40)+", its key in a CAPI provider)");
			StringAssert.Contains(revert,"("+new string('B',40)+", its key in a CNG provider)");
			StringAssert.Contains(revert,"so no key that the users can read is left behind.");
			StringAssert.Contains(revert,@"Remove the loopback adapter ROOT\NET\0003 ('OPMS DB TLS test')");
			StringAssert.Contains(revert,"removes bind-address=127.0.0.1,192.0.2.25");
			StringAssert.Contains(revert,"goes with the opms-dev-tls folder");
			string revertGone=File.ReadAllText(Path.Combine(_folder,"revert-gone.txt"));
			StringAssert.Contains(revertGone,"are not there any more: nothing to remove.");
			StringAssert.Contains(revertGone,@"The loopback adapter ROOT\NET\0003 is not there any more");
			StringAssert.Contains(File.ReadAllText(Path.Combine(_folder,"revert-off.txt")),"No firewall rule to remove");
			//-Revert removes the test network first, while the record in my.ini still names it, and only what Get-TestNetworkStateProblem allows.
			string script=TestHelpers.ReadRepoFile("tools","Enable-LocalDbTls.ps1");
			int indexUndo=script.IndexOf("$netLeft = @(Undo-TestNetwork $recorded $net.Address)",StringComparison.Ordinal);
			int indexRestore=script.IndexOf("Copy-Item -LiteralPath $paths.Backup -Destination $script:Facts.Ini -Force\r\n\t\tRestart-DbService\r\n\t\t$plain",StringComparison.Ordinal);
			if(indexRestore<0) {
				indexRestore=script.IndexOf("Copy-Item -LiteralPath $paths.Backup -Destination $script:Facts.Ini -Force\n\t\tRestart-DbService\n\t\t$plain",StringComparison.Ordinal);
			}
			Assert.IsTrue(indexUndo>0 && indexRestore>indexUndo,"The test network goes before my.ini is restored.");
			StringAssert.Contains(script,"$netProblem = Get-TestNetworkStateProblem $net $netState");
		}

		[TestMethod]
		public void TestNetwork_DryRun_ListsEveryStep_AndChangesNothing() {
			List<string> listAddresses=GetLocalAddresses();
			string address=Enumerable.Range(25,200).Select(x => "192.0.2."+x).First(x => !listAddresses.Contains(x));
			int exitCode=DevDbTlsScriptTests.RunPowerShell(DevDbTlsScriptTests.GetScriptPath(),"-DryRun -WithClientCertificate -WithTestNetwork "+address,out string output);
			if(output.Contains("was not found on this computer") || output.Contains("No connection to 127.0.0.1:3306")) {
				Assert.Inconclusive("No local MariaDB service on 127.0.0.1:3306: "+output);
			}
			if(output.Contains("already on through this script")) {
				Assert.Inconclusive("TLS is already on through the script, so its enable plan is not shown: "+output);
			}
			if(output.Contains("leads into 192.0.2.0/24") || output.Contains("of 192.0.2.0/24 on")) {
				Assert.Inconclusive("This computer uses 192.0.2.0/24 already, so the dry run refuses: "+output);
			}
			Assert.AreEqual(0,exitCode,output);
			string lineIni=output.Split('\n').Select(x => x.Trim()).First(x => x.StartsWith("Option file:"));
			string pathIni=lineIni.Substring("Option file:".Length).Trim();
			CollectionAssert.AreEqual(_arrayTags.OrderBy(x => x).ToList(),GetTags(output),"Every step of the test network is in the plan: "+output);
			StringAssert.Contains(output,"# test-address="+address);
			StringAssert.Contains(output,@"# test-adapter=<the new adapter, ROOT\NET\nnnn>");
			StringAssert.Contains(output,"# test-client-capi=<the CAPI certificate's thumbprint>");
			StringAssert.Contains(output,"bind-address=127.0.0.1,"+address);
			StringAssert.Contains(output,"listens on 127.0.0.1:3306 and on "+address+":3306 (Get-NetTCPConnection)");
			StringAssert.Contains(output,"prints the three OPMS_DB_TLS_TEST_* values");
			StringAssert.Contains(output,"Dry run: nothing was changed.");
			Assert.IsFalse(output.Contains("require_secure_transport=ON"));
			Assert.IsFalse(File.Exists(pathIni+".before-opms-dev-tls"),"No backup was made.");
			Assert.IsFalse(GetLocalAddresses().Contains(address),"No address was added.");
			Assert.IsFalse(NetworkInterface.GetAllNetworkInterfaces().Any(x => x.Name=="OPMS DB TLS test"),"No adapter was created.");
		}

		[TestMethod]
		public void TestNetwork_TheAdaptersSetupApiSource_Compiles_AndNothingIsDownloaded() {
			//Add-Type compiles the source with the .NET Framework's own compiler; nothing of it is called.
			string output=RunDriver(new[] { "Get-LoopbackAdapterSource","Add-LoopbackAdapterType" },new[] {
				"Add-LoopbackAdapterType",
				"$t = 'OpenPmsDentalDevDbTls.LoopbackAdapter' -as [type]",
				"'METHODS ' + ((@($t.GetMethods() | Where-Object { $_.DeclaringType -eq $t } | ForEach-Object { $_.Name }) | Sort-Object) -join ',')",
				"'HWID ' + $t.GetField('HardwareId').GetValue($null)",
			});
			StringAssert.Contains(output,"METHODS Create,GetHardwareIds,Remove\r\n");
			StringAssert.Contains(output,"HWID *MSLOOP\r\n");
			string script=TestHelpers.ReadRepoFile("tools","Enable-LocalDbTls.ps1");
			foreach(string forbidden in new[] { "devcon.exe","Invoke-WebRequest","Invoke-RestMethod","DownloadFile","DownloadString","Start-BitsTransfer","Install-Module",
				"UpdateDriverForPlugAndPlayDevices" })
			{
				Assert.IsFalse(script.Contains(forbidden),"The script uses Windows' own tools only: "+forbidden);
			}
			//Removal checks the hardware id first, in the C# source and in -Revert's state check.
			StringAssert.Contains(script,"is not a loopback adapter (hardware ids \" + string.Join(\", \", ids) + \"); it is not removed.");
		}

		[TestMethod]
		public void TlsCertificateLoginTests_TellsACapiKeyFromACngKey_ByTheKeyProviderInformation() {
			//.NET Framework opens a CAPI key through CNG too (GetRSAPrivateKey gives an RSACng), so the check reads the key provider information.
			//Synthetic information on a certificate in memory: no key is opened or created, nothing goes to a store.
			using(X509Certificate2 x509Certificate2Capi=TestHelpers.NewCertificate("CN=capi",isCa:false,isClientAuth:true))
			using(X509Certificate2 x509Certificate2Cng=TestHelpers.NewCertificate("CN=cng",isCa:false,isClientAuth:true))
			using(X509Certificate2 x509Certificate2User=TestHelpers.NewCertificate("CN=user",isCa:false,isClientAuth:true))
			{
				X509Certificate2 x509Certificate2CapiPublic=new X509Certificate2(x509Certificate2Capi.RawData);
				X509Certificate2 x509Certificate2CngPublic=new X509Certificate2(x509Certificate2Cng.RawData);
				X509Certificate2 x509Certificate2UserPublic=new X509Certificate2(x509Certificate2User.RawData);
				new CertificateKeyProvInfo { IsNamed=true,ContainerName="dbtls-env-test-capi",ProviderName="Microsoft Enhanced RSA and AES Cryptographic Provider",
					ProviderType=24,Flags=CertificateKeyProvInfo.MACHINE_KEYSET_FLAG,KeySpec=1 }.Write(x509Certificate2CapiPublic);
				new CertificateKeyProvInfo { IsNamed=true,ContainerName="dbtls-env-test-cng",ProviderName="Microsoft Software Key Storage Provider",
					ProviderType=0,Flags=CertificateKeyProvInfo.MACHINE_KEYSET_FLAG,KeySpec=0 }.Write(x509Certificate2CngPublic);
				new CertificateKeyProvInfo { IsNamed=true,ContainerName="dbtls-env-test-user",ProviderName="Microsoft Software Key Storage Provider",
					ProviderType=0,Flags=0,KeySpec=0 }.Write(x509Certificate2UserPublic);
				Assert.IsFalse(TlsCertificateLoginTests.IsCngProvider(x509Certificate2CapiPublic,"CAPI"));
				Assert.IsTrue(TlsCertificateLoginTests.IsCngProvider(x509Certificate2CngPublic,"CNG"));
				UnitTestAssertException ex=TestHelpers.AssertThrows<UnitTestAssertException>(() => TlsCertificateLoginTests.IsCngProvider(x509Certificate2UserPublic,"USER"));
				StringAssert.Contains(ex.Message,"the key is in a user's key set, not the computer's");
				UnitTestAssertException exNone=TestHelpers.AssertThrows<UnitTestAssertException>(
					() => TlsCertificateLoginTests.IsCngProvider(new X509Certificate2(x509Certificate2Capi.RawData),"NONE"));
				StringAssert.Contains(exNone.Message,"the certificate names no key");
			}
		}
	}
}
