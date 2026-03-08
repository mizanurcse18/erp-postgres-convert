using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace Manager.Core.CommonDto
{
   public class EmployeeProfileApprovalDtoCore
    {
        public int EPAID { get; set; }
        
        public string TableName { get; set; }
        
        public string ColumnName { get; set; }
        
        public string OldValue { get; set; }
        
        public string NewValue { get; set; }
        
        public int PersonID { get; set; }
        
        public int ApprovalStatusID { get; set; }
        
        public string Remarks { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
    }
}
