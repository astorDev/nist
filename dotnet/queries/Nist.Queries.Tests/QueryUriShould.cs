using System;
using System.Collections.Generic;

using FluentAssertions;

namespace Nist.Queries.Tests;

[TestClass]
public class QueryUriShould
{
    [TestMethod]
    public void NotAppendQuestionMarkWhenThereAreNoParams()
    {
        string uri = new QueryUri("resource", Array.Empty<QueryKeyValue>());
        uri.Should().Be("resource");   
    }

    [TestMethod]
    public void DisplayCorrectlyUriWithMultipleParams()
    {
        string uri = new QueryUri("resource", new QueryKeyValue[] 
        {
            new ("name", "george"),
            new ("age", "17")
        });

        uri.Should().Be("resource?name=george&age=17");
    }

    [TestMethod]
    public void DisplayCorrectlyDateBasedQuery()
    {
        string uri = QueryUri.From("resource", new Query(From: new DateTime(2020, 1, 10, 11, 30, 10)));

        uri.Should().Be("resource?from=2020-01-10T11:30:10.0000000");
    }

    [TestMethod]
    public void DisplayCorrectlyQueryWithEnumerable()
    {
        string uri = QueryUri.From("resource", new Query(Names: new [] { "george", "john" }));

        uri.Should().Be("resource?names=george&names=john");
    }

    [TestMethod]
    public void ProduceCorrectQueryForDictionary()
    {
        string uri = QueryUri.From("resource", new Query(Tags: new Dictionary<string, object> {
            { "category", "test" },
            { "createdAt", new DateTime(2020, 2, 20, 12, 16, 00) }
        }));

        uri.Should().Be("resource?tags.category=test&tags.createdAt=2020-02-20T12:16:00.0000000");
    }

    public record Query(DateTime? From = null, string[]? Names = null, Dictionary<string, object>? Tags = null);
}