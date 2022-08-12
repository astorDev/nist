using Nist.Responses;

public class UnknownKibanaException : Exception {
    public string ErrorMessage { get; }
    public HttpStatusCode StatusCode { get; }
    public UnknownKibanaException(string errorMessage, HttpStatusCode statusCode, Exception innerException) : base(null, innerException) {
        this.ErrorMessage = errorMessage;
        this.StatusCode = statusCode;
    }
    public override string Message => $"{StatusCode} received from kibana api ({ErrorMessage})";
    public static UnknownKibanaException From(UnsuccessfulResponseException ex){
        var error = ex.DeserializedBody<dynamic>()!;
        string errorMessage = error.message;
        return new UnknownKibanaException(errorMessage, ex.StatusCode, ex);
    }

    public static async Task<T> Wrap<T>(Task<T> action, Action<UnknownKibanaException> addtionalHandler = null) {
        try {
            var result = await action;
            return result;
        }
        catch (UnsuccessfulResponseException ex){
            var kibanaEx = From(ex);
            addtionalHandler?.Invoke(kibanaEx);
            throw kibanaEx;
        }
    }
}
