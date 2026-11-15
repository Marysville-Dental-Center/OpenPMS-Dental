// The harness's own pieces that S3a-8b of the .NET 10 move changed, checked without the opt-in and without starting the program (no window
// is made): AssemblyProbe (Mono.Cecil) gives the same texts as the reflection it replaced, an in-process session's process runs from the folder
// it is given with that folder's OpenPmsDental.exe.config as its config and the program's assembly as its entry assembly, and a check's result
// crosses from that process unchanged. Scratch folders in the temp folder, synthetic assemblies and texts; the only process started is
// TestProbeHost, which loads OpenPmsDental.exe without running any of it.
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

		///<summary>An in-process session's process (InProcHost.StartSessionProcess, a new AppDomain before S3a-8b): its application base is the folder
		///it is given, its config is a copy of that folder's OpenPmsDental.exe.config (the program's binding redirects), the program's assembly
		///loads from the folder and is made the entry assembly, and the process and its copy of the host are gone after Dispose. The program itself
		///is not started (no Start command), so no window is made. A scratch folder with this test's copies of OpenPmsDental.exe and its config.
		///</summary>
		[TestMethod]
		public void SessionProcess_RunsFromTheProgramFolder_WithItsConfig() {
			string folderTest=Path.GetDirectoryName(typeof(HarnessTests).Assembly.Location);
			string pathExeTest=Path.Combine(folderTest,"OpenPmsDental.exe");
			Assert.IsTrue(File.Exists(pathExeTest) && File.Exists(pathExeTest+".config"),"WindowChecksTests references OpenPmsDental: "+pathExeTest);
			string folder=NewTempFolder();
			try {
				string pathExe=Path.Combine(folder,"OpenPmsDental.exe");
				File.Copy(pathExeTest,pathExe);
				File.Copy(pathExeTest+".config",pathExe+".config");
				string configText=File.ReadAllText(pathExe+".config");
				string patched;
				string pathConfigChild;
				using(ProbeProcess probeProcess=InProcHost.StartSessionProcess(folder,TimeSpan.FromMinutes(2))) {
					patched=probeProcess.Call(ProgramDriverHost.COMMAND_PATCH,pathExe);
					List<string> listCopies=Directory.GetFiles(folder,"TestProbeHost-*.exe").ToList();
					Assert.AreEqual(1,listCopies.Count,string.Join(", ",listCopies));
					pathConfigChild=listCopies[0]+".config";
					Assert.AreEqual(configText,File.ReadAllText(pathConfigChild),"The child's config is the program's.");
				}
				StringAssert.StartsWith(patched,"Application.StartupPath, Application.ExecutablePath, Assembly.GetEntryAssembly; ",patched);
				StringAssert.Contains(patched,"; OpenPmsDental loaded from "+pathExe+"; ",patched);
				//The .NET Framework names an exe's config with a capital C (".exe.Config"); Windows file names ignore case.
				StringAssert.EndsWith(patched,"; config "+pathConfigChild,StringComparison.OrdinalIgnoreCase,patched);
				Assert.AreEqual(0,Directory.GetFiles(folder,"TestProbeHost-*").Length,"Dispose deletes the copy of the host and its config.");
			}
			finally {
				DeleteQuietly(folder);
			}
		}

		///<summary>A check's result crosses from the session's process as text (CheckResult.ToText, FromText) with every field as it was: texts with
		///tabs, line ends, quotes and non-ASCII letters, empty lists and empty strings, and the row keys.</summary>
		[TestMethod]
		public void CheckResult_TextRoundTrip_KeepsEveryField() {
			CheckResult checkResult=new CheckResult("I4","Title with\ttab and \"quotes\"");
			checkResult.Verified("verified é\r\nsecond line");
			checkResult.Verified("");
			checkResult.ForPerson("for a person");
			checkResult.Fail("failed: (evidence)");
			checkResult.ListShots.Add("I4-shot.png");
			checkResult.Note("a note ~ with the null marker");
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
			Assert.AreEqual(0,checkResultBack.ListNotScripted.Count);
			CollectionAssert.AreEquivalent(checkResult.DictKeys.ToList(),checkResultBack.DictKeys.ToList());
			Assert.AreEqual("",checkResultBack.Reason);
			Assert.AreEqual(CheckStatus.NeedsScreen,checkResultBack.StatusForced);
			Assert.AreEqual(checkResult.Status,checkResultBack.Status);
			Assert.AreEqual(checkResult.Summary,checkResultBack.Summary);
			Assert.IsFalse(checkResult.ToText().Contains("\n"),"One line, as the child's replies are.");
		}
	}
}
