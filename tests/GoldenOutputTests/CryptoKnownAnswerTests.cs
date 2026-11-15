// Known answers of the program's own encryption, recorded with the .NET Framework build (docs/port-notes/S3a-4.md; the plan's risk table, "Crypto"):
// MiscUtils.Encrypt/Decrypt, CentralConnections.Encrypt/Decrypt, a stored signature (the signature box and SigBox.EncryptSigString with the key of a
// treatment plan and of a pay plan) and the RDL engine's data source reference file (DataSourceReference.Create/Retrieve). Every ciphertext is made
// here from a synthetic plaintext, with a test key or the program's own built-in key; no real stored password, connection string or signature is
// read or decrypted. Each test checks both ways: today's encryption gives the recorded ciphertext byte for byte, and the recorded ciphertext
// decrypts to the plaintext (so a .NET 10 build also reads what .NET Framework wrote).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using CodeBase;
using fyiReporting.RDL;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental.UI;
using OpenPmsDentalBusiness;

namespace GoldenOutputTests {
	[TestClass]
	public class CryptoKnownAnswerTests {
		///<summary>Synthetic plaintexts: ASCII, accented letters and a non-BMP character, more than one AES block.</summary>
		public const string PLAINTEXT="Synthetic secret for Golden Synthetic Dental: 0123456789 caf\u00e9 \u00dcn\u00efc\u00f6d\u00e9 \u20ac \U0001F9B7 end.";
		public const string PLAINTEXT_SHORT="x";
		///<summary>A test key for CentralConnections.Encrypt (16 bytes, AES-128), not the program's.</summary>
		public static readonly byte[] TestKey16=Encoding.ASCII.GetBytes("GoldenTestKey016");
		///<summary>A synthetic connection string for the RDL data source reference file, and the test passphrase it is encrypted with.</summary>
		public const string RDL_CONNECTION="Server=db.example.test;Port=3306;Database=golden_synthetic;User ID=synthetic;Password=not-a-real-password";
		public const string RDL_PASSPHRASE="golden test passphrase";
		private const string FIXTURE_TEXT="crypto-known-answers.txt";
		private const string FIXTURE_DSR="rdl-datasource-reference.dsr";
		///<summary>Signature points with tenths, as the signature box keeps them ("0.#" in the current culture, en-US here).</summary>
		private static readonly PointF[] _arrayPointFs=new[] { new PointF(10f,12.5f),new PointF(30.25f,40f),new PointF(0f,0f),new PointF(55.5f,20.1f),
			new PointF(300f,70f) };

		[TestInitialize]
		public void TestInitialize() {
			Golden.UseUsEnglish();
		}

		///<summary>The recorded value of a "name=value" line of the known-answer fixture.</summary>
		private static string GetRecorded(string name) {
			string line=Golden.ReadFixtureText(FIXTURE_TEXT).Split('\n').FirstOrDefault(x => x.StartsWith(name+"=",StringComparison.Ordinal));
			Assert.IsNotNull(line,name+" is not in "+FIXTURE_TEXT);
			return line.Substring(name.Length+1);
		}

		private static string GetSignatureKeyTreatPlan() {
			TreatPlan treatPlan=SignatureKeyGoldenTests.GetTreatPlan(out List<ProcTP> listProcTPs);
			return TreatPlans.GetKeyDataForSignatureSaving(treatPlan,listProcTPs);
		}

		private static string GetSignatureKeyDataPayPlan() {
			PayPlan payPlan=PayPlans.GetOne(GoldenData.PayPlanNum);
			return PayPlans.GetKeyDataStringForSignature(payPlan,Patients.GetLim(payPlan.PatNum),Patients.GetLim(payPlan.Guarantor));
		}

		///<summary>A signature as the treatment plan window stores it: the box keyed with the plan's key hash (SetAutoKeyData), the points, GetSigString.
		///</summary>
		private static string SignTreatPlan(string keyHash) {
			using(SignatureBox signatureBox=new SignatureBox()) {
				signatureBox.SetAutoKeyData(keyHash);
				signatureBox.SetPointList(_arrayPointFs.ToList());
				return signatureBox.GetSigString();
			}
		}

		///<summary>A digital signature ("Digitally Signed by ...") as the box stores it.</summary>
		private static string SignDigitally(string keyHash) {
			using(SignatureBox signatureBox=new SignatureBox()) {
				signatureBox.SetAutoKeyData(keyHash);
				signatureBox.SetDigitalSig("Digitally Signed by GOLD1 - Goldsmith, Grace\r\nDate Signed: 4/15/2025 9:30 AM");
				return signatureBox.GetSigString();
			}
		}

		///<summary>The points or the digital signature text a stored signature decrypts to (SetSigString), points with the round-trip text of each float.</summary>
		private static string ReadSignature(string keyHash,string sigString) {
			using(SignatureBox signatureBox=new SignatureBox()) {
				signatureBox.SetAutoKeyData(keyHash);
				signatureBox.SetSigString(sigString);
				if(signatureBox.IsDigitalSignature()) {
					return "digital:"+signatureBox.GetPointStringForWebTest();
				}
				FieldInfo fieldInfo=typeof(SignatureBox).GetField("listPointFs",BindingFlags.Instance|BindingFlags.NonPublic);
				Assert.IsNotNull(fieldInfo,"SignatureBox.listPointFs");
				List<PointF> listPointFs=(List<PointF>)fieldInfo.GetValue(signatureBox);
				return "points:"+string.Join(";",listPointFs.Select(x => x.X.ToString("R",CultureInfo.InvariantCulture)+","
					+x.Y.ToString("R",CultureInfo.InvariantCulture)));
			}
		}

		private static string GetExpectedPoints() {
			return "points:"+string.Join(";",_arrayPointFs.Select(x => x.X.ToString("R",CultureInfo.InvariantCulture)+","
				+x.Y.ToString("R",CultureInfo.InvariantCulture)));
		}

		///<summary>The pay plan signature of the eClipboard and patient portal path (PayPlans.SignPayPlan): the points scaled to the box
		///(TreatPlans.GetScaledSignature) and encrypted with the MD5 of the key data (SigBox.EncryptSigString).</summary>
		private static string SignPayPlan(string keyData) {
			byte[] arrayHash=ODCrypt.MD5.Hash(Encoding.UTF8.GetBytes(keyData));
			return OpenPmsDentalBusiness.UI.SigBox.EncryptSigString(arrayHash,TreatPlans.GetScaledSignature("10,10;40,30;0,0;80,15;600,140"));
		}

		[TestMethod]
		public void KnownAnswers_EncryptGivesTheRecordedCiphertexts() {
			GoldenDb.Begin();
			string keyHashTreatPlan=GetSignatureKeyTreatPlan();
			StringBuilder stringBuilder=new StringBuilder();
			stringBuilder.Append("MiscUtils.Encrypt="+MiscUtils.Encrypt(PLAINTEXT)+"\n");
			stringBuilder.Append("MiscUtils.Encrypt.short="+MiscUtils.Encrypt(PLAINTEXT_SHORT)+"\n");
			stringBuilder.Append("CentralConnections.Encrypt="+CentralConnections.Encrypt(PLAINTEXT,TestKey16)+"\n");
			stringBuilder.Append("CentralConnections.Encrypt.short="+CentralConnections.Encrypt(PLAINTEXT_SHORT,TestKey16)+"\n");
			stringBuilder.Append("Signature.TreatPlan="+SignTreatPlan(keyHashTreatPlan)+"\n");
			stringBuilder.Append("Signature.TreatPlan.Digital="+SignDigitally(keyHashTreatPlan)+"\n");
			stringBuilder.Append("Signature.PayPlan="+SignPayPlan(GetSignatureKeyDataPayPlan())+"\n");
			Golden.AssertText(FIXTURE_TEXT,stringBuilder.ToString());
		}

		[TestMethod]
		public void KnownAnswers_MiscUtilsDecrypt_ReadsTheRecord() {
			Assert.AreEqual(PLAINTEXT,MiscUtils.Decrypt(GetRecorded("MiscUtils.Encrypt"),doThrow:true));
			Assert.AreEqual(PLAINTEXT_SHORT,MiscUtils.Decrypt(GetRecorded("MiscUtils.Encrypt.short"),doThrow:true));
		}

		[TestMethod]
		public void KnownAnswers_CentralConnectionsDecrypt_ReadsTheRecord() {
			Assert.AreEqual(PLAINTEXT,CentralConnections.Decrypt(GetRecorded("CentralConnections.Encrypt"),TestKey16));
			Assert.AreEqual(PLAINTEXT_SHORT,CentralConnections.Decrypt(GetRecorded("CentralConnections.Encrypt.short"),TestKey16));
		}

		[TestMethod]
		public void KnownAnswers_StoredSignatures_ReadTheRecord() {
			GoldenDb.Begin();
			string keyHashTreatPlan=GetSignatureKeyTreatPlan();
			Assert.AreEqual(GetExpectedPoints(),ReadSignature(keyHashTreatPlan,GetRecorded("Signature.TreatPlan")));
			Assert.AreEqual("digital:Digitally Signed by GOLD1 - Goldsmith, Grace\r\nDate Signed: 4/15/2025 9:30 AM",
				ReadSignature(keyHashTreatPlan,GetRecorded("Signature.TreatPlan.Digital")));
			//The pay plan signature is read back by the box keyed with the raw key data (SetAutoKeyData hashes it with MD5, as EncryptSigString was given).
			string pointsPayPlan=ReadSignature(GetSignatureKeyDataPayPlan(),GetRecorded("Signature.PayPlan"));
			StringAssert.StartsWith(pointsPayPlan,"points:");
			Assert.AreEqual(TreatPlans.GetScaledSignature("10,10;40,30;0,0;80,15;600,140").Split(';').Length,
				pointsPayPlan.Substring("points:".Length).Split(';').Length);
			//With another key nothing is read (the box keeps no points), so the key is what makes the signature valid.
			Assert.AreEqual("points:",ReadSignature(keyHashTreatPlan+"x",GetRecorded("Signature.TreatPlan")));
		}

		///<summary>The RDL engine's data source reference file starts with a random salt, so its bytes differ on every Create: the file recorded by
		///the .NET Framework build is committed, and every build must read it back (Retrieve) to the synthetic connection string.</summary>
		[TestMethod]
		public void KnownAnswers_RdlDataSourceReference_ReadsTheRecord() {
			string pathFixture=Golden.GetFixturePath(FIXTURE_DSR);
			if(Golden.IsRecordMode) {
				string pathTemp=Path.Combine(Path.GetTempPath(),"GoldenOutputTests_"+Guid.NewGuid().ToString("N")+".dsr");
				try {
					DataSourceReference.Create(pathTemp,RDL_CONNECTION,RDL_PASSPHRASE);
					Golden.RecordIfAsked(FIXTURE_DSR,File.ReadAllBytes(pathTemp));
				}
				finally {
					File.Delete(pathTemp);
				}
			}
			Golden.ReadFixture(FIXTURE_DSR);//fails when it is missing
			Assert.AreEqual(RDL_CONNECTION,DataSourceReference.Retrieve(pathFixture,RDL_PASSPHRASE));
			//A file this build writes reads back the same way.
			string pathNew=Path.Combine(Path.GetTempPath(),"GoldenOutputTests_"+Guid.NewGuid().ToString("N")+".dsr");
			try {
				DataSourceReference.Create(pathNew,RDL_CONNECTION,RDL_PASSPHRASE);
				Assert.AreEqual(RDL_CONNECTION,DataSourceReference.Retrieve(pathNew,RDL_PASSPHRASE));
			}
			finally {
				File.Delete(pathNew);
			}
		}
	}
}
