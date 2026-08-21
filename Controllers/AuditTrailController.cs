using ExitInterviewSystem.Data;
using ExitInterviewSystem.Helpers;
using ExitInterviewSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExitInterviewSystem.Controllers
{
    [Authorize]
    public class AuditTrailController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuditTrailController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? search, int page = 1, int pageSize = 20)
        {
            ViewData["Title"] = "Audit Trail";
            ViewBag.Search = search;

            var q = _context.AuditTrails.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                q = q.Where(a =>
                    (a.Username != null && a.Username.Contains(s)) ||
                    (a.Action != null && a.Action.Contains(s)) ||
                    (a.ModuleName != null && a.ModuleName.Contains(s)) ||
                    (a.Details != null && a.Details.Contains(s)) ||
                    (a.IPAddress != null && a.IPAddress.Contains(s)));
            }

            var paged = await PagedResult<AuditTrail>.CreateAsync(
                q.OrderByDescending(a => a.ActionDate), page, pageSize);
            return View(paged);
        }
    }
}
