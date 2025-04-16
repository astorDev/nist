using Nist;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpService<GithubClient>("Github:Url");

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
