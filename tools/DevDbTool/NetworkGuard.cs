// Licensed under the GNU General Public License, version 2. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Reflection;

namespace DevDbTool {
	///<summary>Blocks and records outbound HTTP(S) traffic made through the .NET web stack (WebRequest, SOAP proxies, WebClient and
	///HttpClient, which fall back to WebRequest.DefaultWebProxy on .NET Framework and to HttpClient.DefaultProxy on .NET 10; Install sets
	///both). Building a dev database must never contact Open Dental's servers or anyone else's, so every request is routed to a dead local
	///proxy and its destination is reported at the end of the run. The MySQL connection uses raw sockets and is unaffected.</summary>
	public static class NetworkGuard {
		private static readonly object _lock=new object();
		private static readonly List<string> _listBlocked=new List<string>();

		///<summary>Destinations of every outbound request that was attempted and blocked, in order.</summary>
		public static List<string> ListBlocked {
			get {
				lock(_lock) {
					return new List<string>(_listBlocked);
				}
			}
		}

		public static void Install() {
			BlockingProxy blockingProxy=new BlockingProxy();
			WebRequest.DefaultWebProxy=blockingProxy;
			SetHttpClientDefaultProxy(blockingProxy);
		}

		///<summary>Sets HttpClient.DefaultProxy, the proxy of every HttpClient that has none of its own, when the runtime has it (.NET Core 3.0
		///and later): there HttpClient no longer falls back to WebRequest.DefaultWebProxy, so the guard sets both. .NET Framework has no such
		///property, and its HttpClient goes through WebRequest.DefaultWebProxy; the property is found by reflection, so the same code compiles
		///and runs on both (docs/port-notes/S3a-9.md, section 3). True when it was set.</summary>
		public static bool SetHttpClientDefaultProxy(IWebProxy webProxy) {
			PropertyInfo propertyInfo=typeof(HttpClient).GetProperty("DefaultProxy",BindingFlags.Public|BindingFlags.Static);
			if(propertyInfo==null || !propertyInfo.CanWrite) {
				return false;
			}
			propertyInfo.SetValue(null,webProxy);
			return true;
		}

		private static void Record(Uri uri) {
			string where=uri?.GetLeftPart(UriPartial.Path)??"(unknown)";
			lock(_lock) {
				_listBlocked.Add(where);
			}
			Console.Error.WriteLine("  [network guard] blocked outbound request to "+where);
		}

		///<summary>Sends every request to a local port nothing listens on, so it fails fast without leaving the machine.</summary>
		private class BlockingProxy : IWebProxy {
			private static readonly Uri _uriDeadEnd=new Uri("http://127.0.0.1:9/");

			public ICredentials Credentials { get; set; }

			public Uri GetProxy(Uri destination) {
				Record(destination);
				return _uriDeadEnd;
			}

			public bool IsBypassed(Uri host) {
				return false;
			}
		}
	}
}
