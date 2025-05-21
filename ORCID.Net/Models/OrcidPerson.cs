// This program has been developed by students from the bachelor Computer Science at Utrecht
// University within the Software Project course.
// 
// © Copyright Utrecht University (Department of Information and Computing Sciences)

namespace ORCID.Net.Models;

public class OrcidPerson(string? firstName, string? lastName, string? creditName, string? biography, string? orcid)
{
    public string? FirstName { get; set; } = firstName;
    public string? LastName { get; set; } = lastName;
    public string? CreditName { get; set; } = creditName;
    public string? Biography { get; set; } = biography;

    public string? Orcid { get; set; } = orcid;

    public override string ToString() => $"{FirstName} {LastName} {CreditName} {Biography} {Orcid}";
}
