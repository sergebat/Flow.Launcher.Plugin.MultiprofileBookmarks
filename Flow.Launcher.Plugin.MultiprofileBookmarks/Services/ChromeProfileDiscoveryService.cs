using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Flow.Launcher.Plugin.MultiprofileBookmarks.Models;

namespace Flow.Launcher.Plugin.MultiprofileBookmarks.Services
{
    public class ChromeProfileDiscoveryService : IProfileDiscoveryService
    {
        public List<BrowserProfileInfo> Discover(string localApplicationDataFolderPath)
        {
            var profiles = new List<BrowserProfileInfo>();

            try
            {
                var userDataPath = Path.Combine(localApplicationDataFolderPath, @"Google\Chrome\User Data");
                if (!Directory.Exists(userDataPath))
                {
                    return profiles;
                }

                var displayNames = LoadDisplayNamesFromLocalState(userDataPath);
                foreach (var profileDirectory in GetCandidateDirectories(userDataPath))
                {
                    var bookmarksPath = Path.Combine(userDataPath, profileDirectory, "Bookmarks");
                    if (!File.Exists(bookmarksPath))
                    {
                        continue;
                    }

                    var displayName = displayNames.TryGetValue(profileDirectory, out var name)
                        ? name
                        : profileDirectory;

                    profiles.Add(new BrowserProfileInfo
                    {
                        DirectoryName = profileDirectory,
                        DisplayName = displayName
                    });
                }
            }
            catch (Exception)
            {
                // Silent failure by design.
            }

            return profiles;
        }

        private static IEnumerable<string> GetCandidateDirectories(string userDataPath)
        {
            try
            {
                return Directory.EnumerateDirectories(userDataPath)
                    .Select(Path.GetFileName)
                    .Where(name => !string.IsNullOrEmpty(name))
                    .Where(name => name == "Default" || name.StartsWith("Profile ", StringComparison.Ordinal))
                    .Cast<string>()
                    .OrderBy(name => name == "Default" ? 0 : 1)
                    .ThenBy(name => name, StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }
            catch (Exception)
            {
                return Array.Empty<string>();
            }
        }

        private static Dictionary<string, string> LoadDisplayNamesFromLocalState(string userDataPath)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                var localStatePath = Path.Combine(userDataPath, "Local State");
                if (!File.Exists(localStatePath))
                {
                    return result;
                }

                var jsonString = File.ReadAllText(localStatePath);
                using var doc = JsonDocument.Parse(jsonString);
                var root = doc.RootElement;

                if (!root.TryGetProperty("profile", out var profileNode))
                {
                    return result;
                }

                if (!profileNode.TryGetProperty("info_cache", out var infoCacheNode))
                {
                    return result;
                }

                if (infoCacheNode.ValueKind != JsonValueKind.Object)
                {
                    return result;
                }

                foreach (var property in infoCacheNode.EnumerateObject())
                {
                    if (property.Value.ValueKind != JsonValueKind.Object)
                    {
                        continue;
                    }

                    if (!property.Value.TryGetProperty("name", out var nameNode))
                    {
                        continue;
                    }

                    var name = nameNode.GetString();
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        result[property.Name] = name;
                    }
                }
            }
            catch (Exception)
            {
                // Silent failure by design.
            }

            return result;
        }
    }
}
