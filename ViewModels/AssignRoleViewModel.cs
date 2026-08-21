using Microsoft.AspNetCore.Mvc.Rendering;

namespace ExitInterviewSystem.ViewModels
{
    public class AssignRoleViewModel
    {
        public string? UserId { get; set; }
        public string? RoleName { get; set; }
        public List<SelectListItem> Users { get; set; } = new();
        public List<SelectListItem> Roles { get; set; } = new();
    }
}
