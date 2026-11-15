// The ANSI code page, named explicitly. Encoding.Default is the ANSI code page on .NET Framework but UTF-8 on .NET (Core and 5+), so code that
// means "the Windows ANSI page" asks for it here instead (net10 plan, package S3a-3).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System.Runtime.InteropServices;
using System.Text;

namespace CodeBase {
	public static class ODEncodings {
		///<summary>The ANSI code page of the Windows system locale (GetACP: 1252 on an English or Western European Windows), for text that Windows
		///programs and spreadsheet exports write in the ANSI page. This is exactly what Encoding.Default is on .NET Framework, which also asks
		///GetACP, so on net48 it is a no-op for every thread culture and every user format. It does not follow CultureInfo.CurrentCulture: the
		///culture's ANSICodePage can differ from the system page, and on .NET a culture without one reports 0, which GetEncoding turns into
		///UTF-8. On .NET the code page provider (CodePagesEncodingProvider) must be registered first (stage 3b, ODInitialize and each Main),
		///because only UTF-8, UTF-16, UTF-32, ASCII and Latin-1 are built in there.</summary>
		public static Encoding GetAnsiEncoding() {
			return Encoding.GetEncoding(GetACP());
		}

		///<summary>The system locale's ANSI code page. https://learn.microsoft.com/windows/win32/api/winnls/nf-winnls-getacp</summary>
		[DllImport("kernel32.dll")]
		private static extern int GetACP();
	}
}
