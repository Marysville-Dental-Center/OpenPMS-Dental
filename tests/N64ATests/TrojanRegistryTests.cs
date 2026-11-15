// Trojan's install key (N64-A, item A4 of docs/specs/native64-survey.md, row 16): OpenInstallRegistryKey looks in the 32-bit registry view,
// then the 64-bit one, under Software\TROJAN BENEFIT SERVICE, then under Software\Trojan (the key Open Dental's manual names). The lookup runs
// on a fake registry seam (Trojan.FindInstallKey); no test opens a real registry key.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32;
using OpenPmsDental.Bridges;

namespace N64ATests {
	[TestClass]
	public class TrojanRegistryTests {
		[TestMethod]
		public void LookupOrder_Registry32ThenRegistry64_TheOldKeyThenTheManualsKey() {
			List<Tuple<RegistryView,string>> listOrder=Trojan.GetInstallKeyLookupOrder();
			Assert.AreEqual(@"Registry32 Software\TROJAN BENEFIT SERVICE|Registry64 Software\TROJAN BENEFIT SERVICE|Registry32 Software\Trojan|Registry64 Software\Trojan",
				string.Join("|",listOrder.Select(x => x.Item1+" "+x.Item2)));
			Assert.AreEqual("INSTALLDIR",Trojan.INSTALL_DIR_VALUE_NAME);
		}

		[TestMethod]
		public void Registry32_WithInstallDir_IsTakenFirst_NothingElseOpened() {
			FakeRegistry fakeRegistry=new FakeRegistry();
			fakeRegistry.Add(RegistryView.Registry32,Trojan.INSTALL_KEY_PATH,true);
			fakeRegistry.Add(RegistryView.Registry64,Trojan.INSTALL_KEY_PATH,true);
			FakeKey fakeKey=fakeRegistry.Find();
			Assert.AreEqual(@"Registry32 Software\TROJAN BENEFIT SERVICE",fakeKey.Name);
			Assert.IsFalse(fakeKey.IsDisposed);
			Assert.AreEqual(1,fakeRegistry.ListOpened.Count,"As before: one key opened.");
		}

		[TestMethod]
		public void OnlyRegistry64_IsFound() {
			FakeRegistry fakeRegistry=new FakeRegistry();
			fakeRegistry.Add(RegistryView.Registry64,Trojan.INSTALL_KEY_PATH,true);
			FakeKey fakeKey=fakeRegistry.Find();
			Assert.AreEqual(@"Registry64 Software\TROJAN BENEFIT SERVICE",fakeKey.Name);
			Assert.IsFalse(fakeKey.IsDisposed);
		}

		[TestMethod]
		public void Registry32WithoutInstallDir_Registry64WithIt_TheSecondWins_TheFirstIsDisposed() {
			FakeRegistry fakeRegistry=new FakeRegistry();
			FakeKey fakeKey32=fakeRegistry.Add(RegistryView.Registry32,Trojan.INSTALL_KEY_PATH,false);
			fakeRegistry.Add(RegistryView.Registry64,Trojan.INSTALL_KEY_PATH,true);
			FakeKey fakeKey=fakeRegistry.Find();
			Assert.AreEqual(@"Registry64 Software\TROJAN BENEFIT SERVICE",fakeKey.Name);
			Assert.IsTrue(fakeKey32.IsDisposed);
			Assert.IsFalse(fakeKey.IsDisposed);
		}

		[TestMethod]
		public void OnlyTheManualsKey_IsFound_InEitherView() {
			foreach(RegistryView registryView in new[] { RegistryView.Registry32,RegistryView.Registry64 }) {
				FakeRegistry fakeRegistry=new FakeRegistry();
				fakeRegistry.Add(registryView,Trojan.INSTALL_KEY_PATH_MANUAL,true);
				Assert.AreEqual(registryView+@" Software\Trojan",fakeRegistry.Find().Name);
				Assert.AreEqual(registryView==RegistryView.Registry32 ? 3 : 4,fakeRegistry.ListOpened.Count);
			}
		}

		///<summary>No key has INSTALLDIR: the first key found is returned, as before (the callers then say INSTALLDIR is missing), and every other
		///key opened is disposed.</summary>
		[TestMethod]
		public void NoKeyWithInstallDir_TheFirstFoundIsReturned_TheOthersDisposed() {
			FakeRegistry fakeRegistry=new FakeRegistry();
			FakeKey fakeKeyFirst=fakeRegistry.Add(RegistryView.Registry64,Trojan.INSTALL_KEY_PATH,false);
			FakeKey fakeKeySecond=fakeRegistry.Add(RegistryView.Registry32,Trojan.INSTALL_KEY_PATH_MANUAL,false);
			FakeKey fakeKey=fakeRegistry.Find();
			Assert.AreSame(fakeKeyFirst,fakeKey);
			Assert.IsFalse(fakeKey.IsDisposed);
			Assert.IsTrue(fakeKeySecond.IsDisposed);
			Assert.AreEqual(4,fakeRegistry.ListOpened.Count);
		}

		[TestMethod]
		public void NoKey_Null() {
			FakeRegistry fakeRegistry=new FakeRegistry();
			Assert.IsNull(fakeRegistry.Find());
			Assert.AreEqual(4,fakeRegistry.ListOpened.Count);
		}

		///<summary>The real lookup names a view on every open (Registry32 or Registry64, never Default: PointerSizeScanTests' rule 4) and goes through
		///FindInstallKey, the seam these tests use.</summary>
		[TestMethod]
		public void Source_OpenInstallRegistryKey_UsesTheSeamAndNamedViews() {
			string trojan=TestSupport.ReadRepoFile(@"WpfControlsOD\Bridges\Trojan.cs");
			StringAssert.Contains(trojan,"return FindInstallKey(OpenLocalMachineSubKey,x => x.GetValue(INSTALL_DIR_VALUE_NAME)!=null);");
			StringAssert.Contains(trojan,"RegistryKey.OpenBaseKey(RegistryHive.LocalMachine,registryView)");
			Assert.IsFalse(trojan.Contains("RegistryView.Default"));
		}

		///<summary>A registry of fake keys by (view, path), recording each open.</summary>
		private class FakeRegistry {
			private readonly Dictionary<string,FakeKey> _dictKeys=new Dictionary<string,FakeKey>();
			public readonly List<string> ListOpened=new List<string>();

			public FakeKey Add(RegistryView registryView,string path,bool hasInstallDir) {
				FakeKey fakeKey=new FakeKey { Name=registryView+" "+path,HasInstallDir=hasInstallDir };
				_dictKeys[fakeKey.Name]=fakeKey;
				return fakeKey;
			}

			public FakeKey Find() {
				return Trojan.FindInstallKey((registryView,path) => {
					string name=registryView+" "+path;
					ListOpened.Add(name);
					_dictKeys.TryGetValue(name,out FakeKey fakeKey);
					return fakeKey;
				},x => x.HasInstallDir);
			}
		}

		private class FakeKey:IDisposable {
			public string Name;
			public bool HasInstallDir;
			public bool IsDisposed;

			public void Dispose() {
				IsDisposed=true;
			}
		}
	}
}
