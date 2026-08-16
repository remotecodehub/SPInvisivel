namespace InvisibleSP.Controllers;

[ApiController]
public sealed class IdentityController(IMediator mediator) : ControllerBase
{
    [HttpPost("/register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.RequestAsync<RegisterCommand, IdentityResultResponse>(new RegisterCommand(request.Email, request.Password));
        return result.Succeeded ? Ok() : BadRequest(result);
    }

    [HttpPost("/login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.RequestAsync<LoginCommand, Response<TokenResponse>>(
            new LoginCommand(request.Email, request.Password, request.TwoFactorCode, request.TwoFactorRecoveryCode));
        return result.Succeeded ? Ok(result.Data) : Unauthorized(result);
    }

    [HttpPost("/refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh(RefreshRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.RequestAsync<RefreshTokenCommand, Response<TokenResponse>>(new RefreshTokenCommand(request.RefreshToken));
        return result.Succeeded ? Ok(result.Data) : Unauthorized(result);
    }

    [HttpPost("/revoke")]
    [Authorize]
    public async Task<IActionResult> Revoke(CancellationToken cancellationToken)
    {
        var accessToken = Request.Headers.Authorization.ToString().Replace("Bearer ", string.Empty, StringComparison.OrdinalIgnoreCase);
        var result = await mediator.RequestAsync<RevokeTokenCommand, Response<bool>>(new RevokeTokenCommand(accessToken));
        return result.Data == true ? Ok() : Unauthorized(result);
    }

    [HttpGet("/confirmEmail")]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmEmail(
        [FromQuery] string userId,
        [FromQuery] string code,
        [FromQuery] string? changedEmail,
        CancellationToken cancellationToken)
    {
        var result = await mediator.RequestAsync<ConfirmEmailCommand, Response<bool>>(new ConfirmEmailCommand(userId, code, changedEmail));
        return result.Data == true ? Ok("Thank you for confirming your email.") : BadRequest(result);
    }

    [HttpPost("/resendConfirmationEmail")]
    [AllowAnonymous]
    public async Task<IActionResult> ResendConfirmationEmail(EmailRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.RequestAsync<ResendConfirmationEmailCommand, IdentityResultResponse>(new ResendConfirmationEmailCommand(request.Email));
        return result.Succeeded ? Ok() : BadRequest(result);
    }

    [HttpPost("/forgotPassword")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword(EmailRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.RequestAsync<ForgotPasswordCommand, IdentityResultResponse>(new ForgotPasswordCommand(request.Email));
        return result.Succeeded ? Ok() : BadRequest(result);
    }

    [HttpPost("/resetPassword")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.RequestAsync<ResetPasswordCommand, IdentityResultResponse>(new ResetPasswordCommand(request.Email, request.ResetCode, request.NewPassword));
        return result.Succeeded ? Ok() : BadRequest(result);
    }

    [HttpGet("/manage/info")]
    [Authorize]
    public async Task<IActionResult> GetInfo(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await mediator.RequestAsync<GetIdentityInfoQuery, Response<IdentityInfoResponse>>(new GetIdentityInfoQuery(userId));
        return result.Succeeded ? Ok(result.Data) : NotFound(result);
    }

    [HttpPost("/manage/info")]
    [Authorize]
    public async Task<IActionResult> UpdateInfo(InfoRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await mediator.RequestAsync<UpdateIdentityInfoCommand, IdentityResultResponse>(
            new UpdateIdentityInfoCommand(userId, request.NewEmail, request.NewPassword, request.OldPassword));
        return result.Succeeded ? Ok() : BadRequest(result);
    }

    [HttpPost("/manage/2fa")]
    [Authorize]
    public async Task<IActionResult> ConfigureTwoFactor(TwoFactorRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await mediator.RequestAsync<ConfigureTwoFactorCommand, Response<TwoFactorResponse>>(
            new ConfigureTwoFactorCommand(userId, request.Enable, request.TwoFactorCode, request.ResetRecoveryCodes, request.ResetSharedKey, request.ForgetMachine));
        return result.Succeeded ? Ok(result.Data) : BadRequest(result);
    }

    private string? GetUserId() =>
        User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
}

public sealed record RegisterRequest(string Email, string Password);
public sealed record LoginRequest(string Email, string Password, string? TwoFactorCode = null, string? TwoFactorRecoveryCode = null);
public sealed record RefreshRequest(string RefreshToken);
public sealed record EmailRequest(string Email);
public sealed record ResetPasswordRequest(string Email, string ResetCode, string NewPassword);
public sealed record InfoRequest(string? NewEmail, string? NewPassword, string OldPassword);
public sealed record TwoFactorRequest(
    bool? Enable = null,
    string? TwoFactorCode = null,
    bool ResetRecoveryCodes = false,
    bool ResetSharedKey = false,
    bool ForgetMachine = false);
