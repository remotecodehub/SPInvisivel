namespace InvisibleSP.Application.Identity.Validators;

public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
    }
}

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
        RuleFor(x => x).Must(x => string.IsNullOrWhiteSpace(x.TwoFactorCode) || string.IsNullOrWhiteSpace(x.TwoFactorRecoveryCode))
            .WithMessage("Only one two-factor authentication code may be supplied.");
    }
}

public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator() => RuleFor(x => x.RefreshToken).NotEmpty();
}

public sealed class RevokeTokenCommandValidator : AbstractValidator<RevokeTokenCommand>
{
    public RevokeTokenCommandValidator() => RuleFor(x => x.AccessToken).NotEmpty();
}

public sealed class ConfirmEmailCommandValidator : AbstractValidator<ConfirmEmailCommand>
{
    public ConfirmEmailCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Code).NotEmpty();
    }
}

public sealed class ResendConfirmationEmailCommandValidator : AbstractValidator<ResendConfirmationEmailCommand>
{
    public ResendConfirmationEmailCommandValidator() => RuleFor(x => x.Email).NotEmpty().EmailAddress();
}

public sealed class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator() => RuleFor(x => x.Email).NotEmpty().EmailAddress();
}

public sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.ResetCode).NotEmpty();
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(8);
    }
}

public sealed class UpdateIdentityInfoCommandValidator : AbstractValidator<UpdateIdentityInfoCommand>
{
    public UpdateIdentityInfoCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.OldPassword).NotEmpty();
        RuleFor(x => x.NewEmail).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.NewEmail));
        RuleFor(x => x.NewPassword).MinimumLength(8).When(x => !string.IsNullOrWhiteSpace(x.NewPassword));
    }
}

public sealed class InitializeSetupCommandValidator : AbstractValidator<InitializeSetupCommand>
{
    public InitializeSetupCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
    }
}
