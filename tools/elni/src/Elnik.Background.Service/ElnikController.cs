public class ElnikController
{
    readonly Client client;
    public ElnikController(Client client) { this.client = client; }

    [RunsEvery("00:00:30")]
    public async Task<string[]> EnsureIndexesCreated()
    {
        var indexes = await this.client.GetIndexes();
        foreach (var pending in indexes.Pending) await this.client.PostIndex(pending);

        return indexes.Pending;
    }

    [RunsEvery("00:00:30")]
    public async Task<string[]> EnsureDashboardsCreated() 
    {
        var dashboards = await this.client.GetDashboards();
        foreach (var pending in dashboards.Pending)
            await this.client.PutDashboard(pending);

        return dashboards.Pending;
    }
}