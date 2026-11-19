using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Repositories;
using Xunit;

namespace EquipmentBorrowing.Tests;

public class BorrowEquipmentServiceTests
{
    // Helper to build a fresh service + repos for each test, so tests don't
    // interfere with each other (each test gets its own in-memory data).
    private static (BorrowEquipmentService service, InMemoryBorrowingRepository borrowingRepo)
        CreateService(List<Student> students, List<Equipment> equipment)
    {
        var studentRepository = new InMemoryStudentRepository(students);
        var equipmentRepository = new InMemoryEquipmentRepository(equipment);
        var borrowingRepository = new InMemoryBorrowingRepository();

        var service = new BorrowEquipmentService(studentRepository, equipmentRepository, borrowingRepository);
        return (service, borrowingRepository);
    }

    [Fact]
    public async Task ExecuteAsync_ValidRequest_ReturnsSuccessAndCreatesBorrowing()
    {
        // Arrange
        var students = new List<Student> { new(1, "2026-001", "Juan Dela Cruz", isEligible: true) };
        var equipment = new List<Equipment> { new(100, "EQ-100", "Multimeter", isAvailable: true) };
        var (service, _) = CreateService(students, equipment);

        // Act
        var result = await service.ExecuteAsync(studentId: 1, equipmentId: 100, expectedReturnDate: DateTime.UtcNow.AddDays(7));

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.BorrowingId);
    }

    [Fact]
    public async Task ExecuteAsync_StudentDoesNotExist_ReturnsFailure()
    {
        var students = new List<Student>();
        var equipment = new List<Equipment> { new(100, "EQ-100", "Multimeter", isAvailable: true) };
        var (service, _) = CreateService(students, equipment);

        var result = await service.ExecuteAsync(studentId: 999, equipmentId: 100, expectedReturnDate: DateTime.UtcNow.AddDays(7));

        Assert.False(result.Success);
        Assert.Contains("does not exist", result.ErrorMessage);
    }

    [Fact]
    public async Task ExecuteAsync_StudentNotAllowedToBorrow_ReturnsFailure()
    {
        var students = new List<Student> { new(1, "2026-002", "Maria Santos", isEligible: false) };
        var equipment = new List<Equipment> { new(100, "EQ-100", "Multimeter", isAvailable: true) };
        var (service, _) = CreateService(students, equipment);

        var result = await service.ExecuteAsync(studentId: 1, equipmentId: 100, expectedReturnDate: DateTime.UtcNow.AddDays(7));

        Assert.False(result.Success);
        Assert.Contains("not currently allowed", result.ErrorMessage);
    }

    [Fact]
    public async Task ExecuteAsync_EquipmentDoesNotExist_ReturnsFailure()
    {
        var students = new List<Student> { new(1, "2026-001", "Juan Dela Cruz", isEligible: true) };
        var equipment = new List<Equipment>();
        var (service, _) = CreateService(students, equipment);

        var result = await service.ExecuteAsync(studentId: 1, equipmentId: 999, expectedReturnDate: DateTime.UtcNow.AddDays(7));

        Assert.False(result.Success);
        Assert.Contains("does not exist", result.ErrorMessage);
    }

    [Fact]
    public async Task ExecuteAsync_EquipmentUnavailable_ReturnsFailure()
    {
        var students = new List<Student> { new(1, "2026-001", "Juan Dela Cruz", isEligible: true) };
        var equipment = new List<Equipment> { new(100, "EQ-100", "Multimeter", isAvailable: false) };
        var (service, _) = CreateService(students, equipment);

        var result = await service.ExecuteAsync(studentId: 1, equipmentId: 100, expectedReturnDate: DateTime.UtcNow.AddDays(7));

        Assert.False(result.Success);
        Assert.Contains("not currently available", result.ErrorMessage);
    }

    [Fact]
    public async Task ExecuteAsync_StudentAtMaxActiveBorrowings_ReturnsFailure()
    {
        // Student.MaxActiveBorrowings is fixed at 3, so borrow 3 items, then try a 4th.
        var students = new List<Student> { new(1, "2026-001", "Juan Dela Cruz", isEligible: true) };
        var equipment = new List<Equipment>
        {
            new(100, "EQ-100", "Multimeter", isAvailable: true),
            new(101, "EQ-101", "Oscilloscope", isAvailable: true),
            new(102, "EQ-102", "Soldering Iron", isAvailable: true),
            new(103, "EQ-103", "Breadboard", isAvailable: true),
        };
        var (service, _) = CreateService(students, equipment);

        for (var id = 100; id <= 102; id++)
        {
            var ok = await service.ExecuteAsync(studentId: 1, equipmentId: id, expectedReturnDate: DateTime.UtcNow.AddDays(7));
            Assert.True(ok.Success);
        }

        // The 4th borrow should fail because the student is at their maximum.
        var fourth = await service.ExecuteAsync(studentId: 1, equipmentId: 103, expectedReturnDate: DateTime.UtcNow.AddDays(7));

        Assert.False(fourth.Success);
        Assert.Contains("maximum", fourth.ErrorMessage);
    }
}