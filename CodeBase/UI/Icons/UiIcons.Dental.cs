// The custom dental icons of the UI refresh (docs/specs/ui-refresh-plan.md, section 4.4; package UI-8, docs/port-notes/UI-8.md): the DentalIcon
// rows of the icon table by their enum, drawn like the glyphs. Each Regular icon has a Filled twin, its member named <name>Filled (the "on"
// state: the selected module, a toggled button). WPF: UiIcons.GetGeometryWpf(UiIcons.GetDesign(dentalIcon,sizePixels),sizeSlot,scale);
// Direct2D: OpenPmsDental.UI.Direct2d.FillIcon and DrawIcon with the same design. System and System.Drawing only, C# 7.3: RdlDesign can compile
// this file in as a linked file with the other linkable icon files.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Drawing;

namespace CodeBase {
	public static partial class UiIcons {
		///<summary>The manifest's enum name of the custom dental icons.</summary>
		public const string DENTAL_ENUM="DentalIcon";
		///<summary>The suffix of a Filled twin's member name: ToothFilled.</summary>
		public const string DENTAL_FILLED_SUFFIX="Filled";
		///<summary>The designs of each DentalIcon by rising size, indexed by the enum's value; filled on first use.</summary>
		private static IconDesign[][] _arrayDesignsByDentalIcon;

		///<summary>The designs of a dental icon by rising size. Empty for None.</summary>
		public static IconDesign[] GetDesigns(DentalIcon dentalIcon) {
			IconDesign[][] arrayDesignsByDentalIcon=_arrayDesignsByDentalIcon;
			if(arrayDesignsByDentalIcon==null) {
				lock(_lockDesigns) {
					if(_arrayDesignsByDentalIcon==null) {
						Array arrayValues=Enum.GetValues(typeof(DentalIcon));
						int countIcons=0;
						foreach(object value in arrayValues) {
							countIcons=Math.Max(countIcons,(int)value+1);
						}
						IconDesign[][] arrayNew=new IconDesign[countIcons][];
						foreach(object value in arrayValues) {
							DentalIcon dentalIconEach=(DentalIcon)value;
							if(dentalIconEach==DentalIcon.None) {
								arrayNew[(int)dentalIconEach]=new IconDesign[0];
								continue;
							}
							arrayNew[(int)dentalIconEach]=GetDesigns(DENTAL_ENUM,dentalIconEach.ToString());
						}
						_arrayDesignsByDentalIcon=arrayNew;
					}
					arrayDesignsByDentalIcon=_arrayDesignsByDentalIcon;
				}
			}
			int index=(int)dentalIcon;
			if(index<0 || index>=arrayDesignsByDentalIcon.Length) {
				return new IconDesign[0];
			}
			return arrayDesignsByDentalIcon[index];
		}

		///<summary>The design of a dental icon for a square slot of sizeSlot device pixels (section 3.5: the largest that fits, never stretched up).
		///Null for None.</summary>
		public static IconDesign GetDesign(DentalIcon dentalIcon,int sizeSlot) {
			return GetDesign(GetDesigns(dentalIcon),sizeSlot);
		}

		///<summary>The Filled twin of a Regular dental icon (ToothFilled for Tooth); the icon itself when it is a Filled one or has no twin.</summary>
		public static DentalIcon GetFilled(DentalIcon dentalIcon) {
			if(dentalIcon==DentalIcon.None || dentalIcon.ToString().EndsWith(DENTAL_FILLED_SUFFIX)) {
				return dentalIcon;
			}
			DentalIcon dentalIconFilled;
			if(Enum.TryParse(dentalIcon.ToString()+DENTAL_FILLED_SUFFIX,out dentalIconFilled)) {
				return dentalIconFilled;
			}
			return dentalIcon;
		}

		///<summary>Fills the dental icon's design for the slot in the colour, as Draw does for a glyph. Nothing for None or an empty slot.</summary>
		public static void Draw(Graphics g,DentalIcon dentalIcon,Rectangle rectangleSlot,Color color) {
			Draw(g,GetDesign(dentalIcon,Math.Min(rectangleSlot.Width,rectangleSlot.Height)),rectangleSlot,color);
		}

		///<summary>Fills the dental icon in the colour of a token.</summary>
		public static void Draw(Graphics g,DentalIcon dentalIcon,Rectangle rectangleSlot,EnumUiColor uiColor) {
			Draw(g,dentalIcon,rectangleSlot,UiTheme.GetColor(uiColor));
		}

		///<summary>Fills the dental icon as disabled: in TextDisabled (section 4.2, item 6).</summary>
		public static void DrawDisabled(Graphics g,DentalIcon dentalIcon,Rectangle rectangleSlot) {
			Draw(g,dentalIcon,rectangleSlot,EnumUiColor.TextDisabled);
		}

		///<summary>A new 32-bit ARGB bitmap of sizePixels square with the dental icon in the colour on transparent (GetBitmap). The caller owns it.</summary>
		public static Bitmap GetBitmap(DentalIcon dentalIcon,int sizePixels,Color color) {
			return GetBitmap(GetDesign(dentalIcon,sizePixels),sizePixels,color);
		}

		///<summary>GetBitmap for a slot of size96 at 96 DPI, scaled (UiTheme.ScaleI).</summary>
		public static Bitmap GetBitmap(DentalIcon dentalIcon,int size96,float scale,Color color) {
			return GetBitmap(dentalIcon,UiTheme.ScaleI(size96,scale),color);
		}

		///<summary>The picture of a default procedure button of a new database (UQ-7 of the plan: new-style pictures for new databases only): the
		///dental icon's 20 px design in the light look's Text colour on white, as the bitmap file procbutton.ButtonImage holds today (20x20, 32 bits
		///per pixel, bottom-up rows, every pixel opaque, 96 DPI), in base64. tools/DevDbTool writes these into the new database that
		///db/init/new-database.sql is made from, and ProcButtons.SetToDefault gives them to the default buttons it makes when an office asks for the
		///reset (owner, 2026-11-17); no other code path changes an office's rows. The same icon always gives the same text.</summary>
		///<summary>The default procedure buttons that have a picture, by description, and the dental icon each shows (UQ-7 of the plan, package UI-8;
		///the owner's answer of 2026-11-17: the reset gives them too). Used by tools/DevDbTool for a new database and by ProcButtons.SetToDefault.
		///RCT BU PFM and RCT BU Ceramic share one picture, as they do today.</summary>
		public static readonly System.Collections.Generic.Dictionary<string,DentalIcon> DictProcButtonPictures=
			new System.Collections.Generic.Dictionary<string,DentalIcon> {
			{ "Amalgam",DentalIcon.ProcAmalgam },
			{ "Composite",DentalIcon.ProcComposite },
			{ "Crown-PFM",DentalIcon.ProcCrownPfm },
			{ "Crown-Ceramic",DentalIcon.ProcCrownCeramic },
			{ "RCT",DentalIcon.ProcRct },
			{ "RCT BU PFM",DentalIcon.ProcRctBuildUp },
			{ "RCT BU Ceramic",DentalIcon.ProcRctBuildUp },
			{ "PA",DentalIcon.XrayPeriapical },
		};

		public static string GetProcButtonPicture(DentalIcon dentalIcon) {
			const int size=PROC_BUTTON_PICTURE_SIZE;
			byte[] arrayBytes=new byte[54+size*size*4];
			//BITMAPFILEHEADER: "BM", the file size, two reserved words, the offset of the pixels.
			arrayBytes[0]=(byte)'B';
			arrayBytes[1]=(byte)'M';
			WriteInt32(arrayBytes,2,arrayBytes.Length);
			WriteInt32(arrayBytes,10,54);
			//BITMAPINFOHEADER: its size, width, height (positive: bottom-up), one plane, 32 bits, BI_RGB, image size 0, 3780 pixels per metre
			//(96 DPI) both ways, no palette.
			WriteInt32(arrayBytes,14,40);
			WriteInt32(arrayBytes,18,size);
			WriteInt32(arrayBytes,22,size);
			arrayBytes[26]=1;
			arrayBytes[28]=32;
			WriteInt32(arrayBytes,38,3780);
			WriteInt32(arrayBytes,42,3780);
			using(Bitmap bitmap=new Bitmap(size,size,System.Drawing.Imaging.PixelFormat.Format32bppArgb)) {
				using(Graphics g=Graphics.FromImage(bitmap)) {
					g.Clear(Color.White);
					Draw(g,dentalIcon,new Rectangle(0,0,size,size),UiTheme.GetColorLight(EnumUiColor.Text));
				}
				int offset=54;
				for(int y=size-1;y>=0;y--) {
					for(int x=0;x<size;x++) {
						Color color=bitmap.GetPixel(x,y);
						arrayBytes[offset]=color.B;
						arrayBytes[offset+1]=color.G;
						arrayBytes[offset+2]=color.R;
						arrayBytes[offset+3]=255;
						offset+=4;
					}
				}
			}
			return Convert.ToBase64String(arrayBytes);
		}

		///<summary>The size of a procedure button's picture, in pixels (procbutton.ButtonImage, the Chart module's 20x20 image list).</summary>
		public const int PROC_BUTTON_PICTURE_SIZE=20;

		private static void WriteInt32(byte[] arrayBytes,int offset,int value) {
			arrayBytes[offset]=(byte)value;
			arrayBytes[offset+1]=(byte)(value>>8);
			arrayBytes[offset+2]=(byte)(value>>16);
			arrayBytes[offset+3]=(byte)(value>>24);
		}
	}
}
