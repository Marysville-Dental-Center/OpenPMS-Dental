// UI-OVF (docs/port-notes/UI-OVF.md): the WPF windows' tab order once the buttons, check boxes and radio buttons take the keyboard focus. WPF
// tabs in the order of the visual tree when no TabIndex is set, and the Frms' XAML lists their controls in no particular order (the converter's),
// so a window that sets no tab order of its own tabs in reading order instead: top to bottom, left to right on a row. A window that sets
// TabIndexOD (or TabIndex) anywhere keeps its own order. Containers that are their own tab group (GroupBox, Panel, PanelAutoScroll:
// KeyboardNavigation Local) are ordered inside the same way and take their place in the window's order by their top left corner.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace WpfControls.UI{
	///<summary>The reading tab order of a WPF window (UI-OVF). FrmODBase applies it once the window is loaded.</summary>
	public static class UiTabOrder{
		///<summary>Two controls whose tops are this close (DIPs) are on one row: the row is read left to right.</summary>
		public const double HEIGHT_ROW_TOLERANCE=8;

		///<summary>The order to read the rectangles in (their indexes): rows top to bottom, a row being the rectangles whose tops lie within
		///HEIGHT_ROW_TOLERANCE of the row's first (highest) one; left to right within a row; ties keep their given order.</summary>
		public static List<int> GetReadingOrder(IList<Rect> listRects){
			List<int> listIndexesByTop=Enumerable.Range(0,listRects.Count).OrderBy(x => listRects[x].Top).ThenBy(x => listRects[x].Left).ThenBy(x => x).ToList();
			List<int> listOrder=new List<int>();
			int indexRowStart=0;
			while(indexRowStart<listIndexesByTop.Count){
				double topRow=listRects[listIndexesByTop[indexRowStart]].Top;
				int indexRowEnd=indexRowStart;
				while(indexRowEnd<listIndexesByTop.Count && listRects[listIndexesByTop[indexRowEnd]].Top-topRow<=HEIGHT_ROW_TOLERANCE){
					indexRowEnd++;
				}
				List<int> listRow=listIndexesByTop.GetRange(indexRowStart,indexRowEnd-indexRowStart);
				listOrder.AddRange(listRow.OrderBy(x => listRects[x].Left).ThenBy(x => x));
				indexRowStart=indexRowEnd;
			}
			return listOrder;
		}

		///<summary>True when an element under the root has a TabIndex of its own (TabIndexOD sets it): the window keeps its own tab order.</summary>
		public static bool HasTabIndexSet(DependencyObject dependencyObjectRoot){
			if(dependencyObjectRoot==null){
				return false;
			}
			if(dependencyObjectRoot.ReadLocalValue(KeyboardNavigation.TabIndexProperty)!=DependencyProperty.UnsetValue){
				return true;
			}
			int count=VisualTreeHelper.GetChildrenCount(dependencyObjectRoot);
			for(int i=0;i<count;i++){
				if(HasTabIndexSet(VisualTreeHelper.GetChild(dependencyObjectRoot,i))){
					return true;
				}
			}
			return false;
		}

		///<summary>Gives the tab stops under the root (laid out) TabIndexes in reading order, unless one already has a TabIndex (HasTabIndexSet).
		///Returns the elements given an index, in order (a nested container's own stops follow it in the list). Hidden elements are left out and
		///keep the default (they come last, as before).</summary>
		public static List<UIElement> Apply(FrameworkElement frameworkElementRoot){
			List<UIElement> listOrdered=new List<UIElement>();
			if(frameworkElementRoot==null || HasTabIndexSet(frameworkElementRoot)){
				return listOrdered;
			}
			OrderGroup(frameworkElementRoot,listOrdered);
			return listOrdered;
		}

		///<summary>Orders the members of one tab group (the root, or a container that is its own group) and, inside each nested container, its own.</summary>
		private static void OrderGroup(UIElement uIElementGroup,List<UIElement> listOrdered){
			List<UIElement> listMembers=new List<UIElement>();
			CollectMembers(uIElementGroup,listMembers);
			List<Rect> listRects=new List<Rect>();
			for(int i=0;i<listMembers.Count;i++){
				Point point=listMembers[i].TranslatePoint(new Point(0,0),uIElementGroup);
				listRects.Add(new Rect(point,listMembers[i].RenderSize));
			}
			List<int> listOrder=GetReadingOrder(listRects);
			for(int i=0;i<listOrder.Count;i++){
				UIElement uIElement=listMembers[listOrder[i]];
				KeyboardNavigation.SetTabIndex(uIElement,i+1);
				listOrdered.Add(uIElement);
				if(IsNestedContainer(uIElement)){
					OrderGroup(uIElement,listOrdered);
				}
			}
		}

		///<summary>The members of a group: its visible tab stops, and its nested containers (GroupBox, Panel, PanelAutoScroll), without going into
		///them or into any other tab group (a list's items, a tab control's pages keep their own order).</summary>
		private static void CollectMembers(DependencyObject dependencyObjectParent,List<UIElement> listMembers){
			int count=VisualTreeHelper.GetChildrenCount(dependencyObjectParent);
			for(int i=0;i<count;i++){
				if(!(VisualTreeHelper.GetChild(dependencyObjectParent,i) is UIElement uIElement) || uIElement.Visibility!=Visibility.Visible){
					continue;
				}
				if(IsNestedContainer(uIElement)){
					listMembers.Add(uIElement);
					continue;
				}
				if(uIElement.Focusable && KeyboardNavigation.GetIsTabStop(uIElement)){
					listMembers.Add(uIElement);
					continue;
				}
				if(KeyboardNavigation.GetTabNavigation(uIElement)!=KeyboardNavigationMode.Continue){
					continue;//another kind of tab group: its stops keep their order
				}
				CollectMembers(uIElement,listMembers);
			}
		}

		///<summary>The layout containers that are their own tab group (KeyboardNavigation Local in their constructors).</summary>
		private static bool IsNestedContainer(UIElement uIElement){
			return uIElement is GroupBox || uIElement is Panel || uIElement is PanelAutoScroll;
		}
	}
}
