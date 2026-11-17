// A fake of QBFC's session interfaces that records each call (S3a-13 of the .NET 10 move, docs/port-notes/S3a-13.md, section 6). No QBFC
// object is created and no QuickBooks session is opened: each fake is a class made at run time (System.Reflection.Emit, which .NET 10 has too)
// that implements one interface of Interop.QBFC10 and every interface it derives from, and hands each call to its recorder.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Helper32Tests.QuickBooks {
	///<summary>The base of every fake: its recorder and the name it is recorded under.</summary>
	public abstract class QbfcFakeBase {
		public QbfcRecorder Recorder;
		public string FakeName;
		///<summary>The QBFC 10 interface the fake was made for (N64-B6: LateBoundQbfc reaches its members by name, as IDispatch does).</summary>
		public Type FakeInterface;

		///<summary>Called by every method of the fake (the emitted code); a by-reference argument comes as null.</summary>
		public object Invoke(string methodName,object[] arrayArgs,Type typeReturn) {
			return Recorder.Invoke(this,methodName,arrayArgs,typeReturn);
		}

		public override string ToString() {
			return FakeName;
		}
	}

	///<summary>Records the calls on its fakes in order, as text: "name.Method(args)" for a method, "name.Property=value" for a property set and
	///"name.Property" for a property read that returns a value (a read that returns another interface is the navigation to the next fake, which
	///is named after it, so "request.AppendDepositAddRq().TxnDate.SetValue(...)" shows the whole path). What a call returns: the scripted value
	///(On), else a fake of the returned interface (the same one for the same call text), else the type's default.</summary>
	public sealed class QbfcRecorder {
		public readonly List<string> ListCalls=new List<string>();
		private readonly Dictionary<string,Func<object[],object>> _dictScript=new Dictionary<string,Func<object[],object>>();
		private readonly Dictionary<string,object> _dictChildren=new Dictionary<string,object>();

		///<summary>Scripts what member returns (or throws) on the fake named fakeName: key "fakeName.Member" (a property or method name, without
		///get_/set_).</summary>
		public QbfcRecorder On(string key,Func<object[],object> func) {
			_dictScript[key]=func;
			return this;
		}

		public T Create<T>(string name) where T:class {
			return (T)QbfcFakeFactory.Create(typeof(T),this,name);
		}

		internal object Invoke(QbfcFakeBase fake,string methodName,object[] arrayArgs,Type typeReturn) {
			bool isGetter=methodName.StartsWith("get_",StringComparison.Ordinal);
			bool isSetter=methodName.StartsWith("set_",StringComparison.Ordinal);
			string member=(isGetter || isSetter) ? methodName.Substring(4) : methodName;
			string argsText=string.Join(",",arrayArgs.Select(x => Format(x)));
			string call;
			if(isGetter) {
				call=fake.FakeName+"."+member+(arrayArgs.Length>0 ? "["+argsText+"]" : "");
			}
			else if(isSetter) {
				call=fake.FakeName+"."+member+"="+argsText;
			}
			else {
				call=fake.FakeName+"."+member+"("+argsText+")";
			}
			if(!(isGetter && typeReturn.IsInterface)) {
				ListCalls.Add(call);
			}
			if(_dictScript.TryGetValue(fake.FakeName+"."+member,out Func<object[],object> func)) {
				return func(arrayArgs);
			}
			if(typeReturn==typeof(void)) {
				return null;
			}
			if(typeReturn.IsInterface) {
				if(!_dictChildren.TryGetValue(call,out object child)) {
					child=QbfcFakeFactory.Create(typeReturn,this,call);
					_dictChildren[call]=child;
				}
				return child;
			}
			if(typeReturn.IsValueType) {
				return Activator.CreateInstance(typeReturn);
			}
			return null;
		}

		public static string Format(object value) {
			if(value==null) {
				return "null";
			}
			if(value is string text) {
				return "\""+text+"\"";
			}
			if(value is DateTime dateTime) {
				return dateTime.ToString("o",CultureInfo.InvariantCulture);
			}
			if(value is bool isTrue) {
				return isTrue ? "true" : "false";
			}
			if(value is Enum) {
				return value.ToString();
			}
			if(value is IFormattable formattable) {
				return formattable.ToString(null,CultureInfo.InvariantCulture);
			}
			return value.ToString();
		}
	}

	///<summary>Makes the fake classes: one per interface, cached.</summary>
	internal static class QbfcFakeFactory {
		private static readonly object _lock=new object();
		private static readonly Dictionary<Type,Type> _dictTypes=new Dictionary<Type,Type>();
		private static ModuleBuilder _moduleBuilder;

		public static object Create(Type typeInterface,QbfcRecorder recorder,string name) {
			QbfcFakeBase fake=(QbfcFakeBase)Activator.CreateInstance(GetFakeType(typeInterface));
			fake.Recorder=recorder;
			fake.FakeName=name;
			fake.FakeInterface=typeInterface;
			return fake;
		}

		private static Type GetFakeType(Type typeInterface) {
			lock(_lock) {
				if(_dictTypes.TryGetValue(typeInterface,out Type typeFake)) {
					return typeFake;
				}
				if(_moduleBuilder==null) {
					AssemblyBuilder assemblyBuilder=AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("Helper32Tests.QbfcFakes"),AssemblyBuilderAccess.Run);
					_moduleBuilder=assemblyBuilder.DefineDynamicModule("Helper32Tests.QbfcFakes");
				}
				TypeBuilder typeBuilder=_moduleBuilder.DefineType("Fake"+_dictTypes.Count+"_"+typeInterface.Name,
					TypeAttributes.Public|TypeAttributes.Sealed|TypeAttributes.Class,typeof(QbfcFakeBase));
				MethodInfo methodInvoke=typeof(QbfcFakeBase).GetMethod(nameof(QbfcFakeBase.Invoke));
				MethodInfo methodGetType=typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle));
				List<Type> listInterfaces=new List<Type> { typeInterface };
				listInterfaces.AddRange(typeInterface.GetInterfaces());
				foreach(Type typeImplemented in listInterfaces.Distinct()) {
					typeBuilder.AddInterfaceImplementation(typeImplemented);
					foreach(MethodInfo methodInterface in typeImplemented.GetMethods()) {
						ParameterInfo[] arrayParameters=methodInterface.GetParameters();
						MethodBuilder methodBuilder=typeBuilder.DefineMethod(typeImplemented.Name+"."+methodInterface.Name,
							MethodAttributes.Private|MethodAttributes.Virtual|MethodAttributes.Final|MethodAttributes.HideBySig|MethodAttributes.NewSlot,
							methodInterface.ReturnType,arrayParameters.Select(x => x.ParameterType).ToArray());
						ILGenerator il=methodBuilder.GetILGenerator();
						il.Emit(OpCodes.Ldarg_0);
						il.Emit(OpCodes.Ldstr,methodInterface.Name);
						il.Emit(OpCodes.Ldc_I4,arrayParameters.Length);
						il.Emit(OpCodes.Newarr,typeof(object));
						for(int i=0;i<arrayParameters.Length;i++) {
							Type typeParameter=arrayParameters[i].ParameterType;
							il.Emit(OpCodes.Dup);
							il.Emit(OpCodes.Ldc_I4,i);
							if(typeParameter.IsByRef) {
								il.Emit(OpCodes.Ldnull);
							}
							else {
								il.Emit(OpCodes.Ldarg,(short)(i+1));
								if(typeParameter.IsValueType) {
									il.Emit(OpCodes.Box,typeParameter);
								}
							}
							il.Emit(OpCodes.Stelem_Ref);
						}
						il.Emit(OpCodes.Ldtoken,methodInterface.ReturnType);
						il.Emit(OpCodes.Call,methodGetType);
						il.Emit(OpCodes.Call,methodInvoke);
						if(methodInterface.ReturnType==typeof(void)) {
							il.Emit(OpCodes.Pop);
						}
						else if(methodInterface.ReturnType.IsValueType) {
							il.Emit(OpCodes.Unbox_Any,methodInterface.ReturnType);
						}
						else {
							il.Emit(OpCodes.Castclass,methodInterface.ReturnType);
						}
						il.Emit(OpCodes.Ret);
						typeBuilder.DefineMethodOverride(methodBuilder,methodInterface);
					}
				}
				typeFake=typeBuilder.CreateType();
				_dictTypes[typeInterface]=typeFake;
				return typeFake;
			}
		}
	}
}
