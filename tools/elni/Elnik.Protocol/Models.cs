namespace Elnik.Protocol;

public record About(string Description, string Version, string Environment);

public record DashboardCollection(string[] Dashboards, string[] PendingNisters, string[] DataStreams);

public record Dashboard(string Name, string IndexPattern);

public record NisterDashboardCandidate(string ServiceName);

public record IndexCollection(string[] Indexes, string[] Pending);