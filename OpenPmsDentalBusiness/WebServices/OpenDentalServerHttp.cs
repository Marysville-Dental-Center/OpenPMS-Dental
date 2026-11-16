// The Middle Tier client on HttpClient (package S3b-6 of the .NET 10 move, docs/port-notes/S3b-6.md), in place of the ASMX proxy that Visual
// Studio generated from the server's WSDL (Web References\OpenDentalServer, OpenDentalServerReal), which needs System.Web.Services and so
// does not exist on .NET. It sends the serialized DTO as the raw UTF-8 body of a POST to the server's address and returns the body of the
// answer, the same strings the SOAP envelope carried. No SOAP: the server (OpenPmsDentalServer) changed with it, and the version check
// refuses a client of another build anyway.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Net;
using System.Net.Http;
using System.Text;

namespace OpenPmsDentalBusiness.WebServices {
	///<summary>One Middle Tier server address. Every instance shares one HttpClient (cached, so its connections are reused across requests and
	///threads), with the old proxy's settings: a timeout of one hour, the proxy of MiddleTierProxyConfig.xml (RemotingClient.MidTierProxyAddress)
	///or else the system's, and the old User-Agent.</summary>
	public class OpenDentalServerHttp:IOpenDentalServer {
		///<summary>The User-Agent of the old proxy. Its default, "Mozilla/4.0 (compatible; MSIE 6.0; MS Web Services Client Protocol
		///4.0.30319.296)", was refused by a customer's firewall, which also looked for IE8Mercury (Open Dental's note of the SOAP proxy).</summary>
		public const string USER_AGENT="Mozilla/4.0 (compatible; MSIE 7.0; MS Web Services Client Protocol 4.0.30319.296; IE8Mercury)";
		///<summary>The content type of the request: the DTO as UTF-8 text.</summary>
		public const string MEDIA_TYPE="text/plain";
		///<summary>One hour, as the old proxy's Timeout of 3600000 ms (2020: long processes over the Middle Tier must finish; the server has no
		///execution timeout).</summary>
		public static readonly TimeSpan TimeSpanTimeout=TimeSpan.FromHours(1);
		///<summary>How many characters of an error answer go into the exception's message.</summary>
		private const int ERROR_TEXT_MAX=2000;
		private static readonly object _lockHttpClient=new object();
		///<summary>The cached client, and the proxy settings and handler it was made with.</summary>
		private static HttpClient _httpClient;
		private static string _keyHttpClient;
		private static HttpMessageHandler _httpMessageHandlerOverride;
		///<summary>UTF-8 without a byte order mark.</summary>
		private static readonly UTF8Encoding _utf8=new UTF8Encoding(false);
		private readonly string _url;

		///<summary>url is the server's address as the user gave it (RemotingClient.ServerURI), like https://server/OpenDentalServer/ServiceMain.asmx.
		///</summary>
		public OpenDentalServerHttp(string url) {
			_url=url;
		}

		///<summary>The address this instance sends to.</summary>
		public string Url {
			get {
				return _url;
			}
		}

		///<summary>For tests only: the handler every request goes through instead of the network, such as the test server's in-process handler, or
		///one that accepts a test certificate (tests/MiddleTierHostTests). Null in the program, which sends over the network. Setting it drops the
		///cached client; the handler is never disposed here.</summary>
		public static HttpMessageHandler HttpMessageHandlerOverride {
			get {
				lock(_lockHttpClient) {
					return _httpMessageHandlerOverride;
				}
			}
			set {
				lock(_lockHttpClient) {
					_httpMessageHandlerOverride=value;
					_httpClient=null;
					_keyHttpClient=null;
				}
			}
		}

		///<summary>Sends the serialized DTO and returns the server's answer, as the ASMX proxy's ProcessRequest did. Synchronous, as every Middle Tier
		///call is: SendAsync(...).GetAwaiter().GetResult(), because the test server does not accept HttpClient's synchronous Send. Throws
		///HttpRequestException when no answer comes (HttpRequestError.ConnectionError when the connection could not be made, which
		///IsConnectionFailure tells, for RemotingClient's connection-lost retry) and when the server answers with an error status (413 for a
		///request larger than its limit, with the server's text in the message), and TaskCanceledException after the timeout.</summary>
		public string ProcessRequest(string dtoString) {
			HttpClient httpClient=GetHttpClient();
			using(HttpRequestMessage httpRequestMessage=new HttpRequestMessage(HttpMethod.Post,_url)) {
				httpRequestMessage.Content=new StringContent(dtoString??"",_utf8,MEDIA_TYPE);
				using(HttpResponseMessage httpResponseMessage=httpClient.SendAsync(httpRequestMessage,HttpCompletionOption.ResponseContentRead)
					.GetAwaiter().GetResult())
				{
					string text=httpResponseMessage.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					if(!httpResponseMessage.IsSuccessStatusCode) {
						if(text.Length>ERROR_TEXT_MAX) {
							text=text.Substring(0,ERROR_TEXT_MAX)+"...";
						}
						throw new HttpRequestException(HttpRequestError.Unknown,"The Middle Tier server at "+_url+" answered "
							+(int)httpResponseMessage.StatusCode+" ("+httpResponseMessage.ReasonPhrase+")"+(text.Trim()=="" ? "." : ": "+text.Trim()),
							null,httpResponseMessage.StatusCode);
					}
					return text;
				}
			}
		}

		///<summary>True when ex says the connection to the server could not be made (refused, no route, nothing listening):
		///HttpRequestError.ConnectionError of HttpClient, as WebExceptionStatus.ConnectFailure was for the SOAP proxy. Name resolution failures,
		///proxy errors, error answers and timeouts are not (https://learn.microsoft.com/en-us/dotnet/api/system.net.http.httprequesterror).</summary>
		public static bool IsConnectionFailure(Exception ex) {
			HttpRequestException httpRequestException=ex as HttpRequestException;
			return httpRequestException!=null && httpRequestException.HttpRequestError==HttpRequestError.ConnectionError;
		}

		///<summary>The cached client, made again when the proxy settings or the test handler changed.</summary>
		private static HttpClient GetHttpClient() {
			string proxyAddress=RemotingClient.MidTierProxyAddress??"";
			string proxyUserName=RemotingClient.MidTierProxyUserName??"";
			string proxyPassword=RemotingClient.MidTierProxyPassword??"";
			string key=proxyAddress+"\n"+proxyUserName+"\n"+proxyPassword;
			lock(_lockHttpClient) {
				if(_httpClient!=null && _keyHttpClient==key) {
					return _httpClient;
				}
				HttpClient httpClient;
				if(_httpMessageHandlerOverride!=null) {
					httpClient=new HttpClient(_httpMessageHandlerOverride,false);
				}
				else {
					SocketsHttpHandler socketsHttpHandler=new SocketsHttpHandler();
					if(proxyAddress!="") {
						WebProxy webProxy=new WebProxy(proxyAddress);
						webProxy.Credentials=new NetworkCredential(proxyUserName,proxyPassword);
						socketsHttpHandler.Proxy=webProxy;
						socketsHttpHandler.UseProxy=true;
					}
					//Otherwise the system's proxy (HttpClient.DefaultProxy), as the SOAP proxy took WebRequest.DefaultWebProxy.
					//A pooled connection is made again after 15 minutes, so a server that moved to another address is found.
					socketsHttpHandler.PooledConnectionLifetime=TimeSpan.FromMinutes(15);
					//No redirects, as the SOAP proxy (SoapHttpClientProtocol) followed none: a 301 or 302 would turn the POST into a GET.
					socketsHttpHandler.AllowAutoRedirect=false;
					httpClient=new HttpClient(socketsHttpHandler,true);
				}
				httpClient.Timeout=TimeSpanTimeout;
				httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent",USER_AGENT);
				//The client before is not disposed: another thread may still be sending through it. It closes its connections when collected.
				_httpClient=httpClient;
				_keyHttpClient=key;
				return _httpClient;
			}
		}
	}
}
