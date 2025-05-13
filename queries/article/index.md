# .NET Minimal API Broke FromQuery

> .NET 7 Broke It Even Further. Is There Something Instead?

If you are something like me, you will likely bump into the broken behaviour. You will find out that `FromQuery` no longer binds the whole query object from the request. You will face it when upgrading from Controllers to Minimal API, from .NET 6 to .NET 7. Or even if using `FromQuery` for your new Minimal API.

Gladly, since .NET 7, there's a newer attribute that not just replaces the `FromQuery`, but lets you bind whichever part of the request you want. Let's do a quick recap of the problem and get straight to fixing it!

> Or jump straight to the [TLDR;](#tldr) at the end of this article for a quick fix.

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

Now, if we run the request below:

```http
GET http://localhost:5074/query-controller?name=Egor&age=29
```

We should get the matching query as a JSON:

```json
{
  "name": "Egor",
  "age": 29
}
```

So far, so good. Now, let's add a Minimal API endpoint there as well:

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

When I first bumped into it, I assumed this was just a bug in Minimal API and decided to stick with the good old controllers. But this was a wrong assumption, and things continued to fall apart!

## .NET 7: FromQuery is broken, but AsParameters has arrived

To reproduce the story, let's now upgrade our application one step further: `<TargetFramework>net7.0</TargetFramework>`. Let's run exactly the same request to the controller as we did before:

```http
GET /query-controller?name=Egor&age=29
```

Surprisingly, instead of our query JSON, we will get an empty response:

```http
HTTP/1.1 204  - No Content
```

The reason is that since .NET 7 (and in Minimal APIs before), the `FromQuery` is used to bind only individual parameters, not the whole object. So what do with do with that?

Of course, we can rewrite our request to use individual parameters instead of the query object, but a query object is a nice way to represent a query string. This is especially useful for creating strongly-typed HttpClients, like we did [in this article](https://medium.com/@vosarat1995/creating-strongly-typed-api-clients-in-net-d87a3d7ef016). So, how can we achieve that?

In .NET 7 a new attribute, called `AsParameters` was introduced. The attributes allows binding any part of a request to a single object, which suits our goal perfectly. Let's add a new endpoint using this attribute:

```csharp
app.MapGet("/parameters-mini", ([AsParameters] SimpleQuery query) => query);
```

Running, `GET http://localhost:5074/parameters-mini?name=Egor&age=29` will give us the response we were looking for:

```json
{
  "name": "Egor",
  "age": 29
}
```

The article is all about query parameters, but it's important to understand that `AsParameters` is not just a replacement for `FromQuery`, but the whole new concept. It allows us to include any set of parameters into one object - we can bundle headers values, query strings, route parameters and so on into a single object.

By the way, `AsParameters` attribute will not work in controllers, though. And there are no plans to remove this inconsistency as per [this GitHub issue](https://github.com/dotnet/aspnetcore/issues/42605), so if you are fan of the old style you will have to find some other way around.

Getting back to the queries, let's investigate some new opportunity .NET 7 brought to the table.

## Custom Query Parameters: The New Opportunity

Let's imagine we would like to 

```csharp
public record HardQuery(
    string? Name,
    int? Age,
    CommaSeparatedQueryParameter? Tags
);

public record CommaSeparatedQueryParameter(string[] Parts)
{
    public static bool TryParse(string source, out CommaQueryParameter result)
    {
        result = new (source.Split(','));
        return true;
    }
}
```

Let's switch back to .NET 6 (`<TargetFramework>net6.0</TargetFramework>`) and see what will happen when we run the following query:

```http
GET /hard-query-controller?name=Egor&age=29&tags=tag1,tag2
```

As you might expect, this will just ignore the `tags` parameter:

```json
{
  "name": "Egor",
  "age": 29,
  "tags": null
}
```

Let's now just switch back to .NET 7 `<TargetFramework>net7.0</TargetFramework>` and run a similar query:

```http
GET /hard-parameters-mini?name=Egor&age=29&tags=tag1,tag2
```

Quite surprisingly, with .NET 7 the binding will just work and parse the tags:

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

Well, it didn't exactly "just worked". It worked because we have implemented the static `TryParse(string source, out CommaQueryParameter result)` method. ASP .NET Core used this method in a duck-typing fashion to bind our query parameter. 

This feature allows us to bring our own custom objects as a query parameters. And this is **almost** the last thing I was planning to cover in this article. Let's recap this article and I'll give you one more thing I find helpful when dealing with query strings.

## TLDR;

In a few words, since .NET 7, you should use `AsParameters` instead of `FromQuery` to bind a complex object to a query string.

There's more to the `AsParameters` attribute, of course. `FromQuery` also got a few new features in the latest releases - we've covered it in more depth in this article. You can find the source code for the playground [here on GitHub](https://github.com/astorDev/nist/tree/main/queries/playground). If you also need a way to convert objects to a query string, this repository also has a helper package for that. It's called `Nist.Queries`, and here's how to use it:

```csharp
var uri = QueryUri.From("example", new {
    search = "stuff",
    good = true
});

Console.WriteLine(uri); // example?search=stuff&good=True
```

You might notice that the [home project (NIST)](https://github.com/astorDev/nist) contains many more folders beyond queries. The project aims to be a verbose toolset for HTTP APIs, so there's a high chance you will find something else useful - check it out and don't hesitate to give it a star! ⭐

Claps for this article are also highly appreciated! 😉
