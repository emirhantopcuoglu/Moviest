namespace Moviest.Models
{
    public class UserStatsViewModel
    {
        public string UserName { get; set; } = string.Empty;

        public int TotalInList { get; set; }
        public int TotalWatched { get; set; }
        public int TotalUnwatched { get; set; }
        public int TotalRated { get; set; }
        public double AveragePersonalRating { get; set; }

        public int TotalReviews { get; set; }
        public double AverageReviewRating { get; set; }

        public List<RatingBucket> RatingDistribution { get; set; } = [];
        public List<MonthlyBucket> MonthlyActivity { get; set; } = [];
        public List<WatchlistItem> RecentlyAdded { get; set; } = [];
    }

    public record RatingBucket(int Rating, int Count);
    public record MonthlyBucket(string Label, int Count);
}
