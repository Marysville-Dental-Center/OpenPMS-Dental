using System;

namespace SparksToothChart {
	///<summary>The engine a ToothChartWrapper draws with (ToothChartWrapper.Engine). In memory only: it is never stored, and it is outside the
	///business layer, so docs/port-notes/FORK-ENUMS.md does not apply to it. The stored DrawingMode (computerpref.GraphicsSimple) keeps its three
	///values, 0 DirectX, 1 Simple 2D and 2 OpenGL (docs/specs/wpf-tooth-chart-plan.md, section 1 item 5): a DrawMode request of 0 or 2 starts Wpf3D
	///and 1 starts Simple2D (ToothChartWrapper.DrawMode), and Wpf3D has no stored form. Which engine a computer draws with is the relay's choice
	///(ToothChartRelay.GetEngineForComputer, TC-8b): Open Dental's 3D tooth chart settings, and Simple2D when this computer does not use it. TC-9a retired the
	///DirectX 9 engine (0, through SharpDX) and the OpenGL engine (2, through Tao); the two engines left keep their numbers, so Simple2D still has
	///the value of DrawingMode.Simple2D.</summary>
	public enum ToothChartEngine {
		///<summary>1. The flat 2D chart, drawn with GDI+.</summary>
		Simple2D=1,
		///<summary>3. The WPF 3D chart (namespace SparksToothChart.Wpf3D): on screen a live WPF view (O-19 (b), TC-3), drawn by WPF's render thread
		///as every WPF window of the process is (in OpenPmsDental.exe in software at 1x, because the program sets RenderOptions.ProcessRenderMode to
		///SoftwareOnly, O-34), or with ToothChartWrapper.WpfDrawInSoftware the renderer's supersampled software image of the scene, made on the chart's
		///own render thread and shown by a plain WinForms control; every image (GetBitmap, prints, sheets, the dashboard) is that renderer's image.</summary>
		Wpf3D=3,
	}

	///<summary>The kinds of fallback message of the tooth chart. ToothChartWrapper.LogFallbackOnce writes the first message of each kind in a process
	///to the ToothChart log and keeps the rest in memory only, so a computer that has written one kind still writes the first of another: a
	///computer that once logged a render failure still logs a failure of the live view. In memory only. TC-9a removed the kind of the old engines'
	///fallback (DirectX, then OpenGL, then Simple 2D) with those engines.</summary>
	public enum ToothChartFallbackKind {
		///<summary>The WPF chart could not start and the chart uses Simple 2D.</summary>
		Wpf3DStart,
		///<summary>A WPF image could not be drawn (GetBitmap or the screen) and the Simple 2D image of the same data took its place.</summary>
		Wpf3DRender,
		///<summary>The WPF chart's render thread: its last guard caught an error, a result could not be handed on, or failed images in a row
		///replaced the thread.</summary>
		Wpf3DRenderThread,
		///<summary>The WPF chart's live view could not start on a shown wrapper, or could not show a scene, and the wrapper shows the software render on
		///screen instead.</summary>
		Wpf3DLiveView,
		///<summary>A tool's cursor (the pen's, the eraser's or the colour changer's) could not be made from its embedded file, and the chart shows the
		///arrow for that tool until a later choice of the tool makes the cursor.</summary>
		Cursor,
	}
}
