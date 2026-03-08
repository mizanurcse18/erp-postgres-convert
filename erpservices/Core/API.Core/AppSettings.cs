using System;
using System.Collections.Generic;
using System.Text;

namespace API.Core
{
    public class AppSettings
    {
        public Service Service { get; set; }
        public string Secret { get; set; }
        public string Culture { get; set; }
        public string ExternalSchema { get; set; }
        public string APPVersion { get; set; }
    }

    public class Service
    {
        public string Name { set; get; }
        public string Description { set; get; }
        public string Version { set; get; }
    }    
}
