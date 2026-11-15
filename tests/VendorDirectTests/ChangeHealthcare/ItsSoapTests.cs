// S3a-6 (docs/specs/net10-plan.md; docs/port-notes/S3a-6.md): Change Healthcare (Emdeon) ITS medical claims through the hand-written SOAP
// client. The office does not use ITS today, but it must keep working on .NET 10 (decision 12). A fake ITS endpoint (SoapCaller's test
// handler) checks the request EmdeonMedical builds: the address for the test and production mailboxes, the SOAPAction, the login, the
// message type, the file name and the zipped batch; and it answers with an ItsReturn that EmdeonMedical reads. Synthetic data only; no call
// leaves the process.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalBusiness;
using OpenPmsDentalBusiness.Eclaims;
using Its=OpenPmsDentalBusiness.EmdeonITS;

namespace VendorDirectTests.ChangeHealthcare {
	///<summary>One call the fake ITS endpoint received.</summary>
	public sealed class ItsCall {
		public Uri Uri;
		public string SoapAction;
		public string ContentType;
		public string Operation;
		///<summary>The operation's parameters by name (sUserID, sPassword, sMessageType, ...), each in the ITS namespace.</summary>
		public Dictionary<string,string> DictFields=new Dictionary<string,string>();
	}

	///<summary>A fake ITS web service: answers only the two ITS hosts over HTTPS, records each call and answers with what FuncAnswer returns,
	///serialized as ITS does (document/literal: OperationResponse holding OperationResult with ErrorCode and Response).</summary>
	public sealed class FakeItsEndpoint:HttpMessageHandler {
		public List<ItsCall> ListCalls=new List<ItsCall>();
		public Func<ItsCall,Its.ItsReturn> FuncAnswer=itsCall => new Its.ItsReturn { ErrorCode=209,Response="Mailbox empty" };

		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequestMessage,CancellationToken cancellationToken) {
			Uri uri=httpRequestMessage.RequestUri;
			if(uri.Scheme!="https" || (uri.Host!="its.changehealthcare.com" && uri.Host!="cert.its.changehealthcare.com")) {
				throw new HttpRequestException("Test: not an ITS host: "+uri);
			}
			ItsCall itsCall=new ItsCall();
			itsCall.Uri=uri;
			itsCall.SoapAction=httpRequestMessage.Headers.TryGetValues("SOAPAction",out IEnumerable<string> values) ? values.First() : null;
			itsCall.ContentType=httpRequestMessage.Content.Headers.ContentType?.ToString();
			XmlDocument xmlDocument=new XmlDocument();
			xmlDocument.LoadXml(await httpRequestMessage.Content.ReadAsStringAsync().ConfigureAwait(false));
			XmlElement xmlElementBody=xmlDocument.DocumentElement.ChildNodes.OfType<XmlElement>().Single(x => x.LocalName=="Body");
			XmlElement xmlElementOperation=xmlElementBody.ChildNodes.OfType<XmlElement>().Single();
			Assert.AreEqual(ItsSoapClient.Namespace,xmlElementOperation.NamespaceURI);
			itsCall.Operation=xmlElementOperation.LocalName;
			foreach(XmlElement xmlElementField in xmlElementOperation.ChildNodes.OfType<XmlElement>()) {
				Assert.AreEqual(ItsSoapClient.Namespace,xmlElementField.NamespaceURI,xmlElementField.LocalName);
				itsCall.DictFields[xmlElementField.LocalName]=xmlElementField.InnerText;
			}
			ListCalls.Add(itsCall);
			Its.ItsReturn itsReturn=FuncAnswer(itsCall);
			string xml="<?xml version=\"1.0\" encoding=\"utf-8\"?><soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\""
				+" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><soap:Body>"
				+"<"+itsCall.Operation+"Response xmlns=\""+ItsSoapClient.Namespace+"\"><"+itsCall.Operation+"Result>"
				+"<ErrorCode>"+itsReturn.ErrorCode+"</ErrorCode><Response>"+System.Security.SecurityElement.Escape(itsReturn.Response)+"</Response>"
				+"</"+itsCall.Operation+"Result></"+itsCall.Operation+"Response></soap:Body></soap:Envelope>";
			HttpResponseMessage httpResponseMessage=new HttpResponseMessage(HttpStatusCode.OK);
			httpResponseMessage.Content=new StringContent(xml,Encoding.UTF8,"text/xml");
			return httpResponseMessage;
		}
	}

	[TestClass]
	public class ItsSoapTests {
		private FakeItsEndpoint _fakeItsEndpoint;
		private string _folder;
		private CultureInfo _cultureInfoOld;

		[TestInitialize]
		public void TestInitialize() {
			_fakeItsEndpoint=new FakeItsEndpoint();
			SoapCaller.HttpHandlerForTests=_fakeItsEndpoint;
			_folder=Path.Combine(Path.GetTempPath(),"s3a6_its_"+Guid.NewGuid().ToString("N"));
			Directory.CreateDirectory(Path.Combine(_folder,"export"));
			Directory.CreateDirectory(Path.Combine(_folder,"reports"));
			EmdeonMedical.ErrorMessage="";
			//EmdeonMedical's messages go through Lans.g, which needs no database in en-US.
			_cultureInfoOld=Thread.CurrentThread.CurrentCulture;
			Thread.CurrentThread.CurrentCulture=new CultureInfo("en-US");
		}

		[TestCleanup]
		public void TestCleanup() {
			SoapCaller.HttpHandlerForTests=null;
			Thread.CurrentThread.CurrentCulture=_cultureInfoOld;
			try {
				Directory.Delete(_folder,true);
			}
			catch(Exception ex) {
				ex.DoNothing();
			}
		}

		private Clearinghouse CreateClearinghouse(string isa15) {
			Clearinghouse clearinghouse=new Clearinghouse();
			clearinghouse.Description="Synthetic ITS";
			clearinghouse.CommBridge=EclaimsCommBridge.EmdeonMedical;
			clearinghouse.LoginID="synthetic-its-login";
			clearinghouse.Password="synthetic-its-password";
			clearinghouse.ISA15=isa15;
			clearinghouse.ExportPath=Path.Combine(_folder,"export")+"\\";
			clearinghouse.ResponsePath=Path.Combine(_folder,"reports")+"\\";
			clearinghouse.IsEraDownloadAllowed=EraBehaviors.DownloadAndReceive;
			return clearinghouse;
		}

		private string WriteBatch(int batchNum) {
			string path=Path.Combine(_folder,"export","claims"+batchNum+".txt");
			File.WriteAllText(path,"ISA*00*          *00*          *ZZ*SYNTHETIC      *ZZ*ITS            *261001*0930*^*00501*000000007*0*T*:~"
				+"ST*837*0001*005010X222A1~SE*2*0001~IEA*1*000000007~");
			return path;
		}

		[TestMethod]
		public void Launch_Test_SendsTheZippedBatchToTheTestMailbox() {
			string pathBatch=WriteBatch(7);
			byte[] arrayBytesBatch=File.ReadAllBytes(pathBatch);
			_fakeItsEndpoint.FuncAnswer=itsCall => new Its.ItsReturn { ErrorCode=0,Response="Synthetic file accepted" };
			Assert.IsTrue(EmdeonMedical.Launch(CreateClearinghouse("T"),7,EnumClaimMedType.Medical),EmdeonMedical.ErrorMessage);
			ItsCall itsCall=_fakeItsEndpoint.ListCalls.Single();
			Assert.AreEqual(new Uri("https://cert.its.changehealthcare.com/ITS/itsws.asmx"),itsCall.Uri);
			Assert.AreEqual("\"https://ITSWebService.changehealthcare.com/PutFileExt\"",itsCall.SoapAction);
			Assert.AreEqual("text/xml; charset=utf-8",itsCall.ContentType);
			Assert.AreEqual("PutFileExt",itsCall.Operation);
			CollectionAssert.AreEqual(new[] { "sUserID","sPassword","sMessageType","sFileName","sEncodedPutFile" },itsCall.DictFields.Keys.ToArray(),
				"The parameters in the proxy's order.");
			Assert.AreEqual("synthetic-its-login",itsCall.DictFields["sUserID"]);
			Assert.AreEqual("synthetic-its-password",itsCall.DictFields["sPassword"]);
			Assert.AreEqual("MCT",itsCall.DictFields["sMessageType"],"Medical claims to the test mailbox.");
			Assert.AreEqual("claims7.txt",itsCall.DictFields["sFileName"]);
			//The payload: a zip archive, base64-encoded, with the batch file at its root.
			string folderUnzip=Path.Combine(_folder,"unzip");
			Directory.CreateDirectory(folderUnzip);
			using(MemoryStream memoryStream=new MemoryStream(Convert.FromBase64String(itsCall.DictFields["sEncodedPutFile"]))) {
				List<string> listPaths=ODZip.ExtractAll(memoryStream,folderUnzip,overwrite:false);
				Assert.AreEqual(1,listPaths.Count);
				Assert.AreEqual("claims7.txt",Path.GetFileName(listPaths[0]));
				CollectionAssert.AreEqual(arrayBytesBatch,File.ReadAllBytes(listPaths[0]));
			}
			Assert.IsFalse(File.Exists(pathBatch),"The batch file is removed after it was sent.");
		}

		[TestMethod]
		public void Launch_Production_InstitutionalClaims() {
			WriteBatch(8);
			_fakeItsEndpoint.FuncAnswer=itsCall => new Its.ItsReturn { ErrorCode=0,Response="Synthetic file accepted" };
			Assert.IsTrue(EmdeonMedical.Launch(CreateClearinghouse("P"),8,EnumClaimMedType.Institutional),EmdeonMedical.ErrorMessage);
			ItsCall itsCall=_fakeItsEndpoint.ListCalls.Single();
			Assert.AreEqual(new Uri("https://its.changehealthcare.com/ITS/ITSWS.asmx"),itsCall.Uri);
			Assert.AreEqual("HCD",itsCall.DictFields["sMessageType"],"Institutional claims to the production mailbox.");
		}

		[TestMethod]
		public void Launch_Rejected_RollsBackWithTheItsMessage() {
			string pathBatch=WriteBatch(9);
			_fakeItsEndpoint.FuncAnswer=itsCall => new Its.ItsReturn { ErrorCode=5,Response="Synthetic rejection & reason" };
			Assert.IsFalse(EmdeonMedical.Launch(CreateClearinghouse("T"),9,EnumClaimMedType.Medical));
			StringAssert.Contains(EmdeonMedical.ErrorMessage,"Error number from Emdeon: 5");
			StringAssert.Contains(EmdeonMedical.ErrorMessage,"Error message from Emdeon: Synthetic rejection & reason");
			Assert.IsFalse(File.Exists(pathBatch));
		}

		[TestMethod]
		public void Retrieve_SavesTheReportsAndRemovesThemFromTheMailbox() {
			byte[] arrayBytesZip=ODZip.CreateArchive(new[] { new KeyValuePair<string,byte[]>("synthetic277.txt",Encoding.ASCII.GetBytes("ISA*00*~ST*277*0001~")) });
			_fakeItsEndpoint.FuncAnswer=itsCall => {
				if(itsCall.Operation=="GetFile" && itsCall.DictFields["sMessageType"]=="MCTG") {
					return new Its.ItsReturn { ErrorCode=0,Response=Convert.ToBase64String(arrayBytesZip) };
				}
				return new Its.ItsReturn { ErrorCode=209,Response="Mailbox empty" };
			};
			Assert.IsTrue(EmdeonMedical.Retrieve(CreateClearinghouse("T")),EmdeonMedical.ErrorMessage);
			Assert.IsTrue(_fakeItsEndpoint.ListCalls.All(x => x.Operation=="GetFile"
				&& x.SoapAction=="\"https://ITSWebService.changehealthcare.com/GetFile\""
				&& x.DictFields["sUserID"]=="synthetic-its-login" && x.DictFields["sPassword"]=="synthetic-its-password"));
			CollectionAssert.AreEqual(new[] { "HCTG","MCTG","MCTD","AETG","EDT" },_fakeItsEndpoint.ListCalls.Select(x => x.DictFields["sMessageType"]).ToArray(),
				"Each report type of the test mailbox, and the delete of the one downloaded.");
			string pathReport=Path.Combine(_folder,"reports","synthetic277.txt");
			Assert.IsTrue(File.Exists(pathReport));
			Assert.AreEqual("ISA*00*~ST*277*0001~",File.ReadAllText(pathReport));
			Assert.AreEqual(0,Directory.GetFiles(Path.Combine(_folder,"reports"),"*.zip").Length,"The downloaded zip is removed once extracted.");
		}

		[TestMethod]
		public void Retrieve_Error_GivesTheItsMessage() {
			_fakeItsEndpoint.FuncAnswer=itsCall => new Its.ItsReturn { ErrorCode=101,Response="Synthetic login refused" };
			Assert.IsFalse(EmdeonMedical.Retrieve(CreateClearinghouse("P")));
			StringAssert.Contains(EmdeonMedical.ErrorMessage,"Failed to get reports. Error number from Emdeon: 101");
			StringAssert.Contains(EmdeonMedical.ErrorMessage,"Synthetic login refused");
			Assert.AreEqual("HCDG",_fakeItsEndpoint.ListCalls.Single().DictFields["sMessageType"]);
			Assert.AreEqual(new Uri("https://its.changehealthcare.com/ITS/ITSWS.asmx"),_fakeItsEndpoint.ListCalls.Single().Uri);
		}
	}
}
