namespace InvisibleSP.Application.Common.Identity;

public static class IdentityClaimTypes
{
    public const string Permission = "permission";
}

public static class IdentityPolicies
{
    public const string Administrator = "administrator";
}

public sealed record Response<T>(bool Succeeded, T? Data, IReadOnlyCollection<string> Errors) : IResponse
{
    public static Response<T> Success(T data) => new(true, data, Array.Empty<string>());

    public static Response<T> Failure(IEnumerable<string> errors) => new(false, default, errors.ToArray());
}

public sealed record TokenResponse(
    string TokenType,
    string AccessToken,
    int ExpiresIn,
    string RefreshToken);

public sealed record IdentityResultResponse(bool Succeeded, IReadOnlyCollection<string> Errors) : IResponse
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

public sealed record SetupStatusResponse(bool IsSetupRequired, bool IsSetupComplete) : IResponse;
