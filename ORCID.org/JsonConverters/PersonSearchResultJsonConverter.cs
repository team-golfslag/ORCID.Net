using System.Text.Json;
using System.Text.Json.Serialization;
using ORCID.org.Models;

namespace ORCID.org.JsonConverters;


public class PersonSearchResultJsonConverter : JsonConverter<PersonSearchResult>
{
    public override PersonSearchResult Read(ref Utf8JsonReader reader, Type typeToConvert,
        JsonSerializerOptions options)
    {
        JsonDocument doc = JsonDocument.ParseValue(ref reader);
        JsonElement root = doc.RootElement;

        
        string uri = root.GetProperty("orcid-identifier").GetProperty("uri").GetString();
        string path = root.GetProperty("orcid-identifier").GetProperty("path").GetString();
        string host = root.GetProperty("orcid-identifier").GetProperty("host").GetString();

        return new PersonSearchResult();
    }

    public override void Write(Utf8JsonWriter writer, PersonSearchResult value, JsonSerializerOptions options)
    {
        throw new NotImplementedException(); // Implement if you need to serialize back
    }

}