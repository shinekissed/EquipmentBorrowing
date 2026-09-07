namespace EquipmentBorrowing.Domain;

public class Student
{
    public int Id { get; }
    public string Name { get; }
    public bool IsAllowedToBorrow { get; private set; }
    public int MaxActiveBorrowings { get; }

    public Student(int id, string name, bool isAllowedToBorrow, int maxActiveBorrowings)
    {
        Id = id;
        Name = name;
        IsAllowedToBorrow = isAllowedToBorrow;
        MaxActiveBorrowings = maxActiveBorrowings;
    }

    public void Suspend() => IsAllowedToBorrow = false;
    public void Reinstate() => IsAllowedToBorrow = true;
}