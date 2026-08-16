namespace InvisibleSP.Application.Identity;

using InvisibleSP.Application.Common.Identity;

public interface IIdentityService
{
    Task<IdentityResultResponse> RegisterAsync(string email, string password, CancellationToken cancellationToken);
    Task<TokenResponse?> LoginAsync(string email, string password, string? twoFactorCode, string? twoFactorRecoveryCode, CancellationToken cancellationToken);
    Task<TokenResponse?> RefreshAsync(string refreshToken, CancellationToken cancellationToken);
    Task<bool> RevokeAsync(string accessToken, CancellationToken cancellationToken);
    Task<bool> ConfirmEmailAsync(string userId, string code, string? changedEmail, CancellationToken cancellationToken);
    Task<IdentityResultResponse> ResendConfirmationEmailAsync(string email, CancellationToken cancellationToken);
    Task<IdentityResultResponse> ForgotPasswordAsync(string email, CancellationToken cancellationToken);
    Task<IdentityResultResponse> ResetPasswordAsync(string email, string resetCode, string newPassword, CancellationToken cancellationToken);
    Task<IdentityInfoResponse?> GetInfoAsync(string userId, CancellationToken cancellationToken);
    Task<IdentityResultResponse> UpdateInfoAsync(string userId, string? newEmail, string? newPassword, string oldPassword, CancellationToken cancellationToken);
    Task<TwoFactorResponse?> ConfigureTwoFactorAsync(string userId, bool? enable, string? twoFactorCode, bool resetRecoveryCodes, bool resetSharedKey, bool forgetMachine, CancellationToken cancellationToken);
    Task<SetupStatusResponse> GetSetupStatusAsync(CancellationToken cancellationToken);
    Task<IdentityResultResponse> InitializeSetupAsync(string email, string password, CancellationToken cancellationToken);
}
