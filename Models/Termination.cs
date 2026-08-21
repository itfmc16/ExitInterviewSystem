using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExitInterviewSystem.Models
{
    public class Termination
    {
        public int Id { get; set; }

        [Required, StringLength(150)]
        [Display(Name = "Employee Name")]
        public string EmployeeName { get; set; } = string.Empty;

        [StringLength(50)]
        [Display(Name = "Persal No")]
        public string? PersalNo { get; set; }

        [Display(Name = "Institution")]
        public int? InstitutionId { get; set; }

        [ForeignKey(nameof(InstitutionId))]
        public Institution? Institution { get; set; }

        [Required, StringLength(100)]
        [Display(Name = "Termination Type")]
        public string TerminationType { get; set; } = string.Empty;

        [Display(Name = "Termination Date")]
        [DataType(DataType.Date)]
        public DateTime? TerminationDate { get; set; }

        public string? Reason { get; set; }

        [Display(Name = "Exit Form Completed")]
        public bool ExitFormCompleted { get; set; }

        [StringLength(100)]
        [Display(Name = "Captured By")]
        public string? CapturedBy { get; set; }

        public DateTime DateCreated { get; set; } = DateTime.Now;
    }
}
