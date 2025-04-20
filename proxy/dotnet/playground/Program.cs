using Nist;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddSimpleConsole(c => c.SingleLine = true);

builder.Services.AddHttpService<GithubClient>(new Uri("http://api.github.com"));
builder.Services.AddHttpService<PostmanEchoClient>(new Uri("https://postman-echo.com"));
builder.Services.AddHttpService<SpaceXClient>(new Uri("https://api.spacexdata.com/v4/"));

// builder.Services.AddHttpClient<PostmanEchoClient>(client => {
//     client.BaseAddress = new Uri("https://postman-echo.com");
// });

var app = builder.Build();


// /author and /launch produces error while reading Json Body
// Supposedly, because of the gzip encoding of the original response
//app.UseHttpIOLogging(o => o.Message = HttpIOMessagesRegistry.DefaultWithJsonBodies);


app.MapPost("/echo", async (PostmanEchoClient echo, HttpContext context) => {
    await echo.Http.Proxy(context, "post");
});

app.MapGet("/echo-get", async (PostmanEchoClient echo, HttpContext context) => {
    await echo.Http.Proxy(context, "get");
});

app.MapGet("/author", async (GithubClient github, HttpContext context) => {
    await github.Http.Proxy(context, "users/astorDev");
});

app.MapGet("/launch", async (SpaceXClient spaceX, HttpContext context) => {
    await spaceX.Http.Proxy(context, "launches/latest");
});

app.Run();


public class PostmanEchoClient(HttpClient http) {
    public HttpClient Http => http;
}

public class GithubClient(HttpClient http) {
    public HttpClient Http => http;
}

public class SpaceXClient(HttpClient http) {
    public HttpClient Http => http;
}
