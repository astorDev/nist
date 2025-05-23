namespace Nist;

// public record ObjectPath(
//     string Root,
//     ObjectPath? Child
// )
// {
//     public static ObjectPath Parse(string path, string separator = ".")
//     {
//         var parts = path.Split(separator);
//         return Parse(parts);
//     }

//     public static ObjectPath Parse(string[] parts) => new(
//         parts[0],
//         parts.Length > 1 ? Parse(parts[1..]) : null
//     );

//     override public string ToString()
//     {
//         return ToString(".");
//     }

//     public string ToString(string separator)
//     {
//         return Child != null ? $"{Root}{separator}{Child}" : Root;
//     }
// }

// public static class ObjectPathEnumerableExtensions
// {
//     public static IEnumerable<ObjectPath> GetChildren(this IEnumerable<ObjectPath> pathes, string key) => 
//         pathes
//             .Where(p => p.Root == key && p.Child != null)
//             .Select(p => p.Child!);

//     public static bool Have(this IEnumerable<ObjectPath> pathes, string key) => 
//         pathes.Any(p => p.Root == key);
// }