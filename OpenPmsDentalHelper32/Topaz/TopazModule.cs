// The 32-bit helper's Topaz module (S3a-14 of the .NET 10 move, docs/port-notes/S3a-14.md, section 5): Topaz's signature control
// (Topaz.SigPlusNET in SigPlusNET.dll), created and called here instead of inside the program's windows. The program's TopazWrapper sends the
// calls it made on the control before S3a-14, one by one and by name (key string, encryption and compression modes, signature string, tablet
// state), so stored signatures and their checks do not change; it shows the picture of the control that this module draws, and signing
// happens in this helper's own signing window (TopazSigningWindow), shown over the program's window.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Windows.Forms;

namespace OpenPmsDentalHelper32.Topaz {
	///<summary>The controls of the program's signature boxes, by id, for the helper's lifetime. SigPlusNET.dll is loaded on the first Load or
	///Create (never in the constructor), from the path the program found (TopazWrapper.FindAssemblyPath: the program folder and Topaz's own
	///folders). Each control gets its window at once, as it had one inside the program's form; the helper pumps its messages between two requests
	///(MessagePumpingStream), so the control's window answers while the helper waits.</summary>
	internal sealed class TopazModule:IHelper32Module {
		private readonly Dictionary<string,Type> _dictTypes=new Dictionary<string,Type>(StringComparer.OrdinalIgnoreCase);
		private readonly Dictionary<string,string> _dictLoadErrors=new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);
		private readonly Dictionary<string,Control> _dictControls=new Dictionary<string,Control>();
		private readonly Func<TopazSigningWindow,IntPtr,DialogResult> _funcShowDialog;
		private int _countCreated;

		///<summary>The module the helper runs: the signing window is shown modally over the program's window.</summary>
		public TopazModule() : this(TopazSigningWindow.ShowOver) {
		}

		///<summary>Tests: funcShowDialog stands in for the modal signing window (it never shows a window), with the window and the program's
		///window handle.</summary>
		internal TopazModule(Func<TopazSigningWindow,IntPtr,DialogResult> funcShowDialog) {
			_funcShowDialog=funcShowDialog;
		}

		public string Name {
			get {
				return TopazCommands.MODULE;
			}
		}

		///<summary>The controls alive now (tests).</summary>
		internal int CountControls {
			get {
				return _dictControls.Count;
			}
		}

		public Helper32Reply Handle(Helper32Request request) {
			string[] args=request.Args;
			switch(request.Command) {
				case TopazCommands.LOAD:
					CheckArgs(args,1);
					LoadType(args[0]);
					return Helper32Reply.Ok(new[] { _dictLoadErrors[args[0]] },null);
				case TopazCommands.CREATE:
					CheckArgs(args,1);
					return Helper32Reply.Ok(new[] { Create(args[0]) },null);
				case TopazCommands.RELEASE:
					CheckArgs(args,1);
					if(_dictControls.TryGetValue(args[0],out Control control)) {
						_dictControls.Remove(args[0]);
						control.Dispose();
					}
					return Helper32Reply.Ok(null,null);
				case TopazCommands.CALL:
					if(args.Length<2 || args.Length%2!=0) {
						throw new ArgumentException(TopazCommands.CALL+" takes the control's id, the method and a type and a value for each argument.");
					}
					object[] arrayArgs=new object[(args.Length-2)/2];
					for(int i=0;i<arrayArgs.Length;i++) {
						arrayArgs[i]=TopazCommands.DecodeValue(args[2+2*i],args[3+2*i]);
					}
					Control topazCall=GetControl(args[0]);
					MethodInfo methodInfoCall=GetMethod(topazCall,args[1],arrayArgs);
					object result=Call(topazCall,args[1],arrayArgs);
					if(methodInfoCall.ReturnType==typeof(void)) {
						return Helper32Reply.Ok(new[] { TopazCommands.TYPE_VOID,"" },null);
					}
					return Helper32Reply.Ok(TopazCommands.EncodeValue(result),null);
				case TopazCommands.CAN_SET_AUTO_KEY_BYTES:
					CheckArgs(args,1);
					return Helper32Reply.Ok(new[] { CanSetAutoKeyBytes(GetControl(args[0])) ? "1" : "0" },null);
				case TopazCommands.SET_AUTO_KEY_BYTES:
					CheckArgs(args,1);
					SetAutoKeyBytes(GetControl(args[0]),request.Data);
					return Helper32Reply.Ok(null,null);
				case TopazCommands.PICTURE:
					CheckArgs(args,3);
					return Helper32Reply.Ok(null,GetPicture(GetControl(args[0]),TopazCommands.IntFromText(args[1]),TopazCommands.IntFromText(args[2])));
				case TopazCommands.SIGN:
					CheckArgs(args,8);
					return Sign(GetControl(args[0]),TopazCommands.HandleFromText(args[1]),TopazCommands.IntFromText(args[2]),
						TopazCommands.IntFromText(args[3]),args[4],args[5],args[6],args[7]);
				default:
					throw new ArgumentException("The Topaz module has no command "+request.Command+".");
			}
		}

		private static void CheckArgs(string[] args,int count) {
			if(args.Length!=count) {
				throw new ArgumentException("The command takes "+count.ToString(CultureInfo.InvariantCulture)+" arguments.");
			}
		}

		#region Loading
		///<summary>Loads the control type from path once: the loading of TopazWrapper.GetControlType before S3a-14, with its messages.</summary>
		private Type LoadType(string path) {
			if(_dictTypes.TryGetValue(path,out Type typeLoaded)) {
				return typeLoaded;
			}
			Type type=null;
			string loadError="";
			try {
				Assembly assembly=Assembly.LoadFrom(path);
				type=assembly.GetType(TopazCommands.CONTROL_TYPE_NAME,throwOnError:false);
				if(type==null || !typeof(Control).IsAssignableFrom(type)) {
					type=null;
					loadError=path+" has no "+TopazCommands.CONTROL_TYPE_NAME+" control.";
				}
			}
			catch(Exception ex) {
				type=null;
				loadError=path+" could not be loaded: "+ex.Message;
			}
			_dictTypes[path]=type;
			_dictLoadErrors[path]=loadError;
			return type;
		}

		private string Create(string path) {
			Type type=LoadType(path);
			if(type==null) {
				throw new InvalidOperationException(_dictLoadErrors[path]);
			}
			Control control=(Control)Activator.CreateInstance(type);
			control.Name="sigBoxTopaz";
			control.Size=new Size(362,79);//SignatureBoxWrapper's size at 96 dpi; each picture and the signing window set the program's size.
			IntPtr handle=control.Handle;//its window, as it had one inside the program's form
			_countCreated++;
			string id=System.Diagnostics.Process.GetCurrentProcess().Id.ToString(CultureInfo.InvariantCulture)+"-"
				+_countCreated.ToString(CultureInfo.InvariantCulture);
			_dictControls[id]=control;
			return id;
		}

		private Control GetControl(string id) {
			if(!_dictControls.TryGetValue(id,out Control control)) {
				throw new TopazControlGoneException("The 32-bit helper program has no Topaz control "+id+".");
			}
			return control;
		}
		#endregion Loading

		#region Calls
		///<summary>The public instance method of that name whose parameter count matches; an exact match of the parameter types is preferred.
		///</summary>
		private static MethodInfo GetMethod(Control topaz,string methodName,object[] arrayArgs) {
			List<MethodInfo> listMethodInfos=topaz.GetType().GetMethods(BindingFlags.Public|BindingFlags.Instance)
				.Where(x => x.Name==methodName && x.GetParameters().Length==arrayArgs.Length).ToList();
			return listMethodInfos.FirstOrDefault(x => x.GetParameters().Select(y => y.ParameterType)
				.SequenceEqual(arrayArgs.Select(y => y?.GetType())))
				??listMethodInfos.FirstOrDefault();
		}

		///<summary>Calls the control's public instance method of that name whose parameter count matches, converting the arguments to its parameter
		///types (an exact match is preferred). Exceptions thrown by the method are rethrown as they are, as a compiled call would. TopazWrapper.Call
		///before S3a-14, moved here.</summary>
		internal static object Call(Control topaz,string methodName,params object[] arrayArgs) {
			MethodInfo methodInfo=GetMethod(topaz,methodName,arrayArgs);
			if(methodInfo==null) {
				throw new MissingMethodException(topaz.GetType().FullName,methodName);
			}
			ParameterInfo[] arrayParameterInfos=methodInfo.GetParameters();
			object[] arrayConverted=new object[arrayArgs.Length];
			for(int i=0;i<arrayArgs.Length;i++) {
				Type typeParameter=arrayParameterInfos[i].ParameterType;
				if(arrayArgs[i]==null || typeParameter.IsInstanceOfType(arrayArgs[i])) {
					arrayConverted[i]=arrayArgs[i];
				}
				else if(typeParameter.IsEnum) {
					arrayConverted[i]=Enum.ToObject(typeParameter,arrayArgs[i]);
				}
				else {
					arrayConverted[i]=Convert.ChangeType(arrayArgs[i],typeParameter);
				}
			}
			try {
				return methodInfo.Invoke(topaz,arrayConverted);
			}
			catch(TargetInvocationException ex) when(ex.InnerException!=null) {
				ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
				throw;//Not reached.
			}
		}

		///<summary>The private Sig field of SigPlusNET and its type's private SetAutoKeyData(byte[]), read as TopazWrapper.FillSignatureEncodings
		///read them before S3a-14 (that method bypasses the encodings the public methods force). False when reading them throws; a method that is
		///not there makes SetAutoKeyBytes throw, as the Invoke on it threw.</summary>
		private static bool CanSetAutoKeyBytes(Control topaz) {
			try {
				GetAutoKeyMethod(topaz,out object topazSignature);
				return true;
			}
			catch(Exception) {
				return false;
			}
		}

		private static MethodInfo GetAutoKeyMethod(Control topaz,out object topazSignature) {
			//Get the field info for SigPlusNET's private Sig field.
			FieldInfo fieldInfoSig=topaz.GetType().GetField("Sig",BindingFlags.NonPublic | BindingFlags.Instance);
			//Get the value of the Sig field from the instance of the SigPlusNET object passed in.
			topazSignature=fieldInfoSig.GetValue(topaz);
			//Get the method info of the private SetAutoKeyData() method that accepts a byte array from the Sig field.
			//This method allows us to bypass the incorrect encodings that are forced on us in the public methods.
			return fieldInfoSig.FieldType.GetMethod("SetAutoKeyData",BindingFlags.NonPublic | BindingFlags.Instance,null,new Type[] { typeof(Byte[]) },null);
		}

		private static void SetAutoKeyBytes(Control topaz,byte[] arrayBytes) {
			MethodInfo methodInfoSetAutoKeyData=GetAutoKeyMethod(topaz,out object topazSignature);
			try {
				//Invoke SetAutoKeyData() with the encoded bytes of keyDataHashed.
				methodInfoSetAutoKeyData.Invoke(topazSignature,new object[] { arrayBytes });
			}
			catch(TargetInvocationException ex) when(ex.InnerException!=null) {
				ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
				throw;//Not reached.
			}
		}
		#endregion Calls

		#region Picture and signing
		///<summary>A PNG of the control drawn at width by height (the size of the program's box); the control takes that size, as it took the box's
		///size inside the program.</summary>
		internal static byte[] GetPicture(Control topaz,int width,int height) {
			if(width<=0 || height<=0 || width>10000 || height>10000) {
				throw new ArgumentException("A picture of "+width.ToString(CultureInfo.InvariantCulture)+" by "+height.ToString(CultureInfo.InvariantCulture)
					+" pixels was asked for.");
			}
			if(topaz.Size!=new Size(width,height)) {
				topaz.Size=new Size(width,height);
			}
			using(Bitmap bitmap=new Bitmap(width,height))
			using(MemoryStream memoryStream=new MemoryStream()) {
				topaz.DrawToBitmap(bitmap,new Rectangle(0,0,width,height));
				bitmap.Save(memoryStream,ImageFormat.Png);
				return memoryStream.ToArray();
			}
		}

		///<summary>Shows the signing window with the control over the program's window until it closes. Cancel clears the pad as the window's Clear
		///does. The control leaves the window before the window is disposed, so the signature stays for the program's next calls (GetSigString with
		///the key data when the program saves).</summary>
		private Helper32Reply Sign(Control topaz,IntPtr handleOwner,int width,int height,string title,string textOk,string textClear,string textCancel) {
			if(width>0 && height>0) {
				topaz.Size=new Size(width,height);
			}
			DialogResult dialogResult;
			using(TopazSigningWindow topazSigningWindow=new TopazSigningWindow(topaz,title,textOk,textClear,textCancel)) {
				try {
					dialogResult=_funcShowDialog(topazSigningWindow,handleOwner);
				}
				finally {
					topazSigningWindow.ReleaseControl();
				}
			}
			bool isOk=dialogResult==DialogResult.OK;
			if(!isOk) {
				TopazSigningWindow.ClearPad(topaz);
			}
			return Helper32Reply.Ok(new[] { isOk ? "1" : "0" },GetPicture(topaz,topaz.Width,topaz.Height));
		}
		#endregion Picture and signing
	}
}
