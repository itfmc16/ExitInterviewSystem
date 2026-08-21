using System.ComponentModel.DataAnnotations;

namespace ExitInterviewSystem.Models
{
    public class FinancialYear
    {
        public int Id { get; set; }

        [Required, StringLength(20)]
        [Display(Name = "Financial Year")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        public DateTime DateCreated { get; set; } = DateTime.Now;
    }
}
