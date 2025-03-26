namespace Nist;

public enum DuplicateRegistrationBehavior
{
    Throw,
    Ignore
}

public static class DuplicateRegistrationDecorator
{
    private readonly static HashSet<string> registered = [];

    public static void HandleRegistration(
        this IApplicationBuilder app,
        string key,
        Action<IApplicationBuilder> registration,
        DuplicateRegistrationBehavior behavior = DuplicateRegistrationBehavior.Ignore
    )
    {
        if (registered.Contains(key))
        {
            if (behavior == DuplicateRegistrationBehavior.Throw)
                throw new InvalidOperationException($"{key} has already been registered.");
            
            return;
        }

        registration(app);
        registered.Add(key);
    }
}