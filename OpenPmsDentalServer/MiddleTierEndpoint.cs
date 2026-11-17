// The Middle Tier's one endpoint (package S3b-6 of the .NET 10 move, docs/port-notes/S3b-6.md), in place of the ASMX web method
// ServiceMain.ProcessRequest(string dtoString): a POST to {PathBase}/ServiceMain.asmx or {PathBase}/ whose body is the serialized DTO as
// UTF-8 text, answered with DtoProcessor.ProcessDto's result as UTF-8 text. No SOAP envelope: the client (OpenDentalServerHttp) sends the
// same DTO string the SOAP proxy put into its envelope, and reads the same result string.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.IO;
using System.Net;
using System.Runtime.Loader;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace OpenPmsDentalServer {
	///<summary>Handles every request of the server (ServerHost.Configure gives it the whole pipeline). HTTPS only; one path; POST only; the body
	///limit of ServerSettings; then ProcessDto, synchronously, on a thread of the request's own.</summary>
	public class MiddleTierEndpoint {
		///<summary>The page name of the ASMX web service, which every workstation's saved address ends with.</summary>
		public const string PAGE_NAME="/ServiceMain.asmx";
		///<summary>The content type of the answer: the result string as UTF-8 text (an XML document, a number, True or False, or "0").</summary>
		public const string CONTENT_TYPE="text/plain; charset=utf-8";
		///<summary>UTF-8 without a byte order mark, and with replacement characters for invalid bytes, as StreamReader read a body before.</summary>
		private static readonly UTF8Encoding _utf8=new UTF8Encoding(false,false);
		private readonly ServerSettings _serverSettings;
		///<summary>The application's folder with a trailing separator, as HostingEnvironment.ApplicationPhysicalPath gave it: where
		///OpenDentalServerConfig.xml (or {application name}Config.xml) is, and the Logging folder of verbose logging.</summary>
		private readonly string _contentRoot;

		public MiddleTierEndpoint(ServerSettings serverSettings,string contentRoot) {
			_serverSettings=serverSettings;
			_contentRoot=contentRoot.TrimEnd('\\','/')+Path.DirectorySeparatorChar;
		}

		///<summary>The content root the endpoint gives ProcessDto (with its trailing separator).</summary>
		public string ContentRoot {
			get {
				return _contentRoot;
			}
		}

		///<summary>The RequestDelegate of the server.</summary>
		public async Task HandleAsync(HttpContext context) {
			HttpRequest request=context.Request;
			//HTTPS only: Kestrel listens on HTTPS endpoints only (ServerHost.GetHttpsProblems); this also refuses a plain HTTP binding of IIS.
			if(!request.IsHttps) {
				await WriteTextAsync(context,StatusCodes.Status403Forbidden,"The Middle Tier server accepts HTTPS requests only.");
				return;
			}
			string applicationName;
			if(!TryGetApplicationName(request,out applicationName)) {
				await WriteTextAsync(context,StatusCodes.Status404NotFound,"Not found. The Middle Tier's address ends with "
					+(_serverSettings.PathBase.HasValue ? _serverSettings.PathBase.Value : "")+PAGE_NAME+".");
				return;
			}
			if(!HttpMethods.IsPost(request.Method)) {
				context.Response.Headers["Allow"]="POST";
				await WriteTextAsync(context,StatusCodes.Status405MethodNotAllowed,"The Middle Tier server accepts POST requests only.");
				return;
			}
			long maxRequestBodySize=_serverSettings.MaxRequestBodySize;
			//Kestrel and IIS stop reading at this size themselves (their BadHttpRequestException below); the test server has no such limit, so the
			//body is counted here too.
			IHttpMaxRequestBodySizeFeature httpMaxRequestBodySizeFeature=context.Features.Get<IHttpMaxRequestBodySizeFeature>();
			if(httpMaxRequestBodySizeFeature!=null && !httpMaxRequestBodySizeFeature.IsReadOnly) {
				httpMaxRequestBodySizeFeature.MaxRequestBodySize=maxRequestBodySize;
			}
			string dtoString;
			try {
				if(request.ContentLength!=null && request.ContentLength.Value>maxRequestBodySize) {
					throw new RequestTooLargeException();
				}
				dtoString=await ReadBodyAsync(request.Body,maxRequestBodySize,context.RequestAborted);
			}
			catch(RequestTooLargeException) {
				await WriteTooLargeAsync(context,maxRequestBodySize);
				return;
			}
			catch(BadHttpRequestException ex) when(ex.StatusCode==StatusCodes.Status413PayloadTooLarge) {
				await WriteTooLargeAsync(context,maxRequestBodySize);
				return;
			}
			string result;
			try {
				result=await ProcessOnOwnThreadAsync(dtoString,applicationName,GetClientIp(context));
			}
			catch(Exception ex) {
				//ProcessDto answers every error of the business layer as a DtoException; this is only for what fails around it.
				await WriteTextAsync(context,StatusCodes.Status500InternalServerError,"The Middle Tier server could not process the request: "+ex.Message);
				return;
			}
			context.Response.StatusCode=StatusCodes.Status200OK;
			context.Response.ContentType=CONTENT_TYPE;
			await context.Response.WriteAsync(result??"",_utf8,context.RequestAborted);
		}

		///<summary>ProcessDto on a new thread of this request's own, synchronously from its start to its end: the business layer keeps a request's
		///state in thread-static fields (the ServerMT role, Security.CurUser, PasswordTyped and CurComputerName, the host context of
		///MiddleTierHost), so a request must run on one thread, and a new thread starts with none of another request's values. The await does not
		///hold a thread-pool thread while a long request runs.</summary>
		private Task<string> ProcessOnOwnThreadAsync(string dtoString,string applicationName,string clientIp) {
			TaskCompletionSource<string> taskCompletionSource=new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
			Thread thread=new Thread(() => {
				try {
					taskCompletionSource.SetResult(ProcessRequest(dtoString,applicationName,clientIp));
				}
				catch(Exception ex) {
					taskCompletionSource.SetException(ex);
				}
			});
			thread.Name="MiddleTierRequest";
			thread.IsBackground=true;
			thread.Start();
			return taskCompletionSource.Task;
		}

		///<summary>One request on the calling thread: the ServerMT role for this thread, then ProcessDto's host form, which sets the request's host
		///context (MiddleTierHost) and clears it when the request ends. Reflection that names an assembly by its name (DtoProcessor's
		///Type.GetType of the method's class, the XmlSerializer's generated serializers, plug-ins) resolves in the context this server's
		///assemblies were loaded in: the default one in the server's own process, an isolated one in the host tests.</summary>
		public string ProcessRequest(string dtoString,string applicationName,string clientIp) {
			using(AssemblyLoadContext.EnterContextualReflection(typeof(MiddleTierEndpoint).Assembly)) {
				OpenPmsDentalBusiness.RemotingClient.SetRemotingRoleT(OpenPmsDentalBusiness.MiddleTierRole.ServerMT);
				return OpenPmsDentalBusiness.DtoProcessor.ProcessDto(dtoString,_contentRoot,applicationName,clientIp);
			}
		}

		///<summary>The application name of the request, as HostingEnvironment.ApplicationVirtualPath gave it (/OpenDentalServer, or / at the site
		///root), when the request's path is the endpoint's: {PathBase}/ServiceMain.asmx, {PathBase}/ or {PathBase}. IIS gives the application's
		///virtual path as the request's PathBase (empty at a site root) and the setting is empty there (ServerSettings.Read); on Kestrel and the
		///test server the configured PathBase is matched here.</summary>
		private bool TryGetApplicationName(HttpRequest request,out string applicationName) {
			applicationName=null;
			PathString pathBase=request.PathBase;
			PathString path=request.Path;
			if(!pathBase.HasValue && _serverSettings.PathBase.HasValue) {
				PathString pathRest;
				if(!path.StartsWithSegments(_serverSettings.PathBase,StringComparison.OrdinalIgnoreCase,out pathRest)) {
					return false;
				}
				pathBase=_serverSettings.PathBase;
				path=pathRest;
			}
			string rest=path.HasValue ? path.Value : "";
			if(rest!="" && rest!="/" && !rest.Equals(PAGE_NAME,StringComparison.OrdinalIgnoreCase)) {
				return false;
			}
			applicationName=pathBase.HasValue ? pathBase.Value : "/";
			return true;
		}

		///<summary>The client's address as Request.UserHostAddress gave it: an IPv4 address in its own form, also when a dual-stack socket gives it
		///mapped to IPv6. Null when the server has none (the test server).</summary>
		private static string GetClientIp(HttpContext context) {
			IPAddress ipAddress=context.Connection.RemoteIpAddress;
			if(ipAddress==null) {
				return null;
			}
			if(ipAddress.IsIPv4MappedToIPv6) {
				ipAddress=ipAddress.MapToIPv4();
			}
			return ipAddress.ToString();
		}

		///<summary>The whole body as text, at most maxBytes bytes (RequestTooLargeException beyond them). A UTF-8 byte order mark is dropped.</summary>
		private static async Task<string> ReadBodyAsync(Stream stream,long maxBytes,CancellationToken cancellationToken) {
			using(MemoryStream memoryStream=new MemoryStream()) {
				byte[] buffer=new byte[81920];
				long total=0;
				while(true) {
					int count=await stream.ReadAsync(buffer,0,buffer.Length,cancellationToken);
					if(count==0) {
						break;
					}
					total+=count;
					if(total>maxBytes) {
						throw new RequestTooLargeException();
					}
					memoryStream.Write(buffer,0,count);
				}
				byte[] arrayBytes=memoryStream.GetBuffer();
				int length=(int)memoryStream.Length;
				int start=0;
				if(length>=3 && arrayBytes[0]==0xEF && arrayBytes[1]==0xBB && arrayBytes[2]==0xBF) {
					start=3;
				}
				return _utf8.GetString(arrayBytes,start,length-start);
			}
		}

		private static Task WriteTooLargeAsync(HttpContext context,long maxRequestBodySize) {
			return WriteTextAsync(context,StatusCodes.Status413PayloadTooLarge,"The request is larger than the Middle Tier server accepts ("
				+maxRequestBodySize+" bytes, "+ServerSettings.SECTION+":MaxRequestBodySize).");
		}

		private static async Task WriteTextAsync(HttpContext context,int statusCode,string text) {
			context.Response.StatusCode=statusCode;
			context.Response.ContentType=CONTENT_TYPE;
			await context.Response.WriteAsync(text,_utf8);
		}

		///<summary>The body is larger than the server's limit.</summary>
		private class RequestTooLargeException:Exception {
		}
	}
}
