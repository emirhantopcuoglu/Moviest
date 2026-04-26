using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moviest.Constants;

namespace Moviest.Controllers
{
    [Authorize(Roles = Roles.Admin)]
    public class AdminController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;

        public AdminController(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> UserList()
        {
            var currentAdminUserName = User.Identity?.Name;

            var users = await _userManager.Users
                .AsNoTracking()
                .Where(u => u.UserName != currentAdminUserName)
                .ToListAsync();

            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "Geçersiz kullanıcı kimliği.";
                return RedirectToAction("UserList");
            }

            var currentAdminId = _userManager.GetUserId(User);

            if (id == currentAdminId)
            {
                TempData["Error"] = "Kendi hesabınızı silemezsiniz!";
                return RedirectToAction("UserList");
            }

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                TempData["Error"] = "Kullanıcı bulunamadı.";
                return RedirectToAction("UserList");
            }

            if (await _userManager.IsInRoleAsync(user, Roles.Admin))
            {
                TempData["Error"] = "Diğer adminleri silemezsiniz!";
                return RedirectToAction("UserList");
            }

            await _userManager.DeleteAsync(user);
            TempData["Success"] = $"{user.UserName} başarıyla silindi.";

            return RedirectToAction("UserList");
        }
    }
}
