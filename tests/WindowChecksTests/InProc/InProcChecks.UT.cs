// Group U's toolbar check UT1 (package UI-6 of docs/specs/ui-refresh-plan.md, section 7's UI-6 row; docs/port-notes/UI-6.md): in the running
// program on the scratch copy, the main window at 1024, 1366 and 1920 px wide (768 high), each module in turn: every visible ToolBarOD and the
// main menu keep their items in order, one after the other, and the items past the right edge are listed for the owner (UQ-9), now with the
// office's program link buttons of the copy. Each toolbar and the menu bar is pictured at each width (UT1-<width>-<module>-<toolbar>.png), the
// strips of gallery G-UI2's toolbar part. The thread's renderer is the token renderer. Nothing is clicked but the module buttons; nothing is saved.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CodeBase;
using OpenPmsDental;
using OpenPmsDental.UI;
using OpenPmsDentalBusiness;

namespace WindowChecksTests.InProc {
	public static partial class InProcChecks {
		///<summary>The window widths of the UI-6 row.</summary>
		private static readonly int[] _arrayWidthsUT=new int[] { 1024,1366,1920 };

		public static void CheckUT1(Ui ui,CheckResult checkResult) {
			FormWindowState formWindowStateOld=ui.Invoke(() => FormMain.WindowState);
			Rectangle rectangleOld=ui.Invoke(() => FormMain.Bounds);
			checkResult.Expect(ui.Invoke(() => ToolStripManager.Renderer is UiToolStripRenderer),"The main thread draws its stock menus with the token renderer");
			EnumModuleType[] arrayModules=new EnumModuleType[] { EnumModuleType.Appointments,EnumModuleType.Family,EnumModuleType.Account,
				EnumModuleType.TreatPlan,EnumModuleType.Chart,EnumModuleType.Imaging,EnumModuleType.Manage };
			try {
				SelectPatientU(ui,checkResult,true);
				foreach(int width in _arrayWidthsUT) {
					SetMainSize(ui,checkResult,new Size(width,768));
					foreach(EnumModuleType enumModuleType in arrayModules) {
						GoToModule(ui,enumModuleType);
						System.Threading.Thread.Sleep(500);
						List<ToolBarOD> listToolBars=ui.Invoke(() => Ui.AllControls(FormMain).OfType<ToolBarOD>().Where(x => x.Visible && x.Width>0)
							.OrderBy(x => x.Name).ToList());
						foreach(ToolBarOD toolBarOD in listToolBars) {
							string name=ui.Invoke(() => toolBarOD.Name=="" ? "toolbar" : toolBarOD.Name);
							if(enumModuleType!=EnumModuleType.Appointments && name=="ToolBarMain" && ui.Invoke(() => toolBarOD.Parent==FormMain)) {
								continue;//the main toolbar: measured once per width, with the Appointments module
							}
							CheckToolBarUT(ui,checkResult,toolBarOD,width+" "+enumModuleType+" "+name);
							checkResult.AddShot(ui.ShotDraw(toolBarOD,"UT1-"+width+"-"+enumModuleType+"-"+name+".png"),width+" px, "+enumModuleType+": "+name);
						}
					}
					MenuOD menuOD=ui.Invoke(() => (MenuOD)Ui.GetField(FormMain,"menuMain"));
					string[] arrayMenu=ui.Invoke(() => {
						ToolStrip toolStrip=(ToolStrip)menuOD.Controls[0];
						List<ToolStripItem> listItems=menuOD.GetMenuItems().Cast<ToolStripItem>().Where(x => x.Available).ToList();
						bool isInOrder=true;
						for(int i=1;i<listItems.Count;i++) {
							isInOrder&=listItems[i].Bounds.Left==listItems[i-1].Bounds.Right;
						}
						return new[] { isInOrder.ToString(),string.Join(" | ",listItems.Where(x => x.Bounds.Right>menuOD.Width).Select(x => x.Text)),
							menuOD.Width.ToString(),listItems.Count==0 ? "0" : listItems.Max(x => x.Bounds.Right).ToString() };
					});
					checkResult.Expect(arrayMenu[0]=="True","At "+width+" px the main menu's items follow one another in order");
					if(arrayMenu[1]!="") {
						checkResult.ForPerson("at "+width+" px the main menu's items past its right edge ("+arrayMenu[2]+" px, needed "+arrayMenu[3]+"): "+arrayMenu[1]);
					}
					checkResult.AddShot(ui.ShotDraw(menuOD,"UT1-"+width+"-menu.png"),width+" px: the main menu");
				}
			}
			finally {
				ui.Invoke(() => {
					FormMain.Bounds=rectangleOld;
					FormMain.WindowState=formWindowStateOld;
				});
			}
		}

		///<summary>One toolbar laid out by its own painting: its buttons follow one another in order; the ones past the right edge are noted for the
		///owner.</summary>
		private static void CheckToolBarUT(Ui ui,CheckResult checkResult,ToolBarOD toolBarOD,string what) {
			string[] array=ui.Invoke(() => {
				toolBarOD.Refresh();//OnPaint lays the buttons out (ComputeButtonsizes)
				List<ODToolBarButton> listButtons=toolBarOD.Buttons.Cast<ODToolBarButton>().ToList();
				bool isInOrder=true;
				for(int i=1;i<listButtons.Count;i++) {
					isInOrder&=listButtons[i].Bounds.Left==listButtons[i-1].Bounds.Right;
				}
				List<string> listOver=listButtons.Where(x => x.Style!=ODToolBarButtonStyle.Separator && x.Bounds.Right>toolBarOD.ClientSize.Width)
					.Select(x => x.Text).ToList();
				return new[] { isInOrder.ToString(),string.Join(" | ",listOver),toolBarOD.ClientSize.Width.ToString(),
					listButtons.Count==0 ? "0" : listButtons.Max(x => x.Bounds.Right).ToString(),listButtons.Count.ToString() };
			});
			checkResult.Expect(array[0]=="True",what+": the "+array[4]+" buttons follow one another in order");
			checkResult.Note(what+": "+array[2]+" px wide, the buttons need "+array[3]+" px");
			if(array[1]!="") {
				checkResult.ForPerson(what+": past the right edge: "+array[1]);
			}
		}
	}
}
