using Microsoft.EntityFrameworkCore;
using Nist;

public static class CategoryAggregate
{
    public static async Task<Dictionary<string, TransactionGroup>> ToTransactionGroup<TKey>(this IQueryable<IGrouping<TKey, TransactionRecord>> query, IEnumerable<Path> includes)
    {
        var aggregate = await query.Select(cgr => new GroupAggregateDbResult
        {
            Key =  cgr.Key!.ToString()!,
            TotalSum = includes.Have("totalSum") ? cgr.Sum(x => x.Amount) : null,
            Total = includes.Have("total") ? cgr.Count() : null
        }).ToArrayAsync();

        return aggregate.ToResponseDictionary();
    }
}

public class GroupAggregateDbResult
{
    public required string Key { get; set;}
    public required decimal? TotalSum { get; set;}
    public required int? Total { get; set;}
}

public static class GroupAggregateDbResultExtensions
{
    public static Dictionary<string, TransactionGroup> ToResponseDictionary(this IEnumerable<GroupAggregateDbResult> dbResults)
    {
        return dbResults.ToDictionary(row => row.Key, row => new TransactionGroup (
            row.TotalSum,
            row.Total
        ));
    }
}

public record TransactionGroup(
    decimal? TotalSum,
    int? Total
);