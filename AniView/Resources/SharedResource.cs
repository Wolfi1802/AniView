using System.Resources;

// The neutral (fallback) resources in SharedResource.resx are English.
[assembly: NeutralResourcesLanguage("en")]

namespace AniView.Resources
{
    /// <summary>
    /// Marker type for the shared UI texts. English lives in SharedResource.resx (neutral / fallback),
    /// every further language in SharedResource.{culture}.resx (for example SharedResource.de.resx).
    /// </summary>
    public sealed class SharedResource
    {
    }
}
