using System.Collections.Generic;

namespace Flow.Launcher.Plugin.MultiprofileBookmarks.Services
{
    public interface IProfileIconResolver
    {
        IReadOnlyDictionary<string, string> ResolveIconPathsByProfileDirectory(IReadOnlyList<string> profileDirectories);
    }
}
