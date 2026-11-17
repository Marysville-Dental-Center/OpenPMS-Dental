// Self-tests of check D7 (the WebView2 windows, LAUNCH-3b, docs/launch-checks-3b.md), without the opt-in and without starting anything: the
// plain parts (WebView2Pages: the synthetic page, how a browser's answer is read, which unresolved names matter) and what the check's script
// may and may not do, read from its source. D7 itself needs the desktop and the opt-in; the lead runs it at a connected session.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WindowChecksTests {
	[TestClass]
	public class WebView2PagesTests {
		private static string ReadRepoFile(params string[] arrayParts) {
			return File.ReadAllText(Path.Combine(new[] { WinChkEnv.RepoRoot }.Concat(arrayParts).ToArray()));
		}

		[TestMethod]
		public void DecodeScriptResult_ReadsTheJsonStringOfExecuteScriptAsync() {
			Assert.AreEqual("complete|Line 1\nLine \"2\"",WebView2Pages.DecodeScriptResult("\"complete|Line 1\\nLine \\\"2\\\"\""));
			Assert.AreEqual("",WebView2Pages.DecodeScriptResult("null"));
			Assert.AreEqual("",WebView2Pages.DecodeScriptResult(""));
			Assert.AreEqual("",WebView2Pages.DecodeScriptResult(null));
			Assert.AreEqual("",WebView2Pages.DecodeScriptResult("{not json"),"A broken answer reads as nothing, so the check waits on.");
		}

		[TestMethod]
		public void IsLoaded_NeedsReadyStateCompleteAndTheMarkerInTheText() {
			Assert.IsTrue(WebView2Pages.IsLoaded("complete|Before\n"+WebView2Pages.EMAIL_MARKER+"\nAfter",WebView2Pages.EMAIL_MARKER,out string text));
			StringAssert.Contains(text,"After");
			Assert.IsFalse(WebView2Pages.IsLoaded("interactive|"+WebView2Pages.EMAIL_MARKER,WebView2Pages.EMAIL_MARKER,out _),"Not loaded yet.");
			Assert.IsFalse(WebView2Pages.IsLoaded("complete|about:blank",WebView2Pages.EMAIL_MARKER,out _),"Another page (the blank one before navigation).");
			Assert.IsFalse(WebView2Pages.IsLoaded("complete",WebView2Pages.EMAIL_MARKER,out _));
			Assert.IsFalse(WebView2Pages.IsLoaded("",WebView2Pages.EMAIL_MARKER,out _));
		}

		///<summary>The synthetic e-mail loads nothing from anywhere: no script, image, link, style sheet or address, so the preview (which blocks a
		///received e-mail's remote content anyway) shows it without the network, and no host is named.</summary>
		[TestMethod]
		public void EmailHtml_IsSyntheticAndLocal() {
			string html=WebView2Pages.EmailHtml();
			StringAssert.Contains(html,"<h2>"+WebView2Pages.EMAIL_MARKER+"</h2>");
			foreach(string forbidden in new[] { "<script","src=","href=","http:","https:","url(","@import","<img","<link","<iframe","//" }) {
				Assert.IsFalse(html.IndexOf(forbidden,StringComparison.OrdinalIgnoreCase)>=0,"The synthetic e-mail has no "+forbidden);
			}
			Assert.IsTrue(html.All(x => x<128),"ASCII only.");
		}

		[TestMethod]
		public void GetUnresolvedOfConcern_KeepsTheWebView2AssembliesAndTheKnownGaps_NotResources() {
			List<string> listConcern=WebView2Pages.GetUnresolvedOfConcern(new[] {
				"WinRT.Runtime, Version=2.0.0.0, Culture=neutral, PublicKeyToken=99ea127f02d97709",
				"Microsoft.Windows.SDK.NET, Version=10.0.17763.0",
				"Microsoft.Web.WebView2.Wpf.resources, Version=1.0.0.0, Culture=de",
				"OpenPmsDental.resources, Version=24.3.63.0, Culture=en-US",
				"Microsoft.Web.WebView2.Core, Version=1.0.4314.35",
				"SomethingElse, Version=1.0.0.0",
				"WinRT.Runtime, Version=2.0.0.0, Culture=neutral, PublicKeyToken=99ea127f02d97709",
			});
			CollectionAssert.AreEqual(new[] {
				"WinRT.Runtime, Version=2.0.0.0, Culture=neutral, PublicKeyToken=99ea127f02d97709",
				"Microsoft.Windows.SDK.NET, Version=10.0.17763.0",
				"Microsoft.Web.WebView2.Core, Version=1.0.4314.35",
			},listConcern);
			//The known gaps are ReleaseListTests' entries for Microsoft.Web.WebView2.Wpf.
			string releaseListTests=ReadRepoFile("tests","LibraryBindingTests","ReleaseListTests.cs");
			foreach(string name in WebView2Pages.ArrayKnownGapReferences) {
				StringAssert.Contains(releaseListTests,"{ \""+name+"\",\"Microsoft.Web.WebView2.Wpf of lib_manual");
			}
		}

		///<summary>What D7 opens is what the program has: the e-mail preview's browser is webViewEmail (ODWebView2), and FrmTestSplitters holds the
		///program's WPF WebView2 control (WpfControls.UI.WebView2 around Microsoft.Web.WebView2.Wpf's) and shows the local page D7 waits for.
		///</summary>
		[TestMethod]
		public void TheWindowsD7Opens_HaveTheBrowsersItReads() {
			StringAssert.Contains(ReadRepoFile("OpenPmsDental","User Controls","EmailPreviewControl.Designer.cs"),"CodeBase.Controls.ODWebView2 webViewEmail;");
			StringAssert.Contains(ReadRepoFile("WpfControlsOD","Frms","FrmTestSplitters.xaml"),"<ui:WebView2 x:Name=\"webView2\"");
			string splitters=ReadRepoFile("WpfControlsOD","Frms","FrmTestSplitters.xaml.cs");
			StringAssert.Contains(splitters,"webView2.NavigateToString(CodeBase.HtmlPage.Build(\"<p>"+WebView2Pages.WPF_PAGE_TEXT+"</p>\"");
			string control=ReadRepoFile("WpfControlsOD","UI","Controls","WebView2.xaml");
			StringAssert.Contains(control,"xmlns:wv2=\"clr-namespace:Microsoft.Web.WebView2.Wpf;assembly=Microsoft.Web.WebView2.Wpf\"");
			StringAssert.Contains(control,"<wv2:WebView2 x:Name=\"webView2\"");
		}

		///<summary>D7's script sends no input, saves and sends nothing (the e-mail is never inserted and no send is called), and is pictured with
		///PrintWindow and the browser's own CapturePreviewAsync; its test is behind the opt-in of group D.</summary>
		[TestMethod]
		public void D7_SendsNoInput_SavesAndSendsNothing_AndIsOptIn() {
			string script=ReadRepoFile("tests","WindowChecksTests","InProc","InProcChecks.D7.cs");
			string code=Regex.Replace(script,@"(?m)//.*$","");
			foreach(string forbidden in new[] { "SendInput","keybd_event","mouse_event","SendKeys","EmailMessages.Insert","EmailMessages.Update","butSend","SendEmail",
				"PerformClick","Navigate(\"http" })
			{
				Assert.IsFalse(code.Contains(forbidden),"D7 never does "+forbidden);
			}
			StringAssert.Contains(code,"emailMessage.SentOrReceived=EmailSentOrReceived.Received;");
			StringAssert.Contains(code,"ui.Shot(hwnd,fileWindow)");
			StringAssert.Contains(code,"GetMethod(\"CapturePreviewAsync\")");
			StringAssert.Contains(code,"Count(\"SELECT COUNT(*) FROM emailmessage\")==countEmails");
			string tests=ReadRepoFile("tests","WindowChecksTests","DETests.cs");
			Match match=Regex.Match(tests,@"public void D7_WebView2Windows\(\) \{\s*WinChkFixture\.Begin\(""D""\);");
			Assert.IsTrue(match.Success,"D7 is behind the opt-in of group D.");
		}
	}
}
