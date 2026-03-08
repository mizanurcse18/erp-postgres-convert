using System;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace Core
{
    public class FinderParameter
    {

        public FinderParameter()
        {
        }
        public string name
        {
            get;
            set;
        }
        public string type { get; set; } = "String";
        public string operat { get; set; } = "cn";
        //[JsonConverter(typeof(int))]
        public string value
        {
            get;
            set;
        }

    }
}