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
        var students = new List<Student> { new(1, "Juan Dela Cruz", isAllowedToBorrow: true, maxActiveBorrowings: 2) };
        var equipment = new List<Equipment> { new(100, "Multimeter", isAvailable: true) };
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
        var equipment = new List<Equipment> { new(100, "Multimeter", isAvailable: true) };
        var (service, _) = CreateService(students, equipment);

        var result = await service.ExecuteAsync(studentId: 999, equipmentId: 100, expectedReturnDate: DateTime.UtcNow.AddDays(7));

        Assert.False(result.Success);
        Assert.Contains("does not exist", result.ErrorMessage);
    }

    [Fact]
    public async Task ExecuteAsync_StudentNotAllowedToBorrow_ReturnsFailure()
    {
        var students = new List<Student> { new(1, "Maria Santos", isAllowedToBorrow: false, maxActiveBorrowings: 2) };
        var equipment = new List<Equipment> { new(100, "Multimeter", isAvailable: true) };
        var (service, _) = CreateService(students, equipment);

        var result = await service.ExecuteAsync(studentId: 1, equipmentId: 100, expectedReturnDate: DateTime.UtcNow.AddDays(7));

        Assert.False(result.Success);
        Assert.Contains("not currently allowed", result.ErrorMessage);
    }

    [Fact]
    public async Task ExecuteAsync_EquipmentDoesNotExist_ReturnsFailure()
    {
        var students = new List<Student> { new(1, "Juan Dela Cruz", isAllowedToBorrow: true, maxActiveBorrowings: 2) };
        var equipment = new List<Equipment>();
        var (service, _) = CreateService(students, equipment);

        var result = await service.ExecuteAsync(studentId: 1, equipmentId: 999, expectedReturnDate: DateTime.UtcNow.AddDays(7));

        Assert.False(result.Success);
        Assert.Contains("does not exist", result.ErrorMessage);
    }

    [Fact]
    public async Task ExecuteAsync_EquipmentUnavailable_ReturnsFailure()
    {
        var students = new List<Student> { new(1, "Juan Dela Cruz", isAllowedToBorrow: true, maxActiveBorrowings: 2) };
        var equipment = new List<Equipment> { new(100, "Multimeter", isAvailable: false) };
        var (service, _) = CreateService(students, equipment);

        var result = await service.ExecuteAsync(studentId: 1, equipmentId: 100, expectedReturnDate: DateTime.UtcNow.AddDays(7));

        Assert.False(result.Success);
        Assert.Contains("not currently available", result.ErrorMessage);
    }

    [Fact]
    public async Task ExecuteAsync_StudentAtMaxActiveBorrowings_ReturnsFailure()
    {
        // Student allows only 1 active borrowing.
        var students = new List<Student> { new(1, "Juan Dela Cruz", isAllowedToBorrow: true, maxActiveBorrowings: 1) };
        var equipment = new List<Equipment>
        {
            new(100, "Multimeter", isAvailable: true),
            new(101, "Oscilloscope", isAvailable: true),
        };
        var (service, _) = CreateService(students, equipment);

        // First borrow succeeds and uses up the limit.
        var first = await service.ExecuteAsync(studentId: 1, equipmentId: 100, expectedReturnDate: DateTime.UtcNow.AddDays(7));
        Assert.True(first.Success);

        // Second borrow should fail — student already has 1 active borrowing (their max).
        var second = await service.ExecuteAsync(studentId: 1, equipmentId: 101, expectedReturnDate: DateTime.UtcNow.AddDays(7));

        Assert.False(second.Success);
        Assert.Contains("maximum", second.ErrorMessage);
    }
}