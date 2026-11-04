using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dowlume.Core;

namespace Dowlume.App.ViewModels;

public partial class MainViewModel
{
    private TrackedFile[] _reviewQueue = [];

    [ObservableProperty] private string _reviewSearch = string.Empty;

    public bool IsReviewEmpty => ReviewFiles.Count == 0;
    public string ReviewResultsLabel => Text.Format("Showing {0} of {1} files awaiting review", ReviewFiles.Count, _reviewQueue.Length);
    public string ReviewEmptyMessage => Text.Translate(_reviewQueue.Length == 0
        ? "No files awaiting review."
        : "No matching files. Try another search or clear it.");

    partial void OnReviewSearchChanged(string value) => FilterReviewQueue();

    [RelayCommand]
    private void ClearReviewSearch() => ReviewSearch = string.Empty;

    private void UpdateReviewQueue(IEnumerable<TrackedFile> files)
    {
        _reviewQueue = files.Where(file => file.Status == LifecycleStatus.InReview).ToArray();
        ReviewCount = _reviewQueue.Length;
        FilterReviewQueue();
    }

    private void FilterReviewQueue()
    {
        Guid? selectedId = SelectedReview?.Id;
        string search = ReviewSearch.Trim();
        Replace(ReviewFiles, _reviewQueue.Where(file => file.CurrentPath.Contains(search, StringComparison.OrdinalIgnoreCase)));
        // A hidden selection must never remain the target of a review command.
        SelectedReview = ReviewFiles.FirstOrDefault(file => file.Id == selectedId);
        OnPropertyChanged(nameof(IsReviewEmpty));
        RefreshReviewLabels();
    }

    private void RefreshReviewLabels()
    {
        OnPropertyChanged(nameof(ReviewResultsLabel));
        OnPropertyChanged(nameof(ReviewEmptyMessage));
    }
}
