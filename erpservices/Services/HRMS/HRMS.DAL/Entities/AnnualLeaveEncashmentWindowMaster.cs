using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.DAL.Entities
{
    [Table("AnnualLeaveEncashmentWindowMaster")]
    public class AnnualLeaveEncashmentWindowMaster : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long ALEWMasterID { get; set; }
        [Required]
        [Loggable]
        public int FinancialYearID { get; set; }        
        [Required]
        [Loggable]
        public DateTime StartDate { get; set; }
        [Required]
        [Loggable]
        public DateTime EndDate { get; set; }
        [Required]
        [Loggable]
        public int Status { get; set; }
        [Required]
        [Loggable]
        public string DivisionIDs{ get; set; }        
        [Loggable]
        public string DepartmentIDs { get; set; }
        [Loggable]
        public string EmployeeTypeIDs { get; set; }
    }
}
