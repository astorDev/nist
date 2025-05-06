# .NET Minimal API Broke FromQuery

> .NET 7 Broke It Even Further. Is There Something Instead?

If you are something like me, you will likely bump into the broken behaviour. You will find out that `FromQuery` no longer binds the whole query object from the request. You will face it when upgrading from Controllers to Minimal API, from .NET 6 to .NET 7. Or even if using `FromQuery` for your new Minimal API.

Gladly, since .NET 7 there's a newer attribute that not just replaces the `FromQuery`, but let's you bind whichever part of the request you want. Let's do a quick recap of the problem and get straight to fixing it!

> Or jump straight to the [TLDR;](#tldr) in the end of this article for a quick fix.

## .NET 6: Inconsistent FromQuery Behavior

Let's say we have a .NET 6 Web API, with `Program.cs` like this:

> We can scaffold it with `dotnet new web` and downgrade it like this: `<TargetFramework>net6.0</TargetFramework>`

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

app.Run();

public class DotnetSix : ControllerBase
{
    [HttpGet("/query-controller")]
    public IActionResult Get([FromQuery] SimpleQuery query)
    {
        return Ok(query);
    }
}

public record SimpleQuery(
    string? Name,
    int? Age
);
```

Now if we run the request below:


```http
GET http://localhost:5074/query-controller?name=Egor&age=29
```

We should get the mathching query as a JSON:

```json
{
  "name": "Egor",
  "age": 29
}
```

So far so good. Now, let's add a Minimal API endpoint there as well:

```csharp
app.MapGet("/query-mini", ([FromQuery] SimpleQuery query) => query);
```

We'll call it with exactly the same data as before

```http
GET http://localhost:5074/query-mini?name=Egor&age=29
```

But now we will receive an exception below:

```text
Microsoft.AspNetCore.Http.BadHttpRequestException: Required parameter "SimpleQuery query" was not provided from query string.
   at Microsoft.AspNetCore.Http.RequestDelegateFactory.Log.RequiredParameterNotProvided(HttpContext httpContext, String parameterTypeName, String parameterName, String source, Boolean shouldThrow)
   at lambda_method1(Closure , Object , HttpContext )
   at Microsoft.AspNetCore.Http.RequestDelegateFactory.<>c__DisplayClass36_0.<Create>b__0(HttpContext httpContext)
   at Microsoft.AspNetCore.Routing.EndpointMiddleware.Invoke(HttpContext httpContext)
--- End of stack trace from previous location ---
   at Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddleware.Invoke(HttpContext context)
```

When I first bump into it I've assumed this is just a bug in Minimal API and decided to stick with the good old controllers. But this was a wrong assumption and the things continue to fall apart!

## .NET 7: FromQuery is broken, but AsParameters has arrived

To reproduce the story let's now upgrade our application one step further: `<TargetFramework>net7.0</TargetFramework>`. Let's run exactly the same request to the controller as we did before:

```http
GET /query-controller?name=Egor&age=29
```

Surprisingly, instead of our query JSON we will get an empty response:

```http
HTTP/1.1 204  - No Content
```

The reason is that since .NET 7 (and in Minimal APIs before) the `FromQuery` is used to bind only individual parameters, not the whole object. So what do with do with that?

Of course, we can rewrite our request to use individual parameters instead of the query object, but a query object is a nice way to represent query string. This is especially useful for creating strongly-typed HttpClients, like we did [in this article](https://medium.com/@vosarat1995/creating-strongly-typed-api-clients-in-net-d87a3d7ef016). How can we achieve that?



```csharp
app.MapGet("/parameters-mini", ([AsParameters] SimpleQuery query) => query);
```

```json
{
  "name": "Egor",
  "age": 29
}
```

```http
GET http://localhost:5074/parameters-mini?name=Egor&age=29
```

> `AsParameters` attribute will not work in controllers, though. And there are no plans to remove this inconsistency as per [this GitHub issue](https://github.com/dotnet/aspnetcore/issues/42605)


## Custom Query Parameters: The New Opportunity

```csharp
public record HardQuery(
    string? Name,
    int? Age,
    CommaQueryParameter? Tags
);

public record CommaQueryParameter(string[] Parts)
{
    public static bool TryParse(string source, out CommaQueryParameter result)
    {
        result = new (source.Split(','));
        return true;
    }
}
```

```xml
<TargetFramework>net6.0</TargetFramework>
```

```http
GET /hard-query-controller?name=Egor&age=29&tags=tag1,tag2
```

```json
{
  "name": "Egor",
  "age": 29,
  "tags": null
}
```

```xml
<TargetFramework>net7.0</TargetFramework>
```

```http
GET /hard-parameters-mini?name=Egor&age=29&tags=tag1,tag2
```

```json
{
  "name": "Egor",
  "age": 29,
  "tags": {
    "parts": [
      "tag1",
      "tag2"
    ]
  }
}
```

## TLDR;

In a few words, since .NET 7 you should use `AsParameters` instead of `FromQuery` to bind a complex objects to a query string.

There's more to the `AsParameters` attribute, of course. `FromQuery` also got a few new features in the latest releases - we've covered it in more depth in this article. You can find the source code for the playground [here on the GitHub](https://github.com/astorDev/nist/tree/main/queries/playground). If you also need a way to convert objects to query string this repository also have a helper package for that. It's called `Nist.Queries` and here's how to use it:

```csharp
var uri = QueryUri.From("example", new {
    search = "stuff",
    good = true
});

Console.WriteLine(uri); // example?search=stuff&good=True
```

You might notice, that the [home project (NIST)](https://github.com/astorDev/nist) contains many more folders beyond queries. The project aims to be a verbose toolset for HTTP APIs so there's a high chance you will find something else useful - check it out and don't hesitate to give it a star! ⭐

Claps for this article are also highly appreciated! 😉