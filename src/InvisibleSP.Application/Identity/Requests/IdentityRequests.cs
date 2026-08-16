namespace InvisibleSP.Application.Identity.Requests;

public sealed record RegisterCommand(string Email, string Password) : IRequest;

public sealed record LoginCommand(
    string Email,
    string Password,
    string? TwoFactorCode = null,
    string? TwoFactorRecoveryCode = null) : IRequest;

public sealed record RefreshTokenCommand(string RefreshToken) : IRequest;

public sealed record RevokeTokenCommand(string AccessToken) : IRequest;

public sealed record ConfirmEmailCommand(string UserId, string Code, string? ChangedEmail = null) : IRequest;

public sealed record ResendConfirmationEmailCommand(string Email) : IRequest;

public sealed record ForgotPasswordCommand(string Email) : IRequest;

public sealed record ResetPasswordCommand(string Email, string ResetCode, string NewPassword) : IRequest;

public sealed record GetIdentityInfoQuery(string UserId) : IRequest;

public sealed record UpdateIdentityInfoCommand(
    string UserId,
    string? NewEmail,
    string? NewPassword,
    string OldPassword) : IRequest;

public sealed record ConfigureTwoFactorCommand(
    string UserId,
    bool? Enable,
    string? TwoFactorCode,
    bool ResetRecoveryCodes,
    bool ResetSharedKey,
    bool ForgetMachine) : IRequest;

public sealed record GetSetupStatusQuery : IRequest;

public sealed record InitializeSetupCommand(string Email, string Password) : IRequest;
