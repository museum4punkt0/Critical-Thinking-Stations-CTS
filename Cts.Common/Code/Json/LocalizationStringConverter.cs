using Gemelo.Components.Common.Localization;
using Newtonsoft.Json;
using System;

namespace Gemelo.Components.Cts.Common.Code.Json
{
    public class LocalizationStringConverter : JsonConverter<LocalizationString>
    {
        public override LocalizationString ReadJson(JsonReader reader, Type objectType,
            LocalizationString existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.StartObject)
            {
                LocalizationString result = new LocalizationString();
                reader.Read();
                while (reader.TokenType != JsonToken.EndObject)
                {
                    if (reader.TokenType != JsonToken.PropertyName)
                        throw new JsonSerializationException("Unexpected token!");
                    string language = (string)reader.Value;
                    reader.Read();
                    if (reader.TokenType != JsonToken.String)
                        throw new JsonSerializationException("Unexpected token!");
                    string value = (string)reader.Value;
                    result.SetFor(language, value);
                    reader.Read();
                }
                return result;
            }
            else if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }
            else
            {
                throw new JsonSerializationException("Unexpected token!");
            }
        }

        public override void WriteJson(JsonWriter writer, LocalizationString value, JsonSerializer serializer)
        {
            writer.WriteRawValue(JsonConvert.SerializeObject(value.LocalizedStrings, Formatting.Indented));
        }
    }
}
