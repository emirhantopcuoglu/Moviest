using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moviest.Constants;
using Moviest.Data;
using Moviest.Models;
using static Moviest.Constants.TempDataKeys;

namespace Moviest.Controllers
{
    [Authorize(Roles = Roles.Admin)]
    public class AdminController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IdentityContext _context;

        public AdminController(UserManager<IdentityUser> userManager, IdentityContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var allUsers = await _userManager.Users.AsNoTracking().ToListAsync();
            var adminRole = await _userManager.GetUsersInRoleAsync(Roles.Admin);

            var viewModel = new AdminDashboardViewModel
            {
                TotalUsers = allUsers.Count,
                AdminCount = adminRole.Count,
                RegularUsers = allUsers.Count - adminRole.Count,
                WatchlistCount = await _context.WatchlistItems.CountAsync()
            };

            return View(viewModel);
        }

        [HttpGet]
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData[Error] = "Geçersiz kullanıcı kimliği.";
                return RedirectToAction("UserList");
            }

            var currentAdminId = _userManager.GetUserId(User);

            if (id == currentAdminId)
            {
                TempData[Error] = "Kendi hesabınızı silemezsiniz!";
                return RedirectToAction("UserList");
            }

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                TempData[Error] = "Kullanıcı bulunamadı.";
                return RedirectToAction("UserList");
            }

            if (await _userManager.IsInRoleAsync(user, Roles.Admin))
            {
                TempData[Error] = "Diğer adminleri silemezsiniz!";
                return RedirectToAction("UserList");
            }

            await _userManager.DeleteAsync(user);
            TempData[Success] = $"{user.UserName} başarıyla silindi.";

            return RedirectToAction("UserList");
        }
    }
}
