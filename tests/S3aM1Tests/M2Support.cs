// Helpers for the tests of package S3a-M2 (docs/port-notes/S3a-M2.md): the program modules' menus on ContextMenuStrip. They describe a
// strip's items the way the tests compare them, read the handlers wired to an item or a strip, find the repository's files and run a test
// body on an STA thread (the modules hold WPF hosts). Nothing is shown.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace S3aM1Tests {
	public static class M2Support {
		///<summary>Runs action on a new STA thread with the en-US culture and rethrows what it throws, with its stack.</summary>
		public static void RunSta(Action action) {
			ExceptionDispatchInfo exceptionDispatchInfo=null;
			Thread thread=new Thread(() => {
				try {
					S3aM1Db.SetCultureUs();
					action();
				}
				catch(Exception ex) {
					exceptionDispatchInfo=ExceptionDispatchInfo.Capture(ex);
				}
			});
			thread.SetApartmentState(ApartmentState.STA);
			thread.Start();
			thread.Join();
			exceptionDispatchInfo?.Throw();
		}

		///<summary>The path of a file of the repository, from its parts below the root (the folder that holds OpenDental.sln).</summary>
		public static string RepoFile(params string[] arrayParts) {
			DirectoryInfo directoryInfo=new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
			while(directoryInfo!=null && !File.Exists(Path.Combine(directoryInfo.FullName,"OpenDental.sln"))) {
				directoryInfo=directoryInfo.Parent;
			}
			Assert.IsNotNull(directoryInfo,"The repository root (OpenDental.sln) above "+AppDomain.CurrentDomain.BaseDirectory);
			return Path.Combine(new[] { directoryInfo.FullName }.Concat(arrayParts).ToArray());
		}

		public static string ReadRepoFile(params string[] arrayParts) {
			return File.ReadAllText(RepoFile(arrayParts)).Replace("\r\n","\n");
		}

		///<summary>The names of the methods subscribed to the event of a component (EventClick of ToolStripItem, EventOpening of ToolStripDropDown).</summary>
		public static List<string> GetHandlerNames(Component component,Type typeDeclaring,string eventKeyField) {
			FieldInfo fieldInfoKey=typeDeclaring.GetField(eventKeyField,BindingFlags.NonPublic|BindingFlags.Static);
			Assert.IsNotNull(fieldInfoKey,typeDeclaring.Name+"."+eventKeyField);
			EventHandlerList eventHandlerList=(EventHandlerList)typeof(Component).GetProperty("Events",BindingFlags.NonPublic|BindingFlags.Instance)
				.GetValue(component);
			Delegate delegateHandlers=eventHandlerList[fieldInfoKey.GetValue(null)];
			if(delegateHandlers==null) {
				return new List<string>();
			}
			return delegateHandlers.GetInvocationList().Select(x => x.Method.Name).ToList();
		}

		public static List<string> GetClickHandlerNames(ToolStripItem toolStripItem) {
			return GetHandlerNames(toolStripItem,typeof(ToolStripItem),"EventClick");
		}

		public static List<string> GetOpeningHandlerNames(ToolStripDropDown toolStripDropDown) {
			return GetHandlerNames(toolStripDropDown,typeof(ToolStripDropDown),"EventOpening");
		}

		///<summary>An item as the tests compare it: "-" for a separator; otherwise its text, " ->" and its click handlers, " [Ctrl+D]" for a shortcut,
		///" (hidden)" when not Available, " (disabled)", " (checked)", and its submenu's items in braces. The same as the expected lists, which
		///were written from the WinForms menus on main.</summary>
		public static string Describe(ToolStripItem toolStripItem) {
			if(toolStripItem is ToolStripSeparator) {
				return "-";
			}
			string text=toolStripItem.Text;
			List<string> listHandlers=GetClickHandlerNames(toolStripItem);
			if(listHandlers.Count>0) {
				text+=" ->"+string.Join("+",listHandlers);
			}
			if(toolStripItem is ToolStripMenuItem toolStripMenuItem && toolStripMenuItem.ShortcutKeys!=Keys.None) {
				text+=" ["+new KeysConverter().ConvertToInvariantString(toolStripMenuItem.ShortcutKeys)+"]";
			}
			if(!toolStripItem.Available) {
				text+=" (hidden)";
			}
			if(!toolStripItem.Enabled) {
				text+=" (disabled)";
			}
			if(toolStripItem is ToolStripMenuItem toolStripMenuItemChecked && toolStripMenuItemChecked.Checked) {
				text+=" (checked)";
			}
			if(toolStripItem is ToolStripMenuItem toolStripMenuItemSub && toolStripMenuItemSub.DropDownItems.Count>0) {
				text+=" {"+string.Join(", ",toolStripMenuItemSub.DropDownItems.Cast<ToolStripItem>().Select(x => Describe(x)))+"}";
			}
			return text;
		}

		public static List<string> Describe(ToolStrip toolStrip) {
			return toolStrip.Items.Cast<ToolStripItem>().Select(x => Describe(x)).ToList();
		}

		///<summary>Raises the strip's Opening as showing it over control does, and returns whether the opening stays cancelled. ToolStripDropDown
		///starts it cancelled when no item will show (no item is Available).</summary>
		public static bool Open(ContextMenuStrip contextMenuStrip,Control control) {
			bool isSet=false;
			foreach(string name in new[] { "SourceControlInternal","sourceControl","_sourceControl" }) {
				try {
					Priv.Set(contextMenuStrip,name,control);
					isSet=true;
					break;
				}
				catch(MissingMemberException) {
				}
			}
			Assert.IsTrue(isSet,"No settable source control member on ContextMenuStrip.");
			CancelEventArgs cancelEventArgs=new CancelEventArgs(!contextMenuStrip.Items.Cast<ToolStripItem>().Any(x => x.Available));
			Priv.Call(contextMenuStrip,"OnOpening",cancelEventArgs);
			return cancelEventArgs.Cancel;
		}
	}
}
