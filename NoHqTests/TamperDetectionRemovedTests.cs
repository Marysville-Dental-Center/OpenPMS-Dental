using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalBusiness;
using ProjectFileTests;

namespace NoHqTests {
	///<summary>Tamper detection is removed (NOHASH, docs/specs/remove-tamper-detection.md): the program neither computes nor checks the SecurityHash
	///columns of Open Dental's tables, which it keeps as they are. No hash code, no Database Integrity warning and no Security Hashes tool is left.
	///The SecurityHash fields themselves stay: they are columns of Open Dental's schema. No database or network connection needed.</summary>
	[TestClass]
	public class TamperDetectionRemovedTests {
		private const BindingFlags ALL_DECLARED=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static|BindingFlags.Instance|BindingFlags.DeclaredOnly;

		[TestMethod]
		public void TheHashCode_IsGone() {
			Assembly assemblyCodeBase=typeof(CodeBase.Secrets).Assembly;
			Assert.IsNull(assemblyCodeBase.GetType("CodeBase.TamperHash"),"CodeBase.TamperHash");
			Assert.IsNull(assemblyCodeBase.GetType("CodeBase.SaltedHash"),"CodeBase.SaltedHash");
			Assert.IsNotNull(assemblyCodeBase.GetType("CodeBase.HmacSha256"),"HmacSha256 stays: SecretBox uses it.");
			Assembly assemblyBusiness=typeof(Patient).Assembly;
			Assert.IsNull(assemblyBusiness.GetType("OpenPmsDentalBusiness.Misc.SecurityHash"),"OpenPmsDentalBusiness.Misc.SecurityHash");
			foreach(Type type in new[] { typeof(Appointments),typeof(Claims),typeof(ClaimProcs),typeof(InsSubs),typeof(InsVerifies),typeof(InsVerifyHists),
				typeof(Patients),typeof(PayPlans),typeof(PaySplits),typeof(Payments) })
			{
				string[] arrayNames=type.GetMembers(ALL_DECLARED).Select(x => x.Name)
					.Where(x => x=="HashFields" || x=="SetHashForUpdate" || x=="RehashAfterDirectUpdate" || (x.Contains("Hash") && x.Contains("Valid")))
					.Distinct().ToArray();
				Assert.AreEqual(0,arrayNames.Length,type.Name+": "+string.Join(", ",arrayNames));
			}
			Assert.AreEqual(0,typeof(DatabaseMaintenances).GetMember("SecurityHashRehashPatientTable",ALL_DECLARED).Length,
				"The Security Hashes maintenance method is removed.");
		}

		[TestMethod]
		public void TheSecurityHashFields_Stay() {
			foreach(Type type in new[] { typeof(Appointment),typeof(Claim),typeof(ClaimProc),typeof(HistAppointment),typeof(InsSub),typeof(InsVerify),
				typeof(InsVerifyHist),typeof(Patient),typeof(PayPlan),typeof(PaySplit) })
			{
				Assert.IsNotNull(type.GetField("SecurityHash"),type.Name+".SecurityHash is a column of Open Dental's schema and stays.");
			}
		}

		[TestMethod]
		public void TheWarningTriangles_AreGone() {
			string dirTests=Path.GetDirectoryName(typeof(TamperDetectionRemovedTests).Assembly.Location);//NoHqTests\bin\Debug
			string folder=Path.GetFullPath(Path.Combine(dirTests,"..","..","..","OpenPmsDental","bin",Path.GetFileName(dirTests)));
			Assembly assemblyProgram=ProgramAssembly.Load(folder,"OpenPmsDental");
			Assert.IsNull(assemblyProgram.GetType("OpenPmsDental.UI.WarningIntegrity"),"OpenPmsDental.UI.WarningIntegrity");
			string pathWpf=Path.Combine(folder,"WpfControlsOD.dll");
			if(!File.Exists(pathWpf)) {
				Assert.Inconclusive(pathWpf+" has not been built.");
			}
			Assembly assemblyWpf=Assembly.LoadFrom(pathWpf);
			string[] arrayNames=assemblyWpf.GetTypes().Select(x => x.Name)
				.Where(x => x=="WarningIntegrity" || x=="FrmDatabaseIntegrity" || x=="DatabaseIntegrities" || x=="DatabaseIntegrity").ToArray();
			Assert.AreEqual(0,arrayNames.Length,"No Database Integrity warning or window: "+string.Join(", ",arrayNames));
		}
	}
}
