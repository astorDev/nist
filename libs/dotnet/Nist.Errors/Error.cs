using System.Collections;
using System.Net;

namespace Nist.Errors;

public record Error(HttpStatusCode Code, string Reason)
{
    public Details? ExceptionDetails { get; set; }

    public record Details(string Message, IDictionary? Data, string? InnerExceptionMessage, string? StackTrace)
    {
        public static Details FromException(Exception exception)
        {
            return new
            (
                exception.Message,
                exception.Data.GetEnumerator().MoveNext() ? exception.Data : null,
                exception.InnerException?.Message,
                exception.StackTrace
            );
        }
    }
}