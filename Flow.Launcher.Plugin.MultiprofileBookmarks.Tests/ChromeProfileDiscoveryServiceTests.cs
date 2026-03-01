using System;
using Flow.Launcher.Plugin.MultiprofileBookmarks.Models;
using Flow.Launcher.Plugin.MultiprofileBookmarks.Services;
using Xunit;

namespace Flow.Launcher.Plugin.MultiprofileBookmarks.Tests
{
    public class ChromeProfileDiscoveryServiceTests
    {
        [Fact]
        public void Discover_WhenChromeUserDataFolderMissing_ShouldReturnEmptyList()
        {
            var fixtureRoot = GetFixtureRoot("missing-user-data");
            var service = new ChromeProfileDiscoveryService();

            var profiles = service.Discover(fixtureRoot);

            Assert.Empty(profiles);
        }

        [Fact]
        public void Discover_WhenBookmarksExists_ShouldFindDefaultAndProfileDirectories()
        {
            var fixtureRoot = GetFixtureRoot("bookmarks-exists");
            var service = new ChromeProfileDiscoveryService();
            var profiles = service.Discover(fixtureRoot);
            var expected = new[]
            {
                new BrowserProfileInfo
                {
                    DirectoryName = "Default",
                    DisplayName = "Default"
                },
                new BrowserProfileInfo
                {
                    DirectoryName = "Profile 10",
                    DisplayName = "Profile 10"
                },
                new BrowserProfileInfo
                {
                    DirectoryName = "Profile 2",
                    DisplayName = "Profile 2"
                }
            };

            Assert.Equal(expected, profiles);
        }

        [Fact]
        public void Discover_WhenBookmarksMissing_ShouldIgnoreDirectory()
        {
            var fixtureRoot = GetFixtureRoot("bookmarks-missing");
            var service = new ChromeProfileDiscoveryService();
            var profiles = service.Discover(fixtureRoot);
            var expected = new[]
            {
                new BrowserProfileInfo
                {
                    DirectoryName = "Profile 3",
                    DisplayName = "Profile 3"
                }
            };

            Assert.Equal(expected, profiles);
        }

        [Fact]
        public void Discover_WhenLocalStateHasNames_ShouldUseDisplayNames()
        {
            var fixtureRoot = GetFixtureRoot("local-state-names");
            var service = new ChromeProfileDiscoveryService();
            var profiles = service.Discover(fixtureRoot);
            var expected = new[]
            {
                new BrowserProfileInfo
                {
                    DirectoryName = "Default",
                    DisplayName = "Personal"
                },
                new BrowserProfileInfo
                {
                    DirectoryName = "Profile 2",
                    DisplayName = "Work"
                }
            };

            Assert.Equal(expected, profiles);
        }

        [Fact]
        public void Discover_WhenLocalStateInvalid_ShouldFallbackToDirectoryName()
        {
            var fixtureRoot = GetFixtureRoot("local-state-invalid");
            var service = new ChromeProfileDiscoveryService();
            var profiles = service.Discover(fixtureRoot);
            var expected = new[]
            {
                new BrowserProfileInfo
                {
                    DirectoryName = "Default",
                    DisplayName = "Default"
                }
            };

            Assert.Equal(expected, profiles);
        }

        [Fact]
        public void Discover_WhenLocalStateMissing_ShouldFallbackToDirectoryName()
        {
            var fixtureRoot = GetFixtureRoot("local-state-missing");
            var service = new ChromeProfileDiscoveryService();
            var profiles = service.Discover(fixtureRoot);
            var expected = new[]
            {
                new BrowserProfileInfo
                {
                    DirectoryName = "Profile 2",
                    DisplayName = "Profile 2"
                }
            };

            Assert.Equal(expected, profiles);
        }

        [Fact]
        public void Discover_ShouldOnlyIncludeDefaultAndProfilePrefix()
        {
            var fixtureRoot = GetFixtureRoot("profile-filtering");
            var service = new ChromeProfileDiscoveryService();
            var profiles = service.Discover(fixtureRoot);
            var expected = new[]
            {
                new BrowserProfileInfo
                {
                    DirectoryName = "Default",
                    DisplayName = "Default"
                },
                new BrowserProfileInfo
                {
                    DirectoryName = "Profile 2",
                    DisplayName = "Profile 2"
                }
            };

            Assert.Equal(expected, profiles);
        }

        private static string GetFixtureRoot(string scenario)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MockData", "discovery", scenario);
        }
    }
}
