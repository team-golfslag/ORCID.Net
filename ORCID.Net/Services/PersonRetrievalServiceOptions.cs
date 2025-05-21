// This program has been developed by students from the bachelor Computer Science at Utrecht
// University within the Software Project course.
// 
// © Copyright Utrecht University (Department of Information and Computing Sciences)

namespace ORCID.Net.Services;

/// <summary>
/// Options for configuring the <see cref="PersonRetrievalService"/>.
/// </summary>
public class PersonRetrievalServiceOptions
{
    /// <summary>
    /// The default API version to use if not specified.
    /// </summary>
    public const string DefaultApiVersion = "v3.0";

    /// <summary>
    /// The maximum recommended number of results to fetch when searching by name.
    /// The current implementation of searching by name on ORCID means only getting matching ID's back, not the actual names,
    /// which means that we then have to fetch the name for each individual ID as well, which is expensive; therefore, we limit this.
    /// </summary>
    public const int MaxRecommendedResults = 15;
    
    public PersonRetrievalServiceOptions(
        string baseUrl,
        string clientId,
        string clientSecret,
        string? apiVersion = DefaultApiVersion,
        int maxResults = MaxRecommendedResults)
    {
        ClientId = clientId;
        ClientSecret = clientSecret;
        BaseUrl = new(baseUrl);
        // Format public api URL
        ApiUrl = new($"{BaseUrl.Scheme}://pub.{BaseUrl.Host}/{apiVersion}/");
        MaxResults = maxResults;
    }

    /// <summary>
    /// Gets or sets the base URL for API requests (without version).
    /// </summary>
    public Uri BaseUrl { get; set; }
    
    /// <summary>
    /// Gets or sets the full request URL (base URL + version).
    /// </summary>
    public Uri ApiUrl { get; set; }
    
    /// <summary>
    /// Gets or sets the maximum number of results to retrieve.
    /// </summary>
    public int MaxResults { get; set; }

    /// <summary>
    /// Gets or sets the client ID for OAuth.
    /// </summary>
    public string? ClientId { get; set; }
    
    /// <summary>
    /// Gets or sets the client secret for OAuth.
    /// </summary>
    public string? ClientSecret { get; set; }
}
