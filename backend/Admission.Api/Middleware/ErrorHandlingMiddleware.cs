using Admission.Api.Dtos;

namespace Admission.Api.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ApiException ex)
        {
            await WriteError(context, ex.StatusCode, ex.ErrorCode, ex.Message);
        }
        catch (BadHttpRequestException ex)
        {
            await WriteError(context, StatusCodes.Status400BadRequest, "INVALID_REQUEST", "The request could not be read.");
            _logger.LogDebug(ex, "Bad request");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error");
            await WriteError(context, StatusCodes.Status500InternalServerError, "SERVER_ERROR",
                "Something went wrong on the server. Please try again.");
        }
    }

    private static async Task WriteError(HttpContext context, int status, string code, string message)
    {
        if (context.Response.HasStarted)
            return;

        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(new ErrorResponse(code, message));
    }
}
