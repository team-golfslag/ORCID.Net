using System.Text.Json;
using System.Text.Json.Serialization;

namespace ORCID.Net.Services;

public class PersonRetrievalServiceOptions
{
    public string BaseUrl { get; set; } = "https://pub.sandbox.orcid.org/v3.0/";
    public string MediaHeader { get; set; } = "Accept: application/vnd.orcid+json";
    
    public string AuthorizationCode { get; set; } = "";
    
    public JsonSerializerOptions JsonSerializerOptions { get; set; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.KebabCaseLower,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower),
        },
    };
}