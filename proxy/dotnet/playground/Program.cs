using Nist;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpService<GithubClient>(new Uri("http://api.github.com"));

var app = builder.Build();

app.MapGet("/author", async (GithubClient github, HttpContext context) => {
    await github.Http.Proxy(context, "users/astorDev");
});

app.Run();
