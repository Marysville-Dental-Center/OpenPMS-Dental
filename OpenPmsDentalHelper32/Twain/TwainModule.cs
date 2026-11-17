// The TWAIN module of the 32-bit helper program (S3b-8 of the .NET 10 move, docs/port-notes/S3b-8.md; decision 23 of docs/specs/net10-plan.md:
// "TWAIN devices with only a 32-bit driver need a 32-bit TWAIN path through the helper").
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.IO;
using System.Reflection;
using ImagingCapture.Metadata;
using ImagingDeviceManager.Helper32;
using ImagingDeviceManager.TwainProtocol;

namespace OpenPmsDentalHelper32.Twain {
	///<summary>Runs a TWAIN source that has only a 32-bit driver for the 64-bit program: the commands of ImagingDeviceManager's TwainHelperCommands,
	///answered by its TwainHelperServer on this 32-bit process's DSM (NativeTwainDsm's 32-bit order: SysWOW64\TWAINDSM.dll, which the setup
	///installs, then the legacy twain_32.dll). ImagingDeviceManager's TWAIN code, which S3a-12 made pointer-size safe, is compiled into the helper
	///(OpenPmsDentalHelper32.csproj), with ImagingCapture's capture metadata parser, so a session here is the program's own TwainCaptureSession:
	///its STA worker thread with its message loop and the proxy window the DSM and the source need, the source's own UI, the negotiation, the
	///safe stops, the journal and the quarantine under %ProgramData%\OpenPMS-Dental\Imaging (the same user, the same folders as the program).
	///The server is made on the first command, never in the constructor (IHelper32Module): no DSM is loaded before the program asks.
	///With --test-twain-dsm (tests only) the DSM is a test assembly's fake from the helper's own folder, as --test-module loads a test module.
	///</summary>
	internal sealed class TwainModule:IHelper32Module {
		private readonly HelperArguments _helperArguments;
		private TwainHelperServer _server;

		public TwainModule(HelperArguments helperArguments) {
			_helperArguments=helperArguments;
		}

		public string Name {
			get {
				return TwainHelperCommands.MODULE;
			}
		}

		public Helper32Reply Handle(Helper32Request request) {
			if(_server==null) {
				bool isTest=_helperArguments!=null && _helperArguments.TestTwainDsmAssembly!=null;
				DsmLoader dsmLoader=isTest
					? TestTwainDsm.CreateLoader(_helperArguments.TestTwainDsmAssembly,_helperArguments.TestTwainDsmType,_helperArguments.TestTwainDsmScenario)
					: LoadNativeDsm;
				_server=new TwainHelperServer(dsmLoader,CaptureMetadataParser.ReadKeys,isTestFolderAllowed:isTest);
				AppDomain.CurrentDomain.ProcessExit+=(sender,e) => _server.Dispose();
			}
			byte[] dataReply;
			string[] arrayResults=_server.Handle(request.Command,request.Args,request.Data,out dataReply);
			return Helper32Reply.Ok(arrayResults,dataReply);
		}

		private static ITwainDsm LoadNativeDsm(out string errorText) {
			return NativeTwainDsm.Load(out errorText);
		}
	}

	///<summary>Tests only (--test-twain-dsm): a fake DSM of a test assembly in the helper's own folder, by name (ending with "Tests", as
	///TestModule), whose type has public static Func&lt;IntPtr,IntPtr,uint,ushort,ushort,IntPtr,ushort&gt; CreateDsmEntry(string scenario): one
	///new fake DSM per load, called like DSM_Entry. The test assembly has its own copy of the TWAIN structures, so the two meet only at that
	///delegate, with the same raw pointers a native DSM_Entry gets.</summary>
	internal sealed class TestTwainDsm:ITwainDsm {
		private readonly Func<IntPtr,IntPtr,uint,ushort,ushort,IntPtr,ushort> _funcEntry;

		private TestTwainDsm(Func<IntPtr,IntPtr,uint,ushort,ushort,IntPtr,ushort> funcEntry) {
			_funcEntry=funcEntry;
		}

		public string LibraryName {
			get {
				return "TestTwainDsm (32-bit fake)";
			}
		}

		public static DsmLoader CreateLoader(string assemblyName,string typeName,string scenario) {
			if(!assemblyName.EndsWith("Tests",StringComparison.Ordinal) || assemblyName.IndexOfAny(new[] { '\\','/',':',',' })>=0) {
				throw new ArgumentException("A test DSM comes from a test assembly of the helper's folder, by name: "+assemblyName);
			}
			string path=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,assemblyName+".dll");
			if(!File.Exists(path)) {
				throw new FileNotFoundException("No test assembly "+assemblyName+".dll in the helper's folder.");
			}
			Assembly assembly=Assembly.LoadFrom(path);
			Type type=assembly.GetType(typeName,true);
			MethodInfo methodCreate=type.GetMethod("CreateDsmEntry",BindingFlags.Public|BindingFlags.Static,null,new[] { typeof(string) },null);
			if(methodCreate==null || methodCreate.ReturnType!=typeof(Func<IntPtr,IntPtr,uint,ushort,ushort,IntPtr,ushort>)) {
				throw new MissingMethodException(typeName,"CreateDsmEntry");
			}
			return (out string errorText) => {
				errorText="";
				object entry=methodCreate.Invoke(null,new object[] { scenario });
				if(entry==null) {
					errorText="The test DSM ("+scenario+") has no TWAIN Data Source Manager.";
					return null;
				}
				return new TestTwainDsm((Func<IntPtr,IntPtr,uint,ushort,ushort,IntPtr,ushort>)entry);
			};
		}

		public ushort Entry(IntPtr origin,IntPtr dest,uint dg,ushort dat,ushort msg,IntPtr data) {
			return _funcEntry(origin,dest,dg,dat,msg,data);
		}

		public void Dispose() {
		}
	}
}
