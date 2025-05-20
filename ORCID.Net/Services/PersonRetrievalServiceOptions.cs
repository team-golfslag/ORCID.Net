


using System.Text.Json;
using ORCID.Net.Models;

namespace ORCID.Net.Services;


public class PersonRetrievalServiceOptions
{

    public const string OrcidSandboxUrl = "https://pub.sandbox.orcid.org/v3.0/";
    
    public const string OrcidSandboxUrlPreviousVersion = "https://pub.sandbox.orcid.org/v2.1/";
    
    public const string OrcidProductionUrl = "https://api.orcid.org/v3.0/";
    
    public const string OrcidProductionUrlPreviousVersion = "https://api.orcid.org/v2.1/";
    
    public const string OrcidSandboxAuthUrl = "https://sandbox.orcid.org/oauth/authorize";
    public const string OrcidProductionAuthUrl = "https://orcid.org/oauth/authorize";

    public const string JsonMediaHeader = "application/vnd.orcid+json";

    //Current implementation of searching by name on orcid means only getting matching ID's back not the actual names
    //which means that we then have to fetch the name for each individual ID as well which is expensive therefore we limit this.
    public const int MaxRecommendedResults = 15;
    public string RequestUrl { get; set; }
    
    public string AuthUrl { get; set; }

    public string MediaHeader { get; set; }

    public int MaxResults { get; set; }

    public string? AuthorizationCode { get; set; }
    
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
    
    public DateTime? ExpirationDate { get; set; }
    
    public PersonRetrievalServiceOptions(string authorizationCode, string requestUrl, string mediaHeader, int maxResults){
        AuthorizationCode = authorizationCode;
        RequestUrl = requestUrl;
        MediaHeader = mediaHeader;
        MaxResults = maxResults;
        AuthUrl = OrcidSandboxAuthUrl;
    }


    public PersonRetrievalServiceOptions(OrcidType type, string clientId, string clientSecret, int maxResults)
    {
        switch (type)
        {
            case OrcidType.Sandbox:
                RequestUrl = OrcidSandboxUrl;
                AuthUrl = OrcidSandboxAuthUrl;
                ClientId = clientId;
                ClientSecret = clientSecret;
                break;
            case OrcidType.SandboxPreviousVersion:
                RequestUrl = OrcidSandboxUrlPreviousVersion;
                AuthUrl = OrcidSandboxAuthUrl;
                ClientId = clientId;
                ClientSecret = clientSecret;
                break;
            case OrcidType.ProductionPreviousVersion:
                RequestUrl = OrcidProductionUrlPreviousVersion;
                AuthUrl = OrcidProductionAuthUrl;
                ClientId = clientId;
                ClientSecret = clientSecret;
                break;
            default:
                RequestUrl = OrcidProductionUrl;
                AuthUrl = OrcidProductionAuthUrl;
                ClientId = clientId;
                ClientSecret = clientSecret;
                break;
            
            
        }
        MediaHeader = JsonMediaHeader;
        MaxResults = maxResults;
    }
        
    

    public virtual HttpClient BuildRequestClient()
    {
        HttpClient client = new HttpClient();
        client.BaseAddress = new Uri(RequestUrl);
        return client;
    }

    public async Task InitializeAuthorizationToken()
    {
        HttpClient client = new HttpClient();
        HttpContent content = new StringContent($"client_id={this.ClientId}&client_secret={this.ClientSecret}&grant_type=client_credentials&scope=/read-public");
        var request = new HttpRequestMessage(HttpMethod.Post, AuthUrl);
        request.Content = content;
        request.Headers.Add("Accept", "application/json");
        var response = await client.SendAsync(request);
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadAsStringAsync();
            var token = JsonSerializer.Deserialize<AuthResponse>(result);
            if (token != null)
            {
                AuthorizationCode = token.Token;
                ExpirationDate = DateTime.UtcNow.AddSeconds(token.ExpiresIn ?? 0);
            }
        }
       
    }
    
    public bool IsAuthTokenValid()
    {
        return DateTime.UtcNow < ExpirationDate;
    }
    
    public enum OrcidType
    {
        Sandbox,
        SandboxPreviousVersion,
        Production,
        ProductionPreviousVersion
    }
}