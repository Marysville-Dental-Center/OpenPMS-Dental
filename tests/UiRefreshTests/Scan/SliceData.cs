// UI-12T (docs/port-notes/UI-12.md, "Tooling (UI-12T)"): the grid's data as folders of one file per rollout slice, so two slices running at
// the same time never edit the same file: Data\grid-windows, grid-hand, grid-known, grid-runtime-texts, fit-fixes-superseded and
// layout-snapshot hold pilot.tsv, 1.tsv, 2.tsv, ... (the snapshot also unsliced.tsv, the designer files no slice owns). Every file of a folder
// has the same column header; the tests read the union in slice order. tools/UiFit/split_grid_data.py (slice_data.py) is the Python side.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace UiRefreshTests.Scan {
	///<summary>One data row of a slice folder: the slice (its file's name), the line and its fields.</summary>
	public class SliceLine {
		public string Slice="";
		public string Line="";
		public string[] ArrayFields=new string[0];
	}

	public static class SliceData {
		///<summary>The layout snapshot's file for the designer files no slice owns.</summary>
		public const string UNSLICED="unsliced";

		///<summary>The folder of a data set, e.g. Data\grid-windows.</summary>
		public static string Folder(string name) {
			return RepoPaths.DataFile(name);
		}

		///<summary>The order of slice files: pilot, the numbered slices by number, other names, unsliced last.</summary>
		public static int CompareSlices(string sliceA,string sliceB) {
			Tuple<int,int,string> tupleA=SliceKey(sliceA);
			Tuple<int,int,string> tupleB=SliceKey(sliceB);
			int compare=tupleA.Item1.CompareTo(tupleB.Item1);
			if(compare==0) {
				compare=tupleA.Item2.CompareTo(tupleB.Item2);
			}
			if(compare==0) {
				compare=string.CompareOrdinal(tupleA.Item3,tupleB.Item3);
			}
			return compare;
		}

		private static Tuple<int,int,string> SliceKey(string slice) {
			if(slice=="pilot") {
				return Tuple.Create(0,0,"");
			}
			if(slice.Length>0 && slice.All(char.IsDigit)) {
				return Tuple.Create(1,int.Parse(slice),"");
			}
			return Tuple.Create(slice==UNSLICED ? 3 : 2,0,slice);
		}

		///<summary>The .tsv files of a folder in slice order (empty when the folder does not exist).</summary>
		public static List<string> Files(string folder) {
			if(!Directory.Exists(folder)) {
				return new List<string>();
			}
			List<string> listFiles=Directory.GetFiles(folder,"*.tsv").ToList();
			listFiles.Sort((a,b) => CompareSlices(SliceOf(a),SliceOf(b)));
			return listFiles;
		}

		///<summary>The slice of a slice file: its name without .tsv.</summary>
		public static string SliceOf(string path) {
			return Path.GetFileNameWithoutExtension(path);
		}

		///<summary>The data rows of every file of a folder, in slice order. Comment lines (#) and blank lines are skipped; each file's first other
		///line is its column header and must be header.</summary>
		public static List<SliceLine> ReadLines(string folder,string header) {
			List<SliceLine> listLines=new List<SliceLine>();
			foreach(string file in Files(folder)) {
				bool hasHeader=false;
				foreach(string line in File.ReadAllLines(file)) {
					if(line.Trim()=="" || (!hasHeader && line.StartsWith("#"))) {
						continue;
					}
					if(!hasHeader) {
						if(line!=header) {
							throw new FormatException(file+": the header is \""+line+"\", not \""+header+"\".");
						}
						hasHeader=true;
						continue;
					}
					listLines.Add(new SliceLine { Slice=SliceOf(file),Line=line,ArrayFields=line.Split('\t') });
				}
			}
			return listLines;
		}

		///<summary>The keys (the given columns, tab-joined) that rows of more than one file of the folder have, each with its slices.</summary>
		public static List<string> KeysInTwoFiles(List<SliceLine> listLines,params int[] arrayColumns) {
			Dictionary<string,SortedSet<string>> dictSlices=new Dictionary<string,SortedSet<string>>();
			foreach(SliceLine sliceLine in listLines) {
				string key=string.Join("\t",arrayColumns.Select(x => x<sliceLine.ArrayFields.Length ? sliceLine.ArrayFields[x] : ""));
				if(!dictSlices.ContainsKey(key)) {
					dictSlices[key]=new SortedSet<string>();
				}
				dictSlices[key].Add(sliceLine.Slice);
			}
			return dictSlices.Where(x => x.Value.Count>1).Select(x => x.Key+" ("+string.Join(", ",x.Value)+")").ToList();
		}

		///<summary>window -> slice, from Data\grid-windows.</summary>
		public static Dictionary<string,string> WindowSlices() {
			Dictionary<string,string> dictSlices=new Dictionary<string,string>();
			foreach(SliceLine sliceLine in ReadLines(Folder("grid-windows"),"window\tslice\tmode\tnote")) {
				if(!dictSlices.ContainsKey(sliceLine.ArrayFields[0])) {
					dictSlices[sliceLine.ArrayFields[0]]=sliceLine.Slice;
				}
			}
			return dictSlices;
		}

		///<summary>A designer file's class name: its base name without .Designer.cs (or .cs), lower case.</summary>
		public static string DesignerBase(string designerFile) {
			return Regex.Replace(Path.GetFileName(designerFile),@"(\.designer)?\.cs$","",RegexOptions.IgnoreCase).ToLowerInvariant();
		}

		///<summary>The owner of each designer file for the layout snapshot: the first slice (in slice order) with a window of the file's class name
		///or a grid-hand row in the file; UNSLICED when none. slice_data.py's snapshot_owner is the same rule.</summary>
		public class SnapshotOwners {
			private readonly Dictionary<string,string> _dictByName=new Dictionary<string,string>();
			private readonly Dictionary<string,string> _dictByFile=new Dictionary<string,string>();

			public SnapshotOwners(Dictionary<string,string> dictWindowSlices,IEnumerable<string[]> listHandRows) {
				foreach(KeyValuePair<string,string> pair in dictWindowSlices) {
					Add(_dictByName,pair.Key.Split('.').Last().ToLowerInvariant(),pair.Value);
				}
				foreach(string[] arrayFields in listHandRows) {
					string slice;
					if(dictWindowSlices.TryGetValue(arrayFields[1],out slice)) {
						Add(_dictByFile,arrayFields[0],slice);
					}
				}
			}

			private static void Add(Dictionary<string,string> dict,string key,string slice) {
				string sliceOld;
				if(!dict.TryGetValue(key,out sliceOld) || CompareSlices(slice,sliceOld)<0) {
					dict[key]=slice;
				}
			}

			public string Owner(string designerFile) {
				string slice=UNSLICED;
				string sliceFound;
				if(_dictByName.TryGetValue(DesignerBase(designerFile),out sliceFound) && CompareSlices(sliceFound,slice)<0) {
					slice=sliceFound;
				}
				if(_dictByFile.TryGetValue(designerFile,out sliceFound) && CompareSlices(sliceFound,slice)<0) {
					slice=sliceFound;
				}
				return slice;
			}

			///<summary>The owners from the repository's Data\grid-windows and Data\grid-hand.</summary>
			public static SnapshotOwners FromData() {
				List<string[]> listHand=ReadLines(Folder("grid-hand"),"designer_file\twindow\tcontrol\tproperty\told\tnew\trule").Select(x => x.ArrayFields).ToList();
				return new SnapshotOwners(WindowSlices(),listHand);
			}
		}
	}
}
