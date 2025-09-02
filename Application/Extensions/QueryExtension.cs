using System.Security.Cryptography.X509Certificates;
using Domain.Models.Common;

namespace Application.Extensions;

public static class QueryExtension
{
    public static IQueryable<T> Paginate<T>(this IQueryable<T> query, PagedRequest request)
        => query.Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize);
}