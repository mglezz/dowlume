using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dowlume.Core;

namespace Dowlume.App.ViewModels;

public partial class MainViewModel
{
    public IReadOnlyList<HistoryFilter<OperationKind>> ActivityKinds =>
        [new(Text.Translate("All actions"), null), .. Enum.GetValues<OperationKind>().Select(kind => new HistoryFilter<OperationKind>(Text.Translate(kind.ToString()), kind))];

    public IReadOnlyList<HistoryFilter<OperationStatus>> ActivityStatuses =>
        [new(Text.Translate("All statuses"), null), .. Enum.GetValues<OperationStatus>().Select(status =>
            new HistoryFilter<OperationStatus>(Text.Translate(status == OperationStatus.NeedsAttention ? "Needs attention" : status.ToString()), status))];

    private OperationHistoryQuery _historyQuery = new();
    private OperationHistoryPage? _historyPage;

    [ObservableProperty] private string _activitySearch = string.Empty;
    [ObservableProperty] private HistoryFilter<OperationKind> _selectedActivityKind = new("All actions", null);
    [ObservableProperty] private HistoryFilter<OperationStatus> _selectedActivityStatus = new("All statuses", null);
    public string ActivityPageLabel => _historyPage is not { } page ? Text.Translate("No operations yet.") :
        page.TotalCount == 0 ? Text.Translate("No matching operations.") :
        Text.Format("Page {0} of {1} · {2} matching operations", page.PageNumber, page.PageCount, page.TotalCount);
    [ObservableProperty] private bool _hasPreviousActivityPage;
    [ObservableProperty] private bool _hasNextActivityPage;
    [ObservableProperty] private bool _isActivityEmpty = true;

    [RelayCommand]
    private Task SearchActivityAsync() => RunAsync(async () =>
    {
        _historyQuery = new OperationHistoryQuery(ActivitySearch, SelectedActivityKind.Value, SelectedActivityStatus.Value);
        await RefreshAsync();
    });

    [RelayCommand]
    private Task ClearActivityFiltersAsync() => RunAsync(async () =>
    {
        ActivitySearch = string.Empty;
        SelectedActivityKind = ActivityKinds[0];
        SelectedActivityStatus = ActivityStatuses[0];
        _historyQuery = new OperationHistoryQuery();
        await RefreshAsync();
    });

    [RelayCommand]
    private Task PreviousActivityPageAsync() => ChangeActivityPageAsync(-1);

    [RelayCommand]
    private Task NextActivityPageAsync() => ChangeActivityPageAsync(1);

    private Task ChangeActivityPageAsync(int direction) => RunAsync(async () =>
    {
        if (_historyPage is null || direction < 0 && !_historyPage.HasPreviousPage || direction > 0 && !_historyPage.HasNextPage)
        {
            return;
        }
        int pageNumber = _historyPage.PageNumber + direction;
        _historyQuery = _historyQuery with
        {
            PageNumber = pageNumber,
            ThroughSequence = pageNumber == 1 ? null : _historyPage.ThroughSequence
        };
        await RefreshAsync();
    });

    private void ShowActivityPage(OperationHistoryPage page)
    {
        Guid? selectedId = SelectedOperation?.Id;
        Replace(Activity, page.Items);
        SelectedOperation = Activity.FirstOrDefault(operation => operation.Id == selectedId);
        _historyPage = page;
        _historyQuery = _historyQuery with
        {
            PageNumber = page.PageNumber,
            ThroughSequence = page.PageNumber == 1 ? null : _historyQuery.ThroughSequence
        };
        HasPreviousActivityPage = page.HasPreviousPage;
        HasNextActivityPage = page.HasNextPage;
        IsActivityEmpty = page.TotalCount == 0;
        OnPropertyChanged(nameof(ActivityPageLabel));
    }
}
