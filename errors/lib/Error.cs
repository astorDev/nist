using System.Collections;
using System.Net;

namespace Nist;

public record Error(HttpStatusCode Code, string Reason)
{
    public ExceptionDetailsModel? ExceptionDetails { get; set; }
    public Dictionary<string, object?>? Data { get; set; }

    public record ExceptionDetailsModel(string Message, IDictionary? Data, string? InnerExceptionMessage, string? StackTrace)
    {
        public static ExceptionDetailsModel FromException(Exception exception)
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