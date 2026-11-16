using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPmsDentalBusiness {
#if NETFRAMEWORK
	///<summary>This is a helper class that allows the real OpenDentalServer.ServiceMain class implement IOpenDentalServer.
	///This also gives us a place to add code in the future if we ever need to add anything to OpenDentalServer.ServiceMain.</summary>
	public class OpenDentalServerReal:OpenPmsDentalBusiness.OpenDentalServer.ServiceMain, IOpenDentalServer {
	}
#endif
	//The ASMX proxy it derives from exists on .NET Framework only; S3b-6 of the .NET 10 plan replaces both (OpenDentalServerProxy).
}
