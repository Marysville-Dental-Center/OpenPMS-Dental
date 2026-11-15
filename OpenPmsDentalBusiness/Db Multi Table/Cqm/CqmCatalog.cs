using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace OpenPmsDentalBusiness {
	///<summary>The catalog of the clinical quality measures this program reports: CMS's 2027 eligible-clinician eCQM versions of the seven measures
	///CMS still publishes of the nine the program had (docs/specs/ehr-2027-measures-plan.md, package EHR-6a, sections 4, 5.2, 7 and 11.1). The
	///measures are data, so a yearly update edits this file and the measure classes, not code paths, and no name carries a year (E-31). Per
	///measure: its CMS ID and version, title, steward and developer, set ID and version-specific ID, population groups and strata with their HQMF
	///IDs, scoring basis, improvement notation, and the copyright and disclaimer statements of its HQMF file, verbatim (owner decision 5 for the
	///NCQA measures, E-38 for the four CMS measures). Then the value sets the measures' CQL declares (OID, name, QDM category and datatypes, the
	///measures that declare them), the direct-reference-code roles the importer resolves from the office's Direct Reference Codes file, and the
	///value sets the data-entry windows pick from (section 8). Every fact comes from CMS's public 2027 measure packages (HQMF for the IDs and the
	///statements, CQL for the value sets and direct reference codes; the port note docs/port-notes/EHR-6.md records the files and their SHA-256).
	///**No code of any code system is in this file**: value sets are named by OID and name, direct reference codes by measure, code system and
	///meaning, and for the three LOINC codes the program must tell apart by the display name LOINC gives them (E-23; LOINC's notice is in
	///THIRD-PARTY-NOTICES.txt). The codes come only from the office's own VSAC download (EhrValueSetImport).</summary>
	public class CqmCatalog {
		///<summary>The VSAC release whose value sets and direct reference codes the 2027 versions use (owner decision 3).</summary>
		public const string RELEASE_LABEL="VSAC eCQM Value Sets for Eligible Clinicians, 2027 performance period, published 2026-05-14";
		///<summary>The release's publication date.</summary>
		public const string RELEASE_DATE="2026-05-14";
		///<summary>The performance period the versions are for: January 1 to December 31 of this year.</summary>
		public const int PERFORMANCE_YEAR=2027;
		///<summary>The steward of CMS68, CMS69, CMS74 and CMS75 (each HQMF file's custodian).</summary>
		public const string STEWARD_CMS="Centers for Medicare & Medicaid Services (CMS)";
		///<summary>The steward and developer of CMS138, CMS155 and CMS165.</summary>
		public const string STEWARD_NCQA="National Committee for Quality Assurance";
		///<summary>The developer of the four CMS measures (each HQMF file's author).</summary>
		public const string DEVELOPER_AIR="American Institutes for Research (AIR)";
		///<summary>The QDM category of the ehrcode rows the importer writes for direct reference codes.</summary>
		public const string DIRECT_CODE_CATEGORY="Direct Reference Code";
		///<summary>The fork key that ehrcode.ValueSetOID holds for a direct reference code: drc:[CMS ID]:[role] (section 7.3). No schema change.</summary>
		public const string DIRECT_CODE_KEY_PREFIX="drc:";
		///<summary>The role of a direct reference code that no criterion of this program reads (hospice discharge, FACIT-Pal and the others of
		///section 7.3): imported and listed under it.</summary>
		public const string DIRECT_CODE_ROLE_LISTED="Listed";
		///<summary>The direct-reference-code role of CMS68's "current medications documented" procedure: what the medical window writes and filters
		///the not-performed rows by, and the not-performed item DocumentCurrentMeds (section 8, EHR-7).</summary>
		public const string ROLE_CMS68_MEDS_DOCUMENTED="Cms68MedsDocumented";
		///<summary>The direct-reference-code role of CMS69's BMI exam: the not-performed item BMIExam (section 8, EHR-7).</summary>
		public const string ROLE_CMS69_BMI="Cms69Bmi";

		#region The pick lists of the data-entry windows (EHR-7)
		//The value sets the data-entry windows name (section 8): each is a value set of a measure and one of the pick lists below, so the windows
		//name no value-set OID themselves (ValueSetSourceTests: no value-set OID literal outside this file).
		///<summary>"Follow Up for Above Normal BMI" (CMS69): the intervention window's AboveNormalWeight follow-up, the not-performed BMI follow-up.</summary>
		public const string OID_BMI_FOLLOW_UP_ABOVE="2.16.840.1.113883.3.600.1.1525";
		///<summary>"Follow Up for Below Normal BMI" (CMS69): the intervention window's BelowNormalWeight follow-up, the not-performed BMI follow-up.</summary>
		public const string OID_BMI_FOLLOW_UP_BELOW="2.16.840.1.113883.3.600.1.1528";
		///<summary>"Referrals Where Weight Assessment May Occur" (CMS69): the intervention window's referrals, the not-performed BMI follow-up.</summary>
		public const string OID_BMI_REFERRALS="2.16.840.1.113883.3.600.1.1527";
		///<summary>"Medications for Above Normal BMI" (CMS69), in place of the 2014 "Above Normal Medications" set.</summary>
		public const string OID_BMI_MEDICATIONS_ABOVE="2.16.840.1.113883.3.526.3.1561";
		///<summary>"Medications for Below Normal BMI" (CMS69), in place of the 2014 "Below Normal Medications" set.</summary>
		public const string OID_BMI_MEDICATIONS_BELOW="2.16.840.1.113883.3.526.3.1562";
		///<summary>"Counseling for Nutrition" (CMS155).</summary>
		public const string OID_COUNSELING_NUTRITION="2.16.840.1.113883.3.464.1003.195.12.1003";
		///<summary>"Counseling for Physical Activity" (CMS155).</summary>
		public const string OID_COUNSELING_PHYSICAL_ACTIVITY="2.16.840.1.113883.3.464.1003.118.12.1035";
		///<summary>"Tobacco Use Cessation Counseling" (CMS138).</summary>
		public const string OID_TOBACCO_COUNSELING="2.16.840.1.113883.3.526.3.509";
		///<summary>"Tobacco Use Cessation Pharmacotherapy" (CMS138).</summary>
		public const string OID_TOBACCO_PHARMACOTHERAPY="2.16.840.1.113883.3.526.3.1190";
		///<summary>"Dialysis Services" (CMS165), in place of the 2014 dialysis education and other dialysis services sets (E-28).</summary>
		public const string OID_DIALYSIS_SERVICES="2.16.840.1.113883.3.464.1003.109.12.1013";
		///<summary>"Tobacco Use Screening" (CMS138): the Tobacco Use tab's assessments, the not-performed item TobaccoScreening.</summary>
		public const string OID_TOBACCO_SCREENING="2.16.840.1.113883.3.526.3.1278";
		///<summary>"Tobacco User" (CMS138).</summary>
		public const string OID_TOBACCO_USER="2.16.840.1.113883.3.526.3.1170";
		///<summary>"Tobacco Non User" (CMS138).</summary>
		public const string OID_TOBACCO_NON_USER="2.16.840.1.113883.3.526.3.1189";
		///<summary>"Pregnancy" (CMS69, CMS155, CMS165): the pregnancy problems and the recommended pregnancy codes, alone (the 2014 second set is
		///gone).</summary>
		public const string OID_PREGNANCY="2.16.840.1.113883.3.526.3.378";
		///<summary>"Acute Health Crisis" (CMS68): the reason for current medications not documented.</summary>
		public const string OID_REASON_ACUTE_HEALTH_CRISIS="2.16.840.1.113762.1.4.1248.429";
		///<summary>"Medical Reason" (CMS69): a reason for the BMI exam or the BMI follow-up not performed.</summary>
		public const string OID_REASON_MEDICAL="2.16.840.1.113883.3.526.3.1007";
		///<summary>"Patient Declined" (CMS69): a reason for the BMI exam not performed.</summary>
		public const string OID_REASON_PATIENT_DECLINED="2.16.840.1.113883.3.526.3.1582";

		///<summary>The qualifying-encounter value sets (E-32): the Encounter value sets the seven measures count a visit by, which the recommended
		///default encounter codes come from (FormEhrSettings, FormEncounterTool). Left out: the Encounter value sets that only exclude a patient
		///(Hospice Encounter, Palliative Care Encounter, Frailty Encounter, Encounter Inpatient, Emergency Department Evaluation and Management
		///Visit).</summary>
		private static readonly string[] _arrayQualifyingEncounterOids=new string[] {
			"2.16.840.1.113883.3.464.1003.101.12.1001",//Office Visit
			"2.16.840.1.113883.3.464.1003.101.12.1016",//Home Healthcare Services
			"2.16.840.1.113883.3.464.1003.101.12.1022",//Preventive Care Services, Initial Office Visit, 0 to 17
			"2.16.840.1.113883.3.464.1003.101.12.1023",//Preventive Care Services Initial Office Visit, 18 and Up
			"2.16.840.1.113883.3.464.1003.101.12.1024",//Preventive Care, Established Office Visit, 0 to 17
			"2.16.840.1.113883.3.464.1003.101.12.1025",//Preventive Care Services Established Office Visit, 18 and Up
			"2.16.840.1.113883.3.464.1003.101.12.1026",//Preventive Care Services Individual Counseling
			"2.16.840.1.113883.3.464.1003.101.12.1027",//Preventive Care Services Group Counseling
			"2.16.840.1.113883.3.464.1003.101.12.1080",//Telephone Visits
			"2.16.840.1.113883.3.464.1003.101.12.1089",//Virtual Encounter
			"2.16.840.1.113883.3.464.1003.125.12.1003",//Clinical Oral Evaluation
			"2.16.840.1.113883.3.464.1003.1006",//Nutrition Services
			"2.16.840.1.113883.3.526.3.1011",//Occupational Therapy Evaluation
			"2.16.840.1.113883.3.526.3.1022",//Physical Therapy Evaluation
			"2.16.840.1.113883.3.526.3.1141",//Psychoanalysis
			"2.16.840.1.113883.3.526.3.1240",//Annual Wellness Visit
			"2.16.840.1.113883.3.526.3.1285",//Ophthalmological Services
			"2.16.840.1.113883.3.526.3.1492",//Psych Visit Diagnostic Evaluation
			"2.16.840.1.113883.3.526.3.1496",//Psych Visit Psychotherapy
			"2.16.840.1.113883.3.526.3.1530",//Speech and Hearing Evaluation
			"2.16.840.1.113883.3.600.1.1751",//Encounter to Evaluate BMI
			"2.16.840.1.113883.3.600.1.1834",//Encounter to Document Medications
		};
		#endregion The pick lists of the data-entry windows (EHR-7)

		///<summary>The copyright statement of CMS68, as the HQMF file of each holds it (x/CMS68-v16.0.000-QDM.xml).</summary>

		private const string _copyrightCms68="<p>This electronic clinical quality measure (Measure) and related data specifications are owned and stewarded by the Centers for Medicare "
			+"&amp; Medicaid Services (CMS). CMS contracted (Contract # 75FCMC18D0027/ Task Order #: 75FCMC24F0144) with the American Institutes for "
			+"Research (AIR) to develop this electronic measure. AIR is not responsible for any use of the Measure. AIR makes no representations, "
			+"warranties, or endorsement about the quality of any organization or physician that uses or reports performance measures and AIR has no "
			+"liability to anyone who relies on such measures or specifications.</p>\n"
			+"<p>Limited proprietary coding is contained in the Measure specifications for user convenience. Users of proprietary code sets should obtain "
			+"all necessary licenses from the owners of the code sets. AIR disclaims all liability for use or accuracy of any third-party codes contained "
			+"in the specifications.</p>\n"
			+"<p>CPT (R) only Copyright 2025 American Medical Association. All rights Reserved. No fee schedules, basic unit, relative values or related "
			+"listings are included in CPT (R). AMA does not directly or indirectly practice medicine or dispense medical services. AMA assumes no "
			+"liability for data contained herein.</p>\n"
			+"<p>This material includes SNOMED Clinical Terms (R) (SNOMED CT [R]) which&nbsp;is used by permission of the International Health "
			+"Terminology Standards Development Organisation (IHTSDO). All rights reserved. SNOMED CT (R), was originally created by The College of "
			+"American Pathologists. \"SNOMED\" and \"SNOMED CT\" are registered trademarks of the IHTSDO.</p>";

		///<summary>The disclaimer statement of CMS68, as the HQMF file of each holds it (x/CMS68-v16.0.000-QDM.xml).</summary>

		private const string _disclaimerCms68="<p>This performance Measure is not a clinical guideline, does not establish a standard of medical care, and has not been tested for all "
			+"potential applications.</p>\n"
			+"<p>THE MEASURE AND SPECIFICATIONS ARE PROVIDED \"AS IS\" WITHOUT WARRANTY OF ANY KIND.</p>\n"
			+"<p>Due to technical limitations, registered trademarks are indicated by (R) or [R] and unregistered trademarks are indicated by (TM) or "
			+"[TM].</p>";

		///<summary>The copyright statement of CMS69, as the HQMF file of each holds it (x/CMS69-v15.0.000-QDM.xml).</summary>

		private const string _copyrightCms69="<p>This electronic clinical quality measure (Measure) and related data specifications are owned and stewarded by the Centers for Medicare "
			+"&amp; Medicaid Services (CMS). CMS contracted (Contract # 75FCMC18D0027/ Task Order #: 75FCMC24F0144) with the American Institutes for "
			+"Research (AIR) to develop this electronic measure. AIR is not responsible for any use of the Measure. AIR makes no representations, "
			+"warranties, or endorsement about the quality of any organization or physician that uses or reports performance measures and AIR has no "
			+"liability to anyone who relies on such measures or specifications. Limited proprietary coding is contained in the Measure specifications "
			+"for user convenience. Users of proprietary code sets should obtain all necessary licenses from the owners of the code sets. AIR disclaims "
			+"all liability for use or accuracy of any third-party codes contained in the specifications. CPT (R) only Copyright 2025 American Medical "
			+"Association. All rights Reserved. No fee schedules, basic unit, relative values or related listings are included in CPT (R). AMA does not "
			+"directly or indirectly practice medicine or dispense medical services. AMA assumes no liability for data contained herein.</p>\n"
			+"<p>Some measures use RxNorm, a standardized nomenclature and coding for clinical drugs and drug delivery devices, which is made publicly "
			+"available courtesy of the U.S. National Library of Medicine (NLM), National Institutes of Health, Department of Health and Human Services. "
			+"NLM is not responsible for the measures and does not endorse or recommend this or any other product.</p>";

		///<summary>The disclaimer statement of CMS69, as the HQMF file of each holds it (x/CMS69-v15.0.000-QDM.xml).</summary>

		private const string _disclaimerCms69="<p>These performance measures are not clinical guidelines and do not establish a standard of medical care, and have not been tested for all "
			+"potential applications. THE MEASURES AND SPECIFICATIONS ARE PROVIDED \u201cAS IS\u201d WITHOUT WARRANTY OF ANY KIND. Due to technical "
			+"limitations, registered trademarks are indicated by (R) or [R] and unregistered trademarks are indicated by (TM) or [TM].</p>";

		///<summary>The copyright statement of CMS74, as the HQMF file of each holds it (x/CMS74-v16.0.000-QDM.xml).</summary>

		private const string _copyrightCms74="<p>This Physician Performance Measure (Measure) and related data specifications are owned and stewarded by the Centers for Medicare &amp; "
			+"Medicaid Services (CMS). CMS contracted (Contract # 75FCMC18D0027/ Task Order #: 75FCMC24F0144) with the American Institutes for Research "
			+"(AIR) to develop this electronic measure. AIR disclaims all liability for use or accuracy of any third-party codes contained in the "
			+"specifications. CPT (R) only Copyright 2025 American Medical Association. All rights Reserved. No fee schedules, basic unit, relative "
			+"values or related listings are included in CPT (R). AMA does not directly or indirectly practice medicine or dispense medical services. AMA "
			+"assumes no liability for data contained herein. The American Dental Association (\u201cADA\u201d) holds a copyright to the Current Dental "
			+"Terminology (\u201cCDT\u201d) codes contained in certain measure specifications. The CDT codes in the measure specifications are included "
			+"with the permission of the ADA. All uses of the CDT codes require a license from the ADA. No alteration, amendments, or modifications of "
			+"the CDT or any portion thereof is allowed. Resale, transmission, or distribution of copies of the CDT or other portions of the CDT is also "
			+"not allowed. To inquire about licensing, contact CDT-SNODENT@ada.org. This material contains content from LOINC (http://loinc.org). LOINC "
			+"is copyright (C) Regenstrief Institute, Inc. and the Logical Observation Identifiers Names and Codes (LOINC) Committee and is available at "
			+"no cost under the license at\u202fhttp://loinc.org/license. LOINC (R) is a registered United States trademark of Regenstrief Institute, "
			+"Inc. This material includes SNOMED Clinical Terms (R) (SNOMED CT [R]) which is used by permission of the International Health Terminology "
			+"Standards Development Organisation (IHTSDO). All rights reserved. SNOMED CT (R), was originally created by The College of American "
			+"Pathologists. \"SNOMED\" and \"SNOMED CT\" are registered trademarks of the IHTSDO. ICD-10 is copyright 2025 World Health Organization. All "
			+"Rights Reserved.</p>\n"
			+"<p>\u201cHL7\u201d is the registered trademark of Health Level Seven International.</p>";

		///<summary>The disclaimer statement of CMS74, CMS75, as the HQMF file of each holds it (x/CMS74-v16.0.000-QDM.xml, x/CMS75-v15.0.000-QDM.xml).</summary>

		private const string _disclaimerCms74="<p>The performance Measure is not a clinical guideline and does not establish a standard of medical care, and has not been tested for all "
			+"potential applications.</p>\n"
			+"<p>THE MEASURE AND SPECIFICATIONS ARE PROVIDED \"AS IS\" WITHOUT WARRANTY OF ANY KIND.</p>\n"
			+"<p>Due to technical limitations, registered trademarks are indicated by (R) or [R] and unregistered trademarks are indicated by (TM) or "
			+"[TM].</p>";

		///<summary>The copyright statement of CMS75, as the HQMF file of each holds it (x/CMS75-v15.0.000-QDM.xml).</summary>

		private const string _copyrightCms75="<p>This electronic clinical quality measure (Measure) and related data specifications are owned and stewarded by the Centers for Medicare "
			+"&amp; Medicaid Services (CMS). CMS contracted (Contract # 75FCMC18D0027/ Task Order #: 75FCMC24F0144) with the American Institutes for "
			+"Research (AIR) to develop this electronic measure. AIR is not responsible for any use of the Measure. AIR makes no representations, "
			+"warranties, or endorsement about the quality of any organization or physician that uses or reports performance measures and AIR has no "
			+"liability to anyone who relies on such measures or specifications.</p>\n"
			+"<p>Limited proprietary coding is contained in the Measure specifications for user convenience. Users of proprietary code sets should obtain "
			+"all necessary licenses from the owners of the code sets. AIR disclaims all liability for use or accuracy of any third-party codes contained "
			+"in the specifications.</p>\n"
			+"<p>The American Dental Association (\u201cADA\u201d) holds a copyright to the Current Dental Terminology (\u201cCDT\u201d) codes contained "
			+"in certain measure specifications. The CDT codes in the measure specifications are included with the permission of the ADA. All uses of the "
			+"CDT codes require a license from the ADA. No alteration, amendments, or modifications of the CDT or any portion thereof is allowed. Resale, "
			+"transmission, or distribution of copies of the CDT or other portions of the CDT is also not allowed. To inquire about licensing, contact "
			+"CDT-SNODENT@ada.org.</p>\n"
			+"<p>CPT (R) only Copyright 2025 American Medical Association. All rights Reserved. No fee schedules, basic unit, relative values or related "
			+"listings are included in CPT (R). AMA does not directly or indirectly practice medicine or dispense medical services. AMA assumes no "
			+"liability for data contained herein.&nbsp;&nbsp;</p>\n"
			+"<p>This material contains content from LOINC (http://loinc.org). LOINC is copyright (C) Regenstrief Institute, Inc. and the Logical "
			+"Observation Identifiers Names and Codes (LOINC) Committee and is available at no cost under the license at\u202fhttp://loinc.org/license. "
			+"LOINC (R) is a registered United States trademark of Regenstrief Institute, Inc.</p>\n"
			+"<p>This material includes SNOMED Clinical Terms (R) (SNOMED CT [R]) which is used by permission of the International Health Terminology "
			+"Standards Development Organisation (IHTSDO). All rights reserved. SNOMED CT (R), was originally created by The College of American "
			+"Pathologists. \"SNOMED\" and \"SNOMED CT\" are registered trademarks of the IHTSDO.&nbsp;</p>\n"
			+"<p>ICD-10 is copyright 2025 World Health Organization. All Rights Reserved.&nbsp;</p>\n"
			+"<p>\u201cHL7\u201d is the registered trademark of Health Level Seven International.</p>";

		///<summary>The copyright statement of CMS138, as the HQMF file of each holds it (x/CMS138-v15.0.000-QDM.xml).</summary>

		private const string _copyrightCms138="<p>This Physician Performance Measure (Measure) and related data specifications are owned by the National Committee for Quality Assurance "
			+"(NCQA). NCQA is not responsible for any use of the Measure. NCQA makes no representations, warranties or endorsements about the quality of "
			+"any product, test or protocol identified as numerator compliant or otherwise identified as meeting the requirements of the measure or "
			+"specification. NCQA makes no representations, warranties, or endorsement about the quality of any organization or physician that uses or "
			+"reports performance measures and NCQA has no liability to anyone who relies on such measures or specifications. NCQA holds a copyright in "
			+"the Measure. The Measure may be used for internal, noncommercial purposes (e.g., use by healthcare providers in connection with their "
			+"practices) without obtaining approval from NCQA. All other uses, including a commercial use (including but not limited to vendors using or "
			+"embedding the measures and specifications into any product or service to calculate measure results for customers for any purpose), must be "
			+"approved by NCQA and are subject to a license at the discretion of NCQA. The Physician Consortium for Performance Improvement\u2019s (PCPI) "
			+"and American Medical Association\u2019s (AMA) significant past efforts and contributions to the development and updating of the measure are "
			+"acknowledged. (C) 2012-2026 National Committee for Quality Assurance. All Rights Reserved.</p>\n"
			+"<p>Limited proprietary coding is contained in the Measure specifications for user convenience. Users of proprietary code sets should obtain "
			+"all necessary licenses from the owners of the code sets. NCQA disclaims all liability for use or accuracy of any third-party codes "
			+"contained in the specifications.</p>\n"
			+"<p>CPT (R) only Copyright 2025 American Medical Association. All rights Reserved. No fee schedules, basic unit, relative values or related "
			+"listings are included in CPT (R). AMA does not directly or indirectly practice medicine or dispense medical services. AMA assumes no "
			+"liability for data contained herein.</p>\n"
			+"<p>This material contains content from LOINC (http://loinc.org). LOINC is copyright (C) Regenstrief Institute, Inc. and the Logical "
			+"Observation Identifiers Names and Codes (LOINC) Committee and is available at no cost under the license at\u202fhttp://loinc.org/license. "
			+"LOINC (R) is a registered United States trademark of Regenstrief Institute, Inc.</p>\n"
			+"<p>This material includes SNOMED Clinical Terms (R) (SNOMED CT [R]) which is used by permission of the International Health Terminology "
			+"Standards Development Organisation (IHTSDO). All rights reserved. SNOMED CT (R), was originally created by The College of American "
			+"Pathologists. \"SNOMED\" and \"SNOMED CT\" are registered trademarks of the IHTSDO.</p>\n"
			+"<p>ICD-10 is copyright 2025 World Health Organization. All Rights Reserved.</p>\n"
			+"<p>Some measures use RxNorm, a standardized nomenclature and coding for clinical drugs and drug delivery devices, which is made publicly "
			+"available courtesy of the U.S. National Library of Medicine (NLM), National Institutes of Health, Department of Health and Human Services. "
			+"NLM is not responsible for the measures and does not endorse or recommend this or any other product.</p>\n"
			+"<p>\u201cHL7\u201d is the registered trademark of Health Level Seven International.</p>";

		///<summary>The disclaimer statement of CMS138, CMS155, CMS165, as the HQMF file of each holds it (x/CMS138-v15.0.000-QDM.xml, x/CMS155-v15.0.000-QDM.xml, x/CMS165-v15.0.000-QDM.xml).</summary>

		private const string _disclaimerCms138="<p>The performance Measure is not a clinical guideline and does not establish a standard of medical care, and has not been tested for all "
			+"potential applications. THE MEASURE AND SPECIFICATIONS ARE PROVIDED \"AS IS\" WITHOUT WARRANTY OF ANY KIND.</p>\n"
			+"<p>Due to technical limitations, registered trademarks are indicated by (R) or [R] and unregistered trademarks are indicated by (TM) or "
			+"[TM].</p>";

		///<summary>The copyright statement of CMS155, CMS165, as the HQMF file of each holds it (x/CMS155-v15.0.000-QDM.xml, x/CMS165-v15.0.000-QDM.xml).</summary>

		private const string _copyrightCms155="<p>This Physician Performance Measure (Measure) and related data specifications are owned and were developed by the National Committee for "
			+"Quality Assurance (NCQA). NCQA is not responsible for any use of the Measure. NCQA makes no representations, warranties or endorsements "
			+"about the quality of any product, test or protocol identified as numerator compliant or otherwise identified as meeting the requirements of "
			+"the measure or specification. NCQA makes no representations, warranties, or endorsement about the quality of any organization or physician "
			+"that uses or reports performance measures and NCQA has no liability to anyone who relies on such measures or specifications. NCQA holds a "
			+"copyright in the Measure. The Measure may be used for internal, noncommercial purposes (e.g., use by healthcare providers in connection "
			+"with their practices) without obtaining approval from NCQA. All other uses, including a commercial use (including but not limited to "
			+"vendors using or embedding the measures and specifications into any product or service to calculate measure results for customers for any "
			+"purpose), must be approved by NCQA and are subject to a license at the discretion of NCQA. (C) 2012-2026 National Committee for Quality "
			+"Assurance. All Rights Reserved.</p>\n"
			+"<p>Limited proprietary coding is contained in the Measure specifications for user convenience. Users of proprietary code sets should obtain "
			+"all necessary licenses from the owners of the code sets. NCQA disclaims all liability for use or accuracy of any third-party codes "
			+"contained in the specifications.</p>\n"
			+"<p>CPT (R) only Copyright 2025 American Medical Association. All rights Reserved. No fee schedules, basic unit, relative values or related "
			+"listings are included in CPT (R). AMA does not directly or indirectly practice medicine or dispense medical services. AMA assumes no "
			+"liability for data contained herein.</p>\n"
			+"<p>This material contains content from LOINC (http://loinc.org). LOINC is copyright (C) Regenstrief Institute, Inc. and the Logical "
			+"Observation Identifiers Names and Codes (LOINC) Committee and is available at no cost under the license at\u202fhttp://loinc.org/license. "
			+"LOINC (R) is a registered United States trademark of Regenstrief Institute, Inc.</p>\n"
			+"<p>This material includes SNOMED Clinical Terms (R) (SNOMED CT [R]) which is used by permission of the International Health Terminology "
			+"Standards Development Organisation (IHTSDO). All rights reserved. SNOMED CT (R), was originally created by The College of American "
			+"Pathologists. \"SNOMED\" and \"SNOMED CT\" are registered trademarks of the IHTSDO.</p>\n"
			+"<p>ICD-10 is copyright 2025 World Health Organization. All Rights Reserved.</p>\n"
			+"<p>Some measures use RxNorm, a standardized nomenclature and coding for clinical drugs and drug delivery devices, which is made publicly "
			+"available courtesy of the U.S. National Library of Medicine (NLM), National Institutes of Health, Department of Health and Human Services. "
			+"NLM is not responsible for the measures and does not endorse or recommend this or any other product.</p>\n"
			+"<p>\u201cHL7\u201d is the registered trademark of Health Level Seven International.</p>";

		///<summary>The value sets the seven measures' CQL declares outside CQMCommonQDM (the `valueset` lines of each package's x/cql files; 68
		///distinct): OID, name as the CQL names it, QDM category, the QDM datatypes and attributes the CQL reads it as, and the measures whose CQL
		///(the measure's own library or a library it includes) declares it. "Encounter Inpatient" is among them because HospiceQDM and CMS165 declare
		///it themselves.</summary>
		private static readonly string[][] _arrayValueSets=new string[][] {
			new[] { "2.16.840.1.113762.1.4.1021.121","Federal Administrative Sex","Patient Characteristic","Patient Characteristic Sex","CMS68,CMS69,CMS74,CMS75,CMS138,CMS155,CMS165" },
			new[] { "2.16.840.1.113762.1.4.1047.502","Overweight or Obese","Condition/Diagnosis/Problem","Diagnosis; Attribute: reason","CMS69" },
			new[] { "2.16.840.1.113762.1.4.1248.429","Acute Health Crisis","Attribute","Attribute: negationRationale","CMS68" },
			new[] { "2.16.840.1.113883.3.464.1003.101.12.1001","Office Visit","Encounter","Encounter, Performed","CMS138,CMS155,CMS165" },
			new[] { "2.16.840.1.113883.3.464.1003.101.12.1010","Emergency Department Evaluation and Management Visit","Encounter","Encounter, Performed","CMS165" },
			new[] { "2.16.840.1.113883.3.464.1003.101.12.1016","Home Healthcare Services","Encounter","Encounter, Performed","CMS138,CMS155,CMS165" },
			new[] { "2.16.840.1.113883.3.464.1003.101.12.1022","Preventive Care Services, Initial Office Visit, 0 to 17","Encounter","Encounter, Performed","CMS138,CMS155" },
			new[] { "2.16.840.1.113883.3.464.1003.101.12.1023","Preventive Care Services Initial Office Visit, 18 and Up","Encounter","Encounter, Performed","CMS138,CMS165" },
			new[] { "2.16.840.1.113883.3.464.1003.101.12.1024","Preventive Care, Established Office Visit, 0 to 17","Encounter","Encounter, Performed","CMS138,CMS155" },
			new[] { "2.16.840.1.113883.3.464.1003.101.12.1025","Preventive Care Services Established Office Visit, 18 and Up","Encounter","Encounter, Performed","CMS138,CMS165" },
			new[] { "2.16.840.1.113883.3.464.1003.101.12.1026","Preventive Care Services Individual Counseling","Encounter","Encounter, Performed","CMS138,CMS155" },
			new[] { "2.16.840.1.113883.3.464.1003.101.12.1027","Preventive Care Services Group Counseling","Encounter","Encounter, Performed","CMS138,CMS155" },
			new[] { "2.16.840.1.113883.3.464.1003.101.12.1080","Telephone Visits","Encounter","Encounter, Performed","CMS138,CMS155,CMS165" },
			new[] { "2.16.840.1.113883.3.464.1003.101.12.1088","Frailty Encounter","Encounter","Encounter, Performed","CMS165" },
			new[] { "2.16.840.1.113883.3.464.1003.101.12.1089","Virtual Encounter","Encounter","Encounter, Performed","CMS138,CMS165" },
			new[] { "2.16.840.1.113883.3.464.1003.101.12.1090","Palliative Care Encounter","Encounter","Encounter, Performed","CMS69,CMS165" },
			new[] { "2.16.840.1.113883.3.464.1003.104.12.1011","Essential Hypertension","Condition/Diagnosis/Problem","Diagnosis","CMS165" },
			new[] { "2.16.840.1.113883.3.464.1003.109.12.1012","Kidney Transplant","Procedure","Procedure, Performed","CMS165" },
			new[] { "2.16.840.1.113883.3.464.1003.109.12.1013","Dialysis Services","Procedure","Procedure, Performed","CMS165" },
			new[] { "2.16.840.1.113883.3.464.1003.110.12.1082","Advanced Illness","Condition/Diagnosis/Problem","Diagnosis","CMS165" },
			new[] { "2.16.840.1.113883.3.464.1003.113.12.1074","Frailty Diagnosis","Condition/Diagnosis/Problem","Diagnosis","CMS165" },
			new[] { "2.16.840.1.113883.3.464.1003.113.12.1075","Frailty Symptom","Symptom","Symptom","CMS165" },
			new[] { "2.16.840.1.113883.3.464.1003.118.12.1035","Counseling for Physical Activity","Intervention","Intervention, Performed","CMS155" },
			new[] { "2.16.840.1.113883.3.464.1003.118.12.1300","Frailty Device","Device","Device, Order; Attribute: result","CMS165" },
			new[] { "2.16.840.1.113883.3.464.1003.121.12.1012","BMI percentile","Physical Exam","Physical Exam, Performed","CMS155" },
			new[] { "2.16.840.1.113883.3.464.1003.121.12.1014","Height","Physical Exam","Physical Exam, Performed","CMS155" },
			new[] { "2.16.840.1.113883.3.464.1003.121.12.1015","Weight","Physical Exam","Physical Exam, Performed","CMS155" },
			new[] { "2.16.840.1.113883.3.464.1003.125.12.1002","Fluoride Varnish Application for Children","Procedure","Procedure, Performed","CMS74" },
			new[] { "2.16.840.1.113883.3.464.1003.125.12.1003","Clinical Oral Evaluation","Encounter","Encounter, Performed","CMS74,CMS75" },
			new[] { "2.16.840.1.113883.3.464.1003.125.12.1004","Dental Caries","Condition/Diagnosis/Problem","Diagnosis","CMS75" },
			new[] { "2.16.840.1.113883.3.464.1003.195.12.1003","Counseling for Nutrition","Intervention","Intervention, Performed","CMS155" },
			new[] { "2.16.840.1.113883.3.464.1003.196.12.1510","Dementia Medications","Medication","Medication, Active","CMS165" },
			new[] { "2.16.840.1.113883.3.464.1003.198.12.1135","Palliative Care Intervention","Intervention","Intervention, Performed","CMS69,CMS165" },
			new[] { "2.16.840.1.113883.3.464.1003.1003","Hospice Encounter","Encounter","Encounter, Performed","CMS69,CMS74,CMS75,CMS138,CMS155,CMS165" },
			new[] { "2.16.840.1.113883.3.464.1003.1006","Nutrition Services","Encounter","Encounter, Performed","CMS138" },
			new[] { "2.16.840.1.113883.3.464.1003.1165","Hospice Diagnosis","Condition/Diagnosis/Problem","Diagnosis","CMS69,CMS74,CMS75,CMS138,CMS155,CMS165" },
			new[] { "2.16.840.1.113883.3.464.1003.1167","Palliative Care Diagnosis","Condition/Diagnosis/Problem","Diagnosis","CMS69,CMS165" },
			new[] { "2.16.840.1.113883.3.526.3.353","End Stage Renal Disease","Condition/Diagnosis/Problem","Diagnosis","CMS165" },
			new[] { "2.16.840.1.113883.3.526.3.378","Pregnancy","Condition/Diagnosis/Problem","Diagnosis","CMS69,CMS155,CMS165" },
			new[] { "2.16.840.1.113883.3.526.3.509","Tobacco Use Cessation Counseling","Intervention","Intervention, Performed","CMS138" },
			new[] { "2.16.840.1.113883.3.526.3.1002","Chronic Kidney Disease, Stage 5","Condition/Diagnosis/Problem","Diagnosis","CMS165" },
			new[] { "2.16.840.1.113883.3.526.3.1007","Medical Reason","Attribute","Attribute: negationRationale","CMS69" },
			new[] { "2.16.840.1.113883.3.526.3.1011","Occupational Therapy Evaluation","Encounter","Encounter, Performed","CMS138" },
			new[] { "2.16.840.1.113883.3.526.3.1022","Physical Therapy Evaluation","Encounter","Encounter, Performed","CMS138" },
			new[] { "2.16.840.1.113883.3.526.3.1141","Psychoanalysis","Encounter","Encounter, Performed","CMS138" },
			new[] { "2.16.840.1.113883.3.526.3.1170","Tobacco User","Attribute","Attribute: result","CMS138" },
			new[] { "2.16.840.1.113883.3.526.3.1189","Tobacco Non User","Attribute","Attribute: result","CMS138" },
			new[] { "2.16.840.1.113883.3.526.3.1190","Tobacco Use Cessation Pharmacotherapy","Medication","Medication, Active; Medication, Order","CMS138" },
			new[] { "2.16.840.1.113883.3.526.3.1240","Annual Wellness Visit","Encounter","Encounter, Performed","CMS138,CMS165" },
			new[] { "2.16.840.1.113883.3.526.3.1278","Tobacco Use Screening","Assessment","Assessment, Performed","CMS138" },
			new[] { "2.16.840.1.113883.3.526.3.1285","Ophthalmological Services","Encounter","Encounter, Performed","CMS138" },
			new[] { "2.16.840.1.113883.3.526.3.1492","Psych Visit Diagnostic Evaluation","Encounter","Encounter, Performed","CMS138" },
			new[] { "2.16.840.1.113883.3.526.3.1496","Psych Visit Psychotherapy","Encounter","Encounter, Performed","CMS138" },
			new[] { "2.16.840.1.113883.3.526.3.1530","Speech and Hearing Evaluation","Encounter","Encounter, Performed","CMS138" },
			new[] { "2.16.840.1.113883.3.526.3.1561","Medications for Above Normal BMI","Medication","Medication, Active; Medication, Not Ordered; Medication, Order","CMS69" },
			new[] { "2.16.840.1.113883.3.526.3.1562","Medications for Below Normal BMI","Medication","Medication, Active; Medication, Not Ordered; Medication, Order","CMS69" },
			new[] { "2.16.840.1.113883.3.526.3.1563","Underweight","Condition/Diagnosis/Problem","Diagnosis; Attribute: reason","CMS69" },
			new[] { "2.16.840.1.113883.3.526.3.1582","Patient Declined","Attribute","Attribute: negationRationale","CMS69" },
			new[] { "2.16.840.1.113883.3.526.3.1584","Hospice Care Ambulatory","Intervention","Intervention, Order; Intervention, Performed","CMS69,CMS74,CMS75,CMS138,CMS155,CMS165" },
			new[] { "2.16.840.1.113883.3.600.1.1525","Follow Up for Above Normal BMI","Intervention","Intervention, Not Ordered; Intervention, Not Performed; Intervention, Order; Intervention, Performed","CMS69" },
			new[] { "2.16.840.1.113883.3.600.1.1527","Referrals Where Weight Assessment May Occur","Intervention","Intervention, Not Ordered; Intervention, Order","CMS69" },
			new[] { "2.16.840.1.113883.3.600.1.1528","Follow Up for Below Normal BMI","Intervention","Intervention, Not Ordered; Intervention, Not Performed; Intervention, Order; Intervention, Performed","CMS69" },
			new[] { "2.16.840.1.113883.3.600.1.1751","Encounter to Evaluate BMI","Encounter","Encounter, Performed","CMS69" },
			new[] { "2.16.840.1.113883.3.600.1.1834","Encounter to Document Medications","Encounter","Encounter, Performed","CMS68" },
			new[] { "2.16.840.1.113883.3.666.5.307","Encounter Inpatient","Encounter","Encounter, Performed","CMS69,CMS74,CMS75,CMS138,CMS155,CMS165" },
			new[] { "2.16.840.1.114222.4.11.836","Race","Patient Characteristic","Patient Characteristic Race","CMS68,CMS69,CMS74,CMS75,CMS138,CMS155,CMS165" },
			new[] { "2.16.840.1.114222.4.11.837","Ethnicity","Patient Characteristic","Patient Characteristic Ethnicity","CMS68,CMS69,CMS74,CMS75,CMS138,CMS155,CMS165" },
			new[] { "2.16.840.1.114222.4.11.3591","Payer Type","Patient Characteristic","Patient Characteristic Payer","CMS68,CMS69,CMS74,CMS75,CMS138,CMS155,CMS165" },
		};

		///<summary>The value sets CQMCommonQDM 9.0.000 declares for its hospital functions, which none of the seven measures calls: declared but
		///unused, so they are not in the catalog's value sets (section 7.2). Listed for the record (CatalogTests).</summary>
		private static readonly string[][] _arrayValueSetsUnused=new string[][] {
			new[] { "2.16.840.1.113762.1.4.1029.206","Intensive Care Unit" },
			new[] { "2.16.840.1.113762.1.4.1110.38","Outpatient Surgery Service" },
			new[] { "2.16.840.1.113762.1.4.1111.143","Observation Services" },
			new[] { "2.16.840.1.113883.3.117.1.7.1.292","Emergency Department Visit" },
		};

		///<summary>The value sets the data-entry windows pick from (section 8): OID and what the pick list is for. Each is a value set of a measure
		///too, so one import fills them. The windows name them by the constants above (EHR-7).</summary>
		private static readonly string[][] _arrayPickLists=new string[][] {
			new[] { OID_BMI_FOLLOW_UP_ABOVE,"Interventions: follow-up for an above-normal BMI" },
			new[] { OID_BMI_FOLLOW_UP_BELOW,"Interventions: follow-up for a below-normal BMI" },
			new[] { OID_BMI_REFERRALS,"Interventions: referrals where weight assessment may occur" },
			new[] { OID_BMI_MEDICATIONS_ABOVE,"Interventions and vital signs: medications for an above-normal BMI" },
			new[] { OID_BMI_MEDICATIONS_BELOW,"Interventions and vital signs: medications for a below-normal BMI" },
			new[] { OID_COUNSELING_NUTRITION,"Interventions: counseling for nutrition" },
			new[] { OID_COUNSELING_PHYSICAL_ACTIVITY,"Interventions: counseling for physical activity" },
			new[] { OID_TOBACCO_COUNSELING,"Tobacco use: cessation counseling" },
			new[] { OID_TOBACCO_PHARMACOTHERAPY,"Tobacco use: cessation pharmacotherapy" },
			new[] { OID_DIALYSIS_SERVICES,"Interventions: dialysis services" },
			new[] { OID_TOBACCO_SCREENING,"Tobacco use: screening" },
			new[] { OID_TOBACCO_USER,"Tobacco use: tobacco user" },
			new[] { OID_TOBACCO_NON_USER,"Tobacco use: tobacco non-user" },
			new[] { OID_PREGNANCY,"Problems: pregnancy" },
			new[] { OID_REASON_ACUTE_HEALTH_CRISIS,"Not performed: current medications, reason (acute health crisis)" },
			new[] { OID_REASON_MEDICAL,"Not performed: BMI exam and BMI follow-up, reason (medical reason)" },
			new[] { OID_REASON_PATIENT_DECLINED,"Not performed: BMI exam, reason (patient declined)" },
		};

		///<summary>The direct-reference-code roles (section 7.3): which code of the office's Direct Reference Codes file the engine, the data-entry
		///windows and QRDA need, found by measure and code system, and for LOINC also by the display name the specifications print (E-23). Never a
		///code. Columns: role, CMS ID, code system (the code list's name), the number of codes the role takes, the LOINC display name (or ""), and
		///what the role is for.</summary>
		private static readonly string[][] _arrayDirectCodeRoles=new string[][] {
			new[] { ROLE_CMS68_MEDS_DOCUMENTED,"CMS68","SNOMEDCT","1","","The documentation of current medications: CMS68's numerator and exception; what the medical window writes and filters by" },
			new[] { ROLE_CMS69_BMI,"CMS69","LOINC","1","Body mass index (BMI) [Ratio]","The BMI exam: the not-performed pick list and labels; QRDA I" },
			new[] { "Cms69Virtual","CMS69","ActCode","1","","The encounter class of a virtual encounter: CMS69's \"not virtual\" test" },
			new[] { "Cms138QualifyingVisitCpt","CMS138","CPT","4","","CMS138's qualifying visits by CPT code (every CPT direct reference code of CMS138)" },
			new[] { "Cms138CounselingDx","CMS138","ICD10CM","1","","Tobacco cessation counseling recorded as a diagnosis: CMS138's numerator 2" },
			new[] { "Cms165Systolic","CMS165","LOINC","1","Systolic blood pressure","The systolic blood pressure reading: QRDA I" },
			new[] { "Cms165Diastolic","CMS165","LOINC","1","Diastolic blood pressure","The diastolic blood pressure reading: QRDA I" },
			new[] { "Cms165KidneyTransplantDx","CMS165","ICD10CM","1","","Kidney transplant status as a diagnosis: a CMS165 exclusion" },
		};

		///<summary>The seven measures, in the order the program lists them.</summary>
		private static CqmMeasure[] MakeMeasures() {
			return new CqmMeasure[] {
				MakeMeasure("CMS68","16.0.000","CMS68DocumentationCurrentMeds","Documentation of Current Medications in the Medical Record",STEWARD_CMS,DEVELOPER_AIR,
					"9a032d9c-3d9b-11e1-8634-00237d5bf174","a58f1abe-de03-4ea2-a3ec-48d2a06b3bc5",isEpisodeBased:true,"Process",
					"<p>Higher score indicates better quality</p>",_copyrightCms68,_disclaimerCms68,
					Group(1,"b66641e9-b1bc-4024-9fbc-eccb89c4a7c4",Strata(),
						Pop("IPOP","F63CAC5B-9592-4E1E-82EE-739A66D86CC9","Initial Population"),
						Pop("DENOM","0B529CD7-00FB-4CAE-8B1F-B31DE4BB7E40","Denominator"),
						Pop("NUMER","C2A96F40-F8F6-47B3-AEE5-157F101D3E6E","Numerator"),
						Pop("DENEXCEP","56553A64-494A-472D-905E-78D1CB7A011A","Denominator Exceptions"))
				),
				MakeMeasure("CMS69","15.0.000","CMS69BMIScreenAndFollowUp","Preventive Care and Screening: Body Mass Index (BMI) Screening and Follow Up Plan",
					STEWARD_CMS,DEVELOPER_AIR,"9a031bb8-3d9b-11e1-8634-00237d5bf174","d62e8d03-9687-4097-8351-1a2bad79c9b9",isEpisodeBased:false,"Process",
					"<p>Higher score indicates better quality</p>",_copyrightCms69,_disclaimerCms69,
					Group(1,"a07a9ccb-30d7-4099-be55-97039ce523f0",Strata(),
						Pop("IPOP","5A321168-A454-4BD9-B82E-A403B009DE8F","Initial Population"),
						Pop("DENOM","7980B172-09ED-4A0C-B29C-D652DB845BF5","Denominator"),
						Pop("DENEX","50128A04-F2DD-4B6D-A078-B16904431CEC","Denominator Exclusions"),
						Pop("NUMER","5AF09C9B-07A3-4BF4-A366-7AEA9C934050","Numerator"),
						Pop("DENEXCEP","5509373E-4ACA-4B71-8E56-E42BBB1A3880","Denominator Exceptions"))
				),
				MakeMeasure("CMS74","16.0.000","CMS74DentalCariesPrevention","Primary Caries Prevention Intervention as Offered by Dentists",STEWARD_CMS,DEVELOPER_AIR,
					"0b81b6ba-3b30-41bf-a2f3-95bdc9f558f2","a79e99a5-8583-4041-8fa4-30c2d98c1d8a",isEpisodeBased:false,"Process",
					"<p>A higher score indicates better quality</p>",_copyrightCms74,_disclaimerCms74,
					Group(1,"baaa3b5a-672a-4ec7-b768-904f0a9bba12",Strata(Stratum("02B0863D-66C9-4021-9B6D-FF10C556B9E3","Stratification 1","Patients age 1-5 years at the start of the Measurement Period"),Stratum("C752E176-569A-4D6E-9F28-1E86B6E21B23","Stratification 2","Patients age 6-12 years at the start of the Measurement Period"),Stratum("A21F8CB1-0BA7-4133-9B43-FDA458BF7DC3","Stratification 3","Patients age 13-20 years at the start of the Measurement Period")),
						Pop("IPOP","4A7CD43D-16CD-4CF6-9AE4-41DFDAE5AA15","Initial Population"),
						Pop("DENOM","F2F65A2B-93E7-43A0-B43D-4A4AAD833387","Denominator"),
						Pop("DENEX","C680F977-E081-4CCF-95EA-E47A06E4006C","Denominator Exclusions"),
						Pop("NUMER","08A02549-B703-48B3-9548-DACBA6E63ABD","Numerator"))
				),
				MakeMeasure("CMS75","15.0.000","CMS75ChildrenDentalDecay","Children Who Have Dental Decay or Cavities",STEWARD_CMS,DEVELOPER_AIR,
					"61947125-4376-4a7b-ab7a-ac2be9bd9138","1e4f85db-b6c0-43d8-9d2b-594960bc5ac6",isEpisodeBased:false,"Outcome",
					"<p>Lower score indicates better quality</p>",_copyrightCms75,_disclaimerCms74,
					Group(1,"689bb6b7-4026-4855-8a53-ebe569a2c381",Strata(),
						Pop("IPOP","DBD0EEC9-8373-48E8-BCDA-579441BD5004","Initial Population"),
						Pop("DENOM","8D71CDFB-3183-45E1-B884-5E6A430C2171","Denominator"),
						Pop("DENEX","2C15DE33-98F6-48B6-A301-36666E235F2E","Denominator Exclusions"),
						Pop("NUMER","C0E870C1-BA44-40D3-9ADD-C2D04503A404","Numerator"))
				),
				MakeMeasure("CMS138","15.0.000","CMS138PreventiveTobaccoCessation","Preventive Care and Screening: Tobacco Use: Screening and Cessation Intervention",
					STEWARD_NCQA,STEWARD_NCQA,"e35791df-5b25-41bb-b260-673337bc44a8","f8fd38f2-e26c-48ad-a920-eff52f483956",isEpisodeBased:false,"Process",
					"<p>Higher score indicates better quality</p>",_copyrightCms138,_disclaimerCms138,
					Group(1,"0a95505a-9657-44f2-bf5b-d67650c4a408",Strata(),
						Pop("IPOP","AB01AB8C-5200-4C98-94B1-785687A9469F","Initial Population"),
						Pop("DENOM","C13FB76B-61C6-422F-92ED-71D8FFE685FC","Denominator 1"),
						Pop("DENEX","D80F9855-5B82-4EAB-97E7-48F7E5E24510","Denominator Exclusion"),
						Pop("NUMER","E3DA4ECF-8EE9-4578-B3EB-D269973F8EFB","Numerator 1")),
					Group(2,"0bedd8d2-ab79-48b8-a882-b453e22df6c1",Strata(),
						Pop("IPOP","C508583F-77CE-4004-AC69-13A84B2641D3","Initial Population"),
						Pop("DENOM","5D9336BB-1759-4FB0-ACAD-C944F28ACD10","Denominator 2"),
						Pop("DENEX","D323558D-2C59-476C-A6B9-32E5BCF5D2E8","Denominator Exclusion"),
						Pop("NUMER","21D4794A-1450-4C3D-89AE-A4791DF8FA2A","Numerator 2")),
					Group(3,"eec63bf3-4a7a-43e3-83f5-d2a40ecad474",Strata(),
						Pop("IPOP","D9BEF0F0-26B4-484C-AF92-252622D46734","Initial Population"),
						Pop("DENOM","5455BCAD-B7A1-4F2E-85FF-EAE0881B7666","Denominator 3"),
						Pop("DENEX","64F71C4C-5F42-479C-9EDF-968EBCCFEDE7","Denominator Exclusion"),
						Pop("NUMER","333A53EF-9CFF-4AC2-9F71-BA7010DBA9ED","Numerator 3"))
				),
				MakeMeasure("CMS155","15.0.000","CMS155WeightAssessandCounselforKids",
					"Weight Assessment and Counseling for Nutrition and Physical Activity for Children/Adolescents",STEWARD_NCQA,STEWARD_NCQA,
					"0b63f730-25d6-4248-b11f-8c09c66a04eb","63b284d2-a9b2-4f6d-86f9-40650e49a17b",isEpisodeBased:false,"Process",
					"<p>Higher score indicates better quality</p>",_copyrightCms155,_disclaimerCms138,
					Group(1,"02fcbd36-f17e-4aea-98c1-637a5a8ea040",Strata(Stratum("3816ADA1-1322-44F1-B51F-56D6B0EAE697","Stratification 1","Patients age 3-11 years at the end of the measurement period"),Stratum("C8DDE7EE-387F-4D37-A421-E5EA96A5FDE1","Stratification 2","Patients age 12-17 years at the end of the measurement period")),
						Pop("IPOP","E5101676-A633-4A07-BBD3-10EA995CC8C4","Initial Population"),
						Pop("DENOM","DA528032-CE05-4DA3-9DBB-95E0541336A0","Denominator"),
						Pop("DENEX","7D74CEF9-F64D-4E9D-AB7B-27D82769F5B0","Denominator Exclusions"),
						Pop("NUMER","84F42E8A-2547-4019-82FA-757BAED7E44D","Numerator 1")),
					Group(2,"cee71442-ff8d-4960-969e-e7a17cda7ee6",Strata(Stratum("EDF53EA7-004F-4E52-8934-76BA727BB48D","Stratification 1","Patients age 3-11 years at the end of the measurement period"),Stratum("B8A8B540-E1DE-4CC6-BB12-DD615A26AF0B","Stratification 2","Patients age 12-17 years at the end of the measurement period")),
						Pop("IPOP","D839E368-D010-448D-9978-67B90E5884A2","Initial Population"),
						Pop("DENOM","98C0D643-22F0-4711-8C83-49A304CD5A33","Denominator"),
						Pop("DENEX","AEE993E6-DAF2-41AD-88CD-82C379686716","Denominator Exclusions"),
						Pop("NUMER","925AC5D2-BC98-4B64-BC8D-1584E091E047","Numerator 2")),
					Group(3,"187ded18-dd98-419f-945b-8199543cfa5c",Strata(Stratum("A35008B6-4328-4E0E-838D-C5C1BBF5ECF7","Stratification 1","Patients age 3-11 years at the end of the measurement period"),Stratum("957A6BCF-B1AF-4C03-A763-DD0335696C49","Stratification 2","Patients age 12-17 years at the end of the measurement period")),
						Pop("IPOP","66E4AC14-3CD7-415C-9743-3242866D2DAB","Initial Population"),
						Pop("DENOM","9B7E86ED-146E-4C79-AEDE-8B6C6BE57DC7","Denominator"),
						Pop("DENEX","E35E98C5-0770-49BB-807D-58C851D3A6F6","Denominator Exclusions"),
						Pop("NUMER","2CFDE036-06CE-41B4-92A3-EE3B467883AD","Numerator 3"))
				),
				MakeMeasure("CMS165","15.0.000","CMS165ControllingHighBloodPressure","Controlling High Blood Pressure",STEWARD_NCQA,STEWARD_NCQA,
					"abdc37cc-bac6-4156-9b91-d1be2c8b7268","b83d59fa-65cd-4ccb-9889-70fbc81bcc6e",isEpisodeBased:false,"Intermediate Outcome",
					"<p>Higher score indicates better quality</p>",_copyrightCms155,_disclaimerCms138,
					Group(1,"893a9c04-59af-4fd3-b3fd-889d22142e91",Strata(),
						Pop("IPOP","A35D89C5-C903-4D4C-BDBC-EA70D1254BEF","Initial Population"),
						Pop("DENOM","FECE5EB1-842C-42B6-B2BC-7035C79222E4","Denominator"),
						Pop("DENEX","8D42520C-8C19-47AF-B3E7-A66EAECA0DBD","Denominator Exclusions"),
						Pop("NUMER","73661F57-1A94-4982-8136-EDCE51A9AEA8","Numerator"))
				),
			};
		}

		#region Building the measures
		private static CqmMeasure MakeMeasure(string cmsId,string versionNumber,string libraryName,string title,string steward,string developer,string setId,
			string versionSpecificId,bool isEpisodeBased,string measureType,string improvementNotation,string copyright,string disclaimer,
			params CqmPopulationGroup[] arrayGroups)
		{
			CqmMeasure measure=new CqmMeasure();
			measure.CmsId=cmsId;
			measure.VersionNumber=versionNumber;
			measure.Version=int.Parse(versionNumber.Substring(0,versionNumber.IndexOf('.')),CultureInfo.InvariantCulture);
			measure.LibraryName=libraryName;
			measure.Title=title;
			measure.Steward=steward;
			measure.Developer=developer;
			measure.SetId=setId;
			measure.VersionSpecificId=versionSpecificId;
			measure.IsEpisodeBased=isEpisodeBased;
			measure.Scoring="Proportion";
			measure.MeasureType=measureType;
			measure.ImprovementNotation=improvementNotation;
			measure.IsLowerBetter=improvementNotation.Contains("Lower score");
			measure.Copyright=copyright;
			measure.Disclaimer=disclaimer;
			measure.ListPopulationGroups=arrayGroups.ToList();
			measure.ListValueSetOids=_arrayValueSets.Where(x => x[4].Split(',').Contains(cmsId)).Select(x => x[0]).ToList();
			measure.ListDirectCodeRoles=_arrayDirectCodeRoles.Where(x => x[1]==cmsId).Select(x => x[0]).ToList();
			return measure;
		}

		private static CqmPopulationGroup Group(int number,string id,CqmStratum[] arrayStrata,params CqmPopulation[] arrayPopulations) {
			CqmPopulationGroup group=new CqmPopulationGroup();
			group.Number=number;
			group.Id=id;
			group.ListPopulations=arrayPopulations.ToList();
			group.ListStrata=arrayStrata.ToList();
			return group;
		}

		private static CqmStratum[] Strata(params CqmStratum[] arrayStrata) {
			return arrayStrata;
		}

		private static CqmPopulation Pop(string code,string id,string define) {
			return new CqmPopulation { Code=code,Id=id,Define=define };
		}

		private static CqmStratum Stratum(string id,string define,string description) {
			return new CqmStratum { Id=id,Define=define,Description=description };
		}
		#endregion Building the measures

		#region The catalog
		///<summary>The seven measures (new objects each call), in the order the program lists them: CMS68, CMS69, CMS74, CMS75, CMS138, CMS155,
		///CMS165.</summary>
		public static List<CqmMeasure> GetMeasures() {
			Meth.NoCheckMiddleTierRole();
			return MakeMeasures().ToList();
		}

		///<summary>The measure with this CMS ID, with or without its version ("CMS74" or "CMS74v16"; case ignored), or null. A version other than
		///the catalog's gives null.</summary>
		public static CqmMeasure GetMeasure(string cmsId) {
			Meth.NoCheckMiddleTierRole();
			if(string.IsNullOrWhiteSpace(cmsId)) {
				return null;
			}
			string text=cmsId.Trim();
			return MakeMeasures().FirstOrDefault(x => string.Equals(x.CmsId,text,StringComparison.OrdinalIgnoreCase)
				|| string.Equals(x.MeasureId,text,StringComparison.OrdinalIgnoreCase));
		}

		///<summary>The measures' IDs at their catalog versions, as VSAC's files name them: CMS68v16, CMS69v15, CMS74v16, CMS75v15, CMS138v15,
		///CMS155v15, CMS165v15.</summary>
		public static List<string> GetMeasureIds() {
			Meth.NoCheckMiddleTierRole();
			return MakeMeasures().Select(x => x.MeasureId).ToList();
		}

		///<summary>The value sets of the seven measures (new objects each call), ordered by OID. IsPickList is set for the value sets of the
		///data-entry windows.</summary>
		public static List<CqmValueSet> GetValueSets() {
			Meth.NoCheckMiddleTierRole();
			Dictionary<string,string> dictPickLists=_arrayPickLists.ToDictionary(x => x[0],x => x[1]);
			List<CqmValueSet> listValueSets=new List<CqmValueSet>();
			foreach(string[] row in _arrayValueSets) {
				CqmValueSet valueSet=new CqmValueSet();
				valueSet.Oid=row[0];
				valueSet.Name=row[1];
				valueSet.QDMCategory=row[2];
				valueSet.Datatypes=row[3];
				valueSet.ListCmsIds=row[4].Split(',').ToList();
				valueSet.IsPickList=dictPickLists.TryGetValue(row[0],out valueSet.PickListUse);
				valueSet.PickListUse=valueSet.PickListUse??"";
				listValueSets.Add(valueSet);
			}
			return listValueSets;
		}

		///<summary>The catalog's value set with this OID, or null.</summary>
		public static CqmValueSet GetValueSet(string oid) {
			Meth.NoCheckMiddleTierRole();
			return GetValueSets().FirstOrDefault(x => x.Oid==oid);
		}

		///<summary>The value sets the data-entry windows pick from (section 8), in the order of the windows.</summary>
		public static List<CqmValueSet> GetPickListValueSets() {
			Meth.NoCheckMiddleTierRole();
			List<CqmValueSet> listValueSets=GetValueSets();
			return _arrayPickLists.Select(x => listValueSets.First(y => y.Oid==x[0])).ToList();
		}

		///<summary>The qualifying-encounter value sets (E-32), in the catalog's order of OIDs: the Encounter value sets the measures count a visit
		///by, without those that only exclude a patient. The recommended default encounter codes are their SNOMED CT codes.</summary>
		public static List<CqmValueSet> GetQualifyingEncounterValueSets() {
			Meth.NoCheckMiddleTierRole();
			return GetValueSets().Where(x => _arrayQualifyingEncounterOids.Contains(x.Oid)).ToList();
		}

		///<summary>The OIDs and names CQMCommonQDM declares for its hospital functions, which no measure calls (not in GetValueSets).</summary>
		public static List<KeyValuePair<string,string>> GetValueSetsDeclaredButUnused() {
			Meth.NoCheckMiddleTierRole();
			return _arrayValueSetsUnused.Select(x => new KeyValuePair<string,string>(x[0],x[1])).ToList();
		}

		///<summary>The direct-reference-code roles (new objects each call), in the order of section 7.3.</summary>
		public static List<CqmDirectCodeRole> GetDirectCodeRoles() {
			Meth.NoCheckMiddleTierRole();
			return _arrayDirectCodeRoles.Select(x => new CqmDirectCodeRole {
				Role=x[0],
				CmsId=x[1],
				CodeSystem=x[2],
				CountExpected=int.Parse(x[3],CultureInfo.InvariantCulture),
				DisplayName=x[4],
				Meaning=x[5],
			}).ToList();
		}

		///<summary>The role, or null.</summary>
		public static CqmDirectCodeRole GetDirectCodeRole(string role) {
			Meth.NoCheckMiddleTierRole();
			return GetDirectCodeRoles().FirstOrDefault(x => x.Role==role);
		}

		///<summary>The ehrcode.ValueSetOID of a direct reference code: drc:CMS69:Cms69Bmi, or drc:CMS69:Listed for a code with no role. The CMS ID
		///is without its version, so the engine's key does not change with the yearly update.</summary>
		public static string GetDirectCodeKey(string cmsId,string role) {
			Meth.NoCheckMiddleTierRole();
			return DIRECT_CODE_KEY_PREFIX+cmsId+":"+(string.IsNullOrEmpty(role) ? DIRECT_CODE_ROLE_LISTED : role);
		}

		///<summary>True for an ehrcode.ValueSetOID that is a direct-reference-code key (GetDirectCodeKey), not a value set.</summary>
		public static bool IsDirectCodeKey(string valueSetOID) {
			Meth.NoCheckMiddleTierRole();
			return valueSetOID!=null && valueSetOID.StartsWith(DIRECT_CODE_KEY_PREFIX,StringComparison.Ordinal);
		}
		#endregion The catalog

		#region The statements
		///<summary>A statement (HTML paragraphs, as the HQMF file holds it) as plain text: each paragraph on its own, separated by an empty line, HTML
		///entities decoded (a no-break space as a space), white space inside a paragraph collapsed. The statement itself is never changed; this is
		///how a window or a print-out that cannot show HTML shows it.</summary>
		public static string GetStatementText(string statementHtml) {
			Meth.NoCheckMiddleTierRole();
			if(string.IsNullOrEmpty(statementHtml)) {
				return "";
			}
			List<string> listParagraphs=new List<string>();
			foreach(string part in Regex.Split(statementHtml,@"</p\s*>|<p(?:\s[^>]*)?>|<br\s*/?>",RegexOptions.IgnoreCase)) {
				string text=Regex.Replace(part,@"<[^>]*>","");//Any other tag.
				text=WebUtility.HtmlDecode(text).Replace((char)0xA0,' ').Replace((char)0x202F,' ');
				text=Regex.Replace(text,@"\s+"," ").Trim();
				if(text!="") {
					listParagraphs.Add(text);
				}
			}
			return string.Join("\r\n\r\n",listParagraphs);
		}

		///<summary>The SHA-256 of the text in UTF-8, lower-case hex: how the port note records each statement, so an edit of one shows.</summary>
		public static string GetSha256(string text) {
			Meth.NoCheckMiddleTierRole();
			using(SHA256 sha256=SHA256.Create()) {
				return string.Concat(sha256.ComputeHash(Encoding.UTF8.GetBytes(text??"")).Select(x => x.ToString("x2",CultureInfo.InvariantCulture)));
			}
		}
		#endregion The statements
	}

	///<summary>A measure of the catalog (CqmCatalog). Public fields, so it serializes as the Middle Tier sends objects.</summary>
	public class CqmMeasure {
		///<summary>Without the version: CMS74.</summary>
		public string CmsId="";
		///<summary>The major version: 16.</summary>
		public int Version;
		///<summary>As the HQMF file gives it: 16.0.000.</summary>
		public string VersionNumber="";
		///<summary>The CQL library of the measure: CMS74DentalCariesPrevention.</summary>
		public string LibraryName="";
		public string Title="";
		///<summary>Who owns and stewards the measure (the HQMF file's custodian).</summary>
		public string Steward="";
		///<summary>Who developed it (the HQMF file's author).</summary>
		public string Developer="";
		///<summary>The HQMF set ID, the same in every version (the 2014 engine's version-neutral ID).</summary>
		public string SetId="";
		///<summary>The HQMF ID of this version.</summary>
		public string VersionSpecificId="";
		///<summary>True when each member is an encounter (CMS68), false when it is a patient.</summary>
		public bool IsEpisodeBased;
		///<summary>Proportion, for all seven.</summary>
		public string Scoring="";
		///<summary>Process, Outcome or Intermediate Outcome.</summary>
		public string MeasureType="";
		///<summary>As the HQMF file gives it (an HTML paragraph).</summary>
		public string ImprovementNotation="";
		///<summary>True when a lower rate is better (CMS75).</summary>
		public bool IsLowerBetter;
		///<summary>The copyright statement of the HQMF file, verbatim (HTML paragraphs). Shown, printed and written to QRDA with the measure (section
		///11.1). CqmCatalog.GetStatementText gives it as plain text.</summary>
		public string Copyright="";
		///<summary>The disclaimer statement of the HQMF file, verbatim, as Copyright.</summary>
		public string Disclaimer="";
		public List<CqmPopulationGroup> ListPopulationGroups=new List<CqmPopulationGroup>();
		///<summary>The OIDs of the value sets the measure's CQL declares (outside CQMCommonQDM), ordered by OID.</summary>
		public List<string> ListValueSetOids=new List<string>();
		///<summary>The roles of the measure's direct reference codes that the program reads.</summary>
		public List<string> ListDirectCodeRoles=new List<string>();

		///<summary>CMS74v16, as VSAC's files name a measure.</summary>
		public string MeasureId {
			get {
				return CmsId+"v"+Version.ToString(CultureInfo.InvariantCulture);
			}
		}

		///<summary>74v16, as EhrCode.MeasureIds writes it.</summary>
		public string MeasureIdShort {
			get {
				return MeasureId.Substring(3);
			}
		}

		///<summary>True for the three NCQA measures (CMS138, CMS155, CMS165).</summary>
		public bool IsNcqa {
			get {
				return Steward==CqmCatalog.STEWARD_NCQA;
			}
		}
	}

	///<summary>A population group of a measure (an HQMF population criteria section): CMS138 and CMS155 have three, the others one.</summary>
	public class CqmPopulationGroup {
		///<summary>1, 2 or 3, in the HQMF file's order.</summary>
		public int Number;
		///<summary>The HQMF ID of the population criteria section.</summary>
		public string Id="";
		public List<CqmPopulation> ListPopulations=new List<CqmPopulation>();
		///<summary>The strata of the group (CMS74: three, CMS155: two), each with its own HQMF ID.</summary>
		public List<CqmStratum> ListStrata=new List<CqmStratum>();
	}

	///<summary>A population of a group, with the HQMF ID that QRDA III reports it by.</summary>
	public class CqmPopulation {
		///<summary>IPOP, DENOM, DENEX, NUMER or DENEXCEP (HL7 ActCode, as the HQMF file names them).</summary>
		public string Code="";
		public string Id="";
		///<summary>The CQL define the population is, for example "Numerator 2".</summary>
		public string Define="";
	}

	///<summary>A stratum of a group.</summary>
	public class CqmStratum {
		public string Id="";
		///<summary>The CQL define: "Stratification 1".</summary>
		public string Define="";
		///<summary>Who is in it, as the HQMF file's stratification text says.</summary>
		public string Description="";
	}

	///<summary>A value set of the catalog. No code of it is in the program.</summary>
	public class CqmValueSet {
		public string Oid="";
		public string Name="";
		///<summary>The QDM category, for example Encounter or Condition/Diagnosis/Problem; Attribute for a value set the CQL reads only as an
		///attribute (a negation rationale or a result).</summary>
		public string QDMCategory="";
		///<summary>The QDM datatypes and attributes the CQL reads it as, separated by "; ".</summary>
		public string Datatypes="";
		///<summary>The measures that declare it, without versions: CMS69, CMS165.</summary>
		public List<string> ListCmsIds=new List<string>();
		///<summary>True when a data-entry window picks from it (section 8).</summary>
		public bool IsPickList;
		///<summary>What the pick list is for, or "".</summary>
		public string PickListUse="";
	}

	///<summary>A direct reference code the program needs, by role (section 7.3). The importer finds its row in the office's Direct Reference Codes
	///file: the codes of the measure in the code system (exactly CountExpected of them), and for LOINC the one whose display name matches.</summary>
	public class CqmDirectCodeRole {
		public string Role="";
		///<summary>Without the version: CMS69.</summary>
		public string CmsId="";
		///<summary>The code list's name of the code system: SNOMEDCT, LOINC, ActCode, CPT or ICD10CM.</summary>
		public string CodeSystem="";
		///<summary>How many codes the role takes: 4 for CMS138's CPT visits, 1 for the others.</summary>
		public int CountExpected;
		///<summary>For LOINC only: the display name the specifications print (E-23). "" for the others, which are the only codes of their code
		///system in their measure.</summary>
		public string DisplayName="";
		///<summary>What the role is for.</summary>
		public string Meaning="";
	}
}
