// Lan.C on ToolStrip items (package S3a-M1): a ContextMenuStrip or ToolStripDropDown is translated with the same translation keys (the class
// type and the English text, ampersands included) and to the same texts as the WinForms ContextMenu it replaces; separators are skipped
// instead of throwing. A grid's ContextMenuStrip is translated with the grid's TranslationName, as the grid's WinForms ContextMenu was until
// S3a-M3b removed that bridge. The translation tests run in a non-US culture on the scratch database's language tables (S3aM1Db).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental;

namespace S3aM1Tests {
	[TestClass]
	public class LanToolStripTests {
		private const string CLASS_TYPE="FormS3aM1Menus";
		private const string CULTURE="fr-CA";

		[TestInitialize]
		public void TestInitialize() {
			S3aM1Db.SetCultureUs();
		}

		#region Helpers
		///<summary>A menu as the forms build them: "&amp;File" with a submenu ("Open", a separator, "Close"), a separator, "Print" and "Help".</summary>
		private static ContextMenuStrip MakeStrip() {
			ContextMenuStrip contextMenuStrip=new ContextMenuStrip();
			ToolStripMenuItem toolStripMenuItemFile=new ToolStripMenuItem("&File");
			toolStripMenuItemFile.DropDownItems.Add(new ToolStripMenuItem("Open"));
			toolStripMenuItemFile.DropDownItems.Add(new ToolStripSeparator());
			toolStripMenuItemFile.DropDownItems.Add(new ToolStripMenuItem("Close"));
			contextMenuStrip.Items.Add(toolStripMenuItemFile);
			contextMenuStrip.Items.Add(new ToolStripSeparator());
			contextMenuStrip.Items.Add(new ToolStripMenuItem("Print"));
			contextMenuStrip.Items.Add(new ToolStripMenuItem("Help"));
			return contextMenuStrip;
		}

		///<summary>Every text, depth first, parents before their submenus; "-" for a separator.</summary>
		private static List<string> Texts(ToolStripItemCollection toolStripItemCollection) {
			List<string> listTexts=new List<string>();
			foreach(ToolStripItem toolStripItem in toolStripItemCollection) {
				listTexts.Add(toolStripItem is ToolStripSeparator ? "-" : toolStripItem.Text);
				if(toolStripItem is ToolStripMenuItem toolStripMenuItem) {
					listTexts.AddRange(Texts(toolStripMenuItem.DropDownItems));
				}
			}
			return listTexts;
		}

		private static void AddTranslations() {
			S3aM1Db.AddTranslation(CLASS_TYPE,"&File","&Fichier");
			S3aM1Db.AddTranslation(CLASS_TYPE,"Open","Ouvrir");
			S3aM1Db.AddTranslation(CLASS_TYPE,"Close","Fermer");
			S3aM1Db.AddTranslation(CLASS_TYPE,"Print","Imprimer");
			//"Help" has no translation: it keeps its English text and Lans adds it as a key.
		}
		#endregion Helpers

		[TestMethod]
		public void C_StripWithSeparators_DoesNotThrow_AndLeavesUsTextsAsTheyAre() {
			//Before S3a-M1, Lan.C cast every top-level item to ToolStripMenuItem and threw on a separator, in every culture.
			using(ContextMenuStrip contextMenuStrip=MakeStrip()) {
				Lan.C(CLASS_TYPE,contextMenuStrip);
				Lan.C((Control)null,contextMenuStrip);
				CollectionAssert.AreEqual(new List<string> { "&File","Open","-","Close","-","Print","Help" },Texts(contextMenuStrip.Items));
			}
		}

		[TestMethod]
		public void C_Strip_TranslatesLikeTheContextMenu_WithTheSameKeys() {
			S3aM1Db.ResetOrSkip();
			List<string> listExpected=new List<string> { "&Fichier","Ouvrir","-","Fermer","-","Imprimer","Help" };
			S3aM1Db.RunInCulture(CULTURE,() => {
				AddTranslations();
				S3aM1Db.RefreshLanguageCaches();
				using(ContextMenuStrip contextMenuStrip=MakeStrip()) {
					Lan.C(CLASS_TYPE,contextMenuStrip);
					CollectionAssert.AreEqual(listExpected,Texts(contextMenuStrip.Items));
				}
			});
			List<string> listKeysStrip=S3aM1Db.GetKeys(CLASS_TYPE);
			//The keys: the English texts with their ampersands. The WinForms ContextMenu (S3a-M1 compared the two) gave the same keys and texts
			//and also sent its separators' text "-" to Lans, which adds every text it has not seen as a key; a ToolStripSeparator has nothing to
			//translate, so "-" is the one key the strip does not add.
			CollectionAssert.AreEqual(new List<string> { "&File","Close","Help","Open","Print" },listKeysStrip);
		}

		[TestMethod]
		public void C_GridWithStrip_TranslatesTheStripWithTheGridsTranslationName() {
			//Lan.C (and Lan.F through it) translates a grid's ContextMenuStrip with grid.TranslationName as the class type, as it did a grid's WinForms
			//ContextMenu through the bridge that S3a-M3b removed (FormMedical's grids were its last; they get their strips after Lan.F, in Load, as
			//they got their ContextMenus, so their items are translated by Lan.g when they are made, as before), on fresh tables.
			S3aM1Db.ResetOrSkip();
			S3aM1Db.RunInCulture(CULTURE,() => {
				AddTranslations();
				S3aM1Db.RefreshLanguageCaches();
				using(OpenPmsDental.UI.GridOD grid=new OpenPmsDental.UI.GridOD())
				using(ContextMenuStrip contextMenuStrip=MakeStrip()) {
					grid.TranslationName=CLASS_TYPE;
					grid.DoAddGridItemsToContextMenuStrip=true;
					grid.ContextMenuStrip=contextMenuStrip;
					Lan.C("FormOther",grid);
					CollectionAssert.AreEqual(new List<string> { "&Fichier","Ouvrir","-","Fermer","-","Imprimer","Help" },Texts(contextMenuStrip.Items));
				}
			});
			CollectionAssert.AreEqual(new List<string> { "&File","Close","Help","Open","Print" },S3aM1Db.GetKeys(CLASS_TYPE));
			Assert.AreEqual(0,S3aM1Db.GetKeys("FormOther").Count,"The grid's items use its TranslationName, not the caller's class type.");
		}

		[TestMethod]
		public void C_ToolStripDropDownAndControlOverloads_UseTheControlsClassAsTheKey() {
			S3aM1Db.ResetOrSkip();
			S3aM1Db.RunInCulture(CULTURE,() => {
				S3aM1Db.AddTranslation(nameof(UserControl),"Print","Imprimer");
				S3aM1Db.RefreshLanguageCaches();
				using(UserControl userControl=new UserControl())
				using(ToolStripDropDown toolStripDropDown=new ToolStripDropDown())
				using(ToolStripDropDownMenu toolStripDropDownMenu=new ToolStripDropDownMenu()) {
					toolStripDropDown.Items.Add(new ToolStripMenuItem("Print"));
					toolStripDropDownMenu.Items.Add(new ToolStripSeparator());
					toolStripDropDownMenu.Items.Add(new ToolStripMenuItem("Print"));
					Lan.C(userControl,toolStripDropDown,toolStripDropDownMenu);
					Assert.AreEqual("Imprimer",toolStripDropDown.Items[0].Text);
					Assert.AreEqual("Imprimer",toolStripDropDownMenu.Items[1].Text);
				}
			});
		}
	}
}
