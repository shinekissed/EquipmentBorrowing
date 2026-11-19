using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Repositories;
using Xunit;

namespace EquipmentBorrowing.Tests;

public class StudentServiceTests
{
    [Fact]
    public async Task RegisterStudentAsync_ValidData_ReturnsSuccess()
    {
        var repo = new InMemoryStudentRepository();
        var service = new StudentService(repo);

        var result = await service.RegisterStudentAsync("2026-0001", "Juan Dela Cruz", "juan@univ.edu");

        Assert.True(result.Success);
        var created = await repo.GetByStudentNumberAsync("2026-0001");
        Assert.NotNull(created);
        Assert.Equal("Juan Dela Cruz", created.Name);
        Assert.True(created.IsEligible); // BR-02: newly registered student is eligible
    }

    [Fact]
    public async Task RegisterStudentAsync_BlankRequiredFields_Fails()
    {
        var repo = new InMemoryStudentRepository();
        var service = new StudentService(repo);

        var res1 = await service.RegisterStudentAsync("", "Juan", null);
        var res2 = await service.RegisterStudentAsync("2026-0001", "", null);

        Assert.False(res1.Success);
        Assert.False(res2.Success);
    }

    [Fact]
    public async Task RegisterStudentAsync_DuplicateStudentNumber_Fails()
    {
        var existing = new Student(1, "2026-0001", "Existing Student");
        var repo = new InMemoryStudentRepository(new[] { existing });
        var service = new StudentService(repo);

        var result = await service.RegisterStudentAsync("2026-0001", "New Student", null);

        Assert.False(result.Success);
        Assert.Contains("already exists", result.Message);
    }

    [Fact]
    public async Task UpdateStudentAsync_ValidUpdate_UpdatesDetails()
    {
        var student = new Student(1, "2026-0001", "Old Name", "old@univ.edu");
        var repo = new InMemoryStudentRepository(new[] { student });
        var service = new StudentService(repo);

        var result = await service.UpdateStudentAsync(1, "2026-0001", "New Name", null);

        Assert.True(result.Success);
        var updated = await repo.GetByIdAsync(1);
        Assert.NotNull(updated);
        Assert.Equal("New Name", updated.Name);
        Assert.Null(updated.ContactInfo); // BR-03: blank optional contact clears it
    }

    [Fact]
    public async Task ChangeEligibilityAsync_RestrictAndPermit_UpdatesStatus()
    {
        var student = new Student(1, "2026-0001", "Juan", isEligible: true);
        var repo = new InMemoryStudentRepository(new[] { student });
        var service = new StudentService(repo);

        var restrictResult = await service.ChangeEligibilityAsync(1, false);
        Assert.True(restrictResult.Success);
        Assert.False(student.IsEligible);

        var permitResult = await service.ChangeEligibilityAsync(1, true);
        Assert.True(permitResult.Success);
        Assert.True(student.IsEligible);
    }
}
