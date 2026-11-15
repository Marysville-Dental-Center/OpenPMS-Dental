// Synthetic value-set files for the EHR-2 tests (docs/specs/ehr-rebuild-plan.md, EHR-2; docs/port-notes/EHR-2.md): Excel workbooks written
// in the Office Open XML layout (workbook, relationships, shared strings, one sheet per list of rows), SVS files in the
// layout of VSAC's SVS API (DescribedValueSet, ConceptList/Concept, Group/Keyword), and .zip files of them. Every code is a made-up code
// that starts with TEST (the builders refuse any other); the value-set and code-system OIDs are the real ones, taken from the manifest and
// from the code list at run time. No VSAC, UMLS, CPT, CDT or SNOMED CT content is in any of them. The zip files are written by StoredZip below
// (entries stored, not compressed, with their CRC-32), because this project does not reference System.IO.Compression; the importer reads them
// with ZipArchive as it reads any zip.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
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
			row.ValueSetVersion="20130614";
			return row;
		}
	}

	public static class ValueSetFixtures {
		///<summary>The headers the workbooks use by default: the names the importer expects first (docs/port-notes/EHR-2.md).</summary>
		public static readonly string[] ArrayHeadersDefault=new string[] {
			"CMS ID","Value Set Name","Value Set OID","QDM Category","Definition Version","Code","Descriptor","Code System","Code System Version",
			"Code System OID",
		};

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
				case "measureids":
					return row.MeasureIds;
				case "valuesetname":
					return row.ValueSetName;
				case "valuesetoid":
				case "oid":
					return row.ValueSetOID;
				case "qdmcategory":
					return row.QDMCategory;
				case "definitionversion":
					return row.ValueSetVersion;
				case "code":
					return row.CodeValue;
				case "descriptor":
				case "description":
					return row.Description;
				case "codesystem":
					return row.CodeSystem;
				case "codesystemversion":
					return "TEST";
				case "codesystemoid":
					return row.CodeSystemOID;
				default:
					return "";
			}
		}

		///<summary>A sheet of a workbook: rows above the header (a title), the header, then the rows.</summary>
		public class Sheet {
			public string Name="Sheet1";
			public List<string> ListTitleLines=new List<string>();
			///<summary>Null: no header row at all (a "Read Me" sheet).</summary>
			public string[] ArrayHeaders=ArrayHeadersDefault;
			public List<TestValueSetRow> ListRows=new List<TestValueSetRow>();
			///<summary>Every text as an inline string (t="inlineStr") instead of a shared string, as some exporters write it.</summary>
			public bool UseInlineStrings;
		}

		///<summary>An Excel workbook (.xlsx bytes) with the sheets. Text goes into the shared strings (or inline strings), as Excel writes it.</summary>
		public static byte[] MakeWorkbook(params Sheet[] arraySheets) {
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
					listCells.Add(new List<string> { title });
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
					stringBuilder.Append("</row>");
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
				+"<Override PartName=\"/xl/sharedStrings.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.sharedStrings+xml\"/>");
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
			stringBuilderRels.Append("<Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/sharedStrings\" Target=\"sharedStrings.xml\"/>");
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
			AddEntry(zipArchive,"xl/sharedStrings.xml",stringBuilderShared.ToString());
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
						+"\" codeSystemVersion=\"TEST\" codeSystem=\""+SecurityElement.Escape(row.CodeSystemOID)+"\" displayName=\""+SecurityElement.Escape(row.Description)+"\"/>\r\n");
				}
				stringBuilder.Append("    </ns0:ConceptList>\r\n");
				stringBuilder.Append("    <ns0:Source>TEST steward</ns0:Source>\r\n    <ns0:Type>Extensional</ns0:Type>\r\n    <ns0:Binding>Static</ns0:Binding>\r\n"
					+"    <ns0:Status>Active</ns0:Status>\r\n    <ns0:RevisionDate>2013-06-14</ns0:RevisionDate>\r\n");
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
		///systems for the rest), with the manifest's measures (as CMS IDs), so the file names all nine measures at their versions.</summary>
		public static List<TestValueSetRow> MakeManifestRows(string prefix="TESTM") {
			List<TestValueSetRow> listRows=new List<TestValueSetRow>();
			int index=0;
			foreach(EhrValueSetManifestItem item in EhrValueSetImport.GetManifest()) {
				index++;
				listRows.Add(TestValueSetRow.Make(item.ValueSetOID,"SNOMEDCT",prefix+index.ToString("D4")));
			}
			return listRows;
		}

		///<summary>A large synthetic release of countRows rows: a third in the manifest's value sets (with their measures), the rest in 400 other
		///made-up value sets under the made-up arc 2.16.840.1.113883.3.999999 (with made-up measures CMS1000v3 to CMS1049v3), as the real release
		///holds the value sets of every EP measure.</summary>
		public static List<TestValueSetRow> MakeLargeRelease(int countRows) {
			List<TestValueSetRow> listRows=new List<TestValueSetRow>();
			List<EhrValueSetManifestItem> listManifest=EhrValueSetImport.GetManifest();
			string[] arrayCodeSystems=new[] { "SNOMEDCT","ICD9CM","ICD10CM","CPT","LOINC","RXNORM","HCPCS","CDT" };
			for(int i=0;i<countRows;i++) {
				string codeSystem=arrayCodeSystems[i%arrayCodeSystems.Length];
				TestValueSetRow row;
				if(i%3==0) {
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
	}
	///<summary>A zip file whose entries are stored (method 0) with their CRC-32 and UTF-8 names: the local headers, the central directory and the
	///end record of the ZIP format (PKWARE APPNOTE, section 4.3), which ZipArchive reads.</summary>
	public class StoredZip {
		private readonly MemoryStream _memoryStream=new MemoryStream();
		private readonly MemoryStream _memoryStreamCentral=new MemoryStream();
		private int _countEntries;
		private static uint[] _arrayCrcTable;

		public void Add(string name,byte[] arrayBytes) {
			byte[] arrayName=new UTF8Encoding(false).GetBytes(name);
			uint crc=GetCrc32(arrayBytes);
			long offset=_memoryStream.Position;
			BinaryWriter writer=new BinaryWriter(_memoryStream);
			writer.Write(0x04034b50u);//Local file header.
			writer.Write((ushort)20);//Version needed.
			writer.Write((ushort)0x0800);//Flags: UTF-8 names.
			writer.Write((ushort)0);//Stored.
			writer.Write((ushort)0);//Time.
			writer.Write((ushort)0x5B6E);//Date: 2025-11-14.
			writer.Write(crc);
			writer.Write((uint)arrayBytes.Length);
			writer.Write((uint)arrayBytes.Length);
			writer.Write((ushort)arrayName.Length);
			writer.Write((ushort)0);
			writer.Write(arrayName);
			writer.Write(arrayBytes);
			writer.Flush();
			BinaryWriter writerCentral=new BinaryWriter(_memoryStreamCentral);
			writerCentral.Write(0x02014b50u);//Central directory header.
			writerCentral.Write((ushort)20);//Version made by.
			writerCentral.Write((ushort)20);//Version needed.
			writerCentral.Write((ushort)0x0800);
			writerCentral.Write((ushort)0);
			writerCentral.Write((ushort)0);
			writerCentral.Write((ushort)0x5B6E);
			writerCentral.Write(crc);
			writerCentral.Write((uint)arrayBytes.Length);
			writerCentral.Write((uint)arrayBytes.Length);
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
			uint crc=0xFFFFFFFFu;
			foreach(byte b in arrayBytes) {
				crc=_arrayCrcTable[(crc^b)&0xFF]^(crc>>8);
			}
			return crc^0xFFFFFFFFu;
		}
	}
}
