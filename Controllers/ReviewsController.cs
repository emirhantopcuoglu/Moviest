using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Moviest.Constants;
using Moviest.Data;
using Moviest.Models;

namespace Moviest.Controllers
{
    [Authorize]
    [EnableRateLimiting("api")]
    public class ReviewsController : Controller
    {
        private readonly IdentityContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ReviewsController(IdentityContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(AddReviewRequest request)
        {
            if (!ModelState.IsValid)
            {
                TempData[TempDataKeys.Error] = string.Join(" ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                return RedirectToAction("Details", "Movies", new { id = request.MovieId });
            }

            var userId = _userManager.GetUserId(User)!;
            var userName = _userManager.GetUserName(User)!;

            var exists = await _context.Reviews
                .AnyAsync(r => r.UserId == userId && r.MovieId == request.MovieId);

            if (exists)
            {
                TempData[TempDataKeys.Error] = "Bu film için zaten bir yorum yazdınız.";
                return RedirectToAction("Details", "Movies", new { id = request.MovieId });
            }

            _context.Reviews.Add(new Review
            {
                UserId = userId,
                UserName = userName,
                MovieId = request.MovieId,
                MovieTitle = request.MovieTitle.Length > 400
                    ? request.MovieTitle[..400]
                    : request.MovieTitle,
                Content = request.Content,
                Rating = request.Rating,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            TempData[TempDataKeys.Success] = "Yorumunuz eklendi.";
            return RedirectToAction("Details", "Movies", new { id = request.MovieId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);
            var review = await _context.Reviews.FindAsync(id);

            if (review == null || review.UserId != userId)
            {
                TempData[TempDataKeys.Error] = "Yorum bulunamadı veya silme yetkiniz yok.";
                return RedirectToAction("Index", "Movies");
            }

            var movieId = review.MovieId;
            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            TempData[TempDataKeys.Success] = "Yorumunuz silindi.";
            return RedirectToAction("Details", "Movies", new { id = movieId });
        }
    }
}
