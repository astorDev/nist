namespace Elnik.Protocol;

public class Uris
{
    public const string About = "about";
    public const string Overview = "overview";
    public const string Dashboards = "dashboards";
    public const string Nisters = "nisters";
    public const string Indexes = "indexes";

    public const string NisterDashboards = $"{Dashboards}/{Nisters}";
    public static string Dashboard(string name) => $"{Dashboards}/{name}";
}