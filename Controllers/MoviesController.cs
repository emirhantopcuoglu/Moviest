using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Moviest.Models;
using Moviest.Services;

namespace Moviest.Controllers
{
    [Authorize]
    [EnableRateLimiting("api")]
    public class MoviesController : Controller
    {
        private readonly IMovieService _movieService;

        public MoviesController(IMovieService movieService)
        {
            _movieService = movieService;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            var movies = await _movieService.GetPopularMovies(page);
            if (movies?.Movies == null || !movies.Movies.Any())
                return View(new List<Movie>());

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = movies.TotalPages;
            return View(movies.Movies);
        }

        public async Task<IActionResult> NowPlaying(int page = 1)
        {
            var movies = await _movieService.GetNowPlaying(page);
            if (movies?.Movies == null || !movies.Movies.Any())
                return View(new List<Movie>());

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = movies.TotalPages;
            return View(movies.Movies);
        }

        public async Task<IActionResult> TopRatedMovies(int page = 1)
        {
            var movies = await _movieService.GetTopRatedMovies(page);
            if (movies?.Movies == null || !movies.Movies.Any())
                return View(new List<Movie>());

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = movies.TotalPages;
            return View(movies.Movies);
        }

        public async Task<IActionResult> UpcomingMovies(int page = 1)
        {
            var movies = await _movieService.GetUpcomingMovies(page);
            if (movies?.Movies == null || !movies.Movies.Any())
                return View(new List<Movie>());

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = movies.TotalPages;
            return View(movies.Movies);
        }

        public async Task<IActionResult> Trending(int page = 1)
        {
            var movies = await _movieService.GetTrendingMovies(page);
            if (movies?.Movies == null || !movies.Movies.Any())
                return View(new List<Movie>());

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = movies.TotalPages;
            return View(movies.Movies);
        }

        public async Task<IActionResult> Details(int id)
        {
            var trailersTask = TryGetOrDefaultAsync(() => _movieService.GetTrailer(id), new List<Video>());
            var similarTask = TryGetOrDefaultAsync(() => _movieService.GetSimilarMovies(id), new List<Movie>());
            var castTask = TryGetOrDefaultAsync(() => _movieService.GetMovieCredits(id), new List<Cast>());

            MovieDetails? movieDetails;
            try
            {
                movieDetails = await _movieService.GetMovieDetails(id);
            }
            catch
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable);
            }

            if (movieDetails == null)
                return NotFound();

            movieDetails.Videos = await trailersTask;
            movieDetails.SimilarMovies = await similarTask;
            movieDetails.Cast = await castTask;

            return View(movieDetails);
        }

        public async Task<IActionResult> ActorDetails(int id)
        {
            var actorTask = _movieService.GetActorDetails(id);
            var moviesTask = _movieService.GetActorMovies(id);

            await Task.WhenAll(actorTask, moviesTask);

            var actorViewModel = new ActorDetailsViewModel
            {
                Actor = actorTask.Result,
                Movies = moviesTask.Result
            };
            return View(actorViewModel);
        }

        public async Task<IActionResult> Genres()
        {
            var genres = await _movieService.GetGenres();
            return View(genres);
        }

        public async Task<IActionResult> MoviesByGenre(int id, int page = 1)
        {
            var moviesTask = _movieService.GetMoviesByGenre(id, page);
            var genreNameTask = _movieService.GetGenreNameById(id);

            await Task.WhenAll(moviesTask, genreNameTask);

            var moviesByGenre = moviesTask.Result;
            ViewData["GenreName"] = genreNameTask.Result;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = moviesByGenre.TotalPages;
            return View(moviesByGenre.Movies);
        }

        [HttpGet]
        public async Task<IActionResult> Search(string query, int page = 1, string sortBy = "relevance", double? minRating = null)
        {
            if (string.IsNullOrWhiteSpace(query))
                return View(new SearchResultsViewModel());

            var movieResponse = await _movieService.SearchMovies(query, page);
            var movies = ApplySearchFilters(movieResponse?.Movies ?? new List<Movie>(), sortBy, minRating);

            return View(new SearchResultsViewModel
            {
                Query = query,
                SortBy = sortBy,
                MinRating = minRating,
                CurrentPage = movieResponse?.Page ?? 1,
                TotalPages = movieResponse?.TotalPages ?? 1,
                Movies = movies
            });
        }

        [HttpGet]
        public async Task<IActionResult> SearchSuggestions(string query)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
                return Json(Array.Empty<object>());

            var result = await _movieService.SearchMovies(query, 1);
            var suggestions = result?.Movies?.Take(6).Select(m => new
            {
                id = m.Id,
                title = m.Title,
                year = DateTime.TryParse(m.ReleaseDate, out var d) ? d.Year.ToString() : "",
                poster = m.Poster
            }) ?? Enumerable.Empty<object>();

            return Json(suggestions);
        }

        private static async Task<T> TryGetOrDefaultAsync<T>(Func<Task<T>> action, T fallback)
        {
            try
            {
                return await action();
            }
            catch
            {
                return fallback;
            }
        }

        private static IReadOnlyList<Movie> ApplySearchFilters(IEnumerable<Movie> movies, string? sortBy, double? minRating)
        {
            var filteredMovies = movies;

            if (minRating.HasValue)
                filteredMovies = filteredMovies.Where(movie => movie.VoteAverage >= minRating.Value);

            filteredMovies = sortBy switch
            {
                "rating_desc" => filteredMovies.OrderByDescending(movie => movie.VoteAverage),
                "rating_asc" => filteredMovies.OrderBy(movie => movie.VoteAverage),
                "year_desc" => filteredMovies.OrderByDescending(movie => ParseReleaseYear(movie.ReleaseDate)),
                "year_asc" => filteredMovies.OrderBy(movie => ParseReleaseYear(movie.ReleaseDate)),
                "title_asc" => filteredMovies.OrderBy(movie => movie.Title),
                "title_desc" => filteredMovies.OrderByDescending(movie => movie.Title),
                _ => filteredMovies
            };

            return filteredMovies.ToList();
        }

        private static int ParseReleaseYear(string? releaseDate)
        {
            return DateTime.TryParse(releaseDate, out var parsedDate)
                ? parsedDate.Year
                : 0;
        }
    }
}
