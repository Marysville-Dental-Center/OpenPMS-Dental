// The module toolbars and the main menu as their source code lays them out (package UI-6 of docs/specs/ui-refresh-plan.md, section 7's UI-6 row;
// docs/port-notes/UI-6.md): the buttons of each module's LayoutToolBar method and the top-level items of the main window's LayoutMenu, read from
// the source in the order the code adds them, so the toolbar tests and pictures follow the program without running its code (which needs a
// database). Every button the code can add is read, also those behind a preference or a program link check (marked conditional), so a test sees
// the longest toolbar an office can have, short of the program link buttons (office content, ProgramL.LoadToolBar). No window, no database.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace UiRefreshTests.Toolbars {
	///<summary>A toolbar of the program read from its source.</summary>
	public class ToolBarSource {
		///<summary>Short name for files and reports: main, appts, family, account, txplan, chart, imaging, imaging-paint, imaging-draw.</summary>
		public string Id;
		///<summary>The module or window it belongs to, for people.</summary>
		public string Title;
		///<summary>The source file, relative to the repository root.</summary>
		public string File;
		///<summary>The method that lays it out.</summary>
		public string Method;
		///<summary>The toolbar's field.</summary>
		public string Field;
		///<summary>The type whose designer creates the toolbar's ImageList (built off screen for it), or null for none.</summary>
		public string TypeImageList;
		///<summary>The field of the ImageList in that type.</summary>
		public string FieldImageList;
		///<summary>True for the WPF toolbar (WpfControls.UI.ToolBar, the Imaging module's main toolbar).</summary>
		public bool IsWpf;
		///<summary>The toolbar's width at 100% for a module area of the width (the window's client width less the module bar), from the layout code:
		///docked toolbars take the module's width.</summary>
		public Func<int,int> FuncWidth;
		///<summary>How FuncWidth is computed, for the report.</summary>
		public string WidthRule;
		///<summary>The buttons, in the order the code adds them.</summary>
		public List<ToolBarButtonSource> ListButtons=new List<ToolBarButtonSource>();
		///<summary>What the reader could not resolve, for the report.</summary>
		public List<string> ListNotes=new List<string>();
	}

	///<summary>One button as the code makes it.</summary>
	public class ToolBarButtonSource {
		///<summary>The caption as written (Lan.g's English text), "" for none.</summary>
		public string Text="";
		///<summary>NormalButton, DropDownButton, ToggleButton, Separator, Label or PageNav.</summary>
		public string Style="NormalButton";
		///<summary>The ImageList index, -1 for none.</summary>
		public int ImageIndex=-1;
		///<summary>The EnumIcons member, "" for none.</summary>
		public string Icon="";
		///<summary>True when the code adds it only under a condition (a preference, a program link, a loop).</summary>
		public bool IsConditional;

		public ToolBarButtonSource Copy() {
			return (ToolBarButtonSource)MemberwiseClone();
		}

		public override string ToString() {
			return Style=="Separator" ? "|" : Text;
		}
	}

	///<summary>A top-level item of the main menu.</summary>
	public class MenuItemSource {
		public string Text;
		public bool IsConditional;
	}

	public static class ToolBarSources {
		///<summary>FormODBase's left and right borders at 96 DPI (LayoutManagerForms._widthBorder96, 5 px each): a window's client width is its width
		///less 10 px with the program's own frame.</summary>
		public const int WIDTH_BORDERS96=10;
		///<summary>The module bar's width at 96 DPI (FormOpenDental: moduleBar.Size 51 x 626, docked left).</summary>
		public const int WIDTH_MODULE_BAR96=51;
		///<summary>The window widths of the UI-6 row: 1024, 1366 and 1920 px.</summary>
		public static readonly int[] ArrayWindowWidths=new int[] { 1024,1366,1920 };

		///<summary>The module area's width (the modules and the main toolbar) for a window width, at 100%.</summary>
		public static int GetModuleWidth(int widthWindow) {
			return widthWindow-WIDTH_BORDERS96-WIDTH_MODULE_BAR96;
		}

		///<summary>The main menu's width for a window width (docked top, the client's width), at 100%.</summary>
		public static int GetMenuWidth(int widthWindow) {
			return widthWindow-WIDTH_BORDERS96;
		}

		///<summary>The toolbars of the main window and the seven modules (Manage has none of its own: the main toolbar), read from the source.</summary>
		public static List<ToolBarSource> ReadAll() {
			List<ToolBarSource> list=new List<ToolBarSource>();
			list.Add(new ToolBarSource { Id="main",Title="Main toolbar (every module)",File=@"OpenPmsDental\Main Modules\FormOpenDental.cs",Method="LayoutToolBar",
				Field="ToolBarMain",TypeImageList="OpenPmsDental.FormOpenDental",FieldImageList="imageListMain",FuncWidth=x => x,
				WidthRule="docked top beside the module bar: the module area's width" });
			list.Add(new ToolBarSource { Id="appts",Title="Appointments",File=@"OpenPmsDental\Main Modules\ControlAppt.cs",Method="LayoutToolBar",
				Field="toolBarMain",TypeImageList="OpenPmsDental.ControlAppt",FieldImageList="imageListMain",FuncWidth=x => x,WidthRule="docked top: the module's width" });
			list.Add(new ToolBarSource { Id="family",Title="Family",File=@"OpenPmsDental\Main Modules\ControlFamily.cs",Method="LayoutToolBar",
				Field="toolBarMain",TypeImageList="OpenPmsDental.ControlFamily",FieldImageList="imageListToolBar",FuncWidth=x => x,WidthRule="docked top: the module's width" });
			list.Add(new ToolBarSource { Id="account",Title="Account",File=@"OpenPmsDental\Main Modules\ControlAccount.cs",Method="LayoutToolBar",
				Field="ToolBarMain",TypeImageList="OpenPmsDental.ControlAccount",FieldImageList="imageListMain",FuncWidth=x => x,WidthRule="docked top: the module's width" });
			list.Add(new ToolBarSource { Id="txplan",Title="Treatment Plan",File=@"OpenPmsDental\Main Modules\ControlTreat.cs",Method="LayoutToolBar",
				Field="ToolBarMain",TypeImageList="OpenPmsDental.ControlTreat",FieldImageList="imageListMain",FuncWidth=x => x,WidthRule="docked top: the module's width" });
			list.Add(new ToolBarSource { Id="chart",Title="Chart",File=@"OpenPmsDental\Main Modules\ControlChart.cs",Method="LayoutToolBar",
				Field="ToolBarMain",TypeImageList="OpenPmsDental.ControlChart",FieldImageList="imageListMain",FuncWidth=x => x,WidthRule="docked top: the module's width" });
			list.Add(new ToolBarSource { Id="imaging",Title="Imaging (WPF toolbar)",File=@"OpenPmsDental\Main Modules\ControlImages.cs",Method="LayoutToolBars",
				Field="toolBarMain",IsWpf=true,FuncWidth=x => x,WidthRule="ControlImages.LayoutControls: elementHostToolBarMain is the module's width" });
			list.Add(new ToolBarSource { Id="imaging-paint",Title="Imaging (image toolbar)",File=@"OpenPmsDental\Main Modules\ControlImages.cs",Method="LayoutToolBars",
				Field="toolBarPaint",TypeImageList="OpenPmsDental.ControlImages",FieldImageList="imageListTools2",FuncWidth=x => x-(4+154+4+231+1),
				WidthRule="ControlImages.LayoutControls: right of the windowing (154 px) and zoom (231 px) sliders, to the module's right edge" });
			list.Add(new ToolBarSource { Id="imaging-draw",Title="Imaging (drawing toolbar)",File=@"OpenPmsDental\Main Modules\ControlImages.cs",Method="LayoutToolBarDraw",
				Field="toolBarDraw",TypeImageList="OpenPmsDental.ControlImages",FieldImageList="imageListTools2",FuncWidth=x => x,
				WidthRule="shown in place of the image toolbars while drawing: taken as the module's width" });
			string root=Scan.RepoPaths.Root();
			foreach(ToolBarSource toolBarSource in list) {
				string body=GetMethodBody(File.ReadAllText(Path.Combine(root,toolBarSource.File)),toolBarSource.Method);
				if(toolBarSource.IsWpf) {
					ReadWpfButtons(toolBarSource,body);
				}
				else {
					ReadButtons(toolBarSource,body);
				}
			}
			return list;
		}

		///<summary>The main menu's top-level items (FormOpenDentalMenus.cs, LayoutMenu), in order.</summary>
		public static List<MenuItemSource> ReadMainMenu() {
			string text=File.ReadAllText(Path.Combine(Scan.RepoPaths.Root(),@"OpenPmsDental\Main Modules\FormOpenDentalMenus.cs"));
			string body=RemoveComments(GetMethodBody(text,"LayoutMenu"));
			Dictionary<string,string> dictVars=new Dictionary<string,string>();
			List<MenuItemSource> list=new List<MenuItemSource>();
			foreach(Statement statement in SplitStatements(body)) {
				Match match=Regex.Match(statement.Text,@"^(MenuItemOD\s+)?(?<var>[\w\.]+)\s*=\s*new\s+MenuItemOD\((?<args>.*)\)$",RegexOptions.Singleline);
				if(match.Success) {
					dictVars[match.Groups["var"].Value]=GetText(SplitArgs(match.Groups["args"].Value)[0]);
					continue;
				}
				match=Regex.Match(statement.Text,@"^menuMain\.Add\((?<arg>.*)\)$",RegexOptions.Singleline);
				if(!match.Success) {
					continue;
				}
				string arg=match.Groups["arg"].Value.Trim();
				Match matchNew=Regex.Match(arg,@"^new\s+MenuItemOD\((?<args>.*)\)$",RegexOptions.Singleline);
				string textItem;
				if(matchNew.Success) {
					textItem=GetText(SplitArgs(matchNew.Groups["args"].Value)[0]);
				}
				else if(dictVars.ContainsKey(arg)) {
					textItem=dictVars[arg];
				}
				else {
					List<string> listArgs=SplitArgs(arg);
					textItem=GetText(listArgs[0]);
				}
				list.Add(new MenuItemSource { Text=textItem,IsConditional=statement.Depth>0 });
			}
			return list;
		}

		#region WinForms toolbar
		private static void ReadButtons(ToolBarSource toolBarSource,string body) {
			Dictionary<string,ToolBarButtonSource> dictVars=new Dictionary<string,ToolBarButtonSource>();
			string fieldPattern=Regex.Escape(toolBarSource.Field);
			foreach(Statement statement in SplitStatements(RemoveComments(body))) {
				string text=statement.Text;
				Match match=Regex.Match(text,@"^(ODToolBarButton\s+)?(?<var>[\w\.]+)\s*=\s*new\s+ODToolBarButton\((?<args>.*)\)$",RegexOptions.Singleline);
				if(match.Success) {
					dictVars[match.Groups["var"].Value]=ParseButton(match.Groups["args"].Value,toolBarSource);
					continue;
				}
				match=Regex.Match(text,@"^(?<var>[\w\.]+)\.Style\s*=\s*ODToolBarButtonStyle\.(?<style>\w+)$");
				if(match.Success) {
					if(dictVars.ContainsKey(match.Groups["var"].Value)) {
						dictVars[match.Groups["var"].Value].Style=match.Groups["style"].Value;
					}
					continue;
				}
				match=Regex.Match(text,@"^(this\.)?"+fieldPattern+@"\.Buttons\.Add\((?<arg>.*)\)$",RegexOptions.Singleline);
				if(!match.Success) {
					if(Regex.IsMatch(text,@"\bProgramL\.LoadToolBar\(")) {
						toolBarSource.ListNotes.Add("then the program link buttons the office set up (ProgramL.LoadToolBar), not read");
					}
					continue;
				}
				string arg=match.Groups["arg"].Value.Trim();
				ToolBarButtonSource toolBarButtonSource;
				Match matchNew=Regex.Match(arg,@"^new\s+ODToolBarButton\((?<args>.*)\)$",RegexOptions.Singleline);
				if(matchNew.Success) {
					toolBarButtonSource=ParseButton(matchNew.Groups["args"].Value,toolBarSource);
				}
				else if(dictVars.ContainsKey(arg)) {
					toolBarButtonSource=dictVars[arg].Copy();
				}
				else {
					toolBarSource.ListNotes.Add("Buttons.Add("+arg+"): not read");
					continue;
				}
				toolBarButtonSource.IsConditional=statement.Depth>0;
				toolBarSource.ListButtons.Add(toolBarButtonSource);
			}
		}

		///<summary>The arguments of an ODToolBarButton constructor: (style), (text,imageIndex,toolTip,tag), (text,EnumIcons,toolTip,tag) or
		///(text,click,icon,...).</summary>
		private static ToolBarButtonSource ParseButton(string args,ToolBarSource toolBarSource) {
			List<string> listArgs=SplitArgs(args);
			ToolBarButtonSource toolBarButtonSource=new ToolBarButtonSource();
			if(listArgs.Count==1) {
				Match matchStyle=Regex.Match(listArgs[0],@"ODToolBarButtonStyle\.(?<style>\w+)");
				if(matchStyle.Success) {
					toolBarButtonSource.Style=matchStyle.Groups["style"].Value;
					return toolBarButtonSource;
				}
			}
			toolBarButtonSource.Text=GetText(listArgs[0]);
			if(toolBarButtonSource.Text==null) {
				toolBarSource.ListNotes.Add("a caption that is not a literal ("+listArgs[0].Trim()+"), read as its expression");
				toolBarButtonSource.Text=listArgs[0].Trim();
			}
			for(int i=1;i<listArgs.Count;i++) {
				Match matchIcon=Regex.Match(listArgs[i],@"\bEnumIcons\.(?<icon>\w+)");
				if(matchIcon.Success) {
					toolBarButtonSource.Icon=matchIcon.Groups["icon"].Value;
					break;
				}
			}
			if(listArgs.Count>1) {
				int imageIndex;
				if(int.TryParse(listArgs[1].Trim(),out imageIndex)) {
					toolBarButtonSource.ImageIndex=imageIndex;
				}
			}
			return toolBarButtonSource;
		}
		#endregion WinForms toolbar

		#region WPF toolbar
		private static void ReadWpfButtons(ToolBarSource toolBarSource,string body) {
			Dictionary<string,string> dictStyles=new Dictionary<string,string>();
			string fieldPattern=Regex.Escape(toolBarSource.Field);
			foreach(Statement statement in SplitStatements(RemoveComments(body))) {
				string text=statement.Text;
				Match match=Regex.Match(text,@"^(WpfControls\.UI\.)?ToolBarButtonStyle\s+(?<var>\w+)\s*=\s*(WpfControls\.UI\.)?ToolBarButtonStyle\.(?<style>\w+)$");
				if(match.Success) {
					dictStyles[match.Groups["var"].Value]=match.Groups["style"].Value;
					continue;
				}
				if(Regex.IsMatch(text,@"^"+fieldPattern+@"\.AddSeparator\(\)$")) {
					toolBarSource.ListButtons.Add(new ToolBarButtonSource { Style="Separator",IsConditional=statement.Depth>0 });
					continue;
				}
				match=Regex.Match(text,@"^"+fieldPattern+@"\.Add\((?<args>.*)\)$",RegexOptions.Singleline);
				if(!match.Success) {
					if(Regex.IsMatch(text,@"\bProgramL\.LoadToolBar\(")) {
						toolBarSource.ListNotes.Add("then the program link buttons the office set up (ProgramL.LoadToolBar), not read");
					}
					continue;
				}
				List<string> listArgs=SplitArgs(match.Groups["args"].Value);
				ToolBarButtonSource toolBarButtonSource=new ToolBarButtonSource { IsConditional=statement.Depth>0 };
				toolBarButtonSource.Text=GetText(listArgs[0])??listArgs[0].Trim();
				foreach(string arg in listArgs.Skip(1)) {
					Match matchIcon=Regex.Match(arg,@"\bEnumIcons\.(?<icon>\w+)");
					if(matchIcon.Success) {
						toolBarButtonSource.Icon=matchIcon.Groups["icon"].Value;
					}
					Match matchStyle=Regex.Match(arg,@"ToolBarButtonStyle\.(?<style>\w+)");
					if(matchStyle.Success) {
						toolBarButtonSource.Style=matchStyle.Groups["style"].Value;
					}
					string argValue=Regex.Replace(arg.Trim(),@"^\w+\s*:\s*","");
					if(dictStyles.ContainsKey(argValue)) {
						toolBarButtonSource.Style=dictStyles[argValue];
					}
				}
				toolBarSource.ListButtons.Add(toolBarButtonSource);
			}
		}
		#endregion WPF toolbar

		#region Source text
		///<summary>A statement of a method body and its brace depth inside the body (0: not inside an if, else, loop or block).</summary>
		public class Statement {
			public string Text;
			public int Depth;
		}

		///<summary>The text between the braces of the first method of that name.</summary>
		public static string GetMethodBody(string text,string method) {
			Match match=Regex.Match(text,@"\bvoid\s+"+Regex.Escape(method)+@"\s*\([^)]*\)\s*\{");
			if(!match.Success) {
				throw new Exception("No method "+method);
			}
			int start=match.Index+match.Length;
			int depth=1;
			bool isString=false;
			for(int i=start;i<text.Length;i++) {
				char c=text[i];
				if(isString) {
					if(c=='\\') {
						i++;
					}
					else if(c=='"') {
						isString=false;
					}
					continue;
				}
				if(c=='"') {
					isString=true;
				}
				else if(c=='{') {
					depth++;
				}
				else if(c=='}') {
					depth--;
					if(depth==0) {
						return text.Substring(start,i-start);
					}
				}
			}
			throw new Exception("The method "+method+" does not end.");
		}

		///<summary>The text without // and /* */ comments (strings kept).</summary>
		public static string RemoveComments(string text) {
			StringBuilder stringBuilder=new StringBuilder();
			for(int i=0;i<text.Length;i++) {
				char c=text[i];
				if(c=='"') {
					int end=i+1;
					while(end<text.Length && text[end]!='"') {
						if(text[end]=='\\') {
							end++;
						}
						end++;
					}
					stringBuilder.Append(text,i,Math.Min(end+1,text.Length)-i);
					i=end;
					continue;
				}
				if(c=='/' && i+1<text.Length && text[i+1]=='/') {
					while(i<text.Length && text[i]!='\n') {
						i++;
					}
					stringBuilder.Append('\n');
					continue;
				}
				if(c=='/' && i+1<text.Length && text[i+1]=='*') {
					int end=text.IndexOf("*/",i+2,StringComparison.Ordinal);
					i=end<0 ? text.Length : end+1;
					continue;
				}
				stringBuilder.Append(c);
			}
			return stringBuilder.ToString();
		}

		///<summary>The statements of a body: split at semicolons outside parentheses and strings, each without the if, else, loop and braces in front of
		///it, with its brace depth.</summary>
		public static List<Statement> SplitStatements(string body) {
			List<Statement> list=new List<Statement>();
			int depthBrace=0;
			int depthParen=0;
			int depthStatement=0;
			bool isString=false;
			StringBuilder stringBuilder=new StringBuilder();
			for(int i=0;i<body.Length;i++) {
				char c=body[i];
				if(isString) {
					stringBuilder.Append(c);
					if(c=='\\' && i+1<body.Length) {
						stringBuilder.Append(body[i+1]);
						i++;
					}
					else if(c=='"') {
						isString=false;
					}
					continue;
				}
				if(c=='"') {
					isString=true;
					stringBuilder.Append(c);
					continue;
				}
				if(c=='(') {
					depthParen++;
				}
				else if(c==')') {
					depthParen--;
				}
				else if(depthParen==0 && (c=='{' || c=='}')) {
					depthBrace+=c=='{' ? 1 : -1;
					stringBuilder.Clear();
					continue;
				}
				if(c==';' && depthParen==0) {
					string text=StripLeadingControl(stringBuilder.ToString(),out bool isUnderControl);
					list.Add(new Statement { Text=text,Depth=depthBrace+(isUnderControl ? 1 : 0) });
					stringBuilder.Clear();
					continue;
				}
				stringBuilder.Append(c);
			}
			return list;
		}

		///<summary>The statement without a leading if(...), else, for(...), foreach(...) or while(...) of a statement without braces.</summary>
		private static string StripLeadingControl(string text,out bool isUnderControl) {
			isUnderControl=false;
			text=text.Trim();
			while(true) {
				Match match=Regex.Match(text,@"^(else\b\s*)?(if|for|foreach|while)\s*\(");
				if(match.Success) {
					int depth=1;
					int i=match.Length;
					for(;i<text.Length && depth>0;i++) {
						if(text[i]=='(') {
							depth++;
						}
						else if(text[i]==')') {
							depth--;
						}
					}
					text=text.Substring(i).Trim();
					isUnderControl=true;
					continue;
				}
				match=Regex.Match(text,@"^else\b");
				if(match.Success) {
					text=text.Substring(match.Length).Trim();
					isUnderControl=true;
					continue;
				}
				return text;
			}
		}

		///<summary>The arguments of a call, split at the commas outside parentheses and strings.</summary>
		public static List<string> SplitArgs(string args) {
			List<string> list=new List<string>();
			int depth=0;
			bool isString=false;
			StringBuilder stringBuilder=new StringBuilder();
			for(int i=0;i<args.Length;i++) {
				char c=args[i];
				if(isString) {
					stringBuilder.Append(c);
					if(c=='\\' && i+1<args.Length) {
						stringBuilder.Append(args[i+1]);
						i++;
					}
					else if(c=='"') {
						isString=false;
					}
					continue;
				}
				if(c=='"') {
					isString=true;
				}
				else if(c=='(') {
					depth++;
				}
				else if(c==')') {
					depth--;
				}
				else if(c==',' && depth==0) {
					list.Add(stringBuilder.ToString());
					stringBuilder.Clear();
					continue;
				}
				stringBuilder.Append(c);
			}
			list.Add(stringBuilder.ToString());
			return list;
		}

		///<summary>The English text of a caption argument: "X", Lan.g(this,"X"), Lans.g(...,"X") (the last string literal); null when it has none.</summary>
		public static string GetText(string arg) {
			MatchCollection matchCollection=Regex.Matches(arg,@"""(?<text>(\\.|[^""\\])*)""");
			if(matchCollection.Count==0) {
				return null;
			}
			return Regex.Unescape(matchCollection[matchCollection.Count-1].Groups["text"].Value);
		}
		#endregion Source text
	}
}
