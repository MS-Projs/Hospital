using Microsoft.AspNetCore.Http;

namespace Infrastructure.Extensions;

public static class HttpContextExtension
{
    private static IHttpContextAccessor _httpContextAccessor;

    public static void Configure(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public static string GetRequestPath()
    {
        var request = _httpContextAccessor.HttpContext?.Request;
        return $"{request?.Scheme}://{request?.Host}";
    }
}