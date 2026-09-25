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

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.IsAvailable).IsRequired();

        // Initial Seed Data (Part L)
        builder.HasData(
            new Equipment(100, "Digital Multimeter", isAvailable: true),
            new Equipment(101, "Digital Oscilloscope", isAvailable: false),
            new Equipment(102, "Soldering Station Kit", isAvailable: true),
            new Equipment(103, "DC Power Supply (30V/5A)", isAvailable: true),
            new Equipment(104, "Function Generator", isAvailable: false)
        );
    }
}
