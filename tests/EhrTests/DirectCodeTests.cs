// The release's other two files and the pick of the three (docs/specs/ehr-2027-measures-plan.md, EHR-6a, sections 7.1, 7.3 and 7.4), with
// no database: a synthetic Direct Reference Codes file (one TEST code per role of the catalog) and Code System Versions file in the shapes of
// E-20's record (docs/port-notes/EHR-6.md, section 0; EHR-6a-R: the header rows below the rows above them, the measure in eCQM ID, the value
// set's version from its Expansion Version, the two forms of the code systems' names, the versions each row names), the kind of each file by its header row, the roles resolved (the LOINC ones by display name), a role
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
			Assert.AreEqual(ValueSetFixtures.MakeDirectCodeRows().Count-ValueSetFixtures.COUNT_DIRECT_CODE_ROWS_OTHER_MEASURES,listDirect.Count);
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
				&& x.Contains(ValueSetFixtures.NAME_VALUE_SETS)),string.Join("\r\n",pickTwice.ListRefusals));
			//The second file was not read: its report gives no counts of its kind, only its name, hash, kind and refusal.
			EhrValueSetFile fileTwice=pickTwice.ListFiles.Single(x => x.FileName=="other_TEST.xlsx");
			Assert.IsTrue(fileTwice.IsKindTaken);
			Assert.IsFalse(pickTwice.ListFiles.Single(x => x.FileName==ValueSetFixtures.NAME_VALUE_SETS).IsKindTaken);
			string reportTwice=EhrValueSetImport.GetReport(fileTwice);
			foreach(string counts in new[] { "found:","Rows read:","Codes to import:","Measures:" }) {
				Assert.IsFalse(reportTwice.Contains(counts),counts+" in the report of a file not read:\r\n"+reportTwice);
			}
			StringAssert.Contains(reportTwice,"File: other_TEST.xlsx");
			StringAssert.Contains(reportTwice,"SHA-256: "+fileTwice.Sha256);
			StringAssert.Contains(reportTwice,"Holds: the value sets");
			StringAssert.Contains(reportTwice,"This file was not read further");
			//The first file's report keeps its counts.
			StringAssert.Contains(EhrValueSetImport.GetReport(pickTwice.ListFiles.Single(x => x.FileName==ValueSetFixtures.NAME_VALUE_SETS)),
				"Value sets the measures use, found:");
			//A second Direct Reference Codes file in place of the Code System Versions file: the same.
			EhrValueSetPick pickTwiceDirect=ValueSetFixtures.ParsePick(new List<KeyValuePair<string,byte[]>> { listFiles[0],listFiles[1],
				ValueSetFixtures.Entry("other_direct_TEST.xlsx",listFiles[1].Value) });
			string reportTwiceDirect=EhrValueSetImport.GetReport(pickTwiceDirect.ListFiles.Single(x => x.FileName=="other_direct_TEST.xlsx"));
			Assert.IsFalse(reportTwiceDirect.Contains("Direct reference codes to import:") || reportTwiceDirect.Contains("Rows read:"),reportTwiceDirect);
			StringAssert.Contains(reportTwiceDirect,"This file was not read further");
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
		public void Pick_TheThreeFilesOfThe2026Release_IsRefused_FirstNamingTheReleaseThisProgramNeeds() {
			//The likeliest real mistake: the previous year's three files (CMS74v15 and so on). Each file of a kind is there, so the pick names no
			//kind as missing and no count; the first refusal is a file's own, which the Code System Importer's status line shows (section 7.2).
			List<TestValueSetRow> listValueSets2026=ValueSetFixtures.MakeManifestRows();
			List<TestValueSetRow> listDirectCodes2026=ValueSetFixtures.MakeDirectCodeRows();
			foreach(TestValueSetRow row in listValueSets2026.Concat(listDirectCodes2026)) {//One version before each 2027 version.
				row.MeasureIds=System.Text.RegularExpressions.Regex.Replace(row.MeasureIds,@"v(\d+)",
					x => "v"+(int.Parse(x.Groups[1].Value,System.Globalization.CultureInfo.InvariantCulture)-1).ToString(System.Globalization.CultureInfo.InvariantCulture));
			}
			ValueSetFixtures.Sheet sheet=new ValueSetFixtures.Sheet();
			sheet.Name="All";
			sheet.ListRows=listValueSets2026;
			List<KeyValuePair<string,byte[]>> listFiles=ValueSetFixtures.MakeRelease(ValueSetFixtures.MakeWorkbook(sheet),listDirectCodes2026);
			foreach(EhrValueSetPick pick in new[] { ValueSetFixtures.ParsePick(listFiles),ValueSetFixtures.ParseEachThenPick(listFiles) }) {
				string refusals=string.Join("\r\n",pick.ListRefusals);
				Assert.IsTrue(pick.IsRefused,refusals);
				Assert.AreEqual(3,pick.ListFiles.Count);
				StringAssert.Contains(pick.ListRefusals[0],"None of the seven measures this program reports is in the file at its 2027 version");
				StringAssert.Contains(pick.ListRefusals[0],EhrValueSetImport.RELEASE_LABEL);
				StringAssert.StartsWith(pick.ListRefusals[0],listFiles[0].Key+": ");
				Assert.IsTrue(pick.ListRefusals.Any(x => x.StartsWith(listFiles[1].Key+": None of the seven measures")),refusals);
				Assert.AreEqual(2,pick.ListRefusals.Count,"Only the two files' own refusals: "+refusals);
				Assert.IsFalse(pick.ListRefusals.Any(x => x.StartsWith("The pick has no file with")),refusals);
				Assert.IsFalse(pick.ListRefusals.Any(x => x.Contains("Files picked: 3")),refusals);
				Assert.IsFalse(pick.ListRefusals.Any(x => x.Contains("Pick the three files of the release together")),refusals);
				Assert.AreEqual(0,pick.ListEhrCodesAll.Count,"Nothing to write.");
			}
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
			Assert.IsFalse(pickVersion.ListEhrCodesDirect.Any(x => x.CodeValue==codeSystolic),"Not a measure this program reports: not imported.");
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
			Assert.AreEqual(1,file.ListDirectCodes.Count(x => x.CodeValue=="TESTD7777"),"One row under each of the seven that lists it; CMS1000v1 is not one.");
			Assert.AreEqual("74v16",file.ListDirectCodes.Single(x => x.CodeValue=="TESTD7777").MeasureId);
			Assert.AreEqual(ValueSetFixtures.COUNT_DIRECT_CODE_ROWS_OTHER_MEASURES,file.CountRowsOtherMeasures,"The rows of other measures, not imported.");
			StringAssert.Contains(EhrValueSetImport.GetReport(file),"rows of other measures or versions (not imported): "
				+ValueSetFixtures.COUNT_DIRECT_CODE_ROWS_OTHER_MEASURES);
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
			StringAssert.Contains(fileNoMeasure.ListRefusals[0],"has no column eCQM ID (or CMS ID), Code System (or Code System OID).");
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

		#region E-20's record (EHR-6a-R)
		///<summary>The names of the office's three files of the 2026-05-14 release, as E-20 recorded them (docs/port-notes/EHR-6.md, section 0).</summary>
		private static readonly string[] _arrayNamesOfTheRelease=new[] {
			"ec_hospip_hospop_cms_20260514.xlsx.zip","drc_cms_20260514.xlsx.zip","cs_versions_cms_20260514.xlsx.zip",
		};

		[TestMethod]
		public void Record_ThePickOfTheThreeFilesByTheirNames_IsAccepted_SevenOfSeven() {
			List<KeyValuePair<string,byte[]>> listRelease=ValueSetFixtures.MakeRelease();
			List<KeyValuePair<string,byte[]>> listFiles=listRelease.Select((x,index) => ValueSetFixtures.Entry(_arrayNamesOfTheRelease[index],x.Value)).ToList();
			EhrValueSetPick pick=ValueSetFixtures.ParsePick(listFiles);
			string report=EhrValueSetImport.GetReport(pick);
			Assert.IsFalse(pick.IsRefused,report);
			CollectionAssert.AreEqual(_arrayNamesOfTheRelease,pick.ListFiles.Select(x => x.FileName).ToList());
			Assert.AreEqual(_arrayNamesOfTheRelease[0],pick.FileValueSets.FileName);
			Assert.AreEqual(_arrayNamesOfTheRelease[1],pick.FileDirectCodes.FileName);
			Assert.AreEqual(_arrayNamesOfTheRelease[2],pick.FileCodeSystemVersions.FileName);
			StringAssert.Contains(report,"7 of 7 measures at the 2027 versions");
			StringAssert.Contains(report,"Direct-reference-code roles resolved: 8 of 8");
			//Each file is a .zip holding one workbook; the value sets have a sheet per measure, named by its CMS ID.
			Assert.AreEqual(EhrValueSetImport.ArrayMeasureIds.Length+1,pick.FileValueSets.ListPartsRead.Count,string.Join("; ",pick.FileValueSets.ListPartsRead));
			foreach(string measureId in EhrValueSetImport.ArrayMeasureIds) {
				Assert.IsTrue(pick.FileValueSets.ListPartsRead.Any(x => x.EndsWith(".xlsx ["+measureId+"]")),measureId);
			}
			Assert.AreEqual(0,pick.ListWarnings.Count,string.Join("\r\n",pick.ListWarnings));
		}

		[TestMethod]
		public void Record_TheHeaderRows_AreFoundBelowTheRowsAboveThem() {
			//Value sets: row 1 empty, the header on row 2. Direct Reference Codes: row 1 empty, row 2 a paragraph, the header on row 3. Code System
			//Versions: row 1 empty, row 2 a title, row 3 empty, row 4 the publication date, the header on row 5. No row above a header is read as a row.
			EhrValueSetPick pick=ValueSetFixtures.ParsePick(ValueSetFixtures.MakeRelease());
			Assert.IsFalse(pick.IsRefused,EhrValueSetImport.GetReport(pick));
			Assert.AreEqual(ValueSetFixtures.MakeDirectCodeRows().Count,pick.FileDirectCodes.CountRowsRead);
			Assert.AreEqual(0,pick.FileDirectCodes.CountRowsSkipped);
			Assert.AreEqual(ValueSetFixtures.MakeCodeSystemVersionRows().Count,pick.FileCodeSystemVersions.CountRowsRead);
			Assert.AreEqual(0,pick.FileCodeSystemVersions.CountRowsSkipped);
			Assert.AreEqual(ValueSetFixtures.MakeCodeSystemVersionRows().Count,pick.FileCodeSystemVersions.ListCodeSystemVersions.Count);
			//The manifest's rows, each once per measure it is listed under, as the sheets list them.
			int countRowsBySheet=ValueSetFixtures.MakeManifestRows().Sum(x => x.MeasureIds.Split(',').Length);
			Assert.AreEqual(countRowsBySheet,pick.FileValueSets.CountRowsRead);
			Assert.AreEqual(EhrValueSetImport.GetManifest().Count,pick.FileValueSets.ListEhrCodes.Count);
			Assert.AreEqual(countRowsBySheet-EhrValueSetImport.GetManifest().Count,pick.FileValueSets.CountRowsMerged);
			//EHR-6a's provisional header of the Code System Versions file (a column Version) is still read.
			EhrValueSetFile fileProvisional=EhrValueSetImport.Parse("csv_TEST.xlsx",ValueSetFixtures.MakeCodeSystemVersionsWorkbook(
				ValueSetFixtures.MakeCodeSystemVersionRows(),ValueSetFixtures.ArrayHeadersCodeSystemVersionsProvisional));
			Assert.IsFalse(fileProvisional.IsRefused,EhrValueSetImport.GetReport(fileProvisional));
			Assert.AreEqual("2.80",fileProvisional.ListCodeSystemVersions.Single(x => x.CodeSystem=="LOINC").Version);
		}

		[TestMethod]
		public void Record_TheValueSetsVersion_IsTheExpansionVersion_NeverTheDefinitionVersion() {
			//Both columns in one row: the Expansion Version is the value set's version (E-20: "eCQM Update 2026-05-14"); the Definition Version,
			//which differs from one value set to the next, never fills it.
			EhrValueSetFile file=EhrValueSetImport.Parse("vs_TEST.xlsx",ValueSetFixtures.MakeByCmsWorkbook(ValueSetFixtures.MakeManifestRows()));
			Assert.IsFalse(file.IsRefused,EhrValueSetImport.GetReport(file));
			Assert.AreEqual("Value-set versions in the file: "+ValueSetFixtures.VALUE_SET_VERSION,file.ListNotes.Single(x => x.StartsWith("Value-set versions")));
			//Without Expansion Version: the Expansion ID.
			string[] arrayHeadersNoExpansionVersion=ValueSetFixtures.ArrayHeadersDefault.Where(x => x!="Expansion Version").ToArray();
			EhrValueSetFile fileId=EhrValueSetImport.Parse("vs_TEST.xlsx",ValueSetFixtures.MakeWorkbook(MakeSheet(arrayHeadersNoExpansionVersion,
				TestValueSetRow.Make(CqmCatalog.GetValueSets()[0].Oid,"SNOMEDCT","TEST0001","CMS68v16"))));
			Assert.AreEqual("Value-set versions in the file: "+ValueSetFixtures.EXPANSION_ID,fileId.ListNotes.Single(x => x.StartsWith("Value-set versions")));
			//Without either: the Definition Version, as EHR-2 read the June 2013 workbook.
			string[] arrayHeadersDefinitionOnly=arrayHeadersNoExpansionVersion.Where(x => x!="Expansion ID").ToArray();
			EhrValueSetFile fileDefinition=EhrValueSetImport.Parse("vs_TEST.xlsx",ValueSetFixtures.MakeWorkbook(MakeSheet(arrayHeadersDefinitionOnly,
				TestValueSetRow.Make(CqmCatalog.GetValueSets()[0].Oid,"SNOMEDCT","TEST0001","CMS68v16"))));
			Assert.AreEqual("Value-set versions in the file: TEST-DEFINITION",fileDefinition.ListNotes.Single(x => x.StartsWith("Value-set versions")));
		}

		private static ValueSetFixtures.Sheet MakeSheet(string[] arrayHeaders,params TestValueSetRow[] arrayRows) {
			ValueSetFixtures.Sheet sheet=new ValueSetFixtures.Sheet();
			sheet.Name="CMS68v16";
			sheet.ListTitleLines.Add("");
			sheet.ArrayHeaders=arrayHeaders;
			sheet.ListRows=arrayRows.ToList();
			return sheet;
		}

		[TestMethod]
		public void Record_CodeSystemNames_MapAcrossTheFilesTwoForms() {
			//E-20's record: SNOMEDCT_US (Direct Reference Codes) and SNOMEDCT (value sets, Code System Versions); "HCPCS Level II" (value sets) and
			//HCPCS (Code System Versions); SOPT (both) for the code list's SOP. By name where a row has no OID; the OID decides first.
			Assert.AreEqual("SNOMEDCT",EhrValueSetImport.GetCodeSystemName("","SNOMEDCT_US"));
			Assert.AreEqual("SNOMEDCT",EhrValueSetImport.GetCodeSystemName("","snomedct_us"));
			Assert.AreEqual("SNOMEDCT",EhrValueSetImport.GetCodeSystemName("2.16.840.1.113883.6.96","SNOMEDCT_US"));
			Assert.AreEqual("HCPCS",EhrValueSetImport.GetCodeSystemName("","HCPCS Level II"));
			Assert.AreEqual("HCPCS",EhrValueSetImport.GetCodeSystemName("2.16.840.1.113883.6.285","HCPCS Level II"));
			Assert.AreEqual("SOP",EhrValueSetImport.GetCodeSystemName("","SOPT"));
			Assert.AreEqual("SOP",EhrValueSetImport.GetCodeSystemName("2.16.840.1.113883.3.221.5","SOPT"));
			Assert.AreEqual("ICD10PCS",EhrValueSetImport.GetCodeSystemName("2.16.840.1.113883.6.4","ICD10PCS"),"Without a table here: the file's name.");
			//A direct reference code under SNOMEDCT_US without its OID takes a SNOMEDCT role.
			List<TestValueSetRow> listRows=ValueSetFixtures.MakeDirectCodeRows();
			TestValueSetRow rowMeds=listRows.Single(x => x.MeasureIds=="CMS68v16" && x.CodeSystem=="SNOMEDCT");
			rowMeds.CodeSystem="SNOMEDCT_US";
			rowMeds.CodeSystemOID="";
			EhrValueSetPick pick=ValueSetFixtures.ParsePick(ValueSetFixtures.MakeRelease(listDirectCodes:listRows));
			Assert.IsTrue(pick.ListRoles.Single(x => x.Role=="Cms68MedsDocumented").IsResolved,EhrValueSetImport.GetReport(pick));
			EhrCode ehrCodeMeds=pick.ListEhrCodesDirect.Single(x => x.CodeValue==rowMeds.CodeValue);
			Assert.AreEqual("SNOMEDCT",ehrCodeMeds.CodeSystem);
			Assert.AreEqual("2.16.840.1.113883.6.96",ehrCodeMeds.CodeSystemOID);
			//The value sets' SOPT and "HCPCS Level II" rows count under the code list's names, as the Code System Versions file's do.
			TestValueSetRow rowPayer=TestValueSetRow.Make("2.16.840.1.114222.4.11.3591","SOP","TEST0001","CMS68v16");
			rowPayer.CodeSystem="SOPT";
			rowPayer.CodeSystemOID="";
			EhrValueSetFile file=EhrValueSetImport.Parse("vs_TEST.xlsx",ValueSetFixtures.MakeByCmsWorkbook(new List<TestValueSetRow> { rowPayer }));
			Assert.AreEqual("SOP",file.ListEhrCodes.Single().CodeSystem);
			Assert.AreEqual("2.16.840.1.113883.3.221.5",file.ListEhrCodes.Single().CodeSystemOID);
		}

		[TestMethod]
		public void Record_TheDirectCodesMeasure_IsItsEcqmId_WithTheEcqmVersionWhereTheIdHasNone() {
			List<TestValueSetRow> listRows=ValueSetFixtures.MakeDirectCodeRows();
			//E-20: eCQM ID CMS68v16 with eCQM Version 16. An ID without its version takes the version column's.
			TestValueSetRow rowMeds=listRows.Single(x => x.MeasureIds=="CMS68v16" && x.CodeSystem=="SNOMEDCT");
			rowMeds.MeasureIds="CMS68";
			rowMeds.MeasureVersion="16";
			EhrValueSetFile file=EhrValueSetImport.Parse("drc_TEST.xlsx",ValueSetFixtures.MakeDirectCodesWorkbook(listRows));
			Assert.IsFalse(file.IsRefused,EhrValueSetImport.GetReport(file));
			Assert.AreEqual("68v16",file.ListDirectCodes.Single(x => x.CodeValue==rowMeds.CodeValue).MeasureId);
			CollectionAssert.AreEqual(EhrValueSetImport.ArrayMeasureIds,file.ListMeasuresMatched);
			//An eCQM ID that names its version keeps it, whatever the version column says.
			rowMeds.MeasureIds="CMS68v15";
			EhrValueSetFile fileOld=EhrValueSetImport.Parse("drc_TEST.xlsx",ValueSetFixtures.MakeDirectCodesWorkbook(listRows));
			Assert.IsFalse(fileOld.ListDirectCodes.Any(x => x.CodeValue==rowMeds.CodeValue),"CMS68v15 is not the catalog's version: not imported.");
			Assert.IsTrue(fileOld.ListWarnings.Any(x => x.Contains("CMS68v15") && x.Contains("CMS68v16")),string.Join("\r\n",fileOld.ListWarnings));
		}

		[TestMethod]
		public void Record_AWorkbookOfInlineStrings_WithoutASharedStringsPart_IsRead() {
			ValueSetFixtures.Sheet sheet=MakeSheet(ValueSetFixtures.ArrayHeadersDefault,TestValueSetRow.Make(CqmCatalog.GetValueSets()[0].Oid,"SNOMEDCT","TEST0001","CMS68v16"));
			sheet.UseInlineStrings=true;
			EhrValueSetFile file=EhrValueSetImport.Parse("vs_TEST.xlsx",ValueSetFixtures.MakeWorkbook(false,sheet));
			Assert.IsFalse(file.IsRefused,EhrValueSetImport.GetReport(file));
			Assert.AreEqual("TEST0001",file.ListEhrCodes.Single().CodeValue);
			Assert.AreEqual("TEST description of TEST0001",file.ListEhrCodes.Single().Description);
		}

		[TestMethod]
		public void Record_AVersionTheValueSetsName_ThatTheCodeSystemVersionsFileDoesNotList_Warns() {
			//Each value-set row names its code's version; the Code System Versions file lists the versions the release used (several for one code
			//system). The synthetic release's rows name versions the file lists: no warning (Record_ThePickOfTheThreeFilesByTheirNames_...).
			List<TestValueSetRow> listRows=ValueSetFixtures.MakeManifestRows();
			listRows[0].CodeSystemVersion="2099-01";
			EhrValueSetPick pick=ValueSetFixtures.ParsePick(ValueSetFixtures.MakeRelease(
				ValueSetFixtures.ZipAsVsacGivesIt("vs_TEST.xlsx",ValueSetFixtures.MakeByCmsWorkbook(listRows))));
			Assert.IsFalse(pick.IsRefused,EhrValueSetImport.GetReport(pick));
			string warning=pick.ListWarnings.Single();
			StringAssert.StartsWith(warning,"The value sets name code-system versions the Code System Versions file does not list");
			StringAssert.Contains(warning,"SNOMEDCT 2099-01 (1 codes)");
			//The versions the codes name, with their counts, go into the pick (and so the release record).
			EhrCodeSystemVersion versionNewest=pick.ListCodeSystemVersionsUsed.Single(x => x.CodeSystem=="SNOMEDCT" && x.Version=="2025-09");
			Assert.AreEqual(EhrValueSetImport.GetManifest().Count-1+pick.FileDirectCodes.ListDirectCodes.Count(x => x.CodeSystem=="SNOMEDCT"),versionNewest.CountCodes);
			StringAssert.Contains(EhrValueSetImport.GetReport(pick.FileValueSets),"Code-system versions the codes to import name: SNOMEDCT 2099-01 (1 codes), 2025-09");
		}

		[TestMethod]
		public void CountCodesNewerThanLoaded_CountsTheCodesOfNewerVersions_WhereTheProgramCanTell() {
			List<EhrCodeSystemVersion> listUsed=new List<EhrCodeSystemVersion> {
				new EhrCodeSystemVersion { CodeSystem="SNOMEDCT",Version="2012-07",CountCodes=1 },
				new EhrCodeSystemVersion { CodeSystem="SNOMEDCT",Version="2024-03",CountCodes=5 },
				new EhrCodeSystemVersion { CodeSystem="SNOMEDCT",Version="2025-09",CountCodes=90 },
				new EhrCodeSystemVersion { CodeSystem="CPT",Version="2006",CountCodes=2 },
				new EhrCodeSystemVersion { CodeSystem="CPT",Version="2026",CountCodes=30 },
				new EhrCodeSystemVersion { CodeSystem="CDT",Version="2025",CountCodes=3 },
			};
			Assert.AreEqual(95,EhrValueSetImport.CountCodesNewerThanLoaded("SNOMEDCT","20231001",listUsed));
			Assert.AreEqual(0,EhrValueSetImport.CountCodesNewerThanLoaded("SNOMEDCT","2025-09-01",listUsed),"As new as the newest the codes name.");
			Assert.AreEqual(30,EhrValueSetImport.CountCodesNewerThanLoaded("CPT","2025",listUsed),"CPT 2006 is older than the table: not counted.");
			Assert.IsNull(EhrValueSetImport.CountCodesNewerThanLoaded("CDT","2024",listUsed),"No mapping for CDT: the program cannot tell.");
			Assert.IsNull(EhrValueSetImport.CountCodesNewerThanLoaded("SNOMEDCT","",listUsed),"Nothing loaded.");
			Assert.IsNull(EhrValueSetImport.CountCodesNewerThanLoaded("LOINC","2.80",listUsed),"No LOINC code imported.");
			Assert.AreEqual("2025-09, 2024-03, 2012-07",EhrValueSetImport.GetVersionsUsedShortText("SNOMEDCT",listUsed.Where(x => x.CodeSystem=="SNOMEDCT").ToList()));
			listUsed.Add(new EhrCodeSystemVersion { CodeSystem="SNOMEDCT",Version="2023-03",CountCodes=1 });
			Assert.AreEqual("2025-09, 2024-03, 2023-03 and 1 older",
				EhrValueSetImport.GetVersionsUsedShortText("SNOMEDCT",listUsed.Where(x => x.CodeSystem=="SNOMEDCT").ToList()));
		}
		#endregion E-20's record (EHR-6a-R)
	}
}
