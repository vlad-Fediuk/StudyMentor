namespace study_mentor_api.DTOs;

public sealed class PagedResult<T>
{
    public PagedResult(IEnumerable<T> items, int totalCount, int page, int pageSize)
{
    if (items is null)
        throw new ArgumentNullException(nameof(items));

    if (page < 1)
        throw new ArgumentOutOfRangeException(nameof(page));

    if (pageSize < 1)
        throw new ArgumentOutOfRangeException(nameof(pageSize));

    if (totalCount < 0)
        throw new ArgumentOutOfRangeException(nameof(totalCount));

    Items = items.ToList();
    TotalCount = totalCount;
    Page = page;
    PageSize = pageSize;
}
    public IReadOnlyList<T> Items { get; }
    public int TotalCount { get; }
    public int Page { get; }
    public int PageSize { get; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
