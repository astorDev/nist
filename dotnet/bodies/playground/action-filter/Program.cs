using Nist.Errors;
using System.Net;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

app.UseErrorBody<Error>(ex => ex switch {
    NotEnoughLevelException _ => new (HttpStatusCode.BadRequest, "NotEnoughLevel"),
    _ => new (HttpStatusCode.InternalServerError, "Unknown")
}, showException: false);

app.Use(async (context, next) =>
{
    var x = context;
    await next();
    var responseBodyFeature = context.Features.Get<IHttpResponseBodyFeature>();
    
    
    var y = context;
});

app.Run();

public record Guest(string Name, bool Vip);
public record Ticket(string PartyId, string Receiver, bool LoungeAccess, string Code);
public class NotEnoughLevelException : Exception;