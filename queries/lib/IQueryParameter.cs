namespace Nist;

/// <summary>
/// Marker interface for types that should use <see cref="ToString()"/> for representing as query parameters.
/// </summary>
public interface IQueryParameter
{
    string ToString();
}