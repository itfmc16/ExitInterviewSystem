using System.Diagnostics;
using ExitInterviewSystem.Data;
using ExitInterviewSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExitInterviewSystem.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalExitForms = await _context.ExitInterviewForms.CountAsync();
            ViewBag.TotalTerminations = await _context.Terminations.CountAsync();
            ViewBag.PendingExitForms = await _context.Terminations.CountAsync(t => !t.ExitFormCompleted);
            ViewBag.TotalInstitutions = await _context.Institutions.CountAsync(i => i.IsActive);
            ViewBag.RecentForms = await _context.ExitInterviewForms
                .OrderByDescending(e => e.DateCaptured)
                .Take(5)
                .ToListAsync();

            return View();
        }

        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
