# Port note: IMG-P4d (watching a video in the Imaging module: trim, save frame as photo, the tree's picture)

Why: the owner's imaging design, camera and video (2026-11-22), section 4 "Watching a video". Until now a video document showed nothing
when selected; a double-click opened it in an outside player. IMG-P4e (camera settings) follows.

## What changes

1. **Selecting a video plays it in place of the image** (`OpenPmsDental/User Controls/ControlVideoPlayer.cs`, built in code).
   - The picture comes from the program's browser control (ODWebView2, Microsoft Edge), which plays H.264 MP4 and the other videos Edge
     plays. It reads from the A to Z folder, also on the office server's share, as the module's PDFs do. For cloud or database storage it
     plays a local copy.
   - A video opens paused at its start (or at its trim start), filling the area.
   - The bar below:
     - Play/Pause, one frame back or forward, half speed;
     - a scrub bar showing the position, the trim and its ends (click or drag to seek);
     - Set start / Set end / Clear trim;
     - Save frame as photo;
     - the time, the trim, the teeth and when it was recorded.
   - Edge's own controls, right-click menu and keys are off. Its menu offers "Save video as", which would bypass Export and its ImageExport
     permission.
   - The double-click on a video still opens it in Windows' player, as before.
2. **Trim** is stored in documentadjust (TrimStartMs/TrimEndMs, from IMG-P3a) and the file is never cut.
   - Playback starts at the trim start and stops at the trim end.
   - Changing the trim needs ImageEdit (and Signed Image Edit for a signed video) and is logged ("Video trimmed: ... plays 0:05 to 0:31").
   - A trim that would leave less than 0.2 s is refused.
3. **Save frame as photo**: the frame at the position, read at the video's full size by Media Foundation's source reader
   (`ImagingCapture/Video/VideoFrameReader.cs`).
   - It becomes a new photo, unmounted in the video's category, with the video's teeth and "From video #30 at 0:17".
   - Needs ImageCreate. The tree refreshes.
4. **The tree's picture of a video** is its first frame, letterboxed into 100 by 100, with a play mark and its length.
   - Saved as Thumbnails\[file name], like an image's, by the camera session when it records and by the player the first time a video is
     shown.
   - `Documents.GetThumbnail` returns it for a video (VideoMedia.LoadThumbnail), or "not available" until there is one. With cloud or
     database storage there are no thumbnail files, as for images.
5. **Deleting a video that is shown**: the player leaves the file first, so the delete is not refused for a file in use.
6. **Interop:** IMFSourceReader, MFCreateSourceReaderFromURL, a 64-bit PROPVARIANT, IMFAttributes.GetUINT32/GetUINT64 and
   IMFSample.ConvertToContiguousBuffer, all from the Windows SDK headers. `InteropLayoutTests` pins every new slot (21 interfaces).

7. **Review fixes:**
   - An answer from the previous page (Edge still showing the last video while the next loads) is dropped. Each answer carries the page's
     address and a show counter, so the next video always opens paused, at its trim start, with Edge's controls hidden.
   - Going from one video to the next does not pass through a blank page that could block the second navigation.
   - A delete lets go of the video only after the permission checks and the question, and only for the video shown.
   - Save frame takes the frame shown at the position (the last one that starts at or before it), and the position where the pause landed.
   - A 1080p video's frame is cropped to the picture's 1080 rows (MF_MT_MINIMUM_DISPLAY_APERTURE); the decoder makes 1088.
   - A change of the decoded size is followed.
   - A position that cannot be reached is an error, not a wrong frame.
   - Leaving the module stops the video.
   - The browser's failures never reach the crash dialog.
   - No bitmap is leaked when checking the tree picture.

## Why Edge and not WPF's MediaPlayer

The first attempt used WPF's MediaPlayer, which goes through the legacy Windows Media Player. On this Windows 11 build it refused every MP4
(0xC00D11BA, plain or fragmented), with Windows Media Player installed. Clinic PCs may differ in the same way, so playback uses the Edge
runtime the program already needs, and frames come from Media Foundation directly.

## Not in this package

**Export of a trimmed clip.** Design section 4 says "Export writes the trimmed clip". That needs a remux or re-encode of the MP4; Export
still writes the whole file. Recorded in docs/decisions/owner-questions.md.

## Tests

- `tests/ImagingTests/VideoMediaTests.cs` (5):
  - which files are videos;
  - the trim rules;
  - times and the frame description;
  - the tree picture;
  - the module wiring: player, thumbnail, delete, the permissions, Edge's menu and controls off.
- `tests/ImagingCaptureTests/VideoRecordingTests.cs` (+1): **a real MP4 read back** by VideoFrameReader at full size, the right way up (red
  top, blue bottom), at a position (green in the second second), with its length, and the last frame past the end; and a 1080p video cropped to 1080 rows, with the frame shown at 450 ms (+1).
- `tests/UiRefreshTests/VideoPlayerTests.cs` (2):
  - **the player shows a recorded MP4 in the browser control**, paused at its start, with its length (inconclusive without the WebView2
    Runtime), then a second video straight after it with its own length;
  - the poll answer is parsed.
- `InteropLayoutTests`: the new slots.
- Gallery G-IMG9: the player with a trim.
  - The audit hid Edge's own controls, which were drawn over the video and offered a download.
  - It also made the video fill the area; it had shown at its natural size.

Targeted x64 run:

| Suite | Passed | Total |
|---|---|---|
| ImagingCaptureTests | 843 | 843 |
| ImagingTests | 175 | 175 |
| WindowChecksTests | 30 (44 skipped) | 74 |
| NoHqTests | 127 | 127 |

UiRefreshTests fails the base set only. Its new video player tests pass.
