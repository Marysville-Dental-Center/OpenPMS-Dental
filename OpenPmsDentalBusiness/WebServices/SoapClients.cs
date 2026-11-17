// The vendor SOAP clients that replace the generated SoapHttpClientProtocol proxies (docs/specs/net10-plan.md, S3a-6;
// docs/port-notes/S3a-6.md). Each method is one operation the program uses, declared with the attributes its generated proxy method had, and
// sent through SoapCaller. The data classes stay in the generated Reference.cs files under "Web References". The default addresses are the
// generated proxies' (Properties.Settings); callers set Url as before.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Dci=OpenPmsDentalBusiness.com.dentalxchange.webservices;
using Dea=OpenPmsDentalBusiness.DentalxchangePartnerService;
using Dws=OpenPmsDentalBusiness.Dentalxchange2016;
using Its=OpenPmsDentalBusiness.EmdeonITS;
using Nc=OpenPmsDentalBusiness.NewCropWebService;

namespace OpenPmsDentalBusiness {
	///<summary>DentalXChange's DWS service (ClaimConnect eligibility, claim validation, ERAs and the payer list). Was Dentalxchange2016.DwsService.
	///</summary>
	[SoapBinding("dxci.dws")]
	[XmlInclude(typeof(Dws.Response))]
	[XmlInclude(typeof(Dws.dwsRequest))]
	public class DwsSoapClient:SoapClient {
		public const string Namespace="dxci.dws";

		public DwsSoapClient() {
			Url=Properties.Settings.Default.OpenDentBusiness_Dentalxchange2016_DwsService;
		}

		[SoapOperation("",RequestNamespace=Namespace,ResponseNamespace=Namespace)]
		[return: XmlElement("return",Form=XmlSchemaForm.Unqualified)]
		public Dws.textResponse validateClaim([XmlElement(Form=XmlSchemaForm.Unqualified)] Dws.Credentials credentials,
			[XmlElement(Form=XmlSchemaForm.Unqualified)] Dws.textRequest request)
		{
			return (Dws.textResponse)Invoke(nameof(validateClaim),credentials,request)[0];
		}

		[SoapOperation("",RequestNamespace=Namespace,ResponseNamespace=Namespace)]
		[return: XmlElement("return",Form=XmlSchemaForm.Unqualified)]
		public Dws.textResponse lookupEligibility([XmlElement(Form=XmlSchemaForm.Unqualified)] Dws.Credentials credentials,
			[XmlElement(Form=XmlSchemaForm.Unqualified)] Dws.textRequest request)
		{
			return (Dws.textResponse)Invoke(nameof(lookupEligibility),credentials,request)[0];
		}

		[SoapOperation("",RequestNamespace=Namespace,ResponseNamespace=Namespace)]
		[return: XmlElement("return",Form=XmlSchemaForm.Unqualified)]
		public Dws.UnProcessedEraResponse getUnProcessedEra([XmlElement(Form=XmlSchemaForm.Unqualified)] Dws.Credentials credentials,
			[XmlElement(Form=XmlSchemaForm.Unqualified)] Dws.unProcessedEraRequest request)
		{
			return (Dws.UnProcessedEraResponse)Invoke(nameof(getUnProcessedEra),credentials,request)[0];
		}

		[SoapOperation("",RequestNamespace=Namespace,ResponseNamespace=Namespace)]
		[return: XmlElement("return",Form=XmlSchemaForm.Unqualified)]
		public Dws.PayerListInfoResponse getPayerListService([XmlElement(Form=XmlSchemaForm.Unqualified)] Dws.Credentials credentials,
			[XmlElement(Form=XmlSchemaForm.Unqualified)] Dws.payerListInfoRequest request)
		{
			return (Dws.PayerListInfoResponse)Invoke(nameof(getPayerListService),credentials,request)[0];
		}
	}

	///<summary>DentalXChange's partner attachment service (DEA). Was DentalxchangePartnerService.DeaPartnerService.</summary>
	[SoapBinding("dxci.dea")]
	[XmlInclude(typeof(Dea.Response))]
	[XmlInclude(typeof(Dea.ImageReferenceResponse[]))]
	public class DeaSoapClient:SoapClient {
		public const string Namespace="dxci.dea";

		public DeaSoapClient() {
			Url=Properties.Settings.Default.OpenDentBusiness_DentalxchangePartnerService_DeaPartnerService;
		}

		[SoapOperation("",RequestNamespace=Namespace,ResponseNamespace=Namespace)]
		[return: XmlElement("return",Form=XmlSchemaForm.Unqualified)]
		public Dea.ImageReferenceResponses addImage([XmlElement(Form=XmlSchemaForm.Unqualified)] Dea.Credentials Credentials,
			[XmlElement(Form=XmlSchemaForm.Unqualified)] Dea.AttachmentReference AttachmentReference,
			[XmlElement(Form=XmlSchemaForm.Unqualified)] Dea.AttachmentImage attachmentImage)
		{
			return (Dea.ImageReferenceResponses)Invoke(nameof(addImage),Credentials,AttachmentReference,attachmentImage)[0];
		}

		[SoapOperation("",RequestNamespace=Namespace,ResponseNamespace=Namespace)]
		[return: XmlElement("return",Form=XmlSchemaForm.Unqualified)]
		public Dea.DeaResponse submitAttachment([XmlElement(Form=XmlSchemaForm.Unqualified)] Dea.Credentials Credentials,
			[XmlElement(Form=XmlSchemaForm.Unqualified)] Dea.AttachmentReference AttachmentReference)
		{
			return (Dea.DeaResponse)Invoke(nameof(submitAttachment),Credentials,AttachmentReference)[0];
		}

		[SoapOperation("",RequestNamespace=Namespace,ResponseNamespace=Namespace)]
		[return: XmlElement("return",Form=XmlSchemaForm.Unqualified)]
		public Dea.AttachmentReferenceResponse openAttachment([XmlElement(Form=XmlSchemaForm.Unqualified)] Dea.Credentials Credentials,
			[XmlElement(Form=XmlSchemaForm.Unqualified)] Dea.Attachment attachment)
		{
			return (Dea.AttachmentReferenceResponse)Invoke(nameof(openAttachment),Credentials,attachment)[0];
		}

		[SoapOperation("",RequestNamespace=Namespace,ResponseNamespace=Namespace)]
		[return: XmlElement("return",Form=XmlSchemaForm.Unqualified)]
		public Dea.AttachmentReferenceResponse addNarrative([XmlElement(Form=XmlSchemaForm.Unqualified)] Dea.Credentials Credentials,
			[XmlElement(Form=XmlSchemaForm.Unqualified)] Dea.AttachmentReference attachmentReference,
			[XmlElement(Form=XmlSchemaForm.Unqualified)] string narrative)
		{
			return (Dea.AttachmentReferenceResponse)Invoke(nameof(addNarrative),Credentials,attachmentReference,narrative)[0];
		}

		[SoapOperation("",RequestNamespace=Namespace,ResponseNamespace=Namespace)]
		[return: XmlElement("return",Form=XmlSchemaForm.Unqualified)]
		public Dea.DeaResponse deleteImage([XmlElement(Form=XmlSchemaForm.Unqualified)] Dea.Credentials credentials,
			[XmlElement(Form=XmlSchemaForm.Unqualified)] Dea.ImageReference imageReference)
		{
			return (Dea.DeaResponse)Invoke(nameof(deleteImage),credentials,imageReference)[0];
		}

		[SoapOperation("",RequestNamespace=Namespace,ResponseNamespace=Namespace)]
		[return: XmlElement("return",Form=XmlSchemaForm.Unqualified)]
		public Dea.AttachmentReferenceResponse sendCompleteAttachment([XmlElement(Form=XmlSchemaForm.Unqualified)] Dea.Credentials Credentials,
			[XmlElement(Form=XmlSchemaForm.Unqualified)] Dea.Attachment attachment,
			[XmlArray(Form=XmlSchemaForm.Unqualified)] [XmlArrayItem("attachmentImage",Form=XmlSchemaForm.Unqualified,IsNullable=false)] Dea.AttachmentImage[] attachmentImages,
			[XmlElement(Form=XmlSchemaForm.Unqualified)] bool releaseAttachment,
			[XmlElement(Form=XmlSchemaForm.Unqualified)] [XmlIgnore] bool releaseAttachmentSpecified)
		{
			return (Dea.AttachmentReferenceResponse)Invoke(nameof(sendCompleteAttachment),Credentials,attachment,attachmentImages,releaseAttachment,
				releaseAttachmentSpecified)[0];
		}
	}

	///<summary>Change Healthcare's (Emdeon's) ITS service for medical and institutional claims and their reports. Was EmdeonITS.ITSWS.</summary>
	[SoapBinding(ItsSoapClient.Namespace)]
	public class ItsSoapClient:SoapClient {
		public const string Namespace="https://ITSWebService.changehealthcare.com/";

		public ItsSoapClient() {
			Url=Properties.Settings.Default.OpenDentBusiness_EmdeonITS_ITSWS;
		}

		///<summary>Sends a batch file: sEncodedPutFile is the base64 of a zip archive holding the file sFileName.</summary>
		[SoapOperation(Namespace+"PutFileExt",RequestNamespace=Namespace,ResponseNamespace=Namespace)]
		public Its.ItsReturn PutFileExt(string sUserID,string sPassword,string sMessageType,string sFileName,string sEncodedPutFile) {
			return (Its.ItsReturn)Invoke(nameof(PutFileExt),sUserID,sPassword,sMessageType,sFileName,sEncodedPutFile)[0];
		}

		///<summary>Downloads (or, with a delete message type, removes) the reports of one message type.</summary>
		[SoapOperation(Namespace+"GetFile",RequestNamespace=Namespace,ResponseNamespace=Namespace)]
		public Its.ItsReturn GetFile(string sUserID,string sPassword,string sMessageType) {
			return (Its.ItsReturn)Invoke(nameof(GetFile),sUserID,sPassword,sMessageType)[0];
		}
	}

	///<summary>NewCrop's (Ensora's) Update1 service. Was NewCropWebService.Update1. NewCropConfig.CreateUpdate1Client makes one for the configured
	///environment. The namespace is NewCropConfig's, where NewCrop's addresses live.</summary>
	[SoapBinding(NewCropConfig.SoapNamespace)]
	public class NewCropSoapClient:SoapClient {
		public NewCropSoapClient() {
			Url=Properties.Settings.Default.OpenDentBusiness_NewCrop_Update1;
		}

		[SoapOperation(NewCropConfig.SoapNamespace+"/GetAccountStatusV2",RequestNamespace=NewCropConfig.SoapNamespace,
			ResponseNamespace=NewCropConfig.SoapNamespace)]
		public Nc.AccountStatusDetailV2Result GetAccountStatusV2(Nc.Credentials credentials,Nc.AccountRequest accountRequest,string locationId,
			string userType,string userId)
		{
			return (Nc.AccountStatusDetailV2Result)Invoke(nameof(GetAccountStatusV2),credentials,accountRequest,locationId,userType,userId)[0];
		}

		[SoapOperation(NewCropConfig.SoapNamespace+"/GetPatientFullMedicationHistory6",RequestNamespace=NewCropConfig.SoapNamespace,
			ResponseNamespace=NewCropConfig.SoapNamespace)]
		public Nc.Result GetPatientFullMedicationHistory6(Nc.Credentials credentials,Nc.AccountRequest accountRequest,Nc.PatientRequest patientRequest,
			Nc.PrescriptionHistoryRequest prescriptionHistoryRequest,Nc.PatientInformationRequester patientInformationRequester,string patientIdType,
			string includeSchema)
		{
			return (Nc.Result)Invoke(nameof(GetPatientFullMedicationHistory6),credentials,accountRequest,patientRequest,prescriptionHistoryRequest,
				patientInformationRequester,patientIdType,includeSchema)[0];
		}
	}

	///<summary>DentalXChange's older eligibility service (DCI), used by Edit Insurance Plan's eligibility button when CustomizedForPracticeWeb is
	///on. Was com.dentalxchange.webservices.WebServiceService. Its one used operation is rpc/encoded (SoapRpcMethod), which XmlSerializer cannot be
	///relied on to write on .NET, so the body is written and read by hand, as the old proxy's SoapReflectionImporter mapping wrote it: the
	///lookupEligibility element in the service namespace, its parts in0 and in1 as references to the Credentials and Request elements that follow
	///it, every field typed xsd:string (xsi:nil when null).</summary>
	public class DciSoapClient:SoapClient {
		public const string Namespace="http://www.dentalxchange.com/webservice";

		public DciSoapClient() {
			Url=Properties.Settings.Default.OpenDentBusiness_com_dentalxchange_webservices_WebServiceService;
		}

		///<summary>Asks for eligibility. in1.content is the EligRequest XML; the answer's content is the EligBenefitResponse XML.</summary>
		public Dci.Response lookupEligibility(Dci.Credentials in0,Dci.Request in1) {
			return SoapCaller.Send(this,"",BuildLookupEligibility(in0,in1),ReadLookupEligibilityResponse);
		}

		///<summary>The request lookupEligibility sends.</summary>
		public static byte[] BuildLookupEligibility(Dci.Credentials in0,Dci.Request in1) {
			return SoapCaller.WriteEnvelope(isEncoded:true,Namespace,xmlWriter => {
				xmlWriter.WriteStartElement("lookupEligibility",Namespace);
				WriteReference(xmlWriter,"in0",in0==null ? null : "id1");
				WriteReference(xmlWriter,"in1",in1==null ? null : "id2");
				xmlWriter.WriteEndElement();
				if(in0!=null) {
					WriteStartMultiRef(xmlWriter,"Credentials","id1");
					WriteString(xmlWriter,"client",in0.client);
					WriteString(xmlWriter,"password",in0.password);
					WriteString(xmlWriter,"serviceID",in0.serviceID);
					WriteString(xmlWriter,"username",in0.username);
					WriteString(xmlWriter,"version",in0.version);
					xmlWriter.WriteEndElement();
				}
				if(in1!=null) {
					WriteStartMultiRef(xmlWriter,"Request","id2");
					WriteString(xmlWriter,"content",in1.content);
					xmlWriter.WriteEndElement();
				}
			});
		}

		///<summary>A part: a reference to the element with the id, or xsi:nil when the argument is null.</summary>
		private static void WriteReference(XmlWriter xmlWriter,string name,string id) {
			xmlWriter.WriteStartElement(name);
			if(id==null) {
				xmlWriter.WriteAttributeString("nil",SoapCaller.XsiNamespace,"true");
			}
			else {
				xmlWriter.WriteAttributeString("href","#"+id);
			}
			xmlWriter.WriteEndElement();
		}

		///<summary>The element an href points to: named after its type in the service namespace, with its id and xsi:type.</summary>
		private static void WriteStartMultiRef(XmlWriter xmlWriter,string typeName,string id) {
			xmlWriter.WriteStartElement(typeName,Namespace);
			xmlWriter.WriteAttributeString("id",id);
			xmlWriter.WriteStartAttribute("type",SoapCaller.XsiNamespace);
			xmlWriter.WriteQualifiedName(typeName,Namespace);
			xmlWriter.WriteEndAttribute();
		}

		///<summary>An unqualified string field typed xsd:string, or xsi:nil when null (SoapElement IsNullable=true).</summary>
		private static void WriteString(XmlWriter xmlWriter,string name,string value) {
			xmlWriter.WriteStartElement(name);
			if(value==null) {
				xmlWriter.WriteAttributeString("nil",SoapCaller.XsiNamespace,"true");
			}
			else {
				xmlWriter.WriteStartAttribute("type",SoapCaller.XsiNamespace);
				xmlWriter.WriteQualifiedName("string",SoapCaller.XsdNamespace);
				xmlWriter.WriteEndAttribute();
				xmlWriter.WriteString(value);
			}
			xmlWriter.WriteEndElement();
		}

		///<summary>Reads the response body: lookupEligibilityResponse holding lookupEligibilityReturn, either inline or as an href to a multiRef
		///element after it, whose content field is the answer. Returns null when the return is missing or nil, as the old proxy did.</summary>
		public static Dci.Response ReadLookupEligibilityResponse(XmlReader xmlReader) {
			XmlDocument xmlDocument=new XmlDocument();
			XmlElement xmlElementBody=xmlDocument.CreateElement("Body",SoapCaller.EnvelopeNamespace);
			xmlDocument.AppendChild(xmlElementBody);
			while(xmlReader.NodeType==XmlNodeType.Element) {
				xmlElementBody.AppendChild(xmlDocument.ReadNode(xmlReader));
				xmlReader.MoveToContent();
			}
			XmlElement xmlElementResponse=xmlElementBody.ChildNodes.OfType<XmlElement>().FirstOrDefault();
			if(xmlElementResponse==null) {
				return null;
			}
			XmlElement xmlElementReturn=Resolve(xmlElementBody,
				xmlElementResponse.ChildNodes.OfType<XmlElement>().FirstOrDefault(x => x.LocalName=="lookupEligibilityReturn"));
			if(xmlElementReturn==null || IsNil(xmlElementReturn)) {
				return null;
			}
			Dci.Response response=new Dci.Response();
			XmlElement xmlElementContent=Resolve(xmlElementBody,xmlElementReturn.ChildNodes.OfType<XmlElement>().FirstOrDefault(x => x.LocalName=="content"));
			if(xmlElementContent!=null && !IsNil(xmlElementContent)) {
				response.content=xmlElementContent.InnerText;
			}
			return response;
		}

		///<summary>The element itself, or for an href="#id" the body's element with that id. Null when the reference points nowhere.</summary>
		private static XmlElement Resolve(XmlElement xmlElementBody,XmlElement xmlElement) {
			if(xmlElement==null || !xmlElement.HasAttribute("href")) {
				return xmlElement;
			}
			string href=xmlElement.GetAttribute("href");
			if(!href.StartsWith("#")) {
				return null;
			}
			string id=href.Substring(1);
			return xmlElementBody.ChildNodes.OfType<XmlElement>().FirstOrDefault(x => x.GetAttribute("id")==id);
		}

		private static bool IsNil(XmlElement xmlElement) {
			string nil=xmlElement.GetAttribute("nil",SoapCaller.XsiNamespace);
			return nil=="true" || nil=="1";
		}
	}
}
