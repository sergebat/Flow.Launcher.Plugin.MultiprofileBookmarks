using System;

namespace Flow.Launcher.Plugin.MultiprofileBookmarks.Models
{
    public record Bookmark
    {
        public string Name { get; init; }
        public string Url { get; init; }
        public string Source { get; init; }
        public string Path { get; init; }
    }
}
