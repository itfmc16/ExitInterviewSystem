using ExitInterviewSystem.Helpers;
using ExitInterviewSystem.Data;
using ExitInterviewSystem.Models;
using ExitInterviewSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExitInterviewSystem.Controllers
{
    [Authorize]
    public class FinancialYearsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AuditService _auditService;

        public FinancialYearsController(ApplicationDbContext context, AuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        public async Task<IActionResult> Index(string? search)
        {
            ViewData["Title"] = "Financial Years";
            ViewBag.Search = search;
            var q = _context.FinancialYears.AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                q = q.Where(f => f.Name.Contains(s));
            }
            // No pager on Financial Years — load full list (typically small)
            var list = await q.OrderByDescending(f => f.Name).ToListAsync();
            var paged = PagedResult<FinancialYear>.FromList(list, 1, Math.Max(list.Count, 1));
            return View(paged);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FinancialYear model)
        {
            if (ModelState.IsValid)
            {
                model.DateCreated = AppTime.Now;
                _context.Add(model);
                await _context.SaveChangesAsync();
                await _auditService.LogAsync("Create", "FinancialYears", model.Id, $"Financial year {model.Name} created");
                TempData["Success"] = "Financial year added.";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var item = await _context.FinancialYears.FindAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FinancialYear model)
        {
            if (id != model.Id) return NotFound();
            if (ModelState.IsValid)
            {
                _context.Update(model);
                await _context.SaveChangesAsync();
                await _auditService.LogAsync("Update", "FinancialYears", model.Id, $"Financial year {model.Name} updated");
                TempData["Success"] = "Financial year updated.";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.FinancialYears.FindAsync(id);
            if (item != null)
            {
                _context.FinancialYears.Remove(item);
                await _context.SaveChangesAsync();
                await _auditService.LogAsync("Delete", "FinancialYears", id, $"Financial year {item.Name} deleted");
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
