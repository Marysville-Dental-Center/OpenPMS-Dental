// Topaz's control in the program's own process, the native-first path (S3a-14n of the .NET 10 move, docs/port-notes/S3a-14.md, section
// "S3a-14n: native first"): TopazWrapper's code of before S3a-14, moved here unchanged, for the choice of NativeFirstChooser.Topaz. The program
// loads SigPlusNET.dll itself (Assembly.LoadFrom), makes Topaz.SigPlusNET controls that stand in its own windows, and calls their public methods
// by name. The fallback, the 32-bit helper's control behind a TopazSignatureView, is the other path of TopazWrapper.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Windows.Forms;
using CodeBase;

namespace OpenPmsDental.UI {
	///<summary>Topaz.SigPlusNET in the program's process (TopazWrapper's in-process path, as before S3a-14). Used only when
	///NativeFirstChooser.Topaz chose the program's process.</summary>
	internal static class TopazInProcess {
		///<summary>The Topaz.SigPlusNET control type of the SigPlusNET.dll at path, or null with loadError saying why (TopazWrapper.GetControlType
		///before S3a-14).</summary>
		public static Type LoadControlType(string path,out string loadError) {
			loadError="";
			Type typeSigPlusNET;
			try {
				Assembly assembly=Assembly.LoadFrom(path);
				typeSigPlusNET=assembly.GetType(TopazWrapper.CONTROL_TYPE_NAME,throwOnError:false);
				if(typeSigPlusNET==null || !typeof(Control).IsAssignableFrom(typeSigPlusNET)) {
					typeSigPlusNET=null;
					loadError=path+" has no "+TopazWrapper.CONTROL_TYPE_NAME+" control.";
				}
			}
			catch(Exception ex) {
				typeSigPlusNET=null;
				loadError=path+" could not be loaded: "+ex.Message;
			}
			return typeSigPlusNET;
		}

		///<summary>A new Topaz.SigPlusNET control of that type.</summary>
		public static Control CreateControl(Type typeSigPlusNET) {
			return (Control)Activator.CreateInstance(typeSigPlusNET);
		}

		///<summary>Calls the control's public instance method of that name whose parameter count matches, converting the arguments to its parameter
		///types (an exact match is preferred). Exceptions thrown by the method are rethrown as they are, as a compiled call would.</summary>
		public static object Call(Control topaz,string methodName,params object[] arrayArgs) {
			List<MethodInfo> listMethodInfos=topaz.GetType().GetMethods(BindingFlags.Public|BindingFlags.Instance)
				.Where(x => x.Name==methodName && x.GetParameters().Length==arrayArgs.Length).ToList();
			MethodInfo methodInfo=listMethodInfos.FirstOrDefault(x => x.GetParameters().Select(y => y.ParameterType)
				.SequenceEqual(arrayArgs.Select(y => y.GetType())))
				??listMethodInfos.FirstOrDefault();
			if(methodInfo==null) {
				throw new MissingMethodException(topaz.GetType().FullName,methodName);
			}
			ParameterInfo[] arrayParameterInfos=methodInfo.GetParameters();
			object[] arrayConverted=new object[arrayArgs.Length];
			for(int i=0;i<arrayArgs.Length;i++) {
				Type typeParameter=arrayParameterInfos[i].ParameterType;
				if(typeParameter.IsInstanceOfType(arrayArgs[i])) {
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

		///<summary>For TopazWrapper.FillSignatureEncodings: sets encoded bytes directly as the key data Topaz uses to decrypt the signature, through
		///the control's private Sig field and its private SetAutoKeyData() method that accepts a byte array (reflection, as before S3a-14). This
		///method allows us to bypass the incorrect encodings that are forced on us in the public methods. Null when the control has neither.</summary>
		public static Action<byte[]> GetSetAutoKeyData(Control topaz) {
			FieldInfo fieldInfoSig;
			object topazSignature;
			MethodInfo methodInfoSetAutoKeyData;
			try {
				//Get the field info for SigPlusNET's private Sig field.
				fieldInfoSig=topaz.GetType().GetField("Sig",BindingFlags.NonPublic | BindingFlags.Instance);
				//Get the value of the Sig field from the instance of the SigPlusNET object passed in.
				topazSignature=fieldInfoSig.GetValue(topaz);
				//Get the method info of the private SetAutoKeyData() method that accepts a byte array from the Sig field.
				//This method allows us to bypass the incorrect encodings that are forced on us in the public methods.
				methodInfoSetAutoKeyData=fieldInfoSig.FieldType.GetMethod("SetAutoKeyData",BindingFlags.NonPublic | BindingFlags.Instance,null,new Type[] { typeof(Byte[]) },null);
			}
			catch(Exception ex) {
				ex.DoNothing();
				return null;
			}
			//Invoke SetAutoKeyData() with the encoded bytes (a TargetInvocationException, as before, for what it throws).
			return bytes => methodInfoSetAutoKeyData.Invoke(topazSignature,new object[] { bytes });
		}
	}
}
