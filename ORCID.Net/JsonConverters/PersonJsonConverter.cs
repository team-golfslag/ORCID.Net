using ORCID.Net.ORCIDServiceExceptions;

namespace ORCID.Net.JsonConverters;

using System.Text.Json;
using System.Text.Json.Serialization;
using Models;

public class PersonJsonConverter : JsonConverter<Person>
{
    public override Person Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var jsonDoc = JsonDocument.ParseValue(ref reader);
        var root = jsonDoc.RootElement;

        var nameElement = root.GetProperty("name");

        string? firstName = nameElement.GetProperty("given-names").GetProperty("value").GetString();
        string? lastName = nameElement.TryGetProperty("credit-name", out var last) && last.ValueKind == JsonValueKind.Object
            ? last.GetProperty("value").GetString()
            : null;
        string? creditName = nameElement.TryGetProperty("credit-name", out var credit) && credit.ValueKind == JsonValueKind.Object
            ? credit.GetProperty("value").GetString()
            : null;

        string? biography = root.TryGetProperty("biography", out var bio) && 
                            bio.ValueKind == JsonValueKind.Object &&
                            bio.TryGetProperty("value", out var bioValue)
            ? bioValue.GetString()
            : null;


        return new Person(firstName, lastName, creditName, biography);
    }

    public override void Write(Utf8JsonWriter writer, Person value, JsonSerializerOptions options)
    {
        throw new ORCIDServiceException("Serialization not implemented.");
    }
}
