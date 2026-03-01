using System.Collections.Generic;
using Flow.Launcher.Plugin.MultiprofileBookmarks.Models;

namespace Flow.Launcher.Plugin.MultiprofileBookmarks.Services
{
    public interface IProfileDiscoveryService
    {
        List<BrowserProfileInfo> Discover(string localApplicationDataFolderPath);
    }
}
