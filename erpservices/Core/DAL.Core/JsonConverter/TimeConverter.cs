using Core;

using Newtonsoft.Json.Converters;

namespace DAL.Core.JsonConverter
{
    public class TimeConverter : IsoDateTimeConverter
    {
        public TimeConverter()
        {
            DateTimeFormat = Util.EntryTimeFormat;
        }
    }
}
