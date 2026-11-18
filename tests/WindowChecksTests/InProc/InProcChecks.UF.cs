// Group U's font checks UF1 and UF2 (package UI-2a-1 of docs/specs/ui-refresh-plan.md, section 9 "Content and signatures";
// docs/port-notes/UI-2a.md): a signed note in FormProcEdit stays valid after a change of the program's zoom, a resize of the window and a save
// (UF1), and after a plain open and save (UF2). The mapping of UI-2a-1 sets fonts only in InitializeLayoutManager, before Load and before the
// signature is filled, and the rebuild sets a font only when its size differs (Job 55866: setting a font of the same size fires TextChanged,
// which cleared the signature of a procedure or group note). A zoom change while the window is open does change the note box's size, and Open
// Dental's FormProcEdit then clears the signature (textNotes_TextChanged): UF1 reports that, an owner decision (UI-2a.md, "UF1: the signature
// check and a scale change"; tests/UiRefreshTests/SignatureFontChangeTests.cs shows it without a window). The checks need a shown window (the
// zoom and the resize lay it out through OnResize, the save is the Save button), so they run in the in-process session of the window checks on
// the scratch copy, opt-in, in the owner's connected session (UTests.UF1, UTests.UF2). The note and its signature are synthetic; the rows the
// checks add are deleted again, and the procedure's note is put back as it was.
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
		///<summary>The synthetic note UF1 and UF2 sign.</summary>
		private const string NOTE_UF1="Synthetic signed note for the font check UF1 (UI-2a-1).";

		///<summary>UF1: the signed note after a zoom change, a resize and a save.</summary>
		public static void CheckUF1(Ui ui,CheckResult checkResult) {
			CheckSignedNote(ui,checkResult,"UF1",isZoomAndResize:true);
		}

		///<summary>UF2: the signed note after a plain open and save (no zoom change, no resize): what UF1 checks without its scale change.</summary>
		public static void CheckUF2(Ui ui,CheckResult checkResult) {
			CheckSignedNote(ui,checkResult,"UF2",isZoomAndResize:false);
		}

		private static void CheckSignedNote(Ui ui,CheckResult checkResult,string id,bool isZoomAndResize) {
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
				//The signed note, as FormProcEdit saves one: the program's own signature box, its key data from the procedure and the note. The key data
				//holds the note's user (Procedures.GetSignatureKeyData: the note and Procedure.UserNum), and the window reads that user from the note row
				//(Procedures.GetOneProc with the note), so the user is the row's, Security.CurUser, as when that user signs in the window
				//(FormProcEdit.signatureBoxWrapper_SignatureChanged sets _procedure.UserNum before SaveSignature). Before this row the procedure has no
				//note, and its UserNum (not a database column) is 0.
				string signature=ui.Invoke(() => {
					Procedure procedure=Procedures.GetOneProc(procNum,includeNote:true);
					procedure.UserNum=Security.CurUser.UserNum;
					string keyData=Procedures.GetSignatureKeyData(procedure,NOTE_UF1);
					using(SignatureBox signatureBox=new SignatureBox()) {
						signatureBox.SetTabletState(1);
						signatureBox.SetPointList(new List<PointF> { new PointF(10,10),new PointF(40,30),new PointF(70,12),new PointF(100,35),new PointF(0,0),
							new PointF(20,40),new PointF(90,8) });
						signatureBox.SetKeyString("0000000000000000");
						signatureBox.SetAutoKeyData(keyData);
						string sig=signatureBox.GetSigString();
						ProcNotes.Insert(new ProcNote { PatNum=procedure.PatNum,ProcNum=procNum,EntryDateTime=DateTime.Now,UserNum=procedure.UserNum,
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
				if(!InspectWindow(ui,checkResult,opened,"FormProcEdit",id+"-before.png",close:false)) {
					return;
				}
				Form form=ui.GetForm(opened.Hwnd);
				string state=GetSignatureState(ui,form);
				checkResult.Expect(state==STATE_SHOWN_AND_VALID,"The signed note shows as valid when the window opens",state);
				if(isZoomAndResize) {
					//The program's zoom (Setup > Zoom stores it in ComputerPrefs.LocalComputer.Zoom; every layout reads it), then a resize, which lays the
					//window out at the new zoom (FormODBase.OnResize).
					ui.Invoke(() => {
						ComputerPrefs.LocalComputer.Zoom=120;
						form.Bounds=new Rectangle(form.Left,form.Top,form.Width+60,form.Height+40);
					});
					ui.Invoke(() => Application.DoEvents());
					string fontNotes=ui.Invoke(() => { Control textNotes=(Control)Ui.GetField(form,"textNotes"); return textNotes.Font.Name+" "+textNotes.Font.SizeInPoints; });
					checkResult.Note("The note's font after the zoom: "+fontNotes);
					state=GetSignatureState(ui,form);
					checkResult.Expect(state==STATE_SHOWN_AND_VALID,"After the zoom change and the resize the signature is still shown and valid",state);
					ui.Invoke(() => {
						ComputerPrefs.LocalComputer.Zoom=zoomBefore;
						form.Bounds=new Rectangle(form.Left,form.Top,form.Width-60,form.Height-40);
					});
					state=GetSignatureState(ui,form);
					checkResult.Expect(state==STATE_SHOWN_AND_VALID,"After the zoom and the size go back the signature is still shown and valid",state);
					checkResult.AddShot(ui.Shot(opened.Hwnd,id+"-after.png"),"FormProcEdit after the zoom and the resize");
				}
				//Save: the Save button (butSave), as a click does.
				Ui.Posted posted=ui.Post("FormProcEdit > Save",() => ((IButtonControl)Ui.GetField(form,"butSave")).PerformClick());
				checkResult.Expect(ui.WaitGone(opened.Hwnd,30000),"FormProcEdit saves and closes (Save)",posted.Description);
				ui.CloseStrayWindows(checkResult);
				//The saved note: the newest procnote row of the procedure holds the same note and signature, and the signature is valid for it.
				string[] arraySaved=ui.Invoke(() => {
					Procedure procedure=Procedures.GetOneProc(procNum,includeNote:true);
					string keyData=Procedures.GetSignatureKeyData(procedure,procedure.Note);
					using(SignatureBoxWrapper signatureBoxWrapper=new SignatureBoxWrapper()) {
						signatureBoxWrapper.FillSignature(procedure.SigIsTopaz,keyData,procedure.Signature);
						bool isInvalid=((Control)Ui.GetField(signatureBoxWrapper,"labelInvalidSig")).Visible;
						int countPoints=((SignatureBox)Ui.GetField(signatureBoxWrapper,"signatureBox")).NumberOfTabletPoints();
						return new[] { procedure.Note,procedure.Signature==signature ? "same" : (procedure.Signature=="" ? "changed: no signature" : "changed"),
							isInvalid ? "invalid" : "valid",countPoints.ToString() };
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

		///<summary>GetSignatureState's text for a signature that is shown (points on the program's own box) and valid.</summary>
		private const string STATE_SHOWN_AND_VALID="shown and valid";

		///<summary>FormProcEdit's signature box: STATE_SHOWN_AND_VALID when it shows a signature (points on the program's own box) and its "Invalid
		///Signature" label is hidden; otherwise what it shows, with whether the window took the signature as changed (_signatureChanged, which
		///its save writes) and whether the note box still holds the signed note.</summary>
		private static string GetSignatureState(Ui ui,Form form) {
			return ui.Invoke(() => {
				SignatureBoxWrapper signatureBoxWrapper=(SignatureBoxWrapper)Ui.GetField(form,"signatureBoxWrapper");
				bool isInvalid=((Control)Ui.GetField(signatureBoxWrapper,"labelInvalidSig")).Visible;
				int countPoints=((SignatureBox)Ui.GetField(signatureBoxWrapper,"signatureBox")).NumberOfTabletPoints();
				if(!isInvalid && countPoints>0) {
					return STATE_SHOWN_AND_VALID;
				}
				bool isSignatureChanged=(bool)Ui.GetField(form,"_signatureChanged");
				string note=((Control)Ui.GetField(form,"textNotes")).Text;
				return (isInvalid ? "invalid" : "valid")+", "+countPoints+" points, the window's _signatureChanged "+isSignatureChanged
					+", the note "+(note==NOTE_UF1 ? "unchanged" : "changed");
			});
		}
	}
}
