using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenPmsDentalBusiness {
	///<summary>The data-entry windows on the 2027 catalog (docs/specs/ehr-2027-measures-plan.md, section 8, package EHR-7): which imported value
	///sets and direct-reference-code roles each pick list reads, how a not-performed row or a medication row is labelled, and the recommended
	///default encounter and pregnancy codes (E-32). The value sets are named by the catalog's constants (CqmCatalog), the direct reference codes
	///by role, so no window and no method here names a value-set OID or a code. The codes come only from the office's own VSAC download
	///(EhrValueSetImport) and its code-system tables. Each lookup has a form that takes the list of value-set rows, so the rules are tested without
	///a database; the other form reads EhrCodes.Listt. Data only: every method is NoCheckMiddleTierRole and calls only methods that pass the
	///Middle Tier themselves (EhrCodes.Listt, Snomeds.GetByCodes, Cvxs.GetAll).</summary>
	public class CqmDataEntry {
		///<summary>The most codes one Snomeds.GetByCodes call is sent.</summary>
		private const int COUNT_CODES_PER_QUERY=500;

		#region The release
		///<summary>True when the office's last value-set import (EhrValueSetImport.GetRelease) is the 2027 release this program needs
		///(CqmCatalog.RELEASE_LABEL). False with no import, or with an earlier release (EHR-2's June 2013 import): then every pick list of the
		///catalog is empty and a saved not-performed row of no CVX code is "other" only because the value sets are missing, which the windows say
		///(FormEhrNotPerformed, FormEhrNotPerformedEdit).</summary>
		public static bool IsReleaseImported() {
			Meth.NoCheckMiddleTierRole();
			return IsReleaseImported(EhrValueSetImport.GetRelease());
		}

		///<summary>True when this record of an import is the 2027 release (IsReleaseImported).</summary>
		public static bool IsReleaseImported(EhrValueSetRelease release) {
			Meth.NoCheckMiddleTierRole();
			return release!=null && release.Label==CqmCatalog.RELEASE_LABEL;
		}
		#endregion The release

		#region Direct reference codes
		///<summary>The code of a direct-reference-code role (CqmCatalog.GetDirectCodeRoles) from the office's import, or null when the role matched no
		///row or more than one (the import's report warns of both; section 7.3). Read from EhrCodes.Listt.</summary>
		public static EhrCode GetDirectCode(string role) {
			Meth.NoCheckMiddleTierRole();
			return GetDirectCode(role,EhrCodes.Listt);
		}

		///<summary>The code of a role among these value-set rows, or null when the role is unknown or matched no row or more than one.</summary>
		public static EhrCode GetDirectCode(string role,List<EhrCode> listEhrCodes) {
			Meth.NoCheckMiddleTierRole();
			CqmDirectCodeRole directCodeRole=CqmCatalog.GetDirectCodeRole(role);
			if(directCodeRole==null || listEhrCodes==null) {
				return null;
			}
			string key=CqmCatalog.GetDirectCodeKey(directCodeRole.CmsId,directCodeRole.Role);
			List<EhrCode> listMatches=listEhrCodes.Where(x => x.ValueSetOID==key)
				.GroupBy(x => x.CodeSystem+"|"+x.CodeValue).Select(x => x.First()).ToList();
			if(listMatches.Count!=1) {
				return null;
			}
			return listMatches[0];
		}

		///<summary>True when the code is the role's code among these value-set rows.</summary>
		private static bool IsDirectCode(string role,string codeValue,string codeSystem,List<EhrCode> listEhrCodes) {
			EhrCode ehrCode=GetDirectCode(role,listEhrCodes);
			return ehrCode!=null && ehrCode.CodeValue==codeValue && ehrCode.CodeSystem==codeSystem;
		}
		#endregion Direct reference codes

		#region Not performed
		///<summary>The direct-reference-code role a not-performed item picks its code from, or "" for an item that picks from value sets or from the
		///CVX table: BMIExam the CMS69 BMI exam, DocumentCurrentMeds CMS68's current medications documented (section 8).</summary>
		public static string GetNotPerformedRole(EhrNotPerformedItem item) {
			Meth.NoCheckMiddleTierRole();
			switch(item) {
				case EhrNotPerformedItem.BMIExam:
					return CqmCatalog.ROLE_CMS69_BMI;
				case EhrNotPerformedItem.DocumentCurrentMeds:
					return CqmCatalog.ROLE_CMS68_MEDS_DOCUMENTED;
				default:
					return "";
			}
		}

		///<summary>The value sets a not-performed item picks its code from: TobaccoScreening "Tobacco Use Screening" (CMS138v15 has no exceptions; the
		///item stays), BMIFollowUp (E-34) the follow-up, referral and BMI medication sets of CMS69. Empty for the items that pick a direct reference
		///code (GetNotPerformedRole) or a CVX code (InfluenzaVaccination, E-25).</summary>
		public static List<string> GetNotPerformedValueSetOids(EhrNotPerformedItem item) {
			Meth.NoCheckMiddleTierRole();
			switch(item) {
				case EhrNotPerformedItem.TobaccoScreening:
					return new List<string> { CqmCatalog.OID_TOBACCO_SCREENING };
				case EhrNotPerformedItem.BMIFollowUp:
					return new List<string> { CqmCatalog.OID_BMI_FOLLOW_UP_ABOVE,CqmCatalog.OID_BMI_FOLLOW_UP_BELOW,CqmCatalog.OID_BMI_REFERRALS,
						CqmCatalog.OID_BMI_MEDICATIONS_ABOVE,CqmCatalog.OID_BMI_MEDICATIONS_BELOW };
				default:
					return new List<string>();
			}
		}

		///<summary>The value sets of the reasons an item offers (section 8): CMS69's BMI exam "Medical Reason" and "Patient Declined"; CMS68's
		///"Acute Health Crisis"; the BMI follow-up (E-34) "Medical Reason" only; tobacco screening "Medical Reason" (no 2027 exception reads it). The
		///influenza item, which no 2027 measure uses (E-25), offers "Medical Reason", or with isPatientReason "Patient Declined"; there is no 2027
		///value set of system reasons.</summary>
		public static List<string> GetNotPerformedReasonOids(EhrNotPerformedItem item,bool isPatientReason) {
			Meth.NoCheckMiddleTierRole();
			switch(item) {
				case EhrNotPerformedItem.BMIExam:
					return new List<string> { CqmCatalog.OID_REASON_MEDICAL,CqmCatalog.OID_REASON_PATIENT_DECLINED };
				case EhrNotPerformedItem.InfluenzaVaccination:
					if(isPatientReason) {
						return new List<string> { CqmCatalog.OID_REASON_PATIENT_DECLINED };
					}
					return new List<string> { CqmCatalog.OID_REASON_MEDICAL };
				case EhrNotPerformedItem.TobaccoScreening:
				case EhrNotPerformedItem.BMIFollowUp:
					return new List<string> { CqmCatalog.OID_REASON_MEDICAL };
				case EhrNotPerformedItem.DocumentCurrentMeds:
					return new List<string> { CqmCatalog.OID_REASON_ACUTE_HEALTH_CRISIS };
				default:
					return new List<string>();
			}
		}

		///<summary>The codes a new not-performed row of this item may take: the role's code (none until the office imports the release), the codes of
		///the item's value sets whose code-system table is loaded (each code once), or for the influenza item every code of the program's CVX
		///table (E-25).</summary>
		public static List<EhrCode> GetNotPerformedCodes(EhrNotPerformedItem item) {
			Meth.NoCheckMiddleTierRole();
			if(item==EhrNotPerformedItem.InfluenzaVaccination) {
				return Cvxs.GetAll().OrderBy(x => x.CvxCode,StringComparer.Ordinal)
					.Select(x => new EhrCode { CodeValue=x.CvxCode,CodeSystem="CVX",Description=x.Description }).ToList();
			}
			string role=GetNotPerformedRole(item);
			if(role!="") {
				EhrCode ehrCode=GetDirectCode(role);
				if(ehrCode==null) {
					return new List<EhrCode>();
				}
				return new List<EhrCode> { ehrCode };
			}
			return EhrCodes.GetForValueSetOIDs(GetNotPerformedValueSetOids(item),true)
				.GroupBy(x => x.CodeSystem+"|"+x.CodeValue).Select(x => x.First()).ToList();
		}

		///<summary>The item of a saved not-performed row, from its code: the BMI exam and current-medications roles, the tobacco screening and BMI
		///follow-up value sets, a CVX code the influenza item (E-25); null ("other") for any other code, among them a row saved before the office
		///imported the 2027 release. The windows keep it in the row's Tag and never parse a label (FormEhrNotPerformed).</summary>
		public static EhrNotPerformedItem? GetNotPerformedItem(string codeValue,string codeSystem) {
			Meth.NoCheckMiddleTierRole();
			return GetNotPerformedItem(codeValue,codeSystem,EhrCodes.Listt);
		}

		///<summary>The item of a row with this code among these value-set rows (GetNotPerformedItem), or null.</summary>
		public static EhrNotPerformedItem? GetNotPerformedItem(string codeValue,string codeSystem,List<EhrCode> listEhrCodes) {
			Meth.NoCheckMiddleTierRole();
			if(string.IsNullOrEmpty(codeValue)) {
				return null;
			}
			listEhrCodes=listEhrCodes??new List<EhrCode>();
			if(IsDirectCode(CqmCatalog.ROLE_CMS69_BMI,codeValue,codeSystem,listEhrCodes)) {
				return EhrNotPerformedItem.BMIExam;
			}
			if(IsDirectCode(CqmCatalog.ROLE_CMS68_MEDS_DOCUMENTED,codeValue,codeSystem,listEhrCodes)) {
				return EhrNotPerformedItem.DocumentCurrentMeds;
			}
			if(IsInValueSets(codeValue,codeSystem,GetNotPerformedValueSetOids(EhrNotPerformedItem.TobaccoScreening),listEhrCodes)) {
				return EhrNotPerformedItem.TobaccoScreening;
			}
			if(IsInValueSets(codeValue,codeSystem,GetNotPerformedValueSetOids(EhrNotPerformedItem.BMIFollowUp),listEhrCodes)) {
				return EhrNotPerformedItem.BMIFollowUp;
			}
			if(codeSystem=="CVX") {
				return EhrNotPerformedItem.InfluenzaVaccination;
			}
			return null;
		}
		#endregion Not performed

		#region Interventions
		///<summary>The intervention window's code-set list: label and value set, for one code set or (isAllTypes) every one, in the window's order
		///(section 8). The label starts with the InterventionCodeSet's name, which the window saves. The BMI medications are the 2027 sets, and
		///Dialysis is "Dialysis Services" alone (E-28).</summary>
		public static List<KeyValuePair<string,string>> GetInterventionValueSets(InterventionCodeSet interventionCodeSet,bool isAllTypes) {
			Meth.NoCheckMiddleTierRole();
			List<KeyValuePair<string,string>> listValueSets=new List<KeyValuePair<string,string>>();
			if(isAllTypes || interventionCodeSet==InterventionCodeSet.AboveNormalWeight) {
				listValueSets.Add(Pair(InterventionCodeSet.AboveNormalWeight,"Follow-up",CqmCatalog.OID_BMI_FOLLOW_UP_ABOVE));
				listValueSets.Add(Pair(InterventionCodeSet.AboveNormalWeight,"Referral",CqmCatalog.OID_BMI_REFERRALS));
				listValueSets.Add(Pair(InterventionCodeSet.AboveNormalWeight,"Medication",CqmCatalog.OID_BMI_MEDICATIONS_ABOVE));
			}
			if(isAllTypes || interventionCodeSet==InterventionCodeSet.BelowNormalWeight) {
				listValueSets.Add(Pair(InterventionCodeSet.BelowNormalWeight,"Follow-up",CqmCatalog.OID_BMI_FOLLOW_UP_BELOW));
				listValueSets.Add(Pair(InterventionCodeSet.BelowNormalWeight,"Referral",CqmCatalog.OID_BMI_REFERRALS));
				listValueSets.Add(Pair(InterventionCodeSet.BelowNormalWeight,"Medication",CqmCatalog.OID_BMI_MEDICATIONS_BELOW));
			}
			if(isAllTypes || interventionCodeSet==InterventionCodeSet.Nutrition || interventionCodeSet==InterventionCodeSet.PhysicalActivity) {
				listValueSets.Add(Pair(InterventionCodeSet.Nutrition,"Counseling",CqmCatalog.OID_COUNSELING_NUTRITION));
				listValueSets.Add(Pair(InterventionCodeSet.PhysicalActivity,"Counseling",CqmCatalog.OID_COUNSELING_PHYSICAL_ACTIVITY));
			}
			if(isAllTypes || interventionCodeSet==InterventionCodeSet.TobaccoCessation) {
				listValueSets.Add(Pair(InterventionCodeSet.TobaccoCessation,"Counseling",CqmCatalog.OID_TOBACCO_COUNSELING));
				listValueSets.Add(Pair(InterventionCodeSet.TobaccoCessation,"Medication",CqmCatalog.OID_TOBACCO_PHARMACOTHERAPY));
			}
			if(isAllTypes || interventionCodeSet==InterventionCodeSet.Dialysis) {
				listValueSets.Add(Pair(InterventionCodeSet.Dialysis,"Services",CqmCatalog.OID_DIALYSIS_SERVICES));
			}
			return listValueSets;
		}

		private static KeyValuePair<string,string> Pair(InterventionCodeSet interventionCodeSet,string kind,string oid) {
			return new KeyValuePair<string,string>(interventionCodeSet.ToString()+" "+kind,oid);
		}

		///<summary>The value sets of the medications the intervention windows show as interventions: the 2027 BMI medication sets and tobacco
		///cessation pharmacotherapy.</summary>
		public static List<string> GetMedicationInterventionOids() {
			Meth.NoCheckMiddleTierRole();
			return new List<string> { CqmCatalog.OID_BMI_MEDICATIONS_ABOVE,CqmCatalog.OID_BMI_MEDICATIONS_BELOW,CqmCatalog.OID_TOBACCO_PHARMACOTHERAPY };
		}

		///<summary>The code set a medication row of the intervention window is labelled with, by the imported value set that holds its RxCui:
		///AboveNormalWeight, BelowNormalWeight or TobaccoCessation; None when none of the three holds it (section 8; before, RxCui literals of the
		///2014 sets, and "tobacco" for every other).</summary>
		public static InterventionCodeSet GetMedicationCodeSet(string rxCui) {
			Meth.NoCheckMiddleTierRole();
			return GetMedicationCodeSet(rxCui,EhrCodes.Listt);
		}

		///<summary>The code set of a medication row among these value-set rows (GetMedicationCodeSet).</summary>
		public static InterventionCodeSet GetMedicationCodeSet(string rxCui,List<EhrCode> listEhrCodes) {
			Meth.NoCheckMiddleTierRole();
			listEhrCodes=listEhrCodes??new List<EhrCode>();
			if(IsInValueSets(rxCui,"RXNORM",new List<string> { CqmCatalog.OID_BMI_MEDICATIONS_ABOVE },listEhrCodes)) {
				return InterventionCodeSet.AboveNormalWeight;
			}
			if(IsInValueSets(rxCui,"RXNORM",new List<string> { CqmCatalog.OID_BMI_MEDICATIONS_BELOW },listEhrCodes)) {
				return InterventionCodeSet.BelowNormalWeight;
			}
			if(IsInValueSets(rxCui,"RXNORM",new List<string> { CqmCatalog.OID_TOBACCO_PHARMACOTHERAPY },listEhrCodes)) {
				return InterventionCodeSet.TobaccoCessation;
			}
			return InterventionCodeSet.None;
		}
		#endregion Interventions

		#region Recommended codes
		///<summary>The recommended default encounter codes (E-32): the SNOMED CT codes of the imported qualifying-encounter value sets
		///(CqmCatalog.GetQualifyingEncounterValueSets) that the office's snomed table holds, each once, ordered by code. Empty until the office
		///imports the release and loads SNOMED CT. Replaces the nine SNOMED CT literals of the 2014 versions.</summary>
		public static List<Snomed> GetRecommendedEncounterCodes() {
			Meth.NoCheckMiddleTierRole();
			return GetSnomedsInTable(GetSnomedCodes(CqmCatalog.GetQualifyingEncounterValueSets().Select(x => x.Oid).ToList(),EhrCodes.Listt));
		}

		///<summary>The recommended default pregnancy codes (E-32): the SNOMED CT codes of the imported "Pregnancy" value set that the snomed table
		///holds, each once, ordered by code. Every one is in "Pregnancy", which CMS69, CMS155 and CMS165 read. Replaces the nine SNOMED CT literals
		///picked for the 2014 pregnancy sets.</summary>
		public static List<Snomed> GetRecommendedPregnancyCodes() {
			Meth.NoCheckMiddleTierRole();
			return GetSnomedsInTable(GetSnomedCodes(new List<string> { CqmCatalog.OID_PREGNANCY },EhrCodes.Listt));
		}

		///<summary>The SNOMED CT codes of these value sets among these rows, each once, ordered by code (ordinal).</summary>
		public static List<string> GetSnomedCodes(List<string> listValueSetOids,List<EhrCode> listEhrCodes) {
			Meth.NoCheckMiddleTierRole();
			return (listEhrCodes??new List<EhrCode>()).Where(x => x.CodeSystem=="SNOMEDCT" && listValueSetOids.Contains(x.ValueSetOID))
				.Select(x => x.CodeValue).Distinct().OrderBy(x => x,StringComparer.Ordinal).ToList();
		}

		///<summary>The snomed rows of these codes that the table holds, in the order of the codes. A few queries of at most 500 codes each.</summary>
		private static List<Snomed> GetSnomedsInTable(List<string> listCodes) {
			Dictionary<string,Snomed> dictSnomeds=new Dictionary<string,Snomed>();
			for(int i=0;i<listCodes.Count;i+=COUNT_CODES_PER_QUERY) {
				List<string> listPart=listCodes.Skip(i).Take(COUNT_CODES_PER_QUERY).ToList();
				foreach(Snomed snomed in Snomeds.GetByCodes(string.Join(",",listPart))) {
					if(!dictSnomeds.ContainsKey(snomed.SnomedCode)) {
						dictSnomeds.Add(snomed.SnomedCode,snomed);
					}
				}
			}
			return listCodes.Where(x => dictSnomeds.ContainsKey(x)).Select(x => dictSnomeds[x]).ToList();
		}
		#endregion Recommended codes

		///<summary>True when one of the rows of these value sets has this code in this code system (whether its table is loaded or not).</summary>
		private static bool IsInValueSets(string codeValue,string codeSystem,List<string> listValueSetOids,List<EhrCode> listEhrCodes) {
			return listEhrCodes.Any(x => x.CodeValue==codeValue && x.CodeSystem==codeSystem && listValueSetOids.Contains(x.ValueSetOID));
		}
	}
}
