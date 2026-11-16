// Groups I and J's in-process scripts (the parts this harness reaches): the tooth chart (the filling entered as Enter Treatment does it, the
// refill, Show Big, Save to Images, File > Graphics > 3D chart) and EHR-0 and EHR-1 (Show Features with its confirmation and restart messages, the
// SNOMED CT window, the e-mail Send drop-down and the Direct certificate window's import buttons). The rest of I and J is in InProcChecks.I.cs
// and InProcChecks.J.cs.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using OpenPmsDental;
using OpenPmsDentalBusiness;

namespace WindowChecksTests.InProc {
	public static partial class InProcChecks {
		#region I
		///<summary>I's prep: Chart > tooth 30 > surface O > TreatPlan > Type Proc Code D2391 > OK, through the chart's own handlers.</summary>
		public static void CheckIPREP(Ui ui,CheckResult checkResult) {
			GoToModule(ui,EnumModuleType.Chart);
			Control controlChart=ModuleControl("controlChart");
			int countBefore=Count("SELECT COUNT(*) FROM procedurelog WHERE PatNum=1 AND ToothNum='30' AND ProcStatus=1");
			long procNumBefore=MaxKey("procedurelog","ProcNum");
			string surf=ui.Invoke(() => {
				object relay=Ui.GetField(controlChart,"_toothChartRelay");
				Ui.Call(relay,"SetSelected","30",true);
				Ui.Call(controlChart,"toothChart_ToothSelectionsChanged",Ui.GetField(controlChart,"toothChartWrapper"));
				Ui.Call(controlChart,"butOI_Click",controlChart,EventArgs.Empty);
				return ((Control)Ui.GetField(controlChart,"textSurf")).Text;
			});
			checkResult.Expect(surf=="O","Tooth 30 selected and surface O clicked: the surface box shows "+surf,surf);
			Ui.Posted posted=ui.Post("TreatPlan > Type Proc Code D2391 > OK",() => {
				((RadioButton)Ui.GetField(controlChart,"radioEntryTP")).Checked=true;
				((Control)Ui.GetField(controlChart,"textProcCode")).Text="D2391";
				Ui.Call(controlChart,"butOK_Click",controlChart,EventArgs.Empty);
			});
			System.Threading.Thread.Sleep(2000);
			List<IntPtr> listWindows=ui.GetWindows();
			if(listWindows.Count>0) {
				checkResult.Fail("A window opened instead of adding the filling directly: "+string.Join("; ",listWindows.Select(x => ui.DescribeWindow(x))));
				ui.CloseStrayWindows(checkResult);
			}
			posted.Wait();
			checkResult.Expect(Count("SELECT COUNT(*) FROM procedurelog WHERE PatNum=1 AND ToothNum='30' AND ProcStatus=1")==countBefore+1,
				"The D2391 on 30 O is added (treatment planned) without the Procedure Edit window");
			//The row IUNDO deletes: the one procedure this prep added (none when it added none, or more than one).
			long procNum=AddedKey("procedurelog","ProcNum",procNumBefore,"PatNum=1 AND ToothNum='30' AND ProcStatus=1");
			SetKey(checkResult,"IPREP.ProcNum",procNum);
			checkResult.Note("The prep's procedure: ProcNum "+(procNum==0 ? "none found above "+procNumBefore : procNum.ToString())+" (IUNDO deletes only this one)");
			//Does the chart draw with the WPF chart? Since TC-8 it is every computer's 3D chart, and its pictures (GetBitmap, every host's) are rendered
			//in software, which works in any session, a locked one included, so the group no longer moves the copy to Simple 2D for a locked screen
			//(until TC-9a the DirectX chart had no device there). Any other engine, a fallback logged or GetBitmap throwing is the program's failure.
			Control toothChartWrapper=(Control)Ui.GetField(controlChart,"toothChartWrapper");
			string whyNot=WhyNotWpfChart(ui,toothChartWrapper);
			if(checkResult.Expect(whyNot=="","The chart draws with the WPF chart (its picture renders in this session: "+Native.GetSessionState()+")",whyNot)) {
				checkResult.Note("The WPF chart draws in this session (the chart's picture, GetBitmap, renders)");
			}
		}

		///<summary>B5's on-screen test: the chart's own window printed (PrintWindow) is drawn only while the screen shows it: WPF does not draw its
		///windows on screen while the session is locked. A window a fallback engine drew does not count, so the chart must also draw with the WPF chart
		///(WhyNotWpfChart). Saves the picture (listed, not failed when blank) and returns "" when the window shows the chart, otherwise why not.</summary>
		private static string ShotChartWindow(Ui ui,CheckResult checkResult,Control toothChartWrapper,string fileName) {
			IntPtr hwndChart=ui.Invoke(() => toothChartWrapper.Handle);
			ShotInfo shotInfo=ui.Shot(hwndChart,fileName);
			if(shotInfo.Error=="") {
				checkResult.ListShots.Add(shotInfo.FileName);
			}
			string whyNot=shotInfo.IsDrawn ? WhyNotWpfChart(ui,toothChartWrapper)
				: "its window printed blank, as the screen's WPF content does while the session is locked ("+Native.GetSessionState()+")";
			checkResult.Note("The chart's own window printed (PrintWindow, B5's on-screen test): "+shotInfo+(whyNot=="" ? ", drawn by the WPF chart" : ", not counted: "+whyNot));
			return whyNot;
		}

		///<summary>"" while the chart draws with the WPF chart: its engine is Wpf3D, no fallback is logged in this session, and its picture (GetBitmap)
		///renders; otherwise why not. The WPF chart's pictures are rendered in software (RenderTargetBitmap on the chart's own render thread), so a
		///locked session draws them too: GetBitmap throwing is the program's failure in any session. Run on any thread (it calls the window's thread).
		///Replaces WhyNotDirectX (until TC-9a: the DirectX chart, which had no device while the session was locked).</summary>
		public static string WhyNotWpfChart(Ui ui,Control toothChartWrapper) {
			return ui.Invoke(() => {
				string engine=toothChartWrapper.GetType().GetProperty("Engine").GetValue(toothChartWrapper).ToString();
				if(engine!="Wpf3D") {
					return "the chart draws with "+engine+", not the WPF chart";
				}
				List<string> listFallbacks=(List<string>)Ui.CallStatic(toothChartWrapper.GetType(),"GetFallbackMessages");
				if(listFallbacks.Count>0) {
					return "a tooth chart fallback was logged in this session ("+listFallbacks[listFallbacks.Count-1]+")";
				}
				try {
					using Bitmap bitmap=(Bitmap)Ui.Call(toothChartWrapper,"GetBitmap");
				}
				catch(Exception ex) {
					Exception exBase=ex.GetBaseException();
					return "the chart's picture (GetBitmap) threw "+exBase.GetType().Name+": "+exBase.Message+" ("+Native.GetSessionState()+")";
				}
				return "";
			});
		}

		///<summary>File > Graphics > "3D chart" or "Simple 2D chart" > Save. Returns "" when the window closed, or the message Save showed (answered).
		///</summary>
		private static string SetGraphics(Ui ui,CheckResult checkResult,bool isSimple2D) {
			Ui.Opened openedGraphics=OpenMenu(ui,"File","Graphics");
			if(!InspectWindow(ui,checkResult,openedGraphics,"FormGraphics",null,close:false)) {
				return "File > Graphics did not open";
			}
			IntPtr hwndGraphics=openedGraphics.Hwnd;
			ui.Invoke(() => ((RadioButton)FindControl(ui,hwndGraphics,isSimple2D ? "radioSimpleChart" : "radio3DChart",true)).Checked=true);
			return SaveGraphics(ui,checkResult,openedGraphics);
		}

		///<summary>File > Graphics' Save, with the choices already made in the open window. Returns "" when the window closed, or the message Save showed
		///(answered).</summary>
		private static string SaveGraphics(Ui ui,CheckResult checkResult,Ui.Opened openedGraphics) {
			IntPtr hwndGraphics=openedGraphics.Hwnd;
			List<IntPtr> listBefore=ui.GetWindows();
			ui.Post("Save",() => ((System.Windows.Forms.Button)FindControl(ui,hwndGraphics,"butSave",true)).PerformClick());
			string message="";
			System.Diagnostics.Stopwatch stopwatch=System.Diagnostics.Stopwatch.StartNew();
			while(Native.IsWindow(hwndGraphics) && Native.IsWindowVisible(hwndGraphics) && stopwatch.Elapsed.TotalSeconds<30) {
				IntPtr hwndMessage=ui.GetWindows().FirstOrDefault(x => !listBefore.Contains(x) && Native.IsDialogClass(x));
				if(hwndMessage!=IntPtr.Zero) {
					message=Native.GetDialogMessage(hwndMessage).Trim();
					ui.Close(hwndMessage);
					break;
				}
				System.Threading.Thread.Sleep(200);
			}
			if(message!="" && Native.IsWindow(hwndGraphics)) {
				ui.Close(hwndGraphics);
			}
			ui.WaitGone(hwndGraphics,15000);
			openedGraphics.Posted.Wait();
			ui.CloseStrayWindows(checkResult);
			return message;
		}

		///<summary>B5's on-screen test on its own, for I11 (whose program runs out of process): the Chart's window printed in this session.</summary>
		public static void CheckISCREEN(Ui ui,CheckResult checkResult) {
			GoToModule(ui,EnumModuleType.Chart);
			Control toothChartWrapper=(Control)Ui.GetField(ModuleControl("controlChart"),"toothChartWrapper");
			System.Threading.Thread.Sleep(2000);
			string whyNot=ShotChartWindow(ui,checkResult,toothChartWrapper,"I11-screen.png");
			SetKey(checkResult,"ISCREEN.IsDrawn",whyNot=="" ? 1 : 0);
			checkResult.Note("ISCREEN: "+(whyNot=="" ? "the WPF chart's window is drawn" : whyNot));
		}

		public static void CheckI1(Ui ui,CheckResult checkResult) {
			GoToModule(ui,EnumModuleType.Account);
			GoToModule(ui,EnumModuleType.Chart);
			Control controlChart=ModuleControl("controlChart");
			Control toothChartWrapper=(Control)Ui.GetField(controlChart,"toothChartWrapper");
			ShotInfo shotInfo=ui.Invoke(() => {
				using Bitmap bitmap=(Bitmap)Ui.Call(toothChartWrapper,"GetBitmap");
				return WindowChecksTests.Shot.Bitmap(bitmap,"I1-chart.png");
			});
			checkResult.AddShot(shotInfo,"The chart after leaving the Chart and coming back (its WPF picture)");
			checkResult.ForPerson("the filling on 30 in I1-chart.png, and the date slider below the chart (moving it is mouse work)");
			Ui.Opened opened=ui.Open("Tooth Chart > Show Big",() => Ui.Call(controlChart,"menuItemChartBig_Click",controlChart,EventArgs.Empty));
			if(InspectWindow(ui,checkResult,opened,"FormToothChartingBig",null,close:false)) {
				System.Threading.Thread.Sleep(2000);
				checkResult.AddShot(ui.Shot(opened.Hwnd,"I1-big.png"),"The big chart (PrintWindow)");
				//B5's on-screen test on the big chart's own window: drawn means the screen shows it and I1-big.png holds the real picture.
				Control toothChartWrapperBig=ui.Invoke(() => (Control)Ui.GetField(Control.FromHandle(opened.Hwnd),"toothChartWrapper"));
				string whyNot=toothChartWrapperBig==null ? "no chart found in the window" : ShotChartWindow(ui,checkResult,toothChartWrapperBig,"I1-big-chart.png");
				if(whyNot=="") {
					checkResult.Verified("The big chart's own window prints drawn by the WPF chart (I1-big-chart.png; B5's on-screen test: the screen is not locked)");
					checkResult.ForPerson("the big chart is the same picture as the Chart's (I1-big.png, I1-chart.png)");
				}
				else {
					checkResult.ForPerson("the big chart is the same picture on screen (I1-big.png; its window does not show the chart in this session: "
						+whyNot+")");
				}
				checkResult.Expect(ui.Close(opened.Hwnd),"The big chart closes");
			}
		}

		public static void CheckI2(Ui ui,CheckResult checkResult) {
			GoToModule(ui,EnumModuleType.Chart);
			Control controlChart=ModuleControl("controlChart");
			int countBefore=Count("SELECT COUNT(*) FROM document WHERE PatNum=1");
			string folder=ImageStore.GetPatientFolder(Patients.GetPat(1),ImageStore.GetPreferredAtoZpath());
			List<long> listDocNums=new List<long>();
			try {
				//1. Save to Images.
				listDocNums.Add(SaveChartToImages(ui,checkResult,controlChart,folder,"I2-images.png","Save to Images"));
				//2. File > User Settings > tick "Chart graphics color distinguishes current provider" > Save; Save to Images again; untick > Save.
				int countProvColourBefore=Count("SELECT COUNT(*) FROM userodpref WHERE UserNum="+Security.CurUser.UserNum+" AND FkeyType="
					+(int)UserOdFkeyType.ToothChartUsesDiffColorByProv);
				if(SetUserSettingProvColour(ui,checkResult,true)) {
					checkResult.Expect(Count("SELECT COUNT(*) FROM userodpref WHERE UserNum="+Security.CurUser.UserNum+" AND FkeyType="
						+(int)UserOdFkeyType.ToothChartUsesDiffColorByProv)==countProvColourBefore+1,"File > User Settings > tick the per-provider colour > Save: "
						+"the user preference ToothChartUsesDiffColorByProv is saved");
					listDocNums.Add(SaveChartToImages(ui,checkResult,controlChart,folder,"I2-images-provcolour.png","Save to Images with the per-provider colours on"));
					SetUserSettingProvColour(ui,checkResult,false);
				}
				checkResult.Expect(Count("SELECT COUNT(*) FROM userodpref WHERE UserNum="+Security.CurUser.UserNum+" AND FkeyType="
					+(int)UserOdFkeyType.ToothChartUsesDiffColorByProv)==countProvColourBefore,"File > User Settings > untick > Save: the preference row is deleted again ("
					+countProvColourBefore+" before)");
				checkResult.ForPerson("both images show the chart with the filling (I2-images.png, I2-images-provcolour.png)");
			}
			finally {
				//Undo: the documents this check added and their files (and a thumbnail if one was made), by the business layer.
				foreach(long docNum in listDocNums.Where(x => x>0)) {
					string fileName=DataCore.GetScalar("SELECT FileName FROM document WHERE DocNum="+docNum);
					ui.Invoke(() => {
						Document document=Documents.GetByNum(docNum);
						Documents.Delete(document);
					});
					try {
						if(fileName!="") {
							System.IO.File.Delete(System.IO.Path.Combine(folder,fileName));
							string thumbnail=System.IO.Path.Combine(folder,"Thumbnails",fileName);
							if(System.IO.File.Exists(thumbnail)) {
								System.IO.File.Delete(thumbnail);
							}
						}
					}
					catch(Exception ex) {
						checkResult.Note("File not deleted: "+ex.Message);
					}
				}
			}
			checkResult.Expect(Count("SELECT COUNT(*) FROM document WHERE PatNum=1")==countBefore,"Undo: the images are deleted (business layer, in the scratch copy)");
		}

		///<summary>Tooth Chart > Save to Images: checks that one document is added and its file is there, saves its picture; returns its DocNum (0 when
		///not exactly one was added, so the undo deletes nothing else).</summary>
		private static long SaveChartToImages(Ui ui,CheckResult checkResult,Control controlChart,string folder,string shot,string what) {
			int countBefore=Count("SELECT COUNT(*) FROM document WHERE PatNum=1");
			long docNumBefore=MaxKey("document","DocNum");
			ui.Post("Tooth Chart > Save to Images",() => Ui.Call(controlChart,"menuItemChartSave_Click",controlChart,EventArgs.Empty)).Wait();
			ui.CloseStrayWindows(checkResult);
			checkResult.Expect(Count("SELECT COUNT(*) FROM document WHERE PatNum=1")==countBefore+1,what+" adds a document (the Tooth Charts category)");
			long docNum=AddedKey("document","DocNum",docNumBefore,"PatNum=1");
			if(!checkResult.Expect(docNum>0,what+": the one document it added is found (DocNum above "+docNumBefore+")")) {
				return 0;
			}
			string fileName=DataCore.GetScalar("SELECT FileName FROM document WHERE DocNum="+docNum);
			string path=System.IO.Path.Combine(folder,fileName);
			if(checkResult.Expect(System.IO.File.Exists(path),what+": its file is in the patient's folder ("+fileName+")")) {
				using Bitmap bitmap=new Bitmap(path);
				checkResult.AddShot(WindowChecksTests.Shot.Bitmap(bitmap,shot),what+": the saved image");
			}
			return docNum;
		}

		///<summary>File > User Settings > "Chart graphics color distinguishes current provider" ticked or unticked > Save (FrmUserSetting). False when the
		///window did not open or did not close.</summary>
		private static bool SetUserSettingProvColour(Ui ui,CheckResult checkResult,bool isOn) {
			Ui.Opened opened=OpenMenu(ui,"File","User Settings");
			if(!InspectWindow(ui,checkResult,opened,"FrmUserSetting",null,close:false)) {
				return false;
			}
			System.Windows.FrameworkElement frm=ui.GetFrm(opened.Hwnd);
			ui.Invoke(() => {
				object checkBox=Ui.GetField(frm,"checkToothChartUsesDiffColorByProv");
				checkBox.GetType().GetProperty("Checked").SetValue(checkBox,(bool?)isOn);
			});
			ui.Post("User Settings: Save",() => ClickFrm(ui,frm,"butSave_Click"));
			bool isClosed=checkResult.Expect(ui.WaitGone(opened.Hwnd),"File > User Settings > "+(isOn ? "tick" : "untick")+" 'Chart graphics color distinguishes "
				+"current provider' > Save closes the window");
			opened.Posted.Wait();
			ui.CloseStrayWindows(checkResult);
			return isClosed;
		}

		///<summary>I7: File > Graphics > "3D chart" > Save, which puts back the 3D chart I6 left on Simple 2D (since TC-8 the window has "3D chart" and
		///"Simple 2D chart" and the "Draw in software" switch, and tests no DirectX format); then "Draw in software" on and off again. Each Save closes
		///the window with no message, and the stored row follows the save rule (ToothChartRelay.SetGraphicsChoice): "3D chart" over a stored 1 writes
		///0, the switch writes "WpfSoftware" to DirectXFormat and clears only that value when it goes off.</summary>
		public static void CheckI7(Ui ui,CheckResult checkResult) {
			string sqlRow="SELECT CONCAT(GraphicsSimple,' ',DirectXFormat) FROM computerpref WHERE ComputerName='"+POut.String(CodeBase.ODEnvironment.MachineName)+"'";
			string rowBefore=DataCore.GetScalar(sqlRow);
			checkResult.Note("computerpref before: "+rowBefore);
			Ui.Opened opened=OpenMenu(ui,"File","Graphics");
			if(!InspectWindow(ui,checkResult,opened,"FormGraphics",null,close:false)) {
				return;
			}
			checkResult.AddShot(ui.Shot(opened.Hwnd,"I7.png"),"Graphics");
			IntPtr hwnd=opened.Hwnd;
			ui.Invoke(() => ((RadioButton)FindControl(ui,hwnd,"radio3DChart",true)).Checked=true);
			List<IntPtr> listBefore=ui.GetWindows();
			ui.Post("Save",() => ((System.Windows.Forms.Button)FindControl(ui,hwnd,"butSave",true)).PerformClick());
			System.Threading.Thread.Sleep(3000);
			List<IntPtr> listNew=ui.GetWindows().Where(x => !listBefore.Contains(x)).ToList();
			foreach(IntPtr hwndNew in listNew) {
				checkResult.Fail("Save showed: "+ui.DescribeWindow(hwndNew));
				ui.Close(hwndNew);
			}
			checkResult.Expect(ui.WaitGone(hwnd,30000),"3D chart > Save: no message, the window closes (no DirectX format is tested since TC-8)");
			opened.Posted.Wait();
			GoToModule(ui,EnumModuleType.Account);
			GoToModule(ui,EnumModuleType.Chart);
			Control toothChartWrapper=(Control)Ui.GetField(ModuleControl("controlChart"),"toothChartWrapper");
			string engine=ui.Invoke(() => toothChartWrapper.GetType().GetProperty("Engine").GetValue(toothChartWrapper).ToString());
			checkResult.Expect(engine=="Wpf3D","The Chart draws the 3D chart (the WPF chart) again ("+engine+")");
			string rowAfter3D=DataCore.GetScalar(sqlRow);
			bool isStoredSimple2D=rowBefore.StartsWith("1 ");
			checkResult.Expect(isStoredSimple2D ? rowAfter3D.StartsWith("0 ") : rowAfter3D.Split(' ')[0]==rowBefore.Split(' ')[0],
				"3D chart > Save: "+(isStoredSimple2D ? "over the stored 1 (Simple 2D, from I6) it writes 0" : "a stored 0 or 2 is kept")+" ("+rowBefore+" -> "
				+rowAfter3D+")",rowAfter3D);
			//"Draw in software" on, then off again, each with Save.
			string message=SetDrawInSoftware(ui,checkResult,true);
			string rowSoftware=DataCore.GetScalar(sqlRow);
			bool isDrawnInSoftware=ui.Invoke(() => (bool)toothChartWrapper.GetType().GetProperty("WpfDrawInSoftware").GetValue(toothChartWrapper));
			checkResult.Expect(message=="" && rowSoftware==rowAfter3D.Split(' ')[0]+" WpfSoftware","Draw in software > Save: no message, DirectXFormat is "
				+"'WpfSoftware' and the choice is kept ("+rowSoftware+")",
				message);
			checkResult.Note("The Chart's wrapper draws in software after the Save: "+isDrawnInSoftware);
			message=SetDrawInSoftware(ui,checkResult,false);
			string rowAfter=DataCore.GetScalar(sqlRow);
			checkResult.Expect(message=="" && rowAfter==rowAfter3D,"Draw in software off > Save: no message, the row is as it was ("+rowAfter+")",message);
			checkResult.ForPerson("the Chart's 3D chart after each Save: the live view, then the smoothed picture drawn in software, then the live view again");
		}

		///<summary>File > Graphics > "3D chart" with "Draw in software" ticked or unticked > Save. Returns "" when the window closed, or the message Save
		///showed (answered).</summary>
		private static string SetDrawInSoftware(Ui ui,CheckResult checkResult,bool isOn) {
			Ui.Opened openedGraphics=OpenMenu(ui,"File","Graphics");
			if(!InspectWindow(ui,checkResult,openedGraphics,"FormGraphics",null,close:false)) {
				return "File > Graphics did not open";
			}
			IntPtr hwndGraphics=openedGraphics.Hwnd;
			ui.Invoke(() => {
				((RadioButton)FindControl(ui,hwndGraphics,"radio3DChart",true)).Checked=true;
				((CheckBox)FindControl(ui,hwndGraphics,"checkDrawInSoftware",true)).Checked=isOn;
			});
			return SaveGraphics(ui,checkResult,openedGraphics);
		}

		///<summary>I8's in-process part: the WPF chart (every computer's 3D chart since TC-8, which removed the developer switch OPMS_TOOTHCHART_WPF),
		///Show Big (timed), the big window resized.</summary>
		public static void CheckI8BIG(Ui ui,CheckResult checkResult) {
			GoToModule(ui,EnumModuleType.Chart);
			Control controlChart=ModuleControl("controlChart");
			Control toothChartWrapper=(Control)Ui.GetField(controlChart,"toothChartWrapper");
			string engine=ui.Invoke(() => toothChartWrapper.GetType().GetProperty("Engine").GetValue(toothChartWrapper).ToString());
			checkResult.Expect(engine=="Wpf3D","The Chart runs the WPF chart ("+engine+")");
			System.Diagnostics.Stopwatch stopwatch=System.Diagnostics.Stopwatch.StartNew();
			Ui.Opened opened=ui.Open("Tooth Chart > Show Big",() => Ui.Call(controlChart,"menuItemChartBig_Click",controlChart,EventArgs.Empty));
			if(!InspectWindow(ui,checkResult,opened,"FormToothChartingBig",null,close:false)) {
				return;
			}
			checkResult.Note("Show Big: the window after "+stopwatch.Elapsed.TotalSeconds.ToString("0.0")+" s (in process; the test host's memory is not the program's)");
			System.Threading.Thread.Sleep(2500);
			checkResult.AddShot(ui.Shot(opened.Hwnd,"I8-wpf-big.png"),"The big WPF chart");
			Size sizeBefore=ui.Invoke(() => Control.FromHandle(opened.Hwnd).Size);
			//The big window opens maximized: restore it first, then give it a clearly different size (the end of a corner drag).
			ui.Invoke(() => {
				Form form=(Form)Control.FromHandle(opened.Hwnd);
				form.WindowState=FormWindowState.Normal;
				form.Size=new Size(sizeBefore.Width*3/4,sizeBefore.Height*3/4);
			});
			System.Threading.Thread.Sleep(3000);
			ShotInfo shotInfo=ui.Shot(opened.Hwnd,"I8-wpf-big-resized.png");
			checkResult.AddShot(shotInfo,"The big WPF chart after a resize to 3/4 (set by the window's Size, not a corner drag)");
			checkResult.Note("Big window "+sizeBefore.Width+"x"+sizeBefore.Height+" resized to "+shotInfo.Width+"x"+shotInfo.Height);
			checkResult.ForPerson("sharp, not stretched or blurred after the resize (I8-wpf-big-resized.png); a corner drag is mouse work");
			checkResult.Expect(ui.Close(opened.Hwnd),"The big chart closes");
		}

		///<summary>Undo of I's prep: the D2391 deleted (status deleted), as progress notes > Delete does, by the business layer.</summary>
		public static void CheckIUNDO(Ui ui,CheckResult checkResult) {
			//Only the procedure IPREP added (its key), never another treatment-planned procedure on 30 that the copy already had.
			long procNum=GetKey("IPREP.ProcNum");
			if(procNum==0 || Count("SELECT COUNT(*) FROM procedurelog WHERE ProcNum="+procNum+" AND ProcStatus=1")==0) {
				checkResult.Note("No D2391 of IPREP to delete (IPREP's key: "+procNum+")");
				return;
			}
			ui.Invoke(() => Procedures.Delete(procNum));
			checkResult.Expect(Count("SELECT COUNT(*) FROM procedurelog WHERE ProcNum="+procNum+" AND ProcStatus=6")==1,"The D2391 is deleted (the row stays with status deleted)");
		}
		#endregion I

		#region J
		///<summary>Setup > Advanced Setup > Show Features: EHR on (on is true) or off, with its messages; Save.</summary>
		private static void ShowFeaturesEhr(Ui ui,CheckResult checkResult,bool on,string shotBefore,string shotConfirm) {
			Ui.Opened opened=OpenMenu(ui,"Setup","Advanced Setup","Show Features");
			if(!InspectWindow(ui,checkResult,opened,"FormShowFeatures",null,close:false)) {
				return;
			}
			IntPtr hwnd=opened.Hwnd;
			if(shotBefore!=null) {
				checkResult.AddShot(ui.Shot(hwnd,shotBefore),"Show Features before");
			}
			OpenPmsDental.UI.CheckBox checkEhr=(OpenPmsDental.UI.CheckBox)FindControl(ui,hwnd,"checkEhr");
			checkResult.Expect(ui.Invoke(() => checkEhr.Checked)!=on,"EHR is "+(on ? "unticked" : "ticked")+" before");
			if(on) {
				//Tick, Cancel: unticked, no restart message.
				Ui.Opened openedConfirm=ui.Open("Tick EHR",() => { checkEhr.Checked=true; Ui.Call(Control.FromHandle(hwnd),"checkEhr_Click",checkEhr,EventArgs.Empty); },allowMessage:true);
				if(openedConfirm.IsOpen) {
					string text=Native.GetDialogMessage(openedConfirm.Hwnd);
					checkResult.Note("Confirmation: "+text.Replace("\r"," ").Replace("\n"," "));
					string expected=FormShowFeatures.GetEhrOnConfirmationText();
					checkResult.Expect(Clean2(text)==Clean2(expected),"One confirmation with the program's text",text);
					foreach(string sentence in new[] { "Turning EHR on changes how prescriptions, patient race, required fields and the Chart work, and it shows the EHR tools.",
						"This program is not certified EHR technology. Its measures cannot be used to attest or submit to any program.",
						"A restart is needed for the change to take effect.","Turn EHR on?" })
					{
						checkResult.Expect(Clean2(text).Contains(Clean2(sentence)),"It says '"+sentence+"'",text);
					}
					checkResult.Expect(!Native.GetDialogMessage(openedConfirm.Hwnd).ToLower().Contains("password"),"No password");
					if(shotConfirm!=null) {
						checkResult.AddShot(ui.Shot(openedConfirm.Hwnd,shotConfirm),"The confirmation");
					}
					Native.ClickDialogButton(openedConfirm.Hwnd,"Cancel");
					ui.WaitGone(openedConfirm.Hwnd);
					System.Threading.Thread.Sleep(800);
					List<IntPtr> listAfterCancel=ui.GetWindows().Where(x => Native.IsDialogClass(x)).ToList();
					checkResult.Expect(listAfterCancel.Count==0,"Cancel: no restart message");
					listAfterCancel.ForEach(x => ui.Close(x));
					openedConfirm.Posted.Wait();
					checkResult.Expect(!ui.Invoke(() => checkEhr.Checked),"Cancel: the box unchecks");
				}
				//Tick, OK: the restart message.
				Ui.Opened openedConfirm2=ui.Open("Tick EHR again",() => { checkEhr.Checked=true; Ui.Call(Control.FromHandle(hwnd),"checkEhr_Click",checkEhr,EventArgs.Empty); },allowMessage:true);
				if(openedConfirm2.IsOpen) {
					List<IntPtr> listBefore=ui.GetWindows();
					Native.ClickDialogButton(openedConfirm2.Hwnd,"OK");
					IntPtr hwndRestart=ui.WaitForNewWindow(listBefore.Where(x => x!=openedConfirm2.Hwnd).ToList(),15000);
					string textRestart=hwndRestart==IntPtr.Zero ? "" : Native.GetDialogMessage(hwndRestart);
					checkResult.Expect(textRestart.Contains("You will need to restart the program for the change to take effect."),"OK: 'You will need to restart the program for "
						+"the change to take effect.'",textRestart);
					if(hwndRestart!=IntPtr.Zero) {
						ui.Close(hwndRestart);
					}
					openedConfirm2.Posted.Wait();
				}
			}
			else {
				Ui.Opened openedRestart=ui.Open("Untick EHR",() => { checkEhr.Checked=false; Ui.Call(Control.FromHandle(hwnd),"checkEhr_Click",checkEhr,EventArgs.Empty); },allowMessage:true);
				string text=openedRestart.IsOpen ? Native.GetDialogMessage(openedRestart.Hwnd) : "";
				checkResult.Expect(text.Contains("You will need to restart the program for the change to take effect."),"Untick: the restart message, no confirmation",text);
				if(openedRestart.IsOpen) {
					ui.Close(openedRestart.Hwnd);
				}
				openedRestart.Posted.Wait();
			}
			checkResult.Expect(ui.Invoke(() => checkEhr.Checked)==on,"EHR is "+(on ? "ticked" : "unticked"));
			ui.Post("Save",() => ((System.Windows.Forms.Button)FindControl(ui,hwnd,"butSave",true)).PerformClick());
			checkResult.Expect(ui.WaitGone(hwnd,20000),"Save closes Show Features");
			opened.Posted.Wait();
			ui.CloseStrayWindows(checkResult);
			checkResult.Expect(DataCore.GetScalar("SELECT ValueString FROM preference WHERE PrefName='ShowFeatureEhr'")==(on ? "1" : "0"),"ShowFeatureEhr is "+(on ? "1" : "0"));
		}

		private static string Clean2(string text) {
			return string.Join(" ",(text??"").Split(new[] { ' ','\r','\n' },StringSplitOptions.RemoveEmptyEntries));
		}

		public static void CheckJ1(Ui ui,CheckResult checkResult) {
			ShowFeaturesEhr(ui,checkResult,on:true,shotBefore:"J1-before.png",shotConfirm:"J1-confirm.png");
		}

		public static void CheckJ16(Ui ui,CheckResult checkResult) {
			ShowFeaturesEhr(ui,checkResult,on:false,shotBefore:null,shotConfirm:null);
		}

		///<summary>J1's restart: the Chart, printed as J1-chart-ehr.png.</summary>
		public static void CheckJCHARTJ1(Ui ui,CheckResult checkResult) {
			ChartAfterStart(ui,checkResult,"J1-chart-ehr.png");
		}

		///<summary>J16's restart: the Chart, printed as J16-chart.png.</summary>
		public static void CheckJCHARTJ16(Ui ui,CheckResult checkResult) {
			ChartAfterStart(ui,checkResult,"J16-chart.png");
		}

		///<summary>J17's first restart (EHR saved by the wizard): the Chart, printed as J17-chart-ehr.png.</summary>
		public static void CheckJCHARTJ17ON(Ui ui,CheckResult checkResult) {
			ChartAfterStart(ui,checkResult,"J17-chart-ehr.png");
		}

		///<summary>J17's second restart (EHR off again): the Chart, printed as J17-chart-off.png.</summary>
		public static void CheckJCHARTJ17OFF(Ui ui,CheckResult checkResult) {
			ChartAfterStart(ui,checkResult,"J17-chart-off.png");
		}

		///<summary>After a start: the Chart toolbar has (or has not) the EHR button. Each restart prints its own picture (fileName), so one restart's
		///picture never stands for another's.</summary>
		private static void ChartAfterStart(Ui ui,CheckResult checkResult,string fileName) {
			GoToModule(ui,EnumModuleType.Chart);
			Control controlChart=ModuleControl("controlChart");
			List<string> listButtons=ui.Invoke(() => {
				Control toolBar=Ui.AllControls(controlChart).FirstOrDefault(x => x.GetType().Name=="ToolBarOD" && x.Name=="ToolBarMain")
					??Ui.AllControls(controlChart).First(x => x.GetType().Name=="ToolBarOD");
				System.Collections.IEnumerable buttons=(System.Collections.IEnumerable)toolBar.GetType().GetProperty("Buttons").GetValue(toolBar);
				return buttons.Cast<object>().Select(x => (Member(x,"Text") as string)??"").ToList();
			});
			checkResult.Note("Chart toolbar: "+string.Join(", ",listButtons));
			checkResult.ListVerified.Add("EHR button "+(listButtons.Contains("EHR") ? "present" : "absent"));
			checkResult.AddShot(ui.Shot(ui.Invoke(() => FormMain.Handle),fileName),"The Chart after the start (EHR button "
				+(listButtons.Contains("EHR") ? "present" : "absent")+")");
		}

		public static void CheckJ3(Ui ui,CheckResult checkResult) {
			checkResult.Note("snomed rows: "+DataCore.GetScalar("SELECT COUNT(*) FROM snomed"));
			Ui.Opened openedEhr=OpenMenu(ui,"Setup","Chart","EHR");
			if(!InspectWindow(ui,checkResult,openedEhr,"FormEhrSetup",null,close:false)) {
				return;
			}
			IntPtr hwndEhr=openedEhr.Hwnd;
			Control butSnomed=ui.Invoke(() => Ui.AllControls(Control.FromHandle(hwndEhr)).FirstOrDefault(x => x is IButtonControl && x.Text.Replace("&","").StartsWith("SNOMED")));
			if(checkResult.Expect(butSnomed!=null,"Setup > Chart > EHR has the SNOMED CTs button")) {
				Ui.Opened opened=ui.Open("SNOMED CTs",() => ((IButtonControl)butSnomed).PerformClick());
				if(opened.IsOpen) {
					checkResult.Note("SNOMED window: "+ui.DescribeWindow(opened.Hwnd));
					bool hasTools=ui.Invoke(() => {
						Control control=Control.FromHandle(opened.Hwnd);
						if(control!=null) {
							return Ui.AllControls(control).Any(x => x.Visible && x.Text.Contains("One-Time Tools"));
						}
						System.Windows.FrameworkElement frm=Ui.GetFrmOnThread(control);
						return frm!=null && WpfTree(frm).OfType<System.Windows.FrameworkElement>().Any(x => x.IsVisible && (Member(x,"Text") as string??"").Contains("One-Time Tools"));
					});
					checkResult.Expect(!hasTools,"No 'One-Time Tools' group (no SNOMED rows)");
					checkResult.AddShot(ui.Shot(opened.Hwnd,"J3.png"),"SNOMED CTs");
					checkResult.Expect(ui.Close(opened.Hwnd),"SNOMED CTs closes");
				}
			}
			ui.Close(hwndEhr);
			//[DATA, undone in J13] the synthetic SNOMED row J4 to J13 use.
			DataCore.NonQ("INSERT INTO snomed (SnomedCode,Description) VALUES ('TEST0001','Synthetic gate test code')");
			checkResult.Expect(Count("SELECT COUNT(*) FROM snomed WHERE SnomedCode='TEST0001'")==1,"[DATA] The synthetic SNOMED row TEST0001 is inserted (J13 deletes it)");
		}

		public static void CheckJ15(Ui ui,CheckResult checkResult) {
			Ui.Opened opened=OpenToolbar(ui,"Email");
			if(InspectWindow(ui,checkResult,opened,"FormEmailMessageEdit",null,close:false)) {
				List<string> listItems=ui.Invoke(() => {
					List<string> list=new List<string>();
					//The toolbars' buttons and their drop-down menus (the Send button's menu is an ODToolBarButton's ContextMenuStripDropDown since S3a-M3,
					//which replaced its WinForms DropDownMenu). A ToolStripItem is Visible only while its menu shows: Available is the item's own state.
					foreach(Control toolBar in Ui.AllControls(Control.FromHandle(opened.Hwnd)).Where(x => x.GetType().Name=="ToolBarOD")) {
						foreach(object button in ((System.Collections.IEnumerable)toolBar.GetType().GetProperty("Buttons").GetValue(toolBar)).Cast<object>()) {
							list.Add(toolBar.Name+" button: "+((Member(button,"Text") as string)??""));
							if(Member(button,"ContextMenuStripDropDown") is ContextMenuStrip contextMenuStripButton) {
								list.AddRange(contextMenuStripButton.Items.OfType<ToolStripMenuItem>().Where(x => x.Available).Select(x => toolBar.Name+" menu: "+x.Text.Replace("&","")));
							}
						}
					}
					foreach(Control control in Ui.AllControls(Control.FromHandle(opened.Hwnd)).Where(x => !x.Name.StartsWith("text") && !x.Name.StartsWith("grid"))) {
						object dropDown=Member(control,"ContextMenuStrip")??Member(control,"ContextMenu")??Member(control,"DropDownMenu")??Member(control,"MenuDropDown");
						if(dropDown is ContextMenuStrip contextMenuStrip) {
							list.AddRange(contextMenuStrip.Items.OfType<ToolStripItem>().Where(x => x.Available).Select(x => control.Name+": "+x.Text.Replace("&","")));
						}
						else if(dropDown is ContextMenu contextMenu) {
							list.AddRange(contextMenu.MenuItems.Cast<MenuItem>().Where(x => x.Visible).Select(x => control.Name+": "+x.Text.Replace("&","")));
						}
					}
					return list;
				});
				checkResult.Note("Drop-down items of the window's buttons: "+string.Join("; ",listItems));
				checkResult.Expect(listItems.Any(x => x.EndsWith("menu: Direct Messaging")) && listItems.Any(x => x.EndsWith("menu: Send")),
					"The Send button's drop-down lists Send and Direct Messaging (read from its menu, not opened)",string.Join("; ",listItems));
				checkResult.Expect(listItems.Any(x => x.EndsWith("menu: Cancel")),"The Send button's drop-down lists Cancel (the checklist's third item)",
					string.Join("; ",listItems));
				checkResult.AddShot(ui.Shot(opened.Hwnd,"J15-send-menu.png"),"The new e-mail (the drop-down itself is not opened headless)");
				checkResult.Expect(ui.Close(opened.Hwnd),"The e-mail is cancelled (nothing sent or saved)");
			}
			int countAddresses=Count("SELECT COUNT(*) FROM emailaddress");
			Ui.Opened openedAddresses=OpenMenu(ui,"Setup","Manage","E-mail");
			if(InspectWindow(ui,checkResult,openedAddresses,"FormEmailAddresses",null,close:false)) {
				IntPtr hwndAddresses=openedAddresses.Hwnd;
				Ui.Opened openedEdit=ui.Open("Add",() => ((IButtonControl)FindControl(ui,hwndAddresses,"butAdd",true)).PerformClick());
				if(InspectWindow(ui,checkResult,openedEdit,"FormEmailAddressEdit",null,close:false)) {
					IntPtr hwndEdit=openedEdit.Hwnd;
					Control butCert=ui.Invoke(() => FindControl(ui,hwndEdit,"butRegisterCertificate",true));
					if(checkResult.Expect(butCert!=null,"The address editor has its Certificate button")) {
						//Since DIRECT-CERT (checklist revised 2026-11-09) the Certificate button needs the Username first: with it empty, a message asks for it.
						ui.Invoke(() => FindControl(ui,hwndEdit,"textUsername",true).Text="");
						Ui.Opened openedAsk=ui.Open("Certificate with the Username empty",() => ((IButtonControl)butCert).PerformClick(),allowMessage:true);
						string textAsk=openedAsk.IsOpen && Native.IsDialogClass(openedAsk.Hwnd) ? Native.GetDialogMessage(openedAsk.Hwnd) : "";
						checkResult.Expect(textAsk.Contains("Enter the email address first."),"Certificate with the Username empty asks for it first: '"+textAsk.Trim()+"'",
							openedAsk.IsOpen ? ui.DescribeWindow(openedAsk.Hwnd) : "nothing opened");
						if(openedAsk.IsOpen) {
							ui.Close(openedAsk.Hwnd);
						}
						openedAsk.Posted.Wait();
						//With the checklist's address in Username the registration window opens.
						ui.Invoke(() => FindControl(ui,hwndEdit,"textUsername",true).Text="gatetest@example.invalid");
						Ui.Opened openedReg=ui.Open("Certificate",() => ((IButtonControl)butCert).PerformClick());
						if(InspectWindow(ui,checkResult,openedReg,"FormEmailCertRegister",null,close:false)) {
							IntPtr hwndReg=openedReg.Hwnd;
							checkResult.Note("The registration window's title: '"+Native.GetText(hwndReg)+"' (the checklist calls it Email Certificate Registration)");
							List<string> listButtons=ui.Invoke(() => Ui.AllControls(Control.FromHandle(hwndReg)).Where(x => x is IButtonControl && x.Visible)
								.Select(x => x.Name+" '"+x.Text.Replace("&","")+"'"+(x.Enabled ? "" : " (disabled)")).ToList());
							checkResult.Note("Buttons: "+string.Join(", ",listButtons));
							string groupCert=ui.Invoke(() => FindControl(ui,hwndReg,"groupCertificate",true)?.Text??"");
							checkResult.Expect(groupCert.Contains(".pfx") && groupCert.Contains(".p12"),"The certificate group asks for the provider's .pfx or .p12 ('"+groupCert+"')");
							checkResult.Expect(listButtons.Any(x => x.StartsWith("butImport 'Import'")),"The .pfx/.p12 import button (Import) is there; it is never pressed",
								string.Join(", ",listButtons));
							checkResult.Expect(listButtons.Any(x => x.StartsWith("butImportAnchor ")),"The trust anchor's Import... button is there; it is never pressed",
								string.Join(", ",listButtons));
							checkResult.Expect(!listButtons.Any(x => x.Contains("Send Code")),"No Send Code button (DIRECT-CERT replaced it)",string.Join(", ",listButtons));
							checkResult.AddShot(ui.Shot(hwndReg,"J15-send-code.png"),"The Direct certificate window (nothing imported)");
							ui.Close(hwndReg);
						}
						openedReg.Posted?.Wait();
					}
					ui.Close(hwndEdit);
				}
				openedEdit.Posted?.Wait();
				ui.Close(hwndAddresses);
			}
			checkResult.Expect(Count("SELECT COUNT(*) FROM emailaddress")==countAddresses,"No address saved ("+countAddresses+" before)");
		}
		#endregion J
	}
}
