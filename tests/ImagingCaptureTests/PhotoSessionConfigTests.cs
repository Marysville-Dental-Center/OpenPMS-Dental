// IMG-P4e: the camera session's settings of this computer in capture.ini ([Photo] and [Video]): the design's defaults, writing and reading back,
// the limits, and the Setup log's description of a change. A capture.ini in a temporary folder. Licensed under the GNU General Public License,
// version 2 or later. See LICENSE in the repository root.
using System;
using System.IO;
using ImagingCapture.Photo;
using ImagingDeviceManager.Config;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ImagingCapture.Tests {
	[TestClass]
	public class PhotoSessionConfigTests {
		private string _folder;

		[TestInitialize]
		public void Init() {
			_folder=Path.Combine(Path.GetTempPath(),"opms-photo-config-"+Guid.NewGuid().ToString("N"));
			Directory.CreateDirectory(_folder);
		}

		[TestCleanup]
		public void Cleanup() {
			Directory.Delete(_folder,true);
		}

		[TestMethod]
		public void Defaults_AsTheDesign() {
			PhotoSessionConfig config=PhotoSessionConfig.Read(LocalCaptureConfig.Load(Path.Combine(_folder,"capture.ini")));
			Assert.IsFalse(config.IsFreezeFirst,"Decision C2: save at once.");
			Assert.IsTrue(config.IsClickOn);
			Assert.AreEqual(10,config.MinutesIdle);
			Assert.AreEqual(120,config.VideoLimitSeconds,"Decision C5: 2 minutes.");
			Assert.AreEqual(120,PhotoSessionConfig.Read(null).VideoLimitSeconds);
		}

		[TestMethod]
		public void Write_ReadBack_OtherSectionsKept_LimitsClamped() {
			string path=Path.Combine(_folder,"capture.ini");
			File.WriteAllText(path,"[Camera]\r\nName=USB camera\r\n");
			LocalCaptureConfig localCaptureConfig=LocalCaptureConfig.Load(path);
			PhotoSessionConfig config=new PhotoSessionConfig { IsFreezeFirst=true,IsClickOn=false,MinutesIdle=5,VideoLimitSeconds=90 };
			Assert.IsTrue(config.Write(localCaptureConfig,out string error),error);
			PhotoSessionConfig configRead=PhotoSessionConfig.Read(LocalCaptureConfig.Load(path));
			Assert.IsTrue(configRead.IsFreezeFirst);
			Assert.IsFalse(configRead.IsClickOn);
			Assert.AreEqual(5,configRead.MinutesIdle);
			Assert.AreEqual(90,configRead.VideoLimitSeconds);
			Assert.AreEqual("USB camera",LocalCaptureConfig.Load(path).GetString("Camera","Name"),"The camera's own settings stay.");
			File.AppendAllText(path,"");
			LocalCaptureConfig localCaptureConfigWild=LocalCaptureConfig.Load(path);
			localCaptureConfigWild.Set(PhotoSessionConfig.SectionVideo,PhotoSessionConfig.KeyLimitSeconds,"99999");
			localCaptureConfigWild.Set(PhotoSessionConfig.SectionPhoto,PhotoSessionConfig.KeyIdleMinutes,"0");
			Assert.AreEqual(600,PhotoSessionConfig.Read(localCaptureConfigWild).VideoLimitSeconds,"At most 10 minutes.");
			Assert.AreEqual(1,PhotoSessionConfig.Read(localCaptureConfigWild).MinutesIdle,"At least a minute.");
		}

		[TestMethod]
		public void DescribeChanges_ForTheSetupLog() {
			PhotoSessionConfig configOld=new PhotoSessionConfig();
			PhotoSessionConfig configNew=configOld.Copy();
			Assert.AreEqual("",PhotoSessionConfig.DescribeChanges(configOld,configNew));
			configNew.IsFreezeFirst=true;
			configNew.VideoLimitSeconds=90;
			Assert.AreEqual("Freeze first: off to on; Video limit: 120 to 90 s",PhotoSessionConfig.DescribeChanges(configOld,configNew));
			Assert.IsFalse(configOld.IsFreezeFirst,"Copy is a copy.");
		}
	}
}
