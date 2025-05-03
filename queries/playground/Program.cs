using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

app.MapGet("/query-mini", ([FromQuery] SimpleQuery query) => query);
app.MapGet("/parameters-mini", ([AsParameters] SimpleQuery query) => query);
app.MapGet("/hard-parameters-mini", ([AsParameters] HardQuery query) => query);

app.Run();

public class DotnetSix : ControllerBase
{
    [HttpGet("/query-controller")]
    public IActionResult Get([FromQuery] SimpleQuery query)
    {
        return Ok(query);
    }

    [HttpGet("/parameters-controller")]
    public IActionResult GetParameters([AsParameters] SimpleQuery query)
    {
        return Ok(query);
    }

    [HttpGet("/hard-query-controller")]
    public IActionResult GetHard([FromQuery] HardQuery query)
    {
        return Ok(query);
    }
}

public record SimpleQuery(
    string? Name,
    int? Age
)
{
    public static bool TryParse(string source, out SimpleQuery result)
    {
        throw new NotImplementedException("Parsing logic not implemented.");
    }
}

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