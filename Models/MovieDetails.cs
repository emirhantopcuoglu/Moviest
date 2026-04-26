using System.Text.Json.Serialization;

namespace Moviest.Models
{
    public class MovieDetails
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Overview { get; set; } = string.Empty;

        [JsonPropertyName("poster_path")]
        public string Poster { get; set; } = string.Empty;

        [JsonPropertyName("release_date")]
        public string ReleaseDate { get; set; } = string.Empty;

        [JsonPropertyName("vote_average")]
        public double VoteAverage { get; set; }

        public List<Genre> Genres { get; set; } = [];
        public List<Video> Videos { get; set; } = [];
        public List<Movie> SimilarMovies { get; set; } = [];
        public List<Cast> Cast { get; set; } = [];
    }
}
