// Explicit adoption of an Open Dental database (backlog Risk R1, package M1-03). Normal startup never calls this; an administrator runs it from
// FormAdoptDatabase against a restored copy.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CodeBase;
using CodeBase.VendorDirect;
using DataConnectionBase;
using OpenPmsDentalBusiness.ForkMigrations;

namespace OpenPmsDentalBusiness {
	public class AdoptionPreflight {
		public bool CanAdopt;
		///<summary>True when an earlier adoption of this copy was started and is being finished.</summary>
		public bool IsResume;
		public bool IsAlreadyAdopted;
		///<summary>The Open Dental version the database comes from (on the allowlist when CanAdopt).</summary>
		public Version VersionSource;
		///<summary>Set when the database's DataBaseVersion and ProgramVersion are a pair on ForkVersions.ListAdoptableLabels (25.3.47.0 with 25.3.54.0)
		///rather than VersionSource itself. Such a database is adopted as VersionSource only when its schema equals that version's reference exactly,
		///the patient portal column patient._portal_active_email and key aside.</summary>
		public AdoptableVersionLabels Labels;
		///<summary>True when Labels is set and the schema has not been verified yet (Preflight(ForkDbState) without a schema result). CanAdopt is false
		///then; Preflight() verifies the connected database and decides.</summary>
		public bool IsSchemaCheckPending;
		///<summary>The schema differences that refused a Labels pair. Empty otherwise.</summary>
		public List<string> ListSchemaProblems=new List<string>();
		public string Message="";
	}

	public class AdoptionResult {
		public Version VersionSource;
		///<summary>The DataBaseVersion and ProgramVersion pair the database had when it was adopted through ForkVersions.ListAdoptableLabels. Null
		///otherwise.</summary>
		public AdoptableVersionLabels Labels;
		///<summary>True when patient._portal_active_email was there and was kept, as it was (owner decision of 2026-11-25).</summary>
		public bool IsPortalEmailColumnKept;
		///<summary>True when its unique key portal_active_email_uniq was there and was kept, as it was.</summary>
		public bool IsPortalEmailIndexKept;
		///<summary>Keys on patient._portal_active_email other than portal_active_email_uniq, which adoption drops (none in the databases seen so far).
		///</summary>
		public List<string> ListIndexesDropped=new List<string>();
		public long CountUpdateFileRowsDeleted;
		public List<string> ListPrefsBlanked=new List<string>();
		public List<string> ListLog=new List<string>();
	}

	///<summary>Adopts an Open Dental database whose DataBaseVersion is on the allowlist (24.3.63.0 or 25.3.48.0, ProgramVersion equal to it), or whose
	///DataBaseVersion and ProgramVersion are a pair on ForkVersions.ListAdoptableLabels (25.3.47.0 with 25.3.54.0) and whose schema then equals the
	///25.3.48 reference exactly (the patient portal column patient._portal_active_email aside). Such a pair is adopted as 25.3.48: ForkAdoptedFrom says
	///source=25.3.48.0, so every fork migration step treats it as a 25.3.48 database, and records the real labels in odDataBaseVersion= and
	///odProgramVersion=. Every step is idempotent, and the
	///order is chosen so an interrupted adoption (MyISAM has no transactions) can be run again to finish:
	///1. ForkAdoptedFrom records the start. 2. The patient portal column patient._portal_active_email and its unique key portal_active_email_uniq are
	///kept as they are, for a patient portal (owner decision of 2026-11-25; SchemaVerifier allows them with exactly the expected
	///definitions); any other key on the column is dropped. 3. Open Dental's
	///UpdateFiles and UpdateFilesSegment rows are deleted unread. 4. Preferences that point at Open Dental's servers are blanked. 5. DataBaseVersion and
	///ProgramVersion get the sentinel, so Open Dental clients can no longer use the copy. 6. OdSchemaCompat, then ForkSchemaLevel are written: from
	///here the fork markers govern the database. 7. The fork migration runner runs its pending steps. 8. ForkAdoptedFrom records the finish.</summary>
	public class Adoption {
		public const string PATIENT_PORTAL_EMAIL_COLUMN="_portal_active_email";
		///<summary>The unique key a patient portal puts on the column (as in a 25.3.48 schema-only dump). Adoption keeps it; any other key on the column
		///is dropped.</summary>
		public const string PATIENT_PORTAL_EMAIL_INDEX="portal_active_email_uniq";
		///<summary>Preferences that only ever held addresses of Open Dental's servers. They are blanked whatever they hold. Any other preference whose
		///value is a web address on one of Open Dental's hosts (CodeBase.VendorDirect.HqHosts) is blanked too.</summary>
		public static readonly List<string> ListHqUrlPrefNames=new List<string>() {
			"WebServiceHQServerURL",
			"UpdateServerAddress",
			"UpdateWebsitePath",
			"WebHostSynchServerURL",
			"MobileSyncServerURL",
			"EmailHostingEndpoint",
		};

		///<summary>Decides from the version preferences and markers whether the database can be adopted. A pair on ForkVersions.ListAdoptableLabels
		///also needs the schema verifier's result for the pair's schema version (SchemaVerifier.VerifyDatabase25_3_48); without it CanAdopt is false
		///and IsSchemaCheckPending true.</summary>
		public static AdoptionPreflight Preflight(ForkDbState forkDbState,SchemaVerifyResult schemaVerifyResult=null) {
			AdoptionPreflight adoptionPreflight=new AdoptionPreflight();
			if(forkDbState.IsForkMarked) {
				adoptionPreflight.IsAlreadyAdopted=true;
				adoptionPreflight.Message="This database is already adopted ("+(forkDbState.ForkAdoptedFrom??"no adoption record")+"). Nothing to do.";
				return adoptionPreflight;
			}
			if(forkDbState.IsAdoptionRecordFinished) {
				//The adoption finished (and its fork migration steps ran), but ForkSchemaLevel was removed since. Adopting again would reset the schema
				//level to the baseline and run every step a second time on a migrated database.
				adoptionPreflight.Message="This database was already adopted ("+forkDbState.ForkAdoptedFrom+"), but its version markers (ForkSchemaLevel, "
					+"OdSchemaCompat) were removed. It is not adopted again. Restore a backup of the adopted database.";
				return adoptionPreflight;
			}
			if(forkDbState.IsAdoptionIncomplete) {
				Version versionStarted=ParseAdoptedFromSource(forkDbState.ForkAdoptedFrom);
				if(!ForkVersions.IsAdoptableSource(versionStarted)) {
					adoptionPreflight.Message="The unfinished adoption record is not readable ("+forkDbState.ForkAdoptedFrom+"). Restore the backup taken "
						+"before the adoption.";
					return adoptionPreflight;
				}
				adoptionPreflight.CanAdopt=true;
				adoptionPreflight.IsResume=true;
				adoptionPreflight.VersionSource=versionStarted;
				adoptionPreflight.Message="An adoption from Open Dental "+versionStarted.ToString(3)+" was started on this copy and will be finished.";
				return adoptionPreflight;
			}
			Version versionDb=ForkVersions.ParseVersion(forkDbState.DataBaseVersion);
			Version versionProgram=ForkVersions.ParseVersion(forkDbState.ProgramVersion);
			adoptionPreflight.VersionSource=versionDb;
			AdoptableVersionLabels labels=ForkVersions.FindAdoptableLabels(versionDb,versionProgram);
			if(labels!=null) {
				return PreflightLabels(adoptionPreflight,labels,schemaVerifyResult);
			}
			if(!ForkVersions.IsAdoptable(versionDb)) {
				adoptionPreflight.Message="DataBaseVersion is "+(forkDbState.DataBaseVersion??"missing")+" (ProgramVersion "+(forkDbState.ProgramVersion??"missing")
					+"). Only these Open Dental versions can be adopted: "+string.Join(", ",ForkVersions.ListAdoptableVersions.Select(x => x.ToString(4)))
					+" with ProgramVersion equal to it, or "+string.Join(", ",ForkVersions.ListAdoptableLabels.Select(x => x.ToString()))
					+", or any "+string.Join(", ",ForkVersions.ListAdoptableLines.Select(x => x.Major+"."+x.Minor+".x"))
					+" release (ProgramVersion on the same line, not below DataBaseVersion), when the schema matches exactly.";
				return adoptionPreflight;
			}
			if(versionProgram!=versionDb) {
				adoptionPreflight.Message="ProgramVersion ("+(forkDbState.ProgramVersion??"missing")+") differs from DataBaseVersion ("
					+forkDbState.DataBaseVersion+"). The copy may be part way through an Open Dental update; restore a clean backup.";
				return adoptionPreflight;
			}
			adoptionPreflight.CanAdopt=true;
			adoptionPreflight.Message="Open Dental "+versionDb.ToString(3)+" database. It can be adopted.";
			return adoptionPreflight;
		}

		///<summary>Reads the database and decides whether it can be adopted: version preferences and markers, no failed update, no update in progress,
		///a direct MySQL/MariaDB connection.</summary>
		public static AdoptionPreflight Preflight() {
			Meth.NoCheckMiddleTierRole();
			AdoptionPreflight adoptionPreflight;
			if(RemotingClient.MiddleTierRole==MiddleTierRole.ClientMT || DataConnection.DBtype!=DatabaseType.MySql) {
				adoptionPreflight=new AdoptionPreflight();
				adoptionPreflight.Message="Adoption needs a direct connection to a MySQL or MariaDB database.";
				return adoptionPreflight;
			}
			ForkDbState forkDbState=ForkVersions.ReadState();
			adoptionPreflight=Preflight(forkDbState);
			if(adoptionPreflight.IsSchemaCheckPending) {
				adoptionPreflight=Preflight(forkDbState,VerifySchema(adoptionPreflight.Labels.SchemaVersion));
			}
			if(!adoptionPreflight.CanAdopt) {
				return adoptionPreflight;
			}
			if(PIn.Bool(ForkVersions.GetMarker("CorruptedDatabase")??"0")) {
				adoptionPreflight.CanAdopt=false;
				adoptionPreflight.Message="The database is flagged as corrupted by a failed update (CorruptedDatabase). Restore a clean backup.";
				return adoptionPreflight;
			}
			string updateComputer=ForkVersions.GetMarker("UpdateInProgressOnComputerName")??"";
			if(updateComputer!="") {
				adoptionPreflight.CanAdopt=false;
				adoptionPreflight.Message="An update is marked in progress on '"+updateComputer+"'. Restore a copy taken while no update was running.";
				return adoptionPreflight;
			}
			return adoptionPreflight;
		}

		///<summary>A labels pair is adoptable only when the schema equals the reference of its schema version exactly: no mismatch, and no allowed
		///difference other than the patient portal column patient._portal_active_email and its key (adoption keeps them). The HQ-only, legacy and fork
		///tables that SchemaVerifier otherwise allows are not in an Open Dental database with these labels, so here they count as differences.</summary>
		private static AdoptionPreflight PreflightLabels(AdoptionPreflight adoptionPreflight,AdoptableVersionLabels labels,
			SchemaVerifyResult schemaVerifyResult)
		{
			adoptionPreflight.Labels=labels;
			adoptionPreflight.VersionSource=labels.SchemaVersion;
			string schema=labels.SchemaVersion.ToString(3);
			string described="Open Dental "+labels.ProgramVersion.ToString(3)+" database (DataBaseVersion "+labels.DataBaseVersion.ToString(4)
				+", ProgramVersion "+labels.ProgramVersion.ToString(4)+")";
			if(schemaVerifyResult==null) {
				adoptionPreflight.IsSchemaCheckPending=true;
				adoptionPreflight.Message=described+". It can be adopted as Open Dental "+schema+" only when its schema equals the "+schema
					+" reference exactly; the schema has not been checked.";
				return adoptionPreflight;
			}
			adoptionPreflight.ListSchemaProblems=GetLabelsSchemaProblems(schemaVerifyResult);
			if(adoptionPreflight.ListSchemaProblems.Count>0) {
				adoptionPreflight.Message=described+". Its schema differs from the Open Dental "+schema+" reference, so it is not adopted ("
					+adoptionPreflight.ListSchemaProblems.Count+" difference(s)): "+string.Join("; ",adoptionPreflight.ListSchemaProblems.Take(20))
					+(adoptionPreflight.ListSchemaProblems.Count>20 ? "; and "+(adoptionPreflight.ListSchemaProblems.Count-20)+" more" : "")+".";
				return adoptionPreflight;
			}
			adoptionPreflight.CanAdopt=true;
			adoptionPreflight.Message=described+". Its schema equals the Open Dental "+schema+" reference ("+schemaVerifyResult.CountTablesActual
				+" tables). It can be adopted as Open Dental "+schema+".";
			return adoptionPreflight;
		}

		///<summary>The differences that refuse a labels pair: every mismatch, and every allowed difference other than the office's
		///patient._portal_active_email column and its key. Empty when the schema matches exactly.</summary>
		public static List<string> GetLabelsSchemaProblems(SchemaVerifyResult schemaVerifyResult) {
			return schemaVerifyResult.ListDifferences
				.Where(x => !x.IsExcepted || !IsPortalEmailDifference(x))
				.Select(x => x.ToString())
				.ToList();
		}

		private static bool IsPortalEmailDifference(SchemaDifference difference) {
			return string.Equals(difference.Table,"patient",StringComparison.OrdinalIgnoreCase)
				&& (string.Equals(difference.Item,PATIENT_PORTAL_EMAIL_COLUMN,StringComparison.OrdinalIgnoreCase)
				|| string.Equals(difference.Item,PATIENT_PORTAL_EMAIL_INDEX,StringComparison.OrdinalIgnoreCase));
		}

		///<summary>Verifies the connected database against the reference of the schema version (25.3.48 or 26.2.0 before adoption).</summary>
		private static SchemaVerifyResult VerifySchema(Version versionSchema) {
			return SchemaVerifier.VerifyDatabaseAs(versionSchema);
		}

		///<summary>The start of a ForkAdoptedFrom value: source= the schema version, then, for a labels pair, the real DataBaseVersion and ProgramVersion
		///(odDataBaseVersion=, odProgramVersion=), then started=.</summary>
		public static string FormatAdoptedFromStart(Version versionSource,AdoptableVersionLabels labels,string started) {
			string text="source="+versionSource.ToString(4);
			if(labels!=null) {
				text+=";odDataBaseVersion="+labels.DataBaseVersion.ToString(4)+";odProgramVersion="+labels.ProgramVersion.ToString(4);
			}
			return text+";started="+started;
		}

		///<summary>Adopts the connected database. Throws if the preflight fails or a step fails; running it again finishes an interrupted adoption.
		///Pass the runner for tests; null uses the registered fork migration runner.</summary>
		public static AdoptionResult Run(Action<string> actionLog=null,IForkMigrationRunner runner=null,DateTime? dateTimeUtcNow=null) {
			Meth.NoCheckMiddleTierRole();
			runner=runner??ForkMigrationHandOff.Runner;
			AdoptionResult adoptionResult=new AdoptionResult();
			void Log(string text) {
				adoptionResult.ListLog.Add(text);
				actionLog?.Invoke(text);
			}
			AdoptionPreflight adoptionPreflight=Preflight();
			if(!adoptionPreflight.CanAdopt) {
				throw new ApplicationException(adoptionPreflight.Message);
			}
			Version versionSource=adoptionPreflight.VersionSource;
			adoptionResult.VersionSource=versionSource;
			AdoptableVersionLabels labels=adoptionPreflight.Labels;
			string started=FormatUtc(dateTimeUtcNow??DateTime.UtcNow);
			if(adoptionPreflight.IsResume) {
				string adoptedFromStarted=ForkVersions.GetMarker(ForkVersions.PREF_FORK_ADOPTED_FROM);
				Log("Finishing the adoption started earlier: "+adoptedFromStarted);
				started=ParseAdoptedFromField(adoptedFromStarted,"started")??started;
				labels=ParseAdoptedFromLabels(adoptedFromStarted);
			}
			else {
				ForkVersions.SetMarker(ForkVersions.PREF_FORK_ADOPTED_FROM,FormatAdoptedFromStart(versionSource,labels,started));
				Log("Adoption from Open Dental "+versionSource.ToString(4)+" started."+(labels==null ? "" : " The database is labelled DataBaseVersion "
					+labels.DataBaseVersion.ToString(4)+", ProgramVersion "+labels.ProgramVersion.ToString(4)+"; its schema equals the "
					+versionSource.ToString(3)+" reference."));
			}
			adoptionResult.Labels=labels;
			//2. The patient portal's generated column and its unique key: kept.
			KeepPortalActiveEmail(adoptionResult,Log);
			//3. Open Dental's update files: deleted without being read.
			adoptionResult.CountUpdateFileRowsDeleted=DocumentMiscs.DeleteOpenDentalUpdateFiles();
			Log("Deleted "+adoptionResult.CountUpdateFileRowsDeleted+" Open Dental update file row(s).");
			//4. Addresses of Open Dental's servers.
			adoptionResult.ListPrefsBlanked=BlankHqUrlPrefs();
			Log("Blanked "+adoptionResult.ListPrefsBlanked.Count+" preference(s) that pointed at Open Dental's servers"
				+(adoptionResult.ListPrefsBlanked.Count==0?".":": "+string.Join(", ",adoptionResult.ListPrefsBlanked)+"."));
			//5. Lock out Open Dental clients.
			ForkVersions.SetMarker("DataBaseVersion",ForkVersions.SentinelVersion.ToString(4));
			ForkVersions.SetMarker("ProgramVersion",ForkVersions.SentinelVersion.ToString(4));
			Log("DataBaseVersion and ProgramVersion set to "+ForkVersions.SentinelVersion.ToString(4)+".");
			//6. The fork markers take over. ForkSchemaLevel last: its presence is what marks the database as adopted.
			if(ForkVersions.GetMarker(ForkVersions.PREF_OD_SCHEMA_COMPAT)==null) {
				ForkVersions.SetMarker(ForkVersions.PREF_OD_SCHEMA_COMPAT,versionSource.ToString(3));
			}
			if(ForkVersions.GetMarker(ForkVersions.PREF_FORK_SCHEMA_LEVEL)==null) {
				ForkVersions.SetMarker(ForkVersions.PREF_FORK_SCHEMA_LEVEL,ForkVersions.SCHEMA_LEVEL_BASELINE);
			}
			Log("Fork markers written: OdSchemaCompat "+ForkVersions.GetMarker(ForkVersions.PREF_OD_SCHEMA_COMPAT)+", ForkSchemaLevel "
				+ForkVersions.GetMarker(ForkVersions.PREF_FORK_SCHEMA_LEVEL)+".");
			//7. Fork migration steps: 24.4 to 25.3.48 for a 24.3.63 source, then 25.4 to 26.1.70 and 26.2.0. For 25.3.48 the 25.3 schema and seed steps
			//find nothing to change; for 26.2.0 none of the Open Dental schema steps does (each adds only what is missing), and the seed and data steps
			//that belong to older sources (IsFrom24_3, IsFromBefore25_4) are skipped; the report rows step may rename report rows Open Dental wrote under names this program does not know
			//(ReportRows25_3.PlanRenames predicts it).
			if(ForkMigrationHandOff.HasSteps(runner)) {
				Log("Running fork migration steps up to level "+runner.LatestLevel+".");
				runner.RunPending(Log);
			}
			else {
				Log("No fork migration steps in this build.");
			}
			//8. Done.
			ForkVersions.SetMarker(ForkVersions.PREF_FORK_ADOPTED_FROM,FormatAdoptedFromStart(versionSource,labels,started)
				+";adopted="+FormatUtc(dateTimeUtcNow??DateTime.UtcNow));
			Log("Adoption finished.");
			ODException.SwallowAnyException(() => {
				SecurityLog securityLog=new SecurityLog();
				securityLog.PermType=EnumPermType.UpdateInstall;
				securityLog.UserNum=0;//Adoption runs before anyone logs in.
				securityLog.LogDateTime=DateTime.Now;
				securityLog.LogText="Database adopted from Open Dental "+versionSource.ToString(4)+(labels==null ? "" : " (DataBaseVersion "
					+labels.DataBaseVersion.ToString(4)+", ProgramVersion "+labels.ProgramVersion.ToString(4)+")")+".";
				securityLog.CompName=Security.GetComplexComputerName();
				SecurityLogs.MakeLogEntry(securityLog);
			});
			return adoptionResult;
		}

		///<summary>Keeps patient._portal_active_email and its key portal_active_email_uniq exactly as they are: a patient portal relies on
		///them (one active patient per e-mail address; owner decision of 2026-11-25). Any other key on the column is not the office's and is dropped,
		///as before. The schema verifier allows the column and key only with the expected definitions, so a different definition fails the check
		///after adoption.</summary>
		private static void KeepPortalActiveEmail(AdoptionResult adoptionResult,Action<string> actionLog) {
			string command="SELECT DISTINCT INDEX_NAME FROM information_schema.STATISTICS WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='patient' "
				+"AND COLUMN_NAME='"+PATIENT_PORTAL_EMAIL_COLUMN+"'";
			DataTable table=Db.GetTable(command);
			foreach(DataRow row in table.Rows) {
				string indexName=PIn.String(row["INDEX_NAME"].ToString());
				if(indexName.ToUpper()=="PRIMARY") {
					continue;
				}
				if(indexName==PATIENT_PORTAL_EMAIL_INDEX) {
					adoptionResult.IsPortalEmailIndexKept=true;
					continue;
				}
				Db.NonQ("ALTER TABLE patient DROP INDEX `"+indexName.Replace("`","``")+"`");
				adoptionResult.ListIndexesDropped.Add(indexName);
				actionLog("Dropped index patient."+indexName+" (not the office's key on "+PATIENT_PORTAL_EMAIL_COLUMN+").");
			}
			command="SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='patient' "
				+"AND COLUMN_NAME='"+PATIENT_PORTAL_EMAIL_COLUMN+"'";
			adoptionResult.IsPortalEmailColumnKept=PIn.Long(Db.GetScalar(command))>0;
			if(adoptionResult.IsPortalEmailColumnKept) {
				actionLog("Kept column patient."+PATIENT_PORTAL_EMAIL_COLUMN+(adoptionResult.IsPortalEmailIndexKept ? " and its unique key "+PATIENT_PORTAL_EMAIL_INDEX : "")
					+" (a patient portal's sign-in: one active patient per e-mail address).");
			}
			else {
				actionLog("patient."+PATIENT_PORTAL_EMAIL_COLUMN+" is not present.");
			}
		}

		///<summary>Blanks the listed HQ preferences and any preference whose value is a web address on one of Open Dental's hosts. Returns the
		///names of the preferences changed.</summary>
		private static List<string> BlankHqUrlPrefs() {
			List<string> listBlanked=new List<string>();
			DataTable table=Db.GetTable("SELECT PrefNum,PrefName,ValueString FROM preference");
			foreach(DataRow row in table.Rows) {
				string prefName=PIn.String(row["PrefName"].ToString());
				string value=PIn.String(row["ValueString"].ToString());
				if(value=="") {
					continue;
				}
				if(!ListHqUrlPrefNames.Contains(prefName,StringComparer.OrdinalIgnoreCase) && !IsHqWebAddress(value)) {
					continue;
				}
				Db.NonQ("UPDATE preference SET ValueString='' WHERE PrefNum="+POut.Long(PIn.Long(row["PrefNum"].ToString())));
				listBlanked.Add(prefName);
			}
			return listBlanked;
		}

		///<summary>True when the whole value is one http or https address on one of Open Dental's hosts. Text that merely mentions a host (an e-mail
		///template, say) is left alone.</summary>
		public static bool IsHqWebAddress(string value) {
			string trimmed=(value??"").Trim();
			if(!trimmed.StartsWith("http://",StringComparison.OrdinalIgnoreCase) && !trimmed.StartsWith("https://",StringComparison.OrdinalIgnoreCase)) {
				return false;
			}
			if(trimmed.Any(x => char.IsWhiteSpace(x))) {
				return false;
			}
			return HqHosts.IsBlocked(trimmed);
		}

		///<summary>The source version from a ForkAdoptedFrom value ("source=25.3.48.0;started=...;adopted=..."), or null.</summary>
		public static Version ParseAdoptedFromSource(string adoptedFrom) {
			return ForkVersions.ParseVersion(ParseAdoptedFromField(adoptedFrom,"source"));
		}

		///<summary>The real DataBaseVersion and ProgramVersion pair that a ForkAdoptedFrom value records (odDataBaseVersion=, odProgramVersion=), as its
		///ForkVersions.ListAdoptableLabels entry. Null when it records none or a pair not on the list.</summary>
		public static AdoptableVersionLabels ParseAdoptedFromLabels(string adoptedFrom) {
			Version versionDb=ForkVersions.ParseVersion(ParseAdoptedFromField(adoptedFrom,"odDataBaseVersion"));
			Version versionProgram=ForkVersions.ParseVersion(ParseAdoptedFromField(adoptedFrom,"odProgramVersion"));
			return ForkVersions.FindAdoptableLabels(versionDb,versionProgram);
		}

		///<summary>One field of a ForkAdoptedFrom value, or null.</summary>
		public static string ParseAdoptedFromField(string adoptedFrom,string field) {
			return ForkVersions.ParseAdoptedFromField(adoptedFrom,field);
		}

		private static string FormatUtc(DateTime dateTimeUtc) {
			return dateTimeUtc.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'",CultureInfo.InvariantCulture);
		}
	}
}
