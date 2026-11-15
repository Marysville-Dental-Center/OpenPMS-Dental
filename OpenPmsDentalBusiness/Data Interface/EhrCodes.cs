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
	///value-set codes of the 2014 eCQMs and the EHR pick lists.  Open Dental filled the list from its closed EHR.dll; this program fills the table
	///from the office's own import of VSAC's June 2013 EP release (EhrValueSetImport, below), which replaces every row.  Until the office imports
	///it the table is empty, and so is the list.</summary>
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
		///directly.</summary>
		public static List<EhrCodeSystemStatus> GetCodeSystemStatuses() {
			Meth.NoCheckMiddleTierRole();
			List<EhrCode> listEhrCodes=Listt;
			Dictionary<string,int> dictCountCodes=listEhrCodes.GroupBy(x => x.CodeSystem??"").ToDictionary(x => x.Key,x => x.Count());
			List<string> listCodeSystems=ArrayCodeSystems.ToList();
			listCodeSystems.AddRange(dictCountCodes.Keys.Where(x => !ArrayCodeSystems.Contains(x)).OrderBy(x => x,StringComparer.Ordinal));
			List<EhrCodeSystemStatus> listStatuses=new List<EhrCodeSystemStatus>();
			foreach(string codeSystem in listCodeSystems) {
				EhrCodeSystemStatus status=new EhrCodeSystemStatus();
				status.CodeSystem=codeSystem;
				status.CountMinimum=GetCodeCountMinimum(codeSystem);
				status.HasTable=status.CountMinimum>=0;
				status.CountInDb=status.HasTable ? GetCodeCountInDb(codeSystem) : -1;
				status.IsInDb=IsCodeSystemInDb(codeSystem,status.CountInDb);
				dictCountCodes.TryGetValue(codeSystem,out status.CountValueSetCodes);
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

		///<summary>The measures of every value set that holds the code (EhrCode.MeasureIds joined: the nine measures this program reports, for example
		///"138v2,147v2"). FormSnomeds shows it.</summary>
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
		///<summary>Rows of the imported value sets with this code system.</summary>
		public int CountValueSetCodes;
	}

	///<summary>A value set the engine uses: its OID as the source names it, with its name, its QDM category and the measures that use it (the
	///shipped manifest, docs/specs/ehr-rebuild-plan.md section 8.3). No code of any value set is in it.</summary>
	public class EhrValueSetManifestItem {
		public string ValueSetOID;
		public string ValueSetName;
		public string QDMCategory;
		///<summary>Comma delimited, as EhrCode.MeasureIds: for example 69v2,147v2.</summary>
		public string MeasureIds;
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
		///MeasureIds is not checked: it holds only the nine measures this program reports.</summary>
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

	///<summary>The import the Code System Importer records in the ForkEhrValueSetRelease preference row (JSON).</summary>
	public class EhrValueSetRelease {
		public string Label="";
		public string FileName="";
		public string Sha256="";
		///<summary>UTC, yyyy-MM-ddTHH:mm:ssZ.</summary>
		public string DateImportedUtc="";
		public int CountCodes;
		public int CountValueSets;
		public int CountManifestFound;
		public int CountManifest;
		///<summary>"9 of 9 measures at the versions this program reports", or that the versions could not be checked.</summary>
		public string MeasureCheck="";
	}

	///<summary>The office's import of VSAC's "June 2013 EP Only Release eCQM Value Sets" (publication date June 14, 2013, expansion profile "MU2 EP
	///Update 2013-06-14"; docs/specs/ehr-rebuild-plan.md section 8): the release whose value sets the nine 2014 eCQM versions of QualityMeasures use.
	///The office downloads it from VSAC under its own UMLS licence, as the Excel workbook (.xlsx) or the SVS XML (.xml), or a .zip of either, and
	///imports it in Setup, Chart, EHR, Code System Importer. Nothing of VSAC's content is shipped: the program carries only the manifest (the value
	///sets' OIDs, which its source names, with their names, QDM categories and measures) and these readers. Parse reads the whole file first and
	///checks it; Import then replaces every row of ehrcode and records the release. A file that is refused, or a parse that fails, leaves the old
	///rows.</summary>
	public class EhrValueSetImport {
		///<summary>The fork-owned preference row with the last import (EhrValueSetRelease as JSON). Read by string (no PrefName).</summary>
		public const string PREF_RELEASE="ForkEhrValueSetRelease";
		///<summary>The release this program's measures need (section 8.1).</summary>
		public const string RELEASE_LABEL="VSAC June 2013 EP Only Release eCQM Value Sets (MU2 EP Update 2013-06-14)";
		///<summary>Largest file the importer reads, and largest part inside a .zip or a workbook once uncompressed (counted as it is read). The
		///program is a 32-bit process, and the parts it holds (a workbook inside a .zip, the rows read) must fit beside the rest of it; VSAC's June
		///2013 EP package is far smaller.</summary>
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

		///<summary>The nine 2014 eCQM versions QualityMeasures reports (GetEMeasureNum and GetEMeasureVersion), as VSAC's files name them.</summary>
		public static readonly string[] ArrayMeasureIds=new string[] {
			"CMS68v3","CMS69v2","CMS74v3","CMS75v2","CMS127v2","CMS138v2","CMS147v2","CMS155v2","CMS165v2",
		};

		///<summary>Code systems by their HL7 OID: the OIDs the engine and the codesystem table already use (ConvertDatabases3, EhrCCD). Both HL7
		///gender OIDs (v3 AdministrativeGender and v2 table 0001) are the code list's AdministrativeSex; both ICD-9-CM OIDs (diagnoses and
		///procedures) are ICD9CM, as VSAC names them. A code system not in this list keeps the name the file gives it.</summary>
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
		};

		///<summary>The manifest: every value-set OID the engine's source names (QualityMeasures, DiseaseDefs and the EHR windows), with the value
		///set's name, its QDM category and the measures that use it (without "CMS", as EhrCode.MeasureIds writes them). The names are the ones the
		///source gives beside each OID, without "Grouping Value Set"; the categories follow how the engine uses each value set; the measures are the
		///measure sections of QualityMeasures.GetEhrCqmData and ClassifyPatients that name the OID (the Payer value set is a supplemental data element
		///of every measure). A file's own name, category and measures win; these fill in for a file that has none (an SVS file without Group
		///elements). docs/port-notes/EHR-2.md says how they were made and what the lead checks against CMS's June 2013 specifications.</summary>
		private static readonly string[][] _arrayManifest=new string[][] {
			new[] { "2.16.840.1.113883.3.464.1003.101.12.1001","Office Visit","Encounter","74v3,75v2,127v2,138v2,147v2,155v2,165v2" },
			new[] { "2.16.840.1.113883.3.464.1003.101.12.1008","Outpatient Consultation","Encounter","147v2,165v2" },
			new[] { "2.16.840.1.113883.3.464.1003.101.12.1012","Nursing Facility Visit","Encounter","147v2" },
			new[] { "2.16.840.1.113883.3.464.1003.101.12.1013","Discharge Services - Nursing Facility","Encounter","147v2" },
			new[] { "2.16.840.1.113883.3.464.1003.101.12.1014","Care Services in Long-Term Residential Facility","Encounter","147v2" },
			new[] { "2.16.840.1.113883.3.464.1003.101.12.1016","Home Healthcare Services","Encounter","127v2,147v2,155v2,165v2" },
			new[] { "2.16.840.1.113883.3.464.1003.101.12.1022","Preventive Care- Initial Office Visit, 0 to 17","Encounter","74v3,75v2,147v2,155v2" },
			new[] { "2.16.840.1.113883.3.464.1003.101.12.1023","Preventive Care Services-Initial Office Visit, 18 and Up","Encounter","74v3,75v2,127v2,138v2,147v2,165v2" },
			new[] { "2.16.840.1.113883.3.464.1003.101.12.1024","Preventive Care - Established Office Visit, 0 to 17","Encounter","74v3,75v2,147v2,155v2" },
			new[] { "2.16.840.1.113883.3.464.1003.101.12.1025","Preventive Care Services - Established Office Visit, 18 and Up","Encounter","74v3,75v2,127v2,138v2,147v2,165v2" },
			new[] { "2.16.840.1.113883.3.464.1003.101.12.1026","Preventive Care Services-Individual Counseling","Encounter","138v2,147v2,155v2" },
			new[] { "2.16.840.1.113883.3.464.1003.101.12.1027","Preventive Care Services - Group Counseling","Encounter","138v2,147v2,155v2" },
			new[] { "2.16.840.1.113883.3.464.1003.101.12.1030","Preventive Care Services - Other","Encounter","138v2,147v2" },
			new[] { "2.16.840.1.113883.3.464.1003.101.12.1048","Face-to-Face Interaction","Encounter","74v3,75v2,127v2,138v2,155v2,165v2" },
			new[] { "2.16.840.1.113883.3.464.1003.104.12.1011","Essential Hypertension","Condition/Diagnosis/Problem","165v2" },
			new[] { "2.16.840.1.113883.3.464.1003.109.12.1011","Vascular Access for Dialysis","Procedure","165v2" },
			new[] { "2.16.840.1.113883.3.464.1003.109.12.1012","Kidney Transplant","Procedure","165v2" },
			new[] { "2.16.840.1.113883.3.464.1003.109.12.1013","Dialysis Services","Procedure","165v2" },
			new[] { "2.16.840.1.113883.3.464.1003.109.12.1014","ESRD Monthly Outpatient Services","Encounter","165v2" },
			new[] { "2.16.840.1.113883.3.464.1003.109.12.1015","Other Services Related to Dialysis","Intervention","165v2" },
			new[] { "2.16.840.1.113883.3.464.1003.109.12.1016","Dialysis Education","Intervention","165v2" },
			new[] { "2.16.840.1.113883.3.464.1003.110.12.1027","Pneumococcal Vaccine","Medication","127v2" },
			new[] { "2.16.840.1.113883.3.464.1003.110.12.1028","History of Pneumococcal Vaccine","Condition/Diagnosis/Problem","127v2" },
			new[] { "2.16.840.1.113883.3.464.1003.110.12.1034","Pneumococcal Vaccine Administered","Procedure","127v2" },
			new[] { "2.16.840.1.113883.3.464.1003.118.12.1035","Counseling for Physical Activity","Intervention","155v2" },
			new[] { "2.16.840.1.113883.3.464.1003.121.12.1012","BMI percentile","Physical Exam","155v2" },
			new[] { "2.16.840.1.113883.3.464.1003.121.12.1014","Height","Physical Exam","155v2" },
			new[] { "2.16.840.1.113883.3.464.1003.121.12.1015","Weight","Physical Exam","155v2" },
			new[] { "2.16.840.1.113883.3.464.1003.125.12.1002","Fluoride Varnish Application for Children","Procedure","74v3" },
			new[] { "2.16.840.1.113883.3.464.1003.125.12.1003","Clinical Oral Evaluation","Encounter","74v3,75v2" },
			new[] { "2.16.840.1.113883.3.464.1003.125.12.1004","Dental Caries","Condition/Diagnosis/Problem","75v2" },
			new[] { "2.16.840.1.113883.3.464.1003.195.12.1003","Counseling for Nutrition","Intervention","155v2" },
			new[] { "2.16.840.1.113883.3.526.3.1002","Chronic Kidney Disease, Stage 5","Condition/Diagnosis/Problem","165v2" },
			new[] { "2.16.840.1.113883.3.526.3.1007","Medical Reason","Attribute","138v2,147v2" },
			new[] { "2.16.840.1.113883.3.526.3.1008","Patient Reason","Attribute","147v2" },
			new[] { "2.16.840.1.113883.3.526.3.1009","System Reason","Attribute","147v2" },
			new[] { "2.16.840.1.113883.3.526.3.1011","Occupational Therapy Evaluation","Encounter","138v2" },
			new[] { "2.16.840.1.113883.3.526.3.1012","Patient Provider Interaction","Encounter","147v2" },
			new[] { "2.16.840.1.113883.3.526.3.1020","Health & Behavioral Assessment - Individual","Encounter","138v2" },
			new[] { "2.16.840.1.113883.3.526.3.1032","Systolic Blood Pressure","Physical Exam","165v2" },
			new[] { "2.16.840.1.113883.3.526.3.1033","Diastolic Blood Pressure","Physical Exam","165v2" },
			new[] { "2.16.840.1.113883.3.526.3.1083","Hemodialysis","Procedure","147v2" },
			new[] { "2.16.840.1.113883.3.526.3.1084","Peritoneal Dialysis","Procedure","147v2" },
			new[] { "2.16.840.1.113883.3.526.3.1141","Psychoanalysis","Encounter","138v2" },
			new[] { "2.16.840.1.113883.3.526.3.1170","Tobacco User","Individual Characteristic","138v2" },
			new[] { "2.16.840.1.113883.3.526.3.1185","Previous Receipt of Influenza Vaccine","Procedure","147v2" },
			new[] { "2.16.840.1.113883.3.526.3.1189","Tobacco Non-User","Individual Characteristic","138v2" },
			new[] { "2.16.840.1.113883.3.526.3.1190","Tobacco Use Cessation Pharmacotherapy","Medication","138v2" },
			new[] { "2.16.840.1.113883.3.526.3.1240","Annual Wellness Visit","Encounter","127v2,138v2,147v2,165v2" },
			new[] { "2.16.840.1.113883.3.526.3.1245","Health and Behavioral Assessment - Initial","Encounter","138v2" },
			new[] { "2.16.840.1.113883.3.526.3.1252","Encounter-Influenza","Encounter","147v2" },
			new[] { "2.16.840.1.113883.3.526.3.1253","Allergy to eggs","Condition/Diagnosis/Problem","147v2" },
			new[] { "2.16.840.1.113883.3.526.3.1254","Influenza Vaccine","Medication","147v2" },
			new[] { "2.16.840.1.113883.3.526.3.1255","Influenza Vaccination Declined","Attribute","147v2" },
			new[] { "2.16.840.1.113883.3.526.3.1256","Allergy to Influenza Vaccine","Condition/Diagnosis/Problem","147v2" },
			new[] { "2.16.840.1.113883.3.526.3.1257","Intolerance to Influenza Vaccine","Condition/Diagnosis/Problem","147v2" },
			new[] { "2.16.840.1.113883.3.526.3.1259","Limited Life Expectancy","Condition/Diagnosis/Problem","138v2" },
			new[] { "2.16.840.1.113883.3.526.3.1278","Tobacco Use Screening","Risk Category/Assessment","138v2" },
			new[] { "2.16.840.1.113883.3.526.3.1285","Ophthalmological Services","Encounter","138v2" },
			new[] { "2.16.840.1.113883.3.526.3.1492","Psych Visit - Diagnostic Evaluation","Encounter","138v2" },
			new[] { "2.16.840.1.113883.3.526.3.1496","Psych Visit - Psychotherapy","Encounter","138v2" },
			new[] { "2.16.840.1.113883.3.526.3.353","End Stage Renal Disease","Condition/Diagnosis/Problem","165v2" },
			new[] { "2.16.840.1.113883.3.526.3.378","Pregnancy","Condition/Diagnosis/Problem","155v2,165v2" },
			new[] { "2.16.840.1.113883.3.526.3.402","Influenza Vaccination","Procedure","147v2" },
			new[] { "2.16.840.1.113883.3.526.3.509","Tobacco Use Cessation Counseling","Intervention","138v2" },
			new[] { "2.16.840.1.113883.3.600.1.1498","Above Normal Medications","Medication","69v2" },
			new[] { "2.16.840.1.113883.3.600.1.1499","Below Normal Medications","Medication","69v2" },
			new[] { "2.16.840.1.113883.3.600.1.1502","Medical or Other reason not done","Attribute","68v3,69v2" },
			new[] { "2.16.840.1.113883.3.600.1.1503","Patient Reason Refused","Attribute","69v2" },
			new[] { "2.16.840.1.113883.3.600.1.1525","Above Normal Follow-up","Intervention","69v2" },
			new[] { "2.16.840.1.113883.3.600.1.1527","Referrals where weight assessment may occur","Intervention","69v2" },
			new[] { "2.16.840.1.113883.3.600.1.1528","Below Normal Follow up","Intervention","69v2" },
			new[] { "2.16.840.1.113883.3.600.1.1579","Palliative Care","Procedure","69v2" },
			new[] { "2.16.840.1.113883.3.600.1.1623","Pregnancy Dx","Condition/Diagnosis/Problem","69v2" },
			new[] { "2.16.840.1.113883.3.600.1.1751","BMI Encounter Code Set","Encounter","69v2" },
			new[] { "2.16.840.1.113883.3.600.1.1834","Medications Encounter Code Set","Encounter","68v3" },
			new[] { "2.16.840.1.113883.3.600.1.462","Current Medications Documented SNMD","Procedure","68v3" },
			new[] { "2.16.840.1.113883.3.600.1.681","BMI LOINC Value","Physical Exam","69v2" },
			new[] { "2.16.840.1.114222.4.11.3591","Payer","Individual Characteristic","68v3,69v2,74v3,75v2,127v2,138v2,147v2,155v2,165v2" },
		};

		///<summary>Workbook headers by the field they fill, compared after removing white space and ignoring case (section 8.2). The names VSAC's
		///eCQM workbooks of 2013 are expected to use come first; docs/port-notes/EHR-2.md says which are confirmed from the real file.</summary>
		private static readonly Dictionary<string,string[]> _dictHeaders=new Dictionary<string,string[]> {
			{ nameof(EhrCode.MeasureIds),new[] { "cmsid","cmsids","measureid","measureids","emeasureid","cmsemeasureid","cmsemeasureids" } },
			{ nameof(EhrCode.ValueSetName),new[] { "valuesetname" } },
			{ nameof(EhrCode.ValueSetOID),new[] { "valuesetoid","oid" } },
			{ nameof(EhrCode.QDMCategory),new[] { "qdmcategory","category" } },
			{ nameof(EhrCode.CodeValue),new[] { "code","codevalue" } },
			{ nameof(EhrCode.Description),new[] { "descriptor","description","codedescription","displayname","codedisplayname" } },
			{ nameof(EhrCode.CodeSystem),new[] { "codesystem","codesystemname" } },
			{ nameof(EhrCode.CodeSystemOID),new[] { "codesystemoid" } },
			{ "ValueSetVersion",new[] { "definitionversion","valuesetversion","expansionversion","version" } },
		};

		///<summary>How many rows of a sheet are searched for the header row.</summary>
		private const int MAX_HEADER_ROW=30;

		#region Manifest and code systems
		///<summary>The manifest (a new list of new items each call).</summary>
		public static List<EhrValueSetManifestItem> GetManifest() {
			Meth.NoCheckMiddleTierRole();
			return _arrayManifest.Select(x => new EhrValueSetManifestItem { ValueSetOID=x[0],ValueSetName=x[1],QDMCategory=x[2],MeasureIds=x[3] }).ToList();
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
			string nameKnown=EhrCodes.ArrayCodeSystems.FirstOrDefault(x => string.Equals(x,nameCompact,StringComparison.OrdinalIgnoreCase));
			if(nameKnown!=null) {
				return nameKnown;
			}
			if(string.Equals(nameCompact,"AdministrativeGender",StringComparison.OrdinalIgnoreCase)) {
				return "AdministrativeSex";
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
			List<RawRow> listRawRows=new List<RawRow>();
			try {
				stream.Position=0;
				ReadPart(file,file.FileName,stream,listRawRows,isInsideZip:false,new ReadBudget());
			}
			catch(Exception ex) {
				//A damaged workbook, zip or XML, or a part past a limit: refused, and the old rows stay.
				file.ListRefusals.Add(GetReadErrorText(ex));
				return file;
			}
			if(file.IsRefused) {
				return file;
			}
			Finish(file,listRawRows);
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
		private static void ReadPart(EhrValueSetFile file,string partName,Stream stream,List<RawRow> listRawRows,bool isInsideZip,ReadBudget readBudget) {
			string extension=Path.GetExtension(partName).ToLowerInvariant();
			byte[] arrayStart=ReadStart(stream,67);
			stream.Position=0;
			if(IsZip(arrayStart)) {
				using(ZipArchive zipArchive=new ZipArchive(stream,ZipArchiveMode.Read,leaveOpen:true)) {
					if(zipArchive.Entries.Any(x => x.FullName=="xl/workbook.xml")) {
						ReadWorkbook(file,partName,zipArchive,listRawRows,readBudget);
						return;
					}
					if(isInsideZip) {
						file.ListPartsSkipped.Add(partName+": "+Lans.g(LANS,"a .zip inside the .zip is not read."));
						return;
					}
					ReadZip(file,partName,zipArchive,listRawRows,readBudget);
				}
				return;
			}
			if(extension==".xml" || LooksLikeXml(arrayStart)) {
				using(LimitedReadStream limitedReadStream=new LimitedReadStream(stream,partName,readBudget,isLeaveOpen:true)) {
					ReadSvs(file,partName,limitedReadStream,listRawRows);
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
		private static void ReadZip(EhrValueSetFile file,string partName,ZipArchive zipArchive,List<RawRow> listRawRows,ReadBudget readBudget) {
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
				ReadZipEntry(file,entryName,zipArchiveEntry,listRawRows,readBudget);
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
		private static void ReadZipEntry(EhrValueSetFile file,string entryName,ZipArchiveEntry zipArchiveEntry,List<RawRow> listRawRows,ReadBudget readBudget) {
			byte[] arrayStart;
			using(Stream stream=OpenEntry(zipArchiveEntry,entryName,new ReadBudget())) {//The first bytes, not counted twice.
				arrayStart=ReadStart(stream,67);
			}
			if(IsZip(arrayStart)) {
				using(MemoryStream memoryStream=CopyEntryToMemory(zipArchiveEntry,entryName,readBudget)) {
					ReadPart(file,entryName,memoryStream,listRawRows,isInsideZip:true,readBudget);
				}
				return;
			}
			if(Path.GetExtension(entryName).ToLowerInvariant()==".xml" || LooksLikeXml(arrayStart)) {
				using(Stream stream=OpenEntry(zipArchiveEntry,entryName,readBudget)) {
					ReadSvs(file,entryName,stream,listRawRows);
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
		private static void ReadWorkbook(EhrValueSetFile file,string partName,ZipArchive zipArchive,List<RawRow> listRawRows,ReadBudget readBudget) {
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
					if(ReadSheet(file,sheetLabel,ReadSheetRows(file,stream,listSharedStrings),listRawRows)) {
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

		///<summary>Reads the rows under the sheet's header row, as they are read from the sheet (each row goes to the header search, then to
		///AddRawRow, so MAX_ROWS refuses the file while it is read). Returns false (and names the sheet) when the sheet has no header row: none of its
		///first rows names at least two known columns, one of them Code or Value Set OID; the rest of such a sheet is not read. A header row without
		///the required columns refuses the file.</summary>
		private static bool ReadSheet(EhrValueSetFile file,string sheetLabel,IEnumerable<List<string>> enumerableRows,List<RawRow> listRawRows) {
			int indexRow=-1;
			int indexHeader=-1;
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
					if(dictColumnsRow.Count<2
						|| (!dictColumnsRow.ContainsKey(nameof(EhrCode.CodeValue)) && !dictColumnsRow.ContainsKey(nameof(EhrCode.ValueSetOID))))
					{
						continue;
					}
					indexHeader=indexRow;
					dictColumns=dictColumnsRow;
					List<string> listMissing=new List<string>();
					if(!dictColumns.ContainsKey(nameof(EhrCode.ValueSetOID))) {
						listMissing.Add("Value Set OID");
					}
					if(!dictColumns.ContainsKey(nameof(EhrCode.CodeValue))) {
						listMissing.Add("Code");
					}
					if(!dictColumns.ContainsKey(nameof(EhrCode.CodeSystem)) && !dictColumns.ContainsKey(nameof(EhrCode.CodeSystemOID))) {
						listMissing.Add("Code System (or Code System OID)");
					}
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
				rawRow.ValueSetVersion=GetCell(listRow,dictColumns,"ValueSetVersion");
				rawRow.QDMCategory=GetCell(listRow,dictColumns,nameof(EhrCode.QDMCategory));
				rawRow.ListMeasureIds=ParseMeasureIds(GetCell(listRow,dictColumns,nameof(EhrCode.MeasureIds)));
				rawRow.CodeValue=GetCell(listRow,dictColumns,nameof(EhrCode.CodeValue));
				rawRow.Description=GetCell(listRow,dictColumns,nameof(EhrCode.Description));
				rawRow.CodeSystemName=GetCell(listRow,dictColumns,nameof(EhrCode.CodeSystem));
				rawRow.CodeSystemOID=GetCell(listRow,dictColumns,nameof(EhrCode.CodeSystemOID));
				if(!AddRawRow(file,listRawRows,rawRow)) {
					return true;
				}
			}
			if(indexHeader<0) {
				file.ListPartsSkipped.Add(sheetLabel+": "+Lans.g(LANS,"no header row with the value-set columns."));
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
		private static void ReadSvs(EhrValueSetFile file,string partName,Stream stream,List<RawRow> listRawRows) {
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
								if(!AddRawRow(file,listRawRows,rawRow)) {
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

		///<summary>Adds a row read from the file, skipping and counting one without a value-set OID or a code. Returns false when the file passes
		///MAX_ROWS (the file is then refused).</summary>
		private static bool AddRawRow(EhrValueSetFile file,List<RawRow> listRawRows,RawRow rawRow) {
			file.CountRowsRead++;
			if(file.CountRowsRead>MAX_ROWS) {
				file.ListRefusals.Add(Lans.g(LANS,"The file has more rows than the importer reads")+" ("+MAX_ROWS+").");
				return false;
			}
			if(rawRow.ValueSetOID=="" || rawRow.CodeValue=="") {
				file.CountRowsSkipped++;
				return true;
			}
			listRawRows.Add(rawRow);
			return true;
		}

		///<summary>Merges the rows (one per value set, code system and code, with every measure that lists it), fills in from the manifest what the
		///file does not give, and makes the checks of section 8.2.</summary>
		private static void Finish(EhrValueSetFile file,List<RawRow> listRawRows) {
			Dictionary<string,EhrValueSetManifestItem> dictManifest=GetManifest().ToDictionary(x => x.ValueSetOID,x => x);
			HashSet<string> setMeasureIdsReported=new HashSet<string>(ArrayMeasureIds.Select(x => x.Substring(3)),StringComparer.Ordinal);//"68v3"
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
				//MeasureIds holds only the nine measures this program reports, as Open Dental's list did ("69v2,147v2"): its one reader
				//(GetMeasureIdsForCode, which FormSnomeds shows) needs no more, and the supplemental data elements (Payer, race, ethnicity, sex) are
				//listed under about 64 measures of the release, which would not fit the column. The version check below uses every measure the file
				//names (listMeasureIdsInFile). None of the nine: the manifest's measures, when it lists the value set.
				List<string> listMeasureIdsReported=dictMeasureIds[pair.Key].Where(x => setMeasureIdsReported.Contains(x)).ToList();
				ehrCode.MeasureIds=listMeasureIdsReported.Count>0 ? string.Join(",",SortMeasureIds(listMeasureIdsReported)) : (manifestItem?.MeasureIds??"");
				if(ehrCode.ValueSetName=="" && manifestItem!=null) {
					ehrCode.ValueSetName=manifestItem.ValueSetName;
				}
				if(ehrCode.QDMCategory=="" && manifestItem!=null) {
					ehrCode.QDMCategory=manifestItem.QDMCategory;
				}
				if(new[] { ehrCode.ValueSetName,ehrCode.ValueSetOID,ehrCode.QDMCategory,ehrCode.CodeValue,ehrCode.CodeSystem,ehrCode.CodeSystemOID }
					.Any(x => x.Length>MAX_COLUMN_LENGTH)
					|| Encoding.UTF8.GetByteCount(ehrCode.Description)>MAX_TEXT_BYTES)
				{
					file.CountRowsTooLong++;
					continue;
				}
				file.ListEhrCodes.Add(ehrCode);
			}
			if(file.CountRowsTooLong>0) {
				file.ListWarnings.Add(file.CountRowsTooLong+" "+Lans.g(LANS,"rows have a value longer than 255 characters (a description: longer than")+" "
					+MAX_TEXT_BYTES.ToString("N0",CultureInfo.InvariantCulture)+" "+Lans.g(LANS,"bytes in UTF-8) and are not imported."));
			}
			if(file.CountCellsIgnored>0) {
				file.ListNotes.Add(file.CountCellsIgnored+" "+Lans.g(LANS,"cells to the right of column IV (the 256th column) were not read."));
			}
			//The value sets.
			HashSet<string> setOids=new HashSet<string>(file.ListEhrCodes.Select(x => x.ValueSetOID),StringComparer.Ordinal);
			file.ListManifestOidsFound=dictManifest.Keys.Where(x => setOids.Contains(x)).ToList();
			file.ListManifestOidsMissing=dictManifest.Keys.Where(x => !setOids.Contains(x)).ToList();
			file.CountOtherValueSets=setOids.Count(x => !dictManifest.ContainsKey(x));
			foreach(IGrouping<string,EhrCode> grouping in file.ListEhrCodes.Where(x => !EhrCodes.ArrayCodeSystems.Contains(x.CodeSystem))
				.GroupBy(x => x.CodeSystem).OrderBy(x => x.Key,StringComparer.Ordinal))
			{
				file.ListCodeSystemsWithoutTable.Add((grouping.Key=="" ? Lans.g(LANS,"(no code system)") : grouping.Key)+": "+grouping.Count());
			}
			if(listVersions.Count>0) {
				file.ListNotes.Add(Lans.g(LANS,"Value-set versions in the file")+": "+string.Join(", ",listVersions.OrderBy(x => x,StringComparer.Ordinal).Take(10))
					+(listVersions.Count>10 ? ", ..." : ""));
			}
			if(file.ListEhrCodes.Count==0) {
				file.ListRefusals.Add(Lans.g(LANS,"The file has no value-set codes to import."));
				return;
			}
			//The measure versions (section 8.2).
			file.ListMeasureIdsInFile=SortMeasureIds(listMeasureIdsInFile);
			file.AreVersionsChecked=file.ListMeasureIdsInFile.Count>0;
			if(file.AreVersionsChecked) {
				foreach(string measureIdExpected in ArrayMeasureIds) {
					string measureId=measureIdExpected.Substring(3);//"68v3"
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
					file.ListRefusals.Add(Lans.g(LANS,"None of the nine measures this program reports is in the file at the version it reports")+" ("
						+string.Join(", ",ArrayMeasureIds)+"). "+Lans.g(LANS,"The file names")+": "
						+string.Join(", ",file.ListMeasureIdsInFile.Take(20).Select(x => "CMS"+x))+(file.ListMeasureIdsInFile.Count>20 ? ", ..." : "")+". "
						+Lans.g(LANS,"This program needs the value sets of the June 2013 EP release.")+" "+RELEASE_LABEL);
				}
			}
			else {
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
		#endregion Parsing

		#region Report and import
		///<summary>The text the Code System Importer shows before an import (and for a refused file).</summary>
		public static string GetReport(EhrValueSetFile file) {
			Meth.NoCheckMiddleTierRole();
			Dictionary<string,EhrValueSetManifestItem> dictManifest=GetManifest().ToDictionary(x => x.ValueSetOID,x => x);
			StringBuilder stringBuilder=new StringBuilder();
			stringBuilder.AppendLine(Lans.g(LANS,"File")+": "+file.FileName+" ("+file.CountBytes.ToString("N0",CultureInfo.InvariantCulture)+" "+Lans.g(LANS,"bytes")+")");
			stringBuilder.AppendLine("SHA-256: "+file.Sha256);
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
			stringBuilder.AppendLine(Lans.g(LANS,"Rows read")+": "+file.CountRowsRead+"; "+Lans.g(LANS,"without a value-set OID or a code (skipped)")+": "
				+file.CountRowsSkipped+"; "+Lans.g(LANS,"repeated under another measure (merged)")+": "+file.CountRowsMerged);
			stringBuilder.AppendLine(Lans.g(LANS,"Codes to import")+": "+file.ListEhrCodes.Count+" "+Lans.g(LANS,"in")+" "+file.CountValueSets+" "
				+Lans.g(LANS,"value sets"));
			stringBuilder.AppendLine(Lans.g(LANS,"Value sets the measures use, found")+": "+file.ListManifestOidsFound.Count+" "+Lans.g(LANS,"of")+" "
				+dictManifest.Count+"; "+Lans.g(LANS,"other value sets of the release")+": "+file.CountOtherValueSets);
			if(file.AreVersionsChecked) {
				stringBuilder.AppendLine(Lans.g(LANS,"Measures at the versions this program reports")+": "+file.ListMeasuresMatched.Count+" "+Lans.g(LANS,"of")+" "
					+ArrayMeasureIds.Length+(file.ListMeasuresMatched.Count>0 ? " ("+string.Join(", ",file.ListMeasuresMatched)+")" : ""));
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

		///<summary>Replaces every row of ehrcode with the file's rows and records the release (ForkEhrValueSetRelease) and a Setup security log entry.
		///Returns "" on success, otherwise the reason, and then the release is not recorded. Needs the Setup permission (checked here, without a
		///message, as well as by the window) and a direct connection: it refuses under the Middle Tier, as the CPT import does. MyISAM has no
		///transactions, so the rows are written into a scratch table of the same definition (ehrcode_import_[run], CREATE TABLE ... LIKE), which
		///one RENAME TABLE swaps with ehrcode; the old table is dropped afterwards. A failure before the swap (the server stops, the disk is full)
		///drops the scratch table and leaves every old row. Scratch tables an import left more than a day ago (the program was stopped half way)
		///are dropped first. The caller signals InvalidType.EhrCodes and InvalidType.Prefs.</summary>
		public static string Import(EhrValueSetFile file) {
			if(RemotingClient.MiddleTierRole==MiddleTierRole.ClientMT) {
				return Lans.g(LANS,"Value sets cannot be imported when using the middle tier.");
			}
			string error=GetSetupError();
			if(error!="") {
				return error;
			}
			if(file==null) {
				return Lans.g(LANS,"No file was read.");
			}
			if(file.IsRefused) {
				return string.Join("\r\n",file.ListRefusals);
			}
			List<EhrCode> listEhrCodes=file.ListEhrCodes.Select(x => x.Copy()).ToList();
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
			release.CountManifest=_arrayManifest.Length;
			release.MeasureCheck=GetMeasureCheckText(file);
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
			return file.ListMeasuresMatched.Count+" of "+ArrayMeasureIds.Length+" measures at the versions this program reports";
		}

		///<summary>The Setup security log entry of an import. No code is logged.</summary>
		public static string GetLogText(EhrValueSetRelease release) {
			Meth.NoCheckMiddleTierRole();
			return "EHR value sets imported (Code System Importer): file '"+release.FileName+"', SHA-256 "+release.Sha256+", "+release.CountCodes+" codes in "
				+release.CountValueSets+" value sets, "+release.CountManifestFound+" of the "+release.CountManifest+" value sets the measures use; "
				+release.MeasureCheck+". Every earlier value-set code was replaced.";
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
