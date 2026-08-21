using System.ComponentModel.DataAnnotations;

namespace ExitInterviewSystem.Models
{
    public class District
    {
        public int Id { get; set; }

        [Required, StringLength(150)]
        [Display(Name = "District Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(20)]
        [Display(Name = "Code")]
        public string? Code { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime DateCreated { get; set; } = DateTime.Now;

        public ICollection<Institution> Institutions { get; set; } = new List<Institution>();
    }
}
