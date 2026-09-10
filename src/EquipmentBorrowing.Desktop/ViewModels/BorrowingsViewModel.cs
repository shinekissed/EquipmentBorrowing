using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class BorrowingsViewModel : ViewModelBase
{
    public ObservableCollection<BorrowingItemDisplay> ActiveBorrowings { get; } = new();

    [ObservableProperty]
    private BorrowingItemDisplay? _selectedBorrowing;

    public BorrowingsViewModel()
    {
        LoadSeedData();
    }

    private void LoadSeedData()
    {
        ActiveBorrowings.Add(new BorrowingItemDisplay(
            id: 1,
            studentName: "Juan Dela Cruz",
            studentId: 1,
            equipmentName: "Oscilloscope",
            equipmentId: 101,
            dateBorrowed: DateTime.Now.AddDays(-2),
            dueDate: DateTime.Now.AddDays(5)
        ));

        ActiveBorrowings.Add(new BorrowingItemDisplay(
            id: 2,
            studentName: "Juan Dela Cruz",
            studentId: 1,
            equipmentName: "Function Generator",
            equipmentId: 104,
            dateBorrowed: DateTime.Now.AddDays(-4),
            dueDate: DateTime.Now.AddDays(3)
        ));

        if (ActiveBorrowings.Count > 0)
        {
            SelectedBorrowing = ActiveBorrowings[0];
        }
    }
}

// Display helper model for the Borrowings UI
public class BorrowingItemDisplay
{
    public int Id { get; }
    public string StudentName { get; }
    public int StudentId { get; }
    public string EquipmentName { get; }
    public int EquipmentId { get; }
    public DateTime DateBorrowed { get; }
    public DateTime DueDate { get; }
    public string Status { get; }

    public BorrowingItemDisplay(int id, string studentName, int studentId, string equipmentName, int equipmentId, DateTime dateBorrowed, DateTime dueDate)
    {
        Id = id;
        StudentName = studentName;
        StudentId = studentId;
        EquipmentName = equipmentName;
        EquipmentId = equipmentId;
        DateBorrowed = dateBorrowed;
        DueDate = dueDate;
        Status = "Active";
    }
}