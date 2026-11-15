// SOAP 1.1 calls over HttpClient and XmlSerializer, without System.Web.Services (docs/specs/net10-plan.md, S3a-6; docs/port-notes/S3a-6.md).
// It replaces SoapHttpClientProtocol for the vendor web services: the generated [XmlType]/[SoapType] data classes stay, and each operation is a
// method of a SoapClient that carries the same attributes the generated proxy method carried. The request is serialized with the same
// XmlMembersMapping SoapHttpClientProtocol built from those attributes (XmlReflectionImporter.ImportMembersMapping), inside the same envelope,
// with the same SOAPAction, content type and user agent, and a SOAP fault becomes a SoapFaultException with the fault's text, code, actor and
// detail. The envelope equivalence with the old proxies is proved by tests/VDXC2Tests/SoapEnvelopeTests.cs. DentalXChange's rpc/encoded
// eligibility call (DciSoapClient) writes its body by hand and uses Send here for the rest.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using CodeBase;

namespace OpenPmsDentalBusiness {
	///<summary>The binding namespace of a SoapClient, as WebServiceBindingAttribute gave it: the default namespace of its XML mappings.</summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class SoapBindingAttribute:Attribute {
		public string Namespace;

		public SoapBindingAttribute(string bindingNamespace) {
			Namespace=bindingNamespace;
		}
	}

	///<summary>Marks a method of a SoapClient as one SOAP 1.1 document/literal operation with wrapped parameters, as
	///SoapDocumentMethodAttribute(action, RequestNamespace, ResponseNamespace, Use=Literal, ParameterStyle=Wrapped) did. The request element is
	///the method's name and the response element the method's name and "Response". The parameters and the return value carry the same
	///XmlElement/XmlArray/XmlIgnore attributes as on the generated proxy.</summary>
	[AttributeUsage(AttributeTargets.Method)]
	public class SoapOperationAttribute:Attribute {
		///<summary>The SOAPAction, without quotes. Empty for the DentalXChange services.</summary>
		public string Action;
		public string RequestNamespace;
		public string ResponseNamespace;

		public SoapOperationAttribute(string action) {
			Action=action;
		}
	}

	///<summary>A SOAP fault, in place of System.Web.Services' SoapException: Message is the faultstring, Detail the detail element (null when the
	///fault has none, as SoapException's was).</summary>
	public class SoapFaultException:Exception {
		///<summary>The faultcode, resolved to its namespace (for example soapenv:Server.userException). Null when the fault has none.</summary>
		public XmlQualifiedName Code;
		public string Actor;
		///<summary>The fault's detail element with everything in it, or null.</summary>
		public XmlNode Detail;

		public SoapFaultException(string message,XmlQualifiedName code,string actor,XmlNode detail) : base(message) {
			Code=code;
			Actor=actor;
			Detail=detail;
		}
	}

	///<summary>The base of the hand-written SOAP clients: the address, the time limit and the user agent, as on SoapHttpClientProtocol. A
	///subclass carries SoapBindingAttribute and one SoapOperationAttribute method per operation.</summary>
	public abstract class SoapClient:IDisposable {
		///<summary>The service address. Each client's constructor sets the default from Properties.Settings, as the generated proxy did.</summary>
		public string Url;
		///<summary>Milliseconds. 100000 by default, as on SoapHttpClientProtocol.</summary>
		public int Timeout=100000;
		///<summary>The User-Agent header. By default the one the .NET Framework proxies sent.</summary>
		public string UserAgent=SoapCaller.UserAgentDefault;

		///<summary>Sends the operation methodName (a SoapOperationAttribute method of this client) with arrayArgs, its parameters in order, and
		///returns the response's members (the return value first). Throws SoapFaultException for a SOAP fault and WebException for a failed
		///request, as the generated proxy did.</summary>
		protected object[] Invoke(string methodName,params object[] arrayArgs) {
			return SoapCaller.Invoke(this,methodName,arrayArgs);
		}

		///<summary>Nothing to release; kept so that callers can keep their using blocks.</summary>
		public void Dispose() {
		}
	}

	///<summary>Builds, sends and reads SOAP 1.1 messages for the SoapClient classes.</summary>
	public static class SoapCaller {
		public const string EnvelopeNamespace="http://schemas.xmlsoap.org/soap/envelope/";
		public const string EncodingNamespace="http://schemas.xmlsoap.org/soap/encoding/";
		public const string XsiNamespace="http://www.w3.org/2001/XMLSchema-instance";
		public const string XsdNamespace="http://www.w3.org/2001/XMLSchema";
		///<summary>The User-Agent SoapHttpClientProtocol sent on .NET Framework 4.8 when the caller set none.</summary>
		public const string UserAgentDefault="Mozilla/4.0 (compatible; MSIE 6.0; MS Web Services Client Protocol 4.0.30319.42000)";
		///<summary>The request content type: SOAP 1.1 in UTF-8, without a byte order mark.</summary>
		public const string ContentTypeRequest="text/xml; charset=utf-8";
		///<summary>Tests only: answers every SOAP request instead of the network. Null in the program.</summary>
		public static HttpMessageHandler HttpHandlerForTests;
		private static readonly object _lock=new object();
		private static readonly Dictionary<Type,Dictionary<string,SoapOperation>> _dictOperations=new Dictionary<Type,Dictionary<string,SoapOperation>>();
		private static HttpClient _httpClient;
		private static HttpClient _httpClientDefaultCredentials;

		///<summary>One operation of a client type: its attribute and the serializers of its request and response members.</summary>
		private class SoapOperation {
			public MethodInfo MethodInfo;
			public SoapOperationAttribute SoapOperationAttribute;
			public XmlSerializer XmlSerializerRequest;
			public XmlSerializer XmlSerializerResponse;
		}

		#region Document/literal
		///<summary>Sends the operation and returns the response's members. See SoapClient.Invoke.</summary>
		public static object[] Invoke(SoapClient soapClient,string methodName,object[] arrayArgs) {
			SoapOperation soapOperation=GetOperation(soapClient.GetType(),methodName);
			byte[] arrayBytesRequest=BuildRequest(soapClient.GetType(),methodName,arrayArgs);
			return Send(soapClient,soapOperation.SoapOperationAttribute.Action,arrayBytesRequest,
				xmlReader => (object[])soapOperation.XmlSerializerResponse.Deserialize(xmlReader,null));
		}

		///<summary>The request message the operation sends for arrayArgs: the bytes of the SOAP envelope, in UTF-8 without a byte order mark, written
		///as SoapHttpClientProtocol wrote it.</summary>
		public static byte[] BuildRequest(Type typeClient,string methodName,object[] arrayArgs) {
			SoapOperation soapOperation=GetOperation(typeClient,methodName);
			return WriteEnvelope(isEncoded:false,null,xmlWriter => soapOperation.XmlSerializerRequest.Serialize(xmlWriter,arrayArgs,null,null));
		}

		///<summary>Reads a whole response message for the operation, as Invoke does after the HTTP exchange. For tests and tools.</summary>
		public static object[] ReadResponse(Type typeClient,string methodName,byte[] arrayBytesResponse,string contentType=ContentTypeRequest) {
			SoapOperation soapOperation=GetOperation(typeClient,methodName);
			using MemoryStream memoryStream=new MemoryStream(arrayBytesResponse);
			return ReadEnvelope(memoryStream,contentType,xmlReader => (object[])soapOperation.XmlSerializerResponse.Deserialize(xmlReader,null));
		}

		///<summary>The operation methodName of typeClient. The mappings of every operation of the type are imported with one importer whose default
		///namespace is the binding's and which knows the types named by the client's XmlInclude attributes, as SoapClientType did. Built once per
		///type: XmlSerializer.FromMappings makes a new serializer assembly every time it is called. It is called once per operation, with that
		///operation's request and response, because a serializer assembly keys its mappings by their member types, which two operations can share
		///(validateClaim and lookupEligibility take the same types; SoapHttpClientProtocol gave each mapping its own key, which is internal).
		///</summary>
		private static SoapOperation GetOperation(Type typeClient,string methodName) {
			lock(_lock) {
				if(!_dictOperations.TryGetValue(typeClient,out Dictionary<string,SoapOperation> dictOperations)) {
					dictOperations=ImportOperations(typeClient);
					_dictOperations[typeClient]=dictOperations;
				}
				if(!dictOperations.TryGetValue(methodName,out SoapOperation soapOperation)) {
					throw new ArgumentException("No SOAP operation "+methodName+" on "+typeClient.Name+".",nameof(methodName));
				}
				return soapOperation;
			}
		}

		private static Dictionary<string,SoapOperation> ImportOperations(Type typeClient) {
			SoapBindingAttribute soapBindingAttribute=typeClient.GetCustomAttribute<SoapBindingAttribute>();
			if(soapBindingAttribute==null) {
				throw new InvalidOperationException(typeClient.Name+" has no SoapBindingAttribute.");
			}
			XmlReflectionImporter xmlReflectionImporter=new XmlReflectionImporter(soapBindingAttribute.Namespace);
			xmlReflectionImporter.IncludeTypes(typeClient);
			List<SoapOperation> listSoapOperations=new List<SoapOperation>();
			List<XmlMapping[]> listXmlMappings=new List<XmlMapping[]>();
			MethodInfo[] arrayMethodInfos=typeClient.GetMethods(BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly);
			for(int i=0;i<arrayMethodInfos.Length;i++) {
				SoapOperationAttribute soapOperationAttribute=arrayMethodInfos[i].GetCustomAttribute<SoapOperationAttribute>();
				if(soapOperationAttribute==null) {
					continue;
				}
				SoapOperation soapOperation=new SoapOperation();
				soapOperation.MethodInfo=arrayMethodInfos[i];
				soapOperation.SoapOperationAttribute=soapOperationAttribute;
				listSoapOperations.Add(soapOperation);
				listXmlMappings.Add(new XmlMapping[] {
					xmlReflectionImporter.ImportMembersMapping(arrayMethodInfos[i].Name,soapOperationAttribute.RequestNamespace,
						GetRequestMembers(arrayMethodInfos[i]),true,false,false,XmlMappingAccess.Write),
					xmlReflectionImporter.ImportMembersMapping(arrayMethodInfos[i].Name+"Response",soapOperationAttribute.ResponseNamespace,
						GetResponseMembers(arrayMethodInfos[i]),true,false,false,XmlMappingAccess.Read),
				});
			}
			Dictionary<string,SoapOperation> dictOperations=new Dictionary<string,SoapOperation>();
			for(int i=0;i<listSoapOperations.Count;i++) {
				XmlSerializer[] arrayXmlSerializers=XmlSerializer.FromMappings(listXmlMappings[i]);
				listSoapOperations[i].XmlSerializerRequest=arrayXmlSerializers[0];
				listSoapOperations[i].XmlSerializerResponse=arrayXmlSerializers[1];
				dictOperations[listSoapOperations[i].MethodInfo.Name]=listSoapOperations[i];
			}
			return dictOperations;
		}

		///<summary>One member per parameter, named and typed as the parameter, with the parameter's XML attributes (SoapReflector.GetInParameters).
		///</summary>
		private static XmlReflectionMember[] GetRequestMembers(MethodInfo methodInfo) {
			ParameterInfo[] arrayParameterInfos=methodInfo.GetParameters();
			XmlReflectionMember[] arrayXmlReflectionMembers=new XmlReflectionMember[arrayParameterInfos.Length];
			for(int i=0;i<arrayParameterInfos.Length;i++) {
				XmlReflectionMember xmlReflectionMember=new XmlReflectionMember();
				xmlReflectionMember.MemberName=arrayParameterInfos[i].Name;
				xmlReflectionMember.MemberType=arrayParameterInfos[i].ParameterType;
				xmlReflectionMember.XmlAttributes=new XmlAttributes(arrayParameterInfos[i]);
				xmlReflectionMember.SoapAttributes=new SoapAttributes(arrayParameterInfos[i]);
				arrayXmlReflectionMembers[i]=xmlReflectionMember;
			}
			return arrayXmlReflectionMembers;
		}

		///<summary>The return value as the only member: named after the method and "Result" unless its XmlElement attribute names it (the
		///DentalXChange services name it "return"), with the return value's XML attributes (SoapReflector.GetOutParameters).</summary>
		private static XmlReflectionMember[] GetResponseMembers(MethodInfo methodInfo) {
			if(methodInfo.ReturnType==typeof(void)) {
				return new XmlReflectionMember[0];
			}
			XmlReflectionMember xmlReflectionMember=new XmlReflectionMember();
			xmlReflectionMember.MemberName=methodInfo.Name+"Result";
			xmlReflectionMember.MemberType=methodInfo.ReturnType;
			xmlReflectionMember.IsReturnValue=true;
			xmlReflectionMember.XmlAttributes=new XmlAttributes(methodInfo.ReturnTypeCustomAttributes);
			xmlReflectionMember.XmlAttributes.XmlRoot=null;
			xmlReflectionMember.SoapAttributes=new SoapAttributes(methodInfo.ReturnTypeCustomAttributes);
			return new XmlReflectionMember[] { xmlReflectionMember };
		}
		#endregion Document/literal

		#region Envelope
		///<summary>Writes a SOAP 1.1 envelope around what actionWriteBody writes into the body, the way SoapHttpClientProtocol.Serialize did: an
		///XmlTextWriter over UTF-8 without a byte order mark, the XML declaration, soap:Envelope declaring soap, xsi and xsd (and for rpc/encoded
		///soapenc, tns and types, with soap:encodingStyle on the body), no header.</summary>
		public static byte[] WriteEnvelope(bool isEncoded,string serviceNamespace,Action<XmlWriter> actionWriteBody) {
			using MemoryStream memoryStream=new MemoryStream();
			using(StreamWriter streamWriter=new StreamWriter(memoryStream,new UTF8Encoding(false),1024,leaveOpen:true)) {
				XmlTextWriter xmlTextWriter=new XmlTextWriter(streamWriter);
				xmlTextWriter.WriteStartDocument();
				xmlTextWriter.WriteStartElement("soap","Envelope",EnvelopeNamespace);
				xmlTextWriter.WriteAttributeString("xmlns","soap",null,EnvelopeNamespace);
				if(isEncoded) {
					xmlTextWriter.WriteAttributeString("xmlns","soapenc",null,EncodingNamespace);
					xmlTextWriter.WriteAttributeString("xmlns","tns",null,serviceNamespace);
					xmlTextWriter.WriteAttributeString("xmlns","types",null,serviceNamespace+"/encodedTypes");
				}
				xmlTextWriter.WriteAttributeString("xmlns","xsi",null,XsiNamespace);
				xmlTextWriter.WriteAttributeString("xmlns","xsd",null,XsdNamespace);
				xmlTextWriter.WriteStartElement("Body",EnvelopeNamespace);
				if(isEncoded) {
					xmlTextWriter.WriteAttributeString("soap","encodingStyle",null,EncodingNamespace);
				}
				actionWriteBody(xmlTextWriter);
				xmlTextWriter.WriteEndElement();
				xmlTextWriter.WriteEndElement();
				xmlTextWriter.Flush();
			}
			return memoryStream.ToArray();
		}

		///<summary>Reads a SOAP 1.1 response envelope from stream as SoapHttpClientProtocol.ReadResponse did: the reader honours the content type's
		///charset, prohibits DTDs and resolves nothing; the header is skipped; a fault in the body throws SoapFaultException; otherwise funcReadBody
		///reads the body's content, starting at its first element.</summary>
		public static T ReadEnvelope<T>(Stream stream,string contentType,Func<XmlReader,T> funcReadBody) {
			Encoding encoding=GetCharset(contentType);
			XmlTextReader xmlTextReader=encoding==null ? new XmlTextReader(stream) : new XmlTextReader(new StreamReader(stream,encoding,true));
			using(xmlTextReader) {
				xmlTextReader.DtdProcessing=DtdProcessing.Prohibit;
				xmlTextReader.Normalization=true;
				xmlTextReader.XmlResolver=null;
				xmlTextReader.MoveToContent();
				xmlTextReader.ReadStartElement("Envelope",EnvelopeNamespace);
				xmlTextReader.MoveToContent();
				if(xmlTextReader.IsStartElement("Header",EnvelopeNamespace)) {
					xmlTextReader.Skip();
					xmlTextReader.MoveToContent();
				}
				xmlTextReader.ReadStartElement("Body",EnvelopeNamespace);
				xmlTextReader.MoveToContent();
				if(xmlTextReader.IsStartElement("Fault",EnvelopeNamespace)) {
					throw ReadFault(xmlTextReader);
				}
				return funcReadBody(xmlTextReader);
			}
		}

		///<summary>The SOAP 1.1 fault at the reader (SoapHttpClientProtocol.ReadSoapException): faultcode, faultstring, faultactor and detail,
		///unqualified or in the envelope namespace; anything else is skipped.</summary>
		private static SoapFaultException ReadFault(XmlReader xmlReader) {
			XmlQualifiedName xmlQualifiedNameCode=null;
			string faultString=null;
			string faultActor=null;
			XmlNode xmlNodeDetail=null;
			if(xmlReader.IsEmptyElement) {
				xmlReader.Skip();
				return new SoapFaultException("",null,null,null);
			}
			xmlReader.ReadStartElement("Fault",EnvelopeNamespace);
			xmlReader.MoveToContent();
			while(xmlReader.NodeType!=XmlNodeType.EndElement && xmlReader.NodeType!=XmlNodeType.None) {
				if(xmlReader.NodeType!=XmlNodeType.Element) {
					xmlReader.Read();
					xmlReader.MoveToContent();
					continue;
				}
				if(xmlReader.NamespaceURI!="" && xmlReader.NamespaceURI!=EnvelopeNamespace) {
					xmlReader.Skip();
				}
				else if(xmlReader.LocalName=="faultcode") {
					xmlQualifiedNameCode=ReadFaultCode(xmlReader);
				}
				else if(xmlReader.LocalName=="faultstring") {
					faultString=xmlReader.ReadElementString();
				}
				else if(xmlReader.LocalName=="faultactor") {
					faultActor=xmlReader.ReadElementString();
				}
				else if(xmlReader.LocalName=="detail") {
					xmlNodeDetail=new XmlDocument().ReadNode(xmlReader);
				}
				else {
					xmlReader.Skip();
				}
				xmlReader.MoveToContent();
			}
			return new SoapFaultException(faultString??"",xmlQualifiedNameCode,faultActor,xmlNodeDetail);
		}

		///<summary>A faultcode element's qualified name, its prefix resolved where the element is.</summary>
		private static XmlQualifiedName ReadFaultCode(XmlReader xmlReader) {
			if(xmlReader.IsEmptyElement) {
				xmlReader.Skip();
				return null;
			}
			xmlReader.ReadStartElement();
			string qualifiedName=xmlReader.ReadString().Trim();
			string ns="";
			int indexColon=qualifiedName.IndexOf(':');
			if(indexColon>=0) {
				ns=xmlReader.LookupNamespace(qualifiedName.Substring(0,indexColon))??"";
			}
			xmlReader.ReadEndElement();
			return new XmlQualifiedName(qualifiedName.Substring(indexColon+1),ns);
		}

		///<summary>The charset of a content type, or null when it names none or one this runtime does not know.</summary>
		private static Encoding GetCharset(string contentType) {
			if(string.IsNullOrEmpty(contentType)) {
				return null;
			}
			try {
				MediaTypeHeaderValue mediaTypeHeaderValue=MediaTypeHeaderValue.Parse(contentType);
				if(string.IsNullOrEmpty(mediaTypeHeaderValue.CharSet)) {
					return null;
				}
				return Encoding.GetEncoding(mediaTypeHeaderValue.CharSet.Trim('"'));
			}
			catch(Exception ex) {
				ex.DoNothing();
				return null;
			}
		}

		///<summary>True for the SOAP 1.1 media type, text/xml (and application/soap+xml, which SoapHttpClientProtocol also accepted).</summary>
		private static bool IsSoapContentType(string contentType) {
			if(string.IsNullOrEmpty(contentType)) {
				return false;
			}
			string mediaType=contentType.Split(';')[0].Trim();
			return mediaType.Equals("text/xml",StringComparison.OrdinalIgnoreCase) || mediaType.Equals("application/soap+xml",StringComparison.OrdinalIgnoreCase);
		}
		#endregion Envelope

		#region HTTP
		///<summary>Posts arrayBytesRequest to the client's Url with the SOAPAction soapAction and reads the response, as SoapHttpClientProtocol.Invoke
		///did: no redirects, no cookies, the client's time limit and user agent, and for an address on localhost at a port of 1024 or more the
		///Windows login (UseDefaultCredentials). A status of 300 or more other than 400 and 500 throws WebException (ProtocolError); a response
		///that is not text/xml throws WebException for 400 and InvalidOperationException otherwise; a SOAP fault throws SoapFaultException; a
		///request that fails or times out throws WebException. funcReadBody reads the body of a response without a fault.</summary>
		public static T Send<T>(SoapClient soapClient,string soapAction,byte[] arrayBytesRequest,Func<XmlReader,T> funcReadBody) {
			if(string.IsNullOrEmpty(soapClient.Url)) {
				throw new InvalidOperationException("The SOAP client has no address.");
			}
			Uri uri=new Uri(soapClient.Url);
			using HttpRequestMessage httpRequestMessage=new HttpRequestMessage(HttpMethod.Post,uri);
			ByteArrayContent byteArrayContent=new ByteArrayContent(arrayBytesRequest);
			byteArrayContent.Headers.ContentType=MediaTypeHeaderValue.Parse(ContentTypeRequest);
			httpRequestMessage.Content=byteArrayContent;
			httpRequestMessage.Headers.TryAddWithoutValidation("SOAPAction","\""+(soapAction??"")+"\"");
			if(!string.IsNullOrEmpty(soapClient.UserAgent)) {
				httpRequestMessage.Headers.TryAddWithoutValidation("User-Agent",soapClient.UserAgent);
			}
			HttpMessageHandler httpMessageHandler=HttpHandlerForTests;
			HttpClient httpClient=httpMessageHandler==null ? GetHttpClient(IsLocalFileSystemWebService(uri)) : new HttpClient(httpMessageHandler,false);
			try {
				using CancellationTokenSource cancellationTokenSource=new CancellationTokenSource();
				if(soapClient.Timeout>=0) {
					cancellationTokenSource.CancelAfter(soapClient.Timeout);
				}
				HttpResponseMessage httpResponseMessage;
				try {
					httpResponseMessage=httpClient.SendAsync(httpRequestMessage,HttpCompletionOption.ResponseContentRead,cancellationTokenSource.Token)
						.ConfigureAwait(false).GetAwaiter().GetResult();
				}
				catch(TaskCanceledException ex) {
					throw new WebException("The operation has timed out",ex,WebExceptionStatus.Timeout,null);
				}
				catch(HttpRequestException ex) {
					WebException webExceptionInner=ex.InnerException as WebException;
					throw new WebException(ex.GetBaseException().Message,ex,webExceptionInner?.Status??WebExceptionStatus.UnknownError,null);
				}
				using(httpResponseMessage) {
					byte[] arrayBytesResponse=httpResponseMessage.Content==null ? new byte[0]
						: httpResponseMessage.Content.ReadAsByteArrayAsync().ConfigureAwait(false).GetAwaiter().GetResult();
					string contentType=httpResponseMessage.Content?.Headers.ContentType?.ToString()??"";
					int statusCode=(int)httpResponseMessage.StatusCode;
					if(statusCode>=300 && statusCode!=500 && statusCode!=400) {
						throw new WebException(GetFailureText(httpResponseMessage,arrayBytesResponse,contentType),null,WebExceptionStatus.ProtocolError,null);
					}
					if(!IsSoapContentType(contentType) || arrayBytesResponse.Length==0) {
						if(statusCode==400) {
							throw new WebException(GetFailureText(httpResponseMessage,arrayBytesResponse,contentType),null,WebExceptionStatus.ProtocolError,null);
						}
						throw new InvalidOperationException("Client found response content type of '"+contentType+"', but expected 'text/xml'."
							+Environment.NewLine+GetFailureText(httpResponseMessage,arrayBytesResponse,contentType));
					}
					using MemoryStream memoryStream=new MemoryStream(arrayBytesResponse);
					return ReadEnvelope(memoryStream,contentType,funcReadBody);
				}
			}
			finally {
				if(httpMessageHandler!=null) {
					httpClient.Dispose();
				}
			}
		}

		///<summary>The shared client: no redirects and no cookies, as HttpWebClientProtocol; with the Windows login for a local test service.</summary>
		private static HttpClient GetHttpClient(bool useDefaultCredentials) {
			lock(_lock) {
				if(useDefaultCredentials) {
					if(_httpClientDefaultCredentials==null) {
						_httpClientDefaultCredentials=CreateHttpClient(true);
					}
					return _httpClientDefaultCredentials;
				}
				if(_httpClient==null) {
					_httpClient=CreateHttpClient(false);
				}
				return _httpClient;
			}
		}

		private static HttpClient CreateHttpClient(bool useDefaultCredentials) {
			HttpClientHandler httpClientHandler=new HttpClientHandler();
			httpClientHandler.AllowAutoRedirect=false;
			httpClientHandler.UseCookies=false;
			httpClientHandler.UseDefaultCredentials=useDefaultCredentials;
			HttpClient httpClient=new HttpClient(httpClientHandler);
			httpClient.Timeout=System.Threading.Timeout.InfiniteTimeSpan;//Each request has its own time limit (SoapClient.Timeout).
			return httpClient;
		}

		///<summary>An address on localhost at a port of 1024 or more, for which the generated proxies used the Windows login.</summary>
		private static bool IsLocalFileSystemWebService(Uri uri) {
			return uri.Port>=1024 && string.Compare(uri.Host,"localHost",StringComparison.OrdinalIgnoreCase)==0;
		}

		///<summary>The text of a failed response, as RequestResponseUtils.CreateResponseExceptionString wrote it: the status, or the body when the
		///status is OK and the body is text.</summary>
		private static string GetFailureText(HttpResponseMessage httpResponseMessage,byte[] arrayBytesResponse,string contentType) {
			if(httpResponseMessage.StatusCode!=HttpStatusCode.OK) {
				return "The request failed with HTTP status "+(int)httpResponseMessage.StatusCode+": "+httpResponseMessage.ReasonPhrase+".";
			}
			Encoding encoding=GetCharset(contentType)??Encoding.UTF8;
			return "The request failed with the error message:"+Environment.NewLine+"--"+Environment.NewLine+encoding.GetString(arrayBytesResponse)
				+Environment.NewLine+"--.";
		}
		#endregion HTTP
	}
}
