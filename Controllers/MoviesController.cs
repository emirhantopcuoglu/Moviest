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
                return View("Error", ex.Message);
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var movieDetails = await ExecuteServiceCall(() => _movieService.GetMovieDetails(id));

                var trailers = await ExecuteServiceCall(() => _movieService.GetTrailer(id));

                movieDetails.Videos = trailers;

                return View(movieDetails);
            }
            catch (Exception ex)
            {
                return View("Error", ex.Message);
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
                return View("Error", ex.Message);
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
                return View("Error", ex.Message);
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

