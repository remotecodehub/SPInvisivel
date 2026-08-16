namespace InvisibleSP.UnitTests;

public sealed class IdentityValidatorTests
{
    [Fact]
    public async Task Register_validator_should_reject_invalid_credentials()
    {
        var result = await new RegisterCommandValidator().ValidateAsync(new RegisterCommand("bad", "short"));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Login_validator_should_reject_both_two_factor_codes()
    {
        var result = await new LoginCommandValidator().ValidateAsync(
            new LoginCommand("user@example.com", "Password1!", "123456", "recovery"));
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Reset_validator_should_require_email_code_and_password()
    {
        var result = await new ResetPasswordCommandValidator().ValidateAsync(
            new ResetPasswordCommand("", "", "short"));
        result.IsValid.Should().BeFalse();
        result.Errors.Select(x => x.PropertyName).Should().Contain(["Email", "ResetCode", "NewPassword"]);
    }

    [Fact]
    public async Task Update_validator_should_allow_optional_email_and_password()
    {
        var result = await new UpdateIdentityInfoCommandValidator().ValidateAsync(
            new UpdateIdentityInfoCommand("user-id", null, null, "Password1!"));
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Initialize_setup_validator_should_validate_email_and_password()
    {
        var validator = new InitializeSetupCommandValidator();
        (await validator.ValidateAsync(new InitializeSetupCommand("admin@example.com", "Password1!"))).IsValid.Should().BeTrue();
        (await validator.ValidateAsync(new InitializeSetupCommand("invalid", "short"))).IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Simple_identity_validators_should_require_expected_fields()
    {
        (await new RefreshTokenCommandValidator().ValidateAsync(new RefreshTokenCommand(""))).IsValid.Should().BeFalse();
        (await new RevokeTokenCommandValidator().ValidateAsync(new RevokeTokenCommand(""))).IsValid.Should().BeFalse();
        (await new ConfirmEmailCommandValidator().ValidateAsync(new ConfirmEmailCommand("", ""))).IsValid.Should().BeFalse();
        (await new ResendConfirmationEmailCommandValidator().ValidateAsync(new ResendConfirmationEmailCommand("invalid"))).IsValid.Should().BeFalse();
        (await new ForgotPasswordCommandValidator().ValidateAsync(new ForgotPasswordCommand("invalid"))).IsValid.Should().BeFalse();
    }
}
