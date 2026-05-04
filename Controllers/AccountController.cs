using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Moviest.Data;
using Moviest.Models;
using QRCoder;

namespace Moviest.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IdentityContext _context;

        public AccountController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IdentityContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Movies");
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = new IdentityUser
            {
                UserName = model.Username,
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Movies");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Movies");
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _signInManager.PasswordSignInAsync(
                model.Username, model.Password, model.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
                return RedirectToAction("Index", "Movies");

            if (result.RequiresTwoFactor)
                return RedirectToAction("TwoFactorLogin", new { rememberMe = model.RememberMe });

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "Hesabınız çok fazla başarısız giriş denemesi nedeniyle 15 dakika kilitlendi.");
                return View(model);
            }

            ModelState.AddModelError(string.Empty, "Kullanıcı adı veya şifre hatalı.");
            return View(model);
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult TwoFactorLogin(bool rememberMe = false)
        {
            return View(new TwoFactorLoginViewModel { RememberMe = rememberMe });
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> TwoFactorLogin(TwoFactorLoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var code = model.Code.Replace(" ", "").Replace("-", "");
            var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(code, model.RememberMe, model.RememberMachine);

            if (result.Succeeded)
                return RedirectToAction("Index", "Movies");

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "Hesabınız kilitlendi. Lütfen daha sonra tekrar deneyin.");
                return View(model);
            }

            ModelState.AddModelError(string.Empty, "Geçersiz doğrulama kodu. Lütfen tekrar deneyin.");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login");

            return View(new ProfileViewModel
            {
                Username = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                TwoFactorEnabled = user.TwoFactorEnabled
            });
        }

        [HttpGet]
        public async Task<IActionResult> Setup2FA()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login");

            var key = await _userManager.GetAuthenticatorKeyAsync(user);
            if (string.IsNullOrEmpty(key))
            {
                await _userManager.ResetAuthenticatorKeyAsync(user);
                key = await _userManager.GetAuthenticatorKeyAsync(user);
            }

            var email = await _userManager.GetEmailAsync(user) ?? user.UserName ?? "user";
            var formattedKey = FormatAuthenticatorKey(key!);
            var totpUri = $"otpauth://totp/{Uri.EscapeDataString("Moviest")}:{Uri.EscapeDataString(email)}?secret={key}&issuer={Uri.EscapeDataString("Moviest")}&digits=6";

            return View(new Setup2FAViewModel
            {
                AuthenticatorKey = formattedKey,
                QrCodeDataUrl = GenerateQrCodeDataUrl(totpUri)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Setup2FA(Setup2FAViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login");

            if (!ModelState.IsValid)
            {
                await RepopulateQrModel(user, model);
                return View(model);
            }

            var code = model.Code.Replace(" ", "").Replace("-", "");
            var isValid = await _userManager.VerifyTwoFactorTokenAsync(
                user, _userManager.Options.Tokens.AuthenticatorTokenProvider, code);

            if (!isValid)
            {
                ModelState.AddModelError(string.Empty, "Kod doğrulanamadı. Authenticator uygulamanızın kodu doğru olduğundan emin olun.");
                await RepopulateQrModel(user, model);
                return View(model);
            }

            await _userManager.SetTwoFactorEnabledAsync(user, true);
            TempData["Success"] = "İki faktörlü doğrulama başarıyla etkinleştirildi.";
            return RedirectToAction("Profile");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DisableTwoFactor()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login");

            await _userManager.SetTwoFactorEnabledAsync(user, false);
            TempData["Success"] = "İki faktörlü doğrulama devre dışı bırakıldı.";
            return RedirectToAction("Profile");
        }

        private static string FormatAuthenticatorKey(string key)
        {
            return string.Join(" ", Enumerable.Range(0, (key.Length + 3) / 4)
                .Select(i => key.Substring(i * 4, Math.Min(4, key.Length - i * 4))));
        }

        private static string GenerateQrCodeDataUrl(string totpUri)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrData = qrGenerator.CreateQrCode(totpUri, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrData);
            var pngBytes = qrCode.GetGraphic(6);
            return $"data:image/png;base64,{Convert.ToBase64String(pngBytes)}";
        }

        private async Task RepopulateQrModel(IdentityUser user, Setup2FAViewModel model)
        {
            var key = await _userManager.GetAuthenticatorKeyAsync(user);
            var email = await _userManager.GetEmailAsync(user) ?? user.UserName ?? "user";
            var totpUri = $"otpauth://totp/{Uri.EscapeDataString("Moviest")}:{Uri.EscapeDataString(email)}?secret={key}&issuer={Uri.EscapeDataString("Moviest")}&digits=6";
            model.AuthenticatorKey = FormatAuthenticatorKey(key ?? "");
            model.QrCodeDataUrl = GenerateQrCodeDataUrl(totpUri);
        }

        [HttpGet]
        public IActionResult ChangePassword() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login");

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                TempData["Success"] = "Şifreniz başarıyla değiştirildi.";
                return RedirectToAction("Profile");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Stats()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login");

            var userId = user.Id;

            var watchlist = await _context.WatchlistItems
                .Where(w => w.UserId == userId)
                .AsNoTracking()
                .ToListAsync();

            var reviews = await _context.Reviews
                .Where(r => r.UserId == userId)
                .AsNoTracking()
                .ToListAsync();

            var ratingDist = Enumerable.Range(1, 10)
                .Select(r => new RatingBucket(r, watchlist.Count(w => w.PersonalRating == r)))
                .ToList();

            var monthlyActivity = watchlist
                .GroupBy(w => new { w.AddedAt.Year, w.AddedAt.Month })
                .OrderByDescending(g => g.Key.Year).ThenByDescending(g => g.Key.Month)
                .Take(12)
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new MonthlyBucket($"{g.Key.Year}/{g.Key.Month:D2}", g.Count()))
                .ToList();

            var rated = watchlist.Where(w => w.PersonalRating.HasValue).ToList();

            var vm = new UserStatsViewModel
            {
                UserName = user.UserName ?? string.Empty,
                TotalInList = watchlist.Count,
                TotalWatched = watchlist.Count(w => w.IsWatched),
                TotalUnwatched = watchlist.Count(w => !w.IsWatched),
                TotalRated = rated.Count,
                AveragePersonalRating = rated.Count > 0
                    ? Math.Round(rated.Average(w => w.PersonalRating!.Value), 1)
                    : 0,
                TotalReviews = reviews.Count,
                AverageReviewRating = reviews.Count > 0
                    ? Math.Round(reviews.Average(r => r.Rating), 1)
                    : 0,
                RatingDistribution = ratingDist,
                MonthlyActivity = monthlyActivity,
                RecentlyAdded = watchlist
                    .OrderByDescending(w => w.AddedAt)
                    .Take(5)
                    .ToList()
            };

            return View(vm);
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult AccessDenied() => View();
    }
}
