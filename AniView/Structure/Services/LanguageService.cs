using System.Globalization;
using System.Resources;
using AniView.Models;
using AniView.Resources;
using Microsoft.JSInterop;

namespace AniView.Structure.Services
{
    /// <summary>
    /// A language the user can choose. <see cref="Code"/> is stored in the browser, <see cref="Culture"/> drives the
    /// resource lookup and the formatting of dates and numbers.
    /// </summary>
    public sealed record LanguageOption(string Code, string DisplayName, CultureInfo Culture);

    /// <summary>
    /// Central place for all UI texts. Texts come from the .resx files (Resources/SharedResource*.resx), the chosen
    /// language is kept in the browser's localStorage. The culture is passed explicitly on every lookup, because the
    /// thread culture of a Blazor Server circuit cannot be changed reliably without reloading the page.
    /// Scoped: one instance per circuit. The first access must happen after the circuit is connected (JS interop).
    /// </summary>
    public sealed class LanguageService
    {
        private const string StorageKey = "aniview-language";

        private static readonly ResourceManager Manager = new(typeof(SharedResource));

        /// <summary>
        /// All offered languages. To add a language: add its SharedResource.{culture}.resx and an entry here.
        /// The first entry is the default and the fallback for unknown values.
        /// </summary>
        public static IReadOnlyList<LanguageOption> SupportedLanguages { get; } =
        [
            new LanguageOption("en", "English", CultureInfo.GetCultureInfo("en-US")),
            new LanguageOption("de", "Deutsch", CultureInfo.GetCultureInfo("de-DE"))
        ];

        private readonly IJSRuntime _js;
        private readonly ILogger<LanguageService> _logger;

        private LanguageOption _current = SupportedLanguages[0];
        private bool _initialized;

        public LanguageService(IJSRuntime js, ILogger<LanguageService> logger)
        {
            _js = js;
            _logger = logger;
        }

        /// <summary>
        /// Raised after the language was changed. Components do not need to subscribe: <c>MainLayout</c> re-renders
        /// all components when this event fires.
        /// </summary>
        public event Action? LanguageChanged;

        public LanguageOption Current => _current;

        public string Code => _current.Code;

        public CultureInfo Culture => _current.Culture;

        /// <summary>
        /// Returns the text for a key. Falls back to the English text and finally to the key itself,
        /// so a missing translation is visible instead of throwing.
        /// </summary>
        public string this[string key] => Manager.GetString(key, Culture) ?? key;

        /// <summary>
        /// Returns the text for a key and inserts the arguments, formatted with the current culture.
        /// </summary>
        public string Format(string key, params object?[] args) => string.Format(Culture, this[key], args);

        /// <summary>
        /// Picks "{keyBase}.One" for a count of 1 and "{keyBase}.Other" otherwise; the count is argument {0}.
        /// </summary>
        public string Plural(string keyBase, long count) =>
            Format(count == 1 ? $"{keyBase}.One" : $"{keyBase}.Other", count);

        /// <summary>
        /// Translates a code from the API (for example the status FINISHED) with the key "{category}.{CODE}".
        /// Unknown codes are made readable (DOUBLE_WORD becomes Double word); empty values give null.
        /// </summary>
        public string? Label(string category, string? code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return null;
            }

            var label = Manager.GetString($"{category}.{code.Trim().ToUpperInvariant()}", Culture);
            if (label is not null)
            {
                return label;
            }

            var words = code.Split('_', StringSplitOptions.RemoveEmptyEntries)
                .Select(word => char.ToUpperInvariant(word[0]) + word[1..].ToLowerInvariant());

            return string.Join(' ', words);
        }

        /// <summary>
        /// Translates a code like <see cref="Label"/>, but returns unknown values unchanged
        /// (for free text from the API, for example the role of a staff member).
        /// </summary>
        public string? Translate(string category, string? code) =>
            string.IsNullOrWhiteSpace(code) ? null : Manager.GetString($"{category}.{code.Trim().ToUpperInvariant()}", Culture) ?? code;

        /// <summary>
        /// Shows a title, or a localized placeholder if the API delivered none.
        /// </summary>
        public string DisplayTitle(string? title) =>
            string.IsNullOrWhiteSpace(title) ? this["Anime.UnknownTitle"] : title;

        /// <summary>
        /// Label of a sort column, the same for filter bar and table header (keys "Sort.{Field}").
        /// </summary>
        public string SortFieldLabel(AnimeSortField field) =>
            Manager.GetString($"Sort.{field}", Culture) ?? field.ToString();

        /// <summary>
        /// Message for a rejected request because of the rate limit of the API.
        /// </summary>
        public string RateLimitMessage(TimeSpan? retryAfter) =>
            retryAfter is { } wait
                ? Format("Error.RateLimit.Wait", Math.Ceiling(wait.TotalSeconds))
                : this["Error.RateLimit.Soon"];

        /// <summary>
        /// Formats a point in time with a pattern that is itself a resource (for example "Format.WeekdayDate"),
        /// so every language decides how its dates look.
        /// </summary>
        public string FormatDate(DateTimeOffset value, string patternKey) =>
            value.ToLocalTime().ToString(this[patternKey], Culture);

        /// <summary>
        /// Formats a date as precisely as AniList knows it (year, month and year, or the full date).
        /// </summary>
        public string FormatPartialDate(AnimePartialDate date)
        {
            if (date.Month is { } month and >= 1 and <= 12 && date.Year is >= 1 and <= 9999)
            {
                if (date.Day is { } day && day >= 1 && day <= DateTime.DaysInMonth(date.Year, month))
                {
                    return new DateTime(date.Year, month, day).ToString("d", Culture);
                }

                if (date.Day is null)
                {
                    return new DateTime(date.Year, month, 1).ToString(Culture.DateTimeFormat.YearMonthPattern, Culture);
                }
            }

            return date.Year.ToString(Culture);
        }

        /// <summary>
        /// Loads the stored language (default: English). Call once after the circuit is connected;
        /// if the browser cannot be reached yet, the next call tries again.
        /// </summary>
        public async Task InitializeAsync()
        {
            if (_initialized)
            {
                return;
            }

            string? stored;
            try
            {
                stored = await _js.InvokeAsync<string?>("localStorage.getItem", StorageKey);
            }
            catch (Exception ex) when (IsRecoverable(ex))
            {
                _logger.LogDebug(ex, "The language setting could not be read.");
                return;
            }

            _initialized = true;

            var option = Find(stored);
            if (option is not null && option != _current)
            {
                _current = option;
                LanguageChanged?.Invoke();
            }

            await ApplyDocumentLanguageAsync();
        }

        /// <summary>
        /// Switches the language immediately and stores the choice in the browser.
        /// Unknown codes are ignored.
        /// </summary>
        public async Task SetLanguageAsync(string code)
        {
            var option = Find(code);
            if (option is null || option == _current)
            {
                return;
            }

            _current = option;
            LanguageChanged?.Invoke();

            try
            {
                await _js.InvokeVoidAsync("localStorage.setItem", StorageKey, option.Code);
            }
            catch (Exception ex) when (IsRecoverable(ex))
            {
                _logger.LogDebug(ex, "The language setting could not be saved.");
            }

            await ApplyDocumentLanguageAsync();
        }

        private static LanguageOption? Find(string? code) =>
            SupportedLanguages.FirstOrDefault(language =>
                string.Equals(language.Code, code?.Trim(), StringComparison.OrdinalIgnoreCase));

        // Keeps the lang attribute of the page in sync (screen readers, hyphenation, browser translation hints).
        private async Task ApplyDocumentLanguageAsync()
        {
            try
            {
                await _js.InvokeVoidAsync("document.documentElement.setAttribute", "lang", _current.Code);
            }
            catch (Exception ex) when (IsRecoverable(ex))
            {
                _logger.LogDebug(ex, "The lang attribute could not be set.");
            }
        }

        private static bool IsRecoverable(Exception ex) =>
            ex is JSException or JSDisconnectedException or InvalidOperationException or TaskCanceledException;
    }
}
