using System.Text.Json;
using Moviest.Constants;
using Moviest.Models;

namespace Moviest.Services
{
    public class MovieService : IMovieService
    {
        private const string Language = "tr-TR";
        private const string TrailerType = "Trailer";
        private const string YouTubeSite = "YouTube";
        private const string UnknownGenre = "Bilinmeyen Tür";

        private readonly string _apiKey;
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public MovieService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;

            var baseUrl = configuration[ConfigKeys.ApiBaseUrl]
                ?? throw new InvalidOperationException($"{ConfigKeys.ApiBaseUrl} yapılandırması eksik.");
            _httpClient.BaseAddress = new Uri(baseUrl);

            _apiKey = configuration[ConfigKeys.ApiKey]
                ?? throw new InvalidOperationException($"{ConfigKeys.ApiKey} yapılandırması eksik.");
        }

        private async Task<string> ReadResponseAsync(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"HTTP isteğinde bir sorun oluştu. Hata kodu: {response.StatusCode}");
            }

            var content = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrEmpty(content))
            {
                throw new Exception("Yanıt içeriği boş.");
            }

            return content;
        }

        private string BuildUrl(string endpoint, string queryString)
            => $"{endpoint}?api_key={_apiKey}&language={Language}{queryString}";

        public async Task<MovieResponse> GetPopularMovies(int page = 1)
        {
            var response = await _httpClient.GetAsync(BuildUrl(TmdbEndpoints.PopularMovies, $"&page={page}"));
            var json = await ReadResponseAsync(response);
            return JsonSerializer.Deserialize<MovieResponse>(json, _jsonOptions)!;
        }

        public async Task<MovieResponse> GetNowPlaying(int page = 1)
        {
            var response = await _httpClient.GetAsync(BuildUrl(TmdbEndpoints.NowPlaying, $"&page={page}"));
            var json = await ReadResponseAsync(response);
            return JsonSerializer.Deserialize<MovieResponse>(json, _jsonOptions)!;
        }

        public async Task<MovieResponse> GetTopRatedMovies(int page = 1)
        {
            var response = await _httpClient.GetAsync(BuildUrl(TmdbEndpoints.TopRated, $"&page={page}"));
            var json = await ReadResponseAsync(response);
            return JsonSerializer.Deserialize<MovieResponse>(json, _jsonOptions)!;
        }

        public async Task<MovieResponse> GetUpcomingMovies(int page = 1)
        {
            var response = await _httpClient.GetAsync(BuildUrl(TmdbEndpoints.Upcoming, $"&page={page}"));
            var json = await ReadResponseAsync(response);
            return JsonSerializer.Deserialize<MovieResponse>(json, _jsonOptions)!;
        }

        public async Task<MovieDetails> GetMovieDetails(int id)
        {
            var response = await _httpClient.GetAsync(BuildUrl(string.Format(TmdbEndpoints.MovieDetail, id), ""));
            var json = await ReadResponseAsync(response);
            return JsonSerializer.Deserialize<MovieDetails>(json, _jsonOptions)!;
        }

        public async Task<List<Cast>> GetMovieCredits(int id)
        {
            var response = await _httpClient.GetAsync(BuildUrl(string.Format(TmdbEndpoints.MovieCredits, id), ""));
            var json = await ReadResponseAsync(response);
            var creditsResponse = JsonSerializer.Deserialize<CreditsResponse>(json, _jsonOptions);
            return creditsResponse?.Cast ?? new List<Cast>();
        }

        public async Task<MovieResponse> GetMoviesByGenre(int genreId, int page = 1)
        {
            var response = await _httpClient.GetAsync(BuildUrl(TmdbEndpoints.DiscoverByGenre, $"&with_genres={genreId}&page={page}"));
            var json = await ReadResponseAsync(response);
            return JsonSerializer.Deserialize<MovieResponse>(json, _jsonOptions)!;
        }

        public async Task<GenreListResponse> GetGenres()
        {
            var response = await _httpClient.GetAsync(BuildUrl(TmdbEndpoints.GenreList, ""));
            var json = await ReadResponseAsync(response);
            return JsonSerializer.Deserialize<GenreListResponse>(json, _jsonOptions)!;
        }

        public async Task<string> GetGenreNameById(int id)
        {
            var genreList = await GetGenres();
            var genre = genreList?.Genres?.FirstOrDefault(g => g.Id == id);
            return genre?.Name ?? UnknownGenre;
        }

        public async Task<MovieResponse> SearchMovies(string query, int page)
        {
            var response = await _httpClient.GetAsync(BuildUrl(TmdbEndpoints.SearchMovie, $"&query={Uri.EscapeDataString(query)}&page={page}"));
            var json = await ReadResponseAsync(response);
            return JsonSerializer.Deserialize<MovieResponse>(json, _jsonOptions)!;
        }

        public async Task<List<Video>> GetTrailer(int movieId)
        {
            var url = $"{string.Format(TmdbEndpoints.MovieVideos, movieId)}?api_key={_apiKey}";
            var response = await _httpClient.GetAsync(url);
            var json = await ReadResponseAsync(response);
            var videoResponse = JsonSerializer.Deserialize<VideoResponse>(json, _jsonOptions);
            return videoResponse?.Results?
                .Where(v => v.Type == TrailerType && v.Site == YouTubeSite)
                .ToList() ?? new List<Video>();
        }

        public async Task<List<Movie>> GetSimilarMovies(int movieId, int page = 1)
        {
            var response = await _httpClient.GetAsync(BuildUrl(string.Format(TmdbEndpoints.SimilarMovies, movieId), $"&page={page}"));
            var json = await ReadResponseAsync(response);
            var similarMoviesResponse = JsonSerializer.Deserialize<MovieResponse>(json, _jsonOptions);
            return similarMoviesResponse?.Movies ?? new List<Movie>();
        }

        public async Task<ActorDetails> GetActorDetails(int actorId)
        {
            var response = await _httpClient.GetAsync(BuildUrl(string.Format(TmdbEndpoints.PersonDetail, actorId), ""));
            var json = await ReadResponseAsync(response);
            return JsonSerializer.Deserialize<ActorDetails>(json, _jsonOptions)!;
        }

        public async Task<List<Movie>> GetActorMovies(int actorId)
        {
            var response = await _httpClient.GetAsync(BuildUrl(string.Format(TmdbEndpoints.PersonMovieCredits, actorId), ""));
            var json = await ReadResponseAsync(response);
            var actorCredits = JsonSerializer.Deserialize<ActorCreditsResponse>(json, _jsonOptions);
            return actorCredits?.Cast ?? new List<Movie>();
        }
    }
}
