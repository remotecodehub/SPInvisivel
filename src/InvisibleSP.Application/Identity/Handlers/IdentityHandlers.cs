namespace InvisibleSP.Application.Identity.Handlers;

/// <summary>Handles user registration requests.</summary>
/// <param name="identityService">The identity service that performs registration.</param>
public sealed class RegisterCommandHandler(IIdentityService identityService) : IRequestHandler<RegisterCommand, IdentityResultResponse>
{
    /// <summary>Executes the registration request.</summary>
    /// <param name="context">The message context containing the registration request.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The registration result.</returns>
    public Task<IdentityResultResponse> Handle(IReceiveContext<RegisterCommand> context, CancellationToken cancellationToken) =>
        identityService.RegisterAsync(context.Message.Email, context.Message.Password, cancellationToken);
}

/// <summary>Handles user login requests.</summary>
/// <param name="identityService">The identity service that performs authentication.</param>
public sealed class LoginCommandHandler(IIdentityService identityService) : IRequestHandler<LoginCommand, Response<TokenResponse>>
{
    /// <summary>Executes the login request and converts authentication failure to an application response.</summary>
    /// <param name="context">The message context containing the login request.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The authentication response containing tokens when successful.</returns>
    public async Task<Response<TokenResponse>> Handle(IReceiveContext<LoginCommand> context, CancellationToken cancellationToken)
    {
        var result = await identityService.LoginAsync(context.Message.Email, context.Message.Password, context.Message.TwoFactorCode, context.Message.TwoFactorRecoveryCode, cancellationToken);
        return result is null ? Response<TokenResponse>.Failure(["Invalid credentials."]) : Response<TokenResponse>.Success(result);
    }
}

/// <summary>Handles refresh-token requests.</summary>
/// <param name="identityService">The identity service that validates and exchanges refresh tokens.</param>
public sealed class RefreshTokenCommandHandler(IIdentityService identityService) : IRequestHandler<RefreshTokenCommand, Response<TokenResponse>>
{
    /// <summary>Executes a refresh-token exchange.</summary>
    /// <param name="context">The message context containing the refresh token.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The refreshed token response.</returns>
    public async Task<Response<TokenResponse>> Handle(IReceiveContext<RefreshTokenCommand> context, CancellationToken cancellationToken)
    {
        var result = await identityService.RefreshAsync(context.Message.RefreshToken, cancellationToken);
        return result is null ? Response<TokenResponse>.Failure(["Invalid refresh token."]) : Response<TokenResponse>.Success(result);
    }
}

/// <summary>Handles access-token revocation requests.</summary>
/// <param name="identityService">The identity service that revokes tokens.</param>
public sealed class RevokeTokenCommandHandler(IIdentityService identityService) : IRequestHandler<RevokeTokenCommand, Response<bool>>
{
    /// <summary>Executes token revocation.</summary>
    /// <param name="context">The message context containing the access token.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>A response indicating whether revocation succeeded.</returns>
    public async Task<Response<bool>> Handle(IReceiveContext<RevokeTokenCommand> context, CancellationToken cancellationToken) =>
        Response<bool>.Success(await identityService.RevokeAsync(context.Message.AccessToken, cancellationToken));
}

/// <summary>Handles email-confirmation requests.</summary>
/// <param name="identityService">The identity service that confirms email addresses.</param>
public sealed class ConfirmEmailCommandHandler(IIdentityService identityService) : IRequestHandler<ConfirmEmailCommand, Response<bool>>
{
    /// <summary>Executes email confirmation.</summary>
    /// <param name="context">The message context containing the confirmation request.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>A response indicating whether confirmation succeeded.</returns>
    public async Task<Response<bool>> Handle(IReceiveContext<ConfirmEmailCommand> context, CancellationToken cancellationToken) =>
        Response<bool>.Success(await identityService.ConfirmEmailAsync(context.Message.UserId, context.Message.Code, context.Message.ChangedEmail, cancellationToken));
}

/// <summary>Handles requests to resend email confirmation.</summary>
/// <param name="identityService">The identity service that sends confirmation messages.</param>
public sealed class ResendConfirmationEmailCommandHandler(IIdentityService identityService) : IRequestHandler<ResendConfirmationEmailCommand, IdentityResultResponse>
{
    /// <summary>Executes a confirmation-email resend request.</summary>
    /// <param name="context">The message context containing the email address.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The resend result.</returns>
    public Task<IdentityResultResponse> Handle(IReceiveContext<ResendConfirmationEmailCommand> context, CancellationToken cancellationToken) =>
        identityService.ResendConfirmationEmailAsync(context.Message.Email, cancellationToken);
}

/// <summary>Handles password-recovery requests.</summary>
/// <param name="identityService">The identity service that performs password recovery.</param>
public sealed class ForgotPasswordCommandHandler(IIdentityService identityService) : IRequestHandler<ForgotPasswordCommand, IdentityResultResponse>
{
    /// <summary>Executes a password-recovery request.</summary>
    /// <param name="context">The message context containing the email address.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The recovery result.</returns>
    public Task<IdentityResultResponse> Handle(IReceiveContext<ForgotPasswordCommand> context, CancellationToken cancellationToken) =>
        identityService.ForgotPasswordAsync(context.Message.Email, cancellationToken);
}

/// <summary>Handles password-reset requests.</summary>
/// <param name="identityService">The identity service that resets passwords.</param>
public sealed class ResetPasswordCommandHandler(IIdentityService identityService) : IRequestHandler<ResetPasswordCommand, IdentityResultResponse>
{
    /// <summary>Executes a password reset.</summary>
    /// <param name="context">The message context containing the reset request.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The reset result.</returns>
    public Task<IdentityResultResponse> Handle(IReceiveContext<ResetPasswordCommand> context, CancellationToken cancellationToken) =>
        identityService.ResetPasswordAsync(context.Message.Email, context.Message.ResetCode, context.Message.NewPassword, cancellationToken);
}

/// <summary>Handles identity information queries.</summary>
/// <param name="identityService">The identity service that reads identity information.</param>
public sealed class GetIdentityInfoQueryHandler(IIdentityService identityService) : IRequestHandler<GetIdentityInfoQuery, Response<IdentityInfoResponse>>
{
    /// <summary>Executes an identity information query.</summary>
    /// <param name="context">The message context containing the user identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The identity information response.</returns>
    public async Task<Response<IdentityInfoResponse>> Handle(IReceiveContext<GetIdentityInfoQuery> context, CancellationToken cancellationToken)
    {
        var result = await identityService.GetInfoAsync(context.Message.UserId, cancellationToken);
        return result is null ? Response<IdentityInfoResponse>.Failure(["User not found."]) : Response<IdentityInfoResponse>.Success(result);
    }
}

/// <summary>Handles identity information update requests.</summary>
/// <param name="identityService">The identity service that performs the update.</param>
public sealed class UpdateIdentityInfoCommandHandler(IIdentityService identityService) : IRequestHandler<UpdateIdentityInfoCommand, IdentityResultResponse>
{
    /// <summary>Executes an identity information update.</summary>
    /// <param name="context">The message context containing the update request.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The update result.</returns>
    public Task<IdentityResultResponse> Handle(IReceiveContext<UpdateIdentityInfoCommand> context, CancellationToken cancellationToken) =>
        identityService.UpdateInfoAsync(context.Message.UserId, context.Message.NewEmail, context.Message.NewPassword, context.Message.OldPassword, cancellationToken);
}

/// <summary>Handles two-factor authentication configuration requests.</summary>
/// <param name="identityService">The identity service that configures two-factor authentication.</param>
public sealed class ConfigureTwoFactorCommandHandler(IIdentityService identityService) : IRequestHandler<ConfigureTwoFactorCommand, Response<TwoFactorResponse>>
{
    /// <summary>Executes two-factor configuration.</summary>
    /// <param name="context">The message context containing the configuration request.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The resulting two-factor configuration.</returns>
    public async Task<Response<TwoFactorResponse>> Handle(IReceiveContext<ConfigureTwoFactorCommand> context, CancellationToken cancellationToken)
    {
        var result = await identityService.ConfigureTwoFactorAsync(context.Message.UserId, context.Message.Enable, context.Message.TwoFactorCode, context.Message.ResetRecoveryCodes, context.Message.ResetSharedKey, context.Message.ForgetMachine, cancellationToken);
        return result is null ? Response<TwoFactorResponse>.Failure(["The two-factor configuration request is invalid."]) : Response<TwoFactorResponse>.Success(result);
    }
}

/// <summary>Handles requests for first-time setup status.</summary>
/// <param name="identityService">The identity service that reads setup state.</param>
public sealed class GetSetupStatusQueryHandler(IIdentityService identityService) : IRequestHandler<GetSetupStatusQuery, SetupStatusResponse>
{
    /// <summary>Reads the current setup status.</summary>
    /// <param name="context">The message context containing the query.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The current setup status.</returns>
    public Task<SetupStatusResponse> Handle(IReceiveContext<GetSetupStatusQuery> context, CancellationToken cancellationToken) =>
        identityService.GetSetupStatusAsync(cancellationToken);
}

/// <summary>Handles first-time setup initialization requests.</summary>
/// <param name="identityService">The identity service that creates the initial administrator account.</param>
public sealed class InitializeSetupCommandHandler(IIdentityService identityService) : IRequestHandler<InitializeSetupCommand, IdentityResultResponse>
{
    /// <summary>Executes first-time setup initialization.</summary>
    /// <param name="context">The message context containing setup credentials.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The setup result.</returns>
    public Task<IdentityResultResponse> Handle(IReceiveContext<InitializeSetupCommand> context, CancellationToken cancellationToken) =>
        identityService.InitializeSetupAsync(context.Message.Email, context.Message.Password, cancellationToken);
}
