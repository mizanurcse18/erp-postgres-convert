using System;
using System.Collections.Generic;
using System.Text;

namespace Core.AppContexts
{
    public class Connection
    {
        public string Name { get; set; }
        public string Server { get; set; }
        public string Database { get; set; }
        public string UserId { get; set; }
        public string Password { get; set; }
        public string ConnectionString { get; set; }
    }
}
