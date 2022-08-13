[Route(Uris.Dashboards)]
public class DashboardController
{
    static readonly string importTemplate = File.ReadAllText("dashboardImportTemplate.json");
    const string NisterDashboardMarker = "nister";
    
    public Kibana.Protocol.Client Kibana { get; }

    public DashboardController(Kibana.Protocol.Client kibana) {
        this.Kibana = kibana;
    }

    [HttpGet]
    public async Task<DashboardCollection> GetOverview() {
        var rawDashboards = await UnknownKibanaException.Wrap(this.Kibana.GetDashboards());
        var rawDataStreams = await UnknownKibanaException.Wrap(this.Kibana.GetDataStreams());

        var dashboards = rawDashboards.Items.Select(i => i.Id).ToArray();
        var dataStreams = rawDataStreams.Items.Select(i => i.Name).ToArray();

        var allNisters = dataStreams.Where(d => d.StartsWith("logs-nist-")).Select(nd =>
        {
            var parts = nd.Split('-');
            return $"{parts[2]}-{parts[3]}";
        });
        var dashboardedNisters = dashboards.Where(d => d.StartsWith(NisterDashboardMarker)).Select(d => d.Split(' ')[1]);

        var pendingNisters = allNisters.Except(dashboardedNisters).ToArray();
        
        return new(dashboards, pendingNisters, dataStreams);
    }

    [HttpPut(Uris.Nisters)]
    public async Task<Dashboard> PutNistDashboard([FromBody] NisterDashboardCandidate candidate) {
        var name = $"{NisterDashboardMarker} {candidate.ServiceName}";
        var indexPatternId = $"logs-nist-{candidate.ServiceName}";
        var indexPattern = $"logs-nist-{candidate.ServiceName}-*";

        var importJson = importTemplate
            .Replace("{{dashboardTitle}}", name)
            .Replace("{{indexPatternId}}", indexPatternId)
            .Replace("{{indexPattern}}", indexPattern)
            .Replace("{{dashboardId}}", name);

        var importObject = JsonSerializer.Deserialize<object>(importJson)!;
        await UnknownKibanaException.Wrap(this.Kibana.PostDashboard(importObject));
        
        return new(name, indexPattern);
    }

    [HttpDelete("{id}")]
    public async Task DeleteDashboard([FromRoute] string id) {
        await UnknownKibanaException.Wrap(this.Kibana.DeleteDashboard(id));
    }
}
