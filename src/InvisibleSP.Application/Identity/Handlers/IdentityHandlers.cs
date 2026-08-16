namespace InvisibleSP.Application.Identity.Handlers;

public sealed class RegisterCommandHandler(IIdentityService identityService) : IRequestHandler<RegisterCommand, IdentityResultResponse>
{
    public Task<IdentityResultResponse> Handle(IReceiveContext<RegisterCommand> context, CancellationToken cancellationToken) =>
        identityService.RegisterAsync(context.Message.Email, context.Message.Password, cancellationToken);
}

public sealed class LoginCommandHandler(IIdentityService identityService) : IRequestHandler<LoginCommand, Response<TokenResponse>>
{
    public async Task<Response<TokenResponse>> Handle(IReceiveContext<LoginCommand> context, CancellationToken cancellationToken)
    {
        var result = await identityService.LoginAsync(context.Message.Email, context.Message.Password, context.Message.TwoFactorCode, context.Message.TwoFactorRecoveryCode, cancellationToken);
        return result is null ? Response<TokenResponse>.Failure(["Invalid credentials."]) : Response<TokenResponse>.Success(result);
    }
}

public sealed class RefreshTokenCommandHandler(IIdentityService identityService) : IRequestHandler<RefreshTokenCommand, Response<TokenResponse>>
{
    public async Task<Response<TokenResponse>> Handle(IReceiveContext<RefreshTokenCommand> context, CancellationToken cancellationToken)
    {
        var result = await identityService.RefreshAsync(context.Message.RefreshToken, cancellationToken);
        return result is null ? Response<TokenResponse>.Failure(["Invalid refresh token."]) : Response<TokenResponse>.Success(result);
    }
}

public sealed class RevokeTokenCommandHandler(IIdentityService identityService) : IRequestHandler<RevokeTokenCommand, Response<bool>>
{
    public async Task<Response<bool>> Handle(IReceiveContext<RevokeTokenCommand> context, CancellationToken cancellationToken) =>
        Response<bool>.Success(await identityService.RevokeAsync(context.Message.AccessToken, cancellationToken));
}

public sealed class ConfirmEmailCommandHandler(IIdentityService identityService) : IRequestHandler<ConfirmEmailCommand, Response<bool>>
{
    public async Task<Response<bool>> Handle(IReceiveContext<ConfirmEmailCommand> context, CancellationToken cancellationToken) =>
        Response<bool>.Success(await identityService.ConfirmEmailAsync(context.Message.UserId, context.Message.Code, context.Message.ChangedEmail, cancellationToken));
}

public sealed class ResendConfirmationEmailCommandHandler(IIdentityService identityService) : IRequestHandler<ResendConfirmationEmailCommand, IdentityResultResponse>
{
    public Task<IdentityResultResponse> Handle(IReceiveContext<ResendConfirmationEmailCommand> context, CancellationToken cancellationToken) =>
        identityService.ResendConfirmationEmailAsync(context.Message.Email, cancellationToken);
}

public sealed class ForgotPasswordCommandHandler(IIdentityService identityService) : IRequestHandler<ForgotPasswordCommand, IdentityResultResponse>
{
    public Task<IdentityResultResponse> Handle(IReceiveContext<ForgotPasswordCommand> context, CancellationToken cancellationToken) =>
        identityService.ForgotPasswordAsync(context.Message.Email, cancellationToken);
}

public sealed class ResetPasswordCommandHandler(IIdentityService identityService) : IRequestHandler<ResetPasswordCommand, IdentityResultResponse>
{
    public Task<IdentityResultResponse> Handle(IReceiveContext<ResetPasswordCommand> context, CancellationToken cancellationToken) =>
        identityService.ResetPasswordAsync(context.Message.Email, context.Message.ResetCode, context.Message.NewPassword, cancellationToken);
}

public sealed class GetIdentityInfoQueryHandler(IIdentityService identityService) : IRequestHandler<GetIdentityInfoQuery, Response<IdentityInfoResponse>>
{
    public async Task<Response<IdentityInfoResponse>> Handle(IReceiveContext<GetIdentityInfoQuery> context, CancellationToken cancellationToken)
    {
        var result = await identityService.GetInfoAsync(context.Message.UserId, cancellationToken);
        return result is null ? Response<IdentityInfoResponse>.Failure(["User not found."]) : Response<IdentityInfoResponse>.Success(result);
    }
}

public sealed class UpdateIdentityInfoCommandHandler(IIdentityService identityService) : IRequestHandler<UpdateIdentityInfoCommand, IdentityResultResponse>
{
    public Task<IdentityResultResponse> Handle(IReceiveContext<UpdateIdentityInfoCommand> context, CancellationToken cancellationToken) =>
        identityService.UpdateInfoAsync(context.Message.UserId, context.Message.NewEmail, context.Message.NewPassword, context.Message.OldPassword, cancellationToken);
}

public sealed class ConfigureTwoFactorCommandHandler(IIdentityService identityService) : IRequestHandler<ConfigureTwoFactorCommand, Response<TwoFactorResponse>>
{
    public async Task<Response<TwoFactorResponse>> Handle(IReceiveContext<ConfigureTwoFactorCommand> context, CancellationToken cancellationToken)
    {
        var result = await identityService.ConfigureTwoFactorAsync(context.Message.UserId, context.Message.Enable, context.Message.TwoFactorCode, context.Message.ResetRecoveryCodes, context.Message.ResetSharedKey, context.Message.ForgetMachine, cancellationToken);
        return result is null ? Response<TwoFactorResponse>.Failure(["The two-factor configuration request is invalid."]) : Response<TwoFactorResponse>.Success(result);
    }
}

public sealed class GetSetupStatusQueryHandler(IIdentityService identityService) : IRequestHandler<GetSetupStatusQuery, SetupStatusResponse>
{
    public Task<SetupStatusResponse> Handle(IReceiveContext<GetSetupStatusQuery> context, CancellationToken cancellationToken) =>
        identityService.GetSetupStatusAsync(cancellationToken);
}

public sealed class InitializeSetupCommandHandler(IIdentityService identityService) : IRequestHandler<InitializeSetupCommand, IdentityResultResponse>
{
    public Task<IdentityResultResponse> Handle(IReceiveContext<InitializeSetupCommand> context, CancellationToken cancellationToken) =>
        identityService.InitializeSetupAsync(context.Message.Email, context.Message.Password, cancellationToken);
}
