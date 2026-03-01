using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.IO;
using Flow.Launcher.Plugin.MultiprofileBookmarks.Models;
using Flow.Launcher.Plugin.MultiprofileBookmarks.Services;
using Flow.Launcher.Plugin.SharedModels;

namespace Flow.Launcher.Plugin.MultiprofileBookmarks
{
    public class MultiprofileBookmarks : IPlugin, IReloadable
    {
        private const string GenericIconPath = "Images\\icon.png";
        private static readonly string[] ProfileIconPaths =
        {
            "Images\\bookmarks\\bookmark-0.png",
            "Images\\bookmarks\\bookmark-1.png",
            "Images\\bookmarks\\bookmark-2.png"
        };

        private PluginInitContext _context;
        private sealed class BookmarkEntry
        {
            public ChromeBookmarkSource Source { get; init; }
            public Bookmark Bookmark { get; init; }
            public string IconPath { get; init; }
        }

        private readonly IProfileDiscoveryService _profileDiscoveryService;
        private readonly IProfileIconResolver _profileIconResolver;
        private readonly Func<string> _localAppDataPathProvider;

        private readonly List<ChromeBookmarkSource> _sources = new List<ChromeBookmarkSource>();
        private List<BookmarkEntry> _cache = new List<BookmarkEntry>();

        public MultiprofileBookmarks()
            : this(
                new ChromeProfileDiscoveryService(),
                new CyclingProfileIconResolver(ProfileIconPaths),
                () => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData))
        {
        }

        internal MultiprofileBookmarks(
            IProfileDiscoveryService profileDiscoveryService,
            IProfileIconResolver profileIconResolver,
            Func<string> localAppDataPathProvider)
        {
            _profileDiscoveryService = profileDiscoveryService ?? throw new ArgumentNullException(nameof(profileDiscoveryService));
            _profileIconResolver = profileIconResolver ?? throw new ArgumentNullException(nameof(profileIconResolver));
            _localAppDataPathProvider = localAppDataPathProvider ?? throw new ArgumentNullException(nameof(localAppDataPathProvider));
        }

        public void Init(PluginInitContext context)
        {
            _context = context;

            ReloadData();
        }

        public void ReloadData()
        {
            var localAppData = _localAppDataPathProvider();

            // Initialize sources
            _sources.Clear();
            var discoveredProfiles = _profileDiscoveryService.Discover(localAppData);
            if (discoveredProfiles.Count == 0)
            {
                _sources.Add(new ChromeBookmarkSource(localAppData, "Default", "Default"));
            }
            else
            {
                foreach (var profile in discoveredProfiles)
                {
                    _sources.Add(new ChromeBookmarkSource(localAppData, profile.DirectoryName, profile.DisplayName));
                }
            }

            var profileDirectoriesInDiscoveryOrder = _sources
                .Select(source => source.ProfileDirectory)
                .ToList();

            var iconPathByProfileDirectory = _profileIconResolver
                .ResolveIconPathsByProfileDirectory(profileDirectoriesInDiscoveryOrder);
            
            _cache = _sources
                .SelectMany(source => source.GetBookmarks()
                    .Select(b => new BookmarkEntry
                    {
                        Source = source,
                        Bookmark = b,
                        IconPath = iconPathByProfileDirectory.TryGetValue(source.ProfileDirectory, out var iconPath)
                            ? iconPath
                            : GenericIconPath
                    }))
                .ToList();
        }

        public List<Result> Query(Query query)
        {
            var search = query.Search;

            if (_cache.Count == 0)
            {
                return new List<Result>();
            }

            if (string.IsNullOrWhiteSpace(search))
            {
                return _cache
                    .Select(entry => createResultFromBookmark(entry))
                    .ToList();
            }

            return _cache
                .Select(entry => new
                {
                    Entry = entry,
                    Match = _context.API.FuzzySearch(search, GetBookmarkTitle(entry))
                })
                .Where(x => x.Match.Score > 0)
                .Select(x => createResultFromBookmark(x.Entry, x.Match))
                .ToList();
        }

        private static string GetBookmarkTitle(BookmarkEntry entry)
        {
            return $"{entry.Bookmark.Name} [{entry.Source.ProfileName}]";
        }

        Result createResultFromBookmark(BookmarkEntry entry, MatchResult matchResult = null)
        {
            var source = entry.Source;
            var b = entry.Bookmark;
            return new Result
            {
                Title = GetBookmarkTitle(entry),
                SubTitle = b.Url,
                IcoPath = string.IsNullOrWhiteSpace(entry.IconPath) ? GenericIconPath : entry.IconPath,
                Score = matchResult != null ? matchResult.Score : 5,
                TitleHighlightData = matchResult != null ? matchResult.MatchData : null,
                Action = _ =>
                {
                    return TryLaunchChromeProfile(b.Url, source.ProfileDirectory);
                }
            };
        }

        private static string GetChromeExecutablePathOrFallback()
        {
            var paths = new[]
            {
                Environment.SpecialFolder.LocalApplicationData,
                Environment.SpecialFolder.ProgramFiles,
                Environment.SpecialFolder.ProgramFilesX86
            };

            return paths
                .Select(folder => Path.Combine(Environment.GetFolderPath(folder), @"Google\Chrome\Application\chrome.exe"))
                .FirstOrDefault(File.Exists)
                ?? "chrome.exe";
        }

        private static bool TryLaunchChromeProfile(string url, string profileDirectory)
        {
            try
            {
                var chromePath = GetChromeExecutablePathOrFallback();
                var startInfo = new ProcessStartInfo
                {
                    FileName = chromePath,
                    UseShellExecute = false,
                };
                startInfo.ArgumentList.Add($"--profile-directory={profileDirectory}");
                startInfo.ArgumentList.Add(url);

                Process.Start(startInfo);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
