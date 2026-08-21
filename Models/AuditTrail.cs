using System.ComponentModel.DataAnnotations;

namespace ExitInterviewSystem.Models
{
    public class AuditTrail
    {
        public int Id { get; set; }

        [StringLength(50)]
        public string? Username { get; set; }

        [Required, StringLength(100)]
        public string Action { get; set; } = string.Empty;

        [StringLength(100)]
        public string? ModuleName { get; set; }

        public int? RecordId { get; set; }

        public string? Details { get; set; }

        [StringLength(50)]
        public string? IPAddress { get; set; }

        public DateTime ActionDate { get; set; } = DateTime.Now;
    }
}
