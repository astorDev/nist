public record Error(HttpStatusCode Code, string Reason) : Nist.Errors.Error(Code, Reason)
{
    public static Error WeaponNotExists => new (HttpStatusCode.BadRequest, "WeaponNotExists");
    public static Error Unknown => new (HttpStatusCode.InternalServerError, "Unknown");

    public static Error Interpret(Exception exception) => exception switch
    {
        WeaponNotExistsException _ => WeaponNotExists,
        _ => Unknown
    };
}

public class WeaponNotExistsException : Exception {};