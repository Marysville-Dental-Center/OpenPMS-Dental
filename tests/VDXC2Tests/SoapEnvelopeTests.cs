// S3a-6 (docs/specs/net10-plan.md; docs/port-notes/S3a-6.md): the hand-written SOAP clients (SoapCaller, SoapClients.cs) send what the
// generated SoapHttpClientProtocol proxies sent, for every operation the program uses: DentalXChange DWS (4), DEA (6), NewCrop Update1 (2),
// Change Healthcare ITS (2) and DentalXChange DCI's rpc/encoded lookupEligibility (1). The commit before the proxies were deleted
// (7687144aa) called each old proxy through a WebRequest.RegisterPrefix fake and each new client with the same synthetic arguments and the same
// canned response, found the envelopes byte for byte the same, and recorded the old proxy's request, headers and parsed result under
// SoapEnvelopes (OPMS_SOAP_RECORD=1). These tests now check the new clients against those recordings: an equivalent envelope (canonical XML:
// names, namespaces, attributes with xsi:type resolved, text), the same SOAPAction, content type and method, and the same parsed result; the
// fault texts and codes the proxies gave are checked the same way. No call leaves the process: the new clients only reach the test handler.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using ForkMigrationTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalBusiness;
using Dci=OpenPmsDentalBusiness.com.dentalxchange.webservices;
using Dea=OpenPmsDentalBusiness.DentalxchangePartnerService;
using Dws=OpenPmsDentalBusiness.Dentalxchange2016;
using Its=OpenPmsDentalBusiness.EmdeonITS;
using Nc=OpenPmsDentalBusiness.NewCropWebService;

namespace VDXC2Tests {
	[TestClass]
	public class SoapEnvelopeTests {
		private const string FIXTURE_FOLDER="SoapEnvelopes";
		private SoapCaptureHandler _soapCaptureHandler;

		[TestInitialize]
		public void TestInitialize() {
			_soapCaptureHandler=new SoapCaptureHandler();
			SoapCaller.HttpHandlerForTests=_soapCaptureHandler;
		}

		[TestCleanup]
		public void TestCleanup() {
			SoapCaller.HttpHandlerForTests=null;
		}

		#region Synthetic arguments
		private static Dws.Credentials DwsCredentials() {
			return new Dws.Credentials { Client="Synthetic Partner",ServiceID="TEST-SERVICE-0001",Username="office-login",
				Password="office-password & <not real>",version="26.1.0.0" };
		}

		private static Dea.Credentials DeaCredentials() {
			return new Dea.Credentials { Client="Synthetic Partner",ServiceID="TEST-SERVICE-0001",Username="office-login",Password="office-password" };
		}

		private static Dea.Attachment DeaAttachment() {
			return new Dea.Attachment {
				ProviderClaimID="1234/1",PayerIdCode="ZZ999",Narrative="Synthetic narrative: tooth #3 & #4",SubscriberId="SUB0001",
				PatientLastName="Synthetic",PatientFirstName="Patient",PatientDOB=new DateTime(1980,1,2),DateOfService=new DateTime(2026,10,1),
				DateOfServiceSpecified=true,BillProviderNpi="1234567893",BillProviderLastName="Dentist",BillProviderFirstName="Synthetic",
				BillProviderTaxonomy="1223G0001X",BillProviderAdd1="1 Example Way",BillProviderCity="Exampletown",BillProviderState="ZZ",
				BillProviderZip="00000",BillProviderTaxID="000000000",SubscriberLastName="Synthetic",SubscriberFirstName="Subscriber",
			};
		}

		private static Dea.AttachmentImage DeaImage() {
			return new Dea.AttachmentImage { ImageTypeCode=Dea.imageTypeCode.RB,ImageTypeCodeSpecified=true,ImageFileAsBase64=new byte[] { 0xFF,0xD8,1,2,3,0 },
				ImageFileName="Bitewing",ImageOrientationType=Dea.orientationType.RIGHT,ImageOrientationTypeSpecified=true,ImageDate=new DateTime(2026,10,2),
				ImageDateSpecified=true };
		}

		private static Nc.Credentials NcCredentials() {
			return new Nc.Credentials { PartnerName="SyntheticPartner",Name="synthetic-account",Password="not-a-real-password" };
		}

		private static Nc.AccountRequest NcAccount() {
			return new Nc.AccountRequest { AccountId="OPMS-TEST000001",SiteId="1" };
		}
		#endregion Synthetic arguments

		#region Responses
		///<summary>A document/literal response: the wrapper in responseNamespace, holding returnValue serialized as the element returnName in
		///returnNamespace ("" for the DentalXChange services' unqualified "return").</summary>
		private static string Response(string operation,string responseNamespace,string returnName,string returnNamespace,object returnValue) {
			XmlSerializer xmlSerializer=new XmlSerializer(returnValue.GetType(),new XmlRootAttribute(returnName) { Namespace=returnNamespace });
			StringBuilder stringBuilder=new StringBuilder();
			using(XmlWriter xmlWriter=XmlWriter.Create(stringBuilder,new XmlWriterSettings { OmitXmlDeclaration=true })) {
				xmlSerializer.Serialize(xmlWriter,returnValue);
			}
			return "<?xml version=\"1.0\" encoding=\"utf-8\"?>"
				+"<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\"><soapenv:Header/><soapenv:Body>"
				+"<ns2:"+operation+"Response xmlns:ns2=\""+responseNamespace+"\">"+stringBuilder.ToString()+"</ns2:"+operation+"Response>"
				+"</soapenv:Body></soapenv:Envelope>";
		}

		private static string DwsResponse(string operation,object returnValue) {
			return Response(operation,DwsSoapClient.Namespace,"return","",returnValue);
		}

		private static string DeaResponse(string operation,object returnValue) {
			return Response(operation,DeaSoapClient.Namespace,"return","",returnValue);
		}

		private static string NcResponse(string operation,object returnValue) {
			return Response(operation,NewCropConfig.SoapNamespace,operation+"Result",NewCropConfig.SoapNamespace,returnValue);
		}

		private static string ItsResponse(string operation,object returnValue) {
			return Response(operation,ItsSoapClient.Namespace,operation+"Result",ItsSoapClient.Namespace,returnValue);
		}

		private static Dws.Status StatusOk() {
			return new Dws.Status { code=0,description="Approved" };
		}

		private static Dea.Status DeaStatusOk() {
			return new Dea.Status { code=0,description="OK" };
		}

		///<summary>DCI's answer the way Apache Axis writes rpc/encoded: the return as an href to a multiRef element after the wrapper.</summary>
		private const string DCI_RESPONSE_MULTIREF="<?xml version=\"1.0\" encoding=\"UTF-8\"?>"
			+"<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\""
			+" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><soapenv:Body>"
			+"<ns1:lookupEligibilityResponse soapenv:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\" xmlns:ns1=\"http://www.dentalxchange.com/webservice\">"
			+"<lookupEligibilityReturn href=\"#id0\"/></ns1:lookupEligibilityResponse>"
			+"<multiRef id=\"id0\" soapenc:root=\"0\" soapenv:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\" xsi:type=\"ns2:Response\""
			+" xmlns:soapenc=\"http://schemas.xmlsoap.org/soap/encoding/\" xmlns:ns2=\"http://www.dentalxchange.com/webservice\">"
			+"<content xsi:type=\"xsd:string\">&lt;EligBenefitResponse&gt;&lt;isEligible&gt;0&lt;/isEligible&gt;&lt;/EligBenefitResponse&gt;</content>"
			+"</multiRef></soapenv:Body></soapenv:Envelope>";

		///<summary>The same answer with the return written inline.</summary>
		private const string DCI_RESPONSE_INLINE="<?xml version=\"1.0\" encoding=\"UTF-8\"?>"
			+"<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\""
			+" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><soapenv:Body soapenv:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\">"
			+"<ns1:lookupEligibilityResponse xmlns:ns1=\"http://www.dentalxchange.com/webservice\">"
			+"<lookupEligibilityReturn xsi:type=\"ns1:Response\"><content xsi:type=\"xsd:string\">&lt;EligBenefitResponse&gt;&lt;isEligible&gt;1&lt;/isEligible&gt;"
			+"&lt;/EligBenefitResponse&gt;</content></lookupEligibilityReturn></ns1:lookupEligibilityResponse>"
			+"</soapenv:Body></soapenv:Envelope>";
		#endregion Responses

		#region DentalXChange DWS
		[TestMethod]
		public void Dws_ValidateClaim() {
			Dws.textRequest textRequest=new Dws.textRequest { outputFormat=Dws.Format.XML,validateForAttachment=true,
				Content="ISA*00*          *00*~ST*837*0001~SE*2*0001~" };
			AssertEquivalent("Dws.validateClaim",
				() => new DwsSoapClient { Url=NewUrl("dws") }.validateClaim(DwsCredentials(),textRequest),
				DwsResponse("validateClaim",new Dws.textResponse { Status=StatusOk(),Content="ISA*00*~ST*277*0001~STC*A1:20*20261001*WQ*0~SE*3*0001~" }));
		}

		[TestMethod]
		public void Dws_LookupEligibility() {
			Dws.textRequest textRequest=new Dws.textRequest { outputFormat=Dws.Format.HTML,outputFormatSpecified=true,
				Content="ISA*00*~ST*270*0001~NM1*IL*1*SYNTHETIC*PATIENT~SE*3*0001~ &amp; &lt;" };
			AssertEquivalent("Dws.lookupEligibility",
				() => new DwsSoapClient { Url=NewUrl("dws") }.lookupEligibility(DwsCredentials(),textRequest),
				DwsResponse("lookupEligibility",new Dws.textResponse { Status=StatusOk(),Content="<table><tr><td>Synthetic</td></tr></table>" }));
		}

		[TestMethod]
		public void Dws_LookupEligibility_NullsAndNoContent() {
			//No content in the answer: Benefits270 then reports the status. The request's own nulls are left out, as the proxy left them out.
			Dws.Credentials credentials=new Dws.Credentials { Client="Synthetic Partner",Username="office-login" };
			Dws.textRequest textRequest=new Dws.textRequest();
			AssertEquivalent("Dws.lookupEligibility.nulls",
				() => new DwsSoapClient { Url=NewUrl("dws") }.lookupEligibility(credentials,textRequest),
				DwsResponse("lookupEligibility",new Dws.textResponse { Status=new Dws.Status { code=100,description="Invalid User" } }));
		}

		[TestMethod]
		public void Dws_GetUnProcessedEra() {
			Dws.unProcessedEraRequest unProcessedEraRequest=new Dws.unProcessedEraRequest();
			AssertEquivalent("Dws.getUnProcessedEra",
				() => new DwsSoapClient { Url=NewUrl("dws") }.getUnProcessedEra(DwsCredentials(),unProcessedEraRequest),
				DwsResponse("getUnProcessedEra",new Dws.UnProcessedEraResponse { Status=StatusOk(),
					ClaimPaymentAdvice=new Dws.claimPaymentAdvice { AdditionalEraExists=true,EdiContent="ISA*00*~ST*835*0001~SE*2*0001~" } }));
		}

		[TestMethod]
		public void Dws_GetPayerListService() {
			Dws.payerListInfoRequest payerListInfoRequest=new Dws.payerListInfoRequest { outputFormat=Dws.Format.XML,outputFormatSpecified=true };
			Dws.supportedTransPayer supportedTransPayer=new Dws.supportedTransPayer { PayerIDCode="ZZ999",Name="Synthetic Payer",
				Eligibility=new Dws.supportedTransBean { isSupported=true,isSupportedSpecified=true,
					Requirements=new[] { new Dws.requireBean { name="PatientAndSubscriberReqdForElig",value="true" } } },
				Claim=new Dws.supportedTransBean { isSupported=false,isSupportedSpecified=true } };
			AssertEquivalent("Dws.getPayerListService",
				() => new DwsSoapClient { Url=NewUrl("dws") }.getPayerListService(DwsCredentials(),payerListInfoRequest),
				DwsResponse("getPayerListService",new Dws.PayerListInfoResponse { Status=StatusOk(),Payers=new[] { supportedTransPayer } }));
		}
		#endregion DentalXChange DWS

		#region DentalXChange DEA
		[TestMethod]
		public void Dea_OpenAttachment() {
			AssertEquivalent("Dea.openAttachment",
				() => new DeaSoapClient { Url=NewUrl("dea") }.openAttachment(DeaCredentials(),DeaAttachment()),
				DeaResponse("openAttachment",new Dea.AttachmentReferenceResponse { MsgSuccess=true,Status=DeaStatusOk(),
					AttachmentReference=new Dea.AttachmentReference { AttachmentID="DXC-TEST-1" } }));
		}

		[TestMethod]
		public void Dea_AddImage() {
			Dea.AttachmentReference attachmentReference=new Dea.AttachmentReference { AttachmentID="DXC-TEST-1" };
			AssertEquivalent("Dea.addImage",
				() => new DeaSoapClient { Url=NewUrl("dea") }.addImage(DeaCredentials(),attachmentReference,DeaImage()),
				DeaResponse("addImage",new Dea.ImageReferenceResponses { MsgSuccess=true,Status=DeaStatusOk(),
					ImageReferenceResponses1=new[] { new Dea.ImageReferenceResponse { MsgSuccess=true,Status=DeaStatusOk(),
						ImageReference=new Dea.ImageReference { ImageReferenceId=8080,ImageReferenceIdSpecified=true } } } }));
		}

		[TestMethod]
		public void Dea_SubmitAttachment() {
			Dea.AttachmentReference attachmentReference=new Dea.AttachmentReference { AttachmentID="DXC-TEST-1" };
			AssertEquivalent("Dea.submitAttachment",
				() => new DeaSoapClient { Url=NewUrl("dea") }.submitAttachment(DeaCredentials(),attachmentReference),
				DeaResponse("submitAttachment",new Dea.DeaResponse { MsgSuccess=true,Status=DeaStatusOk(),TransactionId="123456" }));
		}

		[TestMethod]
		public void Dea_AddNarrative() {
			Dea.AttachmentReference attachmentReference=new Dea.AttachmentReference { AttachmentID="DXC-TEST-1" };
			AssertEquivalent("Dea.addNarrative",
				() => new DeaSoapClient { Url=NewUrl("dea") }.addNarrative(DeaCredentials(),attachmentReference,"Synthetic narrative <b> & \"quoted\""),
				DeaResponse("addNarrative",new Dea.AttachmentReferenceResponse { MsgSuccess=true,Status=DeaStatusOk() }));
		}

		[TestMethod]
		public void Dea_AddNarrative_Null() {
			Dea.AttachmentReference attachmentReference=new Dea.AttachmentReference { AttachmentID="DXC-TEST-1" };
			AssertEquivalent("Dea.addNarrative.null",
				() => new DeaSoapClient { Url=NewUrl("dea") }.addNarrative(DeaCredentials(),attachmentReference,null),
				DeaResponse("addNarrative",new Dea.AttachmentReferenceResponse { MsgSuccess=false,Status=new Dea.Status { code=1,description="Operation Failed" } }));
		}

		[TestMethod]
		public void Dea_DeleteImage() {
			Dea.ImageReference imageReference=new Dea.ImageReference { ImageReferenceId=8080,ImageReferenceIdSpecified=true };
			AssertEquivalent("Dea.deleteImage",
				() => new DeaSoapClient { Url=NewUrl("dea") }.deleteImage(DeaCredentials(),imageReference),
				DeaResponse("deleteImage",new Dea.DeaResponse { MsgSuccess=true,Status=DeaStatusOk() }));
		}

		[TestMethod]
		public void Dea_SendCompleteAttachment() {
			Dea.AttachmentImage[] arrayAttachmentImages={ DeaImage(),new Dea.AttachmentImage { ImageTypeCode=Dea.imageTypeCode.P6,ImageTypeCodeSpecified=true,
				ImageFileAsBase64=Encoding.ASCII.GetBytes("%PDF-1.4 synthetic"),ImageFileName="Perio chart.pdf" } };
			AssertEquivalent("Dea.sendCompleteAttachment",
				() => new DeaSoapClient { Url=NewUrl("dea") }.sendCompleteAttachment(DeaCredentials(),DeaAttachment(),arrayAttachmentImages,releaseAttachment:false,
					releaseAttachmentSpecified:false),
				DeaResponse("sendCompleteAttachment",new Dea.AttachmentReferenceResponse { MsgSuccess=true,Status=DeaStatusOk(),
					AttachmentReference=new Dea.AttachmentReference { AttachmentID="DXC-TEST-2" } }));
		}

		[TestMethod]
		public void Dea_SendCompleteAttachment_Released() {
			//The release flag is sent only when its Specified twin is true (the proxy's XmlIgnore'd parameter).
			AssertEquivalent("Dea.sendCompleteAttachment.released",
				() => new DeaSoapClient { Url=NewUrl("dea") }.sendCompleteAttachment(DeaCredentials(),DeaAttachment(),new Dea.AttachmentImage[0],releaseAttachment:true,
					releaseAttachmentSpecified:true),
				DeaResponse("sendCompleteAttachment",new Dea.AttachmentReferenceResponse { MsgSuccess=true,Status=DeaStatusOk() }));
		}
		#endregion DentalXChange DEA

		#region NewCrop
		[TestMethod]
		public void NewCrop_GetAccountStatusV2() {
			AssertEquivalent("NewCrop.GetAccountStatusV2",
				() => new NewCropSoapClient { Url=NewUrl("newcrop") }.GetAccountStatusV2(NcCredentials(),NcAccount(),"0","Staff","42"),
				NcResponse("GetAccountStatusV2",new Nc.AccountStatusDetailV2Result { result=new Nc.Result { Status=Nc.StatusType.OK,Message="Synthetic OK",
					RowCount=1,Timing=12 },accountStatusDetailV2=new Nc.AccountStatusDetailV2 { PendingRxCount="2",UnreadClinicalMessageCount="0" } }));
		}

		[TestMethod]
		public void NewCrop_GetPatientFullMedicationHistory6() {
			Nc.PrescriptionHistoryRequest prescriptionHistoryRequest=new Nc.PrescriptionHistoryRequest { StartHistory=new DateTime(2012,11,2),
				EndHistory=new DateTime(2026,10,1,9,30,0),PrescriptionArchiveStatus="N",PrescriptionStatus="C",PrescriptionSubStatus="S" };
			Nc.PatientInformationRequester patientInformationRequester=new Nc.PatientInformationRequester { UserType="Doctor",UserId="42" };
			string xmlResponseBase64=Convert.ToBase64String(Encoding.UTF8.GetBytes("<NewDataSet><Table><DrugName>Synthetic</DrugName></Table></NewDataSet>"));
			AssertEquivalent("NewCrop.GetPatientFullMedicationHistory6",
				() => new NewCropSoapClient { Url=NewUrl("newcrop") }.GetPatientFullMedicationHistory6(NcCredentials(),NcAccount(),
					new Nc.PatientRequest { PatientId="1001" },prescriptionHistoryRequest,patientInformationRequester,"","N"),
				NcResponse("GetPatientFullMedicationHistory6",new Nc.Result { Status=Nc.StatusType.OK,XmlResponse=xmlResponseBase64,RowCount=1 }));
		}
		#endregion NewCrop

		#region Change Healthcare ITS
		[TestMethod]
		public void Its_PutFileExt() {
			string zipBase64=Convert.ToBase64String(new byte[] { 0x50,0x4B,5,6,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 });
			AssertEquivalent("Its.PutFileExt",
				() => new ItsSoapClient { Url=NewUrl("its") }.PutFileExt("office-login","office-password","MCT","claims7.txt",zipBase64),
				ItsResponse("PutFileExt",new Its.ItsReturn { ErrorCode=0,Response="Synthetic file accepted" }));
		}

		[TestMethod]
		public void Its_GetFile() {
			AssertEquivalent("Its.GetFile",
				() => new ItsSoapClient { Url=NewUrl("its") }.GetFile("office-login","office-password","MCTG"),
				ItsResponse("GetFile",new Its.ItsReturn { ErrorCode=209,Response="Mailbox empty (synthetic)" }));
		}
		#endregion Change Healthcare ITS

		#region DentalXChange DCI (rpc/encoded)
		private static Dci.Credentials DciCredentials() {
			return new Dci.Credentials { client="Synthetic Partner",serviceID="TEST-SERVICE-0001",username="office-login",password="office-password",version="1" };
		}

		private static Dci.Request DciRequest() {
			return new Dci.Request { content="<?xml version=\"1.0\"?><EligRequest><AuthInfo><UserName>office-login</UserName></AuthInfo></EligRequest>" };
		}

		[TestMethod]
		public void Dci_LookupEligibility() {
			AssertEquivalent("Dci.lookupEligibility",
				() => new DciSoapClient { Url=NewUrl("dci") }.lookupEligibility(DciCredentials(),DciRequest()),
				DCI_RESPONSE_MULTIREF);
		}

		[TestMethod]
		public void Dci_LookupEligibility_NullsAndInlineReturn() {
			Dci.Credentials credentials=DciCredentials();
			credentials.password=null;
			credentials.version=null;
			AssertEquivalent("Dci.lookupEligibility.nulls",
				() => new DciSoapClient { Url=NewUrl("dci") }.lookupEligibility(credentials,new Dci.Request()),
				DCI_RESPONSE_INLINE);
		}
		#endregion DentalXChange DCI (rpc/encoded)

		#region Faults
		///<summary>An Axis fault with a detail, as ClaimConnect's comments record DentalXChange's answer to a wrong login.</summary>
		private const string FAULT_WITH_DETAIL="<?xml version=\"1.0\" encoding=\"UTF-8\"?>"
			+"<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\""
			+" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><soapenv:Body><soapenv:Fault>"
			+"<faultcode>soapenv:Server.userException</faultcode><faultstring>Authentication failed.</faultstring><faultactor/>"
			+"<detail><string>Authentication failed.</string><ns1:hostname xmlns:ns1=\"http://xml.apache.org/axis/\">synthetic-host</ns1:hostname></detail>"
			+"</soapenv:Fault></soapenv:Body></soapenv:Envelope>";

		private const string FAULT_WITHOUT_DETAIL="<?xml version=\"1.0\" encoding=\"utf-8\"?>"
			+"<S:Envelope xmlns:S=\"http://schemas.xmlsoap.org/soap/envelope/\"><S:Body><S:Fault xmlns:ns4=\"http://www.w3.org/2003/05/soap-envelope\">"
			+"<faultcode>S:Client</faultcode><faultstring>Malformed document sent.</faultstring></S:Fault></S:Body></S:Envelope>";

		[TestMethod]
		public void Fault_WithDetail_GivesTheSameText() {
			AssertSameFault("Dws.fault.detail",FAULT_WITH_DETAIL,"Authentication failed.","Server.userException","Authentication failed.synthetic-host");
		}

		[TestMethod]
		public void Fault_WithoutDetail_GivesTheSameText() {
			AssertSameFault("Dws.fault.nodetail",FAULT_WITHOUT_DETAIL,"Malformed document sent.","Client",null);
		}

		[TestMethod]
		public void Fault_FromDci_GivesTheSameText() {
			_soapCaptureHandler.ResponseBytes=Encoding.UTF8.GetBytes(FAULT_WITH_DETAIL);
			SoapFaultException soapFaultException=Assert.ThrowsExactly<SoapFaultException>(
				() => new DciSoapClient { Url=NewUrl("dci") }.lookupEligibility(DciCredentials(),DciRequest()));
			Assert.AreEqual("Authentication failed.",soapFaultException.Message);
			Assert.AreEqual("Server.userException",soapFaultException.Code.Name);
			Assert.AreEqual("Authentication failed.synthetic-host",soapFaultException.Detail.InnerText);
		}

		///<summary>The new client's SoapFaultException carries the text, code and detail the old proxy's SoapException carried (the commit before the
		///proxies went compared them directly), so the text ClaimConnect builds from them ("...Claim Connect:  "+Message+"\r\n\r\n"+Detail.InnerText)
		///is the same.</summary>
		private void AssertSameFault(string caseName,string faultXml,string message,string codeName,string detailText) {
			_soapCaptureHandler.ResponseBytes=Encoding.UTF8.GetBytes(faultXml);
			SoapFaultException soapFaultException=Assert.ThrowsExactly<SoapFaultException>(
				() => new DwsSoapClient { Url=NewUrl("dws") }.lookupEligibility(DwsCredentials(),new Dws.textRequest()));
			Assert.AreEqual(message,soapFaultException.Message,caseName);
			Assert.AreEqual(codeName,soapFaultException.Code.Name,caseName);
			Assert.AreEqual(SoapCaller.EnvelopeNamespace,soapFaultException.Code.Namespace,caseName);
			Assert.AreEqual(detailText,soapFaultException.Detail?.InnerText,caseName);
		}
		#endregion Faults

		#region Transport
		[TestMethod]
		public void UserAgent_IsTheOneTheProxiesSent() {
			//The proxies' default on .NET Framework 4.8 (the commit before they went compared it with each proxy's UserAgent).
			Assert.AreEqual("Mozilla/4.0 (compatible; MSIE 6.0; MS Web Services Client Protocol 4.0.30319.42000)",SoapCaller.UserAgentDefault);
			Assert.AreEqual(SoapCaller.UserAgentDefault,new DwsSoapClient().UserAgent);
			Assert.AreEqual(SoapCaller.UserAgentDefault,new DeaSoapClient().UserAgent);
			Assert.AreEqual(SoapCaller.UserAgentDefault,new ItsSoapClient().UserAgent);
			Assert.AreEqual(SoapCaller.UserAgentDefault,new DciSoapClient().UserAgent);
			Assert.AreEqual(SoapCaller.UserAgentDefault,new NewCropSoapClient().UserAgent);
		}

		[TestMethod]
		public void DefaultAddresses_AreTheProxiesDefaults() {
			//The proxies' defaults, from the URL settings they read (kept). The proxies gave their Url back through Uri.ToString, which drops a
			//default port, so the addresses are compared as Uri.
			Assert.AreEqual(new Uri("https://webservices.dentalxchange.com/dws/DwsService"),new Uri(new DwsSoapClient().Url));
			Assert.AreEqual(new Uri("https://prelive2.dentalxchange.com/dea/DeaPartnerService"),new Uri(new DeaSoapClient().Url));
			Assert.AreEqual(new Uri("https://its.changehealthcare.com/ITS/ITSWS.asmx"),new Uri(new ItsSoapClient().Url));
			Assert.AreEqual(new Uri("https://webservices.dentalxchange.com/dws/services/dciservice.svl"),new Uri(new DciSoapClient().Url));
			Assert.AreEqual(new Uri("https://secure.newcropaccounts.com/v7/WebServices/Update1.asmx"),new Uri(new NewCropSoapClient().Url));
			Assert.AreEqual(100000,new DwsSoapClient().Timeout);
		}

		[TestMethod]
		public void NotXml_IsAnError() {
			_soapCaptureHandler.ResponseBytes=Encoding.UTF8.GetBytes("<html><body>Service unavailable</body></html>");
			_soapCaptureHandler.ContentType="text/html";
			InvalidOperationException ex=Assert.ThrowsExactly<InvalidOperationException>(
				() => new ItsSoapClient { Url=NewUrl("its") }.GetFile("office-login","office-password","MCTG"));
			StringAssert.StartsWith(ex.Message,"Client found response content type of 'text/html', but expected 'text/xml'.");
		}

		[TestMethod]
		public void HttpError_IsAWebException() {
			_soapCaptureHandler.ResponseBytes=Encoding.UTF8.GetBytes("Not here");
			_soapCaptureHandler.StatusCode=HttpStatusCode.NotFound;
			_soapCaptureHandler.ContentType="text/plain";
			WebException ex=Assert.ThrowsExactly<WebException>(() => new ItsSoapClient { Url=NewUrl("its") }.GetFile("office-login","office-password","MCTG"));
			Assert.AreEqual(WebExceptionStatus.ProtocolError,ex.Status);
			StringAssert.Contains(ex.Message,"404");
		}

		[TestMethod]
		public void Timeout_IsAWebException() {
			_soapCaptureHandler.Delay=TimeSpan.FromSeconds(5);
			_soapCaptureHandler.ResponseBytes=Encoding.UTF8.GetBytes(ItsResponse("GetFile",new Its.ItsReturn()));
			WebException ex=Assert.ThrowsExactly<WebException>(
				() => new ItsSoapClient { Url=NewUrl("its"),Timeout=200 }.GetFile("office-login","office-password","MCTG"));
			Assert.AreEqual(WebExceptionStatus.Timeout,ex.Status);
		}
		#endregion Transport

		#region Comparison
		///<summary>The clients' address in these tests: never a real host, and only the test handler answers it.</summary>
		private static string NewUrl(string service) {
			return "https://soap-new.example.test/"+service;
		}

		///<summary>Calls the new client with the canned response and compares what it sent and read with what the old proxy sent and read, as
		///recorded.</summary>
		private void AssertEquivalent(string caseName,Func<object> funcNew,string responseXml) {
			_soapCaptureHandler.ResponseBytes=Encoding.UTF8.GetBytes(responseXml);
			object resultNew=funcNew();
			CapturedRequest capturedRequestNew=_soapCaptureHandler.CapturedRequestLast;
			Assert.IsNotNull(capturedRequestNew,caseName);
			Assert.IsNotNull(resultNew,caseName+": the new client read nothing.");
			Assert.AreEqual("POST",capturedRequestNew.Method,caseName);
			Assert.AreEqual(SoapCaller.UserAgentDefault,capturedRequestNew.UserAgent,caseName);
			string headers="SOAPAction: "+capturedRequestNew.SoapAction+"\r\nContent-Type: "+capturedRequestNew.ContentType+"\r\n";
			AssertMatchesFixtures(caseName,capturedRequestNew,headers,SerializeResult(resultNew));
		}

		///<summary>The new client's request and result against what the old proxy sent and read, as recorded.</summary>
		private static void AssertMatchesFixtures(string caseName,CapturedRequest capturedRequestNew,string headersNew,string xmlResultNew) {
			string pathRequest=Path.Combine("tests","VDXC2Tests",FIXTURE_FOLDER,caseName+".request.xml");
			byte[] arrayBytesRecorded=File.ReadAllBytes(TestDb.FindRepoFile(pathRequest));
			Assert.AreEqual(Canonical(arrayBytesRecorded),Canonical(capturedRequestNew.Body),caseName+" (recorded)");
			Assert.AreEqual(NormalizeLineEnds(File.ReadAllText(TestDb.FindRepoFile(Path.ChangeExtension(Path.ChangeExtension(pathRequest,null),".headers.txt")))),
				NormalizeLineEnds(headersNew),caseName+" (recorded)");
			//Canonically: the serializer of another runtime may declare xsi and xsd in another order.
			Assert.AreEqual(Canonical(File.ReadAllBytes(TestDb.FindRepoFile(Path.ChangeExtension(Path.ChangeExtension(pathRequest,null),".result.xml")))),
				Canonical(Encoding.UTF8.GetBytes(xmlResultNew)),caseName+" (recorded)");
		}

		private static string NormalizeLineEnds(string text) {
			return text.Replace("\r\n","\n");
		}

		///<summary>A parsed result as XML, with its runtime type's own mapping.</summary>
		private static string SerializeResult(object result) {
			XmlSerializer xmlSerializer=new XmlSerializer(result.GetType());
			StringBuilder stringBuilder=new StringBuilder();
			using(XmlWriter xmlWriter=XmlWriter.Create(stringBuilder,new XmlWriterSettings { OmitXmlDeclaration=true,Indent=true,NewLineChars="\n" })) {
				xmlSerializer.Serialize(xmlWriter,result);
			}
			return stringBuilder.ToString();
		}

		///<summary>A canonical text of an XML document: each element as {namespace}name, its attributes sorted, without namespace declarations,
		///xsi:type values resolved to {namespace}name, and its text. Prefixes and where namespaces are declared do not matter.</summary>
		public static string Canonical(byte[] arrayBytesXml) {
			XmlDocument xmlDocument=new XmlDocument();
			xmlDocument.PreserveWhitespace=true;
			using(MemoryStream memoryStream=new MemoryStream(arrayBytesXml)) {
				xmlDocument.Load(memoryStream);
			}
			StringBuilder stringBuilder=new StringBuilder();
			AppendCanonical(xmlDocument.DocumentElement,stringBuilder);
			return stringBuilder.ToString();
		}

		private static void AppendCanonical(XmlElement xmlElement,StringBuilder stringBuilder) {
			stringBuilder.Append("<{"+xmlElement.NamespaceURI+"}"+xmlElement.LocalName);
			List<string> listAttributes=new List<string>();
			foreach(XmlAttribute xmlAttribute in xmlElement.Attributes) {
				if(xmlAttribute.NamespaceURI=="http://www.w3.org/2000/xmlns/") {
					continue;
				}
				string value=xmlAttribute.Value;
				if(xmlAttribute.NamespaceURI==SoapCaller.XsiNamespace && xmlAttribute.LocalName=="type") {
					int indexColon=value.IndexOf(':');
					string prefix=indexColon<0 ? "" : value.Substring(0,indexColon);
					value="{"+xmlElement.GetNamespaceOfPrefix(prefix)+"}"+value.Substring(indexColon+1);
				}
				listAttributes.Add(" {"+xmlAttribute.NamespaceURI+"}"+xmlAttribute.LocalName+"=\""+value+"\"");
			}
			listAttributes.Sort(StringComparer.Ordinal);
			stringBuilder.Append(string.Concat(listAttributes)+">");
			foreach(XmlNode xmlNode in xmlElement.ChildNodes) {
				if(xmlNode is XmlElement xmlElementChild) {
					AppendCanonical(xmlElementChild,stringBuilder);
				}
				else if(xmlNode is XmlCharacterData) {
					stringBuilder.Append(System.Security.SecurityElement.Escape(xmlNode.Value));
				}
			}
			stringBuilder.Append("</>");
		}
		#endregion Comparison
	}

	///<summary>One HTTP request as a SOAP client sent it.</summary>
	public class CapturedRequest {
		public Uri Uri;
		public string Method;
		public string ContentType;
		///<summary>The SOAPAction header as sent, quotes included.</summary>
		public string SoapAction;
		public string UserAgent;
		public byte[] Body;
	}

	///<summary>Answers the new clients (SoapCaller.HttpHandlerForTests) with ResponseBytes, and keeps the request. Nothing goes to the network.
	///</summary>
	public class SoapCaptureHandler:HttpMessageHandler {
		public byte[] ResponseBytes;
		public string ContentType="text/xml; charset=utf-8";
		public HttpStatusCode StatusCode=HttpStatusCode.OK;
		public TimeSpan Delay=TimeSpan.Zero;
		public CapturedRequest CapturedRequestLast;
		public List<CapturedRequest> ListCapturedRequests=new List<CapturedRequest>();

		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequestMessage,CancellationToken cancellationToken) {
			CapturedRequest capturedRequest=new CapturedRequest();
			capturedRequest.Uri=httpRequestMessage.RequestUri;
			capturedRequest.Method=httpRequestMessage.Method.Method;
			capturedRequest.ContentType=httpRequestMessage.Content?.Headers.ContentType?.ToString();
			capturedRequest.SoapAction=httpRequestMessage.Headers.TryGetValues("SOAPAction",out IEnumerable<string> values) ? values.First() : null;
			capturedRequest.UserAgent=httpRequestMessage.Headers.TryGetValues("User-Agent",out IEnumerable<string> valuesUserAgent)
				? string.Join(" ",valuesUserAgent) : null;
			capturedRequest.Body=httpRequestMessage.Content==null ? new byte[0] : await httpRequestMessage.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
			CapturedRequestLast=capturedRequest;
			ListCapturedRequests.Add(capturedRequest);
			if(Delay>TimeSpan.Zero) {
				await System.Threading.Tasks.Task.Delay(Delay,cancellationToken).ConfigureAwait(false);
			}
			if(ResponseBytes==null) {
				throw new HttpRequestException("Test: no response set for the SOAP client.");
			}
			HttpResponseMessage httpResponseMessage=new HttpResponseMessage(StatusCode);
			httpResponseMessage.Content=new ByteArrayContent(ResponseBytes);
			httpResponseMessage.Content.Headers.TryAddWithoutValidation("Content-Type",ContentType);
			return httpResponseMessage;
		}
	}
}
