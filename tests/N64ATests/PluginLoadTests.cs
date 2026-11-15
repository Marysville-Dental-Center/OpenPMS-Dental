// The plug-in loaders' check (N64-A, item A2 of docs/specs/native64-survey.md): Plugins.LoadPlugin, the one load path of PluginLoader (the
// program) and Plugins.LoadAllPlugins (the Middle Tier), refuses a plug-in this process cannot load with a clear logged reason naming it, and
// loads a loadable one as before. With synthetic plug-in DLLs only (TestSupport.WritePlugin); in the x86 run and the lane's x64 test build, the
// refusal is checked against what Assembly.LoadFile really does in this process.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalBusiness;

namespace N64ATests {
	[TestClass]
	public class PluginLoadTests {
		private static string _folder;
		private static readonly Version _version48=new Version(4,8);
		private static readonly Version _version10=new Version(10,0);

		[ClassInitialize]
		public static void ClassInitialize(TestContext testContext) {
			_folder=TestSupport.CreateTempFolder("n64a_plugin");
		}

		[ClassCleanup]
		public static void ClassCleanup() {
			Plugins.ListPlugins=null;
			TestSupport.DeleteFolder(_folder);
		}

		///<summary>Each bitness of plug-in in this process: GetRefusalReason refuses exactly the ones Assembly.LoadFile cannot load here
		///(BadImageFormatException), and LoadPlugin loads the others as before (the [name].Plugin type, created, Host set, the container's fields).
		///In the x86 run the x64 plug-in is refused; in the x64 run the x86 ones.</summary>
		[TestMethod]
		public void EachBitness_RefusedExactlyWhenThisProcessCannotLoadIt() {
			List<string> listResults=new List<string>();
			foreach(SyntheticPlatform syntheticPlatform in Enum.GetValues(typeof(SyntheticPlatform))) {
				string name="N64aLoad"+syntheticPlatform;
				string path=TestSupport.WritePlugin(_folder,name,syntheticPlatform,".NETFramework,Version=v4.8");
				string reason=PluginImageCheck.GetRefusalReason(path,name+".dll");
				bool isLoadFailing=false;
				try {
					Assembly.LoadFile(path);
				}
				catch(BadImageFormatException) {
					isLoadFailing=true;
				}
				listResults.Add(syntheticPlatform+": refused "+(reason!=null)+", LoadFile fails "+isLoadFailing);
				//The record of this run, next to the test assembly (the lane's runners print only the first line of a failure).
				File.WriteAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"N64aPluginBitness."+(Environment.Is64BitProcess ? "x64" : "x86")+".txt"),
					listResults);
				if(isLoadFailing!=(reason!=null)) {
					Assert.Fail(syntheticPlatform+" in a "+(Environment.Is64BitProcess ? "64" : "32")+"-bit process: LoadFile fails "+isLoadFailing+", reason "
						+(reason??"(none)")+"\r\n"+string.Join("\r\n",listResults));
				}
				PluginContainer pluginContainer=Plugins.LoadPlugin(path,name+".dll",77,null,out string error);
				if(reason!=null) {
					Assert.IsNull(pluginContainer);
					Assert.AreEqual(reason,error);
					StringAssert.Contains(reason,"The plug-in "+name+".dll ("+path+") is not loaded");
					StringAssert.Contains(reason,Environment.Is64BitProcess ? "(32-bit only), and this program is a 64-bit process"
						: "(64-bit only), and this program is a 32-bit process");
					StringAssert.Contains(reason,"Ask its author for a build for any CPU (AnyCPU) or "+(Environment.Is64BitProcess ? "x64" : "x86"));
					CollectionAssert.Contains(Plugins.GetProblemsLogged(),reason,"The reason is logged.");
					continue;
				}
				Assert.IsNotNull(pluginContainer,syntheticPlatform+": "+error);
				Assert.IsNull(error);
				Assert.AreEqual(name,pluginContainer.Name);
				Assert.AreEqual(77,pluginContainer.ProgramNum);
				Assert.AreEqual(name+".Plugin",pluginContainer.Plugin.GetType().FullName);
				Assert.IsTrue(string.Equals(Path.GetFullPath(path),Path.GetFullPath(pluginContainer.Assemb.Location),StringComparison.OrdinalIgnoreCase),
					pluginContainer.Assemb.Location);
				Assert.IsNull(pluginContainer.Plugin.Host);
			}
			Console.WriteLine(string.Join("\r\n",listResults));
			Assert.IsTrue(listResults.Any(x => x.Contains("refused True")),"One bitness is refused in either process.");
			Assert.IsTrue(listResults.Any(x => x.Contains("refused False")),"Any CPU loads in either process.");
		}

		///<summary>The bitness rule for both processes, whatever this run's bitness: any CPU (also 32-bit preferred) loads in both, x86 only in a
		///32-bit process, x64 only in a 64-bit one.</summary>
		[TestMethod]
		public void BitnessRule_BothProcesses() {
			foreach(SyntheticPlatform syntheticPlatform in Enum.GetValues(typeof(SyntheticPlatform))) {
				string name="N64aRule"+syntheticPlatform;
				string path=TestSupport.WritePlugin(_folder,name,syntheticPlatform,".NETFramework,Version=v4.8");
				string reason32=PluginImageCheck.GetRefusalReason(path,name,PortableExecutableFile.IMAGE_FILE_MACHINE_I386,
					PluginImageCheck.FRAMEWORK_NET_FRAMEWORK,_version48);
				string reason64=PluginImageCheck.GetRefusalReason(path,name,PortableExecutableFile.IMAGE_FILE_MACHINE_AMD64,
					PluginImageCheck.FRAMEWORK_NET_FRAMEWORK,_version48);
				Assert.AreEqual(syntheticPlatform==SyntheticPlatform.X64,reason32!=null,syntheticPlatform+" in 32-bit: "+reason32);
				Assert.AreEqual(syntheticPlatform==SyntheticPlatform.X86,reason64!=null,syntheticPlatform+" in 64-bit: "+reason64);
			}
		}

		///<summary>The framework rule (decision 16 of docs/specs/net10-plan.md on .NET): on the .NET Framework, a .NET Framework or .NET Standard
		///2.0 plug-in loads and a .NET or .NET Standard 2.1 one is refused; on .NET 10, a .NET Framework plug-in is refused with the rebuild
		///message, .NET Standard and .NET up to 10 load, a newer .NET is refused. Without a TargetFrameworkAttribute, the core library tells.</summary>
		[TestMethod]
		public void FrameworkRule_NetFrameworkAndNet10Processes() {
			ushort machine=PortableExecutableFile.IMAGE_FILE_MACHINE_AMD64;
			string pathFramework=TestSupport.WritePlugin(_folder,"N64aTfmFramework",SyntheticPlatform.AnyCpu,".NETFramework,Version=v4.8");
			string pathCore=TestSupport.WritePlugin(_folder,"N64aTfmCore",SyntheticPlatform.AnyCpu,".NETCoreApp,Version=v10.0");
			string pathCore11=TestSupport.WritePlugin(_folder,"N64aTfmCore11",SyntheticPlatform.AnyCpu,".NETCoreApp,Version=v11.0");
			string pathStandard20=TestSupport.WritePlugin(_folder,"N64aTfmStd20",SyntheticPlatform.AnyCpu,".NETStandard,Version=v2.0");
			string pathStandard21=TestSupport.WritePlugin(_folder,"N64aTfmStd21",SyntheticPlatform.AnyCpu,".NETStandard,Version=v2.1");
			string pathNoTfmMscorlib=TestSupport.WritePlugin(_folder,"N64aTfmNone",SyntheticPlatform.AnyCpu,null);
			string pathNoTfmRuntime10=TestSupport.WritePlugin(_folder,"N64aTfmRuntime10",SyntheticPlatform.AnyCpu,null,new Version(10,0,0,0));
			string framework=PluginImageCheck.FRAMEWORK_NET_FRAMEWORK;
			string core=PluginImageCheck.FRAMEWORK_NET_CORE_APP;
			//.NET Framework 4.8 process (stage 3a).
			Assert.IsNull(PluginImageCheck.GetRefusalReason(pathFramework,"p",machine,framework,_version48));
			Assert.IsNull(PluginImageCheck.GetRefusalReason(pathStandard20,"p",machine,framework,_version48));
			Assert.IsNull(PluginImageCheck.GetRefusalReason(pathNoTfmMscorlib,"p",machine,framework,_version48));
			string reason=PluginImageCheck.GetRefusalReason(pathCore,"N64aTfmCore.dll",machine,framework,_version48);
			Assert.AreEqual("The plug-in N64aTfmCore.dll ("+pathCore+") is not loaded: it is built for .NET 10.0, and this program runs on .NET Framework 4.8. "
				+"Use a build of the plug-in for the .NET Framework (net48).",reason);
			StringAssert.Contains(PluginImageCheck.GetRefusalReason(pathStandard21,"p",machine,framework,_version48),
				"built for .NET Standard 2.1, which the .NET Framework cannot load");
			StringAssert.Contains(PluginImageCheck.GetRefusalReason(pathNoTfmRuntime10,"p",machine,framework,_version48),"built for .NET 10.0");
			//.NET 10 process (stage 3b).
			reason=PluginImageCheck.GetRefusalReason(pathFramework,"N64aTfmFramework.dll",machine,core,_version10);
			Assert.AreEqual("The plug-in N64aTfmFramework.dll ("+pathFramework+") is not loaded: it is built for .NET Framework 4.8, and this version of the "
				+"program runs on .NET 10.0, which does not load .NET Framework plug-ins. Ask its author for a build for .NET 10.0 (net10.0-windows, any "
				+"CPU or x64).",reason);
			StringAssert.Contains(PluginImageCheck.GetRefusalReason(pathNoTfmMscorlib,"p",machine,core,_version10),"built for .NET Framework 4.0");
			Assert.IsNull(PluginImageCheck.GetRefusalReason(pathCore,"p",machine,core,_version10));
			Assert.IsNull(PluginImageCheck.GetRefusalReason(pathStandard20,"p",machine,core,_version10));
			Assert.IsNull(PluginImageCheck.GetRefusalReason(pathStandard21,"p",machine,core,_version10));
			Assert.IsNull(PluginImageCheck.GetRefusalReason(pathNoTfmRuntime10,"p",machine,core,_version10));
			StringAssert.Contains(PluginImageCheck.GetRefusalReason(pathCore11,"p",machine,core,_version10),"built for .NET 11.0, newer than the .NET 10.0");
			Assert.IsNotNull(PluginImageCheck.GetRefusalReason(pathCore,"p",machine,core,new Version(8,0)),"A .NET 10 plug-in on .NET 8.");
		}

		[TestMethod]
		public void ProcessFramework_IsTheNetFramework() {
			PluginImageCheck.GetProcessFramework(out string framework,out Version version);
			Assert.AreEqual(PluginImageCheck.FRAMEWORK_NET_FRAMEWORK,framework,"The tests run on .NET Framework 4.8 in stage 3a.");
			Assert.AreEqual(_version48,version);
		}

		///<summary>A file that is not a .NET assembly is refused before LoadFile, naming it; a missing file is left to LoadFile (its own error, as
		///before). The native file is only read, never loaded.</summary>
		[TestMethod]
		public void NotAnAssembly_Refused_MissingFile_LeftToLoadFile() {
			string pathText=Path.Combine(_folder,"N64aText.dll");
			File.WriteAllBytes(pathText,Encoding.ASCII.GetBytes("N64A not an assembly"));
			string reason=PluginImageCheck.GetRefusalReason(pathText,"N64aText.dll");
			Assert.AreEqual("The plug-in N64aText.dll ("+pathText+") is not loaded: it is not a program library (the file has no PE header). Check the "
				+"plug-in's file name in the program link's setup.",reason);
			Assert.IsNull(Plugins.LoadPlugin(pathText,"N64aText.dll",1,null,out string error));
			Assert.AreEqual(reason,error);
			string pathNative=Path.Combine(Environment.SystemDirectory,"kernel32.dll");
			reason=PluginImageCheck.GetRefusalReason(pathNative,"kernel32.dll");
			StringAssert.Contains(reason,"is not loaded: it is a native "+(Environment.Is64BitProcess ? "64-bit (x64)" : "32-bit (x86)"));
			StringAssert.Contains(reason,"not a .NET plug-in assembly");
			Assert.IsNull(PluginImageCheck.GetRefusalReason(Path.Combine(_folder,"N64aMissing.dll"),"N64aMissing.dll"));
		}

		///<summary>A plug-in that loads but has no [name].Plugin type fails as before: the exception's message is the error the program shows
		///("Error loading Plugin:" and it), and the log names the plug-in and the exception.</summary>
		[TestMethod]
		public void LoadableFileWithoutPluginType_FailsAsBefore_AndIsLogged() {
			string pathWritten=TestSupport.WritePlugin(_folder,"N64aWrongName",SyntheticPlatform.AnyCpu,".NETFramework,Version=v4.8");
			string path=Path.Combine(_folder,"N64aOtherName.dll");
			File.Copy(pathWritten,path);
			Assert.IsNull(PluginImageCheck.GetRefusalReason(path,"N64aOtherName.dll"));
			Assert.IsNull(Plugins.LoadPlugin(path,"N64aOtherName.dll",1,null,out string error));
			string expected;
			try {
				Activator.CreateInstance((Type)null);
				expected="";
			}
			catch(ArgumentNullException ex) {
				expected=ex.Message;
			}
			Assert.AreEqual(expected,error,"The same message the loader showed before.");
			Assert.IsTrue(Plugins.GetProblemsLogged().Any(x => x.StartsWith("The plug-in N64aOtherName.dll ("+path+") was not loaded (ArgumentNullException: ")),
				string.Join("\r\n",Plugins.GetProblemsLogged()));
		}

		///<summary>The clear texts for the load errors that PluginLoader showed raw before: BadImageFormatException and FileLoadException (also
		///wrapped in a TargetInvocationException by the plug-in's constructor), naming the plug-in; any other exception keeps its message.</summary>
		[TestMethod]
		public void LoadErrorText_BadImageAndFileLoad_AreExplained() {
			string text=PluginImageCheck.GetLoadErrorText(new BadImageFormatException("bad image"),"N64aX.dll",@"C:\Plugins\N64aX.dll");
			StringAssert.StartsWith(text,@"The plug-in N64aX.dll (C:\Plugins\N64aX.dll) could not be loaded: it, or a library it needs, is not a .NET assembly that "
				+"this "+(Environment.Is64BitProcess ? "64" : "32")+"-bit program can load.");
			StringAssert.EndsWith(text,"(BadImageFormatException: bad image)");
			text=PluginImageCheck.GetLoadErrorText(new TargetInvocationException(new BadImageFormatException("inner")),"N64aX.dll",@"C:\Plugins\N64aX.dll");
			StringAssert.EndsWith(text,"(BadImageFormatException: inner)");
			text=PluginImageCheck.GetLoadErrorText(new FileLoadException("blocked"),"N64aX.dll",@"C:\Plugins\N64aX.dll");
			StringAssert.StartsWith(text,@"The plug-in N64aX.dll (C:\Plugins\N64aX.dll) could not be loaded: the file, or a library it needs, was found but could "
				+"not be loaded.");
			StringAssert.Contains(text,"blocked by Windows (the file's Properties, Unblock)");
			StringAssert.EndsWith(text,"(FileLoadException: blocked)");
			Assert.AreEqual("other",PluginImageCheck.GetLoadErrorText(new InvalidOperationException("other"),"N64aX.dll",@"C:\Plugins\N64aX.dll"));
		}

		///<summary>A hook-time DllNotFoundException or BadImageFormatException (also wrapped) is logged once per plug-in and error, naming the
		///plug-in and the hook; the plug-in's HookException still gets every exception, and other exceptions are not logged.</summary>
		[TestMethod]
		public void HookTime_LibraryErrors_LoggedOnceWithThePluginName() {
			ThrowingPlugin throwingPlugin=new ThrowingPlugin();
			Plugins.ListPlugins=new List<PluginContainer> { new PluginContainer { Plugin=throwingPlugin,ProgramNum=5,Name="N64aHookPlugin" } };
			try {
				int countBefore=Plugins.GetProblemsLogged().Count;
				throwingPlugin.ExceptionNext=new DllNotFoundException("Unable to load DLL 'n64avendor.dll'");
				Plugins.HookAddCode(this,"N64aHook.AddCode");
				Plugins.HookAddCode(this,"N64aHook.AddCode");
				throwingPlugin.ExceptionNext=new TargetInvocationException(new BadImageFormatException("An attempt was made to load a program with an "
					+"incorrect format."));
				Assert.IsFalse(Plugins.HookMethod(this,"N64aHook.Method"));
				throwingPlugin.ExceptionNext=new InvalidOperationException("not a library error");
				Plugins.LaunchToolbarButton(5,0);
				List<string> listNew=Plugins.GetProblemsLogged().Skip(countBefore).ToList();
				Assert.AreEqual(2,listNew.Count,string.Join("\r\n",listNew));
				Assert.AreEqual("The plug-in N64aHookPlugin failed in N64aHook.AddCode: a library it calls was not found (DllNotFoundException: Unable to load "
					+"DLL 'n64avendor.dll').",listNew[0]);
				Assert.AreEqual("The plug-in N64aHookPlugin failed in N64aHook.Method: a library it calls is not one this "
					+(Environment.Is64BitProcess ? "64" : "32")+"-bit program can load (another bitness?) (BadImageFormatException: An attempt was made to "
					+"load a program with an incorrect format.).",listNew[1]);
				Assert.AreEqual(4,throwingPlugin.ListExceptions.Count,"HookException gets every exception, as before.");
			}
			finally {
				Plugins.ListPlugins=null;
			}
		}

		///<summary>CLEAN-1: the hook-time problems kept for logging once are the last 100, like the problem list: a plug-in that fails with ever new
		///messages no longer grows the set without end (it held every one before). A problem among the last 100 is still logged only once; one
		///that dropped out is logged again when it comes back.</summary>
		[TestMethod]
		public void HookTime_ProblemsKeptForLoggingOnce_AreTheLast100() {
			FieldInfo fieldInfo=typeof(Plugins).GetField("_hashSetHookProblemsLogged",BindingFlags.NonPublic|BindingFlags.Static);
			Assert.IsNotNull(fieldInfo);
			HashSet<string> hashSet=(HashSet<string>)fieldInfo.GetValue(null);
			ThrowingPlugin throwingPlugin=new ThrowingPlugin();
			Plugins.ListPlugins=new List<PluginContainer> { new PluginContainer { Plugin=throwingPlugin,ProgramNum=6,Name="N64aCapPlugin" } };
			try {
				for(int i=0;i<150;i++) {
					throwingPlugin.ExceptionNext=new DllNotFoundException("Unable to load DLL 'clean1vendor"+i+".dll'");
					Plugins.HookAddCode(this,"Clean1Hook.AddCode");
				}
				Assert.IsTrue(hashSet.Count<=100,"At most 100 hook problems are kept: "+hashSet.Count);
				//The newest is still logged once only.
				List<string> listBefore=Plugins.GetProblemsLogged();
				throwingPlugin.ExceptionNext=new DllNotFoundException("Unable to load DLL 'clean1vendor149.dll'");
				Plugins.HookAddCode(this,"Clean1Hook.AddCode");
				CollectionAssert.AreEqual(listBefore,Plugins.GetProblemsLogged(),"A problem among the last 100 is not logged again.");
				Assert.IsTrue(Plugins.GetProblemsLogged().Last().Contains("clean1vendor149.dll"));
				//The first one dropped out, so it is logged again.
				throwingPlugin.ExceptionNext=new DllNotFoundException("Unable to load DLL 'clean1vendor0.dll'");
				Plugins.HookAddCode(this,"Clean1Hook.AddCode");
				Assert.AreEqual("The plug-in N64aCapPlugin failed in Clean1Hook.AddCode: a library it calls was not found (DllNotFoundException: Unable to "
					+"load DLL 'clean1vendor0.dll').",Plugins.GetProblemsLogged().Last());
				Assert.IsTrue(hashSet.Count<=100,"Still at most 100: "+hashSet.Count);
				Assert.AreEqual(152,throwingPlugin.ListExceptions.Count,"HookException gets every exception, as before.");
			}
			finally {
				Plugins.ListPlugins=null;
			}
		}

		///<summary>Both loaders load through Plugins.LoadPlugin, so the check runs before every LoadFile; the Middle Tier's empty catch is gone;
		///the program's message box shows the reason after "Error loading Plugin:" as before.</summary>
		[TestMethod]
		public void Source_BothLoadersGoThroughLoadPlugin() {
			string plugins=TestSupport.ReadRepoFile(@"OpenPmsDentalBusiness\Plugins\Plugins.cs");
			string pluginLoader=TestSupport.ReadRepoFile(@"WpfControlsOD\Bridges\PluginLoader.cs");
			StringAssert.Contains(plugins,"PluginContainer pluginContainer=LoadPlugin(dllPath,listPrograms[i].PluginDllName,listPrograms[i].ProgramNum,host,");
			StringAssert.Contains(pluginLoader,"PluginContainer pluginContainer=Plugins.LoadPlugin(dllPath,listPrograms[i].PluginDllName,"
				+"listPrograms[i].ProgramNum,host,out string error);");
			StringAssert.Contains(pluginLoader,"MessageBox.Show(\"Error loading Plugin:\"+listPrograms[i].PluginDllName+\"\\r\\n\"+error);");
			Assert.IsFalse(pluginLoader.Contains("Assembly.LoadFile("),"PluginLoader loads only through Plugins.LoadPlugin.");
			Assert.AreEqual(1,Regex.Matches(plugins,@"Assembly\.LoadFile\(").Count,"One LoadFile, in LoadPlugin.");
			int indexLoadFile=plugins.IndexOf("ass=Assembly.LoadFile(dllPath);");
			int indexCheck=plugins.IndexOf("error=PluginImageCheck.GetRefusalReason(dllPath,pluginDllName);");
			Assert.IsTrue(indexCheck>0 && indexCheck<indexLoadFile,"The check comes before LoadFile.");
			Assert.IsFalse(Regex.IsMatch(plugins,@"catch\s*\{"),"No empty catch in Plugins.cs.");
		}

		///<summary>A plug-in whose hooks throw the exception set in ExceptionNext and record what HookException got.</summary>
		private class ThrowingPlugin:PluginBase {
			public Exception ExceptionNext;
			public List<Exception> ListExceptions=new List<Exception>();

			public override bool HookMethod(object sender,string methodName,params object[] parameters) {
				throw ExceptionNext;
			}

			public override bool HookAddCode(object sender,string hookName,params object[] parameters) {
				throw ExceptionNext;
			}

			public override void LaunchToolbarButton(long patNum) {
				throw ExceptionNext;
			}

			public override void HookException(Exception e) {
				ListExceptions.Add(e);
			}
		}
	}
}
