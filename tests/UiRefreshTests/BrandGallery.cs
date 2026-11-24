// The gallery of UI-BRAND (docs/port-notes/UI-BRAND.md): the windows whose visible text changed when the program stopped calling itself
// OD or Open Dental, before and after, at 100%.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UiRefreshTests {
	///<summary>The gallery's preparation of a built window for the UI-BRAND before pictures (FitGallery, after its state).</summary>
	public static class BrandGallery {
		///<summary>Set to 1: the before pictures. Every control text gets the words it had before UI-BRAND back.</summary>
		public const string VARIABLE_BEFORE="OPMS_UI_BRAND_GALLERY_BEFORE";
		///<summary>The windows whose visible text UI-BRAND changed.</summary>
		public static readonly string[] ArrayWindows=new string[] {
			"OpenPmsDental.FormPath",
			"OpenPmsDental.FormXChargeReconcile",
			"OpenPmsDental.Reporting.Allocators.MyAllocator1.FormInstallAllocator_Provider",
			"OpenPmsDental.Reporting.Allocators.MyAllocator1.FormWarnToCloseComputers",
		};
		///<summary>Each text as UI-BRAND wrote it, and as it was before.</summary>
		private static readonly string[][] _arrayTextsAfterBefore=new string[][] {
			new string[] { "View X-Charge transactions not paid in Open PMS.","View X-Charge transactions with no payment in OD." },
			new string[] { "View Open PMS payments not found in X-Charge.","View payments in OD with no X-Charge transaction." },
			new string[] { "Close Other Open PMS Programs Now!!","Close Other OD Programs Now!!" },
			new string[] { "close all running Open PMS programs","close all running opendental programs" },
			new string[] { "provide you Open PMS program","provide you Opendental program" },
			new string[] { @"\\SERVER\OpenPmsExport\",@"\\SERVER\OpenDentalExport\" },
			new string[] { @"C:\OpenPmsExport\",@"C:\OpenDentalExport\" },
			new string[] { @"\\server\OpenPmsLetters\",@"\\server\OpenDentalLetters\" },
			new string[] { @"C:\OpenPmsLetters\",@"C:\OpenDentalLetters\" },
		};

		public static void Prepare(Control root) {
			if(Environment.GetEnvironmentVariable(VARIABLE_BEFORE)!="1") {
				return;
			}
			foreach(Control control in FinalSweep.Walk(root).ToList()) {
				string text=control.Text??"";
				string textBefore=text;
				for(int i=0;i<_arrayTextsAfterBefore.Length;i++) {
					textBefore=textBefore.Replace(_arrayTextsAfterBefore[i][0],_arrayTextsAfterBefore[i][1]);
				}
				if(textBefore!=text) {
					control.Text=textBefore;
				}
			}
		}
	}

	[TestClass]
	public class BrandGalleryTests {
		///<summary>Opt-in: OPMS_UI_BRAND_GALLERY names the folder of the G-UI3-uibrand pictures. Draws each window of BrandGallery.ArrayWindows
		///through FitGallery in the program's font at 100%, before (BrandGallery.VARIABLE_BEFORE) and after, into before\windows-100 and
		///after\windows-100 (New-UiGallery.ps1's -Before and -After).</summary>
		[TestMethod]
		public void Gallery_UiBrand_BeforeAndAfter() {
			string folder=Environment.GetEnvironmentVariable("OPMS_UI_BRAND_GALLERY");
			if(string.IsNullOrEmpty(folder)) {
				Assert.Inconclusive("Opt-in: OPMS_UI_BRAND_GALLERY names the folder for the UI-BRAND gallery pictures.");
			}
			string[] arrayVariables=new string[] { FitGallery.VARIABLE_FOLDER,FitGallery.VARIABLE_WINDOWS,FitGallery.VARIABLE_FONT,FitGallery.VARIABLE_SCALE,
				BrandGallery.VARIABLE_BEFORE };
			Dictionary<string,string> dictBefore=arrayVariables.ToDictionary(x => x,x => Environment.GetEnvironmentVariable(x));
			try {
				foreach(bool isBefore in new[] { true,false }) {
					string folderSet=Path.Combine(folder,isBefore ? "before" : "after","windows-100");
					Directory.CreateDirectory(folderSet);
					Environment.SetEnvironmentVariable(FitGallery.VARIABLE_FOLDER,folderSet);
					Environment.SetEnvironmentVariable(FitGallery.VARIABLE_WINDOWS,string.Join(",",BrandGallery.ArrayWindows));
					Environment.SetEnvironmentVariable(FitGallery.VARIABLE_FONT,"program");
					Environment.SetEnvironmentVariable(FitGallery.VARIABLE_SCALE,null);
					Environment.SetEnvironmentVariable(BrandGallery.VARIABLE_BEFORE,isBefore ? "1" : null);
					new FitGallery().Gallery_SliceWindows_InTheNewFont();
					string[] arrayFailed=File.ReadAllLines(Path.Combine(folderSet,"gallery-failed.txt"));
					Assert.AreEqual(0,arrayFailed.Length,folderSet+": "+string.Join("; ",arrayFailed));
				}
			}
			finally {
				foreach(KeyValuePair<string,string> pair in dictBefore) {
					Environment.SetEnvironmentVariable(pair.Key,pair.Value);
				}
			}
		}
	}
}
