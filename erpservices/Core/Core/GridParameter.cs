using System.Collections.Generic;

namespace Core
{
    public class GridParameter
    {
        public string TempKeyName { get; set; }

        public bool ServerPagination { get; set; }
        public int Limit { get; set; }
        public int Offset { get; set; }

        public string Order { get; set; }
        public string SearchBy { get; set; }
        public string SearchType { get; set; }
        public string Search { get; set; }
        public string Sort { get; set; }
        public string ApprovalFilterData { get; set; }
        public string AdditionalFilterData { get; set; }
        public string EmployeeTypes { get; set; }

        //public string AssemblyName { get; set; }
        //public string ClassName { get; set; }
        //public string MethodName { get; set; }
        //public string MethodParams { get; set; }

        //public bool IsSession { get; set; }

        public string SortName { get; set; }
        public string SortOrder { get; set; }
        public bool IsSubmittedFromPopup { get; set; }

        public List<FinderParameter> Parameters
        {
            get;
            set;
        }
        public int MenuID { get; set; }
        public SearchModel SearchModel { get; set; }
    }
}
