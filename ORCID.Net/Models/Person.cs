using System.Text.Json.Serialization;

namespace ORCID.Net.Models;

public class Person
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string CreditName { get; set; }
    public string Biography { get; set; }

    public Person(string firstName, string lastName, string creditName, string biography)
    {
        FirstName = firstName;
        LastName = lastName;
        CreditName = creditName;
        Biography = biography;
    }

    public override string ToString() => $"{FirstName} {LastName} {CreditName} {Biography}";
}
