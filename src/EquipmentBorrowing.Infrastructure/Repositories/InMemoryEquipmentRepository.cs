using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public class InMemoryEquipmentRepository : IEquipmentRepository
{
    private readonly Dictionary<int, Equipment> _equipment;

    public InMemoryEquipmentRepository(IEnumerable<Equipment>? seed = null)
    {
        _equipment = (seed ?? Enumerable.Empty<Equipment>())
            .ToDictionary(e => e.Id);
    }

    public Task<List<Equipment>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_equipment.Values.OrderBy(e => e.Name).ToList());
    }

    public Task<Equipment?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        _equipment.TryGetValue(id, out var equipment);
        return Task.FromResult(equipment);
    }

    public Task<Equipment?> GetByIdentifierAsync(string identifier, CancellationToken cancellationToken = default)
    {
        var normalized = identifier.Trim().ToLower();
        var equipment = _equipment.Values
            .FirstOrDefault(e => e.Identifier.ToLower() == normalized);
        return Task.FromResult(equipment);
    }

    public Task AddAsync(Equipment equipment, CancellationToken cancellationToken = default)
    {
        _equipment[equipment.Id] = equipment;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Equipment equipment, CancellationToken cancellationToken = default)
    {
        _equipment[equipment.Id] = equipment;
        return Task.CompletedTask;
    }
}