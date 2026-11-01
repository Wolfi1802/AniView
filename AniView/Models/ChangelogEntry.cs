namespace AniView.Models
{
    /// <summary>
    /// Eine Neuigkeit zur Anwendung selbst, die unter "What's new" angezeigt wird. Die Texte stehen in den
    /// Ressourcen unter "Changelog.{Id}.Title" und "Changelog.{Id}.Description" (je Sprache eine Datei).
    /// </summary>
    public sealed record ChangelogEntry(DateOnly Date, string Id)
    {
        public string TitleKey => $"Changelog.{Id}.Title";

        public string DescriptionKey => $"Changelog.{Id}.Description";
    }
}
