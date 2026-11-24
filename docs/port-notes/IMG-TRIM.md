# Port note: IMG-TRIM (export writes a video's trim)

Why: the owner's imaging design, section 4 ("Export writes the trim range"). The owner approved it as a later package on 2026-11-23, and the
coordinator scheduled it the same day. Until now, Export wrote the whole file of a trimmed video.

## What changes

1. **`ImagingCapture/Video/VideoTrimmer.cs`** (new), `WriteTrimmed(pathSource,pathOut,trimStartMs,trimEndMs,out durationOutMs)`. It
   writes the part of the video between the trim points as a new MP4:
   - Windows' Media Foundation source reader decodes from the frame shown at the trim start up to the trim end. A trim end of 0 means the
     end of the video.
   - Mp4Writer, the recorder's H.264 writer, encodes those frames again. The cut is exact to the frame and needs no key frame at the start.
   - The result starts at 0, and its last frame lasts up to the trim end.
   - There is no sound, as the recordings have none (the owner, 2026-11-23: none at all).
   - The file is written as `<name>.partial` and renamed when complete, so a failure leaves no partial file and keeps a file that was
     there.
   - A start at or past the end counts as 0, as the player does. It throws when the video has no frame in the trim, or runs past 50,000
     frames (about 28 minutes at 30 fps) without reaching the trim end, rather than writing a shortened file.
   - It can be cancelled between frames; a cancel leaves no partial file.
   - VideoFrameReader's Geometry and ReadGeometry are now internal and shared, for the display aperture of 1080p (1088-row) frames.
2. **Export** (`ControlImageDisplay.ExportFile`, used by the single document and the mount item branches of Export):
   - A video with a trim (documentadjust TrimStartMs or TrimEndMs set) is written by VideoTrimmer on a background thread, under the
     program's progress window (ProgressWin), which can cancel it. The save dialog offers the name with `.mp4`.
   - Any other file is exported as before (ImageStore.Export). Images are unchanged.
   - The stored file is never cut.
   - The security log entry adds " (trimmed 0:05 to 0:42, 0:37 long)".
   - The trimmed video is an MP4 whatever name is chosen.
3. No schema change, no new permission (Export still needs ImageExport), and no designer file.

## Tests

- `VideoRecordingTests.Trimmer_WritesOnlyTheTrim_FromTheFrameShownAtTheStart_AndLeavesNoPartialFile` works on a real MP4 written through
  Windows' encoder: 3 s, with each second its own colour.
  - A trim from 1.05 s to 2 s gives a 0.95 s file. Its first frame is the one shown at 1.05 s, and the frame that starts at 2 s (the third second) is left out.
  - No end means up to the end of the video.
  - A start past the end writes the whole video.
  - A target in use, and a cancel, throw, keep the earlier file and leave no `.partial`.
  - The test is inconclusive without an H.264 encoder (Windows N), like the other real-MP4 tests.

## Review

One review was done. Its findings are fixed:

- the encode moved off the UI thread, under ProgressWin with cancel;
- running out of frames now throws instead of writing a shortened file;
- the save dialog offers `.mp4`;
- a start past the end counts as 0, as in the player;
- 1 ms of rounding is allowed at the end;
- the test now covers the target in use and a cancel.

Targeted x64 run, after one full x64 build of the worktree:

| Suite | Passed | Total |
|---|---|---|
| ImagingCaptureTests | 847 | 847 |
| ImagingTests | 175 | 176 |
| UiRefreshTests | 281 (49 skipped) | 330 |
| WindowChecksTests | 30 (44 skipped) | 74 |
| NoHqTests | 127 | 127 |

The one ImagingTests failure is the filter timing test, ASensorSizedPicture_TakesUnderTwoSecondsWithEveryFilter. It ran at 5 s while other
sessions were building, and passes on its own. IMG-TRIM does not touch the filters.
