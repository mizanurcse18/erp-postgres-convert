using Security.DAL.Entities;
using Security.Manager;
using Security.Manager.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Security.Manager.Dto
{
    public class PersonProfileApprovalDto
    {
        public int PPAID { get; set; }
        
        public string TableName { get; set; }
        
        public string ColumnName { get; set; }
        
        public string OldValue { get; set; }
        
        public string NewValue { get; set; }
        
        public int PersonID { get; set; }
        
        public int ApprovalStatusID { get; set; }
        
        public string Remarks { get; set; }
        public int ApprovalProcessID { get; set; } = 0;
    }
}
