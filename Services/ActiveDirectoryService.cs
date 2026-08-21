using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Text;
using ExitInterviewSystem.Models;

namespace ExitInterviewSystem.Services
{
    public class ActiveDirectoryService
    {
        private readonly IConfiguration _configuration;

        // Only the columns shown on the User Activation grid — keeps LDAP fast
        private static readonly string[] SearchListProperties =
        {
            "sAMAccountName", "displayName", "givenName", "sn", "mail",
            "department", "title", "physicalDeliveryOfficeName",
            "employeeID", "employeeNumber", "userAccountControl"
        };

        public ActiveDirectoryService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public bool Authenticate(string username, string password)
        {
            var domain = _configuration["ActiveDirectory:Domain"];
            if (string.IsNullOrWhiteSpace(domain))
                throw new InvalidOperationException("Active Directory domain is not configured.");

            using var context = new PrincipalContext(ContextType.Domain, domain);
            return context.ValidateCredentials(username, password);
        }

        public AdUser? GetUser(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return null;

            var domain = _configuration["ActiveDirectory:Domain"];
            if (string.IsNullOrWhiteSpace(domain))
                throw new InvalidOperationException("Active Directory domain is not configured.");

            using var context = new PrincipalContext(ContextType.Domain, domain);
            var principal = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, username);
            if (principal == null)
                return null;

            var entry = (DirectoryEntry)principal.GetUnderlyingObject();
            return MapEntry(principal, entry);
        }

        /// <summary>
        /// AD user search for the activation grid.
        /// Empty search loads all enabled AD users (paged). Search filters within AD.
        /// Uses minimal properties and light mapping (no DirectoryEntry per row) for speed.
        /// </summary>
        public (List<AdUser> Users, int TotalCount) SearchUsers(string? searchTerm, int pageIndex, int pageSize)
        {
            var domain = _configuration["ActiveDirectory:Domain"];
            if (string.IsNullOrWhiteSpace(domain))
                throw new InvalidOperationException("Active Directory domain is not configured.");

            var results = new List<AdUser>();
            if (pageSize < 1) pageSize = 20;
            if (pageIndex < 0) pageIndex = 0;

            var filter = "(&(objectCategory=person)(objectClass=user)(!(userAccountControl:1.2.840.113556.1.4.803:=2))";
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = EscapeLdap(searchTerm.Trim());
                filter += $"(|(sAMAccountName=*{term}*)(displayName=*{term}*)(givenName=*{term}*)(sn=*{term}*)(mail=*{term}*)(cn=*{term}*))";
            }
            filter += ")";

            using var root = new DirectoryEntry($"LDAP://{domain}");
            using var searcher = new DirectorySearcher(root)
            {
                Filter = filter,
                // LDAP server-side paging — streams results instead of one giant dump
                PageSize = 200,
                SizeLimit = 0,
                SearchScope = SearchScope.Subtree,
                CacheResults = false
            };
            searcher.PropertiesToLoad.Clear();
            foreach (var p in SearchListProperties)
                searcher.PropertiesToLoad.Add(p);

            using var all = searcher.FindAll();
            var total = 0;
            var skip = pageIndex * pageSize;
            var taken = 0;
            var index = 0;

            foreach (SearchResult sr in all)
            {
                total++;
                if (index++ < skip) continue;
                if (taken >= pageSize)
                {
                    // Continue counting for accurate TotalCount without mapping extra rows
                    continue;
                }

                try
                {
                    results.Add(MapSearchResultLight(sr));
                    taken++;
                }
                catch
                {
                    /* skip bad entries */
                }
            }

            return (results, total);
        }

        /// <summary>Lightweight map for grid rows only — no DirectoryEntry, no groups.</summary>
        private static AdUser MapSearchResultLight(SearchResult sr)
        {
            static string P(SearchResult r, string name)
            {
                try
                {
                    if (r.Properties.Contains(name) && r.Properties[name].Count > 0)
                        return r.Properties[name][0]?.ToString()?.Trim() ?? string.Empty;
                }
                catch { }
                return string.Empty;
            }

            var uacStr = P(sr, "userAccountControl");
            var enabled = true;
            if (int.TryParse(uacStr, out var uac))
                enabled = (uac & 2) == 0;

            var mapped = new AdUser
            {
                Username = P(sr, "sAMAccountName"),
                DisplayName = P(sr, "displayName"),
                GivenName = P(sr, "givenName"),
                Surname = P(sr, "sn"),
                Email = P(sr, "mail"),
                Department = P(sr, "department"),
                Title = P(sr, "title"),
                Office = P(sr, "physicalDeliveryOfficeName"),
                EmployeeId = P(sr, "employeeID"),
                EmployeeNumber = P(sr, "employeeNumber"),
                AccountStatus = enabled ? "Enabled" : "Disabled"
            };
            NormalizeNames(mapped);
            return mapped;
        }

        private static AdUser MapEntry(UserPrincipal principal, DirectoryEntry entry)
        {
            static string Prop(DirectoryEntry e, string name)
            {
                try
                {
                    var v = e.Properties[name]?.Value;
                    return v?.ToString()?.Trim() ?? string.Empty;
                }
                catch { return string.Empty; }
            }

            static string PropMultiCn(DirectoryEntry e, string name)
            {
                try
                {
                    var props = e.Properties[name];
                    if (props == null || props.Count == 0) return string.Empty;
                    var list = new List<string>();
                    foreach (var item in props)
                    {
                        var s = item?.ToString() ?? "";
                        if (s.StartsWith("CN=", StringComparison.OrdinalIgnoreCase))
                        {
                            var end = s.IndexOf(',');
                            list.Add(end > 0 ? s.Substring(3, end - 3) : s.Substring(3));
                        }
                        else list.Add(s);
                    }
                    return string.Join("; ", list);
                }
                catch { return string.Empty; }
            }

            var uacStr = Prop(entry, "userAccountControl");
            var enabled = true;
            if (int.TryParse(uacStr, out var uac))
                enabled = (uac & 2) == 0;

            var managerDn = Prop(entry, "manager");
            var managerName = "";
            if (!string.IsNullOrEmpty(managerDn) && managerDn.StartsWith("CN=", StringComparison.OrdinalIgnoreCase))
            {
                var end = managerDn.IndexOf(',');
                managerName = end > 0 ? managerDn.Substring(3, end - 3) : managerDn.Substring(3);
            }

            var mapped = new AdUser
            {
                Username = principal.SamAccountName ?? Prop(entry, "sAMAccountName"),
                UserPrincipalName = principal.UserPrincipalName ?? Prop(entry, "userPrincipalName"),
                Cn = Prop(entry, "cn"),
                DistinguishedName = entry.Path?.Replace("LDAP://", "") ?? Prop(entry, "distinguishedName"),
                DisplayName = principal.DisplayName ?? Prop(entry, "displayName"),
                GivenName = principal.GivenName ?? Prop(entry, "givenName"),
                Surname = principal.Surname ?? Prop(entry, "sn"),
                Initials = Prop(entry, "initials"),
                Email = principal.EmailAddress ?? Prop(entry, "mail"),
                Telephone = principal.VoiceTelephoneNumber ?? Prop(entry, "telephoneNumber"),
                Mobile = Prop(entry, "mobile"),
                IpPhone = Prop(entry, "ipPhone"),
                Fax = Prop(entry, "facsimileTelephoneNumber"),
                Title = Prop(entry, "title"),
                Department = Prop(entry, "department"),
                Company = Prop(entry, "company"),
                Office = Prop(entry, "physicalDeliveryOfficeName"),
                Description = Prop(entry, "description"),
                Manager = managerName,
                ManagerDn = managerDn,
                StreetAddress = Prop(entry, "streetAddress"),
                City = Prop(entry, "l"),
                State = Prop(entry, "st"),
                PostalCode = Prop(entry, "postalCode"),
                Country = !string.IsNullOrEmpty(Prop(entry, "co")) ? Prop(entry, "co") : Prop(entry, "c"),
                EmployeeId = Prop(entry, "employeeID"),
                EmployeeNumber = Prop(entry, "employeeNumber"),
                EmployeeType = Prop(entry, "employeeType"),
                ExtensionAttribute1 = Prop(entry, "extensionAttribute1"),
                ExtensionAttribute2 = Prop(entry, "extensionAttribute2"),
                ExtensionAttribute3 = Prop(entry, "extensionAttribute3"),
                ExtensionAttribute4 = Prop(entry, "extensionAttribute4"),
                ExtensionAttribute5 = Prop(entry, "extensionAttribute5"),
                ExtensionAttribute6 = Prop(entry, "extensionAttribute6"),
                ExtensionAttribute7 = Prop(entry, "extensionAttribute7"),
                ExtensionAttribute8 = Prop(entry, "extensionAttribute8"),
                ExtensionAttribute9 = Prop(entry, "extensionAttribute9"),
                ExtensionAttribute10 = Prop(entry, "extensionAttribute10"),
                ExtensionAttribute11 = Prop(entry, "extensionAttribute11"),
                ExtensionAttribute12 = Prop(entry, "extensionAttribute12"),
                ExtensionAttribute13 = Prop(entry, "extensionAttribute13"),
                ExtensionAttribute14 = Prop(entry, "extensionAttribute14"),
                ExtensionAttribute15 = Prop(entry, "extensionAttribute15"),
                AccountStatus = enabled ? "Enabled" : "Disabled",
                WhenCreated = FormatAdDate(Prop(entry, "whenCreated")),
                WhenChanged = FormatAdDate(Prop(entry, "whenChanged")),
                LogonCount = int.TryParse(Prop(entry, "logonCount"), out var lc) ? lc : 0,
                LastLogon = FormatFileTime(Prop(entry, "lastLogon")),
                Groups = PropMultiCn(entry, "memberOf")
            };
            NormalizeNames(mapped);
            return mapped;
        }

        private static void NormalizeNames(AdUser u)
        {
            static bool LooksPlaceholder(string? s)
            {
                if (string.IsNullOrWhiteSpace(s)) return true;
                var t = s.Trim();
                if (t.All(char.IsDigit)) return true;
                if (t.Contains("intern", StringComparison.OrdinalIgnoreCase)) return true;
                if (t.StartsWith("DOH ", StringComparison.OrdinalIgnoreCase)) return true;
                if (!t.Contains(' ') && t.Length <= 6 && t == t.ToUpperInvariant()) return true;
                return false;
            }

            if (string.IsNullOrWhiteSpace(u.DisplayName)) return;

            var parts = u.DisplayName.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2) return;

            var givenBad = LooksPlaceholder(u.GivenName);
            var snBad = LooksPlaceholder(u.Surname);

            if (givenBad || snBad)
            {
                u.Surname = parts[^1];
                u.GivenName = string.Join(" ", parts.Take(parts.Length - 1));
            }

            if (!string.IsNullOrWhiteSpace(u.Title)
                && string.Equals(u.Title.Trim(), u.DisplayName.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                u.Title = string.Empty;
            }
        }

        private static string FormatAdDate(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return string.Empty;
            if (DateTime.TryParse(raw, System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal,
                    out var dt)
                || DateTime.TryParse(raw, out dt))
            {
                try
                {
                    var sa = TimeZoneInfo.FindSystemTimeZoneById(
                        OperatingSystem.IsWindows() ? "South Africa Standard Time" : "Africa/Johannesburg");
                    dt = TimeZoneInfo.ConvertTime(dt.Kind == DateTimeKind.Unspecified
                        ? DateTime.SpecifyKind(dt, DateTimeKind.Utc) : dt.ToUniversalTime(), sa);
                }
                catch { }
                return dt.ToString("dd MMM yyyy HH:mm:ss");
            }
            return raw;
        }

        private static string FormatFileTime(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw) || raw == "0") return string.Empty;
            try
            {
                if (long.TryParse(raw, out var ft) && ft > 0)
                {
                    var dt = DateTime.FromFileTime(ft);
                    if (dt.Year > 1601)
                        return dt.ToString("dd MMM yyyy HH:mm:ss");
                }
            }
            catch { }
            return raw;
        }

        private static string EscapeLdap(string input)
        {
            var sb = new StringBuilder();
            foreach (var c in input)
            {
                switch (c)
                {
                    case '\\': sb.Append("\\5c"); break;
                    case '*': sb.Append("\\2a"); break;
                    case '(': sb.Append("\\28"); break;
                    case ')': sb.Append("\\29"); break;
                    case '\0': sb.Append("\\00"); break;
                    case '/': sb.Append("\\2f"); break;
                    default: sb.Append(c); break;
                }
            }
            return sb.ToString();
        }
    }
}
