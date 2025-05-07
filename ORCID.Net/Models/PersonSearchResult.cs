using System.Text.Json.Serialization;

namespace ORCID.org.Models;

public class PersonSearchResult
{
    [JsonPropertyName("orcid-identifier")]
    public OrcidIdentifier Id { get; set; }
}
