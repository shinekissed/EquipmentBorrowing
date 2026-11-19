using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Application.DTOs;
using EquipmentBorrowing.Application.Services;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class BorrowingsSearchSortViewModel : ViewModelBase
{
    private readonly BorrowingHistoryService? _historyService;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private BorrowingStatusFilter _selectedStatusFilter = BorrowingStatusFilter.All;

    [ObservableProperty]
    private BorrowingSortField _selectedSortField = BorrowingSortField.CheckoutDate;

    [ObservableProperty]
    private bool _sortAscending;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _hasNoResults;

    [ObservableProperty]
    private string _resultsCountMessage = string.Empty;

    public ObservableCollection<BorrowingRecordDto> Results { get; } = new();

    public BorrowingStatusFilter[] StatusFilters { get; } = Enum.GetValues<BorrowingStatusFilter>();
    public BorrowingSortField[] SortFields { get; } = Enum.GetValues<BorrowingSortField>();

    public BorrowingsSearchSortViewModel() : this(null!)
    {
    }

    public BorrowingsSearchSortViewModel(BorrowingHistoryService? historyService)
    {
        _historyService = historyService;
        _ = SearchAsync();
    }

    [RelayCommand]
    public async Task SearchAsync()
    {
        if (_historyService is null) return;

        IsLoading = true;
        try
        {
            var list = await _historyService.SearchBorrowingsAsync(
                SearchText,
                SelectedStatusFilter,
                SelectedSortField,
                SortAscending);

            Results.Clear();
            foreach (var item in list)
            {
                Results.Add(item);
            }

            HasNoResults = Results.Count == 0;
            ResultsCountMessage = $"{Results.Count} record(s) found";
        }
        catch (Exception ex)
        {
            Results.Clear();
            HasNoResults = true;
            ResultsCountMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task ClearCriteriaAsync()
    {
        SearchText = string.Empty;
        SelectedStatusFilter = BorrowingStatusFilter.All;
        SelectedSortField = BorrowingSortField.CheckoutDate;
        SortAscending = false;
        await SearchAsync();
    }

    partial void OnSelectedStatusFilterChanged(BorrowingStatusFilter value) => _ = SearchAsync();
    partial void OnSelectedSortFieldChanged(BorrowingSortField value) => _ = SearchAsync();
    partial void OnSortAscendingChanged(bool value) => _ = SearchAsync();
}
