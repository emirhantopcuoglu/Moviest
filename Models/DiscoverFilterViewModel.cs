namespace Moviest.Models
{
    public class DiscoverFilterViewModel
    {
        public int? GenreId { get; set; }
        public int? YearFrom { get; set; }
        public int? YearTo { get; set; }
        public double? MinRating { get; set; }
        public string SortBy { get; set; } = "popularity.desc";
        public int Page { get; set; } = 1;
        public int TotalPages { get; set; }
        public List<Movie> Movies { get; set; } = [];
        public List<Genre> Genres { get; set; } = [];
    }
}
