// This program has been developed by students from the bachelor Computer Science at Utrecht
// University within the Software Project course.
// 
// © Copyright Utrecht University (Department of Information and Computing Sciences)

using System.Text.Json.Serialization;

namespace ORCID.Net.Models;

public class PersonSearchResult
{
    [JsonPropertyName("orcid-identifier")]
    public required OrcidIdentifier Id { get; set; }
}
