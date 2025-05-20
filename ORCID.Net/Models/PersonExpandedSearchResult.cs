// This program has been developed by students from the bachelor Computer Science at Utrecht
// University within the Software Project course.
// 
// © Copyright Utrecht University (Department of Information and Computing Sciences)

using System.Text.Json.Serialization;

namespace ORCID.Net.Models;

public class PersonExpandedSearchResult
{
    [JsonPropertyName("orcid-id")]
    public string? Orcid { get; set; }
    [JsonPropertyName("given-names")]
    public string? FirstName { get; set; }
    [JsonPropertyName("family-names")]
    public string? LastName { get; set; }
    [JsonPropertyName("credit-name")]
    public string? CreditName { get; set; }
    
    public OrcidPerson ToPerson()
    {
        return new OrcidPerson(FirstName, LastName, CreditName, null, Orcid);
    }
}
