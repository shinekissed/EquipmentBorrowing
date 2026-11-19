using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private ViewModelBase _currentView;

    public EquipmentViewModel EquipmentVm { get; }
    public BorrowingsViewModel BorrowingsVm { get; }
    public StudentManagementViewModel StudentVm { get; }
    public MultiItemBorrowViewModel MultiBorrowVm { get; }
    public BorrowingHistoryViewModel HistoryVm { get; }
    public BorrowingsSearchSortViewModel SearchSortVm { get; }

    public MainViewModel() : this(
        new EquipmentViewModel(),
        new BorrowingsViewModel(),
        new StudentManagementViewModel(),
        new MultiItemBorrowViewModel(),
        new BorrowingHistoryViewModel(),
        new BorrowingsSearchSortViewModel())
    {
    }

    // Constructor Injection (Part H)
    public MainViewModel(
        EquipmentViewModel equipmentVm,
        BorrowingsViewModel borrowingsVm,
        StudentManagementViewModel studentVm,
        MultiItemBorrowViewModel multiBorrowVm,
        BorrowingHistoryViewModel historyVm,
        BorrowingsSearchSortViewModel searchSortVm)
    {
        EquipmentVm = equipmentVm;
        BorrowingsVm = borrowingsVm;
        StudentVm = studentVm;
        MultiBorrowVm = multiBorrowVm;
        HistoryVm = historyVm;
        SearchSortVm = searchSortVm;

        // Auto-refresh cross-view state when an action happens
        EquipmentVm.OnBorrowSucceeded = () =>
        {
            BorrowingsVm.Refresh();
            _ = HistoryVm.RefreshCommand.ExecuteAsync(null);
            _ = SearchSortVm.SearchCommand.ExecuteAsync(null);
        };

        BorrowingsVm.OnReturnSucceeded = () =>
        {
            EquipmentVm.Refresh();
            _ = HistoryVm.RefreshCommand.ExecuteAsync(null);
            _ = SearchSortVm.SearchCommand.ExecuteAsync(null);
        };

        MultiBorrowVm.OnMultiBorrowSucceeded = () =>
        {
            EquipmentVm.Refresh();
            BorrowingsVm.Refresh();
            _ = HistoryVm.RefreshCommand.ExecuteAsync(null);
            _ = SearchSortVm.SearchCommand.ExecuteAsync(null);
        };

        _currentView = EquipmentVm;
    }

    [RelayCommand]
    private void ShowEquipment()
    {
        EquipmentVm.Refresh();
        CurrentView = EquipmentVm;
    }

    [RelayCommand]
    private void ShowBorrowings()
    {
        BorrowingsVm.Refresh();
        CurrentView = BorrowingsVm;
    }

    [RelayCommand]
    private void ShowMultiBorrow()
    {
        MultiBorrowVm.LoadDataCommand.Execute(null);
        CurrentView = MultiBorrowVm;
    }

    [RelayCommand]
    private void ShowStudents()
    {
        StudentVm.LoadStudentsCommand.Execute(null);
        CurrentView = StudentVm;
    }

    [RelayCommand]
    private void ShowHistory()
    {
        HistoryVm.RefreshCommand.Execute(null);
        CurrentView = HistoryVm;
    }

    [RelayCommand]
    private void ShowSearchSort()
    {
        SearchSortVm.SearchCommand.Execute(null);
        CurrentView = SearchSortVm;
    }
}
