public record Error(HttpStatusCode Code, string Reason) : Nist.Errors.Error(Code, Reason)
{
    public static Error Unknown => new (HttpStatusCode.InternalServerError, "Unknown");

    public static Error Interpret(Exception exception)
    {
        return Unknown;
    }
}

