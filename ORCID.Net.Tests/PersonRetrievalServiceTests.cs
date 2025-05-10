using System.Text;
using Moq;
using Moq.Protected;
using ORCID.Net.Services;
using ORCID.Net.Models;
using ORCID.Net.ORCIDServiceExceptions;
using Xunit;

namespace ORCID.Net.Tests;

public class PersonRetrievalServiceTests
{
    private readonly Mock<HttpMessageHandler> _messageHandlerMock;
    private readonly HttpResponseMessage _response;
    private readonly Mock<PersonRetrievalServiceOptions> _options;

    public PersonRetrievalServiceTests()
    {
        _options = new();
        _messageHandlerMock = new();
        HttpClient client = new(_messageHandlerMock.Object);
        client.BaseAddress = new("https://pub.sandbox.orcid.org/v3.0/");
        _options.Setup(options => options.BuildHttpClient()).Returns(client);
        _response = new();
        _messageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()).Returns(Task.FromResult(_response));
    }

    [Fact]
    public async Task PersonRetrievalTestWithSomeAttributesNull()
    {
        _response.StatusCode = System.Net.HttpStatusCode.OK;
        const string json = """
                            
                                    {
                                      "last-modified-date": null,
                                      "name": {
                                        "created-date": {
                                          "value": 1487783344822
                                        },
                                        "last-modified-date": {
                                          "value": 1487783345135
                                        },
                                        "given-names": {
                                          "value": "mark"
                                        },
                                        "family-name": null,
                                        "credit-name": null,
                                        "source": null,
                                        "visibility": "PUBLIC",
                                        "path": "0000-0001-8564-3504"
                                      },
                                      "other-names": {
                                        "last-modified-date": null,
                                        "other-name": [],
                                        "path": "/0000-0001-8564-3504/other-names"
                                      },
                                      "biography": null,
                                      "researcher-urls": {
                                        "last-modified-date": null,
                                        "researcher-url": [],
                                        "path": "/0000-0001-8564-3504/researcher-urls"
                                      },
                                      "emails": {
                                        "last-modified-date": null,
                                        "email": [],
                                        "path": "/0000-0001-8564-3504/email"
                                      },
                                      "addresses": {
                                        "last-modified-date": null,
                                        "address": [],
                                        "path": "/0000-0001-8564-3504/address"
                                      },
                                      "keywords": {
                                        "last-modified-date": null,
                                        "keyword": [],
                                        "path": "/0000-0001-8564-3504/keywords"
                                      },
                                      "external-identifiers": {
                                        "last-modified-date": null,
                                        "external-identifier": [],
                                        "path": "/0000-0001-8564-3504/external-identifiers"
                                      },
                                      "path": "/0000-0001-8564-3504/person"
                                    }
                                    
                            """;
        MemoryStream personStream = new(Encoding.UTF8.GetBytes(json));
        _response.Content = new StreamContent(personStream);
        PersonRetrievalService service = new(_options.Object);
        Person person = await service.FindPersonByOrcid("Doesn't matter will return set response anyway");

        Assert.NotNull(person);
        Assert.Equal("mark", person.FirstName);
        Assert.Null(person.LastName);
        Assert.Null(person.Biography);
        Assert.Null(person.CreditName);
    }

    [Fact]
    public async Task PersonRetrievalTestWithNoAttributesNull()
    {
        _response.StatusCode = System.Net.HttpStatusCode.OK;
        const string json = """
                            
                                    {
                                      "last-modified-date": null,
                                      "name": {
                                        "created-date": {
                                          "value": 1487783344822
                                        },
                                        "last-modified-date": {
                                          "value": 1487783345135
                                        },
                                        "given-names": {
                                          "value": "mark"
                                        },
                                        "family-name": {
                                          "value": "Jensen"
                                        },
                                        "credit-name": {
                                          "value": "MJ"
                                        },
                                        "source": null,
                                        "visibility": "PUBLIC",
                                        "path": "0000-0001-8564-3504"
                                      },
                                      "other-names": {
                                        "last-modified-date": null,
                                        "other-name": [],
                                        "path": "/0000-0001-8564-3504/other-names"
                                      },
                                      "biography": {
                                          "value": "Upstanding citizen by day but at night he transforms into the rizzler"
                                        },
                                      "researcher-urls": {
                                        "last-modified-date": null,
                                        "researcher-url": [],
                                        "path": "/0000-0001-8564-3504/researcher-urls"
                                      },
                                      "emails": {
                                        "last-modified-date": null,
                                        "email": [],
                                        "path": "/0000-0001-8564-3504/email"
                                      },
                                      "addresses": {
                                        "last-modified-date": null,
                                        "address": [],
                                        "path": "/0000-0001-8564-3504/address"
                                      },
                                      "keywords": {
                                        "last-modified-date": null,
                                        "keyword": [],
                                        "path": "/0000-0001-8564-3504/keywords"
                                      },
                                      "external-identifiers": {
                                        "last-modified-date": null,
                                        "external-identifier": [],
                                        "path": "/0000-0001-8564-3504/external-identifiers"
                                      },
                                      "path": "/0000-0001-8564-3504/person"
                                    }
                                    
                            """;
        MemoryStream personStream = new(Encoding.UTF8.GetBytes(json));
        _response.Content = new StreamContent(personStream);
        PersonRetrievalService service = new(_options.Object);
        Person person = await service.FindPersonByOrcid("Doesn't matter will return set response anyway");

        Assert.NotNull(person);
        Assert.Equal("mark", person.FirstName);
        Assert.Equal("Jensen", person.LastName);
        Assert.NotNull(person.Biography);
        Assert.Equal("MJ", person.CreditName);
    }

    [Fact]
    public async Task PersonSearchByName()
    {
        _response.StatusCode = System.Net.HttpStatusCode.OK;
        const string json = """
                            
                                    {
                                      "result" : [
                                        {
                                          "orcid-identifier" : {
                                            "uri" : "https://sandbox.orcid.org/0000-0002-7614-2895",
                                            "path" : "0000-0002-7614-2895",
                                            "host" : "sandbox.orcid.org"
                                          }
                                        }
                                      ],
                                      "num-found": 1
                                    }
                                    
                            """;

        MemoryStream personStream = new(Encoding.UTF8.GetBytes(json));
        _response.Content = new StreamContent(personStream);
        PersonRetrievalService service = new(_options.Object);
        HttpResponseMessage personResponse = new();
        personResponse.StatusCode = System.Net.HttpStatusCode.OK;
        const string personJson = """
                                  
                                          {
                                            "last-modified-date": null,
                                            "name": {
                                              "created-date": {
                                                "value": 1487783344822
                                              },
                                              "last-modified-date": {
                                                "value": 1487783345135
                                              },
                                              "given-names": {
                                                "value": "mark"
                                              },
                                              "family-name": {
                                                "value": "Jensen"
                                              },
                                              "credit-name": {
                                                "value": "MJ"
                                              },
                                              "source": null,
                                              "visibility": "PUBLIC",
                                              "path": "0000-0001-8564-3504"
                                            },
                                            "other-names": {
                                              "last-modified-date": null,
                                              "other-name": [],
                                              "path": "/0000-0001-8564-3504/other-names"
                                            },
                                            "biography": {
                                                "value": "Upstanding citizen by day but at night he transforms into the rizzler"
                                              },
                                            "researcher-urls": {
                                              "last-modified-date": null,
                                              "researcher-url": [],
                                              "path": "/0000-0001-8564-3504/researcher-urls"
                                            },
                                            "emails": {
                                              "last-modified-date": null,
                                              "email": [],
                                              "path": "/0000-0001-8564-3504/email"
                                            },
                                            "addresses": {
                                              "last-modified-date": null,
                                              "address": [],
                                              "path": "/0000-0001-8564-3504/address"
                                            },
                                            "keywords": {
                                              "last-modified-date": null,
                                              "keyword": [],
                                              "path": "/0000-0001-8564-3504/keywords"
                                            },
                                            "external-identifiers": {
                                              "last-modified-date": null,
                                              "external-identifier": [],
                                              "path": "/0000-0001-8564-3504/external-identifiers"
                                            },
                                            "path": "/0000-0001-8564-3504/person"
                                          }
                                          
                                  """;
        personResponse.Content = new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes(personJson)));
        _messageHandlerMock.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(
                req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri.ToString() == $"https://pub.sandbox.orcid.org/v3.0/0000-0002-7614-2895/person"),
            ItExpr.IsAny<CancellationToken>()).Returns(Task.FromResult(personResponse));

        var people = await service.FindPeopleByName("Doesn't matter will return set response anyway", 100);
        Assert.Single(people);

        Person person = people[0];
        Assert.NotNull(person);
        Assert.Equal("mark", person.FirstName);
        Assert.Equal("Jensen", person.LastName);
        Assert.NotNull(person.Biography);
        Assert.Equal("MJ", person.CreditName);
    }

    [Fact]
    public async Task PersonSearchByNameNoMatches()
    {
        _response.StatusCode = System.Net.HttpStatusCode.OK;
        const string json = """
                            
                                    {
                                      "result" : [],
                                    "num-found": 0
                                    }
                                    
                            """;
        MemoryStream peopleStream = new(Encoding.UTF8.GetBytes(json));
        _response.Content = new StreamContent(peopleStream);
        PersonRetrievalService service = new(_options.Object);
        var people = await service.FindPeopleByName("Doesn't matter will return set response anyway", 100);
        Assert.NotNull(people);
        Assert.Empty(people);
    }

    [Fact]
    public async Task PersonSearchByNameBadResponse()
    {
        _response.StatusCode = System.Net.HttpStatusCode.BadRequest;
        PersonRetrievalService service = new(_options.Object);
        await Assert.ThrowsAsync<ORCIDServiceException>(() =>
            service.FindPeopleByName("Doesn't matter will return set response anyway", 100));
    }

    [Fact]
    public async Task PersonSearchByNameHttpException()
    {
        _messageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()).Throws(new HttpRequestException());
        PersonRetrievalService service = new(_options.Object);
        await Assert.ThrowsAsync<ORCIDServiceException>(() =>
            service.FindPeopleByName("Doesn't matter will return set response anyway", 100));
    }

    [Fact]
    public async Task PersonSearchByNameJsonException()
    {
        _response.StatusCode = System.Net.HttpStatusCode.OK;
        const string json = """
                            
                                    {
                                      "result" : [],
                                    "num-found": 0
                                    }}
                                    
                            """;
        MemoryStream peopleStream = new(Encoding.UTF8.GetBytes(json));
        _response.Content = new StreamContent(peopleStream);
        PersonRetrievalService service = new(_options.Object);
        await Assert.ThrowsAsync<ORCIDServiceException>(() =>
            service.FindPeopleByName("Doesn't matter will return set response anyway", 100));
    }

    [Fact]
    public async Task PersonRetrievalBadResponse()
    {
        _response.StatusCode = System.Net.HttpStatusCode.BadRequest;
        PersonRetrievalService service = new(_options.Object);
        await Assert.ThrowsAsync<ORCIDServiceException>(() =>
            service.FindPersonByOrcid("Doesn't matter will return set response anyway"));
    }

    [Fact]
    public async Task PersonRetrievalHttpException()
    {
        _messageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()).Throws(new HttpRequestException());

        PersonRetrievalService service = new(_options.Object);
        await Assert.ThrowsAsync<ORCIDServiceException>(() =>
            service.FindPersonByOrcid("Doesn't matter will return set response anyway"));
    }

    [Fact]
    public async Task PersonRetrievalJsonException()
    {
        _response.StatusCode = System.Net.HttpStatusCode.OK;
        const string json = """
                            
                                    {
                                      "last-modified-date": null,
                                      "name": {
                                        "created-date": {
                                          "value": 1487783344822
                                        },
                                        "last-modified-date": {
                                          "value": 1487783345135
                                        },
                                        "given-names": {
                                          "value": "mark"
                                        },
                                        "family-name": null,
                                        "credit-name": null,
                                        "source": null,
                                        "visibility": "PUBLIC",
                                        "path": "0000-0001-8564-3504"
                                      },
                                      "other-names": {
                                        "last-modified-date": null,
                                        "other-name": [],
                                        "path": "/0000-0001-8564-3504/other-names"
                                      },
                                      "biography": null,
                                      "researcher-urls": {
                                        "last-modified-date": null,
                                        "researcher-url": [],
                                        "path": "/0000-0001-8564-3504/researcher-urls"
                                      },
                                      "emails": {
                                        "last-modified-date": null,
                                        "email": [],
                                        "path": "/0000-0001-8564-3504/email"
                                      },
                                      "addresses": {
                                        "last-modified-date": null,
                                        "address": [],
                                        "path": "/0000-0001-8564-3504/address"
                                      },
                                      "keywords": {
                                        "last-modified-date": null,
                                        "keyword": [],
                                        "path": "/0000-0001-8564-3504/keywords"
                                      },
                                      "external-identifiers": {
                                        "last-modified-date": null,
                                        "external-identifier": [],
                                        "path": "/0000-0001-8564-3504/external-identifiers"
                                      },
                                      "path": "/0000-0001-8564-3504/person"
                                    }
                                    
                            """;
        MemoryStream personStream = new(Encoding.UTF8.GetBytes(json));
        _response.Content = new StreamContent(personStream);

        PersonRetrievalService service = new(_options.Object);
        await Assert.ThrowsAsync<ORCIDServiceException>(() =>
            service.FindPersonByOrcid("Doesn't matter will return set response anyway"));
    }
}
