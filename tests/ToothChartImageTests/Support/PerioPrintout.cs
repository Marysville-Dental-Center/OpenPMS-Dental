// The graphical perio chart's printout and saved image (package TC-7, docs/specs/wpf-tooth-chart-plan.md, the TC-7 row): FormPerioGraphical's
// RenderPerioPrintout copied line for line, so the image tests can draw it around the WPF chart's picture without referencing the program. A source
// test (PerioPrintoutTests) keeps the copy in step with the window.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using OpenPmsDental;
using SparksToothChart;
using SparksToothChart.Wpf3D;

namespace ToothChartImageTests {
	///<summary>The printout of the graphical perio chart as FormPerioGraphical makes it: RenderPerioPrintout draws the title, the clinic, the patient's
	///name, the birthdate (25.2) and the exam date, centred, then the wrapper's GetBitmap scaled into the rest of the page. Printing draws it into the
	///page's margin bounds (Letter at 100 per inch with 50 margins: 750x1000, origin at the margin); Save to Images draws it into a new 750x1000 bitmap
	///cleared to white with high-quality compositing and smoothing. The texts are synthetic.</summary>
	public static class PerioPrintout {
		///<summary>The page's margin bounds and the saved image's size: 750x1000.</summary>
		public static readonly Size SizePage=new Size(750,1000);
		public const string FolderPrint="perio-print";
		public const string FolderSave="perio-save";
		///<summary>The perio scenes whose printout and saved image are baselines.</summary>
		public static readonly string[] ArraySceneNames={ "perio-full-exam","perio-missing-and-implants","perio-recession-term" };
		public const string ClinicName="Synthetic Test Clinic";
		public const string PatientName="Pat Synthetic";
		///<summary>The birthdate and exam date, in en-US's short date format, as the window would print them under that culture.</summary>
		public static readonly string BirthdateText="DOB: "+new DateTime(1970,1,2).ToString("d",CultureInfo.GetCultureInfo("en-US"));
		public static readonly string ExamDateText=new DateTime(2026,11,15).ToString("d",CultureInfo.GetCultureInfo("en-US"));

		///<summary>The print: RenderPerioPrintout into the margin bounds of a white page, with the Graphics' default settings.</summary>
		public static Bitmap RenderPrint(Bitmap bitmapToothChart) {
			Bitmap bitmap=new Bitmap(SizePage.Width,SizePage.Height);
			using(Graphics g=Graphics.FromImage(bitmap)) {
				g.Clear(Color.White);//the paper
				Render(g,bitmapToothChart,new Rectangle(0,0,SizePage.Width,SizePage.Height));
			}
			return bitmap;
		}

		///<summary>Save to Images (FormPerioGraphical.butSave_Click): a new 750x1000 bitmap cleared to white, high-quality compositing and smoothing.</summary>
		public static Bitmap RenderSave(Bitmap bitmapToothChart) {
			Bitmap bitmap=new Bitmap(750,1000);
			using(Graphics g=Graphics.FromImage(bitmap)) {
				g.Clear(Color.White);
				g.CompositingQuality=CompositingQuality.HighQuality;
				g.SmoothingMode=SmoothingMode.HighQuality;
				Render(g,bitmapToothChart,new Rectangle(0,0,bitmap.Width,bitmap.Height));
			}
			return bitmap;
		}

		///<summary>FormPerioGraphical.RenderPerioPrintout, with the synthetic texts and the chart's picture given.</summary>
		public static void Render(Graphics g,Bitmap bitmapToothChart,Rectangle rectangleMarginBounds) {
			float y=20;
			SizeF sizeF;
			using Font fontTitle=new Font("Arial",15);
			string titleStr="Periodontal Examination";
			sizeF=g.MeasureString(titleStr,fontTitle);
			g.DrawString(titleStr,fontTitle,Brushes.Black,new PointF(rectangleMarginBounds.Width/2f-sizeF.Width/2f,y));
			y+=sizeF.Height;
			//Clinic Name
			using Font font=new Font("Arial",11);
			sizeF=g.MeasureString(ClinicName,font);
			g.DrawString(ClinicName,font,Brushes.Black,new PointF(rectangleMarginBounds.Width/2f-sizeF.Width/2f,y));
			y+=sizeF.Height;
			//PatientName
			sizeF=g.MeasureString(PatientName,font);
			g.DrawString(PatientName,font,Brushes.Black,new PointF(rectangleMarginBounds.Width/2f-sizeF.Width/2f,y));
			y+=sizeF.Height;
			//Birthdate on its own line (25.2).
			sizeF=g.MeasureString(BirthdateText,font);
			g.DrawString(BirthdateText,font,Brushes.Black,new PointF(rectangleMarginBounds.Width/2f-sizeF.Width/2f,y));
			y+=sizeF.Height;
			sizeF=g.MeasureString(ExamDateText,font);
			g.DrawString(ExamDateText,font,Brushes.Black,new PointF(rectangleMarginBounds.Width/2f-sizeF.Width/2f,y));
			y+=sizeF.Height;
			SizeF sizeFAvailable=new SizeF(rectangleMarginBounds.Width,rectangleMarginBounds.Height-y);
			float ratioBitmapToOutput=(float)bitmapToothChart.Height/(float)sizeFAvailable.Height;
			if((float)bitmapToothChart.Width/(float)bitmapToothChart.Height //ratio WtoH of bitmap
				> //if bitmap is proportionally wider than page space
				(float)sizeFAvailable.Width/(float)sizeFAvailable.Height) //ratio WtoH of page
			{
				ratioBitmapToOutput=(float)bitmapToothChart.Width/(float)sizeFAvailable.Width;
			}
			SizeF sizeFBitmapToOut=new SizeF((float)bitmapToothChart.Width/ratioBitmapToOutput,(float)bitmapToothChart.Height/ratioBitmapToOutput);
			g.DrawImage(bitmapToothChart,
				x:sizeFAvailable.Width/2f-sizeFBitmapToOut.Width/2f,
				y:y,
				width:sizeFBitmapToOut.Width,
				height:sizeFBitmapToOut.Height);
		}

		///<summary>For the gallery (G3): the print and the saved image of the perio scene the relay's wrapper holds now, under folder\perio-print and
		///folder\perio-save.</summary>
		public static void WriteGalleryPictures(ToothChartRelay relay,ChartScene scene,string folder) {
			using Bitmap bitmapChart=relay.GetBitmap();
			using(Bitmap bitmapPrint=RenderPrint(bitmapChart)) {
				Directory.CreateDirectory(Path.Combine(folder,FolderPrint));
				PngWriter.Save(bitmapPrint,Path.Combine(folder,FolderPrint,scene.Name+".png"));
			}
			using(Bitmap bitmapSave=RenderSave(bitmapChart)) {
				Directory.CreateDirectory(Path.Combine(folder,FolderSave));
				PngWriter.Save(bitmapSave,Path.Combine(folder,FolderSave,scene.Name+".png"));
			}
		}

		///<summary>The print and the saved image of the perio scene the relay's wrapper holds now (its GetBitmap, as RenderPerioPrintout calls it), each
		///drawn twice: recorded into folderBaseline\perio-print and \perio-save (isRecording), or compared with the baselines there. A picture that a
		///second drawing does not repeat is a failure.</summary>
		public static void Check(ToothChartRelay relay,ChartScene scene,bool isRecording,string folderBaseline,string folderFailures,string windows,
			List<string> listFailures,List<string> listIdentical,List<string> listWritten,List<string> listCompared)
		{
			foreach(string folderName in new[] { FolderPrint,FolderSave }) {
				string name=folderName+"/"+scene.Name;
				Func<Bitmap,Bitmap> funcRender=folderName==FolderPrint?(Func<Bitmap,Bitmap>)RenderPrint:RenderSave;
				int countFallbacks=ToothChartWrapper.GetCountFallbackMessages();
				using Bitmap bitmapChart=relay.GetBitmap();
				string failureFallback=WpfCharts.GetFallbackFailure(name,countFallbacks);
				if(failureFallback!=null) {
					listFailures.Add(failureFallback);
					continue;
				}
				using Bitmap bitmap=funcRender(bitmapChart);
				string pathPng=Path.Combine(folderBaseline,folderName,scene.Name+".png");
				if(isRecording) {
					BitmapFacts bitmapFacts=ChartImageCompare.DescribeBitmap(bitmap);
					string pixelsSecond;
					using(Bitmap bitmapChartSecond=relay.GetBitmap())
					using(Bitmap bitmapSecond=funcRender(bitmapChartSecond)) {
						pixelsSecond=ChartImageCompare.DescribeBitmap(bitmapSecond).PixelsSha256;
					}
					if(pixelsSecond!=bitmapFacts.PixelsSha256) {
						listFailures.Add(name+": a second drawing of the same state gave other pixels.");
					}
					Directory.CreateDirectory(Path.GetDirectoryName(pathPng));
					PngWriter.Save(bitmap,pathPng);
					List<string> listLines=new List<string>();
					listLines.Add("Scene: "+scene.Name);
					listLines.Add("Group: "+scene.Group);
					listLines.Add("Description: "+(folderName==FolderPrint?"The print":"The saved image (Save to Images)")+" of the graphical perio chart, as "
						+"FormPerioGraphical.RenderPerioPrintout makes it around the WPF chart's 649x696 picture, with synthetic texts. "+scene.Description);
					listLines.Add("Size: "+SizePage.Width+"x"+SizePage.Height+" ("+folderName+")");
					listLines.Add("Colours: "+ChartColorSet.Perio);
					listLines.Add("Engine: "+ToothChartEngine.Wpf3D+" (the chart's GetBitmap), drawn into the page with GDI+");
					listLines.Add("Bitmap: "+bitmapFacts.Size.Width+"x"+bitmapFacts.Size.Height+" "+bitmapFacts.PixelFormat);
					listLines.Add("Alpha: "+bitmapFacts.GetAlphaText());
					listLines.Add("Colours in the picture: "+bitmapFacts.CountColors);
					listLines.Add("Pixels SHA-256: "+bitmapFacts.PixelsSha256);
					listLines.Add("Repeatable: "+(pixelsSecond==bitmapFacts.PixelsSha256?"yes (a second drawing gave the same pixels)":"no"));
					listLines.Add("Framework: "+WpfImageTests.GetFramework()+", "+WpfImageTests.GetBitness());
					listLines.Add("Windows: "+windows);
					listLines.Add("Screen DPI seen by the test host: "+WpfRendererTests.GetScreenDpi());
					File.WriteAllLines(Path.ChangeExtension(pathPng,".txt"),listLines,new UTF8Encoding(false));
					listWritten.Add(name);
					continue;
				}
				if(!File.Exists(pathPng)) {
					listFailures.Add(name+": no baseline (record with "+WpfImageTests.EnvRecord+"=1).");
					continue;
				}
				string hashBaseline=LegacyComparison.ReadPixelsSha256(Path.ChangeExtension(pathPng,".txt"));
				if(listIdentical!=null && hashBaseline!=null && hashBaseline==ChartImageCompare.Sha256(ChartImageCompare.GetPixelsArgb(bitmap))) {
					listIdentical.Add(name);
				}
				using Bitmap bitmapBaseline=WpfImageTests.LoadBitmap(pathPng);
				ChartImageCompareResult result=ChartImageCompare.Compare(bitmapBaseline,bitmap);
				if(!result.IsPass) {
					string pathDiff=ChartImageCompare.WriteFailureImages(Path.Combine(folderFailures,folderName),scene.Name,bitmapBaseline,bitmap);
					listFailures.Add(name+": "+result+" Images: "+pathDiff);
				}
				listCompared.Add(name);
			}
		}
	}
}
