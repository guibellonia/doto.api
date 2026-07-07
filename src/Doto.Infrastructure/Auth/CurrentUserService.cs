using Microsoft.AspNetCore.Http;
using Doto.Application.Interfaces;
using System.Security.Claims;

namespace Doto.Infrastructure.Auth;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User =>
       _httpContextAccessor.HttpContext?.User;

    public string? SupabaseUserId =>
         User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    public string? Email =>
        User?.FindFirst(ClaimTypes.Email)?.Value ?? User?.FindFirst("email")?.Value;
}
