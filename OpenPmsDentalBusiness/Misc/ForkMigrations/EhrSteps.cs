// The EHR value sets (docs/specs/ehr-rebuild-plan.md, package EHR-2): the fork-only table ehrcode and the step that creates it, level 2026111401
// of the EHR range 2026111401 to 2026111410 (the nine levels after it are kept for the EHR packages). No step writes a row: the office fills the
// table with its own import of VSAC's June 2013 EP release (EhrValueSetImport, Setup, Chart, EHR, Code System Importer).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;

namespace OpenPmsDentalBusiness.ForkMigrations.FeatureSteps {
	///<summary>The EHR value-set table, exactly as SHOW CREATE TABLE prints it on MariaDB 10.11. Not Open Dental schema: Open Dental's 24.3 code
	///declares the type but keeps the rows in its closed EHR.dll, and no convert script creates the table, so it was one of the schema verifier's
	///HQ-only tables until this package. The schema verifier allows it as an extra table and ForkTables.VerifyDatabase checks it. The step below
	///creates it empty and writes no row into it, so on an adopted database it changes nothing else, and the adoption tools hold it to empty.</summary>
	public class EhrTables:ForkTableSet {
		public const string TABLE_EHR_CODE="ehrcode";

		///<summary>One row per code and value set (a code in several value sets has a row for each), with the columns of EhrCode except IsInDb, which
		///is worked out when the list is loaded. Description is text, because some value-set descriptions are longer than 255 characters. The key
		///ValueSetOID serves the lookups by value set (EhrCodes.GetForValueSetOIDs); CodeValueCodeSystem serves the lookups by code and code system,
		///on prefixes, because both columns in full would pass MyISAM's 1000-byte key limit (utf8mb3 counts 3 bytes a character).</summary>
		public const string CREATE_EHR_CODE=@"CREATE TABLE `ehrcode` (
  `EhrCodeNum` bigint(20) NOT NULL AUTO_INCREMENT,
  `MeasureIds` varchar(255) NOT NULL,
  `ValueSetName` varchar(255) NOT NULL,
  `ValueSetOID` varchar(255) NOT NULL,
  `QDMCategory` varchar(255) NOT NULL,
  `CodeValue` varchar(255) NOT NULL,
  `Description` text NOT NULL,
  `CodeSystem` varchar(255) NOT NULL,
  `CodeSystemOID` varchar(255) NOT NULL,
  PRIMARY KEY (`EhrCodeNum`),
  KEY `ValueSetOID` (`ValueSetOID`),
  KEY `CodeValueCodeSystem` (`CodeValue`(100),`CodeSystem`(50))
) ENGINE=MyISAM DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci";

		public override string Feature {
			get { return "EHR value sets"; }
		}

		public override string Levels {
			get { return "2026111401"; }
		}

		public override string[] ArrayCreateTables {
			get { return new[] { CREATE_EHR_CODE }; }
		}
	}

	///<summary>EHR value sets: the table the office's import of the June 2013 EP release fills. Creates the empty table; writes no row. A table of
	///that name with another definition (made by something other than this step) stops the step with a message that names the differences: the
	///step never changes or drops a table it did not make.</summary>
	[ForkMigration(2026111401,"EHR value sets: add the ehrcode table")]
	public class EhrCodeTableStep:ForkMigrationStep {
		public override void Apply(ForkMigrationContext context) {
			if(context.CreateTableIfMissing(EhrTables.CREATE_EHR_CODE)) {
				return;
			}
			SchemaTable tableActual=context.GetTable(EhrTables.TABLE_EHR_CODE);
			string error=GetDefinitionError(tableActual);
			if(error!="") {
				throw new ApplicationException(error);
			}
		}

		///<summary>"" when the table is exactly the declaration (or is missing); otherwise the message the step stops with, naming each difference.</summary>
		public static string GetDefinitionError(SchemaTable tableActual) {
			if(tableActual==null) {
				return "";
			}
			SchemaSnapshot snapshotExpected=new SchemaSnapshot();
			snapshotExpected.AddTable(SchemaTable.Parse(EhrTables.CREATE_EHR_CODE));
			SchemaSnapshot snapshotActual=new SchemaSnapshot();
			snapshotActual.AddTable(tableActual);
			SchemaVerifyResult result=SchemaVerifier.Compare(snapshotExpected,snapshotActual);
			if(result.IsMatch) {
				return "";
			}
			return "The database already has a table named "+EhrTables.TABLE_EHR_CODE+" whose definition is not the one this program uses for the EHR "
				+"value sets, so the fork migration stopped and changed nothing. Restore the backup, or rename that table after checking what it "
				+"holds, and start the program again.\r\n"+result.GetReport();
		}
	}
}
