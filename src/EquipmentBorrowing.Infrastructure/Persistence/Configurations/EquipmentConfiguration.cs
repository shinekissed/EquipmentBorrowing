using EquipmentBorrowing.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EquipmentBorrowing.Infrastructure.Persistence.Configurations;

public class EquipmentConfiguration : IEntityTypeConfiguration<Equipment>
{
    public void Configure(EntityTypeBuilder<Equipment> builder)
    {
        builder.ToTable("Equipment");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Identifier)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Description)
            .HasMaxLength(255);

        builder.Property(e => e.IsAvailable)
            .IsRequired();

        // Enforce uniqueness in database (BR-05, BR-16)
        builder.HasIndex(e => e.Identifier)
            .IsUnique();

        // Initial Seed Data (BR-06)
        builder.HasData(
            new Equipment(100, "EQ-100", "Digital Multimeter", "Handheld digital multimeter", isAvailable: true),
            new Equipment(101, "EQ-101", "Digital Oscilloscope", "100MHz 2-channel oscilloscope", isAvailable: false),
            new Equipment(102, "EQ-102", "Soldering Station Kit", "Adjustable temperature station", isAvailable: true),
            new Equipment(103, "EQ-103", "DC Power Supply (30V/5A)", "Regulated variable DC power bench", isAvailable: true),
            new Equipment(104, "EQ-104", "Function Generator", "DDS signal generator", isAvailable: false)
        );
    }
}