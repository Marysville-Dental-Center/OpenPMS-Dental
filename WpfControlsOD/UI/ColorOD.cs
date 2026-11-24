using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBase;

namespace OpenPmsDental {
	public class ColorOD {
/*
The plan here is to move to our own version of colors similar to SystemColors.
Some colors will be named according to how they are to be used, and we will be able to find where they are all used.
This is not a move toward themes, but rather a move away from MS themes.
One downside is that these colors will not be available in the designer (yet).
Here are some reference colors. Stored in db as int:
Black=-16777216
White=-1
SlateGray=#708090 = (112,128,144)
Gray=128 (#808080)
Silver=192 (#C0C0C0)
LightGray=211
Control=240
Transparent=16777215 (0,255,255,255) which would be white, except for 0 alpha. 
Empty=0 (0,0,0,0) which would be black, except for 0 alpha, so it's also transparent. Not present in WPF, so generally use Transparent.
You can test for either by testing for A=0.
Both Transparent and Empty are heavily used and either one is fine as long as it is well documented.
You can't compare two colors directly. Here's how to test a color:
if(color.ToArgb()==Color.Transparent.ToArgb()){
if(color.ToArgb()==Color.Empty.ToArgb()){
Or, in WPF, test if(color.ToString()==Color.Transparent.ToString()){  //this works because ToString always give the hex value (#1FA313)
*/

		#region Properties - Public
		//UI-11 (docs/port-notes/UI-11.md, docs/specs/ui-refresh-plan.md section 6): the palette points at the UiTheme tokens by role, so what still
		//uses it draws in the new look and in Windows' high-contrast colours where the tokens do. They were public static fields with fixed values.
		///<summary>The window background token, Surface (#FAFAFA; was 252,253,254, the slightly bluish off-white of the old look).</summary>
		public static Color Background => UiTheme.GetColor(EnumUiColor.Surface);
		///<summary>The controls' grey, SurfaceSubtle (#F3F3F3; was gray 240, SystemColors.Control's place).</summary>
		public static Color Control => UiTheme.GetColor(EnumUiColor.SurfaceSubtle);
		///<summary>Gridlines on white, the Gridline token (#E0E0E0; was gray 180).</summary>
		public static Color Gridline => UiTheme.GetColor(EnumUiColor.Gridline);
		///<summary>The hover fill of normally white areas, RowHover (#F0F4FA; was 229,239,251).</summary>
		public static Color Hover => UiTheme.GetColor(EnumUiColor.RowHover);
		///<summary>The outline of some controls, StrokeStrong (#8A8A8A; was gray 90).</summary>
		public static Color Outline => UiTheme.GetColor(EnumUiColor.StrokeStrong);
		///<summary>Disabled text, TextDisabled (#A0A0A0, gray 160 as before).</summary>
		public static Color DisabledText => UiTheme.GetColor(EnumUiColor.TextDisabled);
		#endregion Properties - Public

		#region Methods
		///<summary>Examples, dark to light: Gray=128, Silver=192, LightGray=211, Control=240</summary>
		public static Color Gray(int saturation){
			return Color.FromArgb(saturation,saturation,saturation);
		}

		public static System.Windows.Media.Color GetColorBackground(){
			//The WPF window background token, Surface (UI-11; was 252,253,254).
			return UiTheme.GetColorWpf(EnumUiColor.Surface);
		}

		/// <summary>Example: amounts of 1 and 4 will be more like the second color.  Amounts of 5 and 1 will be more like the first color.</summary>
		public static Color Mix(Color color1,Color color2,float amount1,float amount2){
			Color color=Color.FromArgb(
				(int)((amount1*color1.R+amount2*color2.R)/(amount1+amount2)),
				(int)((amount1*color1.G+amount2*color2.G)/(amount1+amount2)),
				(int)((amount1*color1.B+amount2*color2.B)/(amount1+amount2)));
			return color;
		}
		#endregion Methods

		#region WPF
		public static System.Windows.Media.Color FromString(string str){
			return (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(str);
		}

		public static Color FromWpf(System.Windows.Media.Color w_color){
			Color color=Color.FromArgb(w_color.A, w_color.R, w_color.G, w_color.B);
			return color;
		}

		///<summary>Examples, dark to light: Gray=128, Silver=192, LightGray=211, Control=240</summary>
		public static System.Windows.Media.Color Gray_Wpf(byte saturation){
			return System.Windows.Media.Color.FromRgb(saturation,saturation,saturation);
		}

		/// <summary>Example: amounts of 1 and 4 will be more like the second color.  Amounts of 5 and 1 will be more like the first color.</summary>
		public static System.Windows.Media.Color Mix_Wpf(System.Windows.Media.Color color1,System.Windows.Media.Color color2,float amount1,float amount2){
			System.Windows.Media.Color color=System.Windows.Media.Color.FromRgb(
				(byte)((amount1*color1.R+amount2*color2.R)/(amount1+amount2)),
				(byte)((amount1*color1.G+amount2*color2.G)/(amount1+amount2)),
				(byte)((amount1*color1.B+amount2*color2.B)/(amount1+amount2)));
			return color;
		}

		public static System.Windows.Media.Color ToWpf(Color color){
			System.Windows.Media.Color w_color=System.Windows.Media.Color.FromArgb(color.A,color.R,color.G,color.B);
			return w_color;
		}
		#endregion WPF


	}
}
