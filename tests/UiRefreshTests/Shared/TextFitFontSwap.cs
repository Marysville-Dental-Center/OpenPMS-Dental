// The test-side font swap of the UI refresh (package UI-0b, docs/specs/ui-refresh-plan.md sections 3.4, 5.4 and 7, docs/port-notes/UI-0b.md):
// it walks a built window and replaces Microsoft Sans Serif with a given font by the rule of section 3.4, so the scan and the galleries can show
// real windows in a font option before the product changes (from UI-2a-1 on, the token override UiTheme.SetFontForTest takes its place).
// The rule: Microsoft Sans Serif at 8, 8.25 or 8.5 pt becomes the body size in the new family; any other size keeps its number; the style is
// kept in full (Bold, Italic, Underline); other families stay. The window's font and every container's font are set as well as the
// controls', so an AutoScaleMode.Font container rescales as it would. Under LayoutManager (a FormODBase window, or a UserControl hosted in
// one) the swap changes the size LayoutManagerForms recorded for each control (Control96Info.FontSize96), as the product's mapping will
// (section 5.1), and lays the window out again: the rebuild then gives a Label or RadioButton the 0.96 factor and keeps only Bold, exactly as
// LayoutManagerForms.LayoutChildren does. Content keeps its font: the tooth chart, the perio chart, print previews, the patient dashboard and
// the document canvases (the marked panels of section 5.1), with everything inside them.
// Shared: tests\WindowChecksTests compiles this file in as a linked file (UI-0c), so it uses only Windows Forms, System.Drawing and
// reflection, and finds the program's types and LayoutManagerForms' fields by name.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace UiRefreshTests.TextFit {
	///<summary>A font option of the scan: today's font, or one of the UQ-1 options.</summary>
	public class TextFitFontOption {
		///<summary>Short name for files and reports: today, segoe9, segoe825.</summary>
		public string Name;
		public string Family;
		public float SizeBody;

		public TextFitFontOption(string name,string family,float sizeBody) {
			Name=name;
			Family=family;
			SizeBody=sizeBody;
		}

		///<summary>Microsoft Sans Serif 8.25, today's font.</summary>
		public static readonly TextFitFontOption Today=new TextFitFontOption("today",TextFitFontSwap.FAMILY_TODAY,8.25f);
		///<summary>Segoe UI 9 pt, the owner's answer to UQ-1.</summary>
		public static readonly TextFitFontOption Segoe9=new TextFitFontOption("segoe9","Segoe UI",9f);
		///<summary>Segoe UI 8.25 pt, UQ-1's other option.</summary>
		public static readonly TextFitFontOption Segoe825=new TextFitFontOption("segoe825","Segoe UI",8.25f);

		public bool IsToday {
			get { return Family==TextFitFontSwap.FAMILY_TODAY && SizeBody==8.25f; }
		}

		public Font NewBodyFont() {
			return new Font(Family,SizeBody,FontStyle.Regular,GraphicsUnit.Point);
		}

		public override string ToString() {
			return Family+" "+SizeBody.ToString(System.Globalization.CultureInfo.InvariantCulture);
		}
	}

	public static class TextFitFontSwap {
		public const string FAMILY_TODAY="Microsoft Sans Serif";
		///<summary>Content types (section 5.1): the swap leaves them and everything inside them alone. Matched by type name over the type's
		///base types.</summary>
		public static readonly string[] ArrayContentTypeNames=new string[] { "ToothChartWrapper","ContrPerio","ODPrintPreviewControl","UserControlDashboard" };
		///<summary>Marked content panels (section 5.1, LayoutManagerForms.MarkContent from UI-2a-1 on): window type name and panel name.</summary>
		public static readonly string[][] ArrayContentPanels=new string[][] {
			new string[] { "FormSheetFillEdit","panelMain" },
			new string[] { "FormSheetDefEdit","panelMain" },
			new string[] { "FormClaimFormEdit","panel1" },
		};
		private static MethodInfo _methodShouldSerializeFont;
		private static bool _isShouldSerializeLookedUp;

		///<summary>The rule of section 3.4 for one font: Microsoft Sans Serif at 8, 8.25 or 8.5 pt becomes sizeBody in family; another size keeps its
		///number; the style is kept in full; another family is returned as it is (the same instance). While the option's family is Microsoft Sans
		///Serif the mapping is the identity, size included (3.4: the 8 pt and 8.5 pt designer fonts keep their size until the flip).</summary>
		public static Font MapFont(Font font,string family,float sizeBody) {
			if(!string.Equals(font.Name,FAMILY_TODAY,StringComparison.OrdinalIgnoreCase)) {
				return font;
			}
			if(string.Equals(family,FAMILY_TODAY,StringComparison.OrdinalIgnoreCase)) {
				return font;
			}
			float size=font.SizeInPoints;
			if(IsBodySize(size)) {
				size=sizeBody;
			}
			return new Font(family,size,font.Style,GraphicsUnit.Point,font.GdiCharSet);
		}

		///<summary>8, 8.25 and 8.5 pt are today's body sizes (section 3.4).</summary>
		public static bool IsBodySize(float size) {
			return size==8f || size==8.25f || size==8.5f;
		}

		///<summary>True for a content control: one of the content types, or a marked panel of its window.</summary>
		public static bool IsContent(Control control) {
			for(Type type=control.GetType();type!=null;type=type.BaseType) {
				if(Array.IndexOf(ArrayContentTypeNames,type.Name)>=0) {
					return true;
				}
			}
			Form form=control.FindForm();
			if(form==null) {
				return false;
			}
			foreach(string[] arrayPanel in ArrayContentPanels) {
				if(control.Name==arrayPanel[1] && IsOfTypeName(form,arrayPanel[0])) {
					return true;
				}
			}
			return false;
		}

		private static bool IsOfTypeName(object obj,string typeName) {
			for(Type type=obj.GetType();type!=null;type=type.BaseType) {
				if(type.Name==typeName) {
					return true;
				}
			}
			return false;
		}

		///<summary>True when the control has a font of its own (not inherited from its parent or the default): Control.ShouldSerializeFont.</summary>
		public static bool HasOwnFont(Control control) {
			if(!_isShouldSerializeLookedUp) {
				_isShouldSerializeLookedUp=true;
				_methodShouldSerializeFont=typeof(Control).GetMethod("ShouldSerializeFont",BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public,
					null,Type.EmptyTypes,null);
			}
			if(_methodShouldSerializeFont==null) {
				return true;
			}
			return (bool)_methodShouldSerializeFont.Invoke(control,null);
		}

		///<summary>The window's LayoutManagerForms (FormODBase.LayoutManager), or null for a window without one.</summary>
		public static object GetLayoutManager(Control root) {
			for(Type type=root.GetType();type!=null;type=type.BaseType) {
				FieldInfo fieldInfo=type.GetField("LayoutManager",BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public|BindingFlags.DeclaredOnly);
				if(fieldInfo!=null) {
					return fieldInfo.GetValue(root);
				}
			}
			return null;
		}

		///<summary>The records LayoutManagerForms keeps (Control96Info by control), or an empty dictionary.</summary>
		public static Dictionary<Control,object> GetControl96Infos(object layoutManager) {
			Dictionary<Control,object> dictInfos=new Dictionary<Control,object>();
			if(layoutManager==null) {
				return dictInfos;
			}
			FieldInfo fieldInfo=layoutManager.GetType().GetField("_listControl96Infos",BindingFlags.Instance|BindingFlags.NonPublic);
			IList list=fieldInfo?.GetValue(layoutManager) as IList;
			if(list==null) {
				return dictInfos;
			}
			foreach(object info in list) {
				Control control=info.GetType().GetField("ControlRef").GetValue(info) as Control;
				if(control!=null) {
					dictInfos[control]=info;
				}
			}
			return dictInfos;
		}

		///<summary>Swaps the fonts of root (a built window, or a host window with its hosted control) to the option by the rule of section 3.4, and
		///lays a LayoutManager window out again. Returns the number of fonts changed. Today's option changes nothing.</summary>
		public static int Swap(Control root,TextFitFontOption fontOption) {
			return Swap(root,fontOption.Family,fontOption.SizeBody);
		}

		public static int Swap(Control root,string family,float sizeBody) {
			object layoutManager=GetLayoutManager(root);
			Dictionary<Control,object> dictInfos=GetControl96Infos(layoutManager);
			int countChanged=0;
			if(!IsContent(root)) {
				Font fontMapped=MapFont(root.Font,family,sizeBody);
				if(!ReferenceEquals(fontMapped,root.Font)) {
					root.Font=fontMapped;
					countChanged++;
				}
				countChanged+=SwapChildren(root,family,sizeBody,dictInfos);
			}
			if(layoutManager!=null) {
				LayOutAgain(root,layoutManager);
			}
			return countChanged;
		}

		private static int SwapChildren(Control parent,string family,float sizeBody,Dictionary<Control,object> dictInfos) {
			int countChanged=0;
			for(int i=0;i<parent.Controls.Count;i++) {
				Control control=parent.Controls[i];
				if(IsContent(control)) {
					continue;
				}
				object info;
				if(dictInfos.TryGetValue(control,out info)) {
					//Recorded by LayoutManagerForms: map the recorded 96 dpi size, as the product's mapping will before it records it.
					FieldInfo fieldSize=info.GetType().GetField("FontSize96");
					float size96=(float)fieldSize.GetValue(info);
					using(Font font96=new Font(control.Font.FontFamily,size96,control.Font.Style,GraphicsUnit.Point)) {
						Font fontMapped=MapFont(font96,family,sizeBody);
						if(!ReferenceEquals(fontMapped,font96)) {
							fieldSize.SetValue(info,fontMapped.SizeInPoints);
							control.Font=fontMapped;
							countChanged++;
						}
					}
				}
				else if(control.Controls.Count>0 || control is ContainerControl || HasOwnFont(control)) {
					Font fontMapped=MapFont(control.Font,family,sizeBody);
					if(!ReferenceEquals(fontMapped,control.Font)) {
						control.Font=fontMapped;
						countChanged++;
					}
				}
				countChanged+=SwapChildren(control,family,sizeBody,dictInfos);
			}
			return countChanged;
		}

		///<summary>LayoutManagerForms.LayoutFormBoundsAndFonts for a FormODBase whose panels exist, else LayoutControlBoundsAndFonts of the root.</summary>
		public static void LayOutAgain(Control root,object layoutManager) {
			Type type=layoutManager.GetType();
			FieldInfo fieldPanelClient=null;
			for(Type typeRoot=root.GetType();typeRoot!=null && fieldPanelClient==null;typeRoot=typeRoot.BaseType) {
				fieldPanelClient=typeRoot.GetField("PanelClient",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.DeclaredOnly);
			}
			Control panelClient=fieldPanelClient?.GetValue(root) as Control;
			if(panelClient!=null) {
				MethodInfo methodForm=type.GetMethod("LayoutFormBoundsAndFonts");
				methodForm.Invoke(layoutManager,new object[] { root });
				return;
			}
			MethodInfo methodControl=type.GetMethod("LayoutControlBoundsAndFonts");
			methodControl.Invoke(layoutManager,new object[] { root });
		}
	}
}
