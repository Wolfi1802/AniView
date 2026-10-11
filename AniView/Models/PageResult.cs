namespace AniView.Models
{
    /// <summary>
    /// Eine einzelne Seite eines paginierten Ergebnisses.
    /// </summary>
    /// <param name="Items">Elemente dieser Seite.</param>
    /// <param name="HasNextPage">Gibt an, ob nach dieser Seite weitere Seiten existieren.</param>
    /// <param name="TotalItems">Gesamtanzahl aller Elemente über alle Seiten, sofern bekannt.</param>
    /// <param name="ItemsPerPage">Anzahl der Elemente pro Seite, sofern bekannt.</param>
    public sealed record PageResult<T>(
        IReadOnlyList<T> Items,
        bool HasNextPage,
        int? TotalItems = null,
        int? ItemsPerPage = null);
}
