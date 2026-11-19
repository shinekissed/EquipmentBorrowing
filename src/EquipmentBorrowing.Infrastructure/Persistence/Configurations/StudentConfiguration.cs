using EquipmentBorrowing.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EquipmentBorrowing.Infrastructure.Persistence.Configurations;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("Students");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.StudentNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.ContactInfo)
            .HasMaxLength(100);

        builder.Property(s => s.IsEligible)
            .IsRequired();

        // Enforce uniqueness in database (BR-01, BR-16)
        builder.HasIndex(s => s.StudentNumber)
            .IsUnique();

        // Initial Seed Data (Safe defaults, BR-02)
        builder.HasData(
            new Student(1, "STU-2026-0001", "Juan Dela Cruz", "09123456789", isEligible: true),
            new Student(2, "STU-2026-0002", "Maria Santos", null, isEligible: false),
            new Student(3, "STU-2026-0003", "Pedro Penduko", "pedro@sample.edu", isEligible: true)
        );
    }
}