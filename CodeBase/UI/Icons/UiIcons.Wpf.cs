// The WPF part of the icon renderer (docs/specs/ui-refresh-plan.md, section 4.2, item 3; package UI-7a): a design as a frozen StreamGeometry
// (fill rule nonzero), in DIPs, chosen for the slot's real pixel size (section 3.5), cached and shared by every thread; DrawWpf and
// GetImageSourceWpf.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace CodeBase {
	public static partial class UiIcons {
		///<summary>The frozen geometries, by design, slot pixels and scale. Frozen objects may be used on any thread.</summary>
		private static readonly Dictionary<WpfKey,StreamGeometry> _dictGeometriesWpf=new Dictionary<WpfKey,StreamGeometry>();
		private static readonly object _lockWpf=new object();

		///<summary>The glyph's geometry for a square slot of sizeSlot DIPs at the scale (device pixels per DIP, 1.5 at 144 DPI): the design for the
		///slot's real pixel size, round(sizeSlot*scale) (section 3.5), centred in the slot on whole device pixels, in DIPs from the slot's top left
		///corner. Frozen and cached. An empty geometry for None.</summary>
		public static Geometry GetGeometryWpf(UiGlyph uiGlyph,double sizeSlot,double scale=1) {
			scale=GetScaleWpf(scale);
			int sizePixels=(int)Math.Round(sizeSlot*scale);
			return GetGeometryWpf(GetDesign(uiGlyph,sizePixels),sizeSlot,scale);
		}

		///<summary>A design's geometry for a square slot of sizeSlot DIPs at the scale (GetGeometryWpf), placed by GetDrawRectangle at the slot's real
		///pixel size. Frozen and cached. An empty geometry for null.</summary>
		public static Geometry GetGeometryWpf(IconDesign iconDesign,double sizeSlot,double scale=1) {
			scale=GetScaleWpf(scale);
			int sizePixels=(int)Math.Round(sizeSlot*scale);
			if(iconDesign==null || sizePixels<=0) {
				return Geometry.Empty;
			}
			WpfKey wpfKey=new WpfKey(iconDesign,sizePixels,scale);
			lock(_lockWpf) {
				StreamGeometry streamGeometry;
				if(_dictGeometriesWpf.TryGetValue(wpfKey,out streamGeometry)) {
					return streamGeometry;
				}
			}
			StreamGeometry streamGeometryNew=new StreamGeometry();
			streamGeometryNew.FillRule=FillRule.Nonzero;
			System.Drawing.Rectangle rectangleDraw=GetDrawRectangle(iconDesign,new System.Drawing.Rectangle(0,0,sizePixels,sizePixels));
			GetPlacement(iconDesign,rectangleDraw.Width,out float offset,out float scaleDesign);
			using(StreamGeometryContext streamGeometryContext=streamGeometryNew.Open()) {
				iconDesign.GetFigures().WriteTo(new WpfPathSink(streamGeometryContext),(float)((rectangleDraw.X+offset)/scale),(float)((rectangleDraw.Y+offset)/scale),
					(float)(scaleDesign/scale));
			}
			streamGeometryNew.Freeze();
			lock(_lockWpf) {
				StreamGeometry streamGeometry;
				if(_dictGeometriesWpf.TryGetValue(wpfKey,out streamGeometry)) {
					return streamGeometry;
				}
				_dictGeometriesWpf.Add(wpfKey,streamGeometryNew);
			}
			return streamGeometryNew;
		}

		///<summary>The scale, or 1 when it is not a positive number.</summary>
		private static double GetScaleWpf(double scale) {
			if(scale<=0 || double.IsNaN(scale) || double.IsInfinity(scale)) {
				return 1;
			}
			return scale;
		}

		///<summary>Fills the glyph in the brush, in the largest square that fits the slot, centred. scale is the device pixels per DIP of the target.</summary>
		public static void DrawWpf(DrawingContext drawingContext,UiGlyph uiGlyph,Rect rectSlot,Brush brush,double scale=1) {
			if(uiGlyph==UiGlyph.None || rectSlot.IsEmpty || rectSlot.Width<=0 || rectSlot.Height<=0) {
				return;
			}
			double sizeSlot=Math.Min(rectSlot.Width,rectSlot.Height);
			Geometry geometry=GetGeometryWpf(uiGlyph,sizeSlot,scale);
			drawingContext.PushTransform(new TranslateTransform(rectSlot.X+(rectSlot.Width-sizeSlot)/2,rectSlot.Y+(rectSlot.Height-sizeSlot)/2));
			drawingContext.DrawGeometry(brush,null,geometry);
			drawingContext.Pop();
		}

		///<summary>Fills the glyph in the colour of a token (UiTheme.GetBrushWpf).</summary>
		public static void DrawWpf(DrawingContext drawingContext,UiGlyph uiGlyph,Rect rectSlot,EnumUiColor uiColor,double scale=1) {
			DrawWpf(drawingContext,uiGlyph,rectSlot,UiTheme.GetBrushWpf(uiColor),scale);
		}

		///<summary>A frozen image of the glyph in the colour, sizeSlot DIPs square (the slot is kept as the image's size), for an Image control or
		///an ImageBrush. scale is the device pixels per DIP where it is shown.</summary>
		public static DrawingImage GetImageSourceWpf(UiGlyph uiGlyph,double sizeSlot,Color color,double scale=1) {
			DrawingGroup drawingGroup=new DrawingGroup();
			//A transparent square of the slot's size, so the image keeps the slot and the glyph stays centred.
			drawingGroup.Children.Add(new GeometryDrawing(Brushes.Transparent,null,new RectangleGeometry(new Rect(0,0,sizeSlot,sizeSlot))));
			SolidColorBrush solidColorBrush=new SolidColorBrush(color);
			solidColorBrush.Freeze();
			drawingGroup.Children.Add(new GeometryDrawing(solidColorBrush,null,GetGeometryWpf(uiGlyph,sizeSlot,scale)));
			DrawingImage drawingImage=new DrawingImage(drawingGroup);
			drawingImage.Freeze();
			return drawingImage;
		}

		///<summary>The key of a cached geometry: the design (by reference: the table's designs live as long as the program), the slot's pixels and
		///the scale.</summary>
		private struct WpfKey:IEquatable<WpfKey> {
			private readonly IconDesign _iconDesign;
			private readonly int _sizePixels;
			private readonly double _scale;

			public WpfKey(IconDesign iconDesign,int sizePixels,double scale) {
				_iconDesign=iconDesign;
				_sizePixels=sizePixels;
				_scale=scale;
			}

			public bool Equals(WpfKey other) {
				return ReferenceEquals(_iconDesign,other._iconDesign) && _sizePixels==other._sizePixels && _scale==other._scale;
			}

			public override bool Equals(object obj) {
				return obj is WpfKey && Equals((WpfKey)obj);
			}

			public override int GetHashCode() {
				return (_iconDesign.GetHashCode()*397^_sizePixels)*397^_scale.GetHashCode();
			}
		}

		///<summary>Writes a design's figures into a StreamGeometry: each figure filled and closed.</summary>
		private sealed class WpfPathSink:IIconPathSink {
			private readonly StreamGeometryContext _streamGeometryContext;

			public WpfPathSink(StreamGeometryContext streamGeometryContext) {
				_streamGeometryContext=streamGeometryContext;
			}

			public void BeginFigure(float x,float y) {
				_streamGeometryContext.BeginFigure(new Point(x,y),isFilled:true,isClosed:true);
			}

			public void AddLine(float x,float y) {
				_streamGeometryContext.LineTo(new Point(x,y),isStroked:true,isSmoothJoin:false);
			}

			public void AddBezier(float x1,float y1,float x2,float y2,float x,float y) {
				_streamGeometryContext.BezierTo(new Point(x1,y1),new Point(x2,y2),new Point(x,y),isStroked:true,isSmoothJoin:false);
			}

			public void EndFigure() {
				//StreamGeometryContext closes a figure by its isClosed flag when the next one begins.
			}
		}
	}
}
