# Enums in OpenApi (Swagger) in .NET 9

> Using OpenApiSchemaTransformer to Customize Various Enum Representations.

![Make Them Make Sense!](thumb.png)

Enums in APIs are surprisingly tricky. There are multiple approaches to representing enums, each problematic in its own way. In this article, we are going to take a look at different ways of representing enums in .NET. For each approach, we will take a look at what we will get in terms of OpenAPI and even improve a few representations.

## Getting Started: Integer Enums

In [this article](https://medium.com/@vosarat1995/making-your-openapi-swagger-docs-ui-awesome-in-net-9-67fbde6b71b5), I've explained how to set up OpenAPI UI with Scalar - we'll start from there. Let's create our API model with a simple enum just to get started:

```csharp
public enum PizzaTopping
{
    Pepperoni,
    Mushrooms,
    Onions,
    Sausage
}

public class Pizza
{
    public required PizzaTopping Topping { get; init; }
}
```

Of course, we'll also need endpoints to get a proper Swagger UI:

```csharp
app.MapGet("/", () => new Pizza {
    Topping = PizzaTopping.Onions
});

app.MapPost("/", (Pizza pizza) =>
{
    return pizza;
});
```

With that setup, if we open our Swagger UI we'll get the following models:

![](demo-starter.png)

You might see the problem with the default view. There is absolutely no information about what values are supposed to be passed to `PizzaTopping`. Let's fix that in the next section.

## First Schema Transformer: Enum Numbers Info

OpenAPI models accept an enum property. However, they are not filled by default. Gladly, Microsoft's OpenAPI system allows us to arbitrarily transform the schema using `SchemaTransformer`. Let's add an extension method filling the `enum` property for an encountered enum object property:

```csharp
public static class OpenApiEnumExtensions
{
    public static OpenApiOptions AddEnumSchemaTransformer(this OpenApiOptions options)
    {
        options.AddSchemaTransformer((schema, context, cancellationToken) =>
        {
            if (context.JsonPropertyInfo?.PropertyType?.IsEnum != true) return Task.CompletedTask;
            if (schema.Enum?.Any() == true) return Task.CompletedTask;

            var values = Enum.GetValues(context.JsonPropertyInfo.PropertyType).Cast<Enum>();

            schema.Enum = [
                .. values.Select(v => new OpenApiInteger((int)(object)v)),
            ];

            return Task.CompletedTask;
        });

        return options;
    }
}
```

Here's how we can register the transformer:

```csharp
builder.Services.AddOpenApi(options =>
{
    options.AddEnumSchemaTransformer();
});
```

Here's what we'll get now:

![](demo-all-ints.png)

Now, we know which options are available. However, we still have no idea what those options mean. Let's add and use an optional flag `includeInformationalStrings` in our `AddEnumSchemaTransformer` method:

```csharp
.. includeInformationalStrings
    ? values.Select(value => new OpenApiString($"Hint: {(int)(object)value} -> {value}"))
    : []
```

With the flag set to `true` by default, we should now see the following:

![](demo-int-hints.png)

This pretty much covers default int-based representation of enums. However, there is a different way to represent enums, which is actually dominant for most popular public APIs - strings. Let's investigate it! 

## Standard Representation: JsonStringEnumConverter

The simplest way to make an enum represented as a string in both the API and OpenAPI model is to decorate it with `[JsonConverter(typeof(JsonStringEnumConverter))]` attribute. Let's utilize that, by adding cheese to our pizza:

```csharp
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PizzaCheeseType
{
    Mozzarella,
    Cheddar,
    Parmesan,
    Gouda
}

public class Pizza
{
    public required PizzaTopping Topping { get; init; }
    public required PizzaCheeseType Cheese { get; init; }
}
```

With the setup, will will get a pretty informative OpenAPI model right ahead:

![](demo-strings.png)

However, there is a small problem with this setup. It's not forward-compatible. What this means is that if we send an unknown value for the topping it will be processed just fine:

```http
POST /

{
    "topping" : 11,
    "cheese" : "Cheddar"
}
```

The request will be processed as if `11` is an existing enum value. However, if we send an unknown type of cheese like this:

```http
POST /

{
    "topping" : 3,
    "cheese" : "Maasdam"
}
```

The request will fail with an exception with the following message:

```text
The JSON value could not be converted to PizzaCheeseType
```

This is also problematic if we use an enum on the client side - any new value added by the server will break our deserialization logic. Let's handle this in the next section.

## Making It Forward-Compatible: IStringEnum

There's a pretty straightforward fix for the problem: use `string` instead of enum. We will most likely need a set of constants defined somewhere anyway. Let's see what that might look like by adding a `CrustType` to our `Pizza`: 

> Note that for the solution to work we should not convert (at least without `Try` pattern) the value to an enum value. Otherwise we will just be moving the problem to another place.

```csharp
public class CrustTypes
{
    public const string Thin = "Thin";
    public const string Thick = "Thick";
    public const string Stuffed = "Stuffed";

    public static string[] All => [Thin, Thick, Stuffed];
}

public class Pizza
{
    // ...

    public required string Crust { get; init; }
}
```

The problem is that with the solution we will lose our enum informativeness. Let's get it back. First thing we'll need to do is connect the `Crust` property and `CrustTypes` class. Here's how we can achieve that:

```csharp
public interface IStringEnum
{
    public static abstract string[] All { get; }
}

public class StringEnumAttribute<ConstEnumType> : Attribute where ConstEnumType : IStringEnum
{
    public Type Type => typeof(ConstEnumType);
}

public class CrustTypes : IStringEnum
{
    public const string Thin = "Thin";
    public const string Thick = "Thick";
    public const string Stuffed = "Stuffed";

    public static string[] All => [Thin, Thick, Stuffed];
}

public class Pizza
{
    [StringEnum<CrustTypes>]
    public required string Crust { get; init; }
}
```

Now, of course, we will need to use the connection in OpenAPI.
Let's first create a simple helper, that is not strictly related to our goal, but makes our future code simpler:

```csharp
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
```

To use the connection, we'll need to find all string enums defined in our solution:

```csharp
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
```

We'll also need to scan an incoming property, to see if it is decorated with `StringEnumAttribute`:

```csharp
class StubStringEnumForNameOf : IStringEnum { public static string[] All => []; }

public static Type? OptionalPropertyStringEnumType(this OpenApiSchemaTransformerContext context)
{
    var attr = context.JsonPropertyInfo?.AttributeProvider?.GetCustomAttributes(typeof(StringEnumAttribute<>), true)
        .FirstOrDefault();

    if (attr == null) return null;

    var type = attr.GetType().GetProperty(nameof(StringEnumAttribute<StubStringEnumForNameOf>.Type))!.GetValue(attr);
    return (Type?)type;
}
```

Finally, let's define our transformer using the extensions we've defined earlier:

```csharp
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
```

And, of course, register the transformer:

```csharp
builder.Services.AddOpenApi(options =>
{
    options.AddEnumSchemaTransformer();
    options.AddStringEnumSchemaTransformer();
});
```

This should be enough to get a proper string enum representation. Let's recap and see all the ways of representing enums in the last section.

## Recap

In this article, we've implemented extension methods for improving the way enums are represented in OpenAPI documents. Instead of recreating the logic from scratch, you can use the following NuGet package:

> To get the whole system with UI in place, consult [this article](https://medium.com/@vosarat1995/making-your-openapi-swagger-docs-ui-awesome-in-net-9-67fbde6b71b5)

```sh
dotnet add package Nist.Enums.OpenApi
```

To apply those customizations, just call the corresponding extension method on `OpenApiOptions`:

```csharp
builder.Services.AddOpenApi(options =>
{
    options.AddEnumSchemaTransformer();
    options.AddStringEnumSchemaTransformer();
});
```

You will get improved representation of all your standard .NET integer enums right away. And if you want to use the string enums, here's an example of how to achieve that:

```csharp
public class CrustTypes : IStringEnum
{
    public const string Thin = "Thin";
    public const string Thick = "Thick";

    public static string[] All => [Thin, Thick];
}

public class Pizza
{
    [StringEnum<CrustTypes>]
    public required string Crust { get; init; }
}
```

Here's an example of an OpenAPI UI you might expect:

![](demo.png)

The package, as well as this article, is part of the [NIST project](https://github.com/astorDev/nist). The project's purpose in a few words is to be a Non-Toxic REST alternative, so there are many interesting things beyond enums - check it out and don't hesitate to give it a star! ⭐

Claps for this article are also highly appreciated! 😉