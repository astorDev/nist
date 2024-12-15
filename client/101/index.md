# Creating Strongly-Typed API Clients in .NET

Let's say we want to call an external HTTP API. Let's also say the API doesn't provide an SDK. We can use `HttpClient` but we would still want to get a strongly typed response object instead of the `HttpResponseMessage`, right? So, let's make a strongly typed client together!

## Spacing Things Up!

We'll use SpaceX API for our little experiment. Let's set up our project - it will have a `SpaceX.Protocol` class library, containing our strongly-typed client and `SpaceX.Tests` to test our efforts. Here are the console commands for setting up our solution:

```sh
dotnet new sln --name SpaceX
dotnet new classlib --name SpaceX.Protocol
dotnet new mstest --name SpaceX.Tests
dotnet sln add SpaceX.Protocol
dotnet sln add SpaceX.Tests
cd SpaceX.Tests
dotnet add reference ../SpaceX.Protocol
```

For a start, let's just see what we can get from the API. We'll create a new HttpClient, make a GET request for the latest launches, and print the results. Let's call our test class `LaunchesShould` and add the method below:

```csharp
[TestMethod]
public async Task ReturnLatestAsRawString()
{
    var http = new HttpClient {
        BaseAddress = new("https://api.spacexdata.com/v4/")
    };

    var response = await http.GetAsync("launches/latest");
    Console.WriteLine(await response.Content.ReadAsStringAsync());
}
```

Running the method should print a json object similar to the one below:

```json
{
  "fairings": null,
  "links": {
    "patch": {
      "small": "https://images2.imgbox.com/eb/d8/D1Yywp0w_o.png",
      "large": "https://images2.imgbox.com/33/2e/k6VE4iYl_o.png"
    },
    "reddit": {
      "campaign": null,
      "launch": "https://www.reddit.com/r/spacex/comments/xvm76j/rspacex_crew5_launchcoast_docking_discussion_and/",
      "media": null,
      "recovery": null
    },
    "flickr": {
      "small": [],
      "original": []
    },
    "presskit": null,
    "webcast": "https://youtu.be/5EwW8ZkArL4",
    "youtube_id": "5EwW8ZkArL4",
    "article": null,
    "wikipedia": "https://en.wikipedia.org/wiki/SpaceX_Crew-5"
  },
  "static_fire_date_utc": null,
  "static_fire_date_unix": null,
  "net": false,
  "window": null,
  "rocket": "5e9d0d95eda69973a809d1ec",
  "success": true,
  "failures": [],
  "details": null,
  "crew": [
    "62dd7196202306255024d13c",
    "62dd71c9202306255024d13d",
    "62dd7210202306255024d13e",
    "62dd7253202306255024d13f"
  ],
  "ships": [],
  "capsules": [
    "617c05591bad2c661a6e2909"
  ],
  "payloads": [
    "62dd73ed202306255024d145"
  ],
  "launchpad": "5e9e4502f509094188566f88",
  "flight_number": 187,
  "name": "Crew-5",
  "date_utc": "2022-10-05T16:00:00.000Z",
  "date_unix": 1664985600,
  "date_local": "2022-10-05T12:00:00-04:00",
  "date_precision": "hour",
  "upcoming": false,
  "cores": [
    {
      "core": "633d9da635a71d1d9c66797b",
      "flight": 1,
      "gridfins": true,
      "legs": true,
      "reused": false,
      "landing_attempt": true,
      "landing_success": true,
      "landing_type": "ASDS",
      "landpad": "5e9e3033383ecbb9e534e7cc"
    }
  ],
  "auto_update": true,
  "tbd": false,
  "launch_library_id": "f33d5ece-e825-4cd8-809f-1d4c72a2e0d3",
  "id": "62dd70d5202306255024d139"
}
```

With that in place, let's carry on to getting a real object from our request.

## Making It Strongly-Typed

In the `SpaceX.Protocol` let's define a `Launch` model. We'll minify it for having just the launch `Name` and `Success` flag. We should also make our path statically represented. Here's the code we will get:

```cs
namespace SpaceX;

public record Launch(string Name, bool? Success);

public class Uris {
    public const string Launches = "launches";
    public const string Latest = "latest";

    public static string LatestLaunch => $"{Launches}/{Latest}";
}
```

Now, we'll need a method converting our `HttpResponseMessage` to the model of our choice. We'll use a package called `Nist.Responses`. Let's install it in the `SpaceX.Protocol`:

```sh
dotnet add package Nist.Responses
```

Now, we should be able to use the `Read` extension method on our response to get the actual model:

```cs
var launch = await response.Read<Launch>();
Console.WriteLine(launch);
```

With that in place, our test should print something like this:

```log
Launch { Name = Crew-5, Success = True }
```

Perhaps the most important part is done now, but we will still need to take care of a few things.

## Logging It Up!

Besides, deserializing the response body the `Read` method ensures the response was successful and logs information about the request and response. Here's an approximation of what the logic of the `Read` method looks like:

```cs
logger ??= NullLogger.Instance;
var body = await response.Content.ReadAsStringAsync();
if (!response.IsSuccessStatusCode)
{
    logger.LogError("{HttpMethod} {Url} > {StatusCode} {ResponseBody}", 
        response.RequestMessage!.Method, 
        response.RequestMessage!.RequestUri!.PathAndQuery, 
        response.StatusCode, 
        body
    );

    throw new UnsuccessfulResponseException(response.StatusCode, response.Headers, body);
}

logger.LogInformation("{HttpMethod} {Url} > {StatusCode} {ResponseBody}", 
    response.RequestMessage!.Method, 
    response.RequestMessage!.RequestUri!.PathAndQuery, 
    response.StatusCode, 
    body
);

var result = Deserialize.Json<T>(body, serializerOptions);
return result;
```

For now, we haven't ripped the benefits of the built-in logging, because we haven't passed an `ILogger` to the method, so `NullLogger.Instance` was used instead. Let's fix it! First, we'll need a package with an actual logger and a package allowing us a simple logging configuration. Here's how we can install them in our test project:

```sh
dotnet add package Microsoft.Extensions.DependencyInjection
dotnet add package Microsoft.Extensions.Logging.Console
```

Now, let's create the logger in our test class constructor. Here's the code:

```cs
readonly ILogger logger;
    
public LaunchesShould()
{
    var services = new ServiceCollection()
        .AddLogging(l => l.AddSimpleConsole(c => c.SingleLine = true))
        .BuildServiceProvider();

    logger = services.GetRequiredService<ILogger<LaunchesShould>>();
}
```

Finally, let's pass the logger to the `Read` method:

```cs
var launch = await response.Read<Launch>(logger);
Console.WriteLine(launch);
```

With that, we will get information about the requests written in our console like this:

```log
info: SpaceX.Tests.LaunchesShould[0] GET /v4/launches/latest > OK {"fairings":null,"links":{"patch":{"small":"https://images2.imgbox.com/eb/d8/D1Yywp0w_o.png","large":"https://images2.imgbox.com/33/2e/k6VE4iYl_o.png"},"reddit":{"campaign":null,"launch":"https://www.reddit.com/r/spacex/comments/xvm76j/rspacex_crew5_launchcoast_docking_discussion_and/","media":null,"recovery":null},"flickr":{"small":[],"original":[]},"presskit":null,"webcast":"https://youtu.be/5EwW8ZkArL4","youtube_id":"5EwW8ZkArL4","article":null,"wikipedia":"https://en.wikipedia.org/wiki/SpaceX_Crew-5"},"static_fire_date_utc":null,"static_fire_date_unix":null,"net":false,"window":null,"rocket":"5e9d0d95eda69973a809d1ec","success":true,"failures":[],"details":null,"crew":["62dd7196202306255024d13c","62dd71c9202306255024d13d","62dd7210202306255024d13e","62dd7253202306255024d13f"],"ships":[],"capsules":["617c05591bad2c661a6e2909"],"payloads":["62dd73ed202306255024d145"],"launchpad":"5e9e4502f509094188566f88","flight_number":187,"name":"Crew-5","date_utc":"2022-10-05T16:00:00.000Z","date_unix":1664985600,"date_local":"2022-10-05T12:00:00-04:00","date_precision":"hour","upcoming":false,"cores":[{"core":"633d9da635a71d1d9c66797b","flight":1,"gridfins":true,"legs":true,"reused":false,"landing_attempt":true,"landing_success":true,"landing_type":"ASDS","landpad":"5e9e3033383ecbb9e534e7cc"}],"auto_update":true,"tbd":false,"launch_library_id":"f33d5ece-e825-4cd8-809f-1d4c72a2e0d3","id":"62dd70d5202306255024d139"}
Launch { Name = Crew-5, Success = True }
```

Now, when we've utilized the `Read` method to its fullest let's get to the next part of our journey - creating a strongly-typed client!

## Create the Client

That shouldn't take us too long, as we just need to assemble a class having `HttpClient` and `ILogger` at its disposal and wrap endpoints in a method. Here's how it might look: 

```cs
public class Client(HttpClient http, ILogger<Client> logger) {
    public async Task<Launch> GetLatestLaunch() => await GetAsync<Launch>(Uris.LatestLaunch);

    public Task<T> GetAsync<T>(string uri) => http.GetAsync(uri).Read<T>(logger);
}
```

There's not much left - just to use the client. Let's simplify its registration by utilizing a `Microsoft.Extensions.Http` package:

```sh
dotnet add package Microsoft.Extensions.Http
```

And here's how our test class can register our client:

```cs
readonly ILogger logger;
readonly Client client;

public LaunchesShould()
{
    var services = new ServiceCollection()
        .AddLogging(l => l.AddSimpleConsole(c => c.SingleLine = true))
        .AddHttpClient<Client>(cl => cl.BaseAddress = new("https://api.spacexdata.com/v4/")).Services
        .BuildServiceProvider();

    logger = services.GetRequiredService<ILogger<LaunchesShould>>();
    client = services.GetRequiredService<Client>();
}
```

Now, getting the latest launch will be just a single strongly typed call to the client:

```cs
var launch = await client.GetLatestLaunch();
Console.WriteLine(launch);
```

There's not much to improve there, so let's wrap this up!

## Wrapping Up!

Having a strongly typed SpaceX API client is nice, but what's more important is that the setup can be used as a base for more complicated scenarios. For example, the `Read` method also supports the custom `JsonSerializerSettings` and you can use `PostAsJsonAsync` from `System.Net.Http.Json` to submit information to an API, `Nist.Queries` package allows you to convert objects to query strings. All of this doesn't require you to change anything in the fundamental setup introduced in this article.

As you may see, nist packages help us to make a strongly typed http client in no time. You can check out the [nist repository](https://github.com/astorDev/nist), there are many more HTTP-related tools. And you can also ... clap to this article 👉👈
