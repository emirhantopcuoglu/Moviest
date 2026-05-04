using Moviest.Models;
using Moviest.Services;

namespace Moviest.Tests.Infrastructure;

internal sealed class TestMovieService : IMovieService
{
    public Func<string, int, Task<MovieResponse>>? SearchMoviesHandler { get; set; }
    public Func<int, Task<MovieDetails>>? GetMovieDetailsHandler { get; set; }
    public Func<int, Task<List<Video>>>? GetTrailerHandler { get; set; }
    public Func<int, int, Task<List<Movie>>>? GetSimilarMoviesHandler { get; set; }
    public Func<int, Task<List<Cast>>>? GetMovieCreditsHandler { get; set; }

    public Task<MovieResponse> GetPopularMovies(int page = 1) => throw new NotImplementedException();
    public Task<MovieResponse> GetNowPlaying(int page = 1) => throw new NotImplementedException();
    public Task<MovieResponse> GetTopRatedMovies(int page = 1) => throw new NotImplementedException();
    public Task<MovieResponse> GetUpcomingMovies(int page = 1) => throw new NotImplementedException();
    public Task<MovieResponse> GetMoviesByGenre(int genreId, int page = 1) => throw new NotImplementedException();
    public Task<GenreListResponse> GetGenres() => throw new NotImplementedException();
    public Task<string> GetGenreNameById(int id) => throw new NotImplementedException();
    public Task<MovieResponse> SearchMovies(string query, int page)
        => SearchMoviesHandler?.Invoke(query, page) ?? throw new NotImplementedException();
    public Task<ActorDetails> GetActorDetails(int actorId) => throw new NotImplementedException();
    public Task<List<Movie>> GetActorMovies(int actorId) => throw new NotImplementedException();
    public Task<MovieResponse> GetTrendingMovies(int page = 1) => throw new NotImplementedException();
    public Task<List<Movie>> GetMovieRecommendations(int movieId) => Task.FromResult(new List<Movie>());

    public Task<MovieDetails> GetMovieDetails(int id)
        => GetMovieDetailsHandler?.Invoke(id) ?? throw new NotImplementedException();

    public Task<List<Video>> GetTrailer(int movieId)
        => GetTrailerHandler?.Invoke(movieId) ?? Task.FromResult(new List<Video>());

    public Task<List<Movie>> GetSimilarMovies(int movieId, int page = 1)
        => GetSimilarMoviesHandler?.Invoke(movieId, page) ?? Task.FromResult(new List<Movie>());

    public Task<List<Cast>> GetMovieCredits(int id)
        => GetMovieCreditsHandler?.Invoke(id) ?? Task.FromResult(new List<Cast>());
}
