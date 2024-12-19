# Exception Handling in ASP .NET Core

Building an exception-handling solution in ASP .NET Core can make your head spin. Although the tooling is pretty comprehensive it's surprisingly hard to find a single ready-to-go setup. In this article, we are going to explore the tools Microsoft provides and build a solution that can be reused for any WebApi you build.

> Or jump straight to [the end](#bonus-section-using-nist-nuget-package) for the TLDR;

## Setting Up the Project

```sh
dotnet new web
```

```sh
app.MapGet("/unknown", () => {
    throw new ("Unknown error");
});
```

`dotnet run`

`curl localhost:5090/unknown`

```text
System.Exception: Unknown error
   at Program.<>c.<<Main>$>b__0_1() in /Users/egortarasov/repos/nist/errors/playground/Program.cs:line 8
   at lambda_method1(Closure, Object, HttpContext)
   at Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddlewareImpl.Invoke(HttpContext context)

HEADERS
=======
Accept: */*
Connection: close
Host: localhost:5090
User-Agent: httpyac
Accept-Encoding: gzip, deflate, br
Content-Type: application/json
```

## Utilizing Problem Details

```cs
builder.Services.AddProblemDetails();
```

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.6.1",
  "title": "System.Exception",
  "status": 500,
  "detail": "Unknown error",
  "exception": {
    "details": "System.Exception: Unknown error\n   at Program.<>c.<<Main>$>b__0_0() in /Users/egortarasov/repos/nist/errors/playground/Program.cs:line 66\n   at lambda_method1(Closure, Object, HttpContext)\n   at Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddlewareImpl.Invoke(HttpContext context)",
    "headers": {
      "Accept": [
        "*/*"
      ],
      "Connection": [
        "close"
      ],
      "Host": [
        "localhost:5090"
      ],
      "User-Agent": [
        "httpyac"
      ],
      "Accept-Encoding": [
        "gzip, deflate, br"
      ],
      "Content-Type": [
        "application/json"
      ]
    },
    "path": "/unknown",
    "endpoint": "HTTP: GET /unknown",
    "routeValues": {}
  },
  "traceId": "00-62392eb524e7b0214d1c8075834b875a-b2a7d703e4310301-00"
}
```

```cs
app.UseExceptionHandler(e => {
    e.Run(async context => {
        var writer = context.RequestServices.GetRequiredService<IProblemDetailsService>();

        var problem = new ProblemDetails() {
            Type = "Unexpected"
        };

        await writer.WriteAsync(new ProblemDetailsContext{
            HttpContext = context,
            ProblemDetails = problem
        });
    });
});
```

```json
{
  "type": "Unexpected",
  "title": "An error occurred while processing your request.",
  "status": 500,
  "traceId": "00-aba6a27507178cf0fda78a7fc88678b9-35a5c87fadf74533-00"
}
```

## Different Errors for Different Exceptions

```csharp
app.MapGet("/wrong-input/{input}", (string input) => {
    throw new WrongInputException(input);
});
```

Of course, if we access the new endpoint we will get the same error as before. 

```csharp
var exception = context.Features.GetRequiredFeature<IExceptionHandlerFeature>()!.Error;
```

```csharp
ProblemDetails ProblemFrom(Exception exception)
{
    return exception switch {
        WrongInputException => new ProblemDetails() {
            Type = "WrongInput",
            Status = (int)HttpStatusCode.BadRequest
        },
        _ => new ProblemDetails() {
            Type = "Unexpected",
            Status = (int)HttpStatusCode.InternalServerError
        }
    };
}
```

```csharp
app.UseExceptionHandler(e => {
    e.Run(async context => {
        var writer = context.RequestServices.GetRequiredService<IProblemDetailsService>();
        var exception = context.Features.GetRequiredFeature<IExceptionHandlerFeature>()!.Error;

        var problem = ProblemFrom(exception);

        await writer.WriteAsync(new ProblemDetailsContext{
            HttpContext = context,
            ProblemDetails = problem
        });
    });
});
```

```json
{
  "type": "WrongInput",
  "title": "Bad Request",
  "status": 400,
  "traceId": "00-93da29fb4bb684d23789ac5def28cce6-ff5767ba877dfae6-00"
}
```

But the actual request code will still be 500!

```csharp
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
```

```csharp
app.UseExceptionHandler(e => {
    e.Run(async context => {
        var writer = context.RequestServices.GetRequiredService<IProblemDetailsService>();
        var exception = context.Features.GetRequiredFeature<IExceptionHandlerFeature>()!.Error;

        var problem = ProblemFrom(exception);
        problem.EnrichWithExceptionDetails(exception);

        context.Response.StatusCode = problem.Status ?? 500;

        await writer.WriteAsync(new ProblemDetailsContext{
            HttpContext = context,
            ProblemDetails = problem
        });
    });
});
```

## Creating a Better Error Model

```cs
public record Error(HttpStatusCode Code, string Reason);
```

```cs
Error ErrorFrom(Exception exception)
{
    return exception switch {
        WrongInputException => new (HttpStatusCode.BadRequest, "WrongInput"),
        _ => new (HttpStatusCode.InternalServerError, "Unknown")
    };
}
```

```cs
app.UseExceptionHandler(e => {
    e.Run(async context => {
        var writer = context.RequestServices.GetRequiredService<IProblemDetailsService>();
        var exception = context.Features.GetRequiredFeature<IExceptionHandlerFeature>()!.Error;

        var error = ErrorFrom(exception);
        var problem = new ProblemDetails {
            Type = error.Reason,
            Status = (int)error.Code
        };
        problem.EnrichWithExceptionDetails(exception);

        context.Response.StatusCode = (int)error.Code;

        await writer.WriteAsync(new ProblemDetailsContext{
            HttpContext = context,
            ProblemDetails = problem
        });
    });
});
```

## Bonus Section: Using Nist Nuget Package

The package we can use to get the exception handling identical to the one we created in this article is called `Nist.Errors`. After installing it like this:

```sh
dotnet add package Nist.Errors
```

We could use its extension method `UseProblemForExceptions` to map exceptions to its built-in error type. Of course, we would also import the method by `using Nist;`. Also, don't forget to `AddProblemDetails`, which is a building block of the handler. So in the end our `Program.cs` should look like this:

```csharp
using Nist;

// ...

builder.Services.AddProblemDetails();

// ...

app.UseProblemForExceptions(
    ex => ex switch {
        WrongInputException => new (HttpStatusCode.BadRequest, "WrongInput"),
        _ => new (HttpStatusCode.InternalServerError, "Unknown")
    },
    builder.Configuration.GetValue<bool>("ShowExceptions")
);
```

When an error occurs the app will return exactly the JSON error object like we saw earlier in this article.

You can check out the package code in the [dedicated repository](https://github.com/astorDev/nist). And you can also ... clap to this article 👉👈
