using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moviest.Constants;
using Moviest.Data;
using Moviest.Models;
using Moviest.Services;

namespace Moviest.Controllers
{
    [Authorize]
    public class WatchlistController : Controller
    {
        private const int MoviePosterMaxLength = 200;
        private const int MovieTitleMaxLength = 400;
        private const int MovieYearMaxLength = 10;

        private readonly IdentityContext _context;
        private readonly IMovieService _movieService;
        private readonly UserManager<IdentityUser> _userManager;

        public WatchlistController(
            IdentityContext context,
            UserManager<IdentityUser> userManager,
            IMovieService movieService)
        {
            _context = context;
            _userManager = userManager;
            _movieService = movieService;
        }

        public async Task<IActionResult> Index(string? query, string status = "all", string sortBy = "recent")
        {
            var userId = _userManager.GetUserId(User);
            var allItems = await _context.WatchlistItems
                .Where(w => w.UserId == userId)
                .AsNoTracking()
                .ToListAsync();

            var filteredItems = ApplyFilters(allItems, query, status, sortBy);

            return View(new WatchlistIndexViewModel
            {
                Query = query ?? string.Empty,
                Status = status,
                SortBy = sortBy,
                TotalCount = allItems.Count,
                WatchedCount = allItems.Count(item => item.IsWatched),
                RatedCount = allItems.Count(item => item.PersonalRating.HasValue),
                Items = filteredItems
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(AddToWatchlistRequest request)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId))
                return Challenge();

            if (!ModelState.IsValid)
                return CreateInvalidAddResponse(request);

            var exists = await _context.WatchlistItems
                .AnyAsync(w => w.UserId == userId && w.MovieId == request.MovieId);

            if (!exists)
            {
                MovieDetails? movieDetails;
                try
                {
                    movieDetails = await _movieService.GetMovieDetails(request.MovieId);
                }
                catch
                {
                    return CreateAddFailureResponse("Film bilgileri alınamadı. Lütfen tekrar deneyin.", request);
                }

                if (movieDetails == null || string.IsNullOrWhiteSpace(movieDetails.Title))
                    return CreateAddFailureResponse("Film bulunamadı.", request);

                _context.WatchlistItems.Add(new WatchlistItem
                {
                    UserId = userId,
                    MovieId = request.MovieId,
                    MovieTitle = Truncate(movieDetails.Title, MovieTitleMaxLength) ?? string.Empty,
                    MoviePoster = Truncate(movieDetails.Poster, MoviePosterMaxLength),
                    MovieYear = Truncate(GetMovieYear(movieDetails.ReleaseDate), MovieYearMaxLength),
                    AddedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = true, message = "Izleme listesine eklendi." });

            TempData[TempDataKeys.Success] = "Film izleme listesine eklendi.";
            return RedirectToLocalOrDefault(request.ReturnUrl, request.MovieId);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int movieId)
        {
            var userId = _userManager.GetUserId(User);
            var item = await _context.WatchlistItems
                .FirstOrDefaultAsync(w => w.UserId == userId && w.MovieId == movieId);

            if (item != null)
            {
                _context.WatchlistItems.Remove(item);
                await _context.SaveChangesAsync();
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = true, message = "Izleme listesinden cikarildi." });

            TempData[TempDataKeys.Success] = "Film izleme listesinden kaldirildi.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> IsInList(int movieId)
        {
            var userId = _userManager.GetUserId(User);
            var inList = await _context.WatchlistItems
                .AnyAsync(w => w.UserId == userId && w.MovieId == movieId);
            return Json(new { inList });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleWatched(int movieId)
        {
            var userId = _userManager.GetUserId(User);
            var item = await _context.WatchlistItems
                .FirstOrDefaultAsync(w => w.UserId == userId && w.MovieId == movieId);

            if (item != null)
            {
                item.IsWatched = !item.IsWatched;
                await _context.SaveChangesAsync();
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = true, isWatched = item?.IsWatched ?? false });

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rate(int movieId, int? rating)
        {
            var userId = _userManager.GetUserId(User);
            var item = await _context.WatchlistItems
                .FirstOrDefaultAsync(w => w.UserId == userId && w.MovieId == movieId);

            if (item != null)
            {
                item.PersonalRating = (rating >= 1 && rating <= 10) ? rating : null;
                await _context.SaveChangesAsync();
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = true });

            return RedirectToAction("Index");
        }

        private IActionResult CreateAddFailureResponse(string message, AddToWatchlistRequest request)
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                return Json(new { success = false, message });
            }

            TempData[TempDataKeys.Error] = message;
            return RedirectToLocalOrDefault(request.ReturnUrl, request.MovieId);
        }

        private IActionResult CreateInvalidAddResponse(AddToWatchlistRequest request)
        {
            const string message = "Gecersiz film istegi.";

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                Response.StatusCode = StatusCodes.Status400BadRequest;
                return Json(new { success = false, message });
            }

            TempData[TempDataKeys.Error] = message;
            return RedirectToLocalOrDefault(request.ReturnUrl, request.MovieId);
        }

        private IActionResult RedirectToLocalOrDefault(string? returnUrl, int movieId)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Details", "Movies", new { id = movieId })!;
        }

        private static string? GetMovieYear(string? releaseDate)
        {
            return DateTime.TryParse(releaseDate, out var parsedDate)
                ? parsedDate.Year.ToString()
                : null;
        }

        private static string? Truncate(string? value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
                return value;

            return value.Length <= maxLength
                ? value
                : value[..maxLength];
        }

        private static IReadOnlyList<WatchlistItem> ApplyFilters(
            IEnumerable<WatchlistItem> items,
            string? query,
            string? status,
            string? sortBy)
        {
            var filteredItems = items;

            if (!string.IsNullOrWhiteSpace(query))
            {
                filteredItems = filteredItems.Where(item =>
                    item.MovieTitle.Contains(query, StringComparison.OrdinalIgnoreCase));
            }

            filteredItems = status switch
            {
                "watched" => filteredItems.Where(item => item.IsWatched),
                "unwatched" => filteredItems.Where(item => !item.IsWatched),
                "rated" => filteredItems.Where(item => item.PersonalRating.HasValue),
                _ => filteredItems
            };

            filteredItems = sortBy switch
            {
                "title" => filteredItems.OrderBy(item => item.MovieTitle),
                "oldest" => filteredItems.OrderBy(item => item.AddedAt),
                "rating" => filteredItems.OrderByDescending(item => item.PersonalRating ?? 0).ThenBy(item => item.MovieTitle),
                _ => filteredItems.OrderByDescending(item => item.AddedAt)
            };

            return filteredItems.ToList();
        }
    }
}
