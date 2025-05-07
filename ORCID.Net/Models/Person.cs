using System.Text.Json.Serialization;

namespace ORCID.Net.Models;

public class Person
{
    [JsonPropertyName("given-name")]
    public required string FirstName { get; set; }
    
    [JsonPropertyName("family-name")]
    public required string LastName { get; set; }
    
    [JsonPropertyName("credit-name")]
    public required string CreditName { get; set; }
    
    [JsonPropertyName("biography")]
    public required string Biography { get; set; }
}