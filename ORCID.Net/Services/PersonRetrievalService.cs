using System.Text.Json;
using ORCID.Net.Models;
using ORCID.Net.JsonConverters;
using ORCID.Net.ORCIDServiceExceptions;

namespace ORCID.Net.Services;

public class PersonRetrievalService : IPersonRetrievalService
{
    public const string DeserializationErrorMessage = "Failed to deserialize person";
    public const string RetrievalErrorMessage = "Failed to retrieve person";


    private readonly HttpClient _httpClient;
    private readonly PersonRetrievalServiceOptions _options;

    public PersonRetrievalService(PersonRetrievalServiceOptions options)
    {
        _options = options;
        _httpClient = _options.BuildRequestClient();
    }

    private static JsonSerializerOptions _jsonSerializerOptions = new()
    {
        Converters =
        {
            new PersonJsonConverter()
        }
    };

    public async Task<OrcidPerson> FindPersonByOrcid(string orcId)
    {
        try
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"{orcId}/person");

            request.Headers.Authorization = new("Bearer", _options.AuthorizationCode);
            request.Headers.Accept.Add(new(_options.MediaHeader));

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                throw new OrcidServiceException(RetrievalErrorMessage, new());

            OrcidPerson? person = await JsonSerializer.DeserializeAsync<OrcidPerson>(
                await response.Content.ReadAsStreamAsync(),
                _jsonSerializerOptions);

            if (person == null)
                throw new OrcidServiceException(DeserializationErrorMessage, new());

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

    public async Task<List<OrcidPerson>> FindPeopleByName(string personName, int preferredAmountOfResults)
    {
        string queryUrl = "search?q={nameQuery}";
        var resultList = await SearchResultRequestAndParse<PersonSearchResult>(queryUrl, "result");
        List<OrcidPerson> returnList = [];
        for (int i = 0;
            i < Math.Min(resultList.Count, Math.Min(preferredAmountOfResults, _options.MaxResults));
            i++)
            returnList.Add(await FindPersonByOrcid(resultList[i].Id.Path!));

        return returnList;
    }


    //WARNING: This method is only compatible with the ORCID API v3.0 but this restriction is not enforced
    //use the FindPeopleByName method for a more generic solution. You can expect an exception if you call this method
    //with the wrong configuration.
    public async Task<List<OrcidPerson>> FindPeopleByNameFast(string personName)
    {
        string queryUrl = $"expanded-search?q={personName}";
        var resultList = await SearchResultRequestAndParse<PersonExpandedSearchResult>(queryUrl, "expanded-result");
        return resultList.Select(people => people.ToPerson()).ToList();
    }


    public async Task<List<T>> SearchResultRequestAndParse<T>(string queryUrl, string jsonListElement) where T : class
    {
        try
        {
            HttpRequestMessage request = new(HttpMethod.Get, queryUrl);
            request.Headers.Authorization = new("Bearer", _options.AuthorizationCode);
            request.Headers.Accept.Add(new(_options.MediaHeader));

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
