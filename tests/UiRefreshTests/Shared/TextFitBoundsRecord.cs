// The bounds record of the UI refresh (package UI-0b, docs/specs/ui-refresh-plan.md sections 3.7 and 5.4, docs/port-notes/UI-0b.md): every
// control's Bounds once its window is built, in one TSV format for the tier 1 scan and the harness's group U walker (UI-0c), and the rule that
// compares a record for the font under test with the before set. A control that sizes itself (AutoSize on at run time, a single-line text box,
// a combo box, date picker or up-down box whose height Windows takes from the font) is keyed apart: the edge it is anchored to stays, its size
// may change, and a new overlap with a visible sibling or a new pass over its parent's client edge is a difference unless fit-fixes.tsv lists
// it. Children of a FlowLayoutPanel or TableLayoutPanel are placed by the panel: they are compared by their order, and the panel by its bounds.
// Shared: tests\WindowChecksTests compiles this file in as a linked file (UI-0c), so it uses only Windows Forms, System.Drawing and reflection.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace UiRefreshTests.TextFit {
	///<summary>One control of a built window in the bounds record.</summary>
	public class TextFitBoundsRow {
		///<summary>The window (or hosted UserControl) type's full name, or the harness's window key.</summary>
		public string Window="";
		///<summary>The control's path from the window: names joined by '/', "." for the window itself (TextFitBoundsRecord.GetPath).</summary>
		public string Path="";
		///<summary>The control type's full name.</summary>
		public string Type="";
		///<summary>Its index in its parent's Controls.</summary>
		public int Index;
		///<summary>Bounds in its parent's client coordinates.</summary>
		public Rectangle Bounds;
		public Size ClientSize;
		///<summary>Visible as designed (the control's own Visible state, not its window's, which is never shown here).</summary>
		public bool IsVisible;
		public bool IsSizingItself;
		public AnchorStyles Anchor;
		public DockStyle Dock;
		///<summary>True when its parent is a FlowLayoutPanel or TableLayoutPanel, which places it.</summary>
		public bool IsInLayoutPanel;

		///<summary>The parent's path, or "" for the window itself.</summary>
		public string ParentPath {
			get {
				if(Path==".") {
					return "";
				}
				int index=Path.LastIndexOf('/');
				if(index<0) {
					return ".";
				}
				return Path.Substring(0,index);
			}
		}

		///<summary>The last part of the path: the control's name, or #index:Type for an unnamed one.</summary>
		public string Name {
			get {
				int index=Path.LastIndexOf('/');
				return Path.Substring(index+1);
			}
		}

		public string Key {
			get { return Window+"\t"+Path; }
		}
	}

	///<summary>One difference between a record for the font under test and the before set.</summary>
	public class TextFitBoundsDifference {
		///<summary>bounds (a control that does not size itself moved or changed size), anchored-edge (a control that sizes itself moved the edge
		///it is anchored to), overlap (a new overlap with a sibling), parent-edge (a new pass over the parent's client edge), order (a layout
		///panel's child changed place), missing or added (a control in one record only).</summary>
		public string Kind="";
		public TextFitBoundsRow Row;
		public TextFitBoundsRow RowBefore;
		///<summary>For an overlap, the sibling's path.</summary>
		public string Other="";

		public override string ToString() {
			return Kind+" "+Row.Window+" "+Row.Path+(Other=="" ? "" : " with "+Other);
		}
	}

	public static class TextFitBoundsRecord {
		public const string HEADER="window\tpath\ttype\tindex\tx\ty\twidth\theight\tclient_width\tclient_height\tvisible\tsizes_itself\tanchor\tdock\tlayout_panel_child";
		private static MethodInfo _methodGetState;
		private static object _valueStateVisible;
		private static bool _isStateLookedUp;

		#region Collect
		///<summary>Every control under root, root included (path "."), in the order of a depth-first walk.</summary>
		public static List<TextFitBoundsRow> Collect(string window,Control root) {
			List<TextFitBoundsRow> listRows=new List<TextFitBoundsRow>();
			listRows.Add(NewRow(window,".",root,0));
			CollectChildren(window,".",root,listRows);
			return listRows;
		}

		private static void CollectChildren(string window,string pathParent,Control parent,List<TextFitBoundsRow> listRows) {
			for(int i=0;i<parent.Controls.Count;i++) {
				Control control=parent.Controls[i];
				string path=GetChildPath(pathParent,parent,i);
				listRows.Add(NewRow(window,path,control,i));
				CollectChildren(window,path,control,listRows);
			}
		}

		///<summary>The path of parent.Controls[index]: its name under the parent's path, or #index:Type when it has no name or shares it with a
		///sibling.</summary>
		public static string GetChildPath(string pathParent,Control parent,int index) {
			Control control=parent.Controls[index];
			string segment=control.Name;
			bool isUnique=!string.IsNullOrEmpty(segment);
			if(isUnique) {
				for(int i=0;i<parent.Controls.Count;i++) {
					if(i!=index && parent.Controls[i].Name==segment) {
						isUnique=false;
						break;
					}
				}
			}
			if(!isUnique) {
				segment="#"+index.ToString(CultureInfo.InvariantCulture)+":"+control.GetType().Name;
			}
			if(pathParent=="." || pathParent=="") {
				return segment;
			}
			return pathParent+"/"+segment;
		}

		///<summary>The path of a control under root, as Collect writes it.</summary>
		public static string GetPath(Control root,Control control) {
			if(control==root) {
				return ".";
			}
			List<Control> listChain=new List<Control>();
			for(Control c=control;c!=null && c!=root;c=c.Parent) {
				listChain.Add(c);
			}
			listChain.Reverse();
			string path=".";
			Control parent=root;
			foreach(Control c in listChain) {
				path=GetChildPath(path,parent,parent.Controls.IndexOf(c));
				parent=c;
			}
			return path;
		}

		private static TextFitBoundsRow NewRow(string window,string path,Control control,int index) {
			TextFitBoundsRow row=new TextFitBoundsRow();
			row.Window=window;
			row.Path=path;
			row.Type=control.GetType().FullName;
			row.Index=index;
			row.Bounds=control.Bounds;
			row.ClientSize=control.ClientSize;
			row.IsVisible=IsDesignedVisible(control);
			row.IsSizingItself=IsSizingItself(control);
			row.Anchor=control.Anchor;
			row.Dock=control.Dock;
			row.IsInLayoutPanel=control.Parent is FlowLayoutPanel || control.Parent is TableLayoutPanel;
			return row;
		}

		///<summary>The control's own Visible state, as the designer or code set it. Control.Visible is false for every control of a window that
		///is not shown, so the state is read the way WinForms keeps it (Control.GetState); true when that cannot be read.</summary>
		public static bool IsDesignedVisible(Control control) {
			if(!_isStateLookedUp) {
				_isStateLookedUp=true;
				foreach(MethodInfo methodInfo in typeof(Control).GetMethods(BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public)) {
					if(methodInfo.Name!="GetState" || methodInfo.ReturnType!=typeof(bool)) {
						continue;
					}
					ParameterInfo[] arrayParameters=methodInfo.GetParameters();
					if(arrayParameters.Length!=1 || !arrayParameters[0].ParameterType.IsEnum) {
						continue;
					}
					try {
						_valueStateVisible=Enum.Parse(arrayParameters[0].ParameterType,"Visible");
						_methodGetState=methodInfo;
					}
					catch(ArgumentException) {
					}
					break;
				}
			}
			if(_methodGetState==null) {
				return true;
			}
			return (bool)_methodGetState.Invoke(control,new object[] { _valueStateVisible });
		}

		///<summary>True for a control that sizes itself from its font or text at run time (section 5.3 of the plan): AutoSize on (a single-line
		///text box has it on by default), or a stock combo box, date picker or up-down box, whose height Windows takes from the font.</summary>
		public static bool IsSizingItself(Control control) {
			if(control is Form) {
				return false;
			}
			if(control is TextBoxBase textBoxBase) {
				if(textBoxBase is System.Windows.Forms.TextBox textBox && textBox.Multiline) {
					return false;
				}
				if(textBoxBase is RichTextBox) {
					return false;
				}
				return textBoxBase.AutoSize;
			}
			if(control is System.Windows.Forms.ComboBox comboBox) {
				return comboBox.DropDownStyle!=ComboBoxStyle.Simple;
			}
			if(control is DateTimePicker || control is UpDownBase) {
				return true;
			}
			return control.AutoSize;
		}
		#endregion Collect

		#region Write and read
		public static void Write(TextWriter textWriter,IEnumerable<TextFitBoundsRow> listRows) {
			textWriter.WriteLine(HEADER);
			foreach(TextFitBoundsRow row in listRows) {
				textWriter.WriteLine(ToLine(row));
			}
		}

		public static string ToLine(TextFitBoundsRow row) {
			return string.Join("\t",new string[] {
				Clean(row.Window),Clean(row.Path),Clean(row.Type),Int(row.Index),
				Int(row.Bounds.X),Int(row.Bounds.Y),Int(row.Bounds.Width),Int(row.Bounds.Height),
				Int(row.ClientSize.Width),Int(row.ClientSize.Height),
				row.IsVisible ? "1" : "0",row.IsSizingItself ? "1" : "0",
				((int)row.Anchor).ToString(CultureInfo.InvariantCulture),row.Dock.ToString(),row.IsInLayoutPanel ? "1" : "0" });
		}

		///<summary>Reads a record written by Write. Lines that start with '#' and the header are skipped.</summary>
		public static List<TextFitBoundsRow> Read(TextReader textReader) {
			List<TextFitBoundsRow> listRows=new List<TextFitBoundsRow>();
			string line;
			while((line=textReader.ReadLine())!=null) {
				if(line=="" || line.StartsWith("#") || line==HEADER) {
					continue;
				}
				string[] arrayFields=line.Split('\t');
				if(arrayFields.Length!=15) {
					throw new FormatException("A bounds record line has "+arrayFields.Length+" fields, not 15: "+line);
				}
				TextFitBoundsRow row=new TextFitBoundsRow();
				row.Window=arrayFields[0];
				row.Path=arrayFields[1];
				row.Type=arrayFields[2];
				row.Index=ParseInt(arrayFields[3]);
				row.Bounds=new Rectangle(ParseInt(arrayFields[4]),ParseInt(arrayFields[5]),ParseInt(arrayFields[6]),ParseInt(arrayFields[7]));
				row.ClientSize=new Size(ParseInt(arrayFields[8]),ParseInt(arrayFields[9]));
				row.IsVisible=arrayFields[10]=="1";
				row.IsSizingItself=arrayFields[11]=="1";
				row.Anchor=(AnchorStyles)ParseInt(arrayFields[12]);
				row.Dock=(DockStyle)Enum.Parse(typeof(DockStyle),arrayFields[13]);
				row.IsInLayoutPanel=arrayFields[14]=="1";
				listRows.Add(row);
			}
			return listRows;
		}

		private static string Clean(string value) {
			return (value??"").Replace("\t"," ").Replace("\r"," ").Replace("\n"," ");
		}

		private static string Int(int value) {
			return value.ToString(CultureInfo.InvariantCulture);
		}

		private static int ParseInt(string value) {
			return int.Parse(value,NumberStyles.Integer,CultureInfo.InvariantCulture);
		}
		#endregion Write and read

		#region Compare
		///<summary>The fit-fixes key of a row: the window and the control's name (the last part of its path).</summary>
		public static string GetFitFixKey(string window,string controlName) {
			return window+"\t"+controlName;
		}

		///<summary>Compares a record for the font under test with the before set by the rules of section 5.4. setFitFixes holds GetFitFixKey keys
		///of the controls fit-fixes.tsv lists; their differences are not reported.</summary>
		public static List<TextFitBoundsDifference> Compare(List<TextFitBoundsRow> listBefore,List<TextFitBoundsRow> listAfter,ICollection<string> setFitFixes) {
			List<TextFitBoundsDifference> listDifferences=new List<TextFitBoundsDifference>();
			Dictionary<string,TextFitBoundsRow> dictBefore=new Dictionary<string,TextFitBoundsRow>();
			foreach(TextFitBoundsRow row in listBefore) {
				dictBefore[row.Key]=row;
			}
			HashSet<string> setAfter=new HashSet<string>();
			foreach(TextFitBoundsRow row in listAfter) {
				setAfter.Add(row.Key);
				if(setFitFixes!=null && setFitFixes.Contains(GetFitFixKey(row.Window,row.Name))) {
					continue;
				}
				TextFitBoundsRow rowBefore;
				if(!dictBefore.TryGetValue(row.Key,out rowBefore)) {
					listDifferences.Add(new TextFitBoundsDifference { Kind="added",Row=row });
					continue;
				}
				string kind=CompareRow(rowBefore,row);
				if(kind!="") {
					listDifferences.Add(new TextFitBoundsDifference { Kind=kind,Row=row,RowBefore=rowBefore });
				}
			}
			foreach(TextFitBoundsRow row in listBefore) {
				if(!setAfter.Contains(row.Key)) {
					listDifferences.Add(new TextFitBoundsDifference { Kind="missing",Row=row,RowBefore=row });
				}
			}
			HashSet<string> setOverlapsBefore=new HashSet<string>(FindOverlaps(listBefore).Select(x => OverlapKey(x)));
			foreach(TextFitBoundsDifference difference in FindOverlaps(listAfter)) {
				if(setOverlapsBefore.Contains(OverlapKey(difference))) {
					continue;
				}
				if(setFitFixes!=null && setFitFixes.Contains(GetFitFixKey(difference.Row.Window,difference.Row.Name))) {
					continue;
				}
				listDifferences.Add(difference);
			}
			return listDifferences;
		}

		private static string OverlapKey(TextFitBoundsDifference difference) {
			return difference.Kind+"\t"+difference.Row.Key+"\t"+difference.Other;
		}

		///<summary>"" when the row keeps to its rule, else the kind of difference.</summary>
		public static string CompareRow(TextFitBoundsRow rowBefore,TextFitBoundsRow row) {
			if(row.IsInLayoutPanel || rowBefore.IsInLayoutPanel) {
				if(row.Index!=rowBefore.Index) {
					return "order";
				}
				return "";
			}
			if(row.IsSizingItself || rowBefore.IsSizingItself) {
				if(GetAnchoredX(rowBefore)!=GetAnchoredX(row) || GetAnchoredY(rowBefore)!=GetAnchoredY(row)) {
					return "anchored-edge";
				}
				return "";
			}
			if(row.Bounds!=rowBefore.Bounds) {
				return "bounds";
			}
			return "";
		}

		///<summary>The horizontal edge a control that sizes itself keeps: its right edge when anchored right only, else its left edge.</summary>
		public static int GetAnchoredX(TextFitBoundsRow row) {
			if((row.Anchor&(AnchorStyles.Left|AnchorStyles.Right))==AnchorStyles.Right) {
				return row.Bounds.Right;
			}
			return row.Bounds.Left;
		}

		///<summary>The vertical edge a control that sizes itself keeps: its bottom edge when anchored bottom only, else its top edge.</summary>
		public static int GetAnchoredY(TextFitBoundsRow row) {
			if((row.Anchor&(AnchorStyles.Top|AnchorStyles.Bottom))==AnchorStyles.Bottom) {
				return row.Bounds.Bottom;
			}
			return row.Bounds.Top;
		}

		///<summary>Every visible control that sizes itself and overlaps a visible sibling (overlap) or passes its parent's client edge
		///(parent-edge). Children of a layout panel are left out (the panel places them); a docked control or sibling is left out (docking fills
		///space by design).</summary>
		public static List<TextFitBoundsDifference> FindOverlaps(List<TextFitBoundsRow> listRows) {
			List<TextFitBoundsDifference> listOverlaps=new List<TextFitBoundsDifference>();
			Dictionary<string,TextFitBoundsRow> dictByKey=new Dictionary<string,TextFitBoundsRow>();
			Dictionary<string,List<TextFitBoundsRow>> dictChildren=new Dictionary<string,List<TextFitBoundsRow>>();
			foreach(TextFitBoundsRow row in listRows) {
				dictByKey[row.Key]=row;
				if(row.Path==".") {
					continue;
				}
				string keyParent=row.Window+"\t"+row.ParentPath;
				List<TextFitBoundsRow> listChildren;
				if(!dictChildren.TryGetValue(keyParent,out listChildren)) {
					listChildren=new List<TextFitBoundsRow>();
					dictChildren[keyParent]=listChildren;
				}
				listChildren.Add(row);
			}
			foreach(TextFitBoundsRow row in listRows) {
				if(!row.IsSizingItself || !row.IsVisible || row.IsInLayoutPanel || row.Dock!=DockStyle.None || row.Path==".") {
					continue;
				}
				string keyParent=row.Window+"\t"+row.ParentPath;
				TextFitBoundsRow rowParent;
				if(dictByKey.TryGetValue(keyParent,out rowParent)) {
					if(row.Bounds.Right>rowParent.ClientSize.Width || row.Bounds.Bottom>rowParent.ClientSize.Height) {
						listOverlaps.Add(new TextFitBoundsDifference { Kind="parent-edge",Row=row });
					}
				}
				foreach(TextFitBoundsRow rowSibling in dictChildren[keyParent]) {
					if(rowSibling==row || !rowSibling.IsVisible || rowSibling.Dock!=DockStyle.None) {
						continue;
					}
					//Two controls that both size themselves: the pair once, under the first path.
					if(rowSibling.IsSizingItself && !rowSibling.IsInLayoutPanel && string.CompareOrdinal(rowSibling.Path,row.Path)<0) {
						continue;
					}
					if(row.Bounds.IntersectsWith(rowSibling.Bounds)) {
						listOverlaps.Add(new TextFitBoundsDifference { Kind="overlap",Row=row,Other=rowSibling.Path });
					}
				}
			}
			return listOverlaps;
		}
		#endregion Compare
	}
}
