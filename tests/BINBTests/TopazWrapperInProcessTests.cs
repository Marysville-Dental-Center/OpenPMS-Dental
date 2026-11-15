// Topaz in the program's own process, the native-first path (S3a-14n of the .NET 10 move, docs/port-notes/S3a-14.md, section "S3a-14n: native
// first"): TopazWrapper asks NativeFirstChooser.Topaz about the SigPlusNET.dll it found, and in this x86 test process (as in the x86 program)
// the chooser picks the program's process. The synthetic stand-in (tests\TopazStandIn, never Topaz's file) is then loaded into this process,
// as main loaded Topaz's file before S3a-14, and these are main's TopazWrapperTests of before S3a-14 (commit 7b1c76c55), with the checks
// that the in-process path is the one taken and that the S3a-14f/14g steps of the helper's path stay out of it. TopazWrapperTests runs the
// fallback path (a copy of the helper) with its own folder; this class has its own folder, so the stand-in each path loads is told apart.
// No window is shown and no helper is started here.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Cecil;
using OpenPmsDental.UI;
using OpenPmsDentalBusiness;

namespace BINBTests {
	[TestClass]
	public class TopazWrapperInProcessTests {
		private static string _folderFake;
		private static string _folderEmpty;
		private const string KEY_DATA="synthetic treatment plan key data";
		private const string SIG_TOPAZ_SHAPED="0A1B2C3D4E5F60718293A4B5C6D7E8F9";

		[ClassInitialize]
		public static void ClassInitialize(TestContext testContext) {
			_folderEmpty=TestPaths.NewTempFolder("topaz_inprocess_none");
			_folderFake=TestPaths.NewTempFolder("topaz_inprocess_fake");
			WriteStandIn(Path.Combine(_folderFake,TopazWrapper.ASSEMBLY_FILE_NAME));
		}

		///<summary>The stand-in as SigPlusNET.dll under the assembly name SigPlusNET (Mono.Cecil), as TopazWrapperTests writes it.</summary>
		private static void WriteStandIn(string path) {
			string pathStandIn=Path.Combine(Path.GetDirectoryName(typeof(TopazWrapperInProcessTests).Assembly.Location),"TopazStandIn.dll");
			Assert.IsTrue(File.Exists(pathStandIn),pathStandIn+" is not built (tests\\TopazStandIn, which BINBTests references).");
			using(MemoryStream memoryStream=new MemoryStream(File.ReadAllBytes(pathStandIn)))
			using(AssemblyDefinition assemblyDefinition=AssemblyDefinition.ReadAssembly(memoryStream)) {
				assemblyDefinition.Name.Name=Path.GetFileNameWithoutExtension(TopazWrapper.ASSEMBLY_FILE_NAME);
				assemblyDefinition.MainModule.Name=TopazWrapper.ASSEMBLY_FILE_NAME;
				assemblyDefinition.Write(path);
			}
		}

		[ClassCleanup]
		public static void ClassCleanup() {
			TopazWrapper.ResetLoader();
		}

		///<summary>Only these folders are searched; no helper client is given, so Topaz's choice is made again from the file found, as in a new
		///program process.</summary>
		private static void Reset(params string[] arrayFolders) {
			TopazWrapper.ResetLoader(arrayFolders.ToList());
		}

		///<summary>The calls the stand-in recorded, in this process (its static Calls list), as main's test read them.</summary>
		private static List<string> GetCalls(Control control) {
			return (List<string>)control.GetType().GetField("Calls").GetValue(null);
		}

		private static Label GetLabel(SignatureBoxWrapper signatureBoxWrapper) {
			return (Label)signatureBoxWrapper.Controls.Find("labelInvalidSig",true).Single();
		}

		private static Control GetSigBoxTopaz(SignatureBoxWrapper signatureBoxWrapper) {
			return signatureBoxWrapper.Controls.Find("sigBoxTopaz",true).Single();
		}

		[TestMethod]
		public void Chooser_PicksTheProgramsProcess_ForTheStandInInThisX86Process() {
			Reset(_folderFake);
			Assert.IsFalse(Environment.Is64BitProcess,"BINBTests runs as x86, as the program does in 3a.");
			Assert.IsTrue(TopazWrapper.IsInstalled());
			Assert.AreEqual(VendorPath.InProcess,NativeFirstChooser.Topaz.Choice);
			//The stand-in is built x86 (32BITREQUIRED): a 32-bit process runs it itself; a 64-bit one would use the helper.
			StringAssert.Contains(NativeFirstChooser.Topaz.Reason,"a managed IL-only assembly that requires 32-bit (x86)");
			StringAssert.StartsWith(NativeFirstChooser.Topaz.LogLine,"Topaz SigPlusNET.dll: in the program's process (32-bit process); ");
			//No SigPlusNET.dll found: nothing to choose, and no choice is made.
			Reset(_folderEmpty);
			Assert.IsFalse(TopazWrapper.IsInstalled());
			Assert.IsNull(NativeFirstChooser.Topaz.Choice);
		}

		[TestMethod]
		public void Installed_LoadsTheControlFromASearchFolderAtRunTime_InThisProcess() {
			Reset(_folderEmpty,_folderFake);
			string pathExpected=Path.Combine(_folderFake,TopazWrapper.ASSEMBLY_FILE_NAME);
			Assert.AreEqual(pathExpected,TopazWrapper.FindAssemblyPath());
			Assert.IsTrue(TopazWrapper.IsInstalled());
			using(Control control=TopazWrapper.GetTopaz()) {
				Assert.AreEqual(TopazWrapper.CONTROL_TYPE_NAME,control.GetType().FullName,"Topaz's own control, as before S3a-14.");
				Assert.IsFalse(control is TopazSignatureView);
				Assert.IsFalse(TopazWrapper.IsInHelper(control));
				Assert.AreEqual(Path.GetFullPath(pathExpected).ToLowerInvariant(),Path.GetFullPath(control.GetType().Assembly.Location).ToLowerInvariant());
			}
		}

		[TestMethod]
		public void Installed_WrapperMakesTheSameCallsAsTheCompiledCode() {
			Reset(_folderFake);
			using(Control control=TopazWrapper.GetTopaz()) {
				List<string> listCalls=GetCalls(control);
				listCalls.Clear();
				TopazWrapper.ClearTopaz(control);
				TopazWrapper.SetTopazCompressionMode(control,2);
				TopazWrapper.SetTopazEncryptionMode(control,3);
				TopazWrapper.SetTopazKeyString(control,"0000000000000000");
				TopazWrapper.SetTopazAutoKeyData(control,"key one");
				TopazWrapper.SetTopazAutoKeyANSIData(control,"key two");
				TopazWrapper.SetTopazState(control,1);
				TopazWrapper.SetTopazSigString(control,"valid");
				CollectionAssert.AreEqual(new[] { "ClearTablet","SetTabletLogicalXSize:2000","SetTabletLogicalYSize:600","SetSigCompressionMode:2",
					"SetEncryptionMode:3","SetKeyString:0000000000000000","AutoKeyStart","SetAutoKeyData:key one","AutoKeyFinish","AutoKeyStart",
					"SetAutoKeyANSIData:key two","AutoKeyFinish","SetTabletState:1","SetSigString:valid" },listCalls);
				Assert.AreEqual(1,TopazWrapper.GetTopazState(control));
				Assert.AreEqual(250,TopazWrapper.GetTopazNumberOfTabletPoints(control));
				Assert.AreEqual("valid",TopazWrapper.GetTopazString(control));
				TopazWrapper.SetTopazSigString(control,"throw");//Topaz throws for a corrupt signature; the wrapper swallows it, as before.
				Assert.AreEqual("SetSigString:throw",listCalls.Last());
			}
		}

		[TestMethod]
		public void Installed_FillSignatureEncodings_UsesTheControlsPrivateKeyMethod() {
			Reset(_folderFake);
			using(Control control=TopazWrapper.GetTopaz()) {
				List<string> listCalls=GetCalls(control);
				listCalls.Clear();
				TopazWrapper.FillSignatureEncodings(control,"key","needs-bytes",SignatureBoxWrapper.SigMode.Default);
				Assert.AreEqual(250,TopazWrapper.GetTopazNumberOfTabletPoints(control));
				Assert.AreEqual(1,listCalls.Count(x => x=="Sig.SetAutoKeyData"),"Stops at the first encoding that shows the signature.");
			}
		}

		[TestMethod]
		public void Installed_SignatureBoxShowsATopazSignature() {
			Reset(_folderFake);
			using(SignatureBoxWrapper signatureBoxWrapper=new SignatureBoxWrapper()) {
				Label labelInvalidSig=GetLabel(signatureBoxWrapper);
				signatureBoxWrapper.FillSignature(true,"synthetic key data","valid");
				Assert.AreEqual(250,signatureBoxWrapper.GetNumberOfTabletPoints(true));
				Assert.AreEqual(signatureBoxWrapper.LabelText,labelInvalidSig.Text);
				Assert.AreNotEqual(TopazWrapper.NOT_INSTALLED_MESSAGE,labelInvalidSig.Text);
				Assert.IsFalse(TopazWrapper.IsInHelper(GetSigBoxTopaz(signatureBoxWrapper)));
				Assert.IsFalse(signatureBoxWrapper.IsTopazInHelper());
			}
		}

		[TestMethod]
		public void Installed_TreatPlanTriesTheOtherKind_InvalidAsBefore() {
			Reset(_folderFake);
			using(SignatureBoxWrapper signatureBoxWrapper=new SignatureBoxWrapper()) {
				signatureBoxWrapper.SignatureMode=SignatureBoxWrapper.SigMode.TreatPlan;
				Label labelInvalidSig=GetLabel(signatureBoxWrapper);
				string textOwn=labelInvalidSig.Text;
				signatureBoxWrapper.FillSignature(true,KEY_DATA,SIG_TOPAZ_SHAPED);
				Assert.IsFalse(signatureBoxWrapper.IsValid);
				Assert.AreEqual(textOwn,labelInvalidSig.Text);
				signatureBoxWrapper.FillSignature(false,KEY_DATA,SIG_TOPAZ_SHAPED);
				Assert.IsFalse(signatureBoxWrapper.IsValid);
				Assert.AreEqual(textOwn,labelInvalidSig.Text);
			}
		}

		[TestMethod]
		public void TopazButton_SignsIntoTheBoxItself_AsBefore_NoSigningWindow() {
			//The patient signs into the box in the program's window: the button clears the pad, sets the mode and turns capture on, and the box
			//takes the focus, as before S3a-14. The helper's signing window (ShowSigningWindow) belongs to the fallback path only.
			Reset(_folderFake);
			using(SignatureBoxWrapper signatureBoxWrapper=new SignatureBoxWrapper()) {
				Control sigBoxTopaz=GetSigBoxTopaz(signatureBoxWrapper);
				List<string> listCalls=GetCalls(sigBoxTopaz);
				listCalls.Clear();
				typeof(SignatureBoxWrapper).GetMethod("butTopazSign_Click",BindingFlags.NonPublic|BindingFlags.Instance)
					.Invoke(signatureBoxWrapper,new object[] { null,EventArgs.Empty });
				CollectionAssert.AreEqual(new[] { "ClearTablet","SetTabletLogicalXSize:2000","SetTabletLogicalYSize:600","SetEncryptionMode:0",
					"SetTabletState:1" },listCalls,"Capture stays on until the focus leaves the box (sigBoxTopaz_Leave).");
				Assert.IsTrue(sigBoxTopaz.Visible);
				Assert.IsTrue(signatureBoxWrapper.GetSigChanged());
				Assert.IsFalse(GetLabel(signatureBoxWrapper).Visible);
				Assert.ThrowsExactly<ArgumentException>(() => TopazWrapper.ShowSigningWindow(sigBoxTopaz,signatureBoxWrapper),
					"No signing window for Topaz's own control.");
				//The focus leaving the box stops capture, as before.
				typeof(SignatureBoxWrapper).GetMethod("sigBoxTopaz_Leave",BindingFlags.NonPublic|BindingFlags.Instance)
					.Invoke(signatureBoxWrapper,new object[] { sigBoxTopaz,EventArgs.Empty });
				Assert.AreEqual("SetTabletState:0",listCalls.Last());
			}
		}

		[TestMethod]
		public void FilledBox_GetSignatureReadsTheControl_AsBefore_NoHeldSignature() {
			//S3a-14g's held signature (returned without asking the helper) is the helper path's only: Topaz's own control keeps the signature as long
			//as the box, so GetSignature reads it back from the control, with the calls of before S3a-14.
			Reset(_folderFake);
			using(SignatureBoxWrapper signatureBoxWrapper=new SignatureBoxWrapper()) {
				signatureBoxWrapper.FillSignature(true,"synthetic key data","valid");
				Control sigBoxTopaz=GetSigBoxTopaz(signatureBoxWrapper);
				TopazWrapper.MarkTopazSignatureFilled(sigBoxTopaz);//does nothing for Topaz's own control
				List<string> listCalls=GetCalls(sigBoxTopaz);
				listCalls.Clear();
				Assert.IsFalse(signatureBoxWrapper.SigIsBlank);
				Assert.AreEqual("valid",signatureBoxWrapper.GetSignature("synthetic key data"));
				CollectionAssert.AreEqual(new[] { "SetKeyString:0000000000000000","AutoKeyStart","SetAutoKeyData:synthetic key data","AutoKeyFinish",
					"SetEncryptionMode:2","SetSigCompressionMode:2","GetSigString" },listCalls);
				signatureBoxWrapper.CheckTopazSignatureReachable();//reads the point count only
				//The forms' S3a-14g step keeps a stored signature only for a Topaz box on the helper's path: this box is read as before.
				Assert.IsFalse(SignatureBoxSaves.IsStoredSignatureKept(signatureBoxWrapper));
				PayPlan payPlan=new PayPlan { Signature="stored",SigIsTopaz=false };
				Assert.IsTrue(SignatureBoxSaves.SetPayPlanSignature(payPlan,signatureBoxWrapper,"synthetic key data"));
				Assert.AreEqual("valid",payPlan.Signature);
				Assert.IsTrue(payPlan.SigIsTopaz);
				Assert.AreEqual("1valid",SignatureBoxSaves.GetEFormSignatureValue("1stored",signatureBoxWrapper,"synthetic key data"));
			}
		}

		[TestMethod]
		public void ProgramsOwnBox_IsReadAsBefore_InTheFormsSaveSteps() {
			//An unchanged box of the program's own (no Topaz): read again on the payment plan's and the eForm's save, as before S3a-14g.
			Reset(_folderEmpty);
			using(SignatureBoxWrapper signatureBoxWrapper=new SignatureBoxWrapper()) {
				Assert.IsFalse(signatureBoxWrapper.GetSigChanged());
				Assert.IsFalse(signatureBoxWrapper.IsTopazInHelper());
				Assert.IsFalse(SignatureBoxSaves.IsStoredSignatureKept(signatureBoxWrapper));
				PayPlan payPlan=new PayPlan { Signature="stored",SigIsTopaz=true };
				Assert.IsTrue(SignatureBoxSaves.SetPayPlanSignature(payPlan,signatureBoxWrapper,"synthetic key data"));
				Assert.AreEqual("",payPlan.Signature,"The empty box's signature, as FormSheetFillEdit read it before S3a-14g.");
				Assert.AreEqual("0",SignatureBoxSaves.GetEFormSignatureValue("0stored",signatureBoxWrapper,"synthetic key data"));
			}
		}

		[TestMethod]
		public void TopazWrapper_LoadsSigPlusNetOnlyThroughTheChooser() {
			//The source: IsInstalled asks NativeFirstChooser.Topaz first, and loads in the program's process (TopazInProcess.LoadControlType) only
			//when it chose the process; else the helper's Load. The in-process file is the only one in the program that loads the assembly.
			string text=File.ReadAllText(Path.Combine(TestPaths.RepoRoot,"OpenPmsDentalBusiness","UI","TopazWrapper.cs"));
			int indexChoose=text.IndexOf("_vendorPath=NativeFirstChooser.Topaz.ChooseForManagedAssembly(path);",StringComparison.Ordinal);
			int indexInProcess=text.IndexOf("if(_vendorPath==VendorPath.InProcess) {",indexChoose,StringComparison.Ordinal);
			int indexLoad=text.IndexOf("TopazInProcess.LoadControlType(path,",indexChoose,StringComparison.Ordinal);
			int indexHelperLoad=text.IndexOf("TopazCommands.LOAD",indexChoose,StringComparison.Ordinal);
			Assert.IsTrue(indexChoose>0 && indexInProcess>indexChoose && indexLoad>indexInProcess && indexHelperLoad>indexLoad,
				"Choose, then the in-process load, then the helper's.");
			string textInProcess=File.ReadAllText(Path.Combine(TestPaths.RepoRoot,"OpenPmsDentalBusiness","UI","TopazInProcess.cs"));
			StringAssert.Contains(textInProcess,"Assembly assembly=Assembly.LoadFrom(path);");
		}
	}
}
