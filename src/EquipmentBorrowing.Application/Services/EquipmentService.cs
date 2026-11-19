using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Services;

public class EquipmentService
{
    private readonly IEquipmentRepository _equipmentRepository;

    public EquipmentService(IEquipmentRepository equipmentRepository)
    {
        _equipmentRepository = equipmentRepository;
    }

    public Task<List<Equipment>> GetAllEquipmentAsync(CancellationToken cancellationToken = default)
    {
        return _equipmentRepository.GetAllAsync(cancellationToken);
    }

    public async Task<ServiceResult> RegisterEquipmentAsync(
        string identifier, string name, string? description,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            return ServiceResult.Fail("Equipment identifier cannot be blank.");
        if (string.IsNullOrWhiteSpace(name))
            return ServiceResult.Fail("Equipment name cannot be blank.");

        var existing = await _equipmentRepository.GetByIdentifierAsync(identifier, cancellationToken);
        if (existing is not null)
            return ServiceResult.Fail($"An equipment item with identifier '{identifier.Trim()}' already exists.");

        // Id 0 lets the database generate the real Id. Newly registered equipment is available by default (BR-06).
        var equipment = new Equipment(0, identifier, name, description, isAvailable: true);
        await _equipmentRepository.AddAsync(equipment, cancellationToken);

        return ServiceResult.Ok($"Equipment '{equipment.Name}' ({equipment.Identifier}) registered successfully.");
    }

    public async Task<ServiceResult> UpdateEquipmentAsync(
        int id, string identifier, string name, string? description,
        CancellationToken cancellationToken = default)
    {
        var equipment = await _equipmentRepository.GetByIdAsync(id, cancellationToken);
        if (equipment is null)
            return ServiceResult.Fail("Equipment not found.");

        if (string.IsNullOrWhiteSpace(identifier))
            return ServiceResult.Fail("Equipment identifier cannot be blank.");
        if (string.IsNullOrWhiteSpace(name))
            return ServiceResult.Fail("Equipment name cannot be blank.");

        var duplicate = await _equipmentRepository.GetByIdentifierAsync(identifier, cancellationToken);
        if (duplicate is not null && duplicate.Id != id)
            return ServiceResult.Fail($"Another equipment item already uses identifier '{identifier.Trim()}'.");

        // Descriptive updates only, preserving availability and borrowing history (BR-07)
        equipment.UpdateDescriptiveInfo(identifier, name, description);
        await _equipmentRepository.UpdateAsync(equipment, cancellationToken);

        return ServiceResult.Ok($"Equipment '{equipment.Name}' details updated.");
    }
}
