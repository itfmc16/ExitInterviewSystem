using System.ComponentModel.DataAnnotations;

namespace ExitInterviewSystem.Models
{
    /// <summary>
    /// Optional numeric level metadata for display (legacy User Level ID).
    /// Permissions are stored against Identity role Id via UserLevelPermission.
    /// </summary>
    public class UserLevel
    {
        [Key]
        public int Id { get; set; }

        /// <summary>Legacy-style level number (-2 Anonymous … 6 Activator).</summary>
        public int LevelId { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = "";

        public string? Description { get; set; }
    }
}
