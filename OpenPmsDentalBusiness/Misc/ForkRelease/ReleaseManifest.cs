// The manifest of a fork release published to the database. This file has no dependencies outside the .NET Framework because
// UpdateFileCopier compiles it too (linked), so the copier can check every file before it replaces the installation.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace OpenPmsDentalBusiness {
	///<summary>One file of a release: its path relative to the installation folder (backslashes), size and SHA-256.</summary>
	public class ReleaseManifestFile {
		public string RelativePath;
		public long Size;
		///<summary>Lower-case hex.</summary>
		public string Sha256;
	}

	///<summary>The runtime a release's programs run on: the framework, its version and the architecture (a .NET runtime identifier), as the optional
	///"Runtime" line of a release manifest names it, for example "Microsoft.NETCore.App 10.0.14 win-x64" (S3a-10 of docs/specs/net10-plan.md). A
	///manifest without that line is a release of the .NET Framework 4.8 build, which is x86 (NetFramework48X86). An installation updates itself only
	///to a release for its own framework and architecture; for another one it needs the new setup (ForkReleases.GetRuntimeRefusal).</summary>
	[Serializable]
	public class ReleaseRuntime {
		///<summary>The framework name of the .NET Framework (the identifier of its target framework moniker).</summary>
		public const string FRAMEWORK_NET_FRAMEWORK=".NETFramework";
		///<summary>The framework name of the .NET runtime (the shared framework every .NET 5+ program runs on).</summary>
		public const string FRAMEWORK_NET_CORE_APP="Microsoft.NETCore.App";
		public const string ARCHITECTURE_X86="win-x86";
		public const string ARCHITECTURE_X64="win-x64";
		public string Framework="";
		public string Version="";
		///<summary>The runtime identifier of the architecture, for example win-x86 or win-x64.</summary>
		public string Architecture="";

		public ReleaseRuntime() {
		}

		public ReleaseRuntime(string framework,string version,string architecture) {
			Framework=framework;
			Version=version;
			Architecture=architecture;
		}

		///<summary>What a release manifest without a "Runtime" line means: every release before the .NET 10 release is the .NET Framework 4.8
		///build, x86.</summary>
		public static ReleaseRuntime NetFramework48X86 {
			get { return new ReleaseRuntime(FRAMEWORK_NET_FRAMEWORK,"4.8",ARCHITECTURE_X86); }
		}

		///<summary>The runtime of this process: the .NET Framework (Environment.Version 4.x, the version given as 4.8, the one the program targets)
		///or the .NET runtime (Environment.Version is the runtime's own version on .NET 5 and later), and win-x64 or win-x86 by the process's
		///pointer size.</summary>
		public static ReleaseRuntime GetRunning() {
			System.Version version=Environment.Version;
			string architecture=Environment.Is64BitProcess ? ARCHITECTURE_X64 : ARCHITECTURE_X86;
			if(version.Major<5) {
				return new ReleaseRuntime(FRAMEWORK_NET_FRAMEWORK,"4.8",architecture);
			}
			return new ReleaseRuntime(FRAMEWORK_NET_CORE_APP,version.ToString(3),architecture);
		}

		///<summary>True when both name the same framework and the same architecture. The version is not compared: a newer runtime of the same
		///framework comes with a release as its runtime part (S3b-7), which the normal self-update installs.</summary>
		public bool IsSameFrameworkAndArchitecture(ReleaseRuntime releaseRuntime) {
			return releaseRuntime!=null && Framework.Equals(releaseRuntime.Framework,StringComparison.OrdinalIgnoreCase)
				&& Architecture.Equals(releaseRuntime.Architecture,StringComparison.OrdinalIgnoreCase);
		}

		///<summary>The value of a "Runtime" line: framework, version and architecture separated by single spaces.</summary>
		public override string ToString() {
			return Framework+" "+Version+" "+Architecture;
		}

		///<summary>Parses the value of a "Runtime" line. Throws FormatException unless it is three parts separated by single spaces: a framework name,
		///a version of two to four numbers and a runtime identifier (letters, digits, '.', '-' and '_').</summary>
		public static ReleaseRuntime Parse(string value) {
			string[] arrayParts=(value??"").Split(' ');
			if(arrayParts.Length!=3 || !IsToken(arrayParts[0]) || !IsToken(arrayParts[2])) {
				throw new FormatException("Bad runtime in the release manifest: "+value);
			}
			System.Version version;
			if(!System.Version.TryParse(arrayParts[1],out version) || version.ToString()!=arrayParts[1]) {
				throw new FormatException("Bad runtime version in the release manifest: "+value);
			}
			return new ReleaseRuntime(arrayParts[0],arrayParts[1],arrayParts[2]);
		}

		///<summary>A framework name or runtime identifier: letters, digits, '.', '-' and '_', at most 100 characters.</summary>
		public static bool IsToken(string value) {
			if(string.IsNullOrEmpty(value) || value.Length>100) {
				return false;
			}
			foreach(char c in value) {
				bool isAllowed=(c>='a' && c<='z') || (c>='A' && c<='Z') || (c>='0' && c<='9') || c=='.' || c=='-' || c=='_';
				if(!isAllowed) {
					return false;
				}
			}
			return true;
		}
	}

	///<summary>The runtime part a release needs: the runtime files that a self-contained .NET 10 release carries beside its programs (decision 20 of
	///docs/specs/net10-plan.md). S3b-7 publishes it in rows of its own, apart from the program part, and only when it changes. Its manifest lines
	///(S3a-10 defines them, so the bridge release reads a .NET 10 manifest): RuntimeVersion, RuntimeArchitectures (the runtime identifiers of the
	///files, the program's first, separated by single spaces), RuntimeArchiveSha256 (the zipped runtime part), RuntimeKey (ComputeKey) and one
	///"RuntimeFile=sha256|size|relative path" line per file.</summary>
	public class ReleaseRuntimePart {
		///<summary>Name of the runtime part's own manifest: inside its archive, in its database row (DocumentMiscType.ForkRuntimeManifest) and in a
		///download folder that holds the runtime part's files beside the program part's (S3b-7). Never in the installation folder.</summary>
		public const string MANIFEST_FILE_NAME="ForkRuntime.manifest";
		public string Version="";
		public List<string> ListArchitectures=new List<string>();
		///<summary>Lower-case hex SHA-256 of the zipped runtime part.</summary>
		public string ArchiveSha256="";
		///<summary>Lower-case hex; must equal ComputeKey().</summary>
		public string Key="";
		public List<ReleaseManifestFile> ListFiles=new List<ReleaseManifestFile>();

		///<summary>The key of the runtime part: the SHA-256 over its version and the names, sizes and SHA-256 values of its files, so two releases on
		///the same runtime files share one runtime part.</summary>
		public string ComputeKey() {
			StringBuilder stringBuilder=new StringBuilder();
			stringBuilder.Append("version|"+Version+"\n");
			foreach(ReleaseManifestFile releaseManifestFile in ListFiles.OrderBy(x => x.RelativePath.Replace('/','\\'),StringComparer.OrdinalIgnoreCase)) {
				stringBuilder.Append(releaseManifestFile.RelativePath.Replace('/','\\').ToLowerInvariant()+"|"
					+releaseManifestFile.Size.ToString(CultureInfo.InvariantCulture)+"|"+releaseManifestFile.Sha256.ToLowerInvariant()+"\n");
			}
			return ReleaseManifest.ComputeSha256(Encoding.UTF8.GetBytes(stringBuilder.ToString()));
		}

		///<summary>The runtime part's own manifest (S3b-7: the row of DocumentMiscType.ForkRuntimeManifest and the copy inside the runtime part's
		///archive): Format, Product, Runtime (the framework, the version and the program's architecture), then the five runtime part lines the program
		///part's manifest carries too. Pass includeArchiveHash=false for the copy inside the archive, which cannot hold its own hash. A program part
		///names the runtime part it needs with the same lines, so the runtime part's key and archive hash are checked against both.</summary>
		public string SerializeManifest(bool includeArchiveHash=true) {
			StringBuilder stringBuilder=new StringBuilder();
			stringBuilder.Append("# Runtime part of a release. Checked with SHA-256 before any file is used.\n");
			stringBuilder.Append("Format="+ReleaseManifest.FORMAT.ToString(CultureInfo.InvariantCulture)+"\n");
			stringBuilder.Append("Product="+ReleaseManifest.PRODUCT_NAME+"\n");
			stringBuilder.Append("Runtime="+ReleaseRuntime.FRAMEWORK_NET_CORE_APP+" "+Version+" "+(ListArchitectures.Count>0 ? ListArchitectures[0] : "")+"\n");
			AppendLines(stringBuilder,includeArchiveHash);
			return stringBuilder.ToString();
		}

		///<summary>The five runtime part lines (RuntimeVersion, RuntimeArchitectures, RuntimeArchiveSha256 unless left out, RuntimeKey, one RuntimeFile
		///per file in path order), as both manifests write them.</summary>
		internal void AppendLines(StringBuilder stringBuilder,bool includeArchiveHash) {
			stringBuilder.Append("RuntimeVersion="+ReleaseManifest.Clean(Version)+"\n");
			stringBuilder.Append("RuntimeArchitectures="+ReleaseManifest.Clean(string.Join(" ",ListArchitectures))+"\n");
			if(includeArchiveHash) {
				stringBuilder.Append("RuntimeArchiveSha256="+ReleaseManifest.Clean(ArchiveSha256)+"\n");
			}
			stringBuilder.Append("RuntimeKey="+ReleaseManifest.Clean(Key)+"\n");
			foreach(ReleaseManifestFile releaseManifestFile in ListFiles.OrderBy(x => x.RelativePath,StringComparer.OrdinalIgnoreCase)) {
				stringBuilder.Append("RuntimeFile="+ReleaseManifest.FormatFileLine(releaseManifestFile)+"\n");
			}
		}

		///<summary>Parses the runtime part's own manifest (SerializeManifest). Throws FormatException for anything malformed: an unknown format or key,
		///another product, a Runtime line that is not Microsoft.NETCore.App or does not fit the runtime part's version and first architecture, a missing
		///line (RuntimeArchiveSha256 may be missing only when requireArchiveHash is false, for the copy inside the archive), a file listed twice, an
		///unsafe path, or a key that does not match the files.</summary>
		public static ReleaseRuntimePart ParseManifest(string text,bool requireArchiveHash=true) {
			if(text==null) {
				throw new FormatException("The runtime part manifest is empty.");
			}
			ReleaseRuntimePart releaseRuntimePart=new ReleaseRuntimePart();
			HashSet<string> hashSetKeys=new HashSet<string>();
			ReleaseRuntime releaseRuntime=null;
			string product=null;
			bool hasFormat=false;
			foreach(string lineRaw in text.Replace("\r\n","\n").Split('\n')) {
				string line=lineRaw.Trim();
				if(line=="" || line.StartsWith("#")) {
					continue;
				}
				int indexEquals=line.IndexOf('=');
				if(indexEquals<=0) {
					throw new FormatException("Bad runtime part manifest line: "+line);
				}
				string key=line.Substring(0,indexEquals);
				string value=line.Substring(indexEquals+1);
				if(key!="RuntimeFile" && !hashSetKeys.Add(key)) {
					throw new FormatException("The runtime part manifest has "+key+" twice.");
				}
				switch(key) {
					case "Format":
						if(value!=ReleaseManifest.FORMAT.ToString(CultureInfo.InvariantCulture)) {
							throw new FormatException("Unknown runtime part manifest format: "+value);
						}
						hasFormat=true;
						break;
					case "Product":
						product=value;
						break;
					case "Runtime":
						releaseRuntime=ReleaseRuntime.Parse(value);
						break;
					case "RuntimeVersion":
					case "RuntimeArchitectures":
					case "RuntimeArchiveSha256":
					case "RuntimeKey":
						ReleaseManifest.ParseRuntimePartLine(releaseRuntimePart,key,value);
						break;
					case "RuntimeFile":
						hashSetKeys.Add(key);
						releaseRuntimePart.ListFiles.Add(ReleaseManifest.ParseFileLine(value));
						break;
					default:
						throw new FormatException("Unknown runtime part manifest key: "+key);
				}
			}
			if(!hasFormat) {
				throw new FormatException("The runtime part manifest has no Format line.");
			}
			if(product!=ReleaseManifest.PRODUCT_NAME) {
				throw new FormatException("The runtime part manifest is for another product ("+product+").");
			}
			if(releaseRuntime==null || !releaseRuntime.Framework.Equals(ReleaseRuntime.FRAMEWORK_NET_CORE_APP,StringComparison.OrdinalIgnoreCase)) {
				throw new FormatException("The runtime part manifest names no .NET runtime ("+ReleaseRuntime.FRAMEWORK_NET_CORE_APP+").");
			}
			foreach(string key in new[] { "RuntimeVersion","RuntimeArchitectures","RuntimeKey","RuntimeFile" }) {
				if(!hashSetKeys.Contains(key)) {
					throw new FormatException("The runtime part manifest has no "+key+" line.");
				}
			}
			if(requireArchiveHash && !hashSetKeys.Contains("RuntimeArchiveSha256")) {
				throw new FormatException("The runtime part manifest has no RuntimeArchiveSha256 line.");
			}
			ReleaseManifest releaseManifestCheck=new ReleaseManifest();
			releaseManifestCheck.Runtime=releaseRuntime;
			ReleaseManifest.CheckRuntimePart(releaseManifestCheck,releaseRuntimePart,
				new HashSet<string>(new[] { "RuntimeVersion","RuntimeArchitectures","RuntimeArchiveSha256","RuntimeKey","RuntimeFile" }));
			return releaseRuntimePart;
		}

		///<summary>True when both name the same runtime files: the same key, version and architectures (the key covers the version and every file).
		///</summary>
		public bool IsSameAs(ReleaseRuntimePart releaseRuntimePart) {
			return releaseRuntimePart!=null && Key.Equals(releaseRuntimePart.Key,StringComparison.OrdinalIgnoreCase)
				&& Version==releaseRuntimePart.Version
				&& string.Join(" ",ListArchitectures).Equals(string.Join(" ",releaseRuntimePart.ListArchitectures),StringComparison.OrdinalIgnoreCase);
		}

		///<summary>Checks every runtime file in the folder: present, with the listed size and SHA-256. Other files of the folder are not looked at.
		///UpdateFileCopier runs it on the installation folder after copying, before it writes the release manifest (S3b-7), so a workstation never
		///claims a release whose runtime files are of another version. Returns the problems found; an empty list means every runtime file matches.
		///</summary>
		public List<string> VerifyFiles(string folder) {
			List<string> listProblems=new List<string>();
			foreach(ReleaseManifestFile releaseManifestFile in ListFiles) {
				listProblems.AddRange(ReleaseManifest.VerifyFile(folder,releaseManifestFile,"runtime file"));
			}
			return listProblems;
		}
	}

	///<summary>The text manifest of a release published by the update server. It names our product, so a manifest from any other program is never
	///accepted, and carries the SHA-256 of the archive and of every file. Format, one "Key=Value" per line, '#' starts a comment:
	///Format, Product, ReleaseVersion, BuildId, BuildUtc (optional), CompatVersion, PublishedUtc, PublishedBy, ArchiveSha256 (only in the database copy, because the
	///archive itself holds a copy of the manifest), Runtime (optional, ReleaseRuntime), the runtime part's lines (optional, ReleaseRuntimePart) and
	///one "File=sha256|size|relative path" line per file.</summary>
	public class ReleaseManifest {
		///<summary>The product name every manifest must carry. The internal rename of the program comes later; release manifests use the new name
		///from the start so they can never be confused with another program's update files.</summary>
		public const string PRODUCT_NAME="Open PMS - Dental";
		///<summary>Name of the manifest file inside the archive and in the installation folder.</summary>
		public const string FILE_NAME="ForkRelease.manifest";
		public const int FORMAT=1;

		public string Product=PRODUCT_NAME;
		///<summary>The release (product) version, e.g. "0.0". Our own scheme, never compared with Open Dental version numbers.</summary>
		public string ReleaseVersion="";
		///<summary>Identifies the exact build: SHA-256 over the program's own assemblies (see ForkReleases.ComputeBuildId).</summary>
		public string BuildId="";
		///<summary>When the build was compiled (UTC, ForkBuildUtc metadata of OpenPmsDental.exe). Orders two builds of the same release version.
		///DateTime.MinValue when unknown; the "BuildUtc" line is then left out.</summary>
		public DateTime BuildUtc=DateTime.MinValue;
		///<summary>The Open Dental compatible version line of the build (24.3.63.0 until the fork markers take over). Information only.</summary>
		public string CompatVersion="";
		public DateTime PublishedUtc=DateTime.MinValue;
		public string PublishedBy="";
		///<summary>SHA-256 of the zipped archive. Blank in the copy of the manifest that is inside the archive.</summary>
		public string ArchiveSha256="";
		///<summary>The runtime the release's programs run on, from the optional "Runtime" line. Null when the manifest has none: a release of the
		///.NET Framework 4.8 build, x86 (GetRuntimeOrDefault).</summary>
		public ReleaseRuntime Runtime;
		///<summary>The runtime part the release needs, from the optional runtime part lines. Null when the manifest has none. Only with Runtime.
		///</summary>
		public ReleaseRuntimePart RuntimePart;
		public List<ReleaseManifestFile> ListFiles=new List<ReleaseManifestFile>();

		///<summary>The runtime the release names, or the .NET Framework 4.8 x86 one of a manifest without a "Runtime" line.</summary>
		public ReleaseRuntime GetRuntimeOrDefault() {
			return Runtime??ReleaseRuntime.NetFramework48X86;
		}

		///<summary>The manifest as text. Pass includeArchiveHash=false for the copy that goes inside the archive.</summary>
		public string Serialize(bool includeArchiveHash=true) {
			StringBuilder stringBuilder=new StringBuilder();
			stringBuilder.Append("# Release manifest. Checked with SHA-256 before any file is used.\n");
			stringBuilder.Append("Format="+FORMAT.ToString(CultureInfo.InvariantCulture)+"\n");
			stringBuilder.Append("Product="+Clean(Product)+"\n");
			stringBuilder.Append("ReleaseVersion="+Clean(ReleaseVersion)+"\n");
			stringBuilder.Append("BuildId="+Clean(BuildId)+"\n");
			if(BuildUtc!=DateTime.MinValue) {
				stringBuilder.Append("BuildUtc="+BuildUtc.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'",CultureInfo.InvariantCulture)+"\n");
			}
			stringBuilder.Append("CompatVersion="+Clean(CompatVersion)+"\n");
			stringBuilder.Append("PublishedUtc="+PublishedUtc.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'",CultureInfo.InvariantCulture)+"\n");
			stringBuilder.Append("PublishedBy="+Clean(PublishedBy)+"\n");
			if(includeArchiveHash) {
				stringBuilder.Append("ArchiveSha256="+Clean(ArchiveSha256)+"\n");
			}
			//The runtime lines are in both copies: RuntimeArchiveSha256 is the hash of the runtime part's own archive, not of this one.
			if(Runtime!=null) {
				stringBuilder.Append("Runtime="+Clean(Runtime.ToString())+"\n");
			}
			if(RuntimePart!=null) {
				RuntimePart.AppendLines(stringBuilder,includeArchiveHash:true);
			}
			foreach(ReleaseManifestFile releaseManifestFile in ListFiles.OrderBy(x => x.RelativePath,StringComparer.OrdinalIgnoreCase)) {
				stringBuilder.Append("File="+FormatFileLine(releaseManifestFile)+"\n");
			}
			return stringBuilder.ToString();
		}

		internal static string FormatFileLine(ReleaseManifestFile releaseManifestFile) {
			return releaseManifestFile.Sha256+"|"+releaseManifestFile.Size.ToString(CultureInfo.InvariantCulture)+"|"+Clean(releaseManifestFile.RelativePath);
		}

		///<summary>Parses manifest text. Throws FormatException for anything malformed: unknown format, a missing required key, a bad hash or an
		///unsafe file path, a runtime part without its Runtime line or with a key, version or architecture that does not fit.</summary>
		public static ReleaseManifest Parse(string text) {
			if(text==null) {
				throw new FormatException("The release manifest is empty.");
			}
			ReleaseManifest releaseManifest=new ReleaseManifest();
			releaseManifest.Product="";
			bool hasFormat=false;
			ReleaseRuntimePart releaseRuntimePart=new ReleaseRuntimePart();
			//Each runtime part key once; a runtime part is there when any of its lines is.
			HashSet<string> hashSetRuntimePartKeys=new HashSet<string>();
			string[] arrayLines=text.Replace("\r\n","\n").Split('\n');
			foreach(string lineRaw in arrayLines) {
				string line=lineRaw.Trim();
				if(line=="" || line.StartsWith("#")) {
					continue;
				}
				int indexEquals=line.IndexOf('=');
				if(indexEquals<=0) {
					throw new FormatException("Bad release manifest line: "+line);
				}
				string key=line.Substring(0,indexEquals);
				string value=line.Substring(indexEquals+1);
				switch(key) {
					case "Format":
						if(value!=FORMAT.ToString(CultureInfo.InvariantCulture)) {
							throw new FormatException("Unknown release manifest format: "+value);
						}
						hasFormat=true;
						break;
					case "Product": releaseManifest.Product=value; break;
					case "ReleaseVersion": releaseManifest.ReleaseVersion=value; break;
					case "BuildId": releaseManifest.BuildId=value; break;
					case "CompatVersion": releaseManifest.CompatVersion=value; break;
					case "PublishedBy": releaseManifest.PublishedBy=value; break;
					case "ArchiveSha256":
						if(value!="" && !IsSha256Hex(value)) {
							throw new FormatException("Bad archive hash in the release manifest.");
						}
						releaseManifest.ArchiveSha256=value;
						break;
					case "PublishedUtc":
						releaseManifest.PublishedUtc=ParseUtc(value,"publish date");
						break;
					case "BuildUtc":
						releaseManifest.BuildUtc=ParseUtc(value,"build time");
						break;
					case "File":
						releaseManifest.ListFiles.Add(ParseFileLine(value));
						break;
					case "Runtime":
						if(releaseManifest.Runtime!=null) {
							throw new FormatException("The release manifest names its runtime twice.");
						}
						releaseManifest.Runtime=ReleaseRuntime.Parse(value);
						break;
					case "RuntimeVersion":
					case "RuntimeArchitectures":
					case "RuntimeArchiveSha256":
					case "RuntimeKey":
						if(!hashSetRuntimePartKeys.Add(key)) {
							throw new FormatException("The release manifest has "+key+" twice.");
						}
						ParseRuntimePartLine(releaseRuntimePart,key,value);
						break;
					case "RuntimeFile":
						hashSetRuntimePartKeys.Add(key);
						releaseRuntimePart.ListFiles.Add(ParseFileLine(value));
						break;
					default:
						throw new FormatException("Unknown release manifest key: "+key);
				}
			}
			if(!hasFormat) {
				throw new FormatException("The release manifest has no Format line.");
			}
			if(releaseManifest.ReleaseVersion=="" || releaseManifest.BuildId=="") {
				throw new FormatException("The release manifest has no release version or build id.");
			}
			if(releaseManifest.ListFiles.GroupBy(x => x.RelativePath,StringComparer.OrdinalIgnoreCase).Any(x => x.Count()>1)) {
				throw new FormatException("The release manifest lists a file twice.");
			}
			if(hashSetRuntimePartKeys.Count>0) {
				CheckRuntimePart(releaseManifest,releaseRuntimePart,hashSetRuntimePartKeys);
				releaseManifest.RuntimePart=releaseRuntimePart;
			}
			return releaseManifest;
		}

		internal static void ParseRuntimePartLine(ReleaseRuntimePart releaseRuntimePart,string key,string value) {
			switch(key) {
				case "RuntimeVersion":
					releaseRuntimePart.Version=value;
					break;
				case "RuntimeArchitectures":
					releaseRuntimePart.ListArchitectures=value.Split(' ').ToList();
					if(releaseRuntimePart.ListArchitectures.Any(x => !ReleaseRuntime.IsToken(x))
						|| releaseRuntimePart.ListArchitectures.Distinct(StringComparer.OrdinalIgnoreCase).Count()!=releaseRuntimePart.ListArchitectures.Count)
					{
						throw new FormatException("Bad runtime architectures in the release manifest: "+value);
					}
					break;
				case "RuntimeArchiveSha256":
					if(!IsSha256Hex(value)) {
						throw new FormatException("Bad runtime archive hash in the release manifest.");
					}
					releaseRuntimePart.ArchiveSha256=value.ToLowerInvariant();
					break;
				case "RuntimeKey":
					if(!IsSha256Hex(value)) {
						throw new FormatException("Bad runtime key in the release manifest.");
					}
					releaseRuntimePart.Key=value.ToLowerInvariant();
					break;
			}
		}

		///<summary>A runtime part needs the Runtime line and every one of its own lines: its version is the Runtime line's, the Runtime line's
		///architecture is the first of its architectures, it lists at least one file, none twice and none that the program part lists too, and its
		///key is the one its version and files give.</summary>
		internal static void CheckRuntimePart(ReleaseManifest releaseManifest,ReleaseRuntimePart releaseRuntimePart,HashSet<string> hashSetKeys) {
			if(releaseManifest.Runtime==null) {
				throw new FormatException("The release manifest has a runtime part but no Runtime line.");
			}
			foreach(string key in new[] { "RuntimeVersion","RuntimeArchitectures","RuntimeArchiveSha256","RuntimeKey","RuntimeFile" }) {
				if(!hashSetKeys.Contains(key)) {
					throw new FormatException("The runtime part of the release manifest has no "+key+" line.");
				}
			}
			if(releaseRuntimePart.Version!=releaseManifest.Runtime.Version) {
				throw new FormatException("The runtime part of the release manifest is version "+releaseRuntimePart.Version+", its Runtime line "
					+releaseManifest.Runtime.Version+".");
			}
			if(!releaseRuntimePart.ListArchitectures[0].Equals(releaseManifest.Runtime.Architecture,StringComparison.OrdinalIgnoreCase)) {
				throw new FormatException("The runtime part of the release manifest is for "+releaseRuntimePart.ListArchitectures[0]+", its Runtime line for "
					+releaseManifest.Runtime.Architecture+".");
			}
			HashSet<string> hashSetPaths=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			foreach(ReleaseManifestFile releaseManifestFile in releaseRuntimePart.ListFiles) {
				if(!hashSetPaths.Add(releaseManifestFile.RelativePath.Replace('/','\\'))) {
					throw new FormatException("The release manifest lists a runtime file twice.");
				}
			}
			if(releaseManifest.ListFiles.Any(x => hashSetPaths.Contains(x.RelativePath.Replace('/','\\')))) {
				throw new FormatException("The release manifest lists a file in both the program part and the runtime part.");
			}
			if(releaseRuntimePart.ComputeKey()!=releaseRuntimePart.Key) {
				throw new FormatException("The runtime key of the release manifest does not match its runtime files.");
			}
		}

		private static DateTime ParseUtc(string value,string what) {
			DateTime dateTime;
			if(!DateTime.TryParseExact(value,"yyyy-MM-dd'T'HH:mm:ss'Z'",CultureInfo.InvariantCulture,
				DateTimeStyles.AdjustToUniversal|DateTimeStyles.AssumeUniversal,out dateTime))
			{
				throw new FormatException("Bad "+what+" in the release manifest: "+value);
			}
			return dateTime;
		}

		internal static ReleaseManifestFile ParseFileLine(string value) {
			string[] arrayParts=value.Split(new char[] { '|' },3);
			if(arrayParts.Length!=3 || !IsSha256Hex(arrayParts[0])) {
				throw new FormatException("Bad file line in the release manifest: "+value);
			}
			long size;
			if(!long.TryParse(arrayParts[1],NumberStyles.None,CultureInfo.InvariantCulture,out size)) {
				throw new FormatException("Bad file size in the release manifest: "+value);
			}
			if(!IsSafeRelativePath(arrayParts[2])) {
				throw new FormatException("Unsafe file path in the release manifest: "+arrayParts[2]);
			}
			ReleaseManifestFile releaseManifestFile=new ReleaseManifestFile();
			releaseManifestFile.Sha256=arrayParts[0].ToLowerInvariant();
			releaseManifestFile.Size=size;
			releaseManifestFile.RelativePath=arrayParts[2];
			return releaseManifestFile;
		}

		///<summary>True if this manifest names our product. A manifest for any other product is never used.</summary>
		public bool IsOurProduct() {
			return Product==PRODUCT_NAME;
		}

		///<summary>True for a relative path that stays inside the folder it is combined with: no drive, no leading separator, no "." or ".." parts and
		///no characters that are invalid in file names. Separators may be '\' or '/'.</summary>
		public static bool IsSafeRelativePath(string relativePath) {
			if(string.IsNullOrWhiteSpace(relativePath) || relativePath.Length>400) {
				return false;
			}
			if(relativePath.StartsWith("\\") || relativePath.StartsWith("/") || relativePath.Contains(":")) {
				return false;
			}
			string[] arrayParts=relativePath.Split('\\','/');
			char[] arrayInvalid=Path.GetInvalidFileNameChars();
			foreach(string part in arrayParts) {
				if(part=="" || part=="." || part==".." || part.Trim()!=part || part.IndexOfAny(arrayInvalid)>=0) {
					return false;
				}
			}
			return true;
		}

		///<summary>Lower-case hex SHA-256 of the bytes.</summary>
		public static string ComputeSha256(byte[] arrayBytes) {
			using(SHA256 sha256=SHA256.Create()) {
				return ToHex(sha256.ComputeHash(arrayBytes));
			}
		}

		///<summary>Lower-case hex SHA-256 of the stream from its current position to its end.</summary>
		public static string ComputeSha256(Stream stream) {
			using(SHA256 sha256=SHA256.Create()) {
				return ToHex(sha256.ComputeHash(stream));
			}
		}

		///<summary>Lower-case hex SHA-256 of the file.</summary>
		public static string ComputeSha256File(string path) {
			using(FileStream fileStream=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.Read)) {
				return ComputeSha256(fileStream);
			}
		}

		///<summary>Checks the files in the folder against this manifest: each listed file must exist with the listed size and SHA-256. Files that are
		///not listed (other than the manifest file itself) are reported too, because the copier would install them. Returns the problems found; an
		///empty list means the folder holds exactly this release.</summary>
		public List<string> VerifyFolder(string folder) {
			return VerifyFolder(folder,includeRuntimePart:false);
		}

		///<summary>VerifyFolder for a folder that holds the program part and, with includeRuntimePart, the runtime part too (S3b-7: a pull whose
		///published runtime key differs from this workstation's downloads both parts into one folder). With includeRuntimePart every RuntimeFile line
		///must be in the folder with its size and SHA-256, counts as listed, and the runtime part's own manifest (ReleaseRuntimePart.MANIFEST_FILE_NAME)
		///is allowed beside this one; a manifest without a runtime part is then a problem. Without it, a runtime file in the folder is "Not in the
		///manifest" as any other file, so a program part download never carries runtime files of its own.</summary>
		public List<string> VerifyFolder(string folder,bool includeRuntimePart) {
			List<string> listProblems=new List<string>();
			if(!IsOurProduct()) {
				listProblems.Add("The manifest is for another product: "+Product);
				return listProblems;
			}
			HashSet<string> hashSetListed=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			List<ReleaseManifestFile> listFilesChecked=ListFiles.ToList();
			if(includeRuntimePart) {
				if(RuntimePart==null) {
					listProblems.Add("The release names no runtime part, but the folder is checked with one.");
					return listProblems;
				}
				listFilesChecked.AddRange(RuntimePart.ListFiles);
			}
			foreach(ReleaseManifestFile releaseManifestFile in listFilesChecked) {
				if(IsSafeRelativePath(releaseManifestFile.RelativePath)) {
					hashSetListed.Add(releaseManifestFile.RelativePath.Replace('/','\\'));
				}
				listProblems.AddRange(VerifyFile(folder,releaseManifestFile,""));
			}
			string folderFull=Path.GetFullPath(folder).TrimEnd('\\')+"\\";
			foreach(string pathFile in Directory.GetFiles(folder,"*",SearchOption.AllDirectories)) {
				string relative=Path.GetFullPath(pathFile).Substring(folderFull.Length);
				if(relative.Equals(FILE_NAME,StringComparison.OrdinalIgnoreCase)) {
					continue;
				}
				if(includeRuntimePart && relative.Equals(ReleaseRuntimePart.MANIFEST_FILE_NAME,StringComparison.OrdinalIgnoreCase)) {
					continue;
				}
				if(!hashSetListed.Contains(relative)) {
					listProblems.Add("Not in the manifest: "+relative);
				}
			}
			return listProblems;
		}

		///<summary>The problems of one listed file in the folder: an unsafe path, missing, the wrong size or the wrong SHA-256. what ("" or "runtime
		///file") goes before the path in each problem.</summary>
		internal static List<string> VerifyFile(string folder,ReleaseManifestFile releaseManifestFile,string what) {
			List<string> listProblems=new List<string>();
			string prefix=what=="" ? "" : what+" ";
			if(!IsSafeRelativePath(releaseManifestFile.RelativePath)) {
				listProblems.Add("Unsafe path: "+prefix+releaseManifestFile.RelativePath);
				return listProblems;
			}
			string path=Path.Combine(folder,releaseManifestFile.RelativePath.Replace('/','\\'));
			if(!File.Exists(path)) {
				listProblems.Add("Missing: "+prefix+releaseManifestFile.RelativePath);
				return listProblems;
			}
			FileInfo fileInfo=new FileInfo(path);
			if(fileInfo.Length!=releaseManifestFile.Size) {
				listProblems.Add("Wrong size: "+prefix+releaseManifestFile.RelativePath);
				return listProblems;
			}
			if(ComputeSha256File(path)!=releaseManifestFile.Sha256) {
				listProblems.Add("Wrong SHA-256: "+prefix+releaseManifestFile.RelativePath);
			}
			return listProblems;
		}

		public static bool IsSha256Hex(string value) {
			if(value==null || value.Length!=64) {
				return false;
			}
			foreach(char c in value) {
				bool isHex=(c>='0' && c<='9') || (c>='a' && c<='f') || (c>='A' && c<='F');
				if(!isHex) {
					return false;
				}
			}
			return true;
		}

		private static string ToHex(byte[] arrayBytes) {
			StringBuilder stringBuilder=new StringBuilder(arrayBytes.Length*2);
			foreach(byte b in arrayBytes) {
				stringBuilder.Append(b.ToString("x2",CultureInfo.InvariantCulture));
			}
			return stringBuilder.ToString();
		}

		///<summary>Values are single-line; line breaks would let a value forge another key.</summary>
		internal static string Clean(string value) {
			return (value??"").Replace("\r"," ").Replace("\n"," ");
		}
	}
}
