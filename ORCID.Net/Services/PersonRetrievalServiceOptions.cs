


namespace ORCID.Net.Services;


public class PersonRetrievalServiceOptions
{

    public const string OrcidSandboxUrl = "https://pub.sandbox.orcid.org/v3.0/";

    public const string JsonMediaHeader = "application/vnd.orcid+json";
    
    public const string XmlMediaHeader = "application/vnd.orcid+xml";
    
    //Current implementation of searching by name on orcid means only getting matching ID's back not the actual names
    //which means that we then have to fetch the name for each individual ID as well which is expensive therefore we limit this.
    public const int MaxRecommendedResults = 15;
    public string BaseUrl { get; set; }

    public string MediaHeader { get; set; }
    
    public int MaxResults { get; set; } 
    
    public string AuthorizationCode { get; set; }

    public PersonRetrievalServiceOptions(string authorizationCode)
    {
        AuthorizationCode = authorizationCode;
        BaseUrl = OrcidSandboxUrl;
        MediaHeader = JsonMediaHeader;
        MaxResults = MaxRecommendedResults;
    }
    
    public PersonRetrievalServiceOptions()
    {
        AuthorizationCode = "";
        BaseUrl = OrcidSandboxUrl;
        MediaHeader = JsonMediaHeader;
        MaxResults = MaxRecommendedResults;
    }

    public PersonRetrievalServiceOptions(string authorizationCode, string baseUrl, string mediaHeader, int maxResults)
    {
        AuthorizationCode = authorizationCode;
        BaseUrl = baseUrl;
        MediaHeader = mediaHeader;
        MaxResults = maxResults;
    }

    public virtual HttpClient BuildHttpClient()
    {
        HttpClient client = new HttpClient();
        client.BaseAddress = new Uri(BaseUrl);
        return client;
    }
}