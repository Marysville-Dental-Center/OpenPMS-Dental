// The WPF chart on x64 (package TC-10, docs/port-notes/TC-10.md; the TC-10 row of docs/specs/wpf-tooth-chart-plan.md, in S3a-15's place under
// the plan's section 6 rule). The lane runs this assembly as x86 (the test runner) and as x64 (its x64 test build, -Projects ToothChartImageTests
// -X64Test, into bin\x64\Debug): the chart assembly and the libraries below it load in the bitness of the test process, the Helix files are
// IL-only and not 32-bit-required, the normal build stays 32-bit-required (PlatformTarget=x86, docs/specs/net10-plan.md section 2.3; x64 on
// .NET 10 since stage 3b, S3b-5W), and the
// largest picture (the big chart window's 926x858, its 3D layer at 3x) renders against its baseline, with its memory measured, in both bitnesses.
// No window is shown and no program is started.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental;
using SparksToothChart;
using SparksToothChart.Wpf3D;

namespace ToothChartImageTests {
	///<summary>The facts of a PE file that say where it loads: the COFF machine, PE32 or PE32+, and the CLI header's flags (0 for a native file).
	///Read from the file's bytes, so nothing is loaded and a file of the other bitness can be read too.</summary>
	public class PeFacts {
		public const ushort MACHINE_I386=0x014C;
		public const ushort MACHINE_AMD64=0x8664;
		public const uint COMIMAGE_FLAGS_ILONLY=0x00000001;
		public const uint COMIMAGE_FLAGS_32BITREQUIRED=0x00000002;
		public const uint COMIMAGE_FLAGS_32BITPREFERRED=0x00020000;
		public ushort Machine;
		public bool IsPE32Plus;
		public bool IsManaged;
		public uint CorFlags;

		public bool IsILOnly {
			get { return (CorFlags & COMIMAGE_FLAGS_ILONLY)!=0; }
		}

		public bool Is32BitRequired {
			get { return (CorFlags & COMIMAGE_FLAGS_32BITREQUIRED)!=0; }
		}

		public bool Is32BitPreferred {
			get { return (CorFlags & COMIMAGE_FLAGS_32BITPREFERRED)!=0; }
		}

		///<summary>True when a 64-bit process can load the file as a managed assembly: PE32+ for AMD64, or PE32 that is IL-only without the 32-bit
		///flags (AnyCPU).</summary>
		public bool LoadsIn64BitProcess {
			get {
				if(!IsManaged) {
					return false;
				}
				if(IsPE32Plus) {
					return Machine==MACHINE_AMD64;
				}
				return Machine==MACHINE_I386 && IsILOnly && !Is32BitRequired && !Is32BitPreferred;
			}
		}

		public override string ToString() {
			return (IsPE32Plus?"PE32+":"PE32")+" machine 0x"+Machine.ToString("X4")+(IsManaged?", CLI flags 0x"+CorFlags.ToString("X8")
				+(IsILOnly?" ILONLY":"")+(Is32BitRequired?" 32BITREQUIRED":"")+(Is32BitPreferred?" 32BITPREFERRED":""):", native");
		}

		public static PeFacts Read(string path) {
			byte[] arrayBytes=File.ReadAllBytes(path);
			Assert.IsTrue(arrayBytes.Length>0x40 && BitConverter.ToUInt16(arrayBytes,0)==0x5A4D,path+" is not a PE file.");//"MZ"
			int offsetPe=BitConverter.ToInt32(arrayBytes,0x3C);
			Assert.AreEqual(0x00004550u,BitConverter.ToUInt32(arrayBytes,offsetPe),path+" has no PE signature.");//"PE\0\0"
			int offsetCoff=offsetPe+4;
			PeFacts peFacts=new PeFacts();
			peFacts.Machine=BitConverter.ToUInt16(arrayBytes,offsetCoff);
			int countSections=BitConverter.ToUInt16(arrayBytes,offsetCoff+2);
			int sizeOptionalHeader=BitConverter.ToUInt16(arrayBytes,offsetCoff+16);
			int offsetOptional=offsetCoff+20;
			ushort magic=BitConverter.ToUInt16(arrayBytes,offsetOptional);
			Assert.IsTrue(magic==0x10B || magic==0x20B,path+": optional header magic 0x"+magic.ToString("X"));
			peFacts.IsPE32Plus=magic==0x20B;
			//The data directories start at 96 (PE32) or 112 (PE32+); the CLI header is the 15th (index 14).
			int offsetDirectories=offsetOptional+(peFacts.IsPE32Plus?112:96);
			int countDirectories=BitConverter.ToInt32(arrayBytes,offsetDirectories-4);
			if(countDirectories<15) {
				return peFacts;
			}
			uint rvaCli=BitConverter.ToUInt32(arrayBytes,offsetDirectories+14*8);
			if(rvaCli==0) {
				return peFacts;
			}
			int offsetSections=offsetOptional+sizeOptionalHeader;
			for(int i=0;i<countSections;i++) {
				int offsetSection=offsetSections+i*40;
				uint sizeVirtual=BitConverter.ToUInt32(arrayBytes,offsetSection+8);
				uint addressVirtual=BitConverter.ToUInt32(arrayBytes,offsetSection+12);
				uint sizeRaw=BitConverter.ToUInt32(arrayBytes,offsetSection+16);
				uint pointerRaw=BitConverter.ToUInt32(arrayBytes,offsetSection+20);
				if(rvaCli>=addressVirtual && rvaCli<addressVirtual+Math.Max(sizeVirtual,sizeRaw)) {
					int offsetCli=(int)(rvaCli-addressVirtual+pointerRaw);
					peFacts.IsManaged=true;
					peFacts.CorFlags=BitConverter.ToUInt32(arrayBytes,offsetCli+16);//cb, runtime version, metadata, then Flags
					return peFacts;
				}
			}
			Assert.Fail(path+": the CLI header's address is in no section.");
			return peFacts;
		}
	}

	[TestClass]
	public class ChartBitnessTests {
		///<summary>"x64" or "x86": the bitness of this test process.</summary>
		private static string Bitness() {
			return Environment.Is64BitProcess?"x64":"x86";
		}

		///<summary>The assemblies of the chart's stack that the x64 test build builds again for x64 (the test project's references and SparksToothChart),
		///by a type of each.</summary>
		private static List<Assembly> GetChartStack() {
			return new List<Assembly> { typeof(ToothChartWrapper).Assembly,typeof(ToothChartRelay).Assembly,typeof(CodeBase.ODFileUtils).Assembly,
				typeof(OpenPmsDentalBusiness.Patient).Assembly,typeof(DataConnectionBase.DataConnection).Assembly,typeof(ChartBitnessTests).Assembly };
		}

		[TestMethod]
		public void ChartAssembly_LoadsInThisProcess_AsBuiltForItsBitness() {
			//The loaded SparksToothChart.dll and the libraries below it, read from their files: in the x64 test build each is PE32+ for AMD64 or
			//AnyCPU (IL-only without the 32-bit flags); in the x86 build each is 32-bit (the 32-bit-required flag, as PlatformTarget=x86 makes it).
			List<string> listLines=new List<string>();
			foreach(Assembly assembly in GetChartStack()) {
				string path=assembly.Location;
				PeFacts peFacts=PeFacts.Read(path);
				assembly.ManifestModule.GetPEKind(out PortableExecutableKinds portableExecutableKinds,out ImageFileMachine imageFileMachine);
				string line=Path.GetFileName(path)+": "+peFacts+" (the CLR: "+portableExecutableKinds+", "+imageFileMachine+")";
				listLines.Add(line);
				Assert.IsTrue(peFacts.IsManaged && peFacts.IsILOnly,line);
				if(Environment.Is64BitProcess) {
					Assert.IsTrue(peFacts.LoadsIn64BitProcess,"Loads in a 64-bit process: "+line);
					Assert.IsFalse(peFacts.Is32BitRequired,line);
				}
				else {
					Assert.AreEqual(PeFacts.MACHINE_I386,peFacts.Machine,line);
					Assert.IsFalse(peFacts.IsPE32Plus,line);
					Assert.IsTrue(peFacts.Is32BitRequired,"The x86 build stays 32-bit-required: "+line);
				}
				Assert.AreEqual(Environment.Is64BitProcess,(portableExecutableKinds & PortableExecutableKinds.Required32Bit)==0,line);
			}
			//The chart assembly itself: the x64 test build makes it PE32+ for AMD64 (the lane's PlatformTarget=x64).
			PeFacts peFactsChart=PeFacts.Read(typeof(ToothChartWrapper).Assembly.Location);
			if(Environment.Is64BitProcess) {
				Assert.IsTrue(peFactsChart.IsPE32Plus && peFactsChart.Machine==PeFacts.MACHINE_AMD64 || peFactsChart.LoadsIn64BitProcess,peFactsChart.ToString());
			}
			Assert.AreEqual(Environment.Is64BitProcess?8:4,IntPtr.Size);
			WpfCharts.WriteEvidence("chart-bitness."+Bitness()+".txt","ChartBitness "+Bitness()+": "+string.Join(" | ",listLines));
		}

		[TestMethod]
		public void HelixLibraries_AreILOnlyAndNot32BitRequired() {
			//The Helix files next to the test assembly, as the restore copied them (the packages of SparksToothChart, TC-4): AnyCPU, so the same files
			//serve both bitnesses. And the ones SparksToothChart references load in this process.
			string folder=Path.GetDirectoryName(typeof(ChartBitnessTests).Assembly.Location);
			List<string> listHelix=Directory.GetFiles(folder,"HelixToolkit*.dll").Select(x => Path.GetFileName(x)).OrderBy(x => x,StringComparer.OrdinalIgnoreCase).ToList();
			List<string> listReferenced=typeof(ToothChartWrapper).Assembly.GetReferencedAssemblies().Select(x => x.Name)
				.Where(x => x.StartsWith("HelixToolkit",StringComparison.OrdinalIgnoreCase)).ToList();
			Assert.IsTrue(listReferenced.Count>0,"SparksToothChart references Helix.");
			foreach(string name in listReferenced) {
				CollectionAssert.Contains(listHelix,name+".dll","Next to the test assembly.");
			}
			Assert.IsTrue(listHelix.Count>=4,"The four Helix files of 3.1.2: "+string.Join(", ",listHelix));
			List<string> listLines=new List<string>();
			foreach(string fileName in listHelix) {
				string path=Path.Combine(folder,fileName);
				PeFacts peFacts=PeFacts.Read(path);
				string line=fileName+": "+peFacts;
				listLines.Add(line);
				Assert.IsTrue(peFacts.IsManaged,line);
				Assert.IsFalse(peFacts.IsPE32Plus,"AnyCPU is PE32: "+line);
				Assert.AreEqual(PeFacts.MACHINE_I386,peFacts.Machine,line);
				Assert.IsTrue(peFacts.IsILOnly,"IL-only: "+line);
				Assert.IsFalse(peFacts.Is32BitRequired,"Not 32-bit-required: "+line);
				Assert.IsFalse(peFacts.Is32BitPreferred,"Not 32-bit-preferred: "+line);
				Assert.IsTrue(peFacts.LoadsIn64BitProcess,line);
				Assert.AreEqual(ProcessorArchitecture.MSIL,GetClrProcessorArchitecture(path),"The runtime's header reader: "+line);
			}
			foreach(string name in listReferenced) {
				Assembly assembly=Assembly.Load(typeof(ToothChartWrapper).Assembly.GetReferencedAssemblies().First(x => x.Name==name));
				Assert.AreEqual(folder.TrimEnd('\\'),Path.GetDirectoryName(assembly.Location).TrimEnd('\\'),name+" loads from the test folder.");
			}
			WpfCharts.WriteEvidence("helix-bitness."+Bitness()+".txt","Helix "+Bitness()+": "+string.Join(" | ",listLines));
		}

		///<summary>The processor architecture of an assembly file as the runtime's own header reader gives it, without loading the file: on .NET
		///Framework AssemblyName.GetAssemblyName's ProcessorArchitecture; on .NET, where that property is always None (SYSLIB0037, S3b-5W),
		///System.Reflection.Metadata's PEReader with the same rule (PE32+ for AMD64: Amd64; PE32 for I386: X86 when 32-bit-required or not IL-only,
		///otherwise MSIL).</summary>
		public static ProcessorArchitecture GetClrProcessorArchitecture(string path) {
#if NETFRAMEWORK
			return AssemblyName.GetAssemblyName(path).ProcessorArchitecture;
#else
			using FileStream fileStream=File.OpenRead(path);
			using System.Reflection.PortableExecutable.PEReader peReader=new System.Reflection.PortableExecutable.PEReader(fileStream);
			System.Reflection.PortableExecutable.PEHeaders peHeaders=peReader.PEHeaders;
			if(peHeaders.CorHeader==null) {
				return ProcessorArchitecture.None;
			}
			System.Reflection.PortableExecutable.CorFlags corFlags=peHeaders.CorHeader.Flags;
			if(peHeaders.CoffHeader.Machine==System.Reflection.PortableExecutable.Machine.Amd64) {
				return ProcessorArchitecture.Amd64;
			}
			if(peHeaders.CoffHeader.Machine!=System.Reflection.PortableExecutable.Machine.I386) {
				return ProcessorArchitecture.None;
			}
			if((corFlags & System.Reflection.PortableExecutable.CorFlags.Requires32Bit)!=0 || (corFlags & System.Reflection.PortableExecutable.CorFlags.ILOnly)==0) {
				return ProcessorArchitecture.X86;
			}
			return ProcessorArchitecture.MSIL;
#endif
		}

		[TestMethod]
		public void NormalBuild_IsX86OnNetFramework_AndX64OnNet10() {
			//net10-plan section 2.3: PlatformTarget=x86 for every SDK-style .NET Framework project until stage 3b; the x64 test build was only the
			//lane's switch (PlatformTarget=x64 into bin\x64\ and obj\x64\). At 3b (S3b-1) every .NET 10 project is x64 (Directory.Build.Net.props),
			//the 32-bit helper aside. No project of the chart's stack sets a platform of its own. TC-10 named this test NormalBuild_StaysX86; S3b-5W
			//gave it the .NET 10 case and this name.
			string props=TestPaths.ReadSource("Directory.Build.props");
			StringAssert.Contains(props,"<PlatformTarget>x86</PlatformTarget>");
			if(!WpfCharts.IsNetFramework) {
				StringAssert.Contains(TestPaths.ReadSource("Directory.Build.Net.props"),"<PlatformTarget>x64</PlatformTarget>");
			}
			foreach(string relative in new[] { @"SparksToothChart\SparksToothChart.csproj",@"WpfControlsOD\WpfControlsOD.csproj",@"CodeBase\xCodeBase.csproj",
				@"OpenPmsDentalBusiness\OpenPmsDentalBusiness.csproj",@"DataConnectionBase\xDataConnectionBase.csproj",@"tests\ToothChartImageTests\ToothChartImageTests.csproj",
				@"tests\TestProbeHost\TestProbeHost.csproj",@"tests\Directory.Build.props" })
			{
				StringAssert.DoesNotMatch(TestPaths.ReadSource(relative),new Regex(@"<PlatformTarget>(?!x86<)"),relative+" sets no other platform target.");
			}
			//The normal build's own SparksToothChart.dll (the library's output, which the program copies), read from the file in either bitness (an
			//x86 file cannot load in an x64 run): 32-bit-required on .NET Framework, PE32+ for AMD64 on .NET 10.
			string pathX86=Path.Combine(TestPaths.RepoRoot,"SparksToothChart","bin","Debug","SparksToothChart.dll");
			if(!File.Exists(pathX86)) {
				string message="The normal build of SparksToothChart ("+pathX86+") is not built here: build the solution filter first.";
				//The rule of tests\ProjectFileTests\BuildOutput.cs: inconclusive on a developer's run, a failure when the lane or a gate requires the output.
				string required=(Environment.GetEnvironmentVariable("OPMS_REQUIRE_BUILD_OUTPUT")??"").Trim();
				if(required=="1" || required.Equals("true",StringComparison.OrdinalIgnoreCase)) {
					Assert.Fail(message+" (OPMS_REQUIRE_BUILD_OUTPUT=1: a missing build output fails.)");
				}
				Assert.Inconclusive(message);
			}
			PeFacts peFacts=PeFacts.Read(pathX86);
			if(!WpfCharts.IsNetFramework) {
				Assert.IsTrue(peFacts.IsPE32Plus,peFacts.ToString());
				Assert.AreEqual(PeFacts.MACHINE_AMD64,peFacts.Machine,peFacts.ToString());
				Assert.IsTrue(peFacts.IsILOnly && !peFacts.Is32BitRequired,"x64: "+peFacts);
				Assert.AreEqual(ProcessorArchitecture.Amd64,GetClrProcessorArchitecture(pathX86));
				return;
			}
			Assert.IsFalse(peFacts.IsPE32Plus,peFacts.ToString());
			Assert.AreEqual(PeFacts.MACHINE_I386,peFacts.Machine,peFacts.ToString());
			Assert.IsTrue(peFacts.IsILOnly && peFacts.Is32BitRequired,"32-bit-required: "+peFacts);
			Assert.AreEqual(ProcessorArchitecture.X86,GetClrProcessorArchitecture(pathX86));
		}

		[TestMethod]
		public void BigChartAt3x_RendersInThisBitness_AndKeepsItsMemory() {
			//The largest picture of the image tests: the big chart window (926x858, FormToothChartBig), whose 3D layer is drawn at 3x (2778x2574, under the
			//8-megapixel cap) and reduced. Drawn against its baseline, then six more times alternating with another scene, so each draw makes a new 3D layer:
			//the private bytes the big chart adds, and what the repeats add (a layer kept per draw would add about 27 MB each), in this bitness.
			ChartSize chartSize=ChartScenes.SizeBigWindow;
			Assert.AreEqual(new Size(926,858),chartSize.Size);
			Assert.AreEqual(3,ToothChartRenderer.GetSupersamplingFactor(chartSize.Size),"The big chart is drawn at 3x.");
			Assert.IsTrue(chartSize.Size.Width*3L*chartSize.Size.Height*3L<=ToothChartRenderer.MaxPixels3D);
			ChartScene scene=ChartScenes.Get("size-big-window");
			ChartScene sceneOther=ChartScenes.Get("size-sheet-wide");
			string pathBaseline=WpfCharts.ForThisBitness(WpfCharts.GetPicturePath(WpfCharts.BaselinesWpf,chartSize,scene));
			Assert.IsTrue(File.Exists(pathBaseline),pathBaseline);
			const int countRepeats=6;
			string evidence=StaRunner.Run(() => {
				long bytesBefore=GetPrivateBytesAfterCollect();
				Stopwatch stopwatch=Stopwatch.StartNew();
				using ToothChartWrapper wrapper=WpfCharts.CreateWrapper(chartSize.Size);
				ToothChartRelay relay=WpfCharts.CreateRelay(wrapper);
				int countFallbacks=ToothChartWrapper.GetCountFallbackMessages();
				using(Bitmap bitmap=WpfCharts.Draw(relay,scene,chartSize.ColorSet))
				using(Bitmap bitmapBaseline=WpfImageTests.LoadBitmap(pathBaseline)) {
					Assert.AreEqual(ToothChartEngine.Wpf3D,wrapper.Engine);
					Assert.IsNull(WpfCharts.GetFallbackFailure("926x858/"+scene.Name,countFallbacks));
					Assert.AreEqual(chartSize.Size,bitmap.Size);
					ChartImageCompareResult result=ChartImageCompare.Compare(bitmapBaseline,bitmap);
					if(!result.IsPass) {
						string pathDiff=ChartImageCompare.WriteFailureImages(Path.Combine(Path.GetTempPath(),"ToothChartImageTests","bitness-failures"),scene.Name,
							bitmapBaseline,bitmap);
						Assert.Fail("926x858/"+scene.Name+" in this "+Bitness()+" process: "+result+" Images: "+pathDiff);
					}
				}
				double millisecondsFirst=stopwatch.Elapsed.TotalMilliseconds;
				long bytesAfterFirst=GetPrivateBytesAfterCollect();
				for(int i=0;i<countRepeats;i++) {
					using(Bitmap bitmap=WpfCharts.Draw(relay,i%2==0?sceneOther:scene,chartSize.ColorSet)) {
						Assert.AreEqual(chartSize.Size,bitmap.Size);
					}
				}
				Assert.IsNull(WpfCharts.GetFallbackFailure("926x858 repeats",countFallbacks));
				long bytesAfterRepeats=GetPrivateBytesAfterCollect();
				Process process=Process.GetCurrentProcess();
				double mbFirst=(bytesAfterFirst-bytesBefore)/1048576.0;
				double mbRepeats=(bytesAfterRepeats-bytesAfterFirst)/1048576.0;
				string text="BigChart "+Bitness()+": 926x858 at 3x matches its baseline; first draw "+millisecondsFirst.ToString("0",CultureInfo.InvariantCulture)+" ms; "
					+"private bytes added by the wrapper and its first draw "+mbFirst.ToString("0.0",CultureInfo.InvariantCulture)+" MB; by "+countRepeats
					+" more draws (a new 3D layer each) "+mbRepeats.ToString("0.0",CultureInfo.InvariantCulture)+" MB; process private bytes "
					+(bytesAfterRepeats/1048576.0).ToString("0",CultureInfo.InvariantCulture)+" MB, peak working set "
					+(process.PeakWorkingSet64/1048576.0).ToString("0",CultureInfo.InvariantCulture)+" MB";
				//A draw keeps no 3D layer of its own: the repeats add less than two layers' worth (the cache holds at most three sizes).
				Assert.IsTrue(mbRepeats<64,text);
				//The big chart fits a 32-bit process with room to spare (the bridge release's risk, section 12 of the plan).
				Assert.IsTrue(mbFirst<400,text);
				return text;
			});
			WpfCharts.WriteEvidence("big-chart-memory."+Bitness()+".txt",evidence);
		}

		///<summary>The private bytes of this process after a full collection, which finalizes the bitmaps and render targets nothing holds any more.
		///</summary>
		private static long GetPrivateBytesAfterCollect() {
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
			Process process=Process.GetCurrentProcess();
			process.Refresh();
			return process.PrivateMemorySize64;
		}
	}
}
