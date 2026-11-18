// Group U's font check UF1 (package UI-2a-1 of docs/specs/ui-refresh-plan.md, section 9 "Content and signatures"; docs/port-notes/UI-2a.md):
// a signed note in FormProcEdit stays valid after a change of the program's zoom, a resize of the window and a save. The mapping of UI-2a-1
// sets fonts only in InitializeLayoutManager, before Load and before the signature is filled, and the rebuild sets a font only when its size
// differs (Job 55866: setting a font of the same size fires TextChanged, which cleared the signature of a procedure or group note). The check
// needs a window that is shown (the zoom and the resize lay it out through OnResize), so it runs in the in-process session of the window
// checks on the scratch copy, opt-in, in the owner's connected session (UTests.UF1). The note and its signature are synthetic; the rows the
// check adds are deleted again, and the procedure's note is put back as it was.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using OpenPmsDental;
using OpenPmsDental.UI;
using OpenPmsDentalBusiness;

namespace WindowChecksTests.InProc {
	public static partial class InProcChecks {
		///<summary>The synthetic note UF1 signs.</summary>
		private const string NOTE_UF1="Synthetic signed note for the font check UF1 (UI-2a-1).";

		public static void CheckUF1(Ui ui,CheckResult checkResult) {
			GoToModule(ui,EnumModuleType.Chart);
			Control controlChart=ModuleControl("controlChart");
			long patNum=ui.Invoke(() => FormOpenDental.PatNumCur);
			long procNum=ui.Invoke(() => {
				long codeNum=ProcedureCodes.GetCodeNum("D1110");
				return PIn.Long(DataCore.GetScalar("SELECT COALESCE(MIN(ProcNum),0) FROM procedurelog WHERE PatNum="+POut.Long(patNum)+" AND CodeNum="
					+POut.Long(codeNum)));
			});
			if(!checkResult.Expect(procNum!=0,"The patient has a D1110 procedure (the synthetic dump's)",patNum.ToString())) {
				return;
			}
			long procNoteNumBefore=MaxKey("procnote","ProcNoteNum");
			int zoomBefore=ui.Invoke(() => ComputerPrefs.LocalComputer.Zoom);
			try {
				//The signed note, as FormProcEdit saves one: the program's own signature box, its key data from the procedure and the note.
				string signature=ui.Invoke(() => {
					Procedure procedure=Procedures.GetOneProc(procNum,includeNote:true);
					string keyData=Procedures.GetSignatureKeyData(procedure,NOTE_UF1);
					using(SignatureBox signatureBox=new SignatureBox()) {
						signatureBox.SetTabletState(1);
						signatureBox.SetPointList(new List<PointF> { new PointF(10,10),new PointF(40,30),new PointF(70,12),new PointF(100,35),new PointF(0,0),
							new PointF(20,40),new PointF(90,8) });
						signatureBox.SetKeyString("0000000000000000");
						signatureBox.SetAutoKeyData(keyData);
						string sig=signatureBox.GetSigString();
						ProcNotes.Insert(new ProcNote { PatNum=procedure.PatNum,ProcNum=procNum,EntryDateTime=DateTime.Now,UserNum=Security.CurUser.UserNum,
							Note=NOTE_UF1,SigIsTopaz=false,Signature=sig });
						return sig;
					}
				});
				checkResult.Expect(signature!="","A signature string was made");
				//Open the procedure as its double-click opens it.
				Ui.Opened opened=ui.Open("Chart > progress notes > double-click the D1110 with the signed note",() => {
					Control gridProg=(Control)Ui.GetField(controlChart,"gridProg");
					int row=FindRow(gridProg,x => x.Contains("D1110"));
					DoubleClickRowOnThread(gridProg,row);
				});
				if(!InspectWindow(ui,checkResult,opened,"FormProcEdit","UF1-before.png",close:false)) {
					return;
				}
				Form form=ui.GetForm(opened.Hwnd);
				checkResult.Expect(IsSignatureShownAndValid(ui,form),"The signed note shows as valid when the window opens");
				//The program's zoom (Setup > Zoom stores it in ComputerPrefs.LocalComputer.Zoom; every layout reads it), then a resize, which lays the
				//window out at the new zoom (FormODBase.OnResize).
				ui.Invoke(() => {
					ComputerPrefs.LocalComputer.Zoom=120;
					form.Bounds=new Rectangle(form.Left,form.Top,form.Width+60,form.Height+40);
				});
				ui.Invoke(() => Application.DoEvents());
				string fontNotes=ui.Invoke(() => { Control textNotes=(Control)Ui.GetField(form,"textNotes"); return textNotes.Font.Name+" "+textNotes.Font.SizeInPoints; });
				checkResult.Note("The note's font after the zoom: "+fontNotes);
				checkResult.Expect(IsSignatureShownAndValid(ui,form),"After the zoom change and the resize the signature is still shown and valid");
				ui.Invoke(() => {
					ComputerPrefs.LocalComputer.Zoom=zoomBefore;
					form.Bounds=new Rectangle(form.Left,form.Top,form.Width-60,form.Height-40);
				});
				checkResult.Expect(IsSignatureShownAndValid(ui,form),"After the zoom and the size go back the signature is still shown and valid");
				checkResult.AddShot(ui.Shot(opened.Hwnd,"UF1-after.png"),"FormProcEdit after the zoom and the resize");
				//Save: OK, as a click does.
				Ui.Posted posted=ui.Post("FormProcEdit > OK",() => ((Control)Ui.GetField(form,"butOK")).GetType().GetMethod("PerformClick").Invoke(Ui.GetField(form,"butOK"),null));
				checkResult.Expect(ui.WaitGone(opened.Hwnd,30000),"FormProcEdit saves and closes (OK)",posted.Description);
				ui.CloseStrayWindows(checkResult);
				//The saved note: the newest procnote row of the procedure holds the same note and signature, and the signature is valid for it.
				string[] arraySaved=ui.Invoke(() => {
					Procedure procedure=Procedures.GetOneProc(procNum,includeNote:true);
					string keyData=Procedures.GetSignatureKeyData(procedure,procedure.Note);
					using(SignatureBoxWrapper signatureBoxWrapper=new SignatureBoxWrapper()) {
						signatureBoxWrapper.FillSignature(procedure.SigIsTopaz,keyData,procedure.Signature);
						bool isInvalid=((Control)Ui.GetField(signatureBoxWrapper,"labelInvalidSig")).Visible;
						int countPoints=((SignatureBox)Ui.GetField(signatureBoxWrapper,"signatureBox")).NumberOfTabletPoints();
						return new[] { procedure.Note,procedure.Signature==signature ? "same" : "changed",isInvalid ? "invalid" : "valid",countPoints.ToString() };
					}
				});
				checkResult.Expect(arraySaved[0]==NOTE_UF1,"The saved note is the signed note",arraySaved[0]);
				checkResult.Expect(arraySaved[1]=="same","The saved signature is the one the note was signed with",arraySaved[1]);
				checkResult.Expect(arraySaved[2]=="valid" && arraySaved[3]!="0","The saved signature is valid for the saved note",arraySaved[2]+", "+arraySaved[3]+" points");
			}
			finally {
				ui.Invoke(() => {
					ComputerPrefs.LocalComputer.Zoom=zoomBefore;
					DataCore.NonQ("DELETE FROM procnote WHERE ProcNum="+POut.Long(procNum)+" AND ProcNoteNum>"+POut.Long(procNoteNumBefore));
				});
			}
		}

		///<summary>FormProcEdit's signature box shows a signature (points on the program's own box) and its "Invalid Signature" label is hidden.</summary>
		private static bool IsSignatureShownAndValid(Ui ui,Form form) {
			return ui.Invoke(() => {
				SignatureBoxWrapper signatureBoxWrapper=(SignatureBoxWrapper)Ui.GetField(form,"signatureBoxWrapper");
				bool isInvalid=((Control)Ui.GetField(signatureBoxWrapper,"labelInvalidSig")).Visible;
				int countPoints=((SignatureBox)Ui.GetField(signatureBoxWrapper,"signatureBox")).NumberOfTabletPoints();
				return !isInvalid && countPoints>0;
			});
		}
	}
}
