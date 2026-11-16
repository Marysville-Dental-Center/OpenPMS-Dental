// This file is part of the open source dental practice management software derived from Open Dental.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Runtime.Serialization;

namespace ImagingDeviceManager {
	///<summary>Thrown for imaging device failures that callers should report to the user.</summary>
	[Serializable]
	public class ImagingDeviceException:Exception {
		///<summary>The EZTwain-style error code associated with the failure, or EZTEC_NONE.</summary>
		public EZTwainErrorCode ErrorCode { get; }

		public ImagingDeviceException() {
		}

		public ImagingDeviceException(string message):base(message) {
		}

		public ImagingDeviceException(string message,Exception innerException):base(message,innerException) {
		}

		public ImagingDeviceException(string message,EZTwainErrorCode errorCode):base(message) {
			ErrorCode=errorCode;
		}

		///<summary>Formatter-based serialization does not exist on .NET, where the base members are obsolete (SYSLIB0051); kept, marked as the
		///base marks them.</summary>
		[Obsolete("Formatter-based serialization is obsolete and not supported on .NET.",DiagnosticId="SYSLIB0051",UrlFormat="https://aka.ms/dotnet-warnings/{0}")]
		protected ImagingDeviceException(SerializationInfo info,StreamingContext context):base(info,context) {
			ErrorCode=(EZTwainErrorCode)info.GetInt32(nameof(ErrorCode));
		}

		[Obsolete("Formatter-based serialization is obsolete and not supported on .NET.",DiagnosticId="SYSLIB0051",UrlFormat="https://aka.ms/dotnet-warnings/{0}")]
		public override void GetObjectData(SerializationInfo info,StreamingContext context) {
			base.GetObjectData(info,context);
			info.AddValue(nameof(ErrorCode),(int)ErrorCode);
		}
	}
}
