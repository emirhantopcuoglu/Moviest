using System.Text.Json.Serialization;

namespace Moviest.Models
{
    public class MovieDetails
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Overview { get; set; }
        [JsonPropertyName("poster_path")]
        public string Poster { get; set; }

        [JsonPropertyName("release_date")]
        public string ReleaseDate { get; set; }

        [JsonPropertyName("vote_average")]
        public double VoteAverage { get; set; }
        public List<Genre> Genres { get; set; }
        public List<Video> Videos { get; set; } = new List<Video>();
        public List<Movie> SimilarMovies { get; set; }
        public List<Cast> Cast { get; set; } = new List<Cast>();
    }
}