// This program has been developed by students from the bachelor Computer Science at Utrecht
// University within the Software Project course.
// 
// © Copyright Utrecht University (Department of Information and Computing Sciences)

using System.Net.Http.Headers;
using System.Text.Json;
using ORCID.Net.JsonConverters;
using ORCID.Net.Models;
using ORCID.Net.ORCIDServiceExceptions;

namespace ORCID.Net.Services;

/// <summary>
/// Service for retrieving person data from the ORCID API.
/// </summary>
public class PersonRetrievalService : IPersonRetrievalService
{
    private static readonly MediaTypeWithQualityHeaderValue MediaHeader = new("application/vnd.orcid+json");

    /// <summary>
    /// Error message for deserialization failures.
    /// </summary>
    public const string DeserializationErrorMessage = "Failed to deserialize person";

    /// <summary>
    /// Error message for retrieval failures.
    /// </summary>
    public const string RetrievalErrorMessage = "Failed to retrieve person";

    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        Converters =
        {
            new PersonJsonConverter(),
        },
    };

    private readonly HttpClient _httpClient;
    private readonly PersonRetrievalServiceOptions _options;
    private readonly AuthResponse _authResponse;

    /// <summary>
    /// Initializes a new instance of the <see cref="PersonRetrievalService"/> class.
    /// </summary>
    /// <param name="options">The options for configuring the service.</param>
    public PersonRetrievalService(PersonRetrievalServiceOptions options)
    {
        _options = options;
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = _options.ApiUrl;
        
        _authResponse = GetAuthResponse() ??
            throw new OrcidServiceException("Failed to obtain authorization token", new());
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="PersonRetrievalService"/> class with specific clients for testing.
    /// </summary>
    /// <param name="options">The options for configuring the service.</param>
    /// <param name="httpClient">HTTP client to use for API requests.</param>
    /// <param name="authResponse">Authentication response to use instead of performing authentication.</param>
    internal PersonRetrievalService(
        PersonRetrievalServiceOptions options, 
        HttpClient httpClient,
        AuthResponse authResponse)
    {
        _options = options;
        _httpClient = httpClient;
        _httpClient.BaseAddress = _options.ApiUrl;
        _authResponse = authResponse;
    }

    private AuthResponse? GetAuthResponse(HttpClient? httpClient = null)
    {
        if (string.IsNullOrEmpty(_options.ClientId) || string.IsNullOrEmpty(_options.ClientSecret))
            throw new InvalidOperationException(
                "Client ID and Client Secret must be provided to initialize the authorization token.");

        // Use provided client or create a new one
        var client = httpClient ?? new HttpClient();
        
        FormUrlEncodedContent content = new([
            new("client_id", _options.ClientId),
            new("client_secret", _options.ClientSecret),
            new("scope", "/read-public"),
            new("grant_type", "client_credentials"),
        ]);

        var authUrl = new Uri(_options.BaseUrl, "oauth/token");
        HttpResponseMessage response = client.PostAsync(authUrl, content)
            .GetAwaiter()
            .GetResult();
        if (response.IsSuccessStatusCode)
        {
            string result = response.Content.ReadAsStringAsync()
                .GetAwaiter()
                .GetResult();
            return JsonSerializer.Deserialize<AuthResponse>(result);
        }

        throw new HttpRequestException($"Failed to obtain authorization token. Status code: {response.StatusCode}");
    }

    /// <summary>
    /// Finds a person by their ORCID ID.
    /// </summary>
    /// <param name="orcId">The ORCID ID of the person to find.</param>
    /// <returns>An <see cref="OrcidPerson"/> object representing the person.</returns>
    /// <exception cref="OrcidServiceException">Thrown when an error occurs during retrieval or deserialization.</exception>
    public async Task<OrcidPerson> FindPersonByOrcid(string orcId)
    {
        try
        {
            HttpRequestMessage request = new(HttpMethod.Get, $"{orcId}/person");

            request.Headers.Authorization = new("Bearer", _authResponse.Token);
            request.Headers.Accept.Add(MediaHeader);

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                throw new OrcidServiceException(RetrievalErrorMessage, new());

            OrcidPerson? person = await JsonSerializer.DeserializeAsync<OrcidPerson>(
                await response.Content.ReadAsStreamAsync(),
                JsonSerializerOptions);

            if (person == null)
                throw new OrcidServiceException(DeserializationErrorMessage, new());
            person.Orcid = orcId;
            return person;
        }
        catch (HttpRequestException e)
        {
            throw new OrcidServiceException(RetrievalErrorMessage, e);
        }
        catch (JsonException e)
        {
            throw new OrcidServiceException(DeserializationErrorMessage, e);
        }
    }

    /// <summary>
    /// Finds people by their name.
    /// </summary>
    /// <param name="personName">The name of the person to search for.</param>
    /// <param name="preferredAmountOfResults">The preferred number of results to return.</param>
    /// <returns>A list of <see cref="OrcidPerson"/> objects matching the search criteria.</returns>
    /// <exception cref="OrcidServiceException">Thrown when an error occurs during retrieval or deserialization.</exception>
    public async Task<List<OrcidPerson>> FindPeopleByName(string personName, int preferredAmountOfResults)
    {
        string queryUrl = "search?q=" + personName;
        var resultList = await SearchResultRequestAndParse<PersonSearchResult>(queryUrl, "result");
        List<OrcidPerson> returnList = [];
        for (int i = 0;
            i < Math.Min(resultList.Count, Math.Min(preferredAmountOfResults, _options.MaxResults));
            i++)
            returnList.Add(await FindPersonByOrcid(resultList[i].Id.Path!));

        return returnList;
    }

    /// <summary>
    /// Finds people by their name using the expanded search, which might be faster but has API version restrictions.
    /// </summary>
    /// <remarks>
    /// WARNING: This method is only compatible with the ORCID API v3.0 but this restriction is not enforced.
    /// Use the <see cref="FindPeopleByName"/> method for a more generic solution. You can expect an exception if you call this method
    /// with the wrong configuration.
    /// </remarks>
    /// <param name="personName">The name of the person to search for.</param>
    /// <returns>A list of <see cref="OrcidPerson"/> objects matching the search criteria.</returns>
    /// <exception cref="OrcidServiceException">Thrown when an error occurs during retrieval or deserialization.</exception>
    public async Task<List<OrcidPerson>> FindPeopleByNameFast(string personName)
    {
        string queryUrl = $"expanded-search?q={personName}";
        var resultList = await SearchResultRequestAndParse<PersonExpandedSearchResult>(queryUrl, "expanded-result");
        return resultList.Select(people => people.ToPerson()).ToList();
    }

    /// <summary>
    /// Performs a search request to the ORCID API and parses the results.
    /// </summary>
    /// <typeparam name="T">The type of the search result items.</typeparam>
    /// <param name="queryUrl">The query URL for the search.</param>
    /// <param name="jsonListElement">The name of the JSON element containing the list of results.</param>
    /// <returns>A list of search result items of type <typeparamref name="T"/>.</returns>
    /// <exception cref="OrcidServiceException">Thrown when an error occurs during the request or parsing.</exception>
    public async Task<List<T>> SearchResultRequestAndParse<T>(string queryUrl, string jsonListElement) where T : class
    {
        try
        {
            HttpRequestMessage request = new(HttpMethod.Get, queryUrl);
            request.Headers.Authorization = new("Bearer", _authResponse.Token);
            request.Headers.Accept.Add(MediaHeader);

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                throw new OrcidServiceException("Failed to retrieve person", new());

            string text = await response.Content.ReadAsStringAsync();
            JsonDocument doc = JsonDocument.Parse(text);

            if (doc.RootElement.GetProperty(jsonListElement).ValueKind == JsonValueKind.Null)
                return [];
            var resultListJson = doc.RootElement.GetProperty(jsonListElement).EnumerateArray().ToArray();

            List<T> resultList = resultListJson
                .Select(element => JsonSerializer.Deserialize<T>(element.GetRawText()))
                .Where(result => result != null)
                .ToList()!;
            return resultList;
        }
        catch (HttpRequestException e)
        {
            throw new OrcidServiceException(RetrievalErrorMessage, e);
        }
        catch (JsonException e)
        {
            throw new OrcidServiceException(DeserializationErrorMessage, e);
        }
    }
}
