namespace InvisibleSP.UnitTests;

public sealed class IdentityBranchTests
{
    [Fact]
    public async Task Register_should_return_identity_errors_for_duplicate_email()
    {
        await using var fixture = await IdentityFixture.CreateAsync();
        (await fixture.Service.RegisterAsync("user@example.com", "Password1!", CancellationToken.None)).Succeeded.Should().BeTrue();

        var result = await fixture.Service.RegisterAsync("user@example.com", "Password1!", CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Login_should_reject_bad_password_and_unconfirmed_user()
    {
        await using var fixture = await IdentityFixture.CreateAsync();
        await fixture.Service.RegisterAsync("user@example.com", "Password1!", CancellationToken.None);

        (await fixture.Service.LoginAsync("user@example.com", "WrongPassword1!", null, null, CancellationToken.None)).Should().BeNull();
        (await fixture.Service.LoginAsync("user@example.com", "Password1!", null, null, CancellationToken.None)).Should().BeNull();
    }

    [Fact]
    public async Task Login_should_reject_invalid_two_factor_code_and_recovery_code()
    {
        await using var fixture = await IdentityFixture.CreateAsync();
        await fixture.Service.InitializeSetupAsync("admin@example.com", "Password1!", CancellationToken.None);
        var user = await fixture.UserManager.FindByEmailAsync("admin@example.com");
        user.Should().NotBeNull();
        await fixture.UserManager.ResetAuthenticatorKeyAsync(user!);
        await fixture.UserManager.SetTwoFactorEnabledAsync(user, true);

        (await fixture.Service.LoginAsync("admin@example.com", "Password1!", "000000", null, CancellationToken.None)).Should().BeNull();
        (await fixture.Service.LoginAsync("admin@example.com", "Password1!", null, "invalid-recovery-code", CancellationToken.None)).Should().BeNull();
    }

    [Fact]
    public async Task Refresh_should_reject_access_tokens_empty_subjects_and_unknown_users()
    {
        await using var fixture = await IdentityFixture.CreateAsync();
        await fixture.Service.InitializeSetupAsync("admin@example.com", "Password1!", CancellationToken.None);
        var user = await fixture.UserManager.FindByEmailAsync("admin@example.com");
        user.Should().NotBeNull();
        var access = await fixture.Service.LoginAsync("admin@example.com", "Password1!", null, null, CancellationToken.None);
        access.Should().NotBeNull();
        (await fixture.Service.RefreshAsync(access!.AccessToken, CancellationToken.None)).Should().BeNull();

        var emptySubject = fixture.TokenService.CreateTokens(string.Empty, "empty@example.com", [], []).RefreshToken;
        (await fixture.Service.RefreshAsync(emptySubject, CancellationToken.None)).Should().BeNull();

        var unknownUser = fixture.TokenService.CreateTokens("missing-user", "missing@example.com", [], []).RefreshToken;
        (await fixture.Service.RefreshAsync(unknownUser, CancellationToken.None)).Should().BeNull();
    }

    [Fact]
    public async Task Confirm_email_should_support_changed_email_and_reject_invalid_code()
    {
        await using var fixture = await IdentityFixture.CreateAsync();
        await fixture.Service.RegisterAsync("old@example.com", "Password1!", CancellationToken.None);
        var link = fixture.EmailSender.ConfirmationLinks.Single();
        var query = new Uri("https://localhost" + link).Query.TrimStart('?')
            .Split('&', StringSplitOptions.RemoveEmptyEntries)
            .Select(part => part.Split('=', 2))
            .ToDictionary(part => part[0], part => Uri.UnescapeDataString(part[1]));

        var user = await fixture.UserManager.FindByIdAsync(query["userId"]);
        user.Should().NotBeNull();
        var changeCode = await fixture.UserManager.GenerateChangeEmailTokenAsync(user!, "new@example.com");
        (await fixture.Service.ConfirmEmailAsync(user!.Id, changeCode, "new@example.com", CancellationToken.None)).Should().BeTrue();
        (await fixture.Service.ConfirmEmailAsync(user.Id, "invalid", null, CancellationToken.None)).Should().BeFalse();
    }

    [Fact]
    public async Task Forgot_password_should_not_send_for_a_user_without_password()
    {
        await using var fixture = await IdentityFixture.CreateAsync();
        var user = new User("external@example.com") { Email = "external@example.com", EmailConfirmed = true };
        (await fixture.UserManager.CreateAsync(user)).Succeeded.Should().BeTrue();

        (await fixture.Service.ForgotPasswordAsync("external@example.com", CancellationToken.None)).Succeeded.Should().BeTrue();
        fixture.EmailSender.PasswordResetLinks.Should().BeEmpty();
    }

    [Fact]
    public async Task Reset_password_should_report_invalid_token_for_existing_user()
    {
        await using var fixture = await IdentityFixture.CreateAsync();
        await fixture.Service.InitializeSetupAsync("admin@example.com", "Password1!", CancellationToken.None);

        var result = await fixture.Service.ResetPasswordAsync("admin@example.com", "invalid", "Password2!", CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Update_info_should_cover_missing_user_wrong_password_and_no_changes()
    {
        await using var fixture = await IdentityFixture.CreateAsync();
        await fixture.Service.InitializeSetupAsync("admin@example.com", "Password1!", CancellationToken.None);
        var user = await fixture.UserManager.FindByEmailAsync("admin@example.com");
        user.Should().NotBeNull();

        (await fixture.Service.UpdateInfoAsync("missing", null, null, "Password1!", CancellationToken.None)).Succeeded.Should().BeFalse();
        (await fixture.Service.UpdateInfoAsync(user!.Id, null, null, "wrong", CancellationToken.None)).Succeeded.Should().BeFalse();
        (await fixture.Service.UpdateInfoAsync(user.Id, user.Email, null, "Password1!", CancellationToken.None)).Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task Setup_should_accept_an_existing_administrator_role_and_reject_invalid_password()
    {
        await using var fixture = await IdentityFixture.CreateAsync();
        var role = new Role("Administrator");
        (await fixture.RoleManager.CreateAsync(role)).Succeeded.Should().BeTrue();

        var invalid = await fixture.Service.InitializeSetupAsync("admin@example.com", "weak", CancellationToken.None);
        invalid.Succeeded.Should().BeFalse();

        var valid = await fixture.Service.InitializeSetupAsync("admin@example.com", "Password1!", CancellationToken.None);
        valid.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task Logging_email_sender_should_complete_both_messages()
    {
        using var provider = new ServiceCollection().AddLogging().BuildServiceProvider();
        var sender = new LoggingIdentityEmailSender(provider.GetRequiredService<ILogger<LoggingIdentityEmailSender>>());

        await sender.SendConfirmationAsync("user@example.com", "/confirm", CancellationToken.None);
        await sender.SendPasswordResetAsync("user@example.com", "/reset", CancellationToken.None);
    }
}