namespace Nist.Logs;

public record HttpIOInformation(
    HttpContext HttpContext,
    string RequestBody,
    string ResponseBody,
    TimeSpan Elapsed,
    RouteValueDictionary RouteValues,
    Exception? Exception
);
