public record TransactionCollection(
    int Count,
    Transaction[] Items,
    int? Total = null,
    TransactionGroupCollection? Groups = null
);

public record TransactionGroupCollection(
    Dictionary<string, TransactionGroup>? Category = null,
    Dictionary<string, Dictionary<string, TransactionGroup>>? Amount = null
);

public record TransactionGroup(
    decimal? TotalSum,
    int? Total
);

public class Transaction
{
    public long Id { get; set; }
    public required string Category { get; set; }
    public required decimal Amount { get; set; }
}