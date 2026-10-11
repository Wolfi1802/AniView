namespace AniView.Models
{
    /// <summary>
    /// Eine einzelne Seite eines paginierten Ergebnisses.
    /// </summary>
    /// <param name="Items">Elemente dieser Seite.</param>
    /// <param name="HasNextPage">Gibt an, ob nach dieser Seite weitere Seiten existieren.</param>
    public sealed record PageResult<T>(IReadOnlyList<T> Items, bool HasNextPage);
}
