using ExitInterviewSystem.Helpers;
using System.Text;
using ExitInterviewSystem.Data;
using ExitInterviewSystem.Models;
using ExitInterviewSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ExitInterviewSystem.Controllers
{
    [Authorize]
    public class InstitutionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AuditService _auditService;

        public InstitutionsController(ApplicationDbContext context, AuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        public async Task<IActionResult> Index(int? districtId, string? search, int page = 1, int pageSize = 20)
        {
            ViewData["Title"] = "Institutions";
            ViewBag.Search = search;
            ViewBag.DistrictId = districtId;

            // AsNoTracking for faster list reads
            var q = _context.Institutions.AsNoTracking().Include(i => i.District).AsQueryable();

            if (districtId.HasValue)
            {
                q = q.Where(i => i.DistrictId == districtId.Value);
                var dist = await _context.Districts.AsNoTracking().FirstOrDefaultAsync(d => d.Id == districtId.Value);
                ViewBag.DistrictName = dist?.Name ?? "";
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                q = q.Where(i => i.Name.Contains(s) ||
                    (i.InstitutionType != null && i.InstitutionType.Contains(s)) ||
                    (i.District != null && i.District.Name.Contains(s)));
            }

            var paged = await PagedResult<Institution>.CreateAsync(q.OrderBy(i => i.Name), page, pageSize);
            return View(paged);
        }

        public async Task<IActionResult> ExportExcel(string? search, int? districtId)
        {
            var q = _context.Institutions.Include(i => i.District).AsQueryable();
            if (districtId.HasValue) q = q.Where(i => i.DistrictId == districtId);
            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(i => i.Name.Contains(search));
            var list = await q.OrderBy(i => i.Name).ToListAsync();
            var sb = new StringBuilder();
            sb.AppendLine("Name\tDistrict\tType\tContact\tActive");
            foreach (var i in list)
                sb.AppendLine($"{i.Name}\t{i.District?.Name}\t{i.InstitutionType}\t{i.ContactNumber}\t{(i.IsActive ? "Yes" : "No")}");
            return File(Encoding.UTF8.GetBytes(sb.ToString()), "application/vnd.ms-excel", "Institutions.xls");
        }

        public async Task<IActionResult> ExportWord(string? search, int? districtId)
        {
            var q = _context.Institutions.Include(i => i.District).AsQueryable();
            if (districtId.HasValue) q = q.Where(i => i.DistrictId == districtId);
            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(i => i.Name.Contains(search));
            var list = await q.OrderBy(i => i.Name).ToListAsync();
            var sb = new StringBuilder();
            sb.Append("<html><body><h2>Institutions</h2><table border='1'><tr><th>Name</th><th>District</th><th>Type</th></tr>");
            foreach (var i in list)
                sb.Append($"<tr><td>{i.Name}</td><td>{i.District?.Name}</td><td>{i.InstitutionType}</td></tr>");
            sb.Append("</table></body></html>");
            return File(Encoding.UTF8.GetBytes(sb.ToString()), "application/msword", "Institutions.doc");
        }

        public async Task<IActionResult> Create(int? districtId)
        {
            ViewData["Title"] = "Add Institution";
            ViewBag.DistrictId = new SelectList(
                await _context.Districts.Where(d => d.IsActive).OrderBy(d => d.Name).ToListAsync(),
                "Id", "Name", districtId);
            return View(new Institution { DistrictId = districtId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Institution model)
        {
            if (ModelState.IsValid)
            {
                model.DateCreated = AppTime.Now;
                _context.Add(model);
                await _context.SaveChangesAsync();
                await _auditService.LogAsync("Create", "Institutions", model.Id, $"Institution {model.Name} created");
                TempData["Success"] = "Institution added.";
                return RedirectToAction(nameof(Index), new { districtId = model.DistrictId });
            }
            ViewData["Title"] = "Add Institution";
            ViewBag.DistrictId = new SelectList(
                await _context.Districts.Where(d => d.IsActive).OrderBy(d => d.Name).ToListAsync(),
                "Id", "Name", model.DistrictId);
            return View(model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var item = await _context.Institutions.FindAsync(id);
            if (item == null) return NotFound();
            ViewData["Title"] = "Edit Institution";
            ViewBag.DistrictId = new SelectList(
                await _context.Districts.Where(d => d.IsActive).OrderBy(d => d.Name).ToListAsync(),
                "Id", "Name", item.DistrictId);
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Institution model)
        {
            if (id != model.Id) return NotFound();
            if (ModelState.IsValid)
            {
                _context.Update(model);
                await _context.SaveChangesAsync();
                await _auditService.LogAsync("Update", "Institutions", model.Id, $"Institution {model.Name} updated");
                TempData["Success"] = "Institution updated.";
                return RedirectToAction(nameof(Index), new { districtId = model.DistrictId });
            }
            ViewData["Title"] = "Edit Institution";
            ViewBag.DistrictId = new SelectList(
                await _context.Districts.Where(d => d.IsActive).OrderBy(d => d.Name).ToListAsync(),
                "Id", "Name", model.DistrictId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int? districtId)
        {
            var item = await _context.Institutions.FindAsync(id);
            if (item != null)
            {
                var did = item.DistrictId;
                _context.Institutions.Remove(item);
                await _context.SaveChangesAsync();
                await _auditService.LogAsync("Delete", "Institutions", id, $"Institution {item.Name} deleted");
                TempData["Success"] = "Institution deleted.";
                return RedirectToAction(nameof(Index), new { districtId = districtId ?? did });
            }
            return RedirectToAction(nameof(Index), new { districtId });
        }

        public async Task<IActionResult> Report(int? id)
        {
            if (id == null) return NotFound();
            var item = await _context.Institutions.Include(i => i.District).FirstOrDefaultAsync(i => i.Id == id);
            if (item == null) return NotFound();
            return View(item);
        }
    }
}
