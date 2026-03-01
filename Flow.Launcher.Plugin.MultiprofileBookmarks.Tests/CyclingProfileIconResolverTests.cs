using Flow.Launcher.Plugin.MultiprofileBookmarks.Services;
using Xunit;
using System.Collections.Generic;
using System;

namespace Flow.Launcher.Plugin.MultiprofileBookmarks.Tests
{
    public class CyclingProfileIconResolverTests
    {
        [Fact]
        public void Constructor_WhenIconListEmpty_ShouldThrow()
        {
            Assert.Throws<ArgumentException>(() => new CyclingProfileIconResolver(new string[0]));
        }

        [Fact]
        public void ResolveIconPathsByProfileDirectory_AssignsDiscoveryOrderAndCycles()
        {
            var resolver = new CyclingProfileIconResolver(
                new[] { "Images\\bookmarks\\bookmark-0.png", "Images\\bookmarks\\bookmark-1.png", "Images\\bookmarks\\bookmark-2.png" });

            var profileDirectories = new List<string> { "Default", "Profile 1", "Profile 2", "Profile 3" };

            var iconPaths = resolver.ResolveIconPathsByProfileDirectory(profileDirectories);
            var expectedPaths = new Dictionary<string, string>
            {
                ["Default"] = "Images\\bookmarks\\bookmark-0.png",
                ["Profile 1"] = "Images\\bookmarks\\bookmark-1.png",
                ["Profile 2"] = "Images\\bookmarks\\bookmark-2.png",
                ["Profile 3"] = "Images\\bookmarks\\bookmark-0.png"
            };

            Assert.Equal(expectedPaths, iconPaths);
        }

        [Fact]
        public void ResolveIconPathsByProfileDirectory_WhenProfileDirectoryBlank_ShouldSkipEntry()
        {
            var resolver = new CyclingProfileIconResolver(
                new[] { "Images\\bookmarks\\bookmark-0.png", "Images\\bookmarks\\bookmark-1.png", "Images\\bookmarks\\bookmark-2.png" });

            var profileDirectories = new List<string> { "Default", "", "Profile 2" };

            var iconPaths = resolver.ResolveIconPathsByProfileDirectory(profileDirectories);
            var expectedPaths = new Dictionary<string, string>
            {
                ["Default"] = "Images\\bookmarks\\bookmark-0.png",
                ["Profile 2"] = "Images\\bookmarks\\bookmark-2.png"
            };

            Assert.Equal(expectedPaths, iconPaths);
        }
    }
}
