## .NET 6: Inconsistent FromQuery Behavior

```xml
<TargetFramework>net6.0</TargetFramework>
```

```http
GET http://localhost:5074/query-controller?name=Egor&age=29
```

```json
{
  "name": "Egor",
  "age": 29
}
```

```http
GET http://localhost:5074/query-mini?name=Egor&age=29
```

```text
Microsoft.AspNetCore.Http.BadHttpRequestException: Required parameter "SimpleQuery query" was not provided from query string.
   at Microsoft.AspNetCore.Http.RequestDelegateFactory.Log.RequiredParameterNotProvided(HttpContext httpContext, String parameterTypeName, String parameterName, String source, Boolean shouldThrow)
   at lambda_method1(Closure , Object , HttpContext )
   at Microsoft.AspNetCore.Http.RequestDelegateFactory.<>c__DisplayClass36_0.<Create>b__0(HttpContext httpContext)
   at Microsoft.AspNetCore.Routing.EndpointMiddleware.Invoke(HttpContext httpContext)
--- End of stack trace from previous location ---
   at Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddleware.Invoke(HttpContext context)
```

## .NET 7: FromQuery is broken, but AsParameters has arrived

```xml
<TargetFramework>net7.0</TargetFramework>
```

```http
GET http://localhost:5074/query-mini?name=Egor&age=29
```

```http
HTTP/1.1 204  - No Content
```

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