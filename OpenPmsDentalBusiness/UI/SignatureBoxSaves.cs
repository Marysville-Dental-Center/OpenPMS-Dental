// What a form saves from a signature box, for the forms that read every box on the save (S3a-14g of the .NET 10 move,
// docs/port-notes/S3a-14.md, section S3a-14g): a Topaz box on the 32-bit helper's path whose signature did not change keeps the stored
// signature, so a Topaz signature is never read back from a helper that ended since the form opened. Every other box (the program's own, and
// Topaz's own control in the program's process, native first since S3a-14n) is read as before S3a-14g.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using OpenPmsDentalBusiness;

namespace OpenPmsDental.UI {
	///<summary>The signature steps of the payment plan's sheet (FormSheetFillEdit.SaveSignaturePayPlan) and of an eForm
	///(CtrlEFormFill.FillFieldsFromControls), here so that a test can run them on a real box. Both throw what SignatureBoxWrapper.GetSignature
	///throws for a changed Topaz box whose helper failed (a Helper32Exception, "Topaz signature pad software could not be reached:"); the caller
	///shows it and writes nothing.</summary>
	public static class SignatureBoxSaves {
		///<summary>True when the form keeps the box's stored signature instead of reading the box again: a Topaz box on the helper's path
		///(SignatureBoxWrapper.IsTopazInHelper) whose signature did not change since the form opened (GetSigChanged), S3a-14g. Every other box is
		///read, as before.</summary>
		public static bool IsStoredSignatureKept(SignatureBoxWrapper signatureBoxWrapper) {
			return !signatureBoxWrapper.GetSigChanged() && signatureBoxWrapper.IsTopazInHelper();
		}

		///<summary>For one signature box of the payment plan's sheet: false, with payPlan unchanged, when the plan keeps its stored signature
		///(IsStoredSignatureKept, S3a-14g). Else sets payPlan.Signature from the box with keyData and, for a signature that is not blank,
		///SigIsTopaz, as FormSheetFillEdit did, and returns true: the caller saves the plan.</summary>
		public static bool SetPayPlanSignature(PayPlan payPlan,SignatureBoxWrapper signatureBoxWrapper,string keyData) {
			if(IsStoredSignatureKept(signatureBoxWrapper)) {
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

		///<summary>The ValueString of an eForm's signature field: valueStringStored as it is when the stored signature is kept
		///(IsStoredSignatureKept, S3a-14g); else "1" for Topaz or "0", then the box's signature with keyData, as CtrlEFormFill did.</summary>
		public static string GetEFormSignatureValue(string valueStringStored,SignatureBoxWrapper signatureBoxWrapper,string keyData) {
			if(IsStoredSignatureKept(signatureBoxWrapper)) {
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
