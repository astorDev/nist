using System.Net;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;
using Nist.Errors;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpLogging(o =>
{
    o.LoggingFields = 
        HttpLoggingFields.RequestBody 
        | HttpLoggingFields.RequestMethod
        | HttpLoggingFields.RequestBody
        | HttpLoggingFields.RequestPath
        | HttpLoggingFields.RequestQuery
        | HttpLoggingFields.ResponseStatusCode
        | HttpLoggingFields.ResponseBody
        | HttpLoggingFields.Duration;

    o.CombineLogs = true;
});

var app = builder.Build();

app.UseHttpLogging();

app.UseErrorBody<Error>(ex => ex switch
{
    BadLookException => new(HttpStatusCode.BadRequest, "BadLook"),
    _ => new(HttpStatusCode.InternalServerError, "Unknown")
}, showException: false);

app.MapPost("parties/{id}/visitors", (string id, [FromQuery] bool goodLooking, Visitor visitor) =>
{
    if (!goodLooking)
    {
        throw new BadLookException();
    }

    return new Invitation(
        id,
        visitor.Name,
        new(visitor.Passcode.Reverse().ToArray())
    );
});

app.Run();

public record Visitor(string Name, string Passcode);
public record Invitation(string PartyId, string Receiver, string Code);
public class BadLookException : Exception { };