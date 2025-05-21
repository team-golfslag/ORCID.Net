// This program has been developed by students from the bachelor Computer Science at Utrecht
// University within the Software Project course.
// 
// © Copyright Utrecht University (Department of Information and Computing Sciences)

using System.Net;
using System.Text;
using System.Text.Json;
using Moq;
using Moq.Protected;
using ORCID.Net.Models;
using ORCID.Net.ORCIDServiceExceptions;
using ORCID.Net.Services;
using Xunit;

namespace ORCID.Net.Tests;

public class PersonRetrievalServiceTests
{
    private readonly Mock<HttpMessageHandler> _messageHandlerMock;
    private readonly PersonRetrievalServiceOptions _options;
    private readonly HttpResponseMessage _response;
    private const string FakeBaseUrl = "https://sandbox.orcid.org";
    private const string FakeApiUrl = "https://pub.sandbox.orcid.org/v3.0/";
    private const string FakeClientId = "fake-client-id";
    private const string FakeClientSecret = "fake-client-secret";
    private const string FakeAccessToken = "fake-token-123";
    private const string FakeApiVersion = "v3.0";
    private const int TestMaxResults = 20;

    public PersonRetrievalServiceTests()
    {
        _messageHandlerMock = new Mock<HttpMessageHandler>();
        _response = new HttpResponseMessage();
        
        // Create and configure the test options
        _options = new PersonRetrievalServiceOptions(
            FakeBaseUrl,
            FakeClientId,
            FakeClientSecret,
            FakeApiVersion,
            TestMaxResults);
    }

    private void SetupAuthTokenResponse()
    {
        var authJson = $@"{{""access_token"":""{FakeAccessToken}"",""token_type"":""bearer"",""expires_in"":631138518}}";
        var authContent = new StringContent(authJson);
        var authResponse = new HttpResponseMessage(HttpStatusCode.OK) { Content = authContent };
        
        _messageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", 
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Method == HttpMethod.Post && req.RequestUri.ToString().EndsWith("oauth/token")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(authResponse);
    }

    private void SetupHttpResponse(HttpStatusCode statusCode, string content)
    {
        _response.StatusCode = statusCode;
        _response.Content = new StringContent(content);
        _messageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", 
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(_response);
    }

    // Helper method to create a test service instance using our mocked HttpClient
    private PersonRetrievalService CreateTestService()
    {
        var httpClient = new HttpClient(_messageHandlerMock.Object);
        var authResponse = new AuthResponse 
        { 
            Token = FakeAccessToken,
            TokenType = "bearer",
            ExpiresIn = 631138518
        };
        
        // Use the internal constructor (requires InternalsVisibleTo for the test project)
        return new PersonRetrievalService(_options, httpClient, authResponse);
    }

    [Fact]
    public async Task PersonRetrievalTestWithSomeAttributesNull()
    {
        // Arrange
        string json = @"
        {
          ""last-modified-date"": null,
          ""name"": {
            ""created-date"": {
              ""value"": 1487783344822
            },
            ""last-modified-date"": {
              ""value"": 1487783345135
            },
            ""given-names"": {
              ""value"": ""mark""
            },
            ""family-name"": null,
            ""credit-name"": null,
            ""source"": null,
            ""visibility"": ""PUBLIC"",
            ""path"": ""0000-0001-8564-3504""
          },
          ""biography"": null,
          ""path"": ""/0000-0001-8564-3504/person""
        }
        ";
        SetupHttpResponse(HttpStatusCode.OK, json);

        // Act
        PersonRetrievalService service = CreateTestService();
        OrcidPerson orcidPerson = await service.FindPersonByOrcid("0000-0001-8564-3504");

        // Assert
        Assert.NotNull(orcidPerson);
        Assert.Equal("mark", orcidPerson.FirstName);
        Assert.Null(orcidPerson.LastName);
        Assert.Null(orcidPerson.CreditName);
        Assert.Null(orcidPerson.Biography);
        Assert.Equal("0000-0001-8564-3504", orcidPerson.Orcid);
    }

    [Fact]
    public async Task PersonRetrievalTestWithNoAttributesNull()
    {
        // Arrange
        string json = @"
        {
          ""last-modified-date"": null,
          ""name"": {
            ""created-date"": {
              ""value"": 1487783344822
            },
            ""last-modified-date"": {
              ""value"": 1487783345135
            },
            ""given-names"": {
              ""value"": ""mark""
            },
            ""family-name"": {
              ""value"": ""Jensen""
            },
            ""credit-name"": {
              ""value"": ""MJ""
            },
            ""source"": null,
            ""visibility"": ""PUBLIC"",
            ""path"": ""0000-0001-8564-3504""
          },
          ""biography"": {
              ""value"": ""Upstanding citizen by day but at night he transforms into the rizzler""
            },
          ""path"": ""/0000-0001-8564-3504/person""
        }
        ";
        SetupHttpResponse(HttpStatusCode.OK, json);

        // Act
        PersonRetrievalService service = CreateTestService();
        OrcidPerson orcidPerson = await service.FindPersonByOrcid("0000-0001-8564-3504");

        // Assert
        Assert.NotNull(orcidPerson);
        Assert.Equal("mark", orcidPerson.FirstName);
        Assert.Equal("Jensen", orcidPerson.LastName);
        Assert.Equal("MJ", orcidPerson.CreditName);
        Assert.Equal("Upstanding citizen by day but at night he transforms into the rizzler", orcidPerson.Biography);
        Assert.Equal("0000-0001-8564-3504", orcidPerson.Orcid);
    }

    [Fact]
    public async Task PersonSearchByName()
    {
        // Arrange
        // First set up the search result
        string searchJson = @"
        {
          ""result"" : [
            {
              ""orcid-identifier"" : {
                ""uri"" : ""https://sandbox.orcid.org/0000-0002-7614-2895"",
                ""path"" : ""0000-0002-7614-2895"",
                ""host"" : ""sandbox.orcid.org""
              }
            }
          ],
          ""num-found"": 1
        }
        ";
        
        // Setup the person detail that will be returned when fetching by ID
        string personJson = @"
        {
          ""last-modified-date"": null,
          ""name"": {
            ""created-date"": {
              ""value"": 1487783344822
            },
            ""last-modified-date"": {
              ""value"": 1487783345135
            },
            ""given-names"": {
              ""value"": ""mark""
            },
            ""family-name"": {
              ""value"": ""Jensen""
            },
            ""credit-name"": {
              ""value"": ""MJ""
            },
            ""source"": null,
            ""visibility"": ""PUBLIC"",
            ""path"": ""0000-0002-7614-2895""
          },
          ""biography"": {
              ""value"": ""Upstanding citizen by day but at night he transforms into the rizzler""
            },
          ""path"": ""/0000-0002-7614-2895/person""
        }
        ";

        // Setup the search response
        _messageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri.ToString().Contains("search")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(searchJson)
            });

        // Setup the person detail response
        _messageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri.ToString().Contains("0000-0002-7614-2895/person")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(personJson)
            });

        // Act
        PersonRetrievalService service = CreateTestService();
        var people = await service.FindPeopleByName("mark", 100);

        // Assert
        Assert.Single(people);
        OrcidPerson person = people[0];
        Assert.NotNull(person);
        Assert.Equal("mark", person.FirstName);
        Assert.Equal("Jensen", person.LastName);
        Assert.Equal("MJ", person.CreditName);
        Assert.Equal("Upstanding citizen by day but at night he transforms into the rizzler", person.Biography);
        Assert.Equal("0000-0002-7614-2895", person.Orcid);
    }

    [Fact]
    public async Task PersonSearchByNameJsonExceptionSecondResponse()
    {
        // Arrange
        // First set up the search result
        string searchJson = @"
        {
          ""result"" : [
            {
              ""orcid-identifier"" : {
                ""uri"" : ""https://sandbox.orcid.org/0000-0002-7614-2895"",
                ""path"" : ""0000-0002-7614-2895"",
                ""host"" : ""sandbox.orcid.org""
              }
            }
          ],
          ""num-found"": 1
        }
        ";
        
        // Setup the person detail that will be returned when fetching by ID, with invalid JSON
        string invalidPersonJson = @"{{{{ Invalid JSON here";

        // Setup the search response
        _messageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri.ToString().Contains("search")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(searchJson)
            });

        // Setup the person detail response with invalid JSON
        _messageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri.ToString().Contains("0000-0002-7614-2895/person")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(invalidPersonJson)
            });

        // Act & Assert
        PersonRetrievalService service = CreateTestService();
        await Assert.ThrowsAsync<OrcidServiceException>(() =>
            service.FindPeopleByName("mark", 10));
    }

    [Fact]
    public async Task PersonSearchByNameNoMatches()
    {
        // Arrange
        string json = @"
        {
          ""result"" : [],
          ""num-found"": 0
        }
        ";
        SetupHttpResponse(HttpStatusCode.OK, json);

        // Act
        PersonRetrievalService service = CreateTestService();
        var people = await service.FindPeopleByName("no-match", 100);

        // Assert
        Assert.NotNull(people);
        Assert.Empty(people);
    }

    [Fact]
    public async Task PersonSearchByNameBadResponse()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.BadRequest, "Bad Request");

        // Act & Assert
        PersonRetrievalService service = CreateTestService();
        await Assert.ThrowsAsync<OrcidServiceException>(() =>
            service.FindPeopleByName("mark", 100));
    }

    [Fact]
    public async Task PersonSearchByNameHttpException()
    {
        // Arrange
        _messageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Simulated network error"));

        // Act & Assert
        PersonRetrievalService service = CreateTestService();
        await Assert.ThrowsAsync<OrcidServiceException>(() =>
            service.FindPeopleByName("mark", 100));
    }

    [Fact]
    public async Task PersonSearchByNameJsonExceptionFirstResponse()
    {
        // Arrange
        string invalidJson = @"{ ""result"": [ Invalid JSON here";
        SetupHttpResponse(HttpStatusCode.OK, invalidJson);

        // Act & Assert
        PersonRetrievalService service = CreateTestService();
        await Assert.ThrowsAsync<OrcidServiceException>(() =>
            service.FindPeopleByName("mark", 100));
    }

    [Fact]
    public async Task PersonRetrievalBadResponse()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.BadRequest, "Bad Request");

        // Act & Assert
        PersonRetrievalService service = CreateTestService();
        await Assert.ThrowsAsync<OrcidServiceException>(() =>
            service.FindPersonByOrcid("0000-0001-8564-3504"));
    }

    [Fact]
    public async Task PersonRetrievalHttpException()
    {
        // Arrange
        _messageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Simulated network error"));

        // Act & Assert
        PersonRetrievalService service = CreateTestService();
        await Assert.ThrowsAsync<OrcidServiceException>(() =>
            service.FindPersonByOrcid("0000-0001-8564-3504"));
    }

    [Fact]
    public async Task PersonRetrievalJsonException()
    {
        // Arrange
        string invalidJson = @"{ ""name"": { ""given-names"": { Invalid JSON here";
        SetupHttpResponse(HttpStatusCode.OK, invalidJson);

        // Act & Assert
        PersonRetrievalService service = CreateTestService();
        await Assert.ThrowsAsync<OrcidServiceException>(() =>
            service.FindPersonByOrcid("0000-0001-8564-3504"));
    }

    [Fact]
    public async Task PersonSearchByNameFastJsonException()
    {
        // Arrange
        string invalidJson = @"{ ""expanded-result"": [ Invalid JSON here";
        SetupHttpResponse(HttpStatusCode.OK, invalidJson);

        // Act & Assert
        PersonRetrievalService service = CreateTestService();
        await Assert.ThrowsAsync<OrcidServiceException>(() =>
            service.FindPeopleByNameFast("mark"));
    }

    [Fact]
    public async Task PersonRetrievalFastBadResponse()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.BadRequest, "Bad Request");

        // Act & Assert
        PersonRetrievalService service = CreateTestService();
        await Assert.ThrowsAsync<OrcidServiceException>(() =>
            service.FindPeopleByNameFast("mark"));
    }

    [Fact]
    public async Task PersonRetrievalFastHttpException()
    {
        // Arrange
        _messageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Simulated network error"));

        // Act & Assert
        PersonRetrievalService service = CreateTestService();
        await Assert.ThrowsAsync<OrcidServiceException>(() =>
            service.FindPeopleByNameFast("mark"));
    }

    [Fact]
    public async Task PersonRetrievalFastGoodCase()
    {
        // Arrange
        string json = @"
        {
          ""expanded-result"" : [ {
            ""orcid-id"" : ""0000-0002-0380-1984"",
            ""given-names"" : ""brenda"",
            ""family-names"" : ""marshall"",
            ""credit-name"" : null,
            ""other-name"" : [ ],
            ""email"" : [ ],
            ""institution-name"" : [ ]
          }, {
            ""orcid-id"" : ""0000-0003-1890-8271"",
            ""given-names"" : ""Brenda Heaton, MPH"",
            ""family-names"" : ""Heaton"",
            ""credit-name"" : ""Brenda Heaton, MPH"",
            ""other-name"" : [ ],
            ""email"" : [ ],
            ""institution-name"" : [ ]
          }],
          ""num-found"" : 2
        }
        ";
        SetupHttpResponse(HttpStatusCode.OK, json);

        // Act
        PersonRetrievalService service = CreateTestService();
        var people = await service.FindPeopleByNameFast("brenda");

        // Assert
        Assert.Equal(2, people.Count);
        Assert.Equal("brenda", people[0].FirstName);
        Assert.Equal("marshall", people[0].LastName);
        Assert.Null(people[0].CreditName);
        Assert.Equal("0000-0002-0380-1984", people[0].Orcid);
        Assert.Equal("Brenda Heaton, MPH", people[1].FirstName);
        Assert.Equal("Heaton", people[1].LastName);
        Assert.Equal("Brenda Heaton, MPH", people[1].CreditName);
        Assert.Equal("0000-0003-1890-8271", people[1].Orcid);
    }

    [Fact]
    public async Task SearchRequestAndParseGoodCase()
    {
        // Arrange
        string json = @"
        {
          ""expanded-result"" : [ {
            ""orcid-id"" : ""0000-0002-0380-1984"",
            ""given-names"" : ""brenda"",
            ""family-names"" : ""marshall"",
            ""credit-name"" : null,
            ""other-name"" : [ ],
            ""email"" : [ ],
            ""institution-name"" : [ ]
          }, {
            ""orcid-id"" : ""0000-0003-1890-8271"",
            ""given-names"" : ""Brenda Heaton, MPH"",
            ""family-names"" : ""Heaton"",
            ""credit-name"" : ""Brenda Heaton, MPH"",
            ""other-name"" : [ ],
            ""email"" : [ ],
            ""institution-name"" : [ ]
          }],
          ""num-found"" : 2
        }
        ";
        SetupHttpResponse(HttpStatusCode.OK, json);

        // Act
        PersonRetrievalService service = CreateTestService();
        var people = await service.SearchResultRequestAndParse<PersonExpandedSearchResult>("expanded-search?q=brenda", "expanded-result");

        // Assert
        Assert.Equal(2, people.Count);
        Assert.Equal("brenda", people[0].FirstName);
        Assert.Equal("marshall", people[0].LastName);
        Assert.Null(people[0].CreditName);
    }

    [Fact]
    public async Task SearchRequestAndParseJsonException()
    {
        // Arrange
        string invalidJson = "Invalid JSON data";
        SetupHttpResponse(HttpStatusCode.OK, invalidJson);

        // Act & Assert
        PersonRetrievalService service = CreateTestService();
        await Assert.ThrowsAsync<OrcidServiceException>(() =>
            service.SearchResultRequestAndParse<PersonExpandedSearchResult>("expanded-search?q=brenda", "expanded-result"));
    }

    [Fact]
    public async Task SearchRequestAndParseHttpException()
    {
        // Arrange
        _messageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Simulated network error"));

        // Act & Assert
        PersonRetrievalService service = CreateTestService();
        await Assert.ThrowsAsync<OrcidServiceException>(() =>
            service.SearchResultRequestAndParse<PersonExpandedSearchResult>("expanded-search?q=brenda", "expanded-result"));
    }

    [Fact]
    public async Task MaxResultsIsRespected()
    {
        // Arrange
        // Create multiple search results
        var searchResults = new List<string>();
        for (int i = 0; i < 30; i++)
        {
            searchResults.Add($@"{{
                ""orcid-identifier"" : {{
                    ""uri"" : ""https://sandbox.orcid.org/0000-0002-7614-{i:D4}"",
                    ""path"" : ""0000-0002-7614-{i:D4}"",
                    ""host"" : ""sandbox.orcid.org""
                }}
            }}");
        }
        
        string searchJson = $@"
        {{
          ""result"" : [{string.Join(",", searchResults)}],
          ""num-found"": 30
        }}
        ";
        
        // Create a template for each index value
        Func<int, string> createPersonJson = index => $@"
        {{
          ""last-modified-date"": null,
          ""name"": {{
            ""created-date"": {{
              ""value"": 1487783344822
            }},
            ""last-modified-date"": {{
              ""value"": 1487783345135
            }},
            ""given-names"": {{
              ""value"": ""Person_{index}""
            }},
            ""family-name"": {{
              ""value"": ""Last_{index}""
            }},
            ""credit-name"": null,
            ""source"": null,
            ""visibility"": ""PUBLIC"",
            ""path"": ""0000-0002-7614-{index:D4}""
          }},
          ""biography"": null,
          ""path"": ""/0000-0002-7614-{index:D4}/person""
        }}
        ";

        // Setup the search response
        _messageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri.ToString().Contains("search")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(searchJson)
            });

        // Setup individual person responses
        for (int i = 0; i < 30; i++)
        {
            string orcid = $"0000-0002-7614-{i:D4}";
            string personJson = createPersonJson(i);
            
            _messageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri.ToString().Contains($"{orcid}/person")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(personJson)
                });
        }

        // Act
        // Create a service with customized options to limit to TestMaxResults (20)
        PersonRetrievalService service = CreateTestService();
        var people = await service.FindPeopleByName("test-name", 100); // Request more than MaxResults

        // Assert
        Assert.Equal(TestMaxResults, people.Count); // Should be limited by TestMaxResults (20), not 100
        
        // Check that the first few results are as expected
        Assert.Equal("Person_0", people[0].FirstName);
        Assert.Equal("Last_0", people[0].LastName);
        Assert.Equal("0000-0002-7614-0000", people[0].Orcid);
        
        Assert.Equal("Person_19", people[19].FirstName);
        Assert.Equal("Last_19", people[19].LastName);
        Assert.Equal("0000-0002-7614-0019", people[19].Orcid);
    }

    [Fact]
    public void ApiVersionIsUsedCorrectly()
    {
        // Arrange
        const string testBaseUrl = "https://sandbox.orcid.org";
        const string customApiVersion = "v2.1";

        // Act
        var options = new PersonRetrievalServiceOptions(
            testBaseUrl,
            FakeClientId,
            FakeClientSecret,
            customApiVersion);

        // Assert
        Assert.Equal("https://pub.sandbox.orcid.org/v2.1/", options.ApiUrl.ToString());
        Assert.Equal(testBaseUrl, options.BaseUrl.ToString().TrimEnd('/'));
    }

    [Fact]
    public void DefaultApiVersionIsUsedWhenNotProvided()
    {
        // Arrange
        const string testBaseUrl = "https://sandbox.orcid.org";

        // Act
        var options = new PersonRetrievalServiceOptions(
            testBaseUrl,
            FakeClientId,
            FakeClientSecret); // No API version provided

        // Assert
        Assert.Equal($"https://pub.sandbox.orcid.org/{PersonRetrievalServiceOptions.DefaultApiVersion}/", 
            options.ApiUrl.ToString());
        Assert.Equal(PersonRetrievalServiceOptions.MaxRecommendedResults, options.MaxResults);
    }
}