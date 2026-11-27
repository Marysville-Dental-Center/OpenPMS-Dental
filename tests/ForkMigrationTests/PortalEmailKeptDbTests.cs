// Package ADOPT-PORTAL-EMAIL: adoption keeps the patient portal column patient._portal_active_email and its unique key (owner decision of 2026-11-25),
// the schema verifier allows them with exactly the expected definitions, and the program loads, saves, merges and backs up patients with them
// there. A duplicate active e-mail address on save is MariaDB's duplicate-key error for now (no friendly message yet; roadmap).
// A fresh development database (the seed dump and the 24.3 convert chain), the office's column and key added, then adopted with every fork
// migration step. Synthetic rows only. Skipped (inconclusive) when the local MariaDB does not answer.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Linq;
using DataConnectionBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySqlConnector;
using OpenPmsDentalBusiness;
using OpenPmsDentalBusiness.ForkMigrations;

namespace ForkMigrationTests {
	[TestClass]
	public class PortalEmailKeptDbTests {
		private static readonly string DB_NAME=TestDb.ScratchName("portalkept");
		private static string _skipReason;
		private static AdoptionResult _adoptionResult;
		private static int _patCounter;

		[ClassInitialize]
		public static void ClassInitialize(TestContext testContext) {
			_skipReason=TestDb.GetSkipReason();
			if(_skipReason!=null) {
				return;
			}
			TestDb.DropStaleScratchDatabases();
			Security.CurUser=Security.CurUser??new Userod { UserNum=1,UserName="Synthetic" };
			TestDb.CreateFresh24_3(DB_NAME);
			//The office's own column and key, exactly as the office's database has them.
			DataCore.NonQ("ALTER TABLE patient "+SchemaVerifier.PortalActiveEmailAddDdl);
			//An Open Dental 24.3.63 database as adoption expects it: ProgramVersion equal to DataBaseVersion, no update running.
			ForkVersions.SetMarker("ProgramVersion",ForkVersions.GetMarker("DataBaseVersion"));
			ForkVersions.SetMarker("UpdateInProgressOnComputerName","");
			_adoptionResult=Adoption.Run();
			Cache.Refresh(InvalidType.Prefs);
		}

		[ClassCleanup]
		public static void ClassCleanup() {
			if(_skipReason==null) {
				TestDb.Drop(DB_NAME);
			}
		}

		[TestInitialize]
		public void TestInitialize() {
			if(_skipReason!=null) {
				Assert.Inconclusive(_skipReason);
			}
			TestDb.Connect(DB_NAME);
		}

		private static bool HasColumn() {
			return DataCore.GetScalar("SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='patient' "
				+"AND COLUMN_NAME='"+SchemaVerifier.PortalActiveEmailColumn+"'")=="1";
		}

		private static bool HasIndex() {
			return DataCore.GetScalar("SELECT COUNT(*) FROM information_schema.STATISTICS WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='patient' "
				+"AND INDEX_NAME='"+SchemaVerifier.PortalActiveEmailIndex+"' AND NON_UNIQUE=0")=="1";
		}

		private static Patient NewPatient(string email,PatientStatus patientStatus=PatientStatus.Patient) {
			_patCounter++;
			Patient patient=new Patient {
				LName="Portalkept"+_patCounter.ToString("000"),
				FName="Synthetic",
				Email=email,
				PatStatus=patientStatus,
				Birthdate=new DateTime(1980,1,1),
			};
			Patients.Insert(patient,false);
			Patient patientOld=patient.Copy();
			patient.Guarantor=patient.PatNum;
			Patients.Update(patient,patientOld);
			return Patients.GetPat(patient.PatNum);
		}

		///<summary>Runs the save and returns MariaDB's error for it, or null when it saved.</summary>
		private static MySqlException SaveError(Action action) {
			try {
				action();
				return null;
			}
			catch(Exception ex) {
				Exception exception=ex;
				while(exception!=null && !(exception is MySqlException)) {
					exception=exception.InnerException;
				}
				Assert.IsNotNull(exception,"Not a MariaDB error: "+ex);
				return (MySqlException)exception;
			}
		}

		[TestMethod]
		public void Adoption_KeepsTheColumnAndKey_AndTheSchemaMatches() {
			Assert.IsTrue(_adoptionResult.IsPortalEmailColumnKept);
			Assert.IsTrue(_adoptionResult.IsPortalEmailIndexKept);
			Assert.AreEqual(0,_adoptionResult.ListIndexesDropped.Count);
			Assert.IsTrue(_adoptionResult.ListLog.Any(x => x.StartsWith("Kept column patient._portal_active_email and its unique key portal_active_email_uniq")),
				string.Join("\n",_adoptionResult.ListLog));
			Assert.IsTrue(HasColumn());
			Assert.IsTrue(HasIndex());
			SchemaTable tablePatient=SchemaTable.FromDatabase(DB_NAME,"patient");
			Assert.AreEqual(SchemaVerifier.PortalActiveEmailColumnDefinition,tablePatient.GetColumn(SchemaVerifier.PortalActiveEmailColumn).Definition);
			Assert.AreEqual(SchemaVerifier.PortalActiveEmailIndexDefinition,tablePatient.GetIndex(SchemaVerifier.PortalActiveEmailIndex).Definition);
			//Against Open Dental 26.2.0: the column and key are the one allowed difference on patient.
			SchemaVerifyResult result=SchemaVerifier.VerifyDatabase(DB_NAME);
			Assert.IsTrue(result.IsMatch,result.GetReport());
			CollectionAssert.AreEquivalent(new[] { "patient."+SchemaVerifier.PortalActiveEmailColumn,"patient."+SchemaVerifier.PortalActiveEmailIndex },
				result.ListDifferences.Where(x => x.Table=="patient").Select(x => x.Table+"."+x.Item).ToList(),result.GetReport());
			//The program's start: the fork migration runner wrote OdSchemaCompat 26.2.0, and the guard opens the database.
			Assert.AreEqual(SchemaVerifier.ReferenceVersion,ForkVersions.GetMarker(ForkVersions.PREF_OD_SCHEMA_COMPAT));
			ForkGuardResult forkGuardResult=ForkVersionGuard.Evaluate(ForkVersions.ReadState());
			Assert.AreEqual(ForkGuardAction.ProceedFork,forkGuardResult.Action,forkGuardResult.Message);
			Assert.IsFalse(forkGuardResult.HasPendingMigrations);
			//Every step again changes nothing and keeps the column.
			ForkMigrationResult resultAgain=ForkMigrationRunner.Run(new ForkMigrationOptions { RunAll=true });
			Assert.AreEqual(0,resultAgain.ListChanges.Count,string.Join("\n",resultAgain.ListChanges));
			Assert.IsTrue(HasColumn() && HasIndex());
		}

		[TestMethod]
		public void Patients_LoadAndSave_WithTheColumn() {
			Patient patient=NewPatient("kept.one@example.test");
			Assert.AreEqual("kept.one@example.test",patient.Email);
			Assert.AreEqual("kept.one@example.test",DataCore.GetScalar("SELECT _portal_active_email FROM patient WHERE PatNum="+POut.Long(patient.PatNum)));
			//Update through the business layer, and the CRUD's own selects.
			Patient patientOld=patient.Copy();
			patient.FName="Renamed";
			patient.Email="kept.two@example.test";
			Patients.Update(patient,patientOld);
			Patient patientRead=Patients.GetPat(patient.PatNum);
			Assert.AreEqual("Renamed",patientRead.FName);
			Assert.AreEqual("kept.two@example.test",DataCore.GetScalar("SELECT _portal_active_email FROM patient WHERE PatNum="+POut.Long(patient.PatNum)));
			List<Patient> listPatients=OpenPmsDentalBusiness.Crud.PatientCrud.SelectMany("SELECT * FROM patient WHERE PatNum="+POut.Long(patient.PatNum));
			Assert.AreEqual(1,listPatients.Count);
			Assert.AreEqual("kept.two@example.test",listPatients[0].Email);
			Assert.IsNotNull(Patients.GetFamily(patient.PatNum));
			//Inactive: the column is NULL and the address is free again.
			patientOld=patientRead.Copy();
			patientRead.PatStatus=PatientStatus.Inactive;
			Patients.Update(patientRead,patientOld);
			Assert.AreEqual("",DataCore.GetScalar("SELECT COALESCE(_portal_active_email,'') FROM patient WHERE PatNum="+POut.Long(patient.PatNum)));
			Patient patientSameAddress=NewPatient("kept.two@example.test");
			Assert.IsTrue(patientSameAddress.PatNum>0);
			//Blank addresses never collide.
			NewPatient("");
			NewPatient("");
		}

		[TestMethod]
		public void Patients_DuplicateActiveEmail_IsMariaDbsDuplicateKeyError() {
			Patient patient=NewPatient("kept.dup@example.test");
			//Insert: a second active patient with the address.
			Patient patientSecond=new Patient { LName="Portalkeptdup",FName="Synthetic",Email="kept.dup@example.test",PatStatus=PatientStatus.Patient };
			MySqlException mySqlException=SaveError(() => Patients.Insert(patientSecond,false));
			Assert.IsNotNull(mySqlException,"A second active patient with the address must not save.");
			Assert.AreEqual(MySqlErrorCode.DuplicateKeyEntry,mySqlException.ErrorCode,mySqlException.Message);
			StringAssert.Contains(mySqlException.Message,SchemaVerifier.PortalActiveEmailIndex);
			//Update: an inactive patient with the address saves, and making it active does not.
			Patient patientInactive=NewPatient("kept.dup@example.test",PatientStatus.Inactive);
			Patient patientOld=patientInactive.Copy();
			patientInactive.PatStatus=PatientStatus.Patient;
			mySqlException=SaveError(() => Patients.Update(patientInactive,patientOld));
			Assert.IsNotNull(mySqlException);
			Assert.AreEqual(MySqlErrorCode.DuplicateKeyEntry,mySqlException.ErrorCode,mySqlException.Message);
			Assert.AreEqual((int)PatientStatus.Inactive,PIn.Int(DataCore.GetScalar("SELECT PatStatus FROM patient WHERE PatNum="+POut.Long(patientInactive.PatNum))));
			Assert.AreEqual(1,PIn.Int(DataCore.GetScalar("SELECT COUNT(*) FROM patient WHERE _portal_active_email='kept.dup@example.test'")));
		}

		[TestMethod]
		public void Patients_Merge_WithTheColumn() {
			Patient patientTo=NewPatient("kept.merge@example.test");
			Patient patientFrom=NewPatient("kept.merge@example.test",PatientStatus.Inactive);
			Assert.IsTrue(Patients.MergeTwoPatients(patientTo.PatNum,patientFrom.PatNum));
			Assert.AreEqual("kept.merge@example.test",Patients.GetPat(patientTo.PatNum).Email);
			Assert.AreEqual(1,PIn.Int(DataCore.GetScalar("SELECT COUNT(*) FROM patient WHERE _portal_active_email='kept.merge@example.test'")));
		}

		[TestMethod]
		public void Backup_CopiesPatientsWithTheColumn() {
			NewPatient("kept.backup@example.test");
			Assert.AreEqual("*",MiscData.GetBackupColumnList(new DataConnection(),DB_NAME,"definition"),"A table without a generated column is copied as before.");
			string columnsPatient=MiscData.GetBackupColumnList(new DataConnection(),DB_NAME,"patient");
			Assert.IsFalse(columnsPatient.Contains(SchemaVerifier.PortalActiveEmailColumn),columnsPatient);
			StringAssert.StartsWith(columnsPatient,"`PatNum`,");
			string databaseBackup=MiscData.MakeABackup(doVerify:true,isAutoBackup:false);
			try {
				Assert.AreEqual(DataCore.GetScalar("SELECT COUNT(*) FROM patient"),DataCore.GetScalar("SELECT COUNT(*) FROM `"+databaseBackup+"`.patient"));
				Assert.AreEqual(DataCore.GetScalar("SELECT COUNT(_portal_active_email) FROM patient"),
					DataCore.GetScalar("SELECT COUNT(_portal_active_email) FROM `"+databaseBackup+"`.patient"),"The backup computes the column again.");
				Assert.AreEqual("1",DataCore.GetScalar("SELECT COUNT(*) FROM information_schema.STATISTICS WHERE TABLE_SCHEMA='"+databaseBackup
					+"' AND TABLE_NAME='patient' AND INDEX_NAME='"+SchemaVerifier.PortalActiveEmailIndex+"' AND SEQ_IN_INDEX=1"));
			}
			finally {
				DataCore.NonQ("DROP DATABASE IF EXISTS `"+databaseBackup+"`");
			}
		}
	}
}
