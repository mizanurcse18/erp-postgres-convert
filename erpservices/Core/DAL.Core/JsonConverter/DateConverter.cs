using Core;

using Newtonsoft.Json.Converters;

namespace DAL.Core.JsonConverter
{
    public class DateConverter : IsoDateTimeConverter
    {
        public DateConverter()
        {
            DateTimeFormat = Util.SysDateFormat;
        }
    }
}
