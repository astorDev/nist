using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Nist.Errors;

public static class WebApplicationExtensions
{
    public static void UseErrorBody<TError>(this IApplicationBuilder app, Func<Exception, TError> errorFactory, bool showException = true) where TError : Error
    {
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
                var exception = exceptionHandlerPathFeature!.Error;

                var error = errorFactory(exception);
                if (showException)
                {
                    error.ExceptionDetails = Error.Details.FromException(exception);
                }

                context.Response.StatusCode = (int)error.Code;
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsync(JsonConvert.SerializeObject(error, new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    NullValueHandling = NullValueHandling.Ignore
                }));
            });
        });
    }
}