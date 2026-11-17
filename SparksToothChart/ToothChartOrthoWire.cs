using System;
using System.Drawing;

namespace SparksToothChart {
	///<summary>What one orthodontic line of the chart is (TC-6, ORTHO-2): the painter's three kinds (OrthoHardwares.EnumOrthoWireType).</summary>
	public enum ToothChartOrthoWireKind {
		///<summary>A wire across one bracket, through its slot (ToothIDend unused).</summary>
		InBracket,
		///<summary>A wire from one bracket to the next (ToothIDstart to ToothIDend).</summary>
		BetweenBrackets,
		///<summary>An elastic from one bracket's centre to another's, in the same arch or across the arches.</summary>
		Elastic,
	}

	///<summary>One wire segment or elastic of the chart (TC-6, ORTHO-2), as the painter sends them (ToothChartPainter.PaintOrthoHardware): its kind,
	///its teeth and its colour (the hardware spec's colour). Only the WPF chart draws it, as a thin line in the 2D layer.</summary>
	public class ToothChartOrthoWire {
		public ToothChartOrthoWireKind Kind;
		public string ToothIDstart;
		///<summary>Empty for InBracket.</summary>
		public string ToothIDend;
		public Color ColorDraw;

		public ToothChartOrthoWire(ToothChartOrthoWireKind kind,string toothIDstart,string toothIDend,Color colorDraw) {
			Kind=kind;
			ToothIDstart=toothIDstart??"";
			ToothIDend=toothIDend??"";
			ColorDraw=colorDraw;
		}

		public ToothChartOrthoWire Copy() {
			return (ToothChartOrthoWire)MemberwiseClone();
		}

		public override string ToString() {
			return Kind+" "+ToothIDstart+(ToothIDend==""?"":"-"+ToothIDend);
		}
	}
}
