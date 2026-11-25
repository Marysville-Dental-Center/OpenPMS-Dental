# Port note: IMG-SRCSET (the sensor driver's settings in Capture settings)

Why: the owner (2026-11-24) asked for "a UI for KPTwain.ini" in driver build 1.4, together with headless operation. The driver's
installation settings lived in an INI file under the Windows folder of each computer: a hand edit there needed administrator rights, was
per computer, and was not logged. With twain-contract 1.4 the driver takes them from the host through a settings capability, so they are
set in Capture settings like every other gated capture setting.

The owner then chose (card answer "Driver describes them"): Capture settings is generic, so it must not carry one driver's list of
settings. The driver describes its own settings (names, types, allowed values, help) through a second capability, and the rows are built
from that description. No driver's setting names are in this program's code.

## What changes

1. **capture.ini keys** (`[Device.N]`):
   - `SettingsCap`: the host-settings capability number from the source's published contract (hex). Gated.
   - `SettingsSchemaCap`: the settings-description capability number (hex). Not gated: it changes no image.
   - `Source.<Section>.<Key>`: one per setting the driver describes. An empty one is not sent, so the driver's own value applies.
2. **The description** (`TwainSessionCapture.GetSettingsSchemaCapability`, `TwainCaptureSession.ReadSettingsSchema`):
   - read at each open, MSG_GET answering a TW_ARRAY of TWTY_STR1024 chunks of one UTF-8 JSON document (at most 64 chunks);
   - carried on `TwainCaptureSourceInfo.SettingsSchemaJson` (also across the 32-bit helper's wire);
   - saved as `source-settings-<N>.json` beside capture.ini (`TwainSourceSettings.SaveSchema`), written only when it changed.
3. **Untrusted input** (`ImagingCapture/Settings/SourceSettingsSchema.cs`, the coordinator's condition): the description is taken only
   within limits, else refused as a whole with one sentence why, and no rows:
   - at most 64 K characters, 100 settings, 32 values per choice, values and defaults of 32 characters, help of 400;
   - schema name `source-settings-schema/1`; types choice, integer, number, text only;
   - keys `Section.Key`, letters and digits, each part starting with a letter, at most 32 each (no `=`, `[`, `]`, `;`, spaces,
     line breaks or NUL can reach capture.ini);
   - values and defaults: letters, digits, space and `. , _ + -`; help without control characters;
   - a key listed twice, or a min above its max, is refused.

   The session's capture log and Test capture say how many settings the driver describes, or why the description was refused.
4. **Capture settings** (`CaptureSettingsCatalog.GetSettings`): the catalog's rows, then one gated row per described setting (choice ->
   the choice list plus empty; integer -> whole number with its range; number -> Decimal with its range; text -> Text), then a row for
   each `Source.*` key in capture.ini (or in the last logged save) that the description does not list, so nothing set is hidden. The help
   says "leave it empty for diagnostic X-rays" for settings the driver marks as changing the image. Two new value kinds: Decimal (a
   number written with '.') and Text (no ';' or '='). No new window: the rows use the screen's editor, its Setup permission and its
   security-log entry.
5. **Gated, without changing existing hashes** (`GatedSettingsHash`): SettingsCap and every `Source.*` key are gated, but each is in the
   canonical text only when it has a value (SettingsCap first, then the Source.* keys in ordinal order ignoring case). A file without them
   hashes exactly as before, and no existing Capture settings save or Test capture record changes; each one set gets a hash of its own.
6. **The session** (`TwainCaptureSession.ApplySourceSettings`), once at open (state 4; owner 2026-11-24: "Once per open" instead of before
   every arming, since the values cannot change within a session and the driver keeps them until it closes):
   - sends `Section.Key=value;...` (`TwainSourceSettings.Build`: every set `Source.*` key in key order) through SettingsCap (a
     TW_ONEVALUE of TWTY_STR1024);
   - then reads back what the driver reports in effect (MSG_GET).
   - Settings that are configured but cannot be applied fail the open with the reason, and the source closes again: no SettingsCap, a
     driver before 1.4 (TWCC_CAPUNSUPPORTED), a value the driver refuses (TWCC_BADVALUE), or a value with ';' or '=' (a hand edit).
   - The answer goes to the capture log, onto every image of the session (`AcquiredImage.SourceSettingsInEffect`) and into its sidecar.
   - Not verified (CaptureSettingsCheck safe values): no SettingsCap and no settings, so the driver keeps its own.
7. **Test capture** lists SettingsCap, SettingsSchemaCap and the settings sent, the description's result, and after a test image the
   "Sensor driver settings in effect".
8. **Headless** needs no code here: `[Device.N] Headless=1` already sends ShowUI=0 when the source reports CAP_UICONTROLLABLE TRUE.
9. Docs: `docs/imaging-capture-design.md` (the [Device.N] block); `docs/imaging/intraoral-capture-setup.md` section 15.

## Tests

- `SourceSettingsSchemaTests` (new, 4): a valid description of every type; malformed (not JSON, wrong schema, no list, unknown or missing
  type, choice without values, min above max, duplicate key); oversized (document, settings, values, help, value, key); unsafe keys
  (`=`, `[`, `]`, `;`, line breaks, NUL, spaces, three parts, non-ASCII), values and defaults, help with a line break.
- `TwainCaptureSessionTests` (+8), against the simulated source, which now has both capabilities: the description read at open in chunks
  (a multi-byte character across a chunk boundary), saved, not rewritten unchanged; a 1.3 source saves nothing; the settings sent before
  the enable, read back, on the image and its sidecar; no SettingsCap, a 1.3 source, a refused value each stop the enable; nothing
  configured means nothing sent; the text from capture.ini.
- `CaptureSettingsModelTests` (+2): rows from a saved description, with their kinds, choices, ranges and help; a refused description gives
  no rows and the reason, and a key in the file stays as a row.
- `GatedSettingsHashTests` (+1): empty keys keep the hash of a file without them, each set key changes it, SettingsSchemaCap is not gated.
- `CaptureSettingsCheckTests`: the catalog's gated keys are the hashed ones, SettingsCap included.
