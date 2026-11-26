// This file is part of the open source dental practice management software derived from Open Dental.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;

namespace ImagingCapture.Camera {
	///<summary>Design 8.6: which program action a camera button asks the camera window for. A program button (NextSlot, PreviousSlot, RetakeLast,
	///Freeze, Record, PauseRecording, PickTeeth, EndSession) acts once per press, on the level becoming active; the window runs the same command
	///its key runs, so a button does exactly what its key does. TakePhoto is not here: it comes as TriggerEngine.Pressed, through the capture
	///lockout. Setting actions are the session's (CameraSettingActions). A window ignores the commands it does not have.</summary>
	public static class CameraButtonAction {
		///<summary>The program action to run for this level change, or null: a release, a held level, a setting action or TakePhoto.</summary>
		public static LevelAction? GetCommand(LevelChangedEventArgs levelChangedEventArgs) {
			if(levelChangedEventArgs==null || !levelChangedEventArgs.IsActive) {
				return null;
			}
			LevelAction action=levelChangedEventArgs.Action;
			if(!LevelActions.IsProgramAction(action) || action==LevelAction.TakePhoto) {
				return null;
			}
			return action;
		}
	}
}
