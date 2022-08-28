namespace Kibana.Protocol;

public class Uris
{
    public const string Dashboards = "api/saved_objects/_find?default_search_operator=AND&has_reference=%5B%5D&page=1&per_page=1000&search_fields=title%5E3&search_fields=description&type=dashboard";
    public const string Indexes = "api/saved_objects/_find?fields=title&fields=type&fields=typeMeta&per_page=10000&type=index-pattern";
    public const string DataStreams = "/internal/index-pattern-management/resolve_index/*";
    public const string DashboardImport = "/api/kibana/dashboards/import";
    public const string IndexPattern = "/api/index_patterns/index_pattern";
    public static string SavedDashboard(string id) => $"api/saved_objects/dashboard/{id}";
    public static string SavedIndex(string id) => $"/api/index_patterns/index_pattern/{id}";
}