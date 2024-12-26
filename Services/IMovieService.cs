using Moviest.Models;

namespace Moviest.Services
{
    public interface IMovieService
    {
        Task<MovieResponse> GetPopularMovies(int page = 1);
        Task<MovieDetails> GetMovieDetails(int id);
        Task<MovieResponse> GetMoviesByGenre(int genreId, int page = 1);
        Task<GenreListResponse> GetGenres();
        Task<MovieResponse> SearchMovies(string query);
    }
}