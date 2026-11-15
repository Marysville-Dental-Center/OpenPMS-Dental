// The ANSI code page, named explicitly. Encoding.Default is the ANSI code page on .NET Framework but UTF-8 on .NET (Core and 5+), so code that
// means "the Windows ANSI page" asks for it here instead (net10 plan, package S3a-3).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System.Globalization;
using System.Text;

namespace CodeBase {
	public static class ODEncodings {
		///<summary>The ANSI code page of the current culture (1252 for English and Western European cultures), for text that Windows programs
		///and spreadsheet exports write in the ANSI page. On .NET Framework this is the page Encoding.Default gave wherever the culture and the
		///Windows system locale share their ANSI page. A culture without an ANSI page reports 0, and page 0 is the system's default page on
		///.NET Framework, as Encoding.Default was. On .NET the code page provider must be registered first (stage 3b, ODInitialize), because
		///only UTF-8, UTF-16, ASCII and Latin-1 are built in there.</summary>
		public static Encoding GetAnsiEncoding() {
			return Encoding.GetEncoding(CultureInfo.CurrentCulture.TextInfo.ANSICodePage);
		}
	}
}
