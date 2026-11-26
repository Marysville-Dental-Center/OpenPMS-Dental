// This file is part of the open source dental practice management software derived from Open Dental.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System.Windows.Media;
using System.Windows.Media.Imaging;
using OpenPmsDental;

namespace ImagingCapture.UI {
	///<summary>The X-ray session's preview in a window of its own (owner, 2026-11-26): a double-click on the session's preview opens it. It is an
	///ordinary window with minimise, maximise and close, so it can be dragged to another monitor (the one facing the patient) and maximised
	///there. It shows the image the preview shows, with the same rotation and flip, and follows it as each new image arrives. It only shows:
	///no buttons, nothing to edit. FrmCaptureSession closes it when the session window closes.</summary>
	public partial class FrmCaptureImageWindow:FrmODBase {
		public FrmCaptureImageWindow() {
			InitializeComponent();
		}

		///<summary>Shows bitmapSource (null: nothing yet) with layoutTransform (the preview's rotation and flip; null: none). caption goes in the
		///title bar after "X-ray", for example the slot.</summary>
		public void SetImage(BitmapSource bitmapSource,Transform layoutTransform,string caption) {
			imageShown.Source=bitmapSource;
			imageShown.LayoutTransform=layoutTransform??Transform.Identity;
			Text=string.IsNullOrWhiteSpace(caption) ? "X-ray" : "X-ray - "+caption;
		}
	}
}
