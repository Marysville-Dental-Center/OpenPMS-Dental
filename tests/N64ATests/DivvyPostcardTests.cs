// Divvy postcards on .NET 10 (open point O-10 of docs/specs/net10-plan.md; package S3b-4, docs/port-notes/S3b-4.md): the stand-in
// PostcardServiceClient sends nothing and answers Failure with the O-10 message instead of throwing, and FormRecallList's send step
// (DivvyPostcardSending) records no recall or reactivation as mailed after a Failure, an exception or no answer. FormRecallList writes the
// commlogs and the recall or reactivation statuses only in ProcessComms, which it runs only through DivvyPostcardSending.RecordIfAllSent;
// here that step is an action that counts its runs, so no test writes to a database or shows a window.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental.DivvyConnect;

namespace N64ATests {
	[TestClass]
	public class DivvyPostcardTests {
		///<summary>The stand-in answers Failure with the O-10 text and does not throw; FormRecallList then records nothing as mailed.</summary>
		[TestMethod]
		public void StandIn_AnswersFailureWithO10Message_RecordsNothing() {
			PostcardServiceClient postcardServiceClient=new PostcardServiceClient();
			Postcard[] postcards=new Postcard[] { new Postcard { Message="Synthetic recall postcard" } };
			PostcardReturnMessage postcardReturnMessage=postcardServiceClient.SendPostcards(Guid.NewGuid(),"user.example.test","synthetic",postcards,
				new Practice { Company="Synthetic Practice" });
			Assert.IsNotNull(postcardReturnMessage);
			Assert.AreEqual(MessageCode.Failure,postcardReturnMessage.MessageCode);
			Assert.AreEqual(PostcardServiceClient.NotPortedMessage,postcardReturnMessage.Message);
			StringAssert.Contains(postcardReturnMessage.Message,"O-10");
			StringAssert.Contains(postcardReturnMessage.Message,"docs/specs/net10-plan.md");
			StringAssert.Contains(postcardReturnMessage.Message,"No postcard was sent.");
			//FormRecallList's path: the send step, then the record step.
			PostcardReturnMessage postcardReturnMessageForm=DivvyPostcardSending.Send(
				() => postcardServiceClient.SendPostcards(Guid.NewGuid(),"user.example.test","synthetic",postcards,new Practice()),out string messages);
			Assert.AreEqual(MessageCode.Failure,postcardReturnMessageForm.MessageCode);
			Assert.AreEqual(PostcardServiceClient.NotPortedMessage,postcardReturnMessageForm.Message);
			Assert.AreEqual("",messages,"No exception.");
			int countRecorded=0;
			Assert.IsFalse(DivvyPostcardSending.RecordIfAllSent(postcardReturnMessageForm,() => countRecorded++));
			Assert.AreEqual(0,countRecorded,"No commlog and no recall or reactivation status written.");
		}

		///<summary>A send that throws (the .NET Framework client on a network error) becomes Failure, with the exception's text in the
		///messages FormRecallList shows, and records nothing as mailed: before, the answer stayed at its default, CompletedSucessfully.</summary>
		[TestMethod]
		public void Send_Throws_AnswersFailure_RecordsNothing() {
			Assert.AreEqual(MessageCode.CompletedSucessfully,new PostcardReturnMessage().MessageCode,"The default answer this guards against.");
			PostcardReturnMessage postcardReturnMessage=DivvyPostcardSending.Send(
				() => throw new InvalidOperationException("Synthetic network error"),out string messages);
			Assert.AreEqual(MessageCode.Failure,postcardReturnMessage.MessageCode);
			Assert.AreEqual("Synthetic network error",postcardReturnMessage.Message);
			StringAssert.StartsWith(messages,"Exception: Synthetic network error\r\n");
			int countRecorded=0;
			Assert.IsFalse(DivvyPostcardSending.RecordIfAllSent(postcardReturnMessage,() => countRecorded++));
			Assert.AreEqual(0,countRecorded);
		}

		///<summary>No answer is a Failure and records nothing.</summary>
		[TestMethod]
		public void Send_NoAnswer_AnswersFailure_RecordsNothing() {
			PostcardReturnMessage postcardReturnMessage=DivvyPostcardSending.Send(() => null,out string messages);
			Assert.AreEqual(MessageCode.Failure,postcardReturnMessage.MessageCode);
			Assert.AreEqual("",messages);
			int countRecorded=0;
			Assert.IsFalse(DivvyPostcardSending.RecordIfAllSent(postcardReturnMessage,() => countRecorded++));
			Assert.IsFalse(DivvyPostcardSending.RecordIfAllSent(null,() => countRecorded++));
			Assert.AreEqual(0,countRecorded);
		}

		///<summary>Only CompletedSucessfully records the rows as mailed, once; CompletedWithErrors records nothing (its per-postcard handling
		///is still Open Dental's to-do in FormRecallList).</summary>
		[TestMethod]
		public void RecordIfAllSent_OnlyCompletedSucessfully_Records() {
			int countRecorded=0;
			PostcardReturnMessage postcardReturnMessageSent=DivvyPostcardSending.Send(
				() => new PostcardReturnMessage { MessageCode=MessageCode.CompletedSucessfully,Message="Synthetic: sent" },out string messages);
			Assert.AreEqual("",messages);
			Assert.IsTrue(DivvyPostcardSending.RecordIfAllSent(postcardReturnMessageSent,() => countRecorded++));
			Assert.AreEqual(1,countRecorded);
			PostcardReturnMessage postcardReturnMessageErrors=new PostcardReturnMessage { MessageCode=MessageCode.CompletedWithErrors };
			Assert.IsFalse(DivvyPostcardSending.RecordIfAllSent(postcardReturnMessageErrors,() => countRecorded++));
			Assert.AreEqual(1,countRecorded);
		}

		///<summary>The source of FormRecallList's E-Cards button (S3b-4's handoff, S3b-5): in butECards_Click, ProcessComms(..., CommItemMode.Mail),
		///which writes the commlogs and the Mailed statuses, runs only inside the action it hands DivvyPostcardSending.RecordIfAllSent, so the
		///counting tests above cover the window's own path. The window's other buttons call ProcessComms directly, as before.</summary>
		[TestMethod]
		public void FormRecallList_ECardsButton_RecordsTheRowsOnlyThroughRecordIfAllSent() {
			string text=File.ReadAllText(Path.Combine(TestSupport.Root(),"OpenPmsDental","Forms","FormRecallList.cs")).Replace("\r\n","\n");
			int indexStart=text.IndexOf("private void butECards_Click(object sender,EventArgs e) {",StringComparison.Ordinal);
			Assert.IsTrue(indexStart>=0,"butECards_Click");
			int indexEnd=text.IndexOf("\n\t\t}\n",indexStart,StringComparison.Ordinal);
			string textMethod=text.Substring(indexStart,indexEnd-indexStart);
			int indexRecord=textMethod.IndexOf("DivvyConnect.DivvyPostcardSending.RecordIfAllSent(postcardReturnMessage,() => {",StringComparison.Ordinal);
			Assert.IsTrue(indexRecord>=0,"The record step.");
			int indexRecordEnd=textMethod.IndexOf("\n\t\t\t});",indexRecord,StringComparison.Ordinal);
			Assert.IsTrue(indexRecordEnd>indexRecord,"The end of the record step's action.");
			MatchCollection matchCollection=Regex.Matches(textMethod,@"ProcessComms\(");
			Assert.AreEqual(2,matchCollection.Count,"Recalls and reactivations.");
			foreach(Match match in matchCollection) {
				Assert.IsTrue(match.Index>indexRecord && match.Index<indexRecordEnd,"ProcessComms outside RecordIfAllSent's action, at "+match.Index+".");
				StringAssert.StartsWith(textMethod.Substring(match.Index),"ProcessComms(CommItemTypeAuto.");
			}
			StringAssert.Contains(textMethod.Substring(indexRecord,indexRecordEnd-indexRecord),"CommItemMode.Mail);");
			Assert.IsTrue(textMethod.Contains("DivvyConnect.DivvyPostcardSending.Send("),"The send step.");
		}
	}
}
