namespace EquipmentBorrowing.Domain;

public class Equipment
{
    public int Id { get; private set; }
    public string Identifier { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsAvailable { get; private set; } = true;

    // Parameterless constructor for EF Core
    protected Equipment() { }

    public Equipment(int id, string identifier, string name, string? description = null, bool isAvailable = true)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            throw new ArgumentException("Equipment identifier cannot be blank.", nameof(identifier));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Equipment name cannot be blank.", nameof(name));

        Id = id;
        Identifier = identifier.Trim();
        Name = name.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        IsAvailable = isAvailable;
    }

    public void UpdateDescriptiveInfo(string identifier, string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            throw new ArgumentException("Equipment identifier cannot be blank.", nameof(identifier));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Equipment name cannot be blank.", nameof(name));

        Identifier = identifier.Trim();
        Name = name.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
    }

    public void MarkAsBorrowed()
    {
        if (!IsAvailable)
            throw new InvalidOperationException($"Equipment '{Identifier}' is already borrowed.");
        IsAvailable = false;
    }

    public void MarkAsReturned()
    {
        IsAvailable = true;
    }
}