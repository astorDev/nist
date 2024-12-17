# Exception Handling in ASP .NET Core

Building an exception-handling solution in ASP .NET Core can make your head spin. Although the tooling is pretty comprehensive it's surprisingly hard to find a single ready-to-go setup. In this article, we are going to explore the tools Microsoft provides and build a solution that can be reused for any WebApi you build.

> Or jump straight to [the end](#bonus-section-using-nist-nuget-package) for the TLDR;

## Setting Up the Project

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

# Bonus Section: Using Nist Nuget Package

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
