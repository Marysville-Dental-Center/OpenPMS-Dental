// What a form saves from a signature box, for the forms that read every box on the save (S3a-14g of the .NET 10 move,
// docs/port-notes/S3a-14.md, section S3a-14g): a box whose signature did not change keeps the stored signature, so a Topaz signature is
// never read back from a 32-bit helper that ended since the form opened.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using OpenPmsDentalBusiness;

namespace OpenPmsDental.UI {
	///<summary>The signature steps of the payment plan's sheet (FormSheetFillEdit.SaveSignaturePayPlan) and of an eForm
	///(CtrlEFormFill.FillFieldsFromControls), here so that a test can run them on a real box. Both throw what SignatureBoxWrapper.GetSignature
	///throws for a changed Topaz box whose helper failed (a Helper32Exception, "Topaz signature pad software could not be reached:"); the caller
	///shows it and writes nothing.</summary>
	public static class SignatureBoxSaves {
		///<summary>For one signature box of the payment plan's sheet: false, with payPlan unchanged, when the box's signature did not change since
		///the sheet opened (GetSigChanged), so the plan keeps its stored signature (S3a-14g). Else sets payPlan.Signature from the box with keyData
		///and, for a signature that is not blank, SigIsTopaz, as FormSheetFillEdit did, and returns true: the caller saves the plan.</summary>
		public static bool SetPayPlanSignature(PayPlan payPlan,SignatureBoxWrapper signatureBoxWrapper,string keyData) {
			if(!signatureBoxWrapper.GetSigChanged()) {
				return false;
			}
			payPlan.Signature=signatureBoxWrapper.GetSignature(keyData);
			if(payPlan.Signature!="") {
				payPlan.SigIsTopaz=false;
				if(signatureBoxWrapper.GetSigIsTopaz()) {
					payPlan.SigIsTopaz=true;
				}
			}
			return true;
		}

		///<summary>The ValueString of an eForm's signature field: valueStringStored as it is when the box's signature did not change since the form
		///was laid out (GetSigChanged), so the stored signature is kept (S3a-14g); else "1" for Topaz or "0", then the box's signature with keyData,
		///as CtrlEFormFill did.</summary>
		public static string GetEFormSignatureValue(string valueStringStored,SignatureBoxWrapper signatureBoxWrapper,string keyData) {
			if(!signatureBoxWrapper.GetSigChanged()) {
				return valueStringStored;
			}
			string signature="0";
			if(signatureBoxWrapper.GetSigIsTopaz()) {
				signature="1";
			}
			signature+=signatureBoxWrapper.GetSignature(keyData);
			return signature;
		}
	}
}
