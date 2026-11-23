// IMG-P4b: the camera session in the Imaging module (owner imaging design, camera and video, sections 1, 2 and 6): the Camera drop-down that
// replaces Video, the session covering the module, its keys, the patient binding and the ends. The module cannot be opened in a test run, so
// these tests read the sources. Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System.IO;
using ForkMigrationTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ImagingTests {
	[TestClass]
	public class CameraSessionWiringTests {
		private static string Read(params string[] arrayParts) {
			return File.ReadAllText(TestDb.FindRepoFile(Path.Combine(arrayParts)));
		}

		private static string MethodBody(string source,string signature) {
			int idxStart=source.IndexOf(signature);
			Assert.IsTrue(idxStart>=0,signature+" not found.");
			int idxEnd=source.IndexOf("\n\t\t}",idxStart);
			Assert.IsTrue(idxEnd>idxStart,signature+" has no closing brace.");
			return source.Substring(idxStart,idxEnd-idxStart);
		}

		private static string Images() {
			return Read("OpenPmsDental","Main Modules","ControlImages.cs");
		}

		private static string Session() {
			return Read("OpenPmsDental","User Controls","ControlCameraSession.cs");
		}

		[TestMethod]
		public void Toolbar_CameraDropDown_ReplacesVideo_WithThePhotoMountsQuickPhotosAndTheVideoWindow() {
			string layout=MethodBody(Images(),"public void LayoutToolBars(");
			Assert.IsFalse(layout.Contains("Lan.g(this,\"Video\"),ToolBarVideo_Click"),"Video is no longer a toolbar button.");
			StringAssert.Contains(layout,"toolBarMain.Add(Lan.g(this,\"Camera\"),ToolBarCamera_Click,");
			StringAssert.Contains(layout,"contextMenuCamera.Opened+=contextMenuCamera_Opened;");
			string fill=MethodBody(Images(),"private void FillCameraMenu(");
			StringAssert.Contains(fill,"menuCameraMount_Click");
			StringAssert.Contains(fill,"menuCameraQuick_Click");
			StringAssert.Contains(fill,"menuCameraVideo_Click");
			StringAssert.Contains(MethodBody(Images(),"private void contextMenuCamera_Opened("),"GetPhotoMountDefs()");
		}

		[TestMethod]
		public void Start_NeedsImageCreate_AndAPatient_TheVideoWindowCloses() {
			string start=MethodBody(Images(),"private void StartCameraSession(");
			StringAssert.Contains(start,"Security.IsAuthorized(EnumPermType.ImageCreate)");
			StringAssert.Contains(start,"CreateMount(mountDef,_patient.PatNum,defNumCategory)");
			string core=MethodBody(Images(),"private void StartCameraSessionCore(");
			StringAssert.Contains(core,"_formLauncherVideo.Close();");
			StringAssert.Contains(core,"BeginInvoke((Action)controlCameraSession.OpenCamera);");
			StringAssert.Contains(MethodBody(Images(),"private void ToolBarCamera_Click("),"Security.IsAuthorized(EnumPermType.ImageCreate)");
		}

		[TestMethod]
		public void Session_EndsOnAPatientChange_LeavingTheModule_AndCoversTheModule() {
			string images=Images();
			StringAssert.Contains(MethodBody(images,"public void CloseFloaters("),"_controlCameraSession?.End(isPatientChange:true);");
			StringAssert.Contains(MethodBody(images,"public void ModuleUnselected("),"_controlCameraSession?.End(isModuleLeft:true);");
			StringAssert.Contains(MethodBody(images,"public void LayoutControls("),"_controlCameraSession.Bounds=new Rectangle(0,0,Width,Height);");
			StringAssert.Contains(MethodBody(images,"public void ControlImages_KeyDown("),"if(_controlCameraSession!=null){");
			string session=Session();
			string odEvent=MethodBody(session,"private void ODEvent_Fired(");
			StringAssert.Contains(odEvent,"PatientBinding.IsPatientChange(e)");
			StringAssert.Contains(odEvent,"Gate.BeginClosing();");
			StringAssert.Contains(MethodBody(session,"public void End("),"ReleaseCamera();");
		}

		[TestMethod]
		public void Session_Keys_SpaceArrowsEsc_AndNoDesignerFile() {
			string keys=MethodBody(Session(),"protected override bool ProcessCmdKey(");
			foreach(string text in new[] { "case Keys.Space:","case Keys.Right:","case Keys.Left:","case Keys.Escape:" }) {
				StringAssert.Contains(keys,text);
			}
			Assert.IsFalse(File.Exists(Path.Combine(Path.GetDirectoryName(TestDb.FindRepoFile(Path.Combine("OpenPmsDental","User Controls","ControlCameraSession.cs"))),
				"ControlCameraSession.Designer.cs")),"Built in code, so the UI rollout's designer data needs no rows.");
			StringAssert.Contains(MethodBody(Images(),"protected override bool ProcessCmdKey("),"Keys.Control|Keys.Shift|Keys.P");
			StringAssert.Contains(keys,"0x40000000","A held Space takes one photo.");
		}

		[TestMethod]
		public void Tools_FreezeTeethRetake_Keys_AndPermissions() {
			string session=Session();
			string keys=MethodBody(session,"protected override bool ProcessCmdKey(");
			StringAssert.Contains(keys,"case Keys.F:");
			StringAssert.Contains(keys,"case Keys.T:");
			StringAssert.Contains(keys,"case Keys.Control|Keys.Z:");
			int idxEscape=keys.IndexOf("case Keys.Escape:");
			Assert.IsTrue(keys.IndexOf("Unfreeze();",idxEscape)<keys.IndexOf("End();",idxEscape),"Esc goes back to live before it ends the session.");
			StringAssert.Contains(MethodBody(session,"private void Retake("),"Security.IsAuthorized(EnumPermType.ImageDelete,suppressMessage:true)");
			StringAssert.Contains(MethodBody(session,"private void Retake("),"_store.MoveToRetake(");
			StringAssert.Contains(MethodBody(session,"private void DeleteShot("),"ImageSignaturePermissions.IsAuthorizedToDelete(document)");
			StringAssert.Contains(MethodBody(session,"private void EditTeethShot("),"ImageSignaturePermissions.IsAuthorizedToEdit(document)");
			StringAssert.Contains(MethodBody(session,"private void RotateShot("),"ImageSignaturePermissions.IsAuthorizedToEdit(document)");
			string capture=MethodBody(session,"public void RequestCapture(");
			StringAssert.Contains(capture,"BeginCaptureFrozen(captureRequestSource,_bitmapFrozen)");
			StringAssert.Contains(capture,"Flash();");
			//review fixes: a camera problem, a new slot and a reconnect end a freeze; Ctrl+Z only for the photo just taken
			StringAssert.Contains(MethodBody(session,"private void ShowStatus("),"Unfreeze();");
			StringAssert.Contains(MethodBody(session,"private void RefreshCamera("),"Unfreeze();");
			StringAssert.Contains(MethodBody(session,"private void Skip("),"UnfreezeIfSlotChanged();");
			StringAssert.Contains(MethodBody(session,"private void RetakeLast("),"_docNumUndoable");
			StringAssert.Contains(MethodBody(session,"private void AfterShotRemoved("),"_docNumLast=");
		}

		[TestMethod]
		public void FullScreen_IdleRelease_UnsavedPhotos() {
			string session=Session();
			StringAssert.Contains(MethodBody(session,"protected override bool ProcessCmdKey("),"case Keys.F11:");
			StringAssert.Contains(MethodBody(session,"private void timerIdle_Tick("),"Sleep();");
			StringAssert.Contains(MethodBody(session,"public void Sleep("),"_videoCaptureSession.CloseCamera();");
			StringAssert.Contains(MethodBody(session,"public void RequestCapture("),"if(_isAsleep) {");
			StringAssert.Contains(MethodBody(session,"public void RequestCapture("),"KeepUnsaved(captureStart.Bitmap,photoShotPlan,photoSaveResult.Error);");
			StringAssert.Contains(MethodBody(session,"private void KeepUnsaved("),"PhotoQuarantine.Write(");
			string images=Images();
			string full=MethodBody(images,"private void controlCameraSession_EventFullScreenToggle(");
			StringAssert.Contains(full,"form.FormBorderStyle=FormBorderStyle.None;");
			StringAssert.Contains(full,"form.MinimizeBox=false;");
			StringAssert.Contains(full,"form.Resize+=");
			StringAssert.Contains(MethodBody(images,"private void controlCameraSession_EventEnded("),"e.CountKept==0");
			StringAssert.Contains(MethodBody(session,"private void timerIdle_Tick("),"if(!IsEnabledForCapture()) {");
			StringAssert.Contains(full,"System.Windows.Forms.Screen.FromControl(this).Bounds");
			Assert.IsFalse(full.Contains("FormWindowState.Maximized"),"The whole monitor by its bounds, never a maximised window.");
			StringAssert.Contains(MethodBody(images,"private void controlCameraSession_EventEnded("),"ExitCameraFullScreen();");
			StringAssert.Contains(MethodBody(images,"private void FillCameraMenu("),"menuCameraUnsaved_Click");
			StringAssert.Contains(MethodBody(images,"private void menuCameraUnsaved_Click("),"Security.IsAuthorized(EnumPermType.ImageCreate)");
		}

		[TestMethod]
		public void Capture_PlansTheSlotFirst_ThenSaves_AndReleasesTheGate() {
			string capture=MethodBody(Session(),"public void RequestCapture(");
			int idxPlan=capture.IndexOf("_model.PlanShot()");
			int idxBegin=capture.IndexOf("BeginCaptureFrozen(");
			int idxSave=capture.IndexOf("PhotoSaver.Save(");
			int idxEnd=capture.IndexOf("EndCapture(captureStart)");
			Assert.IsTrue(idxPlan>0 && idxBegin>idxPlan && idxSave>idxBegin && idxEnd>idxSave,"Plan, take, save, release.");
			Assert.IsFalse(capture.Contains("MessageBox") || capture.Contains("MsgBox"),"No message box in the middle of a session (design section 6).");
			int idxPlaced=capture.IndexOf("if(!photoSaveResult.IsPlaced) {");
			int idxMark=capture.IndexOf("_model.MarkSaved(photoShotPlan,");
			Assert.IsTrue(idxPlaced>0 && idxMark>idxPlaced,"A photo that is not in its slot leaves the slot next.");
		}
	}
}
