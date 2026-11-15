using System;

namespace OpenPmsDentalBusiness {
	///<summary>For EHR module, these are all the codes from various code sets that will affect reporting clinical quality measures.  Users cannot edit.  A fork-only table (EhrTables in Misc\ForkMigrations\EhrSteps.cs, docs/specs/ehr-rebuild-plan.md section 7, EHR-2): Open Dental filled the list from its closed EHR.dll, this program from the office's own import of VSAC's 2026-05-14 release for the 2027 measures with its Direct Reference Codes and Code System Versions files (EhrValueSetImport, Setup, Chart, EHR, Code System Importer; docs/specs/ehr-2027-measures-plan.md, EHR-6a), which replaces every row.  A direct reference code's row has a drc: key as its ValueSetOID (CqmCatalog.GetDirectCodeKey).  No window edits a row.  Selecting from this table always uses the cache pattern (EhrCodes.Listt).</summary>
	[Serializable]
	[CrudTable(HasBatchWriteMethods=true)]
	public class EhrCode:TableBase {
		///<summary>Primary key.</summary>
		[CrudColumn(IsPriKey=true)]
		public long EhrCodeNum;
		///<summary>Clinical quality measure test ID's that utilize this code.  Comma delimited list.  Example: 69v15,165v15.</summary>
		public string MeasureIds;
		///<summary>The National Library of Medicine Value Set Authority Center assigned value set name.  Example: Influenza Vaccination.</summary>
		public string ValueSetName;
		///<summary>The value set object identifier for reporting CQM.  Example: 2.16.840.1.113883.3.526.3.402.</summary>
		public string ValueSetOID;
		///<summary>The Quality Data Model category for this code.  2 examples: Condition/Diagnosis/Problem or Encounter.</summary>
		public string QDMCategory;
		///<summary>The code from the specified code system.  Example: 653.83.  This code can belong to multiple value sets, in which case this table will contain multiple rows for this code.</summary>
		public string CodeValue;
		///<summary>The description for this code.  This will frequently be duplicate data, but keeping it here ensures accurate CQM reporting.  Text, because some descriptions in the value sets are longer than 255 characters.</summary>
		[CrudColumn(SpecialType=CrudSpecialColType.IsText)]
		public string Description;
		///<summary>The code system name for this code.  Possible values are: CDCREC, CDT, CPT, CVX, HCPCS, ICD9CM, ICD10CM, LOINC, RXNORM, SNOMEDCT, SOP, and AdministrativeSex.  A code system this program has no table for keeps the name the office's file gives it (for example ICD10PCS).</summary>
		public string CodeSystem;
		///<summary>The code system object identifier for reporting CQM.  Example: 2.16.840.1.113883.6.103.</summary>
		public string CodeSystemOID;
		///<summary>Not a database column.  True if the code system's table identified by CodeSystem has more rows than the code list expects (EhrCodes.updateCodeExistsHelper).  Set when the list is loaded.</summary>
		[CrudColumn(IsNotDbColumn=true)]
		public bool IsInDb;


		///<summary></summary>
		public EhrCode Copy() {
			return (EhrCode)MemberwiseClone();
		}

	}

}
