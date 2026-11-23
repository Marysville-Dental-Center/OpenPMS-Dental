// IMG-P4a: a mount template slot's camera guide in Setup, Mounts (FormMountItemDefEdit): the guide text, the slot kind and the mirror shot,
// read when the window opens and saved after the slot. The window cannot be opened in a test run, so these tests read the sources.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System.IO;
using ForkMigrationTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ImagingTests {
	[TestClass]
	public class SlotGuideWiringTests {
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

		[TestMethod]
		public void PastingAMount_CopiesEachSlotsGuide() {
			string source=Read("OpenPmsDental","User Controls","ControlImageDisplay.cs");
			int idxInsert=source.IndexOf("mountItem.MountNum=mount.MountNum;\r\n\t\t\t\t\tMountItems.Insert(mountItem);");
			if(idxInsert<0) {
				idxInsert=source.IndexOf("mountItem.MountNum=mount.MountNum;\n\t\t\t\t\tMountItems.Insert(mountItem);");
			}
			Assert.IsTrue(idxInsert>0,"The paste's item copy.");
			int idxGuide=source.IndexOf("MountItemGuides.CopyForMountItem(listMountItemsOrig[i].MountItemNum,mountItem.MountItemNum);",idxInsert);
			Assert.IsTrue(idxGuide>idxInsert && idxGuide-idxInsert<300,"The guide is copied right after the item.");
		}

		[TestMethod]
		public void SlotWindow_HasTheGuideControls() {
			string designer=Read("OpenPmsDental","Forms","FormMountItemDefEdit.designer.cs");
			foreach(string name in new[] { "textGuideLabel","comboSlotKind","checkMirror","labelGuideLabel","labelSlotKind" }) {
				StringAssert.Contains(designer,"this.Controls.Add(this."+name+");",name);
			}
			StringAssert.Contains(designer,"this.textGuideLabel.MaxLength = 255;","The column is varchar(255).");
		}

		[TestMethod]
		public void SlotWindow_ReadsTheGuideOnLoad_SavesItAfterTheSlot() {
			string source=Read("OpenPmsDental","Forms","FormMountItemDefEdit.cs");
			StringAssert.Contains(MethodBody(source,"private void FormMountItemDefEdit_Load("),"FillGuide();");
			StringAssert.Contains(MethodBody(source,"private void FillGuide("),"MountItemDefGuides.GetForMountItemDef(");
			string save=MethodBody(source,"private void butSave_Click(");
			int idxUpdate=save.IndexOf("MountItemDefs.Update(MountItemDefCur);");
			int idxGuide=save.IndexOf("SaveGuide();");
			Assert.IsTrue(idxUpdate>0 && idxGuide>idxUpdate,"The guide is saved after the slot, so a new slot has its key.");
			string saveGuide=MethodBody(source,"private void SaveGuide(");
			StringAssert.Contains(saveGuide,"MountItemDefGuides.DeleteForMountItemDef(","A plain radiograph slot keeps no row.");
			StringAssert.Contains(saveGuide,"MountItemDefGuides.Upsert(");
		}
	}
}
