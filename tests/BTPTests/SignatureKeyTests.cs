// S3a-3 (net10 plan): the treatment plan's signature key data (TreatPlans.GetKeyDataForSignatureHash) keeps the text every stored signature was
// made with: the patient amounts in the current culture with "F2", and no sign on an amount that rounds to zero (.NET would write "-0.00" and
// the stored signature would no longer match). A pin that holds on both runtimes, and before/after equality with the replaced expression on
// fixture plans (the .NET Framework record; inconclusive on .NET). No database.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalBusiness;

namespace BTPTests {
	[TestClass]
	public class SignatureKeyTests {
		private static readonly double _negativeZero=BitConverter.Int64BitsToDouble(long.MinValue);
		private CultureInfo _cultureInfoOld;

		[TestInitialize]
		public void TestInitialize() {
			_cultureInfoOld=Thread.CurrentThread.CurrentCulture;
			Thread.CurrentThread.CurrentCulture=new CultureInfo("en-US");
		}

		[TestCleanup]
		public void TestCleanup() {
			Thread.CurrentThread.CurrentCulture=_cultureInfoOld;
		}

		private static TreatPlan NewPlan() {
			return new TreatPlan { Note="Synthetic plan note",DateTP=new DateTime(2026,11,15),SignatureText="Synthetic Patient",
				SignaturePracticeText="Synthetic Dentist" };
		}

		private static List<ProcTP> NewProcs(params double[] arrayPatAmts) {
			return arrayPatAmts.Select((x,i) => new ProcTP { Descript="Synthetic procedure "+i,PatAmt=x,ItemOrder=i }).ToList();
		}

		[TestMethod]
		public void Pin_KeyData_AmountThatRoundsToZeroHasNoSign() {
			string keyData=TreatPlans.GetKeyDataForSignatureHash(NewPlan(),NewProcs(125.5,-0.004,_negativeZero,0.1+0.2));
			Assert.AreEqual("Synthetic plan note20261115Synthetic PatientSynthetic Dentist"
				+"Synthetic procedure 0125.50Synthetic procedure 10.00Synthetic procedure 20.00Synthetic procedure 30.30",keyData);
			Thread.CurrentThread.CurrentCulture=new CultureInfo("fr-CA");
			StringAssert.Contains(TreatPlans.GetKeyDataForSignatureHash(NewPlan(),NewProcs(125.5)),"125,50",
				"The current culture, as the stored signatures were made.");
		}

		[TestMethod]
		public void BeforeAfter_KeyData_EqualsTheReplacedCode() {
			if(typeof(object).Assembly.GetName().Name!="mscorlib") {
				Assert.Inconclusive("Before/after equality is the .NET Framework record; on .NET the pin of this class holds instead.");
			}
			Random random=new Random(2026);
			List<double> listAmounts=new List<double> { 0,_negativeZero,-0.001,-0.005,-0.0049,12.5,99.995,1e15,-1e-20,0.1+0.2 };
			for(int i=0;i<1500;i++) {
				double amount=random.NextDouble()*Math.Pow(10,random.Next(-3,6));
				listAmounts.Add(i%3==0 ? -amount : amount);
			}
			foreach(string cultureName in new[] { "en-US","en-CA","fr-CA","de-DE" }) {
				Thread.CurrentThread.CurrentCulture=new CultureInfo(cultureName);
				for(int i=0;i<listAmounts.Count;i+=5) {
					TreatPlan treatPlan=NewPlan();
					List<ProcTP> listProcTPs=NewProcs(listAmounts.Skip(i).Take(5).ToArray());
					Assert.AreEqual(OldKeyData(treatPlan,listProcTPs),TreatPlans.GetKeyDataForSignatureHash(treatPlan,listProcTPs),cultureName+" "+i);
				}
			}
		}

		///<summary>TreatPlans.GetKeyDataForSignatureHash before S3a-3.</summary>
		private static string OldKeyData(TreatPlan treatPlan,List<ProcTP> listProcTPs) {
			StringBuilder stringBuilder=new StringBuilder();
			stringBuilder.Append(treatPlan.Note);
			stringBuilder.Append(treatPlan.DateTP.ToString("yyyyMMdd"));
			stringBuilder.Append(treatPlan.SignatureText);
			stringBuilder.Append(treatPlan.SignaturePracticeText);
			for(int i=0;i<listProcTPs.Count;i++) {
				stringBuilder.Append(listProcTPs[i].Descript);
				stringBuilder.Append(listProcTPs[i].PatAmt.ToString("F2"));
			}
			return stringBuilder.ToString();
		}
	}
}
