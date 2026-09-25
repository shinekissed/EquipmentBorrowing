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

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.IsAllowedToBorrow).IsRequired();
        builder.Property(s => s.MaxActiveBorrowings).IsRequired();

        // Initial Seed Data (Part L)
        builder.HasData(
            new Student(1, "Juan Dela Cruz", isAllowedToBorrow: true, maxActiveBorrowings: 2),
            new Student(2, "Maria Santos (Suspended)", isAllowedToBorrow: false, maxActiveBorrowings: 2),
            new Student(3, "Pedro Penduko", isAllowedToBorrow: true, maxActiveBorrowings: 2)
        );
    }
}