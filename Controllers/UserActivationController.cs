using ExitInterviewSystem.Helpers;
using ExitInterviewSystem.Models;
using ExitInterviewSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ExitInterviewSystem.Controllers
{
    [Authorize]
    public class UserActivationController : Controller
    {
        private readonly ActiveDirectoryService _adService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AuditService _auditService;
        private const int PageSize = 20;

        public UserActivationController(
            ActiveDirectoryService adService,
            UserManager<ApplicationUser> userManager,
            AuditService auditService)
        {
            _adService = adService;
            _userManager = userManager;
            _auditService = auditService;
        }

        [HttpGet]
        public IActionResult Index(string? search, int page = 1)
        {
            ViewData["Title"] = "User Activation";
            if (page < 1) page = 1;

            List<AdUser> users = new();
            int total = 0;
            string? error = null;

            try
            {
                (users, total) = _adService.SearchUsers(search, page - 1, PageSize);
            }
            catch (Exception ex)
            {
                error = "Unable to query Active Directory: " + ex.Message;
            }

            ViewBag.Search = search ?? "";
            ViewBag.Page = page;
            ViewBag.PageSize = PageSize;
            ViewBag.TotalRecords = total;
            ViewBag.TotalPages = total == 0 ? 1 : (int)Math.Ceiling(total / (double)PageSize);
            ViewBag.Error = error;

            return View(users);
        }

        [HttpGet]
        public IActionResult Details(string id)
        {
            ViewData["Title"] = "User Activation — Details";
            if (string.IsNullOrWhiteSpace(id))
                return RedirectToAction(nameof(Index));

            AdUser? user = null;
            try
            {
                user = _adService.GetUser(id);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Unable to retrieve user from Active Directory: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }

            if (user == null)
            {
                TempData["Error"] = $"User '{id}' was not found in Active Directory.";
                return RedirectToAction(nameof(Index));
            }

            // Check if already activated in local Identity store
            var local = _userManager.FindByNameAsync(user.Username).GetAwaiter().GetResult();
            ViewBag.IsActivated = local != null && local.IsActive;
            ViewBag.LocalUserId = local?.Id;

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                TempData["Error"] = "Username is required.";
                return RedirectToAction(nameof(Index));
            }

            AdUser? adUser;
            try
            {
                adUser = _adService.GetUser(username);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Unable to retrieve user from Active Directory: " + ex.Message;
                return RedirectToAction(nameof(Details), new { id = username });
            }

            if (adUser == null)
            {
                TempData["Error"] = $"User '{username}' was not found in Active Directory.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.FindByNameAsync(adUser.Username);
            var now = AppTime.Now;

            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = adUser.Username,
                    Email = NullIfEmpty(adUser.Email),
                    FullName = NullIfEmpty(adUser.DisplayName),
                    FirstName = NullIfEmpty(adUser.GivenName),
                    LastName = NullIfEmpty(adUser.Surname),
                    Department = NullIfEmpty(adUser.Department),
                    JobTitle = NullIfEmpty(adUser.Title),
                    EmployeeNumber = NullIfEmpty(adUser.EmployeeNumber),
                    EmployeeId = NullIfEmpty(adUser.EmployeeId),
                    Office = NullIfEmpty(adUser.Office),
                    Telephone = NullIfEmpty(adUser.Telephone),
                    Mobile = NullIfEmpty(adUser.Mobile),
                    Company = NullIfEmpty(adUser.Company),
                    Description = NullIfEmpty(adUser.Description),
                    Manager = NullIfEmpty(adUser.Manager),
                    UserPrincipalName = NullIfEmpty(adUser.UserPrincipalName),
                    DistinguishedName = NullIfEmpty(adUser.DistinguishedName),
                    ExtensionAttribute1 = NullIfEmpty(adUser.ExtensionAttribute1),
                    EmployeeType = NullIfEmpty(adUser.EmployeeType),
                    AdGroups = NullIfEmpty(adUser.Groups),
                    LastLoginDate = now,
                    EmailConfirmed = true,
                    IsActive = true
                };

                var result = await _userManager.CreateAsync(user);
                if (!result.Succeeded)
                {
                    TempData["Error"] = string.Join("; ", result.Errors.Select(e => e.Description));
                    return RedirectToAction(nameof(Details), new { id = username });
                }

                await _userManager.AddToRoleAsync(user, "User");
                await _auditService.LogAsync("Activate", "UserActivation", null, $"Activated AD user {adUser.Username} ({adUser.DisplayName}) [Id={user.Id}]");
                TempData["Success"] = $"User '{adUser.DisplayName}' ({adUser.Username}) has been activated successfully.";
            }
            else
            {
                // Re-activate + refresh profile from AD
                user.IsActive = true;
                user.Email = NullIfEmpty(adUser.Email);
                user.FullName = NullIfEmpty(adUser.DisplayName);
                user.FirstName = NullIfEmpty(adUser.GivenName);
                user.LastName = NullIfEmpty(adUser.Surname);
                user.Department = NullIfEmpty(adUser.Department);
                user.JobTitle = NullIfEmpty(adUser.Title);
                user.EmployeeNumber = NullIfEmpty(adUser.EmployeeNumber);
                user.EmployeeId = NullIfEmpty(adUser.EmployeeId);
                user.Office = NullIfEmpty(adUser.Office);
                user.Telephone = NullIfEmpty(adUser.Telephone);
                user.Mobile = NullIfEmpty(adUser.Mobile);
                user.Company = NullIfEmpty(adUser.Company);
                user.Description = NullIfEmpty(adUser.Description);
                user.Manager = NullIfEmpty(adUser.Manager);
                user.UserPrincipalName = NullIfEmpty(adUser.UserPrincipalName);
                user.DistinguishedName = NullIfEmpty(adUser.DistinguishedName);
                user.ExtensionAttribute1 = NullIfEmpty(adUser.ExtensionAttribute1);
                user.EmployeeType = NullIfEmpty(adUser.EmployeeType);
                user.AdGroups = NullIfEmpty(adUser.Groups);

                await _userManager.UpdateAsync(user);
                await _auditService.LogAsync("ReActivate", "UserActivation", null, $"Re-activated / refreshed AD user {adUser.Username} [Id={user.Id}]");
                TempData["Success"] = $"User '{adUser.DisplayName}' ({adUser.Username}) profile refreshed and activated.";
            }

            return RedirectToAction(nameof(Details), new { id = username });
        }

        private static string? NullIfEmpty(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
