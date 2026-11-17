// The code pages after ODInitialize.Initialize() on .NET 10 (the plan's section 7, "Encodings", 3b; S3b-2 put the registration first in
// Initialize, S3b-5 adds the test): .NET has only the Unicode encodings, ASCII and Latin-1 built in, and the program registers the Windows code
// pages (CodePagesEncodingProvider) first in ODInitialize.Initialize(). PDFsharp registers the provider itself, so a missing registration would
// show only until the first PDF code ran, as failures that look intermittent. So the check runs in a fresh process (TestProbeHost run once,
// with NO_CODE_PAGES: the host leaves the code pages unregistered, and the test assembly's own registration never runs there): before Initialize
// Encoding.GetEncoding(1252) throws; after it 1252 (ANSI text) and 850 (Canada) decode and encode known bytes; and no PdfSharp assembly is
// loaded. No database, no window.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.IO;
using System.Linq;
using System.Text;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestProbeHost;

namespace NoHqTests {
	[TestClass]
	public class EncodingsAfterInitializeTests {
		[TestMethod]
		public void CodePages1252And850_WorkRightAfterODInitialize_BeforeAnyPdfCodeRan() {
			string folder=Path.GetDirectoryName(typeof(EncodingsAfterInitializeTests).Assembly.Location);
			ProbeRunResult probeRunResult=ProbeProcess.RunOnce(folder,null,typeof(EncodingsAfterInitializeProbe),new string[0],TimeSpan.FromMinutes(2),
				ProbeProcess.TimeSpanOutputEnd,true);
			string output=probeRunResult.Output;
			Assert.IsFalse(probeRunResult.IsTimedOut,output);
			Assert.AreEqual(0,probeRunResult.ExitCode,output);
			string[] arrayLines=output.Split('\n').Select(x => x.Trim()).Where(x => x!="").ToArray();
			CollectionAssert.Contains(arrayLines,"BEFORE|NotSupportedException","Without the registration, 1252 is not there: "+output);
			CollectionAssert.Contains(arrayLines,"INITIALIZED","ODInitialize.Initialize() ran: "+output);
			//1252: euro sign, e acute, oe ligature, A.
			CollectionAssert.Contains(arrayLines,"1252|20AC 00E9 0153 0041|roundtrip",output);
			//850: e acute, c cedilla, o slash, sharp s.
			CollectionAssert.Contains(arrayLines,"850|00E9 00E7 00F8 00DF|roundtrip",output);
			CollectionAssert.Contains(arrayLines,"PDFSHARP|none","No PDF code ran in the process: "+output);
		}
	}

	///<summary>The child process of the encodings test (TestProbeHost run once, the code pages left unregistered by the host).</summary>
	public static class EncodingsAfterInitializeProbe {
		public static int Run(string[] args) {
			try {
				Encoding.GetEncoding(1252);
				Console.WriteLine("BEFORE|registered");
			}
			catch(NotSupportedException) {
				Console.WriteLine("BEFORE|NotSupportedException");
			}
			ODInitialize.Initialize();
			Console.WriteLine("INITIALIZED");
			Console.WriteLine(RoundTrip(1252,new byte[] { 0x80,0xE9,0x9C,0x41 }));
			Console.WriteLine(RoundTrip(850,new byte[] { 0x82,0x87,0x9B,0xE1 }));
			string[] arrayPdfSharp=AppDomain.CurrentDomain.GetAssemblies().Select(x => x.GetName().Name)
				.Where(x => x.StartsWith("PdfSharp",StringComparison.OrdinalIgnoreCase)).ToArray();
			Console.WriteLine("PDFSHARP|"+(arrayPdfSharp.Length==0 ? "none" : string.Join(",",arrayPdfSharp)));
			return 0;
		}

		///<summary>"codepage|the decoded characters as hex|roundtrip" when the bytes encode back to themselves, or the exception.</summary>
		private static string RoundTrip(int codePage,byte[] arrayBytes) {
			try {
				Encoding encoding=Encoding.GetEncoding(codePage);
				string text=encoding.GetString(arrayBytes);
				bool isRoundTrip=encoding.GetBytes(text).SequenceEqual(arrayBytes);
				return codePage+"|"+string.Join(" ",text.Select(x => ((int)x).ToString("X4")))+"|"+(isRoundTrip ? "roundtrip" : "changed");
			}
			catch(Exception ex) {
				return codePage+"|"+ex.GetType().Name+": "+ex.Message;
			}
		}
	}
}
