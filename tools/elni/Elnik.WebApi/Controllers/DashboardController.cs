[Route(Uris.Dashboards)]
public class DashboardController
{
    private static readonly string ImportTemplate = File.ReadAllText("dashboardImportTemplate.json");
    private const string NisterDashboardMarker = $"nister";
    
    public Kibana.Protocol.Client Kibana { get; }
    public ILogger<DashboardController> Logger { get; }

    public DashboardController(Kibana.Protocol.Client kibana, ILogger<DashboardController> logger) {
        this.Kibana = kibana;
        this.Logger = logger;
    }

    [HttpGet]
    public async Task<DashboardCollection> GetOverview(){
        var rawDashboards = await UnknownKibanaException.Wrap(Kibana.GetDashboards());
        var rawDataStreams = await UnknownKibanaException.Wrap(Kibana.GetDataStreams());

        var dashboards = rawDashboards.Items.Select(i => i.Id).ToArray();
        var dataStreams = rawDataStreams.Items.Select(i => i.Name).ToArray();

        var allNisters = dataStreams.Where(d => d.StartsWith("logs-nist-")).Select(nd => nd.Split('-')[2]);
        var dashboardedNisters = dashboards.Where(d => d.StartsWith(NisterDashboardMarker)).Select(d => d.Split(' ')[1]);

        var pendingNisters = allNisters.Except(dashboardedNisters).ToArray();
        
        return new(dashboards, pendingNisters, dataStreams);
    }

    [HttpPut(Uris.Nisters)]
    public async Task<Dashboard> PutNistDashboard([FromBody] NisterDashboardCandidate candidate)
    {
        var name = $"{NisterDashboardMarker} {candidate.ServiceName}";
        var indexPatternId = $"logs-nist-{candidate.ServiceName}";
        var indexPattern = $"logs-nist-{candidate.ServiceName}-*";

        var importJson = ImportTemplate
            .Replace("{{dashboardTitle}}", name)
            .Replace("{{indexPatternId}}", indexPatternId)
            .Replace("{{indexPattern}}", indexPattern)
            .Replace("{{dashboardId}}", name);

        var importObject = JsonSerializer.Deserialize<object>(importJson)!;
        await UnknownKibanaException.Wrap(this.Kibana.PostDashboard(importObject));
        
        return new Dashboard(name, indexPattern);
    }

    [HttpDelete("{id}")]
    public async Task DeleteDashboard([FromRoute] string id) {
        await UnknownKibanaException.Wrap(this.Kibana.DeleteDashboard(id));
    }
}
