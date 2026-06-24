# Changelog

## Unreleased changes · 2026-06-24

### Fixes

- **Clearer upload completion**: the temporary dataset-busy message no longer remains visible after the transfer completes. The application now waits for all Java engine output events to finish before displaying the result
- **Protection against delayed events**: late Java output remains available in the logs but can no longer recolor or modify the interface after a completed upload or reset
- **Complete visual reset**: the Reset button also clears the tree's custom selection and correctly restores the status text and color
- **Multiple local-folder flattening**: all selected folders are now processed from the deepest level up to the root. Selecting an entire tree can therefore produce a fully flattened upload
- **Manifest aligned with the preview**: virtual paths are rebuilt from the visible tree immediately before duplicate detection and manifest creation, ensuring that the uploaded structure matches the preparation workspace

### Interface

- **Renamed tab**: the “Files” tab is now called “Upload preparation” to distinguish the local workspace from content already stored on the server
- **Clearer destination header**: the target-folder header is more compact. Long paths are shortened with an ellipsis and remain fully available in a tooltip
- **Resizable window**: the application can now be resized or maximized. Trees, logs, and primary controls adapt to the available space while preserving the original layout
- **Reworded waiting state**: a temporary dataset lock is now presented as a normal server-finalization phase that requires no user action

### Documentation

- **Restructured README**: added a complete usage flow, detailed tab descriptions, a color legend, prerequisites, official download links, architecture notes, and troubleshooting guidance
- **Documented project history**: added the history of the initial project created by LFR54 during a BTS internship, the limitations that prevented the first architecture from being deployed to production, and the later move to the DVUploader engine
- **Documented AI assistance**: transparently described the use of AI during the post-internship refactoring, particularly for the Java component and advanced integration work

## Version 2.0.0 · 2026-06-23

### Major Upload Engine Refactoring

This version marks a major architectural refactoring of **RDG ArboDV**. The upload engine JAR is now embedded directly in the binary and no longer needs to be distributed separately.

- **Embedded Java Engine (JAR Resource)**: The Java engine `DVUploader-v1.3.0-RDGengine.jar` is now included as an embedded resource inside the C# executable. It is automatically extracted at runtime, either to the application directory or to `%LocalAppData%\RDG_ArboDV` when write permissions are restricted. The application can therefore be distributed as a single `.exe` file, while still requiring the .NET 8 Desktop Runtime and Java 8 or later on the target computer.
- **Embedded Application Logo**: The custom application icon (`Logo_RDG_ArboDV.ico`) is now integrated directly in the executable metadata for a professional look in Windows Explorer.
- **Unified Java Engine Linkage**: Fully integrated with the customized `DVUploader` Java engine to:
  - Support the `-manifest` parameter and rebuild an in-memory virtual tree from the paths prepared in the interface
  - Standardize progress output with real line breaks (`\n`) for real-time capture and display in the GUI

### New Graphical and Remote Features

- **Server Tree View**: Interactive "Server Files" tab to browse remote directories in real time (supporting multiple selection with Ctrl/Shift).
- **Visual Destination Selection**: Easily select the target upload directory by clicking directly inside the server tree view.
- **Batch Recursive Remote Download**: Asynchronous download of entire remote files and folders, with local structure rebuilding, transfer speed (MB/s), and estimated time remaining (ETA).
- **Clean Download Cancellation**: Controlled cancellation of remote transfers with removal of the incomplete temporary file.
- **Remote File Management (Right-click context menu)**:
  - *Move*: Graphically move remote files and folders (including on-the-fly directory creation).
  - *Flatten*: Batch flatten remote directories.
  - *Delete*: Sequential, recursive cleaning of files and folders on the server.
  - *Rename*: Rename files on the fly on the server.
- **Remote Metadata Enhancements**: Displays file size and upload date next to the remote filename (e.g. `(4.5 KB — Uploaded on...)`) in a softer, clutter-free visual style.
- **Navigation State Preservation**: Automatically restores expanded folders and the selected path after a refresh or remote operation.
- **Reliable Multiple Selection**: Fixes ghost selection rendering and preserves grouped selections when opening the context menu.

### Architectural Improvements

- **Ordered Sequential execution**: Groups batch operations on the server sequentially using `SemaphoreSlim(1)` to respect the transaction locks of the Dataverse API database.
- **Anti-Ban IP Regulation (Rate Limiting)**: Automatic 350ms delay between batch network requests to prevent IP banning.
- **Move to Dataset Root**: Works around the Dataverse API limitation by explicitly sending `/` when the destination is the dataset root.
- **Dataverse Checksum Compatibility**: Supports checksum values returned as `{ type, value }` objects, preventing deserialization errors while loading remote files.
- **Cross-Thread Safety**: Reads configuration values from the UI thread and passes them explicitly to asynchronous tasks, preventing unauthorized WinForms control access.
- **Real-Time Duplicate Checking**: Instant visual color cues on local files (Green for exact duplicates to skip, Chocolate for files existing in other folders).
- **Lock Interception (Dataset Lock)**: Detects when the server locks the dataset, displaying a clear warning and turning the progress bar to marquee mode to prevent user confusion.
- **Host Language Auto-detection**: Automatically configures the UI in French or English based on the OS culture settings.
- **Preference Management**: Single-use startup warning and persistence of the selected language.
- **Global Reset**: Complete, thread-safe reset of the progress bar, transfer statistics, elapsed time, ETA, and dynamic speed label upon clicking "Reset".
- **Optimized Engine Extraction**: Reuses the extracted JAR when its size matches the embedded resource and falls back to `%LocalAppData%\RDG_ArboDV` when the application directory is not writable.

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
