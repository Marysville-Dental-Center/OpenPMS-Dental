// This file is part of the open source dental practice management software derived from Open Dental.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using ImagingDeviceManager.Config;
using ImagingDeviceManager.Journal;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ImagingDeviceManager.Tests {
	///<summary>IMG-ACL: the folders that hold patient images not yet filed get an access list of their own. The "Real ACL" tests change
	///the access list only of folders they create in their own temporary folder, and give access to this Windows user instead of a group,
	///so the folder can still be removed. Nothing here touches ProgramData, a real group, or any other folder.</summary>
	[TestClass]
	public class ImagingFolderAclTests {
		private string _tempDir;
		private static readonly SecurityIdentifier _sidSystem=new SecurityIdentifier(WellKnownSidType.LocalSystemSid,null);
		private static readonly SecurityIdentifier _sidAdministrators=new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid,null);
		private static readonly SecurityIdentifier _sidUsers=new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid,null);
		private static readonly SecurityIdentifier _sidMe=WindowsIdentity.GetCurrent().User;

		[TestInitialize]
		public void SetUp() {
			_tempDir=Path.Combine(Path.GetTempPath(),"ImagingFolderAclTests_"+Guid.NewGuid().ToString("N"));
			Directory.CreateDirectory(_tempDir);
		}

		[TestCleanup]
		public void TearDown() {
			try {
				Directory.Delete(_tempDir,true);
			}
			catch(IOException) { }
			catch(UnauthorizedAccessException) { }
		}

		#region Which folders
		[TestMethod]
		public void PrivateRoots_AreTheUnsavedPhotos_TheRecordings_AndTheCaptureFolders() {
			CollectionAssert.AreEqual(new[] { @"Photos\unsaved",@"Video\recording","Capture" },ImagingFolderAcl.ListPrivateRoots.ToArray());
			Assert.AreEqual(CaptureFolders.SubfolderCapture,ImagingFolderAcl.GetPrivateRoot(CaptureFolders.SubfolderCapture));
			foreach(string subfolder in new[] { CaptureFolders.SubfolderJournal,CaptureFolders.SubfolderQuarantine,
				CaptureFolders.SubfolderQuarantineResolved,CaptureFolders.SubfolderQuarantineResolved+@"\journal" })
			{
				Assert.AreEqual("Capture",ImagingFolderAcl.GetPrivateRoot(subfolder),subfolder);
			}
			//Setup names the same group (installer\OpenPmsDental.iss, ImagingAccessGroup; InstallerTests checks that side).
			Assert.AreEqual("OpenPMS-Dental Users",ImagingFolderAcl.GroupName);
		}

		[TestMethod]
		public void GetPrivateRoot_IgnoresCaseAndSlashes_AndOnlyMatchesWholeFolderNames() {
			Assert.AreEqual(@"Photos\unsaved",ImagingFolderAcl.GetPrivateRoot("photos/UNSAVED"));
			Assert.AreEqual(@"Video\recording",ImagingFolderAcl.GetPrivateRoot(@"Video\recording\"));
			Assert.AreEqual("Capture",ImagingFolderAcl.GetPrivateRoot(@"capture\journal"));
			foreach(string subfolder in new[] { null,"","Photos","Video","Photos\\unsaved-old","Captures","Capture-old\\journal","Camera" }) {
				Assert.IsNull(ImagingFolderAcl.GetPrivateRoot(subfolder),subfolder ?? "(null)");
			}
		}

		[TestMethod]
		public void GetAccessSid_IsTheGroupWhenItExists_OtherwiseTheUsersGroup() {
			SecurityIdentifier sidGroup=new SecurityIdentifier("S-1-5-21-1-2-3-1009");
			Assert.AreEqual(sidGroup,ImagingFolderAcl.GetAccessSid(() => sidGroup));
			Assert.AreEqual(_sidUsers,ImagingFolderAcl.GetAccessSid(() => null));
			Assert.AreEqual(_sidUsers,ImagingFolderAcl.GetAccessSid(() => throw new IdentityNotMappedException()));
			Assert.AreEqual(_sidUsers,ImagingFolderAcl.GetAccessSid(null));
		}
		#endregion Which folders

		#region Real ACL
		[TestMethod]
		public void RealAcl_MakePrivate_LeavesOnlySystemAdministratorsAndTheAccessSid_AndWhatIsInsideFollows() {
			string folder=Path.Combine(_tempDir,"Capture");
			Directory.CreateDirectory(Path.Combine(folder,"journal"));
			string pathFile=Path.Combine(folder,"journal","image.png");
			File.WriteAllText(pathFile,"x");
			AddRule(folder,new FileSystemAccessRule(_sidUsers,FileSystemRights.Modify,InheritanceFlags.ContainerInherit|InheritanceFlags.ObjectInherit,
				PropagationFlags.None,AccessControlType.Allow));
			Assert.IsFalse(ImagingFolderAcl.IsPrivate(folder,_sidMe),"The Users rule is one too many.");
			Assert.AreEqual("",ImagingFolderAcl.MakePrivate(folder,_sidMe));
			DirectorySecurity directorySecurity=new DirectoryInfo(folder).GetAccessControl(AccessControlSections.Access);
			Assert.IsTrue(directorySecurity.AreAccessRulesProtected,"Nothing inherited from the folder above.");
			List<FileSystemAccessRule> listRules=directorySecurity.GetAccessRules(true,true,typeof(SecurityIdentifier)).Cast<FileSystemAccessRule>().ToList();
			CollectionAssert.AreEquivalent(new[] { _sidSystem,_sidAdministrators,_sidMe },listRules.Select(x => (SecurityIdentifier)x.IdentityReference).ToArray());
			Assert.IsTrue(listRules.All(x => !x.IsInherited && x.AccessControlType==AccessControlType.Allow));
			Assert.AreEqual(FileSystemRights.Modify,listRules.Single(x => x.IdentityReference.Equals(_sidMe)).FileSystemRights & ~FileSystemRights.Synchronize,
				"Modify, not Full control.");
			Assert.IsTrue(ImagingFolderAcl.IsPrivate(folder,_sidMe));
			Assert.IsFalse(ImagingFolderAcl.IsPrivate(folder,_sidUsers),"Private for another group is not private for this one.");
			//The folder and file that were already inside take the new list: no Users rule is left on them.
			Assert.IsTrue(ImagingFolderAcl.IsPrivate(Path.Combine(folder,"journal"),_sidMe));
			List<SecurityIdentifier> listFileSids=new FileInfo(pathFile).GetAccessControl(AccessControlSections.Access)
				.GetAccessRules(true,true,typeof(SecurityIdentifier)).Cast<FileSystemAccessRule>().Select(x => (SecurityIdentifier)x.IdentityReference).ToList();
			CollectionAssert.AreEquivalent(new[] { _sidSystem,_sidAdministrators,_sidMe },listFileSids);
			//A folder made afterwards inherits it.
			Directory.CreateDirectory(Path.Combine(folder,"quarantine"));
			Assert.IsTrue(ImagingFolderAcl.IsPrivate(Path.Combine(folder,"quarantine"),_sidMe));
			//This Windows user can still write and delete there.
			File.WriteAllText(Path.Combine(folder,"quarantine","b.png"),"y");
			File.Delete(Path.Combine(folder,"quarantine","b.png"));
		}

		[TestMethod]
		public void RealAcl_IsPrivate_IsFalseForAMissingFolder_AnExtraSid_OrTooFewRights() {
			Assert.IsFalse(ImagingFolderAcl.IsPrivate(Path.Combine(_tempDir,"missing"),_sidMe));
			Assert.IsFalse(ImagingFolderAcl.IsPrivate(_tempDir,null));
			string folder=Path.Combine(_tempDir,"Photos");
			Directory.CreateDirectory(folder);
			Assert.AreEqual("",ImagingFolderAcl.MakePrivate(folder,_sidMe));
			Assert.IsTrue(ImagingFolderAcl.IsPrivate(folder,_sidMe));
			AddRule(folder,new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid,null),FileSystemRights.ReadData,
				AccessControlType.Allow));
			Assert.IsFalse(ImagingFolderAcl.IsPrivate(folder,_sidMe),"Everyone may read: not private.");
			string folder2=Path.Combine(_tempDir,"Video");
			Directory.CreateDirectory(folder2);
			DirectorySecurity directorySecurity=new DirectorySecurity();
			directorySecurity.SetAccessRuleProtection(true,false);
			const InheritanceFlags inheritanceFlagsBoth=InheritanceFlags.ContainerInherit|InheritanceFlags.ObjectInherit;
			directorySecurity.AddAccessRule(new FileSystemAccessRule(_sidSystem,FileSystemRights.FullControl,inheritanceFlagsBoth,PropagationFlags.None,AccessControlType.Allow));
			directorySecurity.AddAccessRule(new FileSystemAccessRule(_sidAdministrators,FileSystemRights.FullControl,inheritanceFlagsBoth,PropagationFlags.None,AccessControlType.Allow));
			directorySecurity.AddAccessRule(new FileSystemAccessRule(_sidMe,FileSystemRights.ReadAndExecute|FileSystemRights.Write,inheritanceFlagsBoth,
				PropagationFlags.None,AccessControlType.Allow));
			new DirectoryInfo(folder2).SetAccessControl(directorySecurity);
			Assert.IsFalse(ImagingFolderAcl.IsPrivate(folder2,_sidMe),"Read and write without delete is less than Modify.");
		}

		[TestMethod]
		public void RealAcl_EnsureImagingFolder_GivesTheCaptureFolderItsList_AndTheJournalInheritsIt() {
			//A folder in this user's temporary folder already has only SYSTEM, Administrators and this user. The Users rule the Imaging
			//folder has in ProgramData makes Capture not private until it gets its own list.
			string folderImaging=Path.Combine(_tempDir,"Imaging");
			Directory.CreateDirectory(folderImaging);
			AddRule(folderImaging,new FileSystemAccessRule(_sidUsers,FileSystemRights.Modify,InheritanceFlags.ContainerInherit|InheritanceFlags.ObjectInherit,
				PropagationFlags.None,AccessControlType.Allow));
			List<string> listUsersGrants=new List<string>();
			string error;
			Assert.IsTrue(LocalCaptureConfig.EnsureImagingFolder(folderImaging,CaptureFolders.SubfolderJournal,
				path => { listUsersGrants.Add(path); return ""; },path => listUsersGrants.Contains(path),
				path => ImagingFolderAcl.MakePrivate(path,_sidMe),path => ImagingFolderAcl.IsPrivate(path,_sidMe),out error));
			Assert.AreEqual("",error);
			CollectionAssert.AreEqual(new[] { folderImaging },listUsersGrants,"Only the Imaging folder gets the Users rule.");
			Assert.IsTrue(new DirectoryInfo(Path.Combine(folderImaging,"Capture")).GetAccessControl(AccessControlSections.Access).AreAccessRulesProtected);
			Assert.IsFalse(new DirectoryInfo(Path.Combine(folderImaging,"Capture","journal")).GetAccessControl(AccessControlSections.Access).AreAccessRulesProtected,
				"The journal inherits the list and has none of its own.");
			Assert.IsTrue(ImagingFolderAcl.IsPrivate(Path.Combine(folderImaging,"Capture","journal"),_sidMe));
		}
		#endregion Real ACL

		#region EnsureImagingFolder with stand-ins
		[TestMethod]
		public void EnsureImagingFolder_PrivateFolders_GetTheListAtTheirRoot_OthersGetTheUsersRule() {
			string folderImaging=Path.Combine(_tempDir,"Imaging");
			FakeAcl fakeAcl=new FakeAcl();
			string error;
			Assert.IsTrue(fakeAcl.Ensure(folderImaging,@"Capture\quarantine",out error));
			Assert.AreEqual("",error);
			CollectionAssert.AreEqual(new[] { Path.Combine(folderImaging,"Capture") },fakeAcl.ListMakePrivateCalls);
			CollectionAssert.AreEqual(new[] { folderImaging },fakeAcl.ListUsersGrants);
			Assert.IsTrue(Directory.Exists(Path.Combine(folderImaging,"Capture","quarantine")));
			//Again, and another folder under the same root: nothing more to do.
			Assert.IsTrue(fakeAcl.Ensure(folderImaging,"Capture/journal",out error));
			Assert.IsTrue(fakeAcl.Ensure(folderImaging,"Capture",out error));
			Assert.AreEqual(1,fakeAcl.ListMakePrivateCalls.Count);
			//The unsaved photos: only Photos\unsaved, not Photos.
			Assert.IsTrue(fakeAcl.Ensure(folderImaging,"Photos/unsaved",out error));
			Assert.AreEqual(Path.Combine(folderImaging,"Photos","unsaved"),fakeAcl.ListMakePrivateCalls.Last());
			//Video itself is not private and needs nothing: it inherits the Imaging folder's Users rule.
			Assert.IsTrue(fakeAcl.Ensure(folderImaging,"Video",out error));
			Assert.AreEqual(2,fakeAcl.ListMakePrivateCalls.Count);
			Assert.AreEqual(1,fakeAcl.ListUsersGrants.Count);
			Assert.IsTrue(fakeAcl.Ensure(folderImaging,@"Video\recording",out error));
			Assert.AreEqual(Path.Combine(folderImaging,"Video","recording"),fakeAcl.ListMakePrivateCalls.Last());
		}

		[TestMethod]
		public void EnsureImagingFolder_WhenTheListCannotBeSet_TheFolderIsStillUsable_WithAWarning() {
			string folderImaging=Path.Combine(_tempDir,"Imaging");
			FakeAcl fakeAcl=new FakeAcl { FuncMakePrivateResult=path => "Access is denied." };
			string error;
			Assert.IsTrue(fakeAcl.Ensure(folderImaging,@"Photos\unsaved",out error));
			StringAssert.Contains(error,"Access is denied.");
			StringAssert.Contains(error,Path.Combine(folderImaging,"Photos","unsaved"));
			Assert.IsTrue(Directory.Exists(Path.Combine(folderImaging,"Photos","unsaved")),"Kept: capture must not fail for this.");
			//Under a root that could not be set, the subfolder tries for itself.
			Assert.IsTrue(fakeAcl.Ensure(folderImaging,@"Capture\journal",out error));
			CollectionAssert.AreEqual(new[] { Path.Combine(folderImaging,"Photos","unsaved"),Path.Combine(folderImaging,"Capture"),
				Path.Combine(folderImaging,"Capture","journal") },fakeAcl.ListMakePrivateCalls);
			StringAssert.Contains(error,Path.Combine(folderImaging,"Capture","journal"));
			//A step that throws counts as a failure, not a crash.
			FakeAcl fakeAclThrows=new FakeAcl { FuncMakePrivateResult=path => throw new InvalidOperationException("boom") };
			Assert.IsTrue(fakeAclThrows.Ensure(Path.Combine(_tempDir,"Imaging2"),"Capture",out error));
			StringAssert.Contains(error,"boom");
		}

		[TestMethod]
		public void EnsureImagingFolder_WithoutThePrivateSteps_KeepsTheUsersRuleOnly() {
			//The overload the capture folder tests use (CaptureFolders.CreateForFolder) changes no list of its own.
			string folderImaging=Path.Combine(_tempDir,"Imaging");
			List<string> listUsersGrants=new List<string>();
			string error;
			Assert.IsTrue(LocalCaptureConfig.EnsureImagingFolder(folderImaging,@"Capture\quarantine",path => { listUsersGrants.Add(path); return ""; },
				path => listUsersGrants.Any(x => path.StartsWith(x,StringComparison.OrdinalIgnoreCase)),out error));
			CollectionAssert.AreEqual(new[] { folderImaging },listUsersGrants);
			Assert.IsFalse(new DirectoryInfo(Path.Combine(folderImaging,"Capture")).GetAccessControl(AccessControlSections.Access).AreAccessRulesProtected);
		}
		#endregion EnsureImagingFolder with stand-ins

		#region Helpers
		///<summary>Stand-ins for the Users rule and the private list. A folder is private when it or a folder above it was made private
		///(inheritance), and has the Users rule when it or a folder above it got it.</summary>
		private sealed class FakeAcl {
			public readonly List<string> ListUsersGrants=new List<string>();
			public readonly List<string> ListMakePrivateCalls=new List<string>();
			private readonly HashSet<string> _hashSetPrivate=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			public Func<string,string> FuncMakePrivateResult=path => "";

			public bool Ensure(string folderImaging,string subfolder,out string error) {
				return LocalCaptureConfig.EnsureImagingFolder(folderImaging,subfolder,Grant,Has,MakePrivate,IsPrivate,out error);
			}

			private string Grant(string path) {
				ListUsersGrants.Add(path);
				return "";
			}

			private bool Has(string path) {
				return Above(path).Any(x => ListUsersGrants.Contains(x,StringComparer.OrdinalIgnoreCase));
			}

			private string MakePrivate(string path) {
				ListMakePrivateCalls.Add(path);
				string error=FuncMakePrivateResult(path);
				if(error=="") {
					_hashSetPrivate.Add(path);
				}
				return error;
			}

			private bool IsPrivate(string path) {
				return Above(path).Any(x => _hashSetPrivate.Contains(x));
			}

			private static IEnumerable<string> Above(string path) {
				for(string pathCur=path;!string.IsNullOrEmpty(pathCur);pathCur=Path.GetDirectoryName(pathCur)) {
					yield return pathCur;
				}
			}
		}

		private static void AddRule(string folder,FileSystemAccessRule fileSystemAccessRule) {
			DirectoryInfo directoryInfo=new DirectoryInfo(folder);
			DirectorySecurity directorySecurity=directoryInfo.GetAccessControl(AccessControlSections.Access);
			directorySecurity.AddAccessRule(fileSystemAccessRule);
			directoryInfo.SetAccessControl(directorySecurity);
		}
		#endregion Helpers
	}
}
