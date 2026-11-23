// Reassigning an image's teeth (imaging design P2, owner 2026-11-22): the Teeth button, the right-click menu's Teeth, the T key in the module and
// in the image viewer window, the tooth picker, and the rule for an image moved to another slot. The windows cannot be opened in a test run, so
// these tests read the sources. Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System.IO;
using System.Text.RegularExpressions;
using ForkMigrationTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ImagingTests {
	[TestClass]
	public class TeethWiringTests {
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
		public void TeethButton_OnTheToolbar_EnabledLikeFlip() {
			string module=Read("OpenPmsDental","Main Modules","ControlImages.cs");
			StringAssert.Contains(module,"toolBarMain.Add(Lan.g(this,\"Teeth\"),ToolBarTeeth_Click,WpfControls.UI.EnumIcons.Chart32G,tag:TB.Teeth.ToString(),");
			StringAssert.Contains(module,"toolBarMain.SetEnabled(TB.Teeth.ToString(),toolBarButtonState.Flip);","One image: a single image or a mount item.");
			StringAssert.Contains(MethodBody(module,"private void ToolBarTeeth_Click("),"GetOrMakeControlImageDisplay().ToolBarTeeth_Click();");
		}

		[TestMethod]
		public void RightClickMenu_TeethAfterInfo_MadeInCode_HiddenWithoutAnImage() {
			string display=Display();
			StringAssert.Contains(display,"menuPanelMain.Items.Insert(menuPanelMain.Items.IndexOf(menuItemInfo)+1,_menuItemTeeth);");
			StringAssert.Contains(display,"_menuItemTeeth.Click+=menuPanelMain_Click;");
			StringAssert.Contains(MethodBody(display,"private void menuPanelMain_Click("),"else if(menuItem==_menuItemTeeth) {\r\n\t\t\t\tToolBarTeeth_Click();");
			StringAssert.Contains(display,"_menuItemTeeth.Visible=false;//no image selected");
			Assert.IsFalse(Read("OpenPmsDental","User Controls","ControlImageDisplay.Designer.cs").Contains("menuItemTeeth"),
				"The designer file is unchanged (the UI rollout's designer data).");
		}

		[TestMethod]
		public void TKey_InTheModuleAndInTheViewer() {
			StringAssert.Contains(MethodBody(Display(),"protected override bool ProcessCmdKey("),"if(keyData==Keys.T && GetTeethIdx()>-1){");
			string bodyViewer=MethodBody(Read("OpenPmsDental","Forms","FormImageViewer.cs"),"protected override bool ProcessCmdKey(");
			StringAssert.Contains(bodyViewer,"case Keys.T:");
			StringAssert.Contains(bodyViewer,"_controlImageDisplay.EditTeeth(_idxShowing);","The module saves; the viewer only shows.");
		}

		[TestMethod]
		public void EditTeeth_ChecksTheEditPermissions_LogsTheChange_ChangesTheSlotOnlyWhenAsked() {
			string body=MethodBody(Display(),"public void EditTeeth(");
			StringAssert.Contains(body,"if(!ImageSignaturePermissions.IsAuthorizedToEdit(document)) {","ImageEdit, and SignedImageEdit for a signed image.");
			StringAssert.Contains(body,"Documents.Update(document,documentOld);");
			StringAssert.Contains(body,"SecurityLogs.MakeLogEntry(EnumPermType.ImageEdit,PatientCur.PatNum,logText);");
			StringAssert.Contains(body,"ImageSignaturePermissions.LogSignedImageEdited(document,\"teeth changed\");");
			StringAssert.Contains(body,"if(formToothPicker.IsSlotChanged && mountItem!=null && mountItem.ToothNumbers!=toothNumbersNew){");
			StringAssert.Contains(body,"MountItems.Update(mountItem);");
			Assert.IsFalse(body.Contains("MountItemDef"),"The mount's template is never changed from here.");
		}

		[TestMethod]
		public void SwapAndMove_TakeTheNewSlotsTeeth() {
			string display=Display();
			Assert.AreEqual(3,Regex.Matches(display,Regex.Escape("ImageTeeth.GetTeethAfterMove(")).Count,"The swap (both images) and the move to an empty slot.");
			Assert.IsFalse(display.Contains("//only if unchanged by user"),"The old rule (teeth follow only when unchanged) is replaced by design D6.");
		}

		[TestMethod]
		public void ToothPicker_MadeInCode_SlotBoxUnticked_ReturnsTheResultOnly() {
			Assert.IsFalse(File.Exists(Path.Combine(Path.GetDirectoryName(TestDb.FindRepoFile(Path.Combine("OpenPmsDental","Forms","FormToothPicker.cs"))),
				"FormToothPicker.Designer.cs")),"No designer file (the UI rollout's designer data).");
			string picker=Read("OpenPmsDental","Forms","FormToothPicker.cs");
			Assert.IsFalse(picker.Contains("checkSlot.Checked=true"),"Changing the slot's teeth is opt-in.");
			StringAssert.Contains(picker,"Tooth.Display(toothCell.ToothNum,_toothNumberingNomenclature)","The office's tooth numbering.");
			StringAssert.Contains(picker,"Tooth.ParseRange(textTeeth.Text)","Typed teeth are read as Item Info reads them.");
			foreach(string text in new[] { "Documents.Update","MountItems.Update","SecurityLogs." }) {
				Assert.IsFalse(picker.Contains(text),"The picker only returns the teeth; the module saves them. Found: "+text);
			}
		}
	}
}
