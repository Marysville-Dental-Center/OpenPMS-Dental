// A stand-in for the 32-bit helper's Topaz module (S3a-14f, docs/port-notes/S3a-14.md, section S3a-14f) that TopazWrapperTests load into a
// copy of the helper with --test-module in place of the real one, so the signing window's command can answer OK without a window: the real
// module shows TopazSigningWindow, which no test may show. It keeps one fake control per id, as the real module does, and throws the real
// module's TopazControlGoneException (by its full name, which is what the program checks) for an id it does not know, as a new helper does
// for the controls of the one that ended. Only .NET Framework types here, so the helper loads nothing else of this assembly's references.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Globalization;

namespace BINBTests {
	///<summary>Runs in the helper's process: TestModule calls Handle for each request to the module "Topaz".</summary>
	public static class FakeTopazModule {
		///<summary>The extra helper arguments that put this module in place of the helper's Topaz module.</summary>
		public const string ARGS="--test-module BINBTests BINBTests.FakeTopazModule Topaz";
		///<summary>What GetSigString returns after the fake signing window's OK.</summary>
		public const string SIGNATURE_SIGNED="0123456789ABCDEF";
		///<summary>A method of CALL that makes the next signing end with Cancel.</summary>
		public const string METHOD_NEXT_SIGN_CANCELS="FakeNextSignCancels";
		private static readonly Dictionary<string,FakeControl> _dictControls=new Dictionary<string,FakeControl>();
		private static int _countCreated;
		private static bool _isNextSignCancel;

		public static string[] Handle(string command,string[] args,byte[] data,out byte[] dataReply) {
			dataReply=null;
			switch(command) {
				case "Load":
					return new[] { "" };
				case "Create":
					_countCreated++;
					string id="fake-"+_countCreated.ToString(CultureInfo.InvariantCulture);
					_dictControls[id]=new FakeControl();
					return new[] { id };
				case "Release":
					_dictControls.Remove(args[0]);
					return new string[0];
				case "Call":
					return Call(GetControl(args[0]),args[1]);
				case "Picture":
					GetControl(args[0]);
					return new string[0];//no picture: the program draws nothing, as for a failed picture
				case "Sign":
					FakeControl fakeControl=GetControl(args[0]);
					if(_isNextSignCancel) {
						_isNextSignCancel=false;
						fakeControl.Points=0;
						fakeControl.Sig="";
						return new[] { "0" };
					}
					fakeControl.Points=250;//as a patient's signature on the pad
					fakeControl.Sig=SIGNATURE_SIGNED;
					return new[] { "1" };
				default:
					throw new ArgumentException("The fake Topaz module has no command "+command+".");
			}
		}

		private static FakeControl GetControl(string id) {
			if(!_dictControls.TryGetValue(id,out FakeControl fakeControl)) {
				throw new OpenPmsDentalHelper32.Topaz.TopazControlGoneException("The fake Topaz module has no control "+id+".");
			}
			return fakeControl;
		}

		///<summary>The methods TopazWrapper calls, as type and value (TopazCommands' "v", "i" and "s").</summary>
		private static string[] Call(FakeControl fakeControl,string methodName) {
			switch(methodName) {
				case "ClearTablet":
					fakeControl.Points=0;
					fakeControl.Sig="";
					return new[] { "v","" };
				case "NumberOfTabletPoints":
					return new[] { "i",fakeControl.Points.ToString(CultureInfo.InvariantCulture) };
				case "GetSigString":
					return new[] { "s",fakeControl.Sig };
				case "GetTabletState":
					return new[] { "i","0" };//capture is off between two calls; SetTabletState is not kept
				case "SetTabletState":
					return new[] { "v","" };
				case METHOD_NEXT_SIGN_CANCELS:
					_isNextSignCancel=true;
					return new[] { "v","" };
				default:
					return new[] { "v","" };//the key string, the key data and the modes: the fake keeps its signature as it is
			}
		}

		private sealed class FakeControl {
			public int Points;
			public string Sig="";
		}
	}
}

namespace OpenPmsDentalHelper32.Topaz {
	///<summary>The real module's exception for a control id it does not know has this full name; the program recognizes it by the name only
	///(TopazSignatureView.IsControlGone). The helper's own type is internal to the helper.</summary>
	public sealed class TopazControlGoneException:Exception {
		public TopazControlGoneException(string message) : base(message) {
		}
	}
}
