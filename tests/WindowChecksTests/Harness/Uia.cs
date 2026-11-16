// UI Automation for the few steps an out-of-process check needs (a menu's items, a tool's buttons and fields), always scoped to one window of
// the process and always with a timeout: a search from the desktop root took minutes in the spike. Patterns only (Expand, Invoke, Value,
// SelectionItem): no input is sent. A WinForms MenuStrip (RdlDesigner's main menu since S3a-M3a, which has no native HMENU) is reached by its
// items' texts through the strip (ChooseMenuStripItem, WINCHK-4).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Automation;

namespace WindowChecksTests {
	public static class Uia {
		///<summary>Runs the UI Automation call with a timeout; default(T) and a note in error when it fails or does not answer.</summary>
		public static T Try<T>(Func<T> func,int timeoutMs,out string error) {
			error="";
			Task<T> task=Task.Run(func);
			try {
				if(!task.Wait(timeoutMs)) {
					error="UI Automation did not answer within "+timeoutMs/1000+" s";
					return default(T);
				}
				return task.Result;
			}
			catch(AggregateException ex) {
				error=ex.GetBaseException().GetType().Name+": "+ex.GetBaseException().Message;
				return default(T);
			}
		}

		public static AutomationElement Find(AutomationElement root,ControlType controlType,string name,TreeScope treeScope=TreeScope.Descendants) {
			Condition condition=new PropertyCondition(AutomationElement.ControlTypeProperty,controlType);
			if(name!=null) {
				condition=new AndCondition(condition,new PropertyCondition(AutomationElement.NameProperty,name));
			}
			return root.FindFirst(treeScope,condition);
		}

		public static List<string> Names(AutomationElement root,ControlType controlType) {
			return root.FindAll(TreeScope.Descendants,new PropertyCondition(AutomationElement.ControlTypeProperty,controlType)).Cast<AutomationElement>()
				.Select(x => x.Current.Name).ToList();
		}

		///<summary>Expands the main menu's item (by name) of the window and returns the names of its items, as UI Automation sees them once the
		///drop-down is open: from the item itself, else from the process's drop-down windows.</summary>
		public static List<string> GetMenuItems(IntPtr hwndMain,int processId,string menuName,out AutomationElement automationElementMenu) {
			AutomationElement automationElementMain=AutomationElement.FromHandle(hwndMain);
			automationElementMenu=Find(automationElementMain,ControlType.MenuItem,menuName);
			if(automationElementMenu==null) {
				return null;
			}
			((ExpandCollapsePattern)automationElementMenu.GetCurrentPattern(ExpandCollapsePattern.Pattern)).Expand();
			System.Threading.Thread.Sleep(700);
			List<string> listNames=automationElementMenu.FindAll(TreeScope.Descendants,new PropertyCondition(AutomationElement.ControlTypeProperty,ControlType.MenuItem))
				.Cast<AutomationElement>().Select(x => x.Current.Name).ToList();
			if(listNames.Count>0) {
				return listNames;
			}
			foreach(IntPtr hwnd in Native.GetProcessWindows(processId).Where(x => x!=hwndMain)) {
				AutomationElement automationElement=AutomationElement.FromHandle(hwnd);
				listNames.AddRange(Names(automationElement,ControlType.MenuItem));
			}
			return listNames;
		}

		///<summary>Invokes the menu item (by name) in the process's open drop-down windows or under the menu element.</summary>
		public static bool InvokeMenuItem(IntPtr hwndMain,int processId,AutomationElement automationElementMenu,string itemName) {
			AutomationElement automationElementItem=Find(automationElementMenu,ControlType.MenuItem,itemName);
			if(automationElementItem==null) {
				foreach(IntPtr hwnd in Native.GetProcessWindows(processId).Where(x => x!=hwndMain)) {
					automationElementItem=Find(AutomationElement.FromHandle(hwnd),ControlType.MenuItem,itemName);
					if(automationElementItem!=null) {
						break;
					}
				}
			}
			if(automationElementItem==null) {
				return false;
			}
			((InvokePattern)automationElementItem.GetCurrentPattern(InvokePattern.Pattern)).Invoke();
			return true;
		}

		///<summary>Collapses an expanded menu item (closing its drop-down).</summary>
		public static void Collapse(AutomationElement automationElementMenu) {
			try {
				((ExpandCollapsePattern)automationElementMenu.GetCurrentPattern(ExpandCollapsePattern.Pattern)).Collapse();
			}
			catch(Exception) {
			}
		}

		///<summary>What ChooseMenuStripItem found and did.</summary>
		public class MenuChoice {
			///<summary>Every item of the path is in the menu strip.</summary>
			public bool IsFound;
			///<summary>The last item of the path is disabled (after its menu opened, so the menu's DropDownOpening handlers set its state): not chosen.
			///</summary>
			public bool IsDisabled;
			///<summary>The last item was invoked (its Click ran, or was still running when the wait ended).</summary>
			public bool IsInvoked;
			///<summary>The items of each level, "File, Edit, View / Designer, RDL Text, Preview", for the check's evidence.</summary>
			public string Found="";
			///<summary>How the item was reached and chosen.</summary>
			public string Note="";
			///<summary>Why UI Automation failed, or "".</summary>
			public string Error="";
		}

		///<summary>An item's text as a person reads it: without ampersands and without a shortcut after a tab.</summary>
		public static string CleanMenuText(string text) {
			text=text??"";
			int indexTab=text.IndexOf('\t');
			if(indexTab>=0) {
				text=text.Substring(0,indexTab);
			}
			return text.Replace("&","").Trim();
		}

		///<summary>Chooses the item at the path ("View", "RDL Text") of a WinForms MenuStrip in the window, out of process, through UI Automation (a
		///MenuStrip has no native HMENU, so GetMenu and WM_COMMAND do not reach it): the menu bar among the window's children whose items hold the
		///path's first text, each menu of the path expanded (ExpandCollapsePattern, which shows the drop-down and raises its DropDownOpening, as a
		///person opening it does, so the items' Enabled states are current), the item found by its text under the opened menu or in the process's
		///drop-down windows, and invoked (InvokePattern: the item's PerformClick) when it is enabled. Menus left open are collapsed again. No input
		///is sent. Every call has a timeout.</summary>
		public static MenuChoice ChooseMenuStripItem(IntPtr hwndMain,int processId,string[] arrayPath,int timeoutMs=45000) {
			MenuChoice menuChoice=new MenuChoice();
			Try(() => ChooseMenuStripItemCore(hwndMain,processId,arrayPath,menuChoice),timeoutMs,out string error);
			if(error!="") {
				menuChoice.Error=error;
			}
			return menuChoice;
		}

		private static bool ChooseMenuStripItemCore(IntPtr hwndMain,int processId,string[] arrayPath,MenuChoice menuChoice) {
			AutomationElement automationElementMain=AutomationElement.FromHandle(hwndMain);
			Condition conditionMenuItem=new PropertyCondition(AutomationElement.ControlTypeProperty,ControlType.MenuItem);
			Condition conditionMenuBar=new PropertyCondition(AutomationElement.ControlTypeProperty,ControlType.MenuBar);
			//The window's menu bars: its title bar's system menu (the item "System") and the MenuStrip. The strip is a child control of the form; a
			//strip further down (in a panel) is found in the descendants.
			List<AutomationElement> listBars=automationElementMain.FindAll(TreeScope.Children,conditionMenuBar).Cast<AutomationElement>().ToList();
			AutomationElement automationElementItem=FindMenuStripTopItem(listBars,conditionMenuItem,arrayPath[0],out List<string> listTop);
			if(automationElementItem==null) {
				listBars=automationElementMain.FindAll(TreeScope.Descendants,conditionMenuBar).Cast<AutomationElement>().ToList();
				automationElementItem=FindMenuStripTopItem(listBars,conditionMenuItem,arrayPath[0],out listTop);
			}
			menuChoice.Found=string.Join(", ",listTop.Where(x => x!=""));
			if(automationElementItem==null) {
				return false;
			}
			List<AutomationElement> listExpanded=new List<AutomationElement>();
			try {
				for(int level=1;level<arrayPath.Length;level++) {
					AutomationElement automationElementParent=automationElementItem;
					if(automationElementParent.TryGetCurrentPattern(ExpandCollapsePattern.Pattern,out object patternExpand)) {
						((ExpandCollapsePattern)patternExpand).Expand();
						listExpanded.Add(automationElementParent);
					}
					else {
						menuChoice.Note+=(menuChoice.Note=="" ? "" : "; ")+CleanMenuText(automationElementParent.Current.Name)+" has no ExpandCollapsePattern";
					}
					//The drop-down's items: under the opened menu (a WinForms UI Automation provider exposes them there while the drop-down shows), else in
					//the process's other windows (the drop-down is a top-level window of its own).
					List<AutomationElement> listItems;
					System.Diagnostics.Stopwatch stopwatch=System.Diagnostics.Stopwatch.StartNew();
					while(true) {
						listItems=automationElementParent.FindAll(TreeScope.Children,conditionMenuItem).Cast<AutomationElement>().ToList();
						if(listItems.Count==0) {
							foreach(IntPtr hwnd in Native.GetProcessWindows(processId).Where(x => x!=hwndMain)) {
								listItems.AddRange(AutomationElement.FromHandle(hwnd).FindAll(TreeScope.Descendants,conditionMenuItem).Cast<AutomationElement>());
							}
						}
						if(listItems.Count>0 || stopwatch.ElapsedMilliseconds>5000) {
							break;
						}
						System.Threading.Thread.Sleep(200);
					}
					menuChoice.Found+=" / "+string.Join(", ",listItems.Select(x => CleanMenuText(x.Current.Name)).Where(x => x!=""));
					automationElementItem=listItems.FirstOrDefault(x => string.Equals(CleanMenuText(x.Current.Name),arrayPath[level],StringComparison.OrdinalIgnoreCase));
					if(automationElementItem==null) {
						return false;
					}
				}
				menuChoice.IsFound=true;
				if(!automationElementItem.Current.IsEnabled) {
					menuChoice.IsDisabled=true;
					return false;
				}
				if(!automationElementItem.TryGetCurrentPattern(InvokePattern.Pattern,out object patternInvoke)) {
					menuChoice.Error="the item has no InvokePattern";
					return false;
				}
				//The item's Click runs on the program's UI thread; a handler that shows a modal window does not return until it closes, so the wait is
				//bounded and the item counts as chosen once its Click has started.
				Try(() => {
					((InvokePattern)patternInvoke).Invoke();
					return true;
				},10000,out string errorInvoke);
				if(errorInvoke.StartsWith("UI Automation did not answer")) {
					menuChoice.Note+=(menuChoice.Note=="" ? "" : "; ")+"Invoke had not returned after 10 s (the item's Click still runs)";
				}
				else if(errorInvoke!="") {
					menuChoice.Error="Invoke: "+errorInvoke;
					return false;
				}
				menuChoice.IsInvoked=true;
				menuChoice.Note+=(menuChoice.Note=="" ? "" : "; ")+"opened "+string.Join(" > ",arrayPath.Take(arrayPath.Length-1))+" (ExpandCollapsePattern), "
					+"invoked "+arrayPath[arrayPath.Length-1]+" (InvokePattern)";
				return true;
			}
			finally {
				//A menu still open (the item was not chosen, or its Click left it open) is closed again, innermost first.
				foreach(AutomationElement automationElementExpanded in Enumerable.Reverse(listExpanded)) {
					try {
						if(automationElementExpanded.TryGetCurrentPattern(ExpandCollapsePattern.Pattern,out object pattern)
							&& ((ExpandCollapsePattern)pattern).Current.ExpandCollapseState!=ExpandCollapseState.Collapsed)
						{
							((ExpandCollapsePattern)pattern).Collapse();
						}
					}
					catch(Exception) {
						//Gone with its drop-down.
					}
				}
			}
		}

		///<summary>The top-level item of the menu bars whose text is the name; the texts of the bar that has it (or of every bar) in listTexts.</summary>
		private static AutomationElement FindMenuStripTopItem(List<AutomationElement> listBars,Condition conditionMenuItem,string name,out List<string> listTexts) {
			listTexts=new List<string>();
			foreach(AutomationElement automationElementBar in listBars) {
				List<AutomationElement> listItems=automationElementBar.FindAll(TreeScope.Children,conditionMenuItem).Cast<AutomationElement>().ToList();
				AutomationElement automationElementItem=listItems.FirstOrDefault(x => string.Equals(CleanMenuText(x.Current.Name),name,StringComparison.OrdinalIgnoreCase));
				if(automationElementItem!=null) {
					listTexts=listItems.Select(x => CleanMenuText(x.Current.Name)).ToList();
					return automationElementItem;
				}
				listTexts.AddRange(listItems.Select(x => CleanMenuText(x.Current.Name)));
			}
			return null;
		}

		public static bool Invoke(AutomationElement root,ControlType controlType,string name) {
			AutomationElement automationElement=Find(root,controlType,name);
			if(automationElement==null) {
				return false;
			}
			((InvokePattern)automationElement.GetCurrentPattern(InvokePattern.Pattern)).Invoke();
			return true;
		}

		public static bool SetValue(AutomationElement automationElement,string value) {
			if(automationElement==null) {
				return false;
			}
			object pattern;
			if(automationElement.TryGetCurrentPattern(ValuePattern.Pattern,out pattern)) {
				((ValuePattern)pattern).SetValue(value);
				return true;
			}
			return false;
		}
	}
}
