[Route(Uris.Indexes)]
public class IndexController : Controller
{
    static readonly string[] expected = { "logs-*" };

    Kibana.Protocol.Client Kibana { get; }

    public IndexController(Kibana.Protocol.Client kibana) {
        this.Kibana = kibana;
    }

    [HttpGet]
    public async Task<IndexCollection> Get() {
        var rawIndexes = await UnknownKibanaException.Wrap(this.Kibana.GetIndexes());
        var existing = rawIndexes.Items.Select(i => i.Id).ToArray();

        var pending = expected.Except(existing).ToArray();

        return new(existing, pending);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] string index) {
        var candidate = new Kibana.Protocol.IndexCandidate(new(index, index, "@timestamp"));
        await UnknownKibanaException.Wrap(this.Kibana.PostIndex(candidate), 
            ex => { if (ex.ErrorMessage.StartsWith("Duplicate index pattern")) throw new DuplicateException(); }
        );
        return this.Json(index);
    }

    [HttpDelete("{id}")]
    public async Task Delete([FromRoute] string id) {
        await UnknownKibanaException.Wrap(this.Kibana.DeleteIndex(id));
    }

    public class DuplicateException : Exception {}
}