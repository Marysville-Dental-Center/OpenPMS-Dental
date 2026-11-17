// The value-set readers and checks of EHR-2 (docs/specs/ehr-rebuild-plan.md, EHR-2 and section 8.2), with no database: synthetic Excel
// workbooks, SVS files and .zip files of them (Support/ValueSetFixtures.cs; made-up TEST codes, the real value-set and code-system OIDs), the
// header matching, the merge of a value set listed under several measures, the code systems by OID, the measure-version check (the June 2013
// versions imported, the 2014 EP update's versions warned about or refused), an SVS file without Group elements, and files that are refused.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalBusiness;

namespace EhrTests {
	[TestClass]
	public class ValueSetParseTests {
		private const string OID_TOBACCO_SCREENING="2.16.840.1.113883.3.526.3.1278";
		private const string OID_OFFICE_VISIT="2.16.840.1.113883.3.464.1003.101.12.1001";
		private const string OID_PREGNANCY="2.16.840.1.113883.3.526.3.378";
		private const string OID_SNOMEDCT="2.16.840.1.113883.6.96";

		///<summary>The versions VSAC's 2014 EP update (expansion profile "MU2 EP Update 2014-05-30", 2015 reporting) gives the nine measures: each
		///one version later than the June 2013 release (for example CMS74v4).</summary>
		private static readonly string[] _arrayMeasureIds2014Update=new string[] {
			"CMS68v4","CMS69v3","CMS74v4","CMS75v3","CMS127v3","CMS138v3","CMS147v3","CMS155v3","CMS165v3",
		};

		[TestInitialize]
		public void Begin() {
			//Lans.g returns the English text in en-US and needs no database there.
			System.Threading.Thread.CurrentThread.CurrentCulture=new System.Globalization.CultureInfo("en-US");
		}

		private static ValueSetFixtures.Sheet MakeSheet(string name,params TestValueSetRow[] arrayRows) {
			ValueSetFixtures.Sheet sheet=new ValueSetFixtures.Sheet();
			sheet.Name=name;
			sheet.ListRows=arrayRows.ToList();
			return sheet;
		}

		#region Workbook
		[TestMethod]
		public void Workbook_ReadsTheRows_UnderATitle_WithTheirFieldsAndCodeSystems() {
			ValueSetFixtures.Sheet sheet=MakeSheet("CMS138v2",
				TestValueSetRow.Make(OID_TOBACCO_SCREENING,"SNOMEDCT","TEST0001"),
				TestValueSetRow.Make(OID_OFFICE_VISIT,"CPT","TEST0002"),
				TestValueSetRow.Make(OID_PREGNANCY,"ICD9CM","TEST0003"));
			sheet.ListTitleLines.Add("TEST eCQM value sets, June 2013 EP release");
			sheet.ListTitleLines.Add("");
			sheet.ListRows[2].Description="TEST & <description> with \"quotes\"";
			EhrValueSetFile file=EhrValueSetImport.Parse("ep_value_sets.xlsx",ValueSetFixtures.MakeWorkbook(sheet));
			Assert.IsFalse(file.IsRefused,EhrValueSetImport.GetReport(file));
			Assert.AreEqual(3,file.ListEhrCodes.Count,EhrValueSetImport.GetReport(file));
			EhrCode ehrCode=file.ListEhrCodes.Single(x => x.CodeValue=="TEST0001");
			Assert.AreEqual(OID_TOBACCO_SCREENING,ehrCode.ValueSetOID);
			Assert.AreEqual("Tobacco Use Screening",ehrCode.ValueSetName);
			Assert.AreEqual("Risk Category/Assessment",ehrCode.QDMCategory);
			Assert.AreEqual("138v2",ehrCode.MeasureIds,"As EhrCode.MeasureIds writes them: without CMS.");
			Assert.AreEqual("SNOMEDCT",ehrCode.CodeSystem);
			Assert.AreEqual(OID_SNOMEDCT,ehrCode.CodeSystemOID);
			Assert.AreEqual("TEST description of TEST0001",ehrCode.Description,"Rich-text runs joined, the phonetic run left out.");
			Assert.AreEqual("CPT",file.ListEhrCodes.Single(x => x.CodeValue=="TEST0002").CodeSystem);
			Assert.AreEqual("TEST & <description> with \"quotes\"",file.ListEhrCodes.Single(x => x.CodeValue=="TEST0003").Description);
			Assert.AreEqual(3,file.CountRowsRead);
			Assert.AreEqual(64,file.Sha256.Length);
			CollectionAssert.Contains(file.ListPartsRead,"ep_value_sets.xlsx [CMS138v2]");
		}

		[TestMethod]
		public void Workbook_HeadersMatch_IgnoringCaseAndSpaces_InAnyOrder_AndInlineStrings() {
			ValueSetFixtures.Sheet sheet=MakeSheet("Value Sets",TestValueSetRow.Make(OID_TOBACCO_SCREENING,"SNOMEDCT","TEST0001"));
			sheet.ArrayHeaders=new[] { "CODE","codesystem","  value set  OID ","Descriptor","cms id","VALUESETNAME" };
			sheet.UseInlineStrings=true;
			EhrValueSetFile file=EhrValueSetImport.Parse("vs.xlsx",ValueSetFixtures.MakeWorkbook(sheet));
			Assert.IsFalse(file.IsRefused,EhrValueSetImport.GetReport(file));
			EhrCode ehrCode=file.ListEhrCodes.Single();
			Assert.AreEqual("TEST0001",ehrCode.CodeValue);
			Assert.AreEqual(OID_TOBACCO_SCREENING,ehrCode.ValueSetOID);
			Assert.AreEqual(OID_SNOMEDCT,ehrCode.CodeSystemOID,"No Code System OID column: the code list's OID for the name.");
			Assert.AreEqual("Risk Category/Assessment",ehrCode.QDMCategory,"No QDM Category column: the manifest's.");
		}

		[TestMethod]
		public void Workbook_AValueSetUnderSeveralMeasures_IsOneRow_WithTheNineMeasuresItIsListedUnder() {
			TestValueSetRow rowA=TestValueSetRow.Make(OID_OFFICE_VISIT,"CPT","TEST0001","CMS74v3");
			TestValueSetRow rowB=TestValueSetRow.Make(OID_OFFICE_VISIT,"CPT","TEST0001","CMS165v2");
			TestValueSetRow rowC=TestValueSetRow.Make(OID_OFFICE_VISIT,"CPT","TEST0001","CMS1000v1, CMS75v2");
			EhrValueSetFile file=EhrValueSetImport.Parse("vs.xlsx",ValueSetFixtures.MakeWorkbook(MakeSheet("CMS74v3",rowA),MakeSheet("CMS165v2",rowB),
				MakeSheet("Other",rowC)));
			Assert.IsFalse(file.IsRefused,EhrValueSetImport.GetReport(file));
			Assert.AreEqual(1,file.ListEhrCodes.Count);
			Assert.AreEqual(2,file.CountRowsMerged);
			Assert.AreEqual("74v3,75v2,165v2",file.ListEhrCodes[0].MeasureIds,"The nine this program reports, in the engine's order; not the others.");
			CollectionAssert.Contains(file.ListMeasureIdsInFile,"1000v1","The version check sees every measure.");
			//The same code in another value set, or of another code system, is a row of its own.
			EhrValueSetFile file2=EhrValueSetImport.Parse("vs.xlsx",ValueSetFixtures.MakeWorkbook(MakeSheet("A",rowA,
				TestValueSetRow.Make(OID_TOBACCO_SCREENING,"CPT","TEST0001"),TestValueSetRow.Make(OID_OFFICE_VISIT,"HCPCS","TEST0001"))));
			Assert.AreEqual(3,file2.ListEhrCodes.Count);
		}

		[TestMethod]
		public void Workbook_ASheetWithoutAHeaderRow_IsSkippedAndNamed() {
			ValueSetFixtures.Sheet sheetReadMe=new ValueSetFixtures.Sheet();
			sheetReadMe.Name="Read Me";
			sheetReadMe.ArrayHeaders=null;
			sheetReadMe.ListTitleLines.AddRange(new[] { "TEST read me","This workbook lists value sets.","Code" });
			EhrValueSetFile file=EhrValueSetImport.Parse("vs.xlsx",ValueSetFixtures.MakeWorkbook(sheetReadMe,
				MakeSheet("CMS138v2",TestValueSetRow.Make(OID_TOBACCO_SCREENING,"SNOMEDCT","TEST0001"))));
			Assert.IsFalse(file.IsRefused,EhrValueSetImport.GetReport(file));
			Assert.AreEqual(1,file.ListEhrCodes.Count);
			Assert.IsTrue(file.ListPartsSkipped.Any(x => x.Contains("[Read Me]") && x.Contains("no header row")),string.Join("\r\n",file.ListPartsSkipped));
			StringAssert.Contains(EhrValueSetImport.GetReport(file),"Not read: vs.xlsx [Read Me]");
		}

		[TestMethod]
		public void Workbook_AHeaderRowWithoutTheRequiredColumns_IsRefused_NamingThem() {
			ValueSetFixtures.Sheet sheet=MakeSheet("CMS138v2",TestValueSetRow.Make(OID_TOBACCO_SCREENING,"SNOMEDCT","TEST0001"));
			sheet.ArrayHeaders=new[] { "Value Set Name","Code","Descriptor" };
			EhrValueSetFile file=EhrValueSetImport.Parse("vs.xlsx",ValueSetFixtures.MakeWorkbook(sheet));
			Assert.IsTrue(file.IsRefused);
			Assert.AreEqual(0,file.ListEhrCodes.Count);
			string refusal=string.Join("\r\n",file.ListRefusals);
			StringAssert.Contains(refusal,"[CMS138v2]");
			StringAssert.Contains(refusal,"Value Set OID");
			StringAssert.Contains(refusal,"Code System (or Code System OID)");
			Assert.IsFalse(refusal.Contains("Code,"),"Code is there: "+refusal);
			StringAssert.Contains(EhrValueSetImport.GetReport(file),"Nothing was imported");
			//A workbook with no value-set sheet at all is refused too.
			ValueSetFixtures.Sheet sheetReadMe=new ValueSetFixtures.Sheet();
			sheetReadMe.ArrayHeaders=null;
			sheetReadMe.ListTitleLines.Add("TEST");
			Assert.IsTrue(EhrValueSetImport.Parse("vs.xlsx",ValueSetFixtures.MakeWorkbook(sheetReadMe)).IsRefused);
		}

		[TestMethod]
		public void Workbook_RowsWithoutAnOidOrACode_AreSkippedAndCounted() {
			TestValueSetRow rowNoOid=TestValueSetRow.Make(OID_TOBACCO_SCREENING,"SNOMEDCT","TEST0002");
			rowNoOid.ValueSetOID="";
			TestValueSetRow rowNoCode=TestValueSetRow.Make(OID_TOBACCO_SCREENING,"SNOMEDCT","");
			EhrValueSetFile file=EhrValueSetImport.Parse("vs.xlsx",ValueSetFixtures.MakeWorkbook(MakeSheet("A",
				TestValueSetRow.Make(OID_TOBACCO_SCREENING,"SNOMEDCT","TEST0001"),rowNoOid,rowNoCode)));
			Assert.IsFalse(file.IsRefused,EhrValueSetImport.GetReport(file));
			Assert.AreEqual(1,file.ListEhrCodes.Count);
			Assert.AreEqual(3,file.CountRowsRead);
			Assert.AreEqual(2,file.CountRowsSkipped);
			StringAssert.Contains(EhrValueSetImport.GetReport(file),"without a value-set OID or a code (skipped): 2");
		}

		[TestMethod]
		public void Workbook_AValueLongerThanItsColumn_IsNotImported_AndWarned_ADescriptionMayBeLong() {
			TestValueSetRow rowLongName=TestValueSetRow.Make(OID_TOBACCO_SCREENING,"SNOMEDCT","TEST0002");
			rowLongName.ValueSetName=new string('N',256);
			TestValueSetRow rowLongDescription=TestValueSetRow.Make(OID_TOBACCO_SCREENING,"SNOMEDCT","TEST0003");
			rowLongDescription.Description="TEST "+new string('D',600);
			EhrValueSetFile file=EhrValueSetImport.Parse("vs.xlsx",ValueSetFixtures.MakeWorkbook(MakeSheet("A",
				TestValueSetRow.Make(OID_TOBACCO_SCREENING,"SNOMEDCT","TEST0001"),rowLongName,rowLongDescription)));
			Assert.AreEqual(1,file.CountRowsTooLong);
			CollectionAssert.AreEquivalent(new[] { "TEST0001","TEST0003" },file.ListEhrCodes.Select(x => x.CodeValue).ToList());
			Assert.IsTrue(file.ListWarnings.Any(x => x.Contains("longer than 255")));
		}
		#endregion Workbook

		#region SVS
		[TestMethod]
		public void Svs_WithGroups_ReadsTheConcepts_TheCategoryAndTheMeasures() {
			List<TestValueSetRow> listRows=new List<TestValueSetRow> {
				TestValueSetRow.Make(OID_TOBACCO_SCREENING,"SNOMEDCT","TEST0001"),
				TestValueSetRow.Make(OID_TOBACCO_SCREENING,"SNOMEDCT","TEST0002"),
				TestValueSetRow.Make(OID_OFFICE_VISIT,"CPT","TEST0003"),
			};
			listRows[0].QDMCategory="TEST category from the file";
			listRows[1].QDMCategory="TEST category from the file";
			EhrValueSetFile file=EhrValueSetImport.Parse("vs.xml",ValueSetFixtures.MakeSvs(listRows,hasGroups:true));
			Assert.IsFalse(file.IsRefused,EhrValueSetImport.GetReport(file));
			Assert.AreEqual(3,file.ListEhrCodes.Count);
			EhrCode ehrCode=file.ListEhrCodes.First(x => x.CodeValue=="TEST0001");
			Assert.AreEqual("TEST category from the file",ehrCode.QDMCategory,"The file's category wins over the manifest's.");
			Assert.AreEqual("138v2",ehrCode.MeasureIds);
			Assert.AreEqual("Tobacco Use Screening",ehrCode.ValueSetName);
			Assert.AreEqual("74v3,75v2,127v2,138v2,147v2,155v2,165v2",file.ListEhrCodes.First(x => x.CodeValue=="TEST0003").MeasureIds);
			Assert.IsTrue(file.AreVersionsChecked);
			Assert.IsTrue(file.ListNotes.Any(x => x.Contains("20130614")),"The value-set versions are reported.");
		}

		[TestMethod]
		public void Svs_WithoutGroups_Imports_WithTheManifestsMeasures_AndSaysTheVersionsWereNotChecked() {
			List<TestValueSetRow> listRows=new List<TestValueSetRow> {
				TestValueSetRow.Make(OID_TOBACCO_SCREENING,"SNOMEDCT","TEST0001"),
				TestValueSetRow.Make("2.16.840.1.113883.3.999999.1","SNOMEDCT","TEST0002"),
			};
			EhrValueSetFile file=EhrValueSetImport.Parse("vs.xml",ValueSetFixtures.MakeSvs(listRows,hasGroups:false));
			Assert.IsFalse(file.IsRefused,EhrValueSetImport.GetReport(file));
			Assert.IsFalse(file.AreVersionsChecked);
			Assert.AreEqual("138v2",file.ListEhrCodes.Single(x => x.CodeValue=="TEST0001").MeasureIds,"From the manifest.");
			Assert.AreEqual("Risk Category/Assessment",file.ListEhrCodes.Single(x => x.CodeValue=="TEST0001").QDMCategory,"From the manifest.");
			Assert.AreEqual("",file.ListEhrCodes.Single(x => x.CodeValue=="TEST0002").MeasureIds,"Not in the manifest: no measures.");
			Assert.AreEqual(1,file.CountOtherValueSets);
			Assert.IsTrue(file.ListNotes.Any(x => x.Contains("measure versions could not be checked")),string.Join("\r\n",file.ListNotes));
			StringAssert.Contains(EhrValueSetImport.GetReport(file),"measure versions could not be checked");
		}

		[TestMethod]
		public void Svs_WithoutGroups_NoneOfWhoseOidsIsInTheManifest_IsRefused() {
			List<TestValueSetRow> listRows=new List<TestValueSetRow> {
				TestValueSetRow.Make("2.16.840.1.113883.3.999999.1","SNOMEDCT","TEST0001"),
				TestValueSetRow.Make("2.16.840.1.113883.3.999999.2","LOINC","TEST0002"),
			};
			EhrValueSetFile file=EhrValueSetImport.Parse("vs.xml",ValueSetFixtures.MakeSvs(listRows,hasGroups:false));
			Assert.IsTrue(file.IsRefused);
			StringAssert.Contains(string.Join(" ",file.ListRefusals),"none of its value sets is one the measures of this program use");
		}
		#endregion SVS

		#region Measure versions
		[TestMethod]
		public void Versions_TheJune2013Release_HasAllNine_AndEveryManifestValueSet() {
			EhrValueSetFile file=EhrValueSetImport.Parse("vs.xlsx",ValueSetFixtures.MakeWorkbook(MakeSheet("All",ValueSetFixtures.MakeManifestRows().ToArray())));
			Assert.IsFalse(file.IsRefused,EhrValueSetImport.GetReport(file));
			CollectionAssert.AreEqual(EhrValueSetImport.ArrayMeasureIds,file.ListMeasuresMatched);
			Assert.AreEqual(0,file.ListWarnings.Count,string.Join("\r\n",file.ListWarnings));
			Assert.AreEqual(EhrValueSetImport.GetManifest().Count,file.ListManifestOidsFound.Count);
			Assert.AreEqual(0,file.ListManifestOidsMissing.Count);
			StringAssert.Contains(EhrValueSetImport.GetReport(file),"Measures at the versions this program reports: 9 of 9");
		}

		[TestMethod]
		public void Versions_AMeasureAtThe2014UpdatesVersion_GivesTheVersionWarning() {
			List<TestValueSetRow> listRows=new List<TestValueSetRow> {
				TestValueSetRow.Make(OID_TOBACCO_SCREENING,"SNOMEDCT","TEST0001","CMS138v2"),
				TestValueSetRow.Make("2.16.840.1.113883.3.464.1003.125.12.1002","CDT","TEST0002","CMS74v4"),
			};
			EhrValueSetFile file=EhrValueSetImport.Parse("vs.xlsx",ValueSetFixtures.MakeWorkbook(MakeSheet("A",listRows.ToArray())));
			Assert.IsFalse(file.IsRefused,EhrValueSetImport.GetReport(file));
			CollectionAssert.AreEqual(new[] { "CMS138v2" },file.ListMeasuresMatched);
			Assert.IsTrue(file.ListWarnings.Any(x => x.Contains("CMS74v4") && x.Contains("CMS74v3")),string.Join("\r\n",file.ListWarnings));
			Assert.IsTrue(file.ListWarnings.Any(x => x=="CMS68v3 is not in the file."),string.Join("\r\n",file.ListWarnings));
			Assert.IsTrue(file.ListWarnings.Any(x => x.Contains("value sets the measures use are not in the file")));
			string report=EhrValueSetImport.GetReport(file);
			StringAssert.Contains(report,"Value sets the measures use that are not in the file:");
			StringAssert.Contains(report,"2.16.840.1.113883.3.526.3.1170 Tobacco User (138v2)");
		}

		[TestMethod]
		public void Versions_TheWhole2014Update_IsRefused_NoneOfTheNineAtItsVersion() {
			List<TestValueSetRow> listRows=ValueSetFixtures.MakeManifestRows();
			for(int i=0;i<listRows.Count;i++) {
				listRows[i].MeasureIds=_arrayMeasureIds2014Update[i%_arrayMeasureIds2014Update.Length];
			}
			foreach(bool isSvs in new[] { false,true }) {
				EhrValueSetFile file=isSvs ? EhrValueSetImport.Parse("vs.xml",ValueSetFixtures.MakeSvs(listRows,hasGroups:true))
					: EhrValueSetImport.Parse("vs.xlsx",ValueSetFixtures.MakeWorkbook(MakeSheet("A",listRows.ToArray())));
				Assert.IsTrue(file.IsRefused,"svs="+isSvs);
				string refusal=string.Join(" ",file.ListRefusals);
				StringAssert.Contains(refusal,"None of the nine measures this program reports is in the file at the version it reports");
				StringAssert.Contains(refusal,"CMS74v4");
				StringAssert.Contains(refusal,EhrValueSetImport.RELEASE_LABEL);
			}
		}

		[TestMethod]
		public void ParseMeasureIds_ReadsCmsIdsAndShortIds_IgnoresOtherText() {
			CollectionAssert.AreEqual(new[] { "68v3","69v2","147v2","1000v12" },
				EhrValueSetImport.ParseMeasureIds("CMS68v3, cms69V2; 147v2 CMS 1000 v 12, NQF 0419, TEST74, v3, CMS68v3"));
			CollectionAssert.AreEqual(new string[0],EhrValueSetImport.ParseMeasureIds(""));
			CollectionAssert.AreEqual(new[] { "74v3","165v2","1000v1" },EhrValueSetImport.SortMeasureIds(new[] { "1000v1","165v2","74v3","165v2" }));
		}
		#endregion Measure versions

		#region Code systems
		[TestMethod]
		public void CodeSystems_ByTheirHl7Oids_AsTheEngineNamesThem() {
			Dictionary<string,string> dictExpected=new Dictionary<string,string> {
				{ "2.16.840.1.113883.6.96","SNOMEDCT" },
				{ "2.16.840.1.113883.6.103","ICD9CM" },
				{ "2.16.840.1.113883.6.104","ICD9CM" },
				{ "2.16.840.1.113883.6.90","ICD10CM" },
				{ "2.16.840.1.113883.6.1","LOINC" },
				{ "2.16.840.1.113883.6.88","RXNORM" },
				{ "2.16.840.1.113883.6.12","CPT" },
				{ "2.16.840.1.113883.6.13","CDT" },
				{ "2.16.840.1.113883.6.285","HCPCS" },
				{ "2.16.840.1.113883.12.292","CVX" },
				{ "2.16.840.1.113883.6.238","CDCREC" },
				{ "2.16.840.1.113883.3.221.5","SOP" },
				{ "2.16.840.1.113883.5.1","AdministrativeSex" },
				{ "2.16.840.1.113883.18.2","AdministrativeSex" },
			};
			foreach(KeyValuePair<string,string> pair in dictExpected) {
				Assert.AreEqual(pair.Value,EhrValueSetImport.GetCodeSystemName(pair.Key,"TEST name"),pair.Key);
			}
			Assert.AreEqual("ICD10PCS",EhrValueSetImport.GetCodeSystemName("2.16.840.1.113883.6.4","ICD10 PCS"),"Not in the code list: the file's name.");
			Assert.AreEqual("SNOMEDCT",EhrValueSetImport.GetCodeSystemName("","snomedct"),"No OID: the code list's name for the file's.");
			Assert.AreEqual("AdministrativeSex",EhrValueSetImport.GetCodeSystemName("","AdministrativeGender"));
			//The codesystem table's OIDs (the convert scripts) agree with the importer's.
			string convert=TestPaths.ReadSource(@"OpenPmsDentalBusiness\Misc\ConvertDatabases3.cs");
			foreach(string codeSystem in EhrCodes.ArrayCodeSystems.Where(x => x!="AdministrativeSex")) {
				StringAssert.Contains(convert,"'"+codeSystem+"','"+EhrValueSetImport.GetCodeSystemOid(codeSystem)+"'",codeSystem);
			}
		}

		[TestMethod]
		public void CodeSystems_WithoutATable_AreImportedAndNamedInTheReport() {
			TestValueSetRow rowPcs=TestValueSetRow.Make("2.16.840.1.113883.3.526.3.1083","SNOMEDCT","TEST0002");
			rowPcs.CodeSystem="ICD10PCS";
			rowPcs.CodeSystemOID="2.16.840.1.113883.6.4";
			EhrValueSetFile file=EhrValueSetImport.Parse("vs.xlsx",ValueSetFixtures.MakeWorkbook(MakeSheet("A",
				TestValueSetRow.Make(OID_TOBACCO_SCREENING,"SNOMEDCT","TEST0001"),rowPcs)));
			Assert.IsFalse(file.IsRefused,EhrValueSetImport.GetReport(file));
			Assert.AreEqual("ICD10PCS",file.ListEhrCodes.Single(x => x.CodeValue=="TEST0002").CodeSystem);
			CollectionAssert.AreEqual(new[] { "ICD10PCS: 1" },file.ListCodeSystemsWithoutTable);
			StringAssert.Contains(EhrValueSetImport.GetReport(file),"Code systems this program has no table for");
		}
		#endregion Code systems

		#region Zip and refused files
		[TestMethod]
		public void Zip_OfAWorkbookAndAnSvsFile_ReadsBoth_AndSkipsTheRest() {
			byte[] arrayWorkbook=ValueSetFixtures.MakeWorkbook(MakeSheet("CMS138v2",TestValueSetRow.Make(OID_TOBACCO_SCREENING,"SNOMEDCT","TEST0001")));
			byte[] arraySvs=ValueSetFixtures.MakeSvs(new List<TestValueSetRow> { TestValueSetRow.Make(OID_OFFICE_VISIT,"CPT","TEST0002") },hasGroups:true);
			byte[] arrayInnerZip=ValueSetFixtures.MakeZip(ValueSetFixtures.Entry("inner.xml",arraySvs));
			byte[] arrayZip=ValueSetFixtures.MakeZip(
				ValueSetFixtures.Entry("EP_2013/by_cms_id.xlsx",arrayWorkbook),
				ValueSetFixtures.Entry("EP_2013/svs/office_visit.xml",arraySvs),
				ValueSetFixtures.Entry("EP_2013/readme.txt",Encoding.UTF8.GetBytes("TEST read me")),
				ValueSetFixtures.Entry("EP_2013/notes.xml",Encoding.UTF8.GetBytes("<notes>TEST</notes>")),
				ValueSetFixtures.Entry("EP_2013/inner.zip",arrayInnerZip));
			EhrValueSetFile file=EhrValueSetImport.Parse("ep_2013.zip",arrayZip);
			Assert.IsFalse(file.IsRefused,EhrValueSetImport.GetReport(file));
			CollectionAssert.AreEquivalent(new[] { "TEST0001","TEST0002" },file.ListEhrCodes.Select(x => x.CodeValue).ToList());
			string skipped=string.Join("\r\n",file.ListPartsSkipped);
			StringAssert.Contains(skipped,"readme.txt");
			StringAssert.Contains(skipped,"notes.xml");
			StringAssert.Contains(skipped,"inner.zip: a .zip inside the .zip is not read.");
			//A workbook named .zip is read as a workbook.
			Assert.IsFalse(EhrValueSetImport.Parse("download.zip",arrayWorkbook).IsRefused);
			//A .zip with nothing to read is refused.
			EhrValueSetFile fileEmpty=EhrValueSetImport.Parse("other.zip",ValueSetFixtures.MakeZip(ValueSetFixtures.Entry("a.txt",Encoding.UTF8.GetBytes("TEST"))));
			Assert.IsTrue(fileEmpty.IsRefused);
			StringAssert.Contains(string.Join(" ",fileEmpty.ListRefusals),"no VSAC workbook");
		}

		[TestMethod]
		public void RefusedFiles_EmptyUnreadableNotSvsAndXmlWithADtd() {
			Assert.IsTrue(EhrValueSetImport.Parse("vs.xlsx",new byte[0]).IsRefused);
			Assert.IsTrue(EhrValueSetImport.Parse("vs.txt",Encoding.UTF8.GetBytes("TEST,not,a,workbook")).IsRefused);
			EhrValueSetFile fileDamaged=EhrValueSetImport.Parse("vs.xlsx",new byte[] { 0x50,0x4B,0x03,0x04,1,2,3,4,5,6,7,8 });
			Assert.IsTrue(fileDamaged.IsRefused);
			StringAssert.Contains(fileDamaged.ListRefusals[0],"could not be read");
			Assert.IsTrue(EhrValueSetImport.Parse("vs.xml",Encoding.UTF8.GetBytes("<?xml version=\"1.0\"?><notes>TEST</notes>")).IsRefused);
			//An entity that would read a file or expand: the reader prohibits a DTD, so the file is refused and nothing is resolved.
			string xmlDtd="<?xml version=\"1.0\"?><!DOCTYPE x [<!ENTITY e SYSTEM \"file:///c:/windows/win.ini\">]>"
				+"<RetrieveMultipleValueSetsResponse><DescribedValueSet ID=\"2.16.840.1.113883.3.526.3.1278\"><ConceptList>"
				+"<Concept code=\"TEST0001\" codeSystem=\"2.16.840.1.113883.6.96\" displayName=\"&e;\"/></ConceptList></DescribedValueSet></RetrieveMultipleValueSetsResponse>";
			EhrValueSetFile fileDtd=EhrValueSetImport.Parse("vs.xml",Encoding.UTF8.GetBytes(xmlDtd));
			Assert.IsTrue(fileDtd.IsRefused);
			Assert.AreEqual(0,fileDtd.ListEhrCodes.Count);
		}

		[TestMethod]
		public void Report_NamesTheFileItsHashAndTheCounts() {
			byte[] arrayBytes=ValueSetFixtures.MakeWorkbook(MakeSheet("A",TestValueSetRow.Make(OID_TOBACCO_SCREENING,"SNOMEDCT","TEST0001")));
			EhrValueSetFile file=EhrValueSetImport.Parse(@"X:\TEST folder\vs.xlsx",arrayBytes);
			Assert.AreEqual("vs.xlsx",file.FileName,"The name only, never the folder.");
			string expectedHash;
			using(System.Security.Cryptography.SHA256 sha256=System.Security.Cryptography.SHA256.Create()) {
				expectedHash=string.Concat(sha256.ComputeHash(arrayBytes).Select(x => x.ToString("x2")));
			}
			Assert.AreEqual(expectedHash,file.Sha256);
			string report=EhrValueSetImport.GetReport(file);
			StringAssert.Contains(report,"File: vs.xlsx");
			StringAssert.Contains(report,"SHA-256: "+expectedHash);
			StringAssert.Contains(report,"Codes to import: 1 in 1 value sets");
			StringAssert.Contains(report,"Value sets the measures use, found: 1 of "+EhrValueSetImport.GetManifest().Count);
		}
		#endregion Zip and refused files

		#region Fix round: measure IDs, column lengths and the limits of a 32-bit process
		///<summary>The bytes allocated in this application domain while the action runs (the reader's working memory, not only what it keeps).</summary>
		private static long GetBytesAllocated(Action action) {
			AppDomain.MonitoringIsEnabled=true;
			long countBefore=AppDomain.CurrentDomain.MonitoringTotalAllocatedMemorySize;
			action();
			return AppDomain.CurrentDomain.MonitoringTotalAllocatedMemorySize-countBefore;
		}

		[TestMethod]
		public void Workbook_AValueSetListedUnder64Measures_ImportsEveryCode_WithTheNineItListsAsMeasureIds() {
			//The supplemental data elements (Payer, race, ethnicity, sex) are listed under every measure of the June 2013 EP release, about 64 of
			//them: the nine this program reports and made-up others here.
			const string OID_PAYER="2.16.840.1.114222.4.11.3591";
			const string OID_OTHER="2.16.840.1.113883.3.999999.7";
			List<string> listMeasures=EhrValueSetImport.ArrayMeasureIds.ToList();
			for(int i=0;listMeasures.Count<64;i++) {
				listMeasures.Add("CMS"+(1000+i)+"v1");
			}
			List<TestValueSetRow> listRows=new List<TestValueSetRow>();
			foreach(string measureId in listMeasures) {//As the release sorted by CMS ID lists them: each code once under each measure.
				for(int j=1;j<=3;j++) {
					listRows.Add(TestValueSetRow.Make(OID_PAYER,"SOP","TEST000"+j,measureId));
				}
				listRows.Add(TestValueSetRow.Make(OID_OTHER,"SNOMEDCT","TEST0004",measureId));
			}
			//A manifest value set the file lists only under other measures, and a made-up one likewise.
			listRows.Add(TestValueSetRow.Make(OID_TOBACCO_SCREENING,"SNOMEDCT","TEST0005","CMS1000v1"));
			listRows.Add(TestValueSetRow.Make("2.16.840.1.113883.3.999999.8","SNOMEDCT","TEST0006","CMS1001v1"));
			EhrValueSetFile file=EhrValueSetImport.Parse("vs.xlsx",ValueSetFixtures.MakeWorkbook(MakeSheet("All",listRows.ToArray())));
			Assert.IsFalse(file.IsRefused,EhrValueSetImport.GetReport(file));
			Assert.AreEqual(0,file.CountRowsTooLong,EhrValueSetImport.GetReport(file));
			CollectionAssert.AreEquivalent(new[] { "TEST0001","TEST0002","TEST0003","TEST0004","TEST0005","TEST0006" },
				file.ListEhrCodes.Select(x => x.CodeValue).ToList(),"Every code imported.");
			Assert.IsTrue(file.ListEhrCodes.All(x => x.MeasureIds.Length<=EhrValueSetImport.MAX_COLUMN_LENGTH),
				string.Join("\r\n",file.ListEhrCodes.Select(x => x.MeasureIds.Length+" "+x.MeasureIds)));
			const string NINE="68v3,69v2,74v3,75v2,127v2,138v2,147v2,155v2,165v2";
			foreach(string codeValue in new[] { "TEST0001","TEST0002","TEST0003","TEST0004" }) {
				Assert.AreEqual(NINE,file.ListEhrCodes.Single(x => x.CodeValue==codeValue).MeasureIds,"Only the nine this program reports: "+codeValue);
			}
			Assert.AreEqual("138v2",file.ListEhrCodes.Single(x => x.CodeValue=="TEST0005").MeasureIds,"None of the nine left: the manifest's.");
			Assert.AreEqual("",file.ListEhrCodes.Single(x => x.CodeValue=="TEST0006").MeasureIds,"None of the nine, not in the manifest: none.");
			//The version check still sees every measure of the file.
			Assert.AreEqual(64,file.ListMeasureIdsInFile.Count);
			CollectionAssert.AreEqual(EhrValueSetImport.ArrayMeasureIds,file.ListMeasuresMatched);
			Assert.AreEqual(64*4-4,file.CountRowsMerged);
		}

		[TestMethod]
		public void Svs_ADescriptionPastTheTextColumn_IsNotImported_CountedByItsUtf8Bytes() {
			//The description is a text column: 65,535 bytes, which utf8mb3 fills with 21,845 characters of three bytes each.
			TestValueSetRow rowFull=TestValueSetRow.Make(OID_TOBACCO_SCREENING,"SNOMEDCT","TEST0001");
			rowFull.Description=new string('€',21845);
			TestValueSetRow rowOneCharacterMore=TestValueSetRow.Make(OID_TOBACCO_SCREENING,"SNOMEDCT","TEST0002");
			rowOneCharacterMore.Description=new string('€',21846);
			TestValueSetRow rowAsciiOneByteMore=TestValueSetRow.Make(OID_TOBACCO_SCREENING,"SNOMEDCT","TEST0003");
			rowAsciiOneByteMore.Description="TEST "+new string('D',65536-5);
			List<TestValueSetRow> listRows=new List<TestValueSetRow> { rowFull,rowOneCharacterMore,rowAsciiOneByteMore,
				TestValueSetRow.Make(OID_TOBACCO_SCREENING,"SNOMEDCT","TEST0004") };
			EhrValueSetFile file=EhrValueSetImport.Parse("vs.xml",ValueSetFixtures.MakeSvs(listRows,hasGroups:true));
			Assert.IsFalse(file.IsRefused,EhrValueSetImport.GetReport(file));
			Assert.AreEqual(2,file.CountRowsTooLong);
			CollectionAssert.AreEquivalent(new[] { "TEST0001","TEST0004" },file.ListEhrCodes.Select(x => x.CodeValue).ToList());
			Assert.IsTrue(file.ListWarnings.Any(x => x.Contains("longer than 255") && x.Contains("65,535 bytes")),string.Join("\r\n",file.ListWarnings));
		}

		[TestMethod]
		public void Workbook_ManyCellsInColumnZZZ_AreNotHeld_AndASheetOfNothingElseIsRefusedWithAMessage() {
			//Each row holds one cell in column ZZZ (index 18,277): a few bytes of the file each, which a reader that pads every row up to its last
			//column turns into about 18,000 entries per row.
			ValueSetFixtures.Sheet sheetWide=new ValueSetFixtures.Sheet();
			sheetWide.Name="Wide";
			sheetWide.ArrayHeaders=null;
			sheetWide.ColumnExtraCell="ZZZ";
			sheetWide.CountExtraRows=3000;
			byte[] arrayBytes=ValueSetFixtures.MakeWorkbook(sheetWide);
			Assert.IsTrue(arrayBytes.Length<300*1024,arrayBytes.Length.ToString());
			EhrValueSetFile file=null;
			long countAllocated=GetBytesAllocated(() => file=EhrValueSetImport.Parse("wide.xlsx",arrayBytes));
			Assert.IsTrue(file.IsRefused);
			string refusal=string.Join(" ",file.ListRefusals);
			StringAssert.Contains(refusal,"The file has no value-set codes to import.");
			Assert.IsFalse(refusal.Contains("could not be read"),"Refused for what it holds, not for running out of memory: "+refusal);
			StringAssert.Contains(EhrValueSetImport.GetReport(file),"to the right of column IV");
			Assert.IsTrue(countAllocated<64L*1024*1024,"Allocated while reading: "+countAllocated.ToString("N0"));
		}

		[TestMethod]
		public void Workbook_CellsToTheRightOfColumnIV_AreIgnoredAndCounted_TheRowsUnderTheHeaderImport() {
			ValueSetFixtures.Sheet sheet=MakeSheet("CMS138v2",TestValueSetRow.Make(OID_TOBACCO_SCREENING,"SNOMEDCT","TEST0001"),
				TestValueSetRow.Make(OID_TOBACCO_SCREENING,"SNOMEDCT","TEST0002"));
			sheet.ColumnExtraCell="ZZZ";
			sheet.CountExtraRows=3000;
			byte[] arrayBytes=ValueSetFixtures.MakeWorkbook(sheet);
			EhrValueSetFile file=null;
			long countAllocated=GetBytesAllocated(() => file=EhrValueSetImport.Parse("vs.xlsx",arrayBytes));
			Assert.IsFalse(file.IsRefused,EhrValueSetImport.GetReport(file));
			CollectionAssert.AreEquivalent(new[] { "TEST0001","TEST0002" },file.ListEhrCodes.Select(x => x.CodeValue).ToList());
			Assert.AreEqual(2,file.CountRowsRead,"The rows that hold only an ignored cell are empty rows.");
			//The header row, the two rows and the 3,000 others.
			StringAssert.Contains(EhrValueSetImport.GetReport(file),"3003 cells to the right of column IV");
			Assert.IsTrue(countAllocated<64L*1024*1024,"Allocated while reading: "+countAllocated.ToString("N0"));
		}

		///<summary>An SVS file of the given size: the response element around that many spaces (no value set).</summary>
		private static void AddLargeSvs(StoredZip storedZip,string name,long countSpaces,long sizeClaimed=-1) {
			storedZip.AddDeflated(name,Encoding.UTF8.GetBytes("<?xml version=\"1.0\"?><RetrieveMultipleValueSetsResponse>"),(byte)' ',countSpaces,
				Encoding.UTF8.GetBytes("</RetrieveMultipleValueSetsResponse>"),sizeClaimed);
		}

		[TestMethod]
		public void Zip_AnEntryThatInflatesPastThePartLimit_IsRefusedWithAMessage_WhateverItsHeaderSays() {
			byte[] arrayWorkbook=ValueSetFixtures.MakeWorkbook(MakeSheet("CMS138v2",TestValueSetRow.Make(OID_TOBACCO_SCREENING,"SNOMEDCT","TEST0001")));
			foreach(bool isSizeUnderstated in new[] { false,true }) {
				StoredZip storedZip=new StoredZip();
				storedZip.Add("EP_2013/by_cms_id.xlsx",arrayWorkbook);
				AddLargeSvs(storedZip,"EP_2013/large.xml",65L*1024*1024,isSizeUnderstated ? 1000 : -1);
				byte[] arrayZip=storedZip.ToArray();
				Assert.IsTrue(arrayZip.Length<1024*1024,"A small file: "+arrayZip.Length);
				EhrValueSetFile file=EhrValueSetImport.Parse("ep_2013.zip",arrayZip);
				Assert.IsTrue(file.IsRefused,"understated="+isSizeUnderstated+"\r\n"+EhrValueSetImport.GetReport(file));
				string refusal=string.Join(" ",file.ListRefusals);
				Assert.AreEqual(0,file.ListEhrCodes.Count);
				if(isSizeUnderstated) {
					//.NET's ZipArchiveEntry stream stops at the size the header claims, so the part is refused as unreadable XML (its root element
					//is cut off at the claimed 1,000 bytes), not as too large: nothing past the claimed size is read. On the .NET Framework the
					//stream inflated past the claim and the part limit refused it. The lead accepted this on 2026-11-16 (docs/port-notes/S3b-5.md).
					StringAssert.Contains(refusal,"could not be read",refusal);
					StringAssert.Contains(refusal,"Line 1, position 1001",refusal);
					continue;
				}
				StringAssert.Contains(refusal,"larger, uncompressed, than the importer reads","understated="+isSizeUnderstated);
				StringAssert.Contains(refusal,"64 MB");
				StringAssert.Contains(refusal,"EP_2013/large.xml");
			}
		}

		[TestMethod]
		public void Zip_PartsThatTogetherInflatePastTheTotalLimit_AreRefusedWithAMessage() {
			StoredZip storedZip=new StoredZip();
			storedZip.Add("EP_2013/a_by_cms_id.xlsx",ValueSetFixtures.MakeWorkbook(MakeSheet("CMS138v2",TestValueSetRow.Make(OID_TOBACCO_SCREENING,"SNOMEDCT","TEST0001"))));
			for(int i=1;i<=5;i++) {
				AddLargeSvs(storedZip,"EP_2013/svs/large"+i+".xml",60L*1024*1024);//Each under the part limit, 300 MB in all.
			}
			EhrValueSetFile file=EhrValueSetImport.Parse("ep_2013.zip",storedZip.ToArray());
			Assert.IsTrue(file.IsRefused,EhrValueSetImport.GetReport(file));
			string refusal=string.Join(" ",file.ListRefusals);
			StringAssert.Contains(refusal,"256 MB");
			Assert.AreEqual(0,file.ListEhrCodes.Count);
		}

		[TestMethod]
		public void Limits_AreWhatA32BitProcessHolds() {
			//The program is an x86 process: the limits are far above the June 2013 EP package and far below what such a process can hold.
			Assert.IsTrue(EhrValueSetImport.MAX_FILE_BYTES<=64L*1024*1024,EhrValueSetImport.MAX_FILE_BYTES.ToString());
			Assert.IsTrue(EhrValueSetImport.MAX_TOTAL_BYTES<=256L*1024*1024,EhrValueSetImport.MAX_TOTAL_BYTES.ToString());
			Assert.IsTrue(EhrValueSetImport.MAX_ROWS<=1000000,EhrValueSetImport.MAX_ROWS.ToString());
		}
		#endregion Fix round: measure IDs, column lengths and the limits of a 32-bit process
	}
}
