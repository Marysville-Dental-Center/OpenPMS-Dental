// Golden outputs: the X12 837 dental batch the program writes for the fixture's claim, in 5010 and in 4010 (x837Controller.GenerateBatch, as
// Send Claims calls it), recorded with the .NET Framework build (docs/port-notes/S3a-4.md).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalBusiness;
using OpenPmsDentalBusiness.Eclaims;

namespace GoldenOutputTests {
	[TestClass]
	public class X837GoldenTests {
		///<summary>The batch number the fixture batches carry (ISA13, GS06, IEA02).</summary>
		private const int BATCH_NUM=7;

		[TestInitialize]
		public void TestInitialize() {
			GoldenDb.Begin();
		}

		private static string GenerateBatch(long clearinghouseNum) {
			Clearinghouse clearinghouseClin=Clearinghouses.OverrideFields(Clearinghouses.GetClearinghouse(clearinghouseNum),0);
			List<ClaimSendQueueItem> listQueueItems=Claims.GetQueueList(GoldenData.ClaimNum,0,0).ToList();
			Assert.AreEqual(1,listQueueItems.Count);
			return x837Controller.GenerateBatch(clearinghouseClin,listQueueItems,BATCH_NUM,EnumClaimMedType.Dental);
		}

		///<summary>The one documented normalisation of the 837 batches: the interchange date and time (ISA09, ISA10), the functional group's
		///(GS04, GS05) and the transaction's (BHT04, BHT05) are the moment the batch is written, so they are replaced with the tokens
		///@ISA09@, @ISA10@, @GS04@, @GS05@, @BHT04@ and @BHT05@. Nothing else is changed. The element separator is the fixture clearinghouse's
		///default '*'.</summary>
		public static string NormaliseDateTimes(string batch) {
			string[] arrayLines=batch.Split(new[] { "\r\n" },StringSplitOptions.None);
			for(int i=0;i<arrayLines.Length;i++) {
				string[] arrayElements=arrayLines[i].Split('*');
				if(arrayElements[0]=="ISA" && arrayElements.Length>10) {
					arrayElements[9]="@ISA09@";
					arrayElements[10]="@ISA10@";
				}
				else if(arrayElements[0]=="GS" && arrayElements.Length>5) {
					arrayElements[4]="@GS04@";
					arrayElements[5]="@GS05@";
				}
				else if(arrayElements[0]=="BHT" && arrayElements.Length>5) {
					arrayElements[4]="@BHT04@";
					arrayElements[5]="@BHT05@";
				}
				else {
					continue;
				}
				arrayLines[i]=string.Join("*",arrayElements);
			}
			return string.Join("\r\n",arrayLines);
		}

		[TestMethod]
		public void X837_5010_Dental_Batch() {
			string batch=GenerateBatch(GoldenData.ClearinghouseNum5010);
			StringAssert.StartsWith(batch,"ISA*");
			StringAssert.Contains(batch,"005010X224A2");
			Golden.AssertText("x837-5010-dental.txt",NormaliseDateTimes(batch));
		}

		///<summary>The program still writes 4010 batches for a clearinghouse set to x837D_4010 (x837Controller.GenerateBatch).</summary>
		[TestMethod]
		public void X837_4010_Dental_Batch() {
			string batch=GenerateBatch(GoldenData.ClearinghouseNum4010);
			StringAssert.StartsWith(batch,"ISA*");
			StringAssert.Contains(batch,"004010X097A1");
			Golden.AssertText("x837-4010-dental.txt",NormaliseDateTimes(batch));
		}

		[TestMethod]
		public void NormaliseDateTimes_ReplacesOnlyTheSixDateAndTimeElements() {
			string batch="ISA*00*          *00*          *ZZ*SENDER         *ZZ*RECEIVER       *261115*0930*^*00501*000000007*0*T*:~\r\n"
				+"GS*HC*SENDER*RECEIVER*20261115*0930*7*X*005010X224A2~\r\n"
				+"BHT*0019*00*0001*20261115*093012*CH~\r\n"
				+"DTP*472*D8*20250310~\r\n";
			string expected="ISA*00*          *00*          *ZZ*SENDER         *ZZ*RECEIVER       *@ISA09@*@ISA10@*^*00501*000000007*0*T*:~\r\n"
				+"GS*HC*SENDER*RECEIVER*@GS04@*@GS05@*7*X*005010X224A2~\r\n"
				+"BHT*0019*00*0001*@BHT04@*@BHT05@*CH~\r\n"
				+"DTP*472*D8*20250310~\r\n";
			Assert.AreEqual(expected,NormaliseDateTimes(batch));
		}
	}
}
