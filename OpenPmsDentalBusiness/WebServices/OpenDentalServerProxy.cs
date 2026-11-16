using CodeBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OpenPmsDentalBusiness.WebServices {
	public class OpenDentalServerProxy {
		public static OpenDentalServerMockIIS MockOpenDentalServerCur {
			get;
			set;
		}

		///<summary>The mock server when a test set one; otherwise the HTTP client for this thread's server address (RemotingClient.ServerURI).
		///The client, OpenDentalServerHttp, replaced the ASMX proxy in package S3b-6 of the .NET 10 move: it shares one HttpClient with the old
		///proxy's settings (the proxy of MiddleTierProxyConfig.xml, a timeout of one hour, the User-Agent).</summary>
		public static IOpenDentalServer GetOpenDentalServerInstance() {
			if(MockOpenDentalServerCur!=null) {
				return MockOpenDentalServerCur;
			}
			//5/12/2020 - Cameron made the following change to the service timeout so long processes could finish using the middle tier.  This was
			//discussed with Jordan, Nathan, and Allen and was reviewed by Jason.  Changes were also made in the server's configuration to
			//increase the maxRequestLength and executionTimeout as well as the IIS maxAllowedContentLength.  The max request lengths were
			//increased to 1 GB and the timeout was increased to 1 hour.
			//(S3b-6: OpenDentalServerHttp keeps the hour; the server, OpenPmsDentalServer, keeps the 1 GiB limit and has no execution timeout.)
			return new OpenDentalServerHttp(RemotingClient.ServerURI);
		}

	}
}
