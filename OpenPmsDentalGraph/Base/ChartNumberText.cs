// The number labels of the charts (axis labels and point labels: "C0", "C" and the formats the graphs set) as the .NET Framework wrote them
// (package S3b-FMT of the .NET 10 move, decision 24 of docs/specs/net10-plan.md, docs/port-notes/S3b-FMT.md). The chart library formats a
// number label with .NET's own formatter, which formats the exact binary value (1.005 with "C" is $1.00 there, $1.01 on the .NET Framework)
// and keeps the sign of a small negative value that rounds to zero ("($0)" against "$0" with "C0"); its FormatNumber event is the one place
// every label passes, and the label is written again there through ODNumberText.ToStringNetFx. Dates (the date axes' labels) are left as the
// chart writes them: a date's text did not change.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System.Globalization;
using System.Windows.Forms.DataVisualization.Charting;
using CodeBase;

namespace OpenPmsDentalGraph {
	public static class ChartNumberText {
		///<summary>Writes every number label of the chart as the .NET Framework wrote it. Call once, after InitializeComponent.</summary>
		public static void Attach(Chart chart) {
			chart.FormatNumber-=Chart_FormatNumber;
			chart.FormatNumber+=Chart_FormatNumber;
		}

		private static void Chart_FormatNumber(object sender,FormatNumberEventArgs e) {
			if(!IsNumberValueType(e.ValueType) || !IsNumberFormat(e.Format)) {
				return;
			}
			e.LocalizedValue=e.Value.ToStringNetFx(string.IsNullOrEmpty(e.Format) ? null : e.Format,CultureInfo.CurrentCulture);
		}

		///<summary>The value types the chart writes as numbers (not dates or times, which it writes from the date the double stands for).</summary>
		public static bool IsNumberValueType(ChartValueType chartValueType) {
			switch(chartValueType) {
				case ChartValueType.Auto:
				case ChartValueType.Double:
				case ChartValueType.Single:
				case ChartValueType.Int32:
				case ChartValueType.Int64:
				case ChartValueType.UInt32:
				case ChartValueType.UInt64:
					return true;
			}
			return false;
		}

		///<summary>True for no format, a standard number format (a letter and at most two digits) or a custom number format (only 0 # . , % ;
		///E e + - and quoted text); false for a date format such as "MM/dd/yy", which a chart of ChartValueType.Auto may also be given.</summary>
		public static bool IsNumberFormat(string format) {
			if(string.IsNullOrEmpty(format)) {
				return true;
			}
			char first=char.ToUpperInvariant(format[0]);
			if("CDEFGNPRX".IndexOf(first)>=0 && format.Length<=3) {
				for(int i=1;i<format.Length;i++) {
					if(!char.IsDigit(format[i])) {
						return false;
					}
				}
				return true;
			}
			bool isQuoted=false;
			char quote='\0';
			for(int i=0;i<format.Length;i++) {
				char ch=format[i];
				if(isQuoted) {
					if(ch==quote) {
						isQuoted=false;
					}
					continue;
				}
				if(ch=='\'' || ch=='"') {
					isQuoted=true;
					quote=ch;
					continue;
				}
				if(ch=='\\') {
					i++;
					continue;
				}
				if(char.IsLetter(ch) && ch!='E' && ch!='e') {
					return false;
				}
			}
			return true;
		}
	}
}
