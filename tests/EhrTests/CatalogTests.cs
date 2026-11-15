// The 2027 measure catalog of EHR-6a (docs/specs/ehr-2027-measures-plan.md, sections 4.1, 5.2, 7.2, 7.3 and 11.1), with no database: the seven
// measures with their IDs, stewards, population groups and strata; the 68 value sets of the measures' CQL, each measure's list held to the
// record of the port note (docs/port-notes/EHR-6.md) by its SHA-256; the copyright and disclaimer statements held to their recorded SHA-256;
// the direct-reference-code roles and the pick lists; and the guard that no string of the catalog's source looks like a code of a code system.
// With CQM_SPECS_DIR naming a folder of CMS's 2027 packages (one folder per CMS ID with its x/ folder, as downloaded; never in the repository),
// one more test reads the HQMF and CQL files and compares every fact; without it that test is inconclusive.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalBusiness;

namespace EhrTests {
	[TestClass]
	public class CatalogTests {
		private const string PATH_CATALOG=@"OpenPmsDentalBusiness\Db Multi Table\Cqm\CqmCatalog.cs";

		///<summary>Section 4.1: CMS ID and version, steward, set ID, version-specific ID, episode-based, lower is better.</summary>
		private static readonly string[][] _arrayMeasures=new string[][] {
			new[] { "CMS68v16",CqmCatalog.STEWARD_CMS,"9a032d9c-3d9b-11e1-8634-00237d5bf174","a58f1abe-de03-4ea2-a3ec-48d2a06b3bc5","episode","" },
			new[] { "CMS69v15",CqmCatalog.STEWARD_CMS,"9a031bb8-3d9b-11e1-8634-00237d5bf174","d62e8d03-9687-4097-8351-1a2bad79c9b9","","" },
			new[] { "CMS74v16",CqmCatalog.STEWARD_CMS,"0b81b6ba-3b30-41bf-a2f3-95bdc9f558f2","a79e99a5-8583-4041-8fa4-30c2d98c1d8a","","" },
			new[] { "CMS75v15",CqmCatalog.STEWARD_CMS,"61947125-4376-4a7b-ab7a-ac2be9bd9138","1e4f85db-b6c0-43d8-9d2b-594960bc5ac6","","lower" },
			new[] { "CMS138v15",CqmCatalog.STEWARD_NCQA,"e35791df-5b25-41bb-b260-673337bc44a8","f8fd38f2-e26c-48ad-a920-eff52f483956","","" },
			new[] { "CMS155v15",CqmCatalog.STEWARD_NCQA,"0b63f730-25d6-4248-b11f-8c09c66a04eb","63b284d2-a9b2-4f6d-86f9-40650e49a17b","","" },
			new[] { "CMS165v15",CqmCatalog.STEWARD_NCQA,"abdc37cc-bac6-4156-9b91-d1be2c8b7268","b83d59fa-65cd-4ccb-9889-70fbc81bcc6e","","" },
		};

		///<summary>The record of docs/port-notes/EHR-6.md: per measure, the number of distinct value sets its package's CQL files declare outside
		///CQMCommonQDM, and the SHA-256 (ASCII) of their OIDs sorted by number and joined by line feeds, as the writer took them from the downloads.</summary>
		private static readonly string[][] _arrayValueSetRecord=new string[][] {
			new[] { "CMS68","6","3f09743df7b69257e1df560be8295a5627df24ccccfead18181da5d05d415a66" },
			new[] { "CMS69","22","2d80774475ab8681c861bd9a9dfda6319e3f67957a2482d6e1536acec98293a4" },
			new[] { "CMS74","10","d44309819b069ecd5383cc5410678de04e6abe5ac524a645ca5f84bbd0f41509" },
			new[] { "CMS75","10","202eb614d7ec7dc0f482b8a745b4c945c68a5903ac0d694e734da7938d65542f" },
			new[] { "CMS138","32","1a4388142e3ba267d9fb8c6e6aa5638712f247dcc22ebc7358782acf1be178b0" },
			new[] { "CMS155","21","6b70976fba8f215bceea0dbf45bfb4d0c29fab0300d052c0fb7a986ea7a9f113" },
			new[] { "CMS165","31","f13008198a5e26c47532c9d453bc71c3862d814b3bd2f1a2e6e6085e924e1fc9" },
		};

		///<summary>The record of docs/port-notes/EHR-6.md: the SHA-256 (UTF-8) of each measure's copyright and disclaimer statement, as its HQMF
		///file holds them (section 11.1, E-38).</summary>
		private static readonly string[][] _arrayStatementRecord=new string[][] {
			new[] { "CMS68","7bbfc6f482b64ad00b0eb84751e4918b361bae70aef41b87719246a9935a9c52","84cd5619877e6b07e917f613f3454e63bcb97dfecf2ae9a6f13794616f6af1d8" },
			new[] { "CMS69","41ab2c79ed3c19069415b86a5516ecdd491e806436dd1e2449e3b3a0a1afe62f","baccfc321e12c54b01fa8459b18d330c8c116aa6c42708d4bbe2cf810a3eb9cc" },
			new[] { "CMS74","07a6edc274b6757f2823f02b9fca743c98f275445d033beefae2ba20b3aa4930","4d82961204383d1b691dcd8037a9fae17dcb8037787ae685c0026de092ded2a5" },
			new[] { "CMS75","fb472279d6d0d16c9abed7f32275a4e7594de2cf8d784aadaaca0bf2eb41ea15","4d82961204383d1b691dcd8037a9fae17dcb8037787ae685c0026de092ded2a5" },
			new[] { "CMS138","a2468c0577e7c5ff6b3f6366f982849f7f760e69fe59f4b584ffe17d99717914","32f0807b74dfc688f332af48c2f9fb6501b926a384e40486ac75ce2e9b675806" },
			new[] { "CMS155","835263c6306f513098681f83bb18b52ebd6e0069325fcc77e208be877869c88b","32f0807b74dfc688f332af48c2f9fb6501b926a384e40486ac75ce2e9b675806" },
			new[] { "CMS165","835263c6306f513098681f83bb18b52ebd6e0069325fcc77e208be877869c88b","32f0807b74dfc688f332af48c2f9fb6501b926a384e40486ac75ce2e9b675806" },
		};

		///<summary>What a code of a code system looks like (section 10.1): a run of 5 to 18 digits (SNOMED CT, CPT, RxNorm), the LOINC pattern of
		///digits (three to seven: an age range such as 1-5 is not one), a hyphen and a check digit, the ICD-10 pattern of a letter and two digits (with a decimal part), and a letter with four digits or
		///four digits with a letter (CDT, HCPCS Level II, CPT Category II).</summary>
		private static readonly Regex[] _arrayRegexCodeLike=new Regex[] {
			new Regex(@"(?<![0-9A-Za-z])\d{5,18}(?![0-9A-Za-z])"),
			new Regex(@"(?<![0-9A-Za-z.-])\d{3,7}-\d(?![0-9A-Za-z.-])"),
			new Regex(@"(?<![0-9A-Za-z])[A-Z]\d{2}(?:\.[0-9A-Z]{1,4})?(?![0-9A-Za-z])"),
			new Regex(@"(?<![0-9A-Za-z])(?:[A-Z]\d{4}|\d{4}[A-Z])(?![0-9A-Za-z])"),
		};

		private static List<int> OidKey(string oid) {
			return oid.Split('.').Select(x => int.Parse(x,CultureInfo.InvariantCulture)).ToList();
		}

		private static int CompareOids(string oidA,string oidB) {
			List<int> listA=OidKey(oidA);
			List<int> listB=OidKey(oidB);
			for(int i=0;i<Math.Min(listA.Count,listB.Count);i++) {
				if(listA[i]!=listB[i]) {
					return listA[i].CompareTo(listB[i]);
				}
			}
			return listA.Count.CompareTo(listB.Count);
		}

		private static string GetOidListSha256(IEnumerable<string> listOids) {
			List<string> listSorted=listOids.Distinct().ToList();
			listSorted.Sort(CompareOids);
			return CqmCatalog.GetSha256(string.Join("\n",listSorted));
		}

		#region The measures
		[TestMethod]
		public void Catalog_HoldsTheSevenMeasuresOfSection41_WithTheirIdsAndStewards() {
			List<CqmMeasure> listMeasures=CqmCatalog.GetMeasures();
			CollectionAssert.AreEqual(_arrayMeasures.Select(x => x[0]).ToList(),listMeasures.Select(x => x.MeasureId).ToList());
			foreach(string[] row in _arrayMeasures) {
				CqmMeasure measure=CqmCatalog.GetMeasure(row[0]);
				Assert.IsNotNull(measure,row[0]);
				Assert.AreEqual(row[1],measure.Steward,row[0]);
				Assert.AreEqual(row[2],measure.SetId,row[0]);
				Assert.AreEqual(row[3],measure.VersionSpecificId,row[0]);
				Assert.AreEqual(row[4]=="episode",measure.IsEpisodeBased,row[0]);
				Assert.AreEqual(row[5]=="lower",measure.IsLowerBetter,row[0]);
				Assert.AreEqual(measure.Version+".0.000",measure.VersionNumber,row[0]);
				Assert.AreEqual("Proportion",measure.Scoring,row[0]);
				StringAssert.StartsWith(measure.LibraryName,measure.CmsId,row[0]);
				Assert.IsFalse(string.IsNullOrWhiteSpace(measure.Title),row[0]);
				Assert.AreEqual(measure.Steward==CqmCatalog.STEWARD_NCQA,measure.IsNcqa,row[0]);
				Assert.AreEqual(measure.IsNcqa ? CqmCatalog.STEWARD_NCQA : CqmCatalog.DEVELOPER_AIR,measure.Developer,row[0]);
			}
			Assert.AreEqual(3,listMeasures.Count(x => x.IsNcqa),"CMS138, CMS155 and CMS165 (owner decision 5).");
			Assert.AreEqual("Outcome",CqmCatalog.GetMeasure("CMS75").MeasureType);
			Assert.AreEqual("Intermediate Outcome",CqmCatalog.GetMeasure("CMS165").MeasureType);
			//Found with or without its version, never at another version.
			Assert.AreSame(null,CqmCatalog.GetMeasure("CMS74v15"));
			Assert.AreEqual("CMS74v16",CqmCatalog.GetMeasure("cms74").MeasureId);
			Assert.AreEqual("74v16",CqmCatalog.GetMeasure("CMS74v16").MeasureIdShort);
			Assert.IsNull(CqmCatalog.GetMeasure("CMS127"),"Retired for eligible clinicians (section 4.2).");
			Assert.IsNull(CqmCatalog.GetMeasure("CMS147"),"Retired for eligible clinicians (section 4.2).");
			Assert.IsNull(CqmCatalog.GetMeasure(""));
		}

		[TestMethod]
		public void Catalog_PopulationGroupsAndStrata_AreTheHqmfFiles_WithTheirIds() {
			Dictionary<string,int> dictGroups=new Dictionary<string,int> { { "CMS68",1 },{ "CMS69",1 },{ "CMS74",1 },{ "CMS75",1 },{ "CMS138",3 },{ "CMS155",3 },{ "CMS165",1 } };
			Dictionary<string,int> dictStrata=new Dictionary<string,int> { { "CMS74",3 },{ "CMS155",2 } };
			Regex regexGuid=new Regex("^[0-9A-Fa-f]{8}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{12}$");
			foreach(CqmMeasure measure in CqmCatalog.GetMeasures()) {
				Assert.AreEqual(dictGroups[measure.CmsId],measure.ListPopulationGroups.Count,measure.CmsId);
				List<string> listIds=new List<string>();
				for(int i=0;i<measure.ListPopulationGroups.Count;i++) {
					CqmPopulationGroup group=measure.ListPopulationGroups[i];
					Assert.AreEqual(i+1,group.Number);
					listIds.Add(group.Id);
					dictStrata.TryGetValue(measure.CmsId,out int countStrata);
					Assert.AreEqual(countStrata,group.ListStrata.Count,measure.CmsId);
					for(int j=0;j<group.ListStrata.Count;j++) {
						Assert.AreEqual("Stratification "+(j+1),group.ListStrata[j].Define);
						StringAssert.Contains(group.ListStrata[j].Description,"Patients age");
						listIds.Add(group.ListStrata[j].Id);
					}
					CollectionAssert.AreEqual(new[] { "IPOP","DENOM" },group.ListPopulations.Take(2).Select(x => x.Code).ToList(),measure.CmsId);
					Assert.AreEqual("Initial Population",group.ListPopulations[0].Define);
					Assert.IsTrue(group.ListPopulations.Any(x => x.Code=="NUMER"),measure.CmsId);
					listIds.AddRange(group.ListPopulations.Select(x => x.Id));
				}
				Assert.IsTrue(listIds.All(x => regexGuid.IsMatch(x)),measure.CmsId+": "+string.Join(", ",listIds));
				Assert.AreEqual(listIds.Count,listIds.Distinct(StringComparer.OrdinalIgnoreCase).Count(),measure.CmsId+": no ID twice.");
			}
			//Section 6's exclusions and exceptions.
			Assert.IsFalse(CqmCatalog.GetMeasure("CMS68").ListPopulationGroups[0].ListPopulations.Any(x => x.Code=="DENEX"));
			Assert.AreEqual("Denominator Exceptions",CqmCatalog.GetMeasure("CMS68").ListPopulationGroups[0].ListPopulations.Single(x => x.Code=="DENEXCEP").Define);
			Assert.IsTrue(CqmCatalog.GetMeasure("CMS69").ListPopulationGroups[0].ListPopulations.Any(x => x.Code=="DENEXCEP"));
			CollectionAssert.AreEqual(new[] { "Numerator 1","Numerator 2","Numerator 3" },
				CqmCatalog.GetMeasure("CMS138").ListPopulationGroups.Select(x => x.ListPopulations.Single(y => y.Code=="NUMER").Define).ToList());
			CollectionAssert.AreEqual(new[] { "Denominator 1","Denominator 2","Denominator 3" },
				CqmCatalog.GetMeasure("CMS138").ListPopulationGroups.Select(x => x.ListPopulations.Single(y => y.Code=="DENOM").Define).ToList());
		}
		#endregion The measures

		#region The value sets
		[TestMethod]
		public void Catalog_ValueSets_AreThe68OfTheCqlValuesetLines_EachMeasuresListAsRecorded() {
			List<CqmValueSet> listValueSets=CqmCatalog.GetValueSets();
			Assert.AreEqual(68,listValueSets.Count,"68 distinct value sets outside CQMCommonQDM, counted from the valueset lines only (section 7.2).");
			Assert.AreEqual(68,listValueSets.Select(x => x.Oid).Distinct().Count(),"No OID twice.");
			Assert.AreEqual(72,listValueSets.Count+CqmCatalog.GetValueSetsDeclaredButUnused().Count,"72 distinct value sets in all the CQL files.");
			List<string> listOrdered=listValueSets.Select(x => x.Oid).ToList();
			listOrdered.Sort(CompareOids);
			CollectionAssert.AreEqual(listOrdered,listValueSets.Select(x => x.Oid).ToList(),"Ordered by OID.");
			foreach(string[] row in _arrayValueSetRecord) {
				CqmMeasure measure=CqmCatalog.GetMeasure(row[0]);
				Assert.AreEqual(int.Parse(row[1],CultureInfo.InvariantCulture),measure.ListValueSetOids.Count,row[0]);
				Assert.AreEqual(row[2],GetOidListSha256(measure.ListValueSetOids),row[0]+": the record of the port note.");
				//The value set's own list of measures agrees.
				CollectionAssert.AreEquivalent(listValueSets.Where(x => x.ListCmsIds.Contains(row[0])).Select(x => x.Oid).ToList(),measure.ListValueSetOids,row[0]);
			}
			foreach(CqmValueSet valueSet in listValueSets) {
				Assert.IsFalse(string.IsNullOrWhiteSpace(valueSet.Name),valueSet.Oid);
				Assert.IsFalse(string.IsNullOrWhiteSpace(valueSet.QDMCategory),valueSet.Oid);
				Assert.IsFalse(string.IsNullOrWhiteSpace(valueSet.Datatypes),valueSet.Oid);
				Assert.IsTrue(valueSet.ListCmsIds.Count>0 && valueSet.ListCmsIds.All(x => CqmCatalog.GetMeasure(x)!=null),valueSet.Oid);
			}
			//The supplemental data elements are every measure's; "Encounter Inpatient" is there because HospiceQDM and CMS165 declare it.
			foreach(string oid in new[] { "2.16.840.1.114222.4.11.836","2.16.840.1.114222.4.11.837","2.16.840.1.114222.4.11.3591","2.16.840.1.113762.1.4.1021.121" }) {
				Assert.AreEqual(7,CqmCatalog.GetValueSet(oid).ListCmsIds.Count,oid);
			}
			CqmValueSet valueSetInpatient=CqmCatalog.GetValueSet("2.16.840.1.113883.3.666.5.307");
			Assert.AreEqual("Encounter Inpatient",valueSetInpatient.Name);
			CollectionAssert.DoesNotContain(valueSetInpatient.ListCmsIds,"CMS68","CMS68 includes no HospiceQDM.");
			Assert.AreEqual("Encounter",CqmCatalog.GetValueSet("2.16.840.1.113883.3.464.1003.125.12.1003").QDMCategory);
			Assert.AreEqual("Condition/Diagnosis/Problem",CqmCatalog.GetValueSet("2.16.840.1.113883.3.464.1003.125.12.1004").QDMCategory);
			Assert.AreEqual("Attribute",CqmCatalog.GetValueSet("2.16.840.1.113762.1.4.1248.429").QDMCategory,"A negation rationale.");
		}

		[TestMethod]
		public void Catalog_DeclaredButUnused_AreCqmCommonsFourHospitalSets() {
			List<KeyValuePair<string,string>> listUnused=CqmCatalog.GetValueSetsDeclaredButUnused();
			CollectionAssert.AreEquivalent(new[] { "Intensive Care Unit","Outpatient Surgery Service","Observation Services","Emergency Department Visit" },
				listUnused.Select(x => x.Value).ToList());
			foreach(KeyValuePair<string,string> pair in listUnused) {
				Assert.IsNull(CqmCatalog.GetValueSet(pair.Key),pair.Value);
				Assert.IsNull(EhrValueSetImport.GetManifestItem(pair.Key),pair.Value);
			}
		}

		[TestMethod]
		public void Catalog_PickLists_AreValueSetsOfTheMeasures_ForTheWindowsOfSection8() {
			List<CqmValueSet> listPickLists=CqmCatalog.GetPickListValueSets();
			Assert.AreEqual(17,listPickLists.Count);
			Assert.AreEqual(listPickLists.Count,listPickLists.Select(x => x.Oid).Distinct().Count());
			Assert.IsTrue(listPickLists.All(x => x.IsPickList && x.PickListUse!=""));
			Assert.AreEqual(17,CqmCatalog.GetValueSets().Count(x => x.IsPickList));
			//The tobacco and counseling sets the windows use (section 7.2), the BMI follow-up and its medications, dialysis, pregnancy, the reasons.
			foreach(string oid in new[] { "2.16.840.1.113883.3.526.3.1278","2.16.840.1.113883.3.526.3.1170","2.16.840.1.113883.3.526.3.1189",
				"2.16.840.1.113883.3.526.3.509","2.16.840.1.113883.3.526.3.1190","2.16.840.1.113883.3.464.1003.195.12.1003",
				"2.16.840.1.113883.3.464.1003.118.12.1035","2.16.840.1.113883.3.600.1.1525","2.16.840.1.113883.3.600.1.1528",
				"2.16.840.1.113883.3.600.1.1527","2.16.840.1.113883.3.526.3.1561","2.16.840.1.113883.3.526.3.1562",
				"2.16.840.1.113883.3.464.1003.109.12.1013","2.16.840.1.113883.3.526.3.378","2.16.840.1.113762.1.4.1248.429",
				"2.16.840.1.113883.3.526.3.1007","2.16.840.1.113883.3.526.3.1582" })
			{
				Assert.IsTrue(listPickLists.Any(x => x.Oid==oid),oid);
			}
		}
		#endregion The value sets

		#region The direct reference codes
		[TestMethod]
		public void Catalog_DirectCodeRoles_AreSection73s_ByMeasureCodeSystemAndMeaning() {
			List<CqmDirectCodeRole> listRoles=CqmCatalog.GetDirectCodeRoles();
			CollectionAssert.AreEqual(new[] { "Cms68MedsDocumented","Cms69Bmi","Cms69Virtual","Cms138QualifyingVisitCpt","Cms138CounselingDx","Cms165Systolic",
				"Cms165Diastolic","Cms165KidneyTransplantDx" },listRoles.Select(x => x.Role).ToList());
			foreach(CqmDirectCodeRole role in listRoles) {
				Assert.IsNotNull(CqmCatalog.GetMeasure(role.CmsId),role.Role);
				CollectionAssert.Contains(CqmCatalog.GetMeasure(role.CmsId).ListDirectCodeRoles,role.Role);
				Assert.IsFalse(string.IsNullOrWhiteSpace(role.Meaning),role.Role);
				Assert.AreEqual(role.CodeSystem=="LOINC",role.DisplayName!="","Only the LOINC roles are told apart by a display name (E-23): "+role.Role);
				Assert.AreEqual(role.Role=="Cms138QualifyingVisitCpt" ? 4 : 1,role.CountExpected,role.Role);
				Assert.AreEqual(CqmCatalog.DIRECT_CODE_KEY_PREFIX+role.CmsId+":"+role.Role,CqmCatalog.GetDirectCodeKey(role.CmsId,role.Role));
			}
			CollectionAssert.AreEqual(new[] { "Body mass index (BMI) [Ratio]","Systolic blood pressure","Diastolic blood pressure" },
				listRoles.Where(x => x.DisplayName!="").Select(x => x.DisplayName).ToList(),"The three LOINC display names, as the specifications print them.");
			Assert.AreEqual("drc:CMS74:Listed",CqmCatalog.GetDirectCodeKey("CMS74",null));
			Assert.IsTrue(CqmCatalog.IsDirectCodeKey("drc:CMS69:Cms69Bmi"));
			Assert.IsFalse(CqmCatalog.IsDirectCodeKey("2.16.840.1.113883.3.526.3.1278"));
			Assert.IsNull(CqmCatalog.GetDirectCodeRole("Cms147Influenza"));
		}
		#endregion The direct reference codes

		#region The statements
		[TestMethod]
		public void Catalog_Statements_EqualTheirRecordedSha256_ForAllSevenMeasures() {
			foreach(string[] row in _arrayStatementRecord) {
				CqmMeasure measure=CqmCatalog.GetMeasure(row[0]);
				Assert.AreEqual(row[1],CqmCatalog.GetSha256(measure.Copyright),row[0]+": the copyright statement was edited.");
				Assert.AreEqual(row[2],CqmCatalog.GetSha256(measure.Disclaimer),row[0]+": the disclaimer statement was edited.");
				StringAssert.StartsWith(measure.Copyright,"<p>",row[0]);
				StringAssert.Contains(measure.Disclaimer,"WITHOUT WARRANTY OF ANY KIND",row[0]);
			}
			foreach(CqmMeasure measure in CqmCatalog.GetMeasures().Where(x => x.IsNcqa)) {
				StringAssert.Contains(measure.Copyright,"owned","NCQA's statement: "+measure.CmsId);
				StringAssert.Contains(measure.Copyright,"National Committee for Quality Assurance (NCQA)",measure.CmsId);
				StringAssert.Contains(measure.Copyright,"must be approved by NCQA",measure.CmsId);
			}
			foreach(CqmMeasure measure in CqmCatalog.GetMeasures().Where(x => !x.IsNcqa)) {
				StringAssert.Contains(measure.Copyright,"Centers for Medicare &amp; Medicaid Services (CMS)",measure.CmsId);
			}
			Assert.AreEqual("",CqmCatalog.GetSha256(null).Length==64 ? "" : "64 hex digits");
		}

		[TestMethod]
		public void GetStatementText_GivesEachParagraphAsPlainText_TheStatementItselfUnchanged() {
			string html="<p>TEST one &amp; two</p>\n<p>TEST&nbsp;three"+(char)0x202F+"and   four</p>\n<p></p>";
			Assert.AreEqual("TEST one & two\r\n\r\nTEST three and four",CqmCatalog.GetStatementText(html));
			Assert.AreEqual("",CqmCatalog.GetStatementText(null));
			string text=CqmCatalog.GetStatementText(CqmCatalog.GetMeasure("CMS155").Copyright);
			Assert.IsFalse(text.Contains("<p>") || text.Contains("&amp;") || text.Contains("&nbsp;"),text);
			StringAssert.Contains(text,"(C) 2012-2026 National Committee for Quality Assurance. All Rights Reserved.");
			Assert.AreEqual(8,Regex.Matches(text,"\r\n\r\n").Count+1,"The eight paragraphs of CMS155's copyright statement.");
		}
		#endregion The statements

		#region The guard
		///<summary>Section 10.1: no string literal of CqmCatalog.cs that is not an OID or a GUID looks like a code, and neither does a statement as
		///the catalog gives it (the CMS and NCQA statements pass, as their HQMF text does).</summary>
		[TestMethod]
		public void Catalog_HoldsNoCodeLikeLiteral() {
			Regex regexOid=new Regex(@"^\d+(\.\d+)+$");
			Regex regexGuid=new Regex("^[0-9A-Fa-f]{8}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{12}$");
			CSharpSource cSharpSource=CSharpSource.Read(TestPaths.ReadSource(PATH_CATALOG));
			Assert.IsTrue(cSharpSource.ListStringLiterals.Count>300,cSharpSource.ListStringLiterals.Count.ToString());
			List<string> listProblems=new List<string>();
			List<string> listTexts=cSharpSource.ListStringLiterals.Where(x => !regexOid.IsMatch(x) && !regexGuid.IsMatch(x)).ToList();
			foreach(CqmMeasure measure in CqmCatalog.GetMeasures()) {
				listTexts.Add(measure.Copyright);
				listTexts.Add(measure.Disclaimer);
				listTexts.Add(CqmCatalog.GetStatementText(measure.Copyright));
			}
			foreach(string text in listTexts) {
				foreach(Regex regex in _arrayRegexCodeLike) {
					foreach(Match match in regex.Matches(text)) {
						listProblems.Add(match.Value+" in \""+(text.Length>80 ? text.Substring(0,80)+"..." : text)+"\"");
					}
				}
			}
			Assert.AreEqual(0,listProblems.Count,"Looks like a code: "+string.Join("; ",listProblems.Distinct()));
			//The guard finds what it is for (made-up strings of each shape).
			foreach(string sample in new[] { "TEST 12345 TEST","TEST 1234-5 TEST","TEST A12.3 TEST","TEST D1234 TEST","TEST 1234F TEST" }) {
				Assert.IsTrue(_arrayRegexCodeLike.Any(x => x.IsMatch(sample)),sample);
			}
		}

		[TestMethod]
		public void Catalog_IsInTheBusinessNamespace_InTheCqmFolder_AndPlainAscii() {
			//DtoProcessor finds a Meth class by the assembly name and the class name (section 5.2): no sub-namespace.
			Assert.AreEqual("OpenPmsDentalBusiness",typeof(CqmCatalog).Namespace);
			Assert.AreEqual("OpenPmsDentalBusiness",typeof(CqmMeasure).Namespace);
			string source=File.ReadAllText(TestPaths.Combine(PATH_CATALOG));
			Assert.IsTrue(source.All(x => x<128),"The statements' typographic characters are written as \\u escapes.");
			Assert.IsFalse(CSharpSource.Read(source).ListStringLiterals.Any(x => Regex.IsMatch(x,@"\b(2014|June 2013)\b")),"No year of the deleted versions.");
		}
		#endregion The guard

		#region Against the downloads (optional)
		///<summary>With CQM_SPECS_DIR naming a folder of CMS's 2027 packages (CMS68 ... CMS165, each with its x/ folder of the measure's HQMF and
		///CQL files), every fact of the catalog is read again from them: titles, set and version IDs, population and stratum IDs with their defines,
		///the two statements verbatim, and each measure's valueset lines outside CQMCommonQDM. The packages carry codes and never enter the
		///repository; without the folder the test is inconclusive.</summary>
		[TestMethod]
		public void Catalog_EqualsTheDownloadedPackages_WhenCqmSpecsDirIsSet() {
			string folder=Environment.GetEnvironmentVariable("CQM_SPECS_DIR");
			if(string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder)) {
				Assert.Inconclusive("CQM_SPECS_DIR names no folder of CMS's 2027 packages.");
			}
			Regex regexValueSet=new Regex("^valueset\\s+\"([^\"]+)\":\\s*'urn:oid:([0-9.]+)'");
			foreach(CqmMeasure measure in CqmCatalog.GetMeasures()) {
				string folderX=Path.Combine(folder,measure.CmsId,"x");
				string pathHqmf=Path.Combine(folderX,measure.CmsId+"-v"+measure.VersionNumber+"-QDM.xml");
				Assert.IsTrue(File.Exists(pathHqmf),pathHqmf);
				XmlDocument xmlDocument=new XmlDocument();
				xmlDocument.XmlResolver=null;
				using(XmlReader xmlReader=XmlReader.Create(pathHqmf,new XmlReaderSettings { DtdProcessing=DtdProcessing.Prohibit,XmlResolver=null })) {
					xmlDocument.Load(xmlReader);
				}
				XmlNamespaceManager xmlNamespaceManager=new XmlNamespaceManager(xmlDocument.NameTable);
				xmlNamespaceManager.AddNamespace("h","urn:hl7-org:v3");
				XmlElement root=xmlDocument.DocumentElement;
				Assert.AreEqual(measure.VersionSpecificId,root.SelectSingleNode("h:id/@root",xmlNamespaceManager).Value,measure.CmsId);
				Assert.AreEqual(measure.SetId,root.SelectSingleNode("h:setId/@root",xmlNamespaceManager).Value,measure.CmsId);
				Assert.AreEqual(measure.VersionNumber,root.SelectSingleNode("h:versionNumber/@value",xmlNamespaceManager).Value,measure.CmsId);
				Assert.AreEqual(measure.Title,root.SelectSingleNode("h:title/@value",xmlNamespaceManager).Value,measure.CmsId);
				Assert.AreEqual(measure.Steward,root.SelectSingleNode("h:custodian//h:part/@value",xmlNamespaceManager).Value,measure.CmsId);
				Assert.AreEqual(measure.Developer,root.SelectSingleNode("h:author//h:part/@value",xmlNamespaceManager).Value,measure.CmsId);
				Assert.AreEqual(measure.Copyright,root.SelectSingleNode("//h:measureAttribute[h:code/h:displayName/@value='Copyright']/h:value/@value",
					xmlNamespaceManager).Value,measure.CmsId+": the copyright statement, verbatim.");
				Assert.AreEqual(measure.Disclaimer,root.SelectSingleNode("//h:measureAttribute[h:code/h:displayName/@value='Disclaimer']/h:value/@value",
					xmlNamespaceManager).Value,measure.CmsId+": the disclaimer statement, verbatim.");
				Assert.AreEqual(measure.ImprovementNotation,root.SelectSingleNode(
					"//h:measureAttribute[h:code/h:displayName/@value='Improvement Notation']/h:value/@value",xmlNamespaceManager).Value,measure.CmsId);
				XmlNodeList listSections=root.SelectNodes("//h:populationCriteriaSection",xmlNamespaceManager);
				Assert.AreEqual(measure.ListPopulationGroups.Count,listSections.Count,measure.CmsId);
				for(int i=0;i<listSections.Count;i++) {
					CqmPopulationGroup group=measure.ListPopulationGroups[i];
					Assert.AreEqual(group.Id,listSections[i].SelectSingleNode("h:id/@root",xmlNamespaceManager).Value,measure.CmsId);
					List<string> listFile=new List<string>();
					List<string> listStrataFile=new List<string>();
					foreach(XmlNode xmlNode in listSections[i].SelectNodes("h:component/*",xmlNamespaceManager)) {
						if(xmlNode.LocalName=="supplementalDataElement") {
							continue;
						}
						string define=xmlNode.SelectSingleNode(".//h:criteriaReference/h:id/@extension",xmlNamespaceManager).Value;
						define=define.Substring(define.IndexOf('.')+1).Trim('"');
						string id=xmlNode.SelectSingleNode("h:id/@root",xmlNamespaceManager).Value;
						if(xmlNode.LocalName=="stratifierCriteria") {
							listStrataFile.Add(id+" "+define);
							continue;
						}
						listFile.Add(xmlNode.SelectSingleNode("h:code/@code",xmlNamespaceManager).Value+" "+id+" "+define);
					}
					CollectionAssert.AreEqual(listFile,group.ListPopulations.Select(x => x.Code+" "+x.Id+" "+x.Define).ToList(),measure.CmsId+" group "+group.Number);
					CollectionAssert.AreEqual(listStrataFile,group.ListStrata.Select(x => x.Id+" "+x.Define).ToList(),measure.CmsId+" group "+group.Number);
				}
				Dictionary<string,string> dictValueSets=new Dictionary<string,string>();
				foreach(string pathCql in Directory.GetFiles(Path.Combine(folderX,"cql"),"*.cql")) {
					if(Path.GetFileName(pathCql).StartsWith("CQMCommonQDM",StringComparison.Ordinal)) {
						continue;
					}
					foreach(string line in File.ReadAllLines(pathCql,Encoding.UTF8)) {
						Match match=regexValueSet.Match(line.Trim());
						if(match.Success && !dictValueSets.ContainsKey(match.Groups[2].Value)) {
							dictValueSets.Add(match.Groups[2].Value,match.Groups[1].Value);
						}
					}
				}
				CollectionAssert.AreEquivalent(dictValueSets.Keys.ToList(),measure.ListValueSetOids,measure.CmsId+": the CQL's valueset lines and nothing else.");
				foreach(KeyValuePair<string,string> pair in dictValueSets) {
					Assert.AreEqual(pair.Value,CqmCatalog.GetValueSet(pair.Key).Name,measure.CmsId+" "+pair.Key);
				}
			}
		}
		#endregion Against the downloads (optional)
	}
}
