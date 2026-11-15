// This file is part of the open source dental practice management software derived from Open Dental.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using ImagingDeviceManager.TwainProtocol;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ImagingDeviceManager.Tests {
	///<summary>The TW_IDENTITY feature flags against twain.h 2.5.1 (DF_DSM2 0x10000000L, DF_APP2 0x20000000L, DF_DS2 0x40000000L). DF_APP2 and
	///DF_DS2 were swapped until the S3b-0 spike (2026-11-15) found it with the real 64-bit TWAINDSM 2.5.1.</summary>
	[TestClass]
	public class TwainConstantsTests {
		[TestMethod]
		public void FeatureFlags_AreTwainHs() {
			Assert.AreEqual(0x10000000u,Tw.DF_DSM2);
			Assert.AreEqual(0x20000000u,Tw.DF_APP2);
			Assert.AreEqual(0x40000000u,Tw.DF_DS2);
		}
	}
}
