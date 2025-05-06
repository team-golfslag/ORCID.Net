namespace ORCID.org.Models;
using System.Text.Json.Serialization;

public class PersonSearchResponse
{
    public List<PersonSearchResult> Result { get; set; }
    
    [JsonPropertyName("num-found")]
    public int ResultCount { get; set; }
}