namespace InvisibleSP.Application.Common.Identity;

public interface IJwtTokenService
{
    TokenResponse CreateTokens(string userId, string email, IEnumerable<string> roles, IEnumerable<Claim> claims);
    ClaimsPrincipal? ValidateToken(string token, bool validateLifetime = true);
    string? GetTokenId(string token);
    DateTimeOffset? GetExpiration(string token);
}

public interface IRevokedTokenStore
{
    bool IsRevoked(string tokenId);
    void Revoke(string tokenId, DateTimeOffset expiresAt);
}

public interface IIdentityEmailSender
{
    Task SendConfirmationAsync(string email, string confirmationLink, CancellationToken cancellationToken);
    Task SendPasswordResetAsync(string email, string resetLink, CancellationToken cancellationToken);
}
