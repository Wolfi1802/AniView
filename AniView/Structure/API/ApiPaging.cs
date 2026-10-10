namespace AniView.Structure.API
{
    /// <summary>
    /// Gemeinsame Grenzen und Normalisierung für die Seitenparameter der AniList-API.
    /// </summary>
    internal static class ApiPaging
    {
        public const int MIN_PAGE_NUMBER = 1;
        public const int MAX_PAGE_NUMBER = 50;

        public static int GetPageCount(int count) =>
            Math.Clamp(count, MIN_PAGE_NUMBER, MAX_PAGE_NUMBER);

        public static int GetPageIndex(int pageIndex) =>
            Math.Max(MIN_PAGE_NUMBER, pageIndex);
    }
}
