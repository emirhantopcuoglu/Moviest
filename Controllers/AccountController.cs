using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moviest.Data;
using Moviest.Models;
using Moviest.Services;
using QRCoder;

namespace Moviest.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IdentityContext _context;
        private readonly Moviest.Services.IEmailSender _emailSender;
        private readonly EmailSettings _emailSettings;

        public AccountController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IdentityContext context,
            Moviest.Services.IEmailSender emailSender,
            IOptions<EmailSettings> emailSettings)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _emailSender = emailSender;
            _emailSettings = emailSettings.Value;
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
                if (_userManager.Options.SignIn.RequireConfirmedAccount)
                {
                    await SendConfirmationEmailAsync(user);
                    return RedirectToAction("VerificationSent", new { email = model.Email });
                }

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

        private async Task SendConfirmationEmailAsync(IdentityUser user)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmUrl = Url.Action(
                "ConfirmEmail", "Account",
                new { userId = user.Id, token },
                Request.Scheme)!;

            var html = $"""
                <div style="font-family:sans-serif;max-width:480px;margin:auto;padding:32px;background:#111827;border-radius:12px;color:#e2e8f0;">
                    <h2 style="color:#e8b923;margin-bottom:16px;">Moviest — E-posta Doğrulama</h2>
                    <p style="margin-bottom:24px;">Hesabınızı doğrulamak için aşağıdaki butona tıklayın:</p>
                    <a href="{confirmUrl}"
                       style="display:inline-block;background:#e8b923;color:#000;font-weight:700;
                              padding:12px 28px;border-radius:8px;text-decoration:none;font-size:15px;">
                        E-postamı Doğrula
                    </a>
                    <p style="margin-top:24px;font-size:12px;color:#4f5b72;">
                        Bu bağlantı 24 saat geçerlidir. Eğer bu isteği siz yapmadıysanız bu e-postayı yok sayabilirsiniz.
                    </p>
                </div>
                """;

            await _emailSender.SendAsync(user.Email!, "Moviest — E-postanızı Doğrulayın", html);
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
        public IActionResult VerificationSent(string email)
        {
            ViewBag.Email = email;
            return View();
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
                return BadRequest();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            var result = await _userManager.ConfirmEmailAsync(user, token);
            return View(result.Succeeded ? "EmailConfirmed" : "EmailConfirmFailed");
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult ForgotPassword() => View();

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (!_emailSettings.IsConfigured)
            {
                ModelState.AddModelError(string.Empty, "Şifre sıfırlama e-postası şu anda kullanılamıyor. Lütfen yönetici ile iletişime geçin.");
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var resetUrl = Url.Action(
                    "ResetPassword", "Account",
                    new { userId = user.Id, token },
                    Request.Scheme)!;

                var html = $"""
                    <div style="font-family:sans-serif;max-width:480px;margin:auto;padding:32px;background:#111827;border-radius:12px;color:#e2e8f0;">
                        <h2 style="color:#e8b923;margin-bottom:16px;">Moviest — Şifre Sıfırlama</h2>
                        <p style="margin-bottom:24px;">Şifrenizi sıfırlamak için aşağıdaki butona tıklayın:</p>
                        <a href="{resetUrl}"
                           style="display:inline-block;background:#e8b923;color:#000;font-weight:700;
                                  padding:12px 28px;border-radius:8px;text-decoration:none;font-size:15px;">
                            Şifremi Sıfırla
                        </a>
                        <p style="margin-top:24px;font-size:12px;color:#4f5b72;">
                            Bu bağlantı 1 saat geçerlidir. Eğer bu isteği siz yapmadıysanız bu e-postayı yok sayabilirsiniz.
                        </p>
                    </div>
                    """;

                await _emailSender.SendAsync(user.Email!, "Moviest — Şifre Sıfırlama", html);
            }

            return RedirectToAction("ForgotPasswordConfirmation");
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult ForgotPasswordConfirmation() => View();

        [AllowAnonymous]
        [HttpGet]
        public IActionResult ResetPassword(string userId, string token)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
                return BadRequest();

            return View(new ResetPasswordViewModel { UserId = userId, Token = token });
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Geçersiz bağlantı.");
                return View(model);
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
            if (result.Succeeded)
            {
                TempData["Success"] = "Şifreniz başarıyla sıfırlandı. Giriş yapabilirsiniz.";
                return RedirectToAction("Login");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ChangeEmail()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login");

            ViewBag.CurrentEmail = user.Email;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeEmail(ChangeEmailViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login");

            if (!ModelState.IsValid)
            {
                ViewBag.CurrentEmail = user.Email;
                return View(model);
            }

            var passwordCheck = await _userManager.CheckPasswordAsync(user, model.CurrentPassword);
            if (!passwordCheck)
            {
                ModelState.AddModelError(string.Empty, "Şifreniz hatalı.");
                ViewBag.CurrentEmail = user.Email;
                return View(model);
            }

            var existingUser = await _userManager.FindByEmailAsync(model.NewEmail);
            if (existingUser != null && existingUser.Id != user.Id)
            {
                ModelState.AddModelError(string.Empty, "Bu e-posta adresi zaten kullanımda.");
                ViewBag.CurrentEmail = user.Email;
                return View(model);
            }

            user.Email = model.NewEmail;
            user.NormalizedEmail = _userManager.NormalizeEmail(model.NewEmail);
            user.EmailConfirmed = false;
            await _userManager.UpdateAsync(user);

            await _signInManager.RefreshSignInAsync(user);
            TempData["Success"] = "E-posta adresiniz başarıyla güncellendi.";
            return RedirectToAction("Profile");
        }

        [HttpGet]
        public IActionResult DeleteAccount() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAccount(DeleteAccountViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login");

            if (!ModelState.IsValid)
                return View(model);

            var passwordCheck = await _userManager.CheckPasswordAsync(user, model.CurrentPassword);
            if (!passwordCheck)
            {
                ModelState.AddModelError(string.Empty, "Şifreniz hatalı. Hesabınız silinmedi.");
                return View(model);
            }

            await _signInManager.SignOutAsync();
            await _userManager.DeleteAsync(user);
            return RedirectToAction("Register", "Account");
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult AccessDenied() => View();
    }
}
