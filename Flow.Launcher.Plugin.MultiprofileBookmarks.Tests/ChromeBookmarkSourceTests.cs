using Xunit;
using Flow.Launcher.Plugin.MultiprofileBookmarks.Services;
using System;
using System.IO;
using System.Collections.Generic;
using Flow.Launcher.Plugin.MultiprofileBookmarks.Models;

namespace Flow.Launcher.Plugin.MultiprofileBookmarks.Tests
{
    public class ChromeBookmarkSourceTests
    {
        [Fact]
        public void GetBookmarks_WhenFileDoesNotExist_ShouldReturnEmptyList()
        {
            // Arrange
            // Using a path that definitely doesn't contain the Chrome bookmarks structure
            var nonExistentPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var source = new ChromeBookmarkSource(nonExistentPath, "Default", "Chrome");

            // Act
            var results = source.GetBookmarks();

            // Assert
            Assert.Empty(results);
        }

        [Fact]
        public void GetBookmarks_WhenFileIsValid_ShouldReturnBookmarks()
        {
            // Arrange
            // The tests run from the build output directory, where MockData is copied
            var mockDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MockData", "localAppData");
            var source = new ChromeBookmarkSource(mockDataPath, "Default", "Chrome");

            // Act
            var results = source.GetBookmarks();

            // Assert
            var expected = new[]
            {
                new Bookmark
                {
                    Name = "Google",
                    Url = "https://www.google.com/",
                    Source = "Chrome",
                    Path = "Bookmarks Bar"
                },
                new Bookmark
                {
                    Name = "Flow Launcher",
                    Url = "https://flowlauncher.com/",
                    Source = "Chrome",
                    Path = "Bookmarks Bar"
                }
            };

            Assert.Equal(expected, results);
        }
        
        [Fact]
        public void GetBookmarks_WhenFileIsInvalidJson_ShouldReturnEmptyList()
        {
            // Arrange
            var mockDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MockData", "profile-bad");
            var source = new ChromeBookmarkSource(mockDataPath, "Default", "Chrome");

            // Act
            var results = source.GetBookmarks();

            // Assert
            Assert.Empty(results);
        }
        [Fact]
        public void BookmarkRecord_ShouldHaveValueEquality()
        {
            // Arrange
            var bookmark1 = new Bookmark { Name = "Google", Url = "https://google.com", Source = "Chrome", Path = "Bar" };
            var bookmark2 = new Bookmark { Name = "Google", Url = "https://google.com", Source = "Chrome", Path = "Bar" };
            var bookmark3 = new Bookmark { Name = "Bing", Url = "https://bing.com", Source = "Chrome", Path = "Bar" };

            // Assert
            Assert.Equal(bookmark1, bookmark2); // Value equality
            Assert.NotEqual(bookmark1, bookmark3);
            Assert.False(ReferenceEquals(bookmark1, bookmark2)); // Different references
        }
    }
}
