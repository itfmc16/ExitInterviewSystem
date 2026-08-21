using ExitInterviewSystem.Helpers;
using System.Text;
using ExitInterviewSystem.Data;
using ExitInterviewSystem.Models;
using ExitInterviewSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExitInterviewSystem.Controllers
{
    [Authorize]
    public class DistrictsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AuditService _auditService;

        public DistrictsController(ApplicationDbContext context, AuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        /// <summary>
        /// Hub is no longer a dual-card chooser — redirect to the district list
        /// (old-system style: list districts, then open Institutions per district).
        /// </summary>
        public IActionResult Hub() => RedirectToAction(nameof(Index));

        public async Task<IActionResult> Index(string? search, int page = 1, int pageSize = 20)
        {
            ViewData["Title"] = "District";
            ViewBag.Search = search;

            var q = _context.Districts.AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                q = q.Where(d => d.Name.Contains(s) || (d.Code != null && d.Code.Contains(s)));
            }

            var paged = await PagedResult<District>.CreateAsync(q.OrderBy(d => d.Name), page, pageSize);
            return View(paged);
        }

        public async Task<IActionResult> ExportExcel(string? search)
        {
            var q = _context.Districts.Include(d => d.Institutions).AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(d => d.Name.Contains(search));
            var list = await q.OrderBy(d => d.Name).ToListAsync();
            var sb = new StringBuilder();
            sb.AppendLine("District\tCode\tActive\tInstitutions");
            foreach (var d in list)
                sb.AppendLine($"{d.Name}\t{d.Code}\t{(d.IsActive ? "Yes" : "No")}\t{string.Join("; ", d.Institutions.Select(i => i.Name))}");
            return File(Encoding.UTF8.GetBytes(sb.ToString()), "application/vnd.ms-excel", "Districts.xls");
        }

        public async Task<IActionResult> ExportWord(string? search)
        {
            var q = _context.Districts.Include(d => d.Institutions).AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(d => d.Name.Contains(search));
            var list = await q.OrderBy(d => d.Name).ToListAsync();
            var sb = new StringBuilder();
            sb.Append("<html><body><h2>Districts</h2><table border='1'><tr><th>District</th><th>Code</th><th>Active</th></tr>");
            foreach (var d in list)
                sb.Append($"<tr><td>{d.Name}</td><td>{d.Code}</td><td>{(d.IsActive ? "Yes" : "No")}</td></tr>");
            sb.Append("</table></body></html>");
            return File(Encoding.UTF8.GetBytes(sb.ToString()), "application/msword", "Districts.doc");
        }

        public IActionResult Create()
        {
            ViewData["Title"] = "Add District";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(District model)
        {
            if (ModelState.IsValid)
            {
                model.DateCreated = AppTime.Now;
                _context.Add(model);
                await _context.SaveChangesAsync();
                await _auditService.LogAsync("Create", "Districts", model.Id, $"District {model.Name} created");
                TempData["Success"] = "District added.";
                return RedirectToAction(nameof(Index));
            }
            ViewData["Title"] = "Add District";
            return View(model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var item = await _context.Districts.FindAsync(id);
            if (item == null) return NotFound();
            ViewData["Title"] = "Edit District";
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, District model)
        {
            if (id != model.Id) return NotFound();
            if (ModelState.IsValid)
            {
                _context.Update(model);
                await _context.SaveChangesAsync();
                await _auditService.LogAsync("Update", "Districts", model.Id, $"District {model.Name} updated");
                TempData["Success"] = "District updated.";
                return RedirectToAction(nameof(Index));
            }
            ViewData["Title"] = "Edit District";
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.Districts.FindAsync(id);
            if (item != null)
            {
                _context.Districts.Remove(item);
                await _context.SaveChangesAsync();
                await _auditService.LogAsync("Delete", "Districts", id, $"District {item.Name} deleted");
                TempData["Success"] = "District deleted.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
