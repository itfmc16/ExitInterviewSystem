using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExitInterviewSystem.Models
{
    public class Institution
    {
        public int Id { get; set; }

        [Required, StringLength(200)]
        [Display(Name = "Institution Name")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "District")]
        public int? DistrictId { get; set; }

        [ForeignKey(nameof(DistrictId))]
        public District? District { get; set; }

        [StringLength(50)]
        [Display(Name = "Type")]
        public string? InstitutionType { get; set; }

        [StringLength(300)]
        public string? Address { get; set; }

        [StringLength(50)]
        [Display(Name = "Contact Number")]
        public string? ContactNumber { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime DateCreated { get; set; } = DateTime.Now;
    }
}
