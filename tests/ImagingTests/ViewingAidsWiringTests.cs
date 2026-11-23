// IMG-P3c-1: the viewing aids (magnifier, spot enhancer, the original), right-drag brightness/contrast, undo/redo and All in mount. The spot
// enhancer's stretch is tested on pictures; the rest reads the sources, because the windows cannot be opened in a test run. Licensed under the GNU
// General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using ForkMigrationTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalBusiness;

namespace ImagingTests {
	[TestClass]
	public class ViewingAidsWiringTests {
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

		[TestMethod]
		public void StretchContrast_SpreadsAFlatPatchToTheFullRange_KeepsGrey() {
			using Bitmap bitmap=new Bitmap(50,10,PixelFormat.Format32bppArgb);
			for(int x=0;x<50;x++) {
				for(int y=0;y<10;y++) {
					int v=100+x;//100 to 149
					bitmap.SetPixel(x,y,Color.FromArgb(v,v,v));
				}
			}
			ImageAdjust.StretchContrast(bitmap);
			Assert.IsTrue(bitmap.GetPixel(1,5).R<15,"The dark end near black: "+bitmap.GetPixel(1,5));
			Assert.IsTrue(bitmap.GetPixel(48,5).R>240,"The light end near white: "+bitmap.GetPixel(48,5));
			Color color=bitmap.GetPixel(25,5);
			Assert.IsTrue(color.R==color.G && color.G==color.B,"Grey stays grey.");
			using Bitmap bitmapFlat=new Bitmap(8,8,PixelFormat.Format32bppArgb);
			using(Graphics g=Graphics.FromImage(bitmapFlat)) {
				g.Clear(Color.FromArgb(120,120,120));
			}
			ImageAdjust.StretchContrast(bitmapFlat);
			Assert.AreEqual(120,bitmapFlat.GetPixel(4,4).R,"A flat patch is left alone.");
		}

		[TestMethod]
		public void Keys_MagnifierSpotOriginalUndo() {
			string body=MethodBody(Display(),"protected override bool ProcessCmdKey(");
			StringAssert.Contains(body,"if(keyData==Keys.M && ");
			StringAssert.Contains(body,"_magnifierZoom=_magnifierZoom==0 ? 2 : (_magnifierZoom==2 ? 4 : 0);","Off, 2x, 4x.");
			StringAssert.Contains(body,"if(keyData==Keys.S && ");
			StringAssert.Contains(body,"SetShowingOriginal(!_isShowingOriginal);");
			StringAssert.Contains(body,"EventUndoRequested?.Invoke(this,keyData==(Keys.Control|Keys.Y));");
			StringAssert.Contains(MethodBody(Read("OpenPmsDental","Forms","FormImageViewer.cs"),"protected override bool ProcessCmdKey("),
				"_controlImageDisplay.RequestUndo(keyData==(Keys.Control|Keys.Y));");
		}

		[TestMethod]
		public void ViewingAids_OnTheScreenOnly_NothingSaved() {
			string display=Display();
			StringAssert.Contains(MethodBody(display,"private void panelMain_Paint("),"DrawViewingAids(g);");
			Assert.IsFalse(MethodBody(display,"private void DrawMount(Graphics g").Contains("DrawViewingAids"),"Not in prints and copies.");
			string bodyAids=MethodBody(display,"private void DrawViewingAids(");
			StringAssert.Contains(bodyAids,"ImageAdjust.StretchContrast(bitmapSpot);");
			StringAssert.Contains(bodyAids,"graphicsLoupe.ScaleTransform(_magnifierZoom,_magnifierZoom);");
			string bodyOriginal=MethodBody(display,"private void ShowOriginalPicture(");
			foreach(string text in new[] { "Documents.Update","ImageEnhance.Save","DocumentAdjusts." }) {
				Assert.IsFalse(bodyOriginal.Contains(text),"The original is shown, never saved: "+text);
				Assert.IsFalse(bodyAids.Contains(text),"The aids save nothing: "+text);
			}
			StringAssert.Contains(MethodBody(display,"private void panelMain_MouseDown("),"SetShowingOriginal(false);","A click ends it.");
			StringAssert.Contains(MethodBody(display,"public void InvalidateSettingsColor("),"_isShowingOriginal=false;","Any re-render ends it (no thumbnail of the original).");
			StringAssert.Contains(MethodBody(display,"public void SelectFromViewer("),"SetShowingOriginal(false);");
			StringAssert.Contains(MethodBody(display,"public void ClearObjects("),"_isShowingOriginal=false;");
			StringAssert.Contains(bodyAids,"DrawPictureForAid(graphicsLoupe);","The magnifier into its own bitmap, then clipped to its circle.");
		}

		[TestMethod]
		public void RightDrag_ChecksThePermission_SavesThroughOneSave_CancelsTheMenu_NotForDicom() {
			string display=Display();
			string bodyDrag=MethodBody(display,"private bool RightDrag(");
			StringAssert.Contains(bodyDrag,"ImageHelper.IsDicomPath(document.FileName)","A DICOM window stays on the module's slider.");
			StringAssert.Contains(bodyDrag,"if(!CanEditImage(idx,suppressMessage:true)){");
			StringAssert.Contains(bodyDrag,"InvalidateSettingsColor(doApplyAdjust:false);","Fast while it moves.");
			string bodyEnd=MethodBody(display,"private void RightDragEnd(");
			StringAssert.Contains(bodyEnd,"bool isSaved=ImageEnhance.Save(document,_documentDragOld,");
			Assert.IsTrue(bodyEnd.IndexOf("if(!isSaved){")<bodyEnd.IndexOf("EventWindowDragged?.Invoke("),"No events after a refused save.");
			StringAssert.Contains(bodyEnd,"EventWindowDragged?.Invoke(this,EventArgs.Empty);","The Enhance window follows.");
			StringAssert.Contains(bodyDrag,"panelMain.ContextMenuStrip=null;","No menu after a drag.");
			StringAssert.Contains(MethodBody(display,"private void EndRightDrag("),"BeginInvoke(new Action(() => panelMain.ContextMenuStrip=menuPanelMain));","The menu comes back.");
			StringAssert.Contains(MethodBody(display,"private void panelMain_MouseDown("),"EndRightDrag();","Another button ends a right-drag, saved, menu back.");
			StringAssert.Contains(display,"panelMain.MouseCaptureChanged+=","A lost capture ends it too.");
			StringAssert.Contains(MethodBody(display,"private void panelMain_MouseUp("),"_isMouseDownPanel=false;","No pan or item drag left behind.");
			StringAssert.Contains(bodyDrag,"if(_drawMode!=EnumDrawMode.None || _cropPanAdj==EnumCropPanAdj.Crop){","Drawing and cropping keep the right button.");
			StringAssert.Contains(bodyDrag,"_bitmapArrayShowing[idx]==null","Images only.");
			StringAssert.Contains(Read("OpenPmsDental","Main Modules","ControlImages.cs"),"_formImageEnhance.SyncWindowFromImage();");
		}

		[TestMethod]
		public void UndoRedo_AndAllInMount_SaveThroughTheEnhanceSave() {
			string window=Read("OpenPmsDental","Forms","FormImageEnhance.cs");
			string bodyUndo=MethodBody(window,"public void UndoRedo(");
			StringAssert.Contains(bodyUndo,"FlushPending();");
			StringAssert.Contains(bodyUndo,"SaveIfChanged();","An undo is saved like a change, and logged.");
			StringAssert.Contains(MethodBody(window,"private bool SaveIfChanged("),"if(!_isUndoing){","An undo is not a new step.");
			StringAssert.Contains(MethodBody(window,"public void SyncWindowFromImage("),"_stackUndo.Push(","A slider or right-drag window is an undo step.");
			StringAssert.Contains(bodyUndo,"if(!isSaved){","A refused undo puts everything back.");
			StringAssert.Contains(Read("OpenPmsDental","Main Modules","ControlImages.cs"),"controlImageDisplay.FuncCanUndo=","Ctrl+Z only taken with somewhere to go.");
			StringAssert.Contains(MethodBody(window,"public void SetTarget("),"_stackUndo.Clear();","History per image.");
			StringAssert.Contains(MethodBody(window,"private void butAllInMount_Click("),"if(!MsgBox.Show(this,MsgBoxButtons.OKCancel,");
			string bodyMount=MethodBody(Display(),"public int ApplyAdjustToMount(");
			StringAssert.Contains(bodyMount,"if(!ImageEnhance.Save(document,documentOld,settingsOld,imageAdjustSettings,PatientCur.PatNum,PatFolder)){",
				"Each image's permission.");
			StringAssert.Contains(bodyMount,"if(!isDicom && !(windowingMin==0 && windowingMax==0)){","A DICOM window stays.");
		}
	}
}
