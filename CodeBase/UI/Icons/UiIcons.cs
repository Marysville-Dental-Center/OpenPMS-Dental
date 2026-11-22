// The icon renderer of the UI refresh (docs/specs/ui-refresh-plan.md, sections 3.5, 4.1 and 4.2; package UI-7a, docs/port-notes/UI-7a.md):
// Fluent UI System Icons drawn as vectors from the path data that tools/New-IconData.ps1 compiles into IconData.g.cs. No icon font and no
// picture files. Nothing in the program draws with it yet: the control packages (UI-3 to UI-6) switch their glyphs to it, and UI-7b moves the
// EnumIcons onto the same table.
//
// The class is split into partial files:
// - UiIcons.cs (this file): the designs, the design for a slot (section 3.5), the square it is drawn in, and the figures of a design as calls
//   on an IIconPathSink (the Direct2D wrapper of the program implements one: OpenPmsDental.UI.Direct2d.DrawGlyph).
// - UiIcons.Gdi.cs: GDI+ (GraphicsPath, Draw, GetBitmap).
// - UiIcons.ImageList.cs: GetImageList for ImageLists, TreeViews and ListViews.
// - UiIcons.Wpf.cs: WPF (frozen StreamGeometry, DrawWpf, GetImageSourceWpf).
// IconData.g.cs, this file and UiIcons.Gdi.cs use only System and System.Drawing, and UiIcons.ImageList.cs Windows Forms besides, and nothing
// else of CodeBase but UiTheme's linkable files, in C# 7.3, so RdlDesign (which references no CodeBase) can compile them in as linked files
// (UI-9a-7); a UiRefreshTests test checks it. The WPF part is in its own file.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;

namespace CodeBase {
	///<summary>One design of one icon of icons/manifest.json: a Fluent SVG in one design size, as path data in design pixels (IconData.g.cs).
	///Immutable; its figures are read from the path data once, on first use, and shared by every thread.</summary>
	public sealed class IconDesign {
		///<summary>The enum of the manifest row: UiGlyph or EnumIcons.</summary>
		public readonly string EnumName;
		///<summary>The member of the enum.</summary>
		public readonly string Name;
		///<summary>The default colour role: Text, Accent, Danger, Success or Warning (EnumUiColor members).</summary>
		public readonly string Role;
		///<summary>The design size in pixels: the SVG's viewBox is 0 0 Size Size.</summary>
		public readonly int Size;
		///<summary>The SVG's file name under third-party/fluentui-system-icons/icons (or custom/... under icons).</summary>
		public readonly string FileName;
		///<summary>The SHA-256 of the SVG file the path data was made from.</summary>
		public readonly string Sha256;
		///<summary>The path data: M x y, L x y, C x1 y1 x2 y2 x y and Z, separated by spaces, in design pixels.</summary>
		public readonly string PathData;
		///<summary>The manifest row's opticalScale (the UI-9b-4 fix, docs/port-notes/UI-9b.md, section 5.8): the design is drawn this much larger about its
		///centre, so a glyph that Fluent draws smaller than its neighbours (the heading letters: a 14 px cap height in the 24 px design, where the
		///B and the I have 16) gets the same optical size in a strip. Only where the design is scaled anyway (a box of another size than the design,
		///UiIcons.GetPlacement); a design in a box of its own size is drawn pixel for pixel. 1 for every other row.</summary>
		public readonly float OpticalScale;
		private readonly object _lock=new object();
		private volatile IconFigures _iconFigures;

		public IconDesign(string enumName,string name,string role,int size,string fileName,string sha256,string pathData)
			:this(enumName,name,role,size,fileName,sha256,pathData,1f)
		{
		}

		public IconDesign(string enumName,string name,string role,int size,string fileName,string sha256,string pathData,float opticalScale) {
			EnumName=enumName;
			Name=name;
			Role=role;
			Size=size;
			FileName=fileName;
			Sha256=sha256;
			PathData=pathData;
			OpticalScale=opticalScale;
		}

		///<summary>The commands and points of the path data, read once.</summary>
		public IconFigures GetFigures() {
			IconFigures iconFigures=_iconFigures;
			if(iconFigures!=null) {
				return iconFigures;
			}
			lock(_lock) {
				if(_iconFigures==null) {
					_iconFigures=IconFigures.Parse(PathData);
				}
				return _iconFigures;
			}
		}

		public override string ToString() {
			return EnumName+"."+Name+" "+Size+" px ("+FileName+")";
		}
	}

	///<summary>The kinds of drawing command of a design's path data.</summary>
	public enum EnumIconCommand {
		///<summary>Starts a figure at one point.</summary>
		Move,
		///<summary>A straight line to one point.</summary>
		Line,
		///<summary>A cubic curve: two control points and the end point.</summary>
		Cubic,
		///<summary>Closes the figure.</summary>
		Close
	}

	///<summary>A design's figures: the commands and, for each, its points (Move and Line one, Cubic three, Close none) in design pixels.
	///Immutable.</summary>
	public sealed class IconFigures {
		private readonly EnumIconCommand[] _arrayCommands;
		private readonly float[] _arrayPoints;

		private IconFigures(EnumIconCommand[] arrayCommands,float[] arrayPoints) {
			_arrayCommands=arrayCommands;
			_arrayPoints=arrayPoints;
		}

		///<summary>The number of commands.</summary>
		public int CountCommands {
			get { return _arrayCommands.Length; }
		}

		///<summary>The number of figures (Move commands).</summary>
		public int CountFigures {
			get {
				int count=0;
				for(int i=0;i<_arrayCommands.Length;i++) {
					if(_arrayCommands[i]==EnumIconCommand.Move) {
						count++;
					}
				}
				return count;
			}
		}

		///<summary>The smallest rectangle around every point, control points included, in design pixels.</summary>
		public RectangleF GetBounds() {
			if(_arrayPoints.Length==0) {
				return RectangleF.Empty;
			}
			float xMin=float.MaxValue;
			float yMin=float.MaxValue;
			float xMax=float.MinValue;
			float yMax=float.MinValue;
			for(int i=0;i<_arrayPoints.Length;i+=2) {
				xMin=Math.Min(xMin,_arrayPoints[i]);
				xMax=Math.Max(xMax,_arrayPoints[i]);
				yMin=Math.Min(yMin,_arrayPoints[i+1]);
				yMax=Math.Max(yMax,_arrayPoints[i+1]);
			}
			return RectangleF.FromLTRB(xMin,yMin,xMax,yMax);
		}

		///<summary>Sends every figure to the sink, each point mapped as (x*scale+offsetX, y*scale+offsetY). Every figure ends with EndFigure (the
		///figures are filled, so each is closed).</summary>
		public void WriteTo(IIconPathSink iconPathSink,float offsetX,float offsetY,float scale) {
			int indexPoint=0;
			bool isFigureOpen=false;
			for(int i=0;i<_arrayCommands.Length;i++) {
				switch(_arrayCommands[i]) {
					case EnumIconCommand.Move:
						if(isFigureOpen) {
							iconPathSink.EndFigure();
						}
						iconPathSink.BeginFigure(_arrayPoints[indexPoint]*scale+offsetX,_arrayPoints[indexPoint+1]*scale+offsetY);
						indexPoint+=2;
						isFigureOpen=true;
						break;
					case EnumIconCommand.Line:
						iconPathSink.AddLine(_arrayPoints[indexPoint]*scale+offsetX,_arrayPoints[indexPoint+1]*scale+offsetY);
						indexPoint+=2;
						break;
					case EnumIconCommand.Cubic:
						iconPathSink.AddBezier(_arrayPoints[indexPoint]*scale+offsetX,_arrayPoints[indexPoint+1]*scale+offsetY,
							_arrayPoints[indexPoint+2]*scale+offsetX,_arrayPoints[indexPoint+3]*scale+offsetY,
							_arrayPoints[indexPoint+4]*scale+offsetX,_arrayPoints[indexPoint+5]*scale+offsetY);
						indexPoint+=6;
						break;
					case EnumIconCommand.Close:
						if(isFigureOpen) {
							iconPathSink.EndFigure();
						}
						isFigureOpen=false;
						break;
				}
			}
			if(isFigureOpen) {
				iconPathSink.EndFigure();
			}
		}

		///<summary>Reads path data of IconData.g.cs: tokens separated by single spaces, each command letter followed by its numbers. Throws on
		///anything else (the generator writes only this form).</summary>
		public static IconFigures Parse(string pathData) {
			List<EnumIconCommand> listCommands=new List<EnumIconCommand>();
			List<float> listPoints=new List<float>();
			string[] arrayTokens=pathData.Split(new char[] { ' ' },StringSplitOptions.RemoveEmptyEntries);
			int i=0;
			while(i<arrayTokens.Length) {
				string token=arrayTokens[i];
				i++;
				int countNumbers;
				if(token=="M") {
					listCommands.Add(EnumIconCommand.Move);
					countNumbers=2;
				}
				else if(token=="L") {
					listCommands.Add(EnumIconCommand.Line);
					countNumbers=2;
				}
				else if(token=="C") {
					listCommands.Add(EnumIconCommand.Cubic);
					countNumbers=6;
				}
				else if(token=="Z") {
					listCommands.Add(EnumIconCommand.Close);
					countNumbers=0;
				}
				else {
					throw new FormatException("Icon path data: a command was expected, not '"+token+"'.");
				}
				if(listCommands.Count==1 && listCommands[0]!=EnumIconCommand.Move) {
					throw new FormatException("Icon path data must start with M.");
				}
				for(int n=0;n<countNumbers;n++) {
					if(i>=arrayTokens.Length) {
						throw new FormatException("Icon path data ends inside a command.");
					}
					listPoints.Add(ParseNumber(arrayTokens[i]));
					i++;
				}
			}
			return new IconFigures(listCommands.ToArray(),listPoints.ToArray());
		}

		///<summary>A number as the generator writes it: an optional minus, digits, and an optional point with up to 4 digits. Culture-free.</summary>
		private static float ParseNumber(string text) {
			int index=0;
			bool isNegative=false;
			if(text.Length>0 && text[0]=='-') {
				isNegative=true;
				index=1;
			}
			if(index>=text.Length) {
				throw new FormatException("Icon path data: '"+text+"' is not a number.");
			}
			double value=0;
			int countDigits=0;
			while(index<text.Length && text[index]>='0' && text[index]<='9') {
				value=value*10+(text[index]-'0');
				index++;
				countDigits++;
			}
			if(index<text.Length && text[index]=='.') {
				index++;
				double factor=0.1;
				while(index<text.Length && text[index]>='0' && text[index]<='9') {
					value+=(text[index]-'0')*factor;
					factor/=10;
					index++;
					countDigits++;
				}
			}
			if(index!=text.Length || countDigits==0) {
				throw new FormatException("Icon path data: '"+text+"' is not a number.");
			}
			return (float)(isNegative ? -value : value);
		}
	}

	///<summary>Receives a design's figures, already in device pixels (IconFigures.WriteTo). The GDI+ path, the WPF StreamGeometry and the program's
	///Direct2D wrapper (OpenPmsDental.UI.Direct2d) each implement one. Fill rule: nonzero (Direct2D's winding mode).</summary>
	public interface IIconPathSink {
		void BeginFigure(float x,float y);
		void AddLine(float x,float y);
		void AddBezier(float x1,float y1,float x2,float y2,float x,float y);
		///<summary>Closes the figure.</summary>
		void EndFigure();
	}

	///<summary>The Fluent icons of the UI refresh (section 4.2): the designs of IconData.g.cs, the design for a slot, and the drawing on GDI+
	///(UiIcons.Gdi.cs), Direct2D (through IIconPathSink) and WPF (UiIcons.Wpf.cs).</summary>
	public static partial class UiIcons {
		///<summary>The designs of each UiGlyph by rising size, indexed by the enum's value; filled on first use.</summary>
		private static IconDesign[][] _arrayDesignsByGlyph;
		private static readonly object _lockDesigns=new object();

		///<summary>Every design of the table, in manifest order and by rising size (a copy).</summary>
		public static IconDesign[] GetAllDesigns() {
			return IconData.GetDesigns();
		}

		///<summary>The designs of a manifest row by rising size, by its enum and member name (for UI-7b's EnumIcons). Empty when the table has
		///none.</summary>
		public static IconDesign[] GetDesigns(string enumName,string name) {
			List<IconDesign> listDesigns=new List<IconDesign>();
			foreach(IconDesign iconDesign in IconData.GetDesigns()) {
				if(iconDesign.EnumName==enumName && iconDesign.Name==name) {
					listDesigns.Add(iconDesign);
				}
			}
			return listDesigns.ToArray();
		}

		///<summary>The designs of a glyph by rising size. Empty for None.</summary>
		public static IconDesign[] GetDesigns(UiGlyph uiGlyph) {
			IconDesign[][] arrayDesignsByGlyph=_arrayDesignsByGlyph;
			if(arrayDesignsByGlyph==null) {
				lock(_lockDesigns) {
					if(_arrayDesignsByGlyph==null) {
						Array arrayValues=Enum.GetValues(typeof(UiGlyph));
						int countGlyphs=0;
						foreach(object value in arrayValues) {
							countGlyphs=Math.Max(countGlyphs,(int)value+1);
						}
						IconDesign[][] arrayNew=new IconDesign[countGlyphs][];
						foreach(object value in arrayValues) {
							UiGlyph uiGlyphEach=(UiGlyph)value;
							if(uiGlyphEach==UiGlyph.None) {
								arrayNew[(int)uiGlyphEach]=new IconDesign[0];
								continue;
							}
							arrayNew[(int)uiGlyphEach]=GetDesigns("UiGlyph",uiGlyphEach.ToString());
						}
						_arrayDesignsByGlyph=arrayNew;
					}
					arrayDesignsByGlyph=_arrayDesignsByGlyph;
				}
			}
			int index=(int)uiGlyph;
			if(index<0 || index>=arrayDesignsByGlyph.Length) {
				return new IconDesign[0];
			}
			return arrayDesignsByGlyph[index];
		}

		///<summary>The design drawn in a square slot of sizeSlot device pixels (section 3.5). A control glyph (UiGlyph): the largest design of the icon
		///that is no larger than UiTheme.GetIconDesignSize(sizeSlot), never stretched up; when every design is larger than that (a slot below the
		///smallest design), the smallest, which is then drawn shrunk to the slot. An icon of the toolbars, buttons and lists (EnumIcons, DentalIcon;
		///IsBoxFilled): the design of that size when the icon has one, else the nearest in size (by ratio; a tie takes the larger), which is drawn
		///scaled to fill the same box (GetPlacement), so every icon of a strip has one size. Null when the icon has no design (None).</summary>
		public static IconDesign GetDesign(IconDesign[] arrayDesigns,int sizeSlot) {
			if(arrayDesigns==null || arrayDesigns.Length==0) {
				return null;
			}
			int sizeWanted=UiTheme.GetIconDesignSize(Math.Max(sizeSlot,1));
			IconDesign iconDesign=arrayDesigns[0];
			if(IsBoxFilled(iconDesign)) {
				double ratioBest=double.MaxValue;
				for(int i=0;i<arrayDesigns.Length;i++) {
					double ratio=Math.Abs(Math.Log((double)arrayDesigns[i].Size/sizeWanted));
					if(ratio<=ratioBest+1e-9) {//rising sizes: a tie goes to the larger
						ratioBest=ratio;
						iconDesign=arrayDesigns[i];
					}
				}
				return iconDesign;
			}
			for(int i=0;i<arrayDesigns.Length;i++) {
				if(arrayDesigns[i].Size<=sizeWanted) {
					iconDesign=arrayDesigns[i];
				}
			}
			return iconDesign;
		}

		///<summary>Whether a design fills its box (the UI-9b-4 fix, docs/port-notes/UI-9b.md, section 5.8): the icons of the toolbars, buttons, lists
		///and the module bar (EnumIcons and the custom DentalIcon rows) are drawn in the box of the slot (UiTheme.GetIconDesignSize) whatever design
		///sizes the icon has, scaled by the Fluent keylines (GetLiveSize), so a strip's icons have one optical size and stroke weight at every scale.
		///The control glyphs (UiGlyph) keep section 3.5's rule: drawn at their design's size, never stretched up.</summary>
		public static bool IsBoxFilled(IconDesign iconDesign) {
			return iconDesign!=null && iconDesign.EnumName!="UiGlyph";
		}

		///<summary>The side of the Fluent keyline square of a design size (its live area): 10 of 12, 14 of 16, 16 of 20, 20 of 24, 24 of 28, 28 of
		///32 and 40 of 48 px (Fluent pads 1 px at 12 and 16, 2 px at 20 to 32 and 4 px at 48; most icons reach these edges). Other sizes: five sixths.</summary>
		public static float GetLiveSize(int size) {
			switch(size) {
				case 12:
					return 10;
				case 16:
					return 14;
				case 20:
					return 16;
				case 24:
					return 20;
				case 28:
					return 24;
				case 32:
					return 28;
				case 48:
					return 40;
				default:
					return size*5f/6f;
			}
		}

		///<summary>Where a design goes in its box of sizeBox device pixels (the square GetDrawRectangle gives): the scale from design pixels and the
		///offset of the design's corner from the box's corner (the same across and down). A design of the box's size, and every control glyph, fill
		///the box as before (scale sizeBox/Size, offset 0: the design square is the box), so the design is drawn pixel for pixel and its 1 px stems
		///stay on whole pixels; its OpticalScale is not applied there (an optical scale of the design's own size would put every stem and serif on a
		///fraction of a pixel and draw it grey: the UI-9b-4 fix's second review, docs/port-notes/UI-9b.md, section 5.8). A box-filled design of
		///another size, which is scaled anyway, is scaled so its keyline square fills the box's (GetLiveSize(sizeBox)/GetLiveSize(Size); a box below
		///the smallest Fluent size: sizeBox/Size), times its OpticalScale, centred.</summary>
		public static void GetPlacement(IconDesign iconDesign,int sizeBox,out float offset,out float scale) {
			if(!IsBoxFilled(iconDesign) || sizeBox==iconDesign.Size) {
				scale=(float)sizeBox/iconDesign.Size;
				offset=0;
				return;
			}
			if(Array.IndexOf(UiTheme.ArrayIconDesignSizes,sizeBox)>=0) {
				scale=GetLiveSize(sizeBox)/GetLiveSize(iconDesign.Size);
			}
			else {
				scale=(float)sizeBox/iconDesign.Size;
			}
			scale*=iconDesign.OpticalScale;
			offset=(sizeBox-iconDesign.Size*scale)/2f;
		}

		///<summary>The design of a glyph for a square slot of sizeSlot device pixels (GetDesign). Null for None.</summary>
		public static IconDesign GetDesign(UiGlyph uiGlyph,int sizeSlot) {
			return GetDesign(GetDesigns(uiGlyph),sizeSlot);
		}

		///<summary>The square the design is drawn in (its box), in device pixels, centred in the slot on whole pixels (the left and top get the smaller
		///half of an odd remainder, as UiTheme.GetIconRectangle). A control glyph: the design's size (or the slot's, when the design is larger). A
		///box-filled icon (IsBoxFilled): UiTheme.GetIconDesignSize of the slot (or the slot's, when smaller), the same square for every icon of the
		///slot, whichever design it draws (GetPlacement).</summary>
		public static Rectangle GetDrawRectangle(IconDesign iconDesign,Rectangle rectangleSlot) {
			int sizeSlot=Math.Min(rectangleSlot.Width,rectangleSlot.Height);
			int size=Math.Min(iconDesign.Size,sizeSlot);
			if(IsBoxFilled(iconDesign)) {
				size=Math.Min(UiTheme.GetIconDesignSize(Math.Max(sizeSlot,1)),sizeSlot);
			}
			return new Rectangle(rectangleSlot.X+(rectangleSlot.Width-size)/2,rectangleSlot.Y+(rectangleSlot.Height-size)/2,size,size);
		}

		///<summary>Sends the glyph's design for the slot to the sink, in device pixels. Nothing for None or an empty slot.</summary>
		public static void WriteFigures(IIconPathSink iconPathSink,UiGlyph uiGlyph,Rectangle rectangleSlot) {
			IconDesign iconDesign=GetDesign(uiGlyph,Math.Min(rectangleSlot.Width,rectangleSlot.Height));
			WriteFigures(iconPathSink,iconDesign,rectangleSlot);
		}

		///<summary>Sends a design to the sink, placed in the slot by GetDrawRectangle and GetPlacement, in device pixels. Nothing for null or an empty
		///slot.</summary>
		public static void WriteFigures(IIconPathSink iconPathSink,IconDesign iconDesign,Rectangle rectangleSlot) {
			if(iconDesign==null || rectangleSlot.Width<=0 || rectangleSlot.Height<=0) {
				return;
			}
			Rectangle rectangleDraw=GetDrawRectangle(iconDesign,rectangleSlot);
			GetPlacement(iconDesign,rectangleDraw.Width,out float offset,out float scale);
			iconDesign.GetFigures().WriteTo(iconPathSink,rectangleDraw.X+offset,rectangleDraw.Y+offset,scale);
		}

		///<summary>The default colour of a design: its manifest role as a colour token (Text when the role is not a token).</summary>
		public static Color GetRoleColor(IconDesign iconDesign) {
			return UiTheme.GetColor(GetRoleToken(iconDesign));
		}

		///<summary>The colour token of a design's manifest role (Text when the role is not a token).</summary>
		public static EnumUiColor GetRoleToken(IconDesign iconDesign) {
			if(iconDesign!=null) {
				switch(iconDesign.Role) {
					case "Accent":
						return EnumUiColor.Accent;
					case "Danger":
						return EnumUiColor.Danger;
					case "Success":
						return EnumUiColor.Success;
					case "Warning":
						return EnumUiColor.Warning;
				}
			}
			return EnumUiColor.Text;
		}
	}

	public static partial class IconData {
		///<summary>Every design of the table, in manifest order and by rising size (a copy of the generated array).</summary>
		public static IconDesign[] GetDesigns() {
			return (IconDesign[])_arrayDesigns.Clone();
		}
	}
}
