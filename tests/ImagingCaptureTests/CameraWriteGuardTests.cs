// This file is part of the open source dental practice management software derived from Open Dental.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ImagingCapture.Tests {
	///<summary>Design 8.7 (decision 13 of 2026-11-26): the camera is written in two places only, IAMStreamConfig::SetFormat for the stream
	///format and CameraSettingWriter.Write for the camera settings (IAMCameraControl::Set, IAMVideoProcAmp::Set and IKsControl SET inside
	///DirectShowFrameSource). These tests read the ImagingCapture source and the Video window that runs the camera (comments removed) and fail
	///if code calls a vtable placeholder, writes from anywhere else, or opens the driver's property pages.</summary>
	[TestClass]
	public class CameraWriteGuardTests {
		private static List<Tuple<string,string>> _listSources;
		///<summary>Files outside ImagingCapture that run the camera, from the repository root, scanned when present: the Video window
		///(design 8.7: it has no button for the camera's property pages).</summary>
		private static readonly string[] _arrayFilesOutside={ @"OpenPmsDental\Forms\FormVideo.cs",@"OpenPmsDental\Forms\FormVideo.designer.cs" };

		///<summary>(path, code without comments) for every .cs file of ImagingCapture outside bin and obj, with the path relative to
		///ImagingCapture, and for each of _arrayFilesOutside that exists, with the path relative to the repository root.</summary>
		private static List<Tuple<string,string>> GetSources() {
			if(_listSources!=null) {
				return _listSources;
			}
			DirectoryInfo directoryInfo=new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
			string folderProject=null;
			while(directoryInfo!=null) {
				string folder=Path.Combine(directoryInfo.FullName,"ImagingCapture");
				if(File.Exists(Path.Combine(folder,"xImagingCapture.csproj"))) {
					folderProject=folder;
					break;
				}
				directoryInfo=directoryInfo.Parent;
			}
			if(folderProject==null) {
				Assert.Inconclusive("The ImagingCapture source folder was not found above "+AppDomain.CurrentDomain.BaseDirectory);
			}
			_listSources=new List<Tuple<string,string>>();
			foreach(string path in Directory.GetFiles(folderProject,"*.cs",SearchOption.AllDirectories)) {
				string pathRelative=path.Substring(folderProject.Length+1);
				if(pathRelative.StartsWith(@"bin\",StringComparison.OrdinalIgnoreCase) || pathRelative.StartsWith(@"obj\",StringComparison.OrdinalIgnoreCase)) {
					continue;
				}
				_listSources.Add(Tuple.Create(pathRelative,StripComments(File.ReadAllText(path))));
			}
			Assert.IsTrue(_listSources.Count>10,"Too few source files found in "+folderProject);
			string root=Path.GetDirectoryName(folderProject);
			foreach(string fileOutside in _arrayFilesOutside) {
				string path=Path.Combine(root,fileOutside);
				if(File.Exists(path)) {
					_listSources.Add(Tuple.Create(fileOutside,StripComments(File.ReadAllText(path))));
				}
			}
			return _listSources;
		}

		///<summary>Removes /* */ and // comments. The scanned files have no string literal that contains either marker.</summary>
		private static string StripComments(string code) {
			code=Regex.Replace(code,@"/\*.*?\*/","",RegexOptions.Singleline);
			return Regex.Replace(code,@"//[^\r\n]*","");
		}

		private static List<string> FindMatches(string pattern) {
			List<string> listMatches=new List<string>();
			foreach(Tuple<string,string> tuple in GetSources()) {
				foreach(Match match in Regex.Matches(tuple.Item2,pattern)) {
					listMatches.Add(tuple.Item1+": "+match.Value);
				}
			}
			return listMatches;
		}

		[TestMethod]
		public void NoVtablePlaceholderIsCalled() {
			//Declarations look like "int Unused_Set();". A call is a member access: ".Unused_".
			List<string> listCalls=FindMatches(@"\.\s*Unused_\w+");
			Assert.AreEqual(0,listCalls.Count,string.Join("; ",listCalls));
			Assert.AreEqual(0,FindMatches(@"\bUnused_Set\b").Count,"IAMCameraControl's Set is declared for real (decision 13 of 2026-11-26).");
		}

		[TestMethod]
		public void SetFormatIsCalledFromOnePlace() {
			List<string> listCalls=FindMatches(@"\.\s*SetFormat\s*\(");
			Assert.AreEqual(1,listCalls.Count,string.Join("; ",listCalls));
			StringAssert.StartsWith(listCalls[0],@"Uvc\DirectShow\DirectShowFrameSource.cs");
		}

		[TestMethod]
		public void CameraControlWrites_OnePath() {
			//Decision 13 of 2026-11-26: every camera setting the driver offers can be written, through one logged path. The driver's own
			//property pages and the other write interfaces stay out.
			string[] arrayDenied={ "IAMVideoControl","ISpecifyPropertyPages","OleCreatePropertyFrame","IKsPropertySet" };
			foreach(string denied in arrayDenied) {
				List<string> listMatches=FindMatches(Regex.Escape(denied));
				Assert.AreEqual(0,listMatches.Count,denied+": "+string.Join("; ",listMatches));
			}
			List<string> listControlSets=FindMatches(@"_(cameraControl|videoProcAmp)\s*\.\s*Set\b");
			Assert.AreEqual(2,listControlSets.Count,string.Join("; ",listControlSets));
			Assert.IsTrue(listControlSets.All(x => x.StartsWith(@"Uvc\DirectShow\DirectShowFrameSource.cs")),string.Join("; ",listControlSets));
			List<string> listKsCalls=FindMatches(@"_ksControl\s*\.\s*KsProperty\b");
			Assert.AreEqual(1,listKsCalls.Count,"IKsControl is called from KsCall only: "+string.Join("; ",listKsCalls));
			List<string> listKsSets=FindMatches(@"KSPROPERTY_TYPE_SET\b");
			Assert.IsTrue(listKsSets.All(x => x.StartsWith(@"Uvc\DirectShow\DirectShowFrameSource.cs") || x.StartsWith(@"Uvc\Interop\KsControlProperty.cs")),
				string.Join("; ",listKsSets));
			Assert.AreEqual(0,FindMatches(@"\bWriteLightControl\b").Count,"Replaced by WriteControl.");
			List<string> listWrites=FindMatches(@"\.\s*WriteControl\s*\(");
			Assert.AreEqual(1,listWrites.Count,"WriteControl is called from CameraSettingWriter.Write only: "+string.Join("; ",listWrites));
			StringAssert.StartsWith(listWrites[0],@"Video\CameraSettingWriter.cs");
		}
	}
}
