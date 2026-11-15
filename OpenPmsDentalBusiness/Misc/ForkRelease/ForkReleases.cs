// Publishing a fork release to the database and pulling it on the other workstations (decoupling plan Phase 4, backlog Risk R1).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using CodeBase;

namespace OpenPmsDentalBusiness {
	///<summary>Which build is running or was published: the release version ("0.0"), the build id and the build time.</summary>
	[Serializable]
	public class ReleaseIdentity {
		public string ReleaseVersion="";
		public string BuildId="";
		///<summary>When the build was compiled (UTC), from the ForkBuildUtc assembly metadata that OpenPmsDental.csproj writes. Orders two builds of the
		///same release version. DateTime.MinValue when unknown.</summary>
		public DateTime BuildUtc=DateTime.MinValue;
		///<summary>The runtime the build runs on (ReleaseRuntime.GetRunning for the running program). Null when unknown, as for an identity read from
		///the ForkProgramVersion marker: then no release is refused for its runtime (ForkReleases.GetRuntimeRefusal).</summary>
		public ReleaseRuntime Runtime;

		public override string ToString() {
			return ReleaseVersion+" (build "+(BuildId.Length>12?BuildId.Substring(0,12):BuildId)
				+(BuildUtc==DateTime.MinValue?"":", built "+BuildUtc.ToString("yyyy-MM-dd HH:mm",CultureInfo.InvariantCulture)+" UTC")+")";
		}
	}

	///<summary>How this workstation relates to the update server, from the WebServiceServerName preference.</summary>
	public enum UpdateServerRole {
		///<summary>WebServiceServerName names another computer. This workstation never publishes.</summary>
		OtherComputer,
		///<summary>WebServiceServerName is blank (the default), so any workstation may act as the update server. A workstation may publish when
		///nothing is published yet or when its build is known to be newer than the published one; it never replaces a published build that is newer or
		///whose order is unknown.</summary>
		AnyWorkstation,
		///<summary>WebServiceServerName names this computer: the update server chosen by the office. It may replace the published build.</summary>
		ThisComputer,
	}

	public enum ReleaseAction {
		///<summary>Nothing to do: this workstation runs the published build, or nothing applies.</summary>
		None,
		///<summary>Offer to store this workstation's binaries as the published release: nothing is published yet, this is the named update server, or
		///(blank WebServiceServerName) this build is known to be newer than the published one.</summary>
		Publish,
		///<summary>A different build is published that is newer, or whose order is unknown and that this workstation may not replace: update this
		///workstation from it.</summary>
		Pull,
		///<summary>This workstation runs a newer release or build than the one published and may not publish it. Refused; no downgrade is offered.
		///</summary>
		RefuseNewerThanPublished,
		///<summary>The published manifest is not readable or names another product. This workstation neither publishes over it nor pulls; it keeps
		///running its build and says so. A newer program may have published it (S3a-10 of docs/specs/net10-plan.md).</summary>
		RefuseUnreadable,
		///<summary>The published release is for another runtime or architecture than this installation (the .NET 10 release is x64 and carries its
		///own runtime; decisions 19 and 20 of docs/specs/net10-plan.md). This workstation neither publishes over it nor pulls; it keeps running its
		///build and says to run the new setup.</summary>
		RefuseOtherRuntime,
	}

	public class ReleaseDecision {
		public ReleaseAction Action;
		public string Message="";
	}

	///<summary>What Recopy did with a published manifest that is not readable (ForkReleases.ReplaceUnreadableRelease, S3a-10f).</summary>
	public enum ReleaseReplaceOutcome {
		///<summary>No offer: the published manifest is readable or none is published, this computer is not the named update server, or the run is
		///silent. Nothing was asked and nothing changed; Recopy goes on with its usual rules (and refuses the unreadable manifest).</summary>
		NotOffered,
		///<summary>The person answered No. Nothing changed.</summary>
		Declined,
		///<summary>The published manifest changed while the question was shown (another computer published, or the row changed). Nothing was
		///replaced.</summary>
		Changed,
		///<summary>The person answered Yes, but publishing did not finish (the caller showed why). The previous rows stay.</summary>
		Failed,
		///<summary>The rows of types ForkReleaseManifest and ForkReleaseSegment were replaced by this build, published as for a first release.</summary>
		Replaced,
	}

	public class ReleaseReplaceResult {
		public ReleaseReplaceOutcome Outcome;
		///<summary>The manifest now published; null unless Replaced.</summary>
		public ReleaseManifest Manifest;
		///<summary>Why nothing was replaced, for Changed; "" otherwise.</summary>
		public string Message="";
	}

	public class ReleaseDownloadResult {
		public bool IsOk;
		public ReleaseManifest Manifest;
		public List<string> ListProblems=new List<string>();
	}

	///<summary>The fork's own release channel. The update server stores its binaries in documentmisc rows of the fork-only types
	///ForkReleaseManifest and ForkReleaseSegment, with a manifest that names our product and the SHA-256 of the archive and of every file. Other
	///workstations pull that release on startup through UpdateFileCopier. Open Dental's UpdateFiles rows are never read, and there is never a
	///downgrade prompt. See docs/release-process.md.</summary>
	public class ForkReleases {
		///<summary>Bytes of archive per segment row. 786,432 bytes is 1MB of base64, well under the default max_allowed_packet.</summary>
		public const int SEGMENT_BYTES=786432;
		///<summary>Sub folders of the installation folder that are published too (not recursive), the same ones UpdateFileCopier copies.</summary>
		public static readonly List<string> ListSpecialDirs=new List<string>() {
			"OpenDentalReplicationService",
		};
		///<summary>The program's own assemblies. The build id is the SHA-256 over their names and SHA-256 values, so any rebuilt assembly gives a new
		///build id while third-party files are not hashed at every startup. ComputeBuildId skips a missing file silently, so the program-named
		///files come from ProgramNames (the one place a rename changes them), and ReleaseFlowTests checks that every file is in the build output.
		///</summary>
		public static readonly List<string> ListBuildIdFiles=new List<string>() {
			ProgramNames.ExeFileName,
			ProgramNames.BusinessDllFileName,
			"CodeBase.dll",
			"DataConnectionBase.dll",
			"WpfControlsOD.dll",
			ProgramNames.GraphDllFileName,
			ProgramNames.StorageDllFileName,
			"SparksToothChart.dll",
			"ODCrypt.dll",
			"PasswordVaultWrapper.dll",
			"ImagingDeviceManager.dll",
			"ImagingCapture.dll",
			ProgramNames.UpdateFileCopierExeFileName,
		};
		///<summary>Files that an earlier release put in the installation folder and this one no longer ships. UpdateFileCopier copies a release over
		///the installation folder and never deletes a file, so a workstation updated from such a release keeps them; no assembly references them,
		///so nothing loads them. They are never published again (ShouldPublishFile), so a workstation that still has one cannot pass it on. Setup
		///deletes each one on an upgrade ([InstallDelete] in installer\OpenPmsDental.iss) and Build-Release.ps1 refuses to stage one; ReleaseFlowTests
		///checks both lists against this one.</summary>
		public static readonly List<string> ListRetiredFiles=new List<string>() {
			ProgramNames.LegacyCloudDllFileName,//The storage library of v0.0, renamed on 2026-11-02.
			//Libraries of v0.0 that the tier 1 library updates removed (2026-11-02).
			"Tamir.SharpSSH.dll",//SharpSSH, replaced by SSH.NET.
			"Org.Mentalis.Security.dll",//SharpSSH's cryptography library.
			"DiffieHellman.dll",//SharpSSH's key exchange library.
			"OpenPop.dll",//OpenPop.NET, replaced by MailKit's POP3 client.
			"Dicom.Core.dll",//fo-dicom 4, replaced by the in-house DICOM reader (2026-11-02).
			"Dicom.Native.dll",//fo-dicom 4's native codecs.
			"NDde.dll",//NDde (its licence does not fit the GPL), removed on 2026-11-07; the DentX, Dexis Integrator and Vipersoft links use CodeBase.ODDdeClient (2026-11-08).
			//PDFsharp and MigraDoc 1.2, which 6.5 replaces under other file names (2026-11-02).
			"PdfSharp.dll",
			"PdfSharp.Charting.dll",
			"MigraDoc.Rendering.dll",
			//NHunspell and its native Hunspell, replaced by the managed WeCantSpell.Hunspell (2026-11-02).
			"NHunspell.dll",
			"Hunspellx86.dll",
		};
		///<summary>Key of the AssemblyMetadata attribute that holds the build time of OpenPmsDental.exe (written by OpenPmsDental.csproj).</summary>
		public const string BUILD_UTC_METADATA_KEY="ForkBuildUtc";
		private static ReleaseIdentity _releaseIdentityRunning;

		#region Identity
		///<summary>The build id of the files in the folder (see ListBuildIdFiles). Lower-case hex.</summary>
		public static string ComputeBuildId(string folder) {
			StringBuilder stringBuilder=new StringBuilder();
			foreach(string fileName in ListBuildIdFiles.OrderBy(x => x,StringComparer.OrdinalIgnoreCase)) {
				string path=Path.Combine(folder,fileName);
				if(!File.Exists(path)) {
					continue;
				}
				stringBuilder.Append(fileName.ToLowerInvariant()+"|"+ReleaseManifest.ComputeSha256File(path)+"\n");
			}
			return ReleaseManifest.ComputeSha256(Encoding.UTF8.GetBytes(stringBuilder.ToString()));
		}

		///<summary>The running program's identity: release version and build time from OpenPmsDental.exe, and build id of the installation folder.
		///Computed once.</summary>
		public static ReleaseIdentity GetRunningIdentity() {
			if(_releaseIdentityRunning==null) {
				ReleaseIdentity releaseIdentity=new ReleaseIdentity();
				releaseIdentity.ReleaseVersion=ForkVersions.GetReleaseVersion();
				releaseIdentity.BuildId=ComputeBuildId(AppDomain.CurrentDomain.BaseDirectory);
				releaseIdentity.BuildUtc=GetBuildUtc();
				releaseIdentity.Runtime=ReleaseRuntime.GetRunning();
				_releaseIdentityRunning=releaseIdentity;
			}
			return _releaseIdentityRunning;
		}

		///<summary>The build time (UTC) that OpenPmsDental.csproj stamps into OpenPmsDental.exe as [assembly: AssemblyMetadata("ForkBuildUtc",...)] each time
		///the program is compiled. Reads the given assembly, else the entry assembly. DateTime.MinValue when it has no readable stamp.</summary>
		public static DateTime GetBuildUtc(Assembly assembly=null) {
			assembly=assembly??Assembly.GetEntryAssembly();
			if(assembly==null) {
				return DateTime.MinValue;
			}
			AssemblyMetadataAttribute assemblyMetadataAttribute=assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
				.FirstOrDefault(x => x.Key==BUILD_UTC_METADATA_KEY);
			return ParseBuildUtc(assemblyMetadataAttribute?.Value);
		}

		///<summary>Parses a build time in the form yyyy-MM-ddTHH:mm:ssZ. DateTime.MinValue when blank or malformed.</summary>
		public static DateTime ParseBuildUtc(string value) {
			DateTime dateTime;
			if(string.IsNullOrWhiteSpace(value) || !DateTime.TryParseExact(value.Trim(),"yyyy-MM-dd'T'HH:mm:ss'Z'",CultureInfo.InvariantCulture,
				DateTimeStyles.AdjustToUniversal|DateTimeStyles.AssumeUniversal,out dateTime))
			{
				return DateTime.MinValue;
			}
			return dateTime;
		}

		///<summary>The ForkProgramVersion marker value for a release: "releaseVersion+buildId".</summary>
		public static string FormatProgramVersion(ReleaseIdentity releaseIdentity) {
			return releaseIdentity.ReleaseVersion+"+"+releaseIdentity.BuildId;
		}

		///<summary>Parses a ForkProgramVersion marker value. Returns null when blank or malformed.</summary>
		public static ReleaseIdentity ParseProgramVersion(string value) {
			if(string.IsNullOrWhiteSpace(value)) {
				return null;
			}
			int indexPlus=value.IndexOf('+');
			if(indexPlus<=0 || indexPlus==value.Length-1) {
				return null;
			}
			ReleaseIdentity releaseIdentity=new ReleaseIdentity();
			releaseIdentity.ReleaseVersion=value.Substring(0,indexPlus);
			releaseIdentity.BuildId=value.Substring(indexPlus+1);
			return releaseIdentity;
		}
		#endregion Identity

		#region Decision
		///<summary>The role of this workstation from the WebServiceServerName preference: blank means any workstation, otherwise the named computer
		///is the update server and every other computer is not.</summary>
		public static UpdateServerRole GetUpdateServerRole(string updateServerName,Func<string,bool> funcIsThisComputer) {
			if(string.IsNullOrWhiteSpace(updateServerName)) {
				return UpdateServerRole.AnyWorkstation;
			}
			return funcIsThisComputer(updateServerName.Trim()) ? UpdateServerRole.ThisComputer : UpdateServerRole.OtherComputer;
		}

		///<summary>Orders the published build and the running build. Greater than 0 when the published one is newer, less than 0 when the running one
		///is newer, null when the order is unknown. A higher release version is newer. Within one release version (it is pinned at "0.0" for now), the
		///build times decide; if either is missing or they are equal, the order is unknown.</summary>
		public static int? CompareBuilds(ReleaseManifest releaseManifestPublished,ReleaseIdentity releaseIdentityRunning) {
			int compare=ForkVersions.CompareReleaseVersions(releaseManifestPublished.ReleaseVersion,releaseIdentityRunning.ReleaseVersion);
			if(compare!=0) {
				return compare;
			}
			if(releaseManifestPublished.BuildUtc==DateTime.MinValue || releaseIdentityRunning.BuildUtc==DateTime.MinValue
				|| releaseManifestPublished.BuildUtc==releaseIdentityRunning.BuildUtc)
			{
				return null;
			}
			return releaseManifestPublished.BuildUtc.CompareTo(releaseIdentityRunning.BuildUtc);
		}

		///<summary>What this workstation does with the published release. No case ever offers a downgrade:
		///- Nothing published: the update server (named, or any workstation when WebServiceServerName is blank) offers to publish.
		///- A newer build is published: pull it.
		///- The named update server (WebServiceServerName is this computer) may replace the published build: it offers to publish.
		///- This build is known to be newer (higher release version, or same release and later build time): with a blank WebServiceServerName it offers
		///to publish, otherwise it is refused until the update server publishes it.
		///- Same release and the order is unknown: pull. Only the named update server replaces a build it cannot prove older, so a workstation that
		///still runs an old build never publishes it over the office's release.
		///Debug builds never publish or pull; the version guard still runs for them.
		///This overload is for a published manifest that was read (or none is published); see the other for one that is not readable.</summary>
		public static ReleaseDecision Decide(ReleaseManifest releaseManifestPublished,ReleaseIdentity releaseIdentityRunning,
			UpdateServerRole updateServerRole,bool isDebugBuild,string updateServerName="")
		{
			return Decide(releaseManifestPublished,"",releaseIdentityRunning,updateServerRole,isDebugBuild,updateServerName);
		}

		///<summary>Decide with the error GetPublishedManifest gave. Before the rules above, two refusals (S3a-10 of docs/specs/net10-plan.md), for every
		///update server role:
		///- A published manifest that is not readable or names another product (errorPublished, with a null manifest) is not "nothing published":
		///RefuseUnreadable. This workstation never publishes over it and never pulls.
		///- A release for another runtime or architecture than this build (GetRuntimeRefusal): RefuseOtherRuntime, "run the new setup". One
		///exception, the forward switch (IsForwardRuntimeSwitch: this build runs on .NET and the published release is a .NET Framework one): the rules
		///above may still publish this build over it, as on the update server whose first start after the .NET 10 setup publishes the .NET 10 release
		///(docs/specs/net10-plan.md, section 10 step 3). Only Publish survives; a pull, or any other decision, is RefuseOtherRuntime. Nothing ever
		///publishes a .NET Framework release over a .NET one.
		///Both refusals keep this workstation on its build; the message says why.</summary>
		public static ReleaseDecision Decide(ReleaseManifest releaseManifestPublished,string errorPublished,ReleaseIdentity releaseIdentityRunning,
			UpdateServerRole updateServerRole,bool isDebugBuild,string updateServerName="")
		{
			ReleaseDecision releaseDecision=new ReleaseDecision();
			if(isDebugBuild) {
				releaseDecision.Message="Debug builds do not publish or pull releases.";
				return releaseDecision;
			}
			if(releaseManifestPublished==null && !string.IsNullOrEmpty(errorPublished)) {
				releaseDecision.Action=ReleaseAction.RefuseUnreadable;
				releaseDecision.Message=GetUnreadableMessage(errorPublished);
				return releaseDecision;
			}
			if(releaseManifestPublished==null) {
				if(updateServerRole!=UpdateServerRole.OtherComputer) {
					releaseDecision.Action=ReleaseAction.Publish;
					releaseDecision.Message="No release is published yet.";
				}
				return releaseDecision;
			}
			if(releaseManifestPublished.BuildId==releaseIdentityRunning.BuildId) {
				return releaseDecision;
			}
			string refusalRuntime=GetRuntimeRefusal(releaseManifestPublished,releaseIdentityRunning);
			ReleaseDecision releaseDecisionByRole=DecideByRole(releaseManifestPublished,releaseIdentityRunning,updateServerRole,updateServerName);
			if(refusalRuntime=="") {
				return releaseDecisionByRole;
			}
			if(IsForwardRuntimeSwitch(releaseManifestPublished,releaseIdentityRunning) && releaseDecisionByRole.Action==ReleaseAction.Publish) {
				releaseDecisionByRole.Message+="\r\nThe published release is for "+DescribeRuntime(releaseManifestPublished.GetRuntimeOrDefault())
					+" and this build runs on "+DescribeRuntime(releaseIdentityRunning.Runtime)+". Once this build is published, the workstations that "
					+"still run the old release keep running it until the new setup runs on them.";
				return releaseDecisionByRole;
			}
			releaseDecision.Action=ReleaseAction.RefuseOtherRuntime;
			releaseDecision.Message=refusalRuntime;
			if(updateServerRole==UpdateServerRole.ThisComputer && IsForwardRuntimeSwitch(releaseManifestPublished,releaseIdentityRunning)) {
				//The update server on .NET whose rules would pull: the published .NET Framework release is newer than this build. Never a downgrade.
				releaseDecision.Message+="\r\nThis computer is the update server, but the published release is newer than this build, so this build "
					+"does not replace it. Install a newer release for "+DescribeRuntime(releaseIdentityRunning.Runtime)+" on this computer.";
			}
			return releaseDecision;
		}

		///<summary>The rules of Decide (see the first overload) for a readable published manifest of another build, without the refusals.</summary>
		private static ReleaseDecision DecideByRole(ReleaseManifest releaseManifestPublished,ReleaseIdentity releaseIdentityRunning,
			UpdateServerRole updateServerRole,string updateServerName)
		{
			ReleaseDecision releaseDecision=new ReleaseDecision();
			int? compare=CompareBuilds(releaseManifestPublished,releaseIdentityRunning);
			if(compare>0) {
				releaseDecision.Action=ReleaseAction.Pull;
				releaseDecision.Message="A newer build is published: "+Describe(releaseManifestPublished)+".";
				return releaseDecision;
			}
			if(updateServerRole==UpdateServerRole.ThisComputer) {
				releaseDecision.Action=ReleaseAction.Publish;
				releaseDecision.Message="This build is not the published one ("+Describe(releaseManifestPublished)+").";
				return releaseDecision;
			}
			if(compare<0) {
				if(updateServerRole==UpdateServerRole.AnyWorkstation) {
					releaseDecision.Action=ReleaseAction.Publish;
					releaseDecision.Message="This build is newer than the published one ("+Describe(releaseManifestPublished)+").";
					return releaseDecision;
				}
				releaseDecision.Action=ReleaseAction.RefuseNewerThanPublished;
				releaseDecision.Message="This workstation runs "+releaseIdentityRunning+", which is newer than the published release "
					+Describe(releaseManifestPublished)+".\r\n"
					+"Run the new release on the update server"+(string.IsNullOrEmpty(updateServerName)?"":" ("+updateServerName+")")
					+" first so it is published, or install the published release on this workstation.";
				return releaseDecision;
			}
			//Same release version and no build order. Only the named update server may replace the published build, so pull it.
			releaseDecision.Action=ReleaseAction.Pull;
			releaseDecision.Message="The update server published "+Describe(releaseManifestPublished)+".";
			if(updateServerRole==UpdateServerRole.AnyWorkstation) {
				releaseDecision.Message+="\r\nTo publish this build instead, name this computer as the update server (Update Setup) and start it again.";
			}
			return releaseDecision;
		}

		///<summary>True when this workstation may keep running a build other than the published one: the decision would offer to publish it (the
		///named update server, or a build known to be newer with a blank WebServiceServerName). Any other different build must update, except one
		///that cannot: a release for another runtime or architecture (RefuseOtherRuntime) leaves this workstation on its build until the new setup
		///runs, so a restart would not change anything. The forward switch (IsForwardRuntimeSwitch) that Decide publishes is a Publish here too.
		///</summary>
		public static bool MayRunUnpublishedBuild(ReleaseManifest releaseManifestPublished,ReleaseIdentity releaseIdentityRunning,
			UpdateServerRole updateServerRole)
		{
			if(releaseManifestPublished==null || releaseManifestPublished.BuildId==releaseIdentityRunning.BuildId) {
				return true;
			}
			ReleaseAction releaseAction=Decide(releaseManifestPublished,releaseIdentityRunning,updateServerRole,isDebugBuild:false).Action;
			return releaseAction==ReleaseAction.Publish || releaseAction==ReleaseAction.RefuseOtherRuntime;
		}

		///<summary>Why this build cannot update itself to the published release, or "" when it can: the release names another framework or
		///architecture (ReleaseManifest.GetRuntimeOrDefault; a manifest without a Runtime line is the .NET Framework 4.8 x86 build) than the running
		///build (ReleaseIdentity.Runtime). Such a release needs the new setup, which installs the program for that runtime in its own program folder
		///(decisions 19 and 20 of docs/specs/net10-plan.md). "" when the running build's runtime is unknown.</summary>
		public static string GetRuntimeRefusal(ReleaseManifest releaseManifestPublished,ReleaseIdentity releaseIdentityRunning) {
			if(releaseManifestPublished==null || releaseIdentityRunning?.Runtime==null) {
				return "";
			}
			ReleaseRuntime releaseRuntimePublished=releaseManifestPublished.GetRuntimeOrDefault();
			if(releaseRuntimePublished.IsSameFrameworkAndArchitecture(releaseIdentityRunning.Runtime)) {
				return "";
			}
			if(IsForwardRuntimeSwitch(releaseManifestPublished,releaseIdentityRunning)) {
				//This workstation already had the new setup. It never goes back to the .NET Framework release; the update server replaces that.
				return "The update server published "+Describe(releaseManifestPublished)+" for "+DescribeRuntime(releaseRuntimePublished)
					+", but this workstation runs on "+DescribeRuntime(releaseIdentityRunning.Runtime)+".\r\n"
					+"This workstation does not go back to that release. It keeps running "+releaseIdentityRunning+", and nothing was changed on it, "
					+"until the release for its runtime is published: run the new setup on the update server and start the program there.";
			}
			return "The update server published "+Describe(releaseManifestPublished)+" for "+DescribeRuntime(releaseRuntimePublished)
				+", but this workstation runs on "+DescribeRuntime(releaseIdentityRunning.Runtime)+".\r\n"
				+"This workstation cannot update itself to that release: run the new setup on it. Until then it keeps running "
				+releaseIdentityRunning+", and nothing was changed on it.";
		}

		///<summary>True for the one runtime switch a release may be published across: this build runs on .NET (Microsoft.NETCore.App) and the
		///published release is a .NET Framework one (a manifest without a Runtime line is). Decide still applies its role rules to it and lets only
		///Publish through. The other way round (a .NET Framework build over a .NET release) is never true, so the bridge release never publishes
		///over the .NET 10 release. False when either runtime is unknown.</summary>
		public static bool IsForwardRuntimeSwitch(ReleaseManifest releaseManifestPublished,ReleaseIdentity releaseIdentityRunning) {
			if(releaseManifestPublished==null || releaseIdentityRunning?.Runtime==null) {
				return false;
			}
			return releaseIdentityRunning.Runtime.Framework.Equals(ReleaseRuntime.FRAMEWORK_NET_CORE_APP,StringComparison.OrdinalIgnoreCase)
				&& releaseManifestPublished.GetRuntimeOrDefault().Framework.Equals(ReleaseRuntime.FRAMEWORK_NET_FRAMEWORK,StringComparison.OrdinalIgnoreCase);
		}

		///<summary>Why this build may neither pull nor publish over the published release, or "" when nothing stands in the way: the two refusals of
		///Decide (RefuseUnreadable, RefuseOtherRuntime) for the places that act outside it, the version guard's pull and Update Setup's Recopy.
		///updateServerRolePublishing is null for a pull (the version guard). For a publish (Recopy) it is this computer's role: then the forward
		///switch of Decide (IsForwardRuntimeSwitch) is not refused when Decide would publish, so the .NET 10 update server can Recopy over the
		///.NET Framework release.</summary>
		public static string GetRefusal(ReleaseManifest releaseManifestPublished,string errorPublished,ReleaseIdentity releaseIdentityRunning,
			UpdateServerRole? updateServerRolePublishing=null)
		{
			if(releaseManifestPublished==null) {
				return string.IsNullOrEmpty(errorPublished) ? "" : GetUnreadableMessage(errorPublished);
			}
			if(releaseManifestPublished.BuildId==releaseIdentityRunning.BuildId) {
				return "";
			}
			if(updateServerRolePublishing!=null && IsForwardRuntimeSwitch(releaseManifestPublished,releaseIdentityRunning)) {
				ReleaseDecision releaseDecision=Decide(releaseManifestPublished,errorPublished,releaseIdentityRunning,updateServerRolePublishing.Value,
					isDebugBuild:false);
				if(releaseDecision.Action==ReleaseAction.Publish) {
					return "";
				}
				return releaseDecision.Message;
			}
			return GetRuntimeRefusal(releaseManifestPublished,releaseIdentityRunning);
		}

		///<summary>The message for a published manifest that is not readable or names another product (the error of GetPublishedManifest).</summary>
		public static string GetUnreadableMessage(string errorPublished) {
			return errorPublished+"\r\n"
				+"This workstation neither updates from it nor publishes over it, and keeps running its own build. A newer version of the program may "
				+"have published it: if a newer version was installed on the update server, run the new setup on this workstation too. Otherwise "
				+"tell the person who looks after the program, and do not publish a release until they have looked at it.";
		}

		///<summary>The question Recopy asks before it replaces a published manifest that is not readable (S3a-10f, the owner's decision of
		///2026-11-15), or "" when it does not offer that: only on the named update server (WebServiceServerName is this computer), only for a
		///manifest that is not readable or names another product (Decide's RefuseUnreadable: a null manifest with an error), and never in a silent
		///run, so it is never automatic. Every other computer, and the blank WebServiceServerName, keeps refusing it as before.</summary>
		public static string GetReplaceUnreadableQuestion(ReleaseManifest releaseManifestPublished,string errorPublished,
			ReleaseIdentity releaseIdentityRunning,UpdateServerRole updateServerRole,bool isSilent)
		{
			if(isSilent || updateServerRole!=UpdateServerRole.ThisComputer || releaseManifestPublished!=null || string.IsNullOrEmpty(errorPublished)) {
				return "";
			}
			return errorPublished+"\r\n\r\n"
				+"This computer is the update server. Replace the published release with this build?\r\n"
				+"Yes deletes the published release from the database (the release manifest and its archive: the documentmisc rows of types "
				+(int)DocumentMiscType.ForkReleaseManifest+" and "+(int)DocumentMiscType.ForkReleaseSegment+") and publishes this build, "
				+releaseIdentityRunning+", as for a first release. The other workstations then update to this build when they start.\r\n"
				+"If a newer version of the program published that release (a newer version installed on another computer), click No: Yes would "
				+"replace that release with this older build. Click Yes only when the person who looks after the program has looked at it.\r\n"
				+"No changes nothing.";
		}

		///<summary>A runtime for people: ".NET Framework 4.8 (win-x86)" or ".NET 10.0.14 (win-x64)"; another framework by its own name.</summary>
		public static string DescribeRuntime(ReleaseRuntime releaseRuntime) {
			string framework=releaseRuntime.Framework;
			if(framework.Equals(ReleaseRuntime.FRAMEWORK_NET_FRAMEWORK,StringComparison.OrdinalIgnoreCase)) {
				framework=".NET Framework";
			}
			else if(framework.Equals(ReleaseRuntime.FRAMEWORK_NET_CORE_APP,StringComparison.OrdinalIgnoreCase)) {
				framework=".NET";
			}
			return framework+" "+releaseRuntime.Version+" ("+releaseRuntime.Architecture+")";
		}

		private static string Describe(ReleaseManifest releaseManifest) {
			ReleaseIdentity releaseIdentity=new ReleaseIdentity();
			releaseIdentity.ReleaseVersion=releaseManifest.ReleaseVersion;
			releaseIdentity.BuildId=releaseManifest.BuildId;
			releaseIdentity.BuildUtc=releaseManifest.BuildUtc;
			return releaseIdentity.ToString();
		}
		#endregion Decision

		#region Publish
		///<summary>True for files of the installation folder that are published. Local settings, logs and files a plugin marks with a double underscore
		///stay local (the same rules Open Dental used for its update files). Files an earlier release shipped and this one does not
		///(ListRetiredFiles) are never published.</summary>
		public static bool ShouldPublishFile(string fileName) {
			if(fileName.In(ProgramNames.ConfigFileName,ProgramNames.MiddleTierConfigFileName,"ProximitySettings.txt",
				ProgramNames.ReplicationServiceConfigFileName,
				ReleaseManifest.FILE_NAME,"Manifest.txt"))
			{
				return false;
			}
			if(ListRetiredFiles.Contains(fileName,StringComparer.OrdinalIgnoreCase)) {
				return false;
			}
			if(fileName.StartsWith("openlog",StringComparison.OrdinalIgnoreCase) || fileName.Contains("__")) {
				return false;
			}
			return true;
		}

		///<summary>Lists the files of the installation folder that a release contains, as paths relative to the folder.</summary>
		public static List<string> GetReleaseFiles(string folder) {
			List<string> listRelative=new List<string>();
			foreach(string path in Directory.GetFiles(folder)) {
				string name=Path.GetFileName(path);
				if(ShouldPublishFile(name)) {
					listRelative.Add(name);
				}
			}
			foreach(string specialDir in ListSpecialDirs) {
				string folderSpecial=Path.Combine(folder,specialDir);
				if(!Directory.Exists(folderSpecial)) {
					continue;
				}
				foreach(string path in Directory.GetFiles(folderSpecial)) {
					string name=Path.GetFileName(path);
					if(ShouldPublishFile(name)) {
						listRelative.Add(specialDir+"\\"+name);
					}
				}
			}
			return listRelative.OrderBy(x => x,StringComparer.OrdinalIgnoreCase).ToList();
		}

		///<summary>Builds the manifest and zipped archive for the files of the folder. The archive holds the files and a copy of the manifest without
		///the archive hash; the returned manifest has it.</summary>
		public static ReleaseManifest BuildRelease(string folder,ReleaseIdentity releaseIdentity,string publishedBy,DateTime dateTimeUtc,
			out byte[] arrayArchive)
		{
			ReleaseManifest releaseManifest=new ReleaseManifest();
			releaseManifest.ReleaseVersion=releaseIdentity.ReleaseVersion;
			releaseManifest.BuildId=releaseIdentity.BuildId;
			releaseManifest.BuildUtc=releaseIdentity.BuildUtc;
			releaseManifest.CompatVersion=ForkVersions.CompatVersion.ToString(4);
			releaseManifest.PublishedUtc=new DateTime(dateTimeUtc.Year,dateTimeUtc.Month,dateTimeUtc.Day,dateTimeUtc.Hour,dateTimeUtc.Minute,
				dateTimeUtc.Second,DateTimeKind.Utc);
			releaseManifest.PublishedBy=publishedBy??"";
			//A build for any other runtime than .NET Framework 4.8 x86 names its runtime, so a workstation on the bridge release refuses it ("run the
			//new setup") instead of reading it as one of its own. A bridge release writes no Runtime line, as before. S3b-7 adds the runtime part.
			if(releaseIdentity.Runtime!=null && !releaseIdentity.Runtime.IsSameFrameworkAndArchitecture(ReleaseRuntime.NetFramework48X86)) {
				releaseManifest.Runtime=releaseIdentity.Runtime;
			}
			//The archive layout: every file under its relative path with '/' separators, in GetReleaseFiles order, then the manifest at the root.
			using(MemoryStream memoryStream=new MemoryStream()) {
				using(ZipArchive zipArchive=new ZipArchive(memoryStream,ZipArchiveMode.Create,leaveOpen:true)) {
					foreach(string relative in GetReleaseFiles(folder)) {
						string path=Path.Combine(folder,relative);
						byte[] arrayBytes=File.ReadAllBytes(path);//Read once, so the hash and the archive hold the same bytes.
						ReleaseManifestFile releaseManifestFile=new ReleaseManifestFile();
						releaseManifestFile.RelativePath=relative;
						releaseManifestFile.Size=arrayBytes.LongLength;
						releaseManifestFile.Sha256=ReleaseManifest.ComputeSha256(arrayBytes);
						releaseManifest.ListFiles.Add(releaseManifestFile);
						ODZip.AddEntry(zipArchive,relative.Replace('\\','/'),arrayBytes);
					}
					ODZip.AddEntry(zipArchive,ReleaseManifest.FILE_NAME,Encoding.UTF8.GetBytes(releaseManifest.Serialize(includeArchiveHash:false)));
				}
				arrayArchive=memoryStream.ToArray();
			}
			releaseManifest.ArchiveSha256=ReleaseManifest.ComputeSha256(arrayArchive);
			return releaseManifest;
		}

		///<summary>Publishes the files of the folder as the current release: stores the archive segments and the manifest, then sets the
		///ForkProgramVersion marker. Direct connection only. Callers set UpdateInProgressOnComputerName around it so other workstations wait.</summary>
		public static ReleaseManifest Publish(string folder,ReleaseIdentity releaseIdentity,string publishedBy,DateTime? dateTimeUtc=null) {
			Meth.NoCheckMiddleTierRole();
			if(RemotingClient.MiddleTierRole==MiddleTierRole.ClientMT) {
				//The whole archive would travel as one Middle Tier payload. Publishing stays a direct-connection operation, as before S0-T.
				throw new ApplicationException("Releases can only be published over a direct database connection.");
			}
			byte[] arrayArchive;
			ReleaseManifest releaseManifest=BuildRelease(folder,releaseIdentity,publishedBy,dateTimeUtc??DateTime.UtcNow,out arrayArchive);
			List<string> listSegments=new List<string>();
			for(int offset=0;offset<arrayArchive.Length;offset+=SEGMENT_BYTES) {
				int length=Math.Min(SEGMENT_BYTES,arrayArchive.Length-offset);
				listSegments.Add(Convert.ToBase64String(arrayArchive,offset,length));
			}
			DocumentMiscs.ReplaceForkRelease(Convert.ToBase64String(Encoding.UTF8.GetBytes(releaseManifest.Serialize())),listSegments);
			ForkVersions.SetMarker(ForkVersions.PREF_FORK_PROGRAM_VERSION,FormatProgramVersion(releaseIdentity));
			return releaseManifest;
		}

		///<summary>Recopy over a published manifest that is not readable (S3a-10f, docs/port-notes/S3a-10.md section 12). When
		///GetReplaceUnreadableQuestion offers it (the named update server, a manifest Decide refuses as RefuseUnreadable, not silent), asks
		///funcConfirm with that question; on Yes it reads the manifest again, and only when it is still unreadable for the same reason it replaces
		///the rows of types ForkReleaseManifest and ForkReleaseSegment with this build through Publish (as for a first release: Publish deletes every
		///row of both types and writes the new ones) and writes a security log entry. funcConfirm is the seam the program answers with a Yes/No box
		///whose default is No, and the tests answer themselves. funcRunPublish runs the publish (the program wraps it with
		///UpdateInProgressOnComputerName and its progress window, and returns null when it failed); null runs Publish directly. Nothing is asked and
		///nothing changes for NotOffered, Declined and Changed.</summary>
		public static ReleaseReplaceResult ReplaceUnreadableRelease(string folder,ReleaseIdentity releaseIdentity,string publishedBy,string compName,
			UpdateServerRole updateServerRole,bool isSilent,Func<string,bool> funcConfirm,
			Func<Func<ReleaseManifest>,ReleaseManifest> funcRunPublish=null,DateTime? dateTimeUtc=null)
		{
			Meth.NoCheckMiddleTierRole();
			ReleaseReplaceResult releaseReplaceResult=new ReleaseReplaceResult();
			if(RemotingClient.MiddleTierRole==MiddleTierRole.ClientMT) {
				return releaseReplaceResult;//Releases are published over a direct connection only (Publish); Recopy then refuses as before.
			}
			ReleaseManifest releaseManifestPublished=GetPublishedManifest(out string errorPublished);
			string question=GetReplaceUnreadableQuestion(releaseManifestPublished,errorPublished,releaseIdentity,updateServerRole,isSilent);
			if(question=="" || funcConfirm==null) {
				return releaseReplaceResult;
			}
			if(!funcConfirm(question)) {
				releaseReplaceResult.Outcome=ReleaseReplaceOutcome.Declined;
				return releaseReplaceResult;
			}
			ReleaseManifest releaseManifestNow=GetPublishedManifest(out string errorNow);
			if(releaseManifestNow!=null || errorNow!=errorPublished) {
				releaseReplaceResult.Outcome=ReleaseReplaceOutcome.Changed;
				releaseReplaceResult.Message="The published release changed while the question was shown, so nothing was replaced. Click Recopy again "
					+"to see what is published now.";
				return releaseReplaceResult;
			}
			Func<ReleaseManifest> funcPublish=() => Publish(folder,releaseIdentity,publishedBy,dateTimeUtc);
			ReleaseManifest releaseManifest=funcRunPublish==null ? funcPublish() : funcRunPublish(funcPublish);
			if(releaseManifest==null) {
				releaseReplaceResult.Outcome=ReleaseReplaceOutcome.Failed;
				return releaseReplaceResult;
			}
			SecurityLog securityLog=new SecurityLog();
			securityLog.PermType=EnumPermType.UpdateInstall;
			securityLog.UserNum=Security.CurUser?.UserNum??0;
			securityLog.LogDateTime=DateTime.Now;
			securityLog.LogText="Recopy on the update server replaced a published release manifest that was not readable ("+errorPublished
				.Replace("\r"," ").Replace("\n"," ")+"). Release "+releaseIdentity+" published as for a first release with "+releaseManifest.ListFiles.Count
				+" files.";
			securityLog.CompName=compName??"";
			SecurityLogs.MakeLogEntry(securityLog);
			releaseReplaceResult.Outcome=ReleaseReplaceOutcome.Replaced;
			releaseReplaceResult.Manifest=releaseManifest;
			return releaseReplaceResult;
		}
		#endregion Publish

		#region Pull
		///<summary>The published release manifest, or null. Null with error "" when none is published. A manifest that does not parse or names another
		///product is null too, with error saying why: that is not "nothing published", and Decide refuses it (RefuseUnreadable).</summary>
		public static ReleaseManifest GetPublishedManifest(out string error) {
			error="";
			DocumentMisc documentMisc=DocumentMiscs.GetForkReleaseManifest();
			if(documentMisc==null) {
				return null;
			}
			ReleaseManifest releaseManifest;
			try {
				releaseManifest=ReleaseManifest.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(documentMisc.RawBase64??"")));
			}
			catch(Exception ex) {
				error="The published release manifest is not readable: "+ex.Message;
				return null;
			}
			if(!releaseManifest.IsOurProduct()) {
				error="The published release manifest is for another product ("+releaseManifest.Product+").";
				return null;
			}
			return releaseManifest;
		}

		///<summary>The published manifest (for its build time and runtime), or, when it is missing or another build's, a manifest with only the
		///release version and build id of the ForkProgramVersion marker, whose build order is then unknown. Null when the published manifest is not
		///readable or names another product: this workstation can neither pull nor publish then (RefuseUnreadable), so MayRunUnpublishedBuild lets it
		///keep running.</summary>
		public static ReleaseManifest GetPublishedManifestOrMarker(ReleaseIdentity releaseIdentityMarker) {
			ReleaseManifest releaseManifest=GetPublishedManifest(out string error);
			if(releaseManifest==null && error!="") {
				return null;
			}
			if(releaseManifest!=null && releaseManifest.BuildId==releaseIdentityMarker.BuildId) {
				return releaseManifest;
			}
			releaseManifest=new ReleaseManifest();
			releaseManifest.ReleaseVersion=releaseIdentityMarker.ReleaseVersion;
			releaseManifest.BuildId=releaseIdentityMarker.BuildId;
			return releaseManifest;
		}

		///<summary>Downloads the published release into the empty or missing folder and checks it: product name, archive SHA-256, only listed files,
		///safe paths, the archive's own manifest equal to the published one, and every file's size and SHA-256. On any problem IsOk is false and the
		///folder must not be used. With releaseIdentityRunning (the program passes its own), a release for another framework or architecture is
		///refused before anything is downloaded (GetRuntimeRefusal); null, or an identity without a runtime, skips that check.</summary>
		public static ReleaseDownloadResult DownloadAndVerify(string folderDest,ReleaseIdentity releaseIdentityRunning=null) {
			ReleaseDownloadResult releaseDownloadResult=new ReleaseDownloadResult();
			ReleaseManifest releaseManifest=GetPublishedManifest(out string error);
			if(releaseManifest==null) {
				releaseDownloadResult.ListProblems.Add(error==""?"No release is published.":error);
				return releaseDownloadResult;
			}
			releaseDownloadResult.Manifest=releaseManifest;
			string refusalRuntime=GetRuntimeRefusal(releaseManifest,releaseIdentityRunning);
			if(refusalRuntime!="") {
				releaseDownloadResult.ListProblems.Add(refusalRuntime);
				return releaseDownloadResult;
			}
			List<DocumentMisc> listSegments=DocumentMiscs.GetForkReleaseSegments();
			byte[] arrayArchive;
			try {
				using(MemoryStream memoryStream=new MemoryStream()) {
					foreach(DocumentMisc documentMisc in listSegments) {
						byte[] arrayBytes=Convert.FromBase64String(documentMisc.RawBase64??"");
						memoryStream.Write(arrayBytes,0,arrayBytes.Length);
					}
					arrayArchive=memoryStream.ToArray();
				}
			}
			catch(Exception ex) {
				releaseDownloadResult.ListProblems.Add("The release archive is not readable: "+ex.Message);
				return releaseDownloadResult;
			}
			if(ReleaseManifest.ComputeSha256(arrayArchive)!=releaseManifest.ArchiveSha256.ToLowerInvariant()) {
				releaseDownloadResult.ListProblems.Add("The release archive does not match its SHA-256. It may be incomplete; publish it again from the "
					+"update server.");
				return releaseDownloadResult;
			}
			releaseDownloadResult.ListProblems.AddRange(ExtractArchive(arrayArchive,folderDest,releaseManifest));
			if(releaseDownloadResult.ListProblems.Count>0) {
				return releaseDownloadResult;
			}
			releaseDownloadResult.ListProblems.AddRange(releaseManifest.VerifyFolder(folderDest));
			releaseDownloadResult.IsOk=releaseDownloadResult.ListProblems.Count==0;
			return releaseDownloadResult;
		}

		///<summary>The largest manifest read from a release archive. A real one is a few hundred bytes per file.</summary>
		private const long MAX_MANIFEST_BYTES=16*1024*1024;

		///<summary>Extracts the release archive into folderDest, which is emptied first, and returns the problems found; none means every listed file
		///was written. Nothing is ever written outside folderDest: an entry with an unsafe path (absolute, a drive, "..", invalid characters), one
		///that appears twice, one the manifest does not list, or one that inflates beyond its listed size is refused, and the archive's own manifest
		///must equal the published one. File sizes and SHA-256 are checked afterwards by ReleaseManifest.VerifyFolder.</summary>
		public static List<string> ExtractArchive(byte[] arrayArchive,string folderDest,ReleaseManifest releaseManifest) {
			List<string> listProblems=new List<string>();
			if(Directory.Exists(folderDest)) {
				Directory.Delete(folderDest,true);
			}
			Directory.CreateDirectory(folderDest);
			Dictionary<string,ReleaseManifestFile> dictionaryListed=new Dictionary<string,ReleaseManifestFile>(StringComparer.OrdinalIgnoreCase);
			foreach(ReleaseManifestFile releaseManifestFile in releaseManifest.ListFiles) {
				dictionaryListed[releaseManifestFile.RelativePath.Replace('/','\\')]=releaseManifestFile;
			}
			HashSet<string> hashSetSeen=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			string manifestInside=null;
			try {
				using(MemoryStream memoryStream=new MemoryStream(arrayArchive))
				using(ZipArchive zipArchive=new ZipArchive(memoryStream,ZipArchiveMode.Read)) {
					foreach(ZipArchiveEntry zipArchiveEntry in zipArchive.Entries) {
						if(ODZip.IsDirectory(zipArchiveEntry)) {
							continue;
						}
						string relative=zipArchiveEntry.FullName.Replace('/','\\');
						if(!ReleaseManifest.IsSafeRelativePath(relative)) {
							listProblems.Add("Unsafe path in the release archive: "+zipArchiveEntry.FullName);
							continue;
						}
						if(!hashSetSeen.Add(relative)) {
							listProblems.Add("In the release archive twice: "+relative);
							continue;
						}
						if(relative.Equals(ReleaseManifest.FILE_NAME,StringComparison.OrdinalIgnoreCase)) {
							manifestInside=Encoding.UTF8.GetString(ODZip.ReadEntry(zipArchiveEntry,MAX_MANIFEST_BYTES));
							continue;
						}
						if(!dictionaryListed.TryGetValue(relative,out ReleaseManifestFile releaseManifestFile)) {
							listProblems.Add("Not in the manifest: "+relative);
							continue;
						}
						string pathDest=ODZip.GetSafeExtractPath(folderDest,relative);
						if(pathDest==null) {
							listProblems.Add("Unsafe path in the release archive: "+zipArchiveEntry.FullName);
							continue;
						}
						Directory.CreateDirectory(Path.GetDirectoryName(pathDest));
						try {
							using(Stream stream=zipArchiveEntry.Open())
							using(FileStream fileStream=new FileStream(pathDest,FileMode.Create,FileAccess.Write)) {
								ODZip.CopyBounded(stream,fileStream,releaseManifestFile.Size);
							}
						}
						catch(ODZip.EntryTooLargeException) {
							File.Delete(pathDest);
							listProblems.Add("Larger in the release archive than the manifest says: "+relative);
						}
					}
				}
			}
			catch(Exception ex) when(ex is InvalidDataException || ex is ODZip.EntryTooLargeException) {
				//A corrupt archive, or a manifest entry that inflates beyond MAX_MANIFEST_BYTES.
				listProblems.Add("The release archive is not a readable zip archive: "+ex.Message);
				return listProblems;
			}
			if(manifestInside==null) {
				listProblems.Add("The release archive has no manifest.");
				return listProblems;
			}
			if(manifestInside!=releaseManifest.Serialize(includeArchiveHash:false)) {
				listProblems.Add("The manifest inside the release archive differs from the published manifest.");
				return listProblems;
			}
			File.WriteAllText(Path.Combine(folderDest,ReleaseManifest.FILE_NAME),releaseManifest.Serialize(),new UTF8Encoding(false));
			return listProblems;
		}
		#endregion Pull

		#region File copier
		///<summary>The sub folder of the program's temp folder that UpdateFileCopier runs from during a pull (PrefL.OpenFileCopier). It holds only
		///the copier's own files (CopyFileCopier), never the program's, because the copier replaces the program's files while it runs.</summary>
		public const string FILE_COPIER_FOLDER_NAME="ForkReleaseCopier";
		///<summary>What UpdateFileCopier needs beside its exe, by the ending of the file name after "UpdateFileCopier": its .NET Framework settings
		///(.exe.config, which a release does not ship today: the copier runs without it), and the files of a .NET program (.dll, .runtimeconfig.json,
		///.deps.json; the .exe is then the apphost), so a pull of the .NET 10 release starts a copier that runs (S3a-10 (iii), S3b-7, which adds the
		///runtime files). ReleaseFlowTests checks that every file the copier's build writes, apart from symbols, is one of these.</summary>
		public static readonly List<string> ListFileCopierEndings=new List<string>() {
			".exe",
			".exe.config",
			".dll",
			".runtimeconfig.json",
			".deps.json",
		};

		///<summary>The names of UpdateFileCopier's files in the folder (ListFileCopierEndings), the exe first. Throws FileNotFoundException when the
		///folder has no UpdateFileCopier.exe.</summary>
		public static List<string> GetFileCopierFiles(string folder) {
			string baseName=Path.GetFileNameWithoutExtension(ProgramNames.UpdateFileCopierExeFileName);
			if(!File.Exists(Path.Combine(folder,ProgramNames.UpdateFileCopierExeFileName))) {
				throw new FileNotFoundException(ProgramNames.UpdateFileCopierExeFileName+" is not in "+folder+".",
					Path.Combine(folder,ProgramNames.UpdateFileCopierExeFileName));
			}
			List<string> listFileNames=new List<string>();
			foreach(string ending in ListFileCopierEndings) {
				if(File.Exists(Path.Combine(folder,baseName+ending))) {
					listFileNames.Add(baseName+ending);
				}
			}
			return listFileNames;
		}

		///<summary>Copies every file UpdateFileCopier needs (GetFileCopierFiles) from folderSource into folderDest, which is emptied first, so it
		///never mixes the files of two copiers. Returns the path of the copied exe. Throws when the exe is missing or a file cannot be copied (an
		///earlier copier still running from folderDest, for example).</summary>
		public static string CopyFileCopier(string folderSource,string folderDest) {
			List<string> listFileNames=GetFileCopierFiles(folderSource);
			if(Directory.Exists(folderDest)) {
				Directory.Delete(folderDest,true);
			}
			Directory.CreateDirectory(folderDest);
			foreach(string fileName in listFileNames) {
				File.Copy(Path.Combine(folderSource,fileName),Path.Combine(folderDest,fileName),false);
			}
			return Path.Combine(folderDest,ProgramNames.UpdateFileCopierExeFileName);
		}

		///<summary>How PrefL.OpenFileCopier starts UpdateFileCopier: through ShellExecute (UseShellExecute=true). The copier's manifest asks for
		///requireAdministrator, and only ShellExecute asks for consent; CreateProcess, which UseShellExecute=false means, refuses such a program with
		///ERROR_ELEVATION_REQUIRED (740) for a standard user. True is the .NET Framework default, so nothing changes today; on .NET the default is
		///false (S3a-10 (iv) of docs/specs/net10-plan.md).</summary>
		public static System.Diagnostics.ProcessStartInfo GetFileCopierStartInfo(string pathExe,string arguments) {
			System.Diagnostics.ProcessStartInfo processStartInfo=new System.Diagnostics.ProcessStartInfo();
			processStartInfo.FileName=pathExe;
			processStartInfo.Arguments=arguments;
			processStartInfo.UseShellExecute=true;
			return processStartInfo;
		}
		#endregion File copier
	}
}
