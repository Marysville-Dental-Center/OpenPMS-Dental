// The custom dental icons of the UI refresh (package UI-8 of docs/specs/ui-refresh-plan.md, section 4.4; docs/port-notes/UI-8.md;
// icons/README.md, "The custom icons"). Original artwork of this program, drawn here as shapes in Fluent UI System Icons' style: designed on
// a 24 unit square and drawn at each design size (16, 20, 24, 32, 48) with the stroke weight of Fluent's Regular icons of that size (1, 1,
// 1.5, 2 and 2.5 px), rounded ends and joins, one colour, and a Filled twin of every icon. The 16 px designs leave out the details that do
// not fit (section 3.5's design-for-slot rule). Nothing is copied from Open Dental's pictures, from IconOrganizer art or from any icon set;
// the shapes of teeth follow public dental anatomy only.
//
// tools/New-DentalIcons.ps1 compiles this file with Add-Type in Windows PowerShell 5.1, so it is C# 5 (no string interpolation, no nameof, no
// ?. operator, no expression-bodied members) and uses only the .NET Framework and WPF's geometry (PresentationCore, WindowsBase): the strokes
// are widened and the parts combined into one outline with WPF's Geometry.Combine, and the result is written as one <path d> of absolute
// M, L, C, Q, A and Z commands (nonzero fill), which tools/New-IconData.ps1 then reads like a Fluent SVG. No window, no rendering.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;

namespace DentalIconTools {
	///<summary>The custom dental icons: their file name stems, the design sizes, and the SVG of each design.</summary>
	public static class DentalIconArt {
		///<summary>The design sizes of every custom icon (section 3.5: 16, 20, 24 and 32, and 48 for the module bar at 150% and above).</summary>
		public static readonly int[] Sizes=new int[] { 16,20,24,32,48 };
		///<summary>The suffix of a Filled twin's file name stem: tooth_filled_24.svg.</summary>
		public const string FILLED_SUFFIX="_filled";
		///<summary>The line inside every SVG that says whose work it is.</summary>
		public const string SVG_COMMENT="Open PMS - Dental custom icon, original artwork, GPL-2.0-or-later (icons/README.md). Made by tools/New-DentalIcons.ps1.";

		///<summary>The Regular icons' file name stems, in manifest order.</summary>
		public static string[] GetStems() {
			return new string[] {
				"tooth","chart_medical","perio_chart","perio_probe",
				"xray_bitewing","xray_periapical","xray_pano","xray_ceph",
				"sensor","phosphor_plate","mount","intraoral_photo","implant","ortho",
				"proc_amalgam","proc_composite","proc_crown_pfm","proc_crown_ceramic","proc_rct","proc_rct_buildup",
			};
		}

		///<summary>The Fluent Regular stroke weight of a design size, in pixels.</summary>
		public static double GetStrokeWidth(int size) {
			switch(size) {
				case 16:
				case 20:
					return 1;
				case 24:
					return 1.5;
				case 32:
					return 2;
				case 48:
					return 2.5;
			}
			throw new ArgumentException("No design size: "+size);
		}

		///<summary>The file name of a design: tooth_24.svg, tooth_filled_24.svg.</summary>
		public static string GetFileName(string stem,int size,bool isFilled) {
			return stem+(isFilled ? FILLED_SUFFIX : "")+"_"+size.ToString(CultureInfo.InvariantCulture)+".svg";
		}

		///<summary>The SVG text of one design: no XML declaration, no line end, the same text every run.</summary>
		public static string GetSvg(string stem,int size,bool isFilled) {
			IconCanvas iconCanvas=new IconCanvas(size,isFilled);
			Draw(stem,iconCanvas);
			string pathData=iconCanvas.GetPathData();
			string sizeText=size.ToString(CultureInfo.InvariantCulture);
			return "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\""+sizeText+"\" height=\""+sizeText+"\" viewBox=\"0 0 "+sizeText+" "+sizeText
				+"\"><!-- "+SVG_COMMENT+" --><path d=\""+pathData+"\"/></svg>";
		}

		#region The shapes, in 24 unit design coordinates
		///<summary>A molar seen from the side: two cusps, the neck, two roots. The stroke's centre line.</summary>
		private const string MOLAR="M 12 5.2 C 10.9 4.1 9.5 3.5 8 3.5 C 5.6 3.5 4 5.3 4 7.9 C 4 9.7 4.7 11.1 5.5 12.3 C 6.1 13.2 6.4 14.3 6.6 15.6 "
			+"L 7.1 19.2 C 7.25 20.2 7.8 20.8 8.6 20.8 C 9.4 20.8 9.85 20.2 10 19.3 L 10.45 16.1 C 10.6 15.2 11.2 14.6 12 14.6 "
			+"C 12.8 14.6 13.4 15.2 13.55 16.1 L 14 19.3 C 14.15 20.2 14.6 20.8 15.4 20.8 C 16.2 20.8 16.75 20.2 16.9 19.2 L 17.4 15.6 "
			+"C 17.6 14.3 17.9 13.2 18.5 12.3 C 19.3 11.1 20 9.7 20 7.9 C 20 5.3 18.4 3.5 16 3.5 C 14.5 3.5 13.1 4.1 12 5.2 Z";
		///<summary>A front tooth (one root) seen from the front: the crown, the neck, the root.</summary>
		private const string INCISOR="M 12 3.5 C 15.2 3.5 17 5.3 17 8.4 C 17 10.4 16.2 11.9 15.4 13.4 L 13.7 19.2 C 13.4 20.2 12.8 20.8 12 20.8 "
			+"C 11.2 20.8 10.6 20.2 10.3 19.2 L 8.6 13.4 C 7.8 11.9 7 10.4 7 8.4 C 7 5.3 8.8 3.5 12 3.5 Z";
		#endregion

		///<summary>Draws one icon on the canvas, in its style and at its size.</summary>
		private static void Draw(string stem,IconCanvas c) {
			switch(stem) {
				case "tooth":
					//The Chart module: a molar. Filled: the solid tooth (the selected module).
					if(c.IsFilled) {
						c.Fill(c.Path(MOLAR));
					}
					else {
						c.Stroke(c.Path(MOLAR));
					}
					return;
				case "chart_medical":
					//The Chart module in the medical layout: a chart card with a medical cross.
					DrawCardWithCross(c);
					return;
				case "perio_chart":
					DrawPerioChart(c);
					return;
				case "perio_probe":
					DrawProbe(c);
					return;
				case "xray_bitewing":
					DrawBitewing(c);
					return;
				case "xray_periapical":
					DrawPeriapical(c);
					return;
				case "xray_pano":
					DrawPano(c);
					return;
				case "xray_ceph":
					DrawCeph(c);
					return;
				case "sensor":
					DrawSensor(c);
					return;
				case "phosphor_plate":
					DrawPhosphorPlate(c);
					return;
				case "mount":
					DrawMount(c);
					return;
				case "intraoral_photo":
					DrawIntraoralPhoto(c);
					return;
				case "implant":
					DrawImplant(c);
					return;
				case "ortho":
					DrawOrtho(c);
					return;
				case "proc_amalgam":
					DrawAmalgam(c);
					return;
				case "proc_composite":
					DrawComposite(c);
					return;
				case "proc_crown_pfm":
					DrawCrown(c,isPfm:true);
					return;
				case "proc_crown_ceramic":
					DrawCrown(c,isPfm:false);
					return;
				case "proc_rct":
					DrawRct(c,isBuildUp:false);
					return;
				case "proc_rct_buildup":
					DrawRct(c,isBuildUp:true);
					return;
			}
			throw new ArgumentException("No custom icon: "+stem);
		}

		#region The icons
		private static void DrawCardWithCross(IconCanvas c) {
			Geometry card=c.Rect(3.5,3.5,20.5,20.5,3);
			Geometry cross=c.Path("M 10 7 L 14 7 L 14 10 L 17 10 L 17 14 L 14 14 L 14 17 L 10 17 L 10 14 L 7 14 L 7 10 L 10 10 Z");
			if(c.IsFilled) {
				c.Fill(card);
				c.Cut(cross);
				return;
			}
			c.Stroke(card);
			c.Stroke(cross);
		}

		private static void DrawPerioChart(IconCanvas c) {
			//A chart: the gum line over three teeth, and the pocket depths as bars below it.
			Geometry frame=c.Rect(3,3,21,21,2.5);
			Geometry gum=c.Path("M 6 9.5 C 7.3 7.6 8.7 7.6 10 9.5 C 11.3 11.4 12.7 11.4 14 9.5 C 15.3 7.6 16.7 7.6 18 9.5");
			Geometry bars;
			if(c.IsSmall) {
				bars=c.Path("M 8 13.5 L 8 17.5 M 12 13.5 L 12 15.5 M 16 13.5 L 16 17.5");
			}
			else {
				bars=c.Path("M 8 13.5 L 8 18 M 12 13.5 L 12 15.5 M 16 13.5 L 16 16.75");
			}
			if(c.IsFilled) {
				c.Fill(frame);
				c.CutStroke(gum,c.W);
				c.CutStroke(bars,c.W);
				return;
			}
			c.Stroke(frame);
			c.Stroke(gum);
			c.Stroke(bars);
		}

		private static void DrawProbe(IconCanvas c) {
			//A periodontal probe: the handle, the shank and the marked tip, lying from bottom left to top right.
			Geometry handleLine=c.Path("M 4.75 19.25 L 11.5 12.5");
			Geometry handle=c.Widen(handleLine,3.5*c.K);
			Geometry shank=c.Path("M 11.5 12.5 L 16.5 7.5 L 19.5 3.5");
			if(c.IsFilled) {
				c.Fill(handle);
			}
			else {
				c.Stroke(handle);
			}
			c.Stroke(shank);
			if(!c.IsSmall) {
				//The markings: two short ticks across the tip.
				c.Stroke(c.Path("M 13.6 9.1 L 14.9 10.4 M 15.1 7.6 L 16.4 8.9"));
			}
		}

		private static void DrawBitewing(IconCanvas c) {
			//A landscape film with its bite tab, and the upper and lower teeth meeting at the bite line.
			Geometry film=c.Rect(2.5,5,21.5,16.5,2.5);
			Geometry tab=c.Rect(10,16.5,14,19.75,1.25);
			Geometry teeth;
			if(c.IsSmall) {
				teeth=c.Path("M 5.5 10.75 L 18.5 10.75 M 12 8 L 12 13.5");
			}
			else {
				teeth=c.Path("M 5.5 10.75 L 18.5 10.75 M 9.25 8 L 9.25 13.5 M 14.75 8 L 14.75 13.5");
			}
			if(c.IsFilled) {
				c.Fill(film);
				c.Fill(tab);
				c.CutStroke(teeth,c.W);
				return;
			}
			c.Stroke(film);
			c.Stroke(tab);
			c.Stroke(teeth);
		}

		private static void DrawPeriapical(IconCanvas c) {
			//A portrait film with one whole tooth, crown to root tip.
			Geometry film=c.Rect(5.5,2.5,18.5,21.5,2.5);
			Geometry tooth=c.Path(INCISOR,0.6,4.8,4.8);
			if(c.IsFilled) {
				c.Fill(film);
				c.Cut(tooth);
				return;
			}
			c.Stroke(film);
			if(c.IsSmall) {
				c.FillExact(tooth);
			}
			else {
				c.Stroke(tooth);
			}
		}

		private static void DrawPano(IconCanvas c) {
			//A wide film with both dental arches curving along it.
			Geometry film=c.Rect(2,6,22,18,2.5);
			Geometry arches;
			if(c.IsSmall) {
				arches=c.Path("M 5.5 10 C 9.5 13.5 14.5 13.5 18.5 10");
			}
			else {
				arches=c.Path("M 5.5 9.5 C 9.5 12.75 14.5 12.75 18.5 9.5 M 6.5 13 C 10 15.75 14 15.75 17.5 13");
			}
			if(c.IsFilled) {
				c.Fill(film);
				c.CutStroke(arches,c.W);
				return;
			}
			c.Stroke(film);
			c.Stroke(arches);
		}

		private static void DrawCeph(IconCanvas c) {
			//A square film with a head seen from the side, facing left: forehead, nose, lips, chin.
			Geometry film=c.Rect(3,3,21,21,2.5);
			Geometry profile;
			if(c.IsSmall) {
				profile=c.Path("M 15 6.5 C 12.5 7 11.5 8.75 11.75 10.75 L 9.5 13 L 11.5 14 L 11.5 15.5 C 11.5 17 12.5 17.75 14.5 17.75");
			}
			else {
				profile=c.Path("M 15.5 6 C 12.75 6.5 11.75 8.5 12 10.5 L 9.75 12.75 C 9.5 13 9.6 13.4 10 13.5 L 11.75 13.75 L 11.25 14.75 "
					+"L 11.9 15.4 L 11.6 16.25 C 11.4 17.4 12.5 18 14 18 L 15.25 18");
			}
			if(c.IsFilled) {
				c.Fill(film);
				c.CutStroke(profile,c.W);
				return;
			}
			c.Stroke(film);
			c.Stroke(profile);
		}

		private static void DrawSensor(IconCanvas c) {
			//An intraoral sensor: the rounded sensor with its active area, and its cable.
			Geometry sensor=c.Rect(3.5,3,15.5,15.5,3);
			Geometry cable=c.Path("M 9.5 15.5 C 9.5 18.75 11.5 20.5 14.75 20.5 L 20.5 20.5");
			if(c.IsFilled) {
				c.Fill(sensor);
				if(!c.IsSmall) {
					//The active area, set off by a cut line.
					c.CutStroke(c.Rect(7,6.5,12,12,1),c.W);
				}
				c.Stroke(cable);
				return;
			}
			c.Stroke(sensor);
			if(!c.IsSmall) {
				c.Stroke(c.Rect(7,6.5,12,12,1));
			}
			c.Stroke(cable);
		}

		private static void DrawPhosphorPlate(IconCanvas c) {
			//A phosphor plate: the thin plate, its orientation dot and a sheen.
			Geometry plate=c.Rect(5,2.5,19,21.5,2);
			Geometry dot=c.Circle(8.5,6,c.IsSmall ? 1.25 : 1.1);
			Geometry sheen;
			if(c.IsSmall) {
				sheen=c.Path("M 9.5 17.5 L 15.5 11.5");
			}
			else {
				sheen=c.Path("M 9 18 L 15.5 11.5 M 12.5 18.5 L 16 15");
			}
			if(c.IsFilled) {
				c.Fill(plate);
				c.CutExact(dot);
				c.CutStroke(sheen,c.W);
				return;
			}
			c.Stroke(plate);
			c.FillExact(dot);
			c.Stroke(sheen);
		}

		private static void DrawMount(IconCanvas c) {
			//A mount: a frame holding a row of films over a row of films.
			Geometry frame=c.Rect(2,4,22,20,2.5);
			List<Geometry> listFilms=new List<Geometry>();
			if(c.IsSmall) {
				listFilms.Add(c.Rect(5,7,10.75,11.25,1));
				listFilms.Add(c.Rect(13.25,7,19,11.25,1));
				listFilms.Add(c.Rect(5,12.75,10.75,17,1));
				listFilms.Add(c.Rect(13.25,12.75,19,17,1));
			}
			else {
				double[] arrayX=new double[] { 4.75,10.25,15.75 };
				foreach(double x in arrayX) {
					listFilms.Add(c.Rect(x,6.75,x+3.5,11.25,0.75));
					listFilms.Add(c.Rect(x,12.75,x+3.5,17.25,0.75));
				}
			}
			if(c.IsFilled) {
				c.Fill(frame);
				foreach(Geometry film in listFilms) {
					c.CutExact(film);
				}
				return;
			}
			c.Stroke(frame);
			foreach(Geometry film in listFilms) {
				c.Stroke(film);
			}
		}

		private static void DrawIntraoralPhoto(IconCanvas c) {
			//An intraoral photo: a landscape photo with a molar in it.
			Geometry photo=c.Rect(2.5,4,21.5,20,3);
			Geometry tooth=c.Path(MOLAR,0.55,5.4,5.5);
			if(c.IsFilled) {
				c.Fill(photo);
				c.Cut(tooth);
				return;
			}
			c.Stroke(photo);
			if(c.IsSmall) {
				c.FillExact(tooth);
			}
			else {
				c.Stroke(tooth);
			}
		}

		private static void DrawImplant(IconCanvas c) {
			//An implant: the crown, the abutment and the threaded fixture.
			Geometry crown=c.Path("M 7 7 C 7 4.75 9 3.5 12 3.5 C 15 3.5 17 4.75 17 7 C 17 8 16.4 8.5 15.4 8.5 L 8.6 8.5 C 7.6 8.5 7 8 7 7 Z");
			Geometry abutment=c.Path("M 10.5 8.5 L 10.5 11.25 M 13.5 8.5 L 13.5 11.25");
			Geometry body=c.Path("M 8.75 11.25 L 15.25 11.25 L 14.4 19.25 C 14.25 20.3 13.3 21 12 21 C 10.7 21 9.75 20.3 9.6 19.25 Z");
			Geometry threads;
			if(c.IsSmall) {
				threads=c.Path("M 7.5 15.25 L 16.5 15.25");
			}
			else {
				threads=c.Path("M 7.25 14 L 16.75 14 M 7.75 17 L 16.25 17");
			}
			if(c.IsFilled) {
				c.Fill(crown);
				c.Stroke(abutment);
				c.Fill(body);
				c.Stroke(threads);
				return;
			}
			c.Stroke(crown);
			c.Stroke(abutment);
			c.Stroke(body);
			c.Stroke(threads);
		}

		private static void DrawOrtho(IconCanvas c) {
			//Orthodontics: a front tooth seen from the front, a bracket on it and the arch wire through the bracket.
			Geometry tooth=c.Path("M 7 6 C 7 4.3 9 3.5 12 3.5 C 15 3.5 17 4.3 17 6 L 17.5 18 C 17.55 19.3 16.9 20.5 15.5 20.5 L 8.5 20.5 "
				+"C 7.1 20.5 6.45 19.3 6.5 18 Z");
			Geometry bracket=c.Rect(9,9.5,15,14.5,1.25);
			if(c.IsFilled) {
				//The solid tooth; the bracket set off by a cut ring; the wire white across the tooth and dark beyond it.
				c.Fill(tooth);
				c.CutStroke(bracket,c.W*2.2);
				c.FillExact(bracket);
				c.CutStroke(c.Path("M 6.5 12 L 8.5 12 M 15.5 12 L 17.5 12"),c.W);
				c.Stroke(c.Path("M 2.5 12 L 5.5 12 M 18.5 12 L 21.5 12"));
				return;
			}
			c.Stroke(tooth);
			c.Stroke(bracket);
			c.Stroke(c.Path("M 2.5 12 L 21.5 12"));
		}

		private static void DrawAmalgam(IconCanvas c) {
			//An amalgam filling: a molar with a solid filling in its crown.
			Geometry tooth=c.Path(MOLAR);
			Geometry filling=c.Path("M 9 7.25 C 9 6.5 9.6 6.15 10.25 6.4 L 12 7.1 L 13.75 6.4 C 14.4 6.15 15 6.5 15 7.25 L 15 9 C 15 9.6 14.6 10 14 10 L 10 10 C 9.4 10 9 9.6 9 9 Z");
			if(c.IsFilled) {
				c.Fill(tooth);
				c.CutStroke(filling,c.W*2);
				c.FillExact(filling);
				return;
			}
			c.Stroke(tooth);
			c.FillExact(filling);
		}

		private static void DrawComposite(IconCanvas c) {
			//A composite filling: a front tooth with a filling at the corner of its crown.
			Geometry tooth=c.Path(INCISOR);
			Geometry filling=c.Path("M 8.6 6.6 C 9.3 5.6 10.4 5.1 11.6 5.1 L 11.6 8.4 C 11.6 9.2 11 9.75 10.2 9.75 L 8.75 9.75 C 8.4 9.75 8.25 9.4 8.25 8.9 "
				+"C 8.25 8 8.3 7.2 8.6 6.6 Z");
			if(c.IsFilled) {
				c.Fill(tooth);
				c.CutStroke(filling,c.W*2);
				c.FillExact(filling);
				return;
			}
			c.Stroke(tooth);
			c.FillExact(filling);
		}

		private static void DrawCrown(IconCanvas c,bool isPfm) {
			//A crown: a molar whose crown is capped down to the margin. PFM: a metal collar at the margin; ceramic: a shine on the cap.
			Geometry tooth=c.Path(MOLAR);
			Geometry cap=c.Intersect(c.Path(MOLAR),c.Path("M 0 0 L 24 0 L 24 11.75 L 0 11.75 Z"));
			if(c.IsFilled) {
				c.Fill(tooth);
			}
			else {
				c.Stroke(tooth);
				c.Fill(cap);
			}
			if(isPfm) {
				//The collar: a cut line just above the margin, across the whole cap.
				c.CutStroke(c.Path("M 2 9.75 L 22 9.75"),c.W*0.85);
			}
			else {
				//The shine: a short curved cut in the cap's upper left.
				c.CutStroke(c.Path("M 6.6 8.3 C 6.6 7 7.2 6.2 8.4 6.1"),c.W);
			}
			if(c.IsFilled) {
				//The margin, as a cut line, so the Filled crown still shows where the cap ends.
				c.CutStroke(c.Path("M 2 12.75 L 22 12.75"),c.W*0.85);
			}
		}

		private static void DrawRct(IconCanvas c,bool isBuildUp) {
			//A root canal: a molar with its pulp chamber and both canals filled; with a build-up, a solid core in the crown.
			Geometry tooth=c.Path(MOLAR);
			Geometry canals=c.Path("M 10.6 9.75 C 9.75 12.25 9.1 15.25 8.6 19 M 13.4 9.75 C 14.25 12.25 14.9 15.25 15.4 19");
			Geometry core;
			if(isBuildUp) {
				core=c.Path("M 8.25 6.75 C 8.25 6.2 8.7 5.75 9.25 5.75 L 14.75 5.75 C 15.3 5.75 15.75 6.2 15.75 6.75 L 15.75 9.5 C 15.75 10.4 15.1 11 14.25 11 L 9.75 11 "
					+"C 8.9 11 8.25 10.4 8.25 9.5 Z");
			}
			else {
				core=c.Path("M 10 9 C 10 8.45 10.45 8 11 8 L 13 8 C 13.55 8 14 8.45 14 9 L 14 10.25 L 10 10.25 Z");
			}
			if(c.IsFilled) {
				//The solid tooth with the canals and the chamber (or the core) cut out.
				c.Fill(tooth);
				c.CutStroke(canals,c.W);
				c.CutExact(core);
				return;
			}
			c.Stroke(tooth);
			c.Stroke(canals);
			c.FillExact(core);
		}
		#endregion
	}

	///<summary>One design being drawn: the size, the style, the stroke weight, the scale from the 24 unit design square, and the shape so far.
	///Every shape is made in design pixels (0 to Size) and combined into one outline.</summary>
	public sealed class IconCanvas {
		///<summary>How far a curve may stray from its flattening, in design pixels (Geometry.Combine and widening).</summary>
		private const double TOLERANCE=0.005;
		private static readonly Regex _regexNumber=new Regex(@"[+-]?(?:\d+\.?\d*|\.\d+)");
		public readonly int Size;
		public readonly bool IsFilled;
		///<summary>The stroke weight, in design pixels.</summary>
		public readonly double W;
		///<summary>Design pixels per unit of the 24 unit square.</summary>
		public readonly double K;
		private Geometry _geometry=Geometry.Empty;

		public IconCanvas(int size,bool isFilled) {
			Size=size;
			IsFilled=isFilled;
			W=DentalIconArt.GetStrokeWidth(size);
			K=size/24.0;
		}

		///<summary>The 16 px design, which leaves out the details that do not fit.</summary>
		public bool IsSmall {
			get {
				return Size<20;
			}
		}

		#region Shapes
		///<summary>Path data in 24 unit coordinates (absolute M, L, C, Q and Z only, every number a coordinate), scaled to the design.</summary>
		public Geometry Path(string pathData) {
			return Path(pathData,1,0,0);
		}

		///<summary>Path data placed by x*scale+offsetX, y*scale+offsetY in the 24 unit square, then scaled to the design.</summary>
		public Geometry Path(string pathData,double scale,double offsetX,double offsetY) {
			if(Regex.IsMatch(pathData,"[^MLCQZ0-9 .+-]")) {
				throw new ArgumentException("Only absolute M, L, C, Q and Z: "+pathData);
			}
			int index=0;
			string pathScaled=_regexNumber.Replace(pathData,match => {
				double value=double.Parse(match.Value,CultureInfo.InvariantCulture);
				double offset=(index%2==0) ? offsetX : offsetY;
				index++;
				return ((value*scale+offset)*K).ToString("R",CultureInfo.InvariantCulture);
			});
			if(index%2!=0) {
				throw new ArgumentException("The numbers do not come in pairs: "+pathData);
			}
			return Geometry.Parse("F1 "+pathScaled);
		}

		///<summary>A rounded rectangle's stroke centre line, from 24 unit coordinates, with its edges placed so the stroke's outer edge falls on whole
		///design pixels (crisp straight edges, as Fluent's frames).</summary>
		public Geometry Rect(double x0,double y0,double x1,double y1,double radius) {
			double left=SnapLow(x0);
			double top=SnapLow(y0);
			double right=SnapHigh(x1);
			double bottom=SnapHigh(y1);
			double radiusDesign=Math.Max(radius*K,W/2);
			return new RectangleGeometry(new Rect(left,top,right-left,bottom-top),radiusDesign,radiusDesign);
		}

		private double SnapLow(double value) {
			return Math.Round(value*K-W/2,MidpointRounding.AwayFromZero)+W/2;
		}

		private double SnapHigh(double value) {
			return Math.Round(value*K+W/2,MidpointRounding.AwayFromZero)-W/2;
		}

		///<summary>A circle, from 24 unit coordinates.</summary>
		public Geometry Circle(double centerX,double centerY,double radius) {
			return new EllipseGeometry(new Point(centerX*K,centerY*K),radius*K,radius*K);
		}

		///<summary>The area a stroke of the width covers along the shape's lines, with round ends and joins.</summary>
		public Geometry Widen(Geometry geometry,double width) {
			Pen pen=new Pen(Brushes.Black,width);
			pen.StartLineCap=PenLineCap.Round;
			pen.EndLineCap=PenLineCap.Round;
			pen.LineJoin=PenLineJoin.Round;
			return geometry.GetWidenedPathGeometry(pen,TOLERANCE,ToleranceType.Absolute);
		}

		///<summary>The area both shapes cover.</summary>
		public Geometry Intersect(Geometry geometryA,Geometry geometryB) {
			return Geometry.Combine(geometryA,geometryB,GeometryCombineMode.Intersect,null,TOLERANCE,ToleranceType.Absolute);
		}

		///<summary>The area a closed shape encloses.</summary>
		private static Geometry Area(Geometry geometry) {
			return Geometry.Combine(geometry,Geometry.Empty,GeometryCombineMode.Union,null,TOLERANCE,ToleranceType.Absolute);
		}

		///<summary>The area a closed shape encloses together with its stroke: the shape as its Regular outline's outer edge.</summary>
		private Geometry AreaWithStroke(Geometry geometry) {
			return Geometry.Combine(Area(geometry),Widen(geometry,W),GeometryCombineMode.Union,null,TOLERANCE,ToleranceType.Absolute);
		}
		#endregion

		#region Drawing
		///<summary>Draws the shape's lines with the design's stroke weight.</summary>
		public void Stroke(Geometry geometry) {
			Add(Widen(geometry,W));
		}

		///<summary>Draws the shape's lines with another stroke width, in design pixels.</summary>
		public void Stroke(Geometry geometry,double width) {
			Add(Widen(geometry,width));
		}

		///<summary>Fills a closed shape out to its Regular outline's outer edge (the Filled style's silhouette).</summary>
		public void Fill(Geometry geometry) {
			Add(AreaWithStroke(geometry));
		}

		///<summary>Fills exactly the area a closed shape encloses (small solid details).</summary>
		public void FillExact(Geometry geometry) {
			Add(Area(geometry));
		}

		///<summary>Cuts a closed shape, out to its Regular outline's outer edge, out of what is drawn so far.</summary>
		public void Cut(Geometry geometry) {
			Remove(AreaWithStroke(geometry));
		}

		///<summary>Cuts exactly the area a closed shape encloses out of what is drawn so far.</summary>
		public void CutExact(Geometry geometry) {
			Remove(Area(geometry));
		}

		///<summary>Cuts a stroke of the width along the shape's lines out of what is drawn so far (the Filled style's inner lines).</summary>
		public void CutStroke(Geometry geometry,double width) {
			Remove(Widen(geometry,width));
		}

		private void Add(Geometry geometry) {
			_geometry=Geometry.Combine(_geometry,geometry,GeometryCombineMode.Union,null,TOLERANCE,ToleranceType.Absolute);
		}

		private void Remove(Geometry geometry) {
			_geometry=Geometry.Combine(_geometry,geometry,GeometryCombineMode.Exclude,null,TOLERANCE,ToleranceType.Absolute);
		}
		#endregion

		#region Output
		///<summary>The outline as SVG path data: absolute M, L, C and Z, numbers rounded to 2 decimals. WPF's outline of combined shapes is a fine
		///polyline; each figure is fitted with lines and cubic curves again (CurveFitter), as Fluent's SVGs are drawn. Checks that the fitted
		///outline stays within FIT_ERROR of the shape, inside the design square, and fills the same with the nonzero and the even-odd rule (the
		///generator reads nonzero only).</summary>
		public string GetPathData() {
			PathGeometry pathGeometry=_geometry.GetFlattenedPathGeometry(TOLERANCE,ToleranceType.Absolute);
			pathGeometry=Geometry.Combine(pathGeometry,Geometry.Empty,GeometryCombineMode.Union,null,TOLERANCE,ToleranceType.Absolute)
				.GetFlattenedPathGeometry(TOLERANCE,ToleranceType.Absolute);
			if(pathGeometry.Figures.Count==0) {
				throw new InvalidOperationException("The design is empty.");
			}
			StringBuilder stringBuilder=new StringBuilder();
			foreach(PathFigure pathFigure in pathGeometry.Figures) {
				List<Point> listPoints=new List<Point>();
				listPoints.Add(pathFigure.StartPoint);
				foreach(PathSegment pathSegment in pathFigure.Segments) {
					AddPoints(listPoints,pathSegment);
				}
				CurveFitter.AppendClosedFigure(stringBuilder,listPoints);
			}
			string pathData=stringBuilder.ToString().Trim();
			//The text read back must fill the same with both rules: no figure may overlap another the wrong way round.
			Geometry geometryNonzero=Geometry.Parse("F1 "+pathData);
			Geometry geometryEvenOdd=Geometry.Parse("F0 "+pathData);
			double areaNonzero=geometryNonzero.GetArea(TOLERANCE,ToleranceType.Absolute);
			double areaEvenOdd=geometryEvenOdd.GetArea(TOLERANCE,ToleranceType.Absolute);
			if(Math.Abs(areaNonzero-areaEvenOdd)>0.01*Size) {
				throw new InvalidOperationException("The outline fills differently with the nonzero ("+areaNonzero+") and even-odd ("+areaEvenOdd+") rules.");
			}
			//The fitted outline covers the shape: what only one of them covers is a thin seam along the outline, at most FIT_ERROR wide.
			double areaXor=Geometry.Combine(geometryNonzero,pathGeometry,GeometryCombineMode.Xor,null,TOLERANCE,ToleranceType.Absolute)
				.GetArea(TOLERANCE,ToleranceType.Absolute);
			double lengthOutline=GetLength(pathGeometry);
			if(areaXor>lengthOutline*CurveFitter.FIT_ERROR*1.5+0.01) {
				throw new InvalidOperationException("The fitted outline strays from the shape: "+areaXor+" px2 along "+lengthOutline+" px. "+pathData);
			}
			Rect rectBounds=geometryNonzero.Bounds;
			if(rectBounds.Left< -0.01 || rectBounds.Top< -0.01 || rectBounds.Right>Size+0.01 || rectBounds.Bottom>Size+0.01) {
				throw new InvalidOperationException("The design leaves its square: "+rectBounds);
			}
			return pathData;
		}

		///<summary>The total length of a flattened geometry's figures.</summary>
		private static double GetLength(PathGeometry pathGeometry) {
			double length=0;
			foreach(PathFigure pathFigure in pathGeometry.Figures) {
				List<Point> listPoints=new List<Point>();
				listPoints.Add(pathFigure.StartPoint);
				foreach(PathSegment pathSegment in pathFigure.Segments) {
					AddPoints(listPoints,pathSegment);
				}
				for(int i=0;i<listPoints.Count;i++) {
					length+=(listPoints[(i+1)%listPoints.Count]-listPoints[i]).Length;
				}
			}
			return length;
		}

		///<summary>The points of a flattened figure's segment.</summary>
		private static void AddPoints(List<Point> listPoints,PathSegment pathSegment) {
			LineSegment lineSegment=pathSegment as LineSegment;
			if(lineSegment!=null) {
				listPoints.Add(lineSegment.Point);
				return;
			}
			PolyLineSegment polyLineSegment=pathSegment as PolyLineSegment;
			if(polyLineSegment!=null) {
				listPoints.AddRange(polyLineSegment.Points);
				return;
			}
			throw new InvalidOperationException("A flattened figure holds a "+pathSegment.GetType().Name);
		}

		internal static void Append(StringBuilder stringBuilder,string command,params Point[] arrayPoints) {
			stringBuilder.Append(' ').Append(command);
			foreach(Point point in arrayPoints) {
				stringBuilder.Append(' ').Append(Format(point.X)).Append(' ').Append(Format(point.Y));
			}
		}

		private static string Format(double value) {
			string text=Math.Round(value,2,MidpointRounding.AwayFromZero).ToString("0.##",CultureInfo.InvariantCulture);
			if(text=="-0") {
				text="0";
			}
			return text;
		}
		#endregion
	}

	///<summary>Fits a closed polyline with lines and cubic Bezier curves: split at the corners, straight runs as lines, the rest by least squares
	///with the end tangents kept, split where the error is largest until every point is within FIT_ERROR (P. J. Schneider, "An Algorithm for
	///Automatically Fitting Digitized Curves", Graphics Gems, 1990).</summary>
	public static class CurveFitter {
		///<summary>The largest distance of a polyline point from the fitted outline, in design pixels.</summary>
		public const double FIT_ERROR=0.02;
		///<summary>Points closer than this to the one before are dropped (seams of the combined shapes), in design pixels.</summary>
		private const double MIN_STEP=0.01;
		///<summary>A turn sharper than this, in degrees, is a corner (the round joins and ends turn far less from one polyline point to the next).</summary>
		private const double CORNER_DEGREES=35;
		///<summary>Tangents are taken towards the first point at least this far away, in design pixels, so a short seam cannot tilt them.</summary>
		private const double TANGENT_REACH=0.25;
		///<summary>A polyline segment at least this long is a straight edge of the shape, in design pixels.</summary>
		private const double LINE_MIN=1.5;
		///<summary>The spacing of the points a fit is checked on, in design pixels.</summary>
		private const double SAMPLE_STEP=0.1;

		///<summary>Appends one closed figure: M, then L and C commands, then Z.</summary>
		public static void AppendClosedFigure(StringBuilder stringBuilder,List<Point> listPointsIn) {
			List<Point> listPoints=new List<Point>();
			foreach(Point point in listPointsIn) {
				if(listPoints.Count==0 || (point-listPoints[listPoints.Count-1]).Length>=MIN_STEP) {
					listPoints.Add(point);
				}
			}
			while(listPoints.Count>1 && (listPoints[listPoints.Count-1]-listPoints[0]).Length<MIN_STEP) {
				listPoints.RemoveAt(listPoints.Count-1);
			}
			if(listPoints.Count<3) {
				return;//a sliver left by the rounding of the combined shapes: nothing to fill
			}
			int count=listPoints.Count;
			//The breaks: the corners, and both ends of every long polyline segment, which is a straight edge of the shape (a curve's flattening
			//makes much shorter chords).
			SortedSet<int> sortedSetBreaks=new SortedSet<int>();
			for(int i=0;i<count;i++) {
				Vector vectorIn=listPoints[i]-listPoints[(i-1+count)%count];
				Vector vectorOut=listPoints[(i+1)%count]-listPoints[i];
				if(Math.Abs(Vector.AngleBetween(vectorIn,vectorOut))>CORNER_DEGREES) {
					sortedSetBreaks.Add(i);
				}
				if(vectorOut.Length>=LINE_MIN) {
					sortedSetBreaks.Add(i);
					sortedSetBreaks.Add((i+1)%count);
				}
			}
			List<int> listCorners=new List<int>(sortedSetBreaks);
			bool isSmooth=(listCorners.Count==0);
			if(isSmooth) {
				//A figure without corners (a circle, a rounded outline): split it at its first point and the point farthest from it, smoothly.
				int indexFar=0;
				double distanceFar=0;
				for(int i=1;i<count;i++) {
					double distance=(listPoints[i]-listPoints[0]).Length;
					if(distance>distanceFar) {
						distanceFar=distance;
						indexFar=i;
					}
				}
				listCorners.Add(0);
				listCorners.Add(indexFar);
			}
			DentalIconTools.IconCanvas.Append(stringBuilder,"M",listPoints[listCorners[0]]);
			for(int k=0;k<listCorners.Count;k++) {
				int indexStart=listCorners[k];
				int indexEnd=listCorners[(k+1)%listCorners.Count];
				List<Point> listPiece=new List<Point>();
				int index=indexStart;
				while(true) {
					listPiece.Add(listPoints[index]);
					if(listPiece.Count>1 && index==indexEnd) {
						break;
					}
					index=(index+1)%count;
				}
				Vector tangentStart;
				Vector tangentEnd;
				if(isSmooth) {
					tangentStart=GetTangentAround(listPoints,indexStart);
					tangentEnd=-GetTangentAround(listPoints,indexEnd);
				}
				else {
					tangentStart=GetTangentForward(listPiece);
					tangentEnd=GetTangentBackward(listPiece);
				}
				AppendPiece(stringBuilder,listPiece,tangentStart,tangentEnd);
			}
			stringBuilder.Append(" Z");
		}

		///<summary>One piece between breaks: a line when it is straight, otherwise curves. The fit is checked on points at most SAMPLE_STEP apart
		///along the polyline, so a curve cannot bulge between two far polyline points.</summary>
		private static void AppendPiece(StringBuilder stringBuilder,List<Point> listPieceIn,Vector tangentStart,Vector tangentEnd) {
			List<Point> listPiece=new List<Point>();
			listPiece.Add(listPieceIn[0]);
			for(int i=1;i<listPieceIn.Count;i++) {
				Vector vector=listPieceIn[i]-listPieceIn[i-1];
				int countSteps=(int)Math.Ceiling(vector.Length/SAMPLE_STEP);
				for(int step=1;step<=countSteps;step++) {
					listPiece.Add(listPieceIn[i-1]+vector*((double)step/countSteps));
				}
			}
			Point pointFirst=listPiece[0];
			Point pointLast=listPiece[listPiece.Count-1];
			bool isStraight=true;
			for(int i=1;i<listPiece.Count-1;i++) {
				if(GetDistanceToLine(listPiece[i],pointFirst,pointLast)>FIT_ERROR/2) {
					isStraight=false;
					break;
				}
			}
			if(isStraight) {
				DentalIconTools.IconCanvas.Append(stringBuilder,"L",pointLast);
				return;
			}
			FitCubic(stringBuilder,listPiece.ToArray(),0,listPiece.Count-1,tangentStart,tangentEnd);
		}

		private static double GetDistanceToLine(Point point,Point pointA,Point pointB) {
			Vector vectorLine=pointB-pointA;
			double length=vectorLine.Length;
			if(length<1e-9) {
				return (point-pointA).Length;
			}
			return Math.Abs(Vector.CrossProduct(vectorLine,point-pointA))/length;
		}

		private static Vector GetTangentForward(List<Point> listPiece) {
			for(int i=1;i<listPiece.Count;i++) {
				if((listPiece[i]-listPiece[0]).Length>=TANGENT_REACH || i==listPiece.Count-1) {
					return Normalize(listPiece[i]-listPiece[0]);
				}
			}
			return Normalize(listPiece[listPiece.Count-1]-listPiece[0]);
		}

		private static Vector GetTangentBackward(List<Point> listPiece) {
			Point pointLast=listPiece[listPiece.Count-1];
			for(int i=listPiece.Count-2;i>=0;i--) {
				if((listPiece[i]-pointLast).Length>=TANGENT_REACH || i==0) {
					return Normalize(listPiece[i]-pointLast);
				}
			}
			return Normalize(listPiece[0]-pointLast);
		}

		///<summary>The direction of a closed figure through a point, from the points on both sides at TANGENT_REACH.</summary>
		private static Vector GetTangentAround(List<Point> listPoints,int index) {
			int count=listPoints.Count;
			Point pointBefore=listPoints[(index-1+count)%count];
			for(int i=1;i<count/2;i++) {
				pointBefore=listPoints[(index-i+count)%count];
				if((pointBefore-listPoints[index]).Length>=TANGENT_REACH) {
					break;
				}
			}
			Point pointAfter=listPoints[(index+1)%count];
			for(int i=1;i<count/2;i++) {
				pointAfter=listPoints[(index+i)%count];
				if((pointAfter-listPoints[index]).Length>=TANGENT_REACH) {
					break;
				}
			}
			return Normalize(pointAfter-pointBefore);
		}

		private static Vector Normalize(Vector vector) {
			double length=vector.Length;
			if(length<1e-12) {
				return new Vector(0,0);
			}
			return vector/length;
		}

		private static void FitCubic(StringBuilder stringBuilder,Point[] arrayPoints,int first,int last,Vector tangentStart,Vector tangentEnd) {
			int countPoints=last-first+1;
			if(countPoints==2) {
				double distance=(arrayPoints[last]-arrayPoints[first]).Length/3;
				AppendCubic(stringBuilder,arrayPoints[first],arrayPoints[first]+tangentStart*distance,arrayPoints[last]+tangentEnd*distance,arrayPoints[last]);
				return;
			}
			double[] arrayU=ChordLengthParameterize(arrayPoints,first,last);
			Point[] arrayBezier=GenerateBezier(arrayPoints,first,last,arrayU,tangentStart,tangentEnd);
			int indexSplit;
			double errorMax=ComputeMaxError(arrayPoints,first,last,arrayBezier,arrayU,out indexSplit);
			double errorSquared=FIT_ERROR*FIT_ERROR;
			if(errorMax<errorSquared) {
				AppendCubic(stringBuilder,arrayBezier[0],arrayBezier[1],arrayBezier[2],arrayBezier[3]);
				return;
			}
			if(errorMax<errorSquared*16) {
				for(int iteration=0;iteration<20;iteration++) {
					double[] arrayUPrime=Reparameterize(arrayPoints,first,last,arrayU,arrayBezier);
					arrayBezier=GenerateBezier(arrayPoints,first,last,arrayUPrime,tangentStart,tangentEnd);
					errorMax=ComputeMaxError(arrayPoints,first,last,arrayBezier,arrayUPrime,out indexSplit);
					if(errorMax<errorSquared) {
						AppendCubic(stringBuilder,arrayBezier[0],arrayBezier[1],arrayBezier[2],arrayBezier[3]);
						return;
					}
					arrayU=arrayUPrime;
				}
			}
			if(indexSplit<=first) {
				indexSplit=first+1;
			}
			if(indexSplit>=last) {
				indexSplit=last-1;
			}
			Vector tangentCenter=Normalize(arrayPoints[indexSplit-1]-arrayPoints[indexSplit+1]);
			FitCubic(stringBuilder,arrayPoints,first,indexSplit,tangentStart,tangentCenter);
			FitCubic(stringBuilder,arrayPoints,indexSplit,last,-tangentCenter,tangentEnd);
		}

		///<summary>A cubic curve, or a line when its control points lie on the chord.</summary>
		private static void AppendCubic(StringBuilder stringBuilder,Point point0,Point point1,Point point2,Point point3) {
			if(GetDistanceToLine(point1,point0,point3)<FIT_ERROR/4 && GetDistanceToLine(point2,point0,point3)<FIT_ERROR/4) {
				Vector vectorChord=point3-point0;
				double lengthSquared=vectorChord.LengthSquared;
				double along1=lengthSquared<1e-12 ? 0 : Vector.Multiply(point1-point0,vectorChord)/lengthSquared;
				double along2=lengthSquared<1e-12 ? 0 : Vector.Multiply(point2-point0,vectorChord)/lengthSquared;
				if(along1>=0 && along1<=1 && along2>=0 && along2<=1) {
					DentalIconTools.IconCanvas.Append(stringBuilder,"L",point3);
					return;
				}
			}
			DentalIconTools.IconCanvas.Append(stringBuilder,"C",point1,point2,point3);
		}

		private static double[] ChordLengthParameterize(Point[] arrayPoints,int first,int last) {
			double[] arrayU=new double[last-first+1];
			arrayU[0]=0;
			for(int i=first+1;i<=last;i++) {
				arrayU[i-first]=arrayU[i-first-1]+(arrayPoints[i]-arrayPoints[i-1]).Length;
			}
			double total=arrayU[last-first];
			for(int i=1;i<=last-first;i++) {
				arrayU[i]=arrayU[i]/total;
			}
			return arrayU;
		}

		private static Point[] GenerateBezier(Point[] arrayPoints,int first,int last,double[] arrayU,Vector tangentStart,Vector tangentEnd) {
			int countPoints=last-first+1;
			double c00=0,c01=0,c11=0,x0=0,x1=0;
			Point pointFirst=arrayPoints[first];
			Point pointLast=arrayPoints[last];
			for(int i=0;i<countPoints;i++) {
				double u=arrayU[i];
				double b0=(1-u)*(1-u)*(1-u);
				double b1=3*u*(1-u)*(1-u);
				double b2=3*u*u*(1-u);
				double b3=u*u*u;
				Vector a0=tangentStart*b1;
				Vector a1=tangentEnd*b2;
				c00+=Vector.Multiply(a0,a0);
				c01+=Vector.Multiply(a0,a1);
				c11+=Vector.Multiply(a1,a1);
				Vector vectorTmp=arrayPoints[first+i]-new Point(pointFirst.X*(b0+b1)+pointLast.X*(b2+b3),pointFirst.Y*(b0+b1)+pointLast.Y*(b2+b3));
				x0+=Vector.Multiply(a0,vectorTmp);
				x1+=Vector.Multiply(a1,vectorTmp);
			}
			double det=c00*c11-c01*c01;
			double alphaStart=0;
			double alphaEnd=0;
			if(Math.Abs(det)>1e-12) {
				alphaStart=(x0*c11-c01*x1)/det;
				alphaEnd=(c00*x1-c01*x0)/det;
			}
			double lengthSegment=(pointLast-pointFirst).Length;
			double epsilon=1e-6*lengthSegment;
			if(alphaStart<epsilon || alphaEnd<epsilon) {
				double distance=lengthSegment/3;
				alphaStart=distance;
				alphaEnd=distance;
			}
			return new Point[] { pointFirst,pointFirst+tangentStart*alphaStart,pointLast+tangentEnd*alphaEnd,pointLast };
		}

		private static Point Evaluate(Point[] arrayBezier,double u) {
			double b0=(1-u)*(1-u)*(1-u);
			double b1=3*u*(1-u)*(1-u);
			double b2=3*u*u*(1-u);
			double b3=u*u*u;
			return new Point(arrayBezier[0].X*b0+arrayBezier[1].X*b1+arrayBezier[2].X*b2+arrayBezier[3].X*b3,
				arrayBezier[0].Y*b0+arrayBezier[1].Y*b1+arrayBezier[2].Y*b2+arrayBezier[3].Y*b3);
		}

		private static double ComputeMaxError(Point[] arrayPoints,int first,int last,Point[] arrayBezier,double[] arrayU,out int indexSplit) {
			indexSplit=(last-first+1)/2+first;
			double errorMax=0;
			for(int i=first+1;i<last;i++) {
				double distanceSquared=(Evaluate(arrayBezier,arrayU[i-first])-arrayPoints[i]).LengthSquared;
				if(distanceSquared>=errorMax) {
					errorMax=distanceSquared;
					indexSplit=i;
				}
			}
			return errorMax;
		}

		private static double[] Reparameterize(Point[] arrayPoints,int first,int last,double[] arrayU,Point[] arrayBezier) {
			double[] arrayUPrime=new double[last-first+1];
			for(int i=first;i<=last;i++) {
				arrayUPrime[i-first]=NewtonRaphson(arrayBezier,arrayPoints[i],arrayU[i-first]);
			}
			return arrayUPrime;
		}

		private static double NewtonRaphson(Point[] q,Point p,double u) {
			Point q_u=Evaluate(q,u);
			Point[] q1=new Point[3];
			for(int i=0;i<3;i++) {
				q1[i]=new Point((q[i+1].X-q[i].X)*3,(q[i+1].Y-q[i].Y)*3);
			}
			Point[] q2=new Point[2];
			for(int i=0;i<2;i++) {
				q2[i]=new Point((q1[i+1].X-q1[i].X)*2,(q1[i+1].Y-q1[i].Y)*2);
			}
			Point q1_u=new Point(q1[0].X*(1-u)*(1-u)+q1[1].X*2*u*(1-u)+q1[2].X*u*u,q1[0].Y*(1-u)*(1-u)+q1[1].Y*2*u*(1-u)+q1[2].Y*u*u);
			Point q2_u=new Point(q2[0].X*(1-u)+q2[1].X*u,q2[0].Y*(1-u)+q2[1].Y*u);
			double numerator=(q_u.X-p.X)*q1_u.X+(q_u.Y-p.Y)*q1_u.Y;
			double denominator=q1_u.X*q1_u.X+q1_u.Y*q1_u.Y+(q_u.X-p.X)*q2_u.X+(q_u.Y-p.Y)*q2_u.Y;
			if(Math.Abs(denominator)<1e-12) {
				return u;
			}
			double uNew=u-numerator/denominator;
			return Math.Min(1,Math.Max(0,uNew));
		}
	}
}
