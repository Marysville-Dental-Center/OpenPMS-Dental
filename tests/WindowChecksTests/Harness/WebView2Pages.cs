// The plain parts of check D7 (the WebView2 windows, docs/launch-checks-3b.md): the synthetic page the e-mail preview shows, the script that
// reads a browser's state, how its answer is read, and which assembly names the check watches. No program, browser or window here; the
// self-tests (WebView2PagesTests) run them without the opt-in.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace WindowChecksTests {
	public static class WebView2Pages {
		///<summary>The words the e-mail preview's page shows, which D7 waits for in the page's text.</summary>
		public const string EMAIL_MARKER="Window check D7: a synthetic received e-mail";
		///<summary>The text FrmTestSplitters' local page shows (WpfControlsOD/Frms/FrmTestSplitters.xaml.cs), which D7 waits for.</summary>
		public const string WPF_PAGE_TEXT="Splitter test page";
		///<summary>The script D7 runs in a page (CoreWebView2.ExecuteScriptAsync, which runs even where the page's own scripts are off): the
		///document's readyState and the text of its body, separated by '|'.</summary>
		public const string SCRIPT_STATE="document.readyState+'|'+(document.body ? document.body.innerText : '')";
		///<summary>The program's three WebView2 assemblies; Microsoft.Web.WebView2.Wpf is the one LibraryBindingTests could not answer on .NET 10
		///(its WinRT references, ReleaseListTests' known gaps).</summary>
		public static readonly string[] ArrayWebView2Assemblies=new[] { "Microsoft.Web.WebView2.Core","Microsoft.Web.WebView2.WinForms","Microsoft.Web.WebView2.Wpf" };
		///<summary>The references of Microsoft.Web.WebView2.Wpf that the shared framework does not answer (ReleaseListTests' known gaps).</summary>
		public static readonly string[] ArrayKnownGapReferences=new[] { "WinRT.Runtime","Microsoft.Windows.SDK.NET" };

		///<summary>The HTML body of the synthetic received e-mail: a heading with the marker, a paragraph and a small table. Nothing it names loads
		///from anywhere (no script, image, link, style sheet or address), so the preview shows it without the network.</summary>
		public static string EmailHtml() {
			return "<html><head><meta charset=\"utf-8\"><title>Window check D7</title></head><body style=\"font-family:sans-serif\">"
				+"<h2>"+EMAIL_MARKER+"</h2>"
				+"<p>This message was made by the window checks for the e-mail preview. It was never sent or received.</p>"
				+"<table border=\"1\" cellpadding=\"4\"><tr><th>Item</th><th>Value</th></tr><tr><td>Runtime</td><td>.NET 10 x64</td></tr>"
				+"<tr><td>Browser</td><td>WebView2</td></tr></table>"
				+"</body></html>";
		}

		///<summary>The string ExecuteScriptAsync's JSON answer holds ("null" and an empty answer: empty).</summary>
		public static string DecodeScriptResult(string json) {
			if(string.IsNullOrWhiteSpace(json) || json.Trim()=="null") {
				return "";
			}
			try {
				return JsonSerializer.Deserialize<string>(json)??"";
			}
			catch(JsonException) {
				return "";
			}
		}

		///<summary>Whether SCRIPT_STATE's decoded answer says the document has loaded and its text holds marker; text is the body's text.</summary>
		public static bool IsLoaded(string state,string marker,out string text) {
			text="";
			if(string.IsNullOrEmpty(state)) {
				return false;
			}
			int index=state.IndexOf('|');
			string readyState=index<0 ? state : state.Substring(0,index);
			text=index<0 ? "" : state.Substring(index+1);
			return readyState=="complete" && text.Contains(marker);
		}

		///<summary>The names among the assembly names the runtime could not resolve during the check that matter to D7: a WebView2 assembly or one
		///of the known-gap references (satellite resource assemblies left out).</summary>
		public static List<string> GetUnresolvedOfConcern(IEnumerable<string> listNames) {
			List<string> listConcern=new List<string>();
			foreach(string fullName in listNames) {
				string name=(fullName??"").Split(',')[0].Trim();
				if(name.EndsWith(".resources",StringComparison.OrdinalIgnoreCase)) {
					continue;
				}
				if(name.StartsWith("Microsoft.Web.WebView2",StringComparison.OrdinalIgnoreCase)
					|| ArrayKnownGapReferences.Any(x => x.Equals(name,StringComparison.OrdinalIgnoreCase)))
				{
					listConcern.Add(fullName);
				}
			}
			return listConcern.Distinct().ToList();
		}
	}
}
