using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CodeBase;
using OpenPmsDentalBusiness;
using WpfControls.UI;

namespace OpenPmsDental {
///<summary></summary>
	public partial class FrmDocSign:FrmODBase {
		//<summary></summary>
		//public bool IsNew;
		//private Patient PatCur;
		private Document _document;
		///<summary>The note and signature as they were when the window opened, for the 24.4 signature security log.</summary>
		private Document _documentOld;
		///<summary>This keeps the noteChanged event from erasing the signature when first loading.</summary>
		private bool _isStartingUp;
		private bool _isSignatureChanged;
		private Patient _patient;
		private string _patFolderName;
		///<summary>In screeen coords. LL because it's 'anchored' LL and it will grow up and right.</summary>
		public System.Drawing.Point PointLLStart;
		
		///<summary></summary>
		public FrmDocSign(Document document,Patient patient) {
			InitializeComponent();
			_document=document;
			_documentOld=document.Copy();
			_patient=patient;
			Load+=FrmDocSign_Load;
			textNote.TextChanged+=textNote_TextChanged;
			signatureBoxWrapper.SignatureChanged+=signatureBoxWrapper_SignatureChanged;
			PreviewKeyDown+=FrmDocSign_PreviewKeyDown;
		}

		///<summary></summary>
		public void FrmDocSign_Load(object sender, EventArgs e){
			_formFrame.Location=new System.Drawing.Point(PointLLStart.X,PointLLStart.Y-_formFrame.Height+4);
			Lang.F(this);
			//24.4: ImageSignatureCreate for an unsigned image, SignedImageEdit for a signed one. The callers check first; this is the backstop.
			if(!ImageSignaturePermissions.IsAuthorizedToSign(_document)) {
				IsDialogCancel=true;
				Close();
				return;
			}
			_patFolderName=ImageStore.GetPatientFolder(_patient,ImageStore.GetPreferredAtoZpath());
			_isStartingUp=true;
			textNote.Text=_document.Note;
			signatureBoxWrapper.SignatureMode=UI.SignatureBoxWrapper.SigMode.Document;
			string keyData=ImageStore.GetHashString(_document,_patFolderName);
			signatureBoxWrapper.FillSignature(_document.SigIsTopaz,keyData,_document.Signature);
			//If sig is not showing, then try using hash without raw file data included.
			if(signatureBoxWrapper.GetNumberOfTabletPoints(_document.SigIsTopaz)==0) {
				keyData=ImageStore.GetHashString(_document,_patFolderName,includeFileInHash:false);
				signatureBoxWrapper.FillSignature(_document.SigIsTopaz,keyData,_document.Signature);
			}
			_isStartingUp=false;
		}

		private void textNote_TextChanged(object sender,EventArgs e) {
			if(!_isStartingUp//so this happens only if user changes the note
				&& !_isSignatureChanged)//and the original signature is still showing.
			{
				signatureBoxWrapper.ClearSignature();
				//this will call OnSignatureChanged to set UserNum, textUser, and SigChanged
			}
		}

		private void SaveSignature() {
			if(_isSignatureChanged) {
				string keyData=ImageStore.GetHashString(_document,_patFolderName);
				_document.Signature=signatureBoxWrapper.GetSignature(keyData);
				_document.SigIsTopaz=signatureBoxWrapper.GetSigIsTopaz();
			}
		}

		private void signatureBoxWrapper_SignatureChanged(object sender,EventArgs e) {
			_isSignatureChanged=true;
		}

		private void FrmDocSign_PreviewKeyDown(object sender,KeyEventArgs e) {
			if(butSave.IsAltKey(Key.S,e)) {
				butSave_Click(this,new EventArgs());
			}
		}

		private void butSave_Click(object sender, System.EventArgs e){
			//S3a-14g: a Topaz signature the 32-bit helper lost stops the save here, with the message, before the document is written; the window
			//stays open, so the user can clear the box and sign again. Before, GetSignature's failure ended the program.
			string noteOld=_document.Note;
			_document.Note=textNote.Text;//the signature's key data includes the note
			try {
				if(_isSignatureChanged && signatureBoxWrapper.GetSigIsTopaz()) {
					signatureBoxWrapper.CheckTopazSignatureReachable();
				}
				SaveSignature();
			}
			catch(Helper32Exception ex) {
				_document.Note=noteOld;
				MsgBox.Show(ex.Message);
				return;
			}
			Documents.Update(_document);
			ImageSignaturePermissions.LogSignatureChange(_documentOld,_document);
			IsDialogOK=true;
		}

	}
}