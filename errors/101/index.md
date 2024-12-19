# Exception Handling in ASP .NET Core

Building an exception-handling solution in ASP .NET Core can make your head spin. Although the tooling is pretty comprehensive it's surprisingly hard to find a single ready-to-go setup. In this article, we are going to explore the tools Microsoft provides and build a solution that can be reused for any WebApi you build.

> Or jump straight to [the end](#bonus-section-using-nist-nuget-package) for the TLDR;

## Setting Up the Project

First, let's see what we get by default. We'll need to create a project with `dotnet new web` and add an endpoint, that we can call to get an exception. Let's add a method like in the snippet below:

```cs
app.MapGet("/unknown", () => {
    throw new ("Unknown error");
});
```

Now, if would run the app via `dotnet run` and access the endpoint, for example by running `curl localhost:5090/unknown` we'll get a `500` status code and response looking like this:

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

Of course that response is hardly useful for another app since it's in a text format and can not be adequately deserialized. Besides the fact, that the response shows exception details which could cause a security issues, so is not shown in a production environment. Gladly, a much better response is just one line away.

## Utilizing Problem Details

Since .NET 7 we Microsoft provided an standard and reusable error model they call `ProblemDetails`. We can add it in our application by adding just the line below:

```cs
builder.Services.AddProblemDetails();
```

Now, if we rerun the application and call the same endpoint again we will get an extensive JSON object looking something like this:

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

That's already much nicer, but we still have issues. Firstly, the model is still wouldn't be shown in a production environment, since it exposes exception details. The model is pretty overwhelming now with lots of unnecessary details. Fortunately, we can fix both problems by making a custom exception handler, utilizing the `ProblemDetails` infrastructure. Here's the code we can add to do it:

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

With that in place we will get a nice error model, that can be shown in any environment:

```json
{
  "type": "Unexpected",
  "title": "An error occurred while processing your request.",
  "status": 500,
  "traceId": "00-aba6a27507178cf0fda78a7fc88678b9-35a5c87fadf74533-00"
}
```

Although, the model is nice for now we always returning the same type regardless of the error that is actually happening. Let's make something more sophisticated!

## Different Errors for Different Exceptions

Before changing the exception handling let's create a custom exception we will map to a specific error later:

```cs
class WrongInputException(string input) : Exception {
    public override IDictionary Data => new Dictionary<string, object> {
        [ "input" ] = input
    };
}
```

We will also need an endpoint to throw the exception:

```csharp
app.MapGet("/wrong-input/{input}", (string input) => {
    throw new WrongInputException(input);
});
```

Of course, if we access the new endpoint now we will get the same error as before. What we are planning to do is to make an problem response depending on the exception we receive. Let's make a helper method for that:

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

Well, to map an exception we first need to get the occurred exception. We can do it using the `HttpContext` features. Here's the code for it:

```csharp
var exception = context.Features.GetRequiredFeature<IExceptionHandlerFeature>()!.Error;
```

Putting it all together we will get a new version of exception handling looking like this:

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

And now if we will call the 

```json
{
  "type": "WrongInput",
  "title": "Bad Request",
  "status": 400,
  "traceId": "00-93da29fb4bb684d23789ac5def28cce6-ff5767ba877dfae6-00"
}
```

But here's the twist! Despite the `status` field having value of `400`, the actual request code will still be `500`! Let's fix it by explicitly mapping the problem status to the response status code:

```csharp
context.Response.StatusCode = problem.Status ?? 500;
```

In the result our exception code

```csharp
app.UseExceptionHandler(e => {
    e.Run(async context => {
        var writer = context.RequestServices.GetRequiredService<IProblemDetailsService>();
        var exception = context.Features.GetRequiredFeature<IExceptionHandlerFeature>()!.Error;

        var problem = ProblemFrom(exception);

        context.Response.StatusCode = problem.Status ?? 500;

        await writer.WriteAsync(new ProblemDetailsContext{
            HttpContext = context,
            ProblemDetails = problem
        });
    });
});
```

The solution is already comprehensive. But still there are some things to improve - let's discuss it in the next section.

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

## Enriching the Problem

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
