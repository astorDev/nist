using Elnik.WebApi.Misc;

public record Error(HttpStatusCode Code, string Reason) : Nist.Errors.Error(Code, Reason)
{
    public static Error Unknown => new (HttpStatusCode.InternalServerError, "Unknown");
    public static readonly Error DuplicateIndex = new (HttpStatusCode.BadRequest, "DuplicateIndex");
    public static readonly Error DashboardNotFound = new(HttpStatusCode.BadRequest, "DashboardNotFound");

    public static Error Interpret(Exception exception)
    {
        return exception switch {
            IndexController.DuplicateException _ => DuplicateIndex,
            DashboardNotFoundException => DashboardNotFound,
            _ => Unknown
        };
    }
}

