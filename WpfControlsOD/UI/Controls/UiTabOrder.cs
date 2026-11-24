// UI-OVF (docs/port-notes/UI-OVF.md): the WPF windows' tab order once the buttons, check boxes and radio buttons take the keyboard focus. WPF
// tabs in the order of the visual tree when no TabIndex is set, and the Frms' XAML lists their controls in no particular order (the converter's),
// so a window that sets no tab order of its own tabs in reading order instead: top to bottom, left to right on a row. A window that sets
// TabIndexOD (or TabIndex) anywhere keeps its own order, and so does office content (the eForm fill surface, ContentLook.IsPinned), whose
// fields are laid out in their own order. Containers that are their own tab group (GroupBox, Panel, PanelAutoScroll: KeyboardNavigation
// Local) are ordered inside the same way and take their place in the window's order by their top left corner. A control hidden when the order
// is made (it has no place to read) takes the index of the visible one before it in the tree, so it tabs next to that one once it shows.
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

		///<summary>One member of a tab group: a tab stop or a nested container, and whether it is hidden (itself or under a hidden element).</summary>
		private class Member{
			public UIElement UIElement;
			public bool IsHidden;
		}

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

		///<summary>True when an element under the root has a TabIndex of its own (TabIndexOD sets it), or is office content (ContentLook.IsPinned):
		///the window keeps its own tab order.</summary>
		public static bool HasTabIndexSet(DependencyObject dependencyObjectRoot){
			if(dependencyObjectRoot==null){
				return false;
			}
			if(dependencyObjectRoot.ReadLocalValue(KeyboardNavigation.TabIndexProperty)!=DependencyProperty.UnsetValue){
				return true;
			}
			if(ContentLook.GetIsPinned(dependencyObjectRoot)){
				return true;
			}
			foreach(DependencyObject dependencyObjectChild in GetChildren(dependencyObjectRoot)){
				if(HasTabIndexSet(dependencyObjectChild)){
					return true;
				}
			}
			return false;
		}

		///<summary>Gives the tab stops under the root (laid out) TabIndexes in reading order, unless the window has an order of its own
		///(HasTabIndexSet). Returns the visible elements given an index, in order (a nested container's own stops follow it in the list).</summary>
		public static List<UIElement> Apply(FrameworkElement frameworkElementRoot){
			List<UIElement> listOrdered=new List<UIElement>();
			if(frameworkElementRoot==null || HasTabIndexSet(frameworkElementRoot)){
				return listOrdered;
			}
			OrderGroup(frameworkElementRoot,listOrdered);
			//UI-11-final: with the indexes set, each radio button group's stop is the checked one or the first in the new order.
			foreach(DependencyObject dependencyObjectParent in listOrdered.OfType<RadioButton>().Select(x => x.Parent).Distinct().ToList()){
				RadioButton.UpdateGroupTabStops(dependencyObjectParent);
			}
			return listOrdered;
		}

		///<summary>Orders the members of one tab group (the root, or a container that is its own group) and, inside each visible nested container,
		///its own. A hidden member takes the index of the visible member before it in the tree (0 when none is).</summary>
		private static void OrderGroup(UIElement uIElementGroup,List<UIElement> listOrdered){
			List<Member> listMembers=new List<Member>();
			CollectMembers(uIElementGroup,listMembers,isHidden:false);
			List<Member> listVisible=listMembers.Where(x => !x.IsHidden).ToList();
			List<Rect> listRects=new List<Rect>();
			for(int i=0;i<listVisible.Count;i++){
				Point point=listVisible[i].UIElement.TranslatePoint(new Point(0,0),uIElementGroup);
				listRects.Add(new Rect(point,listVisible[i].UIElement.RenderSize));
			}
			List<int> listOrder=GetReadingOrder(listRects);
			for(int i=0;i<listOrder.Count;i++){
				UIElement uIElement=listVisible[listOrder[i]].UIElement;
				KeyboardNavigation.SetTabIndex(uIElement,i+1);
				listOrdered.Add(uIElement);
				if(IsNestedContainer(uIElement)){
					OrderGroup(uIElement,listOrdered);
				}
			}
			int tabIndexBefore=0;
			for(int i=0;i<listMembers.Count;i++){
				if(listMembers[i].IsHidden){
					KeyboardNavigation.SetTabIndex(listMembers[i].UIElement,tabIndexBefore);
					continue;
				}
				tabIndexBefore=KeyboardNavigation.GetTabIndex(listMembers[i].UIElement);
			}
		}

		///<summary>The members of a group in tree order: its tab stops and its nested containers (GroupBox, Panel, PanelAutoScroll), without going
		///into them or into any other tab group (a list's items, a tab control's pages keep their own order).</summary>
		private static void CollectMembers(DependencyObject dependencyObjectParent,List<Member> listMembers,bool isHidden){
			foreach(DependencyObject dependencyObjectChild in GetChildren(dependencyObjectParent)){
				if(!(dependencyObjectChild is UIElement uIElement)){
					continue;
				}
				bool isHiddenChild=isHidden || uIElement.Visibility!=Visibility.Visible;
				//UI-11-final: a radio button is ordered whether or not it is its group's one tab stop now (RadioButton.UpdateGroupTabStops), as
				//the stop moves to the one checked.
				if(IsNestedContainer(uIElement) || (uIElement.Focusable && (KeyboardNavigation.GetIsTabStop(uIElement) || uIElement is RadioButton))){
					listMembers.Add(new Member { UIElement=uIElement,IsHidden=isHiddenChild });
					continue;
				}
				if(KeyboardNavigation.GetTabNavigation(uIElement)!=KeyboardNavigationMode.Continue){
					continue;//another kind of tab group: its stops keep their order
				}
				CollectMembers(uIElement,listMembers,isHiddenChild);
			}
		}

		///<summary>The element's children in the visual tree; for an element not laid out yet (a hidden one has no template applied), its logical
		///children, so a hidden text box's inner stop is still found.</summary>
		private static List<DependencyObject> GetChildren(DependencyObject dependencyObject){
			List<DependencyObject> listChildren=new List<DependencyObject>();
			int count=0;
			if(dependencyObject is Visual || dependencyObject is System.Windows.Media.Media3D.Visual3D){
				count=VisualTreeHelper.GetChildrenCount(dependencyObject);
			}
			for(int i=0;i<count;i++){
				listChildren.Add(VisualTreeHelper.GetChild(dependencyObject,i));
			}
			if(count>0){
				return listChildren;
			}
			foreach(object objectChild in LogicalTreeHelper.GetChildren(dependencyObject)){
				if(objectChild is DependencyObject dependencyObjectChild){
					listChildren.Add(dependencyObjectChild);
				}
			}
			return listChildren;
		}

		///<summary>The layout containers that are their own tab group (KeyboardNavigation Local in their constructors).</summary>
		private static bool IsNestedContainer(UIElement uIElement){
			return uIElement is GroupBox || uIElement is Panel || uIElement is PanelAutoScroll;
		}
	}
}
