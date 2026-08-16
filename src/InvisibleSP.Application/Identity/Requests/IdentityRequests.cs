namespace InvisibleSP.Application.Identity.Requests;

public sealed record RegisterCommand(string Email, string Password) : IRequest<IdentityResultResponse>;

public sealed record LoginCommand(
    string Email,
    string Password,
    string? TwoFactorCode = null,
    string? TwoFactorRecoveryCode = null) : IRequest<TokenResponse?>;

public sealed record RefreshTokenCommand(string RefreshToken) : IRequest<TokenResponse?>;

public sealed record RevokeTokenCommand(string AccessToken) : IRequest<bool>;

public sealed record ConfirmEmailCommand(string UserId, string Code, string? ChangedEmail = null) : IRequest<bool>;

public sealed record ResendConfirmationEmailCommand(string Email) : IRequest<IdentityResultResponse>;

public sealed record ForgotPasswordCommand(string Email) : IRequest<IdentityResultResponse>;

public sealed record ResetPasswordCommand(string Email, string ResetCode, string NewPassword) : IRequest<IdentityResultResponse>;

public sealed record GetIdentityInfoQuery(string UserId) : IRequest<IdentityInfoResponse?>;

public sealed record UpdateIdentityInfoCommand(
    string UserId,
    string? NewEmail,
    string? NewPassword,
    string OldPassword) : IRequest<IdentityResultResponse>;

public sealed record ConfigureTwoFactorCommand(
    string UserId,
    bool? Enable,
    string? TwoFactorCode,
    bool ResetRecoveryCodes,
    bool ResetSharedKey,
    bool ForgetMachine) : IRequest<TwoFactorResponse?>;

public sealed record GetSetupStatusQuery : IRequest<SetupStatusResponse>;

public sealed record InitializeSetupCommand(string Email, string Password) : IRequest<IdentityResultResponse>;
