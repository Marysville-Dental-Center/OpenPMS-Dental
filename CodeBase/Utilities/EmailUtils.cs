using System;
using System.Net;
using System.Net.Mail;

namespace CodeBase {
	public class EmailUtils {

		public static void SendEmail(string toAddress,string subject,string emailBody, string fromAddress,string fromPassword,string fromDisplayName,
			string fromSMTPserver,int fromServerPort,bool enableSsl=false,bool isBodyHtml=true) 
		{
			SmtpClient client=new SmtpClient(fromSMTPserver,fromServerPort);
			client.Credentials=new NetworkCredential(fromAddress,fromPassword);
			client.DeliveryMethod=SmtpDeliveryMethod.Network;
			client.EnableSsl=enableSsl;
			client.Timeout=180000;//3 minutes
			MailMessage message=new MailMessage();
			//.NET 10's MailAddress also refuses an address with two dots in a row (a..b@example.com), which .NET Framework takes: an address that
			//is not valid reads as such instead of a bare FormatException (docs/port-notes/S3a-9.md, section 5).
			try {
				message.From=new MailAddress(fromAddress.Trim(),fromDisplayName);
			}
			catch(FormatException ex) {
				throw new ODException("This is not a valid email address: "+fromAddress.Trim(),ex);
			}
			if(!string.IsNullOrWhiteSpace(toAddress)) {
				try {
					message.To.Add(toAddress.Trim());
				}
				catch(FormatException ex) {
					throw new ODException("This is not a valid email address: "+toAddress.Trim(),ex);
				}
			}
			message.Subject=subject;
			message.Body=emailBody;
			message.IsBodyHtml=isBodyHtml;
			client.Send(message);
		}
	}
}
