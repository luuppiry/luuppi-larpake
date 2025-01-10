using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace LarpakeServer.Identity;

internal sealed class BearerSecuritySchemeTransformer : IOpenApiDocumentTransformer
{
    private readonly IAuthenticationSchemeProvider _provider;

    public BearerSecuritySchemeTransformer(IAuthenticationSchemeProvider provider)
    {
        _provider = provider;
    }

    public async Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        var authenticationSchemes = await _provider.GetAllSchemesAsync();

        // Validate bearer scheme exists
        if (authenticationSchemes.All(x => x.Name is not "Bearer"))
        {
            return;
        }

        var requirements = new Dictionary<string, OpenApiSecurityScheme>
        {
            ["Bearer"] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                In = ParameterLocation.Header,
                BearerFormat = "JWT",
            }
        };

        document.Components ??= new();
        document.Components.SecuritySchemes = requirements;

        var key = new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Id = "Bearer",
                Type = ReferenceType.SecurityScheme
            }
        };
        var operationRequirement = new OpenApiSecurityRequirement
        {
            [key] = []
        };

        var operations = document.Paths.Values.SelectMany(x => x.Operations);
        foreach (var (_, operation) in operations)
        {
            operation.Security.Add(operationRequirement);
        }
    }
}
