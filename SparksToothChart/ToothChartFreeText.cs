using System;
using System.Drawing;
using CodeBase;

namespace SparksToothChart {
	///<summary>One free-text label of the chart (toothinitial rows of type Text, package TC-5): its text, its place and its colour. Location is
	///the top left of the label's text cell in the chart's 410x307 drawing space (ToothChartData.SizeOriginalDrawing), the space of the drawings,
	///and is scaled to the picture as they are. ToothInitialNum names the row, so a move (CursorTool.MoveText, TextMoved) can be saved.</summary>
	public class ToothChartFreeText {
		public string Text;
		public PointF Location;
		public Color ColorText;
		public long ToothInitialNum;

		public ToothChartFreeText(string text,PointF location,Color colorText,long toothInitialNum) {
			Text=text??"";
			Location=location;
			ColorText=colorText;
			ToothInitialNum=toothInitialNum;
		}

		public ToothChartFreeText Copy() {
			return (ToothChartFreeText)MemberwiseClone();
		}

		public override string ToString() {
			return ToothInitialNum+": \""+Text+"\" at "+Location.X.ToStringNetFx()+","+Location.Y.ToStringNetFx();
		}
	}
}
