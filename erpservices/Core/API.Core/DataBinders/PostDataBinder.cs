using System.Collections.Generic;
using Newtonsoft.Json;

namespace API.Core.DataBinders
{
    public class PostDataBinder<T> where T : class, new()
    {
        public static T BindSingle(string jsonString)
        {
            return JsonConvert.DeserializeObject<T>(jsonString);
        }

        public static IEnumerable<T> BindEnumerable(string jsonString)
        {
            return JsonConvert.DeserializeObject<IEnumerable<T>>(jsonString);
        }
    }
}
