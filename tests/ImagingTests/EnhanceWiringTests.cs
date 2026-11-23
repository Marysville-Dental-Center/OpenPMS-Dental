// IMG-P3b: the Enhance window, its toolbar button and keys, saving and logging, the fast slider preview, the "Adjusted" tag (D4), the image
// viewer's refresh and the claim DICOM renders. The windows cannot be opened in a test run, so these tests read the sources. Licensed under the
// GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System.IO;
using ForkMigrationTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ImagingTests {
	[TestClass]
	public class EnhanceWiringTests {
		private static string Read(params string[] arrayParts) {
			return File.ReadAllText(TestDb.FindRepoFile(Path.Combine(arrayParts)));
		}

		///<summary>The text of one method: from its signature to the first closing brace at the method's own indent (two tabs).</summary>
		private static string MethodBody(string source,string signature) {
			int idxStart=source.IndexOf(signature);
			Assert.IsTrue(idxStart>=0,signature+" not found.");
			int idxEnd=source.IndexOf("\n\t\t}",idxStart);
			Assert.IsTrue(idxEnd>idxStart,signature+" has no closing brace.");
			return source.Substring(idxStart,idxEnd-idxStart);
		}

		private static string Display() {
			return Read("OpenPmsDental","User Controls","ControlImageDisplay.cs");
		}

		private static string Module() {
			return Read("OpenPmsDental","Main Modules","ControlImages.cs");
		}

		private static string Window() {
			return Read("OpenPmsDental","Forms","FormImageEnhance.cs");
		}

		[TestMethod]
		public void EnhanceButton_FollowsTheSelection_OneWindow() {
			string module=Module();
			StringAssert.Contains(module,"toolBarMain.Add(Lan.g(this,\"Enhance\"),ToolBarEnhance_Click,");
			StringAssert.Contains(module,"toolBarMain.SetEnabled(TB.Enhance.ToString(),toolBarButtonState.Flip);");
			StringAssert.Contains(module,"_formImageEnhance.SetTarget(GetControlImageDisplaySelected());","Every selection change.");
			StringAssert.Contains(MethodBody(module,"private void ShowEnhance("),"if(_formImageEnhance==null || _formImageEnhance.IsDisposed){");
			StringAssert.Contains(module,"controlImageDisplay.EventEnhanceRequested+=");
		}

		[TestMethod]
		public void AKey_InTheModuleAndTheViewer_AndTheViewerShowsASave() {
			StringAssert.Contains(MethodBody(Display(),"protected override bool ProcessCmdKey("),"if(keyData==Keys.A && GetTeethIdx()>-1){");
			string viewer=Read("OpenPmsDental","Forms","FormImageViewer.cs");
			StringAssert.Contains(MethodBody(viewer,"protected override bool ProcessCmdKey("),"_controlImageDisplay.RequestEnhance(_idxShowing);");
			StringAssert.Contains(viewer,"_controlImageDisplay.EventAdjustSaved+=controlImageDisplay_EventAdjustSaved;");
			StringAssert.Contains(viewer,"_controlImageDisplay.EventAdjustSaved-=controlImageDisplay_EventAdjustSaved;");
		}

		[TestMethod]
		public void Save_ChecksTheEditPermissions_Logs_KeepsTheTrim_RemakesTheThumbnail() {
			string display=Display();
			StringAssert.Contains(MethodBody(display,"public bool CanEditImage("),"ImageSignaturePermissions.IsAuthorizedToEdit(document,suppressMessage);");
			string body=MethodBody(display,"public void SaveAdjust(");
			StringAssert.Contains(body,"Documents.Update(document,documentOld);");
			StringAssert.Contains(body,"DocumentAdjusts.Upsert(documentAdjust);");
			StringAssert.Contains(body,"documentAdjust.TrimStartMs=documentAdjustOld.TrimStartMs;","A video's trim stays.");
			StringAssert.Contains(body,"SecurityLogs.MakeLogEntry(EnumPermType.ImageEdit,PatientCur.PatNum,logText);");
			StringAssert.Contains(body,"ImageSignaturePermissions.LogSignedImageEdited(document,\"enhanced\");");
			StringAssert.Contains(body,"ImageStore.DeleteThumbnailImage(document,PatFolder);");
			string window=Window();
			StringAssert.Contains(window,"_isEditable=_controlImageDisplay.CanEditImage(_idx,suppressMessage:true);");
			StringAssert.Contains(MethodBody(window,"private void SaveIfChanged("),"if(_document==null || !_isEditable");
		}

		[TestMethod]
		public void Presets_NeedTheImageAdjustPresetsPermission_AndAreLogged() {
			string window=Window();
			foreach(string signature in new[] { "private void butSavePreset_Click(","private void DeletePreset(","private void RenamePreset(","private void UpdatePreset(" }) {
				string body=MethodBody(window,signature);
				StringAssert.Contains(body,"if(!Security.IsAuthorized(EnumPermType.ImageAdjustPresets)){",signature);
				StringAssert.Contains(body,"SecurityLogs.MakeLogEntry(EnumPermType.ImageAdjustPresets,0,",signature);
			}
			StringAssert.Contains(MethodBody(window,"private void butPreset_MouseUp("),"if(imageAdjustPreset.ImageAdjustPresetNum==0){","Supplied presets stay.");
		}

		[TestMethod]
		public void Sliders_BrightnessContrastAtOnce_TheFiltersWhenTheyRest() {
			string module=Module();
			string bodyScroll=MethodBody(module,"private void windowingSlider_Scroll(");
			StringAssert.Contains(bodyScroll,"InvalidateSettingsColor(doApplyAdjust:false);","The module's slider while it moves.");
			Assert.IsFalse(MethodBody(module,"private void windowingSlider_ScrollComplete(").Contains("doApplyAdjust:false"),"Full when let go.");
			string window=Window();
			StringAssert.Contains(MethodBody(window,"private void track_ValueChanged("),"_timerFilter.Start();");
			StringAssert.Contains(MethodBody(window,"private void track_Commit("),"SaveIfChanged();");
			StringAssert.Contains(MethodBody(window,"private void Preview("),"if(!_isDicom && _isWindowTouched){",
				"A filter change keeps the window exactly; a DICOM window stays on the module's slider.");
		}

		[TestMethod]
		public void AdjustedTag_OnTheScreenOnly() {
			string display=Display();
			StringAssert.Contains(MethodBody(display,"private void panelMain_Paint("),"DrawAdjustedTags(g);");
			Assert.IsFalse(MethodBody(display,"private void DrawMount(Graphics g").Contains("DrawAdjustedTags"),"Not in prints and copies.");
			StringAssert.Contains(MethodBody(display,"private void DrawAdjustedTags("),"IsAdjustNeutral(","Brightness/contrast alone do not count (D4).");
		}

		[TestMethod]
		public void ClaimDicomRenders_GetTheFilters() {
			string logic=Read("OpenPmsDental","Logic","ClaimAttachDicomL.cs");
			StringAssert.Contains(logic,"ClaimAttachDicom.Render(document,bitmapDicom,ImageDraws.RefreshForDoc(document.DocNum),imageAdjustSettings);");
			StringAssert.Contains(logic,"ClaimAttachDicom.Render(document,bitmapDicom,imageAdjustSettings);");
			StringAssert.Contains(logic,"ClaimAttachDicom.Render(document,bitmapDicom,GetAdjustSettings(document));");
			StringAssert.Contains(Read("OpenPmsDentalBusiness","Imaging","ClaimAttachDicom.cs"),"ImageAdjust.ApplyToCopy(bitmapWindowed,imageAdjustSettings);");
		}
	}
}
