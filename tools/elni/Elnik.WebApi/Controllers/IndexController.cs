[Route(Uris.Indexes)]
public partial class IndexController
{
    public static string[] Expecteds = new [] { "logs-*" };

    public Kibana.Protocol.Client Kibana { get; }

    public IndexController(Kibana.Protocol.Client kibana) {
        this.Kibana = kibana;
    }

    [HttpGet]
    public async Task<IndexCollection> Get() {
        var rawIndexes = await UnknownKibanaException.Wrap(this.Kibana.GetIndexes());
        var indexes = rawIndexes.Items.Select(i => i.Id).ToArray();

        var pending = Expecteds
            .Except(indexes)
            .ToArray();

        return new(indexes, pending);
    }

    [HttpPost]
    public async Task<string> Post([FromBody] string index) {
        var candidate = new Kibana.Protocol.IndexCandidate(new Kibana.Protocol.IndexPattern(index, index));
        await UnknownKibanaException.Wrap(Kibana.PostIndex(candidate), 
            (ex) => { if (ex.ErrorMessage.StartsWith("Duplicate index pattern")) throw new DuplicateException(); }
        );
        return index;
    }

    [HttpDelete("{id}")]
    public async Task Delete([FromRoute] string id) {
        await UnknownKibanaException.Wrap(Kibana.DeleteIndex(id));
    }

    public class DuplicateException : Exception {}
}