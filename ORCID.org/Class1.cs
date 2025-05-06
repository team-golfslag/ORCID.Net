using ORCID.org.Models;
using ORCID.org.Services;
using System;
using System.Threading.Tasks;

namespace ORCID.org;

public class Program
{
	private static PersonRetrievalService api;
	private static PersonRetrievalServiceOptions apiOptions;
	
	public static async Task Main(string[] args)
	{
		Console.WriteLine("Please enter api access token:");
		string accessToken = Console.ReadLine();
		apiOptions = new PersonRetrievalServiceOptions(accessToken);
		api = new PersonRetrievalService(apiOptions);
		var person = await api.FindPersonByOrcid("0000-0002-0272-9909");
		Console.WriteLine(person.ToString());

	}

}