// The eForm fill surface keeps today's look (package UI-4a, docs/port-notes/UI-4a.md; docs/specs/ui-refresh-plan.md sections 1 and 7): eForms
// are patient-facing office content, like sheets, so CtrlEFormFill keeps pinned copies of the looks UI-4a changes (WpfControls.UI.ContentLook).
// A synthetic eForm (no database: two check boxes, a radio button group and a text field;
// the page buttons and the page label of the fill control's own XAML) is laid out without a window, rendered by
// WPF's software renderer and compared with the picture recorded on main before UI-4a changed any control (Data/eform-fill-before.png), with
// the tolerance of ChartImageCompare. OPMS_UI_EFORM_BEFORE_WRITE=1 records the picture again (only on the code before a look change).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental;
using OpenPmsDentalBusiness;
using ToothChartImageTests;
using UiRefreshTests.Scan;

namespace UiRefreshTests {
	[TestClass]
	public class EFormLookTests {
		public const string PATH_BEFORE=@"tests\UiRefreshTests\Data\eform-fill-before.png";
		public const int WIDTH=420;
		public const int HEIGHT=300;

		[TestMethod]
		public void EFormFill_SyntheticForm_LooksAsBefore() {
			string inconclusive=null;
			Bitmap bitmapActual=null;
			IconRendererTests.RunSta(() => {
				if(!ControlPictures.RendersWpf()) {
					inconclusive="WPF draws nothing in this session: the eForm picture waits for a session WPF renders in.";
					return;
				}
				FillPrefs();
				try {
					bitmapActual=RenderFill(enabled:true);
				}
				finally {
					Prefs.ClearCache();
				}
			});
			if(inconclusive!=null) {
				Assert.Inconclusive(inconclusive);
			}
			using(bitmapActual) {
				string pathBefore=Path.Combine(RepoPaths.Root(),PATH_BEFORE);
				if(Environment.GetEnvironmentVariable("OPMS_UI_EFORM_BEFORE_WRITE")=="1") {
					PngWriter.Save(bitmapActual,pathBefore);
					Assert.Inconclusive("Recorded "+pathBefore+".");
				}
				Assert.IsTrue(File.Exists(pathBefore),pathBefore+" is missing.");
				using(Bitmap bitmapLoaded=new Bitmap(pathBefore))
				using(Bitmap bitmapBefore=new Bitmap(bitmapLoaded)) {
					ChartImageCompareResult result=ChartImageCompare.Compare(bitmapBefore,bitmapActual);
					if(!result.IsPass) {
						string folder=ControlPictures.GetOutFolder("eform");
						ChartImageCompare.WriteFailureImages(folder,"eform-fill",bitmapBefore,bitmapActual);
						Assert.Fail("The eForm fill surface changed: "+result+" Pictures in "+folder+".");
					}
				}
			}
		}

		///<summary>The preferences the fill control reads, in the cache without a database (this process refuses every connection).</summary>
		public static void FillPrefs() {
			List<Pref> listPrefs=new List<Pref>();
			listPrefs.Add(new Pref { PrefNum=1,PrefName=PrefName.EformsSpaceToRightEachField.ToString(),ValueString="10",Comments="" });
			listPrefs.Add(new Pref { PrefNum=2,PrefName=PrefName.EformsSpaceBelowEachField.ToString(),ValueString="10",Comments="" });
			Prefs.FillCacheFromTable(OpenPmsDentalBusiness.Crud.PrefCrud.ListToTable(listPrefs));
		}

		///<summary>The synthetic eForm in CtrlEFormFill, laid out without a window as EFormPrintTests does (FrmEFormFillEdit's Load and SetCtrlWidth),
		///rendered at 96 DPI on white. STA thread only.</summary>
		public static Bitmap RenderFill(bool enabled) {
			CtrlEFormFill ctrlEFormFill=new CtrlEFormFill();
			ctrlEFormFill.ListEFormFields=GetSyntheticFields();
			ctrlEFormFill.ShowLabelsBold=false;
			ctrlEFormFill.SpaceBelowEachField=10;
			ctrlEFormFill.SpaceToRightEachField=10;
			ctrlEFormFill.Zoom=1;
			ctrlEFormFill.Width=WIDTH;
			ctrlEFormFill.Height=HEIGHT;
			ctrlEFormFill.IsEnabled=enabled;
			ctrlEFormFill.Measure(new System.Windows.Size(WIDTH,HEIGHT));
			ctrlEFormFill.Arrange(new System.Windows.Rect(new System.Windows.Size(WIDTH,HEIGHT)));
			ctrlEFormFill.UpdateLayout();
			ctrlEFormFill.RefreshLayout();
			ctrlEFormFill.UpdateLayout();
			ctrlEFormFill.RefreshLayout();
			ctrlEFormFill.UpdateLayout();
			return ControlPictures.RenderWpf(ctrlEFormFill,WIDTH,HEIGHT,1);
		}

		///<summary>A checked and an unchecked check box, a radio button group with a choice and a text field. Nothing reads the database.</summary>
		public static List<EFormField> GetSyntheticFields() {
			List<EFormField> listEFormFields=new List<EFormField>();
			listEFormFields.Add(MakeField(EnumEFormFieldType.CheckBox,"Smoker","X",""));
			listEFormFields.Add(MakeField(EnumEFormFieldType.CheckBox,"Allergic to latex","",""));
			listEFormFields.Add(MakeField(EnumEFormFieldType.RadioButtons,"Preferred contact","Phone","Phone|Email|Text"));
			listEFormFields.Add(MakeField(EnumEFormFieldType.TextField,"Emergency contact","Sample Person",""));
			for(int i=0;i<listEFormFields.Count;i++) {
				listEFormFields[i].ItemOrder=i;
			}
			return listEFormFields;
		}

		private static EFormField MakeField(EnumEFormFieldType fieldType,string valueLabel,string valueString,string pickListVis) {
			EFormField eFormField=new EFormField();
			eFormField.FieldType=fieldType;
			eFormField.ValueLabel=valueLabel;
			eFormField.ValueString=valueString;
			eFormField.PickListVis=pickListVis;
			eFormField.PickListDb=pickListVis;
			eFormField.DbLink="";
			eFormField.ConditionalParent="";
			eFormField.ConditionalValue="";
			eFormField.ReportableName="";
			eFormField.ValueLabelEnglish=valueLabel;
			eFormField.PickListVisEnglish=pickListVis;
			eFormField.FontScale=100;
			eFormField.Width=300;
			eFormField.Page=1;
			return eFormField;
		}
	}
}
