using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Moviest.Constants;
using Moviest.Models;

namespace Moviest.Services
{
    public class MovieService : IMovieService
    {
        private const string Language = "tr-TR";
        private const string TrailerType = "Trailer";
        private const string YouTubeSite = "YouTube";
        private const string UnknownGenre = "Bilinmeyen Tur";

        private readonly string _apiKey;
        private readonly IMemoryCache _cache;
        private readonly HttpClient _httpClient;
        private readonly ILogger<MovieService> _logger;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public MovieService(
            HttpClient httpClient,
            IConfiguration configuration,
            IMemoryCache cache,
            ILogger<MovieService> logger)
        {
            _httpClient = httpClient;
            _cache = cache;
            _logger = logger;

            var baseUrl = configuration[ConfigKeys.ApiBaseUrl];
            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new InvalidOperationException($"{ConfigKeys.ApiBaseUrl} yapilandirmasi eksik veya bos.");
            _httpClient.BaseAddress = new Uri(baseUrl);

            _apiKey = configuration[ConfigKeys.ApiKey] ?? string.Empty;
            if (string.IsNullOrWhiteSpace(_apiKey))
                throw new InvalidOperationException($"{ConfigKeys.ApiKey} yapilandirmasi eksik veya bos.");
        }

        public Task<MovieResponse> GetPopularMovies(int page = 1)
        {
            var url = BuildUrl(TmdbEndpoints.PopularMovies, $"&page={page}");
            return GetCachedAsync($"movies:popular:{page}", () => GetAndDeserializeAsync<MovieResponse>(url), TimeSpan.FromMinutes(5));
        }

        public Task<MovieResponse> GetNowPlaying(int page = 1)
        {
            var url = BuildUrl(TmdbEndpoints.NowPlaying, $"&page={page}");
            return GetCachedAsync($"movies:now-playing:{page}", () => GetAndDeserializeAsync<MovieResponse>(url), TimeSpan.FromMinutes(5));
        }

        public Task<MovieResponse> GetTopRatedMovies(int page = 1)
        {
            var url = BuildUrl(TmdbEndpoints.TopRated, $"&page={page}");
            return GetCachedAsync($"movies:top-rated:{page}", () => GetAndDeserializeAsync<MovieResponse>(url), TimeSpan.FromMinutes(5));
        }

        public Task<MovieResponse> GetUpcomingMovies(int page = 1)
        {
            var url = BuildUrl(TmdbEndpoints.Upcoming, $"&page={page}");
            return GetCachedAsync($"movies:upcoming:{page}", () => GetAndDeserializeAsync<MovieResponse>(url), TimeSpan.FromMinutes(5));
        }

        public Task<MovieDetails> GetMovieDetails(int id)
        {
            var url = BuildUrl(string.Format(TmdbEndpoints.MovieDetail, id), string.Empty);
            return GetCachedAsync($"movie:details:{id}", () => GetAndDeserializeAsync<MovieDetails>(url), TimeSpan.FromMinutes(15));
        }

        public async Task<List<Cast>> GetMovieCredits(int id)
        {
            var url = BuildUrl(string.Format(TmdbEndpoints.MovieCredits, id), string.Empty);
            var creditsResponse = await GetCachedAsync(
                $"movie:credits:{id}",
                () => GetAndDeserializeAsync<CreditsResponse>(url),
                TimeSpan.FromMinutes(15));

            return creditsResponse.Cast ?? new List<Cast>();
        }

        public Task<MovieResponse> GetMoviesByGenre(int genreId, int page = 1)
        {
            var url = BuildUrl(TmdbEndpoints.DiscoverByGenre, $"&with_genres={genreId}&page={page}");
            return GetCachedAsync($"movies:genre:{genreId}:{page}", () => GetAndDeserializeAsync<MovieResponse>(url), TimeSpan.FromMinutes(10));
        }

        public Task<GenreListResponse> GetGenres()
        {
            var url = BuildUrl(TmdbEndpoints.GenreList, string.Empty);
            return GetCachedAsync("movies:genres", () => GetAndDeserializeAsync<GenreListResponse>(url), TimeSpan.FromHours(6));
        }

        public async Task<string> GetGenreNameById(int id)
        {
            var genreList = await GetGenres();
            var genre = genreList.Genres?.FirstOrDefault(g => g.Id == id);
            return genre?.Name ?? UnknownGenre;
        }

        public Task<MovieResponse> SearchMovies(string query, int page)
        {
            var url = BuildUrl(TmdbEndpoints.SearchMovie, $"&query={Uri.EscapeDataString(query)}&page={page}");
            return GetAndDeserializeAsync<MovieResponse>(url);
        }

        public async Task<List<Video>> GetTrailer(int movieId)
        {
            var url = BuildUrl(string.Format(TmdbEndpoints.MovieVideos, movieId), string.Empty);
            var videoResponse = await GetCachedAsync(
                $"movie:videos:{movieId}",
                () => GetAndDeserializeAsync<VideoResponse>(url),
                TimeSpan.FromMinutes(15));

            return videoResponse.Results?
                .Where(v => v.Type == TrailerType && v.Site == YouTubeSite)
                .ToList() ?? new List<Video>();
        }

        public async Task<List<Movie>> GetSimilarMovies(int movieId, int page = 1)
        {
            var url = BuildUrl(string.Format(TmdbEndpoints.SimilarMovies, movieId), $"&page={page}");
            var similarMoviesResponse = await GetCachedAsync(
                $"movie:similar:{movieId}:{page}",
                () => GetAndDeserializeAsync<MovieResponse>(url),
                TimeSpan.FromMinutes(10));

            return similarMoviesResponse.Movies ?? new List<Movie>();
        }

        public Task<ActorDetails> GetActorDetails(int actorId)
        {
            var url = BuildUrl(string.Format(TmdbEndpoints.PersonDetail, actorId), string.Empty);
            return GetCachedAsync($"actor:details:{actorId}", () => GetAndDeserializeAsync<ActorDetails>(url), TimeSpan.FromMinutes(30));
        }

        public async Task<List<Movie>> GetActorMovies(int actorId)
        {
            var url = BuildUrl(string.Format(TmdbEndpoints.PersonMovieCredits, actorId), string.Empty);
            var actorCredits = await GetCachedAsync(
                $"actor:movies:{actorId}",
                () => GetAndDeserializeAsync<ActorCreditsResponse>(url),
                TimeSpan.FromMinutes(30));

            return actorCredits.Cast ?? new List<Movie>();
        }

        public Task<MovieResponse> GetTrendingMovies(int page = 1)
        {
            var url = BuildUrl(TmdbEndpoints.TrendingMovies, $"&page={page}");
            return GetCachedAsync($"movies:trending:{page}", () => GetAndDeserializeAsync<MovieResponse>(url), TimeSpan.FromMinutes(5));
        }

        public async Task<List<Movie>> GetMovieRecommendations(int movieId)
        {
            var url = BuildUrl(string.Format(TmdbEndpoints.MovieRecommendations, movieId), string.Empty);
            var response = await GetCachedAsync(
                $"movie:recommendations:{movieId}",
                () => GetAndDeserializeAsync<MovieResponse>(url),
                TimeSpan.FromMinutes(15));
            return response.Movies ?? [];
        }

        private async Task<T> GetAndDeserializeAsync<T>(string url)
        {
            var response = await _httpClient.GetAsync(url);
            var json = await ReadResponseAsync(response);
            var result = JsonSerializer.Deserialize<T>(json, _jsonOptions);

            if (result == null)
            {
                _logger.LogWarning("TMDB response could not be deserialized for endpoint {Endpoint}.",
                    response.RequestMessage?.RequestUri?.AbsolutePath);
                throw new InvalidOperationException("TMDB yaniti islenemedi.");
            }

            return result;
        }

        private Task<T> GetCachedAsync<T>(string cacheKey, Func<Task<T>> factory, TimeSpan duration)
        {
            return _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = duration;
                return await factory();
            })!;
        }

        private async Task<string> ReadResponseAsync(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("TMDB request failed with status code {StatusCode}.", response.StatusCode);
                throw new HttpRequestException($"HTTP isteginde bir sorun olustu. Hata kodu: {response.StatusCode}");
            }

            var content = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(content))
            {
                _logger.LogWarning("TMDB returned an empty response body.");
                throw new InvalidOperationException("Yanit icerigi bos.");
            }

            return content;
        }

        private string BuildUrl(string endpoint, string queryString)
            => $"{endpoint}?api_key={_apiKey}&language={Language}{queryString}";
    }
}
