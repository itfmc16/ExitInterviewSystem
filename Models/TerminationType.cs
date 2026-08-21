using System.ComponentModel.DataAnnotations;

namespace ExitInterviewSystem.Models
{
    public class TerminationType
    {
        public int Id { get; set; }

        [Required, StringLength(200)]
        [Display(Name = "Termination Type")]
        public string Name { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
        public DateTime DateCreated { get; set; } = DateTime.Now;
    }
}
