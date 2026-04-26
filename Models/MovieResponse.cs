using System.Text.Json.Serialization;

namespace Moviest.Models
{
    public class MovieResponse
    {
        [JsonPropertyName("page")]
        public int Page { get; set; }

        [JsonPropertyName("results")]
        public List<Movie> Movies { get; set; } = [];

        [JsonPropertyName("total_pages")]
        public int TotalPages { get; set; }

        public int TotalMovies { get; set; }
    }
}
