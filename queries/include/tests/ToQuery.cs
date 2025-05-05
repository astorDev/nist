using Shouldly;

namespace Nist.Queries.Include.Tests;

[TestClass]
public sealed class ToQuery
{
    [TestMethod]
    public void Standard()
    {
        var query = new QueryWithInclude(
            IncludeQueryParameter.Parse("total,items.name")
        );

        var uri = QueryUri.From("resource", query);
        uri.ToString().ShouldBe("resource?include=total%2Citems.name");
    }
}

public record QueryWithInclude(
    IncludeQueryParameter Include
);