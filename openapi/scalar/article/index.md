# Making Your OpenAPI (Swagger) Docs UI Awesome in .NET 9

Since .NET 9 we no longer get a Swagger UI included in the default `webapi` template. Although the document is still included, now via the `MapOpenApi` call, the UI is not here anymore. Gladly, it's relatively easy to get a documentation UI back. But the UI was boring anyway, so let's get something fancier!

## Meet Scalar

```sh
dotnet add package Scalar.AspNetCore
```

```csharp
app.MapScalarApiReference();
```

## Picking a Theme!

### Theme 0: None

![](none.png)

### Theme 1: Alternative

![](alternative.png)

### Theme 2: Default

![](default.png)

Slightly darker than the `None` theme.

### Theme 3: Moon

![](moon.png)

To be honest, that's the theme I enjoy the least.

### Theme 4: Purple

![](purple.png)

### Theme 5: Solarized

![](solarized.png)

### Theme 6: Blue Planet

![](blue-planet.png)

### Theme 7: Saturn

![](saturn.png)

The Blackest theme.

### Theme 8: Kepler

![](kepler.png)

### Theme 9: Mars

![](mars.png)

This is the most interesting theme out of all, but I'm afraid it will saturate quickly, so I'll probably refrain from it.

### Theme 10: Deep Space

![](deep.png)

## Additional Customizations

## Wrapping Up!

It's very easy to get a nice looking .NET OpenAPI UI with .NET 9 - You just add `Scalar` and pick a theme. My theme of choice will be the `Deep Space` theme. The theme looks interesting, while still not being too extravagant, which makes it suitable for something you would use often.

So here's the line of code I'll add to all my new .NET projects:

```csharp
app.MapScalarApiReference(o => o.WithTheme(ScalarTheme.DeepSpace));
```

Hopefully, this article can serve as a reference for your own customizations. Leave your favorite theme in the comments, and ... claps are appreciated! 👏
