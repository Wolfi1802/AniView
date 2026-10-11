namespace AniView.Components.Shared
{
    /// <summary>
    /// Darstellung der Einträge in der <see cref="InfiniteTable{TItem}"/>.
    /// </summary>
    public enum InfiniteViewType
    {
        /// <summary>
        /// Klassische Tabelle mit Kopfzeile (<c>HeaderContent</c> und <c>RowTemplate</c>).
        /// </summary>
        Table,

        /// <summary>
        /// Raster aus Karten (<c>CardTemplate</c>).
        /// </summary>
        Cards
    }

    /// <summary>
    /// Ein Eintrag samt Position in der Gesamtliste, zum Beispiel für die Anzeige eines Rangs.
    /// </summary>
    /// <param name="Item">Das Element.</param>
    /// <param name="Index">Nullbasierte Position über alle bisher geladenen Seiten.</param>
    public readonly record struct InfiniteEntry<TItem>(TItem Item, int Index);
}
