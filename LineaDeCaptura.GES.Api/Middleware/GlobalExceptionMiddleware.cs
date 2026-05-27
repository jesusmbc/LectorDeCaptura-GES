using LineaDeCaptura.GES.Api.Data;

namespace LineaDeCaptura.GES.Api.Middleware;

public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public GlobalExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IGesRepository repository)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            Guid? transactionGuid = null;
            if (context.Request.RouteValues.TryGetValue("transactionGuid", out var routeValue) && routeValue != null && Guid.TryParse(routeValue.ToString(), out var parsed))
            {
                transactionGuid = parsed;
            }

            await repository.InsertOperationLogAsync(
                transactionGuid,
                null,
                "ERROR",
                "GlobalException",
                "UNHANDLED_EXCEPTION",
                ex.Message,
                ex.ToString(),
                context.Request.Path,
                context.Request.Method,
                context.Connection.RemoteIpAddress?.ToString(),
                context.RequestAborted);

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new
            {
                notification = new
                {
                    tipoSeveridad = 3,
                    mensajeUsuario = "Ocurrio un error al procesar la operacion",
                    mensajeProgramador = ex.Message,
                    severidad = "error"
                },
                data = (object?)null
            });
        }
    }
}
