using System.Linq.Expressions;
using Nist;

public record AmountExpression(string Operator, decimal Value)
{
    public const string FieldKey = "amount";
    public const string GteOperator = "gte";

    public static AmountExpression From(IncludeExpression source)
    {
        return new AmountExpression(
            Operator: source.Operator,
            Value: decimal.Parse(source.Value)
        );
    }

    public static Expression<Func<Transaction, bool>> Gte(decimal value) => x => x.Amount >= value;

    public Expression<Func<Transaction, bool>> ToExpression()
    {
        return Operator switch
        {
            GteOperator => Gte(Value),
            _ => throw new($"operator '{Operator}' not found")
        };
    }

    public override string ToString()
    {
        return $"{Operator}_{Value}";
    }
}

public static class AmountGroupExtensions
{
    public static async Task<Dictionary<string, Dictionary<string, TransactionGroup>>?> SearchAmountGroup(this IQueryable<Transaction> query, ObjectPath[] groupPathes)
    {
        var amountPathes = groupPathes.GetChildren("amount")
            .GetExpressionSubpathes()
            .NewKeys(AmountExpression.From)
            .GroupToDictionary();

        if (amountPathes.Count == 0) return null;

        var result = new Dictionary<string, Dictionary<string, TransactionGroup>>();
        
        foreach (var pair in amountPathes)
        {
            var groups = await query.GroupBy(pair.Key.ToExpression()).ToTransactionGroup(pair.Value);
            result.Add(pair.Key.ToString(), groups);
        }

        return result;
    }
}