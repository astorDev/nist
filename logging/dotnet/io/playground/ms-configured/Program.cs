using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.HttpLogging;
using Nist.Errors;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpLogging(o => {
    o.CombineLogs = true;

    o.LoggingFields = HttpLoggingFields.RequestQuery
        | HttpLoggingFields.RequestMethod
        | HttpLoggingFields.RequestPath
        | HttpLoggingFields.RequestBody
        | HttpLoggingFields.ResponseStatusCode
        | HttpLoggingFields.ResponseBody
        | HttpLoggingFields.Duration;
});

var app = builder.Build();

app.UseHttpLogging();

app.UseErrorBody<Error>(ex => ex switch {
    NotEnoughLevelException _ => new (HttpStatusCode.BadRequest, "NotEnoughLevel"),
    _ => new (HttpStatusCode.InternalServerError, "Unknown")
}, showException: false);

app.MapPost("/parties/{partyId}/guests", (string partyId, [FromQuery] bool? loungeAccess, Guest visitor) => {
    if (loungeAccess == true && !visitor.Vip) 
        throw new NotEnoughLevelException();

    return new Ticket(
        PartyId: partyId,
        Receiver: visitor.Name,
        LoungeAccess: loungeAccess ?? false,
        Code: Guid.NewGuid().ToString()
    );
});

app.Run();

public record Guest(string Name, bool Vip);
public record Ticket(string PartyId, string Receiver, bool LoungeAccess, string Code);
public class NotEnoughLevelException : Exception;