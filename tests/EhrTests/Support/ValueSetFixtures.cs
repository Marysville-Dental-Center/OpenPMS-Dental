// Synthetic value-set files for the EHR-2 tests (docs/specs/ehr-rebuild-plan.md, EHR-2; docs/port-notes/EHR-2.md): Excel workbooks written
// in the Office Open XML layout (workbook, relationships, shared strings, one sheet per list of rows), SVS files in the
// layout of VSAC's SVS API (DescribedValueSet, ConceptList/Concept, Group/Keyword), and .zip files of them. Every code is a made-up code
// that starts with TEST (the builders refuse any other); the value-set and code-system OIDs are the real ones, taken from the manifest and
// from the code list at run time. No VSAC, UMLS, CPT, CDT or SNOMED CT content is in any of them. The zip files are written by StoredZip below
// (entries stored, not compressed, with their CRC-32; or deflated with DeflateStream, which is in System.dll, for an entry that inflates to a
// large part), because this project does not reference System.IO.Compression.dll (ZipArchive); the importer reads them with ZipArchive as it
// reads any zip. EHR-6a (docs/specs/ehr-2027-measures-plan.md) adds the release's other two files, a Direct Reference Codes file (one TEST
// code per role of the catalog) and a Code System Versions file, and the pick of the three. EHR-6a-R gives the three the shapes of E-20's record
// of the real files (docs/port-notes/EHR-6.md, section 0): their header names, the rows above the header (the header on row 2 of the value-set
// workbook, row 3 of the Direct Reference Codes file and row 5 of the Code System Versions file), one sheet per measure in the value-set
// workbook sorted by CMS ID, inline strings, and each workbook in a .zip as VSAC gives it. The names, titles and versions in them are made up.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security;
using System.Text;
using OpenPmsDentalBusiness;

namespace EhrTests {
	///<summary>One synthetic row of a value-set file.</summary>
	public class TestValueSetRow {
		public string MeasureIds="";
		public string ValueSetName="";
		public string ValueSetOID="";
		public string QDMCategory="";
		public string CodeValue="";
		public string Description="";
		public string CodeSystem="";
		public string CodeSystemOID="";
		public string ValueSetVersion="";
		///<summary>The Code System Version column of a value-set or Direct Reference Codes row.</summary>
		public string CodeSystemVersion="TEST";
		///<summary>The Definition Version column of a value-set row: a made-up definition version, never the value set's version (the
		///Expansion Version is).</summary>
		public string DefinitionVersion="TEST-DEFINITION";
		///<summary>The eCQM Version column of a Direct Reference Codes row; null: the version of its eCQM ID (16 for CMS68v16).</summary>
		public string MeasureVersion;
		///<summary>The Expansion ID column of a value-set row.</summary>
		public string ExpansionId=ValueSetFixtures.EXPANSION_ID;

		///<summary>A copy of the row (its fields are strings).</summary>
		public TestValueSetRow MemberwiseCloneRow() {
			return (TestValueSetRow)MemberwiseClone();
		}

		///<summary>A row of the manifest's value set with a made-up code of the code system (its code list name), its OID from the code list.</summary>
		public static TestValueSetRow Make(string valueSetOID,string codeSystem,string codeValue,string measureIds=null) {
			EhrValueSetManifestItem manifestItem=EhrValueSetImport.GetManifestItem(valueSetOID);
			TestValueSetRow row=new TestValueSetRow();
			row.ValueSetOID=valueSetOID;
			row.ValueSetName=manifestItem?.ValueSetName??"TEST value set";
			row.QDMCategory=manifestItem?.QDMCategory??"";
			row.MeasureIds=measureIds??(manifestItem==null ? "" : string.Join(",",manifestItem.MeasureIds.Split(',').Select(x => "CMS"+x)));
			row.CodeValue=codeValue;
			row.Description="TEST description of "+codeValue;
			row.CodeSystem=codeSystem;
			row.CodeSystemOID=EhrValueSetImport.GetCodeSystemOid(codeSystem);
			row.ValueSetVersion=ValueSetFixtures.VALUE_SET_VERSION;
			row.CodeSystemVersion=ValueSetFixtures.GetCodeSystemVersion(codeSystem);
			return row;
		}

		///<summary>A row of a Direct Reference Codes file: a made-up code of the code system under the measures (CMS IDs with versions, comma
		///separated), with the description given (a LOINC role's display name from the catalog) or a made-up one.</summary>
		public static TestValueSetRow MakeDirectCode(string measureIds,string codeSystem,string codeValue,string description=null) {
			TestValueSetRow row=new TestValueSetRow();
			row.MeasureIds=measureIds;
			row.CodeValue=codeValue;
			row.Description=description??("TEST description of "+codeValue);
			row.CodeSystem=codeSystem;
			row.CodeSystemOID=EhrValueSetImport.GetCodeSystemOid(codeSystem);
			row.CodeSystemVersion=ValueSetFixtures.GetCodeSystemVersion(codeSystem);
			return row;
		}

		///<summary>A row of a Code System Versions file.</summary>
		public static TestValueSetRow MakeCodeSystemVersion(string codeSystem,string version) {
			TestValueSetRow row=new TestValueSetRow();
			row.CodeSystem=codeSystem;
			row.CodeSystemOID=EhrValueSetImport.GetCodeSystemOid(codeSystem);
			row.ValueSetVersion=version;//Under a header Version (EHR-6a's provisional shape).
			row.CodeSystemVersion=version;//Under Code System Version (E-20's record).
			return row;
		}
	}

	public static class ValueSetFixtures {
		///<summary>The value-set version the synthetic rows carry (a made-up expansion version of the 2026-05-14 release).</summary>
		public const string VALUE_SET_VERSION="TEST-20260514";
		///<summary>The made-up Expansion ID of the synthetic rows.</summary>
		public const string EXPANSION_ID="TEST-EXPANSION-ID";
		///<summary>The headers of the value-set workbook sorted by CMS ID, as E-20 recorded them (docs/port-notes/EHR-6.md, section 0), in their
		///order: 18 columns on row 2.</summary>
		public static readonly string[] ArrayHeadersDefault=new string[] {
			"CMS ID","Program Candidate","CBE Number","Value Set Name","Value Set OID","QDM Category","Definition Version","Expansion Version",
			"Purpose: Clinical Focus","Purpose: Data Element Scope","Purpose: Inclusion Criteria","Purpose: Exclusion Criteria","Code","Description",
			"Code System","Code System OID","Code System Version","Expansion ID",
		};
		///<summary>The headers of the Direct Reference Codes file, as E-20 recorded them: 13 columns on row 3.</summary>
		public static readonly string[] ArrayHeadersDirectCodes=new string[] {
			"Code","Description","Code System","Code System Version","Code System OID","eCQM Type","eCQM Title","eCQM ID","CBE Number","eCQM Version",
			"Measure GUID","Care Setting","Prior Value Set OID",
		};
		///<summary>The headers of the Code System Versions file, as E-20 recorded them: 3 columns on row 5.</summary>
		public static readonly string[] ArrayHeadersCodeSystemVersions=new string[] {
			"Code System Name","Code System Version","Code System OID",
		};
		///<summary>EHR-6a's provisional headers of the Code System Versions file (a column Version), still read.</summary>
		public static readonly string[] ArrayHeadersCodeSystemVersionsProvisional=new string[] {
			"Code System","Code System OID","Version",
		};

		///<summary>The made-up version of the code system that the synthetic rows name (MakeCodeSystemVersionRows), or "" for one it has none for.</summary>
		public static string GetCodeSystemVersion(string codeSystem) {
			TestValueSetRow row=MakeCodeSystemVersionRows().FirstOrDefault(x => x.CodeSystem==codeSystem);
			return row==null ? "" : row.CodeSystemVersion;
		}

		///<summary>Throws unless every code is a made-up TEST code (section 6 of the plan).</summary>
		public static void CheckCodes(IEnumerable<TestValueSetRow> listRows) {
			foreach(TestValueSetRow row in listRows) {
				if(row.CodeValue!="" && !row.CodeValue.StartsWith("TEST",StringComparison.Ordinal)) {
					throw new ArgumentException("Fixtures use made-up codes that start with TEST only: "+row.CodeValue);
				}
			}
		}

		///<summary>The cell of a row under a header of ArrayHeadersDefault (or a variant of one).</summary>
		private static string GetCell(TestValueSetRow row,string header) {
			switch(new string(header.Where(x => !char.IsWhiteSpace(x)).ToArray()).ToLowerInvariant()) {
				case "cmsid":
				case "ecqmid":
				case "measureids":
					return row.MeasureIds;
				case "ecqmversion": {
					if(row.MeasureVersion!=null) {
						return row.MeasureVersion;
					}
					System.Text.RegularExpressions.Match match=System.Text.RegularExpressions.Regex.Match(row.MeasureIds,@"^CMS\d+v(\d+)$");
					return match.Success ? match.Groups[1].Value : "";
				}
				case "programcandidate":
				case "ecqmtype":
					return row.MeasureIds=="" ? "" : "TEST program";
				case "ecqmtitle":
					return row.MeasureIds=="" ? "" : "TEST title of "+row.MeasureIds;
				case "caresetting":
					return row.MeasureIds=="" ? "" : "TEST setting";
				case "cbenumber":
				case "measureguid":
					return row.MeasureIds=="" ? "" : "TEST-"+row.MeasureIds;
				case "priorvaluesetoid":
					return row.CodeValue=="" ? "" : "TEST prior value set";
				case "valuesetname":
					return row.ValueSetName;
				case "valuesetoid":
				case "oid":
					return row.ValueSetOID;
				case "qdmcategory":
					return row.QDMCategory;
				case "definitionversion":
					return row.DefinitionVersion;
				case "expansionid":
					return row.ExpansionId;
				case "expansionversion":
				case "version":
					return row.ValueSetVersion;
				case "code":
					return row.CodeValue;
				case "descriptor":
				case "description":
					return row.Description;
				case "codesystem":
				case "codesystemname":
					return row.CodeSystem;
				case "codesystemversion":
					return row.CodeSystemVersion;
				case "codesystemoid":
					return row.CodeSystemOID;
				default:
					return "";
			}
		}

		///<summary>A sheet of a workbook: rows above the header (a title), the header, then the rows.</summary>
		public class Sheet {
			public string Name="Sheet1";
			///<summary>The rows above the header: "" an empty row, a tab between the cells of a row of several.</summary>
			public List<string> ListTitleLines=new List<string>();
			///<summary>Null: no header row at all (a "Read Me" sheet).</summary>
			public string[] ArrayHeaders=ArrayHeadersDefault;
			public List<TestValueSetRow> ListRows=new List<TestValueSetRow>();
			///<summary>Every text as an inline string (t="inlineStr") instead of a shared string, as some exporters write it.</summary>
			public bool UseInlineStrings;
			///<summary>When set (for example "ZZZ", the last column a three-letter reference names), every row also gets a number cell in that
			///column, after its other cells.</summary>
			public string ColumnExtraCell;
			///<summary>Rows after the others that hold only the cell in ColumnExtraCell.</summary>
			public int CountExtraRows;
		}

		///<summary>An Excel workbook (.xlsx bytes) with the sheets. Text goes into the shared strings (or inline strings), as Excel writes it.</summary>
		public static byte[] MakeWorkbook(params Sheet[] arraySheets) {
			return MakeWorkbook(true,arraySheets);
		}

		///<summary>MakeWorkbook, with or without the shared-strings part (a workbook of inline strings needs none; E-20's record found the part
		///empty in VSAC's).</summary>
		public static byte[] MakeWorkbook(bool hasSharedStringsPart,params Sheet[] arraySheets) {
			foreach(Sheet sheet in arraySheets) {
				CheckCodes(sheet.ListRows);
			}
			List<string> listShared=new List<string>();
			Dictionary<string,int> dictShared=new Dictionary<string,int>(StringComparer.Ordinal);
			List<string> listSheetXml=new List<string>();
			foreach(Sheet sheet in arraySheets) {
				StringBuilder stringBuilder=new StringBuilder();
				stringBuilder.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
				stringBuilder.Append("<worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\" "
					+"xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\"><sheetData>");
				int indexRow=1;
				List<List<string>> listCells=new List<List<string>>();
				foreach(string title in sheet.ListTitleLines) {
					listCells.Add(title.Split('\t').ToList());
				}
				if(sheet.ArrayHeaders!=null) {
					listCells.Add(sheet.ArrayHeaders.ToList());
					foreach(TestValueSetRow row in sheet.ListRows) {
						listCells.Add(sheet.ArrayHeaders.Select(x => GetCell(row,x)).ToList());
					}
				}
				foreach(List<string> listRowCells in listCells) {
					stringBuilder.Append("<row r=\""+indexRow+"\">");
					for(int i=0;i<listRowCells.Count;i++) {
						string value=listRowCells[i]??"";
						if(value=="") {
							continue;//Excel leaves empty cells out; the reader must place the next cell by its reference.
						}
						string reference=GetColumnName(i)+indexRow;
						if(sheet.UseInlineStrings) {
							stringBuilder.Append("<c r=\""+reference+"\" t=\"inlineStr\"><is><t>"+SecurityElement.Escape(value)+"</t></is></c>");
							continue;
						}
						if(!dictShared.TryGetValue(value,out int indexShared)) {
							indexShared=listShared.Count;
							listShared.Add(value);
							dictShared.Add(value,indexShared);
						}
						stringBuilder.Append("<c r=\""+reference+"\" t=\"s\"><v>"+indexShared+"</v></c>");
					}
					if(sheet.ColumnExtraCell!=null) {
						stringBuilder.Append("<c r=\""+sheet.ColumnExtraCell+indexRow+"\"><v>1</v></c>");
					}
					stringBuilder.Append("</row>");
					indexRow++;
				}
				for(int i=0;i<sheet.CountExtraRows;i++) {
					stringBuilder.Append("<row r=\""+indexRow+"\"><c r=\""+sheet.ColumnExtraCell+indexRow+"\"><v>1</v></c></row>");
					indexRow++;
				}
				stringBuilder.Append("</sheetData></worksheet>");
				listSheetXml.Add(stringBuilder.ToString());
			}
			StoredZip zipArchive=new StoredZip();
			StringBuilder stringBuilderTypes=new StringBuilder();
			stringBuilderTypes.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\">"
				+"<Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/>"
				+"<Default Extension=\"xml\" ContentType=\"application/xml\"/>"
				+"<Override PartName=\"/xl/workbook.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml\"/>"
				+(hasSharedStringsPart ? "<Override PartName=\"/xl/sharedStrings.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.sharedStrings+xml\"/>" : ""));
			for(int i=0;i<arraySheets.Length;i++) {
				stringBuilderTypes.Append("<Override PartName=\"/xl/worksheets/sheet"+(i+1)+".xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/>");
			}
			stringBuilderTypes.Append("</Types>");
			AddEntry(zipArchive,"[Content_Types].xml",stringBuilderTypes.ToString());
			AddEntry(zipArchive,"_rels/.rels","<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">"
				+"<Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument\" Target=\"xl/workbook.xml\"/></Relationships>");
			StringBuilder stringBuilderWorkbook=new StringBuilder();
			StringBuilder stringBuilderRels=new StringBuilder();
			stringBuilderWorkbook.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><workbook xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\" "
				+"xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\"><sheets>");
			stringBuilderRels.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">");
			for(int i=0;i<arraySheets.Length;i++) {
				//The relationship ids are not in sheet order, as in workbooks Excel has edited: the reader must follow them.
				string relationshipId="rId"+(arraySheets.Length-i+2);
				stringBuilderWorkbook.Append("<sheet name=\""+SecurityElement.Escape(arraySheets[i].Name)+"\" sheetId=\""+(i+1)+"\" r:id=\""+relationshipId+"\"/>");
				stringBuilderRels.Append("<Relationship Id=\""+relationshipId+"\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet\" "
					+"Target=\"worksheets/sheet"+(i+1)+".xml\"/>");
				AddEntry(zipArchive,"xl/worksheets/sheet"+(i+1)+".xml",listSheetXml[i]);
			}
			if(hasSharedStringsPart) {
				stringBuilderRels.Append("<Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/sharedStrings\" Target=\"sharedStrings.xml\"/>");
			}
			else if(listShared.Count>0) {
				throw new ArgumentException("A workbook without the shared-strings part holds inline strings only.");
			}
			stringBuilderWorkbook.Append("</sheets></workbook>");
			stringBuilderRels.Append("</Relationships>");
			AddEntry(zipArchive,"xl/workbook.xml",stringBuilderWorkbook.ToString());
			AddEntry(zipArchive,"xl/_rels/workbook.xml.rels",stringBuilderRels.ToString());
			StringBuilder stringBuilderShared=new StringBuilder();
			stringBuilderShared.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><sst xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\" count=\""
				+listShared.Count+"\" uniqueCount=\""+listShared.Count+"\">");
			foreach(string value in listShared) {
				//Some strings as rich text runs, as Excel writes a cell with mixed formatting.
				if(value.StartsWith("TEST description of ",StringComparison.Ordinal)) {
					stringBuilderShared.Append("<si><r><t xml:space=\"preserve\">TEST description </t></r><r><rPr><b/></rPr><t>"
						+SecurityElement.Escape(value.Substring("TEST description ".Length))+"</t></r><rPh sb=\"0\" eb=\"1\"><t>PHONETIC</t></rPh></si>");
					continue;
				}
				stringBuilderShared.Append("<si><t>"+SecurityElement.Escape(value)+"</t></si>");
			}
			stringBuilderShared.Append("</sst>");
			if(hasSharedStringsPart) {
				AddEntry(zipArchive,"xl/sharedStrings.xml",stringBuilderShared.ToString());
			}
			return zipArchive.ToArray();
		}

		///<summary>"A" for 0, "Z" for 25, "AA" for 26.</summary>
		public static string GetColumnName(int index) {
			string name="";
			index++;
			while(index>0) {
				int remainder=(index-1)%26;
				name=(char)('A'+remainder)+name;
				index=(index-1)/26;
			}
			return name;
		}

		private static void AddEntry(StoredZip zipArchive,string name,string text) {
			zipArchive.Add(name,new UTF8Encoding(false).GetBytes(text));
		}

		///<summary>An SVS file (RetrieveMultipleValueSetsResponse) with one DescribedValueSet per value set of the rows. With hasGroups, each value
		///set has a CATEGORY group (its QDM category) and a "CMS eMeasure ID" group (its measures) and an NQF group the reader ignores; without, it
		///has none (the file then names no measures).</summary>
		public static byte[] MakeSvs(List<TestValueSetRow> listRows,bool hasGroups) {
			CheckCodes(listRows);
			StringBuilder stringBuilder=new StringBuilder();
			stringBuilder.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n");
			stringBuilder.Append("<ns0:RetrieveMultipleValueSetsResponse xmlns:ns0=\"urn:ihe:iti:svs:2008\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">\r\n");
			foreach(IGrouping<string,TestValueSetRow> grouping in listRows.GroupBy(x => x.ValueSetOID)) {
				TestValueSetRow rowFirst=grouping.First();
				stringBuilder.Append("  <ns0:DescribedValueSet ID=\""+SecurityElement.Escape(rowFirst.ValueSetOID)+"\" displayName=\""+SecurityElement.Escape(rowFirst.ValueSetName)
					+"\" version=\""+SecurityElement.Escape(rowFirst.ValueSetVersion)+"\">\r\n");
				stringBuilder.Append("    <ns0:ConceptList>\r\n");
				foreach(TestValueSetRow row in grouping) {
					stringBuilder.Append("      <ns0:Concept code=\""+SecurityElement.Escape(row.CodeValue)+"\" codeSystemName=\""+SecurityElement.Escape(row.CodeSystem)
						+"\" codeSystemVersion=\""+SecurityElement.Escape(row.CodeSystemVersion)+"\" codeSystem=\""+SecurityElement.Escape(row.CodeSystemOID)+"\" displayName=\""+SecurityElement.Escape(row.Description)+"\"/>\r\n");
				}
				stringBuilder.Append("    </ns0:ConceptList>\r\n");
				stringBuilder.Append("    <ns0:Source>TEST steward</ns0:Source>\r\n    <ns0:Type>Extensional</ns0:Type>\r\n    <ns0:Binding>Static</ns0:Binding>\r\n"
					+"    <ns0:Status>Active</ns0:Status>\r\n    <ns0:RevisionDate>2026-05-14</ns0:RevisionDate>\r\n");
				if(hasGroups) {
					stringBuilder.Append("    <ns0:Group displayName=\"CATEGORY\" ID=\"1\" sourceOrganization=\"TEST\">\r\n      <ns0:Keyword>"
						+SecurityElement.Escape(rowFirst.QDMCategory)+"</ns0:Keyword>\r\n    </ns0:Group>\r\n");
					stringBuilder.Append("    <ns0:Group displayName=\"CMS eMeasure ID\" ID=\"2\" sourceOrganization=\"TEST\">\r\n");
					foreach(string measureId in grouping.SelectMany(x => x.MeasureIds.Split(',')).Select(x => x.Trim()).Where(x => x!="").Distinct()) {
						stringBuilder.Append("      <ns0:Keyword>"+SecurityElement.Escape(measureId)+"</ns0:Keyword>\r\n");
					}
					stringBuilder.Append("    </ns0:Group>\r\n");
					stringBuilder.Append("    <ns0:Group displayName=\"NQF Number\" ID=\"3\" sourceOrganization=\"TEST\">\r\n      <ns0:Keyword>TEST-NQF</ns0:Keyword>\r\n    </ns0:Group>\r\n");
				}
				stringBuilder.Append("  </ns0:DescribedValueSet>\r\n");
			}
			stringBuilder.Append("</ns0:RetrieveMultipleValueSetsResponse>\r\n");
			return new UTF8Encoding(false).GetBytes(stringBuilder.ToString());
		}

		///<summary>A .zip with the entries (name, bytes).</summary>
		public static byte[] MakeZip(params KeyValuePair<string,byte[]>[] arrayEntries) {
			StoredZip storedZip=new StoredZip();
			foreach(KeyValuePair<string,byte[]> pair in arrayEntries) {
				storedZip.Add(pair.Key,pair.Value);
			}
			return storedZip.ToArray();
		}

		public static KeyValuePair<string,byte[]> Entry(string name,byte[] arrayBytes) {
			return new KeyValuePair<string,byte[]>(name,arrayBytes);
		}

		///<summary>One made-up code in each value set of the manifest, of a code system that suits it (SNOMEDCT for most; the code list's own
		///systems for the rest), with the manifest's measures (as CMS IDs), so the file names all seven measures at their 2027 versions.</summary>
		public static List<TestValueSetRow> MakeManifestRows(string prefix="TESTM") {
			List<TestValueSetRow> listRows=new List<TestValueSetRow>();
			int index=0;
			foreach(EhrValueSetManifestItem item in EhrValueSetImport.GetManifest()) {
				index++;
				listRows.Add(TestValueSetRow.Make(item.ValueSetOID,"SNOMEDCT",prefix+index.ToString("D4")));
			}
			return listRows;
		}

		///<summary>A large synthetic release of countRows rows: ten in eleven in the manifest's value sets (with their measures), the rest in 400
		///other made-up value sets under the made-up arc 2.16.840.1.113883.3.999999 (with made-up measures CMS1000v3 to CMS1049v3), as the real
		///release holds the value sets of every measure; the importer keeps only the first (E-30).</summary>
		public static List<TestValueSetRow> MakeLargeRelease(int countRows) {
			List<TestValueSetRow> listRows=new List<TestValueSetRow>();
			List<EhrValueSetManifestItem> listManifest=EhrValueSetImport.GetManifest();
			string[] arrayCodeSystems=new[] { "SNOMEDCT","ICD9CM","ICD10CM","CPT","LOINC","RXNORM","HCPCS","CDT" };
			for(int i=0;i<countRows;i++) {
				string codeSystem=arrayCodeSystems[i%arrayCodeSystems.Length];
				TestValueSetRow row;
				if(i%11!=0) {
					row=TestValueSetRow.Make(listManifest[i%listManifest.Count].ValueSetOID,codeSystem,"TESTL"+i.ToString("D7"));
				}
				else {
					row=TestValueSetRow.Make("2.16.840.1.113883.3.999999."+(i%400),codeSystem,"TESTL"+i.ToString("D7"),"CMS"+(1000+i%50)+"v3");
					row.ValueSetName="TEST other value set "+(i%400);
					row.QDMCategory="TEST category";
				}
				listRows.Add(row);
			}
			return listRows;
		}

		#region The release's other two files (EHR-6a)
		///<summary>A Direct Reference Codes file (.xlsx bytes) of the rows (TestValueSetRow.MakeDirectCode), under the default headers or others, in
		///the shape of E-20's record: one sheet, row 1 empty, row 2 an explanatory paragraph (made up here), the header on row 3, inline strings.</summary>
		public static byte[] MakeDirectCodesWorkbook(List<TestValueSetRow> listRows,string[] arrayHeaders=null) {
			Sheet sheet=new Sheet();
			sheet.Name="drc-TEST";
			sheet.ListTitleLines.Add("");
			sheet.ListTitleLines.Add("TEST explanatory paragraph of a Direct Reference Codes file, with a Code and a Code System named in its text.");
			sheet.ArrayHeaders=arrayHeaders??ArrayHeadersDirectCodes;
			sheet.ListRows=listRows;
			sheet.UseInlineStrings=true;
			return MakeWorkbook(sheet);
		}

		///<summary>A Code System Versions file (.xlsx bytes) of the rows (TestValueSetRow.MakeCodeSystemVersion), in the shape of E-20's record: one
		///sheet, row 1 empty, row 2 a title, row 3 empty, row 4 the publication date (two cells), the header on row 5, inline strings.</summary>
		public static byte[] MakeCodeSystemVersionsWorkbook(List<TestValueSetRow> listRows,string[] arrayHeaders=null) {
			Sheet sheet=new Sheet();
			sheet.Name="Code System Versions";
			sheet.ListTitleLines.Add("");
			sheet.ListTitleLines.Add("TEST title of a Code System Versions file");
			sheet.ListTitleLines.Add("");
			sheet.ListTitleLines.Add("Publication Date\tTEST 2026-05-14");
			sheet.ArrayHeaders=arrayHeaders??ArrayHeadersCodeSystemVersions;
			sheet.ListRows=listRows;
			sheet.UseInlineStrings=true;
			return MakeWorkbook(sheet);
		}

		///<summary>A value-set workbook sorted by CMS ID (.xlsx bytes), in the shape of E-20's record: one sheet per measure, named by its CMS ID
		///(CMS68v16), each row under one measure (a row of several measures is on each of their sheets), row 1 empty, the header on row 2, inline
		///strings. Rows that name no measure go on a sheet "TEST no measure".</summary>
		public static byte[] MakeByCmsWorkbook(List<TestValueSetRow> listRows) {
			List<Sheet> listSheets=new List<Sheet>();
			Dictionary<string,Sheet> dictSheets=new Dictionary<string,Sheet>(StringComparer.Ordinal);
			foreach(TestValueSetRow row in listRows) {
				List<string> listMeasureIds=row.MeasureIds.Split(',').Select(x => x.Trim()).Where(x => x!="").ToList();
				if(listMeasureIds.Count==0) {
					listMeasureIds.Add("");
				}
				foreach(string measureId in listMeasureIds) {
					if(!dictSheets.TryGetValue(measureId,out Sheet sheet)) {
						sheet=new Sheet();
						sheet.Name=measureId=="" ? "TEST no measure" : measureId;
						sheet.ListTitleLines.Add("");
						sheet.UseInlineStrings=true;
						dictSheets.Add(measureId,sheet);
						listSheets.Add(sheet);
					}
					TestValueSetRow rowOfSheet=(TestValueSetRow)row.MemberwiseCloneRow();
					rowOfSheet.MeasureIds=measureId;
					sheet.ListRows.Add(rowOfSheet);
				}
			}
			return MakeWorkbook(listSheets.ToArray());
		}

		///<summary>The workbook in a .zip under the name, as VSAC gives each of the three files (ec_..._cms_20260514.xlsx.zip holds
		///ec_..._cms_20260514.xlsx).</summary>
		public static byte[] ZipAsVsacGivesIt(string nameXlsx,byte[] arrayXlsx) {
			return MakeZip(Entry(nameXlsx,arrayXlsx));
		}

		///<summary>One made-up TEST code per direct-reference-code role of the catalog (four for CMS138's CPT visits), under the role's measure at its
		///2027 version, the LOINC roles with the display name the catalog gives them. Each LOINC role's measure gets one more LOINC code with another
		///display name, which no role takes, and CMS74, CMS75 and CMS155 a SNOMED CT code with no role, so the rows that are only listed are there
		///too and the file names all seven measures.</summary>
		public static List<TestValueSetRow> MakeDirectCodeRows(string prefix="TESTD") {
			List<TestValueSetRow> listRows=new List<TestValueSetRow>();
			int index=0;
			foreach(CqmDirectCodeRole role in CqmCatalog.GetDirectCodeRoles()) {
				string measureId=CqmCatalog.GetMeasure(role.CmsId).MeasureId;
				for(int i=0;i<role.CountExpected;i++) {
					index++;
					listRows.Add(TestValueSetRow.MakeDirectCode(measureId,role.CodeSystem,prefix+index.ToString("D4"),
						role.DisplayName=="" ? null : role.DisplayName));
				}
			}
			foreach(string cmsId in new[] { "CMS69","CMS165" }) {
				index++;
				listRows.Add(TestValueSetRow.MakeDirectCode(CqmCatalog.GetMeasure(cmsId).MeasureId,"LOINC",prefix+index.ToString("D4"),"TEST other LOINC display"));
			}
			foreach(string cmsId in new[] { "CMS74","CMS75","CMS155" }) {//As the hospice codes are listed under them: no role.
				index++;
				listRows.Add(TestValueSetRow.MakeDirectCode(CqmCatalog.GetMeasure(cmsId).MeasureId,"SNOMEDCT",prefix+index.ToString("D4")));
			}
			//As the real file lists the direct reference codes of the release's other measures (hospital ones among them), which are not imported:
			//made-up measures CMS9001v1 and CMS9002v4, the first also under the SNOMEDCT_US name the real file writes.
			index++;
			TestValueSetRow rowOther=TestValueSetRow.MakeDirectCode("CMS9001v1","SNOMEDCT",prefix+index.ToString("D4"));
			rowOther.CodeSystem="SNOMEDCT_US";
			listRows.Add(rowOther);
			index++;
			listRows.Add(TestValueSetRow.MakeDirectCode("CMS9002v4","LOINC",prefix+index.ToString("D4")));
			return listRows;
		}

		///<summary>The made-up rows of other measures MakeDirectCodeRows adds (not imported).</summary>
		public const int COUNT_DIRECT_CODE_ROWS_OTHER_MEASURES=2;

		///<summary>A made-up version of each code system the code list knows, in the formats a Code System Versions file is expected to use
		///(provisional): dates for SNOMED CT, RxNorm and CVX, a release number for LOINC, a year for ICD-10-CM, CPT and HCPCS.</summary>
		public static List<TestValueSetRow> MakeCodeSystemVersionRows() {
			return new List<TestValueSetRow> {
				TestValueSetRow.MakeCodeSystemVersion("SNOMEDCT","2025-09"),
				TestValueSetRow.MakeCodeSystemVersion("LOINC","2.80"),
				TestValueSetRow.MakeCodeSystemVersion("ICD10CM","2026"),
				TestValueSetRow.MakeCodeSystemVersion("CPT","2026"),
				TestValueSetRow.MakeCodeSystemVersion("RXNORM","2026-04"),
				TestValueSetRow.MakeCodeSystemVersion("CVX","2026-03-31"),
				TestValueSetRow.MakeCodeSystemVersion("HCPCS","2026"),
				TestValueSetRow.MakeCodeSystemVersion("CDCREC","1.2"),
			};
		}

		///<summary>The three files of a synthetic release (names and bytes), each in a .zip as VSAC gives it: the value sets as given (by default a
		///workbook sorted by CMS ID of every catalog value set, MakeByCmsWorkbook), the Direct Reference Codes file (one code per role by default)
		///and the Code System Versions file. Value sets given as bytes are used as they are, under nameValueSets.</summary>
		public static List<KeyValuePair<string,byte[]>> MakeRelease(byte[] arrayValueSets=null,List<TestValueSetRow> listDirectCodes=null,
			List<TestValueSetRow> listCodeSystemVersions=null,string nameValueSets=NAME_VALUE_SETS)
		{
			if(arrayValueSets==null) {
				arrayValueSets=ZipAsVsacGivesIt("ec_value_sets_cms_TEST.xlsx",MakeByCmsWorkbook(MakeManifestRows()));
			}
			return new List<KeyValuePair<string,byte[]>> {
				Entry(nameValueSets,arrayValueSets),
				Entry(NAME_DIRECT_CODES,ZipAsVsacGivesIt("drc_TEST.xlsx",MakeDirectCodesWorkbook(listDirectCodes??MakeDirectCodeRows()))),
				Entry(NAME_CODE_SYSTEM_VERSIONS,ZipAsVsacGivesIt("cs_versions_TEST.xlsx",
					MakeCodeSystemVersionsWorkbook(listCodeSystemVersions??MakeCodeSystemVersionRows()))),
			};
		}

		///<summary>The names of MakeRelease's three files.</summary>
		public const string NAME_VALUE_SETS="ec_value_sets_cms_TEST.xlsx.zip";
		public const string NAME_DIRECT_CODES="drc_TEST.xlsx.zip";
		public const string NAME_CODE_SYSTEM_VERSIONS="cs_versions_TEST.xlsx.zip";

		///<summary>Reads the files as one pick (EhrValueSetImport.ParsePick), as the Code System Importer does.</summary>
		public static EhrValueSetPick ParsePick(List<KeyValuePair<string,byte[]>> listFiles) {
			return EhrValueSetImport.ParsePick(listFiles.Select(x => x.Key).ToList(),index => new MemoryStream(listFiles[index].Value,writable:false));
		}

		///<summary>Parses each file alone and checks them together (EhrValueSetImport.MakePick), as for a pick read file by file.</summary>
		public static EhrValueSetPick ParseEachThenPick(List<KeyValuePair<string,byte[]>> listFiles) {
			return EhrValueSetImport.MakePick(listFiles.Select(x => EhrValueSetImport.Parse(x.Key,x.Value)).ToList());
		}
		#endregion The release's other two files (EHR-6a)
	}
	///<summary>A zip file whose entries are stored (method 0) with their CRC-32 and UTF-8 names: the local headers, the central directory and the
	///end record of the ZIP format (PKWARE APPNOTE, section 4.3), which ZipArchive reads.</summary>
	public class StoredZip {
		private readonly MemoryStream _memoryStream=new MemoryStream();
		private readonly MemoryStream _memoryStreamCentral=new MemoryStream();
		private int _countEntries;
		private static uint[] _arrayCrcTable;

		public void Add(string name,byte[] arrayBytes) {
			AddEntry(name,arrayBytes,method:0,GetCrc32(arrayBytes),arrayBytes.Length);
		}

		///<summary>Adds a deflated entry (method 8, written with DeflateStream of System.dll) whose content is the head, then countFill bytes of
		///fill, then the tail: a small entry that inflates to a large part. The headers give the real uncompressed size, or sizeClaimed when it is
		///set (a header that understates the size, which the reader must not trust).</summary>
		public void AddDeflated(string name,byte[] arrayHead,byte fill,long countFill,byte[] arrayTail,long sizeClaimed=-1) {
			uint crc=0xFFFFFFFFu;
			long countUncompressed=0;
			byte[] arrayCompressed;
			using(MemoryStream memoryStream=new MemoryStream()) {
				using(DeflateStream deflateStream=new DeflateStream(memoryStream,CompressionMode.Compress,leaveOpen:true)) {
					crc=UpdateCrc32(crc,arrayHead,arrayHead.Length);
					deflateStream.Write(arrayHead,0,arrayHead.Length);
					byte[] arrayFill=Enumerable.Repeat(fill,1024*1024).ToArray();
					for(long countLeft=countFill;countLeft>0;) {
						int count=(int)Math.Min(countLeft,arrayFill.Length);
						crc=UpdateCrc32(crc,arrayFill,count);
						deflateStream.Write(arrayFill,0,count);
						countLeft-=count;
					}
					crc=UpdateCrc32(crc,arrayTail,arrayTail.Length);
					deflateStream.Write(arrayTail,0,arrayTail.Length);
				}
				arrayCompressed=memoryStream.ToArray();
			}
			countUncompressed=arrayHead.Length+countFill+arrayTail.Length;
			AddEntry(name,arrayCompressed,method:8,crc^0xFFFFFFFFu,sizeClaimed>=0 ? sizeClaimed : countUncompressed);
		}

		private void AddEntry(string name,byte[] arrayData,ushort method,uint crc,long sizeUncompressed) {
			byte[] arrayName=new UTF8Encoding(false).GetBytes(name);
			long offset=_memoryStream.Position;
			BinaryWriter writer=new BinaryWriter(_memoryStream);
			writer.Write(0x04034b50u);//Local file header.
			writer.Write((ushort)20);//Version needed.
			writer.Write((ushort)0x0800);//Flags: UTF-8 names.
			writer.Write(method);//0 stored, 8 deflated.
			writer.Write((ushort)0);//Time.
			writer.Write((ushort)0x5B6E);//Date: 2025-11-14.
			writer.Write(crc);
			writer.Write((uint)arrayData.Length);
			writer.Write((uint)sizeUncompressed);
			writer.Write((ushort)arrayName.Length);
			writer.Write((ushort)0);
			writer.Write(arrayName);
			writer.Write(arrayData);
			writer.Flush();
			BinaryWriter writerCentral=new BinaryWriter(_memoryStreamCentral);
			writerCentral.Write(0x02014b50u);//Central directory header.
			writerCentral.Write((ushort)20);//Version made by.
			writerCentral.Write((ushort)20);//Version needed.
			writerCentral.Write((ushort)0x0800);
			writerCentral.Write(method);
			writerCentral.Write((ushort)0);
			writerCentral.Write((ushort)0x5B6E);
			writerCentral.Write(crc);
			writerCentral.Write((uint)arrayData.Length);
			writerCentral.Write((uint)sizeUncompressed);
			writerCentral.Write((ushort)arrayName.Length);
			writerCentral.Write((ushort)0);//Extra field.
			writerCentral.Write((ushort)0);//Comment.
			writerCentral.Write((ushort)0);//Disk.
			writerCentral.Write((ushort)0);//Internal attributes.
			writerCentral.Write(0u);//External attributes.
			writerCentral.Write((uint)offset);
			writerCentral.Write(arrayName);
			writerCentral.Flush();
			_countEntries++;
		}

		public byte[] ToArray() {
			using(MemoryStream memoryStream=new MemoryStream()) {
				_memoryStream.WriteTo(memoryStream);
				long offsetCentral=memoryStream.Position;
				_memoryStreamCentral.WriteTo(memoryStream);
				BinaryWriter writer=new BinaryWriter(memoryStream);
				writer.Write(0x06054b50u);//End of central directory.
				writer.Write((ushort)0);
				writer.Write((ushort)0);
				writer.Write((ushort)_countEntries);
				writer.Write((ushort)_countEntries);
				writer.Write((uint)_memoryStreamCentral.Length);
				writer.Write((uint)offsetCentral);
				writer.Write((ushort)0);
				writer.Flush();
				return memoryStream.ToArray();
			}
		}

		///<summary>The CRC-32 of ZIP (polynomial 0xEDB88320).</summary>
		public static uint GetCrc32(byte[] arrayBytes) {
			return UpdateCrc32(0xFFFFFFFFu,arrayBytes,arrayBytes.Length)^0xFFFFFFFFu;
		}

		///<summary>The running CRC-32 (start with 0xFFFFFFFF, end with an exclusive or of 0xFFFFFFFF) over the first count bytes.</summary>
		private static uint UpdateCrc32(uint crc,byte[] arrayBytes,int count) {
			if(_arrayCrcTable==null) {
				uint[] arrayTable=new uint[256];
				for(uint i=0;i<256;i++) {
					uint value=i;
					for(int j=0;j<8;j++) {
						value=(value&1)!=0 ? 0xEDB88320u^(value>>1) : value>>1;
					}
					arrayTable[i]=value;
				}
				_arrayCrcTable=arrayTable;
			}
			for(int i=0;i<count;i++) {
				crc=_arrayCrcTable[(crc^arrayBytes[i])&0xFF]^(crc>>8);
			}
			return crc;
		}
	}
}
