// The pictures of gallery G-UI1 part 3 (package UI-7b of docs/specs/ui-refresh-plan.md, section 8; docs/port-notes/UI-7b.md): every EnumIcons of
// both IconLibrary folders as the program draws it, at the 16, 20 and 24 px slots (and 32 px for the module bar's icons) at 100, 125, 150 and 200%,
// off screen. The same test made the before set on main (today's Gen_*.cs pictures, before UI-7b removed them) and makes the after set on the
// package's branch (the Fluent icons), so both sets come from one drawing rule: a white square of the slot's real pixels and the library's own
// call, IconLibrary.Draw for a WinForms icon (Direct2D into a bitmap, as the program's buttons, toolbars and module bar draw) and, for an icon only
// the WPF library draws, what its DrawWpf puts in the grid. Each run checks that every icon draws something; with OPMS_UI7B_ICON_PICTURES set to a
// folder it also writes the PNGs there (icons\<name>\<slot>px-<scale>.png and one sheet per scale, sheet-<scale>.png) for tools/New-UiGallery.ps1.
// No window, no database, no network. Needs Direct2dWrapper.dll (the full build); inconclusive without it.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WinEnumIcons=OpenPmsDental.UI.EnumIcons;
using WinIconLibrary=OpenPmsDental.UI.IconLibrary;
using WpfEnumIcons=WpfControls.UI.EnumIcons;
using WpfIconLibrary=WpfControls.UI.IconLibrary;

namespace UiRefreshTests {
	[TestClass]
	public class IconLibraryPictureTests {
		///<summary>The folder the pictures are written to, when set.</summary>
		public const string VARIABLE_PICTURES="OPMS_UI7B_ICON_PICTURES";
		///<summary>The slots of the row's gallery, in 96 DPI pixels.</summary>
		public static readonly int[] ArraySlots96=new int[] { 16,20,24 };
		///<summary>The slot of the module bar's icons (IconLibrary.Width 32), in 96 DPI pixels, pictured besides.</summary>
		public const int SLOT_MODULE96=32;
		///<summary>The scales of the row's gallery.</summary>
		public static readonly float[] ArrayScales=new float[] { 1f,1.25f,1.5f,2f };

		///<summary>Every EnumIcons name of both libraries but None, in the WPF enum's order (it holds every WinForms member).</summary>
		public static List<string> GetAllNames() {
			List<string> listNames=Enum.GetNames(typeof(WpfEnumIcons)).Where(x => x!="None").ToList();
			foreach(string name in Enum.GetNames(typeof(WinEnumIcons)).Where(x => x!="None")) {
				Assert.IsTrue(listNames.Contains(name),"The WinForms EnumIcons."+name+" is not in the WPF enum.");
			}
			return listNames;
		}

		///<summary>Whether the icon is a module bar icon (IconLibrary.Width 32 in the library that draws it).</summary>
		public static bool IsModuleIcon(string name) {
			WinEnumIcons winEnumIcons;
			if(Enum.TryParse(name,out winEnumIcons)) {
				return WinIconLibrary.Width(winEnumIcons)==32;
			}
			return WpfIconLibrary.Width((WpfEnumIcons)Enum.Parse(typeof(WpfEnumIcons),name))==32;
		}

		///<summary>The slots an icon is pictured at, in 96 DPI pixels.</summary>
		public static int[] GetSlots96(string name) {
			if(IsModuleIcon(name)) {
				return ArraySlots96.Concat(new[] { SLOT_MODULE96 }).ToArray();
			}
			return ArraySlots96;
		}

		///<summary>The scale as the file names give it: 100, 125, 150, 200.</summary>
		public static string GetScaleText(float scale) {
			return ((int)Math.Round(scale*100)).ToString();
		}

		///<summary>A new white bitmap of sizePixels square with the icon drawn by its library: the WinForms IconLibrary.Draw when the WinForms enum
		///has the name and draws something (before UI-7b two WinForms members, ImageSelectorFile and ImageSelectorMount, were empty: only the WPF
		///image selector draws them), otherwise what the WPF IconLibrary.DrawWpf puts in a grid of the WPF default 22 DIPs: a bitmap drawn into the
		///slot, the old vector shapes (their geometry filled with GDI+, since WPF draws nothing in a disconnected session) or a Fluent icon's
		///design. Call on an STA thread.</summary>
		public static Bitmap DrawLibraryIcon(string name,int sizePixels) {
			WinEnumIcons winEnumIcons;
			if(Enum.TryParse(name,out winEnumIcons)) {
				Bitmap bitmapWin=NewWhiteBitmap(sizePixels);
				using(Graphics g=Graphics.FromImage(bitmapWin)) {
					WinIconLibrary.Draw(g,winEnumIcons,new Rectangle(0,0,sizePixels,sizePixels),Color.White,Color.White);
				}
				if(IconRendererTests.GetInk(bitmapWin,isOnWhite:true).Any(x => x>0)) {
					return bitmapWin;
				}
				bitmapWin.Dispose();
			}
			Bitmap bitmap=NewWhiteBitmap(sizePixels);
			using(Graphics g=Graphics.FromImage(bitmap)) {
				System.Windows.Controls.Grid grid=new System.Windows.Controls.Grid();
				grid.Width=22;
				grid.Height=22;
				WpfIconLibrary.DrawWpf((WpfEnumIcons)Enum.Parse(typeof(WpfEnumIcons),name),grid);
				if(grid.Children.Count==0) {
					return bitmap;//before UI-7b the WPF ImageSelectorMount was empty too: the image selector showed no mount icon
				}
				Assert.AreEqual(1,grid.Children.Count,"WPF EnumIcons."+name+": DrawWpf put "+grid.Children.Count+" elements in the grid.");
				object child=grid.Children[0];
				if(child is System.Windows.Controls.Image image) {
					System.Windows.Media.Imaging.BitmapSource bitmapSource=image.Source as System.Windows.Media.Imaging.BitmapSource;
					Assert.IsNotNull(bitmapSource,"WPF EnumIcons."+name+": the image has no bitmap.");
					using(Bitmap bitmapIcon=ConvertToGdi(bitmapSource)) {
						g.InterpolationMode=InterpolationMode.HighQualityBicubic;
						g.PixelOffsetMode=PixelOffsetMode.Half;
						g.DrawImage(bitmapIcon,new Rectangle(0,0,sizePixels,sizePixels));
					}
					return bitmap;
				}
				if(child is System.Windows.Shapes.Path path) {
					//A Fluent icon: the same design the geometry was made from, filled with GDI+ in the path's colour.
					System.Windows.Media.SolidColorBrush solidColorBrush=path.Fill as System.Windows.Media.SolidColorBrush;
					Assert.IsNotNull(solidColorBrush,"WPF EnumIcons."+name+": the path has no solid fill.");
					IconDesign iconDesign=UiIcons.GetDesign(UiIcons.GetDesigns("EnumIcons",name),sizePixels);
					Assert.IsNotNull(iconDesign,"WPF EnumIcons."+name+" has no design in the table.");
					UiIcons.Draw(g,iconDesign,new Rectangle(0,0,sizePixels,sizePixels),ToGdi(solidColorBrush.Color));
					return bitmap;
				}
				if(child is System.Windows.Controls.Canvas) {
					DrawShapes(g,grid,sizePixels/22.0);
					return bitmap;
				}
				Assert.Fail("WPF EnumIcons."+name+": DrawWpf put a "+child.GetType().Name+" in the grid, which this test cannot picture.");
			}
			return bitmap;
		}

		private static Bitmap NewWhiteBitmap(int sizePixels) {
			Bitmap bitmap=new Bitmap(sizePixels,sizePixels,PixelFormat.Format32bppArgb);
			bitmap.SetResolution(96,96);
			using(Graphics g=Graphics.FromImage(bitmap)) {
				g.Clear(Color.White);
			}
			return bitmap;
		}

		private static Color ToGdi(System.Windows.Media.Color color) {
			return Color.FromArgb(color.A,color.R,color.G,color.B);
		}

		///<summary>A WPF bitmap as a new GDI+ bitmap (through PNG).</summary>
		private static Bitmap ConvertToGdi(System.Windows.Media.Imaging.BitmapSource bitmapSource) {
			//Through WPF's own conversion to 32-bit BGRA, so a palette picture's transparent colour stays transparent (as WPF shows it).
			System.Windows.Media.Imaging.FormatConvertedBitmap formatConvertedBitmap=new System.Windows.Media.Imaging.FormatConvertedBitmap(bitmapSource,
				System.Windows.Media.PixelFormats.Bgra32,null,0);
			int width=formatConvertedBitmap.PixelWidth;
			int height=formatConvertedBitmap.PixelHeight;
			byte[] arrayBytes=new byte[width*height*4];
			formatConvertedBitmap.CopyPixels(arrayBytes,width*4,0);
			Bitmap bitmap=new Bitmap(width,height,PixelFormat.Format32bppArgb);
			BitmapData bitmapData=bitmap.LockBits(new Rectangle(0,0,width,height),ImageLockMode.WriteOnly,PixelFormat.Format32bppArgb);
			for(int y=0;y<height;y++) {
				System.Runtime.InteropServices.Marshal.Copy(arrayBytes,y*width*4,bitmapData.Scan0+y*bitmapData.Stride,width*4);
			}
			bitmap.UnlockBits(bitmapData);
			return bitmap;
		}

		///<summary>The old WPF vector icons (DrawSvg's shapes on a scaled Canvas) filled with GDI+: the grid is laid out (no rendering needed), and
		///each shape's geometry, moved to the grid by its transform, is flattened and filled in its fill, and its outline widened and filled in its
		///stroke. scale is device pixels per DIP.</summary>
		private static void DrawShapes(Graphics g,System.Windows.Controls.Grid grid,double scale) {
			grid.Measure(new System.Windows.Size(grid.Width,grid.Height));
			grid.Arrange(new System.Windows.Rect(0,0,grid.Width,grid.Height));
			grid.UpdateLayout();
			GraphicsState graphicsState=g.Save();
			g.SmoothingMode=SmoothingMode.AntiAlias;
			g.PixelOffsetMode=PixelOffsetMode.Half;
			foreach(System.Windows.Shapes.Shape shape in FindShapes(grid)) {
				System.Windows.Media.Transform transform=shape.TransformToAncestor(grid) as System.Windows.Media.Transform;
				Assert.IsNotNull(transform,"A shape's transform is not affine.");
				System.Windows.Media.Matrix matrix=transform.Value;
				matrix.Scale(scale,scale);
				System.Windows.Media.Geometry geometry=shape.RenderedGeometry.Clone();
				geometry.Transform=new System.Windows.Media.MatrixTransform(matrix);
				if(shape.Fill is System.Windows.Media.SolidColorBrush solidColorBrushFill) {
					FillGeometry(g,geometry,ToGdi(solidColorBrushFill.Color));
				}
				if(shape.Stroke is System.Windows.Media.SolidColorBrush solidColorBrushStroke && shape.StrokeThickness>0) {
					System.Windows.Media.Pen pen=new System.Windows.Media.Pen(solidColorBrushStroke,shape.StrokeThickness*Math.Sqrt(Math.Abs(matrix.Determinant)));
					FillGeometry(g,geometry.GetFlattenedPathGeometry().GetWidenedPathGeometry(pen),ToGdi(solidColorBrushStroke.Color));
				}
			}
			g.Restore(graphicsState);
		}

		private static List<System.Windows.Shapes.Shape> FindShapes(System.Windows.DependencyObject dependencyObject) {
			List<System.Windows.Shapes.Shape> listShapes=new List<System.Windows.Shapes.Shape>();
			foreach(object child in System.Windows.LogicalTreeHelper.GetChildren(dependencyObject)) {
				if(child is System.Windows.Shapes.Shape shape) {
					listShapes.Add(shape);
				}
				else if(child is System.Windows.DependencyObject dependencyObjectChild) {
					listShapes.AddRange(FindShapes(dependencyObjectChild));
				}
			}
			return listShapes;
		}

		///<summary>Fills a WPF geometry (in device pixels) with GDI+: flattened to polygons, with the geometry's fill rule.</summary>
		private static void FillGeometry(Graphics g,System.Windows.Media.Geometry geometry,Color color) {
			System.Windows.Media.PathGeometry pathGeometry=geometry.GetFlattenedPathGeometry(0.05,System.Windows.Media.ToleranceType.Absolute);
			FillMode fillMode=pathGeometry.FillRule==System.Windows.Media.FillRule.Nonzero ? FillMode.Winding : FillMode.Alternate;
			using(GraphicsPath graphicsPath=new GraphicsPath(fillMode))
			using(SolidBrush solidBrush=new SolidBrush(color)) {
				foreach(System.Windows.Media.PathFigure pathFigure in pathGeometry.Figures) {
					List<PointF> listPoints=new List<PointF> { new PointF((float)pathFigure.StartPoint.X,(float)pathFigure.StartPoint.Y) };
					foreach(System.Windows.Media.PathSegment pathSegment in pathFigure.Segments) {
						if(pathSegment is System.Windows.Media.PolyLineSegment polyLineSegment) {
							listPoints.AddRange(polyLineSegment.Points.Select(x => new PointF((float)x.X,(float)x.Y)));
						}
						else if(pathSegment is System.Windows.Media.LineSegment lineSegment) {
							listPoints.Add(new PointF((float)lineSegment.Point.X,(float)lineSegment.Point.Y));
						}
					}
					if(listPoints.Count>=3) {
						graphicsPath.AddPolygon(listPoints.ToArray());
					}
				}
				g.FillPath(solidBrush,graphicsPath);
			}
		}

		[TestMethod]
		public void Pictures_EveryEnumIconsDrawsAtEverySlotAndScale() {
			IconRendererTests.RequireDirect2d();
			string folder=Environment.GetEnvironmentVariable(VARIABLE_PICTURES);
			List<string> listNames=GetAllNames();
			IconRendererTests.RunSta(() => {
				bool isOnlyGdiOld=WinIconLibrary.OnlyGDI;
				WinIconLibrary.OnlyGDI=false;
				try {
					foreach(float scale in ArrayScales) {
						Dictionary<string,Bitmap> dictPictures=new Dictionary<string,Bitmap>();
						try {
							foreach(string name in listNames) {
								foreach(int slot96 in GetSlots96(name)) {
									int sizePixels=UiTheme.ScaleI(slot96,scale);
									Bitmap bitmap=DrawLibraryIcon(name,sizePixels);
									dictPictures.Add(name+"|"+slot96,bitmap);
									int[] arrayInk=IconRendererTests.GetInk(bitmap,isOnWhite:true);
									//Before UI-7b (no EnumIcons rows in the table) ImageSelectorMount drew nothing in either library; with the rows every icon must draw.
									Assert.IsTrue(arrayInk.Any(x => x>=32) || UiIcons.GetDesigns("EnumIcons",name).Length==0,"EnumIcons."+name+" draws nothing at "+slot96+" px, "+GetScaleText(scale)+"%.");
									if(!string.IsNullOrEmpty(folder)) {
										string folderIcon=Path.Combine(folder,"icons",name);
										Directory.CreateDirectory(folderIcon);
										bitmap.Save(Path.Combine(folderIcon,slot96+"px-"+GetScaleText(scale)+".png"),ImageFormat.Png);
									}
								}
							}
							Assert.IsFalse(WinIconLibrary.OnlyGDI,"Direct2D failed and IconLibrary fell back to GDI+.");
							if(!string.IsNullOrEmpty(folder)) {
								using(Bitmap bitmapSheet=DrawSheet(listNames,dictPictures,scale)) {
									bitmapSheet.Save(Path.Combine(folder,"sheet-"+GetScaleText(scale)+".png"),ImageFormat.Png);
								}
							}
						}
						finally {
							foreach(Bitmap bitmap in dictPictures.Values) {
								bitmap.Dispose();
							}
						}
					}
				}
				finally {
					WinIconLibrary.OnlyGDI=isOnlyGdiOld;
				}
			});
		}

		///<summary>One sheet of every icon at one scale: a row per icon with its name, then a cell per slot (16, 20, 24 and the module bar's 32 px),
		///each the slot's real pixels on white with a 6 px margin. The layout depends only on the names and the scale, so a before and an after
		///sheet line up pixel for pixel.</summary>
		public static Bitmap DrawSheet(List<string> listNames,Dictionary<string,Bitmap> dictPictures,float scale) {
			const int widthName=190;
			const int margin=6;
			int[] arraySlots=ArraySlots96.Concat(new[] { SLOT_MODULE96 }).ToArray();
			int[] arrayCellWidths=arraySlots.Select(x => UiTheme.ScaleI(x,scale)+2*margin).ToArray();
			int heightRow=UiTheme.ScaleI(SLOT_MODULE96,scale)+2*margin;
			int heightHeader=24;
			int width=widthName+arrayCellWidths.Sum();
			int height=heightHeader+heightRow*listNames.Count;
			Bitmap bitmapSheet=new Bitmap(width,height,PixelFormat.Format32bppArgb);
			bitmapSheet.SetResolution(96,96);
			using(Graphics g=Graphics.FromImage(bitmapSheet))
			using(Font font=new Font("Segoe UI",9f,FontStyle.Regular,GraphicsUnit.Point))
			using(Pen penLine=new Pen(Color.FromArgb(224,224,224))) {
				g.Clear(Color.White);
				g.TextRenderingHint=TextRenderingHint.ClearTypeGridFit;
				g.DrawString(GetScaleText(scale)+"%",font,Brushes.Black,4,4);
				int x=widthName;
				for(int c=0;c<arraySlots.Length;c++) {
					g.DrawString(arraySlots[c]+" px",font,Brushes.Black,x+2,4);
					x+=arrayCellWidths[c];
				}
				for(int r=0;r<listNames.Count;r++) {
					int y=heightHeader+r*heightRow;
					g.DrawLine(penLine,0,y,width,y);
					g.DrawString(listNames[r],font,Brushes.Black,4,y+(heightRow-font.Height)/2);
					x=widthName;
					for(int c=0;c<arraySlots.Length;c++) {
						Bitmap bitmap;
						if(dictPictures.TryGetValue(listNames[r]+"|"+arraySlots[c],out bitmap)) {
							g.DrawImageUnscaled(bitmap,x+margin,y+(heightRow-bitmap.Height)/2);
						}
						x+=arrayCellWidths[c];
					}
				}
			}
			return bitmapSheet;
		}
	}
}
