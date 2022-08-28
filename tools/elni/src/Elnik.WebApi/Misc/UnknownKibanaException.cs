public class UnknownKibanaException : Exception {
    public string ErrorMessage { get; }
    public HttpStatusCode StatusCode { get; }
    public override string Message => $"{this.StatusCode} received from kibana api ({this.ErrorMessage})";
    
    public UnknownKibanaException(string errorMessage, HttpStatusCode statusCode, Exception innerException) : base(null, innerException) {
        this.ErrorMessage = errorMessage;
        this.StatusCode = statusCode;
    }

    public static UnknownKibanaException From(UnsuccessfulResponseException ex){
        var error = ex.DeserializedBody<dynamic>()!;
        string errorMessage = error.message;
        return new(errorMessage, ex.StatusCode, ex);
    }

    public static async Task<T> Wrap<T>(Task<T> action, Action<UnknownKibanaException>? additionalHandler = null) {
        try { return await action; }
        catch (UnsuccessfulResponseException ex) {
            var kibanaEx = From(ex);
            additionalHandler?.Invoke(kibanaEx);
            throw kibanaEx;
        }
    }
}
