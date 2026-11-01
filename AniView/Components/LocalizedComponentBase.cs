using AniView.Structure.Services;
using Microsoft.AspNetCore.Components;

namespace AniView.Components
{
    /// <summary>
    /// Base class of all components (set once in _Imports.razor). Provides the <see cref="L"/> property for UI texts
    /// and re-renders the component when the language changes.
    /// </summary>
    public abstract class LocalizedComponentBase : ComponentBase
    {
        /// <summary>
        /// Name of the cascading value that MainLayout provides.
        /// </summary>
        public const string LanguageCascadeName = "AniViewLanguage";

        /// <summary>
        /// Central source of all UI texts, for example <c>@L["Nav.Simulcast"]</c>.
        /// </summary>
        [Inject]
        protected LanguageService L { get; set; } = default!;

        /// <summary>
        /// Code of the active language. Never read it: the cascading value only exists so that Blazor
        /// re-renders every component when MainLayout publishes a new language code.
        /// </summary>
        [CascadingParameter(Name = LanguageCascadeName)]
        public string? ActiveLanguage { get; set; }
    }
}
