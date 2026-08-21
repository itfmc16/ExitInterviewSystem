using Microsoft.AspNetCore.Identity;

namespace ExitInterviewSystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Populated only from Active Directory values at login / activation
        public string? FullName { get; set; }              // displayName
        public string? FirstName { get; set; }             // givenName
        public string? LastName { get; set; }              // sn
        public string? Department { get; set; }            // department
        public string? JobTitle { get; set; }              // title
        public string? EmployeeNumber { get; set; }        // employeeNumber
        public string? EmployeeId { get; set; }            // employeeID
        public string? Office { get; set; }                // physicalDeliveryOfficeName
        public string? Telephone { get; set; }             // telephoneNumber
        public string? Mobile { get; set; }
        public string? Company { get; set; }
        public string? Description { get; set; }
        public string? Manager { get; set; }
        public string? UserPrincipalName { get; set; }
        public string? DistinguishedName { get; set; }
        public string? ExtensionAttribute1 { get; set; }   // often Persal
        public string? EmployeeType { get; set; }
        public string? AdGroups { get; set; }

        public DateTime LastLoginDate { get; set; }
        public bool IsActive { get; set; } = true;
        public int? InstitutionId { get; set; }
        public int? DistrictId { get; set; }
    }
}
