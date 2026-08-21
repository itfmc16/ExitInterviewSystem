namespace ExitInterviewSystem.Models
{
    /// <summary>
    /// Full Active Directory user profile (all commonly used attributes).
    /// ProxyAddresses, HomeDirectory and HomeDrive are intentionally excluded.
    /// </summary>
    public class AdUser
    {
        // Identity
        public string Username { get; set; } = string.Empty;          // sAMAccountName
        public string UserPrincipalName { get; set; } = string.Empty;
        public string Cn { get; set; } = string.Empty;                 // cn
        public string DistinguishedName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;        // displayName
        public string GivenName { get; set; } = string.Empty;          // givenName (first name)
        public string Surname { get; set; } = string.Empty;            // sn
        public string Initials { get; set; } = string.Empty;

        // Contact
        public string Email { get; set; } = string.Empty;              // mail
        public string Telephone { get; set; } = string.Empty;          // telephoneNumber
        public string Mobile { get; set; } = string.Empty;
        public string IpPhone { get; set; } = string.Empty;
        public string Fax { get; set; } = string.Empty;

        // Organisation / job
        public string Title { get; set; } = string.Empty;              // title (job title)
        public string Department { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public string Office { get; set; } = string.Empty;             // physicalDeliveryOfficeName
        public string Description { get; set; } = string.Empty;
        public string Manager { get; set; } = string.Empty;            // friendly name
        public string ManagerDn { get; set; } = string.Empty;
        public string StreetAddress { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;               // l
        public string State { get; set; } = string.Empty;              // st
        public string PostalCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;            // co / c

        // Employee identifiers
        public string EmployeeId { get; set; } = string.Empty;         // employeeID
        public string EmployeeNumber { get; set; } = string.Empty;     // employeeNumber
        public string EmployeeType { get; set; } = string.Empty;

        // Extension attributes (Persal often lives here)
        public string ExtensionAttribute1 { get; set; } = string.Empty;
        public string ExtensionAttribute2 { get; set; } = string.Empty;
        public string ExtensionAttribute3 { get; set; } = string.Empty;
        public string ExtensionAttribute4 { get; set; } = string.Empty;
        public string ExtensionAttribute5 { get; set; } = string.Empty;
        public string ExtensionAttribute6 { get; set; } = string.Empty;
        public string ExtensionAttribute7 { get; set; } = string.Empty;
        public string ExtensionAttribute8 { get; set; } = string.Empty;
        public string ExtensionAttribute9 { get; set; } = string.Empty;
        public string ExtensionAttribute10 { get; set; } = string.Empty;
        public string ExtensionAttribute11 { get; set; } = string.Empty;
        public string ExtensionAttribute12 { get; set; } = string.Empty;
        public string ExtensionAttribute13 { get; set; } = string.Empty;
        public string ExtensionAttribute14 { get; set; } = string.Empty;
        public string ExtensionAttribute15 { get; set; } = string.Empty;

        // Account / security (read-only informative)
        public string AccountStatus { get; set; } = string.Empty;      // Enabled / Disabled
        public string WhenCreated { get; set; } = string.Empty;
        public string WhenChanged { get; set; } = string.Empty;
        public int LogonCount { get; set; }
        public string LastLogon { get; set; } = string.Empty;
        public string Groups { get; set; } = string.Empty;             // memberOf (CN list)

        // Convenience alias used elsewhere in the app
        public string FullName
        {
            get => !string.IsNullOrWhiteSpace(DisplayName) ? DisplayName : $"{GivenName} {Surname}".Trim();
            set => DisplayName = value ?? string.Empty;
        }
    }
}
