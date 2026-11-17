// S3a-1c of the .NET 10 plan (docs/port-notes/S3a-1.md, section 9): Fees.SaveIncrease (Fee Tools, Increase by %) saves each fee and its
// security log entries as one unit. A cancel of the progress window stops the increase between two fees, never between a fee and its entries,
// and a fee whose save started is finished with its entries. The cancel is the window's token set on the test's thread (ODCancellation.Use),
// as FrmProgressAuto sets it on its worker thread; the database layer then refuses every command outside a unit. Against this run's scratch
// database (TestDb, the feetest_ one of FeeDbTests; no other database is used). Synthetic data only.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalBusiness;

namespace FeeTests {
	[TestClass]
	public class FeeIncreaseCancelDbTests {
		private static string _skipReason;
		private static readonly DateTime _date2027=new DateTime(2027,1,1);

		[ClassInitialize]
		public static void ClassInitialize(TestContext testContext) {
			_skipReason=TestDb.EnsureDatabase();
		}

		[TestInitialize]
		public void TestInitialize() {
			if(_skipReason!=null) {
				Assert.Inconclusive(_skipReason);
			}
			TestDb.EnsureDatabase();
		}

		///<summary>Three fees in effect today (amounts 100, 200, 300) of a new fee schedule.</summary>
		private static FeeSched CreateScheduleOfThree(out List<ProcedureCode> listProcedureCodes) {
			FeeSched feeSched=FeeDbTests.NewFeeSched();
			listProcedureCodes=new List<ProcedureCode>();
			for(int i=0;i<3;i++) {
				ProcedureCode procedureCode=FeeDbTests.NewCode();
				listProcedureCodes.Add(procedureCode);
				FeeDbTests.InsertFee(feeSched.FeeSchedNum,procedureCode.CodeNum,100*(i+1),DateTime.MinValue);
			}
			return feeSched;
		}

		private static int CountLogEntries(string logText) {
			return PIn.Int(DataCore.GetScalar("SELECT COUNT(*) FROM securitylog WHERE LogText='"+POut.String(logText)+"'"));
		}

		///<summary>The callback of SaveIncrease as the window passes it: the user cancels while the first fee's unit runs, then the unit writes its
		///log entry (a database write after the cancel, allowed inside the unit). The increase stops before the second fee.</summary>
		[TestMethod]
		public void SaveIncrease_InPlace_CancelDuringTheFirstFee_ItsLogEntryWritten_NoFurtherFee() {
			FeeSched feeSched=CreateScheduleOfThree(out List<ProcedureCode> listProcedureCodes);
			List<Fee> listFeesBase=Fees.GetListExactAsOf(feeSched.FeeSchedNum,0,0,DateTime.Today);
			List<Fee> listFeesLogged=new List<Fee>();
			string logPrefix="S3a-1c increase "+Guid.NewGuid().ToString("N").Substring(0,8)+" ";
			using CancellationTokenSource cancellationTokenSource=new CancellationTokenSource();
			using(ODCancellation.Use(cancellationTokenSource.Token)) {
				Assert.ThrowsExactly<OperationCanceledException>(() => Fees.SaveIncrease(listFeesBase,10,2,DateTime.MinValue,fee => {
					cancellationTokenSource.Cancel();
					SecurityLogs.MakeLogEntry(EnumPermType.ProcFeeEdit,0,logPrefix+fee.FeeNum,fee.CodeNum,DateTime.MinValue);
					listFeesLogged.Add(fee);
				}));
				Assert.ThrowsExactly<OperationCanceledException>(() => DataCore.GetScalar("SELECT 1"),"Outside the unit the cancel holds.");
			}
			Assert.AreEqual(1,listFeesLogged.Count,"One unit ran: the fee under way at the cancel.");
			Assert.AreEqual(1,CountLogEntries(logPrefix+listFeesLogged[0].FeeNum),"Its log entry was written after the cancel, inside the unit.");
			List<Fee> listFeesNow=Fees.GetListExactAllDates(feeSched.FeeSchedNum,0,0);
			Assert.AreEqual(3,listFeesNow.Count,"Saved in place: no new row.");
			for(int i=0;i<listProcedureCodes.Count;i++) {
				Fee fee=listFeesNow.Single(x => x.CodeNum==listProcedureCodes[i].CodeNum);
				double amountBase=100*(i+1);
				if(fee.FeeNum==listFeesLogged[0].FeeNum) {
					Assert.AreEqual(amountBase*1.1,fee.Amount,0.001,"The fee of the unit is increased.");
				}
				else {
					Assert.AreEqual(amountBase,fee.Amount,0.001,"No fee after the cancel is increased.");
				}
			}
		}

		///<summary>The same with a new effective date: the rows dated 2027 are inserted one unit at a time; after the cancel no further row is
		///inserted, and the one inserted has its log entry.</summary>
		[TestMethod]
		public void SaveIncrease_NewDate_CancelDuringTheFirstFee_OneRowInserted_WithItsLogEntry() {
			FeeSched feeSched=CreateScheduleOfThree(out List<ProcedureCode> listProcedureCodes);
			List<Fee> listFeesBase=Fees.GetListExactAsOf(feeSched.FeeSchedNum,0,0,DateTime.Today);
			List<Fee> listFeesLogged=new List<Fee>();
			string logPrefix="S3a-1c increase dated "+Guid.NewGuid().ToString("N").Substring(0,8)+" ";
			using CancellationTokenSource cancellationTokenSource=new CancellationTokenSource();
			using(ODCancellation.Use(cancellationTokenSource.Token)) {
				Assert.ThrowsExactly<OperationCanceledException>(() => Fees.SaveIncrease(listFeesBase,10,2,_date2027,fee => {
					cancellationTokenSource.Cancel();
					SecurityLogs.MakeLogEntry(EnumPermType.ProcFeeEdit,0,logPrefix+fee.FeeNum,fee.CodeNum,DateTime.MinValue);
					listFeesLogged.Add(fee);
				}));
			}
			Assert.AreEqual(1,listFeesLogged.Count);
			Assert.AreEqual(_date2027,listFeesLogged[0].DateEffective);
			Assert.AreEqual(1,CountLogEntries(logPrefix+listFeesLogged[0].FeeNum));
			List<Fee> listFeesDated=Fees.GetListExactAllDates(feeSched.FeeSchedNum,0,0).FindAll(x => x.DateEffective==_date2027);
			Assert.AreEqual(1,listFeesDated.Count,"Only the row of the unit was inserted.");
			Assert.AreEqual(listFeesLogged[0].FeeNum,listFeesDated[0].FeeNum);
		}

		///<summary>Without a cancel the callback is called once for every saved row, with the row as saved (its new amount), and the return value
		///is unchanged.</summary>
		[TestMethod]
		public void SaveIncrease_NoCancel_CallbackOncePerSavedRow() {
			FeeSched feeSched=CreateScheduleOfThree(out List<ProcedureCode> listProcedureCodes);
			List<Fee> listFeesLogged=new List<Fee>();
			List<Fee> listFeesSaved=Fees.SaveIncrease(Fees.GetListExactAsOf(feeSched.FeeSchedNum,0,0,DateTime.Today),10,2,DateTime.MinValue,
				fee => listFeesLogged.Add(fee));
			Assert.AreEqual(3,listFeesSaved.Count);
			CollectionAssert.AreEqual(listFeesSaved.Select(x => x.FeeNum).ToList(),listFeesLogged.Select(x => x.FeeNum).ToList());
			CollectionAssert.AreEquivalent(new[] { 110d,220d,330d },listFeesLogged.Select(x => Math.Round(x.Amount,2)).ToArray());
		}
	}
}
