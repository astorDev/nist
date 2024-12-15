# Creating Strongly-Typed API Clients in .NET

Let's say we want to call an external HTTP API. Let's also say the API doesn't provide an SDK. We can use `HttpClient` but we would still want to get a strongly typed response object instead of the `HttpResponseMessage`, right? So, let's make a strongly typed client together!

## Spacing Things Up!

```sh
dotnet new sln --name SpaceX
dotnet new classlib --name SpaceX.Protocol
dotnet new mstest --name SpaceX.Tests
dotnet sln add SpaceX.Protocol
dotnet sln add SpaceX.Tests
cd SpaceX.Tests
dotnet add reference ../SpaceX.Protocol
```

`LaunchesShould`

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

## Making It Strongly-Typed

```cs
namespace SpaceX;

public record Launch(string Name, bool? Success);

public class Uris {
    public const string Launches = "launches";
    public const string Latest = "latest";

    public static string LatestLaunch => $"{Launches}/{Latest}";
}
```

from the `SpaceX.Protocol` folder

```sh
dotnet add package Nist.Responses
```

```cs
var launch = await response.Read<Launch>();
Console.WriteLine(launch);
```

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

## Logging It Up!

```sh
dotnet add package Microsoft.Extensions.DependencyInjection
dotnet add package Microsoft.Extensions.Logging.Console
```

```cs
readonly ILogger logger;
readonly Client client;
    
public LaunchesShould()
{
    var services = new ServiceCollection()
        .AddLogging(l => l.AddSimpleConsole(c => c.SingleLine = true))
        .BuildServiceProvider();

    logger = services.GetRequiredService<ILogger<LaunchesShould>>();
}
```

```cs
var launch = await response.Read<Launch>(logger);
Console.WriteLine(launch);
```

```log
info: SpaceX.Tests.LaunchesShould[0] GET /v4/launches/latest > OK {"fairings":null,"links":{"patch":{"small":"https://images2.imgbox.com/eb/d8/D1Yywp0w_o.png","large":"https://images2.imgbox.com/33/2e/k6VE4iYl_o.png"},"reddit":{"campaign":null,"launch":"https://www.reddit.com/r/spacex/comments/xvm76j/rspacex_crew5_launchcoast_docking_discussion_and/","media":null,"recovery":null},"flickr":{"small":[],"original":[]},"presskit":null,"webcast":"https://youtu.be/5EwW8ZkArL4","youtube_id":"5EwW8ZkArL4","article":null,"wikipedia":"https://en.wikipedia.org/wiki/SpaceX_Crew-5"},"static_fire_date_utc":null,"static_fire_date_unix":null,"net":false,"window":null,"rocket":"5e9d0d95eda69973a809d1ec","success":true,"failures":[],"details":null,"crew":["62dd7196202306255024d13c","62dd71c9202306255024d13d","62dd7210202306255024d13e","62dd7253202306255024d13f"],"ships":[],"capsules":["617c05591bad2c661a6e2909"],"payloads":["62dd73ed202306255024d145"],"launchpad":"5e9e4502f509094188566f88","flight_number":187,"name":"Crew-5","date_utc":"2022-10-05T16:00:00.000Z","date_unix":1664985600,"date_local":"2022-10-05T12:00:00-04:00","date_precision":"hour","upcoming":false,"cores":[{"core":"633d9da635a71d1d9c66797b","flight":1,"gridfins":true,"legs":true,"reused":false,"landing_attempt":true,"landing_success":true,"landing_type":"ASDS","landpad":"5e9e3033383ecbb9e534e7cc"}],"auto_update":true,"tbd":false,"launch_library_id":"f33d5ece-e825-4cd8-809f-1d4c72a2e0d3","id":"62dd70d5202306255024d139"}
Launch { Name = Crew-5, Success = True }
```

## Create the Client

```cs
public class Client(HttpClient http, ILogger<Client> logger) {
    public async Task<Launch> GetLatestLaunch() => await GetAsync<Launch>(Uris.LatestLaunch);

    public Task<T> GetAsync<T>(string uri) => http.GetAsync(uri).Read<T>(logger);
}
```

```sh
dotnet add package Microsoft.Extensions.Http
```

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

```cs
var launch = await client.GetLatestLaunch();
Console.WriteLine(launch);
```

## Wrapping Up!

Having a strongly typed SpaceX API client is nice, but what's more important is that the setup can be used as a base for any other setup. Utilizing NIST packages we were able to do it in no time. 

You can check out the [nist repository](https://github.com/astorDev/nist) for more http-related tooling. And you can also ... clap to this article 👉👈


