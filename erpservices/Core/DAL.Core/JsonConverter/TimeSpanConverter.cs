using System;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Core;

namespace DAL.Core.JsonConverter
{
    public class TimeSpanConverter : Newtonsoft.Json.JsonConverter
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return JToken.ReadFrom(reader).Value<long>();
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(long) == objectType;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, DateTime.MinValue.Add((TimeSpan)value).ToString(Util.EntryTimeFormat));
        }
    }
}
