using LarpakeServer.Services.Implementations;
using LarpakeServer.Services.Options;
using Microsoft.Extensions.Options;

namespace LarpakeServer.Services;

public class InviteKeyService : RandomKeyService
{
    readonly InviteKeyOptions _options;

    public int KeyLength => _options.KeyLength;

    public InviteKeyService(IOptions<InviteKeyOptions> options)
    {
        _options = options.Value;
    }

    public string GenerateKey()
    {
        return GenerateKey(stackalloc char[_options.KeyLength]).ToString();
    }
}
