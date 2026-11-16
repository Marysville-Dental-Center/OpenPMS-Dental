// The shared settings of the .NET 10 build (package S3b-1 of the .NET 10 plan, docs/specs/net10-plan.md; docs/port-notes/S3b-1.md):
// Directory.Build.props sets TargetFramework net10.0-windows for every SDK-style project that names none and names Directory.Build.Net.props as
// BeforeMicrosoftNETSdkTargets, which gives a .NET 10 project x64 (the helper x86), a runtime identifier for exes and test projects,
// self-contained exes, framework-dependent test projects, NLS and the two platform attributes; a project that names net48 builds as on main.
// The settings are checked on small projects under the repository's own Directory.Build.props, Directory.Build.Net.props, Directory.Build.targets,
// Directory.Packages.props and tests\Directory.Build.props, copied into a scratch folder, and on every SDK-style project of the solution filter
// evaluated with TargetFramework net10.0-windows (what each gets once its layer package of stage 3b deletes its own net48 line). Also the x64
// solution platforms of OpenDental.sln, the pinned language versions and the reader's import of BeforeMicrosoftNETSdkTargets.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ProjectFileTests {
	[TestClass]
	public class Net10SettingsTests {
		private const string NET10="net10.0-windows";
		private const string HELPER="OpenPmsDentalHelper32";

		private static void AssertThrows<T>(Action action,string expectedText) where T:Exception {
			try {
				action();
			}
			catch(T ex) {
				StringAssert.Contains(ex.Message,expectedText);
				return;
			}
			Assert.Fail("No "+typeof(T).Name+" for "+expectedText);
		}

		///<summary>A scratch folder with the repository's own shared files in place of the fixtures' (FixtureFolder.Create writes those first).</summary>
		private static FixtureFolder CreateWithRepositorySettings() {
			FixtureFolder fixtureFolder=FixtureFolder.Create();
			string root=FixtureFolder.RepoRoot();
			foreach(string file in new[] { "Directory.Build.props","Directory.Build.Net.props","Directory.Build.targets","Directory.Packages.props",
				@"tests\Directory.Build.props" })
			{
				fixtureFolder.WriteFile(file,File.ReadAllText(Path.Combine(root,file)));
			}
			return fixtureFolder;
		}

		private static ProjectFile LoadProject(FixtureFolder fixtureFolder,string relativePath,string properties) {
			return LoadProject(fixtureFolder,relativePath,properties,"");
		}

		private static ProjectFile LoadProject(FixtureFolder fixtureFolder,string relativePath,string properties,string items) {
			fixtureFolder.WriteFile(relativePath,"<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup>"+properties+"</PropertyGroup><ItemGroup>"+items
				+"</ItemGroup></Project>");
			fixtureFolder.WriteFile(Path.Combine(Path.GetDirectoryName(relativePath),"Program.cs"),"");
			return ProjectFile.Load(fixtureFolder.Combine(relativePath));
		}

		///<summary>The values of the RuntimeHostConfigurationOption items of the name (Include), as Name=Value.</summary>
		private static List<string> HostOptions(ProjectFile projectFile) {
			return projectFile.GetItems("RuntimeHostConfigurationOption").Select(x => x.Include+"="+x.GetMetadata("Value")).ToList();
		}

		[TestMethod]
		public void AnExe_IsNet10X64_SelfContainedForWinX64_WithNls() {
			using(FixtureFolder fixtureFolder=CreateWithRepositorySettings()) {
				ProjectFile projectFile=LoadProject(fixtureFolder,@"Program\Program.csproj","<OutputType>WinExe</OutputType><LangVersion>8.0</LangVersion>");
				Assert.AreEqual(NET10,projectFile.GetProperty("TargetFramework"),"Directory.Build.props.");
				Assert.AreEqual(".NETCoreApp",projectFile.GetProperty("TargetFrameworkIdentifier"));
				Assert.AreEqual("x64",projectFile.GetProperty("PlatformTarget"),"Decision 19, whatever the solution platform (x86 here).");
				Assert.AreEqual("win-x64",projectFile.GetProperty("RuntimeIdentifier"));
				Assert.AreEqual("true",projectFile.GetProperty("SelfContained"),"Decision 20.");
				Assert.AreEqual("false",projectFile.GetProperty("AppendRuntimeIdentifierToOutputPath"));
				Assert.AreEqual("false",projectFile.GetProperty("AppendTargetFrameworkToOutputPath"));
				Assert.AreEqual(@"bin\Debug\",projectFile.GetProperty("OutputPath"),"Flat: no platform, framework or runtime identifier folder.");
				Assert.AreEqual(@"obj\Debug\",projectFile.GetProperty("IntermediateOutputPath"));
				Assert.AreEqual("en",projectFile.GetProperty("SatelliteResourceLanguages"));
				Assert.AreEqual("false",projectFile.GetProperty("PublishTrimmed"));
				Assert.AreEqual("8.0",projectFile.GetProperty("LangVersion"),"The project's own.");
				//What the SDK computes for a .NET exe (the evaluator since S3b-1): the managed assembly is Program.dll, the apphost Program.exe.
				Assert.AreEqual(".dll",projectFile.GetProperty("TargetExt"));
				Assert.AreEqual("Program.dll",projectFile.GetProperty("TargetFileName"));
				Assert.AreEqual("true",projectFile.GetProperty("UseAppHost"));
				Assert.AreEqual(".exe",projectFile.GetProperty("_NativeExecutableExtension"));
				Assert.AreEqual("true",projectFile.GetProperty("GenerateRuntimeConfigurationFiles"));
				Assert.AreEqual("true",projectFile.GetProperty("GenerateResourceUsePreserializedResources"));
				Assert.AreEqual("true",projectFile.GetProperty("NoStdLib"));
				CollectionAssert.AreEqual(new[] { "System.Globalization.UseNls=true" },HostOptions(projectFile),"Decision 4.");
				//The SDK writes only its two platform attributes; AssemblyInfo.cs and SharedAssemblyInfo.cs keep the rest.
				Assert.AreEqual("true",projectFile.GetProperty("GenerateAssemblyInfo"));
				foreach(string name in new[] { "GenerateAssemblyVersionAttribute","GenerateAssemblyFileVersionAttribute","GenerateAssemblyTitleAttribute",
					"GenerateAssemblyProductAttribute","GenerateAssemblyCompanyAttribute","GenerateAssemblyCopyrightAttribute",
					"GenerateAssemblyInformationalVersionAttribute","GenerateAssemblyConfigurationAttribute","GenerateAssemblyDescriptionAttribute",
					"GenerateAssemblyTrademarkAttribute","GenerateNeutralResourcesLanguageAttribute","GenerateRepositoryUrlAttribute",
					"GenerateAssemblyMetadataAttributes","GenerateInternalsVisibleToAttributes" })
				{
					Assert.AreEqual("false",projectFile.GetProperty(name),name);
				}
				Assert.AreEqual("",projectFile.GetProperty("GenerateTargetPlatformAttribute"),"Left to the SDK, which writes it.");
				Assert.AreEqual("",projectFile.GetProperty("GenerateSupportedOSPlatformAttribute"),"Left to the SDK, which writes it.");
				CollectionAssert.Contains(projectFile.GetProperty("WarningsAsErrors").Split(';'),"WFDEV006");
				Assert.AreEqual("",projectFile.GetProperty("AutoGenerateBindingRedirects"),"No binding redirects on .NET.");
				Assert.AreNotEqual("true",projectFile.GetProperty("Prefer32Bit"));
			}
		}

		[TestMethod]
		public void ALibrary_IsNet10X64_WithoutRuntimeIdentifierOrNls() {
			using(FixtureFolder fixtureFolder=CreateWithRepositorySettings()) {
				ProjectFile projectFile=LoadProject(fixtureFolder,@"Library\Library.csproj","<OutputType>Library</OutputType>");
				Assert.AreEqual(NET10,projectFile.GetProperty("TargetFramework"));
				Assert.AreEqual("x64",projectFile.GetProperty("PlatformTarget"));
				Assert.AreEqual("",projectFile.GetProperty("SelfContained"));
				Assert.IsFalse(projectFile.IsPropertyKnown("RuntimeIdentifier"),"None set; the SDK's inference is not modelled (MSBuild: '').");
				Assert.AreEqual(".dll",projectFile.GetProperty("TargetExt"));
				Assert.AreEqual("",projectFile.GetProperty("GenerateRuntimeConfigurationFiles"),"No runtime output.");
				Assert.AreEqual("",projectFile.GetProperty("_NativeExecutableExtension"));
				Assert.AreEqual(0,HostOptions(projectFile).Count);
				Assert.AreEqual("true",projectFile.GetProperty("GenerateAssemblyInfo"));
				Assert.AreEqual(@"bin\Debug\",projectFile.GetProperty("OutputPath"));
			}
		}

		[TestMethod]
		public void TheHelper_IsNet10X86_SelfContainedForWinX86() {
			using(FixtureFolder fixtureFolder=CreateWithRepositorySettings()) {
				ProjectFile projectFile=LoadProject(fixtureFolder,HELPER+@"\"+HELPER+".csproj","<OutputType>WinExe</OutputType>");
				Assert.AreEqual(NET10,projectFile.GetProperty("TargetFramework"));
				Assert.AreEqual("x86",projectFile.GetProperty("PlatformTarget"),"O-15: the one 32-bit project.");
				Assert.AreEqual("win-x86",projectFile.GetProperty("RuntimeIdentifier"));
				Assert.AreEqual("true",projectFile.GetProperty("SelfContained"),"Its own x86 runtime, in Helper32\\ (S3b-4).");
				CollectionAssert.AreEqual(new[] { "System.Globalization.UseNls=true" },HostOptions(projectFile));
			}
		}

		[TestMethod]
		public void ATestProject_IsNet10X64FrameworkDependent_SinceS3b5_AndOneThatNamesNet48StaysOnIt() {
			using(FixtureFolder fixtureFolder=CreateWithRepositorySettings()) {
				//A project under tests\ that names net48 itself (TopazStandIn): .NET Framework 4.8 and x86 as before stage 3b, and no binding
				//redirects of its own (a library); the shared test settings still apply.
				ProjectFile projectFile=LoadProject(fixtureFolder,@"tests\Sample.Tests\Sample.Tests.csproj","<TargetFramework>net48</TargetFramework>");
				Assert.AreEqual("net48",projectFile.GetProperty("TargetFramework"),"Its own line.");
				Assert.AreEqual("true",projectFile.GetProperty("ODTestProject"));
				Assert.AreEqual("x86",projectFile.GetProperty("PlatformTarget"));
				Assert.AreEqual("",projectFile.GetProperty("SelfContained"));
				Assert.AreEqual(0,HostOptions(projectFile).Count);
				Assert.AreEqual("false",projectFile.GetProperty("GenerateAssemblyInfo"));
				//The SDK's default for .NET Framework 4.7.2 and later is AutoGenerateBindingRedirects true, but a library writes no .config without
				//GenerateBindingRedirectsOutputType, which only tests\Directory.Build.props set until S3b-5.
				Assert.AreEqual("",projectFile.GetProperty("GenerateBindingRedirectsOutputType"));
				Assert.AreEqual("8.0",projectFile.GetProperty("LangVersion"));
				//Since S3b-5 tests\Directory.Build.props names no target framework: every other test project gets the repository's net10.0-windows.
				StringAssert.DoesNotMatch(File.ReadAllText(fixtureFolder.Combine(@"tests\Directory.Build.props")),
					new System.Text.RegularExpressions.Regex("<TargetFramework>"));
				projectFile=LoadProject(fixtureFolder,@"tests\Net.Tests\Net.Tests.csproj","");
				Assert.AreEqual(NET10,projectFile.GetProperty("TargetFramework"));
				Assert.AreEqual("",projectFile.GetProperty("AppConfig"),"No shared binding-redirect config (tests\\BindingRedirects.config went in S3b-5).");
				Assert.AreEqual("x64",projectFile.GetProperty("PlatformTarget"));
				Assert.AreEqual("win-x64",projectFile.GetProperty("RuntimeIdentifier"));
				Assert.AreEqual("false",projectFile.GetProperty("SelfContained"),"Runs on the build PC's machine-wide x64 runtime.");
				Assert.AreEqual(".exe",projectFile.GetProperty("_NativeExecutableExtension"),"From its runtime identifier (Microsoft.NET.Sdk.Shared.targets).");
				CollectionAssert.AreEqual(new[] { "System.Globalization.UseNls=true" },HostOptions(projectFile));
				Assert.AreEqual("",projectFile.GetProperty("AutoGenerateBindingRedirects"));
				Assert.AreEqual("",projectFile.GetProperty("GenerateBindingRedirectsOutputType"));
				//A test project that is an exe (UnitTests): a test project.
				projectFile=LoadProject(fixtureFolder,@"tests\Exe.Tests\Exe.Tests.csproj","<OutputType>WinExe</OutputType>");
				Assert.AreEqual("false",projectFile.GetProperty("SelfContained"));
				Assert.AreEqual("win-x64",projectFile.GetProperty("RuntimeIdentifier"));
			}
		}

		[TestMethod]
		public void AProjectThatNamesNet48_BuildsAsOnMain() {
			using(FixtureFolder fixtureFolder=CreateWithRepositorySettings()) {
				ProjectFile projectFile=LoadProject(fixtureFolder,@"Program\Program.csproj","<TargetFramework>net48</TargetFramework><OutputType>WinExe</OutputType>");
				Assert.AreEqual(".NETFramework",projectFile.GetProperty("TargetFrameworkIdentifier"));
				Assert.AreEqual("x86",projectFile.GetProperty("PlatformTarget"));
				Assert.AreEqual("",projectFile.GetProperty("SelfContained"));
				Assert.AreEqual("false",projectFile.GetProperty("GenerateAssemblyInfo"));
				Assert.AreEqual("",projectFile.GetProperty("PublishTrimmed"));
				Assert.AreEqual(0,HostOptions(projectFile).Count);
				Assert.IsFalse(projectFile.GetProperty("WarningsAsErrors").Split(';').Contains("WFDEV006"));
				Assert.AreEqual("true",projectFile.GetProperty("AutoGenerateBindingRedirects"),"The SDK's default for a .NET Framework exe.");
				Assert.AreEqual(".exe",projectFile.GetProperty("TargetExt"));
				Assert.AreEqual("",projectFile.GetProperty("NoStdLib"));
				Assert.AreEqual(@"bin\Debug\",projectFile.GetProperty("OutputPath"));
			}
		}

		[TestMethod]
		public void ThePackageVersions_FollowTheTargetFramework() {
			using(FixtureFolder fixtureFolder=CreateWithRepositorySettings()) {
				const string ITEMS="<PackageReference Include=\"Microsoft.Web.WebView2\" /><PackageReference Include=\"Mono.Cecil\" />"
					+"<PackageReference Include=\"System.Management\" />";
				ProjectFile projectFileNet=LoadProject(fixtureFolder,@"Library\Library.csproj","<OutputType>Library</OutputType>",ITEMS);
				ProjectFile projectFileNet48=LoadProject(fixtureFolder,@"Old\Old.csproj","<TargetFramework>net48</TargetFramework>",ITEMS);
				Assert.AreEqual("1.0.4314.35",projectFileNet.GetPackageVersion("Microsoft.Web.WebView2"),"Batch D (S3b-0 item 5).");
				Assert.AreEqual("1.0.2420.47",projectFileNet48.GetPackageVersion("Microsoft.Web.WebView2"));
				Assert.AreEqual("0.11.6",projectFileNet.GetPackageVersion("Mono.Cecil"));
				Assert.AreEqual("0.10.3",projectFileNet48.GetPackageVersion("Mono.Cecil"));
				Assert.AreEqual("10.0.15",projectFileNet.GetPackageVersion("System.Management"),"The runtime version of the pinned SDK 10.0.115.");
			}
		}

		[TestMethod]
		public void EverySdkStyleProject_RetargetedToNet10_GetsTheSettingsOfItsKind() {
			string root=FixtureFolder.RepoRoot();
			Dictionary<string,string> dictGlobal=new Dictionary<string,string> { { "TargetFramework",NET10 } };
			List<string> listProblems=new List<string>();
			int countExes=0;
			int countTests=0;
			int countLibraries=0;
			foreach(string path in ProjectFile.GetModelledSolutionFilterProjects(Path.Combine(root,"OpenDental.Local.slnf"))) {
				ProjectFile projectFileNow=ProjectFile.Load(path);
				if(!projectFileNow.IsSdkStyle) {
					continue;
				}
				string name=Path.GetFileNameWithoutExtension(path);
				//Today: the settings follow the project's own target framework.
				bool isNet10Now=projectFileNow.GetProperty("TargetFramework")==NET10;
				string platformNow=projectFileNow.GetProperty("PlatformTarget");
				string expectedNow=!isNet10Now ? "x86" : name==HELPER ? "x86" : "x64";
				if(platformNow!=expectedNow) {
					listProblems.Add(name+": PlatformTarget "+platformNow+" on "+projectFileNow.GetProperty("TargetFramework"));
				}
				ProjectFile projectFile;
				try {
					projectFile=ProjectFile.Load(path,dictGlobal);
				}
				catch(NotSupportedException ex) {
					listProblems.Add(name+" on "+NET10+": "+ex.Message);
					continue;
				}
				bool isHelper=name==HELPER;
				bool isTest=projectFile.GetProperty("ODTestProject")=="true";
				bool isExe=new[] { "Exe","WinExe" }.Contains(projectFile.GetProperty("OutputType"));
				Dictionary<string,string> dictExpected=new Dictionary<string,string>();
				dictExpected["PlatformTarget"]=isHelper ? "x86" : "x64";
				dictExpected["SelfContained"]=isTest ? "false" : isExe ? "true" : "";
				dictExpected["AppendRuntimeIdentifierToOutputPath"]="false";
				dictExpected["SatelliteResourceLanguages"]="en";
				dictExpected["GenerateAssemblyInfo"]="true";
				dictExpected["GenerateAssemblyVersionAttribute"]="false";
				if(isHelper) {
					dictExpected["RuntimeIdentifier"]="win-x86";
				}
				else if(isExe || isTest) {
					dictExpected["RuntimeIdentifier"]="win-x64";
				}
				foreach(KeyValuePair<string,string> pair in dictExpected) {
					string value=projectFile.GetProperty(pair.Key);
					if(value!=pair.Value) {
						listProblems.Add(name+": "+pair.Key+" "+value+", expected "+pair.Value);
					}
				}
				string outputPath=projectFile.GetProperty("OutputPath");
				if(!Regex.IsMatch(outputPath,@"^bin\\(Debug|Release)\\$") && !outputPath.EndsWith(@"\bin\Debug\",StringComparison.OrdinalIgnoreCase)) {
					listProblems.Add(name+": OutputPath "+outputPath+" is not flat");
				}
				if(!projectFile.GetProperty("WarningsAsErrors").Split(';').Contains("WFDEV006")) {
					listProblems.Add(name+": no WFDEV006 in WarningsAsErrors");
				}
				//The repository's options; a .NET test project that is restored also gets the MSTest adapter's own (its build files'
				//MSTest.EnableParentProcessQuery, read from the restore output since tests\RdlCodeTests, S3b-2b).
				List<string> listOptions=HostOptions(projectFile).Where(x => !(isTest && x.StartsWith("MSTest.",StringComparison.Ordinal))).ToList();
				bool hasNls=listOptions.SequenceEqual(new[] { "System.Globalization.UseNls=true" });
				if((isExe || isTest) ? !hasNls : listOptions.Count!=0) {
					listProblems.Add(name+": RuntimeHostConfigurationOption "+string.Join(",",listOptions));
				}
				if(isTest) {
					countTests++;
				}
				else if(isExe) {
					countExes++;
				}
				else {
					countLibraries++;
				}
			}
			Assert.AreEqual(0,listProblems.Count,listProblems.Count+" problems: "+string.Join("; ",listProblems.Take(30)));
			Assert.IsTrue(countTests>=100,countTests+" test projects.");
			Assert.IsTrue(countExes>=12,countExes+" exes: the program, its tools and the helper at least.");
			Assert.IsTrue(countLibraries>=15,countLibraries+" libraries.");
		}

		[TestMethod]
		public void EverySdkStyleProject_PinsItsLanguageVersion() {
			//S3b-1: a .NET 10 project would otherwise get C# 14, the SDK's default for net10.0, or whatever a newer compiler calls latest. The
			//third-party source builds (third-party\build: upstream's language versions, latest and preview) are S3b-2's.
			string root=FixtureFolder.RepoRoot();
			List<string> listProblems=new List<string>();
			foreach(string path in ProjectFile.GetModelledSolutionFilterProjects(Path.Combine(root,"OpenDental.Local.slnf"))) {
				if(path.Substring(root.Length+1).StartsWith(@"third-party\",StringComparison.OrdinalIgnoreCase)) {
					continue;
				}
				ProjectFile projectFile=ProjectFile.Load(path);
				if(!projectFile.IsSdkStyle) {
					continue;
				}
				string langVersion=projectFile.GetProperty("LangVersion");
				decimal version;
				if(!decimal.TryParse(langVersion,NumberStyles.AllowDecimalPoint,CultureInfo.InvariantCulture,out version)) {
					listProblems.Add(Path.GetFileName(path)+": LangVersion '"+langVersion+"'");
				}
			}
			Assert.AreEqual(0,listProblems.Count,string.Join("; ",listProblems));
		}

		[TestMethod]
		public void DirectoryBuildTargets_BindingRedirectRules_OnlyTheValueTupleOneIsLeft_ForNetFrameworkOnly() {
			//S3b-1 kept both binding-redirect rules of Directory.Build.targets while projects targeted .NET Framework 4.8; S3b-5, which retargets
			//the test projects, deleted the test projects' AppConfig rule with tests\BindingRedirects.config. The System.ValueTuple target stays for
			//.NET Framework only, as long as tests\TopazStandIn targets it (a library, which generates no redirects).
			const string FRAMEWORK_ONLY="'$(TargetFrameworkIdentifier)' == '.NETFramework'";
			string root=FixtureFolder.RepoRoot();
			XDocument xDocument=XDocument.Load(Path.Combine(root,"Directory.Build.targets"));
			XNamespace xNamespace=xDocument.Root.Name.Namespace;
			List<XElement> listTargets=xDocument.Root.Elements(xNamespace+"Target").Where(x => (string)x.Attribute("Name")=="ODDropValueTupleRedirect").ToList();
			Assert.AreEqual(1,listTargets.Count,"The System.ValueTuple redirect target stays for .NET Framework.");
			Assert.AreEqual(FRAMEWORK_ONLY,(string)listTargets[0].Attribute("Condition"));
			Assert.AreEqual(0,xDocument.Root.Descendants(xNamespace+"AppConfig").Count(),"No AppConfig rule (S3b-5).");
			Assert.IsFalse(File.Exists(Path.Combine(root,@"tests\BindingRedirects.config")),"tests\\BindingRedirects.config went with S3b-5.");
			using(FixtureFolder fixtureFolder=CreateWithRepositorySettings()) {
				//A test project without an app.config gets none, on .NET 10 and on .NET Framework alike.
				ProjectFile projectFile=LoadProject(fixtureFolder,@"tests\Sample.Tests\Sample.Tests.csproj","");
				Assert.AreEqual(".NETCoreApp",projectFile.GetProperty("TargetFrameworkIdentifier"));
				Assert.AreEqual("",projectFile.GetProperty("AppConfig"));
				projectFile=LoadProject(fixtureFolder,@"tests\Old.Tests\Old.Tests.csproj","<TargetFramework>net48</TargetFramework>");
				Assert.AreEqual(".NETFramework",projectFile.GetProperty("TargetFrameworkIdentifier"));
				Assert.AreEqual("",projectFile.GetProperty("AppConfig"));
			}
			//The repository's own test projects: SshHostKeyTests (no app.config of its own, the example until S3b-5) is .NET 10 and gets none;
			//TopazStandIn is the one project that names net48.
			ProjectFile projectFileSsh=ProjectFile.Load(Path.Combine(root,@"tests\SshHostKeyTests\SshHostKeyTests.csproj"));
			Assert.AreEqual(NET10,projectFileSsh.GetProperty("TargetFramework"));
			Assert.AreEqual("",projectFileSsh.GetProperty("AppConfig"));
			ProjectFile projectFileTopaz=ProjectFile.Load(Path.Combine(root,@"tests\TopazStandIn\TopazStandIn.csproj"));
			Assert.AreEqual("net48",projectFileTopaz.GetProperty("TargetFramework"));
			Assert.AreEqual("",projectFileTopaz.GetProperty("AppConfig"));
		}

		///<summary>Each DotNetCliToolReference (the per-project CLI tool of Microsoft.XmlSerializer.Generator, which builds the business library's
		///and the program's XmlSerializers files, S3b-3 and S3b-4) names the version of Directory.Packages.props: central package management does
		///not cover that item, so an SDK bump that moves the PackageVersion must move both items too (S3b-4's handoff, S3b-5).</summary>
		[TestMethod]
		public void EveryDotNetCliToolReference_HasTheCentralPackageVersion() {
			string root=FixtureFolder.RepoRoot();
			XDocument xDocumentPackages=XDocument.Load(Path.Combine(root,"Directory.Packages.props"));
			Dictionary<string,List<string>> dictVersions=xDocumentPackages.Descendants().Where(x => x.Name.LocalName=="PackageVersion")
				.GroupBy(x => (string)x.Attribute("Include"),StringComparer.OrdinalIgnoreCase)
				.ToDictionary(x => x.Key,x => x.Select(y => (string)y.Attribute("Version")).ToList(),StringComparer.OrdinalIgnoreCase);
			List<string> listFound=new List<string>();
			List<string> listProblems=new List<string>();
			foreach(string pathProject in Directory.GetFiles(root,"*.csproj",SearchOption.AllDirectories)
				.Where(x => !x.Contains(@"\bin\") && !x.Contains(@"\obj\") && !x.Contains(@"\third-party\") && !x.Contains(@"\.claude\") && !x.Contains(@"\spike\")))
			{
				foreach(XElement xElement in XDocument.Load(pathProject).Descendants().Where(x => x.Name.LocalName=="DotNetCliToolReference")) {
					string include=(string)xElement.Attribute("Include");
					string version=(string)xElement.Attribute("Version");
					string label=pathProject.Substring(root.Length).TrimStart('\\')+": "+include+" "+version;
					listFound.Add(label);
					if(!dictVersions.TryGetValue(include,out List<string> listVersions) || listVersions.Count!=1 || listVersions[0]!=version) {
						listProblems.Add(label+", Directory.Packages.props: "+(listVersions==null ? "none" : string.Join(", ",listVersions)));
					}
				}
			}
			CollectionAssert.AreEquivalent(new[] { "OpenPmsDental\\OpenPmsDental.csproj","OpenPmsDentalBusiness\\OpenPmsDentalBusiness.csproj" },
				listFound.Select(x => x.Substring(0,x.IndexOf(':'))).ToArray(),string.Join("\r\n",listFound));
			Assert.AreEqual(0,listProblems.Count,string.Join("\r\n",listProblems));
		}

		[TestMethod]
		public void TheSolution_HasX64Platforms_MappedAsItsX86Ones() {
			string textSln=File.ReadAllText(Path.Combine(FixtureFolder.RepoRoot(),"OpenDental.sln"));
			StringAssert.Contains(textSln,"\t\tDebug|x64 = Debug|x64\r\n");
			StringAssert.Contains(textSln,"\t\tRelease|x64 = Release|x64\r\n");
			StringAssert.Contains(textSln,"\t\tDebug|x86 = Debug|x86\r\n","x86 stays until nothing builds it.");
			StringAssert.Contains(textSln,"\t\tDebug|Any CPU = Debug|Any CPU\r\n","Any CPU stays until nothing builds it.");
			Dictionary<string,string> dictMappings=new Dictionary<string,string>();
			foreach(Match match in Regex.Matches(textSln,@"^\t\t(\{[0-9A-Fa-f-]+\}\.(?:Debug|Release)\|(?:x86|x64)\.(?:ActiveCfg|Build\.0)) = ([^\r\n]+)\r$",
				RegexOptions.Multiline))
			{
				dictMappings[match.Groups[1].Value]=match.Groups[2].Value;
			}
			//The exception keeps its x86 project configuration for the x64 solution platforms: OpenPmsDentalHelper32 names x86 as its only
			//platform (O-15), as its Any CPU mappings do. OpenPmsDentalServer, the classic project that kept x86 until S3b-6 (it had no x64
			//configuration), is an SDK-style project since then and maps x64 to x64 like the others.
			string[] arrayX86Projects=new[] { "{CDB4F87A-1B32-436C-9096-74BE4D9FD071}" };
			StringAssert.Contains(textSln,"\""+HELPER+"\", \""+HELPER+"\\"+HELPER+".csproj\", \""+arrayX86Projects[0]+"\"");
			StringAssert.Contains(textSln,"\t\t{712C6A2B-88E1-49E9-AC67-F6372F0047BC}.Debug|x64.ActiveCfg = Debug|x64\r\n","OpenPmsDentalServer (S3b-6).");
			List<string> listProblems=new List<string>();
			int countX86=0;
			foreach(KeyValuePair<string,string> pair in dictMappings.Where(x => x.Key.Contains("|x86."))) {
				countX86++;
				string keyX64=pair.Key.Replace("|x86.","|x64.");
				string expected=arrayX86Projects.Any(x => pair.Key.StartsWith(x+".")) ? pair.Value : Regex.Replace(pair.Value,@"\|(x86|Win32)$","|x64");
				string actual;
				if(!dictMappings.TryGetValue(keyX64,out actual)) {
					listProblems.Add(keyX64+" missing");
				}
				else if(actual!=expected) {
					listProblems.Add(keyX64+" = "+actual+", expected "+expected);
				}
			}
			Assert.AreEqual(countX86,dictMappings.Count-countX86,"As many x64 mappings as x86 ones.");
			Assert.IsTrue(countX86>300,countX86+" x86 mappings.");
			Assert.AreEqual(0,listProblems.Count,string.Join("; ",listProblems.Take(20)));
		}

		[TestMethod]
		public void EveryClassicProject_HasTheConfigurationItsX64SolutionPlatformMapsTo() {
			//A classic project reads its OutputPath from a Configuration|Platform group; for a project configuration it has no group for, MSBuild's
			//default bin\<platform>\<configuration>\ stands in for the evaluation and the build stops in _CheckForInvalidConfigurationAndPlatform
			//(ProjectFormatTests.Classic_TheConfigurationDecidesTheConditionalProperties). So the x64 solution platforms must map every classic project
			//to a configuration that gives it the OutputPath of its x86 mapping, which the build lane uses today.
			string root=FixtureFolder.RepoRoot();
			List<string> listProblems=new List<string>();
			int countClassic=0;
			foreach(string path in ProjectFile.GetModelledSolutionFilterProjects(Path.Combine(root,"OpenDental.Local.slnf"))) {
				if(!path.EndsWith(".csproj",StringComparison.OrdinalIgnoreCase) || ProjectFile.Load(path).IsSdkStyle) {
					continue;
				}
				countClassic++;
				foreach(string configuration in new[] { "Debug","Release" }) {
					string configurationX86;
					string platformX86;
					ProjectFile.GetSolutionConfiguration(path,configuration,"x86",out configurationX86,out platformX86);
					string configurationX64;
					string platformX64;
					ProjectFile.GetSolutionConfiguration(path,configuration,"x64",out configurationX64,out platformX64);
					string outputPathX86=ProjectFile.Load(path,configurationX86,platformX86).GetProperty("OutputPath");
					string outputPathX64=ProjectFile.Load(path,configurationX64,platformX64).GetProperty("OutputPath");
					if(outputPathX64!=outputPathX86) {
						listProblems.Add(Path.GetFileName(path)+" "+configuration+"|x64 -> "+configurationX64+"|"+platformX64+": OutputPath "+outputPathX64
							+", for "+configuration+"|x86 "+outputPathX86);
					}
				}
			}
			Assert.AreEqual(0,listProblems.Count,string.Join("; ",listProblems));
			//OpenPmsDentalServer, the last classic C# project of the filter, became SDK-style in S3b-6, so the loop finds none since then; the
			//check stays for any classic project added later.
			Assert.IsTrue(ProjectFile.IsSdkStyleDocument(XDocument.Load(Path.Combine(root,"OpenPmsDentalServer","OpenPmsDentalServer.csproj"))),
				"OpenPmsDentalServer is SDK-style since S3b-6 ("+countClassic+" classic projects).");
		}

		///<summary>The Middle Tier server (S3b-6) is a Microsoft.NET.Sdk.Web project, which ProjectFile does not model, so the tests that evaluate
		///every project of the filter leave it out (GetModelledSolutionFilterProjects). It is the only one, and its file is checked here: a .NET 10
		///exe (no target framework of its own, so the repository's net10.0-windows with the settings of Directory.Build.Net.props), its pinned
		///language version, Windows Forms for the business library, central package versions, the shared assembly info. When it is built, its
		///runtimeconfig names the three runtimes it carries (self-contained) and NLS.</summary>
		[TestMethod]
		public void TheMiddleTierServer_IsTheOneWebSdkProject_ANet10Exe() {
			string root=FixtureFolder.RepoRoot();
			string pathServer=Path.Combine(root,"OpenPmsDentalServer","OpenPmsDentalServer.csproj");
			List<string> listUnmodelled=ProjectFile.GetSolutionFilterProjects(Path.Combine(root,"OpenDental.Local.slnf"))
				.Where(x => ProjectFile.GetUnmodelledSdk(x)!=null).ToList();
			CollectionAssert.AreEqual(new[] { pathServer },listUnmodelled);
			Assert.AreEqual("Microsoft.NET.Sdk.Web",ProjectFile.GetUnmodelledSdk(pathServer));
			AssertThrows<NotSupportedException>(() => ProjectFile.Load(pathServer),"the SDK Microsoft.NET.Sdk.Web is not modelled");
			XDocument xDocument=XDocument.Load(pathServer);
			Dictionary<string,string> dictProperties=new Dictionary<string,string>();
			foreach(XElement xElementGroup in xDocument.Root.Elements().Where(x => x.Name.LocalName=="PropertyGroup" && x.Attribute("Condition")==null)) {
				foreach(XElement xElementProperty in xElementGroup.Elements()) {
					dictProperties[xElementProperty.Name.LocalName]=xElementProperty.Value.Trim();
				}
			}
			Assert.IsFalse(dictProperties.ContainsKey("TargetFramework"),"The repository's net10.0-windows.");
			Assert.IsFalse(dictProperties.ContainsKey("TargetFrameworks"));
			Assert.AreEqual("Exe",dictProperties["OutputType"]);
			Assert.AreEqual("OpenPmsDentalServer",dictProperties["AssemblyName"]);
			Assert.AreEqual("8.0",dictProperties["LangVersion"]);
			Assert.AreEqual("true",dictProperties["UseWindowsForms"]);
			List<XElement> listPackageReferences=xDocument.Descendants().Where(x => x.Name.LocalName=="PackageReference").ToList();
			Assert.IsTrue(listPackageReferences.Any(x => (string)x.Attribute("Include")=="Microsoft.Extensions.Hosting.WindowsServices"));
			Assert.IsTrue(listPackageReferences.All(x => x.Attribute("Version")==null),"Versions come from Directory.Packages.props.");
			Assert.IsTrue(xDocument.Descendants().Any(x => x.Name.LocalName=="ProjectReference"
				&& (string)x.Attribute("Include")==@"..\OpenPmsDentalBusiness\OpenPmsDentalBusiness.csproj"));
			Assert.IsTrue(xDocument.Descendants().Any(x => x.Name.LocalName=="Compile" && (string)x.Attribute("Include")==@"..\SharedAssemblyInfo.cs"));
			string pathRuntimeConfig=Path.Combine(root,"OpenPmsDentalServer","bin","Debug","OpenPmsDentalServer.runtimeconfig.json");
			if(!File.Exists(pathRuntimeConfig)) {
				return;//not built: the file's checks above are the test
			}
			string textRuntimeConfig=File.ReadAllText(pathRuntimeConfig);
			StringAssert.Contains(textRuntimeConfig,"\"includedFrameworks\"","Self-contained.");
			foreach(string framework in new[] { "Microsoft.NETCore.App","Microsoft.WindowsDesktop.App","Microsoft.AspNetCore.App" }) {
				StringAssert.Contains(textRuntimeConfig,"\""+framework+"\"");
			}
			StringAssert.Contains(textRuntimeConfig,"\"System.Globalization.UseNls\": true");
		}

		[TestMethod]
		public void BeforeMicrosoftNETSdkTargets_IsReadAfterTheProject_AndARelativeOrMissingFileThrows() {
			using(FixtureFolder fixtureFolder=FixtureFolder.Create()) {
				//A file the project names, read after its own properties (it sees them) and before the SDK's targets.
				fixtureFolder.WriteFile(@"Shared\After.props","<Project><PropertyGroup><FixtureSeen>[$(FixtureOwn)]</FixtureSeen>"
					+"<OutputType Condition=\"'$(FixtureOwn)' == 'yes'\">WinExe</OutputType></PropertyGroup></Project>");
				fixtureFolder.EditProject("SdkStyle","<PropertyGroup>","<PropertyGroup><FixtureOwn>yes</FixtureOwn>"
					+"<BeforeMicrosoftNETSdkTargets>"+fixtureFolder.Combine(@"Shared\After.props")+"</BeforeMicrosoftNETSdkTargets>");
				ProjectFile projectFile=fixtureFolder.Load("SdkStyle");
				Assert.AreEqual("[yes]",projectFile.GetProperty("FixtureSeen"));
				Assert.AreEqual("WinExe",projectFile.GetProperty("OutputType"));
				Assert.AreEqual("true",projectFile.GetProperty("_IsExecutable"),"Microsoft.NET.Sdk.BeforeCommon.targets sees it.");
				//Relative: MSBuild resolves it from Sdk.targets' folder.
				fixtureFolder.EditProject("SdkStyle",fixtureFolder.Combine(@"Shared\After.props"),@"..\..\Shared\After.props");
				AssertThrows<NotSupportedException>(() => fixtureFolder.Load("SdkStyle"),"a relative $(BeforeMicrosoftNETSdkTargets)");
				//Missing: MSBuild's import has no Exists condition.
				fixtureFolder.EditProject("SdkStyle",@"..\..\Shared\After.props",fixtureFolder.Combine(@"Shared\Missing.props"));
				AssertThrows<NotSupportedException>(() => fixtureFolder.Load("SdkStyle"),"names a file that is not there");
			}
		}
	}
}
