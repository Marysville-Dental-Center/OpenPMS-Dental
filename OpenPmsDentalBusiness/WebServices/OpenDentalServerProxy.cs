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

		public static IOpenDentalServer GetOpenDentalServerInstance() {
			if(MockOpenDentalServerCur!=null) {
				return MockOpenDentalServerCur;
			}
#if !NETFRAMEWORK
			//The ASMX client proxy (Web References\OpenDentalServer) needs System.Web.Services, which .NET 10 does not have. Its HttpClient
			//replacement is package S3b-6 of the .NET 10 plan; until it is merged, the .NET 10 build cannot reach a Middle Tier server.
			throw new NotSupportedException(MiddleTierClientNotPortedMessage);
#else
			OpenDentalServerReal service=new OpenDentalServerReal();
			service.Url=RemotingClient.ServerURI;
			if(RemotingClient.MidTierProxyAddress!=null && RemotingClient.MidTierProxyAddress!="") {
				IWebProxy proxy=new WebProxy(RemotingClient.MidTierProxyAddress);
				ICredentials cred=new NetworkCredential(RemotingClient.MidTierProxyUserName,RemotingClient.MidTierProxyPassword);
				proxy.Credentials=cred;
				service.Proxy=proxy;
			}
			//5/12/2020 - Cameron made the following change to the service timeout so long processes could finish using the middle tier.  This was
			//discussed with Jordan, Nathan, and Allen and was reviewed by Jason.  Changes were also made in the OpenPmsDentalServer/Web.config file to
			//increase the ASP.NET maxRequestLength and executionTimeout as well as the IIS maxAllowedContentLength.  The max request lengths were
			//increased to 1 GB and the timeout was increased to 1 hour here and in the Web.config file.
			service.Timeout=3600000;//default timeout is 100000 ms (1 min 40 sec), increased so longer running processes don't timeout
			//The default useragent is
			//Mozilla/4.0 (compatible; MSIE 6.0; MS Web Services Client Protocol 4.0.30319.296)
			//But DHS firewall doesn't allow that.  MSIE 6.0 is probably too old, and their firewall also looks for IE8Mercury.
			service.UserAgent="Mozilla/4.0 (compatible; MSIE 7.0; MS Web Services Client Protocol 4.0.30319.296; IE8Mercury)";
			return service;
#endif
		}

		///<summary>The message of the .NET 10 build when it is asked to reach a Middle Tier server before package S3b-6 ports the client.</summary>
		public const string MiddleTierClientNotPortedMessage="The Middle Tier client is not available yet in the .NET 10 build of this program: "
			+"its port to HttpClient is package S3b-6 of the .NET 10 plan (docs/specs/net10-plan.md). Connect directly to the database instead.";

	}
}
