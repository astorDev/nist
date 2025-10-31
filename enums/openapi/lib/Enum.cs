using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Any;

namespace Nist;

public static class OpenApiEnumExtensions
{
    public static OpenApiOptions AddEnumSchemaTransformer(this OpenApiOptions options, bool includeInformationalStrings = true)
    {
        options.AddSchemaTransformer((schema, context, cancellationToken) =>
        {
            if (context.JsonPropertyInfo?.PropertyType?.IsEnum != true) return Task.CompletedTask;
            if (schema.Enum?.Any() == true) return Task.CompletedTask;

            var values = Enum.GetValues(context.JsonPropertyInfo.PropertyType).Cast<Enum>();

            schema.Enum = [
                .. values.Select(v => new OpenApiInteger((int)(object)v)),
                .. includeInformationalStrings
                    ? values.Select(value => new OpenApiString($"Hint: {(int)(object)value} -> {value}"))
                    : []
            ];

            return Task.CompletedTask;
        });

        return options;
    }
}