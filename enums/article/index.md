## Why Over-Engineer: Auto-Generated Enum Numbers.

```csharp
public enum Status
{
    Pending, // 0
    Processed // 1
}
```

```csharp
public enum Status
{
    Pending, // 0
    Processing, // ❗ overtakes 1
    Completed // becomes 2
}
```

- ❌ Explicit Breaking-Change Sensibility
- ❌ Descriptive API contracts.
- ✅ Easy Rename.
- ❌ Support for Unknown Values.
- ✅ Easy Setup.

## A Better Approach: Explicit Enum Codes.

- 🍊 Explicit Breaking-Change Sensibility
- 🍊 Descriptive API contracts.
- ✅ Easy Rename.
- ❌ Support for Unknown Values.
- ✅ Easy Setup.

```csharp
public enum Status
{
    Pending = 100,
    Processing = 150, // can be put "in the middle"
    Completed = 200 // works
}
```

## Chasing Descriptiveness: Enum .ToString()

- 🍊 Explicit Breaking-Change Sensibility
- ✅ Descriptive API contracts.
- ❌ Easy Rename.
- ❌ Support for Unknown Values.
- ✅ Easy Setup.

💡 Other:

- 🍊 Theoretically configurable naming convention (like the UPPERCASE)

```csharp
public enum Status
{
    Pending
    Processed
}
```

```csharp
public enum Status
{
    Pending,
    Completed // Processed is no longer accepted
}
```

## Almost Ultimate: Const Strings

> Assumes that the Status stores as strings as well.

- ✅ Explicit Breaking-Change Sensibility
- ✅ Descriptive API contracts.
- 🍊 Easy Rename.
- ✅ Support for Unknown Values.
- ❌ Easy Setup.

💡 Other:

- ✅ Support for arbitrary naming convention (like the UPPERCASE)
- 🍊 No `Data -> Domain -> Contract` mapping. `Data -> Contract` is super easy though.

```csharp
namespace Contracts
{
    public class Status
    {
        public const string Pending = "PENDING";
        public const string Processing = "PROCESSING";

        [Obsolete("Use Completed")]
        public const string Processed = "PROCESSED";
        public const string Completed = "COMPLETED";
    }
}

namespace Domain
{
    public enum Status
    {
        Pending,
        Processing,
        Completed, // can be renamed without 
    
        // Sometimes we can allow new values without a code migration.
        Unknown
    }
}

namespace Host
{
    public static class Mapper
    {
        public static Status ToStatus(this string statusString)
        {
            return statusString switch
            {
                "PENDING" => Status.Pending,
                "PROCESSING" => Status.Processing,
                "COMPLETED" or "PROCESSED" => Status.Completed,
                _ => Status.Unknown
            }
        }
    }
}
```