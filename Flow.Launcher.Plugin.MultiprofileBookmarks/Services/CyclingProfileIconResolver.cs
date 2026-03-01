using System;
using System.Collections.Generic;

namespace Flow.Launcher.Plugin.MultiprofileBookmarks.Services
{
    public class CyclingProfileIconResolver : IProfileIconResolver
    {
        private readonly IReadOnlyList<string> _iconPaths;

        public CyclingProfileIconResolver(IReadOnlyList<string> iconPaths)
        {
            if (iconPaths == null)
            {
                throw new ArgumentNullException(nameof(iconPaths));
            }

            if (iconPaths.Count == 0)
            {
                throw new ArgumentException("At least one icon path is required.", nameof(iconPaths));
            }

            _iconPaths = iconPaths;
        }

        public IReadOnlyDictionary<string, string> ResolveIconPathsByProfileDirectory(IReadOnlyList<string> profileDirectories)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (profileDirectories == null || profileDirectories.Count == 0)
            {
                return result;
            }

            for (var i = 0; i < profileDirectories.Count; i++)
            {
                var profileDirectory = profileDirectories[i];
                if (string.IsNullOrWhiteSpace(profileDirectory))
                {
                    continue;
                }

                result[profileDirectory] = _iconPaths[i % _iconPaths.Count];
            }

            return result;
        }
    }
}
