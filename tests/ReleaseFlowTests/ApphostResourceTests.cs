// The Win32 resources of every staged .exe (S3a-10 (v) of docs/specs/net10-plan.md): the requested execution level of RT_MANIFEST, the file and
// product version of RT_VERSION and the icon of RT_GROUP_ICON, compared with the record that today's net48 Release build wrote
// (Fixtures\ApphostResources.net48.txt), so that stage 3b proves its apphosts carry the same. Inconclusive when no Release build is there: the
// stage gate runs the Release dry run (tools\Build-Release.ps1 -SkipInstaller) before the Full run.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ReleaseFlowTests {
	///<summary>The resources of one .exe that the record keeps.</summary>
	internal class ExeResources {
		public string FileName="";
		///<summary>The level attribute of requestedExecutionLevel in RT_MANIFEST 1, or "none" without one.</summary>
		public string ExecutionLevel="";
		///<summary>The uiAccess attribute of requestedExecutionLevel, or "none".</summary>
		public string UiAccess="";
		public string FileVersion="";
		public string ProductVersion="";
		///<summary>The SHA-256 of each RT_GROUP_ICON as an .ico file (its entries and RT_ICON images, without the resource ids), in resource order.
		///</summary>
		public List<string> ListIconSha256=new List<string>();

		///<summary>The record's line: file|level|uiAccess|FileVersion|ProductVersion|icon SHA-256 values separated by ','.</summary>
		public string ToLine() {
			return FileName+"|"+ExecutionLevel+"|"+UiAccess+"|"+FileVersion+"|"+ProductVersion+"|"+(ListIconSha256.Count==0 ? "none" : string.Join(",",ListIconSha256));
		}
	}

	///<summary>Reads RT_MANIFEST, RT_VERSION and RT_GROUP_ICON from an .exe loaded as a data file, so the architecture of the file does not matter
	///(an x64 apphost reads the same from an x86 process).</summary>
	internal static class ExeResourceReader {
		private const uint LOAD_LIBRARY_AS_DATAFILE=0x2;
		private const uint LOAD_LIBRARY_AS_IMAGE_RESOURCE=0x20;
		private static readonly IntPtr RT_ICON=(IntPtr)3;
		private static readonly IntPtr RT_GROUP_ICON=(IntPtr)14;
		private static readonly IntPtr RT_MANIFEST=(IntPtr)24;

		[DllImport("kernel32.dll",SetLastError=true,CharSet=CharSet.Unicode)]
		private static extern IntPtr LoadLibraryEx(string lpFileName,IntPtr hFile,uint dwFlags);
		[DllImport("kernel32.dll",SetLastError=true)]
		private static extern bool FreeLibrary(IntPtr hModule);
		[DllImport("kernel32.dll",SetLastError=true)]
		private static extern IntPtr FindResource(IntPtr hModule,IntPtr lpName,IntPtr lpType);
		[DllImport("kernel32.dll",SetLastError=true)]
		private static extern IntPtr LoadResource(IntPtr hModule,IntPtr hResInfo);
		[DllImport("kernel32.dll",SetLastError=true)]
		private static extern IntPtr LockResource(IntPtr hResData);
		[DllImport("kernel32.dll",SetLastError=true)]
		private static extern uint SizeofResource(IntPtr hModule,IntPtr hResInfo);
		private delegate bool EnumResNameProc(IntPtr hModule,IntPtr lpType,IntPtr lpName,IntPtr lParam);
		[DllImport("kernel32.dll",SetLastError=true,CharSet=CharSet.Unicode)]
		private static extern bool EnumResourceNames(IntPtr hModule,IntPtr lpType,EnumResNameProc lpEnumFunc,IntPtr lParam);

		public static ExeResources Read(string path) {
			ExeResources exeResources=new ExeResources();
			exeResources.FileName=Path.GetFileName(path);
			FileVersionInfo fileVersionInfo=FileVersionInfo.GetVersionInfo(path);
			exeResources.FileVersion=fileVersionInfo.FileVersion??"";
			exeResources.ProductVersion=fileVersionInfo.ProductVersion??"";
			IntPtr hModule=LoadLibraryEx(path,IntPtr.Zero,LOAD_LIBRARY_AS_DATAFILE|LOAD_LIBRARY_AS_IMAGE_RESOURCE);
			if(hModule==IntPtr.Zero) {
				throw new Win32Exception(Marshal.GetLastWin32Error(),"LoadLibraryEx "+path);
			}
			try {
				ReadManifest(hModule,exeResources);
				foreach(byte[] arrayGroup in ReadAll(hModule,RT_GROUP_ICON)) {
					exeResources.ListIconSha256.Add(Sha256(ToIcoFile(hModule,arrayGroup)));
				}
			}
			finally {
				FreeLibrary(hModule);
			}
			return exeResources;
		}

		private static void ReadManifest(IntPtr hModule,ExeResources exeResources) {
			exeResources.ExecutionLevel="none";
			exeResources.UiAccess="none";
			byte[] arrayManifest=ReadById(hModule,RT_MANIFEST,1);
			if(arrayManifest==null) {
				return;
			}
			string text=Encoding.UTF8.GetString(arrayManifest).TrimStart('﻿');
			XmlDocument xmlDocument=new XmlDocument();
			xmlDocument.LoadXml(text);
			XmlElement xmlElement=xmlDocument.SelectSingleNode("//*[local-name()='requestedExecutionLevel']") as XmlElement;
			if(xmlElement==null) {
				return;
			}
			exeResources.ExecutionLevel=xmlElement.HasAttribute("level") ? xmlElement.GetAttribute("level") : "none";
			exeResources.UiAccess=xmlElement.HasAttribute("uiAccess") ? xmlElement.GetAttribute("uiAccess") : "none";
		}

		///<summary>The bytes of every resource of the type, in the order the resource directory lists them.</summary>
		private static List<byte[]> ReadAll(IntPtr hModule,IntPtr type) {
			List<byte[]> listResources=new List<byte[]>();
			//The name pointer is valid only during the callback, so each resource is read there.
			EnumResNameProc enumResNameProc=(h,t,name,l) => {
				listResources.Add(ReadResource(hModule,FindResource(hModule,name,type)));
				return true;
			};
			EnumResourceNames(hModule,type,enumResNameProc,IntPtr.Zero);//False when the file has none of the type.
			GC.KeepAlive(enumResNameProc);
			return listResources;
		}

		private static byte[] ReadById(IntPtr hModule,IntPtr type,int id) {
			IntPtr hResInfo=FindResource(hModule,(IntPtr)id,type);
			return hResInfo==IntPtr.Zero ? null : ReadResource(hModule,hResInfo);
		}

		private static byte[] ReadResource(IntPtr hModule,IntPtr hResInfo) {
			if(hResInfo==IntPtr.Zero) {
				throw new Win32Exception(Marshal.GetLastWin32Error(),"FindResource");
			}
			uint size=SizeofResource(hModule,hResInfo);
			IntPtr pointer=LockResource(LoadResource(hModule,hResInfo));
			if(pointer==IntPtr.Zero) {
				throw new Win32Exception(Marshal.GetLastWin32Error(),"LoadResource");
			}
			byte[] arrayBytes=new byte[size];
			Marshal.Copy(pointer,arrayBytes,0,(int)size);
			return arrayBytes;
		}

		///<summary>The icon group as an .ico file: ICONDIR, one ICONDIRENTRY per image (GRPICONDIRENTRY with the image's offset in place of its
		///resource id) and the RT_ICON images, so the hash covers the icon and not the ids a compiler gives its resources.</summary>
		private static byte[] ToIcoFile(IntPtr hModule,byte[] arrayGroup) {
			int count=BitConverter.ToUInt16(arrayGroup,4);
			List<byte[]> listImages=new List<byte[]>();
			for(int i=0;i<count;i++) {
				int id=BitConverter.ToUInt16(arrayGroup,6+i*14+12);
				byte[] arrayImage=ReadById(hModule,RT_ICON,id);
				if(arrayImage==null) {
					throw new InvalidDataException("RT_GROUP_ICON names RT_ICON "+id+", which the file does not have.");
				}
				listImages.Add(arrayImage);
			}
			using(MemoryStream memoryStream=new MemoryStream())
			using(BinaryWriter binaryWriter=new BinaryWriter(memoryStream)) {
				binaryWriter.Write((ushort)0);
				binaryWriter.Write((ushort)1);
				binaryWriter.Write((ushort)count);
				int offset=6+16*count;
				for(int i=0;i<count;i++) {
					binaryWriter.Write(arrayGroup,6+i*14,12);//Width, height, colours, reserved, planes, bit count, bytes in the image.
					binaryWriter.Write(offset);
					offset+=listImages[i].Length;
				}
				foreach(byte[] arrayImage in listImages) {
					binaryWriter.Write(arrayImage);
				}
				binaryWriter.Flush();
				return memoryStream.ToArray();
			}
		}

		public static string Sha256(byte[] arrayBytes) {
			using(SHA256 sha256=SHA256.Create()) {
				return string.Concat(sha256.ComputeHash(arrayBytes).Select(x => x.ToString("x2",CultureInfo.InvariantCulture)));
			}
		}
	}

	[TestClass]
	public class ApphostResourceTests {
		///<summary>Set to 1 to write the record from the Release build (on .NET Framework only, which the record is of).</summary>
		public const string RECORD_VARIABLE="OPMS_APPHOST_RECORD";
		///<summary>A staging folder of Build-Release.ps1 to read instead of OpenPmsDental\bin\Release, whose files it copies.</summary>
		public const string STAGING_VARIABLE="OPMS_RELEASE_STAGING";
		private static readonly string[] _arrayRecordPath=new[] { "tests","ReleaseFlowTests","Fixtures","ApphostResources.net48.txt" };

		///<summary>The .exe files Build-Release.ps1 stages (its allow-lists, from -ListFiles).</summary>
		private static List<string> GetStagedExeNames() {
			ProcessStartInfo processStartInfo=new ProcessStartInfo("powershell.exe","-NoProfile -ExecutionPolicy Bypass -File \""
				+Path.Combine(TestPaths.RepoRoot,"tools","Build-Release.ps1")+"\" -ListFiles");
			processStartInfo.UseShellExecute=false;
			processStartInfo.RedirectStandardOutput=true;
			processStartInfo.RedirectStandardError=true;
			processStartInfo.CreateNoWindow=true;
			using(Process process=Process.Start(processStartInfo)) {
				System.Threading.Tasks.Task<string> taskError=process.StandardError.ReadToEndAsync();
				string output=process.StandardOutput.ReadToEnd();
				process.WaitForExit();
				Assert.AreEqual(0,process.ExitCode,"Build-Release.ps1 -ListFiles: "+taskError.Result);
				return output.Split('\n').Select(x => x.Trim()).Where(x => x.StartsWith("own|") || x.StartsWith("third|"))
					.Select(x => x.Substring(x.IndexOf('|')+1))
					.Where(x => x.EndsWith(".exe",StringComparison.OrdinalIgnoreCase))
					.OrderBy(x => x,StringComparer.OrdinalIgnoreCase).ToList();
			}
		}

		///<summary>The folder whose .exe files are read: OPMS_RELEASE_STAGING, or the Release output Build-Release.ps1 stages from. Inconclusive
		///when it has no Release build, and when that build is stale (S3a-10f): it lacks one of the exe files (the staged ones, and in a compare
		///run the record's too), or one of them was written before the newest commit that touched its project (the folder named as the exe) or
		///SharedAssemblyInfo.cs. The message lists the exe files' write times. Without git the commit check is skipped.</summary>
		internal static string GetReleaseFolder(List<string> listExeNames,bool isRecording) {
			string folder=Environment.GetEnvironmentVariable(STAGING_VARIABLE);
			if(string.IsNullOrWhiteSpace(folder)) {
				folder=Path.Combine(TestPaths.RepoRoot,"OpenPmsDental","bin","Release");
			}
			if(!File.Exists(Path.Combine(folder,"OpenPmsDental.exe"))) {
				Assert.Inconclusive("No Release build at "+folder+". The resources are read from the Release build, which the build lane does not "
					+"make: run the Release dry run first (tools\\Build-Release.ps1 -BuildLane <odbuild.ps1> -OutDir <folder> -SkipInstaller), as the "
					+"stage gate does before the Full run.");
			}
			List<string> listNames=listExeNames.ToList();
			if(!isRecording) {
				listNames=listNames.Union(ReadRecord().Keys.Select(x => GetStagedPath(x)),StringComparer.OrdinalIgnoreCase).ToList();
			}
			listNames=listNames.OrderBy(x => x,StringComparer.OrdinalIgnoreCase).ToList();
			List<string> listProblems=new List<string>();
			List<string> listTimes=new List<string>();
			foreach(string name in listNames) {
				string path=Path.Combine(folder,name);
				if(!File.Exists(path)) {
					listProblems.Add(name+" is missing");
					listTimes.Add(name+": missing");
					continue;
				}
				DateTime dateTimeWriteUtc=File.GetLastWriteTimeUtc(path);
				string project=Path.GetFileNameWithoutExtension(name);
				DateTime? dateTimeCommitUtc=GetNewestCommitUtc(project,"SharedAssemblyInfo.cs");
				listTimes.Add(name+": written "+FormatUtc(dateTimeWriteUtc)
					+(dateTimeCommitUtc==null ? "" : ", newest commit touching "+project+" or SharedAssemblyInfo.cs "+FormatUtc(dateTimeCommitUtc.Value)));
				if(dateTimeCommitUtc!=null && dateTimeWriteUtc<dateTimeCommitUtc.Value) {
					listProblems.Add(name+" is older than the newest commit touching "+project);
				}
			}
			if(listProblems.Count>0) {
				Assert.Inconclusive("The Release build at "+folder+" is stale ("+string.Join("; ",listProblems)+"), so its resources say nothing about "
					+"this commit: run tools/Build-Release.ps1 first (tools\\Build-Release.ps1 -BuildLane <odbuild.ps1> -OutDir <folder> -SkipInstaller), "
					+"as the stage gate does before the Full run, then run this test again.\nWrite times of the exe files:\n"+string.Join("\n",listTimes));
			}
			return folder;
		}

		private static string FormatUtc(DateTime dateTimeUtc) {
			return dateTimeUtc.ToString("yyyy-MM-dd HH:mm:ss",CultureInfo.InvariantCulture)+" UTC";
		}

		///<summary>The committer time (UTC) of the newest commit that touched any of the paths (relative to the repository root), from git log. Null
		///when git cannot tell (no git on the PATH, not a work tree, no such commit).</summary>
		internal static DateTime? GetNewestCommitUtc(params string[] arrayPaths) {
			ProcessStartInfo processStartInfo=new ProcessStartInfo("git.exe","log -1 --format=%ct -- "+string.Join(" ",arrayPaths.Select(x => "\""+x+"\"")));
			processStartInfo.WorkingDirectory=TestPaths.RepoRoot;
			processStartInfo.UseShellExecute=false;
			processStartInfo.RedirectStandardOutput=true;
			processStartInfo.RedirectStandardError=true;
			processStartInfo.CreateNoWindow=true;
			try {
				using(Process process=Process.Start(processStartInfo)) {
					System.Threading.Tasks.Task<string> taskError=process.StandardError.ReadToEndAsync();
					string output=process.StandardOutput.ReadToEnd().Trim();
					if(!process.WaitForExit(60000) || process.ExitCode!=0) {
						return null;
					}
					long seconds;
					if(!long.TryParse(output,NumberStyles.None,CultureInfo.InvariantCulture,out seconds)) {
						return null;
					}
					return new DateTime(1970,1,1,0,0,0,DateTimeKind.Utc).AddSeconds(seconds);
				}
			}
			catch(Win32Exception) {
				return null;//No git.exe on the PATH.
			}
		}

		///<summary>Where the release stages an exe the record names by its file name: since stage 3b the 32-bit helper is in the sub folder Helper32\
		///(S3b-4, S3b-7); every other exe is at the top. The record keeps the net48 file names.</summary>
		internal static string GetStagedPath(string fileName) {
			if(Environment.Version.Major>=5 && fileName.Equals("OpenPmsDentalHelper32.exe",StringComparison.OrdinalIgnoreCase)) {
				return @"Helper32\"+fileName;
			}
			return fileName;
		}

		internal static Dictionary<string,string> ReadRecord() {
			Dictionary<string,string> dictLines=new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);
			foreach(string lineRaw in TestPaths.ReadSource(_arrayRecordPath).Split('\n')) {
				string line=lineRaw.Trim();
				if(line=="" || line.StartsWith("#")) {
					continue;
				}
				dictLines.Add(line.Substring(0,line.IndexOf('|')),line);
			}
			return dictLines;
		}

		[TestMethod]
		public void StagedExes_CarryTheResourcesOfTheNet48Record() {
			List<string> listExeNames=GetStagedExeNames();
			Assert.IsTrue(listExeNames.Count>=4,"Build-Release.ps1 stages "+string.Join(", ",listExeNames));
			bool isRecording=Environment.GetEnvironmentVariable(RECORD_VARIABLE)=="1";
			string folder=GetReleaseFolder(listExeNames,isRecording);
			List<ExeResources> listResources=listExeNames.Select(x => ExeResourceReader.Read(Path.Combine(folder,x))).ToList();
			if(isRecording) {
				Assert.IsTrue(Environment.Version.Major<5,"The record is of the net48 build; 3b compares with it and never records.");
				StringBuilder stringBuilder=new StringBuilder();
				stringBuilder.Append("# The Win32 resources of every .exe that tools\\Build-Release.ps1 stages, recorded from the net48 Release build by\n");
				stringBuilder.Append("# ApphostResourceTests with "+RECORD_VARIABLE+"=1 (S3a-10, docs/port-notes/S3a-10.md). Stage 3b's apphosts must carry the same.\n");
				stringBuilder.Append("# file|requestedExecutionLevel|uiAccess|FileVersion|ProductVersion|SHA-256 of each RT_GROUP_ICON as an .ico file\n");
				foreach(ExeResources exeResources in listResources) {
					stringBuilder.Append(exeResources.ToLine()+"\n");
				}
				File.WriteAllText(Path.Combine(TestPaths.RepoRoot,Path.Combine(_arrayRecordPath)),stringBuilder.ToString(),new UTF8Encoding(false));
				Assert.Inconclusive("Recorded "+listResources.Count+" files from "+folder+". Review and commit the record.");
			}
			Dictionary<string,string> dictRecord=ReadRecord();
			CollectionAssert.AreEquivalent(dictRecord.Keys.Select(x => GetStagedPath(x)).ToArray(),listExeNames.ToArray(),
				"The record names every staged .exe, and only those (the helper in Helper32\\ since stage 3b).");
			List<string> listDifferences=new List<string>();
			foreach(ExeResources exeResources in listResources) {
				if(exeResources.ToLine()!=dictRecord[exeResources.FileName]) {
					listDifferences.Add("record: "+dictRecord[exeResources.FileName]+"\n  built: "+exeResources.ToLine());
				}
			}
			Assert.AreEqual(0,listDifferences.Count,"Resources differ from the net48 record:\n"+string.Join("\n",listDifferences));
		}

		///<summary>The values the plan names, apart from the record: the three tools that change the installation or the services ask for
		///administrator rights, the program does not, and the program's file version is the compatibility version.</summary>
		[TestMethod]
		public void Record_HasTheElevationAndVersionsThePlanNames() {
			Dictionary<string,string> dictRecord=ReadRecord();
			//Every staged exe is built with ApplicationIcon branding\app.ico, and its icon group as an .ico file is that file byte for byte.
			string shaIcon=ExeResourceReader.Sha256(File.ReadAllBytes(Path.Combine(TestPaths.RepoRoot,"branding","app.ico")));
			foreach(KeyValuePair<string,string> pair in dictRecord) {
				Assert.AreEqual(shaIcon,pair.Value.Split('|')[5],pair.Key+": the icon is branding\\app.ico.");
			}
			foreach(string fileName in new[] { "OpenPmsDentalDbSettings.exe","ServiceManager.exe","UpdateFileCopier.exe" }) {
				Assert.AreEqual("requireAdministrator",dictRecord[fileName].Split('|')[1],fileName);
				Assert.AreEqual("false",dictRecord[fileName].Split('|')[2],fileName);
			}
			Assert.AreEqual("asInvoker",dictRecord["OpenPmsDental.exe"].Split('|')[1]);
			Assert.AreEqual("24.3.63.0",dictRecord["OpenPmsDental.exe"].Split('|')[3],"ReleaseFilesTests: the compatibility version.");
			foreach(KeyValuePair<string,string> pair in dictRecord) {
				string[] arrayParts=pair.Value.Split('|');
				Assert.AreEqual(6,arrayParts.Length,pair.Value);
				Assert.AreEqual("0.0",arrayParts[4],pair.Key+": the product version is the release version.");
				Assert.AreNotEqual("none",arrayParts[5],pair.Key+" has an icon.");
				Assert.IsTrue(arrayParts[5].Split(',').All(x => x.Length==64),pair.Value);
			}
		}

		///<summary>The reader on a file whose resources are known: an .exe of the .NET Framework itself would do, but one built here is used, the
		///copier, from this configuration's output (requireAdministrator, an icon).</summary>
		[TestMethod]
		public void Reader_ReadsTheCopiersManifestAndIcon() {
			string configuration=Path.GetFileName(Path.GetDirectoryName(typeof(ApphostResourceTests).Assembly.Location));
			string path=Path.Combine(TestPaths.RepoRoot,"UpdateFileCopier","bin",configuration,"UpdateFileCopier.exe");
			ProjectFileTests.BuildOutput.RequireFile(path,path+" is not built. Build OpenDental.Local.slnf ("+configuration+"|x86) first.");
			ExeResources exeResources=ExeResourceReader.Read(path);
			Assert.AreEqual("requireAdministrator",exeResources.ExecutionLevel);
			Assert.AreEqual("false",exeResources.UiAccess);
			Assert.AreEqual(1,exeResources.ListIconSha256.Count);
			Assert.AreEqual("0.0",exeResources.ProductVersion);
		}

		///<summary>A stale Release output (S3a-10f: one from before an exe was staged, or older than a commit to an exe's project) ends the record
		///test as inconclusive with "run tools/Build-Release.ps1 first" and the exe files' write times, never with a FileNotFoundException.
		///Synthetic files in a staging folder named by OPMS_RELEASE_STAGING; nothing is read as a resource.</summary>
		[TestMethod]
		public void StaleReleaseOutput_IsInconclusive_WithTheWriteTimes() {
			List<string> listRecordNames=ReadRecord().Keys.OrderBy(x => x,StringComparer.OrdinalIgnoreCase).ToList();
			string valueBefore=Environment.GetEnvironmentVariable(STAGING_VARIABLE);
			string folder=TestPaths.NewTempFolder("s3a10f_stale");
			try {
				Environment.SetEnvironmentVariable(STAGING_VARIABLE,folder);
				//An output from before the helper existed: the program, but not every exe the record names.
				File.WriteAllText(Path.Combine(folder,"OpenPmsDental.exe"),"synthetic");
				string message=GetInconclusiveMessage(() => GetReleaseFolder(new List<string> { "OpenPmsDental.exe" },isRecording:false));
				StringAssert.Contains(message,"run tools/Build-Release.ps1 first");
				foreach(string name in listRecordNames.Where(x => x!="OpenPmsDental.exe")) {
					StringAssert.Contains(message,name+" is missing");
					StringAssert.Contains(message,name+": missing");
				}
				StringAssert.Contains(message,"OpenPmsDental.exe: written ");
				//Every exe there and newer than any commit: the folder is used.
				foreach(string name in listRecordNames) {
					File.WriteAllText(Path.Combine(folder,name),"synthetic");
					File.SetLastWriteTimeUtc(Path.Combine(folder,name),DateTime.UtcNow.AddDays(1));
				}
				Assert.AreEqual(folder,GetReleaseFolder(listRecordNames,isRecording:false));
				//Older than the newest commit that touched its project.
				DateTime? dateTimeCommitUtc=GetNewestCommitUtc("OpenPmsDental","SharedAssemblyInfo.cs");
				if(dateTimeCommitUtc==null) {
					Assert.Inconclusive("git cannot tell the newest commit of OpenPmsDental here (no git.exe on the PATH, or not a work tree): the commit "
						+"check is skipped then. The missing-file check passed.");
				}
				File.SetLastWriteTimeUtc(Path.Combine(folder,"OpenPmsDental.exe"),dateTimeCommitUtc.Value.AddMinutes(-1));
				message=GetInconclusiveMessage(() => GetReleaseFolder(listRecordNames,isRecording:false));
				StringAssert.Contains(message,"run tools/Build-Release.ps1 first");
				StringAssert.Contains(message,"OpenPmsDental.exe is older than the newest commit touching OpenPmsDental");
				StringAssert.Contains(message,"OpenPmsDental.exe: written "+FormatUtc(dateTimeCommitUtc.Value.AddMinutes(-1))+", newest commit touching "
					+"OpenPmsDental or SharedAssemblyInfo.cs "+FormatUtc(dateTimeCommitUtc.Value));
				Assert.IsFalse(message.Contains("ServiceManager.exe is older"),message);
			}
			finally {
				Environment.SetEnvironmentVariable(STAGING_VARIABLE,valueBefore);
				try {
					Directory.Delete(folder,true);
				}
				catch(Exception) {
				}
			}
		}

		private static string GetInconclusiveMessage(Action action) {
			try {
				action();
			}
			catch(AssertInconclusiveException ex) {
				return ex.Message;
			}
			Assert.Fail("Not inconclusive.");
			return "";
		}
	}
}
