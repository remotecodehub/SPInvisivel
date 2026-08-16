namespace InvisibleSP.Infrastructure.Identity.Services;

public sealed class IdentityService(
    UserManager<User> userManager,
    RoleManager<Role> roleManager,
    SignInManager<User> signInManager,
    IJwtTokenService tokenService,
    IRevokedTokenStore revokedTokenStore,
    IIdentityEmailSender emailSender,
    ILogger<IdentityService> logger) : IIdentityService
{
    private const string AdministratorRole = "Administrator";
    private const string AdministratorPermission = "system.admin";

    public async Task<IdentityResultResponse> RegisterAsync(string email, string password, CancellationToken cancellationToken)
    {
        var user = new User(email)
        {
            Email = email,
            DisplayName = "",
            FirstName = "",
            SurName = "",
            EmailConfirmed = false
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            return Failure(result);
        }

        var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
        await emailSender.SendConfirmationAsync(
            email,
            $"/confirmEmail?userId={Uri.EscapeDataString(user.Id)}&code={Uri.EscapeDataString(code)}",
            cancellationToken);

        return IdentityResultResponse.Success();
    }

    public async Task<TokenResponse?> LoginAsync(
        string email,
        string password,
        string? twoFactorCode,
        string? twoFactorRecoveryCode,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return null;
        }

        var passwordResult = await signInManager.CheckPasswordSignInAsync(user, password, true);
        if (passwordResult.IsLockedOut || passwordResult.IsNotAllowed)
        {
            return null;
        }

        if (passwordResult.RequiresTwoFactor)
        {
            var valid = !string.IsNullOrWhiteSpace(twoFactorCode)
                ? await userManager.VerifyTwoFactorTokenAsync(user, userManager.Options.Tokens.AuthenticatorTokenProvider, twoFactorCode)
                : !string.IsNullOrWhiteSpace(twoFactorRecoveryCode) &&
                  (await userManager.RedeemTwoFactorRecoveryCodeAsync(user, twoFactorRecoveryCode)).Succeeded;

            if (!valid)
            {
                return null;
            }
        }
        else if (!passwordResult.Succeeded)
        {
            return null;
        }

        return await CreateUserTokensAsync(user, cancellationToken);
    }

    public async Task<TokenResponse?> RefreshAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var principal = tokenService.ValidateToken(refreshToken);
        if (principal is null || !string.Equals(principal.FindFirstValue(JwtRegisteredClaimNames.Typ), "refresh", StringComparison.Ordinal))
        {
            return null;
        }

        var userId = principal.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return null;
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return null;
        }

        var tokenId = tokenService.GetTokenId(refreshToken);
        var expiration = tokenService.GetExpiration(refreshToken);
        var tokens = await CreateUserTokensAsync(user, cancellationToken);

        if (!string.IsNullOrWhiteSpace(tokenId) && expiration.HasValue)
        {
            revokedTokenStore.Revoke(tokenId, expiration.Value);
        }

        return tokens;
    }

    public Task<bool> RevokeAsync(string accessToken, CancellationToken cancellationToken)
    {
        var principal = tokenService.ValidateToken(accessToken);
        var tokenId = tokenService.GetTokenId(accessToken);
        var expiration = tokenService.GetExpiration(accessToken);

        if (principal is null || string.IsNullOrWhiteSpace(tokenId) || !expiration.HasValue)
        {
            return Task.FromResult(false);
        }

        revokedTokenStore.Revoke(tokenId, expiration.Value);
        return Task.FromResult(true);
    }

    public async Task<bool> ConfirmEmailAsync(string userId, string code, string? changedEmail, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return false;
        }

        var result = !string.IsNullOrWhiteSpace(changedEmail)
            ? await userManager.ChangeEmailAsync(user, changedEmail, code)
            : await userManager.ConfirmEmailAsync(user, code);

        return result.Succeeded;
    }

    public async Task<IdentityResultResponse> ResendConfirmationEmailAsync(string email, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null || await userManager.IsEmailConfirmedAsync(user))
        {
            return IdentityResultResponse.Success();
        }

        var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
        await emailSender.SendConfirmationAsync(
            email,
            $"/confirmEmail?userId={Uri.EscapeDataString(user.Id)}&code={Uri.EscapeDataString(code)}",
            cancellationToken);

        return IdentityResultResponse.Success();
    }

    public async Task<IdentityResultResponse> ForgotPasswordAsync(string email, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null || !await userManager.HasPasswordAsync(user))
        {
            return IdentityResultResponse.Success();
        }

        var code = await userManager.GeneratePasswordResetTokenAsync(user);
        await emailSender.SendPasswordResetAsync(
            email,
            $"/resetPassword?email={Uri.EscapeDataString(email)}&code={Uri.EscapeDataString(code)}",
            cancellationToken);

        return IdentityResultResponse.Success();
    }

    public async Task<IdentityResultResponse> ResetPasswordAsync(
        string email,
        string resetCode,
        string newPassword,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return IdentityResultResponse.Failure(["Invalid password reset request."]);
        }

        var result = await userManager.ResetPasswordAsync(user, resetCode, newPassword);
        return result.Succeeded ? IdentityResultResponse.Success() : Failure(result);
    }

    public async Task<IdentityInfoResponse?> GetInfoAsync(string userId, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId);
        return user is null
            ? null
            : new IdentityInfoResponse(user.Email ?? string.Empty, await userManager.IsEmailConfirmedAsync(user));
    }

    public async Task<IdentityResultResponse> UpdateInfoAsync(
        string userId,
        string? newEmail,
        string? newPassword,
        string oldPassword,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null || !await userManager.CheckPasswordAsync(user, oldPassword))
        {
            return IdentityResultResponse.Failure(["The current credentials are invalid."]);
        }

        if (!string.IsNullOrWhiteSpace(newEmail) && !string.Equals(user.Email, newEmail, StringComparison.OrdinalIgnoreCase))
        {
            var emailCode = await userManager.GenerateChangeEmailTokenAsync(user, newEmail);
            var emailResult = await userManager.ChangeEmailAsync(user, newEmail, emailCode);
            if (!emailResult.Succeeded)
            {
                return Failure(emailResult);
            }
        }

        if (!string.IsNullOrWhiteSpace(newPassword))
        {
            var passwordResult = await userManager.ChangePasswordAsync(user, oldPassword, newPassword);
            if (!passwordResult.Succeeded)
            {
                return Failure(passwordResult);
            }
        }

        return IdentityResultResponse.Success();
    }

    public async Task<TwoFactorResponse?> ConfigureTwoFactorAsync(
        string userId,
        bool? enable,
        string? twoFactorCode,
        bool resetRecoveryCodes,
        bool resetSharedKey,
        bool forgetMachine,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return null;
        }

        if (enable == true)
        {
            if (string.IsNullOrWhiteSpace(twoFactorCode) ||
                !await userManager.VerifyTwoFactorTokenAsync(user, userManager.Options.Tokens.AuthenticatorTokenProvider, twoFactorCode))
            {
                return null;
            }

            await userManager.SetTwoFactorEnabledAsync(user, true);
        }
        else if (enable == false || resetSharedKey)
        {
            await userManager.SetTwoFactorEnabledAsync(user, false);
        }

        if (resetSharedKey)
        {
            await userManager.ResetAuthenticatorKeyAsync(user);
        }

        string[]? recoveryCodes = null;
        if (resetRecoveryCodes || (enable == true && await userManager.CountRecoveryCodesAsync(user) == 0))
        {
            recoveryCodes = (await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10))?.ToArray();
        }

        var key = await userManager.GetAuthenticatorKeyAsync(user);
        if (string.IsNullOrWhiteSpace(key))
        {
            await userManager.ResetAuthenticatorKeyAsync(user);
            key = await userManager.GetAuthenticatorKeyAsync(user);
        }

        var recoveryCodesLeft = await userManager.CountRecoveryCodesAsync(user);
        return new TwoFactorResponse(
            key,
            recoveryCodesLeft,
            recoveryCodes,
            await userManager.GetTwoFactorEnabledAsync(user),
            false);
    }

    public async Task<SetupStatusResponse> GetSetupStatusAsync(CancellationToken cancellationToken)
    {
        var hasUsers = await userManager.Users.AsNoTracking().AnyAsync(cancellationToken);
        return new SetupStatusResponse(!hasUsers, hasUsers);
    }

    public async Task<IdentityResultResponse> InitializeSetupAsync(string email, string password, CancellationToken cancellationToken)
    {
        if (await userManager.Users.AsNoTracking().AnyAsync(cancellationToken))
        {
            return IdentityResultResponse.Failure(["The system setup has already been completed."]);
        }

        var role = await roleManager.FindByNameAsync(AdministratorRole);
        if (role is null)
        {
            role = new Role(AdministratorRole);
            var roleResult = await roleManager.CreateAsync(role);
            if (!roleResult.Succeeded)
            {
                return Failure(roleResult);
            }

            var claimResult = await roleManager.AddClaimAsync(role, new Claim(IdentityClaimTypes.Permission, AdministratorPermission));
            if (!claimResult.Succeeded)
            {
                return Failure(claimResult);
            }
        }

        var user = new User(email)
        {
            Email = email,
            EmailConfirmed = true
        };

        var userResult = await userManager.CreateAsync(user, password);
        if (!userResult.Succeeded)
        {
            return Failure(userResult);
        }

        var membershipResult = await userManager.AddToRoleAsync(user, AdministratorRole);
        if (!membershipResult.Succeeded)
        {
            return Failure(membershipResult);
        }

        logger.LogInformation("Initial system setup completed for user {UserId}.", user.Id);
        return IdentityResultResponse.Success();
    }

    private async Task<TokenResponse> CreateUserTokensAsync(User user, CancellationToken cancellationToken)
    {
        var roles = await userManager.GetRolesAsync(user);
        var claims = await userManager.GetClaimsAsync(user);

        foreach (var roleName in roles)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role is null)
            {
                continue;
            }

            claims = claims.Concat(await roleManager.GetClaimsAsync(role)).ToList();
        }

        return tokenService.CreateTokens(user.Id, user.Email ?? user.UserName ?? user.Id, roles, claims);
    }

    private static IdentityResultResponse Failure(IdentityResult result) =>
        IdentityResultResponse.Failure(result.Errors.Select(error => error.Description));
}

public sealed class LoggingIdentityEmailSender(ILogger<LoggingIdentityEmailSender> logger) : IIdentityEmailSender
{
    public Task SendConfirmationAsync(string email, string confirmationLink, CancellationToken cancellationToken)
    {
        logger.LogInformation("Identity confirmation link for {Email}: {Link}", email, confirmationLink);
        return Task.CompletedTask;
    }

    public Task SendPasswordResetAsync(string email, string resetLink, CancellationToken cancellationToken)
    {
        logger.LogInformation("Identity password reset link for {Email}: {Link}", email, resetLink);
        return Task.CompletedTask;
    }
}
