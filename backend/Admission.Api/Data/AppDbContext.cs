using Admission.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Admission.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<SchoolClass> Classes => Set<SchoolClass>();
    public DbSet<Hold> Holds => Set<Hold>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SchoolClass>(entity =>
        {
            entity.ToTable("classes", t => t.HasCheckConstraint("ck_classes_total_seats", "total_seats > 0"));
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Id).HasColumnName("id").HasMaxLength(32);
            entity.Property(c => c.Name).HasColumnName("name").HasMaxLength(100);
            entity.Property(c => c.NormalizedName).HasColumnName("normalized_name").HasMaxLength(100);
            entity.Property(c => c.TotalSeats).HasColumnName("total_seats");
            entity.Property(c => c.CreatedAt).HasColumnName("created_at");
            entity.HasIndex(c => c.NormalizedName).IsUnique();
        });

        modelBuilder.Entity<Hold>(entity =>
        {
            entity.ToTable("holds");
            entity.HasKey(h => h.Id);
            entity.Property(h => h.Id).HasColumnName("id").HasMaxLength(32);
            entity.Property(h => h.ClassId).HasColumnName("class_id").HasMaxLength(32);
            entity.Property(h => h.StudentId).HasColumnName("student_id").HasMaxLength(17);
            entity.Property(h => h.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(16);
            entity.Property(h => h.CreatedAt).HasColumnName("created_at");
            entity.Property(h => h.ExpiresAt).HasColumnName("expires_at");
            entity.Property(h => h.ConfirmedAt).HasColumnName("confirmed_at");
            entity.Property(h => h.CancelledAt).HasColumnName("cancelled_at");

            entity.HasOne(h => h.Class)
                .WithMany(c => c.Holds)
                .HasForeignKey(h => h.ClassId);

            // A student can have only one active (HELD/CONFIRMED) application per class.
            entity.HasIndex(h => new { h.ClassId, h.StudentId })
                .IsUnique()
                .HasFilter("status IN ('HELD', 'CONFIRMED')")
                .HasDatabaseName("ux_holds_one_active_per_student");

            entity.HasIndex(h => new { h.ClassId, h.Status });
            entity.HasIndex(h => h.StudentId);
        });
    }
}
