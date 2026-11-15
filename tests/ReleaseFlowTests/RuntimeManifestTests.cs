// The release-channel guards of the bridge release (S3a-10 of docs/specs/net10-plan.md, docs/port-notes/S3a-10.md): the Runtime line and the
// runtime part's lines of a release manifest, and the two refusals, a manifest that is not readable and a release for another runtime or
// architecture. Synthetic values only.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalBusiness;

namespace ReleaseFlowTests {
	[TestClass]
	public class RuntimeManifestTests {
		private static readonly UpdateServerRole[] _arrayRoles=new[] { UpdateServerRole.ThisComputer,UpdateServerRole.AnyWorkstation,UpdateServerRole.OtherComputer };
		private static readonly DateTime _dateTimeOld=new DateTime(2026,11,1,12,0,0,DateTimeKind.Utc);
		private static readonly DateTime _dateTimeNew=new DateTime(2027,2,1,12,0,0,DateTimeKind.Utc);

		///<summary>The example .NET 10 manifest of the port note (tests\ReleaseFlowTests\Fixtures\Net10Manifest.example.txt).</summary>
		private static string ReadNet10Example() {
			return TestPaths.ReadSource("tests","ReleaseFlowTests","Fixtures","Net10Manifest.example.txt").Replace("\r\n","\n");
		}

		private static ReleaseIdentity RunningBridge(string release="0.0",string buildId="bridge",DateTime? dateTimeBuild=null) {
			return new ReleaseIdentity { ReleaseVersion=release,BuildId=buildId,BuildUtc=dateTimeBuild??_dateTimeOld,Runtime=ReleaseRuntime.NetFramework48X86 };
		}

		private static ReleaseManifest Published(string release,string buildId,DateTime dateTimeBuild,ReleaseRuntime releaseRuntime=null) {
			ReleaseManifest releaseManifest=new ReleaseManifest();
			releaseManifest.ReleaseVersion=release;
			releaseManifest.BuildId=buildId;
			releaseManifest.BuildUtc=dateTimeBuild;
			releaseManifest.Runtime=releaseRuntime;
			return releaseManifest;
		}

		private static ReleaseRuntimePart MakeRuntimePart(string version) {
			ReleaseRuntimePart releaseRuntimePart=new ReleaseRuntimePart();
			releaseRuntimePart.Version=version;
			releaseRuntimePart.ListArchitectures=new List<string> { "win-x64","win-x86" };
			releaseRuntimePart.ArchiveSha256=new string('6',64);
			releaseRuntimePart.ListFiles.Add(new ReleaseManifestFile { RelativePath="coreclr.dll",Size=7340032,Sha256=new string('1',64) });
			releaseRuntimePart.ListFiles.Add(new ReleaseManifestFile { RelativePath="Helper32\\coreclr.dll",Size=5242880,Sha256=new string('2',64) });
			releaseRuntimePart.Key=releaseRuntimePart.ComputeKey();
			return releaseRuntimePart;
		}

		private static ReleaseManifest MakeNet10Manifest() {
			ReleaseManifest releaseManifest=Published("1.0",new string('4',64),_dateTimeNew,ReleaseRuntime.Parse("Microsoft.NETCore.App 10.0.14 win-x64"));
			releaseManifest.CompatVersion="24.3.63.0";
			releaseManifest.PublishedUtc=_dateTimeNew;
			releaseManifest.PublishedBy="UPDATESERVER";
			releaseManifest.ArchiveSha256=new string('5',64);
			releaseManifest.RuntimePart=MakeRuntimePart("10.0.14");
			releaseManifest.ListFiles.Add(new ReleaseManifestFile { RelativePath="OpenPmsDental.exe",Size=155648,Sha256=new string('8',64) });
			return releaseManifest;
		}

		#region The manifest lines
		[TestMethod]
		public void Net10ExampleManifest_IsReadable_WithItsRuntimeAndRuntimePart() {
			string text=ReadNet10Example();
			ReleaseManifest releaseManifest=ReleaseManifest.Parse(text);
			Assert.IsTrue(releaseManifest.IsOurProduct());
			Assert.AreEqual(ReleaseRuntime.FRAMEWORK_NET_CORE_APP,releaseManifest.Runtime.Framework);
			Assert.AreEqual("10.0.14",releaseManifest.Runtime.Version);
			Assert.AreEqual("win-x64",releaseManifest.Runtime.Architecture);
			Assert.AreEqual("10.0.14",releaseManifest.RuntimePart.Version);
			CollectionAssert.AreEqual(new[] { "win-x64","win-x86" },releaseManifest.RuntimePart.ListArchitectures);
			Assert.AreEqual(new string('6',64),releaseManifest.RuntimePart.ArchiveSha256);
			Assert.AreEqual(releaseManifest.RuntimePart.ComputeKey(),releaseManifest.RuntimePart.Key);
			CollectionAssert.AreEqual(new[] { "coreclr.dll","Helper32\\coreclr.dll","System.Private.CoreLib.dll" },
				releaseManifest.RuntimePart.ListFiles.Select(x => x.RelativePath).ToArray());
			Assert.AreEqual(4,releaseManifest.ListFiles.Count,"The runtime files are not program files.");
			Assert.AreEqual(text,releaseManifest.Serialize(),"The example is exactly what Serialize writes.");
		}

		[TestMethod]
		public void RuntimeLines_RoundTrip_AndAreInTheArchiveCopyToo() {
			ReleaseManifest releaseManifest=MakeNet10Manifest();
			ReleaseManifest releaseManifestRead=ReleaseManifest.Parse(releaseManifest.Serialize());
			Assert.AreEqual("Microsoft.NETCore.App 10.0.14 win-x64",releaseManifestRead.Runtime.ToString());
			Assert.AreEqual(releaseManifest.RuntimePart.Key,releaseManifestRead.RuntimePart.Key);
			Assert.AreEqual(releaseManifest.Serialize(),releaseManifestRead.Serialize());
			string textInside=releaseManifest.Serialize(includeArchiveHash:false);
			StringAssert.Contains(textInside,"\nRuntime=Microsoft.NETCore.App 10.0.14 win-x64\n");
			StringAssert.Contains(textInside,"\nRuntimeArchiveSha256="+new string('6',64)+"\n","The runtime part's archive hash is not this archive's.");
			Assert.IsFalse(textInside.Contains("\nArchiveSha256="));
		}

		[TestMethod]
		public void ManifestWithoutRuntimeLine_IsTheNetFramework48X86Build_AndWritesNoRuntimeLine() {
			ReleaseManifest releaseManifest=Published("0.0",new string('a',64),_dateTimeOld);
			string text=releaseManifest.Serialize();
			Assert.IsFalse(text.Contains("Runtime"),"A bridge release writes no runtime line (3b's release writes it).");
			ReleaseManifest releaseManifestRead=ReleaseManifest.Parse(text);
			Assert.IsNull(releaseManifestRead.Runtime);
			Assert.IsNull(releaseManifestRead.RuntimePart);
			Assert.AreEqual(".NETFramework 4.8 win-x86",releaseManifestRead.GetRuntimeOrDefault().ToString());
		}

		[TestMethod]
		public void RuntimeLineAlone_IsReadable() {
			ReleaseManifest releaseManifest=Published("0.0",new string('a',64),_dateTimeOld,ReleaseRuntime.Parse(".NETFramework 4.8 win-x86"));
			ReleaseManifest releaseManifestRead=ReleaseManifest.Parse(releaseManifest.Serialize());
			Assert.AreEqual(".NETFramework 4.8 win-x86",releaseManifestRead.Runtime.ToString());
			Assert.IsNull(releaseManifestRead.RuntimePart);
		}

		[TestMethod]
		public void RuntimeValue_Malformed_Throws() {
			foreach(string value in new[] { "","Microsoft.NETCore.App","Microsoft.NETCore.App 10.0.14","Microsoft.NETCore.App 10.0.14 win-x64 extra",
				"Microsoft.NETCore.App  10.0.14 win-x64","Microsoft.NETCore.App ten win-x64","Microsoft.NETCore.App 10 win-x64",
				"Microsoft.NETCore.App 10.0.014 win-x64","Microsoft/NETCore 10.0.14 win-x64","Microsoft.NETCore.App 10.0.14 win|x64" })
			{
				Assert.ThrowsExactly<FormatException>(() => ReleaseRuntime.Parse(value),"'"+value+"'");
			}
		}

		///<summary>Every way a runtime part can be malformed makes the manifest unreadable, which the bridge refuses instead of reading half of it.
		///</summary>
		[TestMethod]
		public void RuntimePart_Malformed_Throws() {
			string text=ReadNet10Example();
			string keyLine=text.Split('\n').Single(x => x.StartsWith("RuntimeKey="));
			Dictionary<string,string> dictCases=new Dictionary<string,string>() {
				{ "no Runtime line",text.Replace("Runtime=Microsoft.NETCore.App 10.0.14 win-x64\n","") },
				{ "no RuntimeVersion",text.Replace("RuntimeVersion=10.0.14\n","") },
				{ "no RuntimeArchitectures",text.Replace("RuntimeArchitectures=win-x64 win-x86\n","") },
				{ "no RuntimeArchiveSha256",text.Replace("RuntimeArchiveSha256="+new string('6',64)+"\n","") },
				{ "no RuntimeKey",text.Replace(keyLine+"\n","") },
				{ "no RuntimeFile",string.Join("\n",text.Split('\n').Where(x => !x.StartsWith("RuntimeFile="))) },
				{ "version differs from the Runtime line",text.Replace("RuntimeVersion=10.0.14","RuntimeVersion=10.0.15") },
				{ "first architecture differs from the Runtime line",text.Replace("RuntimeArchitectures=win-x64 win-x86","RuntimeArchitectures=win-x86 win-x64") },
				{ "an architecture twice",text.Replace("RuntimeArchitectures=win-x64 win-x86","RuntimeArchitectures=win-x64 win-x64") },
				{ "a bad architecture",text.Replace("RuntimeArchitectures=win-x64 win-x86","RuntimeArchitectures=win-x64 win/x86") },
				{ "a bad archive hash",text.Replace("RuntimeArchiveSha256="+new string('6',64),"RuntimeArchiveSha256=66") },
				{ "a bad key",text.Replace(keyLine,"RuntimeKey=zz") },
				{ "a key that does not match the files",text.Replace(keyLine,"RuntimeKey="+new string('0',64)) },
				{ "a runtime file changed under the key",text.Replace("|7340032|coreclr.dll","|7340033|coreclr.dll") },
				{ "a runtime file twice",text.Replace("|5242880|Helper32\\coreclr.dll","|5242880|coreclr.dll") },
				{ "a runtime file that is a program file too",text.Replace("|15728640|System.Private.CoreLib.dll","|15728640|OpenPmsDental.dll") },
				{ "an unsafe runtime path",text.Replace("|5242880|Helper32\\coreclr.dll","|5242880|..\\coreclr.dll") },
				{ "a key line twice",text.Replace("RuntimeVersion=10.0.14\n","RuntimeVersion=10.0.14\nRuntimeVersion=10.0.14\n") },
				{ "the Runtime line twice",text.Replace("Runtime=Microsoft.NETCore.App 10.0.14 win-x64\n",
					"Runtime=Microsoft.NETCore.App 10.0.14 win-x64\nRuntime=Microsoft.NETCore.App 10.0.14 win-x64\n") },
			};
			foreach(KeyValuePair<string,string> pair in dictCases) {
				Assert.AreNotEqual(text,pair.Value,"The case does not change the text: "+pair.Key);
				Assert.ThrowsExactly<FormatException>(() => ReleaseManifest.Parse(pair.Value),pair.Key);
			}
		}

		[TestMethod]
		public void RuntimeKey_CoversVersionNamesSizesAndHashes_NotTheOrderOrCase() {
			ReleaseRuntimePart releaseRuntimePart=MakeRuntimePart("10.0.14");
			string key=releaseRuntimePart.ComputeKey();
			ReleaseRuntimePart releaseRuntimePartReordered=MakeRuntimePart("10.0.14");
			releaseRuntimePartReordered.ListFiles.Reverse();
			releaseRuntimePartReordered.ListFiles[0].RelativePath=releaseRuntimePartReordered.ListFiles[0].RelativePath.ToUpperInvariant();
			Assert.AreEqual(key,releaseRuntimePartReordered.ComputeKey());
			Assert.AreNotEqual(key,MakeRuntimePart("10.0.15").ComputeKey());
			ReleaseRuntimePart releaseRuntimePartChanged=MakeRuntimePart("10.0.14");
			releaseRuntimePartChanged.ListFiles[1].Size++;
			Assert.AreNotEqual(key,releaseRuntimePartChanged.ComputeKey());
			releaseRuntimePartChanged=MakeRuntimePart("10.0.14");
			releaseRuntimePartChanged.ListFiles[1].Sha256=new string('9',64);
			Assert.AreNotEqual(key,releaseRuntimePartChanged.ComputeKey());
			releaseRuntimePartChanged=MakeRuntimePart("10.0.14");
			releaseRuntimePartChanged.ListFiles[1].RelativePath="Helper32\\clrjit.dll";
			Assert.AreNotEqual(key,releaseRuntimePartChanged.ComputeKey());
		}

		[TestMethod]
		public void RunningRuntime_OfTheTestProcess_IsTheNetFramework() {
			ReleaseRuntime releaseRuntime=ReleaseRuntime.GetRunning();
			Assert.AreEqual(ReleaseRuntime.FRAMEWORK_NET_FRAMEWORK,releaseRuntime.Framework,"These tests run on .NET Framework 4.8 until stage 3b.");
			Assert.AreEqual("4.8",releaseRuntime.Version);
			Assert.AreEqual(Environment.Is64BitProcess ? "win-x64" : "win-x86",releaseRuntime.Architecture);
		}

		[TestMethod]
		public void SameFrameworkAndArchitecture_IgnoresTheVersion() {
			ReleaseRuntime releaseRuntime=ReleaseRuntime.Parse("Microsoft.NETCore.App 10.0.14 win-x64");
			Assert.IsTrue(releaseRuntime.IsSameFrameworkAndArchitecture(ReleaseRuntime.Parse("Microsoft.NETCore.App 10.0.15 win-x64")),
				"A newer runtime of the same framework is the runtime part of a normal self-update (S3b-7).");
			Assert.IsTrue(releaseRuntime.IsSameFrameworkAndArchitecture(ReleaseRuntime.Parse("microsoft.netcore.app 10.0.14 WIN-X64")));
			Assert.IsFalse(releaseRuntime.IsSameFrameworkAndArchitecture(ReleaseRuntime.Parse("Microsoft.NETCore.App 10.0.14 win-x86")));
			Assert.IsFalse(releaseRuntime.IsSameFrameworkAndArchitecture(ReleaseRuntime.NetFramework48X86));
			Assert.IsFalse(releaseRuntime.IsSameFrameworkAndArchitecture(null));
		}
		#endregion The manifest lines

		#region Refusals
		///<summary>The bridge release (.NET Framework, x86) refuses the .NET 10 release (x64, its own runtime) for every update server role, newer
		///or not, and says to run the new setup; the named update server does not offer to publish its old build over it.</summary>
		[TestMethod]
		public void Net10Release_IsRefusedWithRunTheNewSetup_ForEveryRole() {
			ReleaseManifest releaseManifest=ReleaseManifest.Parse(ReadNet10Example());
			foreach(UpdateServerRole updateServerRole in _arrayRoles) {
				foreach(ReleaseIdentity releaseIdentity in new[] { RunningBridge(),RunningBridge("2.0","newer",new DateTime(2028,1,1,0,0,0,DateTimeKind.Utc)) }) {
					ReleaseDecision releaseDecision=ForkReleases.Decide(releaseManifest,releaseIdentity,updateServerRole,false,"SERVER1");
					Assert.AreEqual(ReleaseAction.RefuseOtherRuntime,releaseDecision.Action,updateServerRole+", running "+releaseIdentity);
					StringAssert.Contains(releaseDecision.Message,"run the new setup");
					StringAssert.Contains(releaseDecision.Message,".NET 10.0.14 (win-x64)");
					StringAssert.Contains(releaseDecision.Message,".NET Framework 4.8 (win-x86)");
					Assert.AreEqual(releaseDecision.Message,ForkReleases.GetRuntimeRefusal(releaseManifest,releaseIdentity));
					Assert.AreEqual(releaseDecision.Message,ForkReleases.GetRefusal(releaseManifest,"",releaseIdentity));
				}
			}
		}

		[TestMethod]
		public void ReleaseForAnotherArchitecture_IsRefused() {
			ReleaseManifest releaseManifest=Published("0.1","x64build",_dateTimeNew,ReleaseRuntime.Parse(".NETFramework 4.8 win-x64"));
			ReleaseDecision releaseDecision=ForkReleases.Decide(releaseManifest,RunningBridge(),UpdateServerRole.OtherComputer,false);
			Assert.AreEqual(ReleaseAction.RefuseOtherRuntime,releaseDecision.Action);
			StringAssert.Contains(releaseDecision.Message,"run the new setup");
			//And the other way round: an x64 install (3b) refuses an x86 release, also one without a Runtime line.
			ReleaseIdentity releaseIdentityX64=RunningBridge();
			releaseIdentityX64.Runtime=ReleaseRuntime.Parse("Microsoft.NETCore.App 10.0.14 win-x64");
			Assert.AreEqual(ReleaseAction.RefuseOtherRuntime,
				ForkReleases.Decide(Published("2.0","old",_dateTimeNew),releaseIdentityX64,UpdateServerRole.OtherComputer,false).Action);
			Assert.AreEqual(ReleaseAction.RefuseOtherRuntime,ForkReleases.Decide(Published("2.0","old",_dateTimeNew,ReleaseRuntime.Parse(
				"Microsoft.NETCore.App 10.0.14 win-x86")),releaseIdentityX64,UpdateServerRole.ThisComputer,false).Action);
		}

		[TestMethod]
		public void ReleaseForThisRuntime_IsDecidedAsBefore() {
			//The same framework and architecture, explicit or by default: the rules of ForkReleases.Decide as before S3a-10.
			foreach(ReleaseRuntime releaseRuntime in new[] { null,ReleaseRuntime.NetFramework48X86,ReleaseRuntime.Parse(".NETFramework 4.8.1 win-x86") }) {
				Assert.AreEqual(ReleaseAction.Pull,ForkReleases.Decide(Published("0.0","new",_dateTimeNew,releaseRuntime),RunningBridge(),
					UpdateServerRole.OtherComputer,false).Action);
				Assert.AreEqual(ReleaseAction.Publish,ForkReleases.Decide(Published("0.0","other",DateTime.MinValue,releaseRuntime),RunningBridge(),
					UpdateServerRole.ThisComputer,false).Action,"The named update server replaces a build it cannot order.");
				Assert.AreEqual(ReleaseAction.None,ForkReleases.Decide(Published("0.0","bridge",_dateTimeNew,releaseRuntime),RunningBridge(),
					UpdateServerRole.OtherComputer,false).Action);
				Assert.AreEqual("",ForkReleases.GetRuntimeRefusal(Published("0.0","new",_dateTimeNew,releaseRuntime),RunningBridge()));
			}
			//The .NET 10 install of 3b pulls a newer .NET 10 release, also on a newer runtime.
			ReleaseIdentity releaseIdentityNet10=RunningBridge("1.0","net10old",_dateTimeOld);
			releaseIdentityNet10.Runtime=ReleaseRuntime.Parse("Microsoft.NETCore.App 10.0.14 win-x64");
			Assert.AreEqual(ReleaseAction.Pull,ForkReleases.Decide(Published("1.0","net10new",_dateTimeNew,ReleaseRuntime.Parse(
				"Microsoft.NETCore.App 10.0.15 win-x64")),releaseIdentityNet10,UpdateServerRole.OtherComputer,false).Action);
		}

		[TestMethod]
		public void UnknownRunningRuntime_RefusesNothingForTheRuntime() {
			ReleaseIdentity releaseIdentity=RunningBridge();
			releaseIdentity.Runtime=null;
			ReleaseManifest releaseManifest=ReleaseManifest.Parse(ReadNet10Example());
			Assert.AreEqual("",ForkReleases.GetRuntimeRefusal(releaseManifest,releaseIdentity));
			Assert.AreEqual(ReleaseAction.Pull,ForkReleases.Decide(releaseManifest,releaseIdentity,UpdateServerRole.OtherComputer,false).Action);
		}

		[TestMethod]
		public void RunningIdentity_CarriesTheRuntimeOfThisProcess() {
			ReleaseIdentity releaseIdentity=ForkReleases.GetRunningIdentity();
			Assert.IsNotNull(releaseIdentity.Runtime);
			Assert.AreEqual(ReleaseRuntime.GetRunning().ToString(),releaseIdentity.Runtime.ToString());
		}

		///<summary>A manifest that is not readable or names another product is not "nothing published": refused for every role, never published
		///over, never pulled.</summary>
		[TestMethod]
		public void UnreadableManifest_IsRefused_ForEveryRole_NotPublishedOver() {
			foreach(string error in new[] { "The published release manifest is not readable: Unknown release manifest format: 2",
				"The published release manifest is for another product (Some Other Program)." })
			{
				foreach(UpdateServerRole updateServerRole in _arrayRoles) {
					ReleaseDecision releaseDecision=ForkReleases.Decide(null,error,RunningBridge(),updateServerRole,false,"SERVER1");
					Assert.AreEqual(ReleaseAction.RefuseUnreadable,releaseDecision.Action,updateServerRole.ToString());
					Assert.IsTrue(releaseDecision.Message.StartsWith(error),releaseDecision.Message);
					StringAssert.Contains(releaseDecision.Message,"neither updates from it nor publishes over it");
					StringAssert.Contains(releaseDecision.Message,"run the new setup");
					Assert.AreEqual(releaseDecision.Message,ForkReleases.GetRefusal(null,error,RunningBridge()));
				}
				Assert.AreEqual(ReleaseAction.None,ForkReleases.Decide(null,error,RunningBridge(),UpdateServerRole.ThisComputer,true).Action,
					"Debug builds neither publish nor pull, as before.");
			}
		}

		[TestMethod]
		public void NothingPublished_StillOffersToPublish() {
			Assert.AreEqual(ReleaseAction.Publish,ForkReleases.Decide(null,"",RunningBridge(),UpdateServerRole.ThisComputer,false).Action);
			Assert.AreEqual(ReleaseAction.Publish,ForkReleases.Decide(null,null,RunningBridge(),UpdateServerRole.AnyWorkstation,false).Action);
			Assert.AreEqual(ReleaseAction.None,ForkReleases.Decide(null,"",RunningBridge(),UpdateServerRole.OtherComputer,false).Action);
			Assert.AreEqual("",ForkReleases.GetRefusal(null,"",RunningBridge()));
		}

		///<summary>IsDbConnectionSafe restarts a workstation when another build is published, unless it may keep running: a release it cannot
		///update to (another runtime) does not restart it in a loop.</summary>
		[TestMethod]
		public void MayRunUnpublishedBuild_WhenTheReleaseIsForAnotherRuntime() {
			ReleaseManifest releaseManifest=ReleaseManifest.Parse(ReadNet10Example());
			foreach(UpdateServerRole updateServerRole in _arrayRoles) {
				Assert.IsTrue(ForkReleases.MayRunUnpublishedBuild(releaseManifest,RunningBridge(),updateServerRole),updateServerRole.ToString());
			}
			Assert.IsFalse(ForkReleases.MayRunUnpublishedBuild(Published("0.0","new",_dateTimeNew),RunningBridge(),UpdateServerRole.OtherComputer),
				"A newer bridge build still restarts the workstation.");
		}

		private static ReleaseIdentity RunningNet10(string release="1.0",string buildId="net10",DateTime? dateTimeBuild=null) {
			ReleaseIdentity releaseIdentity=RunningBridge(release,buildId,dateTimeBuild??_dateTimeNew);
			releaseIdentity.Runtime=ReleaseRuntime.Parse("Microsoft.NETCore.App 10.0.14 win-x64");
			return releaseIdentity;
		}

		///<summary>Plan section 10 step 3: the update server gets the .NET 10 setup first, and its first start publishes the .NET 10 release over
		///the bridge release (a manifest without a Runtime line). Decide, Recopy's GetRefusal and MayRunUnpublishedBuild all let it publish.</summary>
		[TestMethod]
		public void Net10UpdateServer_PublishesOverTheBridgeRelease() {
			ReleaseIdentity releaseIdentity=RunningNet10();
			foreach(ReleaseManifest releaseManifest in new[] { Published("0.0","bridge",_dateTimeOld),Published("0.0","bridge",DateTime.MinValue),
				Published("0.0","bridge",_dateTimeOld,ReleaseRuntime.NetFramework48X86),Published("1.0","bridge",DateTime.MinValue) })
			{
				Assert.IsTrue(ForkReleases.IsForwardRuntimeSwitch(releaseManifest,releaseIdentity));
				ReleaseDecision releaseDecision=ForkReleases.Decide(releaseManifest,releaseIdentity,UpdateServerRole.ThisComputer,false,"SERVER1");
				Assert.AreEqual(ReleaseAction.Publish,releaseDecision.Action,releaseManifest.ReleaseVersion+" "+releaseManifest.BuildUtc);
				StringAssert.Contains(releaseDecision.Message,"This build is not the published one");
				StringAssert.Contains(releaseDecision.Message,"keep running it until the new setup runs on them");
				Assert.IsFalse(releaseDecision.Message.Contains("run the new setup on it"),releaseDecision.Message);
				Assert.AreEqual("",ForkReleases.GetRefusal(releaseManifest,"",releaseIdentity,UpdateServerRole.ThisComputer),"Recopy publishes.");
				Assert.IsTrue(ForkReleases.MayRunUnpublishedBuild(releaseManifest,releaseIdentity,UpdateServerRole.ThisComputer));
				//The version guard's pull: a .NET 10 install never pulls a .NET Framework release.
				Assert.AreNotEqual("",ForkReleases.GetRefusal(releaseManifest,"",releaseIdentity));
			}
			//Blank WebServiceServerName: a build known to be newer publishes, as for one runtime.
			ReleaseManifest releaseManifestOlder=Published("0.0","bridge",_dateTimeOld);
			Assert.AreEqual(ReleaseAction.Publish,ForkReleases.Decide(releaseManifestOlder,releaseIdentity,UpdateServerRole.AnyWorkstation,false).Action);
			Assert.AreEqual("",ForkReleases.GetRefusal(releaseManifestOlder,"",releaseIdentity,UpdateServerRole.AnyWorkstation));
		}

		///<summary>Every forward case the role rules would not publish is RefuseOtherRuntime: a workstation that is not the update server, a blank
		///WebServiceServerName without a known order, and a published .NET Framework release that is newer than the .NET 10 build (never a downgrade).
		///None of them pulls.</summary>
		[TestMethod]
		public void Net10Workstation_NotTheUpdateServer_RefusesTheBridgeRelease() {
			ReleaseIdentity releaseIdentity=RunningNet10();
			ReleaseManifest releaseManifest=Published("0.0","bridge",_dateTimeOld);
			ReleaseDecision releaseDecision=ForkReleases.Decide(releaseManifest,releaseIdentity,UpdateServerRole.OtherComputer,false,"SERVER1");
			Assert.AreEqual(ReleaseAction.RefuseOtherRuntime,releaseDecision.Action);
			StringAssert.Contains(releaseDecision.Message,"does not go back to that release");
			StringAssert.Contains(releaseDecision.Message,"run the new setup on the update server");
			StringAssert.Contains(releaseDecision.Message,".NET Framework 4.8 (win-x86)");
			StringAssert.Contains(releaseDecision.Message,".NET 10.0.14 (win-x64)");
			Assert.AreEqual(releaseDecision.Message,ForkReleases.GetRuntimeRefusal(releaseManifest,releaseIdentity));
			Assert.AreEqual(releaseDecision.Message,ForkReleases.GetRefusal(releaseManifest,"",releaseIdentity));
			Assert.AreEqual(releaseDecision.Message,ForkReleases.GetRefusal(releaseManifest,"",releaseIdentity,UpdateServerRole.OtherComputer));
			Assert.IsTrue(ForkReleases.MayRunUnpublishedBuild(releaseManifest,releaseIdentity,UpdateServerRole.OtherComputer),
				"No restart loop while the bridge release is still published.");
			//Blank WebServiceServerName, same release and no build order: the rules would pull, so refused.
			ReleaseManifest releaseManifestUnordered=Published("1.0","bridge",DateTime.MinValue);
			Assert.AreEqual(ReleaseAction.RefuseOtherRuntime,
				ForkReleases.Decide(releaseManifestUnordered,releaseIdentity,UpdateServerRole.AnyWorkstation,false).Action);
			Assert.AreNotEqual("",ForkReleases.GetRefusal(releaseManifestUnordered,"",releaseIdentity,UpdateServerRole.AnyWorkstation));
			//A newer .NET Framework release: neither the update server nor any workstation publishes over it, and none pulls it.
			ReleaseManifest releaseManifestNewer=Published("2.0","bridgenewer",_dateTimeOld);
			foreach(UpdateServerRole updateServerRole in _arrayRoles) {
				ReleaseDecision releaseDecisionNewer=ForkReleases.Decide(releaseManifestNewer,releaseIdentity,updateServerRole,false);
				Assert.AreEqual(ReleaseAction.RefuseOtherRuntime,releaseDecisionNewer.Action,updateServerRole.ToString());
				Assert.AreEqual(releaseDecisionNewer.Message,ForkReleases.GetRefusal(releaseManifestNewer,"",releaseIdentity,updateServerRole));
			}
			StringAssert.Contains(ForkReleases.Decide(releaseManifestNewer,releaseIdentity,UpdateServerRole.ThisComputer,false).Message,
				"This computer is the update server, but the published release is newer than this build");
		}

		///<summary>The exception is one way only: the bridge release never publishes over a .NET release, with or without a role for Recopy, and a
		///.NET 10 build never publishes over a .NET release for another architecture.</summary>
		[TestMethod]
		public void ForwardSwitchOnly_NeverBackToTheNetFramework() {
			ReleaseManifest releaseManifestNet10=ReleaseManifest.Parse(ReadNet10Example());
			Assert.IsFalse(ForkReleases.IsForwardRuntimeSwitch(releaseManifestNet10,RunningBridge()));
			foreach(UpdateServerRole updateServerRole in _arrayRoles) {
				Assert.AreEqual(ReleaseAction.RefuseOtherRuntime,ForkReleases.Decide(releaseManifestNet10,RunningBridge(),updateServerRole,false).Action);
				StringAssert.Contains(ForkReleases.GetRefusal(releaseManifestNet10,"",RunningBridge(),updateServerRole),"run the new setup on it");
			}
			ReleaseManifest releaseManifestX86=Published("0.0","net10x86",_dateTimeOld,ReleaseRuntime.Parse("Microsoft.NETCore.App 10.0.14 win-x86"));
			Assert.IsFalse(ForkReleases.IsForwardRuntimeSwitch(releaseManifestX86,RunningNet10()));
			Assert.AreEqual(ReleaseAction.RefuseOtherRuntime,
				ForkReleases.Decide(releaseManifestX86,RunningNet10(),UpdateServerRole.ThisComputer,false).Action);
			Assert.AreNotEqual("",ForkReleases.GetRefusal(releaseManifestX86,"",RunningNet10(),UpdateServerRole.ThisComputer));
			ReleaseIdentity releaseIdentityUnknown=RunningNet10();
			releaseIdentityUnknown.Runtime=null;
			Assert.IsFalse(ForkReleases.IsForwardRuntimeSwitch(Published("0.0","bridge",_dateTimeOld),releaseIdentityUnknown));
		}

		///<summary>What the .NET 10 update server publishes names its runtime, so the bridge workstations refuse it instead of pulling .NET 10 files
		///into their folder; a bridge release still writes no Runtime line.</summary>
		[TestMethod]
		public void BuildRelease_NamesTheRuntime_OfAnyBuildButTheBridge() {
			string folder=TestPaths.NewTempFolder("runtime_line");
			System.IO.File.WriteAllText(System.IO.Path.Combine(folder,"OpenPmsDental.exe"),"synthetic");
			DateTime dateTimeUtc=new DateTime(2027,2,1,8,0,0,DateTimeKind.Utc);
			ReleaseManifest releaseManifestNet10=ForkReleases.BuildRelease(folder,RunningNet10(),"SERVER1",dateTimeUtc,out byte[] _);
			Assert.AreEqual("Microsoft.NETCore.App 10.0.14 win-x64",releaseManifestNet10.Runtime.ToString());
			ReleaseManifest releaseManifestRead=ReleaseManifest.Parse(releaseManifestNet10.Serialize());
			Assert.AreEqual(ReleaseAction.RefuseOtherRuntime,ForkReleases.Decide(releaseManifestRead,RunningBridge(),UpdateServerRole.OtherComputer,false).Action);
			ReleaseManifest releaseManifestBridge=ForkReleases.BuildRelease(folder,RunningBridge(),"SERVER1",dateTimeUtc,out byte[] _);
			Assert.IsNull(releaseManifestBridge.Runtime);
			Assert.IsFalse(releaseManifestBridge.Serialize().Contains("Runtime"));
		}

		[TestMethod]
		public void DescribeRuntime_NamesTheFrameworkForPeople() {
			Assert.AreEqual(".NET Framework 4.8 (win-x86)",ForkReleases.DescribeRuntime(ReleaseRuntime.NetFramework48X86));
			Assert.AreEqual(".NET 10.0.14 (win-x64)",ForkReleases.DescribeRuntime(ReleaseRuntime.Parse("Microsoft.NETCore.App 10.0.14 win-x64")));
			Assert.AreEqual("Mono 6.12 (linux-x64)",ForkReleases.DescribeRuntime(ReleaseRuntime.Parse("Mono 6.12 linux-x64")));
		}
		#endregion Refusals
	}
}
