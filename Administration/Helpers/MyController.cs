using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Administration.Helpers;

public class MyController<T> : ControllerBase
{
    protected Guid UserId => GetUserId();

    private Guid GetUserId()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                     User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        return Guid.Parse(userId ?? string.Empty);
    }
}