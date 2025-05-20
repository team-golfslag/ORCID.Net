using ORCID.Net.ORCIDServiceExceptions;

namespace ORCID.Net.JsonConverters;

using System.Text.Json;
using System.Text.Json.Serialization;
using Models;

public class PersonJsonConverter : JsonConverter<OrcidPerson>
{
    public override OrcidPerson Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using JsonDocument jsonDoc = JsonDocument.ParseValue(ref reader);
        JsonElement root = jsonDoc.RootElement;

        JsonElement nameElement = root.GetProperty("name");

        string? firstName = nameElement.GetProperty("given-names").GetProperty("value").GetString();
        
        
        string? lastName = nameElement.TryGetProperty("family-name", out JsonElement last) && last.ValueKind == JsonValueKind.Object
            ? last.GetProperty("value").GetString()
            : null;
        string? creditName = nameElement.TryGetProperty("credit-name", out JsonElement credit) && credit.ValueKind == JsonValueKind.Object
            ? credit.GetProperty("value").GetString()
            : null;

        string? biography = root.TryGetProperty("biography", out JsonElement bio) && 
                            bio.ValueKind == JsonValueKind.Object &&
                            bio.TryGetProperty("value", out JsonElement bioValue)
            ? bioValue.GetString()
            : null;


        return new(firstName, lastName, creditName, biography, null);
    }

    public override void Write(Utf8JsonWriter writer, OrcidPerson value, JsonSerializerOptions options)
    {
        throw new OrcidServiceException("Serialization not implemented.");
    }
}
