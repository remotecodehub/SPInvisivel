namespace InvisibleSP.Application.Common.Identity;

public static class IdentityClaimTypes
{
    public const string Permission = "permission";
}

public static class IdentityPolicies
{
    public const string Setup = "setup";
}

public sealed record TokenResponse(
    string TokenType,
    string AccessToken,
    int ExpiresIn,
    string RefreshToken);

public sealed record IdentityResultResponse(bool Succeeded, IReadOnlyCollection<string> Errors)
{
    public static IdentityResultResponse Success() => new(true, Array.Empty<string>());

    public static IdentityResultResponse Failure(IEnumerable<string> errors) =>
        new(false, errors.ToArray());
}

public sealed record IdentityInfoResponse(string Email, bool IsEmailConfirmed);

public sealed record TwoFactorResponse(
    string? SharedKey,
    int RecoveryCodesLeft,
    IReadOnlyCollection<string>? RecoveryCodes,
    bool IsTwoFactorEnabled,
    bool IsMachineRemembered);

public sealed record SetupStatusResponse(bool IsSetupRequired, bool IsSetupComplete);
