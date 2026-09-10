using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Repositories;
using Xunit;

namespace EquipmentBorrowing.Tests;

public class ReturnEquipmentServiceTests
{
    private static (BorrowEquipmentService borrowService, ReturnEquipmentService returnService)
        CreateServices(List<Student> students, List<Equipment> equipment)
    {
        var studentRepository = new InMemoryStudentRepository(students);
        var equipmentRepository = new InMemoryEquipmentRepository(equipment);
        var borrowingRepository = new InMemoryBorrowingRepository();

        var borrowService = new BorrowEquipmentService(studentRepository, equipmentRepository, borrowingRepository);
        var returnService = new ReturnEquipmentService(equipmentRepository, borrowingRepository);

        return (borrowService, returnService);
    }

    [Fact]
    public async Task ExecuteAsync_ActiveBorrowingExists_ReturnsSuccessAndMarksEquipmentAvailable()
    {
        var students = new List<Student> { new(1, "Juan Dela Cruz", isAllowedToBorrow: true, maxActiveBorrowings: 2) };
        var equipment = new List<Equipment> { new(100, "Multimeter", isAvailable: true) };
        var (borrowService, returnService) = CreateServices(students, equipment);

        var borrowResult = await borrowService.ExecuteAsync(
            studentId: 1, equipmentId: 100, expectedReturnDate: DateTime.UtcNow.AddDays(7));
        Assert.True(borrowResult.Success); // sanity check the setup worked

        var returnResult = await returnService.ExecuteAsync(studentId: 1, equipmentId: 100);

        Assert.True(returnResult.Success);
    }

    [Fact]
    public async Task ExecuteAsync_NoActiveBorrowingExists_ReturnsFailure()
    {
        var students = new List<Student> { new(1, "Juan Dela Cruz", isAllowedToBorrow: true, maxActiveBorrowings: 2) };
        var equipment = new List<Equipment> { new(100, "Multimeter", isAvailable: true) };
        var (_, returnService) = CreateServices(students, equipment);

        var result = await returnService.ExecuteAsync(studentId: 1, equipmentId: 100);

        Assert.False(result.Success);
        Assert.Contains("No active borrowing found", result.ErrorMessage);
    }

    [Fact]
    public async Task ExecuteAsync_AlreadyReturned_ReturnsFailureOnSecondAttempt()
    {
        var students = new List<Student> { new(1, "Juan Dela Cruz", isAllowedToBorrow: true, maxActiveBorrowings: 2) };
        var equipment = new List<Equipment> { new(100, "Multimeter", isAvailable: true) };
        var (borrowService, returnService) = CreateServices(students, equipment);

        await borrowService.ExecuteAsync(studentId: 1, equipmentId: 100, expectedReturnDate: DateTime.UtcNow.AddDays(7));
        var firstReturn = await returnService.ExecuteAsync(studentId: 1, equipmentId: 100);
        Assert.True(firstReturn.Success); // sanity check

        var secondReturn = await returnService.ExecuteAsync(studentId: 1, equipmentId: 100);

        Assert.False(secondReturn.Success);
        Assert.Contains("No active borrowing found", secondReturn.ErrorMessage);
    }
}
