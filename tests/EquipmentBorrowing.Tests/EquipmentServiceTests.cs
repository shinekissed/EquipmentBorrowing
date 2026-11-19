using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Repositories;
using Xunit;

namespace EquipmentBorrowing.Tests;

public class EquipmentServiceTests
{
    [Fact]
    public async Task RegisterEquipmentAsync_ValidData_ReturnsSuccessAndAvailable()
    {
        var repo = new InMemoryEquipmentRepository();
        var service = new EquipmentService(repo);

        var result = await service.RegisterEquipmentAsync("EQ-999", "Oscilloscope", "Digital 100MHz");

        Assert.True(result.Success);
        var created = await repo.GetByIdentifierAsync("EQ-999");
        Assert.NotNull(created);
        Assert.Equal("Oscilloscope", created.Name);
        Assert.True(created.IsAvailable); // BR-06: newly registered equipment is available
    }

    [Fact]
    public async Task RegisterEquipmentAsync_BlankRequiredFields_Fails()
    {
        var repo = new InMemoryEquipmentRepository();
        var service = new EquipmentService(repo);

        var res1 = await service.RegisterEquipmentAsync("", "Oscilloscope", null);
        var res2 = await service.RegisterEquipmentAsync("EQ-999", "", null);

        Assert.False(res1.Success);
        Assert.False(res2.Success);
    }

    [Fact]
    public async Task RegisterEquipmentAsync_DuplicateIdentifier_Fails()
    {
        var existing = new Equipment(1, "EQ-001", "Existing Multimeter");
        var repo = new InMemoryEquipmentRepository(new[] { existing });
        var service = new EquipmentService(repo);

        var result = await service.RegisterEquipmentAsync("EQ-001", "New Multimeter", null);

        Assert.False(result.Success);
        Assert.Contains("already exists", result.Message);
    }

    [Fact]
    public async Task UpdateEquipmentAsync_ValidUpdate_PreservesAvailability()
    {
        var equipment = new Equipment(1, "EQ-001", "Old Name", "Old Desc", isAvailable: false);
        var repo = new InMemoryEquipmentRepository(new[] { equipment });
        var service = new EquipmentService(repo);

        var result = await service.UpdateEquipmentAsync(1, "EQ-001-MOD", "Updated Name", null);

        Assert.True(result.Success);
        var updated = await repo.GetByIdAsync(1);
        Assert.NotNull(updated);
        Assert.Equal("EQ-001-MOD", updated.Identifier);
        Assert.Equal("Updated Name", updated.Name);
        Assert.Null(updated.Description);
        Assert.False(updated.IsAvailable); // BR-07: availability remains intact through descriptive edits
    }

    [Fact]
    public async Task UpdateEquipmentAsync_DuplicateIdentifierOnAnotherItem_Fails()
    {
        var item1 = new Equipment(1, "EQ-001", "Item 1");
        var item2 = new Equipment(2, "EQ-002", "Item 2");
        var repo = new InMemoryEquipmentRepository(new[] { item1, item2 });
        var service = new EquipmentService(repo);

        var result = await service.UpdateEquipmentAsync(2, "EQ-001", "Item 2 Renamed", null);

        Assert.False(result.Success);
        Assert.Contains("already uses identifier", result.Message);
    }
}
