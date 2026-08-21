using ExitInterviewSystem.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ExitInterviewSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<FinancialYear> FinancialYears { get; set; }
        public DbSet<UserLevelPermission> UserLevelPermissions { get; set; }
        public DbSet<District> Districts { get; set; }
        public DbSet<Institution> Institutions { get; set; }
        public DbSet<Termination> Terminations { get; set; }
        public DbSet<TerminationType> TerminationTypes { get; set; }
        public DbSet<ExitInterviewForm> ExitInterviewForms { get; set; }
        public DbSet<AuditTrail> AuditTrails { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<FinancialYear>().ToTable("FinancialYears");
            builder.Entity<District>().ToTable("Districts");
            builder.Entity<Institution>().ToTable("Institutions");
            builder.Entity<Termination>().ToTable("Terminations");
            builder.Entity<TerminationType>().ToTable("TerminationTypes");
            builder.Entity<ExitInterviewForm>().ToTable("ExitInterviewForms");
            builder.Entity<AuditTrail>().ToTable("AuditTrails");

            // Seed Financial Years
            builder.Entity<FinancialYear>().HasData(
                new FinancialYear { Id = 1, Name = "2022/23", IsActive = true },
                new FinancialYear { Id = 2, Name = "2023/24", IsActive = true },
                new FinancialYear { Id = 3, Name = "2024/25", IsActive = true },
                new FinancialYear { Id = 4, Name = "2025/26", IsActive = true }
            );

            // Seed Districts
            builder.Entity<District>().HasData(
                new District { Id = 1, Name = "eThekwini", Code = "ETH" },
                new District { Id = 2, Name = "uMgungundlovu", Code = "UMG" },
                new District { Id = 3, Name = "uThukela", Code = "UTH" },
                new District { Id = 4, Name = "Amajuba", Code = "AMA" },
                new District { Id = 5, Name = "Zululand", Code = "ZUL" },
                new District { Id = 6, Name = "uMkhanyakude", Code = "UMK" },
                new District { Id = 7, Name = "King Cetshwayo", Code = "KCE" },
                new District { Id = 8, Name = "iLembe", Code = "ILE" },
                new District { Id = 9, Name = "Harry Gwala", Code = "HGW" },
                new District { Id = 10, Name = "uMzinyathi", Code = "UMZ" },
                new District { Id = 11, Name = "Ugu", Code = "UGU" }
            );

            // Seed Institutions
            builder.Entity<Institution>().HasData(
                new Institution { Id = 1, Name = "Inkosi Albert Luthuli Central Hospital", DistrictId = 1, InstitutionType = "Hospital" },
                new Institution { Id = 2, Name = "King Edward VIII Hospital", DistrictId = 1, InstitutionType = "Hospital" },
                new Institution { Id = 3, Name = "Grey's Hospital", DistrictId = 2, InstitutionType = "Hospital" },
                new Institution { Id = 4, Name = "Edendale Hospital", DistrictId = 2, InstitutionType = "Hospital" },
                new Institution { Id = 5, Name = "Ladysmith Hospital", DistrictId = 3, InstitutionType = "Hospital" },
                new Institution { Id = 6, Name = "Madadeni Hospital", DistrictId = 4, InstitutionType = "Hospital" },
                new Institution { Id = 7, Name = "Ngwelezana Hospital", DistrictId = 7, InstitutionType = "Hospital" },
                new Institution { Id = 8, Name = "Stanger Hospital", DistrictId = 8, InstitutionType = "Hospital" }
            );

            builder.Entity<TerminationType>().HasData(
                new TerminationType { Id = 1, Name = "Retirement (65 Years)" },
                new TerminationType { Id = 2, Name = "Retirement (60-65 Years)" },
                new TerminationType { Id = 3, Name = "Retirement (Below 60 Years)" },
                new TerminationType { Id = 4, Name = "Ill-Health Retirement/Medical Boarding/Injury" },
                new TerminationType { Id = 5, Name = "Transfer" },
                new TerminationType { Id = 6, Name = "Resignation" },
                new TerminationType { Id = 7, Name = "Contract Employment (Appointment interms of Public Service Act)" },
                new TerminationType { Id = 8, Name = "Deceased" }
            );

        }
    }
}
