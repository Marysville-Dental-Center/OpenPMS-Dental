// The send step of Divvy postcards (FormRecallList's E-Cards button), kept apart from the window so it can be tested: the call to the
// postcard service, and the rule that recalls or reactivations are recorded as mailed (commlog and status) only when the service says every
// postcard was sent. A call that throws, or that gives no answer, counts as a Failure, so nothing is recorded as mailed (on the .NET
// Framework build a network error used to leave the message at its default, CompletedSucessfully, and record every selected row as mailed;
// the .NET 10 stand-in of DivvyPostcardService.cs answers Failure with the O-10 text, docs/port-notes/S3b-4.md).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;

namespace OpenPmsDental.DivvyConnect {
	///<summary>The send step of Divvy postcards, for FormRecallList.</summary>
	public static class DivvyPostcardSending {
		///<summary>Runs funcSend (the service's SendPostcards) and returns its answer. An exception becomes a Failure answer whose message is the
		///exception's, and messages gets the exception's text as FormRecallList showed it; no answer at all becomes a Failure answer. Either way
		///the caller records nothing as mailed.</summary>
		public static PostcardReturnMessage Send(Func<PostcardReturnMessage> funcSend,out string messages) {
			messages="";
			PostcardReturnMessage postcardReturnMessage;
			try {
				postcardReturnMessage=funcSend();
			}
			catch(Exception ex) {
				messages+="Exception: "+ex.Message+"\r\nData: "+ex.Data+"\r\n";
				return new PostcardReturnMessage { MessageCode=MessageCode.Failure,Message=ex.Message };
			}
			if(postcardReturnMessage==null) {
				return new PostcardReturnMessage { MessageCode=MessageCode.Failure,Message="The postcard service gave no answer." };
			}
			return postcardReturnMessage;
		}

		///<summary>Runs actionRecordAsMailed (the commlogs and the recall or reactivation statuses) only when the service says every postcard was
		///sent (CompletedSucessfully), and returns whether it ran. A Failure, CompletedWithErrors or no answer records nothing.</summary>
		public static bool RecordIfAllSent(PostcardReturnMessage postcardReturnMessage,Action actionRecordAsMailed) {
			if(postcardReturnMessage==null || postcardReturnMessage.MessageCode!=MessageCode.CompletedSucessfully) {
				return false;
			}
			actionRecordAsMailed();
			return true;
		}
	}
}
