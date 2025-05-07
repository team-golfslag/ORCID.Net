using System.Text.Json.Serialization;

namespace ORCID.Net.Models;

public class OrcidIdentifier
{
    [JsonPropertyName("uri")]
    public string Uri { get; set; }
    [JsonPropertyName("path")]
    public string Path { get; set; }
    [JsonPropertyName("host")]
    public string Host { get; set; }
}