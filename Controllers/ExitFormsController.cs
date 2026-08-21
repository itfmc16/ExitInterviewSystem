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
    public class ExitFormsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AuditService _auditService;

        public ExitFormsController(ApplicationDbContext context, AuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        public async Task<IActionResult> Index(
            int? institutionId,
            DateTime? exitFrom,
            DateTime? exitTo,
            DateTime? capturedFrom,
            DateTime? capturedTo,
            string? search,
            int page = 1,
            int pageSize = 20)
        {
            ViewData["Title"] = "Employee Exit Interview Form";
            ViewBag.InstitutionId = institutionId;
            ViewBag.ExitFrom = exitFrom?.ToString("yyyy-MM-dd");
            ViewBag.ExitTo = exitTo?.ToString("yyyy-MM-dd");
            ViewBag.CapturedFrom = capturedFrom?.ToString("yyyy-MM-dd");
            ViewBag.CapturedTo = capturedTo?.ToString("yyyy-MM-dd");
            ViewBag.Search = search;

            ViewBag.Institutions = new SelectList(
                await _context.Institutions.Where(i => i.IsActive).OrderBy(i => i.Name).ToListAsync(),
                "Id", "Name", institutionId);

            if (institutionId.HasValue)
            {
                var inst = await _context.Institutions.FindAsync(institutionId.Value);
                ViewBag.InstitutionName = inst?.Name;
            }

            var q = _context.ExitInterviewForms
                .Include(e => e.Institution)
                .Include(e => e.FinancialYear)
                .AsQueryable();

            if (institutionId.HasValue)
                q = q.Where(e => e.InstitutionId == institutionId);

            if (exitFrom.HasValue)
                q = q.Where(e => e.DateOfTermination >= exitFrom);

            if (exitTo.HasValue)
                q = q.Where(e => e.DateOfTermination <= exitTo);

            if (capturedFrom.HasValue)
                q = q.Where(e => e.DateCaptured >= capturedFrom);

            if (capturedTo.HasValue)
                q = q.Where(e => e.DateCaptured <= capturedTo.Value.AddDays(1));

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                q = q.Where(e =>
                    (e.Name != null && e.Name.Contains(s)) ||
                    (e.PersalNo != null && e.PersalNo.Contains(s)) ||
                    (e.Rank != null && e.Rank.Contains(s)) ||
                    (e.TerminationType != null && e.TerminationType.Contains(s)) ||
                    (e.InstitutionOfficeComponent != null && e.InstitutionOfficeComponent.Contains(s)) ||
                    (e.Institution != null && e.Institution.Name.Contains(s)));
            }

            var paged = await Helpers.PagedResult<Models.ExitInterviewForm>.CreateAsync(
                q.OrderByDescending(e => e.DateCaptured), page, pageSize);
            return View(paged);
        }

        public async Task<IActionResult> ExportExcel(
            int? institutionId, DateTime? exitFrom, DateTime? exitTo,
            DateTime? capturedFrom, DateTime? capturedTo, string? search)
        {
            var forms = await GetFilteredAsync(institutionId, exitFrom, exitTo, capturedFrom, capturedTo, search);
            var sb = new StringBuilder();
            sb.AppendLine("Institution\tFull Name\tPersal Number\tGender\tExit Type\tJob Title\tDate of Appointment\tDate of Exit\tDate Captured");
            foreach (var e in forms)
            {
                sb.AppendLine(string.Join("\t",
                    Csv(e.Institution?.Name ?? e.InstitutionOfficeComponent),
                    Csv(e.Name),
                    Csv(e.PersalNo),
                    Csv(e.Gender),
                    Csv(e.TerminationType),
                    Csv(e.Rank ?? e.PostSalaryLevel),
                    e.DateOfAppointment?.ToString("yyyy-MM-dd"),
                    e.DateOfTermination?.ToString("yyyy-MM-dd"),
                    e.DateCaptured.ToString("yyyy-MM-dd")));
            }
            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "application/vnd.ms-excel", "ExitForms.xls");
        }

        public async Task<IActionResult> ExportWord(
            int? institutionId, DateTime? exitFrom, DateTime? exitTo,
            DateTime? capturedFrom, DateTime? capturedTo, string? search)
        {
            var forms = await GetFilteredAsync(institutionId, exitFrom, exitTo, capturedFrom, capturedTo, search);
            var sb = new StringBuilder();
            sb.Append("<html><head><meta charset='utf-8'><title>Exit Forms</title></head><body>");
            sb.Append("<h2>Exit Interview Forms</h2>");
            sb.Append("<table border='1' cellpadding='4' cellspacing='0' style='border-collapse:collapse;font-family:Segoe UI;font-size:11pt;'>");
            sb.Append("<tr style='background:#a8c99a;'><th>Institution</th><th>Full Name</th><th>Persal Number</th><th>Gender</th><th>Exit Type</th><th>Job Title</th><th>Date of Appointment</th><th>Date of Exit</th><th>Date Captured</th></tr>");
            foreach (var e in forms)
            {
                sb.Append("<tr>");
                sb.Append($"<td>{H(e.Institution?.Name ?? e.InstitutionOfficeComponent)}</td>");
                sb.Append($"<td>{H(e.Name)}</td>");
                sb.Append($"<td>{H(e.PersalNo)}</td>");
                sb.Append($"<td>{H(e.Gender)}</td>");
                sb.Append($"<td>{H(e.TerminationType)}</td>");
                sb.Append($"<td>{H(e.Rank ?? e.PostSalaryLevel)}</td>");
                sb.Append($"<td>{e.DateOfAppointment:yyyy-MM-dd}</td>");
                sb.Append($"<td>{e.DateOfTermination:yyyy-MM-dd}</td>");
                sb.Append($"<td>{e.DateCaptured:yyyy-MM-dd}</td>");
                sb.Append("</tr>");
            }
            sb.Append("</table></body></html>");
            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "application/msword", "ExitForms.doc");
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var form = await _context.ExitInterviewForms
                .Include(e => e.Institution)
                .Include(e => e.FinancialYear)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (form == null) return NotFound();
            ViewData["Title"] = "Exit Form Details";
            return View(form);
        }

        public async Task<IActionResult> Create()
        {
            ViewData["Title"] = "Add Exit Details";
            await PopulateDropdowns();
            return View(new ExitInterviewForm());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ExitInterviewForm model)
        {
            ViewData["Title"] = "Add Exit Details";
            if (ModelState.IsValid)
            {
                model.CapturedBy = User.Identity?.Name;
                model.DateCaptured = AppTime.Now;
                _context.Add(model);
                await _context.SaveChangesAsync();
                await _auditService.LogAsync("Create", "ExitForms", model.Id, $"Exit form created for {model.Name}");
                TempData["Success"] = "Exit interview form saved successfully.";
                return RedirectToAction(nameof(Index));
            }
            await PopulateDropdowns(model.InstitutionId, model.FinancialYearId);
            return View(model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var form = await _context.ExitInterviewForms.FindAsync(id);
            if (form == null) return NotFound();
            ViewData["Title"] = "Edit Exit Details";
            await PopulateDropdowns(form.InstitutionId, form.FinancialYearId);
            return View(form);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ExitInterviewForm model)
        {
            if (id != model.Id) return NotFound();
            ViewData["Title"] = "Edit Exit Details";
            if (ModelState.IsValid)
            {
                _context.Update(model);
                await _context.SaveChangesAsync();
                await _auditService.LogAsync("Update", "ExitForms", model.Id, $"Exit form updated for {model.Name}");
                TempData["Success"] = "Exit interview form updated.";
                return RedirectToAction(nameof(Index));
            }
            await PopulateDropdowns(model.InstitutionId, model.FinancialYearId);
            return View(model);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var form = await _context.ExitInterviewForms
                .Include(e => e.Institution)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (form == null) return NotFound();
            ViewData["Title"] = "Delete Exit Form";
            return View(form);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var form = await _context.ExitInterviewForms.FindAsync(id);
            if (form != null)
            {
                _context.ExitInterviewForms.Remove(form);
                await _context.SaveChangesAsync();
                await _auditService.LogAsync("Delete", "ExitForms", id, $"Exit form deleted for {form.Name}");
            }
            TempData["Success"] = "Exit interview form deleted.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<List<ExitInterviewForm>> GetFilteredAsync(
            int? institutionId, DateTime? exitFrom, DateTime? exitTo,
            DateTime? capturedFrom, DateTime? capturedTo, string? search)
        {
            var q = _context.ExitInterviewForms.Include(e => e.Institution).AsQueryable();
            if (institutionId.HasValue) q = q.Where(e => e.InstitutionId == institutionId);
            if (exitFrom.HasValue) q = q.Where(e => e.DateOfTermination >= exitFrom);
            if (exitTo.HasValue) q = q.Where(e => e.DateOfTermination <= exitTo);
            if (capturedFrom.HasValue) q = q.Where(e => e.DateCaptured >= capturedFrom);
            if (capturedTo.HasValue) q = q.Where(e => e.DateCaptured <= capturedTo.Value.AddDays(1));
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                q = q.Where(e =>
                    (e.Name != null && e.Name.Contains(s)) ||
                    (e.PersalNo != null && e.PersalNo.Contains(s)) ||
                    (e.Institution != null && e.Institution.Name.Contains(s)));
            }
            return await q.OrderByDescending(e => e.DateCaptured).ToListAsync();
        }

        private static string Csv(string? v) => (v ?? "").Replace("\t", " ");
        private static string H(string? v) => System.Net.WebUtility.HtmlEncode(v ?? "");

        private async Task PopulateDropdowns(int? institutionId = null, int? financialYearId = null)
        {
            ViewBag.InstitutionId = new SelectList(
                await _context.Institutions.Where(i => i.IsActive).OrderBy(i => i.Name).ToListAsync(),
                "Id", "Name", institutionId);
            ViewBag.FinancialYearId = new SelectList(
                await _context.FinancialYears.Where(f => f.IsActive).OrderByDescending(f => f.Name).ToListAsync(),
                "Id", "Name", financialYearId);
            var termTypes = await _context.TerminationTypes.Where(x => x.IsActive).OrderBy(x => x.Name).Select(x => x.Name).ToListAsync();
            if (!termTypes.Any())
            {
                termTypes = new List<string> {
                    "Resignation", "Retirement (60-65 Years)", "Retirement", "Incapacity", "Dismissal", "EISP", "Transfer", "Other"
                };
            }
            ViewBag.TerminationTypes = new SelectList(termTypes);
        }
    }
}
