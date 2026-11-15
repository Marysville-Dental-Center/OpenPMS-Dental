// S3a-6 (docs/specs/net10-plan.md, decision 12 and open point O-6; docs/port-notes/S3a-6.md): the Practice-Web DentalXChange eligibility path
// (Edit Insurance Plan with CustomizedForPracticeWeb on) through DciSoapClient, which writes DCI's rpc/encoded lookupEligibility by hand. The
// request the client builds (element, namespace, parts and their types) is checked as the old proxy's SoapReflectionImporter mapping wrote it,
// and a recorded, SYNTHETIC answer (DciLookupEligibilityResponse.xml) is read into the content that FormInsPlan's
// ProcessEligibilityResponseDentalXchange reads. No call leaves the process: SoapCaller's test handler answers.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Linq;
using System.Net;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalBusiness;
using VendorDirectTests.Foundation;
using Dci=OpenPmsDentalBusiness.com.dentalxchange.webservices;

namespace VendorDirectTests.DentalXChange {
	[TestClass]
	public class DciEligibilityTests {
		private const string SOAP_ENC="http://schemas.xmlsoap.org/soap/encoding/";
		private const string XSI="http://www.w3.org/2001/XMLSchema-instance";
		private const string XSD="http://www.w3.org/2001/XMLSchema";
		private FakeHttpHandler _fakeHttpHandler;

		[TestInitialize]
		public void TestInitialize() {
			_fakeHttpHandler=new FakeHttpHandler();
			SoapCaller.HttpHandlerForTests=_fakeHttpHandler;
		}

		[TestCleanup]
		public void TestCleanup() {
			SoapCaller.HttpHandlerForTests=null;
		}

		private static string ReadRecordedResponse() {
			return DxcTestFiles.ReadRepoFile(@"tests\VendorDirectTests\DentalXChange\DciLookupEligibilityResponse.xml");
		}

		///<summary>The credentials FormInsPlan.EligibilityCheckDentalXchange fills, with synthetic values.</summary>
		private static Dci.Credentials Credentials() {
			return new Dci.Credentials { serviceID="TEST-SERVICE-0001",username="office-login",password="office-password",client="Synthetic Partner",version="1" };
		}

		private static Dci.Request Request() {
			return new Dci.Request { content="<EligRequest xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><AuthInfo><UserName>office-login</UserName>"
				+"<Password>office-password</Password></AuthInfo><Patient><LastName>SYNTHETIC</LastName></Patient></EligRequest>" };
		}

		[TestMethod]
		public void Request_IsTheRpcEncodedLookupEligibility() {
			_fakeHttpHandler.Enqueue(HttpStatusCode.OK,ReadRecordedResponse(),"text/xml");
			DciSoapClient dciSoapClient=new DciSoapClient();
			Assert.AreEqual(new Uri("https://webservices.dentalxchange.com/dws/services/dciservice.svl"),new Uri(dciSoapClient.Url),
				"The address of the kept URL setting OpenDentBusiness_com_dentalxchange_webservices_WebServiceService.");
			dciSoapClient.lookupEligibility(Credentials(),Request());
			RecordedRequest recordedRequest=_fakeHttpHandler.Requests.Single();
			Assert.AreEqual(new Uri("https://webservices.dentalxchange.com/dws/services/dciservice.svl"),recordedRequest.RequestUri);
			Assert.AreEqual("POST",recordedRequest.Method.Method);
			Assert.AreEqual("\"\"",recordedRequest.GetHeader("SOAPAction"),"rpc/encoded with an empty SOAPAction, quoted.");
			Assert.AreEqual("text/xml",recordedRequest.ContentType);
			XmlDocument xmlDocument=new XmlDocument();
			xmlDocument.LoadXml(recordedRequest.Body);
			XmlElement xmlElementEnvelope=xmlDocument.DocumentElement;
			Assert.AreEqual(SoapCaller.EnvelopeNamespace,xmlElementEnvelope.NamespaceURI);
			XmlElement xmlElementBody=xmlElementEnvelope.ChildNodes.OfType<XmlElement>().Single();
			Assert.AreEqual("Body",xmlElementBody.LocalName);
			Assert.AreEqual(SOAP_ENC,xmlElementBody.GetAttribute("encodingStyle",SoapCaller.EnvelopeNamespace),"SOAP encoding on the body.");
			XmlElement[] arrayElements=xmlElementBody.ChildNodes.OfType<XmlElement>().ToArray();
			Assert.AreEqual(3,arrayElements.Length,"The call, then the two parts it refers to.");
			//The call: lookupEligibility in the service namespace, its parts in0 and in1 unqualified, as references.
			XmlElement xmlElementCall=arrayElements[0];
			Assert.AreEqual("lookupEligibility",xmlElementCall.LocalName);
			Assert.AreEqual(DciSoapClient.Namespace,xmlElementCall.NamespaceURI);
			XmlElement[] arrayParts=xmlElementCall.ChildNodes.OfType<XmlElement>().ToArray();
			CollectionAssert.AreEqual(new[] { "in0","in1" },arrayParts.Select(x => x.LocalName).ToArray());
			Assert.IsTrue(arrayParts.All(x => x.NamespaceURI==""));
			//in0: Credentials, typed tns:Credentials, every field an xsd:string in the order of the old proxy's data class.
			XmlElement xmlElementCredentials=FindById(xmlElementBody,arrayParts[0].GetAttribute("href"));
			Assert.AreEqual("Credentials",xmlElementCredentials.LocalName);
			Assert.AreEqual(DciSoapClient.Namespace,xmlElementCredentials.NamespaceURI);
			Assert.AreEqual(DciSoapClient.Namespace+"#Credentials",XsiType(xmlElementCredentials));
			XmlElement[] arrayFields=xmlElementCredentials.ChildNodes.OfType<XmlElement>().ToArray();
			CollectionAssert.AreEqual(new[] { "client","password","serviceID","username","version" },arrayFields.Select(x => x.LocalName).ToArray());
			Assert.IsTrue(arrayFields.All(x => x.NamespaceURI=="" && XsiType(x)==XSD+"#string"));
			CollectionAssert.AreEqual(new[] { "Synthetic Partner","office-password","TEST-SERVICE-0001","office-login","1" },
				arrayFields.Select(x => x.InnerText).ToArray());
			//in1: Request, typed tns:Request, its content the EligRequest XML as text.
			XmlElement xmlElementRequest=FindById(xmlElementBody,arrayParts[1].GetAttribute("href"));
			Assert.AreEqual("Request",xmlElementRequest.LocalName);
			Assert.AreEqual(DciSoapClient.Namespace+"#Request",XsiType(xmlElementRequest));
			XmlElement xmlElementContent=xmlElementRequest.ChildNodes.OfType<XmlElement>().Single();
			Assert.AreEqual("content",xmlElementContent.LocalName);
			Assert.AreEqual(XSD+"#string",XsiType(xmlElementContent));
			Assert.AreEqual(Request().content,xmlElementContent.InnerText);
		}

		[TestMethod]
		public void Request_NullFieldsAreNil() {
			_fakeHttpHandler.Enqueue(HttpStatusCode.OK,ReadRecordedResponse(),"text/xml");
			Dci.Credentials credentials=Credentials();
			credentials.password=null;
			new DciSoapClient().lookupEligibility(credentials,new Dci.Request());
			XmlDocument xmlDocument=new XmlDocument();
			xmlDocument.LoadXml(_fakeHttpHandler.Requests.Single().Body);
			XmlElement xmlElementPassword=(XmlElement)xmlDocument.GetElementsByTagName("password")[0];
			Assert.AreEqual("true",xmlElementPassword.GetAttribute("nil",XSI));
			Assert.AreEqual("",xmlElementPassword.InnerText);
			XmlElement xmlElementContent=(XmlElement)xmlDocument.GetElementsByTagName("content")[0];
			Assert.AreEqual("true",xmlElementContent.GetAttribute("nil",XSI));
		}

		[TestMethod]
		public void RecordedResponse_GivesTheEligBenefitResponse() {
			_fakeHttpHandler.Enqueue(HttpStatusCode.OK,ReadRecordedResponse(),"text/xml");//StringContent adds charset=utf-8.
			Dci.Response response=new DciSoapClient().lookupEligibility(Credentials(),Request());
			Assert.IsNotNull(response);
			//What FormInsPlan.ProcessEligibilityResponseDentalXchange reads: the content as XML, and EligBenefitResponse/isEligible.
			XmlDocument xmlDocument=new XmlDocument();
			xmlDocument.LoadXml(response.content.ToString());
			Assert.AreEqual("0",xmlDocument.SelectSingleNode("EligBenefitResponse/isEligible").InnerText,"0 is eligible: the benefits window opens.");
			//And what FormEligibilityResponseDisplay lists.
			XmlNodeList xmlNodeListBenefits=xmlDocument.SelectNodes("EligBenefitResponse/BenefitResponse/PlanBenefit");
			Assert.AreEqual(3,xmlNodeListBenefits.Count);
			Assert.AreEqual("Deductible",xmlNodeListBenefits[1].SelectSingleNode("BenefitInfo").InnerText);
			Assert.AreEqual("50.00",xmlNodeListBenefits[1].SelectSingleNode("BenefitAmount").InnerText);
		}

		[TestMethod]
		public void InlineResponse_NotEligible() {
			string xml="<?xml version=\"1.0\" encoding=\"UTF-8\"?><soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\""
				+" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><soapenv:Body>"
				+"<ns1:lookupEligibilityResponse xmlns:ns1=\"http://www.dentalxchange.com/webservice\"><lookupEligibilityReturn xsi:type=\"ns1:Response\">"
				+"<content xsi:type=\"xsd:string\">&lt;EligBenefitResponse&gt;&lt;isEligible&gt;1&lt;/isEligible&gt;&lt;Response&gt;&lt;ErrorCode&gt;42&lt;/ErrorCode&gt;"
				+"&lt;ErrorMsg&gt;Synthetic subscriber not found&lt;/ErrorMsg&gt;&lt;/Response&gt;&lt;/EligBenefitResponse&gt;</content>"
				+"</lookupEligibilityReturn></ns1:lookupEligibilityResponse></soapenv:Body></soapenv:Envelope>";
			_fakeHttpHandler.Enqueue(HttpStatusCode.OK,xml,"text/xml");
			Dci.Response response=new DciSoapClient().lookupEligibility(Credentials(),Request());
			XmlDocument xmlDocument=new XmlDocument();
			xmlDocument.LoadXml(response.content);
			Assert.AreEqual("1",xmlDocument.SelectSingleNode("EligBenefitResponse/isEligible").InnerText);
			Assert.AreEqual("42",xmlDocument.SelectSingleNode("EligBenefitResponse/Response/ErrorCode").InnerText);
			Assert.AreEqual("Synthetic subscriber not found",xmlDocument.SelectSingleNode("EligBenefitResponse/Response/ErrorMsg").InnerText);
		}

		[TestMethod]
		public void Fault_IsASoapFaultException() {
			//FormInsPlan catches every exception and shows its own message; the fault's text is still there for anyone who looks.
			string xml="<?xml version=\"1.0\" encoding=\"UTF-8\"?><soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\"><soapenv:Body>"
				+"<soapenv:Fault><faultcode>soapenv:Server.userException</faultcode><faultstring>Authentication failed.</faultstring>"
				+"<detail><string>Authentication failed.</string></detail></soapenv:Fault></soapenv:Body></soapenv:Envelope>";
			_fakeHttpHandler.Enqueue(HttpStatusCode.InternalServerError,xml,"text/xml");
			SoapFaultException soapFaultException=Assert.ThrowsExactly<SoapFaultException>(() => new DciSoapClient().lookupEligibility(Credentials(),Request()));
			Assert.AreEqual("Authentication failed.",soapFaultException.Message);
			Assert.AreEqual("Server.userException",soapFaultException.Code.Name);
			Assert.AreEqual("Authentication failed.",soapFaultException.Detail.InnerText);
		}

		private static XmlElement FindById(XmlElement xmlElementBody,string href) {
			Assert.IsTrue(href.StartsWith("#"),href);
			return xmlElementBody.ChildNodes.OfType<XmlElement>().Single(x => x.GetAttribute("id")==href.Substring(1));
		}

		///<summary>The element's xsi:type as namespace#name.</summary>
		private static string XsiType(XmlElement xmlElement) {
			string value=xmlElement.GetAttribute("type",XSI);
			int indexColon=value.IndexOf(':');
			return xmlElement.GetNamespaceOfPrefix(value.Substring(0,indexColon))+"#"+value.Substring(indexColon+1);
		}
	}
}
