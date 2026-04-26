using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moviest.Models;
using Moviest.Services;
namespace Moviest.Controllers
{
    [Authorize]
    public class MoviesController : Controller
    {
        private readonly MovieService _movieService;

        public MoviesController(MovieService movieService)
        {
            _movieService = movieService;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            var movies = await _movieService.GetPopularMovies(page);

            if (movies?.Movies == null || !movies.Movies.Any())
            {
                return View(new List<MovieResponse>());
            }

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = movies.TotalPages;
            return View(movies.Movies);
        }

        public async Task<IActionResult> NowPlaying(int page = 1)
        {
            var movies = await _movieService.GetNowPlaying(page);

            if (movies?.Movies == null || !movies.Movies.Any())
            {
                return View(new List<MovieResponse>());
            }

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = movies.TotalPages;
            return View(movies.Movies);
        }

        public async Task<IActionResult> TopRatedMovies(int page = 1)
        {
            var movies = await _movieService.GetTopRatedMovies(page);

            if (movies?.Movies == null || !movies.Movies.Any())
            {
                return View(new List<MovieResponse>());
            }

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = movies.TotalPages;
            return View(movies.Movies);
        }

        public async Task<IActionResult> UpcomingMovies(int page = 1)
        {
            var movies = await _movieService.GetUpcomingMovies(page);

            if (movies?.Movies == null || !movies.Movies.Any())
            {
                return View(new List<MovieResponse>());
            }

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = movies.TotalPages;
            return View(movies.Movies);
        }

        public async Task<IActionResult> Details(int id)
        {
            var detailsTask = _movieService.GetMovieDetails(id);
            var trailersTask = _movieService.GetTrailer(id);
            var similarTask = _movieService.GetSimilarMovies(id);
            var castTask = _movieService.GetMovieCredits(id);

            await Task.WhenAll(detailsTask, trailersTask, similarTask, castTask);

            var movieDetails = detailsTask.Result;
            movieDetails.Videos = trailersTask.Result;
            movieDetails.SimilarMovies = similarTask.Result;
            movieDetails.Cast = castTask.Result;

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
            var moviesByGenre = await _movieService.GetMoviesByGenre(id, page);
            var genreName = await _movieService.GetGenreNameById(id);
            ViewData["GenreName"] = genreName;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = moviesByGenre.TotalPages;
            return View(moviesByGenre.Movies);
        }

        [HttpGet]
        public async Task<IActionResult> Search(string query, int page = 1)
        {
            var movieResponse = await _movieService.SearchMovies(query, page);
            ViewBag.Query = query;
            ViewBag.CurrentPage = movieResponse.Page;
            ViewBag.TotalPages = movieResponse.TotalPages;
            return View(movieResponse.Movies);
        }
    }
}
