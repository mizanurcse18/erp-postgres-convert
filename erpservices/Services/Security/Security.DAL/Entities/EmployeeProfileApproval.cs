using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Security.DAL.Entities
{
    [Table("EmployeeProfileApproval")]
    public class EmployeeProfileApproval : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int EPAID { get; set; }
        [Loggable]
        public string TableName { get; set; }
        [Loggable]
        public string ColumnName { get; set; }
        [Loggable]
        public string OldValue { get; set; }
        [Loggable]
        public string NewValue { get; set; }
        [Loggable]
        public int PersonID { get; set; }
        [Loggable]
        public int ApprovalStatusID { get; set; }
        [Loggable]
        public string Remarks { get; set; }

    }
}
