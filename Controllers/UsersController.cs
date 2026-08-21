using ExitInterviewSystem.Helpers;
using ExitInterviewSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExitInterviewSystem.Controllers
{
    [Authorize]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string? search, int page = 1, int pageSize = 20)
        {
            ViewData["Title"] = "Users";
            ViewBag.Search = search;

            var q = _userManager.Users.AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                q = q.Where(u =>
                    (u.UserName != null && u.UserName.Contains(s)) ||
                    (u.FullName != null && u.FullName.Contains(s)) ||
                    (u.Email != null && u.Email.Contains(s)) ||
                    (u.Department != null && u.Department.Contains(s)));
            }

            var paged = await PagedResult<ApplicationUser>.CreateAsync(
                q.OrderBy(u => u.UserName), page, pageSize);
            return View(paged);
        }
    }
}
