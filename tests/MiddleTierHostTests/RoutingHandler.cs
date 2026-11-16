// The handler the client's HttpClient sends through in the host tests (package S3b-6 of the .NET 10 move, docs/port-notes/S3b-6.md;
// OpenDentalServerHttp.HttpMessageHandlerOverride): a request to one of the in-process servers' host names goes to that server's test server
// handler; every other request goes over the network, as the program's would (127.0.0.1, or the server URL of a run against a running server).
// One host name stands for a server that goes down and comes back, for the connection-lost retry.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace MiddleTierHostTests {
	public class RoutingHandler:HttpMessageHandler {
		///<summary>The host name of the server that goes down and comes back (RetryTests).</summary>
		public const string HOST_RETRY="s3b6-r.example.test";
		private readonly ConcurrentDictionary<string,HttpMessageInvoker> _dictHosts=new ConcurrentDictionary<string,HttpMessageInvoker>(
			StringComparer.OrdinalIgnoreCase);
		private readonly HttpMessageInvoker _httpMessageInvokerNetwork;
		///<summary>Where a request to HOST_RETRY goes while it is up.</summary>
		private HttpMessageInvoker _httpMessageInvokerRetryUp;
		private volatile bool _isRetryHostDown;

		///<summary>certificateThumbprint: the SHA-1 thumbprint of a test certificate the network requests accept besides the ones Windows trusts
		///(a run against a server URL with a test certificate); null for Windows' own validation only.</summary>
		public RoutingHandler(string certificateThumbprint) {
			SocketsHttpHandler socketsHttpHandler=new SocketsHttpHandler();
			if(!string.IsNullOrWhiteSpace(certificateThumbprint)) {
				string thumbprint=certificateThumbprint.Replace(" ","").Trim();
				socketsHttpHandler.SslOptions.RemoteCertificateValidationCallback=(sender,certificate,chain,sslPolicyErrors) =>
					sslPolicyErrors==SslPolicyErrors.None
					|| (certificate!=null && string.Equals(certificate.GetCertHashString(),thumbprint,StringComparison.OrdinalIgnoreCase));
			}
			_httpMessageInvokerNetwork=new HttpMessageInvoker(socketsHttpHandler,true);
			ClosedPortUri=new Uri("https://127.0.0.1:"+GetClosedPort()+"/");
		}

		///<summary>An address on this computer where nothing listens: a connection to it is refused.</summary>
		public Uri ClosedPortUri { get; private set; }

		///<summary>Requests to host go to handler (an in-process server's).</summary>
		public void AddHost(string host,HttpMessageHandler handler) {
			_dictHosts[host]=new HttpMessageInvoker(handler,false);
		}

		///<summary>While down, a request to HOST_RETRY goes to ClosedPortUri over the network and its connection is refused; while up, it goes to
		///handlerUp.</summary>
		public void SetRetryHost(HttpMessageHandler handlerUp,bool isDown) {
			_httpMessageInvokerRetryUp=new HttpMessageInvoker(handlerUp,false);
			_isRetryHostDown=isDown;
		}

		public bool IsRetryHostDown {
			get {
				return _isRetryHostDown;
			}
			set {
				_isRetryHostDown=value;
			}
		}

		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken cancellationToken) {
			string host=request.RequestUri.Host;
			if(string.Equals(host,HOST_RETRY,StringComparison.OrdinalIgnoreCase)) {
				if(_isRetryHostDown) {
					return _httpMessageInvokerNetwork.SendAsync(CopyTo(request,new Uri(ClosedPortUri,request.RequestUri.PathAndQuery)),cancellationToken);
				}
				return _httpMessageInvokerRetryUp.SendAsync(request,cancellationToken);
			}
			HttpMessageInvoker httpMessageInvoker;
			if(_dictHosts.TryGetValue(host,out httpMessageInvoker)) {
				return httpMessageInvoker.SendAsync(request,cancellationToken);
			}
			return _httpMessageInvokerNetwork.SendAsync(request,cancellationToken);
		}

		///<summary>The request to another address, with its content and headers.</summary>
		private static HttpRequestMessage CopyTo(HttpRequestMessage request,Uri uri) {
			HttpRequestMessage httpRequestMessage=new HttpRequestMessage(request.Method,uri);
			httpRequestMessage.Content=request.Content;
			httpRequestMessage.Version=request.Version;
			foreach(System.Collections.Generic.KeyValuePair<string,System.Collections.Generic.IEnumerable<string>> header in request.Headers) {
				httpRequestMessage.Headers.TryAddWithoutValidation(header.Key,header.Value);
			}
			return httpRequestMessage;
		}

		///<summary>A port of 127.0.0.1 that nothing listens on: one the system gave a listener that has stopped again.</summary>
		private static int GetClosedPort() {
			TcpListener tcpListener=new TcpListener(IPAddress.Loopback,0);
			tcpListener.Start();
			int port=((IPEndPoint)tcpListener.LocalEndpoint).Port;
			tcpListener.Stop();
			return port;
		}
	}
}
