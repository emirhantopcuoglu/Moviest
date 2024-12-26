using System.Text.Json;
using Moviest.Models;

namespace Moviest.Services
{
    public class MovieService : IMovieService
    {
        private readonly string apiKey;
        private readonly HttpClient _httpClient;

        public MovieService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(configuration["ApiSettings:BaseUrl"]);
            apiKey = configuration["ApiSettings:Key"];

            if (string.IsNullOrEmpty(apiKey))
            {
                throw new Exception("API anahtarı bulunamadı.");
            }
        }

        // Bu fonksiyon olası HTTP hataları ve null dönüşler için hata mesajı üretir.
        private string HandleErrorMessage(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"HTTP isteğinde bir sorun oluştu. Hata kodu: {response.StatusCode}");
            }

            var content = response.Content.ReadAsStringAsync().Result;

            if (string.IsNullOrEmpty(content))
            {
                throw new Exception("Yanıt içeriği boş.");
            }

            return content;
        }

        // Her deserialization işleminde tekrar nesne üretmek yerine tek bir nesne, tekrar kullanılmak üzere oluşturuldu.
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public async Task<MovieResponse> GetPopularMovies(int page = 1)
        {
            var response = await _httpClient.GetAsync($"movie/popular?api_key={apiKey}&language=tr-TR&page={page}");

            HandleErrorMessage(response);

            var json = HandleErrorMessage(response);

            return JsonSerializer.Deserialize<MovieResponse>(json, _jsonOptions);
        }

        public async Task<MovieDetails> GetMovieDetails(int id)
        {
            var response = await _httpClient.GetAsync($"movie/{id}?api_key={apiKey}&language=tr-TR");
            HandleErrorMessage(response);

            var json = HandleErrorMessage(response);

            return JsonSerializer.Deserialize<MovieDetails>(json, _jsonOptions);
        }

        public async Task<MovieResponse> GetMoviesByGenre(int genreId)
        {
            var response = await _httpClient.GetAsync($"discover/movie?api_key={apiKey}&language=tr-TR&with_genres={genreId}");
            HandleErrorMessage(response);

            var json = HandleErrorMessage(response);

            return JsonSerializer.Deserialize<MovieResponse>(json, _jsonOptions);
        }

        public async Task<GenreListResponse> GetGenres()
        {
            var response = await _httpClient.GetAsync($"genre/movie/list?api_key={apiKey}&language=tr-TR");
            HandleErrorMessage(response);

            var json = HandleErrorMessage(response);

            return JsonSerializer.Deserialize<GenreListResponse>(json, _jsonOptions);
        }

        public async Task<MovieResponse> SearchMovies(string query)
        {
            var response = await _httpClient.GetAsync($"search/movie?api_key={apiKey}&language=tr-TR&query={Uri.EscapeDataString(query)}");
            HandleErrorMessage(response);

            var json = HandleErrorMessage(response);

            return JsonSerializer.Deserialize<MovieResponse>(json, _jsonOptions);
        }
    }
}