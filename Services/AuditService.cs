using ExitInterviewSystem.Data;
using ExitInterviewSystem.Helpers;
using ExitInterviewSystem.Models;

namespace ExitInterviewSystem.Services
{
    public class AuditService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogAsync(string action, string? moduleName = null, int? recordId = null, string? details = null)
        {
            var username = _httpContextAccessor.HttpContext?.User?.Identity?.Name;
            var ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

            _context.AuditTrails.Add(new AuditTrail
            {
                Username = username,
                Action = action,
                ModuleName = moduleName,
                RecordId = recordId,
                Details = details,
                IPAddress = ip,
                ActionDate = AppTime.Now
            });

            await _context.SaveChangesAsync();
        }
    }
}
