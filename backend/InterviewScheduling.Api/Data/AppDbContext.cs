using InterviewScheduling.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace InterviewScheduling.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<TeacherOffering> TeacherOfferings => Set<TeacherOffering>();
    public DbSet<WeeklyAvailability> WeeklyAvailabilities => Set<WeeklyAvailability>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<SchoolSettings> SchoolSettings => Set<SchoolSettings>();
    public DbSet<TeacherAccessRequest> TeacherAccessRequests => Set<TeacherAccessRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.GoogleSub).IsUnique();
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.Email).HasMaxLength(320);
            e.Property(x => x.DisplayName).HasMaxLength(200);
            e.Property(x => x.GoogleSub).HasMaxLength(128);
            e.Property(x => x.PasswordHash).HasMaxLength(200);
            e.Property(x => x.StudentSchoolEmail).HasMaxLength(320);
            e.Property(x => x.InterviewAttendeeName).HasMaxLength(200);
            e.Property(x => x.RelationshipToStudent).HasMaxLength(120);
        });

        modelBuilder.Entity<Subject>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Code).IsUnique();
            e.Property(x => x.Code).HasMaxLength(32);
            e.Property(x => x.Name).HasMaxLength(160);
        });

        modelBuilder.Entity<TeacherOffering>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.CourseTitle).HasMaxLength(200);
            e.Property(x => x.GradeLevel).HasMaxLength(64);
            e.Property(x => x.SectionLabel).HasMaxLength(64);
            e.HasOne(x => x.Teacher).WithMany().HasForeignKey(x => x.TeacherUserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Subject).WithMany().HasForeignKey(x => x.SubjectId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TeacherAccessRequest>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Message).HasMaxLength(2000);
            e.HasOne(x => x.Applicant).WithMany().HasForeignKey(x => x.ApplicantUserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.DecidedBy).WithMany().HasForeignKey(x => x.DecidedByUserId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<WeeklyAvailability>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.TeacherOffering).WithMany(x => x.WeeklyAvailabilities).HasForeignKey(x => x.TeacherOfferingId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Booking>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.StudentSchoolEmail).HasMaxLength(320);
            e.Property(x => x.InterviewAttendeeName).HasMaxLength(200);
            e.Property(x => x.RelationshipToStudent).HasMaxLength(120);
            e.HasIndex(x => new { x.TeacherOfferingId, x.StartUtc }).IsUnique()
                .HasFilter("\"CancelledAt\" IS NULL");
            e.HasOne(x => x.TeacherOffering).WithMany(x => x.Bookings).HasForeignKey(x => x.TeacherOfferingId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Parent).WithMany().HasForeignKey(x => x.ParentUserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SchoolSettings>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.SchoolTimeZoneId).HasMaxLength(128);
        });
    }
}
