// Licensed under the GNU General Public License, version 2. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using DataConnectionBase;
using OpenPmsDentalBusiness;

namespace DevDbTool {
	///<summary>Turns the scratch database of --export-init-sql into the new database an office starts from: removes the rows that belong to the
	///PC or the run that built it, and the seed dump's fictional practice, providers and schedules, and writes the adoption record of a new
	///database. Data only: the schema is left exactly as the fork migrations made it.</summary>
	public class InitSqlScrub {
		///<summary>Replaces every timestamp the build run wrote (the ON UPDATE stamps the convert scripts touched, the update history row, dates
		///the convert scripts put into preferences), so the same source always gives the same file.</summary>
		public static readonly DateTime FixedDateTime=new DateTime(2026,1,1,0,0,0);
		///<summary>DocPath of a new database: the product's default A to Z folder (UI-BRAND-2, owner 2026-11-24: new installs use OpenPmsImages).</summary>
		public const string SeedDocPath=@"C:\OpenPmsImages\";
		///<summary>The ForkAdoptedFrom value of a new database, for this build's latest fork schema level. "new=1" says the database was created from
		///the file, not copied from an Open Dental database. "source" keeps the line its rows come from, the fork's own 24.3.63 compatibility line
		///that the seed dump is converted on, because the fork migration steps decide what a database still needs from it
		///(Adoption.ParseAdoptedFromSource): a step written later for "databases from before" a version then treats a new database like any other
		///database of that line. "level" is the fork schema level the file was made at. "adopted" ends the record: the version guard and the adoption
		///tools read a record with it as a finished adoption (ForkDbState.IsAdoptionRecordFinished). No times: the file must not change between
		///runs.</summary>
		public static string GetAdoptedFromValue(string level) {
			return "new=1;source="+ForkVersions.CompatVersion.ToString(4)+";level="+level+";adopted="+InitSqlExport.RepoPath;
		}

		///<summary>Tables emptied, and why.</summary>
		public static readonly Dictionary<string,string> DictTablesEmptied=new Dictionary<string,string> {
			{ "computer","workstations of the PCs that used the seed dump and of this run" },
			{ "computerpref","per-workstation settings of the same PCs" },
			{ "securitylog","the audit trail of the seed dump's history and of this run (with computer names)" },
			{ "securityloghash","the hashes of those audit rows" },
			{ "signalod","cache refresh signals of this run" },
			{ "alertitem","notices of Open Dental's eServices from the seed dump" },
			{ "ehrmeasureevent","an event logged when the seed dump was made" },
			{ "schedule","the seed dump's 2007 schedules of its fictional providers" },
			{ "scheduleop","operatories of those schedules" },
			{ "zipcode","the seed dump's fictional zip codes" },
			{ "emailhostingtemplate","Open Dental's mass e-mail templates (an eService this program removed), which load images from Open Dental's web "
				+"site" },
		};
		///<summary>Preferences with the seed dump's fictional practice name, address and phone numbers. Blanked; the office enters its own at the
		///first logon.</summary>
		public static readonly string[] ArrayPracticePrefsBlanked={
			"PracticeTitle","PracticeAddress","PracticeAddress2","PracticeCity","PracticeST","PracticeZip","PracticePhone","PracticeFax",
			"PracticeBankNumber","PracticeBillingPhone","PracticeBillingAddress","PracticeBillingAddress2","PracticeBillingCity","PracticeBillingST",
			"PracticeBillingZip","PracticePayToPhone","PracticePayToAddress","PracticePayToAddress2","PracticePayToCity","PracticePayToST",
			"PracticePayToZip",
		};
		///<summary>Identifiers of the seed dump's fictional providers (social security, license, DEA, NPI and payer numbers). Blanked.</summary>
		public static readonly string[] ArrayProviderColumnsBlanked={
			"FName","MI","Suffix","SSN","StateLicense","DEANum","BlueCrossID","MedicaidID","NationalProvID","CanadianOfficeNum","StateRxID",
			"StateWhereLicensed","EcwID","CustomID",
		};
		public static readonly string[] ArrayProviderClinicColumnsBlanked={ "DEANum","StateLicense","StateRxID","StateWhereLicensed","CareCreditMerchantId" };
		///<summary>The preferences that name the program, set to the product name in the new database: the main window title and the software name
		///of the messages and the About box. The seed dump holds Open Dental's defaults ("Open Dental", "Open Dental Software"). The program already
		///shows those as the product name (AboutBoxInfo.GetDisplayName); a new database is not an adopted copy, so its rows say so themselves. An
		///adopted database keeps Open Dental's values, because adoption may change no other row (AdoptionRowCheck).</summary>
		public static readonly string[] ArrayProductNamePrefs={ "MainWindowTitle","SoftwareName" };
		///<summary>The tables of the medical code systems (ICD-9-CM, ICD-10-CM, CPT, SNOMED CT, LOINC, RxNorm, CVX, HCPCS, UCUM, CDC race and
		///ethnicity, payer typology). Their code titles are third-party content, much of it copyrighted and licensed (owner 2026-11-26: "shouldn't
		///have any copyright materials"), so a new database has none: emptied (only the seed dump's ICD-9-CM list has rows today). An office adds
		///the codes it uses, or imports a code system under its own license.</summary>
		public static readonly string[] ArrayCodeSystemTables={ "cdcrec","cpt","cvx","hcpcs","icd10","icd9","loinc","rxnorm","snomed","sop","ucum" };
		///<summary>The default procedure buttons that have a picture and their dental icons: CodeBase.UiIcons.DictProcButtonPictures, shared with
		///ProcButtons.SetToDefault (UQ-7; the reset gives them too since the owner's answer of 2026-11-17). The seed dump's rows were made with
		///Open Dental's pictures, so the scrub gives the new database these.</summary>
		public static readonly Dictionary<string,CodeBase.DentalIcon> DictProcButtonPictures=CodeBase.UiIcons.DictProcButtonPictures;

		private readonly string _database;
		private readonly DateTime _dateTimeWindowStart;
		private readonly DateTime _dateTimeWindowEnd;
		public readonly List<string> ListLog=new List<string>();

		///<summary>dateTimeServerStart and dateTimeServerEnd: the server's clock when the build started and when it ended. Timestamps between them (with
		///a day of margin each way, for code that writes UTC) were written by the run.</summary>
		public InitSqlScrub(string database,DateTime dateTimeServerStart,DateTime dateTimeServerEnd) {
			_database=database;
			_dateTimeWindowStart=dateTimeServerStart.AddDays(-1);
			_dateTimeWindowEnd=dateTimeServerEnd.AddDays(1);
		}

		public void Run() {
			EmptyTables();
			RemoveThirdPartyContent();
			BlankProviders();
			BlankPracticePrefs();
			SetPrefs();
			SetProcButtonPictures();
			ReplaceRunTimestamps();
			ReplaceRunDatesInPrefs();
		}

		private void EmptyTables() {
			foreach(KeyValuePair<string,string> pair in DictTablesEmptied) {
				if(!TableExists(pair.Key)) {
					continue;
				}
				long count=PIn.Long(DataCore.GetScalar("SELECT COUNT(*) FROM `"+pair.Key+"`"));
				if(count==0) {
					continue;
				}
				DataCore.NonQ("DELETE FROM `"+pair.Key+"`");
				ListLog.Add("Emptied "+pair.Key+" ("+count+" rows): "+pair.Value+".");
			}
			//The seed's birthday reminder rule pointed at one of the removed templates.
			if(ColumnExists("apptreminderrule","EmailHostingTemplateNum")) {
				long count=DataCore.NonQ("UPDATE apptreminderrule SET EmailHostingTemplateNum=0 WHERE EmailHostingTemplateNum<>0");
				if(count>0) {
					ListLog.Add("Reminder rules no longer point at a removed mass e-mail template ("+count+" rows).");
				}
			}
		}

		///<summary>INITSQL-COPYRIGHT (owner 2026-11-26: "db/init/new-database.sql shouldn't have any copyright materials"): no medical code system
		///titles (ArrayCodeSystemTables), no vendor logos on the program link buttons, and no ADA CDT descriptor on a CDT-style procedure code (the
		///seed dump has none since INIT-SQL; this keeps a convert script or a later seed from bringing one back). Data only, no schema change.
		///CheckDatabase checks the result.</summary>
		private void RemoveThirdPartyContent() {
			foreach(string tableName in ArrayCodeSystemTables) {
				if(!TableExists(tableName)) {
					continue;
				}
				long count=PIn.Long(DataCore.GetScalar("SELECT COUNT(*) FROM `"+tableName+"`"));
				if(count==0) {
					continue;
				}
				DataCore.NonQ("DELETE FROM `"+tableName+"`");
				ListLog.Add("Emptied "+tableName+" ("+count+" rows): code titles of a third-party code system.");
			}
			//program.ButtonImage: the toolbar pictures of the program links are the vendors' logos. A link without one shows its text (ProgramL).
			long countLogos=DataCore.NonQ("UPDATE program SET ButtonImage='' WHERE ButtonImage<>''");
			ListLog.Add("Program link button pictures (vendor logos) removed ("+countLogos+" rows).");
			//CDT-style codes (D and four digits): named by their code only, as the seed dump has them.
			long countCodes=DataCore.NonQ("UPDATE procedurecode SET Descript=CONCAT('Procedure ',ProcCode),AbbrDesc=ProcCode,LaymanTerm='' "
				+"WHERE ProcCode REGEXP '^D[0-9]{4}$' AND (Descript<>CONCAT('Procedure ',ProcCode) OR AbbrDesc<>ProcCode OR LaymanTerm<>'')");
			ListLog.Add("CDT-style procedure codes named by their code only ("+countCodes+" rows changed).");
			//Owner, 2026-11-26: "make the password blank". A program link's password properties start empty in a new database (the seed
			//dump carried an encrypted default for one of them).
			long countPasswords=DataCore.NonQ("UPDATE programproperty SET PropertyValue='' WHERE PropertyDesc LIKE '%password%' AND PropertyValue<>''");
			ListLog.Add("Program link passwords blanked ("+countPasswords+" rows).");
		}

		private void BlankProviders() {
			List<string> listSets=ArrayProviderColumnsBlanked.Where(x => ColumnExists("provider",x)).Select(x => "`"+x+"`=''").ToList();
			//Generic names instead of the fictional ones: "Dentist" for the dentist, "Hygienist" for the secondary (hygienist) provider.
			listSets.Insert(0,"LName=IF(IsSecondary=1,'Hygienist','Dentist')");
			long count=DataCore.NonQ("UPDATE provider SET "+string.Join(",",listSets));
			ListLog.Add("Providers: generic names, identifiers blanked ("+count+" rows).");
			listSets=ArrayProviderClinicColumnsBlanked.Where(x => ColumnExists("providerclinic",x)).Select(x => "`"+x+"`=''").ToList();
			if(listSets.Count>0 && TableExists("providerclinic")) {
				count=DataCore.NonQ("UPDATE providerclinic SET "+string.Join(",",listSets));
				ListLog.Add("Provider clinic rows: identifiers blanked ("+count+" rows).");
			}
		}

		private void BlankPracticePrefs() {
			List<string> listBlanked=new List<string>();
			foreach(string prefName in ArrayPracticePrefsBlanked) {
				string value=DevDatabase.GetPrefRaw(prefName);
				if(string.IsNullOrEmpty(value)) {
					continue;
				}
				DataCore.NonQ("UPDATE preference SET ValueString='' WHERE PrefName='"+POut.String(prefName)+"'");
				listBlanked.Add(prefName);
			}
			ListLog.Add("Practice preferences blanked: "+(listBlanked.Count==0 ? "none" : string.Join(", ",listBlanked))+".");
		}

		private void SetPrefs() {
			DataCore.NonQ("UPDATE preference SET ValueString='"+POut.String(SeedDocPath)+"' WHERE PrefName='DocPath'");
			//UI-BRAND-2 review: the Office bridge's document folder default follows DocPath, so a new install's two defaults agree.
			DataCore.NonQ("UPDATE programproperty SET PropertyValue='"+POut.String(SeedDocPath)+"' WHERE PropertyDesc='Document folder'"
				+" AND PropertyValue='"+POut.String(@"C:\OpenDentImages\")+"'");
			string level=ForkMigrationHandOff.Runner.LatestLevel;
			string adoptedFrom=GetAdoptedFromValue(level);
			ForkVersions.SetMarker(ForkVersions.PREF_FORK_ADOPTED_FROM,adoptedFrom);
			ListLog.Add("DocPath "+SeedDocPath+"; ForkAdoptedFrom "+adoptedFrom+".");
			foreach(string prefName in ArrayProductNamePrefs) {
				DataCore.NonQ("UPDATE preference SET ValueString='"+POut.String(CodeBase.VendorDirect.ProductIdentity.ProductName)+"' "
					+"WHERE PrefName='"+POut.String(prefName)+"'");
			}
			ListLog.Add(string.Join(" and ",ArrayProductNamePrefs)+": "+CodeBase.VendorDirect.ProductIdentity.ProductName+".");
			long count=DataCore.NonQ("UPDATE userod SET IsPasswordResetRequired=1 WHERE UserName='Admin' AND COALESCE(Password,'')=''");
			ListLog.Add("Admin must set a password at the first logon (IsPasswordResetRequired, "+count+" row).");
		}

		///<summary>Gives the default procedure buttons of DictProcButtonPictures their new-style pictures (ProcButtons.SetToDefault already gives them
		///since the owner's answer of 2026-11-17; the seed dump's rows may predate it). Each must then be exactly one row holding its new picture;
		///anything else stops the export, so a file without them is never written.</summary>
		private void SetProcButtonPictures() {
			foreach(KeyValuePair<string,CodeBase.DentalIcon> pair in DictProcButtonPictures) {
				string picture=CodeBase.UiIcons.GetProcButtonPicture(pair.Value);
				DataCore.NonQ("UPDATE procbutton SET ButtonImage='"+POut.String(picture)+"' "
					+"WHERE Description='"+POut.String(pair.Key)+"' AND ButtonImage<>''");
				//Counted afterwards: an UPDATE that leaves a row unchanged may report 0 rows.
				long count=PIn.Long(DataCore.GetScalar("SELECT COUNT(*) FROM procbutton WHERE Description='"+POut.String(pair.Key)+"' "
					+"AND ButtonImage='"+POut.String(picture)+"'"));
				if(count!=1) {
					throw new ApplicationException("The default procedure button '"+pair.Key+"' with a picture was found "+count+" times, not once.");
				}
			}
			ListLog.Add("Default procedure buttons: new-style pictures for "+string.Join(", ",DictProcButtonPictures.Keys)+" (UQ-7).");
		}

		///<summary>One UPDATE per table that sets every date, datetime and timestamp column of a row the run stamped, so the ON UPDATE timestamps of the
		///row are assigned too and do not move again.</summary>
		private void ReplaceRunTimestamps() {
			string command="SELECT TABLE_NAME,COLUMN_NAME,DATA_TYPE FROM information_schema.COLUMNS WHERE TABLE_SCHEMA='"+POut.String(_database)+"' "
				+"AND DATA_TYPE IN ('date','datetime','timestamp') ORDER BY TABLE_NAME,ORDINAL_POSITION";
			DataTable table=DataCore.GetTable(command);
			string start=_dateTimeWindowStart.ToString("yyyy-MM-dd HH:mm:ss",CultureInfo.InvariantCulture);
			string end=_dateTimeWindowEnd.ToString("yyyy-MM-dd HH:mm:ss",CultureInfo.InvariantCulture);
			foreach(IGrouping<string,DataRow> grouping in table.Rows.Cast<DataRow>().GroupBy(x => x["TABLE_NAME"].ToString())) {
				List<string> listInWindow=new List<string>();
				List<string> listSets=new List<string>();
				foreach(DataRow row in grouping) {
					string column="`"+row["COLUMN_NAME"].ToString()+"`";
					string fixedValue=row["DATA_TYPE"].ToString()=="date"
						? FixedDateTime.ToString("yyyy-MM-dd",CultureInfo.InvariantCulture)
						: FixedDateTime.ToString("yyyy-MM-dd HH:mm:ss",CultureInfo.InvariantCulture);
					string inWindow=column+" BETWEEN '"+start+"' AND '"+end+"'";
					listInWindow.Add("("+inWindow+")");
					listSets.Add(column+"=IF("+inWindow+",'"+fixedValue+"',"+column+")");
				}
				string where=string.Join(" OR ",listInWindow);
				long count=PIn.Long(DataCore.GetScalar("SELECT COUNT(*) FROM `"+grouping.Key+"` WHERE "+where));
				if(count==0) {
					continue;
				}
				DataCore.NonQ("UPDATE `"+grouping.Key+"` SET "+string.Join(",",listSets)+" WHERE "+where);
				ListLog.Add("Timestamps of this run set to "+FixedDateTime.ToString("yyyy-MM-dd",CultureInfo.InvariantCulture)+" in "+grouping.Key
					+" ("+count+" rows).");
			}
		}

		///<summary>Preferences that the convert scripts set to a date of the run (AgingServiceTimeDue: today at 02:00). The date part becomes the fixed
		///date; the time of day, which is what the preference means, stays.</summary>
		private void ReplaceRunDatesInPrefs() {
			Regex regex=new Regex(@"^(\d{4}-\d{2}-\d{2})((?: \d{2}:\d{2}:\d{2})?)$");
			DataTable table=DataCore.GetTable("SELECT PrefNum,PrefName,ValueString FROM preference ORDER BY PrefNum");
			foreach(DataRow row in table.Rows) {
				string value=row["ValueString"].ToString();
				Match match=regex.Match(value);
				if(!match.Success
					|| !DateTime.TryParseExact(match.Groups[1].Value,"yyyy-MM-dd",CultureInfo.InvariantCulture,DateTimeStyles.None,out DateTime date)
					|| date<_dateTimeWindowStart.Date || date>_dateTimeWindowEnd.Date)
				{
					continue;
				}
				string valueNew=FixedDateTime.ToString("yyyy-MM-dd",CultureInfo.InvariantCulture)+match.Groups[2].Value;
				DataCore.NonQ("UPDATE preference SET ValueString='"+POut.String(valueNew)+"' WHERE PrefNum="+POut.Long(PIn.Long(row["PrefNum"].ToString())));
				ListLog.Add("Preference "+row["PrefName"]+": "+value+" became "+valueNew+".");
			}
		}

		///<summary>Throws when the scrubbed database is not what a new database must be: patient rows, an enabled program link, a table that is not
		///MyISAM, or version markers the startup guard would not open with every step applied.</summary>
		public void CheckDatabase() {
			List<string> listProblems=new List<string>();
			string command="SELECT TABLE_NAME FROM information_schema.COLUMNS WHERE TABLE_SCHEMA='"+POut.String(_database)+"' AND COLUMN_NAME='PatNum' "
				+"ORDER BY TABLE_NAME";
			foreach(DataRow row in DataCore.GetTable(command).Rows) {
				string tableName=row[0].ToString();
				long count=PIn.Long(DataCore.GetScalar("SELECT COUNT(*) FROM `"+tableName+"` WHERE PatNum<>0"));
				if(count>0) {
					listProblems.Add(tableName+" has "+count+" row(s) of a patient.");
				}
			}
			foreach(string tableName in new[] { "patient","appointment","computer","computerpref","securitylog" }) {
				if(TableExists(tableName) && PIn.Long(DataCore.GetScalar("SELECT COUNT(*) FROM `"+tableName+"`"))>0) {
					listProblems.Add(tableName+" is not empty.");
				}
			}
			foreach(string tableName in ArrayCodeSystemTables) {
				if(TableExists(tableName) && PIn.Long(DataCore.GetScalar("SELECT COUNT(*) FROM `"+tableName+"`"))>0) {
					listProblems.Add(tableName+" has code titles of a third-party code system.");
				}
			}
			long countLogos=PIn.Long(DataCore.GetScalar("SELECT COUNT(*) FROM program WHERE ButtonImage<>''"));
			if(countLogos>0) {
				listProblems.Add(countLogos+" program link(s) have a button picture (a vendor logo).");
			}
			long countCodes=PIn.Long(DataCore.GetScalar("SELECT COUNT(*) FROM procedurecode WHERE ProcCode REGEXP '^D[0-9]{4}$' "
				+"AND (Descript<>CONCAT('Procedure ',ProcCode) OR AbbrDesc<>ProcCode OR LaymanTerm<>'')"));
			if(countCodes>0) {
				listProblems.Add(countCodes+" CDT-style procedure code(s) have a description of their own.");
			}
			DataTable tablePrograms=DataCore.GetTable("SELECT ProgName FROM program WHERE Enabled<>0 ORDER BY ProgName");
			foreach(DataRow row in tablePrograms.Rows) {
				listProblems.Add("Program link "+row[0]+" is enabled; a new database has every program link off.");
			}
			command="SELECT TABLE_NAME,ENGINE FROM information_schema.TABLES WHERE TABLE_SCHEMA='"+POut.String(_database)+"' "
				+"AND (TABLE_TYPE<>'BASE TABLE' OR ENGINE<>'MyISAM') ORDER BY TABLE_NAME";
			foreach(DataRow row in DataCore.GetTable(command).Rows) {
				listProblems.Add("Table "+row[0]+" is "+row[1]+", not a MyISAM table. Set default-storage-engine=MyISAM in my.ini.");
			}
			ForkDbState forkDbState=ForkVersions.ReadState();
			ForkGuardResult forkGuardResult=ForkVersionGuard.Evaluate(forkDbState);
			if(forkGuardResult.Action!=ForkGuardAction.ProceedFork || forkGuardResult.HasPendingMigrations) {
				listProblems.Add("The startup version guard says "+forkGuardResult.Action+" (pending: "+string.Join(", ",forkGuardResult.ListPendingLevels)
					+"): "+forkGuardResult.Message);
			}
			if(!forkDbState.IsAdoptionRecordFinished) {
				listProblems.Add("The adoption record is not finished: "+forkDbState.ForkAdoptedFrom);
			}
			if(listProblems.Count>0) {
				throw new ApplicationException("The scrubbed database is not a clean new database:\r\n  "+string.Join("\r\n  ",listProblems));
			}
			ListLog.Add("Checked: no patient rows, no code system titles, vendor logos or CDT descriptors, every program link off, every table MyISAM, "
				+"the startup guard opens it with no step pending.");
		}

		///<summary>What the generated text must not contain: this PC's names, the run's dates (a timestamp the scrub missed) and Open Dental's hosts
		///(CodeBase.VendorDirect.HqHosts). Case-insensitive. Returns one line per find, with the line number.</summary>
		public static List<string> FindLeftovers(string text,List<string> listForbidden,List<string> listRunDates) {
			List<string> listProblems=new List<string>();
			foreach(string value in listForbidden.Concat(listRunDates)) {
				int index=text.IndexOf(value,StringComparison.OrdinalIgnoreCase);
				if(index>=0) {
					listProblems.Add("'"+value+"' "+Where(text,index));
				}
			}
			foreach(string domain in CodeBase.VendorDirect.HqHosts.BlockedDomains) {
				Match match=Regex.Match(text,@"(?i)(?<![a-z0-9-])(?:[a-z0-9-]+\.)*"+Regex.Escape(domain)+"(?![a-z0-9-])");
				if(match.Success) {
					listProblems.Add("Open Dental host '"+match.Value+"' "+Where(text,match.Index));
				}
			}
			return listProblems;
		}

		private static string Where(string text,int index) {
			int line=1;
			for(int i=0;i<index;i++) {
				if(text[i]=='\n') {
					line++;
				}
			}
			int indexLineStart=text.LastIndexOf('\n',index)+1;
			int indexSnippet=Math.Max(indexLineStart,index-60);
			return "at line "+line+": ..."+text.Substring(indexSnippet,Math.Min(160,text.Length-indexSnippet));
		}

		private bool TableExists(string tableName) {
			return DataCore.GetScalar("SELECT COUNT(*) FROM information_schema.TABLES WHERE TABLE_SCHEMA='"+POut.String(_database)+"' AND TABLE_NAME='"
				+POut.String(tableName)+"'")!="0";
		}

		private bool ColumnExists(string tableName,string columnName) {
			return DataCore.GetScalar("SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_SCHEMA='"+POut.String(_database)+"' AND TABLE_NAME='"
				+POut.String(tableName)+"' AND COLUMN_NAME='"+POut.String(columnName)+"'")!="0";
		}
	}
}
