public record Error(HttpStatusCode Code, string Reason) : Nist.Errors.Error(Code, Reason)
{
    public static Error Unknown => new (HttpStatusCode.InternalServerError, "Unknown");
    public static Error DuplicateIndex = new (HttpStatusCode.BadRequest, "DuplicateIndex");

    public static Error Interpret(Exception exception)
    {
        return exception switch {
            IndexController.DuplicateException _ => DuplicateIndex,
            _ => Unknown
        };
    }
}

