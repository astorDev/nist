# NIST - Nice State Transfer

NIST is a non-arrogant brother of REST. Tired of hearing what your HTTP API [must](https://roy.gbiv.com/untangled/2008/rest-apis-must-be-hypertext-driven) do to become REST? To be NIST an HTTP API needs to do only 3 things:

1. Use different URIs for different operations/requests 
2. Utilize GET for read and other verbs for write.
3. Use 2xx codes for success and other codes for errors.

And even if an API breaks a rule a few times - that's okay, it's still NIST.

## We also have packages!

Beyond being a software architectural style NIST provides a handful of packages, aiming to help you create and consume HTTP APIs, especially the NIST ones:

- [C# Nist.Logs](/dotnet/logging) - Request and response logs.
- [C# Nist.Error](/dotnet/errors/) - Exceptions to responses mapping.
- [C# Nist.Bodies](/dotnet/bodies/) - Request and Response as strings in `HttpContext`.
- [C# Nist.Queries](/dotnet/queries/) - Query strings from objects.
- [C# Nist.Registration](/dotnet/registration/) - Simple DI registration of clients.
- [C# Nist.Responses](/dotnet/responses/) - `HttpResponse` to a concrete response object.
