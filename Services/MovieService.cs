using System.Text.Json;
using Moviest.Models;

namespace Moviest.Services
{
    public class MovieService : IMovieService
    {
        private const string Language = "tr-TR";
        private const string TrailerType = "Trailer";
        private const string YouTubeSite = "YouTube";

        private readonly string _apiKey;
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public MovieService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;

            var baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl yapılandırması eksik.");
            _httpClient.BaseAddress = new Uri(baseUrl);

            _apiKey = configuration["ApiSettings:Key"]
                ?? throw new InvalidOperationException("ApiSettings:Key yapılandırması eksik.");
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

        public async Task<MovieResponse> GetPopularMovies(int page = 1)
        {
            var response = await _httpClient.GetAsync($"movie/popular?api_key={_apiKey}&language={Language}&page={page}");
            var json = await ReadResponseAsync(response);
            return JsonSerializer.Deserialize<MovieResponse>(json, _jsonOptions)!;
        }

        public async Task<MovieResponse> GetNowPlaying(int page = 1)
        {
            var response = await _httpClient.GetAsync($"movie/now_playing?api_key={_apiKey}&language={Language}&page={page}");
            var json = await ReadResponseAsync(response);
            return JsonSerializer.Deserialize<MovieResponse>(json, _jsonOptions)!;
        }

        public async Task<MovieResponse> GetTopRatedMovies(int page = 1)
        {
            var response = await _httpClient.GetAsync($"movie/top_rated?api_key={_apiKey}&language={Language}&page={page}");
            var json = await ReadResponseAsync(response);
            return JsonSerializer.Deserialize<MovieResponse>(json, _jsonOptions)!;
        }

        public async Task<MovieResponse> GetUpcomingMovies(int page = 1)
        {
            var response = await _httpClient.GetAsync($"movie/upcoming?api_key={_apiKey}&language={Language}&page={page}");
            var json = await ReadResponseAsync(response);
            return JsonSerializer.Deserialize<MovieResponse>(json, _jsonOptions)!;
        }

        public async Task<MovieDetails> GetMovieDetails(int id)
        {
            var response = await _httpClient.GetAsync($"movie/{id}?api_key={_apiKey}&language={Language}");
            var json = await ReadResponseAsync(response);
            return JsonSerializer.Deserialize<MovieDetails>(json, _jsonOptions)!;
        }

        public async Task<List<Cast>> GetMovieCredits(int id)
        {
            var response = await _httpClient.GetAsync($"movie/{id}/credits?api_key={_apiKey}&language={Language}");
            var json = await ReadResponseAsync(response);
            var creditsResponse = JsonSerializer.Deserialize<CreditsResponse>(json, _jsonOptions);
            return creditsResponse?.Cast ?? new List<Cast>();
        }

        public async Task<MovieResponse> GetMoviesByGenre(int genreId, int page = 1)
        {
            var response = await _httpClient.GetAsync($"discover/movie?api_key={_apiKey}&language={Language}&with_genres={genreId}&page={page}");
            var json = await ReadResponseAsync(response);
            return JsonSerializer.Deserialize<MovieResponse>(json, _jsonOptions)!;
        }

        public async Task<GenreListResponse> GetGenres()
        {
            var response = await _httpClient.GetAsync($"genre/movie/list?api_key={_apiKey}&language={Language}");
            var json = await ReadResponseAsync(response);
            return JsonSerializer.Deserialize<GenreListResponse>(json, _jsonOptions)!;
        }

        public async Task<string> GetGenreNameById(int id)
        {
            var genreList = await GetGenres();
            var genre = genreList?.Genres?.FirstOrDefault(g => g.Id == id);
            return genre?.Name ?? "Bilinmeyen Tür";
        }

        public async Task<MovieResponse> SearchMovies(string query, int page)
        {
            var response = await _httpClient.GetAsync($"search/movie?api_key={_apiKey}&language={Language}&query={Uri.EscapeDataString(query)}&page={page}");
            var json = await ReadResponseAsync(response);
            return JsonSerializer.Deserialize<MovieResponse>(json, _jsonOptions)!;
        }

        public async Task<List<Video>> GetTrailer(int movieId)
        {
            var response = await _httpClient.GetAsync($"movie/{movieId}/videos?api_key={_apiKey}");
            var json = await ReadResponseAsync(response);
            var videoResponse = JsonSerializer.Deserialize<VideoResponse>(json, _jsonOptions);
            return videoResponse?.Results?
                .Where(v => v.Type == TrailerType && v.Site == YouTubeSite)
                .ToList() ?? new List<Video>();
        }

        public async Task<List<Movie>> GetSimilarMovies(int movieId, int page = 1)
        {
            var response = await _httpClient.GetAsync($"movie/{movieId}/similar?api_key={_apiKey}&language={Language}&page={page}");
            var json = await ReadResponseAsync(response);
            var similarMoviesResponse = JsonSerializer.Deserialize<MovieResponse>(json, _jsonOptions);
            return similarMoviesResponse?.Movies ?? new List<Movie>();
        }

        public async Task<ActorDetails> GetActorDetails(int actorId)
        {
            var response = await _httpClient.GetAsync($"person/{actorId}?api_key={_apiKey}&language={Language}");
            var json = await ReadResponseAsync(response);
            return JsonSerializer.Deserialize<ActorDetails>(json, _jsonOptions)!;
        }

        public async Task<List<Movie>> GetActorMovies(int actorId)
        {
            var response = await _httpClient.GetAsync($"person/{actorId}/movie_credits?api_key={_apiKey}&language={Language}");
            var json = await ReadResponseAsync(response);
            var actorCredits = JsonSerializer.Deserialize<ActorCreditsResponse>(json, _jsonOptions);
            return actorCredits?.Cast ?? new List<Movie>();
        }
    }
}
