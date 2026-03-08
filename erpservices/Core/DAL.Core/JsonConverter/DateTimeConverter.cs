using Core;

using Newtonsoft.Json.Converters;

namespace DAL.Core.JsonConverter
{
    public class DateTimeConverter : IsoDateTimeConverter
    {
        public DateTimeConverter()
        {
            DateTimeFormat = Util.SysDateTimeFormat;
        }
    }
}
