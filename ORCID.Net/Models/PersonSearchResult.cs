using System.Text.Json.Serialization;

namespace ORCID.Net.Models;

public class PersonSearchResult
{
    [JsonPropertyName("orcid-identifier")]
    public required OrcidIdentifier Id { get; set; }
}
