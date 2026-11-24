// UI-BRAND-2 (docs/port-notes/UI-BRAND.md section 10): fork migration step 2026112451 renames Dental Studio's and Podium's program property
// descriptions that named Open Dental, on a new database (the program and programproperty tables of db/init/new-database.sql) and on an
// existing office's database (synthetic rows as Open Dental's conversions wrote them, with the office's own values), each run twice. The
// scratch databases are named uibrand2_<run>_<purpose> and dropped afterwards. Synthetic data only.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySqlConnector;
using OpenPmsDentalBusiness;
using OpenPmsDentalBusiness.ForkMigrations;
using OpenPmsDentalBusiness.ForkMigrations.FeatureSteps;

namespace ForkMigrationTests {
	[TestClass]
	public class ProgramPropertyDescRenameStepTests {
		private const string PREFIX="uibrand2_";
		private const string PROGRAM_COLUMNS="ProgramNum,ProgName,ProgDesc,Enabled,Path,CommandLine,Note,PluginDllName,ButtonImage,FileTemplate,FilePath,IsDisabledByHq,CustErr";
		private const string PROPERTY_COLUMNS="ProgramPropertyNum,ProgramNum,PropertyDesc,PropertyValue,ComputerName,ClinicNum,IsMasked,IsHighSecurity";
		private static readonly string _runId=DateTime.UtcNow.ToString("yyMMddHHmmss")+"_"+System.Diagnostics.Process.GetCurrentProcess().Id;

		[TestMethod]
		public void Step_HasItsDatedLevel_AndTheCodeReadsTheNewTexts() {
			List<ForkMigrationStep> listSteps=ForkMigrationRunner.GetSteps();
			ForkMigrationStep step=listSteps.Single(x => x is ProgramPropertyDescRenameStep);
			Assert.AreEqual(2026112451,step.Level);
			ForkMigrationRunner.ValidateStep(step);
			ForkMigrationRunner.ValidateLevelsUnique(listSteps);
			Assert.IsTrue(step.GetType().Namespace.EndsWith(".FeatureSteps"),"A feature data step, not an Open Dental schema step.");
			Assert.AreEqual("UserName (clear to use the Open PMS username)",ProgramPropertyDescRenames.DentalStudioUserName);
			Assert.AreEqual("Enter 0 to use Open PMS for sending review invitations, or 1 to use the Service",ProgramPropertyDescRenames.PodiumUseService);
			Assert.AreEqual(ProgramPropertyDescRenames.PodiumUseService,Podium.PropertyDescs.UseService,"Podium's setup window looks the row up by the new text.");
			Assert.AreEqual(2,ProgramPropertyDescRenames.ListRenames.Count);
			foreach(ProgramPropertyDescRenames.Rename rename in ProgramPropertyDescRenames.ListRenames) {
				Assert.IsTrue(rename.DescNew.Length<=255,"programproperty.PropertyDesc is varchar(255).");
				StringAssert.Contains(rename.DescNew,"Open PMS");
			}
			string readme=File.ReadAllText(TestDb.FindRepoFile(Path.Combine("OpenPmsDentalBusiness","Misc","ForkMigrations","README.md")));
			StringAssert.Contains(readme,"| 2026112451 | `ProgramPropertyDescRenameStep.cs`");
		}

		///<summary>A new database: the program and programproperty tables exactly as db/init/new-database.sql has them. After the step the two rows
		///carry the new texts with their values (the step renames them, or the init file already has them once it is regenerated), nothing else in
		///the two tables changed, and a second run changes nothing.</summary>
		[TestMethod]
		public void Step_NewDatabase_GivesTheNewTexts_SecondRunChangesNothing() {
			RequireServer();
			string database=CreateScratch("new");
			try {
				string sql=File.ReadAllText(TestDb.FindRepoFile(Path.Combine("db","init","new-database.sql")),new UTF8Encoding(false));
				foreach(string table in new[] { "program","programproperty" }) {
					DataCore.NonQ(ExtractStatement(sql,"CREATE TABLE `"+table+"` ("));
					DataCore.NonQ(ExtractStatement(sql,"INSERT INTO `"+table+"` ("));
				}
				string checksumOtherRows=Checksum("programproperty WHERE ProgramNum NOT IN (SELECT ProgramNum FROM program WHERE ProgName IN ('DentalStudio','Podium'))");
				string checksumPrograms=Checksum("program");
				long countProperties=Count("programproperty");
				RunStep(database);
				Assert.AreEqual("Admin",Value("DentalStudio",ProgramPropertyDescRenames.DentalStudioUserName));
				Assert.AreEqual("0",Value("Podium",ProgramPropertyDescRenames.PodiumUseService));
				Assert.AreEqual(0,Count("programproperty WHERE PropertyDesc IN ('"+POut.String(ProgramPropertyDescRenames.DentalStudioUserNameOld)+"','"
					+POut.String(ProgramPropertyDescRenames.PodiumUseServiceOld)+"')"),"No old text is left.");
				Assert.AreEqual(countProperties,Count("programproperty"),"No row added or removed.");
				Assert.AreEqual(checksumOtherRows,Checksum("programproperty WHERE ProgramNum NOT IN (SELECT ProgramNum FROM program WHERE ProgName IN ('DentalStudio','Podium'))"));
				Assert.AreEqual(checksumPrograms,Checksum("program"),"program is not changed.");
				string checksumAfter=Checksum("programproperty");
				List<string> listChangesAgain=RunStep(database);
				Assert.AreEqual(0,listChangesAgain.Count,"A second run changes nothing: "+string.Join("\r\n",listChangesAgain));
				Assert.AreEqual(checksumAfter,Checksum("programproperty"));
			}
			finally {
				DropScratch(database);
			}
		}

		///<summary>An existing office's database, as Open Dental's conversions left it, with the office's own values: the Dental Studio and Podium
		///rows of every clinic are renamed (PropertyDesc only, values and every other column kept), whatever the case of ProgName; the same old text
		///under another program, and a row whose program already has the new text for its clinic, stay. A second run changes nothing.</summary>
		[TestMethod]
		public void Step_ExistingDatabase_RenamesOnlyTheOldTexts_KeepsValues_SecondRunChangesNothing() {
			RequireServer();
			string database=CreateScratch("existing");
			try {
				string sql=File.ReadAllText(TestDb.FindRepoFile(Path.Combine("db","init","new-database.sql")),new UTF8Encoding(false));
				DataCore.NonQ(ExtractStatement(sql,"CREATE TABLE `program` ("));
				DataCore.NonQ(ExtractStatement(sql,"CREATE TABLE `programproperty` ("));
				string dentalStudioOld=POut.String(ProgramPropertyDescRenames.DentalStudioUserNameOld);
				string podiumOld=POut.String(ProgramPropertyDescRenames.PodiumUseServiceOld);
				DataCore.NonQ("INSERT INTO program ("+PROGRAM_COLUMNS+") VALUES "
					+"(68,'DentalStudio','Dental Studio',1,'DS.exe','','','','','','',0,''),"
					+"(85,'podium ','Podium',1,'','','','','','','',0,''),"
					+"(90,'SomeOtherLink','Some other link',0,'','','','','','','',0,'')");
				DataCore.NonQ("INSERT INTO programproperty ("+PROPERTY_COLUMNS+") VALUES "
					+"(92,68,'"+dentalStudioOld+"','frontdesk','',0,0,0),"
					+"(93,68,'UserPassword','secret-synthetic','',0,1,0),"
					+"(136,85,'"+podiumOld+"','1','',0,0,0),"
					+"(137,85,'"+podiumOld+"','0','',5,0,0),"
					+"(138,85,'"+podiumOld+"','1','',6,0,0),"
					+"(139,85,'"+POut.String(ProgramPropertyDescRenames.PodiumUseService)+"','0','',6,0,0),"
					+"(140,90,'"+dentalStudioOld+"','kept','',0,0,0)");
				List<string> listChanges=RunStep(database);
				CollectionAssert.AreEqual(new[] {
					"Renamed the DentalStudio program property (ProgramPropertyNum 92, ProgramNum 68, ClinicNum 0) to '"+ProgramPropertyDescRenames.DentalStudioUserName+"'",
					"Renamed the Podium program property (ProgramPropertyNum 136, ProgramNum 85, ClinicNum 0) to '"+ProgramPropertyDescRenames.PodiumUseService+"'",
					"Renamed the Podium program property (ProgramPropertyNum 137, ProgramNum 85, ClinicNum 5) to '"+ProgramPropertyDescRenames.PodiumUseService+"'",
					"Kept the Podium program property (ProgramPropertyNum 138) under its old description: its program already has '"
						+ProgramPropertyDescRenames.PodiumUseService+"' for ClinicNum 6",
				},listChanges,string.Join("\r\n",listChanges));
				Assert.AreEqual(ProgramPropertyDescRenames.DentalStudioUserName+"|frontdesk|0",Row(92));
				Assert.AreEqual("UserPassword|secret-synthetic|0",Row(93));
				Assert.AreEqual(ProgramPropertyDescRenames.PodiumUseService+"|1|0",Row(136));
				Assert.AreEqual(ProgramPropertyDescRenames.PodiumUseService+"|0|5",Row(137));
				Assert.AreEqual(ProgramPropertyDescRenames.PodiumUseServiceOld+"|1|6",Row(138),"Its program has the new text for clinic 6 already.");
				Assert.AreEqual(ProgramPropertyDescRenames.PodiumUseService+"|0|6",Row(139));
				Assert.AreEqual(ProgramPropertyDescRenames.DentalStudioUserNameOld+"|kept|0",Row(140),"Another program's row is not Dental Studio's.");
				Assert.AreEqual(7,Count("programproperty"));
				string checksumAfter=Checksum("programproperty");
				List<string> listChangesAgain=RunStep(database);
				Assert.IsFalse(listChangesAgain.Any(x => x.Contains("Renamed")),"A second run renames nothing: "+string.Join("\r\n",listChangesAgain));
				Assert.AreEqual(checksumAfter,Checksum("programproperty"));
			}
			finally {
				DropScratch(database);
			}
		}

		///<summary>The plan reads nothing: on rows alone it renames exactly the old texts of the two programs.</summary>
		[TestMethod]
		public void Plan_OnRows_RenamesOnlyTheOldTextsOfTheTwoPrograms() {
			List<DxcCreditScoreRemoved.ProgramRow> listPrograms=new List<DxcCreditScoreRemoved.ProgramRow> {
				new DxcCreditScoreRemoved.ProgramRow { ProgramNum=1,ProgName="DENTALSTUDIO" },
				new DxcCreditScoreRemoved.ProgramRow { ProgramNum=2,ProgName="Podium" },
			};
			List<ProgramPropertyDescRenames.PropertyRow> listRows=new List<ProgramPropertyDescRenames.PropertyRow> {
				new ProgramPropertyDescRenames.PropertyRow { ProgramPropertyNum=10,ProgramNum=1,PropertyDesc=ProgramPropertyDescRenames.DentalStudioUserNameOld },
				new ProgramPropertyDescRenames.PropertyRow { ProgramPropertyNum=11,ProgramNum=1,PropertyDesc=ProgramPropertyDescRenames.DentalStudioUserNameOld.ToUpperInvariant() },
				new ProgramPropertyDescRenames.PropertyRow { ProgramPropertyNum=12,ProgramNum=2,PropertyDesc=ProgramPropertyDescRenames.DentalStudioUserNameOld },
				new ProgramPropertyDescRenames.PropertyRow { ProgramPropertyNum=13,ProgramNum=2,PropertyDesc=ProgramPropertyDescRenames.PodiumUseServiceOld },
				new ProgramPropertyDescRenames.PropertyRow { ProgramPropertyNum=14,ProgramNum=3,PropertyDesc=ProgramPropertyDescRenames.PodiumUseServiceOld },
			};
			ProgramPropertyDescRenames.RowPlan plan=ProgramPropertyDescRenames.Plan(listPrograms,listRows);
			CollectionAssert.AreEqual(new long[] { 10,13 },plan.ListRenamed.Select(x => x.Row.ProgramPropertyNum).ToList());
			Assert.AreEqual(0,plan.ListKept.Count);
			Assert.IsTrue(plan.IsChanging);
			Assert.IsFalse(ProgramPropertyDescRenames.Plan(listPrograms,new List<ProgramPropertyDescRenames.PropertyRow>()).IsChanging);
		}

		private static void RequireServer() {
			string skipReason=TestDb.GetSkipReason();
			if(skipReason!=null) {
				Assert.Inconclusive(skipReason);
			}
		}

		private static string CreateScratch(string purpose) {
			string name=PREFIX+_runId+"_"+purpose;
			using(MySqlConnection connection=TestDb.OpenServer()) {
				TestDb.Exec(connection,"DROP DATABASE IF EXISTS `"+name+"`");
				TestDb.Exec(connection,"CREATE DATABASE `"+name+"` CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci");
			}
			TestDb.Connect(name);
			return name;
		}

		private static void DropScratch(string name) {
			if(!name.StartsWith(PREFIX,StringComparison.Ordinal)) {
				throw new ArgumentException("These tests only drop databases whose name starts with "+PREFIX);
			}
			using(MySqlConnection connection=TestDb.OpenServer()) {
				TestDb.Exec(connection,"DROP DATABASE IF EXISTS `"+name+"`");
			}
		}

		private static List<string> RunStep(string database) {
			ForkMigrationContext context=new ForkMigrationContext(database);
			new ProgramPropertyDescRenameStep().Apply(context);
			return context.ListChanges.ToList();
		}

		///<summary>The statement of the init file that starts with start, up to the semicolon that ends a line.</summary>
		private static string ExtractStatement(string sql,string start) {
			int index=sql.IndexOf(start,StringComparison.Ordinal);
			Assert.IsTrue(index>=0,"db/init/new-database.sql has no "+start);
			int indexEnd=index;
			while(true) {
				indexEnd=sql.IndexOf(';',indexEnd);
				Assert.IsTrue(indexEnd>=0,"No end of "+start);
				if(indexEnd+1>=sql.Length || sql[indexEnd+1]=='\n' || sql[indexEnd+1]=='\r') {
					break;
				}
				indexEnd++;
			}
			return sql.Substring(index,indexEnd-index+1);
		}

		private static string Value(string progName,string propertyDesc) {
			return DataCore.GetScalar("SELECT pp.PropertyValue FROM programproperty pp INNER JOIN program p ON p.ProgramNum=pp.ProgramNum "
				+"WHERE p.ProgName='"+POut.String(progName)+"' AND pp.ClinicNum=0 AND pp.PropertyDesc='"+POut.String(propertyDesc)+"'");
		}

		private static string Row(long programPropertyNum) {
			return DataCore.GetScalar("SELECT CONCAT(PropertyDesc,'|',PropertyValue,'|',ClinicNum) FROM programproperty WHERE ProgramPropertyNum="
				+POut.Long(programPropertyNum));
		}

		private static long Count(string tableAndWhere) {
			return PIn.Long(DataCore.GetScalar("SELECT COUNT(*) FROM "+tableAndWhere));
		}

		///<summary>The number of the selected rows and a checksum of every column of them (program or programproperty, by the table name; the
		///nullable PropertyValue and Note also by whether they are NULL).</summary>
		private static string Checksum(string tableAndWhere) {
			bool isProperty=tableAndWhere.StartsWith("programproperty",StringComparison.Ordinal);
			string columns=(isProperty ? PROPERTY_COLUMNS : PROGRAM_COLUMNS)+","+(isProperty ? "ISNULL(PropertyValue)" : "ISNULL(Note)");
			return DataCore.GetScalar("SELECT CONCAT(COUNT(*),'-',IFNULL(SUM(CRC32(CONCAT_WS('|',"+columns+"))),0)) FROM "+tableAndWhere);
		}
	}
}
