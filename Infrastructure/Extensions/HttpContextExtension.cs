using Microsoft.AspNetCore.Http;

namespace Infrastructure.Extensions;

public static class HttpContextExtension
{
    public static string GetRequestPath(this HttpContext httpContext)
    {
        var request = httpContext.Request;
        return $"{request.Scheme}://{request.Host}";
    }

    public static string GetRequestPath(this IHttpContextAccessor httpContextAccessor)
    {
        var request = httpContextAccessor.HttpContext?.Request;
        return request != null ? $"{request.Scheme}://{request.Host}" : string.Empty;
    }
}