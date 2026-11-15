// The database TLS check of S2-2 (docs/specs/net10-plan.md, the S2-2 row; open points O-8 and O-12; docs/port-notes/S2-2.md, section 6): the
// program's connection over the one local test address (192.0.2.x, the test adapter of tools/Enable-LocalDbTls.ps1 -WithTestNetwork; the
// owner's narrow exception to "only 127.0.0.1:3306", which ReadSetup enforces before any connection) to the local test server, as a user
// created REQUIRE X509, with SslMode=VerifyFull, the test CA and a client certificate from the computer's store (LocalMachine\My, where the connection settings tool puts
// it, OpenPmsDentalDbSettings/ClientCertificateStore.cs): once with a certificate whose private key is in a CAPI provider and once with one in a
// CNG provider. Inconclusive unless the run names the server and both certificates (the variables below); a variable that is set wrongly fails.
// The test creates the REQUIRE X509 user and an s22_ scratch database with one synthetic row through the local test server's own loopback
// connection (127.0.0.1:3306, root, DevDbTls), and drops both afterwards.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using CodeBase;
using DataConnectionBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySqlConnector;

namespace DbConnectionSecurityTests {
	[TestClass]
	public class TlsCertificateLoginTests {
		///<summary>The one local test address (an IPv4 address from 192.0.2.1 to 192.0.2.254 that this computer has: the test adapter of
		///tools/Enable-LocalDbTls.ps1 -WithTestNetwork) at which the local test server listens and which its certificate names, in the program's
		///notation ("192.0.2.x" or "192.0.2.x:3306"). Nothing else: the owner's exception (OWNER_EXCEPTION).</summary>
		public const string VARIABLE_SERVER="OPMS_DB_TLS_TEST_SERVER";
		///<summary>Why ReadSetup refuses any other server before a connection.</summary>
		public const string OWNER_EXCEPTION="The owner's narrow exception of 2026-11-14 to 'only 127.0.0.1:3306' (docs/owner-questions.md; "
			+"docs/port-notes/DB-TLS.md, section 11) lets these tests connect to the one local test address on port 3306 and to nothing else: "
			+VARIABLE_SERVER+" must be an IPv4 address from 192.0.2.1 to 192.0.2.254 that this computer has (the test adapter of "
			+"tools/Enable-LocalDbTls.ps1 -WithTestNetwork). Nothing was connected.";
		///<summary>The thumbprint of a client certificate in LocalMachine\My whose private key is in a CAPI (CryptoAPI) provider.</summary>
		public const string VARIABLE_CAPI="OPMS_DB_TLS_TEST_CAPI_THUMBPRINT";
		///<summary>The thumbprint of a client certificate in LocalMachine\My whose private key is in a CNG (Key Storage Provider) provider.</summary>
		public const string VARIABLE_CNG="OPMS_DB_TLS_TEST_CNG_THUMBPRINT";
		///<summary>Optional: the CA (PEM) that signed the server's certificate. Without it, the local test server's CA (DevDbTls.GetCaPath).</summary>
		public const string VARIABLE_CA="OPMS_DB_TLS_TEST_CA";
		private const string ROOT="Server=127.0.0.1;Port=3306;User ID=root;Password=;Connection Timeout=10;Pooling=false";
		private string _folder;

		[TestInitialize]
		public void SetUp() {
			_folder=TestHelpers.NewTempFolder();
		}

		[TestCleanup]
		public void CleanUp() {
			TestHelpers.DeleteFolder(_folder);
		}

		private static string GetVariable(string name) {
			return (Environment.GetEnvironmentVariable(name)??"").Trim();
		}

		///<summary>This computer's own unicast addresses, read from its network interfaces (nothing is changed, nothing is connected).</summary>
		private static List<IPAddress> GetLocalUnicastAddresses() {
			return NetworkInterface.GetAllNetworkInterfaces()
				.SelectMany(x => x.GetIPProperties().UnicastAddresses)
				.Select(x => x.Address)
				.ToList();
		}

		///<summary>Why a host is not the one local test address of the owner's exception, or null when it is: an IPv4 address in its plain dotted
		///form (no name, no leading zeros, no other notation IPAddress.TryParse takes) from 192.0.2.1 to 192.0.2.254, and one of this computer's
		///own unicast addresses (listLocalAddresses). No name is looked up.</summary>
		internal static string GetTestServerProblem(string host,IEnumerable<IPAddress> listLocalAddresses) {
			System.Text.RegularExpressions.Match match=System.Text.RegularExpressions.Regex.Match(host,@"^192\.0\.2\.(?<last>[1-9][0-9]{0,2})$");
			if(!match.Success || int.Parse(match.Groups["last"].Value,CultureInfo.InvariantCulture)>254) {
				return host+" is not an IPv4 address from 192.0.2.1 to 192.0.2.254";
			}
			IPAddress ipAddress=IPAddress.Parse(host);
			if(!listLocalAddresses.Any(x => x.AddressFamily==AddressFamily.InterNetwork && x.Equals(ipAddress))) {
				return host+" is not one of this computer's own addresses";
			}
			return null;
		}

		///<summary>The test's set-up from the variables: inconclusive when none of the three is set, a failure when only some are, or when a value
		///is not what it must be (a loopback server, another port than the local test server's, a server that is not the one local test address,
		///no CA, a certificate that is not there).</summary>
		private static void ReadSetup(out string serverTest,out string sslCa,out string thumbprintCapi,out string thumbprintCng) {
			ReadSetup(GetVariable,GetLocalUnicastAddresses,out serverTest,out sslCa,out thumbprintCapi,out thumbprintCng);
		}

		///<summary>ReadSetup with the variables from funcGetVariable and this computer's addresses from funcGetLocalAddresses, so that a test can
		///give synthetic ones. Connects to nothing: every refusal comes before the test's first connection.</summary>
		internal static void ReadSetup(Func<string,string> funcGetVariable,Func<List<IPAddress>> funcGetLocalAddresses,out string serverTest,out string sslCa,
			out string thumbprintCapi,out string thumbprintCng)
		{
			serverTest=(funcGetVariable(VARIABLE_SERVER)??"").Trim();
			thumbprintCapi=(funcGetVariable(VARIABLE_CAPI)??"").Trim().Replace(" ","").ToUpperInvariant();
			thumbprintCng=(funcGetVariable(VARIABLE_CNG)??"").Trim().Replace(" ","").ToUpperInvariant();
			if(serverTest=="" && thumbprintCapi=="" && thumbprintCng=="") {
				Assert.Inconclusive("The database TLS check needs "+VARIABLE_SERVER+", "+VARIABLE_CAPI+" and "+VARIABLE_CNG+" (docs/port-notes/S2-2.md, "
					+"section 6).");
			}
			Assert.IsTrue(serverTest!="" && thumbprintCapi!="" && thumbprintCng!="","Set all three of "+VARIABLE_SERVER+", "+VARIABLE_CAPI+" and "
				+VARIABLE_CNG+", or none.");
			Assert.IsTrue(DbConnectionSecurity.SplitServerList(serverTest,out string host,out string port) && !host.Contains(","),
				VARIABLE_SERVER+" names one server: "+serverTest);
			Assert.IsFalse(DbConnectionSecurity.IsLoopbackConnectorServer(host),VARIABLE_SERVER+" must be a non-loopback address of this computer, "
				+"not "+serverTest+": the rule exempts loopback connections from TLS.");
			Assert.IsTrue(port=="" || port=="3306",VARIABLE_SERVER+": the local test server listens on port 3306, the one the test creates its user on.");
			string problemServer=GetTestServerProblem(host,funcGetLocalAddresses());
			if(problemServer!=null) {
				Assert.Fail(VARIABLE_SERVER+"="+serverTest+": "+problemServer+". "+OWNER_EXCEPTION);
			}
			sslCa=(funcGetVariable(VARIABLE_CA)??"").Trim();
			if(sslCa=="") {
				sslCa=DevDbTls.GetCaPath();
			}
			Assert.IsTrue(sslCa!="" && File.Exists(sslCa),"No CA: set "+VARIABLE_CA+" or run "+DevDbTls.ENABLE_SCRIPT+" (docs/port-notes/DB-TLS.md).");
		}

		///<summary>The certificate in LocalMachine\My with its private key, and whether that key is in a CNG provider (false: CAPI). The kind comes
		///from the certificate's key provider information (IsCngProvider), then the key must open for this account as that kind.</summary>
		private static bool IsCngKey(string thumbprint,string variable) {
			using(X509Store x509Store=new X509Store(StoreName.My,StoreLocation.LocalMachine)) {
				x509Store.Open(OpenFlags.ReadOnly|OpenFlags.OpenExistingOnly);
				X509Certificate2Collection x509Certificate2Collection=x509Store.Certificates.Find(X509FindType.FindByThumbprint,thumbprint,false);
				Assert.AreEqual(1,x509Certificate2Collection.Count,variable+": no certificate "+thumbprint+" in LocalMachine\\My.");
				X509Certificate2 x509Certificate2=x509Certificate2Collection[0];
				Assert.IsTrue(x509Certificate2.HasPrivateKey,variable+": the certificate has no private key.");
				bool isCng=IsCngProvider(x509Certificate2,variable);
				if(!isCng) {
					RSACryptoServiceProvider rsaCryptoServiceProvider=null;
					try {
						rsaCryptoServiceProvider=x509Certificate2.PrivateKey as RSACryptoServiceProvider;
					}
					catch(CryptographicException) {
						rsaCryptoServiceProvider=null;
					}
					Assert.IsNotNull(rsaCryptoServiceProvider,variable+": this account cannot read the CAPI private key (an RSA key).");
					return false;
				}
				AsymmetricAlgorithm asymmetricAlgorithm=(AsymmetricAlgorithm)x509Certificate2.GetRSAPrivateKey()??x509Certificate2.GetECDsaPrivateKey();
				Assert.IsNotNull(asymmetricAlgorithm,variable+": this account cannot read the private key (an RSA or ECDSA key).");
				Assert.IsTrue(asymmetricAlgorithm is RSACng || asymmetricAlgorithm is ECDsaCng,variable+": a key of "+asymmetricAlgorithm.GetType().Name);
				return true;
			}
		}

		///<summary>True when the certificate's key provider information (read without opening the key, CertificateKeyProvInfo) names a CNG key
		///storage provider (provider type 0), false for a CAPI provider; the key must be in the computer's key set, where the connection settings
		///tool imports it. Not the type of the key object: .NET Framework's GetRSAPrivateKey opens a CAPI key through CNG too, as an RSACng
		///(ClientCertificateStore.GetCapiRsaPrivateKey), so that type cannot tell the two apart.</summary>
		internal static bool IsCngProvider(X509Certificate2 x509Certificate2,string variable) {
			CertificateKeyProvInfo certificateKeyProvInfo=CertificateKeyProvInfo.Read(x509Certificate2);
			Assert.IsTrue(certificateKeyProvInfo.IsNamed && !certificateKeyProvInfo.IsUnknown,variable+": the certificate names no key it can be read for ("
				+certificateKeyProvInfo.Problem+").");
			Assert.IsTrue(certificateKeyProvInfo.IsMachine,variable+": the key is in a user's key set, not the computer's (LocalMachine\\My, as the "
				+"connection settings tool imports it): "+certificateKeyProvInfo.Describe());
			return certificateKeyProvInfo.ProviderType==0;
		}

		private static void Run(MySqlConnection mySqlConnection,string command) {
			using(MySqlCommand mySqlCommand=new MySqlCommand(command,mySqlConnection)) {
				mySqlCommand.ExecuteNonQuery();
			}
		}

		private static Dictionary<string,string> GetSessionValues(MySqlConnection mySqlConnection) {
			Dictionary<string,string> dictValues=new Dictionary<string,string>();
			using(MySqlCommand mySqlCommand=new MySqlCommand("SHOW SESSION STATUS WHERE Variable_name IN ('Ssl_cipher','Ssl_version')",mySqlConnection))
			using(MySqlDataReader mySqlDataReader=mySqlCommand.ExecuteReader()) {
				while(mySqlDataReader.Read()) {
					dictValues[mySqlDataReader.GetString(0).ToLowerInvariant()]=mySqlDataReader.GetString(1);
				}
			}
			using(MySqlCommand mySqlCommand=new MySqlCommand("SELECT CURRENT_USER()",mySqlConnection)) {
				dictValues["current_user"]=Convert.ToString(mySqlCommand.ExecuteScalar(),CultureInfo.InvariantCulture);
			}
			return dictValues;
		}

		[TestMethod]
		public void NonLoopbackServer_VerifyFull_X509User_WithACapiKeyAndWithACngKey() {
			ReadSetup(out string serverTest,out string sslCa,out string thumbprintCapi,out string thumbprintCng);
			Assert.IsFalse(IsCngKey(thumbprintCapi,VARIABLE_CAPI),VARIABLE_CAPI+" names a certificate whose key is in a CNG provider.");
			Assert.IsTrue(IsCngKey(thumbprintCng,VARIABLE_CNG),VARIABLE_CNG+" names a certificate whose key is in a CAPI provider.");
			string suffix=DateTime.UtcNow.ToString("MMddHHmmss",CultureInfo.InvariantCulture)+"_"+Guid.NewGuid().ToString("N").Substring(0,6);
			string database="s22_tls_"+suffix;
			string user="s22_x509_"+suffix;
			using(MySqlConnection mySqlConnectionAdmin=new MySqlConnection(DevDbTls.Apply(ROOT))) {
				mySqlConnectionAdmin.Open();
				try {
					Run(mySqlConnectionAdmin,"CREATE DATABASE `"+database+"` CHARACTER SET utf8mb3");
					Run(mySqlConnectionAdmin,"CREATE TABLE `"+database+"`.sample (SampleNum bigint NOT NULL AUTO_INCREMENT PRIMARY KEY,Txt varchar(50)) ENGINE=MyISAM");
					Run(mySqlConnectionAdmin,"INSERT INTO `"+database+"`.sample (Txt) VALUES ('synthetic tls row')");
					//No password: the certificate is the only credential, so a login proves the certificate was sent and accepted.
					Run(mySqlConnectionAdmin,"CREATE USER '"+user+"'@'%' REQUIRE X509");
					Run(mySqlConnectionAdmin,"GRANT SELECT ON `"+database+"`.* TO '"+user+"'@'%'");
					foreach(string thumbprint in new[] { thumbprintCapi,thumbprintCng }) {
						string label=(thumbprint==thumbprintCapi) ? "CAPI" : "CNG";
						//The program's own string for a server over the network, with this certificate: VerifyFull, the CA, LocalMachine.
						string connectionString=DataConnection.BuildSimpleConnectionString(DatabaseType.MySql,serverTest,database,user,"",sslCa:sslCa,
							certificateThumbprint:thumbprint);
						MySqlConnectionStringBuilder builder=new MySqlConnectionStringBuilder(connectionString);
						Assert.AreEqual(MySqlSslMode.VerifyFull,builder.SslMode,label);
						Assert.AreEqual(sslCa,builder.SslCa,label+": this computer has an office CA (DataConnection.OfficeCaPath), which the string takes "
							+"instead of the test CA.");
						Assert.AreEqual(MySqlCertificateStoreLocation.LocalMachine,builder.CertificateStoreLocation,label);
						using(MySqlConnection mySqlConnection=DbConnectionSecurity.NewMySqlConnection(connectionString)) {
							mySqlConnection.Open();
							Dictionary<string,string> dictValues=GetSessionValues(mySqlConnection);
							Assert.AreNotEqual("",dictValues["ssl_cipher"],label+": the session is encrypted.");
							CollectionAssert.Contains(new[] { "TLSv1.2","TLSv1.3" },dictValues["ssl_version"],label);
							StringAssert.StartsWith(dictValues["current_user"],user+"@",label+": logged in as the REQUIRE X509 user.");
							using(MySqlCommand mySqlCommand=new MySqlCommand("SELECT Txt FROM sample",mySqlConnection)) {
								Assert.AreEqual("synthetic tls row",Convert.ToString(mySqlCommand.ExecuteScalar(),CultureInfo.InvariantCulture),
									label+": the same server the test created the database on.");
							}
							MySqlConnection.ClearPool(mySqlConnection);
						}
					}
					//Without a certificate the REQUIRE X509 user is refused (the rule refuses such a string first, so it goes to MySqlConnector
					//directly), and with another CA the server is refused: the verification over this address is real.
					MySqlConnectionStringBuilder builderNoCertificate=new MySqlConnectionStringBuilder(DataConnection.BuildSimpleConnectionString(
						DatabaseType.MySql,serverTest,database,user,"",sslCa:sslCa,certificateThumbprint:"")) { Pooling=false };
					using(MySqlConnection mySqlConnection=new MySqlConnection(builderNoCertificate.ConnectionString)) {
						MySqlException ex=TestHelpers.AssertThrows<MySqlException>(() => mySqlConnection.Open());
						Assert.AreEqual(MySqlErrorCode.AccessDenied,ex.ErrorCode,ex.Message);
					}
					MySqlConnectionStringBuilder builderOtherCa=new MySqlConnectionStringBuilder(DataConnection.BuildSimpleConnectionString(
						DatabaseType.MySql,serverTest,database,user,"",sslCa:TestHelpers.WriteCaPem(_folder,"other-ca.pem"),certificateThumbprint:thumbprintCng))
						{ Pooling=false };
					using(MySqlConnection mySqlConnection=new MySqlConnection(builderOtherCa.ConnectionString)) {
						TestHelpers.AssertThrows<MySqlException>(() => mySqlConnection.Open());
					}
				}
				finally {
					Run(mySqlConnectionAdmin,"DROP USER IF EXISTS '"+user+"'@'%'");
					Run(mySqlConnectionAdmin,"DROP DATABASE IF EXISTS `"+database+"`");
				}
			}
		}

		[TestMethod]
		public void TheServerVariable_OnlyTheOneLocalTestAddress_AnythingElseFailsBeforeAnyConnection() {
			//Synthetic variables and synthetic addresses of "this computer" (a LAN address among them): no name is looked up and nothing is
			//connected. ReadSetup opens no connection, and a refusal comes before it even reads the CA variable, its last step before the test's
			//first connection.
			List<IPAddress> listLocal=new List<IPAddress> { IPAddress.Parse("127.0.0.1"),IPAddress.Parse("10.0.0.5"),IPAddress.Parse("192.0.2.25"),
				IPAddress.Parse("::1") };
			string pathCa=TestHelpers.WriteCaPem(_folder,"test-ca.pem");
			string notTheRange="is not an IPv4 address from 192.0.2.1 to 192.0.2.254";
			string notLocal="is not one of this computer's own addresses";
			foreach(string[] arrayCase in new[] {
				new[] { "10.0.0.5",notTheRange },//a LAN address, even one this computer has
				new[] { "10.0.0.5:3306",notTheRange },
				new[] { "office-server.example.test",notTheRange },//an office server's name: never resolved
				new[] { "OFFICESERVER",notTheRange },
				new[] { "192.0.2.99",notLocal },//of the range, but not this computer's
				new[] { "192.0.2.99:3306",notLocal },
				new[] { "192.0.2.0",notTheRange },
				new[] { "192.0.2.255",notTheRange },
				new[] { "192.0.2.025",notTheRange },//leading zero
				new[] { "3221225985",notTheRange },//192.0.2.1 as one number, which IPAddress.TryParse takes
				new[] { "192.0.2.25.example.test",notTheRange },
				new[] { "[192.0.2.99]:3306",notLocal },//the program's bracketed notation is read as the address inside
				new[] { "198.51.100.25",notTheRange },//another documentation range
			})
			{
				List<string> listRead=new List<string>();
				Dictionary<string,string> dictVariables=new Dictionary<string,string> { { VARIABLE_SERVER,arrayCase[0] },{ VARIABLE_CAPI,new string('A',40) },
					{ VARIABLE_CNG,new string('B',40) },{ VARIABLE_CA,pathCa } };
				Func<string,string> funcGetVariable=name => {
					listRead.Add(name);
					return dictVariables.TryGetValue(name,out string value) ? value : null;
				};
				AssertFailedException ex=TestHelpers.AssertThrows<AssertFailedException>(() => ReadSetup(funcGetVariable,() => listLocal,out _,out _,out _,out _));
				StringAssert.Contains(ex.Message,VARIABLE_SERVER+"="+arrayCase[0]+": ",arrayCase[0]);
				StringAssert.Contains(ex.Message,arrayCase[1],arrayCase[0]);
				StringAssert.Contains(ex.Message,"lets these tests connect to the one local test address on port 3306 and to nothing else",
					arrayCase[0]+": the message names the owner's exception.");
				CollectionAssert.DoesNotContain(listRead,VARIABLE_CA,arrayCase[0]+": refused before the CA, the last step before a connection.");
			}
			//The loopback address and another port keep their own refusals, also before the CA.
			foreach(string[] arrayCase in new[] { new[] { "127.0.0.1","must be a non-loopback address" },new[] { "192.0.2.25:3307","listens on port 3306" } }) {
				Dictionary<string,string> dictVariables=new Dictionary<string,string> { { VARIABLE_SERVER,arrayCase[0] },{ VARIABLE_CAPI,new string('A',40) },
					{ VARIABLE_CNG,new string('B',40) },{ VARIABLE_CA,pathCa } };
				AssertFailedException ex=TestHelpers.AssertThrows<AssertFailedException>(
					() => ReadSetup(name => dictVariables.TryGetValue(name,out string value) ? value : null,() => listLocal,out _,out _,out _,out _));
				StringAssert.Contains(ex.Message,arrayCase[1],arrayCase[0]);
			}
			//The one local test address passes the set-up, with or without the port.
			foreach(string server in new[] { "192.0.2.25","192.0.2.25:3306"," 192.0.2.25 " }) {
				Dictionary<string,string> dictVariables=new Dictionary<string,string> { { VARIABLE_SERVER,server },{ VARIABLE_CAPI,new string('a',40) },
					{ VARIABLE_CNG,new string('B',40) },{ VARIABLE_CA,pathCa } };
				ReadSetup(name => dictVariables.TryGetValue(name,out string value) ? value : null,() => listLocal,out string serverTest,out string sslCa,
					out string thumbprintCapi,out string thumbprintCng);
				Assert.AreEqual(server.Trim(),serverTest);
				Assert.AreEqual(pathCa,sslCa);
				Assert.AreEqual(new string('A',40),thumbprintCapi);
			}
			//The addresses are this computer's own: GetTestServerProblem takes only an IPv4 address of the list, never an IPv6 one.
			Assert.AreEqual("192.0.2.25 "+notLocal,GetTestServerProblem("192.0.2.25",new[] { IPAddress.Parse("::ffff:192.0.2.25") }));
			Assert.IsNull(GetTestServerProblem("192.0.2.25",listLocal));
		}

		[TestMethod]
		public void WithoutTheVariables_TheCheckIsInconclusive_NeverAPass() {
			//The gate passes the variables (docs/port-notes/S2-2.md, section 6); a run without them must not count as the check.
			if(GetVariable(VARIABLE_SERVER)!="" || GetVariable(VARIABLE_CAPI)!="" || GetVariable(VARIABLE_CNG)!="") {
				Assert.Inconclusive("The variables are set: the check itself runs.");
			}
			UnitTestAssertException exceptionCaught=null;
			try {
				ReadSetup(out _,out _,out _,out _);
			}
			catch(UnitTestAssertException ex) {
				exceptionCaught=ex;
			}
			Assert.IsInstanceOfType(exceptionCaught,typeof(AssertInconclusiveException));
		}
	}
}
