using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Moviest.Models;

namespace Moviest.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
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

            return View(new Setup2FAViewModel { AuthenticatorKey = formattedKey, TotpUri = totpUri });
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
                var key = await _userManager.GetAuthenticatorKeyAsync(user);
                var email = await _userManager.GetEmailAsync(user) ?? user.UserName ?? "user";
                model.AuthenticatorKey = FormatAuthenticatorKey(key ?? "");
                model.TotpUri = $"otpauth://totp/{Uri.EscapeDataString("Moviest")}:{Uri.EscapeDataString(email)}?secret={key}&issuer={Uri.EscapeDataString("Moviest")}&digits=6";
                return View(model);
            }

            var code = model.Code.Replace(" ", "").Replace("-", "");
            var isValid = await _userManager.VerifyTwoFactorTokenAsync(
                user, _userManager.Options.Tokens.AuthenticatorTokenProvider, code);

            if (!isValid)
            {
                ModelState.AddModelError(string.Empty, "Kod doğrulanamadı. Authenticator uygulamanızın kodu doğru olduğundan emin olun.");
                var key = await _userManager.GetAuthenticatorKeyAsync(user);
                var email = await _userManager.GetEmailAsync(user) ?? user.UserName ?? "user";
                model.AuthenticatorKey = FormatAuthenticatorKey(key ?? "");
                model.TotpUri = $"otpauth://totp/{Uri.EscapeDataString("Moviest")}:{Uri.EscapeDataString(email)}?secret={key}&issuer={Uri.EscapeDataString("Moviest")}&digits=6";
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

        [AllowAnonymous]
        [HttpGet]
        public IActionResult AccessDenied() => View();
    }
}
