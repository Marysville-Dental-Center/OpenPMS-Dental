// The recording QBFC fakes (QbfcFake.cs) reached late-bound (N64-B6, docs/port-notes/N64-B6.md): a DynamicObject that a dynamic call of
// QuickBooksLateBound.cs reaches as it would reach a 64-bit QBFC object through IDispatch, by member name. Each member is looked up by name
// on the QBFC 10 interface the fake was made for (the 64-bit QBFC has the same members), its arguments converted as IDispatch converts a
// VARIANT (an enum parameter takes the int the late-bound code passes, VT_I4; another fake is the COM object it stands for), and the call made
// on the typed fake, which records it in the same text as the QBFC 10 path's calls. The types of the arguments as the late-bound code passed
// them are recorded too, with each one that is not the automation type of QBFC 10's parameter. No QBFC object is created.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace Helper32Tests.QuickBooks {
	///<summary>The argument types of one late-bound run: each call as "fake.Member(Type,...)" or "fake.Member=Type", and the arguments whose type
	///is not the automation type of QBFC 10's parameter.</summary>
	public sealed class LateBoundCallLog {
		public readonly List<string> ListArgTypes=new List<string>();
		public readonly List<string> ListMismatches=new List<string>();
	}

	///<summary>A QBFC fake reached by name, as IDispatch reaches the 64-bit QBFC's objects.</summary>
	public sealed class LateBoundQbfc:DynamicObject {
		public readonly QbfcFakeBase Fake;
		private readonly LateBoundCallLog _callLog;

		public LateBoundQbfc(object fake,LateBoundCallLog callLog) {
			Fake=(QbfcFakeBase)fake;
			_callLog=callLog;
		}

		public override string ToString() {
			return Fake.FakeName;
		}

		private IEnumerable<Type> GetInterfaces() {
			return new[] { Fake.FakeInterface }.Concat(Fake.FakeInterface.GetInterfaces());
		}

		private MethodInfo FindMethod(string name,object[] arrayArgs) {
			List<MethodInfo> listMethods=GetInterfaces().SelectMany(x => x.GetMethods()).Where(x => x.Name==name && x.GetParameters().Length==arrayArgs.Length)
				.ToList();
			if(listMethods.Count<=1) {
				return listMethods.FirstOrDefault();
			}
			return listMethods.FirstOrDefault(x => x.GetParameters().Select((y,i) => IsAutomationType(arrayArgs[i],y.ParameterType)).All(y => y))
				??listMethods[0];
		}

		public override bool TryGetMember(GetMemberBinder binder,out object result) {
			MethodInfo methodInfo=FindMethod("get_"+binder.Name,new object[0]);
			if(methodInfo==null) {
				result=null;
				return false;
			}
			result=Wrap(Call(methodInfo,new object[0]));
			return true;
		}

		public override bool TrySetMember(SetMemberBinder binder,object value) {
			MethodInfo methodInfo=FindMethod("set_"+binder.Name,new[] { value });
			if(methodInfo==null) {
				return false;
			}
			_callLog.ListArgTypes.Add(Fake.FakeName+"."+binder.Name+"="+TypeName(value));
			Call(methodInfo,ConvertArgs(methodInfo,binder.Name,new[] { value }));
			return true;
		}

		public override bool TryInvokeMember(InvokeMemberBinder binder,object[] args,out object result) {
			MethodInfo methodInfo=FindMethod(binder.Name,args);
			if(methodInfo==null) {
				result=null;
				return false;
			}
			_callLog.ListArgTypes.Add(Fake.FakeName+"."+binder.Name+"("+string.Join(",",args.Select(x => TypeName(x)))+")");
			result=Wrap(Call(methodInfo,ConvertArgs(methodInfo,binder.Name,args)));
			return true;
		}

		private object[] ConvertArgs(MethodInfo methodInfo,string member,object[] args) {
			ParameterInfo[] arrayParameters=methodInfo.GetParameters();
			object[] arrayConverted=new object[args.Length];
			for(int i=0;i<args.Length;i++) {
				Type typeParameter=arrayParameters[i].ParameterType;
				if(!IsAutomationType(args[i],typeParameter)) {
					_callLog.ListMismatches.Add(Fake.FakeName+"."+member+" argument "+i+": "+TypeName(args[i])+" for QBFC 10's "+typeParameter.Name);
				}
				if(args[i] is LateBoundQbfc lateBoundQbfc) {
					arrayConverted[i]=lateBoundQbfc.Fake;
				}
				else if(typeParameter.IsEnum && args[i]!=null) {
					arrayConverted[i]=Enum.ToObject(typeParameter,args[i]);
				}
				else {
					arrayConverted[i]=args[i];
				}
			}
			return arrayConverted;
		}

		///<summary>True when arg is what IDispatch needs for a parameter of that QBFC 10 type: an enum's underlying type (VT_I4), the COM object
		///for an interface, otherwise that very type (VT_BSTR, VT_I2, VT_I4, VT_DATE, VT_R8, VT_BOOL).</summary>
		private static bool IsAutomationType(object arg,Type typeParameter) {
			if(arg==null) {
				return !typeParameter.IsValueType;
			}
			if(typeParameter.IsEnum) {
				return arg.GetType()==Enum.GetUnderlyingType(typeParameter);
			}
			if(typeParameter.IsInterface) {
				return arg is LateBoundQbfc;
			}
			return arg.GetType()==typeParameter;
		}

		private static string TypeName(object value) {
			if(value==null) {
				return "null";
			}
			if(value is LateBoundQbfc) {
				return "COM object";
			}
			return value.GetType().Name;
		}

		private object Call(MethodInfo methodInfo,object[] args) {
			try {
				return methodInfo.Invoke(Fake,args);
			}
			catch(TargetInvocationException ex) when(ex.InnerException!=null) {
				ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
				throw;
			}
		}

		private object Wrap(object value) {
			if(value is QbfcFakeBase) {
				return new LateBoundQbfc(value,_callLog);
			}
			return value;
		}
	}
}
