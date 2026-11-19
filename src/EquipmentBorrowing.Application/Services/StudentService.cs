using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Services;

public class StudentService
{
    private readonly IStudentRepository _studentRepository;

    public StudentService(IStudentRepository studentRepository)
    {
        _studentRepository = studentRepository;
    }

    public Task<List<Student>> GetAllStudentsAsync(CancellationToken cancellationToken = default)
    {
        return _studentRepository.GetAllAsync(cancellationToken);
    }

    public async Task<ServiceResult> RegisterStudentAsync(
        string studentNumber, string name, string? contactInfo,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(studentNumber))
            return ServiceResult.Fail("Student number cannot be blank.");
        if (string.IsNullOrWhiteSpace(name))
            return ServiceResult.Fail("Student name cannot be blank.");

        var existing = await _studentRepository.GetByStudentNumberAsync(studentNumber, cancellationToken);
        if (existing is not null)
            return ServiceResult.Fail($"A student with number '{studentNumber.Trim()}' already exists.");

        // Id 0 lets the database generate the real Id. New students are eligible by default.
        var student = new Student(0, studentNumber, name, contactInfo, isEligible: true);
        await _studentRepository.AddAsync(student, cancellationToken);

        return ServiceResult.Ok($"Student '{student.Name}' registered successfully.");
    }

    public async Task<ServiceResult> UpdateStudentAsync(
        int id, string studentNumber, string name, string? contactInfo,
        CancellationToken cancellationToken = default)
    {
        var student = await _studentRepository.GetByIdAsync(id, cancellationToken);
        if (student is null)
            return ServiceResult.Fail("Student not found.");

        if (string.IsNullOrWhiteSpace(studentNumber))
            return ServiceResult.Fail("Student number cannot be blank.");
        if (string.IsNullOrWhiteSpace(name))
            return ServiceResult.Fail("Student name cannot be blank.");

        var duplicate = await _studentRepository.GetByStudentNumberAsync(studentNumber, cancellationToken);
        if (duplicate is not null && duplicate.Id != id)
            return ServiceResult.Fail($"Another student already uses number '{studentNumber.Trim()}'.");

        student.UpdateInformation(studentNumber, name, contactInfo);
        await _studentRepository.UpdateAsync(student, cancellationToken);

        return ServiceResult.Ok("Student details updated.");
    }

    public async Task<ServiceResult> ChangeEligibilityAsync(
        int id, bool isEligible, CancellationToken cancellationToken = default)
    {
        var student = await _studentRepository.GetByIdAsync(id, cancellationToken);
        if (student is null)
            return ServiceResult.Fail("Student not found.");

        if (isEligible) student.Permit();
        else student.Restrict();

        await _studentRepository.UpdateAsync(student, cancellationToken);

        return ServiceResult.Ok(isEligible
            ? $"{student.Name} is now allowed to borrow."
            : $"{student.Name} is now restricted from borrowing.");
    }
}