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
			StringAssert.Contains(MethodBody(images,"public void ModuleUnselected("),"_controlCameraSession?.End();");
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
			StringAssert.Contains(MethodBody(Images(),"protected override bool ProcessCmdKey("),"Keys.Control|Keys.Shift|Keys.C");
		}

		[TestMethod]
		public void Capture_PlansTheSlotFirst_ThenSaves_AndReleasesTheGate() {
			string capture=MethodBody(Session(),"public void RequestCapture(");
			int idxPlan=capture.IndexOf("_model.PlanShot()");
			int idxBegin=capture.IndexOf("BeginCapture(");
			int idxSave=capture.IndexOf("PhotoSaver.Save(");
			int idxEnd=capture.IndexOf("EndCapture(captureStart)");
			Assert.IsTrue(idxPlan>0 && idxBegin>idxPlan && idxSave>idxBegin && idxEnd>idxSave,"Plan, take, save, release.");
			Assert.IsFalse(capture.Contains("MessageBox") || capture.Contains("MsgBox"),"No message box in the middle of a session (design section 6).");
		}
	}
}
