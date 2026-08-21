using ExitInterviewSystem.Helpers;
using ExitInterviewSystem.Models;
using ExitInterviewSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ExitInterviewSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly ActiveDirectoryService _adService;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AuditService _auditService;

        public AccountController(
            ActiveDirectoryService adService,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            AuditService auditService)
        {
            _adService = adService;
            _userManager = userManager;
            _signInManager = signInManager;
            _auditService = auditService;
        }

        [AllowAnonymous]
        public IActionResult Login()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            if (!ModelState.IsValid)
                return View(request);

            bool authenticated;
            try
            {
                authenticated = _adService.Authenticate(request.Username, request.Password);
            }
            catch (Exception)
            {
                // AD unreachable — no local fallback accounts
                ModelState.AddModelError("", "Unable to reach Active Directory. Please try again later.");
                return View(request);
            }

            if (!authenticated)
            {
                ModelState.AddModelError("", "Invalid username or password.");
                return View(request);
            }

            AdUser? adUser;
            try
            {
                adUser = _adService.GetUser(request.Username);
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Unable to retrieve user details from Active Directory.");
                return View(request);
            }

            if (adUser == null || string.IsNullOrWhiteSpace(adUser.Username))
            {
                ModelState.AddModelError("", "User not found in Active Directory.");
                return View(request);
            }

            var user = await _userManager.FindByNameAsync(adUser.Username);
            var now = AppTime.Now;

            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = adUser.Username,
                    Email = NullIfEmpty(adUser.Email),
                    FullName = NullIfEmpty(adUser.FullName),
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
                    foreach (var error in result.Errors)
                        ModelState.AddModelError("", error.Description);
                    return View(request);
                }

                await _userManager.AddToRoleAsync(user, "User");
            }
            else
            {
                if (!user.IsActive)
                {
                    ModelState.AddModelError("", "Your account has been deactivated. Contact the administrator.");
                    return View(request);
                }

                // Refresh profile from AD only (exact values; empty AD → null)
                user.Email = NullIfEmpty(adUser.Email);
                user.FullName = NullIfEmpty(adUser.FullName);
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
                user.LastLoginDate = now;
                await _userManager.UpdateAsync(user);
            }

            // Non-persistent session cookie — must log in again after browser close or app restart
            await _signInManager.SignOutAsync();
            await _signInManager.SignInAsync(user, isPersistent: false);
            await _auditService.LogAsync("Login", "Account", null, $"User {user.UserName} logged in");

            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var username = User.Identity?.Name;
            await _signInManager.SignOutAsync();
            await _auditService.LogAsync("Logout", "Account", null, $"User {username} logged out");
            return RedirectToAction("Login", "Account");
        }

        [Authorize]
        public IActionResult ChangePassword()
        {
            ViewData["Title"] = "Change Password";
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            ViewData["Title"] = "Change Password";
            if (string.IsNullOrWhiteSpace(newPassword) || newPassword != confirmPassword)
            {
                ModelState.AddModelError("", "New password and confirmation do not match.");
                return View();
            }
            TempData["Success"] = "Password changes for AD accounts must be done in Active Directory. Contact your system administrator.";
            return RedirectToAction(nameof(ChangePassword));
        }

        private static string? NullIfEmpty(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
