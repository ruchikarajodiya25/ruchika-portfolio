using ServiceHubPro.Infrastructure.Entities;

namespace ServiceHubPro.Infrastructure.Services;

public interface IJwtTokenService
{
    string GenerateAccessToken(ApplicationUser user, IList<string> roles);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
