

namespace Domain.Models.Common;

public class PagedResult<T> 
{
    public int TotalCount { get; set; }

    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public bool IsFirst { get; set; }

    public bool IsLast { get; set; }

    public IReadOnlyCollection<T> Data { get; set; }

    public PagedResult()
    {
        TotalCount = PageNumber = PageSize = 0;
        IsFirst = IsLast = true;
        Data = [];
    }

    public PagedResult(List<T> data)
    {
        IsFirst = IsLast = true;
        Data = data;
        TotalCount = data.Count;
        PageNumber = PageSize = 0;
    }

    public PagedResult(List<T> data, int allCount, PagedRequest pagedRequest)
    {
        IsFirst = pagedRequest.PageNumber == 1;
        Data = data;
        TotalCount = allCount;
        PageNumber = pagedRequest.PageNumber;
        PageSize = pagedRequest.PageSize;
        IsLast = pagedRequest.PageNumber * pagedRequest.PageSize >= allCount;
    }

    public PagedResult(int totalCount, PagedRequest pagedRequest, bool isFirst, bool isLast, List<T> data)
    {
        TotalCount = totalCount;
        PageNumber = pagedRequest.PageNumber;
        PageSize = pagedRequest.PageSize;
        IsFirst = isFirst;
        IsLast = isLast;
        Data = data;
    }

}