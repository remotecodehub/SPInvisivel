namespace InvisibleSP.UnitTests;

public sealed class IdentityFailurePathTests
{
    [Fact]
    public async Task Identity_service_should_fail_safely_for_invalid_credentials_and_tokens()
    {
        await using var fixture = await IdentityFixture.CreateAsync();

        (await fixture.Service.LoginAsync("missing@example.com", "Password1!", null, null, CancellationToken.None)).Should().BeNull();
        (await fixture.Service.RefreshAsync("not-a-token", CancellationToken.None)).Should().BeNull();
        (await fixture.Service.RevokeAsync("not-a-token", CancellationToken.None)).Should().BeFalse();
        (await fixture.Service.ConfirmEmailAsync("missing", "code", null, CancellationToken.None)).Should().BeFalse();
        (await fixture.Service.GetInfoAsync("missing", CancellationToken.None)).Should().BeNull();
        (await fixture.Service.ResetPasswordAsync("missing@example.com", "code", "Password2!", CancellationToken.None)).Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task Resend_confirmation_should_be_idempotent_for_unknown_or_confirmed_users()
    {
        await using var fixture = await IdentityFixture.CreateAsync();

        (await fixture.Service.ResendConfirmationEmailAsync("missing@example.com", CancellationToken.None)).Succeeded.Should().BeTrue();
        await fixture.Service.InitializeSetupAsync("admin@example.com", "Password1!", CancellationToken.None);
        (await fixture.Service.ResendConfirmationEmailAsync("admin@example.com", CancellationToken.None)).Succeeded.Should().BeTrue();
        fixture.EmailSender.ConfirmationLinks.Should().BeEmpty();
    }
}
