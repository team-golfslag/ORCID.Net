using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ORCID.Net.Models;
using ORCID.Net.ORCIDServiceExceptions;

namespace ORCID.Net.Services;

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
                var person = await response.Content.ReadFromJsonAsync<Person>(_options.JsonSerializerOptions);
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
    
}