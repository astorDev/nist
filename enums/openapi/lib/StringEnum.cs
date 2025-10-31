using System.Reflection;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Any;

namespace Nist;

public interface IStringEnum
{
    public static abstract string[] All { get; }
}

public class StringEnumAttribute<ConstEnumType> : Attribute where ConstEnumType : IStringEnum
{
    public Type Type => typeof(ConstEnumType);
}

public static class ReflectionHelper
{
    public static IEnumerable<Type> FinalImplementationsOf(this IEnumerable<Assembly> assemblies, Type interfaceType) =>
        assemblies.SelectMany(assembly => assembly
            .GetExportedTypes()
            .Where(t => interfaceType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
        );

    public static T CallStaticProperty<T>(this Type type, string propertyName) where T : class
    {
        var propertyInfo = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Static)
            ?? throw new InvalidOperationException($"Type `{type.FullName}` does not have a static property named `{propertyName}`.");
            
        return propertyInfo.GetValue(null) as T ?? throw new InvalidOperationException($"Type `{type.FullName}` property `{propertyName}` is not of type `{typeof(T).FullName}`.");
    }
}

public static class StringEnumCollector
{
    public static Dictionary<Type, string[]> GetAllLoadedStringEnumTypes()
    {
        var dict = new Dictionary<Type, string[]>();
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var stringEnumTypes = assemblies.FinalImplementationsOf(typeof(IStringEnum));

        return stringEnumTypes.ToDictionary(
            type => type,
            type => type.CallStaticProperty<string[]>(nameof(IStringEnum.All))
        );
    }

    class StubStringEnumForNameOf : IStringEnum { public static string[] All => []; }

    public static Type? OptionalPropertyStringEnumType(this OpenApiSchemaTransformerContext context)
    {
        var attr = context.JsonPropertyInfo?.AttributeProvider?.GetCustomAttributes(typeof(StringEnumAttribute<>), true)
            .FirstOrDefault();

        if (attr == null) return null;

        var type = attr.GetType().GetProperty(nameof(StringEnumAttribute<StubStringEnumForNameOf>.Type))!.GetValue(attr);
        return (Type?)type;
    }

    public static OpenApiOptions AddStringEnumSchemaTransformer(this OpenApiOptions options)
    {
        var typeValueDictionary = GetAllLoadedStringEnumTypes();

        options.AddSchemaTransformer((schema, context, cancellationToken) =>
        {
            var description = context.OptionalPropertyStringEnumType();
            if (description != null && typeValueDictionary.TryGetValue(description, out var constEnumValues))
            {
                schema.Enum = [.. constEnumValues.Select(v => new OpenApiString(v))];
            }

            return Task.CompletedTask;
        });

        return options;
    }
}