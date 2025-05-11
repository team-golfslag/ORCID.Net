// This program has been developed by students from the bachelor Computer Science at Utrecht
// University within the Software Project course.
// 
// © Copyright Utrecht University (Department of Information and Computing Sciences)

using ORCID.Net.Models;

namespace ORCID.Net.Services;

public interface IPersonRetrievalService
{
    Task<Person> FindPersonByOrcid(string personId);
    
    Task<List<Person>> FindPeopleByName(string personName, int resultAmount);
}
