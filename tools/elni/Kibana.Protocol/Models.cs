using Newtonsoft.Json;

namespace Kibana.Protocol;

public record Indice([JsonProperty("data_stream")] string? DataStream);

public record SavedObjectCollection([JsonProperty("saved_objects")] SavedObject[] Items);

public record SavedObject(string Id, Attributes Attributes, Reference[] References);

public record Attributes(string Title);

public record Reference(string Id);

public record DataStreamCollection([JsonProperty("data_streams")] DataStream[] Items);

public record DataStream(string Name);

public record IndexCandidate(IndexPattern index_pattern);

public record IndexPattern(string title, string id);