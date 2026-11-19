using EquipmentBorrowing.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EquipmentBorrowing.Infrastructure.Persistence.Configurations;

public class BorrowingConfiguration : IEntityTypeConfiguration<Borrowing>
{
    public void Configure(EntityTypeBuilder<Borrowing> builder)
    {
        builder.ToTable("Borrowings");
        builder.HasKey(b => b.Id);

        builder.Property(b => b.DateBorrowed).IsRequired();
        builder.Property(b => b.ExpectedReturnDate).IsRequired();
        builder.Property(b => b.ActualReturnDate);
        builder.Property(b => b.Status).IsRequired();

        // Foreign keys with referential integrity (BR-14, BR-16)
        builder.HasOne(b => b.Student)
            .WithMany()
            .HasForeignKey(b => b.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.Equipment)
            .WithMany()
            .HasForeignKey(b => b.EquipmentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Database constraint: Prevent more than one active borrowing for the same item (BR-10, BR-16)
        builder.HasIndex(b => b.EquipmentId)
            .IsUnique()
            .HasFilter("\"Status\" = 0"); // 0 = BorrowingStatus.Active
    }
}