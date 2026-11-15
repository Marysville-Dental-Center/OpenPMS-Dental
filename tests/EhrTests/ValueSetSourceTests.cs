// The EHR-2 declarations and sources, with no database (docs/specs/ehr-rebuild-plan.md, EHR-2): the manifest's nine measures are the engine's
// (QualityMeasures.GetEMeasureNum and GetEMeasureVersion, read by reflection, so QualityMeasures.cs is not edited) and its OIDs are exactly the
// value-set OIDs the source names; the step's level and the ehrcode declaration, the table class and its CRUD, the schema verifier's lists,
// the adoption rule and the README; the stale comments; and the rule of section 6 that no fixture in tests/EhrTests holds a literal of a code
// from the engine's own lists.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalBusiness;
using OpenPmsDentalBusiness.ForkMigrations;
using OpenPmsDentalBusiness.ForkMigrations.FeatureSteps;

namespace EhrTests {
	[TestClass]
	public class ValueSetSourceTests {
		private const string TABLE="ehrcode";

		#region The manifest
		[TestMethod]
		public void Manifest_TheNineMeasures_AreTheVersionsTheEngineReports() {
			Type typeQualityType=typeof(QualityMeasures).Assembly.GetType("OpenPmsDentalBusiness.QualityType2014",throwOnError:true);
			MethodInfo methodNum=typeof(QualityMeasures).GetMethod("GetEMeasureNum",BindingFlags.NonPublic|BindingFlags.Static);
			MethodInfo methodVersion=typeof(QualityMeasures).GetMethod("GetEMeasureVersion",BindingFlags.NonPublic|BindingFlags.Static);
			Assert.IsNotNull(methodNum);
			Assert.IsNotNull(methodVersion);
			List<string> listEngine=new List<string>();
			foreach(object qualityType in Enum.GetValues(typeQualityType)) {
				string measureId="CMS"+(string)methodNum.Invoke(null,new[] { qualityType })+"v"+(string)methodVersion.Invoke(null,new[] { qualityType });
				if(!listEngine.Contains(measureId)) {
					listEngine.Add(measureId);
				}
			}
			CollectionAssert.AreEquivalent(listEngine,EhrValueSetImport.ArrayMeasureIds);
			CollectionAssert.AreEqual(new[] { "CMS68v3","CMS69v2","CMS74v3","CMS75v2","CMS127v2","CMS138v2","CMS147v2","CMS155v2","CMS165v2" },
				EhrValueSetImport.ArrayMeasureIds,"Section 8.1 of the plan.");
		}

		///<summary>The value-set OIDs the source names: every OID under 2.16.840.1.113883.3 (the arc VSAC's value sets of these measures use) in a
		///string literal of the program's code, except the legacy OID root of OIDInternals and the Source of Payment Typology code system
		///(2.16.840.1.113883.3.221.5, a code system), and the PHDSC Payer value set (2.16.840.1.114222.4.11.3591).</summary>
		[TestMethod]
		public void Manifest_ItsOids_AreExactlyTheValueSetOidsTheSourceNames() {
			Regex regexOid=new Regex(@"^(2\.16\.840\.1\.113883\.3\.[0-9.]*[0-9]|2\.16\.840\.1\.114222\.4\.11\.[0-9.]*[0-9])$");
			Dictionary<string,List<string>> dictOidFiles=new Dictionary<string,List<string>>();
			foreach(string path in TestPaths.ProgramSourceFiles().Where(x => TestPaths.Relative(x).StartsWith(@"OpenPmsDental\",StringComparison.OrdinalIgnoreCase)
				|| TestPaths.Relative(x).StartsWith(@"OpenPmsDentalBusiness\",StringComparison.OrdinalIgnoreCase)))
			{
				if(TestPaths.Relative(path).StartsWith(@"OpenPmsDentalBusiness\Data Interface\EhrCodes.cs",StringComparison.OrdinalIgnoreCase)) {
					continue;//The manifest itself.
				}
				CSharpSource cSharpSource=CSharpSource.Read(File.ReadAllText(path));
				foreach(string literal in cSharpSource.ListStringLiterals) {
					if(!regexOid.IsMatch(literal) || literal.StartsWith(OIDInternals.LegacyOidArc) || literal=="2.16.840.1.113883.3.221.5") {
						continue;
					}
					if(!dictOidFiles.ContainsKey(literal)) {
						dictOidFiles.Add(literal,new List<string>());
					}
					dictOidFiles[literal].Add(TestPaths.Relative(path));
				}
			}
			List<string> listManifest=EhrValueSetImport.GetManifest().Select(x => x.ValueSetOID).ToList();
			List<string> listMissingFromManifest=dictOidFiles.Keys.Where(x => !listManifest.Contains(x)).OrderBy(x => x).ToList();
			List<string> listNotInSource=listManifest.Where(x => !dictOidFiles.ContainsKey(x)).OrderBy(x => x).ToList();
			Assert.AreEqual(0,listMissingFromManifest.Count,"Named in the source, not in the manifest: "
				+string.Join("; ",listMissingFromManifest.Select(x => x+" ("+string.Join(", ",dictOidFiles[x].Distinct())+")")));
			Assert.AreEqual(0,listNotInSource.Count,"In the manifest, named nowhere in the source: "+string.Join("; ",listNotInSource));
			Assert.AreEqual(79,listManifest.Count,"The two surveys counted 79 and 81 (section 7, EHR-2 step 3).");
		}

		[TestMethod]
		public void Manifest_EachItemHasANameACategoryAndOnlyTheNineMeasures_Once() {
			List<EhrValueSetManifestItem> listManifest=EhrValueSetImport.GetManifest();
			Assert.AreEqual(listManifest.Count,listManifest.Select(x => x.ValueSetOID).Distinct().Count(),"No OID twice.");
			List<string> listNine=EhrValueSetImport.ArrayMeasureIds.Select(x => x.Substring(3)).ToList();
			foreach(EhrValueSetManifestItem item in listManifest) {
				Assert.IsFalse(string.IsNullOrWhiteSpace(item.ValueSetName),item.ValueSetOID);
				Assert.IsFalse(item.ValueSetName.EndsWith("Value Set"),item.ValueSetOID+": "+item.ValueSetName);
				Assert.IsFalse(string.IsNullOrWhiteSpace(item.QDMCategory),item.ValueSetOID);
				List<string> listMeasureIds=item.MeasureIds.Split(',').ToList();
				Assert.IsTrue(listMeasureIds.Count>0 && listMeasureIds.All(x => listNine.Contains(x)),item.ValueSetOID+": "+item.MeasureIds);
				Assert.AreEqual(string.Join(",",EhrValueSetImport.SortMeasureIds(listMeasureIds)),item.MeasureIds,"In the engine's order: "+item.ValueSetOID);
			}
			//Every measure has value sets.
			foreach(string measureId in listNine) {
				Assert.IsTrue(listManifest.Any(x => x.MeasureIds.Split(',').Contains(measureId)),measureId);
			}
			//The names the windows give beside their OIDs.
			Assert.AreEqual("Tobacco Use Screening",EhrValueSetImport.GetManifestItem("2.16.840.1.113883.3.526.3.1278").ValueSetName);
			Assert.AreEqual("BMI LOINC Value",EhrValueSetImport.GetManifestItem("2.16.840.1.113883.3.600.1.681").ValueSetName);
			Assert.IsNull(EhrValueSetImport.GetManifestItem("2.16.840.1.113883.3.999999.1"));
		}
		#endregion The manifest

		#region The step, the table and the lists
		[TestMethod]
		public void Step_HasTheFirstEhrLevel_TheRangeIsKept_AndTheLevelsAreValid() {
			List<ForkMigrationStep> listSteps=ForkMigrationRunner.GetSteps();
			List<ForkMigrationStep> listEhrSteps=listSteps.Where(x => x.Level>=2026111401 && x.Level<=2026111410).ToList();
			Assert.AreEqual(1,listEhrSteps.Count);
			Assert.IsInstanceOfType(listEhrSteps[0],typeof(EhrCodeTableStep));
			Assert.AreEqual(2026111401,listEhrSteps[0].Level);
			foreach(ForkMigrationStep step in listSteps) {
				ForkMigrationRunner.ValidateStep(step);
			}
			ForkMigrationRunner.ValidateLevelsUnique(listSteps);
			Assert.IsTrue(listSteps.Where(x => x.Level<2026111401).All(x => x.Level<=2026110801),"Every level on main before EHR-2 is below it.");
		}

		[TestMethod]
		public void Table_IsAForkTable_DeclaredOnce_AndNoLongerHqOnly() {
			CollectionAssert.Contains(ForkTables.GetTableNames(),TABLE);
			Assert.IsInstanceOfType(ForkTables.GetSet(TABLE),typeof(EhrTables));
			Assert.AreEqual("EHR value sets",new EhrTables().Feature);
			Assert.AreEqual("2026111401",new EhrTables().Levels);
			Assert.IsNull(SchemaVerifier.GetReference().GetTable(TABLE),"Not Open Dental schema.");
			CollectionAssert.DoesNotContain(SchemaVerifier.ArrayHqOnlyTables,TABLE);
			Assert.AreEqual(53,SchemaVerifier.ArrayHqOnlyTables.Length);
			foreach(List<SchemaException> listExceptions in new[] { SchemaVerifier.DocumentedExceptions,SchemaVerifier.DocumentedExceptions26_2_0 }) {
				List<SchemaException> listEhrcode=listExceptions.Where(x => x.Table==TABLE).ToList();
				Assert.AreEqual(1,listEhrcode.Count,"Excepted once.");
				Assert.AreEqual(SchemaDifferenceKind.ExtraTable,listEhrcode[0].Kind);
				StringAssert.Contains(listEhrcode[0].Reason,"EHR value sets");
			}
			SchemaTable schemaTable=ForkTables.GetSnapshot().GetTable(TABLE);
			Assert.AreEqual("MyISAM",schemaTable.Engine);
			Assert.AreEqual("utf8mb3",schemaTable.Charset);
			Assert.IsNotNull(schemaTable.GetIndex("ValueSetOID"));
			Assert.IsNotNull(schemaTable.GetIndex("CodeValueCodeSystem"));
			Assert.AreEqual("text NOT NULL",schemaTable.GetColumn("Description").Definition);
			Assert.AreEqual("",EhrCodeTableStep.GetDefinitionError(SchemaTable.Parse(EhrTables.CREATE_EHR_CODE)));
			Assert.AreEqual("",EhrCodeTableStep.GetDefinitionError(null));
		}

		[TestMethod]
		public void TableClass_MapsTheDeclarationColumnByColumn_IsInDbIsNotAColumn() {
			SchemaTable schemaTable=ForkTables.GetSnapshot().GetTable(TABLE);
			Assert.AreEqual(TABLE,CrudTableAttribute.GetTableName(typeof(EhrCode)).ToLowerInvariant());
			List<string> listFields=typeof(EhrCode).GetFields(BindingFlags.Public|BindingFlags.Instance)
				.Where(x => !(x.GetCustomAttributes(typeof(CrudColumnAttribute),true).OfType<CrudColumnAttribute>().FirstOrDefault()?.IsNotDbColumn??false))
				.Select(x => x.Name).ToList();
			CollectionAssert.AreEqual(listFields,schemaTable.Columns.Select(x => x.Name).ToList());
			Assert.IsNull(schemaTable.GetColumn("IsInDb"));
			CrudColumnAttribute crudColumnIsInDb=typeof(EhrCode).GetField("IsInDb").GetCustomAttributes(typeof(CrudColumnAttribute),true).OfType<CrudColumnAttribute>().Single();
			Assert.IsTrue(crudColumnIsInDb.IsNotDbColumn);
			CrudTableAttribute crudTable=typeof(EhrCode).GetCustomAttributes(typeof(CrudTableAttribute),true).OfType<CrudTableAttribute>().Single();
			Assert.IsFalse(crudTable.IsMissingInGeneral,"A real table: the round-trip tests and the DevDbTool schema check cover it.");
			string crud=TestPaths.ReadSource(@"OpenPmsDentalBusiness\Crud\EhrCodeCrud.cs");
			Assert.IsFalse(crud.Contains("IsInDb"),"The CRUD is regenerated without the column.");
			StringAssert.Contains(crud,"//This file is automatically generated.");
			StringAssert.Contains(crud,"public static void InsertMany(List<EhrCode> listEhrCodes)");
		}

		[TestMethod]
		public void AdoptionRule_Readme_AndTheVerifiersList_NameTheTable() {
			string json=TestPaths.ReadSource(@"tools\SyntheticData\allowed-differences.adoption.json");
			StringAssert.Contains(json,"\"Kind\": \"TableAdded\", \"Table\": \"ehrcode\"");
			string readme=TestPaths.ReadSource(@"OpenPmsDentalBusiness\Misc\ForkMigrations\README.md");
			StringAssert.Contains(readme,"| 2026111401 | `EhrSteps.cs`");
			StringAssert.Contains(readme,"The 53 Open Dental HQ-only tables");
			Assert.IsFalse(readme.Contains("The 54 Open Dental HQ-only tables"));
			string verifier=TestPaths.ReadSource(@"OpenPmsDentalBusiness\Misc\ForkMigrations\SchemaVerifier.cs");
			StringAssert.Contains(verifier,"The 53 tables that our 24.3 code declares");
		}

		[TestMethod]
		public void StaleComments_NameTheImport_NotEhrDll() {
			foreach(string path in new[] { @"OpenPmsDental\Forms\FormMedical.cs",@"OpenPmsDental\Forms\FormEhrNotPerformedEdit.cs",@"OpenPmsDental\Forms\FormEhrPatientSmoking.cs" }) {
				string source=TestPaths.ReadSource(path);
				Assert.IsFalse(Regex.IsMatch(source,@"//[^\r\n]*EHR\.dll[^\r\n]*ehrcode list"),path);
				StringAssert.Contains(source,"//Happens until the office imports the value sets (Setup, Chart, EHR, Code System Importer)",path);
			}
			string ehrCode=TestPaths.ReadSource(@"OpenPmsDentalBusiness\TableTypes\EhrCode.cs");
			Assert.IsFalse(ehrCode.Contains("This is not an actual table in the database"));
		}
		#endregion The step, the table and the lists

		#region Fixtures
		///<summary>Section 6 of the plan: no fixture of tests/EhrTests holds a literal of a code from the engine's own lists (the SNOMED CT lab-result
		///codes of EhrMeasures, the two CDT codes of the Demographics measure, the RxNorm and CPT lists of QualityMeasures), so nothing licensed is
		///added and no fixture changes if counsel has those lists moved to the office's import (E-8, E-9). The codes are taken from the engine's
		///string literals at run time: numbers of five or more digits and CDT codes (D and four digits).</summary>
		[TestMethod]
		public void Fixtures_HoldNoLiteralOfACodeFromTheEnginesLists() {
			Regex regexCode=new Regex(@"(?<![0-9A-Za-z.])(D\d{4}|\d{5,9})(?![0-9A-Za-z.])");
			HashSet<string> setEngineCodes=new HashSet<string>(StringComparer.Ordinal);
			foreach(string path in new[] { @"OpenPmsDentalBusiness\Data Interface\EhrMeasures.cs",@"OpenPmsDentalBusiness\Db Multi Table\QualityMeasures.cs" }) {
				CSharpSource cSharpSource=CSharpSource.Read(TestPaths.ReadSource(path));
				foreach(string literal in cSharpSource.ListStringLiterals) {
					if(Regex.IsMatch(literal,@"^\s*[0-9.]+\s*$") && literal.Count(x => x=='.')>1) {
						continue;//An OID.
					}
					foreach(Match match in regexCode.Matches(literal)) {
						setEngineCodes.Add(match.Groups[1].Value);
					}
				}
			}
			Assert.IsTrue(setEngineCodes.Count>50,"The engine's lists were found: "+setEngineCodes.Count);
			Assert.IsTrue(setEngineCodes.Any(x => x.StartsWith("D")),"The CDT codes of the Demographics measure are among them.");
			List<string> listProblems=new List<string>();
			foreach(string path in Directory.GetFiles(TestPaths.Combine(@"tests\EhrTests"),"*.*",SearchOption.AllDirectories)
				.Where(x => !x.Contains(@"\bin\") && !x.Contains(@"\obj\") && (x.EndsWith(".cs") || x.EndsWith(".xml") || x.EndsWith(".html"))))
			{
				string text=File.ReadAllText(path);
				IEnumerable<string> listTexts=path.EndsWith(".cs") ? CSharpSource.Read(text).ListStringLiterals : new List<string> { text };
				foreach(string literal in listTexts) {
					foreach(Match match in regexCode.Matches(literal)) {
						if(setEngineCodes.Contains(match.Groups[1].Value)) {
							listProblems.Add(TestPaths.Relative(path)+": "+match.Groups[1].Value);
						}
					}
				}
			}
			Assert.AreEqual(0,listProblems.Count,"A code of the engine's lists is written in a fixture: "+string.Join("; ",listProblems.Distinct()));
		}

		[TestMethod]
		public void EhrCodes_TheListsConsumers_ReadListtOnce_IntoALocal() {
			//Listt is read again when the codes change (InvalidType.EhrCodes): a method that read it on every pass of a loop could mix two lists.
			string source=TestPaths.ReadSource(@"OpenPmsDentalBusiness\Data Interface\EhrCodes.cs");
			source=Regex.Replace(source,@"/\*.*?\*/","",RegexOptions.Singleline);//The methods Open Dental left commented out.
			source=Regex.Replace(source,@"//[^\r\n]*","");
			int start=source.IndexOf("public class EhrCodes {",StringComparison.Ordinal);
			string classEhrCodes=source.Substring(start,source.IndexOf("public class EhrCodeSystemStatus",StringComparison.Ordinal)-start);
			List<string> listLines=classEhrCodes.Split('\n').Where(x => Regex.IsMatch(x,@"\bListt\s*[\[.]")).Select(x => x.Trim()).ToList();
			Assert.AreEqual(0,listLines.Count,"Listt indexed or called in place of a local copy:\r\n"+string.Join("\r\n",listLines));
			Assert.IsTrue(Regex.Matches(classEhrCodes,@"=\s*Listt\s*;").Count>=7,"Each consumer copies the list to a local once.");
		}

		[TestMethod]
		public void Window_ReadsTheValueSetFileAsAStream_NotWhole() {
			string window=TestPaths.ReadSource(@"OpenPmsDental\Forms\FormCodeSystemsImport.cs");
			string click=window.Substring(window.IndexOf("private void butImportValueSets_Click",StringComparison.Ordinal));
			click=click.Substring(0,click.IndexOf("\r\n\t\t}\r\n",StringComparison.Ordinal));
			Assert.IsFalse(click.Contains("File.ReadAllBytes"),"The file is not read whole into memory.");
			StringAssert.Contains(click,"new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.Read)");
			StringAssert.Contains(click,"EhrValueSetImport.Parse(path,fileStream)");
		}

		[TestMethod]
		public void Fixtures_RefuseACodeThatIsNotATestCode() {
			Expect.Throws<ArgumentException>(() => ValueSetFixtures.MakeWorkbook(new ValueSetFixtures.Sheet {
				ListRows=new List<TestValueSetRow> { TestValueSetRow.Make("2.16.840.1.113883.3.526.3.1278","SNOMEDCT","NOTTEST1") } }));
			Expect.Throws<ArgumentException>(() => ValueSetFixtures.MakeSvs(new List<TestValueSetRow> {
				TestValueSetRow.Make("2.16.840.1.113883.3.526.3.1278","SNOMEDCT","X1") },true));
		}
		#endregion Fixtures
	}
}
