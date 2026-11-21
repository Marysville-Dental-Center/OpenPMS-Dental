using System;
using System.Linq;
using System.Windows.Forms;
using OpenPmsDentalBusiness;
using OpenPmsDental.UI;


namespace OpenPmsDental.User_Controls.SetupWizard {
	public partial class UserControlSetupWizFeatures:SetupWizControl {
		///<summary>The pictures of the information labels' list by index (UI-9b-3, docs/port-notes/UI-9b.md, section 4): 0 an information label, 1 the selected one as icons, 16 px
		///as the old list; a label draws its picture at its own size.</summary>
		public static readonly EnumIcons[] ArrayIconsImageList=new EnumIcons[] { EnumIcons.Info,EnumIcons.InfoSelected };
		private ImageList _imageListInfo;

		public UserControlSetupWizFeatures() {
			InitializeComponent();
			_imageListInfo=IconLibrary.GetImageList(ArrayIconsImageList,16);//UI-9b-3
			foreach(Label label in new Label[] { labelInfoMedIns,labelInfoEHR,labelInfoClinical,labelInfoClinics,labelInfoInsurance,labelInfoCapitation,labelInfoMedicaid }) {
				label.ImageList=_imageListInfo;
			}
			Disposed+=(sender,e) => _imageListInfo?.Dispose();
			this.OnControlDone += ControlDone;
		}

		private void UserControlSetupWizFeatures_Load(object sender,EventArgs e) {
			RefreshControls();
		}

		private void RefreshControls() {
			checkCapitation.Checked=!PrefC.GetBool(PrefName.EasyHideCapitation);
			checkMedicaid.Checked=!PrefC.GetBool(PrefName.EasyHideMedicaid);
			checkInsurance.Checked=!PrefC.GetBool(PrefName.EasyHideInsurance);
			checkClinical.Checked=!PrefC.GetBool(PrefName.EasyHideClinical);
			checkNoClinics.Checked=PrefC.HasClinicsEnabled;
			checkMedicalIns.Checked=PrefC.GetBool(PrefName.ShowFeatureMedicalInsurance);
			checkEhr.Checked=PrefC.GetBool(PrefName.ShowFeatureEhr);
			IsDone=true;
		}

		private void labelInfo_MouseClick(object sender,MouseEventArgs e) {
			panelInfo.Controls.OfType<Label>().ToList().ForEach(x => x.ImageIndex=0);
			//foreach(Control item in panelInfo.Controls) {
			//	if(item.GetType() == typeof(Label)) {
			//		((Label)item).ImageIndex = 0;
			//	}
			//}
			((Label)sender).ImageIndex = 1;
			labelExplanation.Text = (string)((Label)sender).Tag;
		}

		///<summary>The same confirmation and messages as Show Features (FormShowFeatures.HandleEhrClick). The value is saved in ControlDone.</summary>
		private void checkEhr_Click(object sender,EventArgs e) {
			checkEhr.Checked=FormShowFeatures.HandleEhrClick(checkEhr.Checked);
		}

		private void butAdvanced_Click(object sender,EventArgs e) {
			if(!Security.IsAuthorized(EnumPermType.ShowFeatures)) {
				return;
			}
			using FormShowFeatures FormFS=new FormShowFeatures();
			FormFS.ShowDialog();
			RefreshControls();
		}

		private void ControlDone(object sender, EventArgs e) {
			if(
				Prefs.UpdateBool(PrefName.EasyHideCapitation,!checkCapitation.Checked)
				| Prefs.UpdateBool(PrefName.EasyHideMedicaid,!checkMedicaid.Checked)
				| Prefs.UpdateBool(PrefName.EasyHideInsurance,!checkInsurance.Checked)
				| Prefs.UpdateBool(PrefName.EasyHideClinical,!checkClinical.Checked)
				| Prefs.UpdateBool(PrefName.EasyNoClinics,!checkNoClinics.Checked)
				| Prefs.UpdateBool(PrefName.ShowFeatureMedicalInsurance,checkMedicalIns.Checked)
				| Prefs.UpdateBool(PrefName.ShowFeatureEhr,checkEhr.Checked)
			) {
				DataValid.SetInvalid(InvalidType.Prefs);
			}
		}
	}
}
