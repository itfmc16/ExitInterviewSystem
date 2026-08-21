using System.ComponentModel.DataAnnotations;

namespace ExitInterviewSystem.Models
{
    /// <summary>
    /// Row in the User Level Permissions matrix (one per table/module per Identity role).
    /// </summary>
    public class UserLevelPermission
    {
        public int Id { get; set; }

        [Required, StringLength(128)]
        public string RoleId { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string TableName { get; set; } = string.Empty;

        public bool CanAdd { get; set; }
        public bool CanDelete { get; set; }
        public bool CanEdit { get; set; }
        public bool CanList { get; set; }
        public bool CanView { get; set; }
        public bool CanSearch { get; set; }
    }
}
