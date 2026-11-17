// The harness's own pieces that S3a-8b of the .NET 10 move changed, checked without the opt-in and without starting the program (no window
// is made): AssemblyProbe (Mono.Cecil) gives the same texts as the reflection it replaced, an in-process session's process is the program's own
// process in the program's output folder (its runtime, runtimeconfig, deps.json and .dll.config, OpenPmsDental.dll as its entry assembly;
// WINCHK-5), and a check's result crosses from that process unchanged. Scratch folders in the temp folder, synthetic assemblies and texts; the
// only process started is OpenPmsDental.exe with TestProbeHost's startup hook, which loads OpenPmsDental.dll without running its Main.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Cecil;
using TestProbeHost;
using WindowChecksTests.InProc;

namespace WindowChecksTests {
	[TestClass]
	public class HarnessTests {
		private static string NewTempFolder() {
			string folder=Path.Combine(Path.GetTempPath(),"winchk_harness_"+Guid.NewGuid().ToString("N"));
			Directory.CreateDirectory(folder);
			return folder;
		}

		private static void DeleteQuietly(string folder) {
			try {
				Directory.Delete(folder,true);
			}
			catch(Exception) {
				//Left in the temp folder.
			}
		}

		///<summary>A synthetic library with two AssemblyMetadata attributes (one of them a build stamp as OpenPmsDental.csproj writes it) and an
		///XmlSerializerVersionAttribute as sgen writes it into an XmlSerializers assembly, written with Mono.Cecil.</summary>
		private static string WriteSyntheticAssembly(string folder,string parentAssemblyId) {
			ModuleParameters moduleParameters=new ModuleParameters();
			moduleParameters.Kind=ModuleKind.Dll;
			moduleParameters.Runtime=TargetRuntime.Net_4_0;
			string path=Path.Combine(folder,"WinChkSynthetic.dll");
			using(AssemblyDefinition assemblyDefinition=AssemblyDefinition.CreateAssembly(new AssemblyNameDefinition("WinChkSynthetic",new Version(1,2,3,4)),
				"WinChkSynthetic.dll",moduleParameters))
			{
				ModuleDefinition moduleDefinition=assemblyDefinition.MainModule;
				MethodReference methodReferenceMetadata=moduleDefinition.ImportReference(typeof(AssemblyMetadataAttribute)
					.GetConstructor(new[] { typeof(string),typeof(string) }));
				foreach(string[] arrayPair in new[] { new[] { "ForkBuildUtc","2026-01-02T03:04:05Z" },new[] { "Synthetic Key","a \"quoted\" value" } }) {
					CustomAttribute customAttribute=new CustomAttribute(methodReferenceMetadata);
					customAttribute.ConstructorArguments.Add(new CustomAttributeArgument(moduleDefinition.TypeSystem.String,arrayPair[0]));
					customAttribute.ConstructorArguments.Add(new CustomAttributeArgument(moduleDefinition.TypeSystem.String,arrayPair[1]));
					assemblyDefinition.CustomAttributes.Add(customAttribute);
				}
				CustomAttribute customAttributeSerializer=new CustomAttribute(moduleDefinition.ImportReference(
					typeof(System.Xml.Serialization.XmlSerializerVersionAttribute).GetConstructor(Type.EmptyTypes)));
				customAttributeSerializer.Properties.Add(new Mono.Cecil.CustomAttributeNamedArgument("ParentAssemblyId",
					new CustomAttributeArgument(moduleDefinition.TypeSystem.String,parentAssemblyId)));
				customAttributeSerializer.Properties.Add(new Mono.Cecil.CustomAttributeNamedArgument("Version",
					new CustomAttributeArgument(moduleDefinition.TypeSystem.String,"4.0.0.0")));
				assemblyDefinition.CustomAttributes.Add(customAttributeSerializer);
				assemblyDefinition.Write(path);
			}
			return path;
		}

		///<summary>AssemblyProbe reads with Mono.Cecil what the reflection-only loads read before, in the same texts: the AssemblyMetadata lines as
		///CustomAttributeTypedArgument.ToString writes the arguments (A3's regular expression expects them), the MVID, and the serializer
		///attribute's named arguments in their order (A8 looks for "ParentAssemblyId = MVID,"). Compared with reflection over the same bytes.
		///</summary>
		[TestMethod]
		public void AssemblyProbe_GivesTheTextsReflectionGave() {
			string folder=NewTempFolder();
			try {
				string parentAssemblyId=Guid.NewGuid()+",";
				string path=WriteSyntheticAssembly(folder,parentAssemblyId);
				Assembly assembly=Assembly.Load(File.ReadAllBytes(path));//metadata only: the assembly has no code
				string[] arrayReflection=assembly.GetCustomAttributesData().Where(x => x.AttributeType.Name=="AssemblyMetadataAttribute")
					.Select(x => string.Join(" = ",x.ConstructorArguments.Select(y => y.ToString()))).ToArray();
				string[] arrayProbe=AssemblyProbe.GetMetadata(folder,"WinChkSynthetic.dll");
				CollectionAssert.AreEqual(arrayReflection,arrayProbe,string.Join(" | ",arrayProbe));
				CollectionAssert.Contains(arrayProbe,"\"ForkBuildUtc\" = \"2026-01-02T03:04:05Z\"");
				Assert.AreEqual(assembly.ManifestModule.ModuleVersionId.ToString(),AssemblyProbe.GetMvid(folder,"WinChkSynthetic.dll"));
				CustomAttributeData customAttributeData=assembly.GetCustomAttributesData().Single(x => x.AttributeType.Name=="XmlSerializerVersionAttribute");
				string serializerReflection=string.Join("; ",customAttributeData.NamedArguments.Select(x => x.MemberName+" = "+x.TypedValue.Value));
				Assert.AreEqual(serializerReflection,AssemblyProbe.GetSerializerVersion(folder,"WinChkSynthetic.dll"));
				Assert.AreEqual("ParentAssemblyId = "+parentAssemblyId+"; Version = 4.0.0.0",AssemblyProbe.GetSerializerVersion(folder,"WinChkSynthetic.dll"));
				//An assembly without the attribute: "", as before.
				Assert.AreEqual("",AssemblyProbe.GetSerializerVersion(Path.GetDirectoryName(typeof(HarnessTests).Assembly.Location),"Mono.Cecil.dll"));
				//Nothing is locked: the file can be deleted at once.
				File.Delete(path);
			}
			finally {
				DeleteQuietly(folder);
			}
		}

		///<summary>An in-process session's process (InProcHost.StartSessionProcess; a new AppDomain before S3a-8b, a copy of TestProbeHost with
		///OpenPmsDental.exe.config on .NET Framework): on .NET 10 the program's own process, its apphost OpenPmsDental.exe started from the
		///program's output folder (OpenPmsDental\bin\Debug) with TestProbeHost's startup hook. Its application base and working folder are that
		///folder; .NET reads the folder's files and nothing else: its self-contained runtime (System.Private.CoreLib and Windows Forms load from the
		///folder, not from a machine-wide .NET), OpenPmsDental.deps.json (the host's deps file), every property of OpenPmsDental.runtimeconfig.json
		///(the NLS mode of decision 4 among them) and OpenPmsDental.dll.config (System.Configuration's file, and the program's Properties.Settings
		///values from it); OpenPmsDental loads from the folder's OpenPmsDental.dll and is the entry assembly from the start, so the patch has nothing
		///to set; the startup hook's variable is gone (no process the program starts inherits it). Nothing is copied into the folder, and the
		///process is gone after Dispose. The program's Main does not run and no Start command is sent, so no window is made.</summary>
		[TestMethod]
		public void SessionProcess_RunsFromTheProgramFolder_WithItsConfig() {
			string folder=WinChkEnv.DebugFolder;
			string pathExe=Path.Combine(folder,"OpenPmsDental.exe");
			string pathDll=Path.Combine(folder,"OpenPmsDental.dll");
			if(!File.Exists(pathExe) || !File.Exists(pathDll)) {
				ProjectFileTests.BuildOutput.NotBuilt("OpenPmsDental is not built in "+folder+". Build OpenDental.Local.slnf (Debug|x64) first.");
				return;
			}
			string pathRuntimeConfig=Path.Combine(folder,"OpenPmsDental.runtimeconfig.json");
			string pathDeps=Path.Combine(folder,"OpenPmsDental.deps.json");
			string pathConfig=Path.Combine(folder,"OpenPmsDental.dll.config");
			Assert.IsTrue(File.Exists(pathRuntimeConfig) && File.Exists(pathDeps) && File.Exists(pathConfig),"The program's .runtimeconfig.json, .deps.json "
				+"and .dll.config are beside it in "+folder);
			Assert.IsTrue(File.Exists(Path.Combine(folder,"hostfxr.dll")) && File.Exists(Path.Combine(folder,"coreclr.dll")),
				"The program is self-contained: its runtime is in its folder (decision 20).");
			//The runtimeconfig's properties, as .NET hands them to the process (text: "true" for true).
			Dictionary<string,string> dictProperties=new Dictionary<string,string>();
			using(System.Text.Json.JsonDocument jsonDocument=System.Text.Json.JsonDocument.Parse(File.ReadAllText(pathRuntimeConfig))) {
				System.Text.Json.JsonElement jsonElementOptions=jsonDocument.RootElement.GetProperty("runtimeOptions");
				Assert.IsTrue(jsonElementOptions.TryGetProperty("includedFrameworks",out _),"A self-contained runtimeconfig names includedFrameworks.");
				if(jsonElementOptions.TryGetProperty("configProperties",out System.Text.Json.JsonElement jsonElementProperties)) {
					foreach(System.Text.Json.JsonProperty jsonProperty in jsonElementProperties.EnumerateObject()) {
						dictProperties[jsonProperty.Name]=jsonProperty.Value.ValueKind==System.Text.Json.JsonValueKind.String ? jsonProperty.Value.GetString()
							: jsonProperty.Value.GetRawText();
					}
				}
			}
			Assert.IsTrue(dictProperties.ContainsKey("System.Globalization.UseNls"),"The program's runtimeconfig sets the NLS mode (decision 4).");
			//The applicationSettings values of the program's .dll.config.
			System.Xml.XmlDocument xmlDocument=new System.Xml.XmlDocument();
			xmlDocument.Load(pathConfig);
			Dictionary<string,string> dictSettings=xmlDocument.SelectNodes("/configuration/applicationSettings/OpenPmsDental.Properties.Settings/setting")
				.Cast<System.Xml.XmlElement>().ToDictionary(x => x.GetAttribute("name"),x => x.SelectSingleNode("value").InnerText);
			Assert.IsTrue(dictSettings.Count>0,"The program's .dll.config holds its Properties.Settings values.");
			List<string> listFilesBefore=Directory.GetFiles(folder).OrderBy(x => x).ToList();
			string patched;
			string pathEntryBefore;
			string pathEntryAfter;
			string[] arrayRuntime;
			int processId;
			using(ProbeProcess probeProcess=InProcHost.StartSessionProcess(folder,TimeSpan.FromMinutes(2))) {
				processId=probeProcess.ProcessId;
				pathEntryBefore=probeProcess.Call(ProgramDriverHost.COMMAND_GET_ENTRY_ASSEMBLY);
				patched=probeProcess.Call(ProgramDriverHost.COMMAND_PATCH,pathExe);
				//S3a-FU2: the child's own Assembly.GetEntryAssembly() is the program's assembly in the folder, not only named in the patch list.
				pathEntryAfter=probeProcess.Call(ProgramDriverHost.COMMAND_GET_ENTRY_ASSEMBLY);
				arrayRuntime=ProbeText.SplitList(probeProcess.Call(ProgramDriverHost.COMMAND_GET_RUNTIME,ProbeText.JoinList(dictProperties.Keys
					.Concat(dictSettings.Keys.Select(x => "Setting:"+x)))));
				Assert.AreEqual(0,Directory.GetFiles(folder,"TestProbeHost*").Length,"Nothing of TestProbeHost is copied into the program's folder.");
			}
			Dictionary<string,string> dictRuntime=arrayRuntime.ToDictionary(x => x.Substring(0,x.IndexOf('=')),x => x.Substring(x.IndexOf('=')+1));
			string runtime=string.Join(" | ",arrayRuntime);
			Assert.AreEqual(pathDll,pathEntryBefore,true,"The entry assembly is the program's OpenPmsDental.dll from the start (its own process).");
			StringAssert.StartsWith(patched,"Application.StartupPath (own), Application.ExecutablePath (own), Assembly.GetEntryAssembly (own); ",patched);
			StringAssert.Contains(patched,"; OpenPmsDental loaded from "+pathDll+"; ",patched);
			Assert.AreEqual(pathDll,pathEntryAfter,true,"After the patch the entry assembly is still the program's in the folder.");
			StringAssert.EndsWith(patched,"; config "+pathConfig,StringComparison.OrdinalIgnoreCase,patched);
			Assert.AreEqual(pathExe,dictRuntime["ProcessPath"],true,"The process is the program's apphost: "+runtime);
			Assert.AreEqual(folder.TrimEnd('\\')+"\\",dictRuntime["BaseDirectory"],true,"Its application base is the program's folder: "+runtime);
			Assert.AreEqual(folder,Path.GetDirectoryName(dictRuntime["CoreLib"]),true,"The runtime is the folder's own: "+runtime);
			Assert.AreEqual(folder,Path.GetDirectoryName(dictRuntime["WindowsForms"]),true,"Windows Forms is the folder's own: "+runtime);
			StringAssert.StartsWith(dictRuntime["Framework"],".NET 10.",runtime);
			Assert.AreEqual("True",dictRuntime["Is64BitProcess"],runtime);
			CollectionAssert.Contains(dictRuntime["DepsFiles"].Split(';').Select(x => x.ToLowerInvariant()).ToList(),pathDeps.ToLowerInvariant(),
				"The host read the program's .deps.json: "+runtime);
			Assert.AreEqual(pathDll,dictRuntime["Program"],true,runtime);
			Assert.AreEqual("",dictRuntime["StartupHooksVariable"],"The startup hook removed its variable: "+runtime);
			Assert.AreEqual(pathConfig,dictRuntime["ConfigFile"],true,"System.Configuration reads the program's .dll.config: "+runtime);
			foreach(KeyValuePair<string,string> pair in dictProperties) {
				Assert.AreEqual(pair.Value,dictRuntime["Property."+pair.Key],true,"The runtimeconfig property "+pair.Key+": "+runtime);
			}
			//Each setting the Settings class declares has the .dll.config's value (a listed setting it does not declare cannot be read).
			int countSettings=0;
			foreach(KeyValuePair<string,string> pair in dictSettings) {
				if(dictRuntime["Setting."+pair.Key]=="(not declared)") {
					continue;
				}
				Assert.AreEqual(pair.Value,dictRuntime["Setting."+pair.Key],"The program's setting "+pair.Key+" from its .dll.config: "+runtime);
				countSettings++;
			}
			Assert.IsTrue(countSettings>0,"At least one of the program's settings is read from its .dll.config: "+runtime);
			bool isGone;
			try {
				using(System.Diagnostics.Process process=System.Diagnostics.Process.GetProcessById(processId)) {
					isGone=process.HasExited;
				}
			}
			catch(ArgumentException) {
				isGone=true;//no process of that id
			}
			Assert.IsTrue(isGone,"Dispose ends the session's process.");
			CollectionAssert.AreEqual(listFilesBefore,Directory.GetFiles(folder).OrderBy(x => x).ToList(),"The program's folder is as it was.");
		}

		///<summary>A check's result crosses from the session's process as text (CheckResult.ToText, FromText) with every field as it was: texts with
		///tabs, line ends, quotes and non-ASCII letters, empty lists and empty strings, and the row keys.</summary>
		[TestMethod]
		public void CheckResult_TextRoundTrip_KeepsEveryField() {
			CheckResult checkResult=new CheckResult("I4","Title with\ttab and \"quotes\"");
			checkResult.Verified("verified \u00e9\r\nsecond line");
			checkResult.Verified("");
			checkResult.ForPerson("for a person");
			checkResult.Fail("failed: (evidence)");
			checkResult.ListShots.Add("I4-shot.png");
			checkResult.Note("a note ~ with the null marker");
			checkResult.NotScripted("not scripted é\twith a tab");
			checkResult.NotScripted("");
			checkResult.DictKeys["I4.RefAttachNum"]=9007199254740993;
			checkResult.DictKeys["I.Simple2D"]=0;
			checkResult.Reason="";
			checkResult.StatusForced=CheckStatus.NeedsScreen;
			CheckResult checkResultBack=CheckResult.FromText(checkResult.ToText());
			Assert.AreEqual(checkResult.Id,checkResultBack.Id);
			Assert.AreEqual(checkResult.Title,checkResultBack.Title);
			CollectionAssert.AreEqual(checkResult.ListVerified,checkResultBack.ListVerified);
			CollectionAssert.AreEqual(checkResult.ListForPerson,checkResultBack.ListForPerson);
			CollectionAssert.AreEqual(checkResult.ListFailures,checkResultBack.ListFailures);
			CollectionAssert.AreEqual(checkResult.ListShots,checkResultBack.ListShots);
			CollectionAssert.AreEqual(checkResult.ListNotes,checkResultBack.ListNotes);
			Assert.AreEqual(2,checkResultBack.ListNotScripted.Count);
			CollectionAssert.AreEqual(checkResult.ListNotScripted,checkResultBack.ListNotScripted);
			CollectionAssert.AreEquivalent(checkResult.DictKeys.ToList(),checkResultBack.DictKeys.ToList());
			Assert.AreEqual("",checkResultBack.Reason);
			Assert.AreEqual(CheckStatus.NeedsScreen,checkResultBack.StatusForced);
			Assert.AreEqual(checkResult.Status,checkResultBack.Status);
			Assert.AreEqual(checkResult.Summary,checkResultBack.Summary);
			Assert.IsFalse(checkResult.ToText().Contains("\n"),"One line, as the child's replies are.");
		}
	}
}
