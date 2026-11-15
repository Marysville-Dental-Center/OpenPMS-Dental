// The address rules of S3a-9 (v) of the .NET 10 plan (docs/specs/net10-plan.md; docs/port-notes/S3a-9.md, section 5). .NET 10's MailAddress
// refuses an address with two dots in a row (https://learn.microsoft.com/en-us/dotnet/core/compatibility/10, Networking); .NET Framework takes
// it. These tests pin today's result for a..b@example.com at each site (MailAddress, MailAddressCollection, the sending sites' message builders
// and the validation in EmailAddresses.GetValidMailAddress); stage 3b changes the "Today" tests to the .NET 10 result, which is the
// not-valid-address error the sending sites give for any address MailAddress refuses (the NotValid tests, the same on both runtimes).
// Synthetic addresses only; nothing is sent and nothing connects.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Linq;
using System.Net.Mail;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MimeKit;
using OpenPmsDentalBusiness;
using OpenPmsDentalBusiness.Email;

namespace LibraryUpdateTests {
	[TestClass]
	public class AddressRulesTests {
		private const string ADDRESS_DOTS="a..b@example.com";
		private const string ADDRESS_FROM="front@example.test";

		private static BasicEmailMessage MakeMessage(string fromAddress,string toAddress,string ccAddress="",string bccAddress="") {
			return new BasicEmailMessage() {
				FromAddress=fromAddress,
				ToAddress=toAddress,
				CcAddress=ccAddress,
				BccAddress=bccAddress,
				Subject="Synthetic",
				BodyText="Synthetic body.",
			};
		}

		private static BasicEmailAddress MakeAddress() {
			return new BasicEmailAddress() { EmailUsername=ADDRESS_FROM,AuthenticationType=BasicOAuthType.Google };
		}

		#region Today (.NET Framework): two dots in a row are taken; 3b changes these to the .NET 10 result
		[TestMethod]
		public void Today_MailAddress_TakesConsecutiveDots() {
			MailAddress mailAddress=new MailAddress(ADDRESS_DOTS);
			Assert.AreEqual(ADDRESS_DOTS,mailAddress.Address);
			Assert.AreEqual("a..b",mailAddress.User);
			Assert.AreEqual("example.com",mailAddress.Host);
			MailAddressCollection mailAddressCollection=new MailAddressCollection();
			mailAddressCollection.Add("x@example.com, "+ADDRESS_DOTS);
			CollectionAssert.AreEqual(new[] { "x@example.com",ADDRESS_DOTS },mailAddressCollection.Select(x => x.Address).ToArray());
		}

		///<summary>The MIME builder of the Gmail, Microsoft 365 and MailKit SMTP sends: MailAddress takes the address today, then MimeKit's
		///MailboxAddress refuses it ("Invalid local-part"), for From, To, Cc and Bcc alike. On .NET 10 MailAddress refuses it first, and the
		///builder throws the not-valid-address ODException (NotValid_CreateMIMEMsg_SaysTheAddressIsNotValid) before MimeKit sees it.</summary>
		[TestMethod]
		public void Today_CreateMIMEMsg_MimeKitRefusesConsecutiveDots() {
			Assert.ThrowsExactly<ParseException>(() => SendEmail.CreateMIMEMsg(MakeAddress(),MakeMessage(ADDRESS_FROM,ADDRESS_DOTS)));
			Assert.ThrowsExactly<ParseException>(() => SendEmail.CreateMIMEMsg(MakeAddress(),MakeMessage(ADDRESS_FROM,"",ADDRESS_DOTS)));
			Assert.ThrowsExactly<ParseException>(() => SendEmail.CreateMIMEMsg(MakeAddress(),MakeMessage(ADDRESS_FROM,"","",ADDRESS_DOTS)));
			Assert.ThrowsExactly<ParseException>(() => SendEmail.CreateMIMEMsg(MakeAddress(),MakeMessage(ADDRESS_DOTS,ADDRESS_FROM)));
			Assert.AreEqual(ADDRESS_FROM,SendEmail.CreateMIMEMsg(MakeAddress(),MakeMessage(ADDRESS_FROM,ADDRESS_FROM)).To.Mailboxes.Single().Address,
				"A valid address builds.");
		}

		///<summary>The System.Net.Mail builder of the SMTP send on ports other than 465 without a signature: it takes the address today, and the
		///SMTP server decides.</summary>

		[TestMethod]
		public void Today_BasicEmailMessageToMailMessage_SendsToConsecutiveDots() {
			using(MailMessage mailMessage=SendEmail.BasicEmailMessageToMailMessage(MakeMessage(ADDRESS_DOTS,ADDRESS_DOTS,ADDRESS_DOTS,ADDRESS_DOTS))) {
				Assert.AreEqual(ADDRESS_DOTS,mailMessage.From.Address);
				Assert.AreEqual(ADDRESS_DOTS,mailMessage.To.Single().Address);
				Assert.AreEqual(ADDRESS_DOTS,mailMessage.CC.Single().Address);
				Assert.AreEqual(ADDRESS_DOTS,mailMessage.Bcc.Single().Address);
			}
		}
		#endregion Today (.NET Framework): two dots in a row are taken; 3b changes these to the .NET 10 result

		#region The same on both runtimes
		///<summary>The validation site says not valid on both runtimes: today its pattern refuses two dots in a row, on .NET 10 MailAddress does
		///and its catch returns null.</summary>
		[TestMethod]
		public void GetValidMailAddress_ConsecutiveDots_IsNotValid() {
			Assert.IsNull(EmailAddresses.GetValidMailAddress(ADDRESS_DOTS));
			Assert.IsNull(EmailAddresses.GetValidMailAddress("a@example..com"));
			Assert.IsNull(EmailAddresses.GetValidMailAddress("not an address"));
			Assert.AreEqual("a.b@example.com",EmailAddresses.GetValidMailAddress("a.b@example.com")?.Address,"One dot is fine.");
		}

		///<summary>An address that MailAddress refuses on both runtimes reads as not valid at the sending sites, with the FormatException
		///inside, for From, To, Cc and Bcc: what two dots in a row give on .NET 10.</summary>
		[TestMethod]
		public void NotValid_CreateMIMEMsg_SaysTheAddressIsNotValid() {
			AssertNotValid("not an address",() => SendEmail.CreateMIMEMsg(MakeAddress(),MakeMessage(ADDRESS_FROM,"not an address")));
			AssertNotValid("bob@",() => SendEmail.CreateMIMEMsg(MakeAddress(),MakeMessage("bob@",ADDRESS_FROM)));
			AssertNotValid("x@example.com, bob@",() => SendEmail.CreateMIMEMsg(MakeAddress(),MakeMessage(ADDRESS_FROM,ADDRESS_FROM,"x@example.com, bob@")));
			AssertNotValid("bob@",() => SendEmail.CreateMIMEMsg(MakeAddress(),MakeMessage(ADDRESS_FROM,"","","bob@")));
		}

		[TestMethod]
		public void NotValid_BasicEmailMessageToMailMessage_SaysTheAddressIsNotValid() {
			AssertNotValid("not an address",() => SendEmail.BasicEmailMessageToMailMessage(MakeMessage(ADDRESS_FROM,"not an address")));
			AssertNotValid("bob@",() => SendEmail.BasicEmailMessageToMailMessage(MakeMessage(" bob@ ",ADDRESS_FROM)));
			AssertNotValid("bob@",() => SendEmail.BasicEmailMessageToMailMessage(MakeMessage(ADDRESS_FROM,ADDRESS_FROM,"bob@")));
			AssertNotValid("bob@",() => SendEmail.BasicEmailMessageToMailMessage(MakeMessage(ADDRESS_FROM,ADDRESS_FROM,"","bob@")));
		}

		///<summary>CodeBase's EmailUtils.SendEmail refuses the address before it sends anything (the message is built before SmtpClient.Send;
		///the server name is a reserved .test name in any case).</summary>
		[TestMethod]
		public void NotValid_EmailUtilsSendEmail_SaysTheAddressIsNotValid() {
			AssertNotValid("not an address",() => EmailUtils.SendEmail("not an address","Synthetic","Synthetic body.",ADDRESS_FROM,"",
				"Front Desk","smtp.example.test",25));
			AssertNotValid("bob@",() => EmailUtils.SendEmail(ADDRESS_FROM,"Synthetic","Synthetic body.","bob@","","Front Desk","smtp.example.test",25));
		}

		///<summary>A blank address is not a FormatException and stays as MailAddress throws it.</summary>
		[TestMethod]
		public void BlankFrom_StaysAnArgumentException() {
			Assert.ThrowsExactly<ArgumentException>(() => SendEmail.CreateMIMEMsg(MakeAddress(),MakeMessage("",ADDRESS_FROM)));
		}
		#endregion The same on both runtimes

		private static void AssertNotValid(string address,Action action) {
			ODException odException=Assert.ThrowsExactly<ODException>(action);
			Assert.AreEqual("This is not a valid email address: "+address,odException.Message);
			Assert.IsInstanceOfType<FormatException>(odException.InnerException);
		}
	}
}
