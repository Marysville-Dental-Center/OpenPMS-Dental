// The places the framework, not the code, turns a double into text (S3b-FMT, decision 24 of docs/specs/net10-plan.md,
// docs/port-notes/S3b-FMT.md, "framework places"): each measured on .NET 10 against the .NET Framework's text, and the narrow point that
// makes it equal.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows.Forms.DataVisualization.Charting;
using System.Xml.Serialization;
using CodeBase;
using DataConnectionBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FormatParityTests {
	[TestClass]
	public class FrameworkPlacesTests {
		private static readonly double _sum=0.1+0.2;
		private CultureInfo _cultureInfoOld;

		[TestInitialize]
		public void TestInitialize() {
			_cultureInfoOld=Thread.CurrentThread.CurrentCulture;
			Thread.CurrentThread.CurrentCulture=CultureInfo.GetCultureInfo("en-US");
		}

		[TestCleanup]
		public void TestCleanup() {
			Thread.CurrentThread.CurrentCulture=_cultureInfoOld;
		}

		[TestMethod]
		public void DataTableCell_ObjectToString_IsDotNetsText_AndTheHelperGivesTheNetFrameworks() {
			//A user query's sum is a double cell; object.ToString() of it is .NET's shortest text. MakeReadable (SIn.ByteArray), the user query
			//grid, the report exports and ReportComplex's raw text go through ToStringNetFxIfNumber.
			DataTable table=new DataTable();
			table.Columns.Add("Sum",typeof(double));
			table.Columns.Add("Name");
			table.Rows.Add(_sum,"x");
			Assert.AreEqual("0.30000000000000004",table.Rows[0][0].ToString(),".NET 10's own text.");
			Assert.AreEqual("0.3",ODNumberText.ToStringNetFxIfNumber(table.Rows[0][0]));
			Assert.AreEqual("x",ODNumberText.ToStringNetFxIfNumber(table.Rows[0][1]));
			Assert.AreEqual("0.3",SIn.ByteArray(table.Rows[0][0]),"MakeReadable's cell text.");
			Assert.AreEqual("",ODNumberText.ToStringNetFxIfNumber(DBNull.Value));
		}

		[TestMethod]
		public void DataColumnOfStrings_StoresDotNetsTextOfADouble_SoTheReportsStoreTheHelpersText() {
			//RpInsAging, RpProdInc, RpUnearnedIncome, FormRpProcNotBilledIns, FormRpProdInc and AccountModules stored doubles in string columns;
			//the column converts with .NET's text, so they store ToStringNetFx() now.
			DataTable table=new DataTable();
			table.Columns.Add("Text");
			DataRow row=table.NewRow();
			row["Text"]=_sum;
			Assert.AreEqual("0.30000000000000004",row["Text"],".NET 10's own conversion.");
			row["Text"]=_sum.ToStringNetFx();
			Assert.AreEqual("0.3",row["Text"]);
		}

		[TestMethod]
		public void MiddleTierTable_SendsTheNetFrameworkText_AndXmlSerializerRoundTrips() {
			//XmlConverter.TableToXml sends a double cell as text, which is the client's (string) cell: the .NET Framework's 15 digits.
			DataTable table=new DataTable("t");
			table.Columns.Add("Sum",typeof(double));
			table.Rows.Add(_sum);
			DataTable tableBack=OpenPmsDentalBusiness.XmlConverter.XmlToTable(OpenPmsDentalBusiness.XmlConverter.TableToXml(table));
			Assert.AreEqual("0.3",tableBack.Rows[0][0].ToString());
			//The DTOs (Meth.GetObject's arguments and results) go through XmlSerializer, which writes a double as XmlConvert does: both ends
			//are this build, and every value reads back to the same bits.
			XmlSerializer xmlSerializer=new XmlSerializer(typeof(NumberDto));
			double[] arrayValues=new[] { _sum,1.0/3,1.005,-0.0,double.MaxValue,double.Epsilon,-1e-300,650217936677028.5,double.NaN,
				double.PositiveInfinity,double.NegativeInfinity };
			foreach(double value in arrayValues) {
				NumberDto numberDto=new NumberDto { ValueDouble=value,ValueFloat=(float)value };
				StringWriter stringWriter=new StringWriter();
				xmlSerializer.Serialize(stringWriter,numberDto);
				NumberDto numberDtoBack=(NumberDto)xmlSerializer.Deserialize(new StringReader(stringWriter.ToString()));
				Assert.AreEqual(BitConverter.DoubleToInt64Bits(value),BitConverter.DoubleToInt64Bits(numberDtoBack.ValueDouble),value.ToString("R"));
				Assert.AreEqual(BitConverter.SingleToInt32Bits((float)value),BitConverter.SingleToInt32Bits(numberDtoBack.ValueFloat),value.ToString("R"));
			}
		}

		public class NumberDto {
			public double ValueDouble;
			public float ValueFloat;
		}

		[TestMethod]
		public void ChartLabels_AreWrittenAgainThroughTheHelper() {
			//The chart library formats labels with .NET's formatter: 1.005 with "C" is $1.00 there. ChartNumberText.Attach writes them again.
			Chart chart=new Chart();
			chart.Width=300;
			chart.Height=200;
			chart.ChartAreas.Add(new ChartArea("a"));
			Series series=new Series("s");
			series.ChartArea="a";
			series.ChartType=SeriesChartType.Column;
			series.IsValueShownAsLabel=true;
			series.LabelFormat="C";
			series.Points.AddXY(1,1.005);
			chart.Series.Add(series);
			OpenPmsDentalGraph.ChartNumberText.Attach(chart);
			string label=null;
			chart.FormatNumber+=(sender,e) => {
				if(e.ElementType==ChartElementType.DataPoint) {
					label=e.LocalizedValue;
				}
			};
			using(MemoryStream memoryStream=new MemoryStream()) {
				chart.SaveImage(memoryStream,ChartImageFormat.Png);
			}
			Assert.AreEqual("$1.01",label);
			Assert.IsTrue(OpenPmsDentalGraph.ChartNumberText.IsNumberFormat("C0"));
			Assert.IsTrue(OpenPmsDentalGraph.ChartNumberText.IsNumberFormat("#,##0.00"));
			Assert.IsFalse(OpenPmsDentalGraph.ChartNumberText.IsNumberFormat("MM/dd/yy"));
			Assert.IsFalse(OpenPmsDentalGraph.ChartNumberText.IsNumberValueType(ChartValueType.DateTime));
		}

		[TestMethod]
		public void RdlCStrAndFormat_WriteTheNetFrameworkText() {
			Assert.AreEqual("0.3",fyiReporting.RDL.VBFunctions.CStr(_sum));
			Assert.AreEqual("abc",fyiReporting.RDL.VBFunctions.CStr("abc"));
		}
	}
}
