namespace study_mentor_api.DTOs;

public sealed class CrudQueryOptions
{
    private const int MaxPageSize = 100;
    private const int DefaultPage = 1;
    private const int DefaultPageSize = 20;

    public string? SearchTerm { get; init; }
    public string? SortBy { get; init; }
    public bool SortDescending { get; init; }

    private int _page = DefaultPage;
    public int Page
    {
        get => _page;
        init => _page = value < 1 ? DefaultPage : value;
    }

    private int _pageSize = DefaultPageSize;
    public int PageSize
    {
        get => _pageSize;
        init => _pageSize = value <= 0 ? DefaultPageSize  : value > MaxPageSize ? MaxPageSize   : value;
    }
}