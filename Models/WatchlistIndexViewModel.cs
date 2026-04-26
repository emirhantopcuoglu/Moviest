namespace Moviest.Models;

public class WatchlistIndexViewModel
{
    public string Query { get; set; } = string.Empty;
    public string Status { get; set; } = "all";
    public string SortBy { get; set; } = "recent";
    public int TotalCount { get; set; }
    public int WatchedCount { get; set; }
    public int RatedCount { get; set; }
    public IReadOnlyList<WatchlistItem> Items { get; set; } = Array.Empty<WatchlistItem>();
}
