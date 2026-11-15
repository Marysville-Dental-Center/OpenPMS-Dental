// Stand-ins for DentalXChange's two attachment services, so the V-DXC2 tests never reach the network:
//  - RecordingApiRequest replaces APIRequest.Inst, which every XConnect call goes through, and answers with canned JSON.
//  - FakeDeaHandler answers the partner service's SOAP client (DeaSoapClient, through SoapCaller.HttpHandlerForTests) in memory with a canned
//    SOAP response. Unless a test has turned it on, it refuses every request instead of passing it on, and it never answers another host.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json;
using OpenPmsDentalBusiness;
using OpenPmsDentalBusiness.WebBridges;

namespace VDXC2Tests {
	///<summary>Records every XConnect request and answers from DictResponses (endpoint to JSON). Unknown endpoints throw.</summary>
	public class RecordingApiRequest:IApiRequest {
		public List<ApiCall> ListCalls=new List<ApiCall>();
		public Dictionary<string,string> DictResponses=new Dictionary<string,string>();

		public T SendRequest<T>(string urlEndpoint,HttpMethod method,AuthenticationHeaderValue authHeaderVal,string body,HttpContentType contentType,
			HttpClient clientOverride,List<string> queryParameters,JsonSerializerSettings deserializeSettings)
		{
			ApiCall apiCall=new ApiCall();
			apiCall.Endpoint=urlEndpoint;
			apiCall.Method=method;
			apiCall.Body=body;
			apiCall.ListQueryParameters=queryParameters==null ? new List<string>() : queryParameters.ToList();
			if(clientOverride!=null) {
				apiCall.BaseAddress=clientOverride.BaseAddress;
				apiCall.ApiKey=GetHeader(clientOverride,"API-Key");
				apiCall.Username=GetHeader(clientOverride,"username");
			}
			ListCalls.Add(apiCall);
			string json;
			if(!DictResponses.TryGetValue(urlEndpoint,out json)) {
				throw new InvalidOperationException("Unexpected XConnect endpoint in a test: "+urlEndpoint);
			}
			return JsonConvert.DeserializeObject<T>(json,deserializeSettings);
		}

		private static string GetHeader(HttpClient httpClient,string name) {
			IEnumerable<string> values;
			if(httpClient.DefaultRequestHeaders.TryGetValues(name,out values)) {
				return values.FirstOrDefault();
			}
			return null;
		}

		public T SendRequest<T,U>(string urlEndpoint,HttpMethod method,AuthenticationHeaderValue authHeaderVal,U body,HttpContentType contentType,
			HttpClient clientOverride,List<string> queryParameters,JsonSerializerSettings deserializeSettings)
		{
			throw new NotSupportedException();
		}

		public Task<T> GetRequestAsync<T>(string urlEndpoint,HttpClient clientOverride) {
			throw new NotSupportedException();
		}

		public Task<T> PostRequestAsync<T>(string urlEndpoint,string body,HttpContentType contentType,HttpClient clientOverride) {
			throw new NotSupportedException();
		}

		public Task<T> SendRequestAsync<T,U>(string urlEndpoint,HttpMethod method,AuthenticationHeaderValue authHeaderVal,U body,HttpContentType contentType,
			HttpClient clientOverride,List<string> queryParameters,JsonSerializerSettings deserializeSettings)
		{
			throw new NotSupportedException();
		}
	}

	public class ApiCall {
		public string Endpoint;
		public HttpMethod Method;
		public string Body;
		public List<string> ListQueryParameters;
		public Uri BaseAddress;
		public string ApiKey;
		public string Username;
	}

	///<summary>One SOAP request the partner service client sent: the address, the operation (first element of the SOAP body) and the whole request.
	///</summary>
	public class SoapCall {
		public Uri Uri;
		public string Operation;
		public XmlDocument XmlDocumentRequest;
	}

	///<summary>Answers the DentalXChange partner service (DEA) in memory, as SoapCaller.HttpHandlerForTests (S3a-6; it was a WebRequest prefix
	///before). While IsOn is false it refuses every request, and a request to any host but DentalXChange's two partner service hosts is refused
	///too, so nothing can reach DentalXChange or anyone else by accident.</summary>
	public class FakeDeaHandler:HttpMessageHandler {
		public static readonly FakeDeaHandler Inst=new FakeDeaHandler();
		public bool IsOn;
		public List<SoapCall> ListCalls=new List<SoapCall>();
		///<summary>Operation name to the object returned in the SOAP response's "return" element.</summary>
		public Dictionary<string,object> DictReturns=new Dictionary<string,object>();

		///<summary>Makes this handler answer every SOAP client call of the test (SoapCaller.HttpHandlerForTests).</summary>
		public static void Install() {
			SoapCaller.HttpHandlerForTests=Inst;
		}

		///<summary>Puts SoapCaller back to the network; call when the test is done.</summary>
		public static void Uninstall() {
			if(SoapCaller.HttpHandlerForTests==Inst) {
				SoapCaller.HttpHandlerForTests=null;
			}
		}

		public void Reset() {
			IsOn=false;
			ListCalls.Clear();
			DictReturns.Clear();
		}

		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequestMessage,CancellationToken cancellationToken) {
			Uri uri=httpRequestMessage.RequestUri;
			if(!IsOn) {
				throw new HttpRequestException("Test: the DentalXChange partner service is not answered by this test.");
			}
			bool isDeaHost=(uri.Host=="prelive2.dentalxchange.com" || uri.Host=="webservices.dentalxchange.com") && uri.AbsolutePath.StartsWith("/dea/");
			if(uri.Scheme!="https" || !isDeaHost) {
				throw new HttpRequestException("Test: not the DentalXChange partner service: "+uri);
			}
			byte[] arrayBytesRequest=await httpRequestMessage.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
			HttpResponseMessage httpResponseMessage=new HttpResponseMessage(HttpStatusCode.OK);
			httpResponseMessage.Content=new ByteArrayContent(Answer(uri,arrayBytesRequest));
			httpResponseMessage.Content.Headers.TryAddWithoutValidation("Content-Type","text/xml; charset=utf-8");
			return httpResponseMessage;
		}

		///<summary>Records the request and returns the response body for its operation.</summary>
		public byte[] Answer(Uri uri,byte[] arrayBytesRequest) {
			XmlDocument xmlDocument=new XmlDocument();
			xmlDocument.LoadXml(Encoding.UTF8.GetString(arrayBytesRequest).TrimStart('\uFEFF'));
			XmlNode xmlNodeBody=xmlDocument.DocumentElement.ChildNodes.Cast<XmlNode>().First(x => x.LocalName=="Body");
			string operation=xmlNodeBody.ChildNodes.Cast<XmlNode>().First(x => x.NodeType==XmlNodeType.Element).LocalName;
			ListCalls.Add(new SoapCall { Uri=uri,Operation=operation,XmlDocumentRequest=xmlDocument });
			object returnValue;
			if(!DictReturns.TryGetValue(operation,out returnValue)) {
				throw new HttpRequestException("Test: unexpected partner service operation "+operation);
			}
			return Encoding.UTF8.GetBytes(BuildEnvelope(operation,returnValue));
		}

		///<summary>A SOAP 1.1 document/literal wrapped response: operationResponse in the dxci.dea namespace, holding an unqualified "return" element
		///serialized with the generated data classes' own type mappings.</summary>
		public static string BuildEnvelope(string operation,object returnValue) {
			XmlSerializer xmlSerializer=new XmlSerializer(returnValue.GetType(),new XmlRootAttribute("return") { Namespace="" });
			StringBuilder stringBuilder=new StringBuilder();
			XmlWriterSettings xmlWriterSettings=new XmlWriterSettings { OmitXmlDeclaration=true };
			using(XmlWriter xmlWriter=XmlWriter.Create(stringBuilder,xmlWriterSettings)) {
				xmlSerializer.Serialize(xmlWriter,returnValue);
			}
			return "<?xml version=\"1.0\" encoding=\"utf-8\"?>"
				+"<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\"><soap:Body>"
				+"<ns2:"+operation+"Response xmlns:ns2=\"dxci.dea\">"+stringBuilder.ToString()+"</ns2:"+operation+"Response>"
				+"</soap:Body></soap:Envelope>";
		}
	}
}
