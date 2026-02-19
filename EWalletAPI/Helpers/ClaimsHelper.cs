using System.Security.Claims;

namespace EWalletAPI.Helpers;

public static class ClaimsHelper
{
    /// <summary>
    /// Extracts the authenticated user's Guid from the JWT "sub" claim.
    /// Throws InvalidOperationException if the claim is missing or malformed.
    /// </summary>
    public static Guid GetUserId(ClaimsPrincipal principal)
    {
        var sub = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? principal.FindFirstValue("sub");

        if (string.IsNullOrWhiteSpace(sub) || !Guid.TryParse(sub, out var userId))
            throw new InvalidOperationException("Invalid or missing user identity in token.");

        return userId;
    }

    public static string GetUserEmail(ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(ClaimTypes.Email)
               ?? principal.FindFirstValue("email")
               ?? string.Empty;
    }

    public static string GetUserRole(ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
    }
}
