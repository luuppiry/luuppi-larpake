using System.Diagnostics.CodeAnalysis;

namespace LarpakeServer.Identity;

public readonly struct RefreshTokenValidationResult
{
    private RefreshTokenValidationResult(bool isValid, Guid? tokenFamily, Guid? userId)
    {
        IsValid = isValid;
        TokenFamily = tokenFamily;
        UserId = userId;
    }

    public RefreshTokenValidationResult(Guid tokenFamily, Guid? userId) 
        : this(true, tokenFamily, userId)
    {
    }

   


    [MemberNotNullWhen(true, nameof(TokenFamily), nameof(UserId))]
    public bool IsValid { get; }

    public Guid? TokenFamily { get; }
    public Guid? UserId { get; }

    public static readonly RefreshTokenValidationResult Invalid = new(false, null, null);

    public void Deconstruct(out bool isValid, [NotNullIfNotNull(nameof(isValid))] out Guid? tokenFamily, [NotNullIfNotNull(nameof(isValid))] out Guid? userId)
    {
        isValid = IsValid;
        tokenFamily = TokenFamily!;
        userId = UserId!;
    }
}
