using ExitInterviewSystem.Data;
using ExitInterviewSystem.Models;
using ExitInterviewSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ExitInterviewSystem.Controllers
{
    [Authorize]
    public class RolesController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public static readonly string[] PermissionTables =
        {
            "District", "Employees Exit Form", "Institutions", "Termination", "Users",
            "Audit Trail", "User Level Permissions", "User Levels", "Financial Years",
            "District Report", "Improvements", "Institution_Report", "Home",
            "InstitutionReport.asp", "Employees Problem Lists", "Position", "Messages",
            "hrdistricts.asp", "imessage.asp", "Connect", "User Registration",
            "Search Institution", "Reset Password", "Change Password", "Confirm Password",
            "Questions", "User Activation", "sysdiagrams"
        };

        public RolesController(
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index(string? search)
        {
            ViewData["Title"] = "User Levels";
            ViewBag.Search = search;
            var roles = _roleManager.Roles.AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                roles = roles.Where(r => r.Name != null && r.Name.Contains(s));
            }
            return View(await roles.OrderBy(r => r.Name).ToListAsync());
        }

        [HttpGet]
        public IActionResult Create(string? copyFrom = null)
        {
            ViewData["Title"] = "Create User Level";
            ViewBag.CopyFrom = copyFrom;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Create")]
        public async Task<IActionResult> CreatePost(string roleName)
        {
            if (!string.IsNullOrWhiteSpace(roleName))
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    await _roleManager.CreateAsync(new IdentityRole(roleName.Trim()));
                    TempData["Success"] = $"User level '{roleName}' created.";
                }
                else
                {
                    TempData["Error"] = "User level already exists.";
                }
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role != null
                && !string.Equals(role.Name, "Administrator", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(role.Name, "Anonymous", StringComparison.OrdinalIgnoreCase))
            {
                var perms = _context.UserLevelPermissions.Where(p => p.RoleId == id);
                _context.UserLevelPermissions.RemoveRange(perms);
                await _context.SaveChangesAsync();
                await _roleManager.DeleteAsync(role);
                TempData["Success"] = "User level deleted.";
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Permissions(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return NotFound();

            ViewData["Title"] = "User Level Permissions";
            ViewBag.RoleId = role.Id;
            ViewBag.RoleName = role.Name;

            var existing = await _context.UserLevelPermissions
                .Where(p => p.RoleId == id)
                .ToListAsync();

            var list = new List<UserLevelPermission>();
            foreach (var table in PermissionTables)
            {
                var row = existing.FirstOrDefault(p => p.TableName == table)
                          ?? new UserLevelPermission { RoleId = id, TableName = table };
                list.Add(row);
            }

            return View(list);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Permissions(string roleId, List<UserLevelPermission> model)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null) return NotFound();

            var existing = await _context.UserLevelPermissions
                .Where(p => p.RoleId == roleId)
                .ToListAsync();
            _context.UserLevelPermissions.RemoveRange(existing);

            foreach (var row in model)
            {
                if (string.IsNullOrWhiteSpace(row.TableName)) continue;
                row.Id = 0;
                row.RoleId = roleId;
                _context.UserLevelPermissions.Add(row);
            }
            await _context.SaveChangesAsync();
            TempData["Success"] = "Permissions updated.";
            return RedirectToAction(nameof(Permissions), new { id = roleId });
        }

        public async Task<IActionResult> Assign()
        {
            var users = await _userManager.Users.OrderBy(u => u.UserName).ToListAsync();
            var roles = await _roleManager.Roles.OrderBy(r => r.Name).ToListAsync();

            var model = new AssignRoleViewModel
            {
                Users = users.Select(u => new SelectListItem { Value = u.Id, Text = $"{u.UserName} ({u.FullName})" }).ToList(),
                Roles = roles.Select(r => new SelectListItem { Value = r.Name!, Text = r.Name }).ToList()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign(AssignRoleViewModel model)
        {
            if (string.IsNullOrEmpty(model.UserId) || string.IsNullOrEmpty(model.RoleName))
            {
                TempData["Error"] = "Please select a user and a role.";
                return RedirectToAction(nameof(Assign));
            }

            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction(nameof(Assign));
            }

            if (!await _userManager.IsInRoleAsync(user, model.RoleName))
            {
                await _userManager.AddToRoleAsync(user, model.RoleName);
                TempData["Success"] = $"Role '{model.RoleName}' assigned to {user.UserName}.";
            }
            else
            {
                TempData["Error"] = "User already has this role.";
            }

            return RedirectToAction(nameof(Assign));
        }
    }
}
