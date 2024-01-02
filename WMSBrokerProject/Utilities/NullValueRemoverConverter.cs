using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WMSBrokerProject.Utilities
{
    //public class NullValueRemoverConverter : JsonConverter
    //{
    //    public override bool CanConvert(Type objectType)
    //    {
    //        return true;
    //    }

    //    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    //    {
    //        var jObject = JObject.FromObject(value, serializer);

    //        var propertiesToRemove = jObject
    //            .Properties()
    //            .Where(p => p.Value.Type == JTokenType.Null)
    //            .ToList();

    //        foreach (var property in propertiesToRemove)
    //        {
    //            property.Remove();
    //        }

    //        jObject.WriteTo(writer);
    //    }
    //}
}
