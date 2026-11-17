// Licensed under the GNU General Public License, version 2. See LICENSE in the repository root.
using System;
using System.Net;
using System.Net.Http;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SyntheticData.Infrastructure;

namespace SyntheticData.Tests {
	///<summary>The tool's network guard (S3a-9 (iii), docs/port-notes/S3a-9.md): Install routes WebRequest and HttpClient to the dead local
	///proxy and records each destination. On .NET Framework HttpClient goes through WebRequest.DefaultWebProxy; on .NET 10 it goes through
	///HttpClient.DefaultProxy, which Install sets too where the runtime has it. Each test puts both proxies back. Nothing leaves the machine:
	///the only connection is to 127.0.0.1:9, where nothing listens, and the destination's name (*.example.test) is never looked up.</summary>
	[TestClass]
	public class NetworkGuardTests {
		private const string URL_BLOCKED="http://blocked.example.test/synthetic";
		private IWebProxy _webProxyOld;
		private PropertyInfo _propertyInfoDefaultProxy;
		private IWebProxy _webProxyHttpClientOld;

		[TestInitialize]
		public void SetUp() {
			_webProxyOld=WebRequest.DefaultWebProxy;
			_propertyInfoDefaultProxy=typeof(HttpClient).GetProperty("DefaultProxy",BindingFlags.Public|BindingFlags.Static);
			if(_propertyInfoDefaultProxy!=null) {
				_webProxyHttpClientOld=(IWebProxy)_propertyInfoDefaultProxy.GetValue(null);
			}
		}

		[TestCleanup]
		public void CleanUp() {
			WebRequest.DefaultWebProxy=_webProxyOld;
			if(_propertyInfoDefaultProxy!=null) {
				_propertyInfoDefaultProxy.SetValue(null,_webProxyHttpClientOld);
			}
		}

		[TestMethod]
		public void Install_SetsBothDefaultProxies_WhereTheRuntimeHasThem() {
			NetworkGuard.Install();
			IWebProxy webProxy=WebRequest.DefaultWebProxy;
			Assert.AreNotSame(_webProxyOld,webProxy,"WebRequest.DefaultWebProxy is the guard's proxy.");
			int countBefore=NetworkGuard.ListBlocked.Count;
			Assert.AreEqual(new Uri("http://127.0.0.1:9/"),webProxy.GetProxy(new Uri(URL_BLOCKED)),"The dead local proxy.");
			Assert.IsFalse(webProxy.IsBypassed(new Uri(URL_BLOCKED)),"No host is bypassed.");
			Assert.AreEqual(countBefore+1,NetworkGuard.ListBlocked.Count);
			Assert.AreEqual(URL_BLOCKED,NetworkGuard.ListBlocked[countBefore],"The destination is recorded.");
			if(_propertyInfoDefaultProxy==null) {
				//.NET Framework: HttpClient has no DefaultProxy and uses WebRequest.DefaultWebProxy (HttpClient_IsBlocked shows it).
				Assert.IsFalse(NetworkGuard.SetHttpClientDefaultProxy(webProxy),"Nothing to set on this runtime.");
			}
			else {
				Assert.AreSame(webProxy,_propertyInfoDefaultProxy.GetValue(null),"HttpClient.DefaultProxy is the guard's proxy too.");
			}
		}

		[TestMethod]
		public void HttpClient_IsBlocked_AndItsDestinationRecorded() {
			NetworkGuard.Install();
			int countBefore=NetworkGuard.ListBlocked.Count;
			Exception exception=null;
			bool isSuccess=false;
			using(HttpClient httpClient=new HttpClient()) {
				httpClient.Timeout=TimeSpan.FromSeconds(30);
				try {
					isSuccess=httpClient.GetAsync(URL_BLOCKED).GetAwaiter().GetResult().IsSuccessStatusCode;
				}
				catch(Exception ex) {
					exception=ex;
				}
			}
			Assert.IsFalse(isSuccess,"The request must not succeed.");
			Assert.IsTrue(NetworkGuard.ListBlocked.Count>countBefore,"The guard saw the request: "+exception);
			Assert.AreEqual(URL_BLOCKED,NetworkGuard.ListBlocked[countBefore]);
		}
	}
}
