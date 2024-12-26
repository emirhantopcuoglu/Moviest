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

                return View(movies.Movies);
            }
            catch (Exception ex)
            {
                return View("Error", ex.Message);
            }
        }

        public async Task<ActionResult> Details(int id)
        {
            try
            {
                var movieDetails = await ExecuteServiceCall(() => _movieService.GetMovieDetails(id));
                return View(movieDetails);
            }
            catch (Exception ex)
            {
                return View("Error", ex.Message);
            }
        }
    }
}

