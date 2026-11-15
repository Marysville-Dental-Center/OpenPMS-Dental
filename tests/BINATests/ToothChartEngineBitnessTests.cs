// Tests that the tooth chart's DirectX and OpenGL engines run in a 64-bit process (package S3a-15 of the .NET 10 move, docs/specs/net10-plan.md,
// decision 19; docs/port-notes/S3a-15.md). The engines stay on x64, with the fallback of ToothChartFallbackTests as the safety net: the chart is
// not moved to 2D on purpose. The lane runs this assembly as x86 (the test runner) and as x64 (its x64 test build), so every check here holds in
// the bitness of the test process: the engine libraries are IL-only and not 32-bit-required, they call only Windows files that exist in both
// bitnesses, every handle they take is an IntPtr, the DirectX runtime check loads the D3DX file of the process's own bitness, and each mode
// starts on a hidden form through StartFirstWorkingMode or falls back. A mode this computer cannot start is reported, never failed.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Cecil;
using OpenPmsDentalBusiness;
using SparksToothChart;

namespace BINATests {
	[TestClass]
	public class ToothChartEngineBitnessTests {
		///<summary>The managed libraries of the DirectX and OpenGL engines, file references in Required dlls (SparksToothChart and CodeBase).</summary>
		private static readonly string[] _arrayEngineLibraries=new[] { "SharpDX","SharpDX.Direct3D9","SharpDX.Mathematics","Tao.OpenGl","Tao.Platform.Windows" };
		///<summary>The Windows files the engine libraries call (their module references), which 64-bit Windows has in System32 and in SysWOW64.</summary>
		private static readonly string[] _arrayWindowsDlls=new[] { "kernel32.dll","user32.dll","gdi32.dll","ole32.dll","winmm.dll","d3d9.dll","opengl32.dll","glu32.dll" };
		///<summary>Tao.OpenGl's module references for Linux and macOS, which it only uses there.</summary>
		private static readonly string[] _arrayNonWindowsDlls=new[] { "libGL.so","libdl.dylib" };
		///<summary>The size of a hidden chart: a width whose 24-bit rows need no padding, as the OpenGL chart's picture reads them.</summary>
		private static readonly Size _sizeChart=new Size(400,300);
		private static readonly object _lockRecord=new object();
		private const ushort IMAGE_FILE_MACHINE_I386=0x014C;
		private const ushort IMAGE_FILE_MACHINE_AMD64=0x8664;

		///<summary>"x64" or "x86": the bitness of this test process.</summary>
		private static string Bitness() {
			return Environment.Is64BitProcess?"x64":"x86";
		}

		///<summary>The file next to the test assembly that records which mode came up in this bitness (ToothChartEngineModes.x86.txt or .x64.txt),
		///read for the port note (the lane's runners print only counts for a passing run; see Record).</summary>
		private static string RecordPath() {
			return Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"ToothChartEngineModes."+Bitness()+".txt");
		}

		[ClassInitialize]
		public static void ClassInitialize(TestContext testContext) {
			try {
				File.Delete(RecordPath());
			}
			catch(Exception ex) {
				Trace.WriteLine(ex.Message);
			}
		}

		///<summary>Writes line to the record file and to the test's output (Console, which MSTest keeps with the test's result, so a runner or
		///logger that shows passing tests' output carries it). The lane's runners print only counts and failures, so the port note's evidence is
		///the record file, copied out of bin before a clean.</summary>
		private static void Record(string line) {
			string lineRecord=DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")+" "+Bitness()+" "+line;
			Console.WriteLine("ToothChartEngineModes: "+lineRecord);
			lock(_lockRecord) {
				File.AppendAllText(RecordPath(),lineRecord+Environment.NewLine);
			}
		}

		///<summary>Runs action on a new STA thread, which a WinForms control wants, and rethrows what it throws (an Inconclusive as itself).</summary>
		private static void RunSta(Action action) {
			Exception exception=null;
			Thread thread=new Thread(() => {
				try {
					action();
				}
				catch(Exception ex) {
					exception=ex;
				}
			});
			thread.SetApartmentState(ApartmentState.STA);
			thread.IsBackground=true;
			thread.Start();
			Assert.IsTrue(thread.Join(180000),"The STA thread did not finish.");
			if(exception is AssertInconclusiveException || exception is AssertFailedException) {
				throw exception;
			}
			if(exception!=null) {
				throw new AssertFailedException(exception.GetType().Name+": "+exception.Message,exception);
			}
		}

		///<summary>Reads a managed file's module with Mono.Cecil from a copy of its bytes, so the file stays unlocked and nothing is loaded.</summary>
		private static T ReadModule<T>(string path,Func<ModuleDefinition,T> func) {
			using(MemoryStream memoryStream=new MemoryStream(File.ReadAllBytes(path)))
			using(ModuleDefinition moduleDefinition=ModuleDefinition.ReadModule(memoryStream)) {
				return func(moduleDefinition);
			}
		}

		///<summary>The machine field of a PE file's COFF header (IMAGE_FILE_MACHINE_I386 or IMAGE_FILE_MACHINE_AMD64 for a native DLL).</summary>
		private static ushort ReadPeMachine(string path) {
			using(FileStream fileStream=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.ReadWrite|FileShare.Delete))
			using(BinaryReader binaryReader=new BinaryReader(fileStream)) {
				Assert.AreEqual(0x5A4D,binaryReader.ReadUInt16(),path+" is not a PE file.");//"MZ"
				fileStream.Position=0x3C;
				int offsetPe=binaryReader.ReadInt32();
				fileStream.Position=offsetPe;
				Assert.AreEqual(0x00004550u,binaryReader.ReadUInt32(),path+" has no PE signature.");//"PE\0\0"
				return binaryReader.ReadUInt16();
			}
		}

		///<summary>The engine libraries this process loads: the five of _arrayEngineLibraries, by the names SparksToothChart and CodeBase reference.
		///Loading them is the check that they load in this bitness (a 32-bit-required library throws BadImageFormatException in a 64-bit process).</summary>
		private static Dictionary<string,Assembly> LoadEngineLibraries() {
			List<AssemblyName> listReferenced=typeof(ToothChartWrapper).Assembly.GetReferencedAssemblies()
				.Concat(typeof(CodeBase.OpenGLWinFormsControl).Assembly.GetReferencedAssemblies()).ToList();
			Dictionary<string,Assembly> dictionaryAssemblies=new Dictionary<string,Assembly>(StringComparer.OrdinalIgnoreCase);
			foreach(string name in _arrayEngineLibraries) {
				AssemblyName assemblyName=listReferenced.FirstOrDefault(x => string.Equals(x.Name,name,StringComparison.OrdinalIgnoreCase));
				if(assemblyName==null) {
					//SharpDX and SharpDX.Mathematics come in through SharpDX.Direct3D9's own references.
					assemblyName=new AssemblyName(name);
				}
				dictionaryAssemblies[name]=Assembly.Load(assemblyName);
			}
			return dictionaryAssemblies;
		}

		[TestMethod]
		public void EngineLibraries_AreILOnlyAndNot32BitRequired() {
			//The CLI header of each library, in Required dlls (what ships) and in the test folder (what this process loads): IL-only, no
			//32-bit-required or 32-bit-preferred flag, the PE32 machine of an AnyCPU file. Read with Mono.Cecil and with the CLR's own header
			//reader (AssemblyName.GetAssemblyName, which does not load the file), so one reader's mistake does not hide a flag.
			string folderRequired=Path.Combine(RepoFiles.Root(),"Required dlls");
			foreach(string name in _arrayEngineLibraries) {
				string pathRequired=Path.Combine(folderRequired,name+".dll");
				Assert.IsTrue(File.Exists(pathRequired),pathRequired+" is missing.");
				List<string> listPaths=new List<string> { pathRequired };
				string pathLoaded=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,name+".dll");
				if(File.Exists(pathLoaded)) {
					CollectionAssert.AreEqual(File.ReadAllBytes(pathRequired),File.ReadAllBytes(pathLoaded),pathLoaded+" is not the file of Required dlls.");
					listPaths.Add(pathLoaded);
				}
				foreach(string path in listPaths) {
					ModuleAttributes moduleAttributes=ReadModule(path,x => x.Attributes);
					TargetArchitecture targetArchitecture=ReadModule(path,x => x.Architecture);
					Assert.IsTrue(moduleAttributes.HasFlag(ModuleAttributes.ILOnly),path+" is not IL-only: "+moduleAttributes);
					Assert.IsFalse(moduleAttributes.HasFlag(ModuleAttributes.Required32Bit),path+" is 32-bit-required: "+moduleAttributes);
					Assert.IsFalse(moduleAttributes.HasFlag(ModuleAttributes.Preferred32Bit),path+" prefers 32-bit: "+moduleAttributes);
					Assert.AreEqual(TargetArchitecture.I386,targetArchitecture,path+" is not an AnyCPU (PE32) file.");
					Assert.AreEqual(ProcessorArchitecture.MSIL,AssemblyName.GetAssemblyName(path).ProcessorArchitecture,path);
				}
			}
		}

		[TestMethod]
		public void EngineLibraries_LoadInThisProcess() {
			Dictionary<string,Assembly> dictionaryAssemblies=LoadEngineLibraries();
			foreach(string name in _arrayEngineLibraries) {
				Assembly assembly=dictionaryAssemblies[name];
				Assert.AreEqual(ProcessorArchitecture.MSIL,assembly.GetName().ProcessorArchitecture,name);
				Assert.IsTrue(assembly.GetTypes().Length>0,name+" has no type.");//every type loads in this bitness
			}
			//The types the charts are made of load too.
			Assert.IsNotNull(typeof(ToothChartDirectX).TypeHandle);
			Assert.IsNotNull(typeof(ToothChartOpenGL).TypeHandle);
		}

		[TestMethod]
		public void EngineLibraries_CallOnlyWindowsFilesOfBothBitnesses() {
			string folderRequired=Path.Combine(RepoFiles.Root(),"Required dlls");
			List<string> listUnknown=new List<string>();
			foreach(string name in _arrayEngineLibraries) {
				List<string> listModules=ReadModule(Path.Combine(folderRequired,name+".dll"),x => x.ModuleReferences.Select(y => y.Name).ToList());
				foreach(string module in listModules) {
					string fileName=Path.HasExtension(module)?module:module+".dll";//SharpDX names kernel32 both ways
					if(_arrayWindowsDlls.Contains(fileName,StringComparer.OrdinalIgnoreCase) || _arrayNonWindowsDlls.Contains(fileName)
						|| string.Equals(fileName,ToothChartWrapper.D3DX_DLL,StringComparison.OrdinalIgnoreCase))
					{
						continue;
					}
					listUnknown.Add(name+": "+module);
				}
			}
			Assert.AreEqual(0,listUnknown.Count,"Native files not checked for both bitnesses: "+string.Join(", ",listUnknown));
			//d3dx9_43.dll is Microsoft's DirectX End-User Runtime, checked in the process's own bitness (DirectXRuntimeCheck_...). The others
			//are part of Windows: 64-bit Windows has each in System32 (64-bit) and in SysWOW64 (32-bit).
			string folderNative=CodeBase.ODSystemFolders.GetNativeSystemFolder();
			string folderX86=Environment.GetFolderPath(Environment.SpecialFolder.SystemX86);
			foreach(string fileName in _arrayWindowsDlls) {
				string pathNative=Path.Combine(folderNative,fileName);
				Assert.IsTrue(File.Exists(pathNative),pathNative+" is missing.");
				if(Environment.Is64BitOperatingSystem) {
					Assert.AreEqual(IMAGE_FILE_MACHINE_AMD64,ReadPeMachine(pathNative),pathNative);
					Assert.AreEqual(IMAGE_FILE_MACHINE_I386,ReadPeMachine(Path.Combine(folderX86,fileName)),Path.Combine(folderX86,fileName));
				}
			}
		}

		[TestMethod]
		public void Handles_ArePassedAsIntPtr() {
			//Every native declaration of the engine libraries, of SparksToothChart and of CodeBase's OpenGL control that takes a window, device,
			//rendering context or module handle takes it as IntPtr, pointer-sized in either bitness. The one int handle is Tao's joystick capture,
			//which no code calls.
			Regex regexHandleName=new Regex(@"^h[A-Z]|^(?i:hdc|hwnd|hglrc|hmodule|hinstance|hrgn|hpal)$|(?i:handle|context)$");
			List<string> listPaths=_arrayEngineLibraries.Select(x => Path.Combine(RepoFiles.Root(),"Required dlls",x+".dll")).ToList();
			listPaths.Add(typeof(ToothChartWrapper).Assembly.Location);
			listPaths.Add(typeof(CodeBase.OpenGLWinFormsControl).Assembly.Location);
			List<string> listOffenders=new List<string>();
			int countDeclarations=0;
			foreach(string path in listPaths) {
				bool isCodeBase=string.Equals(path,typeof(CodeBase.OpenGLWinFormsControl).Assembly.Location,StringComparison.OrdinalIgnoreCase);
				ReadModule(path,moduleDefinition => {
					foreach(TypeDefinition typeDefinition in moduleDefinition.GetTypes()) {
						if(isCodeBase && typeDefinition.FullName!=typeof(CodeBase.OpenGLWinFormsControl).FullName) {
							continue;//the rest of CodeBase is NoHqTests' pointer-size scan (S3a-12)
						}
						foreach(MethodDefinition methodDefinition in typeDefinition.Methods.Where(x => x.IsPInvokeImpl)) {
							countDeclarations++;
							foreach(ParameterDefinition parameterDefinition in methodDefinition.Parameters) {
								string typeName=parameterDefinition.ParameterType.FullName;
								if((typeName=="System.Int32" || typeName=="System.UInt32") && regexHandleName.IsMatch(parameterDefinition.Name)) {
									listOffenders.Add(Path.GetFileName(path)+": "+typeDefinition.Name+"."+methodDefinition.Name+"("+parameterDefinition.Name+")");
								}
							}
						}
					}
					return 0;
				});
			}
			Assert.IsTrue(countDeclarations>100,"Too few native declarations found ("+countDeclarations+"): the scan read the wrong files.");
			CollectionAssert.AreEqual(new[] { "Tao.Platform.Windows.dll: Winmm.joySetCapture(hwnd)" },listOffenders,
				"Handles that are not IntPtr: "+string.Join(", ",listOffenders));
			List<string> listCallers=RepoFiles.Files("*.cs").Where(x => RepoFiles.Read(x).Contains("joySetCapture")).ToList();
			Assert.AreEqual(0,listCallers.Count,"Tao's joySetCapture takes its window as int: "+string.Join(", ",listCallers));
			//The calls the charts make, by their signatures in the loaded libraries.
			Dictionary<string,Assembly> dictionaryAssemblies=LoadEngineLibraries();
			Type typePresentParameters=dictionaryAssemblies["SharpDX.Direct3D9"].GetType("SharpDX.Direct3D9.PresentParameters",true);
			Assert.AreEqual(typeof(IntPtr),typePresentParameters.GetField("DeviceWindowHandle").FieldType,"PresentParameters.DeviceWindowHandle");
			Type typeDevice=dictionaryAssemblies["SharpDX.Direct3D9"].GetType("SharpDX.Direct3D9.Device",true);
			Assert.IsTrue(typeDevice.GetConstructors().Any(x => x.GetParameters().Length==6 && x.GetParameters()[3].ParameterType==typeof(IntPtr)
				&& x.GetParameters()[3].Name=="hFocusWindow"),"Device(Direct3D,int,DeviceType,IntPtr hFocusWindow,CreateFlags,PresentParameters[])");
			Assembly assemblyTaoWindows=dictionaryAssemblies["Tao.Platform.Windows"];
			Type typeGdi=assemblyTaoWindows.GetType("Tao.Platform.Windows.Gdi",true);
			Type typeWgl=assemblyTaoWindows.GetType("Tao.Platform.Windows.Wgl",true);
			Type typeUser=assemblyTaoWindows.GetType("Tao.Platform.Windows.User",true);
			Type typePixelFormat=assemblyTaoWindows.GetType("Tao.Platform.Windows.Gdi+PIXELFORMATDESCRIPTOR",true);
			AssertSignature(typeWgl,"wglCreateContext",typeof(IntPtr),typeof(IntPtr));
			AssertSignature(typeWgl,"wglMakeCurrent",typeof(bool),typeof(IntPtr),typeof(IntPtr));
			AssertSignature(typeWgl,"wglDeleteContext",typeof(bool),typeof(IntPtr));
			AssertSignature(typeWgl,"wglGetCurrentContext",typeof(IntPtr));
			AssertSignature(typeGdi,"SetPixelFormat",typeof(bool),typeof(IntPtr),typeof(int),typePixelFormat.MakeByRefType());
			AssertSignature(typeGdi,"SwapBuffersFast",typeof(int),typeof(IntPtr));
			AssertSignature(typeUser,"GetDC",typeof(IntPtr),typeof(IntPtr));
			AssertSignature(typeUser,"ReleaseDC",typeof(bool),typeof(IntPtr),typeof(IntPtr));
			AssertSignature(typeof(CodeBase.OpenGLWinFormsControl),"_DescribePixelFormat",typeof(int),typeof(IntPtr),typeof(int),typeof(uint),
				typePixelFormat.MakeByRefType());
			AssertSignature(typeof(CodeBase.OpenGLWinFormsControl),"GetHDC",typeof(IntPtr));
			BindingFlags bindingFlags=BindingFlags.Instance|BindingFlags.NonPublic;
			Assert.AreEqual(typeof(IntPtr),typeof(CodeBase.OpenGLWinFormsControl).GetField("deviceContext",bindingFlags).FieldType);
			Assert.AreEqual(typeof(IntPtr),typeof(CodeBase.OpenGLWinFormsControl).GetField("renderContext",bindingFlags).FieldType);
			//PIXELFORMATDESCRIPTOR has no pointer: 40 bytes in both bitnesses, as nSize tells Windows.
			Assert.AreEqual(40,Marshal.SizeOf(typePixelFormat));
			//The sources hand the window handle over as it is: never cut to an int.
			string directX=RepoFiles.Read(@"SparksToothChart\ToothChartDirectX.cs");
			StringAssert.Contains(directX,"pp.DeviceWindowHandle=control.Handle;");
			StringAssert.Contains(directX,"device=new Device(_d3d,adapterIndex,deviceType,control.Handle,createFlagsBehavior,presentParameters);");
			StringAssert.Contains(RepoFiles.Read(@"CodeBase\Controls\OpenGLWinFormsControl.cs"),"return User.GetDC(this.Handle);");
			Regex regexCut=new Regex(@"\(\s*(int|uint|Int32|UInt32|long)\s*\)\s*[\w.]*Handle\b|Handle\s*\.\s*ToInt(32|64)\s*\(|\b(hdc|hwnd|hglrc)\s*\.\s*ToInt32\s*\(");
			List<string> listCuts=new List<string>();
			foreach(string relative in Directory.GetFiles(Path.Combine(RepoFiles.Root(),"SparksToothChart"),"*.cs",SearchOption.AllDirectories)
				.Select(x => x.Substring(RepoFiles.Root().Length).TrimStart('\\')).Where(x => !x.Contains(@"\obj\") && !x.Contains(@"\bin\"))
				.Concat(new[] { @"CodeBase\Controls\OpenGLWinFormsControl.cs" }))
			{
				foreach(Match match in regexCut.Matches(RepoFiles.Read(relative))) {
					listCuts.Add(relative+": "+match.Value);
				}
			}
			Assert.AreEqual(0,listCuts.Count,"A handle cut to a number: "+string.Join(", ",listCuts));
		}

		///<summary>Asserts that type has a static method name with exactly these parameter types and this return type.</summary>
		private static void AssertSignature(Type type,string name,Type typeReturn,params Type[] arrayTypesParameter) {
			MethodInfo methodInfo=type.GetMethods(BindingFlags.Static|BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic)
				.FirstOrDefault(x => x.Name==name && x.GetParameters().Select(y => y.ParameterType).SequenceEqual(arrayTypesParameter));
			Assert.IsNotNull(methodInfo,type.Name+"."+name+"("+string.Join(",",arrayTypesParameter.Select(x => x.Name))+") not found.");
			Assert.AreEqual(typeReturn,methodInfo.ReturnType,type.Name+"."+name);
		}

		[TestMethod]
		public void DirectXRuntimeCheck_LoadsTheD3dxOfThisBitness() {
			//A 64-bit process needs the 64-bit d3dx9_43.dll in System32, a 32-bit one the 32-bit file in SysWOW64 (System32 on 32-bit Windows).
			//The DirectX End-User Runtime installs both on 64-bit Windows; IsDirectXRuntimeAvailable loads the file, so it sees only this bitness's.
			bool isAvailable=ToothChartWrapper.IsDirectXRuntimeAvailable();
			string folder=Environment.Is64BitProcess?CodeBase.ODSystemFolders.GetNativeSystemFolder():Environment.GetFolderPath(Environment.SpecialFolder.SystemX86);
			string pathSystem=Path.Combine(folder,ToothChartWrapper.D3DX_DLL);
			Record("d3dx9_43.dll in "+folder+": "+(File.Exists(pathSystem)?"installed":"missing")+"; IsDirectXRuntimeAvailable="+isAvailable);
			if(File.Exists(pathSystem)) {
				Assert.IsTrue(isAvailable,pathSystem+" is installed, so the DirectX chart can start in this "+Bitness()+" process.");
			}
			if(!isAvailable) {
				Assert.Inconclusive(ToothChartWrapper.D3DX_DLL+" is not installed for "+Bitness()+" processes on this computer ("+pathSystem
					+" missing), so the DirectX chart falls back to OpenGL here.");
			}
			//The file this process loads is one of its own bitness.
			IntPtr intPtrModule=LoadLibrary(ToothChartWrapper.D3DX_DLL);
			Assert.AreNotEqual(IntPtr.Zero,intPtrModule,"IsDirectXRuntimeAvailable said true, but "+ToothChartWrapper.D3DX_DLL+" does not load.");
			try {
				StringBuilder stringBuilder=new StringBuilder(1024);
				Assert.IsTrue(GetModuleFileName(intPtrModule,stringBuilder,stringBuilder.Capacity)>0);
				//In a 32-bit process on 64-bit Windows the name says System32, which WOW64 redirects to SysWOW64 for the load and for the read below.
				string pathLoaded=stringBuilder.ToString();
				ushort machineExpected=Environment.Is64BitProcess?IMAGE_FILE_MACHINE_AMD64:IMAGE_FILE_MACHINE_I386;
				Assert.AreEqual(machineExpected,ReadPeMachine(pathLoaded),pathLoaded+" is not a "+Bitness()+" file.");
				Record("d3dx9_43.dll loaded from "+pathLoaded+(CodeBase.ODSystemFolders.IsWow64Process()?" (WOW64: the SysWOW64 file)":"")
					+", PE machine 0x"+machineExpected.ToString("X4"));
			}
			finally {
				FreeLibrary(intPtrModule);
			}
		}

		[DllImport("kernel32.dll",CharSet=CharSet.Unicode,SetLastError=true)]
		private static extern IntPtr LoadLibrary(string lpFileName);

		[DllImport("kernel32.dll",SetLastError=true)]
		private static extern bool FreeLibrary(IntPtr hModule);

		[DllImport("kernel32.dll",CharSet=CharSet.Unicode,SetLastError=true)]
		private static extern int GetModuleFileName(IntPtr hModule,StringBuilder lpFilename,int nSize);

		#region Modes on a hidden form
		///<summary>Starts drawingModeRequested on a chart on a hidden form, as the Chart module does (ToothChartRelay: hardware graphics, the saved
		///pixel format, here none, and for DirectX the device format the program picks with GetPreferredDirectXFormat), through DrawMode and so
		///StartFirstWorkingMode. Records the mode that came up and whether it drew a picture. The Simple 2D chart must start and draw; DirectX and
		///OpenGL may fall back, which is reported as Inconclusive with the reason after the mode it fell back to has drawn.</summary>
		private static void StartModeOnHiddenForm(DrawingMode drawingModeRequested) {
			RunSta(() => {
				using(Form form=new Form()) {
					form.ShowInTaskbar=false;
					form.FormBorderStyle=FormBorderStyle.None;
					form.StartPosition=FormStartPosition.Manual;
					form.ClientSize=_sizeChart;
					IntPtr intPtrForm=form.Handle;//creates the form's window; the form is never shown
					Assert.AreNotEqual(IntPtr.Zero,intPtrForm);
					Stopwatch stopwatch=Stopwatch.StartNew();
					string directXFormat=null;
					if(drawingModeRequested==DrawingMode.DirectX) {
						directXFormat=ToothChartWrapper.GetPreferredDirectXFormat(form);//"invalid" without D3DX or without a working format
					}
					long msFormat=stopwatch.ElapsedMilliseconds;
					int countFallbacksBefore=ToothChartWrapper.GetCountFallbackMessages();
					string drawResult;
					DrawingMode drawingModeStarted;
					int pixelFormat;
					using(ToothChartWrapper wrapper=new ToothChartWrapper()) {
						wrapper.Location=new Point(0,0);
						wrapper.Size=_sizeChart;
						wrapper.UseHardware=true;
						wrapper.PreferredPixelFormatNumber=0;
						if(directXFormat!=null && directXFormat!="invalid") {
							wrapper.DeviceFormat=new ToothChartDirectX.DirectXDeviceFormat(directXFormat);
						}
						form.Controls.Add(wrapper);
						wrapper.DrawMode=drawingModeRequested;
						drawingModeStarted=wrapper.DrawMode;
						pixelFormat=wrapper.PreferredPixelFormatNumber;
						Assert.IsFalse(form.Visible,"The form was shown.");
						try {
							using(Bitmap bitmap=wrapper.GetBitmap()) {
								Assert.AreEqual(_sizeChart,bitmap.Size);
								drawResult="drew a "+bitmap.Width+"x"+bitmap.Height+" picture";
							}
						}
						catch(Exception ex) when(!(ex is AssertFailedException)) {
							drawResult="could not draw ("+ex.GetType().Name+": "+ex.Message+")";
						}
						form.Controls.Remove(wrapper);
					}
					List<string> listFallbacks=ToothChartWrapper.GetFallbackMessagesSince(countFallbacksBefore);
					string line="requested "+drawingModeRequested+", came up "+drawingModeStarted+", "+drawResult
						+(drawingModeRequested==DrawingMode.DirectX?", device format "+(directXFormat??"none")+" (search "+msFormat+" ms)":"")
						+(drawingModeStarted==DrawingMode.OpenGL?", pixel format "+pixelFormat:"")
						+", "+stopwatch.ElapsedMilliseconds+" ms"+(listFallbacks.Count==0?"":"; fallback: "+string.Join(" | ",listFallbacks));
					Record(line);
					//The mode that came up is the one asked for or one of its fallbacks, and it is a mode that drew, unless it is the one asked for and
					//this computer cannot draw with it.
					DrawingMode drawingMode=drawingModeRequested;
					List<DrawingMode> listAllowed=new List<DrawingMode> { drawingMode };
					while(drawingMode!=DrawingMode.Simple2D) {
						drawingMode=ToothChartWrapper.GetFallbackDrawMode(drawingMode);
						listAllowed.Add(drawingMode);
					}
					CollectionAssert.Contains(listAllowed,drawingModeStarted,line);
					bool isDrawn=drawResult.StartsWith("drew");
					if(drawingModeStarted==DrawingMode.Simple2D) {
						Assert.IsTrue(isDrawn,"The Simple 2D chart, the last resort, must draw: "+line);
					}
					if(drawingModeStarted!=drawingModeRequested) {
						Assert.IsTrue(isDrawn,"The fallback mode must draw: "+line);
						Assert.IsTrue(listFallbacks.Count>0,"A fallback is logged: "+line);
						Assert.Inconclusive("This "+Bitness()+" process could not start "+drawingModeRequested+" here; the fallback ran: "+line);
					}
					Assert.AreEqual(0,listFallbacks.Count,"No fallback when the mode started: "+line);
					if(drawingModeRequested==DrawingMode.DirectX && directXFormat=="invalid") {
						Assert.Inconclusive("No DirectX device format works in this "+Bitness()+" process here (the program then stores OpenGL): "+line);
					}
					if(!isDrawn) {
						Assert.Inconclusive("This "+Bitness()+" process started "+drawingModeRequested+" but could not draw with it here: "+line);
					}
				}
			});
		}

		[TestMethod]
		public void DirectX_StartsOnAHiddenFormOrFallsBack() {
			StartModeOnHiddenForm(DrawingMode.DirectX);
		}

		[TestMethod]
		public void OpenGL_StartsOnAHiddenFormOrFallsBack() {
			StartModeOnHiddenForm(DrawingMode.OpenGL);
		}

		[TestMethod]
		public void Simple2D_StartsOnAHiddenForm() {
			StartModeOnHiddenForm(DrawingMode.Simple2D);
		}
		#endregion Modes on a hidden form
	}
}
