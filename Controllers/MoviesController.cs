using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Moviest.Models;
using Moviest.Services;
namespace Moviest.Controllers
{
    public class MoviesController : Controller
    {
        private readonly MovieService _movieService;

        public MoviesController(MovieService movieService)
        {
            _movieService = movieService;
        }

        private async Task<T> ExecuteServiceCall<T>(Func<Task<T>> serviceCall)
        {
            try
            {
                return await serviceCall();
            }
            catch (Exception ex)
            {

                throw new Exception("Bir hata oluştu.", ex);
            }
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            try
            {
                var movies = await ExecuteServiceCall(() => _movieService.GetPopularMovies(page));

                if (movies?.Movies == null || !movies.Movies.Any())
                {
                    return View(new List<MovieResponse>());
                }

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = movies.TotalPages;
                return View(movies.Movies);
            }
            catch (Exception ex)
            {
                var errorModel = new ErrorViewModel
                {
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                    Message = ex.Message,
                    StackTrace = ex.StackTrace
                };

                return View("Error", errorModel);
            }
        }

        public async Task<IActionResult> NowPlaying(int page = 1)
        {
            try
            {
                var movies = await ExecuteServiceCall(() => _movieService.GetNowPlaying(page));

                if (movies?.Movies == null || !movies.Movies.Any())
                {
                    return View(new List<MovieResponse>());
                }

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = movies.TotalPages;
                return View(movies.Movies);
            }
            catch (Exception ex)
            {
                var errorModel = new ErrorViewModel
                {
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                    Message = ex.Message,
                    StackTrace = ex.StackTrace
                };

                return View("Error", errorModel);
            }
        }

        public async Task<IActionResult> TopRatedMovies(int page = 1)
        {
            try
            {
                var movies = await ExecuteServiceCall(() => _movieService.GetTopRatedMovies(page));

                if (movies?.Movies == null || !movies.Movies.Any())
                {
                    return View(new List<MovieResponse>());
                }

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = movies.TotalPages;
                return View(movies.Movies);
            }
            catch (Exception ex)
            {
                var errorModel = new ErrorViewModel
                {
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                    Message = ex.Message,
                    StackTrace = ex.StackTrace
                };

                return View("Error", errorModel);
            }
        }

        public async Task<IActionResult> UpcomingMovies(int page = 1)
        {
            try
            {
                var movies = await ExecuteServiceCall(() => _movieService.GetUpcomingMovies(page));

                if (movies?.Movies == null || !movies.Movies.Any())
                {
                    return View(new List<MovieResponse>());
                }

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = movies.TotalPages;
                return View(movies.Movies);
            }
            catch (Exception ex)
            {
                var errorModel = new ErrorViewModel
                {
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                    Message = ex.Message,
                    StackTrace = ex.StackTrace
                };

                return View("Error", errorModel);
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var movieDetails = await ExecuteServiceCall(() => _movieService.GetMovieDetails(id));
                var trailers = await ExecuteServiceCall(() => _movieService.GetTrailer(id));
                var similarMovies = await ExecuteServiceCall(() => _movieService.GetSimilarMovies(id));
                var cast = await ExecuteServiceCall(() => _movieService.GetMovieCredits(id));

                movieDetails.Videos = trailers;
                movieDetails.SimilarMovies = similarMovies;
                movieDetails.Cast = cast;

                return View(movieDetails);
            }
            catch (Exception ex)
            {
                var errorModel = new ErrorViewModel
                {
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                    Message = ex.Message,
                    StackTrace = ex.StackTrace
                };

                return View("Error", errorModel);
            }
        }

        public async Task<IActionResult> ActorDetails(int id)
        {
            try
            {
                var actorDetails = await ExecuteServiceCall(() => _movieService.GetActorDetails(id));
                var actorMovies = await ExecuteServiceCall(() => _movieService.GetActorMovies(id));

                var actorViewModel = new ActorDetailsViewModel
                {
                    Actor = actorDetails,
                    Movies = actorMovies
                };
                return View(actorViewModel);
            }
            catch (Exception ex)
            {
                var errorModel = new ErrorViewModel
                {
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                    Message = ex.Message,
                    StackTrace = ex.StackTrace
                };

                return View("Error", errorModel);
            }
        }

        public async Task<IActionResult> Genres()
        {
            try
            {
                var genres = await ExecuteServiceCall(() => _movieService.GetGenres());
                return View(genres);
            }
            catch (Exception ex)
            {
                var errorModel = new ErrorViewModel
                {
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                    Message = ex.Message,
                    StackTrace = ex.StackTrace
                };

                return View("Error", errorModel);
            }
        }

        public async Task<IActionResult> MoviesByGenre(int id, int page = 1)
        {
            try
            {
                var moviesByGenre = await ExecuteServiceCall(() => _movieService.GetMoviesByGenre(id, page));
                var genreName = await ExecuteServiceCall(() => _movieService.GetGenreNameById(id));
                ViewData["GenreName"] = genreName;
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = moviesByGenre.TotalPages;
                return View(moviesByGenre.Movies);
            }
            catch (Exception ex)
            {
                var errorModel = new ErrorViewModel
                {
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                    Message = ex.Message,
                    StackTrace = ex.StackTrace
                };

                return View("Error", errorModel);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Search(string query, int page = 1)
        {
            var movieResponse = await ExecuteServiceCall(() => _movieService.SearchMovies(query, page));
            ViewBag.Query = query;
            ViewBag.CurrentPage = movieResponse.Page;
            ViewBag.TotalPages = movieResponse.TotalPages;
            return View(movieResponse.Movies);
        }
    }
}

