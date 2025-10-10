using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Administration.Helpers;

public class MyController<T> : ControllerBase
{
    protected long UserId => GetUserId();

    private long GetUserId()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                     User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        return long.Parse(userId ?? string.Empty);
    }
}