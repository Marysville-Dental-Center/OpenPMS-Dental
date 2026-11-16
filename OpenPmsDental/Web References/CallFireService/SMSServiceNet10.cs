// CallFire SMS texting on .NET 10 (S3b-4 of docs/specs/net10-plan.md, docs/port-notes/S3b-4.md). The generated proxy beside this file
// (Reference.cs) is a System.Web.Services SoapHttpClientProtocol, which .NET does not have, so it compiles for the .NET Framework only, and
// sending a text through CallFire waits for its port: open point O-10 of the plan (the kept integrations, owner 2026-11-08). Until then this
// class keeps the proxy's members that the program uses (FormTxtMsgEdit, FormEtrans835s's designer), and sendSMSCampaign throws
// NotSupportedException with a message that names O-10, which FormTxtMsgEdit shows ("Error sending text message.").
// The two messages of the service (SMSService.wsdl, document/literal, wrapped) are declared here as the classes the port serializes: the
// XmlSerializer generator writes their serializers into OpenPmsDental.XmlSerializers.dll (SGenTypes in OpenPmsDental.csproj), the file that
// held the proxy's serializers when SGen built it on the .NET Framework.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
#if !NETFRAMEWORK
using System;
using System.ComponentModel;
using System.Net;
using System.Xml.Serialization;

namespace OpenPmsDental.CallFireService {
	///<summary>CallFire's SMS service. On .NET 10 sendSMSCampaign throws NotSupportedException (open point O-10); see the file's header.</summary>
	[DesignerCategory("code")]
	public partial class SMSService:Component {
		///<summary>The service's namespace (SMSService.wsdl, targetNamespace).</summary>
		public const string Namespace="http://api.campaign.dialer.skyyconsulting.com";
		///<summary>The text of the exception sendSMSCampaign throws, naming the plan row.</summary>
		public const string NotPortedMessage="CallFire text messages are not available yet in the .NET 10 build of Open PMS - Dental: the SOAP client "
			+"CallFire's service needs is not part of .NET, and its port is open point O-10 of the .NET 10 plan (docs/specs/net10-plan.md). "
			+"No text message was sent.";

		///<summary>The service address, from Properties.Settings as the generated proxy took it.</summary>
		public string Url { get; set; }
		///<summary>Kept from the proxy for the designer code that sets it; not used.</summary>
		public ICredentials Credentials { get; set; }
		///<summary>Kept from the proxy for the designer code that sets it; not used.</summary>
		public bool UseDefaultCredentials { get; set; }

		public SMSService() {
			Url=global::OpenPmsDental.Properties.Settings.Default.OpenDental_CallFireService_SMSService;
		}

		///<summary>Throws NotSupportedException with NotPortedMessage: nothing is sent (open point O-10).</summary>
		public long sendSMSCampaign(string key,string[] numbers,string campaignName) {
			throw new NotSupportedException(NotPortedMessage);
		}
	}

	///<summary>The request message sendSMSCampaign (SMSService.wsdl: element tns:sendSMSCampaign; key, numbers as tns:ArrayOfString of string
	///elements, campaignName; each nillable, qualified in the service's namespace).</summary>
	[XmlRoot("sendSMSCampaign",Namespace=SMSService.Namespace)]
	public class SendSMSCampaignRequest {
		[XmlElement("key",IsNullable=true)]
		public string Key;
		[XmlArray("numbers",IsNullable=true)]
		[XmlArrayItem("string",IsNullable=true)]
		public string[] Numbers;
		[XmlElement("campaignName",IsNullable=true)]
		public string CampaignName;
	}

	///<summary>The response message sendSMSCampaignResponse (SMSService.wsdl: element tns:sendSMSCampaignResponse; out, a long).</summary>
	[XmlRoot("sendSMSCampaignResponse",Namespace=SMSService.Namespace)]
	public class SendSMSCampaignResponse {
		[XmlElement("out")]
		public long Out;
	}
}
#endif
