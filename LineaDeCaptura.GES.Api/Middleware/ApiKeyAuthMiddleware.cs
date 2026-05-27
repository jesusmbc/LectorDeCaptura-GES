using LineaDeCaptura.GES.Api.Data;
using LineaDeCaptura.GES.Api.Options;
using Microsoft.Extensions.Options;

namespace LineaDeCaptura.GES.Api.Middleware;

public sealed class ApiKeyAuthMiddleware
{
    private readonly RequestDelegate _next;

    public ApiKeyAuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IOptions<SecurityOptions> securityOptions, IGesRepository repository)
    {
        if (!context.Request.Path.StartsWithSegments("/api/pos", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        var headerName = securityOptions.Value.ApiKeyHeaderName;
        if (!context.Request.Headers.TryGetValue(headerName, out var apiKeyValues))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new
            {
                notification = new
                {
                    tipoSeveridad = 3,
                    mensajeUsuario = "Api key is required",
                    mensajeProgramador = "Missing api key header",
                    severidad = "error"
                },
                data = (object?)null
            });
            return;
        }

        var apiKey = apiKeyValues.ToString();
        var isValid = await repository.ValidateApiKeyAsync(apiKey, context.RequestAborted);
        if (!isValid)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new
            {
                notification = new
                {
                    tipoSeveridad = 3,
                    mensajeUsuario = "Api key is invalid",
                    mensajeProgramador = "Api key not found or disabled",
                    severidad = "error"
                },
                data = (object?)null
            });
            return;
        }

        await _next(context);
    }
}
