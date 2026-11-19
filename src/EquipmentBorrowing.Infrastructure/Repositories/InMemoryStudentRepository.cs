using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public class InMemoryStudentRepository : IStudentRepository
{
    private readonly Dictionary<int, Student> _students;

    public InMemoryStudentRepository(IEnumerable<Student>? seed = null)
    {
        _students = (seed ?? Enumerable.Empty<Student>())
            .ToDictionary(s => s.Id);
    }

    public Task<List<Student>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_students.Values.OrderBy(s => s.Name).ToList());
    }

    public Task<Student?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        _students.TryGetValue(id, out var student);
        return Task.FromResult(student);
    }

    public Task<Student?> GetByStudentNumberAsync(string studentNumber, CancellationToken cancellationToken = default)
    {
        var normalized = studentNumber.Trim().ToLower();
        var student = _students.Values
            .FirstOrDefault(s => s.StudentNumber.ToLower() == normalized);
        return Task.FromResult(student);
    }

    public Task AddAsync(Student student, CancellationToken cancellationToken = default)
    {
        _students[student.Id] = student;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Student student, CancellationToken cancellationToken = default)
    {
        _students[student.Id] = student;
        return Task.CompletedTask;
    }
}