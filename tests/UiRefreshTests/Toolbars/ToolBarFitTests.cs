// UI-6's toolbar test (docs/specs/ui-refresh-plan.md, section 7's UI-6 row and section 9; docs/port-notes/UI-6.md): every module toolbar and the
// main menu at window widths 1024, 1366 and 1920 px keep their order and fit, or the overflow is listed for the owner. The buttons come from the
// modules' own LayoutToolBar code (ToolBarSources, every button the code can add), laid out by the toolbars' own code (ToolBarModel), at 100% in
// today's font and in Segoe UI 9 (UI-2b's font, so the owner sees now what the flip will push past the right edge, UQ-9). A toolbar's width
// comes from the main window's layout: the window less its two 5 px borders and the 51 px module bar. Data\toolbar-overflow.tsv lists the
// overflow the program has: the test fails when a toolbar overflows where the list does not say so, or no longer overflows where it does (run
// with OPMS_UI6_WRITE_OVERFLOW=1 to rewrite the list, then review the diff). The window check UT1 (tests\WindowChecksTests) measures the same in
// the running program, with the office's program link buttons. No window, no database.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental.UI;
using UiRefreshTests.Scan;

namespace UiRefreshTests.Toolbars {
	[TestClass]
	public class ToolBarFitTests {
		///<summary>Set to 1 to rewrite Data\toolbar-overflow.tsv from this run.</summary>
		public const string VARIABLE_WRITE="OPMS_UI6_WRITE_OVERFLOW";
		///<summary>The list of the overflow the program has.</summary>
		public const string FILE_OVERFLOW=@"tests\UiRefreshTests\Data\toolbar-overflow.tsv";

		///<summary>One toolbar or the menu at one window width in one font.</summary>
		public class FitRow {
			public string Id;
			public string Font;
			public int WidthWindow;
			public int WidthAvailable;
			public int WidthNeeded;
			public List<string> ListOverflow;
			public List<LaidOutItem> ListItems;

			public string Key {
				get { return Id+"\t"+Font+"\t"+WidthWindow; }
			}
		}

		///<summary>The two fonts measured: today's (the toolbars' own Microsoft Sans Serif 8.25) and UI-2b's Segoe UI 9.</summary>
		public static List<Tuple<string,Font>> GetFonts() {
			return new List<Tuple<string,Font>> {
				Tuple.Create("today",(Font)null),
				Tuple.Create("segoe9",new Font(UiTheme.FONT_FAMILY,UiTheme.FONT_SIZE_BODY)),
			};
		}

		///<summary>Every toolbar and the menu at every width in both fonts. Call on an STA thread.</summary>
		public static List<FitRow> MeasureAll() {
			NoDatabaseGuard.Install();
			List<FitRow> listRows=new List<FitRow>();
			List<ToolBarSource> listToolBars=ToolBarSources.ReadAll();
			foreach(ToolBarSource toolBarSource in listToolBars) {
				ImageList imageList=ToolBarModel.GetImageList(toolBarSource);
				foreach(Tuple<string,Font> tupleFont in GetFonts()) {
					if(toolBarSource.IsWpf && tupleFont.Item2!=null) {
						continue;//The WPF toolbar draws in WPF's own Segoe UI 9 (12 DIP) already; the flip does not change it.
					}
					//Today's: the toolbar's own font from today's tokens (the program's tokens are UI-2b's since the flip).
					UiTheme.SetFontForTest(tupleFont.Item2==null ? UiFontOption.Today : null);
					try {
						foreach(int widthWindow in ToolBarSources.ArrayWindowWidths) {
							int widthAvailable=toolBarSource.FuncWidth(ToolBarSources.GetModuleWidth(widthWindow));
							List<LaidOutItem> listItems;
							if(toolBarSource.IsWpf) {
								listItems=ToolBarModel.LayOutWpf(toolBarSource);
							}
							else {
								using(ToolBarOD toolBarOD=ToolBarModel.BuildToolBarOD(toolBarSource,imageList,widthAvailable,tupleFont.Item2)) {
									listItems=ToolBarModel.LayOut(toolBarOD,toolBarSource);
								}
							}
							listRows.Add(new FitRow { Id=toolBarSource.Id,Font=tupleFont.Item1,WidthWindow=widthWindow,WidthAvailable=widthAvailable,
								WidthNeeded=listItems.Count==0 ? 0 : listItems.Max(x => x.Right),ListOverflow=ToolBarModel.GetOverflow(listItems,widthAvailable),ListItems=listItems });
						}
					}
					finally {
						UiTheme.SetFontForTest(null);
					}
				}
			}
			List<MenuItemSource> listMenu=ToolBarSources.ReadMainMenu();
			foreach(Tuple<string,Font> tupleFont in GetFonts()) {
				Font font=tupleFont.Item2??new Font(UiTheme.FONT_FAMILY_TODAY,8.25f);
				List<LaidOutItem> listItems=ToolBarModel.LayOutMenu(listMenu,font);
				foreach(int widthWindow in ToolBarSources.ArrayWindowWidths) {
					int widthAvailable=ToolBarSources.GetMenuWidth(widthWindow);
					listRows.Add(new FitRow { Id="menu",Font=tupleFont.Item1,WidthWindow=widthWindow,WidthAvailable=widthAvailable,
						WidthNeeded=listItems.Max(x => x.Right),ListOverflow=ToolBarModel.GetOverflow(listItems,widthAvailable),ListItems=listItems });
				}
			}
			return listRows;
		}

		///<summary>The report for people: per toolbar, font and width, the room, the width the buttons need and what runs past the edge.</summary>
		public static string GetReport(List<FitRow> listRows) {
			StringBuilder stringBuilder=new StringBuilder();
			stringBuilder.AppendLine("toolbar\tfont\twindow\tavailable\tneeded\toverflow");
			foreach(FitRow fitRow in listRows) {
				stringBuilder.AppendLine(fitRow.Id+"\t"+fitRow.Font+"\t"+fitRow.WidthWindow+"\t"+fitRow.WidthAvailable+"\t"+fitRow.WidthNeeded+"\t"
					+string.Join(" | ",fitRow.ListOverflow));
			}
			return stringBuilder.ToString();
		}

		///<summary>The overflow list's lines (toolbar, font, window width, the captions past the edge), without comments.</summary>
		public static Dictionary<string,string> ReadOverflowList() {
			Dictionary<string,string> dict=new Dictionary<string,string>();
			string path=Path.Combine(RepoPaths.Root(),FILE_OVERFLOW);
			if(!File.Exists(path)) {
				return dict;
			}
			foreach(string line in File.ReadAllLines(path)) {
				if(line.Trim()=="" || line.StartsWith("#")) {
					continue;
				}
				string[] arrayFields=line.Split('\t');
				dict[arrayFields[0]+"\t"+arrayFields[1]+"\t"+arrayFields[2]]=arrayFields.Length>3 ? arrayFields[3] : "";
			}
			return dict;
		}

		public static void WriteOverflowList(List<FitRow> listRows) {
			StringBuilder stringBuilder=new StringBuilder();
			stringBuilder.Append("# The toolbars and the main menu whose items run past the right edge (UI-6, docs/port-notes/UI-6.md; ToolBarFitTests): toolbar, font\r\n");
			stringBuilder.Append("# (today: Microsoft Sans Serif 8.25; segoe9: UI-2b's Segoe UI 9), window width in px at 100%, the captions past the edge. Written by\r\n");
			stringBuilder.Append("# the test with OPMS_UI6_WRITE_OVERFLOW=1. Every button the code can add is counted (also those behind a preference); the office's\r\n");
			stringBuilder.Append("# program link buttons come after them. For the owner (UQ-9): a toolbar does not wrap or scroll, so what does not fit is cut off at the right edge.\r\n");
			foreach(FitRow fitRow in listRows.Where(x => x.ListOverflow.Count>0)) {
				stringBuilder.Append(fitRow.Key+"\t"+string.Join(" | ",fitRow.ListOverflow)+"\r\n");
			}
			File.WriteAllText(Path.Combine(RepoPaths.Root(),FILE_OVERFLOW),stringBuilder.ToString(),new UTF8Encoding(false));
		}

		[TestMethod]
		public void Sources_EveryModuleToolbarAndTheMenuAreRead() {
			List<ToolBarSource> listToolBars=ToolBarSources.ReadAll();
			foreach(ToolBarSource toolBarSource in listToolBars) {
				Assert.IsTrue(toolBarSource.ListButtons.Count(x => x.Style!="Separator")>=3,toolBarSource.Id+" has "+toolBarSource.ListButtons.Count+" buttons read.");
				Assert.IsFalse(toolBarSource.ListButtons.Any(x => x.Style!="Separator" && x.Text==null),toolBarSource.Id);
			}
			CollectionAssert.AreEquivalent(new[] { "main","appts","family","account","txplan","chart","imaging","imaging-paint","imaging-draw" },
				listToolBars.Select(x => x.Id).ToList());
			ToolBarSource toolBarAccount=listToolBars.Single(x => x.Id=="account");
			CollectionAssert.AreEqual(new[] { "Payment","Adjustment","New Claim","|","Payment Plan","Installment Plan","|","Quick Procs","Repeating Charge","|","Statement" },
				toolBarAccount.ListButtons.Select(x => x.ToString()).ToList(),"The Account module's buttons in their order (ControlAccount.LayoutToolBar).");
			Assert.AreEqual("DropDownButton",toolBarAccount.ListButtons[0].Style);
			Assert.AreEqual("Payment",toolBarAccount.ListButtons[0].Icon,"An EnumIcons since UI-9b-1 (today's ImageIndex 1).");
			Assert.IsTrue(toolBarAccount.ListButtons.Single(x => x.Text=="Repeating Charge").IsConditional,"Behind EasyHideRepeatCharges.");
			ToolBarSource toolBarMain=listToolBars.Single(x => x.Id=="main");
			Assert.AreEqual("Select Patient",toolBarMain.ListButtons[0].Text);
			Assert.AreEqual("PatSelect",toolBarMain.ListButtons[0].Icon);
			Assert.AreEqual("Popups",toolBarMain.ListButtons.Last().Text);
			ToolBarSource toolBarImaging=listToolBars.Single(x => x.Id=="imaging");
			Assert.AreEqual("Print",toolBarImaging.ListButtons[0].Text);
			Assert.AreEqual("DropDownButton",toolBarImaging.ListButtons.Single(x => x.Text=="Import").Style,"The style through the local buttonStyle.");
			List<MenuItemSource> listMenu=ToolBarSources.ReadMainMenu();
			CollectionAssert.AreEqual(new[] { "Log &Off","&File","&Setup","&Lists","&Reports","Custom Reports","&Tools","&Clinics","Alerts (0)","&Help" },
				listMenu.Select(x => x.Text).ToList(),"The main menu's top-level items (FormOpenDentalMenus.LayoutMenu).");
		}

		[TestMethod]
		public void Toolbars_KeepTheirOrder_AndFitAt1024_1366_1920_OrTheOverflowIsListed() {
			List<FitRow> listRows=null;
			IconRendererTests.RunSta(() => listRows=MeasureAll());
			string report=GetReport(listRows);
			Console.WriteLine(report);
			string folder=Environment.GetEnvironmentVariable(ToolBarPictureTests.VARIABLE_PICTURES);
			if(!string.IsNullOrWhiteSpace(folder)) {
				Directory.CreateDirectory(folder);
				File.WriteAllText(Path.Combine(folder,"toolbar-fit.tsv"),report,new UTF8Encoding(false));
			}
			//Order: each item starts where the one before it ends, in the order the code adds them (the width formula of 3.3 unchanged, UQ-9).
			foreach(FitRow fitRow in listRows) {
				for(int i=1;i<fitRow.ListItems.Count;i++) {
					Assert.AreEqual(fitRow.ListItems[i-1].Right,fitRow.ListItems[i].Left,fitRow.Key.Replace("\t"," ")+": item "+i+" ("+fitRow.ListItems[i].Text
						+") does not follow the one before it.");
				}
				Assert.IsTrue(fitRow.ListItems.All(x => x.Right>x.Left),fitRow.Key.Replace("\t"," ")+": an item with no width.");
			}
			if(ProjectFileTests.BuildOutput.IsRequiredValue(Environment.GetEnvironmentVariable(VARIABLE_WRITE))) {
				WriteOverflowList(listRows);
			}
			Dictionary<string,string> dictListed=ReadOverflowList();
			List<string> listProblems=new List<string>();
			foreach(FitRow fitRow in listRows) {
				string overflow=string.Join(" | ",fitRow.ListOverflow);
				string listed;
				bool isListed=dictListed.TryGetValue(fitRow.Key,out listed);
				if(fitRow.ListOverflow.Count>0 && (!isListed || listed!=overflow)) {
					listProblems.Add(fitRow.Key.Replace("\t"," ")+" px: past the right edge: "+overflow+(isListed ? " (listed: "+listed+")" : " (not in the list)"));
				}
				if(fitRow.ListOverflow.Count==0 && isListed) {
					listProblems.Add(fitRow.Key.Replace("\t"," ")+" px: fits now, but the list says "+listed);
				}
			}
			Assert.AreEqual(0,listProblems.Count,FILE_OVERFLOW+" does not match ("+VARIABLE_WRITE+"=1 rewrites it):\r\n"+string.Join("\r\n",listProblems));
		}
	}
}
