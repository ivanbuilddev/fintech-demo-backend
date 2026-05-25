using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace Fintech.Api.Controllers;
public class CustomControllerBase : ControllerBase
{
    protected Guid GetCurrentUserGuid()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if(userId == null) return Guid.Empty;
        return Guid.Parse(userId);
    }
}