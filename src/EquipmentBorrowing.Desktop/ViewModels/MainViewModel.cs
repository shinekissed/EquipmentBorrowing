using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private ViewModelBase _currentView;

    public EquipmentViewModel EquipmentVm { get; }
    public BorrowingsViewModel BorrowingsVm { get; }

    public MainViewModel()
    {
        EquipmentVm = new EquipmentViewModel();
        BorrowingsVm = new BorrowingsViewModel();
        _currentView = EquipmentVm; // Default view on startup
    }

    [RelayCommand]
    private void ShowEquipment()
    {
        CurrentView = EquipmentVm;
    }

    [RelayCommand]
    private void ShowBorrowings()
    {
        CurrentView = BorrowingsVm;
    }
}