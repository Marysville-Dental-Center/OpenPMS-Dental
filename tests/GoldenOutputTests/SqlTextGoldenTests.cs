// Golden outputs: the SQL text the program writes for values computed in doubles and floats (SOut.Double, SOut.Float through POut and the Crud
// classes, and Prefs.UpdateDouble without rounding), captured from the commands as they go to the database (QueryMonitor, the Query Monitor
// window's hook) and recorded with the .NET Framework build (docs/port-notes/S3a-4.md).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeBase;
using DataConnectionBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalBusiness;

namespace GoldenOutputTests {
	[TestClass]
	public class SqlTextGoldenTests {
		[TestInitialize]
		public void TestInitialize() {
			GoldenDb.Begin();
		}

		///<summary>The commands the action sends to the database, in order, as the Query Monitor shows them (parameters filled in). Only this thread's
		///commands: the monitor is the thread's own (QueryMonitor.Monitor is thread-static when set).</summary>
		public static List<string> CaptureCommands(Action action) {
			List<string> listCommands=new List<string>();
			HashSet<DbQueryObj> hashSetSeen=new HashSet<DbQueryObj>();
			QueryMonitor queryMonitor=new QueryMonitor();
			queryMonitor.IsMonitoring=true;
			queryMonitor.Fired+=(ODEventArgs e) => {
				//Fired when a command starts and again when it ends, with the same object.
				if(e.Tag is DbQueryObj dbQueryObj && !dbQueryObj.IsPayload && hashSetSeen.Add(dbQueryObj)) {
					listCommands.Add(dbQueryObj.Command);
				}
			};
			QueryMonitor.Monitor=queryMonitor;
			try {
				action();
			}
			finally {
				QueryMonitor.Monitor=null;
			}
			return listCommands;
		}

		[TestMethod]
		public void SqlText_ComputedDoublesAndFloats() {
			StringBuilder stringBuilder=new StringBuilder();
			//A procedure's fee and drug quantity computed in doubles and floats (ProcedureCrud: POut.Double rounds to two places, POut.Float writes
			//seven significant digits). One update per value, so each command has one column.
			//The fixture's rows are put back afterwards, so the other outputs never see these values, whatever order the tests run in.
			long procNum=GoldenData.ListProcNums1[1];
			Procedure procedureOriginal=Procedures.GetOneProc(procNum,false);
			string salesTaxOriginal=PrefC.GetString(PrefName.SalesTaxPercentage);
			try {
				foreach(double fee in new[] { (0.1+0.2)*3,(0.10+0.70)-0.80,1234.56/3,-0.005,1e15/3 }) {
					Procedure procedure=Procedures.GetOneProc(procNum,false);
					Procedure procedureOld=procedure.Copy();
					procedure.ProcFee=fee;
					stringBuilder.Append(string.Join("\n",CaptureCommands(() => OpenPmsDentalBusiness.Crud.ProcedureCrud.Update(procedure,procedureOld)))+"\n");
				}
				foreach(float drugQty in new[] { 1f/3f,16777217f,0.1f*3f,2.5e-8f }) {
					Procedure procedure=Procedures.GetOneProc(procNum,false);
					Procedure procedureOld=procedure.Copy();
					procedure.DrugQty=drugQty;
					stringBuilder.Append(string.Join("\n",CaptureCommands(() => OpenPmsDentalBusiness.Crud.ProcedureCrud.Update(procedure,procedureOld)))+"\n");
				}
				//A percentage preference saved without rounding (the sales tax percentage, as Setup > Account saves it): SOut.Double without rounding,
				//15 significant digits.
				foreach(double percent in new[] { 0.1+0.2,4.712*1.1,(0.10+0.70)-0.80,100.0/3 }) {
					stringBuilder.Append(string.Join("\n",CaptureCommands(() => Prefs.UpdateDouble(PrefName.SalesTaxPercentage,percent,false))
						.Where(x => x.StartsWith("UPDATE preference",StringComparison.Ordinal)))+"\n");
				}
			}
			finally {
				OpenPmsDentalBusiness.Crud.ProcedureCrud.Update(procedureOriginal,Procedures.GetOneProc(procNum,false));
				Prefs.UpdateString(PrefName.SalesTaxPercentage,salesTaxOriginal);
				Cache.Refresh(InvalidType.Prefs);
			}
			Assert.AreEqual(procedureOriginal.ProcFee,Procedures.GetOneProc(procNum,false).ProcFee,"The fixture's procedure is back.");
			Golden.AssertText("sql-text-computed-values.txt",stringBuilder.ToString());
		}

		[TestMethod]
		public void SqlText_SOutDirect() {
			//The SOut methods themselves, with the values above, in the forms the program calls: rounding (the default), 15 digits in the current
			//culture and with a period, a fixed number of places, and floats. Each line names its value by its bits (hex), which no runtime formats
			//differently.
			StringBuilder stringBuilder=new StringBuilder();
			double[] arrayDoubles=new[] { 0.1+0.2,(0.10+0.70)-0.80,-0.004,-0.005,1234.56/3,1e15/3,1e-5,123456789012345678.0,-0.0 };
			foreach(double value in arrayDoubles) {
				stringBuilder.Append("Double(0x"+BitConverter.DoubleToInt64Bits(value).ToString("x16")+"): "+SOut.Double(value)+" | "
					+SOut.Double(value,false)+" | "+SOut.Double(value,false,true)+" | "+SOut.Double(value,3)+" | "+SOut.Double(value,4)+"\n");
			}
			float[] arrayFloats=new[] { 1f/3f,16777217f,0.1f+0.2f,-(0.1f+0.7f-0.8f),2.5e-8f,-0f };
			foreach(float value in arrayFloats) {
				stringBuilder.Append("Float(0x"+BitConverter.ToInt32(BitConverter.GetBytes(value),0).ToString("x8")+"): "+SOut.Float(value)+"\n");
			}
			Golden.AssertText("sout-computed-values.txt",stringBuilder.ToString());
		}
	}
}
