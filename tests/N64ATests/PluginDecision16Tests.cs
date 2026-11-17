// The plug-in test of section 7 of docs/specs/net10-plan.md (decision 16, package S3b-4; docs/port-notes/S3b-4.md): in the .NET 10 program a
// plug-in DLL built for the .NET Framework gets a clear message and is not loaded, and the rebuilt PluginExample (net10.0-windows) loads. The
// .NET Framework plug-ins are a synthetic one (TestSupport.WritePlugin) and a real .NET Framework assembly from the Windows folder; both are
// only read (CodeBase.PortableExecutableFile, System.Reflection.Metadata on .NET), never loaded. PluginExample is the lane's build of it,
// loaded through Plugins.LoadPlugin as the program's PluginLoader does; its Host setter runs its database conversion, so that test uses a
// scratch database on the local server (s3b4_plugin_..., dropped afterwards) and is inconclusive when no server answers.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using CodeBase;
using DataConnectionBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySqlConnector;
using OpenPmsDentalBusiness;
using ProjectFileTests;

namespace N64ATests {
	[TestClass]
	public class PluginDecision16Tests {
		private const string SERVER="127.0.0.1";
		private const string CONNECTION="Server="+SERVER+";Port=3306;User ID=root;Password=;Connection Timeout=15";
		private static string _folder;

		[ClassInitialize]
		public static void ClassInitialize(TestContext testContext) {
			_folder=TestSupport.CreateTempFolder("s3b4_plugin");
		}

		[ClassCleanup]
		public static void ClassCleanup() {
			Plugins.ListPlugins=null;
			TestSupport.DeleteFolder(_folder);
		}

		///<summary>This run is the .NET 10 x64 process the program is.</summary>
		[TestMethod]
		public void Process_IsDotNet10_X64() {
			Assert.IsTrue(Environment.Is64BitProcess);
			Assert.AreEqual(10,Environment.Version.Major);
			PluginImageCheck.GetProcessFramework(out string framework,out Version version);
			Assert.AreEqual(PluginImageCheck.FRAMEWORK_NET_CORE_APP,framework);
			Assert.AreEqual(new Version(10,0),version);
		}

		///<summary>A plug-in built for the .NET Framework (its TargetFrameworkAttribute .NETFramework,Version=v4.8, any CPU, so the bitness lets it
		///through): LoadPlugin returns no plug-in, the error is the clear message of decision 16, it is logged, and the file is not loaded into any
		///load context of this process. The attribute is read from the file with System.Reflection.Metadata.</summary>
		[TestMethod]
		public void NetFrameworkPlugin_GetsTheClearMessage_AndIsNotLoaded() {
			string name="S3b4NetFrameworkPlugin";
			string path=TestSupport.WritePlugin(_folder,name,SyntheticPlatform.AnyCpu,".NETFramework,Version=v4.8");
			PortableExecutableFile portableExecutableFile=PortableExecutableFile.Read(path);
			Assert.AreEqual(".NETFramework,Version=v4.8",portableExecutableFile.TargetFramework);
			Assert.AreEqual("",portableExecutableFile.MetadataError);
			Assert.IsTrue(portableExecutableFile.CanLoadInProcess(PortableExecutableFile.IMAGE_FILE_MACHINE_AMD64),"Any CPU: only the framework refuses it.");
			PluginContainer pluginContainer=Plugins.LoadPlugin(path,name+".dll",42,null,out string error);
			Assert.IsNull(pluginContainer);
			string expected="The plug-in "+name+".dll ("+path+") is not loaded: it is built for .NET Framework 4.8, and this version of the program "
				+"runs on .NET 10.0, which does not load .NET Framework plug-ins. Ask its author for a build for .NET 10.0 (net10.0-windows, any CPU or x64).";
			Assert.AreEqual(expected,error);
			CollectionAssert.Contains(Plugins.GetProblemsLogged(),expected,"The reason is logged.");
			AssertNotLoaded(path);
		}

		///<summary>A real .NET Framework assembly (System.Xml.Linq.dll of the 64-bit .NET Framework 4 folder, IL only, with no
		///TargetFrameworkAttribute: its core library, mscorlib, tells) is refused the same way, without loading it.</summary>
		[TestMethod]
		public void RealNetFrameworkAssembly_IsRefused_WithoutLoadingIt() {
			string path=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows),"Microsoft.NET","Framework64","v4.0.30319",
				"System.Xml.Linq.dll");
			if(!File.Exists(path)) {
				Assert.Inconclusive("No .NET Framework 4 on this computer: "+path);
			}
			PortableExecutableFile portableExecutableFile=PortableExecutableFile.Read(path);
			Assert.AreEqual("mscorlib",portableExecutableFile.CoreLibraryName);
			PluginContainer pluginContainer=Plugins.LoadPlugin(path,"System.Xml.Linq.dll",43,null,out string error);
			Assert.IsNull(pluginContainer);
			StringAssert.StartsWith(error,"The plug-in System.Xml.Linq.dll ("+path+") is not loaded: it is built for .NET Framework 4.0, and this version of "
				+"the program runs on .NET 10.0, which does not load .NET Framework plug-ins.");
			AssertNotLoaded(path);
		}

		///<summary>The rebuilt PluginExample (the lane's build of PluginExample\PluginExample.csproj, net10.0-windows x64) passes the check and
		///loads through LoadPlugin as the program loads it: PluginExample.Plugin, created, Host set, which runs the plug-in's own database
		///conversion (ConvertPluginDatabase.Begin) against a scratch database: its version row and its table are there afterwards.</summary>
		[TestMethod]
		public void RebuiltPluginExample_Loads() {
			string configuration=AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\').EndsWith("Release",StringComparison.OrdinalIgnoreCase) ? "Release"
				: "Debug";
			string path=Path.Combine(TestSupport.Root(),"PluginExample","bin",configuration,"PluginExample.dll");
			BuildOutput.RequireFile(path,path+" is not built. Build PluginExample (the lane: -Projects PluginExample -ScopedRestore -Platform x64).");
			PortableExecutableFile portableExecutableFile=PortableExecutableFile.Read(path);
			Assert.AreEqual(".NETCoreApp,Version=v10.0",portableExecutableFile.TargetFramework);
			Assert.AreEqual(PortableExecutableFile.IMAGE_FILE_MACHINE_AMD64,portableExecutableFile.Machine,"x64, as every project of the .NET 10 build.");
			Assert.IsNull(PluginImageCheck.GetRefusalReason(path,"PluginExample.dll"));
			string dbName="s3b4_plugin_"+DateTime.UtcNow.ToString("yyMMddHHmmss")+"_"+System.Diagnostics.Process.GetCurrentProcess().Id;
			try {
				using(MySqlConnection connection=new MySqlConnection(DevDbTls.Apply(CONNECTION))) {
					connection.Open();
					Exec(connection,"CREATE DATABASE `"+dbName+"` CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci");
				}
			}
			catch(Exception ex) {
				Assert.Inconclusive("No scratch MariaDB database: "+ex.GetBaseException().Message);
			}
			try {
				using(MySqlConnection connection=new MySqlConnection(DevDbTls.Apply(CONNECTION+";Database="+dbName))) {
					connection.Open();
					//The columns of the program's preference table that the plug-in's conversion reads and writes (synthetic database).
					Exec(connection,"CREATE TABLE preference (PrefNum bigint NOT NULL auto_increment,PrefName varchar(255) NOT NULL DEFAULT '',"
						+"ValueString text,Comments text,PRIMARY KEY(PrefNum))");
				}
				RemotingClient.MiddleTierRole=MiddleTierRole.ClientDirect;
				new DataConnection().SetDb(SERVER,dbName,"root","","","",DatabaseType.MySql,true,DevDbTls.GetSslCa(SERVER));
				PluginContainer pluginContainer=Plugins.LoadPlugin(path,"PluginExample.dll",44,null,out string error);
				Assert.IsNotNull(pluginContainer,error);
				Assert.IsNull(error);
				Assert.AreEqual("PluginExample",pluginContainer.Name);
				Assert.AreEqual(44,pluginContainer.ProgramNum);
				Assert.AreEqual("PluginExample.Plugin",pluginContainer.Plugin.GetType().FullName);
				Assert.IsInstanceOfType(pluginContainer.Plugin,typeof(PluginBase));
				Assert.IsTrue(string.Equals(Path.GetFullPath(path),Path.GetFullPath(pluginContainer.Assemb.Location),StringComparison.OrdinalIgnoreCase),
					pluginContainer.Assemb.Location);
				Assert.AreEqual(new Version(7,1,0,0),pluginContainer.Assemb.GetName().Version);
				using(MySqlConnection connection=new MySqlConnection(DevDbTls.Apply(CONNECTION+";Database="+dbName))) {
					connection.Open();
					using MySqlCommand command=new MySqlCommand("SELECT ValueString FROM preference WHERE PrefName='Plugin_JSS_DataBaseVersion'",connection);
					Assert.AreEqual("7.1.0.0",command.ExecuteScalar() as string,"The plug-in's conversion ran up to its own version.");
					using MySqlCommand commandTable=new MySqlCommand("SELECT COUNT(*) FROM information_schema.TABLES WHERE TABLE_SCHEMA='"+dbName
						+"' AND TABLE_NAME='jss_dev_myveryuniquetable'",connection);
					Assert.AreEqual(1L,Convert.ToInt64(commandTable.ExecuteScalar()));
				}
			}
			finally {
				try {
					using(MySqlConnection connection=new MySqlConnection(DevDbTls.Apply(CONNECTION))) {
						connection.Open();
						Exec(connection,"DROP DATABASE IF EXISTS `"+dbName+"`");
					}
				}
				catch(Exception ex) {
					ex.DoNothing();//leaving the scratch database behind is harmless
				}
			}
		}

		///<summary>No assembly of any load context of this process came from path.</summary>
		private static void AssertNotLoaded(string path) {
			string pathFull=Path.GetFullPath(path);
			foreach(AssemblyLoadContext assemblyLoadContext in AssemblyLoadContext.All) {
				foreach(Assembly assembly in assemblyLoadContext.Assemblies) {
					if(assembly.IsDynamic || string.IsNullOrEmpty(assembly.Location)) {
						continue;
					}
					Assert.IsFalse(string.Equals(Path.GetFullPath(assembly.Location),pathFull,StringComparison.OrdinalIgnoreCase),
						"Loaded into "+assemblyLoadContext.Name+": "+assembly.FullName);
				}
			}
		}

		private static void Exec(MySqlConnection connection,string command) {
			using(MySqlCommand mySqlCommand=new MySqlCommand(command,connection)) {
				mySqlCommand.ExecuteNonQuery();
			}
		}
	}
}
