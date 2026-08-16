namespace InvisibleSP.Application.Common.Identity;

public sealed class JwtOptions
{
    public const string SectionName = "Authentication:Jwt";

    public required string SecretKey { get; set; }
    public string Issuer { get; set; } = "InvisibleSP";
    public string Audience { get; set; } = "InvisibleSP";
    public TimeSpan AccessTokenLifetime { get; set; } = TimeSpan.FromMinutes(15);
    public TimeSpan RefreshTokenLifetime { get; set; } = TimeSpan.FromDays(14);

}
