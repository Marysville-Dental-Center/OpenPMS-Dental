// Golden outputs: the key data and the key hash of the treatment-plan and pay-plan signatures (TreatPlans.GetKeyDataForSignatureHash and
// GetKeyDataForSignatureSaving, PayPlans.GetKeyDataStringForSignature and GetKeyDataForSignature), for the fixture's rows and for amounts computed
// in doubles, in en-US (the keys use the current culture, as every stored signature was made). Recorded with the .NET Framework build
// (docs/port-notes/S3a-4.md). A key that differs makes every stored signature of its kind show as invalid.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalBusiness;

namespace GoldenOutputTests {
	[TestClass]
	public class SignatureKeyGoldenTests {
		[TestInitialize]
		public void TestInitialize() {
			GoldenDb.Begin();
		}

		///<summary>The treatment plan and its procedures as the fixture database holds them.</summary>
		public static TreatPlan GetTreatPlan(out List<ProcTP> listProcTPs) {
			TreatPlan treatPlan=TreatPlans.GetOne(GoldenData.TreatPlanNum);
			listProcTPs=ProcTPs.RefreshForTP(GoldenData.TreatPlanNum).OrderBy(x => x.ItemOrder).ToList();
			Assert.AreEqual(3,listProcTPs.Count);
			return treatPlan;
		}

		///<summary>The same procedures with patient portions computed in doubles as the treatment plan grid computes them (fee minus insurance):
		///0.30-0.15, (0.10+0.70)-0.80 (about -1.1e-16), and 1234.56/3.</summary>
		public static List<ProcTP> GetComputedProcTPs(List<ProcTP> listProcTPs) {
			List<ProcTP> listComputed=listProcTPs.Select(x => x.Copy()).ToList();
			listComputed[0].PatAmt=1234.56/3;
			listComputed[1].PatAmt=(0.1+0.2)-0.15;
			listComputed[2].PatAmt=(0.10+0.70)-0.80;
			return listComputed;
		}

		///<summary>The pay plan with amounts computed in doubles: an APR of 0.1+0.2 and a payment of 1375/12 (114.58333...).</summary>
		public static PayPlan GetComputedPayPlan(PayPlan payPlan) {
			PayPlan payPlanComputed=payPlan.Copy();
			payPlanComputed.APR=0.1+0.2;
			payPlanComputed.PayAmt=1375.0/12;
			return payPlanComputed;
		}

		///<summary>A key hash (a string of 16 ASCII characters made from the MD5 bytes, '?' for each byte over 127) as the hex of its UTF-16 code
		///units, so the fixture holds no control characters.</summary>
		public static string HashToHex(string hash) {
			return string.Concat(hash.Select(x => ((int)x).ToString("x4")));
		}

		[TestMethod]
		public void SignatureKeys_TreatPlanAndPayPlan() {
			StringBuilder stringBuilder=new StringBuilder();
			TreatPlan treatPlan=GetTreatPlan(out List<ProcTP> listProcTPs);
			string keyData=TreatPlans.GetKeyDataForSignatureHash(treatPlan,listProcTPs);
			stringBuilder.Append("treatplan.keydata="+keyData+"\n");
			stringBuilder.Append("treatplan.hash="+HashToHex(TreatPlans.GetKeyDataForSignatureSaving(treatPlan,listProcTPs))+"\n");
			List<ProcTP> listComputed=GetComputedProcTPs(listProcTPs);
			stringBuilder.Append("treatplan.computed.keydata="+TreatPlans.GetKeyDataForSignatureHash(treatPlan,listComputed)+"\n");
			stringBuilder.Append("treatplan.computed.hash="+HashToHex(TreatPlans.GetKeyDataForSignatureSaving(treatPlan,listComputed))+"\n");
			PayPlan payPlan=PayPlans.GetOne(GoldenData.PayPlanNum);
			Patient patient=Patients.GetLim(payPlan.PatNum);
			Patient patientGuarantor=Patients.GetLim(payPlan.Guarantor);
			stringBuilder.Append("payplan.keydata="+PayPlans.GetKeyDataStringForSignature(payPlan,patient,patientGuarantor)+"\n");
			stringBuilder.Append("payplan.hash="+HashToHex(PayPlans.GetKeyDataForSignature(payPlan))+"\n");
			PayPlan payPlanComputed=GetComputedPayPlan(payPlan);
			stringBuilder.Append("payplan.computed.keydata="+PayPlans.GetKeyDataStringForSignature(payPlanComputed,patient,patientGuarantor)+"\n");
			stringBuilder.Append("payplan.computed.hash="+HashToHex(PayPlans.GetKeyDataForSignature(payPlanComputed))+"\n");
			Golden.AssertText("signature-keys.txt",stringBuilder.ToString());
		}
	}
}
