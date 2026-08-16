namespace InvisibleSP.Application.Common.Identity;

public sealed class JwtOptions
{
    public const string SectionName = "Authentication:Jwt";

    public required string SecretKey { get; init; }
    public string Issuer { get; init; } = "InvisibleSP";
    public string Audience { get; init; } = "InvisibleSP";
    public TimeSpan AccessTokenLifetime { get; init; } = TimeSpan.FromMinutes(15);
    public TimeSpan RefreshTokenLifetime { get; init; } = TimeSpan.FromDays(14);
}
