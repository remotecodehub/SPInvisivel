namespace InvisibleSP.Application.Common.Identity;

public static class IdentityClaimTypes
{
    public const string Permission = "permission";
}

public static class IdentityPolicies
{
    public const string Administrator = "administrator";
}

public sealed record TokenResponse(
    string TokenType,
    string AccessToken,
    int ExpiresIn,
    string RefreshToken) : IResponse;

public sealed record IdentityResultResponse(bool Succeeded, IReadOnlyCollection<string> Errors) : IResponse
{
    public static IdentityResultResponse Success() => new(true, Array.Empty<string>());

    public static IdentityResultResponse Failure(IEnumerable<string> errors) =>
        new(false, errors.ToArray());
}

public sealed record BoolResponse(bool Value) : IResponse;

public sealed record IdentityInfoResponse(string Email, bool IsEmailConfirmed) : IResponse;

public sealed record TwoFactorResponse(
    string? SharedKey,
    int RecoveryCodesLeft,
    IReadOnlyCollection<string>? RecoveryCodes,
    bool IsTwoFactorEnabled,
    bool IsMachineRemembered) : IResponse;

public sealed record SetupStatusResponse(bool IsSetupRequired, bool IsSetupComplete) : IResponse;
