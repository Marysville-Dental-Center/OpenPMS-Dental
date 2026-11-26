using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using CodeBase;
using OpenPmsDentalBusiness;

namespace OpenPmsDental {
	public partial class FormMountItemDefEdit:FormODBase {
		public MountItemDef MountItemDefCur;
		///<summary>FB-1: the type of the whole mount, chosen on the mount (FormMountDefEdit). An X-ray mount's slots are radiographs, so
		///there is no kind to pick; a photo mount's slot is an intraoral or an extraoral photo. A mount can no longer mix the two.</summary>
		public bool IsPhotoMount;

		public FormMountItemDefEdit() {
			InitializeComponent();
			InitializeLayoutManager();
			Lan.F(this);
		}

		private void FormMountItemDefEdit_Load(object sender, EventArgs e){
			textXpos.Text=MountItemDefCur.Xpos.ToString();
			textYpos.Text=MountItemDefCur.Ypos.ToString();
			textWidth.Text=MountItemDefCur.Width.ToString();
			textHeight.Text=MountItemDefCur.Height.ToString();
			textRotate.Value=MountItemDefCur.RotateOnAcquire;
			textToothNumbers.Text=Tooth.DisplayRange(MountItemDefCur.ToothNumbers);
			textTextShowing.Text=MountItemDefCur.TextShowing;
			textFontSize.Value=MountItemDefCur.FontSize;
			FillGuide();
		}

		///<summary>IMG-P4a: the slot's guide for the camera session (mountitemdefguide). A slot without a row is a radiograph slot with no guide.
		///FB-1: the kind list depends on the mount type (IsPhotoMount).</summary>
		private void FillGuide() {
			//FB-1: only the kinds of the mount's type. Photo mount: index 0 Intraoral, 1 Extraoral (the value minus 1).
			comboSlotKind.Items.Clear();
			comboSlotKind.Items.Add(Lan.g(this,"Intraoral photo"));
			comboSlotKind.Items.Add(Lan.g(this,"Extraoral photo"));
			MountItemDefGuide mountItemDefGuide=null;
			if(!MountItemDefCur.IsNew) {
				mountItemDefGuide=MountItemDefGuides.GetForMountItemDef(MountItemDefCur.MountItemDefNum);
			}
			textGuideLabel.Text=mountItemDefGuide?.GuideLabel??"";
			checkMirror.Checked=IsPhotoMount && (mountItemDefGuide?.IsMirror??false);
			comboSlotKind.SelectedIndex=mountItemDefGuide?.SlotKind==MountSlotKind.Extraoral ? 1 : 0;
			comboSlotKind.Visible=IsPhotoMount;
			labelSlotKind.Visible=IsPhotoMount;
			checkMirror.Visible=IsPhotoMount;
		}

		///<summary>IMG-P4a: saves the slot's guide after the slot is saved: a radiograph slot with no guide text and no mirror keeps no row.
		///Changes reach mounts made from now on; a patient's existing mount keeps the guide it was made with.</summary>
		private void SaveGuide() {
			MountSlotKind mountSlotKind=MountSlotKind.Radiograph;
			if(IsPhotoMount) {
				mountSlotKind=comboSlotKind.SelectedIndex==1 ? MountSlotKind.Extraoral : MountSlotKind.Intraoral;
			}
			bool isMirror=IsPhotoMount && checkMirror.Checked;
			string guideLabel=textGuideLabel.Text.Trim();
			if(mountSlotKind==MountSlotKind.Radiograph && guideLabel=="" && !isMirror) {
				MountItemDefGuides.DeleteForMountItemDef(MountItemDefCur.MountItemDefNum);
				return;
			}
			MountItemDefGuides.Upsert(new MountItemDefGuide { MountItemDefNum=MountItemDefCur.MountItemDefNum,GuideLabel=guideLabel,
				IsMirror=isMirror,SlotKind=mountSlotKind });
		}

		private void butAddField_Click(object sender,EventArgs e) {
			List<MessageReplaceType> listMessageReplaceTypes=new List<MessageReplaceType>();
			listMessageReplaceTypes.Add(MessageReplaceType.Office);
			listMessageReplaceTypes.Add(MessageReplaceType.Patient);
			listMessageReplaceTypes.Add(MessageReplaceType.Mount);
			FrmMessageReplacements frmMessageReplacements=new FrmMessageReplacements(listMessageReplaceTypes);
			frmMessageReplacements.MessageReplacementSystemType=MessageReplacementSystemType.Mount;
			frmMessageReplacements.IsSelectionMode=true;
			frmMessageReplacements.ShowDialog();
			if(frmMessageReplacements.IsDialogOK) {
				textTextShowing.SelectedText=frmMessageReplacements.ReplacementTextSelected;
			}
		}

		private void butDelete_Click(object sender, EventArgs e){
			if(MountItemDefCur.IsNew){//although not currenly used
				DialogResult=DialogResult.Cancel;
				return;
			}
			if(!MsgBox.Show(this,MsgBoxButtons.OKCancel,"Delete?")){
				return;
			}
			MountItemDefs.Delete(MountItemDefCur.MountItemDefNum);
			DialogResult=DialogResult.OK;
		}

		private void butSave_Click(object sender,EventArgs e) {
			if(!textXpos.IsValid()
				|| !textYpos.IsValid()
				|| !textWidth.IsValid()
				|| !textHeight.IsValid()
				|| !textRotate.IsValid()
				|| !textFontSize.IsValid())
			{
				MsgBox.Show(this,"Please fix data entry errors first.");
				return;
			}
			if(!textRotate.Value.In(0,90,180,270)){
				MsgBox.Show(this,"Rotate value invalid.");
				return;
			}
			if(!textTextShowing.Text.IsNullOrEmpty() && textFontSize.Value==0){
				MsgBox.Show(this,"Please enter a font size.");
				return;
			}
			try{
				MountItemDefCur.ToothNumbers=Tooth.ParseRange(textToothNumbers.Text);
			}
			catch(Exception ex){
				MessageBox.Show(ex.Message);
				return;
			}
			MountItemDefCur.Xpos=PIn.Int(textXpos.Text);
			MountItemDefCur.Ypos=PIn.Int(textYpos.Text);
			MountItemDefCur.Width=PIn.Int(textWidth.Text);
			MountItemDefCur.Height=PIn.Int(textHeight.Text);
			MountItemDefCur.RotateOnAcquire=textRotate.Value;
			MountItemDefCur.TextShowing=textTextShowing.Text;
			MountItemDefCur.FontSize=(float)textFontSize.Value;
			if(MountItemDefCur.IsNew){//but not currenly used
				MountItemDefs.Insert(MountItemDefCur);
			}
			else{
				MountItemDefs.Update(MountItemDefCur);
			}
			SaveGuide();
			DialogResult=DialogResult.OK;
		}

	}
}