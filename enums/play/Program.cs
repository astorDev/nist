using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Http.Json;
using Protocol;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddSimpleConsole(c => c.SingleLine = true);

builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// builder.Services.Configure<JsonOptions>(options =>
//     options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(
//         JsonNamingPolicy.SnakeCaseUpper
//     ))
// );

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();

app.MapGet("/", () => new
{
    Message = "Hello World!"
});

app.MapPost("/orders", (Protocol.Order order) =>
{
    return order;
});

app.MapPost("/deliveries", (Protocol.Delivery delivery) =>
{
    return delivery;
});

app.MapPatch("/orders/{id}", (int id, Protocol.OrderPatch orderPatch) =>
{
    return new Protocol.Order
    {
        Id = id,
        Status = Enum.Parse<Domain.OrderStatus>(orderPatch.Status)
    };
});
// .WithOpenApi(op =>
// {
//     var statusSchema = op.RequestBody.Content["application/json"].Schema.Properties["Status"];
//     statusSchema.Enum = [.. OrderStatuses.All.Select(status => new Microsoft.OpenApi.Any.OpenApiString(status))];
//     return op;
// });

app.Run();

namespace Protocol
{
    using Domain;

    public class Order
    {
        public required int Id { get; set; }
        public required OrderStatus Status { get; set; }
    }

    public record Delivery(
        int Id,
        OrderStatus Status
    );

    public record OrderPatch
    {
        public required string Status { get; init; }
    }

    public class OrderStatuses
    {
        public const string Pending = "Pending";
        public const string Preparing = "Preparing";
        public const string Delivering = "Delivering";
        public const string Delivered = "Delivered";
        public const string Cancelled = "Cancelled";

        public static readonly string[] All = new[]
        {
            Pending,
            Preparing,
            Delivering,
            Delivered,
            Cancelled
        };
    }
}

namespace Domain
{
    public enum OrderStatus
    {
        Pending,
        Preparing,
        InDelivery,
        Delivered,
        Cancelled
    }
}