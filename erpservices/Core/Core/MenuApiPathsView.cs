using System;
using System.Collections.Generic;
using System.Text;

namespace Core
{
    public class MenuApiPathsView
    {
        public int MenuID { get; set; }
        public bool CanCreate { get; set; }
        public bool CanRead { get; set; }
        public int UserID { get; set; }
        public string Module { get; set; }
        public string ApiPath { get; set; }
        public string ActionType { get; set; }
        public string Controller { get; set; }
    }
}
