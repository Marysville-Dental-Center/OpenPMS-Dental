// Group U's frame checks UX1 to UX3 (package UI-3 of docs/specs/ui-refresh-plan.md, section 3.6 "Window frame" and section 6 "Misc Colors";
// docs/port-notes/UI-3.md), the parts of gallery G-UI2 that need a shown window: UX1 a customised office (Misc Colors 11 to 13 changed) on the
// main window, a FormODBase dialog and a WPF window in FormFrame; UX2 the Imaging module with two floating image windows, one selected and one
// not; UX3 the trial of Windows 11's rounded corners (FramePainter.ENV_DWM_CORNERS) on a dialog. The frame's look without a shown window is
// tested in tests/UiRefreshTests/FrameTests.cs. They run in the in-process session of the window checks on the scratch copy, opt-in, in the
// owner's connected session (UTests.UX1toUX3). The office's colours are synthetic and are put back as they were.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using OpenPmsDental;
using OpenPmsDental.UI;
using OpenPmsDentalBusiness;

namespace WindowChecksTests.InProc {
	public static partial class InProcChecks {
		///<summary>UX1's office: a green title bar, a dark red outline and yellow text, all three changed from Open Dental's stock values.</summary>
		private static readonly Color _colorUX1Border=Color.FromArgb(32,96,64);
		private static readonly Color _colorUX1Outline=Color.FromArgb(120,0,0);
		private static readonly Color _colorUX1Text=Color.FromArgb(255,230,120);

		///<summary>UX1: a customised office. The three Misc Colors are changed in the copy and in the main window's frame (as FormOpenDental sets them
		///at start-up), then the main window, Help > About and Lists > Zip Codes (a Frm, whose FormFrame reads the colours on Load) are pictured.</summary>
		public static void CheckUX1(Ui ui,CheckResult checkResult) {
			List<Def> listDefsBefore=ui.Invoke(() => Defs.GetDefsForCategory(DefCat.MiscColors).Select(x => x.Copy()).ToList());
			bool hasItems=listDefsBefore.Count>(int)DefCatMiscColors.MainBorderText;
			if(!checkResult.Expect(hasItems,"The copy has the Misc Colors up to Main Border Text",listDefsBefore.Count.ToString())) {
				return;
			}
			try {
				SetOfficeTitleColors(ui,_colorUX1Border,_colorUX1Outline,_colorUX1Text);
				IntPtr hwndMain=ui.Invoke(() => FormMain.Handle);
				checkResult.AddShot(ui.Shot(hwndMain,"UX1-office-main.png"),"The main window of an office with its own title bar colours");
				Ui.Opened opened=OpenMenu(ui,"Help","About");
				if(opened.IsOpen) {
					checkResult.AddShot(ui.Shot(opened.Hwnd,"UX1-office-about.png"),"Help > About (FormODBase) in the office's colours");
					checkResult.Expect(ui.Close(opened.Hwnd),"About closes");
				}
				opened=OpenMenu(ui,"Lists","Zip Codes");
				if(opened.IsOpen) {
					checkResult.AddShot(ui.Shot(opened.Hwnd,"UX1-office-frm.png"),"Lists > Zip Codes (a Frm in FormFrame) in the office's colours");
					checkResult.Expect(ui.Close(opened.Hwnd),"Zip Codes closes");
				}
			}
			finally {
				SetOfficeTitleColors(ui,listDefsBefore[(int)DefCatMiscColors.MainBorder].ItemColor,listDefsBefore[(int)DefCatMiscColors.MainBorderOutline].ItemColor,
					listDefsBefore[(int)DefCatMiscColors.MainBorderText].ItemColor);
			}
			Color colorAfter=ui.Invoke(() => Defs.GetDefsForCategory(DefCat.MiscColors)[(int)DefCatMiscColors.MainBorder].ItemColor);
			checkResult.Expect(colorAfter.ToArgb()==listDefsBefore[(int)DefCatMiscColors.MainBorder].ItemColor.ToArgb(),"The office's colours are put back");
		}

		///<summary>Writes the three title bar Misc Colors to the copy, refreshes the definitions cache (FormFrame reads it on Load) and sets them on the
		///main window's frame as FormOpenDental does at start-up (FormODBase keeps them for every window).</summary>
		private static void SetOfficeTitleColors(Ui ui,Color colorBorder,Color colorOutline,Color colorText) {
			ui.Invoke(() => {
				List<Def> listDefs=Defs.GetDefsForCategory(DefCat.MiscColors);
				Color[] arrayColors=new[] { colorBorder,colorOutline,colorText };
				DefCatMiscColors[] arrayItems=new[] { DefCatMiscColors.MainBorder,DefCatMiscColors.MainBorderOutline,DefCatMiscColors.MainBorderText };
				for(int i=0;i<arrayItems.Length;i++) {
					Def def=listDefs[(int)arrayItems[i]].Copy();
					def.ItemColor=arrayColors[i];
					Defs.Update(def);
				}
				Cache.Refresh(InvalidType.Defs);
				MethodInfo methodInfo=typeof(FormODBase).GetMethod("SetBorderColor",BindingFlags.Instance|BindingFlags.NonPublic);
				for(int i=0;i<arrayItems.Length;i++) {
					methodInfo.Invoke(FormMain,new object[] { arrayItems[i],arrayColors[i] });
				}
			});
		}

		///<summary>UX2: the Imaging module with two floating image windows, the first selected and the second not (IsImageFloatSelected, which the
		///module sets when a floater is activated). The synthetic dump has no image files, so the floaters are empty: the check is about their
		///frames. Both are pictured, then closed.</summary>
		public static void CheckUX2(Ui ui,CheckResult checkResult) {
			GoToModule(ui,EnumModuleType.Imaging);
			if(!checkResult.Expect(GetModule(ui)==EnumModuleType.Imaging,"The Imaging module opens")) {
				return;
			}
			List<FormImageFloat> listFloats=new List<FormImageFloat>();
			try {
				ui.Invoke(() => {
					Rectangle rectangleMain=FormMain.Bounds;
					for(int i=0;i<2;i++) {
						FormImageFloat formImageFloat=new FormImageFloat();
						formImageFloat.Text=i==0 ? "Image 1 (selected)" : "Image 2";
						formImageFloat.Show(FormMain);
						formImageFloat.Bounds=new Rectangle(rectangleMain.X+300+i*460,rectangleMain.Y+220,440,320);
						listFloats.Add(formImageFloat);
					}
					listFloats[0].IsImageFloatSelected=true;
					listFloats[1].IsImageFloatSelected=false;
					foreach(FormImageFloat formImageFloat in listFloats) {
						formImageFloat.Refresh();
					}
				});
				IntPtr hwndSelected=ui.Invoke(() => listFloats[0].Handle);
				IntPtr hwndUnselected=ui.Invoke(() => listFloats[1].Handle);
				checkResult.Expect(ui.Invoke(() => listFloats[0].IsImageFloatSelected && !listFloats[1].IsImageFloatSelected),"One floater is selected, one is not");
				checkResult.AddShot(ui.Shot(hwndSelected,"UX2-imagefloat-selected.png"),"The selected floating image window");
				checkResult.AddShot(ui.Shot(hwndUnselected,"UX2-imagefloat-unselected.png"),"A floating image window that is not selected");
				IntPtr hwndMain=ui.Invoke(() => FormMain.Handle);
				checkResult.AddShot(ui.Shot(hwndMain,"UX2-imaging-main.png"),"The Imaging module behind the floaters");
			}
			finally {
				ui.Invoke(() => {
					foreach(FormImageFloat formImageFloat in listFloats) {
						if(!formImageFloat.IsDisposed) {
							formImageFloat.Close();
						}
					}
				});
			}
		}

		///<summary>UX3: the trial of rounded corners. With FramePainter.ENV_DWM_CORNERS set to "round" in this session, Help > About asks Windows for
		///rounded corners and a border in the outline colour when its handle is made; it is pictured, then the variable is cleared again. Kept in
		///the program only if this picture is clean (docs/port-notes/UI-3.md).</summary>
		public static void CheckUX3(Ui ui,CheckResult checkResult) {
			string valueBefore=Environment.GetEnvironmentVariable(FramePainter.ENV_DWM_CORNERS);
			try {
				Environment.SetEnvironmentVariable(FramePainter.ENV_DWM_CORNERS,"round");
				Ui.Opened opened=OpenMenu(ui,"Help","About");
				if(opened.IsOpen) {
					checkResult.AddShot(ui.Shot(opened.Hwnd,"UX3-dwm-round-about.png"),"Help > About with the rounded-corner trial on");
					checkResult.Expect(ui.Close(opened.Hwnd),"About closes");
				}
				Environment.SetEnvironmentVariable(FramePainter.ENV_DWM_CORNERS,"small");
				opened=OpenMenu(ui,"Help","About");
				if(opened.IsOpen) {
					checkResult.AddShot(ui.Shot(opened.Hwnd,"UX3-dwm-small-about.png"),"Help > About with the small rounded-corner trial on");
					checkResult.Expect(ui.Close(opened.Hwnd),"About closes");
				}
			}
			finally {
				Environment.SetEnvironmentVariable(FramePainter.ENV_DWM_CORNERS,valueBefore);
			}
			checkResult.Note("Look at UX3's pictures and at the window on screen: the corners must be clean (no square title bar corner over a rounded "
				+"window, no gap at the top). PrintWindow pictures the window's own pixels; the corner rounding is Windows' and shows best on screen.");
		}
	}
}
