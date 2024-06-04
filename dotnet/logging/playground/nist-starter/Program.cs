using Microsoft.AspNetCore.Mvc;
using Nist.Logs;
using Nist.Bodies;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.UseRequestBodyStringReader();
app.UseHttpIOLogging();
app.UseResponseBodyStringReader();

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