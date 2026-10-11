using AniView.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace AniView.Components.Shared
{
    /// <summary>
    /// Tabelle mit Infinite Scroll. Intern wird seitenweise geladen, für den Nutzer wirkt die Liste endlos.
    /// Es gibt zwei Quellen (genau eine muss gesetzt sein):
    /// <list type="bullet">
    /// <item><see cref="Items"/>: vorhandene Liste, die seitenweise (<see cref="PageSize"/>) angezeigt wird.</item>
    /// <item><see cref="LoadPage"/>: Request pro Seite, wird beim Scrollen jeweils für die nächste Seite aufgerufen.</item>
    /// </list>
    /// Bereits geladene Zeilen bleiben erhalten; beim Hochscrollen wird nichts neu geladen.
    /// </summary>
    public partial class InfiniteTable<TItem>
    {
        private const string ModulePath = "./Components/Shared/InfiniteTable.razor.js";

        private readonly List<TItem> _rows = [];

        private CancellationTokenSource _cts = new();
        private IJSObjectReference? _module;
        private DotNetObjectReference<InfiniteTable<TItem>>? _selfReference;
        private ElementReference _sentinel;

        private IReadOnlyList<TItem>? _knownItems;
        private object? _knownResetKey;
        private bool _parametersInitialized;

        private int _nextPage = 1;
        private bool _hasMore = true;
        private bool _isLoading;
        private bool _needsRefresh;
        private string? _error;

        /// <summary>
        /// Vorhandene Liste, die seitenweise angezeigt wird. Wird die Referenz ausgetauscht, beginnt die Tabelle von vorn.
        /// </summary>
        [Parameter]
        public IReadOnlyList<TItem>? Items { get; set; }

        /// <summary>
        /// Lädt eine Seite per Request. Parameter: Seitennummer (ab 1), Seitengröße, Abbruchtoken.
        /// </summary>
        [Parameter]
        public Func<int, int, CancellationToken, Task<PageResult<TItem>>>? LoadPage { get; set; }

        /// <summary>
        /// Anzahl der Elemente pro Seite.
        /// </summary>
        [Parameter]
        public int PageSize { get; set; } = 20;

        /// <summary>
        /// Ändert sich dieser Wert (Equals), wird die Tabelle zurückgesetzt und von Seite 1 neu geladen,
        /// zum Beispiel bei einem geänderten Filter.
        /// </summary>
        [Parameter]
        public object? ResetKey { get; set; }

        /// <summary>
        /// Steuert die Darstellung. Bei <see cref="InfiniteViewType.Table"/> werden <see cref="HeaderContent"/> und
        /// <see cref="RowTemplate"/> benötigt, bei <see cref="InfiniteViewType.Cards"/> das <see cref="CardTemplate"/>.
        /// </summary>
        [Parameter]
        public InfiniteViewType ViewType { get; set; } = InfiniteViewType.Table;

        /// <summary>
        /// Spaltenüberschriften (th-Elemente), nur für <see cref="InfiniteViewType.Table"/>.
        /// </summary>
        [Parameter]
        public RenderFragment? HeaderContent { get; set; }

        /// <summary>
        /// Zellen einer Zeile (td-Elemente), nur für <see cref="InfiniteViewType.Table"/>.
        /// </summary>
        [Parameter]
        public RenderFragment<TItem>? RowTemplate { get; set; }

        /// <summary>
        /// Eine Karte pro Eintrag, nur für <see cref="InfiniteViewType.Cards"/>.
        /// </summary>
        [Parameter]
        public RenderFragment<InfiniteEntry<TItem>>? CardTemplate { get; set; }

        /// <summary>
        /// Optional: wandelt einen Ladefehler in eine Meldung für den Nutzer um.
        /// Ohne Angabe oder bei null wird ein allgemeiner Hinweis angezeigt.
        /// </summary>
        [Parameter]
        public Func<Exception, string?>? ErrorMessageProvider { get; set; }

        /// <summary>
        /// Anzeige, wenn keine Einträge vorhanden sind.
        /// </summary>
        [Parameter]
        public RenderFragment? EmptyContent { get; set; }

        [Parameter]
        public bool Dense { get; set; }

        /// <summary>
        /// Abstand in Pixeln unterhalb der Tabelle, ab dem die nächste Seite vorab geladen wird.
        /// </summary>
        [Parameter]
        public int PreloadDistance { get; set; } = 400;

        [Parameter]
        public string ErrorTitle { get; set; } = "Die Einträge konnten nicht geladen werden";

        [Inject]
        private IJSRuntime JS { get; set; } = default!;

        [Inject]
        private ILogger<InfiniteTable<TItem>> Logger { get; set; } = default!;

        private bool IsEmpty => !_isLoading && !_hasMore && _error is null && _rows.Count == 0;

        /// <summary>
        /// Verwirft alle geladenen Zeilen und lädt ab Seite 1 neu.
        /// </summary>
        public void Reload()
        {
            Reset();
            StateHasChanged();
        }

        protected override void OnParametersSet()
        {
            if ((Items is null) == (LoadPage is null))
            {
                throw new InvalidOperationException(
                    $"{nameof(InfiniteTable<TItem>)} benötigt genau einen der Parameter {nameof(Items)} oder {nameof(LoadPage)}.");
            }

            var missingTemplate = ViewType switch
            {
                InfiniteViewType.Cards when CardTemplate is null => nameof(CardTemplate),
                InfiniteViewType.Table when RowTemplate is null || HeaderContent is null =>
                    $"{nameof(HeaderContent)} und {nameof(RowTemplate)}",
                _ => null
            };

            if (missingTemplate is not null)
            {
                throw new InvalidOperationException(
                    $"{nameof(InfiniteTable<TItem>)} mit {nameof(ViewType)} {ViewType} benötigt {missingTemplate}.");
            }

            var sourceChanged = _parametersInitialized
                && (!ReferenceEquals(Items, _knownItems) || !Equals(ResetKey, _knownResetKey));

            _knownItems = Items;
            _knownResetKey = ResetKey;
            _parametersInitialized = true;

            if (sourceChanged)
            {
                Reset();
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                _module = await JS.InvokeAsync<IJSObjectReference>("import", ModulePath);
                _selfReference = DotNetObjectReference.Create(this);

                // Der Observer meldet beim Start sofort den aktuellen Zustand und löst damit die erste Seite aus.
                await _module.InvokeVoidAsync("observe", _sentinel, _selfReference, PreloadDistance);
            }
            else if (_needsRefresh && _module is not null)
            {
                _needsRefresh = false;

                // Ein Observer meldet nur Änderungen. Nach jedem Ladevorgang erneut anmelden, damit
                // auch bei noch nicht gefülltem Bildschirm die nächste Seite nachgeladen wird.
                await _module.InvokeVoidAsync("refresh", _sentinel);
            }
        }

        [JSInvokable]
        public async Task OnSentinelVisible()
        {
            // Nach einem Fehler wird nur noch per Button erneut geladen, sonst entstünde eine Endlosschleife.
            if (_error is null)
            {
                await LoadNextPageAsync();
            }
        }

        private Task RetryAsync()
        {
            _error = null;
            return LoadNextPageAsync();
        }

        private async Task LoadNextPageAsync()
        {
            if (_isLoading || !_hasMore)
            {
                return;
            }

            var cts = _cts;
            var token = cts.Token;

            _isLoading = true;
            _error = null;
            StateHasChanged();

            try
            {
                var page = await FetchPageAsync(_nextPage, token);
                token.ThrowIfCancellationRequested();

                _rows.AddRange(page.Items);
                _nextPage++;
                _hasMore = page.HasNextPage && page.Items.Count > 0;
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                // Die Tabelle wurde zurückgesetzt oder entfernt, das Ergebnis wird nicht mehr gebraucht.
                return;
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Seite {Page} der Tabelle konnte nicht geladen werden.", _nextPage);
                _error = ErrorMessageProvider?.Invoke(ex)
                    ?? "Bitte prüfe deine Verbindung und versuche es erneut.";
            }
            finally
            {
                if (ReferenceEquals(cts, _cts))
                {
                    _isLoading = false;
                    _needsRefresh = true;
                    StateHasChanged();
                }
            }
        }

        private Task<PageResult<TItem>> FetchPageAsync(int pageNumber, CancellationToken token)
        {
            var size = Math.Max(1, PageSize);

            if (LoadPage is not null)
            {
                return LoadPage(pageNumber, size, token);
            }

            var source = Items!;
            var start = (pageNumber - 1) * size;
            var end = Math.Min(start + size, source.Count);

            var slice = new List<TItem>(Math.Max(0, end - start));
            for (var i = start; i < end; i++)
            {
                slice.Add(source[i]);
            }

            return Task.FromResult(new PageResult<TItem>(slice, end < source.Count));
        }

        private void Reset()
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = new CancellationTokenSource();

            _rows.Clear();
            _nextPage = 1;
            _hasMore = true;
            _isLoading = false;
            _error = null;
            _needsRefresh = true;
        }

        public async ValueTask DisposeAsync()
        {
            _cts.Cancel();
            _cts.Dispose();

            if (_module is not null)
            {
                try
                {
                    await _module.InvokeVoidAsync("dispose", _sentinel);
                    await _module.DisposeAsync();
                }
                catch (JSDisconnectedException)
                {
                    // Die Verbindung ist bereits getrennt, es gibt nichts mehr aufzuräumen.
                }
            }

            _selfReference?.Dispose();
        }
    }
}
