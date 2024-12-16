using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;

namespace Nist.Errors;

public static class WebApplicationExtensions
{
    public static void UseErrorBody<TError>(this IApplicationBuilder app, Func<Exception, TError> errorFactory, bool showException = true) where TError : Error
    {
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
                var exception = exceptionHandlerPathFeature!.Error;

                var error = errorFactory(exception);
                if (showException)
                {
                    error.ExceptionDetails = Error.Details.FromException(exception);
                }

                context.Response.StatusCode = (int)error.Code;
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsync(JsonSerializer.Serialize(error, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                }));
            });
        });
    }

    public static IApplicationBuilder UseProblemForExceptions<TError>(
        this IApplicationBuilder app, 
        Func<Exception, TError> exceptionMapper,
        Func<TError, HttpStatusCode> statusCodeExtractor,
        Func<TError, Exception, ProblemDetails> problemDetailsMapper)
    {
        return app.UseExceptionHandler(e => {
            e.Run(async context => {
                var exception = context.Features.GetRequiredFeature<IExceptionHandlerFeature>()!.Error;
                var writer = context.RequestServices.GetRequiredService<IProblemDetailsService>();

                var error = exceptionMapper(exception);
                var problem = problemDetailsMapper(error, exception);

                context.Response.StatusCode = (int)statusCodeExtractor(error);

                await writer.WriteAsync(new ProblemDetailsContext{
                    HttpContext = context,
                    ProblemDetails = problem
                });
            });
        });
    }


    public static IApplicationBuilder UseProblemForExceptions(
        this IApplicationBuilder app, 
        Func<Exception, Error> exceptionMapper,
        bool showExceptions = false)
    {
        return app.UseProblemForExceptions(
            exceptionMapper,
            e => e.Code,
            (error, exception) => ProblemFrom(
                error, 
                exception,
                showExceptions
            )
        );
    }

    public static ProblemDetails ProblemFrom(Error error, Exception exception, bool includeException = false)
    {
        var result =  new ProblemDetails
        {
            Type = error.Reason,
            Status = (int)error.Code
        };

        if (includeException) {
            result.Extensions = new Dictionary<string, object?> {
                ["exception"] = new {
                    exception.Message,
                    exception.StackTrace,
                    exception.Data
                }
            };
        }

        return result;
    }
}