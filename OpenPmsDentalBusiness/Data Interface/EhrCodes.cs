using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace OpenPmsDentalBusiness {
	///<summary>Never insert or update one row, use cache pattern only.  The fork-only table ehrcode (docs/specs/ehr-rebuild-plan.md, EHR-2) holds the
	///value-set codes of the eCQMs and the EHR pick lists, and the measures' direct reference codes (ValueSetOID a drc: key, CqmCatalog).  Open Dental
	///filled the list from its closed EHR.dll; this program fills the table from the office's own import of VSAC's 2026-05-14 release for the 2027
	///measures with its Direct Reference Codes and Code System Versions files (EhrValueSetImport, below; docs/specs/ehr-2027-measures-plan.md,
	///EHR-6a), which replaces every row.  Until the office imports it the table is empty, and so is the list.</summary>
	public class EhrCodes {
		#region CachePattern
		//Atypical cache pattern. listt is read from the ehrcode table the first time it is asked for (through the Middle Tier on a client), and read
		//again by UpdateList (InvalidType.EhrCodes, which Cache.GetCacheDs runs on a direct connection and on the Middle Tier server). A Middle Tier
		//client drops its list in Cache.FillCache, so its next read fetches the new rows from the server.

		///<summary>A list of all EhrCodes.</summary>
		private static List<EhrCode> listt;
		private static object lockListt=new object();

		///<summary>A list of all EhrCodes: the rows of the ehrcode table, read the first time they are asked for, with IsInDb worked out.</summary>
		public static List<EhrCode> Listt {
			get {
				lock(lockListt) {
					if(listt==null) {
						listt=GetAllFromDb();
						updateCodeExistsHelper();
					}
					return listt;
				}
			}
			set {
				lock(lockListt) {
					listt=value;
				}
			}
		}

		///<summary>Forces an update to the in-memory list: reads the table again, then works out IsInDb.</summary>
		public static void UpdateList() {
			Meth.NoCheckMiddleTierRole();
			lock(lockListt) {
				listt=GetAllFromDb();
				updateCodeExistsHelper();
			}
		}

		///<summary>Drops the in-memory list, so the next read of Listt reads the table again (through the Middle Tier on a client). Used by a Middle
		///Tier client when it is told that the codes changed (Cache.FillCache, InvalidType.EhrCodes).</summary>
		public static void ClearList() {
			Meth.NoCheckMiddleTierRole();
			lock(lockListt) {
				listt=null;
			}
		}

		///<summary>Every row of the ehrcode table, in the order they were imported. IsInDb is false in every row (it is not a column); Listt works it
		///out. A database the fork step 2026111401 has not run on yet (a tool's or a test's) has no such table: an empty list, as before EHR-2.</summary>
		public static List<EhrCode> GetAllFromDb() {
			if(RemotingClient.MiddleTierRole==MiddleTierRole.ClientMT) {
				return Meth.GetObject<List<EhrCode>>(MethodBase.GetCurrentMethod());
			}
			string command="SELECT COUNT(*) FROM information_schema.TABLES WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='ehrcode'";
			if(PIn.Long(Db.GetScalar(command))==0) {
				return new List<EhrCode>();
			}
			return Crud.EhrCodeCrud.SelectMany("SELECT * FROM ehrcode ORDER BY EhrCodeNum");
		}

		///<summary>The code systems the code list knows, in the order Open Dental's list named them. AdministrativeSex has no table and is always "in
		///the database".</summary>
		public static readonly string[] ArrayCodeSystems=new string[] {
			"AdministrativeSex","CDCREC","CDT","CPT","CVX","HCPCS","ICD9CM","ICD10CM","LOINC","RXNORM","SNOMEDCT","SOP",
		};

		///<summary>The number of rows a code system's table must have more than for its codes to count as in the database (IsInDb). Hard-coded from
		///Open Dental's EHR code list, lowered slightly to give a buffer, in case some codes are removed later. -1 for a code system with no table
		///(AdministrativeSex, or one the list does not know).</summary>
		public static long GetCodeCountMinimum(string codeSystem) {
			Meth.NoCheckMiddleTierRole();
			switch(codeSystem) {
				case "CDCREC":
					return 5;
				case "CDT":
					return 10;
				case "CPT":
					return 300;
				case "CVX":
					return 5;
				case "HCPCS":
					return 20;
				case "ICD9CM":
					return 1500;
				case "ICD10CM":
					return 2000;
				case "LOINC":
					return 20;
				case "RXNORM":
					return 40;
				case "SNOMEDCT":
					return 700;
				case "SOP":
					return 100;
				default:
					return -1;
			}
		}

		///<summary>The number of rows in the code system's table (each count is its own Middle Tier call), or -1 for a code system with no table.</summary>
		public static long GetCodeCountInDb(string codeSystem) {
			Meth.NoCheckMiddleTierRole();
			switch(codeSystem) {
				case "CDCREC":
					return Cdcrecs.GetCodeCount();
				case "CDT":
					return ProcedureCodes.GetCodeCount();
				case "CPT":
					return Cpts.GetCodeCount();
				case "CVX":
					return Cvxs.GetCodeCount();
				case "HCPCS":
					return Hcpcses.GetCodeCount();
				case "ICD9CM":
					return ICD9s.GetCodeCount();
				case "ICD10CM":
					return Icd10s.GetCodeCount();
				case "LOINC":
					return Loincs.GetCodeCount();
				case "RXNORM":
					return RxNorms.GetCodeCount();
				case "SNOMEDCT":
					return Snomeds.GetCodeCount();
				case "SOP":
					return Sops.GetCodeCount();
				default:
					return -1;
			}
		}

		///<summary>True when the codes of the code system count as in the database: always for AdministrativeSex, never for a code system the list
		///does not know, otherwise when its table has more rows than GetCodeCountMinimum.</summary>
		private static bool IsCodeSystemInDb(string codeSystem,long countInDb) {
			if(codeSystem=="AdministrativeSex") {
				return true;
			}
			long countMinimum=GetCodeCountMinimum(codeSystem);
			if(countMinimum<0) {
				return false;
			}
			return countInDb>countMinimum;
		}

		///<summary>If the number of codes in the code tables (I.e. Snomed, Loinc, Cpt, etc.) are greater than the number we expect then this will set
		///IsInDb=true otherwise false. Each table is counted once, the first time a code of its code system needs it.</summary>
		private static void updateCodeExistsHelper() {
			Meth.NoCheckMiddleTierRole();
			if(listt.Count==0) {
				return;
			}
			Dictionary<string,bool> dictIsInDb=new Dictionary<string,bool>();
			for(int i=0;i<listt.Count;i++) {
				if(listt[i].IsInDb) {
					continue;//The codes are already present in the database, so we don't need to check again.
				}
				string codeSystem=listt[i].CodeSystem??"";
				if(!dictIsInDb.TryGetValue(codeSystem,out bool isInDb)) {
					isInDb=IsCodeSystemInDb(codeSystem,codeSystem=="AdministrativeSex" ? -1 : GetCodeCountInDb(codeSystem));
					dictIsInDb[codeSystem]=isInDb;
				}
				listt[i].IsInDb=isInDb;
			}
		}

		///<summary>The code-system status that the Code System Importer shows (EHR-2, step 4; owner decision of 2026-10-18, E-17): for every code
		///system the list knows, and every other code system the imported value sets use, the rows in its table next to the minimum the code list
		///expects and the number of value-set codes of that system. The office then knows which code-system tables it still has to load
		///directly. Since EHR-6a also the version of each loaded table (codesystem.VersionCur) next to the version the imported release names in its
		///Code System Versions file, and whether the loaded table is older, where the two can be compared (EhrValueSetImport.CompareCodeSystemVersions).</summary>
		public static List<EhrCodeSystemStatus> GetCodeSystemStatuses() {
			Meth.NoCheckMiddleTierRole();
			List<EhrCode> listEhrCodes=Listt;
			Dictionary<string,int> dictCountCodes=listEhrCodes.GroupBy(x => x.CodeSystem??"").ToDictionary(x => x.Key,x => x.Count());
			List<string> listCodeSystems=ArrayCodeSystems.ToList();
			listCodeSystems.AddRange(dictCountCodes.Keys.Where(x => !ArrayCodeSystems.Contains(x)).OrderBy(x => x,StringComparer.Ordinal));
			Dictionary<string,string> dictVersionsLoaded=new Dictionary<string,string>(StringComparer.Ordinal);
			foreach(CodeSystem codeSystemRow in CodeSystems.GetForCurrentVersion(true)) {
				dictVersionsLoaded[codeSystemRow.CodeSystemName??""]=codeSystemRow.VersionCur??"";
			}
			Dictionary<string,string> dictVersionsRelease=new Dictionary<string,string>(StringComparer.Ordinal);
			EhrValueSetRelease release=EhrValueSetImport.GetRelease();
			if(release!=null && release.ListCodeSystemVersions!=null) {
				foreach(IGrouping<string,EhrCodeSystemVersion> grouping in release.ListCodeSystemVersions.GroupBy(x => x.CodeSystem??"")) {
					dictVersionsRelease.Add(grouping.Key,string.Join(", ",grouping.Select(x => x.Version??"").Distinct()));
				}
			}
			List<EhrCodeSystemStatus> listStatuses=new List<EhrCodeSystemStatus>();
			foreach(string codeSystem in listCodeSystems) {
				EhrCodeSystemStatus status=new EhrCodeSystemStatus();
				status.CodeSystem=codeSystem;
				status.CountMinimum=GetCodeCountMinimum(codeSystem);
				status.HasTable=status.CountMinimum>=0;
				status.CountInDb=status.HasTable ? GetCodeCountInDb(codeSystem) : -1;
				status.IsInDb=IsCodeSystemInDb(codeSystem,status.CountInDb);
				dictCountCodes.TryGetValue(codeSystem,out status.CountValueSetCodes);
				dictVersionsLoaded.TryGetValue(codeSystem,out status.VersionLoaded);
				status.VersionLoaded=status.VersionLoaded??"";
				dictVersionsRelease.TryGetValue(codeSystem,out status.VersionInRelease);
				status.VersionInRelease=status.VersionInRelease??"";
				int? compare=EhrValueSetImport.CompareCodeSystemVersions(codeSystem,status.VersionLoaded,status.VersionInRelease);
				status.IsVersionCompared=compare.HasValue;
				status.IsOlderThanRelease=status.HasTable && status.IsInDb && compare.HasValue && compare.Value<0;
				listStatuses.Add(status);
			}
			return listStatuses;
		}

		///<summary>Fills the list from a table of ehrcode rows, then works out IsInDb.</summary>
		public static void FillCache(DataTable table) {
			Meth.NoCheckMiddleTierRole();
			lock(lockListt) {
				listt=Crud.EhrCodeCrud.TableToList(table);
				updateCodeExistsHelper();
			}
		}
		#endregion

		///<summary>The measures of every value set that holds the code (EhrCode.MeasureIds joined: the seven measures this program reports, for example
		///"138v15,165v15"). FormSnomeds shows it.</summary>
		public static string GetMeasureIdsForCode(string codeValue,string codeSystem) {
			Meth.NoCheckMiddleTierRole();
			List<EhrCode> listEhrCodes=Listt;//Once: the list is read again when the codes change.
			string retval="";
			for(int i=0;i<listEhrCodes.Count;i++) {
				if(listEhrCodes[i].CodeValue==codeValue && listEhrCodes[i].CodeSystem==codeSystem) {
					if(retval.Contains(listEhrCodes[i].MeasureIds)) {
						continue;
					}
					if(retval!="") {
						retval+=",";
					}
					retval+=listEhrCodes[i].MeasureIds;
				}
			}
			return retval;
		}

		///<summary>Returns a list of EhrCode objects that belong to one of the value sets identified by the ValueSetOIDs supplied.</summary>
		public static List<EhrCode> GetForValueSetOIDs(List<string> listValueSetOIDs) {
			return GetForValueSetOIDs(listValueSetOIDs,false);
		}

		///<summary>Returns a list of EhrCode objects that belong to one of the value sets identified by the ValueSetOIDs supplied AND only those codes that exist in the corresponding table in the database.</summary>
		public static List<EhrCode> GetForValueSetOIDs(List<string> listValueSetOIDs,bool usingIsInDb) {
			List<EhrCode> listEhrCodes=Listt;
			List<EhrCode> retval=new List<EhrCode>();
			for(int i=0;i<listEhrCodes.Count;i++) {
				if(usingIsInDb && !listEhrCodes[i].IsInDb) {
					continue;
				}
				if(listValueSetOIDs.Contains(listEhrCodes[i].ValueSetOID)) {
					retval.Add(listEhrCodes[i]);
				}
			}
			return retval;
		}

		///<summary>Returns a dictionary of CodeValue and CodeSystem pairs where the value set is in the supplied list.</summary>
		public static Dictionary<string,string> GetCodeAndCodeSystem(List<string> listValueSetOIDs,bool usingIsInDb) {
			List<EhrCode> listEhrCodes=Listt;
			Dictionary<string,string> retval=new Dictionary<string,string>();
			for(int i=0;i<listEhrCodes.Count;i++) {
				if(usingIsInDb && !listEhrCodes[i].IsInDb) {
					continue;
				}
				for(int j=0;j<listValueSetOIDs.Count;j++) {
					if(listEhrCodes[i].ValueSetOID!=listValueSetOIDs[j]) {
						continue;
					}
					if(!retval.ContainsKey(listEhrCodes[i].CodeValue)) {
						retval.Add(listEhrCodes[i].CodeValue,listEhrCodes[i].CodeSystem);
					}
					break;
				}
			}
			return retval;
		}

		///<summary>Returns a dictionary of CodeValue,CodeSystem pairs of all codes that belong to every ValueSetOID sent in the incoming list as long as the code exists in the corresponding table in the database.</summary>
		public static Dictionary<string,string> GetCodesExistingInAllSets(List<string> listValueSetOIDs) {
			List<EhrCode> listEhrCodes=Listt;
			Dictionary<string,string> dictionaryRet=new Dictionary<string,string>();
			Dictionary<string,int> dictionaryCodeCount=new Dictionary<string,int>();
			for(int i=0;i<listEhrCodes.Count;i++) {
				if(!listEhrCodes[i].IsInDb) {
					continue;
				}
				for(int j=0;j<listValueSetOIDs.Count;j++) {
					if(listEhrCodes[i].ValueSetOID!=listValueSetOIDs[j]) {
						continue;
					}
					string keyCur=listEhrCodes[i].CodeValue+","+listEhrCodes[i].CodeSystem;
					if(dictionaryCodeCount.ContainsKey(keyCur)) {
						dictionaryCodeCount[keyCur]++;//code already in list, increase find count
					}
					else {
						dictionaryCodeCount.Add(keyCur,1);//new find
					}
				}
			}
			foreach(KeyValuePair<string,int> kpairCur in dictionaryCodeCount) {
				string[] codeValueSystem=kpairCur.Key.Split(new string[] { "," },StringSplitOptions.RemoveEmptyEntries);
				if(dictionaryRet.ContainsKey(codeValueSystem[0])) {
					continue;
				}
				if(kpairCur.Value==listValueSetOIDs.Count) {
					dictionaryRet.Add(codeValueSystem[0],codeValueSystem[1]);
				}
			}
			return dictionaryRet;
		}

		public static List<string> GetValueSetOIDsForCode(string codeValue,string codeSystem) {
			List<EhrCode> listEhrCodes=Listt;
			List<string> listValueSetOIDs=new List<string>();
			for(int i=0;i<listEhrCodes.Count;i++) {
				if(listValueSetOIDs.Contains(listEhrCodes[i].ValueSetOID)) {
					continue;
				}
				if(listEhrCodes[i].CodeValue==codeValue && listEhrCodes[i].CodeSystem==codeSystem) {
					listValueSetOIDs.Add(listEhrCodes[i].ValueSetOID);
				}
			}
			return listValueSetOIDs;
		}

		///<summary>Returns EhrCodes for the specified EhrMeasureEventType ordered by how often and how recently they have been used.  Results are
		///ordered by applying a weight based on the date diff from current date to DateTEvent of the EhrMeasureEvents.  EhrCodes used most
		///recently will have the largest weight and help move the EhrCode to the top of the list.  Specify a limit amount if the result set should only
		///be a certain number of EhrCodes at most.</summary>
		public static List<EhrCode> GetForEventTypeByUse(EhrMeasureEventType ehrMeasureEventTypes) {
			List<EhrCode> listEhrCodesAll=Listt;
			List<EhrCode> listEhrCodes=new List<EhrCode>();
			//list of CodeValueResults of the specified type ordered by a weight calculated by summing values based on how recently the codes were used
			List<string> listCodeValueResults=EhrMeasureEvents.GetListCodesUsedForType(ehrMeasureEventTypes);
			foreach(string codeStr in listCodeValueResults) {
				EhrCode ehrCodeCur=listEhrCodesAll.FirstOrDefault(x => x.CodeValue==codeStr);
				Snomed snomedCur=null;
				if(ehrCodeCur==null) {
					snomedCur=Snomeds.GetByCode(codeStr);
					if(snomedCur==null) {
						continue;
					}
					ehrCodeCur=new EhrCode { CodeValue=snomedCur.SnomedCode,Description=snomedCur.Description };
				}
				listEhrCodes.Add(ehrCodeCur);
			}
			return listEhrCodes.OrderBy(x => x.Description).ToList();
		}

		///<summary>Returns a list of EhrCodes for the specified InterventionCodeSet and ValueSetOID for any medication interventions.  Results will
		///contain both intervention codes and medication codes that have been used in the last year.</summary>
		public static List<EhrCode> GetForIntervAndMedByUse(InterventionCodeSet interventionCodeSet,List<string> listValueSetOIDsMed) {
			List<EhrCode> listEhrCodesAll=Listt;
			//get all MedicationPats where the RxCui is one of the RxCui strings for list of ValueSetOIDs provided
			List<string> listMedPats=MedicationPats.GetAllForRxCuis(listEhrCodesAll.Where(x => x.IsInDb && listValueSetOIDsMed.Contains(x.ValueSetOID))
				.Select(x => x.CodeValue).ToList());
			List<string> listInterventions=Interventions.GetAllForCodeSet(interventionCodeSet);
			List<EhrCode> listEhrCodes=new List<EhrCode>();
			foreach(string codeStr in listMedPats.Union(listInterventions)) {
				EhrCode codeCur=listEhrCodesAll.FirstOrDefault(x => x.CodeValue==codeStr);
				if(codeCur==null) {
					continue;
				}
				listEhrCodes.Add(codeCur);
			}
			//we might find the "wrong" ehr code, because a single code may be in multiple code sets. This is still a valid code.
			return listEhrCodes.OrderBy(x => x.Description).ToList();
		}


		/*
		Only pull out the methods below as you need them.  Otherwise, leave them commented out.

		///<summary></summary>
		public static List<string> GetValueSetFromCodeAndCategory(string codeValue,string codeSystem,string category) {
			List<string> retval=new List<string>();
			for(int i=0;i<Listt.Count;i++) {
				if(Listt[i].CodeValue==codeValue && Listt[i].CodeSystem==codeSystem && Listt[i].QDMCategory==category) {
					retval.Add(Listt[i].ValueSetName);
				}
			}
			return retval;
		}

		///<summary>Used for adding codes, returns a hashset of codevalue+valuesetoid.</summary>
		public static HashSet<string> GetAllCodesHashSet() {
			HashSet<string> retVal=new HashSet<string>();
			for(int i=0;i<Listt.Count;i++) {
				retVal.Add(Listt[i].CodeValue+Listt[i].ValueSetOID);
			}
			return retVal;
		}
		 * 
		///<summary></summary>
		public static List<EhrCode> Refresh(long patNum){
			if(RemotingClient.MiddleTierRole==MiddleTierRole.ClientMT) {
				return Meth.GetObject<List<EhrCode>>(MethodBase.GetCurrentMethod(),patNum);
			}
			string command="SELECT * FROM ehrcode WHERE PatNum = "+POut.Long(patNum);
			return Crud.EhrCodeCrud.SelectMany(command);
		}

		///<summary></summary>
		public static long Insert(EhrCode ehrCode) {
			if(RemotingClient.MiddleTierRole==MiddleTierRole.ClientMT) {
				ehrCode.EhrCodeNum=Meth.GetLong(MethodBase.GetCurrentMethod(),ehrCode);
				return ehrCode.EhrCodeNum;
			}
			return Crud.EhrCodeCrud.Insert(ehrCode);
		}

		///<summary>Gets one EhrCode from the db.</summary>
		public static EhrCode GetOne(long ehrCodeNum){
			if(RemotingClient.MiddleTierRole==MiddleTierRole.ClientMT){
				return Meth.GetObject<EhrCode>(MethodBase.GetCurrentMethod(),ehrCodeNum);
			}
			return Crud.EhrCodeCrud.SelectOne(ehrCodeNum);
		}

		///<summary></summary>
		public static void Update(EhrCode ehrCode){
			if(RemotingClient.MiddleTierRole==MiddleTierRole.ClientMT){
				Meth.GetVoid(MethodBase.GetCurrentMethod(),ehrCode);
				return;
			}
			Crud.EhrCodeCrud.Update(ehrCode);
		}

		///<summary></summary>
		public static void Delete(long ehrCodeNum) {
			if(RemotingClient.MiddleTierRole==MiddleTierRole.ClientMT) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),ehrCodeNum);
				return;
			}
			string command= "DELETE FROM ehrcode WHERE EhrCodeNum = "+POut.Long(ehrCodeNum);
			Db.NonQ(command);
		}
		*/
	}

	///<summary>One row of the Code System Importer's code-system status (EhrCodes.GetCodeSystemStatuses).</summary>
	public class EhrCodeSystemStatus {
		///<summary>The code system's name as the code list writes it, for example SNOMEDCT.</summary>
		public string CodeSystem;
		///<summary>Rows in the code system's table, or -1 when it has none.</summary>
		public long CountInDb;
		///<summary>The number of rows the table must have more than (EhrCodes.GetCodeCountMinimum), or -1 when it has no table.</summary>
		public long CountMinimum;
		///<summary>False for AdministrativeSex and for code systems the code list does not know.</summary>
		public bool HasTable;
		///<summary>True when the value-set codes of this system count as in the database (IsInDb).</summary>
		public bool IsInDb;
		///<summary>Rows of the imported value sets (and direct reference codes) with this code system.</summary>
		public int CountValueSetCodes;
		///<summary>The version of the loaded table (codesystem.VersionCur), or "".</summary>
		public string VersionLoaded="";
		///<summary>The version the imported release names for this code system in its Code System Versions file, or "".</summary>
		public string VersionInRelease="";
		///<summary>True when the two versions could be compared (a mapping exists for this code system and both versions follow it).</summary>
		public bool IsVersionCompared;
		///<summary>True when the table is loaded and older than the release's version: codes of the newer value-set expansions are missing from the
		///pick lists (section 7.4).</summary>
		public bool IsOlderThanRelease;
	}

	///<summary>A value set the measures use: its OID, name, QDM category and the measures that use it, from the catalog (CqmCatalog; the manifest of
	///docs/specs/ehr-2027-measures-plan.md section 7.2). No code of any value set is in it.</summary>
	public class EhrValueSetManifestItem {
		public string ValueSetOID;
		public string ValueSetName;
		public string QDMCategory;
		///<summary>Comma delimited, as EhrCode.MeasureIds: for example 69v15,165v15.</summary>
		public string MeasureIds;
	}

	///<summary>What a file of the release holds (section 7.1). Not stored in the database.</summary>
	public enum EhrValueSetFileKind {
		///<summary>Nothing this importer reads.</summary>
		Unknown,
		///<summary>The value sets: the Excel workbook, the SVS XML, or a .zip of them.</summary>
		ValueSets,
		///<summary>The Direct Reference Codes file.</summary>
		DirectReferenceCodes,
		///<summary>The Code System Versions file.</summary>
		CodeSystemVersions,
		///<summary>Rows of more than one of the three kinds in one file: refused.</summary>
		Mixed,
	}

	///<summary>A direct reference code of the office's Direct Reference Codes file, one per measure, code system and code. No code of any code
	///system is in the program; these come only from the file.</summary>
	public class EhrDirectCode {
		///<summary>Without the version, as the drc: key holds it: CMS69.</summary>
		public string CmsId="";
		///<summary>As EhrCode.MeasureIds writes it: 69v15.</summary>
		public string MeasureId="";
		public string CodeValue="";
		public string Description="";
		public string CodeSystem="";
		public string CodeSystemOID="";
		public string CodeSystemVersion="";
	}

	///<summary>A code system and its version, as the release's Code System Versions file names them.</summary>
	public class EhrCodeSystemVersion {
		///<summary>The code list's name (SNOMEDCT, LOINC, ...), or the file's name without spaces for a code system the list does not know.</summary>
		public string CodeSystem="";
		public string CodeSystemOID="";
		public string Version="";
	}

	///<summary>One of the three files of an import, as the release record keeps it.</summary>
	public class EhrValueSetReleaseFile {
		public EhrValueSetFileKind Kind;
		public string FileName="";
		public string Sha256="";
		public long CountBytes;
	}

	///<summary>How a direct-reference-code role of the catalog (CqmCatalog.GetDirectCodeRoles) resolved against the office's file.</summary>
	public class EhrDirectCodeRoleResult {
		public string Role="";
		public string CmsId="";
		///<summary>Codes of the file that matched the role.</summary>
		public int CountMatched;
		public int CountExpected;
		///<summary>True when the role's codes are written under its key. A role that matches no code, or more than it takes, is not: its criterion
		///is then "not calculated" (section 7.3).</summary>
		public bool IsResolved;
	}

	///<summary>What EhrValueSetImport.Parse read from the office's file, the checks it made, and the rows an import would write. A file that is
	///refused writes nothing.</summary>
	public class EhrValueSetFile {
		public string FileName="";
		///<summary>SHA-256 of the file the office chose (the .zip, .xlsx or .xml), lower-case hex.</summary>
		public string Sha256="";
		public long CountBytes;
		///<summary>The parts read: the file itself, or the workbooks and SVS files inside a .zip, with the sheets read.</summary>
		public List<string> ListPartsRead=new List<string>();
		///<summary>Parts and sheets not read, with the reason.</summary>
		public List<string> ListPartsSkipped=new List<string>();
		///<summary>The reasons the file is refused. Empty when it can be imported.</summary>
		public List<string> ListRefusals=new List<string>();
		public List<string> ListWarnings=new List<string>();
		public List<string> ListNotes=new List<string>();
		///<summary>The rows an import writes: one per value set, code system and code.</summary>
		public List<EhrCode> ListEhrCodes=new List<EhrCode>();
		///<summary>Rows (workbook rows or SVS concepts) read, before merging.</summary>
		public int CountRowsRead;
		///<summary>Rows without a value-set OID or a code, skipped.</summary>
		public int CountRowsSkipped;
		///<summary>Rows with a value longer than its column (255 characters; the description, a text column, 65,535 bytes of UTF-8), skipped.
		///MeasureIds is not checked: it holds only the seven measures this program reports.</summary>
		public int CountRowsTooLong;
		///<summary>Workbook cells with a value to the right of column IV (EhrValueSetImport.MAX_COLUMNS), not read.</summary>
		public int CountCellsIgnored;
		///<summary>Rows that repeat a value set, code system and code already read (a value set listed under several measures), merged into one.</summary>
		public int CountRowsMerged;
		///<summary>The measure IDs the file names, normalized as EhrCode.MeasureIds writes them (68v3).</summary>
		public List<string> ListMeasureIdsInFile=new List<string>();
		///<summary>False when the file names no measure, so the versions could not be checked.</summary>
		public bool AreVersionsChecked;
		///<summary>The manifest's measures the file has at the version this program reports.</summary>
		public List<string> ListMeasuresMatched=new List<string>();
		public List<string> ListManifestOidsFound=new List<string>();
		public List<string> ListManifestOidsMissing=new List<string>();
		///<summary>Value sets of the file that the manifest does not list (other measures of the release). Imported too.</summary>
		public int CountOtherValueSets;
		///<summary>Code systems of the file that this program has no table for (for example ICD10PCS), with their code counts.</summary>
		public List<string> ListCodeSystemsWithoutTable=new List<string>();
		///<summary>Which of the three files of the release this is, by the rows it holds (EHR-6a).</summary>
		public EhrValueSetFileKind Kind;
		///<summary>A Direct Reference Codes file's rows: one per measure, code system and code.</summary>
		public List<EhrDirectCode> ListDirectCodes=new List<EhrDirectCode>();
		///<summary>A Code System Versions file's rows: one per code system.</summary>
		public List<EhrCodeSystemVersion> ListCodeSystemVersions=new List<EhrCodeSystemVersion>();

		public bool IsRefused {
			get {
				return ListRefusals.Count>0;
			}
		}

		public int CountValueSets {
			get {
				return ListEhrCodes.Select(x => x.ValueSetOID).Distinct().Count();
			}
		}
	}

	///<summary>The import the Code System Importer records in the ForkEhrValueSetRelease preference row (JSON): the release, its three files, its
	///code-system versions and the role table (docs/specs/ehr-2027-measures-plan.md, section 7.1).</summary>
	public class EhrValueSetRelease {
		public string Label="";
		///<summary>The value-set file's name (the window shows it).</summary>
		public string FileName="";
		///<summary>The value-set file's SHA-256.</summary>
		public string Sha256="";
		///<summary>UTC, yyyy-MM-ddTHH:mm:ssZ.</summary>
		public string DateImportedUtc="";
		///<summary>Every row written: value-set codes and direct reference codes.</summary>
		public int CountCodes;
		public int CountValueSets;
		public int CountManifestFound;
		public int CountManifest;
		///<summary>"7 of 7 measures at the 2027 versions", or that the versions could not be checked.</summary>
		public string MeasureCheck="";
		///<summary>Rows of the Direct Reference Codes file written (part of CountCodes).</summary>
		public int CountDirectCodes;
		///<summary>The three files: kind, name, SHA-256 and size.</summary>
		public List<EhrValueSetReleaseFile> ListFiles=new List<EhrValueSetReleaseFile>();
		///<summary>The Code System Versions file's rows.</summary>
		public List<EhrCodeSystemVersion> ListCodeSystemVersions=new List<EhrCodeSystemVersion>();
		///<summary>The direct-reference-code roles and how each resolved.</summary>
		public List<EhrDirectCodeRoleResult> ListRoles=new List<EhrDirectCodeRoleResult>();
	}

	///<summary>The office's pick of the three files of the release, read and checked together (EhrValueSetImport.MakePick): the value sets, the
	///Direct Reference Codes and the Code System Versions. A pick without all three, or with a refused file, is refused, and nothing is written
	///(section 7.1). Import writes both kinds of row in one swap.</summary>
	public class EhrValueSetPick {
		///<summary>The files picked, as Parse read each.</summary>
		public List<EhrValueSetFile> ListFiles=new List<EhrValueSetFile>();
		public EhrValueSetFile FileValueSets;
		public EhrValueSetFile FileDirectCodes;
		public EhrValueSetFile FileCodeSystemVersions;
		///<summary>The reasons the pick is refused. Empty when it can be imported.</summary>
		public List<string> ListRefusals=new List<string>();
		public List<string> ListWarnings=new List<string>();
		///<summary>The Direct Reference Codes file's rows as ehrcode rows, under their drc: keys.</summary>
		public List<EhrCode> ListEhrCodesDirect=new List<EhrCode>();
		///<summary>Each role of the catalog and how it resolved.</summary>
		public List<EhrDirectCodeRoleResult> ListRoles=new List<EhrDirectCodeRoleResult>();

		public bool IsRefused {
			get {
				return ListRefusals.Count>0;
			}
		}

		///<summary>Every row an import writes: the value-set codes, then the direct reference codes.</summary>
		public List<EhrCode> ListEhrCodesAll {
			get {
				List<EhrCode> listEhrCodes=new List<EhrCode>();
				if(FileValueSets!=null) {
					listEhrCodes.AddRange(FileValueSets.ListEhrCodes);
				}
				listEhrCodes.AddRange(ListEhrCodesDirect);
				return listEhrCodes;
			}
		}
	}

	///<summary>The office's import of VSAC's "eCQM Value Sets for Eligible Clinicians" published 2026-05-14 for the 2027 performance period, with
	///that release's "Direct Reference Codes" and "Code System Versions" files (docs/specs/ehr-2027-measures-plan.md, EHR-6a, sections 7.1-7.4;
	///owner decision 3): the release whose value sets the 2027 versions of the catalog's seven measures use (CqmCatalog). The office downloads the
	///three files from VSAC under its own UMLS licence (the value sets as the Excel workbook (.xlsx) or the SVS XML (.xml), or a .zip of either)
	///and imports them together in Setup, Chart, EHR, Code System Importer. Nothing of VSAC's content is shipped: the program carries only the
	///catalog (the value sets' OIDs, names, QDM categories and measures, the direct-code roles) and these readers. Parse reads each file whole
	///first and checks it; MakePick checks the three together and resolves the roles; Import then replaces every row of ehrcode, both kinds in
	///one swap, and records the release. A pick without all three files, a file that is refused, or a parse that fails, leaves the old rows.
	///The header names of the three files are provisional until the record of the real files (E-20; docs/port-notes/EHR-6.md).</summary>
	public class EhrValueSetImport {
		///<summary>The fork-owned preference row with the last import (EhrValueSetRelease as JSON). Read by string (no PrefName).</summary>
		public const string PREF_RELEASE="ForkEhrValueSetRelease";
		///<summary>The release this program's measures need (section 7.1).</summary>
		public const string RELEASE_LABEL=CqmCatalog.RELEASE_LABEL;
		///<summary>Largest file the importer reads, and largest part inside a .zip or a workbook once uncompressed (counted as it is read). The
		///program is a 32-bit process, and the parts it holds (a workbook inside a .zip, the rows read) must fit beside the rest of it. The real
		///sizes of the 2026-05-14 files are E-20's record, which decides E-30 (section 7.2).</summary>
		public const long MAX_FILE_BYTES=64L*1024*1024;
		///<summary>Largest total of the uncompressed parts of one file.</summary>
		public const long MAX_TOTAL_BYTES=256L*1024*1024;
		public const int MAX_ZIP_ENTRIES=20000;
		///<summary>Most rows (workbook rows or SVS concepts) of one file, counted while it is read.</summary>
		public const int MAX_ROWS=1000000;
		///<summary>Longest value of a varchar column of ehrcode.</summary>
		public const int MAX_COLUMN_LENGTH=255;
		///<summary>Longest description (ehrcode.Description is text): 65,535 bytes, here counted in UTF-8 (utf8mb3 holds 21,845 characters of three
		///bytes).</summary>
		public const int MAX_TEXT_BYTES=65535;
		///<summary>The workbook columns read: A to IV. VSAC's workbooks have about ten.</summary>
		public const int MAX_COLUMNS=256;
		private const string LANS="EhrValueSetImport";
		///<summary>A test seam (tests/EhrTests): called after each batch of rows the import writes, with the number of rows written so far. Null in
		///the program.</summary>
		internal static Action<int> ActionAfterInsertBatch;

		///<summary>The catalog's seven measures at their 2027 versions, as VSAC's files name them (CqmCatalog.GetMeasureIds): CMS68v16, CMS69v15,
		///CMS74v16, CMS75v15, CMS138v15, CMS155v15, CMS165v15.</summary>
		public static readonly string[] ArrayMeasureIds=CqmCatalog.GetMeasureIds().ToArray();

		///<summary>Code systems by their HL7 OID: the OIDs the engine and the codesystem table already use (ConvertDatabases3, EhrCCD). Both HL7
		///gender OIDs (v3 AdministrativeGender and v2 table 0001) are the code list's AdministrativeSex; both ICD-9-CM OIDs (diagnoses and
		///procedures) are ICD9CM, as VSAC names them. HL7 ActCode (one direct reference code of CMS69) has no table here, so its codes never count
		///as in the database. A code system not in this list keeps the name the file gives it.</summary>
		private static readonly Dictionary<string,string> _dictCodeSystemByOid=new Dictionary<string,string>(StringComparer.Ordinal) {
			{ "2.16.840.1.113883.5.1","AdministrativeSex" },
			{ "2.16.840.1.113883.18.2","AdministrativeSex" },
			{ "2.16.840.1.113883.6.238","CDCREC" },
			{ "2.16.840.1.113883.6.13","CDT" },
			{ "2.16.840.1.113883.6.12","CPT" },
			{ "2.16.840.1.113883.12.292","CVX" },
			{ "2.16.840.1.113883.6.285","HCPCS" },
			{ "2.16.840.1.113883.6.103","ICD9CM" },
			{ "2.16.840.1.113883.6.104","ICD9CM" },
			{ "2.16.840.1.113883.6.90","ICD10CM" },
			{ "2.16.840.1.113883.6.1","LOINC" },
			{ "2.16.840.1.113883.6.88","RXNORM" },
			{ "2.16.840.1.113883.6.96","SNOMEDCT" },
			{ "2.16.840.1.113883.3.221.5","SOP" },
			{ "2.16.840.1.113883.5.4","ActCode" },
		};

		///<summary>The code list's OID of each code system, for a file that names the code system but not its OID.</summary>
		private static readonly Dictionary<string,string> _dictOidByCodeSystem=new Dictionary<string,string>(StringComparer.Ordinal) {
			{ "AdministrativeSex","2.16.840.1.113883.5.1" },
			{ "CDCREC","2.16.840.1.113883.6.238" },
			{ "CDT","2.16.840.1.113883.6.13" },
			{ "CPT","2.16.840.1.113883.6.12" },
			{ "CVX","2.16.840.1.113883.12.292" },
			{ "HCPCS","2.16.840.1.113883.6.285" },
			{ "ICD9CM","2.16.840.1.113883.6.103" },
			{ "ICD10CM","2.16.840.1.113883.6.90" },
			{ "LOINC","2.16.840.1.113883.6.1" },
			{ "RXNORM","2.16.840.1.113883.6.88" },
			{ "SNOMEDCT","2.16.840.1.113883.6.96" },
			{ "SOP","2.16.840.1.113883.3.221.5" },
			{ "ActCode","2.16.840.1.113883.5.4" },
		};

		///<summary>Headers by the field they fill, compared after removing white space and ignoring case (sections 7.1 and 7.3), for the three
		///files: the value-set workbook (CMS ID, Value Set Name, Value Set OID, QDM Category, Code, Description, Code System, Code System OID, Code
		///System Version, the versions), the Direct Reference Codes file (CMS ID, Code, Description, Code System, Code System OID, Code System
		///Version) and the Code System Versions file (Code System, Code System OID, Version). **Provisional:** the real files' headers are E-20's
		///record (docs/port-notes/EHR-6.md); the names VSAC's eCQM files are expected to use come first, then EHR-2's.</summary>
		private static readonly Dictionary<string,string[]> _dictHeaders=new Dictionary<string,string[]> {
			{ nameof(EhrCode.MeasureIds),new[] { "cmsid","cmsids","measureid","measureids","emeasureid","cmsemeasureid","cmsemeasureids","measure" } },
			{ nameof(EhrCode.ValueSetName),new[] { "valuesetname" } },
			{ nameof(EhrCode.ValueSetOID),new[] { "valuesetoid","oid" } },
			{ nameof(EhrCode.QDMCategory),new[] { "qdmcategory","category" } },
			{ nameof(EhrCode.CodeValue),new[] { "code","codevalue","directreferencecode" } },
			{ nameof(EhrCode.Description),new[] { "description","descriptor","codedescription","displayname","codedisplayname","display","codename" } },
			{ nameof(EhrCode.CodeSystem),new[] { "codesystem","codesystemname" } },
			{ nameof(EhrCode.CodeSystemOID),new[] { "codesystemoid" } },
			{ FIELD_CODE_SYSTEM_VERSION,new[] { "codesystemversion" } },
			{ FIELD_VALUE_SET_VERSION,new[] { "expansionversion","definitionversion","valuesetversion","version" } },
		};

		///<summary>The header field of a code-system version (Code System Version).</summary>
		private const string FIELD_CODE_SYSTEM_VERSION="CodeSystemVersion";
		///<summary>The header field of a value set's version, or of a code system's in the Code System Versions file (Version).</summary>
		private const string FIELD_VALUE_SET_VERSION="ValueSetVersion";

		///<summary>How many rows of a sheet are searched for the header row.</summary>
		private const int MAX_HEADER_ROW=30;

		#region Manifest and code systems
		///<summary>The manifest (a new list of new items each call), built from the catalog (section 7.2): the value sets of the measures, which hold
		///the pick-list value sets of section 8 too, each with the measures that declare it at their 2027 versions ("69v15,165v15"). A file's own name,
		///category and measures win; these fill in for a file that has none (an SVS file without Group elements).</summary>
		public static List<EhrValueSetManifestItem> GetManifest() {
			Meth.NoCheckMiddleTierRole();
			Dictionary<string,string> dictMeasureIds=CqmCatalog.GetMeasures().ToDictionary(x => x.CmsId,x => x.MeasureIdShort);
			List<EhrValueSetManifestItem> listItems=new List<EhrValueSetManifestItem>();
			foreach(CqmValueSet valueSet in CqmCatalog.GetValueSets()) {
				EhrValueSetManifestItem item=new EhrValueSetManifestItem();
				item.ValueSetOID=valueSet.Oid;
				item.ValueSetName=valueSet.Name;
				item.QDMCategory=valueSet.QDMCategory;
				item.MeasureIds=string.Join(",",SortMeasureIds(valueSet.ListCmsIds.Select(x => dictMeasureIds[x])));
				listItems.Add(item);
			}
			return listItems;
		}

		///<summary>The manifest's item for the OID, or null.</summary>
		public static EhrValueSetManifestItem GetManifestItem(string valueSetOID) {
			Meth.NoCheckMiddleTierRole();
			return GetManifest().FirstOrDefault(x => x.ValueSetOID==valueSetOID);
		}

		///<summary>The code list's name of the code system with this OID (for example SNOMEDCT for 2.16.840.1.113883.6.96). For an OID the list
		///does not know: the code list's name matching the file's name (ignoring case and spaces), otherwise the file's name with the spaces removed.
		///Blank when both are blank.</summary>
		public static string GetCodeSystemName(string codeSystemOID,string codeSystemNameInFile) {
			Meth.NoCheckMiddleTierRole();
			codeSystemOID=(codeSystemOID??"").Trim();
			if(_dictCodeSystemByOid.TryGetValue(codeSystemOID,out string codeSystem)) {
				return codeSystem;
			}
			string nameCompact=Regex.Replace(codeSystemNameInFile??"",@"\s+","");
			string nameKnown=_dictOidByCodeSystem.Keys.FirstOrDefault(x => string.Equals(x,nameCompact,StringComparison.OrdinalIgnoreCase));
			if(nameKnown!=null) {
				return nameKnown;
			}
			if(string.Equals(nameCompact,"AdministrativeGender",StringComparison.OrdinalIgnoreCase)) {
				return "AdministrativeSex";
			}
			if(string.Equals(nameCompact,"HL7ActCode",StringComparison.OrdinalIgnoreCase)) {
				return "ActCode";
			}
			return nameCompact;
		}

		///<summary>The OID of a code system the code list knows (by its code list name), or "".</summary>
		public static string GetCodeSystemOid(string codeSystem) {
			Meth.NoCheckMiddleTierRole();
			if(codeSystem!=null && _dictOidByCodeSystem.TryGetValue(codeSystem,out string oid)) {
				return oid;
			}
			return "";
		}

		///<summary>Every measure ID in the text (comma, semicolon or space separated; "CMS68v3" or "68v3"), as EhrCode.MeasureIds writes them:
		///"68v3". Other text is ignored.</summary>
		public static List<string> ParseMeasureIds(string text) {
			Meth.NoCheckMiddleTierRole();
			List<string> listMeasureIds=new List<string>();
			if(string.IsNullOrWhiteSpace(text)) {
				return listMeasureIds;
			}
			foreach(Match match in Regex.Matches(text,@"(?<![A-Za-z0-9])(?:CMS)?\s*(\d{1,4})\s*v\s*(\d{1,3})(?![A-Za-z0-9])",RegexOptions.IgnoreCase)) {
				string measureId=int.Parse(match.Groups[1].Value,CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture)+"v"
					+int.Parse(match.Groups[2].Value,CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
				if(!listMeasureIds.Contains(measureId)) {
					listMeasureIds.Add(measureId);
				}
			}
			return listMeasureIds;
		}

		///<summary>The measure IDs in the order of ArrayMeasureIds first, then the others by number and version.</summary>
		public static List<string> SortMeasureIds(IEnumerable<string> listMeasureIds) {
			Meth.NoCheckMiddleTierRole();
			List<string> listKnown=ArrayMeasureIds.Select(x => x.Substring(3)).ToList();
			return listMeasureIds.Distinct()
				.OrderBy(x => listKnown.IndexOf(x)<0 ? int.MaxValue : listKnown.IndexOf(x))
				.ThenBy(x => GetMeasureNumber(x))
				.ThenBy(x => x,StringComparer.Ordinal)
				.ToList();
		}

		///<summary>The number of a measure ID ("74" for "74v3"), or -1.</summary>
		private static int GetMeasureNumber(string measureId) {
			Match match=Regex.Match(measureId??"",@"^(\d+)v\d+$");
			return match.Success ? int.Parse(match.Groups[1].Value,CultureInfo.InvariantCulture) : -1;
		}
		#endregion Manifest and code systems

		#region Parsing
		///<summary>A row as the file gives it, before merging.</summary>
		private class RawRow {
			public string ValueSetOID="";
			public string ValueSetName="";
			public string ValueSetVersion="";
			public string QDMCategory="";
			public List<string> ListMeasureIds=new List<string>();
			public string CodeValue="";
			public string Description="";
			public string CodeSystemName="";
			public string CodeSystemOID="";
			public string CodeSystemVersion="";
		}

		///<summary>The rows read from one file, by kind: value-set rows (workbook or SVS), Direct Reference Codes rows (RawRow without a value-set
		///OID), and Code System Versions rows (RawRow with only the code system and its version).</summary>
		private class RawRows {
			public List<RawRow> ListValueSets=new List<RawRow>();
			public List<RawRow> ListDirectCodes=new List<RawRow>();
			public List<RawRow> ListCodeSystemVersions=new List<RawRow>();
			///<summary>The kinds of the sheets (or SVS files) rows were read from, skipped rows included.</summary>
			public HashSet<EhrValueSetFileKind> SetKinds=new HashSet<EhrValueSetFileKind>();
		}

		///<summary>The uncompressed bytes read so far from the parts of one file, against MAX_TOTAL_BYTES.</summary>
		private class ReadBudget {
			public long CountBytesTotal;
		}

		///<summary>A part that passes one of the importer's limits. Parse refuses the file with the message.</summary>
		private class LimitException:ApplicationException {
			public LimitException(string message):base(message) {
			}
		}

		///<summary>Reads a part (the chosen file, or an entry of a .zip or a workbook) forward only, counting its bytes as they are read: past
		///MAX_FILE_BYTES for the part, or past MAX_TOTAL_BYTES for all the parts of the file, it throws a LimitException. The size a zip entry claims
		///is not trusted.</summary>
		private class LimitedReadStream:Stream {
			private readonly Stream _stream;
			private readonly string _partName;
			private readonly ReadBudget _readBudget;
			private readonly bool _isLeaveOpen;
			private long _countBytes;

			public LimitedReadStream(Stream stream,string partName,ReadBudget readBudget,bool isLeaveOpen) {
				_stream=stream;
				_partName=partName;
				_readBudget=readBudget;
				_isLeaveOpen=isLeaveOpen;
			}

			public override bool CanRead => true;
			public override bool CanSeek => false;
			public override bool CanWrite => false;
			public override long Length => throw new NotSupportedException();

			public override long Position {
				get => _countBytes;
				set => throw new NotSupportedException();
			}

			public override int Read(byte[] buffer,int offset,int count) {
				int countRead=_stream.Read(buffer,offset,count);
				_countBytes+=countRead;
				_readBudget.CountBytesTotal+=countRead;
				if(_countBytes>MAX_FILE_BYTES) {
					throw new LimitException(GetPartLimitText(_partName));
				}
				if(_readBudget.CountBytesTotal>MAX_TOTAL_BYTES) {
					throw new LimitException(GetTotalLimitText(_partName));
				}
				return countRead;
			}

			public override void Flush() {
			}

			public override long Seek(long offset,SeekOrigin origin) {
				throw new NotSupportedException();
			}

			public override void SetLength(long value) {
				throw new NotSupportedException();
			}

			public override void Write(byte[] buffer,int offset,int count) {
				throw new NotSupportedException();
			}

			protected override void Dispose(bool disposing) {
				if(disposing && !_isLeaveOpen) {
					_stream.Dispose();
				}
				base.Dispose(disposing);
			}
		}

		private static string GetPartLimitText(string partName) {
			return Lans.g(LANS,"A part of the file is larger, uncompressed, than the importer reads")+" ("+(MAX_FILE_BYTES/1024/1024)+" MB): "+partName;
		}

		private static string GetTotalLimitText(string partName) {
			return Lans.g(LANS,"The parts of the file are larger together, uncompressed, than the importer reads")+" ("+(MAX_TOTAL_BYTES/1024/1024)+" MB "
				+Lans.g(LANS,"in all")+"; "+Lans.g(LANS,"passed in")+" "+partName+").";
		}

		///<summary>Reads and checks the whole file (a .xlsx workbook, an SVS .xml, or a .zip of either; a workbook named .zip is read as a
		///workbook) and returns what an import would write. Nothing is written. Problems are in the result (ListRefusals): this does not throw for a
		///bad file.</summary>
		public static EhrValueSetFile Parse(string fileName,byte[] arrayBytes) {
			Meth.NoCheckMiddleTierRole();
			using(MemoryStream memoryStream=new MemoryStream(arrayBytes??new byte[0],writable:false)) {
				return Parse(fileName,memoryStream);
			}
		}

		///<summary>Parse of a file the caller opened (the Code System Importer opens a FileStream, so the file is never held whole in memory). The
		///stream must be seekable: it is read once for the SHA-256 and again for the content. It is left open. The parts are read one record at a
		///time: no more than MAX_FILE_BYTES of the file, MAX_FILE_BYTES per uncompressed part and MAX_TOTAL_BYTES in all, MAX_ROWS rows (counted
		///while the file is read) and MAX_COLUMNS columns of a workbook, so a file made to exhaust a 32-bit process is refused, not read.</summary>
		public static EhrValueSetFile Parse(string fileName,Stream stream) {
			Meth.NoCheckMiddleTierRole();
			EhrValueSetFile file=new EhrValueSetFile();
			file.FileName=Path.GetFileName(fileName??"");
			if(stream==null || stream.Length==0) {
				file.ListRefusals.Add(Lans.g(LANS,"The file is empty."));
				return file;
			}
			file.CountBytes=stream.Length;
			if(file.CountBytes>MAX_FILE_BYTES) {
				file.ListRefusals.Add(Lans.g(LANS,"The file is larger than the importer reads")+" ("+(MAX_FILE_BYTES/1024/1024)+" MB).");
				return file;
			}
			stream.Position=0;
			using(SHA256 sha256=SHA256.Create()) {
				file.Sha256=string.Concat(sha256.ComputeHash(stream).Select(x => x.ToString("x2",CultureInfo.InvariantCulture)));
			}
			RawRows rawRows=new RawRows();
			try {
				stream.Position=0;
				ReadPart(file,file.FileName,stream,rawRows,isInsideZip:false,new ReadBudget());
			}
			catch(Exception ex) {
				//A damaged workbook, zip or XML, or a part past a limit: refused, and the old rows stay.
				file.ListRefusals.Add(GetReadErrorText(ex));
				return file;
			}
			if(file.IsRefused) {
				return file;
			}
			Finish(file,rawRows);
			return file;
		}

		///<summary>The refusal for an exception while the file was read: a limit's own message, or "could not be read" with the reason.</summary>
		private static string GetReadErrorText(Exception ex) {
			for(Exception exception=ex;exception!=null;exception=exception.InnerException) {
				if(exception is LimitException) {
					return exception.Message;
				}
				if(exception is OutOfMemoryException) {
					return Lans.g(LANS,"The file is too large for the importer to read.");
				}
			}
			return Lans.g(LANS,"The file could not be read")+": "+ex.Message;
		}

		///<summary>Up to count bytes from the start of a stream (fewer when it is shorter).</summary>
		private static byte[] ReadStart(Stream stream,int count) {
			byte[] arrayBytes=new byte[count];
			int countRead=0;
			while(countRead<count) {
				int countNow=stream.Read(arrayBytes,countRead,count-countRead);
				if(countNow==0) {
					break;
				}
				countRead+=countNow;
			}
			if(countRead<count) {
				Array.Resize(ref arrayBytes,countRead);
			}
			return arrayBytes;
		}

		///<summary>Reads one part: the file itself or a workbook inside a .zip, from a seekable stream at its start.</summary>
		private static void ReadPart(EhrValueSetFile file,string partName,Stream stream,RawRows rawRows,bool isInsideZip,ReadBudget readBudget) {
			string extension=Path.GetExtension(partName).ToLowerInvariant();
			byte[] arrayStart=ReadStart(stream,67);
			stream.Position=0;
			if(IsZip(arrayStart)) {
				using(ZipArchive zipArchive=new ZipArchive(stream,ZipArchiveMode.Read,leaveOpen:true)) {
					if(zipArchive.Entries.Any(x => x.FullName=="xl/workbook.xml")) {
						ReadWorkbook(file,partName,zipArchive,rawRows,readBudget);
						return;
					}
					if(isInsideZip) {
						file.ListPartsSkipped.Add(partName+": "+Lans.g(LANS,"a .zip inside the .zip is not read."));
						return;
					}
					ReadZip(file,partName,zipArchive,rawRows,readBudget);
				}
				return;
			}
			if(extension==".xml" || LooksLikeXml(arrayStart)) {
				using(LimitedReadStream limitedReadStream=new LimitedReadStream(stream,partName,readBudget,isLeaveOpen:true)) {
					ReadSvs(file,partName,limitedReadStream,rawRows);
				}
				return;
			}
			if(isInsideZip) {
				file.ListPartsSkipped.Add(partName+": "+Lans.g(LANS,"not a workbook (.xlsx) or an SVS file (.xml)."));
				return;
			}
			file.ListRefusals.Add(Lans.g(LANS,"The file is not a VSAC workbook (.xlsx), an SVS file (.xml) or a .zip of them."));
		}

		///<summary>"PK": a zip, or a workbook (which is a zip).</summary>
		private static bool IsZip(byte[] arrayStart) {
			return arrayStart.Length>=4 && arrayStart[0]==0x50 && arrayStart[1]==0x4B;
		}

		private static bool LooksLikeXml(byte[] arrayBytes) {
			int start=0;
			if(arrayBytes.Length>=3 && arrayBytes[0]==0xEF && arrayBytes[1]==0xBB && arrayBytes[2]==0xBF) {
				start=3;
			}
			for(int i=start;i<arrayBytes.Length && i<start+64;i++) {
				if(arrayBytes[i]==' ' || arrayBytes[i]=='\r' || arrayBytes[i]=='\n' || arrayBytes[i]=='\t') {
					continue;
				}
				return arrayBytes[i]=='<';
			}
			return false;
		}

		///<summary>Reads every workbook and SVS file of a .zip (any folder). Other entries are skipped and named.</summary>
		private static void ReadZip(EhrValueSetFile file,string partName,ZipArchive zipArchive,RawRows rawRows,ReadBudget readBudget) {
			if(zipArchive.Entries.Count>MAX_ZIP_ENTRIES) {
				file.ListRefusals.Add(Lans.g(LANS,"The .zip has more entries than the importer reads")+" ("+MAX_ZIP_ENTRIES+").");
				return;
			}
			int countPartsRead=0;
			foreach(ZipArchiveEntry zipArchiveEntry in zipArchive.Entries.OrderBy(x => x.FullName,StringComparer.Ordinal)) {
				if(zipArchiveEntry.FullName.EndsWith("/") || zipArchiveEntry.FullName.EndsWith("\\")) {
					continue;//A folder.
				}
				string extension=Path.GetExtension(zipArchiveEntry.Name).ToLowerInvariant();
				string entryName=partName+"/"+zipArchiveEntry.FullName;
				if(extension==".zip") {
					file.ListPartsSkipped.Add(entryName+": "+Lans.g(LANS,"a .zip inside the .zip is not read."));
					continue;
				}
				if(extension!=".xlsx" && extension!=".xml") {
					file.ListPartsSkipped.Add(entryName+": "+Lans.g(LANS,"not a workbook (.xlsx) or an SVS file (.xml)."));
					continue;
				}
				int countPartsBefore=file.ListPartsRead.Count;
				ReadZipEntry(file,entryName,zipArchiveEntry,rawRows,readBudget);
				if(file.IsRefused) {
					return;
				}
				if(file.ListPartsRead.Count>countPartsBefore) {
					countPartsRead++;
				}
			}
			if(countPartsRead==0) {
				file.ListRefusals.Add(Lans.g(LANS,"The .zip has no VSAC workbook (.xlsx) or SVS file (.xml) with value sets."));
			}
		}

		///<summary>Reads a workbook or SVS entry of a .zip. An SVS file is read straight from the entry; a workbook (a zip itself, which needs a
		///seekable stream) is copied into memory first, within MAX_FILE_BYTES.</summary>
		private static void ReadZipEntry(EhrValueSetFile file,string entryName,ZipArchiveEntry zipArchiveEntry,RawRows rawRows,ReadBudget readBudget) {
			byte[] arrayStart;
			using(Stream stream=OpenEntry(zipArchiveEntry,entryName,new ReadBudget())) {//The first bytes, not counted twice.
				arrayStart=ReadStart(stream,67);
			}
			if(IsZip(arrayStart)) {
				using(MemoryStream memoryStream=CopyEntryToMemory(zipArchiveEntry,entryName,readBudget)) {
					ReadPart(file,entryName,memoryStream,rawRows,isInsideZip:true,readBudget);
				}
				return;
			}
			if(Path.GetExtension(entryName).ToLowerInvariant()==".xml" || LooksLikeXml(arrayStart)) {
				using(Stream stream=OpenEntry(zipArchiveEntry,entryName,readBudget)) {
					ReadSvs(file,entryName,stream,rawRows);
				}
				return;
			}
			file.ListPartsSkipped.Add(entryName+": "+Lans.g(LANS,"not a workbook (.xlsx) or an SVS file (.xml)."));
		}

		///<summary>The entry's uncompressed bytes as a forward-only stream that counts them (LimitedReadStream). An entry that claims more than
		///MAX_FILE_BYTES is refused before it is read.</summary>
		private static Stream OpenEntry(ZipArchiveEntry zipArchiveEntry,string entryName,ReadBudget readBudget) {
			if(zipArchiveEntry.Length>MAX_FILE_BYTES) {
				throw new LimitException(GetPartLimitText(entryName));
			}
			return new LimitedReadStream(zipArchiveEntry.Open(),entryName,readBudget,isLeaveOpen:false);
		}

		///<summary>A workbook inside a .zip, in memory (at most MAX_FILE_BYTES; the stream is sized by the entry's claimed length).</summary>
		private static MemoryStream CopyEntryToMemory(ZipArchiveEntry zipArchiveEntry,string entryName,ReadBudget readBudget) {
			MemoryStream memoryStream=new MemoryStream((int)Math.Min(Math.Max(zipArchiveEntry.Length,0),MAX_FILE_BYTES));
			try {
				using(Stream stream=OpenEntry(zipArchiveEntry,entryName,readBudget)) {
					stream.CopyTo(memoryStream);
				}
			}
			catch {
				memoryStream.Dispose();
				throw;
			}
			memoryStream.Position=0;
			return memoryStream;
		}

		private static XmlReaderSettings GetXmlReaderSettings() {
			XmlReaderSettings xmlReaderSettings=new XmlReaderSettings();
			xmlReaderSettings.DtdProcessing=DtdProcessing.Prohibit;//No entities, no external files.
			xmlReaderSettings.XmlResolver=null;
			xmlReaderSettings.IgnoreComments=true;
			xmlReaderSettings.IgnoreProcessingInstructions=true;
			xmlReaderSettings.CloseInput=false;
			return xmlReaderSettings;
		}

		///<summary>The text of the element the reader is on (every text node inside it, as XElement.Value gives it), leaving the reader on its end
		///element (or on the element itself when it is empty).</summary>
		private static string ReadElementText(XmlReader xmlReader) {
			if(xmlReader.IsEmptyElement) {
				return "";
			}
			int depthElement=xmlReader.Depth;
			string text="";
			while(xmlReader.Read()) {
				if(xmlReader.NodeType==XmlNodeType.EndElement && xmlReader.Depth==depthElement) {
					break;
				}
				if(IsTextNode(xmlReader.NodeType)) {
					text+=xmlReader.Value;
				}
			}
			return text;
		}

		private static bool IsTextNode(XmlNodeType xmlNodeType) {
			return xmlNodeType==XmlNodeType.Text || xmlNodeType==XmlNodeType.CDATA || xmlNodeType==XmlNodeType.Whitespace
				|| xmlNodeType==XmlNodeType.SignificantWhitespace;
		}

		#region Workbook
		///<summary>Reads every sheet of an Excel workbook (Office Open XML: the sheet XML and the shared strings, with System.IO.Compression; no
		///library), each part straight from its entry. A sheet with no header row is skipped and named. A header row that lacks a required column
		///refuses the file and names the missing columns.</summary>
		private static void ReadWorkbook(EhrValueSetFile file,string partName,ZipArchive zipArchive,RawRows rawRows,ReadBudget readBudget) {
			List<string> listSharedStrings=new List<string>();
			ZipArchiveEntry entryShared=zipArchive.GetEntry("xl/sharedStrings.xml");
			if(entryShared!=null) {
				using(Stream stream=OpenEntry(entryShared,partName+"/"+entryShared.FullName,readBudget)) {
					listSharedStrings=ReadSharedStrings(stream);
				}
			}
			//The sheets in workbook order, with the part each relationship points to.
			Dictionary<string,string> dictTargetById=new Dictionary<string,string>(StringComparer.Ordinal);
			ZipArchiveEntry entryRels=zipArchive.GetEntry("xl/_rels/workbook.xml.rels");
			if(entryRels!=null) {
				using(Stream stream=OpenEntry(entryRels,partName+"/"+entryRels.FullName,readBudget))
				using(XmlReader xmlReader=XmlReader.Create(stream,GetXmlReaderSettings())) {
					while(xmlReader.Read()) {
						if(xmlReader.NodeType==XmlNodeType.Element && xmlReader.LocalName=="Relationship") {
							string id=xmlReader.GetAttribute("Id")??"";
							string target=xmlReader.GetAttribute("Target")??"";
							dictTargetById[id]=target;
						}
					}
				}
			}
			List<KeyValuePair<string,string>> listSheets=new List<KeyValuePair<string,string>>();//sheet name, part path
			ZipArchiveEntry entryWorkbook=zipArchive.GetEntry("xl/workbook.xml");
			using(Stream stream=OpenEntry(entryWorkbook,partName+"/"+entryWorkbook.FullName,readBudget))
			using(XmlReader xmlReader=XmlReader.Create(stream,GetXmlReaderSettings())) {
				while(xmlReader.Read()) {
					if(xmlReader.NodeType!=XmlNodeType.Element || xmlReader.LocalName!="sheet") {
						continue;
					}
					string sheetName=xmlReader.GetAttribute("name")??"";
					string relationshipId="";
					for(int i=0;i<xmlReader.AttributeCount;i++) {
						xmlReader.MoveToAttribute(i);
						if(xmlReader.LocalName=="id" && xmlReader.Prefix!="") {
							relationshipId=xmlReader.Value;
						}
					}
					xmlReader.MoveToElement();
					if(!dictTargetById.TryGetValue(relationshipId,out string target)) {
						continue;
					}
					string path=target.StartsWith("/") ? target.TrimStart('/') : "xl/"+target;
					listSheets.Add(new KeyValuePair<string,string>(sheetName,NormalizeZipPath(path)));
				}
			}
			int countSheetsRead=0;
			foreach(KeyValuePair<string,string> pairSheet in listSheets) {
				ZipArchiveEntry entrySheet=zipArchive.GetEntry(pairSheet.Value);
				string sheetLabel=partName+" ["+pairSheet.Key+"]";
				if(entrySheet==null) {
					file.ListPartsSkipped.Add(sheetLabel+": "+Lans.g(LANS,"the sheet's part is missing."));
					continue;
				}
				using(Stream stream=OpenEntry(entrySheet,partName+"/"+entrySheet.FullName,readBudget)) {
					if(ReadSheet(file,sheetLabel,ReadSheetRows(file,stream,listSharedStrings),rawRows)) {
						countSheetsRead++;
					}
				}
				if(file.IsRefused) {
					return;
				}
			}
			if(countSheetsRead>0) {
				file.ListPartsRead.Add(partName);
			}
			else {
				file.ListPartsSkipped.Add(partName+": "+Lans.g(LANS,"no sheet has a header row with the value-set columns."));
			}
		}

		///<summary>"xl/worksheets/../worksheets/sheet1.xml" as "xl/worksheets/sheet1.xml".</summary>
		private static string NormalizeZipPath(string path) {
			List<string> listParts=new List<string>();
			foreach(string part in path.Replace('\\','/').Split('/')) {
				if(part=="" || part==".") {
					continue;
				}
				if(part=="..") {
					if(listParts.Count>0) {
						listParts.RemoveAt(listParts.Count-1);
					}
					continue;
				}
				listParts.Add(part);
			}
			return string.Join("/",listParts);
		}

		///<summary>The shared strings of a workbook, in order: the text of each si element (its t, or the t of its runs; phonetic runs left out).</summary>
		private static List<string> ReadSharedStrings(Stream stream) {
			List<string> listStrings=new List<string>();
			using(XmlReader xmlReader=XmlReader.Create(stream,GetXmlReaderSettings())) {
				while(xmlReader.Read()) {
					if(xmlReader.NodeType==XmlNodeType.Element && xmlReader.LocalName=="si") {
						listStrings.Add(ReadCellContent(xmlReader,out string _));
					}
				}
			}
			return listStrings;
		}

		///<summary>Reads the cell (c) or shared string (si) element the reader is on: returns the text of its t elements outside phonetic runs (rPh),
		///and gives the text of its first v child in valueV (null when it has none). Leaves the reader on the element's end (or on the element when it
		///is empty).</summary>
		private static string ReadCellContent(XmlReader xmlReader,out string valueV) {
			valueV=null;
			if(xmlReader.IsEmptyElement) {
				return "";
			}
			int depthElement=xmlReader.Depth;
			int depthPhonetic=-1;//The open rPh element's depth.
			int depthText=-1;//The open t or v element's depth, whose text is being read.
			bool isTextV=false;
			string textT=null;
			while(xmlReader.Read()) {
				XmlNodeType xmlNodeType=xmlReader.NodeType;
				if(xmlNodeType==XmlNodeType.EndElement) {
					if(xmlReader.Depth==depthElement) {
						break;
					}
					if(xmlReader.Depth==depthPhonetic) {
						depthPhonetic=-1;
					}
					if(xmlReader.Depth==depthText) {
						depthText=-1;
					}
					continue;
				}
				if(xmlNodeType==XmlNodeType.Element) {
					if(depthText>=0) {
						continue;
					}
					string localName=xmlReader.LocalName;
					if(localName=="rPh") {
						if(depthPhonetic<0 && !xmlReader.IsEmptyElement) {
							depthPhonetic=xmlReader.Depth;
						}
					}
					else if(localName=="t" && depthPhonetic<0) {
						textT=textT??"";
						if(!xmlReader.IsEmptyElement) {
							depthText=xmlReader.Depth;
							isTextV=false;
						}
					}
					else if(localName=="v" && xmlReader.Depth==depthElement+1 && valueV==null) {
						valueV="";
						if(!xmlReader.IsEmptyElement) {
							depthText=xmlReader.Depth;
							isTextV=true;
						}
					}
					continue;
				}
				if(depthText>=0 && xmlReader.Depth==depthText+1 && IsTextNode(xmlNodeType)) {
					if(isTextV) {
						valueV+=xmlReader.Value;
					}
					else {
						textT+=xmlReader.Value;
					}
				}
			}
			return textT??"";
		}

		///<summary>The rows of a sheet, one at a time as the sheet's part is read, as lists of cell texts by column (A is 0). Missing cells are "".
		///Shared strings, inline strings, formula strings, numbers (as the sheet stores them) and booleans. Cells to the right of column IV
		///(MAX_COLUMNS) are not read, and counted (CountCellsIgnored): a cell far to the right would otherwise cost a whole row of empty cells.</summary>
		private static IEnumerable<List<string>> ReadSheetRows(EhrValueSetFile file,Stream stream,List<string> listSharedStrings) {
			using(XmlReader xmlReader=XmlReader.Create(stream,GetXmlReaderSettings())) {
				while(xmlReader.Read()) {
					if(xmlReader.NodeType!=XmlNodeType.Element || xmlReader.LocalName!="row") {
						continue;
					}
					List<string> listRow=new List<string>();
					if(!xmlReader.IsEmptyElement) {
						int depthRow=xmlReader.Depth;
						int indexColumnNext=0;
						while(xmlReader.Read()) {
							if(xmlReader.NodeType==XmlNodeType.EndElement && xmlReader.Depth==depthRow) {
								break;
							}
							if(xmlReader.NodeType!=XmlNodeType.Element || xmlReader.LocalName!="c" || xmlReader.Depth!=depthRow+1) {
								continue;
							}
							string reference=xmlReader.GetAttribute("r");
							string type=xmlReader.GetAttribute("t")??"";
							int indexColumn=reference==null ? -1 : GetColumnIndex(reference);
							if(indexColumn<0) {
								indexColumn=indexColumnNext;
							}
							indexColumnNext=indexColumn+1;
							string textInline=ReadCellContent(xmlReader,out string valueV);
							string value;
							if(type=="s") {
								value="";
								if(int.TryParse(valueV,NumberStyles.Integer,CultureInfo.InvariantCulture,out int indexShared)
									&& indexShared>=0 && indexShared<listSharedStrings.Count)
								{
									value=listSharedStrings[indexShared];
								}
							}
							else if(type=="inlineStr") {
								value=textInline;
							}
							else if(type=="b") {
								value=valueV=="1" ? "TRUE" : "FALSE";
							}
							else {
								value=valueV??"";//"n" (the default), "str" (a formula's text) and "e" (an error) as the sheet stores them.
							}
							if(indexColumn>=MAX_COLUMNS) {
								if(value!="") {
									file.CountCellsIgnored++;
								}
								continue;
							}
							while(listRow.Count<=indexColumn) {
								listRow.Add("");
							}
							listRow[indexColumn]=value;
						}
					}
					yield return listRow;
				}
			}
		}

		///<summary>The column index of a cell reference ("C12" is 2), or -1.</summary>
		private static int GetColumnIndex(string reference) {
			int index=0;
			int countLetters=0;
			foreach(char c in reference) {
				if(c>='A' && c<='Z') {
					index=index*26+(c-'A'+1);
					countLetters++;
				}
				else if(c>='a' && c<='z') {
					index=index*26+(c-'a'+1);
					countLetters++;
				}
				else {
					break;
				}
			}
			if(countLetters==0 || countLetters>3) {
				return -1;
			}
			return index-1;
		}

		///<summary>The field a header names (for example ValueSetOID for "Value Set OID"), or null.</summary>
		private static string GetHeaderField(string header) {
			string headerCompact=Regex.Replace(header??"",@"\s+","").ToLowerInvariant();
			foreach(KeyValuePair<string,string[]> pair in _dictHeaders) {
				if(pair.Value.Contains(headerCompact)) {
					return pair.Key;
				}
			}
			return null;
		}

		///<summary>The kind of sheet a header row names, or Unknown when it is no header row. A value-set sheet names a Value Set OID (or a value-set
		///name or QDM category beside a code); a Direct Reference Codes sheet names a Code without a value set; a Code System Versions sheet names a
		///code system and a version without a code (section 7.1; the real headers are E-20's record).</summary>
		private static EhrValueSetFileKind GetSheetKind(Dictionary<string,int> dictColumns) {
			if(dictColumns.Count<2) {
				return EhrValueSetFileKind.Unknown;
			}
			bool hasCode=dictColumns.ContainsKey(nameof(EhrCode.CodeValue));
			bool hasCodeSystem=dictColumns.ContainsKey(nameof(EhrCode.CodeSystem)) || dictColumns.ContainsKey(nameof(EhrCode.CodeSystemOID));
			bool hasVersion=dictColumns.ContainsKey(FIELD_VALUE_SET_VERSION) || dictColumns.ContainsKey(FIELD_CODE_SYSTEM_VERSION);
			if(dictColumns.ContainsKey(nameof(EhrCode.ValueSetOID))
				|| (hasCode && (dictColumns.ContainsKey(nameof(EhrCode.ValueSetName)) || dictColumns.ContainsKey(nameof(EhrCode.QDMCategory)))))
			{
				return EhrValueSetFileKind.ValueSets;
			}
			if(hasCode) {
				return EhrValueSetFileKind.DirectReferenceCodes;
			}
			if(hasCodeSystem && hasVersion) {
				return EhrValueSetFileKind.CodeSystemVersions;
			}
			return EhrValueSetFileKind.Unknown;
		}

		///<summary>The required columns a header row of this kind lacks, as the refusal names them.</summary>
		private static List<string> GetMissingColumns(EhrValueSetFileKind kind,Dictionary<string,int> dictColumns) {
			List<string> listMissing=new List<string>();
			bool hasCodeSystem=dictColumns.ContainsKey(nameof(EhrCode.CodeSystem)) || dictColumns.ContainsKey(nameof(EhrCode.CodeSystemOID));
			if(kind==EhrValueSetFileKind.ValueSets && !dictColumns.ContainsKey(nameof(EhrCode.ValueSetOID))) {
				listMissing.Add("Value Set OID");
			}
			if(kind==EhrValueSetFileKind.DirectReferenceCodes && !dictColumns.ContainsKey(nameof(EhrCode.MeasureIds))) {
				listMissing.Add("CMS ID");
			}
			if(kind!=EhrValueSetFileKind.CodeSystemVersions && !dictColumns.ContainsKey(nameof(EhrCode.CodeValue))) {
				listMissing.Add("Code");
			}
			if(!hasCodeSystem) {
				listMissing.Add("Code System (or Code System OID)");
			}
			return listMissing;
		}

		///<summary>Reads the rows under the sheet's header row, as they are read from the sheet (each row goes to the header search, then to
		///AddRawRow, so MAX_ROWS refuses the file while it is read). The header row decides the sheet's kind (GetSheetKind): value sets, direct
		///reference codes or code-system versions. Returns false (and names the sheet) when the sheet has no header row: none of its first rows
		///names the columns of one of the three kinds; the rest of such a sheet is not read. A header row without the required columns refuses the
		///file.</summary>
		private static bool ReadSheet(EhrValueSetFile file,string sheetLabel,IEnumerable<List<string>> enumerableRows,RawRows rawRows) {
			int indexRow=-1;
			int indexHeader=-1;
			EhrValueSetFileKind kind=EhrValueSetFileKind.Unknown;
			Dictionary<string,int> dictColumns=null;
			foreach(List<string> listRow in enumerableRows) {
				indexRow++;
				if(indexHeader<0) {
					if(indexRow>=MAX_HEADER_ROW) {
						break;
					}
					Dictionary<string,int> dictColumnsRow=new Dictionary<string,int>();
					for(int j=0;j<listRow.Count;j++) {
						string field=GetHeaderField(listRow[j]);
						if(field!=null && !dictColumnsRow.ContainsKey(field)) {
							dictColumnsRow.Add(field,j);
						}
					}
					kind=GetSheetKind(dictColumnsRow);
					if(kind==EhrValueSetFileKind.Unknown) {
						continue;
					}
					indexHeader=indexRow;
					dictColumns=dictColumnsRow;
					rawRows.SetKinds.Add(kind);
					List<string> listMissing=GetMissingColumns(kind,dictColumns);
					if(listMissing.Count>0) {
						file.ListRefusals.Add(sheetLabel+": "+Lans.g(LANS,"the header row (row")+" "+(indexHeader+1)+") "+Lans.g(LANS,"has no column")+" "
							+string.Join(", ",listMissing)+".");
						return false;
					}
					continue;
				}
				if(listRow.All(x => string.IsNullOrWhiteSpace(x))) {
					continue;
				}
				RawRow rawRow=new RawRow();
				rawRow.ValueSetOID=GetCell(listRow,dictColumns,nameof(EhrCode.ValueSetOID));
				rawRow.ValueSetName=GetCell(listRow,dictColumns,nameof(EhrCode.ValueSetName));
				rawRow.ValueSetVersion=GetCell(listRow,dictColumns,FIELD_VALUE_SET_VERSION);
				rawRow.QDMCategory=GetCell(listRow,dictColumns,nameof(EhrCode.QDMCategory));
				rawRow.ListMeasureIds=ParseMeasureIds(GetCell(listRow,dictColumns,nameof(EhrCode.MeasureIds)));
				rawRow.CodeValue=GetCell(listRow,dictColumns,nameof(EhrCode.CodeValue));
				rawRow.Description=GetCell(listRow,dictColumns,nameof(EhrCode.Description));
				rawRow.CodeSystemName=GetCell(listRow,dictColumns,nameof(EhrCode.CodeSystem));
				rawRow.CodeSystemOID=GetCell(listRow,dictColumns,nameof(EhrCode.CodeSystemOID));
				rawRow.CodeSystemVersion=GetCell(listRow,dictColumns,FIELD_CODE_SYSTEM_VERSION);
				if(kind==EhrValueSetFileKind.CodeSystemVersions && rawRow.CodeSystemVersion=="") {
					rawRow.CodeSystemVersion=rawRow.ValueSetVersion;//The file's Version column is the code system's.
				}
				if(!AddRawRow(file,rawRows,rawRow,kind)) {
					return true;
				}
			}
			if(indexHeader<0) {
				file.ListPartsSkipped.Add(sheetLabel+": "+Lans.g(LANS,"no header row with the columns of a value-set, Direct Reference Codes or Code System Versions file."));
				return false;
			}
			file.ListPartsRead.Add(sheetLabel);
			return true;
		}

		private static string GetCell(List<string> listRow,Dictionary<string,int> dictColumns,string field) {
			if(!dictColumns.TryGetValue(field,out int index) || index>=listRow.Count) {
				return "";
			}
			return (listRow[index]??"").Trim();
		}
		#endregion Workbook

		#region SVS
		///<summary>Reads an SVS file (IHE Sharing Value Sets, as VSAC's SVS API returns it): DescribedValueSet (or ValueSet) elements with their ID,
		///displayName and version, their ConceptList/Concept entries (code, codeSystem, codeSystemName, displayName), and their Group elements, when
		///present, for the QDM category (a group named CATEGORY) and the measure IDs (a group whose name has CMS or Measure in it). Element names are
		///matched without their namespace. Read one node at a time: each concept is a row as soon as it is read (MAX_ROWS counts it), and gets its
		///value set's category and measures when the value set ends, since the groups follow the concepts.</summary>
		private static void ReadSvs(EhrValueSetFile file,string partName,Stream stream,RawRows rawRows) {
			int countValueSets=0;
			using(XmlReader xmlReader=XmlReader.Create(stream,GetXmlReaderSettings())) {
				while(xmlReader.Read()) {
					if(xmlReader.NodeType!=XmlNodeType.Element || (xmlReader.LocalName!="DescribedValueSet" && xmlReader.LocalName!="ValueSet")) {
						continue;
					}
					countValueSets++;
					string valueSetOID=(xmlReader.GetAttribute("ID")??"").Trim();
					string valueSetName=(xmlReader.GetAttribute("displayName")??"").Trim();
					string valueSetVersion=(xmlReader.GetAttribute("version")??"").Trim();
					List<string> listCategories=new List<string>();
					List<string> listMeasureIds=new List<string>();
					List<RawRow> listRowsOfValueSet=new List<RawRow>();
					if(!xmlReader.IsEmptyElement) {
						int depthValueSet=xmlReader.Depth;
						while(xmlReader.Read()) {
							if(xmlReader.NodeType==XmlNodeType.EndElement && xmlReader.Depth==depthValueSet) {
								break;
							}
							if(xmlReader.NodeType!=XmlNodeType.Element) {
								continue;
							}
							if(xmlReader.LocalName=="Concept") {
								RawRow rawRow=new RawRow();
								rawRow.ValueSetOID=valueSetOID;
								rawRow.ValueSetName=valueSetName;
								rawRow.ValueSetVersion=valueSetVersion;
								rawRow.CodeValue=(xmlReader.GetAttribute("code")??"").Trim();
								rawRow.CodeSystemOID=(xmlReader.GetAttribute("codeSystem")??"").Trim();
								rawRow.CodeSystemName=(xmlReader.GetAttribute("codeSystemName")??"").Trim();
								rawRow.Description=(xmlReader.GetAttribute("displayName")??"").Trim();
								if(!AddRawRow(file,rawRows,rawRow,EhrValueSetFileKind.ValueSets)) {
									return;
								}
								listRowsOfValueSet.Add(rawRow);
								continue;
							}
							if(xmlReader.LocalName!="Group" || xmlReader.Depth!=depthValueSet+1) {
								continue;
							}
							string groupName=(xmlReader.GetAttribute("displayName")??"").Trim();
							List<string> listKeywords=new List<string>();
							if(!xmlReader.IsEmptyElement) {
								int depthGroup=xmlReader.Depth;
								while(xmlReader.Read()) {
									if(xmlReader.NodeType==XmlNodeType.EndElement && xmlReader.Depth==depthGroup) {
										break;
									}
									if(xmlReader.NodeType==XmlNodeType.Element && xmlReader.LocalName=="Keyword" && xmlReader.Depth==depthGroup+1) {
										string keyword=ReadElementText(xmlReader).Trim();
										if(keyword!="") {
											listKeywords.Add(keyword);
										}
									}
								}
							}
							if(groupName.Equals("CATEGORY",StringComparison.OrdinalIgnoreCase)) {
								listCategories.AddRange(listKeywords.Where(x => !listCategories.Contains(x)));
							}
							else if(groupName.IndexOf("CMS",StringComparison.OrdinalIgnoreCase)>=0 || groupName.IndexOf("Measure",StringComparison.OrdinalIgnoreCase)>=0) {
								foreach(string keyword in listKeywords) {
									listMeasureIds.AddRange(ParseMeasureIds(keyword).Where(x => !listMeasureIds.Contains(x)));
								}
							}
						}
					}
					string qdmCategory=string.Join(", ",listCategories);
					foreach(RawRow rawRow in listRowsOfValueSet) {
						rawRow.QDMCategory=qdmCategory;
						rawRow.ListMeasureIds=listMeasureIds;//Read only from here on: the rows of one value set share it.
					}
				}
			}
			if(countValueSets==0) {
				file.ListPartsSkipped.Add(partName+": "+Lans.g(LANS,"no DescribedValueSet or ValueSet element (not an SVS file)."));
				if(!partName.Contains("/")) {
					file.ListRefusals.Add(Lans.g(LANS,"The file is not an SVS file: it has no DescribedValueSet or ValueSet element."));
				}
				return;
			}
			file.ListPartsRead.Add(partName);
		}
		#endregion SVS

		///<summary>Adds a row read from the file, of the kind of its sheet, skipping and counting one without what that kind needs (a value-set OID and
		///a code; a code; a code system and a version). Returns false when the file passes MAX_ROWS (the file is then refused).</summary>
		private static bool AddRawRow(EhrValueSetFile file,RawRows rawRows,RawRow rawRow,EhrValueSetFileKind kind) {
			file.CountRowsRead++;
			if(file.CountRowsRead>MAX_ROWS) {
				file.ListRefusals.Add(Lans.g(LANS,"The file has more rows than the importer reads")+" ("+MAX_ROWS+").");
				return false;
			}
			rawRows.SetKinds.Add(kind);
			switch(kind) {
				case EhrValueSetFileKind.DirectReferenceCodes:
					if(rawRow.CodeValue=="") {
						file.CountRowsSkipped++;
						return true;
					}
					rawRows.ListDirectCodes.Add(rawRow);
					return true;
				case EhrValueSetFileKind.CodeSystemVersions:
					if((rawRow.CodeSystemName=="" && rawRow.CodeSystemOID=="") || rawRow.CodeSystemVersion=="") {
						file.CountRowsSkipped++;
						return true;
					}
					rawRows.ListCodeSystemVersions.Add(rawRow);
					return true;
				default:
					if(rawRow.ValueSetOID=="" || rawRow.CodeValue=="") {
						file.CountRowsSkipped++;
						return true;
					}
					rawRows.ListValueSets.Add(rawRow);
					return true;
			}
		}

		///<summary>Decides which of the three files of the release this is by the rows it holds, then finishes it as that kind. A file with rows of
		///more than one kind is refused: the office picks each file as VSAC gives it (section 7.1).</summary>
		private static void Finish(EhrValueSetFile file,RawRows rawRows) {
			if(file.CountCellsIgnored>0) {
				file.ListNotes.Add(file.CountCellsIgnored+" "+Lans.g(LANS,"cells to the right of column IV (the 256th column) were not read."));
			}
			List<EhrValueSetFileKind> listKinds=rawRows.SetKinds.Where(x => x!=EhrValueSetFileKind.Unknown).OrderBy(x => x).ToList();
			if(listKinds.Count>1) {
				file.Kind=EhrValueSetFileKind.Mixed;
				file.ListRefusals.Add(Lans.g(LANS,"The file holds the rows of more than one of the release's three files")+" ("
					+string.Join(", ",listKinds.Select(x => GetKindText(x)))+"). "
					+Lans.g(LANS,"Pick the value sets, the Direct Reference Codes file and the Code System Versions file each as VSAC gives it."));
				return;
			}
			file.Kind=listKinds.Count==0 ? EhrValueSetFileKind.Unknown : listKinds[0];
			switch(file.Kind) {
				case EhrValueSetFileKind.DirectReferenceCodes:
					FinishDirectCodes(file,rawRows.ListDirectCodes);
					return;
				case EhrValueSetFileKind.CodeSystemVersions:
					FinishCodeSystemVersions(file,rawRows.ListCodeSystemVersions);
					return;
				default:
					FinishValueSets(file,rawRows.ListValueSets);
					return;
			}
		}

		///<summary>What a kind of file is called in the report.</summary>
		public static string GetKindText(EhrValueSetFileKind kind) {
			Meth.NoCheckMiddleTierRole();
			switch(kind) {
				case EhrValueSetFileKind.ValueSets:
					return Lans.g(LANS,"the value sets");
				case EhrValueSetFileKind.DirectReferenceCodes:
					return Lans.g(LANS,"the Direct Reference Codes file");
				case EhrValueSetFileKind.CodeSystemVersions:
					return Lans.g(LANS,"the Code System Versions file");
				case EhrValueSetFileKind.Mixed:
					return Lans.g(LANS,"rows of more than one file");
				default:
					return Lans.g(LANS,"nothing this importer reads");
			}
		}

		///<summary>Merges the value-set rows (one per value set, code system and code, with every measure that lists it), fills in from the manifest
		///what the file does not give, and makes the checks of sections 7.1 and 7.2.</summary>
		private static void FinishValueSets(EhrValueSetFile file,List<RawRow> listRawRows) {
			Dictionary<string,EhrValueSetManifestItem> dictManifest=GetManifest().ToDictionary(x => x.ValueSetOID,x => x);
			HashSet<string> setMeasureIdsReported=new HashSet<string>(ArrayMeasureIds.Select(x => x.Substring(3)),StringComparer.Ordinal);//"68v16"
			Dictionary<string,EhrCode> dictEhrCodes=new Dictionary<string,EhrCode>(StringComparer.Ordinal);
			Dictionary<string,List<string>> dictMeasureIds=new Dictionary<string,List<string>>(StringComparer.Ordinal);
			List<string> listMeasureIdsInFile=new List<string>();
			List<string> listVersions=new List<string>();
			foreach(RawRow rawRow in listRawRows) {
				string codeSystem=GetCodeSystemName(rawRow.CodeSystemOID,rawRow.CodeSystemName);
				string codeSystemOID=rawRow.CodeSystemOID!="" ? rawRow.CodeSystemOID : GetCodeSystemOid(codeSystem);
				listMeasureIdsInFile.AddRange(rawRow.ListMeasureIds.Where(x => !listMeasureIdsInFile.Contains(x)));
				if(rawRow.ValueSetVersion!="" && !listVersions.Contains(rawRow.ValueSetVersion)) {
					listVersions.Add(rawRow.ValueSetVersion);
				}
				string key=rawRow.ValueSetOID+"\t"+codeSystem+"\t"+rawRow.CodeValue;
				if(dictEhrCodes.TryGetValue(key,out EhrCode ehrCodeExisting)) {
					file.CountRowsMerged++;
					dictMeasureIds[key].AddRange(rawRow.ListMeasureIds.Where(x => !dictMeasureIds[key].Contains(x)));
					if(ehrCodeExisting.QDMCategory=="" && rawRow.QDMCategory!="") {
						ehrCodeExisting.QDMCategory=rawRow.QDMCategory;
					}
					if(ehrCodeExisting.Description=="" && rawRow.Description!="") {
						ehrCodeExisting.Description=rawRow.Description;
					}
					continue;
				}
				EhrCode ehrCode=new EhrCode();
				ehrCode.ValueSetOID=rawRow.ValueSetOID;
				ehrCode.ValueSetName=rawRow.ValueSetName;
				ehrCode.QDMCategory=rawRow.QDMCategory;
				ehrCode.CodeValue=rawRow.CodeValue;
				ehrCode.Description=rawRow.Description;
				ehrCode.CodeSystem=codeSystem;
				ehrCode.CodeSystemOID=codeSystemOID;
				dictEhrCodes.Add(key,ehrCode);
				dictMeasureIds.Add(key,rawRow.ListMeasureIds.ToList());
			}
			foreach(KeyValuePair<string,EhrCode> pair in dictEhrCodes) {
				EhrCode ehrCode=pair.Value;
				dictManifest.TryGetValue(ehrCode.ValueSetOID,out EhrValueSetManifestItem manifestItem);
				//MeasureIds holds only the seven measures this program reports, as Open Dental's list held its nine ("69v15,165v15"): its one reader
				//(GetMeasureIdsForCode, which FormSnomeds shows) needs no more, and the supplemental data elements (Payer, race, ethnicity, sex) are
				//listed under every measure of the release, which would not fit the column. The version check below uses every measure the file
				//names (listMeasureIdsInFile). None of the seven: the manifest's measures, when it lists the value set.
				List<string> listMeasureIdsReported=dictMeasureIds[pair.Key].Where(x => setMeasureIdsReported.Contains(x)).ToList();
				ehrCode.MeasureIds=listMeasureIdsReported.Count>0 ? string.Join(",",SortMeasureIds(listMeasureIdsReported)) : (manifestItem?.MeasureIds??"");
				if(ehrCode.ValueSetName=="" && manifestItem!=null) {
					ehrCode.ValueSetName=manifestItem.ValueSetName;
				}
				if(ehrCode.QDMCategory=="" && manifestItem!=null) {
					ehrCode.QDMCategory=manifestItem.QDMCategory;
				}
				if(IsTooLong(ehrCode)) {
					file.CountRowsTooLong++;
					continue;
				}
				file.ListEhrCodes.Add(ehrCode);
			}
			AddTooLongWarning(file);
			//The value sets.
			HashSet<string> setOids=new HashSet<string>(file.ListEhrCodes.Select(x => x.ValueSetOID),StringComparer.Ordinal);
			file.ListManifestOidsFound=dictManifest.Keys.Where(x => setOids.Contains(x)).ToList();
			file.ListManifestOidsMissing=dictManifest.Keys.Where(x => !setOids.Contains(x)).ToList();
			file.CountOtherValueSets=setOids.Count(x => !dictManifest.ContainsKey(x));
			AddCodeSystemsWithoutTable(file,file.ListEhrCodes);
			if(listVersions.Count>0) {
				file.ListNotes.Add(Lans.g(LANS,"Value-set versions in the file")+": "+string.Join(", ",listVersions.OrderBy(x => x,StringComparer.Ordinal).Take(10))
					+(listVersions.Count>10 ? ", ..." : ""));
			}
			if(file.ListEhrCodes.Count==0) {
				file.ListRefusals.Add(Lans.g(LANS,"The file has no value-set codes to import."));
				return;
			}
			//The measure versions (section 7.2).
			CheckMeasureVersions(file,listMeasureIdsInFile);
			if(!file.AreVersionsChecked) {
				if(file.ListManifestOidsFound.Count==0) {
					file.ListRefusals.Add(Lans.g(LANS,"The file names no measures, and none of its value sets is one the measures of this program use."));
				}
				else {
					file.ListNotes.Add(Lans.g(LANS,"The file names no measures, so the measure versions could not be checked. The measures of each value set "
						+"come from this program's list."));
				}
			}
			if(file.ListManifestOidsMissing.Count>0 && !file.IsRefused) {
				file.ListWarnings.Add(file.ListManifestOidsMissing.Count+" "+Lans.g(LANS,"of the")+" "+dictManifest.Count+" "
					+Lans.g(LANS,"value sets the measures use are not in the file."));
			}
		}

		///<summary>The Direct Reference Codes file's rows, one per measure, code system and code (a code listed under several measures is a row
		///under each, since each measure's roles are its own), with the measure-version check (section 7.3). The roles are resolved when the three
		///files are picked together (MakePick).</summary>
		private static void FinishDirectCodes(EhrValueSetFile file,List<RawRow> listRawRows) {
			Dictionary<string,EhrDirectCode> dictDirectCodes=new Dictionary<string,EhrDirectCode>(StringComparer.Ordinal);
			List<string> listMeasureIdsInFile=new List<string>();
			int countWithoutMeasure=0;
			foreach(RawRow rawRow in listRawRows) {
				if(rawRow.ListMeasureIds.Count==0) {
					countWithoutMeasure++;
					file.CountRowsSkipped++;
					continue;
				}
				string codeSystem=GetCodeSystemName(rawRow.CodeSystemOID,rawRow.CodeSystemName);
				string codeSystemOID=rawRow.CodeSystemOID!="" ? rawRow.CodeSystemOID : GetCodeSystemOid(codeSystem);
				foreach(string measureId in rawRow.ListMeasureIds) {
					if(!listMeasureIdsInFile.Contains(measureId)) {
						listMeasureIdsInFile.Add(measureId);
					}
					string cmsId="CMS"+GetMeasureNumber(measureId).ToString(CultureInfo.InvariantCulture);
					string key=measureId+"\t"+codeSystem+"\t"+rawRow.CodeValue;
					if(dictDirectCodes.TryGetValue(key,out EhrDirectCode directCodeExisting)) {
						file.CountRowsMerged++;
						if(directCodeExisting.Description=="" && rawRow.Description!="") {
							directCodeExisting.Description=rawRow.Description;
						}
						continue;
					}
					EhrDirectCode directCode=new EhrDirectCode();
					directCode.CmsId=cmsId;
					directCode.MeasureId=measureId;
					directCode.CodeValue=rawRow.CodeValue;
					directCode.Description=rawRow.Description;
					directCode.CodeSystem=codeSystem;
					directCode.CodeSystemOID=codeSystemOID;
					directCode.CodeSystemVersion=rawRow.CodeSystemVersion;
					dictDirectCodes.Add(key,directCode);
				}
			}
			foreach(EhrDirectCode directCode in dictDirectCodes.Values) {
				if(new[] { directCode.CodeValue,directCode.CodeSystem,directCode.CodeSystemOID }.Any(x => x.Length>MAX_COLUMN_LENGTH)
					|| Encoding.UTF8.GetByteCount(directCode.Description)>MAX_TEXT_BYTES)
				{
					file.CountRowsTooLong++;
					continue;
				}
				file.ListDirectCodes.Add(directCode);
			}
			AddTooLongWarning(file);
			if(countWithoutMeasure>0) {
				file.ListWarnings.Add(countWithoutMeasure+" "+Lans.g(LANS,"rows of the Direct Reference Codes file name no measure and are not imported."));
			}
			if(file.ListDirectCodes.Count==0) {
				file.ListRefusals.Add(Lans.g(LANS,"The Direct Reference Codes file has no direct reference codes to import."));
				return;
			}
			CheckMeasureVersions(file,listMeasureIdsInFile);
		}

		///<summary>The Code System Versions file's rows: one per code system and version (section 7.4).</summary>
		private static void FinishCodeSystemVersions(EhrValueSetFile file,List<RawRow> listRawRows) {
			foreach(RawRow rawRow in listRawRows) {
				string codeSystem=GetCodeSystemName(rawRow.CodeSystemOID,rawRow.CodeSystemName);
				string codeSystemOID=rawRow.CodeSystemOID!="" ? rawRow.CodeSystemOID : GetCodeSystemOid(codeSystem);
				if(file.ListCodeSystemVersions.Any(x => x.CodeSystem==codeSystem && x.Version==rawRow.CodeSystemVersion)) {
					file.CountRowsMerged++;
					continue;
				}
				if(codeSystem.Length>MAX_COLUMN_LENGTH || rawRow.CodeSystemVersion.Length>MAX_COLUMN_LENGTH) {
					file.CountRowsTooLong++;
					continue;
				}
				file.ListCodeSystemVersions.Add(new EhrCodeSystemVersion { CodeSystem=codeSystem,CodeSystemOID=codeSystemOID,Version=rawRow.CodeSystemVersion });
			}
			AddTooLongWarning(file);
			if(file.ListCodeSystemVersions.Count==0) {
				file.ListRefusals.Add(Lans.g(LANS,"The Code System Versions file has no code-system versions."));
			}
		}

		///<summary>True when a value of the row is longer than its column (ehrcode's varchar columns 255 characters, the description 65,535 bytes of
		///UTF-8).</summary>
		private static bool IsTooLong(EhrCode ehrCode) {
			return new[] { ehrCode.ValueSetName,ehrCode.ValueSetOID,ehrCode.QDMCategory,ehrCode.CodeValue,ehrCode.CodeSystem,ehrCode.CodeSystemOID }
				.Any(x => (x??"").Length>MAX_COLUMN_LENGTH)
				|| Encoding.UTF8.GetByteCount(ehrCode.Description??"")>MAX_TEXT_BYTES;
		}

		private static void AddTooLongWarning(EhrValueSetFile file) {
			if(file.CountRowsTooLong>0) {
				file.ListWarnings.Add(file.CountRowsTooLong+" "+Lans.g(LANS,"rows have a value longer than 255 characters (a description: longer than")+" "
					+MAX_TEXT_BYTES.ToString("N0",CultureInfo.InvariantCulture)+" "+Lans.g(LANS,"bytes in UTF-8) and are not imported."));
			}
		}

		///<summary>Names the code systems of the rows that this program has no table for (for example ICD10PCS), with their code counts.</summary>
		private static void AddCodeSystemsWithoutTable(EhrValueSetFile file,List<EhrCode> listEhrCodes) {
			foreach(IGrouping<string,EhrCode> grouping in listEhrCodes.Where(x => !EhrCodes.ArrayCodeSystems.Contains(x.CodeSystem))
				.GroupBy(x => x.CodeSystem).OrderBy(x => x.Key,StringComparer.Ordinal))
			{
				file.ListCodeSystemsWithoutTable.Add((grouping.Key=="" ? Lans.g(LANS,"(no code system)") : grouping.Key)+": "+grouping.Count());
			}
		}

		///<summary>The measure-version check of a file that names measures (section 7.2): each of the catalog's seven measures missing from the
		///file, or at another version (for example the 2026 release's CMS74v15), gives a warning that names it; a file with none of them at its 2027
		///version is refused, naming the release this program needs. A file that names no measure is not checked here (AreVersionsChecked false).</summary>
		private static void CheckMeasureVersions(EhrValueSetFile file,List<string> listMeasureIdsInFile) {
			file.ListMeasureIdsInFile=SortMeasureIds(listMeasureIdsInFile);
			file.AreVersionsChecked=file.ListMeasureIdsInFile.Count>0;
			if(!file.AreVersionsChecked) {
				return;
			}
			foreach(string measureIdExpected in ArrayMeasureIds) {
				string measureId=measureIdExpected.Substring(3);//"68v16"
				if(file.ListMeasureIdsInFile.Contains(measureId)) {
					file.ListMeasuresMatched.Add(measureIdExpected);
					continue;
				}
				int number=GetMeasureNumber(measureId);
				List<string> listOtherVersions=file.ListMeasureIdsInFile.Where(x => GetMeasureNumber(x)==number).Select(x => "CMS"+x).ToList();
				if(listOtherVersions.Count>0) {
					file.ListWarnings.Add(Lans.g(LANS,"The file has")+" "+string.Join(", ",listOtherVersions)+" "+Lans.g(LANS,"where this program reports")+" "
						+measureIdExpected+".");
				}
				else {
					file.ListWarnings.Add(measureIdExpected+" "+Lans.g(LANS,"is not in the file."));
				}
			}
			if(file.ListMeasuresMatched.Count==0) {
				file.ListRefusals.Add(Lans.g(LANS,"None of the seven measures this program reports is in the file at its 2027 version")+" ("
					+string.Join(", ",ArrayMeasureIds)+"). "+Lans.g(LANS,"The file names")+": "
					+string.Join(", ",file.ListMeasureIdsInFile.Take(20).Select(x => "CMS"+x))+(file.ListMeasureIdsInFile.Count>20 ? ", ..." : "")+". "
					+Lans.g(LANS,"This program needs the release for the 2027 performance period:")+" "+RELEASE_LABEL+".");
			}
		}
		#endregion Parsing

		#region Report and import
		///<summary>The text the Code System Importer shows for one file before an import (and for a refused file).</summary>
		public static string GetReport(EhrValueSetFile file) {
			Meth.NoCheckMiddleTierRole();
			Dictionary<string,EhrValueSetManifestItem> dictManifest=GetManifest().ToDictionary(x => x.ValueSetOID,x => x);
			StringBuilder stringBuilder=new StringBuilder();
			stringBuilder.AppendLine(Lans.g(LANS,"File")+": "+file.FileName+" ("+file.CountBytes.ToString("N0",CultureInfo.InvariantCulture)+" "+Lans.g(LANS,"bytes")+")");
			stringBuilder.AppendLine("SHA-256: "+file.Sha256);
			if(!file.IsRefused || file.Kind!=EhrValueSetFileKind.Unknown) {
				stringBuilder.AppendLine(Lans.g(LANS,"Holds")+": "+GetKindText(file.Kind));
			}
			if(file.IsRefused) {
				stringBuilder.AppendLine();
				stringBuilder.AppendLine(Lans.g(LANS,"The file is refused. Nothing was imported, and the value sets in the database are unchanged."));
				foreach(string refusal in file.ListRefusals) {
					stringBuilder.AppendLine("- "+refusal);
				}
			}
			stringBuilder.AppendLine();
			stringBuilder.AppendLine(Lans.g(LANS,"Read")+": "+string.Join("; ",file.ListPartsRead));
			foreach(string skipped in file.ListPartsSkipped) {
				stringBuilder.AppendLine(Lans.g(LANS,"Not read")+": "+skipped);
			}
			if(file.Kind==EhrValueSetFileKind.DirectReferenceCodes) {
				stringBuilder.AppendLine(Lans.g(LANS,"Rows read")+": "+file.CountRowsRead+"; "+Lans.g(LANS,"without a code or a measure (skipped)")+": "
					+file.CountRowsSkipped+"; "+Lans.g(LANS,"repeated (merged)")+": "+file.CountRowsMerged);
				stringBuilder.AppendLine(Lans.g(LANS,"Direct reference codes to import")+": "+file.ListDirectCodes.Count+" "+Lans.g(LANS,"of")+" "
					+file.ListDirectCodes.Select(x => x.MeasureId).Distinct().Count()+" "+Lans.g(LANS,"measures"));
			}
			else if(file.Kind==EhrValueSetFileKind.CodeSystemVersions) {
				stringBuilder.AppendLine(Lans.g(LANS,"Rows read")+": "+file.CountRowsRead+"; "+Lans.g(LANS,"without a code system or a version (skipped)")+": "
					+file.CountRowsSkipped);
				stringBuilder.AppendLine(Lans.g(LANS,"Code-system versions")+": "
					+string.Join(", ",file.ListCodeSystemVersions.Select(x => x.CodeSystem+" "+x.Version)));
			}
			else {
				stringBuilder.AppendLine(Lans.g(LANS,"Rows read")+": "+file.CountRowsRead+"; "+Lans.g(LANS,"without a value-set OID or a code (skipped)")+": "
					+file.CountRowsSkipped+"; "+Lans.g(LANS,"repeated under another measure (merged)")+": "+file.CountRowsMerged);
				stringBuilder.AppendLine(Lans.g(LANS,"Codes to import")+": "+file.ListEhrCodes.Count+" "+Lans.g(LANS,"in")+" "+file.CountValueSets+" "
					+Lans.g(LANS,"value sets"));
				stringBuilder.AppendLine(Lans.g(LANS,"Value sets the measures use, found")+": "+file.ListManifestOidsFound.Count+" "+Lans.g(LANS,"of")+" "
					+dictManifest.Count+"; "+Lans.g(LANS,"other value sets of the release")+": "+file.CountOtherValueSets);
			}
			if(file.AreVersionsChecked) {
				stringBuilder.AppendLine(Lans.g(LANS,"Measures")+": "+GetMeasureCheckText(file)
					+(file.ListMeasuresMatched.Count>0 ? " ("+string.Join(", ",file.ListMeasuresMatched)+")" : ""));
			}
			if(file.ListCodeSystemsWithoutTable.Count>0) {
				stringBuilder.AppendLine(Lans.g(LANS,"Code systems this program has no table for (imported, never counted as in the database)")+": "
					+string.Join(", ",file.ListCodeSystemsWithoutTable));
			}
			foreach(string note in file.ListNotes) {
				stringBuilder.AppendLine(note);
			}
			if(file.ListWarnings.Count>0) {
				stringBuilder.AppendLine();
				stringBuilder.AppendLine(Lans.g(LANS,"Warnings")+":");
				foreach(string warning in file.ListWarnings) {
					stringBuilder.AppendLine("- "+warning);
				}
			}
			if(file.ListManifestOidsMissing.Count>0 && !file.IsRefused) {
				stringBuilder.AppendLine();
				stringBuilder.AppendLine(Lans.g(LANS,"Value sets the measures use that are not in the file")+":");
				foreach(string oid in file.ListManifestOidsMissing) {
					stringBuilder.AppendLine("- "+oid+" "+dictManifest[oid].ValueSetName+" ("+dictManifest[oid].MeasureIds+")");
				}
			}
			return stringBuilder.ToString().TrimEnd();
		}

		///<summary>The office's pick of the release's three files, as Parse read each (section 7.1): checks that it holds exactly the three (the
		///value sets, the Direct Reference Codes file and the Code System Versions file), each once and none refused, then turns the direct reference
		///codes into ehrcode rows under their drc: keys and resolves the catalog's roles. A pick without all three, or with a refused file, is
		///refused: nothing is written. Nothing is written here either way.</summary>
		public static EhrValueSetPick MakePick(List<EhrValueSetFile> listFiles) {
			Meth.NoCheckMiddleTierRole();
			EhrValueSetPick pick=new EhrValueSetPick();
			pick.ListFiles=(listFiles??new List<EhrValueSetFile>()).Where(x => x!=null).ToList();
			foreach(EhrValueSetFile file in pick.ListFiles.Where(x => x.IsRefused)) {
				pick.ListRefusals.Add(file.FileName+": "+string.Join(" ",file.ListRefusals));
			}
			EhrValueSetFileKind[] arrayKinds=new[] { EhrValueSetFileKind.ValueSets,EhrValueSetFileKind.DirectReferenceCodes,EhrValueSetFileKind.CodeSystemVersions };
			foreach(EhrValueSetFileKind kind in arrayKinds) {
				List<EhrValueSetFile> listOfKind=pick.ListFiles.Where(x => !x.IsRefused && x.Kind==kind).ToList();
				if(listOfKind.Count==0) {
					pick.ListRefusals.Add(Lans.g(LANS,"The pick has no file with")+" "+GetKindText(kind)+".");
				}
				else if(listOfKind.Count>1) {
					pick.ListRefusals.Add(Lans.g(LANS,"The pick has more than one file with")+" "+GetKindText(kind)+": "
						+string.Join(", ",listOfKind.Select(x => x.FileName))+".");
				}
			}
			if(pick.ListFiles.Count!=3 || pick.ListRefusals.Count>0) {
				pick.ListRefusals.Insert(0,Lans.g(LANS,"Pick the three files of the release together: the value sets (the Excel workbook, the SVS XML or a "
					+".zip of either), the Direct Reference Codes file and the Code System Versions file. Files picked")+": "+pick.ListFiles.Count+".");
				return pick;
			}
			pick.FileValueSets=pick.ListFiles.Single(x => x.Kind==EhrValueSetFileKind.ValueSets);
			pick.FileDirectCodes=pick.ListFiles.Single(x => x.Kind==EhrValueSetFileKind.DirectReferenceCodes);
			pick.FileCodeSystemVersions=pick.ListFiles.Single(x => x.Kind==EhrValueSetFileKind.CodeSystemVersions);
			ResolveDirectCodes(pick);
			return pick;
		}

		///<summary>The direct reference codes as ehrcode rows (section 7.3): ValueSetOID the fork key drc:[CMS ID]:[role], QDMCategory "Direct
		///Reference Code", MeasureIds the measure when it is one of the seven at its 2027 version. Each role of the catalog takes the codes of its
		///measure (at the catalog's version) in its code system, and for a LOINC role the one whose description is its display name (E-23). A role
		///that takes one code and matches none or several, or a role of several codes that matches another number, gives a warning naming it; a
		///role of one code that does not match exactly one is not resolved (its criterion is then "not calculated"), and its codes stay listed.
		///Every other code is listed under drc:[CMS ID]:Listed.</summary>
		private static void ResolveDirectCodes(EhrValueSetPick pick) {
			Dictionary<string,CqmMeasure> dictMeasures=CqmCatalog.GetMeasures().ToDictionary(x => x.CmsId,x => x);
			Dictionary<EhrDirectCode,string> dictRoles=new Dictionary<EhrDirectCode,string>();
			foreach(CqmDirectCodeRole role in CqmCatalog.GetDirectCodeRoles()) {
				string measureId=dictMeasures[role.CmsId].MeasureIdShort;
				List<EhrDirectCode> listMatches=pick.FileDirectCodes.ListDirectCodes
					.Where(x => x.MeasureId==measureId && x.CodeSystem==role.CodeSystem)
					.Where(x => role.DisplayName=="" || NormalizeDisplayName(x.Description)==NormalizeDisplayName(role.DisplayName))
					.ToList();
				EhrDirectCodeRoleResult result=new EhrDirectCodeRoleResult();
				result.Role=role.Role;
				result.CmsId=role.CmsId;
				result.CountMatched=listMatches.Count;
				result.CountExpected=role.CountExpected;
				result.IsResolved=role.CountExpected==1 ? listMatches.Count==1 : listMatches.Count>0;
				pick.ListRoles.Add(result);
				if(listMatches.Count!=role.CountExpected) {
					pick.ListWarnings.Add(Lans.g(LANS,"The direct-reference-code role")+" "+role.Role+" ("+dictMeasures[role.CmsId].MeasureId+", "+role.CodeSystem+") "
						+Lans.g(LANS,"matches")+" "+listMatches.Count+" "+Lans.g(LANS,"codes of the file where it takes")+" "+role.CountExpected+"."
						+(result.IsResolved ? "" : " "+Lans.g(LANS,"Its criterion is not calculated until the role resolves.")));
				}
				if(!result.IsResolved) {
					continue;
				}
				foreach(EhrDirectCode directCode in listMatches) {
					dictRoles[directCode]=role.Role;
				}
			}
			HashSet<string> setMeasureIdsReported=new HashSet<string>(ArrayMeasureIds.Select(x => x.Substring(3)),StringComparer.Ordinal);
			foreach(EhrDirectCode directCode in pick.FileDirectCodes.ListDirectCodes) {
				dictRoles.TryGetValue(directCode,out string role);
				EhrCode ehrCode=new EhrCode();
				ehrCode.ValueSetOID=CqmCatalog.GetDirectCodeKey(directCode.CmsId,role);
				ehrCode.ValueSetName=Lans.g(LANS,"Direct reference code")+(role==null ? "" : ": "+role);
				ehrCode.QDMCategory=CqmCatalog.DIRECT_CODE_CATEGORY;
				ehrCode.MeasureIds=setMeasureIdsReported.Contains(directCode.MeasureId) ? directCode.MeasureId : "";
				ehrCode.CodeValue=directCode.CodeValue;
				ehrCode.Description=directCode.Description;
				ehrCode.CodeSystem=directCode.CodeSystem;
				ehrCode.CodeSystemOID=directCode.CodeSystemOID;
				pick.ListEhrCodesDirect.Add(ehrCode);
			}
		}

		///<summary>A display name compared without case and with white space collapsed.</summary>
		private static string NormalizeDisplayName(string displayName) {
			return Regex.Replace(displayName??"",@"\s+"," ").Trim().ToLowerInvariant();
		}

		///<summary>The text the Code System Importer shows for a pick of the three files before an import (and for a refused pick): the pick's own
		///refusals and warnings, the roles, then each file's report.</summary>
		public static string GetReport(EhrValueSetPick pick) {
			Meth.NoCheckMiddleTierRole();
			StringBuilder stringBuilder=new StringBuilder();
			stringBuilder.AppendLine(Lans.g(LANS,"Release")+": "+RELEASE_LABEL);
			if(pick.IsRefused) {
				stringBuilder.AppendLine();
				stringBuilder.AppendLine(Lans.g(LANS,"The pick is refused. Nothing was imported, and the value sets in the database are unchanged."));
				foreach(string refusal in pick.ListRefusals) {
					stringBuilder.AppendLine("- "+refusal);
				}
			}
			else {
				stringBuilder.AppendLine(Lans.g(LANS,"Codes to import")+": "+pick.ListEhrCodesAll.Count+" ("+pick.FileValueSets.ListEhrCodes.Count+" "
					+Lans.g(LANS,"value-set codes")+", "+pick.ListEhrCodesDirect.Count+" "+Lans.g(LANS,"direct reference codes")+")");
				stringBuilder.AppendLine(Lans.g(LANS,"Direct-reference-code roles resolved")+": "+pick.ListRoles.Count(x => x.IsResolved)+" "+Lans.g(LANS,"of")+" "
					+pick.ListRoles.Count);
				foreach(EhrDirectCodeRoleResult result in pick.ListRoles) {
					stringBuilder.AppendLine("- "+result.Role+" ("+result.CmsId+"): "+result.CountMatched+" "+Lans.g(LANS,"of")+" "+result.CountExpected+" "
						+Lans.g(LANS,"codes")+(result.IsResolved ? "" : ", "+Lans.g(LANS,"not resolved")));
				}
			}
			if(pick.ListWarnings.Count>0) {
				stringBuilder.AppendLine();
				stringBuilder.AppendLine(Lans.g(LANS,"Warnings")+":");
				foreach(string warning in pick.ListWarnings) {
					stringBuilder.AppendLine("- "+warning);
				}
			}
			foreach(EhrValueSetFile file in pick.ListFiles) {
				stringBuilder.AppendLine();
				stringBuilder.AppendLine("----------");
				stringBuilder.AppendLine(GetReport(file));
			}
			return stringBuilder.ToString().TrimEnd();
		}

		///<summary>Replaces every row of ehrcode with the pick's rows, the value-set codes and the direct reference codes together in one swap, and
		///records the release (ForkEhrValueSetRelease: the three files, the code-system versions and the roles) and a Setup security log entry
		///(section 7.1). Returns "" on success, otherwise the reason, and then the release is not recorded. Needs the Setup permission (checked here,
		///without a message, as well as by the window) and a direct connection: it refuses under the Middle Tier, as the CPT import does. A refused
		///pick (not the three files, or a refused file) writes nothing. MyISAM has no transactions, so the rows are written into a scratch table of
		///the same definition (ehrcode_import_[run], CREATE TABLE ... LIKE), which one RENAME TABLE swaps with ehrcode; the old table is dropped
		///afterwards. A failure before the swap (the server stops, the disk is full) drops the scratch table and leaves every old row of both kinds.
		///Scratch tables an import left more than a day ago (the program was stopped half way) are dropped first. The caller signals
		///InvalidType.EhrCodes and InvalidType.Prefs.</summary>
		public static string Import(EhrValueSetPick pick) {
			if(RemotingClient.MiddleTierRole==MiddleTierRole.ClientMT) {
				return Lans.g(LANS,"Value sets cannot be imported when using the middle tier.");
			}
			string error=GetSetupError();
			if(error!="") {
				return error;
			}
			if(pick==null) {
				return Lans.g(LANS,"No file was read.");
			}
			if(pick.IsRefused) {
				return string.Join("\r\n",pick.ListRefusals);
			}
			EhrValueSetFile file=pick.FileValueSets;
			List<EhrCode> listEhrCodes=pick.ListEhrCodesAll.Select(x => x.Copy()).ToList();
			try {
				DropLeftoverTables();
			}
			catch(Exception) {
				//Not needed for this import: the next one tries again.
			}
			string run=DateTime.UtcNow.ToString(SCRATCH_DATE_FORMAT,CultureInfo.InvariantCulture)+"_"+Guid.NewGuid().ToString("N").Substring(0,6);
			string tableImport=SCRATCH_PREFIX_IMPORT+run;
			string tableOld=SCRATCH_PREFIX_OLD+run;
			bool isSwapped=false;
			try {
				Db.NonQ("CREATE TABLE `"+tableImport+"` LIKE ehrcode");
				InsertRows(tableImport,listEhrCodes);
				long countWritten=PIn.Long(Db.GetScalar("SELECT COUNT(*) FROM `"+tableImport+"`"));
				if(countWritten!=listEhrCodes.Count) {
					return Lans.g(LANS,"The import wrote")+" "+countWritten+" "+Lans.g(LANS,"of")+" "+listEhrCodes.Count+" "
						+Lans.g(LANS,"codes, so the value sets in the database are unchanged. Import the file again.");
				}
				Db.NonQ("RENAME TABLE ehrcode TO `"+tableOld+"`, `"+tableImport+"` TO ehrcode");//One statement: other workstations see the old rows or the new.
				isSwapped=true;
			}
			catch(Exception ex) {
				return Lans.g(LANS,"The import failed, and the value sets in the database are unchanged")+": "+ex.Message;
			}
			finally {
				if(!isSwapped) {
					DropTableQuietly(tableImport);
				}
			}
			DropTableQuietly(tableOld);
			EhrValueSetRelease release=new EhrValueSetRelease();
			release.Label=RELEASE_LABEL;
			release.FileName=file.FileName;
			release.Sha256=file.Sha256;
			release.DateImportedUtc=DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ",CultureInfo.InvariantCulture);
			release.CountCodes=listEhrCodes.Count;
			release.CountValueSets=file.CountValueSets;
			release.CountManifestFound=file.ListManifestOidsFound.Count;
			release.CountManifest=GetManifest().Count;
			release.MeasureCheck=GetMeasureCheckText(file);
			release.CountDirectCodes=pick.ListEhrCodesDirect.Count;
			foreach(EhrValueSetFile filePicked in new[] { pick.FileValueSets,pick.FileDirectCodes,pick.FileCodeSystemVersions }) {
				release.ListFiles.Add(new EhrValueSetReleaseFile { Kind=filePicked.Kind,FileName=filePicked.FileName,Sha256=filePicked.Sha256,
					CountBytes=filePicked.CountBytes });
			}
			release.ListCodeSystemVersions=pick.FileCodeSystemVersions.ListCodeSystemVersions.ToList();
			release.ListRoles=pick.ListRoles.ToList();
			PrefC.UpsertRaw(PREF_RELEASE,JsonConvert.SerializeObject(release));
			SecurityLogs.MakeLogEntry(EnumPermType.Setup,0,GetLogText(release));
			return "";
		}

		///<summary>The names of the import's scratch tables: the prefix, the UTC time the import started (yyyyMMddHHmmss), "_" and six hex digits.</summary>
		private const string SCRATCH_PREFIX_IMPORT="ehrcode_import_";
		private const string SCRATCH_PREFIX_OLD="ehrcode_old_";
		private const string SCRATCH_DATE_FORMAT="yyyyMMddHHmmss";

		///<summary>Writes the rows into the table (the import's scratch table), as many to a statement as the packet size allows, as
		///EhrCodeCrud.InsertMany does for ehrcode (with random keys when the database uses them). ActionAfterInsertBatch follows each statement.</summary>
		private static void InsertRows(string tableName,List<EhrCode> listEhrCodes) {
			bool isRandomKeys=PrefC.RandomKeys;
			string commandStart="INSERT INTO `"+tableName+"` ("+(isRandomKeys ? "EhrCodeNum," : "")
				+"MeasureIds,ValueSetName,ValueSetOID,QDMCategory,CodeValue,Description,CodeSystem,CodeSystemOID) VALUES ";
			StringBuilder stringBuilder=null;
			int countInCommand=0;
			int countWritten=0;
			foreach(EhrCode ehrCode in listEhrCodes) {
				if(isRandomKeys) {
					ehrCode.EhrCodeNum=ReplicationServers.GetKey(tableName,"EhrCodeNum");
				}
				string row="("+(isRandomKeys ? POut.Long(ehrCode.EhrCodeNum)+"," : "")
					+"'"+POut.String(ehrCode.MeasureIds)+"',"
					+"'"+POut.String(ehrCode.ValueSetName)+"',"
					+"'"+POut.String(ehrCode.ValueSetOID)+"',"
					+"'"+POut.String(ehrCode.QDMCategory)+"',"
					+"'"+POut.String(ehrCode.CodeValue)+"',"
					+"'"+POut.String(ehrCode.Description)+"',"
					+"'"+POut.String(ehrCode.CodeSystem)+"',"
					+"'"+POut.String(ehrCode.CodeSystemOID)+"')";
				if(stringBuilder!=null && stringBuilder.Length+row.Length+1>TableBase.MaxAllowedPacketCount) {
					Db.NonQ(stringBuilder.ToString());
					countWritten+=countInCommand;
					ActionAfterInsertBatch?.Invoke(countWritten);
					stringBuilder=null;
				}
				if(stringBuilder==null) {
					stringBuilder=new StringBuilder(commandStart);
					countInCommand=0;
				}
				else {
					stringBuilder.Append(",");
				}
				stringBuilder.Append(row);
				countInCommand++;
			}
			if(stringBuilder!=null) {
				Db.NonQ(stringBuilder.ToString());
				countWritten+=countInCommand;
				ActionAfterInsertBatch?.Invoke(countWritten);
			}
		}

		///<summary>Drops the scratch tables (ehrcode_import_... and ehrcode_old_..., by the names Import gives them) that an import started more than
		///a day ago left behind. A table of today may be another workstation's import, and a table of another name is not this program's: both stay.</summary>
		private static void DropLeftoverTables() {
			DataTable table=Db.GetTable(@"SELECT TABLE_NAME FROM information_schema.TABLES WHERE TABLE_SCHEMA=DATABASE() "
				+@"AND (TABLE_NAME LIKE 'ehrcode\_import\_%' OR TABLE_NAME LIKE 'ehrcode\_old\_%')");
			foreach(DataRow dataRow in table.Rows) {
				string tableName=dataRow[0].ToString();
				Match match=Regex.Match(tableName,"^(?:"+SCRATCH_PREFIX_IMPORT+"|"+SCRATCH_PREFIX_OLD+@")(\d{14})_[0-9a-f]{6}$");
				if(!match.Success || !DateTime.TryParseExact(match.Groups[1].Value,SCRATCH_DATE_FORMAT,CultureInfo.InvariantCulture,
					DateTimeStyles.AssumeUniversal|DateTimeStyles.AdjustToUniversal,out DateTime dateStartedUtc))
				{
					continue;
				}
				if(DateTime.UtcNow-dateStartedUtc<TimeSpan.FromDays(1)) {
					continue;
				}
				Db.NonQ("DROP TABLE IF EXISTS `"+tableName+"`");
			}
		}

		///<summary>Drops the table if it exists. A failure is ignored: DropLeftoverTables drops it a day later.</summary>
		private static void DropTableQuietly(string tableName) {
			try {
				Db.NonQ("DROP TABLE IF EXISTS `"+tableName+"`");
			}
			catch(Exception) {
			}
		}

		///<summary>"9 of 9 measures at the versions this program reports", or that the file named no measure.</summary>
		private static string GetMeasureCheckText(EhrValueSetFile file) {
			if(!file.AreVersionsChecked) {
				return "measure versions not checked: the file names no measures";
			}
			return file.ListMeasuresMatched.Count+" of "+ArrayMeasureIds.Length+" measures at the "+CqmCatalog.PERFORMANCE_YEAR+" versions";
		}

		///<summary>The Setup security log entry of an import: the release, the three files with their SHA-256, the counts and the roles. No code is
		///logged.</summary>
		public static string GetLogText(EhrValueSetRelease release) {
			Meth.NoCheckMiddleTierRole();
			string files=string.Join(", ",(release.ListFiles??new List<EhrValueSetReleaseFile>()).Select(x => x.Kind+" '"+x.FileName+"' SHA-256 "+x.Sha256));
			int countRolesResolved=(release.ListRoles??new List<EhrDirectCodeRoleResult>()).Count(x => x.IsResolved);
			return "EHR value sets imported (Code System Importer): "+release.Label+"; files "+files+"; "+release.CountCodes+" codes ("
				+release.CountDirectCodes+" direct reference codes) in "+release.CountValueSets+" value sets, "+release.CountManifestFound+" of the "
				+release.CountManifest+" value sets the measures use; "+release.MeasureCheck+"; "+countRolesResolved+" of "
				+(release.ListRoles??new List<EhrDirectCodeRoleResult>()).Count+" direct-reference-code roles resolved; "
				+(release.ListCodeSystemVersions??new List<EhrCodeSystemVersion>()).Count+" code-system versions recorded. Every earlier value-set code "
				+"and direct reference code was replaced.";
		}

		///<summary>Compares a loaded code-system table's version (codesystem.VersionCur) with the version the release names for it (section 7.4).
		///The two are written in different formats, so each code system has its own mapping to a comparable value: LOINC by its release number
		///(2.80), SNOMED CT, RxNorm and CVX by the date the version names (2025-09, 20250901), ICD-10-CM, CPT and HCPCS by the year. Returns a negative
		///number when the loaded version is older, 0 when they are the same, a positive one when it is newer, and null when there is no mapping for
		///the code system or either version does not follow it (the window then shows both strings without a flag). The release may name several
		///versions of one code system (separated by ", "): the newest counts. **Provisional** until E-20's record of the real file.</summary>
		public static int? CompareCodeSystemVersions(string codeSystem,string versionLoaded,string versionRelease) {
			Meth.NoCheckMiddleTierRole();
			string keyLoaded=GetComparableVersion(codeSystem,versionLoaded);
			if(keyLoaded==null) {
				return null;
			}
			List<string> listKeysRelease=(versionRelease??"").Split(new[] { ", " },StringSplitOptions.RemoveEmptyEntries)
				.Select(x => GetComparableVersion(codeSystem,x)).Where(x => x!=null).ToList();
			if(listKeysRelease.Count==0) {
				return null;
			}
			string keyRelease=listKeysRelease.OrderByDescending(x => x,StringComparer.Ordinal).First();
			return Math.Sign(string.CompareOrdinal(keyLoaded,keyRelease));
		}

		///<summary>The version as a string that compares in order (fixed width), or null when the code system has no mapping or the version does not
		///follow it.</summary>
		private static string GetComparableVersion(string codeSystem,string version) {
			if(string.IsNullOrWhiteSpace(version)) {
				return null;
			}
			Match match;
			switch(codeSystem) {
				case "LOINC":
					match=Regex.Match(version,@"(?<![0-9.])(\d{1,2})\.(\d{1,3})(?![0-9.])");
					if(!match.Success) {
						return null;
					}
					return int.Parse(match.Groups[1].Value,CultureInfo.InvariantCulture).ToString("D3",CultureInfo.InvariantCulture)+"."
						+int.Parse(match.Groups[2].Value,CultureInfo.InvariantCulture).ToString("D3",CultureInfo.InvariantCulture);
				case "SNOMEDCT":
				case "RXNORM":
				case "CVX":
					//The last date the version names: yyyy-MM-dd, yyyyMMdd, yyyy-MM or yyyyMM (a missing day is the first).
					MatchCollection matchCollection=Regex.Matches(version,@"(?<!\d)((?:19|20)\d{2})-?(0[1-9]|1[0-2])(?:-?(0[1-9]|[12]\d|3[01]))?(?!\d)");
					if(matchCollection.Count==0) {
						return null;
					}
					match=matchCollection[matchCollection.Count-1];
					return match.Groups[1].Value+match.Groups[2].Value+(match.Groups[3].Success ? match.Groups[3].Value : "01");
				case "ICD10CM":
				case "CPT":
				case "HCPCS":
					match=Regex.Match(version,@"(?<!\d)((?:19|20)\d{2})(?!\d)");
					if(!match.Success) {
						return null;
					}
					return match.Groups[1].Value;
				default:
					return null;
			}
		}

		///<summary>The last import (the ForkEhrValueSetRelease row), or null when there was none or the row cannot be read.</summary>
		public static EhrValueSetRelease GetRelease() {
			Meth.NoCheckMiddleTierRole();
			string json=PrefC.GetRawOrDefault(PREF_RELEASE,"");
			if(string.IsNullOrWhiteSpace(json)) {
				return null;
			}
			try {
				return JsonConvert.DeserializeObject<EhrValueSetRelease>(json);
			}
			catch(Exception) {
				return null;//Not written by this program; the window shows "not imported".
			}
		}

		///<summary>"" when the current user has the Setup permission, otherwise the standard reason. No message box.</summary>
		private static string GetSetupError() {
			string error="";
			if(Security.IsAuthorized(EnumPermType.Setup,DateTime.MinValue,actionNotAuthorized:x => error=x)) {
				return "";
			}
			if(error=="") {
				error=Lans.g("Security","Not authorized for")+"\r\n"+GroupPermissions.GetDesc(EnumPermType.Setup);
			}
			return error;
		}
		#endregion Report and import
	}
}
