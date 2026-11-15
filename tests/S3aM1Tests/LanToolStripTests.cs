// Lan.C on ToolStrip items (package S3a-M1): a ContextMenuStrip or ToolStripDropDown is translated with the same translation keys (the class
// type and the English text, ampersands included) and to the same texts as the WinForms ContextMenu it replaces; separators are skipped
// instead of throwing. The translation tests run in a non-US culture on the scratch database's language tables (S3aM1Db).
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

		///<summary>The same menu as a WinForms ContextMenu (what the forms build until S3a-M2/M3 convert them).</summary>
		private static ContextMenu MakeContextMenu() {
			MenuItem menuItemFile=new MenuItem("&File",new[] { new MenuItem("Open"),new MenuItem("-"),new MenuItem("Close") });
			return new ContextMenu(new[] { menuItemFile,new MenuItem("-"),new MenuItem("Print"),new MenuItem("Help") });
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

		private static List<string> Texts(Menu.MenuItemCollection menuItemCollection) {
			List<string> listTexts=new List<string>();
			foreach(MenuItem menuItem in menuItemCollection) {
				listTexts.Add(menuItem.Text);
				listTexts.AddRange(Texts(menuItem.MenuItems));
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
			//The same menu as a WinForms ContextMenu through what is left of the bridge (S3a-M3 removed Lan.C(string,params Menu[]); the grid
			//bridge's TranslateMenuItems, for FormMedical's grids until S3a-M3b, translates each item the same way), on fresh tables.
			S3aM1Db.ResetOrSkip();
			S3aM1Db.RunInCulture(CULTURE,() => {
				AddTranslations();
				S3aM1Db.RefreshLanguageCaches();
				using(ContextMenu contextMenu=MakeContextMenu()) {
					System.Reflection.MethodInfo methodInfoTranslate=typeof(Lan).GetMethod("TranslateMenuItems",
						System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static);
					foreach(MenuItem menuItem in contextMenu.MenuItems) {
						methodInfoTranslate.Invoke(null,new object[] { CLASS_TYPE,menuItem });
					}
					CollectionAssert.AreEqual(listExpected,Texts(contextMenu.MenuItems));
				}
			});
			List<string> listKeysMenu=S3aM1Db.GetKeys(CLASS_TYPE);
			//The keys: the English texts with their ampersands. The ContextMenu also sent its separators' text "-" to Lans, which adds every text
			//it has not seen as a key; a ToolStripSeparator has nothing to translate, so "-" is the one key the strip does not add.
			CollectionAssert.AreEqual(new List<string> { "&File","Close","Help","Open","Print" },listKeysStrip);
			CollectionAssert.AreEqual(listKeysStrip,listKeysMenu.Where(x => x!="-").ToList());
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
