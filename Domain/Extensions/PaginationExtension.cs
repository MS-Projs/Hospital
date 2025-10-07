using Domain.Models.Common;

namespace Domain.Extensions;

public static class PaginationExtension
{
    public static Result<PagedResult<T>> ToListResponse<T>(this List<T> list)
    {
        var pagedResult = new PagedResult<T>(list);
        return new Result<PagedResult<T>>(pagedResult);
    }

    // Sahifalangan (paged) javob
    public static Result<PagedResult<T>> ToListResponse<T>(this List<T> list, int pageNumber, int pageSize)
    {
        var totalCount = list.Count;
        var skip = (pageNumber - 1) * pageSize;
        var pageItems = list.Skip(skip).Take(pageSize).ToList();

        var pagedRequest = new PagedRequest
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var pagedResult = new PagedResult<T>(
            totalCount: totalCount,
            pagedRequest: pagedRequest,
            isFirst: pageNumber == 1,
            isLast: pageNumber * pageSize >= totalCount,
            data: pageItems
        );

        return new Result<PagedResult<T>>(pagedResult);
    }
}