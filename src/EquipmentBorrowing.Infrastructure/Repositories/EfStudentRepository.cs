using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public class EfStudentRepository : IStudentRepository
{
    private readonly EquipmentBorrowingDbContext _context;

    public EfStudentRepository(EquipmentBorrowingDbContext context)
    {
        _context = context;
    }

    public async Task<List<Student>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Students
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Student?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Students
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<Student?> GetByStudentNumberAsync(string studentNumber, CancellationToken cancellationToken = default)
    {
        var normalized = studentNumber.Trim().ToLower();
        return await _context.Students
            .FirstOrDefaultAsync(s => s.StudentNumber.ToLower() == normalized, cancellationToken);
    }

    public async Task AddAsync(Student student, CancellationToken cancellationToken = default)
    {
        await _context.Students.AddAsync(student, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Student student, CancellationToken cancellationToken = default)
    {
        _context.Students.Update(student);
        await _context.SaveChangesAsync(cancellationToken);
    }
}