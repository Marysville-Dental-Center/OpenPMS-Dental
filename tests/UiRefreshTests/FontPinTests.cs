// UI-2b's pins (docs/port-notes/UI-2b.md, section 1; docs/port-notes/UI-2a.md, "the UI-2b checklist"): the printouts and office content that drew
// with an inherited font before the flip keep today's font (Microsoft Sans Serif 8.25, UiTheme.NewFontPinned), so they cannot follow the body
// font. A source check of each pinned site (it uses the pin and no inherited Font), the pin helpers, the mobile layout previews of the sheet
// designer (pinned and marked as content, while the editor's own controls follow the body font), and the WPF drawing Font (its default follows the
// body family; ForDocument keeps today's font for printouts and documents). The print guard (PrintGuardTests, PinnedFontPrintGuardTests) renders the
// pinned printouts with both font sets. No database, no window shown.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental;
using UiRefreshTests.Scan;

namespace UiRefreshTests {
	[TestClass]
	public class FontPinTests {
		///<summary>A use of a control's inherited font in code: Font or this.Font as an argument, assigned, or its family or size read.</summary>
		private static readonly Regex _regexInheritedFont=new Regex(@"(?<![\w.])(this\.)?Font\s*[,);]|(?<![\w.])(this\.)?Font\.(FontFamily|Size|SizeInPoints|Name|Style)\b");

		///<summary>The pinned sites: file, method (the constructor by its class name), and the text the pinned method must hold.</summary>
		public static readonly string[][] ArrayPinnedSites=new string[][] {
			new[] { @"OpenPmsDental\Forms\FormEquipment.cs","pd_PrintPage","UiTheme.NewFontPinned()" },
			new[] { @"OpenPmsDental\Forms\FormSheetDefEdit.cs","DrawInsurance","UiTheme.NewFontPinned()" },
			new[] { @"OpenPmsDental\Forms\FormSheetDefEdit.cs","DrawSpecial","UiTheme.NewFontPinned()" },
			new[] { @"OpenPmsDentalBusiness\UI\SignatureBox.cs","GetSigImage","UiTheme.NewFontPinned(Font)" },
			new[] { @"OpenPmsDentalGraph\Base\Legend.cs","PrintCopy","Font=UiTheme.NewFontPinned()" },
			new[] { @"CodeBase\Utilities\MsgBoxCopyPaste.cs","Print","UiTheme.NewFontPinned()" },
			new[] { @"OpenPmsDental\SheetFramework\SheetEditMobileCtrl.cs","SheetEditMobileCtrl","panelPreview.Font=UiTheme.NewFontPinned()" },
			new[] { @"WpfControlsOD\Frms\FrmChildCareMap.xaml.cs","pd_PrintPage","Font.ForDocument()" },
			new[] { @"WpfControlsOD\Frms\FrmFriendlyException.xaml.cs","pd_PrintPage","Font.ForDocument()" },
			new[] { @"WpfControlsOD\Frms\FrmMsgBoxCopyPaste.xaml.cs","pd_PrintPage","Font.ForDocument()" },
		};

		[TestCleanup]
		public void TestCleanup() {
			UiTheme.SetFontForTest(null);
		}

		[TestMethod]
		public void PinnedSites_UseThePin_AndNoInheritedFont() {
			List<string> listFailures=new List<string>();
			foreach(string[] site in ArrayPinnedSites) {
				string body=GetMethodBody(ReadSource(site[0]),site[1]);
				if(body==null) {
					listFailures.Add(site[0]+" "+site[1]+": method not found");
					continue;
				}
				if(!body.Contains(site[2])) {
					listFailures.Add(site[0]+" "+site[1]+": no "+site[2]);
				}
				if(site[0].StartsWith("WpfControlsOD")) {
					//The WPF drawing Font's default follows the body family: a printout must not take it.
					if(Regex.IsMatch(body,@"new\s+Font\s*\(\s*\)")) {
						listFailures.Add(site[0]+" "+site[1]+": new Font() takes the body family");
					}
					continue;
				}
				//The constructor of the mobile layout editor sets its own font to the body token on purpose (its chrome follows the body font).
				string bodyChecked=site[1]=="SheetEditMobileCtrl" ? body.Replace("Font=LayoutManagerForms.FontInitial;","") : body;
				foreach(string line in bodyChecked.Split('\n')) {
					string code=StripComment(line);
					if(code.Contains("NewFontPinned(Font)")) {
						code=code.Replace("NewFontPinned(Font)","NewFontPinned(x)");//the pin at the inherited font's scale reads only its size
					}
					if(_regexInheritedFont.IsMatch(code)) {
						listFailures.Add(site[0]+" "+site[1]+": inherited font in '"+line.Trim()+"'");
					}
				}
			}
			//The WPF mount layout editor: a mount item's own text is the mount's content (as MountHelper draws it); the editor's labels follow the body.
			string textMount=ReadSource(@"WpfControlsOD\Frms\FrmMountLayoutEdit.xaml.cs");
			int countTextShowing=Regex.Matches(textMount,@"Font font=Font\.ForDocument\(\);[^\r\n]*\r?\n\s*font\.Size=_listMountItems\[\w+\]\.FontSize\*_ratio;").Count;
			if(countTextShowing!=2) {
				listFailures.Add("FrmMountLayoutEdit: the two TextShowing fonts take Font.ForDocument() (found "+countTextShowing+")");
			}
			//FormMountDefEdit (the lead's decision of 2026-11-18): the editor's own label of an item without text is the body font at the zoom.
			string textMountDef=ReadSource(@"OpenPmsDental\Forms\FormMountDefEdit.cs");
			if(!textMountDef.Contains("using Font font2=new Font(UiTheme.FontBody.FontFamily,LayoutManager.ScaleFontODZoom(UiTheme.FontBody.SizeInPoints));")) {
				listFailures.Add("FormMountDefEdit: the label of an item without text follows FontBody");
			}
			Assert.AreEqual(0,listFailures.Count,string.Join(Environment.NewLine,listFailures));
		}

		[TestMethod]
		public void PinHelpers_KeepTodaysFont_AtTheInheritedScale() {
			foreach(UiFontOption uiFontOption in new[] { UiFontOption.Today,UiFontOption.UI2b,UiFontOption.Compact }) {
				UiTheme.SetFontForTest(uiFontOption);
				using(Font font=UiTheme.NewFontPinned()) {
					Assert.AreEqual("Microsoft Sans Serif",font.Name,uiFontOption.Name);
					Assert.AreEqual(8.25f,font.SizeInPoints,uiFontOption.Name);
					Assert.AreEqual(FontStyle.Regular,font.Style,uiFontOption.Name);
				}
				//The body token as a window holds it, at 100% and zoomed to 120% (LayoutManager scales a control's font by the zoom).
				foreach(float zoom in new[] { 1f,1.2f }) {
					using(Font fontInherited=new Font(UiTheme.FontBody.FontFamily,UiTheme.FontBody.SizeInPoints*zoom,FontStyle.Bold))
					using(Font font=UiTheme.NewFontPinned(fontInherited)) {
						Assert.AreEqual("Microsoft Sans Serif",font.Name,uiFontOption.Name);
						Assert.AreEqual(8.25f*zoom,font.SizeInPoints,0.001f,uiFontOption.Name+" at "+zoom);
						Assert.AreEqual(FontStyle.Bold,font.Style,uiFontOption.Name);
					}
				}
			}
			//With today's tokens the pin at an inherited font's scale is exactly that font's size (no rounding): nothing changes before the flip.
			UiTheme.SetFontForTest(UiFontOption.Today);
			using(Font fontInherited=new Font(UiTheme.FONT_FAMILY_TODAY,8.25f*1.2f))
			using(Font font=UiTheme.NewFontPinned(fontInherited)) {
				Assert.AreEqual(fontInherited.SizeInPoints,font.SizeInPoints);
			}
		}

		[TestMethod]
		public void SheetEditMobileCtrl_PreviewsKeepTodaysFont_TheEditorFollowsTheBody() {
			StaRunner.Run(() => {
				NoDatabaseGuard.Install();
				foreach(UiFontOption uiFontOption in new[] { UiFontOption.Today,UiFontOption.UI2b }) {
					UiTheme.SetFontForTest(uiFontOption);
					try {
						using(SheetEditMobileCtrl sheetEditMobileCtrl=new SheetEditMobileCtrl()) {
							Control panelPreview=sheetEditMobileCtrl.Controls.Find("panelPreview",true).Single();
							Assert.AreEqual("Microsoft Sans Serif",panelPreview.Font.Name,uiFontOption.Name);
							Assert.AreEqual(8.25f,panelPreview.Font.SizeInPoints,uiFontOption.Name);
							Assert.IsTrue(LayoutManagerForms.IsContent(panelPreview,panelPreview.Parent),"The previews are content.");
							//A preview added later inherits the pin.
							Panel panelField=new Panel();
							panelPreview.Controls.Add(panelField);
							Assert.AreSame(panelPreview.Font,panelField.Font);
							//The editor's own controls follow the body token.
							Assert.AreEqual(UiTheme.FontBody.Name,sheetEditMobileCtrl.Font.Name,uiFontOption.Name);
							Control butAddHeader=sheetEditMobileCtrl.Controls.Find("butAddHeader",true).Single();
							Assert.AreEqual(UiTheme.FontBody.Name,butAddHeader.Font.Name,uiFontOption.Name);
						}
					}
					finally {
						UiTheme.SetFontForTest(null);
					}
				}
			});
		}

		[TestMethod]
		public void WpfDrawingFont_DefaultFollowsTheBodyFamily_ForDocumentKeepsTodaysFont() {
			foreach(UiFontOption uiFontOption in new[] { UiFontOption.Today,UiFontOption.UI2b }) {
				UiTheme.SetFontForTest(uiFontOption);
				OpenPmsDental.Drawing.Font font=new OpenPmsDental.Drawing.Font();
				Assert.AreEqual(uiFontOption.FamilyName,font.Name,uiFontOption.Name);
				Assert.AreEqual(8.25,font.Size,uiFontOption.Name);
				OpenPmsDental.Drawing.Font fontDocument=OpenPmsDental.Drawing.Font.ForDocument();
				Assert.AreEqual("Microsoft Sans Serif",fontDocument.Name,uiFontOption.Name);
				Assert.AreEqual(8.25,fontDocument.Size,uiFontOption.Name);
				Assert.IsFalse(fontDocument.IsBold);
			}
		}

		[TestMethod]
		public void GetMethodBody_FindsTheBody_AndTheCheckFindsAnInheritedFont() {
			string text="class A {\r\n\t//a comment with { a brace\r\n\tprivate void Draw(Graphics g) {\r\n\t\tif(x) {\r\n\t\t\tg.DrawString(\"}\",Font,b,0,0);\r\n\t\t}\r\n\t}\r\n\tprivate void Other() {\r\n\t}\r\n}";
			string body=GetMethodBody(text,"Draw");
			Assert.IsNotNull(body);
			Assert.IsTrue(body.Contains("DrawString"));
			Assert.IsFalse(body.Contains("Other"));
			Assert.IsTrue(_regexInheritedFont.IsMatch("g.DrawString(s,Font,b,0,0);"));
			Assert.IsTrue(_regexInheritedFont.IsMatch("Font font=this.Font;"));
			Assert.IsTrue(_regexInheritedFont.IsMatch("font=new Font(Font.FontFamily,16,FontStyle.Bold);"));
			Assert.IsFalse(_regexInheritedFont.IsMatch("using Font fontTotal=UiTheme.NewFontPinned();"));
			Assert.IsFalse(_regexInheritedFont.IsMatch("g.DrawString(s,fontTotal,b,0,0);"));
			Assert.IsFalse(_regexInheritedFont.IsMatch("font=new Font(UiTheme.FONT_FAMILY_TODAY,16,FontStyle.Bold);"));
		}

		#region Helpers
		public static string ReadSource(string relative) {
			return File.ReadAllText(Path.Combine(RepoRoot(),relative));
		}

		private static string RepoRoot() {
			string dir=AppContext.BaseDirectory;
			while(dir!=null && !File.Exists(Path.Combine(dir,"OpenDental.sln"))) {
				dir=Path.GetDirectoryName(dir.TrimEnd(Path.DirectorySeparatorChar));
			}
			Assert.IsNotNull(dir,"the repository root above "+AppContext.BaseDirectory);
			return dir;
		}

		///<summary>The body of a method or constructor (from its opening brace to the matching closing brace), found by its declaration: an access
		///modifier, then the name and an opening parenthesis on the same line. Braces in strings, characters and line comments do not count. Null when
		///there is none.</summary>
		public static string GetMethodBody(string text,string name) {
			Match match=Regex.Match(text,@"^[ \t]*(public|private|protected|internal)\b[^\r\n;=]*?\b"+Regex.Escape(name)+@"\s*\(",RegexOptions.Multiline);
			if(!match.Success) {
				return null;
			}
			int start=text.IndexOf('{',match.Index);
			if(start<0) {
				return null;
			}
			int depth=0;
			for(int i=start;i<text.Length;i++) {
				char c=text[i];
				if(c=='/' && i+1<text.Length && text[i+1]=='/') {
					i=text.IndexOf('\n',i);
					if(i<0) {
						return null;
					}
					continue;
				}
				if(c=='"' || c=='\'') {
					for(i++;i<text.Length && text[i]!=c;i++) {
						if(text[i]=='\\') {
							i++;
						}
					}
					continue;
				}
				if(c=='{') {
					depth++;
				}
				else if(c=='}') {
					depth--;
					if(depth==0) {
						return text.Substring(start,i-start+1);
					}
				}
			}
			return null;
		}

		private static string StripComment(string line) {
			int index=line.IndexOf("//",StringComparison.Ordinal);
			return index<0 ? line : line.Substring(0,index);
		}
		#endregion Helpers
	}
}
