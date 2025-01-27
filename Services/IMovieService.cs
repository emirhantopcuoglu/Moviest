using Microsoft.AspNetCore.Mvc.RazorPages;
using Moviest.Models;

namespace Moviest.Services
{
    public interface IMovieService
    {
        Task<MovieResponse> GetPopularMovies(int page = 1);
        Task<MovieResponse> GetNowPlaying(int page = 1);
        Task<MovieResponse> GetTopRatedMovies(int page = 1);
        Task<MovieResponse> GetUpcomingMovies(int page = 1);
        Task<MovieDetails> GetMovieDetails(int id);
        Task<MovieResponse> GetMoviesByGenre(int genreId, int page = 1);
        Task<GenreListResponse> GetGenres();
        Task<MovieResponse> SearchMovies(string query, int page);
        Task<List<Video>> GetTrailer(int movieId);
        Task<List<Movie>> GetSimilarMovies(int movieId, int page = 1);
        Task<List<Cast>> GetMovieCredits(int id);
        Task<ActorDetails> GetActorDetails(int actorId);
        Task<List<Movie>> GetActorMovies(int actorId);
    }
}