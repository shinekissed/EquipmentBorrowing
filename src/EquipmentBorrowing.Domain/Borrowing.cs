namespace EquipmentBorrowing.Domain;

public class Borrowing
{
    public int Id { get; private set; }
    public int StudentId { get; private set; }
    public Student? Student { get; private set; }

    public int EquipmentId { get; private set; }
    public Equipment? Equipment { get; private set; }

    public DateTime DateBorrowed { get; private set; }
    public DateTime ExpectedReturnDate { get; private set; }
    public DateTime? ActualReturnDate { get; private set; }
    public BorrowingStatus Status { get; private set; }

    // Parameterless constructor for EF Core
    protected Borrowing() { }

    public Borrowing(
        int id,
        int studentId,
        int equipmentId,
        DateTime dateBorrowed,
        DateTime expectedReturnDate)
    {
        if (expectedReturnDate.Date <= dateBorrowed.Date)
            throw new ArgumentException("Expected return date must be later than checkout date.", nameof(expectedReturnDate));

        Id = id;
        StudentId = studentId;
        EquipmentId = equipmentId;
        DateBorrowed = dateBorrowed;
        ExpectedReturnDate = expectedReturnDate;
        Status = BorrowingStatus.Active;
    }

    public void MarkAsReturned() => MarkAsReturned(DateTime.UtcNow);

    public void MarkAsReturned(DateTime actualReturnDate)
    {
        if (Status == BorrowingStatus.Returned)
            throw new InvalidOperationException($"Borrowing {Id} has already been returned.");

        if (actualReturnDate.Date < DateBorrowed.Date)
            throw new ArgumentException("Actual return date cannot precede checkout date.", nameof(actualReturnDate));

        ActualReturnDate = actualReturnDate;
        Status = BorrowingStatus.Returned;
    }
}