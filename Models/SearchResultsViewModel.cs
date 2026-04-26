namespace Moviest.Models;

public class SearchResultsViewModel
{
    public string Query { get; set; } = string.Empty;
    public string SortBy { get; set; } = "relevance";
    public double? MinRating { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int ResultCount => Movies.Count;
    public IReadOnlyList<Movie> Movies { get; set; } = Array.Empty<Movie>();
}
