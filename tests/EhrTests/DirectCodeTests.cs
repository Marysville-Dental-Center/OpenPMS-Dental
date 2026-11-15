// The release's other two files and the pick of the three (docs/specs/ehr-2027-measures-plan.md, EHR-6a, sections 7.1, 7.3 and 7.4), with
// no database: a synthetic Direct Reference Codes file (one TEST code per role of the catalog) and Code System Versions file in the provisional
// shapes of docs/port-notes/EHR-6.md, the kind of each file by its header row, the roles resolved (the LOINC ones by display name), a role
// missing or twice, the measure-version check of the Direct Reference Codes file, the pick refused without all three files or with a file of
// two kinds, the pick read under one row budget with a second file of a kind refused at its first row (a 32-bit process holds no more than one
// parsed value-set file), and the code-system versions compared with a loaded table's. Made-up TEST codes only.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalBusiness;

namespace EhrTests {
	[TestClass]
	public class DirectCodeTests {
		[TestInitialize]
		public void Begin() {
			//Lans.g returns the English text in en-US and needs no database there.
			System.Threading.Thread.CurrentThread.CurrentCulture=new System.Globalization.CultureInfo("en-US");
		}

		#region The pick
		[TestMethod]
		public void Release_TheSyntheticThreeFiles_GiveSevenOfSeven_EveryRoleResolved() {
			EhrValueSetPick pick=ValueSetFixtures.ParsePick(ValueSetFixtures.MakeRelease());
			string report=EhrValueSetImport.GetReport(pick);
			Assert.IsFalse(pick.IsRefused,report);
			Assert.AreEqual(EhrValueSetFileKind.ValueSets,pick.FileValueSets.Kind);
			Assert.AreEqual(EhrValueSetFileKind.DirectReferenceCodes,pick.FileDirectCodes.Kind);
			Assert.AreEqual(EhrValueSetFileKind.CodeSystemVersions,pick.FileCodeSystemVersions.Kind);
			CollectionAssert.AreEqual(EhrValueSetImport.ArrayMeasureIds,pick.FileValueSets.ListMeasuresMatched);
			StringAssert.Contains(report,"7 of 7 measures at the 2027 versions");
			Assert.AreEqual(8,pick.ListRoles.Count);
			Assert.IsTrue(pick.ListRoles.All(x => x.IsResolved && x.CountMatched==x.CountExpected),report);
			Assert.AreEqual(0,pick.ListWarnings.Count,string.Join("\r\n",pick.ListWarnings));
			StringAssert.Contains(report,"Direct-reference-code roles resolved: 8 of 8");
			//Each direct reference code is an ehrcode row under its key, with the category and the measure.
			List<EhrCode> listDirect=pick.ListEhrCodesDirect;
			Assert.AreEqual(ValueSetFixtures.MakeDirectCodeRows().Count,listDirect.Count);
			Assert.IsTrue(listDirect.All(x => x.QDMCategory==CqmCatalog.DIRECT_CODE_CATEGORY && CqmCatalog.IsDirectCodeKey(x.ValueSetOID)));
			Assert.AreEqual(4,listDirect.Count(x => x.ValueSetOID=="drc:CMS138:Cms138QualifyingVisitCpt"));
			EhrCode ehrCodeBmi=listDirect.Single(x => x.ValueSetOID=="drc:CMS69:Cms69Bmi");
			Assert.AreEqual("LOINC",ehrCodeBmi.CodeSystem);
			Assert.AreEqual("2.16.840.1.113883.6.1",ehrCodeBmi.CodeSystemOID);
			Assert.AreEqual("69v15",ehrCodeBmi.MeasureIds);
			Assert.AreEqual("Body mass index (BMI) [Ratio]",ehrCodeBmi.Description);
			Assert.AreEqual("ActCode",listDirect.Single(x => x.ValueSetOID=="drc:CMS69:Cms69Virtual").CodeSystem);
			//The codes no role takes are listed under their measure.
			CollectionAssert.AreEquivalent(new[] { "drc:CMS69:Listed","drc:CMS165:Listed","drc:CMS74:Listed","drc:CMS75:Listed","drc:CMS155:Listed" },
				listDirect.Where(x => x.ValueSetOID.EndsWith(":Listed")).Select(x => x.ValueSetOID).ToList());
			CollectionAssert.AreEqual(EhrValueSetImport.ArrayMeasureIds,pick.FileDirectCodes.ListMeasuresMatched,"The file names all seven.");
			Assert.AreEqual(pick.FileValueSets.ListEhrCodes.Count+listDirect.Count,pick.ListEhrCodesAll.Count);
			//The code-system versions.
			Assert.AreEqual("2.80",pick.FileCodeSystemVersions.ListCodeSystemVersions.Single(x => x.CodeSystem=="LOINC").Version);
			Assert.AreEqual("2.16.840.1.113883.6.96",pick.FileCodeSystemVersions.ListCodeSystemVersions.Single(x => x.CodeSystem=="SNOMEDCT").CodeSystemOID);
			StringAssert.Contains(report,"SNOMEDCT 2025-09");
		}

		[TestMethod]
		public void Pick_WithoutAllThreeFiles_IsRefused_NamingWhatIsMissing() {
			List<KeyValuePair<string,byte[]>> listFiles=ValueSetFixtures.MakeRelease();
			//Each file alone, and each pair: refused before any file is read.
			for(int i=0;i<3;i++) {
				EhrValueSetPick pickOne=ValueSetFixtures.ParsePick(new List<KeyValuePair<string,byte[]>> { listFiles[i] });
				Assert.IsTrue(pickOne.IsRefused,listFiles[i].Key);
				StringAssert.Contains(pickOne.ListRefusals[0],"Pick the three files of the release together");
				StringAssert.Contains(pickOne.ListRefusals[0],"Files picked: 1.");
				Assert.AreEqual(0,pickOne.ListFiles.Count,"No file is read.");
				EhrValueSetPick pickTwo=ValueSetFixtures.ParsePick(listFiles.Where((x,index) => index!=i).ToList());
				Assert.IsTrue(pickTwo.IsRefused,"Without "+listFiles[i].Key);
				StringAssert.Contains(pickTwo.ListRefusals[0],"Files picked: 2.");
				Assert.AreEqual(0,pickTwo.ListEhrCodesAll.Count,"Nothing to write.");
				//The files read one by one and checked together name the kind that is missing.
				EhrValueSetPick pickTwoEach=ValueSetFixtures.ParseEachThenPick(listFiles.Where((x,index) => index!=i).ToList());
				Assert.IsTrue(pickTwoEach.ListRefusals.Any(x => x.StartsWith("The pick has no file with ")),string.Join("\r\n",pickTwoEach.ListRefusals));
			}
			//Three files, one of them not of the release: the missing kind is named.
			KeyValuePair<string,byte[]> pairReadme=ValueSetFixtures.Entry("readme.xml",System.Text.Encoding.UTF8.GetBytes("<notes>TEST</notes>"));
			EhrValueSetPick pickNoDirect=ValueSetFixtures.ParsePick(new List<KeyValuePair<string,byte[]>> { listFiles[0],pairReadme,listFiles[2] });
			Assert.IsTrue(pickNoDirect.ListRefusals.Any(x => x=="The pick has no file with the Direct Reference Codes file."),
				string.Join("\r\n",pickNoDirect.ListRefusals));
			Assert.IsTrue(pickNoDirect.ListRefusals.Any(x => x.StartsWith("readme.xml: ")),string.Join("\r\n",pickNoDirect.ListRefusals));
			//Two value-set files in place of the Direct Reference Codes file: the second is refused at its first row.
			EhrValueSetPick pickTwice=ValueSetFixtures.ParsePick(new List<KeyValuePair<string,byte[]>> { listFiles[0],
				ValueSetFixtures.Entry("other_TEST.xlsx",listFiles[0].Value),listFiles[2] });
			Assert.IsTrue(pickTwice.IsRefused);
			Assert.IsTrue(pickTwice.ListRefusals.Any(x => x.StartsWith("other_TEST.xlsx: The pick has more than one file with the value sets")
				&& x.Contains("ec_value_sets_TEST.xlsx")),string.Join("\r\n",pickTwice.ListRefusals));
			//A fourth file, even a good one: refused before any file is read.
			EhrValueSetPick pickFour=ValueSetFixtures.ParsePick(listFiles.Concat(new[] { pairReadme }).ToList());
			Assert.IsTrue(pickFour.IsRefused);
			StringAssert.Contains(pickFour.ListRefusals[0],"Files picked: 4.");
			Assert.AreEqual(0,pickFour.ListFiles.Count);
			//The same four read one by one and checked together are refused too.
			EhrValueSetPick pickFourEach=ValueSetFixtures.ParseEachThenPick(listFiles.Concat(new[] { pairReadme }).ToList());
			Assert.IsTrue(pickFourEach.IsRefused);
			Assert.IsTrue(pickFourEach.ListRefusals.Any(x => x.StartsWith("readme.xml: ")),string.Join("\r\n",pickFourEach.ListRefusals));
			//None at all, and null.
			Assert.IsTrue(EhrValueSetImport.MakePick(new List<EhrValueSetFile>()).IsRefused);
			Assert.IsTrue(EhrValueSetImport.MakePick(null).IsRefused);
			Assert.IsTrue(EhrValueSetImport.ParsePick(null,index => throw new AssertFailedException("No file is opened.")).IsRefused);
			StringAssert.Contains(EhrValueSetImport.GetReport(pickNoDirect),"The pick is refused. Nothing was imported");
		}

		[TestMethod]
		public void Pick_OfFourFiles_TwoOfThemValueSets_IsRefused_BeforeAnyFileIsOpened() {
			List<KeyValuePair<string,byte[]>> listFiles=ValueSetFixtures.MakeRelease();
			//The workbook and the same release's value sets again (as an office might pick the .xlsx and the SVS .zip), with the two other files.
			listFiles.Insert(1,ValueSetFixtures.Entry("ec_value_sets_svs_TEST.xlsx",listFiles[0].Value));
			List<int> listIndexesOpened=new List<int>();
			EhrValueSetPick pick=EhrValueSetImport.ParsePick(listFiles.Select(x => x.Key).ToList(),index => {
				listIndexesOpened.Add(index);
				return new MemoryStream(listFiles[index].Value,writable:false);
			});
			Assert.IsTrue(pick.IsRefused);
			StringAssert.Contains(pick.ListRefusals[0],"Files picked: 4.");
			Assert.AreEqual(0,listIndexesOpened.Count,"No file is opened.");
			Assert.AreEqual(0,pick.ListFiles.Count,"No value-set file's rows are held.");
			Assert.AreEqual(0,pick.ListEhrCodesAll.Count);
		}

		[TestMethod]
		public void Pick_ASecondValueSetFile_KeepsNoRows_OnlyTheFirstFilesRowsAreHeld() {
			List<KeyValuePair<string,byte[]>> listFiles=ValueSetFixtures.MakeRelease();
			int countValueSetCodes=EhrValueSetImport.Parse(listFiles[0].Key,listFiles[0].Value).ListEhrCodes.Count;
			Assert.IsTrue(countValueSetCodes>0);
			//The value sets twice and the Code System Versions file.
			EhrValueSetPick pick=ValueSetFixtures.ParsePick(new List<KeyValuePair<string,byte[]>> { listFiles[0],
				ValueSetFixtures.Entry("ec_value_sets_again_TEST.xlsx",listFiles[0].Value),listFiles[2] });
			Assert.IsTrue(pick.IsRefused);
			Assert.AreEqual(3,pick.ListFiles.Count);
			EhrValueSetFile fileSecond=pick.ListFiles[1];
			Assert.IsTrue(fileSecond.IsRefused);
			Assert.AreEqual(EhrValueSetFileKind.ValueSets,fileSecond.Kind);
			Assert.AreEqual(1,fileSecond.CountRowsRead,"Read no further than its first row.");
			Assert.AreEqual(0,fileSecond.ListEhrCodes.Count);
			StringAssert.Contains(fileSecond.ListRefusals[0],"its rows are not kept");
			Assert.AreEqual(countValueSetCodes,pick.ListFiles.Sum(x => x.ListEhrCodes.Count),"The rows of one value-set file at most.");
			Assert.AreEqual(0,pick.ListEhrCodesAll.Count,"Nothing to write.");
			StringAssert.Contains(EhrValueSetImport.GetReport(pick),"ec_value_sets_again_TEST.xlsx");
		}

		[TestMethod]
		public void Pick_TheRowBudget_IsForTheWholePick() {
			List<KeyValuePair<string,byte[]>> listFiles=ValueSetFixtures.MakeRelease();
			List<string> listNames=listFiles.Select(x => x.Key).ToList();
			List<int> listRows=listFiles.Select(x => EhrValueSetImport.Parse(x.Key,x.Value).CountRowsRead).ToList();
			Assert.IsTrue(listRows.All(x => x>0));
			Func<int,Stream> funcOpen=index => new MemoryStream(listFiles[index].Value,writable:false);
			//Exactly the rows of the three: accepted.
			EhrValueSetPick pickExact=EhrValueSetImport.ParsePick(listNames,funcOpen,listRows.Sum());
			Assert.IsFalse(pickExact.IsRefused,EhrValueSetImport.GetReport(pickExact));
			//One row less: each file is within it alone, the three together are not. The last file is refused, and nothing is written.
			Assert.IsTrue(listRows.Max()<listRows.Sum()-1);
			EhrValueSetPick pickOver=EhrValueSetImport.ParsePick(listNames,funcOpen,listRows.Sum()-1);
			Assert.IsTrue(pickOver.IsRefused);
			Assert.IsFalse(pickOver.ListFiles[0].IsRefused);
			Assert.IsTrue(pickOver.ListFiles[2].IsRefused);
			StringAssert.Contains(pickOver.ListFiles[2].ListRefusals[0],"The files of the pick have more rows together than the importer reads ("
				+(listRows.Sum()-1)+").");
			Assert.AreEqual(0,pickOver.ListEhrCodesAll.Count);
		}

		[TestMethod]
		public void Pick_AFileWithTheRowsOfTwoKinds_IsRefused() {
			ValueSetFixtures.Sheet sheetValueSets=new ValueSetFixtures.Sheet();
			sheetValueSets.Name="Value Sets";
			sheetValueSets.ListRows=new List<TestValueSetRow> { TestValueSetRow.Make("2.16.840.1.113883.3.526.3.1278","SNOMEDCT","TEST0001") };
			ValueSetFixtures.Sheet sheetDirect=new ValueSetFixtures.Sheet();
			sheetDirect.Name="Direct Reference Codes";
			sheetDirect.ArrayHeaders=ValueSetFixtures.ArrayHeadersDirectCodes;
			sheetDirect.ListRows=new List<TestValueSetRow> { TestValueSetRow.MakeDirectCode("CMS68v16","SNOMEDCT","TEST0002") };
			EhrValueSetFile file=EhrValueSetImport.Parse("both_TEST.xlsx",ValueSetFixtures.MakeWorkbook(sheetValueSets,sheetDirect));
			Assert.IsTrue(file.IsRefused);
			Assert.AreEqual(EhrValueSetFileKind.Mixed,file.Kind);
			StringAssert.Contains(file.ListRefusals[0],"more than one of the release's three files");
			StringAssert.Contains(file.ListRefusals[0],"the value sets, the Direct Reference Codes file");
		}
		#endregion The pick

		#region The Direct Reference Codes file
		[TestMethod]
		public void DirectCodes_ARoleMissing_AndARoleTwice_Warn_AndTheTwiceIsNotResolved() {
			List<TestValueSetRow> listRows=ValueSetFixtures.MakeDirectCodeRows();
			//CMS68's documentation code missing, CMS165's kidney transplant status twice.
			listRows.RemoveAll(x => x.MeasureIds=="CMS68v16");
			listRows.Add(TestValueSetRow.MakeDirectCode("CMS165v15","ICD10CM","TESTD9999"));
			EhrValueSetPick pick=ValueSetFixtures.ParsePick(ValueSetFixtures.MakeRelease(listDirectCodes:listRows));
			Assert.IsFalse(pick.IsRefused,"A role that does not resolve warns; it does not refuse: "+EhrValueSetImport.GetReport(pick));
			EhrDirectCodeRoleResult resultMissing=pick.ListRoles.Single(x => x.Role=="Cms68MedsDocumented");
			Assert.IsFalse(resultMissing.IsResolved);
			Assert.AreEqual(0,resultMissing.CountMatched);
			EhrDirectCodeRoleResult resultTwice=pick.ListRoles.Single(x => x.Role=="Cms165KidneyTransplantDx");
			Assert.IsFalse(resultTwice.IsResolved);
			Assert.AreEqual(2,resultTwice.CountMatched);
			Assert.IsTrue(pick.ListWarnings.Any(x => x.Contains("Cms68MedsDocumented") && x.Contains("matches 0 codes of the file where it takes 1")),
				string.Join("\r\n",pick.ListWarnings));
			Assert.IsTrue(pick.ListWarnings.Any(x => x.Contains("Cms165KidneyTransplantDx") && x.Contains("matches 2 codes")
				&& x.Contains("not calculated")),string.Join("\r\n",pick.ListWarnings));
			Assert.AreEqual(0,pick.ListEhrCodesDirect.Count(x => x.ValueSetOID=="drc:CMS165:Cms165KidneyTransplantDx"),"Neither code takes the role.");
			Assert.AreEqual(2,pick.ListEhrCodesDirect.Count(x => x.ValueSetOID=="drc:CMS165:Listed" && x.CodeSystem=="ICD10CM"),"Both stay listed.");
			Assert.AreEqual(6,pick.ListRoles.Count(x => x.IsResolved));
			StringAssert.Contains(EhrValueSetImport.GetReport(pick),"Direct-reference-code roles resolved: 6 of 8");
			//CMS138's CPT visits: three of the four still take the role, with a warning.
			List<TestValueSetRow> listRowsCpt=ValueSetFixtures.MakeDirectCodeRows();
			listRowsCpt.Remove(listRowsCpt.First(x => x.CodeSystem=="CPT"));
			EhrValueSetPick pickCpt=ValueSetFixtures.ParsePick(ValueSetFixtures.MakeRelease(listDirectCodes:listRowsCpt));
			EhrDirectCodeRoleResult resultCpt=pickCpt.ListRoles.Single(x => x.Role=="Cms138QualifyingVisitCpt");
			Assert.IsTrue(resultCpt.IsResolved);
			Assert.AreEqual(3,resultCpt.CountMatched);
			Assert.IsTrue(pickCpt.ListWarnings.Any(x => x.Contains("Cms138QualifyingVisitCpt") && x.Contains("matches 3 codes of the file where it takes 4")));
		}

		[TestMethod]
		public void DirectCodes_TheLoincRoles_AreToldApartByTheirDisplayNames() {
			List<TestValueSetRow> listRows=ValueSetFixtures.MakeDirectCodeRows();
			EhrValueSetPick pick=ValueSetFixtures.ParsePick(ValueSetFixtures.MakeRelease(listDirectCodes:listRows));
			string codeSystolic=listRows.Single(x => x.Description=="Systolic blood pressure").CodeValue;
			string codeDiastolic=listRows.Single(x => x.Description=="Diastolic blood pressure").CodeValue;
			Assert.AreEqual("drc:CMS165:Cms165Systolic",pick.ListEhrCodesDirect.Single(x => x.CodeValue==codeSystolic).ValueSetOID);
			Assert.AreEqual("drc:CMS165:Cms165Diastolic",pick.ListEhrCodesDirect.Single(x => x.CodeValue==codeDiastolic).ValueSetOID);
			Assert.AreEqual(1,pick.ListEhrCodesDirect.Count(x => x.ValueSetOID=="drc:CMS165:Listed" && x.CodeSystem=="LOINC"),"The other LOINC code.");
			//The display name is compared without case and with white space collapsed.
			listRows.Single(x => x.CodeValue==codeSystolic).Description="  SYSTOLIC   blood pressure ";
			EhrValueSetPick pickCase=ValueSetFixtures.ParsePick(ValueSetFixtures.MakeRelease(listDirectCodes:listRows));
			Assert.IsTrue(pickCase.ListRoles.Single(x => x.Role=="Cms165Systolic").IsResolved);
			//A LOINC code of another measure, or of the 2026 version, does not take the role.
			listRows.Single(x => x.CodeValue==codeSystolic).MeasureIds="CMS165v14";
			EhrValueSetPick pickVersion=ValueSetFixtures.ParsePick(ValueSetFixtures.MakeRelease(listDirectCodes:listRows));
			Assert.IsFalse(pickVersion.ListRoles.Single(x => x.Role=="Cms165Systolic").IsResolved);
			Assert.IsTrue(pickVersion.ListRoles.Single(x => x.Role=="Cms165Diastolic").IsResolved);
			Assert.AreEqual("",pickVersion.ListEhrCodesDirect.Single(x => x.CodeValue==codeSystolic).MeasureIds,"Not a measure this program reports.");
		}

		[TestMethod]
		public void DirectCodes_TheFileIsCheckedForThe2027Versions_AndRowsWithoutAMeasureAreSkipped() {
			List<TestValueSetRow> listRows=ValueSetFixtures.MakeDirectCodeRows();
			listRows.Add(TestValueSetRow.MakeDirectCode("","SNOMEDCT","TESTD8888"));
			listRows.Add(TestValueSetRow.MakeDirectCode("CMS1000v1, CMS74v16","SNOMEDCT","TESTD7777"));
			EhrValueSetFile file=EhrValueSetImport.Parse("drc_TEST.xlsx",ValueSetFixtures.MakeDirectCodesWorkbook(listRows));
			Assert.IsFalse(file.IsRefused,EhrValueSetImport.GetReport(file));
			Assert.AreEqual(EhrValueSetFileKind.DirectReferenceCodes,file.Kind);
			CollectionAssert.AreEqual(EhrValueSetImport.ArrayMeasureIds,file.ListMeasuresMatched);
			Assert.AreEqual(1,file.CountRowsSkipped);
			Assert.IsTrue(file.ListWarnings.Any(x => x.StartsWith("1 rows of the Direct Reference Codes file name no measure")));
			Assert.AreEqual(2,file.ListDirectCodes.Count(x => x.CodeValue=="TESTD7777"),"One row under each measure that lists it.");
			StringAssert.Contains(EhrValueSetImport.GetReport(file),"Holds: the Direct Reference Codes file");
			//The 2026 release's file: refused, naming the release this program needs.
			List<TestValueSetRow> listRows2026=ValueSetFixtures.MakeDirectCodeRows();
			foreach(TestValueSetRow row in listRows2026) {//One version before each 2027 version: CMS74v15, CMS69v14, ...
				row.MeasureIds=System.Text.RegularExpressions.Regex.Replace(row.MeasureIds,@"v(\d+)",
					x => "v"+(int.Parse(x.Groups[1].Value,System.Globalization.CultureInfo.InvariantCulture)-1).ToString(System.Globalization.CultureInfo.InvariantCulture));
			}
			EhrValueSetFile file2026=EhrValueSetImport.Parse("drc_2026_TEST.xlsx",ValueSetFixtures.MakeDirectCodesWorkbook(listRows2026));
			Assert.IsTrue(file2026.IsRefused,EhrValueSetImport.GetReport(file2026));
			StringAssert.Contains(file2026.ListRefusals[0],"None of the seven measures this program reports is in the file at its 2027 version");
			//A header row without the CMS ID or the code system refuses the file, naming them.
			EhrValueSetFile fileNoMeasure=EhrValueSetImport.Parse("drc_TEST.xlsx",ValueSetFixtures.MakeDirectCodesWorkbook(listRows,
				new[] { "Code","Description" }));
			Assert.IsTrue(fileNoMeasure.IsRefused);
			StringAssert.Contains(fileNoMeasure.ListRefusals[0],"has no column CMS ID, Code System (or Code System OID).");
			//An empty file of the kind is refused.
			Assert.IsTrue(EhrValueSetImport.Parse("drc_TEST.xlsx",ValueSetFixtures.MakeDirectCodesWorkbook(new List<TestValueSetRow>())).IsRefused);
		}
		#endregion The Direct Reference Codes file

		#region The Code System Versions file
		[TestMethod]
		public void CodeSystemVersions_TheFileIsRead_ByNameOrOid() {
			List<TestValueSetRow> listRows=ValueSetFixtures.MakeCodeSystemVersionRows();
			TestValueSetRow rowNoOid=TestValueSetRow.MakeCodeSystemVersion("ICD10CM","2026");
			rowNoOid.CodeSystemOID="";
			rowNoOid.CodeSystem="ICD10 CM";
			listRows[2]=rowNoOid;
			listRows.Add(TestValueSetRow.MakeCodeSystemVersion("LOINC",""));//No version: skipped.
			EhrValueSetFile file=EhrValueSetImport.Parse("csv_TEST.xlsx",ValueSetFixtures.MakeCodeSystemVersionsWorkbook(listRows));
			Assert.IsFalse(file.IsRefused,EhrValueSetImport.GetReport(file));
			Assert.AreEqual(EhrValueSetFileKind.CodeSystemVersions,file.Kind);
			Assert.AreEqual(8,file.ListCodeSystemVersions.Count);
			Assert.AreEqual(1,file.CountRowsSkipped);
			Assert.AreEqual("2026",file.ListCodeSystemVersions.Single(x => x.CodeSystem=="ICD10CM").Version);
			Assert.AreEqual("2.16.840.1.113883.6.90",file.ListCodeSystemVersions.Single(x => x.CodeSystem=="ICD10CM").CodeSystemOID);
			//"Code System Version" in place of "Version".
			EhrValueSetFile fileOtherHeader=EhrValueSetImport.Parse("csv_TEST.xlsx",ValueSetFixtures.MakeCodeSystemVersionsWorkbook(
				ValueSetFixtures.MakeCodeSystemVersionRows().Select(x => { x.CodeSystemVersion=x.ValueSetVersion;return x; }).ToList(),
				new[] { "Code System","Code System Version" }));
			Assert.IsFalse(fileOtherHeader.IsRefused,EhrValueSetImport.GetReport(fileOtherHeader));
			Assert.AreEqual("2.80",fileOtherHeader.ListCodeSystemVersions.Single(x => x.CodeSystem=="LOINC").Version);
			Assert.IsTrue(EhrValueSetImport.Parse("csv_TEST.xlsx",ValueSetFixtures.MakeCodeSystemVersionsWorkbook(new List<TestValueSetRow>())).IsRefused);
		}

		[TestMethod]
		public void CompareCodeSystemVersions_MapsEachCodeSystemsFormats_AndShowsBothWhereNoMappingExists() {
			//Negative: the loaded table is older than the release.
			Assert.AreEqual(-1,EhrValueSetImport.CompareCodeSystemVersions("LOINC","2.76","2.80"));
			Assert.AreEqual(0,EhrValueSetImport.CompareCodeSystemVersions("LOINC","Version 2.80","2.80"));
			Assert.AreEqual(1,EhrValueSetImport.CompareCodeSystemVersions("LOINC","2.81","2.80"));
			Assert.AreEqual(-1,EhrValueSetImport.CompareCodeSystemVersions("LOINC","2.9","2.80"),"2.9 is release 9, before release 80.");
			Assert.AreEqual(-1,EhrValueSetImport.CompareCodeSystemVersions("SNOMEDCT","20250301","2025-09"));
			Assert.AreEqual(0,EhrValueSetImport.CompareCodeSystemVersions("SNOMEDCT","2025-09-01","20250901"));
			Assert.AreEqual(1,EhrValueSetImport.CompareCodeSystemVersions("SNOMEDCT","US edition 2026-03","2025-09"));
			Assert.AreEqual(-1,EhrValueSetImport.CompareCodeSystemVersions("RXNORM","2025-12","2026-04"));
			Assert.AreEqual(-1,EhrValueSetImport.CompareCodeSystemVersions("ICD10CM","2025","2026"));
			Assert.AreEqual(0,EhrValueSetImport.CompareCodeSystemVersions("CPT","2026","CPT 2026"));
			//Several versions of one code system in the release: the newest counts.
			Assert.AreEqual(-1,EhrValueSetImport.CompareCodeSystemVersions("SNOMEDCT","2025-09","2025-03, 2026-03"));
			//No mapping, or a version that does not follow it: not compared.
			Assert.IsNull(EhrValueSetImport.CompareCodeSystemVersions("CDCREC","1.0","1.2"));
			Assert.IsNull(EhrValueSetImport.CompareCodeSystemVersions("LOINC","","2.80"));
			Assert.IsNull(EhrValueSetImport.CompareCodeSystemVersions("SNOMEDCT","TEST","2025-09"));
			Assert.IsNull(EhrValueSetImport.CompareCodeSystemVersions("ICD10CM","2026",""));
		}
		#endregion The Code System Versions file
	}
}
