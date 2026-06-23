# Changelog

## Version 2.0.0 · 2026-06-23

### Major Upload Engine Refactoring

This version marks a major architectural refactoring of **RDG ArboDV**, eliminating all external dependencies on disk by embedding its upload engine directly inside the binary.

- **Embedded Java Engine (JAR Resource)**: The Java engine `DVUploader-v1.3.0-RDGengine.jar` is now directly included as an embedded resource inside the C# executable. It is automatically extracted to disk at runtime (in the application's base directory or in `%LocalAppData%\RDG_ArboDV` if write permissions are restricted), making the C# executable 100% self-contained and ready to run from a single `.exe` file.
- **Embedded Application Logo**: The custom application icon (`Logo_RDG_ArboDV.ico`) is now integrated directly in the executable metadata for a professional look in Windows Explorer.
- **Unified Java Engine Linkage**: Fully integrated with the customized `DVUploader` Java engine to:
  - Support the `-manifest` parameter and virtual tree processing in memory for asynchronous local uploads.
  - Standardize progress stdout with real line breaks (`
`) for real-time capture and representation in the GUI.

### New Graphical and Remote Features

- **Server Tree View**: Interactive "Server Files" tab to browse remote directories in real time (supporting multiple selection with Ctrl/Shift).
- **Visual Destination Selection**: Easily select the target upload directory by clicking directly inside the server tree view.
- **Batch Recursive Remote Download**: Asynchronous download of entire remote files and folders, with local structure rebuilding, transfer speed (MB/s), and estimated time remaining (ETA).
- **Remote File Management (Right-click context menu)**:
  - *Move*: Graphically move remote files and folders (including on-the-fly directory creation).
  - *Flatten*: Batch flatten remote directories.
  - *Delete*: Sequential, recursive cleaning of files and folders on the server.
  - *Rename*: Rename files on the fly on the server.
- **Remote Metadata Enhancements**: Displays file size and upload date next to the remote filename (e.g. `(4.5 KB — Uploaded on...)`) in a softer, clutter-free visual style.

### Architectural Improvements

- **Ordered Sequential execution**: Groups batch operations on the server sequentially using `SemaphoreSlim(1)` to respect the transaction locks of the Dataverse API database.
- **Anti-Ban IP Regulation (Rate Limiting)**: Automatic 350ms delay between batch network requests to prevent IP banning.
- **Real-Time Duplicate Checking**: Instant visual color cues on local files (Green for exact duplicates to skip, Chocolate for files existing in other folders).
- **Lock Interception (Dataset Lock)**: Detects when the server locks the dataset, displaying a clear warning and turning the progress bar to marquee mode to prevent user confusion.
- **Host Language Auto-detection**: Automatically configures the UI in French or English based on the OS culture settings.
- **Session Management**: Single-use startup warning and automatic configuration persistence.
- **Global Reset**: Complete, thread-safe reset of the progress bar, transfer statistics, elapsed time, ETA, and dynamic speed label upon clicking "Reset".

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
