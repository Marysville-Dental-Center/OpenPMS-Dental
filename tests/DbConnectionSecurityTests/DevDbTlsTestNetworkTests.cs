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
			foreach(string address in new[] { "127.0.0.1","192.0.2.0","192.0.2.255","192.0.2.256","192.0.2.025","192.0.2","192.0.2.25.1","localhost","::1" }) {
				Assert.AreEqual(1,DevDbTlsScriptTests.RunPowerShell(script,"-DryRun -WithTestNetwork "+address,out string output),address+": "+output);
				StringAssert.Contains(output,"REFUSED: -WithTestNetwork "+address+" is not a host address of 192.0.2.0/24",address);
				Assert.IsFalse(output.Contains("Service:"),address+": refused before the service is even looked at.");
				Assert.IsFalse(output.Contains("Plan:"),address);
			}
			string folder=Path.Combine(_folder,"certs");
			Assert.AreEqual(1,DevDbTlsScriptTests.RunPowerShell(script,"-TestCertificatesFolder \""+folder+"\" -WithTestNetwork 127.0.0.1",out string outputFolder),outputFolder);
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
				"$addresses = @((Addr '127.0.0.1' 'Loopback'))",
				"$routes = @((Route '0.0.0.0/0' 'Ethernet'), (Route '255.255.255.255/32' 'Ethernet'), (Route '127.0.0.0/8' 'Loopback'))",
				"function U([string]$Case, $Addr, $Rt) { $p = Get-TestAddressUseProblem '192.0.2.25' $Addr $Rt; if ($null -eq $p) { $p = 'OK' }; 'USE ' + $Case + ': ' + $p }",
				"U 'free' $addresses $routes",
				"U 'same' ($addresses + @(Addr '192.0.2.25' 'Wi-Fi')) $routes",
				"U 'other' ($addresses + @(Addr '192.0.2.99' 'vEthernet')) $routes",
				"U 'net24' $addresses ($routes + @(Route '192.0.2.0/24' 'VPN'))",
				"U 'net23' $addresses ($routes + @(Route '192.0.2.0/23' 'VPN'))",
				"U 'host32' $addresses ($routes + @(Route '192.0.2.200/32' 'VPN'))",
				"U 'half' $addresses ($routes + @(Route '192.0.2.128/25' 'VPN'))",
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
			StringAssert.Contains(output,"USE net23: -WithTestNetwork 192.0.2.25: the route 192.0.2.0/23 on VPN leads into 192.0.2.0/24");
			StringAssert.Contains(output,"USE host32: -WithTestNetwork 192.0.2.25: the route 192.0.2.200/32 on VPN leads into 192.0.2.0/24");
			StringAssert.Contains(output,"USE half: -WithTestNetwork 192.0.2.25: the route 192.0.2.128/25 on VPN leads into 192.0.2.0/24");
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
			string output=RunDriver(new[] { "Add-ToMysqldSection","Get-BlockLines","Get-CreatedParents","Get-RecordedTestNetwork","ConvertFrom-TestNetworkLines","Get-TestNetworkRecordProblem",
				"Get-TestAddressFormProblem","Get-ServerOptions","Get-ExpectedBindAddress","Get-BindText","Remove-Block","Test-HasBlock","Get-RecordedFolders",
				"Get-KeyRecordProblem","ConvertFrom-KeyRecord","ConvertTo-KeyRecord","Get-MachineKeyFolder" },new[] {
				"function KeyInfo([string]$Kind, [string]$Provider, [string]$Container, [string]$Leaf) { [pscustomobject]@{ Provider = $Provider; Container = $Container; File = (Join-Path (Get-MachineKeyFolder $Kind) $Leaf) } }",
				"$capiKey = ConvertTo-KeyRecord (KeyInfo 'CAPI' $CapiProviderName 'dbtls-env-capi' (('a' * 32) + '_0f1e2d3c-4b5a-6978-8796-a5b4c3d2e1f0'))",
				"$cngKey = ConvertTo-KeyRecord (KeyInfo 'CNG' $CngProviderName 'dbtls-env-cng' (('b' * 40) + '_0f1e2d3c-4b5a-6978-8796-a5b4c3d2e1f0'))",
				"$net = [pscustomobject]@{ Address = '192.0.2.25'; AdapterId = 'ROOT\\NET\\0003'; FirewallRules = @($FirewallAllowName, $FirewallBlockName); CapiThumbprint = ('A' * 40); CngThumbprint = ('B' * 40); CapiKey = $capiKey; CngKey = $cngKey }",
				"function Paths($Net) { [pscustomobject]@{ DevFolder = 'C:\\u\\OpenPmsDental\\DevDbTls'; ClientFolder = ''; CreatedParents = @(); Ca = 'C:\\x\\ca.pem'; ServerCert = 'C:\\x\\s.pem'; ServerKey = 'C:\\x\\k.pem'; TestNetwork = $Net } }",
				"$ini = \"[mysqld]`nport=3306`nbind-address=127.0.0.1`n`n[client]`nport=3306`n\"",
				"$new = Add-ToMysqldSection $ini (Get-BlockLines (Paths $net) $false) \"`n\"",
				"[System.IO.File]::WriteAllText((Join-Path $Folder 'new.ini'), $new)",
				"$r = Get-RecordedTestNetwork $new",
				"'RECORD ' + $r.Address + '|' + $r.AdapterId + '|' + (@($r.FirewallRules) -join ',') + '|' + $r.CapiThumbprint + '|' + $r.CngThumbprint + '|' + $r.CapiKey + '|' + $r.CngKey",
				"'RECORDOK ' + ($null -eq (Get-TestNetworkRecordProblem $r))",
				"'BIND ' + (Get-ServerOptions $new)['bind_address'] + ' ' + (Get-ExpectedBindAddress $new) + ' ' + (Get-ExpectedBindAddress $ini) + ' ' + (Get-BindText '127.0.0.1, 192.0.2.25')",
				"'FOLDERS ' + (Get-RecordedFolders $new).DevFolder",
				"'REMOVED ' + ((Remove-Block $new) -eq $ini) + ' ' + ($null -eq (Get-RecordedTestNetwork (Remove-Block $new)))",
				"$require = Add-ToMysqldSection (Remove-Block $new) (Get-BlockLines (Paths $r) $true) \"`n\"",
				"$r2 = Get-RecordedTestNetwork $require",
				"'REQUIRE ' + ($r2.AdapterId -eq $r.AdapterId) + ' ' + ($r2.CngThumbprint -eq $r.CngThumbprint -and $r2.CapiKey -eq $r.CapiKey -and $r2.CngKey -eq $r.CngKey) + ' ' + (@($r2.FirewallRules).Count) + ' ' + (Get-ServerOptions $require)['require_secure_transport'] + ' ' + (Get-ServerOptions $require)['bind_address']",
				"'NONET ' + ($null -eq (Get-RecordedTestNetwork (Add-ToMysqldSection $ini (Get-BlockLines (Paths $null) $false) \"`n\"))) + ' ' + ((Add-ToMysqldSection $ini (Get-BlockLines (Paths $null) $false) \"`n\").Contains('bind-address=127.0.0.1,'))",
				"$off = [pscustomobject]@{ Address = '192.0.2.25'; AdapterId = 'ROOT\\NET\\0004'; FirewallRules = @(); CapiThumbprint = ('C' * 40); CngThumbprint = ('D' * 40); CapiKey = $capiKey; CngKey = $cngKey }",
				"$rOff = Get-RecordedTestNetwork (Add-ToMysqldSection $ini (Get-BlockLines (Paths $off) $false) \"`n\")",
				"'OFF ' + @($rOff.FirewallRules).Count + ' ' + ($null -eq (Get-TestNetworkRecordProblem $rOff))",
				"function P([string]$Case, [hashtable]$Changes) {",
				"  $c = [pscustomobject]@{ Address = $r.Address; AdapterId = $r.AdapterId; FirewallRules = @($r.FirewallRules); CapiThumbprint = $r.CapiThumbprint; CngThumbprint = $r.CngThumbprint; CapiKey = $r.CapiKey; CngKey = $r.CngKey }",
				"  foreach ($k in $Changes.Keys) { $c.$k = $Changes[$k] }",
				"  $p = Get-TestNetworkRecordProblem $c; if ($null -eq $p) { $p = 'OK' }; 'PROBLEM ' + $Case + ': ' + $p",
				"}",
				"P 'address' @{ Address = '127.0.0.1' }",
				"P 'adapter' @{ AdapterId = 'PCI\\VEN_8086&DEV_15B8\\3&11583659&0&FE' }",
				"P 'adapterdots' @{ AdapterId = 'ROOT\\NET\\0001\\..' }",
				"P 'rule' @{ FirewallRules = @('Some other rule') }",
				"P 'thumb' @{ CngThumbprint = 'xyz' }",
				"P 'keyprovider' @{ CapiKey = $capiKey.Replace($CapiProviderName, 'Microsoft Strong Cryptographic Provider') }",
				"P 'keykind' @{ CngKey = $capiKey }",
				"P 'keyfolder' @{ CngKey = ($CngProviderName + '|dbtls-env-cng|C:/Users/Public/dbtls-env-key') }",
				"P 'keyotherfolder' @{ CapiKey = $cngKey.Replace($CngProviderName, $CapiProviderName) }",
				"P 'keydots' @{ CapiKey = ($capiKey + '/../../../../Keys/x') }",
				"P 'keycontainer' @{ CapiKey = ($capiKey -replace '\\|dbtls-env-capi\\|', '|..\\x|') }",
				"P 'keyparts' @{ CapiKey = 'only|two' }",
				"P 'keymissing' @{ CngKey = '' }",
			});
			string textNew=File.ReadAllText(Path.Combine(_folder,"new.ini"));
			string folderCrypto=Environment.GetEnvironmentVariable("ProgramData").Replace('\\','/')+"/Microsoft/Crypto/";
			string capiKey="Microsoft Enhanced RSA and AES Cryptographic Provider|dbtls-env-capi|"+folderCrypto+"RSA/MachineKeys/"+new string('a',32)
				+"_0f1e2d3c-4b5a-6978-8796-a5b4c3d2e1f0";
			string cngKey="Microsoft Software Key Storage Provider|dbtls-env-cng|"+folderCrypto+"Keys/"+new string('b',40)+"_0f1e2d3c-4b5a-6978-8796-a5b4c3d2e1f0";
			StringAssert.Contains(textNew,"\n# test-address=192.0.2.25\n# test-adapter=ROOT\\NET\\0003\n# test-firewall-rule=OpenPmsDental-DbTlsTest-Allow3306\n"
				+"# test-firewall-rule=OpenPmsDental-DbTlsTest-Block3306\n# test-client-capi="+new string('A',40)+"\n# test-client-cng="+new string('B',40)+"\n"
				+"# test-client-capi-key="+capiKey+"\n# test-client-cng-key="+cngKey+"\nssl_ca=",
				"The record as comment lines, after the folders' record; each key's provider, container and file, so -Revert finds a key whose certificate is gone.");
			StringAssert.Contains(textNew,"tls_version=TLSv1.2,TLSv1.3\nbind-address=127.0.0.1,192.0.2.25\n# END Open PMS - Dental local test TLS\n");
			StringAssert.Contains(output,"RECORD 192.0.2.25|ROOT\\NET\\0003|OpenPmsDental-DbTlsTest-Allow3306,OpenPmsDental-DbTlsTest-Block3306|"+new string('A',40)+"|"+new string('B',40)
				+"|"+capiKey+"|"+cngKey+"\r\n");
			StringAssert.Contains(output,"RECORDOK True");
			StringAssert.Contains(output,"BIND 127.0.0.1,192.0.2.25 127.0.0.1,192.0.2.25 127.0.0.1 127.0.0.1,192.0.2.25\r\n",
				"The block's bind-address follows my.ini's and wins; the later modes expect it exactly when the block records the address.");
			StringAssert.Contains(output,@"FOLDERS C:\u\OpenPmsDental\DevDbTls");
			StringAssert.Contains(output,"REMOVED True True","Removing the block gives my.ini back as it was.");
			StringAssert.Contains(output,"REQUIRE True True 2 ON 127.0.0.1,192.0.2.25\r\n","-RequireSecureTransport's block keeps the record and the address.");
			StringAssert.Contains(output,"NONET True False","Without -WithTestNetwork: no record, no second address.");
			StringAssert.Contains(output,"OFF 0 True","The firewall was off: no rule recorded.");
			StringAssert.Contains(output,"PROBLEM address: my.ini records the test address 127.0.0.1, which is not a host address of 192.0.2.0/24.");
			StringAssert.Contains(output,@"PROBLEM adapter: my.ini records the test adapter 'PCI\VEN_8086&DEV_15B8\3&11583659&0&FE', which is not a device this script creates");
			StringAssert.Contains(output,@"PROBLEM adapterdots: my.ini records the test adapter 'ROOT\NET\0001\..'");
			StringAssert.Contains(output,"PROBLEM rule: my.ini records the firewall rule 'Some other rule', which is not one of this script's");
			StringAssert.Contains(output,"PROBLEM thumb: my.ini records the test client certificate 'xyz', which is not a thumbprint");
			//The recorded keys: this script's provider of the kind, a container name, a file directly in the computer's key folder of the kind.
			string folderMachineKeys=Environment.GetEnvironmentVariable("ProgramData")+@"\Microsoft\Crypto\RSA\MachineKeys";
			string folderKeys=Environment.GetEnvironmentVariable("ProgramData")+@"\Microsoft\Crypto\Keys";
			StringAssert.Contains(output,"PROBLEM keyprovider: my.ini records the key of the CAPI test client certificate as 'Microsoft Strong Cryptographic Provider|"
				+"dbtls-env-capi|","A CAPI key in another provider");
			StringAssert.Contains(output,"', whose provider is not 'Microsoft Enhanced RSA and AES Cryptographic Provider'.");
			StringAssert.Contains(output,"PROBLEM keykind: my.ini records the key of the CNG test client certificate as '"+capiKey+"', whose provider is not "
				+"'Microsoft Software Key Storage Provider'.");
			StringAssert.Contains(output,"PROBLEM keyfolder: my.ini records the key of the CNG test client certificate as 'Microsoft Software Key Storage Provider|"
				+"dbtls-env-cng|C:/Users/Public/dbtls-env-key', whose key file is not a file in "+folderKeys+".");
			StringAssert.Contains(output,"PROBLEM keyotherfolder: my.ini records the key of the CAPI test client certificate as 'Microsoft Enhanced RSA and AES "
				+"Cryptographic Provider|dbtls-env-cng|"+folderCrypto+"Keys/","A CNG key folder for a CAPI key");
			StringAssert.Contains(output,"', whose key file is not a file in "+folderMachineKeys+".");
			StringAssert.Contains(output,"PROBLEM keydots: my.ini records the key of the CAPI test client certificate as '"+capiKey+"/../../../../Keys/x', whose key file "
				+"is not a file in "+folderMachineKeys+".");
			StringAssert.Contains(output,"PROBLEM keycontainer: my.ini records the key of the CAPI test client certificate as 'Microsoft Enhanced RSA and AES "
				+@"Cryptographic Provider|..\x|"+folderCrypto+"RSA/MachineKeys/","A container name with a path in it");
			StringAssert.Contains(output,"', whose container name is empty or has a path in it.");
			StringAssert.Contains(output,"PROBLEM keyparts: my.ini records the key of the CAPI test client certificate as 'only|two', which is not "
				+"<provider>|<container>|<key file>.");
			StringAssert.Contains(output,"PROBLEM keymissing: my.ini records the key of the CNG test client certificate as '', which is not "
				+"<provider>|<container>|<key file>.","A record without the key is refused: -Revert could not find a key left behind.");
			//The source: the later modes read the record (Get-Paths), and every mode's checks expect the recorded bind-address.
			string script=TestHelpers.ReadRepoFile("tools","Enable-LocalDbTls.ps1");
			StringAssert.Contains(script,"$testNetwork = Get-RecordedTestNetwork $script:IniFile.Text");
			StringAssert.Contains(script,"$expectedBind = Get-ExpectedBindAddress $script:IniFile.Text");
		}

		[TestMethod]
		public void TestNetwork_RevertsPlan_UndoesEveryStepOfTheEnablePlan_AndRemovesOnlyWhatTheScriptMade() {
			string output=RunDriver(new[] { "Get-TestNetworkEnablePlan","Get-TestNetworkRevertPlan","Get-TestNetworkStateProblem","Get-FirewallBlockRanges","Test-FirewallOn","Get-TestNetworkJournalPath",
				"ConvertFrom-KeyRecord","ConvertTo-KeyRecord","Get-MachineKeyFolder" },new[] {
				"$capiKey = ConvertTo-KeyRecord ([pscustomobject]@{ Provider = $CapiProviderName; Container = 'dbtls-env-capi'; File = (Join-Path (Get-MachineKeyFolder 'CAPI') 'capi-key-file') })",
				"$cngKey = ConvertTo-KeyRecord ([pscustomobject]@{ Provider = $CngProviderName; Container = 'dbtls-env-cng'; File = (Join-Path (Get-MachineKeyFolder 'CNG') 'cng-key-file') })",
				"$r = [pscustomobject]@{ Address = '192.0.2.25'; AdapterId = 'ROOT\\NET\\0003'; FirewallRules = @($FirewallAllowName, $FirewallBlockName); CapiThumbprint = ('A' * 40); CngThumbprint = ('B' * 40); CapiKey = $capiKey; CngKey = $cngKey }",
				"function S([bool]$Present, [string[]]$Hw = @('*MSLOOP'), [string]$Group = $FirewallGroup, [string]$CapiSubject = '', [bool]$Issued = $true, [bool]$HasCa = $true) {",
				"  if ($CapiSubject -eq '') { $CapiSubject = 'CN=' + $TestClientCapiName + ', O=Open PMS - Dental local test' }",
				"  [pscustomobject]@{",
				"    Adapter = [pscustomobject]@{ Exists = $Present; HardwareIds = $Hw; Addresses = @('192.0.2.25'); AddressThere = $Present }",
				"    Rules = @($r.FirewallRules | ForEach-Object { [pscustomobject]@{ Name = $_; Exists = $Present; Group = $Group } })",
				"    Certificates = @([pscustomobject]@{ Kind = 'CAPI'; Thumbprint = $r.CapiThumbprint; Name = $TestClientCapiName; Exists = $Present; Subject = $CapiSubject; IssuedByCa = $Issued; Key = (ConvertFrom-KeyRecord $r.CapiKey 'CAPI'); KeyExists = $Present },",
				"      [pscustomobject]@{ Kind = 'CNG'; Thumbprint = $r.CngThumbprint; Name = $TestClientCngName; Exists = $Present; Subject = ('CN=' + $TestClientCngName + ', O=Open PMS - Dental local test'); IssuedByCa = $Issued; Key = (ConvertFrom-KeyRecord $r.CngKey 'CNG'); KeyExists = $Present })",
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
				"$rOff = [pscustomobject]@{ Address = '192.0.2.25'; AdapterId = 'ROOT\\NET\\0003'; FirewallRules = @(); CapiThumbprint = ('A' * 40); CngThumbprint = ('B' * 40); CapiKey = $capiKey; CngKey = $cngKey }",
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
			string fileCapiKey=Environment.GetEnvironmentVariable("ProgramData")+@"\Microsoft\Crypto\RSA\MachineKeys\capi-key-file";
			string fileCngKey=Environment.GetEnvironmentVariable("ProgramData")+@"\Microsoft\Crypto\Keys\cng-key-file";
			StringAssert.Contains(revert,"and delete its key first (the recorded file "+fileCapiKey+").");
			StringAssert.Contains(revert,"and delete its key first (the recorded file "+fileCngKey+").");
			StringAssert.Contains(File.ReadAllText(Path.Combine(_folder,"revert-gone.txt")),"file "+fileCapiKey+" is gone too: nothing to remove.",
				"The certificate and its recorded key file are both gone.");
			StringAssert.Contains(revert,@"Remove the loopback adapter ROOT\NET\0003 ('OPMS DB TLS test')");
			StringAssert.Contains(revert,"removes bind-address=127.0.0.1,192.0.2.25");
			StringAssert.Contains(revert,"goes with the opms-dev-tls folder");
			string revertGone=File.ReadAllText(Path.Combine(_folder,"revert-gone.txt"));
			StringAssert.Contains(revertGone,"are not there any more: nothing to remove.");
			StringAssert.Contains(revertGone,@"The loopback adapter ROOT\NET\0003 is not there any more");
			StringAssert.Contains(File.ReadAllText(Path.Combine(_folder,"revert-off.txt")),"No firewall rule to remove");
			//-Revert removes the test network first, while the record in my.ini still names it, and only what Get-TestNetworkStateProblem allows.
			string script=TestHelpers.ReadRepoFile("tools","Enable-LocalDbTls.ps1");
			int indexUndo=script.IndexOf("$undone = Undo-TestNetwork $recorded $net.Address",StringComparison.Ordinal);
			System.Text.RegularExpressions.Match matchRestore=System.Text.RegularExpressions.Regex.Match(script,
				@"Copy-Item -LiteralPath \$paths\.Backup -Destination \$script:Facts\.Ini -Force\s+Restart-DbService\s+\$plain");
			Assert.IsTrue(matchRestore.Success,"-Revert's restore of my.ini.");
			int indexRestore=matchRestore.Index;
			Assert.IsTrue(indexUndo>0 && indexRestore>indexUndo,"The test network goes before my.ini is restored.");
			//The order in -Revert's plan: the adapter first, then the certificates, the firewall rules last.
			int indexPlanAdapter=revert.IndexOf("[adapter] Remove the loopback adapter",StringComparison.Ordinal);
			int indexPlanCertificate=revert.IndexOf("[client-certificates] Remove",StringComparison.Ordinal);
			int indexPlanFirewall=revert.IndexOf("[firewall] Remove the firewall rules",StringComparison.Ordinal);
			Assert.IsTrue(indexPlanAdapter>=0 && indexPlanCertificate>indexPlanAdapter && indexPlanFirewall>indexPlanCertificate,
				"-Revert's plan removes the adapter first and the firewall rules last: "+revert);
			StringAssert.Contains(revert,"If the adapter cannot be removed, the rules stay (they keep other computers away from the address)");
			StringAssert.Contains(enable,@"its record line goes to the journal C:\x\opms-dev-tls\test-network.record");
			StringAssert.Contains(script,"$netProblem = Get-TestNetworkStateProblem $net $netState");
		}

		[TestMethod]
		public void TestNetwork_ACertificateDeletedByHand_LeavesItsKeyFile_RevertsPlanAndRemovalDeleteTheRecordedKey() {
			//certlm.msc's Delete, or certutil -delstore, removes only the store entry: the key file stays, with BUILTIN\Users' read access. The record
			//names each key, so -Revert's state, plan and removal find it. Synthetic: random thumbprints that are in no store (LocalMachine\My is only
			//read), key "files" in the test's temporary folder, and Remove-TestKey replaced by one that only deletes that file: no key is opened, and
			//nothing in a store or in the computer's key folders is touched.
			string thumbprintCapi=(Guid.NewGuid().ToString("N")+Guid.NewGuid().ToString("N")).Substring(0,40).ToUpperInvariant();
			string thumbprintCng=(Guid.NewGuid().ToString("N")+Guid.NewGuid().ToString("N")).Substring(0,40).ToUpperInvariant();
			string fileCapi=Path.Combine(_folder,"capi-key-file");
			string fileCng=Path.Combine(_folder,"cng-key-file");
			string output=RunDriver(new[] { "Get-TestNetworkState","Get-TestNetworkRevertPlan","ConvertFrom-KeyRecord","ConvertTo-KeyRecord","Find-MachineCertificate",
				"Remove-TestClientCertificate","Get-CreatedKey","Get-ManualCertificateRemoval","Get-ErrorText" },new[] {
				"function Test-TestAdapterExists([string]$InstanceId) { $false }",
				"function Test-HasAddress([string]$Address) { $false }",
				"$capiFile = Join-Path $Folder 'capi-key-file'",
				"$cngFile = Join-Path $Folder 'cng-key-file'",
				"[System.IO.File]::WriteAllText($capiFile, 'synthetic')",
				"$capiKey = ConvertTo-KeyRecord ([pscustomobject]@{ Provider = $CapiProviderName; Container = 'dbtls-env-capi'; File = $capiFile })",
				"$cngKey = ConvertTo-KeyRecord ([pscustomobject]@{ Provider = $CngProviderName; Container = 'dbtls-env-cng'; File = $cngFile })",
				"$r = [pscustomobject]@{ Address = '192.0.2.25'; AdapterId = 'ROOT\\NET\\9999'; FirewallRules = @(); CapiThumbprint = '"+thumbprintCapi+"'; CngThumbprint = '"
					+thumbprintCng+"'; CapiKey = $capiKey; CngKey = $cngKey }",
				"$state = Get-TestNetworkState $r ''",
				"'EXISTS ' + (@($state.Certificates | ForEach-Object { $_.Kind + '=' + $_.Exists + '/' + $_.KeyExists }) -join ' ')",
				"[System.IO.File]::WriteAllLines((Join-Path $Folder 'revert.txt'), [string[]](Get-TestNetworkRevertPlan $r $state))",
				"$script:deleted = @()",
				"function Remove-TestKey($Key) { $script:deleted += ($Key.Kind + ' ' + $Key.Provider + ' ' + $Key.ProviderType + ' ' + $Key.Container); Remove-Item -LiteralPath $Key.File }",
				"$capi = ConvertFrom-KeyRecord $r.CapiKey 'CAPI'",
				"'REMOVED ' + ((Remove-TestClientCertificate $r.CapiThumbprint $capi) -eq '') + ' ' + (Test-Path -LiteralPath $capiFile) + ' ' + ($script:deleted -join ',')",
				"[System.IO.File]::WriteAllText($capiFile, 'synthetic')",
				"function Remove-TestKey($Key) { }",
				"'LEFT ' + (Remove-TestClientCertificate $r.CapiThumbprint $capi)",
				"'NOTHING ' + ((Remove-TestClientCertificate $r.CngThumbprint (ConvertFrom-KeyRecord $r.CngKey 'CNG')) -eq '')",
				"$created = [pscustomobject]@{ Thumbprints = @($r.CapiThumbprint, $r.CngThumbprint); Keys = @([pscustomobject]@{ Thumbprint = $r.CapiThumbprint; Key = $capi }) }",
				"'MANUAL ' + (Get-ManualCertificateRemoval $r.CapiThumbprint (Get-CreatedKey $created $r.CapiThumbprint))",
				"'MANUALUNKNOWN ' + (Get-ManualCertificateRemoval $r.CngThumbprint (Get-CreatedKey $created $r.CngThumbprint))",
			});
			StringAssert.Contains(output,"EXISTS CAPI=False/True CNG=False/False\r\n","Both certificates are gone; the CAPI key file is still there.");
			string revert=File.ReadAllText(Path.Combine(_folder,"revert.txt"));
			StringAssert.Contains(revert,"#. [client-certificates] The CAPI certificate "+thumbprintCapi+" is not in LocalMachine\\My any more, but its key");
			StringAssert.Contains(revert,"file "+fileCapi+" is still there (BUILTIN\\Users can read it): delete that key (the container dbtls-env-capi");
			StringAssert.Contains(revert,"in 'Microsoft Enhanced RSA and AES Cryptographic Provider', once it opens as that very file) and check that the file is gone.");
			StringAssert.Contains(revert,"#. [client-certificates] The CNG certificate "+thumbprintCng+" is not in LocalMachine\\My any more, and its key");
			StringAssert.Contains(revert,"file "+fileCng+" is gone too: nothing to remove.");
			Assert.IsFalse(revert.Contains(thumbprintCapi+" is not in LocalMachine\\My any more: nothing to remove"),"Not 'nothing to remove' while the key file is there.");
			StringAssert.Contains(output,"REMOVED True False CAPI Microsoft Enhanced RSA and AES Cryptographic Provider 24 dbtls-env-capi\r\n",
				"With the certificate gone, the removal deletes the recorded key (the CSP's type for CspParameters) and checks that the file is gone.");
			StringAssert.Contains(output,"LEFT the key file "+fileCapi+" of "+thumbprintCapi+" is still there\r\n","A key file that stays is reported, never passed over.");
			StringAssert.Contains(output,"NOTHING True\r\n");
			//What a failed undo (exit 4) tells the owner: the key first, by its provider and container, then the store entry; not certlm.msc.
			StringAssert.Contains(output,"MANUAL the certificate "+thumbprintCapi+" with its key file "+fileCapi+": first certutil -csp \"Microsoft Enhanced RSA and AES "
				+"Cryptographic Provider\" -delkey \"dbtls-env-capi\", then certutil -delstore My "+thumbprintCapi+", and check that the key file is gone\r\n");
			StringAssert.Contains(output,"MANUALUNKNOWN the certificate "+thumbprintCng+": if certutil -store My "+thumbprintCng+" shows it, first delete its key with "
				+"certutil -csp \"<its Provider>\" -delkey <its Key Container>");
			string script=TestHelpers.ReadRepoFile("tools","Enable-LocalDbTls.ps1");
			Assert.IsFalse(script.Contains("with their keys (certlm.msc)"),"certlm.msc's Delete leaves the key file.");
			StringAssert.Contains(script,"Write-Host \"  $(Get-ManualCertificateRemoval $thumbprint (Get-CreatedKey $created $thumbprint)).\"");
			StringAssert.Contains(script,"$left = Remove-TestClientCertificate $thumbprint (Get-CreatedKey $Created $thumbprint)");
			StringAssert.Contains(script,"Keys = $recordedKeys;","-Revert passes the recorded keys to the removal.");
			StringAssert.Contains(script,"$Created.Keys = @($Created.Keys) + @([pscustomobject]@{ Thumbprint = $item[3]; Key = $info })","The enable run's undo knows each key.");
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
			if(DevDbTlsScriptTests.IsLeftByAnEnableRun(output)) {
				Assert.Inconclusive("An enable run that did not finish left its opms-dev-tls folder (until the owner's -Revert), so the plan is not shown: "+output);
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
			StringAssert.Contains(output,"# test-client-capi-key=Microsoft Enhanced RSA and AES Cryptographic Provider|<its key container>|<its key file in ");
			StringAssert.Contains(output,"# test-client-cng-key=Microsoft Software Key Storage Provider|<its key name>|<its key file in ");
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
			//Add-Type compiles the source with the .NET Framework's own compiler. Create and Remove are never called; only Exists and GetHardwareIds,
			//which only read (SetupDiOpenDeviceInfo, no elevation): a device id no computer has, and the root of the device tree that every one has.
			string output=RunDriver(new[] { "Get-LoopbackAdapterSource","Add-LoopbackAdapterType","Test-TestAdapterExists","Get-TestAdapterHardwareIds" },new[] {
				"Add-LoopbackAdapterType",
				"$t = 'OpenPmsDentalDevDbTls.LoopbackAdapter' -as [type]",
				"'METHODS ' + ((@($t.GetMethods() | Where-Object { $_.DeclaringType -eq $t } | ForEach-Object { $_.Name }) | Sort-Object) -join ',')",
				"'HWID ' + $t.GetField('HardwareId').GetValue($null)",
				@"'EXISTS missing ' + (Test-TestAdapterExists 'ROOT\OPMSDBTLSNOSUCHDEVICE\0000') + ' root ' + (Test-TestAdapterExists 'HTREE\ROOT\0')",
				@"$ids = @(Get-TestAdapterHardwareIds 'HTREE\ROOT\0')",
				"'IDS ' + @(@($ids) | Where-Object { $_ -isnot [string] }).Count",
			});
			StringAssert.Contains(output,"METHODS Create,Exists,GetHardwareIds,Remove\r\n");
			StringAssert.Contains(output,"EXISTS missing False root True\r\n","Exists tells a device Windows knows from one it does not, without Get-PnpDevice.");
			StringAssert.Contains(output,"IDS 0\r\n","The hardware ids come one by one, each a string, not a list inside a list.");
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

		///<summary>The driver lines of a journal at $journal (in the test's temporary folder) with what an enable run that got as far as both keys
		///wrote: the address, the adapter, both rules, both thumbprints and both keys (key "files" named in the computer's key folders, never
		///touched). Set-ProtectedAcl is replaced: the journal's ACL is not set here (that needs elevation).</summary>
		private static string[] GetJournalLines(string adapterId=@"ROOT\NET\0042") {
			return new[] {
				"function Set-ProtectedAcl($Path, $FullSids, $ReadSids) { Write-Host ('ACL ' + (Split-Path -Leaf $Path) + ' full ' + @($FullSids).Count + ' read ' + @($ReadSids).Count) }",
				"function KeyLine([string]$Kind, [string]$Provider) { ConvertTo-KeyRecord ([pscustomobject]@{ Provider = $Provider; Container = ('dbtls-env-' + $Kind.ToLower()); File = (Join-Path (Get-MachineKeyFolder $Kind) ('keyfile-' + $Kind.ToLower())) }) }",
				"foreach ($line in @(($RecordAddressPrefix + '192.0.2.25'), ($RecordAdapterPrefix + '"+adapterId+"'), ($RecordFirewallPrefix + $FirewallAllowName), ($RecordFirewallPrefix + $FirewallBlockName),",
				"  ($RecordCapiPrefix + ('A' * 40)), ($RecordCapiKeyPrefix + (KeyLine 'CAPI' $CapiProviderName)), ($RecordCngPrefix + ('B' * 40)), ($RecordCngKeyPrefix + (KeyLine 'CNG' $CngProviderName)))) {",
				"  Add-TestNetworkJournalLine $journal $line",
				"}",
			};
		}

		[TestMethod]
		public void TestNetwork_TheJournal_RecordsEachItemBeforeItIsMade_AndReadsBackForRevert() {
			//Install-TestNetwork with every step that changes this computer replaced: the adapter's creation by a type of the same name that only
			//reads the journal, the adapter's settings, the firewall rules, the import and the users' read access by functions that only look at
			//the journal. Nothing is created, imported or changed; the .pfx "files" are three bytes in the test's temporary folder.
			string output=RunDriver(new[] { "Install-TestNetwork","Get-TestNetworkJournalPath","Add-TestNetworkJournalLine","Add-TestFirewallRules","Get-FirewallBlockRanges",
				"ConvertTo-KeyRecord","ConvertFrom-KeyRecord","Get-MachineKeyFolder","Read-TestNetworkJournal","ConvertFrom-TestNetworkLines","Get-TestNetworkRecordProblem",
				"Get-TestAddressFormProblem","Get-KeyRecordProblem","Get-TestNetworkRevertPlan" },new[] {
				"function Set-ProtectedAcl($Path, $FullSids, $ReadSids) { Write-Host ('ACL ' + (Split-Path -Leaf $Path) + ' full ' + @($FullSids).Count + ' read ' + @($ReadSids).Count) }",
				"function Add-LoopbackAdapterType { }",
				"Add-Type -TypeDefinition 'namespace OpenPmsDentalDevDbTls { public static class LoopbackAdapter { public static string JournalPath; public static string Seen; public static string Create(string description, out bool needsReboot) { Seen = System.IO.File.Exists(JournalPath) ? System.IO.File.ReadAllText(JournalPath) : \"<none>\"; needsReboot = false; return @\"ROOT\\NET\\0042\"; } } }'",
				"function J([string]$Path) { if (Test-Path -LiteralPath $Path) { ([System.IO.File]::ReadAllText($Path)) -replace \"`r`n\", '|' } else { '<none>' } }",
				"function Set-TestAdapter($InstanceId, $Address) { Write-Host ('SETADAPTER ' + $InstanceId + ' journal ' + (J $script:journal)); 7 }",
				"function Get-TestAdapterProblem($Index, $Address) { $null }",
				"function New-NetFirewallRule { param($Name, $DisplayName, $Action, $RemoteAddress, $Group, $Direction, $Protocol, $LocalPort, $LocalAddress, $Profile, $Enabled)",
				"  Write-Host ('RULE ' + $Name + ' recorded first ' + (J $script:journal).Contains($RecordFirewallPrefix + $Name)) }",
				"function Import-TestClientCertificate($PfxPath, $Password, $Provider, $Kind, $Thumbprint) {",
				"  Write-Host ('IMPORT ' + $Kind + ' thumbprint recorded first ' + (J $script:journal).Contains($Thumbprint) + ' pfx there ' + (Test-Path -LiteralPath $PfxPath))",
				"  [pscustomobject]@{ Kind = $Kind; Provider = $Provider; Container = ('dbtls-env-' + $Kind.ToLower()); File = (Join-Path (Get-MachineKeyFolder $Kind) ('keyfile-' + $Kind.ToLower())); IsMachine = $true }",
				"}",
				"function Grant-UsersReadToKeyFile($Path) {",
				"  Write-Host ('GRANT ' + (Split-Path -Leaf $Path) + ' key recorded first ' + (J $script:journal).Contains($Path.Replace('\\', '/')))",
				"  if ($script:crashAt -ne '' -and $Path.EndsWith($script:crashAt)) { throw 'synthetic crash before the users get read access' }",
				"}",
				"function Run([string]$Name, [string]$CrashAt) {",
				"  $script:crashAt = $CrashAt",
				"  $tls = Join-Path $Folder $Name",
				"  New-Item -ItemType Directory -Path $tls | Out-Null",
				"  $script:journal = Join-Path $tls $TestNetworkJournalName",
				"  [OpenPmsDentalDevDbTls.LoopbackAdapter]::JournalPath = $script:journal",
				"  $net = [pscustomobject]@{ Address = '192.0.2.25'; AdapterId = ''; FirewallRules = @($FirewallAllowName, $FirewallBlockName); CapiThumbprint = ('A' * 40); CngThumbprint = ('B' * 40); CapiKey = ''; CngKey = '' }",
				"  $set = [pscustomobject]@{ TestCapiPfx = [byte[]](1, 2, 3); TestCngPfx = [byte[]](4, 5, 6); TestCapiThumbprint = ('A' * 40); TestCngThumbprint = ('B' * 40); TestPfxPassword = 'synthetic' }",
				"  $created = [pscustomobject]@{ AdapterId = ''; FirewallRules = @(); Thumbprints = @(); Keys = @(); PfxFolder = '' }",
				"  try { Install-TestNetwork ([pscustomobject]@{ TlsFolder = $tls; TestNetwork = $net }) $set $created; Write-Host ($Name + ' INSTALLED') } catch { Write-Host ($Name + ' STOPPED ' + $_.Exception.Message) }",
				"  Write-Host ($Name + ' SEENATCREATE ' + ([OpenPmsDentalDevDbTls.LoopbackAdapter]::Seen -replace \"`r`n\", '|'))",
				"  Write-Host ($Name + ' JOURNAL ' + (J $script:journal))",
				"  $r = Read-TestNetworkJournal $script:journal",
				"  Write-Host ($Name + ' READ ' + $r.Address + ' ' + $r.AdapterId + ' ' + (@($r.FirewallRules) -join ',') + ' ' + ($r.CapiThumbprint -eq ('A' * 40)) + ' ' + ($r.CngThumbprint -eq ('B' * 40)) + ' ' + ($r.CapiKey -ne '') + ' ' + ($r.CngKey -ne ''))",
				"  Write-Host ($Name + ' PROBLEM ' + ($null -eq (Get-TestNetworkRecordProblem $r $script:journal -Partial)) + ' ' + ($null -eq (Get-TestNetworkRecordProblem $r)))",
				"}",
				"Run 'whole' ''",
				"Run 'crash' 'keyfile-cng'",
				//A journal of a run that stopped right after the address line (before the adapter was made): accepted as partial, and -Revert's plan
				//says there is nothing of the adapter, the certificates or the rules to remove; a journal with a line no run writes is refused.
				"$early = Join-Path $Folder 'early.record'",
				"Add-TestNetworkJournalLine $early ($RecordAddressPrefix + '192.0.2.25')",
				"$e = Read-TestNetworkJournal $early",
				"'EARLY ' + ($null -eq (Get-TestNetworkRecordProblem $e $early -Partial)) + ' ' + (Get-TestNetworkRecordProblem $e $early)",
				"$state = [pscustomobject]@{ Adapter = [pscustomobject]@{ Exists = $false; HardwareIds = @(); Addresses = @() }; Rules = @(); Certificates = @(); HasCa = $true }",
				"[System.IO.File]::WriteAllLines((Join-Path $Folder 'early.txt'), [string[]](Get-TestNetworkRevertPlan $e $state -FromJournal))",
				"$bad = Join-Path $Folder 'bad.record'",
				"Add-TestNetworkJournalLine $bad ($RecordAddressPrefix + '192.0.2.25')",
				"Add-TestNetworkJournalLine $bad ($RecordAdapterPrefix + 'PCI\\VEN_8086&DEV_15B8\\3&11583659&0&FE')",
				"'BAD ' + (Get-TestNetworkRecordProblem (Read-TestNetworkJournal $bad) $bad -Partial)",
				"$keyOnly = Join-Path $Folder 'keyonly.record'",
				"Add-TestNetworkJournalLine $keyOnly ($RecordAddressPrefix + '192.0.2.25')",
				"Add-TestNetworkJournalLine $keyOnly ($RecordCngKeyPrefix + $CngProviderName + '|x|' + (Join-Path (Get-MachineKeyFolder 'CNG') 'k'))",
				"'KEYONLY ' + (Get-TestNetworkRecordProblem (Read-TestNetworkJournal $keyOnly) $keyOnly -Partial)",
				"'NOJOURNAL ' + ($null -eq (Read-TestNetworkJournal (Join-Path $Folder 'none.record')))",
			});
			//Write-Host lines end with a bare line feed.
			output=output.Replace("\r\n","\n");
			string journalWhole=Path.Combine(_folder,"whole","test-network.record");
			//Each item's line is in the journal before the item is made.
			StringAssert.Contains(output,"whole SEENATCREATE # test-address=192.0.2.25|\n","Only the address before the adapter is created.");
			StringAssert.Contains(output,@"SETADAPTER ROOT\NET\0042 journal # test-address=192.0.2.25|# test-adapter=ROOT\NET\0042|","The adapter's id right after its creation.");
			StringAssert.Contains(output,"RULE OpenPmsDental-DbTlsTest-Allow3306 recorded first True\n");
			StringAssert.Contains(output,"RULE OpenPmsDental-DbTlsTest-Block3306 recorded first True\n");
			StringAssert.Contains(output,"IMPORT CAPI thumbprint recorded first True pfx there True\n");
			StringAssert.Contains(output,"IMPORT CNG thumbprint recorded first True pfx there True\n");
			StringAssert.Contains(output,"GRANT keyfile-capi key recorded first True\n","Each key before the users get read access to it.");
			StringAssert.Contains(output,"GRANT keyfile-cng key recorded first True\n");
			StringAssert.Contains(output,"ACL test-network.record full 0 read 0\n","The journal gets a protected ACL: Administrators and SYSTEM only.");
			StringAssert.Contains(output,"whole INSTALLED\n");
			string capiKey="Microsoft Enhanced RSA and AES Cryptographic Provider|dbtls-env-capi|"
				+(Environment.GetEnvironmentVariable("ProgramData")+@"\Microsoft\Crypto\RSA\MachineKeys\keyfile-capi").Replace('\\','/');
			StringAssert.Contains(output,"whole JOURNAL # test-address=192.0.2.25|# test-adapter=ROOT\\NET\\0042|# test-firewall-rule=OpenPmsDental-DbTlsTest-Allow3306|"
				+"# test-firewall-rule=OpenPmsDental-DbTlsTest-Block3306|# test-client-capi="+new string('A',40)+"|# test-client-capi-key="+capiKey+"|# test-client-cng="
				+new string('B',40)+"|# test-client-cng-key=Microsoft Software Key Storage Provider|dbtls-env-cng|");
			StringAssert.Contains(output,@"whole READ 192.0.2.25 ROOT\NET\0042 OpenPmsDental-DbTlsTest-Allow3306,OpenPmsDental-DbTlsTest-Block3306 True True True True");
			StringAssert.Contains(output,"whole PROBLEM True True\n","The journal of a whole run is a record that -Revert accepts.");
			//A run that crashes after the CNG import, before the users' read access: the journal already names that key.
			StringAssert.Contains(output,"crash STOPPED synthetic crash before the users get read access\n");
			StringAssert.Contains(output,@"crash READ 192.0.2.25 ROOT\NET\0042 OpenPmsDental-DbTlsTest-Allow3306,OpenPmsDental-DbTlsTest-Block3306 True True True True");
			Assert.IsTrue(File.Exists(journalWhole));
			StringAssert.Contains(output,"EARLY True "+Path.Combine(_folder,"early.record")+" records the test adapter '', which is not a device this script creates",
				"Only as a journal (-Partial) may the adapter be missing.");
			string early=File.ReadAllText(Path.Combine(_folder,"early.txt"));
			StringAssert.Contains(early,"[adapter] The record names no loopback adapter (the run stopped before it made one): nothing to remove.");
			StringAssert.Contains(early,"[client-certificates] The record names no test client certificate (the run stopped before it imported one): nothing to remove.");
			StringAssert.Contains(early,"[firewall] No firewall rule to remove");
			StringAssert.Contains(early,"my.ini has no block of this script (the run stopped before it wrote it, or its undo restored my.ini)");
			CollectionAssert.AreEqual(_arrayTags.OrderBy(x => x).ToList(),GetTags(early),"Every tag, also for a journal of a run that stopped early: "+early);
			StringAssert.Contains(output,"BAD "+Path.Combine(_folder,"bad.record")+@" records the test adapter 'PCI\VEN_8086&DEV_15B8\3&11583659&0&FE', which is not a device");
			StringAssert.Contains(output,"KEYONLY "+Path.Combine(_folder,"keyonly.record")+" records a key of the CNG test client certificate, but not its thumbprint.");
			StringAssert.Contains(output,"NOJOURNAL True\n");
		}

		///<summary>Runs Invoke-Revert in a driver with a journal in a scratch opms-dev-tls folder and no block in a scratch my.ini: everything that
		///would change this computer (the service, the test network's removal, the files' removal, the confirmation) is replaced by functions that
		///print what they were given, and the state of the journal's items is synthetic (present until the replaced undo "removes" them, unless
		///<paramref name="isUndoLeavingItems"/>). With <paramref name="hasBackup"/> a backup of my.ini is there (an undo that restored my.ini
		///and then failed at the restart); without, there is none (a run interrupted before it copied my.ini).</summary>
		private string RunRevertWithJournal(string name,bool hasBackup,bool isUndoLeavingItems,int exitCodeExpected) {
			List<string> listBody=new List<string> {
				"$root = Join-Path $Folder '"+name+"'",
				"$tls = Join-Path $root 'opms-dev-tls'",
				"New-Item -ItemType Directory -Path $tls | Out-Null",
				"$journal = Join-Path $tls $TestNetworkJournalName",
			};
			listBody.AddRange(GetJournalLines());
			listBody.AddRange(new[] {
				"$script:Facts = [pscustomobject]@{ Ini = (Join-Path $root 'my.ini'); Home = $root }",
				"[System.IO.File]::WriteAllText($script:Facts.Ini, \"[mysqld]`r`nport=3306`r`nbind-address=127.0.0.1`r`n\")",
				"$script:IniFile = [pscustomobject]@{ Text = [System.IO.File]::ReadAllText($script:Facts.Ini); Newline = \"`r`n\" }",
				"$DryRun = $false",
				(hasBackup ? "Copy-Item -LiteralPath $script:Facts.Ini -Destination ($script:Facts.Ini + $BackupSuffix)" : "'NO BACKUP'"),
				"function Assert-LocalServer { }",
				"function Get-Paths([bool]$ForEnable) { [pscustomobject]@{ TlsFolder = $tls; Ca = (Join-Path $tls 'ca.pem'); DevFolder = (Join-Path $root 'dev'); DevCa = (Join-Path $root 'dev\\ca.pem')",
				"  ClientFolder = ''; ClientCert = ''; ClientKey = ''; IsRecorded = $false; CreatedParents = @(); Backup = ($script:Facts.Ini + $BackupSuffix); IniTemp = ($script:Facts.Ini + $IniTempSuffix)",
				"  TestNetwork = (Get-RecordedTestNetwork $script:IniFile.Text) } }",
				"$script:present = $true",
				"$script:undoClears = "+(isUndoLeavingItems ? "$false" : "$true"),
				"function Get-TestNetworkState($Record, [string]$CaPath) {",
				"  Write-Host ('STATE ' + $Record.AdapterId + ' present ' + $script:present + ' ca ' + (Split-Path -Leaf $CaPath))",
				"  $certificates = @()",
				"  foreach ($item in @(@('CAPI', $Record.CapiThumbprint, $TestClientCapiName, $Record.CapiKey), @('CNG', $Record.CngThumbprint, $TestClientCngName, $Record.CngKey))) {",
				"    $certificates += [pscustomobject]@{ Kind = $item[0]; Thumbprint = $item[1]; Name = $item[2]; Exists = $script:present; Subject = ('CN=' + $item[2] + ', O=Open PMS - Dental local test')",
				"      IssuedByCa = $true; Key = (ConvertFrom-KeyRecord $item[3] $item[0]); KeyExists = $script:present }",
				"  }",
				"  [pscustomobject]@{ Adapter = [pscustomobject]@{ Exists = $script:present; HardwareIds = @('*MSLOOP'); Addresses = @($Record.Address); AddressThere = $script:present }",
				"    Rules = @($Record.FirewallRules | ForEach-Object { [pscustomobject]@{ Name = $_; Exists = $script:present; Group = $FirewallGroup } }); Certificates = $certificates; HasCa = $true }",
				"}",
				"function Confirm-Change([string]$Question) { Write-Host ('CONFIRM ' + $Question) }",
				"function Remove-TestAdapter($InstanceId, $Address) { Write-Host ('UNDO ADAPTER ' + $InstanceId + ' ' + $Address) }",
				"function Remove-TestClientCertificate($Thumbprint, $RecordedKey) { Write-Host ('UNDO CERT ' + $Thumbprint.Substring(0, 4) + ' ' + $RecordedKey.Container); '' }",
				"function Remove-TestFirewallRule($Name) { Write-Host ('UNDO RULE ' + $Name); if ($script:undoClears -and $Name -eq $FirewallBlockName) { $script:present = $false } }",
				"function Restart-DbService { Write-Host 'RESTART' }",
				"function Get-ServerState([string]$Tls) { [pscustomobject]@{ Ok = $true; Error = '' } }",
				"function Read-OptionFile([string]$Path) { [pscustomobject]@{ Text = [System.IO.File]::ReadAllText($Path) } }",
				"function Get-NetTCPConnection { @() }",
				"function Remove-NewFiles($Paths) { Write-Host ('REMOVEFILES ' + (Split-Path -Leaf $Paths.TlsFolder) + ' journal there ' + (Test-Path -LiteralPath $journal)) }",
				"Invoke-Revert",
			});
			return DevDbTlsScriptTests.RunDriverIn(_folder,new[] { "Invoke-Revert","Get-TestNetworkJournalPath","Read-TestNetworkJournal","ConvertFrom-TestNetworkLines",
				"Get-RecordedTestNetwork","Get-TestNetworkRecordProblem","Get-TestAddressFormProblem","Get-KeyRecordProblem","ConvertFrom-KeyRecord","ConvertTo-KeyRecord",
				"Get-MachineKeyFolder","Test-HasBlock","Remove-Block","Get-TestNetworkStateProblem","Get-TestNetworkRevertPlan","Get-TestNetworkLeft","Get-DevFiles",
				"Get-CreatedParents","Add-TestNetworkJournalLine","Undo-TestNetwork","Get-CreatedKey","Get-ErrorText","Test-FolderReadable","Get-TestNetworkRemovedLines" },listBody.ToArray(),exitCodeExpected);
		}

		[TestMethod]
		public void TestNetwork_Revert_AJournalWithoutABlock_RemovesTheAdapterTheRulesAndTheKeys_AlsoWithoutABackup() {
			//(b) An interrupted enable run: no backup, no block, the journal. -Revert does not stop at the 'backup is missing' refusal: it plans and
			//runs the test network's removal from the journal, then the files, and leaves my.ini and the service alone.
			string output=RunRevertWithJournal("interrupted",hasBackup:false,isUndoLeavingItems:false,exitCodeExpected:0);
			Assert.IsFalse(output.Contains("REFUSED"),output);
			Assert.IsFalse(output.Contains("is missing, so the original my.ini cannot be restored"),output);
			string fileCapiKey=Environment.GetEnvironmentVariable("ProgramData")+@"\Microsoft\Crypto\RSA\MachineKeys\keyfile-capi";
			string fileCngKey=Environment.GetEnvironmentVariable("ProgramData")+@"\Microsoft\Crypto\Keys\keyfile-cng";
			StringAssert.Contains(output,"an enable run stopped before it");
			StringAssert.Contains(output,"First the test network 192.0.2.25 of an enable run that did not finish, as its journal "
				+Path.Combine(_folder,"interrupted","opms-dev-tls","test-network.record")+" records it");
			StringAssert.Contains(output,@"[adapter] Remove the loopback adapter ROOT\NET\0042 ('OPMS DB TLS test') first");
			StringAssert.Contains(output,"[firewall] Remove the firewall rules OpenPmsDental-DbTlsTest-Allow3306 and OpenPmsDental-DbTlsTest-Block3306");
			StringAssert.Contains(output,"and delete its key first (the recorded file "+fileCapiKey+").");
			StringAssert.Contains(output,"and delete its key first (the recorded file "+fileCngKey+").");
			CollectionAssert.AreEqual(_arrayTags.OrderBy(x => x).ToList(),GetTags(output),"-Revert's plan from the journal undoes every step: "+output);
			//The script's own Undo-TestNetwork, with only the removals replaced: the adapter first, then the certificates with the journal's keys, the
			//rules last; and an undo that leaves nothing is taken as done (the empty list, not a list that holds it).
			int indexUndoAdapter=output.IndexOf(@"UNDO ADAPTER ROOT\NET\0042 192.0.2.25",StringComparison.Ordinal);
			int indexUndoCapi=output.IndexOf("UNDO CERT AAAA dbtls-env-capi",StringComparison.Ordinal);
			int indexUndoCng=output.IndexOf("UNDO CERT BBBB dbtls-env-cng",StringComparison.Ordinal);
			int indexUndoAllow=output.IndexOf("UNDO RULE OpenPmsDental-DbTlsTest-Allow3306",StringComparison.Ordinal);
			int indexUndoBlock=output.IndexOf("UNDO RULE OpenPmsDental-DbTlsTest-Block3306",StringComparison.Ordinal);
			Assert.IsTrue(indexUndoAdapter>0 && indexUndoCapi>indexUndoAdapter && indexUndoCng>indexUndoCapi && indexUndoAllow>indexUndoCng
				&& indexUndoBlock>indexUndoAllow,"The adapter first, the rules last: "+output);
			StringAssert.Contains(output,@"Removed of the test network 192.0.2.25: the loopback adapter ROOT\NET\0042 with its address 192.0.2.25, the CAPI certificate "
				+new string('A',40)+" with its key file "+fileCapiKey+", the CNG certificate "+new string('B',40)+" with its key file "+fileCngKey
				+", the firewall rule OpenPmsDental-DbTlsTest-Allow3306, the firewall rule OpenPmsDental-DbTlsTest-Block3306.");
			Assert.IsFalse(output.Contains("Already gone before this run"),"Everything was there: "+output);
			Assert.IsFalse(output.Contains("is not fully removed"),output);
			Assert.IsFalse(output.Contains("RESTART"),"my.ini and the service are not touched: "+output);
			Assert.IsFalse(output.Contains("Restore "),"No restore of my.ini without a backup: "+output);
			int indexConfirm=output.IndexOf("CONFIRM",StringComparison.Ordinal);
			int indexUndo=output.IndexOf("UNDO ",StringComparison.Ordinal);
			int indexGone=output.IndexOf(@"STATE ROOT\NET\0042 present False",StringComparison.Ordinal);
			int indexFiles=output.IndexOf("REMOVEFILES opms-dev-tls journal there True",StringComparison.Ordinal);
			Assert.IsTrue(indexConfirm>0 && indexUndo>indexConfirm && indexGone>indexUndo && indexFiles>indexGone,
				"The test network goes first, the journal's items are checked gone, and only then the opms-dev-tls folder: "+output);
			StringAssert.Contains(output,"Removed what the enable run that did not finish had made; my.ini and the service were not touched.");
			//(a) An undo that restored my.ini and then failed at the restart: a backup, no block, the journal. -Revert removes the test network from
			//the journal, then restores my.ini, restarts and removes the files.
			string outputBackup=RunRevertWithJournal("restartfailed",hasBackup:true,isUndoLeavingItems:false,exitCodeExpected:0);
			Assert.IsFalse(outputBackup.Contains("REFUSED"),outputBackup);
			StringAssert.Contains(outputBackup,"as its journal "+Path.Combine(_folder,"restartfailed","opms-dev-tls","test-network.record")+" records it");
			StringAssert.Contains(outputBackup,@"[adapter] Remove the loopback adapter ROOT\NET\0042");
			CollectionAssert.AreEqual(_arrayTags.OrderBy(x => x).ToList(),GetTags(outputBackup),outputBackup);
			int indexUndoBackup=outputBackup.IndexOf("UNDO ",StringComparison.Ordinal);
			int indexRestart=outputBackup.IndexOf("RESTART",StringComparison.Ordinal);
			int indexFilesBackup=outputBackup.IndexOf("REMOVEFILES",StringComparison.Ordinal);
			Assert.IsTrue(indexUndoBackup>0 && indexRestart>indexUndoBackup && indexFilesBackup>indexRestart,outputBackup);
			StringAssert.Contains(outputBackup,"TLS is off again");
			Assert.IsFalse(File.Exists(Path.Combine(_folder,"restartfailed","my.ini.before-opms-dev-tls")),"The backup is removed at the end.");
			//An item the journal names is still there after the undo: -Revert stops (exit 4), the opms-dev-tls folder with the journal and ca.pem
			//stays, and nothing else is changed.
			string outputLeft=RunRevertWithJournal("left",hasBackup:true,isUndoLeavingItems:true,exitCodeExpected:4);
			StringAssert.Contains(outputLeft,@"FAILED: the test network is not fully removed: the loopback adapter ROOT\NET\0042 (named in the journal ");
			StringAssert.Contains(outputLeft,"the key file "+fileCapiKey+" (named in the journal ");
			StringAssert.Contains(outputLeft,"stays with it, so -Revert can run again once that is put right.");
			Assert.IsFalse(outputLeft.Contains("REMOVEFILES"),"The opms-dev-tls folder is never deleted while the journal names an item that is there: "+outputLeft);
			Assert.IsFalse(outputLeft.Contains("RESTART"),outputLeft);
			Assert.IsTrue(File.Exists(Path.Combine(_folder,"left","my.ini.before-opms-dev-tls")),"The backup stays.");
			Assert.IsTrue(File.Exists(Path.Combine(_folder,"left","opms-dev-tls","test-network.record")),"The journal stays.");
		}

		[TestMethod]
		public void TestNetwork_UndoTestNetwork_TheAdapterFirst_TheRulesLast_AndKeptWhileTheAdapterStays() {
			//The server may still listen on the test address while -Revert runs (my.ini is restored afterwards), so the block rule goes only once the
			//address is gone. Every removal is replaced by a function that only notes its call.
			string output=RunDriver(new[] { "Undo-TestNetwork","Get-CreatedKey","Get-ErrorText" },new[] {
				"$script:calls = @()",
				"$script:adapterFails = $false",
				"function Remove-TestAdapter($InstanceId, $Address) { $script:calls += ('ADAPTER ' + $InstanceId + ' ' + $Address); if ($script:adapterFails) { throw 'DIF_REMOVE failed (synthetic)' } }",
				"function Remove-TestClientCertificate($Thumbprint, $RecordedKey) { $script:calls += ('CERT ' + $Thumbprint.Substring(0, 4) + ' ' + $RecordedKey.Container); '' }",
				"function Remove-TestFirewallRule($Name) { $script:calls += ('RULE ' + $Name) }",
				"$keys = @([pscustomobject]@{ Thumbprint = ('A' * 40); Key = [pscustomobject]@{ Container = 'capi' } }, [pscustomobject]@{ Thumbprint = ('B' * 40); Key = [pscustomobject]@{ Container = 'cng' } })",
				"$created = [pscustomobject]@{ AdapterId = 'ROOT\\NET\\0042'; FirewallRules = @($FirewallAllowName, $FirewallBlockName); Thumbprints = @(('A' * 40), ('B' * 40)); Keys = $keys; PfxFolder = '' }",
				"$p = Undo-TestNetwork $created '192.0.2.25'",
				"'OK calls: ' + ($script:calls -join ', ')",
				"'OK problems: ' + @($p).Count",
				"$script:calls = @()",
				"$script:adapterFails = $true",
				"$p = Undo-TestNetwork $created '192.0.2.25'",
				"'FAILS calls: ' + ($script:calls -join ', ')",
				"'FAILS problems: ' + (@($p) -join ' || ')",
			});
			StringAssert.Contains(output,@"OK calls: ADAPTER ROOT\NET\0042 192.0.2.25, CERT AAAA capi, CERT BBBB cng, RULE OpenPmsDental-DbTlsTest-Allow3306, RULE OpenPmsDental-DbTlsTest-Block3306"
				+"\r\n","The adapter first, then the certificates with their keys, the rules last.");
			StringAssert.Contains(output,"OK problems: 0\r\n");
			StringAssert.Contains(output,@"FAILS calls: ADAPTER ROOT\NET\0042 192.0.2.25, CERT AAAA capi, CERT BBBB cng"+"\r\n",
				"With the adapter still there, Remove-TestFirewallRule is never called; the certificates with their Users-readable keys still go.");
			//Each problem names the line it was raised on ($_.InvocationInfo.PositionMessage: here the stub's line in the driver).
			StringAssert.Contains(output,@"FAILS problems: the loopback adapter ROOT\NET\0042: DIF_REMOVE failed (synthetic) [At ");
			StringAssert.Contains(output,"throw 'DIF_REMOVE");
			StringAssert.Contains(output,"] || the firewall rules OpenPmsDental-DbTlsTest-Allow3306 and OpenPmsDental-DbTlsTest-Block3306 are kept, because the adapter "
				+"and its address 192.0.2.25 are still there");
		}

		[TestMethod]
		public void TestNetwork_AFailedRunsUndo_UndoesTheTestNetwork_AlsoWhenTheRestartFails_AndKeepsTheJournalWhileItIsNotUndone() {
			//Undo-FailedEnable and the script's own Undo-TestNetwork on scratch files (my.ini, its backup, the journal in the test's temporary
			//folder); the restart, the adapter's and the rule's removal and the files' undo are replaced by functions that note their call.
			string output=RunDriver(new[] { "Undo-FailedEnable","Get-TestNetworkJournalPath","Undo-TestNetwork","Get-CreatedKey","Get-ErrorText" },new[] {
				"function Restart-DbService { $script:calls += 'RESTART'; if ($script:restartFails) { throw 'the service did not start (synthetic)' } }",
				"function Remove-TestAdapter($InstanceId, $Address) { $script:calls += ('ADAPTER ' + $Address); if ($script:adapterFails) { throw 'synthetic' } }",
				"function Remove-TestFirewallRule($Name) { $script:calls += ('RULE ' + $Name) }",
				"function Undo-EnableFiles($Paths, $Saved, [switch]$KeepTlsFolder) { $script:calls += ('FILES keep ' + $KeepTlsFolder) }",
				"$created = [pscustomobject]@{ AdapterId = 'ROOT\\NET\\0042'; FirewallRules = @($FirewallAllowName); Thumbprints = @(); Keys = @(); PfxFolder = '' }",
				"function Case([string]$Name, [bool]$RestartFails, [bool]$AdapterFails) {",
				"  $script:calls = @(); $script:restartFails = $RestartFails; $script:adapterFails = $AdapterFails",
				"  $root = Join-Path $Folder $Name",
				"  $tls = Join-Path $root 'opms-dev-tls'",
				"  New-Item -ItemType Directory -Path $tls | Out-Null",
				"  $script:Facts = [pscustomobject]@{ Ini = (Join-Path $root 'my.ini') }",
				"  [System.IO.File]::WriteAllText($script:Facts.Ini, 'with the block')",
				"  [System.IO.File]::WriteAllText(($script:Facts.Ini + '.before'), 'the old my.ini')",
				"  [System.IO.File]::WriteAllText((Join-Path $tls $TestNetworkJournalName), '# test-address=192.0.2.25')",
				"  $paths = [pscustomobject]@{ TlsFolder = $tls; Backup = ($script:Facts.Ini + '.before') }",
				"  $u = Undo-FailedEnable $paths $null $created '192.0.2.25' $true",
				"  Write-Host ($Name + ' calls: ' + ($script:calls -join ', '))",
				"  Write-Host ($Name + ' problems: ' + (@($u.Problems) -join ' || '))",
				"  Write-Host ($Name + ' ini ' + $u.IniUndone + ' myini ' + [System.IO.File]::ReadAllText($script:Facts.Ini) + ' backup ' + (Test-Path -LiteralPath $paths.Backup) + ' journal ' + (Test-Path -LiteralPath $u.Journal))",
				"}",
				"Case 'restartfails' $true $false",
				"Case 'restartfailsnetleft' $true $true",
				"Case 'netleft' $false $true",
				"Case 'allundone' $false $false",
			});
			//Write-Host lines end with a bare line feed.
			output=output.Replace("\r\n","\n");
			//(a) The restart fails: the test network is still undone; the journal goes once that undo leaves nothing; the backup stays for -Revert.
			StringAssert.Contains(output,"restartfails calls: RESTART, ADAPTER 192.0.2.25, RULE OpenPmsDental-DbTlsTest-Allow3306\n",
				"The test network's undo runs although the restart failed.");
			StringAssert.Contains(output,"restartfails problems: my.ini and the service: the service did not start (synthetic) [At ");
			StringAssert.Contains(output,"restartfails ini False myini the old my.ini backup True journal False\n");
			//The restart fails and the test network is not undone: the journal stays, the only record once my.ini is the old file again.
			StringAssert.Contains(output,"restartfailsnetleft calls: RESTART, ADAPTER 192.0.2.25\n","The rule stays while the adapter does.");
			StringAssert.Contains(output,"restartfailsnetleft problems: my.ini and the service: the service did not start (synthetic) [At ");
			StringAssert.Contains(output,@"|| the test network could not be undone: the loopback adapter ROOT\NET\0042: synthetic [At ");
			StringAssert.Contains(output,"]; the firewall rules OpenPmsDental-DbTlsTest-Allow3306 are kept");
			StringAssert.Contains(output,"restartfailsnetleft ini False myini the old my.ini backup True journal True\n");
			//my.ini restored but the test network not undone: the files are undone, the opms-dev-tls folder (the journal, ca.pem) is kept.
			StringAssert.Contains(output,"netleft calls: RESTART, ADAPTER 192.0.2.25, FILES keep True\n");
			StringAssert.Contains(output,"netleft ini True myini the old my.ini backup False journal True\n");
			StringAssert.Contains(output,"allundone calls: RESTART, ADAPTER 192.0.2.25, RULE OpenPmsDental-DbTlsTest-Allow3306, FILES keep False\n");
			StringAssert.Contains(output,"allundone problems: \n");
			StringAssert.Contains(output,"allundone ini True myini the old my.ini backup False journal False\n");
			//The exit-4 text says that -Revert also undoes an interrupted run's test network, from the journal.
			string script=TestHelpers.ReadRepoFile("tools","Enable-LocalDbTls.ps1");
			StringAssert.Contains(script,"$undo = Undo-FailedEnable $paths $saved $created $testAddress $changedIni");
			StringAssert.Contains(script,"interrupted run''s test network: the adapter first, then both test certificates with their keys, the firewall rules last");
		}

		///<summary>The client's error in the owner's run of 2026-11-14 at the test address (root is root@localhost: MariaDB refuses another host
		///with 1130 before the TLS handshake, and the client wraps that in ERROR 2002).</summary>
		private const string OWNERS_RUN_ERROR="ERROR 2002 (HY000): Received error packet before completion of TLS handshake. The authenticity of the following "
			+"error cannot be verified: 1130 - Host '192.0.2.25' is not allowed to connect to this MariaDB server";

		///<summary>Invoke-Enable -WithClientCertificate -WithTestNetwork 192.0.2.25 in a driver, from its checks to its end, with the certificates
		///made for real (in memory, their files in the test's temporary folder) and every step that would change this computer replaced: the
		///folders' ACLs, the test network (Install-TestNetwork only notes what it would have made), the restart, the server's answers (TLS on, both
		///listeners, the bind address) and the client's check at the test address, which ends with <paramref name="clientError"/> ('' for
		///success). A failure's undo is replaced by one that only notes its call. Returns the output; asserts the exit code.</summary>
		private string RunEnableAtTheTestAddress(string name,string clientError,int exitCodeExpected) {
			string[] arrayBody=new[] {
				"$root = Join-Path $Folder '"+name+"'",
				"New-Item -ItemType Directory -Path $root | Out-Null",
				"$script:Facts = [pscustomobject]@{ Ini = (Join-Path $root 'my.ini'); Home = $root; Account = 'LocalSystem' }",
				"[System.IO.File]::WriteAllText($script:Facts.Ini, \"[mysqld]`r`nport=3306`r`nbind-address=127.0.0.1`r`ndatadir=C:/x/data`r`n\")",
				"$script:IniFile = Read-OptionFile $script:Facts.Ini",
				"$User = 'root'",
				"$DevUser = ''",
				"$WithTestNetwork = '192.0.2.25'",
				"$script:clientError = '"+clientError.Replace("'","''")+"'",
				"function Assert-LocalServer { [pscustomobject]@{ Values = @{ have_ssl = 'DISABLED'; datadir = 'C:/x/data'; version = '10.11.19-MariaDB' } } }",
				"$tls = Join-Path $root 'opms-dev-tls'",
				"function Get-Paths([bool]$ForEnable) { [pscustomobject]@{ TlsFolder = $tls; Ca = (Join-Path $tls 'ca.pem'); ServerCert = (Join-Path $tls 'server-cert.pem')",
				"  ServerKey = (Join-Path $tls 'server-key.pem'); DevFolder = (Join-Path $root 'dev'); DevCa = (Join-Path $root 'dev\\ca.pem'); ClientFolder = (Join-Path $root 'client')",
				"  ClientCert = (Join-Path $root 'client\\client-cert.pem'); ClientKey = (Join-Path $root 'client\\client-key.pem'); IsRecorded = $false; CreatedParents = @()",
				"  Backup = ($script:Facts.Ini + $BackupSuffix); IniTemp = ($script:Facts.Ini + $IniTempSuffix); TestNetwork = $null } }",
				"function Test-InsideRepository([string]$Path) { $false }",
				"function Get-LocalAppDataRoots { @() }",
				"function Get-FolderProblem { $null }",
				"function Get-CimInstance { [pscustomobject]@{ UserName = '' } }",
				"function Get-NetworkFacts { [pscustomobject]@{ Addresses = @(); Routes = @(); FirewallProfiles = @([pscustomobject]@{ Name = 'Domain'; Enabled = $true }); OpenInterfaces = @()",
				"  Loopbacks = @(); AdapterNameTaken = $false; RulesTaken = @(); HasNetloop = $true; HostName = 'testhost' } }",
				"function Confirm-Change([string]$Question) { Write-Host ('CONFIRM ' + $Question) }",
				"function Set-ProtectedAcl($Path, $FullSids, $ReadSids) { }",
				"function Install-TestNetwork($Paths, $Set, $Created) {",
				"  $Paths.TestNetwork.AdapterId = 'ROOT\\NET\\0000'; $Created.AdapterId = 'ROOT\\NET\\0000'",
				"  $Created.FirewallRules = @($FirewallAllowName, $FirewallBlockName); $Created.Thumbprints = @($Set.TestCapiThumbprint, $Set.TestCngThumbprint)",
				"  Write-Host 'INSTALLED (synthetic)'",
				"}",
				"function Restart-DbService { Write-Host 'RESTART (synthetic)' }",
				"function Get-ServerState([string]$Tls) { [pscustomobject]@{ Ok = $true; Error = ''; Values = @{ have_ssl = 'YES'; ssl_cipher = 'TLS_AES_256_GCM_SHA384'; ssl_version = 'TLSv1.3'",
				"  ssl_server_not_after = 'Nov 13 2031'; bind_address = '127.0.0.1,192.0.2.25' } } }",
				"function Get-TestListenerProblem([string]$Address, [int]$ServicePid) { $null }",
				"function Get-ServiceProcessId { 4242 }",
				"function Invoke-DbClient([string]$Sql, [string]$Tls = 'tls', [string]$ServerAddress = '127.0.0.1') {",
				"  Write-Host ('CLIENT ' + $ServerAddress + ' ca ' + (Split-Path -Leaf $Tls))",
				"  if ($script:clientError -eq '') { [pscustomobject]@{ ExitCode = 0; Lines = @('1'); Error = '' } } else { [pscustomobject]@{ ExitCode = 1; Lines = @(); Error = $script:clientError } }",
				"}",
				"function Undo-FailedEnable($Paths, $Saved, $Created, [string]$TestAddress, [bool]$ChangedIni) { Write-Host 'UNDO (synthetic)'; [pscustomobject]@{ Problems = @(); IniUndone = $true; NetLeft = @(); Journal = '' } }",
				"Invoke-Enable",
			};
			return DevDbTlsScriptTests.RunDriverIn(_folder,new[] { "Invoke-Enable","Get-ServerOptions","Test-HasBlock","Test-FolderOverlap","Get-NormalPath","Get-Sid",
				"Get-TestNetworkProblem","Get-TestAddressUseProblem","ConvertTo-AddressNumber","Test-FirewallOn","Get-MachineKeyFolder","Get-BlockLines","Get-CreatedParents",
				"Add-ToMysqldSection","Get-DevFiles","Get-FolderPlanText","Get-TestNetworkEnablePlan","Get-FirewallBlockRanges","Get-TestNetworkJournalPath","New-CertificateSet",
				"New-SerialNumber","ConvertTo-Pem","ConvertFrom-Hex","New-Request","Add-LeafExtensions","New-PfxPassword","New-TestClientPfx","Save-ExistingItems","Get-AccessSddl",
				"Write-AsciiFile","Read-OptionFile","Write-OptionFile","Get-BindText","Get-ServerCertificateAddressProblem","Read-PemCertificate","Get-CertificateIpAddresses",
				"Read-DerLength","Get-TestAddressCheck","Get-TestAddressLoginNote","Write-TestVariables","Get-ErrorText","Get-IgnoredTestNetworkNote","Get-RecordedTestNetwork",
				"ConvertFrom-TestNetworkLines" },arrayBody,exitCodeExpected);
		}

		[TestMethod]
		public void TestNetwork_OwnersRun_TheServersRefusalOfRootAtTheTestAddress_IsANote_ACertificateFailureStaysFatal() {
			//(1) The owner's run of 2026-11-14 ended here with exit 4: the refusal of root@192.0.2.25 (1130, before TLS, wrapped in ERROR 2002 "...
			//TLS handshake ...") was taken for a certificate failure. Now it is a NOTE, the run ends with exit 0 and prints the test's values.
			string output=RunEnableAtTheTestAddress("owner",OWNERS_RUN_ERROR,0).Replace("\r\n","\n");
			StringAssert.Contains(output,"CLIENT 192.0.2.25 ca ca.pem");
			StringAssert.Contains(output,"NOTE: at 192.0.2.25 the server refused the check's login as root ("+OWNERS_RUN_ERROR+").\n");
			StringAssert.Contains(output,"the certificate at the\n  address is verified by TlsCertificateLoginTests with its own X509 user.\n");
			StringAssert.Contains(output,"This run checked that the server certificate ("+Path.Combine(_folder,"owner","opms-dev-tls","server-cert.pem")+") names 192.0.2.25;",
				"The run checked from the file that the server certificate names the address.");
			StringAssert.Contains(output,"$env:OPMS_DB_TLS_TEST_SERVER='192.0.2.25'");
			Assert.IsFalse(output.Contains("FAILED"),output);
			Assert.IsFalse(output.Contains("UNDO"),output);
			//The client verified the certificate there: no note.
			string outputVerified=RunEnableAtTheTestAddress("verified","",0);
			StringAssert.Contains(outputVerified,"The server certificate was verified at 192.0.2.25 too.");
			Assert.IsFalse(outputVerified.Contains("NOTE: at 192.0.2.25"),outputVerified);
			//A real certificate failure stays fatal (exit 3, after the undo), and so does any other error at the address.
			string outputCertificate=RunEnableAtTheTestAddress("certificate","ERROR 2026 (HY000): TLS/SSL error: Validation of SSL server certificate failed",3);
			StringAssert.Contains(outputCertificate,"FAILED: The client could not verify the server certificate at 192.0.2.25: ERROR 2026 (HY000): TLS/SSL error: Validation of SSL "
				+"server certificate failed");
			StringAssert.Contains(outputCertificate,"UNDO (synthetic)");
			string outputOther=RunEnableAtTheTestAddress("other","ERROR 2013 (HY000): Lost connection to server at 'handshake: reading initial communication packet', system error: 0",3);
			StringAssert.Contains(outputOther,"FAILED: The client's check at 192.0.2.25:3306 failed: ERROR 2013 (HY000): Lost connection");
			StringAssert.Contains(outputOther,"UNDO (synthetic)");
			//(2) The classification and the server certificate's names, on the script's own functions; the certificate made for real, in memory.
			string outputCheck=RunDriver(new[] { "Get-TestAddressCheck","New-CertificateSet","New-SerialNumber","ConvertTo-Pem","ConvertFrom-Hex","New-Request","Add-LeafExtensions",
				"New-PfxPassword","New-TestClientPfx","Write-AsciiFile","Read-PemCertificate","Get-CertificateIpAddresses","Read-DerLength","Get-ServerCertificateAddressProblem" },new[] {
				"function C([string]$Case, [int]$Code, [string]$Text) { 'CHECK ' + $Case + ' ' + (Get-TestAddressCheck $Code $Text) }",
				"C 'owner' 1 '"+OWNERS_RUN_ERROR.Replace("'","''")+"'",
				"C 'plain1130' 1 'ERROR 1130 (HY000): Host ''192.0.2.25'' is not allowed to connect to this MariaDB server'",
				"C 'denied' 1 'ERROR 1045 (28000): Access denied for user ''root''@''192.0.2.25'' (using password: NO)'",
				"C 'wrapped1045' 1 'ERROR 2002 (HY000): Received error packet before completion of TLS handshake. The authenticity of the following error cannot be verified: 1045 - Access denied for user ''root''@''192.0.2.25'''",
				"C 'wrappedother' 1 'ERROR 2002 (HY000): Received error packet before completion of TLS handshake. The authenticity of the following error cannot be verified: 1040 - Too many connections'",
				"C 'tls' 1 'ERROR 2026 (HY000): TLS/SSL error: self-signed certificate in certificate chain'",
				"C 'lost' 1 'ERROR 2013 (HY000): Lost connection to server during query'",
				"C 'ok' 0 ''",
				"$set = New-CertificateSet $false '192.0.2.25' 'testhost'",
				"$path = Join-Path $Folder 'server-cert.pem'",
				"Write-AsciiFile $path $set.ServerCert",
				"$cert = Read-PemCertificate $path",
				"'SAN ' + ((Get-CertificateIpAddresses $cert) -join ',')",
				"'SANOK ' + ($null -eq (Get-ServerCertificateAddressProblem $path '192.0.2.25'))",
				"'SANOTHER ' + (Get-ServerCertificateAddressProblem $path '192.0.2.26')",
				"$plain = New-CertificateSet $false",
				"$pathPlain = Join-Path $Folder 'server-cert-plain.pem'",
				"Write-AsciiFile $pathPlain $plain.ServerCert",
				"'SANPLAIN ' + (Get-ServerCertificateAddressProblem $pathPlain '192.0.2.25')",
			});
			StringAssert.Contains(outputCheck,"CHECK owner login-refused\r\n","The owner's exact error is the server's refusal of the login, not a certificate failure.");
			StringAssert.Contains(outputCheck,"CHECK plain1130 login-refused\r\n");
			StringAssert.Contains(outputCheck,"CHECK denied login-refused\r\n");
			StringAssert.Contains(outputCheck,"CHECK wrapped1045 login-refused\r\n");
			StringAssert.Contains(outputCheck,"CHECK wrappedother certificate\r\n","Another error before the end of the TLS handshake is no login refusal.");
			StringAssert.Contains(outputCheck,"CHECK tls certificate\r\n");
			StringAssert.Contains(outputCheck,"CHECK lost failed\r\n");
			StringAssert.Contains(outputCheck,"CHECK ok verified\r\n");
			StringAssert.Contains(outputCheck,"SAN 127.0.0.1,192.0.2.25\r\n","The IP entries of the subject alternative names, read from the DER.");
			StringAssert.Contains(outputCheck,"SANOK True\r\n");
			StringAssert.Contains(outputCheck,"SANOTHER The server certificate "+Path.Combine(_folder,"server-cert.pem")+" does not name 192.0.2.26 (its IP addresses: "
				+"127.0.0.1, 192.0.2.25).");
			StringAssert.Contains(outputCheck,"SANPLAIN The server certificate "+Path.Combine(_folder,"server-cert-plain.pem")+" does not name 192.0.2.25 (its IP addresses: "
				+"127.0.0.1).");
			string script=TestHelpers.ReadRepoFile("tools","Enable-LocalDbTls.ps1");
			Assert.IsFalse(script.Contains("$atTest.ExitCode -ne 0 -and $atTest.Error -match '(?i)SSL|TLS|certificate'"),"The check of main 9bf1de770 that failed the owner's run.");
		}

		[TestMethod]
		public void TestNetwork_OwnersRun_RemoveTestAdapter_UnderStrictMode_TheWaitAsksAgainAfterAnError_AndNoListBecomesAString() {
			//(2) The undo of the owner's run threw "The property 'Count' cannot be found on this object" once the adapter was gone: in
			//Remove-TestAdapter, '$address = @(...)' assigned to its [string] parameter $Address (PowerShell's names ignore case), so the empty list
			//became a string, which has no Count under Set-StrictMode. The device and the addresses here are synthetic: a type of the SetupAPI
			//class's name whose Exists and Remove only change a field, and Get-NetIPAddress (and, for main's script, Get-PnpDevice) following it.
			string output=RunDriver(new[] { "Remove-TestAdapter","Get-ErrorText","Test-HasAddress","Test-TestAdapterExists" },new[] {
				"function Add-LoopbackAdapterType { }",
				"Add-Type -TypeDefinition 'namespace OpenPmsDentalDevDbTls { public static class LoopbackAdapter { public static bool Present; public static bool Removed; "
					+"public static int ThrowsAfterRemove; public static bool Stays; public static bool Exists(string instanceId) { if (Removed && ThrowsAfterRemove != 0) { "
					+"if (ThrowsAfterRemove > 0) { ThrowsAfterRemove--; } throw new System.InvalidOperationException(\"SetupDiOpenDeviceInfo failed (synthetic)\"); } return Present; } "
					+"public static void Remove(string instanceId) { Removed = true; if (!Stays) { Present = false; } } } }'",
				"function Get-PnpDevice { if ([OpenPmsDentalDevDbTls.LoopbackAdapter]::Present) { [pscustomobject]@{ InstanceId = 'ROOT\\NET\\0000' } } }",
				"function Get-NetIPAddress { [pscustomobject]@{ IPAddress = '127.0.0.1' }; if ([OpenPmsDentalDevDbTls.LoopbackAdapter]::Present) { [pscustomobject]@{ IPAddress = '192.0.2.25' } } }",
				"function Case([string]$Name, [bool]$Present, [int]$Throws, [bool]$Stays, [int]$Seconds) {",
				"  [OpenPmsDentalDevDbTls.LoopbackAdapter]::Present = $Present; [OpenPmsDentalDevDbTls.LoopbackAdapter]::Removed = $false",
				"  [OpenPmsDentalDevDbTls.LoopbackAdapter]::ThrowsAfterRemove = $Throws; [OpenPmsDentalDevDbTls.LoopbackAdapter]::Stays = $Stays",
				"  try {",
				"    if ($Seconds -gt 0) { Remove-TestAdapter 'ROOT\\NET\\0000' '192.0.2.25' $Seconds } else { Remove-TestAdapter 'ROOT\\NET\\0000' '192.0.2.25' }",
				"    Write-Host ($Name + ' GONE removed ' + [OpenPmsDentalDevDbTls.LoopbackAdapter]::Removed)",
				"  }",
				"  catch { Write-Host ($Name + ' THROWS ' + $_.Exception.Message) }",
				"}",
				"Case 'owner' $true 0 $false 0",
				"Case 'missing' $false 0 $false 0",
				"Case 'flaky' $true 2 $false 0",
				"Case 'stays' $true 0 $true 1",
				"Case 'unknown' $true -1 $false 1",
			});
			//Write-Host lines end with a bare line feed.
			output=output.Replace("\r\n","\n");
			StringAssert.Contains(output,"owner GONE removed True\n","The owner's case: the device removed, then device and address gone. "+output);
			Assert.IsFalse(output.Contains("cannot be found on this object"),output);
			StringAssert.Contains(output,"missing GONE removed False\n","A device that is gone already is not removed again, and that is no problem.");
			StringAssert.Contains(output,"flaky GONE removed True\n","A check that throws while it waits is asked again.");
			StringAssert.Contains(output,@"stays THROWS After the removal of the test adapter, within 1 seconds: the device ROOT\NET\0000 is still there; this computer still has "
				+"the address 192.0.2.25.\n");
			StringAssert.Contains(output,@"unknown THROWS After the removal of the test adapter, within 1 seconds: whether the device ROOT\NET\0000 is still there is not known "
				+"(Exception calling \"Exists\" with \"1\" argument(s): \"SetupDiOpenDeviceInfo failed (synthetic)\" [At line:");
			//The whole script: no function assigns a variable (or a foreach variable) that shares a parameter's name, read from its syntax tree.
			string outputScan=RunDriver(new string[0],new[] {
				"$count = 0",
				"foreach ($f in $ast.FindAll({ param($a) $a -is [System.Management.Automation.Language.FunctionDefinitionAst] }, $true)) {",
				"  $count++",
				"  $params = @()",
				"  if ($null -ne $f.Parameters) { $params += @($f.Parameters) }",
				"  if ($null -ne $f.Body.ParamBlock) { $params += @($f.Body.ParamBlock.Parameters) }",
				"  $lower = @($params | ForEach-Object { $_.Name.VariablePath.UserPath.ToLowerInvariant() })",
				"  foreach ($s in $f.Body.FindAll({ param($a) ($a -is [System.Management.Automation.Language.AssignmentStatementAst] -and $a.Left -is [System.Management.Automation.Language.VariableExpressionAst]) -or $a -is [System.Management.Automation.Language.ForEachStatementAst] }, $true)) {",
				"    if ($s -is [System.Management.Automation.Language.ForEachStatementAst]) { $v = $s.Variable } else { $v = $s.Left }",
				"    if ($lower -contains $v.VariablePath.UserPath.ToLowerInvariant()) { Write-Host ('COLLISION ' + $f.Name + ' $' + $v.VariablePath.UserPath + ' line ' + $s.Extent.StartLineNumber) }",
				"  }",
				"}",
				"Write-Host ('SCANNED ' + $count)",
			});
			Assert.IsFalse(outputScan.Contains("COLLISION"),"A local that shares a parameter's name takes the parameter's type: "+outputScan);
			System.Text.RegularExpressions.Match matchScanned=System.Text.RegularExpressions.Regex.Match(outputScan,@"SCANNED (\d+)");
			Assert.IsTrue(matchScanned.Success && int.Parse(matchScanned.Groups[1].Value)>100,"Every function of the script was scanned: "+outputScan);
			string script=TestHelpers.ReadRepoFile("tools","Enable-LocalDbTls.ps1");
			Assert.IsFalse(script.Contains("Get-PnpDevice -InstanceId"),"Whether the device is there comes from the SetupAPI (LoopbackAdapter.Exists).");
			StringAssert.Contains(script,"return [bool][OpenPmsDentalDevDbTls.LoopbackAdapter]::Exists($InstanceId)");
		}

		///<summary>This PC after the owner's first run (2026-11-14, exit 4), as the lead read it: the journal names the address, the adapter
		///ROOT\NET\0000, both rules, both thumbprints and both keys; the device, the address, both certificates and both key files are gone; the two
		///rules are there; my.ini has no block and there is no backup; the opms-dev-tls folder holds the journal and ca.pem. Invoke-Revert with the
		///script's own state, plan, undo and report, and every system call replaced: whether the device is there (Test-TestAdapterExists, and
		///Get-PnpDevice for main's script), the addresses (Get-NetIPAddress), the adapters, the rules (Get-NetFirewallRule; Remove-NetFirewallRule
		///only takes the rule out of a list), the store (Find-MachineCertificate: nothing), the service and the listeners. The key "files" are named
		///in the computer's key folders and are not there; every file the run removes is in the test's temporary folder.</summary>
		private string RunRevertOnTheOwnersPc(string name,bool isDryRun) {
			List<string> listBody=new List<string> {
				"$root = Join-Path $Folder '"+name+"'",
				"$tls = Join-Path $root 'opms-dev-tls'",
				"New-Item -ItemType Directory -Path $tls | Out-Null",
				"$journal = Join-Path $tls $TestNetworkJournalName",
			};
			listBody.AddRange(GetJournalLines(@"ROOT\NET\0000"));
			listBody.AddRange(new[] {
				"$set = New-CertificateSet $false",
				"[System.IO.File]::WriteAllText((Join-Path $tls 'ca.pem'), $set.CaCert)",
				"$script:Facts = [pscustomobject]@{ Ini = (Join-Path $root 'my.ini'); Home = $root }",
				"[System.IO.File]::WriteAllText($script:Facts.Ini, \"[mysqld]`r`nport=3306`r`nbind-address=127.0.0.1`r`n\")",
				"$script:IniFile = [pscustomobject]@{ Text = [System.IO.File]::ReadAllText($script:Facts.Ini); Newline = \"`r`n\" }",
				"$DryRun = $"+(isDryRun ? "true" : "false"),
				"function Assert-LocalServer { }",
				"function Get-Paths([bool]$ForEnable) { [pscustomobject]@{ TlsFolder = $tls; Ca = (Join-Path $tls 'ca.pem'); DevFolder = (Join-Path $root 'dev'); DevCa = (Join-Path $root 'dev\\ca.pem')",
				"  ClientFolder = ''; ClientCert = ''; ClientKey = ''; IsRecorded = $false; CreatedParents = @(); Backup = ($script:Facts.Ini + $BackupSuffix); IniTemp = ($script:Facts.Ini + $IniTempSuffix)",
				"  TestNetwork = (Get-RecordedTestNetwork $script:IniFile.Text) } }",
				"function Add-LoopbackAdapterType { throw 'No SetupAPI call here (synthetic).' }",
				"function Test-TestAdapterExists([string]$InstanceId) { Write-Host ('DEVICE? ' + $InstanceId); $false }",
				"function Get-TestAdapterHardwareIds([string]$InstanceId) { throw 'The device is gone (synthetic).' }",
				"function Get-PnpDevice { }",
				"function Get-NetIPAddress { [pscustomobject]@{ IPAddress = '127.0.0.1' } }",
				"function Get-NetAdapter { }",
				"$script:rules = @(@($FirewallAllowName, $FirewallBlockName) | ForEach-Object { [pscustomobject]@{ Name = $_; Group = $FirewallGroup } })",
				"function Get-NetFirewallRule { $script:rules }",
				"function Remove-NetFirewallRule { param($Name) Write-Host ('REMOVE RULE ' + $Name); $script:rules = @($script:rules | Where-Object { $_.Name -ne $Name }) }",
				"function Find-MachineCertificate([string]$Thumbprint) { $null }",
				"function Remove-MachineCertificate($Certificate) { throw 'There is no store entry to remove (synthetic).' }",
				"function Restart-DbService { Write-Host 'RESTART' }",
				"function Get-ServerState([string]$Tls) { [pscustomobject]@{ Ok = $true; Error = ''; Values = @{} } }",
				"function Read-OptionFile([string]$Path) { throw 'There is no backup to read (synthetic).' }",
				"function Get-NetTCPConnection { }",
			});
			List<string> listFunctions=new List<string> { "Invoke-Revert","Get-TestNetworkJournalPath","Read-TestNetworkJournal","ConvertFrom-TestNetworkLines",
				"Get-RecordedTestNetwork","Get-TestNetworkRecordProblem","Get-TestAddressFormProblem","Get-KeyRecordProblem","ConvertFrom-KeyRecord","ConvertTo-KeyRecord",
				"Get-MachineKeyFolder","Test-HasBlock","Remove-Block","Get-TestNetworkState","Get-TestNetworkStateProblem","Get-TestNetworkRevertPlan","Get-TestNetworkLeft",
				"Get-TestNetworkRemovedLines","Get-DevFiles","Get-CreatedParents","Add-TestNetworkJournalLine","Undo-TestNetwork","Get-CreatedKey","Remove-TestAdapter",
				"Test-HasAddress","Remove-TestFirewallRule","Remove-TestClientCertificate","Remove-TestKey","Get-ErrorText","Read-PemCertificate","Test-IssuedByCa",
				"Remove-NewFiles","Remove-EmptyFolder","Test-FolderReadable","New-CertificateSet","New-SerialNumber","ConvertTo-Pem","ConvertFrom-Hex","New-Request",
				"Add-LeafExtensions","New-PfxPassword","New-TestClientPfx" };
			if(isDryRun) {
				listFunctions.Add("Confirm-Change");
			}
			else {
				listBody.Add("function Confirm-Change([string]$Question) { Write-Host ('CONFIRM ' + $Question) }");
			}
			listBody.Add("Invoke-Revert");
			return DevDbTlsScriptTests.RunDriverIn(_folder,listFunctions.ToArray(),listBody.ToArray(),0);
		}

		[TestMethod]
		public void TestNetwork_OwnersPcAfterTheFailedRun_RevertRemovesTheRulesAndTheFolder_AndReportsTheRestAsGone() {
			string fileCapiKey=Environment.GetEnvironmentVariable("ProgramData")+@"\Microsoft\Crypto\RSA\MachineKeys\keyfile-capi";
			string fileCngKey=Environment.GetEnvironmentVariable("ProgramData")+@"\Microsoft\Crypto\Keys\keyfile-cng";
			string folderTls=Path.Combine(_folder,"revert","opms-dev-tls");
			//(3) The owner's -DryRun -Revert: the plan names exactly the rules and the folder to remove, and the rest as gone.
			string outputDry=RunRevertOnTheOwnersPc("dryrun",isDryRun:true).Replace("\r\n","\n");
			foreach(string text in new[] { outputDry }) {
				Assert.IsFalse(text.Contains("REFUSED"),text);
				StringAssert.Contains(text,"an enable run stopped before it");
				StringAssert.Contains(text,"or its undo restored my.ini and removed the backup but could not remove");
				StringAssert.Contains(text,"First the test network 192.0.2.25 of an enable run that did not finish, as its journal "
					+Path.Combine(_folder,"dryrun","opms-dev-tls","test-network.record")+" records it");
				StringAssert.Contains(text,@"[adapter] The loopback adapter ROOT\NET\0000 is not there any more: nothing to remove.");
				StringAssert.Contains(text,"[address] [bindings] Its address 192.0.2.25/32 is not on this computer any more, and its bindings went with the adapter:\n"
					+"     nothing to remove.");
				StringAssert.Contains(text,"[client-certificates] The CAPI certificate "+new string('A',40)+" is not in LocalMachine\\My any more, and its key\n     file "+fileCapiKey
					+" is gone too: nothing to remove.");
				StringAssert.Contains(text,"[client-certificates] The CNG certificate "+new string('B',40)+" is not in LocalMachine\\My any more, and its key\n     file "+fileCngKey
					+" is gone too: nothing to remove.");
				StringAssert.Contains(text,"[firewall] Remove the firewall rules OpenPmsDental-DbTlsTest-Allow3306 and OpenPmsDental-DbTlsTest-Block3306 (group 'Open PMS - Dental "
					+"local test TLS') last");
				StringAssert.Contains(text,"my.ini has no block of this script (the run stopped before it wrote it, or its undo restored my.ini)");
				CollectionAssert.AreEqual(_arrayTags.OrderBy(x => x).ToList(),GetTags(text),text);
				Assert.IsFalse(text.Contains("Restore "),"No restore of my.ini: there is no backup. "+text);
			}
			StringAssert.Contains(outputDry,". Remove "+Path.Combine(_folder,"dryrun","opms-dev-tls")+", "+Path.Combine(_folder,"dryrun","dev","ca.pem")+";",
				"The opms-dev-tls folder (the journal, ca.pem) and the dev folder's copy, which is not there; no backup.");
			StringAssert.Contains(outputDry,"Dry run: nothing was changed.");
			Assert.IsFalse(outputDry.Contains("REMOVE RULE"),outputDry);
			Assert.IsTrue(File.Exists(Path.Combine(_folder,"dryrun","opms-dev-tls","test-network.record")),"The dry run changes nothing.");
			//The owner's -Revert: the two rules and the folder go; the adapter, the address, both certificates and their key files are reported
			//as gone already, not as problems; my.ini and the service are not touched.
			string output=RunRevertOnTheOwnersPc("revert",isDryRun:false).Replace("\r\n","\n");
			Assert.IsFalse(output.Contains("REFUSED"),output);
			Assert.IsFalse(output.Contains("FAILED"),output);
			Assert.IsFalse(output.Contains("is not fully removed"),output);
			StringAssert.Contains(output,"CONFIRM Turn TLS off again?");
			int indexAllow=output.IndexOf("REMOVE RULE OpenPmsDental-DbTlsTest-Allow3306\n",StringComparison.Ordinal);
			int indexBlock=output.IndexOf("REMOVE RULE OpenPmsDental-DbTlsTest-Block3306\n",StringComparison.Ordinal);
			int indexDevice=output.IndexOf(@"DEVICE? ROOT\NET\0000",output.IndexOf("CONFIRM",StringComparison.Ordinal),StringComparison.Ordinal);
			Assert.IsTrue(indexDevice>0 && indexAllow>indexDevice && indexBlock>indexAllow,"The device checked gone first, then the rules: "+output);
			StringAssert.Contains(output,"Removed of the test network 192.0.2.25: the firewall rule OpenPmsDental-DbTlsTest-Allow3306, the firewall rule "
				+"OpenPmsDental-DbTlsTest-Block3306.\n");
			StringAssert.Contains(output,@"Already gone before this run (not a problem): the loopback adapter ROOT\NET\0000 and its address 192.0.2.25, the CAPI certificate "
				+new string('A',40)+" and its key file "+fileCapiKey+", the CNG certificate "+new string('B',40)+" and its key file "+fileCngKey+".\n");
			StringAssert.Contains(output,"Removed what the enable run that did not finish had made; my.ini and the service were not touched.");
			Assert.IsFalse(output.Contains("RESTART"),output);
			Assert.IsFalse(Directory.Exists(folderTls),"The opms-dev-tls folder with the journal and ca.pem is removed.");
			Assert.AreEqual("[mysqld]\r\nport=3306\r\nbind-address=127.0.0.1\r\n",File.ReadAllText(Path.Combine(_folder,"revert","my.ini")),"my.ini is not touched.");
		}

		[TestMethod]
		public void TestNetwork_ACertificateWhoseKeyFileWasDeletedByHand_LosesItsStoreEntryToo_WhenTheKeyIsKnown() {
			//A store entry whose key does not open any more (its key file deleted by hand): with the key known (the record, the run's own undo), the
			//entry goes and nothing is left; without, the entry stays and the error (and the exit-4 text) says how to find the key. Synthetic: the
			//"store" is a variable, Get-TestKeyInfo throws as for a missing key file, the recorded key file is in the test's temporary folder and
			//not there; nothing in a store or in the computer's key folders is touched.
			string thumbprint=new string('C',40);
			string output=RunDriver(new[] { "Remove-TestClientCertificate","Remove-TestKey","Get-ErrorText","ConvertFrom-KeyRecord","Undo-TestNetwork","Get-CreatedKey",
				"Get-ManualCertificateRemoval" },new[] {
				"$thumb = '"+thumbprint+"'",
				"$script:inStore = $true",
				"function Find-MachineCertificate([string]$Thumbprint) { if ($script:inStore) { [pscustomobject]@{ Thumbprint = $Thumbprint; HasPrivateKey = $true } } else { $null } }",
				"function Get-TestKeyInfo($Certificate) { throw 'Keyset does not exist (synthetic: the key file was deleted by hand)' }",
				"function Remove-MachineCertificate($Certificate) { Write-Host ('STORE REMOVED ' + $Certificate.Thumbprint.Substring(0, 4)); $script:inStore = $false }",
				"$key = ConvertFrom-KeyRecord ($CngProviderName + '|dbtls-env-cng|' + (Join-Path $Folder 'cng-key-file-deleted-by-hand')) 'CNG'",
				"'KNOWN [' + (Remove-TestClientCertificate $thumb $key) + '] in the store ' + $script:inStore",
				"$script:inStore = $true",
				"try { $left = Remove-TestClientCertificate $thumb $null; 'UNKNOWN not thrown [' + $left + ']' } catch { 'UNKNOWN ' + $_.Exception.Message }",
				"'UNKNOWN in the store ' + $script:inStore",
				"function Remove-TestFirewallRule($Name) { }",
				"$created = [pscustomobject]@{ AdapterId = ''; FirewallRules = @(); Thumbprints = @($thumb); Keys = @(); PfxFolder = '' }",
				"$p = Undo-TestNetwork $created '192.0.2.25'",
				"'UNDO ' + (@($p) -join ' || ')",
				"'MANUAL ' + (Get-ManualCertificateRemoval $thumb (Get-CreatedKey $created $thumb))",
			});
			output=output.Replace("\r\n","\n");
			StringAssert.Contains(output,"STORE REMOVED CCCC\n");
			StringAssert.Contains(output,"KNOWN [] in the store False\n","The entry is gone, and nothing is left: the key file was gone already.");
			StringAssert.Contains(output,"UNKNOWN The key of "+thumbprint+" does not open (Keyset does not exist (synthetic: the key file was deleted by hand) [At ");
			StringAssert.Contains(output,"and no record names its key file, so its store entry stays: certutil -store My "+thumbprint+" shows its Provider and Key Container.\n");
			StringAssert.Contains(output,"UNKNOWN in the store True\n");
			StringAssert.Contains(output,"UNDO the certificate "+thumbprint+" in LocalMachine\\My (with its key): The key of "+thumbprint+" does not open");
			StringAssert.Contains(output,"MANUAL the certificate "+thumbprint+": if certutil -store My "+thumbprint+" shows it, first delete its key with certutil -csp");
		}

		[TestMethod]
		public void TestNetwork_The32BitPowerShell_IsRefusedFirst_AndASecondEnableRunSaysItIgnoresTheAddress() {
			string output=RunDriver(new[] { "Get-ProcessBitnessProblem","Get-IgnoredTestNetworkNote" },new[] {
				"'BIT32 ' + (Get-ProcessBitnessProblem $true $false)",
				"'BIT64 ' + ($null -eq (Get-ProcessBitnessProblem $true $true)) + ' ' + ($null -eq (Get-ProcessBitnessProblem $false $false))",
				"$rec = [pscustomobject]@{ Address = '192.0.2.25' }",
				"'NOTE none ' + @(Get-IgnoredTestNetworkNote '' $rec).Count",
				"'NOTE norecord ' + (@(Get-IgnoredTestNetworkNote '192.0.2.30' $null) -join ' ')",
				"'NOTE same ' + (@(Get-IgnoredTestNetworkNote '192.0.2.25' $rec) -join ' ')",
				"'NOTE other ' + (@(Get-IgnoredTestNetworkNote '192.0.2.30' $rec) -join ' ')",
			});
			StringAssert.Contains(output,"BIT32 This is the 32-bit Windows PowerShell (SysWOW64) on 64-bit Windows;");
			StringAssert.Contains(output,"BIT64 True True\r\n");
			StringAssert.Contains(output,"NOTE none 0\r\n");
			StringAssert.Contains(output,"NOTE norecord NOTE: -WithTestNetwork 192.0.2.30 is ignored: TLS is on without a test network (the block records none).");
			StringAssert.Contains(output,"NOTE same NOTE: -WithTestNetwork 192.0.2.25 is ignored: the block records that test network already (its values below).\r\n");
			StringAssert.Contains(output,"NOTE other NOTE: -WithTestNetwork 192.0.2.30 is ignored: the block records the test network 192.0.2.25 (its values below).");
			StringAssert.Contains(output,"run -Revert first and then this script again with -WithTestNetwork 192.0.2.30.");
			//A second enable run with the block in my.ini: TLS is on, nothing to do, and it says that it ignores the other address.
			string outputSecond=DevDbTlsScriptTests.RunDriverIn(_folder,new[] { "Invoke-Enable","Test-HasBlock","Get-RecordedTestNetwork","ConvertFrom-TestNetworkLines",
				"Get-TestNetworkRecordProblem","Get-TestAddressFormProblem","Get-KeyRecordProblem","ConvertFrom-KeyRecord","Get-MachineKeyFolder","Get-IgnoredTestNetworkNote",
				"Write-TestVariables" },new[] {
				"$script:Facts = [pscustomobject]@{ Ini = 'C:\\x\\my.ini' }",
				"$script:IniFile = [pscustomobject]@{ Text = (\"[mysqld]`nport=3306`n\" + $MarkerBegin + \"`n\" + $RecordAddressPrefix + \"192.0.2.25`n\" + $MarkerEnd + \"`n\") }",
				"$WithTestNetwork = '192.0.2.30'",
				"function Assert-LocalServer { }",
				"function Get-Paths([bool]$ForEnable) { }",
				"Invoke-Enable",
			},0);
			StringAssert.Contains(outputSecond,"TLS is already on through this script");
			StringAssert.Contains(outputSecond,"NOTE: -WithTestNetwork 192.0.2.30 is ignored: the block records the test network 192.0.2.25");
			//The real script in the 32-bit Windows PowerShell of a 64-bit Windows: refused before anything else, even a dry run.
			string script=TestHelpers.ReadRepoFile("tools","Enable-LocalDbTls.ps1");
			int indexBitness=script.IndexOf("$bitnessProblem = Get-ProcessBitnessProblem ([Environment]::Is64BitOperatingSystem) ([Environment]::Is64BitProcess)",StringComparison.Ordinal);
			int indexModes=script.IndexOf("$modes = @($RequireSecureTransport.IsPresent",StringComparison.Ordinal);
			Assert.IsTrue(indexBitness>0 && indexModes>indexBitness,"The check comes first.");
			string pathPowerShell32=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows),"SysWOW64","WindowsPowerShell","v1.0","powershell.exe");
			if(!Environment.Is64BitOperatingSystem || !File.Exists(pathPowerShell32)) {
				Assert.Inconclusive("Not a 64-bit Windows with the 32-bit Windows PowerShell: the function's check above is all there is to test.");
			}
			Assert.AreEqual(1,DevDbTlsScriptTests.RunPowerShellWith(pathPowerShell32,DevDbTlsScriptTests.GetScriptPath(),"-DryRun",out string output32),output32);
			StringAssert.Contains(output32,"REFUSED: This is the 32-bit Windows PowerShell (SysWOW64) on 64-bit Windows;");
			Assert.IsFalse(output32.Contains("Service:"),"Refused before the service is looked at: "+output32);
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
