namespace EquipmentBorrowing.Domain;

public class Student
{
    public int Id { get; private set; }
    public string StudentNumber { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? ContactInfo { get; private set; }
    public bool IsEligible { get; private set; } = true;
    public bool IsAllowedToBorrow => IsEligible;
    public int MaxActiveBorrowings => 3;

    // Parameterless constructor for EF Core
    protected Student() { }

    public Student(int id, string studentNumber, string name, string? contactInfo = null, bool isEligible = true)
    {
        if (string.IsNullOrWhiteSpace(studentNumber))
            throw new ArgumentException("Student number cannot be blank.", nameof(studentNumber));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Student name cannot be blank.", nameof(name));

        Id = id;
        StudentNumber = studentNumber.Trim();
        Name = name.Trim();
        ContactInfo = string.IsNullOrWhiteSpace(contactInfo) ? null : contactInfo.Trim();
        IsEligible = isEligible;
    }

    public void UpdateInformation(string studentNumber, string name, string? contactInfo)
    {
        if (string.IsNullOrWhiteSpace(studentNumber))
            throw new ArgumentException("Student number cannot be blank.", nameof(studentNumber));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Student name cannot be blank.", nameof(name));

        StudentNumber = studentNumber.Trim();
        Name = name.Trim();
        ContactInfo = string.IsNullOrWhiteSpace(contactInfo) ? null : contactInfo.Trim();
    }

    public void Restrict() => IsEligible = false;
    public void Permit() => IsEligible = true;
}