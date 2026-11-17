// The fixture files of the golden outputs (docs/port-notes/S3a-4.md): compare an output byte for byte with its committed record, or, in record mode,
// write the record anew. The records were written by the .NET Framework 4.8 build; stage 3b of the .NET 10 move compares the .NET 10 build with them
// as they are.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GoldenOutputTests {
	///<summary>Reads, compares and (in record mode) writes the files in tests\GoldenOutputTests\Fixtures of the source tree. The fixtures are read
	///from the source tree, not from the output folder, so a record lands where git sees it and every build (x86 on net48, x64 on net10) compares
	///with the same committed bytes. .gitattributes keeps them byte for byte (-text: no line-end conversion in any checkout).</summary>
	public static class Golden {
		///<summary>OPMS_GOLDEN_RECORD=1 writes every fixture the run reaches anew instead of comparing. Off by default. Only on .NET Framework: the
		///fixtures are the .NET Framework record, so a record on another runtime fails instead of writing.</summary>
		public const string ENV_RECORD="OPMS_GOLDEN_RECORD";
		private const string FIXTURE_FOLDER="Fixtures";
		///<summary>Text fixtures are UTF-8 without a byte order mark.</summary>
		public static readonly Encoding EncodingText=new UTF8Encoding(false);

		public static bool IsRecordMode {
			get {
				return Environment.GetEnvironmentVariable(ENV_RECORD)=="1";
			}
		}

		public static bool IsNetFramework {
			get {
				return RuntimeInformation.FrameworkDescription.StartsWith(".NET Framework",StringComparison.Ordinal);
			}
		}

		///<summary>The folder tests\GoldenOutputTests\Fixtures of the source tree above the test assembly.</summary>
		public static string GetFixtureFolder() {
			DirectoryInfo dir=new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
			while(dir!=null) {
				string candidate=Path.Combine(dir.FullName,"tests","GoldenOutputTests");
				if(File.Exists(Path.Combine(candidate,"GoldenOutputTests.csproj"))) {
					return Path.Combine(candidate,FIXTURE_FOLDER);
				}
				dir=dir.Parent;
			}
			throw new DirectoryNotFoundException("tests\\GoldenOutputTests not found above "+AppDomain.CurrentDomain.BaseDirectory);
		}

		public static string GetFixturePath(string fixtureName) {
			if(fixtureName.Contains("..") || Path.IsPathRooted(fixtureName)) {
				throw new ArgumentException("A fixture name is a file name under Fixtures: "+fixtureName);
			}
			return Path.Combine(GetFixtureFolder(),fixtureName);
		}

		///<summary>The committed bytes of a fixture. Fails when it is missing (not in record mode).</summary>
		public static byte[] ReadFixture(string fixtureName) {
			string path=GetFixturePath(fixtureName);
			if(!File.Exists(path)) {
				Assert.Fail("The fixture "+fixtureName+" is missing. It is recorded with the .NET Framework build and "+ENV_RECORD+"=1 (docs/port-notes/S3a-4.md).");
			}
			return File.ReadAllBytes(path);
		}

		public static string ReadFixtureText(string fixtureName) {
			return EncodingText.GetString(ReadFixture(fixtureName));
		}

		///<summary>Compares the text, as UTF-8 without a byte order mark, with the fixture byte for byte; in record mode writes it.</summary>
		public static void AssertText(string fixtureName,string actual) {
			AssertBytes(fixtureName,EncodingText.GetBytes(actual));
		}

		///<summary>Compares the bytes with the fixture; in record mode writes them as the fixture. A difference fails with the first differing line of
		///text (or byte) and the run's output written next to the test assembly as &lt;name&gt;.actual for a diff.</summary>
		public static void AssertBytes(string fixtureName,byte[] actual) {
			if(RecordIfAsked(fixtureName,actual)) {
				return;
			}
			byte[] expected=ReadFixture(fixtureName);
			if(expected.SequenceEqual(actual)) {
				return;
			}
			string pathActual=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"GoldenActual",fixtureName+".actual");
			try {
				Directory.CreateDirectory(Path.GetDirectoryName(pathActual));
				File.WriteAllBytes(pathActual,actual);
			}
			catch(Exception) {
				pathActual="(could not be written)";
			}
			Assert.Fail(fixtureName+" differs from the .NET Framework record: "+DescribeDifference(expected,actual)+" This run's output: "+pathActual);
		}

		///<summary>In record mode writes the fixture and returns true. Record mode on another runtime than .NET Framework fails.</summary>
		public static bool RecordIfAsked(string fixtureName,byte[] actual) {
			if(!IsRecordMode) {
				return false;
			}
			if(!IsNetFramework) {
				Assert.Fail("The golden fixtures are the .NET Framework 4.8 record; "+ENV_RECORD+"=1 is refused on "+RuntimeInformation.FrameworkDescription+".");
			}
			string path=GetFixturePath(fixtureName);
			Directory.CreateDirectory(Path.GetDirectoryName(path));
			File.WriteAllBytes(path,actual);
			Console.WriteLine("Recorded "+fixtureName+" ("+actual.Length+" bytes).");
			return true;
		}

		private static string DescribeDifference(byte[] expected,byte[] actual) {
			int index=0;
			while(index<expected.Length && index<actual.Length && expected[index]==actual[index]) {
				index++;
			}
			string textExpected=EncodingText.GetString(expected);
			string textActual=EncodingText.GetString(actual);
			string[] arrayLinesExpected=textExpected.Split('\n');
			string[] arrayLinesActual=textActual.Split('\n');
			for(int i=0;i<Math.Max(arrayLinesExpected.Length,arrayLinesActual.Length);i++) {
				string lineExpected=i<arrayLinesExpected.Length ? arrayLinesExpected[i] : "(no line)";
				string lineActual=i<arrayLinesActual.Length ? arrayLinesActual[i] : "(no line)";
				if(lineExpected!=lineActual) {
					return "first difference at byte "+index+" (expected "+expected.Length+" bytes, got "+actual.Length+"), line "+(i+1)+": expected ["
						+Visible(lineExpected)+"], got ["+Visible(lineActual)+"].";
				}
			}
			return "first difference at byte "+index+" (expected "+expected.Length+" bytes, got "+actual.Length+").";
		}

		private static string Visible(string text) {
			return text.Replace("\r","\\r").Replace("\t","\\t");
		}

		///<summary>Bytes as lowercase hex, 32 bytes to a line, for binary values in text fixtures.</summary>
		public static string ToHex(byte[] arrayBytes) {
			StringBuilder stringBuilder=new StringBuilder();
			for(int i=0;i<arrayBytes.Length;i++) {
				if(i>0 && i%32==0) {
					stringBuilder.Append("\n");
				}
				stringBuilder.Append(arrayBytes[i].ToString("x2",CultureInfo.InvariantCulture));
			}
			return stringBuilder.ToString();
		}

		///<summary>en-US as the program's culture-bound outputs are recorded: the read-only culture (CultureInfo.GetCultureInfo), so no regional
		///override of the test PC changes a text. For the business layer's thread and for new threads.</summary>
		public static void UseUsEnglish() {
			CultureInfo cultureInfo=CultureInfo.GetCultureInfo("en-US");
			Thread.CurrentThread.CurrentCulture=cultureInfo;
			Thread.CurrentThread.CurrentUICulture=cultureInfo;
			CultureInfo.DefaultThreadCurrentCulture=cultureInfo;
			CultureInfo.DefaultThreadCurrentUICulture=cultureInfo;
		}
	}
}
