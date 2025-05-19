using System.Text.Json;
using ORCID.Net.Models;
using ORCID.Net.JsonConverters;
using ORCID.Net.ORCIDServiceExceptions;

namespace ORCID.Net.Services;

public class PersonRetrievalService :IPersonRetrievalService
{
    private readonly HttpClient _httpClient;
    private readonly PersonRetrievalServiceOptions _options;

    public PersonRetrievalService(PersonRetrievalServiceOptions options)
    {
        _options = options;
        _httpClient = _options.BuildHttpClient();
    }

    private static JsonSerializerOptions _jsonSerializerOptions = new()
    {
        Converters =
        {
            new PersonJsonConverter()
        }
    };

    public async Task<Person> FindPersonByOrcid(string orcId)
    {
        try
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"{orcId}/person");

            request.Headers.Authorization = new("Bearer", _options.AuthorizationCode);
            request.Headers.Accept.Add(new(_options.MediaHeader));

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                throw new OrcidServiceException("Failed to retrieve person", new());

            Person? person = await JsonSerializer.DeserializeAsync<Person>(
                await response.Content.ReadAsStreamAsync(),
                _jsonSerializerOptions);
            
            if (person == null)
                throw new OrcidServiceException("Failed to deserialize person", new());

            return person;
        }
        catch (HttpRequestException e)
        {
            throw new OrcidServiceException("Failed to retrieve person", e);
        }
        catch (JsonException e)
        {
            throw new OrcidServiceException("Failed to deserialize person", e);
        }
    }

    public async Task<List<Person>> FindPeopleByName(string nameQuery, int preferredAmountOfResults)
    {
        try
        {
            HttpRequestMessage request = new(HttpMethod.Get, $"search?q={nameQuery}");
            request.Headers.Authorization = new("Bearer", _options.AuthorizationCode);
            request.Headers.Accept.Add(new(_options.MediaHeader));

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                throw new OrcidServiceException("Failed to retrieve person", new());
            
            string text = await response.Content.ReadAsStringAsync();
            JsonDocument doc = JsonDocument.Parse(text);
            
            if (doc.RootElement.GetProperty("result").ValueKind == JsonValueKind.Null) 
                return [];
            var resultListJson = doc.RootElement.GetProperty("result").EnumerateArray().ToArray();
            
            List<PersonSearchResult> resultList = resultListJson
                .Select(element => JsonSerializer.Deserialize<PersonSearchResult>(element.GetRawText()))
                .Where(result => result != null)
                .ToList()!;
            List<Person> returnList = [];
            for (int i = 0;
                i < Math.Min(resultList.Count, Math.Min(preferredAmountOfResults, _options.MaxResults));
                i++)
                returnList.Add(await FindPersonByOrcid(resultList[i].Id.Path!));

            return returnList;

        }
        catch (HttpRequestException e)
        {
            throw new OrcidServiceException("Failed to retrieve person", e);
        }
        catch (JsonException e)
        {
            throw new OrcidServiceException("Failed to deserialize person", e);
        }
    }
    
    
    //WARNING: This method is only compatible with the ORCID API v3.0 but this restriction is not enforced
    //use the FindPeopleByName method for a more generic solution. You can expect an exception if you call this method
    //with the wrong configuration.
    public async Task<List<Person>> FindPeopleByNameFast(string nameQuery)
    {
        try
        {
            HttpRequestMessage request = new(HttpMethod.Get, $"expanded-search?q={nameQuery}");
            request.Headers.Authorization = new("Bearer", _options.AuthorizationCode);
            request.Headers.Accept.Add(new(_options.MediaHeader));

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                throw new OrcidServiceException("Failed to retrieve person", new());
            
            string text = await response.Content.ReadAsStringAsync();
            JsonDocument doc = JsonDocument.Parse(text);
            
            if (doc.RootElement.GetProperty("expanded-result").ValueKind == JsonValueKind.Null) 
                return [];
            var resultListJson = doc.RootElement.GetProperty("expanded-result").EnumerateArray().ToArray();
            
            List<PersonExpandedSearchResult> resultList = resultListJson
                .Select(element => JsonSerializer.Deserialize<PersonExpandedSearchResult>(element.GetRawText()))
                .Where(result => result != null)
                .ToList()!;
        
            return resultList.Select(people => people.ToPerson()).ToList();

        }
        catch (HttpRequestException e)
        {
            throw new OrcidServiceException("Failed to retrieve person", e);
        }
        catch (JsonException e)
        {
            throw new OrcidServiceException("Failed to deserialize person", e);
        }
    }

    public async Task<List<T>> SearchResultRequestAndParse<T>(string url, string query, string jsonListElement)
    {
        try
        {
            HttpRequestMessage request = new(HttpMethod.Get, url + query);
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
            throw new OrcidServiceException("Failed to retrieve person", e);
        }
        catch (JsonException e)
        {
            throw new OrcidServiceException("Failed to deserialize person", e);
        }
    }
}