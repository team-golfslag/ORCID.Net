


namespace ORCID.Net.Services;


public class PersonRetrievalServiceOptions(string authorizationCode, string baseUrl, string mediaHeader, int maxResults)
{

    public const string OrcidSandboxUrl = "https://pub.sandbox.orcid.org/v3.0/";
    
    public const string OrcidSandboxUrlPreviousVersion = "https://pub.sandbox.orcid.org/v2.1/";

    public const string JsonMediaHeader = "application/vnd.orcid+json";

    //Current implementation of searching by name on orcid means only getting matching ID's back not the actual names
    //which means that we then have to fetch the name for each individual ID as well which is expensive therefore we limit this.
    public const int MaxRecommendedResults = 15;
    public string BaseUrl { get; set; } = baseUrl;

    public string MediaHeader { get; set; } = mediaHeader;

    public int MaxResults { get; set; } = maxResults;

    public string AuthorizationCode { get; set; } = authorizationCode;


    public PersonRetrievalServiceOptions() : this("", OrcidSandboxUrl, JsonMediaHeader, MaxRecommendedResults)
    {
    }

    public virtual HttpClient BuildHttpClient()
    {
        HttpClient client = new HttpClient();
        client.BaseAddress = new Uri(BaseUrl);
        return client;
    }
}