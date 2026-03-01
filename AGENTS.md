# Agent Context: Flow Launcher Multi-Profile Bookmarks

## Project Goal
A Flow Launcher plugin that indexes Chrome bookmarks across multiple specific profiles and launches them in the correct browser instance.

## Architectural Constraints & Patterns
- **Framework:** Must target `net9.0-windows`.
- **Browser Scope:** Currently limited to Chrome. Future expansion to Firefox is planned, so `IBookmarkSource` must remain generic.
- **Error Handling:** Silent failure is preferred over crashing. If something is wrong when autocompleting, navigating or reading bookmarks, log error and recover (do nothing, return empty list, etc).
- **Performance:** Bookmarks are cached in-memory. `ReloadData` must be efficient as it runs on plugin init.

## Key Implementation Details (The "Why")

## Dev Workflows
### Adding a New Profile
1. Locate the `Init` method in `Main.cs`.
2. Add a new `ChromeBookmarkSource` with the directory name and desired display name.
3. *Note: Do not modify `ChromeBookmarkSource.cs` logic when just adding a profile.*

### Adding a New Browser
1. Create a class implementing `IBookmarkSource` (e.g., `FirefoxBookmarkSource`).
2. Implement JSON parsing specific to that browser.
3. Register the new source in `Main.Init`.
