namespace AniView.Models
{
    /// <summary>
    /// Eine Neuigkeit zur Anwendung selbst, die unter "What's new" angezeigt wird.
    /// </summary>
    public sealed record ChangelogEntry(DateOnly Date, string Title, string Description);
}
