using Nist;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddSimpleConsole(c => c.SingleLine = true);

builder.Services.AddHttpService<GithubClient>(new Uri("http://api.github.com"));
builder.Services.AddHttpService<PostmanEchoClient>(new Uri("https://postman-echo.com"));

var app = builder.Build();

app.UseHttpIOLogging(o => o.Message = HttpIOMessagesRegistry.DefaultWithJsonBodies);

app.MapGet("/author", async (GithubClient github, HttpContext context) => {
    await github.Http.Proxy(context, "users/astorDev");
});

app.MapPost("/echo", async (PostmanEchoClient echo, HttpContext context) => {
    await echo.Http.Proxy(context, "post");
});

app.MapPost("simple-echo", async (HttpContext context) => {

});

app.Run();

public class GithubClient(HttpClient http) {
    public HttpClient Http => http;
}