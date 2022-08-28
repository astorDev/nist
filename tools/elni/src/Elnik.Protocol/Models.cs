namespace Elnik.Protocol;

public record About(string Description, string Version, string Environment);

public record DashboardCollection(string[] Existing, string[] Pending, string[] DataStreams);

public record Dashboard(string Name, string IndexPattern);

public record NisterDashboardCandidate(string ServiceName);

public record IndexCollection(string[] Existing, string[] Pending);