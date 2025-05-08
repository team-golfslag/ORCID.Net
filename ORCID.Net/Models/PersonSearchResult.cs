using System.Text.Json.Serialization;

namespace ORCID.Net.Models;

public class PersonSearchResult
{
    [JsonPropertyName("orcid-identifier")]
    public OrcidIdentifier Id { get; set; }
}
