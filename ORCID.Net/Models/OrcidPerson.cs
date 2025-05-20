
namespace ORCID.Net.Models;

public class OrcidPerson(string? firstName, string? lastName, string? creditName, string? biography)
{

    public string? FirstName { get; set; } = firstName;
    public string? LastName { get; set; } = lastName;
    public string? CreditName { get; set; } = creditName;
    public string? Biography { get; set; } = biography;

    public override string ToString() => $"{FirstName} {LastName} {CreditName} {Biography}";
}

