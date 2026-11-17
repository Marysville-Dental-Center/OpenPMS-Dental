// The built assemblies name none of the removed assemblies, the Word letter merge is late bound, and AvaTax stays disabled without its SDK.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalBusiness;
using ProjectFileTests;

namespace BINBTests {
	[TestClass]
	public class CompiledReferencesTests {
		private static readonly string[] _arrayRemovedAssemblies={ "SigPlusNET","Interop.Word","Interop.Microsoft.Office.Core","Interop.VBIDE","stdole",
			"AxInterop.AcroPDFLib","Interop.AcroPDFLib","Interop.Acrobat","Interop.AFORMAUTLib","zxing","zxing.presentation","MigraDoc.RtfRendering",
			"Tao.FreeGlut","Avalara.AvaTax.RestClient.net45","RS232","Ionic.Zip","installutil" };

		///<summary>One assembly reference: its name, and whether it names Windows Runtime metadata (ContentType=WindowsRuntime).</summary>
		private class AssemblyRef {
			public string Name;
			public bool IsWindowsRuntime;
		}

		private static List<AssemblyRef> ToRefs(AssemblyName[] arrayAssemblyNames) {
			return arrayAssemblyNames.Select(x => new AssemblyRef { Name=x.Name,IsWindowsRuntime=x.ContentType==AssemblyContentType.WindowsRuntime }).ToList();
		}

		///<summary>The references of a built file, read with Mono.Cecil from the metadata of a copy of its bytes: nothing is loaded or locked (S3a-8
		///of the .NET 10 move: .NET has no reflection-only load).</summary>
		private static List<AssemblyRef> ReadRefs(string path) {
			using(MemoryStream memoryStream=new MemoryStream(File.ReadAllBytes(path)))
			using(Mono.Cecil.ModuleDefinition moduleDefinition=Mono.Cecil.ModuleDefinition.ReadModule(memoryStream)) {
				return moduleDefinition.AssemblyReferences.Select(x => new AssemblyRef { Name=x.Name,IsWindowsRuntime=x.IsWindowsRuntime }).ToList();
			}
		}

		private static List<string> GetRemovedReferences(List<AssemblyRef> listRefs) {
			return listRefs.Select(x => x.Name).Where(x => _arrayRemovedAssemblies.Contains(x,StringComparer.OrdinalIgnoreCase)).ToList();
		}

		[TestMethod]
		public void BuiltAssemblies_ReferenceNoneOfTheRemovedAssemblies() {
			Dictionary<string,List<AssemblyRef>> dictionaryReferences=new Dictionary<string,List<AssemblyRef>> {
				{ "OpenPmsDentalBusiness",ToRefs(typeof(LetterMerges).Assembly.GetReferencedAssemblies()) },
				{ "CodeBase",ToRefs(typeof(ODZip).Assembly.GetReferencedAssemblies()) },
				{ "PasswordVaultWrapper",ToRefs(typeof(PasswordVaultWrapper.WindowsPasswordVaultWrapper).Assembly.GetReferencedAssemblies()) },
			};
			//The program itself, when this checkout has built it. Its public references are read without running any of its code. The managed
			//assemblies of the two programs (ProgramAssembly: the .dll on .NET, where the .exe is the apphost Mono.Cecil cannot read).
			foreach(string relative in new[] { ProgramAssembly.FindRelative(TestPaths.RepoRoot,@"OpenPmsDental\bin\Debug","OpenPmsDental"),
				ProgramAssembly.FindRelative(TestPaths.RepoRoot,@"ServiceManager\bin\Debug","ServiceManager"),
				@"SparksToothChart\bin\Debug\SparksToothChart.dll" })
			{
				string path=Path.Combine(TestPaths.RepoRoot,relative);
				if(File.Exists(path)) {
					dictionaryReferences.Add(relative,ReadRefs(path));
				}
			}
			List<string> listViolations=new List<string>();
			foreach(KeyValuePair<string,List<AssemblyRef>> keyValuePair in dictionaryReferences) {
				listViolations.AddRange(GetRemovedReferences(keyValuePair.Value).Select(x => keyValuePair.Key+" references "+x));
				//Since S3a-5 (the Credential Manager in place of PasswordVault) no assembly references Windows Runtime metadata (Windows.winmd).
				listViolations.AddRange(keyValuePair.Value.Where(x => x.IsWindowsRuntime)
					.Select(x => keyValuePair.Key+" references the Windows Runtime metadata "+x.Name));
			}
			Assert.AreEqual(0,listViolations.Count,string.Join("; ",listViolations));
		}

		[TestMethod]
		public void WordLetterMerge_IsLateBound() {
			Assert.AreEqual(typeof(object),typeof(LetterMerges).GetMethod("GetWordApp").ReturnType,"Word's automation object is used as dynamic.");
			//The values of Word's own enumerations, from Word's object model.
			Assert.AreEqual(0,LetterMerges.WORD_MAIL_MERGE_TO_NEW_DOCUMENT);
			Assert.AreEqual(1,LetterMerges.WORD_MAIL_MERGE_TO_PRINTER);
			Assert.AreEqual(1,LetterMerges.WORD_WINDOW_STATE_MAXIMIZE);
			Assert.AreEqual(2,LetterMerges.WORD_WINDOW_STATE_MINIMIZE);
			Regex regexTyped=new Regex(@"(\bWord\.(_?Application|_?Document|MailMerge)\s+\w+\s*[;=]|\bWord\.Wd\w+\.|new\s+Word\.)");
			List<string> listViolations=new List<string>();
			foreach(string folder in new[] { "OpenPmsDental","OpenPmsDentalBusiness","CodeBase","WpfControlsOD" }) {
				foreach(string path in TestPaths.GetFiles(Path.Combine(TestPaths.RepoRoot,folder),".cs")) {
					string[] arrayLines=File.ReadAllLines(path);
					for(int i=0;i<arrayLines.Length;i++) {
						if(arrayLines[i].TrimStart().StartsWith("//")) {
							continue;
						}
						if(regexTyped.IsMatch(arrayLines[i])) {
							listViolations.Add(path.Substring(TestPaths.RepoRoot.Length)+"("+(i+1)+"): "+arrayLines[i].Trim());
						}
					}
				}
			}
			Assert.AreEqual(0,listViolations.Count,"Word interop types are used again: "+string.Join("; ",listViolations));
		}

		[TestMethod]
		public void AvaTax_StaysDisabledWithoutItsSdk() {
			Assert.IsFalse(AvaTax.IsEnabled());
			Assert.IsFalse(AvaTax.IsTaxable(1));
			//A Procedure's constructor reads preferences, which needs a database; the disabled checks never look at their arguments.
			Assert.IsFalse(AvaTax.DoSendProcToAvalara(null));
			Assert.IsFalse(AvaTax.PingAvaTax());
			Assert.AreEqual(0m,AvaTax.GetEstimate(1,1,100));
			Assert.IsFalse(AvaTax.DidUpdateAdjustment(null,null));
			Assert.IsFalse(AvaTax.DoCreateReturnAdjustment(null,null,null));
		}
	}
}
