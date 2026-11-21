// The longer 25.4 wording still fits the controls that show it, at 96 dpi with the default font (the body token UiTheme.FontBody: Microsoft Sans
// Serif 8.25 until UI-2b, Segoe UI 9 since; UI-2a-1). Since UI-2b a check box the text-fit work could not widen is on a needs-owner list. The layout manager scales sizes and fonts together, so a text that fits at 96 dpi fits at every zoom.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace M2TERMTests {
	[TestClass]
	public class TerminologyLayoutTests {
		///<summary>OpenPmsDental.UI.CheckBox draws its text in Width-sqrSize-4 pixels, sqrSize being 12 at 96 dpi (CheckBox.OnPaint).</summary>
		private const int CHECKBOX_BOX_AND_GAP=12+4;

		private static Font NewDefaultFont() {
			return (Font)CodeBase.UiTheme.FontBody.Clone();//UI-2a-1: the token, so after the flip (UI-2b) this measures in Segoe UI
		}

		[TestMethod]
		public void PreferenceCheckBoxes_TextFits() {
			List<(string Designer,string ControlName)> listCheckBoxes=new List<(string,string)> {
				(TermSources.FAMILY_INSURANCE_DESIGNER,"checkInsPlanExclusionsUseUCR"),
				(TermSources.FAMILY_INSURANCE_DESIGNER,"checkEnableZeroWriteoffOnAnnualMax"),
				(TermSources.FAMILY_INSURANCE_DESIGNER,"checkEnableZeroWriteoffOnLimitations"),
				(TermSources.FAMILY_INSURANCE_DESIGNER,"checkInsDefaultShowUCRonClaims"),
				(TermSources.ACCOUNT_CLAIM_SEND_DESIGNER,"checkPpoUseUcr"),
			};
			using Bitmap bitmap=new Bitmap(1,1);
			using Graphics g=Graphics.FromImage(bitmap);
			using Font font=NewDefaultFont();
			Assert.AreEqual(96f,g.DpiX,"Measured at 96 dpi.");
			HashSet<string> setOwner=GetOwnerListedControls();
			foreach((string designer,string controlName) in listCheckBoxes) {
				DesignerControl control=DesignerControl.Read(TestPaths.ReadRepoFile(designer),controlName);
				//CheckBox.OnPaint draws with Graphics.DrawString on one line (the control is 17 pixels high).
				float widthText=g.MeasureString(control.Text,font).Width;
				int widthAvailable=control.Width-CHECKBOX_BOX_AND_GAP;
				if(widthText>widthAvailable && setOwner.Contains(controlName)) {
					//UI-2b: the text-fit work could not widen it (no free space beside it); it is on a needs-owner list with its reason, the owner decides.
					System.Console.WriteLine(controlName+" is on a needs-owner list: needs "+widthText+" pixels, has "+widthAvailable+".");
					continue;
				}
				Assert.IsTrue(widthText<=widthAvailable,controlName+": \""+control.Text+"\" needs "+widthText+" pixels, the control has "+widthAvailable+".");
			}
		}

		///<summary>UI-2b: the names of the preference pages' controls on the needs-owner lists of the text-fit work (docs/port-notes/UI-*-needs-owner.tsv;
		///the control's path's last part), for the two pages this test reads.</summary>
		private static HashSet<string> GetOwnerListedControls() {
			HashSet<string> setNames=new HashSet<string>();
			string folder=TestPaths.GetRepoFile("docs/port-notes");
			foreach(string path in System.IO.Directory.GetFiles(folder,"UI-*-needs-owner.tsv")) {
				foreach(string line in System.IO.File.ReadAllLines(path)) {
					string[] arrayFields=line.Split('\t');
					if(line.StartsWith("#") || arrayFields.Length<3
						|| (arrayFields[1]!="OpenPmsDental.UserControlFamilyInsurance" && arrayFields[1]!="OpenPmsDental.UserControlAccountClaimSend"))
					{
						continue;
					}
					setNames.Add(arrayFields[2].Substring(arrayFields[2].LastIndexOf('/')+1));
				}
			}
			return setNames;
		}

		[TestMethod]
		public void BlueBookPercentLabel_TextFits_AndDoesNotOverlap() {
			string designer=TestPaths.ReadRepoFile(TermSources.BLUE_BOOK_RULES_DESIGNER);
			DesignerControl label=DesignerControl.Read(designer,"labelUcrFeePercent");
			using Font font=NewDefaultFont();
			//A WinForms Label draws with TextRenderer (GDI).
			int widthText=TextRenderer.MeasureText(label.Text,font).Width;
			Assert.IsTrue(widthText<=label.Width,"\""+label.Text+"\" needs "+widthText+" pixels, the label has "+label.Width+".");
			//Right-aligned next to the percent box, as before; its left edge stays clear of the Allowed Fee Method list.
			System.Text.RegularExpressions.Match matchText=System.Text.RegularExpressions.Regex.Match(designer,
				@"this\.textUcrFeePercent\.Location = new System\.Drawing\.Point\((\d+),");
			Assert.IsTrue(matchText.Success);
			Assert.AreEqual(int.Parse(matchText.Groups[1].Value)-1,label.X+label.Width,"The label ends one pixel before the percent box.");
			System.Text.RegularExpressions.Match matchListLocation=System.Text.RegularExpressions.Regex.Match(designer,
				@"this\.listAllowedFeeMethod\.Location = new System\.Drawing\.Point\((\d+),");
			System.Text.RegularExpressions.Match matchListSize=System.Text.RegularExpressions.Regex.Match(designer,
				@"this\.listAllowedFeeMethod\.Size = new System\.Drawing\.Size\((\d+),");
			Assert.IsTrue(matchListLocation.Success && matchListSize.Success);
			int rightOfList=int.Parse(matchListLocation.Groups[1].Value)+int.Parse(matchListSize.Groups[1].Value);
			Assert.IsTrue(label.X>rightOfList,"The label starts at "+label.X+", inside the list that ends at "+rightOfList+".");
		}
	}
}
