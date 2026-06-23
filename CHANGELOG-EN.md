# Changelog

## Version 1.3.0 · 2026-06-23

### New features

- **Server Tree View**: Added a new interactive "Server Files" tab displaying the entire remote layout under a convenient virtual root folder (supporting multiple selection with Ctrl/Shift).
- **Visual Destination Selection**: Upload folder path selection by clicking directly in the remote tree view, with a quick button to reset back to the root.
- **Batch Recursive Remote Download**: Context menu option (right-click) to download multiple files and folders from the server simultaneously (multiple selection), with local structure rebuilding, transfer speed (in Mo/s or MB/s), and estimated time remaining (ETA).
- **Remote File Management (multiple selection right-click)**:
  - *Move*: Move multiple files or folders simultaneously to a target directory via `FolderPickerDialog`.
  - *Flatten*: Flatten multiple remote folders at once (moving all their contents up one level to their respective parents).
  - *Delete*: Recursive deletion of multiple selected files or folders with global item counting and a single confirmation.
  - *Rename*: Rename remote files on the fly (limited to single selection).
- **File Metadata Suffix**: Displays file size (dynamically formatted) and upload date next to the remote filename (e.g. `(4.5 KB — Uploaded on...)`) in a lighter and softer visual style (slate gray when unselected, light blue-gray when selected) to avoid clutter.
- **Automatic Language Detection**: Detects OS culture settings on the very first start and defaults to English or French.
- **Single-use Startup Warning**: The file size warning dialog only shows up once and is stored for future launches.

### Improvements

- **Ordered Sequential Remote Operations**: Batch operations on the server (delete, move, flatten) run sequentially using `SemaphoreSlim(1)` to prevent concurrent transaction lock conflicts on the Dataverse database.
- **Right-click Multi-selection Preservation**: Right-clicking an item that is already part of a multi-selection (Ctrl/Shift) now preserves the entire selection instead of collapsing it to a single item, allowing smooth batch actions.
- **Real-time Duplicate Scanning**: Analyzes duplicates on the server reactively with color codes (Green for exact duplicates to skip, Chocolate for files existing in other directories with path details).
- **Smart Timer**: Freezes elapsed time instantly upon completion or failure of transfers, preventing the clock from ticking while reading the final notification message.
- **Cancellation & Safety**: Full CANCEL button support for downloads, with partial file cleanup to prevent corrupt data.
- **Full Reset**: Resets progress bar, transfer statistics, elapsed time, ETA, and the dynamic speed label upon clicking "Reset" (retaining API Key and DOI).
- **Smart Auto-refresh**: Automated background synchronisation of remote files after uploads, with expanded/collapsed folder state preservation.
- **Global Alignment**: Synchronised C# GUI executable and Java engine version (`DVUploader-v1.3.0-RDGengine.jar`).

### Fixes

- **Moving to Root Folder (/)**: Workaround for the Dataverse API ignoring empty values for `directoryLabel` which prevented files from being moved back to the root (utilizes a `"/"` payload that is sanitized by the server).
- **Thread Safety**: Connection parameters are captured on the UI thread before kicking off background tasks, resolving cross-thread `InvalidOperationException` crashes.
- UI improvements and general stability fixes (logs auto-scroll, resolved JSON deserialization checksum errors from the Dataverse API, fixed node selection when clicking on the right empty space of a label).

---

## Version 1.2.0 · 2026-04-15

### New features

- Added a language selector with English as the default language and French available as an option.
- Translated the main interface, statistics, dialogs, contextual help, and the About window.
- Added automatic DOI normalization to the `doi:...` format from a `https://doi.org/...` URL.

### Improvements

- Removed the automatic paste behavior from the API Key and DOI fields.
- Added information buttons to explain the API Key and DOI more clearly.
- Updated the project presentation in the README.

## Version 1.0.1 · 2025-06-18

### New features

- In the event of an error, the software retries the upload.

## Version 1.0.0 · 2025-06-17

### New features

- Added the ability to flatten a folder, meaning all its contents are moved up one level to the folder's parent, and the original folder is removed.

### Fixes

- Fixed an issue where UTF-8 encoding added unwanted characters in front of filenames during upload.

## Version 0.9.0 · 2025-06-11

- Initial stable release.
