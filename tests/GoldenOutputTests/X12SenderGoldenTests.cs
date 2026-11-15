// X12-SENDER against the fixture database (docs/port-notes/X12-SENDER.md): with a blank Sender TIN the practice is the sender of the 837 batch
// (5010 and 4010) and of the 270, never Open Dental Inc. The batch is the recorded fixture with only the sender segments changed (ISA06, GS02,
// 1000A NM1 and PER): the fixtures themselves are recorded with a Sender TIN, which keeps today's behaviour and is not re-recorded. Missing practice
// values refuse the batch before anything is written. Synthetic data only; the practice sender is the fixture's Default Provider (TIN
// 123456789), its title and its phone.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Linq;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalBusiness;
using OpenPmsDentalBusiness.Eclaims;

namespace GoldenOutputTests {
	[TestClass]
	public class X12SenderGoldenTests {
		private const int BATCH_NUM=7;
		///<summary>Open Dental Inc's identities that upstream sent with a blank Sender TIN.</summary>
		private static readonly string[] _arrayOpenDentalIncValues={ "810624427","204944520","8776861248","8662390469","OPEN DENTAL SOFTWARE",
			"OPEN DENTAL SOFTWARE INC.","JORDAN SPARKS" };

		[TestInitialize]
		public void TestInitialize() {
			GoldenDb.Begin();
		}

		///<summary>The fixture clearinghouse with its Sender TIN, Name and Telephone blank ('The practice is the "sender"').</summary>
		private static Clearinghouse GetClearinghouseWithoutSender(long clearinghouseNum) {
			Clearinghouse clearinghouseClin=Clearinghouses.OverrideFields(Clearinghouses.GetClearinghouse(clearinghouseNum),0);
			clearinghouseClin.SenderTIN="";
			clearinghouseClin.SenderName="";
			clearinghouseClin.SenderTelephone="";
			return clearinghouseClin;
		}

		private static List<ClaimSendQueueItem> GetQueueItems() {
			List<ClaimSendQueueItem> listQueueItems=Claims.GetQueueList(GoldenData.ClaimNum,0,0).ToList();
			Assert.AreEqual(1,listQueueItems.Count);
			return listQueueItems;
		}

		///<summary>text with oldValue replaced by newValue, which must occur exactly once.</summary>
		private static string ReplaceOnce(string text,string oldValue,string newValue) {
			int index=text.IndexOf(oldValue,StringComparison.Ordinal);
			Assert.IsTrue(index>=0,"Not in the fixture: "+oldValue);
			Assert.AreEqual(-1,text.IndexOf(oldValue,index+1,StringComparison.Ordinal),"More than once in the fixture: "+oldValue);
			return text.Substring(0,index)+newValue+text.Substring(index+oldValue.Length);
		}

		///<summary>The lines of the two texts that differ, as "line n: expected | actual". Both must have the same number of lines.</summary>
		private static List<string> GetChangedLines(string textOld,string textNew) {
			string[] arrayOld=textOld.Split(new[] { "\r\n" },StringSplitOptions.None);
			string[] arrayNew=textNew.Split(new[] { "\r\n" },StringSplitOptions.None);
			Assert.AreEqual(arrayOld.Length,arrayNew.Length,"The batch has the fixture's segments.");
			List<string> listChanged=new List<string>();
			for(int i=0;i<arrayOld.Length;i++) {
				if(arrayOld[i]!=arrayNew[i]) {
					listChanged.Add("line "+(i+1)+": "+arrayOld[i]+" | "+arrayNew[i]);
				}
			}
			return listChanged;
		}

		private static void AssertNoOpenDentalInc(string batch) {
			foreach(string value in _arrayOpenDentalIncValues) {
				Assert.IsFalse(batch.ToUpperInvariant().Contains(value),value);
			}
		}

		[TestMethod]
		public void X837_5010_BlankSenderTin_ThePracticeIsTheSender_OnlyTheSenderSegmentsChange() {
			string batch=X837GoldenTests.NormaliseDateTimes(x837Controller.GenerateBatch(GetClearinghouseWithoutSender(GoldenData.ClearinghouseNum5010),
				GetQueueItems(),BATCH_NUM,EnumClaimMedType.Dental));
			string fixture=Golden.ReadFixtureText("x837-5010-dental.txt");
			string expected=fixture;
			expected=ReplaceOnce(expected,"*ZZ*991234567      *","*ZZ*123456789      *");//ISA06
			expected=ReplaceOnce(expected,"GS*HC*991234567*","GS*HC*123456789*");//GS02
			expected=ReplaceOnce(expected,"NM1*41*2*GOLDEN SYNTHETIC BILLING*****46*991234567~","NM1*41*2*GOLDEN SYNTHETIC DENTAL*****46*123456789~");
			expected=ReplaceOnce(expected,"PER*IC**TE*5035550199~","PER*IC**TE*5035550100~");//1000A PER (PER02 only for ClaimConnect and Emdeon Medical)
			Assert.AreEqual(expected,batch);
			Assert.AreEqual(4,GetChangedLines(fixture,batch).Count,string.Join("\r\n",GetChangedLines(fixture,batch)));
			AssertNoOpenDentalInc(batch);
		}

		[TestMethod]
		public void X837_4010_BlankSenderTin_ThePracticeIsTheSender_OnlyTheSenderSegmentsChange() {
			string batch=X837GoldenTests.NormaliseDateTimes(x837Controller.GenerateBatch(GetClearinghouseWithoutSender(GoldenData.ClearinghouseNum4010),
				GetQueueItems(),BATCH_NUM,EnumClaimMedType.Dental));
			string fixture=Golden.ReadFixtureText("x837-4010-dental.txt");
			string expected=fixture;
			expected=ReplaceOnce(expected,"*ZZ*991234567      *","*ZZ*123456789      *");//ISA06
			expected=ReplaceOnce(expected,"GS*HC*991234567*","GS*HC*123456789*");//GS02
			expected=ReplaceOnce(expected,"NM1*41*2*GOLDEN SYNTHETIC BILLING*****46*991234567~","NM1*41*2*GOLDEN SYNTHETIC DENTAL*****46*123456789~");
			expected=ReplaceOnce(expected,"PER*IC*GOLDEN SYNTHETIC BILLING*TE*5035550199~","PER*IC*GOLDEN SYNTHETIC DENTAL*TE*5035550100~");
			Assert.AreEqual(expected,batch);
			Assert.AreEqual(4,GetChangedLines(fixture,batch).Count,string.Join("\r\n",GetChangedLines(fixture,batch)));
			AssertNoOpenDentalInc(batch);
		}

		///<summary>Upstream sent another Open Dental Inc name, TIN and telephone to Emdeon Medical (ISA08 133052274). The practice is the sender there
		///too; PER02 is still written for Emdeon Medical.</summary>
		[TestMethod]
		public void X837_5010_EmdeonMedical_BlankSenderTin_ThePracticeIsTheSender() {
			Clearinghouse clearinghouseClin=GetClearinghouseWithoutSender(GoldenData.ClearinghouseNum5010);
			clearinghouseClin.ISA08="133052274";
			string batch=x837Controller.GenerateBatch(clearinghouseClin,GetQueueItems(),BATCH_NUM,EnumClaimMedType.Dental);
			List<string> listLines=batch.Split(new[] { "\r\n" },StringSplitOptions.None).ToList();
			StringAssert.StartsWith(listLines.First(x => x.StartsWith("ISA*")),"ISA*00*          *00*          *ZZ*123456789      *ZZ*133052274      *");
			Assert.AreEqual("NM1*41*2*GOLDEN SYNTHETIC DENTAL*****46*123456789~",listLines.Single(x => x.StartsWith("NM1*41*")));
			int indexSubmitter=listLines.FindIndex(x => x.StartsWith("NM1*41*"));
			Assert.AreEqual("PER*IC*GOLDEN SYNTHETIC DENTAL*TE*5035550100~",listLines[indexSubmitter+1]);
			AssertNoOpenDentalInc(batch);
		}

		[TestMethod]
		public void X837_SenderTinSet_TheFixtureStays() {
			//The recorded fixtures have a Sender TIN: the clearinghouse is the sender, byte for byte as before (X837GoldenTests compares them too).
			Clearinghouse clearinghouseClin=Clearinghouses.OverrideFields(Clearinghouses.GetClearinghouse(GoldenData.ClearinghouseNum5010),0);
			Assert.AreEqual("991234567",clearinghouseClin.SenderTIN);
			string batch=X837GoldenTests.NormaliseDateTimes(x837Controller.GenerateBatch(clearinghouseClin,GetQueueItems(),BATCH_NUM,EnumClaimMedType.Dental));
			Golden.AssertText("x837-5010-dental.txt",batch);
		}

		[TestMethod]
		public void X270_BlankSenderTin_ThePracticeTinInIsa06AndGs02() {
			Claim claim=Claims.GetClaim(GoldenData.ClaimNum);
			InsPlan insPlan=InsPlans.GetPlan(claim.PlanNum,new List<InsPlan>());
			InsSub insSub=InsSubs.GetSub(claim.InsSubNum,null);
			Carrier carrier=Carriers.GetCarrier(insPlan.CarrierNum);
			Patient patient=Patients.GetPat(claim.PatNum);
			Provider provBill=Providers.GetProv(Providers.GetBillingProvNum(patient.PriProv,patient.ClinicNum));
			Clearinghouse clearinghouseClin=GetClearinghouseWithoutSender(GoldenData.ClearinghouseNum4010);
			string missing=X270.Validate(clearinghouseClin,carrier,provBill,null,insPlan,patient,insSub,patient);
			Assert.IsFalse(missing.Contains("Practice as sender"),missing);
			string message=X270.GenerateMessageText(clearinghouseClin,carrier,provBill,null,insPlan,patient,insSub,patient);
			List<string> listLines=message.Split(new[] { "\r\n" },StringSplitOptions.None).ToList();
			StringAssert.StartsWith(listLines[0],"ISA*00*          *00*          *ZZ*123456789      *ZZ*SYNRECEIVER    *");
			StringAssert.StartsWith(listLines[1],"GS*HS*123456789*SYNRECEIVER*");
			AssertNoOpenDentalInc(message);
		}

		[TestMethod]
		public void PracticeTitleBlank_BatchRefused_ClaimShowsWhatIsMissing() {
			Clearinghouse clearinghouseClin=GetClearinghouseWithoutSender(GoldenData.ClearinghouseNum5010);
			Prefs.UpdateString(PrefName.PracticeTitle,"");
			Cache.Refresh(InvalidType.Prefs);
			try {
				ODException odException=Assert.ThrowsExactly<ODException>(
					() => x837Controller.GenerateBatch(clearinghouseClin,GetQueueItems(),BATCH_NUM,EnumClaimMedType.Dental));
				StringAssert.StartsWith(odException.Message,"The batch was not created and nothing was sent.");
				StringAssert.Contains(odException.Message,"the practice title (Setup > Practice > Provider Name or Practice Title)");
				AssertNoOpenDentalInc(odException.Message);
				Assert.ThrowsExactly<ODException>(() => x837Controller.GenerateBatch(GetClearinghouseWithoutSender(GoldenData.ClearinghouseNum4010),
					GetQueueItems(),BATCH_NUM,EnumClaimMedType.Dental));
				Assert.IsFalse(x837Controller.CanMakeSender(clearinghouseClin,GetQueueItems(),true),"Automatic sends are refused without a window.");
				ClaimSendQueueItem queueItem=GetQueueItems()[0];
				X837_5010.Validate(clearinghouseClin,queueItem);
				StringAssert.Contains(queueItem.MissingData,"Practice as sender: the practice title (Setup > Practice > Provider Name or Practice Title)");
				//With a Sender TIN the clearinghouse is the sender: the practice title is not needed for it (the claims' own check still applies).
				Clearinghouse clearinghouseWithSender=Clearinghouses.OverrideFields(Clearinghouses.GetClearinghouse(GoldenData.ClearinghouseNum5010),0);
				Assert.IsTrue(x837Controller.CanMakeSender(clearinghouseWithSender,GetQueueItems(),true));
			}
			finally {
				Prefs.UpdateString(PrefName.PracticeTitle,GoldenData.PracticeTitle);
				Cache.Refresh(InvalidType.Prefs);
			}
		}

		[TestMethod]
		public void PhoneAndTinMissing_BatchRefused_ListsBoth() {
			Clearinghouse clearinghouseClin=GetClearinghouseWithoutSender(GoldenData.ClearinghouseNum5010);
			long provNumDefault=PrefC.GetLong(PrefName.PracticeDefaultProv);
			Prefs.UpdateString(PrefName.PracticePhone,"555010");
			Prefs.UpdateLong(PrefName.PracticeDefaultProv,0);
			Cache.Refresh(InvalidType.Prefs);
			try {
				ODException odException=Assert.ThrowsExactly<ODException>(
					() => x837Controller.GenerateBatch(clearinghouseClin,GetQueueItems(),BATCH_NUM,EnumClaimMedType.Dental));
				StringAssert.Contains(odException.Message,"(Setup > Practice > Default Insurance Billing Provider and Default Provider)");
				StringAssert.Contains(odException.Message,"a 10 digit telephone number (Setup > Practice > Phone)");
				AssertNoOpenDentalInc(odException.Message);
			}
			finally {
				Prefs.UpdateString(PrefName.PracticePhone,"5035550100");
				Prefs.UpdateLong(PrefName.PracticeDefaultProv,provNumDefault);
				Cache.Refresh(InvalidType.Prefs);
			}
		}
	}
}
