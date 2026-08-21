# Exit Interview Form System (Rebuilt)

ASP.NET Core 8 MVC rebuild of the KZN Health Exit Interview Form System.

## Key updates in this version

1. **District → Institutions flow (legacy style)**  
   - District list shows *District Name* with an **Institutions** button per row.  
   - Clicking Institutions opens institutions for that district (header shows district name).  
   - Dual-card Hub page is removed (redirects to District list).

2. **Pagination on data-heavy pages**  
   Shared pager control: `Page « < 1 > » of N` · `Records X to Y of Z`  
   Applied to Districts, Institutions, Financial Years (and ready for other lists).

3. **Clear date/time format**  
   - Culture: `en-ZA` (South Africa).  
   - AD timestamps shown as **`dd MMM yyyy HH:mm:ss`** (e.g. `13 Aug 2026 05:47:55`) so “8/13/2026” is never mistaken for a 13th month.  
   - Application clock uses **SAST (UTC+2)** via `AppTime.Now`.

4. **User Activation name cleanup**  
   When AD `givenName` / `sn` look like placeholders (intern accounts, org codes), names are derived from `displayName`.  
   Title is cleared when it was incorrectly set to the display name.

5. **User Levels page**  
   Matches the legacy screen: User Level ID, name, action icons (view / edit / copy / delete / permissions).

6. **User Level Permissions matrix**  
   Full table of modules with Add/Copy, Delete, Edit, List, View, Search checkboxes.

7. **Financial Years**  
   List with toolbar, search, pager, and action icons (legacy style).

## Run

```bash
cd ExitInterviewSystem
dotnet restore
dotnet run
```

Configure SQL Server connection and AD domain in `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=...;Database=ExitInterviewDB;..."
},
"ActiveDirectory": {
  "Domain": "KZNHEALTH"
}
```

Requires network access to the domain controller for AD login and User Activation.

## Tech

- .NET 8, ASP.NET Core MVC, Identity, EF Core SQL Server  
- System.DirectoryServices for Active Directory  
