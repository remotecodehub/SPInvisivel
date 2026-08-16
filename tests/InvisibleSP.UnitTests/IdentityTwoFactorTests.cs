namespace InvisibleSP.UnitTests;

public sealed class IdentityTwoFactorTests
{
    [Fact]
    public async Task Two_factor_should_be_configurable_and_used_during_login()
    {
        await using var fixture = await IdentityFixture.CreateAsync();
        await fixture.Service.InitializeSetupAsync("admin@example.com", "Password1!", CancellationToken.None);

        var user = await fixture.UserManager.FindByEmailAsync("admin@example.com");
        user.Should().NotBeNull();

        await fixture.UserManager.ResetAuthenticatorKeyAsync(user!);
        var code = await fixture.UserManager.GenerateTwoFactorTokenAsync(
            user,
            fixture.UserManager.Options.Tokens.AuthenticatorTokenProvider);

        var enabled = await fixture.Service.ConfigureTwoFactorAsync(
            user!.Id,
            true,
            code,
            true,
            false,
            false,
            CancellationToken.None);

        enabled.Should().NotBeNull();
        enabled!.IsTwoFactorEnabled.Should().BeTrue();
        enabled.RecoveryCodes.Should().NotBeNullOrEmpty();

        var loginCode = await fixture.UserManager.GenerateTwoFactorTokenAsync(
            user,
            fixture.UserManager.Options.Tokens.AuthenticatorTokenProvider);
        var login = await fixture.Service.LoginAsync("admin@example.com", "Password1!", loginCode, null, CancellationToken.None);
        login.Should().NotBeNull();

        var disabled = await fixture.Service.ConfigureTwoFactorAsync(
            user.Id,
            false,
            null,
            false,
            true,
            false,
            CancellationToken.None);
        disabled.Should().NotBeNull();
        disabled!.IsTwoFactorEnabled.Should().BeFalse();
    }
}
