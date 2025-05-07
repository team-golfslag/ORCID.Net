using System.Text.Json;
using System.Text.Json.Serialization;

namespace ORCID.Net.Services;

public class PersonRetrievalServiceOptions
{
    public string BaseUrl { get; set; } = "https://pub.sandbox.orcid.org/v3.0/";

    public string MediaHeader { get; set; } = "application/vnd.orcid+json";
    
    public int MaxResults { get; set; } = 15;
    
    public string AuthorizationCode { get; set; } = "";

    public PersonRetrievalServiceOptions(string authorizationCode)
    {
        AuthorizationCode = authorizationCode;
    }


}