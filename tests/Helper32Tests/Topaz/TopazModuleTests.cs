// The helper's Topaz module against the synthetic Topaz control (tests\TopazStandIn, never Topaz's own SigPlusNET.dll) (S3a-14 of the .NET 10
// move, docs/port-notes/S3a-14.md, section 5): the module makes the calls TopazWrapper sends on the control, by name, with their arguments and
// results; the private key method; the picture; and the signing window, through its model only (it is never shown: the module's dialog is a
// stand-in that presses the window's buttons).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Helper32Tests.Support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalHelper32;
using OpenPmsDentalHelper32.Topaz;

namespace Helper32Tests.Topaz {
	[TestClass]
	public class TopazModuleTests {
		private static string _pathStandIn;

		///<summary>The module runs in this test process here, which is x64 since S3b-5 (the helper's x86 process runs it in
		///VendorModulesProcessTests): so the stand-in, built x86 like Topaz's x86-only builds, is written for any CPU (Mono.Cecil) into a folder
		///of its own, as an any-CPU SigPlusNET.dll would be (S3a-14n's in-process path).</summary>
		private static string PathStandIn {
			get {
				if(_pathStandIn==null) {
					string folder=Path.Combine(HelperCopy.FolderTests,"topaz-anycpu-"+Guid.NewGuid().ToString("N").Substring(0,8));
					Directory.CreateDirectory(folder);
					//A name of its own: the x86 TopazStandIn.dll beside this test is in the test's own .deps.json, so a load of an assembly of that
					//name by path would get that one; and not SigPlusNET, which Constructor_LoadsNoVendorCode looks for in this process.
					string path=Path.Combine(folder,"TopazStandInAnyCpu.dll");
					using(Mono.Cecil.AssemblyDefinition assemblyDefinition=Mono.Cecil.AssemblyDefinition.ReadAssembly(Path.Combine(HelperCopy.FolderTests,
						"TopazStandIn.dll"),new Mono.Cecil.ReaderParameters { InMemory=true }))
					{
						assemblyDefinition.Name.Name="TopazStandInAnyCpu";
						assemblyDefinition.MainModule.Name="TopazStandInAnyCpu.dll";
						assemblyDefinition.MainModule.Architecture=Mono.Cecil.TargetArchitecture.I386;
						assemblyDefinition.MainModule.Attributes=Mono.Cecil.ModuleAttributes.ILOnly;
						assemblyDefinition.Write(path);
					}
					_pathStandIn=path;
				}
				return _pathStandIn;
			}
		}

		private static Helper32Reply Handle(TopazModule module,string command,byte[] data,params string[] args) {
			return module.Handle(new Helper32Request { Module=TopazCommands.MODULE,Command=command,Args=args,Data=data });
		}

		///<summary>A call of the control's method, as TopazSignatureView sends it; the decoded result.</summary>
		private static object Call(TopazModule module,string id,string methodName,params object[] arrayArgs) {
			string[] args=new[] { id,methodName }.Concat(arrayArgs.SelectMany(x => TopazCommands.EncodeValue(x))).ToArray();
			string[] arrayResults=Handle(module,TopazCommands.CALL,null,args).Results;
			Assert.AreEqual(2,arrayResults.Length);
			return TopazCommands.DecodeValue(arrayResults[0],arrayResults[1]);
		}

		private static List<string> GetCalls(TopazModule module,string id) {
			string text=(string)Call(module,id,"GetRecordedCalls");
			return text=="" ? new List<string>() : text.Split('\n').ToList();
		}

		private static string Create(TopazModule module) {
			string id=Handle(module,TopazCommands.CREATE,null,PathStandIn).Results[0];
			Call(module,id,"ClearRecordedCalls");
			return id;
		}

		private static TopazModule NewModule() {
			return new TopazModule((window,handle) => throw new AssertFailedException("No signing window in this test."));
		}

		[TestMethod]
		public void Load_TheControlFromThePathTheProgramFound() {
			TopazModule module=NewModule();
			CollectionAssert.AreEqual(new[] { "" },Handle(module,TopazCommands.LOAD,null,PathStandIn).Results);
			//Not an assembly, an assembly without the control: TopazWrapper's messages of before.
			string pathNotAssembly=Path.Combine(HelperCopy.Folder,"Helper32Tests-not-an-assembly-"+Guid.NewGuid().ToString("N")+".dll");
			File.WriteAllText(pathNotAssembly,"synthetic, not an assembly");
			try {
				string error=Handle(module,TopazCommands.LOAD,null,pathNotAssembly).Results[0];
				StringAssert.StartsWith(error,pathNotAssembly+" could not be loaded: ");
				InvalidOperationException exception=Assert.ThrowsExactly<InvalidOperationException>(() => Handle(module,TopazCommands.CREATE,null,
					pathNotAssembly));
				Assert.AreEqual(error,exception.Message);
			}
			finally {
				File.Delete(pathNotAssembly);
			}
			string pathOther=typeof(TopazModuleTests).Assembly.Location;
			CollectionAssert.AreEqual(new[] { pathOther+" has no Topaz.SigPlusNET control." },Handle(module,TopazCommands.LOAD,null,pathOther).Results);
		}

		[TestMethod]
		public void Create_OneControlPerBox_ReleaseDisposesIt() {
			TopazModule module=NewModule();
			string id1=Create(module);
			string id2=Create(module);
			Assert.AreNotEqual(id1,id2);
			StringAssert.StartsWith(id1,Process.GetCurrentProcess().Id.ToString(CultureInfo.InvariantCulture)+"-","Ids name the helper's process.");
			Assert.AreEqual(2,module.CountControls);
			Call(module,id1,"SetSigString","valid");
			Assert.AreEqual(0,Call(module,id2,"NumberOfTabletPoints"),"Each box has its own control.");
			Handle(module,TopazCommands.RELEASE,null,id1);
			Assert.AreEqual(1,module.CountControls);
			Assert.ThrowsExactly<TopazControlGoneException>(() => Call(module,id1,"NumberOfTabletPoints"));
			Handle(module,TopazCommands.RELEASE,null,id1);//twice is harmless
			Assert.ThrowsExactly<TopazControlGoneException>(() => Call(module,"1-999","NumberOfTabletPoints"));
		}

		[TestMethod]
		public void Call_MakesTopazWrappersCalls_WithTheirArgumentsAndResults() {
			TopazModule module=NewModule();
			string id=Create(module);
			//The calls of TopazWrapper's ClearTopaz, SetTopaz... and SetTopazAutoKey... methods, as TopazWrapperTests pinned them before S3a-14.
			Assert.IsNull(Call(module,id,"ClearTablet"));
			Call(module,id,"SetTabletLogicalXSize",2000);
			Call(module,id,"SetTabletLogicalYSize",600);
			Call(module,id,"SetSigCompressionMode",2);
			Call(module,id,"SetEncryptionMode",3);
			Call(module,id,"SetKeyString","0000000000000000");
			Call(module,id,"AutoKeyStart");
			Call(module,id,"SetAutoKeyData","key one");
			Call(module,id,"AutoKeyFinish");
			Call(module,id,"AutoKeyStart");
			Call(module,id,"SetAutoKeyANSIData","key two \u00e9\t");
			Call(module,id,"AutoKeyFinish");
			Call(module,id,"SetTabletState",1);
			Call(module,id,"SetSigString","valid");
			CollectionAssert.AreEqual(new[] { "ClearTablet","SetTabletLogicalXSize:2000","SetTabletLogicalYSize:600","SetSigCompressionMode:2",
				"SetEncryptionMode:3","SetKeyString:0000000000000000","AutoKeyStart","SetAutoKeyData:key one","AutoKeyFinish","AutoKeyStart",
				"SetAutoKeyANSIData:key two \u00e9\t","AutoKeyFinish","SetTabletState:1","SetSigString:valid" },GetCalls(module,id));
			Assert.AreEqual(1,Call(module,id,"GetTabletState"));
			Assert.AreEqual(250,Call(module,id,"NumberOfTabletPoints"));
			Assert.AreEqual("valid",Call(module,id,"GetSigString"));
			//What the control throws (a corrupt signature) reaches the program with its type; TopazWrapper.SetTopazSigString swallows it, as before.
			IndexOutOfRangeException exception=Assert.ThrowsExactly<IndexOutOfRangeException>(() => Call(module,id,"SetSigString","throw"));
			Assert.AreEqual("corrupt signature",exception.Message);
			Assert.AreEqual(typeof(IndexOutOfRangeException).FullName,Helper32Reply.FromException(exception).ErrorType);
			Assert.ThrowsExactly<MissingMethodException>(() => Call(module,id,"NoSuchMethod"));
			Assert.ThrowsExactly<MissingMethodException>(() => Call(module,id,"SetTabletState"),"The parameter count must match.");
		}

		[TestMethod]
		public void Values_CrossWithTheirTypes() {
			foreach(object value in new object[] { null,0,-1,int.MaxValue,long.MinValue,(short)-7,true,false,0.1,double.MaxValue,"","\u00e9\0 \r\n" }) {
				string[] arrayEncoded=TopazCommands.EncodeValue(value);
				Assert.AreEqual(value,TopazCommands.DecodeValue(arrayEncoded[0],arrayEncoded[1]),"Value "+(value??"null"));
			}
			Assert.IsNull(TopazCommands.DecodeValue(TopazCommands.TYPE_VOID,""));
			Assert.ThrowsExactly<ArgumentException>(() => TopazCommands.DecodeValue("q","1"));
		}

		[TestMethod]
		public void Values_AStringWithALoneSurrogate_CrossesAsItsCodeUnits() {
			//S3a-14f: note text cut in the middle of an emoji. The protocol's strict UTF-8 cannot carry it, and Topaz got the string itself when it
			//ran in the program's process, so it crosses unchanged as UTF-16 code units; a well-formed string stays plain text.
			foreach(string value in new[] { "note \uD83D","\uDE00","a\uDE00\uD83Db","\uD83D😀","end \uDBFF" }) {
				Assert.IsTrue(TopazCommands.HasLoneSurrogate(value),"Has a lone surrogate: "+value.Length);
				string[] arrayEncoded=TopazCommands.EncodeValue(value);
				Assert.AreEqual(TopazCommands.TYPE_STRING_UTF16,arrayEncoded[0]);
				Assert.IsFalse(TopazCommands.HasLoneSurrogate(arrayEncoded[1]));
				Assert.AreEqual(value,(string)TopazCommands.DecodeValue(arrayEncoded[0],arrayEncoded[1]));
				//The encoded text is what the protocol writes: it goes through the strict UTF-8 the protocol uses.
				new UTF8Encoding(false,true).GetBytes(arrayEncoded[1]);
				Assert.ThrowsExactly<EncoderFallbackException>(() => new UTF8Encoding(false,true).GetBytes(value),"Why the plain text cannot cross.");
			}
			CollectionAssert.AreEqual(new[] { TopazCommands.TYPE_STRING_UTF16,"0061D83D" },TopazCommands.EncodeValue("a\uD83D"));
			foreach(string value in new[] { "","plain","emoji 😀 whole","é中" }) {
				Assert.IsFalse(TopazCommands.HasLoneSurrogate(value));
				CollectionAssert.AreEqual(new[] { TopazCommands.TYPE_STRING,value },TopazCommands.EncodeValue(value),"Well-formed text as before.");
			}
			Assert.ThrowsExactly<ArgumentException>(() => TopazCommands.DecodeValue(TopazCommands.TYPE_STRING_UTF16,"D83"));
			Assert.ThrowsExactly<FormatException>(() => TopazCommands.DecodeValue(TopazCommands.TYPE_STRING_UTF16,"XYZW"));
		}

		[TestMethod]
		public void Call_AStringWithALoneSurrogate_ReachesTheControlUnchanged_AndComesBack() {
			//The module's side of the same: Call decodes the argument and the stand-in gets the very string (as Topaz got it in the program's
			//process); a string result with a lone surrogate is encoded back the same way.
			TopazModule module=NewModule();
			string id=Create(module);
			Call(module,id,"ClearRecordedCalls");
			Call(module,id,"SetAutoKeyData","note \uD83D");
			Assert.AreEqual("SetAutoKeyData:note \uD83D",(string)Call(module,id,"GetRecordedCalls"));
			string[] arrayResults=Handle(module,TopazCommands.CALL,null,id,"GetRecordedCalls").Results;
			Assert.AreEqual(TopazCommands.TYPE_STRING_UTF16,arrayResults[0]);
		}

		[TestMethod]
		public void SetAutoKeyBytes_UsesTheControlsPrivateKeyMethod() {
			TopazModule module=NewModule();
			string id=Create(module);
			CollectionAssert.AreEqual(new[] { "1" },Handle(module,TopazCommands.CAN_SET_AUTO_KEY_BYTES,null,id).Results);
			Call(module,id,"SetSigString","needs-bytes");
			Assert.AreEqual(0,Call(module,id,"NumberOfTabletPoints"));
			Handle(module,TopazCommands.SET_AUTO_KEY_BYTES,new byte[] { 1,2,3 },id);
			Call(module,id,"SetSigString","needs-bytes");
			Assert.AreEqual(250,Call(module,id,"NumberOfTabletPoints"));
			CollectionAssert.Contains(GetCalls(module,id),"Sig.SetAutoKeyData");
		}

		private static Bitmap ToBitmap(byte[] arrayPng) {
			Assert.IsNotNull(arrayPng);
			CollectionAssert.AreEqual(new byte[] { 0x89,(byte)'P',(byte)'N',(byte)'G' },arrayPng.Take(4).ToArray(),"A PNG.");
			using(MemoryStream memoryStream=new MemoryStream(arrayPng))
			using(Image image=Image.FromStream(memoryStream)) {
				return new Bitmap(image);
			}
		}

		private static bool IsBlack(Color color) {
			return color.R<64 && color.G<64 && color.B<64;
		}

		[TestMethod]
		public void Picture_IsTheControlDrawnAtTheBoxsSize() {
			TopazModule module=NewModule();
			string id=Create(module);
			using(Bitmap bitmap=ToBitmap(Handle(module,TopazCommands.PICTURE,null,id,"300","70").Data)) {
				Assert.AreEqual(new Size(300,70),bitmap.Size);
				Assert.IsFalse(IsBlack(bitmap.GetPixel(150,35)),"No points, no signature drawn.");
			}
			Call(module,id,"SetSigString","valid");
			using(Bitmap bitmap=ToBitmap(Handle(module,TopazCommands.PICTURE,null,id,"362","79").Data)) {
				Assert.AreEqual(new Size(362,79),bitmap.Size);
				Assert.IsTrue(IsBlack(bitmap.GetPixel(0,0)),"The stand-in's frame.");
				Assert.IsTrue(IsBlack(bitmap.GetPixel(180,39)) || IsBlack(bitmap.GetPixel(180,40)),"The stand-in's diagonal.");
				Assert.IsFalse(IsBlack(bitmap.GetPixel(300,10)));
			}
			Assert.ThrowsExactly<ArgumentException>(() => Handle(module,TopazCommands.PICTURE,null,id,"0","79"));
			Assert.ThrowsExactly<ArgumentException>(() => Handle(module,TopazCommands.PICTURE,null,id,"362","100000"));
		}

		private static readonly IntPtr _handleOwner=new IntPtr(0x0BADF00D);

		private static string[] SignArgs(string id) {
			return new[] { id,TopazCommands.HandleToText(_handleOwner),"360","77","Sign on the Topaz pad","OK","Clear","Cancel" };
		}

		[TestMethod]
		public void Sign_ShowsTheControlInTheSigningWindow_OkKeepsTheSignature() {
			List<string> listSeen=new List<string>();
			string id=null;
			TopazModule module=null;
			module=new TopazModule((window,handle) => {
				Assert.AreEqual(_handleOwner,handle,"Over the program's window.");
				Assert.IsFalse(window.Visible,"The test never shows the window.");
				Assert.AreEqual("Sign on the Topaz pad",window.Text);
				Assert.AreSame(window,window.ControlTopaz.Parent,"The control is in the window while the patient signs.");
				Assert.AreEqual("Topaz.SigPlusNET",window.ControlTopaz.GetType().FullName);
				Assert.AreEqual(new Size(360,77),window.ControlTopaz.Size,"The size of the program's box.");
				Assert.AreEqual("OK",window.ButtonOk.Text);
				Assert.AreEqual("Clear",window.ButtonClear.Text);
				Assert.AreEqual("Cancel",window.ButtonCancel.Text);
				foreach(Button button in new[] { window.ButtonOk,window.ButtonClear,window.ButtonCancel }) {
					Assert.IsTrue(button.Top>=window.ControlTopaz.Bottom,"The buttons are below the control.");
					Assert.IsTrue(button.Right<=window.ClientSize.Width && button.Bottom<=window.ClientSize.Height,button.Name+" is inside the window.");
				}
				Assert.AreSame(window.ButtonOk,window.AcceptButton);
				Assert.AreSame(window.ButtonCancel,window.CancelButton);
				//The patient signs, presses Clear, signs again and presses OK.
				TopazModule.Call(window.ControlTopaz,"SetSigString","valid");
				window.PressClear();
				listSeen.Add(string.Join(",",GetCalls(module,id)));
				TopazModule.Call(window.ControlTopaz,"SetSigString","valid");
				window.PressOk();
				return window.DialogResult;
			});
			id=Create(module);
			Call(module,id,"SetTabletState",1);//TopazWrapper turned capture on before
			Helper32Reply reply=Handle(module,TopazCommands.SIGN,null,SignArgs(id));
			CollectionAssert.AreEqual(new[] { "1" },reply.Results);
			Assert.AreEqual("SetTabletState:1,SetSigString:valid,ClearTablet,SetTabletLogicalXSize:2000,SetTabletLogicalYSize:600",listSeen.Single(),
				"Clear clears the pad as TopazWrapper.ClearTopaz did.");
			using(Bitmap bitmap=ToBitmap(reply.Data)) {
				Assert.AreEqual(new Size(360,77),bitmap.Size);
				Assert.IsTrue(IsBlack(bitmap.GetPixel(0,0)),"The picture of the signature comes with the reply.");
			}
			//The control outlived the window: the program's save reads the signature with its key data, as before.
			Assert.AreEqual(250,Call(module,id,"NumberOfTabletPoints"));
			Assert.AreEqual("valid",Call(module,id,"GetSigString"));
			Assert.AreEqual(1,module.CountControls);
		}

		[TestMethod]
		public void Sign_Cancel_ClearsThePad() {
			TopazModule module=new TopazModule((window,handle) => {
				TopazModule.Call(window.ControlTopaz,"SetSigString","valid");
				window.PressCancel();
				return window.DialogResult;
			});
			string id=Create(module);
			Helper32Reply reply=Handle(module,TopazCommands.SIGN,null,SignArgs(id));
			CollectionAssert.AreEqual(new[] { "0" },reply.Results);
			Assert.AreEqual(0,Call(module,id,"NumberOfTabletPoints"));
			CollectionAssert.AreEqual(new[] { "SetSigString:valid","ClearTablet","SetTabletLogicalXSize:2000","SetTabletLogicalYSize:600","ClearTablet",
				"SetTabletLogicalXSize:2000","SetTabletLogicalYSize:600" },GetCalls(module,id),"Cancel clears; the module clears once more.");
		}

		[TestMethod]
		public void Sign_WindowFails_TheControlStays() {
			TopazModule module=new TopazModule((window,handle) => throw new InvalidOperationException("Synthetic window failure"));
			string id=Create(module);
			Assert.ThrowsExactly<InvalidOperationException>(() => Handle(module,TopazCommands.SIGN,null,SignArgs(id)));
			Call(module,id,"SetSigString","valid");
			Assert.AreEqual(250,Call(module,id,"NumberOfTabletPoints"),"The control was taken out of the window before it was disposed.");
		}

		[TestMethod]
		public void Arguments_AreChecked() {
			TopazModule module=NewModule();
			string id=Create(module);
			Assert.ThrowsExactly<ArgumentException>(() => Handle(module,TopazCommands.CALL,null,id));
			Assert.ThrowsExactly<ArgumentException>(() => Handle(module,TopazCommands.CALL,null,id,"SetTabletState","i"));
			Assert.ThrowsExactly<ArgumentException>(() => Handle(module,TopazCommands.PICTURE,null,id,"1"));
			Assert.ThrowsExactly<ArgumentException>(() => Handle(module,TopazCommands.SIGN,null,id));
			Assert.ThrowsExactly<ArgumentException>(() => Handle(module,"NoSuchCommand",null));
			Assert.AreEqual(0,GetCalls(module,id).Count,"No call reached the control.");
		}

		[TestMethod]
		public void Constructor_LoadsNoVendorCode() {
			TopazModule module=new TopazModule();
			Assert.AreEqual(TopazCommands.MODULE,module.Name);
			Assert.AreEqual(0,module.CountControls);
			Assert.IsFalse(AppDomain.CurrentDomain.GetAssemblies().Any(x => x.GetName().Name=="SigPlusNET"),"No SigPlusNET is loaded by a constructor.");
		}

		[TestMethod]
		public void SigningWindow_Layout_WithLongTexts() {
			using(Control control=new Control()) {
				control.Size=new Size(120,40);
				using(TopazSigningWindow window=new TopazSigningWindow(control,"Title","A long OK","A long Clear","A long Cancel")) {
					Assert.IsTrue(window.ClientSize.Width>=window.ButtonOk.Width*3,"The window is wide enough for its buttons when the box is small.");
					Assert.IsTrue(window.ButtonOk.Right<=window.ButtonClear.Left && window.ButtonClear.Right<=window.ButtonCancel.Left);
					window.ReleaseControl();
					Assert.IsNull(control.Parent);
				}
				Assert.IsFalse(control.IsDisposed,"The window never disposes of the control.");
			}
		}
	}
}
