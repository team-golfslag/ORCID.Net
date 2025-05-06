using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using ORCID.org.Models;
using ORCID.org.ORCIDServiceExceptions;

namespace ORCID.org.Services;

using System.Net.Http.Json;
using JsonConverters;

public class PersonRetrievalService
{
    public HttpClient _httpClient;
    public PersonRetrievalServiceOptions _options;

    public PersonRetrievalService(PersonRetrievalServiceOptions options)
    {
        _httpClient = new();
        _options = options;
        _httpClient.BaseAddress = new Uri(_options.BaseUrl);
    }

    public async Task<Person> FindPersonByOrcid(string orcID)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{orcID}/person");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.AuthorizationCode);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(_options.MediaHeader));
            
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var person = await JsonSerializer.DeserializeAsync<Person>(
                    await response.Content.ReadAsStreamAsync(),
                    new JsonSerializerOptions
                    {
                        Converters = { new PersonJsonConverter() }
                    });
                return person;
            }
            else
            {
                throw new ORCIDServiceException("Failed to retrieve person", new Exception());
            }
        }
        catch (HttpRequestException e)
        {
            throw new ORCIDServiceException("Failed to retrieve person", e);
        }
        catch (JsonException e)
        {
            throw new ORCIDServiceException("Failed to deserialize person", e);
        }
    }
    
    public async Task<List<Person>> FindPeoplyByName(string nameQuery, int preferredAmountOfResults)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"search?q={nameQuery}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.AuthorizationCode);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(_options.MediaHeader));
            
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var text = await response.Content.ReadAsStringAsync();
                JsonDocument doc = JsonDocument.Parse(text);
                if(doc.RootElement.GetProperty("result").ValueKind == JsonValueKind.Null) return new List<Person>();
                var resultListJson = doc.RootElement.GetProperty("result").EnumerateArray().ToArray();
                List<PersonSearchResult> resultList = resultListJson.Select(element => JsonSerializer.Deserialize<PersonSearchResult>(element.GetRawText())).ToList();
                List<Person> returnList = new List<Person>();
                for (int i = 0; i < Math.Min(resultList.Count, Math.Min(preferredAmountOfResults, _options.MaxResults)); i++)
                {
                    returnList.Add(await FindPersonByOrcid(resultList[i].Id.Path));
                }
                return returnList;
            }

            throw new ORCIDServiceException("Failed to retrieve person", new Exception());
        }
        catch (HttpRequestException e)
        {
            throw new ORCIDServiceException("Failed to retrieve person", e);
        }
        catch (JsonException e)
        {
            throw new ORCIDServiceException("Failed to deserialize person", e);
        }
    }
    
}