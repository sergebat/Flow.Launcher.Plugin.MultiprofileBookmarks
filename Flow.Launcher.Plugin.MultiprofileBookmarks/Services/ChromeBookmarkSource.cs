using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Flow.Launcher.Plugin.MultiprofileBookmarks.Models;

namespace Flow.Launcher.Plugin.MultiprofileBookmarks.Services
{
    public class ChromeBookmarkSource : IBookmarkSource
    {
        private readonly string _bookmarksPath;
        public string ProfileName { get; private set; }
        public string ProfileDirectory { get; private set; }

        public ChromeBookmarkSource(string localApplicationDataFolderPath, string profileFolderName, string profileName)
        {
            _bookmarksPath = Path.Combine(localApplicationDataFolderPath, @"Google\Chrome\User Data", profileFolderName, "Bookmarks");
            ProfileName = profileName;
            ProfileDirectory = profileFolderName;
        }

        public List<Bookmark> GetBookmarks()
        {
            var bookmarks = new List<Bookmark>();

            if (!File.Exists(_bookmarksPath))
            {
                return bookmarks;
            }

            try
            {
                var jsonString = File.ReadAllText(_bookmarksPath);
                using var doc = JsonDocument.Parse(jsonString);
                var root = doc.RootElement;

                if (root.TryGetProperty("roots", out var roots))
                {
                    if (roots.TryGetProperty("bookmark_bar", out var bookmarkBar))
                    {
                        Traverse(bookmarkBar, bookmarks, "");
                    }
                    if (roots.TryGetProperty("other", out var other))
                    {
                        Traverse(other, bookmarks, "");
                    }
                    if (roots.TryGetProperty("synced", out var synced))
                    {
                        Traverse(synced, bookmarks, "");
                    }
                }
            }
            catch (Exception)
            {
                // In case of error, just return what we have or empty
            }

            return bookmarks;
        }

        private void Traverse(JsonElement element, List<Bookmark> bookmarks, string path)
        {
            if (element.ValueKind != JsonValueKind.Object) return;

            if (element.TryGetProperty("type", out var typeProp))
            {
                var type = typeProp.GetString();
                if (type == "url")
                {
                    var name = element.TryGetProperty("name", out var nameProp) ? nameProp.GetString() : "";
                    var url = element.TryGetProperty("url", out var urlProp) ? urlProp.GetString() : "";
                    
                    if (!string.IsNullOrEmpty(url))
                    {
                        bookmarks.Add(new Bookmark
                        {
                            Name = name,
                            Url = url,
                            Source = "Chrome",
                            Path = path
                        });
                    }
                }
                else if (type == "folder")
                {
                    var name = element.TryGetProperty("name", out var nameProp) ? nameProp.GetString() : "";
                    
                    // If path is empty (root level passed in), use name. detailed path construction.
                    // For roots like "bookmark_bar", they have a name "Bookmarks Bar".
                    // So first level path will be "Bookmarks Bar".
                    var newPath = string.IsNullOrEmpty(path) ? name : $"{path}/{name}";

                    if (element.TryGetProperty("children", out var children))
                    {
                        foreach (var child in children.EnumerateArray())
                        {
                            Traverse(child, bookmarks, newPath);
                        }
                    }
                }
            }
        }
    }
}
