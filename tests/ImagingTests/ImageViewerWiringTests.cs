// The image viewer window (imaging design P1, owner 2026-11-22): a double-click on an image opens that image in its own window at normal size,
// never full screen or maximised, and never Item Info. The windows cannot be opened in a test run (the GUI is not launched), so these tests read
// the sources. Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System.IO;
using ForkMigrationTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ImagingTests {
	[TestClass]
	public class ImageViewerWiringTests {
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

		private static string Viewer() {
			return Read("OpenPmsDental","Forms","FormImageViewer.cs");
		}

		[TestMethod]
		public void CanvasDoubleClick_OpensTheViewer_NeverItemInfoOrMountEdit() {
			string body=MethodBody(Display(),"private void panelMain_MouseDoubleClick(");
			StringAssert.Contains(body,"int idxHit=HitTestInMount(e.X,e.Y);","The slot under the pointer, not the selection.");
			StringAssert.Contains(body,"EventViewerRequested?.Invoke(this,idxHit);","A filled mount slot opens in the viewer.");
			StringAssert.Contains(body,"EventViewerRequested?.Invoke(this,0);","A single image opens in the viewer.");
			StringAssert.Contains(body,"GetViewerIndexes().Contains(idxHit)","An empty slot opens nothing.");
			Assert.IsFalse(body.Contains("FrmDocInfo"),"A double-click no longer opens Item Info.");
			Assert.IsFalse(body.Contains("FrmMountEdit"),"A double-click on the mount's background no longer opens Mount Edit.");
			StringAssert.Contains(body,"FrmImageDrawEdit","Text drawing mode still edits the text on a double-click.");
			StringAssert.Contains(body,"FrmMountItemEdit","A mount's text item still opens its editor.");
		}

		[TestMethod]
		public void ItemInfo_StaysOnTheInfoButtonTheMenuAndAltEnter() {
			string display=Display();
			string bodyInfo=MethodBody(display,"public void ToolBarInfo_Click(");
			StringAssert.Contains(bodyInfo,"FrmDocInfo");
			StringAssert.Contains(bodyInfo,"FrmMountEdit","Info with no mount item selected edits the mount.");
			StringAssert.Contains(MethodBody(display,"private void menuPanelMain_Click("),"menuItemInfo");
			string bodyKey=MethodBody(display,"protected override bool ProcessCmdKey(");
			StringAssert.Contains(bodyKey,"keyData==(Keys.Alt|Keys.Enter)");
			StringAssert.Contains(bodyKey,"ToolBarInfo_Click(this,EventArgs.Empty);");
		}

		[TestMethod]
		public void TreeDoubleClick_OpensAnImageInTheViewer_AndAMountAsAClickDoes() {
			string body=MethodBody(Module(),"private void imageSelector_ItemDoubleClick(");
			StringAssert.Contains(body,"BeginInvoke(new Action(() => ShowImageViewer(controlImageDisplay,0)));","After the tree's mouse sequence.");
			Assert.IsFalse(body.Contains("FrmDocInfo"),"The tree's double-click no longer opens Item Info.");
			Assert.IsFalse(body.Contains("FrmMountEdit"),"The tree's double-click no longer opens Mount Edit.");
			StringAssert.Contains(body,"ODFileUtils.ShellOpen(filePath);","Other files still open in their own program.");
		}

		[TestMethod]
		public void Viewer_OneReusedWindow_ClosedOnPatientChange_NeverMaximised() {
			string module=Module();
			StringAssert.Contains(module,"controlImageDisplay.EventViewerRequested+=");
			string body=MethodBody(module,"private void ShowImageViewer(");
			StringAssert.Contains(body,"if(_formImageViewer==null || _formImageViewer.IsDisposed){","One window is reused.");
			StringAssert.Contains(body,"_formImageViewer.SetSource(controlImageDisplay,idx);");
			StringAssert.Contains(body,"_formImageViewer.SetStartBounds(controlImageDisplay);");
			StringAssert.Contains(MethodBody(module,"public void CloseFloaters("),"_formImageViewer.Close();","A patient change closes the viewer.");
			string viewer=Viewer();
			Assert.IsFalse(body.Contains("Maximized"),"The viewer never opens maximised (owner, 2026-11-22).");
			Assert.IsFalse(viewer.Contains("FormWindowState.Maximized"),"The viewer never maximises itself.");
			Assert.IsFalse(viewer.Contains("FormBorderStyle.None"),"No borderless full-screen window.");
		}

		[TestMethod]
		public void Viewer_KeysAndMouse_AndReturnsToTheMountOnTheImageLastShown() {
			string viewer=Viewer();
			string bodyKey=MethodBody(viewer,"protected override bool ProcessCmdKey(");
			StringAssert.Contains(bodyKey,"case Keys.Escape:");
			StringAssert.Contains(bodyKey,"case Keys.Left:");
			StringAssert.Contains(bodyKey,"case Keys.Right:");
			StringAssert.Contains(bodyKey,"case Keys.Home:");
			StringAssert.Contains(MethodBody(viewer,"private void canvas_MouseDoubleClick("),"Close();");
			StringAssert.Contains(MethodBody(viewer,"private void FormImageViewer_FormClosing("),"_controlImageDisplay.SelectFromViewer(_idxShowing);");
			StringAssert.Contains(MethodBody(Display(),"public void SelectFromViewer("),"LoadBitmap(idx,EnumLoadBitmapType.OnlyRaw);");
		}

		[TestMethod]
		public void Viewer_ChangesNothing() {
			string viewer=Viewer();
			foreach(string text in new[] { "Documents.Update","MountItems.Update","SecurityLogs.","Security.IsAuthorized","ImageStore." }) {
				Assert.IsFalse(viewer.Contains(text),"The viewer only shows the image; it does not use "+text);
			}
		}

		[TestMethod]
		public void ViewerBitmap_IsTheImageAsTheModuleShowsIt() {
			string body=MethodBody(Display(),"public Bitmap GetViewerBitmap(");
			StringAssert.Contains(body,"DrawDocument(g);","A single image draws as the module draws it, drawings included.");
			StringAssert.Contains(body,"ImageHelper.DrawDocument(g,document,bitmapShowing,new List<ImageDraw>());","A mount item: crop, rotation and flip.");
			StringAssert.Contains(body,"DrawDrawings(0,g);","The mount's drawings over the item.");
			StringAssert.Contains(MethodBody(Display(),"public List<int> GetViewerIndexes("),"_listMountItems[i].ItemOrder<1",
				"Text items (0) and the unmounted images (-1) are not slots of the mount.");
		}
	}
}
