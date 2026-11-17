// The Chart module's TextBelow strip through a real sheet layout (package TC-5, fix round 5): SheetLayoutController hides every child of ControlChart
// that is neither a sheet field nor a static control, the strip among them, on every LayoutControls(); ControlChart's layout of the strip after it
// (LayoutTextBelowAfterSheet) must show the strip again while it has text and the layout places the date slider. A real ControlChart (built, never
// shown, no database) is laid out with the internal Chart module sheet, as LayoutControls lays it out, in the same order: the hide pass, then the
// strip. Its private members are reached by name, so a rename fails here, not in an office.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental;
using OpenPmsDentalBusiness;

namespace BSHELLTests {
	[TestClass]
	public class ChartTextBelowLayoutTests {
		#region Helpers
		///<summary>Runs the action on a new STA thread, as WinForms and WPF controls need, and throws what it threw on the test thread.</summary>
		private static void RunSta(Action action) {
			ExceptionDispatchInfo exceptionDispatchInfo=null;
			Thread thread=new Thread(() => {
				try {
					action();
				}
				catch(Exception ex) {
					exceptionDispatchInfo=ExceptionDispatchInfo.Capture(ex);
				}
			});
			thread.SetApartmentState(ApartmentState.STA);
			thread.IsBackground=true;
			thread.Start();
			thread.Join();
			exceptionDispatchInfo?.Throw();
		}

		private static T GetField<T>(ControlChart controlChart,string name) {
			FieldInfo fieldInfo=typeof(ControlChart).GetField(name,BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public);
			Assert.IsNotNull(fieldInfo,"ControlChart."+name);
			return (T)fieldInfo.GetValue(controlChart);
		}

		private static object Invoke(ControlChart controlChart,string name,Type[] arrayTypes,params object[] arrayArgs) {
			MethodInfo methodInfo=typeof(ControlChart).GetMethod(name,BindingFlags.Instance|BindingFlags.NonPublic,null,arrayTypes,null);
			Assert.IsNotNull(methodInfo,"ControlChart."+name);
			try {
				return methodInfo.Invoke(controlChart,arrayArgs);
			}
			catch(TargetInvocationException ex) {
				ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
				throw;
			}
		}

		///<summary>What LayoutControls() does with a sheet, without the database that picks the user's sheet and the layout mode: the sheet layout
		///(with its hide pass), the LayoutManager's synch, then the strip. Returns the control dictionary.</summary>
		private static Dictionary<string,Control> LayOut(ControlChart controlChart,SheetLayoutController sheetLayoutController,SheetDef sheetDef) {
			Dictionary<string,Control> dictionaryStringControl=(Dictionary<string,Control>)Invoke(controlChart,"LayoutSheet_GetControlDict",
				new[] { typeof(SheetFieldLayoutMode) },SheetFieldLayoutMode.Default);
			sheetLayoutController.InitLayoutForSheetDef(sheetDef,SheetFieldLayoutMode.Default,dictionaryStringControl,isForcedRefresh:true);
			foreach(Control control in dictionaryStringControl.Values) {
				controlChart.LayoutManager.Synch(control);
			}
			Invoke(controlChart,"LayoutTextBelowAfterSheet",new[] { typeof(SheetFieldLayoutMode),typeof(Dictionary<string,Control>) },
				SheetFieldLayoutMode.Default,dictionaryStringControl);
			return dictionaryStringControl;
		}
		#endregion Helpers

		[TestMethod]
		public void TextBelowStrip_ShownAgainAfterTheSheetLayoutsHidePass_OnlyWithTheSliderInTheLayout() {
			Userod userodOld=Security.CurUser;
			Security.CurUser=new Userod { UserNum=1,UserName="Synthetic" };//InitLayoutForSheetDef records the user; nothing is saved.
			try {
				RunSta(() => {
					using ControlChart controlChart=new ControlChart();//Built, never shown: with no parent, a child's Visible is its own flag.
					controlChart.Size=new Size(1200,700);
					//Without a FormODBase the LayoutManager keeps no 96 dpi list and ignores moves; in MS layout mode (FormODBase.IsLayoutMS) it sets the
					//bounds directly, at 100%.
					controlChart.LayoutManager.IsLayoutMS=true;
					ToothChartTextBelowStrip strip=GetField<ToothChartTextBelowStrip>(controlChart,"textTextBelow");
					Panel panelSlider=GetField<Panel>(controlChart,"panelToothTrackBar");
					Control controlNotes=GetField<Control>(controlChart,"textTreatmentNotes");
					//The static controls ControlChart.InitializeOnStartup gives the controller.
					SheetLayoutController sheetLayoutController=new SheetLayoutController(controlChart.LayoutManager,controlChart,
						GetField<Control>(controlChart,"ToolBarMain"),GetField<Control>(controlChart,"tabControlImages"),GetField<Control>(controlChart,"panelImages"));
					typeof(ControlChart).GetField("_sheetLayoutController",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(controlChart,sheetLayoutController);
					SheetDef sheetDefStandard=SheetsInternal.GetSheetDef(SheetInternalType.ChartModule);
					//The sheet places the notes right under the slider: a patient without TextBelow, notes where the sheet puts them.
					LayOut(controlChart,sheetLayoutController,sheetDefStandard);
					Rectangle rectangleNotesSheet=controlNotes.Bounds;
					Size sizeMinimumNotesSheet=controlNotes.MinimumSize;
					Assert.AreEqual(rectangleNotesSheet.Height,sizeMinimumNotesSheet.Height,"The sheet layout makes the field's size the notes' minimum.");
					Assert.IsTrue(panelSlider.Visible && controlNotes.Visible,"The sheet places the slider and the notes.");
					Assert.IsFalse(strip.Visible,"No text: hidden.");
					//The patient's chart is filled (RefreshModuleScreen, FillToothChart): the strip has text and shows.
					strip.Text="Occlusal Guard";
					strip.Visible=true;
					//ModuleSelected then lays out (LayoutControls), last. The sheet's hide pass alone leaves the strip hidden: the order the fix covers.
					Dictionary<string,Control> dictionaryStringControl=(Dictionary<string,Control>)Invoke(controlChart,"LayoutSheet_GetControlDict",
						new[] { typeof(SheetFieldLayoutMode) },SheetFieldLayoutMode.Default);
					sheetLayoutController.InitLayoutForSheetDef(sheetDefStandard,SheetFieldLayoutMode.Default,dictionaryStringControl,isForcedRefresh:true);
					Assert.IsFalse(strip.Visible,"The sheet layout hides the strip, which is not a sheet field.");
					Invoke(controlChart,"LayoutTextBelowAfterSheet",new[] { typeof(SheetFieldLayoutMode),typeof(Dictionary<string,Control>) },
						SheetFieldLayoutMode.Default,dictionaryStringControl);
					Assert.IsTrue(strip.Visible,"Shown again after the layout, with no fill after it.");
					Assert.AreEqual(panelSlider.Left,strip.Left);
					Assert.AreEqual(panelSlider.Width,strip.Width);
					Assert.IsTrue(strip.Top>=panelSlider.Bottom && strip.Top<=panelSlider.Bottom+controlChart.LayoutManager.Scale(2),"Right under the slider.");
					Assert.IsTrue(controlNotes.Top>strip.Bottom,"The notes start under the strip: no overlap. Strip "+strip.Bounds+", notes "+controlNotes.Bounds+".");
					Assert.AreEqual(rectangleNotesSheet.Bottom,controlNotes.Bottom,"The notes keep their bottom edge.");
					Assert.IsTrue(controlNotes.Height<rectangleNotesSheet.Height,"Shorter than the field: its minimum height is lowered, or the notes would"
						+" grow down over the control below them.");
					//Another layout (another patient, a chart view, the layout menu): still shown, the notes still under it, no gap.
					LayOut(controlChart,sheetLayoutController,sheetDefStandard);
					Assert.IsTrue(strip.Visible,"Still shown after a second layout.");
					Assert.IsTrue(controlNotes.Top>strip.Bottom && controlNotes.Top<=strip.Bottom+controlChart.LayoutManager.Scale(2),"No gap above the notes.");
					//The text goes (a patient without TextBelow): hidden and the notes get the sheet's place back.
					strip.Text="";
					LayOut(controlChart,sheetLayoutController,sheetDefStandard);
					Assert.IsFalse(strip.Visible);
					Assert.AreEqual(rectangleNotesSheet,controlNotes.Bounds,"The sheet's place.");
					Assert.AreEqual(sizeMinimumNotesSheet,controlNotes.MinimumSize,"The field's minimum size again.");
					//A custom layout without the slider: the strip never shows, floating over other controls, and moves nothing.
					SheetDef sheetDefNoSlider=SheetsInternal.GetSheetDef(SheetInternalType.ChartModule);
					sheetDefNoSlider.SheetFieldDefs.RemoveAll(x => x.FieldName=="TrackToothProcDates");
					strip.Text="Occlusal Guard";
					LayOut(controlChart,sheetLayoutController,sheetDefNoSlider);
					Assert.IsFalse(panelSlider.Visible,"The layout has no slider.");
					Assert.IsFalse(strip.Visible,"No slider in the layout: no strip.");
					Assert.AreEqual(rectangleNotesSheet,controlNotes.Bounds,"The notes keep the sheet's place.");
					//And back to the standard layout: shown again.
					LayOut(controlChart,sheetLayoutController,sheetDefStandard);
					Assert.IsTrue(strip.Visible);
				});
			}
			finally {
				Security.CurUser=userodOld;
			}
		}
	}
}
