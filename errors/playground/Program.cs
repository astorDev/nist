using System.Collections;
using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Nist;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();

var app = builder.Build();

// v1

// app.UseExceptionHandler(e => {
//     e.Run(async context => {
//         var writer = context.RequestServices.GetRequiredService<IProblemDetailsService>();

//         var problem = new ProblemDetails() {
//             Type = "Unexpected"
//         };

//         await writer.WriteAsync(new ProblemDetailsContext{
//             HttpContext = context,
//             ProblemDetails = problem
//         });
//     });
// });

// v2:

// app.UseExceptionHandler(e => {
//     e.Run(async context => {
//         var writer = context.RequestServices.GetRequiredService<IProblemDetailsService>();

//         var problem = new ProblemDetails() {
//             Type = "Unexpected"
//         };

//         await writer.WriteAsync(new ProblemDetailsContext{
//             HttpContext = context,
//             ProblemDetails = problem
//         });
//     });
// });

// v3:

// app.UseExceptionHandler(e => {
//     e.Run(async context => {
//         var writer = context.RequestServices.GetRequiredService<IProblemDetailsService>();
//         var exception = context.Features.GetRequiredFeature<IExceptionHandlerFeature>()!.Error;

//         var problem = ProblemFrom(exception);

//         await writer.WriteAsync(new ProblemDetailsContext{
//             HttpContext = context,
//             ProblemDetails = problem
//         });
//     });
// });

// v4:

// app.UseExceptionHandler(e => {
//     e.Run(async context => {
//         var writer = context.RequestServices.GetRequiredService<IProblemDetailsService>();
//         var exception = context.Features.GetRequiredFeature<IExceptionHandlerFeature>()!.Error;

//         var problem = ProblemFrom(exception);

//         context.Response.StatusCode = problem.Status ?? 500;

//         await writer.WriteAsync(new ProblemDetailsContext{
//             HttpContext = context,
//             ProblemDetails = problem
//         });
//     });
// });

// v5:

// app.UseExceptionHandler(e => {
//     e.Run(async context => {
//         var writer = context.RequestServices.GetRequiredService<IProblemDetailsService>();
//         var exception = context.Features.GetRequiredFeature<IExceptionHandlerFeature>()!.Error;
//         var configuration = context.RequestServices.GetRequiredService<IConfiguration>();

//         var problem = ProblemFrom(exception);
//         if (configuration.GetValue<bool>("ShowExceptions")) {
//             problem.EnrichWithExceptionDetails(exception);
//         }

//         context.Response.StatusCode = problem.Status ?? 500;

//         await writer.WriteAsync(new ProblemDetailsContext{
//             HttpContext = context,
//             ProblemDetails = problem
//         });
//     });
// });

// v6:

app.UseExceptionHandler(e => {
    e.Run(async context => {
        var writer = context.RequestServices.GetRequiredService<IProblemDetailsService>();
        var exception = context.Features.GetRequiredFeature<IExceptionHandlerFeature>()!.Error;

        var error = ErrorFrom(exception);
        var problem = new ProblemDetails {
            Type = error.Reason,
            Status = (int)error.Code
        };

        var configuration = context.RequestServices.GetRequiredService<IConfiguration>();
        if (configuration.GetValue<bool>("ShowExceptions"))
            problem.EnrichWithExceptionDetails(exception);

        context.Response.StatusCode = (int)error.Code;

        await writer.WriteAsync(new ProblemDetailsContext{
            HttpContext = context,
            ProblemDetails = problem
        });
    });
});

app.MapGet("/unknown", () => {
    throw new ("Unknown error 4");
});

app.MapGet("/wrong-input/{input}", (string input) => {
    throw new WrongInputException(input);
});

app.Run();

Error ErrorFrom(Exception exception)
{
    return exception switch {
        WrongInputException => new (HttpStatusCode.BadRequest, "WrongInput"),
        _ => new (HttpStatusCode.InternalServerError, "Unknown")
    };
}

public static class ProblemDetailsExceptionEnricher
{
    public static void EnrichWithExceptionDetails(this ProblemDetails problem, Exception exception)
    {
        problem.Extensions = new Dictionary<string, object?> {
            ["exception"] = new {
                exception.Message,
                exception.StackTrace,
                exception.Data
            }
        };
    }
}

public record Error(HttpStatusCode Code, string Reason);

class WrongInputException(string input) : Exception {
    public override IDictionary Data => new Dictionary<string, object> {
        [ "input" ] = input
    };
}



public static class ExceptionsToProblems
{
    public static IApplicationBuilder UseProblemExceptionHandler<TError>(
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


    public static IApplicationBuilder UseProblemExceptionHandler(
        this IApplicationBuilder app, 
        Func<Exception, Error> exceptionMapper,
        bool showExceptions = false)
    {
        return app.UseProblemExceptionHandler(
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