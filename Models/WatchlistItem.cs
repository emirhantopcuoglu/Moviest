namespace Moviest.Models
{
    public class WatchlistItem
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int MovieId { get; set; }
        public string MovieTitle { get; set; } = string.Empty;
        public string? MoviePoster { get; set; }
        public string? MovieYear { get; set; }
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
        public bool IsWatched { get; set; } = false;
        public int? PersonalRating { get; set; }
    }
}
