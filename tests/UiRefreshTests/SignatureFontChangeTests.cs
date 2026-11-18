// A signed procedure note in FormProcEdit and the layout's font changes (UF1 fix, docs/port-notes/UI-2a.md section "UF1: the signature check
// and a scale change"). FormProcEdit is built off screen as the scan builds it (WindowBuilder: the base constructor, InitializeComponent and
// InitializeLayoutManager, no Load, never shown), given a synthetic procedure, user and signed note the way FillControlsOnStartup fills them,
// and laid out as FormODBase does it (OnResize, and SetScaleMS on WM_DPICHANGED). The window handles are made so the note box raises its
// events as in the program, but the window is not shown. No database (the no-database guard refuses every connection).
// What the tests show:
// - a resize at the same scale, and the save path (SaveSignature), keep the signed note's signature: the plain open, resize and save of UF1/UF2;
// - a scale change while the window is open (another Windows scale when the window moves to another screen, or the program's zoom) sets
//   textNotes' font to its new size; the rich text box raises TextChanged for it, FormProcEdit.textNotes_TextChanged takes that for an edit of
//   the note and clears the signature, and the save then stores the note with no signature. That is Open Dental's code unchanged (24.3.1:
//   textNotes_TextChanged, LayoutManagerForms' font rebuild with Job 55866's same-size guard, FormODBase's WM_DPICHANGED), so the test pins
//   it as Open Dental's behaviour, an owner decision (UI-2a.md); the signature box's own rescale clears nothing (the second half of the test).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental;
using OpenPmsDental.UI;
using OpenPmsDentalBusiness;
using UiRefreshTests.Scan;

namespace UiRefreshTests {
	[TestClass]
	public class SignatureFontChangeTests {
		private const string NOTE="Synthetic signed note for the signature font test.";
		private const long USER_NUM=7;

		[TestMethod]
		public void FormProcEdit_SignedNote_ResizeAtTheSameScale_KeepsTheSignature_AndTheSaveKeepsIt() {
			StaRunner.Run(() => {
				using(BuiltWindow builtWindow=BuildSigned(out FormProcEdit formProcEdit,out string signature)) {
					AssertSignatureShownAndValid(formProcEdit,"after the fill");
					int countTextChanged=CountTextChanged(formProcEdit,() => {
						formProcEdit.Bounds=new Rectangle(formProcEdit.Left,formProcEdit.Top,formProcEdit.Width+60,formProcEdit.Height+40);
						builtWindow.LayoutManager.LayoutFormBoundsAndFonts(formProcEdit);
						formProcEdit.Bounds=new Rectangle(formProcEdit.Left,formProcEdit.Top,formProcEdit.Width-60,formProcEdit.Height-40);
						builtWindow.LayoutManager.LayoutFormBoundsAndFonts(formProcEdit);
					});
					Assert.AreEqual(0,countTextChanged,"A resize at the same scale sets no font of the note box (Job 55866's guard).");
					AssertSignatureShownAndValid(formProcEdit,"after the resize");
					Assert.IsFalse((bool)GetField(formProcEdit,"_signatureChanged"),"The signature is not changed.");
					//The save: SaveSignature keeps the stored signature, and the key data the save would use is the one the note was signed with.
					Procedure procedure=(Procedure)GetField(formProcEdit,"_procedure");
					Control textNotes=(Control)GetField(formProcEdit,"textNotes");
					Assert.AreEqual(Procedures.GetSignatureKeyData(procedure),Procedures.GetSignatureKeyData(procedure,textNotes.Text),"The note box holds the signed note.");
					InvokeNonPublic(formProcEdit,"SaveSignature");
					Assert.AreEqual(signature,procedure.Signature,"The save keeps the signature.");
				}
			});
		}

		[TestMethod]
		public void FormProcEdit_SignedNote_AScaleChange_ClearsTheSignature_OpenDentalBehaviour_OwnerDecision() {
			StaRunner.Run(() => {
				//Another Windows scale (WM_DPICHANGED: SetScaleMS, then the layout) and the program's zoom (the layout reads the zoom; ZoomTest stands for it).
				foreach(string scaleChange in new[] { "Windows scale 150%","program zoom 120%" }) {
					using(BuiltWindow builtWindow=BuildSigned(out FormProcEdit formProcEdit,out string signature)) {
						AssertSignatureShownAndValid(formProcEdit,scaleChange+", after the fill");
						Control textNotes=(Control)GetField(formProcEdit,"textNotes");
						float sizeBefore=textNotes.Font.SizeInPoints;
						int countTextChanged=CountTextChanged(formProcEdit,() => ChangeScale(builtWindow,scaleChange));
						Assert.AreNotEqual(sizeBefore,textNotes.Font.SizeInPoints,scaleChange+": the layout sets the note box's font to its new size.");
						Assert.IsTrue(countTextChanged>0,scaleChange+": the rich text box raises TextChanged for the new font.");
						Assert.AreEqual(NOTE,textNotes.Text,scaleChange+": the note itself is unchanged.");
						SignatureBoxWrapper signatureBoxWrapper=(SignatureBoxWrapper)GetField(formProcEdit,"signatureBoxWrapper");
						Assert.IsTrue((bool)GetField(formProcEdit,"_signatureChanged"),scaleChange+": textNotes_TextChanged took it for an edit of the note.");
						Assert.IsTrue(signatureBoxWrapper.SigIsBlank,scaleChange+": the signature is cleared (Open Dental's behaviour; owner decision, UI-2a.md).");
						Procedure procedure=(Procedure)GetField(formProcEdit,"_procedure");
						InvokeNonPublic(formProcEdit,"SaveSignature");
						Assert.AreEqual("",procedure.Signature,scaleChange+": the save then stores the note without its signature.");
					}
				}
				//The cause is the note box's TextChanged, not the signature box: without FormProcEdit's handler the same scale change keeps the signature.
				using(BuiltWindow builtWindow=BuildSigned(out FormProcEdit formProcEdit,out string signature)) {
					Control textNotes=(Control)GetField(formProcEdit,"textNotes");
					EventHandler eventHandler=(EventHandler)Delegate.CreateDelegate(typeof(EventHandler),formProcEdit,"textNotes_TextChanged");
					textNotes.TextChanged-=eventHandler;
					ChangeScale(builtWindow,"Windows scale 150%");
					AssertSignatureShownAndValid(formProcEdit,"after the scale change without textNotes_TextChanged");
					Assert.IsFalse((bool)GetField(formProcEdit,"_signatureChanged"));
					Procedure procedure=(Procedure)GetField(formProcEdit,"_procedure");
					InvokeNonPublic(formProcEdit,"SaveSignature");
					Assert.AreEqual(signature,procedure.Signature);
				}
			});
		}

		///<summary>Builds FormProcEdit off screen at 100%, makes the handles of the window and of the note box (not shown), and fills a synthetic
		///procedure's signed note as FillControlsOnStartup does: the note, then the signature with the key data of the procedure (the note and the
		///note's user, Procedures.GetSignatureKeyData), inside _isStartingUp.</summary>
		private static BuiltWindow BuildSigned(out FormProcEdit formProcEdit,out string signature) {
			ScanType scanType=new ScanType { Type=typeof(FormProcEdit),Kind=BuildKind.LayoutManagerWindow,Assembly=typeof(FormProcEdit).Assembly.GetName().Name,
				WindowKey=typeof(FormProcEdit).FullName };
			BuiltWindow builtWindow=WindowBuilder.Build(scanType);
			formProcEdit=(FormProcEdit)builtWindow.Form;
			Procedure procedure=new Procedure { ProcNum=1,PatNum=1,Note=NOTE,UserNum=USER_NUM };
			SetField(formProcEdit,"_procedure",procedure);
			SetField(formProcEdit,"_procedureOld",procedure.Copy());
			SetField(formProcEdit,"_Userod",new Userod { UserNum=USER_NUM,UserName="synthetic" });
			ODtextBox textNotes=(ODtextBox)GetField(formProcEdit,"textNotes");
			textNotes.SpellCheckIsEnabled=false;//its timer and scroll handlers would read the spell check preference
			IntPtr handleForm=formProcEdit.Handle;
			IntPtr handleNotes=textNotes.Handle;
			Assert.IsFalse(formProcEdit.Visible,"The window is not shown.");
			signature=MakeSignature(Procedures.GetSignatureKeyData(procedure));
			procedure.Signature=signature;
			SetField(formProcEdit,"_isStartingUp",true);
			textNotes.Text=procedure.Note;
			((Control)GetField(formProcEdit,"textUser")).Text="synthetic";
			SignatureBoxWrapper signatureBoxWrapper=(SignatureBoxWrapper)GetField(formProcEdit,"signatureBoxWrapper");
			signatureBoxWrapper.FillSignature(procedure.SigIsTopaz,Procedures.GetSignatureKeyData(procedure),procedure.Signature);
			SetField(formProcEdit,"_isStartingUp",false);
			return builtWindow;
		}

		private static void ChangeScale(BuiltWindow builtWindow,string scaleChange) {
			if(scaleChange.StartsWith("Windows")) {
				builtWindow.LayOutAt(1.5f);
				return;
			}
			builtWindow.LayoutManager.ZoomTest=120;
			try {
				builtWindow.LayOutAt(1f);
			}
			finally {
				builtWindow.LayoutManager.ZoomTest=0;
			}
		}

		///<summary>A signature from the program's own box for the key data, as the box makes it when a user signs (SignatureBoxWrapper.GetSignature).</summary>
		private static string MakeSignature(string keyData) {
			using(SignatureBox signatureBox=new SignatureBox()) {
				signatureBox.SetTabletState(1);
				signatureBox.SetPointList(new List<PointF> { new PointF(10,10),new PointF(40,30),new PointF(70,12),new PointF(100,35),new PointF(0,0),
					new PointF(20,40),new PointF(90,8) });
				signatureBox.SetKeyString("0000000000000000");
				signatureBox.SetAutoKeyData(keyData);
				return signatureBox.GetSigString();
			}
		}

		private static void AssertSignatureShownAndValid(FormProcEdit formProcEdit,string when) {
			SignatureBoxWrapper signatureBoxWrapper=(SignatureBoxWrapper)GetField(formProcEdit,"signatureBoxWrapper");
			Assert.IsTrue(signatureBoxWrapper.IsValid,"The signature is valid "+when+".");
			Assert.IsTrue(((SignatureBox)GetField(signatureBoxWrapper,"signatureBox")).NumberOfTabletPoints()>0,"The signature is shown "+when+".");
		}

		private static int CountTextChanged(FormProcEdit formProcEdit,Action action) {
			Control textNotes=(Control)GetField(formProcEdit,"textNotes");
			int count=0;
			EventHandler eventHandler=(sender,e) => count++;
			textNotes.TextChanged+=eventHandler;
			try {
				action();
			}
			finally {
				textNotes.TextChanged-=eventHandler;
			}
			return count;
		}

		private static FieldInfo FindField(object obj,string name) {
			for(Type type=obj.GetType();type!=null;type=type.BaseType) {
				FieldInfo fieldInfo=type.GetField(name,BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public|BindingFlags.DeclaredOnly);
				if(fieldInfo!=null) {
					return fieldInfo;
				}
			}
			throw new Exception("No field "+name+" in "+obj.GetType().Name);
		}

		private static object GetField(object obj,string name) {
			return FindField(obj,name).GetValue(obj);
		}

		private static void SetField(object obj,string name,object value) {
			FindField(obj,name).SetValue(obj,value);
		}

		private static void InvokeNonPublic(object obj,string name) {
			MethodInfo methodInfo=obj.GetType().GetMethod(name,BindingFlags.Instance|BindingFlags.NonPublic);
			try {
				methodInfo.Invoke(obj,null);
			}
			catch(TargetInvocationException targetInvocationException) {
				System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(targetInvocationException.InnerException).Throw();
			}
		}
	}
}
