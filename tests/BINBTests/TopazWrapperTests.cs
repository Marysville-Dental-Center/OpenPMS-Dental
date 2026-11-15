// The Topaz signature pad without SigPlusNET.dll in the build: absent, the program says so; present (a synthetic stand-in, tests\TopazStandIn,
// never Topaz's file), the wrapper finds it in the search folders at run time and makes the same calls the compiled code made. Since S3a-14 of
// the .NET 10 move the control runs in the 32-bit helper program: these tests start a copy of the helper (Helper32Tests' HelperCopy), which
// loads the stand-in, and end it and delete it afterwards; the calls are read back from the stand-in in the helper.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;
using CodeBase;
using Helper32Tests.Support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Cecil;
using OpenPmsDental.UI;

namespace BINBTests {
	[TestClass]
	public class TopazWrapperTests {
		private static string _folderFake;
		private static string _folderEmpty;
		private static HelperCopy _helperCopy;
		private static Helper32Client _helper32Client;

		[ClassInitialize]
		public static void ClassInitialize(TestContext testContext) {
			_folderEmpty=TestPaths.NewTempFolder("topaz_none");
			_folderFake=TestPaths.NewTempFolder("topaz_fake");
			WriteStandIn(Path.Combine(_folderFake,TopazWrapper.ASSEMBLY_FILE_NAME));
			_helperCopy=HelperCopy.Create();
			_helper32Client=new Helper32Client(_helperCopy.PathExe);
		}

		///<summary>Only these folders are searched, and the controls live in this class's copy of the helper.</summary>
		private static void Reset(params string[] arrayFolders) {
			TopazWrapper.ResetLoader(arrayFolders.ToList(),_helper32Client);
		}

		///<summary>Writes the synthetic stand-in (tests\TopazStandIn: Topaz.SigPlusNET with the public methods the wrapper calls, every call recorded in
		///its static Calls list; SetSigString("valid") gives 250 points, "needs-bytes" gives points only after the private Sig.SetAutoKeyData(byte[])
		///was called, "throw" throws as Topaz's control does for a corrupt signature) as the file at path, under the assembly name SigPlusNET, as
		///Topaz's own file is named. Mono.Cecil renames it, so it is another assembly than the TopazStandIn.dll beside this test, which nothing here
		///loads. Before S3a-8b of the .NET 10 move the same source was compiled here with the .NET Framework's C# compiler, which .NET does not have.
		///</summary>
		private static void WriteStandIn(string path) {
			string pathStandIn=Path.Combine(Path.GetDirectoryName(typeof(TopazWrapperTests).Assembly.Location),"TopazStandIn.dll");
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
			try {
				_helper32Client?.Dispose();
			}
			finally {
				_helperCopy?.Dispose();//waits for the copy's process to end (ends it after 15 seconds) and deletes the copy
			}
			Assert.AreEqual(0,_helperCopy==null ? 0 : _helperCopy.GetProcesses().Count,"No helper is left.");
		}

		///<summary>The calls the stand-in recorded, read back from the helper's process, where the control lives.</summary>
		private static List<string> GetCalls(Control control) {
			string text=(string)((TopazSignatureView)control).Call("GetRecordedCalls");
			return text=="" ? new List<string>() : text.Split('\n').ToList();
		}

		private static void ClearCalls(Control control) {
			((TopazSignatureView)control).Call("ClearRecordedCalls");
		}

		[TestMethod]
		public void Message_IsTheOneTheOwnerAskedFor() {
			Assert.AreEqual("Topaz signature pad software is not installed.",TopazWrapper.NOT_INSTALLED_MESSAGE);
		}

		[TestMethod]
		public void NotInstalled_GetTopazThrowsTheNotInstalledMessage() {
			Reset(_folderEmpty);
			Assert.AreEqual("",TopazWrapper.FindAssemblyPath());
			Assert.IsFalse(TopazWrapper.IsInstalled());
			TopazWrapper.TopazNotInstalledException topazNotInstalledException=Expect.Throws<TopazWrapper.TopazNotInstalledException>(
				() => TopazWrapper.GetTopaz());
			Assert.AreEqual(TopazWrapper.NOT_INSTALLED_MESSAGE,topazNotInstalledException.Message);
		}

		[TestMethod]
		public void NotInstalled_EveryCallOnTheMissingControlDoesNothing() {
			Reset(_folderEmpty);
			Assert.AreEqual(0,TopazWrapper.GetTopazNumberOfTabletPoints(null));
			Assert.AreEqual(0,TopazWrapper.GetTopazState(null));
			Assert.AreEqual("",TopazWrapper.GetTopazString(null));
			TopazWrapper.ClearTopaz(null);
			TopazWrapper.SetTopazCompressionMode(null,2);
			TopazWrapper.SetTopazEncryptionMode(null,2);
			TopazWrapper.SetTopazKeyString(null,"0000000000000000");
			TopazWrapper.SetTopazAutoKeyData(null,"key");
			TopazWrapper.SetTopazAutoKeyANSIData(null,"key");
			TopazWrapper.SetTopazSigString(null,"sig");
			TopazWrapper.SetTopazState(null,1);
			TopazWrapper.FillSignatureANSI(null,"key","sig",SignatureBoxWrapper.SigMode.Default);
			TopazWrapper.FillSignatureEncodings(null,"key","sig",SignatureBoxWrapper.SigMode.Default);
		}

		[TestMethod]
		public void NotInstalled_SignatureBoxShowsTheMessage_ThenItsOwnTextAgain() {
			Reset(_folderEmpty);
			using(SignatureBoxWrapper signatureBoxWrapper=new SignatureBoxWrapper()) {
				Label labelInvalidSig=(Label)signatureBoxWrapper.Controls.Find("labelInvalidSig",true).Single();
				string textOwn=labelInvalidSig.Text;
				signatureBoxWrapper.FillSignature(true,"synthetic key data","FFFFFFFF00");
				Assert.AreEqual(TopazWrapper.NOT_INSTALLED_MESSAGE,labelInvalidSig.Text,"A Topaz signature cannot be shown or checked without Topaz.");
				Assert.AreEqual(textOwn,signatureBoxWrapper.LabelText,"Callers still see their own label text.");
				Assert.AreEqual(0,signatureBoxWrapper.GetNumberOfTabletPoints(true));
				signatureBoxWrapper.ClearSignature();
				Assert.AreEqual(textOwn,labelInvalidSig.Text);
				signatureBoxWrapper.LabelText="Changed by a caller";
				signatureBoxWrapper.FillSignature(true,"synthetic key data","FFFFFFFF00");
				signatureBoxWrapper.LabelText="Changed again while the message shows";
				signatureBoxWrapper.ClearSignature();
				Assert.AreEqual("Changed again while the message shows",labelInvalidSig.Text);
			}
		}

		///<summary>A string shaped like a Topaz signature (hex digits). It is valid base 64, so the program's own box tries to decrypt it, and fails.</summary>
		private const string SIG_TOPAZ_SHAPED="0A1B2C3D4E5F60718293A4B5C6D7E8F9";
		private const string KEY_DATA="synthetic treatment plan key data";

		private static Label GetLabel(SignatureBoxWrapper signatureBoxWrapper) {
			return (Label)signatureBoxWrapper.Controls.Find("labelInvalidSig",true).Single();
		}

		///<summary>A signature made in the program's own box for a treatment plan (digitally signed, synthetic signer), as FormTPsign saves one.</summary>
		private static string GetOwnTreatPlanSignature(string keyData) {
			using(SignatureBox signatureBox=new SignatureBox()) {
				signatureBox.SetKeyString(OpenPmsDentalBusiness.TreatPlans.GetHashStringForSignature(keyData));
				signatureBox.SetDigitalSig("Digitally Signed by Synthetic Test User");
				return signatureBox.GetSigString();
			}
		}

		[TestMethod]
		public void NotInstalled_TreatPlanTriesTheOtherKind_TheMessageStays() {
			//FormTPsign and SheetDrawingJob.GetSigTPHelper: a Topaz signature that does not show is filled once more as the program's own.
			Reset(_folderEmpty);
			using(SignatureBoxWrapper signatureBoxWrapper=new SignatureBoxWrapper()) {
				signatureBoxWrapper.SignatureMode=SignatureBoxWrapper.SigMode.TreatPlan;
				Label labelInvalidSig=GetLabel(signatureBoxWrapper);
				string textOwn=labelInvalidSig.Text;
				signatureBoxWrapper.FillSignature(true,KEY_DATA,SIG_TOPAZ_SHAPED);
				Assert.IsFalse(signatureBoxWrapper.IsValid);
				signatureBoxWrapper.FillSignature(false,KEY_DATA,SIG_TOPAZ_SHAPED);
				Assert.AreEqual(TopazWrapper.NOT_INSTALLED_MESSAGE,labelInvalidSig.Text,"Not 'Invalid Signature': Topaz could not check it.");
				Assert.IsTrue(labelInvalidSig.Visible);
				Assert.IsFalse(signatureBoxWrapper.IsValid);
				Assert.AreEqual(0,signatureBoxWrapper.GetNumberOfTabletPoints(false));
				Assert.AreEqual(textOwn,signatureBoxWrapper.LabelText,"Callers still see their own label text.");
				signatureBoxWrapper.ClearSignature();
				Assert.AreEqual(textOwn,labelInvalidSig.Text);
				Assert.IsFalse(labelInvalidSig.Visible);
			}
		}

		[TestMethod]
		public void NotInstalled_OwnKindFirstThenTopaz_TheMessageShows() {
			//The same retry when the SigIsTopaz column says the program's own box: both orders end with the same message.
			Reset(_folderEmpty);
			using(SignatureBoxWrapper signatureBoxWrapper=new SignatureBoxWrapper()) {
				signatureBoxWrapper.SignatureMode=SignatureBoxWrapper.SigMode.TreatPlan;
				Label labelInvalidSig=GetLabel(signatureBoxWrapper);
				string textOwn=labelInvalidSig.Text;
				signatureBoxWrapper.FillSignature(false,KEY_DATA,SIG_TOPAZ_SHAPED);
				Assert.AreEqual(textOwn,labelInvalidSig.Text);
				signatureBoxWrapper.FillSignature(true,KEY_DATA,SIG_TOPAZ_SHAPED);
				Assert.AreEqual(TopazWrapper.NOT_INSTALLED_MESSAGE,labelInvalidSig.Text);
				Assert.IsFalse(signatureBoxWrapper.IsValid);
			}
		}

		[TestMethod]
		public void NotInstalled_TheOwnSignatureOfATopazMarkedPlanStillShows() {
			//One SigIsTopaz column for two boxes: the column says Topaz, but this box was signed in the program's own box.
			Reset(_folderEmpty);
			string signature=GetOwnTreatPlanSignature(KEY_DATA);
			Assert.IsFalse(SignatureBoxWrapper.IsTopazShaped(signature),"The program's own box stores base 64, not Topaz's hex: "+signature);
			using(SignatureBoxWrapper signatureBoxWrapper=new SignatureBoxWrapper()) {
				signatureBoxWrapper.SignatureMode=SignatureBoxWrapper.SigMode.TreatPlan;
				Label labelInvalidSig=GetLabel(signatureBoxWrapper);
				string textOwn=labelInvalidSig.Text;
				signatureBoxWrapper.FillSignature(true,KEY_DATA,signature);
				Assert.IsFalse(signatureBoxWrapper.IsValid);
				Assert.AreEqual(textOwn,labelInvalidSig.Text,"Not a Topaz string, so no Topaz message: the first try fails as on main.");
				signatureBoxWrapper.FillSignature(false,KEY_DATA,signature);
				Assert.IsTrue(signatureBoxWrapper.IsValid,"The program's own signature is shown and checked without Topaz.");
				Assert.IsFalse(labelInvalidSig.Visible);
				Assert.AreEqual(textOwn,labelInvalidSig.Text);
				Assert.AreNotEqual(0,signatureBoxWrapper.GetNumberOfTabletPoints(false));
				//Changed key data (the plan changed after signing): the program's own check fails, and nothing Topaz could check is pending.
				signatureBoxWrapper.FillSignature(false,KEY_DATA+" changed",signature);
				Assert.IsFalse(signatureBoxWrapper.IsValid);
				Assert.AreEqual(textOwn,labelInvalidSig.Text,"A failed check of the program's own signature still says so.");
			}
		}

		///<summary>FormTPsign's fill of one treatment plan box: the SigIsTopaz column first, then the other kind when the first is not valid.</summary>
		private static void FillAsFormTPsign(SignatureBoxWrapper signatureBoxWrapper,bool sigIsTopaz,string keyData,string signature) {
			signatureBoxWrapper.FillSignature(sigIsTopaz,keyData,signature);
			if(!signatureBoxWrapper.IsValid) {
				signatureBoxWrapper.FillSignature(!sigIsTopaz,keyData,signature);
			}
		}

		[TestMethod]
		public void NotInstalled_OwnSignatureWithChangedKeyData_OwnKindFirst_SaysInvalidSignature() {
			//SigIsTopaz=false: FillSignature(false) fails, then FillSignature(true) without Topaz. A tampered or changed plan is still flagged.
			Reset(_folderEmpty);
			string signature=GetOwnTreatPlanSignature(KEY_DATA);
			using(SignatureBoxWrapper signatureBoxWrapper=new SignatureBoxWrapper()) {
				signatureBoxWrapper.SignatureMode=SignatureBoxWrapper.SigMode.TreatPlan;
				Label labelInvalidSig=GetLabel(signatureBoxWrapper);
				string textOwn=labelInvalidSig.Text;
				Assert.AreEqual("Invalid Signature",textOwn);
				signatureBoxWrapper.FillSignature(false,KEY_DATA+" changed",signature);
				Assert.IsFalse(signatureBoxWrapper.IsValid);
				Assert.AreEqual(textOwn,labelInvalidSig.Text);
				signatureBoxWrapper.FillSignature(true,KEY_DATA+" changed",signature);
				Assert.IsFalse(signatureBoxWrapper.IsValid);
				Assert.IsTrue(labelInvalidSig.Visible);
				Assert.AreEqual("Invalid Signature",labelInvalidSig.Text,"Not the Topaz message: the office never used Topaz.");
				Assert.AreEqual(textOwn,signatureBoxWrapper.LabelText);
			}
			using(SignatureBoxWrapper signatureBoxWrapper=new SignatureBoxWrapper()) {
				signatureBoxWrapper.SignatureMode=SignatureBoxWrapper.SigMode.TreatPlan;
				FillAsFormTPsign(signatureBoxWrapper,false,KEY_DATA+" changed",signature);
				Assert.IsFalse(signatureBoxWrapper.IsValid);
				Assert.AreEqual("Invalid Signature",GetLabel(signatureBoxWrapper).Text);
			}
		}

		[TestMethod]
		public void NotInstalled_OwnSignatureWithChangedKeyData_TopazFirst_SaysInvalidSignature() {
			//SigIsTopaz=true but this box was signed in the program's own box: FillSignature(true) without Topaz, then FillSignature(false) fails.
			Reset(_folderEmpty);
			string signature=GetOwnTreatPlanSignature(KEY_DATA);
			using(SignatureBoxWrapper signatureBoxWrapper=new SignatureBoxWrapper()) {
				signatureBoxWrapper.SignatureMode=SignatureBoxWrapper.SigMode.TreatPlan;
				Label labelInvalidSig=GetLabel(signatureBoxWrapper);
				string textOwn=labelInvalidSig.Text;
				Assert.AreEqual("Invalid Signature",textOwn);
				signatureBoxWrapper.FillSignature(true,KEY_DATA+" changed",signature);
				Assert.IsFalse(signatureBoxWrapper.IsValid);
				Assert.AreEqual(textOwn,labelInvalidSig.Text);
				signatureBoxWrapper.FillSignature(false,KEY_DATA+" changed",signature);
				Assert.IsFalse(signatureBoxWrapper.IsValid);
				Assert.IsTrue(labelInvalidSig.Visible);
				Assert.AreEqual("Invalid Signature",labelInvalidSig.Text,"A failed check of the program's own signature is not hidden behind the Topaz message.");
				Assert.AreEqual(textOwn,signatureBoxWrapper.LabelText);
			}
			using(SignatureBoxWrapper signatureBoxWrapper=new SignatureBoxWrapper()) {
				signatureBoxWrapper.SignatureMode=SignatureBoxWrapper.SigMode.TreatPlan;
				FillAsFormTPsign(signatureBoxWrapper,true,KEY_DATA+" changed",signature);
				Assert.IsFalse(signatureBoxWrapper.IsValid);
				Assert.AreEqual("Invalid Signature",GetLabel(signatureBoxWrapper).Text);
			}
		}

		[TestMethod]
		public void NotInstalled_TopazShapedSignature_BothFormTPsignOrders_TheMessageShows() {
			//The earlier fix still holds for a real Topaz-shaped string in FormTPsign's own retry order, whichever kind the column says.
			Reset(_folderEmpty);
			foreach(bool sigIsTopaz in new[] { true,false }) {
				using(SignatureBoxWrapper signatureBoxWrapper=new SignatureBoxWrapper()) {
					signatureBoxWrapper.SignatureMode=SignatureBoxWrapper.SigMode.TreatPlan;
					FillAsFormTPsign(signatureBoxWrapper,sigIsTopaz,KEY_DATA,SIG_TOPAZ_SHAPED);
					Assert.IsFalse(signatureBoxWrapper.IsValid);
					Assert.AreEqual(TopazWrapper.NOT_INSTALLED_MESSAGE,GetLabel(signatureBoxWrapper).Text,"SigIsTopaz="+sigIsTopaz);
				}
			}
		}

		[TestMethod]
		public void IsTopazShaped_HexOnly() {
			Assert.IsTrue(SignatureBoxWrapper.IsTopazShaped(SIG_TOPAZ_SHAPED));
			Assert.IsTrue(SignatureBoxWrapper.IsTopazShaped("ffff 0000\r\n"));
			Assert.IsFalse(SignatureBoxWrapper.IsTopazShaped(""));
			Assert.IsFalse(SignatureBoxWrapper.IsTopazShaped(null));
			Assert.IsFalse(SignatureBoxWrapper.IsTopazShaped("  "));
			Assert.IsFalse(SignatureBoxWrapper.IsTopazShaped("QUJDRA=="),"Base 64 padding is not hex.");
			Assert.IsFalse(SignatureBoxWrapper.IsTopazShaped("0A1B+2C3/"));
			Assert.IsFalse(SignatureBoxWrapper.IsTopazShaped("45,68;48,70;0,0"),"Plain points are not hex.");
			Assert.IsFalse(SignatureBoxWrapper.IsTopazShaped(GetOwnTreatPlanSignature(KEY_DATA)));
			Assert.IsFalse(SignatureBoxWrapper.IsTopazShaped(GetOwnTreatPlanSignature("another synthetic plan")));
		}

		[TestMethod]
		public void NotInstalled_AnotherSignatureAfterTheMessage_GetsTheOwnText() {
			//The message is kept only for the signature Topaz could not check, never for a different one.
			Reset(_folderEmpty);
			using(SignatureBoxWrapper signatureBoxWrapper=new SignatureBoxWrapper()) {
				signatureBoxWrapper.SignatureMode=SignatureBoxWrapper.SigMode.TreatPlan;
				Label labelInvalidSig=GetLabel(signatureBoxWrapper);
				string textOwn=labelInvalidSig.Text;
				signatureBoxWrapper.FillSignature(true,KEY_DATA,SIG_TOPAZ_SHAPED);
				signatureBoxWrapper.FillSignature(false,KEY_DATA,"1A2B3C4D5E6F708192A3B4C5D6E7F809");
				Assert.AreEqual(textOwn,labelInvalidSig.Text);
				Assert.IsFalse(signatureBoxWrapper.IsValid);
				//After ClearSignature nothing is pending either.
				signatureBoxWrapper.FillSignature(true,KEY_DATA,SIG_TOPAZ_SHAPED);
				signatureBoxWrapper.ClearSignature();
				signatureBoxWrapper.FillSignature(false,KEY_DATA,SIG_TOPAZ_SHAPED);
				Assert.AreEqual(textOwn,labelInvalidSig.Text);
			}
		}

		[TestMethod]
		public void Installed_TreatPlanTriesTheOtherKind_InvalidAsBefore() {
			//With Topaz installed a Topaz signature that fails both checks says the label's own text, as before.
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
		public void Installed_LoadsTheControlFromASearchFolderAtRunTime() {
			Reset(_folderEmpty,_folderFake);
			string pathExpected=Path.Combine(_folderFake,TopazWrapper.ASSEMBLY_FILE_NAME);
			Assert.AreEqual(pathExpected,TopazWrapper.FindAssemblyPath());
			Assert.IsTrue(TopazWrapper.IsInstalled());
			using(Control control=TopazWrapper.GetTopaz()) {
				Assert.IsInstanceOfType(control,typeof(TopazSignatureView),"The program's window gets the view of the helper's control.");
				//The helper loaded the file the program found; the program's process loads no SigPlusNET.
				string location=(string)((TopazSignatureView)control).Call("GetAssemblyLocation");
				Assert.AreEqual(Path.GetFullPath(pathExpected).ToLowerInvariant(),Path.GetFullPath(location).ToLowerInvariant());
				StringAssert.StartsWith(((TopazSignatureView)control).HelperControlId,_helper32Client.ProcessId.ToString()+"-");
				Assert.IsFalse(AppDomain.CurrentDomain.GetAssemblies().Any(x => x.GetName().Name=="SigPlusNET"));
			}
		}

		[TestMethod]
		public void Installed_WrapperMakesTheSameCallsAsTheCompiledCode() {
			Reset(_folderFake);
			using(Control control=TopazWrapper.GetTopaz()) {
				ClearCalls(control);
				TopazWrapper.ClearTopaz(control);
				TopazWrapper.SetTopazCompressionMode(control,2);
				TopazWrapper.SetTopazEncryptionMode(control,3);
				TopazWrapper.SetTopazKeyString(control,"0000000000000000");
				TopazWrapper.SetTopazAutoKeyData(control,"key one");
				TopazWrapper.SetTopazAutoKeyANSIData(control,"key two");
				TopazWrapper.SetTopazState(control,1);
				TopazWrapper.SetTopazSigString(control,"valid");
				List<string> listCalls=GetCalls(control);
				CollectionAssert.AreEqual(new[] { "ClearTablet","SetTabletLogicalXSize:2000","SetTabletLogicalYSize:600","SetSigCompressionMode:2",
					"SetEncryptionMode:3","SetKeyString:0000000000000000","AutoKeyStart","SetAutoKeyData:key one","AutoKeyFinish","AutoKeyStart",
					"SetAutoKeyANSIData:key two","AutoKeyFinish","SetTabletState:1","SetSigString:valid" },listCalls);
				Assert.AreEqual(1,TopazWrapper.GetTopazState(control));
				Assert.AreEqual(250,TopazWrapper.GetTopazNumberOfTabletPoints(control));
				Assert.AreEqual("valid",TopazWrapper.GetTopazString(control));
				TopazWrapper.SetTopazSigString(control,"throw");//Topaz throws for a corrupt signature; the wrapper swallows it, as before.
			}
		}

		[TestMethod]
		public void Installed_FillSignatureEncodings_UsesTheControlsPrivateKeyMethod() {
			Reset(_folderFake);
			using(Control control=TopazWrapper.GetTopaz()) {
				ClearCalls(control);
				TopazWrapper.FillSignatureEncodings(control,"key","needs-bytes",SignatureBoxWrapper.SigMode.Default);
				Assert.AreEqual(250,TopazWrapper.GetTopazNumberOfTabletPoints(control));
				List<string> listCalls=GetCalls(control);
				Assert.AreEqual(1,listCalls.Count(x => x=="Sig.SetAutoKeyData"),"Stops at the first encoding that shows the signature.");
			}
		}

		[TestMethod]
		public void Installed_SignatureBoxShowsATopazSignature() {
			Reset(_folderFake);
			using(SignatureBoxWrapper signatureBoxWrapper=new SignatureBoxWrapper()) {
				Label labelInvalidSig=(Label)signatureBoxWrapper.Controls.Find("labelInvalidSig",true).Single();
				signatureBoxWrapper.FillSignature(true,"synthetic key data","valid");
				Assert.AreEqual(250,signatureBoxWrapper.GetNumberOfTabletPoints(true));
				Assert.AreEqual(signatureBoxWrapper.LabelText,labelInvalidSig.Text);
				Assert.AreNotEqual(TopazWrapper.NOT_INSTALLED_MESSAGE,labelInvalidSig.Text);
				Assert.IsTrue(signatureBoxWrapper.IsValid);
				Assert.IsTrue(signatureBoxWrapper.GetSigIsTopaz());
				//The stored signature shows as the helper's picture of its control: the stand-in draws a black frame and diagonal for a signature.
				using(Bitmap bitmap=signatureBoxWrapper.GetSigImage()) {
					Assert.IsTrue(IsBlack(bitmap.GetPixel(0,0)),"The picture of the signature, drawn by the helper.");
					Assert.IsFalse(IsBlack(bitmap.GetPixel(bitmap.Width-10,10)));
				}
				//Saving reads the signature with the key data, from the helper's control, as before.
				Assert.AreEqual("valid",signatureBoxWrapper.GetSignature("synthetic key data"));
			}
		}

		private static bool IsBlack(Color color) {
			return color.R<64 && color.G<64 && color.B<64;
		}

		[TestMethod]
		public void Installed_HelperEndedBetweenTwoSignatures_TheControlIsCreatedAgain() {
			Reset(_folderFake);
			using(SignatureBoxWrapper signatureBoxWrapper=new SignatureBoxWrapper()) {
				signatureBoxWrapper.FillSignature(true,"synthetic key data","valid");
				Assert.AreEqual(250,signatureBoxWrapper.GetNumberOfTabletPoints(true));
				int processId=_helper32Client.ProcessId;
				using(Process process=Process.GetProcessById(processId)) {
					process.Kill();//this test's copy of the helper, as a crash of vendor code would end it
					Assert.IsTrue(process.WaitForExit(10000));
				}
				//The next fill starts a new helper and creates the control there again; the signature shows and is checked as before.
				signatureBoxWrapper.FillSignature(true,"synthetic key data","valid");
				Assert.AreEqual(250,signatureBoxWrapper.GetNumberOfTabletPoints(true));
				Assert.IsTrue(signatureBoxWrapper.IsValid);
				Assert.AreNotEqual(processId,_helper32Client.ProcessId);
			}
		}

		///<summary>Runs action on an STA thread, as the program's UI thread (the signing window's call waits there with its messages dispatched),
		///and rethrows what it threw.</summary>
		private static void RunOnSta(Action action) {
			Exception exceptionThread=null;
			System.Threading.Thread thread=new System.Threading.Thread(() => {
				try {
					action();
				}
				catch(Exception exception) {
					exceptionThread=exception;
				}
			});
			thread.SetApartmentState(System.Threading.ApartmentState.STA);
			thread.IsBackground=true;
			thread.Start();
			Assert.IsTrue(thread.Join(120000),"The STA thread ends.");
			if(exceptionThread!=null) {
				System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(exceptionThread).Throw();
			}
		}

		///<summary>A copy of the helper whose Topaz module is FakeTopazModule (its signing command answers without a window), for action; the
		///copy is ended and deleted afterwards, and no process of it may be left.</summary>
		private static void WithFakeTopazModule(Action<Helper32Client> action) {
			HelperCopy helperCopy=HelperCopy.Create();
			try {
				using(Helper32Client helper32Client=new Helper32Client(helperCopy.PathExe,FakeTopazModule.ARGS)) {
					TopazWrapper.ResetLoader(new List<string> { _folderFake },helper32Client);
					try {
						RunOnSta(() => action(helper32Client));
					}
					finally {
						TopazWrapper.ResetLoader();
					}
				}
			}
			finally {
				helperCopy.Dispose();
			}
			Assert.AreEqual(0,helperCopy.GetProcesses().Count,"No process of the fake module's helper copy is left.");
			Assert.IsFalse(File.Exists(helperCopy.PathExe),"The fake module's helper copy is deleted.");
		}

		///<summary>The patient signs in the signing window, as SignatureBoxWrapper's Topaz button does it: clear, encryption mode 0, capture on,
		///the window (here FakeTopazModule's OK), capture off.</summary>
		private static bool SignAsTheTopazButton(SignatureBoxWrapper signatureBoxWrapper,Control sigBoxTopaz) {
			sigBoxTopaz.Visible=true;
			TopazWrapper.ClearTopaz(sigBoxTopaz);
			TopazWrapper.SetTopazEncryptionMode(sigBoxTopaz,0);
			TopazWrapper.SetTopazState(sigBoxTopaz,1);
			bool isOk=TopazWrapper.ShowSigningWindow(sigBoxTopaz,signatureBoxWrapper);
			if(TopazWrapper.GetTopazState(sigBoxTopaz)==1) {
				TopazWrapper.SetTopazState(sigBoxTopaz,0);
			}
			return isOk;
		}

		[TestMethod]
		public void Signed_HelperEndsBeforeTheSave_GetSignatureFailsInsteadOfReturningNoSignature() {
			//S3a-14f: before, the view made an empty control in the new helper and GetSignature returned "", so the form saved no signature and said
			//nothing. Now the save fails with "could not be reached", in the box and in the exception, until the box is cleared.
			WithFakeTopazModule(helper32Client => {
				using(SignatureBoxWrapper signatureBoxWrapper=new SignatureBoxWrapper()) {
					TopazSignatureView sigBoxTopaz=(TopazSignatureView)signatureBoxWrapper.Controls.Find("sigBoxTopaz",true).Single();
					Assert.IsTrue(SignAsTheTopazButton(signatureBoxWrapper,sigBoxTopaz),"The fake signing window answers OK.");
					Assert.IsTrue(sigBoxTopaz.IsSignatureCaptured);
					Assert.AreEqual(FakeTopazModule.SIGNATURE_SIGNED,signatureBoxWrapper.GetSignature("synthetic key data"),"Saved while the helper runs.");
					Assert.IsTrue(sigBoxTopaz.IsSignatureCaptured,"Saving keeps the signature.");
					int processId=helper32Client.ProcessId;
					using(Process process=Process.GetProcessById(processId)) {
						process.Kill();//this test's copy of the helper, as a crash of vendor code would end it after the patient pressed OK
						Assert.IsTrue(process.WaitForExit(10000));
					}
					Helper32Exception helper32Exception=Assert.ThrowsExactly<Helper32Exception>(() => signatureBoxWrapper.GetSignature("synthetic key data"),
						"GetSignature never returns \"\" for the lost signature.");
					Assert.AreEqual(Helper32Failure.Crashed,helper32Exception.Failure);
					StringAssert.StartsWith(helper32Exception.Message,"Topaz signature pad software could not be reached: ");
					StringAssert.Contains(helper32Exception.Message,"ended after the patient signed on the Topaz pad");
					Assert.IsTrue(sigBoxTopaz.IsSignatureLost);
					Label labelInvalidSig=GetLabel(signatureBoxWrapper);
					Assert.IsTrue(labelInvalidSig.Visible,"The box says it.");
					Assert.AreEqual(helper32Exception.Message,labelInvalidSig.Text);
					Assert.IsFalse(signatureBoxWrapper.IsValid);
					Assert.AreNotEqual(processId,helper32Client.ProcessId,"A new helper answered.");
					//A second save fails again; the other reads of the signature fail too.
					Assert.ThrowsExactly<Helper32Exception>(() => signatureBoxWrapper.GetSignature("synthetic key data"));
					Assert.ThrowsExactly<Helper32Exception>(() => signatureBoxWrapper.GetNumberOfTabletPoints(true));
					Assert.ThrowsExactly<Helper32Exception>(() => { _=signatureBoxWrapper.SigIsBlank; });
					//The tablet state never reads the signature: a window's FormClosing (SetTabletState(0)) still closes quietly.
					signatureBoxWrapper.SetTabletState(0);
					Assert.IsTrue(sigBoxTopaz.IsSignatureLost,"The tablet state does not bring the signature back.");
					Assert.ThrowsExactly<Helper32Exception>(() => signatureBoxWrapper.GetSignature("synthetic key data"));
					//Clearing the box (its X button) ends the state: the box is empty, and a new signature can be made and saved.
					signatureBoxWrapper.ClearSignature();
					Assert.IsFalse(sigBoxTopaz.IsSignatureLost);
					Assert.IsFalse(sigBoxTopaz.IsSignatureCaptured);
					sigBoxTopaz.Visible=true;
					Assert.AreEqual("",signatureBoxWrapper.GetSignature("synthetic key data"),"A cleared box has no signature, as before.");
					Assert.IsTrue(SignAsTheTopazButton(signatureBoxWrapper,sigBoxTopaz));
					Assert.AreEqual(FakeTopazModule.SIGNATURE_SIGNED,signatureBoxWrapper.GetSignature("synthetic key data"));
				}
			});
		}

		[TestMethod]
		public void Signed_NoSignatureCapturedForCancel_AndAFilledSignatureIsMadeAgainAsBefore() {
			//Only a signature the patient signed (OK) counts as captured. Cancel clears the pad; a stored signature shown with FillSignature is made
			//again in a new helper from the database, as before (Installed_HelperEndedBetweenTwoSignatures_TheControlIsCreatedAgain).
			WithFakeTopazModule(helper32Client => {
				using(SignatureBoxWrapper signatureBoxWrapper=new SignatureBoxWrapper()) {
					TopazSignatureView sigBoxTopaz=(TopazSignatureView)signatureBoxWrapper.Controls.Find("sigBoxTopaz",true).Single();
					Assert.IsTrue(SignAsTheTopazButton(signatureBoxWrapper,sigBoxTopaz));
					Assert.IsTrue(sigBoxTopaz.IsSignatureCaptured);
					sigBoxTopaz.Call(FakeTopazModule.METHOD_NEXT_SIGN_CANCELS);
					Assert.IsFalse(SignAsTheTopazButton(signatureBoxWrapper,sigBoxTopaz),"Cancel.");
					Assert.IsFalse(sigBoxTopaz.IsSignatureCaptured,"A new signing forgets the last one; Cancel captures nothing.");
					using(Process process=Process.GetProcessById(helper32Client.ProcessId)) {
						process.Kill();//this test's copy of the helper
						Assert.IsTrue(process.WaitForExit(10000));
					}
					Assert.AreEqual("",signatureBoxWrapper.GetSignature("synthetic key data"),"Nothing was signed, so nothing is lost.");
					Assert.IsFalse(sigBoxTopaz.IsSignatureLost);
				}
			});
		}

		[TestMethod]
		public void Signed_HelperEndsBeforeTheSave_CheckBeforeWritingFails_AndChangesNothing() {
			//S3a-14f, second round: FormSheetFillEdit wrote the sheet and its other fields before GetSignature threw. CheckTopazSignatureReachable
			//lets a form find the lost signature before it writes anything (AreTopazSignaturesReachable); while the helper runs it changes nothing.
			WithFakeTopazModule(helper32Client => {
				using(SignatureBoxWrapper signatureBoxWrapperOwn=new SignatureBoxWrapper()) {
					signatureBoxWrapperOwn.CheckTopazSignatureReachable();//the program's own box: nothing to check, no helper call
				}
				using(SignatureBoxWrapper signatureBoxWrapper=new SignatureBoxWrapper()) {
					TopazSignatureView sigBoxTopaz=(TopazSignatureView)signatureBoxWrapper.Controls.Find("sigBoxTopaz",true).Single();
					Assert.IsTrue(SignAsTheTopazButton(signatureBoxWrapper,sigBoxTopaz));
					signatureBoxWrapper.CheckTopazSignatureReachable();
					signatureBoxWrapper.CheckTopazSignatureReachable();
					Assert.IsTrue(sigBoxTopaz.IsSignatureCaptured,"The check keeps the signature.");
					Label labelInvalidSig=GetLabel(signatureBoxWrapper);
					Assert.IsFalse(labelInvalidSig.Visible,"The check of a reachable signature says nothing.");
					Assert.AreEqual(FakeTopazModule.SIGNATURE_SIGNED,signatureBoxWrapper.GetSignature("synthetic key data"),"The check changed nothing.");
					int processId=helper32Client.ProcessId;
					using(Process process=Process.GetProcessById(processId)) {
						process.Kill();//this test's copy of the helper
						Assert.IsTrue(process.WaitForExit(10000));
					}
					Helper32Exception helper32Exception=Assert.ThrowsExactly<Helper32Exception>(() => signatureBoxWrapper.CheckTopazSignatureReachable(),
						"The check finds the lost signature before anything is written.");
					Assert.AreEqual(Helper32Failure.Crashed,helper32Exception.Failure);
					StringAssert.StartsWith(helper32Exception.Message,"Topaz signature pad software could not be reached: ");
					StringAssert.Contains(helper32Exception.Message,"ended after the patient signed on the Topaz pad");
					Assert.IsTrue(sigBoxTopaz.IsSignatureLost);
					Assert.IsTrue(labelInvalidSig.Visible,"The box says it.");
					Assert.AreEqual(helper32Exception.Message,labelInvalidSig.Text);
					//The form stays open: the same box still fails (GetSignature too) until the user clears it and signs again.
					Assert.ThrowsExactly<Helper32Exception>(() => signatureBoxWrapper.CheckTopazSignatureReachable());
					Assert.ThrowsExactly<Helper32Exception>(() => signatureBoxWrapper.GetSignature("synthetic key data"));
					signatureBoxWrapper.ClearSignature();
					signatureBoxWrapper.CheckTopazSignatureReachable();//a cleared box hides the Topaz control: nothing to check
					Assert.IsTrue(SignAsTheTopazButton(signatureBoxWrapper,sigBoxTopaz));
					signatureBoxWrapper.CheckTopazSignatureReachable();
					Assert.AreEqual(FakeTopazModule.SIGNATURE_SIGNED,signatureBoxWrapper.GetSignature("synthetic key data"));
				}
			});
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

		[TestMethod]
		public void FormSheetFillEdit_ChecksTopazSignaturesBeforeItWritesTheSheet() {
			//S3a-14f, second round: TryToSaveData wrote the sheet (Sheets.Insert or Update) and its other fields (SheetFields.Sync) before
			//GetSignature found the lost Topaz signature, so a sheet without the patient's signature was stored. The check comes before any of it.
			string source=TestPaths.ReadSource("OpenPmsDental","Forms","FormSheetFillEdit.cs").Replace("\r\n","\n");
			string textCheck=GetMethodText(source,"private bool AreTopazSignaturesReachable() {");
			StringAssert.Contains(textCheck,".GetSigChanged()");
			StringAssert.Contains(textCheck,".GetSigIsTopaz()");
			StringAssert.Contains(textCheck,".CheckTopazSignatureReachable();");
			StringAssert.Contains(textCheck,"catch(Helper32Exception ex) {");
			StringAssert.Contains(textCheck,"return false;");
			string textSave=GetMethodText(source,"private bool TryToSaveData() {");
			int indexCheck=textSave.IndexOf("if(!AreTopazSignaturesReachable()) {",StringComparison.Ordinal);
			Assert.IsTrue(indexCheck>=0,"TryToSaveData checks the Topaz signatures.");
			StringAssert.Contains(textSave.Substring(indexCheck),"if(!AreTopazSignaturesReachable()) {\n\t\t\t\treturn false;",
				"A failed check stops the save.");
			foreach(string write in new[] { "TrySetDateSubmitted()","Sheets.Insert(","Sheets.Update(","Sheets.SaveParameters(","SheetFields.Sync(",
				"SheetFields.GetFieldsAndParameters(","ImageStore.Import(",".GetSignature(","DidChangeSheet=true" })
			{
				int indexWrite=textSave.IndexOf(write,StringComparison.Ordinal);
				Assert.IsTrue(indexWrite>=0,write+" is in TryToSaveData (update this test if the method changed).");
				Assert.IsTrue(indexCheck<indexWrite,"TryToSaveData checks the Topaz signatures before "+write);
			}
			//The payment plan's save writes the plan with its signature (SaveSignaturePayPlan): the check comes first there too.
			string textButSave=GetMethodText(source,"private void butSave_Click(object sender,EventArgs e) {");
			int indexCheckPayPlan=textButSave.IndexOf("if(!AreTopazSignaturesReachable()) {",StringComparison.Ordinal);
			int indexSavePayPlan=textButSave.IndexOf("SaveSignaturePayPlan();",StringComparison.Ordinal);
			Assert.IsTrue(indexCheckPayPlan>=0 && indexCheckPayPlan<indexSavePayPlan,"The payment plan's save checks the Topaz signatures first.");
			//Third round: VerifyRequiredFields (OK, the exam sheet's and the payment plan's save) reads a required box's SigIsBlank, which throws for
			//a lost signature: the check comes first and stops it.
			string textVerify=GetMethodText(source,"private bool VerifyRequiredFields(){");
			int indexCheckVerify=textVerify.IndexOf("if(!AreTopazSignaturesReachable()) {\n\t\t\t\treturn false;",StringComparison.Ordinal);
			int indexSigIsBlank=textVerify.IndexOf(".SigIsBlank",StringComparison.Ordinal);
			Assert.IsTrue(indexSigIsBlank>=0,"VerifyRequiredFields reads SigIsBlank (update this test if the method changed).");
			Assert.IsTrue(indexCheckVerify>=0 && indexCheckVerify<indexSigIsBlank,"VerifyRequiredFields checks the Topaz signatures before SigIsBlank.");
			//Every SaveSignaturePayPlan in the file (the save, Email, PDF, Print, Print or Email) comes right after a check that returns.
			int countSavePayPlan=0;
			for(int index=source.IndexOf("SaveSignaturePayPlan();",StringComparison.Ordinal);index>=0;
				index=source.IndexOf("SaveSignaturePayPlan();",index+1,StringComparison.Ordinal))
			{
				countSavePayPlan++;
				int indexCheckBefore=source.LastIndexOf("if(!AreTopazSignaturesReachable()) {",index,StringComparison.Ordinal);
				Assert.IsTrue(indexCheckBefore>=0,"A check before the SaveSignaturePayPlan at "+index);
				string textBetween=source.Substring(indexCheckBefore,index-indexCheckBefore);
				StringAssert.StartsWith(textBetween.Replace("//S3a-14f: the form stays open for a lost Topaz signature",""),
					"if(!AreTopazSignaturesReachable()) {\n","The check at "+indexCheckBefore);
				Assert.AreEqual(1,Regex.Matches(textBetween,@"\breturn\b").Count,"The check right before the SaveSignaturePayPlan at "+index+" returns.");
				Assert.AreEqual(3,textBetween.Split('\n').Length-1,"Nothing between the check and the SaveSignaturePayPlan at "+index+": "+textBetween);
			}
			Assert.AreEqual(5,countSavePayPlan,"SaveSignaturePayPlan is called five times (update this test if that changed).");
		}

		[TestMethod]
		public void ProcedureForms_ALostTopazSignatureNeverReachesProceduresUpdate() {
			//S3a-14f, third round: FormProcNoteAppend, FormProcEdit and FormProcGroup caught every exception around SaveSignature, showed "Error saving
			//signature." and went on to Procedures.Update, which stored the note beside the old signature (FormProcNoteAppend then closed). A
			//Helper32Exception (the lost signature) is caught first and returns; each form checks the Topaz signature before it writes anything.
			foreach(string fileName in new[] { "FormProcNoteAppend.cs","FormProcEdit.cs","FormProcGroup.cs" }) {
				string source=TestPaths.ReadSource("OpenPmsDental","Forms",fileName).Replace("\r\n","\n");
				Assert.AreEqual(1,Regex.Matches(source,@"\n\t+SaveSignature\(\);").Count,fileName+": SaveSignature is called once (update this test if that changed).");
				Match matchTry=Regex.Match(source,@"\n\t+try \{\n\t+SaveSignature\(\);\n\t+\}\n\t+catch\(");
				Assert.IsTrue(matchTry.Success,fileName+": SaveSignature is alone in a try block.");
				int indexSave=source.IndexOf("SaveSignature();",matchTry.Index,StringComparison.Ordinal);
				int indexCatchHelper=source.IndexOf("catch(Helper32Exception ex) {",indexSave,StringComparison.Ordinal);
				int indexCatchAll=source.IndexOf("catch(Exception ex)",indexSave,StringComparison.Ordinal);
				int indexUpdate=source.IndexOf("Procedures.Update(",indexSave,StringComparison.Ordinal);
				Assert.IsTrue(indexCatchAll>indexSave && indexUpdate>indexCatchAll,fileName+": the catch-all is followed by Procedures.Update (update this test).");
				Assert.IsTrue(indexCatchHelper>indexSave && indexCatchHelper<indexCatchAll,fileName+": Helper32Exception is caught ahead of Exception.");
				string textCatchHelper=source.Substring(indexCatchHelper,indexCatchAll-indexCatchHelper);
				Assert.IsTrue(Regex.IsMatch(textCatchHelper,@"\n\t+return;\n\t+\}\n\t+$"),fileName+": the Helper32Exception catch returns: "+textCatchHelper);
				StringAssert.Contains(textCatchHelper,"ex.Message",fileName+": the catch shows the message.");
				Assert.IsFalse(textCatchHelper.Contains("Procedures.Update("),fileName);
			}
			//FormProcNoteAppend: the check comes before the note, its user and the alert change; the catch puts the note and its user back.
			string sourceAppend=TestPaths.ReadSource("OpenPmsDental","Forms","FormProcNoteAppend.cs").Replace("\r\n","\n");
			string textAppendSave=GetMethodText(sourceAppend,"private void butSave_Click(object sender,EventArgs e) {");
			int indexCheckAppend=textAppendSave.IndexOf("signatureBoxWrapper.CheckTopazSignatureReachable();",StringComparison.Ordinal);
			Assert.IsTrue(indexCheckAppend>=0,"FormProcNoteAppend checks the Topaz signature.");
			StringAssert.Contains(textAppendSave.Substring(indexCheckAppend),"catch(Helper32Exception ex) {\n\t\t\t\t\tMessageBox.Show(ex.Message);\n\t\t\t\t\treturn;");
			foreach(string write in new[] { "ProcedureCur.Copy()","ProcedureCur.UserNum=","ProcedureCur.Note=","AlertItems.Insert(","SaveSignature();" }) {
				int indexWrite=textAppendSave.IndexOf(write,StringComparison.Ordinal);
				Assert.IsTrue(indexWrite>=0,write+" is in FormProcNoteAppend.butSave_Click (update this test if the method changed).");
				Assert.IsTrue(indexCheckAppend<indexWrite,"FormProcNoteAppend checks the Topaz signature before "+write);
			}
			string textAppendCatch=textAppendSave.Substring(textAppendSave.IndexOf("catch(Helper32Exception ex) {",
				textAppendSave.IndexOf("SaveSignature();",StringComparison.Ordinal),StringComparison.Ordinal));
			StringAssert.Contains(textAppendCatch,"ProcedureCur.Note=procedure.Note;");
			StringAssert.Contains(textAppendCatch,"ProcedureCur.UserNum=procedure.UserNum;");
			//FormProcEdit and FormProcGroup: the check returns false or returns at the top of EntriesAreValid (before its SigIsBlank) and before
			//SaveAndClose writes anything.
			AssertCheckedFirst("FormProcEdit.cs","private bool IsTopazSignatureReachable() {","private bool EntriesAreValid() {",
				"if(!IsTopazSignatureReachable()) {\n\t\t\t\treturn false;",new[] { ".SigIsBlank","Procedures.EntriesAreValid(" },new[] { "_signatureChanged" });
			AssertCheckedFirst("FormProcEdit.cs","private bool IsTopazSignatureReachable() {","private void SaveAndClose() {",
				"if(!IsTopazSignatureReachable()) {",new[] { "ClaimProcs.Update(","PaySplits.UpdateAttachedPaySplits(","Adjustments.Update(","SaveSignature();",
				"Procedures.Update(" },new[] { "_signatureChanged" });
			AssertCheckedFirst("FormProcGroup.cs","private bool AreTopazSignaturesReachable() {","private bool EntriesAreValid() {",
				"if(!AreTopazSignaturesReachable()) {\n\t\t\t\treturn false;",new[] { "IsAuthorizedForChanges(",".SigIsBlank","AreSecondSignatureEntriesValid(" },
				new[] { "_hasSigChanged","_hasSig2Changed","signatureBoxWrapper2.CheckTopazSignatureReachable();" });
			AssertCheckedFirst("FormProcGroup.cs","private bool AreTopazSignaturesReachable() {","private void SaveAndClose() {",
				"if(!AreTopazSignaturesReachable()) {",new[] { "ProcGroupItems.SyncForGroup(","SaveSignature();","Procedures.Update(","SecurityLogs.MakeLogEntry(" },
				new[] { "_hasSigChanged" });
		}

		///<summary>In fileName, the check method checkSignature reads a changed Topaz box and returns false on a Helper32Exception, and in the method
		///methodSignature the call callCheck comes before each of listWrites.</summary>
		private static void AssertCheckedFirst(string fileName,string checkSignature,string methodSignature,string callCheck,string[] listWrites,
			string[] listCheckTexts)
		{
			string source=TestPaths.ReadSource("OpenPmsDental","Forms",fileName).Replace("\r\n","\n");
			string textCheck=GetMethodText(source,checkSignature);
			StringAssert.Contains(textCheck,".GetSigIsTopaz()",fileName);
			StringAssert.Contains(textCheck,".CheckTopazSignatureReachable();",fileName);
			StringAssert.Contains(textCheck,"catch(Helper32Exception ex) {",fileName);
			StringAssert.Contains(textCheck,"return false;",fileName);
			foreach(string checkText in listCheckTexts) {
				StringAssert.Contains(textCheck,checkText,fileName);
			}
			string textMethod=GetMethodText(source,methodSignature);
			int indexCheck=textMethod.IndexOf(callCheck,StringComparison.Ordinal);
			Assert.IsTrue(indexCheck>=0,fileName+": "+methodSignature+" checks the Topaz signature.");
			Assert.IsTrue(Regex.IsMatch(textMethod.Substring(indexCheck),@"^if\(![A-Za-z]+\(\)\) \{(//[^\n]*)?\n\t+return( false)?;"),
				fileName+": the failed check in "+methodSignature+" returns.");
			foreach(string write in listWrites) {
				int indexWrite=textMethod.IndexOf(write,StringComparison.Ordinal);
				Assert.IsTrue(indexWrite>=0,fileName+": "+write+" is in "+methodSignature+" (update this test if the method changed).");
				Assert.IsTrue(indexCheck<indexWrite,fileName+": "+methodSignature+" checks the Topaz signature before "+write);
			}
		}

		[TestMethod]
		public void LoneSurrogate_KeyDataReachesTopazUnchanged_AsInTheProgramsProcess() {
			//S3a-14f: a note cut in the middle of an emoji leaves half a surrogate pair in the key data, which the protocol's strict UTF-8 cannot
			//carry (EncoderFallbackException, not caught before). On main the program passed the string itself to Topaz's method in its own process,
			//so the helper's control must get the same string: the stand-in records each argument as it got it, and the recorded calls come back
			//through the same encoding.
			string keyDataCut="Synthetic note "+"\uD83D";//the high half of an emoji, its low half cut off
			string keyDataLow="\uDE00"+" synthetic";//a lone low half
			string keyDataPair="Synthetic note 😀";//a whole emoji: well-formed, sent as plain text
			Reset(_folderFake);
			using(Control control=TopazWrapper.GetTopaz()) {
				ClearCalls(control);
				TopazWrapper.SetTopazAutoKeyData(control,keyDataCut);
				TopazWrapper.SetTopazAutoKeyANSIData(control,keyDataLow);
				TopazWrapper.SetTopazKeyString(control,keyDataPair);
				TopazWrapper.SetTopazSigString(control,"valid");
				CollectionAssert.AreEqual(new[] { "AutoKeyStart","SetAutoKeyData:"+keyDataCut,"AutoKeyFinish","AutoKeyStart","SetAutoKeyANSIData:"+keyDataLow,
					"AutoKeyFinish","SetKeyString:"+keyDataPair,"SetSigString:valid" },GetCalls(control));
				Assert.AreEqual(250,TopazWrapper.GetTopazNumberOfTabletPoints(control));
				//The fill and the save of a box with such key data go through, with the same calls as for any key data.
				using(SignatureBoxWrapper signatureBoxWrapper=new SignatureBoxWrapper()) {
					signatureBoxWrapper.FillSignature(true,keyDataCut,"valid");
					Assert.IsTrue(signatureBoxWrapper.IsValid);
					Assert.AreEqual("valid",signatureBoxWrapper.GetSignature(keyDataCut));
				}
			}
		}

		[TestMethod]
		public void NotInstalled_NoHelperIsStarted() {
			//Most offices have no Topaz software: no SigPlusNET.dll is found, and no helper is started for a signature box.
			using(Helper32Client helper32Client=new Helper32Client(_helperCopy.PathExe)) {
				TopazWrapper.ResetLoader(new List<string> { _folderEmpty },helper32Client);
				using(SignatureBoxWrapper signatureBoxWrapper=new SignatureBoxWrapper()) {
					signatureBoxWrapper.FillSignature(true,"synthetic key data",SIG_TOPAZ_SHAPED);
					Assert.IsFalse(signatureBoxWrapper.IsValid);
				}
				Assert.AreEqual(0,helper32Client.ProcessId);
			}
		}

		[TestMethod]
		public void HelperMissing_TopazCountsAsNotInstalled_WithTheHelpersMessage() {
			string pathMissing=Path.Combine(_folderEmpty,Helper32Client.HELPER_FILE_NAME);
			using(Helper32Client helper32Client=new Helper32Client(pathMissing)) {
				TopazWrapper.ResetLoader(new List<string> { _folderFake },helper32Client);
				Assert.IsFalse(TopazWrapper.IsInstalled());
				StringAssert.StartsWith(TopazWrapper.GetNotInstalledMessage(),TopazWrapper.NOT_INSTALLED_MESSAGE+"\r\n"+Path.Combine(_folderFake,
					TopazWrapper.ASSEMBLY_FILE_NAME)+" could not be loaded: ");
				StringAssert.Contains(TopazWrapper.GetNotInstalledMessage(),"was not found at "+pathMissing);
				Expect.Throws<TopazWrapper.TopazNotInstalledException>(() => TopazWrapper.GetTopaz());
				using(SignatureBoxWrapper signatureBoxWrapper=new SignatureBoxWrapper()) {
					signatureBoxWrapper.FillSignature(true,"synthetic key data",SIG_TOPAZ_SHAPED);
					Assert.AreEqual(TopazWrapper.NOT_INSTALLED_MESSAGE,GetLabel(signatureBoxWrapper).Text);
				}
			}
		}

		[TestMethod]
		public void DefaultSearch_StartsWithTheProgramFolder() {
			TopazWrapper.ResetLoader();
			Assert.AreSame(Helper32Client.Default,TopazWrapper.GetHelper32Client(),"Without a test's client the program's helper runs the controls.");
			List<string> listFolders=TopazWrapper.GetSearchFolders();
			Assert.AreEqual(AppDomain.CurrentDomain.BaseDirectory,listFolders[0]);
			Assert.IsTrue(listFolders.Any(x => x.EndsWith("\\SigPlus",StringComparison.OrdinalIgnoreCase)),string.Join("; ",listFolders));
			Assert.IsTrue(listFolders.Skip(1).All(x => x.IndexOf("Topaz",StringComparison.OrdinalIgnoreCase)>=0 || x.EndsWith("\\SigPlus")),
				"Only Topaz's own folders are searched besides the program folder: "+string.Join("; ",listFolders));
		}
	}
}
