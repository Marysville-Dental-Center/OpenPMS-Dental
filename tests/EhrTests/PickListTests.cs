// The data-entry windows on the 2027 catalog (docs/specs/ehr-2027-measures-plan.md, section 8, package EHR-7), with no database: the catalog's
// constants and qualifying-encounter value sets, the direct code of a role, the not-performed items (their roles, value sets and reasons, the
// item of a saved row from its code, "other" for an unknown one, the fifth item of E-34), the intervention window's code-set lists and its
// medication labels, the recommended SNOMED CT codes picked from the value-set rows, and source checks of the windows (the grid's Tag, no
// Enum.Parse, the read-only "other" row, the role's code in the medical window, no OID or SNOMED CT literal left). Every code is a made-up TEST
// code; the value-set rows are built in memory. PickListDbTests fills the same lists from an imported synthetic release.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalBusiness;

namespace EhrTests {
	[TestClass]
	public class PickListTests {
		private const string PATH_NOT_PERFORMED=@"OpenPmsDental\Forms\FormEhrNotPerformed.cs";
		private const string PATH_NOT_PERFORMED_EDIT=@"OpenPmsDental\Forms\FormEhrNotPerformedEdit.cs";

		///<summary>A value-set row of the list, as the import writes it.</summary>
		private static EhrCode Row(string valueSetOID,string codeSystem,string codeValue,bool isInDb=true) {
			return new EhrCode { ValueSetOID=valueSetOID,CodeSystem=codeSystem,CodeValue=codeValue,Description="TEST description of "+codeValue,IsInDb=isInDb };
		}

		///<summary>The row of a direct reference code of a role, under its drc: key.</summary>
		private static EhrCode DirectRow(string role,string codeSystem,string codeValue) {
			CqmDirectCodeRole directCodeRole=CqmCatalog.GetDirectCodeRole(role);
			EhrCode ehrCode=Row(CqmCatalog.GetDirectCodeKey(directCodeRole.CmsId,role),codeSystem,codeValue);
			ehrCode.QDMCategory=CqmCatalog.DIRECT_CODE_CATEGORY;
			return ehrCode;
		}

		///<summary>A synthetic list: the two roles the windows read, one code in each pick list of the not-performed items and the medications.</summary>
		private static List<EhrCode> MakeList() {
			return new List<EhrCode> {
				DirectRow(CqmCatalog.ROLE_CMS69_BMI,"LOINC","TEST-L1"),
				DirectRow(CqmCatalog.ROLE_CMS68_MEDS_DOCUMENTED,"SNOMEDCT","TEST-S1"),
				Row(CqmCatalog.OID_TOBACCO_SCREENING,"LOINC","TEST-L2"),
				Row(CqmCatalog.OID_BMI_FOLLOW_UP_ABOVE,"SNOMEDCT","TEST-S2"),
				Row(CqmCatalog.OID_BMI_FOLLOW_UP_BELOW,"HCPCS","TEST-H1"),
				Row(CqmCatalog.OID_BMI_REFERRALS,"SNOMEDCT","TEST-S3"),
				Row(CqmCatalog.OID_BMI_MEDICATIONS_ABOVE,"RXNORM","TEST-R1"),
				Row(CqmCatalog.OID_BMI_MEDICATIONS_BELOW,"RXNORM","TEST-R2"),
				Row(CqmCatalog.OID_TOBACCO_PHARMACOTHERAPY,"RXNORM","TEST-R3"),
				Row(CqmCatalog.OID_TOBACCO_USER,"SNOMEDCT","TEST-S4"),
			};
		}

		#region The catalog's constants
		[TestMethod]
		public void Constants_AreTheCatalogsPickLists_EachOnce() {
			List<string> listOids=typeof(CqmCatalog).GetFields().Where(x => x.IsLiteral && x.Name.StartsWith("OID_")).Select(x => (string)x.GetValue(null)).ToList();
			Assert.AreEqual(17,listOids.Count,"One constant per pick list of section 8.");
			CollectionAssert.AreEquivalent(CqmCatalog.GetPickListValueSets().Select(x => x.Oid).ToList(),listOids);
			Assert.AreEqual("Cms68MedsDocumented",CqmCatalog.ROLE_CMS68_MEDS_DOCUMENTED);
			Assert.AreEqual("Cms69Bmi",CqmCatalog.ROLE_CMS69_BMI);
			Assert.IsNotNull(CqmCatalog.GetDirectCodeRole(CqmCatalog.ROLE_CMS68_MEDS_DOCUMENTED));
			Assert.IsNotNull(CqmCatalog.GetDirectCodeRole(CqmCatalog.ROLE_CMS69_BMI));
			Assert.AreEqual("Pregnancy",CqmCatalog.GetValueSet(CqmCatalog.OID_PREGNANCY).Name);
			Assert.AreEqual("Dialysis Services",CqmCatalog.GetValueSet(CqmCatalog.OID_DIALYSIS_SERVICES).Name);
			Assert.AreEqual("Medications for Above Normal BMI",CqmCatalog.GetValueSet(CqmCatalog.OID_BMI_MEDICATIONS_ABOVE).Name);
			Assert.AreEqual("Medications for Below Normal BMI",CqmCatalog.GetValueSet(CqmCatalog.OID_BMI_MEDICATIONS_BELOW).Name);
		}

		[TestMethod]
		public void QualifyingEncounters_AreTheMeasuresVisitSets_WithoutTheExclusions() {
			List<CqmValueSet> listQualifying=CqmCatalog.GetQualifyingEncounterValueSets();
			Assert.AreEqual(22,listQualifying.Count);
			Assert.IsTrue(listQualifying.All(x => x.QDMCategory=="Encounter" && x.Datatypes.Contains("Encounter, Performed")));
			List<string> listNames=listQualifying.Select(x => x.Name).ToList();
			foreach(string name in new[] { "Office Visit","Clinical Oral Evaluation","Encounter to Evaluate BMI","Encounter to Document Medications",
				"Telephone Visits","Virtual Encounter","Annual Wellness Visit","Home Healthcare Services" })
			{
				CollectionAssert.Contains(listNames,name);
			}
			foreach(string name in new[] { "Hospice Encounter","Palliative Care Encounter","Frailty Encounter","Encounter Inpatient",
				"Emergency Department Evaluation and Management Visit" })
			{
				CollectionAssert.DoesNotContain(listNames,name,"An Encounter value set that only excludes a patient.");
			}
			//Every Encounter value set of the catalog is one or the other.
			Assert.AreEqual(27,CqmCatalog.GetValueSets().Count(x => x.QDMCategory=="Encounter"));
		}
		#endregion The catalog's constants

		#region Direct reference codes
		[TestMethod]
		public void DirectCode_IsTheRolesOneCode_NoneWithoutTheImportOrWithTwo() {
			List<EhrCode> listEhrCodes=MakeList();
			EhrCode ehrCode=CqmDataEntry.GetDirectCode(CqmCatalog.ROLE_CMS68_MEDS_DOCUMENTED,listEhrCodes);
			Assert.AreEqual("TEST-S1",ehrCode.CodeValue);
			Assert.AreEqual("SNOMEDCT",ehrCode.CodeSystem);
			Assert.AreEqual("TEST-L1",CqmDataEntry.GetDirectCode(CqmCatalog.ROLE_CMS69_BMI,listEhrCodes).CodeValue);
			Assert.IsNull(CqmDataEntry.GetDirectCode(CqmCatalog.ROLE_CMS68_MEDS_DOCUMENTED,new List<EhrCode>()),"Before the import: no code.");
			Assert.IsNull(CqmDataEntry.GetDirectCode("Cms147Influenza",listEhrCodes),"No such role.");
			//The same code twice (it is listed under the measure once per row) is one code; two codes are none (the import warned).
			listEhrCodes.Add(DirectRow(CqmCatalog.ROLE_CMS68_MEDS_DOCUMENTED,"SNOMEDCT","TEST-S1"));
			Assert.AreEqual("TEST-S1",CqmDataEntry.GetDirectCode(CqmCatalog.ROLE_CMS68_MEDS_DOCUMENTED,listEhrCodes).CodeValue);
			listEhrCodes.Add(DirectRow(CqmCatalog.ROLE_CMS68_MEDS_DOCUMENTED,"SNOMEDCT","TEST-S9"));
			Assert.IsNull(CqmDataEntry.GetDirectCode(CqmCatalog.ROLE_CMS68_MEDS_DOCUMENTED,listEhrCodes));
			//A code of the value sets is never a role's code.
			Assert.IsNull(CqmDataEntry.GetDirectCode(CqmCatalog.ROLE_CMS69_BMI,new List<EhrCode> { Row(CqmCatalog.OID_TOBACCO_SCREENING,"LOINC","TEST-L1") }));
		}
		#endregion Direct reference codes

		#region Not performed
		[TestMethod]
		public void NotPerformedItem_HasAFifthValue4_TheItemsByPosition() {
			CollectionAssert.AreEqual(new[] { 0,1,2,3,4 },Enum.GetValues(typeof(EhrNotPerformedItem)).Cast<int>().ToList(),
				"The windows index the items by position, so the fork-only item is the next number (FORK-ENUMS.md).");
			Assert.AreEqual("BMIFollowUp",Enum.GetNames(typeof(EhrNotPerformedItem))[4]);
			Assert.AreEqual(4,(int)EhrNotPerformedItem.BMIFollowUp);
			string forkEnums=TestPaths.ReadSource(@"docs\port-notes\FORK-ENUMS.md");
			StringAssert.Contains(forkEnums,"EhrNotPerformedItem");
			StringAssert.Contains(forkEnums,"| EhrNotPerformedItem | 4     | BMIFollowUp");
		}

		[TestMethod]
		public void NotPerformed_EachItemsCodesAndReasons_AreTheCatalogs() {
			Assert.AreEqual(CqmCatalog.ROLE_CMS69_BMI,CqmDataEntry.GetNotPerformedRole(EhrNotPerformedItem.BMIExam));
			Assert.AreEqual(CqmCatalog.ROLE_CMS68_MEDS_DOCUMENTED,CqmDataEntry.GetNotPerformedRole(EhrNotPerformedItem.DocumentCurrentMeds));
			Assert.AreEqual("",CqmDataEntry.GetNotPerformedRole(EhrNotPerformedItem.TobaccoScreening));
			Assert.AreEqual("",CqmDataEntry.GetNotPerformedRole(EhrNotPerformedItem.InfluenzaVaccination));
			Assert.AreEqual("",CqmDataEntry.GetNotPerformedRole(EhrNotPerformedItem.BMIFollowUp));
			CollectionAssert.AreEqual(new[] { CqmCatalog.OID_TOBACCO_SCREENING },CqmDataEntry.GetNotPerformedValueSetOids(EhrNotPerformedItem.TobaccoScreening));
			CollectionAssert.AreEqual(new[] { CqmCatalog.OID_BMI_FOLLOW_UP_ABOVE,CqmCatalog.OID_BMI_FOLLOW_UP_BELOW,CqmCatalog.OID_BMI_REFERRALS,
				CqmCatalog.OID_BMI_MEDICATIONS_ABOVE,CqmCatalog.OID_BMI_MEDICATIONS_BELOW },CqmDataEntry.GetNotPerformedValueSetOids(EhrNotPerformedItem.BMIFollowUp),
				"E-34: the follow-up, referral and BMI medication sets.");
			Assert.AreEqual(0,CqmDataEntry.GetNotPerformedValueSetOids(EhrNotPerformedItem.InfluenzaVaccination).Count,"E-25: the CVX table.");
			Assert.AreEqual(0,CqmDataEntry.GetNotPerformedValueSetOids(EhrNotPerformedItem.BMIExam).Count);
			//The reasons (section 8).
			CollectionAssert.AreEqual(new[] { CqmCatalog.OID_REASON_MEDICAL,CqmCatalog.OID_REASON_PATIENT_DECLINED },
				CqmDataEntry.GetNotPerformedReasonOids(EhrNotPerformedItem.BMIExam,false));
			CollectionAssert.AreEqual(new[] { CqmCatalog.OID_REASON_ACUTE_HEALTH_CRISIS },CqmDataEntry.GetNotPerformedReasonOids(EhrNotPerformedItem.DocumentCurrentMeds,false));
			CollectionAssert.AreEqual(new[] { CqmCatalog.OID_REASON_MEDICAL },CqmDataEntry.GetNotPerformedReasonOids(EhrNotPerformedItem.BMIFollowUp,false),
				"E-34: Medical Reason only.");
			CollectionAssert.AreEqual(new[] { CqmCatalog.OID_REASON_MEDICAL },CqmDataEntry.GetNotPerformedReasonOids(EhrNotPerformedItem.TobaccoScreening,false));
			CollectionAssert.AreEqual(new[] { CqmCatalog.OID_REASON_MEDICAL },CqmDataEntry.GetNotPerformedReasonOids(EhrNotPerformedItem.InfluenzaVaccination,false));
			CollectionAssert.AreEqual(new[] { CqmCatalog.OID_REASON_PATIENT_DECLINED },CqmDataEntry.GetNotPerformedReasonOids(EhrNotPerformedItem.InfluenzaVaccination,true));
			//Every value set an item reads is a pick list of the catalog.
			List<string> listPickLists=CqmCatalog.GetPickListValueSets().Select(x => x.Oid).ToList();
			foreach(EhrNotPerformedItem item in Enum.GetValues(typeof(EhrNotPerformedItem))) {
				foreach(string oid in CqmDataEntry.GetNotPerformedValueSetOids(item).Concat(CqmDataEntry.GetNotPerformedReasonOids(item,false))
					.Concat(CqmDataEntry.GetNotPerformedReasonOids(item,true)))
				{
					CollectionAssert.Contains(listPickLists,oid,item.ToString());
				}
			}
		}

		[TestMethod]
		public void NotPerformedItem_OfASavedRow_ByRoleCatalogAndCvx_AnUnknownCodeIsOther() {
			List<EhrCode> listEhrCodes=MakeList();
			Assert.AreEqual(EhrNotPerformedItem.BMIExam,CqmDataEntry.GetNotPerformedItem("TEST-L1","LOINC",listEhrCodes));
			Assert.AreEqual(EhrNotPerformedItem.DocumentCurrentMeds,CqmDataEntry.GetNotPerformedItem("TEST-S1","SNOMEDCT",listEhrCodes));
			Assert.AreEqual(EhrNotPerformedItem.TobaccoScreening,CqmDataEntry.GetNotPerformedItem("TEST-L2","LOINC",listEhrCodes));
			Assert.AreEqual(EhrNotPerformedItem.BMIFollowUp,CqmDataEntry.GetNotPerformedItem("TEST-S2","SNOMEDCT",listEhrCodes));
			Assert.AreEqual(EhrNotPerformedItem.BMIFollowUp,CqmDataEntry.GetNotPerformedItem("TEST-H1","HCPCS",listEhrCodes));
			Assert.AreEqual(EhrNotPerformedItem.BMIFollowUp,CqmDataEntry.GetNotPerformedItem("TEST-S3","SNOMEDCT",listEhrCodes));
			Assert.AreEqual(EhrNotPerformedItem.BMIFollowUp,CqmDataEntry.GetNotPerformedItem("TEST-R2","RXNORM",listEhrCodes));
			Assert.AreEqual(EhrNotPerformedItem.InfluenzaVaccination,CqmDataEntry.GetNotPerformedItem("TEST-C1","CVX",listEhrCodes),"E-25: a CVX code.");
			//"other": a code of no item (once every other row was "influenza"), the right code in the wrong code system, a code of another value set,
			//no code. The item is null, never a guess.
			Assert.IsNull(CqmDataEntry.GetNotPerformedItem("TEST-X1","SNOMEDCT",listEhrCodes));
			Assert.IsNull(CqmDataEntry.GetNotPerformedItem("TEST-L1","SNOMEDCT",listEhrCodes));
			Assert.IsNull(CqmDataEntry.GetNotPerformedItem("TEST-S4","SNOMEDCT",listEhrCodes),"Tobacco User is not an item's set.");
			Assert.IsNull(CqmDataEntry.GetNotPerformedItem("TEST-R3","RXNORM",listEhrCodes),"Tobacco pharmacotherapy is not the BMI follow-up.");
			Assert.IsNull(CqmDataEntry.GetNotPerformedItem("","",listEhrCodes));
			Assert.IsNull(CqmDataEntry.GetNotPerformedItem(null,null,listEhrCodes));
			//Before the office imports the 2027 release, only the CVX rows are known.
			Assert.IsNull(CqmDataEntry.GetNotPerformedItem("TEST-L1","LOINC",new List<EhrCode>()));
			Assert.AreEqual(EhrNotPerformedItem.InfluenzaVaccination,CqmDataEntry.GetNotPerformedItem("TEST-C1","CVX",null));
			//A code whose table is not loaded still labels its row (the labels read membership, not IsInDb).
			Assert.AreEqual(EhrNotPerformedItem.TobaccoScreening,CqmDataEntry.GetNotPerformedItem("TEST-L3","LOINC",
				new List<EhrCode> { Row(CqmCatalog.OID_TOBACCO_SCREENING,"LOINC","TEST-L3",isInDb:false) }));
		}

		///<summary>The grid keeps each row's item, or null, in the row's Tag; the double-click reads the Tag and never parses the label (which threw on
		///"other"); an "other" row opens read-only, a known row with its item. The window names no code: the labels come from CqmDataEntry.</summary>
		[TestMethod]
		public void NotPerformedGrid_KeepsTheItemInTheTag_TheDoubleClickReadsIt_AndOtherOpensReadOnly() {
			string source=TestPaths.ReadSource(PATH_NOT_PERFORMED);
			string fillGrid=CSharpSource.GetBlock(source,"private void FillGrid()");
			StringAssert.Contains(fillGrid,"CqmDataEntry.GetNotPerformedItem(listNotPerf[i].CodeValue,listNotPerf[i].CodeSystem,listEhrCodes)");
			StringAssert.Contains(fillGrid,"row.Tag=itemNotPerformed;");
			StringAssert.Contains(fillGrid,"Lan.g(this,\"other\")");
			Assert.IsFalse(fillGrid.Contains("InfluenzaVaccination"),"No row defaults to influenza.");
			string doubleClick=CSharpSource.GetBlock(source,"private void gridMain_CellDoubleClick(");
			Assert.IsFalse(doubleClick.Contains("Enum.Parse"),"The handler parses no label.");
			Assert.IsFalse(doubleClick.Contains(".Cells["),"The handler reads no cell.");
			StringAssert.Contains(doubleClick,"gridMain.ListGridRows[e.Row].Tag as EhrNotPerformedItem?");
			StringAssert.Contains(doubleClick,"FormEE.IsReadOnly=true;");
			StringAssert.Contains(doubleClick,"FormEE.SelectedItemIndex=(int)itemNotPerformed.Value;");
			Assert.IsFalse(Regex.IsMatch(CSharpSource.Read(source).CodeWithoutComments,"Enum\\.Parse\\(typeof\\(EhrNotPerformedItem\\)"));
			//The edit window: an "other" row (or no item) is shown read-only, with no save and no delete; no item index -1 reaches the switch.
			string sourceEdit=TestPaths.ReadSource(PATH_NOT_PERFORMED_EDIT);
			string load=CSharpSource.GetBlock(sourceEdit,"private void FormEhrNotPerformedEdit_Load(");
			StringAssert.Contains(load,"if(IsReadOnly || !Enum.IsDefined(typeof(EhrNotPerformedItem),SelectedItemIndex)) {");
			StringAssert.Contains(load,"FillReadOnly();");
			StringAssert.Contains(load,"CqmDataEntry.GetNotPerformedCodes(itemNotPerformed)");
			Assert.IsTrue(load.IndexOf("FillReadOnly();",StringComparison.Ordinal)<load.IndexOf("(EhrNotPerformedItem)SelectedItemIndex",StringComparison.Ordinal),
				"The read-only case returns before the item is taken from the index.");
			string readOnly=CSharpSource.GetBlock(sourceEdit,"private void FillReadOnly()");
			foreach(string line in new[] { "butSave.Enabled=false;","butDelete.Enabled=false;","comboCodeReason.Enabled=false;","textNote.ReadOnly=true;",
				"textDate.ReadOnly=true;","textCode.Text=EhrNotPerfCur.CodeValue;","textDate.Text=EhrNotPerfCur.DateEntry.ToShortDateString();" })
			{
				StringAssert.Contains(readOnly,line);
			}
			StringAssert.Contains(CSharpSource.GetBlock(sourceEdit,"private void FillReasonList()"),"CqmDataEntry.GetNotPerformedReasonOids(itemNotPerformed,");
		}

		///<summary>The release is the 2027 one only by its label: no record, or a record of an earlier release (EHR-2's June 2013 import), is not.</summary>
		[TestMethod]
		public void ReleaseImported_OnlyThe2027ReleasesRecord() {
			Assert.IsFalse(CqmDataEntry.IsReleaseImported(null));
			Assert.IsFalse(CqmDataEntry.IsReleaseImported(new EhrValueSetRelease()),"A record with no label.");
			Assert.IsFalse(CqmDataEntry.IsReleaseImported(new EhrValueSetRelease { Label="TEST earlier release" }));
			Assert.IsTrue(CqmDataEntry.IsReleaseImported(new EhrValueSetRelease { Label=CqmCatalog.RELEASE_LABEL }));
			Assert.AreEqual(EhrValueSetImport.RELEASE_LABEL,CqmCatalog.RELEASE_LABEL);
		}

		///<summary>Before the office imports the 2027 release every row but the CVX rows is "other": the grid's label and the read-only window say
		///that the value sets are missing and where to import them; the plain "other" label and the "code is of no item" title are kept for an office
		///that has imported the release.</summary>
		[TestMethod]
		public void NotPerformed_OtherRows_SayWhenTheReleaseIsNotImported() {
			string fillGrid=CSharpSource.GetBlock(TestPaths.ReadSource(PATH_NOT_PERFORMED),"private void FillGrid()").Replace("\r\n","\n");//Either line ending
			StringAssert.Contains(fillGrid,"bool isReleaseImported=CqmDataEntry.IsReleaseImported();");
			StringAssert.Contains(fillGrid,"if(itemNotPerformed==null && !isReleaseImported) {\n\t\t\t\t\trow.Cells.Add(Lan.g(this,\"other (2027 value sets not imported)\"));");
			StringAssert.Contains(fillGrid,"else if(itemNotPerformed==null) {\n\t\t\t\t\trow.Cells.Add(Lan.g(this,\"other\"));");
			Assert.IsTrue(fillGrid.IndexOf("CqmDataEntry.IsReleaseImported()",StringComparison.Ordinal)<fillGrid.IndexOf("for(int i=0;",StringComparison.Ordinal),
				"Read once, not per row.");
			string readOnly=CSharpSource.GetBlock(TestPaths.ReadSource(PATH_NOT_PERFORMED_EDIT),"private void FillReadOnly()");
			int indexIf=readOnly.IndexOf("if(CqmDataEntry.IsReleaseImported()) {",StringComparison.Ordinal);
			int indexOfNoItem=readOnly.IndexOf("\"read-only, the code is of no item\"",StringComparison.Ordinal);
			int indexElse=readOnly.IndexOf("else {",StringComparison.Ordinal);
			int indexNotImported=readOnly.IndexOf("\"read-only, the 2027 value sets are not imported\"",StringComparison.Ordinal);
			int indexMessage=readOnly.IndexOf("textDescription.Text=Lan.g(this,\"The EHR value sets of the 2027 release are not imported "
				+"(Setup | Chart | EHR, Code System Importer); until then this row's item is not known.\");",StringComparison.Ordinal);
			Assert.IsTrue(indexIf>=0 && indexOfNoItem>indexIf && indexElse>indexOfNoItem && indexNotImported>indexElse && indexMessage>indexElse,
				"The old title only with the release imported; without it the title and the message say why.");
		}
		#endregion Not performed

		#region Interventions
		[TestMethod]
		public void Interventions_TheCodeSetLists_AreThe2027ValueSets() {
			List<KeyValuePair<string,string>> listAll=CqmDataEntry.GetInterventionValueSets(InterventionCodeSet.None,true);
			CollectionAssert.AreEqual(new[] { "AboveNormalWeight Follow-up","AboveNormalWeight Referral","AboveNormalWeight Medication",
				"BelowNormalWeight Follow-up","BelowNormalWeight Referral","BelowNormalWeight Medication","Nutrition Counseling","PhysicalActivity Counseling",
				"TobaccoCessation Counseling","TobaccoCessation Medication","Dialysis Services" },listAll.Select(x => x.Key).ToList());
			CollectionAssert.AreEqual(new[] { CqmCatalog.OID_BMI_FOLLOW_UP_ABOVE,CqmCatalog.OID_BMI_REFERRALS,CqmCatalog.OID_BMI_MEDICATIONS_ABOVE,
				CqmCatalog.OID_BMI_FOLLOW_UP_BELOW,CqmCatalog.OID_BMI_REFERRALS,CqmCatalog.OID_BMI_MEDICATIONS_BELOW,CqmCatalog.OID_COUNSELING_NUTRITION,
				CqmCatalog.OID_COUNSELING_PHYSICAL_ACTIVITY,CqmCatalog.OID_TOBACCO_COUNSELING,CqmCatalog.OID_TOBACCO_PHARMACOTHERAPY,CqmCatalog.OID_DIALYSIS_SERVICES },
				listAll.Select(x => x.Value).ToList());
			//The window saves the first word of the label as the code set.
			foreach(KeyValuePair<string,string> pair in listAll) {
				Assert.IsTrue(Enum.TryParse(pair.Key.Split(' ')[0],out InterventionCodeSet _),pair.Key);
			}
			CollectionAssert.AreEqual(new[] { "Dialysis Services" },CqmDataEntry.GetInterventionValueSets(InterventionCodeSet.Dialysis,false).Select(x => x.Key).ToList(),
				"E-28: Dialysis Services alone.");
			CollectionAssert.AreEqual(new[] { "Nutrition Counseling","PhysicalActivity Counseling" },
				CqmDataEntry.GetInterventionValueSets(InterventionCodeSet.PhysicalActivity,false).Select(x => x.Key).ToList());
			Assert.AreEqual(3,CqmDataEntry.GetInterventionValueSets(InterventionCodeSet.BelowNormalWeight,false).Count);
			Assert.AreEqual(0,CqmDataEntry.GetInterventionValueSets(InterventionCodeSet.None,false).Count);
			//The window takes its list from here.
			string window=TestPaths.ReadSource(@"OpenPmsDental\Forms\FormInterventionEdit.cs");
			StringAssert.Contains(window,"CqmDataEntry.GetInterventionValueSets(InterventionCur.CodeSet,IsAllTypes)");
		}

		///<summary>The intervention window says why its list is empty (no import, or no code-system table), in the grid's title and once on load of a
		///new entry; and a saved intervention whose code is in none of the 2027 lists (a 2014 Dialysis Education row, E-28) keeps its code as a selected
		///row, so saving again keeps it, as the not-performed window keeps a saved reason.</summary>
		[TestMethod]
		public void Interventions_AnEmptyListSaysWhy_AndASavedCodeOutsideTheListsIsKept() {
			const string message="The codes of these intervention lists come from the EHR value sets of the 2027 release and the code-system tables; "
				+"run the Code System Importer in Setup | Chart | EHR.";
			string window=TestPaths.ReadSource(@"OpenPmsDental\Forms\FormInterventionEdit.cs");
			string fillGrid=CSharpSource.GetBlock(window,"private void FillGrid()");
			int indexRead=fillGrid.IndexOf("_listEhrCodes=EhrCodes.GetForValueSetOIDs(listValSetOIDs,true);",StringComparison.Ordinal);
			int indexEmpty=fillGrid.IndexOf("if(_listEhrCodes.Count==0) {",StringComparison.Ordinal);
			int indexTitle=fillGrid.IndexOf("gridMain.Title=_titleGrid+\" - \"+Lan.g(this,\"none: the codes of these intervention lists come from the EHR value "
				+"sets of the 2027 release and the code-system tables; run the Code System Importer in Setup | Chart | EHR.\");",StringComparison.Ordinal);
			int indexSaved=fillGrid.IndexOf("if(!InterventionCur.IsNew && !string.IsNullOrEmpty(InterventionCur.CodeValue)",StringComparison.Ordinal);
			int indexAdd=fillGrid.IndexOf("_listEhrCodes.Add(ehrCodeSaved);",StringComparison.Ordinal);
			int indexLoop=fillGrid.IndexOf("for(int i=0;i<_listEhrCodes.Count;i++) {",StringComparison.Ordinal);
			int indexSelect=fillGrid.IndexOf("gridMain.SetSelected(idxSelected,true);",StringComparison.Ordinal);
			Assert.IsTrue(indexRead>=0 && indexEmpty>indexRead && indexTitle>indexEmpty,"The empty list is said in the grid's title.");
			Assert.IsTrue(indexSaved>indexEmpty && indexAdd>indexSaved && indexLoop>indexAdd && indexSelect>indexLoop,
				"The saved code is added before the rows are filled, and its row is selected like any other.");
			StringAssert.Contains(fillGrid,"gridMain.Title=_titleGrid;");
			StringAssert.Contains(fillGrid,"ehrCodeSaved.CodeValue=InterventionCur.CodeValue;");
			StringAssert.Contains(fillGrid,"ehrCodeSaved.CodeSystem=InterventionCur.CodeSystem;");
			StringAssert.Contains(fillGrid,"if(_listEhrCodes[i].CodeValue==InterventionCur.CodeValue && _listEhrCodes[i].CodeSystem==InterventionCur.CodeSystem) {");
			string load=CSharpSource.GetBlock(window,"private void FormInterventionEdit_Load(");
			Assert.IsTrue(load.IndexOf("_titleGrid=gridMain.Title;",StringComparison.Ordinal)<load.IndexOf("FillGrid();",StringComparison.Ordinal));
			StringAssert.Contains(load,"if(IsSelectionMode && EhrCodes.GetForValueSetOIDs(new List<string>(_dictionaryValueCodeSets.Values),true).Count==0) {");
			StringAssert.Contains(load,"MsgBox.Show(this,\""+message+"\");");
			//Saving with "All" a code that is in no list keeps the code set; listVSFound[0] is read only when one set holds the code.
			string save=CSharpSource.GetBlock(window,"private void butSave_Click(");
			StringAssert.Contains(save,"else if(listVSFound.Count==1) {");
			StringAssert.Contains(save,"selectedCodeSet=InterventionCur.CodeSet.ToString();");
		}

		[TestMethod]
		public void Interventions_MedicationRows_AreLabelledByTheValueSetThatHoldsTheRxCui() {
			List<EhrCode> listEhrCodes=MakeList();
			Assert.AreEqual(InterventionCodeSet.AboveNormalWeight,CqmDataEntry.GetMedicationCodeSet("TEST-R1",listEhrCodes));
			Assert.AreEqual(InterventionCodeSet.BelowNormalWeight,CqmDataEntry.GetMedicationCodeSet("TEST-R2",listEhrCodes));
			Assert.AreEqual(InterventionCodeSet.TobaccoCessation,CqmDataEntry.GetMedicationCodeSet("TEST-R3",listEhrCodes));
			Assert.AreEqual(InterventionCodeSet.None,CqmDataEntry.GetMedicationCodeSet("TEST-R9",listEhrCodes),"Not \"tobacco\" for every other.");
			Assert.AreEqual(InterventionCodeSet.None,CqmDataEntry.GetMedicationCodeSet("TEST-S2",listEhrCodes),"A SNOMED CT row is no medication.");
			CollectionAssert.AreEqual(new[] { CqmCatalog.OID_BMI_MEDICATIONS_ABOVE,CqmCatalog.OID_BMI_MEDICATIONS_BELOW,CqmCatalog.OID_TOBACCO_PHARMACOTHERAPY },
				CqmDataEntry.GetMedicationInterventionOids());
			string window=TestPaths.ReadSource(@"OpenPmsDental\Forms\FormInterventions.cs");
			StringAssert.Contains(window,"CqmDataEntry.GetMedicationCodeSet(_listMedicationPats[i].RxCui.ToString(),listEhrCodesAll)");
			StringAssert.Contains(window,"CqmDataEntry.GetMedicationInterventionOids()");
			Assert.IsFalse(Regex.IsMatch(CSharpSource.Read(window).CodeWithoutComments,@"RxCui\s*==\s*\d"),"No RxCui literal.");
			string vitals=TestPaths.ReadSource(@"OpenPmsDental\Forms\FormVitalsignEdit2014.cs");
			StringAssert.Contains(vitals,"EhrCodes.GetForValueSetOIDs(new List<string> { CqmCatalog.OID_BMI_MEDICATIONS_ABOVE,CqmCatalog.OID_BMI_MEDICATIONS_BELOW },true)");
		}
		#endregion Interventions

		#region The medical window and the recommended codes
		///<summary>The medical window writes the role's code for "current medications documented" and filters the not-performed rows by it; with no
		///role resolved it says so and writes nothing. It names no code.</summary>
		[TestMethod]
		public void MedicalWindow_WritesTheRolesCode_AndNothingWithoutARole() {
			string source=TestPaths.ReadSource(@"OpenPmsDental\Forms\FormMedical.cs");
			string save=CSharpSource.GetBlock(source,"private void butSave_Click(");
			int indexIf=save.IndexOf("if(radioMedsDocumentedYes.Checked && _EhrMeasureEventNum==0) {",StringComparison.Ordinal);
			Assert.IsTrue(indexIf>=0);
			string partYes=save.Substring(indexIf,save.IndexOf("if(radioMedsDocumentedNo.Checked) {",StringComparison.Ordinal)-indexIf);
			StringAssert.Contains(partYes,"EhrCode ehrCodeMedsDocumented=CqmDataEntry.GetDirectCode(CqmCatalog.ROLE_CMS68_MEDS_DOCUMENTED);");
			int indexNull=partYes.IndexOf("if(ehrCodeMedsDocumented==null) {",StringComparison.Ordinal);
			int indexInsert=partYes.IndexOf("EhrMeasureEvents.Insert(ehrMeasureEvent);",StringComparison.Ordinal);
			int indexElse=partYes.IndexOf("else {",indexNull,StringComparison.Ordinal);
			Assert.IsTrue(indexNull>=0 && indexElse>indexNull && indexInsert>indexElse,"The event is inserted only in the else of the null check.");
			StringAssert.Contains(partYes.Substring(indexNull,indexElse-indexNull),"MsgBox.Show(this,");
			StringAssert.Contains(partYes,"ehrMeasureEvent.CodeValueEvent=ehrCodeMedsDocumented.CodeValue;");
			StringAssert.Contains(partYes,"ehrMeasureEvent.CodeSystemEvent=ehrCodeMedsDocumented.CodeSystem;");
			//The not-performed rows of today are filtered by the same role.
			StringAssert.Contains(source,"listEhrNotPerformeds[i].CodeValue!=ehrCodeMedsDocumented.CodeValue");
			AssertNoCodeLiteral(source,"FormMedical.cs");
		}

		///<summary>The recommended encounter and pregnancy codes come from the import (E-32): no SNOMED CT literal is left in the two windows.</summary>
		[TestMethod]
		public void RecommendedCodes_ComeFromTheImport_NoSnomedLiteralLeft() {
			foreach(string path in new[] { @"OpenPmsDental\Forms\FormEhrSettings.cs",@"OpenPmsDental\Forms\FormEncounterTool.cs" }) {
				string source=TestPaths.ReadSource(path);
				AssertNoCodeLiteral(source,path);
				StringAssert.Contains(source,"CqmDataEntry.GetRecommendedEncounterCodes()",path);
			}
			StringAssert.Contains(TestPaths.ReadSource(@"OpenPmsDental\Forms\FormEhrSettings.cs"),"CqmDataEntry.GetRecommendedPregnancyCodes()");
			//The codes are those of the value sets' SNOMED CT rows, each once, in code order.
			List<EhrCode> listEhrCodes=new List<EhrCode> {
				Row(CqmCatalog.OID_PREGNANCY,"SNOMEDCT","TEST-P2"),Row(CqmCatalog.OID_PREGNANCY,"SNOMEDCT","TEST-P1"),Row(CqmCatalog.OID_PREGNANCY,"ICD10CM","TEST-P3"),
				Row(CqmCatalog.OID_PREGNANCY,"SNOMEDCT","TEST-P1"),Row(CqmCatalog.OID_TOBACCO_USER,"SNOMEDCT","TEST-P4"),
			};
			CollectionAssert.AreEqual(new[] { "TEST-P1","TEST-P2" },CqmDataEntry.GetSnomedCodes(new List<string> { CqmCatalog.OID_PREGNANCY },listEhrCodes));
			Assert.AreEqual(0,CqmDataEntry.GetSnomedCodes(new List<string> { CqmCatalog.OID_PREGNANCY },null).Count);
		}

		///<summary>No string literal of the source is a code-like number (SNOMED CT concept IDs, RxCuis) or a LOINC code: the windows read every code
		///from the import.</summary>
		private static void AssertNoCodeLiteral(string source,string name) {
			List<string> listCodes=CSharpSource.Read(source).ListStringLiterals.Where(x => Regex.IsMatch(x,@"^\s*(\d{5,18}|\d{3,7}-\d)\s*$")).ToList();
			Assert.AreEqual(0,listCodes.Count,name+" holds a code literal: "+string.Join(", ",listCodes));
		}
		#endregion The medical window and the recommended codes

		#region No value-set OID in the windows
		[TestMethod]
		public void Windows_NameEveryValueSetByTheCatalogsConstants() {
			Regex regexOid=new Regex(@"^2\.16\.840\.1\.(113883\.3|113762\.1\.4|114222\.4\.11)\.");
			foreach(string path in new[] { PATH_NOT_PERFORMED,PATH_NOT_PERFORMED_EDIT,@"OpenPmsDental\Forms\FormEhrPatientSmoking.cs",
				@"OpenPmsDental\Forms\FormInterventionEdit.cs",@"OpenPmsDental\Forms\FormInterventions.cs",@"OpenPmsDental\Forms\FormMedical.cs",
				@"OpenPmsDental\Forms\FormVitalsignEdit2014.cs",@"OpenPmsDentalBusiness\Data Interface\DiseaseDefs.cs",
				@"OpenPmsDentalBusiness\Db Multi Table\Cqm\CqmDataEntry.cs" })
			{
				List<string> listOids=CSharpSource.Read(TestPaths.ReadSource(path)).ListStringLiterals.Where(x => regexOid.IsMatch(x)).ToList();
				Assert.AreEqual(0,listOids.Count,path+": "+string.Join(", ",listOids));
			}
			//The not-performed rows no longer hold the 2014 LOINC and SNOMED CT literals.
			AssertNoCodeLiteral(TestPaths.ReadSource(PATH_NOT_PERFORMED),"FormEhrNotPerformed.cs");
			AssertNoCodeLiteral(TestPaths.ReadSource(PATH_NOT_PERFORMED_EDIT),"FormEhrNotPerformedEdit.cs");
		}
		#endregion No value-set OID in the windows
	}
}
