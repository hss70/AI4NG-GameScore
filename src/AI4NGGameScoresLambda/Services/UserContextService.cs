using System.Security.Claims;
using AI4NGGameScoresLambda.Interfaces;
using Microsoft.AspNetCore.Http;

namespace AI4NGGameScoresLambda.Services;

public sealed class UserContextService : IUserContextService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContextService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string GetUserId()
    {
        var httpContext = _httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("No active HTTP context.");

        var userId =
            httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ??
            httpContext.User.FindFirstValue("sub");

        if (string.IsNullOrWhiteSpace(userId))
            throw new UnauthorizedAccessException("Authenticated user ID claim was not found.");

        return userId;
    }
}