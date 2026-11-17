// The local check of E-20 (docs/port-notes/EHR-6.md, section 0; docs/specs/ehr-2027-measures-plan.md, section 7.1): the program's importer
// reads the office's own download of the 2026-05-14 release (the value-set workbook sorted by CMS ID, the Direct Reference Codes file and the
// Code System Versions file) and writes it into a scratch database of its own, ehr6ar_ plus this run's id on the local MariaDB (127.0.0.1:3306,
// built as Support/ReleaseDb.cs builds its own, dropped after the test). It runs only when EHR_VSAC_RELEASE_DIR names the folder that holds the
// three files; otherwise it is inconclusive, so CI and other machines never need them. The files are licensed content: nothing of them is in the
// repository, and the test writes only counts, versions and the report's checks (never a code or a description) to its output, and to the
// file EHR6AR_RESULT_FILE names, when set.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using DataConnectionBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySqlConnector;
using OpenPmsDentalBusiness;
using OpenPmsDentalBusiness.ForkMigrations;

namespace EhrTests {
	[TestClass]
	public class RealReleaseTests {
		///<summary>The folder of the office's download (the three files as VSAC gives them). Unset: the test is inconclusive.</summary>
		private const string ENV_RELEASE_DIR="EHR_VSAC_RELEASE_DIR";
		private const string ENV_RESULT_FILE="EHR6AR_RESULT_FILE";
		private const string PREFIX="ehr6ar_";
		private static readonly string _dbName=PREFIX+DateTime.UtcNow.ToString("yyMMddHHmmss",CultureInfo.InvariantCulture)+"_"
			+Process.GetCurrentProcess().Id+"_"+new Random().Next(1000,10000);
		private static bool _isBuilt;

		[ClassCleanup]
		public static void ClassCleanup() {
			ScratchDb.UseDirectConnection();
			EhrCodes.ClearList();
			if(_isBuilt) {
				Drop(_dbName);
			}
		}

		[TestMethod]
		public void RealRelease_TheOfficesThreeFiles_ImportSevenOfSeven_WhenTheFolderIsSet() {
			string folder=Environment.GetEnvironmentVariable(ENV_RELEASE_DIR);
			if(string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder)) {
				Assert.Inconclusive(ENV_RELEASE_DIR+" names no folder: the office's own download is not here (E-20's local check).");
			}
			string pathValueSets=FindOne(folder,"ec_*_cms_*.xlsx.zip");
			string pathDirect=FindOne(folder,"drc_cms_*.xlsx.zip");
			string pathVersions=FindOne(folder,"cs_versions_cms_*.xlsx.zip");
			string skipReason=ScratchDb.GetSkipReason();
			if(skipReason!=null) {
				Assert.Inconclusive(skipReason);
			}
			StringBuilder stringBuilderOut=new StringBuilder();
			CultureInfo cultureBefore=Thread.CurrentThread.CurrentCulture;
			Thread.CurrentThread.CurrentCulture=new CultureInfo("en-US");
			try {
				BuildDb();
				//The pick, read as the Code System Importer reads it (FileStreams, one after another).
				List<string> listPaths=new List<string> { pathValueSets,pathDirect,pathVersions };
				GC.Collect();
				long bytesManagedBefore=GC.GetTotalMemory(true);
				Process process=Process.GetCurrentProcess();
				process.Refresh();
				long bytesPrivateBefore=process.PrivateMemorySize64;
				//The import's own peak, sampled while it runs: the process's peak working set is that of every test the host ran before.
				long bytesPrivatePeak=bytesPrivateBefore;
				long bytesManagedPeak=bytesManagedBefore;
				ManualResetEvent manualResetEventDone=new ManualResetEvent(false);
				Thread threadSampler=new Thread(() => {
					do {
						Process processNow=Process.GetCurrentProcess();
						bytesPrivatePeak=Math.Max(bytesPrivatePeak,processNow.PrivateMemorySize64);
						bytesManagedPeak=Math.Max(bytesManagedPeak,GC.GetTotalMemory(false));
						processNow.Dispose();
					} while(!manualResetEventDone.WaitOne(50));
				});
				threadSampler.IsBackground=true;
				threadSampler.Start();
				Stopwatch stopwatchParse=Stopwatch.StartNew();
				EhrValueSetPick pick=EhrValueSetImport.ParsePick(listPaths,index => new FileStream(listPaths[index],FileMode.Open,FileAccess.Read,FileShare.Read));
				stopwatchParse.Stop();
				string report=EhrValueSetImport.GetReport(pick);
				Assert.IsFalse(pick.IsRefused,ReportWithoutLists(report));
				Stopwatch stopwatchImport=Stopwatch.StartNew();
				string error=EhrValueSetImport.Import(pick);
				stopwatchImport.Stop();
				manualResetEventDone.Set();
				threadSampler.Join();
				long bytesManagedPick=GC.GetTotalMemory(true);
				Assert.AreEqual("",error);
				process.Refresh();
				long bytesPeakWorkingSet=process.PeakWorkingSet64;
				//The counts.
				EhrValueSetFile fileValueSets=pick.FileValueSets;
				stringBuilderOut.AppendLine("E-20 local check, "+DateTime.Now.ToString("yyyy-MM-dd HH:mm",CultureInfo.InvariantCulture)+", 64-bit process: "
					+Environment.Is64BitProcess);
				foreach(EhrValueSetFile file in pick.ListFiles) {
					stringBuilderOut.AppendLine("file "+file.FileName+" "+file.CountBytes.ToString("N0",CultureInfo.InvariantCulture)+" bytes, SHA-256 "+file.Sha256
						+", kind "+file.Kind+", rows read "+file.CountRowsRead.ToString("N0",CultureInfo.InvariantCulture)+", parts read "+file.ListPartsRead.Count
						+", parts skipped "+file.ListPartsSkipped.Count);
				}
				stringBuilderOut.AppendLine("value sets: codes kept "+fileValueSets.ListEhrCodes.Count.ToString("N0",CultureInfo.InvariantCulture)+" in "
					+fileValueSets.CountValueSets+" value sets; manifest found "+fileValueSets.ListManifestOidsFound.Count+" of "+EhrValueSetImport.GetManifest().Count
					+"; rows merged "+fileValueSets.CountRowsMerged.ToString("N0",CultureInfo.InvariantCulture)+"; other value sets (not kept) "
					+fileValueSets.CountOtherValueSets.ToString("N0",CultureInfo.InvariantCulture)+" with "
					+fileValueSets.CountRowsOtherValueSets.ToString("N0",CultureInfo.InvariantCulture)+" rows; measures in the file "
					+fileValueSets.ListMeasureIdsInFile.Count+"; too long "+fileValueSets.CountRowsTooLong);
				stringBuilderOut.AppendLine("measure check (value sets): "+string.Join(", ",fileValueSets.ListMeasuresMatched)+" = "
					+fileValueSets.ListMeasuresMatched.Count+" of "+EhrValueSetImport.ArrayMeasureIds.Length);
				stringBuilderOut.AppendLine("measure check (direct codes): "+pick.FileDirectCodes.ListMeasuresMatched.Count+" of "+EhrValueSetImport.ArrayMeasureIds.Length
					+"; rows of other measures (not imported) "+pick.FileDirectCodes.CountRowsOtherMeasures);
				List<EhrCode> listRows=EhrCodes.GetAllFromDb();
				foreach(CqmMeasure measure in CqmCatalog.GetMeasures()) {
					List<EhrCode> listOfMeasure=listRows.Where(x => !CqmCatalog.IsDirectCodeKey(x.ValueSetOID)
						&& (x.MeasureIds??"").Split(',').Contains(measure.MeasureIdShort)).ToList();
					int countCatalog=CqmCatalog.GetValueSets().Count(x => x.ListCmsIds.Contains(measure.CmsId));
					List<EhrCode> listDirect=listRows.Where(x => x.ValueSetOID.StartsWith("drc:"+measure.CmsId+":",StringComparison.Ordinal)).ToList();
					stringBuilderOut.AppendLine(measure.MeasureId+": value sets "+listOfMeasure.Select(x => x.ValueSetOID).Distinct().Count()+" of "+countCatalog
						+", codes "+listOfMeasure.Count.ToString("N0",CultureInfo.InvariantCulture)+"; direct codes "+listDirect.Count+" ("
						+string.Join(", ",listDirect.GroupBy(x => x.ValueSetOID.Substring(x.ValueSetOID.LastIndexOf(':')+1)).OrderBy(x => x.Key,StringComparer.Ordinal)
						.Select(x => x.Key+" "+x.Count()))+")");
				}
				stringBuilderOut.AppendLine("roles resolved "+pick.ListRoles.Count(x => x.IsResolved)+" of "+pick.ListRoles.Count+": "
					+string.Join("; ",pick.ListRoles.Select(x => x.Role+" "+x.CountMatched+"/"+x.CountExpected+(x.IsResolved ? "" : " not resolved"))));
				stringBuilderOut.AppendLine("code-system versions the codes name: "+EhrValueSetImport.GetCodeSystemVersionsUsedText(pick.ListCodeSystemVersionsUsed));
				stringBuilderOut.AppendLine("Code System Versions file: "+string.Join(", ",pick.FileCodeSystemVersions.ListCodeSystemVersions.Select(x => x.CodeSystem+" "+x.Version)));
				stringBuilderOut.AppendLine("pick warnings: "+(pick.ListWarnings.Count==0 ? "none" : string.Join(" | ",pick.ListWarnings)));
				stringBuilderOut.AppendLine("value-set file notes: "+string.Join(" | ",fileValueSets.ListNotes));
				stringBuilderOut.AppendLine("value-set file warnings: "+(fileValueSets.ListWarnings.Count==0 ? "none" : string.Join(" | ",fileValueSets.ListWarnings)));
				stringBuilderOut.AppendLine("direct codes file warnings: "+(pick.FileDirectCodes.ListWarnings.Count==0 ? "none" : string.Join(" | ",pick.FileDirectCodes.ListWarnings)));
				//The version check against loaded tables: made-up loaded versions in the scratch database (its code tables are empty, so only the
				//comparison is shown, not IsInDb).
				DataCore.NonQ("UPDATE codesystem SET VersionCur='2025' WHERE CodeSystemName='CPT'");
				DataCore.NonQ("UPDATE codesystem SET VersionCur='2025-03-01' WHERE CodeSystemName='SNOMEDCT'");
				DataCore.NonQ("UPDATE codesystem SET VersionCur='2.81' WHERE CodeSystemName='LOINC'");
				EhrCodes.ClearList();
				foreach(EhrCodeSystemStatus status in EhrCodes.GetCodeSystemStatuses().Where(x => x.CountValueSetCodes+x.CountDirectCodes>0)) {
					stringBuilderOut.AppendLine("status "+status.CodeSystem+": value-set codes "+status.CountValueSetCodes+", direct codes "+status.CountDirectCodes
						+", loaded '"+status.VersionLoaded+"', in the release '"+status.VersionInRelease+"', compared "+status.IsVersionCompared
						+", codes newer than loaded "+status.CountCodesNewer);
				}
				stringBuilderOut.AppendLine("time: parse "+stopwatchParse.ElapsedMilliseconds.ToString("N0",CultureInfo.InvariantCulture)+" ms, import "
					+stopwatchImport.ElapsedMilliseconds.ToString("N0",CultureInfo.InvariantCulture)+" ms");
				stringBuilderOut.AppendLine("memory: managed heap before "+(bytesManagedBefore/1024/1024)+" MB, peak while reading and importing "
					+(bytesManagedPeak/1024/1024)+" MB, after with the pick held "+(bytesManagedPick/1024/1024)+" MB; private bytes before "
					+(bytesPrivateBefore/1024/1024)+" MB, peak while reading and importing "+(bytesPrivatePeak/1024/1024)+" MB; peak working set of the "
					+"test host over the whole run "+(bytesPeakWorkingSet/1024/1024)+" MB");
				//The two other sortings of the release: refused by the part limit, naming the workbook to pick.
				foreach(string pattern in new[] { "ec_*_category_*.xlsx.zip","ec_*_unique_vs_*.xlsx.zip" }) {
					string pathOther=Directory.GetFiles(folder,pattern).SingleOrDefault();
					if(pathOther==null) {
						continue;
					}
					EhrValueSetFile fileOther;
					using(FileStream fileStream=new FileStream(pathOther,FileMode.Open,FileAccess.Read,FileShare.Read)) {
						fileOther=EhrValueSetImport.Parse(Path.GetFileName(pathOther),fileStream);
					}
					stringBuilderOut.AppendLine("other sorting "+fileOther.FileName+": refused "+fileOther.IsRefused+" ("+string.Join(" ",fileOther.ListRefusals)+")");
					Assert.IsTrue(fileOther.IsRefused,fileOther.FileName);
					StringAssert.Contains(string.Join(" ",fileOther.ListRefusals),"pick the one sorted by CMS ID");
				}
				//The checks.
				CollectionAssert.AreEqual(EhrValueSetImport.ArrayMeasureIds,fileValueSets.ListMeasuresMatched,"7 of 7 measures at the 2027 versions.");
				CollectionAssert.AreEqual(EhrValueSetImport.ArrayMeasureIds,pick.FileDirectCodes.ListMeasuresMatched);
				Assert.AreEqual(EhrValueSetImport.GetManifest().Count,fileValueSets.ListManifestOidsFound.Count,"Every value set of the catalog is in the release.");
				Assert.IsTrue(fileValueSets.CountOtherValueSets>0,"The release holds other measures' value sets, not kept.");
				Assert.AreEqual(pick.ListEhrCodesAll.Count,listRows.Count);
			}
			finally {
				Thread.CurrentThread.CurrentCulture=cultureBefore;
				//Back to the EHR-1 scratch database, as ReleaseDb.End does, then this one dropped.
				ScratchDb.UseDirectConnection();
				EhrCodes.ClearList();
				Security.CurUser=null;
				Security.PasswordTyped="";
				if(ScratchDb.SkipReason==null && ValueSetDb.Exists(ScratchDb.DbName)) {
					ScratchDb.Connect(ScratchDb.DbName);
					ValueSetDb.RefreshCaches();
				}
				if(_isBuilt) {
					Drop(_dbName);
					_isBuilt=false;
				}
				string text=stringBuilderOut.ToString();
				Console.WriteLine(text);
				string pathResult=Environment.GetEnvironmentVariable(ENV_RESULT_FILE);
				if(!string.IsNullOrEmpty(pathResult)) {
					File.WriteAllText(pathResult,text,new UTF8Encoding(false));
				}
			}
		}

		///<summary>The one file of the folder that matches the pattern; fails when there is none or more than one.</summary>
		private static string FindOne(string folder,string pattern) {
			string[] arrayPaths=Directory.GetFiles(folder,pattern);
			Assert.AreEqual(1,arrayPaths.Length,"One file "+pattern+" in "+ENV_RELEASE_DIR+".");
			return arrayPaths[0];
		}

		///<summary>The report up to the list of the value sets not in the file (which names the catalog's value sets only).</summary>
		private static string ReportWithoutLists(string report) {
			int index=report.IndexOf("Value sets the measures use that are not in the file",StringComparison.Ordinal);
			return index<0 ? report : report.Substring(0,index);
		}

		///<summary>The scratch database, as Support/ReleaseDb.cs builds its own: the committed db/init/new-database.sql, then the fork migrations,
		///connected directly as user 1 (Admin).</summary>
		private static void BuildDb() {
			_isBuilt=true;//Before the database exists, so the clean-up drops a half-built one too.
			Drop(_dbName);
			using(MySqlConnection connection=ScratchDb.OpenServer()) {
				ScratchDb.Exec(connection,"CREATE DATABASE `"+_dbName+"` CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci");
			}
			string text=File.ReadAllText(TestPaths.Combine(ScratchDb.RepoPathInitSql),new UTF8Encoding(false)).Replace("\r\n","\n");
			using(MySqlConnection connection=ScratchDb.OpenServer(_dbName)) {
				foreach(string statement in text.Split(new[] { ";\n" },StringSplitOptions.None)) {
					if(statement.Trim()=="") {
						continue;
					}
					ScratchDb.Exec(connection,statement);
				}
			}
			ScratchDb.UseDirectConnection();
			ScratchDb.Connect(_dbName);
			ValueSetDb.RefreshCaches();
			ForkMigrationRunner.Run();
			ValueSetDb.RefreshCaches();
			EhrCodes.ClearList();
		}

		private static void Drop(string name) {
			if(!name.StartsWith(PREFIX,StringComparison.Ordinal)) {
				throw new ArgumentException("This test only drops databases whose name starts with "+PREFIX+".");
			}
			using(MySqlConnection connection=ScratchDb.OpenServer()) {
				ScratchDb.Exec(connection,"DROP DATABASE IF EXISTS `"+name+"`");
			}
		}
	}
}
