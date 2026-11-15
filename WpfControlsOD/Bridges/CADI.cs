using System;
using System.Windows.Forms;
using OpenPmsDentalBusiness;
using CodeBase;
using OpenPmsDentalHelper32.Cadi;

namespace OpenPmsDental.Bridges {
	/// <summary>CADI uses their own OLE format for passing data to CADI. 
	/// See the bridging documents in \\opendental.od\serverfiles\Storage\OPEN DENTAL\Programmers Documents\Bridge Info\CADI for details.
	/// Since N64-A3 (native 64-bit first, net10-plan decision 23; docs/port-notes/N64-A3.md) CADI's automation object is called from the
	/// program's own process as before (CADINativeWindow, in CADIInProcess.cs) whenever this process can reach it, and from the 32-bit helper
	/// program's Cadi module (OpenPmsDentalHelper32), with the same calls, only when it cannot: the class is registered only as a 32-bit DLL
	/// server, or its activation or first call fails with REGDB_E_CLASSNOTREG, CO_E_SERVER_EXEC_FAILURE, E_NOINTERFACE or
	/// TYPE_E_LIBNOTREGISTERED (NativeFirstChooser.Cadi, a 64-bit program only). The choice is made once per program process and logged.</summary>
	public static class CADI {
		///<summary>Keep as a static object so it doesn't get garbage collected.</summary>
		private static CADINativeWindow CADIWindow; 
		///<summary>Per bridging documents, birthdate is specified using COM TDATE, which is calculated as the number of days that have elapsed since 12/30/1899</summary>
		private static readonly DateTime TDATE_START=new DateTime(1899,12,30);
		///<summary>How long the helper's SendData may take: creating CADI's object starts CADI when it is not running. When it passes, the helper
		///is ended (CADI keeps running) and SendData shows the helper's message.</summary>
		private static readonly TimeSpan _timeSpanCall=TimeSpan.FromMinutes(5);

		public static void SendData(Program ProgramCur,Patient pat) {
			try {
				CadiPatient cadiPatient=null;
				if(pat!=null) {
					cadiPatient=GetCadiPatient(ProgramCur,pat);
				}
				if(CADIInProcess.IsChosen()) {
					if(CADIWindow==null) { //Only release this reference once the CADIWindow instance has closed.
						CADIWindow=new CADINativeWindow(new Action(() => CADIWindow=null ));
					}
					if(CADIWindow.SendData(cadiPatient)) {
						return;
					}
					//N64-A3: this process cannot reach CADI's server and no call reached CADI: the helper repeats the call once, now.
				}
				SendDataToHelper(cadiPatient);
			}
			catch (Exception e) {
				MessageBox.Show(e.Message);
				SecurityLogs.MakeLogEntry(EnumPermType.ChartModule,ODMethodsT.Coalesce(pat).PatNum,e.Message);
			}
		}

		///<summary>The values CADINativeWindow.SendData sent CADI for pat before N64-A3, worked out the same way. The two program properties are
		///read here, before CADI's object is made (before N64-A3 they were read between OleBeginTransaction and OleSetPatientName; CADI's calls
		///and their order are the same), so the helper gets the same values.</summary>
		private static CadiPatient GetCadiPatient(Program ProgramCur,Patient pat) {
			string id="";
			if(ProgramProperties.GetPropVal(ProgramCur.ProgramNum,ProgramProperties.PropertyDescs.PatOrChartNum)=="0") {
				id=pat.PatNum.ToString();
			}
			else {
				id=pat.ChartNumber;
			}
			CadiPatient cadiPatient=new CadiPatient();
			cadiPatient.Name=pat.LName+", "+pat.FName;
			cadiPatient.DateOfBirth=pat.Birthdate.Subtract(TDATE_START).TotalDays;
			cadiPatient.Age=pat.Age;
			cadiPatient.Sex=pat.Gender==PatientGender.Female ? "F" : "M";
			//Send patient images file path
			string imagePath=ProgramProperties.GetPropVal(ProgramCur.ProgramNum,ProgramProperties.PropertyDescs.ImageFolder);
			cadiPatient.PicLibPath=CodeBase.ODFileUtils.CombinePaths(imagePath,id);
			return cadiPatient;
		}

		///<summary>The fallback (N64-A3): the helper's Cadi module makes CADINativeWindow's calls in the 32-bit helper and keeps CADI's object there
		///between calls. CADI may bring itself to the front (OleBringToFrontApp): the call runs while this thread dispatches its messages with its
		///windows disabled (Helper32Client.CallPumping, which allows the helper to set the foreground window), and CADI stays in front. What CADI's
		///calls threw comes back with its message (Unable to access CADI, ...); a failure of the helper itself (missing, not answering, ended) adds
		///the helper's message.</summary>
		private static void SendDataToHelper(CadiPatient cadiPatient) {
			try {
				Helper32Client.Default.CallPumping(CadiCommands.MODULE,CadiCommands.SEND_DATA,CadiCommands.EncodePatient(cadiPatient),null,_timeSpanCall,
					isReactivating:false);
			}
			catch(Helper32Exception ex) when(ex.Failure!=Helper32Failure.Remote) {
				throw new Exception("Unable to open CADI.\r\n"+ex.Message);
			}
		}
	}
}