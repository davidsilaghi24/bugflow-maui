namespace BugFlow.Pages;

public static class ListPageUiState
{
    public static void SetLoading(ActivityIndicator indicator, bool isLoading)
    {
        indicator.IsRunning = isLoading;
        indicator.IsVisible = isLoading;
    }

    public static void SetEmptyState(View emptyState, int itemCount, bool isLoading)
    {
        emptyState.IsVisible = itemCount == 0 && !isLoading;
    }
}
