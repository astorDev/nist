using Elnik.WebApi.Misc;

[Route(Uris.Dashboards)]
public class DashboardController
{
    static readonly string ImportTemplate = File.ReadAllText("dashboardImportTemplate.json");
    static readonly string[] Others = { "nisters" };
    static readonly string NisterPrefix = "nisters:";
    
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
        var dashboardedNisters = dashboards.Where(d => d.StartsWith(NisterPrefix)).Select(d => d.Replace(NisterPrefix, ""));

        var pendingNisters = allNisters.Except(dashboardedNisters).ToArray();
        var pendingNistersDashboards = pendingNisters.Select(n => $"{NisterPrefix}{n}");
        var otherPendingDashboards = Others.Except(dashboards);

        return new(dashboards, pendingNistersDashboards.Union(otherPendingDashboards).ToArray(), dataStreams);
    }



    [HttpPut("{name}")]
    public async Task<Dashboard> Put([FromRoute] string name)
    {
        if (name.StartsWith(NisterPrefix)) return await PutNister(name.Replace(NisterPrefix, ""));
        
        var json = DashboardNotFoundException.Wrap<FileNotFoundException>(() => File.ReadAllText($"{name}.json"));
        var importObject = JsonSerializer.Deserialize<object>(json)!;
        dynamic? response = await UnknownKibanaException.Wrap(this.Kibana.PostDashboard(importObject));

        string dashboardId = response?.objects[0].id!;
        string indexId = response?.objects[1].id!;
        return new(dashboardId, indexId);
    }

    [HttpDelete("{id}")]
    public async Task Delete([FromRoute] string id) {
        await UnknownKibanaException.Wrap(this.Kibana.DeleteDashboard(id));
    }
}
