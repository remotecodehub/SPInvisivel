namespace InvisibleSP.UnitTests;

public sealed class JwtTokenServiceTests
{
    [Fact]
    public void Token_service_should_reject_invalid_secret_length()
    {
        var store = new RevokedTokenStore();
        var service = new JwtTokenService(
            Options.Create(new JwtOptions { SecretKey = "short" }),
            store);

        var action = () => service.CreateTokens("user", "user@example.com", [], []);
        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Revoked_token_store_should_expire_entries()
    {
        var store = new RevokedTokenStore();
        store.Revoke("token", DateTimeOffset.UtcNow.AddMinutes(1));
        store.IsRevoked("token").Should().BeTrue();

        store.Revoke("expired", DateTimeOffset.UtcNow.AddSeconds(-1));
        store.IsRevoked("expired").Should().BeFalse();
    }

    [Fact]
    public void Token_service_should_round_trip_claims_and_metadata()
    {
        var store = new RevokedTokenStore();
        var service = new JwtTokenService(
            Options.Create(new JwtOptions
            {
                SecretKey = "InvisibleSP-test-secret-key-with-at-least-256-bits-2026",
                Issuer = "InvisibleSP",
                Audience = "InvisibleSP",
                AccessTokenLifetime = TimeSpan.FromMinutes(5),
                RefreshTokenLifetime = TimeSpan.FromHours(1)
            }),
            store);

        var tokens = service.CreateTokens(
            "user-id",
            "user@example.com",
            ["Administrator"],
            [new Claim(IdentityClaimTypes.Permission, "system.admin")]);

        var principal = service.ValidateToken(tokens.AccessToken);
        principal.Should().NotBeNull();
        principal!.FindFirstValue(ClaimTypes.Role).Should().Be("Administrator");
        principal.FindFirstValue(IdentityClaimTypes.Permission).Should().Be("system.admin");
        service.GetTokenId(tokens.AccessToken).Should().NotBeNullOrWhiteSpace();
        service.GetExpiration(tokens.AccessToken).Should().BeAfter(DateTimeOffset.UtcNow);
        service.ValidateToken("not-a-token").Should().BeNull();
    }
}
