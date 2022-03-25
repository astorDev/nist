public record Error(HttpStatusCode Code, string Reason) : Nist.Errors.Error(Code, Reason)
{
    public static Error Unknown => new (HttpStatusCode.InternalServerError, "Unknown");
    public static Error GovernmentNotWelcomed => new (HttpStatusCode.BadRequest, "GovernmentNotWelcomed");

    public static Error Interpret(Exception exception)
    {
        return exception switch
        {
            GreetingController.GovernmentNotWelcomedException _ => GovernmentNotWelcomed,
            _ => Unknown
        };
    }
}

