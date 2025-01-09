using System.Diagnostics.CodeAnalysis;

namespace LarpakeServer.Helpers;

public readonly struct RefreshTokenValidationResult
{
    private RefreshTokenValidationResult(bool isValid, Guid? tokenFamily)
    {
        IsValid = isValid;
        TokenFamily = tokenFamily;
    }

    public RefreshTokenValidationResult(Guid tokenFamily)
    {
        IsValid = true;
        TokenFamily = tokenFamily;
    }

    [MemberNotNullWhen(true, nameof(TokenFamily))]
    public bool IsValid { get; }

    public Guid? TokenFamily { get; }

    public static readonly RefreshTokenValidationResult Invalid = new(false, null);

    public void Deconstruct(out bool isValid, [NotNullIfNotNull(nameof(isValid))] out Guid? tokenFamily)
    {
        isValid = IsValid;
        tokenFamily = TokenFamily!;
    }
}
