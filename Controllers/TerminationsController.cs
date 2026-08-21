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
    public class TerminationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AuditService _auditService;

        public TerminationsController(ApplicationDbContext context, AuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        public async Task<IActionResult> Index(string? search)
        {
            ViewData["Title"] = "Termination";
            ViewBag.Search = search;
            var q = _context.TerminationTypes.AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                q = q.Where(t => t.Name.Contains(s));
            }
            var list = await q.OrderBy(t => t.Id).ToListAsync();
            return View(list);
        }

        public async Task<IActionResult> ExportExcel(string? search)
        {
            var list = await FilterAsync(search);
            var sb = new StringBuilder();
            sb.AppendLine("Termination ID\tTermination Type");
            foreach (var t in list)
                sb.AppendLine($"{t.Id}\t{t.Name}");
            return File(Encoding.UTF8.GetBytes(sb.ToString()), "application/vnd.ms-excel", "TerminationTypes.xls");
        }

        public async Task<IActionResult> ExportWord(string? search)
        {
            var list = await FilterAsync(search);
            var sb = new StringBuilder();
            sb.Append("<html><body><h2>Termination Types</h2><table border='1' cellpadding='4'><tr><th>Termination ID</th><th>Termination Type</th></tr>");
            foreach (var t in list)
                sb.Append($"<tr><td>{t.Id}</td><td>{System.Net.WebUtility.HtmlEncode(t.Name)}</td></tr>");
            sb.Append("</table></body></html>");
            return File(Encoding.UTF8.GetBytes(sb.ToString()), "application/msword", "TerminationTypes.doc");
        }

        public IActionResult Create()
        {
            ViewData["Title"] = "Add Termination Type";
            return View(new TerminationType());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TerminationType model)
        {
            if (ModelState.IsValid)
            {
                model.DateCreated = AppTime.Now;
                _context.Add(model);
                await _context.SaveChangesAsync();
                await _auditService.LogAsync("Create", "TerminationTypes", model.Id, model.Name);
                TempData["Success"] = "Termination type added.";
                return RedirectToAction(nameof(Index));
            }
            ViewData["Title"] = "Add Termination Type";
            return View(model);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var item = await _context.TerminationTypes.FindAsync(id);
            if (item == null) return NotFound();
            ViewData["Title"] = "Termination Type Details";
            return View(item);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var item = await _context.TerminationTypes.FindAsync(id);
            if (item == null) return NotFound();
            ViewData["Title"] = "Edit Termination Type";
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TerminationType model)
        {
            if (id != model.Id) return NotFound();
            if (ModelState.IsValid)
            {
                _context.Update(model);
                await _context.SaveChangesAsync();
                await _auditService.LogAsync("Update", "TerminationTypes", model.Id, model.Name);
                TempData["Success"] = "Termination type updated.";
                return RedirectToAction(nameof(Index));
            }
            ViewData["Title"] = "Edit Termination Type";
            return View(model);
        }

        public async Task<IActionResult> Copy(int id)
        {
            var item = await _context.TerminationTypes.FindAsync(id);
            if (item == null) return NotFound();
            var copy = new TerminationType
            {
                Name = item.Name + " (Copy)",
                IsActive = item.IsActive,
                DateCreated = AppTime.Now
            };
            _context.Add(copy);
            await _context.SaveChangesAsync();
            await _auditService.LogAsync("Copy", "TerminationTypes", copy.Id, copy.Name);
            TempData["Success"] = "Termination type copied.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.TerminationTypes.FindAsync(id);
            if (item != null)
            {
                _context.TerminationTypes.Remove(item);
                await _context.SaveChangesAsync();
                await _auditService.LogAsync("Delete", "TerminationTypes", id, item.Name);
                TempData["Success"] = "Termination type deleted.";
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task<List<TerminationType>> FilterAsync(string? search)
        {
            var q = _context.TerminationTypes.AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(t => t.Name.Contains(search));
            return await q.OrderBy(t => t.Id).ToListAsync();
        }
    }
}
