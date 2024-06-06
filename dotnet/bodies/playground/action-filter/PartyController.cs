using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

[IOObjectSaver]
public class PartyController 
{
    [HttpPost("/parties/{partyId}/guests")]
    public Ticket AddGuest(string partyId, [FromQuery] bool? loungeAccess, [FromBody] Guest visitor)
    {
        if (loungeAccess == true && !visitor.Vip) 
            throw new NotEnoughLevelException();

        return new Ticket(
            PartyId: partyId,
            Receiver: visitor.Name,
            LoungeAccess: loungeAccess ?? false,
            Code: Guid.NewGuid().ToString()
        );
    }
}

public class IOObjectSaverAttribute : Attribute, IResultFilter
{
    public void OnResultExecuted(ResultExecutedContext context)
    {
        var x = context;
    }

    public void OnResultExecuting(ResultExecutingContext context)
    {
        var x = context;
    }
}