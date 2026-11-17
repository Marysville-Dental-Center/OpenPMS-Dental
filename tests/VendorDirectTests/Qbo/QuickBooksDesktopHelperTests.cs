// S3a-13 of the .NET 10 move (docs/port-notes/S3a-13.md): QuickBooks Desktop moved into the 32-bit helper program; QuickBooks Online is untouched.
// QuickBooks Online is the program's own code (OpenPmsDentalBusiness.Qbo, OAuth and REST) and never goes through the helper or QBFC; QuickBooks
// Desktop keeps its public methods (the four windows that call them do not change) and sends today's arguments to the helper. The end-to-end
// test runs a copy of the helper with a stand-in for its QuickBooks module (no QBFC, no QuickBooks); the copy is ended and deleted.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental.Bridges;
using VendorDirectTests.Foundation;

namespace VendorDirectTests.Qbo {
	[TestClass]
	public class QuickBooksDesktopHelperTests {
		private static string ReadRepoFile(string relativePath) {
			return File.ReadAllText(Path.Combine(FoundationLayoutTests.GetRepositoryRoot(),relativePath));
		}

		[TestMethod]
		public void QuickBooksOnline_NeverGoesThroughTheHelperOrQbfc() {
			string folder=Path.Combine(FoundationLayoutTests.GetRepositoryRoot(),@"OpenPmsDentalBusiness\Bridges\QuickBooksOnline");
			string[] arrayFiles=Directory.GetFiles(folder,"*.cs").Concat(new[] {
				Path.Combine(FoundationLayoutTests.GetRepositoryRoot(),@"OpenPmsDental\Forms\FormQuickBooksOnlineSetup.cs") }).ToArray();
			Assert.IsTrue(arrayFiles.Length>=10);
			foreach(string file in arrayFiles) {
				string text=File.ReadAllText(file);
				foreach(string forbidden in new[] { "Helper32","QBFC","QBSessionManager","OpenPmsDental.Bridges.QuickBooks." }) {
					Assert.IsFalse(text.Contains(forbidden),Path.GetFileName(file)+" contains "+forbidden);
				}
			}
			//The deposit window still takes each path by the accounting software preference.
			string textDeposit=ReadRepoFile(@"OpenPmsDental\Forms\FormDepositEdit.cs");
			StringAssert.Contains(textDeposit,"QuickBooks.CreateDeposit(");
			StringAssert.Contains(textDeposit,"QuickBooksOnline.");
		}

		[TestMethod]
		public void QuickBooksDesktop_KeepsItsPublicMethods_WithNoQbfcType() {
			Type type=typeof(QuickBooks);
			string[] arrayMethods=type.GetMethods(BindingFlags.Public|BindingFlags.Static|BindingFlags.DeclaredOnly)
				.Select(x => x.ReturnType.Name+" "+x.Name+"("+string.Join(",",x.GetParameters().Select(y => y.ParameterType.Name+" "+y.Name))+")")
				.OrderBy(x => x,StringComparer.Ordinal).ToArray();
			CollectionAssert.AreEqual(new[] {
				"List`1 GetListOfAccounts()",
				"List`1 GetListOfClasses()",
				"String TestConnection(String companyPath)",
				"Void CreateDeposit(DateTime txnDate,String depositAcct,String incomeAcct,Double amount,String memo,String classRef)",
			},arrayMethods);
			Assert.IsNotNull(type.GetConstructor(Type.EmptyTypes),"The public constructor stays.");
			//S3a-14n (native first): WpfControlsOD carries QBFC's interop types again, embedded as before S3a-13, for its in-process path
			//(QuickBooksInProcess); it still references no interop assembly, so none is copied or shipped.
			Assert.IsTrue(type.Assembly.GetTypes().Any(x => x.Namespace!=null && x.Namespace.StartsWith("Interop.QBFC",StringComparison.Ordinal)),
				"WpfControlsOD embeds the QBFC types its in-process path uses.");
			Assert.IsFalse(type.Assembly.GetReferencedAssemblies().Any(x => x.Name.StartsWith("Interop.",StringComparison.Ordinal)));
		}

		///<summary>The deposit's arguments for the helper, as BuildDepositAddRq used them: the amount formatted in the program's culture ("F"), and
		///no class when none was given (the class preference is then not read, as before).</summary>
		[TestMethod]
		public void CreateDeposit_SendsTodaysValues() {
			MethodInfo method=typeof(QuickBooks).GetMethod("GetCreateDepositArgs",BindingFlags.NonPublic|BindingFlags.Static);
			Assert.IsNotNull(method);
			DateTime dateDeposit=new DateTime(2026,11,14);
			string[] arrayArgs=(string[])method.Invoke(null,new object[] { @"C:\QuickBooks\Synthetic Company.qbw",dateDeposit,"Checking","Income",1234.5,
				"memo (synthetic)","" });
			CollectionAssert.AreEqual(new[] { @"C:\QuickBooks\Synthetic Company.qbw",dateDeposit.ToBinary().ToString(System.Globalization.CultureInfo.InvariantCulture),
				"Checking","Income",1234.5.ToString("F"),"memo (synthetic)","","0" },arrayArgs);
		}

		///<summary>TestConnection through a copy of the helper whose QuickBooks module is a stand-in that echoes its arguments: the program's
		///QuickBooks class reaches the module "QuickBooks" with the company file; a missing helper gives TestConnection's "Error: " message.</summary>
		[TestMethod]
		public void TestConnection_GoesThroughTheHelper_AndAMissingHelperIsAnErrorMessage() {
			//A copy of the helper in a helper folder of this test process (Helper32Tests' HelperCopy, linked): since S3b-5 the helper is the program
			//output's self-contained x86 one (Helper32\), with this assembly written for any CPU beside it for the stand-in module.
			Helper32Tests.Support.HelperCopy helperCopy=Helper32Tests.Support.HelperCopy.Create();
			string folder=Helper32Tests.Support.HelperCopy.Folder;
			string pathCopy=helperCopy.PathExe;
			FieldInfo fieldDefault=typeof(Helper32Client).GetField("_clientDefault",BindingFlags.NonPublic|BindingFlags.Static);
			Assert.IsNotNull(fieldDefault);
			object clientBefore=fieldDefault.GetValue(null);
			Helper32Client client=new Helper32Client(pathCopy,"--test-module VendorDirectTests "+typeof(QuickBooksStandIn).FullName+" QuickBooks");
			try {
				//S3a-14n: the helper's path, as a 64-bit program without a 64-bit QBFC takes it; the chooser is fixed first, so no QBFC object is
				//activated in this process.
				NativeFirstChooser.QuickBooksDesktop.ForceChoiceForTests(VendorPath.Helper32);
				fieldDefault.SetValue(null,client);
				Assert.AreEqual(@"stand-in TestConnection C:\QuickBooks\Synthetic Company.qbw",QuickBooks.TestConnection(@"C:\QuickBooks\Synthetic Company.qbw"));
				int processId=client.ProcessId;
				Assert.AreNotEqual(0,processId);
				//N64-B6: a 64-bit program whose 64-bit QBFC chooser picked the helper (no 64-bit QBFC, or a failed connection) reaches the same
				//helper module, and never asks QBFC 10's chooser (no QBFC 10 activation in a 64-bit process).
				NativeFirstChooser.QuickBooksDesktop.ResetForTests();
				NativeFirstChooser.QuickBooksDesktopNative64.ResetForTests(true);
				NativeFirstChooser.QuickBooksDesktopNative64.ForceChoiceForTests(VendorPath.Helper32);
				Assert.AreEqual(@"stand-in TestConnection C:\QuickBooks\Synthetic Company.qbw",QuickBooks.TestConnection(@"C:\QuickBooks\Synthetic Company.qbw"));
				Assert.AreEqual(processId,client.ProcessId,"The same helper.");
				Assert.IsNull(NativeFirstChooser.QuickBooksDesktop.Choice,"QBFC 10's chooser is not used in a 64-bit program.");
				NativeFirstChooser.QuickBooksDesktopNative64.ResetForTests();
				NativeFirstChooser.QuickBooksDesktop.ForceChoiceForTests(VendorPath.Helper32);
				fieldDefault.SetValue(null,new Helper32Client(Path.Combine(folder,"OpenPmsDentalHelper32-000000000000.exe")));
				StringAssert.StartsWith(QuickBooks.TestConnection(@"C:\QuickBooks\Synthetic Company.qbw"),"Error: The 32-bit helper program "
					+"OpenPmsDentalHelper32.exe was not found");
			}
			finally {
				string nameCopy=Path.GetFileNameWithoutExtension(pathCopy);
				try {
					NativeFirstChooser.QuickBooksDesktop.ResetForTests();
					NativeFirstChooser.QuickBooksDesktopNative64.ResetForTests();
					fieldDefault.SetValue(null,clientBefore);
					client.Dispose();
					for(int i=0;i<40 && Process.GetProcesses().Any(x => SafeName(x)==nameCopy);i++) {
						Thread.Sleep(250);
					}
					Assert.IsFalse(Process.GetProcesses().Any(x => SafeName(x)==nameCopy),"The copy's helper ended.");
				}
				finally {
					//CLEAN-1 (S3a-13's later cleanups): the copy is deleted even when a check above failed, as HelperTestBase does. A copy still
					//running (only this test starts a helper of this random name) is ended first, so its file can be deleted.
					EndCopiesStillRunning(nameCopy);
					helperCopy.Dispose();
				}
			}
		}

		///<summary>Ends every process named nameCopy (this test's own helper copy, by its random name) and waits up to 10 s for each. Never throws.</summary>
		private static void EndCopiesStillRunning(string nameCopy) {
			foreach(Process process in Process.GetProcesses()) {
				try {
					if(SafeName(process)!=nameCopy) {
						continue;
					}
					process.Kill();
					process.WaitForExit(10000);
				}
				catch(Exception) {
					//Ended meanwhile, or not ours to end: the delete below reports what is left.
				}
				finally {
					process.Dispose();
				}
			}
		}

		///<summary>Deletes the file when it is there; a file that cannot be deleted fails the test with its path.</summary>
		private static void DeleteIfThere(string path) {
			if(File.Exists(path)) {
				File.Delete(path);
			}
		}

		private static string SafeName(Process process) {
			try {
				return process.ProcessName;
			}
			catch(Exception) {
				return "";
			}
		}
	}

	///<summary>Runs in the helper's process in place of its QuickBooks module (no QBFC): answers each command with its name and arguments.</summary>
	public static class QuickBooksStandIn {
		public static string[] Handle(string command,string[] args,byte[] data,out byte[] dataReply) {
			dataReply=null;
			return new[] { "stand-in "+command+" "+string.Join("|",args) };
		}
	}
}
