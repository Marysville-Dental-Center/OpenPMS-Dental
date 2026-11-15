// Helpers of the N64-A tests: the repository root, a temporary folder per test class, and the synthetic plug-in DLLs written with Mono.Cecil
// (any CPU, x86 only, x64, with a chosen TargetFrameworkAttribute or core library), so no test loads a real third-party plug-in.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Versioning;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Cecil;
using Mono.Cecil.Cil;
using OpenPmsDentalBusiness;

namespace N64ATests {
	///<summary>The kinds of synthetic plug-in DLL.</summary>
	public enum SyntheticPlatform {
		///<summary>IL only, PE32, x86 machine, no 32BITREQUIRED: platform AnyCPU.</summary>
		AnyCpu,
		///<summary>IL only, PE32, 32BITREQUIRED: platform x86.</summary>
		X86,
		///<summary>IL only, PE32, 32BITREQUIRED and 32BITPREFERRED (anycpu32bitpreferred, which compilers allow for an exe only).</summary>
		X86Preferred,
		///<summary>IL only, PE32+, AMD64 machine: platform x64.</summary>
		X64,
	}

	internal static class TestSupport {
		///<summary>The repository root: the first folder above the test assembly with LICENSE and NOTICE.</summary>
		public static string Root() {
			DirectoryInfo directoryInfo=new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
			while(directoryInfo!=null) {
				if(File.Exists(Path.Combine(directoryInfo.FullName,"LICENSE")) && File.Exists(Path.Combine(directoryInfo.FullName,"NOTICE"))) {
					return directoryInfo.FullName;
				}
				directoryInfo=directoryInfo.Parent;
			}
			Assert.Fail("Repository root not found above "+AppDomain.CurrentDomain.BaseDirectory);
			return null;
		}

		public static string ReadRepoFile(string relativePath) {
			return File.ReadAllText(Path.Combine(Root(),relativePath));
		}

		///<summary>The files under folder that match one of the patterns, without build output (bin, obj), git data, worktrees (.claude) and
		///packages folders.</summary>
		public static List<string> EnumerateRepoFiles(string folder,params string[] arrayPatterns) {
			List<string> listFiles=new List<string>();
			foreach(string pattern in arrayPatterns) {
				listFiles.AddRange(Directory.GetFiles(folder,pattern));
			}
			foreach(string folderSub in Directory.GetDirectories(folder)) {
				string name=Path.GetFileName(folderSub);
				if(name.Equals("bin",StringComparison.OrdinalIgnoreCase) || name.Equals("obj",StringComparison.OrdinalIgnoreCase) || name==".git"
					|| name==".claude" || name==".vs" || name.Equals("packages",StringComparison.OrdinalIgnoreCase) || name=="node_modules")
				{
					continue;
				}
				listFiles.AddRange(EnumerateRepoFiles(folderSub,arrayPatterns));
			}
			return listFiles;
		}

		///<summary>A new empty folder under the user's temporary folder, for one test class; DeleteFolder removes it.</summary>
		public static string CreateTempFolder(string prefix) {
			string folder=Path.Combine(Path.GetTempPath(),prefix+"_"+Guid.NewGuid().ToString("N").Substring(0,12));
			Directory.CreateDirectory(folder);
			return folder;
		}

		///<summary>Deletes the folder; a DLL this process has loaded cannot be deleted and stays (in the temporary folder).</summary>
		public static void DeleteFolder(string folder) {
			if(string.IsNullOrEmpty(folder) || !Directory.Exists(folder)) {
				return;
			}
			foreach(string path in Directory.GetFiles(folder)) {
				try {
					File.Delete(path);
				}
				catch(Exception ex) {
					ex.ToString();//loaded by this process
				}
			}
			try {
				Directory.Delete(folder,false);
			}
			catch(Exception ex) {
				ex.ToString();
			}
		}

		///<summary>Writes folder\name.dll: an assembly named name with one public type [name].Plugin derived from PluginBase (a public constructor
		///that calls the base one), as a plug-in has. targetFramework is its TargetFrameworkAttribute's value, or null for none. A non-null
		///versionSystemRuntime adds a reference to System.Runtime of that version (the core library of .NET). Returns the path.</summary>
		public static string WritePlugin(string folder,string name,SyntheticPlatform syntheticPlatform,string targetFramework,
			Version versionSystemRuntime=null)
		{
			TargetArchitecture targetArchitecture=(syntheticPlatform==SyntheticPlatform.X64) ? TargetArchitecture.AMD64 : TargetArchitecture.I386;
			ModuleAttributes moduleAttributes=ModuleAttributes.ILOnly;
			if(syntheticPlatform==SyntheticPlatform.X86 || syntheticPlatform==SyntheticPlatform.X86Preferred) {
				moduleAttributes|=ModuleAttributes.Required32Bit;
			}
			if(syntheticPlatform==SyntheticPlatform.X86Preferred) {
				moduleAttributes|=ModuleAttributes.Preferred32Bit;
			}
			ModuleParameters moduleParameters=new ModuleParameters {
				Kind=ModuleKind.Dll,
				Architecture=targetArchitecture,
				Runtime=TargetRuntime.Net_4_0,
			};
			string path=Path.Combine(folder,name+".dll");
			using(AssemblyDefinition assemblyDefinition=AssemblyDefinition.CreateAssembly(new AssemblyNameDefinition(name,new Version(1,0,0,0)),
				name+".dll",moduleParameters))
			{
				ModuleDefinition moduleDefinition=assemblyDefinition.MainModule;
				moduleDefinition.Attributes=moduleAttributes;
				TypeDefinition typeDefinition=new TypeDefinition(name,"Plugin",Mono.Cecil.TypeAttributes.Public|Mono.Cecil.TypeAttributes.Class
					|Mono.Cecil.TypeAttributes.BeforeFieldInit,moduleDefinition.ImportReference(typeof(PluginBase)));
				MethodDefinition methodDefinitionCtor=new MethodDefinition(".ctor",Mono.Cecil.MethodAttributes.Public|Mono.Cecil.MethodAttributes.HideBySig
					|Mono.Cecil.MethodAttributes.SpecialName|Mono.Cecil.MethodAttributes.RTSpecialName,moduleDefinition.TypeSystem.Void);
				ConstructorInfo constructorInfoBase=typeof(PluginBase).GetConstructor(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic,
					null,Type.EmptyTypes,null);
				ILProcessor ilProcessor=methodDefinitionCtor.Body.GetILProcessor();
				ilProcessor.Emit(OpCodes.Ldarg_0);
				ilProcessor.Emit(OpCodes.Call,moduleDefinition.ImportReference(constructorInfoBase));
				ilProcessor.Emit(OpCodes.Ret);
				typeDefinition.Methods.Add(methodDefinitionCtor);
				moduleDefinition.Types.Add(typeDefinition);
				if(targetFramework!=null) {
					MethodReference methodReferenceAttribute=moduleDefinition.ImportReference(typeof(TargetFrameworkAttribute).GetConstructor(new[] { typeof(string) }));
					CustomAttribute customAttribute=new CustomAttribute(methodReferenceAttribute);
					customAttribute.ConstructorArguments.Add(new CustomAttributeArgument(moduleDefinition.TypeSystem.String,targetFramework));
					assemblyDefinition.CustomAttributes.Add(customAttribute);
				}
				if(versionSystemRuntime!=null) {
					moduleDefinition.AssemblyReferences.Add(new AssemblyNameReference("System.Runtime",versionSystemRuntime) {
						PublicKeyToken=new byte[] { 0xb0,0x3f,0x5f,0x7f,0x11,0xd5,0x0a,0x3a }
					});
				}
				assemblyDefinition.Write(path);
			}
			return path;
		}
	}
}
