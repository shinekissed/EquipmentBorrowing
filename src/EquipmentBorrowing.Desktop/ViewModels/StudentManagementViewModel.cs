using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class StudentManagementViewModel : ViewModelBase
{
    private readonly StudentService? _studentService;

    public ObservableCollection<Student> Students { get; } = new();

    [ObservableProperty]
    private Student? _selectedStudent;

    // Register Form Fields (UC-12)
    [ObservableProperty]
    private string _newStudentNumber = string.Empty;

    [ObservableProperty]
    private string _newName = string.Empty;

    [ObservableProperty]
    private string _newContactInfo = string.Empty;

    // Edit Form Fields (UC-13)
    [ObservableProperty]
    private string _editStudentNumber = string.Empty;

    [ObservableProperty]
    private string _editName = string.Empty;

    [ObservableProperty]
    private string _editContactInfo = string.Empty;

    // Feedback
    [ObservableProperty]
    private string? _feedbackMessage;

    [ObservableProperty]
    private bool _isFeedbackSuccess;

    [ObservableProperty]
    private bool _hasFeedback;

    // Fallback constructor
    public StudentManagementViewModel() : this(null!)
    {
    }

    // DI Constructor
    public StudentManagementViewModel(StudentService? studentService)
    {
        _studentService = studentService;
        if (_studentService != null)
        {
            _ = LoadStudentsAsync();
        }
    }

    partial void OnSelectedStudentChanged(Student? value)
    {
        if (value is not null)
        {
            EditStudentNumber = value.StudentNumber;
            EditName = value.Name;
            EditContactInfo = value.ContactInfo ?? string.Empty;
        }
        else
        {
            EditStudentNumber = string.Empty;
            EditName = string.Empty;
            EditContactInfo = string.Empty;
        }
    }

    [RelayCommand]
    public async Task LoadStudentsAsync()
    {
        if (_studentService is null) return;

        try
        {
            var list = await _studentService.GetAllStudentsAsync();
            Students.Clear();
            foreach (var student in list)
            {
                Students.Add(student);
            }

            if (SelectedStudent is not null)
            {
                SelectedStudent = Students.FirstOrDefault(s => s.Id == SelectedStudent.Id);
            }
            else if (Students.Count > 0)
            {
                SelectedStudent = Students[0];
            }
        }
        catch (Exception ex)
        {
            ShowFeedback($"Failed to load students: {ex.Message}", false);
        }
    }

    [RelayCommand]
    public async Task RegisterStudentAsync()
    {
        if (_studentService is null) return;

        var result = await _studentService.RegisterStudentAsync(
            NewStudentNumber,
            NewName,
            string.IsNullOrWhiteSpace(NewContactInfo) ? null : NewContactInfo);

        if (result.Success)
        {
            ShowFeedback(result.Message, true);
            NewStudentNumber = string.Empty;
            NewName = string.Empty;
            NewContactInfo = string.Empty;
            await LoadStudentsAsync();
        }
        else
        {
            ShowFeedback(result.Message, false);
        }
    }

    [RelayCommand]
    public async Task UpdateStudentAsync()
    {
        if (_studentService is null) return;

        if (SelectedStudent is null)
        {
            ShowFeedback("Please select a student from the list first.", false);
            return;
        }

        var result = await _studentService.UpdateStudentAsync(
            SelectedStudent.Id,
            EditStudentNumber,
            EditName,
            string.IsNullOrWhiteSpace(EditContactInfo) ? null : EditContactInfo);

        if (result.Success)
        {
            ShowFeedback(result.Message, true);
            var currentId = SelectedStudent.Id;
            await LoadStudentsAsync();
            SelectedStudent = Students.FirstOrDefault(s => s.Id == currentId);
        }
        else
        {
            ShowFeedback(result.Message, false);
        }
    }

    [RelayCommand]
    public async Task ToggleEligibilityAsync()
    {
        if (_studentService is null) return;

        if (SelectedStudent is null)
        {
            ShowFeedback("Please select a student to change eligibility.", false);
            return;
        }

        var newEligibility = !SelectedStudent.IsEligible;
        var result = await _studentService.ChangeEligibilityAsync(SelectedStudent.Id, newEligibility);

        if (result.Success)
        {
            ShowFeedback(result.Message, true);
            var currentId = SelectedStudent.Id;
            await LoadStudentsAsync();
            SelectedStudent = Students.FirstOrDefault(s => s.Id == currentId);
        }
        else
        {
            ShowFeedback(result.Message, false);
        }
    }

    [RelayCommand]
    public async Task PermitStudentAsync()
    {
        if (_studentService is null) return;

        if (SelectedStudent is null)
        {
            ShowFeedback("Please select a student first.", false);
            return;
        }

        var result = await _studentService.ChangeEligibilityAsync(SelectedStudent.Id, true);

        if (result.Success)
        {
            ShowFeedback(result.Message, true);
            var currentId = SelectedStudent.Id;
            await LoadStudentsAsync();
            SelectedStudent = Students.FirstOrDefault(s => s.Id == currentId);
        }
        else
        {
            ShowFeedback(result.Message, false);
        }
    }

    [RelayCommand]
    public async Task RestrictStudentAsync()
    {
        if (_studentService is null) return;

        if (SelectedStudent is null)
        {
            ShowFeedback("Please select a student first.", false);
            return;
        }

        var result = await _studentService.ChangeEligibilityAsync(SelectedStudent.Id, false);

        if (result.Success)
        {
            ShowFeedback(result.Message, true);
            var currentId = SelectedStudent.Id;
            await LoadStudentsAsync();
            SelectedStudent = Students.FirstOrDefault(s => s.Id == currentId);
        }
        else
        {
            ShowFeedback(result.Message, false);
        }
    }

    private void ShowFeedback(string message, bool isSuccess)
    {
        FeedbackMessage = message;
        IsFeedbackSuccess = isSuccess;
        HasFeedback = true;
    }
}
