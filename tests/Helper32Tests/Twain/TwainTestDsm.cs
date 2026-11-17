// The fake TWAIN DSM the 32-bit helper loads in the S3b-8 tests (--test-twain-dsm; docs/port-notes/S3b-8.md, section 8).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Threading;
using ImagingDeviceManager.Tests;
using ImagingDeviceManager.TwainProtocol;

namespace Helper32Tests.Twain {
	///<summary>ImagingDeviceManagerTests' FakeTwainDsm with its contract 1.1 source (FakeHelperSource, ProductName SENSOR) and a scanner, set up
	///by scenario. The 32-bit helper calls CreateDsmEntry in its own process (this assembly's any-CPU copy beside it, TwainModule's TestTwainDsm),
	///so every TWAIN structure there has its x86 size; the tests call CreateFake for the same fake in their own 64-bit process. Synthetic images
	///only.</summary>
	public static class TwainTestDsm {
		///<summary>The fake sensor's ProductName (FakeHelperSource's).</summary>
		public const string SENSOR="Fake Helper Sensor";
		///<summary>The fake scanner every scenario lists too.</summary>
		public const string SCANNER_32="Fake 32-bit Scanner";
		///<summary>A 16-bit exposure (6 x 4 samples, with its 8-bit view) then an 8-bit one (9 x 5), with fixed capture ids.</summary>
		public const string SCENARIO_CAPTURE="capture";
		///<summary>The sensor waits for an exposure that never comes (FakeHelperEnd.Never).</summary>
		public const string SCENARIO_NEVER="never";
		///<summary>DAT_IDENTITY MSG_GETFIRST to the DSM never returns: the helper stops answering ListSources.</summary>
		public const string SCENARIO_HANG_LIST="hang-list";

		///<summary>For the helper: one new fake DSM per load, called like DSM_Entry.</summary>
		public static Func<IntPtr,IntPtr,uint,ushort,ushort,IntPtr,ushort> CreateDsmEntry(string scenario) {
			FakeTwainDsm fake=CreateFake(scenario);
			if(scenario==SCENARIO_HANG_LIST) {
				return (origin,dest,dg,dat,msg,data) => {
					if(dest==IntPtr.Zero && dat==Tw.DAT_IDENTITY && msg==Tw.MSG_GETFIRST) {
						Thread.Sleep(Timeout.Infinite);
					}
					return fake.Entry(origin,dest,dg,dat,msg,data);
				};
			}
			return fake.Entry;
		}

		internal static FakeTwainDsm CreateFake(string scenario) {
			FakeTwainDsm fake=new FakeTwainDsm();
			fake.ListSourceNames=new List<string> { SCANNER_32 };
			FakeHelperSource helper=new FakeHelperSource();
			helper.ListExposures.Clear();
			switch(scenario) {
				case SCENARIO_CAPTURE:
					helper.ListExposures.Add(Exposure16(6,4,"cid-16"));
					helper.ListExposures.Add(Exposure8(9,"cid-8"));
					break;
				case SCENARIO_NEVER:
					helper.End=FakeHelperEnd.Never;
					break;
			}
			fake.AddHelperSource(helper);
			return fake;
		}

		///<summary>A sample of the 16-bit exposure, as TwainCapturePathTests.Sample16.</summary>
		public static int Sample16(int x,int y) {
			return (x*4099+y*257+0x0101)&0xFFFF;
		}

		private static FakePage Exposure16(int width,int height,string captureId) {
			FakePage page=FakePage.CreateGray8(width,height,FakeHelperSource.Dpi,(x,y) => (byte)(x*11+y));
			page.Original16=FakePage.CreateGray16(width,height,FakeHelperSource.Dpi,(x,y) => Sample16(x,y));
			page.CaptureId=captureId;
			page.Original16.CaptureId=captureId;
			return page;
		}

		private static FakePage Exposure8(int width,string captureId) {
			FakePage page=FakePage.CreateGray8(width,5,FakeHelperSource.Dpi,(x,y) => (byte)(x*7+y));
			page.CaptureId=captureId;
			return page;
		}
	}
}
