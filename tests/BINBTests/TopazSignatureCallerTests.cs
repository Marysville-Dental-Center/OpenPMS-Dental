// Source scans of the callers of the signature box's reads (S3a-14g of the .NET 10 move, docs/port-notes/S3a-14.md, section S3a-14g): a
// Topaz signature the 32-bit helper lost throws a Helper32Exception from GetSignature, SigIsBlank and CheckTopazSignatureReachable. Every
// caller either checks before it writes or catches the exception, shows it and stops before it writes, so the program never ends on it
// (ProgramEntry's handler) and no record is written without the signature. The forms cannot run in a test (they need the program's database
// and windows), so these tests read their source; the behaviour of the box itself is TopazWrapperTests'.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BINBTests {
	[TestClass]
	public class TopazSignatureCallerTests {
		private const string CATCH_HELPER="catch(Helper32Exception ex) {";

		private static string Read(params string[] arrayParts) {
			return TestPaths.ReadSource(arrayParts).Replace("\r\n","\n");
		}

		///<summary>The text of the method that starts with signature in source, up to the next "\n\t\t}" at its own indent.</summary>
		private static string GetMethodText(string source,string signature) {
			int indexStart=source.IndexOf(signature,StringComparison.Ordinal);
			Assert.IsTrue(indexStart>=0,signature+" is in the source.");
			Assert.AreEqual(-1,source.IndexOf(signature,indexStart+1,StringComparison.Ordinal),signature+" is in the source once.");
			int indexEnd=source.IndexOf("\n\t\t}",indexStart,StringComparison.Ordinal);
			Assert.IsTrue(indexEnd>indexStart,"The end of "+signature);
			return source.Substring(indexStart,indexEnd-indexStart);
		}

		///<summary>The try block of text whose body holds call and whose first catch is catch(Helper32Exception ex): its catch body, which must
		///show ex.Message and hold textStop (the return that stops the save).</summary>
		private static string AssertInHelperTry(string text,string call,string textStop,string what) {
			List<Tuple<string,string>> listTryBlocks=TopazWrapperTests.GetHelper32TryBlocks(text);
			Tuple<string,string> tuple=listTryBlocks.Find(x => x.Item1.Contains(call));
			Assert.IsNotNull(tuple,what+": "+call+" is in a try whose first catch is "+CATCH_HELPER);
			StringAssert.Contains(tuple.Item2,"ex.Message",what+": the catch shows the message.");
			StringAssert.Contains(tuple.Item2,textStop,what+": the catch stops.");
			return tuple.Item2;
		}

		///<summary>In text, the first indexOf of before comes before the first of each of listAfter, every one of which must be there.</summary>
		private static void AssertBefore(string text,string before,string[] listAfter,string what) {
			int indexBefore=text.IndexOf(before,StringComparison.Ordinal);
			Assert.IsTrue(indexBefore>=0,what+": "+before+" is there.");
			foreach(string after in listAfter) {
				int indexAfter=text.IndexOf(after,StringComparison.Ordinal);
				Assert.IsTrue(indexAfter>=0,what+": "+after+" is there (update this test if the method changed).");
				Assert.IsTrue(indexBefore<indexAfter,what+": "+before+" comes before "+after);
			}
		}

		[TestMethod]
		public void EForms_UnchangedBoxesKeepTheirValue_AndALostSignatureNeverEndsTheProgram() {
			//S3a-14g: FillFieldsFromControls read every signature box, changed or not, and ignored its field filter for them: a stored Topaz
			//signature was saved as "1" once the helper had ended, and a lost signature signed in this session ended the program from a checkbox
			//click, a date box, Print, Save Image, the resize handler or Save.
			string sourceCtrl=Read("WpfControlsOD","UI","Controls","UserControls","CtrlEFormFill.xaml.cs");
			string textFill=GetMethodText(sourceCtrl,"public void FillFieldsFromControls(EFormField eFormField=null){");
			int indexSigLoop=textFill.IndexOf("//After pulling all other fields, we can pull signatures from sigboxes",StringComparison.Ordinal);
			Assert.IsTrue(indexSigLoop>=0,"The signature loop (update this test if the method changed).");
			string textSigLoop=textFill.Substring(indexSigLoop);
			int indexFilter=textSigLoop.IndexOf("if(eFormField!=null && ListEFormFields[i]!=eFormField){",StringComparison.Ordinal);
			int indexSigBox=textSigLoop.IndexOf("if(ListEFormFields[i].FieldType!=EnumEFormFieldType.SigBox){",StringComparison.Ordinal);
			Assert.IsTrue(indexFilter>=0 && indexFilter<indexSigBox,"The signature loop honours the field filter.");
			StringAssert.Contains(textSigLoop,"ListEFormFields[i].ValueString=OpenPmsDental.UI.SignatureBoxSaves.GetEFormSignatureValue(ListEFormFields[i].ValueString,");
			Assert.IsFalse(textFill.Contains(".GetSignature("),"FillFieldsFromControls reads no box itself (SignatureBoxSaves skips unchanged ones).");
			string textCheck=GetMethodText(sourceCtrl,"public void CheckTopazSignaturesReachable(){");
			StringAssert.Contains(textCheck,"if(signatureBoxWrapper.GetSigChanged() && signatureBoxWrapper.GetSigIsTopaz()){\n\t\t\t\t\tsignatureBoxWrapper.CheckTopazSignatureReachable();");
			//Every call with no filter (it reads the signatures) is FrmEFormFillEdit's, each in a try that catches the helper's failure and returns.
			Assert.AreEqual(0,Regex.Matches(sourceCtrl,@"FillFieldsFromControls\(\);").Count,"CtrlEFormFill calls it only with a field.");
			string sourceFrm=Read("WpfControlsOD","Frms","FrmEFormFillEdit.xaml.cs");
			List<Tuple<string,string>> listTryBlocks=TopazWrapperTests.GetHelper32TryBlocks(sourceFrm);
			int countCalls=0;
			foreach(Match match in Regex.Matches(sourceFrm,@"\n(\t+)ctrlEFormFill\.FillFieldsFromControls\(\);")) {
				countCalls++;
				int indexTry=sourceFrm.LastIndexOf("try {\n",match.Index,StringComparison.Ordinal);
				Assert.IsTrue(indexTry>=0,"A try before the call at "+match.Index);
				string textBetween=sourceFrm.Substring(indexTry,match.Index-indexTry);
				Assert.IsTrue(Regex.IsMatch(textBetween,@"^try \{\n(\t+ctrlEFormFill\.CheckTopazSignaturesReachable\(\);\n)?$"),
					"The call at "+match.Index+" is first in its try: "+textBetween);
				Assert.IsTrue(listTryBlocks.Exists(x => x.Item1.Contains("FillFieldsFromControls();")),"The try's first catch is "+CATCH_HELPER);
			}
			Assert.AreEqual(4,countCalls,"Print, Save Image, the resize handler and the save (update this test if that changed).");
			Assert.AreEqual(4,listTryBlocks.FindAll(x => x.Item1.Contains("FillFieldsFromControls();")).Count,"Each call has its own try.");
			string textSave=GetMethodText(sourceFrm,"private void TryToSaveData() {");
			string textCatchSave=AssertInHelperTry(textSave,"ctrlEFormFill.CheckTopazSignaturesReachable();","return;","FrmEFormFillEdit.TryToSaveData");
			StringAssert.Contains(textCatchSave,"_wasError=true;");
			AssertBefore(textSave,"ctrlEFormFill.CheckTopazSignaturesReachable();",new[] { "ctrlEFormFill.FillFieldsFromControls();","EForms.Insert(",
				"EForms.Update(","EFormFields.DeleteForForm(","EFormFields.Insert(","SecurityLogs.MakeLogEntry(" },"FrmEFormFillEdit.TryToSaveData");
			foreach(string signature in new[] { "private void butPrint_Click(object sender,EventArgs e) {","private void butSaveImage_Click(object sender,EventArgs e) {",
				"private void SetCtrlWidth(){" })
			{
				string textMethod=GetMethodText(sourceFrm,signature);
				Tuple<string,string> tuple=TopazWrapperTests.GetHelper32TryBlocks(textMethod).Find(x => x.Item1.Contains("FillFieldsFromControls();"));
				Assert.IsNotNull(tuple,signature);
				StringAssert.Contains(tuple.Item2,"return;",signature+": the catch returns.");
			}
		}

		[TestMethod]
		public void FormPayConnect2_ALostSignatureAfterTheChargeStillRecordsThePayment() {
			//S3a-14g: SendSignature runs after the card was charged; a lost Topaz signature ended the program before the payment was recorded.
			string source=Read("OpenPmsDental","Forms","FormPayConnect2.cs");
			string textSend=GetMethodText(source,"private PayConnect2Response SendSignature() {");
			AssertInHelperTry(textSend,"sigBoxWrapper.GetSignature(\"\")","return null;","FormPayConnect2.SendSignature");
			AssertInHelperTry(textSend,"sigBoxWrapper.GetSigImage()","return null;","FormPayConnect2.SendSignature");
			Assert.AreEqual(1,Regex.Matches(textSend,@"\.GetSignature\(").Count);
		}

		[TestMethod]
		public void FormOrthoChart_ALostSignatureNeverEndsTheProgram_AndStopsTheSaveFirst() {
			//S3a-14g: SaveSignatureToDict's GetSignature threw from the grid's cell events, the lock timer, OK and FormClosing, and the program ended.
			string source=Read("OpenPmsDental","Forms","FormOrthoChart.cs");
			string textDict=GetMethodText(source,"private void SaveSignatureToDict(int idxRow) {");
			AssertBefore(textDict,"if(orthoSignature.IsTopaz && !_doesTopazNeedSave) {",new[] { ".GetSignature(" },
				"SaveSignatureToDict: a Topaz box not signed again is not read");
			//Every call of SaveSignatureToDict but SaveToDb's is alone in a try that catches the helper's failure.
			List<Tuple<string,string>> listTryBlocks=TopazWrapperTests.GetHelper32TryBlocks(source);
			int countCalls=0;
			foreach(Match match in Regex.Matches(source,@"\n\t+SaveSignatureToDict\([^)]*\);")) {
				countCalls++;
				string textCall=match.Value.Trim();
				int indexMethod=source.LastIndexOf("\n\t\tprivate ",match.Index,StringComparison.Ordinal);
				if(source.Substring(indexMethod).StartsWith("\n\t\tprivate void SaveToDb(",StringComparison.Ordinal)) {
					continue;
				}
				Assert.IsTrue(listTryBlocks.Exists(x => x.Item1.Trim()==textCall),"FormOrthoChart: "+textCall+" at "+match.Index+" is alone in a try with "+CATCH_HELPER);
			}
			Assert.AreEqual(5,countCalls,"Five calls (update this test if that changed).");
			//SaveToDb reads the signature before it writes; every caller catches its failure or put the signature in first.
			string textSaveToDb=GetMethodText(source,"private void SaveToDb(bool isSignatureInDict=false){");
			AssertBefore(textSaveToDb,"SaveSignatureToDict(gridMain.SelectedCell.Y);",new[] { "OrthoCharts.Sync(","OrthoChartRows.Sync(" },"FormOrthoChart.SaveToDb");
			int countSaves=0;
			foreach(Match match in Regex.Matches(source,@"\n\t+SaveToDb\([^)]*\);")) {
				countSaves++;
				if(match.Value.Contains("isSignatureInDict:true")) {
					continue;
				}
				Assert.IsTrue(listTryBlocks.Exists(x => x.Item1.Contains(match.Value.Trim())),"FormOrthoChart: SaveToDb at "+match.Index+" is in a try with "+CATCH_HELPER);
			}
			Assert.AreEqual(3,countSaves,"The lock timer, OK and FormClosing (update this test if that changed).");
			string textOk=GetMethodText(source,"private void butSave_Click(object sender,EventArgs e) {");
			AssertInHelperTry(textOk,"SaveSignatureToDict(gridMain.SelectedCell.Y);","return;","FormOrthoChart.butSave_Click");
			AssertBefore(textOk,"SaveSignatureToDict(gridMain.SelectedCell.Y);",new[] { "PatientNotes.SetUserNumOrthoLocked(","SaveToDb(isSignatureInDict:true);" },
				"FormOrthoChart.butSave_Click");
		}

		[TestMethod]
		public void FormTPsign_DocumentForms_ALostSignatureStopsTheSaveBeforeTheWrite() {
			//S3a-14g: FormTPsign, FrmDocSign and FrmChartLetterProperties read the signature (GetSignature, SigIsBlank) with no catch: the program
			//ended. Each checks and catches before it writes.
			string sourceTp=Read("OpenPmsDental","Forms","FormTPsign.cs");
			string textCheckTp=GetMethodText(sourceTp,"private bool AreTopazSignaturesReachable(bool isSigChanged,bool isSigPracticeChanged) {");
			StringAssert.Contains(textCheckTp,"signatureBoxWrapper.CheckTopazSignatureReachable();");
			StringAssert.Contains(textCheckTp,"signatureBoxWrapperPractice.CheckTopazSignatureReachable();");
			StringAssert.Contains(textCheckTp,CATCH_HELPER);
			string textSaveTp=GetMethodText(sourceTp,"private void butSave_Click(object sender,EventArgs e) {");
			StringAssert.Contains(textSaveTp,"if(!AreTopazSignaturesReachable(isSigChanged,isSigPracticeChanged)) {\n\t\t\t\treturn;");
			AssertInHelperTry(textSaveTp,"SaveSignature();","return;","FormTPsign.butSave_Click");
			AssertBefore(textSaveTp,"if(!AreTopazSignaturesReachable(",new[] { "SaveSignature();","TreatPlans.Update(" },"FormTPsign.butSave_Click");
			AssertBefore(textSaveTp,"SaveSignature();",new[] { "TreatPlans.Update(","SecurityLogs.MakeLogEntry(" },"FormTPsign.butSave_Click");
			string sourceDoc=Read("WpfControlsOD","Frms","FrmDocSign.xaml.cs");
			string textSaveDoc=GetMethodText(sourceDoc,"private void butSave_Click(object sender, System.EventArgs e){");
			AssertInHelperTry(textSaveDoc,"signatureBoxWrapper.CheckTopazSignatureReachable();","return;","FrmDocSign.butSave_Click");
			AssertInHelperTry(textSaveDoc,"SaveSignature();","return;","FrmDocSign.butSave_Click");
			AssertBefore(textSaveDoc,"SaveSignature();",new[] { "Documents.Update(","ImageSignaturePermissions.LogSignatureChange(" },"FrmDocSign.butSave_Click");
			string sourceLetter=Read("WpfControlsOD","Frms","FrmChartLetterProperties.xaml.cs");
			string textSaveLetter=GetMethodText(sourceLetter,"private void butSave_Click(object sender,EventArgs e) {");
			AssertInHelperTry(textSaveLetter,"signatureBoxWrapper.SigIsBlank","return;","FrmChartLetterProperties.butSave_Click");
			AssertInHelperTry(textSaveLetter,"signatureBoxWrapper.GetSignature(","return;","FrmChartLetterProperties.butSave_Click");
			AssertBefore(textSaveLetter,"signatureBoxWrapper.CheckTopazSignatureReachable();",new[] { "signatureBoxWrapper.SigIsBlank","ChartLetters.UpdateProperties(" },
				"FrmChartLetterProperties.butSave_Click");
		}

		[TestMethod]
		public void ProcedureForms_DialogsAndAlerts_ALostSignatureStopsBeforeTheFirstWrite() {
			//S3a-14g: FormProcGroup's merge window and the forms' "has not been re-signed" questions are dialogs, during which the helper can end;
			//after them SigIsBlank ended the program, or the "signature cleared" alerts were written before the save's check stopped it.
			string sourceGroup=Read("OpenPmsDental","Forms","FormProcGroup.cs");
			const string CHECK_GROUP="if(!AreTopazSignaturesReachable()) {";
			string textMerge=GetMethodText(sourceGroup,"private bool TryResolveNoteMerge() {");
			int indexDialog=textMerge.IndexOf("frmProcNoteMerge.ShowDialog();",StringComparison.Ordinal);
			int indexCheckMerge=textMerge.IndexOf(CHECK_GROUP+"\n\t\t\t\treturn false;",StringComparison.Ordinal);
			Assert.IsTrue(indexDialog>=0 && indexCheckMerge>indexDialog,"TryResolveNoteMerge checks after the merge window.");
			foreach(string after in new[] { "KeepTheirNote(","ProcNoteMerges.TakeAsBaseline(",".ClearSignature()" }) {
				int indexAfter=textMerge.IndexOf(after,indexDialog,StringComparison.Ordinal);
				Assert.IsTrue(indexAfter>indexCheckMerge,"TryResolveNoteMerge checks before "+after);
			}
			string textSaveGroup=GetMethodText(sourceGroup,"private void butSave_Click(object sender,System.EventArgs e) {");
			AssertCheckedAfterTheLastDialog(textSaveGroup,CHECK_GROUP,new[] { "TryResolveNoteMerge()","ConfirmSecondSignatureRemoved()","MsgBox.Show(" },
				new[] { "AlertItems.Insert(","AlertSecondSignerCleared();","SaveAndClose();" },"FormProcGroup.butSave_Click");
			string textLockGroup=GetMethodText(sourceGroup,"private void butLock_Click(object sender,EventArgs e) {");
			AssertCheckedAfterTheLastDialog(textLockGroup,CHECK_GROUP,new[] { "TryResolveNoteMerge()" },new[] { "ProcedureGroup.IsLocked=true;","SaveAndClose();" },
				"FormProcGroup.butLock_Click");
			string textCloseGroup=GetMethodText(sourceGroup,"private void SaveAndClose() {");
			AssertBefore(textCloseGroup,"if(!TryReadSignaturesBeforeWrites(",new[] { "ProcGroupItems.SyncForGroup(","SaveSignature();","Procedures.Update(" },
				"FormProcGroup.SaveAndClose");
			StringAssert.Contains(textCloseGroup,"if(!TryReadSignaturesBeforeWrites(_canEditStructure && (listProceduresAdded.Count>0 || listProceduresRemoved.Count>0))) {\n\t\t\t\treturn;");
			string textReadGroup=GetMethodText(sourceGroup,"private bool TryReadSignaturesBeforeWrites(bool isSyncingLinks) {");
			AssertInHelperTry(textReadGroup,"_signatureRead=signatureBoxWrapper.GetSignature(keyData);","return false;","FormProcGroup.TryReadSignaturesBeforeWrites");
			AssertInHelperTry(textReadGroup,"_signature2Read=signatureBoxWrapper2.GetSignature(keyData2);","return false;","FormProcGroup.TryReadSignaturesBeforeWrites");
			StringAssert.Contains(GetMethodText(sourceGroup,"private void SaveSignature(){"),"if(_signatureRead!=null && keyData==_keyDataSignatureRead) {");
			StringAssert.Contains(GetMethodText(sourceGroup,"private void SaveSignature2() {"),"if(_signature2Read!=null && keyData2==_keyDataSignature2Read) {");
			//FormProcEdit: the check after the question and before the alert; the signature read before the claimprocs, pay splits and adjustments.
			string sourceEdit=Read("OpenPmsDental","Forms","FormProcEdit.cs");
			string textSaveEdit=GetMethodText(sourceEdit,"private void butSave_Click(object sender,System.EventArgs e) {");
			AssertCheckedAfterTheLastDialog(textSaveEdit,"if(!IsTopazSignatureReachable()) {",new[] { "EntriesAreValid()","MsgBox.Show(" },
				new[] { "AlertItems.Insert(","SaveAndClose();" },"FormProcEdit.butSave_Click");
			string textCloseEdit=GetMethodText(sourceEdit,"private void SaveAndClose() {");
			StringAssert.Contains(textCloseEdit,"if(!TryReadSignatureBeforeWrites()) {");
			AssertBefore(textCloseEdit,"if(!TryReadSignatureBeforeWrites()) {",new[] { "ClaimProcs.Update(","PaySplits.UpdateAttachedPaySplits(","Adjustments.Update(",
				"SaveSignature();","Procedures.Update(" },"FormProcEdit.SaveAndClose");
			int indexNoteMerge=textCloseEdit.IndexOf("NoteMerge_TryResolve()",StringComparison.Ordinal);
			Assert.IsTrue(indexNoteMerge>=0 && indexNoteMerge<textCloseEdit.IndexOf("if(!TryReadSignatureBeforeWrites()) {",StringComparison.Ordinal),
				"FormProcEdit reads the signature after the merge window.");
			string textReadEdit=GetMethodText(sourceEdit,"private bool TryReadSignatureBeforeWrites() {");
			AssertInHelperTry(textReadEdit,"_signatureRead=signatureBoxWrapper.GetSignature(keyData);","return false;","FormProcEdit.TryReadSignatureBeforeWrites");
			StringAssert.Contains(GetMethodText(sourceEdit,"private void SaveSignature(){"),"if(_signatureRead!=null && keyData==_keyDataSignatureRead) {");
			//FormProcNoteAppend: the signature is read before the alert is written.
			string sourceAppend=Read("OpenPmsDental","Forms","FormProcNoteAppend.cs");
			string textSaveAppend=GetMethodText(sourceAppend,"private void butSave_Click(object sender,EventArgs e) {");
			AssertBefore(textSaveAppend,"SaveSignature();",new[] { "AlertItems.Insert(","Procedures.Update(" },"FormProcNoteAppend.butSave_Click");
		}

		///<summary>In text, a check (check, followed by a return) comes after the last of listDialogs and before each of listWrites, with no dialog
		///(MsgBox.Show, ShowDialog) between the check and the first write.</summary>
		private static void AssertCheckedAfterTheLastDialog(string text,string check,string[] listDialogs,string[] listWrites,string what) {
			int indexLastDialog=-1;
			foreach(string dialog in listDialogs) {
				int index=text.LastIndexOf(dialog,StringComparison.Ordinal);
				Assert.IsTrue(index>=0,what+": "+dialog+" is there (update this test if the method changed).");
				indexLastDialog=Math.Max(indexLastDialog,index);
			}
			int indexCheck=text.IndexOf(check,indexLastDialog,StringComparison.Ordinal);
			Assert.IsTrue(indexCheck>indexLastDialog,what+": "+check+" comes after the last dialog.");
			Assert.IsTrue(Regex.IsMatch(text.Substring(indexCheck),@"^if\(![A-Za-z]+\(\)\) \{\n\t+return;"),what+": the failed check returns.");
			int indexFirstWrite=int.MaxValue;
			foreach(string write in listWrites) {
				int indexWrite=text.IndexOf(write,indexLastDialog,StringComparison.Ordinal);
				Assert.IsTrue(indexWrite>indexCheck,what+": the check comes before "+write);
				indexFirstWrite=Math.Min(indexFirstWrite,indexWrite);
			}
			string textBetween=text.Substring(indexCheck,indexFirstWrite-indexCheck);
			Assert.IsFalse(textBetween.Contains("MsgBox.Show(") || textBetween.Contains("ShowDialog(") || textBetween.Contains("MessageBox.Show("),
				what+": no dialog between the check and the first write: "+textBetween);
		}
	}
}
