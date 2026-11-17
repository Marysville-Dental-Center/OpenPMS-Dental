// Check D7 (LAUNCH-3b, docs/launch-checks-3b.md): the program's WebView2 windows on .NET 10 x64. The e-mail preview (FormEmailMessageEdit with
// a synthetic received HTML e-mail that is never saved or sent; its browser is CodeBase's ODWebView2, Microsoft.Web.WebView2.WinForms) and the
// program's WPF WebView2 control (WpfControls.UI.WebView2 around Microsoft.Web.WebView2.Wpf.WebView2) in FrmTestSplitters, the Frm of
// WpfControlsOD that holds it with a local page. Each waits until its page has loaded (the document's readyState and its text, read with
// ExecuteScriptAsync) and is pictured twice: the window with PrintWindow (PW_RENDERFULLCONTENT) and the page with the browser's own
// CapturePreviewAsync, which does not depend on how the window draws. The check proves at run time what LibraryBindingTests could not answer:
// Microsoft.Web.WebView2.Wpf's references resolve on .NET 10 x64 (ReleaseListTests' known gaps WinRT.Runtime and Microsoft.Windows.SDK.NET are
// never needed). Nothing is typed, clicked, saved or sent; no page loads anything from the network.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using OpenPmsDental;
using OpenPmsDentalBusiness;

namespace WindowChecksTests.InProc {
	public static partial class InProcChecks {
		public static void CheckD7(Ui ui,CheckResult checkResult) {
			//Every assembly name the runtime cannot resolve while the two windows run (AssemblyResolve is raised for a failed load in the default
			//context): a missing reference of a WebView2 assembly would show here.
			List<string> listUnresolved=new List<string>();
			ResolveEventHandler resolveEventHandler=(sender,args) => {
				lock(listUnresolved) {
					listUnresolved.Add(args.Name);
				}
				return null;
			};
			AppDomain.CurrentDomain.AssemblyResolve+=resolveEventHandler;
			try {
				CheckD7EmailPreview(ui,checkResult);
				CheckD7WpfWebView2(ui,checkResult);
			}
			finally {
				AppDomain.CurrentDomain.AssemblyResolve-=resolveEventHandler;
			}
			string folder=AppContext.BaseDirectory;
			List<Assembly> listLoaded=AppDomain.CurrentDomain.GetAssemblies().Where(x => (x.GetName().Name??"").StartsWith("Microsoft.Web.WebView2")).ToList();
			foreach(string name in WebView2Pages.ArrayWebView2Assemblies) {
				Assembly assembly=listLoaded.FirstOrDefault(x => x.GetName().Name==name);
				string location=assembly==null ? "" : assembly.Location;
				checkResult.Expect(assembly!=null && location.StartsWith(folder,StringComparison.OrdinalIgnoreCase),name+" is loaded from the program folder ("
					+(assembly==null ? "not loaded" : assembly.GetName().Version+", "+location)+")");
			}
			List<string> listAll;
			lock(listUnresolved) {
				listAll=listUnresolved.Distinct().ToList();
			}
			List<string> listConcern=WebView2Pages.GetUnresolvedOfConcern(listAll);
			checkResult.Expect(listConcern.Count==0,"No reference of the WebView2 assemblies went unresolved while both windows ran (unresolved: "
				+string.Join(", ",listConcern)+")");
			List<string> listGapsLoaded=AppDomain.CurrentDomain.GetAssemblies().Select(x => x.GetName().Name)
				.Where(x => WebView2Pages.ArrayKnownGapReferences.Contains(x)).ToList();
			checkResult.Note("Microsoft.Web.WebView2.Wpf's WinRT references ("+string.Join(", ",WebView2Pages.ArrayKnownGapReferences)+") loaded: "
				+(listGapsLoaded.Count==0 ? "none (never needed)" : string.Join(", ",listGapsLoaded)));
			checkResult.Note("Assembly names not resolved during D7 (any): "+(listAll.Count==0 ? "none" : string.Join(", ",listAll)));
			checkResult.ForPerson("WebView2 panes above 100 % scaling (a display setting of the session); the pages in D7-email-page.png and D7-wpf-page.png");
		}

		///<summary>The e-mail preview: a received HTML e-mail (synthetic, on patient #1, never inserted) opened in FormEmailMessageEdit as the inbox
		///opens one; its preview shows the HTML in ODWebView2 without scripts or remote content. Closed with Cancel: nothing saved or sent.</summary>
		private static void CheckD7EmailPreview(Ui ui,CheckResult checkResult) {
			int countEmails=Count("SELECT COUNT(*) FROM emailmessage");
			EmailMessage emailMessage=new EmailMessage();
			emailMessage.PatNum=FormOpenDental.PatNumCur;
			emailMessage.SentOrReceived=EmailSentOrReceived.Received;
			emailMessage.HtmlType=EmailType.RawHtml;
			emailMessage.FromAddress="sender@example.test";
			emailMessage.ToAddress="office@example.test";
			emailMessage.Subject="Window check D7 (synthetic)";
			emailMessage.BodyText=WebView2Pages.EmailHtml();
			emailMessage.MsgDateTime=DateTime.Now;
			Ui.Opened opened=ui.Open("A received HTML e-mail in FormEmailMessageEdit (the e-mail preview; not saved)",() => {
				using FormEmailMessageEdit formEmailMessageEdit=new FormEmailMessageEdit(emailMessage);
				formEmailMessageEdit.ShowDialog();
			});
			if(InspectWindow(ui,checkResult,opened,"FormEmailMessageEdit",null,close:false)) {
				IntPtr hwnd=opened.Hwnd;
				Control webViewEmail=ui.Invoke(() => FindControl(ui,hwnd,"webViewEmail",true));
				if(checkResult.Expect(webViewEmail!=null,"The e-mail preview has its browser (webViewEmail)")) {
					checkResult.Note("The preview's browser: "+ui.Invoke(() => webViewEmail.GetType().FullName+" (a "+webViewEmail.GetType().BaseType?.FullName+"), visible "
						+webViewEmail.Visible+", "+webViewEmail.Width+"x"+webViewEmail.Height));
					IntPtr hwndBrowser=ui.Invoke(() => webViewEmail.IsHandleCreated ? webViewEmail.Handle : IntPtr.Zero);
					Func<object> funcCore=() => webViewEmail.GetType().GetProperty("CoreWebView2")?.GetValue(webViewEmail);
					WaitAndPictureWebView2(ui,checkResult,hwnd,hwndBrowser,funcCore,WebView2Pages.EMAIL_MARKER,"D7-email",
						"The e-mail preview (FormEmailMessageEdit, ODWebView2)");
				}
				checkResult.Expect(ui.Close(hwnd),"The e-mail window closes (Cancel: nothing sent or saved)");
			}
			opened.Posted?.Wait();
			checkResult.Expect(Count("SELECT COUNT(*) FROM emailmessage")==countEmails,"Nothing saved ("+countEmails+" e-mails before and after)");
		}

		///<summary>The program's WPF WebView2 control (WpfControls.UI.WebView2, Microsoft.Web.WebView2.Wpf) in FrmTestSplitters, the Frm of
		///WpfControlsOD that holds it: its Load initializes the browser and shows a local page (never online).</summary>
		private static void CheckD7WpfWebView2(Ui ui,CheckResult checkResult) {
			Ui.Opened opened=ui.Open("FrmTestSplitters (the program's WPF WebView2 control with a local page)",() => new FrmTestSplitters().ShowDialog());
			if(!InspectWindow(ui,checkResult,opened,"FrmTestSplitters",null,close:false)) {
				return;
			}
			IntPtr hwnd=opened.Hwnd;
			System.Windows.FrameworkElement frm=ui.GetFrm(hwnd);
			Func<object> funcInner=() => {
				object wrapper=frm==null ? null : Ui.GetField(frm,"webView2");
				return wrapper==null ? null : Ui.GetField(wrapper,"webView2");
			};
			string typeInner=ui.Invoke(() => funcInner()?.GetType().FullName??"none");
			checkResult.Expect(typeInner=="Microsoft.Web.WebView2.Wpf.WebView2","The Frm's browser is WpfControls.UI.WebView2 around Microsoft.Web.WebView2.Wpf.WebView2 ("
				+typeInner+")");
			IntPtr hwndBrowser=ui.Invoke(() => (funcInner() as System.Windows.Interop.HwndHost)?.Handle??IntPtr.Zero);
			Func<object> funcCore=() => {
				object inner=funcInner();
				return inner?.GetType().GetProperty("CoreWebView2")?.GetValue(inner);
			};
			WaitAndPictureWebView2(ui,checkResult,hwnd,hwndBrowser,funcCore,WebView2Pages.WPF_PAGE_TEXT,"D7-wpf",
				"The WPF WebView2 control (FrmTestSplitters, Microsoft.Web.WebView2.Wpf)");
			checkResult.Expect(ui.Close(hwnd),"FrmTestSplitters closes");
			opened.Posted?.Wait();
		}

		///<summary>Waits (up to 60 s) until the browser is ready and its page has loaded with marker in its text, then pictures the window
		///(PrintWindow, PW_RENDERFULLCONTENT: fileStem.png) and the page (CoreWebView2.CapturePreviewAsync: fileStem-page.png).</summary>
		private static void WaitAndPictureWebView2(Ui ui,CheckResult checkResult,IntPtr hwnd,IntPtr hwndBrowser,Func<object> funcCore,string marker,string fileStem,
			string what)
		{
			string state="";
			string text="";
			string error="";
			bool isReady=false;
			bool isLoaded=false;
			System.Diagnostics.Stopwatch stopwatch=System.Diagnostics.Stopwatch.StartNew();
			while(stopwatch.Elapsed.TotalSeconds<60 && !isLoaded) {
				System.Threading.Tasks.Task<string> task=ui.Invoke(() => {
					object coreWebView2=funcCore();
					return coreWebView2==null ? null : (System.Threading.Tasks.Task<string>)Ui.Call(coreWebView2,"ExecuteScriptAsync",WebView2Pages.SCRIPT_STATE);
				});
				if(task!=null) {
					isReady=true;
					try {
						if(task.Wait(10000)) {
							state=WebView2Pages.DecodeScriptResult(task.Result);
							isLoaded=WebView2Pages.IsLoaded(state,marker,out text);
						}
					}
					catch(Exception ex) {
						error=ex.GetBaseException().Message;
					}
				}
				if(!isLoaded) {
					Thread.Sleep(500);
				}
			}
			string stateShort=state.Length>160 ? state.Substring(0,160)+"..." : state;
			checkResult.Expect(isReady,what+": the browser starts (CoreWebView2 ready within 60 s)");
			checkResult.Expect(isLoaded,what+": the navigation completes and the page shows '"+marker+"' (readyState|text: "+stateShort.Replace("\n"," ")
				+(error=="" ? "" : "; "+error)+")",state);
			string source=ui.Invoke(() => {
				object coreWebView2=funcCore();
				return coreWebView2==null ? "" : (string)coreWebView2.GetType().GetProperty("Source").GetValue(coreWebView2);
			});
			checkResult.Note(what+": the page's address "+(source.Length>120 ? source.Substring(0,120)+"..." : source));
			Thread.Sleep(500);
			string fileWindow=fileStem+".png";
			checkResult.AddShot(ui.Shot(hwnd,fileWindow),what+": the window (PrintWindow, PW_RENDERFULLCONTENT)");
			if(hwndBrowser!=IntPtr.Zero && File.Exists(Path.Combine(WinChkEnv.OutFolder,fileWindow))) {
				Rectangle rectangleWindow=Native.GetRect(hwnd);
				Rectangle rectangleBrowser=Native.GetRect(hwndBrowser);
				rectangleBrowser.Offset(-rectangleWindow.X,-rectangleWindow.Y);
				checkResult.Note(what+": the browser's area of "+fileWindow+" ("+rectangleBrowser+") has "+WindowChecksTests.Shot.CountColorsIn(fileWindow,rectangleBrowser)
					+" colours on a 3-pixel grid");
			}
			//The page as the browser draws it, whatever the window's printing shows of its hosted browser.
			string filePage=fileStem+"-page.png";
			MemoryStream memoryStream=new MemoryStream();
			string errorCapture="";
			System.Threading.Tasks.Task taskCapture=null;
			try {
				taskCapture=ui.Invoke(() => {
					object coreWebView2=funcCore();
					if(coreWebView2==null) {
						return null;
					}
					MethodInfo methodInfo=coreWebView2.GetType().GetMethod("CapturePreviewAsync");
					object formatPng=Enum.ToObject(methodInfo.GetParameters()[0].ParameterType,0);//CoreWebView2CapturePreviewImageFormat.Png
					return (System.Threading.Tasks.Task)methodInfo.Invoke(coreWebView2,new object[] { formatPng,memoryStream });
				});
				if(taskCapture==null) {
					errorCapture="no browser";
				}
				else if(!taskCapture.Wait(15000)) {
					errorCapture="CapturePreviewAsync did not complete within 15 s";
				}
			}
			catch(Exception ex) {
				errorCapture=ex.GetBaseException().Message;
			}
			if(!checkResult.Expect(errorCapture=="" && memoryStream.Length>0,what+": the browser pictures its page (CapturePreviewAsync, "+filePage+")",errorCapture)) {
				return;
			}
			memoryStream.Position=0;
			using Bitmap bitmap=new Bitmap(memoryStream);
			ShotInfo shotInfo=WindowChecksTests.Shot.Bitmap(bitmap,filePage);
			checkResult.ListShots.Add(filePage);
			double shareInk=GetShareInk(bitmap);
			checkResult.Expect(shotInfo.Error=="" && shareInk>0.001,what+": the page's picture shows its text ("+shotInfo+", "+(shareInk*100).ToString("0.00")
				+"% of its pixels not background)",shotInfo.ToString());
		}

		///<summary>The share of the picture's pixels (every one, so a short line of text on a white page counts) that differ from its top left
		///pixel, the page's background.</summary>
		private static double GetShareInk(Bitmap bitmap) {
			if(bitmap.Width==0 || bitmap.Height==0) {
				return 0;
			}
			int argbBackground=bitmap.GetPixel(0,0).ToArgb();
			long count=0;
			for(int x=0;x<bitmap.Width;x++) {
				for(int y=0;y<bitmap.Height;y++) {
					if(bitmap.GetPixel(x,y).ToArgb()!=argbBackground) {
						count++;
					}
				}
			}
			return (double)count/((long)bitmap.Width*bitmap.Height);
		}
	}
}
