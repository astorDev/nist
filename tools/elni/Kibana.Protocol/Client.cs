using System.Net.Http.Json;

using Nist.Responses;

namespace Kibana.Protocol;

public class Client
{
    public HttpClient Http { get; }

    public Client(HttpClient http)
    {
        this.Http = http;
    }

    public Task<SavedObjectCollection> GetDashboards() =>
        this.Http.GetAsync(Uris.Dashboards).Read<SavedObjectCollection>();

    public Task<SavedObjectCollection> GetIndexes() =>
        this.Http.GetAsync(Uris.Indexes).Read<SavedObjectCollection>();

    public Task<DataStreamCollection> GetDataStreams() =>
        this.Http.GetAsync(Uris.DataStreams).Read<DataStreamCollection>();

    public Task<object> PostDashboard(object request) =>
        this.Http.PostAsJsonAsync(Uris.DashboardImport, request).Read<object>();

    public Task<dynamic?> DeleteDashboard(string id) =>
        this.Http.DeleteAsync(Uris.SavedDashboard(id)).ReadNullable<dynamic>();

    public Task<dynamic> PostIndex(IndexCandidate candidate) =>
        this.Http.PostAsJsonAsync(Uris.IndexPattern, candidate).Read<dynamic>();

    public Task<dynamic?> DeleteIndex(string id) =>
        this.Http.DeleteAsync(Uris.SavedIndex(id)).ReadNullable<dynamic>();
}