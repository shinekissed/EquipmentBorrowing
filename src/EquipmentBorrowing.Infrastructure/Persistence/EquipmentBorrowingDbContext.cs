using EquipmentBorrowing.Domain;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowing.Infrastructure.Persistence;

public class EquipmentBorrowingDbContext : DbContext
{
    public const string DefaultConnectionString = "Host=localhost;Port=5432;Database=equipment_borrowing;Username=postgres;Password=postgres;";

    public DbSet<Student> Students => Set<Student>();
    public DbSet<Equipment> Equipment => Set<Equipment>();
    public DbSet<Borrowing> Borrowings => Set<Borrowing>();

    public EquipmentBorrowingDbContext() { }

    public EquipmentBorrowingDbContext(DbContextOptions<EquipmentBorrowingDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EquipmentBorrowingDbContext).Assembly);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var connStr = Environment.GetEnvironmentVariable("EQUIPMENT_BORROWING_DB_CONN") ?? DefaultConnectionString;
            optionsBuilder.UseNpgsql(connStr);
        }
    }
}