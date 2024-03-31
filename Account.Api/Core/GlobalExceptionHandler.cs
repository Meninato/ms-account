﻿using Microsoft.AspNetCore.Diagnostics;

namespace Account.Api.Core;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, 
        Exception exception, CancellationToken cancellationToken)
    {
        var response = httpContext.Response;
        ErrorResponse errorResponse;

        switch (exception)
        {
            case AppException e:
                response.StatusCode = e.StatusCode;
                errorResponse = ErrorResponse.From(e);
                break;
            default:
                _logger.LogError(exception, exception.Message);
                response.StatusCode = StatusCodes.Status500InternalServerError;
                errorResponse = ErrorResponse.From(exception);
                break;
        }

        await response.WriteAsJsonAsync(errorResponse, cancellationToken);

        return true;
    }
}
