// X12-SENDER rule tests (docs/port-notes/X12-SENDER.md): who the sender of an X12 batch is when the clearinghouse has no Sender TIN. Owner
// decision of 2026-11-15: the practice's own TIN, name and telephone, never Open Dental Inc's. No database: the values come in through
// X12SenderSource, as X12SenderSource.FromCache fills them from the preference, clinic and provider caches. Synthetic values only.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using CodeBase;
using ForkMigrationTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalBusiness;

namespace BCLAIMTests {
	[TestClass]
	public class X12SenderTests {
		///<summary>Open Dental Inc's identities that upstream sent (837 4010 and 5010, the 270, and the Emdeon Medical variant).</summary>
		private static readonly string[] _arrayOpenDentalIncValues={ "810624427","204944520","8776861248","8662390469","OPEN DENTAL SOFTWARE",
			"Open Dental Software Inc.","JORDAN SPARKS" };
		private static readonly Provider _provDefault=new Provider { ProvNum=31,Abbr="SYNDEF",SSN="991110001",UsingTIN=true };
		private static readonly Provider _provSpecific=new Provider { ProvNum=32,Abbr="SYNBILL",SSN="991110002",UsingTIN=true };
		private static readonly Provider _provClinicDefault=new Provider { ProvNum=33,Abbr="SYNCLIN",SSN="991110003",UsingTIN=true };
		private static readonly Provider _provSsn=new Provider { ProvNum=34,Abbr="SYNSSN",SSN="991110004",UsingTIN=false };
		private static readonly Provider _provDashes=new Provider { ProvNum=35,Abbr="SYNDASH",SSN="99-1110005",UsingTIN=true };

		[TestInitialize]
		public void TestInitialize() {
			Thread.CurrentThread.CurrentCulture=new CultureInfo("en-US");
		}

		///<summary>A practice without clinics whose billing provider is the Default Practice Provider: TIN 991110001, its title and its phone.</summary>
		private static X12SenderSource NewSource() {
			List<Provider> listProviders=new List<Provider> { _provDefault,_provSpecific,_provClinicDefault,_provSsn,_provDashes };
			return new X12SenderSource {
				HasClinicsEnabled=false,
				Clinic=null,
				InsBillingProv=0,
				PracticeDefaultProv=_provDefault.ProvNum,
				FuncGetProv=(provNum) => listProviders.FirstOrDefault(x => x.ProvNum==provNum),
				PracticeTitle="Synthetic Smiles Dental",
				UseBillingAddressOnClaims=false,
				PracticeBillingPhone="5035550111",
				PracticePhone="5035550110",
			};
		}

		private static Clinic NewClinic(long insBillingProv,long defaultProv) {
			return new Clinic { ClinicNum=7,Abbr="SYNWEST",Description="Synthetic West",InsBillingProv=insBillingProv,DefaultProv=defaultProv,
				Phone="5035550170" };
		}

		private static X12Sender GetSender(X12SenderSource source) {
			List<string> listMissing;
			X12Sender sender=X12Sender.GetPracticeSender(source,out listMissing);
			Assert.IsNotNull(sender,string.Join("; ",listMissing));
			Assert.AreEqual(0,listMissing.Count);
			Assert.IsFalse(sender.IsFromClearinghouse);
			return sender;
		}

		private static List<string> GetMissing(X12SenderSource source) {
			List<string> listMissing;
			X12Sender sender=X12Sender.GetPracticeSender(source,out listMissing);
			Assert.IsNull(sender,"A practice sender with something missing is never made.");
			return listMissing;
		}

		#region Sender TIN set: today's behaviour
		[TestMethod]
		public void SenderTinSet_TheClearinghouseIsTheSender() {
			Clearinghouse clearinghouse=new Clearinghouse { Description="Synthetic clearinghouse",SenderTIN="ZZSYN0001",SenderName="Synthetic Billing Service",
				SenderTelephone="5035550199" };
			//GetSender does not read the practice at all when the clearinghouse has a Sender TIN (no cache and no database here).
			X12Sender sender=X12Sender.GetSender(clearinghouse,7);
			Assert.IsTrue(sender.IsFromClearinghouse);
			Assert.AreEqual("ZZSYN0001",sender.TIN);
			Assert.AreEqual("Synthetic Billing Service",sender.Name);
			Assert.AreEqual("5035550199",sender.Phone);
			Assert.AreEqual("ZZSYN0001      ",X12Generator.GetISA06(sender),"ISA06 is padded to 15.");
			Assert.AreEqual("ZZSYN0001",X12Generator.GetGS02(sender));
			Assert.AreEqual(0,X12Sender.GetMissing(clearinghouse,7).Count);
		}
		#endregion Sender TIN set: today's behaviour

		#region TIN from the practice's billing provider
		[TestMethod]
		public void DefaultPracticeProvider_TinOfTheDefaultProvider() {
			X12Sender sender=GetSender(NewSource());
			Assert.AreEqual("991110001",sender.TIN);
			Assert.AreEqual("Synthetic Smiles Dental",sender.Name);
			Assert.AreEqual("5035550110",sender.Phone);
			Assert.AreEqual("991110001      ",X12Generator.GetISA06(sender));
			Assert.AreEqual("991110001",X12Generator.GetGS02(sender));
		}

		[TestMethod]
		public void SpecificBillingProvider_ItsTin() {
			X12SenderSource source=NewSource();
			source.InsBillingProv=_provSpecific.ProvNum;
			Assert.AreEqual("991110002",GetSender(source).TIN,"The provider the claims name as billing provider (Providers.GetBillingProvNum).");
		}

		[TestMethod]
		public void TreatingProviderBills_NoClinic_TinOfTheDefaultProvider() {
			X12SenderSource source=NewSource();
			source.InsBillingProv=-1;
			Assert.AreEqual("991110001",GetSender(source).TIN,"Treating Provider names no single provider: the practice's Default Provider.");
		}

		[TestMethod]
		public void Clinic_SpecificBillingProvider_ItsTinAndTheClinicPhone() {
			X12SenderSource source=NewSource();
			source.HasClinicsEnabled=true;
			source.InsBillingProv=_provSpecific.ProvNum;//The practice's setting is not used for a clinic.
			source.Clinic=NewClinic(_provClinicDefault.ProvNum,0);
			X12Sender sender=GetSender(source);
			Assert.AreEqual("991110003",sender.TIN);
			Assert.AreEqual("5035550170",sender.Phone,"The phone the claims send in 2010AA PER04 for the clinic.");
			Assert.AreEqual("Synthetic Smiles Dental",sender.Name,"The claims send the practice title as the billing contact for every clinic.");
		}

		[TestMethod]
		public void Clinic_DefaultPracticeProvider_TinOfThePracticeDefaultProvider() {
			X12SenderSource source=NewSource();
			source.HasClinicsEnabled=true;
			source.Clinic=NewClinic(0,_provClinicDefault.ProvNum);
			Assert.AreEqual("991110001",GetSender(source).TIN,"0 is the practice's Default Provider, as Providers.GetBillingProvNum reads it.");
		}

		[TestMethod]
		public void Clinic_TreatingProviderBills_TinOfTheClinicDefaultProvider() {
			X12SenderSource source=NewSource();
			source.HasClinicsEnabled=true;
			source.Clinic=NewClinic(-1,_provClinicDefault.ProvNum);
			Assert.AreEqual("991110003",GetSender(source).TIN);
			source.Clinic=NewClinic(-1,0);
			Assert.AreEqual("991110001",GetSender(source).TIN,"Without a clinic Default Provider, the practice's.");
		}

		[TestMethod]
		public void ClinicsOff_TheClinicIsIgnored() {
			X12SenderSource source=NewSource();
			source.HasClinicsEnabled=false;
			source.Clinic=NewClinic(_provClinicDefault.ProvNum,_provClinicDefault.ProvNum);
			X12Sender sender=GetSender(source);
			Assert.AreEqual("991110001",sender.TIN);
			Assert.AreEqual("5035550110",sender.Phone);
		}

		[TestMethod]
		public void UseBillingAddressOnClaims_TheBillingPhone() {
			X12SenderSource source=NewSource();
			source.UseBillingAddressOnClaims=true;
			Assert.AreEqual("5035550111",GetSender(source).Phone);
			source.HasClinicsEnabled=true;
			source.Clinic=NewClinic(0,0);
			Assert.AreEqual("5035550111",GetSender(source).Phone,"The claims send the billing phone ahead of the clinic's (2010AA PER04).");
		}
		#endregion TIN from the practice's billing provider

		#region Missing: refused
		[TestMethod]
		public void NoBillingProvider_Refused() {
			X12SenderSource source=NewSource();
			source.PracticeDefaultProv=0;
			List<string> listMissing=GetMissing(source);
			Assert.AreEqual(1,listMissing.Count);
			StringAssert.Contains(listMissing[0],"billing provider");
			StringAssert.Contains(listMissing[0],"Setup > Practice > Default Insurance Billing Provider and Default Provider");
			source.PracticeDefaultProv=999;//A provider that is not in the cache.
			Assert.AreEqual(1,GetMissing(source).Count);
		}

		[TestMethod]
		public void BillingProviderUsesSsn_Refused() {
			X12SenderSource source=NewSource();
			source.InsBillingProv=_provSsn.ProvNum;
			List<string> listMissing=GetMissing(source);
			Assert.AreEqual(1,listMissing.Count);
			StringAssert.Contains(listMissing[0],"9 digit TIN");
			StringAssert.Contains(listMissing[0],"SYNSSN");
			StringAssert.Contains(listMissing[0],"marked TIN, not SSN");
			StringAssert.Contains(listMissing[0],"Lists > Providers > SYNSSN > SSN or TIN");
			Assert.IsFalse(listMissing[0].Contains("991110004"),"A social security number is never put in a message.");
		}

		[TestMethod]
		public void TinWithDashesOrBlank_Refused() {
			X12SenderSource source=NewSource();
			source.InsBillingProv=_provDashes.ProvNum;
			StringAssert.Contains(GetMissing(source).Single(),"SYNDASH");
			_provSpecific.SSN="";
			try {
				source.InsBillingProv=_provSpecific.ProvNum;
				StringAssert.Contains(GetMissing(source).Single(),"SYNBILL");
			}
			finally {
				_provSpecific.SSN="991110002";
			}
		}

		[TestMethod]
		public void ClinicBillingProviderMissing_SaysWhichClinic() {
			X12SenderSource source=NewSource();
			source.HasClinicsEnabled=true;
			source.PracticeDefaultProv=0;
			source.Clinic=NewClinic(0,0);
			StringAssert.Contains(GetMissing(source).Single(),"Lists > Clinics > SYNWEST > Default Insurance Billing Provider");
		}

		///<summary>CLEAN-1 (X12-SENDER's later cleanups): with Treating Provider billing, the TIN entry says why a Default Provider's TIN is the
		///sender TIN, naming the clinic's or the practice's Default Provider, in one whole sentence. Other settings do not get the sentence.</summary>
		[TestMethod]
		public void TreatingProviderBills_Missing_TheEntryNamesTheCase() {
			const string TREATING_PRACTICE="The Default Insurance Billing Provider is Treating Provider, which names no single provider, so the sender TIN "
				+"is that of the practice's Default Provider.";
			const string TREATING_CLINIC="The Default Insurance Billing Provider is Treating Provider, which names no single provider, so the sender TIN "
				+"is that of the clinic's Default Provider.";
			X12SenderSource source=NewSource();
			source.InsBillingProv=-1;
			source.PracticeDefaultProv=0;
			string missing=GetMissing(source).Single();
			StringAssert.Contains(missing,"Setup > Practice > Default Insurance Billing Provider and Default Provider");
			StringAssert.Contains(missing,TREATING_PRACTICE);
			source.PracticeDefaultProv=_provSsn.ProvNum;
			missing=GetMissing(source).Single();
			StringAssert.Contains(missing,"Lists > Providers > SYNSSN > SSN or TIN");
			StringAssert.Contains(missing,TREATING_PRACTICE);
			//A clinic with Treating Provider: its own Default Provider, or the practice's when it has none.
			source.HasClinicsEnabled=true;
			source.Clinic=NewClinic(-1,_provSsn.ProvNum);
			StringAssert.Contains(GetMissing(source).Single(),TREATING_CLINIC);
			source.Clinic=NewClinic(-1,0);
			StringAssert.Contains(GetMissing(source).Single(),TREATING_PRACTICE);
			//A specific billing provider, or the Default Practice Provider: no such sentence.
			source.Clinic=NewClinic(_provSsn.ProvNum,0);
			Assert.IsFalse(GetMissing(source).Single().Contains("Treating Provider"));
			source.Clinic=NewClinic(0,0);
			Assert.IsFalse(GetMissing(source).Single().Contains("Treating Provider"));
		}

		[TestMethod]
		public void BlankPracticeTitle_Refused() {
			X12SenderSource source=NewSource();
			source.PracticeTitle="  ";
			StringAssert.Contains(GetMissing(source).Single(),"Setup > Practice > Provider Name or Practice Title");
			source.PracticeTitle="@@@";//Nothing left once X12 strips what it cannot send.
			StringAssert.Contains(GetMissing(source).Single(),"practice title");
			source.PracticeTitle=null;
			Assert.AreEqual(1,GetMissing(source).Count);
		}

		[TestMethod]
		public void PhoneNotTenDigits_Refused_AndSaysWhere() {
			X12SenderSource source=NewSource();
			source.PracticePhone="503555011";
			StringAssert.Contains(GetMissing(source).Single(),"Setup > Practice > Phone");
			source.PracticePhone="(503)555-0110";
			Assert.AreEqual(1,GetMissing(source).Count,"Ten digits with no punctuation, as the claims' phone.");
			source.PracticePhone="";
			Assert.AreEqual(1,GetMissing(source).Count);
			source.UseBillingAddressOnClaims=true;
			source.PracticeBillingPhone="";
			StringAssert.Contains(GetMissing(source).Single(),"Setup > Practice > Billing Address > Phone");
			source.UseBillingAddressOnClaims=false;
			source.HasClinicsEnabled=true;
			Clinic clinic=NewClinic(0,0);
			clinic.Phone="";
			source.Clinic=clinic;
			StringAssert.Contains(GetMissing(source).Single(),"Lists > Clinics > SYNWEST > Phone");
		}

		[TestMethod]
		public void EverythingMissing_ListsAllThree() {
			X12SenderSource source=NewSource();
			source.PracticeDefaultProv=0;
			source.PracticeTitle="";
			source.PracticePhone="";
			List<string> listMissing=GetMissing(source);
			Assert.AreEqual(3,listMissing.Count,string.Join("; ",listMissing));
		}

		[TestMethod]
		public void RefusalMessage_SaysWhatAndWhere_AndNeverNamesOpenDentalInc() {
			X12SenderSource source=NewSource();
			source.PracticeTitle="";
			source.PracticePhone="";
			List<string> listMissing=GetMissing(source);
			Clearinghouse clearinghouse=new Clearinghouse { Description="Synthetic clearinghouse",SenderTIN="" };
			string message=X12Sender.GetRefusalMessage(clearinghouse,listMissing);
			StringAssert.StartsWith(message,"The batch was not created and nothing was sent.");
			StringAssert.Contains(message,"Synthetic clearinghouse has no Sender TIN, so the practice is the sender");
			StringAssert.Contains(message,"- the practice title (Setup > Practice > Provider Name or Practice Title)");
			StringAssert.Contains(message,"- a 10 digit telephone number (Setup > Practice > Phone)");
			StringAssert.Contains(message,"Setup > Family/Insurance > Clearinghouses");
			foreach(string value in _arrayOpenDentalIncValues) {
				Assert.IsFalse(message.Contains(value),value);
			}
		}
		#endregion Missing: refused

		#region Source scan
		///<summary>The X12 writers no longer carry Open Dental Inc's TIN, name or telephone anywhere (code or comments). The received files' samples
		///(X277, X835, X997) are not senders and keep theirs.</summary>
		[TestMethod]
		public void SourceScan_NoWriterNamesOpenDentalInc() {
			string[] arrayFiles={ @"X12\X837_5010.cs",@"X12\X837_4010.cs",@"X12\X270.cs",@"X12\X12Generator.cs",@"X12\X12Sender.cs",@"X12\X12Validate.cs",
				@"Eclaims\x837Controller.cs",@"Eclaims\x270Controller.cs",@"TableTypes\Clearinghouse.cs" };
			List<string> listOffenders=new List<string>();
			foreach(string file in arrayFiles) {
				string source=File.ReadAllText(TestDb.FindRepoFile(Path.Combine("OpenPmsDentalBusiness",file)));
				foreach(string value in _arrayOpenDentalIncValues) {
					if(source.IndexOf(value,StringComparison.OrdinalIgnoreCase)>=0) {
						listOffenders.Add(file+": "+value);
					}
				}
			}
			Assert.AreEqual(0,listOffenders.Count,string.Join("\r\n",listOffenders));
		}

		///<summary>Every writer gets its sender from X12Sender, so the rule is the same for the 837 4010 and 5010 and the 270, and for HQ and clinic
		///clearinghouses.</summary>
		[TestMethod]
		public void SourceScan_EveryWriterUsesX12Sender() {
			foreach(string file in new[] { "X837_5010.cs","X837_4010.cs","X270.cs" }) {
				string source=File.ReadAllText(TestDb.FindRepoFile(Path.Combine("OpenPmsDentalBusiness","X12",file)));
				Assert.AreEqual(1,Regex.Matches(source,@"X12Sender\.GetSender\(clearinghouseClin,").Count,file);
				Assert.IsFalse(Regex.IsMatch(source,@"GetISA06\(clearinghouseClin\)|GetGS02\(clearinghouseClin\)"),file);
				Assert.IsFalse(Regex.IsMatch(source,@"SenderTIN\s*==\s*""""|SenderTIN\s*!=\s*"""""),file+": the writers no longer choose the sender themselves.");
			}
		}
		#endregion Source scan
	}
}
