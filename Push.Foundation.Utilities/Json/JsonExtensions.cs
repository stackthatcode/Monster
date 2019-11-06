using System.IO;
using Newtonsoft.Json;

namespace Push.Foundation.Utilities.Json
{
    public static class JsonExtensions
    {
        public static JsonSerializerSettings SerializerSettings = new JsonSerializerSettings();
        
        
        public static string SerializeToJson(
                this object input,
                Formatting formatting = Formatting.Indented)
        {
            var stringWriter = new StringWriter();
            input.SerializeToJson(stringWriter, formatting);
            return stringWriter.ToString();
        }

        public static void SerializeToJson(
                this object input, 
                TextWriter textWriter,
                Formatting formatting = Formatting.Indented)
        {
            var writer = new JsonTextWriter(textWriter) { Formatting = formatting };
            var serializer = JsonSerializer.Create(SerializerSettings);
            serializer.Serialize(writer, input);
            writer.Flush();
        }

        public static T DeserializeFromJson<T>(this string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
