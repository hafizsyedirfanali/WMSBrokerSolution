using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace WMSBrokerProject.Utilities
{
	//public class DontPassNullConverter : System.Text.Json.Serialization.JsonConverter
 //   {
 //       public DontPassNullConverter()
 //       {
            
 //       }
 //       public override bool CanConvert(Type typeToConvert)
	//	{
	//		throw new NotImplementedException();
	//	}
	//	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	//	{
	//		throw new NotImplementedException();
	//	}

	//	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	//	{
	//		var jObject = JObject.FromObject(value, serializer);

	//		var propertiesToRemove = jObject
	//			.Properties()
	//			.Where(p => p.Value.Type == JTokenType.Null)
	//			.ToList();

	//		foreach (var property in propertiesToRemove)
	//		{
	//			property.Remove();
	//		}

	//		jObject.WriteTo(writer);
	//	}

	//}
}
