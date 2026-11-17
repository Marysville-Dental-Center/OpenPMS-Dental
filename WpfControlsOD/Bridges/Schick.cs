#if !DISABLE_WINDOWS_BRIDGES
using System;
using System.Windows.Forms;
using CodeBase;
using OpenPmsDentalBusiness;
using System.Collections.Generic;
using OpenPmsDentalHelper32.Schick;

namespace OpenPmsDental.Bridges{
	///<summary>Provides bridging functionality to Schick CDR.
	///The algorithm provided below was inspired by the Schick example project named CSAutoApp_2008.
	///Since S3a-14 of the .NET 10 move (docs/port-notes/S3a-14.md) CDR DICOM's automation objects are created and called in the 32-bit helper
	///program (OpenPmsDentalHelper32, its Schick module, with the calls this class made before), because the program is 64-bit after stage 3b
	///and nothing proves yet that CDR's COM classes serve a 64-bit caller (open point O-14).
	///Since S3a-14n (native first, net10-plan decision 23) the helper is the fallback: CDR's automation server is called from the program's own
	///process whenever its class activates here (SchickInProcess, the code of before S3a-14, NativeFirstChooser.SchickCdr), and only when it does
	///not (not registered for a 64-bit caller, or its server could not start) does the patient go to the helper. The choice is made once per
	///program process and logged.</summary>
	public class Schick{
		///<summary>How long ShowExam may take on the helper's path: CDR starts when it is not running, and creating an exam waits for the user's answer.
		///When it passes, the helper is ended with everything it held (docs/port-notes/S3a-14.md, section 3) and SendData shows the helper's
		///message.</summary>
		private static readonly TimeSpan _timeSpanCall=TimeSpan.FromMinutes(30);

		public Schick() {
		}

		///<summary>Launches the main Patient Document window of Schick.</summary>
		public static void SendData(Program ProgramCur,Patient pat){
			if(pat==null){
				return;
			}
			List<ProgramProperty> ForProgram=ProgramProperties.GetForProgram(ProgramCur.ProgramNum);
			ProgramProperty PPCur=ProgramProperties.GetCur(ForProgram,"Enter 0 to use PatientNum, or 1 to use ChartNum");
			string patID="";
			if(PPCur.PropertyValue=="0") {
				patID=pat.PatNum.ToString();
			}
			else {
				patID=pat.ChartNumber;
			}
			string version4or5=ProgramProperties.GetPropVal(ProgramCur.ProgramNum,"Schick Version 4 or 5");
			if(version4or5!="5") {
				//Version 4 went through Open Dental's closed VBbridges.dll, which this program does not ship (removed 2026-11-01). Schick publishes no
				//interface for version 4 to rebuild it from. Version 5 below uses Schick's own CDRDicom automation interface.
				MsgBox.Show("Schick",BridgeNotAvailable.SchickVersion4);
				return;
			}
			try {
				ShowExam(pat,patID);
			}
			catch(Helper32Exception ex) when(ex.Failure!=Helper32Failure.Remote) {
				//The helper itself failed (missing, not started, not answering, ended): its own message, so the office knows what to repair.
				MessageBox.Show("Error launching Schick CDR Dicom.\r\n"+ex.Message);
			}
			catch {
				MessageBox.Show("Error launching Schick CDR Dicom.");
			}
		}

		///<summary>Native first (S3a-14n): CDR's automation server is called from this process as before S3a-14 (SchickInProcess.ShowExam), which
		///returns false when NativeFirstChooser.SchickCdr chose the helper. Then the fallback: the helper's Schick module loads the patient in CDR's
		///exam window, or creates the exam (SchickCommands.SHOW_EXAM). The helper may
		///bring the exam window to the foreground, as the program did, and it stays there (no reactivation of the program's window). The call
		///runs on another thread while this thread dispatches its messages with its windows disabled (Helper32Client.CallPumping), so the
		///program keeps painting while CDR starts or its create-exam window waits for the user, as it did during the COM call it made itself,
		///and a second click cannot start another call meanwhile.</summary>
		private static void ShowExam(Patient pat,string patID) {
			if(SchickInProcess.ShowExam(pat.LName,pat.FName,patID)) {
				return;
			}
			Helper32Client.Default.CallPumping(SchickCommands.MODULE,SchickCommands.SHOW_EXAM,new[] { pat.LName,pat.FName,patID },null,_timeSpanCall,
				isReactivating:false);
		}

	}
}
#endif