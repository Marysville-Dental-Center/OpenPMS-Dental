// S3a-3 (net10 plan): the pay plan's signature key data (PayPlans.GetKeyDataStringForSignature, hashed by GetKeyDataForSignature) keeps the
// text every stored signature was made with: the APR as double.ToString() gave it on .NET Framework (15 significant digits, current culture),
// the payment amount with "f" in the current culture and no sign when it rounds to zero. A pin that holds on both runtimes, and before/after
// equality with the replaced expressions on fixture plans (the .NET Framework record; inconclusive on .NET). No database.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalBusiness;

namespace PayPlanTests {
	[TestClass]
	public class SignatureKeyTests {
		private static readonly double _negativeZero=BitConverter.Int64BitsToDouble(long.MinValue);
		private CultureInfo _cultureInfoOld;

		[TestInitialize]
		public void TestInitialize() {
			_cultureInfoOld=Thread.CurrentThread.CurrentCulture;
			Thread.CurrentThread.CurrentCulture=CultureInfo.GetCultureInfo("en-US");
		}

		[TestCleanup]
		public void TestCleanup() {
			Thread.CurrentThread.CurrentCulture=_cultureInfoOld;
		}

		private static Patient NewPatient(string fName) {
			return new Patient { LName="Synthetic",FName=fName,Preferred="" };
		}

		private static PayPlan NewPayPlan(double apr,double payAmt) {
			return new PayPlan { APR=apr,NumberOfPayments=12,PayAmt=payAmt,ChargeFrequency=PayPlanFrequency.Monthly };
		}

		[TestMethod]
		public void Pin_KeyData() {
			string keyData=PayPlans.GetKeyDataStringForSignature(NewPayPlan(0.1+0.2,-0.004),NewPatient("Pat"),NewPatient("Guar"));
			Assert.AreEqual("0.3"+"12"+"0.00"+PayPlanFrequency.Monthly.GetDescription()+"Pat Synthetic"+"Guar Synthetic",keyData,
				".NET writes 0.30000000000000004 for the APR and -0.00 for the amount without S3a-3.");
			Assert.AreEqual("12.5120.00",PayPlans.GetKeyDataStringForSignature(NewPayPlan(12.5,_negativeZero),NewPatient("A"),NewPatient("B"))
				.Substring(0,10));
			Thread.CurrentThread.CurrentCulture=CultureInfo.GetCultureInfo("fr-CA");
			StringAssert.StartsWith(PayPlans.GetKeyDataStringForSignature(NewPayPlan(12.5,100),NewPatient("A"),NewPatient("B")),"12,512100,00",
				"The current culture, as the stored signatures were made.");
		}

		[TestMethod]
		public void BeforeAfter_KeyData_EqualsTheReplacedCode() {
			if(typeof(object).Assembly.GetName().Name!="mscorlib") {
				Assert.Inconclusive("Before/after equality is the .NET Framework record; on .NET the pin of this class holds instead.");
			}
			Random random=new Random(910);
			List<double> listValues=new List<double> { 0,_negativeZero,-0.001,-0.005,0.1+0.2,1.0/3,18.99,24.9,1e15,-1e-20 };
			for(int i=0;i<2000;i++) {
				double value=random.NextDouble()*Math.Pow(10,random.Next(-3,6));
				if(i%4==0) {
					value=Math.Round(value,random.Next(0,4));
				}
				listValues.Add(i%3==0 ? -value : value);
			}
			Patient patient=NewPatient("Pat");
			Patient patientGuar=NewPatient("Guar");
			foreach(string cultureName in new[] { "en-US","en-CA","fr-CA","de-DE" }) {
				Thread.CurrentThread.CurrentCulture=CultureInfo.GetCultureInfo(cultureName);
				for(int i=0;i<listValues.Count;i++) {
					PayPlan payPlan=NewPayPlan(listValues[i],listValues[listValues.Count-1-i]);
					string keyDataOld=PayPlans.GetKeyDataStringForSignature(payPlan.APR.ToString(),payPlan.NumberOfPayments.ToString(),
						payPlan.PayAmt.ToString("f"),payPlan.ChargeFrequency.GetDescription(),patient.GetNameFirstOrPrefL(),patientGuar.GetNameFirstOrPrefL());
					Assert.AreEqual(keyDataOld,PayPlans.GetKeyDataStringForSignature(payPlan,patient,patientGuar),cultureName+" "+i);
				}
			}
		}
	}
}
