// This program has been developed by students from the bachelor Computer Science at Utrecht
// University within the Software Project course.
// 
// © Copyright Utrecht University (Department of Information and Computing Sciences)

using System.Net;
using System.Text;
using Moq;
using Moq.Protected;
using ORCID.Net.Models;
using ORCID.Net.ORCIDServiceExceptions;
using ORCID.Net.Services;
using Xunit;

namespace ORCID.Net.Tests;

public class PersonRetrievalServiceTests
{
    private HttpClient _client;
    private readonly Mock<HttpMessageHandler> _messageHandlerMock;
    private readonly Mock<PersonRetrievalServiceOptions> _options;
    private readonly HttpResponseMessage _response;

    public PersonRetrievalServiceTests()
    {
        _options = new(PersonRetrievalServiceOptions.OrcidType.Production, "clientId", "clientSecret", 100);
        _messageHandlerMock = new();
        _client = new(_messageHandlerMock.Object);
        _client.BaseAddress = new(PersonRetrievalServiceOptions.OrcidSandboxUrl);
        _options.Setup(options => options.BuildRequestClient()).Returns(_client);
        _response = new();
        _messageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()).Returns(Task.FromResult(_response));
    }

    [Fact]
    public async Task PersonRetrievalTestWithSomeAttributesNull()
    {
        _response.StatusCode = HttpStatusCode.OK;
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
          ""other-names"": {
            ""last-modified-date"": null,
            ""other-name"": [],
            ""path"": ""/0000-0001-8564-3504/other-names""
          },
          ""biography"": null,
          ""researcher-urls"": {
            ""last-modified-date"": null,
            ""researcher-url"": [],
            ""path"": ""/0000-0001-8564-3504/researcher-urls""
          },
          ""emails"": {
            ""last-modified-date"": null,
            ""email"": [],
            ""path"": ""/0000-0001-8564-3504/email""
          },
          ""addresses"": {
            ""last-modified-date"": null,
            ""address"": [],
            ""path"": ""/0000-0001-8564-3504/address""
          },
          ""keywords"": {
            ""last-modified-date"": null,
            ""keyword"": [],
            ""path"": ""/0000-0001-8564-3504/keywords""
          },
          ""external-identifiers"": {
            ""last-modified-date"": null,
            ""external-identifier"": [],
            ""path"": ""/0000-0001-8564-3504/external-identifiers""
          },
          ""path"": ""/0000-0001-8564-3504/person""
        }
        ";
        MemoryStream personStream = new(Encoding.UTF8.GetBytes(json));
        _response.Content = new StreamContent(personStream);
        PersonRetrievalService service = new(_options.Object);
        OrcidPerson orcidPerson = await service.FindPersonByOrcid("Doesn't matter will return set response anyway");

        Assert.NotNull(orcidPerson);
        Assert.Equal("mark", orcidPerson.FirstName);
        Assert.Null(orcidPerson.LastName);
        Assert.Null(orcidPerson.Biography);
        Assert.Null(orcidPerson.CreditName);
    }

    [Fact]
    public async Task PersonRetrievalTestWithNoAttributesNull()
    {
        _response.StatusCode = HttpStatusCode.OK;
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
          ""other-names"": {
            ""last-modified-date"": null,
            ""other-name"": [],
            ""path"": ""/0000-0001-8564-3504/other-names""
          },
          ""biography"": {
              ""value"": ""Upstanding citizen by day but at night he transforms into the rizzler""
            },
          ""researcher-urls"": {
            ""last-modified-date"": null,
            ""researcher-url"": [],
            ""path"": ""/0000-0001-8564-3504/researcher-urls""
          },
          ""emails"": {
            ""last-modified-date"": null,
            ""email"": [],
            ""path"": ""/0000-0001-8564-3504/email""
          },
          ""addresses"": {
            ""last-modified-date"": null,
            ""address"": [],
            ""path"": ""/0000-0001-8564-3504/address""
          },
          ""keywords"": {
            ""last-modified-date"": null,
            ""keyword"": [],
            ""path"": ""/0000-0001-8564-3504/keywords""
          },
          ""external-identifiers"": {
            ""last-modified-date"": null,
            ""external-identifier"": [],
            ""path"": ""/0000-0001-8564-3504/external-identifiers""
          },
          ""path"": ""/0000-0001-8564-3504/person""
        }
        ";
        MemoryStream personStream = new(Encoding.UTF8.GetBytes(json));
        _response.Content = new StreamContent(personStream);
        PersonRetrievalService service = new(_options.Object);
        OrcidPerson orcidPerson = await service.FindPersonByOrcid("Doesn't matter will return set response anyway");

        Assert.NotNull(orcidPerson);
        Assert.Equal("mark", orcidPerson.FirstName);
        Assert.Equal("Jensen", orcidPerson.LastName);
        Assert.NotNull(orcidPerson.Biography);
        Assert.Equal("MJ", orcidPerson.CreditName);
    }

    [Fact]
    public async Task PersonSearchByName()
    {
        _response.StatusCode = HttpStatusCode.OK;
        string json = @"
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

        MemoryStream personStream = new(Encoding.UTF8.GetBytes(json));
        _response.Content = new StreamContent(personStream);
        PersonRetrievalService service = new(_options.Object);
        HttpResponseMessage personResponse = new();
        personResponse.StatusCode = HttpStatusCode.OK;
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
            ""path"": ""0000-0001-8564-3504""
          },
          ""other-names"": {
            ""last-modified-date"": null,
            ""other-name"": [],
            ""path"": ""/0000-0001-8564-3504/other-names""
          },
          ""biography"": {
              ""value"": ""Upstanding citizen by day but at night he transforms into the rizzler""
            },
          ""researcher-urls"": {
            ""last-modified-date"": null,
            ""researcher-url"": [],
            ""path"": ""/0000-0001-8564-3504/researcher-urls""
          },
          ""emails"": {
            ""last-modified-date"": null,
            ""email"": [],
            ""path"": ""/0000-0001-8564-3504/email""
          },
          ""addresses"": {
            ""last-modified-date"": null,
            ""address"": [],
            ""path"": ""/0000-0001-8564-3504/address""
          },
          ""keywords"": {
            ""last-modified-date"": null,
            ""keyword"": [],
            ""path"": ""/0000-0001-8564-3504/keywords""
          },
          ""external-identifiers"": {
            ""last-modified-date"": null,
            ""external-identifier"": [],
            ""path"": ""/0000-0001-8564-3504/external-identifiers""
          },
          ""path"": ""/0000-0001-8564-3504/person""
        }
        ";
        personResponse.Content = new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes(personJson)));
        _messageHandlerMock.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Get &&
                req.RequestUri != null &&
                req.RequestUri.ToString() == "https://pub.sandbox.orcid.org/v3.0/0000-0002-7614-2895/person"),
            ItExpr.IsAny<CancellationToken>()).Returns(Task.FromResult(personResponse));
        var people = await service.FindPeopleByName("Doesn't matter will return set response anyway", 100);
        Assert.Single(people);
        OrcidPerson person = people[0];
        Assert.NotNull(person);
        Assert.Equal("mark", person.FirstName);
        Assert.Equal("Jensen", person.LastName);
        Assert.NotNull(person.Biography);
        Assert.Equal("MJ", person.CreditName);
    }

    [Fact]
    public async Task PersonSearchByNameJsonExceptionSecondResponse()
    {
        _response.StatusCode = HttpStatusCode.OK;
        string json = @"
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

        MemoryStream personStream = new(Encoding.UTF8.GetBytes(json));
        _response.Content = new StreamContent(personStream);
        PersonRetrievalService service = new(_options.Object);
        HttpResponseMessage personResponse = new();
        personResponse.StatusCode = HttpStatusCode.OK;
        string personJson = @"
        {{{{
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
          ""other-names"": {
            ""last-modified-date"": null,
            ""other-name"": [],
            ""path"": ""/0000-0001-8564-3504/other-names""
          },
          ""biography"": {
              ""value"": ""Upstanding citizen by day but at night he transforms into the rizzler""
            },
          ""researcher-urls"": {
            ""last-modified-date"": null,
            ""researcher-url"": [],
            ""path"": ""/0000-0001-8564-3504/researcher-urls""
          },
          ""emails"": {
            ""last-modified-date"": null,
            ""email"": [],
            ""path"": ""/0000-0001-8564-3504/email""
          },
          ""addresses"": {
            ""last-modified-date"": null,
            ""address"": [],
            ""path"": ""/0000-0001-8564-3504/address""
          },
          ""keywords"": {
            ""last-modified-date"": null,
            ""keyword"": [],
            ""path"": ""/0000-0001-8564-3504/keywords""
          },
          ""external-identifiers"": {
            ""last-modified-date"": null,
            ""external-identifier"": [],
            ""path"": ""/0000-0001-8564-3504/external-identifiers""
          },
          ""path"": ""/0000-0001-8564-3504/person""
        }}}
        ";
        personResponse.Content = new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes(personJson)));
        _messageHandlerMock.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Get &&
                req.RequestUri != null &&
                req.RequestUri.ToString() == "https://pub.sandbox.orcid.org/v3.0/0000-0002-7614-2895/person"),
            ItExpr.IsAny<CancellationToken>()).Returns(Task.FromResult(personResponse));
        await Assert.ThrowsAsync<OrcidServiceException>(() =>
            service.FindPeopleByName("Doesnt matter will return set response", 10));
    }

    [Fact]
    public async Task PersonSearchByNameNoMatches()
    {
        _response.StatusCode = HttpStatusCode.OK;
        string json = @"
        {
          ""result"" : [],
        ""num-found"": 0
        }
        ";
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
        _response.StatusCode = HttpStatusCode.BadRequest;
        PersonRetrievalService service = new(_options.Object);
        await Assert.ThrowsAsync<OrcidServiceException>(() =>
            service.FindPeopleByName("Doesn't matter will return set response anyway", 100));
    }

    [Fact]
    public async Task PersonSearchByNameHttpException()
    {
        _messageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()).Throws(new HttpRequestException());
        PersonRetrievalService service = new(_options.Object);
        await Assert.ThrowsAsync<OrcidServiceException>(() =>
            service.FindPeopleByName("Doesn't matter will return set response anyway", 100));
    }

    [Fact]
    public async Task PersonSearchByNameJsonExceptionFirstResponse()
    {
        _response.StatusCode = HttpStatusCode.OK;
        string json = @"
        {
          ""result"" : [],
        ""num-found"": 0
        }}
        ";
        MemoryStream peopleStream = new(Encoding.UTF8.GetBytes(json));
        _response.Content = new StreamContent(peopleStream);
        PersonRetrievalService service = new(_options.Object);
        await Assert.ThrowsAsync<OrcidServiceException>(() =>
            service.FindPeopleByName("Doesn't matter will return set response anyway", 100));
    }

    [Fact]
    public async Task PersonRetrievalBadResponse()
    {
        _response.StatusCode = HttpStatusCode.BadRequest;
        PersonRetrievalService service = new(_options.Object);
        await Assert.ThrowsAsync<OrcidServiceException>(() =>
            service.FindPersonByOrcid("Doesn't matter will return set response anyway"));
    }

    [Fact]
    public async Task PersonRetrievalHttpException()
    {
        _messageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()).Throws(new HttpRequestException());
        PersonRetrievalService service = new(_options.Object);
        await Assert.ThrowsAsync<OrcidServiceException>(() =>
            service.FindPersonByOrcid("Doesn't matter will return set response anyway"));
    }

    [Fact]
    public async Task PersonRetrievalJsonException()
    {
        _response.StatusCode = HttpStatusCode.OK;
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
          ""other-names"": {
            ""last-modified-date"": null,
            ""other-name"": [],
            ""path"": ""/0000-0001-8564-3504/other-names""
          },
          ""biography"": null,
          ""researcher-urls"": {
            ""last-modified-date"": null,
            ""researcher-url"": [],
            ""path"": ""/0000-0001-8564-3504/researcher-urls""
          },
          ""emails"": {
            ""last-modified-date"": null,
            ""email"": [],
            ""path"": ""/0000-0001-8564-3504/email""
          },
          ""addresses"": {
            ""last-modified-date"": null,
            ""address"": [],
            ""path"": ""/0000-0001-8564-3504/address""
          },
          ""keywords"": {
            ""last-modified-date"": null,
            ""keyword"": [],
            ""path"": ""/0000-0001-8564-3504/keywords""
          },
          ""external-identifiers"": {
            ""last-modified-date"": null,
            ""external-identifier"": [],
            ""path"": ""/0000-0001-8564-3504/external-identifiers""
          },
          ""path"": ""/0000-0001-8564-3504/person""
        }}}}}
        ";
        MemoryStream personStream = new(Encoding.UTF8.GetBytes(json));
        _response.Content = new StreamContent(personStream);
        PersonRetrievalService service = new(_options.Object);
        await Assert.ThrowsAsync<OrcidServiceException>(() =>
            service.FindPersonByOrcid("Doesn't matter will return set response anyway"));
    }

    [Fact]
    public async Task PersonSearchByNameFastJsonException()
    {
        _response.StatusCode = HttpStatusCode.OK;
        string json = @"
        {
          ""result"" : [],
        ""num-found"": 0
        }}
        ";
        MemoryStream peopleStream = new(Encoding.UTF8.GetBytes(json));
        _response.Content = new StreamContent(peopleStream);
        PersonRetrievalService service = new(_options.Object);
        await Assert.ThrowsAsync<OrcidServiceException>(() =>
            service.FindPeopleByNameFast("Doesn't matter will return set response anyway"));
    }

    [Fact]
    public async Task PersonRetrievalFastBadResponse()
    {
        _response.StatusCode = HttpStatusCode.BadRequest;
        PersonRetrievalService service = new(_options.Object);
        await Assert.ThrowsAsync<OrcidServiceException>(() =>
            service.FindPeopleByNameFast("Doesn't matter will return set response anyway"));
    }

    [Fact]
    public async Task PersonRetrievalFastHttpException()
    {
        _messageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()).Throws(new HttpRequestException());
        PersonRetrievalService service = new(_options.Object);
        await Assert.ThrowsAsync<OrcidServiceException>(() =>
            service.FindPeopleByNameFast("Doesn't matter will return set response anyway"));
    }

    [Fact]
    public async Task PersonRetrievalFastWrongVersionHttpException()
    {
        _client = new(_messageHandlerMock.Object);
        _client.BaseAddress = new(PersonRetrievalServiceOptions.OrcidSandboxUrlPreviousVersion);
        _options.Setup(options => options.BuildRequestClient()).Returns(_client);
        PersonRetrievalService service = new(_options.Object);
        await Assert.ThrowsAsync<OrcidServiceException>(() =>
            service.FindPeopleByNameFast("Doesn't matter will return set response anyway"));
    }

    [Fact]
    public async Task PersonRetrievalFastGoodCase()
    {
        _response.StatusCode = HttpStatusCode.OK;
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
          }, {
            ""orcid-id"" : ""0000-0002-4286-1306"",
            ""given-names"" : ""Brenda J. Waning, MPH"",
            ""family-names"" : ""Waning"",
            ""credit-name"" : ""Brenda J. Waning, MPH"",
            ""other-name"" : [ ],
            ""email"" : [ ],
            ""institution-name"" : [ ]
          }, {
            ""orcid-id"" : ""0000-0003-1779-572X"",
            ""given-names"" : ""Brenda"",
            ""family-names"" : ""Waning"",
            ""credit-name"" : null,
            ""other-name"" : [ ],
            ""email"" : [ ""bwaning@bu.edu"" ],
            ""institution-name"" : [ ]
          }, {
            ""orcid-id"" : ""0000-0003-3316-8742"",
            ""given-names"" : ""Brenda"",
            ""family-names"" : ""Waning"",
            ""credit-name"" : null,
            ""other-name"" : [ ],
            ""email"" : [ ""bwaning456@bu.edu"" ],
            ""institution-name"" : [ ]
          }, {
            ""orcid-id"" : ""0000-0001-6993-4838"",
            ""given-names"" : ""Brenda"",
            ""family-names"" : ""Waning"",
            ""credit-name"" : null,
            ""other-name"" : [ ],
            ""email"" : [ ""bwaning496@bu.edu"" ],
            ""institution-name"" : [ ]
          }, {
            ""orcid-id"" : ""0009-0002-4725-1512"",
            ""given-names"" : ""Brenda"",
            ""family-names"" : ""Medeiros"",
            ""credit-name"" : null,
            ""other-name"" : [ ],
            ""email"" : [ ],
            ""institution-name"" : [ ""Universidade Federal de São Carlos"" ]
          }, {
            ""orcid-id"" : ""0000-0001-9154-5636"",
            ""given-names"" : ""Brenda"",
            ""family-names"" : ""Deely"",
            ""credit-name"" : null,
            ""other-name"" : [ ],
            ""email"" : [ ""bdeely@bepress.com"" ],
            ""institution-name"" : [ ""Bepress (United States)"" ]
          }, {
            ""orcid-id"" : ""0000-0001-7424-8357"",
            ""given-names"" : ""Brenda"",
            ""family-names"" : ""Heaton"",
            ""credit-name"" : null,
            ""other-name"" : [ ],
            ""email"" : [ ""brenda9456@bu.edu"" ],
            ""institution-name"" : [ ]
          }, {
            ""orcid-id"" : ""0000-0002-1597-6677"",
            ""given-names"" : ""Brenda"",
            ""family-names"" : ""Heaton"",
            ""credit-name"" : null,
            ""other-name"" : [ ],
            ""email"" : [ ""brenda9@bu.edu"" ],
            ""institution-name"" : [ ]
          }, {
            ""orcid-id"" : ""0000-0001-8800-6551"",
            ""given-names"" : ""Brenda"",
            ""family-names"" : ""Heaton"",
            ""credit-name"" : null,
            ""other-name"" : [ ],
            ""email"" : [ ""brenda9496@bu.edu"" ],
            ""institution-name"" : [ ]
          }, {
            ""orcid-id"" : ""0000-0003-1927-387X"",
            ""given-names"" : ""Brenda"",
            ""family-names"" : ""Schlagenhauf"",
            ""credit-name"" : null,
            ""other-name"" : [ ],
            ""email"" : [ ],
            ""institution-name"" : [ ]
          }, {
            ""orcid-id"" : ""0000-0002-9071-7691"",
            ""given-names"" : ""Brenda"",
            ""family-names"" : ""Dean"",
            ""credit-name"" : null,
            ""other-name"" : [ ],
            ""email"" : [ ],
            ""institution-name"" : [ ]
          }, {
            ""orcid-id"" : ""0000-0002-7807-7898"",
            ""given-names"" : ""Brenda"",
            ""family-names"" : ""Dean"",
            ""credit-name"" : null,
            ""other-name"" : [ ],
            ""email"" : [ ],
            ""institution-name"" : [ ]
          }, {
            ""orcid-id"" : ""0000-0002-1826-0949"",
            ""given-names"" : ""Brenda"",
            ""family-names"" : ""Dean"",
            ""credit-name"" : null,
            ""other-name"" : [ ],
            ""email"" : [ ],
            ""institution-name"" : [ ]
          }, {
            ""orcid-id"" : ""0000-0003-2755-4129"",
            ""given-names"" : ""Brenda"",
            ""family-names"" : ""Linares"",
            ""credit-name"" : null,
            ""other-name"" : [ ],
            ""email"" : [ ],
            ""institution-name"" : [ ]
          }, {
            ""orcid-id"" : ""0000-0002-1034-6860"",
            ""given-names"" : ""Brenda"",
            ""family-names"" : ""Barrie"",
            ""credit-name"" : null,
            ""other-name"" : [ ],
            ""email"" : [ ],
            ""institution-name"" : [ ]
          }, {
            ""orcid-id"" : ""0000-0001-5985-448X"",
            ""given-names"" : ""brenda"",
            ""family-names"" : ""jones"",
            ""credit-name"" : null,
            ""other-name"" : [ ],
            ""email"" : [ ],
            ""institution-name"" : [ ]
          }, {
            ""orcid-id"" : ""0000-0001-6034-3038"",
            ""given-names"" : ""Brenda"",
            ""family-names"" : ""Njoko"",
            ""credit-name"" : null,
            ""other-name"" : [ ],
            ""email"" : [ ],
            ""institution-name"" : [ ]
          }, {
            ""orcid-id"" : ""0009-0005-2448-5991"",
            ""given-names"" : ""Brenda"",
            ""family-names"" : ""Tandayu"",
            ""credit-name"" : null,
            ""other-name"" : [ ],
            ""email"" : [ ],
            ""institution-name"" : [ ]
          }, {
            ""orcid-id"" : ""0009-0007-8875-055X"",
            ""given-names"" : ""Brenda"",
            ""family-names"" : ""Young"",
            ""credit-name"" : null,
            ""other-name"" : [ ],
            ""email"" : [ ],
            ""institution-name"" : [ ]
          }, {
            ""orcid-id"" : ""0009-0009-3210-3753"",
            ""given-names"" : ""Brenda"",
            ""family-names"" : ""Carter"",
            ""credit-name"" : null,
            ""other-name"" : [ ],
            ""email"" : [ ],
            ""institution-name"" : [ ]
          }, {
            ""orcid-id"" : ""0009-0006-3132-4704"",
            ""given-names"" : ""Brenda"",
            ""family-names"" : ""Lemons"",
            ""credit-name"" : null,
            ""other-name"" : [ ],
            ""email"" : [ ],
            ""institution-name"" : [ ]
          }, {
            ""orcid-id"" : ""0000-0001-6087-6037"",
            ""given-names"" : ""Brenda"",
            ""family-names"" : ""Cross"",
            ""credit-name"" : null,
            ""other-name"" : [ ],
            ""email"" : [ ],
            ""institution-name"" : [ ]
          }, {
            ""orcid-id"" : ""0000-0003-0357-0415"",
            ""given-names"" : ""Brenda"",
            ""family-names"" : ""Griffith-Williams"",
            ""credit-name"" : null,
            ""other-name"" : [ ],
            ""email"" : [ ],
            ""institution-name"" : [ ]
          }, {
            ""orcid-id"" : ""0000-0002-8612-6266"",
            ""given-names"" : ""BRENDA"",
            ""family-names"" : ""BUITRAGO"",
            ""credit-name"" : null,
            ""other-name"" : [ ],
            ""email"" : [ ],
            ""institution-name"" : [ ]
          }, {
            ""orcid-id"" : ""0000-0002-4528-628X"",
            ""given-names"" : ""BRENDA"",
            ""family-names"" : ""ROMERO"",
            ""credit-name"" : null,
            ""other-name"" : [ ],
            ""email"" : [ ],
            ""institution-name"" : [ ""University of Colorado Boulder"" ]
          }, {
            ""orcid-id"" : ""0000-0001-9442-8996"",
            ""given-names"" : ""Brenda"",
            ""family-names"" : ""Heaton"",
            ""credit-name"" : null,
            ""other-name"" : [ ],
            ""email"" : [ ],
            ""institution-name"" : [ ""Boston University Goldman School of Dental Medicine"" ]
          }, {
            ""orcid-id"" : ""0000-0001-5427-9893"",
            ""given-names"" : ""BRENDA"",
            ""family-names"" : ""ROMERO"",
            ""credit-name"" : null,
            ""other-name"" : [ ],
            ""email"" : [ ],
            ""institution-name"" : [ ""University of Colorado Boulder"" ]
          }, {
            ""orcid-id"" : ""0000-0003-1631-1746"",
            ""given-names"" : ""Brenda"",
            ""family-names"" : ""Waning"",
            ""credit-name"" : null,
            ""other-name"" : [ ],
            ""email"" : [ ],
            ""institution-name"" : [ ""Boston University School of Medicine"" ]
          } ],
          ""num-found"" : 30
        }
        ";

        MemoryStream personStream = new(Encoding.UTF8.GetBytes(json));
        _response.Content = new StreamContent(personStream);
        PersonRetrievalService service = new(_options.Object);
        var people = await service.FindPeopleByNameFast("Doesn't matter will return set response anyway");
        Assert.Equal(30, people.Count);
        OrcidPerson person = people[0];
        Assert.NotNull(person);
        Assert.Equal("brenda", person.FirstName);
        Assert.Equal("marshall", person.LastName);
        Assert.Null(person.Biography);
        Assert.Null(person.CreditName);
    }

    [Fact]
    public async Task SearchRequestAndParseGoodCase()
    {
        _response.StatusCode = HttpStatusCode.OK;
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
          }, {
            ""orcid-id"" : ""0000-0002-4286-1306"",
            ""given-names"" : ""Brenda J. Waning, MPH"",
            ""family-names"" : ""Waning"",
            ""credit-name"" : ""Brenda J. Waning, MPH"",
            ""other-name"" : [ ],
            ""email"" : [ ],
            ""institution-name"" : [ ]
          }, {
            ""orcid-id"" : ""0000-0003-1779-572X"",
            ""given-names"" : ""Brenda"",
            ""family-names"" : ""Waning"",
            ""credit-name"" : null,
            ""other-name"" : [ ],
            ""email"" : [ ""bwaning@bu.edu"" ],
            ""institution-name"" : [ ]
          }, {
            ""orcid-id"" : ""0000-0003-3316-8742"",
            ""given-names"" : ""Brenda"",
            ""family-names"" : ""Waning"",
            ""credit-name"" : null,
            ""other-name"" : [ ],
            ""email"" : [ ""bwaning456@bu.edu"" ],
            ""institution-name"" : [ ]
          }, {
            ""orcid-id"" : ""0000-0001-6993-4838"",
            ""given-names"" : ""Brenda"",
            ""family-names"" : ""Waning"",
            ""credit-name"" : null,
            ""other-name"" : [ ],
            ""email"" : [ ""bwaning496@bu.edu"" ],
            ""institution-name"" : [ ]
          }, {
            ""orcid-id"" : ""0009-0002-4725-1512"",
            ""given-names"" : ""Brenda"",
            ""family-names"" : ""Medeiros"",
            ""credit-name"" : null,
            ""other-name"" : [ ],
            ""email"" : [ ],
            ""institution-name"" : [ ""Universidade Federal de São Carlos"" ]
          }, {
            ""orcid-id"" : ""0000-0001-9154-5636"",
            ""given-names"" : ""Brenda"",
            ""family-names"" : ""Deely"",
            ""credit-name"" : null,
            ""other-name"" : [ ],
            ""email"" : [ ""bdeely@bepress.com"" ],
            ""institution-name"" : [ ""Bepress (United States)"" ]
          }, {
            ""orcid-id"" : ""0000-0001-7424-8357"",
            ""given-names"" : ""Brenda"",
            ""family-names"" : ""Heaton"",
            ""credit-name"" : null,
            ""other-name"" : [ ],
            ""email"" : [ ""brenda9456@bu.edu"" ],
            ""institution-name"" : [ ]
          }, {
            ""orcid-id"" : ""0000-0002-1597-6677"",
            ""given-names"" : ""Brenda"",
            ""family-names"" : ""Heaton"",
            ""credit-name"" : null,
            ""other-name"" : [ ],
            ""email"" : [ ""brenda9@bu.edu"" ],
            ""institution-name"" : [ ]
          }, {
            ""orcid-id"" : ""0000-0001-8800-6551"",
            ""given-names"" : ""Brenda"",
            ""family-names"" : ""Heaton"",
            ""credit-name"" : null,
            ""other-name"" : [ ],
            ""email"" : [ ""brenda9496@bu.edu"" ],
            ""institution-name"" : [ ]
          }, {
            ""orcid-id"" : ""0000-0003-1927-387X"",
            ""given-names"" : ""Brenda"",
            ""family-names"" : ""Schlagenhauf"",
            ""credit-name"" : null,
            ""other-name"" : [ ],
            ""email"" : [ ],
            ""institution-name"" : [ ]
          }, {
            ""orcid-id"" : ""0000-0002-9071-7691"",
            ""given-names"" : ""Brenda"",
            ""family-names"" : ""Dean"",
            ""credit-name"" : null,
            ""other-name"" : [ ],
            ""email"" : [ ],
            ""institution-name"" : [ ]
          }, {
            ""orcid-id"" : ""0000-0002-7807-7898"",
            ""given-names"" : ""Brenda"",
            ""family-names"" : ""Dean"",
            ""credit-name"" : null,
            ""other-name"" : [ ],
            ""email"" : [ ],
            ""institution-name"" : [ ]
          }, {
            ""orcid-id"" : ""0000-0002-1826-0949"",
            ""given-names"" : ""Brenda"",
            ""family-names"" : ""Dean"",
            ""credit-name"" : null,
            ""other-name"" : [ ],
            ""email"" : [ ],
            ""institution-name"" : [ ]
          }, {
            ""orcid-id"" : ""0000-0003-2755-4129"",
            ""given-names"" : ""Brenda"",
            ""family-names"" : ""Linares"",
            ""credit-name"" : null,
            ""other-name"" : [ ],
            ""email"" : [ ],
            ""institution-name"" : [ ]
          }, {
            ""orcid-id"" : ""0000-0002-1034-6860"",
            ""given-names"" : ""Brenda"",
            ""family-names"" : ""Barrie"",
            ""credit-name"" : null,
            ""other-name"" : [ ],
            ""email"" : [ ],
            ""institution-name"" : [ ]
          }, {
            ""orcid-id"" : ""0000-0001-5985-448X"",
            ""given-names"" : ""brenda"",
            ""family-names"" : ""jones"",
            ""credit-name"" : null,
            ""other-name"" : [ ],
            ""email"" : [ ],
            ""institution-name"" : [ ]
          }, {
            ""orcid-id"" : ""0000-0001-6034-3038"",
            ""given-names"" : ""Brenda"",
            ""family-names"" : ""Njoko"",
            ""credit-name"" : null,
            ""other-name"" : [ ],
            ""email"" : [ ],
            ""institution-name"" : [ ]
          }, {
            ""orcid-id"" : ""0009-0005-2448-5991"",
            ""given-names"" : ""Brenda"",
            ""family-names"" : ""Tandayu"",
            ""credit-name"" : null,
            ""other-name"" : [ ],
            ""email"" : [ ],
            ""institution-name"" : [ ]
          }, {
            ""orcid-id"" : ""0009-0007-8875-055X"",
            ""given-names"" : ""Brenda"",
            ""family-names"" : ""Young"",
            ""credit-name"" : null,
            ""other-name"" : [ ],
            ""email"" : [ ],
            ""institution-name"" : [ ]
          }, {
            ""orcid-id"" : ""0009-0009-3210-3753"",
            ""given-names"" : ""Brenda"",
            ""family-names"" : ""Carter"",
            ""credit-name"" : null,
            ""other-name"" : [ ],
            ""email"" : [ ],
            ""institution-name"" : [ ]
          }, {
            ""orcid-id"" : ""0009-0006-3132-4704"",
            ""given-names"" : ""Brenda"",
            ""family-names"" : ""Lemons"",
            ""credit-name"" : null,
            ""other-name"" : [ ],
            ""email"" : [ ],
            ""institution-name"" : [ ]
          }, {
            ""orcid-id"" : ""0000-0001-6087-6037"",
            ""given-names"" : ""Brenda"",
            ""family-names"" : ""Cross"",
            ""credit-name"" : null,
            ""other-name"" : [ ],
            ""email"" : [ ],
            ""institution-name"" : [ ]
          }, {
            ""orcid-id"" : ""0000-0003-0357-0415"",
            ""given-names"" : ""Brenda"",
            ""family-names"" : ""Griffith-Williams"",
            ""credit-name"" : null,
            ""other-name"" : [ ],
            ""email"" : [ ],
            ""institution-name"" : [ ]
          }, {
            ""orcid-id"" : ""0000-0002-8612-6266"",
            ""given-names"" : ""BRENDA"",
            ""family-names"" : ""BUITRAGO"",
            ""credit-name"" : null,
            ""other-name"" : [ ],
            ""email"" : [ ],
            ""institution-name"" : [ ]
          }, {
            ""orcid-id"" : ""0000-0002-4528-628X"",
            ""given-names"" : ""BRENDA"",
            ""family-names"" : ""ROMERO"",
            ""credit-name"" : null,
            ""other-name"" : [ ],
            ""email"" : [ ],
            ""institution-name"" : [ ""University of Colorado Boulder"" ]
          }, {
            ""orcid-id"" : ""0000-0001-9442-8996"",
            ""given-names"" : ""Brenda"",
            ""family-names"" : ""Heaton"",
            ""credit-name"" : null,
            ""other-name"" : [ ],
            ""email"" : [ ],
            ""institution-name"" : [ ""Boston University Goldman School of Dental Medicine"" ]
          }, {
            ""orcid-id"" : ""0000-0001-5427-9893"",
            ""given-names"" : ""BRENDA"",
            ""family-names"" : ""ROMERO"",
            ""credit-name"" : null,
            ""other-name"" : [ ],
            ""email"" : [ ],
            ""institution-name"" : [ ""University of Colorado Boulder"" ]
          }, {
            ""orcid-id"" : ""0000-0003-1631-1746"",
            ""given-names"" : ""Brenda"",
            ""family-names"" : ""Waning"",
            ""credit-name"" : null,
            ""other-name"" : [ ],
            ""email"" : [ ],
            ""institution-name"" : [ ""Boston University School of Medicine"" ]
          } ],
          ""num-found"" : 30
        }
        ";

        MemoryStream personStream = new(Encoding.UTF8.GetBytes(json));
        _response.Content = new StreamContent(personStream);
        PersonRetrievalService service = new(_options.Object);
        var people =
            await service.SearchResultRequestAndParse<PersonExpandedSearchResult>(
                "Doesn't matter will return set response anyway", "expanded-result");
        Assert.Equal(30, people.Count);
        PersonExpandedSearchResult person = people[0];
        Assert.NotNull(person);
        Assert.Equal("brenda", person.FirstName);
        Assert.Equal("marshall", person.LastName);
        Assert.Null(person.CreditName);
    }

    [Fact]
    public async Task SearchRequestAndParseJsonException()
    {
        _response.StatusCode = HttpStatusCode.OK;
        string json = "klsdfjlsdfj";

        MemoryStream personStream = new(Encoding.UTF8.GetBytes(json));
        _response.Content = new StreamContent(personStream);
        PersonRetrievalService service = new(_options.Object);
        await Assert.ThrowsAsync<OrcidServiceException>(() =>
            service.SearchResultRequestAndParse<PersonExpandedSearchResult>(
                "Doesn't matter will return set response anyway", "expanded-result"));
    }

    [Fact]
    public async Task SearchRequestAndParseHttpException()
    {
        _messageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()).Throws(new HttpRequestException());
        PersonRetrievalService service = new(_options.Object);
        await Assert.ThrowsAsync<OrcidServiceException>(() =>
            service.SearchResultRequestAndParse<PersonExpandedSearchResult>(
                "Doesn't matter will return set response anyway", "sldfjlsdkj"));
    }
}
