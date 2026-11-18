using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CodeBase;

namespace WpfControls.UI{
	/*
The icons of the WPF toolbars, buttons, tree views and the image selector. Since package UI-7b of the UI refresh (docs/specs/ui-refresh-plan.md,
sections 3.5, 4.2 and 4.3; docs/port-notes/UI-7b.md) every member draws a Fluent UI System Icon from the one icon table in CodeBase
(CodeBase/UI/Icons, made by tools/New-IconData.ps1 from icons/manifest.json, where each member has its row keyed by its name), the same rows
the WinForms IconLibrary (OpenPmsDental/UI/IconLibrary) draws. Chart32G, Chart32W and ChartMed32 have no row until UI-8 makes the dental icons;
WPF never drew them (they had no WPF picture).
To add an icon: add the member here (and to the WinForms enum when WinForms draws it), add its manifest row (icons/README.md), run the generator.
	*/
	public enum EnumIcons{
		None,
		Account32,
		Acquire,
		Add,
		Appt32,
		ArrowLeft,
		ArrowRight,
		ArrowsAll,
		BreakAptX,
		Chart32,
		Chart32G,
		Chart32W,
		ChartMed32,
		CommLog,
		Complete,
		Copy,
		Crop,
		DeleteX,
		Email,
		Export,
		Family32,
		Flip,
		Hand,
		ImageSelectorDoc,
		ImageSelectorFile,
		ImageSelectorFolder,
		ImageSelectorFolderWeb,
		ImageSelectorMount,
		ImageSelectorPhoto,
		ImageSelectorXray,
		Imaging32,
		Import,
		Info,
		Manage32,
		Paste,
		PatAdd,
		PatDelete,
		Patient,
		PatMoveFam,
		PatSelect,
		PatSetGuarantor,
		Print,
		Probe,
		Recall,
		RotateL,
		RotateR,
		ScanMulti,
		ScanPhoto,
		ScanXray,
		Text,
		TreatPlan32,
		TreatPlanMed32,
		Video,
		WebMail
	}

	///<summary></summary>
	public partial class IconLibrary{
		///<summary>The manifest's enum name of these icons' rows.</summary>
		public const string MANIFEST_ENUM="EnumIcons";
		///<summary>The suffix of the row of an icon's Filled style ("on" states, section 4.3): Appt32Filled.</summary>
		public const string FILLED_SUFFIX="Filled";
		///<summary>The designs of each member by rising size, Regular and Filled, indexed by the enum's value; filled on first use.</summary>
		private static IconDesign[][] _arrayDesignsRegular;
		private static IconDesign[][] _arrayDesignsFilled;
		private static readonly object _lockDesigns=new object();

		///<summary>Adds the icon to the grid as a Path filled in the icon's colour (its manifest role, UQ-4), the size of grid.Width (22 DIPs in buttons
		///and toolbars, 16 in a tree view for example; square icons only). The design is the one for the slot's real pixel size at the grid's DPI
		///(section 3.5), centred on whole device pixels. Any margin is set by the caller on the grid. Nothing for None or an icon without a row
		///(Chart32G, Chart32W, ChartMed32).</summary>
		public static void DrawWpf(EnumIcons enumIcons,System.Windows.Controls.Grid grid){
			DrawWpf(enumIcons,grid,null,isFilled:false);
		}

		///<summary>DrawWpf in a brush of the caller's (null: the icon's own colour), Regular or Filled (the Filled style falls back to Regular).</summary>
		public static void DrawWpf(EnumIcons enumIcons,System.Windows.Controls.Grid grid,Brush brush,bool isFilled){
			IconDesign[] arrayDesigns=GetDesigns(enumIcons,isFilled);
			if(arrayDesigns.Length==0){
				return;
			}
			double sizeSlot=grid.Width;
			if(double.IsNaN(sizeSlot) || sizeSlot<=0){
				sizeSlot=Width(enumIcons);
			}
			double scale=VisualTreeHelper.GetDpi(grid).PixelsPerDip;
			IconDesign iconDesign=UiIcons.GetDesign(arrayDesigns,(int)Math.Round(sizeSlot*scale));
			System.Windows.Shapes.Path path=new System.Windows.Shapes.Path();
			path.Data=UiIcons.GetGeometryWpf(iconDesign,sizeSlot,scale);
			path.Fill=brush ?? UiTheme.GetBrushWpf(UiIcons.GetRoleToken(iconDesign));
			path.Width=sizeSlot;
			path.Height=sizeSlot;
			path.Stretch=Stretch.None;
			path.HorizontalAlignment=HorizontalAlignment.Left;
			path.VerticalAlignment=VerticalAlignment.Top;
			path.SnapsToDevicePixels=true;
			grid.Children.Add(path);
		}

		///<summary>The icon's designs in the icon table by rising size, Regular or Filled. The Filled style falls back to Regular when the icon has no
		///Filled row. Empty for None and the icons without a row.</summary>
		public static IconDesign[] GetDesigns(EnumIcons icon,bool isFilled=false){
			if(_arrayDesignsRegular==null){
				lock(_lockDesigns){
					if(_arrayDesignsRegular==null){
						Array arrayValues=Enum.GetValues(typeof(EnumIcons));
						int count=0;
						foreach(object value in arrayValues){
							count=Math.Max(count,(int)value+1);
						}
						IconDesign[][] arrayRegular=new IconDesign[count][];
						IconDesign[][] arrayFilled=new IconDesign[count][];
						foreach(object value in arrayValues){
							EnumIcons iconEach=(EnumIcons)value;
							if(iconEach==EnumIcons.None){
								arrayRegular[(int)value]=new IconDesign[0];
								arrayFilled[(int)value]=new IconDesign[0];
								continue;
							}
							arrayRegular[(int)value]=UiIcons.GetDesigns(MANIFEST_ENUM,iconEach.ToString());
							IconDesign[] arrayDesignsFilled=UiIcons.GetDesigns(MANIFEST_ENUM,iconEach.ToString()+FILLED_SUFFIX);
							if(arrayDesignsFilled.Length==0){
								arrayDesignsFilled=arrayRegular[(int)value];
							}
							arrayFilled[(int)value]=arrayDesignsFilled;
						}
						_arrayDesignsFilled=arrayFilled;
						_arrayDesignsRegular=arrayRegular;
					}
				}
			}
			IconDesign[][] arrayDesigns=isFilled ? _arrayDesignsFilled : _arrayDesignsRegular;
			int index=(int)icon;
			if(index<0 || index>=arrayDesigns.Length){
				return new IconDesign[0];
			}
			return arrayDesigns[index];
		}

		///<summary>32 or 22</summary>
		public static int Width(EnumIcons icon){
			switch(icon){
				case EnumIcons.Account32:
				case EnumIcons.Appt32:
				case EnumIcons.Family32:
				case EnumIcons.Imaging32:
				case EnumIcons.Manage32:
				case EnumIcons.TreatPlan32:
					return 32;
				default:
					return 22;
			}
		}

		///<summary>Whether the icon is drawn as vectors: every icon with a row in the icon table since UI-7b.</summary>
		public static bool IsVector(EnumIcons icon){
			return GetDesigns(icon).Length>0;
		}

		///<summary>Whether WPF draws the icon: every icon with a row in the icon table since UI-7b (before, only the icons the IconGenerator had moved
		///to WPF).</summary>
		public static bool IsWPF(EnumIcons icon){
			return GetDesigns(icon).Length>0;
		}

	}


}


//https://github.com/jingwood/d2dlib/issues/2
