// Unit tests of the CREATE TABLE parser and the schema comparison. No database.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalBusiness.ForkMigrations;

namespace ForkMigrationTests {
	[TestClass]
	public class SchemaSnapshotTests {
		private const string DdlFee=@"--
-- Table structure for table `fee`
--

DROP TABLE IF EXISTS `fee`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
CREATE TABLE `fee` (
  `FeeNum` bigint(20) NOT NULL AUTO_INCREMENT,
  `Amount` double NOT NULL DEFAULT 0,
  `OldCode` varchar(15) CHARACTER SET utf8mb3 COLLATE utf8mb3_bin NOT NULL DEFAULT '',
  `SecDateTEdit` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `Note` text NOT NULL,
  PRIMARY KEY (`FeeNum`),
  KEY `indexADACode` (`OldCode`),
  UNIQUE KEY `AmountNote` (`Amount`,`Note`(30)) USING BTREE
) ENGINE=MyISAM AUTO_INCREMENT=1234 DEFAULT CHARSET=utf8 COLLATE=utf8_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
INSERT INTO `fee` VALUES (1,2,'x',NOW(),'');
";

		[TestMethod]
		public void Parse_ReadsColumnsKeysAndOptions() {
			SchemaSnapshot snapshot=SchemaSnapshot.Parse(DdlFee);
			Assert.AreEqual(1,snapshot.Tables.Count);
			SchemaTable table=snapshot.GetTable("FEE");
			Assert.IsNotNull(table,"Table names are matched without case.");
			CollectionAssert.AreEqual(new[] { "FeeNum","Amount","OldCode","SecDateTEdit","Note" },table.Columns.Select(x => x.Name).ToList());
			Assert.AreEqual("bigint(20) NOT NULL AUTO_INCREMENT",table.GetColumn("FeeNum").Definition);
			Assert.AreEqual("varchar(15) CHARACTER SET utf8mb3 COLLATE utf8mb3_bin NOT NULL DEFAULT ''",table.GetColumn("OldCode").Definition);
			Assert.AreEqual("timestamp NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp()",table.GetColumn("SecDateTEdit").Definition,
				"CURRENT_TIMESTAMP spellings are normalized.");
			Assert.AreEqual(3,table.GetColumn("OldCode").Position);
			Assert.AreEqual(3,table.Indexes.Count);
			Assert.IsTrue(table.GetIndex("PRIMARY").IsPrimary);
			Assert.AreEqual("KEY (`OldCode`)",table.GetIndex("indexadacode").Definition);
			SchemaIndex unique=table.GetIndex("AmountNote");
			Assert.AreEqual("UNIQUE KEY",unique.Kind);
			Assert.AreEqual("`Amount`,`Note`(30)",unique.Columns);
			Assert.AreEqual("USING BTREE",unique.Options);
			Assert.AreEqual("MyISAM",table.Engine);
			Assert.AreEqual("utf8mb3",table.Charset,"utf8 is the old name of utf8mb3.");
			Assert.AreEqual("utf8mb3_general_ci",table.Collation);
			Assert.AreEqual("",table.OtherOptions,"AUTO_INCREMENT is data, not schema.");
		}

		[TestMethod]
		public void ToDdl_RoundTrips() {
			SchemaSnapshot snapshot=SchemaSnapshot.Parse(DdlFee);
			string ddl=snapshot.ToDdl("header line");
			StringAssert.StartsWith(ddl,"-- header line");
			StringAssert.DoesNotMatch(ddl,new System.Text.RegularExpressions.Regex("AUTO_INCREMENT=\\d"));
			SchemaSnapshot again=SchemaSnapshot.Parse(ddl);
			Assert.AreEqual(0,SchemaVerifier.Compare(snapshot,again).ListDifferences.Count);
			Assert.AreEqual(ddl,again.ToDdl("header line"));
		}

		[TestMethod]
		public void Parse_CollationDefaultsFromCharset() {
			SchemaSnapshot snapshot=SchemaSnapshot.Parse("CREATE TABLE `equipment` (\n  `A` text NOT NULL\n) ENGINE=MyISAM DEFAULT CHARSET=latin1;\n");
			Assert.AreEqual("latin1",snapshot.GetTable("equipment").Charset);
			Assert.AreEqual("latin1_swedish_ci",snapshot.GetTable("equipment").Collation);
		}

		[TestMethod]
		public void Compare_FindsEveryKindOfDifference() {
			SchemaSnapshot expected=SchemaSnapshot.Parse(@"CREATE TABLE `a` (
  `Num` bigint(20) NOT NULL AUTO_INCREMENT,
  `B` tinyint(4) NOT NULL,
  `C` varchar(25) DEFAULT NULL,
  `D` date NOT NULL DEFAULT '0001-01-01',
  PRIMARY KEY (`Num`),
  KEY `B` (`B`),
  KEY `CD` (`C`,`D`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;
CREATE TABLE `gone` (
  `Num` bigint(20) NOT NULL AUTO_INCREMENT,
  PRIMARY KEY (`Num`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;
");
			SchemaSnapshot actual=SchemaSnapshot.Parse(@"CREATE TABLE `a` (
  `Num` bigint(20) NOT NULL AUTO_INCREMENT,
  `D` date NOT NULL,
  `C` varchar(50) DEFAULT NULL,
  `E` int(11) NOT NULL,
  PRIMARY KEY (`Num`),
  KEY `CD` (`C`),
  KEY `E` (`E`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
CREATE TABLE `extra` (
  `Num` bigint(20) NOT NULL AUTO_INCREMENT,
  PRIMARY KEY (`Num`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;
");
			SchemaVerifyResult result=SchemaVerifier.Compare(expected,actual);
			Assert.IsFalse(result.IsMatch);
			string report=result.GetReport();
			AssertHas(result,SchemaDifferenceKind.MissingTable,"gone",null,report);
			AssertHas(result,SchemaDifferenceKind.ExtraTable,"extra",null,report);
			AssertHas(result,SchemaDifferenceKind.TableOptions,"a","ENGINE",report);
			AssertHas(result,SchemaDifferenceKind.TableOptions,"a","CHARSET",report);
			AssertHas(result,SchemaDifferenceKind.TableOptions,"a","COLLATE",report);
			AssertHas(result,SchemaDifferenceKind.MissingColumn,"a","B",report);
			AssertHas(result,SchemaDifferenceKind.ExtraColumn,"a","E",report);
			AssertHas(result,SchemaDifferenceKind.ColumnDefinition,"a","C",report);
			AssertHas(result,SchemaDifferenceKind.ColumnDefinition,"a","D",report);
			AssertHas(result,SchemaDifferenceKind.ColumnOrder,"a",null,report);
			AssertHas(result,SchemaDifferenceKind.MissingIndex,"a","B",report);
			AssertHas(result,SchemaDifferenceKind.ExtraIndex,"a","E",report);
			AssertHas(result,SchemaDifferenceKind.IndexDefinition,"a","CD",report);
			Assert.AreEqual(13,result.ListDifferences.Count,report);
		}

		///<summary>Package ADOPT-PORTAL-EMAIL: adoption keeps the patient portal column patient._portal_active_email and its key, and every reference allows them
		///only with exactly the expected definitions. Any other definition, kind or table is a mismatch.</summary>
		[TestMethod]
		public void Compare_PortalActiveEmail_AllowedOnlyWithTheOfficesDefinition() {
			const string patientStart="CREATE TABLE `patient` (\n  `PatNum` bigint(20) NOT NULL,\n  `PatStatus` tinyint(3) unsigned NOT NULL DEFAULT 0,\n"
				+"  `Email` varchar(100) DEFAULT '',\n";
			const string patientEnd="  PRIMARY KEY (`PatNum`)";
			const string options="\n) ENGINE=MyISAM DEFAULT CHARSET=utf8mb3;\n";
			string columnOffice="  `_portal_active_email` "+SchemaVerifier.PortalActiveEmailColumnDefinition+",\n";
			string indexOffice=",\n  UNIQUE KEY `portal_active_email_uniq` (`_portal_active_email`)";
			SchemaSnapshot expected=SchemaSnapshot.Parse(patientStart+patientEnd+options);
			SchemaSnapshot actualOffice=SchemaSnapshot.Parse(patientStart+columnOffice+patientEnd+indexOffice+options);
			//The constants are what the parser (and SHOW CREATE TABLE) gives for the expected DDL.
			Assert.AreEqual(SchemaVerifier.PortalActiveEmailColumnDefinition,actualOffice.GetTable("patient").GetColumn("_portal_active_email").Definition);
			Assert.AreEqual(SchemaVerifier.PortalActiveEmailIndexDefinition,actualOffice.GetTable("patient").GetIndex("portal_active_email_uniq").Definition);
			foreach(List<SchemaException> listExceptions in new[] { SchemaVerifier.DocumentedExceptions,SchemaVerifier.DocumentedExceptions26_1_70,
				SchemaVerifier.DocumentedExceptions25_3_48 })
			{
				SchemaVerifyResult result=SchemaVerifier.Compare(expected,actualOffice,listExceptions);
				Assert.IsTrue(result.IsMatch,result.GetReport());
				Assert.AreEqual(2,result.ListAllowed.Count,result.GetReport());
				Assert.IsTrue(result.ListAllowed.All(x => x.ExceptionReason.Contains("keep")),result.GetReport());
				//Without the column and key it matches too (a database that never had them).
				Assert.AreEqual(0,SchemaVerifier.Compare(expected,expected,listExceptions).ListDifferences.Count);
			}
			string[] arrayColumnsOther=new[] {
				"  `_portal_active_email` varchar(100) GENERATED ALWAYS AS (case when `PatStatus` = 0 then `Email` else NULL end) VIRTUAL,\n",
				"  `_portal_active_email` varchar(100) GENERATED ALWAYS AS (case when `PatStatus` = 0 and `Email` <> '' then `Email` else NULL end) STORED,\n",
				"  `_portal_active_email` varchar(255) GENERATED ALWAYS AS (case when `PatStatus` = 0 and `Email` <> '' then `Email` else NULL end) VIRTUAL,\n",
				"  `_portal_active_email` varchar(100) DEFAULT NULL,\n",
			};
			foreach(string columnOther in arrayColumnsOther) {
				SchemaVerifyResult result=SchemaVerifier.Compare(expected,SchemaSnapshot.Parse(patientStart+columnOther+patientEnd+indexOffice+options),
					SchemaVerifier.DocumentedExceptions);
				Assert.IsFalse(result.IsMatch,columnOther);
				Assert.AreEqual(1,result.ListMismatches.Count,result.GetReport());
				Assert.AreEqual(SchemaDifferenceKind.ExtraColumn,result.ListMismatches[0].Kind);
			}
			string[] arrayIndexesOther=new[] {
				",\n  KEY `portal_active_email_uniq` (`_portal_active_email`)",
				",\n  UNIQUE KEY `portal_active_email_uniq` (`_portal_active_email`,`PatNum`)",
				",\n  UNIQUE KEY `portal_active_email_uniq` (`Email`)",
			};
			foreach(string indexOther in arrayIndexesOther) {
				SchemaVerifyResult result=SchemaVerifier.Compare(expected,SchemaSnapshot.Parse(patientStart+columnOffice+patientEnd+indexOther+options),
					SchemaVerifier.DocumentedExceptions);
				Assert.IsFalse(result.IsMatch,indexOther);
				Assert.AreEqual(1,result.ListMismatches.Count,result.GetReport());
				Assert.AreEqual(SchemaDifferenceKind.ExtraIndex,result.ListMismatches[0].Kind);
			}
			//Another key on the column, or the column on another table, is not allowed.
			SchemaVerifyResult resultExtraKey=SchemaVerifier.Compare(expected,SchemaSnapshot.Parse(patientStart+columnOffice+patientEnd+indexOffice
				+",\n  KEY `idx_portal` (`_portal_active_email`)"+options),SchemaVerifier.DocumentedExceptions);
			Assert.AreEqual(1,resultExtraKey.ListMismatches.Count,resultExtraKey.GetReport());
			Assert.AreEqual("idx_portal",resultExtraKey.ListMismatches[0].Item);
			string definition="CREATE TABLE `definition` (\n  `DefNum` bigint(20) NOT NULL,\n  PRIMARY KEY (`DefNum`)"+options;
			string definitionWithColumn="CREATE TABLE `definition` (\n  `DefNum` bigint(20) NOT NULL,\n"+columnOffice+"  PRIMARY KEY (`DefNum`)"+options;
			SchemaVerifyResult resultOtherTable=SchemaVerifier.Compare(SchemaSnapshot.Parse(definition),SchemaSnapshot.Parse(definitionWithColumn),
				SchemaVerifier.DocumentedExceptions);
			Assert.IsFalse(resultOtherTable.IsMatch,resultOtherTable.GetReport());
		}

		[TestMethod]
		public void Compare_ExceptionsAllowDifferencesWithAReason() {
			SchemaSnapshot expected=SchemaSnapshot.Parse("CREATE TABLE `patient` (\n  `PatNum` bigint(20) NOT NULL\n) ENGINE=MyISAM DEFAULT CHARSET=utf8mb3;\n");
			SchemaSnapshot actual=SchemaSnapshot.Parse("CREATE TABLE `patient` (\n  `PatNum` bigint(20) NOT NULL,\n"
				+"  `_portal_active_email` varchar(100) GENERATED ALWAYS AS (case when `PatStatus` = 0 and `Email` <> '' then `Email` else NULL end) VIRTUAL,\n"
				+"  UNIQUE KEY `portal_active_email_uniq` (`_portal_active_email`)\n) ENGINE=MyISAM DEFAULT CHARSET=utf8mb3;\n"
				+"CREATE TABLE `job` (\n  `JobNum` bigint(20) NOT NULL\n) ENGINE=MyISAM DEFAULT CHARSET=utf8mb3;\n"
				+"CREATE TABLE `mystery` (\n  `Num` bigint(20) NOT NULL\n) ENGINE=MyISAM DEFAULT CHARSET=utf8mb3;\n");
			SchemaVerifyResult result=SchemaVerifier.Compare(expected,actual,SchemaVerifier.DocumentedExceptions25_3_48);
			Assert.IsFalse(result.IsMatch,"An unknown extra table is still a mismatch.");
			Assert.AreEqual(1,result.ListMismatches.Count,result.GetReport());
			Assert.AreEqual("mystery",result.ListMismatches[0].Table);
			Assert.AreEqual(3,result.ListAllowed.Count,result.GetReport());
			Assert.IsTrue(result.ListAllowed.All(x => !string.IsNullOrEmpty(x.ExceptionReason)));
			//An HQ-only table is allowed only as an extra table, never as a missing one.
			SchemaVerifyResult resultMissing=SchemaVerifier.Compare(actual,expected,SchemaVerifier.DocumentedExceptions25_3_48);
			Assert.IsTrue(resultMissing.ListMismatches.Any(x => x.Kind==SchemaDifferenceKind.MissingTable && x.Table=="job"));
		}

		private static void AssertHas(SchemaVerifyResult result,SchemaDifferenceKind kind,string table,string item,string report) {
			Assert.IsTrue(result.ListDifferences.Any(x => x.Kind==kind && x.Table==table && x.Item==item),kind+" "+table+"."+item+" not found in:\r\n"
				+report);
		}
	}
}
