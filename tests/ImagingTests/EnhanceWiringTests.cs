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
		public void Save_ByDocument_ThroughOneSaveThatChecksThePermissionAgain() {
			string display=Display();
			StringAssert.Contains(MethodBody(display,"public bool CanEditImage("),"ImageSignaturePermissions.IsAuthorizedToEdit(document,suppressMessage);");
			string body=MethodBody(display,"public bool SaveAdjust(Document document,");
			StringAssert.Contains(body,"if(!ImageEnhance.Save(document,documentOld,settingsOld,settingsNew,PatientCur.PatNum,PatFolder)){");
			Assert.IsFalse(body.Contains("ThumbnailRefresh();\r\n\t\t\tSetWindowingSlider") && !body.Contains("if(!IsMountShowing()){"),"A mount's thumbnail is left.");
			string save=MethodBody(Read("OpenPmsDentalBusiness","Imaging","ImageEnhance.cs"),"public static bool Save(");
			Assert.IsTrue(save.IndexOf("ImageSignaturePermissions.IsAuthorizedToEdit(document,suppressMessage:true)")<save.IndexOf("Documents.Update("),
				"The permission first (a signing while the window is open).");
			StringAssert.Contains(save,"documentAdjust.TrimStartMs=documentAdjustOld.TrimStartMs;","A video's trim stays.");
			StringAssert.Contains(save,"SecurityLogs.MakeLogEntry(EnumPermType.ImageEdit,patNum,logText);");
			StringAssert.Contains(save,"ImageSignaturePermissions.LogSignedImageEdited(document,\"enhanced\");");
			string window=Window();
			StringAssert.Contains(window,"_isEditable=_controlImageDisplay.CanEditImage(_idx,suppressMessage:true);");
			string bodySave=MethodBody(window,"private bool SaveIfChanged(");
			StringAssert.Contains(bodySave,"_controlImageDisplay.SaveAdjust(_document,_documentOld,_settingsOld,_settings);","By document, not by index.");
			StringAssert.Contains(bodySave,"ImageEnhance.Save(_document,_documentOld,_settingsOld,_settings,_document.PatNum,patFolder);","Without the display.");
		}

		[TestMethod]
		public void Window_RechecksThePermission_FlushesBeforeItMovesOn_FollowsTheModulesSlider_ClosesOnAPatientChange() {
			string window=Window();
			string bodyTarget=MethodBody(window,"public void SetTarget(");
			Assert.IsTrue(bodyTarget.IndexOf("bool isEditable=_controlImageDisplay.CanEditImage(_idx,suppressMessage:true);")<bodyTarget.IndexOf("FlushPending();"),
				"The same image: the permission again (signed in place).");
			StringAssert.Contains(MethodBody(window,"protected override void OnFormClosing("),"FlushPending();");
			StringAssert.Contains(MethodBody(window,"private void FlushPending("),"Preview(doFilter:true);","Shown with its filters.");
			StringAssert.Contains(MethodBody(window,"private void timerFilter_Tick("),"SaveIfChanged();","The wheel and keys have no MouseUp.");
			string module=Module();
			StringAssert.Contains(MethodBody(module,"private void windowingSlider_ScrollComplete("),"_formImageEnhance.SyncWindowFromImage();");
			StringAssert.Contains(MethodBody(module,"public void CloseFloaters("),"_formImageEnhance.Close();");
			StringAssert.Contains(module,"ShowEnhance((ControlImageDisplay)sender)","The display that asked.");
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
			StringAssert.Contains(MethodBody(window,"private void Preview("),"if(!_isDicom && (_isWindowTouched || _windowFixed!=null)){",
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
